using CollectorRT.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsyncOAuth;
using Newtonsoft.Json.Linq;

namespace CollectorRT.Data.Downloaders
{
    class TwitterDownloader : Downloader
    {
        private static string ConsumerKey = "OzHmVZmElz2xh20nBezHiw";
        private static string ConsumerSecret = "wJqaPwVr19lZ5J7SxJxmUJmyu5lOYzg5NOi1KZ2o";

        public static async Task<int> UpdateSource(Source source)
        {
            int newArticles = 0;

            try
            {
                string data = "";

                if (source.Kind == "twitter")
                {
                    System.Diagnostics.Debug.WriteLine("Downloading twitter home timeline.");
                    data = await GetUserHomeTimeline();
                }
                else if (source.Kind == "twitter-user")
                {
                    System.Diagnostics.Debug.WriteLine("Downloading twitter user [" + source.Url + "] timeline");
                    data = await GetUserTweets(source.Url);
                }
                else
                {
                    return newArticles;
                }

                var tweets = JObject.Parse("{\"data\": " + data + "}")["data"].ToList();

                newArticles += AddEntries(tweets, source);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            return newArticles;
        }

        private static System.Net.Http.HttpClient ClientForCurrentUser
        {
            get
            {
                return OAuthUtility.CreateOAuthClient(ConsumerKey, ConsumerSecret, Account.Current.TwitterAccessToken);
            }
        }

        private static async Task<string> GetUserHomeTimeline()
        {
            return await ClientForCurrentUser.GetStringAsync("https://api.twitter.com/1.1/statuses/home_timeline.json?count=40&page=1");
        }

        private static async Task<string> GetUserTweets(string screen_name)
        {
            return await ClientForCurrentUser.GetStringAsync("https://api.twitter.com/1.1/statuses/user_timeline.json?count=40&page=1&include_entities=true&exclude_replies=false&screen_name=" + screen_name);
        }

        private static int AddEntries(List<JToken> tweets, Source source)
        {
            int i = 0;

            var now = DateTime.Now;

            foreach (var status in tweets)
            {
                var id = String.Format("{0}-{1}", source.ID, (string)status["id"]);

                bool exist = DB.Current.entries.Where(e => e.ID == id).Any();

                if (exist)
                {
                    System.Diagnostics.Debug.WriteLine("Skipping tweet " + id);
                    continue;
                }

                try
                {
                    var tweet = new Entry
                    {
                        Kind = "tweet",
                        ID = id,
                        Title = System.Net.WebUtility.HtmlDecode((string)status["text"]),
                        DatePublish = ParseTwitterDateTime((string)status["created_at"]),
                        DateInsert = now,
                        Author = (string)status["user"]["name"],
                        AuthorUsername = (string)status["user"]["screen_name"],
                        AuthorThumbnail = ((string)status["user"]["profile_image_url"]).Replace("_normal", "_bigger"),
                        Link = "https://twitter.com/" + (string)status["user"]["screen_name"]
                             + "/status/" + id
                        ,
                        Source = source.ID,
                        Fetched = false,
                        SourceURL = source.Kind
                    };

                    if (status["entities"] != null)
                    {
                        //Image
                        if (status["entities"]["media"] != null)
                        {
                            foreach (JToken img in status["entities"]["media"].Children())
                            {
                                tweet.ThumbnailURL = (string)img["media_url"];
                                break;
                            }
                        }

                        //URL
                        if (status["entities"]["urls"] != null)
                        {
                            foreach (JToken url in status["entities"]["urls"].Children())
                            {
                                tweet.Link = (string)url["expanded_url"];
                                break;
                            }
                        }

                        if (tweet.Link == null)
                        {
                            //It's no possible to gather a thumbnail, There's no link!
                            tweet.Fetched = true;
                        }
                    }

                    if (DB.Current.connection.Insert(tweet) > 0)
                    {
                        System.Diagnostics.Debug.WriteLine("Tweet added " + tweet.ID);
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

        public static DateTime ParseTwitterDateTime(string date)
        {
            const string format = "ddd MMM dd HH:mm:ss zzzz yyyy";
            return DateTime.ParseExact(date, format, System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
