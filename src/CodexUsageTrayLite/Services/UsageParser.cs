using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using CodexUsageTrayLite.Infrastructure;
using CodexUsageTrayLite.Models;

namespace CodexUsageTrayLite.Services
{
    internal static class UsageParser
    {
        private static readonly Regex FiveHourLabel = new Regex(
            @"(?:5\s*(?:小[时時])\s*(?:(?:使用|用量)\s*)?(?:限[额額]|限制|上限)|5\s*시간\s*(?:(?:사용량|사용)\s*)?(?:한도|제한)|5\s*時間(?:の)?\s*(?:(?:使用量|利用|使用)\s*)?(?:制限|上限)|5\s*[-]?\s*(?:hour|hr)[\s-]*(?:usage\s*)?limit|five\s*hour\s*(?:usage\s*)?limit)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static readonly Regex WeeklyLabel = new Regex(
            @"(?:(?:每\s*[周週]|[周週])\s*(?:(?:使用|用量)\s*)?(?:限[额額]|限制|上限)|주간\s*(?:(?:사용량|사용)\s*)?(?:한도|제한)|(?:週間|週ごとの)\s*(?:(?:使用量|利用|使用)\s*)?(?:制限|上限)|weekly\s*(?:usage\s*)?limit|week\s*(?:usage\s*)?limit)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static readonly Regex SparkLabel = new Regex(
            @"(?:GPT\s*[-\s.]*5\.?3\s*[-\s]*Codex\s*[-\s]*Spark|Codex\s*[-\s]*Spark|\bSpark\b)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static readonly Regex Percent = new Regex(@"(?<value>\d{1,3})\s*%", RegexOptions.CultureInvariant);
        private static readonly Regex ResetLine = new Regex(
            @"(?im)^.*(?:reset(?:s|ting)?|重置|重设|重設|초기화|재설정|リセット).*$",
            RegexOptions.CultureInvariant);
        private static readonly Regex ExplicitAvailableUsageResetCount = new Regex(
            @"(?<!\d)(?:" +
            @"(?<before>\d{1,9})\s+(?:banked\s+|usage\s+|codex\s+)?resets?\s+(?:available|remaining)\b|" +
            @"(?<after>\d{1,9})\s+(?:available|remaining)\s+(?:banked\s+|usage\s+|codex\s+)?resets?\b|" +
            @"(?:有|尚有|共有)?\s*(?<chinese>\d{1,9})\s*(?:个|個|次|条|條)?\s*(?:可用(?:的)?|剩余(?:的)?|剩餘(?:的)?|可使用(?:的)?)\s*(?:(?:用量|使用量|使用)(?:限制|上限|限[额額])?\s*)?(?:重置|重设|重設)|" +
            @"(?:可用|剩余|剩餘)\s*(?:(?:用量|使用量|使用)(?:限制|上限|限[额額])?\s*)?(?:重置|重设|重設)(?:数量|數量)?\s*[:：]?\s*(?<chineseAfter>\d{1,9})|" +
            @"(?<korean>\d{1,9})\s*회?\s*(?:사용\s*가능(?:한)?|남은)\s*(?:재설정|초기화)|" +
            @"(?:재설정|초기화)\s*(?<koreanAfter>\d{1,9})\s*회?\s*사용\s*가능|" +
            @"(?<japanese>\d{1,9})\s*(?:件|回)?\s*(?:利用|使用)可能(?:な)?\s*(?:使用量|利用)?(?:制限|上限)?(?:の)?リセット|" +
            @"(?:利用|使用)可能(?:な)?\s*(?:使用量|利用)?(?:制限|上限)?(?:の)?リセット(?:数)?\s*[:：]?\s*(?<japaneseAfter>\d{1,9}))",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static readonly Regex UsageResetSectionHeading = new Regex(
            @"(?im)^[ \t]*(?:Usage[ \t]+limit[ \t]+resets|(?:用量|使用量|使用)(?:限制|上限|限[额額])?[ \t]*(?:重置|重设|重設)|(?:使用|用量)[ \t]*限[额額][ \t]*(?:重置|重设|重設)|(?:사용량|사용)[ \t]*(?:한도|제한)[ \t]*(?:재설정|초기화)|(?:使用量|利用|使用)?(?:制限|上限)(?:の)?[ \t]*リセット)[ \t]*\r?$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static readonly Regex UsageResetSectionBoundary = new Regex(
            @"(?im)^[ \t]*(?:Auto[ \t-]*reload|Usage[ \t]+breakdown|自动(?:充值|重新加载|加值)|自動(?:加值|儲值|重新載入)|用量(?:明细|明細|分析)|使用量?(?:明细|明細|分析|详情|詳情)|자동[ \t]*(?:충전|새로[ \t]*고침)|사용량[ \t]*(?:분석|내역|세부[ \t]*정보)|自動(?:リロード|再読み込み|チャージ)|使用(?:量|状況)(?:の)?(?:内訳|詳細))[ \t]*\r?$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static readonly Regex UseResetAction = new Regex(
            @"(?im)^[ \t]*(?:Use[ \t]+reset|(?:使用(?:此)?|应用|應用|套用)(?:重置|重设|重設)|(?:재설정|초기화)[ \t]*사용|사용[ \t]*(?:재설정|초기화)|사용)[ \t]*\r?$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static readonly Regex ExplicitNoUsageResets = new Regex(
            @"(?im)^[ \t]*(?:(?:No|0)[ \t]+(?:(?:usage[ \t]+limit|banked|usage|codex)[ \t]+)?resets?[ \t]+(?:available|remaining)(?:[ \t]+at[ \t]+this[ \t]+time)?[.!]?|" +
            @"(?:目前|当前|當前|现在|現在)?[ \t]*(?:没有|沒有|无|無)[ \t]*(?:任何)?[ \t]*(?:可用的?)?[ \t]*(?:(?:用量|使用量|使用)(?:限制|上限|限[额額])?[ \t]*)?(?:重置|重设|重設)(?:可用)?[。.!！]?|" +
            @"현재[ \t]*(?:사용[ \t]*가능한[ \t]*)?(?:(?:사용량|사용)[ \t]*(?:한도|제한)[ \t]*)?(?:재설정|초기화)(?:이|가)?[ \t]*없습니다[.!]?|" +
            @"(?:現在|現時)?[ \t]*(?:利用|使用)可能な?[ \t]*(?:(?:使用量|利用)(?:制限|上限)(?:の)?[ \t]*)?リセット(?:は|が)?[ \t]*(?:ありません|ない)[。.!！]?)[ \t]*\r?$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static readonly Regex AvailableUsageResetTabCount = new Regex(
            @"(?im)^[ \t]*(?:Available|可用|사용[ \t]*가능|(?:利用|使用)可能)[ \t]*[:：]?[ \t]*(?:\r?\n[ \t]*)?[\(\[]?(?<count>\d{1,9})[\)\]]?[ \t]*\r?$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static readonly Regex RemainingDirection = new Regex(
            @"\b(?:remaining|left)\b|剩余|剩餘|尚余|尚餘|可用|남음|남은|잔여|残り|残量",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static readonly Regex UsedDirection = new Regex(
            @"\b(?:used|consumed)\b|已使用|已用|已消耗|已耗用|用了|사용됨|사용한|소진|使用済み|消費済み",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static readonly Regex ExplicitNoFiveHourLimit = new Regex(
            @"(?im)^[ \t]*(?:unlimited|no[ \t]+limit|not[ \t]+applicable|n/?a|无限|無限|无上限|無上限|不受限制|不限|무제한|제한[ \t]*없음|無制限|制限なし)[ \t]*[.!。！]?[ \t]*\r?$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static readonly Regex ResetDescription = new Regex(
            @"\buse\s+(?:a\s+)?reset\b|(?:使用|应用|應用|套用)(?:此|一次)?(?:重置|重设|重設).*(?:恢复|恢復|還原)|(?:재설정|초기화).*(?:복원|회복)|リセット(?:を)?(?:使用|使).*(?:復元|回復)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        public static bool TryParse(string pageText, DateTime now, out UsageSnapshot snapshot, out string error)
        {
            UsageFieldDiagnostics diagnostics;
            return TryParse(pageText, now, out snapshot, out error, out diagnostics);
        }

        internal static bool TryParse(
            string pageText,
            DateTime now,
            out UsageSnapshot snapshot,
            out string error,
            out UsageFieldDiagnostics diagnostics)
        {
            snapshot = null;
            error = null;
            diagnostics = new UsageFieldDiagnostics();
            if (string.IsNullOrWhiteSpace(pageText))
            {
                diagnostics.MarkUnavailable(UsageDataField.Weekly, UsageFieldUnavailableReason.PageTextUnreadable);
                diagnostics.MarkUnavailable(UsageDataField.Resets, UsageFieldUnavailableReason.PageTextUnreadable);
                error = "The usage page did not contain readable text.";
                return false;
            }

            pageText = NormalizePageText(pageText);

            int? fiveHour = null;
            int? weekly = null;
            DateTime? fiveReset = null;
            DateTime? weeklyReset = null;
            var sawFiveHourLabel = false;
            var explicitlyNoFiveHourLimit = false;
            var labels = FindLabels(pageText);
            foreach (var label in labels)
            {
                if (label.IsSpark)
                {
                    continue;
                }
                var segment = SegmentAfter(pageText, labels, label);
                if (label.IsFiveHour)
                {
                    sawFiveHourLabel = true;
                    if (ExplicitNoFiveHourLimit.IsMatch(segment))
                    {
                        explicitlyNoFiveHourLimit = true;
                        continue;
                    }
                }
                var parsed = ParsePercent(segment);
                if (!parsed.HasValue)
                {
                    continue;
                }
                if (label.IsFiveHour && !fiveHour.HasValue)
                {
                    fiveHour = parsed.Value;
                    fiveReset = ParseReset(segment, now);
                }
                else if (!label.IsFiveHour && !weekly.HasValue)
                {
                    weekly = parsed.Value;
                    weeklyReset = ParseReset(segment, now);
                }
            }

            if (!weekly.HasValue)
            {
                diagnostics.MarkUnavailable(UsageDataField.Weekly, UsageFieldUnavailableReason.PercentageUnreadable);
            }
            if (sawFiveHourLabel && !fiveHour.HasValue && !explicitlyNoFiveHourLimit)
            {
                diagnostics.MarkUnavailable(UsageDataField.FiveHour, UsageFieldUnavailableReason.PercentageUnreadable);
            }
            var usageResetCount = ParseAvailableUsageResetCount(pageText);
            if (!usageResetCount.HasValue)
            {
                diagnostics.MarkUnavailable(
                    UsageDataField.Resets,
                    ContainsRecognizedUsageResetSection(pageText)
                        ? UsageFieldUnavailableReason.ResetSectionUnrecognized
                        : UsageFieldUnavailableReason.ResetSectionMissing);
            }
            if (!weekly.HasValue)
            {
                error = "Could not parse the weekly usage percentage.";
                return false;
            }
            if (sawFiveHourLabel && !fiveHour.HasValue && !explicitlyNoFiveHourLimit)
            {
                error = "Could not parse the 5-hour usage percentage.";
                return false;
            }

            snapshot = new UsageSnapshot
            {
                fiveHourLimitApplies = fiveHour.HasValue,
                fiveHourRemainingPercent = fiveHour,
                weeklyRemainingPercent = weekly,
                fiveHourResetAt = fiveReset,
                weeklyResetAt = weeklyReset,
                availableUsageResetCount = usageResetCount,
                lastSuccessfulFetchAt = now,
                fiveHourLimitAbsenceInferred = !sawFiveHourLabel
            };
            return true;
        }

        internal static int? ParseAvailableUsageResetCount(string pageText)
        {
            if (string.IsNullOrWhiteSpace(pageText)) return null;
            pageText = NormalizePageText(pageText);
            var explicitCount = ExplicitAvailableUsageResetCount.Match(pageText);
            if (explicitCount.Success)
            {
                var value = FirstSuccessfulGroup(
                    explicitCount,
                    "before",
                    "after",
                    "chinese",
                    "chineseAfter",
                    "korean",
                    "koreanAfter",
                    "japanese",
                    "japaneseAfter");
                int count;
                return int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out count)
                    ? (int?)count
                    : null;
            }

            var heading = UsageResetSectionHeading.Match(pageText);
            if (!heading.Success) return null;
            var sectionStart = heading.Index + heading.Length;
            var boundary = UsageResetSectionBoundary.Match(pageText, sectionStart);
            var sectionLength = (boundary.Success ? boundary.Index : pageText.Length) - sectionStart;
            if (sectionLength < 0) return null;
            var section = pageText.Substring(sectionStart, sectionLength);
            var tabCount = AvailableUsageResetTabCount.Match(section);
            if (tabCount.Success)
            {
                int count;
                if (int.TryParse(tabCount.Groups["count"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out count))
                {
                    return count;
                }
            }
            var entryCount = UseResetAction.Matches(section).Count;
            if (entryCount > 0) return entryCount;
            return ExplicitNoUsageResets.IsMatch(section) ? (int?)0 : null;
        }

        internal static bool ContainsRecognizedUsageResetSection(string pageText)
        {
            return !string.IsNullOrWhiteSpace(pageText) &&
                UsageResetSectionHeading.IsMatch(NormalizePageText(pageText));
        }

        public static bool LooksLikeLoginRequired(Uri source, string pageText)
        {
            var path = source == null ? string.Empty : source.AbsolutePath;
            if (path.IndexOf("/auth/", StringComparison.OrdinalIgnoreCase) >= 0 ||
                path.IndexOf("/login", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
            var text = pageText ?? string.Empty;
            var prompt = text.IndexOf("Log in", StringComparison.OrdinalIgnoreCase) >= 0 ||
                text.IndexOf("Sign in", StringComparison.OrdinalIgnoreCase) >= 0 ||
                text.IndexOf("登录", StringComparison.OrdinalIgnoreCase) >= 0 ||
                text.IndexOf("登入", StringComparison.OrdinalIgnoreCase) >= 0 ||
                text.IndexOf("登錄", StringComparison.OrdinalIgnoreCase) >= 0 ||
                text.IndexOf("로그인", StringComparison.OrdinalIgnoreCase) >= 0 ||
                text.IndexOf("ログイン", StringComparison.OrdinalIgnoreCase) >= 0;
            return prompt && !WeeklyLabel.IsMatch(text);
        }

        internal static int? ParsePercent(string segment)
        {
            segment = NormalizePageText(segment ?? string.Empty);
            var match = Percent.Match(segment);
            int raw;
            if (!match.Success || !int.TryParse(match.Groups["value"].Value, out raw) || raw < 0 || raw > 100)
            {
                return null;
            }
            var contextStart = segment.LastIndexOfAny(new[] { '\r', '\n' }, Math.Max(0, match.Index));
            contextStart = contextStart < 0 ? 0 : contextStart + 1;
            var contextEnd = segment.IndexOfAny(new[] { '\r', '\n' }, match.Index + match.Length);
            if (contextEnd < 0) contextEnd = segment.Length;
            var contextLength = contextEnd - contextStart;
            var context = contextLength > 0 ? segment.Substring(contextStart, contextLength) : string.Empty;
            var saysRemaining = RemainingDirection.IsMatch(context);
            var saysUsed = UsedDirection.IsMatch(context);
            if (saysRemaining && saysUsed)
            {
                return null;
            }
            if (saysUsed)
            {
                return Percentage.FromUsed(raw);
            }
            // preview.7 treated an unqualified value as remaining; preserve that verified baseline behavior.
            return Percentage.Clamp(raw);
        }

        internal static DateTime? ParseReset(string segment, DateTime now)
        {
            var match = ResetLine.Match(NormalizePageText(segment ?? string.Empty));
            if (!match.Success)
            {
                return null;
            }
            return ResetTimeParser.TryParse(match.Value, now);
        }

        private static List<UsageLabel> FindLabels(string text)
        {
            var result = new List<UsageLabel>();
            AddLabels(result, text, FiveHourLabel, true);
            AddLabels(result, text, WeeklyLabel, false);
            result.Sort((left, right) => left.Index.CompareTo(right.Index));
            return result;
        }

        private static void AddLabels(List<UsageLabel> labels, string text, Regex regex, bool fiveHour)
        {
            foreach (Match match in regex.Matches(text))
            {
                var lineStart = text.LastIndexOfAny(new[] { '\r', '\n' }, Math.Max(0, match.Index));
                lineStart = lineStart < 0 ? 0 : lineStart + 1;
                var lineEnd = text.IndexOfAny(new[] { '\r', '\n' }, match.Index + match.Length);
                if (lineEnd < 0) lineEnd = text.Length;
                var currentLine = text.Substring(lineStart, lineEnd - lineStart);
                if (ResetDescription.IsMatch(currentLine)) continue;
                var contextStart = lineStart;
                if (lineStart > 0)
                {
                    var scan = lineStart - 1;
                    while (scan >= 0 && (text[scan] == '\r' || text[scan] == '\n')) scan--;
                    var previousBreak = scan >= 0
                        ? text.LastIndexOfAny(new[] { '\r', '\n' }, scan)
                        : -1;
                    contextStart = previousBreak < 0 ? 0 : previousBreak + 1;
                }
                var line = text.Substring(contextStart, lineEnd - contextStart);
                labels.Add(new UsageLabel
                {
                    Index = match.Index,
                    EndIndex = match.Index + match.Length,
                    IsFiveHour = fiveHour,
                    IsSpark = SparkLabel.IsMatch(line)
                });
            }
        }

        private static string SegmentAfter(string text, List<UsageLabel> labels, UsageLabel label)
        {
            var end = Math.Min(text.Length, label.EndIndex + 400);
            foreach (var next in labels)
            {
                if (next.Index > label.Index)
                {
                    end = Math.Min(end, next.Index);
                    break;
                }
            }
            return end <= label.EndIndex ? string.Empty : text.Substring(label.EndIndex, end - label.EndIndex);
        }

        internal static string NormalizePageText(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value.Normalize(NormalizationForm.FormKC)
                .Replace('\u00a0', ' ')
                .Replace('\u3000', ' ');
        }

        private static string FirstSuccessfulGroup(Match match, params string[] names)
        {
            foreach (var name in names)
            {
                if (match.Groups[name].Success) return match.Groups[name].Value;
            }
            return string.Empty;
        }

        private sealed class UsageLabel
        {
            public int Index { get; set; }
            public int EndIndex { get; set; }
            public bool IsFiveHour { get; set; }
            public bool IsSpark { get; set; }
        }
    }

    internal static class ResetTimeParser
    {
        private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en-US");
        private static readonly CultureInfo ChineseSimplified = CultureInfo.GetCultureInfo("zh-CN");
        private static readonly CultureInfo ChineseTraditional = CultureInfo.GetCultureInfo("zh-TW");
        private static readonly CultureInfo Japanese = CultureInfo.GetCultureInfo("ja-JP");
        private static readonly CultureInfo Korean = CultureInfo.GetCultureInfo("ko-KR");

        public static DateTime? TryParse(string value, DateTime now)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }
            var text = Regex.Replace(UsageParser.NormalizePageText(value), @"\s+", " ").Trim();
            text = Regex.Replace(text, @"(?i)\b(?:resets?|resetting)\b", string.Empty).Trim(' ', ':', '-', '–');
            text = Regex.Replace(text, @"(?i)\b(?:at|on)\b", " ");
            text = Regex.Replace(text, @"(?:重置|重设|重設)(?:时间|時間)?(?:为|為|于|於)?", string.Empty);
            text = Regex.Replace(text, @"^(?:将于|將於)\s*", string.Empty);
            text = Regex.Replace(text, @"(?:초기화|재설정)", string.Empty);
            text = Regex.Replace(text, @"リセット", string.Empty);
            text = Regex.Replace(text, @"(?:时间|時間)\s*[:：]?", string.Empty);
            text = Regex.Replace(text, @"(?<=\d)\s*(?:に|에)(?=\s|$)", string.Empty);
            text = text.Trim(' ', ':', '-', '–');
            text = Regex.Replace(text, @"(\d{4})\.\s*(\d{1,2})\.\s*(\d{1,2})\.?", "$1-$2-$3");
            text = Regex.Replace(text, @"(?:上午|早上|凌晨)\s*(\d{1,2}:\d{2})", "$1 AM");
            text = Regex.Replace(text, @"(?:下午|晚上|中午)\s*(\d{1,2}:\d{2})", "$1 PM");
            text = Regex.Replace(text, @"오전\s*(\d{1,2}:\d{2})", "$1 AM");
            text = Regex.Replace(text, @"오후\s*(\d{1,2}:\d{2})", "$1 PM");
            text = Regex.Replace(text, @"\s+", " ").Trim();

            var hasExplicitYear = Regex.IsMatch(
                text,
                @"(?<!\d)\d{4}(?!\d)|\d{4}\s*(?:年|년)",
                RegexOptions.CultureInvariant);
            if (!hasExplicitYear && Regex.IsMatch(
                text,
                @"^\d{1,2}[/-]\d{1,2}\s+\d{1,2}:\d{2}(?::\d{2})?\s*(?:AM|PM)?$",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            {
                text = now.Year.ToString(CultureInfo.InvariantCulture) + "/" + text;
            }

            DateTime parsed;
            var styles = DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal;
            if (!DateTime.TryParse(text, English, styles, out parsed) &&
                !DateTime.TryParse(text, ChineseSimplified, styles, out parsed) &&
                !DateTime.TryParse(text, ChineseTraditional, styles, out parsed) &&
                !DateTime.TryParse(text, Japanese, styles, out parsed) &&
                !DateTime.TryParse(text, Korean, styles, out parsed) &&
                !DateTime.TryParse(text, CultureInfo.CurrentCulture, styles, out parsed))
            {
                return null;
            }
            parsed = DateTime.SpecifyKind(parsed, DateTimeKind.Local);
            var timeOnly = Regex.IsMatch(text, @"^\d{1,2}:\d{2}(?::\d{2})?\s*(?:AM|PM)?$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            if (timeOnly)
            {
                parsed = DateTime.SpecifyKind(now.Date.Add(parsed.TimeOfDay), DateTimeKind.Local);
            }
            if (parsed < now.AddMinutes(-1))
            {
                if (timeOnly)
                {
                    parsed = parsed.AddDays(1);
                }
                else if (!hasExplicitYear)
                {
                    parsed = parsed.AddYears(1);
                }
            }
            return parsed;
        }
    }
}
