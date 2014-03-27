using CollectorRT.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectorRT.Data.Downloaders
{
    class RSSDownloader : Downloader
    {
        public static async Task<bool> UpdateSource(Source source)
        {
            return true;
        }
    }
}
