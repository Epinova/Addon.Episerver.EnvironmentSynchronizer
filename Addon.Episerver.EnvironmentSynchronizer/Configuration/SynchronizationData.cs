using Addon.Episerver.EnvironmentSynchronizer.Configuration;
using System.Collections.Generic;

namespace Addon.Episerver.EnvironmentSynchronizer.Models
{
	public class SynchronizationData
	{
		public bool RunAsInitializationModule { get; set; }
		public bool RunInitializationModuleEveryStartup { get; set; }
		public List<EnvironmentSynchronizerSiteDefinition> SiteDefinitions { get; set; }
		public List<ScheduledJobDefinition> ScheduledJobs { get; set; }
	}
}
