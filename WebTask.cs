using CefSharp;
using CefSharp.Handler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace visa.vfsglobal.com
{
    internal class WebTask
    {
        public static async Task rasieForErrors(WebHandelers webHandelers)
        {
            for (int i = 0; i < 10; i++)
            {


                if (!string.IsNullOrWhiteSpace(await webHandelers.getTextContentById("errorEmail")))
                {
                    throw new Exception(await webHandelers.getTextContentById("errorEmail"));
                }
                else if (!string.IsNullOrWhiteSpace(await webHandelers.getContentTextFromXPATH("/html/body/app-root/div/header/app-notification/div/span")))
                {
                    throw new Exception(await webHandelers.getContentTextFromXPATH("/html/body/app-root/div/header/app-notification/div/span"));
                }
                else if (!string.IsNullOrWhiteSpace(await webHandelers.getContentTextFromXPATH("/html/body/app-root/div/div/app-login/section/div/div/mat-card/div/div/div")))
                {
                    throw new Exception(await webHandelers.getContentTextFromXPATH("/html/body/app-root/div/div/app-login/section/div/div/mat-card/div/div/div"));
                }
                else if (webHandelers.isCanseling)
                {
                    return;
                }
                await Task.Delay(5000);
            }
        }
        public static async Task page1(WebHandelers webHandelers, string username, string password, bool sendRealKeys = true)
        {

            if (webHandelers.chromiumWebBrowser.Address != "https://visa.vfsglobal.com/irn/en/dnk/login" || !await webHandelers.isElementExist("email"))
            {
                await webHandelers.chromiumWebBrowser.LoadUrlAsync("https://visa.vfsglobal.com/irn/en/dnk/login");
            }
            await webHandelers.WaitForFrameLoad();
            await webHandelers.waitPageReady();
            await Task.Delay(1000);
            await acseptCoocieIfExsist(webHandelers);
            rasieForErrors(webHandelers);
            if (sendRealKeys)
            {

                await webHandelers.sendKeysByXpath("//*[@id=\"email\"]", username);
            }
            else
            {
                await webHandelers.sendKeyToId("email", username);
            }

            await Task.Delay(1000);
            if (webHandelers.isCanseling)
            {
                return;
            }
            if (sendRealKeys)
            {
                await webHandelers.sendKeysByXpath("//*[@id=\"password\"]", password);
            }
            else
            {
                await webHandelers.sendKeyToId("password", password);
            }


            await Task.Delay(1000);
            if (webHandelers.isCanseling)
            {
                return;
            }

            var result = await webHandelers.getPointOfElemntStartFromTopLeft("body > app-root > div > div > app-login > section > div > div > mat-card > form > app-cloudflare-captcha-container");
            int x = (int)result.Left + 30;
            int y = (int)result.Top + 30;
            if (webHandelers.isCanseling)
            {
                return;
            }
            webHandelers.DrawCircle(x, y);
            webHandelers.chromiumWebBrowser.GetBrowser().GetHost().SendMouseClickEvent(x, y, MouseButtonType.Left, false, 1, CefEventFlags.None);
            Thread.Sleep(100);  // Use Thread.Sleep to ensure it's caught by ThreadAbortException
            webHandelers.chromiumWebBrowser.GetBrowser().GetHost().SendMouseClickEvent(x, y, MouseButtonType.Left, true, 1, CefEventFlags.None);

            bool cludFlareSusses = false;
            for (int i = 0; i < 5; i++)
            {
                if (webHandelers.isCanseling)
                {
                    return;
                }
                await Task.Delay(1000);
                if (await webHandelers.isCloudFlareSucses())
                {
                    cludFlareSusses = true;
                    break;
                }
            }


            if (cludFlareSusses)
            {
                await webHandelers.clickByXpath("/html/body/app-root/div/div/app-login/section/div/div/mat-card/form/button");
                await webHandelers.waitPageReady();
                if (!string.IsNullOrWhiteSpace(await webHandelers.getTextContentById("errorEmail")))
                {
                    throw new Exception(await webHandelers.getTextContentById("errorEmail"));
                }
                else if (!string.IsNullOrWhiteSpace(await webHandelers.getContentTextFromXPATH("/html/body/app-root/div/header/app-notification/div/span")))
                {
                    throw new Exception(await webHandelers.getContentTextFromXPATH("/html/body/app-root/div/header/app-notification/div/span"));
                }
                else if (!string.IsNullOrWhiteSpace(await webHandelers.getContentTextFromXPATH("/html/body/app-root/div/div/app-login/section/div/div/mat-card/div/div/div")))
                {
                    throw new Exception(await webHandelers.getContentTextFromXPATH("/html/body/app-root/div/div/app-login/section/div/div/mat-card/div/div/div"));
                }
            }
            else
            {
                throw new Exception("cloudflare detected us");
            }
        }

        public static async Task<bool> page2(WebHandelers webHandelers, string smsCode)
        {

            await webHandelers.WaitForFrameLoad();
            await acseptCoocieIfExsist(webHandelers);
            while (!await webHandelers.isPageReady())
            {
                if (webHandelers.isCanseling)
                {
                    return false;
                }
                await Task.Delay(1000);
            }

            await webHandelers.sendKeysByXpath("//*[@id=\"mat-input-5\"]", smsCode, true, 60, 10);
            var result = await webHandelers.getPointOfElemntStartFromTopLeft("body > app-root > div > div > app-login > section > div > div > mat-card > form > app-cloudflare-captcha-container");
            //MessageBox.Show($"{result.Top},{result.Left}");
            int x = (int)result.Left + 30;
            int y = (int)result.Top + 30;
            webHandelers.chromiumWebBrowser.GetBrowser().GetHost().SendMouseClickEvent(x, y, MouseButtonType.Left, false, 1, CefEventFlags.None);
            System.Threading.Thread.Sleep(100);
            webHandelers.chromiumWebBrowser.GetBrowser().GetHost().SendMouseClickEvent(x, y, MouseButtonType.Left, true, 1, CefEventFlags.None);
            await Task.Delay(4000);

            bool cludFlareSusses = false;
            for (int i = 0; i < 5; i++)
            {

                await Task.Delay(1000);
                if (await webHandelers.isCloudFlareSucses())
                {
                    cludFlareSusses = true;
                    break;
                }
            }
            if (cludFlareSusses)
            {
                await webHandelers.clickByXpath("/html/body/app-root/div/div/app-login/section/div/div/mat-card/form/button");
                return true;
            }
            return false;
        }
        public static async Task page4(WebHandelers webHandelers, string selectBox1, string selectBox2, string selectBox3)
        {
            await webHandelers.waitPageReady();
            if (webHandelers.isCanseling)
                return;
            try
            {
                await acseptCoocieIfExsist(webHandelers);
                await webHandelers.SelectFromSelectBox(0, selectBox1);
                await Task.Delay(1000);
                await webHandelers.waitPageReady();
                await webHandelers.SelectFromSelectBox(1, selectBox2);
                await Task.Delay(1000);
                await webHandelers.waitPageReady();
                await webHandelers.SelectFromSelectBox(2, selectBox3);
                await Task.Delay(1000);
                await webHandelers.waitPageReady();
            }
            catch (Exception e)
            {

                if (string.IsNullOrWhiteSpace(await webHandelers.getContentTextFromXPATH("/html/body/app-root/div/div/app-login/section/div/div/mat-card/div/div")))
                {
                    throw new Exception(await webHandelers.getContentTextFromXPATH("/html/body/app-root/div/div/app-login/section/div/div/mat-card/div/div"));
                }
                else if (string.IsNullOrWhiteSpace(await webHandelers.getContentTextFromXPATH("/html/body/app-root/div/div/app-login/section/div/div/mat-card/div/div")))
                {
                    throw new Exception(await webHandelers.getContentTextFromXPATH("/html/body/app-root/div/div/app-login/section/div/div/mat-card/div/div"));
                }
                else
                {
                    throw e;
                }
            }
        }
        public static async Task page3(WebHandelers webHandelers)
        {
            if (webHandelers.isCanseling)
                return;

            try
            {
                await webHandelers.waitPageReady();
                await Task.Delay(1000);
                await acseptCoocieIfExsist(webHandelers);
                if (webHandelers.chromiumWebBrowser.Address == "https://visa.vfsglobal.com/irn/en/dnk/login")
                {
                    webHandelers.chromiumWebBrowser.Reload();
                    throw new Exception("page 2 fail");
                }
                if (!string.IsNullOrEmpty(await webHandelers.getContentTextFromXPATH("/html/body/app-root/div/div/app-login/section/div/div/mat-card/div/div")))
                {
                    throw new Exception(await webHandelers.getContentTextFromXPATH("/html/body/app-root/div/div/app-login/section/div/div/mat-card/div/div"));
                }
                await webHandelers.clickByXpath("/html/body/app-root/div/div/app-dashboard/section[1]/div/div[2]/div/button/span[1]");
                await webHandelers.waitPageReady();
                await Task.Delay(1000);
                //page4


            }
            catch (Exception e)
            {

                if (string.IsNullOrWhiteSpace(await webHandelers.getContentTextFromXPATH("/html/body/app-root/div/div/app-login/section/div/div/mat-card/div/div")))
                {
                    throw new Exception(await webHandelers.getContentTextFromXPATH("/html/body/app-root/div/div/app-login/section/div/div/mat-card/div/div"));
                }
                else if (string.IsNullOrWhiteSpace(await webHandelers.getContentTextFromXPATH("/html/body/app-root/div/div/app-login/section/div/div/mat-card/div/div")))
                {
                    throw new Exception(await webHandelers.getContentTextFromXPATH("/html/body/app-root/div/div/app-login/section/div/div/mat-card/div/div"));
                }
                else
                {
                    throw e;
                }
            }



        }
        public static async Task acseptCoocieIfExsist(WebHandelers webHandelers)
        {
            await webHandelers.ClickElementById("onetrust-accept-btn-handler");
        }





        public static async Task resetModem(WebHandelers webHandelers, string loginpage, string password)
        {
            webHandelers.isCanseling = false;
            await webHandelers.chromiumWebBrowser.LoadUrlAsync(loginpage);
            await webHandelers.simpleSetValue("login_password", password);
            await webHandelers.ClickElementById("login_btn");
            await webHandelers.WaitForFrameLoad();
            await Task.Delay(3000);
            await webHandelers.clickbyQury("#header_right > table > tbody > tr > td:nth-child(4) > div");
            await Task.Delay(3000);
            await webHandelers.chromiumWebBrowser.GetMainFrame().EvaluateScriptAsync("EMUI.RebootController.rebootExe()");
            await API.waitOnline(webHandelers);
        }
        public static async Task RestartApplication(int user_index)
        {
            // Get the path to the current executable
            string exePath = Application.ExecutablePath;

            // Prepare the cmd command to wait for 2 seconds and then restart the app
            string cmdArgs = $"/C timeout /T 2 & \"{exePath}\" {user_index}";

            // Start cmd process with the timeout and restart logic
            Process.Start(new ProcessStartInfo("cmd.exe", cmdArgs)
            {
                CreateNoWindow = true,
                UseShellExecute = false
            });

            // Close the current instance of the application
            Application.Exit();
        }
    }

}