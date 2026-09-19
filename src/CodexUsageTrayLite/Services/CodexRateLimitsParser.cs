using System;
using System.Collections.Generic;
using System.Globalization;
using CodexUsageTrayLite.Infrastructure;
using CodexUsageTrayLite.Models;

namespace CodexUsageTrayLite.Services
{
    internal static class CodexRateLimitsParser
    {
        internal const int FiveHourWindowMinutes = 300;
        internal const int WeeklyWindowMinutes = 10080;
        private static readonly DateTime UnixEpochUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static bool TryParse(IDictionary<string, object> result, DateTime fetchedAt, out UsageSnapshot snapshot, out string error)
        {
            UsageFieldDiagnostics diagnostics;
            return TryParse(result, fetchedAt, out snapshot, out error, out diagnostics);
        }

        internal static bool TryParse(
            IDictionary<string, object> result,
            DateTime fetchedAt,
            out UsageSnapshot snapshot,
            out string error,
            out UsageFieldDiagnostics diagnostics)
        {
            snapshot = null;
            error = null;
            diagnostics = new UsageFieldDiagnostics();
            UsageFieldUnavailableReason resetReason;
            var resetCount = ParseAvailableUsageResetCount(result, out resetReason);
            if (!resetCount.HasValue)
            {
                diagnostics.MarkUnavailable(UsageDataField.Resets, resetReason);
            }
            var bucket = SelectGeneralBucket(result);
            if (bucket == null)
            {
                diagnostics.MarkUnavailable(UsageDataField.Weekly, UsageFieldUnavailableReason.RateLimitBucketMissing);
                error = "Codex CLI did not return the general Codex rate-limit bucket.";
                return false;
            }

            RateWindow fiveHour = null;
            RateWindow weekly = null;
            foreach (var key in new[] { "primary", "secondary" })
            {
                object raw;
                if (!bucket.TryGetValue(key, out raw) || raw == null) continue;
                RateWindow window;
                var rawWindow = AsDictionary(raw);
                if (!TryParseWindow(rawWindow, out window))
                {
                    diagnostics.MarkUnavailable(
                        ClassifyWindowField(rawWindow),
                        UsageFieldUnavailableReason.RateLimitWindowInvalid);
                    error = "Codex CLI returned an invalid rate-limit window.";
                    return false;
                }
                if (window.DurationMinutes == FiveHourWindowMinutes) fiveHour = window;
                else if (window.DurationMinutes == WeeklyWindowMinutes) weekly = window;
            }

            if (weekly == null)
            {
                diagnostics.MarkUnavailable(UsageDataField.Weekly, UsageFieldUnavailableReason.RateLimitWindowMissing);
                error = "Codex CLI did not return the weekly rate-limit window.";
                return false;
            }

            snapshot = new UsageSnapshot
            {
                userLevel = UserLevelHelper.ReadFromDictionary(bucket),
                fiveHourLimitApplies = fiveHour != null,
                fiveHourRemainingPercent = fiveHour == null
                    ? (int?)null
                    : Percentage.FromUsed((int)Math.Round(fiveHour.UsedPercent, MidpointRounding.AwayFromZero)),
                weeklyRemainingPercent = Percentage.FromUsed((int)Math.Round(weekly.UsedPercent, MidpointRounding.AwayFromZero)),
                fiveHourResetAt = fiveHour == null ? (DateTime?)null : fiveHour.ResetAtUtc,
                weeklyResetAt = weekly.ResetAtUtc,
                availableUsageResetCount = resetCount,
                lastSuccessfulFetchAt = fetchedAt
            };
            return true;
        }

        internal static int? ParseAvailableUsageResetCount(IDictionary<string, object> result)
        {
            UsageFieldUnavailableReason reason;
            return ParseAvailableUsageResetCount(result, out reason);
        }

