# Codex Usage Tray Lite 更新日志

英文版：[CHANGELOG.md](CHANGELOG.md)

本文记录本地开发版本 `0.1.0-local` 至当前版本的功能变化。记录依据为本开发会话中的实际发布回执、项目规格、测试文档、源码和已保留的版本化 ZIP。

`0.2.16` 之前的版本均为本地开发迭代，并在本更新历史中保留 `-local` 后缀；`0.2.16` 是首次发布到 GitHub 的版本。从 `0.1.1-local` 起，每次用户可见更新都使用新版本号和不可覆盖的独立 ZIP；当前本地候选包与 GitHub Release 资产统一使用不含 `local` 的 `CodexUsageTrayLite-v<版本>-win-x64.zip` 文件名。

## 0.2.18 - 2026-09-23

### 更新

- 后续本地候选包与 GitHub Release ZIP 统一命名为 `CodexUsageTrayLite-v<版本>-win-x64.zip`；既有历史包仍按不可覆盖原则保留原名。
- 在便携 ZIP 中新增完整的简体中文说明 `README.zh-CN.txt`，并将英文说明统一为 `README.txt`。
- 每个 ZIP 继续包含完整的中英文更新历史：`0.2.15` 及更早版本保留 `-local` 后缀，`0.2.16` 起使用公开版本号。
- 打包内的源码链接会自动跟随当前版本，并新增当前版本标题、历史版本分界、未替换模板变量、README 源码链接及新 ZIP 名称不含 `local` 等自动检查。
- 本次仅调整文档与打包流程；用量读取、菜单行为、图标渲染、设置、刷新周期和进程生命周期均与 `0.2.17` 相同。

## 0.2.17 - 2026-09-23

### 更新

- 将 WebView2 与 Codex CLI 两种模式的托盘一级菜单统一精简为 15 个状态和操作条目。邮箱与账号等级合并为一行，5-Hour、Weekly、可用重置次数和上次更新时间仍各自完整显示。
- 将 WebView2 的代理与登录会话维护项移入“用量来源”子菜单；Codex CLI 模式会连同相关分隔线一起隐藏。日志、配置文件夹、帮助和关于移入新的“工具与帮助”子菜单，菜单最大层级仍为两级。
- 在“用量来源”“刷新间隔”“图标样式”“主题”“语言”右侧显示当前选择。这些摘要不绑定快捷键；切换选定来源不会改写“上次更新”中记录的实际成功来源。
- 邮箱／等级、5-Hour 和 Weekly 三个只读主状态行在浅色和深色主题下使用高对比正文颜色；重置次数和上次更新时间保持次级灰色。其他普通禁用操作仍使用原有禁用样式。
- 五种界面语言中的既有 `Both (Default)` 图标样式文案改为 `Stacked (Default)`（简体中文为“上下双层（默认）”），不改变保存枚举、默认模式、渲染逻辑、阈值或仅 Weekly 回退行为。
- 新增由正式 WinForms 菜单渲染器生成的英语／简体中文浅色与深色、WebView2／CLI 子菜单，以及 150% 字体／长邮箱检查图。89 项自动化测试全部通过。

## 0.2.16 - 2026-09-19

### 更新

- 优化并排双条在权威确认账号等级没有 5-Hour 限制时的显示：Weekly 现在占用整个内框，不再保留空白右半区。
- 仍有双用量窗口的账号继续使用已批准的 R2 左 Weekly / 右 5-Hour 布局。5-Hour 适用性未知时仍显示 Unknown，不会当作无限制或 0。
- 新增 Weekly 0/1/20/21/50/51/100%、16/20/24/32 像素及 Light/Dark 下的全宽回退精确检查，并验证其像素与 Weekly Only 完全一致。88 项自动化测试全部通过。

## 0.2.15-local - 2026-09-19

### 新增

- 新增第四种持久化图标样式 `并排双条`（Side by Side），保留 `两者（默认）` 为默认值，并保留所有已有用户选择。
- 并排双条在现有 1 像素外框内把画布分为左侧 Weekly 和右侧 5-Hour 两半。两个平顶直角矩形都从底部向上填充，没有侧边留白、中间间隙、分隔线、圆角或顶部凸起，并分别沿用现有像素级百分比取整。
- 为英文、简体中文、繁体中文、日语和韩语补齐菜单文字，并根据已批准的 R2 几何生成正式渲染器预览。

### 更新

- 并排双条保持现有配色与预警归属：Weekly 独立使用绿/橙/红，5-Hour 始终保持主题蓝色，只有 5-Hour 控制外框的中性/橙/红。权威确认 5-Hour 不适用时，右半保持中性空区，不会把缺失值误当作 0。
- 扩展持久化、Tooltip、精确像素几何、0/1/20/21/50/51/100 阈值、Light/Dark、16/20/24/32 像素、缓存切换、不可用数据、特殊状态和原生资源测试。87 项自动化测试全部通过。

## 0.2.14-local - 2026-09-14

### 更新

- 放大额度方框内的 `i`、`?`、`L` 托盘状态字符，并统一改为红色粗体，使其在 Light 和 Dark 任务栏主题中更容易识别。
- 渲染器会按每个 Segoe UI 粗体字符的实际轮廓，以单一等比缩放系数适配方框内的最大安全区域。`i` 等窄字符优先占满可用高度，同时保持原始纵横比例不失真。
- 新增 16、20、24、32 像素及 Light/Dark 组合下的实心红色核心与最小占用高度回归断言。85 项自动化测试全部通过。

## 0.2.13-local - 2026-09-14

### 修复

- 收紧 Pro 的 5-Hour N/A 规则：只有权威确认 5-Hour 不适用时才显示 `i` 和 `5-Hour N/A`。如果未来 Pro 响应确实包含有效且适用的 5-Hour，用量图标、Tooltip 和菜单会保留真实值，不会只因套餐标签而丢弃。
- 新增“Pro 但存在权威有效 5-Hour 值”的冲突边界回归断言。84 项自动化测试全部通过。

## 0.2.12-local - 2026-09-14

### 新增

- 在 Refresh interval 正下方新增 `Icon style` 子菜单，依次提供 `5-Hour Only`、`Weekly Only` 和 `Both (Default)`。三项保持单选，选择后立即应用、保存，并在下次启动时恢复；新安装和旧配置迁移均默认 Both。
- 为五种已支持的程序语言补齐本地化菜单资源，且没有复用已经废弃的旧 A-D `iconStyle` 字段。

