using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms;

namespace WindowsAPIs
{
    class Utils
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("User32.Dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder s, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(UInt32 dwDesiredAccess, Int32 bInheritHandle, UInt32 dwProcessId);

        [DllImport("psapi.dll")]
        private static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, [In] [MarshalAs(UnmanagedType.U4)] int nSize);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("user32", CharSet = CharSet.Auto)]
        private static extern int GetWindowLong(IntPtr hWnd, int index);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern uint GetWindowLongPtr(IntPtr hWnd, int index);
        //private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int index);

        private const uint WS_VISIBLE = 0x10000000;
        private const uint WS_THICKFRAME = 0x00040000;
        private const uint WS_BORDER = 0x00800000;
        private const uint WS_DLGFRAME = 0x00400000;
        private const int GWL_STYLE = -16;
        public int SW_RESTORE = 9;

        public string className = "";
        public string title = "";
        public string command = "";
        public string arguments = "";
        public string verbose = "false";

        public bool isRunning = false;
        
        public bool hasGUI(IntPtr hwnd)
        {
            uint style = (uint)GetWindowStyle(hwnd);
            uint mixStyle = (WS_VISIBLE | WS_BORDER | WS_DLGFRAME);

            if ((style & mixStyle) == mixStyle)
            {
                return true;
            }

            return false;
        }

        // http://hongliang.seesaa.net/article/148698461.html のコードを利用しました
        private uint GetWindowStyle(IntPtr hWnd)
        {
            // 32bitプロセスか64bitプロセスかで呼び出すAPIが変わる
            if (IntPtr.Size == 4)
                return (uint)(GetWindowLong(hWnd, GWL_STYLE));
            else
                return GetWindowLongPtr(hWnd, GWL_STYLE);
        }

        private Boolean IsExists(string filename)
        {
            if (System.IO.File.Exists(filename))
            {
                return true;
            }

            string[] paths = System.Environment.GetEnvironmentVariable("PATH").Split(';');
            foreach (string path in paths)
            {
                if (System.IO.File.Exists(path + "\\" + filename))
                {
                    return true;
                }
            }
            return false;
        }

        public void MoveWindowTop(IntPtr hWnd)
        {
            ShowWindowAsync(hWnd, SW_RESTORE);
            SetForegroundWindow(hWnd);
        }

        public void OutputDatetime()
        {
            Debug.WriteLine(System.DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss.fff"));
        }

        public void ExecuteCommand()
        {
            //MessageBox.Show(System.Environment.GetEnvironmentVariable("PATH"));
            if (verbose == "true")
            {
                string msg = "Class='" + className + "' ";
                msg += "Title='"+ title + "' " ;
                msg += "Command='"+ command + "' " ;
                msg += "Arguments='"+ arguments + "' " ;
                msg += "Verbose='" + verbose + "'";

                MessageBox.Show(msg);
            }

            if (isRunning)
            {
                return;
            }
            if (command == "")
            {
                return;
            }
            if (IsExists(command) == false)
            {
                System.Windows.Forms.MessageBox.Show(command + " doesn't exist");
                return;
            }

            Process p = new Process();
            p.StartInfo.FileName = command;
            p.StartInfo.Arguments = arguments;
            p.Start();
        }

        private const int nChars = 1024;

        public string getActiveWindowModuleName(bool isOnlyBinary = true)
        {
            StringBuilder sb = new StringBuilder(nChars);

            IntPtr hWnd = GetForegroundWindow();
            uint processId = 0;
            GetWindowThreadProcessId(hWnd, out processId);
            IntPtr hProcess = OpenProcess(1040, 0, processId);
            GetModuleFileNameEx(hProcess, IntPtr.Zero, sb, nChars);
            CloseHandle(hProcess);

            if (isOnlyBinary)
            {
                string[] ary = sb.ToString().Split('\\');
                return ary[ary.Length-1];
            }

            return sb.ToString();
        }
    }
}
