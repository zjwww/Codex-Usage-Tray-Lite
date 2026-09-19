using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace CodexUsageTrayLite.Infrastructure
{
    internal enum LogEventLevel
    {
        Info,
        Warning,
        Error
    }

    internal static class SafeLogger
    {
        private static readonly object Sync = new object();
        private static readonly Regex UrlQuery = new Regex(@"(?<scheme>https?://)(?<host>[^/\s?#]+)(?:[^\s]*)?", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static readonly Regex SensitivePair = new Regex(@"(?i)\b(cookie|authorization|access[_ -]?token|oauth[_ -]?token|password)\b\s*[:=]\s*[^\s,;]+", RegexOptions.CultureInvariant);
        private static readonly Regex EmailAddress = new Regex(@"\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,63}\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        public static void Write(string eventName, string details = null)
        {
            Write(InferLevel(eventName), eventName, details);
        }

        public static void Write(LogEventLevel level, string eventName, string details = null)
        {
            lock (Sync)
            {
                try
                {
                    AppPaths.EnsureBaseDirectories();
                    var timestamp = DateTime.Now;
                    RotateDailyIfNeeded(AppPaths.LogPath, timestamp);
                    var line = FormatLine(timestamp, level, eventName, details);
                    File.AppendAllText(AppPaths.LogPath, line + Environment.NewLine, new UTF8Encoding(false));
                }
                catch
                {
                    // Logging must never terminate the tray process.
                }
            }
        }

        internal static string Sanitize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }
            var text = value.Replace('\r', ' ').Replace('\n', ' ');
            text = UrlQuery.Replace(text, "${scheme}${host}/[redacted]");
            text = SensitivePair.Replace(text, "$1=[redacted]");
            text = EmailAddress.Replace(text, match => AccountEmailPrivacy.MaskForDisplay(match.Value) ?? "[redacted-email]");
            return text.Length <= 800 ? text : text.Substring(0, 800) + "...";
        }

        internal static string FormatLine(DateTime timestamp, LogEventLevel level, string eventName, string details)
        {
            var line = timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff") +
                " | level=" + FormatLevel(level) +
                " | event=" + SanitizeEvent(eventName);
            var safeDetails = Sanitize(details);
            if (!string.IsNullOrWhiteSpace(safeDetails)) line += " | " + safeDetails;
            return line;
        }

        internal static LogEventLevel InferLevel(string eventName)
        {
            var value = eventName ?? string.Empty;
            if (value.Equals("Fatal", StringComparison.OrdinalIgnoreCase) ||
                Contains(value, ".Exception") ||
                Contains(value, ".ProcessFailed") ||
                Contains(value, ".ProcessStopFailed") ||
                Contains(value, ".TransportFailed") ||
                Contains(value, ".StartFailed") ||
                Contains(value, ".LoginCreateFailed"))
            {
                return LogEventLevel.Error;
            }
            if (Contains(value, ".Failed") ||
                Contains(value, ".Timeout") ||
                Contains(value, ".Skipped") ||
                Contains(value, ".Cancelled") ||
                Contains(value, ".NotFound") ||
                Contains(value, ".Unavailable") ||
                Contains(value, ".Detected"))
            {
                return LogEventLevel.Warning;
            }
            return LogEventLevel.Info;
        }

        private static bool Contains(string value, string fragment)
        {
            return value.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string FormatLevel(LogEventLevel level)
        {
            switch (level)
            {
                case LogEventLevel.Warning:
                    return "warning";
                case LogEventLevel.Error:
                    return "error";
                default:
                    return "info";
            }
        }

        private static string SanitizeEvent(string value)
        {
            return Regex.Replace(value ?? "Event", @"[^A-Za-z0-9_.-]", "_");
        }

        internal static void RotateDailyIfNeeded(string logPath, DateTime timestamp)
        {
            if (string.IsNullOrWhiteSpace(logPath)) throw new ArgumentException("A log path is required.", "logPath");

            var file = new FileInfo(logPath);
            if (!file.Exists || file.Length == 0)
            {
                return;
            }

            var logDate = file.LastWriteTime.Date;
            if (logDate >= timestamp.Date)
            {
                return;
            }

            var archivePath = BuildDailyArchivePath(logPath, logDate);
            if (!File.Exists(archivePath))
            {
                File.Move(logPath, archivePath);
                return;
            }

            using (var archive = new FileStream(archivePath, FileMode.Append, FileAccess.Write, FileShare.Read))
            {
                if (archive.Length > 0)
                {
                    var separator = new UTF8Encoding(false).GetBytes(Environment.NewLine);
                    archive.Write(separator, 0, separator.Length);
                }
                using (var active = new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    active.CopyTo(archive);
                }
            }
            File.Delete(logPath);
        }

        internal static string BuildDailyArchivePath(string logPath, DateTime logDate)
        {
            if (string.IsNullOrWhiteSpace(logPath)) throw new ArgumentException("A log path is required.", "logPath");
            var directory = Path.GetDirectoryName(logPath) ?? string.Empty;
            var fileName = Path.GetFileNameWithoutExtension(logPath);
            var extension = Path.GetExtension(logPath);
            return Path.Combine(directory, fileName + "_" + logDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + extension);
        }
    }
}
