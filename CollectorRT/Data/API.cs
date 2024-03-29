﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CollectorRT.Data
{
    class API
    {
        public static string baseURL = "http://collectorwp.com/api/v1/";

        public static async Task<bool> AccountLogin(string email, string password)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(API.baseURL);
                var content = new FormUrlEncodedContent(new[] 
            {
                new KeyValuePair<string, string>("email", email),
                new KeyValuePair<string, string>("password", password)
            });
                var result = await client.PostAsync("login/", content);
                return result.IsSuccessStatusCode;
            }
        }

        public static async Task<HttpResponseMessage> PullData()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(API.baseURL);
                var content = new FormUrlEncodedContent(new[] 
            {
                new KeyValuePair<string, string>("email", Account.Current.Email),
                new KeyValuePair<string, string>("password", Account.Current.Password),
                new KeyValuePair<string, string>("datesync", Account.Current.LastSyncTimestamp.ToString())
            });
                System.Diagnostics.Debug.WriteLine("Requesting sync for timestamp " + Account.Current.LastSyncTimestamp.ToString());
                return await client.PostAsync("pull/", content);
            }
        }
    }
}
