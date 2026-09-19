using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace CodexUsageTrayLite.Services
{
    internal static class CodexProcessStartInfoFactory
    {
        public static ProcessStartInfo Create(string executable)
        {
            if (string.IsNullOrWhiteSpace(executable)) throw new ArgumentNullException("executable");
            var extension = Path.GetExtension(executable);
            var commandScript = extension.Equals(".cmd", StringComparison.OrdinalIgnoreCase) ||
                extension.Equals(".bat", StringComparison.OrdinalIgnoreCase);
            var info = commandScript
                ? new ProcessStartInfo(
                    Environment.GetEnvironmentVariable("COMSPEC") ?? Path.Combine(Environment.SystemDirectory, "cmd.exe"),
                    "/d /s /c \"\"" + executable + "\" app-server\"")
                : new ProcessStartInfo(executable, "app-server");
            info.UseShellExecute = false;
            info.RedirectStandardInput = true;
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;
            info.CreateNoWindow = true;
            info.StandardOutputEncoding = Encoding.UTF8;
            info.StandardErrorEncoding = Encoding.UTF8;
            return info;
        }
    }
}
