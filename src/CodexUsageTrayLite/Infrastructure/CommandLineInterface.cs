using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using CodexUsageTrayLite.Models;
using CodexUsageTrayLite.Services;

namespace CodexUsageTrayLite.Infrastructure
{
    internal enum CommandLineVerb
    {
        Run,
        Help,
        Version,
        Status,
        Refresh,
        Exit,
        LogPath,
        ConfigPath,
        Invalid
    }

    internal sealed class CommandLineRequest
    {
        public CommandLineVerb Verb { get; private set; }
        public string Error { get; private set; }

        private CommandLineRequest(CommandLineVerb verb, string error = null)
        {
            Verb = verb;
            Error = error;
        }

        public static CommandLineRequest Parse(string[] args)
        {
            if (args == null || args.Length == 0) return new CommandLineRequest(CommandLineVerb.Run);
            if (args.Length != 1)
            {
                return new CommandLineRequest(CommandLineVerb.Invalid, "Only one command-line option can be used at a time.");
            }

            var raw = (args[0] ?? string.Empty).Trim();
            if (raw.Length == 0) return new CommandLineRequest(CommandLineVerb.Invalid, "The command-line option is empty.");
            var option = raw.TrimStart('-', '/').ToLowerInvariant();
            switch (option)
            {
                case "startup":
                    // Existing HKCU Run entries use --startup. It intentionally
                    // follows the normal no-option tray startup path.
                    return new CommandLineRequest(CommandLineVerb.Run);
                case "help":
                case "h":
                case "?":
                    return new CommandLineRequest(CommandLineVerb.Help);
                case "version":
                case "v":
                    return new CommandLineRequest(CommandLineVerb.Version);
                case "status":
                    return new CommandLineRequest(CommandLineVerb.Status);
                case "refresh":
                    return new CommandLineRequest(CommandLineVerb.Refresh);
                case "exit":
                case "quit":
                    return new CommandLineRequest(CommandLineVerb.Exit);
                case "log-path":
                case "logs-path":
                    return new CommandLineRequest(CommandLineVerb.LogPath);
                case "config-path":
                case "data-path":
                    return new CommandLineRequest(CommandLineVerb.ConfigPath);
                default:
                    return new CommandLineRequest(CommandLineVerb.Invalid, "Unknown command-line option: " + raw);
            }
        }
    }

    internal static class CommandLineInterface
    {
        private const int StdOutputHandle = -11;
        private const int StdErrorHandle = -12;
        private const uint AttachParentProcess = 0xFFFFFFFF;
        private static readonly IntPtr InvalidHandleValue = new IntPtr(-1);
        private static InteractivePromptState interactivePrompt;

        public static string HelpText
        {
            get
            {
                return
                    "Codex Usage Tray Lite " + AppConstants.Version + Environment.NewLine +
                    Environment.NewLine +
                    "Usage: CodexUsageTrayLite.exe [option]" + Environment.NewLine +
                    Environment.NewLine +
                    "With no option, starts the notification-area application." + Environment.NewLine +
                    Environment.NewLine +
                    "Options:" + Environment.NewLine +
                    "  -help, -h, -?       Show this English command-line help." + Environment.NewLine +
                    "  -version, -v        Print the application version." + Environment.NewLine +
                    "  -status             Print the last saved usage snapshot; does not refresh." + Environment.NewLine +
                    "  -refresh            Ask the running tray instance to refresh now." + Environment.NewLine +
                    "  -exit, -quit        Ask the running tray instance to exit." + Environment.NewLine +
                    "  -log-path           Print the current log file path." + Environment.NewLine +
                    "  -config-path        Print the application data/configuration directory." + Environment.NewLine +
                    Environment.NewLine +
                    "The -refresh and -exit/-quit options require an already running tray instance.";
            }
        }

        public static void PrepareConsole()
        {
            try
            {
                if (!HasUsableHandle(GetStdHandle(StdOutputHandle)) &&
                    !AttachConsole(AttachParentProcess))
                {
                    AllocConsole();
                }

                var output = new StreamWriter(Console.OpenStandardOutput(), new UTF8Encoding(false)) { AutoFlush = true };
                var error = HasUsableHandle(GetStdHandle(StdErrorHandle))
                    ? new StreamWriter(Console.OpenStandardError(), new UTF8Encoding(false)) { AutoFlush = true }
                    : output;
                Console.SetOut(output);
                Console.SetError(error);

                interactivePrompt = CaptureInteractivePrompt(GetStdHandle(StdOutputHandle));
                if (interactivePrompt != null && !TryClearInteractivePrompt(interactivePrompt))
                {
                    // cmd.exe and PowerShell do not wait for a GUI-subsystem EXE.
                    // If its prematurely rendered prompt changed before it could
                    // be cleared safely, retain the v0.2.30 newline fallback.
                    Console.Out.WriteLine();
                }
            }
            catch
            {
                // Command execution and exit codes remain useful even if a host
                // launches this WinExe without an attachable or redirected console.
            }
        }

