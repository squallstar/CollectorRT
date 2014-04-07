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

            return i;
        }
    }
}
