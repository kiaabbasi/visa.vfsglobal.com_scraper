using CefSharp.WinForms;
using CefSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Text.RegularExpressions;
using CefSharp.DevTools.CSS;
using System.Windows.Forms;
using System.IO;
using CefSharp.DevTools.Accessibility;
using System.Xml.Linq;
using System.Reflection;


namespace visa.vfsglobal.com
{
    public  class WebHandelers
    {

        public ChromiumWebBrowser chromiumWebBrowser;
        public bool isCanseling =false;
        public bool pageisloaded = false;


        public WebHandelers(ChromiumWebBrowser chromiumWebBrowser )
        {
            this.chromiumWebBrowser = chromiumWebBrowser;
            
            
        }
        
        public async Task<bool> isExistElementByXPATH(string xpath)
        {
            string script = @$"
            (function() {{
                {{
                    var xpath = '{xpath}';
                    var element = document.evaluate(xpath, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue;
                    return  element !== null && element !== undefined;

                }}
            }})()
        ";

            JavascriptResponse response = await chromiumWebBrowser.GetMainFrame().EvaluateScriptAsync(script);

            
            return (bool)response.Result;
           
        }
        public async Task clickbyQury(string quey)
        {
            string clickscript = $"document.querySelector('{quey}').click();";
            JavascriptResponse response = await chromiumWebBrowser.GetMainFrame().EvaluateScriptAsync(clickscript);
        }
        public async Task<string> getContentCustomFromSelectBox(int selectbox)
        {
            string customQury = $@"
            var g = document.getElementsByClassName('form-group')[{selectbox}];
            var e = g.children[g.children.length - 1];
            var matSelectElement = e.getElementsByTagName('mat-select')[0];
            var contentText = matSelectElement.textContent;
            contentText;";

            JavascriptResponse response = await chromiumWebBrowser.GetMainFrame().EvaluateScriptAsync(customQury);

            if (response.Success && response.Result != null)
            {
                return response.Result.ToString();
            }
            else
            {
                // Logging or handling the error can be done here
                return null;
            }
        }

        public async Task<bool> SelectFromSelectBox(int selectboxCount,string text)
        {
            string scriptTofind = @$"
            var g= document.getElementsByClassName(""form-group"")[{selectboxCount}]
            var e = g.children[g.children.length-1]
            var matSelectElements = e.getElementsByTagName('mat-select')[0].click();";


            JavascriptResponse response1 = await chromiumWebBrowser.GetMainFrame().EvaluateScriptAsync(scriptTofind);
            string scriptToClick = @$"function findAndClick() {{
            // Find all elements with the class 'mat-option-text'
            let elements = document.querySelectorAll('.mat-option-text');

            // Iterate through the elements
            for (let element of elements) {{
        
                if (element.textContent.trim() === '{text}') {{
            
                    element.click();
                    return true; // Return true if the element is found and clicked
                }}
            }}

            return false; // Return false if the element is not found
        }}
        findAndClick();
        ";


            JavascriptResponse response = await chromiumWebBrowser.GetMainFrame().EvaluateScriptAsync(scriptToClick);
            if (response.Success)
            {
                return(bool) response.Result;
            }
            return false;
        }

