using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CollectorRT.Data.Tables;
using System.Text.RegularExpressions;

namespace CollectorRT.Data
{
    class Downloader
    {
        protected static int maxTextLength = 1000;

        protected static string MD5(string str)
        {
            return MD5CryptoServiceProvider.GetMd5String(str);
        }

        protected static string StripHtml(string html)
        {
            return Regex.Replace(html, "<.+?>", string.Empty);
        }

        public static string FirstImageFromText(string text)
        {
            if (text == null) return null;

            // (https?:\\/\\/[^'\">]+\\.(?:png|jpg|jpeg))
            // ((?!.*/(twitter|facebook|linkedin|googleplus|email))https?://[^'\"> \r]+\\.(?:png|jpg|jpeg))

            Match match = Regex.Match(text, "((?!.*/(twitter|facebook|linkedin|googleplus|email))https?://[^'\"> \r]+\\.(?:png|jpg|jpeg))",
                RegexOptions.IgnoreCase);

            // Here we check the Match instance.
            if (match.Success)
            {
                // Finally, we get the Group value and display it.
                string key = match.Groups[1].Value;
                if (key.Length > 255) return null;
                return key;
            }

            return null;
        }
    }
}
