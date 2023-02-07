using System;
using Moq;
using System.Collections.Generic;
using Addon.Episerver.EnvironmentSynchronizer.Configuration;
using Addon.Episerver.EnvironmentSynchronizer.Models;
using Addon.Episerver.EnvironmentSynchronizer.Synchronizers.ScheduledJobs;
using EPiServer.DataAbstraction;
using EPiServer.Scheduler;
using Xunit;

namespace Addon.Episerver.EnvironmentSynchronizer.Test
{
	public class ScheduleJobSynchronizerTests
	{
		private List<ScheduledJob> ListOfScheduledJobs()
		{
			var listOfScheduledJobs = new List<ScheduledJob>();
			listOfScheduledJobs.Add(new ScheduledJob { ID = Guid.Parse("5c7d4c45-2e67-4275-a567-e7b6c98429c2"), IsEnabled = false, Name = "Test" });
			listOfScheduledJobs.Add(new ScheduledJob { ID = Guid.Parse("147f865b-3360-4804-9640-81e5cfe1d56c"), IsEnabled = false, Name = "Test2" });
			return listOfScheduledJobs;
		}

		[Fact]
		public void SynchronizeOneScheduledJob()
		{
			//Arrange
			var scheduleJobRepo = new Mock<IScheduledJobRepository>();
			var scheduleJobExecutor = new Mock<IScheduledJobExecutor>();

			var configReader = new Mock<IConfigurationReader>();

			scheduleJobRepo.Setup(x => x.List()).Returns(ListOfScheduledJobs);

			var syncData = new SynchronizationData();
			syncData.RunAsInitializationModule = false;
			syncData.RunInitializationModuleEveryStartup = false;
			syncData.ScheduledJobs = new List<ScheduledJobDefinition>();
			syncData.ScheduledJobs.Add(new ScheduledJobDefinition { Id = "5c7d4c45-2e67-4275-a567-e7b6c98429c2", IsEnabled = true, Name = "Test", AutoRun = false });
			configReader.Setup(x => x.ReadConfiguration()).Returns(syncData);

			var scheduledJobSynchronizer = new ScheduledJobSynchronizer(scheduleJobRepo.Object, scheduleJobExecutor.Object, configReader.Object);
			var environmentName = "environmentTest";

			//Act
			var resultLog = scheduledJobSynchronizer.Synchronize(environmentName);

			//Assert
			Assert.Contains("Set Test (5c7d4c45-2e67-4275-a567-e7b6c98429c2) to IsEnabled=True.", resultLog);
			Assert.DoesNotContain("Ran Test (5c7d4c45-2e67-4275-a567-e7b6c98429c2).", resultLog);
		}

		[Fact]
		public void SynchronizeOneScheduledJob_WithAutoRun()
		{
			//Arrange
			var scheduleJobRepo = new Mock<IScheduledJobRepository>();
			var scheduleJobExecutor = new Mock<IScheduledJobExecutor>();

			var configReader = new Mock<IConfigurationReader>();

			scheduleJobRepo.Setup(x => x.List()).Returns(ListOfScheduledJobs);

			var syncData = new SynchronizationData();
			syncData.RunAsInitializationModule = false;
			syncData.RunInitializationModuleEveryStartup = false;
			syncData.ScheduledJobs = new List<ScheduledJobDefinition>();
			syncData.ScheduledJobs.Add(new ScheduledJobDefinition { Id = "5c7d4c45-2e67-4275-a567-e7b6c98429c2", IsEnabled = true, Name = "Test", AutoRun = true });
			configReader.Setup(x => x.ReadConfiguration()).Returns(syncData);

			var scheduledJobSynchronizer = new ScheduledJobSynchronizer(scheduleJobRepo.Object, scheduleJobExecutor.Object, configReader.Object);
			var environmentName = "environmentTest";

			//Act
			var resultLog = scheduledJobSynchronizer.Synchronize(environmentName);

			//Assert
			Assert.Contains("Set Test (5c7d4c45-2e67-4275-a567-e7b6c98429c2) to IsEnabled=True.", resultLog);
			Assert.Contains("Ran Test (5c7d4c45-2e67-4275-a567-e7b6c98429c2).", resultLog);
		}

