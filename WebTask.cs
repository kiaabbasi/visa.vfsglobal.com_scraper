using CefSharp;
using CefSharp.Handler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace visa.vfsglobal.com
{
    internal class WebTask
    {

        public static async Task page1(WebHandelers webHandelers, string username, string password, bool sendRealKeys = true)
        {
            
            if (webHandelers.chromiumWebBrowser.Address!= "https://visa.vfsglobal.com/irn/en/dnk/login" || ! await webHandelers.isElementExist("email"))
            {
                await webHandelers.chromiumWebBrowser.LoadUrlAsync("https://visa.vfsglobal.com/irn/en/dnk/login");
            }
            await webHandelers.WaitForFrameLoad();
            await webHandelers.waitPageReady();
            await Task.Delay(1000);
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
            if (!string.IsNullOrWhiteSpace(await webHandelers.getTextContentById("errorEmail")))
            {
                throw new Exception(await webHandelers.getTextContentById("errorEmail"));
            }
            else if (! string.IsNullOrWhiteSpace( await webHandelers.getContentTextFromXPATH("/html/body/app-root/div/header/app-notification/div/span")))
            {
                throw new Exception(await webHandelers.getContentTextFromXPATH("/html/body/app-root/div/header/app-notification/div/span"));
            }
            
            if (cludFlareSusses)
            {
                await webHandelers.clickByXpath("/html/body/app-root/div/div/app-login/section/div/div/mat-card/form/button");
            }
            else
            {
                throw new Exception("cloudflare detected us");
            } 
        }

        public static async Task<bool> page2(WebHandelers webHandelers, string smsCode)
        {


            await webHandelers.WaitForFrameLoad();
            while (!await webHandelers.isPageReady())
            {
                if (webHandelers.isCanseling)
                {
                    return false;
                }
                await Task.Delay(1000);
            }

            await webHandelers.sendKeysByXpath("//*[@id=\"mat-input-5\"]", smsCode,true,60,10);
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
        public static async Task page3_4(WebHandelers webHandelers, string selectBox1, string selectBox2, string selectBox3)
        {
            if (webHandelers.isCanseling)
                return;
            

            try
            {
                await webHandelers.waitPageReady();
                await Task.Delay(1000);
                await webHandelers.clickByXpath("/html/body/app-root/div/div/app-dashboard/section[1]/div/div[2]/div/button/span[1]");
                await webHandelers.waitPageReady();
                await Task.Delay(1000);
                //page4
                if (webHandelers.isCanseling)
                    return;
                await webHandelers.SelectFromSelectBox("#mat-select-value-1", selectBox1);
                await Task.Delay(1000);
                await webHandelers.waitPageReady();
                await webHandelers.SelectFromSelectBox("#mat-select-value-5", selectBox2);
                await Task.Delay(1000);
                await webHandelers.waitPageReady();
                await webHandelers.SelectFromSelectBox("#mat-select-value-3", selectBox3);
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
    }
    public class CustomRequestHandler : RequestHandler
    {
        private string proxy;

        public CustomRequestHandler(string initialProxy)
        {
            proxy = initialProxy;
        }

        public void SetProxy(string newProxy)
        {
            proxy = newProxy;
            UpdateProxySettings();
        }

        private void UpdateProxySettings()
        {
            Cef.UIThreadTaskFactory.StartNew(() =>
            {
                var browser = Cef.GetGlobalRequestContext();
                if (!string.IsNullOrEmpty(proxy))
                {
                    var proxyDict = new Dictionary<string, object>
                {
                    { "mode", "fixed_servers" },
                    { "server", proxy }
                };
                    browser.SetPreference("proxy", proxyDict, out string error);
                }
                else
                {
                    var proxyDict = new Dictionary<string, object>
                {
                    { "mode", "direct" }
                };
                    browser.SetPreference("proxy", proxyDict, out string error);
                }
            });
        }

        protected override IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
        {
            return base.GetResourceRequestHandler(chromiumWebBrowser, browser, frame, request, isNavigation, isDownload, requestInitiator, ref disableDefaultHandling);
        }
    }
}