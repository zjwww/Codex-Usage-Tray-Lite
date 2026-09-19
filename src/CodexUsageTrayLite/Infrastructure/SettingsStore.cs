using System;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;
using CodexUsageTrayLite.Models;

namespace CodexUsageTrayLite.Infrastructure
{
    internal static class SettingsStore
    {
        internal const int CurrentSettingsSchemaVersion = 5;
        internal static readonly int[] AllowedRefreshIntervals = { 1, 2, 5, 10, 15, 30, 60 };

        public static AppSettings Load()
        {
            AppPaths.EnsureBaseDirectories();
            if (!File.Exists(AppPaths.SettingsPath))
            {
                return AppSettings.CreateDefault();
            }
            try
            {
                var serializer = new JavaScriptSerializer();
                var settings = serializer.Deserialize<AppSettings>(File.ReadAllText(AppPaths.SettingsPath, Encoding.UTF8));
                return Normalize(settings);
            }
            catch (Exception ex)
            {
                SafeLogger.Write("Settings.LoadFailed", ex.GetType().Name + ": " + ex.Message);
                return AppSettings.CreateDefault();
            }
        }

        public static void Save(AppSettings settings)
        {
            settings = Normalize(settings);
            AppPaths.EnsureBaseDirectories();
            var serializer = new JavaScriptSerializer();
            var json = serializer.Serialize(settings);
            var temp = AppPaths.SettingsPath + ".tmp";
            File.WriteAllText(temp, json, new UTF8Encoding(false));
            if (File.Exists(AppPaths.SettingsPath))
            {
                File.Replace(temp, AppPaths.SettingsPath, null);
            }
            else
            {
                File.Move(temp, AppPaths.SettingsPath);
            }
        }

        public static AppLanguage LoadLanguageForStartup()
        {
            if (!File.Exists(AppPaths.SettingsPath))
                return LanguageService.DetectInitialLanguage(System.Globalization.CultureInfo.InstalledUICulture);
            try
            {
                var serializer = new JavaScriptSerializer();
                var settings = serializer.Deserialize<AppSettings>(File.ReadAllText(AppPaths.SettingsPath, Encoding.UTF8));
                return settings != null && settings.settingsSchemaVersion >= 4
                    ? LanguageService.Normalize(settings.language)
                    : AppLanguage.English;
            }
            catch
            {
                return LanguageService.DetectInitialLanguage(System.Globalization.CultureInfo.InstalledUICulture);
            }
        }

        public static bool IsAllowedRefreshInterval(int minutes)
        {
            return Array.IndexOf(AllowedRefreshIntervals, minutes) >= 0;
        }

        internal static AppSettings Normalize(AppSettings settings)
        {
            if (settings == null)
            {
                return AppSettings.CreateDefault();
            }
            if (settings.settingsSchemaVersion < 1)
            {
                settings.setupComplete = settings.lastSuccessfulUsage != null && !settings.loginRequired;
                if (!settings.setupComplete) settings.loginRequired = true;
            }
            if (settings.settingsSchemaVersion < 2) settings.themeMode = AppThemeMode.System;
            if (settings.settingsSchemaVersion < 3)
            {
                settings.usageSource = UsageSource.WebView2;
                settings.lastSuccessfulUsageSource = UsageSource.WebView2;
            }
            if (settings.settingsSchemaVersion < 4) settings.language = AppLanguage.English;
            if (settings.settingsSchemaVersion < 5) settings.quotaIconMode = QuotaIconMode.Both;
            settings.settingsSchemaVersion = CurrentSettingsSchemaVersion;
            if (!IsAllowedRefreshInterval(settings.refreshIntervalMinutes))
            {
                settings.refreshIntervalMinutes = AppConstants.DefaultRefreshMinutes;
            }
            if (!Enum.IsDefined(typeof(ProxyMode), settings.proxyMode))
            {
                settings.proxyMode = ProxyMode.System;
            }
            if (!Enum.IsDefined(typeof(UsageSource), settings.usageSource))
            {
                settings.usageSource = UsageSource.WebView2;
            }
            if (!Enum.IsDefined(typeof(UsageSource), settings.lastSuccessfulUsageSource))
            {
                settings.lastSuccessfulUsageSource = UsageSource.WebView2;
            }
            if (string.IsNullOrWhiteSpace(settings.proxyHost))
            {
                settings.proxyHost = "127.0.0.1";
            }
            if (settings.proxyPort < 1 || settings.proxyPort > 65535)
            {
                settings.proxyPort = 7890;
            }
            if (string.IsNullOrWhiteSpace(settings.iconStyle))
            {
                settings.iconStyle = "B";
            }
            if (!Enum.IsDefined(typeof(QuotaIconMode), settings.quotaIconMode))
            {
                settings.quotaIconMode = QuotaIconMode.Both;
            }
            settings.themeMode = UI.ThemeService.Normalize(settings.themeMode);
            settings.language = LanguageService.Normalize(settings.language);
            if (settings.lastSuccessfulUsage != null)
            {
                if (!Enum.IsDefined(typeof(UserLevel), settings.lastSuccessfulUsage.userLevel))
                {
                    settings.lastSuccessfulUsage.userLevel = UserLevel.Unknown;
                }
                settings.lastSuccessfulUsage.fiveHourRemainingPercent = Percentage.ClampNullable(settings.lastSuccessfulUsage.fiveHourRemainingPercent);
                settings.lastSuccessfulUsage.weeklyRemainingPercent = Percentage.ClampNullable(settings.lastSuccessfulUsage.weeklyRemainingPercent);
                if (settings.lastSuccessfulUsage.fiveHourLimitApplies == false)
                {
                    settings.lastSuccessfulUsage.fiveHourRemainingPercent = null;
                    settings.lastSuccessfulUsage.fiveHourResetAt = null;
                }
                else if (settings.lastSuccessfulUsage.fiveHourRemainingPercent.HasValue)
                {
                    settings.lastSuccessfulUsage.fiveHourLimitApplies = true;
                }
                if (settings.lastSuccessfulUsage.availableUsageResetCount < 0)
                {
                    settings.lastSuccessfulUsage.availableUsageResetCount = null;
                }
            }
            return settings;
        }
    }

    internal static class Percentage
    {
        public static int Clamp(int value)
        {
            return Math.Max(0, Math.Min(100, value));
        }

        public static int FromUsed(int used)
        {
            return 100 - Clamp(used);
        }

        public static int? ClampNullable(int? value)
        {
            return value.HasValue ? (int?)Clamp(value.Value) : null;
        }
    }
}
