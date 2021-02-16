using System;
using System.Diagnostics;
using System.Text;
using EPiServer.Logging;
using EPiServer.PlugIn;
using EPiServer.Scheduler;

namespace Addon.Episerver.EnvironmentSynchronizer.Jobs
{
	[ScheduledPlugIn(
		DisplayName = "Environment Synchronization", 
		Description = "Ensures that content and settings that are stored in the databases are corrected given the current environment. This is helpful after a content synchronization between different Episerver environments. https://github.com/ovelartelius/episerver-env-sync", 
		SortIndex = 100,
		GUID = "1eda8c91-a367-41df-adee-e6143b1e37c3")
	]
	public class EnvironmentSynchronizationJob : ScheduledJobBase
	{
		private static readonly ILogger Logger = LogManager.GetLogger();
		private readonly IEnvironmentSynchronizationManager _environmentSynchronizationManager;

		public EnvironmentSynchronizationJob(IEnvironmentSynchronizationManager environmentSynchronizationManager)
		{
			IsStoppable = false;
			_environmentSynchronizationManager = environmentSynchronizationManager;
		}

		private long Duration { get; set; }

		public override string Execute()
		{
			var tmr = Stopwatch.StartNew();
			var resultLog = new StringBuilder();

			try
			{
				resultLog.AppendLine(_environmentSynchronizationManager.Synchronize()) ;
			}
			catch (Exception ex)
			{
				Logger.Error("Error when run Environment Synchronization job.", ex);
			}

			tmr.Stop();
			Duration = tmr.ElapsedMilliseconds;

			resultLog.AppendLine(
				$"Ran environment synchronization job. {Duration}ms on {Environment.MachineName}.");

			return resultLog.ToString();
		}
	}
}
