using System;
using Addon.Episerver.EnvironmentSynchronizer.Configuration;
using Addon.Episerver.EnvironmentSynchronizer.DynamicData;
using Addon.Episerver.EnvironmentSynchronizer.Jobs;
using EPiServer.DataAbstraction;
using EPiServer.Logging;
using EPiServer.PlugIn;
using EPiServer.Scheduler;

namespace Addon.Episerver.EnvironmentSynchronizer.InitializationModule
{
	public interface IInitializationExecuter
	{
		void Initialize();
	}
	public class InitializationExecuter : IInitializationExecuter
	{
		private static readonly ILogger Logger = LogManager.GetLogger();

		private readonly IScheduledJobRepository _scheduledJobRepository;
		private readonly IConfigurationReader _configurationReader;
		private readonly IScheduledJobExecutor _scheduledJobExecutor;
		private readonly IEnvironmentSynchronizationManager _environmentSynchronizationManager;
		private readonly IEnvironmentSynchronizationStore _environmentSynchronizationStore;

		public InitializationExecuter(IScheduledJobRepository scheduledJobRepository, IConfigurationReader configurationReader, IScheduledJobExecutor scheduledJobExecutor, IEnvironmentSynchronizationManager environmentSynchronizationManager, IEnvironmentSynchronizationStore environmentSynchronizationStore)
		{
			_scheduledJobRepository = scheduledJobRepository;
			_configurationReader = configurationReader;
			_scheduledJobExecutor = scheduledJobExecutor;
			_environmentSynchronizationManager = environmentSynchronizationManager;	
			_environmentSynchronizationStore = environmentSynchronizationStore;
		}

		public void Initialize()
        {
			var syncData = _configurationReader.ReadConfiguration();

			if (syncData.RunAsInitializationModule)
			{
				Logger.Information($"Environment Synchronizer found RunAsInitializationModule=true");
				var runInitialization = true;

				if (!syncData.RunInitializationModuleEveryStartup)
				{
					Logger.Information($"Environment Synchronizer found RunInitializationModuleEveryStartup=false");
					var stamp = _environmentSynchronizationStore.GetStamp();
					if (stamp != null && stamp.Environment == _environmentSynchronizationManager.GetEnvironmentName())
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
					var job = _scheduledJobRepository.Get(Guid.Parse(jobId));
					_scheduledJobExecutor.StartAsync(job,
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


    }
}
