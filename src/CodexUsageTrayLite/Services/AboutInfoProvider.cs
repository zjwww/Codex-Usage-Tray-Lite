using System;
using System.Reflection;
using Microsoft.Web.WebView2.Core;

namespace CodexUsageTrayLite.Services
{
    internal sealed class AboutInfo
    {
        public string ProgramName { get; set; }
        public string Version { get; set; }
        public string Architecture { get; set; }
        public string TargetFramework { get; set; }
        public string BuildTimeUtc { get; set; }
        public string WebView2SdkVersion { get; set; }
        public string WebView2RuntimeVersion { get; set; }
        public string Baseline { get; set; }
        public string License { get; set; }
    }

    internal static class AboutInfoProvider
    {
        private const string BuildTimestampKey = "BuildTimestampUtc";

        public static AboutInfo GetCurrent()
        {
            var assembly = typeof(AboutInfoProvider).Assembly;
            return new AboutInfo
            {
                ProgramName = AppConstants.Name,
                Version = AppConstants.Version,
                Architecture = Environment.Is64BitProcess ? "x64" : "x86",
                TargetFramework = ".NET Framework 4.8",
                BuildTimeUtc = ReadMetadata(assembly, BuildTimestampKey) ?? "Unavailable",
                WebView2SdkVersion = typeof(CoreWebView2Environment).Assembly.GetName().Version.ToString(),
                WebView2RuntimeVersion = ReadRuntimeVersion(),
                Baseline = AppConstants.BaselineTag + " (" + AppConstants.BaselineCommit.Substring(0, 12) + ")",
                License = "GPL-3.0-only"
            };
        }

        internal static string ReadMetadata(Assembly assembly, string key)
        {
            if (assembly == null || string.IsNullOrWhiteSpace(key)) return null;
            foreach (AssemblyMetadataAttribute attribute in assembly.GetCustomAttributes(typeof(AssemblyMetadataAttribute), false))
            {
                if (string.Equals(attribute.Key, key, StringComparison.Ordinal)) return attribute.Value;
            }
            return null;
        }

        private static string ReadRuntimeVersion()
        {
            try
            {
                var version = CoreWebView2Environment.GetAvailableBrowserVersionString();
                return string.IsNullOrWhiteSpace(version) ? "Not detected" : version;
            }
            catch
            {
                return "Not detected";
            }
        }
    }
}
