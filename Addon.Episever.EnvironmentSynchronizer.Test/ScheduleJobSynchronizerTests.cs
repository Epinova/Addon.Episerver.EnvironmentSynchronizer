using System;
using Addon.Episerver.EnvironmentSynchronizer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using Addon.Episerver.EnvironmentSynchronizer.Configuration;
using Addon.Episerver.EnvironmentSynchronizer.DynamicData;
using Addon.Episerver.EnvironmentSynchronizer.Models;
using Addon.Episerver.EnvironmentSynchronizer.Synchronizers.ScheduledJobs;
using EPiServer.DataAbstraction;

namespace Addon.Episerver.EnvironmentSynchronizer.Test
{
    [TestClass]
    public class ScheduleJobSynchronizerTests
    {
	    private List<ScheduledJob> ListOfScheduledJobs()
	    {
		    var listOfScheduledJobs = new List<ScheduledJob>();
		    listOfScheduledJobs.Add(new ScheduledJob { ID = Guid.Parse("5c7d4c45-2e67-4275-a567-e7b6c98429c2"), IsEnabled = false, Name = "Test" });
		    listOfScheduledJobs.Add(new ScheduledJob { ID = Guid.Parse("147f865b-3360-4804-9640-81e5cfe1d56c"), IsEnabled = false, Name = "Test2" });
		    return listOfScheduledJobs;
	    }

		[TestMethod]
        public void SynchronizeOneScheduledJob()
        {
            var scheduleJobRepo = new Mock<IScheduledJobRepository>();
            var configReader = new Mock<IConfigurationReader>();

            scheduleJobRepo.Setup(x => x.List()).Returns(ListOfScheduledJobs);

            var syncData = new SynchronizationData();
            syncData.RunAsInitializationModule = false;
            syncData.RunInitializationModuleEveryStartup = false;
            syncData.ScheduledJobs = new List<ScheduledJobDefinition>();
            syncData.ScheduledJobs.Add(new ScheduledJobDefinition { Id = "5c7d4c45-2e67-4275-a567-e7b6c98429c2", IsEnabled = true, Name = "Test" });
            configReader.Setup(x => x.ReadConfiguration()).Returns(syncData);

            var scheduledJobSynchronizer = new ScheduledJobSynchronizer(scheduleJobRepo.Object, configReader.Object);
            var environmentName = "environmentTest";

            var resultLog = scheduledJobSynchronizer.Synchronize(environmentName);

            Assert.IsTrue(resultLog.Contains("Set Test (5c7d4c45-2e67-4275-a567-e7b6c98429c2) to IsEnabled=True."));
        }

        [TestMethod]
        public void SynchronizeAllScheduledJobs()
        {
	        var scheduleJobRepo = new Mock<IScheduledJobRepository>();
	        var configReader = new Mock<IConfigurationReader>();

	        scheduleJobRepo.Setup(x => x.List()).Returns(ListOfScheduledJobs);

	        var syncData = new SynchronizationData();
	        syncData.RunAsInitializationModule = false;
	        syncData.RunInitializationModuleEveryStartup = false;
	        syncData.ScheduledJobs = new List<ScheduledJobDefinition>();
	        syncData.ScheduledJobs.Add(new ScheduledJobDefinition { Id = "*", IsEnabled = true, Name = string.Empty });
	        configReader.Setup(x => x.ReadConfiguration()).Returns(syncData);

	        var scheduledJobSynchronizer = new ScheduledJobSynchronizer(scheduleJobRepo.Object, configReader.Object);
	        var environmentName = "environmentTest";

	        var resultLog = scheduledJobSynchronizer.Synchronize(environmentName);

            Assert.IsTrue(resultLog.Contains("Set Test (5c7d4c45-2e67-4275-a567-e7b6c98429c2) to IsEnabled=True."));
	        Assert.IsTrue(resultLog.Contains("Set Test2 (147f865b-3360-4804-9640-81e5cfe1d56c) to IsEnabled=True."));
        }

        [TestMethod]
        public void SynchronizeAllExceptOneScheduledJobs()
        {
	        var scheduleJobRepo = new Mock<IScheduledJobRepository>();
	        var configReader = new Mock<IConfigurationReader>();

	        scheduleJobRepo.Setup(x => x.List()).Returns(ListOfScheduledJobs);

	        var syncData = new SynchronizationData();
	        syncData.RunAsInitializationModule = false;
	        syncData.RunInitializationModuleEveryStartup = false;
	        syncData.ScheduledJobs = new List<ScheduledJobDefinition>();
	        syncData.ScheduledJobs.Add(new ScheduledJobDefinition { Id = "*", IsEnabled = true, Name = string.Empty });
	        syncData.ScheduledJobs.Add(new ScheduledJobDefinition { Id = "147f865b-3360-4804-9640-81e5cfe1d56c", IsEnabled = false, Name = string.Empty });
			configReader.Setup(x => x.ReadConfiguration()).Returns(syncData);

	        var scheduledJobSynchronizer = new ScheduledJobSynchronizer(scheduleJobRepo.Object, configReader.Object);
	        var environmentName = "environmentTest";

	        var resultLog = scheduledJobSynchronizer.Synchronize(environmentName);

	        Assert.IsTrue(resultLog.Contains("Set Test (5c7d4c45-2e67-4275-a567-e7b6c98429c2) to IsEnabled=True."));
	        Assert.IsTrue(resultLog.Contains("Set Test2 (147f865b-3360-4804-9640-81e5cfe1d56c) to IsEnabled=False."));
        }

        [TestMethod]
        public void SynchronizeNoneScheduledJobs()
        {
	        var scheduleJobRepo = new Mock<IScheduledJobRepository>();
	        var configReader = new Mock<IConfigurationReader>();

	        scheduleJobRepo.Setup(x => x.List()).Returns(ListOfScheduledJobs);

	        var syncData = new SynchronizationData();
	        syncData.RunAsInitializationModule = false;
	        syncData.RunInitializationModuleEveryStartup = false;
	        syncData.ScheduledJobs = new List<ScheduledJobDefinition>();
	        syncData.ScheduledJobs.Add(new ScheduledJobDefinition { Id = "xxx", IsEnabled = true, Name = "Wrong" });
	        configReader.Setup(x => x.ReadConfiguration()).Returns(syncData);

	        var scheduledJobSynchronizer = new ScheduledJobSynchronizer(scheduleJobRepo.Object, configReader.Object);
	        var environmentName = "environmentTest";

	        var resultLog = scheduledJobSynchronizer.Synchronize(environmentName);

	        Assert.IsTrue(resultLog.Contains("Could not find scheduled job with id=\"xxx\" name=\"Wrong\""));
        }
	}
}
