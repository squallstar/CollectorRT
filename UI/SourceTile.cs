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

        public SourceTile(Source source)
        {
            _source = source;
            _entry = source.FirstEntryWithImage();

            BuildTile();
        }

        public Source source
        {
            get
            {
                return _source;
            }
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
            Width = 300;
            Height = 200;
            Background = new SolidColorBrush(Colors.White);

            var bg = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Bottom,
                Background = new SolidColorBrush(Color.FromArgb(140,0,0,0))
            };

            bg.SetValue(Canvas.ZIndexProperty, 1);

            if (entry != null)
            {
                var content = new TextBlock
                {
                    Text = entry.Title,
                    FontSize = 18,
                    TextWrapping = Windows.UI.Xaml.TextWrapping.Wrap,
                    Foreground = new SolidColorBrush(Colors.White),
                    Margin = new Thickness(20, 10, 20, 0)
                };

                bg.Children.Add(content);
            }

            var title = new TextBlock
            {
                Text = source.Title,
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.White),
                Margin = new Thickness(20,10,20,10)
            };

            bg.Children.Add(title);
            Children.Add(bg);

            this.backgroundImage = new Image();
            this.backgroundImage.Stretch = Stretch.UniformToFill;
            this.backgroundImage.Opacity = 0.85;
            this.Children.Add(this.backgroundImage);

            if (entry != null && entry.ThumbnailURL != null)
            {
                this.backgroundImage.Source = new BitmapImage(new Uri(entry.ThumbnailURL));
            }
        }
    }
}
