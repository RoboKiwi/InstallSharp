using System;
using System.IO;
using System.Linq;

namespace InstallSharp
{
    public class CommandLineArgumentsFactory
    {
        static readonly char[] flagChars = {'+', '-', '/'};
        static readonly string[] launchArgs = {"launch", "start", "run"};
        static readonly string[] silentArgs = {"silent", "quiet", "q"};
        static readonly string[] adminArgs = {"admin", "elevate", "uac"};
        static readonly string[] allFlags = silentArgs.Concat(launchArgs).Concat(adminArgs).ToArray();

        public static CommandLineArguments Parse(ApplicationUpdaterConfig config)
        {
            var cmd = new CommandLineArguments();

            // Default to no setup command unless we can find one
            cmd.Command = Command.None;

            var commandLine = config.CommandLine ?? Environment.CommandLine;

            // Split the command line up into strings we can parse

            // Args are positional, and exclude any flags (args that start with '/', '+' or '-')
            cmd.Args = commandLine.Split(" ", StringSplitOptions.RemoveEmptyEntries).Where(x => !IsFlag(x) ).Select(x => x.Trim()).ToArray();
            
            // Collect the flags, removing the leading flag characters, then trimming and making lower case for later comparison
            cmd.Flags = commandLine.Split(" ", StringSplitOptions.RemoveEmptyEntries).Where( IsFlag )
                .Select(x => x.TrimStart(flagChars).Trim().ToLowerInvariant())
                .ToArray();

            // If we're executing as *.update.exe, then applying a side-by-side update is inferred
            var isUpdateExe = config.FileName?.EndsWith(config.UpdateSuffix, StringComparison.OrdinalIgnoreCase);
            if (isUpdateExe == true)
            {
                cmd.Command = Command.ApplyUpdate;
            }
            else
            {
                // If there is no first argument, or it isn't "setup", then this isn't a setup commandline, so we do nothing.
                if (cmd.Args.Length == 0) return cmd;
                if (!cmd.Args[0].Equals(config.CommandLineArgument ?? ApplicationUpdaterConfig.DefaultSetupArg, StringComparison.OrdinalIgnoreCase)) return cmd;

                if (cmd.Args.Length == 1)
                {
                    // If the only command was "setup" then we can infer "install" as the command
                    cmd.Command = Command.Install;
                }
                else
                {
                    // Parse the command
                    if (!Enum.TryParse(cmd.Args[1].Trim(), true, out Command command))
                    {
                        Console.WriteLine($"Unknown command: ${cmd.Args[1].Trim()}");
                        return cmd;
                    }

                    cmd.Command = command;
                }
            }

            // Parse optional flag arguments that can exist on any commands
            foreach (var arg in cmd.Flags)
            {
                if (silentArgs.Any(x => x == arg)) cmd.Silent = true;
                else if (launchArgs.Any(x => x == arg)) cmd.Launch = true;
                else if (adminArgs.Any(x => x == arg)) cmd.Elevate = true;
            }

            // Parse optional positional arguments for specific commands
            switch (cmd.Command)
            {
                case Command.Install:
                case Command.Update:
                    if (cmd.Args.Length > 2)
                    {
                        cmd.Target = Path.GetFullPath(cmd.Args[2]);
                    }
                    break;
            }

            return cmd;
        }

        internal static bool IsFlag(string arg)
        {
            return !string.IsNullOrWhiteSpace(arg) && flagChars.Any(x => x == arg[0]);
        }
    }
}