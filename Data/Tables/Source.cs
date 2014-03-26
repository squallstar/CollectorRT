using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
