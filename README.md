<div align="center">
  <picture>
    <source media="(prefers-color-scheme: dark)" srcset="docs/images/logo-dark.png">
    <source media="(prefers-color-scheme: light)" srcset="docs/images/logo-light.png">
    <img src="docs/images/logo-light.png" alt="Codex Usage Tray Lite logo" width="96">
  </picture>
  <h1>Codex Usage Tray Lite</h1>
  <p>A lightweight Windows tray companion for your remaining Codex usage.</p>
  <p>
    <a href="https://github.com/zjwww/Codex-Usage-Tray-Lite/releases/latest"><img src="https://img.shields.io/github/v/release/zjwww/Codex-Usage-Tray-Lite?sort=semver&amp;style=flat" alt="Latest release"></a>
    <img src="https://img.shields.io/badge/platform-Windows%2011%20x64-0078D4?style=flat" alt="Windows 11 x64">
    <img src="https://img.shields.io/badge/.NET%20Framework-4.8-512BD4?style=flat" alt=".NET Framework 4.8">
    <a href="LICENSE"><img src="https://img.shields.io/github/license/zjwww/Codex-Usage-Tray-Lite?style=flat" alt="License"></a>
  </p>
  <p><a href="README.md">English</a> | <a href="README.zh-CN.md">简体中文</a> | <a href="https://github.com/zjwww/Codex-Usage-Tray-Lite/releases/latest">Download latest</a> | <a href="https://github.com/zjwww/Codex-Usage-Tray-Lite/issues">Report an issue</a></p>
</div>

## Overview

Codex Usage Tray Lite is an unofficial portable Windows notification-area utility. It displays remaining Weekly usage and, when applicable, 5-Hour usage through a dynamic tray icon, a compact tooltip, and a context menu.

It is not developed, endorsed, or supported by OpenAI. OpenAI, ChatGPT, and Codex belong to their respective trademark owners.

## Main features

- **Two usage sources:** sign in through an app-specific WebView2 browser, or use an installed Codex CLI already signed in with ChatGPT.
- **Four tray styles:** 5-Hour Only, Weekly Only, Stacked (Default), and Side by Side. Weekly fills the complete frame when the account authoritatively has no 5-Hour quota in Stacked or Side by Side.
- **Clear states:** distinguish remaining quota, unknown data, login required, fetch errors, and an inapplicable 5-Hour window. Missing values are never invented as zero.
- **Account details:** email, supported account level, quotas, available usage resets, and last update. The app only reads reset availability; it never consumes a reset.
- **Refresh controls:** manual refresh and intervals of 1, 2, 5, 10, 15, 30, or 60 minutes; the default is 15 minutes.
- **Appearance:** follow Windows, Light, or Dark; English, Simplified Chinese, Traditional Chinese, Japanese, and Korean UI. Compact tooltips remain in English.
- **Local preferences:** start at login, open logs/config folder, and WebView2 system/direct/HTTP/SOCKS5 proxy settings. Proxy authentication is not supported.
- **Short-lived collection:** WebView2 controllers and CLI processes are disposed after collection. No desktop widget, telemetry, developer-operated backend, installer, or automatic updater.

## Preview

These images are renderings using sample values, not screenshots of a live account. Tray previews use the application renderer at 16/20/24/32 px, enlarged for inspection.

### All tray styles and states

![All four tray styles with light/dark variants and special states](docs/images/quota-styles.png)

Weekly uses green above 50%, orange at 21–50%, and red at 0–20%. In dual-quota modes, the 5-Hour value independently controls its warning frame. `i` means the selected 5-Hour quota is not applicable, `?` means unknown, `/` means fetch error, and `L` means login required.

### Side by Side and Weekly-only accounts

![Side by Side quota combinations and the full-width Weekly fallback](docs/images/side-by-side.png)

### Interactive menu preview

<a href="https://zjwww.github.io/Codex-Usage-Tray-Lite/en.html">
  <picture>
    <source media="(prefers-color-scheme: dark)" srcset="docs/images/menu-demo-en-dark.gif">
    <source media="(prefers-color-scheme: light)" srcset="docs/images/menu-demo-en-light.gif">
    <img src="docs/images/menu-demo-en-light.gif" alt="Animated English menu demo: usage source, refresh interval, icon style, and Tools &amp; Help" width="760">
  </picture>
