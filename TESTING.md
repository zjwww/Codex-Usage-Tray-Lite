# Codex Usage Tray Lite Testing

## Build and automated checks

Use the repository-local SDK (it is ignored and not shipped):

```powershell
$env:DOTNET_CLI_HOME = (Resolve-Path '.tools').Path
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
& '.\.tools\dotnet\dotnet.exe' restore '.\CodexUsageTrayLite.sln' --configfile '.\NuGet.Config'
& '.\.tools\dotnet\dotnet.exe' build '.\CodexUsageTrayLite.sln' -c Release -p:Platform=x64 --no-restore
& '.\tests\CodexUsageTrayLite.Tests\bin\x64\Release\net48\CodexUsageTrayLite.Tests.exe'
```

Create the versioned portable ZIP only after the full build and test gate:

```powershell
& '.\scripts\package-release.ps1'
```

The script writes a ZIP and matching `.zip.sha256` under `artifacts\packages`. It must fail if either output for the current version already exists. Increase `Version`, `AssemblyVersion`, `FileVersion`, `InformationalVersion`, and `AppConstants.Version` for every new package; never delete an older package merely to reuse its version.

## 0.3.1 leading interactive spacer removal - 2026-09-25

- `CommandLineParsingAndStatus` now covers selection of the interactive output row: the row immediately above CMD's premature prompt is reused only when it is inside the visible window and its complete console-buffer width is blank. A nonblank, unavailable, or offscreen row leaves output on the validated prompt row, so existing content is never overwritten.
- A real interactive `cmd.exe /Q /K` PTY run reproduced CMD's extra spacer, then verified v0.3.1 places `Codex Usage Tray Lite 0.3.1` immediately below the command, leaves exactly one empty row below the result, and restores the complete prompt. `echo READY` succeeded at that restored prompt without an additional Enter.
- The final packaging gate completed with 0 errors and two restricted-network `NU1900` warnings; all 97 automated tests passed. The packaging resource check held Handles at `358 -> 358`, the final icon GDI loop remained `18 -> 18`, and the menu-region lifecycle regression passed.
- Package: `artifacts\packages\CodexUsageTrayLite-v0.3.1-win-x64.zip`; SHA-256 `91626a6ad2fe2964fd78addafa81d9bff57dac47a1b8ac4159a6ad8561f1c634`. The independent audit verified the exact 22-file allowlist, all 21 internal manifest hashes, matching sidecar, EXE file version `0.3.1.0`, bilingual package guides and release documents, complete changelogs, forbidden-file exclusions, and no retained real account email in package text.
- The installed v0.3.0 process (PID 7816) accepted its own `-exit` request with exit code 0. After the requested five-second wait, the process count was zero. All 22 v0.3.1 package files were copied to `C:\ZZZ\Soft_Running\CodexUsageTrayLite`; the installed EXE is `0.3.1.0`, all 21 manifest hashes match, no package file is missing, the existing non-package `README-local.txt` was preserved, and the new program remains stopped for the user's manual launch.
- The reported standard CMD case was live-verified. Custom, multiline, colored, wrapped, or third-party prompts retain conservative safeguards but were not all live-tested. No authenticated v0.3.1 usage refresh was run because this change does not alter refresh or account behavior.

## 0.3.0 conventional interactive command layout - 2026-09-25

- `CommandLineParsingAndStatus` now covers the exact safety predicate used before clearing a prematurely rendered shell prompt. An unchanged printable single-line prompt at its original row/cursor position is eligible; typed command text, control characters, or a moved cursor/row force the existing newline fallback. The implementation never injects Enter or other keyboard input and stays inactive when output is redirected.
- The console path re-reads and validates the visible prompt immediately before output. When safe, it clears only that prompt's occupied cells through `FillConsoleOutputCharacter`, returns the cursor to column zero with `SetConsoleCursorPosition`, writes the command result, adds one blank line, and restores the exact prompt text. Unsupported consoles and API failures preserve the safe v0.2.30 layout.
- A real interactive `cmd.exe /Q /K` PTY run verified `CodexUsageTrayLite.exe -version` displays `Codex Usage Tray Lite 0.3.0`, one blank line, and then the restored prompt. Running `echo READY` at that restored prompt succeeded without another Enter, proving the shell remained immediately usable.
- The final packaging gate completed with 0 errors and two restricted-network `NU1900` warnings; all 97 automated tests passed. The packaging resource check held Handles at `358 -> 358`, the final icon GDI loop remained `18 -> 18`, and the menu-region lifecycle regression passed.
- Package: `artifacts\packages\CodexUsageTrayLite-v0.3.0-win-x64.zip`; SHA-256 `578aafae244ac12feb9ce924528d0f426871f342017645b10e6178962dc8aa9e`. The independent audit verified the exact 22-file allowlist, all 21 internal manifest hashes, matching sidecar, EXE file version `0.3.0.0`, bilingual package guides and release documents, complete changelogs, forbidden-file exclusions, and no retained real account email in text source.
- The installed v0.2.30 process (PID 9428) accepted its own `-exit` request with exit code 0. After the requested five-second wait, the process count was zero. All 22 v0.3.0 package files were then copied to `C:\ZZZ\Soft_Running\CodexUsageTrayLite`; the installed EXE is `0.3.0.0`, all 21 manifest hashes match, no package file is missing, no extra file needed preservation, and the new program remains stopped for the user's manual launch.
- The default Command Prompt layout above was live-verified. Custom, multiline, colored, or third-party prompts retain a deliberately conservative fallback but were not all live-tested. No authenticated v0.3.0 usage refresh was run because this change does not alter refresh or account behavior.

## 0.2.30 interactive shell-prompt restoration - 2026-09-25

- `CommandLineParsingAndStatus` now covers bounded printable single-line prompt eligibility for default Command Prompt and PowerShell forms, and rejects command text or control-character content. The implementation reads only the current visible console line, never injects Enter or keyboard input, and remains inactive for redirected output.
- A real interactive `cmd.exe /Q /K` PTY run verified both the immediate `-version` path and the approximately two-second `-exit` path. Each moved output below the shell's prematurely rendered prompt and restored `C:\ZZZ\Codex_Projects\CodexUsageTrayLite>` at the end without additional input. The `-exit` request reached the running installed v0.2.29 tray and returned success.
- The final packaging gate completed with 0 errors and two restricted-network `NU1900` warnings; all 97 automated tests passed. The 100-session resource check stayed within its bounded allowance at Handles `353 -> 358`; the final icon GDI loop remained `18 -> 18`, and the menu-region lifecycle regression passed.
- Package: `artifacts\packages\CodexUsageTrayLite-v0.2.30-win-x64.zip`; SHA-256 `4840bb479907ecccc1efc95d3d7a5d59f2c8922b7c8d1a04bafcec10f4101b2a`. The independent audit verified the exact 22-file allowlist, all 21 internal manifest hashes, external sidecar, EXE file version `0.2.30.0`, bilingual package guides and release documents, complete changelogs, forbidden-file exclusions, and no retained real account email in text source.
- After the successful `-exit`, the target remained stopped for an additional five-second check. All 22 package files were copied to `C:\ZZZ\Soft_Running\CodexUsageTrayLite`; the installed EXE is `0.2.30.0`, all 21 manifest hashes match, no package file is missing, the pre-existing non-package `README-local.txt` was preserved, and no application process was restarted. The user retains control of the next launch.
- The default single-line prompts shown above were live-verified. Custom/multiline/color prompt fidelity and a real authenticated v0.2.30 `-refresh`/`Refresh.Success` cycle remain target-machine follow-up checks.

## 0.2.29 startup-argument compatibility - 2026-09-25

- The command parser regression now requires the existing `--startup` argument emitted by `StartupManager.BuildCommand(...)` to resolve to the normal tray-run path. This preserves current-user Start at login while keeping the new user-facing one-option command interface unchanged.
- The final packaging gate completed with 0 errors and two restricted-network `NU1900` warnings; all 97 automated tests passed. The 100-session resource check held Handles at `358 -> 358`; the final icon GDI loop remained `18 -> 18`, and the menu-region lifecycle regression passed.
- Package: `artifacts\packages\CodexUsageTrayLite-v0.2.29-win-x64.zip`; SHA-256 `0d824a311ec839ed0658f8b57ba6072437afddfd75a9e3ac225d4be51cbbdcba`. The independent audit verified the exact 22-file allowlist, all 21 internal manifest hashes, external sidecar, EXE file version `0.2.29.0`, bilingual package guides and release documents, complete changelogs, forbidden-file exclusions, and synchronized loose mirror.
- A v0.2.28 pre-handoff candidate had already been packaged when this compatibility issue was found. It was preserved rather than overwritten or deleted; v0.2.29 supersedes it and is the delivery package.
- The installed tray process was not stopped or replaced. Therefore control of a real running v0.2.29 instance through `-refresh`/`-exit` and a resulting authenticated `Refresh.Success` line remain post-upgrade acceptance checks; the compiled command output, named-event transport, refresh formatter, UI-thread dispatch contract, normal automated refresh paths, and startup-argument mapping were exercised without using the user's live account.

## 0.2.28 refresh-success logging and command-line interface - 2026-09-25

