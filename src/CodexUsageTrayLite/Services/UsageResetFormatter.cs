using System.Globalization;
using CodexUsageTrayLite.Models;

namespace CodexUsageTrayLite.Services
{
    internal static class UsageResetFormatter
    {
        public static string FormatMenu(int? availableCount)
        {
            return FormatMenu(availableCount, AppLanguage.English);
        }

        public static string FormatMenu(int? availableCount, AppLanguage language)
        {
            return UiText.For(language).FormatUsageResets(availableCount);
        }
    }
}
