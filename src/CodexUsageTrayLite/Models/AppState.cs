using System;

namespace CodexUsageTrayLite.Models
{
    internal enum TrayVisualState
    {
        Normal,
        Unknown,
        FetchError,
        LoginRequired
    }

    internal enum FetchStatus
    {
        Success,
        Busy,
        LoginRequired,
        NetworkError,
        ParseError,
        RuntimeMissing,
        Timeout,
        Cancelled,
        UnexpectedError
    }

    internal sealed class FetchOutcome
    {
        public FetchStatus Status { get; private set; }
        public UsageSnapshot Snapshot { get; private set; }
        public string Message { get; private set; }
        public TimeSpan Duration { get; internal set; }
        public bool AccountEmailRead { get; private set; }
        public string AccountEmail { get; private set; }

        public bool IsSuccess { get { return Status == FetchStatus.Success && Snapshot != null; } }

        public static FetchOutcome Create(FetchStatus status, string message, UsageSnapshot snapshot = null)
        {
            return new FetchOutcome { Status = status, Message = message, Snapshot = snapshot };
        }

        public FetchOutcome WithAccountEmail(string email)
        {
            AccountEmailRead = true;
            AccountEmail = email;
            return this;
        }
    }
}
