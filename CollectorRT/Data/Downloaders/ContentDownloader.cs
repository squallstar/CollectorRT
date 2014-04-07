using CollectorRT.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Fizzler.Systems.HtmlAgilityPack;
using System.Threading;

namespace CollectorRT.Data.Downloaders
{
    public class ContentDownloader : Downloader
    {
        private bool _IsBusy = false;

        private CancellationTokenSource _sourceToken;

        private static int DownloadsPerThread = 10;

        public static ContentDownloader Current
        {
            get
            {
                return (Windows.UI.Xaml.Application.Current as App).contentDownloader;
            }
        }

        public bool IsBusy
        {
            get
            {
                return _IsBusy;
            }
        }

        /// <summary>
        /// Stops the worker
        /// </summary>
        public void Stop()
        {
            if (_sourceToken != null)
            {
                _sourceToken.Cancel();
                _sourceToken = null;
                _IsBusy = false;
            }
        }

        /// <summary>
        /// Runs the worker on the background thread
        /// </summary>
        public async void Run(Source source = null)
        {
            Stop();

            _sourceToken = new CancellationTokenSource();

            await Task.Run(() => _Run(source, _sourceToken.Token), _sourceToken.Token);
        }

        private async void _Run(Source source, CancellationToken cancellationToken)
        {
            if (IsBusy) return;

            var tmp = DB.Current.entries.Where(e => e.Fetched != true);

            if (source != null)
            {
                tmp = tmp.Where(s => s.Source == source.ID);
            }

            var toDownload = tmp.OrderByDescending(e => e.DateInsert).Take(DownloadsPerThread).ToList();
            if (toDownload.Count == 0)
            {
                if (source != null)
                {
                    System.Diagnostics.Debug.WriteLine("The worker downloaded all the contents for the given source.");
                    Run();
                    return;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("The worker has nothing to do");
                    return;
                }
            }

            System.Diagnostics.Debug.WriteLine("Running the worker for " + toDownload.Count + " entries.");

            _IsBusy = true;

            foreach (var entry in toDownload)
            {
                try
                {
                    await GetContentForEntry(entry);
                }
                catch (Exception) { }

                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }
                catch (Exception)
                {
                    DB.Current.connection.UpdateAll(toDownload);
                    _IsBusy = false;
                    return;
                }
            }

            DB.Current.connection.UpdateAll(toDownload);
            _IsBusy = false;

            Run(source);
        }

        public async Task<bool> GetContentForEntry(Entry entry)
        {
            if (entry.Kind != "facebook")
            {
                try
                {
                    var entryUrl = entry.Link;

                    System.Diagnostics.Debug.WriteLine("Downloading " + entryUrl);

                    var data = await DownloadContentFromUrl(entryUrl);

                    if (data != null)
                    {
                        var doc = new HtmlAgilityPack.HtmlDocument();
                        doc.LoadHtml(data);

                        // 1. title

                        if (entry.Title == null || entry.Kind != "rss")
                        {
                            var title = doc.DocumentNode.QuerySelector("title");

                            if (title != null)
                            {
                                entry.Title = title.InnerText;
                            }
                        }
                           
                        // 2. url

                        var url = doc.DocumentNode.QuerySelector("meta[property='og:url']");
                     
                        if (url == null)
                        {
                            url = doc.DocumentNode.QuerySelector("meta[name='og:url']");
                        }

                        if (url != null && url.Attributes["content"].Value != null)
                        {
                            System.Diagnostics.Debug.WriteLine("-- follow link found");
                            entry.Link = url.Attributes["content"].Value;
                        }

                        // 3. summary

                        if (entry.Summary == null)
                        {
                            var summary = doc.DocumentNode.QuerySelector("meta[property='description']");

                            if (summary == null || summary.Attributes["content"] == null)
                            {
                                summary = doc.DocumentNode.QuerySelector("meta[name='description']");
                            }

                            if (summary == null || summary.Attributes["content"] == null)
                            {
                                summary = doc.DocumentNode.QuerySelector("meta[name='og:description']");
                            }

                            if (summary != null && summary.Attributes["content"] != null)
                            {
                                System.Diagnostics.Debug.WriteLine("-- summary found");
                                string s = summary.Attributes["content"].Value;
                                if (s != null && s.Length > 10)
                                {
                                    entry.Summary = System.Net.WebUtility.HtmlDecode(s);
                                }
                            }
                        }

                        // 4. image

                        var img = doc.DocumentNode.QuerySelector("meta[name='twitter:image']");
                        if (img == null || img.Attributes["content"] == null)
                        {
                            img = doc.DocumentNode.QuerySelector("meta[property='og:image']");

                            if (img == null)
                            {
                                img = doc.DocumentNode.QuerySelector("meta[name='og:image']");
                            }
                        }

                        if (img != null && img.Attributes["content"] != null)
                        {
                            var imgUrl = img.Attributes["content"].Value;

                            entry.ThumbnailURL = System.Net.WebUtility.HtmlDecode(imgUrl);

                            if (entry.ThumbnailURL.StartsWith("/"))
                            {
                                var x = new Uri(entryUrl);

                                if (entry.ThumbnailURL.StartsWith("//"))
                                {
                                    // URL without protocol
                                    entry.ThumbnailURL = String.Format("{0}:{1}", x.Scheme, entry.ThumbnailURL);

                                    System.Diagnostics.Debug.WriteLine("Image url resolved to absolute: " + entry.ThumbnailURL);
                                }
                                else
                                {
                                    // Relative URL
                                    entry.ThumbnailURL = String.Format("{0}://{1}{2}", x.Scheme, x.Host, entry.ThumbnailURL);

                                    System.Diagnostics.Debug.WriteLine("Image url resolved to absolute: " + entry.ThumbnailURL);
                                }
                                
                               
                                
                            }
                        }
                    }
                }
                finally
                {
                    entry.Fetched = true;
                }
            }

            return true;
        }
    }
}
