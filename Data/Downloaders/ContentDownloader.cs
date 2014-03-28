using CollectorRT.Data.Tables;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectorRT.Data.Downloaders
{
    public class ContentDownloader : Downloader
    {
        private bool _IsBusy = false;

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
            System.Diagnostics.Debug.WriteLine("Running the worker...");

            var toDownload = DB.Current.entries.Where(e => e.ThumbnailHasBeenDownloaded == false).OrderByDescending(e => e.DateInsert).Take(5).ToList();
            if (toDownload.Count == 0) return;

            _IsBusy = true;

            System.Diagnostics.Debug.WriteLine("To download: " + toDownload.Count);

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
                    System.Diagnostics.Debug.WriteLine("Downloading " + entry.Link);

                    var data = await DownloadContentFromUrl(entry.Link);

                    if (data != null)
                    {
                        HtmlDocument doc = new HtmlDocument();
                        doc.LoadHtml(data);
                           
                        // 1. url

                        var url = doc.DocumentNode.Descendants("//meta[@property='og:url']").FirstOrDefault();
                     
                        if (url == null)
                        {
                            url = doc.DocumentNode.Descendants("//meta[@name='og:url']").FirstOrDefault();
                        }

                        if (url != null && url.Attributes["content"].Value != null)
                        {
                            System.Diagnostics.Debug.WriteLine("-- follow link found");
                            entry.Link = url.Attributes["content"].Value;
                        }

                        // 2. summary

                        if (entry.Summary == null)
                        {
                            var summary = doc.DocumentNode.Descendants("//meta[@property='description']").FirstOrDefault();
                            if (summary == null || summary.Attributes["content"] == null)
                            {
                                summary = doc.DocumentNode.Descendants("//meta[@name='og:description']").FirstOrDefault();
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

                        var img = doc.DocumentNode.Descendants("//meta[@name='twitter:image']").FirstOrDefault();
                        if (img == null || img.Attributes["content"] == null)
                        {
                            System.Diagnostics.Debug.WriteLine("-- image found");
                            img = doc.DocumentNode.Descendants("//meta[@property='og:image']").FirstOrDefault();

                            if (img == null)
                            {
                                img = doc.DocumentNode.Descendants("//meta[@name='og:image']").FirstOrDefault();
                            }
                        }

                        if (img != null && img.Attributes["content"] != null)
                        {
                            var imgUrl = img.Attributes["content"].Value;
                            System.Diagnostics.Debug.WriteLine("-- image found for real " + imgUrl);

                            entry.ThumbnailURL = System.Net.WebUtility.HtmlDecode(imgUrl);
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
