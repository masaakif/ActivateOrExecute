using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
//using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using WindowsAPIs;

namespace ActivateOrExecute
{
    class Startup
    {
        private static Utils u = new Utils();
        
        /*
         * args
         * 
         * -classname=<class name> (part of class name is acceptable)
         * -title=<title>  (part of title is acceptable)
         * -command=<command>
         * -arguments="<args>" quoted by double quotes
         * 
         */
        public static void Main(string[] args)
        {
            foreach (string arg in args)
            {
                if (arg.StartsWith("-classname"))
                {
                    u.className = arg.Substring(11, arg.Length - 11).ToLower();
                }
                else if (arg.StartsWith("-title"))
                {
                    u.title = arg.Substring(7, arg.Length - 7).ToLower();
                }
                else if (arg.StartsWith("-command"))
                {
                    u.command = arg.Substring(9, arg.Length - 9);
                }
                else if (arg.StartsWith("-arguments"))
                {
                    u.arguments = arg.Substring(11, arg.Length - 11);
                }
                else if (arg.StartsWith("-verbose"))
                {
                    if (arg.Length < 9)
                    {
                        u.verbose = "true";
                    }
                    else
                    {
                        u.verbose = arg.Substring(9, arg.Length - 9);
                    }
                }
            }

            if (args.Length == 0)
            {
                System.Windows.Forms.MessageBox.Show("Argumens:\n-classname\n-title\n-command\n-arguments\n-verbose", "Usage");
            }

            if (u.className != "" || u.title != "")
            {
                u.FindThenExecute(callback);
            }

            u.ExecuteCommand();
        }

        private static int callback(Utils u2, IntPtr hWnd, StringBuilder sbClassName, StringBuilder sbWindowText, ref object o)
        {
            if (u2.hasGUI(hWnd))
            {
                Debug.Write(hWnd);
                Debug.WriteLine(" " + sbClassName.ToString() + " : " + sbWindowText.ToString());
                u2.MoveWindowTop(hWnd);
                u2.isRunning = true;
            }

            return 1;
        }
    }
}
