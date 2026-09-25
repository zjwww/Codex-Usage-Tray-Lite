# Codex Usage Tray Lite Changelog

Simplified Chinese edition: [CHANGELOG.zh-CN.md](CHANGELOG.zh-CN.md)

This file records the functional changes from the local development release `0.1.0-local` through the current version. It is based on actual release handoffs from this development session, the project specification, test documentation, source code, and retained versioned ZIP archives.

Version headings follow the actual [GitHub Release history](https://github.com/zjwww/Codex-Usage-Tray-Lite/releases): published versions use `x.y.z`; local iterations that were never published use `x.y.z-local`. Before every GitHub publication, reconcile every heading in both changelogs and update both READMEs against that history. The explicitly authorized target may use its public heading during preparation; this does not mean it has already been published. `0.3.1` is the current publication target; the verified previous public releases are `0.2.16`, `0.2.18`, and `0.2.26`.

Starting with `0.1.1-local`, every user-visible update receives a new version number and a separate immutable ZIP. Application version fields remain numeric. From this publication onward, local packages use `CodexUsageTrayLite-v<version>-local-win-x64.zip`; authorized GitHub packages are created separately as `CodexUsageTrayLite-v<version>-win-x64.zip` after documentation reconciliation. Local candidates are retained. Historical archives remain unchanged except for the authorized one-time rename of the v0.3.1 local candidate; its ZIP bytes and hash are preserved.

## 0.3.1 - 2026-09-25

### Release preparation

- Separated local and GitHub package filenames and reconciled historical version headings against actual GitHub Releases. Both channels include bilingual guides, complete changelogs, and release notes; the formal package contains the reviewed publication documents.

### Fixed

- Removed the extra empty line that Command Prompt can place above command output when launching the GUI-subsystem executable. After validating and clearing the unchanged premature prompt, the app now reuses CMD's immediately preceding spacer row only when that complete visible console row is still blank.
- Kept the requested single empty line below command output and the exact restored prompt. A nonblank, unavailable, or offscreen preceding row is never overwritten; all existing prompt-text/cursor checks and the no-key-injection rule remain in force.
- Added output-row safety assertions and verified the final layout in a real interactive CMD session, including a successful follow-up command at the restored prompt. All 97 automated tests pass.

## 0.3.0-local - 2026-09-25

### Changed

- Refined direct interactive Command Prompt and PowerShell output into the conventional layout: the shell's unchanged prematurely rendered prompt is cleared in place, command output begins on that line, one empty line follows the result, and the exact visible prompt is then restored.
- Before clearing, the app rechecks the prompt text, cursor column, and row. Typed input, a moved cursor, unsupported/multiline content, redirection, or any console API failure retains the safe v0.2.30 newline fallback. The implementation never injects Enter or other keyboard input.
- Added prompt text/cursor/row safety assertions and a real interactive CMD validation followed by another command at the restored prompt. All 97 automated tests pass.

## 0.2.30-local - 2026-09-25

### Fixed

- Fixed interactive Command Prompt and PowerShell output from the GUI-subsystem EXE leaving the shell without a visible trailing prompt until Enter was pressed. The app now reads the already-rendered single-line shell prompt, moves command output below it, and restores the same visible prompt after output completes.
- Prompt restoration never sends Enter, injects keystrokes, changes the shell prompt, or converts the app to a console-subsystem executable, so normal tray and Start at login launches still avoid a console flash. Redirected/scripted output remains unchanged and should still use normal process waiting when command ordering matters.
- Added prompt-eligibility regression coverage while preserving the internal `--startup` tray-launch path. All 97 automated tests pass.

## 0.2.29-local - 2026-09-25

### Fixed

- Preserved the existing internal `--startup` launch argument used by the current-user Start at login registry entry. It now follows the normal tray-start path instead of being rejected by the new command-line parser.
- Added a regression assertion binding `StartupManager.BuildCommand(...)` to the parser's startup behavior. All 97 automated tests pass.

## 0.2.28-local - 2026-09-25

### Added

- Added one structured `Refresh.Success` record after every successful refresh. The single line contains the source, masked email, user level, 5-Hour and Weekly remaining values/reset times, available usage-reset count, fetched timestamp, and refresh duration; unavailable values are explicit `Unknown` or `N/A`.
- Added an English-only command-line interface: `-help`, `-version`, `-status`, `-refresh`, `-exit`/`-quit`, `-log-path`, and `-config-path`, with `-h`, `-?`, `-v`, `-logs-path`, and `-data-path` aliases. One option is accepted per invocation, and invalid syntax returns exit code 64.
- Added lightweight session-local command signaling so `-refresh` and `-exit`/`-quit` control the already-running tray instance without launching a second WebView2/CLI collection. Missing or unresponsive instances return exit code 2.

### Changed

- `-status` prints the last saved usage snapshot without network access; path commands only print local paths. Starting the EXE without an option keeps the existing notification-area behavior.
- Added parser, saved-status, privacy, named-command-channel, and compiled executable stdout/stderr/exit-code coverage. All 97 automated tests pass.

## 0.2.27-local - 2026-09-25

### Fixed

- Prevented primary and secondary status rows from being drawn twice by ignoring the empty native shortcut-column text callback; Weekly, 5-Hour, Usage resets, and Last updated now retain their intended font weight.
- Applied the same DPI-scaled rounded geometry to the actual main-menu and submenu window regions, backgrounds, and borders, removing rectangular corner protrusions in Light and Dark modes.
- Preserved native summary-column width reservation, zero-gap submenu placement, keyboard navigation, the existing palette hierarchy, and square system rendering in Windows High Contrast mode.
- Added single-pass pixel regression, main/submenu region and resize coverage, and repeated region-lifecycle checks. All 93 automated tests pass.

## 0.2.26 - 2026-09-24

### Fixed

- Corrected the About menu's version prefix from `v.` to `v`; this release displays `v0.2.26`.
- Preserved the smaller right-aligned summary styling and all v0.2.24 Update-menu behavior. All 91 automated tests pass.

## 0.2.25-local - 2026-09-24

### Changed

- Prefixed the About menu's right-side version summary with `v.`; for this release it displays `v.0.2.25`.
- Kept the existing smaller summary font, right alignment, submenu geometry, Update command, and all functional behavior unchanged. All 91 automated tests pass.

## 0.2.24-local - 2026-09-24

### Added

- Added **Update** between Help and About in the Tools & Help submenu. Its right-side summary reads `GitHub`, and clicking it opens the project's latest GitHub Release page in the Windows default browser.
- Added localized Update labels and browser-open failure messages for English, Simplified Chinese, Traditional Chinese, Japanese, and Korean.

### Changed

- About now displays the current application version in the same smaller right-aligned summary style used elsewhere in the menu.
- Added HTTPS URL-launch validation, structured update-page launch logging, menu-order/summary regression coverage, localization coverage, and production submenu renders. All 91 automated tests pass.

## 0.2.23-local - 2026-09-24

### Fixed

- Aligned the left edge of custom account, quota, reset, update, and setting-summary labels with ordinary command rows in the main tray menu.
- Custom rows now retain the native Windows `TextRenderer` glyph padding used by standard `ToolStripMenuItem` text instead of starting approximately five pixels farther left.
- Preserved the v0.2.22 native item bounds and zero-gap submenu placement; right-aligned setting summaries, menu hierarchy, commands, and behavior are unchanged.
- Added a renderer contract regression and production WinForms renders for English/Simplified Chinese, Light/Dark, CLI/WebView2, and long-email font scaling. All 90 automated tests pass.

## 0.2.22-local - 2026-09-24

### Fixed

- Removed the remaining approximately 11-pixel gap between the parent tray menu and its submenus.
- Removed per-item horizontal margin and padding that increased the menu-item bounds without increasing the visible `ContextMenuStrip` window. Visual breathing room remains provided by the renderer's text geometry and inset selection background, so the R2 appearance is retained without affecting submenu coordinates.
- Strengthened the regression baseline to use a truly unmodified WinForms submenu. All five summary-bearing parent items must now match its native edge behavior, including the normal one-pixel border overlap, in both normal-email and long-email layouts.

## 0.2.21-local - 2026-09-23

### Partial fix

- Fixed the actual cause of detached submenus: summary-bearing parent items could become wider than the visible `ContextMenuStrip` because their custom preferred width was not included in `ToolStripDropDownMenu`'s native text metrics.
- Current-setting text now reserves width through WinForms' native secondary-text layout metric but remains custom-drawn once, at the approved smaller size and secondary color. The invisible excess item width is removed, so native submenu placement starts at the parent menu edge.
- Removed the ineffective v0.2.20 direction override and restored WinForms' own screen-collision and left/right cascade behavior.
- Target-machine verification showed that the large gap was reduced but an approximately 11-pixel gap remained. The first baseline still included the application's custom horizontal item spacing; `0.2.22` corrects that baseline and removes the remaining spacing from layout bounds.

## 0.2.20-local - 2026-09-23

### Attempted fix

- Attempted to address detached submenus by selecting an explicit adjacent left/right direction before opening.
- Target-machine verification showed the gap was unchanged because the underlying parent item remained wider than the visible menu. This attempt is superseded by the size-metric correction in `0.2.21`.

## 0.2.19-local - 2026-09-23

### Changed

- Refined the native tray menu with a minimum 34-logical-pixel row height, wider spacing, soft separators, rounded selection highlights, and a cleaner border in Light and Dark modes.
- Rendered current-setting summaries independently at a smaller size and secondary theme color, so long labels and right-aligned values no longer compete for the same text column.
- Kept primary account/quota rows high contrast, secondary status rows muted, and truly disabled actions visibly disabled.
- Replaced block-style checked backgrounds with a clear check mark while preserving native keyboard navigation, submenu behavior, working-area avoidance, and the existing menu hierarchy and commands.
- Added production WinForms render coverage for English and Simplified Chinese, Light and Dark, WebView2 and CLI, selected rows, submenus, long email addresses, and 100%/125%/150%/200% font-scale cases. All 89 automated tests pass.

## 0.2.18 - 2026-09-23

### Changed

- Standardized all new local-candidate and GitHub-release ZIP names as `CodexUsageTrayLite-v<version>-win-x64.zip`; retained older archives under their original immutable names.
- Added a complete Simplified Chinese portable-package guide as `README.zh-CN.txt` beside the English `README.txt`.
- Kept the complete bilingual changelog in every ZIP, preserving the `-local` suffix through `0.2.15` and using public version names from `0.2.16` onward.
- Made the packaged source URL follow the current version automatically and added packaging checks for current release headings, historical cutover markers, unresolved template tokens, README source links, and `local`-free new ZIP names.
- This documentation and packaging release does not change usage retrieval, menu behavior, icon rendering, settings, refresh timing, or process lifecycle behavior from `0.2.17`.

## 0.2.17-local - 2026-09-23

### Changed

- Reorganized the tray menu into 15 top-level status and action rows in both WebView2 and Codex CLI modes. Email and account level now share one row, while 5-Hour, Weekly, available resets, and last-updated details remain individually visible.
- Moved WebView2 proxy and login-session maintenance under Usage source and hid those entries, including their separators, in Codex CLI mode. Moved logs, config folder, Help, and About under a new Tools & Help submenu without adding a third menu level.
- Added right-aligned current selections to Usage source, Refresh interval, Icon style, Theme, and Language. These summaries have no accelerator keys, and changing the selected source does not rewrite the last successful source shown in Last updated.
- Made the three primary read-only status rows use normal high-contrast text in Light and Dark while keeping reset count and last-updated rows secondary. Other disabled actions retain the normal disabled appearance.
- Renamed the existing `Both (Default)` icon-style label to `Stacked (Default)` in all five UI languages without changing the saved enum, default, renderer, thresholds, or Weekly-only fallback behavior.
- Added production WinForms renderer snapshots for English and Simplified Chinese in Light and Dark, WebView2 and CLI submenu states, and a 150% font / long-email case. All 89 automated tests pass.

## 0.2.16 - 2026-09-19

### Changed

- Refined Side by Side for account levels that authoritatively have no 5-Hour limit: the Weekly value now uses the complete inner frame instead of leaving an empty right half.
- Applicable dual-window accounts retain the approved R2 Weekly-left / 5-Hour-right layout. Unknown 5-Hour applicability still renders Unknown rather than being treated as unlimited or zero.
- Added exact full-width fallback checks for Weekly 0/1/20/21/50/51/100% at 16/20/24/32 pixels in Light and Dark, including pixel equality with Weekly Only. All 88 automated tests pass.

## 0.2.15-local - 2026-09-19

### Added

- Added `Side by Side` as a fourth persistent Icon style while retaining `Both (Default)` as the default and preserving every existing saved choice.
- Side by Side divides the existing one-pixel framed inner canvas into adjacent Weekly-left and 5-Hour-right halves. Both flat rectangular fills start at the bottom, have no side padding, center gap, divider, rounded corner, or raised top, and independently use the existing pixel-precise percentage rounding.
- Added localized menu text for English, Simplified Chinese, Traditional Chinese, Japanese, and Korean, plus a renderer-backed preview derived from the approved R2 geometry.

### Changed

- Side by Side keeps the current colors and warning ownership: Weekly independently changes green/orange/red, 5-Hour stays theme blue, and only 5-Hour changes the outside frame to neutral/orange/red. When 5-Hour is authoritatively not applicable, the right half remains neutral instead of treating the missing value as zero.
- Expanded persistence, Tooltip, exact-pixel geometry, 0/1/20/21/50/51/100 thresholds, Light/Dark, 16/20/24/32-pixel, cache-transition, unavailable-data, special-state, and native-resource coverage. All 87 automated tests pass.

## 0.2.14-local - 2026-09-14

### Changed

- Enlarged the boxed `i`, `?`, and `L` tray-state glyphs and standardized them on the same bold red treatment for faster recognition in both Light and Dark taskbar themes.
- The renderer now fits each glyph's actual bold Segoe UI outline to the largest safe area inside the frame with one uniform scale factor. Narrow glyphs such as `i` use the maximum available height without distorting their original aspect ratio.
- Added Light/Dark regression assertions at 16, 20, 24, and 32 pixels for the solid-red glyph core and minimum occupied height. All 85 automated tests pass.

## 0.2.13-local - 2026-09-14

### Fixed

- Tightened the Pro 5-Hour N/A rule: the `i` icon and `5-Hour N/A` require authoritative non-applicability. If a Pro response ever contains a valid applicable 5-Hour quota, its real value is retained in the icon, Tooltip, and menu instead of being discarded solely because of the plan label.
- Added regression assertions for the conflicting-but-authoritative Pro 5-Hour value path. All 84 automated tests pass.

## 0.2.12-local - 2026-09-14

### Added

- Added an `Icon style` submenu directly below Refresh interval with `5-Hour Only`, `Weekly Only`, and `Both (Default)`. The single checked choice is applied immediately, saved, and restored at startup. New and migrated installations use Both.
- Added localized menu resources for all five supported application languages without repurposing the obsolete legacy `iconStyle` A-D setting.

### Changed

- Extended the quota renderer with independent Both, Weekly-only, and 5-Hour-only layer paths. Hidden quota values no longer influence visible warning colors or icon-cache keys; Both keeps the established appearance and automatically uses Weekly alone when 5-Hour is authoritatively not applicable.
- In 5-Hour Only mode, confirmed Pro Weekly-only data renders an empty quota frame with a high-contrast `i`. Unknown applicability, login-required, and refresh errors remain distinct and take priority over the information mark.
- In 5-Hour Only mode, the Tooltip contains the 5-Hour line alone and displays `5-Hour N/A` for Pro/non-applicable data. Weekly Only and Both deliberately retain the existing Tooltip behavior.
- Expanded settings migration, localization, Tooltip, Light/Dark, 16/20/24/32-pixel, threshold, cache-isolation, special-state, and native-resource regression coverage. All 84 automated tests pass.

## 0.2.11-local - 2026-09-11

### Fixed

- Corrected the account-level Tooltip rule introduced in 0.2.10: Plus accounts retain their applicable 5-Hour quota in both the Tooltip and menu. Only Pro (the currently supported level above Plus), or authoritative Weekly-only data, suppresses the Tooltip's 5-Hour line and uses `5-Hour N/A` in the menu.
- Updated regression coverage to distinguish Plus from Pro while preserving the existing Free/Go and Weekly-only behavior. All 80 automated tests pass.

## 0.2.10-local - 2026-09-11

### Added

- Added disabled account-status rows below Email for `User Level`, `5-Hour`, and `Weekly`, followed by the existing Usage resets and Last updated rows.
- Added conservative Free/Go/Plus/Pro detection. Codex CLI reads an exact `planType` from its existing `account/read` response and can use the rate-limit bucket's exact plan type as a fallback. WebView2 reads supported exact plan fields from its existing same-origin session response and otherwise accepts only an exact standalone `FREE`, `GO`, `PLUS`, or `PRO` badge near the top of the rendered page. Unsupported or missing values remain Unknown; Pro 5x/20x variants are intentionally not detected.
- Added localized User Level row labels to all five application `.resx` resource sets. Canonical Free/Go/Plus/Pro names remain unchanged across languages.

### Changed

- Removed the account email and `[CLI]`/`[WV2]` source tag from the Windows Tooltip. A normal account now shows only the 5-Hour and Weekly lines; detected Plus/Pro or an authoritative Weekly-only snapshot shows only Weekly.
- The menu always retains its 5-Hour row. Detected Plus/Pro and Weekly-only snapshots show `5-Hour N/A`; otherwise the 5-Hour and Weekly rows use the same percentage/reset text as the Tooltip.
- Fixed Help dialog paragraph and line breaks being collapsed by the Windows multiline Edit control. Resource text is now normalized to Windows CRLF before it is assigned to the control, without changing copied text.
- Expanded account-level, Tooltip, menu-line, settings, localization, and rendered Help multiline regression coverage. All 80 automated tests pass.

## 0.2.9-local - 2026-09-11

### Added

- Added Japanese and Korean application UI, including the tray menu, Proxy, Help, About, Login / Usage window, validation messages, and application prompts.
- Added editable `Resources/Strings.ja-JP.resx` and `Resources/Strings.ko-KR.resx` source resources. Release packages now carry the corresponding compiled `ja-JP` and `ko-KR` satellite assemblies alongside the existing Chinese satellites.
- Added Japanese and Korean choices to the Language submenu. A fresh installation follows Japanese or Korean Windows UI language in the same way as the supported Chinese locales; an existing saved language remains unchanged. Tooltip text intentionally remains English.

### Changed

- Expanded WebView2 Usage and available-reset parsing to cover verified Traditional Chinese (Hong Kong and Taiwan), Japanese, and Korean page wording, including localized Weekly/5-hour labels, remaining/used direction, reset timestamps, reset-section headings, exact available-count tabs, action labels, empty states, and section boundaries.
- Added authenticated real-page validation with Computer Use for Traditional Chinese (Hong Kong), Traditional Chinese (Taiwan), Japanese, and Korean, then restored the account website language to Simplified Chinese.
- Kept parsing conservative: unknown wording, ambiguous reset sections, or an unreadable Weekly value remains ParseError/Unknown rather than being inferred.
- Expanded localization, migration, dialog, parser, DOM-probe, and packaging coverage. All 79 automated tests pass.

## 0.2.8-local - 2026-09-11

### Changed

- When a successfully parsed snapshot identifies a Weekly-only plan (the current Pro-account shape with no applicable 5-Hour limit), the Tooltip now omits the 5-Hour row instead of displaying `5-Hour N/A`.
- A detected Pro/Weekly-only Tooltip now contains two lines: masked account plus source, then Weekly remaining/reset information. Normal dual-window accounts retain the existing three-line 5-Hour and Weekly layout.
- The same behavior applies to CLI and WebView2 sources and to cached/error-state Tooltips. Source and status suffixes remain visible, and the Windows 63-character limit remains enforced.
- Updated the Weekly-only Tooltip regression contract for both sources. All 75 automated tests pass.

## 0.2.7-local - 2026-09-10

### Added

- Added a unified `UsageField.Unavailable` warning event whenever the final result of a refresh cannot read `resets`, `five-hour`, or `weekly` data.
- Each event records only `source`, `field`, and a stable `reason`, such as `response-field-missing`, `percentage-unreadable`, `rate-limit-window-invalid`, `reset-section-unrecognized`, or `dom-probe-failed`. It never records quota values, page text, account identifiers, cookies, or tokens.
- Codex CLI diagnostics distinguish missing/invalid reset response fields and missing/invalid rate-limit windows. WebView2 diagnostics distinguish unreadable percentages, missing/unrecognized reset sections, and DOM-probe exceptions.
- Diagnostics are emitted once per unavailable field after the refresh has reached its final result, not for every retry. A valid zero count and a Weekly-only plan's intentionally non-applicable 5-Hour field are not warnings.
- Added field-classification, privacy-format, severity, and Weekly-only false-positive coverage. All 75 automated tests pass.

## 0.2.6-local - 2026-09-10

### Fixed

- Fixed WebView2 continuing to show `Usage resets: Unknown` after the Pro Usage page changed its zero state to an `Available 0` tab above reset history.
- The bounded body-text parser now reads exact `Available N`, `Available (N)`, Simplified/Traditional-Chinese `可用 N`, and Korean `사용 가능 N` lines only inside the recognized Usage-limit-resets section.
- The read-only DOM probe implements the same exact section-bounded tab-label rule. It still gives explicit counts/actions priority, ignores History dates and out-of-section text, and never clicks an element.
- Added a fixture matching the user's authenticated screenshot, including `Available 0`, History, Past 30 days, and dated reset-history rows, plus positive, localized, and out-of-section safeguards. All 74 automated tests pass.

## 0.2.5-local - 2026-09-10

### Fixed

- Kept the v0.2.4 Weekly-only/no-reset-section zero fallback, but explicitly separated a completed no-result DOM probe from a probe that threw an exception.
- A WebView2 script/runtime probe failure can no longer enter the zero fallback; it remains Unknown and retains the existing sanitized `WebView.UsageResetReadFailed` diagnostic.
- Extended the same zero-versus-Unknown regression test with the failed-probe case. All 74 automated tests pass.

## 0.2.4-local - 2026-09-10

### Fixed

- Fixed WebView2 showing `Usage resets: Unknown` on a confirmed Weekly-only plan when the Pro Usage page omits the entire Usage-limit-resets section because no banked resets are available.
- The fallback is deliberately narrow: it runs only after the existing 12-attempt DOM probe is exhausted, the quota result authoritatively says the 5-hour limit does not apply, and a final settled-page read still contains no recognized reset-section heading. That combination maps to zero.
- A recognized but unreadable/ambiguous reset section, a normal dual-window plan, missing quota data, and probe failures continue to return Unknown rather than a guessed zero.
- Added a privacy-minimized `WebView.UsageResetResolved` info event that records only the weekly-only/section-absent reason, never page text or usage values.
- Added exact zero-versus-Unknown fallback coverage. All 74 automated tests pass.

## 0.2.3-local - 2026-09-10

### Fixed

- Fixed both Codex CLI and WebView2 refreshes failing after an account changes to a plan that exposes a Weekly window but no 5-hour rate-limit window.
- The CLI parser now treats the observed single `10080`-minute window plus an absent secondary window as an authoritative Weekly-only plan instead of a malformed response. Weekly remains required; invalid or missing Weekly data still fails safely.
- The WebView2 parser now accepts a Weekly quota when the settled page has no general 5-hour quota card, or when the card explicitly reports Unlimited/N/A. It ignores 5-hour/weekly phrases inside the reset-section description and waits through three additional reads before accepting an inferred missing card, avoiding a premature result during lazy rendering.
- Added an explicit nullable 5-hour-applicability field to cached snapshots. Existing numeric snapshots migrate as applicable; Weekly-only snapshots clear stale 5-hour percentage/reset values without changing the settings schema.
- Weekly-only tooltips display `5-Hour N/A`. The tray icon remains a normal quota icon, renders the Weekly base layer, omits the blue 5-hour overlay, and uses a neutral border instead of the red fetch-error slash.
- Added exact single-window CLI, Weekly-only/explicit-unlimited WebView2, reset-description exclusion, settings migration, Tooltip, and renderer tests. All 73 automated tests pass.

## 0.2.2-local - 2026-09-10

### Changed

- Expanded WebView2 Usage-page parsing across English, Simplified Chinese, Traditional Chinese, and Korean wording without coupling it to the application's selected UI language or changing the website language.
- Added Unicode NFKC normalization so full-width digits, percent signs, colons, slashes, and mixed-language quota rows are parsed consistently.
- Added localized 5-hour/weekly labels, remaining-versus-used direction markers, reset-time markers, date/time formats, available-reset counts, reset-section headings, action labels, boundaries, and explicit no-resets banners.
- Preserved conservative behavior: conflicting percentage directions, ambiguous reset sections, and unknown wording remain unreadable/Unknown rather than being guessed; Codex Spark quota cards remain excluded.
- Expanded the read-only DOM fallback with the same four-language reset-section vocabulary. It still counts only visible enabled actions inside the bounded section, never clicks a reset, never changes page language, and never returns or logs page text.
- Added multilingual, mixed/full-width, cross-midnight, cross-date, zero/one/multiple reset, delayed-rendering-contract, ambiguity, and Spark-exclusion coverage. All 70 automated tests pass.

## 0.2.1-local - 2026-09-09

### Changed

- Replaced the in-code English/Simplified Chinese/Traditional Chinese string tables with standard .NET `.resx` resources.
- English is now the neutral resource in `Resources/Strings.resx`; Simplified Chinese is maintained in `Resources/Strings.zh-CN.resx`; Traditional Chinese is maintained in `Resources/Strings.zh-TW.resx`.
- `UiText` now resolves named resources through a cached `ResourceManager` and explicit `en-US`, `zh-CN`, or `zh-TW` culture. Existing language selection, first-run detection, live switching, fallback behavior, dialogs, and English-only Tooltip output remain unchanged.
- Release packages now include compiled `zh-CN/CodexUsageTrayLite.resources.dll` and `zh-TW/CodexUsageTrayLite.resources.dll` satellite assemblies. The editable `.resx` source files are not runtime configuration files and are not shipped.
- The allowlist package audit and README were updated for the two language directories. All 65 automated tests pass, including a new check for the embedded neutral resource and both compiled satellite resources.

## 0.2.0-local - 2026-09-09

### Added

- Added a Language submenu directly below Theme with `English`, `简体中文`, and `繁體中文` choices. Changing the selection updates the tray menu immediately and also updates an open Login / Usage window.
- Added complete Simplified Chinese and Traditional Chinese application UI text for the tray menu, source-specific entries, refresh intervals, Proxy, Help, About, Login / Usage status, validation messages, startup/failure prompts, and confirmation dialogs.
- Added first-run Windows UI-language detection: Simplified Chinese locales start in Simplified Chinese, Traditional Chinese locales start in Traditional Chinese, and all other locales start in English.
- Added localized application-owned prompt dialogs so OK, Yes, and No follow the selected app language even when it differs from the Windows display language.

### Changed

- Corrected the English Help wording and added `in your account` to both statements that the app never uses a reset. Simplified and Traditional Help are translated from this corrected text.
- Corrected the English 1-minute menu label from `1 minutes` to `1 minute`.
- Increased the settings schema from 3 to 4 to persist the selected language. Existing installations migrate to English to preserve the previous UI; new installations detect the initial language once and persist later user choices.
- Tooltips deliberately remain in the existing compact English format and retain the 63-character limit.
- All 64 automated tests pass, including new coverage for locale detection, settings migration, localized status formatting, all three Help texts, Proxy/About/Login dialogs, live Login-window language switching, and unchanged Tooltip output.

## 0.1.29-local - 2026-09-09

### Fixed

- Fixed Proxy, Help, About, and Login / Usage showing the Dark application icon while their effective window theme was Light.
- The 0.1.28 implementation selected the title-bar icon directly from the current Windows registry mode, independently of the Form theme. That allowed an explicit application Light theme to create the screenshot-observed Light window / Dark icon mismatch.
- Visible Forms now select the icon from the same resolved palette that controls their title bar and content: Light uses `AppIconLight`, Dark uses `AppIconDark`, and Follow Windows resolves both from the current Windows app theme.
- Open Follow Windows dialogs now have a second refresh path through Windows user-preference and display-setting events in addition to their window messages. The queued UI-thread refresh avoids retaining a stale icon when Windows changes appearance while a dialog remains open.

### Behavior and validation

- Explicit Light and Dark application themes remain stable across unrelated Windows appearance notifications; Follow Windows continues to update with Windows.
- System event handlers and replaced icon resources are detached/disposed with each Form. The hidden non-activating WebView2 background host remains outside this visible-window behavior.
- All 61 automated tests passed. The regression test now exercises every visible Form in Light, Dark, and Follow Windows modes and verifies that the title-bar icon matches the effective window theme while retaining the existing native-size and GDI/USER cleanup checks.
- Usage retrieval, CLI, WebView2, proxy, refresh, tooltip, dynamic tray icon, MessageBox semantics, settings schema, and logs are unchanged.

## 0.1.28-local - 2026-09-09

### Changed

- Adopted the approved Option C code-meter artwork as the static application identity icon, using the transparent Light source and the matching Dark source retained under `docs/icon-options/app-icon-r2`.
- Added complete Light and Dark ICO resources with native 16, 20, 24, 32, 40, 48, 64, 96, 128, and 256 px frames. The EXE and Windows Explorer use the Light variant by default.
- Proxy, Help, About, and Login / Usage Forms now use the Light or Dark application icon selected from the current Windows system color mode. This title-bar icon choice is intentionally independent of the application's manual Follow Windows / Light / Dark content-theme setting.
- Open Forms refresh their title-bar icon after Windows setting, theme, or DPI-change notifications. Replaced icon resources are explicitly disposed.

### Behavior and validation

- The dynamic Usage tray icon, its two quota layers and warning colors, Usage retrieval, CLI, WebView2, proxy, timer, tooltip, settings, and logs are unchanged.
- The unshown, non-activating background WebView2 host remains a plain hidden Form and does not receive the visible-window icon behavior.
- Standard WinForms MessageBoxes retain their exact owner, modality, buttons, return values, default-button behavior, and Windows-provided Information / Warning / Error / Question glyphs. They cannot use a custom title-bar `Form.Icon` without being replaced by custom dialogs, so this release deliberately leaves them unchanged.
- All 61 automated tests passed. Coverage verifies both embedded multi-size ICOs, the EXE's Light default including a Windows-extracted 256 px full-canvas frame, system-theme icon selection for every visible Form, runtime Light/Dark replacement, and bounded GDI/USER resource use.
- The built EXE displayed the new Light code-meter icon in a real Windows 11 File Explorer Details view on a dark background. Windows icon caching can retain an older rendering for a previously opened build path; extracting the versioned package to a new directory avoids that stale-cache case.

## 0.1.27-local - 2026-09-08

### Added

- Added a `Security and Privacy` section to the English Help dialog.
- The section explains that login and account information is not sent to the developer or third parties; passwords, cookies, and access tokens are not read or stored by the app; WebView2 and Codex CLI retain control of their own credentials; and the full account email is memory-only with masked logging.
- It also states that the app has no telemetry or cloud backend, reads Usage information only, and never consumes a Usage reset.

### Behavior and validation

- Increased the Help dialog height while retaining its centered, fixed-size, scrollable, theme-aware layout.
- No collection, storage, networking, authentication, Usage, proxy, refresh, tooltip, icon, or settings behavior changed; this release documents the existing privacy boundaries in-app.
- All 59 automated tests passed, with expanded assertions for the security heading, credential handling, email retention, telemetry, and read-only reset statements.

## 0.1.26-local - 2026-09-08

### Added

- Added a `Help` tray-menu item immediately above `About`.
- Added a concise English Help dialog explaining the difference between WebView2 and Codex CLI, the basic setup steps for each source, and which login and proxy settings apply.
- The Help dialog states that both sources only read Usage information and that the app never consumes a Usage reset.

### Behavior and validation

- The dialog is centered, fixed-size, keyboard-dismissable, and follows the application's Follow Windows, Light, or Dark theme.
- No Usage fetching, login, proxy, refresh, tooltip, icon, or settings behavior changed.
- All 59 automated tests passed, including Help content, layout, theme, and the existing functional and native-resource regression suite.

## 0.1.25-local - 2026-09-07

### Changed

- 5-Hour remaining quota independently controls the outer border: black in Light / white in Dark above 50%, orange at 21-50%, and red at 0-20%.
- Weekly remaining quota independently controls its lower fill: green above 50%, orange at 21-50%, and red at 0-20%. Weekly never changes the border.
- The centered 72%-width blue overlay and independent pixel heights are preserved. Zero Weekly has no fill; special states retain their marks and neutral theme border.
- Icon cache keys include both warning bands so threshold crossings redraw even when rounded pixel heights stay the same.

### Validation

- 58 automated tests passed, including independent color combinations, threshold boundaries, cache transitions and recovery, reference geometry and resource lifetime.
- Inspected the built production renderer at 16/20/24/32 px in both themes. Live taskbar and mixed-monitor DPI acceptance remain manual checks.

## 0.1.24-local - 2026-09-07

### Changed

- Replaced the live battery icon with the approved full-canvas rectangle: one-pixel border, full-width green Weekly base, and centered 72%-width blue 5-Hour overlay.
- Both remaining quotas independently set bottom-aligned pixel heights, without the old 5% buckets. Positive values have at least one pixel; zero is empty. Colors stay green/blue at low quota.
- Light/Dark colors follow the existing application theme setting. Theme and display-setting notifications redraw the icon without fetching Usage.
- Unknown, Error and LoginRequired keep distinct marks; missing quota data is not rendered as zero. Native icon caching is bounded to 16 entries.

### Validation

- 56 automated tests passed, including reference-pixel checks at 16/20/24/32 px in both themes, quota/state checks and bounded-cache disposal.
- Inspected a preview generated from the built production renderer. Live Windows taskbar and mixed-monitor DPI acceptance remain manual checks.
- Usage retrieval, CLI, WebView2 parsing, proxy, timers and logs retain their existing behavior.

## 0.1.23-local - 2026-09-06

### Fixed

- Fixed WebView2 showing `Usage resets: Unknown` after all available Usage resets have been consumed and the page displays `No usage limit resets available at this time.`
- Added exact recognition of that explicit zero-availability banner to both the bounded body-text parser and the delayed DOM fallback.
- Recognition remains limited to the `Usage limit resets` section before `Auto reload` or `Usage breakdown`; ambiguous wording and the same text outside the section remain Unknown.

### Behavior and validation

- The result is cached and displayed as `Usage resets: 0`, following the existing explicit-zero policy. The app remains read-only and does not click or consume a reset.
- Added screenshot-shaped zero-banner, ambiguous-text, section-boundary, and DOM-contract coverage. All 53 automated tests passed.
- The user confirmed that the 0.1.22 no-activate WebView2 host restored normal Windows 11 screen-saver operation.

## 0.1.22-local - 2026-09-06

### Fixed

- Fixed WebView2 mode preventing or interrupting the Windows 11 screen saver during periodic background refreshes.
- The regression was traced to the 0.1.21 desktop-layout reset reader showing a transparent, offscreen 1280x900 top-level Form for each fetch. The reset DOM probe itself remains read-only and unchanged.
- Background WebView2 now creates only the hidden parent HWND required by the controller and never calls `Form.Show()`.
- Added `WS_EX_NOACTIVATE`, tool-window styling, and `ShowWithoutActivation` protection so the background host cannot take activation if it is ever shown accidentally.

### Behavior and validation

- The 1280x900 controller viewport and delayed Usage-reset DOM reader are preserved; no reset, quota, email, menu, tooltip, CLI, proxy, or settings behavior changed.
- Added an automated hidden-host visibility/activation contract. All 52 automated tests passed.
- A real temporary-profile WebView2 controller probe navigated successfully with the host never shown, confirmed the 1280x900 DOM viewport, closed the controller without process killing, and observed a normal browser-process exit.
- The authenticated profile probe could not run concurrently with the user's existing tray instance because that profile was in use; the prior 0.1.21 authenticated reset-reader result remained the functional baseline for the unchanged DOM probe. The user subsequently confirmed on Windows 11 that the screen saver starts normally with WebView2 mode running.

## 0.1.21-local - 2026-09-05

### Fixed

- Replaced WebView2's text-line-only reset-list fallback with a DOM-based reader verified against the application's real authenticated profile.
- The hidden WebView now uses a 1280x900 desktop-layout viewport instead of 2x2 pixels, preventing the Usage page from selecting or lazily retaining an unusable tiny layout.
- After quota parsing, the reader scrolls the `Usage limit resets` heading into view and retries for up to 2.75 seconds so delayed reset rows can appear before the session closes.
- Each enabled, visible interactive element labeled exactly `Use reset` inside the bounded section counts once. Disabled, hidden, explanatory, and out-of-section text remains excluded.

### Behavior and safety

- The DOM probe is read-only: it never clicks a button, invokes reset consumption, or records page text, account data, quota values, or reset counts in logs.
- CLI behavior, menu placement, Tooltip content, settings compatibility, and no-idle-WebView lifecycle remain unchanged. The larger viewport exists only during a WebView2 fetch.

### Validation

- The Release x64 build and all 51 automated tests passed.
- A real authenticated WebView2 probe using the app's existing dedicated profile returned `usageResetCountRead=true` while printing no account, quota, or reset value.

## 0.1.20-local - 2026-09-05

### Fixed

- Fixed WebView2 reporting `Usage resets: Unknown` when the Usage page presents available resets as a list instead of an explicit numeric summary.
- Inside the bounded `Usage limit resets` section, each exact `Use reset` action row now counts as one available reset.
- The parser excludes the explanatory `Use a reset to restore...` sentence and any matching text after the next `Auto reload` or `Usage breakdown` section boundary.

### Behavior and safety

- Explicit numeric summaries remain supported. An explicit no-resets message maps to 0; a missing or ambiguous section remains Unknown rather than being guessed as zero.
- CLI parsing, menu placement, Tooltip content, network requests, reset-consumption behavior, and logging privacy are unchanged.

### Validation

- The Release x64 build and all 50 automated tests passed.
- Added screenshot-shaped single-row, multi-row, explicit-empty, CRLF, explanatory-text, and out-of-section fixtures.

## 0.1.19-local - 2026-09-05

### Added

- Added a disabled `Usage resets: N` status row directly below the account Email in the tray menu. The tooltip remains unchanged.
- Codex CLI mode reads the authoritative top-level `rateLimitResetCredits.availableCount` from the existing `account/rateLimits/read` response; no extra request is made.
- WebView2 mode recognizes an explicit numeric available-reset statement in the rendered Usage-page text without using cookies, tokens, or a private reset endpoint.

### Behavior and safety

- Missing, unsupported, stale-source, or login-required reset data displays `Usage resets: Unknown`; only an explicitly returned zero displays `Usage resets: 0`.
- Existing cached settings remain compatible. The reset count is cached with the last successful Usage snapshot but is shown only while that snapshot belongs to the currently selected, authenticated source.
- The app remains read-only: it does not invoke any reset-consumption action and does not write reset counts to logs.

### Validation

- The Release x64 build and all 49 automated tests passed.
- Added coverage for CLI summary parsing, explicit WebView2 count wording, zero-versus-Unknown handling, source mismatch, login-required state, and settings serialization.

## 0.1.18-local - 2026-09-04

### Added

- Added local-calendar-day log rotation. `CodexUsageTrayLite.log` is always the active log for the current day.
- Before the first write after local midnight, the previous active log is archived as `CodexUsageTrayLite_YYYYMMDD.log` using the date on which that log was last written.
- If the app was not running for one or more days, the next startup archives the existing active log under its actual last-write date without creating empty files for skipped dates.
- If the exact daily archive already exists, the remaining active segment is appended to it instead of overwriting or deleting either segment.

### Changed

- Replaced the previous 2 MiB `.log.1` size rotation with daily date-stamped archives.
- `Open logs` continues to open the current `CodexUsageTrayLite.log`; date-stamped historical logs remain in the same `logs` directory and are not deleted automatically.

### Validation

- The Release x64 build and all 47 automated tests passed.
- Added coverage for midnight rollover, same-day writes, exact archive naming, and collision-safe preservation of multiple segments from one date.

## 0.1.17-local - 2026-09-04

### Changed

- Split the bilingual Changelog into the English `CHANGELOG.md` and Chinese `CHANGELOG.zh-CN.md` files.
- Split the bilingual release notes into the English `RELEASE_NOTES.md` and Chinese `RELEASE_NOTES.zh-CN.md` files.
- Added all four language-specific Markdown files to the portable-package allowlist and internal SHA-256 manifest.
- Recorded the project convention that bilingual Markdown documentation must use an English base filename and a matching `.zh-CN.md` Chinese filename instead of mixing languages in one file.
- Runtime functionality is unchanged from `0.1.16-local`.

### Validation

- The Release x64 build and all 46 automated tests passed.
- The package audit verified both English files and both Chinese files as separate entries.

## 0.1.16-local - 2026-09-04

### Added

- Added a complete English edition of this file, preserving matching feature, fix, security, distribution, and validation records for every release from `0.1.0-local` through the current version.
- Added a complete English edition of `RELEASE_NOTES.md`, including the current-release summary, upgrade guidance, full capability summary, requirements, validation results, known limitations, and version index.

### Changed

- Standardized both release documents on a bilingual structure with Chinese first, English second, and in-page language navigation.
- This release does not change Usage retrieval, menus, tooltips, refresh intervals, or process-lifecycle behavior; runtime functionality is unchanged from `0.1.15-local`.

### Validation

- The Release x64 build and all 46 automated tests passed.
- Both bilingual documents remain included in the portable ZIP and its internal SHA-256 manifest.

## 0.1.15-local - 2026-09-04

### Added

- Added `1 minute` and `2 minutes` to Refresh interval, making the complete set 1, 2, 5, 10, 15, 30, and 60 minutes.
- Created this file and `RELEASE_NOTES.md`, reconstructing the history from the initial build through the current release.
- Added `CHANGELOG.md` and `RELEASE_NOTES.md` to the portable-package allowlist.

### Changed

- The default refresh interval remains 15 minutes; 1-minute and 2-minute refreshes are used only after the user explicitly selects them.
- Updated the project specification, test checklist, and portable-package documentation to match the new intervals and current menu/tooltip behavior.

### Validation

- The Release x64 build and all 46 automated tests passed.
- Package contents continue to use an exact allowlist and internal SHA-256 manifest.

## 0.1.14-local - 2026-09-04

### Changed

- Reorganized the tray menu into account status, common actions, source/refresh settings, application preferences, maintenance information, and Exit groups.
- Consolidated Email and Last updated at the top; grouped About with the log and configuration entries in the maintenance area.
- CLI mode hides `Proxy settings (WebView2)` and `Clear WebView2 login session`, leaving only CLI-relevant actions.
- WebView2 mode shows its source-specific login page, Proxy, and WebView2 session-cleanup actions.

### Validation

- All 46 automated tests passed.

## 0.1.13-local - 2026-09-04

### Added

- Added the short source tag `[CLI]` or `[WV2]` after the masked email on the tooltip's first line.

### Changed

- Dynamically allocates email space within the NotifyIcon 63-character hard limit; ordinary addresses remain as complete as possible and long domains are compacted in the middle.
- In extreme combinations such as `100%`, long dates, `[cached]`, `[error]`, or `[login]`, the source tag, both quota lines, and the status marker take priority.
- When very little email space remains, the tooltip still tries to preserve the first character, `@`, and an ellipsis instead of truncating the entire tail.

### Validation

- All 45 automated tests passed.

## 0.1.12-local - 2026-09-03

### Changed

- After the Codex CLI root process exits normally, descendants in its Job Object receive up to 500 ms to exit naturally, checked every 50 ms.
- When descendants drain during that grace period, `CodexCli.ProcessExit` is logged at `info`; if processes remain afterward, the event stays at `warning` and the Job Object performs final cleanup.
- Added `jobDrainWaitMs` to CLI lifecycle logs to reduce false warning reports for short-lived child processes that are exiting normally.
- Changed `Open logs` to open `CodexUsageTrayLite.log` directly through its Windows file association; if the log does not yet exist, it is created first instead of opening only the directory.

### Validation

- All 45 automated tests passed.
- Three real CLI validations all produced normal `info` exit records.

## 0.1.11-local - 2026-09-03

### Added

- Added structured `level=info|warning|error` and `event=...` fields to log records.
- After account data is parsed successfully, logs record the source and masked email, for example `email=u***@example.com`.

### Security

- Log email masking now matches the UI rule: first character + `***` + complete domain.
- The log sanitizer rescans detail text and replaces any original email that enters it unexpectedly.
- The tray menu still shows the full email only in process memory; the full address is never written to settings or logs.

### Validation

- All 43 automated tests passed.

## 0.1.10-local - 2026-09-03

### Added

- CLI mode reads the account email from the official `account/read` response it already requests.
- WebView2 mode attempts to read the account email with a same-origin `/api/auth/session` request in the existing `chatgpt.com` page context; failure does not interrupt Usage retrieval.
- The tooltip's first line shows a masked email, while the tray menu's first line shows the complete email.

### Changed

- Renamed the core tooltip label from `5Hours` to `5-Hour`, producing a three-line Email, 5-Hour, and Weekly layout.
- Long email addresses are compacted automatically to respect the NotifyIcon 63-character limit.

### Security

- The full email exists only in current-process memory and is cleared when the source changes, login becomes invalid, the session is cleared, or the app exits.
- The app does not read or output cookies, tokens, Authorization headers, WebView2 session-response bodies, or CLI credential files.

### Validation

- All 42 automated tests passed.
- Real local tests confirmed email retrieval from both the CLI and the dedicated WebView2 profile; test output exposed only success/failure and masked values.

## 0.1.9-local - 2026-09-03

### Fixed

- Fixed the CLI-to-WebView2 switch so it refreshes immediately instead of waiting for the user to open the login page.
- Switching to either valid source performs one immediate validation that is allowed even when setup is incomplete; an existing valid WebView2 profile no longer requires opening the login window.
- When login is genuinely invalid, the app remains silently in login-required state until the user chooses to open the login page.
- Fixed reset values containing only a time, with no date, so they no longer incorrectly inherit the system's current date and instead use a deterministic date anchor.

### Validation

- All 41 automated tests passed.

## 0.1.8-local - 2026-09-02

### Added

- Added CLI and WebView2 lifecycle logs that distinguish natural exit, explicit shutdown, and abnormal failure.
- CLI logs include the launched PID, whether stdin was closed, exit type, exit code, whether exact-PID `Process.Kill()` was called, the Job Object active-process count, and kill-on-close state.
- WebView2 logs include controller creation, return from `CoreWebView2Controller.Close()`, `processKill=false`, ProcessFailed details, and the final `BrowserProcessExited` type.

### Changed

- Process-control reporting explicitly distinguishes “close the process tree started by this operation” from “terminate same-named processes on the system”; the app never terminates Codex, Node, Edge, or WebView2 by process name.

### Validation

- All 40 automated tests passed.
- A real WebView2 probe observed asynchronous `BrowserProcessExited=Normal` after controller Close; the test PID then disappeared without calling Kill.

## 0.1.7-local - 2026-09-02

### Added

- Added `Usage source > WebView2 / Codex CLI`.
- CLI mode starts the installed `codex app-server` on demand and performs JSONL initialize/initialized, `account/read`, and `account/rateLimits/read` exchanges.
- Accepts only the general Codex bucket, maps the 300-minute window to 5-Hour and the 10,080-minute window to Weekly, and converts `usedPercent` to remaining percentage.
- Locates `codex.exe`, `codex.cmd`, and `codex.bat`; command scripts launch through a correctly quoted `cmd.exe` invocation.
- Each read uses an independent Job Object and private standard input/output streams, then closes only the process tree for that operation.

### Changed

- CLI mode does not apply the WebView2 proxy or manipulate the WebView2 login session.
- The CLI source does not read `auth.json`, tokens, or other credential files; it reuses the installed CLI's own login state and network configuration.
- The CLI is an optional dependency and is not bundled in the ZIP; no DLL or runtime was added, and the portable package grew by only about 10.7 KB.

### Validation

- All 37 automated tests passed.
- A real validation against the locally logged-in CLI read both quota windows in about 1.681 seconds without printing quota/account values or leaving a new process behind.

## 0.1.6-local - 2026-09-02

### Changed

- Enlarged the Option A battery icon proportionally by about 10.4%, reaching the safe maximum visual size allowed by the current Windows tray slot.
- Preserved the battery body's exact 3:4 ratio; the body, terminal, outline, fill, and status marks all use the same scale to avoid distortion.
- Replaced the fixed 32×32 source with native icon generation based on Windows `SM_CXSMICON`, clamped to the 16–32 px range.

### Research

- Evaluated the CLI app-server method used by `codex-usage-widget`, including package-size and resource effects. A lightweight on-demand CLI mode was found feasible, but CLI support was not implemented in this release.

### Validation

- All 32 automated tests passed.

## 0.1.5-local - 2026-09-02

### Added

- Added persistent `Theme > Follow Windows / Light / Dark` choices.
- Applied the selected theme to the tray menu, Proxy, About, login window, and Windows 11 title bars.
- WebView2 uses the official PreferredColorScheme; third-party OAuth pages may retain their own theme.

### Changed

- Existing settings migrate to `Follow Windows` without changing their configured refresh state.

### Validation

- All 31 automated tests passed.

## 0.1.4-local - 2026-09-02

### Added

- Added About immediately above Exit and a screen-centered About dialog.
- About displays the application name, version, x64 architecture, .NET target, UTC build time, WebView2 SDK/Runtime, upstream baseline, and MIT license information.

### Fixed

- Changed Proxy Settings from the screen's upper-left corner to the center of the current screen.

### Distribution

- Synchronized the loose test copy in the workspace, then renamed the ambiguous `CodexUsageTrayLite-v0.1.0-local` directory to the version-neutral `artifacts/CodexUsageTrayLite-local`. The directory rename did not create another application version.

### Validation

- All 29 automated tests passed.

## 0.1.3-local - 2026-09-02

### Changed

- A fresh, unconfigured WebView2 installation no longer performs the four-second startup refresh, starts a periodic timer, or creates a hidden WebView2 instance.
- `Refresh now` is disabled until the first login is completed; changing Proxy alone does not trigger a background request.
- After the user explicitly opens and closes the login window, the app validates once; manual and periodic refresh are enabled only after Usage is retrieved successfully for the first time.
- Login expiration or clearing the login session stops the timer and disables manual WebView2 refresh again.
- Existing settings migrate according to previous successful Usage and loginRequired state, avoiding disruption to configured installations.

### Validation

- All 28 automated tests passed.

## 0.1.2-local - 2026-09-02

### Fixed

- Fixed `Open login / usage page` doing nothing when a hidden startup refresh was busy.
- Opening the login window or Proxy Settings first cancels and disposes the active hidden fetch before continuing with the user action.
- Explicit cancellation and network timeout are now distinct; user-initiated cancellation no longer shows a false `[error]` state.
- WebView2 initialization failures show their specific error; fixed races between page parsing and controller disposal during cancellation.
- Ordinary Microsoft Edge windows remain independent: they do not need to be closed and are never controlled by this application.

### Validation

- All 26 automated tests passed.

## 0.1.1-local - 2026-09-01

### Added

- Established the standard versioned portable-release workflow in `scripts/package-release.ps1`.
- Each package run validates version fields, restores dependencies, builds Release x64, runs the automated tests, and generates an internal `SHA256SUMS.txt`, ZIP, and external `.zip.sha256` sidecar.
- The packaging script uses an exact file allowlist that excludes PDBs, profiles, settings, logs, dumps, secrets, tokens, cookies, environment files, and repository build tools.
- The script rejects an existing ZIP or sidecar immediately, preserving every older release.

### Distribution

- The first versioned package was `CodexUsageTrayLite-v0.1.1-local-win-x64.zip`.
- The target Windows 11 x64 system still requires .NET Framework 4.8 or later; WebView2 mode requires the Evergreen Runtime.

### Validation

- All 25 automated tests passed.
- The ZIP allowlist, internal manifest, external checksum, and overwrite refusal were verified.

## 0.1.0-local - 2026-09-01

`0.1.0-local` was the initial loose artifact and was updated in place several times on the same day without changing its version. The following record therefore describes that phase's final state; overwrite-style development stopped with the next release.

### Added

- Created an independent .NET Framework 4.8, WinForms, x64 project based on MIT-licensed `saveway/codex-usage-monitor` `v2.0.0-preview.7` (commit `36e9679164dcd7e5ef23d1f35822664785fad01f`).
- Created a system-tray-only architecture, removing the Widget, Overlay, charts, Toast, BalloonTip, and quota pop-up from the new executable.
- Added an on-demand hidden WebView2 controller. After each fetch completes, fails, times out, or is canceled, events are detached, the controller is closed, and the host Form is disposed. Login state is kept in this app's dedicated `%LOCALAPPDATA%/CodexUsageTrayLite/webview2-profile`.
- Added System, Direct, Custom HTTP, and Custom SOCKS5 proxy modes. Direct uses `--no-proxy-server`; proxy arguments are validated to prevent browser-argument injection.
- Added original upright battery tray-icon concepts A–D with 16/20/24/32 px previews; the final choice changed from narrower Option B to Option A.
- The icon represents 5-hour remaining quota and supports normal, warning, low-quota, Unknown, Error, LoginRequired, and true 0% states. Dynamic icons are cached and their HICON/GDI resources are released correctly.
- The tooltip shows 5-hour/Weekly remaining and local reset times while retaining cached/error/login status within the 63-character limit.
- Added 5, 10, 15, 30, and 60 minute refresh intervals with a 15-minute default and four-second startup delay.
- Added Start at login, Last updated, Open logs/config, Clear login session, safe logging, and single-instance behavior.
- Created `PROJECT_SPEC.md`, `LOCAL_DEV_NOTES.md`, `TESTING.md`, the Solution, test harness, and icon-design documentation.

### Fixed during the initial stage

- Adjusted tooltip copy to the aligned `5Hours ...` / `Weekly ...` layout.
- Expanded the Proxy dialog from 390×205 to 620×340 over several iterations, added Direct, shortened labels, and fixed high-DPI text overlap.
- Changed the tray icon from Option B to Option A.

### Validation and known limitation

- The initial implementation passed 22 tests; the final in-place update of this version passed 25.
- A 100-cycle injected refresh test passed for session creation/disposal and GDI checks.
- A real rapid 50-controller WebView2 test confirmed eventual browser-process exit, but host Handles grew from 286 to 475; an 8–12 hour natural-interval soak remains required before public distribution.
