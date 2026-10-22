using System;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;

namespace CodexUsageTrayLite.Services
{
    internal enum UsageOverviewState { Unavailable, Ready, Selected }

    internal static class WebViewUsagePageNavigator
    {
        internal static string Script => BuildScript(true);

        internal static string BuildScript(bool allowClick, string previousDocument = null) => @"(() => {
const normalize = value => (value || '').normalize('NFKC').replace(/\s+/g, ' ').trim().toLowerCase();
const overview = new Set(['overview', '概览', '概覽', '總覽', '总览', '概要', '개요']);
const analytics = new Set(['analytics', '分析', '统计', '統計', 'アナリティクス', '분석']);
const usageHeadings = new Set(['usage', '用量', '使用量', '사용량', '使用状況', 'codex and work analytics']);
const visible = element => {
  const style = getComputedStyle(element);
  return element.getClientRects().length > 0 && style.display !== 'none' && style.visibility !== 'hidden';
};
const controls = Array.from(document.querySelectorAll('[role=tab],button,a')).filter(visible);
const text = element => normalize(element.innerText || element.textContent);
const result = { state: 'unavailable', reason: 'heading-missing', documentState: document.readyState,
  candidates: 0, analyticsSelected: false, trustedOrigin: location.origin === 'https://chatgpt.com',
  documentKey: String(performance.timeOrigin), text: document.body ? document.body.innerText : '' };
const finish = (state, reason) => { result.state = state; result.reason = reason; return result; };
if (!result.trustedOrigin) return finish('unavailable', 'origin-not-ready');
if (__OLD_DOCUMENT__ !== null && result.documentKey === __OLD_DOCUMENT__) return finish('unavailable', 'document-replacing');
const usageHeading = Array.from(document.querySelectorAll('h1,h2,h3,[role=heading]')).some(element =>
    visible(element) && usageHeadings.has(text(element)));
// Some site revisions render the title as a plain div. Only allow that fallback
// on a known Usage route, rather than treating arbitrary document text as a title.
const usageRoute = /\/codex\/cloud\/settings\/analytics(?:\/|$)|\/settings\/usage(?:\/|$)/i.test(location.pathname) ||
  /^#settings\/usage(?:\/|$)/i.test(location.hash);
if (!usageHeading && !usageRoute) return result;
const selected = element => element.getAttribute('aria-selected') === 'true' ||
  element.getAttribute('data-state') === 'active' || element.getAttribute('aria-current') === 'page';
const groupFor = element => {
  let group = element.parentElement;
  for (let depth = 0; group && depth < 10; depth++, group = group.parentElement) {
    if (group === document.body || group === document.documentElement) break;
    const members = controls.filter(other => group.contains(other));
    // Accept deep wrappers, but do not widen the search into an entire settings
    // pane containing unrelated actions. A role=tablist is an explicit group.
    const compact = text(group).length <= 320 && members.length <= 12;
    if ((group.getAttribute('role') === 'tablist' || compact) &&
        members.some(other => other !== element && analytics.has(text(other)))) return group;
  }
  return null;
};
const candidates = controls.filter(element => overview.has(text(element))).filter(element => {
  if (element.disabled || element.getAttribute('aria-disabled') === 'true') return false;
  if (element.tagName === 'A') {
    const url = new URL(element.getAttribute('href') || '', location.href);
    if (url.origin !== location.origin || url.protocol !== 'https:' || element.hasAttribute('download')) return false;
  }
  return Boolean(groupFor(element));
});
result.candidates = candidates.length;
if (candidates.length !== 1) return finish('unavailable', candidates.length ? 'ambiguous-controls' : 'control-missing');
const target = candidates[0];
const group = groupFor(target);
result.analyticsSelected = controls.some(other => group.contains(other) && analytics.has(text(other)) && selected(other));
if (selected(target)) return finish('ready', 'overview-active');
if (!__ALLOW_CLICK__) return finish('unavailable', 'switch-pending');
target.click();
return finish('selected', 'click-dispatched');
})()".Replace("__ALLOW_CLICK__", allowClick ? "true" : "false")
            .Replace("__OLD_DOCUMENT__", new JavaScriptSerializer().Serialize(previousDocument));

        internal static bool IsChatGptSource(Uri source)
        {
            return source != null && source.Scheme == Uri.UriSchemeHttps &&
                string.Equals(source.Host, "chatgpt.com", StringComparison.OrdinalIgnoreCase);
        }

        internal static async Task<UsageOverviewState> SelectOverviewAsync(CoreWebView2 core)
        {
            Uri source;
            if (core == null || !Uri.TryCreate(core.Source, UriKind.Absolute, out source) || !IsChatGptSource(source))
                return UsageOverviewState.Unavailable;
            return ParseResult(await core.ExecuteScriptAsync(Script));
        }

        internal static UsageOverviewState ParseResult(string json)
        {
            return ParseObservation(json).State;
        }

        internal static UsagePageObservation ParseObservation(string json)
        {
            var observation = new UsagePageObservation();
            if (string.IsNullOrEmpty(json)) return observation;
            var raw = new JavaScriptSerializer().DeserializeObject(json);
            var values = raw as IDictionary<string, object>;
            // Preserve the small result parser contract used by older probes.
            if (raw is string) { observation.State = ParseState((string)raw); return observation; }
            if (values == null) return observation;
            object value;
            if (values.TryGetValue("state", out value)) observation.State = ParseState(value as string);
            if (values.TryGetValue("text", out value)) observation.Text = value as string ?? string.Empty;
            if (values.TryGetValue("documentState", out value))
            {
                var state = value as string;
                observation.DocumentState = state == "complete" || state == "interactive" || state == "loading" ? state : "unknown";
            }
            if (values.TryGetValue("reason", out value)) observation.Reason = SafeReason(value as string);
            if (values.TryGetValue("candidates", out value) && value is int) observation.Candidates = Math.Max(0, Math.Min(100, (int)value));
            if (values.TryGetValue("analyticsSelected", out value)) observation.AnalyticsSelected = value is bool && (bool)value;
            if (values.TryGetValue("trustedOrigin", out value)) observation.TrustedOrigin = value is bool && (bool)value;
            if (values.TryGetValue("documentKey", out value)) observation.DocumentKey = value as string;
            return observation;
        }

        private static UsageOverviewState ParseState(string value)
        {
            return value == "selected" ? UsageOverviewState.Selected :
                value == "ready" ? UsageOverviewState.Ready : UsageOverviewState.Unavailable;
        }

        private static string SafeReason(string value)
        {
            switch (value)
            {
                case "heading-missing": case "ambiguous-controls": case "control-missing":
                case "overview-active": case "switch-pending": case "click-dispatched": return value;
                case "origin-not-ready": case "document-replacing": return value;
                default: return "unavailable";
            }
        }
    }

    internal sealed class UsagePageObservation
    {
        public UsageOverviewState State { get; set; }
        public string Reason { get; set; } = "unavailable";
        public string DocumentState { get; set; } = "unknown";
        public int Candidates { get; set; }
        public bool AnalyticsSelected { get; set; }
        public bool TrustedOrigin { get; set; }
        public string DocumentKey { get; set; }
        public string Text { get; set; } = string.Empty;

        public string Diagnostic => "document=" + DocumentState + " overview=" + State +
            " reason=" + Reason + " candidates=" + Candidates +
            " analyticsActive=" + AnalyticsSelected.ToString().ToLowerInvariant() +
            " trustedOrigin=" + TrustedOrigin.ToString().ToLowerInvariant() + " textChars=" + Text.Length;
    }

    internal sealed class UsagePageReadiness
    {
        private string signature;
        private TimeSpan firstSeen;
        private int samples;
        public bool HasCandidate => signature != null;

        public void Reset() { signature = null; samples = 0; }

        public bool Observe(CodexUsageTrayLite.Models.UsageSnapshot snapshot, TimeSpan elapsed)
        {
            var key = snapshot.weeklyRemainingPercent + ":" + snapshot.fiveHourRemainingPercent + ":" + snapshot.fiveHourLimitAbsenceInferred;
            if (signature != key) { signature = key; firstSeen = elapsed; samples = 0; }
            samples++;
            var delay = snapshot.fiveHourLimitAbsenceInferred ? 2250 : 400;
            return samples >= 2 && (elapsed - firstSeen).TotalMilliseconds >= delay;
        }

        internal static bool ShouldRecover(int attempt, TimeSpan elapsed, bool operationFailed)
        {
            return attempt == 1 && (operationFailed || elapsed >= TimeSpan.FromSeconds(18));
        }
    }
}
