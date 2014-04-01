using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CollectorRT.Data.Downloaders;
using System.Text.RegularExpressions;

namespace CollectorRT.Data.Tables
{
    public class Source
    {
        [SQLite.PrimaryKey, SQLite.AutoIncrement]
        public int ID { get; set; }
        public string Guid { get; set; }
        public string Title { get; set; }
        public string Kind { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public int UnreadEntries { get; set; }

        public DateTime DateUpdate { get; set; }

        private static int OutDatedAfterMinutes = 5;

        private string _urlSeparator = "\n";

        public static int UpToDate = -1;

        public string[] Urls
        {
            get
            {
                if (this.Url != null && this.Url.Contains(_urlSeparator))
                {
                    //Multiple sources
                    return Regex.Split(this.Url, _urlSeparator);
                }
                else
                {
                    //Single sources
                    return new string[] { this.Url };
                }
            }
        }

        public async Task<int> update(bool force = false)
        {
            if (!force && DateUpdate.Ticks >= DateTime.Now.AddMinutes(-OutDatedAfterMinutes).Ticks)
            {
                System.Diagnostics.Debug.WriteLine("No need to update source " + ID);
                return Source.UpToDate;
            }

            System.Diagnostics.Debug.WriteLine("Updating source " + ID);

            int newArticles = 0;

            if (Kind == "rss") newArticles = await RSSDownloader.UpdateSource(this);
            else if (Kind == "tumblr") newArticles = await TumblrDownloader.UpdateSource(this);

            UnreadEntries += newArticles;
            DateUpdate = DateTime.Now;
            DB.Current.connection.Update(this);

            return newArticles;
        }

        public Entry FirstEntryWithImage()
        {
            var entry = DB.Current.entries.Where(e => e.Source == this.ID && e.ThumbnailURL != null).OrderByDescending(e => e.DateInsert).FirstOrDefault();

            if (entry == null)
            {
                return DB.Current.entries.Where(e => e.Source == this.ID).OrderByDescending(e => e.DateInsert).FirstOrDefault();
            }

            return entry;
        }
    }
}