        public async Task waitPageReady()
        {
            await WaitForFrameLoad();
            
            while (!await isPageReady())
            {
                if (isCanseling)
                {
                    return;
                }
                await Task.Delay(1000);
            }
        }
        private async Task<bool> elmentExistwithClassName(string nm)
        {
            string script = @$"
        (function() {{
            var element = document.querySelector('.{nm}');
            return element !== null;
        }})();";
        if (isCanseling)
            return false;

        string notif = await getContentTextFromXPATH("/html/body/app-root/div/header/app-notification/div/html/body/app-root/div/header/app-notification/div");
        if (!string.IsNullOrWhiteSpace(notif))
        {
            return false;// its disable it
        }

            JavascriptResponse response = await chromiumWebBrowser.GetMainFrame().EvaluateScriptAsync(script);

        if (response.Success && response.Result is bool result)
        {
            return result;
        }

        return false;
        }
        public async Task<bool> isPageReady()
        {
            string script = @"
            (function() {
                var loader = document.getElementById('loader');
                if (loader) {
                    var display = window.getComputedStyle(loader).display;
                    return display === 'none';
                }
                return false;
            })();";

            if (isCanseling)
                return false;

            
            JavascriptResponse response = await chromiumWebBrowser.GetMainFrame().EvaluateScriptAsync(script);

            if (response.Success && response.Result is bool result)
            {
                return result &&  ! await elmentExistwithClassName("ngx-overlay.loading-foreground");
            }

            return false;
        }
        public async Task RemoveCircle()
        {
            string script = @"
                (function() {
                    var canvas = document.getElementsByClassName('dynamicCircle')[0];
                    if (canvas) {
                        canvas.parentNode.removeChild(canvas);
                    }
                })();
            ";

            await chromiumWebBrowser.GetMainFrame().EvaluateScriptAsync(script);
        }
        public async void DrawCircle(int x, int y)
        {
            return;
            string script = $@"
                (function() {{
                    var canvas = document.createElement('canvas');
                    canvas.width = window.innerWidth;
                    canvas.class='dynamicCircle';
                    canvas.height = window.innerHeight;
                    canvas.style.position = 'absolute';
                    canvas.style.top = '0';
                    canvas.style.left = '0';
                    canvas.style.pointerEvents = 'none';
                    document.body.appendChild(canvas);

                    var ctx = canvas.getContext('2d');
                    ctx.beginPath();
                    ctx.arc({x}, {y}, 3, 0, 2 * Math.PI);
                    ctx.fillStyle = 'blue';
                    ctx.fill();
                    ctx.strokeStyle = 'black';
                    ctx.stroke();
                }})();
            ";

            await chromiumWebBrowser.GetMainFrame().EvaluateScriptAsync(script);
        }
        public async Task<string> getContentTextFromXPATH(string xpath)
        {
            string script = $@"
            (function() {{
                var xpath = '{xpath}';
                var element = document.evaluate(xpath, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue;
                var textContent = element ? element.textContent : '';
                return textContent;
                
            }})()
        ";

            var response = await chromiumWebBrowser.GetMainFrame().EvaluateScriptAsync(script);

            return response.Result.ToString();
        }
        public async Task<string> getContentTextFromQurySelector(string qury)
        {
            string script = $@"
            (function() {{
                var element = document.querySelector('{qury}');
                var textContent = element.textContent;
                return textContent;
                
            }})()
        ";

            var response = await chromiumWebBrowser.GetMainFrame().EvaluateScriptAsync(script);
            if (response.Success)
            {


                return response.Result.ToString();
            }
            return null;
        }
    

        public async Task<bool> canIContnueRefreshing()
        {
            string script = @$"// Function to check if either mat-dialog-3 or mat-dialog-1 exists
            function checkMatDialogExists() {{
              const dialog3 = document.getElementById('mat-dialog-3');
              const dialog1 = document.getElementById('mat-dialog-0');
  
              return dialog3 !== null || dialog1 !== null;
            }}

            // Call the function
            checkMatDialogExists();
            ";


            var response = await chromiumWebBrowser.GetMainFrame().EvaluateScriptAsync(script);
            return !(bool)response.Result;
        }

        public async Task sendKeysByXpath(string xpath, string value,bool cleanBefore=true,int randomXRange=20,int randomYRange=10)
        {

            // JavaScript to find the element by XPath and click it
            string script = $@"
        (function() {{
            var xpath = '{xpath}';
            var element = document.evaluate(xpath, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue;
            if (element) {{
                var rect = element.getBoundingClientRect();
                return {{ x: rect.left + window.scrollX, y: rect.top + window.scrollY }};
            }}
            return null;
        }})()
    ";

            // Execute the JavaScript on the main frame and get the result
            if (cleanBefore)
            {
                await Task.Delay(800);
            }
            await Task.Delay(200);
            var response = await chromiumWebBrowser.GetMainFrame().EvaluateScriptAsync(script);
            
            if (response.Success && response.Result != null)
            {
                if (isCanseling)
                    return;
                Random random = new Random();
                dynamic result = response.Result;
                int x=(int)result.x + 5 + random.Next(randomXRange);
                int y=(int)result.y + 5 + random.Next(randomYRange);
                DrawCircle(x, y);
                chromiumWebBrowser.GetBrowser().GetHost().SendMouseClickEvent(x,y, MouseButtonType.Left, true, 1, CefEventFlags.None);
                await Task.Delay(random.Next(10,100));
                chromiumWebBrowser.GetBrowser().GetHost().SendMouseClickEvent(x, y, MouseButtonType.Left, false, 1, CefEventFlags.None);
                if (cleanBefore)
                {
                    //await setValueBuyXpath(xpath, "");
                    await Task.Delay(random.Next(30, 100));
                    chromiumWebBrowser.GetBrowser().GetHost().SendKeyEvent(new KeyEvent { WindowsKeyCode = (int)Keys.A, Modifiers = CefEventFlags.ControlDown, Type = KeyEventType.RawKeyDown });
                    await Task.Delay(random.Next(10, 70));
                    chromiumWebBrowser.GetBrowser().GetHost().SendKeyEvent(new KeyEvent { WindowsKeyCode = (int)Keys.A, Modifiers = CefEventFlags.ControlDown, Type = KeyEventType.KeyUp });
                    await Task.Delay(random.Next(10, 70));
                    chromiumWebBrowser.GetBrowser().GetHost().SendKeyEvent(new KeyEvent { WindowsKeyCode = (int)Keys.Delete, Type = KeyEventType.RawKeyDown });
                    await Task.Delay(random.Next(10, 60));
                    chromiumWebBrowser.GetBrowser().GetHost().SendKeyEvent(new KeyEvent { WindowsKeyCode = (int)Keys.Delete, Type = KeyEventType.KeyUp });

                }
                foreach (char c in value)
                {
                    if (isCanseling)
                        return;
                    chromiumWebBrowser.GetBrowser().GetHost().SendKeyEvent(new KeyEvent { WindowsKeyCode = (int)c, Type = KeyEventType.Char });
                    await Task.Delay(random.Next(10, 90));
                }
                

            }

        }
        public async Task setValueBuyXpath(string xpath, string value)
        {
            string script = $@"
var result = document.evaluate('{xpath}', document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null);
var element = result.singleNodeValue;
if (element) {{
    element.value = '{value}';
}} else {{
    console.error('No element found for the given xpath');
}}";

            // Evaluate the script in the browser
            var response = await chromiumWebBrowser.GetMainFrame().EvaluateScriptAsync(script);
        }
        public async Task<string[]> getResultFromPage4()
        {
            string script = @"
            (function() {
                var rtn = [];
                var d1 = document.getElementsByClassName('border-info mb-0 ng-star-inserted');
                if (d1.length > 0) {
                    for (let index = 0; index < d1[0].children.length; index++) {
                        rtn.push(d1[0].children[index].textContent.trim());
                    }
                }
                return rtn;
            })();
        ";

            var response = await chromiumWebBrowser.GetMainFrame().EvaluateScriptAsync(script);

            if (response.Success && response.Result is List<object> resultList)
            {
                return resultList.Cast<string>().ToArray();
            }
            else
            {
                return new string[0];
            }
        }



        public async Task<bool> isCloudFlareSucses()
        {
            return !String.IsNullOrWhiteSpace(await getAttributeByXpath("//*[@name=\"cf-turnstile-response\"]","value"));
        }
        public async Task<string> getAttributeByXpath( string xpath, string attr)
        {
            // Define the JavaScript code to be injected
            string script = $@"
            (function() {{
                var xpathExpression = '{xpath}';
                var result = document.evaluate(xpathExpression, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null);
                var element = result.singleNodeValue;
                if (element) {{
                    return element.getAttribute('{attr}');
                }} else {{
                    return null;
                }}
            }})();";

            // Execute the JavaScript code
            var response = await chromiumWebBrowser.GetMainFrame().EvaluateScriptAsync(script);

            // Check the result and return the attribute value if found
            if (response.Success && response.Result != null)
            {
                return response.Result.ToString();
            }
            else
            {
                // Handle the case where the element or attribute is not found
                return null;
            }
        }
        public async Task clickByXpath(string xpath)
        {

            // JavaScript to find the element by XPath and click it
            string script = $@"
            var xpath = '{xpath}';
            var element = document.evaluate(xpath, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue;
            if (element) {{
                element.click();
             }}";
            if (isCanseling)
            {
                return;
            }
            // Execute the JavaScript on the main frame
            var javascriptResponse=  await chromiumWebBrowser.GetMainFrame().EvaluateScriptAsync(script);

        }
        public async Task cansel()
        {
            this.isCanseling = true;
        }
        public async Task<dynamic> getPointOfElemntStartFromTopLeft(string qurySelector)
        {
            // Evaluate JavaScript code
            JavascriptResponse response = await chromiumWebBrowser.GetMainFrame().EvaluateScriptAsync(@$"
            // Select the element
var element = document.querySelector('{qurySelector}');

// Get the bounding rectangle of the element
var rect = element ? element.getBoundingClientRect() : {{ top: null, left: null }};

// Calculate the position in the document
var positionInDocument = {{
    top: (rect.top !== null ? rect.top + window.pageYOffset  : null),
    left: (rect.left !== null ? rect.left + window.pageXOffset  : null)
}};

// Output the position in the document
positionInDocument;

        ");

            // Check if the script execution was successful
            if (response.Success && response.Result != null)
            {
                // Cast the result to dynamic type
                dynamic result = response.Result;

                // Optionally, you can cast to a more specific type if you know the structure
                // var result = (Dictionary<string, object>)response.Result;

                // Extract values from the result
                var top = (double?)result.top;
                var left = (double?)result.left;

                // Check if values are valid
                if (top.HasValue && left.HasValue)
                {
                    // Return the result as a dynamic object or any other desired type
                    return new
                    {
                        Top = top.Value,
                        Left = left.Value
                    };
                }
                else
                {
                    // Handle case where values are null
                    throw new InvalidOperationException("Element not found or position could not be calculated.");
                }
            }
            else
            {
                // Handle script execution failure
                throw new InvalidOperationException("JavaScript evaluation failed or returned null.");
            }
        }
        public async Task<bool> ClickByPoint(double x, double y)
        {
            string script = $@"
        var event = new MouseEvent('click', {{
            view: window,
            bubbles: true,
            cancelable: true,
            clientX: {x},
            clientY: {y}
        }});
        
        // Dispatch the event to the document
        document.dispatchEvent(event);
    ";

            // Evaluate the script
            var response = await chromiumWebBrowser.GetMainFrame().EvaluateScriptAsync(script);

            return response.Success;
        }
        public  async Task<string> getAriaHiddenValue(string elementId)
        {
            string script = $@"
            var element = document.getElementById('{elementId}');
            var ariaHidden = element ? element.getAttribute('aria-hidden') : null;
            ariaHidden;";
            await WaitForFrameLoad();
            var response = await chromiumWebBrowser.GetMainFrame().EvaluateScriptAsync(script);

            if (response.Success && response.Result is string ariaHiddenValue)
            {
                return ariaHiddenValue;
            }

            return "false"; // Handle the case where the attribute value couldn't be retrieved
        }
        public async Task<List<string>> getSelectItemsAsync(string selectId)
        {
            string script = $@"
               var select = document.getElementById('ctl00_ContentPlaceHolder1_ddlBankName');
                var options = select.options;
                var optionValues = [];

                for (var i = 0; i < options.length; i++) {{
                  if (options[i]) {{
                    optionValues.push(options[i].text);
                  }}
                }}

                optionValues.join(';');

            ";
            if (chromiumWebBrowser.CanExecuteJavascriptInMainFrame)
            {

                await WaitForFrameLoad();
                var response = await chromiumWebBrowser.GetMainFrame().EvaluateScriptAsync(script);
                if (response.Success && response.Result != null)
                {
                    string optionValues = response.Result.ToString();
                    string[] valuesArray = optionValues.Split(';');

                    return new List<string>(valuesArray);
                }
            }

            return new List<string>();
        }
        private bool AreListsEqual(List<string> list1, IEnumerable<string> list2)
        {
            
            return new HashSet<string>(list1).SetEquals(new HashSet<string>(list2));
        }
        public async Task updateBankList(ListBox listBox)
        {
            List<string> selectItems;
            do
            {

                selectItems = await getSelectItemsAsync("ctl00_ContentPlaceHolder1_ddlBankName");

            } while (selectItems.Count <= 0 && !isCanseling);
            if (selectItems.Count > 0)
            {
                selectItems.RemoveAt(0);
            }




            if (!AreListsEqual(selectItems, listBox.Items.Cast<string>()))
            {
                listBox.Invoke(new Action(() => {
                    listBox.Items.Clear();
                    listBox.Items.AddRange(selectItems.ToArray());
                }));

            }
        }
        public async Task<DataRow> selectFromBanks(DataTable banks,bool hozory=false)
        {
            

            foreach (DataRow row in banks.Rows)
            {
                
                                                            
                if (isCanseling)
                {
                    return null;
                }

               


                string pattern =row["name"].ToString();
                
                bool result=await SelectItemFromItem("ctl00_ContentPlaceHolder1_ddlBankName", pattern);
                
                result = await GetSelectedIndexFromSelect("ctl00_ContentPlaceHolder1_ddlBankName") != 0 && result;
                if (!result)
                    continue;
                /*bool need2wait = false;
                do
                {

                    await Task.Delay(100);
                    need2wait = (await getAriaHiddenValue("ctl00_ContentPlaceHolder1_updateProgress") == "false");
                    if (isCanseling)
                    {
                        return null;
                    }

                }

                while (need2wait  );//i dont know it*/
                
                await Task.Delay(1000);
                if ( await isElementExist("ctl00_ContentPlaceHolder1_divHozoriType")) // i chek the hozori chekbox
                {
                    if (!hozory)
                    {
                        await ClickElementById("ctl00_ContentPlaceHolder1_rbtnGheireHozori");
                        return row;
                    }
                    else
                    {
                        
                        
                        await ClickElementById("ctl00_ContentPlaceHolder1_rbtnHozori");
                        
                        do
                        {

                            await Task.Delay(100);
                         
                            if (isCanseling)
                            {
                                return null;
                            }

                        }
                        while (await isSelectEmpty("ctl00_ContentPlaceHolder1_lstBoxSuggShb"));
                    }
                    

                }
                if (result)
                {
                    if (string.IsNullOrEmpty(row["branchName"].ToString())|| string.IsNullOrWhiteSpace(row["branchName"].ToString()) || row["branchName"].ToString()==" ")
                    {
                        await SelectFromSelectByIndex("ctl00_ContentPlaceHolder1_lstBoxSuggShb", 1);
                    }
                    else
                    {

                        //System.Windows.Forms.MessageBox.Show(row["branchName"].ToString());
                        await sendKeyToId("ctl00_ContentPlaceHolder1_tbSuggShbSearch", row["branchName"].ToString());
                        await WaitForFrameLoad();
                        //Click image butten  (search)
                        await chromiumWebBrowser.EvaluateScriptAsync("var button = document.getElementById('ctl00_ContentPlaceHolder1_imgBtnSearchSuggShb');\r\nif (button) {\r\n    button.click();\r\n}\r\n");//Click image butten  (search)
                        
                       
                    }
                    
                    
                    if (await GetSelectedIndexFromSelect("ctl00_ContentPlaceHolder1_lstBoxSuggShb") == 0)
                        return await selectFromBanks(banks,hozory);
                    int selectedIndex = await GetSelectedIndexFromSelect("ctl00_ContentPlaceHolder1_lstBoxSuggShb");
                    if (selectedIndex != 0)
                    {

                        return row;
                    }
                }
            }
            return null;
        }
        public async Task<bool> isSelectEmpty(string id)
        {
            string script = $"document.getElementById('{id}').options.length === 0";
            await WaitForFrameLoad();
            var response = await chromiumWebBrowser.GetMainFrame().EvaluateScriptAsync(script);

            if (response.Success && response.Result is bool isEmpty)
            {
                return isEmpty;
            }

            return true; // Assume it's empty in case of any issues with the script execution or response
        }

        public async Task<string> getTextContentById(string elementId)
        {
            string script = $@"
            var element = document.getElementById('{elementId}');
            var textContent = element ? element.textContent : null;
            textContent;";
            await WaitForFrameLoad();
            var response = await chromiumWebBrowser.GetMainFrame().EvaluateScriptAsync(script);

            if (response.Success && response.Result is string textContent)
            {
                return textContent;
            }

            return null; // Handle the case where the text content couldn't be retrieved
        }
        public async Task<bool> SelectFromSelectByRejex(string elementId, string itemRegexPattern, bool canEmpty = false)
        {
            if (isCanseling)
            {
                return false;
            }

            if (!canEmpty)
            {
                string script1 = @"
                var selectElement = document.getElementById('" + elementId + @"');
                selectElement.selectedIndex !== -1;
                ";
                        JavascriptResponse response1;
                        do
                        {
                            await Task.Delay(20);
                            await WaitForFrameLoad();
                            response1 = await chromiumWebBrowser.GetMainFrame().EvaluateScriptAsync(script1);
                            if (!response1.Success)
                                response1.Result = false;
                        } while (!(bool)response1.Result);
                    }

                    string script2 = @"
            var itemRegex = new RegExp('" + itemRegexPattern + @"');

            var selectElement = document.getElementById('" + elementId + @"');

            for (var i = 0; i < selectElement.options.length; i++) {
                if (itemRegex.test(selectElement.options[i].text)) {
                    // Set the 'selected' attribute to true for the matching option
                    selectElement.selectedIndex = i;
                    break; // Exit the loop once a match is found
                }
            }

            // Trigger the 'onchange' event to initiate the postback
            var onchangeEvent = new Event('change');
            selectElement.dispatchEvent(onchangeEvent);
            ";

                    Random random = new Random();
                    await WaitForFrameLoad();
                    JavascriptResponse response = await chromiumWebBrowser.GetMainFrame().EvaluateScriptAsync(script2);
                    await Task.Delay(random.Next(40, 300));

                    return response.Success;
                }


        public async Task<bool> SelectFromSelect(string elementId, string itemText, bool canEmpty = false)
        {
            //itemText.Replace('ی', 'ي');
            if (isCanseling)
            {
                return false;
            }
            if (!canEmpty)
            {
                string script1 = @"
                var selectElement = document.getElementById('" + elementId + @"');
                selectElement.selectedIndex !== -1;
                ";
                JavascriptResponse response1;
                do
                {
                    await Task.Delay(20);
                    response1 = await chromiumWebBrowser.GetMainFrame().EvaluateScriptAsync(script1);
                    if (!response1.Success)
                        response1.Result = false;
                } while (!(bool)response1.Result);




            }
            string script2 = @"
            var desiredValue = '" + itemText + @"';

            
            var selectElement = document.getElementById('" + elementId + @"');

           
            for (var i = 0; i < selectElement.options.length; i++) {
                if (selectElement.options[i].text === desiredValue) {
                    // Set the 'selected' attribute to true for the matching option
                    selectElement.options[i].selected = true;
                    break; // Exit the loop once a match is found
                }
            }

            // Trigger the 'onchange' event to initiate the postback
            var onchangeEvent = new Event('change');
            selectElement.dispatchEvent(onchangeEvent);
            ";


            Random random = new Random();

            JavascriptResponse response = await chromiumWebBrowser.GetMainFrame().EvaluateScriptAsync(script2);
            await Task.Delay(random.Next(40, 300));

            return response.Success;
        }
        public async Task<bool> SelectItemFromItem(string elementId, string itemText)
        {
            itemText = itemText.Replace('ی', 'ي');
            string script = @"
            var desiredValue = '" + itemText + @"';
            var selectElement = document.getElementById('" + elementId + @"');
            var selectedIndex = -1;

            for (var i = 0; i < selectElement.options.length; i++) {
                var optionText = selectElement.options[i].textContent;

                // Replace Persian 'ی' character with 'ي' in the optionText
                optionText = optionText.replace(/ی/g, 'ي');

                if (optionText.includes(desiredValue)) {
                    selectedIndex = i;
                    break; // Exit the loop once a match is found
                }
            }

            selectedIndex;
        ";
            JavascriptResponse response = await chromiumWebBrowser.GetMainFrame().EvaluateScriptAsync(script);
            if (response.Success)
            {
                await SelectFromSelectByIndex(elementId, (int)response.Result);
            }
            
            
            return response.Success && response.Result.ToString() != "-1";
        }


        public async Task sendKeyToId(string id, string value,bool runEvent=true)
        {
            
            try
            {
                if (isCanseling)
                {
                    return;
                }
                string clearScript = $"document.getElementById('{id}').value = '';";
                await chromiumWebBrowser.EvaluateScriptAsync(clearScript);
                if (runEvent)
                {

                    await sendKeysByXpath($"//*[@id=\"{id}\"]", value[0].ToString());
                    value = value.Substring(1);
                }
                

                string script = $"document.getElementById('{id}').value += '{value}';";
                await chromiumWebBrowser.EvaluateScriptAsync(script);

                
                
            }
            catch
            {

            }
        }
        public async Task WaitForFrameLoad()
        {
            
            await chromiumWebBrowser.WaitForInitialLoadAsync(); 
            
        }
        

        public async Task<bool> isElementExist(string elementId)
        {
            string script = @"
        var element = document.getElementById('" + elementId + @"');
        element !== null && element !== undefined;";


            await WaitForFrameLoad();
            if (!chromiumWebBrowser.CanExecuteJavascriptInMainFrame)
            {
                await Task.Delay(1000);
                return await isElementExist(elementId);
            }
            JavascriptResponse response = await chromiumWebBrowser.GetMainFrame().EvaluateScriptAsync(script);
           

            if (response.Success && response.Result is bool)
            {
                return (bool)response.Result;
            }

            return false;
        }

        public async Task SelectFromSelectByIndex(string id, int index)
        {
            if (isCanseling)
            {
                return;
            }
            string script = @"
            var selectElement = document.getElementById('" + id + @"');
            selectElement.selectedIndex = " + index.ToString() + @";

            // Trigger the 'change' event to simulate human-like selection
            var onchangeEvent = new Event('change');
            selectElement.dispatchEvent(onchangeEvent);";
            await WaitForFrameLoad();
            await chromiumWebBrowser.EvaluateScriptAsync(script);
        }

        public async Task<bool> ClickElementById(string elementId)
        {
            string script = @"
        var element = document.getElementById('" + elementId + @"');
if (element) {
    // Simulate a human-like click
    element.click();
    true;
} else {
    false;
}
";
            if (isCanseling)
            {
                return false;
            }
            JavascriptResponse response = await chromiumWebBrowser.GetMainFrame().EvaluateScriptAsync(script);

            return response.Success && response.Result is bool && (bool)response.Result;
        }
        public async Task<string> GetValueById(string id)
        {
            if (isCanseling)
            {
                return null; // or some default value
            }

            string script = @"
            var element = document.getElementById('" + id + @"');
            var value = element.value;

            value;";
            await WaitForFrameLoad();
            // Evaluate the JavaScript code and get the result
            var response = await chromiumWebBrowser.EvaluateScriptAsync(script);

            // Check if the script execution was successful and get the result
            if (response.Result != null && response.Success)
            {
                return response.Result.ToString();
            }
            else
            {
                // Handle error or return default value
                return null; // or some default value
            }
        }
        
        
        public async Task<int> GetSelectedIndexFromSelect(string id)
        {
            string script = @"
                (function() {
                    var selectElement = document.getElementById('" + id + @"');
                    var selectedIndex = selectElement.selectedIndex;
                    return selectedIndex;
                })();
            ";
            await WaitForFrameLoad();
            CefSharp.JavascriptResponse response = await chromiumWebBrowser.GetMainFrame().EvaluateScriptAsync(script);
            int index;
            
            if (!int.TryParse(response.Result?.ToString(), out index))
            {
                index = 0;
            }

            return index;
        }
    }
}
