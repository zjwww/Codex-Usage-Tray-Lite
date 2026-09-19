using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using CodexUsageTrayLite.Models;

namespace CodexUsageTrayLite.Services
{
    internal static class UserLevelHelper
    {
        private static readonly Regex StandaloneBadge = new Regex(
            @"(?m)^\s*(FREE|GO|PLUS|PRO)\s*$",
            RegexOptions.CultureInvariant);

        public static UserLevel Parse(object rawValue)
        {
            var value = rawValue == null
                ? string.Empty
                : Convert.ToString(rawValue, CultureInfo.InvariantCulture) ?? string.Empty;
            var normalized = Regex.Replace(value.Trim().ToLowerInvariant(), @"[\s_-]+", string.Empty);
            switch (normalized)
            {
                case "free":
                case "chatgptfree":
                    return UserLevel.Free;
                case "go":
                case "chatgptgo":
                    return UserLevel.Go;
                case "plus":
                case "chatgptplus":
                    return UserLevel.Plus;
                case "pro":
                case "prolite":
                case "chatgptpro":
                    return UserLevel.Pro;
                default:
                    return UserLevel.Unknown;
            }
        }

        public static UserLevel ReadFromDictionary(IDictionary<string, object> values)
        {
            if (values == null) return UserLevel.Unknown;
            object raw;
            foreach (var key in new[] { "planType", "plan_type", "userLevel", "user_level" })
            {
                if (values.TryGetValue(key, out raw))
                {
                    var parsed = Parse(raw);
                    if (parsed != UserLevel.Unknown) return parsed;
                }
            }
            return UserLevel.Unknown;
        }

        public static UserLevel ReadFromVisiblePageText(string pageText)
        {
            if (string.IsNullOrWhiteSpace(pageText)) return UserLevel.Unknown;
            var lines = pageText.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            var inspected = 0;
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                inspected++;
                var match = StandaloneBadge.Match(line.Normalize(NormalizationForm.FormKC));
                if (match.Success) return Parse(match.Groups[1].Value);
                if (inspected >= 40) break;
            }
            return UserLevel.Unknown;
        }

        public static bool IsAbovePlus(UserLevel level)
        {
            return level == UserLevel.Pro;
        }

        public static string DisplayName(UserLevel level, string unknown)
        {
            switch (level)
            {
                case UserLevel.Free: return "Free";
                case UserLevel.Go: return "Go";
                case UserLevel.Plus: return "Plus";
                case UserLevel.Pro: return "Pro";
                default: return string.IsNullOrWhiteSpace(unknown) ? "Unknown" : unknown;
            }
        }
    }
}
