using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CollectorRT.Data.Tables;

namespace CollectorRT.Data
{
    public class DB
    {
        public string DBPath { get; set; }

        public DB()
        {
            // Get a reference to the SQLite database
            this.DBPath = System.IO.Path.Combine(
                Windows.Storage.ApplicationData.Current.LocalFolder.Path, "customers.sqlite");
            // Initialize the database if necessary
            using (var db = new SQLite.SQLiteConnection(this.DBPath))
            {
                // Create the tables if they don't exist
                db.CreateTable<Source>();
            }
        }
    }
}
