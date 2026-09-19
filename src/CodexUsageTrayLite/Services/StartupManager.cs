using System;
using System.Reflection;
using Microsoft.Win32;

namespace CodexUsageTrayLite.Services
{
    internal static class StartupManager
    {
        private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

        public static bool IsEnabled()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, false))
            {
                var current = key == null ? null : key.GetValue(AppConstants.RunValueName) as string;
                return string.Equals(Normalize(current), Normalize(BuildCommand()), StringComparison.OrdinalIgnoreCase);
            }
        }

        public static void SetEnabled(bool enabled)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(RunKeyPath))
            {
                if (key == null) throw new InvalidOperationException("The current-user Run key could not be opened.");
                if (enabled)
                {
                    key.SetValue(AppConstants.RunValueName, BuildCommand(), RegistryValueKind.String);
                }
                else
                {
                    key.DeleteValue(AppConstants.RunValueName, false);
                }
            }
        }

        internal static string BuildCommand(string executablePath = null)
        {
            var path = executablePath ?? Assembly.GetExecutingAssembly().Location;
            return "\"" + path + "\" --startup";
        }

        private static string Normalize(string value)
        {
            return (value ?? string.Empty).Trim();
        }
    }
}
