#region Using directives

using System;
using System.Runtime.InteropServices;
using System.Text;

#endregion

namespace Master.CompactFramework.Sync
{
    /// <summary>
    /// Summary description for Shell.
    /// </summary>
    public class Shell
    {
        #region [ PInvoke defintions ]

        private class SHELLEXECUTEEX
        {
            public UInt32 cbSize;
            public UInt32 fMask;
            public IntPtr hwnd;
            public IntPtr lpVerb;
            public IntPtr lpFile;
            public IntPtr lpParameters;
            public IntPtr lpDirectory;
            public int nShow;
            public IntPtr hInstApp;

            // Optional members 
            public IntPtr lpIDList;
            public IntPtr lpClass;
            public IntPtr hkeyClass;
            public UInt32 dwHotKey;
            public IntPtr hIcon;
            public IntPtr hProcess;
        }

        [DllImport("coredll")]
        extern static int ShellExecuteEx(SHELLEXECUTEEX ex);

        [DllImport("coredll")]
        extern static IntPtr LocalAlloc(int flags, int size);

        [DllImport("coredll")]
        extern static void LocalFree(IntPtr ptr);

        [DllImport("coredll")]
        extern static int WaitForSingleObject(IntPtr hHandle, int dwMilliseconds);

        //[DllImport("coredll")]
        //extern static bool CloseHandle(IntPtr hObject);

        //[DllImport("coredll")]
        //extern static int TerminateProcess(IntPtr processIdOrHandle, IntPtr exitCode);

        #endregion

        public static IntPtr ExecuteFile(string file)
        {
            int nSize = file.Length * 2 + 2;
            IntPtr pData = LocalAlloc(0x40, nSize);
            Marshal.Copy(Encoding.Unicode.GetBytes(file), 0, pData, nSize - 2);
            SHELLEXECUTEEX see = new SHELLEXECUTEEX();
            see.cbSize = 60;
            see.dwHotKey = 0;
            see.fMask = 0x00000040;
            see.hIcon = IntPtr.Zero;
            see.hInstApp = IntPtr.Zero;
            see.hProcess = IntPtr.Zero;
            see.lpClass = IntPtr.Zero;
            see.lpDirectory = IntPtr.Zero;
            see.lpIDList = IntPtr.Zero;
            see.lpParameters = IntPtr.Zero;
            see.lpVerb = IntPtr.Zero;
            see.nShow = 0;
            see.lpFile = pData;

            ShellExecuteEx(see);
            LocalFree(pData);

            return see.hProcess;
        }

        public static void WaitForSingleObject(IntPtr hHandle)
        {
            // [ aguarda infinitamente ]
            WaitForSingleObject(hHandle, -1);
        }
    }
}