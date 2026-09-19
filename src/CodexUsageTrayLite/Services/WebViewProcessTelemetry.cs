using System;
using System.Globalization;
using CodexUsageTrayLite.Infrastructure;
using Microsoft.Web.WebView2.Core;

namespace CodexUsageTrayLite.Services
{
    internal static class WebViewProcessTelemetry
    {
        private static readonly object Sync = new object();
        private static uint lastExitedBrowserProcessId;
        private static DateTime lastBrowserExitLogUtc;

        public static void TrackEnvironment(CoreWebView2Environment environment)
        {
            if (environment == null) return;
            try
            {
                environment.BrowserProcessExited += HandleBrowserProcessExited;
            }
            catch (Exception ex)
            {
                SafeLogger.Write("WebView.BrowserExitTrackingUnavailable", ex.GetType().Name);
            }
        }

        public static uint ControllerCreated(string scope, CoreWebView2 core)
        {
            var browserProcessId = GetBrowserProcessId(core);
            SafeLogger.Write(
                "WebView.ControllerCreated",
                "scope=" + NormalizeScope(scope) +
                " browserPid=" + FormatProcessId(browserProcessId) +
                " ownership=controller processKill=false");
            return browserProcessId;
        }

        public static void ControllerCloseReturned(string scope, uint browserProcessId)
        {
            SafeLogger.Write("WebView.ControllerClose", FormatControllerCloseDetails(scope, browserProcessId, "returned", null));
        }

        public static void ControllerCloseFailed(string scope, uint browserProcessId, Exception exception)
        {
            SafeLogger.Write(
                LogEventLevel.Error,
                "WebView.ControllerClose",
                FormatControllerCloseDetails(scope, browserProcessId, "exception", exception == null ? "Unknown" : exception.GetType().Name));
        }

        public static void ProcessFailed(string scope, uint browserProcessId, CoreWebView2ProcessFailedEventArgs args)
        {
            if (args == null) return;
            string kind;
            string reason;
            string exitCode;
            try { kind = args.ProcessFailedKind.ToString(); }
            catch (Exception) { kind = "unavailable"; }
            try { reason = args.Reason.ToString(); }
            catch (Exception) { reason = "unavailable"; }
            try { exitCode = args.ExitCode.ToString(CultureInfo.InvariantCulture); }
            catch (Exception) { exitCode = "unavailable"; }
            SafeLogger.Write(
                LogEventLevel.Error,
                "WebView.ProcessFailed",
                "scope=" + NormalizeScope(scope) +
                " browserPid=" + FormatProcessId(browserProcessId) +
                " kind=" + kind +
                " reason=" + reason +
                " exitCode=" + exitCode);
        }

        internal static string FormatControllerCloseDetails(string scope, uint browserProcessId, string result, string exceptionType)
        {
            var details = "scope=" + NormalizeScope(scope) +
                " browserPid=" + FormatProcessId(browserProcessId) +
                " api=CoreWebView2Controller.Close result=" + (string.IsNullOrWhiteSpace(result) ? "unknown" : result) +
                " processKill=false browserExit=asynchronous";
            if (!string.IsNullOrWhiteSpace(exceptionType)) details += " exception=" + exceptionType;
            return details;
        }

        private static void HandleBrowserProcessExited(object sender, CoreWebView2BrowserProcessExitedEventArgs args)
        {
            if (args == null) return;
            lock (Sync)
            {
                var now = DateTime.UtcNow;
                if (lastExitedBrowserProcessId == args.BrowserProcessId && now - lastBrowserExitLogUtc < TimeSpan.FromSeconds(5)) return;
                lastExitedBrowserProcessId = args.BrowserProcessId;
                lastBrowserExitLogUtc = now;
            }
            SafeLogger.Write(
                args.BrowserProcessExitKind == CoreWebView2BrowserProcessExitKind.Normal ? LogEventLevel.Info : LogEventLevel.Error,
                "WebView.BrowserProcessExited",
                "browserPid=" + FormatProcessId(args.BrowserProcessId) +
                " exitKind=" + args.BrowserProcessExitKind +
                " source=runtime-event");
        }

        private static uint GetBrowserProcessId(CoreWebView2 core)
        {
            try { return core == null ? 0 : core.BrowserProcessId; }
            catch (Exception) { return 0; }
        }

        private static string FormatProcessId(uint processId)
        {
            return processId == 0 ? "unavailable" : processId.ToString(CultureInfo.InvariantCulture);
        }

        private static string NormalizeScope(string scope)
        {
            return string.Equals(scope, "login", StringComparison.OrdinalIgnoreCase) ? "login" :
                string.Equals(scope, "probe", StringComparison.OrdinalIgnoreCase) ? "probe" : "fetch";
        }
    }
}
