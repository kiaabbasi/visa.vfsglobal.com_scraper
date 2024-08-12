using CefSharp;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace visa.vfsglobal.com
{
    public partial class Form1 : Form
    {
        private ChromiumWebBrowser browser;
        private WebHandelers webHandelers;
        private CustomRequestHandler requestHandler;

        public Form1()
        {
            InitializeComponent();
            InitializeChromium();
        }

        private void InitializeChromium()
        {
            // Initialize CefSharp settings
            var settings = new CefSettings();
            Cef.Initialize(settings);

            // Create the custom request handler with no initial proxy
            requestHandler = new CustomRequestHandler(null);

            // Create a new instance of ChromiumWebBrowser
            browser = new ChromiumWebBrowser("https://visa.vfsglobal.com/irn/en/dnk/login")//https://visa.vfsglobal.com/irn/en/dnk/login
            {
                Dock = DockStyle.Fill,
                RequestHandler = requestHandler // Set the custom request handler
            };

            // Add the browser to the panelMain
            panelBrowser.Controls.Add(browser);
            webHandelers = new WebHandelers(browser);
        }

        

        private void button1_Click(object sender, EventArgs e)
        {
            browser.ShowDevTools();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            webHandelers.RemoveCircle();
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
                else if (await webHandelers.isExistElementByXPATH("/html/body/app-root/div/header/app-notification/div"))
                {
                    throw new Exception(await webHandelers.getContentTextFromXPATH("/html/body/app-root/div/header/app-notification/div"));
                }
                await Task.Delay (1000);
            }
            Invoke(new Action(() => lblStatus.Text = "کد دریافت شد"));
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
                
                string code = await API.getOTPFromServer(txtPhoneNumber.Text);
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
        private System.Windows.Forms.Timer timer; // Declare a timer
        private TimeSpan elapsedTime; // To track elapsed time

        bool startLock = false;
        private async Task start ()
        {
            Task autoSmsFilerTask = Task.CompletedTask;
            bool erorGeneraterd=false;
            try
            {
                startLock = true;
                Invoke(new Action(() => lblStatus.Text = "درحال اجرا"));
                
                while (true)
                {

                    if (webHandelers.isCanseling)
                    {
                        return;
                    }
                    await WebTask.page1(webHandelers, txtUsername.Text, txtPassword.Text);
                    Invoke(new Action(() => lblStatus.Text = "در انتظار کد"));
                    autoSmsFilerTask =fillSmsUseingApi();
                    int resulrt =await forcSmsNotEmpty();
                    if (resulrt == 0)
                    {
                        while (!await WebTask.page2(webHandelers, txtSmsCode.Text))
                        {
                            Invoke(new Action(() => lblStatus.Text = "خطا در کلادفلر تلاش مجدد در 10 ثاتیه"));
                            await customDeley(10);
                        }
                        
                    }
                    await Task.Delay(1000);
                    await WebTask.page3_4(webHandelers, GetComboBoxApplicationCentreTextSafe(), GetComboBoAppointmentCategoryTextSafe(), GetsiticoneComboBox3TextSafe());
                    int resultLoop=await getResultAsLoop();
                    if (resultLoop == -1)
                    {
                        return;
                    }
                }
            
               

            }
            catch (ThreadAbortException)
            {
               
                Thread.ResetAbort(); 
                
            }
            catch(Exception e)
            {
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
                webHandelers.isCanseling = false;
                startLock = false;
            }
        }


        private Thread mainThread = null;
        
        private async void btnStart_Click(object sender, EventArgs e)
        {
            
            
            if (! startLock)
            {
                webHandelers.isCanseling = false;
                txtSmsCode.Tag = false.ToString();
                if (await webHandelers.isPageReady())
                {
                    timer = new System.Windows.Forms.Timer();
                    timer.Interval = 1000; // Set interval to 1 second
                    timer.Tick += Timer_Tick;
                    elapsedTime = TimeSpan.Zero; // Initialize elapsed time
                    timer.Start();
                    mainThread = new Thread(async () => await start());
                    
                    mainThread.Start();
                }
            }
            
        }

        private  async void siticoneButton1_Click(object sender, EventArgs e)
        {
            
            if (!webHandelers.isCanseling || mainThread.IsAlive)
            {
                webHandelers.cansel().Wait();

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
        private async Task<int> getResultAsLoop()
        {
            Random rnd = new Random();
            while (true)
            {
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
                Invoke(new Action(() => lblStatus.Text = $"{deley} ثانیه"));
                await customDeley(deley);
                Invoke(new Action(() => lblStatus.Text = "رفرش صفحه"));
                if ((bool)comboBoxAppointmentCategory.Invoke(new Func<bool>(() => comboBoxAppointmentCategory.SelectedIndex ==0 )))
                {
                    await webHandelers.SelectFromSelectBox("#mat-select-value-5", (string)comboBoxAppointmentCategory.Invoke(new Func<string>(() => comboBoxAppointmentCategory.Items[1].ToString())));
                }
                else
                {
                    await webHandelers.SelectFromSelectBox("#mat-select-value-5", (string)comboBoxAppointmentCategory.Invoke(new Func<string>(() => comboBoxAppointmentCategory.Items[0].ToString())));
                }

                
                await Task.Delay(1000);
                await webHandelers.waitPageReady();
                await webHandelers.SelectFromSelectBox("#mat-select-value-5", GetComboBoAppointmentCategoryTextSafe());
                await Task.Delay(1000);
                await webHandelers.waitPageReady();
                await webHandelers.SelectFromSelectBox("#mat-select-value-3", GetsiticoneComboBox3TextSafe());
                await Task.Delay(1000);
                await webHandelers.waitPageReady();
                if (! await webHandelers.canIContnueRefreshing())
                {
                    return -2;
                }
            }
        }

        private void txtProxy_TextChanged(object sender, EventArgs e)
        {
            string proxy = txtProxy.Text;
            requestHandler.SetProxy(proxy);
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

        private void txtEndNumber_TextChanged(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(txtEndNumber.Text))
            {
                txtEndNumber.Text = "0";
            }
            if (Convert.ToInt32(txtEndNumber.Text) < Convert.ToInt32(txtStartNumber.Text))
            {
                txtEndNumber.Text = txtStartNumber.Text;
            }
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
            txtSmsCode.Tag = false.ToString();
            updateComboBoxUsers();

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
    }
}
