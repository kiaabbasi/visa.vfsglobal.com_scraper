using CefSharp.DevTools.Network;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace visa.vfsglobal.com
{
    internal class API
    {
        private static string apiUrl = "https://vfs.vooria.ir/api/";

        public static async Task<bool> SendResultToServer(string[] result, string phone)
        {
            string send = string.Join(",", result);

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("key", "unc5");

                var formData = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("phone", phone),
                    new KeyValuePair<string, string>("announcement", send)
                };
                HttpContent content = new FormUrlEncodedContent(formData);

                HttpResponseMessage response = await httpClient.PostAsync(apiUrl + "message", content);


                string responseBody = await response.Content.ReadAsStringAsync();
                Dictionary<string, object> result2 = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseBody);


                return result2["message"].ToString()=="SUCCESS";

            }
        }

        public static async Task<string> getOTPFromServer(string phone)
        {
           

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("key", "unc5");

                var formData = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("phone", phone),
                    
                };
                HttpContent content = new FormUrlEncodedContent(formData);

                HttpResponseMessage response = await httpClient.PostAsync(apiUrl + "res", content);


                string responseContent = await response.Content.ReadAsStringAsync();

                string responseBody = await response.Content.ReadAsStringAsync();
                Dictionary<string, object> result2 = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseBody);
                if (result2["message"].ToString() == "SUCCESS")
                {
                    return result2["sms"].ToString();
                }
                return null;    

            }


        }
    }
}