### 更新

- 额度图标渲染器新增独立的 Both、仅 Weekly 和仅 5-Hour 图层路径。被隐藏的额度不再影响可见图标的预警颜色或缓存键；Both 保持现有外观，并在权威确认 5-Hour 不适用时自动只绘制 Weekly。
- 在 5-Hour Only 模式中，已确认的 Pro 仅 Weekly 数据显示空额度方框和高对比度字母 `i`。5-Hour 适用性未知、需要登录和刷新失败仍保持不同状态，并优先于信息标记。
- 在 5-Hour Only 模式中，Tooltip 只显示 5-Hour 行；Pro/明确不适用时显示 `5-Hour N/A`。Weekly Only 和 Both 按要求继续沿用原有 Tooltip 行为。
- 扩展设置迁移、本地化、Tooltip、Light/Dark、16/20/24/32 像素、预警边界、缓存隔离、特殊状态和原生资源回归覆盖。84 项自动化测试全部通过。

## 0.2.11-local - 2026-09-11

### 修复

- 修正 0.2.10 引入的账号等级 Tooltip 规则：Plus 账号继续在 Tooltip 和菜单中显示适用的 5-Hour 用量。只有 Pro（当前支持的高于 Plus 的等级）或权威的仅 Weekly 数据，才省略 Tooltip 的 5-Hour 行，并在菜单中显示 `5-Hour N/A`。
- 更新回归覆盖，明确区分 Plus 与 Pro，同时保持原有 Free/Go 及仅 Weekly 行为。80 项自动化测试全部通过。

## 0.2.10-local - 2026-09-11

### 新增

- 在 Email 下方新增不可点击的 `User Level`、`5-Hour` 和 `Weekly` 账号状态行，之后继续显示原有的 Usage resets 与 Last updated。
- 新增保守的 Free/Go/Plus/Pro 识别。Codex CLI 从已有 `account/read` 响应读取明确的 `planType`，必要时使用 rate-limit bucket 中的明确套餐类型；WebView2 从已有同源 session 响应读取受支持的明确套餐字段，否则只接受渲染页面顶部附近独立成行的 `FREE`、`GO`、`PLUS` 或 `PRO` 徽标。缺失或不支持的值保持“未知”；本版明确不探测 Pro 5x/20x。
- 五套程序 `.resx` 资源均加入本地化 User Level 行标签；Free/Go/Plus/Pro 等级名称在各种语言中保持统一。

### 更新

- Windows Tooltip 取消账号 Email 及 `[CLI]`/`[WV2]` 来源标签。普通账号只显示 5-Hour 和 Weekly 两行；明确识别为 Plus/Pro 或权威的仅 Weekly 快照时，只显示 Weekly。
- 菜单始终保留 5-Hour 行；Plus/Pro 及仅 Weekly 快照显示 `5-Hour N/A`，其它情况的 5-Hour、Weekly 百分比和重置时间与 Tooltip 相同。
- 修复 Help 对话框的段落与换行被 Windows 多行 Edit 控件挤成一团的问题。资源文字交给控件前会统一转换成 Windows CRLF，复制出来的文本内容不变。
- 扩展账号等级、Tooltip、菜单额度行、设置、本地化和 Help 实际多行控件回归覆盖。80 项自动化测试全部通过。

## 0.2.9-local - 2026-09-11

### 新增

- 新增日语和韩语程序界面，覆盖托盘菜单、Proxy、Help、About、Login / Usage 窗口、校验信息和程序提示。
- 新增可编辑的 `Resources/Strings.ja-JP.resx` 与 `Resources/Strings.ko-KR.resx` 源资源；发布包在原有中文附属程序集之外，新增编译后的 `ja-JP` 和 `ko-KR` 附属程序集。
- Language 子菜单新增日语和韩语。全新安装会像已支持的中文地区一样跟随日语或韩语 Windows UI 语言；已有安装继续保留已经保存的语言。Tooltip 按设计保持英文。

### 更新

- 扩展 WebView2 Usage 与可用 reset 解析，覆盖已经验证的繁体中文（香港）、繁体中文（台湾）、日语和韩语网页文案，包括本地化的 Weekly/5-hour 标签、剩余/已用方向、重置时间、reset 区域标题、精确可用数量标签、操作文字、空状态和区域边界。
- 通过 Computer Use 对繁体中文（香港）、繁体中文（台湾）、日语和韩语进行了真实登录页面验证，完成后将账号网页语言恢复为简体中文。
- 继续保持保守解析：未知文案、含义模糊的 reset 区域或无法读取的 Weekly 数值仍返回 ParseError/Unknown，不进行猜测。
- 扩展多语言、迁移、对话框、解析器、DOM 探针和打包覆盖。79 项自动化测试全部通过。

## 0.2.8-local - 2026-09-11

### 更新

- 成功读取的快照被识别为仅 Weekly 套餐（即当前没有适用 5-Hour 限制的 Pro 账号结构）时，Tooltip 现在会直接省略 5-Hour 行，不再显示 `5-Hour N/A`。
- 检测到 Pro/仅 Weekly 后，Tooltip 改为两行：第一行是脱敏账号及来源，第二行是 Weekly 剩余量和重置时间。普通双额度账号继续使用原有的 5-Hour、Weekly 三行布局。
- CLI、WebView2 以及缓存/错误状态的 Tooltip 均采用相同行为；来源和状态后缀继续保留，并继续满足 Windows 的 63 字符上限。
- 更新 CLI/WebView2 两种来源的仅 Weekly Tooltip 回归契约。75 项自动化测试全部通过。

## 0.2.7-local - 2026-09-10

### 新增

- 新增统一的 `UsageField.Unavailable` warning 事件：每次刷新得到最终结果后，只要 `resets`、`five-hour` 或 `weekly` 任一字段无法读取，就会记录对应事件。
- 每条事件只记录 `source`、`field` 和稳定的 `reason`，例如 `response-field-missing`、`percentage-unreadable`、`rate-limit-window-invalid`、`reset-section-unrecognized` 或 `dom-probe-failed`；不会记录用量数值、网页正文、账号标识、Cookie 或 Token。
- Codex CLI 诊断会区分 reset 响应字段缺失/无效及额度窗口缺失/无效；WebView2 诊断会区分百分比不可读、reset 区域缺失/无法识别和 DOM 探针异常。
- 字段事件只在刷新达到最终结果后记录，每个不可用字段最多一条，不会在每次重试时刷屏。合法的零值和仅 Weekly 套餐中明确不适用的 5-Hour 不属于 warning。
- 新增字段分类、隐私格式、日志级别及仅 Weekly 防误报测试。75 项自动化测试全部通过。

