﻿using CollectorRT.Data;
using CollectorRT.Data.Downloaders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Il modello di applicazione vuota è documentato all'indirizzo http://go.microsoft.com/fwlink/?LinkId=234227

namespace CollectorRT
{
    /// <summary>
    /// Fornisci un comportamento specifico dell'applicazione in supplemento alla classe Application predefinita.
    /// </summary>
    sealed partial class App : Application
    {
        public Account account;
        public DB db;
        public ContentDownloader contentDownloader;

        /// <summary>
        /// Inizializza l'oggetto Application singleton.  Si tratta della prima riga del codice creato
        /// eseguita e, come tale, corrisponde all'equivalente logico di main() o WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Richiamato quando l'applicazione viene avviata normalmente dall'utente.  All'avvio dell'applicazione
        /// verranno utilizzati altri punti di ingresso per aprire un file specifico.
        /// </summary>
        /// <param name="e">Dettagli sulla richiesta e il processo di avvio.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {

#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            Frame rootFrame = Window.Current.Content as Frame;

            // Non ripetere l'inizializzazione dell'applicazione se la finestra già dispone di contenuto,
            // assicurarsi solo che la finestra sia attiva
            if (rootFrame == null)
            {
                account = new Account();
                db = new DB();

                contentDownloader = new ContentDownloader();
                contentDownloader.Run();

                // Windows Store Apps
                AsyncOAuth.OAuthUtility.ComputeHash = (key, buffer) =>
                {
                    var crypt = Windows.Security.Cryptography.Core.MacAlgorithmProvider.OpenAlgorithm("HMAC_SHA1");
                    var keyBuffer = Windows.Security.Cryptography.CryptographicBuffer.CreateFromByteArray(key);
                    var cryptKey = crypt.CreateKey(keyBuffer);

                    var dataBuffer = Windows.Security.Cryptography.CryptographicBuffer.CreateFromByteArray(buffer);
                    var signBuffer = Windows.Security.Cryptography.Core.CryptographicEngine.Sign(cryptKey, dataBuffer);

                    byte[] value;
                    Windows.Security.Cryptography.CryptographicBuffer.CopyToByteArray(signBuffer, out value);
                    return value;
                };


                // Creare un frame che agisca da contesto di navigazione e passare alla prima pagina
                rootFrame = new Frame();
                // Imposta la lingua predefinita
                rootFrame.Language = Windows.Globalization.ApplicationLanguages.Languages[0];

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Caricare lo stato dall'applicazione sospesa in precedenza
                }

                // Posizionare il frame nella finestra corrente
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // Quando lo stack di navigazione non viene ripristinato, esegui la navigazione alla prima pagina,
                // configurando la nuova pagina per passare le informazioni richieste come parametro di
                // navigazione
                rootFrame.Navigate(typeof(MainPage), e.Arguments);
            }
            // Assicurarsi che la finestra corrente sia attiva
            Window.Current.Activate();
        }

        //private async void CreateDatabase()
        //{
        //    SQLiteAsyncConnection conn = new SQLiteAsyncConnection("people");
        //    await conn.CreateTableAsync<Person>();
        //}

        /// <summary>
        /// Chiamato quando la navigazione a una determinata pagina ha esito negativo
        /// </summary>
        /// <param name="sender">Frame la cui navigazione non è riuscita</param>
        /// <param name="e">Dettagli sull'errore di navigazione.</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Richiamato quando l'esecuzione dell'applicazione viene sospesa.  Lo stato dell'applicazione viene salvato
        /// senza che sia noto se l'applicazione verrà terminata o ripresa con il contenuto
        /// della memoria ancora integro.
        /// </summary>
        /// <param name="sender">Origine della richiesta di sospensione.</param>
        /// <param name="e">Dettagli relativi alla richiesta di sospensione.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Salvare lo stato dell'applicazione e interrompere qualsiasi attività in background
            deferral.Complete();
        }
    }
}
