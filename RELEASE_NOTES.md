# What's new in 0.3.1

- Local candidates now use `-local` in their ZIP filenames. Authorized GitHub packages are generated separately with reviewed bilingual documentation; historical changelog headings now distinguish published releases from local iterations.

- Every successful refresh now writes one structured `Refresh.Success` line containing the selected source, masked email, user level, 5-Hour and Weekly values/reset times, available usage resets, fetched time, and duration. Missing values are explicit, and an unmasked email is never written.
- Added an English-only command-line interface for help/version, the last saved status, refresh/exit control of the running tray instance, and local log/config paths.
- `-refresh` and `-exit`/`-quit` use two lightweight session-local signals. They do not create a second tray, WebView2 controller, or Codex CLI fetch process.
- Preserved the existing internal `--startup` argument so current-user Start at login continues to enter the normal tray-start path.
- Refined direct interactive Command Prompt and PowerShell output: an unchanged prematurely rendered prompt is cleared in place; CMD's preceding spacer row is reused only when the complete visible row is blank, so the result begins immediately below the command; one empty line separates it from the restored prompt, and no key is injected.
- If prompt text or cursor position changes, or the console form is unsupported, the safe v0.2.30 newline layout is retained automatically. Normal tray and Start at login launches still avoid a console flash.
- Added command parsing, saved-status, privacy, named-channel, compiled executable output/exit-code, prompt text/cursor/output-row safety, and startup-argument compatibility coverage. The v0.3.1 build passes all 97 automated tests.

**Install:** download the Windows x64 ZIP and matching checksum, verify the hash, extract all files into a permanent folder, and run `CodexUsageTrayLite.exe`. Requires Windows 11 x64 and .NET Framework 4.8 or later. Install WebView2 Evergreen Runtime for WebView2 mode, or use an installed Codex CLI already signed in with ChatGPT for CLI mode.

**Upgrade:** choose Exit before replacing all application files. Keep the same folder when start at login is enabled, or re-enable that setting after moving. Settings and WebView2 sessions under `%LOCALAPPDATA%\CodexUsageTrayLite` are retained.
