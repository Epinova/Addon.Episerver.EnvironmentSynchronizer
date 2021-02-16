using Addon.Episerver.EnvironmentSynchronizer.Configuration;
using EPiServer.Web;
using System.Collections.Generic;

namespace Addon.Episerver.EnvironmentSynchronizer.Models
{
	public class SynchronizationData : ISynchronizationData
	{
		public bool RunAsInitializationModule { get; set; }
		public bool RunInitializationModuleEveryStartup { get; set; }
		public List<SiteDefinition> SiteDefinitions { get; set; }
		public List<ScheduledJobDefinition> ScheduledJobs { get; set; }
	}
}