- `RefreshSuccessLogFormatting` verifies one-line source, masked-email, user-level, 5-Hour/Weekly values and reset times, available reset count, fetched timestamp, and duration output. It covers a normal dual-window snapshot, authoritative Weekly-only `N/A`, explicit zero, and Unknown fields, and rejects the raw email.
- `CommandLineParsingAndStatus` covers every documented option and alias, one-option-only rejection, English saved-snapshot formatting, Pro/Weekly-only `5-Hour: N/A`, and offline status semantics. `CommandLineExecutableOutput` launches the compiled WinExe with redirected stdout/stderr and verifies version/help/invalid-option output and exit codes.
- `SingleInstanceCommandChannel` exercises both refresh and exit signals through a unique session-local test channel and verifies disposal. The production commands deliberately signal the existing tray process instead of constructing a second `TrayApplicationContext`, WebView2 controller, or Codex CLI fetch process.
- The final packaging gate completed with 0 errors and two restricted-network `NU1900` warnings; all 97 automated tests passed. The 100-session resource check held Handles at `358 -> 358`; the final icon GDI loop remained `18 -> 18`, and the menu-region lifecycle regression passed.
- Package: `artifacts\packages\CodexUsageTrayLite-v0.2.28-win-x64.zip`; SHA-256 `5cd7067dcaa70604cb546890f967f946ae3878827cb12ecf51886ceda8337c0d`. The independent audit verified the exact 22-file allowlist, all 21 internal manifest hashes, external sidecar, EXE file version `0.2.28.0`, bilingual package guides and release documents, complete changelogs, forbidden-file exclusions, and synchronized loose mirror.
- The installed tray process was not stopped or replaced. Therefore control of a real running v0.2.28 instance through `-refresh`/`-exit` and a resulting authenticated `Refresh.Success` line remain post-upgrade acceptance checks; the compiled command output, named-event transport, refresh formatter, UI-thread dispatch contract, and normal automated refresh paths were exercised without using the user's live account.

## 0.2.27 status-text and rounded-window correction - 2026-09-25

- `TrayMenuTextSinglePassContract` verifies status rows do not request an empty native shortcut render pass, rejects non-label callbacks in the custom renderer, and compares the left-side rendered pixels even when the framework callback is deliberately re-enabled.
- `TrayMenuRoundedWindowRegion` verifies matching rounded clipping on both a main menu and submenu, rebuilds the region after resize, restores square geometry for the High Contrast path, and repeats the lifecycle 40 times without GDI growth beyond the allowed process noise.
- Production WinForms renders under `artifacts\verification\v0.2.27-menu` cover English/Simplified Chinese, Light/Dark, CLI/WebView2, and 125%/150%/200% font-scale cases. These are offscreen production-control renders; the current Computer Use surface exposed no native app inventory, so live notification-area placement and hardware-DPI behavior were not verified in this pass.
- The final packaging gate completed with 0 errors and two restricted-network `NU1900` warnings; all 93 automated tests passed. The 100-session resource check finished at Handles `343 -> 340`; menu-region lifecycle stayed within the +4-object guard and the final icon GDI loop remained `18 -> 18`.
- Package: `artifacts\packages\CodexUsageTrayLite-v0.2.27-win-x64.zip`; SHA-256 `f0024de1471dbe33e1ddf969184e229d6c75b2af4bb17bc01e371e1574524fd0`. The independent audit verified the exact allowlist, all 21 internal manifest hashes, external sidecar, EXE file version `0.2.27.0`, bilingual READMEs, complete changelogs, forbidden-file exclusions, and synchronized loose mirror.

## 0.2.26 About version-prefix correction - 2026-09-24

- The Tools & Help regression requires the About secondary text to equal `v` plus `AppConstants.Version`; for this build the visible value is `v0.2.26`.
- Production WinForms renders under `artifacts\validation\menu-r9` cover the corrected summary in English/Simplified Chinese, Light/Dark, CLI/WebView2, and 125%/150%/200% font-scale cases. These are offscreen production-control renders, not a live notification-area capture.
- The final packaging gate completed with 0 errors and two restricted-network `NU1900` warnings; all 91 automated tests passed. The 100-session resource check finished at Handles `343 -> 340`; GDI objects remained `17 -> 17`.
- Package: `artifacts\packages\CodexUsageTrayLite-v0.2.26-win-x64.zip`; SHA-256 `0be4ede80892319e11acf57b7518460ba9580d149e11c69c7e977ebad3033620`. The independent audit verified the 22-file allowlist, all 21 internal manifest hashes, external sidecar, EXE file version `0.2.26.0`, bilingual READMEs, source URL, forbidden-file exclusions, and synchronized loose mirror.

## 0.2.25 About version-prefix refinement - 2026-09-24

- The existing Tools & Help regression now requires the About secondary text to equal `v.` plus `AppConstants.Version`; for this build the visible value is `v.0.2.25`.
- Production WinForms renders under `artifacts\validation\menu-r8` cover the prefixed summary in English/Simplified Chinese, Light/Dark, CLI/WebView2, and 125%/150%/200% font-scale cases. These are offscreen production-control renders, not a live notification-area capture.
- The final packaging gate completed with 0 errors and two restricted-network `NU1900` warnings; all 91 automated tests passed. The 100-session resource check finished at Handles `343 -> 340`; GDI objects remained `17 -> 17`.
- Package: `artifacts\packages\CodexUsageTrayLite-v0.2.25-win-x64.zip`; SHA-256 `11756305831a2204f7c9d7b35261b71bdda616e5421266e9b45e98b2497bbdb9`. The independent audit verified the 22-file allowlist, all 21 internal manifest hashes, external sidecar, EXE file version `0.2.25.0`, bilingual READMEs, source URL, complete changelog, forbidden-file exclusions, and synchronized loose mirror.

## 0.2.24 GitHub update menu and version summary - 2026-09-24

- `TrayMenuReorganizationAndTheme` verifies the Tools & Help order `OpenLogs|OpenConfig||Help|Update|About`, the `GitHub` Update summary, the current-version About summary, and native secondary-text width reservation for both rows.
- `ExternalUrlOpenLaunch` verifies that the exact project `/releases/latest` URL uses the Windows shell/default browser and rejects non-HTTPS or malformed addresses without actually opening a browser.
- Resource-key parity and localized-text contracts cover Update and its failure message in English, Simplified Chinese, Traditional Chinese, Japanese, and Korean.
- Production WinForms renders under `artifacts\validation\menu-r7` cover the Tools & Help submenu in English/Simplified Chinese, Light/Dark, CLI/WebView2, and 125%/150%/200% font-scale cases. These are offscreen production-control renders, not a live notification-area or browser-launch test.
- The final packaging gate completed with 0 errors and two restricted-network `NU1900` warnings; all 91 automated tests passed. The 100-session resource check finished at Handles `343 -> 340`; GDI objects remained `17 -> 17`.
- Package: `artifacts\packages\CodexUsageTrayLite-v0.2.24-win-x64.zip`; SHA-256 `c0326c091071d08d078ac38c29be8134910ba3df2658993e422f44efa6ab7e7a`. The independent audit verified the 22-file allowlist, all 21 internal manifest hashes, external sidecar, EXE file version `0.2.24.0`, bilingual READMEs, source URL, complete changelog, forbidden-file exclusions, and synchronized loose mirror.

## 0.2.23 main-menu left-edge alignment - 2026-09-24

- The target-machine screenshot showed two text baselines: status and summary-bearing rows started approximately five pixels farther left than ordinary command rows.
- Source inspection traced the difference to the custom renderer's `TextFormatFlags.NoPadding`. Standard WinForms menu text retains native `TextRenderer` glyph padding, while the custom status/summary path explicitly removed it.
- The custom left-label path now uses the same native glyph padding and also treats ampersands in read-only values literally. Right-aligned summaries and the item geometry used for submenu placement are unchanged.
- `TrayMenuTextAlignmentContract` prevents the custom path from restoring `NoPadding`; the existing native submenu-overhang regression continues to protect the v0.2.22 zero-gap fix.
- Production WinForms renders under `artifacts\validation\menu-r5` visually confirm a common left edge in English/Simplified Chinese, Light/Dark, WebView2/CLI, and 125%/150%/200% long-email font-scale cases. These are offscreen production-control renders, not a live notification-area or hardware-DPI capture.
- The final packaging gate completed with 0 errors and two restricted-network `NU1900` warnings; all 90 automated tests passed. The 100-session resource check finished at Handles `343 -> 340`; GDI objects remained `17 -> 17`.
- Package: `artifacts\packages\CodexUsageTrayLite-v0.2.23-win-x64.zip`; SHA-256 `bea44342146548a267e1f3e193109ae06b475bfae2b5b4ca2bb7d6df04975caf`. The independent audit verified the 22-file allowlist, all 21 internal manifest hashes, external sidecar, EXE file version `0.2.23.0`, bilingual READMEs, current source URL, complete changelog, forbidden-file exclusions, and synchronized loose mirror.

## 0.2.22 zero-gap submenu edge alignment - 2026-09-24

