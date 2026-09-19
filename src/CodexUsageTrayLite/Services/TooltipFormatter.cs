using System;
using System.Globalization;
using CodexUsageTrayLite.Models;

namespace CodexUsageTrayLite.Services
{
    internal static class TooltipFormatter
    {
        public static string Build(
            UsageSnapshot snapshot,
            TrayVisualState state,
            bool cached,
            QuotaIconMode iconMode = QuotaIconMode.Both)
        {
            var text = BuildUsageText(snapshot, state, cached, iconMode);
            return text.Length <= AppConstants.NotifyIconTextLimit
                ? text
                : text.Substring(0, AppConstants.NotifyIconTextLimit);
        }

        internal static string BuildFiveHourMenuLine(UsageSnapshot snapshot)
        {
            if (!ShouldDisplayFiveHour(snapshot)) return "5-Hour N/A";
            return FormatFiveHour(
                snapshot == null ? null : snapshot.fiveHourRemainingPercent,
                snapshot == null ? null : snapshot.fiveHourResetAt);
        }

        internal static string BuildWeeklyMenuLine(UsageSnapshot snapshot, TrayVisualState state, bool cached)
        {
            return FormatWeekly(
                snapshot == null ? null : snapshot.weeklyRemainingPercent,
                snapshot == null ? null : snapshot.weeklyResetAt) + StatusSuffix(state, cached);
        }

        internal static bool ShouldDisplayFiveHour(UsageSnapshot snapshot)
        {
            if (snapshot == null) return true;
            if (UserLevelHelper.IsAbovePlus(snapshot.userLevel) && snapshot.KnownFiveHourLimitApplicability != true) return false;
            return snapshot.IsFiveHourLimitApplicable;
        }

        private static string BuildUsageText(UsageSnapshot snapshot, TrayVisualState state, bool cached, QuotaIconMode iconMode)
        {
            if (iconMode == QuotaIconMode.FiveHourOnly)
            {
                return BuildFiveHourOnlyText(snapshot) + StatusSuffix(state, cached);
            }
            var weeklyText = BuildWeeklyMenuLine(snapshot, state, cached);
            return ShouldDisplayFiveHour(snapshot)
                ? BuildFiveHourMenuLine(snapshot) + "\n" + weeklyText
                : weeklyText;
        }

        private static string BuildFiveHourOnlyText(UsageSnapshot snapshot)
        {
            if (snapshot != null &&
                (snapshot.fiveHourLimitApplies == false ||
                 (UserLevelHelper.IsAbovePlus(snapshot.userLevel) && snapshot.KnownFiveHourLimitApplicability != true)))
            {
                return "5-Hour N/A";
            }
            return FormatFiveHour(
                snapshot == null ? null : snapshot.fiveHourRemainingPercent,
                snapshot == null ? null : snapshot.fiveHourResetAt);
        }

        internal static string FormatReset(DateTime? value, bool includeDate)
        {
            if (!value.HasValue) return "--";
            var local = value.Value.Kind == DateTimeKind.Utc ? value.Value.ToLocalTime() : value.Value;
            return includeDate
                ? local.ToString("HH:mm M/d", CultureInfo.CurrentCulture)
                : local.ToString("HH:mm", CultureInfo.CurrentCulture);
        }

        private static string FormatPercent(int? value)
        {
            return value.HasValue ? value.Value.ToString(CultureInfo.InvariantCulture) + "%" : "--";
        }

        private static string FormatFiveHour(int? remaining, DateTime? reset)
        {
            return "5-Hour " + FormatPercent(remaining) + " (" + FormatReset(reset, false) + ")";
        }

        private static string FormatWeekly(int? remaining, DateTime? reset)
        {
            return "Weekly " + FormatPercent(remaining) + " (" + FormatReset(reset, true) + ")";
        }

        private static string StatusSuffix(TrayVisualState state, bool cached)
        {
            if (state == TrayVisualState.LoginRequired) return " [login]";
            if (state == TrayVisualState.FetchError) return " [error]";
            if (cached) return " [cached]";
            return string.Empty;
        }
    }
}
