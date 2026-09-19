using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CodexUsageTrayLite.Infrastructure;
using CodexUsageTrayLite.Models;
using CodexUsageTrayLite.Rendering;
using CodexUsageTrayLite.Services;
using CodexUsageTrayLite.UI;

namespace CodexUsageTrayLite
{
    internal sealed class TrayApplicationContext : ApplicationContext, IDisposable
    {
        private readonly NotifyIcon notifyIcon;
        private readonly ContextMenuStrip menu;
        private readonly ToolStripMenuItem accountEmailItem;
        private readonly ToolStripMenuItem userLevelItem;
        private readonly ToolStripMenuItem fiveHourUsageItem;
        private readonly ToolStripMenuItem weeklyUsageItem;
        private readonly ToolStripMenuItem usageResetsItem;
        private readonly ToolStripMenuItem lastUpdatedItem;
        private readonly ToolStripMenuItem startAtLoginItem;
        private readonly ToolStripMenuItem refreshNowItem;
        private readonly ToolStripMenuItem openLoginItem;
        private readonly ToolStripMenuItem proxySettingsItem;
        private readonly ToolStripMenuItem clearSessionItem;
        private readonly ToolStripMenuItem usageSourceMenuItem;
        private readonly ToolStripMenuItem refreshIntervalMenuItem;
        private readonly ToolStripMenuItem quotaIconModeMenuItem;
        private readonly ToolStripMenuItem themeMenuItem;
        private readonly ToolStripMenuItem languageMenuItem;
        private readonly ToolStripMenuItem openLogsItem;
        private readonly ToolStripMenuItem openConfigItem;
        private readonly ToolStripMenuItem helpItem;
        private readonly ToolStripMenuItem aboutItem;
        private readonly ToolStripMenuItem exitItem;
        private readonly Dictionary<int, ToolStripMenuItem> intervalItems = new Dictionary<int, ToolStripMenuItem>();
        private readonly Dictionary<QuotaIconMode, ToolStripMenuItem> quotaIconModeItems = new Dictionary<QuotaIconMode, ToolStripMenuItem>();
        private readonly Dictionary<AppThemeMode, ToolStripMenuItem> themeItems = new Dictionary<AppThemeMode, ToolStripMenuItem>();
        private readonly Dictionary<UsageSource, ToolStripMenuItem> sourceItems = new Dictionary<UsageSource, ToolStripMenuItem>();
        private readonly Dictionary<AppLanguage, ToolStripMenuItem> languageItems = new Dictionary<AppLanguage, ToolStripMenuItem>();
        private readonly System.Windows.Forms.Timer refreshTimer;
        private readonly System.Windows.Forms.Timer startupTimer;
        private readonly BatteryTrayIconRenderer iconRenderer;
        private readonly UsageFetchService fetchService;
        private readonly CancellationTokenSource applicationCancellation = new CancellationTokenSource();
        private AppSettings settings;
        private UiText ui;
        private LoginBrowserForm loginForm;
        private Task activeRefreshTask = Task.CompletedTask;
        private TrayVisualState visualState;
        private bool showingCachedData;
        private bool interactiveActionPending;
        private bool exiting;
        private bool disposed;
        private string currentAccountEmail;
        private UsageSource? currentAccountEmailSource;