        internal static int? ParseAvailableUsageResetCount(
            IDictionary<string, object> result,
            out UsageFieldUnavailableReason unavailableReason)
        {
            unavailableReason = UsageFieldUnavailableReason.None;
            if (result == null)
            {
                unavailableReason = UsageFieldUnavailableReason.ResponseFieldMissing;
                return null;
            }
            object rawSummary;
            if (!result.TryGetValue("rateLimitResetCredits", out rawSummary) || rawSummary == null)
            {
                unavailableReason = UsageFieldUnavailableReason.ResponseFieldMissing;
                return null;
            }
            var summary = AsDictionary(rawSummary);
            if (summary == null)
            {
                unavailableReason = UsageFieldUnavailableReason.ResponseFieldInvalid;
                return null;
            }
            object rawCount;
            if (!summary.TryGetValue("availableCount", out rawCount) || rawCount == null)
            {
                unavailableReason = UsageFieldUnavailableReason.ResponseFieldMissing;
                return null;
            }
            try
            {
                var count = Convert.ToDecimal(rawCount, CultureInfo.InvariantCulture);
                if (count < 0 || count > int.MaxValue || count != decimal.Truncate(count))
                {
                    unavailableReason = UsageFieldUnavailableReason.ResponseFieldInvalid;
                    return null;
                }
                return decimal.ToInt32(count);
            }
            catch (Exception ex) when (ex is FormatException || ex is InvalidCastException || ex is OverflowException)
            {
                unavailableReason = UsageFieldUnavailableReason.ResponseFieldInvalid;
                return null;
            }
        }

        private static UsageDataField ClassifyWindowField(IDictionary<string, object> value)
        {
            if (value == null) return UsageDataField.Weekly;
            object rawDuration;
            if (!value.TryGetValue("windowDurationMins", out rawDuration) || rawDuration == null)
            {
                return UsageDataField.Weekly;
            }
            try
            {
                return Convert.ToInt32(rawDuration, CultureInfo.InvariantCulture) == FiveHourWindowMinutes
                    ? UsageDataField.FiveHour
                    : UsageDataField.Weekly;
            }
            catch (Exception ex) when (ex is FormatException || ex is InvalidCastException || ex is OverflowException)
            {
                return UsageDataField.Weekly;
            }
        }

        private static IDictionary<string, object> SelectGeneralBucket(IDictionary<string, object> result)
        {
            if (result == null) return null;
            object raw;
            var backwardCompatible = result.TryGetValue("rateLimits", out raw) ? AsDictionary(raw) : null;
            if (backwardCompatible != null) return backwardCompatible;

            var buckets = result.TryGetValue("rateLimitsByLimitId", out raw) ? AsDictionary(raw) : null;
            if (buckets == null) return null;
            object general;
            return buckets.TryGetValue("codex", out general) ? AsDictionary(general) : null;
        }

        private static bool TryParseWindow(IDictionary<string, object> value, out RateWindow window)
        {
            window = null;
            if (value == null) return false;
            object rawUsed;
            object rawDuration;
            if (!value.TryGetValue("usedPercent", out rawUsed) || !value.TryGetValue("windowDurationMins", out rawDuration)) return false;
            double used;
            int duration;
            try
            {
                used = Convert.ToDouble(rawUsed, CultureInfo.InvariantCulture);
                duration = Convert.ToInt32(rawDuration, CultureInfo.InvariantCulture);
            }
            catch (Exception ex) when (ex is FormatException || ex is InvalidCastException || ex is OverflowException)
            {
                return false;
            }
            if (double.IsNaN(used) || double.IsInfinity(used) || used < 0 || used > 100 || duration <= 0) return false;

            DateTime? reset = null;
            object rawReset;
            if (value.TryGetValue("resetsAt", out rawReset) && rawReset != null)
            {
                try
                {
                    var seconds = Convert.ToInt64(rawReset, CultureInfo.InvariantCulture);
                    if (seconds > 0) reset = UnixEpochUtc.AddSeconds(seconds);
                }
                catch (Exception ex) when (ex is FormatException || ex is InvalidCastException || ex is OverflowException || ex is ArgumentOutOfRangeException)
                {
                    return false;
                }
            }
            window = new RateWindow { UsedPercent = used, DurationMinutes = duration, ResetAtUtc = reset };
            return true;
        }

        internal static IDictionary<string, object> AsDictionary(object value)
        {
            return value as IDictionary<string, object>;
        }

        private sealed class RateWindow
        {
            public double UsedPercent { get; set; }
            public int DurationMinutes { get; set; }
            public DateTime? ResetAtUtc { get; set; }
        }
    }
}
