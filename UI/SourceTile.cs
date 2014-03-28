using CollectorRT.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace CollectorRT.UI
{
    class SourceTile : Grid
    {
        private Source _source;
        private Entry _entry;

        private Image backgroundImage;
        private TextBlock title;
        private TextBlock content;

        public SourceTile(Source source)
        {
            _source = source;
            UpdateEntry();

            BuildTile();
        }

        public Source source
        {
            get
            {
                return _source;
            }
        }

        public void UpdateEntry()
        {
            _entry = source.FirstEntryWithImage();
        }

        public Entry entry
        {
            get
            {
                return _entry;
            }
        }

        private void BuildTile()
        {
            if (source.ID % 3 == 0)
            {
                SetValue(VariableSizedWrapGrid.ColumnSpanProperty, 2);
            }

            Margin = new Thickness(0, 0, 20, 20);

            var bg = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Bottom,
                Background = new SolidColorBrush(Color.FromArgb(140,0,0,0))
            };

            bg.SetValue(Canvas.ZIndexProperty, 1);

            if (entry != null)
            {
                content = new TextBlock
                {
                    FontSize = 21,
                    LineHeight = 25,
                    FontFamily = new FontFamily("/Assets/ProximaNova-R.ttf#Proxima Nova"),
                    TextWrapping = Windows.UI.Xaml.TextWrapping.Wrap,
                    Foreground = new SolidColorBrush(Colors.White),
                    Margin = new Thickness(20, 15, 20, -5)
                };

                bg.Children.Add(content);
            }

            title = new TextBlock
            {
                Text = source.Title.ToUpper(),
                FontFamily = new FontFamily("/Assets/ProximaNova-B.ttf#Proxima Nova"),
                FontSize = 15,
                Foreground = new SolidColorBrush(Colors.White),
                Margin = new Thickness(20,15,20,15)
            };

            bg.Children.Add(title);
            Children.Add(bg);

            backgroundImage = new Image();
            backgroundImage.Stretch = Stretch.UniformToFill;
            backgroundImage.Opacity = 0.85;
            this.Children.Add(this.backgroundImage);

            UpdateDisplayedContent();
        }

        public void UpdateDisplayedContent()
        {
            int descriptionLength = GetValue(VariableSizedWrapGrid.ColumnSpanProperty).ToString() == "1" ? 70 : 130;

            if (entry != null)
            {
                content.Text = entry.Title.Length > descriptionLength ? entry.Title.Substring(0, descriptionLength-1) + "..." : entry.Title;

                if (entry.ThumbnailURL != null)
                {
                    Background = new SolidColorBrush(Colors.White);
                    this.backgroundImage.Source = new BitmapImage(new Uri(entry.ThumbnailURL));
                }
                else
                {
                    Background = new SolidColorBrush(Color.FromArgb(255, 188, 45, 48));
                }
            }
            else
            {
                Background = new SolidColorBrush(Color.FromArgb(255, 188, 45, 48));
            }
        }
    }
}