        public TrayApplicationContext()
        {
            settings = SettingsStore.Load();
            ui = UiText.For(settings.language);
            iconRenderer = new BatteryTrayIconRenderer();
            fetchService = new UsageFetchService();
            visualState = settings.loginRequired
                ? TrayVisualState.LoginRequired
                : settings.lastSuccessfulUsage == null ? TrayVisualState.Unknown : TrayVisualState.Normal;
            showingCachedData = settings.lastSuccessfulUsage != null;

            menu = new ContextMenuStrip();
            accountEmailItem = AddMenu(ui.FormatAccountEmail(null), null);
            accountEmailItem.Enabled = false;
            userLevelItem = AddMenu(ui.FormatUserLevel(UserLevel.Unknown), null);
            userLevelItem.Enabled = false;
            fiveHourUsageItem = AddMenu(TooltipFormatter.BuildFiveHourMenuLine(null), null);
            fiveHourUsageItem.Enabled = false;
            weeklyUsageItem = AddMenu(TooltipFormatter.BuildWeeklyMenuLine(null, visualState, showingCachedData), null);
            weeklyUsageItem.Enabled = false;
            usageResetsItem = AddMenu(ui.FormatUsageResets(null), null);
            usageResetsItem.Enabled = false;
            lastUpdatedItem = AddMenu(ui.FormatLastUpdated(null, settings.lastSuccessfulUsageSource), null);
            lastUpdatedItem.Enabled = false;
            menu.Items.Add(new ToolStripSeparator());

            refreshNowItem = AddMenu(ui.RefreshNow, (sender, args) => StartRefresh(true));
            openLoginItem = AddMenu(ui.OpenLogin, async (sender, args) => await HandleOpenLoginAsync());
            menu.Items.Add(new ToolStripSeparator());

            usageSourceMenuItem = CreateUsageSourceMenu();
            menu.Items.Add(usageSourceMenuItem);
            refreshIntervalMenuItem = CreateIntervalMenu();
            menu.Items.Add(refreshIntervalMenuItem);
            quotaIconModeMenuItem = CreateQuotaIconModeMenu();
            menu.Items.Add(quotaIconModeMenuItem);
            proxySettingsItem = AddMenu(ui.ProxySettings, async (sender, args) => await OpenProxySettingsAsync());
            clearSessionItem = AddMenu(ui.ClearSession, async (sender, args) => await ClearLoginSessionAsync());
            menu.Items.Add(new ToolStripSeparator());

            startAtLoginItem = AddMenu(ui.StartAtLogin, HandleStartAtLogin);
            startAtLoginItem.CheckOnClick = false;
            themeMenuItem = CreateThemeMenu();
            menu.Items.Add(themeMenuItem);
            languageMenuItem = CreateLanguageMenu();
            menu.Items.Add(languageMenuItem);
            menu.Items.Add(new ToolStripSeparator());

            openLogsItem = AddMenu(ui.OpenLogs, (sender, args) => OpenLogFile());
            openConfigItem = AddMenu(ui.OpenConfig, (sender, args) => OpenFolder(AppPaths.DataDirectory));
            helpItem = AddMenu(ui.Help, (sender, args) => ShowHelp());
            aboutItem = AddMenu(ui.About, (sender, args) => ShowAbout());
            menu.Items.Add(new ToolStripSeparator());

            exitItem = AddMenu(ui.Exit, async (sender, args) => await ExitApplicationAsync());
            ApplyLanguage(settings.language, false);
            ApplyTheme(settings.themeMode, false);
            menu.Opening += (sender, args) => RefreshMenuState();

            notifyIcon = new NotifyIcon
            {
                ContextMenuStrip = menu,
                Visible = false
            };
            notifyIcon.DoubleClick += (sender, args) => StartRefresh(true);

            refreshTimer = new System.Windows.Forms.Timer();
            refreshTimer.Tick += (sender, args) => StartRefresh(false);
            startupTimer = new System.Windows.Forms.Timer { Interval = 4000 };
            startupTimer.Tick += (sender, args) =>
            {
                startupTimer.Stop();
                StartRefresh(false);
            };
            ApplyRefreshInterval(settings.refreshIntervalMinutes, false);
            UpdateDisplay();
            notifyIcon.Visible = true;
            var themeDispatchHandle = menu.Handle;
            Microsoft.Win32.SystemEvents.UserPreferenceChanged += HandleVisualPreferenceChanged;
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged += HandleDisplaySettingsChanged;
            if (settings.CanRefreshUsage()) startupTimer.Start();
        }

        protected override void ExitThreadCore()
        {
            if (disposed)
            {
                base.ExitThreadCore();
                return;
            }
            disposed = true;
            Microsoft.Win32.SystemEvents.UserPreferenceChanged -= HandleVisualPreferenceChanged;
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged -= HandleDisplaySettingsChanged;
            exiting = true;
            SafeLogger.Write("Shutdown.Start");
            refreshTimer.Stop();
            startupTimer.Stop();
            applicationCancellation.Cancel();
            fetchService.Stop();
            if (loginForm != null)
            {
                var form = loginForm;
                loginForm = null;
                form.FormClosed -= HandleLoginFormClosed;
                form.Close();
                form.Dispose();
            }
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
            menu.Dispose();
            refreshTimer.Dispose();
            startupTimer.Dispose();
            fetchService.Dispose();
            WebViewEnvironmentFactory.ClearCache();
            applicationCancellation.Dispose();
            iconRenderer.Dispose();
            SafeLogger.Write("Shutdown.Complete");
            base.ExitThreadCore();
        }

