using CollectorRT.Data.Tables;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace CollectorRT.Data
{
    public class Account
    {
        Windows.Storage.ApplicationDataContainer settings = Windows.Storage.ApplicationData.Current.LocalSettings;

        private static string AccountLogged = "Account.Logged";
        private static string AccountEmail = "Account.Email";
        private static string AccountPassword = "Account.Password";
        private static string AccountLastSync = "Account.LastSync";

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

        public int LastSyncTimestamp
        {
            get
            {
                if (settings.Values.ContainsKey(AccountLastSync))
                {
                    return int.Parse(settings.Values[AccountLastSync].ToString());
                }
                else
                {
                    return 0;
                }
            }
            set
            {
                settings.Values[AccountLastSync] = value;
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

            var success = data.IsSuccessStatusCode;

            if (success)
            {
                var response = await data.Content.ReadAsStringAsync();

                if (response != null && response != "")
                {
                    JObject result = null;

                    try
                    {
                        result = JObject.Parse(response);
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine("JSON Parsing failed: " + e.Message);
                        return success;
                    }

                    int datesync = int.Parse(result["datesync"].ToString());

                    var db = (Application.Current as App).db;

                    var serverSources = JsonConvert.DeserializeObject<List<Source>>(result["collections"].ToString());
                    var localSources = DB.Current.sources.ToList();

                    var sourcesToRemove = new List<Source>(localSources);
                    int countNew = 0, countUpdate = 0;

                    foreach (var x in serverSources)
                    {
                        Source serverSource = x;
                        Source existingSource = null;

                        foreach (var s in localSources)
                        {
                            if (serverSource.Guid == s.Guid)
                            {
                                existingSource = s;
                                break;
                            }
                        }

                        if (existingSource != null)
                        {
                            sourcesToRemove.Remove(existingSource);
                            countUpdate++;

                            //Update the existing source
                            existingSource.Description = serverSource.Description;
                            existingSource.Kind = serverSource.Kind;
                            existingSource.Title = serverSource.Title;
                            existingSource.UnreadEntries = serverSource.UnreadEntries;
                            existingSource.Url = serverSource.Url;

                            db.connection.Update(existingSource);
                            System.Diagnostics.Debug.WriteLine("Source updated");
                        }
                        else
                        {
                            //New source
                            countNew++;

                            db.connection.Insert(new Source
                            {
                                Guid = serverSource.Guid,
                                Title = serverSource.Title,
                                Kind = serverSource.Kind,
                                Description = serverSource.Description,
                                Url = serverSource.Url,
                                DateUpdate = DateTime.Today,
                                UnreadEntries = 0
                            });

                            System.Diagnostics.Debug.WriteLine("Source added");
                        }
                    }

                    foreach (var s in sourcesToRemove)
                    {
                        db.connection.Delete(s);
                        System.Diagnostics.Debug.WriteLine("Source removed");
                    }

                    if (result["preferences"] != null)
                    {
                        var serverSettings = JsonConvert.DeserializeObject<IDictionary<string, object>>(result["preferences"].ToString());

                        //Add new keys
                        this.ReplaceIntSetting(serverSettings, "Settings.EntriesLimit");

                        this.ReplaceStringSetting(serverSettings, "Twitter.Token.Key");
                        this.ReplaceStringSetting(serverSettings, "Twitter.Token.Secret");
                        this.ReplaceStringSetting(serverSettings, "Twitter.userId");
                        this.ReplaceStringSetting(serverSettings, "Twitter.screenName");

                        this.ReplaceStringSetting(serverSettings, "Settings.FB.AccessToken");
                        this.ReplaceStringSetting(serverSettings, "Settings.FB.ID");
                        this.ReplaceStringSetting(serverSettings, "Settings.FB.FullName");

                        this.ReplaceStringSetting(serverSettings, "Settings.Instagram.AccessToken");
                        this.ReplaceStringSetting(serverSettings, "Settings.Instagram.FullName");

                        //this.ReplaceStringSetting(serverSettings, Pocket.AccessTokenKey);
                        //this.ReplaceStringSetting(serverSettings, Pocket.UsernameKey);

                        this.LastSyncTimestamp = int.Parse(result["datesync"].ToString());
                    }
                }
            }

            return success;
        }

        private void ReplaceIntSetting(IDictionary<string, object> collection, string key)
        {
            try
            {
                if (collection.ContainsKey(key))
                {
                    settings.Values[key] = Int32.Parse(collection[key].ToString());
                }
                else if (settings.Values.ContainsKey(key))
                {
                    settings.Values.Remove(key);
                }
            }
            catch (Exception) { }
        }

        private void ReplaceStringSetting(IDictionary<string, object> collection, string key)
        {
            try
            {
                if (collection.ContainsKey(key))
                {
                    settings.Values[key] = collection[key].ToString();
                }
                else if (settings.Values.ContainsKey(key))
                {
                    settings.Values.Remove(key);
                }
            }
            catch (Exception) { }
        }
    }
}
