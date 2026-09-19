# Third-party notices

## Project origins

- [saveway/codex-usage-monitor](https://github.com/saveway/codex-usage-monitor): original native WebView2 baseline, tag `v2.0.0-preview.7`, commit `36e9679164dcd7e5ef23d1f35822664785fad01f`. Copyright (c) 2026 Codex Usage Monitor contributors. [Retained MIT license](third-party/codex-usage-monitor-MIT.txt).
- [ognjeeen/codex-usage-widget](https://github.com/ognjeeen/codex-usage-widget): acknowledged as a reference project. Copyright (c) 2026 Ognjen Marinković. [Retained MIT license](third-party/codex-usage-widget-MIT.txt).

These upstream notices are retained; this project does not claim ownership of upstream contributions. Its Windows tray-only design, bounded collection lifecycle, dual-source support, localization, and quota rendering are described in the README. The upstream programs' old releases and Git histories are not included.

## Microsoft WebView2 SDK

The application references `Microsoft.Web.WebView2` version `1.0.4022.49` and distributes its Core assembly and x64 loader. Retained package files:

- [WebView2 license](third-party/WebView2-LICENSE.txt)
- [WebView2 third-party notices](third-party/WebView2-NOTICE.txt)

The separately installed Microsoft Edge WebView2 Runtime is not bundled. Microsoft .NET Framework is also a system prerequisite, not a bundled runtime. The optional Codex CLI and its runtime are not bundled.