        public static void CompleteConsoleOutput()
        {
            try
            {
                Console.Out.Flush();
                Console.Error.Flush();
                if (interactivePrompt != null)
                {
                    // Keep one empty line between human-readable command output
                    // and the restored interactive shell prompt.
                    Console.Out.WriteLine();
                    Console.Out.Write(interactivePrompt.Prefix);
                    Console.Out.Flush();
                }
            }
            catch
            {
                // Output and the process exit code remain authoritative when a
                // console host disappears before prompt restoration completes.
            }
            finally
            {
                interactivePrompt = null;
            }
        }

        internal static bool IsRestorablePromptPrefix(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length > 512) return false;
            var trimmed = value.TrimEnd();
            if (trimmed.Length == 0 || trimmed[trimmed.Length - 1] != '>') return false;
            for (var i = 0; i < value.Length; i++)
            {
                if (char.IsControl(value[i])) return false;
            }
            return true;
        }

        internal static bool CanClearPromptLine(
            string expectedPrefix,
            string currentPrefix,
            int expectedColumn,
            int currentColumn,
            int expectedRow,
            int currentRow)
        {
            return IsRestorablePromptPrefix(expectedPrefix) &&
                string.Equals(expectedPrefix, currentPrefix, StringComparison.Ordinal) &&
                expectedColumn > 0 && expectedColumn == currentColumn &&
                expectedRow == currentRow;
        }

        public static string FormatSavedStatus(AppSettings settings)
        {
            settings = settings ?? AppSettings.CreateDefault();
            var snapshot = settings.lastSuccessfulUsage;
            var source = snapshot == null ? settings.usageSource : settings.lastSuccessfulUsageSource;
            var text = new StringBuilder();
            text.AppendLine(AppConstants.Name + " " + AppConstants.Version);
            text.AppendLine("Status: " + SavedStatusName(settings, snapshot));
            text.AppendLine("Source: " + SourceName(source));
            text.AppendLine("User level: " + UserLevelHelper.DisplayName(snapshot == null ? UserLevel.Unknown : snapshot.userLevel, "Unknown"));
            text.AppendLine("5-Hour: " + FormatFiveHour(snapshot));
            text.AppendLine("Weekly: " + FormatQuota(snapshot == null ? null : snapshot.weeklyRemainingPercent,
                snapshot == null ? null : snapshot.weeklyResetAt));
            text.AppendLine("Usage resets: " + FormatNullableInt(snapshot == null ? null : snapshot.availableUsageResetCount));
            text.Append("Last updated: " + FormatTimestamp(snapshot == null ? null : (DateTime?)snapshot.lastSuccessfulFetchAt) +
                " (" + SourceName(source) + ")");
            return text.ToString();
        }

        private static string SavedStatusName(AppSettings settings, UsageSnapshot snapshot)
        {
            if (settings.loginRequired) return "Login required";
            if (snapshot != null) return "Saved usage available";
            return settings.setupComplete ? "No saved usage" : "Not configured";
        }

        private static string FormatFiveHour(UsageSnapshot snapshot)
        {
            if (snapshot != null && !TooltipFormatter.ShouldDisplayFiveHour(snapshot)) return "N/A";
            return FormatQuota(snapshot == null ? null : snapshot.fiveHourRemainingPercent,
                snapshot == null ? null : snapshot.fiveHourResetAt);
        }

        private static string FormatQuota(int? remaining, DateTime? resetAt)
        {
            return (remaining.HasValue ? remaining.Value.ToString(CultureInfo.InvariantCulture) + "%" : "Unknown") +
                " (" + FormatTimestamp(resetAt) + ")";
        }

        private static string FormatNullableInt(int? value)
        {
            return value.HasValue ? value.Value.ToString(CultureInfo.InvariantCulture) : "Unknown";
        }

        private static string FormatTimestamp(DateTime? value)
        {
            if (!value.HasValue || value.Value == default(DateTime)) return "Unknown";
            var timestamp = value.Value.Kind == DateTimeKind.Utc ? value.Value.ToLocalTime() : value.Value;
            return timestamp.ToString("yyyy-MM-dd HH:mm:ss zzz", CultureInfo.InvariantCulture);
        }

        private static string SourceName(UsageSource source)
        {
            return source == UsageSource.CodexCli ? "Codex CLI" : "WebView2";
        }

        private static bool HasUsableHandle(IntPtr handle)
        {
            return handle != IntPtr.Zero && handle != InvalidHandleValue;
        }

