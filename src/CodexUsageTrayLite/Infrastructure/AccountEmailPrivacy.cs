using System;
using CodexUsageTrayLite.Models;

namespace CodexUsageTrayLite.Infrastructure
{
    internal static class AccountEmailPrivacy
    {
        private const int MaximumEmailLength = 320;

        public static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var email = value.Trim();
            if (email.Length > MaximumEmailLength) return null;
            var at = email.IndexOf('@');
            if (at <= 0 || at != email.LastIndexOf('@') || at >= email.Length - 1) return null;
            foreach (var character in email)
            {
                if (char.IsControl(character) || char.IsWhiteSpace(character)) return null;
            }
            return email;
        }

        public static string MaskForDisplay(string email)
        {
            var normalized = Normalize(email);
            if (normalized == null) return null;
            var at = normalized.IndexOf('@');
            return normalized.Substring(0, 1) + "***" + normalized.Substring(at);
        }

        public static string FormatMenu(string email)
        {
            return FormatMenu(email, AppLanguage.English);
        }

        public static string FormatMenu(string email, AppLanguage language)
        {
            return UiText.For(language).FormatAccountEmail(email);
        }

    }
}
