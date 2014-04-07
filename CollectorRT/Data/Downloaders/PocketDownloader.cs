using CollectorRT.Data.Tables;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectorRT.Data.Downloaders
{
    class PocketDownloader : Downloader
    {
        private static string ConsumerKey = "15122-eb3e504d583657401a096928";

        private static Dictionary<string, string> GetBaseAuthData()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("consumer_key", ConsumerKey);
            data.Add("access_token", Account.Current.PocketAccessToken);

            return data;
        }

        public static async Task<int> UpdateSource(Source source)
        {
            int newArticles = 0;

            try
            {
                var data = GetBaseAuthData();
                data.Add("count", "45");
                data.Add("state", "all");
                data.Add("sort", "newest");
                data.Add("detailType", "complete");

                string postData = JsonConvert.SerializeObject(data);

                var response = await PostContentToUrl("https://getpocket.com/v3/get", postData);

                var objects = JObject.Parse("{\"data\": " + response + "}")["data"]["list"];

                var list = new List<JToken>();
                foreach (JToken j in objects)
                {
                    list.Add((j as JProperty).Value);
                }

                newArticles += AddEntries(list, source);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            return newArticles;
        }

        private static int AddEntries(List<JToken> entries, Source source)
        {
            int i = 0;
            var now = DateTime.Now;

            foreach (var item in entries)
            {
                try
                {
                    var itemId = source.ID + "-" + item["item_id"].ToString();

                    bool exist = DB.Current.entries.Where(e => e.ID == itemId).Any();

                    if (exist)
                    {
                        System.Diagnostics.Debug.WriteLine("Skipping pocket item " + itemId);
                        continue;
                    }

                    DateTime fakeTime = now.AddSeconds(-i);

                    var entry = new Entry
                    {
                        ID = itemId,
                        Kind = source.Kind,
                        Source = source.ID,
                        SourceURL = "pocket-" + source.Url,
                        Link = item["resolved_url"] != null ? item["resolved_url"].ToString() : item["normal_url"].ToString(),
                        DatePublish = fakeTime,
                        DateInsert = fakeTime,
                        ObjectID = item["item_id"].ToString(),
                        Summary = item["excerpt"] != null ? item["excerpt"].ToString() : ""
                    };

                    if (item["title"] != null)
                    {
                        entry.Title = item["title"].ToString();
                    }
                    else if (item["given_title"] != null)
                    {
                        entry.Title = item["given_title"].ToString();
                    }

                    if (item["has_image"].ToString() == "1" && item["images"] != null)
                    {
                        foreach (var x in item["images"])
                        {
                            entry.ThumbnailURL = x.FirstOrDefault()["src"].ToString();
                            entry.Fetched = true;
                            break;
                        }
                    }

                    if (DB.Current.connection.Insert(entry) > 0)
                    {
                        System.Diagnostics.Debug.WriteLine("Pocket item inserted " + entry.ID);
                        i++;
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                }
            }

            return i;
        }
    }
}
