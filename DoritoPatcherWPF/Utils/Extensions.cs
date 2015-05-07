using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DoritoPatcherWPF.Utils
{
    public static class Extensions
    {

        private static readonly Regex _regex = new Regex(@"[?|&]([\w\.]+)=([^?|^&]+)");

        public static Dictionary<string, string> ParseQueryString(this Uri uri)
        {
            var match = _regex.Match(uri.PathAndQuery);
            var paramaters = new Dictionary<string, string>();
            while (match.Success)
            {
                paramaters.Add(match.Groups[1].Value, match.Groups[2].Value);
                match = match.NextMatch();
            }
            return paramaters;
        }
        
    }
}
