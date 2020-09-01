namespace InstallSharp
{
    /// <summary>
    /// Contains detailed result of a setup operation such as an update check, or update download
    /// </summary>
    public class SetupResult
    {
        /// <summary>
        /// Flag if the setup operation is in an error (or <see cref="SetupError.None"/> if success)
        /// </summary>
        public SetupError Error { get; set; }

        /// <summary>
        /// The error code
        /// </summary>
        public int ErrorCode { get; set; }

        /// <summary>
        /// The error details
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Does the application need to be restarted so the update can be applied
        /// </summary>
        public bool IsPendingRestart { get; set; }

        /// <summary>
        /// The current application's version
        /// </summary>
        public SemanticVersion CurrentVersion { get; set; }

        /// <summary>
        /// The version of the update
        /// </summary>
        public SemanticVersion NewVersion { get; set; }

        /// <summary>
        /// Is the update a pre release
        /// </summary>
        public bool IsPreRelease { get; set; }

        /// <summary>
        /// Has the update been downloaded but needs to be applied
        /// </summary>
        public bool IsDownloaded { get; set; }

        /// <summary>
        /// Did the setup operation fail
        /// </summary>
        public bool IsError => Error != SetupError.Unknown;

        /// <summary>
        /// Did the setup operation succeed
        /// </summary>
        public bool IsSuccess => Error == SetupError.Unknown;
    }
}