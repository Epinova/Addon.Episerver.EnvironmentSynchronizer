using Addon.Episerver.EnvironmentSynchronizer.Configuration;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Addon.Episerver.EnvironmentSynchronizer.InitializationModule
{

	[InitializableModule]
	[ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
	public class SynchronizationInitializationModule : IInitializableModule
	{
		private static readonly ILogger Logger = LogManager.GetLogger();

		public void Initialize(InitializationEngine context)
		{
			try
			{
				var _executer = ServiceLocator.Current.GetInstance<IInitializationExecuter>();

				Logger.Information($"InitializableModule:SynchronizationInitializationModule Initialize");
				_executer.Initialize();

			}
			catch (InvalidOperationException inOpEx)
			{
				if (inOpEx.Message.Contains("IInitializationExecuter"))
				{
					Logger.Error("Addon.EpiServer.EnvironmentSynchronizer tried to run InitializationModule but 'services.AddEnvironmentSynchronization(_configuration)' looks like it is missing in startup.cs.", inOpEx);
				} else
				{
					Logger.Error("Addon.EpiServer.EnvironmentSynchronizer tried to run InitializationExecuter.Initialize but failed.", inOpEx);
				}
				
			}
			catch (Exception ex)
			{
				Logger.Error("Could not get load EnvironmentSynchronizationJob. SynchronizationInitializationModule will not run.", ex);
			}

			//var scheduledJobRepository = ServiceLocator.Current.GetInstance<IScheduledJobRepository>();
			//var configReader = ServiceLocator.Current.GetInstance<IConfigurationReader>();
			//var scheduledJobExecutor = ServiceLocator.Current.GetInstance<IScheduledJobExecutor>();

			//var syncData = configReader.ReadConfiguration();

			//if (syncData.RunAsInitializationModule)
			//{
			//	Logger.Information($"Environment Synchronizer found RunAsInitializationModule=true");
			//	var runInitialization = true;
			//	var environmentSynchronizationManager = ServiceLocator.Current.GetInstance<EnvironmentSynchronizationManager>();

			//	if (!syncData.RunInitializationModuleEveryStartup)
			//	{
			//		Logger.Information($"Environment Synchronizer found RunInitializationModuleEveryStartup=false");
			//		var store = ServiceLocator.Current.GetInstance<EnvironmentSynchronizationStore>();
			//		var stamp = store.GetStamp();
			//		if (stamp != null && stamp.Environment == environmentSynchronizationManager.GetEnvironmentName())
			//		{
			//			runInitialization = false;
			//			Logger.Information($"Environment Synchronizer will not run. Stamp match the current environment {stamp.Environment}");
			//		}
			//	}

			//	if (!runInitialization) { return; }

			//	try
			//	{
			//		var jobId =
			//			((ScheduledPlugInAttribute)typeof(EnvironmentSynchronizationJob).GetCustomAttributes(
			//				typeof(ScheduledPlugInAttribute), true)[0]).GUID;
			//		var job = scheduledJobRepository.Get(Guid.Parse(jobId));
			//		scheduledJobExecutor.StartAsync(job,
			//			new JobExecutionOptions
			//			{
			//				RunSynchronously = true,
			//				Trigger = ScheduledJobTrigger.User
			//			});
			//	}
			//	catch (Exception ex)
			//	{
			//		Logger.Error("Could not get find or load EnvironmentSynchronizationJob. SynchronizationInitializationModule will not run.", ex);
			//	}
			//}
		}

		public void Preload(string[] parameters) { }

		public void Uninitialize(InitializationEngine context) { }


	}
}
