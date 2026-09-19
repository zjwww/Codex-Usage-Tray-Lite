using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using CodexUsageTrayLite.Infrastructure;
using CodexUsageTrayLite.Models;
using CodexUsageTrayLite.Services;
using Microsoft.Web.WebView2.Core;

namespace CodexUsageTrayLite.UI
{
    internal sealed class LoginBrowserForm : ThemedForm
    {
        private readonly ProxySettings proxy;
        private readonly ToolStripStatusLabel status;
        private UiText ui;
        private LoginStatusState statusState;
        private CoreWebView2Environment environment;
        private CoreWebView2Controller controller;
        private CoreWebView2 core;
        private uint browserProcessId;
        private CancellationTokenSource initializationCancellation;
        private bool disposed;

        public bool ReachedUsagePage { get; private set; }

        public LoginBrowserForm(ProxySettings proxy, AppThemeMode themeMode = AppThemeMode.System, AppLanguage language = AppLanguage.English)
            : base(themeMode)
        {
            ui = UiText.For(language);
            this.proxy = proxy ?? new ProxySettings { mode = ProxyMode.System };
            Text = ui.LoginDialogTitle;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1120, 800);
            ShowInTaskbar = true;

            var strip = new StatusStrip();
            statusState = LoginStatusState.Opening;
            status = new ToolStripStatusLabel(ui.LoginOpening);
            strip.Items.Add(status);
            Controls.Add(strip);
            Resize += HandleResize;
            Shown += HandleShown;
            FormClosed += (sender, args) => SafeLogger.Write("WebView.LoginClosed", ReachedUsagePage ? "usage page reached" : "usage page not reached");
            ApplyAppTheme(themeMode);
        }

        internal void ApplyLanguage(AppLanguage language)
        {
            ui = UiText.For(language);
            Text = ui.LoginDialogTitle;
            status.Text = StatusText(statusState);
        }

