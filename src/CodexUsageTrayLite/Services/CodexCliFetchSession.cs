using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using CodexUsageTrayLite.Infrastructure;
using CodexUsageTrayLite.Models;

namespace CodexUsageTrayLite.Services
{
    internal sealed class CodexAppServerRpcException : Exception
    {
        public CodexAppServerRpcException(string message) : base(message) { }
    }

    internal sealed class CodexCliFetchSession : IUsageFetchSession
    {
        private const int MaxStderrCharacters = 8192;
        private readonly object processSync = new object();
        private readonly object stderrSync = new object();
        private readonly StringBuilder stderr = new StringBuilder();
        private readonly JavaScriptSerializer serializer = new JavaScriptSerializer
        {
            MaxJsonLength = AppConstants.CodexCliMaxJsonLength,
            RecursionLimit = 64
        };
        private Process process;
        private OwnedProcessJob job;
        private long nextRequestId;
        private bool disposed;

        public async Task<FetchOutcome> RunAsync(CancellationToken token)
        {
            if (disposed) throw new ObjectDisposedException(GetType().Name);
            try
            {
                StartProcess();
                await RequestAsync(
                    "initialize",
                    new Dictionary<string, object>
                    {
                        {
                            "clientInfo",
                            new Dictionary<string, object>
                            {
                                { "name", "codex_usage_tray_lite" },
                                { "title", AppConstants.Name },
                                { "version", AppConstants.Version }
                            }
                        }
                    },
                    token).ConfigureAwait(false);
                await NotifyAsync("initialized", new Dictionary<string, object>(), token).ConfigureAwait(false);

                var accountResult = await RequestAsync(
                    "account/read",
                    new Dictionary<string, object> { { "refreshToken", false } },
                    token).ConfigureAwait(false);
                object account;
                if (!accountResult.TryGetValue("account", out account) || account == null)
                {
                    return FetchOutcome.Create(FetchStatus.LoginRequired, "Codex CLI is not signed in with ChatGPT.");
                }
                string accountEmail;
                UserLevel accountLevel;
                var accountEmailRead = AccountEmailHelper.TryReadCliAccount(account, out accountEmail, out accountLevel);

                var result = await RequestAsync("account/rateLimits/read", null, token).ConfigureAwait(false);
                UsageSnapshot snapshot;
                string parseError;
                UsageFieldDiagnostics diagnostics;
                if (!CodexRateLimitsParser.TryParse(result, DateTime.Now, out snapshot, out parseError, out diagnostics))
                {
                    UsageFieldTelemetry.WriteAll(UsageSource.CodexCli, diagnostics);
                    return FetchOutcome.Create(FetchStatus.ParseError, parseError);
                }
                UsageFieldTelemetry.WriteAll(UsageSource.CodexCli, diagnostics);
                if (accountLevel != UserLevel.Unknown) snapshot.userLevel = accountLevel;
                var outcome = FetchOutcome.Create(FetchStatus.Success, "Usage read from Codex CLI.", snapshot);
                return accountEmailRead ? outcome.WithAccountEmail(accountEmail) : outcome;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (CodexCliNotFoundException ex)
            {
                SafeLogger.Write("CodexCli.NotFound", ex.Message);
                return FetchOutcome.Create(FetchStatus.RuntimeMissing, ex.Message);
            }
            catch (CodexAppServerRpcException ex)
            {
                var message = ex.Message ?? string.Empty;
                var authenticationError = IsAuthenticationError(message);
                SafeLogger.Write("CodexCli.RpcFailed", "authentication=" + authenticationError);
                return FetchOutcome.Create(authenticationError ? FetchStatus.LoginRequired : FetchStatus.NetworkError,
                    authenticationError ? "Codex CLI sign-in is required." : "Codex CLI could not read rate limits.");
            }
            catch (Win32Exception ex)
            {
                SafeLogger.Write("CodexCli.StartFailed", ex.NativeErrorCode.ToString(CultureInfo.InvariantCulture));
                return FetchOutcome.Create(FetchStatus.RuntimeMissing, "Codex CLI could not be started.");
            }
            catch (Exception ex) when (ex is IOException || ex is InvalidOperationException)
            {
                SafeLogger.Write("CodexCli.TransportFailed", ex.GetType().Name + ": " + ex.Message + StderrSuffix());
                return FetchOutcome.Create(FetchStatus.NetworkError, "The Codex CLI app-server connection failed.");
            }
            catch (Exception ex) when (ex is ArgumentException || ex is FormatException || ex is OverflowException)
            {
                SafeLogger.Write("CodexCli.ParseFailed", ex.GetType().Name + ": " + ex.Message);
                return FetchOutcome.Create(FetchStatus.ParseError, "Codex CLI returned an unsupported response.");
            }
            finally
            {
                StopProcess();
            }
        }

        private void StartProcess()
        {
            var executable = CodexCliExecutableLocator.Resolve();
            var started = new Process { StartInfo = CodexProcessStartInfoFactory.Create(executable), EnableRaisingEvents = true };
            started.ErrorDataReceived += HandleErrorData;
            try
            {
                if (!started.Start()) throw new InvalidOperationException("Codex CLI did not start.");
            }
            catch
            {
                started.ErrorDataReceived -= HandleErrorData;
                started.Dispose();
                throw;
            }
            var ownedJob = OwnedProcessJob.TryCreate();
            var assigned = ownedJob != null && ownedJob.TryAssign(started);
            if (ownedJob != null && !assigned)
            {
                ownedJob.Dispose();
                ownedJob = null;
            }
            started.BeginErrorReadLine();
            lock (processSync)
            {
                if (disposed)
                {
                    if (ownedJob != null) ownedJob.Dispose();
                    started.Dispose();
                    throw new ObjectDisposedException(GetType().Name);
                }
                process = started;
                job = ownedJob;
            }
            SafeLogger.Write(
                "CodexCli.ProcessStarted",
                "pid=" + started.Id.ToString(CultureInfo.InvariantCulture) +
                " launcher=" + Path.GetExtension(executable).TrimStart('.').ToLowerInvariant() +
                " jobAssigned=" + assigned.ToString().ToLowerInvariant());
        }

        private async Task<IDictionary<string, object>> RequestAsync(string method, object parameters, CancellationToken token)
        {
            var id = Interlocked.Increment(ref nextRequestId);
            var request = new Dictionary<string, object>
            {
                { "method", method },
                { "id", id }
            };
            if (parameters != null) request.Add("params", parameters);
            SendMessage(request, token);

            while (true)
            {
                var response = await ReadMessageAsync(token).ConfigureAwait(false);
                object rawId;
                if (!response.TryGetValue("id", out rawId) || rawId == null) continue;
                long responseId;
                try
                {
                    responseId = Convert.ToInt64(rawId, CultureInfo.InvariantCulture);
                }
                catch (Exception ex) when (ex is FormatException || ex is InvalidCastException || ex is OverflowException)
                {
                    continue;
                }
                if (responseId != id) continue;

                object rawError;
                if (response.TryGetValue("error", out rawError) && rawError != null)
                {
                    var error = CodexRateLimitsParser.AsDictionary(rawError);
                    object rawMessage;
                    var message = error != null && error.TryGetValue("message", out rawMessage)
                        ? Convert.ToString(rawMessage, CultureInfo.InvariantCulture)
                        : "Codex app-server request failed.";
                    throw new CodexAppServerRpcException(message);
                }
                object rawResult;
                return response.TryGetValue("result", out rawResult)
                    ? CodexRateLimitsParser.AsDictionary(rawResult) ?? new Dictionary<string, object>()
                    : new Dictionary<string, object>();
            }
        }

        private Task NotifyAsync(string method, object parameters, CancellationToken token)
        {
            SendMessage(new Dictionary<string, object>
            {
                { "method", method },
                { "params", parameters }
            }, token);
            return Task.CompletedTask;
        }

        private void SendMessage(IDictionary<string, object> message, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            Process current;
            lock (processSync) current = process;
            if (current == null || current.HasExited) throw new IOException("Codex CLI exited before the request was sent.");
            var json = serializer.Serialize(message);
            current.StandardInput.WriteLine(json);
            current.StandardInput.Flush();
        }

        private async Task<IDictionary<string, object>> ReadMessageAsync(CancellationToken token)
        {
            Process current;
            lock (processSync) current = process;
            if (current == null) throw new IOException("Codex CLI is not running.");
            var line = await WithCancellation(current.StandardOutput.ReadLineAsync(), token).ConfigureAwait(false);
            if (line == null) throw new IOException("Codex CLI closed the app-server stream." + StderrSuffix());
            if (line.Length > AppConstants.CodexCliMaxJsonLength) throw new FormatException("Codex CLI response exceeded the size limit.");
            var message = serializer.DeserializeObject(line) as IDictionary<string, object>;
            if (message == null) throw new FormatException("Codex CLI returned a non-object JSON message.");
            return message;
        }

        private static async Task<T> WithCancellation<T>(Task<T> task, CancellationToken token)
        {
            var cancellation = new TaskCompletionSource<bool>();
            using (token.Register(state => ((TaskCompletionSource<bool>)state).TrySetResult(true), cancellation))
            {
                if (task != await Task.WhenAny(task, cancellation.Task).ConfigureAwait(false))
                {
                    throw new OperationCanceledException(token);
                }
            }
            return await task.ConfigureAwait(false);
        }

        private void HandleErrorData(object sender, DataReceivedEventArgs args)
        {
            if (string.IsNullOrWhiteSpace(args.Data)) return;
            lock (stderrSync)
            {
                if (stderr.Length >= MaxStderrCharacters) return;
                var remaining = MaxStderrCharacters - stderr.Length;
                var value = args.Data.Length <= remaining ? args.Data : args.Data.Substring(0, remaining);
                if (stderr.Length > 0) stderr.Append(' ');
                stderr.Append(value);
            }
        }

        private string StderrSuffix()
        {
            lock (stderrSync)
            {
                if (stderr.Length == 0) return string.Empty;
                // Stderr can contain account- or machine-specific diagnostics. Record only
                // that diagnostics existed, never their content.
                return " stderrPresent=true";
            }
        }

        private static bool IsAuthenticationError(string message)
        {
            return message.IndexOf("authentication", StringComparison.OrdinalIgnoreCase) >= 0 ||
                message.IndexOf("sign in", StringComparison.OrdinalIgnoreCase) >= 0 ||
                message.IndexOf("login", StringComparison.OrdinalIgnoreCase) >= 0 ||
                message.IndexOf("reauthentication", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void StopProcess()
        {
            Process current;
            OwnedProcessJob currentJob;
            lock (processSync)
            {
                current = process;
                currentJob = job;
                process = null;
                job = null;
            }
            if (current != null)
            {
                CodexProcessStopResult stopResult;
                try
                {
                    stopResult = CodexProcessStopper.Stop(current, AppConstants.CodexCliStopGraceMilliseconds);
                }
                catch (Exception ex)
                {
                    stopResult = new CodexProcessStopResult
                    {
                        ProcessId = 0,
                        ExitType = "stop-exception"
                    };
                    SafeLogger.Write("CodexCli.ProcessStopFailed", ex.GetType().Name);
                }
                try { current.CancelErrorRead(); } catch (Exception ex) when (ex is InvalidOperationException) { }
                current.ErrorDataReceived -= HandleErrorData;
                current.Dispose();

                uint activeProcesses = 0;
                var jobAssigned = currentJob != null;
                var jobQuerySucceeded = false;
                long jobDrainWaitMilliseconds = 0;
                if (jobAssigned)
                {
                    jobQuerySucceeded = ShouldWaitForJobDrain(stopResult)
                        ? currentJob.WaitForNoActiveProcesses(
                            AppConstants.CodexCliJobDrainGraceMilliseconds,
                            AppConstants.CodexCliJobDrainPollMilliseconds,
                            out activeProcesses,
                            out jobDrainWaitMilliseconds)
                        : currentJob.TryGetActiveProcessCount(out activeProcesses);
                }
                stopResult.JobDrainWaitMilliseconds = jobDrainWaitMilliseconds;
                if (currentJob != null) currentJob.Dispose();
                currentJob = null;
                SafeLogger.Write(
                    ProcessExitLogLevel(stopResult, jobQuerySucceeded, activeProcesses),
                    "CodexCli.ProcessExit",
                    stopResult.ToLogDetails(jobAssigned, jobQuerySucceeded, activeProcesses));
            }
            else if (currentJob != null)
            {
                uint activeProcesses;
                var jobQuerySucceeded = currentJob.TryGetActiveProcessCount(out activeProcesses);
                currentJob.Dispose();
                var stopResult = new CodexProcessStopResult { ProcessId = 0, ExitType = "root-unavailable" };
                SafeLogger.Write(
                    ProcessExitLogLevel(stopResult, jobQuerySucceeded, activeProcesses),
                    "CodexCli.ProcessExit",
                    stopResult.ToLogDetails(true, jobQuerySucceeded, activeProcesses));
            }
        }

        internal static LogEventLevel ProcessExitLogLevel(CodexProcessStopResult result, bool jobQuerySucceeded, uint activeProcesses)
        {
            if (result == null) return LogEventLevel.Error;
            if (string.Equals(result.ExitType, CodexProcessStopper.ForcedKillTimeout, StringComparison.Ordinal) ||
                string.Equals(result.ExitType, CodexProcessStopper.ForcedKillFailed, StringComparison.Ordinal) ||
                string.Equals(result.ExitType, "stop-exception", StringComparison.Ordinal))
            {
                return LogEventLevel.Error;
            }
            if (string.Equals(result.ExitType, CodexProcessStopper.ForcedKill, StringComparison.Ordinal) ||
                string.Equals(result.ExitType, "root-unavailable", StringComparison.Ordinal) ||
                !jobQuerySucceeded || activeProcesses > 0)
            {
                return LogEventLevel.Warning;
            }
            return LogEventLevel.Info;
        }

        internal static bool ShouldWaitForJobDrain(CodexProcessStopResult result)
        {
            if (result == null) return false;
            return string.Equals(result.ExitType, CodexProcessStopper.AlreadyExited, StringComparison.Ordinal) ||
                string.Equals(result.ExitType, CodexProcessStopper.GracefulAfterStdinClose, StringComparison.Ordinal) ||
                string.Equals(result.ExitType, CodexProcessStopper.NaturalDuringStop, StringComparison.Ordinal);
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            StopProcess();
        }
    }
}
