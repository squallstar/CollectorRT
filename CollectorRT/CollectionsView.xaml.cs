using CollectorRT.Data;
using CollectorRT.Data.Downloaders;
using CollectorRT.Data.Tables;
using CollectorRT.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Il modello di elemento per la pagina vuota è documentato all'indirizzo http://go.microsoft.com/fwlink/?LinkId=234238

namespace CollectorRT
{
    /// <summary>
    /// Pagina vuota che può essere utilizzata autonomamente oppure esplorata all'interno di un frame.
    /// </summary>
    public sealed partial class CollectionsView : Page
    {
        private List<Source> sources;
        private List<SourceTile> tiles;

        private DispatcherTimer timer;

        public CollectionsView()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;

            tiles = new List<SourceTile>();
            sources = DB.Current.sources.OrderByDescending(s => s.DateUpdate).ToList();

            foreach (var source in sources)
            {
                var tile = new SourceTile(source);
                tiles.Add(tile);

                collectionsGrid.Children.Add(tile);
            }

            timer = new DispatcherTimer();
            timer.Tick += timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 0, 10);
            timer.Start();

            UpdateSources();
        }

        ~CollectionsView()
        {
            timer.Stop();
        }

        void timer_Tick(object sender, object e)
        {
            System.Diagnostics.Debug.WriteLine("Updating tiles when needed...");

            foreach (var tile in tiles)
            {
                tile.UpdateIfChanged();
            }
        }

        public async void UpdateSources()
        {
            int x = tiles.Count;

            foreach (var tile in tiles)
            {
                toUpdateSources.Text = String.Format("{0}", x);

                // Update the source on a background thread
                var updateSource = await Task.Run(() => tile.source.update());

                if (updateSource != Source.UpToDate)
                {
                    tile.UpdateIfChanged();
                }

                x--;
            }

            toUpdateSources.Text = "";
            loader.IsActive = false;

            ContentDownloader.Current.Run();
        }
    }
}
