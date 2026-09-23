# What's new in 0.2.18

- New local-candidate and GitHub-release archives now share the permanent name `CodexUsageTrayLite-v<version>-win-x64.zip`, without a `local` suffix. Existing historical archives are not renamed or overwritten.
- The portable ZIP now includes `README.txt` and a complete Simplified Chinese `README.zh-CN.txt`.
- The bilingual changelog remains complete inside the ZIP: versions through `0.2.15` retain their historical `-local` suffix, while `0.2.16` and later use public version numbers.
- The packaged source link now follows the current version automatically. The packaging gate rejects stale release headings, missing history cutover markers, unresolved template tokens, incorrect source tags, and new archive names containing `local`.
- Application behavior is unchanged from `0.2.17`.

## Included application changes from 0.2.17

- The tray menu is reduced to 15 top-level rows in both WebView2 and Codex CLI modes. Email and account level share the first row; the quota and update rows remain individually visible.
- WebView2 proxy/session maintenance now lives under Usage source and disappears cleanly in CLI mode. Logs, config folder, Help, and About are grouped under Tools & Help.
- Usage source, Refresh interval, Icon style, Theme, and Language show their current value on the right. The menu remains two levels deep and keeps Refresh and login/help actions directly available.
- Primary read-only usage rows are high contrast in both themes, while secondary reset/update rows remain muted. Long email and 150% font rendering are covered by the production WinForms render check.
- `Both (Default)` is now displayed as `Stacked (Default)` across all five UI languages. This is a label-only change; saved settings and icon rendering are unchanged.
- The v0.2.18 build repeats the 89 automated application tests. English and Simplified Chinese Light/Dark menu renders, source/tool/style submenus, CLI visibility, keyboard-compatible native menu structure, and long-text layout were also inspected for the unchanged application behavior. The renderer snapshots are not a live account refresh or long-duration test.

**Install:** download the Windows x64 ZIP and matching checksum, verify the hash, extract all files into a permanent folder, and run `CodexUsageTrayLite.exe`. Requires Windows 11 x64 and .NET Framework 4.8 or later. Install WebView2 Evergreen Runtime for WebView2 mode, or use an installed Codex CLI already signed in with ChatGPT for CLI mode.

**Upgrade:** choose Exit before replacing all application files. Keep the same folder when start at login is enabled, or re-enable that setting after moving. Settings and WebView2 sessions under `%LOCALAPPDATA%\CodexUsageTrayLite` are retained.
