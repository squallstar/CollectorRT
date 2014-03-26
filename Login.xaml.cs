using CollectorRT.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
                btnLogin.Visibility = Visibility.Collapsed;
                txtEmail.Visibility = Visibility.Collapsed;
                txtPassword.Visibility = Visibility.Collapsed;
                loader.IsActive = true;
                txtMessage.Visibility = Visibility.Visible;

                var syncSuccess = await Account.Current.Sync();
            }
        }
    }
}
