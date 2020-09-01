using System;

namespace InstallSharp
{
    /// <summary>
    /// Specifies the additional application setup parameters such as the update URL
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class InstallSharpAttribute : Attribute
    {
        /// <summary>
        /// The URL where releases are kept
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// GUID for the application, used to register in Add/Remove Programs.
        /// </summary>
        public string Guid { get; set; }

        // public InstallSharpAttribute(string url)
        // {
        //     Url = url;
        // }
        //
        // public InstallSharpAttribute(Guid guid)
        // {
        //     Guid = guid;
        // }
    }
}
