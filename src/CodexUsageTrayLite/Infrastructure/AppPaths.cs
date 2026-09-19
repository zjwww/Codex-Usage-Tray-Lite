using System;
using System.IO;

namespace CodexUsageTrayLite.Infrastructure
{
    internal static class AppPaths
    {
        public static readonly string DataDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            AppConstants.AssemblyName);

        public static readonly string LogsDirectory = Path.Combine(DataDirectory, "logs");
        public static readonly string WebViewProfileDirectory = Path.Combine(DataDirectory, "webview2-profile");
        public static readonly string SettingsPath = Path.Combine(DataDirectory, "settings.json");
        public static readonly string LogPath = Path.Combine(LogsDirectory, "CodexUsageTrayLite.log");

        public static void EnsureBaseDirectories()
        {
            Directory.CreateDirectory(DataDirectory);
            Directory.CreateDirectory(LogsDirectory);
        }
    }
}
