using System;
using System.Drawing;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using CodexUsageTrayLite.Infrastructure;
using CodexUsageTrayLite.Models;
using Microsoft.Web.WebView2.Core;

namespace CodexUsageTrayLite.Services
{
    internal sealed class WebViewFetchSession : IUsageFetchSession
    {
        internal const int MissingFiveHourConfirmationAttempts = 3;
        private readonly ProxySettings proxy;
        private readonly CoreWebView2Environment testEnvironment;
        private readonly Action<CoreWebView2> initializeTestCore;
        private readonly string usageUrl;
        private Form hostForm;
        private CoreWebView2Controller controller;
        private CoreWebView2 core;
        private CoreWebView2Environment environment;
        private uint browserProcessId;
        private TaskCompletionSource<FetchOutcome> completion;
        private CancellationToken token;
        private bool disposed;
        private bool usageResetProbeFailed;
        private readonly Stopwatch elapsed = new Stopwatch();
        private string stage = "starting";
        private int pageAttempt;
        private bool navigationInFlight;
        private bool navigationFailed;
        private ulong navigationId;
        private string lastPageDiagnostic;
        private string sourceCategory = "unavailable";
        private UsagePageObservation lastObservation = new UsagePageObservation();

        public WebViewFetchSession(ProxySettings proxy)
            : this(proxy, null, AppConstants.UsageUrl, null)
        {
        }

        // Internal injection for offline integration probes. The application
        // factory always uses the default constructor and official Usage URL.
        internal WebViewFetchSession(ProxySettings proxy, CoreWebView2Environment testEnvironment,
            string usageUrl, Action<CoreWebView2> initializeTestCore)
        {
            this.proxy = proxy ?? new ProxySettings { mode = ProxyMode.System };
            this.testEnvironment = testEnvironment;
            this.usageUrl = usageUrl;
            this.initializeTestCore = initializeTestCore;
        }

        public async Task<FetchOutcome> RunAsync(CancellationToken cancellationToken)
        {
            token = cancellationToken;
            completion = new TaskCompletionSource<FetchOutcome>(TaskCreationOptions.RunContinuationsAsynchronously);
            elapsed.Start();
            using (token.Register(TrySetCancelled))
            {
                // Cancellation must also complete while environment/controller
                // creation is pending. The worker closes any late controller.
                _ = InitializeAndReadAsync();
                return await completion.Task;
            }
        }

