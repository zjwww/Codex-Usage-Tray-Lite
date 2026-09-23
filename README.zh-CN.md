<div align="center">
  <picture>
    <source media="(prefers-color-scheme: dark)" srcset="docs/images/logo-dark.png">
    <source media="(prefers-color-scheme: light)" srcset="docs/images/logo-light.png">
    <img src="docs/images/logo-light.png" alt="Codex Usage Tray Lite 图标" width="96">
  </picture>
  <h1>Codex Usage Tray Lite</h1>
  <p>在 Windows 系统托盘中轻量查看 Codex 剩余额度。</p>
  <p>
    <a href="https://github.com/zjwww/Codex-Usage-Tray-Lite/releases/latest"><img src="https://img.shields.io/github/v/release/zjwww/Codex-Usage-Tray-Lite?sort=semver&amp;style=flat" alt="最新版本"></a>
    <img src="https://img.shields.io/badge/platform-Windows%2011%20x64-0078D4?style=flat" alt="Windows 11 x64">
    <img src="https://img.shields.io/badge/.NET%20Framework-4.8-512BD4?style=flat" alt=".NET Framework 4.8">
    <a href="LICENSE"><img src="https://img.shields.io/github/license/zjwww/Codex-Usage-Tray-Lite?style=flat" alt="许可证"></a>
  </p>
  <p><a href="README.md">English</a> | <a href="README.zh-CN.md">简体中文</a> | <a href="https://github.com/zjwww/Codex-Usage-Tray-Lite/releases/latest">下载最新版</a> | <a href="https://github.com/zjwww/Codex-Usage-Tray-Lite/issues">问题反馈</a></p>
</div>

## 软件介绍

Codex Usage Tray Lite 是非官方的 Windows 便携式系统托盘工具，通过动态图标、简洁的提示文本和右键菜单显示每周剩余额度，以及账户适用时的 5 小时剩余额度。

本项目并非由 OpenAI 开发、认可或提供支持。OpenAI、ChatGPT 和 Codex 商标归各自权利人所有。

## 主要功能

- **两种数据源：**使用应用独立的 WebView2 浏览器登录，或使用已安装且通过 ChatGPT 登录的 Codex CLI。
- **四种托盘样式：**仅 5 小时、仅每周、上下双层（默认）、并排双条。上下双层和并排双条模式在明确确认账户没有 5 小时额度时，使用完整边框显示每周额度。
- **状态明确：**区分剩余额度、未知数据、需要登录、获取失败和不适用的 5 小时额度；不会把缺失值伪造为零。
- **账户信息：**邮箱、支持识别的账户等级、额度、可用重置次数和最后更新时间。程序只读取重置次数，不会消耗重置机会。
- **刷新控制：**支持手动刷新，以及 1、2、5、10、15、30、60 分钟间隔；默认 15 分钟。
- **外观：**跟随 Windows、浅色、深色；支持英文、简体中文、繁体中文、日文和韩文界面。简洁的托盘提示保留英文。
- **本地偏好：**开机启动、打开日志／配置目录，以及 WebView2 的系统／直连／HTTP／SOCKS5 代理设置。不支持代理认证。
- **按需采集：**每次采集完成后释放 WebView2 控制器及 CLI 进程。没有桌面小组件、遥测、开发者后台服务、安装器或自动更新。

## 界面预览

以下是使用示例数值的渲染图，不是真实账户截图。托盘预览由程序渲染器生成，展示 16／20／24／32 像素效果，并放大便于查看。

### 全部托盘样式与状态

![四种托盘样式、浅深色变体及特殊状态](docs/images/quota-styles.png)

每周剩余额度高于 50% 时为绿色，21–50% 为橙色，0–20% 为红色。在双额度模式下，5 小时额度独立控制其警示边框。`i` 表示所选 5 小时额度不适用，`?` 表示未知，`/` 表示获取失败，`L` 表示需要登录。

### 并排双条及仅每周额度账户

![并排双条的额度组合与每周全宽回退效果](docs/images/side-by-side.png)

### 英文菜单

<img src="docs/images/menu-english-light.png" alt="英文菜单 — 浅色" width="251"> <img src="docs/images/menu-english-dark.png" alt="英文菜单 — 深色" width="251">

菜单渲染图使用真实英文资源文本和程序的菜单渲染器，数据为示例。在 CLI 模式下，登录操作变为 **Codex CLI login help**，并隐藏 WebView2 代理和会话清理操作。

### 现有程序图标

![现有程序图标的两种变体及全部尺寸](docs/images/app-icon-sizes.png)

静态程序图标用于可执行文件及对话框；系统托盘使用上面的动态额度样式。

## 运行环境与系统要求

