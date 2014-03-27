using CollectorRT.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace CollectorRT.UI
{
    class SourceTile : Grid
    {
        private Source _source;

        public SourceTile(Source source)
        {
            _source = source;
            BuildTile();
        }

        public Source source
        {
            get
            {
                return _source;
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

            var title = new TextBlock
            {
                Text = source.Title,
                FontSize = 14,
                Foreground = new SolidColorBrush(Colors.White),
                Margin = new Thickness(20,10,20,10)
            };

            bg.Children.Add(title);

            Children.Add(bg);
        }
    }
}
