# 0.2.26 本版更新

- 将“关于”右侧的版本格式修正为 `v0.2.26`，`v` 后不再带点号。
- 较小摘要字号、右对齐方式、“帮助／更新／关于”顺序、GitHub 更新命令及子菜单几何均保持不变。
- 正式 WinForms 渲染覆盖英语／简体中文、浅色／深色和字体缩放场景。v0.2.26 构建通过全部 91 项自动化测试。

**安装：**下载 Windows x64 ZIP 及对应校验文件，核对哈希后将全部文件解压到固定目录，运行 `CodexUsageTrayLite.exe`。需要 Windows 11 x64 和 .NET Framework 4.8 或更高版本。WebView2 模式需要安装 WebView2 Evergreen Runtime；CLI 模式使用已安装且通过 ChatGPT 登录的 Codex CLI。

**升级：**替换全部程序文件前先选择退出。启用开机启动时保持安装目录不变，或移动后重新启用该设置。`%LOCALAPPDATA%\CodexUsageTrayLite` 中的设置和 WebView2 会话会保留。
