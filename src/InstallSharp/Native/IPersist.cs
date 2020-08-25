using System;
using System.Runtime.InteropServices;

namespace InstallSharp.Native
{
    [ComImport()]
    [Guid("0000010C-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IPersist
    {
        [PreserveSig]
        /// Returns the class identifier for the component object
        void GetClassID(out Guid pClassID);
    }
}