- Target-machine evidence showed that v0.2.21 reduced the large submenu offset but retained an approximately 11-pixel gap. The remaining offset matched the extra horizontal `Margin` and `Padding` applied to each menu item by `ApplyMenuLayout`.
- The regression baseline is now a truly unmodified `ContextMenuStrip` / `ToolStripMenuItem`; its parent item ends one pixel inside the client edge, which produces the normal border overlap in WinForms' right-cascade calculation.
- Before the spacing correction, `TrayMenuReorganizationAndTheme` failed with the summary parent item 11 pixels beyond that native baseline. After removing item-level horizontal margin/padding, all five summary-bearing rows match the native overhang for both normal-email and long-email menus.
- Renderer-managed text placement, smaller summary text, row height, separator insets, and rounded selection-background insets retain the intended R2 breathing room without changing the item bounds used for submenu positioning.
- Production WinForms renders under `artifacts\validation\menu-r4` confirm the main-menu appearance, summaries, checks, and submenus remain visually correct in English/Simplified Chinese, Light/Dark, and the 200%-font long-email case.
- The final packaging gate completed with 0 errors and only the two known restricted-network `NU1900` warnings, then passed 89 automated tests, 0 failed. The 100-session resource check finished at Handles `338 -> 340`; GDI objects remained `17 -> 17`.
- Package: `artifacts\packages\CodexUsageTrayLite-v0.2.22-win-x64.zip`; SHA-256 `13053bc6d156c6a2637d38a3c16913dd4def1a4ff740aa5a933ceed5e3702ae1`. The independent audit verified the 22-file allowlist, all 21 internal manifest hashes, external sidecar, EXE file version `0.2.22.0`, bilingual READMEs, source URL, complete changelog, and synchronized loose mirror.

## 0.2.21 partial submenu parent-width correction - 2026-09-23

- The official WinForms implementation positions a right-cascading submenu by adding the parent item's `Width` to its screen origin. Its `ToolStripDropDownMenu` sizing path calculates menu width from native text and secondary-text metrics rather than the custom `GetPreferredSize` width used in v0.2.19.
- A before-fix regression run reproduced the defect: `TrayMenuReorganizationAndTheme` failed because the summary-bearing `UsageSource` item exceeded the native parent-menu overhang baseline. Removing the custom width addition and reserving summary width through the native secondary-text metric makes the same test pass.
- The custom renderer suppresses the native secondary-text paint pass and draws the smaller summary exactly once; production WinForms renders under `artifacts\validation\menu-r3` confirm no duplicate summary in English/Simplified Chinese, Light/Dark, normal and 200%-font long-email cases.
- The initial regression compared all five summary-bearing rows against a baseline that still had `ApplyMenuLayout` spacing. Target-machine verification found the resulting approximately 11-pixel residual gap; `0.2.22` replaces it with a truly unmodified native baseline.
- The final packaging gate completed with 0 errors and only the two known restricted-network `NU1900` warnings, then passed 89 automated tests, 0 failed. The 100-session resource check finished at Handles `343 -> 340`; GDI objects remained `17 -> 17`.
- Package: `artifacts\packages\CodexUsageTrayLite-v0.2.21-win-x64.zip`; SHA-256 `3e3f146db71bbb688c7f53acfcee74421716485924aefc49044d8dc5765a359b`. The independent audit verified the 22-file allowlist, all 21 internal manifest hashes, external sidecar, EXE file version `0.2.21.0`, bilingual READMEs, source URL, complete changelog, and synchronized loose mirror.

## 0.2.20 attempted adjacent submenu placement - 2026-09-23

- This version attempted an explicit `Right`/`Left` direction override. Target-machine verification showed the gap unchanged because the parent item width still exceeded the visible menu; the attempt is superseded by `0.2.21`.
- The final packaging gate completed with 0 errors and only the two known restricted-network `NU1900` warnings, then passed all 89 automated tests, 0 failed. The 100-session resource check finished at Handles `338 -> 340`; GDI objects remained `18 -> 18`.
- Package: `artifacts\packages\CodexUsageTrayLite-v0.2.20-win-x64.zip`; SHA-256 `f94ab8c1f0e046d5e1e621b4e8943b0e6c8593ffedbba8ac24a6c8106dbf18d1`. The independent audit verified the 22-file allowlist, all 21 internal manifest hashes, external sidecar, EXE file version `0.2.20.0`, bilingual READMEs, current source URL, full changelog, and synchronized loose mirror.
- Windows Computer Use inventory failed after the documented retry and session reset (`apps=[]`, `nodeRepl.fetch request failed`). The fix therefore has build/test coverage but no live notification-area placement claim in this run; the target desktop should confirm the submenu touches the parent edge on both normal and screen-edge paths.

## 0.2.19 tray-menu visual refinement R2 - 2026-09-23

- `TrayMenuReorganizationAndTheme` verifies the unchanged 15-row hierarchy and command visibility plus the new 34-logical-pixel minimum row height, independent smaller summary font, separate secondary/disabled palette colors, rounded renderer, and summary text that no longer uses the shortcut-key column.
- The production renderer keeps status hierarchy: primary account/quota rows use the main theme color, reset/update rows use the secondary color (`#535963` Light and `#C0C3CA` Dark), and genuinely disabled actions use the disabled color.
- The renderer draws soft single separators, rounded selection highlights, a rounded painted border, theme-aware arrows, and a standalone check mark without the prior block background. Native `ContextMenuStrip` ownership, keyboard handling, submenus, and working-area height constraints remain unchanged.
- `artifacts\validation\menu-r2` contains production WinForms offscreen renders for English and Simplified Chinese in Light and Dark, WebView2 and CLI, main/selected rows, Usage source, Refresh interval, Icon style, and Tools & Help submenus. Long-email variants exercise 125%, 150%, and 200% font scaling without clipping; these are font-scale simulations on the production controls, not actual per-monitor hardware-DPI captures.
- The final packaging gate completed with 0 errors and only the two known restricted-network `NU1900` warnings, then passed all 89 automated tests, 0 failed. The 100-session resource check finished at Handles `343 -> 345`; GDI objects remained `18 -> 18`.
- Package: `artifacts\packages\CodexUsageTrayLite-v0.2.19-win-x64.zip`; SHA-256 `8f105203eaf7a208890e67c1e3ff06863808232d08d8456aaaf30a6d89b975c6`. An independent audit verified the 22-file allowlist, all 21 internal manifest hashes, external sidecar, EXE file version `0.2.19.0`, both READMEs, dynamic source URL, complete changelog cutover, and the synchronized version-neutral loose mirror.
- No authenticated account refresh, live notification-area menu interaction, physical 100%/125%/150%/200% monitor-DPI switch, or long-duration run is claimed for this menu-only change. Windows still controls the outer drop-down window shadow and final host-window corner behavior.

## 0.2.18 bilingual portable documentation and package consistency - 2026-09-23

- The versioned package and GitHub release asset convention is `CodexUsageTrayLite-v<version>-win-x64.zip`; current and future package names must not contain `local`. Existing historical archives retain their original immutable names.
- The portable package now contains `README.txt` and a complete Simplified Chinese `README.zh-CN.txt`. Both are generated from versioned templates and must point to `tree/v<current-version>` with no unresolved template tokens.
- Both packaged changelogs retain the complete history. The packaging gate requires `0.1.0-local` and `0.2.15-local`, requires public headings for `0.2.16`, `0.2.17`, and the current version, and rejects `0.2.16-local` or `0.2.17-local` headings.
- The packaging gate completed with 0 errors and only the two known restricted-network `NU1900` warnings, then passed all 89 automated tests, 0 failed. The 100-session lifecycle finished at Handles `338 -> 340`; GDI objects remained `17 -> 17`.
- Package: `artifacts\packages\CodexUsageTrayLite-v0.2.18-win-x64.zip`; SHA-256 `183482d416c560865807514dea25b0e44874da86a82afde39d2e5dea5f8580c9`. An independent post-build audit verified the 22-file ZIP allowlist, all 21 internal manifest hashes, external sidecar, EXE file version `0.2.18.0`, both READMEs, dynamic source URLs, changelog cutover markers, and the synchronized version-neutral loose mirror.
- This release changes documentation and packaging rules only. Usage retrieval, menus, settings, refresh timing, proxy behavior, quota parsing, process lifecycle, and icon rendering are unchanged from `0.2.17`; the earlier production-WinForms menu renders therefore remain the applicable visual evidence.

## 0.2.17 tray-menu reorganization R1 - 2026-09-23

- `TrayMenuReorganizationAndTheme` verifies the fixed 15-item top-level order, five read-only status rows, merged email/level row, WebView2-only maintenance visibility and separators, Tools & Help grouping, CLI login-help label, and the distinction between the selected source and `lastSuccessfulUsageSource`.
- Usage source, Refresh interval, Icon style, Theme, and Language use right-aligned display summaries with `ShortcutKeys == Keys.None`. Start at login retains its native check state and the menu remains keyboard-navigable with no third level.
- Primary read-only status rows are rendered with the theme text color despite remaining disabled/non-clickable; reset count and last updated use muted text. A separate assertion proves a normal disabled item still resolves to the disabled color.
- `artifacts\validation\menu-r1` contains production `TrayApplicationContext` / `AppToolStripRenderer` snapshots for English and Simplified Chinese in Light and Dark, WebView2 and CLI source submenus, Tools & Help, Icon style, plus an English Dark 150%-font long-email case. The retained HTML previews were not used as implementation proof.
- Computer Use could not expose any native Windows app windows after two inventory attempts and a helper reset (`apps=[]`; browser inventory fetch failed). Consequently, these final screenshots are deterministic production-WinForms renders rather than a live notification-area capture. No account refresh, WebView2 login, or long-duration run was performed for this menu-only release.
- The packaging gate completed with 0 errors and only the two known restricted-network `NU1900` warnings, then passed all 89 automated tests, 0 failed. The 100-session lifecycle finished at Handles `343 -> 340`; GDI objects remained `17 -> 17`.
- Package: `artifacts\packages\CodexUsageTrayLite-v0.2.17-win-x64.zip`; SHA-256 `f69588ab6617a53193c52408bc860e28be944d81180b567196ade7a45d6f785e`. The 21-file ZIP allowlist, 20-file internal manifest, external sidecar, four localization satellites, and synchronized version-neutral loose mirror all verified.

