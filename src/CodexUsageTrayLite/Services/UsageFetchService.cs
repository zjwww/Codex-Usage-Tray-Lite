using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CodexUsageTrayLite.Infrastructure;
using CodexUsageTrayLite.Models;

namespace CodexUsageTrayLite.Services
{
    internal interface IUsageFetchSession : IDisposable
    {
        Task<FetchOutcome> RunAsync(CancellationToken token);
    }

    internal interface IUsageFetchSessionFactory
    {
        IUsageFetchSession Create(UsageSource source, ProxySettings proxy);
    }

    internal sealed class UsageFetchSessionFactory : IUsageFetchSessionFactory
    {
        public IUsageFetchSession Create(UsageSource source, ProxySettings proxy)
        {
            switch (source)
            {
                case UsageSource.CodexCli:
                    return new CodexCliFetchSession();
                case UsageSource.WebView2:
                default:
                    return new WebViewFetchSession(proxy);
            }
        }
    }

    internal sealed class UsageFetchService : IDisposable
    {
        private readonly SemaphoreSlim gate = new SemaphoreSlim(1, 1);
        private readonly object cancellationSync = new object();
        private readonly IUsageFetchSessionFactory factory;
        private CancellationTokenSource activeCancellation;
        private bool activeCancellationRequested;
        private bool stopping;
        private bool disposed;

        public UsageFetchService(IUsageFetchSessionFactory factory = null)
        {
            this.factory = factory ?? new UsageFetchSessionFactory();
        }

        public bool IsBusy { get { return gate.CurrentCount == 0; } }

        public async Task<FetchOutcome> FetchAsync(UsageSource source, ProxySettings proxy, CancellationToken applicationToken)
        {
            if (stopping || disposed)
            {
                return FetchOutcome.Create(FetchStatus.Cancelled, "The application is stopping.");
            }
            if (!await gate.WaitAsync(0))
            {
                SafeLogger.Write("Refresh.Skipped", "another refresh is active");
                return FetchOutcome.Create(FetchStatus.Busy, "Another refresh is already active.");
            }

            var stopwatch = Stopwatch.StartNew();
            try
            {
                lock (cancellationSync)
                {
                    activeCancellationRequested = false;
                    activeCancellation = CancellationTokenSource.CreateLinkedTokenSource(applicationToken);
                    activeCancellation.CancelAfter(TimeSpan.FromSeconds(AppConstants.FetchTimeoutSeconds));
                }
                var sourceDetail = source == UsageSource.WebView2
                    ? "source=" + source + " proxy=" + (proxy == null ? ProxyMode.System : proxy.mode)
                    : "source=" + source;
                SafeLogger.Write("Refresh.Start", sourceDetail);
                FetchOutcome outcome;
                using (var session = factory.Create(source, proxy == null ? null : proxy.Clone()))
                {
                    outcome = await session.RunAsync(activeCancellation.Token);
                }
                stopwatch.Stop();
                outcome.Duration = stopwatch.Elapsed;
                SafeLogger.Write(
                    outcome.IsSuccess ? "Refresh.Complete" : "Refresh.Failed",
                    "status=" + outcome.Status + " durationMs=" + (long)outcome.Duration.TotalMilliseconds);
                return outcome;
            }
            catch (OperationCanceledException)
            {
                stopwatch.Stop();
                bool explicitlyCancelled;
                lock (cancellationSync)
                {
                    explicitlyCancelled = activeCancellationRequested;
                }
                var status = stopping || applicationToken.IsCancellationRequested || explicitlyCancelled
                    ? FetchStatus.Cancelled
                    : FetchStatus.Timeout;
                SafeLogger.Write(status == FetchStatus.Timeout ? "Refresh.Timeout" : "Refresh.Cancelled", "durationMs=" + (long)stopwatch.Elapsed.TotalMilliseconds);
                return FetchOutcome.Create(status, status == FetchStatus.Timeout ? "The usage fetch timed out." : "The usage fetch was cancelled.");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                SafeLogger.Write("Refresh.Exception", ex.GetType().Name + ": " + ex.Message);
                return FetchOutcome.Create(FetchStatus.UnexpectedError, "The usage fetch failed unexpectedly.");
            }
            finally
            {
                CancellationTokenSource cancellation;
                lock (cancellationSync)
                {
                    cancellation = activeCancellation;
                    activeCancellation = null;
                    activeCancellationRequested = false;
                }
                if (cancellation != null) cancellation.Dispose();
                gate.Release();
                if (disposed) gate.Dispose();
            }
        }

        public bool CancelActive(string reason)
        {
            lock (cancellationSync)
            {
                if (activeCancellation == null)
                {
                    return false;
                }
                activeCancellationRequested = true;
                try
                {
                    activeCancellation.Cancel();
                }
                catch (ObjectDisposedException)
                {
                    return false;
                }
            }
            SafeLogger.Write("Refresh.CancelRequested", "reason=" + (string.IsNullOrWhiteSpace(reason) ? "interactive" : reason));
            return true;
        }

        public void Stop()
        {
            stopping = true;
            CancelActive("shutdown");
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            Stop();
            if (gate.CurrentCount > 0) gate.Dispose();
        }
    }
}
