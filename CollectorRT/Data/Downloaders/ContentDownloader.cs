using CollectorRT.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Fizzler.Systems.HtmlAgilityPack;

namespace CollectorRT.Data.Downloaders
{
    public class ContentDownloader : Downloader
    {
        private bool _IsBusy = false;

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
        /// Runs the worker on the background thread
        /// </summary>
        public async void Run()
        {
            await Task.Run(() => _Run());
        }

        private async void _Run()
        {
            if (IsBusy) return;

            var toDownload = DB.Current.entries.Where(e => e.ThumbnailHasBeenDownloaded == false).OrderByDescending(e => e.DateInsert).Take(DownloadsPerThread).ToList();
            if (toDownload.Count == 0) {
                System.Diagnostics.Debug.WriteLine("The worker has nothing to do");
                return;
            }

            System.Diagnostics.Debug.WriteLine("Running the worker for " + toDownload.Count + " entries.");

            _IsBusy = true;

            foreach (var entry in toDownload)
            {
                await GetContentForEntry(entry);
            }

            DB.Current.connection.UpdateAll(toDownload);

            _IsBusy = false;

            Run();
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
                           
                        // 1. url

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

                        // 2. summary

                        if (entry.Summary == null)
                        {
                            var summary = doc.DocumentNode.QuerySelector("meta[property='description']");
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

                        // 3. image

                        var img = doc.DocumentNode.QuerySelector("meta[name='twitter:image']");
                        if (img == null || img.Attributes["content"] == null)
                        {
                            System.Diagnostics.Debug.WriteLine("-- image found");
                            img = doc.DocumentNode.QuerySelector("meta[property='og:image']");

                            if (img == null)
                            {
                                img = doc.DocumentNode.QuerySelector("meta[name='og:image']");
                            }
                        }

                        if (img != null && img.Attributes["content"] != null)
                        {
                            var imgUrl = img.Attributes["content"].Value;
                            System.Diagnostics.Debug.WriteLine("-- image found for real " + imgUrl);

                            entry.ThumbnailURL = System.Net.WebUtility.HtmlDecode(imgUrl);

                            if (entry.ThumbnailURL.StartsWith("/"))
                            {
                                var x = new Uri(entryUrl);

                                if (entry.ThumbnailURL.StartsWith("//"))
                                {
                                    // URL without protocol
                                    entry.ThumbnailURL = String.Format("{0}:{1}", x.Scheme,entry.ThumbnailURL);

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
                    entry.ThumbnailHasBeenDownloaded = true;
                }
            }

            return true;
        }
    }
}
