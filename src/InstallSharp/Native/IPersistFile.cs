using System;
using System.Runtime.InteropServices;

namespace InstallSharp.Native
{
    [ComImport]
    [Guid("0000010B-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IPersistFile
    {
        [PreserveSig]
        void GetClassID(out Guid pClassID);

        /// Checks for changes since last file write
        void IsDirty();

        /// Opens the specified file and initializes the object from its contents
        void Load([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, uint dwMode);

        /// Saves the object into the specified file
        void Save([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, [MarshalAs(UnmanagedType.Bool)] bool fRemember);

        /// Notifies the object that save is completed"
        void SaveCompleted([MarshalAs(UnmanagedType.LPWStr)] string pszFileName);

        /// Gets the current name of the file associated with the object
        void GetCurFile([MarshalAs(UnmanagedType.LPWStr)] out string ppszFileName);
    }
}