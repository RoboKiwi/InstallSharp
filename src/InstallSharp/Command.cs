namespace InstallSharp
{
    public enum Command
    {
        /// <summary>
        /// Normal program execution
        /// </summary>
        None = 0,

        /// <summary>
        /// Registers the program shortcuts and with Add / Remove Programs (ARP), and optionally copies to a specified location
        /// </summary>
        Install,

        /// <summary>
        /// Downloads and applies update if available
        /// </summary>
        Update,

        /// <summary>
        /// Applies downloaded update
        /// </summary>
        ApplyUpdate,

        /// <summary>
        /// Uninstalls the program
        /// </summary>
        Uninstall,

        /// <summary>
        /// Manage Windows service
        /// </summary>
        Service,

        /// <summary>
        /// Cleans up temporary downloaded exe
        /// </summary>
        Cleanup
    }
}