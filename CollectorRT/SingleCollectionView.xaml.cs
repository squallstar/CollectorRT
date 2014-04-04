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
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Il modello di elemento per la pagina base è documentato all'indirizzo http://go.microsoft.com/fwlink/?LinkId=234237

namespace CollectorRT
{
    /// <summary>
    /// Pagina base che fornisce caratteristiche comuni alla maggior parte delle applicazioni.
    /// </summary>
    public sealed partial class SingleCollectionView : Page
    {
        private Source _source;
        private List<EntryItem> entries;

        private static int EntriesPageLimit = 20;

        private NavigationHelper navigationHelper;

        /// <summary>
        /// NavigationHelper viene utilizzato in oggi pagina per favorire la navigazione e 
        /// la gestione del ciclo di vita dei processi
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public SingleCollectionView()
        {
            this.InitializeComponent();

            this.entries = new List<EntryItem>();

            this.itemsGrid.ItemHeight = 630;
            this.itemsGrid.ItemWidth = 420.0;

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
        }

        public void AppendElements()
        {
            var _newEntries = DB.Current.entries.Where(e => e.Source == _source.ID).OrderByDescending(e => e.DatePublish).Skip(entries.Count).Take(EntriesPageLimit).ToList();

            foreach (var entry in _newEntries)
            {
                var item = new EntryItem(entry);
                entries.Add(item);

                this.itemsGrid.Children.Add(item);
            }
        }

        private void OnScrollViewerViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var verticalOffset = sv.HorizontalOffset;
            var maxVerticalOffset = sv.ExtentWidth - sv.ViewportWidth;

            if (maxVerticalOffset < 0 ||
                verticalOffset == maxVerticalOffset)
            {
                // Scrolled to the far right
                this.AppendElements();
            }
        }

        /// <summary>
        /// Popola la pagina con il contenuto passato durante la navigazione. Vengono inoltre forniti eventuali stati
        /// salvati durante la ricreazione di una pagina in una sessione precedente.
        /// </summary>
        /// <param name="sender">
        /// Origine dell'evento. In genere <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Dati evento che forniscono il parametro di navigazione passato a
        /// <see cref="Frame.Navigate(Type, Object)"/> quando la pagina è stata inizialmente richiesta e
        /// un dizionario di stato mantenuto da questa pagina nel corso di una sessione
        /// precedente. Lo stato è null la prima volta che viene visitata una pagina.</param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// Mantiene lo stato associato a questa pagina in caso di sospensione dell'applicazione o se la
        /// viene scartata dalla cache di navigazione.  I valori devono essere conformi ai requisiti di
        /// serializzazione di <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">Origine dell'evento. In genere <see cref="NavigationHelper"/></param>
        /// <param name="e">Dati di evento che forniscono un dizionario vuoto da popolare con
        /// uno stato serializzabile.</param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        /// I metodi forniti in questa sezione vengono utilizzati per consentire a
        /// NavigationHelper di rispondere ai metodi di navigazione della pagina.
        /// 
        /// La logica specifica della pagina deve essere inserita nel gestore eventi per  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// e <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// Il parametro di navigazione è disponibile nel metodo LoadState 
        /// oltre allo stato della pagina conservato durante una sessione precedente.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);

            _source = e.Parameter as Source;
            this.pageTitle.Text = _source.Title;

            ContentDownloader.Current.Stop();
            ContentDownloader.Current.Run(this._source);

            this.AppendElements();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }
    }
}