## 0.2.6-local - 2026-09-10

### 修复

- 修复 Pro Usage 页面把零值状态改为 reset 历史上方的 `Available 0` 标签后，WebView2 仍显示 `Usage resets: Unknown` 的问题。
- 有区域边界的正文解析器现在可以读取 `Available N`、`Available (N)`、简繁体中文 `可用 N` 及韩文 `사용 가능 N` 精确行，并且只在已识别的 Usage-limit-resets 区域内生效。
- 只读 DOM 探针加入相同的区域限定标签规则；明确数量/可用操作仍有优先级，History 日期和区域外文字不会参与计数，探针不会点击任何元素。
- 新增与用户真实登录截图一致的样例，包含 `Available 0`、History、Past 30 days 及带日期的 reset 历史行，并覆盖正数、多语言及区域外防误判。74 项自动化测试全部通过。

## 0.2.5-local - 2026-09-10

### 修复

- 保留 v0.2.4 的“仅 Weekly 且无 reset 区域时回退为 0”逻辑，同时明确区分“DOM 探针完整结束但无结果”和“探针执行异常”。
- WebView2 脚本/Runtime 探针失败时不再进入零值回退；此时仍保持 Unknown，并保留原有脱敏的 `WebView.UsageResetReadFailed` 诊断事件。
- 同一组 0 与 Unknown 回归测试增加探针失败场景。74 项自动化测试全部通过。

## 0.2.4-local - 2026-09-10

### 修复

- 修复已确认的仅 Weekly 套餐在 Pro Usage 页面因没有可用预存 reset 而完全不显示 Usage-limit-resets 区域时，WebView2 菜单仍显示 `Usage resets: Unknown` 的问题。
- 回退条件被严格限定：必须先完整执行原有 12 次 DOM 探针、额度结果明确表示 5 小时限制不适用，并且最终稳定页面仍不包含已识别的 reset 区域标题；只有该组合才映射为 0。
- 如果 reset 区域存在但无法理解或含义模糊、账号仍是普通双窗口套餐、额度数据缺失或探针失败，仍显示 Unknown，不猜测为 0。
- 新增隐私最小化的 `WebView.UsageResetResolved` info 日志事件，只记录“仅 Weekly 且区域缺失”的原因，不记录网页正文或用量数值。
- 新增精确的 0 与 Unknown 回退测试。74 项自动化测试全部通过。

## 0.2.3-local - 2026-09-10

### 修复

- 修复账号切换到只提供 Weekly、没有 5 小时额度窗口的套餐后，Codex CLI 和 WebView2 两种来源都刷新失败的问题。
- CLI 解析器现在会把实际观测到的单个 `10080` 分钟窗口及缺失的 secondary 窗口识别为明确的仅 Weekly 套餐，而不是错误响应。Weekly 仍为必需项；Weekly 缺失或无效时仍会安全失败。
- WebView2 解析器现在允许稳定后的页面只有 Weekly 额度卡，或 5 小时卡明确显示 Unlimited/N/A；同时排除 reset 说明文字里的 5 小时/每周词组，并在根据“卡片缺失”判断前额外读取三次，避免页面延迟渲染时过早判断。
- 缓存快照新增可空的 5 小时限制适用状态。旧版带数值的快照自动视为适用；仅 Weekly 快照会清除旧的 5 小时百分比及重置时间，设置 schema 保持不变。
- 仅 Weekly 套餐的 Tooltip 显示 `5-Hour N/A`。托盘图标保持正常额度状态，只绘制 Weekly 底层、取消蓝色 5 小时叠层，并使用中性边框，不再显示红色错误斜线。
- 新增真实结构对应的 CLI 单窗口、WebView2 仅 Weekly/明确无限、reset 说明排除、设置迁移、Tooltip 和图标渲染测试。73 项自动化测试全部通过。

## 0.2.2-local - 2026-09-10

### 更新

- 扩展 WebView2 Usage 网页解析，支持英文、简体中文、繁体中文和韩文文案；网页解析与程序所选 UI 语言相互独立，也不会修改网页语言。
- 新增 Unicode NFKC 规范化，可一致处理全角数字、百分号、冒号、斜线以及混合语言的额度行。
- 补齐多语言 5 小时/每周标签、剩余/已用方向、重置时间标记、日期时间格式、可用重置数量、重置区域标题、操作文字、区域边界及明确无可用重置提示。
- 保持保守策略：方向冲突、含义模糊的重置区域或未知文案不会被猜测；仍显示无法读取/未知，并继续排除 Codex Spark 额度卡片。
- 只读 DOM 后备探针同步加入四种语言的重置区域词组；仍只统计区域内可见且启用的精确操作，不点击重置、不更改网页语言，也不返回或记录页面正文。
- 新增多语言、混合语言/全角字符、跨午夜、跨日期、0/1/多个重置、延迟渲染契约、歧义及 Spark 排除测试。70 项自动化测试全部通过。

## 0.2.1-local - 2026-09-09

### 更新

- 将源码中硬编码的英文/简体中文/繁体中文文字表迁移为标准 .NET `.resx` 资源架构。
- 英文作为中性资源保存在 `Resources/Strings.resx`，简体中文保存在 `Resources/Strings.zh-CN.resx`，繁体中文保存在 `Resources/Strings.zh-TW.resx`。
- `UiText` 现在通过缓存的 `ResourceManager` 和明确的 `en-US`、`zh-CN`、`zh-TW` culture 读取命名资源。原有语言选择、首次识别、实时切换、回退行为、对话框和仅英文 Tooltip 均保持不变。
- 发布包新增编译后的 `zh-CN/CodexUsageTrayLite.resources.dll` 与 `zh-TW/CodexUsageTrayLite.resources.dll` 附属资源程序集。可编辑的 `.resx` 是源码文件，并非运行时配置文件，因此不随发布包提供。
- 打包白名单审计和 README 已纳入两个语言目录。65 项自动化测试全部通过，新增中性嵌入资源及两套编译附属资源检查。

