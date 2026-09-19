# What's new

- First public release of Codex Usage Tray Lite, starting at v0.2.16. Earlier local versions and Git history are not published.
- Published under GPL-3.0-only, with the original MIT attributions and bundled dependency notices retained.
- Includes WebView2 and Codex CLI usage sources, four tray icon styles, Light/Dark/System themes, and five UI languages.
- Side by Side now fills the entire inner frame with Weekly when the account authoritatively has no applicable 5-Hour quota. Accounts with both quotas retain Weekly on the left and 5-Hour on the right; ambiguous data remains Unknown.
- Weekly keeps its own green/orange/red thresholds. The Weekly-only fallback has a neutral outer frame; tooltip behavior, saved preferences, and refresh behavior are unchanged.
- The local v0.2.16 build passed 88 automated tests, including fallback pixel checks at 0/1/20/21/50/51/100%, 16/20/24/32 px, and Light/Dark themes. These are automated checks, not a complete live-browser or long-duration acceptance test.

**Install:** download the Windows x64 ZIP and matching checksum, verify the hash, extract all files into a permanent folder, and run `CodexUsageTrayLite.exe`. Requires Windows 11 x64 and .NET Framework 4.8 or later. Install WebView2 Evergreen Runtime for WebView2 mode, or use an installed Codex CLI already signed in with ChatGPT for CLI mode.

**Upgrade from a local build:** choose Exit before replacing all application files. Keep the same folder when start at login is enabled, or re-enable that setting after moving. Settings and WebView2 sessions under `%LOCALAPPDATA%\CodexUsageTrayLite` are retained.
