using System;
using System.Runtime.InteropServices;

namespace InstallSharp.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public class ProcessInformation
    {
        public IntPtr hProcess = IntPtr.Zero;
        public IntPtr hThread = IntPtr.Zero;
        public int dwProcessId;
        public int dwThreadId;
    }
}