        private async Task InitializeAndReadAsync()
        {
            try
            {
                SetStage("runtime");
                var version = CoreWebView2Environment.GetAvailableBrowserVersionString();
                if (string.IsNullOrWhiteSpace(version))
                {
                    TrySetOutcome(FetchOutcome.Create(FetchStatus.RuntimeMissing, "Microsoft Edge WebView2 Runtime is not installed."));
                    return;
                }
                if (testEnvironment == null) System.IO.Directory.CreateDirectory(AppPaths.WebViewProfileDirectory);
                CreateHiddenHost();
                SetStage("environment");
                environment = testEnvironment ?? await WebViewEnvironmentFactory.CreateAsync(proxy, token);
                token.ThrowIfCancellationRequested();
                SetStage("controller");
                var created = await environment.CreateCoreWebView2ControllerAsync(hostForm.Handle);
                if (disposed || token.IsCancellationRequested)
                {
                    created.Close();
                    SafeLogger.Write("WebView.LateControllerClosed", "processKill=false");
                    return;
                }
                controller = created;
                controller.Bounds = new Rectangle(Point.Empty, WebViewUsageResetDomProbe.BackgroundViewportSize);
                controller.IsVisible = false;
                core = controller.CoreWebView2;
                WebViewEnvironmentFactory.ApplySecureSettings(core);
                initializeTestCore?.Invoke(core);
                core.NavigationStarting += HandleNavigationStarting;
                core.NavigationCompleted += HandleNavigationCompleted;
                core.ProcessFailed += HandleProcessFailed;
                browserProcessId = WebViewProcessTelemetry.ControllerCreated("fetch", core);
                SafeLogger.Write("WebView.FetchCreated", "runtime=" + version + " browserPid=" + (browserProcessId == 0 ? "unavailable" : browserProcessId.ToString()));
                await ReadWithRecoveryAsync();
            }
            catch (WebView2RuntimeNotFoundException)
            {
                TrySetOutcome(FetchOutcome.Create(FetchStatus.RuntimeMissing, "Microsoft Edge WebView2 Runtime is not installed."));
            }
            catch (OperationCanceledException) { TrySetCancelled(); }
            catch (Exception) when (token.IsCancellationRequested || disposed) { TrySetCancelled(); }
            catch (Exception ex)
            {
                WriteDiagnostic("exception-" + ex.GetType().Name);
                TrySetOutcome(FetchOutcome.Create(FetchStatus.UnexpectedError, "The WebView2 usage read failed."));
            }
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            try
            {
                if (core != null)
                {
                    core.NavigationStarting -= HandleNavigationStarting;
                    core.NavigationCompleted -= HandleNavigationCompleted;
                    core.ProcessFailed -= HandleProcessFailed;
                    core.Stop();
                }
            }
            catch (Exception ex)
            {
                SafeLogger.Write("WebView.FetchDetachFailed", ex.GetType().Name);
            }
            if (controller != null)
            {
                try
                {
                    controller.Close();
                    WebViewProcessTelemetry.ControllerCloseReturned("fetch", browserProcessId);
                }
                catch (Exception ex)
                {
                    WebViewProcessTelemetry.ControllerCloseFailed("fetch", browserProcessId, ex);
                }
                finally
                {
                    controller = null;
                }
            }
            core = null;
            if (hostForm != null)
            {
                hostForm.Dispose();
                hostForm = null;
            }
            environment = null;
            completion = null;
            SafeLogger.Write("WebView.FetchDisposed");
        }

        private void CreateHiddenHost()
        {
            var backgroundHost = new BackgroundWebViewHostForm(WebViewUsageResetDomProbe.BackgroundViewportSize);
            hostForm = backgroundHost;
            var unused = backgroundHost.CreateHostHandle();
            SafeLogger.Write(
                "WebView.BackgroundHostCreated",
                "visible=" + backgroundHost.Visible.ToString().ToLowerInvariant() +
                " activation=disabled viewport=" + backgroundHost.ClientSize.Width + "x" + backgroundHost.ClientSize.Height);
        }

        private void HandleNavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs args)
        {
            navigationId = args.NavigationId;
            navigationInFlight = true;
            navigationFailed = false;
            Uri destination;
            if (Uri.TryCreate(args.Uri, UriKind.Absolute, out destination)) UpdateSourceCategory(destination);
            if (!Uri.TryCreate(args.Uri, UriKind.Absolute, out destination) || !IsAllowedBackgroundDestination(destination))
            {
                args.Cancel = true;
                TrySetOutcome(FetchOutcome.Create(FetchStatus.LoginRequired, "Interactive sign-in is required."));
            }
        }

