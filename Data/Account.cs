using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CollectorRT.Data
{
    public class Account
    {
        Windows.Storage.ApplicationDataContainer settings = Windows.Storage.ApplicationData.Current.LocalSettings;

        private static string AccountLogged = "Account.Logged";
        private static string AccountEmail = "Account.Email";
        private static string AccountPassword = "Account.Password";
        //private static string AccountLastSync = "Account.LastSync";

        public static Account Current
        {
            get
            {
                return (Windows.UI.Xaml.Application.Current as App).account;
            }
        }

        public bool isLoggedIn
        {
            get
            {
                return settings.Values.ContainsKey(AccountLogged);
            }
        }

        public string Email
        {
            get
            {
                return settings.Values.ContainsKey(AccountEmail) ? settings.Values[AccountEmail].ToString() : null;
            }
        }

        public string Password
        {
            get
            {
                return settings.Values.ContainsKey(AccountPassword) ? settings.Values[AccountPassword].ToString() : null;
            }
        }

        public void Logout()
        {
            settings.Values.Remove(AccountLogged);
            settings.Values.Remove(AccountEmail);
            settings.Values.Remove(AccountPassword);
        }

        public async Task<bool> Login(string email, string password)
        {
            var success = await API.AccountLogin(email, password);
                
            if (success)
            {
                settings.Values[AccountLogged] = true;
                settings.Values[AccountEmail] = email;
                settings.Values[AccountPassword] = password;
            }

            return success;
        }

        public async Task<bool> Sync()
        {
            var data = await API.PullData();

            if (data.IsSuccessStatusCode)
            {
                var response = await data.Content.ReadAsStringAsync();
            }

            return data.IsSuccessStatusCode;
        }
    }
}
