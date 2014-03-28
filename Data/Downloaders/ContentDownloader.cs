using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectorRT.Data.Downloaders
{
    public class ContentDownloader
    {
        public static ContentDownloader Current
        {
            get
            {
                return (Windows.UI.Xaml.Application.Current as App).contentDownloader;
            }
        }

        public bool IsBusy
        {
            get
            {
                return true;
            }
        }

        public void Run()
        {
            if (IsBusy) return;


        }
    }
}
