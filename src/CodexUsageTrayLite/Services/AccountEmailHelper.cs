using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.Script.Serialization;
using CodexUsageTrayLite.Infrastructure;
using CodexUsageTrayLite.Models;

namespace CodexUsageTrayLite.Services
{
    internal static class AccountEmailHelper
    {
        private const int MaximumSessionMessageLength = 4096;
        private static readonly JavaScriptSerializer Serializer = new JavaScriptSerializer();

        public static bool TryReadCliAccount(object account, out string email)
        {
            UserLevel unused;
            return TryReadCliAccount(account, out email, out unused);
        }

        public static bool TryReadCliAccount(object account, out string email, out UserLevel userLevel)
        {
            email = null;
            userLevel = UserLevel.Unknown;
            var values = account as IDictionary<string, object>;
            if (values == null) return false;

            object rawEmail;
            if (values.TryGetValue("email", out rawEmail) && rawEmail != null)
            {
                email = AccountEmailPrivacy.Normalize(Convert.ToString(rawEmail, CultureInfo.InvariantCulture));
            }
            userLevel = UserLevelHelper.ReadFromDictionary(values);
            return true;
        }

        public static string BuildWebViewScript(string messagePrefix)
        {
            if (string.IsNullOrEmpty(messagePrefix)) throw new ArgumentException("A message prefix is required.", "messagePrefix");
            var prefixJson = Serializer.Serialize(messagePrefix);
            return @"(async function () {
                const prefix = " + prefixJson + @";
                const send = value => window.chrome.webview.postMessage(prefix + JSON.stringify(value));
                const controller = new AbortController();
                const timeout = setTimeout(() => controller.abort(), 5000);
                try {
                    const response = await fetch('/api/auth/session', {
                        method: 'GET',
                        credentials: 'include',
                        cache: 'no-store',
                        redirect: 'error',
                        signal: controller.signal
                    });
                    const contentType = response.headers.get('content-type') || '';
                    if (!response.ok || contentType.toLowerCase().indexOf('application/json') < 0) {
                        send({ resolved: false, email: null });
                        return;
                    }
                    const text = await response.text();
                    if (text.length > 32768) {
                        send({ resolved: false, email: null });
                        return;
                    }
                    const data = JSON.parse(text);
                    let email = null;
                    if (data && data.user && typeof data.user.email === 'string') email = data.user.email;
                    else if (data && typeof data.email === 'string') email = data.email;
                    else if (data && data.account && typeof data.account.email === 'string') email = data.account.email;
                    const planCandidates = [
                        data && data.user && data.user.planType,
                        data && data.user && data.user.plan_type,
                        data && data.user && data.user.plan,
                        data && data.user && data.user.subscription && data.user.subscription.planType,
                        data && data.user && data.user.subscription && data.user.subscription.plan_type,
                        data && data.user && data.user.subscription && data.user.subscription.plan,
                        data && data.account && data.account.planType,
                        data && data.account && data.account.plan_type,
                        data && data.account && data.account.plan,
                        data && data.account && data.account.subscription && data.account.subscription.planType,
                        data && data.account && data.account.subscription && data.account.subscription.plan_type,
                        data && data.account && data.account.subscription && data.account.subscription.plan,
                        data && data.subscription && data.subscription.planType,
                        data && data.subscription && data.subscription.plan_type,
                        data && data.subscription && data.subscription.plan,
                        data && data.planType,
                        data && data.plan_type
                    ];
                    const userLevel = planCandidates.find(value => typeof value === 'string') || null;
                    send({ resolved: true, email: email, userLevel: userLevel });
                } catch (_) {
                    send({ resolved: false, email: null });
                } finally {
                    clearTimeout(timeout);
                }
            })();";
        }

        public static bool TryReadWebViewMessage(string message, string messagePrefix, out string email)
        {
            UserLevel unused;
            return TryReadWebViewMessage(message, messagePrefix, out email, out unused);
        }

        public static bool TryReadWebViewMessage(
            string message,
            string messagePrefix,
            out string email,
            out UserLevel userLevel)
        {
            email = null;
            userLevel = UserLevel.Unknown;
            if (string.IsNullOrEmpty(message) || string.IsNullOrEmpty(messagePrefix) ||
                message.Length > messagePrefix.Length + MaximumSessionMessageLength ||
                !message.StartsWith(messagePrefix, StringComparison.Ordinal))
            {
                return false;
            }

            try
            {
                var values = Serializer.DeserializeObject(message.Substring(messagePrefix.Length)) as IDictionary<string, object>;
                if (values == null) return false;
                object rawResolved;
                bool resolved;
                if (!values.TryGetValue("resolved", out rawResolved) ||
                    !bool.TryParse(Convert.ToString(rawResolved, CultureInfo.InvariantCulture), out resolved) || !resolved)
                {
                    return false;
                }

                object rawEmail;
                if (values.TryGetValue("email", out rawEmail) && rawEmail != null)
                {
                    email = AccountEmailPrivacy.Normalize(Convert.ToString(rawEmail, CultureInfo.InvariantCulture));
                }
                object rawUserLevel;
                if (values.TryGetValue("userLevel", out rawUserLevel))
                {
                    userLevel = UserLevelHelper.Parse(rawUserLevel);
                }
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

    }
}
