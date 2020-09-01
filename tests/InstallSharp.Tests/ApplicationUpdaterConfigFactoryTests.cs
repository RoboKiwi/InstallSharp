using System;
using System.Diagnostics;
using Xunit;

namespace InstallSharp.Tests
{
    public class ApplicationUpdaterConfigFactoryTests
    {
        [Fact]
        public void InferredDefaults()
        {
            // As we're running in UnitTest, infer defaults from the specified InstallSharp assembly instead of the Unit Test host
            var assembly = typeof(ApplicationUpdater).Assembly;
            var info = FileVersionInfo.GetVersionInfo(assembly.Location);
            
            // Execute
            var args = ApplicationUpdaterConfigFactory.Create(null, info, assembly);
            
            // Verify
            Assert.Equal("InstallSharp", args.Name);
            Assert.Equal("InstallSharp.dll", args.FileName);
            Assert.Equal("InstallSharp.dll", args.AssetName);
            Assert.Equal(assembly.Location, args.FullFileName);
            Assert.Equal("InstallSharp", args.ProductName);
            Assert.Equal("Robo Kiwi", args.CompanyName);
            Assert.Equal(info.FileVersion, args.Version);
            Assert.Equal(new Guid("55C2CFC0-A66A-4E54-A516-7483C4A57D9E"), args.Guid);

            Assert.Equal(Environment.ExpandEnvironmentVariables("%LocalAppData%\\Programs\\InstallSharp"), args.InstallPath);

            Assert.Equal("github.com/RoboKiwi/InstallSharp", args.UpdateUrl);
        }


    }

    public class ApplicationUpdaterTests
    {
        [Fact]
        public void CommandLine()
        {
            var config = ApplicationUpdaterConfigFactory.Create(null, null, null);

        }
    }
}