## 0.2.0-local - 2026-09-09

### 新增

- 在 Theme 下方新增 Language 子菜单，提供 `English`、`简体中文`、`繁體中文`。切换后托盘菜单会立即更新，已打开的 Login / Usage 窗口也会同步更新。
- 为托盘菜单、来源专属项目、刷新间隔、Proxy、Help、About、Login / Usage 状态、校验提示、启动/错误提示及确认对话框补齐简体中文和繁体中文界面文字。
- 新增首次启动 Windows UI 语言识别：简体中文地区使用简体中文，繁体中文地区使用繁体中文，其余地区使用 English。
- 新增程序自有的多语言提示对话框，因此即使程序语言与 Windows 显示语言不同，确定、是、否按钮也会跟随程序语言。

### 更新

- 校正英文 Help 文法，并在两处“程序绝不会使用 reset”的说明后加入 `in your account`；简体和繁体 Help 均基于修正后的英文翻译。
- 英文 1 分钟菜单由不正确的 `1 minutes` 修正为 `1 minute`。
- 设置格式由 schema 3 升级为 schema 4，以保存语言选择。已有安装迁移时保持 English，避免升级后擅自改变原界面；全新安装只在首次初始化时识别系统语言，之后保存用户选择。
- Tooltip 按要求继续使用现有紧凑英文格式，并保持 63 字符上限。
- 64 项自动化测试全部通过，新增地区语言识别、设置迁移、本地化状态格式、三种 Help、Proxy/About/Login 对话框、登录窗口实时切换语言以及 Tooltip 不变等覆盖。

## 0.1.29-local - 2026-09-09

### 修复

- 修复 Proxy、Help、About 和 Login / Usage 的实际窗口主题为 Light 时，标题栏仍显示 Dark 程序图标的问题。
- 0.1.28 直接根据 Windows 注册表当前模式选择标题栏图标，并刻意独立于 Form 主题，因此程序明确选择 Light 时可能出现截图中的“浅色窗口 + 深色图标”不一致。
- 可见 Form 现在根据控制标题栏和内容的同一个最终配色选择图标：Light 使用 `AppIconLight`，Dark 使用 `AppIconDark`，Follow Windows 则让窗口和图标同时根据 Windows 当前应用主题解析。
- 已打开的 Follow Windows 对话框除了窗口消息外，还通过 Windows 用户偏好和显示设置事件增加第二条刷新路径；刷新会排入 UI 线程，避免 Windows 外观变化时继续保留旧图标。

### 行为与验证

- 明确选择 Light 或 Dark 时，不会被无关的 Windows 外观通知改变；Follow Windows 继续随 Windows 更新。
- 每个 Form 释放时会解除系统事件并释放被替换的图标资源。隐藏且禁止激活的 WebView2 后台宿主仍不参与可见窗口逻辑。
- 61 项自动化测试全部通过。回归测试现在会让四个可见 Form 分别经历 Light、Dark 和 Follow Windows，并确认标题栏图标与窗口最终主题一致，同时保留原生尺寸及 GDI/USER 资源释放检查。
- Usage 获取、CLI、WebView2、Proxy、刷新、Tooltip、动态托盘图标、MessageBox 语义、设置格式及日志均未改变。

## 0.1.28-local - 2026-09-09

### 更新

- 采用已确认的 Option C “代码括号 + 用量条”图稿作为程序静态身份图标；使用 `docs/icon-options/app-icon-r2` 中保留的透明浅色源图和对应深色源图。
- 新增完整的 Light/Dark ICO 资源，原生尺寸包括 16、20、24、32、40、48、64、96、128 和 256 px。EXE 及 Windows 资源管理器默认使用 Light 版本。
- Proxy、Help、About 和 Login / Usage 窗口现在根据 Windows 当前系统配色选择 Light 或 Dark 程序图标。标题栏图标的选择刻意独立于程序手动设置的 Follow Windows / Light / Dark 内容主题。
- 已打开窗口在收到 Windows 设置、主题或 DPI 变化通知后会刷新标题栏图标；被替换的图标资源会明确释放。

### 行为与验证

- 动态 Usage 托盘图标、两层额度显示和预警色、Usage 获取、CLI、WebView2、Proxy、定时器、Tooltip、设置及日志行为均未改变。
- 从不显示且禁止激活的 WebView2 后台宿主仍为普通隐藏 Form，不套用可见窗口的程序图标逻辑。
- 标准 WinForms MessageBox 完整保留原来的 owner、模态、按钮、返回值、默认按钮行为和 Windows 自带 Information / Warning / Error / Question 图形。若要自定义标题栏 `Form.Icon` 必须将其替换为自绘对话框，因此本版刻意不作改造。
- 61 项自动化测试全部通过，覆盖两套嵌入式多尺寸 ICO、EXE 默认 Light 图标（包括由 Windows 提取的 256 px 满画布帧）、每个可见 Form 的系统主题图标选择、运行时 Light/Dark 替换，以及 GDI/USER 资源上限。
- 编译后的 EXE 已在真实 Windows 11 深色资源管理器的详细信息视图中显示新的 Light 代码用量图标。同一编译路径曾被打开时，Windows 图标缓存可能短暂保留旧渲染；将版本化安装包解压到新目录可避开该缓存情况。

## 0.1.27-local - 2026-09-08

### 新增

- 在英文 Help 对话框中新增 `Security and Privacy` 段落。
- 该段说明登录和账号信息不会发送给开发者或第三方；程序不读取或保存密码、Cookie 和 access token；WebView2 与 Codex CLI 分别管理自己的凭据；完整账号邮箱仅存在于内存，日志只记录脱敏形式。
- 同时说明程序没有遥测或云后端，只读取 Usage 信息，并且不会消耗 Usage reset。

### 行为与验证

- 增加 Help 对话框高度，同时保留居中、固定大小、可滚动和主题适配布局。
- 数据收集、保存、网络、认证、Usage、Proxy、刷新、Tooltip、图标及设置行为均未改变；本版仅在程序内说明既有隐私边界。
- 59 项自动化测试全部通过，并扩充了安全标题、凭据处理、邮箱保留、遥测及 reset 只读声明的断言。

## 0.1.26-local - 2026-09-08

### 新增