        public new void Dispose()
        {
            if (!disposed) ExitThreadCore();
            base.Dispose();
        }

        private ToolStripMenuItem AddMenu(string text, EventHandler handler)
        {
            var item = new ToolStripMenuItem(text);
            if (handler != null) item.Click += handler;
            menu.Items.Add(item);
            return item;
        }

        private ToolStripMenuItem CreateIntervalMenu()
        {
            var parent = new ToolStripMenuItem(ui.RefreshInterval);
            foreach (var minutes in SettingsStore.AllowedRefreshIntervals)
            {
                var localMinutes = minutes;
                var item = new ToolStripMenuItem(ui.FormatMinutes(minutes));
                item.Click += (sender, args) => ApplyRefreshInterval(localMinutes, true);
                intervalItems.Add(minutes, item);
                parent.DropDownItems.Add(item);
            }
            return parent;
        }

        private ToolStripMenuItem CreateThemeMenu()
        {
            var parent = new ToolStripMenuItem(ui.Theme);
            AddThemeItem(parent, AppThemeMode.System, ui.ThemeFollowWindows);
            AddThemeItem(parent, AppThemeMode.Light, ui.ThemeLight);
            AddThemeItem(parent, AppThemeMode.Dark, ui.ThemeDark);
            return parent;
        }

        private ToolStripMenuItem CreateQuotaIconModeMenu()
        {
            var parent = new ToolStripMenuItem(ui.IconStyleMenu);
            AddQuotaIconModeItem(parent, QuotaIconMode.FiveHourOnly, ui.IconStyleFiveHourOnly);
            AddQuotaIconModeItem(parent, QuotaIconMode.WeeklyOnly, ui.IconStyleWeeklyOnly);
            AddQuotaIconModeItem(parent, QuotaIconMode.Both, ui.IconStyleBoth);
            AddQuotaIconModeItem(parent, QuotaIconMode.SideBySide, ui.IconStyleSideBySide);
            return parent;
        }

        private ToolStripMenuItem CreateLanguageMenu()
        {
            var parent = new ToolStripMenuItem(ui.LanguageMenu);
            AddLanguageItem(parent, AppLanguage.English, ui.LanguageEnglish);
            AddLanguageItem(parent, AppLanguage.ChineseSimplified, ui.LanguageChineseSimplified);
            AddLanguageItem(parent, AppLanguage.ChineseTraditional, ui.LanguageChineseTraditional);
            AddLanguageItem(parent, AppLanguage.Japanese, ui.LanguageJapanese);
            AddLanguageItem(parent, AppLanguage.Korean, ui.LanguageKorean);
            return parent;
        }

        private ToolStripMenuItem CreateUsageSourceMenu()
        {
            var parent = new ToolStripMenuItem(ui.UsageSourceMenu);
            AddUsageSourceItem(parent, UsageSource.WebView2, ui.UsageSourceWebView2);
            AddUsageSourceItem(parent, UsageSource.CodexCli, ui.UsageSourceCodexCli);
            return parent;
        }

        private void AddUsageSourceItem(ToolStripMenuItem parent, UsageSource source, string text)
        {
            var localSource = source;
            var item = new ToolStripMenuItem(text);
            item.Click += async (sender, args) => await ApplyUsageSourceAsync(localSource);
            sourceItems.Add(source, item);
            parent.DropDownItems.Add(item);
        }

        private void AddThemeItem(ToolStripMenuItem parent, AppThemeMode mode, string text)
        {
            var localMode = mode;
            var item = new ToolStripMenuItem(text);
            item.Click += (sender, args) => ApplyTheme(localMode, true);
            themeItems.Add(mode, item);
            parent.DropDownItems.Add(item);
        }

        private void AddQuotaIconModeItem(ToolStripMenuItem parent, QuotaIconMode mode, string text)
        {
            var localMode = mode;
            var item = new ToolStripMenuItem(text);
            item.Click += (sender, args) => ApplyQuotaIconMode(localMode, true);
            quotaIconModeItems.Add(mode, item);
            parent.DropDownItems.Add(item);
        }

        private void AddLanguageItem(ToolStripMenuItem parent, AppLanguage language, string text)
        {
            var localLanguage = language;
            var item = new ToolStripMenuItem(text);
            item.Click += (sender, args) => ApplyLanguage(localLanguage, true);
            languageItems.Add(language, item);
            parent.DropDownItems.Add(item);
        }

