using System;
using System.IO;
using System.Threading;

namespace CodexUsageTrayLite.Infrastructure
{
    internal enum SingleInstanceCommand
    {
        Refresh,
        Exit
    }

    internal sealed class SingleInstanceCommandChannel : IDisposable
    {
        internal const string ProductionChannelId = "CodexUsageTrayLite.Command.v1";
        private readonly EventWaitHandle refreshEvent;
        private readonly EventWaitHandle exitEvent;
        private readonly RegisteredWaitHandle refreshRegistration;
        private readonly RegisteredWaitHandle exitRegistration;
        private readonly Action<SingleInstanceCommand> commandHandler;
        private int disposed;

        public SingleInstanceCommandChannel(Action<SingleInstanceCommand> commandHandler, string channelId = null)
        {
            if (commandHandler == null) throw new ArgumentNullException("commandHandler");
            this.commandHandler = commandHandler;
            var id = NormalizeChannelId(channelId);
            bool created;
            refreshEvent = new EventWaitHandle(false, EventResetMode.AutoReset, EventName(id, SingleInstanceCommand.Refresh), out created);
            exitEvent = new EventWaitHandle(false, EventResetMode.AutoReset, EventName(id, SingleInstanceCommand.Exit), out created);
            refreshRegistration = ThreadPool.RegisterWaitForSingleObject(
                refreshEvent, HandleSignal, SingleInstanceCommand.Refresh, Timeout.Infinite, false);
            exitRegistration = ThreadPool.RegisterWaitForSingleObject(
                exitEvent, HandleSignal, SingleInstanceCommand.Exit, Timeout.Infinite, false);
        }

        public static bool TrySend(SingleInstanceCommand command, TimeSpan timeout, string channelId = null)
        {
            if (timeout < TimeSpan.Zero) throw new ArgumentOutOfRangeException("timeout");
            var id = NormalizeChannelId(channelId);
            var deadline = DateTime.UtcNow + timeout;
            do
            {
                try
                {
                    using (var signal = EventWaitHandle.OpenExisting(EventName(id, command)))
                    {
                        return signal.Set();
                    }
                }
                catch (WaitHandleCannotBeOpenedException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                    return false;
                }
                catch (IOException)
                {
                }

                if (DateTime.UtcNow >= deadline) return false;
                Thread.Sleep(40);
            }
            while (true);
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref disposed, 1) != 0) return;
            refreshRegistration.Unregister(null);
            exitRegistration.Unregister(null);
            refreshEvent.Dispose();
            exitEvent.Dispose();
        }

        private void HandleSignal(object state, bool timedOut)
        {
            if (timedOut || Volatile.Read(ref disposed) != 0) return;
            commandHandler((SingleInstanceCommand)state);
        }

        private static string NormalizeChannelId(string channelId)
        {
            var value = string.IsNullOrWhiteSpace(channelId) ? ProductionChannelId : channelId.Trim();
            if (value.IndexOfAny(new[] { '\\', '/', ':' }) >= 0)
                throw new ArgumentException("The command channel identifier contains an invalid character.", "channelId");
            return value;
        }

        private static string EventName(string channelId, SingleInstanceCommand command)
        {
            return @"Local\" + channelId + "." + command;
        }
    }
}