- 在托盘菜单 `About` 正上方新增 `Help`。
- 新增简洁的英文帮助对话框，说明 WebView2 与 Codex CLI 的区别、各自的基本配置步骤，以及登录和 Proxy 设置分别适用于哪种来源。
- 帮助内容明确说明两种来源都只读取 Usage 信息，程序不会消耗 Usage reset。

### 行为与验证

- 对话框居中、固定大小、可用键盘关闭，并跟随程序的 Follow Windows、Light 或 Dark 主题。
- Usage 获取、登录、Proxy、定时刷新、Tooltip、图标和设置行为均未改变。
- 59 项自动化测试全部通过，包括 Help 内容、布局、主题及既有功能和原生资源回归测试。

## 0.1.25-local - 2026-09-07

### 更新

- 5-Hour 剩余用量独立控制外围边框：超过 50% 时浅色主题为黑色、深色主题为白色，21%–50% 为橙色，0%–20% 为红色。
- Weekly 剩余用量独立控制下层填充：超过 50% 为绿色，21%–50% 为橙色，0%–20% 为红色；Weekly 不影响边框。
- 保留居中 72% 宽蓝色叠层和两项独立像素高度。Weekly 为零时不填充；异常状态保留原有标记及主题中性边框。
- 图标缓存键包含两项预警级别，确保跨越阈值时即使取整后的像素高度不变，也会切换颜色。

### 验证

- 58 项自动化测试通过，涵盖独立颜色组合、阈值边界、缓存切换与恢复、参考几何和资源释放。
- 已检查正式渲染器在两种主题下的 16/20/24/32 px 输出。真实托盘和混合 DPI 多显示器效果仍需人工验收。

## 0.1.24-local - 2026-09-07

### 更新

- 托盘图标改为已确认的满画布矩形：四边 1 像素边框，绿色 Weekly 全宽底层，蓝色 5-Hour 居中叠层占内部宽度 72%。
- 两项剩余用量分别控制从底部向上的像素高度，取消旧版 5% 分档；正值至少显示 1 像素，零值不填充。低额度时仍保持绿色和蓝色。
- 浅色/深色配色遵循现有应用主题选项，主题和显示设置通知触发重绘，不触发 Usage 获取。
- 保留 Unknown、Error、LoginRequired 的不同标记；缺失用量不会当作零值。原生图标缓存限制为 16 项。

### 验证

- 56 项自动化测试通过，涵盖两种主题下 16/20/24/32 px 逐像素样例校验、用量与状态区分、缓存上限和资源释放。
- 已检查由编译后的正式渲染器生成的预览图；真实 Windows 托盘和混合 DPI 多显示器效果仍需人工验收。
- Usage 获取、CLI、WebView2 解析、代理、定时刷新及日志保持原有行为。

## 0.1.23-local - 2026-09-06

### 修复

- 修复所有可用 Usage reset 都已用完、页面显示 `No usage limit resets available at this time.` 时，WebView2 菜单仍显示 `Usage resets: Unknown` 的问题。
- 正文区域解析器和延迟 DOM 兜底现在都会精确识别这条“明确无可用 reset”的提示。
- 识别范围仍严格限制在 `Usage limit resets` 区域以及 `Auto reload` 或 `Usage breakdown` 边界之前；含义不明确的文字及区域外同名文字仍保持 Unknown。

### 行为与验证

- 结果按现有明确零值策略缓存并显示为 `Usage resets: 0`；程序仍保持只读，不会点击或消耗 reset。
- 新增与截图一致的零值提示、模糊文案、区域边界及 DOM 约束测试；53 项自动化测试全部通过。
- 用户已确认 0.1.22 的 WebView2 禁止激活宿主修复后，Windows 11 屏保恢复正常。

## 0.1.22-local - 2026-09-06

### 修复

- 修复 WebView2 模式在周期后台刷新期间阻止或打断 Windows 11 屏保的问题。
- 回归点确认在 0.1.21 的桌面布局 reset 读取：每次 fetch 都显示了一个透明、移到屏外的 1280x900 顶层 Form；reset DOM 探针本身仍保持只读且没有改动。
- 后台 WebView2 现在只创建 controller 所需的隐藏父 HWND，不再调用 `Form.Show()`。
- 增加 `WS_EX_NOACTIVATE`、工具窗口样式和 `ShowWithoutActivation` 保护，即使将来意外调用显示，后台宿主也不能取得激活状态。

### 行为与验证

- 保留 1280x900 controller 视口和延迟 Usage reset DOM 读取；reset、额度、email、菜单、Tooltip、CLI、Proxy 和设置行为均未改变。
- 新增后台宿主不可见/禁止激活约束测试；52 项自动化测试全部通过。
- 使用临时 profile 的真实 WebView2 controller 探测在宿主从未显示的情况下成功导航，确认 DOM 视口仍为 1280x900；controller 未使用进程 kill 即完成关闭，并观察到 browser process 正常退出。
- 由于用户现有托盘实例正在占用登录 profile，本次无法并发执行 authenticated profile 探测；未修改的 DOM 探针继续以 0.1.21 已通过的真实登录 reset 读取结果为功能基线。随后用户已在 Windows 11 上确认 WebView2 模式运行时屏保可以正常启动。

## 0.1.21-local - 2026-09-05

### 修复

- 将 WebView2 仅依赖文本行的 reset 列表后备解析改为 DOM 读取，并使用应用现有的真实已登录 profile 完成验证。
- 隐藏 WebView 从 2x2 像素改用 1280x900 的桌面布局视口，避免 Usage 页面选择或延迟保留无法使用的极小布局。
- 额度解析成功后，将 `Usage limit resets` 标题滚动到可视区域，并在最长 2.75 秒内短暂重试，让延迟出现的 reset 条目有机会加载后再关闭 session。
- 只统计限定区域内标签精确为 `Use reset`、可用且可见的交互元素；禁用、隐藏、说明文字及区域外文本继续排除。

### 行为与安全

- DOM 探针严格只读：不会点击按钮、调用 reset 消耗动作，也不会把页面文字、账号、额度或 reset 数量写入日志。
- CLI 行为、菜单位置、Tooltip 内容、设置兼容性及空闲时不保留 WebView 的生命周期均不变；较大视口只在 WebView2 刷新期间存在。

### 验证

