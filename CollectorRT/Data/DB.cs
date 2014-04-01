using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CollectorRT.Data.Tables;
using Windows.UI.Xaml;

namespace CollectorRT.Data
{
    public class DB
    {
        public string DBPath { get; set; }
        public SQLite.SQLiteConnection connection;

        public DB()
        {
            // Get a reference to the SQLite database
            this.DBPath = System.IO.Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "collector.sqlite");

            connection = new SQLite.SQLiteConnection(this.DBPath);

            // Create the tables if they don't exist
            connection.CreateTable<Source>();
            connection.CreateTable<Entry>();
        }

        public static DB Current
        {
            get
            {
                return (Application.Current as App).db;
            }
        }

        public SQLite.TableQuery<Source> sources
        {
            get
            {
                return connection.Table<Source>();
            }
        }

        public SQLite.TableQuery<Entry> entries
        {
            get
            {
                return connection.Table<Entry>();
            }
        }
    }
}