        protected override void OnAppThemeApplied(ThemePalette palette)
        {
            base.OnAppThemeApplied(palette);
            if (controller != null)
            {
                try { controller.DefaultBackgroundColor = palette.WindowBackground; }
                catch (Exception ex) { SafeLogger.Write("Theme.WebViewBackgroundFailed", ex.GetType().Name); }
            }
            ThemeService.ApplyToWebView(core, ThemeMode);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !disposed)
            {
                disposed = true;
                if (initializationCancellation != null)
                {
                    initializationCancellation.Cancel();
                    initializationCancellation.Dispose();
                    initializationCancellation = null;
                }
                try
                {
                    if (core != null)
                    {
                        core.NavigationCompleted -= HandleNavigationCompleted;
                        core.ProcessFailed -= HandleProcessFailed;
                        core.Stop();
                    }
                }
                catch
                {
                }
                if (controller != null)
                {
                    try
                    {
                        controller.Close();
                        WebViewProcessTelemetry.ControllerCloseReturned("login", browserProcessId);
                    }
                    catch (Exception ex) { WebViewProcessTelemetry.ControllerCloseFailed("login", browserProcessId, ex); }
                    finally { controller = null; }
                }
                core = null;
                environment = null;
                SafeLogger.Write("WebView.LoginDisposed");
            }
            base.Dispose(disposing);
        }

        private async void HandleShown(object sender, EventArgs args)
        {
            Shown -= HandleShown;
            initializationCancellation = new CancellationTokenSource(TimeSpan.FromSeconds(AppConstants.FetchTimeoutSeconds));
            try
            {
                var version = CoreWebView2Environment.GetAvailableBrowserVersionString();
                if (string.IsNullOrWhiteSpace(version)) throw new WebView2RuntimeNotFoundException("Runtime missing.");
                System.IO.Directory.CreateDirectory(AppPaths.WebViewProfileDirectory);
                environment = await WebViewEnvironmentFactory.CreateAsync(proxy, initializationCancellation.Token);
                controller = await environment.CreateCoreWebView2ControllerAsync(Handle);
                controller.Bounds = BrowserBounds();
                controller.DefaultBackgroundColor = ThemeService.GetPalette(ThemeMode).WindowBackground;
                controller.IsVisible = true;
                core = controller.CoreWebView2;
                WebViewEnvironmentFactory.ApplySecureSettings(core);
                ThemeService.ApplyToWebView(core, ThemeMode);
                core.NavigationCompleted += HandleNavigationCompleted;
                core.ProcessFailed += HandleProcessFailed;
                browserProcessId = WebViewProcessTelemetry.ControllerCreated("login", core);
                SafeLogger.Write("WebView.LoginCreated", "proxy=" + proxy.mode + " runtime=" + version + " browserPid=" + (browserProcessId == 0 ? "unavailable" : browserProcessId.ToString()));
                core.Navigate(AppConstants.UsageUrl);
            }
            catch (WebView2RuntimeNotFoundException)
            {
                SetStatus(LoginStatusState.RuntimeRequired);
                LocalizedMessageBox.Show(this, ui.WebViewRuntimeRequired, AppConstants.Name,
                    MessageBoxButtons.OK, MessageBoxIcon.Error, ThemeMode, ui.Language);
            }
            catch (OperationCanceledException)
            {
                SetStatus(LoginStatusState.Timeout);
                LocalizedMessageBox.Show(this, ui.WebViewTimeout, AppConstants.Name,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning, ThemeMode, ui.Language);
            }
            catch (Exception ex)
            {
                SafeLogger.Write("WebView.LoginCreateFailed", ex.GetType().Name + ": " + ex.Message);
                SetStatus(LoginStatusState.InitializeFailed);
                LocalizedMessageBox.Show(this, ui.WebViewInitializeFailed(SafeLogger.Sanitize(ex.Message)), AppConstants.Name,
                    MessageBoxButtons.OK, MessageBoxIcon.Error, ThemeMode, ui.Language);
            }
        }

        private void HandleNavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs args)
        {
            if (!args.IsSuccess)
            {
                SetStatus(LoginStatusState.PageLoadFailed);
                return;
            }
            Uri source;
            Uri.TryCreate(core == null ? null : core.Source, UriKind.Absolute, out source);
            ReachedUsagePage = source != null && source.AbsolutePath.IndexOf("/codex/cloud/settings/analytics", StringComparison.OrdinalIgnoreCase) >= 0;
            SetStatus(ReachedUsagePage ? LoginStatusState.UsageReached : LoginStatusState.CompleteSignIn);
        }

        private void HandleProcessFailed(object sender, CoreWebView2ProcessFailedEventArgs args)
        {
            WebViewProcessTelemetry.ProcessFailed("login", browserProcessId, args);
        }

        private void HandleResize(object sender, EventArgs args)
        {
            if (controller != null) controller.Bounds = BrowserBounds();
        }

        private Rectangle BrowserBounds()
        {
            var height = Math.Max(1, ClientSize.Height - (status == null || status.Owner == null ? 0 : status.Owner.Height));
            return new Rectangle(0, 0, Math.Max(1, ClientSize.Width), height);
        }

        private void SetStatus(LoginStatusState state)
        {
            statusState = state;
            status.Text = StatusText(state);
        }

        private string StatusText(LoginStatusState state)
        {
            switch (state)
            {
                case LoginStatusState.RuntimeRequired: return ui.LoginRuntimeStatus;
                case LoginStatusState.Timeout: return ui.LoginTimeoutStatus;
                case LoginStatusState.InitializeFailed: return ui.LoginInitializeFailedStatus;
                case LoginStatusState.PageLoadFailed: return ui.LoginPageLoadFailed;
                case LoginStatusState.UsageReached: return ui.LoginUsageReached;
                case LoginStatusState.CompleteSignIn: return ui.LoginCompleteSignIn;
                default: return ui.LoginOpening;
            }
        }

        private enum LoginStatusState
        {
            Opening,
            RuntimeRequired,
            Timeout,
            InitializeFailed,
            PageLoadFailed,
            UsageReached,
            CompleteSignIn
        }
    }
}
