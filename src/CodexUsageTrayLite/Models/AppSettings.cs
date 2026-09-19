using System.Globalization;
using CodexUsageTrayLite.Infrastructure;

namespace CodexUsageTrayLite.Models
{
    internal sealed class AppSettings
    {
        public int settingsSchemaVersion { get; set; }
        public bool setupComplete { get; set; }
        public UsageSource usageSource { get; set; }
        public UsageSource lastSuccessfulUsageSource { get; set; }
        public int refreshIntervalMinutes { get; set; }
        public ProxyMode proxyMode { get; set; }
        public string proxyHost { get; set; }
        public int proxyPort { get; set; }
        public string iconStyle { get; set; }
        public QuotaIconMode quotaIconMode { get; set; }
        public AppThemeMode themeMode { get; set; }
        public AppLanguage language { get; set; }
        public UsageSnapshot lastSuccessfulUsage { get; set; }
        public string lastSuccessfulFetchTime { get; set; }
        public bool loginRequired { get; set; }

        public ProxySettings GetProxySettings()
        {
            return new ProxySettings
            {
                mode = proxyMode,
                host = proxyHost,
                port = proxyPort
            };
        }

        public bool CanRefreshUsage()
        {
            return setupComplete && !loginRequired;
        }

        public bool CanManuallyRefreshUsage()
        {
            return CanRefreshUsage() || usageSource == UsageSource.CodexCli;
        }

        public static AppSettings CreateDefault()
        {
            return new AppSettings
            {
                settingsSchemaVersion = SettingsStore.CurrentSettingsSchemaVersion,
                setupComplete = false,
                usageSource = UsageSource.WebView2,
                lastSuccessfulUsageSource = UsageSource.WebView2,
                refreshIntervalMinutes = AppConstants.DefaultRefreshMinutes,
                proxyMode = ProxyMode.System,
                proxyHost = "127.0.0.1",
                proxyPort = 7890,
                iconStyle = "B",
                quotaIconMode = QuotaIconMode.Both,
                themeMode = AppThemeMode.System,
                language = LanguageService.DetectInitialLanguage(CultureInfo.InstalledUICulture),
                lastSuccessfulUsage = null,
                lastSuccessfulFetchTime = null,
                loginRequired = true
            };
        }
    }
}