        private void ApplyLanguage(AppLanguage language, bool save)
        {
            settings.language = LanguageService.Normalize(language);
            ui = UiText.For(settings.language);
            refreshNowItem.Text = ui.RefreshNow;
            usageSourceMenuItem.Text = ui.UsageSourceMenu;
            refreshIntervalMenuItem.Text = ui.RefreshInterval;
            quotaIconModeMenuItem.Text = ui.IconStyleMenu;
            proxySettingsItem.Text = ui.ProxySettings;
            clearSessionItem.Text = ui.ClearSession;
            startAtLoginItem.Text = ui.StartAtLogin;
            themeMenuItem.Text = ui.Theme;
            languageMenuItem.Text = ui.LanguageMenu;
            openLogsItem.Text = ui.OpenLogs;
            openConfigItem.Text = ui.OpenConfig;
            helpItem.Text = ui.Help;
            aboutItem.Text = ui.About;
            exitItem.Text = ui.Exit;
            sourceItems[UsageSource.WebView2].Text = ui.UsageSourceWebView2;
            sourceItems[UsageSource.CodexCli].Text = ui.UsageSourceCodexCli;
            quotaIconModeItems[QuotaIconMode.FiveHourOnly].Text = ui.IconStyleFiveHourOnly;
            quotaIconModeItems[QuotaIconMode.WeeklyOnly].Text = ui.IconStyleWeeklyOnly;
            quotaIconModeItems[QuotaIconMode.Both].Text = ui.IconStyleBoth;
            quotaIconModeItems[QuotaIconMode.SideBySide].Text = ui.IconStyleSideBySide;
            themeItems[AppThemeMode.System].Text = ui.ThemeFollowWindows;
            themeItems[AppThemeMode.Light].Text = ui.ThemeLight;
            themeItems[AppThemeMode.Dark].Text = ui.ThemeDark;
            foreach (var pair in intervalItems) pair.Value.Text = ui.FormatMinutes(pair.Key);
            foreach (var pair in languageItems) pair.Value.Checked = pair.Key == settings.language;
            if (loginForm != null && !loginForm.IsDisposed) loginForm.ApplyLanguage(settings.language);
            RefreshMenuState();
            ThemeService.ApplyToMenu(menu, settings.themeMode);
            if (save)
            {
                SaveSettingsSafely();
                SafeLogger.Write("Language.Changed", "language=" + settings.language);
            }
        }

        private void ApplyTheme(AppThemeMode mode, bool save)
        {
            settings.themeMode = ThemeService.Normalize(mode);
            ThemeService.ApplyToMenu(menu, settings.themeMode);
            foreach (var pair in themeItems) pair.Value.Checked = pair.Key == settings.themeMode;
            if (loginForm != null && !loginForm.IsDisposed) loginForm.ApplyAppTheme(settings.themeMode);
            if (notifyIcon != null) UpdateTrayIcon();
            if (save)
            {
                SaveSettingsSafely();
                SafeLogger.Write("Theme.Changed", "mode=" + settings.themeMode);
            }
        }

        private void ApplyQuotaIconMode(QuotaIconMode mode, bool save)
        {
            settings.quotaIconMode = Enum.IsDefined(typeof(QuotaIconMode), mode)
                ? mode
                : QuotaIconMode.Both;
            foreach (var pair in quotaIconModeItems) pair.Value.Checked = pair.Key == settings.quotaIconMode;
            UpdateDisplay();
            if (save)
            {
                SaveSettingsSafely();
                SafeLogger.Write("IconMode.Changed", "mode=" + settings.quotaIconMode);
            }
        }

        private void ApplyRefreshInterval(int minutes, bool save)
        {
            if (!SettingsStore.IsAllowedRefreshInterval(minutes)) minutes = AppConstants.DefaultRefreshMinutes;
            settings.refreshIntervalMinutes = minutes;
            refreshTimer.Interval = checked(minutes * 60 * 1000);
            UpdateRefreshTimer();
            foreach (var pair in intervalItems) pair.Value.Checked = pair.Key == minutes;
            if (save) SaveSettingsSafely();
        }

