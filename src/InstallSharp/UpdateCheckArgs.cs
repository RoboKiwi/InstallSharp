using System.Collections.Generic;

namespace InstallSharp
{
    public class UpdateCheckArgs
    {
        public UpdateCheckArgs()
        {
            IgnoreTags = new List<string>();
        }

        /// <summary>
        /// The full URI to the GitHub releases API
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// List of tags to ignore releases for.
        /// </summary>
        public IList<string> IgnoreTags { get; set; }

        /// <summary>
        /// Allow updating to pre releases. Default of <c>false</c>.
        /// </summary>
        public bool AllowPreRelease { get; set; }

        /// <summary>
        /// The name of the asset to download and install. Defaults to the current exe name.
        /// </summary>
        public string AssetName { get; set; }
    }
}