using System;
using System.Globalization;
using System.Resources;
using CodexUsageTrayLite.Infrastructure;
using CodexUsageTrayLite.Models;
using CodexUsageTrayLite.Services;

namespace CodexUsageTrayLite
{
    internal sealed class UiText
    {
        internal const string ResourceBaseName = "CodexUsageTrayLite.Resources.Strings";
        private static readonly ResourceManager Resources = new ResourceManager(ResourceBaseName, typeof(UiText).Assembly);
        private readonly AppLanguage language;
        private readonly CultureInfo culture;

        private UiText(AppLanguage language)
        {
            this.language = LanguageService.Normalize(language);
            culture = LanguageService.GetCulture(this.language);
        }

        public static UiText For(AppLanguage language)
        {
            return new UiText(language);
        }

        public AppLanguage Language { get { return language; } }
        internal CultureInfo Culture { get { return culture; } }

        private string Get(string key)
        {
            var value = Resources.GetString(key, culture);
            if (value == null) throw new MissingManifestResourceException("Missing UI resource: " + key);
            return value;
        }

        private string Format(string key, params object[] values)
        {
            return string.Format(culture, Get(key), values);
        }

        public string RefreshNow { get { return Get("RefreshNow"); } }
        public string OpenLogin { get { return Get("OpenLogin"); } }
        public string CodexCliLoginHelp { get { return Get("CodexCliLoginHelp"); } }
        public string UsageSourceMenu { get { return Get("UsageSourceMenu"); } }
        public string UsageSourceWebView2 { get { return Get("UsageSourceWebView2"); } }
        public string UsageSourceCodexCli { get { return Get("UsageSourceCodexCli"); } }
        public string RefreshInterval { get { return Get("RefreshInterval"); } }
        public string IconStyleMenu { get { return Get("IconStyleMenu"); } }
        public string IconStyleFiveHourOnly { get { return Get("IconStyleFiveHourOnly"); } }
        public string IconStyleWeeklyOnly { get { return Get("IconStyleWeeklyOnly"); } }
        public string IconStyleBoth { get { return Get("IconStyleBoth"); } }
        public string IconStyleSideBySide { get { return Get("IconStyleSideBySide"); } }
        public string StartAtLogin { get { return Get("StartAtLogin"); } }
        public string ProxySettings { get { return Get("ProxySettings"); } }
        public string Theme { get { return Get("Theme"); } }
        public string ThemeFollowWindows { get { return Get("ThemeFollowWindows"); } }
        public string ThemeLight { get { return Get("ThemeLight"); } }
        public string ThemeDark { get { return Get("ThemeDark"); } }
        public string LanguageMenu { get { return Get("LanguageMenu"); } }
        public string LanguageEnglish { get { return Get("LanguageEnglish"); } }
        public string LanguageChineseSimplified { get { return Get("LanguageChineseSimplified"); } }
        public string LanguageChineseTraditional { get { return Get("LanguageChineseTraditional"); } }
        public string LanguageJapanese { get { return Get("LanguageJapanese"); } }
        public string LanguageKorean { get { return Get("LanguageKorean"); } }
        public string OpenLogs { get { return Get("OpenLogs"); } }
        public string OpenConfig { get { return Get("OpenConfig"); } }
        public string ClearSession { get { return Get("ClearSession"); } }
        public string Help { get { return Get("Help"); } }
        public string About { get { return Get("About"); } }
        public string Exit { get { return Get("Exit"); } }
        public string Unknown { get { return Get("Unknown"); } }
        public string Never { get { return Get("Never"); } }

        public string FormatMinutes(int minutes)
        {
            return Format(minutes == 1 ? "MinutesSingularFormat" : "MinutesPluralFormat", minutes);
        }

        public string FormatAccountEmail(string email)
        {
            return Format("AccountEmailFormat", AccountEmailPrivacy.Normalize(email) ?? "--");
        }

        public string FormatUserLevel(UserLevel level)
        {
            return Format("UserLevelFormat", UserLevelHelper.DisplayName(level, Unknown));
        }

        public string FormatUsageResets(int? count)
        {
            return count.HasValue
                ? Format("UsageResetsCountFormat", count.Value)
                : Format("UsageResetsUnknownFormat", Unknown);
        }

        public string FormatLastUpdated(DateTime? time, UsageSource source)
        {
            return time.HasValue
                ? Format("LastUpdatedFormat", time.Value.ToLocalTime().ToString("HH:mm"), UsageSourceName(source))
                : Format("LastUpdatedNeverFormat", Never);
        }

        public string UsageSourceName(UsageSource source)
        {
            return source == UsageSource.CodexCli ? UsageSourceCodexCli : UsageSourceWebView2;
        }

        public string ProxyDialogTitle { get { return Get("ProxyDialogTitle"); } }
        public string ProxyModeLabel { get { return Get("ProxyModeLabel"); } }
        public string ProxyHostLabel { get { return Get("ProxyHostLabel"); } }
        public string ProxyPortLabel { get { return Get("ProxyPortLabel"); } }
        public string ProxySystem { get { return Get("ProxySystem"); } }
        public string ProxyDirect { get { return Get("ProxyDirect"); } }
        public string ProxyHttp { get { return Get("ProxyHttp"); } }
        public string ProxySocks5 { get { return Get("ProxySocks5"); } }
        public string ProxySystemDescription { get { return Get("ProxySystemDescription"); } }
        public string ProxyDirectDescription { get { return Get("ProxyDirectDescription"); } }
        public string ProxyHttpDescription { get { return Get("ProxyHttpDescription"); } }
        public string ProxySocks5Description { get { return Get("ProxySocks5Description"); } }
        public string Save { get { return Get("Save"); } }
        public string Cancel { get { return Get("Cancel"); } }
        public string Ok { get { return Get("Ok"); } }
        public string Yes { get { return Get("Yes"); } }
        public string No { get { return Get("No"); } }
        public string InvalidProxySettings { get { return Get("InvalidProxySettings"); } }

