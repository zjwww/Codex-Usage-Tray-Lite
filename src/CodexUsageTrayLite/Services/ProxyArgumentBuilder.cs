using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using CodexUsageTrayLite.Models;

namespace CodexUsageTrayLite.Services
{
    internal static class ProxyArgumentBuilder
    {
        private static readonly string[] ProxySwitches =
        {
            "--proxy-server", "--no-proxy-server", "--proxy-pac-url",
            "--proxy-auto-detect", "--proxy-bypass-list"
        };

        public static string Build(ProxySettings settings)
        {
            settings = settings ?? new ProxySettings { mode = ProxyMode.System };
            switch (settings.mode)
            {
                case ProxyMode.System:
                    return string.Empty;
                case ProxyMode.Direct:
                    return "--no-proxy-server";
                case ProxyMode.Http:
                    Validate(settings);
                    return "--proxy-server=http://" + FormatHost(settings.host) + ":" + settings.port.ToString(CultureInfo.InvariantCulture);
                case ProxyMode.Socks5:
                    Validate(settings);
                    return "--proxy-server=socks5://" + FormatHost(settings.host) + ":" + settings.port.ToString(CultureInfo.InvariantCulture);
                default:
                    throw new ArgumentOutOfRangeException("settings.mode");
            }
        }

        public static void Validate(ProxySettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }
            if (settings.mode != ProxyMode.Http && settings.mode != ProxyMode.Socks5)
            {
                return;
            }
            var host = (settings.host ?? string.Empty).Trim();
            if (host.Length == 0 || host.Length > 253 || host.IndexOfAny(new[] { ' ', '\t', '\r', '\n', '"', '\'', '/', '\\', '=' }) >= 0)
            {
                throw new ArgumentException("Host must be a DNS name or IP address.", "settings.host");
            }
            IPAddress address;
            if (!IPAddress.TryParse(host.Trim('[', ']'), out address) && Uri.CheckHostName(host) == UriHostNameType.Unknown)
            {
                throw new ArgumentException("Host must be a DNS name or IP address.", "settings.host");
            }
            if (settings.port < 1 || settings.port > 65535)
            {
                throw new ArgumentOutOfRangeException("settings.port", "Port must be between 1 and 65535.");
            }
        }

        public static string SanitizeExternalArguments(string arguments, out bool proxyArgumentsDetected)
        {
            proxyArgumentsDetected = false;
            var kept = new List<string>();
            var tokens = Tokenize(arguments);
            foreach (var token in tokens)
            {
                if (IsProxySwitch(token))
                {
                    proxyArgumentsDetected = true;
                    continue;
                }
                kept.Add(QuoteIfNeeded(token));
            }
            return string.Join(" ", kept.ToArray());
        }

        private static bool IsProxySwitch(string token)
        {
            foreach (var key in ProxySwitches)
            {
                if (string.Equals(token, key, StringComparison.OrdinalIgnoreCase) ||
                    token.StartsWith(key + "=", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        private static List<string> Tokenize(string value)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(value))
            {
                return result;
            }
            var builder = new StringBuilder();
            var quoted = false;
            for (var i = 0; i < value.Length; i++)
            {
                var character = value[i];
                if (character == '"')
                {
                    quoted = !quoted;
                    continue;
                }
                if (char.IsWhiteSpace(character) && !quoted)
                {
                    if (builder.Length > 0)
                    {
                        result.Add(builder.ToString());
                        builder.Clear();
                    }
                    continue;
                }
                builder.Append(character);
            }
            if (builder.Length > 0)
            {
                result.Add(builder.ToString());
            }
            return result;
        }

        private static string QuoteIfNeeded(string value)
        {
            return value.IndexOf(' ') >= 0 ? "\"" + value.Replace("\"", string.Empty) + "\"" : value;
        }

        private static string FormatHost(string host)
        {
            var value = host.Trim().Trim('[', ']');
            return value.IndexOf(':') >= 0 ? "[" + value + "]" : value;
        }
    }
}
