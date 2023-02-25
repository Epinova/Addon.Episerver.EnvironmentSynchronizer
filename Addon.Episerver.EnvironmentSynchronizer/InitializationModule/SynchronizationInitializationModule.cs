﻿using System;
using Addon.Episerver.EnvironmentSynchronizer.Configuration;
using Addon.Episerver.EnvironmentSynchronizer.DynamicData;
using Addon.Episerver.EnvironmentSynchronizer.Jobs;
using EPiServer.DataAbstraction;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Logging;
using EPiServer.PlugIn;
using EPiServer.Scheduler;
using EPiServer.ServiceLocation;

namespace Addon.Episerver.EnvironmentSynchronizer.InitializationModule
{

	[InitializableModule]
	[ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
	public class SynchronizationInitializationModule : IInitializableModule
	{
		private static readonly ILogger Logger = LogManager.GetLogger();

        public void Initialize(InitializationEngine context)
        {
            var scheduledJobRepository =  ServiceLocator.Current.GetInstance<IScheduledJobRepository>();
			var configReader = ServiceLocator.Current.GetInstance<IConfigurationReader>();
            var scheduledJobExecutor = ServiceLocator.Current.GetInstance<IScheduledJobExecutor>();

			var syncData = configReader.ReadConfiguration();

			if (syncData.RunAsInitializationModule)
			{
				Logger.Information($"Environment Synchronizer found RunAsInitializationModule=true");
				var runInitialization = true;
				var environmentSynchronizationManager = ServiceLocator.Current.GetInstance<EnvironmentSynchronizationManager>();

				if (!syncData.RunInitializationModuleEveryStartup)
				{
					Logger.Information($"Environment Synchronizer found RunInitializationModuleEveryStartup=false");
					var store = ServiceLocator.Current.GetInstance<EnvironmentSynchronizationStore>();
					var stamp = store.GetStamp();
					if (stamp != null && stamp.Environment == environmentSynchronizationManager.GetEnvironmentName())
					{
						runInitialization = false;
						Logger.Information($"Environment Synchronizer will not run. Stamp match the current environment {stamp.Environment}");
					}
				}

				if (!runInitialization) { return; }

				try
				{
					var jobId =
						((ScheduledPlugInAttribute) typeof(EnvironmentSynchronizationJob).GetCustomAttributes(
							typeof(ScheduledPlugInAttribute), true)[0]).GUID;
					var job = scheduledJobRepository.Get(Guid.Parse(jobId));
                    scheduledJobExecutor.StartAsync(job,
                        new JobExecutionOptions
                        {
                            RunSynchronously = true,
                            Trigger = ScheduledJobTrigger.User
                        });
				}
				catch (Exception ex)
				{
					Logger.Error("Could not get find or load EnvironmentSynchronizationJob. SynchronizationInitializationModule will not run.", ex);
				}
			}
		}

		public void Preload(string[] parameters) { }

		public void Uninitialize(InitializationEngine context) { }


    }
}
