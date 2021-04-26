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

		private readonly ISynchronizerConfiguration _configuration;

		public ConfigurationReader(ISynchronizerConfiguration synchronizerConfiguration)
        {
			_configuration = synchronizerConfiguration;
        }

		public SynchronizationData ReadConfiguration()
		{
			var syncData = new SynchronizationData();

			if(_configuration.Settings == null)
            {
				return syncData;
            }

			try
			{
				syncData.RunAsInitializationModule = _configuration.Settings.RunAsInitializationModule;
				syncData.RunInitializationModuleEveryStartup = _configuration.Settings.RunInitializationModuleEveryStartup;

				if (_configuration.Settings.SiteDefinitions != null && _configuration.Settings.SiteDefinitions.Count > 0)
				{
					syncData.SiteDefinitions = new List<SiteDefinition>();
					foreach (SiteDefinitionElement element in _configuration.Settings.SiteDefinitions)
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

				if (_configuration.Settings.ScheduleJobs != null && _configuration.Settings.ScheduleJobs.Count > 0)
				{
					syncData.ScheduledJobs = new List<ScheduledJobDefinition>();
					foreach (ScheduledJobElement element in _configuration.Settings.ScheduleJobs)
					{
						var job = new ScheduledJobDefinition
						{
							Id = element.Id,
							Name = element.Name,
							IsEnabled = element.IsEnabled,
							AutoRun = element.AutoRun
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