        public string LocalizeProxyValidation(Exception exception)
        {
            if (language == AppLanguage.English) return exception == null ? InvalidProxySettings : exception.Message;
            if (exception is ArgumentOutOfRangeException) return Get("ProxyValidationPort");
            if (exception is ArgumentException) return Get("ProxyValidationHost");
            return InvalidProxySettings;
        }

        public string HelpDialogTitle { get { return Format("HelpDialogTitleFormat", AppConstants.Name); } }
        public string HelpHeading { get { return Get("HelpHeading"); } }
        public string HelpBody { get { return Get("HelpBody"); } }
        public string AboutDialogTitle { get { return Format("AboutDialogTitleFormat", AppConstants.Name); } }

        public string BuildAboutDetails(AboutInfo info)
        {
            return string.Join(Environment.NewLine, new[]
            {
                Format("AboutVersionFormat", info.Version),
                Format("AboutArchitectureFormat", info.Architecture),
                Format("AboutTargetFrameworkFormat", info.TargetFramework),
                Format("AboutBuildTimeFormat", info.BuildTimeUtc),
                Format("AboutWebView2SdkFormat", info.WebView2SdkVersion),
                Format("AboutWebView2RuntimeFormat", info.WebView2RuntimeVersion),
                Format("AboutBaselineFormat", info.Baseline),
                Format("AboutLicenseFormat", info.License)
            });
        }

        public string LoginDialogTitle { get { return Format("LoginDialogTitleFormat", AppConstants.Name); } }
        public string LoginOpening { get { return Get("LoginOpening"); } }
        public string LoginRuntimeStatus { get { return Get("LoginRuntimeStatus"); } }
        public string LoginTimeoutStatus { get { return Get("LoginTimeoutStatus"); } }
        public string LoginInitializeFailedStatus { get { return Get("LoginInitializeFailedStatus"); } }
        public string LoginPageLoadFailed { get { return Get("LoginPageLoadFailed"); } }
        public string LoginUsageReached { get { return Get("LoginUsageReached"); } }
        public string LoginCompleteSignIn { get { return Get("LoginCompleteSignIn"); } }
        public string AlreadyRunning { get { return Format("AlreadyRunningFormat", AppConstants.Name); } }
        public string FatalStart(string error, string logPath) { return Format("FatalStartFormat", AppConstants.Name, error, logPath); }
        public string LoginOpenFailed { get { return Get("LoginOpenFailed"); } }
        public string CodexCliHelpMessage { get { return Get("CodexCliHelpMessage"); } }
        public string CloseLoginBeforeProxy { get { return Get("CloseLoginBeforeProxy"); } }
        public string RefreshStillStopping { get { return Get("RefreshStillStopping"); } }
        public string StartupChangeFailed { get { return Get("StartupChangeFailed"); } }
        public string ClearSessionPrompt { get { return Get("ClearSessionPrompt"); } }
        public string ClearSessionFailed { get { return Get("ClearSessionFailed"); } }
        public string LogOpenFailed { get { return Get("LogOpenFailed"); } }
        public string WebViewRuntimeRequired { get { return Get("WebViewRuntimeRequired"); } }
        public string WebViewTimeout { get { return Get("WebViewTimeout"); } }
        public string WebViewInitializeFailed(string error) { return Format("WebViewInitializeFailedFormat", error); }
    }

    internal static class LanguageService
    {
        public static AppLanguage Normalize(AppLanguage language)
        {
            return Enum.IsDefined(typeof(AppLanguage), language) ? language : AppLanguage.English;
        }

        public static CultureInfo GetCulture(AppLanguage language)
        {
            switch (Normalize(language))
            {
                case AppLanguage.ChineseSimplified: return CultureInfo.GetCultureInfo("zh-CN");
                case AppLanguage.ChineseTraditional: return CultureInfo.GetCultureInfo("zh-TW");
                case AppLanguage.Japanese: return CultureInfo.GetCultureInfo("ja-JP");
                case AppLanguage.Korean: return CultureInfo.GetCultureInfo("ko-KR");
                default: return CultureInfo.GetCultureInfo("en-US");
            }
        }

        public static AppLanguage DetectInitialLanguage(CultureInfo culture)
        {
            var name = culture == null ? string.Empty : culture.Name ?? string.Empty;
            if (name.StartsWith("ja", StringComparison.OrdinalIgnoreCase)) return AppLanguage.Japanese;
            if (name.StartsWith("ko", StringComparison.OrdinalIgnoreCase)) return AppLanguage.Korean;
            if (!name.StartsWith("zh", StringComparison.OrdinalIgnoreCase)) return AppLanguage.English;
            if (name.IndexOf("Hant", StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.EndsWith("-TW", StringComparison.OrdinalIgnoreCase) ||
                name.EndsWith("-HK", StringComparison.OrdinalIgnoreCase) ||
                name.EndsWith("-MO", StringComparison.OrdinalIgnoreCase))
                return AppLanguage.ChineseTraditional;
            return AppLanguage.ChineseSimplified;
        }
    }
}
