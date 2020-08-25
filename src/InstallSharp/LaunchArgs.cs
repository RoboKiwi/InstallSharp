namespace InstallSharp
{
    public class LaunchArgs
    {
        public LaunchArgs(string target)
        {
            Target = target;
        }

        public LaunchArgs(string target, string args) : this(target)
        {
            Args = args;
        }

        public LaunchArgs()
        {
        }

        public string Target { get; set; }
        public string Args { get; set; }
    }
}