using System.Collections.Generic;
using Addon.Episerver.EnvironmentSynchronizer.Configuration;
using EPiServer.Web;

namespace Addon.Episerver.EnvironmentSynchronizer.Models
{
	public interface ISynchronizationData
	{
		bool RunAsInitializationModule { get; set; }
		List<SiteDefinition> SiteDefinitions { get; set; }
		List<ScheduledJobDefinition> ScheduledJobs { get; set; }
	}
}