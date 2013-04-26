using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PointGaming
{
    public struct UrlMatch
    {
        public int Offset;
        public string Text;
    }

    public class UrlMatcher
    {
        public static readonly string[] GenericTLDs = new string[]
        {
            "edu", "gov", "mil", "aero", "asia", "biz", "cat", "com", "coop", "info", "int", "jobs",
            "mobi", "museum", "name", "net", "org", "post", "pro", "tel", "travel", "xxx",
        };
        public static readonly string[] CountryTLDs = new string[]
        {
            "ac", "ad", "ae", "af", "ag", "ai", "al", "am", "an", "ao", "aq", "ar", "as", "at", "au",
            "aw", "ax", "az", "ba", "bb", "bd", "be", "bf", "bg", "bh", "bi", "bj", "bm", "bn", "bo",
            "br", "bs", "bt", "bv", "bw", "by", "bz", "ca", "cc", "cd", "cf", "cg", "ch", "ci", "ck",
            "cl", "cm", "cn", "co", "cr", "cs", "cu", "cv", "cx", "cy", "cz", "dd", "de", "dj", "dk",
            "dm", "do", "dz", "ec", "ee", "eg", "eh", "er", "es", "et", "eu", "fi", "fj", "fk", "fm",
            "fo", "fr", "ga", "gb", "gd", "ge", "gf", "gg", "gh", "gi", "gl", "gm", "gn", "gp", "gq",
            "gr", "gs", "gt", "gu", "gw", "gy", "hk", "hm", "hn", "hr", "ht", "hu", "id", "ie", "il",
            "im", "in", "io", "iq", "ir", "is", "it", "je", "jm", "jo", "jp", "ke", "kg", "kh", "ki",
            "km", "kn", "kp", "kr", "kw", "ky", "kz", "la", "lb", "lc", "li", "lk", "lr", "ls", "lt",
            "lu", "lv", "ly", "ma", "mc", "md", "me", "mg", "mh", "mk", "ml", "mm", "mn", "mo", "mp",
            "mq", "mr", "ms", "mt", "mu", "mv", "mw", "mx", "my", "mz", "na", "nc", "ne", "nf", "ng",
            "ni", "nl", "no", "np", "nr", "nu", "nz", "om", "pa", "pe", "pf", "pg", "ph", "pk", "pl",
            "pm", "pn", "pr", "ps", "pt", "pw", "py", "qa", "re", "ro", "rs", "ru", "rw", "sa", "sb",
            "sc", "sd", "se", "sg", "sh", "si", "sj", "sk", "sl", "sm", "sn", "so", "sr", "ss", "st",
            "su", "sv", "sx", "sy", "sz", "tc", "td", "tf", "tg", "th", "tj", "tk", "tl", "tm", "tn",
            "to", "tp", "tr", "tt", "tv", "tw", "tz", "ua", "ug", "uk", "us", "uy", "uz", "va", "vc",
            "ve", "vg", "vi", "vn", "vu", "wf", "ws", "ye", "yt", "yu", "za", "zm", "zw",
        };

        public static readonly char[] WhitespaceCharacters = new char[]
        {
            ' ', '\r', '\n', '\t', '\v', '\f'
        };

        public static readonly char[] DomainNameEndCharacters = new char[]
        {
            '/'
        };

        public static bool TryGetMatch(string mine, int startOffset, out UrlMatch urlMatch)
        {
            var regex = new Regex("http[s]?://[^\\s]*");
            var match = regex.Match(mine, startOffset);
            if (match.Success)
            {
                var result = match.Value;
                if (result.EndsWith(".") || result.EndsWith("?") || result.EndsWith("!"))
                    result = result.Substring(0, result.Length - 1);
                urlMatch = new UrlMatch{ Offset = match.Index, Text = result };
                return true;
            }
            urlMatch = new UrlMatch();
            return false;
        }
    }
}
