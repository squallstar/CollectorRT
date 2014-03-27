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
            SyndicationClient client = new SyndicationClient
            {
                Timeout = 8000
            };

            Uri feedUri = new Uri(feedUrl);

            try
            {
                System.Diagnostics.Debug.WriteLine("Getting feed " + feedUrl);

                SyndicationFeed feed = await client.RetrieveFeedAsync(feedUri);

                AddEntriesFromFeed(feed, source, feedUrl);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return false;
            }

            return true;
        }

        private static void AddEntriesFromFeed (SyndicationFeed feed, Source source, string sourceUrl)
        {
            int i = 0;

            foreach (SyndicationItem item in feed.Items)
            {
                try
                {
                    string itemId = null;
                    string link = null;
                    string title = item.Title.ToString();

                    if (item.Links.Count > 0)
                    {
                        foreach (var l in item.Links)
                        {
                            if (l != null && l.Uri != null && l.Uri.IsAbsoluteUri)
                            {
                                link = l.Uri.AbsoluteUri;
                                break;
                            }
                        }
                    }

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

                    bool exist = DB.Current.entries.Where(e => e.ID == itemId).Any();

                    if (exist)
                    {
                        System.Diagnostics.Debug.WriteLine("Skipping entry " + itemId);
                        continue;
                    }

                    i++;

                    Entry entry = new Entry
                    {
                        ID = itemId,
                        Kind = "rss",
                        Title = title,
                        Link = link,
                        ThumbnailHasBeenDownloaded = false,
                        DateInsert = DateTime.Now,
                        SourceURL = sourceUrl
                    };

                    if (item.Summary != null)
                    {
                        entry.Summary = item.Summary.ToString();
                    }

                    if (item.PublishedDate != null && item.PublishedDate.Ticks > 0)
                    {
                        entry.DatePublish = item.PublishedDate.DateTime;
                    }
                    else
                    {
                        entry.DatePublish = entry.DateInsert;
                    }

                    if (feed.SourceFormat == SyndicationFormat.Atom10)
                    {
                        entry.ContentText = item.Content.Text;
                    }
                    else if (feed.SourceFormat == SyndicationFormat.Rss20)
                    {
                        entry.ContentText = entry.Summary;
                    }

                    DB.Current.connection.Insert(entry);

                    System.Diagnostics.Debug.WriteLine("Article inserted " + entry.ID);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("Cannot add rss entry: " + e.Message);
                }
            }

            System.Diagnostics.Debug.WriteLine(String.Format("{0} articles added for source {1}", i, sourceUrl));
        }
    }
}
