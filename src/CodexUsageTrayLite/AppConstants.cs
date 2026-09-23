namespace CodexUsageTrayLite
{
    internal static class AppConstants
    {
        public const string Name = "Codex Usage Tray Lite";
        public const string AssemblyName = "CodexUsageTrayLite";
        public const string Version = "0.2.18";
        public const string BaselineTag = "v2.0.0-preview.7";
        public const string BaselineCommit = "36e9679164dcd7e5ef23d1f35822664785fad01f";
        public const string UpstreamUrl = "https://github.com/saveway/codex-usage-monitor";
        public const string UsageUrl = "https://chatgpt.com/codex/cloud/settings/analytics#usage";
        public const string RunValueName = "CodexUsageTrayLite";
        public const int DefaultRefreshMinutes = 15;
        public const int FetchTimeoutSeconds = 45;
        public const int NotifyIconTextLimit = 63;
        public const int CodexCliMaxJsonLength = 1024 * 1024;
        public const int CodexCliStopGraceMilliseconds = 3000;
        public const int CodexCliJobDrainGraceMilliseconds = 500;
        public const int CodexCliJobDrainPollMilliseconds = 50;
    }
}