- Release x64 构建和 51 项自动化测试全部通过。
- 使用应用现有的真实已登录 WebView2 profile 实测得到 `usageResetCountRead=true`，且没有输出账号、额度或 reset 实际数值。

## 0.1.20-local - 2026-09-05

### 修复

- 修复 WebView2 页面以列表而不是数字汇总展示可用 reset 时，菜单错误显示 `Usage resets: Unknown` 的问题。
- 在有明确边界的 `Usage limit resets` 区域内，每一条精确的 `Use reset` 操作行计为一个可用 reset。
- 说明文字 `Use a reset to restore...` 以及下一个 `Auto reload` 或 `Usage breakdown` 区域之后的同名文字都不会被误计。

### 行为与安全

- 继续支持明确的数字汇总；页面明确表示没有 reset 时计为 0，区域缺失或结构不明确时仍显示 Unknown，不猜测为 0。
- CLI 解析、菜单位置、Tooltip 内容、网络请求、reset 消耗行为及日志隐私规则均不变。

### 验证

- Release x64 构建和 50 项自动化测试全部通过。
- 新增与截图结构相同的单条、多条、明确为空、CRLF、说明文字和区域外文本测试样例。

## 0.1.19-local - 2026-09-05

### 新增

- 在托盘菜单账号 Email 的下一行新增不可点击的 `Usage resets: N` 状态项；Tooltip 保持原样，不加入该信息。
- Codex CLI 模式直接读取现有 `account/rateLimits/read` 响应顶层的权威字段 `rateLimitResetCredits.availableCount`，不增加额外请求。
- WebView2 模式只在渲染后的 Usage 页面文字明确给出可用 reset 数字时读取，不使用 Cookie、Token 或私有 reset 接口。

### 行为与安全

- 数据缺失、不支持、属于其它来源或当前需要重新登录时显示 `Usage resets: Unknown`；只有来源明确返回 0 时才显示 `Usage resets: 0`。
- 现有设置文件保持兼容；数量随最近一次成功 Usage 快照缓存，但只在该快照属于当前已登录来源时显示。
- 程序仍为只读：不会调用任何 reset 消耗动作，也不会把 reset 数量写入日志。

### 验证

- Release x64 构建和 49 项自动化测试全部通过。
- 新增 CLI summary、WebView2 明确数字文案、0 与 Unknown 区分、来源不匹配、登录失效及设置序列化测试。

## 0.1.18-local - 2026-09-04

### 新增

- 新增按本地自然日轮换日志；`CodexUsageTrayLite.log` 始终作为当天的活动日志。
- 本地时间跨过 0 点后的第一次写入前，将前一个活动日志按其最后写入日期归档为 `CodexUsageTrayLite_YYYYMMDD.log`。
- 如果程序停机一天或多天，下次启动时按现有活动日志的实际最后写入日期归档，不为没有日志的日期创建空文件。
- 如果同一天的归档文件已经存在，将剩余活动日志追加到该归档，不覆盖或删除任一段日志。

### 变更

- 原有超过 2 MiB 后生成 `.log.1` 的容量轮换机制改为每日日期归档。
- `Open logs` 继续打开当前的 `CodexUsageTrayLite.log`；带日期的历史日志保留在同一个 `logs` 目录，不会自动删除。

### 验证

- Release x64 构建和 47 项自动化测试通过。
- 新增对跨午夜轮换、同日继续写入、归档文件精确命名和同日多段日志无损合并的测试。

## 0.1.17-local - 2026-09-04

### 变更

- 将双语 Changelog 拆分为英文 `CHANGELOG.md` 和中文 `CHANGELOG.zh-CN.md`。
- 将双语发布说明拆分为英文 `RELEASE_NOTES.md` 和中文 `RELEASE_NOTES.zh-CN.md`。
- 便携包白名单和内部 SHA-256 清单改为包含四份独立语言文档。
- 写入项目维护约定：双语 Markdown 文档一律使用英文基础文件名和对应的 `.zh-CN.md` 中文文件，不在同一个文件内混排两种语言。
- 程序运行功能与 `0.1.16-local` 相同。

### 验证

- Release x64 构建和 46 项自动化测试通过。
- 包内容审计确认两份英文文件和两份中文文件均为独立条目。

## 0.1.16-local - 2026-09-04

### 新增

- 为本文件补齐完整英文版本，逐版本对应保留 `0.1.0-local` 至当前版本的功能、修复、安全、发布和验证记录。
- 为 `RELEASE_NOTES.md` 补齐完整英文版本，包括当前版说明、升级须知、完整能力、运行要求、验证结果、已知限制和版本索引。

### 变更

- 两份发布文档统一为“中文在前、English 在后”的双语结构，并增加页内语言导航。
- 本版本不改变 Usage 获取、菜单、Tooltip、刷新周期或进程生命周期行为；运行功能与 `0.1.15-local` 相同。

### 验证

- Release x64 构建和 46 项自动化测试通过。
- 双语文档继续随便携 ZIP 交付，并纳入包内 SHA-256 清单。

## 0.1.15-local - 2026-09-04

### 新增

- Refresh interval 新增 `1 minute` 和 `2 minutes`，完整选项变为 1、2、5、10、15、30、60 分钟。
- 新建本文件和 `RELEASE_NOTES.md`，补录初始版至当前版的历史。
- 便携包白名单新增 `CHANGELOG.md` 和 `RELEASE_NOTES.md`。

### 变更

- 默认刷新周期保持 15 分钟；1 分钟和 2 分钟仅在用户明确选择后启用。
- 更新项目规格、测试清单和便携包说明，使其与新增周期及当前菜单/Tooltip 行为一致。

### 验证

- Release x64 构建和 46 项自动化测试通过。
- 包内文件继续使用精确白名单及内部 SHA-256 清单。

## 0.1.14-local - 2026-09-04

### 变更

- 重新整理托盘菜单：账号状态、常用操作、来源/刷新设置、程序偏好、维护信息和 Exit 分组。
- Email 与 Last updated 集中在顶部；About 与日志/配置入口集中在维护区域。
- CLI 模式隐藏 `Proxy settings (WebView2)` 和 `Clear WebView2 login session`，只保留 CLI 相关操作。
- WebView2 模式显示登录页、Proxy 和清理 WebView2 session 等专属操作。

### 验证

- 46 项自动化测试通过。

## 0.1.13-local - 2026-09-04