</a>

**[Try the interactive menu demo](https://zjwww.github.io/Codex-Usage-Tray-Lite/en.html)** · [Light animation](docs/images/menu-demo-en-light.gif) · [Dark animation](docs/images/menu-demo-en-dark.gif)

The animation and linked page are browser simulations of the v0.2.26 menu, using the application's English labels and sample data. The preview follows your light/dark preference. In the interactive demo, open submenus, switch usage sources, and change the interval or theme; nothing connects to an account or changes your computer settings. Native Windows fonts, shadows, and positioning may differ. The demo offers English and Simplified Chinese; the desktop app supports five languages.

In CLI mode, the login action becomes **Codex CLI login help**, and WebView2 proxy/session actions are hidden. [View the native-control Light rendering](docs/images/menu-english-light.png) or [Dark rendering](docs/images/menu-english-dark.png).

Under **Tools & Help**, **Update** opens the latest GitHub Release in the default browser and **About** shows the running application version in the right-side summary.

### Existing application icon

![Existing application identity icon in both variants and all sizes](docs/images/app-icon-sizes.png)

The static application icon is used by the executable and dialogs; the tray uses the dynamic quota styles above.

## Requirements

| Component | Requirement |
| --- | --- |
| Operating system | Windows 11 x64 (target environment) |
| Application runtime | .NET Framework 4.8 or later; not bundled |
| WebView2 source | Microsoft Edge WebView2 Evergreen Runtime and a ChatGPT login; runtime not bundled |
| Codex CLI source | Codex CLI on `PATH`, already signed in with ChatGPT; CLI and its runtime are not bundled |
| Connectivity | Access to the services required by the selected source |

Windows 10 and Windows on ARM are not validated targets. Python, Playwright, and a bundled Chromium browser are not required by this application.

## Install and start

1. Download the Windows x64 ZIP and matching `.zip.sha256` from the [latest Release](https://github.com/zjwww/Codex-Usage-Tray-Lite/releases/latest).
2. Compare the SHA-256 hash before extracting:

   ```powershell
   Get-FileHash .\CodexUsageTrayLite-v0.3.1-win-x64.zip -Algorithm SHA256
   Get-Content .\CodexUsageTrayLite-v0.3.1-win-x64.zip.sha256
   ```

3. Extract the **entire ZIP** into a permanent folder. Keep the DLLs and language folders beside the executable. Do not run from inside the ZIP.
4. Run `CodexUsageTrayLite.exe` and find its icon in the Windows notification area, including the overflow area.
5. For WebView2, choose **Open login / usage page**, sign in directly to ChatGPT, then close that window. For CLI, select **Usage source → Codex CLI** after signing in through the CLI.

A fresh installation waits for source setup instead of fetching automatically. Selecting a source validates it immediately; periodic refresh begins after a successful read. **Refresh now** retries the configured source. **Start at login** is optional.

The executable is not code-signed. Verify the download and its source before running it.

## Command line

Command-line text is English only. With no option, the executable starts the normal notification-area application. Use one option per invocation:

| Option | Action |
| --- | --- |
| `-help`, `-h`, `-?` | Show command-line help. |
| `-version`, `-v` | Print the application version. |
| `-status` | Print the last saved usage snapshot without refreshing or accessing the network. |
| `-refresh` | Ask the already-running tray instance to refresh now. |
| `-exit`, `-quit` | Ask the already-running tray instance to exit cleanly. |
| `-log-path` | Print the current log-file path. |
| `-config-path` | Print the local application data/configuration directory. |

`-refresh` and `-exit`/`-quit` require a responsive tray instance in the same Windows session; they return exit code 2 otherwise. Invalid syntax returns 64. The program remains a Windows GUI-subsystem executable to avoid a console flash during ordinary startup. In direct interactive Command Prompt and PowerShell use, an unchanged prematurely rendered prompt is cleared in place. If CMD left a completely blank visible spacer row immediately above it, output safely reuses that row so the result directly follows the command; one empty line separates the result from the restored prompt. No Enter or other key is injected, and changed/unsupported prompts use a safe newline fallback. Scripts must still wait for this GUI process explicitly when command ordering matters, for example with `Start-Process .\CodexUsageTrayLite.exe -ArgumentList '-version' -NoNewWindow -Wait`. Redirected process APIs receive stdout/stderr and exit codes normally. The internal `--startup` argument used by Start at login is safe to run manually but is equivalent to a normal no-option tray launch.

## Upgrade and removal

To upgrade, choose **Exit**, extract the new complete package, and replace the application files while it is stopped. Keep the same installation path if start at login is enabled, or disable/re-enable that option after moving the app. Settings and the WebView2 profile live outside the application folder and are retained.

To remove, disable **Start at login**, choose **Exit**, and delete the application folder. To also remove settings, logs, and saved WebView2 sessions, delete `%LOCALAPPDATA%\CodexUsageTrayLite` after exit.

## Privacy and troubleshooting

- Local data is stored under `%LOCALAPPDATA%\CodexUsageTrayLite`. Never publish its `webview2-profile` directory: it can contain an authenticated session.
- Account email is kept in memory and shown in the menu. Logs use a masked email. Each successful refresh records one structured quota snapshot with source, supported account level, 5-Hour/Weekly values and reset times, available reset count, fetched time, and duration. The app does not read Codex CLI credential files or another browser's profile.
- WebView2 owns its saved cookies/session; CLI owns its own authentication. No developer-operated server receives your data.
- Use **Open logs** for diagnostics. Review logs before attaching them to an issue. Daily log archives are not automatically removed.
- If WebView2 login expires, reopen the login page. A missing runtime must be installed separately. CLI mode needs a working, signed-in CLI on `PATH`.
- WebView2 collection depends on the website layout; CLI collection depends on the installed app-server protocol. Upstream changes can break either source.

## Build and validation

Build on Windows with a .NET SDK capable of building the .NET Framework 4.8 project. NuGet restores the framework reference assemblies and WebView2 SDK.

```powershell
dotnet restore .\CodexUsageTrayLite.sln --configfile .\NuGet.Config
dotnet build .\CodexUsageTrayLite.sln -c Release -p:Platform=x64 --no-restore
& .\tests\CodexUsageTrayLite.Tests\bin\x64\Release\net48\CodexUsageTrayLite.Tests.exe
```

The v0.3.1 build passed **97 automated tests**. This covers parsers, settings, localization, renderer pixels, command-line output/exit codes, prompt text/cursor/output-row safety and startup compatibility, session-local command signaling, menu-region lifecycle, process cleanup, and simulated resource lifecycles; it is not proof of every authenticated browser flow, proxy setup, every custom/multiline/color shell prompt, live notification-area placement, hardware DPI, or an 8–12 hour soak. See [validation details](TESTING.md).

## Version history and publication

See the complete [changelog](CHANGELOG.md). Version headings follow the actual [GitHub Release history](https://github.com/zjwww/Codex-Usage-Tray-Lite/releases): public versions use `x.y.z`; local iterations that were never published use `x.y.z-local`. A higher local version does not imply that a public download exists.

Before every GitHub publication, update both READMEs and reconcile every heading in both changelogs with actual Release history, then finalize package documentation. The explicitly authorized target may use its public heading during preparation. Application version fields remain numeric; filenames distinguish package channels:

- Local iteration: `CodexUsageTrayLite-v<version>-local-win-x64.zip`, created with `scripts/package-release.ps1`.
- Authorized GitHub release: `CodexUsageTrayLite-v<version>-win-x64.zip`, created separately with `scripts/package-release.ps1 -GitHubRelease` after documentation review.

Both channels include bilingual package guides, complete changelogs, release notes, and checksums. Retain the local candidate when creating its formal counterpart; never overwrite an existing package in either channel. Historical archives keep their names, except for the authorized one-time rename of the v0.3.1 candidate to the local convention.

## License and acknowledgments

Licensed under **GPL-3.0-only**. When distributing a modified version, provide its corresponding source under the GPL. Private modifications do not require publication. See [LICENSE](LICENSE) and [third-party notices](THIRD_PARTY_NOTICES.md); upstream MIT notices are retained.

- [saveway/codex-usage-monitor](https://github.com/saveway/codex-usage-monitor) — the MIT-licensed native WebView2 preview was the original baseline (`v2.0.0-preview.7`, commit `36e9679164dcd7e5ef23d1f35822664785fad01f`).
- [ognjeeen/codex-usage-widget](https://github.com/ognjeeen/codex-usage-widget) — an MIT-licensed reference project.
