using System;
using System.Runtime.InteropServices;

namespace InstallSharp.Native
{
    internal static class Win32Api
    {
        const string kernel32ClientBaseApi = "kernel32.dll";
        const string shell32ClientBaseApi = "shell32.dll";

        /// <see href="https://docs.microsoft.com/en-nz/windows/win32/api/shlobj_core/nf-shlobj_core-shgetknownfolderpath"/>
        [DllImport(shell32ClientBaseApi, CharSet = CharSet.Unicode, SetLastError = false, BestFitMapping = false, ExactSpelling = true)]
        static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out string ppszPath);

        /// <summary>
        /// Moves an existing file or directory, including its children, with various move options.
        /// </summary>
        /// <param name="existingFileName">
        /// <p>The current name of the file or directory on the local computer.</p>
        /// <p>If <paramref name="moveFileFlags"/> specifies <see cref="MoveFileFlags.DelayUntilReboot"/>,
        /// the file cannot exist on a remote share, because delayed operations are performed
        /// before the network is available.</p>
        /// </param>
        /// <param name="newFileName">
        /// <p>The new name of the file or directory on the local computer.</p>
        /// <p>When moving a file, the destination can be on a different file system or volume.
        /// If the destination is on another drive, you must set the <see cref="MoveFileFlags.CopyAllowed"/> flag in <paramref name="moveFileFlags"/>.</p>
        /// <p>When moving a directory, the destination must be on the same drive.</p>
        /// <p>If <paramref name="moveFileFlags"/> specifies <see cref="MoveFileFlags.DelayUntilReboot"/> and <paramref name="newFileName"/> is <c>null</c>, <see cref="MoveFileEx"/> registers the <paramref name="existingFileName"/> file to be deleted when the system restarts.
        /// If <paramref name="existingFileName"/> refers to a directory, the system removes the directory at restart only if the directory is empty.</p>
        /// </param>
        /// <param name="moveFileFlags">The options flags for the file or directory move. See <see cref="MoveFileFlags"/>.</param>
        /// <returns>
        /// <p>If the function succeeds, the return value is nonzero.</p>
        /// <p>If the function fails, the return value is zero (0). To get extended error information, call <see cref="Marshal.GetLastWin32Error"/>.</p>
        /// </returns>
        [DllImport(kernel32ClientBaseApi, SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool MoveFileEx(string existingFileName, string newFileName, MoveFileFlags moveFileFlags);

        /// <summary>
        /// Flags for specifying options to <see cref="MoveFileEx"/>.
        /// </summary>
        [Flags]
        internal enum MoveFileFlags
        {
            /// <summary>
            /// <p>If a file named lpNewFileName exists, the function replaces its contents with the contents of the
            /// lpExistingFileName file, provided that security requirements regarding access control lists (ACLs) are met.
            /// For more information, see the Remarks section of this topic.</p>
            /// <p>This value cannot be used if newFileName or existingFileName names a directory.</p>
            /// </summary>
            ReplaceExisting = 0x00000001,

            /// <summary>
            /// If the file is to be moved to a different volume, the function simulates the move by using the
            /// CopyFile and DeleteFile functions.
            /// <p>This value cannot be used with <see cref="DelayUntilReboot"/>.</p>
            /// </summary>
            CopyAllowed = 0x00000002,

            /// <summary>
            /// <p>The system does not move the file until the operating system is restarted.
            /// The system moves the file immediately after AUTOCHK is executed, but before creating any paging files.
            /// Consequently, this parameter enables the function to delete paging files from previous startups.</p>
            /// <p>This value can be used only if the process is in the context of a user who belongs to the
            /// administrators group or the LocalSystem account.</p>
            /// <p>This value cannot be used with <see cref="CopyAllowed"/>.</p>
            /// </summary>
            DelayUntilReboot = 0x00000004,

            /// <summary>
            /// <p>The function does not return until the file is actually moved on the disk.</p>
            /// <p>Setting this value guarantees that a move performed as a copy and delete operation
            /// is flushed to disk before the function returns. The flush occurs at the end of the copy operation.</p>
            /// <p>This value has no effect if <see cref="DelayUntilReboot"/> is set.</p>
            /// </summary>
            WriteThrough = 0x00000008,

            /// <summary>
            /// Reserved for future use.
            /// </summary>
            CreateHardlink = 0x00000010,

            /// <summary>
            /// The function fails if the source file is a link source, but the file cannot be tracked after the move.
            /// This situation can occur if the destination is a volume formatted with the FAT file system.
            /// </summary>
            FailIfNotTrackable = 0x00000020
        }
        
        internal static string GetKnownFolderPath(string folderGuid, SpecialFolderOption option)
        {
            // System.Private.CoreLib/src/System/Environment.Win32.cs
            var folderId = new Guid(folderGuid);
            int hr = SHGetKnownFolderPath(folderId, (uint)option, IntPtr.Zero, out string path);
            return hr != 0 ? string.Empty : path;
        }

        internal enum SpecialFolderOption
        {
            None = 0,
            Create = SpecialFolderOptionValues.KF_FLAG_CREATE,
            DoNotVerify = SpecialFolderOptionValues.KF_FLAG_DONT_VERIFY,
            DefaultPath = SpecialFolderOptionValues.KF_FLAG_DEFAULT_PATH
        }

        // These values are specific to Windows and are known to SHGetFolderPath, however they are
        // also the values used in the SpecialFolderOption enum.  As such, we keep them as constants
        // with their Win32 names, but keep them here rather than in Interop.Kernel32 as they're
        // used on all platforms.
        internal static class SpecialFolderOptionValues
        {
            /// <summary>
            /// Forces the creation of the specified folder if that folder does not already exist.
            /// The security provisions predefined for that folder are applied.
            /// If the folder does not exist and cannot be created, the function returns a failure code and no path is returned.
            /// </summary>
            internal const int KF_FLAG_CREATE = 0x8000;

            /// <summary>
            /// Do not verify the folder's existence before attempting to retrieve the path or IDList.
            /// If this flag is not set, an attempt is made to verify that the folder is truly present at the path.
            /// If that verification fails due to the folder being absent or inaccessible, the function returns a failure code and no path is returned.
            /// </summary>
            internal const int KF_FLAG_DONT_VERIFY = 0x4000;
            
            /// <summary>
            /// Gets the default path for a known folder.
            /// If this flag is not set, the function retrieves the current—and possibly redirected—path of the folder.
            /// The execution of this flag includes a verification of the folder's existence unless <see cref="KF_FLAG_DONT_VERIFY"/>  is set.
            /// </summary>
            internal const int KF_FLAG_DEFAULT_PATH = 0x00000400;
        }

        /// <see href="https://docs.microsoft.com/en-us/windows/win32/shell/knownfolderid" />
        internal static class KnownFolders
        {
            /// <summary>
            /// (CSIDL_ADMINTOOLS) Per user Administrative Tools
            /// "%APPDATA%\Microsoft\Windows\Start Menu\Programs\Administrative Tools"
            /// </summary>
            internal const string AdminTools = "{724EF170-A42D-4FEF-9F26-B60E846FBA4F}";

            /// <summary>
            /// (CSIDL_CDBURN_AREA) Temporary Burn folder
            /// "%LOCALAPPDATA%\Microsoft\Windows\Burn\Burn"
            /// </summary>
            internal const string CDBurning = "{9E52AB10-F80D-49DF-ACB8-4330F5687855}";

            /// <summary>
            /// (CSIDL_COMMON_ADMINTOOLS) Common Administrative Tools
            /// "%ALLUSERSPROFILE%\Microsoft\Windows\Start Menu\Programs\Administrative Tools"
            /// </summary>
            internal const string CommonAdminTools = "{D0384E7D-BAC3-4797-8F14-CBA229B392B5}";

            /// <summary>
            /// (CSIDL_COMMON_OEM_LINKS) OEM Links folder
            /// "%ALLUSERSPROFILE%\OEM Links"
            /// </summary>
            internal const string CommonOEMLinks = "{C1BAE2D0-10DF-4334-BEDD-7AA20B227A9D}";

            /// <summary>
            /// (CSIDL_COMMON_PROGRAMS) Common Programs folder
            /// "%ALLUSERSPROFILE%\Microsoft\Windows\Start Menu\Programs"
            /// </summary>
            internal const string CommonPrograms = "{0139D44E-6AFE-49F2-8690-3DAFCAE6FFB8}";

            /// <summary>
            /// (CSIDL_COMMON_STARTMENU) Common Start Menu folder
            /// "%ALLUSERSPROFILE%\Microsoft\Windows\Start Menu"
            /// </summary>
            internal const string CommonStartMenu = "{A4115719-D62E-491D-AA7C-E74B8BE3B067}";

            /// <summary>
            /// (CSIDL_COMMON_STARTUP, CSIDL_COMMON_ALTSTARTUP) Common Startup folder
            /// "%ALLUSERSPROFILE%\Microsoft\Windows\Start Menu\Programs\StartUp"
            /// </summary>
            internal const string CommonStartup = "{82A5EA35-D9CD-47C5-9629-E15D2F714E6E}";

            /// <summary>
            /// (CSIDL_COMMON_TEMPLATES) Common Templates folder
            /// "%ALLUSERSPROFILE%\Microsoft\Windows\Templates"
            /// </summary>
            internal const string CommonTemplates = "{B94237E7-57AC-4347-9151-B08C6C32D1F7}";

            /// <summary>
            /// (CSIDL_DRIVES) Computer virtual folder
            /// </summary>
            internal const string ComputerFolder = "{0AC0837C-BBF8-452A-850D-79D08E667CA7}";

            /// <summary>
            /// (CSIDL_CONNECTIONS) Network Connections virtual folder
            /// </summary>
            internal const string ConnectionsFolder = "{6F0CD92B-2E97-45D1-88FF-B0D186B8DEDD}";

            /// <summary>
            /// (CSIDL_CONTROLS) Control Panel virtual folder
            /// </summary>
            internal const string ControlPanelFolder = "{82A74AEB-AEB4-465C-A014-D097EE346D63}";

            /// <summary>
            /// (CSIDL_COOKIES) Cookies folder
            /// "%APPDATA%\Microsoft\Windows\Cookies"
            /// </summary>
            internal const string Cookies = "{2B0F765D-C0E9-4171-908E-08A611B84FF6}";

            /// <summary>
            /// (CSIDL_DESKTOP, CSIDL_DESKTOPDIRECTORY) Desktop folder
            /// "%USERPROFILE%\Desktop"
            /// </summary>
            internal const string Desktop = "{B4BFCC3A-DB2C-424C-B029-7FE99A87C641}";

            /// <summary>
            /// (CSIDL_MYDOCUMENTS, CSIDL_PERSONAL) Documents (My Documents) folder
            /// "%USERPROFILE%\Documents"
            /// </summary>
            internal const string Documents = "{FDD39AD0-238F-46AF-ADB4-6C85480369C7}";

            /// <summary>
            /// (CISDL_DOWNLOADS, CSIDL_PERSONAL) Downloads folder
            /// "%USERPROFILE%\Downloads"
            /// </summary>
            internal const string Downloads = "{374DE290-123F-4565-9164-39C4925E467B}";

            /// <summary>
            /// (CSIDL_FAVORITES, CSIDL_COMMON_FAVORITES) Favorites folder
            /// "%USERPROFILE%\Favorites"
            /// </summary>
            internal const string Favorites = "{1777F761-68AD-4D8A-87BD-30B759FA33DD}";

            /// <summary>
            /// (CSIDL_FONTS) Fonts folder
            /// "%windir%\Fonts"
            /// </summary>
            internal const string Fonts = "{FD228CB7-AE11-4AE3-864C-16F3910AB8FE}";

            /// <summary>
            /// (CSIDL_HISTORY) History folder
            /// "%LOCALAPPDATA%\Microsoft\Windows\History"
            /// </summary>
            internal const string History = "{D9DC8A3B-B784-432E-A781-5A1130A75963}";

            /// <summary>
            /// (CSIDL_INTERNET_CACHE) Temporary Internet Files folder
            /// "%LOCALAPPDATA%\Microsoft\Windows\Temporary Internet Files"
            /// </summary>
            internal const string InternetCache = "{352481E8-33BE-4251-BA85-6007CAEDCF9D}";

            /// <summary>
            /// (CSIDL_INTERNET) The Internet virtual folder
            /// </summary>
            internal const string InternetFolder = "{4D9F7874-4E0C-4904-967B-40B0D20C3E4B}";

            /// <summary>
            /// (CSIDL_LOCAL_APPDATA) Local folder
            /// "%LOCALAPPDATA%" ("%USERPROFILE%\AppData\Local")
            /// </summary>
            internal const string LocalAppData = "{F1B32785-6FBA-4FCF-9D55-7B8E7F157091}";

            /// <summary>
            /// (CSIDL_RESOURCES_LOCALIZED) Fixed localized resources folder
            /// "%windir%\resources\0409" (per active codepage)
            /// </summary>
            internal const string LocalizedResourcesDir = "{2A00375E-224C-49DE-B8D1-440DF7EF3DDC}";

            /// <summary>
            /// (CSIDL_MYMUSIC) Music folder
            /// "%USERPROFILE%\Music"
            /// </summary>
            internal const string Music = "{4BD8D571-6D19-48D3-BE97-422220080E43}";

            /// <summary>
            /// (CSIDL_NETHOOD) Network shortcuts folder "%APPDATA%\Microsoft\Windows\Network Shortcuts"
            /// </summary>
            internal const string NetHood = "{C5ABBF53-E17F-4121-8900-86626FC2C973}";

            /// <summary>
            /// (CSIDL_NETWORK, CSIDL_COMPUTERSNEARME) Network virtual folder
            /// </summary>
            internal const string NetworkFolder = "{D20BEEC4-5CA8-4905-AE3B-BF251EA09B53}";

            /// <summary>
            /// (CSIDL_MYPICTURES) Pictures folder "%USERPROFILE%\Pictures"
            /// </summary>
            internal const string Pictures = "{33E28130-4E1E-4676-835A-98395C3BC3BB}";

            /// <summary>
            /// (CSIDL_PRINTERS) Printers virtual folder
            /// </summary>
            internal const string PrintersFolder = "{76FC4E2D-D6AD-4519-A663-37BD56068185}";

            /// <summary>
            /// (CSIDL_PRINTHOOD) Printer Shortcuts folder
            /// "%APPDATA%\Microsoft\Windows\Printer Shortcuts"
            /// </summary>
            internal const string PrintHood = "{9274BD8D-CFD1-41C3-B35E-B13F55A758F4}";

            /// <summary>
            /// (CSIDL_PROFILE) The root users profile folder "%USERPROFILE%"
            /// ("%SystemDrive%\Users\%USERNAME%")
            /// </summary>
            internal const string Profile = "{5E6C858F-0E22-4760-9AFE-EA3317B67173}";

            /// <summary>
            /// (CSIDL_COMMON_APPDATA) ProgramData folder
            /// "%ALLUSERSPROFILE%" ("%ProgramData%", "%SystemDrive%\ProgramData")
            /// </summary>
            internal const string ProgramData = "{62AB5D82-FDC1-4DC3-A9DD-070D1D495D97}";

            /// <summary>
            /// (CSIDL_PROGRAM_FILES) Program Files folder for the current process architecture
            /// "%ProgramFiles%" ("%SystemDrive%\Program Files")
            /// </summary>
            internal const string ProgramFiles = "{905e63b6-c1bf-494e-b29c-65b732d3d21a}";

            /// <summary>
            /// (CSIDL_PROGRAM_FILESX86) 32 bit Program Files folder (available to both 32/64 bit processes)
            /// </summary>
            internal const string ProgramFilesX86 = "{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}";

            /// <summary>
            /// (CSIDL_PROGRAM_FILES_COMMON) Common Program Files folder for the current process architecture
            /// "%ProgramFiles%\Common Files"
            /// </summary>
            internal const string ProgramFilesCommon = "{F7F1ED05-9F6D-47A2-AAAE-29D317C6F066}";

            /// <summary>
            /// (CSIDL_PROGRAM_FILES_COMMONX86) Common 32 bit Program Files folder (available to both 32/64 bit processes)
            /// </summary>
            internal const string ProgramFilesCommonX86 = "{DE974D24-D9C6-4D3E-BF91-F4455120B917}";

            /// <summary>
            /// (CSIDL_PROGRAMS) Start menu Programs folder
            /// "%APPDATA%\Microsoft\Windows\Start Menu\Programs"
            /// </summary>
            internal const string Programs = "{A77F5D77-2E2B-44C3-A6A2-ABA601054A51}";

            /// <summary>
            /// (CSIDL_COMMON_DESKTOPDIRECTORY) Public Desktop folder
            /// "%PUBLIC%\Desktop"
            /// </summary>
            internal const string PublicDesktop = "{C4AA340D-F20F-4863-AFEF-F87EF2E6BA25}";

            /// <summary>
            /// (CSIDL_COMMON_DOCUMENTS) Public Documents folder
            /// "%PUBLIC%\Documents"
            /// </summary>
            internal const string PublicDocuments = "{ED4824AF-DCE4-45A8-81E2-FC7965083634}";

            /// <summary>
            /// (CSIDL_COMMON_MUSIC) Public Music folder
            /// "%PUBLIC%\Music"
            /// </summary>
            internal const string PublicMusic = "{3214FAB5-9757-4298-BB61-92A9DEAA44FF}";

            /// <summary>
            /// (CSIDL_COMMON_PICTURES) Public Pictures folder
            /// "%PUBLIC%\Pictures"
            /// </summary>
            internal const string PublicPictures = "{B6EBFB86-6907-413C-9AF7-4FC2ABF07CC5}";

            /// <summary>
            /// (CSIDL_COMMON_VIDEO) Public Videos folder
            /// "%PUBLIC%\Videos"
            /// </summary>
            internal const string PublicVideos = "{2400183A-6185-49FB-A2D8-4A392A602BA3}";

            /// <summary>
            /// (CSIDL_RECENT) Recent Items folder
            /// "%APPDATA%\Microsoft\Windows\Recent"
            /// </summary>
            internal const string Recent = "{AE50C081-EBD2-438A-8655-8A092E34987A}";

            /// <summary>
            /// (CSIDL_BITBUCKET) Recycle Bin virtual folder
            /// </summary>
            internal const string RecycleBinFolder = "{B7534046-3ECB-4C18-BE4E-64CD4CB7D6AC}";

            /// <summary>
            /// (CSIDL_RESOURCES) Resources fixed folder
            /// "%windir%\Resources"
            /// </summary>
            internal const string ResourceDir = "{8AD10C31-2ADB-4296-A8F7-E4701232C972}";

            /// <summary>
            /// (CSIDL_APPDATA) Roaming user application data folder
            /// "%APPDATA%" ("%USERPROFILE%\AppData\Roaming")
            /// </summary>
            internal const string RoamingAppData = "{3EB685DB-65F9-4CF6-A03A-E3EF65729F3D}";

            /// <summary>
            /// (CSIDL_SENDTO) SendTo folder
            /// "%APPDATA%\Microsoft\Windows\SendTo"
            /// </summary>
            internal const string SendTo = "{8983036C-27C0-404B-8F08-102D10DCFD74}";

            /// <summary>
            /// (CSIDL_STARTMENU) Start Menu folder
            /// "%APPDATA%\Microsoft\Windows\Start Menu"
            /// </summary>
            internal const string StartMenu = "{625B53C3-AB48-4EC1-BA1F-A1EF4146FC19}";

            /// <summary>
            /// (CSIDL_STARTUP, CSIDL_ALTSTARTUP) Startup folder
            /// "%APPDATA%\Microsoft\Windows\Start Menu\Programs\StartUp"
            /// </summary>
            internal const string Startup = "{B97D20BB-F46A-4C97-BA10-5E3608430854}";

            /// <summary>
            /// (CSIDL_SYSTEM) System32 folder
            /// "%windir%\system32"
            /// </summary>
            internal const string System = "{1AC14E77-02E7-4E5D-B744-2EB1AE5198B7}";

            /// <summary>
            /// (CSIDL_SYSTEMX86) X86 System32 folder
            /// "%windir%\system32" or "%windir%\syswow64"
            /// </summary>
            internal const string SystemX86 = "{D65231B0-B2F1-4857-A4CE-A8E7C6EA7D27}";

            /// <summary>
            /// (CSIDL_TEMPLATES) Templates folder
            /// "%APPDATA%\Microsoft\Windows\Templates"
            /// </summary>
            internal const string Templates = "{A63293E8-664E-48DB-A079-DF759E0509F7}";

            /// <summary>
            /// (CSIDL_MYVIDEO) Videos folder
            /// "%USERPROFILE%\Videos"
            /// </summary>
            internal const string Videos = "{18989B1D-99B5-455B-841C-AB7C74E4DDFC}";

            /// <summary>
            /// (CSIDL_WINDOWS) Windows folder "%windir%"
            /// </summary>
            internal const string Windows = "{F38BF404-1D43-42F2-9305-67DE0B28FC23}";
        }
    }
}