using Addon.Episerver.EnvironmentSynchronizer.Configuration;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Addon.Episerver.EnvironmentSynchronizer.Synchronizers.SiteDefinitions
{
    [ServiceConfiguration(typeof(IEnvironmentSynchronizer))]
    public class SiteDefinitionSynchronizer : IEnvironmentSynchronizer
    {
        private static readonly ILogger Logger = LogManager.GetLogger();
        private readonly ISiteDefinitionRepository _siteDefinitionRepository;
        private readonly IConfigurationReader _configurationReader;
        private StringBuilder resultLog = new StringBuilder();

        public SiteDefinitionSynchronizer(
            ISiteDefinitionRepository siteDefinitionRepository,
            IConfigurationReader configurationReader)
        {
	        Logger.Information("SiteDefinitionSynchronizer initialized.");
            _siteDefinitionRepository = siteDefinitionRepository;
            _configurationReader = configurationReader;
        }

        public string Synchronize(string environmentName)
        {
            var syncConfiguration = _configurationReader.ReadConfiguration();

            if (syncConfiguration.SiteDefinitions == null || !syncConfiguration.SiteDefinitions.Any())
            {
                Logger.Information("No site definitions found to synchronize.");
                resultLog.AppendLine("No site definitions found to synchronize.<br />");
                return resultLog.ToString();
            }

            var stopwatch = Stopwatch.StartNew();

            try
            {
                var updatedSites = MergeSiteDefinitions(syncConfiguration.SiteDefinitions);

                Logger.Information($"Updated total of {updatedSites} sites.");
                resultLog.AppendLine($"Updated total of {updatedSites} sites.<br />");
            }
            catch (Exception ex)
            {
                Logger.Error("An exception occured when trying to synchronize site definitions", ex);
                resultLog.AppendLine($"An exception occured when trying to synchronize site definitions: {ex.Message}<br />");
            }

            stopwatch.Stop();
            Logger.Information($"Synchronize site definitions took {stopwatch.ElapsedMilliseconds}ms.");

            return resultLog.ToString();
        }

        private int MergeSiteDefinitions(IEnumerable<SiteDefinition> siteDefinitionsToUpdate)
        {
            var updatedSites = 0;
            var existingSites = _siteDefinitionRepository.List();

            foreach (var siteDefinitionToUpdate in siteDefinitionsToUpdate)
            {
	            SiteDefinition site = GetExistingSiteDefinition(existingSites, siteDefinitionToUpdate);
                if (site != null)
                {
                    site = site.CreateWritableClone();
                    if (!string.IsNullOrEmpty(siteDefinitionToUpdate.Name) && site.Name != siteDefinitionToUpdate.Name)
                    {
                        // Will set the name of the site to the provided Name if Id exist and Name is specified and different from the found existing site.
	                    site.Name = siteDefinitionToUpdate.Name;
                    }
                    site.SiteUrl = siteDefinitionToUpdate.SiteUrl;
                    site.Hosts = siteDefinitionToUpdate.Hosts;

                    _siteDefinitionRepository.Save(site);
                    updatedSites++;
                    Logger.Information($"Updated {siteDefinitionToUpdate.Name} to site URL {siteDefinitionToUpdate.SiteUrl} and {siteDefinitionToUpdate.Hosts.Count} hostnames.");
                    resultLog.AppendLine($"Updated {siteDefinitionToUpdate.Name} to site URL {siteDefinitionToUpdate.SiteUrl} and {siteDefinitionToUpdate.Hosts.Count} hostnames.<br />");
                }
                else
                {
                    Logger.Warning($"Could not find site {siteDefinitionToUpdate.Name} or site already has site URL {siteDefinitionToUpdate.SiteUrl}.");
                    resultLog.AppendLine($"Could not find site {siteDefinitionToUpdate.Name} or site already has site URL {siteDefinitionToUpdate.SiteUrl}.<br />");
                }
            }

            return updatedSites;
        }

        private SiteDefinition GetExistingSiteDefinition(IEnumerable<SiteDefinition> existingSites, SiteDefinition siteDefinitionToUpdate)
        {
            SiteDefinition siteDefinition = null;

            if (siteDefinitionToUpdate.Id != Guid.Empty)
            {
                //Update the site definition if it doesn't have the same value for SiteUrl 
                siteDefinition = existingSites.FirstOrDefault(s => s.Id == siteDefinitionToUpdate.Id);
            }
            else
            {
                //Update the site definition if it doesn't have the same value for SiteUrl 
                siteDefinition = existingSites.FirstOrDefault(s => s.Name == siteDefinitionToUpdate.Name);
            }

            return siteDefinition;
        }
    }
}
