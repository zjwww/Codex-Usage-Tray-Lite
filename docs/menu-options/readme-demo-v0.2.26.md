# README menu demo for v0.2.26

This documentation-only update adds an English and Simplified Chinese browser simulation and four animated GIFs. It does not change application code or the immutable v0.2.26 package.

## Public assets

- `../site/`: deploy this directory as the GitHub Pages artifact root. It contains `index.html`, `en.html`, `zh-CN.html`, shared CSS/JavaScript, and generated menu labels. No server, third-party scripts, fonts, analytics, or account access is required.
- `../images/menu-demo-{en,zh-cn}-{light,dark}.gif`: 13.5-second loops with eight states covering the main menu, usage source, refresh interval, interval selection, icon style, style selection, Tools & Help, and theme.
- `../images/menu-chinese-{light,dark}.png`: existing v0.2.26 production-control renderings copied from validation output; these complement the browser simulation and the existing English images.
- The two repository READMEs use the matching locale's theme-aware GIF and link to the corresponding Pages page.

Expected URLs after publication:

- https://zjwww.github.io/Codex-Usage-Tray-Lite/en.html
- https://zjwww.github.io/Codex-Usage-Tray-Lite/zh-CN.html

Publication must occur in the CUTL-04 thread after local resources are complete. Pages deployment should expose only `docs/site`, not repository-local validation output, runtime profiles, build files, or internal tools. If the Pages URL differs, update both README links together before publishing. GitHub-side setup and publication are not performed by these generation scripts.

## Reproduction

1. Run `scripts/generate-menu-demo-data.py` with Python. Labels come directly from the application's English and Simplified Chinese resources; the version comes from the project file.
2. Run `scripts/render-menu-demo.cjs` with Node and Playwright. Use `CUTL_PLAYWRIGHT_MODULE` and `CUTL_BROWSER_PATH` to override the local Windows defaults.
3. Run `scripts/encode-menu-demo-gifs.py` with Python and Pillow.

The render and encode scripts intentionally target this v0.2.26 delivery. Frames and verification reports are stored under `artifacts/validation/readme-menu-demo-v0.2.26`, outside the publication payload. Existing candidate icon and menu resources are retained.

## Validation boundary

Local browser checks cover both languages and themes, fifteen top-level rows, every depicted submenu, the About version, source-specific items, the last-successful-source label, simulated refresh, keyboard navigation, theme changes, language navigation, and 880/736/390/320-pixel viewports. The tested pages issue zero remote requests and zero JavaScript errors. GIF validation checks frame count, loop behavior, shared canvas dimensions, durations, and a file-size ceiling. Encoded GIF frames and narrow layouts were visually inspected.

The page is explicitly labeled as a simulation. Three additional desktop languages are shown but disabled in this bilingual demo. Action dialogs do not log in, clear sessions, modify startup, or read real usage. The Update dialog contains an explicit link to the latest Release. Font-scale screenshots and a browser mockup do not establish Windows hardware-DPI or live taskbar behavior.

The packaged v0.2.26 application's recorded test result is 91 passed. Its ZIP SHA-256 remains `0be4ede80892319e11acf57b7518460ba9580d149e11c69c7e977ebad3033620`. The README/site work does not repack or overwrite that ZIP.
