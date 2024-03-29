﻿using System;
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

        public string ObjectID { get; set; }

        [SQLite.Indexed(Name = "IndexFetched")]
        public bool Fetched { get; set; }

        [SQLite.Indexed(Name = "IndexThumbnailURL")]
        public string ThumbnailURL { get; set; }

        public string Link { get; set; }
        public string Kind { get; set; }
        public string Author { get; set; }
        public string AuthorUsername { get; set; }
        public string AuthorThumbnail { get; set; }

        public bool IsLiked { get; set; }
        public int LikesCount { get; set; }

        [SQLite.Indexed(Name = "IndexSource")]
        public int Source { get; set; }

        //[SQLite.Indexed(Name = "IndexThumbnailHasBeenDownloaded")]
        //public bool ThumbnailHasBeenDownloaded { get; set; }

        [SQLite.Indexed(Name = "IndexDateInsert")]
        public DateTime DateInsert { get; set; }

        [SQLite.Indexed(Name = "IndexDatePublish")]
        public DateTime DatePublish { get; set; }

        public string SourceTitle { get; set; }
        public string SourceURL { get; set; }

        public string AuthorDisplayString
        {
            get
            {
                string ret = "No source name";

                if (Author != null) ret = Author;
                else if (SourceTitle != null) ret = SourceTitle;

                return ret;
            }
        }

        public string SummaryOrContent
        {
            get
            {
                if (Summary != null) return Summary;
                else return ContentText;
            }
        }
    }
}
