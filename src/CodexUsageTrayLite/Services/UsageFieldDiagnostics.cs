using System;
using System.Collections.Generic;
using CodexUsageTrayLite.Infrastructure;
using CodexUsageTrayLite.Models;

namespace CodexUsageTrayLite.Services
{
    internal enum UsageDataField
    {
        FiveHour,
        Weekly,
        Resets
    }

    internal enum UsageFieldUnavailableReason
    {
        None,
        PageTextUnreadable,
        PercentageUnreadable,
        RateLimitBucketMissing,
        RateLimitWindowMissing,
        RateLimitWindowInvalid,
        ResponseFieldMissing,
        ResponseFieldInvalid,
        DomProbeFailed,
        ResetSectionMissing,
        ResetSectionUnrecognized
    }

    internal sealed class UsageFieldDiagnostics
    {
        private readonly Dictionary<UsageDataField, UsageFieldUnavailableReason> unavailable =
            new Dictionary<UsageDataField, UsageFieldUnavailableReason>();

        public bool HasUnavailableFields
        {
            get { return unavailable.Count > 0; }
        }

        public void MarkUnavailable(UsageDataField field, UsageFieldUnavailableReason reason)
        {
            if (reason == UsageFieldUnavailableReason.None || unavailable.ContainsKey(field)) return;
            unavailable.Add(field, reason);
        }

        public bool TryGetReason(UsageDataField field, out UsageFieldUnavailableReason reason)
        {
            return unavailable.TryGetValue(field, out reason);
        }
    }

    internal static class UsageFieldTelemetry
    {
        internal const string EventName = "UsageField.Unavailable";

        public static void WriteAll(UsageSource source, UsageFieldDiagnostics diagnostics)
        {
            if (diagnostics == null) return;
            foreach (var field in new[] { UsageDataField.FiveHour, UsageDataField.Weekly, UsageDataField.Resets })
            {
                UsageFieldUnavailableReason reason;
                if (diagnostics.TryGetReason(field, out reason)) Write(source, field, reason);
            }
        }

        public static void Write(UsageSource source, UsageDataField field, UsageFieldUnavailableReason reason)
        {
            if (reason == UsageFieldUnavailableReason.None) return;
            SafeLogger.Write(EventName, FormatDetails(source, field, reason));
        }

        internal static string FormatDetails(UsageSource source, UsageDataField field, UsageFieldUnavailableReason reason)
        {
            return "source=" + FormatSource(source) +
                " field=" + FormatField(field) +
                " reason=" + FormatReason(reason);
        }

        private static string FormatSource(UsageSource source)
        {
            return source == UsageSource.CodexCli ? "CodexCli" : "WebView2";
        }

        private static string FormatField(UsageDataField field)
        {
            switch (field)
            {
                case UsageDataField.FiveHour:
                    return "five-hour";
                case UsageDataField.Resets:
                    return "resets";
                default:
                    return "weekly";
            }
        }

        private static string FormatReason(UsageFieldUnavailableReason reason)
        {
            switch (reason)
            {
                case UsageFieldUnavailableReason.PageTextUnreadable:
                    return "page-text-unreadable";
                case UsageFieldUnavailableReason.PercentageUnreadable:
                    return "percentage-unreadable";
                case UsageFieldUnavailableReason.RateLimitBucketMissing:
                    return "rate-limit-bucket-missing";
                case UsageFieldUnavailableReason.RateLimitWindowMissing:
                    return "rate-limit-window-missing";
                case UsageFieldUnavailableReason.RateLimitWindowInvalid:
                    return "rate-limit-window-invalid";
                case UsageFieldUnavailableReason.ResponseFieldMissing:
                    return "response-field-missing";
                case UsageFieldUnavailableReason.ResponseFieldInvalid:
                    return "response-field-invalid";
                case UsageFieldUnavailableReason.DomProbeFailed:
                    return "dom-probe-failed";
                case UsageFieldUnavailableReason.ResetSectionMissing:
                    return "reset-section-missing";
                case UsageFieldUnavailableReason.ResetSectionUnrecognized:
                    return "reset-section-unrecognized";
                default:
                    return "unknown";
            }
        }
    }
}
