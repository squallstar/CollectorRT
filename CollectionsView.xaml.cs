using CollectorRT.Data;
using CollectorRT.Data.Tables;
using CollectorRT.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

        public CollectionsView()
        {
            this.InitializeComponent();

            tiles = new List<SourceTile>();
            sources = DB.Current.sources.OrderByDescending(s => s.DateUpdate).ToList();

            foreach (var source in sources)
            {
                var tile = new SourceTile(source);
                tiles.Add(tile);

                collectionsGrid.Children.Add(tile);
            }

            UpdateSources();
        }

        public async void UpdateSources()
        {
            foreach (var tile in tiles)
            {
                var updateSource = await tile.source.update();

                if (updateSource != Source.UpToDate)
                {
                    tile.UpdateEntry();
                    tile.UpdateDisplayedContent();
                }
            }

            loader.IsActive = false;
        }
    }
}
