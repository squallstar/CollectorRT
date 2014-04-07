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

        public static async Task<int> UpdateSource(Source source)
        {
            int newArticles = 0;

            try
            {
                System.Diagnostics.Debug.WriteLine("Downloading feed for tumblr blog " + source.Url);

                string url = String.Format("http://api.tumblr.com/v2/blog/{0}.tumblr.com/posts?api_key={1}&limit=20", source.Url, ConsumerKey);
                var data = await DownloadContentFromUrl(url);
                var posts = JObject.Parse("{\"data\": " + data + "}")["data"]["response"]["posts"].ToList();

                newArticles += AddEntries(posts, source);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            return newArticles;
        }

        private static int AddEntries(List<JToken> posts, Source source)
        {
            int i = 0;

            foreach (var post in posts)
            {
                var itemId = source.ID + "-" + post["id"].ToString();

                bool exist = DB.Current.entries.Where(e => e.ID == itemId).Any();

                if (exist)
                {
                    System.Diagnostics.Debug.WriteLine("Skipping tumblr entry " + itemId);
                    continue;
                }

                var title = post["title"] != null ? post["title"].ToString().Trim() : null;

                DateTime PubDate = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                PubDate = PubDate.AddSeconds(Int32.Parse(post["timestamp"].ToString())).ToLocalTime();

                string body = null;

                if (post["body"] != null)
                {
                    body = CleanString(post["body"].ToString(), 1000);
                }

                Entry entry = new Entry
                {
                    ID = itemId,
                    Kind = "tumblr",
                    Source = source.ID,
                    SourceURL = "tumblr-" + source.Url,
                    Title = title != null ? title : "Untitled",
                    Fetched = false,
                    Link = post["post_url"] != null ? post["post_url"].ToString() : null,
                    SourceTitle = post["source_title"] != null ? post["source_title"].ToString() : null,
                    DatePublish = PubDate,
                    DateInsert = DateTime.Now,
                    ContentText = body,
                    Author = source.Title,
                    AuthorUsername = source.Title
                };

                if (entry.ContentText != null) entry.Summary = body.Length > 140 ? body.Substring(0, 139) + "..." : null;                

                if (DB.Current.connection.Insert(entry) > 0)
                {
                    System.Diagnostics.Debug.WriteLine("Tumblr post inserted " + entry.ID);
                    i++;
                }
            }

            return i;
        }
    }
}