		[Fact]
		public void SynchronizeAllScheduledJobs()
		{
			//Arrange
			var scheduleJobRepo = new Mock<IScheduledJobRepository>();
			var scheduleJobExecutor = new Mock<IScheduledJobExecutor>();
			var configReader = new Mock<IConfigurationReader>();

			scheduleJobRepo.Setup(x => x.List()).Returns(ListOfScheduledJobs);

			var syncData = new SynchronizationData();
			syncData.RunAsInitializationModule = false;
			syncData.RunInitializationModuleEveryStartup = false;
			syncData.ScheduledJobs = new List<ScheduledJobDefinition>();
			syncData.ScheduledJobs.Add(new ScheduledJobDefinition { Id = "*", IsEnabled = true, Name = string.Empty });
			configReader.Setup(x => x.ReadConfiguration()).Returns(syncData);

			var scheduledJobSynchronizer = new ScheduledJobSynchronizer(scheduleJobRepo.Object, scheduleJobExecutor.Object, configReader.Object);
			var environmentName = "environmentTest";

			//Act
			var resultLog = scheduledJobSynchronizer.Synchronize(environmentName);

			//Assert
			Assert.Contains("Set Test (5c7d4c45-2e67-4275-a567-e7b6c98429c2) to IsEnabled=True.", resultLog);
			Assert.Contains("Set Test2 (147f865b-3360-4804-9640-81e5cfe1d56c) to IsEnabled=True.", resultLog);
		}

		[Fact]
		public void SynchronizeAllExceptOneScheduledJobs()
		{
			//Arrange
			var scheduleJobRepo = new Mock<IScheduledJobRepository>();
			var scheduleJobExecutor = new Mock<IScheduledJobExecutor>();

			var configReader = new Mock<IConfigurationReader>();

			scheduleJobRepo.Setup(x => x.List()).Returns(ListOfScheduledJobs);

			var syncData = new SynchronizationData();
			syncData.RunAsInitializationModule = false;
			syncData.RunInitializationModuleEveryStartup = false;
			syncData.ScheduledJobs = new List<ScheduledJobDefinition>();
			syncData.ScheduledJobs.Add(new ScheduledJobDefinition { Id = "*", IsEnabled = true, Name = string.Empty });
			syncData.ScheduledJobs.Add(new ScheduledJobDefinition { Id = "147f865b-3360-4804-9640-81e5cfe1d56c", IsEnabled = false, Name = string.Empty });
			configReader.Setup(x => x.ReadConfiguration()).Returns(syncData);

			var scheduledJobSynchronizer = new ScheduledJobSynchronizer(scheduleJobRepo.Object, scheduleJobExecutor.Object, configReader.Object);
			var environmentName = "environmentTest";

			//Act
			var resultLog = scheduledJobSynchronizer.Synchronize(environmentName);

			//Assert
			Assert.Contains("Set Test (5c7d4c45-2e67-4275-a567-e7b6c98429c2) to IsEnabled=True.", resultLog);
			Assert.Contains("Set Test2 (147f865b-3360-4804-9640-81e5cfe1d56c) to IsEnabled=False.", resultLog);
		}

		[Fact]
		public void SynchronizeNoneScheduledJobs()
		{
			//Arrange
			var scheduleJobRepo = new Mock<IScheduledJobRepository>();
			var scheduleJobExecutor = new Mock<IScheduledJobExecutor>();
			var configReader = new Mock<IConfigurationReader>();

			scheduleJobRepo.Setup(x => x.List()).Returns(ListOfScheduledJobs);

			var syncData = new SynchronizationData();
			syncData.RunAsInitializationModule = false;
			syncData.RunInitializationModuleEveryStartup = false;
			syncData.ScheduledJobs = new List<ScheduledJobDefinition>();
			syncData.ScheduledJobs.Add(new ScheduledJobDefinition { Id = "xxx", IsEnabled = true, Name = "Wrong" });
			configReader.Setup(x => x.ReadConfiguration()).Returns(syncData);

			var scheduledJobSynchronizer = new ScheduledJobSynchronizer(scheduleJobRepo.Object, scheduleJobExecutor.Object, configReader.Object);
			var environmentName = "environmentTest";

			//Act
			var resultLog = scheduledJobSynchronizer.Synchronize(environmentName);

			//Assert
			Assert.Contains("Could not find scheduled job with id=\"xxx\" name=\"Wrong\"", resultLog);
		}

	}
}
