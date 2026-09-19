using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using CodexUsageTrayLite.Infrastructure;
using CodexUsageTrayLite.Models;
using CodexUsageTrayLite.Rendering;
using CodexUsageTrayLite.Services;
using CodexUsageTrayLite.UI;
using Microsoft.Web.WebView2.Core;

namespace CodexUsageTrayLite.Tests
{
    internal static class TestRunner
    {
        private static readonly List<string> Failures = new List<string>();
        private static int passed;

        [STAThread]
        private static int Main(string[] args)
        {
            Application.EnableVisualStyles();
            try
            {
                if (args.Length >= 2 && args[0] == "--generate-icons")
                {
                    IconPreviewGenerator.Generate(args[1]);
                    Console.WriteLine("Generated icon previews under docs\\icon-options.");
                    return 0;
                }
                if (args.Length >= 2 && args[0] == "--generate-quota-modes")
                {
                    IconPreviewGenerator.GenerateQuotaModes(args[1]);
                    Console.WriteLine("Generated quota mode preview under docs\\icon-options.");
                    return 0;
                }
                if (args.Length >= 2 && args[0] == "--generate-side-by-side")
                {
                    IconPreviewGenerator.GenerateSideBySide(args[1]);
                    Console.WriteLine("Generated Side by Side preview under docs\\icon-options.");
                    return 0;
                }
                if (args.Length >= 1 && args[0] == "--webview-probe")
                {
                    var count = args.Length >= 2 ? int.Parse(args[1]) : 50;
                    return WebViewLifetimeProbe.Run(count);
                }
                if (args.Length >= 1 && args[0] == "--webview-log-probe")
                {
                    return WebViewLifetimeProbe.RunTelemetry();
                }
                if (args.Length >= 1 && args[0] == "--form-probe")
                {
                    var count = args.Length >= 2 ? int.Parse(args[1]) : 50;
                    return WebViewLifetimeProbe.RunForms(count);
                }
                if (args.Length >= 1 && args[0] == "--cli-probe")
                {
                    return RunCliProbeAsync().GetAwaiter().GetResult();
                }
                if (args.Length >= 1 && args[0] == "--webview-email-probe")
                {
                    return RunWebViewEmailProbe();
                }
                return RunAllAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return 2;
            }
        }

        private static async Task<int> RunAllAsync()
        {
            Test("UsedToRemaining", () => Equal(71, Percentage.FromUsed(29)));
            Test("PercentageClamping", () => { Equal(0, Percentage.Clamp(-8)); Equal(100, Percentage.Clamp(109)); });
            Test("ParserNormalUsed", ParserNormalUsed);
            Test("ParserRemainingBaseline", ParserRemainingBaseline);
            Test("ParserMalformed", ParserMalformed);
            Test("ParserLoginPage", ParserLoginPage);
            Test("ParserWhitespaceAndKorean", ParserWhitespaceAndKorean);
            Test("ParserSimplifiedChinese", ParserSimplifiedChinese);
            Test("ParserTraditionalChinese", ParserTraditionalChinese);
            Test("ParserHongKongTraditionalWeeklyOnly", ParserHongKongTraditionalWeeklyOnly);
            Test("ParserTaiwanTraditionalWeeklyOnly", ParserTaiwanTraditionalWeeklyOnly);
            Test("ParserJapaneseWeeklyOnly", ParserJapaneseWeeklyOnly);
            Test("ParserKoreanWeeklyOnly", ParserKoreanWeeklyOnly);
            Test("ParserWeeklyOnlyAndExplicitUnlimitedPlans", ParserWeeklyOnlyAndExplicitUnlimitedPlans);
            Test("ParserMixedLanguageAndFullWidth", ParserMixedLanguageAndFullWidth);
            Test("ParserSparkAndAmbiguousDirectionSafeguards", ParserSparkAndAmbiguousDirectionSafeguards);
            Test("UsageResetCountParsing", UsageResetCountParsing);
            Test("LocalizedUsageResetCountParsing", LocalizedUsageResetCountParsing);
            Test("UsageResetEntryListParsing", UsageResetEntryListParsing);
            Test("UsageResetUnavailableBannerParsing", UsageResetUnavailableBannerParsing);
            Test("WebViewUsageResetDomProbeContract", WebViewUsageResetDomProbeContract);
            Test("WebViewWeeklyOnlyResetFallback", WebViewWeeklyOnlyResetFallback);
            Test("BackgroundWebViewHostDoesNotActivate", BackgroundWebViewHostDoesNotActivate);
            Test("ResetTimeParsing", ResetTimeParsing);
            Test("ResetTimeFormatting", ResetTimeFormatting);
            Test("TooltipRequestedFormat", TooltipRequestedFormat);
            Test("AccountEmailExtractionAndFormatting", AccountEmailExtractionAndFormatting);
            Test("UserLevelDetectionAndFormatting", UserLevelDetectionAndFormatting);
            Test("ProxyArguments", ProxyArguments);
            Test("ProxyExternalArgumentPriority", ProxyExternalArgumentPriority);
            Test("ProxyValidation", ProxyValidation);
            Test("SettingsSerialization", SettingsSerialization);
            Test("QuotaIconModeSettingsMigration", QuotaIconModeSettingsMigration);
            Test("FirstRunRefreshPolicy", FirstRunRefreshPolicy);
            Test("LegacySettingsRefreshMigration", LegacySettingsRefreshMigration);
            Test("UsageSourceSettingsMigration", UsageSourceSettingsMigration);
            Test("UsageSourceSwitchRefreshPolicy", UsageSourceSwitchRefreshPolicy);
            Test("UsageSourceMenuVisibilityPolicy", UsageSourceMenuVisibilityPolicy);
            Test("UsageResetMenuState", UsageResetMenuState);
            Test("CodexCliRateLimitsParsing", CodexCliRateLimitsParsing);
            Test("CodexCliWeeklyOnlyPlanParsing", CodexCliWeeklyOnlyPlanParsing);
            Test("CodexCliRateLimitsFallbackAndValidation", CodexCliRateLimitsFallbackAndValidation);
            Test("CodexCliProcessLaunchModes", CodexCliProcessLaunchModes);
            Test("CodexCliLocatorPathSearch", CodexCliLocatorPathSearch);
            Test("CodexCliGracefulProcessExitTelemetry", CodexCliGracefulProcessExitTelemetry);
            Test("CodexCliForcedProcessExitTelemetry", CodexCliForcedProcessExitTelemetry);
            Test("CodexCliJobDrainSeverity", CodexCliJobDrainSeverity);
            Test("WebViewProcessTelemetryFormatting", WebViewProcessTelemetryFormatting);
            Test("ThemeSettingsAndResolution", ThemeSettingsAndResolution);
            Test("LanguageDetectionAndMigration", LanguageDetectionAndMigration);
            Test("LocalizedTextContracts", LocalizedTextContracts);
            Test("ResxLocalizationResources", ResxLocalizationResources);
            Test("RefreshIntervalValidation", RefreshIntervalValidation);
            Test("IconThresholdMapping", IconThresholdMapping);
            Test("DualQuotaReferencePixels", DualQuotaReferencePixels);
            Test("QuotaIconModeTooltipContract", QuotaIconModeTooltipContract);
            Test("QuotaIconModeRenderingMatrix", QuotaIconModeRenderingMatrix);
            Test("SideBySideRenderingMatrix", SideBySideRenderingMatrix);
            Test("SideBySideWeeklyFallback", SideBySideWeeklyFallback);
            Test("SideBySideStateAndCache", SideBySideStateAndCache);
            Test("AttentionGlyphMarks", AttentionGlyphMarks);
            Test("QuotaIconModeCacheAndStatePriority", QuotaIconModeCacheAndStatePriority);
            Test("WeeklyOnlyTooltipAndIcon", WeeklyOnlyTooltipAndIcon);
            Test("DualQuotaPrecisionAndStates", DualQuotaPrecisionAndStates);
            Test("DualQuotaCacheLifecycle", DualQuotaCacheLifecycle);
            Test("IndependentQuotaWarningColors", IndependentQuotaWarningColors);
            Test("QuotaWarningCacheTransitions", QuotaWarningCacheTransitions);
            Test("SelectedIconStyle", () => Equal(BatteryIconStyle.A, BatteryTrayIconRenderer.SelectedStyle));
            Test("MaximizedIconGeometryAndNativeSize", MaximizedIconGeometryAndNativeSize);
            Test("UnknownErrorStateMapping", UnknownErrorStateMapping);
            Test("TooltipLength", TooltipLength);
            Test("StartupPathQuoting", StartupPathQuoting);
            Test("SafeLoggerRedaction", SafeLoggerRedaction);
            Test("LogEventLevelAndFormat", LogEventLevelAndFormat);
            Test("UsageFieldUnavailableDiagnostics", UsageFieldUnavailableDiagnostics);
            Test("LogDailyRotation", LogDailyRotation);
            Test("LogFileOpenLaunch", LogFileOpenLaunch);
            await TestAsync("RefreshConcurrency", RefreshConcurrency);
            await TestAsync("RefreshCancellation", RefreshCancellation);
            await TestAsync("RepeatedRefreshLifecycle100", RepeatedRefreshLifecycle100);
            Test("GdiIconLifecycle", GdiIconLifecycle);
            Test("ThemeControlApplication", ThemeControlApplication);
            Test("ProxyDialogLayoutAndDirectOption", ProxyDialogLayoutAndDirectOption);
            Test("AboutDialogMetadataAndLayout", AboutDialogMetadataAndLayout);
            Test("HelpDialogContentAndLayout", HelpDialogContentAndLayout);
            Test("LocalizedDialogs", LocalizedDialogs);
            Test("ApplicationIconResourcesAndExecutable", ApplicationIconResourcesAndExecutable);
            Test("ApplicationIconWindowThemeAndLifecycle", ApplicationIconWindowThemeAndLifecycle);

            Console.WriteLine();
            Console.WriteLine("Passed: " + passed);
            Console.WriteLine("Failed: " + Failures.Count);
            foreach (var failure in Failures) Console.WriteLine("FAIL " + failure);
            return Failures.Count == 0 ? 0 : 1;
        }

        private static void ParserNormalUsed()
        {
            var text = "5-hour usage limit\n29% used\nResets at 12:48 PM\nWeekly usage limit\n19% used\nResets on Sep 5, 2026 2:30 PM\n1 reset available";
            UsageSnapshot snapshot;
            string error;
            True(UsageParser.TryParse(text, new DateTime(2026, 9, 1, 10, 0, 0), out snapshot, out error), error);
            Equal(71, snapshot.fiveHourRemainingPercent.Value);
            Equal(81, snapshot.weeklyRemainingPercent.Value);
            True(snapshot.fiveHourResetAt.HasValue, "5H reset missing");
            True(snapshot.weeklyResetAt.HasValue, "weekly reset missing");
            Equal(1, snapshot.availableUsageResetCount.Value);
        }

        private static void ParserRemainingBaseline()
        {
            var text = "5 hour usage limit\n71% remaining\nReset 12:48\nWeekly limit\n81% left\nReset Sep 5 14:30";
            UsageSnapshot snapshot;
            string error;
            True(UsageParser.TryParse(text, new DateTime(2026, 9, 1, 10, 0, 0), out snapshot, out error), error);
            Equal(71, snapshot.fiveHourRemainingPercent.Value);
            Equal(81, snapshot.weeklyRemainingPercent.Value);
        }

        private static void ParserMalformed()
        {
            UsageSnapshot snapshot;
            string error;
            True(!UsageParser.TryParse("unexpected structure", DateTime.Now, out snapshot, out error), "Malformed input parsed unexpectedly.");
            True(snapshot == null, "Malformed snapshot should be null.");
        }

        private static void ParserLoginPage()
        {
            True(UsageParser.LooksLikeLoginRequired(new Uri("https://chatgpt.com/auth/login"), "Log in"), "Login page not detected.");
        }

        private static void ParserWhitespaceAndKorean()
        {
            var text = "5 시간 사용 한도\n  20 % 사용됨\n2026. 9. 9. 오후 1:00 초기화\n주간 사용 한도\n 40% 사용됨\n2026. 9. 13. 오후 2:30 초기화\n" +
                "사용량 한도 재설정\n전체 재설정\n재설정 사용\n자동 충전";
            UsageSnapshot snapshot;
            string error;
            True(UsageParser.TryParse(text, new DateTime(2026, 9, 9, 10, 0, 0), out snapshot, out error), error);
            Equal(80, snapshot.fiveHourRemainingPercent.Value);
            Equal(60, snapshot.weeklyRemainingPercent.Value);
            Equal(new DateTime(2026, 9, 9, 13, 0, 0), snapshot.fiveHourResetAt.Value);
            Equal(new DateTime(2026, 9, 13, 14, 30, 0), snapshot.weeklyResetAt.Value);
            Equal(1, snapshot.availableUsageResetCount.Value);
        }

        private static void ParserSimplifiedChinese()
        {
            var text = "5 小时使用限额\n剩余 42％\n重置于 2026年9月9日下午 8:37\n" +
                "每周使用限额\n已使用 50%\n重置于 2026年9月11日下午 6:47\n" +
                "用量限制重置\n有 2 次可用重置\n自动充值";
            UsageSnapshot snapshot;
            string error;
            True(UsageParser.TryParse(text, new DateTime(2026, 9, 9, 10, 0, 0), out snapshot, out error), error);
            Equal(42, snapshot.fiveHourRemainingPercent.Value);
            Equal(50, snapshot.weeklyRemainingPercent.Value);
            Equal(new DateTime(2026, 9, 9, 20, 37, 0), snapshot.fiveHourResetAt.Value);
            Equal(new DateTime(2026, 9, 11, 18, 47, 0), snapshot.weeklyResetAt.Value);
            Equal(2, snapshot.availableUsageResetCount.Value);
        }

        private static void ParserTraditionalChinese()
        {
            var text = "5 小時用量限制\n已使用 37％\n重設於 2026年9月9日下午 9:05\n" +
                "每週用量限制\n剩餘 63％\n重設於 9月13日下午 8:15\n" +
                "用量限制重設\n完整重設\n使用重設\n自動加值";
            UsageSnapshot snapshot;
            string error;
            True(UsageParser.TryParse(text, new DateTime(2026, 9, 9, 10, 0, 0), out snapshot, out error), error);
            Equal(63, snapshot.fiveHourRemainingPercent.Value);
            Equal(63, snapshot.weeklyRemainingPercent.Value);
            Equal(new DateTime(2026, 9, 9, 21, 5, 0), snapshot.fiveHourResetAt.Value);
            Equal(new DateTime(2026, 9, 13, 20, 15, 0), snapshot.weeklyResetAt.Value);
            Equal(1, snapshot.availableUsageResetCount.Value);
        }

        private static void ParserHongKongTraditionalWeeklyOnly()
        {
            var text = "每週用量上限\n87%\n剩餘\n將於 2026年9月17日 下午7:35 重設\n" +
                "用量限額重設\n使用一次重設，即可恢復 5 小時用量上限、每週用量上限，或兩者。\n" +
                "可用\n0\n記錄\n過去 30 天\n自動加值";
            UsageSnapshot snapshot;
            string error;
            True(UsageParser.TryParse(text, new DateTime(2026, 9, 11, 11, 0, 0), out snapshot, out error), error);
            True(!snapshot.IsFiveHourLimitApplicable, "The Hong Kong reset description was mistaken for a 5-hour quota card.");
            True(snapshot.fiveHourLimitAbsenceInferred, "The Hong Kong weekly-only page did not request stabilization.");
            Equal(87, snapshot.weeklyRemainingPercent.Value);
            Equal(new DateTime(2026, 9, 17, 19, 35, 0), snapshot.weeklyResetAt.Value);
            Equal(0, snapshot.availableUsageResetCount.Value);
        }

        private static void ParserTaiwanTraditionalWeeklyOnly()
        {
            var text = "每週用量上限\n81%\n剩餘\n重設時間 2026年9月17日 下午7:35\n" +
                "使用量限制重設\n使用重置即可恢復 5 小時用量上限、每週用量上限，或兩者皆恢復。\n" +
                "可用\n0\n歷史紀錄\n過去 30 天\n自動儲值";
            UsageSnapshot snapshot;
            string error;
            True(UsageParser.TryParse(text, new DateTime(2026, 9, 11, 11, 0, 0), out snapshot, out error), error);
            True(!snapshot.IsFiveHourLimitApplicable, "The Taiwan reset description was mistaken for a 5-hour quota card.");
            True(snapshot.fiveHourLimitAbsenceInferred, "The Taiwan weekly-only page did not request stabilization.");
            Equal(81, snapshot.weeklyRemainingPercent.Value);
            Equal(new DateTime(2026, 9, 17, 19, 35, 0), snapshot.weeklyResetAt.Value);
            Equal(0, snapshot.availableUsageResetCount.Value);
        }

        private static void ParserJapaneseWeeklyOnly()
        {
            var text = "週間利用上限\n78%\n残り\nリセット：2026/09/17 19:35\n" +
                "利用制限のリセット\nリセットを使って、5時間の上限、週ごとの上限、またはその両方を復元できます。\n" +
                "利用可能\n0\n履歴\n過去30日間\nリセットを使用\n9月6日\n20:14 GMT+8\n" +
                "リセットを受け取りました\n9月5日\n6:28 GMT+8\n自動チャージ\n使用状況の内訳";
            UsageSnapshot snapshot;
            string error;
            True(UsageParser.TryParse(text, new DateTime(2026, 9, 11, 11, 0, 0), out snapshot, out error), error);
            True(!snapshot.IsFiveHourLimitApplicable, "The Japanese reset description was mistaken for a 5-hour quota card.");
            True(snapshot.fiveHourLimitAbsenceInferred, "The Japanese weekly-only page did not request stabilization.");
            Equal(78, snapshot.weeklyRemainingPercent.Value);
            Equal(new DateTime(2026, 9, 17, 19, 35, 0), snapshot.weeklyResetAt.Value);
            Equal(0, snapshot.availableUsageResetCount.Value);

            var historyOnly = "利用制限のリセット\nリセットを使って、5時間の上限、週ごとの上限、またはその両方を復元できます。\n" +
                "履歴\nリセットを使用\n9月6日\n20:14 GMT+8\n自動チャージ";
            True(!UsageParser.ParseAvailableUsageResetCount(historyOnly).HasValue,
                "A Japanese reset-history entry was mistaken for an available reset action.");
        }

        private static void ParserKoreanWeeklyOnly()
        {
            var text = "주간 사용 한도\n78%\n남음\n2026. 9. 17. 오후 7:35 초기화\n" +
                "사용량 한도 재설정\n재설정을 사용해 5시간 한도, 주간 한도 또는 둘 다를 복원하세요.\n" +
                "사용 가능\n0\n내역\n지난 30일\n재설정 사용됨\n9월 6일\n오후 8:14 GMT+8\n" +
                "재설정 받음\n9월 5일\n오전 6:28 GMT+8\n자동 충전\n사용량 세부 정보";
            UsageSnapshot snapshot;
            string error;
            True(UsageParser.TryParse(text, new DateTime(2026, 9, 11, 11, 0, 0), out snapshot, out error), error);
            True(!snapshot.IsFiveHourLimitApplicable, "The Korean reset description was mistaken for a 5-hour quota card.");
            True(snapshot.fiveHourLimitAbsenceInferred, "The Korean weekly-only page did not request stabilization.");
            Equal(78, snapshot.weeklyRemainingPercent.Value);
            Equal(new DateTime(2026, 9, 17, 19, 35, 0), snapshot.weeklyResetAt.Value);
            Equal(0, snapshot.availableUsageResetCount.Value);
        }

