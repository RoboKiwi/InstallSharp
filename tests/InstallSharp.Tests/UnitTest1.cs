using System;
using Xunit;

namespace InstallSharp.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var updater = new ApplicationUpdater();

            Assert.NotNull(ApplicationUpdater.fileVersionInfo);

        }
    }
}
