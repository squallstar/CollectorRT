using CollectorRT.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Web.Syndication;

namespace CollectorRT.Data.Downloaders
{
    class RSSDownloader : Downloader
    {
        public static async Task<bool> UpdateSource(Source source)
        {
            try
            {
                foreach (var url in source.Urls)
                {
                    await GetFeedAsync(url, source);
                }
                return true;
            }   
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            return false;
        }

        private static async Task<bool> GetFeedAsync(string feedUrl, Source source)
        {
            SyndicationClient client = new SyndicationClient();
            Uri feedUri = new Uri(feedUrl);

            try
            {
                SyndicationFeed feed = await client.RetrieveFeedAsync(feedUri);

                AddEntriesFromFeed(feed, source, feedUrl);

                //feedData.Title = feed.Title.Text;
            }
            catch (Exception ex)
            {
                // Log Error.
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return false;
            }

            return true;
        }

        private static void AddEntriesFromFeed (SyndicationFeed feed, Source source, string sourceUrl)
        {
            foreach (SyndicationItem item in feed.Items)
            {
                try
                {
                    string itemId = null;
                    string link = null;
                    string title = item.Title.ToString();

                    if (item.Id == null)
                    {
                        if (link != null)
                        {
                            itemId = String.Format("{0}-{1}", source.ID, MD5(link));
                        }
                        else if (item.Title != null)
                        {
                            itemId = String.Format("{0}-{1}", source.ID, MD5(title));
                        }
                        else
                        {
                            itemId = String.Format("{0}-{1}", source.ID, DateTime.Now.ToString());
                        }
                    }
                    else
                    {
                        itemId = String.Format("{0}-{1}", source.ID, MD5(item.Id));
                    }

                    System.Diagnostics.Debug.WriteLine("Item id " + itemId);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                }

                //FeedItem feedItem = new FeedItem();
                //feedItem.Title = item.Title.Text;
                //feedItem.PubDate = item.PublishedDate.DateTime;
                //feedItem.Author = item.Authors[0].Name.ToString();
                //if (feed.SourceFormat == SyndicationFormat.Atom10)
                //{
                //    feedItem.Content = item.Content.Text;
                //}
                //else if (feed.SourceFormat == SyndicationFormat.Rss20)
                //{
                //    feedItem.Content = item.Summary.Text;
                //}
                //feedData.Items.Add(feedItem);
            }
        }
    }
}