## 0.2.16-local full-width Weekly fallback - 2026-09-19

- Side by Side now expands Weekly across the complete inner frame only when `fiveHourLimitApplies == false`. Applicable accounts retain the R2 split geometry; `null` applicability still becomes Unknown.
- `SideBySideWeeklyFallback` checks Weekly 0/1/20/21/50/51/100% at 16/20/24/32 pixels in Light and Dark, validates every inner pixel and the neutral frame, and requires exact bitmap equality with Weekly Only.
- `docs\icon-options\side-by-side-v0.2.16.png` was generated from the production renderer and visually inspected. The `No 5-Hour` column now uses full-width Weekly, while all applicable columns retain the approved R2 split geometry. This is a synthesized renderer check, not a live notification-area capture.
- The final packaging gate completed with 0 errors and only the two known restricted-network `NU1900` warnings, then passed all 88 automated tests, 0 failed. The 100-session lifecycle finished at Handles `343 -> 340`; GDI objects remained `14 -> 14`.
- Package: `artifacts\packages\CodexUsageTrayLite-v0.2.16-local-win-x64.zip`; SHA-256 `2c1c304c5d11246b401f81c0aa0e7ddd099087ed6f46d97790e48f7bb7fd48a6`. The 16-file ZIP allowlist, 15-file internal manifest, external sidecar, four localization satellites, and synchronized version-neutral loose mirror all verified.

## 0.2.15-local Side-by-Side quota layout - 2026-09-19

- The new persisted `SideBySide` mode keeps Both as the default, reuses Both Tooltip rules, and is localized as `Side by Side`, `并排双条`, `並排雙條`, `左右並列`, and `나란히 표시`.
- The pixel matrix checks every interior pixel for all combinations of 0/1/20/21/50/51/100% at 16/20/24/32 pixels in Light and Dark. It proves Weekly-left and 5-Hour-right complete half-widths, bottom-up independent heights, no center seam or padding, theme blue on the right, independent Weekly thresholds, and 5-Hour-only frame thresholds.
- State/cache coverage verifies authoritative no-5-Hour as a neutral empty right half, ambiguous applicability as Unknown, unchanged `?`/slash/`L`, hidden-value cache isolation, layout cache isolation, and both threshold transitions. Existing GDI and 100-session lifecycle checks remain active.
- `docs\icon-options\side-by-side-v0.2.15.png` is generated by the production renderer and was visually compared with the approved `docs\icon-options\side-by-side-tray-r2\side-by-side-tray-preview.png`. R1/R2 source previews remain untouched. This is a synthesized renderer comparison, not a live notification-area capture.
- The final packaging gate completed with 0 errors and only the two known restricted-network `NU1900` warnings, then passed all 87 automated tests, 0 failed. The 100-session lifecycle finished at Handles `338 -> 340`; GDI objects remained `14 -> 14`.
- Package: `artifacts\packages\CodexUsageTrayLite-v0.2.15-local-win-x64.zip`; SHA-256 `ad8854de6250de4c89b9f115a7c133c4ca31783653c5dab1c04fe1333cd5a5bd`. The 16-file ZIP allowlist, 15-file internal manifest, external sidecar, four localization satellites, and synchronized version-neutral loose mirror all verified.

## 0.2.14-local enlarged red state glyphs - 2026-09-14

- `i`, `?`, and `L` are generated from bold Segoe UI outlines and uniformly fitted to the largest inset rectangle that avoids edge clipping; no glyph is stretched on either axis.
- Dedicated coverage checks a solid `#f04444` core and minimum occupied height for all three glyphs at 16, 20, 24, and 32 pixels in both Light and Dark rendering. The existing state-priority, distinct-hash, cache, threshold, and GDI lifecycle checks remain active.
- `docs\icon-options\quota-icon-modes-v0.2.14.png` was generated from the final renderer and visually inspected. The 16-pixel glyphs remain distinct and uncropped; this is a synthesized renderer check, not a live notification-area capture.
- The final packaging gate completed with 0 errors and only the two known `NU1900` warnings caused by unavailable NuGet vulnerability-index access, then passed all 85 automated tests, 0 failed. The 100-session lifecycle finished at Handles `344 -> 347`; GDI objects remained `14 -> 14`.
- Package: `artifacts\packages\CodexUsageTrayLite-v0.2.14-local-win-x64.zip`; SHA-256 `2f489f6a75b3b7d88409ba3ca2dff097aa388e8bcaed7bcba2a2ba4519b7d2ac`. The 16-file ZIP allowlist, 15-file internal manifest, external sidecar, four localization satellites, and synchronized version-neutral loose mirror all verified.

## 0.2.13-local authoritative Pro 5-Hour conflict handling - 2026-09-14

- Regression coverage now proves that the Pro `i`/`N/A` path requires confirmed non-applicability. A valid applicable Pro 5-Hour value remains visible in the renderer, 5-Hour Only Tooltip, and menu.
- The 84-test suite continues to cover all three icon modes, setting migration/persistence, five-language resources, Tooltip status/length, applicable/non-applicable/unknown identity, warning thresholds, themes, DPI sizes, cache isolation, and special-state/resource lifecycle behavior.
- `docs\icon-options\quota-icon-modes-v0.2.13.png` is generated from the final renderer. As in 0.2.12, live Windows tray automation remains unverified because the approved Computer Use inventory repeatedly failed with `nodeRepl.fetch request failed`; the existing installed process was not interrupted.
- The final packaging gate built x64 Release with 0 warnings and 0 errors, then passed all 84 automated tests, 0 failed. The 100-session lifecycle finished at Handles `373 -> 371`; GDI objects remained `14 -> 14`.
- Package: `artifacts\packages\CodexUsageTrayLite-v0.2.13-local-win-x64.zip`; SHA-256 `3c3644030792a065e4469991dd866eb00c7aefc2495de4fa1a1456076da2ed3c`. The 16-file ZIP allowlist, 15-file internal manifest, external sidecar, four localization satellites, and synchronized version-neutral loose mirror all verified.

## 0.2.12-local persistent quota icon modes - 2026-09-14

- Settings coverage verifies new-install Both, schema-4 migration to Both without reusing or overwriting the legacy A-D `iconStyle`, invalid-value normalization, JSON persistence, and the settings-schema increment to 5.
- Tooltip coverage verifies 5-Hour Only values and `N/A`, cached/error/login suffixes, preserved Both and Weekly Only behavior, unknown applicability, and the 63-character limit. Confirmed Pro Weekly-only identity requires explicit Pro plus `fiveHourLimitApplies=false`; valid 5-Hour evidence is not discarded.
- Renderer coverage crosses Both/Weekly Only/5-Hour Only with explicit applicable/not-applicable/unknown states, normal/cached/error/login behavior, 0/20/50/100 thresholds, Light/Dark, 16/20/24/32 pixels, the Pro `i`, hidden-layer warning isolation, cache-key isolation, and special-state priority.
- `docs\icon-options\quota-icon-modes-v0.2.12.png` was generated from the actual renderer and visually inspected at all supported test sizes. The `i` remains distinct from Unknown `?`, FetchError slash, and LoginRequired `L`; this is a synthesized renderer check, not a live notification-area capture.
- Live tray automation was attempted only through the approved Computer Use interface. Two inventory attempts plus a clean session reset all failed with `nodeRepl.fetch request failed` and returned no Windows applications. The already-running installed instance was therefore left untouched; menu clicking, live Tooltip hover, and restart persistence are not claimed as verified in this run.
- The final packaging gate built x64 Release with 0 warnings and 0 errors, then passed all 84 automated tests, 0 failed. The 100-session lifecycle remained within its bounded allowance at Handles `376 -> 379`; GDI objects remained `14 -> 14`.
- Package: `artifacts\packages\CodexUsageTrayLite-v0.2.12-local-win-x64.zip`; SHA-256 `0a5101769abe4ceeff7922c6a63c6d0e93f9cc797aa2cd0b3a84d97c8c6d273b`. The 16-file ZIP allowlist, 15-file internal manifest, external sidecar, four localization satellites, and synchronized version-neutral loose mirror all verified.

## 0.2.11-local Plus/Pro 5-Hour distinction - 2026-09-11