        private async Task ApplyUsageSourceAsync(UsageSource source)
        {
            if (source == settings.usageSource || exiting || interactiveActionPending) return;
            interactiveActionPending = true;
            var validateSelectedSource = false;
            try
            {
                startupTimer.Stop();
                refreshTimer.Stop();
                if (!await CancelActiveRefreshAsync("usage-source")) return;
                if (loginForm != null)
                {
                    var form = loginForm;
                    loginForm = null;
                    form.FormClosed -= HandleLoginFormClosed;
                    form.Close();
                    form.Dispose();
                }
                settings.usageSource = source;
                ClearCurrentAccountEmail();
                settings.setupComplete = false;
                settings.loginRequired = true;
                visualState = TrayVisualState.LoginRequired;
                showingCachedData = settings.lastSuccessfulUsage != null;
                SaveSettingsSafely();
                UpdateDisplay();
                SafeLogger.Write("UsageSource.Changed", "source=" + source);
                validateSelectedSource = ShouldValidateSourceAfterSwitch(source);
            }
            finally
            {
                interactiveActionPending = false;
                RefreshMenuState();
            }
            if (validateSelectedSource && !exiting) StartRefresh(true, true);
            else UpdateRefreshTimer();
        }

        internal static bool ShouldValidateSourceAfterSwitch(UsageSource source)
        {
            return source == UsageSource.WebView2 || source == UsageSource.CodexCli;
        }

        private void StartRefresh(bool manual, bool allowUnconfigured = false)
        {
            var allowed = manual ? settings.CanManuallyRefreshUsage() : settings.CanRefreshUsage();
            if (!allowUnconfigured && !allowed) return;
            if (exiting || interactiveActionPending || fetchService.IsBusy || (loginForm != null && !loginForm.IsDisposed))
            {
                return;
            }
            refreshTimer.Stop();
            activeRefreshTask = RefreshAsync(manual);
        }

        private async Task RefreshAsync(bool manual)
        {
            refreshNowItem.Enabled = false;
            try
            {
                var outcome = await fetchService.FetchAsync(settings.usageSource, settings.GetProxySettings(), applicationCancellation.Token);
                if (outcome.IsSuccess)
                {
                    if (outcome.AccountEmailRead)
                    {
                        currentAccountEmail = AccountEmailPrivacy.Normalize(outcome.AccountEmail);
                        currentAccountEmailSource = settings.usageSource;
                        SafeLogger.Write(
                            currentAccountEmail == null ? LogEventLevel.Warning : LogEventLevel.Info,
                            currentAccountEmail == null ? "Account.EmailUnavailable" : "Account.EmailResolved",
                            "source=" + settings.usageSource + " email=" + (AccountEmailPrivacy.MaskForDisplay(currentAccountEmail) ?? "unavailable"));
                    }
                    settings.lastSuccessfulUsage = outcome.Snapshot.Clone();
                    settings.lastSuccessfulFetchTime = outcome.Snapshot.lastSuccessfulFetchAt.ToString("o", CultureInfo.InvariantCulture);
                    settings.setupComplete = true;
                    settings.loginRequired = false;
                    settings.lastSuccessfulUsageSource = settings.usageSource;
                    visualState = TrayVisualState.Normal;
                    showingCachedData = false;
                    SaveSettingsSafely();
                }
                else if (outcome.Status == FetchStatus.LoginRequired)
                {
                    ClearCurrentAccountEmail();
                    settings.setupComplete = false;
                    settings.loginRequired = true;
                    visualState = TrayVisualState.LoginRequired;
                    SaveSettingsSafely();
                }
                else if (outcome.Status != FetchStatus.Busy && outcome.Status != FetchStatus.Cancelled)
                {
                    visualState = TrayVisualState.FetchError;
                }
                UpdateDisplay();
            }
            finally
            {
                UpdateRefreshTimer();
                RefreshMenuState();
            }
        }

