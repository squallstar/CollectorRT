using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectorRT.Data.Tables
{
    public class Entry
    {
        [SQLite.PrimaryKey]
        public string ID { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string ContentText { get; set; }
        public string ThumbnailURL { get; set; }
        public string Link { get; set; }
        public string Kind { get; set; }
        public string Author { get; set; }
        public string AuthorUsername { get; set; }
        public int Source { get; set; }
        public bool ThumbnailHasBeenDownloaded { get; set; }
        public DateTime DateInsert { get; set; }
        public DateTime DatePublish { get; set; }
        public string SourceTitle { get; set; }
        public string SourceURL { get; set; }
    }
}
