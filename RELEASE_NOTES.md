# What's new in 0.3.3

- Includes the updated Usage Overview support from the intervening local iteration: parse English reset countdowns and exclude historical/credit percentages from current quotas. Countdown-derived reset times are estimates.
- Formal packages now reuse tested local-candidate binaries without rebuilding; the EXE hash remains identical while bilingual publication documents and checksums are refreshed.

- Improved intermittent WebView2 failures on the updated Usage page: poll independently of navigation completion, verify visible quota data, and confirm Weekly-only data while a 5-hour card may still be loading.
- Broadened safe Overview/Analytics group detection and added spaced, bounded re-selection when a click has no effect. Readable quota data prevents further clicking.
- Adds one recovery navigation for a stalled page or navigation/script failure, within the existing 45-second total deadline. Pending creation and page waits remain cancellable.
- Added HTTP/WebView navigation status, document/tab state, fetch stage, recovery, and timeout diagnostics without logging page text, full URLs, cookies, or tokens.
- Keeps background hosts hidden/non-activating and closes controllers normally. Reset consumption and purchase actions are never invoked. CLI behavior is unchanged.

Validation: 103 automated tests, real-WebView2 offline scenarios including the production recovery/cancellation path, and 20 consecutive authenticated local hidden reads. Other-machine and long-duration validation remains distinct from local tests.

**Install:** download the Windows x64 ZIP and matching checksum, verify the hash, extract all files into a permanent folder, and run `CodexUsageTrayLite.exe`. Requires Windows 11 x64 and .NET Framework 4.8 or later. Install WebView2 Evergreen Runtime for WebView2 mode, or use an installed Codex CLI already signed in with ChatGPT for CLI mode.

**Upgrade:** choose Exit before replacing all application files. Keep the same folder when start at login is enabled, or re-enable that setting after moving. Settings and WebView2 sessions under `%LOCALAPPDATA%\CodexUsageTrayLite` are retained.
