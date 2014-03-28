﻿using CollectorRT.Data;
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
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            txtMessage.Visibility = Visibility.Collapsed;

            if (Account.Current.isLoggedIn)
            {
                ConnectionProfile profile = NetworkInformation.GetInternetConnectionProfile();

                if (profile.GetNetworkConnectivityLevel() >= NetworkConnectivityLevel.InternetAccess)
                {
                    SyncAccountAndProceed();
                }
                else
                {
                    Proceed();
                }
            }
        }

        private async void SyncAccountAndProceed()
        {
            await SyncAccount();
            Proceed();
        }

        private void Proceed()
        {
            this.Frame.Navigate(typeof(CollectionsView));
        }

        private async Task<bool> SyncAccount()
        {
            txtDescription.Visibility = Visibility.Collapsed;
            btnLogin.Visibility = Visibility.Collapsed;
            txtEmail.Visibility = Visibility.Collapsed;
            txtPassword.Visibility = Visibility.Collapsed;
            loader.IsActive = true;
            txtMessage.Visibility = Visibility.Visible;

            return await Account.Current.Sync();
        }

        private async void LoadCollections()
        {

            var syncSuccess = await SyncAccount();
            if (syncSuccess)
            {
                GetCollectionArticles();
            }
            else
            {
                MessageDialog msgDialog = new MessageDialog("We couldn't sync your account details. Please try to login with your Collector account on your Windows Phone, then try again to log here with the same account.", "Sync failed");

                UICommand okBtn = new UICommand("OK");
                msgDialog.Commands.Add(okBtn);

                await msgDialog.ShowAsync();

                txtDescription.Visibility = Visibility.Visible;
                txtEmail.IsEnabled = true;
                txtPassword.IsEnabled = true;
                txtPassword.Text = "";
                btnLogin.IsEnabled = true;
                loader.IsActive = false;
                txtMessage.Visibility = Visibility.Collapsed;
            }
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            txtEmail.IsEnabled = false;
            txtPassword.IsEnabled = false;
            btnLogin.IsEnabled = false;
            loader.IsActive = true;

            var success = await (Application.Current as App).account.Login(txtEmail.Text, txtPassword.Text);

            if (!success)
            {
                txtEmail.IsEnabled = true;
                txtPassword.IsEnabled = true;
                txtPassword.Text = "";
                btnLogin.IsEnabled = true;
                loader.IsActive = false;

                MessageDialog msgDialog = new MessageDialog("The email address or password you entered are wrong.", "Login details invalid");

                UICommand okBtn = new UICommand("OK");
                msgDialog.Commands.Add(okBtn);

                await msgDialog.ShowAsync();

                txtPassword.Focus(FocusState.Keyboard);
            }
            else
            {
                LoadCollections();
            }
        }

        private async void GetCollectionArticles()
        {
            var collections = DB.Current.sources.ToList();

            var toUpdate = collections.Count;

            foreach (var collection in collections)
            {
                txtMessage.Text = String.Format("Fetching articles...\r\n{0} collections left", toUpdate);
                await collection.update();
                toUpdate--;
            }

            txtMessage.Text = "Clearing up...";
            Proceed();
        }
    }
}