        private static InteractivePromptState CaptureInteractivePrompt(IntPtr outputHandle)
        {
            if (!HasUsableHandle(outputHandle)) return null;
            ConsoleScreenBufferInfo info;
            if (!GetConsoleScreenBufferInfo(outputHandle, out info)) return null;
            var length = Math.Min(info.CursorPosition.X, (short)512);
            if (length <= 0) return null;

            var prefix = ReadConsoleCharacters(outputHandle, length, info.CursorPosition.Y);
            return IsRestorablePromptPrefix(prefix)
                ? new InteractivePromptState(outputHandle, prefix, info.CursorPosition.X, info.CursorPosition.Y)
                : null;
        }

        private static bool TryClearInteractivePrompt(InteractivePromptState prompt)
        {
            ConsoleScreenBufferInfo currentInfo;
            if (!GetConsoleScreenBufferInfo(prompt.OutputHandle, out currentInfo)) return false;
            var currentPrefix = ReadConsoleCharacters(prompt.OutputHandle, prompt.CellCount, prompt.Row);
            if (!CanClearPromptLine(
                prompt.Prefix,
                currentPrefix,
                prompt.CellCount,
                currentInfo.CursorPosition.X,
                prompt.Row,
                currentInfo.CursorPosition.Y))
            {
                return false;
            }

            var promptOrigin = new ConsoleCoordinate { X = 0, Y = prompt.Row };
            var outputRow = SelectInteractiveOutputRow(
                prompt.Row,
                currentInfo.Window.Top,
                IsConsoleRowBlank(prompt.OutputHandle, (short)(prompt.Row - 1), currentInfo.Size.X));
            uint written;
            if (!FillConsoleOutputCharacter(prompt.OutputHandle, ' ', (uint)prompt.CellCount, promptOrigin, out written) ||
                written != (uint)prompt.CellCount)
            {
                return false;
            }
            return SetConsoleCursorPosition(prompt.OutputHandle,
                new ConsoleCoordinate { X = 0, Y = (short)outputRow });
        }

        internal static int SelectInteractiveOutputRow(int promptRow, int windowTop, bool previousRowBlank)
        {
            // cmd.exe inserts a blank row before the premature prompt when it
            // launches a GUI-subsystem executable. Reuse that row only when it
            // is still visible and completely empty; otherwise keep the prompt
            // row so no existing console content can be overwritten.
            return previousRowBlank && promptRow > windowTop ? promptRow - 1 : promptRow;
        }

        private static bool IsConsoleRowBlank(IntPtr outputHandle, short row, short width)
        {
            if (row < 0 || width <= 0) return false;
            var buffer = new char[width];
            uint read;
            if (!ReadConsoleOutputCharacter(outputHandle, buffer, (uint)buffer.Length,
                new ConsoleCoordinate { X = 0, Y = row }, out read) || read != (uint)buffer.Length)
            {
                return false;
            }
            for (var i = 0; i < buffer.Length; i++)
            {
                if (buffer[i] != ' ' && buffer[i] != '\0') return false;
            }
            return true;
        }

        private static string ReadConsoleCharacters(IntPtr outputHandle, int length, short row)
        {
            if (length <= 0 || length > 512) return null;
            var buffer = new char[length];
            uint read;
            if (!ReadConsoleOutputCharacter(outputHandle, buffer, (uint)length,
                new ConsoleCoordinate { X = 0, Y = row }, out read) || read != (uint)length)
            {
                return null;
            }
            return new string(buffer, 0, (int)read).TrimEnd('\0');
        }

        private sealed class InteractivePromptState
        {
            public InteractivePromptState(IntPtr outputHandle, string prefix, short cellCount, short row)
            {
                OutputHandle = outputHandle;
                Prefix = prefix;
                CellCount = cellCount;
                Row = row;
            }

            public IntPtr OutputHandle { get; private set; }
            public string Prefix { get; private set; }
            public short CellCount { get; private set; }
            public short Row { get; private set; }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ConsoleCoordinate
        {
            public short X;
            public short Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SmallRectangle
        {
            public short Left;
            public short Top;
            public short Right;
            public short Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ConsoleScreenBufferInfo
        {
            public ConsoleCoordinate Size;
            public ConsoleCoordinate CursorPosition;
            public short Attributes;
            public SmallRectangle Window;
            public ConsoleCoordinate MaximumWindowSize;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AttachConsole(uint processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int standardHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetConsoleScreenBufferInfo(
            IntPtr consoleOutput,
            out ConsoleScreenBufferInfo consoleScreenBufferInfo);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ReadConsoleOutputCharacter(
            IntPtr consoleOutput,
            [Out] char[] character,
            uint length,
            ConsoleCoordinate readCoordinate,
            out uint numberOfCharsRead);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FillConsoleOutputCharacter(
            IntPtr consoleOutput,
            char character,
            uint length,
            ConsoleCoordinate writeCoordinate,
            out uint numberOfCharsWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetConsoleCursorPosition(
            IntPtr consoleOutput,
            ConsoleCoordinate cursorPosition);
    }
}