        private static void ParserWeeklyOnlyAndExplicitUnlimitedPlans()
        {
            var weeklyOnly =
                "Weekly usage limit\n9% used\nResets Sep 17, 2026 8:15 PM\n" +
                "Usage limit resets\nUse a reset to restore your 5-hour limit, weekly limit, or both.\n" +
                "No usage limit resets available at this time.\nAuto reload";
            UsageSnapshot snapshot;
            string error;
            True(UsageParser.TryParse(weeklyOnly, new DateTime(2026, 9, 10, 19, 0, 0), out snapshot, out error), error);
            True(!snapshot.IsFiveHourLimitApplicable, "An absent 5-hour plan limit was treated as applicable.");
            True(snapshot.fiveHourLimitAbsenceInferred, "The missing WebView2 card did not request stabilization.");
            True(!snapshot.fiveHourRemainingPercent.HasValue && !snapshot.fiveHourResetAt.HasValue,
                "A weekly-only plan synthesized 5-hour values.");
            Equal(91, snapshot.weeklyRemainingPercent.Value);
            Equal(0, snapshot.availableUsageResetCount.Value);

            var explicitUnlimited =
                "5-hour usage limit\nUnlimited\nWeekly usage limit\n12% used\nResets Sep 17, 2026 8:15 PM";
            True(UsageParser.TryParse(explicitUnlimited, new DateTime(2026, 9, 10, 19, 0, 0), out snapshot, out error), error);
            True(!snapshot.IsFiveHourLimitApplicable, "An explicit unlimited 5-hour limit was treated as numeric.");
            True(!snapshot.fiveHourLimitAbsenceInferred, "An explicit unlimited card was treated as a temporarily missing card.");
            Equal(88, snapshot.weeklyRemainingPercent.Value);
            Equal(3, WebViewFetchSession.MissingFiveHourConfirmationAttempts);
        }

        private static void ParserMixedLanguageAndFullWidth()
        {
            var text = "５-hour usage limit\n已用 ２９％\n重置于 ２３：３０\n" +
                "每週用量限制\n８１％ remaining\n重設於 ９／１１ １８：４７";
            UsageSnapshot snapshot;
            string error;
            True(UsageParser.TryParse(text, new DateTime(2026, 9, 9, 23, 40, 0), out snapshot, out error), error);
            Equal(71, snapshot.fiveHourRemainingPercent.Value);
            Equal(81, snapshot.weeklyRemainingPercent.Value);
            True(snapshot.fiveHourResetAt.HasValue, "Full-width time-only reset did not parse.");
            True(snapshot.weeklyResetAt.HasValue, "Full-width cross-date reset did not parse.");
            Equal(new DateTime(2026, 9, 10, 23, 30, 0), snapshot.fiveHourResetAt.Value);
            Equal(new DateTime(2026, 9, 11, 18, 47, 0), snapshot.weeklyResetAt.Value);
        }

        private static void ParserSparkAndAmbiguousDirectionSafeguards()
        {
            var text = "GPT-5.3-Codex-Spark\n5-hour usage limit\n99% remaining\n" +
                "5-hour usage limit\n71% remaining\nWeekly usage limit\n81% remaining";
            UsageSnapshot snapshot;
            string error;
            True(UsageParser.TryParse(text, new DateTime(2026, 9, 9, 10, 0, 0), out snapshot, out error), error);
            Equal(71, snapshot.fiveHourRemainingPercent.Value);
            Equal(81, snapshot.weeklyRemainingPercent.Value);
            True(!UsageParser.ParsePercent("20% remaining; 80% used").HasValue,
                "Conflicting percentage direction text was guessed instead of rejected.");
        }

        private static void UsageResetCountParsing()
        {
            Equal(1, UsageParser.ParseAvailableUsageResetCount("You have 1 reset available.").Value);
            Equal(12, UsageParser.ParseAvailableUsageResetCount("12 available Codex resets").Value);
            Equal(0, UsageParser.ParseAvailableUsageResetCount("0 usage resets available").Value);
            True(!UsageParser.ParseAvailableUsageResetCount("Full reset").HasValue,
                "A WebView reset label without an explicit count was treated as an exact value.");
            True(!UsageParser.ParseAvailableUsageResetCount("Resets at 12:48 PM").HasValue,
                "A quota reset time was mistaken for an available reset credit count.");
        }

        private static void LocalizedUsageResetCountParsing()
        {
            Equal(2, UsageParser.ParseAvailableUsageResetCount("有 2 次可用重置").Value);
            Equal(3, UsageParser.ParseAvailableUsageResetCount("有 3 個可用重設").Value);
            Equal(4, UsageParser.ParseAvailableUsageResetCount("재설정 4회 사용 가능").Value);
            Equal(5, UsageParser.ParseAvailableUsageResetCount(
                "用量限制重置\n可用 5\n历史\n过去 30 天\n自动充值").Value);
            Equal(6, UsageParser.ParseAvailableUsageResetCount(
                "사용량 한도 재설정\n사용 가능 6\n기록\n지난 30일\n자동 충전").Value);

            var simplifiedEntries =
                "用量限制重置\n使用重置恢复 5 小时或每周限额。\n" +
                "完整重置\n使用重置\n5 小时重置\n使用重置\n自动充值";
            Equal(2, UsageParser.ParseAvailableUsageResetCount(simplifiedEntries).Value);

            var traditionalEmpty = "用量限制重設\n目前沒有可用的用量限制重設。\n自動加值";
            Equal(0, UsageParser.ParseAvailableUsageResetCount(traditionalEmpty).Value);
            var koreanEmpty = "사용량 한도 재설정\n현재 사용 가능한 재설정이 없습니다.\n자동 충전";
            Equal(0, UsageParser.ParseAvailableUsageResetCount(koreanEmpty).Value);
            True(!UsageParser.ParseAvailableUsageResetCount(
                    "用量限制重置\n当前状态未知。\n自动充值").HasValue,
                "An ambiguous localized reset state was treated as zero.");
        }

        private static void UsageResetEntryListParsing()
        {
            var screenshotTab =
                "Usage limit resets\n" +
                "Use a reset to restore your 5-hour limit, weekly limit, or both.\n" +
                "Available 0\nHistory\nPast 30 days\n" +
                "Reset used\nSep 6\nReset received\nSep 5\nAuto reload";
            Equal(0, UsageParser.ParseAvailableUsageResetCount(screenshotTab).Value);
            Equal(2, UsageParser.ParseAvailableUsageResetCount(
                screenshotTab.Replace("Available 0", "Available (2)")).Value);
            True(!UsageParser.ParseAvailableUsageResetCount(
                    "Available 7\nHistory\nPast 30 days").HasValue,
                "An Available tab outside the reset section was counted.");

            var oneEntry =
                "Credits remaining\n0\nUsage limit resets\n" +
                "Use a reset to restore your 5-hour limit, weekly limit, or both.\n" +
                "Full reset (Weekly + 5 hr)\nExpires Oct 5, 6:28 AM\nUse reset\n" +
                "Auto reload\nSettings";
            Equal(1, UsageParser.ParseAvailableUsageResetCount(oneEntry).Value);

            var twoEntries =
                "Usage limit resets\r\nUse a reset to restore your limits.\r\n" +
                "Full reset (Weekly + 5 hr)\r\nExpires Oct 5\r\nUse reset\r\n" +
                "5-hour reset\r\nExpires Oct 6\r\nUse reset\r\nUsage breakdown";
            Equal(2, UsageParser.ParseAvailableUsageResetCount(twoEntries).Value);

            Equal(0, UsageParser.ParseAvailableUsageResetCount(
                "Usage limit resets\nNo resets available.\nAuto reload").Value);
            True(!UsageParser.ParseAvailableUsageResetCount(
                    "Usage limit resets\nUse a reset to restore your limits.\nAuto reload\nUse reset").HasValue,
                "Descriptive or out-of-section reset text was counted as an available entry.");
        }

        private static void UsageResetUnavailableBannerParsing()
        {
            var exactBanner =
                "Usage limit resets\n" +
                "Use a reset to restore your 5-hour limit, weekly limit, or both.\n" +
                "No usage limit resets available at this time.\n" +
                "Auto reload";
            Equal(0, UsageParser.ParseAvailableUsageResetCount(exactBanner).Value);
            True(!UsageParser.ParseAvailableUsageResetCount(
                    "Usage limit resets\nAvailability unknown at this time.\nAuto reload").HasValue,
                "An ambiguous reset status was treated as an explicit zero.");
            True(!UsageParser.ParseAvailableUsageResetCount(
                    "Usage limit resets\nAuto reload\nNo usage limit resets available at this time.").HasValue,
                "An out-of-section unavailable banner was treated as an explicit zero.");
        }

        private static void WebViewUsageResetDomProbeContract()
        {
            Equal(1, WebViewUsageResetDomProbe.ParseCount("1").Value);
            Equal(0, WebViewUsageResetDomProbe.ParseCount("0").Value);
            True(!WebViewUsageResetDomProbe.ParseCount("null").HasValue, "A missing DOM result was treated as zero.");
            True(!WebViewUsageResetDomProbe.ParseCount("-1").HasValue, "A negative DOM result was accepted.");
            True(!WebViewUsageResetDomProbe.ParseCount("1.5").HasValue, "A fractional DOM result was accepted.");
            True(WebViewUsageResetDomProbe.BackgroundViewportSize.Width >= 1280 &&
                 WebViewUsageResetDomProbe.BackgroundViewportSize.Height >= 800,
                "The hidden WebView viewport is too small for the desktop Usage layout.");
            var script = WebViewUsageResetDomProbe.Script;
            True(script.IndexOf("Usage limit resets", StringComparison.OrdinalIgnoreCase) >= 0 &&
                 script.IndexOf("Use reset", StringComparison.OrdinalIgnoreCase) >= 0 &&
                 script.Contains("availableCountPatterns") && script.Contains("^available") &&
                 script.Contains("scrollIntoView") && script.Contains("[role=\"button\"]") &&
                 script.IndexOf("at this time", StringComparison.OrdinalIgnoreCase) >= 0 &&
                 script.IndexOf("Auto reload", StringComparison.OrdinalIgnoreCase) >= 0 &&
                 script.IndexOf("Usage breakdown", StringComparison.OrdinalIgnoreCase) >= 0,
                "The DOM probe lacks its section, action, scroll, or boundary safeguards.");
            True(script.Contains("用量限制重置") && script.Contains("用量限制重設") &&
                 script.Contains("用量限額重設") &&
                 script.Contains("사용량 한도 재설정") && script.Contains("使用制限のリセット") &&
                 script.Contains("利用制限のリセット") && script.Contains("自動チャージ") &&
                 script.Contains("使用状況の内訳") &&
                 script.Contains("normalize('NFKC')"),
                "The DOM probe lacks multilingual or full-width-text support.");
            True(WebViewUsageResetDomProbe.MaximumAttempts * WebViewUsageResetDomProbe.RetryDelayMilliseconds >= 3000,
                "The DOM probe does not allow enough time for delayed reset-card rendering.");
        }

        private static void WebViewWeeklyOnlyResetFallback()
        {
            var weeklyOnly = new UsageSnapshot
            {
                fiveHourLimitApplies = false,
                weeklyRemainingPercent = 98
            };
            var withoutSection =
                "Weekly usage limit\n2% used\nResets Sep 17, 2026 8:15 PM\n" +
                "Credits remaining\n0\nAuto reload";
            Equal(0, WebViewFetchSession.ResolveMissingUsageResetCountForWeeklyOnly(
                weeklyOnly,
                withoutSection,
                true).Value);
            True(!UsageParser.ContainsRecognizedUsageResetSection(withoutSection),
                "A weekly-only page without a reset section reported one.");

            var ambiguousSection = withoutSection +
                "\nUsage limit resets\nUnexpected future page wording\nUsage breakdown";
            True(UsageParser.ContainsRecognizedUsageResetSection(ambiguousSection),
                "A recognized reset-section heading was missed.");
            True(!WebViewFetchSession.ResolveMissingUsageResetCountForWeeklyOnly(
                    weeklyOnly,
                    ambiguousSection,
                    true).HasValue,
                "An ambiguous reset section was converted to zero.");

            var normalPlan = new UsageSnapshot
            {
                fiveHourLimitApplies = true,
                fiveHourRemainingPercent = 80,
                weeklyRemainingPercent = 98
            };
            True(!WebViewFetchSession.ResolveMissingUsageResetCountForWeeklyOnly(
                    normalPlan,
                    withoutSection,
                    true).HasValue,
                "A missing reset section on a normal dual-window plan was converted to zero.");
            True(!WebViewFetchSession.ResolveMissingUsageResetCountForWeeklyOnly(
                    null,
                    withoutSection,
                    true).HasValue,
                "A missing snapshot was converted to zero.");
            True(!WebViewFetchSession.ResolveMissingUsageResetCountForWeeklyOnly(
                    weeklyOnly,
                    withoutSection,
                    false).HasValue,
                "A failed DOM probe was converted to zero.");
        }