- The account-level regression now treats Plus and Pro separately. A Plus snapshot with an applicable 5-Hour window retains both Tooltip lines and its actual menu value; Pro omits the Tooltip's 5-Hour line and keeps `5-Hour N/A` in the menu.
- Free and Go continue to retain applicable 5-Hour data. Independently, authoritative Weekly-only data continues to omit the Tooltip row and use menu `N/A`.
- The pre-package Release x64 build passed with 0 errors and only the two expected restricted-network `NU1900` warnings. All 80 automated tests passed, 0 failed; the 100-session resource loop held Handles at `355 -> 355`, and GDI objects remained `14 -> 14`.
- The packaging gate repeated all 80 tests with Handles `357 -> 357` and GDI objects `14 -> 14`. Package: `artifacts\packages\CodexUsageTrayLite-v0.2.11-local-win-x64.zip`; SHA-256 `7bd9897d081d7199287dca02a9ca5e74c451ceeab03e22e70b5327fa1186f802`. The 16-file ZIP allowlist, 15-file internal manifest, external sidecar, four localization satellites, and synchronized version-neutral loose mirror all verified.

## 0.2.10-local account status, quota-only Tooltip, and Help layout - 2026-09-11

- The Tooltip regression contract contains no email or source tag: dual-window data produces exactly the 5-Hour and Weekly lines, while explicit Plus/Pro and authoritative Weekly-only data produces Weekly only. All cases remain within the 63-character NotifyIcon limit and preserve cached/error/login status.
- The menu formatter always retains 5-Hour, uses `N/A` for Plus/Pro and Weekly-only snapshots, and gives Weekly the exact same percentage/reset/status text as the Tooltip. The status ordering is Email, User Level, 5-Hour, Weekly, Usage resets, and Last updated.
- Account-level fixtures cover CLI `account/read.planType`, the CLI rate-limit fallback, supported WebView2 session fields, exact standalone rendered-page badges, Free/Go/Plus/Pro normalization, unsupported Unknown behavior, and the explicit omission of Pro 5x/20x probing.
- Help resources are normalized from LF/CR/LF mixtures to Windows CRLF before assignment. The visible multiline TextBox must expose at least 20 separate lines instead of one wrapped block, while the copied content remains unchanged.
- The pre-package Release x64 build passed with 0 errors and the two expected restricted-network `NU1900` warnings. All 80 automated tests passed, 0 failed; the 100-session resource loop held Handles at `355 -> 355`, and GDI objects remained `14 -> 14`.
- The packaging gate repeated all 80 tests with Handles `357 -> 357` and GDI objects `14 -> 14`. Package: `artifacts\packages\CodexUsageTrayLite-v0.2.10-local-win-x64.zip`; SHA-256 `138ce833205d15178c11c10422b4aed3f0eccfe7fe33bcccbc5cd5f9cb2e815e`. The 16-file ZIP allowlist, 15-file internal manifest, external sidecar, four localization satellites, and synchronized version-neutral loose mirror all verified.

## 0.2.9-local Japanese/Korean UI and live multilingual WebView2 validation - 2026-09-11

- Authenticated ChatGPT Usage pages were switched with Computer Use and inspected in Traditional Chinese (Hong Kong), Traditional Chinese (Taiwan), Japanese, and Korean. The account website language was restored to Simplified Chinese after collection.
- Exact page-shaped fixtures cover each locale's Weekly-only Pro card, reset timestamp, Usage-limit-resets heading, zero-value available tab, reset description, and next-section boundary. The parser and read-only DOM probe never click a reset and retain Unknown/ParseError for unsupported semantics.
- The application now has five UI choices backed by the neutral English resource and `zh-CN`, `zh-TW`, `ja-JP`, and `ko-KR` satellites. Tests verify complete/equal key sets, explicit culture mapping, Japanese/Korean Windows first-run detection, saved-language retention, dialogs, and unchanged English Tooltips.
- The pre-package Release x64 build passed with 0 errors and the two expected `NU1900` warnings because the restricted environment could not reach the NuGet vulnerability index. All 79 automated tests passed, 0 failed; the 100-session resource loop held Handles at `355 -> 355`, and GDI objects remained `14 -> 14`.
- The final 0.2.9 executable was not started against the authenticated profile while the user's 0.2.8 instance owned it. The final parser/resources are covered by the exact fixtures and binary tests; one post-upgrade background refresh remains the end-to-end acceptance check.
- Package: `artifacts\packages\CodexUsageTrayLite-v0.2.9-local-win-x64.zip`; SHA-256 `d264c4afe66ef34b64a9b7ae42ee8b2f25f69a2f2a4f7bac7db8a915d9289dfe`. The 16-file ZIP allowlist, 15-file internal manifest, external sidecar, four localization satellites, and synchronized version-neutral loose mirror all verified.

## 0.2.6-local WebView2 `Available N` reset-tab validation - 2026-09-10

- The packaging Release x64 build passed with 0 errors and two `NU1900` warnings because the online vulnerability index was unavailable. All 74 automated tests passed, 0 failed; the 100-session resource loop held Handles at `351 -> 351`, and GDI objects remained `14 -> 14`.
- The updated fixture reproduces the user's authenticated Pro page: `Usage limit resets`, its description, `Available 0`, History, Past 30 days, and dated `Reset used` / `Reset received` rows. It resolves to zero without counting History dates.
- Additional assertions cover `Available (2)`, Simplified/Traditional-Chinese `可用 5`, Korean `사용 가능 6`, and rejection of `Available 7` outside the recognized reset section. The DOM script contains the same exact, normalized, section-bounded patterns.
- Existing v0.2.5 runtime logs show successful Usage refreshes taking roughly 7.3-7.7 seconds while the old reset probe exhausted its retry interval. The screenshot supplies the previously missing real-page wording.
- The running v0.2.5 instance owned the authenticated WebView2 profile, so v0.2.6 did not start a competing live session; the user's post-upgrade refresh remains the end-to-end acceptance check.
- Package: `artifacts\packages\CodexUsageTrayLite-v0.2.6-local-win-x64.zip`; SHA-256 `992938ca6a44d0eb1fe7f4e0107cebcbbf636de4d1ccc5f7f78690c93889e4cc`. The ZIP allowlist, 13-file internal manifest, external sidecar, and synchronized version-neutral loose mirror all verified.

## 0.2.5-local WebView2 reset-probe failure safeguard - 2026-09-10

- The v0.2.4 Weekly-only/absent-section fallback is retained, but production code now records whether the DOM probe exited normally or threw. Only a normally completed no-result probe may continue to the final page-section check.
- The existing fallback regression test now also proves a failed probe stays Unknown even for a Weekly-only page with no recognized reset section. The total remains 74 tests because the new assertion extends the same behavioral contract.
- The packaging Release x64 build passed with 0 errors and two `NU1900` warnings because the online vulnerability index was unavailable. All 74 automated tests passed, 0 failed; the 100-session resource loop held Handles at `351 -> 351`, and GDI objects remained `14 -> 14`.
- The running 0.2.3 tray instance owned the authenticated WebView2 profile, so the 0.2.5 executable did not start a competing live profile session; the user's post-upgrade refresh remains the end-to-end acceptance check.
- Package: `artifacts\packages\CodexUsageTrayLite-v0.2.5-local-win-x64.zip`; SHA-256 `39513d3e4f7c2fd074ef1b643b6a827fd231b377dea0fecfae602e1cf407a86c`. The ZIP allowlist, 13-file internal manifest, external sidecar, and synchronized version-neutral loose mirror all verified.

## 0.2.4-local WebView2 Weekly-only zero-reset validation - 2026-09-10

- The packaging Release x64 build passed with 0 errors and two `NU1900` warnings because the online vulnerability index was unavailable. All 74 automated tests passed, 0 failed; the 100-session resource loop held Handles at `351 -> 351`, and GDI objects remained `14 -> 14`.
- The new fallback test proves that, after the production DOM retry path is exhausted, a confirmed Weekly-only snapshot plus a final page with no recognized reset-section heading maps to zero. A recognized but ambiguous section, a normal dual-window plan, and a missing snapshot remain Unknown.
- The user's 0.2.3 runtime logs show repeated successful WebView2 refreshes taking approximately 7.5 seconds, consistent with the missing-5-hour confirmation and full reset DOM-probe windows completing without a reset result. A separate current-account read-only check confirmed `availableCount=0`.
- The running 0.2.3 tray instance owned the authenticated WebView2 profile, so the 0.2.4 executable did not start a competing live profile session; the user's post-upgrade refresh remains the end-to-end acceptance check.
- Package: `artifacts\packages\CodexUsageTrayLite-v0.2.4-local-win-x64.zip`; SHA-256 `42e11ed4466d116636d55b561fa38d677b42931904917a8aa4418e05e5a56b26`. The ZIP allowlist, 13-file internal manifest, external sidecar, and synchronized version-neutral loose mirror all verified.

## 0.2.3-local Weekly-only plan validation - 2026-09-10

