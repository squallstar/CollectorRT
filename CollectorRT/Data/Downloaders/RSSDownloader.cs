using CollectorRT.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Web.Syndication;

namespace CollectorRT.Data.Downloaders
{
    class RSSDownloader : Downloader
    {
        public static async Task<int> UpdateSource(Source source)
        {
            int newArticles = 0;

            try
            {
                foreach (var url in source.Urls)
                {
                    newArticles += await GetFeedAsync(url, source);
                }
            }   
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            return newArticles;
        }

        private static async Task<int> GetFeedAsync(string feedUrl, Source source)
        {
            int newArticles = 0;

            SyndicationClient client = new SyndicationClient
            {
                Timeout = 6000
            };

            Uri feedUri = new Uri(feedUrl);

            try
            {
                System.Diagnostics.Debug.WriteLine("Getting feed " + feedUrl);

                SyndicationFeed feed = await client.RetrieveFeedAsync(feedUri);

                newArticles = AddEntriesFromFeed(feed, source, feedUrl);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            return newArticles;
        }

        private static int AddEntriesFromFeed (SyndicationFeed feed, Source source, string sourceUrl)
        {
            int i = 0;

            foreach (SyndicationItem item in feed.Items)
            {
                try
                {
                    string itemId = null;
                    string link = null;
                    string title = item.Title.Text.Trim();

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

                    Entry entry = new Entry
                    {
                        ID = itemId,
                        Source = source.ID,
                        Kind = "rss",
                        Title = title,
                        Link = link,
                        Fetched = false,
                        DateInsert = DateTime.Now,
                        SourceURL = sourceUrl
                    };

                    if (item.Summary != null)
                    {
                        entry.Summary = CleanString(item.Summary.Text);
                    }

                    if (item.PublishedDate != null && item.PublishedDate.Ticks > 0)
                    {
                        entry.DatePublish = item.PublishedDate.DateTime;
                    }
                    else
                    {
                        entry.DatePublish = entry.DateInsert;
                    }

                    if (item.Content != null)
                    {
                        entry.ContentText = CleanString(item.Content.Text, maxTextLength);
                        entry.ThumbnailURL = FirstImageFromText(item.Content.Text);
                    }
                    else
                    {
                        string content = null;
                        foreach (ISyndicationNode ext in item.ElementExtensions)
                        {
                            if (ext.NodeName == "content")
                            {
                                content = ext.NodeValue;
                                break;
                            }
                        }

                        if (content != null)
                        {
                            entry.ContentText = CleanString(content, maxTextLength);
                            entry.ThumbnailURL = FirstImageFromText(content);
                        }
                    }

                    if (entry.ThumbnailURL == null)
                    {
                        foreach (var l in item.Links)
                        {
                            if (l == null || l.Uri == null || !l.Uri.IsAbsoluteUri) continue;

                            string found = FirstImageFromText(l.Uri.AbsoluteUri);
                            if (found != null)
                            {
                                entry.ThumbnailURL = found;
                                break;
                            }
                        }
                    }

                    if (item.Authors.Count > 0)
                    {
                        SyndicationPerson person = item.Authors.FirstOrDefault();
                        if (person.Name != null) entry.Author = person.Name;
                        if (person.Email != null) entry.AuthorUsername = person.Email;
                    }

                    if (entry.ThumbnailURL != null)
                    {
                        entry.Fetched = true;
                    }

                    if (DB.Current.connection.Insert(entry) > 0)
                    {
                        i++;
                        System.Diagnostics.Debug.WriteLine("RSS Article saved " + entry.ID);
                    }

                    
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("Cannot add rss entry: " + e.Message);
                }
            }

            System.Diagnostics.Debug.WriteLine(String.Format("{0} articles added for source {1}", i, sourceUrl));

            return i;
        }
    }
}
