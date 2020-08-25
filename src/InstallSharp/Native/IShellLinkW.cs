using System;
using System.Runtime.InteropServices;
using System.Text;

namespace InstallSharp.Native
{
    [ComImport()]
    [Guid("000214F9-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IShellLinkW
    {
        //[helpstring("Retrieves the path and filename of a shell link object")]
        void GetPath(
            [Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile,
            int cchMaxPath,
            ref _WIN32_FIND_DATAW pfd,
            uint fFlags);

        //[helpstring("Retrieves the list of shell link item identifiers")]
        void GetIDList(out IntPtr ppidl);

        //[helpstring("Sets the list of shell link item identifiers")]
        void SetIDList(IntPtr pidl);

        //[helpstring("Retrieves the shell link description string")]
        void GetDescription(
            [Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile,
            int cchMaxName);

        //[helpstring("Sets the shell link description string")]
        void SetDescription(
            [MarshalAs(UnmanagedType.LPWStr)] string pszName);

        //[helpstring("Retrieves the name of the shell link working directory")]
        void GetWorkingDirectory(
            [Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir,
            int cchMaxPath);

        //[helpstring("Sets the name of the shell link working directory")]
        void SetWorkingDirectory(
            [MarshalAs(UnmanagedType.LPWStr)] string pszDir);

        //[helpstring("Retrieves the shell link command-line arguments")]
        void GetArguments(
            [Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs,
            int cchMaxPath);

        //[helpstring("Sets the shell link command-line arguments")]
        void SetArguments(
            [MarshalAs(UnmanagedType.LPWStr)] string pszArgs);

        //[propget, helpstring("Retrieves or sets the shell link hot key")]
        void GetHotkey(out short pwHotkey);
        //[propput, helpstring("Retrieves or sets the shell link hot key")]
        void SetHotkey(short pwHotkey);

        //[propget, helpstring("Retrieves or sets the shell link show command")]
        void GetShowCmd(out uint piShowCmd);
        //[propput, helpstring("Retrieves or sets the shell link show command")]
        void SetShowCmd(uint piShowCmd);

        //[helpstring("Retrieves the location (path and index) of the shell link icon")]
        void GetIconLocation(
            [Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath,
            int cchIconPath,
            out int piIcon);

        //[helpstring("Sets the location (path and index) of the shell link icon")]
        void SetIconLocation(
            [MarshalAs(UnmanagedType.LPWStr)] string pszIconPath,
            int iIcon);

        //[helpstring("Sets the shell link relative path")]
        void SetRelativePath(
            [MarshalAs(UnmanagedType.LPWStr)] string pszPathRel,
            uint dwReserved);

        //[helpstring("Resolves a shell link. The system searches for the shell link object and updates the shell link path and its list of identifiers (if necessary)")]
        void Resolve(
            IntPtr hWnd,
            uint fFlags);

        //[helpstring("Sets the shell link path and filename")]
        void SetPath(
            [MarshalAs(UnmanagedType.LPWStr)] string pszFile);
    }
}