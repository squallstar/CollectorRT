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
using Windows.UI.Xaml.Media.Imaging;
using CollectorRT.Data;

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
        private TextBlock _content;
        private Image _img;

        public EntryItem (Entry entry)
        {
            _entry = entry;

            this.Build();
        }

        public void Build()
        {
            Background = new SolidColorBrush(Colors.White);
            Margin = new Thickness(0, 0, 10, 10);

            VerticalAlignment = VerticalAlignment.Stretch;

            foreach (UIElement el in this.Children.ToList())
            {
                this.Children.Remove(el);
            }

            this._BuildTallVertical();
        }

        public void UpdateIfChanged()
        {
            var newEntry = DB.Current.entries.Where(e => e.ID == _entry.ID).FirstOrDefault();

            if (_entry.ThumbnailURL != newEntry.ThumbnailURL || _entry.Summary != newEntry.Summary || _entry.Link != newEntry.Link)
            {
                // The object on the DB was updated
                _entry = newEntry;

                Build();
            }
        }

        private void _BuildTallVertical()
        {
            // 1. Title

            //this.RowDefinitions.Add(new RowDefinition
            //{
            //    MaxHeight = 300
            //});

            if (_entry.ThumbnailURL != null)
            {
                _img = new Image();
                _img.Margin = new Thickness(0, 0, 0, 0);
                _img.VerticalAlignment = VerticalAlignment.Top;
                _img.HorizontalAlignment = HorizontalAlignment.Left;
                _img.Stretch = Stretch.UniformToFill;
                _img.MaxHeight = 250;

                try
                {
                    //This cause the app to crash if the url is not well formed
                    _img.Source = new BitmapImage(new Uri(_entry.ThumbnailURL, UriKind.Absolute));

                    this.Children.Add(_img);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                    _img = null;
                }
            }

            _title = new TextBlock
            {
                FontSize = 37,
                LineHeight = 45,
                FontFamily = new FontFamily("/Assets/ProximaNovaCondensed.ttf#ProximaNovaCondensed"),
                Text = _entry.Title,
                Margin = new Thickness(20, 20, 20, 15),
                TextWrapping = Windows.UI.Xaml.TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Colors.Black),
                VerticalAlignment = VerticalAlignment.Top
            };

            this.Children.Add(_title);


            // 2. Author - Source

            _author = new TextBlock
            {
                FontSize = 20,
                LineHeight = 25,
                FontFamily = new FontFamily("/Assets/ProximaNova-R.ttf#Proxima Nova"),
                Text = _entry.AuthorDisplayString,
                Margin = new Thickness(20, 0, 20, 20),
                TextWrapping = Windows.UI.Xaml.TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Color.FromArgb(255,190,43,43)),
                VerticalAlignment = VerticalAlignment.Top
            };

            //3. Description

            if (_entry.ContentText != null)
            {
                _content = new TextBlock
                {
                    FontSize = 20,
                    LineHeight = 25,
                    FontFamily = new FontFamily("/Assets/ProximaNova-R.ttf#Proxima Nova"),
                    Text = _entry.ContentText,
                    Margin = new Thickness(20, 0, 20, 20),
                    TextWrapping = Windows.UI.Xaml.TextWrapping.Wrap,
                    Foreground = new SolidColorBrush(Color.FromArgb(255, 110, 110, 110)),
                    VerticalAlignment = VerticalAlignment.Top
                };

                this.Children.Add(_content);
            }
        }
    }
}
