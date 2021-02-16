using System.Configuration;

namespace Addon.Episerver.EnvironmentSynchronizer.Configuration
{
	public class SynchronizerSection : ConfigurationSection
	{
		[ConfigurationProperty("runAsInitializationModule", IsRequired = false)]
		public bool RunAsInitializationModule
		{
			get => (bool)this["runAsInitializationModule"];
			set => this["runAsInitializationModule"] = value;
		}

		[ConfigurationProperty("runInitializationModuleEveryStartup", IsRequired = false)]
		public bool RunInitializationModuleEveryStartup
		{
			get => (bool)this["runInitializationModuleEveryStartup"];
			set => this["runInitializationModuleEveryStartup"] = value;
		}

		[ConfigurationProperty("sitedefinitions")]
		[ConfigurationCollection(typeof(SiteDefinitionCollection), AddItemName = "sitedefinition")]
		public SiteDefinitionCollection SiteDefinitions
		{
			get
			{
				if (base["sitedefinitions"] != null && ((ConfigurationElement)base["sitedefinitions"]).ElementInformation.IsPresent)
				{
					return (SiteDefinitionCollection)base["sitedefinitions"];
				}
				else
				{
					var defaultCollection = new SiteDefinitionCollection();
					defaultCollection.AddElement(new SiteDefinitionElement { Name = string.Empty });
					return defaultCollection;
				}
			}
		}

		[ConfigurationProperty("scheduledjobs")]
		[ConfigurationCollection(typeof(ScheduledJobCollection), AddItemName = "scheduledjob")]
		public ScheduledJobCollection ScheduleJobs
		{
			get
			{
				if (base["scheduledjobs"] != null && ((ConfigurationElement)base["scheduledjobs"]).ElementInformation.IsPresent)
				{
					return (ScheduledJobCollection)base["scheduledjobs"];
				}
				else
				{
					var defaultCollection = new ScheduledJobCollection();
					defaultCollection.AddElement(new ScheduledJobElement() { Name = string.Empty });
					return defaultCollection;
				}
			}
		}
	}
}
