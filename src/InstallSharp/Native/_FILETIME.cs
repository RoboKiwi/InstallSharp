using System.Runtime.InteropServices;

namespace InstallSharp.Native
{
    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 0)]
    struct _FILETIME
    {
        public uint dwLowDateTime;
        public uint dwHighDateTime;
    }
}