using CollectorRT.Data.Tables;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectorRT.Data.Downloaders
{
    class InstagramDownloader : Downloader
    {
        private static string AppID = "d23ce3a064e34cee8303c76c9c5fc264";
        private static string AppSecret = "cb390114051040009793c6d7bae4bdaf";

        public static async Task<int> UpdateSource(Source source)
        {
            int newArticles = 0;

            try
            {
                string url = String.Format("https://api.instagram.com/v1/users/self/feed?access_token={0}&count={1}", Account.Current.InstagramAccessToken, 40);

                string result = await DownloadContentFromUrl(url);

                var photos = JObject.Parse("{\"x\": " + result + "}")["x"]["data"].ToList();

                newArticles = AddEntries(photos, source);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            return newArticles;

        }

        public static int AddEntries(List<JToken> entries, Source source)
        {
            int i = 0;
            var now = DateTime.Now;

            foreach (var photo in entries)
            {
                try
                {
                    var itemId = source.ID + "-" + photo["id"].ToString();

                    bool exist = DB.Current.entries.Where(e => e.ID == itemId).Any();

                    if (exist)
                    {
                        System.Diagnostics.Debug.WriteLine("Skipping instagram item " + itemId);
                        continue;
                    }

                    DateTime PubDate = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                    PubDate = PubDate.AddSeconds(Int32.Parse(photo["created_time"].ToString())).ToLocalTime();

                    string title = null;
                    if (photo.Value<JObject>("caption") != null)
                    {
                        if (photo["caption"]["text"] != null) title = photo["caption"]["text"].ToString();
                    }

                    var entry = new Entry
                    {
                        ID = itemId,
                        Kind = source.Kind,
                        Source = source.ID,
                        SourceURL = "instagram-" + source.Url,
                        Title = title != null ? title : "Untitled",
                        Link = photo["link"] != null ? photo["link"].ToString() : null,
                        DatePublish = PubDate,
                        DateInsert = now,
                        Author = photo["user"]["full_name"] != null ? photo["user"]["full_name"].ToString() : "",
                        AuthorUsername = photo["user"]["username"].ToString(),
                        AuthorThumbnail = photo["user"]["profile_picture"].ToString(),
                        ThumbnailURL = photo["images"]["standard_resolution"]["url"].ToString(),
                        Fetched = true,
                        ObjectID = photo["id"].ToString()
                    };

                    if (photo["likes"] != null)
                    {
                        if (photo["likes"]["count"] != null)
                        {
                            entry.LikesCount = int.Parse(photo["likes"]["count"].ToString());
                        }
                        else
                        {
                            entry.LikesCount = 0;
                        }
                    }
                    else
                    {
                        entry.LikesCount = 0;
                    }


                    if (photo.Value<JObject>("location") != null)
                    {
                        if (photo["location"]["name"] != null)
                        {
                            entry.Summary = photo["location"]["name"].ToString();
                        }
                    }

                    if (photo["user_has_liked"] != null)
                    {
                        entry.IsLiked = bool.Parse(photo["user_has_liked"].ToString());
                    }

                    if (DB.Current.connection.Insert(photo) > 0)
                    {
                        System.Diagnostics.Debug.WriteLine("Instagram photo saved " + entry.ID);
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