        private async Task OpenLoginBrowserAsync()
        {
            if (settings.usageSource != UsageSource.WebView2) return;
            if (exiting) return;
            if (loginForm != null && !loginForm.IsDisposed)
            {
                loginForm.Activate();
                return;
            }
            if (interactiveActionPending) return;

            interactiveActionPending = true;
            var opened = false;
            try
            {
                startupTimer.Stop();
                refreshTimer.Stop();
                if (!await CancelActiveRefreshAsync("open-login")) return;
                refreshTimer.Stop();
                if (exiting) return;

                SafeLogger.Write("WebView.LoginRequested");
                loginForm = new LoginBrowserForm(settings.GetProxySettings(), settings.themeMode, settings.language);
                loginForm.FormClosed += HandleLoginFormClosed;
                loginForm.Show();
                opened = true;
            }
            catch (Exception ex)
            {
                SafeLogger.Write("WebView.LoginOpenFailed", ex.GetType().Name + ": " + ex.Message);
                if (loginForm != null)
                {
                    loginForm.FormClosed -= HandleLoginFormClosed;
                    loginForm.Dispose();
                    loginForm = null;
                }
                ShowMessage(ui.LoginOpenFailed, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                interactiveActionPending = false;
                if (!exiting && !opened && (loginForm == null || loginForm.IsDisposed)) UpdateRefreshTimer();
                RefreshMenuState();
            }
        }

        private async Task HandleOpenLoginAsync()
        {
            if (settings.usageSource == UsageSource.CodexCli)
            {
                ShowMessage(ui.CodexCliHelpMessage, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            await OpenLoginBrowserAsync();
        }

        private void HandleLoginFormClosed(object sender, FormClosedEventArgs args)
        {
            var form = loginForm;
            loginForm = null;
            if (form != null)
            {
                form.FormClosed -= HandleLoginFormClosed;
                form.Dispose();
            }
            if (!exiting) StartRefresh(false, true);
        }

        private async Task OpenProxySettingsAsync()
        {
            if (settings.usageSource != UsageSource.WebView2) return;
            if (exiting) return;
            if (loginForm != null && !loginForm.IsDisposed)
            {
                ShowMessage(ui.CloseLoginBeforeProxy, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (interactiveActionPending) return;

            interactiveActionPending = true;
            var changed = false;
            try
            {
                startupTimer.Stop();
                refreshTimer.Stop();
                if (!await CancelActiveRefreshAsync("proxy-settings")) return;
                refreshTimer.Stop();

                using (var dialog = new ProxySettingsForm(settings.GetProxySettings(), settings.themeMode, settings.language))
                {
                    if (dialog.ShowDialog() != DialogResult.OK) return;
                    var proxy = dialog.SelectedProxy;
                    settings.proxyMode = proxy.mode;
                    settings.proxyHost = proxy.host;
                    settings.proxyPort = proxy.port;
                    WebViewEnvironmentFactory.ClearCache();
                    SaveSettingsSafely();
                    SafeLogger.Write("Proxy.Changed", "mode=" + proxy.mode);
                    changed = true;
                }
            }
            finally
            {
                interactiveActionPending = false;
                if (!exiting)
                {
                    if (changed && settings.usageSource == UsageSource.WebView2 && settings.CanRefreshUsage()) StartRefresh(false);
                    else UpdateRefreshTimer();
                }
                RefreshMenuState();
            }
        }

        private async Task<bool> CancelActiveRefreshAsync(string reason)
        {
            if (!fetchService.IsBusy) return true;

            fetchService.CancelActive(reason);
            var refresh = activeRefreshTask ?? Task.CompletedTask;
            var completed = await Task.WhenAny(refresh, Task.Delay(TimeSpan.FromSeconds(5)));
            if (completed == refresh && !fetchService.IsBusy) return true;

            SafeLogger.Write("Refresh.CancelWaitTimeout", "reason=" + reason);
            ShowMessage(ui.RefreshStillStopping, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        private void HandleStartAtLogin(object sender, EventArgs args)
        {
            try
            {
                StartupManager.SetEnabled(!StartupManager.IsEnabled());
                startAtLoginItem.Checked = StartupManager.IsEnabled();
                SafeLogger.Write("Startup.Changed", "enabled=" + startAtLoginItem.Checked);
            }
            catch (Exception ex)
            {
                SafeLogger.Write("Startup.ChangeFailed", ex.GetType().Name + ": " + ex.Message);
                ShowMessage(ui.StartupChangeFailed, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task ClearLoginSessionAsync()
        {
            if (settings.usageSource != UsageSource.WebView2) return;
            if (fetchService.IsBusy) return;
            var answer = ShowMessage(ui.ClearSessionPrompt, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (answer != DialogResult.Yes) return;
            if (loginForm != null)
            {
                var form = loginForm;
                loginForm = null;
                form.FormClosed -= HandleLoginFormClosed;
                form.Close();
                form.Dispose();
            }
            try
            {
                WebViewEnvironmentFactory.ClearCache();
                await Task.Run((Action)ProfileSessionManager.ClearOwnProfile);
                ClearCurrentAccountEmail();
                settings.setupComplete = false;
                settings.loginRequired = true;
                visualState = TrayVisualState.LoginRequired;
                SaveSettingsSafely();
                UpdateRefreshTimer();
                UpdateDisplay();
                SafeLogger.Write("Session.Cleared");
            }
            catch (Exception ex)
            {
                SafeLogger.Write("Session.ClearFailed", ex.GetType().Name + ": " + ex.Message);
                ShowMessage(ui.ClearSessionFailed, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshMenuState()
        {
            if (settings.themeMode == AppThemeMode.System) ApplyTheme(AppThemeMode.System, false);
            try
            {
                startAtLoginItem.Checked = StartupManager.IsEnabled();
            }
            catch (Exception ex)
            {
                startAtLoginItem.Checked = false;
                SafeLogger.Write("Startup.ReadFailed", ex.GetType().Name + ": " + ex.Message);
            }
            foreach (var pair in sourceItems) pair.Value.Checked = pair.Key == settings.usageSource;
            foreach (var pair in quotaIconModeItems) pair.Value.Checked = pair.Key == settings.quotaIconMode;
            var snapshot = settings.lastSuccessfulUsage;
            accountEmailItem.Text = ui.FormatAccountEmail(CurrentAccountEmail());
            userLevelItem.Text = ui.FormatUserLevel(CurrentUserLevel());
            fiveHourUsageItem.Text = TooltipFormatter.BuildFiveHourMenuLine(snapshot);
            weeklyUsageItem.Text = TooltipFormatter.BuildWeeklyMenuLine(snapshot, visualState, showingCachedData);
            usageResetsItem.Text = ui.FormatUsageResets(CurrentAvailableUsageResetCount());
            var webViewSource = settings.usageSource == UsageSource.WebView2;
            openLoginItem.Text = webViewSource ? ui.OpenLogin : ui.CodexCliLoginHelp;
            proxySettingsItem.Visible = ShouldShowWebViewOnlyMenuItems(settings.usageSource);
            clearSessionItem.Visible = ShouldShowWebViewOnlyMenuItems(settings.usageSource);
            proxySettingsItem.Enabled = webViewSource && !interactiveActionPending && !fetchService.IsBusy;
            clearSessionItem.Enabled = webViewSource && !interactiveActionPending && !fetchService.IsBusy;
            refreshNowItem.Enabled = settings.CanManuallyRefreshUsage() && !interactiveActionPending && !fetchService.IsBusy && (loginForm == null || loginForm.IsDisposed);
            lastUpdatedItem.Text = snapshot == null || snapshot.lastSuccessfulFetchAt == default(DateTime)
                ? ui.FormatLastUpdated(null, settings.lastSuccessfulUsageSource)
                : ui.FormatLastUpdated(snapshot.lastSuccessfulFetchAt, settings.lastSuccessfulUsageSource);
        }

        internal static bool ShouldShowWebViewOnlyMenuItems(UsageSource source)
        {
            return source == UsageSource.WebView2;
        }

        private void UpdateDisplay()
        {
            var snapshot = settings.lastSuccessfulUsage;
            UpdateTrayIcon();
            notifyIcon.Text = TooltipFormatter.Build(snapshot, visualState, showingCachedData, settings.quotaIconMode);
            RefreshMenuState();
        }

        private void HandleVisualPreferenceChanged(object sender, Microsoft.Win32.UserPreferenceChangedEventArgs args)
        {
            QueueIconRefresh();
        }

        private void HandleDisplaySettingsChanged(object sender, EventArgs args)
        {
            QueueIconRefresh();
        }

        private void QueueIconRefresh()
        {
            if (disposed || exiting || menu.IsDisposed) return;
            try
            {
                menu.BeginInvoke(new Action(() => { if (!disposed && !exiting) UpdateTrayIcon(); }));
            }
            catch (InvalidOperationException) { }
        }

        private void UpdateTrayIcon()
        {
            var snapshot = settings.lastSuccessfulUsage;
            notifyIcon.Icon = iconRenderer.GetDualIcon(snapshot == null ? null : snapshot.fiveHourRemainingPercent,
                snapshot == null ? null : snapshot.weeklyRemainingPercent, visualState,
                ThemeService.ResolveEffectiveMode(settings.themeMode) == AppThemeMode.Dark,
                snapshot == null ? null : snapshot.KnownFiveHourLimitApplicability,
                settings.quotaIconMode,
                ShouldShowFiveHourNotApplicableInfo(snapshot));
        }

        internal static bool ShouldShowFiveHourNotApplicableInfo(UsageSnapshot snapshot)
        {
            return snapshot != null &&
                snapshot.userLevel == UserLevel.Pro &&
                snapshot.fiveHourLimitApplies == false &&
                !snapshot.fiveHourRemainingPercent.HasValue;
        }

        private string CurrentAccountEmail()
        {
            return currentAccountEmailSource.HasValue && currentAccountEmailSource.Value == settings.usageSource
                ? currentAccountEmail
                : null;
        }

        private int? CurrentAvailableUsageResetCount()
        {
            return SelectAvailableUsageResetCount(
                settings.lastSuccessfulUsage,
                settings.loginRequired,
                settings.lastSuccessfulUsageSource,
                settings.usageSource);
        }

        private UserLevel CurrentUserLevel()
        {
            return SelectUserLevel(
                settings.lastSuccessfulUsage,
                settings.loginRequired,
                settings.lastSuccessfulUsageSource,
                settings.usageSource);
        }

        internal static UserLevel SelectUserLevel(
            UsageSnapshot snapshot,
            bool loginRequired,
            UsageSource lastSuccessfulSource,
            UsageSource selectedSource)
        {
            return snapshot != null && !loginRequired && lastSuccessfulSource == selectedSource
                ? snapshot.userLevel
                : UserLevel.Unknown;
        }

        internal static int? SelectAvailableUsageResetCount(
            UsageSnapshot snapshot,
            bool loginRequired,
            UsageSource lastSuccessfulSource,
            UsageSource selectedSource)
        {
            return snapshot != null && !loginRequired && lastSuccessfulSource == selectedSource
                ? snapshot.availableUsageResetCount
                : null;
        }

        private void ClearCurrentAccountEmail()
        {
            currentAccountEmail = null;
            currentAccountEmailSource = null;
        }

        private void UpdateRefreshTimer()
        {
            refreshTimer.Stop();
            if (!exiting && !interactiveActionPending && settings.CanRefreshUsage() &&
                !fetchService.IsBusy && (loginForm == null || loginForm.IsDisposed))
            {
                refreshTimer.Start();
            }
        }

        private void SaveSettingsSafely()
        {
            try
            {
                SettingsStore.Save(settings);
            }
            catch (Exception ex)
            {
                SafeLogger.Write("Settings.SaveFailed", ex.GetType().Name + ": " + ex.Message);
            }
        }

        private static void OpenFolder(string path)
        {
            try
            {
                System.IO.Directory.CreateDirectory(path);
                Process.Start("explorer.exe", "\"" + path + "\"");
            }
            catch (Exception ex)
            {
                SafeLogger.Write("Folder.OpenFailed", ex.GetType().Name + ": " + ex.Message);
            }
        }

        private void OpenLogFile()
        {
            try
            {
                AppPaths.EnsureBaseDirectories();
                SafeLogger.Write("Logs.OpenRequested");
                if (!System.IO.File.Exists(AppPaths.LogPath))
                {
                    using (System.IO.File.Create(AppPaths.LogPath))
                    {
                    }
                }
                Process.Start(CreateFileOpenStartInfo(AppPaths.LogPath));
            }
            catch (Exception ex)
            {
                SafeLogger.Write("File.OpenFailed", ex.GetType().Name + ": " + ex.Message);
                ShowMessage(ui.LogOpenFailed, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        internal static ProcessStartInfo CreateFileOpenStartInfo(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("A file path is required.", "path");
            return new ProcessStartInfo
            {
                FileName = System.IO.Path.GetFullPath(path),
                UseShellExecute = true
            };
        }

        private DialogResult ShowMessage(string message, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            return LocalizedMessageBox.Show(null, message, AppConstants.Name, buttons, icon, settings.themeMode, settings.language);
        }

        private async Task ExitApplicationAsync()
        {
            if (exiting) return;
            exiting = true;
            refreshTimer.Stop();
            startupTimer.Stop();
            applicationCancellation.Cancel();
            fetchService.Stop();
            if (activeRefreshTask != null && !activeRefreshTask.IsCompleted)
            {
                await Task.WhenAny(activeRefreshTask, Task.Delay(5000));
            }
            ExitThread();
        }

        private void ShowAbout()
        {
            using (var dialog = new AboutForm(settings.themeMode, settings.language))
            {
                dialog.ShowDialog();
            }
        }

        private void ShowHelp()
        {
            using (var dialog = new HelpForm(settings.themeMode, settings.language))
            {
                dialog.ShowDialog();
            }
        }
    }
}
