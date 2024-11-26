using CefSharp.DevTools.Network;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace visa.vfsglobal.com
{
    internal class API
    {
        private static string apiUrl = "https://vfs.vooria.ir/api/";

        public static async Task<bool> SendResultToServer(string[] result, string phone)
        {
            string send = string.Join(",", result.Select(date => date.Split(' ')[0]));

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


                return result2["message"].ToString() == "SUCCESS";

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

        public class EmailMessage
        {
            public int id { get; set; }
            public string from { get; set; }
            public string subject { get; set; }
            public string date { get; set; } // Date stored as string
        }

        // Dictionary to store the last date for each email account
        static Dictionary<string, DateTime> lastdateDict = new Dictionary<string, DateTime>();



        public static async Task<int> getNewestOtpEmailId(string username, string domain)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                HttpResponseMessage response = await httpClient.GetAsync($"https://www.1secmail.com/api/v1/?action=getMessages&login={username}&domain={domain}");
                string responseBody = await response.Content.ReadAsStringAsync();

                // Deserialize to a list of EmailMessage objects
                List<EmailMessage> result = JsonConvert.DeserializeObject<List<EmailMessage>>(responseBody);

                EmailMessage newestOtpEmail = null;

                // Combine username and domain to create a unique key
                string emailKey = $"{username}@{domain}";

                // Initialize lastdate if this is the first time this email is processed
                if (!lastdateDict.ContainsKey(emailKey))
                {
                    lastdateDict[emailKey] = DateTime.MinValue;
                }

                foreach (var emailMessage in result)
                {
                    if (emailMessage.subject.Contains("One Time Password"))
                    {
                        DateTime emailDate = DateTime.Parse(emailMessage.date);

                        // Check if this email is the newest we've found so far
                        if (newestOtpEmail == null || emailDate > DateTime.Parse(newestOtpEmail.date))
                        {
                            newestOtpEmail = emailMessage;
                        }
                    }
                }

                // If a matching email is found
                if (newestOtpEmail != null)
                {
                    DateTime newestEmailDate = DateTime.Parse(newestOtpEmail.date);
                    TimeSpan timeDifference = DateTime.UtcNow - newestEmailDate;

                    // Check if the email is newer than the last recorded date and within 2 minutes of the current time
                    if (newestEmailDate > lastdateDict[emailKey] && timeDifference.TotalMinutes <= 2)
                    {
                        // Update lastdateDict with the date of the newest OTP email
                        lastdateDict[emailKey] = newestEmailDate;
                        return newestOtpEmail.id;
                    }
                }

                // Return -1 if no newer email is found or if the email is older than 2 minutes
                return -1;
            }
        }

        public static async Task waitOnline(WebHandelers web)
        {
            while( !await imIOnline())
            {
                if (web.isCanseling == true)
                    return;
                await Task.Delay(1000);
            }
        }
        public static async Task<bool> imIOnline()
        {
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    HttpResponseMessage response = await httpClient.GetAsync("https://api.ipify.org/?format=json");
                }

                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        public static async Task<string> getTheOtp(string username,string domain)
        {
            int id = await getNewestOtpEmailId(username, domain);
            if (id == -1)
                return null;

            using (HttpClient httpClient = new HttpClient())
            {
                HttpResponseMessage response = await httpClient.GetAsync($"https://www.1secmail.com/api/v1/?action=readMessage&login={username}&domain={domain}&id={id}");
                string responseBody = await response.Content.ReadAsStringAsync();

                // Deserialize the response to a dictionary
                Dictionary<string, object> result = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseBody);

                if (result.TryGetValue("htmlBody", out object htmlBodyObj) && htmlBodyObj is string htmlBody)
                {
                    string searchString = "The OTP for your application with VFS Global is";
                    int startIndex = htmlBody.IndexOf(searchString);

                    if (startIndex != -1)
                    {
                        // Calculate the position where the OTP starts
                        int otpStartIndex = startIndex + searchString.Length;

                        // Extract the OTP, which is 6 characters long
                        string otp = htmlBody.Substring(otpStartIndex+1, 6);

                        return otp;
                    }
                }
            }

            return null; // Return null if OTP is not found
        }

    }
}