using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
    
namespace visa.vfsglobal.com
{
    


public class CustomRequestHandler : IRequestHandler
    {
        public string username;
        public string password;

        public string clientSource;
        public string authorize;
        public string requestBodyForLongOrShorStay;
        public CustomRequestHandler(string username = null, string password = null)
        {
            this.username = username;
            this.password = password;
        }

        

      

        public void SetCredentials(string username, string password)
        {
            this.username = username;
            this.password = password;
        }


        public void SetProxy(IRequestContext requestContext, string proxyUrl)
        {
            if (string.IsNullOrEmpty(proxyUrl))
            {
                var proxySettings = new Dictionary<string, object>
            {
                { "mode", "direct" }
            };

                requestContext.SetPreference("proxy", proxySettings, out string error);
                if (!string.IsNullOrEmpty(error))
                {
                    Console.WriteLine($"Error disabling proxy: {error}");
                }
            }
            else
            {
                var proxySettings = new Dictionary<string, object>
            {
                { "mode", "fixed_servers" },
                { "server", proxyUrl }
            };

                requestContext.SetPreference("proxy", proxySettings, out string error);
                if (!string.IsNullOrEmpty(error))
                {
                    Console.WriteLine($"Error setting proxy: {error}");
                }
            }
        }

        public bool GetAuthCredentials(IWebBrowser chromiumWebBrowser, IBrowser browser, string originUrl, bool isProxy, string host, int port, string realm, string scheme, IAuthCallback callback)
        {
            if (isProxy && !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                // ارسال اعتبارنامه‌های احراز هویت پروکسی
                callback.Continue(username, password);
                return true;
            }
            return false;
        }

        
     
        public IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
        {
            if (request.Url.Contains("CheckIsSlotAvailable"))
            {
       
                var clientSource = request.GetHeaderByName("clientSource");
                var authorize = request.GetHeaderByName("authorize");
                if (!string.IsNullOrEmpty(clientSource) && !string.IsNullOrEmpty(authorize)) {
                    if (request.PostData != null)
                    {
                        StringBuilder postDataString = new StringBuilder();
                        

                        foreach (var element in request.PostData.Elements)
                        {
                            if (element.Type == PostDataElementType.Bytes)
                            {
                         
                                string elementString = Encoding.UTF8.GetString(element.Bytes);
                                postDataString.Append(elementString);
                            }
                            
                        }

                       
                        this.requestBodyForLongOrShorStay = postDataString.ToString();
                    }
                    this.authorize = authorize;
                    this.clientSource = clientSource;
                }
            }
            return null;
        }

        public bool OnBeforeBrowse(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool userGesture, bool isRedirect)
        {
            
            return false;
        }

        public bool OnCertificateError(IWebBrowser chromiumWebBrowser, IBrowser browser, CefErrorCode errorCode, string requestUrl, ISslInfo sslInfo, IRequestCallback callback)
        {
            return false;
        }

        public void OnDocumentAvailableInMainFrame(IWebBrowser chromiumWebBrowser, IBrowser browser) 
        {
        }

        public bool OnOpenUrlFromTab(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, string targetUrl, WindowOpenDisposition targetDisposition, bool userGesture)
        {
            return false;
        }

        public void OnRenderProcessTerminated(IWebBrowser chromiumWebBrowser, IBrowser browser, CefTerminationStatus status) { }

        public void OnRenderViewReady(IWebBrowser chromiumWebBrowser, IBrowser browser) { }

        public bool OnSelectClientCertificate(IWebBrowser chromiumWebBrowser, IBrowser browser, bool isProxy, string host, int port, X509Certificate2Collection certificates, ISelectClientCertificateCallback callback)
        {
            return false;
        }
    }



}
