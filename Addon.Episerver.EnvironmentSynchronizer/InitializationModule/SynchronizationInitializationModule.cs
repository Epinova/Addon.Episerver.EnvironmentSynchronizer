using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Logging;
using System;
using Microsoft.Extensions.DependencyInjection;

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
				var executer = context.Services.GetRequiredService<IInitializationExecuter>();

				Logger.Information($"InitializableModule:SynchronizationInitializationModule Initialize");
				executer.Initialize();
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
