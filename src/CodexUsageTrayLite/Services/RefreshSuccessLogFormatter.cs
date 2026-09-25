using System;
using System.Globalization;
using CodexUsageTrayLite.Infrastructure;
using CodexUsageTrayLite.Models;

namespace CodexUsageTrayLite.Services
{
    internal static class RefreshSuccessLogFormatter
    {
        public static string Format(UsageSnapshot snapshot, UsageSource source, string accountEmail, TimeSpan duration)
        {
            if (snapshot == null) throw new ArgumentNullException("snapshot");
            var fiveHourApplies = TooltipFormatter.ShouldDisplayFiveHour(snapshot);
            return "source=" + SourceName(source) +
                " email=" + (AccountEmailPrivacy.MaskForDisplay(accountEmail) ?? "unavailable") +
                " userLevel=" + UserLevelHelper.DisplayName(snapshot.userLevel, "Unknown") +
                " fiveHourRemaining=" + (fiveHourApplies ? FormatPercent(snapshot.fiveHourRemainingPercent) : "N/A") +
                " fiveHourResetAt=" + (fiveHourApplies ? FormatTimestamp(snapshot.fiveHourResetAt) : "N/A") +
                " weeklyRemaining=" + FormatPercent(snapshot.weeklyRemainingPercent) +
                " weeklyResetAt=" + FormatTimestamp(snapshot.weeklyResetAt) +
                " usageResets=" + FormatNullableInt(snapshot.availableUsageResetCount) +
                " fetchedAt=" + FormatTimestamp(snapshot.lastSuccessfulFetchAt) +
                " durationMs=" + ((long)Math.Max(0, duration.TotalMilliseconds)).ToString(CultureInfo.InvariantCulture);
        }

        private static string SourceName(UsageSource source)
        {
            return source == UsageSource.CodexCli ? "CodexCli" : "WebView2";
        }

        private static string FormatPercent(int? value)
        {
            return value.HasValue ? value.Value.ToString(CultureInfo.InvariantCulture) + "%" : "Unknown";
        }

        private static string FormatNullableInt(int? value)
        {
            return value.HasValue ? value.Value.ToString(CultureInfo.InvariantCulture) : "Unknown";
        }

        private static string FormatTimestamp(DateTime value)
        {
            return value == default(DateTime) ? "Unknown" : FormatTimestamp((DateTime?)value);
        }

        private static string FormatTimestamp(DateTime? value)
        {
            if (!value.HasValue || value.Value == default(DateTime)) return "Unknown";
            var timestamp = value.Value.Kind == DateTimeKind.Utc ? value.Value.ToLocalTime() : value.Value;
            return timestamp.ToString("yyyy-MM-dd'T'HH:mm:sszzz", CultureInfo.InvariantCulture);
        }
    }
}
