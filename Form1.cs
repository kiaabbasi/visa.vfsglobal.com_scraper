using CefSharp;
using CefSharp.DevTools.Page;
using CefSharp.Handler;
using CefSharp.Internals;
using CefSharp.WinForms;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace visa.vfsglobal.com
{
    public partial class Form1 : Form
    {
        private ChromiumWebBrowser browser;
        private WebHandelers webHandelers;
        private CustomRequestHandler customRequestHandler;

        public Form1()
        {
            InitializeComponent();
            InitializeChromium();
        }
        int userIndextoRun = -1;
        public Form1(int userIndex) :this() 
        {
            
            userIndextoRun = userIndex;
        }
        public CefSettings settings;
        private void InitializeChromium(string proxy = "")
        {
            string username = "";
            string password = "";
            settings = new CefSettings();

           

           
            Cef.Initialize(settings);

            customRequestHandler = new CustomRequestHandler(username, password);
            browser = new ChromiumWebBrowser("https://api.ipify.org/?format=json")
            {
                Dock = DockStyle.Fill,
                RequestHandler = customRequestHandler
            };
            panelBrowser.Controls.Add(browser);
            webHandelers = new WebHandelers(browser);
        }

     
        
        private void button1_Click(object sender, EventArgs e)
        {
            browser.ShowDevTools();
        }

        
        private async Task<int> forcSmsNotEmpty()
        {
            while (txtSmsCode.Tag.ToString()==false.ToString()){
                if (webHandelers.isCanseling)
                {
                    return -1;
                }
                else if ((await webHandelers.getContentTextFromXPATH("/html/body/app-root/div/div/app-dashboard/section[1]/div/div[1]/div[2]/button/span[1]")).Contains("Start New Booking"))
                {
                    return 2;
                }
                else if (await webHandelers.isExistElementByXPATH("/html/body/app-root/div/header/app-notification/div") && !string.IsNullOrWhiteSpace(await webHandelers.getContentTextFromXPATH("/html/body/app-root/div/header/app-notification/div")))
                {
                    throw new Exception(await webHandelers.getContentTextFromXPATH("/html/body/app-root/div/header/app-notification/div"));
                }
                await Task.Delay (1000);
            }
            
            Invoke(new Action(() => {
                lblStatus.Text = "کد دریافت شد";
            } ));
            return 0;
        }
        private async Task fillSmsUseingApi()
        {
            for (int i = 1; i < 5; i++)
            {
                await customDeley(5*i);
                if (webHandelers.isCanseling)
                {
                    return;
                }
                string code="",send = "";
                if (string.IsNullOrEmpty(txtPhoneNumber.Text))
                {
                    string[] emaiAndDomain = txtUsername.Text.Split('@');
                    code = await API.getTheOtp(emaiAndDomain[0], emaiAndDomain[1]);
                }
                else
                {
                    send = txtPhoneNumber.Text;
                    code = await API.getOTPFromServer(send);
                }
                
                if (!string.IsNullOrEmpty(code) && txtSmsCode.Tag.ToString() == false.ToString())
                {
                    txtSmsCode.Text = code;
                    btnSubmitSms.PerformClick();
                    return;
                }
               
            }
        }
        private string GetComboBoxApplicationCentreTextSafe()
        {
            if (comboBoxApplicationCentre.InvokeRequired)
            {
                // If we're not on the UI thread, marshal the call to the UI thread
                return (string)comboBoxApplicationCentre.Invoke(new Func<string>(() => comboBoxApplicationCentre.Text));
            }
            else
            {
                // If we're on the UI thread, get the text directly
                return comboBoxApplicationCentre.Text;
            }
        }
        private string GetComboBoAppointmentCategoryTextSafe()
        {
            if (comboBoxAppointmentCategory.InvokeRequired)
            {
                // If we're not on the UI thread, marshal the call to the UI thread
                return (string)comboBoxAppointmentCategory.Invoke(new Func<string>(() => comboBoxAppointmentCategory.Text));
            }
            else
            {
                // If we're on the UI thread, get the text directly
                return comboBoxAppointmentCategory.Text;
            }
        }
        private string GetsiticoneComboBox3TextSafe()
        {
            if (siticoneComboBox3.InvokeRequired)
            {
                // If we're not on the UI thread, marshal the call to the UI thread
                return (string)siticoneComboBox3.Invoke(new Func<string>(() => siticoneComboBox3.Text));
            }
            else
            {
                // If we're on the UI thread, get the text directly
                return siticoneComboBox3.Text;
            }
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            elapsedTime = elapsedTime.Add(TimeSpan.FromSeconds(1)); // Increase elapsed time by 1 second
            
            Invoke(new Action(() => lblTimer.Text = elapsedTime.ToString(@"hh\:mm\:ss")));
        }

        public async Task TryLoadUrlWithTimeout(string url,int load_trys)
        {
            int tryis=0; 
            while (browser.Address != url && tryis <= load_trys)
            {
                await browser.LoadUrlAsync(url);
                await Task.Delay(2000);
                tryis++;
            }

            
        }



        private System.Windows.Forms.Timer timer; // Declare a timer
        private TimeSpan elapsedTime; // To track elapsed time
        bool startLock = false;
        bool resetModemRequrned=false;
        private async Task start()
        {
            Task autoSmsFilerTask = Task.CompletedTask;
            bool erorGeneraterd = false;

            
            
            try
            {
                startLock = true;
                Invoke(new Action(() => lblStatus.Text = "درحال اجرا"));
                if (resetModemRequrned)
                {
                    btnResetModem.PerformClick();
                    resetModemRequrned = false;
                }



                int resultLoop = 0;
                while (startPending)
                {
                    if (resultLoop != -3)
                    {

                        if (webHandelers.chromiumWebBrowser.Address != "https://visa.vfsglobal.com/irn/en/dnk/login")
                        {
                            await TryLoadUrlWithTimeout("https://visa.vfsglobal.com/irn/en/dnk/login",220);
                        }
                        await webHandelers.waitPageReady();

                        if (webHandelers.isCanseling)
                        {
                            return;
                        }
                        await WebTask.page1(webHandelers, txtUsername.Text, txtPassword.Text);

                        Invoke(new Action(() => lblStatus.Text = "در انتظار کد"));
                        autoSmsFilerTask = fillSmsUseingApi();
                        int resulrt = await forcSmsNotEmpty();

                        if (resulrt == 0)
                        {
                            while (!await WebTask.page2(webHandelers, txtSmsCode.Text))
                            {
                                if (webHandelers.isCanseling)
                                {
                                    return;
                                }
                                Invoke(new Action(() => lblStatus.Text = "خطا در کلادفلر تلاش مجدد در 10 ثاتیه"));
                                await customDeley(10);
                            }
                            Invoke(new Action(() =>
                            {

                                txtSmsCode.Text = "";
                                txtSmsCode.Tag = false.ToString();
                            }));
                        }
                    }
                    await Task.Delay(1000);
                    await WebTask.page3(webHandelers);
                    await WebTask.page4(webHandelers, GetComboBoxApplicationCentreTextSafe(), GetComboBoAppointmentCategoryTextSafe(), GetsiticoneComboBox3TextSafe());
                    resultLoop = await getResultAsloopUsingAPI();
                    if (resultLoop == -1)
                    {
                        return;
                    }
                    if (resultLoop == -3)
                    {

                    }
                }



            }
            catch (ThreadAbortException)
            {

                Thread.ResetAbort();
                var cookieManager = browser.GetCookieManager();

                // You can use the `GetCookiesAsync` method to retrieve cookies


            }
            catch (Exception e)
            {
                if (e.Message.Contains("Sorry you have encountered an error"))
                {

                    Cef.GetGlobalCookieManager().DeleteCookies("", "");
                    Invoke(new Action(() => btnRefresh.PerformClick()));
                    browser.GetBrowser().Reload(true);
                    //MessageBox.Show("Please set the proxy and try again");
                    Cef.GetGlobalCookieManager().DeleteCookies("", "");
                    resetModemRequrned = true;
                    //btnResetModem.PerformClick()
                    await Task.Delay(2000);
                }
                string a = browser.GetSourceAsync().Result;
                Invoke(new Action(() => lblStatus.Text = e.Message));
                erorGeneraterd = true;
            }
            finally
            {
                await webHandelers.cansel();
                await autoSmsFilerTask;
                txtSmsCode.Tag = false.ToString();
                if (!erorGeneraterd)
                {
                    Invoke(new Action(() => lblStatus.Text = "در انتظار شروع"));
                }
                timer.Stop();
                await Task.Delay(2000);
                webHandelers.isCanseling = false;
                startLock = false;

            }
            if (startPending)
                await start();

        }
        private Thread mainThread = null;
        
        private async void btnStart_Click(object sender, EventArgs e)
        {
            startPending = true;

            if (!startLock)
            {
                txtSmsCode.Clear();
                webHandelers.isCanseling = false;
                txtSmsCode.Tag = false.ToString();
                timer = new System.Windows.Forms.Timer();
                timer.Interval = 1000; // Set interval to 1 second
                timer.Tick += Timer_Tick;
                elapsedTime = TimeSpan.Zero; // Initialize elapsed time
                timer.Start();
                mainThread = new Thread(async () => { await start(); });
                mainThread.Start();

            }
            else
            {
                btnStop.PerformClick();
            }
            
        }
        private bool startPending=false;
        private  async void siticoneButton1_Click(object sender, EventArgs e)
        {
            
            if (!webHandelers.isCanseling || mainThread.IsAlive)
            {
                webHandelers.cansel().Wait();
                startPending = false;
            }
        }

        private async void btnSubmitSms_Click(object sender, EventArgs e)
        {
            txtSmsCode.Tag = true.ToString();
        }

        private async Task customDeley(int time)
        {
            for (int i = 0; i < time*10; i++)
            {
                if (webHandelers.isCanseling)
                {
                    return;
                }
                await Task.Delay(100);
            }
        }
        private async Task<int> getResultAsloopUsingAPI()
        {
            if (!string.IsNullOrEmpty(customRequestHandler.authorize) && !string.IsNullOrEmpty(customRequestHandler.clientSource))
            {
                while (true)
                {


                    Random rnd = new Random();
                    string requestBodyEscaped = customRequestHandler.requestBodyForLongOrShorStay.Replace("\"", "\\\"");

                    // Wrap the fetch call inside an async function
                    string script = $@"(async () => {{
            const response = await fetch(""https://lift-api.vfsglobal.com/appointment/CheckIsSlotAvailable"", {{
                ""credentials"": ""include"",
                ""headers"": {{
                    ""User-Agent"": ""Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36"",
                    ""Accept"": ""application/json, text/plain, */*"",
                    ""Accept-Language"": ""en-US,en;q=0.5"",
                    ""authorize"": ""{customRequestHandler.authorize}"",
                    ""Content-Type"": ""application/json;charset=utf-8"",
                    ""route"": ""irn/en/dnk"",
                    ""clientSource"": ""{customRequestHandler.clientSource}"",
                    ""Sec-Fetch-Dest"": ""empty"",
                    ""Sec-Fetch-Mode"": ""cors"",
                    ""Sec-Fetch-Site"": ""same-site""
                }},
                ""referrer"": ""https://visa.vfsglobal.com/"",
                ""body"": ""{requestBodyEscaped}"",
                ""method"": ""POST"",
                ""mode"": ""cors""
            }});

            const result = await response.json();
            return  JSON.stringify(result.earliestSlotLists);
        }})()";

                    var result = await browser.GetMainFrame().EvaluateScriptAsync(script);

                    if (result.Success && result.Result != null)
                    {

                        List<SlotInfo> slotInfos = JsonConvert.DeserializeObject<List<SlotInfo>>(result.Result.ToString());

                        string[] results = new string[slotInfos.Count];
                        for (int i = 0; i < slotInfos.Count; i++)
                        {
                            results[i] = slotInfos[i].date;
                        }
                        Invoke(new Action(() => lblStatus.Text = "ارسال دیتا به سرور"));
                        if (await API.SendResultToServer(results, txtPhoneNumber.Text))
                            Invoke(new Action(() => lblStatus.Text = "ارسال شد"));
                        else
                            Invoke(new Action(() => lblStatus.Text = "خطا در ارسال اطلاعات"));
                        int deley = rnd.Next(Convert.ToInt32(txtStartNumber.Text), Convert.ToInt32(txtEndNumber.Text));
                        await Task.Delay(1000);
                        Invoke(new Action(() => lblStatus.Text = $"{deley} ثانیه"));
                        await customDeley(deley - 1);
                        Invoke(new Action(() => lblStatus.Text = "رفرش صفحه"));
                    }
                    else
                    {
                        return -2; // time to loging again
                    }
                }
            }
            return 0;
        }

        private async Task<int> getResultAsLoop()
        {
            
            Random rnd = new Random();
            while (true)
            {
                if (!string.IsNullOrWhiteSpace(await webHandelers.getContentTextFromXPATH("/html/body/app-root/div/header/app-notification/div/span")))
                {
                    throw new Exception(await webHandelers.getContentTextFromXPATH("/html/body/app-root/div/header/app-notification/div/span"));
                }
                else if (!string.IsNullOrWhiteSpace(await webHandelers.getContentTextFromXPATH("/html/body/app-root/div/div/app-login/section/div/div/mat-card/div/div/div")))
                {
                    throw new Exception(await webHandelers.getContentTextFromXPATH("/html/body/app-root/div/div/app-login/section/div/div/mat-card/div/div/div"));
                }
                if (webHandelers.isCanseling)
                {
                    return -1;
                }
                string[] result = await webHandelers.getResultFromPage4();
                Invoke(new Action(() => lblStatus.Text = "ارسال دیتا به سرور"));
                if (await API.SendResultToServer(result, txtPhoneNumber.Text))
                    Invoke(new Action(() => lblStatus.Text = "ارسال شد"));
                else
                    Invoke(new Action(() => lblStatus.Text = "خطا در ارسال اطلاعات"));
                int deley = rnd.Next(Convert.ToInt32(txtStartNumber.Text), Convert.ToInt32(txtEndNumber.Text));
                await Task.Delay(1000);
                Invoke(new Action(() => lblStatus.Text = $"{deley} ثانیه"));
                await customDeley(deley-1);
                Invoke(new Action(() => lblStatus.Text = "رفرش صفحه"));
                if ((bool)comboBoxAppointmentCategory.Invoke(new Func<bool>(() => comboBoxAppointmentCategory.SelectedIndex ==0 )))
                {
                    await webHandelers.SelectFromSelectBox(1, (string)comboBoxAppointmentCategory.Invoke(new Func<string>(() => comboBoxAppointmentCategory.Items[1].ToString())));
                }
                else
                {
                    await webHandelers.SelectFromSelectBox(1, (string)comboBoxAppointmentCategory.Invoke(new Func<string>(() => comboBoxAppointmentCategory.Items[0].ToString())));
                }
                for (int i = 1; i < 4; i++)
                {


                    if (webHandelers.isCanseling)
                        return -1;
                    await Task.Delay((1000 * i) / 2);
                    await webHandelers.waitPageReady();
                    await Task.Delay((1000 * i) / 2);
                    await webHandelers.SelectFromSelectBox(1, GetComboBoAppointmentCategoryTextSafe());
                    
                    await Task.Delay((1000 * i) / 2);
                    await webHandelers.waitPageReady();
                    await Task.Delay((1000 * i) / 2);
                    await webHandelers.SelectFromSelectBox(2, GetsiticoneComboBox3TextSafe());

                    await Task.Delay((1000 * i) / 2);
                    await webHandelers.waitPageReady();
                    await Task.Delay((1000 * i) / 2);
                    string setedVal = await webHandelers.getContentCustomFromSelectBox(2);
                    string mustBeval = GetsiticoneComboBox3TextSafe();


                    if (setedVal == null)
                    {
                        throw new Exception("Sorry you have encountered an error");
                    }
                    if (setedVal == mustBeval)
                    {
                        break;
                    }
                    if ( i >=3 && setedVal != mustBeval)
                    {
                        await webHandelers.clickByXpath("//*[@id=\"navbarDropdown\"]");
                        await Task.Delay(1000);
                        await webHandelers.clickByXpath("/html/body/app-root/div/header/div[1]/div/div/div/div/ul/li[1]/a");// dashbord butten
                        await webHandelers.waitPageReady();
                        return -3;//time to go dashbord
                    }
                }
                
                if (! await webHandelers.canIContnueRefreshing())
                {
                    return -2; // time to loging again
                }
            }
        }

        

        private void txtEndNumber_KeyDown(object sender, KeyEventArgs e)
        {
            // Allow control keys like Backspace, Delete, Enter, etc.
            if (e.KeyCode == Keys.Back || e.KeyCode == Keys.Delete || e.KeyCode == Keys.Enter ||
                e.KeyCode == Keys.Tab || e.KeyCode == Keys.Escape || e.KeyCode == Keys.Left ||
                e.KeyCode == Keys.Right || e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
            {
                return;
            }

            // Allow only number keys (0-9) and Numpad keys (0-9)
            if ((e.KeyCode < Keys.D0 || e.KeyCode > Keys.D9) &&
                (e.KeyCode < Keys.NumPad0 || e.KeyCode > Keys.NumPad9))
            {
                // Suppress the key press event if it is not a number key
                e.SuppressKeyPress = true;
            }
        }

        private void txtStartNumber_TextChanged(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(txtStartNumber.Text))
            {
                txtStartNumber.Text = "0";
            }
            if (Convert.ToInt32(txtStartNumber.Text)>Convert.ToInt32(txtEndNumber.Text))
            {
                txtStartNumber.Text = txtEndNumber.Text;
            }
        }

        

        private void Form1_Load_1(object sender, EventArgs e)
        {
            if (File.Exists("./modemPassword.txt"))
            { 
                string content = File.ReadAllText("./modemPassword.txt");
                txtModemPassword.Text = content;
            }
            txtSmsCode.Tag = false.ToString();
            updateComboBoxUsers();
            if (userIndextoRun > -1)
            {
                comboBoxUsers.SelectedIndex = userIndextoRun;
                btnStart.PerformClick();
            }
        }
        List<string> usersOnComboBoxIdis = new List<string>();
        private void updateComboBoxUsers()
        {
            usersOnComboBoxIdis.Clear();
            comboBoxUsers.Items.Clear();
            DataTable dt= dbCommand.runQuery( "SELECT * FROM users", dbCommand.conctionStringDataBase1);
            foreach (DataRow Row in dt.Rows)
            {
                comboBoxUsers.Items.Add(Row["username"].ToString());
                usersOnComboBoxIdis.Add(Row["Id"].ToString());
            }
        }

        private void comboBoxUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxUsers.SelectedIndex != -1)
            {
                DataTable dt = dbCommand.runQuery($"SELECT * FROM users WHERE Id={usersOnComboBoxIdis[comboBoxUsers.SelectedIndex]}", dbCommand.conctionStringDataBase1);
                loadUser(dt.Rows[0]);
            }
        }
        private void loadUser(DataRow userRow)
        {
            txtUsername.Text = userRow["username"].ToString();
            txtPassword.Text = userRow["password"].ToString();
            txtProxy.Text = userRow["proxy"].ToString();
            txtPhoneNumber.Text = userRow["phone"].ToString();
            comboBoxApplicationCentre.SelectedItem = userRow["combobox1"].ToString();
            comboBoxAppointmentCategory.SelectedItem = userRow["combobox2"].ToString();
            siticoneComboBox3.SelectedItem = userRow["combobox3"].ToString();

            comboBoxApplicationCentre.Refresh();
            comboBoxAppointmentCategory.Refresh();
            siticoneComboBox3.Refresh();
        }

        private void btnNewUser_Click(object sender, EventArgs e)
        {
            
            txtUsername.Clear();
            txtPassword.Clear();
            txtProxy.Clear();
            txtPhoneNumber.Clear();
            txtSmsCode.Clear();
            comboBoxUsers.SelectedIndex = -1;

        }

        private void SaveUser_Click(object sender, EventArgs e)
        {
            string userid;
            bool newUser = false;
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                return;
            }
            if (comboBoxUsers.SelectedIndex != -1)
            {
                userid = usersOnComboBoxIdis[comboBoxUsers.SelectedIndex];
                dbCommand.runQuery($"DELETE FROM users WHERE  Id={userid}", dbCommand.conctionStringDataBase1);
            }
            else
            {
                userid = dbCommand.getLastIdFromUser()+1.ToString();
                
            }

            string insertQuery = $"INSERT INTO users (Id, username, password, proxy, combobox1, combobox2, combobox3, phone) " +
                            $"VALUES ({userid}, '{txtUsername.Text}', '{txtPassword.Text}', '{txtProxy.Text}'," +
                            $" '{comboBoxApplicationCentre.Text}', '{comboBoxAppointmentCategory.Text}', '{siticoneComboBox3.Text}', '{txtPhoneNumber.Text}');";
            dbCommand.runQuery(insertQuery,dbCommand.conctionStringDataBase1);


            updateComboBoxUsers();
            comboBoxUsers.SelectedItem = txtUsername.Text;
            MessageBox.Show("user has ben saved","success",MessageBoxButtons.OK,MessageBoxIcon.Information);
        }

        private void removeUser_Click(object sender, EventArgs e)
        {
            if (comboBoxUsers.SelectedIndex != -1)
            {
                string userid = usersOnComboBoxIdis[comboBoxUsers.SelectedIndex];
                dbCommand.runQuery($"DELETE FROM users WHERE  Id={userid}", dbCommand.conctionStringDataBase1);
                btnNewUser.PerformClick();
                updateComboBoxUsers();
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            browser.LoadUrl("https://api.ipify.org/?format=json");
        }

        private void txtProxy_TextChanged(object sender, EventArgs e)
        {
            string proxy = txtProxy.Text;

            if (string.IsNullOrEmpty(proxy))
            {
                IRequestContext requestC2ontext = browser.GetBrowser().GetHost().RequestContext;
                Cef.UIThreadTaskFactory.StartNew(() =>
                {
                 
                    customRequestHandler.SetProxy(requestC2ontext, proxy);
                });
                return;
            }
            string[] prx = proxy.Split(':');
            if (prx.Length <4)
            {
                MessageBox.Show("The Curect format is ip:port:username:password \nif you have no user name and password try this ip:port::");
                return;
            }
            proxy = prx[0]+":"+prx[1];
            IRequestContext requestContext = browser.GetBrowser().GetHost().RequestContext;

            if (requestContext != null && customRequestHandler != null)
            {

                Cef.UIThreadTaskFactory.StartNew(() =>
                {
                    customRequestHandler.username = prx[2];
                    customRequestHandler.password = prx[3];
                    customRequestHandler.SetProxy(requestContext, proxy);
                });
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            getResultAsloopUsingAPI();
        }

        

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (txtModemPassword.Text !="")
            {
                using (StreamWriter writer = new StreamWriter("./modemPassword.txt"))
                { 
                    writer.Write(txtModemPassword.Text);
                }
            }
            Cef.Shutdown();
        }

        private async void button2_Click_2(object sender, EventArgs e)
        {
            
            await WebTask.resetModem(webHandelers,txtModemLogin.Text,txtModemPassword.Text);
            await WebTask.RestartApplication(comboBoxUsers.SelectedIndex);
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            WebTask.RestartApplication(comboBoxUsers.SelectedIndex);
        }
    }


    public class SlotInfo
    {
        public string applicant { get; set; }
        public string date { get; set; }
    }

}