        private void HandleNavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs args)
        {
            if (disposed || args.NavigationId != navigationId) return;
            navigationInFlight = false;
            navigationFailed = !args.IsSuccess;
            UpdateSourceCategory(CurrentSource());
            SafeLogger.Write(
                args.IsSuccess ? LogEventLevel.Info : LogEventLevel.Warning,
                "WebView.NavigationCompleted",
                "attempt=" + pageAttempt + " success=" + args.IsSuccess.ToString().ToLowerInvariant() +
                " httpStatus=" + args.HttpStatusCode + " webError=" + args.WebErrorStatus +
                " source=" + SourceCategory());
        }

        private async Task ReadWithRecoveryAsync()
        {
            var lastError = "The expected usage fields were not found.";
            var diagnostics = new UsageFieldDiagnostics();
            for (pageAttempt = 1; pageAttempt <= 2; pageAttempt++)
            {
                token.ThrowIfCancellationRequested();
                if (completion == null || completion.Task.IsCompleted) return;
                lastPageDiagnostic = null;
                navigationFailed = false;
                var previousDocument = pageAttempt > 1 ? lastObservation.DocumentKey : null;
                SetStage("navigation");
                if (pageAttempt > 1) core.Stop();
                core.Navigate(usageUrl);
                var pageElapsed = Stopwatch.StartNew();
                var readiness = new UsagePageReadiness();
                var lastClick = TimeSpan.FromDays(-1);
                var clickCount = 0;
                var operationFailed = false;
                while (pageElapsed.Elapsed < TimeSpan.FromSeconds(18))
                {
                    token.ThrowIfCancellationRequested();
                    if (completion == null || completion.Task.IsCompleted) return;
                    if (navigationFailed) { operationFailed = true; break; }
                    if (navigationInFlight)
                    {
                        SetStage("navigation");
                    }
                    try
                    {
                        var allowClick = !readiness.HasCandidate && clickCount < 3 && pageElapsed.Elapsed - lastClick >= TimeSpan.FromSeconds(2);
                        var json = await AwaitScriptAsync(core.ExecuteScriptAsync(WebViewUsagePageNavigator.BuildScript(allowClick, previousDocument)));
                        lastObservation = WebViewUsagePageNavigator.ParseObservation(json);
                        TraceObservation();
                    }
                    catch (TimeoutException)
                    {
                        WriteDiagnostic("script-timeout");
                        operationFailed = true;
                        break;
                    }
                    catch (System.Runtime.InteropServices.COMException) when (!disposed && !token.IsCancellationRequested)
                    {
                        WriteDiagnostic("script-com-error");
                        operationFailed = true;
                        break;
                    }
                    catch (InvalidOperationException) when (!disposed && !token.IsCancellationRequested)
                    {
                        // A script can race a real navigation; let the next poll
                        // observe the new document before deciding to recover.
                        readiness.Reset();
                        SetStage("document-transition");
                        await Task.Delay(400, token);
                        continue;
                    }
                    var text = lastObservation.Text;
                    if (UsageParser.LooksLikeLoginRequired(CurrentSource(), text))
                    {
                        TrySetOutcome(FetchOutcome.Create(FetchStatus.LoginRequired, "ChatGPT login is required."));
                        return;
                    }
                    if (!WebViewUsagePageNavigator.IsChatGptSource(CurrentSource()) || !lastObservation.TrustedOrigin ||
                        (previousDocument != null && lastObservation.DocumentKey == previousDocument))
                    {
                        readiness.Reset();
                        SetStage("wait-new-usage-document");
                    }
                    else if (lastObservation.DocumentState == "loading")
                    {
                        readiness.Reset();
                        SetStage("wait-document");
                    }
                    else if (lastObservation.State == UsageOverviewState.Selected)
                    {
                        clickCount++;
                        lastClick = pageElapsed.Elapsed;
                        readiness.Reset();
                        SetStage("wait-overview");
                        SafeLogger.Write("WebView.UsageOverviewSelected", "context=fetch attempt=" + pageAttempt + " click=" + clickCount + " confirmation=pending");
                    }
                    else
                    {
                        UsageSnapshot snapshot;
                        if (!lastObservation.AnalyticsSelected &&
                            UsageParser.TryParse(text, DateTime.Now, out snapshot, out lastError, out diagnostics))
                        {
                            SetStage("confirm-quota");
                            if (readiness.Observe(snapshot, pageElapsed.Elapsed))
                            {
                                SafeLogger.Write("WebView.UsagePageReady",
                                    "attempt=" + pageAttempt + " evidence=stable-rendered-quota source=" + SourceCategory());
                                await CompleteSnapshotAsync(snapshot, text);
                                return;
                            }
                        }
                        else
                        {
                            readiness.Reset();
                            SetStage(lastObservation.State == UsageOverviewState.Ready ? "wait-quota" : "wait-overview");
                            // Keep diagnostics populated even while Analytics is active.
                            UsageSnapshot unused;
                            UsageParser.TryParse(text, DateTime.Now, out unused, out lastError, out diagnostics);
                        }
                    }
                    await Task.Delay(400, token);
                }
                if (UsagePageReadiness.ShouldRecover(pageAttempt, pageElapsed.Elapsed, operationFailed))
                {
                    WriteDiagnostic(operationFailed ? "navigation-or-script-failed-retry" : "page-not-ready-retry");
                    SafeLogger.Write(LogEventLevel.Warning, "WebView.FetchRecovery",
                        "attempt=1 nextAttempt=2 action=reload-usage remainingMs=" +
                        Math.Max(0, AppConstants.FetchTimeoutSeconds * 1000 - (long)elapsed.Elapsed.TotalMilliseconds));
                    continue;
                }
                UsageSnapshot failedSnapshot;
                string failedParseError;
                UsageParser.TryParse(lastObservation.Text, DateTime.Now, out failedSnapshot, out failedParseError, out diagnostics);
                UsageFieldTelemetry.WriteAll(UsageSource.WebView2, diagnostics);
                WriteDiagnostic(operationFailed ? "operation-failed" : "page-not-ready");
                TrySetOutcome(FetchOutcome.Create(navigationFailed ? FetchStatus.NetworkError : FetchStatus.ParseError, lastError));
                return;
            }
        }

        private async Task CompleteSnapshotAsync(UsageSnapshot snapshot, string text)
        {
            if (!snapshot.availableUsageResetCount.HasValue)
            {
                SetStage("read-resets");
                snapshot.availableUsageResetCount = await ReadAvailableUsageResetCountAsync();
                if (!snapshot.availableUsageResetCount.HasValue)
                {
                    var settledText = await ReadPageTextAsync();
                    snapshot.availableUsageResetCount = ResolveMissingUsageResetCountForWeeklyOnly(
                        snapshot, settledText, !usageResetProbeFailed);
                    if (snapshot.availableUsageResetCount.HasValue)
                        SafeLogger.Write("WebView.UsageResetResolved", "reason=weekly-only-section-absent");
                    else
                        UsageFieldTelemetry.Write(UsageSource.WebView2, UsageDataField.Resets,
                            ClassifyMissingUsageReset(settledText, usageResetProbeFailed));
                }
            }
            SetStage("read-account");
            var accountEmail = await ReadAccountEmailAsync();
            snapshot.userLevel = accountEmail.UserLevel != UserLevel.Unknown
                ? accountEmail.UserLevel : UserLevelHelper.ReadFromVisiblePageText(text);
            SetStage("complete");
            var outcome = FetchOutcome.Create(FetchStatus.Success, "Usage updated.", snapshot);
            TrySetOutcome(accountEmail.Read ? outcome.WithAccountEmail(accountEmail.Email) : outcome);
        }

        private void SetStage(string value)
        {
            if (disposed || stage == value) return;
            stage = value;
            SafeLogger.Write("WebView.FetchStage", "attempt=" + pageAttempt + " stage=" + stage +
                " elapsedMs=" + (long)elapsed.Elapsed.TotalMilliseconds);
        }

        private void TraceObservation()
        {
            UpdateSourceCategory(CurrentSource());
            var signature = lastObservation.DocumentState + ":" + lastObservation.State + ":" +
                lastObservation.Reason + ":" + lastObservation.Candidates + ":" + lastObservation.AnalyticsSelected;
            if (signature == lastPageDiagnostic) return;
            lastPageDiagnostic = signature;
            SafeLogger.Write("WebView.UsagePageState", "attempt=" + pageAttempt + " source=" +
                SourceCategory() + " " + lastObservation.Diagnostic);
        }

        private void WriteDiagnostic(string reason)
        {
            if (disposed) return;
            SafeLogger.Write(LogEventLevel.Warning, "WebView.FetchDiagnostic",
                "attempt=" + pageAttempt + " stage=" + stage + " reason=" + reason +
                " elapsedMs=" + (long)elapsed.Elapsed.TotalMilliseconds + " source=" +
                SourceCategory() + " navigationPending=" + navigationInFlight.ToString().ToLowerInvariant() +
                " " + lastObservation.Diagnostic);
        }

        private string SourceCategory()
        {
            // Cancellation callbacks can run off the UI thread. Never access
            // CoreWebView2 from diagnostics; use only this captured category.
            return sourceCategory;
        }

        private void UpdateSourceCategory(Uri source)
        {
            if (source == null) { sourceCategory = "unavailable"; return; }
            if (source.AbsolutePath.IndexOf("/auth", StringComparison.OrdinalIgnoreCase) >= 0 ||
                source.AbsolutePath.IndexOf("/login", StringComparison.OrdinalIgnoreCase) >= 0) sourceCategory = "auth";
            else if (!WebViewUsagePageNavigator.IsChatGptSource(source)) sourceCategory = "other-origin";
            else if (source.AbsolutePath.IndexOf("/settings/analytics", StringComparison.OrdinalIgnoreCase) >= 0) sourceCategory = "legacy-analytics";
            else if (source.AbsolutePath.IndexOf("/settings/usage", StringComparison.OrdinalIgnoreCase) >= 0 ||
                source.Fragment.StartsWith("#settings/usage", StringComparison.OrdinalIgnoreCase)) sourceCategory = "usage";
            else sourceCategory = "chatgpt-other";
        }

        private async Task<string> AwaitScriptAsync(Task<string> operation)
        {
            var timeout = Task.Delay(3500, token);
            var winner = await Task.WhenAny(operation, timeout);
            token.ThrowIfCancellationRequested();
            if (winner == operation) return await operation;
            // Observe a late fault without keeping the failed document alive.
            _ = operation.ContinueWith(task => { var ignored = task.Exception; },
                TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);
            throw new TimeoutException();
        }

        private void HandleProcessFailed(object sender, CoreWebView2ProcessFailedEventArgs args)
        {
            WebViewProcessTelemetry.ProcessFailed("fetch", browserProcessId, args);
            TrySetOutcome(FetchOutcome.Create(FetchStatus.UnexpectedError, "The WebView2 process failed."));
        }

        private void TrySetOutcome(FetchOutcome outcome)
        {
            var pending = completion;
            if (pending != null) pending.TrySetResult(outcome);
        }

        private void TrySetCancelled()
        {
            var pending = completion;
            if (pending != null && !pending.Task.IsCompleted)
            {
                WriteDiagnostic("cancelled-or-timeout");
                pending.TrySetCanceled(token);
            }
        }

        private async Task<string> ReadPageTextAsync()
        {
            var json = await AwaitScriptAsync(core.ExecuteScriptAsync("document.body ? document.body.innerText : ''"));
            return new JavaScriptSerializer().Deserialize<string>(json) ?? string.Empty;
        }

        private async Task<int?> ReadAvailableUsageResetCountAsync()
        {
            usageResetProbeFailed = false;
            try
            {
                for (var attempt = 0; attempt < WebViewUsageResetDomProbe.MaximumAttempts; attempt++)
                {
                    token.ThrowIfCancellationRequested();
                    var json = await AwaitScriptAsync(core.ExecuteScriptAsync(WebViewUsageResetDomProbe.Script));
                    var count = WebViewUsageResetDomProbe.ParseCount(json);
                    if (count.HasValue) return count;
                    if (attempt + 1 < WebViewUsageResetDomProbe.MaximumAttempts)
                    {
                        await Task.Delay(WebViewUsageResetDomProbe.RetryDelayMilliseconds, token);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                usageResetProbeFailed = true;
                SafeLogger.Write("WebView.UsageResetReadFailed", ex.GetType().Name);
            }
            return null;
        }

        internal static int? ResolveMissingUsageResetCountForWeeklyOnly(
            UsageSnapshot snapshot,
            string settledPageText,
            bool probeCompletedWithoutError)
        {
            if (!probeCompletedWithoutError || snapshot == null || snapshot.IsFiveHourLimitApplicable)
            {
                return null;
            }
            return UsageParser.ContainsRecognizedUsageResetSection(settledPageText)
                ? (int?)null
                : 0;
        }

        internal static UsageFieldUnavailableReason ClassifyMissingUsageReset(
            string settledPageText,
            bool probeFailed)
        {
            if (probeFailed) return UsageFieldUnavailableReason.DomProbeFailed;
            return UsageParser.ContainsRecognizedUsageResetSection(settledPageText)
                ? UsageFieldUnavailableReason.ResetSectionUnrecognized
                : UsageFieldUnavailableReason.ResetSectionMissing;
        }

        private async Task<AccountEmailReadResult> ReadAccountEmailAsync()
        {
            var source = CurrentSource();
            if (source == null || !string.Equals(source.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(source.Host, "chatgpt.com", StringComparison.OrdinalIgnoreCase))
            {
                return AccountEmailReadResult.Unavailable;
            }

            var messagePrefix = "CodexUsageTrayLite.Email." + Guid.NewGuid().ToString("N") + ":";
            var message = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            EventHandler<CoreWebView2WebMessageReceivedEventArgs> handler = (sender, args) =>
            {
                try
                {
                    Uri messageSource;
                    if (!Uri.TryCreate(args.Source, UriKind.Absolute, out messageSource) ||
                        !string.Equals(messageSource.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) ||
                        !string.Equals(messageSource.Host, "chatgpt.com", StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }
                    var value = args.TryGetWebMessageAsString();
                    if (!string.IsNullOrEmpty(value) && value.StartsWith(messagePrefix, StringComparison.Ordinal))
                    {
                        message.TrySetResult(value);
                    }
                }
                catch (ArgumentException)
                {
                }
                catch (InvalidOperationException)
                {
                }
            };

            core.WebMessageReceived += handler;
            try
            {
                var remaining = AppConstants.FetchTimeoutSeconds * 1000 - elapsed.Elapsed.TotalMilliseconds - 500;
                if (remaining < 1000) return AccountEmailReadResult.Unavailable;
                await AwaitScriptAsync(core.ExecuteScriptAsync(AccountEmailHelper.BuildWebViewScript(messagePrefix)));
                var timeout = Task.Delay(TimeSpan.FromMilliseconds(Math.Min(7000, remaining)), token);
                var winner = await Task.WhenAny(message.Task, timeout);
                token.ThrowIfCancellationRequested();
                if (winner != message.Task) return AccountEmailReadResult.Unavailable;

                string email;
                UserLevel userLevel;
                return AccountEmailHelper.TryReadWebViewMessage(message.Task.Result, messagePrefix, out email, out userLevel)
                    ? new AccountEmailReadResult(true, email, userLevel)
                    : AccountEmailReadResult.Unavailable;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                SafeLogger.Write("WebView.EmailReadFailed", ex.GetType().Name);
                return AccountEmailReadResult.Unavailable;
            }
            finally
            {
                if (core != null) core.WebMessageReceived -= handler;
            }
        }

        private Uri CurrentSource()
        {
            Uri value;
            return core != null && Uri.TryCreate(core.Source, UriKind.Absolute, out value) ? value : null;
        }

        private static bool IsAllowedBackgroundDestination(Uri uri)
        {
            if (uri == null || !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            return IsHostOrSubdomain(uri.Host, "chatgpt.com") || IsHostOrSubdomain(uri.Host, "openai.com");
        }

        private static bool IsHostOrSubdomain(string host, string allowed)
        {
            return string.Equals(host, allowed, StringComparison.OrdinalIgnoreCase) ||
                host.EndsWith("." + allowed, StringComparison.OrdinalIgnoreCase);
        }

        private sealed class AccountEmailReadResult
        {
            public static readonly AccountEmailReadResult Unavailable = new AccountEmailReadResult(false, null, UserLevel.Unknown);

            public AccountEmailReadResult(bool read, string email, UserLevel userLevel)
            {
                Read = read;
                Email = email;
                UserLevel = userLevel;
            }

            public bool Read { get; private set; }
            public string Email { get; private set; }
            public UserLevel UserLevel { get; private set; }
        }
    }
}
