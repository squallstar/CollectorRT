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

        public async void Run()
        {
            if (IsBusy) return;

            var toDownload = DB.Current.entries.Where(e => e.ThumbnailHasBeenDownloaded == false).OrderByDescending(e => e.DateInsert).Take(5).ToList();
            if (toDownload.Count == 0) return;

            _IsBusy = true;

            foreach (var entry in toDownload)
            {
                await GetContentForEntry(entry);
            }

            DB.Current.connection.UpdateAll(toDownload);

            _IsBusy = false;
        }

        public async Task<bool> GetContentForEntry(Entry entry)
        {
            if (entry.Kind != "facebook" || (entry.AuthorUsername == null || entry.AuthorUsername == ""))
            {
                try
                {
                    var data = await DownloadContentFromUrl(entry.Link);

                    if (data != null)
                    {
                        HtmlDocument doc = new HtmlDocument();
                        doc.LoadHtml(data);

                        var url = doc.DocumentNode.Descendants("//meta[@property='og:url']").FirstOrDefault();

                     
                        if (url == null)
                        {
                            url = doc.DocumentNode.Descendants("//meta[@name='og:url']").FirstOrDefault();
                        }

                        if (url != null && url.Attributes["content"].Value != null)
                        {
                            entry.Link = url.Attributes["content"].Value;
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
