using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using CollectorRT.Data.Tables;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.Xaml;

namespace CollectorRT.UI
{
    public class EntryItem : StackPanel
    {
        public static int SidePadding = 20;
        public static int Spacer = 10;

        private Entry _entry;

        private TextBlock _title;
        private TextBlock _author;
        private TextBlock _datepost;
        private Image _img;

        public EntryItem (Entry entry)
        {
            _entry = entry;

            this.Build();
        }

        public void Build()
        {
            Background = new SolidColorBrush(Colors.White);
            Margin = new Thickness(0, 0, 20, 20);

            _title = new TextBlock
            {
                FontSize = 24,
                LineHeight = 30,
                FontFamily = new FontFamily("/Assets/ProximaNova-R.ttf#Proxima Nova"),
                Text = _entry.Title,
                Margin = new Thickness(30,30,30,30),
                TextWrapping = Windows.UI.Xaml.TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Colors.Black)
            };

            this.Children.Add(_title);
        }
    }
}
