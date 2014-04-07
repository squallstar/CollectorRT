using CollectorRT.Common;
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
using Windows.Networking.Connectivity;
using Windows.UI.Popups;
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

            this.NavigationCacheMode = NavigationCacheMode.Enabled;

            this.SizeChanged += OnWindowSizeChanged;

            tiles = new List<SourceTile>();
            sources = DB.Current.sources.OrderByDescending(s => s.DateUpdate).ToList();

            foreach (var source in sources)
            {
                var tile = new SourceTile(source);
                tile.Tapped += tile_Tapped;
                tiles.Add(tile);

                collectionsGrid.Children.Add(tile);
            }

            timer = new DispatcherTimer();
            timer.Tick += timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 0, 13);

            UpdateSources();
        }

        void timer_Tick(object sender, object e)
        {
            foreach (var tile in tiles)
            {
                tile.UpdateIfChanged();
            }
        }

        public async void UpdateSources(bool force = false)
        {
            ConnectionProfile profile = NetworkInformation.GetInternetConnectionProfile();

            if (profile.GetNetworkConnectivityLevel() >= NetworkConnectivityLevel.InternetAccess)
            {
                int x = tiles.Count;

                this.btnRefresh.IsEnabled = false;
                this.btnRefresh.Label = "Fetching...";

                foreach (var tile in tiles)
                {
                    toUpdateSources.Text = String.Format("{0}", x);

                    // Update the source on a background thread
                    var updateSource = await Task.Run(() => tile.source.update(force));

                    if (updateSource != Source.UpToDate)
                    {
                        tile.UpdateIfChanged();
                    }

                    x--;
                }
            }

            toUpdateSources.Text = "";
            this.btnRefresh.IsEnabled = true;
            this.btnRefresh.Label = "Sync/Refresh";
            loader.IsActive = false;
        }

        void tile_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SingleCollectionView), (sender as SourceTile).source);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //navigationHelper.OnNavigatedTo(e);
            timer.Start();

            ContentDownloader.Current.Run();

            OnWindowSizeChanged(null, null);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            //navigationHelper.OnNavigatedFrom(e);
            timer.Stop();
        }

        private void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            //var CurrentViewState = Windows.UI.ViewManagement.ApplicationView.Value;
            //double AppWidth = e.Size.Width;
            //double AppHeight = e.Size.Height;

            System.Diagnostics.Debug.WriteLine("Window size changed");

            UpdateItemsSize();
        }

        private void UpdateItemsSize()
        {
            var bounds = Window.Current.Bounds;
            double height = bounds.Height;
            double width = bounds.Width;

            if (width > 600)
            {
                // Horizontal mode

                sv.VerticalScrollMode = ScrollMode.Disabled;
                sv.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;

                sv.HorizontalScrollMode = ScrollMode.Enabled;
                sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;

                double squareSize = height / 4;

                this.collectionsGrid.ItemHeight = squareSize;
                this.collectionsGrid.ItemWidth = squareSize;
            }
            else
            {
                // Vertical mode

                sv.VerticalScrollMode = ScrollMode.Enabled;
                sv.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

                sv.HorizontalScrollMode = ScrollMode.Disabled;
                sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;

                this.collectionsGrid.ItemHeight = 200.0;
                this.collectionsGrid.ItemWidth = Double.NaN;
            }
        }

        private async void SyncRefresh_Click(object sender, RoutedEventArgs e)
        {
            this.BottomAppBar.IsOpen = false;
            this.loader.IsActive = true;
            this.btnRefresh.IsEnabled = false;
            this.btnRefresh.Label = "Syncing...";

            await Account.Current.Sync();
            UpdateSources(true);
        }

        private async void Logout_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog msgDialog = new MessageDialog("Do you want to log out from your Collector account?\r\n\r\nTo clean up temporary files on this device, the app will also exit.", "Logout");

            UICommand okBtn = new UICommand("Log out");
            okBtn.Invoked = Logout_Confirm_Click;
            msgDialog.Commands.Add(okBtn);

            msgDialog.Commands.Add(new UICommand("Cancel"));

            await msgDialog.ShowAsync();
        }

        private void Logout_Confirm_Click(IUICommand command)
        {
            this.BottomAppBar.IsOpen = false;

            ContentDownloader.Current.Stop();
            Account.Current.Logout();

            DB.Current.Clean();

            Application.Current.Exit();
        }
    }
}
