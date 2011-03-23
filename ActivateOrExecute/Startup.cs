using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
//using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace ActivateOrExecute
{
    class Startup
    {
        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, ref uint procId);

        // コールバックメソッドのデリゲート
        private delegate int EnumerateWindowsCallback(IntPtr hWnd, int lParam);

        [DllImport("user32", EntryPoint = "EnumWindows")]
        private static extern int EnumWindows(EnumerateWindowsCallback lpEnumFunc, int lParam);

        [DllImport("User32.Dll", CharSet = CharSet.Unicode)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder s, int nMaxCount);

        [DllImport("User32.Dll", CharSet = CharSet.Unicode)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder s, int nMaxCount);

        [DllImport("User32.dll", CharSet = CharSet.Unicode)]
        static extern IntPtr FindWindow(string lpszClass, string lpszWindow);
 
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
                    u.verbose = arg.Substring(9, arg.Length - 9);
                }
            }

            
            //u.className = "vim".ToLower() ;
            //u.title = "parsed";

            if (u.className != "" || u.title != "")
            {
                EnumWindows(new EnumerateWindowsCallback(Startup.EnumerateWindows), 0);
            }

            u.ExecuteCommand();
        }

        public static int EnumerateWindows(IntPtr hWnd, int lParam)
        {
            uint procId = 0;
            uint result = GetWindowThreadProcessId(hWnd, ref procId);
            StringBuilder sbClassName = new StringBuilder(256);
            StringBuilder sbWindowText = new StringBuilder(256);
            GetClassName(hWnd, sbClassName, sbClassName.Capacity);
            GetWindowText(hWnd, sbWindowText, sbWindowText.Capacity);

            bool isClassExist = false;
            bool isTitleExist = false;

            if (u.className == "")
            {
                isClassExist = true;
            }
            else if (sbClassName.ToString().ToLower().IndexOf(u.className) > -1)
            {
                isClassExist = true;
            }

            if (u.title == "")
            {
                isTitleExist = true;
            }
            else if (sbWindowText.ToString().ToLower().IndexOf(u.title) > -1)
            {
                isTitleExist = true;
            }

            if (isClassExist && isTitleExist)
            {
                if (u.hasGUI(hWnd))
                {
                    Debug.Write(hWnd);
                    Debug.WriteLine(" " + sbClassName.ToString() + " : " + sbWindowText.ToString());
                    u.MoveWindowTop(hWnd);
                    u.isRunning = true;
                }
            }
            
            return 1;
        }
    }
}
