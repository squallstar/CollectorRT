using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CollectorRT.Data.Downloaders;

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

        public async Task<bool> update()
        {
            System.Diagnostics.Debug.WriteLine("Updating source " + ID);

            bool success = false;

            if (Kind == "rss") success = await RSSDownloader.UpdateSource(this);

            System.Diagnostics.Debug.WriteLine(success ? "Done!" : "Cannot update the source");

            return success;
        }
    }
}
