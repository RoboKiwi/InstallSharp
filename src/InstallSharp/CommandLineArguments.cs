using System;
using System.Diagnostics;
using System.IO;

namespace InstallSharp
{
    public class CommandLineArguments
    {
        public Command Command { get; set; }

        public CommandLineArguments(string[] args)
        {
            Command = Command.None;

            // If we're executing as *.update.exe, that means
            // we need to update the side-by-side exe with this update
            var exeName = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
            var isUpdateExe = exeName.EndsWith(".update.exe", StringComparison.OrdinalIgnoreCase);

            if (isUpdateExe)
            {
                Command = Command.ApplyUpdate;
            }
            else
            {
                if (args == null || args.Length <= 1) return;

                var setupCmd = args[0].Trim();
                var commandArg = args[1].Trim();

                // If we haven't detected a command yet, and the base command isn't "setup" then we have nothing to do here
                if (Command == Command.None && !setupCmd.Equals("setup", StringComparison.OrdinalIgnoreCase)) return;

                if (!Enum.TryParse(commandArg, true, out Command command))
                {
                    Console.WriteLine($"Unknown command: ${commandArg}");
                    return;
                }

                Command = command;
            }

            foreach (var arg in args)
            {
                switch (arg.Trim().ToLowerInvariant().TrimStart('-','/','+'))
                {
                    case "silent":
                        Silent = true;
                        break;
                    case "launch":
                        Launch = true;
                        break;
                }
            }

            switch (Command)
            {
                case Command.None:
                    break;

                case Command.Install:
                    if (args.Length > 2)
                    {
                        Target = Path.GetFullPath(args[2]);
                    }
                    break;

                case Command.Update:
                    if (args.Length > 2)
                    {
                        Target = Path.GetFullPath(args[2]);
                    }
                    break;

                case Command.Uninstall:
                case Command.Service:
                case Command.ApplyUpdate:
                default:
                    break;
            }
        }

        /// <summary>
        /// Re-launch the program after the update / installation
        /// </summary>
        public bool Launch { get; set; }

        public string Target { get; set; }

        public bool Silent { get; set; }
    }
}
