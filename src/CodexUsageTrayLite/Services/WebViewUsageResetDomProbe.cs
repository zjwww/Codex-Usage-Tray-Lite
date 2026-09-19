using System;
using System.Drawing;
using System.Globalization;
using System.Web.Script.Serialization;

namespace CodexUsageTrayLite.Services
{
    internal static class WebViewUsageResetDomProbe
    {
        public const int MaximumAttempts = 12;
        public const int RetryDelayMilliseconds = 250;

        public static Size BackgroundViewportSize
        {
            get { return new Size(1280, 900); }
        }

        public static string Script
        {
            get
            {
                return @"(() => {
const normalize = value => (value || '').normalize('NFKC').replace(/[\s\u00a0\u3000]+/g, ' ').trim();
const key = value => normalize(value).toLowerCase();
const ownText = element => normalize(Array.from(element.childNodes || [])
  .filter(node => node.nodeType === Node.TEXT_NODE)
  .map(node => node.textContent || '')
  .join(' '));
const fullText = element => normalize(element.innerText || element.textContent || '');
const headingKeys = new Set([
  'Usage limit resets',
  '用量限制重置', '用量上限重置', '使用限额重置', '使用限制重置', '使用量限制重置',
  '用量限制重設', '用量上限重設', '用量限額重設', '使用限額重設', '使用限制重設', '使用量限制重設',
  '사용량 한도 재설정', '사용 한도 재설정', '사용량 제한 재설정',
  '사용량 한도 초기화', '사용 한도 초기화', '사용량 제한 초기화',
  '使用制限のリセット', '使用量制限のリセット', '利用制限のリセット', '利用上限のリセット'
].map(key));
const boundaryKeys = new Set([
  'Auto reload', 'Usage breakdown',
  '自动充值', '自动重新加载', '自动加值', '用量明细', '使用明细', '使用详情',
  '自動加值', '自動儲值', '自動重新載入', '用量明細', '使用明細', '使用詳情',
  '자동 충전', '자동 새로 고침', '사용량 분석', '사용량 내역', '사용량 세부 정보',
  '自動リロード', '自動再読み込み', '自動チャージ', '使用量の内訳', '使用状況の内訳', '使用量の詳細', '使用状況の詳細'
].map(key));
const actionKeys = new Set([
  'Use reset',
  '使用重置', '使用此重置', '应用重置', '使用重设', '使用此重设',
  '使用重設', '使用此重設', '應用重設', '套用重設',
  '재설정 사용', '초기화 사용', '사용 재설정', '사용 초기화', '사용',
  'リセットを使用', 'リセット使用', '使用する'
].map(key));
const availableCountPatterns = [
  /^available\s*[:：]?\s*[\(\[]?(\d{1,9})[\)\]]?$/,
  /^可用\s*[:：]?\s*[\(\[]?(\d{1,9})[\)\]]?$/,
  /^사용\s*가능\s*[:：]?\s*[\(\[]?(\d{1,9})[\)\]]?$/,
  /^(?:利用|使用)可能\s*[:：]?\s*[\(\[]?(\d{1,9})[\)\]]?$/
];
const elements = Array.from(document.querySelectorAll('body *'));
const heading = elements.find(element => headingKeys.has(key(ownText(element)))) ||
  elements.find(element => headingKeys.has(key(fullText(element))));
if (!heading) {
  const bodyHeight = document.body ? document.body.scrollHeight : 0;
  const rootHeight = document.documentElement ? document.documentElement.scrollHeight : 0;
  window.scrollTo(0, Math.max(bodyHeight, rootHeight));
  return null;
}
heading.scrollIntoView({ block: 'center', inline: 'nearest' });
const follows = (left, right) => Boolean(left.compareDocumentPosition(right) & Node.DOCUMENT_POSITION_FOLLOWING);
const boundary = elements.find(element => {
  if (!follows(heading, element)) return false;
  return boundaryKeys.has(key(ownText(element)));
});
const isInsideSection = element => follows(heading, element) && (!boundary || follows(element, boundary));
for (const element of elements) {
  if (!isInsideSection(element)) continue;
  const label = key(fullText(element));
  for (const pattern of availableCountPatterns) {
    const match = pattern.exec(label);
    if (match) return Number(match[1]);
  }
}
const actions = Array.from(document.querySelectorAll('button,[role=""button""],a'))
  .filter(element => {
    if (!isInsideSection(element) || !actionKeys.has(key(fullText(element)))) return false;
    if (element.disabled || (element.getAttribute('aria-disabled') || '').toLowerCase() === 'true') return false;
    const style = window.getComputedStyle(element);
    return style.display !== 'none' && style.visibility !== 'hidden';
  });
if (actions.length > 0) return actions.length;
const explicitlyEmpty = elements.some(element => {
  if (!isInsideSection(element)) return false;
  const text = key(fullText(element));
  return /^(?:no|0) (?:(?:usage limit|banked|usage|codex) )?resets? (?:available|remaining)(?: at this time)?[.!]?$/.test(text) ||
    /^(?:目前|当前|现在)?\s*(?:没有|无)\s*(?:任何)?\s*(?:可用的?)?\s*(?:(?:用量|使用量|使用)(?:限制|上限|限额)?\s*)?(?:重置|重设)(?:可用)?[。.!！]?$/.test(text) ||
    /^(?:目前|當前|現在)?\s*(?:沒有|無)\s*(?:任何)?\s*(?:可用的?)?\s*(?:(?:用量|使用量|使用)(?:限制|上限|限額)?\s*)?(?:重置|重設)(?:可用)?[。.!！]?$/.test(text) ||
    /^현재\s*(?:사용\s*가능한\s*)?(?:(?:사용량|사용)\s*(?:한도|제한)\s*)?(?:재설정|초기화)(?:이|가)?\s*없습니다[.!]?$/.test(text) ||
    /^(?:現在|現時)?\s*(?:利用|使用)可能な?\s*(?:(?:使用量|利用)(?:制限|上限)の?\s*)?リセット(?:は|が)?\s*(?:ありません|ない)[。.!！]?$/.test(text);
});
return explicitlyEmpty ? 0 : null;
})()";
            }
        }

        public static int? ParseCount(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            object raw;
            try
            {
                raw = new JavaScriptSerializer().DeserializeObject(json);
            }
            catch (ArgumentException)
            {
                return null;
            }
            if (raw == null) return null;
            try
            {
                var count = Convert.ToDecimal(raw, CultureInfo.InvariantCulture);
                if (count < 0 || count > int.MaxValue || count != decimal.Truncate(count)) return null;
                return decimal.ToInt32(count);
            }
            catch (Exception ex) when (ex is FormatException || ex is InvalidCastException || ex is OverflowException)
            {
                return null;
            }
        }
    }
}
