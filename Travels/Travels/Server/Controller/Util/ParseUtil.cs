using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Travels.Server.Controller.Util
{
    internal static class ParseUtil
    {
        public delegate bool TryParseDelegate<T>(string str, out T value);

        public static bool TryGetIdFromUrl(string url, out int id)
        {
            id = 0;

            var idx1 = url.IndexOf('/');
            if (idx1 == -1)
                return false;

            var idx2 = url.IndexOf('/', idx1 + 1);
            if (idx2 == -1)
                idx2 = url.IndexOf('?', idx1 + 1);

            string idStr;
            if (idx2 == -1)
                idStr = url.Substring(idx1 + 1);
            else
                idStr = url.Substring(idx1 + 1, idx2 - idx1 - 1);

            return int.TryParse(idStr, out id);
        }

        public static Dictionary<string, string> ParseQueryString(string url)
        {
            var idx = url.IndexOf('?');
            if (idx == -1 || idx == url.Length - 1)
                return new Dictionary<string, string>();

            var pairs = url.Substring(idx + 1).Split('&', StringSplitOptions.RemoveEmptyEntries);
            var result = new Dictionary<string, string>(pairs.Length);

            foreach (var pair in pairs)
            {
                var idx2 = pair.IndexOf('=');
                if (idx2 == -1 || idx2 == pair.Length - 1)
                {
                    result[pair] = null;
                }
                else
                {
                    result[pair.Substring(0, idx2)] = pair.Substring(idx2 +1, pair.Length - idx2 - 1);
                }
            }

            return result;
        }

        public static bool TryGetValueFromPayload<T>(JToken payload, string key, TryParseDelegate<T> tryParseDelegate, out T? value) where T : struct 
        {
            value = null;

            var jtoken = payload[key];
            if (jtoken == null)
                return true;

            var valueStr = jtoken.Value<string>();
            if (string.IsNullOrEmpty(valueStr))
                return false;

            if (tryParseDelegate(valueStr, out var valueTmp))
            {
                value = valueTmp;
                return true;
            }

            return false;
        }

        public static bool TryGetStringValueFromPayload(JToken payload, string key, out string value)
        {
            value = null;

            var jtoken = payload[key];
            if (jtoken == null)
                return true;

            value = jtoken.Value<string>();
            if (string.IsNullOrEmpty(value))
                return false;

            value = Uri.UnescapeDataString(value).Replace('+', ' ');
            return true;
        }
    }
}
