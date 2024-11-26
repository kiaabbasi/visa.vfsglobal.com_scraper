using CefSharp.DevTools.DOM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace visa.vfsglobal.com
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            if (args.Length > 0)
            {
                Application.Run(new Form1(Convert.ToInt16(args[0])));
            }
            else
            {
                Application.Run(new Form1());
            }
        }
    }
}
