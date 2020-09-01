using System;
using Xunit;

namespace InstallSharp.Tests
{
    public class UpdateContextTests
    {
        [Fact]
        public void DetectExeUpdate()
        {
            var config = new ApplicationUpdaterConfig
            {
                FullFileName = @"D:\Apps\MyApp\MyApp.update.exe",
            };

            var updater = new ApplicationUpdater(config);

            var context = updater.GetUpdateContext();

            Assert.True(context.IsUpdate);
            Assert.Equal(PackageType.Executable, context.PackageType);
            Assert.Equal(@"D:\Apps\MyApp\MyApp.exe", context.UpdateDestination.FullName);
            Assert.Equal(@"D:\Apps\MyApp\MyApp.update.exe", context.UpdateSource.FullName);
        }


        [Fact]
        public void DetectExeUpdateWithCustomSuffix()
        {
            var config = new ApplicationUpdaterConfig
            {
                FullFileName = @"D:\Apps\MyApp\MyApp.patch.exe",
                UpdateSuffix = ".patch"
            };

            var updater = new ApplicationUpdater(config);

            var context = updater.GetUpdateContext();

            Assert.True(context.IsUpdate);
            Assert.Equal(PackageType.Executable, context.PackageType);
            Assert.Equal(@"D:\Apps\MyApp\MyApp.exe", context.UpdateDestination.FullName);
            Assert.Equal(@"D:\Apps\MyApp\MyApp.patch.exe", context.UpdateSource.FullName);
        }

        [Fact]
        public void DetectArchiveUpdate()
        {
            var config = new ApplicationUpdaterConfig
            {
                FullFileName = @"D:\Apps\MyApp\.update\MyApp.exe"
            };

            var updater = new ApplicationUpdater(config);

            var context = updater.GetUpdateContext();

            Assert.True(context.IsUpdate);
            Assert.Equal(PackageType.Archive, context.PackageType);
            Assert.Equal(@"D:\Apps\MyApp", context.UpdateDestination.FullName);
            Assert.Equal(@"D:\Apps\MyApp\.update", context.UpdateSource.FullName);
        }

        [Fact]
        public void DetectArchiveUpdateWithCustomSuffix()
        {
            var config = new ApplicationUpdaterConfig
            {
                FullFileName = @"D:\Apps\MyApp\.tmp\MyApp.exe",
                UpdateSuffix = ".tmp"
            };

            var updater = new ApplicationUpdater(config);

            var context = updater.GetUpdateContext();

            Assert.True(context.IsUpdate);
            Assert.Equal(PackageType.Archive, context.PackageType);
            Assert.Equal(@"D:\Apps\MyApp", context.UpdateDestination.FullName);
            Assert.Equal(@"D:\Apps\MyApp\.tmp", context.UpdateSource.FullName);
        }

        [Fact]
        public void DetectArchiveTemp()
        {
            var config = new ApplicationUpdaterConfig
            {
                FullFileName = Environment.ExpandEnvironmentVariables(@"%TEMP%\TempZip\MyApp.exe")
            };

            var updater = new ApplicationUpdater(config);

            var context = updater.GetUpdateContext();

            Assert.False(context.IsUpdate);
            Assert.False(context.IsDeployed);
            Assert.Equal(PackageType.Archive, context.PackageType);
            Assert.Equal( Environment.ExpandEnvironmentVariables(@"%LocalAppData%\Programs\MyApp"), context.UpdateDestination.FullName);
            Assert.Equal(Environment.ExpandEnvironmentVariables(@"%TEMP%\TempZip"), context.UpdateSource.FullName);
        }

        [Fact]
        public void DetectDownloadsFolder()
        {
            var config = new ApplicationUpdaterConfig
            {
                FullFileName = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\Downloads\MyApp.exe")
            };

            var updater = new ApplicationUpdater(config);

            var context = updater.GetUpdateContext();

            Assert.False(context.IsUpdate);
            Assert.False(context.IsDeployed);
            Assert.Equal(PackageType.Executable, context.PackageType);
            Assert.Equal(Environment.ExpandEnvironmentVariables(@"%LocalAppData%\Programs\MyApp"), context.UpdateDestination.FullName);
            Assert.Equal(Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\Downloads\MyApp.exe"), context.UpdateSource.FullName);
        }
    }
}