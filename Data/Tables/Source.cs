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

        private string _urlSeparator = "\n";

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

        public async Task<bool> update(bool force = false)
        {
            if (!force && DateUpdate.Ticks >= DateTime.Now.AddMinutes(-5).Ticks)
            {
                System.Diagnostics.Debug.WriteLine("No need to update source " + ID);
                return true;
            }

            System.Diagnostics.Debug.WriteLine("Updating source " + ID);

            bool success = false;

            if (Kind == "rss") success = await RSSDownloader.UpdateSource(this);

            if (success)
            {
                DateUpdate = DateTime.Now;
                DB.Current.connection.Update(this);

                System.Diagnostics.Debug.WriteLine("Updating source: done!");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Cannot update the source of kind " + Kind);
            }

            return success;
        }
    }
}