| 组件 | 要求 |
| --- | --- |
| 操作系统 | Windows 11 x64（目标环境） |
| 程序运行时 | .NET Framework 4.8 或更高版本；不随包附带 |
| WebView2 数据源 | Microsoft Edge WebView2 Evergreen Runtime 及 ChatGPT 登录；运行时不随包附带 |
| Codex CLI 数据源 | `PATH` 中已有 Codex CLI，且通过 ChatGPT 登录；CLI 及其运行时不随包附带 |
| 网络 | 能访问所选数据源需要的服务 |

Windows 10 和 Windows on ARM 尚未作为目标环境验证。本程序不需要 Python、Playwright 或捆绑的 Chromium 浏览器。

## 安装与启动

1. 从[最新 Release](https://github.com/zjwww/Codex-Usage-Tray-Lite/releases/latest)下载 Windows x64 ZIP 和对应的 `.zip.sha256`。
2. 解压前比对 SHA-256：

   ```powershell
   Get-FileHash .\CodexUsageTrayLite-v0.2.18-win-x64.zip -Algorithm SHA256
   Get-Content .\CodexUsageTrayLite-v0.2.18-win-x64.zip.sha256
   ```

3. 将**整个 ZIP** 解压到固定目录，保留可执行文件旁的 DLL 和语言目录。不要直接在压缩包内运行。
4. 运行 `CodexUsageTrayLite.exe`，在 Windows 通知区域（包括折叠区域）找到程序图标。
5. 使用 WebView2 时，选择 **Open login / usage page**，直接登录 ChatGPT 后关闭窗口。使用 CLI 时，先通过 CLI 登录，再选择 **Usage source → Codex CLI**。

全新安装会等待数据源设置，不会立即自动获取。选择数据源时会立即验证，成功读取后才启用定时刷新。**Refresh now** 可重试已配置的数据源；**Start at login** 为可选项。

可执行文件未进行代码签名。运行前请核验下载文件及来源。

## 升级与卸载

升级时先选择 **Exit**，解压新版本完整文件，在程序退出后替换程序文件。如果启用了开机启动，建议保持安装路径不变；移动目录后可关闭并重新启用该选项。设置和 WebView2 配置保存在程序目录之外，会继续保留。

卸载时关闭 **Start at login**，选择 **Exit**，然后删除程序目录。如需同时删除设置、日志和已保存的 WebView2 会话，退出后删除 `%LOCALAPPDATA%\CodexUsageTrayLite`。

## 隐私与故障排查

- 本地数据保存在 `%LOCALAPPDATA%\CodexUsageTrayLite`。切勿公开其中的 `webview2-profile`，它可能包含有效登录会话。
- 账户邮箱在内存中保存并显示于菜单；日志使用脱敏邮箱。程序不会读取 Codex CLI 凭据文件或其他浏览器的配置目录。
- WebView2 自行管理保存的 Cookie／会话，CLI 自行管理认证。没有接收用户数据的开发者后台服务。
- 使用 **Open logs** 查看诊断信息，提交问题前请先检查日志内容。按日归档的日志不会自动删除。
- WebView2 登录失效时重新打开登录页；缺少运行时则需单独安装。CLI 模式需要 `PATH` 中存在已登录、可正常使用的 CLI。
- WebView2 采集依赖网站布局，CLI 采集依赖已安装版本的 app-server 协议；上游变化可能使任一数据源失效。

## 构建与验证

在 Windows 上使用能够构建 .NET Framework 4.8 项目的 .NET SDK。NuGet 会还原框架引用程序集和 WebView2 SDK。

```powershell
dotnet restore .\CodexUsageTrayLite.sln --configfile .\NuGet.Config
dotnet build .\CodexUsageTrayLite.sln -c Release -p:Platform=x64 --no-restore
& .\tests\CodexUsageTrayLite.Tests\bin\x64\Release\net48\CodexUsageTrayLite.Tests.exe
```

v0.2.18 构建通过了 **89 项自动化测试**，覆盖解析器、设置、本地化、图标像素、进程清理和模拟资源生命周期；这不代表全部真实登录流程、代理环境、硬件 DPI 或 8–12 小时持续运行均已验证。参见[验证说明](TESTING.md)。

## 许可证与致谢

本项目采用 **GPL-3.0-only**。分发修改版时需要按 GPL 提供相应源码；私人修改不要求公开。详见 [LICENSE](LICENSE) 和[第三方声明](THIRD_PARTY_NOTICES.md)，上游 MIT 声明继续保留。

- [saveway/codex-usage-monitor](https://github.com/saveway/codex-usage-monitor) — 采用 MIT 许可证，其原生 WebView2 预览版是本项目的初始基础（`v2.0.0-preview.7`，提交 `36e9679164dcd7e5ef23d1f35822664785fad01f`）。
- [ognjeeen/codex-usage-widget](https://github.com/ognjeeen/codex-usage-widget) — 采用 MIT 许可证的参考项目。
