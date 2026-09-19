using System;
using System.Threading;
using System.Threading.Tasks;
using CodexUsageTrayLite.Infrastructure;
using CodexUsageTrayLite.Models;
using Microsoft.Web.WebView2.Core;

namespace CodexUsageTrayLite.Services
{
    internal static class WebViewEnvironmentFactory
    {
        private const string AdditionalArgumentsVariable = "WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS";
        private static readonly SemaphoreSlim EnvironmentGate = new SemaphoreSlim(1, 1);
        private static CoreWebView2Environment cachedEnvironment;
        private static string cachedSignature;

        public static async Task<CoreWebView2Environment> CreateAsync(ProxySettings proxy, CancellationToken token)
        {
            await EnvironmentGate.WaitAsync(token);
            var original = Environment.GetEnvironmentVariable(AdditionalArgumentsVariable, EnvironmentVariableTarget.Process);
            try
            {
                var signature = BuildSignature(proxy, original);
                if (cachedEnvironment != null && string.Equals(cachedSignature, signature, StringComparison.Ordinal))
                {
                    return cachedEnvironment;
                }
                bool proxyDetected;
                var sanitized = ProxyArgumentBuilder.SanitizeExternalArguments(original, out proxyDetected);
                if (!string.IsNullOrWhiteSpace(original))
                {
                    SafeLogger.Write("WebView.ExternalArgumentsDetected", proxyDetected ? "external proxy switches removed" : "no proxy switches found");
                }
                Environment.SetEnvironmentVariable(
                    AdditionalArgumentsVariable,
                    string.IsNullOrWhiteSpace(sanitized) ? null : sanitized,
                    EnvironmentVariableTarget.Process);

                var proxyArgument = ProxyArgumentBuilder.Build(proxy);
                var optionsArguments = "--disk-cache-size=16777216 --media-cache-size=8388608";
                if (!string.IsNullOrWhiteSpace(proxyArgument))
                {
                    optionsArguments += " " + proxyArgument;
                }
                var options = new CoreWebView2EnvironmentOptions(optionsArguments);
                SafeLogger.Write("WebView.EnvironmentCreate", "proxy=" + (proxy == null ? ProxyMode.System : proxy.mode));
                cachedEnvironment = await CoreWebView2Environment.CreateAsync(null, AppPaths.WebViewProfileDirectory, options);
                WebViewProcessTelemetry.TrackEnvironment(cachedEnvironment);
                cachedSignature = signature;
                return cachedEnvironment;
            }
            finally
            {
                Environment.SetEnvironmentVariable(AdditionalArgumentsVariable, original, EnvironmentVariableTarget.Process);
                EnvironmentGate.Release();
            }
        }

        public static void ClearCache()
        {
            cachedEnvironment = null;
            cachedSignature = null;
        }

        public static void ApplySecureSettings(CoreWebView2 core)
        {
            core.Settings.AreDevToolsEnabled = false;
            core.Settings.AreDefaultContextMenusEnabled = false;
            core.Settings.IsStatusBarEnabled = false;
            core.Settings.IsPasswordAutosaveEnabled = false;
            core.Settings.IsGeneralAutofillEnabled = false;
        }

        private static string BuildSignature(ProxySettings proxy, string externalArguments)
        {
            bool detected;
            var clean = ProxyArgumentBuilder.SanitizeExternalArguments(externalArguments, out detected);
            return ProxyArgumentBuilder.Build(proxy) + "|" + clean;
        }
    }
}
