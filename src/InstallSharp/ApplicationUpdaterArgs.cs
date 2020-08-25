using System;
using System.Diagnostics;
using System.Net.Http;

namespace InstallSharp
{
    /// <summary>
    /// Arguments for creating an <see cref="ApplicationUpdater"/>. Only the <see cref="UpdateUrl"/> must be specified,
    /// while the other values can be implied from your application exe. You should configure a Guid for your executable
    /// to make the installer more robust; see <see cref="Guid"/>.
    /// </summary>
    public class ApplicationUpdaterArgs
    {
        /// <summary>
        /// Creates a new instance of <see cref="ApplicationUpdaterArgs"/> with defaults.
        /// </summary>
        public ApplicationUpdaterArgs() { }

        /// <summary>
        /// Creates a new instance of <see cref="ApplicationUpdaterArgs"/> with a specified <see cref="UpdateUrl"/>.
        /// </summary>
        /// <param name="updateUri"></param>
        public ApplicationUpdaterArgs(string updateUri)
        {
            UpdateUrl = updateUri;
        }

        /// <summary>
        /// The unique GUID to identify the application, used for Add/Remove Programs registry entries.
        /// Optional but highly recommended, as it allows you to rename your application without breaking old installs.
        /// Add a <see cref="System.Runtime.InteropServices.GuidAttribute"/> to your AssemblyInfo, and don't change it.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// The URL where updates are found. Currently only GitHub releases are supported.
        /// e.g. https://api.github.com/repos/RoboKiwi/InstallSharp/releases
        /// </summary>
        public string UpdateUrl { get; set; }

        /// <summary>
        /// The product name. Defaults to the product name in the current executable's <see cref="FileVersionInfo.ProductName"/>.
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// Company name. Defaults to the company name found in current executable's <see cref="FileVersionInfo.CompanyName"/>.
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// The name of the application executable. Defaults to the current executable's <see cref="FileVersionInfo.FileName"/>.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// The asset name to find when checking for new releases from <see cref="UpdateUrl"/>. Defaults to <see cref="FileName"/>.
        /// </summary>
        public string AssetName { get; set; }

        /// <summary>
        /// An optional progress provider to allow the <see cref="ApplicationUpdater"/> to report back
        /// progress on setup operations, such as the download of a new update.
        /// <p>This allows you to show status or progress bars in your user interface.</p>
        /// </summary>
        public IProgress<ProgressModel> Progress { get; set; }

        /// <summary>
        /// Allows you to provide your own <see cref="HttpClient"/> for the <see cref="ApplicationUpdater"/>
        /// to use when talking to the update API or downloading files. Defaults to <c>null</c> (recommended) to
        /// use the default implementation.
        /// </summary>
        public HttpClient HttpClient { get; set; }

        /// <summary>
        /// Short name for the program, defaulting to <see cref="FileName"/> without the file extension. Used to
        /// determine default installation folder, or installation registry entry if <see cref="Guid"/> is not configured.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Default installation path when not being treated as a portable app. Defaults to
        /// <c>"%LocalAppData%\Programs\%Name%"</c> where <c>%Name%</c> is the short application name specified by <see cref="Name"/>.
        /// </summary>
        public string InstallPath { get; set; }

        /// <summary>
        /// The full path and name of the executable
        /// </summary>
        public string FullFileName { get; set; }

        /// <summary>
        /// The current version, defaulting to <see cref="FileVersionInfo.FileVersion"/>.
        /// </summary>
        public string Version { get; set; }
    }
}