- The packaging Release x64 build passed with 0 errors and two `NU1900` warnings because the online vulnerability index was unavailable. All 73 automated tests passed, 0 failed; the 100-session resource loop held Handles at `351 -> 351`, and GDI objects remained `14 -> 14`.
- The new fixtures cover the observed general-Codex response with one 10,080-minute Weekly window and no secondary window, WebView2 pages with an absent or explicitly Unlimited 5-hour card, reset-description exclusion, three additional delayed-render confirmation reads, cache compatibility, `5-Hour N/A`, and Weekly-only icon pixels.
- A direct read-only current-account usage check confirmed one 10,080-minute window and no secondary window. Existing application logs independently show both CLI and WebView2 returning ParseError after the account-plan change, which isolates the old common assumption that both quota windows must exist.
- The installed Codex CLI currently reports `Not logged in`, so the new live CLI path could not complete an authenticated end-to-end probe; run `codex login` before manual CLI acceptance. The user's running older tray instance owns the authenticated WebView2 profile, so no competing live WebView2 probe was started.
- Package: `artifacts\packages\CodexUsageTrayLite-v0.2.3-local-win-x64.zip`; SHA-256 `dd4838d4b8ef5e41f524008ecc5b1a94b961199f333810f31b21916182dfab5b`. The ZIP allowlist, 13-file internal manifest, external sidecar, and synchronized version-neutral loose mirror all verified.

## 0.2.2-local multilingual WebView2 parsing validation - 2026-09-10

- 70 tests passed, 0 failed during packaging; Release x64 build passed with 0 errors and two `NU1900` warnings because the online vulnerability index was unavailable. The 100-session resource loop held Handles at `352 -> 352`, and GDI objects remained `14 -> 14`.
- Parser fixtures cover English, Simplified Chinese, Traditional Chinese, and Korean 5-hour/weekly labels; remaining and used directions; localized reset markers and dates; time-only next-day rollover; mixed-language rows; NFKC-normalized full-width digits and punctuation; explicit 0/1/multiple available-reset states; ambiguous Unknown behavior; and Codex Spark exclusion.
- The DOM-probe contract verifies localized section/action/boundary vocabulary, NFKC normalization, a minimum three-second delayed-render retry window, strict visible/enabled action counting, and explicit-zero handling. The extracted embedded JavaScript also passed a Node syntax compilation check.
- The user's running tray instance kept the authenticated WebView2 profile locked, so the release did not start a competing profile session or switch the account's website language. Per-language live authenticated-page verification remains manual.
- Package: `artifacts\packages\CodexUsageTrayLite-v0.2.2-local-win-x64.zip`; SHA-256 `300502b4aa46694c548033ad0cc25cffeecdea2475bd3bd8ec7d993926f91ad1`. The ZIP allowlist, internal manifest, external sidecar, and synchronized version-neutral loose mirror all verified.

## 0.2.1-local localization-resource validation - 2026-09-09

- 65 tests passed, 0 failed; Release x64 build passed with 0 errors and two `NU1900` warnings because the online vulnerability index was unavailable.
- Resource coverage verifies the neutral English `.resx` embedded in the application, compiled `zh-CN` and `zh-TW` satellite assemblies, matching key sets across all three cultures, explicit culture mapping, existing localized menu/dialog contracts, and unchanged English Tooltip output.
- The package allowlist now requires both satellite assemblies. The package manifest and synchronized loose mirror each verified 13 payload hashes.
- Relative to 0.2.0, the ZIP grew by 10,358 bytes compressed; WebView2, CLI, timer, icon, and runtime lifecycle behavior are unchanged.

## 0.2.0-local localization validation - 2026-09-09

- Release x64 build passed with 0 errors. Two `NU1900` warnings reported that the online NuGet vulnerability index was unavailable; pinned packages restored and built successfully.
- 64 tests passed, 0 failed. New checks cover non-Chinese/Simplified/Traditional Windows locale mapping, schema 3-to-4 language migration, persisted enum serialization, exact localized status rows and interval labels, all three Help bodies, Simplified Proxy/About, live Simplified-to-Traditional Login-window switching, and the unchanged three-line English Tooltip contract.
- The packaging run held process Handles at `346 -> 346` across the 100-session fake lifecycle and GDI objects at `14 -> 14`.
- Package: `artifacts\packages\CodexUsageTrayLite-v0.2.0-local-win-x64.zip`; SHA-256 `c7f638668ff892ce53d3695b6b72615aaf8691619001006dacc197cbe31fd03c`.

Generate the renderer-backed design sheets with:

```powershell
& '.\tests\CodexUsageTrayLite.Tests\bin\x64\Release\net48\CodexUsageTrayLite.Tests.exe' --generate-icons '.'
```

Run the real WebView2 controller lifecycle probe with a temporary, unauthenticated profile and a local HTML string:

```powershell
& '.\tests\CodexUsageTrayLite.Tests\bin\x64\Release\net48\CodexUsageTrayLite.Tests.exe' --webview-probe 50
```

This test deliberately returns nonzero when the Handle trend exceeds its threshold. It never navigates to ChatGPT, reads credentials, calls `GC.Collect()`, or kills a process.

Run the one-controller lifecycle log probe with the same temporary unauthenticated profile and local HTML, but without applying the rapid-loop Handle gate:

```powershell
& '.\tests\CodexUsageTrayLite.Tests\bin\x64\Release\net48\CodexUsageTrayLite.Tests.exe' --webview-log-probe
```

It must return zero and add `WebView.ControllerCreated`, `WebView.ControllerClose ... processKill=false`, and `WebView.BrowserProcessExited ... exitKind=Normal` to the local app log.

Run the opt-in real Codex CLI source probe only on a machine whose CLI is already signed in:

```powershell
& '.\tests\CodexUsageTrayLite.Tests\bin\x64\Release\net48\CodexUsageTrayLite.Tests.exe' --cli-probe
```

The probe performs the same app-server handshake and rate-limit read as the tray application. It verifies that both required windows were returned, but deliberately prints neither account data nor quota values. Run it outside an isolated build sandbox when validating the user's actual CLI login.

Run the opt-in real WebView2 account/reset probe only after closing every Codex Usage Tray Lite instance so the application's dedicated profile is not locked:

```powershell
& '.\tests\CodexUsageTrayLite.Tests\bin\x64\Release\net48\CodexUsageTrayLite.Tests.exe' --webview-email-probe
```

The probe performs the same authenticated WebView2 fetch as the tray application and requires Usage, email, and an available-reset count to be readable. It prints only boolean success fields, never the email, quota values, or reset count, and it never clicks or consumes a reset. Run it outside an isolated build sandbox when validating the user's actual WebView2 profile.

## 0.1.25-local independent warning validation - 2026-09-07

- 58 tests passed, 0 failed; Release x64 build passed with 0 errors and two NU1900 warnings (online vulnerability index unavailable).
- Color checks cover both themes, four icon sizes, all combinations of 100/51/50/21/20/1/0 percent, neutral special states, and cache transitions/recovery at both thresholds.
- Production renderer preview: [dual-quota-0.1.25.png](docs/icon-options/dual-quota-0.1.25.png). Live Windows taskbar and mixed-monitor DPI validation remain manual.

## 0.1.24-local icon validation - 2026-09-07

- Release build: 0 errors; two NU1900 warnings for the unavailable online vulnerability index.
- Automated harness: 56 passed, 0 failed. Dual-quota reference pixels verified at 16/20/24/32 px in both themes; quota/state handling and bounded icon-cache lifetime passed.
- Production renderer preview: [dual-quota-0.1.24.png](docs/icon-options/dual-quota-0.1.24.png). Live taskbar appearance, theme-switch notifications and mixed-monitor DPI behavior require manual acceptance.
- Existing legacy Option A-D tests and preview assets are retained as historical design coverage; the live tray now calls GetDualIcon.

## Results recorded through 2026-09-06

- Release x64 build: passed, 0 errors. The sandboxed packaging run emitted two `NU1900` warnings because the online NuGet vulnerability index was unavailable; cached pinned packages restored successfully.
- Automated logic/resource harness: 53 passed, 0 failed for `0.1.23-local`, including the screenshot-shaped `No usage limit resets available at this time.` banner, explicit zero-versus-Unknown and section-boundary safeguards, the unshown/no-activate WebView2 host contract, 1280x900 background viewport and delayed DOM-probe contract, CLI/explicit-number available-reset parsing, current-source menu policy, daily log rollover, refresh intervals, tooltip compaction, and process-lifecycle coverage.
- Injected 100-fetch service loop: 100 sessions created and 100 disposed across both source routes; the `0.1.23-local` packaging run held Handle count at `358 -> 358` (within the +12 gate); host working set approximately 46.5 MiB at completion.
- Icon/GDI loop: GDI objects `9 -> 9` after 20 renderer lifetimes covering all percentages and special states.
- Real authenticated CLI probe against the final binary: succeeded in 1.665 seconds with both required windows present and no account/quota values printed. Its log recorded `exitType=graceful-after-stdin-close`, `exitCode=0`, `killAttempted=false`, `jobActiveBeforeClose=0`, and no newly launched `codex` process remained.
- Real authenticated WebView2 probe against the `0.1.21-local` build and the application's existing dedicated profile: succeeded with both Usage and email available plus `usageResetCountRead=true`; it printed no email, quota value, or reset count and invoked no reset action.
- The 0.1.22 authenticated probe was not rerun because an existing tray instance held the same profile. The reset DOM script is unchanged; the temporary-profile probe below validates the new unshown-host controller path and its retained desktop viewport.
- Real temporary-profile WebView lifecycle probe against the 0.1.22 build: the controller navigated while its parent Form remained unshown, JavaScript reported the expected 1280x900 DOM viewport, `CoreWebView2Controller.Close()` returned with `processKill=false`, and the runtime then emitted a normal `BrowserProcessExited` event. The probe used no network, credentials, foreground window, or process kill.
- Plain WinForms control test: 50 Forms changed host Handles `300 -> 326`.
- Real direct-controller loop: all 50 controller create/navigate/detach/`Close` operations completed; final `BrowserProcessExited` event arrived; process-info peak was 1; host Handles `286 -> 475`; host working set about 29.1 MB -> 38.9 MB. **Handle trend gate not passed.**
- WinForms WebView2 Dispose comparison: browser exit succeeded but Handles `290 -> 494`, worse than direct controller.

