using CollectorRT.Data.Tables;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectorRT.Data.Downloaders
{
    class TumblrDownloader : Downloader
    {
        private static string ConsumerKey = "wJ4QXyzoTxTSWYuqRiArCpXn00uAl4nNDRffsLS2il6cL17uUt";

        public static async Task<bool> UpdateSource(Source source)
        {
            try
            {
                string url = String.Format("http://api.tumblr.com/v2/blog/{0}.tumblr.com/posts?api_key={1}&limit=20", source.Url, ConsumerKey);
                var data = await DownloadContentFromUrl(url);
                var posts = JObject.Parse("{\"data\": " + data + "}")["data"]["posts"].ToList();

                AddEntries(posts);

                return true;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            return false;
        }

        private static void AddEntries(List<JToken> posts)
        {

        }
    }
}
