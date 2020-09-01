using System;
using Xunit;

namespace InstallSharp.Tests
{
    public class GuidTests
    {
        [Fact]
        public void PassedConfigurationTakesFirstPrecendence()
        {
            // Setup
            var args = new ApplicationUpdaterConfig {Guid = new Guid("55E21349-157D-4ADC-83ED-B47AE1CDB7E4") };
            var config = ApplicationUpdaterConfigFactory.Create(args, null, GetType().Assembly);

            // Verify assert we pick up the correct GUID
            Assert.Equal(new Guid("55E21349-157D-4ADC-83ED-B47AE1CDB7E4"), config.Guid);
        }

        [Fact]
        public void UseAssemblyGuidByDefault()
        {
            // Use the InstallSharp assembly, as it has a GUID attribute but no InstallSharp attribute. If we use the unit test assembly,
            // it has an InstallSharp attribute which will take precendence and not use the GUID attribute.
            var assembly = typeof(ApplicationUpdater).Assembly;

            // Setup
            var config = ApplicationUpdaterConfigFactory.Create(null, null, assembly);
            
            // Verify assert we pick up the correct GUID
            Assert.Equal(new Guid("55C2CFC0-A66A-4E54-A516-7483C4A57D9E"), config.Guid);
        }

        [Fact]
        public void InstallSharpAttributeTakesSecondPrecedence()
        {
            var assembly = GetType().Assembly;

            // Setup
            var config = ApplicationUpdaterConfigFactory.Create(null, null, assembly);

            // Verify assert we pick up the correct GUID
            Assert.Equal(new Guid("2607EA54-696D-4411-9F95-661C1FA6F626"), config.Guid);
        }
    }
}