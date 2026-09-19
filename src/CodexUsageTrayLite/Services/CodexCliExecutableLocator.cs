using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CodexUsageTrayLite.Services
{
    internal sealed class CodexCliNotFoundException : Exception
    {
        public CodexCliNotFoundException(string message) : base(message) { }
    }

    internal static class CodexCliExecutableLocator
    {
        internal const string OverrideEnvironmentVariable = "CODEX_USAGE_TRAY_LITE_CODEX_PATH";
        private static readonly string[] ExecutableNames = { "codex.exe", "codex.cmd", "codex.bat" };

        public static string Resolve()
        {
            var overridePath = Environment.GetEnvironmentVariable(OverrideEnvironmentVariable);
            if (!string.IsNullOrWhiteSpace(overridePath))
            {
                return ValidateExecutable(overridePath, true);
            }

            var fromPath = FindOnPath(Environment.GetEnvironmentVariable("PATH"));
            if (fromPath != null) return fromPath;

            var roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var fromNpm = FindInDirectory(Path.Combine(roaming, "npm"));
            if (fromNpm != null) return fromNpm;

            var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var codexBin = Path.Combine(local, "OpenAI", "Codex", "bin");
            var fromDesktop = FindNewestDesktopExecutable(codexBin);
            if (fromDesktop != null) return fromDesktop;

            throw new CodexCliNotFoundException(
                "Codex CLI was not found. Install Codex CLI or set " + OverrideEnvironmentVariable + " to codex.exe or codex.cmd.");
        }

        internal static string FindOnPath(string pathValue)
        {
            if (string.IsNullOrWhiteSpace(pathValue)) return null;
            foreach (var rawDirectory in pathValue.Split(Path.PathSeparator))
            {
                var directory = (rawDirectory ?? string.Empty).Trim().Trim('"');
                if (directory.Length == 0) continue;
                try
                {
                    directory = Environment.ExpandEnvironmentVariables(directory);
                    var match = FindInDirectory(directory);
                    if (match != null) return match;
                }
                catch (Exception ex) when (ex is ArgumentException || ex is NotSupportedException || ex is PathTooLongException)
                {
                }
            }
            return null;
        }

        private static string FindInDirectory(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory)) return null;
            foreach (var name in ExecutableNames)
            {
                var candidate = Path.Combine(directory, name);
                if (File.Exists(candidate)) return Path.GetFullPath(candidate);
            }
            return null;
        }

        private static string FindNewestDesktopExecutable(string directory)
        {
            if (!Directory.Exists(directory)) return null;
            try
            {
                return Directory.EnumerateFiles(directory, "codex.exe", SearchOption.AllDirectories)
                    .Select(path => new { Path = path, LastWrite = File.GetLastWriteTimeUtc(path) })
                    .OrderByDescending(item => item.LastWrite)
                    .Select(item => Path.GetFullPath(item.Path))
                    .FirstOrDefault();
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is PathTooLongException)
            {
                return null;
            }
        }

        private static string ValidateExecutable(string path, bool explicitOverride)
        {
            string fullPath;
            try
            {
                fullPath = Path.GetFullPath(Environment.ExpandEnvironmentVariables(path.Trim().Trim('"')));
            }
            catch (Exception ex) when (ex is ArgumentException || ex is NotSupportedException || ex is PathTooLongException)
            {
                throw new CodexCliNotFoundException("The configured Codex CLI path is invalid.");
            }
            var extension = Path.GetExtension(fullPath);
            var allowed = extension.Equals(".exe", StringComparison.OrdinalIgnoreCase) ||
                extension.Equals(".cmd", StringComparison.OrdinalIgnoreCase) ||
                extension.Equals(".bat", StringComparison.OrdinalIgnoreCase);
            if (!allowed || !File.Exists(fullPath))
            {
                throw new CodexCliNotFoundException(explicitOverride
                    ? "The Codex CLI path in " + OverrideEnvironmentVariable + " does not point to an existing .exe, .cmd, or .bat file."
                    : "Codex CLI was not found.");
            }
            return fullPath;
        }
    }
}
