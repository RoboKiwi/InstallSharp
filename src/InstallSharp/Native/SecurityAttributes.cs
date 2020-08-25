using System;
using System.Runtime.InteropServices;

namespace InstallSharp.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public class SecurityAttributes
    {
        public int nLength = 12;
        public SafeLocalMemHandle lpSecurityDescriptor = new SafeLocalMemHandle(IntPtr.Zero, false);
        public bool bInheritHandle;
    }
}