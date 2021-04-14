using Addon.Episerver.EnvironmentSynchronizer.Models;
using EPiServer.Logging;
using EPiServer.Web;
using System;
using System.Collections.Generic;
using EPiServer.ServiceLocation;

namespace Addon.Episerver.EnvironmentSynchronizer.Configuration
{
	[ServiceConfiguration(typeof(IConfigurationReader))]
	public class ConfigurationReader : IConfigurationReader
	{
		private static readonly ILogger Logger = LogManager.GetLogger();

		private readonly ISynchronizerConfiguration configuration;

		public ConfigurationReader(ISynchronizerConfiguration synchronizerConfiguration)
        {
			this.configuration = synchronizerConfiguration;
        }

		public SynchronizationData ReadConfiguration()
		{
			var syncData = new SynchronizationData();

			if(this.configuration.Settings == null)
            {
				return syncData;
            }

			try
			{
				syncData.RunAsInitializationModule = this.configuration.Settings.RunAsInitializationModule;
				syncData.RunInitializationModuleEveryStartup = this.configuration.Settings.RunInitializationModuleEveryStartup;

				if (this.configuration.Settings.SiteDefinitions != null && this.configuration.Settings.SiteDefinitions.Count > 0)
				{
					syncData.SiteDefinitions = new List<SiteDefinition>();
					foreach (SiteDefinitionElement element in this.configuration.Settings.SiteDefinitions)
					{
						var siteDefinition = new SiteDefinition()
						{
							Id = string.IsNullOrEmpty(element.Id) ? Guid.Empty : new Guid(element.Id),
							Name = string.IsNullOrEmpty(element.Name) ? string.Empty : element.Name,
							SiteUrl = string.IsNullOrEmpty(element.SiteUrl) ? null : new Uri(element.SiteUrl),
							Hosts = element.Hosts.ToHostDefinitions()
						};
						if (!string.IsNullOrEmpty(siteDefinition.Name) && siteDefinition.SiteUrl != null)
						{
							syncData.SiteDefinitions.Add(siteDefinition);
						}
					}
				}
				else
				{
					Logger.Information($"Found no site definitions to handle.");
				}

				if (this.configuration.Settings.ScheduleJobs != null && this.configuration.Settings.ScheduleJobs.Count > 0)
				{
					syncData.ScheduledJobs = new List<ScheduledJobDefinition>();
					foreach (ScheduledJobElement element in this.configuration.Settings.ScheduleJobs)
					{
						var job = new ScheduledJobDefinition
						{
							Id = element.Id,
							Name = element.Name,
							IsEnabled = element.IsEnabled
						};
						syncData.ScheduledJobs.Add(job);
					}
				}
				else
				{
					Logger.Information($"Found no schedule jobs to handle.");
				}
			}
			catch (Exception ex)
			{
				Logger.Error($"No configuration found in the web.config. Missing env.synchronizer section.", ex);
			}

			return syncData;
		}
	}
}
