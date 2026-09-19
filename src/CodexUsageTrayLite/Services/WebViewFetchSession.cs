using System;
using System.Drawing;
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
        private Form hostForm;
        private CoreWebView2Controller controller;
        private CoreWebView2 core;
        private CoreWebView2Environment environment;
        private uint browserProcessId;
        private TaskCompletionSource<FetchOutcome> completion;
        private CancellationToken token;
        private bool reading;
        private bool disposed;
        private bool usageResetProbeFailed;

        public WebViewFetchSession(ProxySettings proxy)
        {
            this.proxy = proxy ?? new ProxySettings { mode = ProxyMode.System };
        }

        public async Task<FetchOutcome> RunAsync(CancellationToken cancellationToken)
        {
            token = cancellationToken;
            completion = new TaskCompletionSource<FetchOutcome>(TaskCreationOptions.RunContinuationsAsynchronously);
            try
            {
                var version = CoreWebView2Environment.GetAvailableBrowserVersionString();
                if (string.IsNullOrWhiteSpace(version))
                {
                    return FetchOutcome.Create(FetchStatus.RuntimeMissing, "Microsoft Edge WebView2 Runtime is not installed.");
                }
                System.IO.Directory.CreateDirectory(AppPaths.WebViewProfileDirectory);
                CreateHiddenHost();
                environment = await WebViewEnvironmentFactory.CreateAsync(proxy, token);
                controller = await environment.CreateCoreWebView2ControllerAsync(hostForm.Handle);
                controller.Bounds = new Rectangle(Point.Empty, WebViewUsageResetDomProbe.BackgroundViewportSize);
                controller.IsVisible = false;
                core = controller.CoreWebView2;
                WebViewEnvironmentFactory.ApplySecureSettings(core);
                core.NavigationStarting += HandleNavigationStarting;
                core.NavigationCompleted += HandleNavigationCompleted;
                core.ProcessFailed += HandleProcessFailed;
                browserProcessId = WebViewProcessTelemetry.ControllerCreated("fetch", core);
                SafeLogger.Write("WebView.FetchCreated", "runtime=" + version + " browserPid=" + (browserProcessId == 0 ? "unavailable" : browserProcessId.ToString()));
                using (token.Register(TrySetCancelled))
                {
                    core.Navigate(AppConstants.UsageUrl);
                    return await completion.Task;
                }
            }
            catch (WebView2RuntimeNotFoundException)
            {
                return FetchOutcome.Create(FetchStatus.RuntimeMissing, "Microsoft Edge WebView2 Runtime is not installed.");
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
            Uri destination;
            if (!Uri.TryCreate(args.Uri, UriKind.Absolute, out destination) || !IsAllowedBackgroundDestination(destination))
            {
                args.Cancel = true;
                TrySetOutcome(FetchOutcome.Create(FetchStatus.LoginRequired, "Interactive sign-in is required."));
            }
        }

        private async void HandleNavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs args)
        {
            var pending = completion;
            if (pending == null || pending.Task.IsCompleted || reading)
            {
                return;
            }
            if (!args.IsSuccess)
            {
                TrySetOutcome(FetchOutcome.Create(FetchStatus.NetworkError, "The ChatGPT usage page could not be loaded."));
                return;
            }
                reading = true;
            try
            {
                var lastError = "The expected usage fields were not found.";
                var lastDiagnostics = new UsageFieldDiagnostics();
                for (var attempt = 0; attempt < 30; attempt++)
                {
                    token.ThrowIfCancellationRequested();
                    var text = await ReadPageTextAsync();
                    if (UsageParser.LooksLikeLoginRequired(CurrentSource(), text))
                    {
                        TrySetOutcome(FetchOutcome.Create(FetchStatus.LoginRequired, "ChatGPT login is required."));
                        return;
                    }
                    UsageSnapshot snapshot;
                    UsageFieldDiagnostics diagnostics;
                    if (UsageParser.TryParse(text, DateTime.Now, out snapshot, out lastError, out diagnostics))
                    {
                        if (snapshot.fiveHourLimitAbsenceInferred && attempt < MissingFiveHourConfirmationAttempts)
                        {
                            await Task.Delay(750, token);
                            continue;
                        }
                        if (!snapshot.availableUsageResetCount.HasValue)
                        {
                            snapshot.availableUsageResetCount = await ReadAvailableUsageResetCountAsync();
                            if (!snapshot.availableUsageResetCount.HasValue)
                            {
                                var settledText = await ReadPageTextAsync();
                                snapshot.availableUsageResetCount = ResolveMissingUsageResetCountForWeeklyOnly(
                                    snapshot,
                                    settledText,
                                    !usageResetProbeFailed);
                                if (snapshot.availableUsageResetCount.HasValue)
                                {
                                    SafeLogger.Write(
                                        "WebView.UsageResetResolved",
                                        "reason=weekly-only-section-absent");
                                }
                                else
                                {
                                    var resetReason = ClassifyMissingUsageReset(settledText, usageResetProbeFailed);
                                    UsageFieldTelemetry.Write(UsageSource.WebView2, UsageDataField.Resets, resetReason);
                                }
                            }
                        }
                        var outcome = FetchOutcome.Create(FetchStatus.Success, "Usage updated.", snapshot);
                        var accountEmail = await ReadAccountEmailAsync();
                        snapshot.userLevel = accountEmail.UserLevel != UserLevel.Unknown
                            ? accountEmail.UserLevel
                            : UserLevelHelper.ReadFromVisiblePageText(text);
                        TrySetOutcome(accountEmail.Read ? outcome.WithAccountEmail(accountEmail.Email) : outcome);
                        return;
                    }
                    lastDiagnostics = diagnostics;
                    await Task.Delay(750, token);
                }
                UsageFieldTelemetry.WriteAll(UsageSource.WebView2, lastDiagnostics);
                TrySetOutcome(FetchOutcome.Create(FetchStatus.ParseError, lastError));
            }
            catch (OperationCanceledException)
            {
                TrySetCancelled();
            }
            catch (Exception) when (token.IsCancellationRequested)
            {
                TrySetCancelled();
            }
            catch (Exception ex)
            {
                SafeLogger.Write("Parser.Failed", ex.GetType().Name + ": " + ex.Message);
                TrySetOutcome(FetchOutcome.Create(FetchStatus.ParseError, "The usage page could not be parsed."));
            }
            finally
            {
                reading = false;
            }
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
            if (pending != null) pending.TrySetCanceled(token);
        }

        private async Task<string> ReadPageTextAsync()
        {
            var json = await core.ExecuteScriptAsync("document.body ? document.body.innerText : ''");
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
                    var json = await core.ExecuteScriptAsync(WebViewUsageResetDomProbe.Script);
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
                await core.ExecuteScriptAsync(AccountEmailHelper.BuildWebViewScript(messagePrefix));
                var timeout = Task.Delay(TimeSpan.FromSeconds(7), token);
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
