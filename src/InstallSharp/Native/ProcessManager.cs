using System;
using System.Runtime.InteropServices;
using System.Text;

namespace InstallSharp.Native
{
    public class ProcessManager
    {
        [DllImport("Kernel32", CharSet = CharSet.Auto, SetLastError = true, BestFitMapping = false)]
        public static extern bool CreateProcess(
            [MarshalAs(UnmanagedType.LPTStr)]string applicationName,
            StringBuilder commandLine, 
            SecurityAttributes processAttributes,
            SecurityAttributes threadAttributes, 
            bool inheritHandles, 
            int creationFlags,
            IntPtr environment, 
            [MarshalAs(UnmanagedType.LPTStr)]string currentDirectory,
            StartupInfo startupInfo, 
            ProcessInformation processInformation
        );
    }
}