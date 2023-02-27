using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
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
				if (inOpEx.Message.Contains("InitializationExecuter"))
				{
					Logger.Error("Addon.EpiServer.EnvironmentSynchronizer tried to run InitializationModule but 'services.AddEnvironmentSynchronization();' looks like it is missing in startup.cs.", inOpEx);
				} else
				{
					Logger.Error("Addon.EpiServer.EnvironmentSynchronizer tried to run InitializationExecuter.Initialize but failed.", inOpEx);
				}
				
			}
			catch (Exception ex)
			{
				Logger.Error("Could not get load EnvironmentSynchronizationJob. SynchronizationInitializationModule will not run.", ex);
			}
		}

		public void Preload(string[] parameters) { }

		public void Uninitialize(InitializationEngine context) { }


	}
}
