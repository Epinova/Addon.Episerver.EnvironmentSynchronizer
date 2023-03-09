using Addon.Episerver.EnvironmentSynchronizer.Models;
using EPiServer.Logging;
using EPiServer.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using Microsoft.Extensions.Options;

namespace Addon.Episerver.EnvironmentSynchronizer.Configuration
{
	public class ConfigurationReader : IConfigurationReader
	{
		private static readonly ILogger Logger = LogManager.GetLogger();

		private readonly EnvironmentSynchronizerOptions _configuration;

		public ConfigurationReader(IOptions<EnvironmentSynchronizerOptions> synchronizerConfiguration)
        {
			_configuration = synchronizerConfiguration.Value;
        }

		public SynchronizationData ReadConfiguration()
		{
			var syncData = new SynchronizationData();

			if(_configuration == null)
            {
				return syncData;
            }

			try
			{
				syncData.RunAsInitializationModule = _configuration.RunAsInitializationModule;
				syncData.RunInitializationModuleEveryStartup = _configuration.RunInitializationModuleEveryStartup;

				if (_configuration.SiteDefinitions != null && _configuration.SiteDefinitions.Count > 0)
				{
					syncData.SiteDefinitions = new List<EnvironmentSynchronizerSiteDefinition>();
					foreach (var options in _configuration.SiteDefinitions)
					{
						var siteDefinition = new EnvironmentSynchronizerSiteDefinition()
						{
							Id = string.IsNullOrEmpty(options.Id) ? Guid.Empty : new Guid(options.Id),
							Name = string.IsNullOrEmpty(options.Name) ? string.Empty : options.Name,
							SiteUrl = string.IsNullOrEmpty(options.SiteUrl) ? null : new Uri(options.SiteUrl),
							Hosts = ToHostDefinitions(options.Hosts),
							ForceLogin = options.ForceLogin
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

				if (_configuration.ScheduledJobs != null && _configuration.ScheduledJobs.Count > 0)
				{
					syncData.ScheduledJobs = new List<ScheduledJobDefinition>();
					foreach (var options in _configuration.ScheduledJobs)
					{
						var job = new ScheduledJobDefinition
						{
							Id = options.Id,
							Name = options.Name,
							IsEnabled = options.IsEnabled,
							AutoRun = options.AutoRun
						};
						syncData.ScheduledJobs.Add(job);
					}
				}
				else
				{
					Logger.Information($"Found no schedule jobs to handle.");
				}
			}
			catch (ArgumentException argEx)
			{
				if (argEx.Message.Contains("is not a valid host name"))
				{
					Logger.Error($"EnvironmentSynchronizer configuration found in the appSettings.json but there are hostnames that is not valid.", argEx);
				} else
				{
					Logger.Error($"EnvironmentSynchronizer configuration found in the appSettings.json but some arguments is not correct.", argEx);
				}
				
			}
			catch (Exception ex)
			{
				Logger.Error($"No configuration found in the appSettings.json. Missing EnvironmentSynchronizer section.", ex);
			}

			return syncData;
		}

		private List<HostDefinition> ToHostDefinitions(IList<HostOptions> hosts)
		{
			return hosts.Select(hostOptions => {
				return new HostDefinition
				{
					Name = hostOptions.Name,
					Type = hostOptions.Type != HostDefinitionType.Undefined ? hostOptions.Type : HostDefinitionType.Undefined,
					UseSecureConnection = hostOptions.UseSecureConnection,
					Language = string.IsNullOrEmpty(hostOptions.Language) ? null : new CultureInfo(hostOptions.Language)
				};
			}).ToList();
		}
	}
}