The real rapid-cycle test proves the owned browser process exits, but it also exposes WebView2 .NET wrapper handles that remain pending normal managed collection in this artificial cold-start loop. Product code uses the lower-growth direct `CoreWebView2Controller.Close()` path and never calls `GC.Collect()`. An 8-12 hour naturally spaced soak is required before claiming long-term Handle stability.

## Functional checklist

### 1. Fresh launch

1. Ensure `%LOCALAPPDATA%\CodexUsageTrayLite` does not exist or move it to a private backup location.
2. Start `CodexUsageTrayLite.exe` without elevation.
3. Confirm a battery icon appears, no desktop Widget/window appears, and no toast/balloon notification appears.
4. Confirm the initial tooltip shows login-required/unknown values and **Refresh now** is disabled.
5. Wait beyond four seconds and beyond the selected refresh interval. Confirm no `Refresh.Start` or `WebView.FetchCreated` log entry appears and no app-owned WebView2 process starts.
6. Change Proxy settings and confirm that action alone still does not start a fetch or enable **Refresh now**.

### 2. First login

1. Right-click and select **Open login / usage page**.
2. Confirm the visible browser opens only then.
3. Complete ChatGPT/OpenAI login, including the preferred OAuth provider if applicable.
4. Confirm the usage page is reachable, then close the window.
5. Confirm the window closes rather than hiding and exactly one validation refresh starts.
6. After that refresh first succeeds, confirm **Refresh now** becomes enabled and the normal periodic timer begins.

### 2a. Codex CLI source

1. Ensure the separately installed Codex CLI can run `codex login` / `/status` successfully.
2. Select **Usage source > Codex CLI**. Confirm one validation fetch starts immediately, no WebView/login window opens, and the tooltip updates after success.
3. Confirm the disabled rows are **Email**, **User Level**, **5-Hour**, **Weekly**, **Usage resets**, and **Last updated** in that order. User Level must show an exact Free/Go/Plus/Pro value or Unknown. Plus must retain its applicable 5-Hour value; Pro must show **5-Hour N/A**. An explicit reset zero must show **Usage resets: 0**; an older CLI that omits the field must show **Usage resets: Unknown**.
4. Confirm **Last updated** ends with `(Codex CLI)`, **Refresh now** stays available for retry, and periodic refresh starts only after the first success.
5. Confirm **Proxy settings (WebView2)** and **Clear WebView2 login session** are hidden in this mode. **Codex CLI login help** must explain that login is CLI-owned and must not log out or alter the CLI account.
6. After refresh, confirm no additional app-server `codex`/`node` process remains. Ordinary Codex desktop/CLI processes that predated the refresh must remain untouched.
7. Select **Usage source > WebView2** and confirm one hidden WebView2 validation fetch starts immediately. Confirm **Proxy settings (WebView2)** and **Clear WebView2 login session** appear, while the CLI help entry is replaced by **Open login / usage page**. If its saved profile is still authenticated, the tooltip must update without opening the login window; only a real login-required result should require **Open login / usage page**. In the rendered **Usage limit resets** section, confirm an exact **Available N** tab supplies N, and each row with an exact **Use reset** action contributes one when that tab is absent. History dates and the descriptive **Use a reset to restore...** sentence must not be counted. An explicit no-resets message maps to 0. After the full probe window, a confirmed Weekly-only page that omits the entire reset section also maps to 0; a recognized but ambiguous section and absent sections on normal dual-window plans remain Unknown.

### 3. Login persistence

1. Exit normally and verify `CodexUsageTrayLite.exe` ends.
2. Restart and wait for the delayed fetch.
3. Confirm usage loads without another sign-in.
4. Do not inspect, copy, upload, or share `webview2-profile`; it contains sensitive session data.

### 4. Exit and restart

Use **Exit** while idle and during a refresh. Verify the timer stops, the controller closes, the tray icon disappears, the EXE ends, and owned WebView2 processes exit after a reasonable delay.

### 5. Background fetch and Refresh now

Confirm no browser window becomes visible. Rapidly click **Refresh now** and verify only one fetch starts (use the log timestamps). Failed attempts must retain the last successful percentages and must not update **Last updated**.

On a machine where System proxy cannot reach ChatGPT, start a refresh and immediately select **Open login / usage page**. The hidden refresh must log `Refresh.CancelRequested` followed by `Refresh.Cancelled`, dispose its controller, and open the visible login window instead of doing nothing. Repeat with **Proxy settings**: it must cancel the hidden refresh and open the dialog instead of showing the old generic busy message. Keep an ordinary Microsoft Edge window open during both checks; it must remain unaffected.

### 5a. Windows screen saver regression

Select WebView2 with a valid login and a 1-minute refresh interval, then leave Windows idle beyond the configured screen-saver timeout. Confirm the screen saver starts and remains active across at least two scheduled refresh times. After returning, verify the log contains matching `WebView.BackgroundHostCreated visible=false activation=disabled`, `WebView.ControllerClose`, and normal browser-exit events. Repeat once with the 15-minute default if practical. No WebView2 fetch may open or activate a window. This elapsed-time Windows integration check is required because the automated contract can verify hidden/no-activate window construction but cannot start the user's configured screen saver safely.

User acceptance for 0.1.22 on Windows 11 confirmed that the screen saver starts normally while WebView2 mode is running. Retain this check for future changes to the background host or controller viewport.

### 6. Refresh intervals

Select each of 1, 2, 5, 10, 15, 30, and 60 minutes. Confirm only one item is checked, `settings.json` changes, the selection survives restart, and successful log entries match the selected cadence. Default on a clean profile must remain 15 minutes.

### 7. Proxy modes

For each test, close the login browser before changing settings:

- **System**: verify normal Windows proxy behavior and no application proxy switch.
- **Direct**: verify a machine that normally needs the proxy cannot fetch, while last-success data remains.
- **Custom HTTP**: test a known local HTTP proxy such as `127.0.0.1:7890`; verify login and fetch.
- **Custom SOCKS5**: test a known local SOCKS5 proxy such as `127.0.0.1:1080`; verify login and fetch.
- **Unavailable proxy**: choose an unused port and confirm the 45-second timeout/network failure path, retained data, `[error]`, and controller disposal.
- Validation: blank host, browser-flag text, port 0, and port 65536 must be rejected.

If `WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS` exists, verify logs say only that external arguments were detected; no argument values may be logged. Application proxy selection must win, and unrelated external arguments must remain effective.

### 8. Login expiry and Clear login session

When login expires, background fetch must show login-required state without opening a browser. For **Clear login session**, answer No once (nothing changes), then Yes. Confirm only `%LOCALAPPDATA%\CodexUsageTrayLite\webview2-profile` is removed, cached usage remains visible with login status, and Edge/Chrome/Firefox/Codex sessions are unaffected.

### 9. Start at login

Toggle the menu and inspect `HKCU\Software\Microsoft\Windows\CurrentVersion\Run\CodexUsageTrayLite`. Confirm the EXE path is quoted and ends in `--startup`. Test from a path containing spaces. Disable it and confirm only this value is removed. No administrator prompt or HKLM entry is allowed.

### 9a. Proxy and About positioning

Open **Proxy settings** from the tray with no owner window and confirm it appears centered on the active screen rather than at the upper-left corner. Open **About** immediately above **Exit** and confirm the centered dialog shows program name, current version, x64 architecture, .NET target, a parseable UTC build time, WebView2 SDK/Runtime versions, upstream baseline, and MIT license.

### 9b. Global light and dark themes

Use **Theme > Follow Windows / Light / Dark** and confirm exactly one choice is checked and persists after restart. Verify the tray menu, its disabled rows and submenus, Proxy, About, the login window title/status areas, text inputs, buttons, and Windows 11 title bars remain readable in both explicit modes. **Follow Windows** must reflect the current Windows app theme when each UI opens and react to a Windows setting-change broadcast while a dialog is open. In the login WebView, verify pages that honor `prefers-color-scheme` follow the selected preference; third-party OAuth pages may retain their own theme. Do not inject CSS into ChatGPT or identity-provider pages.

### 9c. Static application and window icons

