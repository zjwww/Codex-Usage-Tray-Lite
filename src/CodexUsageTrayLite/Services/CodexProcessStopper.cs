using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace CodexUsageTrayLite.Services
{
    internal sealed class CodexProcessStopResult
    {
        public int ProcessId { get; set; }
        public string ExitType { get; set; }
        public int? ExitCode { get; set; }
        public bool StandardInputClosed { get; set; }
        public bool KillAttempted { get; set; }
        public bool KillCallReturned { get; set; }
        public long ElapsedMilliseconds { get; set; }
        public long JobDrainWaitMilliseconds { get; set; }

        public string ToLogDetails(bool jobAssigned, bool jobQuerySucceeded, uint jobActiveBeforeClose)
        {
            var jobCloseAction = !jobAssigned
                ? "not-assigned"
                : !jobQuerySucceeded
                    ? "kill-on-close-status-unknown"
                    : jobActiveBeforeClose > 0
                        ? "kill-on-close-requested"
                        : "no-active-processes";
            return "pid=" + ProcessId.ToString(CultureInfo.InvariantCulture) +
                " exitType=" + (string.IsNullOrWhiteSpace(ExitType) ? "unknown" : ExitType) +
                " exitCode=" + (ExitCode.HasValue ? ExitCode.Value.ToString(CultureInfo.InvariantCulture) : "unavailable") +
                " stdinClosed=" + StandardInputClosed.ToString().ToLowerInvariant() +
                " killAttempted=" + KillAttempted.ToString().ToLowerInvariant() +
                " killCallReturned=" + KillCallReturned.ToString().ToLowerInvariant() +
                " jobAssigned=" + jobAssigned.ToString().ToLowerInvariant() +
                " jobActiveBeforeClose=" + (jobQuerySucceeded ? jobActiveBeforeClose.ToString(CultureInfo.InvariantCulture) : "unavailable") +
                " jobCloseAction=" + jobCloseAction +
                " jobDrainWaitMs=" + JobDrainWaitMilliseconds.ToString(CultureInfo.InvariantCulture) +
                " elapsedMs=" + ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture);
        }
    }

    internal static class CodexProcessStopper
    {
        internal const string AlreadyExited = "already-exited";
        internal const string GracefulAfterStdinClose = "graceful-after-stdin-close";
        internal const string NaturalDuringStop = "natural-during-stop";
        internal const string ForcedKill = "forced-kill";
        internal const string ForcedKillTimeout = "forced-kill-timeout";
        internal const string ForcedKillFailed = "forced-kill-failed";

        public static CodexProcessStopResult Stop(Process process, int graceMilliseconds)
        {
            if (process == null) throw new ArgumentNullException("process");
            if (graceMilliseconds < 0) throw new ArgumentOutOfRangeException("graceMilliseconds");

            var watch = Stopwatch.StartNew();
            var result = new CodexProcessStopResult { ProcessId = TryGetProcessId(process) };
            var initiallyExited = HasExited(process);
            if (initiallyExited)
            {
                result.ExitType = AlreadyExited;
            }
            else
            {
                try
                {
                    process.StandardInput.Close();
                    result.StandardInputClosed = true;
                }
                catch (Exception ex) when (ex is IOException || ex is InvalidOperationException)
                {
                }

                if (WaitForExit(process, graceMilliseconds))
                {
                    result.ExitType = result.StandardInputClosed ? GracefulAfterStdinClose : NaturalDuringStop;
                }
                else
                {
                    result.KillAttempted = true;
                    try
                    {
                        process.Kill();
                        result.KillCallReturned = true;
                    }
                    catch (Exception ex) when (ex is InvalidOperationException || ex is Win32Exception || ex is NotSupportedException)
                    {
                    }

                    if (WaitForExit(process, graceMilliseconds))
                    {
                        result.ExitType = result.KillCallReturned ? ForcedKill : NaturalDuringStop;
                    }
                    else
                    {
                        result.ExitType = result.KillCallReturned ? ForcedKillTimeout : ForcedKillFailed;
                    }
                }
            }

            result.ExitCode = TryGetExitCode(process);
            watch.Stop();
            result.ElapsedMilliseconds = watch.ElapsedMilliseconds;
            return result;
        }

        private static int TryGetProcessId(Process process)
        {
            try { return process.Id; }
            catch (InvalidOperationException) { return 0; }
        }

        private static bool HasExited(Process process)
        {
            try { return process.HasExited; }
            catch (InvalidOperationException) { return false; }
            catch (Win32Exception) { return false; }
        }

        private static bool WaitForExit(Process process, int milliseconds)
        {
            try
            {
                if (process.HasExited || process.WaitForExit(milliseconds))
                {
                    process.WaitForExit();
                    return true;
                }
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is Win32Exception)
            {
            }
            return HasExited(process);
        }

        private static int? TryGetExitCode(Process process)
        {
            try { return process.HasExited ? (int?)process.ExitCode : null; }
            catch (Exception ex) when (ex is InvalidOperationException || ex is Win32Exception) { return null; }
        }
    }
}
