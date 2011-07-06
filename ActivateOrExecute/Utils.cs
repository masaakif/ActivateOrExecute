using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms;

namespace WindowsAPIs
{
    class ModuleInfo
    {
        public string moduleName = "";
        public string fullPath = "";
    }

    class Utils
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("User32.Dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder s, int nMaxCount);

        [DllImport("User32.Dll", CharSet = CharSet.Unicode)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder s, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "GetWindow",SetLastError = true)]
        public static extern IntPtr GetNextWindow(IntPtr hwnd, [MarshalAs(UnmanagedType.U4)] int wFlag);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, ref uint lpdwProcessId);

        // When you don't want the ProcessId, use this overload and pass IntPtr.Zero for the second parameter
        [DllImport("user32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(UInt32 dwDesiredAccess, Int32 bInheritHandle, UInt32 dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle(IntPtr hObject);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern uint GetTopWindow(IntPtr hWnd);

        /* Enum Windows functions */
        private delegate int EnumerateWindowsCallback(IntPtr hWnd, int lParam);

        [DllImport("user32", EntryPoint = "EnumWindows")]
        private static extern int EnumWindows(EnumerateWindowsCallback lpEnumFunc, int lParam);

        public delegate int CallBackPtr(Utils u2, IntPtr hWnd, StringBuilder sbClassName, StringBuilder sbWindowText, ref object o);

        [DllImport("user32.dll")]
        private static extern uint GetGuiResources(IntPtr hObject, uint uiFlags);

        [DllImport("psapi.dll")]
        private static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName
            , [In] [MarshalAs(UnmanagedType.U4)] int nSize);

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
        private const int GWL_HWNDPARENT = -8;

        private const uint GW_HWNDNEXT = 0x2;
        private const uint GW_HWNDPREV = 0x3;

        public int SW_RESTORE = 9;

        private const UInt32 PROCESS_QUERY_INFORMATION = 0x0400;
        private const uint GR_GDIOBJECTS = 0;
        private const uint GR_USEROBJECTS = 1;

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

        public ModuleInfo getActiveWindowModuleName(bool isOnlyBinary = true)
        {
            StringBuilder sb = new StringBuilder(nChars);
            ModuleInfo mi = new ModuleInfo();

            IntPtr hWnd = GetForegroundWindow();
            uint processId = 0;
            GetWindowThreadProcessId(hWnd, ref processId);
            IntPtr hProcess = OpenProcess(1040, 0, processId);
            GetModuleFileNameEx(hProcess, IntPtr.Zero, sb, nChars);
            CloseHandle(hProcess);

            mi.fullPath = sb.ToString();
            
            if (isOnlyBinary)
            {
                string[] ary = mi.fullPath.Split('\\');
                
                mi.moduleName = ary[ary.Length-1];
                return mi;
            }

            mi.moduleName = mi.fullPath;
            return mi;
        }

        public uint GetWindowHandle(uint targetPID)
        {
            uint hWnd = GetTopWindow((IntPtr)null);
            while (hWnd != 0)
            {
                if (!IsWindowVisible((IntPtr)hWnd))
                {
                    hWnd = (uint)(GetNextWindow((IntPtr)hWnd, (int)GW_HWNDNEXT));
                    continue;
                }
                uint processID = 0;
                processID = GetWindowThreadProcessId((IntPtr)hWnd, ref processID);
                if (targetPID == processID)
                {
                    return hWnd;
                }
                hWnd = (uint)(GetNextWindow((IntPtr)hWnd, (int)GW_HWNDNEXT));
            }

            return 0;            
        }

        public uint GetGDIObjects(UInt32 dwProcessId)
        {
            IntPtr hProcess = OpenProcess(PROCESS_QUERY_INFORMATION, -1, dwProcessId);
            if (hProcess != null)
            {
                uint cnt = GetGuiResources(hProcess, GR_GDIOBJECTS);
                CloseHandle(hProcess);
                return cnt;
            }
            return 0;
        }

        public uint GetUserObjects(UInt32 dwProcessId)
        {
            IntPtr hProcess = OpenProcess(PROCESS_QUERY_INFORMATION, -1, dwProcessId);
            if (hProcess != null)
            {
                uint cnt = GetGuiResources(hProcess, GR_USEROBJECTS);
                CloseHandle(hProcess);
                return cnt;
            }
            return 0;
        }

        private static Utils u;
        private static CallBackPtr cb;

        public bool FindThenExecute(CallBackPtr callback)
        {
            u = this;
            cb = callback;

            EnumWindows(Utils.EnumCallBack, 0);
            return true;
        }


        public static int EnumCallBack(IntPtr hWnd, int lParam)
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
                object o = new object();
                cb(u, hWnd, sbClassName, sbWindowText, ref o);
            }

            return 1;
        }        
    }
}
