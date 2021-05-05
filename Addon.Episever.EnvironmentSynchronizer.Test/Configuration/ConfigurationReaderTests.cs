using Microsoft.VisualStudio.TestTools.UnitTesting;
using Addon.Episerver.EnvironmentSynchronizer.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using System.Configuration;
using System.IO;

namespace Addon.Episerver.EnvironmentSynchronizer.Configuration.Tests
{
    [TestClass()]
    public class ConfigurationReaderTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod()]
        public void ReadConfiguration_Null_Settings_Test()
        {
            // Arrange
            var moqConfig = new Mock<ISynchronizerConfiguration>();

            moqConfig.SetupGet(p => p.Settings).Returns((SynchronizerSection)null);

            var configReader = new ConfigurationReader(moqConfig.Object);

            // Act
            var data = configReader.ReadConfiguration();

            // Assert
            Assert.IsNotNull(data);
        }

        [TestMethod()]
        [DeploymentItem("test-configs\\all-settings.config")]
        public void ReadConfiguration_SiteDefinition_All_Settings_Test()
        {
            // Arrange

            var configFileMap = new ExeConfigurationFileMap()
            {
                ExeConfigFilename = Path.Combine(TestContext.DeploymentDirectory, "all-settings.config")
            };

            var config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
            var synchronizerSection = config.GetSection("env.synchronizer") as SynchronizerSection;

            var moqConfig = new Mock<ISynchronizerConfiguration>();

            moqConfig.SetupGet(p => p.Settings).Returns(synchronizerSection);

            var configReader = new ConfigurationReader(moqConfig.Object);

            // Act
            var data = configReader.ReadConfiguration();

            // Assert
            Assert.IsNotNull(data);

            // Site Definitions
            Assert.AreEqual(1, data.SiteDefinitions.Count);
            Assert.AreEqual(Guid.Parse("6AAEAF2F-20F9-41EB-8260-D0BBA76DB141"), data.SiteDefinitions[0].Id);
            Assert.AreEqual("CustomerX", data.SiteDefinitions[0].Name);
            Assert.AreEqual("https://custxmstr972znb5prep.azurewebsites.net/", data.SiteDefinitions[0].SiteUrl.AbsoluteUri);
            Assert.AreEqual(2, data.SiteDefinitions[0].Hosts.Count);

            Assert.AreEqual("*", data.SiteDefinitions[0].Hosts[0].Name);
            Assert.AreEqual("en", data.SiteDefinitions[0].Hosts[0].Language.Name);
            Assert.AreEqual(false, data.SiteDefinitions[0].Hosts[0].UseSecureConnection);

            // Scheduled jobs
            Assert.AreEqual(2, data.ScheduledJobs.Count);

            Assert.AreEqual("*", data.ScheduledJobs[0].Name);
            Assert.AreEqual("*", data.ScheduledJobs[0].Id);
            Assert.AreEqual(false, data.ScheduledJobs[0].IsEnabled);
            Assert.AreEqual(false, data.ScheduledJobs[0].AutoRun);

            Assert.AreEqual("YourScheduledJob", data.ScheduledJobs[1].Name);
            Assert.AreEqual("", data.ScheduledJobs[1].Id);
            Assert.AreEqual(true, data.ScheduledJobs[1].IsEnabled);
            Assert.AreEqual(true, data.ScheduledJobs[1].AutoRun);
        }

        [TestMethod()]
        [DeploymentItem("test-configs\\no-settings.config")]
        public void ReadConfiguration_SiteDefinition_No_Settings_Test()
        {
            // Arrange

            var configFileMap = new ExeConfigurationFileMap()
            {
                ExeConfigFilename = Path.Combine(TestContext.DeploymentDirectory, "no-settings.config")
            };

            var config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
            var synchronizerSection = config.GetSection("env.synchronizer") as SynchronizerSection;

            var moqConfig = new Mock<ISynchronizerConfiguration>();

            moqConfig.SetupGet(p => p.Settings).Returns(synchronizerSection);

            var configReader = new ConfigurationReader(moqConfig.Object);

            // Act
            var data = configReader.ReadConfiguration();

            // Assert
            Assert.IsNotNull(data);

            Assert.IsFalse(data.RunAsInitializationModule);
            Assert.IsFalse(data.RunInitializationModuleEveryStartup);

            Assert.IsNull(data.SiteDefinitions);
            Assert.IsNull(data.ScheduledJobs);
        }
    }
}