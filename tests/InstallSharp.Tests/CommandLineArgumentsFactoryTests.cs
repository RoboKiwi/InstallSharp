using Xunit;

namespace InstallSharp.Tests
{
    public class CommandLineArgumentsFactoryTests
    {
        [Fact]
        public void Flags()
        {
            // Launch
            Assert.False(CommandLineArgumentsFactory.Parse(new ApplicationUpdaterConfig{CommandLine = "setup install"}).Launch );
            Assert.True(CommandLineArgumentsFactory.Parse(new ApplicationUpdaterConfig{CommandLine = "setup install /launch"}).Launch );
            Assert.True(CommandLineArgumentsFactory.Parse(new ApplicationUpdaterConfig{CommandLine = "setup install /start"}).Launch );

            // Silent
            Assert.False(CommandLineArgumentsFactory.Parse(new ApplicationUpdaterConfig{CommandLine = "setup install"}).Silent );
            Assert.True(CommandLineArgumentsFactory.Parse(new ApplicationUpdaterConfig{CommandLine = "setup install /silent"}).Silent );
            Assert.True(CommandLineArgumentsFactory.Parse(new ApplicationUpdaterConfig{CommandLine = "setup install /quiet"}).Silent );

            // Elevation
            Assert.False(CommandLineArgumentsFactory.Parse(new ApplicationUpdaterConfig { CommandLine = "setup install" }).Elevate);
            Assert.True(CommandLineArgumentsFactory.Parse(new ApplicationUpdaterConfig { CommandLine = "setup install /uac" }).Elevate);
            Assert.True(CommandLineArgumentsFactory.Parse(new ApplicationUpdaterConfig { CommandLine = "setup install /admin" }).Elevate);
            Assert.True(CommandLineArgumentsFactory.Parse(new ApplicationUpdaterConfig { CommandLine = "setup install /elevate" }).Elevate);
        }

        [Fact]
        public void IsFlag()
        {
            Assert.True( CommandLineArgumentsFactory.IsFlag("/silent"));
            Assert.True( CommandLineArgumentsFactory.IsFlag("/quiet"));
            Assert.True( CommandLineArgumentsFactory.IsFlag("/uac"));
            Assert.True( CommandLineArgumentsFactory.IsFlag("/admin"));
            Assert.True( CommandLineArgumentsFactory.IsFlag("/elevate"));
            Assert.True( CommandLineArgumentsFactory.IsFlag("/launch"));
            Assert.True( CommandLineArgumentsFactory.IsFlag("/run"));
            Assert.True( CommandLineArgumentsFactory.IsFlag("/unknown"));

            Assert.False( CommandLineArgumentsFactory.IsFlag("run"));
            Assert.False( CommandLineArgumentsFactory.IsFlag("admin"));
            Assert.False( CommandLineArgumentsFactory.IsFlag("silent"));
            Assert.False( CommandLineArgumentsFactory.IsFlag("elevate"));
            Assert.False(CommandLineArgumentsFactory.IsFlag("unknown"));
        }

        [Fact]
        public void CustomSetupArg()
        {
            Assert.True(CommandLineArgumentsFactory.Parse(new ApplicationUpdaterConfig { CommandLineArgument = "installsharp", CommandLine = "installsharp /launch" }).Launch);
        }
    }
}