using System;
using System.Diagnostics;
using System.IO;

namespace InstallSharp
{
    public sealed class CommandLineArguments
    {
        public Command Command { get; set; }

        public CommandLineArguments()
        {
        }

        /// <summary>
        /// Re-launch the program after the update / installation
        /// </summary>
        public bool Launch { get; set; }

        /// <summary>
        /// The process needs to be elevated (UAC)
        /// </summary>
        public bool Elevate { get; set; }

        public string Target { get; set; }

        public bool Silent { get; set; }

        internal string[] Args { get; set; }

        internal string[] Flags { get; set; }
    }
}
