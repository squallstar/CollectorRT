using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CollectorRT.Data.Tables;

namespace CollectorRT.Data
{
    class Downloader
    {
        public static string MD5(string str)
        {
            return MD5CryptoServiceProvider.GetMd5String(str);
        }
    }
}
