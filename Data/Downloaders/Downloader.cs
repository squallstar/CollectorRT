using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CollectorRT.Data.Tables;
using System.Text.RegularExpressions;
using System.Net.Http;

namespace CollectorRT.Data
{
    public class Downloader
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

        public static async Task<string> DownloadContentFromUrl(string url)
        {
            using (var client = new HttpClient())
            {
                return await client.GetStringAsync(url);                
            }
        }

        public static string CleanString(string value, int maxLength = 1000)
        {
            if (value == null) return null;

            int strLength = 0;
            string fixedString = "";

            fixedString = StripHtml(Regex.Replace(value.ToString(), "<br ?/?>|</p>", "\r\n"));

            // Remove encoded HTML characters.
            //fixedString = HttpUtility.HtmlDecode(fixedString).Trim();

            strLength = fixedString.ToString().Length;

            // Some feed management tools include an image tag in the Description field of an RSS feed, 
            // so even if the Description field (and thus, the Summary property) is not populated, it could still contain HTML. 
            // Due to this, after we strip tags from the string, we should return null if there is nothing left in the resulting string. 
            if (strLength == 0)
            {
                return null;
            }

            // Truncate the text if it is too long. 
            else if (strLength > maxLength)
            {
                try
                {
                    fixedString = fixedString.Substring(0, maxLength);

                    // Unless we take the next step, the string trunrancation could occur in the middle of a word.
                    // Using LastIndexOf we can find the last space character in the string and truncate there. 
                    int idx = fixedString.LastIndexOf(" ");
                    if (fixedString.Length > idx)
                    {
                        fixedString = fixedString.Substring(0, idx);
                    }
                }
                catch (Exception)
                {
                    //Nothing
                }

                fixedString += "...";
            }

            return fixedString;
        }
    }
}