### 新增

- Tooltip 第一行在脱敏邮箱后增加来源短标签：`[CLI]` 或 `[WV2]`。

### 变更

- Tooltip 按 63 字符硬上限动态分配邮箱空间；普通邮箱尽量完整显示，长域名从中间压缩。
- 在 `100%`、长日期、`[cached]`、`[error]` 或 `[login]` 等极端组合下，优先保留来源标签、两条额度信息和状态标记。
- 邮箱空间极小时仍尽量保留首字符、`@` 和省略号，避免整段尾部截断。

### 验证

- 45 项自动化测试通过。

## 0.1.12-local - 2026-09-03

### 变更

- Codex CLI 根进程正常退出后，给其 Job Object 内后代进程最多 500 ms 自然退出时间，每 50 ms 检查一次。
- 后代进程在宽限期内归零时，`CodexCli.ProcessExit` 记录为 `info`；宽限期后仍存在时保持 `warning` 并由 Job Object 收尾。
- CLI 生命周期日志新增 `jobDrainWaitMs`，减少将正常的瞬时子进程退出误报为 warning。
- `Open logs` 改为直接通过 Windows 文件关联打开 `CodexUsageTrayLite.log`；日志不存在时先创建，不再只打开日志目录。

### 验证

- 45 项自动化测试通过。
- 三次真实 CLI 验证均得到正常 `info` 退出记录。

## 0.1.11-local - 2026-09-03

### 新增

- 日志格式新增结构化字段：`level=info|warning|error` 和 `event=...`。
- 成功解析账号后记录来源和脱敏邮箱，例如 `email=u***@example.com`。

### 安全

- 日志邮箱脱敏与界面规则统一为“首字符 + `***` + 完整域名”。
- 日志清洗器会再次扫描并替换意外进入详情文本的原始邮箱。
- 右键菜单仍只在进程内存中显示完整邮箱，不把完整邮箱写入设置或日志。

### 验证

- 43 项自动化测试通过。

## 0.1.10-local - 2026-09-03

### 新增

- CLI 模式从已执行的官方 `account/read` 响应读取账号邮箱。
- WebView2 模式在现有 `chatgpt.com` 页面上下文中执行同源 `/api/auth/session` 请求，尝试读取账号邮箱；失败不会影响 Usage 获取。
- Tooltip 第一行显示脱敏邮箱，右键菜单第一行显示完整邮箱。

### 变更

- Tooltip 核心标签由 `5Hours` 改为 `5-Hour`，形成三行格式：Email、5-Hour、Weekly。
- 长邮箱根据 NotifyIcon 63 字符限制自动压缩。

### 安全

- 完整邮箱仅保存在当前进程内存；切换来源、登录失效、清除 session 或退出时清除。
- 不读取或输出 Cookie、Token、Authorization header、WebView2 session 响应正文或 CLI 凭据文件。

### 验证

- 42 项自动化测试通过。
- 本机真实 CLI 和独立 WebView2 profile 均验证可取得邮箱，测试输出只显示是否成功和脱敏结果。

## 0.1.9-local - 2026-09-03

### 修复

- 修复从 CLI 切换回 WebView2 时不会立即刷新、必须手动打开登录页面才更新的问题。
- 切换到任一有效来源都会执行一次允许未配置状态的即时验证；现有 WebView2 profile 有效时无需打开登录窗口。
- 登录确实失效时保持静默 login-required 状态，等待用户主动打开登录页。
- 修复只有时间、没有日期的 reset 值错误套用系统当天日期的问题，使日期锚定更确定。

### 验证

- 41 项自动化测试通过。

## 0.1.8-local - 2026-09-02

### 新增

- 新增 CLI 和 WebView2 生命周期日志，可区分自然退出、显式关闭和异常失败。
- CLI 日志记录启动 PID、stdin 是否关闭、退出类型、退出码、是否调用精确 PID 的 `Process.Kill()`、Job Object 活跃进程数和 kill-on-close 状态。
- WebView2 日志记录 controller 创建、`CoreWebView2Controller.Close()` 返回、`processKill=false`、ProcessFailed 详情和最终 `BrowserProcessExited` 类型。

### 变更

- 进程控制信息明确区分“关闭本次启动的进程树”和“按进程名结束系统中的同名进程”；程序从不按名称结束 Codex、Node、Edge 或 WebView2。

### 验证

- 40 项自动化测试通过。
- 真实 WebView2 探针观察到 controller Close 后异步 `BrowserProcessExited=Normal`，测试 PID 随后消失且未调用 Kill。

## 0.1.7-local - 2026-09-02

### 新增

- 新增 `Usage source > WebView2 / Codex CLI`。
- CLI 模式按需启动已安装的 `codex app-server`，完成 JSONL initialize/initialized、`account/read` 和 `account/rateLimits/read`。
- 只接受 general Codex bucket，将 300 分钟窗口映射为 5-Hour、10,080 分钟窗口映射为 Weekly，并把 `usedPercent` 转换为 remaining。
- 支持定位 `codex.exe`、`codex.cmd` 和 `codex.bat`；命令脚本通过正确引用的 `cmd.exe` 启动。
- 每次读取使用独立 Job Object 和私有标准输入/输出，结束后关闭本次进程树。

### 变更

- CLI 模式不应用 WebView2 Proxy，也不操作 WebView2 login session。
- CLI 不读取 `auth.json`、Token 或其它凭据文件；复用已安装 CLI 自己的登录状态和网络配置。
- CLI 为可选依赖，不打入 ZIP，不增加 DLL 或运行时；便携包只增加约 10.7 KB。

### 验证

- 37 项自动化测试通过。
- 本机已登录 CLI 的真实验证成功读取两个额度窗口，耗时约 1.681 秒，未打印额度或账号内容，也未遗留新进程。

## 0.1.6-local - 2026-09-02

### 变更

- Option A 电池图标整体等比放大约 10.4%，达到 Windows 当前托盘槽位允许的安全最大视觉尺寸。
- 保持电池主体严格 3:4 比例；主体、触点、描边、填充和状态标记使用同一倍率，避免纵横失真。
- 不再固定提供 32×32 源图，而是读取 Windows `SM_CXSMICON`，在 16–32 px 范围内按当前系统指标生成原生图标。

### 调研