        private static void BackgroundWebViewHostDoesNotActivate()
        {
            var viewport = WebViewUsageResetDomProbe.BackgroundViewportSize;
            Exception failure = null;
            var thread = new Thread(() =>
            {
                try
                {
                    using (var host = new BackgroundWebViewHostForm(viewport))
                    {
                        True(!host.Visible, "The background WebView host was visible before handle creation.");
                        True(host.CreateHostHandle() != IntPtr.Zero, "The background WebView host HWND was not created.");
                        True(!host.Visible, "Creating the background WebView host HWND made the form visible.");
                        True(host.SuppressesActivation, "The background WebView host does not suppress activation.");
                        Equal(viewport, host.ClientSize);
                    }
                }
                catch (Exception ex)
                {
                    failure = ex;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            True(thread.Join(TimeSpan.FromSeconds(10)), "The background WebView host contract test timed out.");
            if (failure != null) throw failure;
        }

        private static void ResetTimeParsing()
        {
            var now = new DateTime(2026, 9, 1, 13, 0, 0);
            var time = ResetTimeParser.TryParse("Resets at 12:48 PM", now);
            True(time.HasValue, "Time-only reset did not parse.");
            Equal(new DateTime(2026, 9, 2, 12, 48, 0), time.Value);
            var dated = ResetTimeParser.TryParse("Resets on Sep 5, 2026 2:30 PM", now);
            Equal(new DateTime(2026, 9, 5, 14, 30, 0), dated.Value);
            var simplifiedEvening = ResetTimeParser.TryParse("重置时间：9月11日晚上 8:15", now);
            Equal(new DateTime(2026, 9, 11, 20, 15, 0), simplifiedEvening.Value);
            var traditionalMorning = ResetTimeParser.TryParse("重設時間：9月12日上午 6:05", now);
            Equal(new DateTime(2026, 9, 12, 6, 5, 0), traditionalMorning.Value);
            var japanese = ResetTimeParser.TryParse("2026年9月17日 19:35にリセット", now);
            Equal(new DateTime(2026, 9, 17, 19, 35, 0), japanese.Value);
            var korean = ResetTimeParser.TryParse("2026년 9월 17일 오후 7:35에 재설정", now);
            Equal(new DateTime(2026, 9, 17, 19, 35, 0), korean.Value);
        }

        private static void ResetTimeFormatting()
        {
            Equal("12:48", TooltipFormatter.FormatReset(new DateTime(2026, 9, 1, 12, 48, 0), false));
            Equal("14:30 9/5", TooltipFormatter.FormatReset(new DateTime(2026, 9, 5, 14, 30, 0), true));
        }

        private static void TooltipRequestedFormat()
        {
            var snapshot = new UsageSnapshot
            {
                fiveHourRemainingPercent = 20,
                weeklyRemainingPercent = 62,
                fiveHourResetAt = new DateTime(2026, 9, 1, 14, 54, 0),
                weeklyResetAt = new DateTime(2026, 9, 7, 14, 58, 0)
            };
            Equal("5-Hour 20% (14:54)\nWeekly 62% (14:58 9/7)",
                TooltipFormatter.Build(snapshot, TrayVisualState.Normal, false));
        }

        private static void AccountEmailExtractionAndFormatting()
        {
            string email;
            UserLevel userLevel;
            True(AccountEmailHelper.TryReadCliAccount(
                new Dictionary<string, object>
                {
                    { "type", "chatgpt" },
                    { "email", " user@example.com " },
                    { "planType", "plus" }
                },
                out email, out userLevel), "CLI account response was not recognized.");
            Equal("user@example.com", email);
            Equal(UserLevel.Plus, userLevel);

            True(AccountEmailHelper.TryReadCliAccount(
                new Dictionary<string, object> { { "type", "apiKey" } }, out email),
                "A recognized account without email should still resolve identity lookup.");
            True(email == null, "An API-key account unexpectedly produced an email.");

            const string prefix = "probe:";
            True(AccountEmailHelper.TryReadWebViewMessage(
                prefix + "{\"resolved\":true,\"email\":\"web@example.com\",\"userLevel\":\"pro\"}", prefix, out email, out userLevel),
                "WebView session response was not recognized.");
            Equal("web@example.com", email);
            Equal(UserLevel.Pro, userLevel);
            True(!AccountEmailHelper.TryReadWebViewMessage(
                prefix + "{\"resolved\":false,\"email\":null}", prefix, out email),
                "An unresolved WebView response was accepted.");

            var script = AccountEmailHelper.BuildWebViewScript(prefix);
            True(script.Contains("/api/auth/session") && script.Contains("credentials: 'include'") && script.Contains("cache: 'no-store'"),
                "WebView account script lacks the constrained same-origin session request.");
            True(script.Contains("userLevel") && script.Contains("planType") && script.Contains("plan_type"),
                "WebView account script lacks user-level candidates.");
            Equal("user@example.com", AccountEmailPrivacy.Normalize(" user@example.com "));
            Equal("u***@example.com", AccountEmailPrivacy.MaskForDisplay("user@example.com"));
            Equal("Email: user@example.com", AccountEmailPrivacy.FormatMenu("user@example.com"));
            Equal("Email: --", AccountEmailPrivacy.FormatMenu(null));

            var outcome = FetchOutcome.Create(FetchStatus.Success, "ok", new UsageSnapshot()).WithAccountEmail("user@example.com");
            True(outcome.AccountEmailRead, "Fetch outcome did not retain email-read state.");
            Equal("user@example.com", outcome.AccountEmail);
        }

        private static void UserLevelDetectionAndFormatting()
        {
            Equal(UserLevel.Free, UserLevelHelper.Parse("free"));
            Equal(UserLevel.Go, UserLevelHelper.Parse("chatgpt_go"));
            Equal(UserLevel.Plus, UserLevelHelper.Parse("PLUS"));
            Equal(UserLevel.Pro, UserLevelHelper.Parse("prolite"));
            Equal(UserLevel.Unknown, UserLevelHelper.Parse("business"));
            Equal(UserLevel.Pro, UserLevelHelper.ReadFromVisiblePageText(
                "Code\nSecurity\nApp\nDocs\nPRO\nSettings\nAnalytics"));
            Equal(UserLevel.Unknown, UserLevelHelper.ReadFromVisiblePageText(
                "A professional usage summary\nUpgrade your plan"));
            Equal(UserLevel.Unknown, UserLevelHelper.ReadFromVisiblePageText(
                "Code\npro\nUsage"));
            True(!UserLevelHelper.IsAbovePlus(UserLevel.Plus), "Plus was incorrectly treated as above Plus.");
            True(UserLevelHelper.IsAbovePlus(UserLevel.Pro), "Pro was not treated as above Plus.");
            True(!UserLevelHelper.IsAbovePlus(UserLevel.Go), "Go was treated as above Plus.");
            Equal("User Level: Pro", UiText.For(AppLanguage.English).FormatUserLevel(UserLevel.Pro));
            Equal("用户等级：Plus", UiText.For(AppLanguage.ChineseSimplified).FormatUserLevel(UserLevel.Plus));

            var plusSnapshot = new UsageSnapshot
            {
                userLevel = UserLevel.Plus,
                fiveHourLimitApplies = true,
                fiveHourRemainingPercent = 50,
                weeklyRemainingPercent = 80,
                fiveHourResetAt = new DateTime(2026, 9, 11, 12, 0, 0),
                weeklyResetAt = new DateTime(2026, 9, 17, 19, 35, 0)
            };
            True(TooltipFormatter.Build(plusSnapshot, TrayVisualState.Normal, false).StartsWith("5-Hour 50%", StringComparison.Ordinal),
                "Plus tooltip incorrectly suppressed its applicable 5-Hour row.");
            True(TooltipFormatter.BuildFiveHourMenuLine(plusSnapshot).StartsWith("5-Hour 50%", StringComparison.Ordinal),
                "Plus menu incorrectly replaced its applicable 5-Hour value with N/A.");
            plusSnapshot.userLevel = UserLevel.Pro;
            True(TooltipFormatter.Build(plusSnapshot, TrayVisualState.Normal, false).Contains("5-Hour 50%"),
                "Authoritative Pro 5-Hour data was discarded.");
            True(TooltipFormatter.BuildFiveHourMenuLine(plusSnapshot).Contains("5-Hour 50%"),
                "The Pro menu discarded authoritative 5-Hour data.");
            plusSnapshot.fiveHourLimitApplies = false;
            plusSnapshot.fiveHourRemainingPercent = null;
            plusSnapshot.fiveHourResetAt = null;
            True(!TooltipFormatter.Build(plusSnapshot, TrayVisualState.Normal, false).Contains("5-Hour"),
                "Confirmed Pro Weekly-only data did not suppress the 5-Hour row.");
            Equal("5-Hour N/A", TooltipFormatter.BuildFiveHourMenuLine(plusSnapshot));
            plusSnapshot.userLevel = UserLevel.Go;
            plusSnapshot.fiveHourLimitApplies = true;
            plusSnapshot.fiveHourRemainingPercent = 50;
            True(TooltipFormatter.Build(plusSnapshot, TrayVisualState.Normal, false).StartsWith("5-Hour 50%", StringComparison.Ordinal),
                "Go tooltip incorrectly suppressed its applicable 5-Hour row.");
        }

        private static void ProxyArguments()
        {
            Equal(string.Empty, ProxyArgumentBuilder.Build(new ProxySettings { mode = ProxyMode.System }));
            Equal("--no-proxy-server", ProxyArgumentBuilder.Build(new ProxySettings { mode = ProxyMode.Direct }));
            Equal("--proxy-server=http://127.0.0.1:7890", ProxyArgumentBuilder.Build(new ProxySettings { mode = ProxyMode.Http, host = "127.0.0.1", port = 7890 }));
            Equal("--proxy-server=socks5://127.0.0.1:1080", ProxyArgumentBuilder.Build(new ProxySettings { mode = ProxyMode.Socks5, host = "127.0.0.1", port = 1080 }));
        }

        private static void ProxyDialogLayoutAndDirectOption()
        {
            using (var form = new ProxySettingsForm(new ProxySettings { mode = ProxyMode.Direct, host = "127.0.0.1", port = 7890 }))
            {
                form.CreateControl();
                form.PerformLayout();
                Equal(FormStartPosition.CenterScreen, form.StartPosition);
                True(form.ClientSize.Width >= 620 && form.ClientSize.Height >= 340, "Proxy dialog is smaller than the enlarged layout.");
                ComboBox modes = null;
                Label modeLabel = null;
                foreach (Control control in form.Controls)
                {
                    modes = modes ?? control as ComboBox;
                    var label = control as Label;
                    if (label != null && label.Text == "Proxy mode:") modeLabel = label;
                }
                True(modes != null, "Proxy mode selector is missing.");
                True(modeLabel != null, "Proxy mode label is missing.");
                True(!modeLabel.Bounds.IntersectsWith(modes.Bounds), "Proxy mode label overlaps its selector.");
                Equal((int)ProxyMode.Direct, modes.SelectedIndex);
                Equal("Direct (no proxy)", modes.Items[(int)ProxyMode.Direct].ToString());
            }
        }

        private static void AboutDialogMetadataAndLayout()
        {
            var info = AboutInfoProvider.GetCurrent();
            Equal(AppConstants.Name, info.ProgramName);
            Equal(AppConstants.Version, info.Version);
            Equal("x64", info.Architecture);
            DateTimeOffset buildTime;
            True(DateTimeOffset.TryParse(info.BuildTimeUtc, out buildTime), "Embedded build time is missing or invalid.");
            True(!string.IsNullOrWhiteSpace(info.WebView2SdkVersion), "WebView2 SDK version is missing.");

            using (var form = new AboutForm())
            {
                form.CreateControl();
                form.PerformLayout();
                Equal(FormStartPosition.CenterScreen, form.StartPosition);
                True(form.DetailsText.Contains("Version: " + AppConstants.Version), "About dialog lacks the current version.");
                True(form.DetailsText.Contains("Architecture: x64"), "About dialog lacks the architecture.");
                True(form.DetailsText.Contains("Build time (UTC): "), "About dialog lacks build time.");
            }
        }

        private static void HelpDialogContentAndLayout()
        {
            using (var form = new HelpForm(AppThemeMode.Dark))
            {
                form.CreateControl();
                form.PerformLayout();
                Equal(FormStartPosition.CenterScreen, form.StartPosition);
                Equal(FormBorderStyle.FixedDialog, form.FormBorderStyle);
                Equal(AppConstants.Name + " Help", form.Text);
                True(form.HelpText.Contains("WebView2"), "Help dialog lacks WebView2 guidance.");
                True(form.HelpText.Contains("Codex CLI"), "Help dialog lacks Codex CLI guidance.");
                True(form.HelpText.Contains("codex login"), "Help dialog lacks CLI login guidance.");
                True(form.HelpText.Contains("Proxy settings apply only to this mode."), "Help dialog lacks WebView2 proxy guidance.");
                True(form.HelpText.Contains("This app never uses a reset in your account."), "Help dialog lacks the read-only reset statement.");
                True(form.HelpText.Contains("Security and Privacy"), "Help dialog lacks the security and privacy heading.");
                True(form.HelpText.Contains("does not send your login or account information"), "Help dialog lacks the no-account-upload statement.");
                True(form.HelpText.Contains("Passwords, cookies, and access tokens are not read or stored"), "Help dialog lacks credential-handling guidance.");
                True(form.HelpText.Contains("kept in memory, never saved"), "Help dialog lacks email-retention guidance.");
                True(form.HelpText.Contains("no telemetry or cloud backend"), "Help dialog lacks telemetry guidance.");
                True(form.HelpText.Contains(Environment.NewLine + Environment.NewLine + "WebView2"),
                    "Help dialog did not preserve the paragraph break before WebView2.");
                True(!form.HelpText.Replace(Environment.NewLine, string.Empty).Contains("\n"),
                    "Help dialog retained lone LF characters that the Windows Edit control collapses.");
                TextBox helpBox = null;
                foreach (Control control in form.Controls)
                {
                    var candidate = control as TextBox;
                    if (candidate != null && candidate.Multiline) helpBox = candidate;
                }
                True(helpBox != null && helpBox.Lines.Length >= 20,
                    "The rendered Help TextBox collapsed its multiline content.");
                True(form.ClientSize.Height >= 620, "Help dialog is too short for the privacy section.");
                Equal(AppThemeMode.Dark, form.ThemeMode);
                Equal(ThemeService.GetPalette(AppThemeMode.Dark).WindowBackground, form.BackColor);
            }
        }

        private static void ProxyExternalArgumentPriority()
        {
            bool detected;
            var clean = ProxyArgumentBuilder.SanitizeExternalArguments("--disable-gpu --proxy-server=http://bad:1 \"--lang=en US\"", out detected);
            True(detected, "External proxy was not detected.");
            True(!clean.Contains("proxy-server"), "External proxy was not removed.");
            True(clean.Contains("--disable-gpu"), "Unrelated argument was removed.");
        }

        private static void ProxyValidation()
        {
            Throws<ArgumentException>(() => ProxyArgumentBuilder.Validate(new ProxySettings { mode = ProxyMode.Http, host = "127.0.0.1 --flag", port = 7890 }));
            Throws<ArgumentOutOfRangeException>(() => ProxyArgumentBuilder.Validate(new ProxySettings { mode = ProxyMode.Socks5, host = "localhost", port = 70000 }));
        }

        private static void SettingsSerialization()
        {
            var source = AppSettings.CreateDefault();
            source.refreshIntervalMinutes = 30;
            source.setupComplete = true;
            source.loginRequired = false;
            source.themeMode = AppThemeMode.Dark;
            source.language = AppLanguage.ChineseTraditional;
            source.quotaIconMode = QuotaIconMode.SideBySide;
            source.usageSource = UsageSource.CodexCli;
            source.lastSuccessfulUsageSource = UsageSource.CodexCli;
            source.lastSuccessfulUsage = new UsageSnapshot { userLevel = UserLevel.Plus, fiveHourRemainingPercent = 71, weeklyRemainingPercent = 81, availableUsageResetCount = 2, lastSuccessfulFetchAt = new DateTime(2026, 9, 1, 10, 0, 0) };
            var serializer = new JavaScriptSerializer();
            var copy = serializer.Deserialize<AppSettings>(serializer.Serialize(source));
            Equal(30, copy.refreshIntervalMinutes);
            Equal(AppThemeMode.Dark, copy.themeMode);
            Equal(AppLanguage.ChineseTraditional, copy.language);
            Equal(QuotaIconMode.SideBySide, copy.quotaIconMode);
            Equal(UsageSource.CodexCli, copy.usageSource);
            Equal(UsageSource.CodexCli, copy.lastSuccessfulUsageSource);
            True(copy.CanRefreshUsage(), "Serialized configured state did not allow refresh.");
            Equal(71, copy.lastSuccessfulUsage.fiveHourRemainingPercent.Value);
            Equal(UserLevel.Plus, copy.lastSuccessfulUsage.userLevel);
            True(copy.lastSuccessfulUsage.IsFiveHourLimitApplicable, "A legacy numeric 5-hour snapshot lost applicability after serialization.");
            Equal(2, copy.lastSuccessfulUsage.availableUsageResetCount.Value);

            copy.lastSuccessfulUsage.fiveHourLimitApplies = false;
            copy.lastSuccessfulUsage.userLevel = (UserLevel)99;
            copy.lastSuccessfulUsage.fiveHourRemainingPercent = 71;
            copy.lastSuccessfulUsage.fiveHourResetAt = DateTime.Now;
            SettingsStore.Normalize(copy);
            True(!copy.lastSuccessfulUsage.IsFiveHourLimitApplicable, "A weekly-only snapshot lost its explicit applicability state.");
            True(!copy.lastSuccessfulUsage.fiveHourRemainingPercent.HasValue && !copy.lastSuccessfulUsage.fiveHourResetAt.HasValue,
                "Weekly-only settings retained stale 5-hour values.");
            Equal(UserLevel.Unknown, copy.lastSuccessfulUsage.userLevel);
        }

        private static void QuotaIconModeSettingsMigration()
        {
            var defaults = AppSettings.CreateDefault();
            Equal(QuotaIconMode.Both, defaults.quotaIconMode);

            var legacy = new AppSettings
            {
                settingsSchemaVersion = 4,
                refreshIntervalMinutes = 15,
                proxyMode = ProxyMode.System,
                proxyHost = "127.0.0.1",
                proxyPort = 7890,
                iconStyle = "D",
                quotaIconMode = QuotaIconMode.FiveHourOnly,
                themeMode = AppThemeMode.System,
                language = AppLanguage.English
            };
            SettingsStore.Normalize(legacy);
            Equal(SettingsStore.CurrentSettingsSchemaVersion, legacy.settingsSchemaVersion);
            Equal(QuotaIconMode.Both, legacy.quotaIconMode);
            Equal("D", legacy.iconStyle);

            legacy.quotaIconMode = (QuotaIconMode)99;
            SettingsStore.Normalize(legacy);
            Equal(QuotaIconMode.Both, legacy.quotaIconMode);
            Equal(QuotaIconMode.Both, BatteryTrayIconRenderer.NormalizeIconMode((QuotaIconMode)99));

            legacy.settingsSchemaVersion = SettingsStore.CurrentSettingsSchemaVersion;
            legacy.quotaIconMode = QuotaIconMode.SideBySide;
            SettingsStore.Normalize(legacy);
            Equal(QuotaIconMode.SideBySide, legacy.quotaIconMode);
        }

        private static void FirstRunRefreshPolicy()
        {
            var settings = AppSettings.CreateDefault();
            Equal(SettingsStore.CurrentSettingsSchemaVersion, settings.settingsSchemaVersion);
            True(!settings.setupComplete, "First-run settings were marked configured.");
            True(settings.loginRequired, "First-run settings did not request login.");
            True(!settings.CanRefreshUsage(), "First-run settings allowed background refresh.");
            True(!settings.CanManuallyRefreshUsage(), "First-run WebView2 settings allowed manual refresh.");

            settings.usageSource = UsageSource.CodexCli;
            True(settings.CanManuallyRefreshUsage(), "An explicitly selected Codex CLI source did not allow a manual first fetch.");
            True(!settings.CanRefreshUsage(), "An unverified Codex CLI source allowed background refresh.");

            settings.setupComplete = true;
            settings.loginRequired = false;
            True(settings.CanRefreshUsage(), "Successful setup did not enable refresh.");
            settings.loginRequired = true;
            True(!settings.CanRefreshUsage(), "Login-required state still allowed refresh.");
        }

        private static void UsageSourceSettingsMigration()
        {
            var versionTwo = new AppSettings
            {
                settingsSchemaVersion = 2,
                setupComplete = true,
                refreshIntervalMinutes = 15,
                proxyMode = ProxyMode.System,
                themeMode = AppThemeMode.Dark,
                usageSource = (UsageSource)99,
                lastSuccessfulUsageSource = (UsageSource)99,
                loginRequired = false,
                lastSuccessfulUsage = new UsageSnapshot { fiveHourRemainingPercent = 71, weeklyRemainingPercent = 81 }
            };
            SettingsStore.Normalize(versionTwo);
            Equal(SettingsStore.CurrentSettingsSchemaVersion, versionTwo.settingsSchemaVersion);
            Equal(UsageSource.WebView2, versionTwo.usageSource);
            Equal(UsageSource.WebView2, versionTwo.lastSuccessfulUsageSource);
            True(versionTwo.CanRefreshUsage(), "Usage-source migration changed the configured WebView2 state.");

            versionTwo.usageSource = (UsageSource)99;
            SettingsStore.Normalize(versionTwo);
            Equal(UsageSource.WebView2, versionTwo.usageSource);
        }

        private static void UsageSourceSwitchRefreshPolicy()
        {
            True(TrayApplicationContext.ShouldValidateSourceAfterSwitch(UsageSource.WebView2), "Switching to WebView2 did not request immediate validation.");
            True(TrayApplicationContext.ShouldValidateSourceAfterSwitch(UsageSource.CodexCli), "Switching to Codex CLI did not request immediate validation.");
            True(!TrayApplicationContext.ShouldValidateSourceAfterSwitch((UsageSource)99), "An unknown usage source requested validation.");
        }

        private static void UsageSourceMenuVisibilityPolicy()
        {
            True(TrayApplicationContext.ShouldShowWebViewOnlyMenuItems(UsageSource.WebView2), "WebView2 mode hid its connection/session menu items.");
            True(!TrayApplicationContext.ShouldShowWebViewOnlyMenuItems(UsageSource.CodexCli), "Codex CLI mode exposed WebView2-only menu items.");
            True(!TrayApplicationContext.ShouldShowWebViewOnlyMenuItems((UsageSource)99), "An unknown source exposed WebView2-only menu items.");
        }

        private static void UsageResetMenuState()
        {
            var snapshot = new UsageSnapshot { userLevel = UserLevel.Pro, availableUsageResetCount = 3 };
            Equal("Usage resets: 3", UsageResetFormatter.FormatMenu(
                TrayApplicationContext.SelectAvailableUsageResetCount(snapshot, false, UsageSource.CodexCli, UsageSource.CodexCli)));
            Equal("Usage resets: 0", UsageResetFormatter.FormatMenu(0));
            Equal("Usage resets: Unknown", UsageResetFormatter.FormatMenu(
                TrayApplicationContext.SelectAvailableUsageResetCount(snapshot, false, UsageSource.CodexCli, UsageSource.WebView2)));
            Equal("Usage resets: Unknown", UsageResetFormatter.FormatMenu(
                TrayApplicationContext.SelectAvailableUsageResetCount(snapshot, true, UsageSource.CodexCli, UsageSource.CodexCli)));
            Equal(UserLevel.Pro, TrayApplicationContext.SelectUserLevel(
                snapshot, false, UsageSource.CodexCli, UsageSource.CodexCli));
            Equal(UserLevel.Unknown, TrayApplicationContext.SelectUserLevel(
                snapshot, false, UsageSource.CodexCli, UsageSource.WebView2));
            Equal(UserLevel.Unknown, TrayApplicationContext.SelectUserLevel(
                snapshot, true, UsageSource.CodexCli, UsageSource.CodexCli));
        }

        private static void CodexCliRateLimitsParsing()
        {
            var serializer = new JavaScriptSerializer();
            var result = serializer.DeserializeObject(
                "{\"rateLimits\":{\"limitId\":\"codex\",\"primary\":{\"usedPercent\":20,\"windowDurationMins\":300,\"resetsAt\":1788300000},\"secondary\":{\"usedPercent\":38,\"windowDurationMins\":10080,\"resetsAt\":1788900000}},\"rateLimitResetCredits\":{\"availableCount\":2,\"credits\":[{\"status\":\"available\"}]}}") as IDictionary<string, object>;
            UsageSnapshot snapshot;
            string error;
            var fetchedAt = new DateTime(2026, 9, 2, 16, 0, 0, DateTimeKind.Local);
            True(CodexRateLimitsParser.TryParse(result, fetchedAt, out snapshot, out error), error);
            Equal(80, snapshot.fiveHourRemainingPercent.Value);
            Equal(62, snapshot.weeklyRemainingPercent.Value);
            Equal(DateTimeKind.Utc, snapshot.fiveHourResetAt.Value.Kind);
            Equal(DateTimeKind.Utc, snapshot.weeklyResetAt.Value.Kind);
            Equal(2, snapshot.availableUsageResetCount.Value);
            Equal(fetchedAt, snapshot.lastSuccessfulFetchAt);
        }

        private static void CodexCliWeeklyOnlyPlanParsing()
        {
            var serializer = new JavaScriptSerializer();
            var result = serializer.DeserializeObject(
                "{\"rateLimits\":{\"limitId\":\"codex\",\"primary\":{\"usedPercent\":0,\"windowDurationMins\":10080,\"resetsAt\":1789644947},\"secondary\":null,\"planType\":\"prolite\"},\"rateLimitResetCredits\":{\"availableCount\":0,\"credits\":[]}}") as IDictionary<string, object>;
            UsageSnapshot snapshot;
            string error;
            True(CodexRateLimitsParser.TryParse(result, DateTime.Now, out snapshot, out error), error);
            True(!snapshot.IsFiveHourLimitApplicable, "The CLI weekly-only plan synthesized a 5-hour window.");
            True(!snapshot.fiveHourRemainingPercent.HasValue && !snapshot.fiveHourResetAt.HasValue,
                "The CLI weekly-only plan retained 5-hour values.");
            Equal(100, snapshot.weeklyRemainingPercent.Value);
            Equal(0, snapshot.availableUsageResetCount.Value);
            Equal(UserLevel.Pro, snapshot.userLevel);
        }

        private static void CodexCliRateLimitsFallbackAndValidation()
        {
            var serializer = new JavaScriptSerializer();
            var fallback = serializer.DeserializeObject(
                "{\"rateLimitsByLimitId\":{\"codex_other\":{\"primary\":{\"usedPercent\":99,\"windowDurationMins\":60}},\"codex\":{\"primary\":{\"usedPercent\":29.4,\"windowDurationMins\":300},\"secondary\":{\"usedPercent\":19.6,\"windowDurationMins\":10080}}}}") as IDictionary<string, object>;
            UsageSnapshot snapshot;
            string error;
            True(CodexRateLimitsParser.TryParse(fallback, DateTime.Now, out snapshot, out error), error);
            Equal(71, snapshot.fiveHourRemainingPercent.Value);
            Equal(80, snapshot.weeklyRemainingPercent.Value);
            True(!snapshot.availableUsageResetCount.HasValue, "A missing CLI reset-credit summary was treated as zero.");

            var invalidResetCount = serializer.DeserializeObject(
                "{\"rateLimitResetCredits\":{\"availableCount\":-1}}") as IDictionary<string, object>;
            True(!CodexRateLimitsParser.ParseAvailableUsageResetCount(invalidResetCount).HasValue,
                "A negative CLI reset-credit count was accepted.");

            var missingWeekly = serializer.DeserializeObject(
                "{\"rateLimits\":{\"primary\":{\"usedPercent\":20,\"windowDurationMins\":300}}}") as IDictionary<string, object>;
            True(!CodexRateLimitsParser.TryParse(missingWeekly, DateTime.Now, out snapshot, out error), "Missing weekly CLI window was accepted.");
            True(error.IndexOf("weekly", StringComparison.OrdinalIgnoreCase) >= 0, "Missing weekly error was not specific.");
        }

        private static void CodexCliProcessLaunchModes()
        {
            var executable = CodexProcessStartInfoFactory.Create(@"C:\Tools\codex.exe");
            Equal(@"C:\Tools\codex.exe", executable.FileName);
            Equal("app-server", executable.Arguments);
            True(!executable.UseShellExecute && executable.CreateNoWindow, "Executable launch is not hidden/direct.");
            True(executable.RedirectStandardInput && executable.RedirectStandardOutput && executable.RedirectStandardError, "Executable streams are not redirected.");

            var script = CodexProcessStartInfoFactory.Create(@"C:\Program Files\Codex CLI\codex.cmd");
            True(script.FileName.EndsWith("cmd.exe", StringComparison.OrdinalIgnoreCase), "Command script does not use cmd.exe.");
            True(script.Arguments.Contains("\"C:\\Program Files\\Codex CLI\\codex.cmd\" app-server"), "Command script path was not quoted.");
            True(script.Arguments.StartsWith("/d /s /c", StringComparison.Ordinal), "Safe cmd.exe switches are missing.");
        }

        private static void CodexCliLocatorPathSearch()
        {
            var directory = Path.Combine(Path.GetTempPath(), "CodexUsageTrayLite-Locator-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            try
            {
                var expected = Path.Combine(directory, "codex.cmd");
                File.WriteAllText(expected, string.Empty);
                Equal(Path.GetFullPath(expected), CodexCliExecutableLocator.FindOnPath(directory));
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        private static void CodexCliGracefulProcessExitTelemetry()
        {
            using (var process = StartTelemetryTestProcess(
                Environment.GetEnvironmentVariable("COMSPEC") ?? Path.Combine(Environment.SystemDirectory, "cmd.exe"),
                "/d /s /c \"set /p value=\""))
            {
                var result = CodexProcessStopper.Stop(process, 1000);
                Equal(CodexProcessStopper.GracefulAfterStdinClose, result.ExitType);
                True(result.StandardInputClosed, "Graceful process stdin was not closed.");
                True(!result.KillAttempted, "Graceful process unexpectedly used Kill.");
                True(result.ExitCode.HasValue, "Graceful process exit code was not captured.");
                var log = result.ToLogDetails(true, true, 0);
                True(log.Contains("killAttempted=false") && log.Contains("jobCloseAction=no-active-processes"), "Graceful telemetry fields are incomplete.");
            }
        }

        private static void CodexCliForcedProcessExitTelemetry()
        {
            using (var process = StartTelemetryTestProcess(
                Path.Combine(Environment.SystemDirectory, "PING.EXE"),
                "-n 30 127.0.0.1"))
            {
                var result = CodexProcessStopper.Stop(process, 50);
                Equal(CodexProcessStopper.ForcedKill, result.ExitType);
                True(result.KillAttempted && result.KillCallReturned, "Hung-process fallback Kill was not recorded.");
                True(result.ExitCode.HasValue, "Forced process exit code was not captured.");
                var log = result.ToLogDetails(true, true, 1);
                True(log.Contains("exitType=forced-kill") && log.Contains("jobCloseAction=kill-on-close-requested"), "Forced telemetry fields are incomplete.");
            }
        }

        private static void CodexCliJobDrainSeverity()
        {
            var graceful = new CodexProcessStopResult
            {
                ExitType = CodexProcessStopper.GracefulAfterStdinClose,
                ExitCode = 0,
                StandardInputClosed = true,
                JobDrainWaitMilliseconds = 50
            };
            True(CodexCliFetchSession.ShouldWaitForJobDrain(graceful), "Graceful CLI exit did not request a child-process drain wait.");
            Equal(LogEventLevel.Info, CodexCliFetchSession.ProcessExitLogLevel(graceful, true, 0));
            Equal(LogEventLevel.Warning, CodexCliFetchSession.ProcessExitLogLevel(graceful, true, 1));
            True(graceful.ToLogDetails(true, true, 0).Contains("jobDrainWaitMs=50"), "CLI drain wait telemetry is missing.");

            var forced = new CodexProcessStopResult { ExitType = CodexProcessStopper.ForcedKill };
            True(!CodexCliFetchSession.ShouldWaitForJobDrain(forced), "Forced CLI exit unexpectedly waits for natural drain.");
            Equal(LogEventLevel.Warning, CodexCliFetchSession.ProcessExitLogLevel(forced, true, 0));

            var failed = new CodexProcessStopResult { ExitType = CodexProcessStopper.ForcedKillTimeout };
            Equal(LogEventLevel.Error, CodexCliFetchSession.ProcessExitLogLevel(failed, true, 1));
        }

        private static void WebViewProcessTelemetryFormatting()
        {
            var returned = WebViewProcessTelemetry.FormatControllerCloseDetails("fetch", 1234, "returned", null);
            True(returned.Contains("browserPid=1234"), "WebView browser PID is missing.");
            True(returned.Contains("api=CoreWebView2Controller.Close"), "WebView close API is missing.");
            True(returned.Contains("processKill=false"), "WebView no-Kill state is missing.");
            True(returned.Contains("browserExit=asynchronous"), "WebView asynchronous exit state is missing.");

            var failed = WebViewProcessTelemetry.FormatControllerCloseDetails("login", 0, "exception", "InvalidOperationException");
            True(failed.Contains("scope=login") && failed.Contains("browserPid=unavailable") && failed.Contains("exception=InvalidOperationException"), "WebView failed-close telemetry is incomplete.");
        }

        private static Process StartTelemetryTestProcess(string executable, string arguments)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo(executable, arguments)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            process.Start();
            return process;
        }

        private static void LegacySettingsRefreshMigration()
        {
            var legacyConfigured = new AppSettings
            {
                settingsSchemaVersion = 0,
                refreshIntervalMinutes = 15,
                proxyMode = ProxyMode.System,
                loginRequired = false,
                lastSuccessfulUsage = new UsageSnapshot { fiveHourRemainingPercent = 71, weeklyRemainingPercent = 81 }
            };
            SettingsStore.Normalize(legacyConfigured);
            Equal(SettingsStore.CurrentSettingsSchemaVersion, legacyConfigured.settingsSchemaVersion);
            True(legacyConfigured.CanRefreshUsage(), "A successful legacy installation was not migrated as configured.");

            var legacyLoggedOut = new AppSettings
            {
                settingsSchemaVersion = 0,
                refreshIntervalMinutes = 15,
                proxyMode = ProxyMode.System,
                loginRequired = true,
                lastSuccessfulUsage = new UsageSnapshot { fiveHourRemainingPercent = 71, weeklyRemainingPercent = 81 }
            };
            SettingsStore.Normalize(legacyLoggedOut);
            True(!legacyLoggedOut.CanRefreshUsage(), "A logged-out legacy installation incorrectly allowed refresh.");

            var legacyNeverConfigured = new AppSettings
            {
                settingsSchemaVersion = 0,
                refreshIntervalMinutes = 15,
                proxyMode = ProxyMode.System,
                loginRequired = false,
                lastSuccessfulUsage = null
            };
            SettingsStore.Normalize(legacyNeverConfigured);
            True(legacyNeverConfigured.loginRequired, "An unconfigured legacy installation was not migrated to login-required state.");
            True(!legacyNeverConfigured.CanRefreshUsage(), "An unconfigured legacy installation incorrectly allowed refresh.");
        }

        private static void ThemeSettingsAndResolution()
        {
            var defaults = AppSettings.CreateDefault();
            Equal(AppThemeMode.System, defaults.themeMode);
            Equal(AppThemeMode.Dark, ThemeService.ResolveEffectiveMode(AppThemeMode.System, true));
            Equal(AppThemeMode.Light, ThemeService.ResolveEffectiveMode(AppThemeMode.System, false));
            Equal(AppThemeMode.Dark, ThemeService.ResolveEffectiveMode(AppThemeMode.Dark, false));
            Equal(AppThemeMode.System, ThemeService.Normalize((AppThemeMode)99));
            Equal(CoreWebView2PreferredColorScheme.Auto, ThemeService.ToWebViewColorScheme(AppThemeMode.System));
            Equal(CoreWebView2PreferredColorScheme.Light, ThemeService.ToWebViewColorScheme(AppThemeMode.Light));
            Equal(CoreWebView2PreferredColorScheme.Dark, ThemeService.ToWebViewColorScheme(AppThemeMode.Dark));

            var versionOne = new AppSettings
            {
                settingsSchemaVersion = 1,
                setupComplete = true,
                refreshIntervalMinutes = 15,
                proxyMode = ProxyMode.System,
                proxyHost = "127.0.0.1",
                proxyPort = 7890,
                themeMode = (AppThemeMode)99,
                loginRequired = false,
                lastSuccessfulUsage = new UsageSnapshot { fiveHourRemainingPercent = 71, weeklyRemainingPercent = 81 }
            };
            SettingsStore.Normalize(versionOne);
            Equal(SettingsStore.CurrentSettingsSchemaVersion, versionOne.settingsSchemaVersion);
            Equal(AppThemeMode.System, versionOne.themeMode);
            True(versionOne.setupComplete, "Theme migration changed the configured refresh state.");
        }

        private static void LanguageDetectionAndMigration()
        {
            Equal(5, Enum.GetValues(typeof(AppLanguage)).Length);
            Equal(AppLanguage.English, LanguageService.DetectInitialLanguage(new CultureInfo("en-US")));
            Equal(AppLanguage.Japanese, LanguageService.DetectInitialLanguage(new CultureInfo("ja-JP")));
            Equal(AppLanguage.Korean, LanguageService.DetectInitialLanguage(new CultureInfo("ko-KR")));
            Equal(AppLanguage.ChineseSimplified, LanguageService.DetectInitialLanguage(new CultureInfo("zh-CN")));
            Equal(AppLanguage.ChineseSimplified, LanguageService.DetectInitialLanguage(new CultureInfo("zh-SG")));
            Equal(AppLanguage.ChineseTraditional, LanguageService.DetectInitialLanguage(new CultureInfo("zh-TW")));
            Equal(AppLanguage.ChineseTraditional, LanguageService.DetectInitialLanguage(new CultureInfo("zh-HK")));
            Equal(AppLanguage.English, LanguageService.Normalize((AppLanguage)99));

            var versionThree = new AppSettings
            {
                settingsSchemaVersion = 3,
                language = AppLanguage.ChineseTraditional,
                refreshIntervalMinutes = 15,
                proxyMode = ProxyMode.System,
                proxyHost = "127.0.0.1",
                proxyPort = 7890,
                themeMode = AppThemeMode.System
            };
            SettingsStore.Normalize(versionThree);
            Equal(AppLanguage.English, versionThree.language);
            Equal(SettingsStore.CurrentSettingsSchemaVersion, versionThree.settingsSchemaVersion);

            versionThree.language = (AppLanguage)99;
            SettingsStore.Normalize(versionThree);
            Equal(AppLanguage.English, versionThree.language);

            var versionFour = new AppSettings
            {
                settingsSchemaVersion = SettingsStore.CurrentSettingsSchemaVersion,
                language = AppLanguage.Japanese,
                refreshIntervalMinutes = 15,
                proxyMode = ProxyMode.System,
                proxyHost = "127.0.0.1",
                proxyPort = 7890,
                themeMode = AppThemeMode.System
            };
            SettingsStore.Normalize(versionFour);
            Equal(AppLanguage.Japanese, versionFour.language);
            versionFour.language = AppLanguage.Korean;
            SettingsStore.Normalize(versionFour);
            Equal(AppLanguage.Korean, versionFour.language);
        }

        private static void LocalizedTextContracts()
        {
            var english = UiText.For(AppLanguage.English);
            var simplified = UiText.For(AppLanguage.ChineseSimplified);
            var traditional = UiText.For(AppLanguage.ChineseTraditional);
            var japanese = UiText.For(AppLanguage.Japanese);
            var korean = UiText.For(AppLanguage.Korean);
            Equal("Language", english.LanguageMenu);
            Equal("语言", simplified.LanguageMenu);
            Equal("語言", traditional.LanguageMenu);
            Equal("言語", japanese.LanguageMenu);
            Equal("언어", korean.LanguageMenu);
            Equal("Icon style", english.IconStyleMenu);
            Equal("5-Hour Only", english.IconStyleFiveHourOnly);
            Equal("Weekly Only", english.IconStyleWeeklyOnly);
            Equal("Both (Default)", english.IconStyleBoth);
            Equal("Side by Side", english.IconStyleSideBySide);
            Equal("图标样式", simplified.IconStyleMenu);
            Equal("两者（默认）", simplified.IconStyleBoth);
            Equal("并排双条", simplified.IconStyleSideBySide);
            Equal("圖示樣式", traditional.IconStyleMenu);
            Equal("兩者（預設）", traditional.IconStyleBoth);
            Equal("並排雙條", traditional.IconStyleSideBySide);
            Equal("アイコン表示", japanese.IconStyleMenu);
            Equal("両方（既定）", japanese.IconStyleBoth);
            Equal("左右並列", japanese.IconStyleSideBySide);
            Equal("아이콘 표시", korean.IconStyleMenu);
            Equal("둘 다(기본값)", korean.IconStyleBoth);
            Equal("나란히 표시", korean.IconStyleSideBySide);
            Equal("邮箱账号：user@example.com", simplified.FormatAccountEmail("user@example.com"));
            Equal("用量重置：2 个", simplified.FormatUsageResets(2));
            Equal("用量重置：未知", simplified.FormatUsageResets(null));
            Equal("上次更新：12:44 (WebView2)", simplified.FormatLastUpdated(
                new DateTime(2026, 9, 9, 12, 44, 0, DateTimeKind.Local), UsageSource.WebView2));
            Equal("電子郵件帳號：user@example.com", traditional.FormatAccountEmail("user@example.com"));
            Equal("用量重設：2 個", traditional.FormatUsageResets(2));
            Equal("1 minute", english.FormatMinutes(1));
            Equal("1 分钟", simplified.FormatMinutes(1));
            Equal("1 分鐘", traditional.FormatMinutes(1));
            Equal("1 分", japanese.FormatMinutes(1));
            Equal("1분", korean.FormatMinutes(1));
            Equal(3, english.HelpBody.Split(new[] { "in your account" }, StringSplitOptions.None).Length);
            True(simplified.HelpBody.Contains("本程序绝不会使用您账号中的用量重置"), "Simplified Help lacks the account-scoped reset statement.");
            True(traditional.HelpBody.Contains("本程式絕不會使用您帳號中的用量重設"), "Traditional Help lacks the account-scoped reset statement.");
            True(japanese.HelpBody.Contains("セキュリティとプライバシー"), "Japanese Help was not selected.");
            True(korean.HelpBody.Contains("보안 및 개인정보 보호"), "Korean Help was not selected.");
            Equal("English", simplified.LanguageEnglish);
            Equal("简体中文", simplified.LanguageChineseSimplified);
            Equal("繁體中文", simplified.LanguageChineseTraditional);
            Equal("日本語", simplified.LanguageJapanese);
            Equal("한국어", simplified.LanguageKorean);
        }

        private static void ResxLocalizationResources()
        {
            var applicationAssembly = typeof(UiText).Assembly;
            True(Array.IndexOf(applicationAssembly.GetManifestResourceNames(), UiText.ResourceBaseName + ".resources") >= 0,
                "The neutral English .resx resource is not embedded in the application assembly.");
            Equal("en-US", LanguageService.GetCulture(AppLanguage.English).Name);
            Equal("zh-CN", LanguageService.GetCulture(AppLanguage.ChineseSimplified).Name);
            Equal("zh-TW", LanguageService.GetCulture(AppLanguage.ChineseTraditional).Name);
            Equal("ja-JP", LanguageService.GetCulture(AppLanguage.Japanese).Name);
            Equal("ko-KR", LanguageService.GetCulture(AppLanguage.Korean).Name);

            var assemblyDirectory = Path.GetDirectoryName(applicationAssembly.Location);
            foreach (var cultureName in new[] { "ja-JP", "ko-KR", "zh-CN", "zh-TW" })
            {
                var satellitePath = Path.Combine(assemblyDirectory, cultureName, AppConstants.AssemblyName + ".resources.dll");
                True(File.Exists(satellitePath), "Missing localization satellite: " + satellitePath);
                var satellite = System.Reflection.Assembly.LoadFrom(satellitePath);
                True(Array.IndexOf(satellite.GetManifestResourceNames(), UiText.ResourceBaseName + "." + cultureName + ".resources") >= 0,
                    "The " + cultureName + " satellite does not contain the expected compiled .resx resource.");
            }

            var manager = new System.Resources.ResourceManager(UiText.ResourceBaseName, applicationAssembly);
            try
            {
                var englishKeys = ResourceKeys(manager.GetResourceSet(LanguageService.GetCulture(AppLanguage.English), true, true));
                True(englishKeys.Count >= 80, "The neutral UI resource set is unexpectedly incomplete.");
                foreach (var language in new[]
                {
                    AppLanguage.ChineseSimplified,
                    AppLanguage.ChineseTraditional,
                    AppLanguage.Japanese,
                    AppLanguage.Korean
                })
                {
                    var localizedKeys = ResourceKeys(manager.GetResourceSet(LanguageService.GetCulture(language), true, true));
                    True(englishKeys.SetEquals(localizedKeys), language + " resource keys do not match the neutral English resource keys.");
                }
            }
            finally
            {
                manager.ReleaseAllResources();
            }
        }

        private static HashSet<string> ResourceKeys(System.Resources.ResourceSet resourceSet)
        {
            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (System.Collections.DictionaryEntry entry in resourceSet) keys.Add((string)entry.Key);
            return keys;
        }

        private static void LocalizedDialogs()
        {
            using (var proxy = new ProxySettingsForm(
                new ProxySettings { mode = ProxyMode.Direct, host = "127.0.0.1", port = 7890 },
                AppThemeMode.Light,
                AppLanguage.ChineseSimplified))
            {
                Equal("代理设置", proxy.Text);
                ComboBox modes = null;
                var labels = new List<string>();
                var buttons = new List<string>();
                foreach (Control control in proxy.Controls)
                {
                    modes = modes ?? control as ComboBox;
                    if (control is Label) labels.Add(control.Text);
                    if (control is Button) buttons.Add(control.Text);
                }
                True(labels.Contains("代理模式：") && labels.Contains("代理主机：") && labels.Contains("代理端口："),
                    "Simplified Proxy labels are incomplete.");
                Equal("直连（不使用代理）", modes.Items[(int)ProxyMode.Direct].ToString());
                True(buttons.Contains("保存") && buttons.Contains("取消"), "Simplified Proxy buttons are incomplete.");
            }

            using (var help = new HelpForm(AppThemeMode.Light, AppLanguage.ChineseSimplified))
            using (var traditionalHelp = new HelpForm(AppThemeMode.Light, AppLanguage.ChineseTraditional))
            using (var japaneseHelp = new HelpForm(AppThemeMode.Light, AppLanguage.Japanese))
            using (var koreanHelp = new HelpForm(AppThemeMode.Light, AppLanguage.Korean))
            using (var about = new AboutForm(AppThemeMode.Light, AppLanguage.ChineseSimplified))
            using (var japaneseAbout = new AboutForm(AppThemeMode.Light, AppLanguage.Japanese))
            using (var koreanAbout = new AboutForm(AppThemeMode.Light, AppLanguage.Korean))
            using (var login = new LoginBrowserForm(new ProxySettings { mode = ProxyMode.System }, AppThemeMode.Light, AppLanguage.ChineseSimplified))
            {
                Equal(AppConstants.Name + " 帮助", help.Text);
                True(help.HelpText.Contains("安全与隐私"), "Simplified Help was not selected.");
                Equal(AppConstants.Name + " 說明", traditionalHelp.Text);
                True(traditionalHelp.HelpText.Contains("安全性與隱私權"), "Traditional Help was not selected.");
                Equal(AppConstants.Name + " ヘルプ", japaneseHelp.Text);
                Equal(AppConstants.Name + " 도움말", koreanHelp.Text);
                Equal("关于 " + AppConstants.Name, about.Text);
                True(about.DetailsText.Contains("版本：" + AppConstants.Version), "Simplified About details are missing.");
                Equal(AppConstants.Name + " のバージョン情報", japaneseAbout.Text);
                True(japaneseAbout.DetailsText.Contains("バージョン：" + AppConstants.Version), "Japanese About details are missing.");
                Equal(AppConstants.Name + " 정보", koreanAbout.Text);
                True(koreanAbout.DetailsText.Contains("버전: " + AppConstants.Version), "Korean About details are missing.");
                Equal(AppConstants.Name + " - 登录 / 用量", login.Text);
                login.ApplyLanguage(AppLanguage.ChineseTraditional);
                Equal(AppConstants.Name + " - 登入 / 用量", login.Text);
                StatusStrip strip = null;
                foreach (Control control in login.Controls) strip = strip ?? control as StatusStrip;
                True(strip != null && strip.Items.Count == 1 && strip.Items[0].Text == "正在開啟 ChatGPT…",
                    "The open login window did not update its status language.");
            }
        }

        private static void ApplicationIconResourcesAndExecutable()
        {
            const string expectedSizes = "16,20,24,32,40,48,64,96,128,256";
            Equal(expectedSizes, string.Join(",", ApplicationIconService.ReadEmbeddedSizes(false)));
            Equal(expectedSizes, string.Join(",", ApplicationIconService.ReadEmbeddedSizes(true)));

            using (var light = ApplicationIconService.Create(false))
            using (var dark = ApplicationIconService.Create(true))
            {
                True(light.Icon != null && dark.Icon != null, "An embedded application icon could not be loaded.");
                foreach (var size in new[] { 16, 20, 24, 32, 40, 48, 64, 96, 128 })
                {
                    using (var lightSize = new Icon(light.Icon, size, size))
                    using (var darkSize = new Icon(dark.Icon, size, size))
                    {
                        Equal(size, lightSize.Width);
                        Equal(size, lightSize.Height);
                        Equal(size, darkSize.Width);
                        Equal(size, darkSize.Height);
                        using (var lightBitmap = lightSize.ToBitmap())
                        using (var darkBitmap = darkSize.ToBitmap())
                        {
                            Equal(0, lightBitmap.GetPixel(0, 0).A);
                            Equal(0, darkBitmap.GetPixel(0, 0).A);
                        }
                    }
                }
                True(IconTopBrightness(light.Icon) > IconTopBrightness(dark.Icon) + 350,
                    "The Light and Dark application icons do not have distinct system-theme backgrounds.");
            }

            using (var executableIcon = Icon.ExtractAssociatedIcon(typeof(AppConstants).Assembly.Location))
            {
                True(executableIcon != null, "The application executable has no associated icon.");
                True(IconTopBrightness(executableIcon) > 600, "The executable's default icon is not the Light variant.");
            }

            var extractedHandles = new IntPtr[1];
            var extractedIds = new uint[1];
            var extractedCount = PrivateExtractIcons(
                typeof(AppConstants).Assembly.Location,
                0,
                256,
                256,
                extractedHandles,
                extractedIds,
                1,
                0);
            True(extractedCount == 1 && extractedHandles[0] != IntPtr.Zero,
                "Windows could not extract the executable's 256 px application icon.");
            try
            {
                using (var extractedIcon = (Icon)Icon.FromHandle(extractedHandles[0]).Clone())
                using (var extractedBitmap = extractedIcon.ToBitmap())
                {
                    Equal(256, extractedBitmap.Width);
                    Equal(256, extractedBitmap.Height);
                    var bounds = AlphaBounds(extractedBitmap);
                    True(bounds.Width >= 220 && bounds.Height >= 220,
                        "The executable's 256 px icon does not use the expected canvas: " + bounds + ".");
                    True(IconTopBrightness(extractedIcon) > 600,
                        "The executable's 256 px default icon is not the Light variant.");
                }
            }
            finally
            {
                DestroyIcon(extractedHandles[0]);
            }
        }

        private static void ApplicationIconWindowThemeAndLifecycle()
        {
            True(ApplicationIconService.IsSystemAppearanceMessage(0x001A), "WM_SETTINGCHANGE does not refresh application icons.");
            True(ApplicationIconService.IsSystemAppearanceMessage(0x031A), "WM_THEMECHANGED does not refresh application icons.");
            True(ApplicationIconService.IsSystemAppearanceMessage(0x02E0), "WM_DPICHANGED does not refresh application icons.");
            True(!ApplicationIconService.IsSystemAppearanceMessage(0x000F), "WM_PAINT was treated as a system-appearance change.");

            using (var proxy = new ProxySettingsForm(new ProxySettings { mode = ProxyMode.System }, AppThemeMode.Light))
            using (var help = new HelpForm(AppThemeMode.Light))
            using (var about = new AboutForm(AppThemeMode.Light))
            using (var login = new LoginBrowserForm(new ProxySettings { mode = ProxyMode.System }, AppThemeMode.Light))
            {
                foreach (var form in new ThemedForm[] { proxy, help, about, login })
                {
                    form.CreateControl();
                    True(form.Icon != null, form.GetType().Name + " has no title-bar icon.");
                    Equal((bool?)false, form.UsesDarkApplicationIcon);
                    form.ApplyAppTheme(AppThemeMode.Dark);
                    Equal((bool?)true, form.UsesDarkApplicationIcon);
                    form.ApplyAppTheme(AppThemeMode.Light);
                    Equal((bool?)false, form.UsesDarkApplicationIcon);
                }
                var lightHash = ApplyAndHashApplicationIcon(help, false);
                var darkHash = ApplyAndHashApplicationIcon(help, true);
                True(lightHash != darkHash, "Changing the Windows icon variant did not change the visible Form icon.");

                help.ApplyAppTheme(AppThemeMode.System);
                Equal((bool?)ThemeService.IsSystemDarkMode(), help.UsesDarkApplicationIcon);
            }

            using (var warmup = new HelpForm())
            {
                warmup.CreateControl();
                warmup.ApplyApplicationIcon(false, true);
                warmup.ApplyApplicationIcon(true, true);
            }
            var processHandle = Process.GetCurrentProcess().Handle;
            var gdiBefore = GetGuiResources(processHandle, 0);
            var userBefore = GetGuiResources(processHandle, 1);
            for (var iteration = 0; iteration < 25; iteration++)
            {
                using (var form = new HelpForm())
                {
                    form.CreateControl();
                    for (var switchIndex = 0; switchIndex < 10; switchIndex++)
                    {
                        form.ApplyApplicationIcon((switchIndex & 1) != 0, true);
                    }
                }
            }
            var gdiAfter = GetGuiResources(processHandle, 0);
            var userAfter = GetGuiResources(processHandle, 1);
            True(gdiAfter <= gdiBefore + 4, "Application icon switching leaked GDI objects: " + gdiBefore + " -> " + gdiAfter);
            True(userAfter <= userBefore + 4, "Application icon switching leaked USER objects: " + userBefore + " -> " + userAfter);
        }

        private static long ApplyAndHashApplicationIcon(ThemedForm form, bool dark)
        {
            form.ApplyApplicationIcon(dark, true);
            Equal((bool?)dark, form.UsesDarkApplicationIcon);
            using (var bitmap = form.Icon.ToBitmap()) return BitmapHash(bitmap);
        }

        private static int IconTopBrightness(Icon icon)
        {
            using (var bitmap = icon.ToBitmap())
            {
                var color = bitmap.GetPixel(bitmap.Width / 2, Math.Max(1, bitmap.Height / 6));
                return color.R + color.G + color.B;
            }
        }

        private static Rectangle AlphaBounds(Bitmap bitmap)
        {
            var left = bitmap.Width;
            var top = bitmap.Height;
            var right = -1;
            var bottom = -1;
            for (var y = 0; y < bitmap.Height; y++)
            {
                for (var x = 0; x < bitmap.Width; x++)
                {
                    if (bitmap.GetPixel(x, y).A == 0) continue;
                    left = Math.Min(left, x);
                    top = Math.Min(top, y);
                    right = Math.Max(right, x);
                    bottom = Math.Max(bottom, y);
                }
            }
            return right < left ? Rectangle.Empty : Rectangle.FromLTRB(left, top, right + 1, bottom + 1);
        }

        private static void ThemeControlApplication()
        {
            var dark = ThemeService.GetPalette(AppThemeMode.Dark);
            using (var menu = new ContextMenuStrip())
            {
                var item = new ToolStripMenuItem("Parent");
                item.DropDownItems.Add(new ToolStripMenuItem("Child") { Enabled = false });
                menu.Items.Add(item);
                ThemeService.ApplyToMenu(menu, AppThemeMode.Dark);
                Equal(dark.SurfaceBackground, menu.BackColor);
                Equal(dark.Text, item.ForeColor);
                Equal(dark.MutedText, item.DropDownItems[0].ForeColor);
            }

            using (var proxy = new ProxySettingsForm(new ProxySettings { mode = ProxyMode.Direct }, AppThemeMode.Dark))
            {
                Equal(AppThemeMode.Dark, proxy.ThemeMode);
                Equal(dark.WindowBackground, proxy.BackColor);
                ComboBox modes = null;
                TextBox host = null;
                foreach (Control control in proxy.Controls)
                {
                    modes = modes ?? control as ComboBox;
                    var textBox = control as TextBox;
                    if (textBox != null && textBox.Width == 370) host = textBox;
                }
                True(modes != null && host != null, "Proxy theme controls are missing.");
                Equal(dark.DisabledBackground, host.BackColor);
                modes.SelectedIndex = (int)ProxyMode.Http;
                True(host.Enabled, "Custom proxy host did not become enabled.");
                Equal(dark.InputBackground, host.BackColor);
                proxy.ApplyAppTheme(AppThemeMode.Light);
                Equal(AppThemeMode.Light, proxy.ThemeMode);
                True(proxy.BackColor != dark.WindowBackground, "Proxy dialog stayed dark after switching to Light.");
            }

            using (var about = new AboutForm(AppThemeMode.Dark))
            {
                Equal(dark.WindowBackground, about.BackColor);
            }

            using (var help = new HelpForm(AppThemeMode.Dark))
            {
                Equal(dark.WindowBackground, help.BackColor);
            }

            using (var login = new LoginBrowserForm(new ProxySettings { mode = ProxyMode.System }, AppThemeMode.Dark))
            {
                Equal(AppThemeMode.Dark, login.ThemeMode);
                Equal(dark.WindowBackground, login.BackColor);
            }
        }

        private static void RefreshIntervalValidation()
        {
            foreach (var value in new[] { 1, 2, 5, 10, 15, 30, 60 }) True(SettingsStore.IsAllowedRefreshInterval(value), value + " rejected");
            foreach (var value in new[] { -1, 0, 3, 4, 61 }) True(!SettingsStore.IsAllowedRefreshInterval(value), value + " accepted");
            Equal(15, AppSettings.CreateDefault().refreshIntervalMinutes);
        }

        private static void DualQuotaReferencePixels()
        {
            foreach (var size in new[] { 16, 20, 24, 32 })
            foreach (var dark in new[] { false, true })
            using (var bitmap = BatteryTrayIconRenderer.RenderDualBitmap(43, 91, TrayVisualState.Normal, size, dark))
            {
                var inner = size - 2;
                var widths = new Dictionary<int, int> { {16,10}, {20,13}, {24,16}, {32,22} };
                var weekHeights = new Dictionary<int, int> { {16,13}, {20,16}, {24,20}, {32,27} };
                var fiveHeights = new Dictionary<int, int> { {16,6}, {20,8}, {24,9}, {32,13} };
                var xStart = 1 + (inner - widths[size]) / 2;
                for (var y = 0; y < size; y++)
                for (var x = 0; x < size; x++)
                {
                    var expected = dark ? "#3b3f41" : "#cfd3d5";
                    if (y >= size - 1 - weekHeights[size]) expected = "#4b926a";
                    if (y >= size - 1 - fiveHeights[size] && x >= xStart && x < xStart + widths[size]) expected = dark ? "#60cdff" : "#0067c0";
                    if (x == 0 || y == 0 || x == size - 1 || y == size - 1) expected = "#ff9800";
                    Equal(ColorTranslator.FromHtml(expected).ToArgb(), bitmap.GetPixel(x, y).ToArgb());
                }
            }
        }

        private static void QuotaIconModeTooltipContract()
        {
            var plus = new UsageSnapshot
            {
                userLevel = UserLevel.Plus,
                fiveHourLimitApplies = true,
                fiveHourRemainingPercent = 43,
                weeklyRemainingPercent = 91,
                fiveHourResetAt = new DateTime(2026, 9, 14, 14, 54, 0),
                weeklyResetAt = new DateTime(2026, 9, 17, 14, 58, 0)
            };
            var normal = TooltipFormatter.Build(plus, TrayVisualState.Normal, false);
            Equal(normal, TooltipFormatter.Build(plus, TrayVisualState.Normal, false, QuotaIconMode.Both));
            Equal(normal, TooltipFormatter.Build(plus, TrayVisualState.Normal, false, QuotaIconMode.WeeklyOnly));
            Equal(normal, TooltipFormatter.Build(plus, TrayVisualState.Normal, false, QuotaIconMode.SideBySide));
            var fiveOnly = TooltipFormatter.Build(plus, TrayVisualState.Normal, false, QuotaIconMode.FiveHourOnly);
            True(fiveOnly.StartsWith("5-Hour 43%", StringComparison.Ordinal), "5-Hour Only lost its applicable quota.");
            True(!fiveOnly.Contains("Weekly"), "5-Hour Only retained the Weekly Tooltip row.");
            True(TooltipFormatter.Build(plus, TrayVisualState.Normal, true, QuotaIconMode.FiveHourOnly).EndsWith(" [cached]", StringComparison.Ordinal),
                "5-Hour Only lost the cached suffix.");
            True(TooltipFormatter.Build(plus, TrayVisualState.FetchError, true, QuotaIconMode.FiveHourOnly).EndsWith(" [error]", StringComparison.Ordinal),
                "5-Hour Only lost the error suffix.");
            True(TooltipFormatter.Build(plus, TrayVisualState.LoginRequired, false, QuotaIconMode.FiveHourOnly).EndsWith(" [login]", StringComparison.Ordinal),
                "5-Hour Only lost the login suffix.");

            var pro = new UsageSnapshot
            {
                userLevel = UserLevel.Pro,
                fiveHourLimitApplies = false,
                weeklyRemainingPercent = 91,
                weeklyResetAt = plus.weeklyResetAt
            };
            Equal("5-Hour N/A", TooltipFormatter.Build(pro, TrayVisualState.Normal, false, QuotaIconMode.FiveHourOnly));
            True(TooltipFormatter.Build(pro, TrayVisualState.Normal, true, QuotaIconMode.FiveHourOnly).EndsWith(" [cached]", StringComparison.Ordinal),
                "Cached Pro N/A Tooltip lost its status.");
            True(TooltipFormatter.Build(pro, TrayVisualState.Normal, false, QuotaIconMode.Both).StartsWith("Weekly 91%", StringComparison.Ordinal),
                "Both changed the existing Pro Tooltip behavior.");
            True(TooltipFormatter.Build(pro, TrayVisualState.Normal, false, QuotaIconMode.WeeklyOnly).StartsWith("Weekly 91%", StringComparison.Ordinal),
                "Weekly Only changed the existing Pro Tooltip behavior.");
            Equal(TooltipFormatter.Build(pro, TrayVisualState.Normal, false, QuotaIconMode.Both),
                TooltipFormatter.Build(pro, TrayVisualState.Normal, false, QuotaIconMode.SideBySide));
            var unknownApplicability = new UsageSnapshot
            {
                userLevel = UserLevel.Unknown,
                fiveHourLimitApplies = null,
                weeklyRemainingPercent = 91,
                weeklyResetAt = plus.weeklyResetAt
            };
            True(TooltipFormatter.Build(unknownApplicability, TrayVisualState.Normal, false, QuotaIconMode.FiveHourOnly).StartsWith("5-Hour --", StringComparison.Ordinal),
                "Unknown 5-Hour applicability was mislabeled as N/A.");
            True(!unknownApplicability.KnownFiveHourLimitApplicability.HasValue, "Unknown 5-Hour applicability was collapsed to false.");
            foreach (var mode in new[] { QuotaIconMode.FiveHourOnly, QuotaIconMode.WeeklyOnly, QuotaIconMode.Both, QuotaIconMode.SideBySide })
            {
                True(TooltipFormatter.Build(plus, TrayVisualState.FetchError, true, mode).Length <= AppConstants.NotifyIconTextLimit,
                    mode + " Tooltip exceeds the NotifyIcon limit.");
            }
        }

        private static void QuotaIconModeRenderingMatrix()
        {
            foreach (var size in new[] { 16, 20, 24, 32 })
            foreach (var dark in new[] { false, true })
            {
                using (var both = BatteryTrayIconRenderer.RenderDualBitmap(43, 91, TrayVisualState.Normal, size, dark, true, QuotaIconMode.Both))
                using (var weekly = BatteryTrayIconRenderer.RenderDualBitmap(1, 91, TrayVisualState.Normal, size, dark, true, QuotaIconMode.WeeklyOnly))
                using (var five = BatteryTrayIconRenderer.RenderDualBitmap(43, 1, TrayVisualState.Normal, size, dark, true, QuotaIconMode.FiveHourOnly))
                using (var information = BatteryTrayIconRenderer.RenderDualBitmap(0, 91, TrayVisualState.Normal, size, dark, false, QuotaIconMode.FiveHourOnly, true))
                using (var unavailableUnknown = BatteryTrayIconRenderer.RenderDualBitmap(0, 91, TrayVisualState.Normal, size, dark, false, QuotaIconMode.FiveHourOnly, false))
                using (var applicabilityUnknown = BatteryTrayIconRenderer.RenderDualBitmap(43, 91, TrayVisualState.Normal, size, dark, null, QuotaIconMode.Both, false))
                {
                    var track = ColorTranslator.FromHtml(dark ? "#3b3f41" : "#cfd3d5");
                    var neutral = ColorTranslator.FromHtml(dark ? "#f0f3f2" : "#000000");
                    var blue = ColorTranslator.FromHtml(dark ? "#60cdff" : "#0067c0");
                    var green = ColorTranslator.FromHtml("#4b926a");
                    Equal(blue.ToArgb(), both.GetPixel(size / 2, size - 2).ToArgb());
                    Equal(green.ToArgb(), weekly.GetPixel(size / 2, size - 2).ToArgb());
                    Equal(neutral.ToArgb(), weekly.GetPixel(0, 0).ToArgb());
                    Equal(blue.ToArgb(), five.GetPixel(size / 2, size - 2).ToArgb());
                    Equal(track.ToArgb(), five.GetPixel(1, size - 2).ToArgb());
                    True(BitmapHash(information) != BitmapHash(unavailableUnknown), "The Pro information icon is indistinguishable from Unknown at " + size + "px.");
                    Equal(BitmapHash(unavailableUnknown), BitmapHash(applicabilityUnknown));

                    var informationPixels = 0;
                    for (var y = 1; y < size - 1; y++)
                    for (var x = 1; x < size - 1; x++)
                        if (information.GetPixel(x, y).ToArgb() != track.ToArgb()) informationPixels++;
                    True(informationPixels >= Math.Max(2, size / 4), "The information mark is not readable at " + size + "px.");
                }

                foreach (var value in new[] { 0, 20, 50, 100 })
                {
                    using (var five = BatteryTrayIconRenderer.RenderDualBitmap(value, 100 - value, TrayVisualState.Normal, size, dark, true, QuotaIconMode.FiveHourOnly))
                    using (var weekly = BatteryTrayIconRenderer.RenderDualBitmap(100 - value, value, TrayVisualState.Normal, size, dark, true, QuotaIconMode.WeeklyOnly))
                    {
                        var expectedFiveBorder = value <= 20 ? "#f04444" : value <= 50 ? "#ff9800" : dark ? "#f0f3f2" : "#000000";
                        Equal(ColorTranslator.FromHtml(expectedFiveBorder).ToArgb(), five.GetPixel(0, 0).ToArgb());
                        Equal(ColorTranslator.FromHtml(dark ? "#f0f3f2" : "#000000").ToArgb(), weekly.GetPixel(0, 0).ToArgb());
                    }
                }
            }
        }

        private static void QuotaIconModeCacheAndStatePriority()
        {
            var pro = new UsageSnapshot { userLevel = UserLevel.Pro, fiveHourLimitApplies = false, weeklyRemainingPercent = 91 };
            True(TrayApplicationContext.ShouldShowFiveHourNotApplicableInfo(pro), "Confirmed Pro weekly-only data did not enable the information icon.");
            pro.userLevel = UserLevel.Plus;
            True(!TrayApplicationContext.ShouldShowFiveHourNotApplicableInfo(pro), "Plus was mistaken for a Pro information-only state.");
            pro.userLevel = UserLevel.Pro;
            pro.fiveHourLimitApplies = true;
            pro.fiveHourRemainingPercent = 50;
            True(!TrayApplicationContext.ShouldShowFiveHourNotApplicableInfo(pro), "Valid Pro 5-Hour data was discarded as N/A.");
            True(TooltipFormatter.Build(pro, TrayVisualState.Normal, false, QuotaIconMode.FiveHourOnly).StartsWith("5-Hour 50%", StringComparison.Ordinal),
                "Valid Pro 5-Hour data was discarded from the 5-Hour-only Tooltip.");
            True(TooltipFormatter.BuildFiveHourMenuLine(pro).StartsWith("5-Hour 50%", StringComparison.Ordinal),
                "Valid Pro 5-Hour data was discarded from the menu.");

            using (var renderer = new BatteryTrayIconRenderer())
            {
                var weeklyA = renderer.GetDualIcon(1, 91, TrayVisualState.Normal, false, true, QuotaIconMode.WeeklyOnly);
                var weeklyB = renderer.GetDualIcon(99, 91, TrayVisualState.Normal, false, true, QuotaIconMode.WeeklyOnly);
                True(ReferenceEquals(weeklyA, weeklyB), "Hidden 5-Hour data polluted the Weekly-only cache key.");
                var fiveA = renderer.GetDualIcon(43, 1, TrayVisualState.Normal, false, true, QuotaIconMode.FiveHourOnly);
                var fiveB = renderer.GetDualIcon(43, 99, TrayVisualState.Normal, false, true, QuotaIconMode.FiveHourOnly);
                True(ReferenceEquals(fiveA, fiveB), "Hidden Weekly data polluted the 5-Hour-only cache key.");
                var both = renderer.GetDualIcon(43, 91, TrayVisualState.Normal, false, true, QuotaIconMode.Both);
                True(!ReferenceEquals(fiveA, both) && !ReferenceEquals(weeklyA, both), "Different icon modes shared a cache entry.");

                var information = renderer.GetDualIcon(null, 91, TrayVisualState.Normal, false, false, QuotaIconMode.FiveHourOnly, true);
                var unknown = renderer.GetDualIcon(null, 91, TrayVisualState.Normal, false, false, QuotaIconMode.FiveHourOnly, false);
                True(!ReferenceEquals(information, unknown), "Pro N/A reused the Unknown icon.");
                var errorWithInfoRequest = renderer.GetDualIcon(null, 91, TrayVisualState.FetchError, false, false, QuotaIconMode.FiveHourOnly, true);
                var error = renderer.GetDualIcon(null, 12, TrayVisualState.FetchError, false, false, QuotaIconMode.FiveHourOnly, false);
                True(ReferenceEquals(errorWithInfoRequest, error), "The information state overrode a refresh failure.");
                var loginWithInfoRequest = renderer.GetDualIcon(null, 91, TrayVisualState.LoginRequired, false, false, QuotaIconMode.FiveHourOnly, true);
                var login = renderer.GetDualIcon(null, null, TrayVisualState.LoginRequired, false, false, QuotaIconMode.FiveHourOnly, false);
                True(ReferenceEquals(loginWithInfoRequest, login), "The information state overrode login-required.");
                var bothUnknown = renderer.GetDualIcon(null, 91, TrayVisualState.Normal, false, null, QuotaIconMode.Both, false);
                var fiveUnknown = renderer.GetDualIcon(null, 91, TrayVisualState.Normal, false, null, QuotaIconMode.FiveHourOnly, false);
                True(!ReferenceEquals(bothUnknown, weeklyA), "Both treated unknown 5-Hour applicability as confirmed Weekly-only.");
                True(!ReferenceEquals(fiveUnknown, information), "Unknown 5-Hour applicability was shown as Pro N/A.");
            }
        }

        private static void SideBySideRenderingMatrix()
        {
            var values = new[] { 0, 1, 20, 21, 50, 51, 100 };
            foreach (var size in new[] { 16, 20, 24, 32 })
            foreach (var dark in new[] { false, true })
            foreach (var weekly in values)
            foreach (var five in values)
            using (var bitmap = BatteryTrayIconRenderer.RenderDualBitmap(
                five, weekly, TrayVisualState.Normal, size, dark, true, QuotaIconMode.SideBySide))
            {
                var inner = size - 2;
                var weeklyWidth = inner / 2;
                var weeklyHeight = BatteryTrayIconRenderer.FillPixels(weekly, inner);
                var fiveHeight = BatteryTrayIconRenderer.FillPixels(five, inner);
                var track = ColorTranslator.FromHtml(dark ? "#3b3f41" : "#cfd3d5");
                var weeklyFill = ExpectedQuotaColor(weekly, "#4b926a");
                var fiveFill = ColorTranslator.FromHtml(dark ? "#60cdff" : "#0067c0");
                var outline = ExpectedQuotaColor(five, dark ? "#f0f3f2" : "#000000");

                Equal(outline.ToArgb(), bitmap.GetPixel(0, 0).ToArgb());
                Equal(outline.ToArgb(), bitmap.GetPixel(size - 1, size - 1).ToArgb());
                for (var y = 1; y < size - 1; y++)
                for (var x = 1; x < size - 1; x++)
                {
                    var leftHalf = x < 1 + weeklyWidth;
                    var fillHeight = leftHalf ? weeklyHeight : fiveHeight;
                    var expected = y >= size - 1 - fillHeight
                        ? leftHalf ? weeklyFill : fiveFill
                        : track;
                    Equal(expected.ToArgb(), bitmap.GetPixel(x, y).ToArgb());
                }
            }
        }

        private static void SideBySideStateAndCache()
        {
            foreach (var size in new[] { 16, 20, 24, 32 })
            foreach (var dark in new[] { false, true })
            {
                var neutral = ColorTranslator.FromHtml(dark ? "#f0f3f2" : "#000000");
                using (var noFive = BatteryTrayIconRenderer.RenderDualBitmap(
                    0, 91, TrayVisualState.Normal, size, dark, false, QuotaIconMode.SideBySide))
                {
                    var firstRightPixel = 1 + (size - 2) / 2;
                    Equal(ColorTranslator.FromHtml("#4b926a").ToArgb(), noFive.GetPixel(1, size - 2).ToArgb());
                    Equal(ColorTranslator.FromHtml("#4b926a").ToArgb(), noFive.GetPixel(firstRightPixel, size - 2).ToArgb());
                    Equal(neutral.ToArgb(), noFive.GetPixel(0, 0).ToArgb());
                }
                using (var inferredUnknown = BatteryTrayIconRenderer.RenderDualBitmap(
                    43, 91, TrayVisualState.Normal, size, dark, null, QuotaIconMode.SideBySide))
                using (var explicitUnknown = BatteryTrayIconRenderer.RenderDualBitmap(
                    0, 0, TrayVisualState.Unknown, size, dark, null, QuotaIconMode.SideBySide))
                {
                    Equal(BitmapHash(explicitUnknown), BitmapHash(inferredUnknown));
                }
                foreach (var state in new[] { TrayVisualState.Unknown, TrayVisualState.FetchError, TrayVisualState.LoginRequired })
                using (var side = BatteryTrayIconRenderer.RenderDualBitmap(0, 0, state, size, dark, true, QuotaIconMode.SideBySide))
                using (var both = BatteryTrayIconRenderer.RenderDualBitmap(0, 0, state, size, dark, true, QuotaIconMode.Both))
                {
                    Equal(BitmapHash(both), BitmapHash(side));
                }
            }

            using (var renderer = new BatteryTrayIconRenderer())
            {
                var side = renderer.GetDualIcon(71, 91, TrayVisualState.Normal, false, true, QuotaIconMode.SideBySide);
                True(ReferenceEquals(side, renderer.GetDualIcon(71, 91, TrayVisualState.Normal, false, true, QuotaIconMode.SideBySide)),
                    "Side by Side did not reuse its exact cache entry.");
                var both = renderer.GetDualIcon(71, 91, TrayVisualState.Normal, false, true, QuotaIconMode.Both);
                True(!ReferenceEquals(side, both), "Side by Side shared the Both layout cache entry.");
                var noFiveA = renderer.GetDualIcon(null, 91, TrayVisualState.Normal, false, false, QuotaIconMode.SideBySide);
                var noFiveB = renderer.GetDualIcon(100, 91, TrayVisualState.Normal, false, false, QuotaIconMode.SideBySide);
                True(ReferenceEquals(noFiveA, noFiveB), "A hidden 5-Hour value polluted the Side by Side cache key.");
                var unknown = renderer.GetDualIcon(null, 91, TrayVisualState.Normal, false, null, QuotaIconMode.SideBySide);
                True(!ReferenceEquals(noFiveA, unknown), "Unknown applicability was collapsed into a confirmed full-width Weekly fallback.");
                True(!ReferenceEquals(
                    renderer.GetDualIcon(51, 91, TrayVisualState.Normal, false, true, QuotaIconMode.SideBySide),
                    renderer.GetDualIcon(50, 91, TrayVisualState.Normal, false, true, QuotaIconMode.SideBySide)),
                    "The 5-Hour border threshold reused the prior Side by Side cache entry.");
                True(!ReferenceEquals(
                    renderer.GetDualIcon(71, 51, TrayVisualState.Normal, false, true, QuotaIconMode.SideBySide),
                    renderer.GetDualIcon(71, 50, TrayVisualState.Normal, false, true, QuotaIconMode.SideBySide)),
                    "The Weekly fill threshold reused the prior Side by Side cache entry.");
            }
        }

        private static void SideBySideWeeklyFallback()
        {
            foreach (var size in new[] { 16, 20, 24, 32 })
            foreach (var dark in new[] { false, true })
            foreach (var weekly in new[] { 0, 1, 20, 21, 50, 51, 100 })
            using (var side = BatteryTrayIconRenderer.RenderDualBitmap(
                0, weekly, TrayVisualState.Normal, size, dark, false, QuotaIconMode.SideBySide))
            using (var weeklyOnly = BatteryTrayIconRenderer.RenderDualBitmap(
                0, weekly, TrayVisualState.Normal, size, dark, false, QuotaIconMode.WeeklyOnly))
            {
                Equal(BitmapHash(weeklyOnly), BitmapHash(side));
                var inner = size - 2;
                var fillHeight = BatteryTrayIconRenderer.FillPixels(weekly, inner);
                var track = ColorTranslator.FromHtml(dark ? "#3b3f41" : "#cfd3d5");
                var fill = ExpectedQuotaColor(weekly, "#4b926a");
                var neutral = ColorTranslator.FromHtml(dark ? "#f0f3f2" : "#000000");
                Equal(neutral.ToArgb(), side.GetPixel(0, 0).ToArgb());
                Equal(neutral.ToArgb(), side.GetPixel(size - 1, size - 1).ToArgb());
                for (var y = 1; y < size - 1; y++)
                for (var x = 1; x < size - 1; x++)
                {
                    var expected = y >= size - 1 - fillHeight ? fill : track;
                    Equal(expected.ToArgb(), side.GetPixel(x, y).ToArgb());
                }
            }
        }

        private static Color ExpectedQuotaColor(int value, string normal)
        {
            if (value <= 20) return ColorTranslator.FromHtml("#f04444");
            if (value <= 50) return ColorTranslator.FromHtml("#ff9800");
            return ColorTranslator.FromHtml(normal);
        }

        private static void AttentionGlyphMarks()
        {
            var expected = ColorTranslator.FromHtml(BatteryTrayIconRenderer.AttentionGlyphColorHex).ToArgb();
            foreach (var size in new[] { 16, 20, 24, 32 })
            foreach (var dark in new[] { false, true })
            {
                var bitmaps = new[]
                {
                    BatteryTrayIconRenderer.RenderDualBitmap(0, 91, TrayVisualState.Normal, size, dark, false, QuotaIconMode.FiveHourOnly, true),
                    BatteryTrayIconRenderer.RenderDualBitmap(0, 0, TrayVisualState.Unknown, size, dark),
                    BatteryTrayIconRenderer.RenderDualBitmap(0, 0, TrayVisualState.LoginRequired, size, dark)
                };
                var names = new[] { "i", "?", "L" };
                try
                {
                    for (var index = 0; index < bitmaps.Length; index++)
                    {
                        var redPixels = 0;
                        var minY = size;
                        var maxY = -1;
                        for (var y = 1; y < size - 1; y++)
                        for (var x = 1; x < size - 1; x++)
                        {
                            if (bitmaps[index].GetPixel(x, y).ToArgb() != expected) continue;
                            redPixels++;
                            minY = Math.Min(minY, y);
                            maxY = Math.Max(maxY, y);
                        }

                        True(redPixels >= Math.Max(4, size / 3), names[index] + " lacks a readable solid-red core at " + size + "px.");
                        True(maxY >= minY && maxY - minY + 1 >= (int)Math.Floor((size - 2) * .60d),
                            names[index] + " does not use the available icon height at " + size + "px.");
                    }
                }
                finally
                {
                    foreach (var bitmap in bitmaps) bitmap.Dispose();
                }
            }
        }

        private static void WeeklyOnlyTooltipAndIcon()
        {
            var snapshot = new UsageSnapshot
            {
                userLevel = UserLevel.Pro,
                fiveHourLimitApplies = false,
                weeklyRemainingPercent = 91,
                weeklyResetAt = new DateTime(2026, 9, 17, 20, 15, 0)
            };
            var tooltip = TooltipFormatter.Build(snapshot, TrayVisualState.Normal, false);
            var lines = tooltip.Split('\n');
            Equal(1, lines.Length);
            True(lines[0].StartsWith("Weekly 91%", StringComparison.Ordinal), "Weekly-only tooltip lost its quota.");
            True(!tooltip.Contains("5-Hour"), "The detected Pro/Weekly-only tooltip still displayed a 5-Hour row.");
            True(!tooltip.Contains("[error]"), "A weekly-only plan was displayed as an error.");
            Equal("5-Hour N/A", TooltipFormatter.BuildFiveHourMenuLine(snapshot));

            var webViewTooltip = TooltipFormatter.Build(snapshot, TrayVisualState.FetchError, true);
            var webViewLines = webViewTooltip.Split('\n');
            Equal(1, webViewLines.Length);
            True(webViewLines[0].EndsWith(" [error]", StringComparison.Ordinal), "The Pro tooltip lost its status suffix.");
            True(!webViewTooltip.Contains("5-Hour"), "A cached Pro WebView2 tooltip restored the hidden 5-Hour row.");
            True(webViewTooltip.Length <= AppConstants.NotifyIconTextLimit, "The Pro tooltip exceeds the NotifyIcon limit.");

            foreach (var dark in new[] { false, true })
            using (var bitmap = BatteryTrayIconRenderer.RenderDualBitmap(0, 91, TrayVisualState.Normal, 16, dark, false))
            {
                var neutralBorder = ColorTranslator.FromHtml(dark ? "#f0f3f2" : "#000000");
                var weeklyFill = ColorTranslator.FromHtml("#4b926a");
                Equal(neutralBorder.ToArgb(), bitmap.GetPixel(0, 0).ToArgb());
                Equal(weeklyFill.ToArgb(), bitmap.GetPixel(8, 14).ToArgb());
                True(bitmap.GetPixel(8, 14).ToArgb() != ColorTranslator.FromHtml(dark ? "#60cdff" : "#0067c0").ToArgb(),
                    "The weekly-only icon still drew a 5-hour overlay.");
            }
            using (var renderer = new BatteryTrayIconRenderer())
            {
                var weeklyOnly = renderer.GetDualIcon(null, 91, TrayVisualState.Normal, false, false);
                var error = renderer.GetDualIcon(null, 91, TrayVisualState.FetchError, false, false);
                True(!ReferenceEquals(weeklyOnly, error), "The weekly-only icon reused the error icon.");
            }
        }

        private static void IndependentQuotaWarningColors()
        {
            var values = new[] { 100, 51, 50, 21, 20, 1, 0 };
            var colors = new[] { "", "", "#ff9800", "#ff9800", "#f04444", "#f04444", "#f04444" };
            foreach (var size in new[] { 16, 20, 24, 32 })
            foreach (var dark in new[] { false, true })
            for (var f = 0; f < values.Length; f++)
            for (var w = 0; w < values.Length; w++)
            using (var bitmap = BatteryTrayIconRenderer.RenderDualBitmap(values[f], values[w], TrayVisualState.Normal, size, dark))
            {
                var border = colors[f] == "" ? (dark ? "#f0f3f2" : "#000000") : colors[f];
                var weekly = values[w] == 0 ? (dark ? "#3b3f41" : "#cfd3d5") : colors[w] == "" ? "#4b926a" : colors[w];
                Equal(ColorTranslator.FromHtml(border).ToArgb(), bitmap.GetPixel(0, 0).ToArgb());
                Equal(ColorTranslator.FromHtml(border).ToArgb(), bitmap.GetPixel(size - 1, size - 1).ToArgb());
                Equal(ColorTranslator.FromHtml(weekly).ToArgb(), bitmap.GetPixel(1, size - 2).ToArgb());
                if (values[f] > 0)
                    Equal(ColorTranslator.FromHtml(dark ? "#60cdff" : "#0067c0").ToArgb(), bitmap.GetPixel(size / 2, size - 2).ToArgb());
            }
            foreach (var dark in new[] { false, true })
            foreach (var state in new[] { TrayVisualState.Unknown, TrayVisualState.FetchError, TrayVisualState.LoginRequired })
            using (var bitmap = BatteryTrayIconRenderer.RenderDualBitmap(0, 0, state, 32, dark))
                Equal(ColorTranslator.FromHtml(dark ? "#f0f3f2" : "#000000").ToArgb(), bitmap.GetPixel(0, 0).ToArgb());
        }

        private static void QuotaWarningCacheTransitions()
        {
            foreach (var dark in new[] { false, true })
            using (var renderer = new BatteryTrayIconRenderer())
            {
                foreach (var boundary in new[] { 50, 20 })
                {
                    // Adjacent percentages can have identical pixel heights but different warning colors.
                    var above = renderer.GetDualIcon(boundary + 1, 91, TrayVisualState.Normal, dark);
                    var below = renderer.GetDualIcon(boundary, 91, TrayVisualState.Normal, dark);
                    True(!ReferenceEquals(above, below), "5-Hour color transition reused the previous icon.");
                    True(ReferenceEquals(above, renderer.GetDualIcon(boundary + 1, 91, TrayVisualState.Normal, dark)), "Recovery did not restore the cached icon.");
                    var weeklyAbove = renderer.GetDualIcon(80, boundary + 1, TrayVisualState.Normal, dark);
                    var weeklyBelow = renderer.GetDualIcon(80, boundary, TrayVisualState.Normal, dark);
                    True(!ReferenceEquals(weeklyAbove, weeklyBelow), "Weekly color transition reused the previous icon.");
                    using (var bitmap = below.ToBitmap())
                        Equal(ColorTranslator.FromHtml(boundary == 50 ? "#ff9800" : "#f04444").ToArgb(), bitmap.GetPixel(0, 0).ToArgb());
                }
            }
        }

        private static void DualQuotaPrecisionAndStates()
        {
            foreach (var size in new[] {16,20,24,32})
            {
                Equal(0, BatteryTrayIconRenderer.FillPixels(0, size - 2));
                Equal(1, BatteryTrayIconRenderer.FillPixels(1, size - 2));
                Equal(size - 2, BatteryTrayIconRenderer.FillPixels(100, size - 2));
                var previous = 0;
                for (var value = 0; value <= 100; value++)
                {
                    var height = BatteryTrayIconRenderer.FillPixels(value, size - 2);
                    True(height >= previous, "Quota height must be monotonic.");
                    previous = height;
                }
                foreach (var dark in new[] {false,true})
                {
                    var hashes = new HashSet<long>();
                    foreach (TrayVisualState state in Enum.GetValues(typeof(TrayVisualState)))
                    using (var bitmap = BatteryTrayIconRenderer.RenderDualBitmap(0, 0, state, size, dark))
                        True(hashes.Add(BitmapHash(bitmap)), "Zero and special states must remain distinct.");
                }
            }
            using (var renderer = new BatteryTrayIconRenderer())
            {
                True(ReferenceEquals(renderer.GetDualIcon(null, 91, TrayVisualState.Normal, false), renderer.GetDualIcon(null, null, TrayVisualState.Unknown, false)), "Missing quota must be Unknown.");
                True(!ReferenceEquals(renderer.GetDualIcon(43, 91, TrayVisualState.Normal, false), renderer.GetDualIcon(43, 0, TrayVisualState.Normal, false)), "Weekly must affect the icon.");
            }
        }

        private static void DualQuotaCacheLifecycle()
        {
            var before = GetGuiResources(Process.GetCurrentProcess().Handle, 0);
            for (var run = 0; run < 3; run++)
            using (var renderer = new BatteryTrayIconRenderer())
            {
                for (var five = 0; five <= 100; five += 3)
                for (var week = 0; week <= 100; week += 3)
                    renderer.GetDualIcon(five, week, TrayVisualState.Normal, run % 2 == 0, true,
                        run % 2 == 0 ? QuotaIconMode.SideBySide : QuotaIconMode.Both);
                True(GetGuiResources(Process.GetCurrentProcess().Handle, 0) <= before + 140, "Dual icon cache exceeded its native resource bound.");
            }
            True(GetGuiResources(Process.GetCurrentProcess().Handle, 0) <= before + 4, "Dual icon disposal leaked GDI objects.");
        }

        private static void IconThresholdMapping()
        {
            Equal(QuotaBand.Normal, BatteryTrayIconRenderer.GetQuotaBand(51));
            Equal(QuotaBand.Reminder, BatteryTrayIconRenderer.GetQuotaBand(50));
            Equal(QuotaBand.Reminder, BatteryTrayIconRenderer.GetQuotaBand(21));
            Equal(QuotaBand.Low, BatteryTrayIconRenderer.GetQuotaBand(20));
            Equal(QuotaBand.Low, BatteryTrayIconRenderer.GetQuotaBand(0));
        }

        private static void MaximizedIconGeometryAndNativeSize()
        {
            var preferredSize = BatteryTrayIconRenderer.GetPreferredIconSize();
            True(preferredSize >= 16 && preferredSize <= 32, "Native tray icon size is outside the supported range: " + preferredSize);
            foreach (var size in new[] { 16, 20, 24, 32 })
            {
                var body = BatteryTrayIconRenderer.GetSelectedStyleBody(size);
                True(Math.Abs(body.Width / body.Height - .75f) < .0001f, "Option A aspect ratio changed at " + size + " px.");
                True(body.Height > size * .75f, "Option A was not enlarged at " + size + " px.");
                True(body.Left >= 0 && body.Top >= 0 && body.Right <= size && body.Bottom <= size, "Option A escaped its canvas at " + size + " px.");
            }
            using (var renderer = new BatteryTrayIconRenderer())
            {
                var icon = renderer.GetIcon(71, TrayVisualState.Normal);
                Equal(preferredSize, icon.Width);
                Equal(preferredSize, icon.Height);
            }
        }

        private static void UnknownErrorStateMapping()
        {
            using (var unknown = BatteryTrayIconRenderer.RenderBitmap(BatteryIconStyle.B, 0, TrayVisualState.Unknown, 16))
            using (var error = BatteryTrayIconRenderer.RenderBitmap(BatteryIconStyle.B, 0, TrayVisualState.FetchError, 16))
            using (var zero = BatteryTrayIconRenderer.RenderBitmap(BatteryIconStyle.B, 0, TrayVisualState.Normal, 16))
            {
                True(BitmapHash(unknown) != BitmapHash(error), "Unknown and error icons are identical.");
                True(BitmapHash(unknown) != BitmapHash(zero), "Unknown and 0% icons are identical.");
                True(BitmapHash(error) != BitmapHash(zero), "Error and 0% icons are identical.");
            }
        }

        private static void TooltipLength()
        {
            var snapshot = new UsageSnapshot
            {
                fiveHourRemainingPercent = 100,
                weeklyRemainingPercent = 100,
                fiveHourResetAt = new DateTime(2026, 12, 31, 23, 59, 0),
                weeklyResetAt = new DateTime(2026, 12, 31, 23, 59, 0)
            };
            var text = TooltipFormatter.Build(snapshot, TrayVisualState.FetchError, true);
            True(text.Length <= AppConstants.NotifyIconTextLimit, "Tooltip exceeds NotifyIcon limit.");
            var lines = text.Split('\n');
            Equal(2, lines.Length);
            True(!text.Contains("@") && !text.Contains("[CLI]") && !text.Contains("[WV2]"),
                "Tooltip still identifies the account or source.");
            True(lines[0].Contains("5-Hour") && lines[1].Contains("Weekly"), "Tooltip lacks core usage lines.");
            True(lines[1].EndsWith(" [error]", StringComparison.Ordinal), "Tooltip lost the status marker.");

            var unavailable = TooltipFormatter.Build(snapshot, TrayVisualState.LoginRequired, true);
            True(unavailable.Split('\n')[1].EndsWith(" [login]", StringComparison.Ordinal), "Login tooltip lacks its status marker.");
            True(unavailable.Length <= AppConstants.NotifyIconTextLimit, "Login tooltip exceeds NotifyIcon limit.");
        }

        private static void StartupPathQuoting()
        {
            Equal("\"C:\\Program Files\\Codex Usage Tray Lite\\CodexUsageTrayLite.exe\" --startup", StartupManager.BuildCommand("C:\\Program Files\\Codex Usage Tray Lite\\CodexUsageTrayLite.exe"));
        }

        private static void SafeLoggerRedaction()
        {
            var value = SafeLogger.Sanitize("failed https://auth.example.com/oauth/callback?code=secret password=hunter2 email=user@example.com");
            True(!value.Contains("secret") && !value.Contains("hunter2") && !value.Contains("/oauth/") && !value.Contains("email=user@example.com"),
                "Sensitive log data was not redacted.");
            True(value.Contains("email=u***@example.com"), "Email masking does not match the tooltip display rule.");
        }

        private static void LogEventLevelAndFormat()
        {
            Equal(LogEventLevel.Info, SafeLogger.InferLevel("Refresh.Complete"));
            Equal(LogEventLevel.Warning, SafeLogger.InferLevel("Refresh.Failed"));
            Equal(LogEventLevel.Error, SafeLogger.InferLevel("WebView.ProcessFailed"));
            Equal(LogEventLevel.Error, SafeLogger.InferLevel("Fatal"));

            var line = SafeLogger.FormatLine(
                new DateTime(2026, 9, 3, 16, 20, 30, 123),
                LogEventLevel.Warning,
                "Account.EmailResolved",
                "source=WebView2 email=user@example.com");
            True(line.StartsWith("2026-09-03 16:20:30.123 | level=warning | event=Account.EmailResolved | ", StringComparison.Ordinal),
                "Structured log fields are missing or out of order.");
            True(line.Contains("email=u***@example.com") && !line.Contains("email=user@example.com"),
                "Structured log line contains an unmasked email.");
        }

        private static void UsageFieldUnavailableDiagnostics()
        {
            Equal(LogEventLevel.Warning, SafeLogger.InferLevel(UsageFieldTelemetry.EventName));
            Equal(
                "source=WebView2 field=five-hour reason=percentage-unreadable",
                UsageFieldTelemetry.FormatDetails(
                    UsageSource.WebView2,
                    UsageDataField.FiveHour,
                    UsageFieldUnavailableReason.PercentageUnreadable));
            Equal(
                "source=CodexCli field=weekly reason=rate-limit-window-missing",
                UsageFieldTelemetry.FormatDetails(
                    UsageSource.CodexCli,
                    UsageDataField.Weekly,
                    UsageFieldUnavailableReason.RateLimitWindowMissing));
            Equal(
                "source=WebView2 field=resets reason=reset-section-unrecognized",
                UsageFieldTelemetry.FormatDetails(
                    UsageSource.WebView2,
                    UsageDataField.Resets,
                    UsageFieldUnavailableReason.ResetSectionUnrecognized));

            UsageSnapshot snapshot;
            string error;
            UsageFieldDiagnostics diagnostics;
            var malformedWebView =
                "5-hour usage limit\nnot a percentage\nWeekly usage limit\nnot a percentage\n" +
                "Usage limit resets\nHistory\nReset used\nAuto reload";
            True(!UsageParser.TryParse(
                malformedWebView,
                DateTime.Now,
                out snapshot,
                out error,
                out diagnostics), "Malformed WebView fields were accepted.");
            UsageFieldUnavailableReason reason;
            True(diagnostics.TryGetReason(UsageDataField.FiveHour, out reason), "The failed 5-hour field was not identified.");
            Equal(UsageFieldUnavailableReason.PercentageUnreadable, reason);
            True(diagnostics.TryGetReason(UsageDataField.Weekly, out reason), "The failed weekly field was not identified.");
            Equal(UsageFieldUnavailableReason.PercentageUnreadable, reason);
            True(diagnostics.TryGetReason(UsageDataField.Resets, out reason), "The failed WebView reset field was not identified.");
            Equal(UsageFieldUnavailableReason.ResetSectionUnrecognized, reason);

            var serializer = new JavaScriptSerializer();
            var cliMissingReset = serializer.DeserializeObject(
                "{\"rateLimits\":{\"primary\":{\"usedPercent\":0,\"windowDurationMins\":10080}}}") as IDictionary<string, object>;
            True(CodexRateLimitsParser.TryParse(
                cliMissingReset,
                DateTime.Now,
                out snapshot,
                out error,
                out diagnostics), error);
            True(diagnostics.TryGetReason(UsageDataField.Resets, out reason), "The missing CLI reset field was not identified.");
            Equal(UsageFieldUnavailableReason.ResponseFieldMissing, reason);
            True(!diagnostics.TryGetReason(UsageDataField.FiveHour, out reason),
                "A valid Weekly-only plan incorrectly logged 5-hour as unavailable.");

            var cliInvalidFiveHour = serializer.DeserializeObject(
                "{\"rateLimits\":{\"primary\":{\"usedPercent\":\"invalid\",\"windowDurationMins\":300},\"secondary\":{\"usedPercent\":20,\"windowDurationMins\":10080}},\"rateLimitResetCredits\":{\"availableCount\":0}}") as IDictionary<string, object>;
            True(!CodexRateLimitsParser.TryParse(
                cliInvalidFiveHour,
                DateTime.Now,
                out snapshot,
                out error,
                out diagnostics), "An invalid CLI 5-hour window was accepted.");
            True(diagnostics.TryGetReason(UsageDataField.FiveHour, out reason), "The invalid CLI 5-hour window was not identified.");
            Equal(UsageFieldUnavailableReason.RateLimitWindowInvalid, reason);

            Equal(
                UsageFieldUnavailableReason.DomProbeFailed,
                WebViewFetchSession.ClassifyMissingUsageReset("Usage limit resets", true));
            Equal(
                UsageFieldUnavailableReason.ResetSectionUnrecognized,
                WebViewFetchSession.ClassifyMissingUsageReset("Usage limit resets\nHistory\nReset used\nAuto reload", false));
            Equal(
                UsageFieldUnavailableReason.ResetSectionMissing,
                WebViewFetchSession.ClassifyMissingUsageReset("Weekly usage limit\n80% remaining", false));
        }

        private static void LogFileOpenLaunch()
        {
            var path = Path.Combine(Path.GetTempPath(), "CodexUsageTrayLite", "CodexUsageTrayLite.log");
            var startInfo = TrayApplicationContext.CreateFileOpenStartInfo(path);
            Equal(Path.GetFullPath(path), startInfo.FileName);
            True(startInfo.UseShellExecute, "Log file is not opened through its Windows file association.");
        }

        private static void LogDailyRotation()
        {
            var root = Path.Combine(Path.GetTempPath(), "CodexUsageTrayLite.Tests", "log-rotation-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            var activePath = Path.Combine(root, "CodexUsageTrayLite.log");
            var previousDate = new DateTime(2026, 9, 3);
            var currentDate = previousDate.AddDays(1);
            var archivePath = Path.Combine(root, "CodexUsageTrayLite_20260903.log");
            try
            {
                File.WriteAllText(activePath, "previous-day", new UTF8Encoding(false));
                File.SetLastWriteTime(activePath, previousDate.AddHours(23).AddMinutes(59));
                SafeLogger.RotateDailyIfNeeded(activePath, currentDate);

                True(!File.Exists(activePath), "The previous day's active log was not moved.");
                True(File.Exists(archivePath), "The date-stamped archive was not created.");
                Equal("previous-day", File.ReadAllText(archivePath));
                Equal(archivePath, SafeLogger.BuildDailyArchivePath(activePath, previousDate));

                File.WriteAllText(activePath, "current-day", new UTF8Encoding(false));
                File.SetLastWriteTime(activePath, currentDate.AddHours(8));
                SafeLogger.RotateDailyIfNeeded(activePath, currentDate.AddHours(12));
                True(File.Exists(activePath), "A same-day log was rotated unexpectedly.");

                File.WriteAllText(activePath, "second-previous-day-part", new UTF8Encoding(false));
                File.SetLastWriteTime(activePath, previousDate.AddHours(22));
                SafeLogger.RotateDailyIfNeeded(activePath, currentDate);
                var archived = File.ReadAllText(archivePath);
                True(archived.Contains("previous-day") && archived.Contains("second-previous-day-part"),
                    "An existing daily archive did not preserve both log segments.");
            }
            finally
            {
                foreach (var file in Directory.GetFiles(root)) File.Delete(file);
                Directory.Delete(root);
            }
        }

        private static async Task RefreshConcurrency()
        {
            var factory = new FakeSessionFactory(50);
            using (var service = new UsageFetchService(factory))
            {
                var first = service.FetchAsync(UsageSource.WebView2, new ProxySettings { mode = ProxyMode.System }, CancellationToken.None);
                await Task.Delay(5);
                var second = await service.FetchAsync(UsageSource.WebView2, new ProxySettings { mode = ProxyMode.System }, CancellationToken.None);
                Equal(FetchStatus.Busy, second.Status);
                await first;
            }
            Equal(1, factory.Created);
            Equal(1, factory.Disposed);
        }

        private static async Task RepeatedRefreshLifecycle100()
        {
            var factory = new FakeSessionFactory(1);
            var process = Process.GetCurrentProcess();
            var handlesBefore = process.HandleCount;
            using (var service = new UsageFetchService(factory))
            {
                for (var index = 0; index < 100; index++)
                {
                    var source = index % 2 == 0 ? UsageSource.WebView2 : UsageSource.CodexCli;
                    var outcome = await service.FetchAsync(source, new ProxySettings { mode = ProxyMode.System }, CancellationToken.None);
                    Equal(FetchStatus.Success, outcome.Status);
                }
            }
            process.Refresh();
            Equal(100, factory.Created);
            Equal(100, factory.Disposed);
            True(process.HandleCount <= handlesBefore + 12, "Handle count grew unexpectedly: " + handlesBefore + " -> " + process.HandleCount);
            Console.WriteLine("Resource loop: sessions=100 handles=" + handlesBefore + "->" + process.HandleCount + " workingSet=" + process.WorkingSet64);
        }

        private static async Task RefreshCancellation()
        {
            var factory = new FakeSessionFactory(5000);
            using (var service = new UsageFetchService(factory))
            {
                True(!service.CancelActive("idle-test"), "An idle service reported an active refresh.");
                var fetch = service.FetchAsync(UsageSource.CodexCli, new ProxySettings { mode = ProxyMode.System }, CancellationToken.None);
                await Task.Delay(10);
                True(service.IsBusy, "The cancellable refresh did not become active.");
                True(service.CancelActive("interactive-test"), "The active refresh could not be cancelled.");
                var outcome = await fetch;
                Equal(FetchStatus.Cancelled, outcome.Status);
                True(!service.IsBusy, "The service stayed busy after cancellation.");
            }
            Equal(1, factory.Created);
            Equal(1, factory.Disposed);
        }

        private static void GdiIconLifecycle()
        {
            // Warm up process-wide System.Drawing font/text caches before measuring owned resources.
            using (var warmup = new BatteryTrayIconRenderer())
            {
                warmup.GetIcon(71, TrayVisualState.Normal);
                warmup.GetIcon(null, TrayVisualState.Unknown);
                warmup.GetIcon(null, TrayVisualState.FetchError);
                warmup.GetIcon(null, TrayVisualState.LoginRequired);
            }
            var before = GetGuiResources(Process.GetCurrentProcess().Handle, 0);
            for (var index = 0; index < 20; index++)
            {
                using (var renderer = new BatteryTrayIconRenderer())
                {
                    for (var percent = 0; percent <= 100; percent++) renderer.GetIcon(percent, TrayVisualState.Normal);
                    renderer.GetIcon(null, TrayVisualState.Unknown);
                    renderer.GetIcon(null, TrayVisualState.FetchError);
                    renderer.GetIcon(null, TrayVisualState.LoginRequired);
                }
            }
            var after = GetGuiResources(Process.GetCurrentProcess().Handle, 0);
            True(after <= before + 4, "GDI objects grew unexpectedly: " + before + " -> " + after);
            Console.WriteLine("GDI loop: " + before + "->" + after);
        }

        private static long BitmapHash(Bitmap bitmap)
        {
            long hash = 17;
            for (var y = 0; y < bitmap.Height; y++)
                for (var x = 0; x < bitmap.Width; x++) hash = unchecked(hash * 31 + bitmap.GetPixel(x, y).ToArgb());
            return hash;
        }

        private static void Test(string name, Action action)
        {
            try { action(); passed++; Console.WriteLine("PASS " + name); }
            catch (Exception ex) { Failures.Add(name + ": " + ex.Message); }
        }

        private static async Task TestAsync(string name, Func<Task> action)
        {
            try { await action(); passed++; Console.WriteLine("PASS " + name); }
            catch (Exception ex) { Failures.Add(name + ": " + ex.Message); }
        }

        private static void True(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
        private static void Equal<T>(T expected, T actual) { if (!EqualityComparer<T>.Default.Equals(expected, actual)) throw new InvalidOperationException("Expected " + expected + ", got " + actual + "."); }
        private static void Throws<T>(Action action) where T : Exception
        {
            try { action(); }
            catch (T) { return; }
            throw new InvalidOperationException("Expected " + typeof(T).Name + ".");
        }

        [DllImport("user32.dll")]
        private static extern int GetGuiResources(IntPtr process, int flags);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern uint PrivateExtractIcons(
            string fileName,
            int iconIndex,
            int iconWidth,
            int iconHeight,
            [Out] IntPtr[] iconHandles,
            [Out] uint[] iconIds,
            uint iconCount,
            uint flags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DestroyIcon(IntPtr iconHandle);

        private sealed class FakeSessionFactory : IUsageFetchSessionFactory
        {
            private readonly int delay;
            public int Created;
            public int Disposed;
            public FakeSessionFactory(int delay) { this.delay = delay; }
            public UsageSource LastSource;
            public IUsageFetchSession Create(UsageSource source, ProxySettings proxy) { Created++; LastSource = source; return new FakeSession(this, delay); }
        }

        private static async Task<int> RunCliProbeAsync()
        {
            using (var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
            using (var session = new CodexCliFetchSession())
            {
                var watch = Stopwatch.StartNew();
                var outcome = await session.RunAsync(timeout.Token);
                watch.Stop();
                if (!outcome.IsSuccess || !outcome.Snapshot.weeklyRemainingPercent.HasValue ||
                    (outcome.Snapshot.IsFiveHourLimitApplicable && !outcome.Snapshot.fiveHourRemainingPercent.HasValue))
                {
                    Console.Error.WriteLine("CLI probe failed: " + outcome.Status + " " + outcome.Message);
                    return 1;
                }
                if (!outcome.AccountEmailRead || string.IsNullOrWhiteSpace(outcome.AccountEmail))
                {
                    Console.Error.WriteLine("CLI probe failed: account email was unavailable.");
                    return 1;
                }
                Console.WriteLine("CLI probe succeeded: windows=" +
                    (outcome.Snapshot.IsFiveHourLimitApplicable ? "2" : "1") +
                    " fiveHourApplicable=" + outcome.Snapshot.IsFiveHourLimitApplicable.ToString().ToLowerInvariant() +
                    " elapsedMs=" + watch.ElapsedMilliseconds +
                    " usageValuesPrinted=false emailRead=true emailPrinted=false");
                return 0;
            }
        }

        private static int RunWebViewEmailProbe()
        {
            var result = 2;
            using (var runner = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                ShowInTaskbar = false,
                StartPosition = FormStartPosition.Manual,
                Location = new Point(-32000, -32000),
                Size = new Size(2, 2),
                Opacity = 0
            })
            {
                runner.Shown += async (sender, args) =>
                {
                    try
                    {
                        using (var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(45)))
                        using (var session = new WebViewFetchSession(new ProxySettings { mode = ProxyMode.System }))
                        {
                            var outcome = await session.RunAsync(timeout.Token);
                            if (!outcome.IsSuccess || !outcome.AccountEmailRead || string.IsNullOrWhiteSpace(outcome.AccountEmail) ||
                                outcome.Snapshot == null || !outcome.Snapshot.availableUsageResetCount.HasValue)
                            {
                                Console.Error.WriteLine("WebView probe failed: status=" + outcome.Status +
                                    " emailRead=" + outcome.AccountEmailRead +
                                    " usageResetCountRead=" + (outcome.Snapshot != null && outcome.Snapshot.availableUsageResetCount.HasValue));
                                result = 1;
                            }
                            else
                            {
                                Console.WriteLine("WebView probe succeeded: usageValuesPrinted=false emailRead=true emailPrinted=false usageResetCountRead=true usageResetCountPrinted=false");
                                result = 0;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine("WebView probe failed: " + ex.GetType().Name);
                        result = 2;
                    }
                    finally
                    {
                        runner.Close();
                    }
                };
                Application.Run(runner);
            }
            return result;
        }

        private sealed class FakeSession : IUsageFetchSession
        {
            private readonly FakeSessionFactory owner;
            private readonly int delay;
            public FakeSession(FakeSessionFactory owner, int delay) { this.owner = owner; this.delay = delay; }
            public async Task<FetchOutcome> RunAsync(CancellationToken token)
            {
                await Task.Delay(delay, token);
                return FetchOutcome.Create(FetchStatus.Success, "ok", new UsageSnapshot { fiveHourRemainingPercent = 71, weeklyRemainingPercent = 81, lastSuccessfulFetchAt = DateTime.Now });
            }
            public void Dispose() { owner.Disposed++; }
        }
    }

    internal static class IconPreviewGenerator
    {
        private static readonly int[] Values = { 100, 75, 50, 25, 10, 0 };
        private static readonly int[] Sizes = { 16, 20, 24, 32 };

        public static void Generate(string repositoryRoot)
        {
            var output = Path.Combine(Path.GetFullPath(repositoryRoot), "docs", "icon-options");
            Directory.CreateDirectory(output);
            var sheets = new List<Bitmap>();
            try
            {
                foreach (BatteryIconStyle style in Enum.GetValues(typeof(BatteryIconStyle)))
                {
                    var sheet = CreateOptionSheet(style);
                    sheets.Add(sheet);
                    sheet.Save(Path.Combine(output, "option-" + style.ToString().ToLowerInvariant() + ".png"), ImageFormat.Png);
                }
                using (var contact = new Bitmap(1220, 940))
                using (var graphics = Graphics.FromImage(contact))
                using (var title = new Font("Segoe UI", 20, FontStyle.Bold))
                using (var label = new Font("Segoe UI", 12, FontStyle.Bold))
                using (var textBrush = new SolidBrush(Color.FromArgb(232, 235, 237)))
                {
                    graphics.Clear(Color.FromArgb(32, 35, 38));
                    graphics.DrawString("Codex Usage Tray Lite - Battery icon options", title, textBrush, 22, 16);
                    for (var index = 0; index < sheets.Count; index++)
                    {
                        var x = 20 + (index % 2) * 600;
                        var y = 62 + (index / 2) * 430;
                        graphics.DrawString("Option " + ((BatteryIconStyle)index), label, textBrush, x, y);
                        graphics.DrawImage(sheets[index], new Rectangle(x, y + 30, 580, 380));
                    }
                    contact.Save(Path.Combine(output, "contact-sheet.png"), ImageFormat.Png);
                }
            }
            finally
            {
                foreach (var sheet in sheets) sheet.Dispose();
            }
        }

        public static void GenerateQuotaModes(string repositoryRoot)
        {
            var output = Path.Combine(Path.GetFullPath(repositoryRoot), "docs", "icon-options");
            Directory.CreateDirectory(output);
            var modes = new[]
            {
                "Both", "Side by Side", "Weekly Only", "5-Hour Only", "Pro N/A", "Unknown", "Error", "Login"
            };
            using (var sheet = new Bitmap(1260, 800))
            using (var graphics = Graphics.FromImage(sheet))
            using (var title = new Font("Segoe UI", 20, FontStyle.Bold))
            using (var label = new Font("Segoe UI", 10, FontStyle.Bold))
            using (var small = new Font("Segoe UI", 9, FontStyle.Regular))
            {
                graphics.Clear(Color.FromArgb(239, 241, 243));
                graphics.DrawString("Quota icon display modes - v0.2.16", title, Brushes.Black, 20, 14);
                for (var column = 0; column < modes.Length; column++)
                    graphics.DrawString(modes[column], label, Brushes.Black, 120 + column * 140, 58);
                var row = 0;
                foreach (var dark in new[] { false, true })
                foreach (var size in new[] { 16, 20, 24, 32 })
                {
                    var y = 86 + row * 86;
                    var rowBackground = dark ? Color.FromArgb(32, 35, 38) : Color.FromArgb(248, 249, 250);
                    using (var rowBrush = new SolidBrush(rowBackground))
                    using (var textBrush = new SolidBrush(dark ? Color.White : Color.Black))
                    {
                        graphics.FillRectangle(rowBrush, 12, y - 8, 1236, 78);
                        graphics.DrawString((dark ? "Dark " : "Light ") + size + " px", small, textBrush, 22, y + 20);
                        for (var column = 0; column < modes.Length; column++)
                        {
                            var state = column == 5 ? TrayVisualState.Unknown : column == 6 ? TrayVisualState.FetchError : column == 7 ? TrayVisualState.LoginRequired : TrayVisualState.Normal;
                            var mode = column == 1 ? QuotaIconMode.SideBySide : column == 2 ? QuotaIconMode.WeeklyOnly : column == 3 || column == 4 ? QuotaIconMode.FiveHourOnly : QuotaIconMode.Both;
                            var applies = column == 4 ? (bool?)false : true;
                            var information = column == 4;
                            using (var bitmap = BatteryTrayIconRenderer.RenderDualBitmap(43, 91, state, size, dark, applies, mode, information))
                            {
                                graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                                graphics.PixelOffsetMode = PixelOffsetMode.Half;
                                graphics.DrawImage(bitmap, new Rectangle(120 + column * 140, y, size * 2, size * 2), 0, 0, size, size, GraphicsUnit.Pixel);
                            }
                        }
                    }
                    row++;
                }
                sheet.Save(Path.Combine(output, "quota-icon-modes-v0.2.16.png"), ImageFormat.Png);
            }
        }

        public static void GenerateSideBySide(string repositoryRoot)
        {
            var output = Path.Combine(Path.GetFullPath(repositoryRoot), "docs", "icon-options");
            Directory.CreateDirectory(output);
            var labels = new[]
            {
                "Normal", "5-Hour 45%", "5-Hour 15%", "Weekly 43%", "Weekly 15%", "Both 15%", "No 5-Hour"
            };
            var fiveValues = new[] { 71, 45, 15, 71, 71, 15, 0 };
            var weeklyValues = new[] { 91, 91, 91, 43, 15, 15, 91 };
            using (var sheet = new Bitmap(1120, 790))
            using (var graphics = Graphics.FromImage(sheet))
            using (var title = new Font("Segoe UI", 20, FontStyle.Bold))
            using (var label = new Font("Segoe UI", 9, FontStyle.Bold))
            using (var small = new Font("Segoe UI", 9, FontStyle.Regular))
            {
                graphics.Clear(Color.FromArgb(239, 241, 243));
                graphics.DrawString("Side-by-Side quota icon - v0.2.16", title, Brushes.Black, 20, 14);
                for (var column = 0; column < labels.Length; column++)
                {
                    graphics.DrawString(labels[column], label, Brushes.Black, 112 + column * 142, 58);
                    graphics.DrawString("W " + weeklyValues[column] + "% / 5H " + (column == 6 ? "N/A" : fiveValues[column] + "%"),
                        small, Brushes.DimGray, 112 + column * 142, 76);
                }

                var row = 0;
                foreach (var dark in new[] { false, true })
                foreach (var size in new[] { 16, 20, 24, 32 })
                {
                    var y = 108 + row * 82;
                    var rowBackground = dark ? Color.FromArgb(32, 35, 38) : Color.FromArgb(248, 249, 250);
                    using (var rowBrush = new SolidBrush(rowBackground))
                    using (var textBrush = new SolidBrush(dark ? Color.White : Color.Black))
                    {
                        graphics.FillRectangle(rowBrush, 12, y - 8, 1096, 74);
                        graphics.DrawString((dark ? "Dark " : "Light ") + size + " px", small, textBrush, 22, y + 20);
                        for (var column = 0; column < labels.Length; column++)
                        using (var bitmap = BatteryTrayIconRenderer.RenderDualBitmap(
                            fiveValues[column], weeklyValues[column], TrayVisualState.Normal, size, dark,
                            column == 6 ? (bool?)false : true, QuotaIconMode.SideBySide))
                        {
                            graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                            graphics.PixelOffsetMode = PixelOffsetMode.Half;
                            graphics.DrawImage(bitmap, new Rectangle(112 + column * 142, y, size * 2, size * 2),
                                0, 0, size, size, GraphicsUnit.Pixel);
                        }
                    }
                    row++;
                }
                sheet.Save(Path.Combine(output, "side-by-side-v0.2.16.png"), ImageFormat.Png);
            }
        }

        private static Bitmap CreateOptionSheet(BatteryIconStyle style)
        {
            var sheet = new Bitmap(1160, 760);
            using (var graphics = Graphics.FromImage(sheet))
            using (var title = new Font("Segoe UI", 24, FontStyle.Bold))
            using (var label = new Font("Segoe UI", 12, FontStyle.Regular))
            using (var small = new Font("Segoe UI", 10, FontStyle.Regular))
            using (var brush = new SolidBrush(Color.FromArgb(232, 235, 237)))
            {
                graphics.Clear(Color.FromArgb(32, 35, 38));
                graphics.DrawString("Option " + style, title, brush, 24, 18);
                var columns = new[] { "100%", "75%", "50%", "25%", "10%", "0%", "Unknown", "Error", "Login" };
                for (var column = 0; column < columns.Length; column++) graphics.DrawString(columns[column], label, brush, 28 + column * 124, 72);
                for (var row = 0; row < Sizes.Length; row++)
                {
                    var size = Sizes[row];
                    var y = 118 + row * 150;
                    graphics.DrawString(size + "x" + size, label, brush, 8, y + 30);
                    for (var column = 0; column < columns.Length; column++)
                    {
                        var state = column == 6 ? TrayVisualState.Unknown : column == 7 ? TrayVisualState.FetchError : column == 8 ? TrayVisualState.LoginRequired : TrayVisualState.Normal;
                        var value = column < Values.Length ? Values[column] : 0;
                        using (var icon = BatteryTrayIconRenderer.RenderBitmap(style, value, state, size))
                        {
                            var x = 44 + column * 124;
                            graphics.DrawImageUnscaled(icon, x + (32 - size) / 2, y);
                            graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                            graphics.PixelOffsetMode = PixelOffsetMode.Half;
                            graphics.DrawImage(icon, new Rectangle(x, y + 42, size * 3, size * 3), 0, 0, size, size, GraphicsUnit.Pixel);
                            graphics.DrawString("native + 3x", small, brush, x, y + 48 + size * 3);
                        }
                    }
                }
            }
            return sheet;
        }
    }
}