1. Extract the current versioned ZIP into a new directory so File Explorer cannot reuse an icon cached for an older in-place build. Check `CodexUsageTrayLite.exe` in Details, Large icons, and Extra large icons views. Confirm it uses the Light Option C code-meter artwork and remains sharp at each size.
2. Open Proxy, Help, About, and Login / Usage. Confirm every title bar shows the application icon and the hidden WebView2 background host never becomes visible or receives focus.
3. Force the application's theme to Light while Windows uses Dark, then repeat with the application in Dark while Windows uses Light. Both the window and its title-bar icon must follow the explicit application selection; this is the regression check for the 0.1.28 Light window / Dark icon mismatch.
4. Select Follow Windows, leave at least one visible dialog open, and change the Windows app color mode. Confirm the window and title-bar icon switch together between Light and Dark without reopening the dialog. Repeat across a DPI change if a mixed-DPI display is available.
5. Trigger existing Information, Warning, Error, and Question MessageBoxes. Confirm their owner/modality, buttons, default button, keyboard behavior, and return path remain unchanged. Their content glyph is Windows-owned and is not expected to use the custom application icon.

### 9d. Application and WebView language coverage

Select English, Simplified Chinese, Traditional Chinese, Japanese, and Korean in the application Language submenu. For each choice, verify the tray menu, Proxy, Help, About, Login / Usage window, validation messages, and prompts update immediately; restart once and confirm the choice persists. On fresh Windows profiles with `ja-JP` and `ko-KR` UI languages, confirm the corresponding language is selected automatically. Tooltips must remain in their compact English format.

Website language is independent of the application language. In the authenticated ChatGPT profile, repeat the Usage-page check for Traditional Chinese (Hong Kong), Traditional Chinese (Taiwan), Japanese, and Korean. Confirm Weekly remaining/reset and `Available 0` (or the current actual count) parse without opening or clicking a reset. Restore the user's preferred website language after testing.

### 10. Tooltip and last-success cache

Verify a normal dual-window Tooltip contains only `5-Hour 20% (14:54)` and `Weekly 62% (14:58 9/7)` with local quota-reset times. It must contain no Email, `@`, `[CLI]`, or `[WV2]`. An exact Plus level with an applicable 5-Hour window must retain both Tooltip lines and the real menu value. An exact Pro level and an authoritative Weekly-only snapshot must omit the 5-Hour line from the Tooltip; the menu must still show **5-Hour N/A**. Free/Go with an applicable 5-Hour window also retains both Tooltip lines. Missing or malformed data must not be converted into a non-applicable plan, and available banked resets must not enter the Tooltip. Tooltip text must not exceed 63 characters; `[cached]`, `[error]`, or `[login]` remains on the Weekly line. The menu must retain the full Email followed by User Level, 5-Hour, Weekly, Usage resets, and Last updated. On source mismatch or login-required state, the reset row must show **Unknown**. On fetch failure, `[error]` must be present while prior values and **Last updated** remain unchanged. Missing reset/weekly values show `--`.

New log records must use `timestamp | level=info|warning|error | event=... | details`. After either source resolves the account, verify an `Account.EmailResolved` record contains only the fixed masked `first-character***@domain` form. Search settings and logs to confirm the unmasked address is absent; the full address may appear only in the live menu process memory and must not appear in the Tooltip.

### 11. Battery values and states

Use unit tests and preview sheets to inspect 100%, 75%, 50%, 25%, 10%, and 0%. Confirm thresholds 51/21/20 and colors. Unknown (`?`), Error (slash), Login (`L`), and actual 0% must all be visually distinct.

### 12. Parser failure and network failure

The automated harness covers normal English, used-to-remaining, baseline remaining, Simplified Chinese, both Traditional Chinese variants, Japanese, Korean/whitespace, malformed, missing, and login-page inputs. For runtime failure, disconnect the network or use an unavailable proxy; no crash, page dump, HTML log, token, or query URL is acceptable.

### 13. WebView2 Runtime missing

On a controlled VM without Evergreen WebView2 Runtime, confirm a clear error appears. The application must not download a runtime automatically.

### 14. Repeated refresh, Handle, and GDI checks

Run the automated 100-session harness and the real 50-controller probe above. For live monitoring:

```powershell
Get-Process -Name CodexUsageTrayLite -ErrorAction SilentlyContinue |
  Select-Object Id, WorkingSet64, PrivateMemorySize64, HandleCount, StartTime

Get-Process -Name msedgewebview2 -ErrorAction SilentlyContinue |
  Select-Object Id, Parent, WorkingSet64, PrivateMemorySize64, HandleCount, StartTime
```

Use Task Manager Details or Process Explorer to add **Working set**, **Private bytes**, **Handles**, **GDI objects**, and **USER objects** columns. Record values immediately before refresh, during load, after controller close, and 15 seconds later. Never use `taskkill /IM msedgewebview2.exe` because unrelated applications share that process name.

### 15. Idle memory

After a successful fetch and closed login window, wait 15-60 seconds. Confirm no browser window exists and no browser process parented/owned by this test instance remains. Record EXE working set/private bytes and compare with the upstream 150-800+ MB observation.

### 16. 8-12 hour soak

Run once at 5 minutes and once at 15 minutes. Keep the login browser closed. Record this table for each run:

| Time | EXE working set | EXE private bytes | Handles | GDI | USER | Owned WebView2 after idle | Successful fetches | Failures |
|---|---:|---:|---:|---:|---:|---|---:|---:|
| Start | | | | | | | | |
| 1 hour | | | | | | | | |
| 4 hours | | | | | | | | |
| 8 hours | | | | | | | | |
| 12 hours | | | | | | | | |

The rapid probe's Handle growth makes this soak a release gate: a monotonic trend requires further lifecycle work before public distribution.

### 17. DPI and taskbar appearance

At Windows scale 100%, 125%, 150%, and 200%, inspect tray output corresponding to 16, 20, 24, and 32 px. Check both light and dark taskbars. Verify outlines, 0% fill, `?`, slash, and `L` remain legible and no Microsoft/OpenAI/Codex logo appears.

### 18. Logs and config

Confirm **Open logs** opens `%LOCALAPPDATA%\CodexUsageTrayLite\logs\CodexUsageTrayLite.log` directly through the Windows file association, while **Open config folder** opens only `%LOCALAPPDATA%\CodexUsageTrayLite`. Search logs for `cookie`, `authorization`, `token`, `password`, `?code=`, page HTML, and body text; none may expose sensitive values. Confirm no settings, log, profile, dump, PDB, or private data appears in the release artifact.

Confirm that the first write after local midnight moves the prior non-empty active log to `CodexUsageTrayLite_YYYYMMDD.log` and writes the new event to a new `CodexUsageTrayLite.log`. Restart with an older active log and confirm its actual last-write date is used. Existing same-date archives must preserve both existing and newly appended segments. Daily archives are not automatically deleted.

For a normal CLI refresh, inspect `CodexCli.ProcessExit`: a child process that drains within 500 ms must produce `level=info`, `jobActiveBeforeClose=0`, `jobCloseAction=no-active-processes`, and a nonnegative `jobDrainWaitMs`. A child still active after the grace period must remain `warning` with `kill-on-close-requested`.

Interpret process-lifecycle entries as follows:

| Log entry/field | Meaning |
|---|---|
| `CodexCli.ProcessStarted` | Exact launcher PID, launcher type, and whether its private Job Object was assigned. |
| `exitType=graceful-after-stdin-close` | Closing the app-server pipe was sufficient; `Process.Kill()` was not called. |
| `exitType=forced-kill` with `killAttempted=true` | The exact launched root did not exit within three seconds; its targeted `Process.Kill()` returned and the root then exited. |
| `forced-kill-timeout` / `forced-kill-failed` | Targeted fallback did not produce a confirmed root exit; investigate the same PID and surrounding errors. |
| `jobActiveBeforeClose=N`, `jobCloseAction=kill-on-close-requested` | The private Job still reported active owned processes immediately before handle close, so Windows kill-on-close cleanup was requested. This does not refer to unrelated Codex/Node processes. |
| `WebView.ControllerClose ... result=returned processKill=false` | The owned controller's `Close()` API returned; this is not yet proof that the browser process exited. |
| `WebView.ProcessFailed` | WebView2 reported an unexpected process exit or renderer unresponsiveness; kind, reason, and numeric exit code are included when supported. |
| `WebView.BrowserProcessExited` | The WebView2 environment reported the actual browser-process collection exit, with browser PID and normal/failed exit kind. |

## Tests not completed in this development session

Authenticated WebView2 first login/session reuse, real WebView2 ChatGPT parsing against the user's account, end-to-end clicking of the new tray source selector, HTTP proxy network flow, SOCKS5 network flow, startup execution after Windows sign-in, clear-session deletion of an authenticated profile, light/dark taskbar acceptance, all DPI scales on hardware, and the 8-12 hour soak remain manual. The real Codex CLI rate-limit path itself was completed as recorded above. Remaining checks require interactive UI, working proxy endpoints, Windows sign-out/restart, or elapsed time and were not fabricated.

## Public release v0.2.16 - 2026-09-19

Published as GPL-3.0-only with retained upstream MIT and WebView2 notices. Functional code is unchanged from 0.2.16-local; the public build removes the local suffix and updates About license metadata. The original local package is retained.

Release checkout: artifacts/publication (independent main history). Public package: artifacts/packages/CodexUsageTrayLite-v0.2.16-win-x64.zip. Tests: 88 passed, 0 failed. SHA-256: be8328dad6607ccee8d05dcc83a9163264203921de256a88494e10a79514bbf7. Build completed with 0 errors and 2 NU1900 vulnerability-feed warnings.
