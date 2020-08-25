using System.Runtime.InteropServices;

namespace InstallSharp.Native
{
    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 0,
        CharSet = CharSet.Unicode)]
    struct _WIN32_FIND_DATAW
    {
        public uint dwFileAttributes;
        public _FILETIME ftCreationTime;
        public _FILETIME ftLastAccessTime;
        public _FILETIME ftLastWriteTime;
        public uint nFileSizeHigh;
        public uint nFileSizeLow;
        public uint dwReserved0;
        public uint dwReserved1;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] // MAX_PATH
        public string cFileName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
        public string cAlternateFileName;
    }
}