- 评估 `codex-usage-widget` 的 CLI app-server 读取方式、包体和资源占用；确认轻量按需 CLI 模式可行，但本版本尚未加入 CLI 功能。

### 验证

- 32 项自动化测试通过。

## 0.1.5-local - 2026-09-02

### 新增

- 新增 `Theme > Follow Windows / Light / Dark`，选择持久化保存。
- 主题应用于托盘菜单、Proxy、About、登录窗口和 Windows 11 标题栏。
- WebView2 使用官方 PreferredColorScheme；第三方 OAuth 页面可以保留自己的主题。

### 变更

- 旧设置自动迁移到 `Follow Windows`，不改变已配置的刷新状态。

### 验证

- 31 项自动化测试通过。

## 0.1.4-local - 2026-09-02

### 新增

- Exit 上方新增 About 菜单和屏幕居中的 About 对话框。
- About 显示程序名、版本、x64 架构、.NET 目标、UTC 构建时间、WebView2 SDK/Runtime、上游 baseline 和 MIT 许可证信息。

### 修复

- Proxy Settings 从屏幕左上角改为当前屏幕中央显示。

### 发布

- 同步更新工作目录内的松散测试副本；随后将容易产生版本歧义的 `CodexUsageTrayLite-v0.1.0-local` 改为版本中立的 `artifacts/CodexUsageTrayLite-local`。目录改名没有另增程序版本。

### 验证

- 29 项自动化测试通过。

## 0.1.3-local - 2026-09-02

### 变更

- 全新、未配置的 WebView2 安装不再执行 4 秒启动刷新，也不启动周期刷新或创建隐藏 WebView2。
- 未完成首次登录时 `Refresh now` 置灰；仅修改 Proxy 不会触发后台请求。
- 用户主动打开登录窗口并关闭后执行一次验证；首次成功取得 Usage 后才启用手动和周期刷新。
- 登录失效或清除 login session 后停止定时器并重新禁用 WebView2 手动刷新。
- 旧设置按是否已有成功 Usage 和 loginRequired 状态迁移，避免破坏已配置安装。

### 验证

- 28 项自动化测试通过。

## 0.1.2-local - 2026-09-02

### 修复

- 修复启动隐藏刷新忙碌时点击 `Open login / usage page` 没反应的问题。
- 打开登录窗口或 Proxy Settings 时先取消并释放当前隐藏 fetch，再执行用户操作。
- 明确区分主动取消和网络超时，主动取消不再误显示 `[error]`。
- WebView2 初始化失败时显示具体错误；修复取消期间页面解析与 controller 释放的竞态。
- 普通 Microsoft Edge 窗口保持独立，不需要关闭，也不会被本程序控制。

### 验证

- 26 项自动化测试通过。

## 0.1.1-local - 2026-09-01

### 新增

- 建立标准版本化便携发布流程 `scripts/package-release.ps1`。
- 每次打包自动校验版本字段、还原依赖、Release x64 构建、运行自动化测试、生成内部 `SHA256SUMS.txt`、ZIP 和外部 `.zip.sha256`。
- 打包脚本使用精确文件白名单，排除 PDB、profile、settings、logs、dump、secret、token、cookie、环境文件和构建工具。
- 当前版本 ZIP 或 sidecar 已存在时立即拒绝覆盖，旧版本永久保留。

### 发布

- 首个版本化包为 `CodexUsageTrayLite-v0.1.1-local-win-x64.zip`。
- 目标 Windows 11 x64 仍需 .NET Framework 4.8+；WebView2 模式需要 Evergreen Runtime。

### 验证

- 25 项自动化测试通过。
- ZIP 白名单、内部清单、外部校验和禁止覆盖机制验证通过。

## 0.1.0-local - 2026-09-01

`0.1.0-local` 是初始开发阶段的松散 artifact，同一天内曾在不改变版本号的情况下迭代，因此以下记录代表该阶段最终状态；从下一版开始停止这种覆盖式开发。

### 新增

- 从 MIT 许可的 `saveway/codex-usage-monitor` `v2.0.0-preview.7`（commit `36e9679164dcd7e5ef23d1f35822664785fad01f`）建立独立的 .NET Framework 4.8、WinForms、x64 项目。
- 创建纯系统托盘架构，移除新 EXE 中的 Widget、Overlay、图表、Toast、BalloonTip 和额度弹窗。
- 按需创建隐藏 WebView2 controller；每次 fetch 完成、失败、超时或取消后解绑事件、Close controller、释放 host Form。登录状态保存在本应用独立的 `%LOCALAPPDATA%/CodexUsageTrayLite/webview2-profile`。
- 新增 System、Direct、Custom HTTP、Custom SOCKS5 Proxy；Direct 使用 `--no-proxy-server`，Proxy 参数经过验证并防止浏览器参数注入。
- 新增原创竖立电池托盘图标 A–D 方案和 16/20/24/32 px 预览；最终由较窄 Option B 切换到 Option A。
- 图标表示 5 小时 remaining，支持正常、提醒、低额度、Unknown、Error、LoginRequired 和真实 0% 状态；动态图标缓存并正确释放 HICON/GDI 对象。
- Tooltip 显示 5 小时/Weekly remaining 和本地 reset 时间，并在 63 字符范围内保留 cached/error/login 状态。
- 提供 5、10、15、30、60 分钟刷新周期，默认 15 分钟；启动刷新延迟 4 秒。
- 新增 Start at login、Last updated、Open logs/config、Clear login session、安全日志和单实例行为。
- 创建 `PROJECT_SPEC.md`、`LOCAL_DEV_NOTES.md`、`TESTING.md`、Solution、测试 harness 和图标设计文档。

### 初始阶段修复

- Tooltip 文案调整为对齐的 `5Hours ...` / `Weekly ...` 格式。
- Proxy 对话框从 390×205 逐步扩大到 620×340，加入 Direct 选项，缩短标签并修复高 DPI 文字重叠。
- 托盘图标由 Option B 切换为 Option A。

### 验证与已知限制

- 初始实现 22 项测试通过；该版本最终一次原地更新为 25 项测试通过。
- 100 次注入式 refresh 的 session 创建/释放和 GDI 检查通过。
- 真实 WebView2 快速 50 次 controller 测试确认最终 browser process 退出，但宿主 Handle 从 286 增至 475；8–12 小时自然间隔 soak 仍是公开发布前的验证要求。
