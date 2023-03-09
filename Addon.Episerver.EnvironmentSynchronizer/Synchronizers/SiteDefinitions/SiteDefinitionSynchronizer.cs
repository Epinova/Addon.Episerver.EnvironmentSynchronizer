using Addon.Episerver.EnvironmentSynchronizer.Configuration;
using EPiServer.DataAbstraction;
using EPiServer.Logging;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Addon.Episerver.EnvironmentSynchronizer.Synchronizers.SiteDefinitions
{
	[ServiceConfiguration(typeof(IEnvironmentSynchronizer))]
    public class SiteDefinitionSynchronizer : IEnvironmentSynchronizer
    {
        private static readonly ILogger Logger = LogManager.GetLogger();
        private readonly ISiteDefinitionRepository _siteDefinitionRepository;
		private readonly IContentSecurityRepository _contentSecurityRepository;
		private readonly IConfigurationReader _configurationReader;
        private StringBuilder resultLog = new StringBuilder();
        private string _environmentName = string.Empty;


		public SiteDefinitionSynchronizer(
            ISiteDefinitionRepository siteDefinitionRepository,
            IContentSecurityRepository contentSecurityRepository,
            IConfigurationReader configurationReader)
        {
	        Logger.Information("SiteDefinitionSynchronizer initialized.");
            _siteDefinitionRepository = siteDefinitionRepository;
            _contentSecurityRepository = contentSecurityRepository;
            _configurationReader = configurationReader;
        }

        public string Synchronize(string environmentName)
        {
            _environmentName = environmentName;

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

        private int MergeSiteDefinitions(IEnumerable<EnvironmentSynchronizerSiteDefinition> siteDefinitionsToUpdate)
        {
            var updatedSites = 0;
            var existingSites = _siteDefinitionRepository.List();

            foreach (var siteDefinitionToUpdate in siteDefinitionsToUpdate)
            {
	            SiteDefinition site = GetExistingSiteDefinition(existingSites, siteDefinitionToUpdate);
                if (site != null)
                {
					UpdateSiteDefinistionValues(site, siteDefinitionToUpdate);
					updatedSites++;

					UpdateSitePermissions(site, siteDefinitionToUpdate);
				}
                else
                {
                    Logger.Warning($"Could not find site {siteDefinitionToUpdate.Name} or site already has site URL {siteDefinitionToUpdate.SiteUrl}.");
                    resultLog.AppendLine($"Could not find site {siteDefinitionToUpdate.Name} or site already has site URL {siteDefinitionToUpdate.SiteUrl}.<br />");
                }
            }

            return updatedSites;
        }

        private void UpdateSiteDefinistionValues(SiteDefinition site, EnvironmentSynchronizerSiteDefinition siteDefinitionToUpdate)
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
			Logger.Information($"Updated {siteDefinitionToUpdate.Name} to site URL {siteDefinitionToUpdate.SiteUrl} and {siteDefinitionToUpdate.Hosts.Count} hostnames.");
			resultLog.AppendLine($"Updated {siteDefinitionToUpdate.Name} to site URL {siteDefinitionToUpdate.SiteUrl} and {siteDefinitionToUpdate.Hosts.Count} hostnames.<br />");

		}

		private void UpdateSitePermissions(SiteDefinition site, EnvironmentSynchronizerSiteDefinition siteDefinitionToUpdate)
		{
			var siteStartPageContentLink = site.StartPage;

			if (siteDefinitionToUpdate.ForceLogin || siteDefinitionToUpdate.SetRoles.Any() || siteDefinitionToUpdate.RemoveRoles.Any() && siteStartPageContentLink != null)
			{
				IContentSecurityDescriptor securityDescriptor = (IContentSecurityDescriptor)_contentSecurityRepository.Get(siteStartPageContentLink).CreateWritableClone();

				if (securityDescriptor != null)
				{
					if (securityDescriptor.IsInherited)
					{
						securityDescriptor.IsInherited = false;
					}

					var existingEntries = ExisitngAce(securityDescriptor);

					if (siteDefinitionToUpdate.SetRoles.Any())
					{
						SetRoles(existingEntries, siteDefinitionToUpdate);
					}

					if (siteDefinitionToUpdate.RemoveRoles.Any())
					{
						RemoveRoles(existingEntries, siteDefinitionToUpdate);
					}

					if (siteDefinitionToUpdate.ForceLogin)
					{
						RemoveRole(existingEntries, new RemoveRoleDefinition { Name = "Everyone" } , siteDefinitionToUpdate.Name);
					}

					SetAce(securityDescriptor, existingEntries);

					_contentSecurityRepository.Save(siteStartPageContentLink, securityDescriptor, SecuritySaveType.Replace);
					_contentSecurityRepository.Save(siteStartPageContentLink, securityDescriptor, SecuritySaveType.ReplaceChildPermissions);

				} else {
					Logger.Error($"Could not get a security descriptor from site {site.Name} startpage.");
				}
			}

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

		private List<AccessControlEntry> ExisitngAce(IContentSecurityDescriptor securityDescriptor)
		{
			return securityDescriptor.Entries.Select(x => x).ToList();
		}

		private void SetAce(IContentSecurityDescriptor securityDescriptor, IEnumerable<AccessControlEntry> existingEntries)
		{
			securityDescriptor.Clear();
			foreach (var entry in existingEntries)
			{
				securityDescriptor.AddEntry(entry);
			}
		}

		private void SetRoles(List<AccessControlEntry> existingEntries, EnvironmentSynchronizerSiteDefinition siteDefinitionToUpdate)
		{
			foreach (var role in siteDefinitionToUpdate.SetRoles)
			{
				var existingRole = existingEntries.Where(x => x.Name == role.Name).FirstOrDefault();
				if (existingRole != null)
				{
					existingEntries.Remove(existingRole);
				}
				existingEntries.Add(new AccessControlEntry(role.Name, role.Access, SecurityEntityType.Role));
				Logger.Information($"Set AccessControlEntry {role.Name} AccessLevel.{role.Access} for site {siteDefinitionToUpdate.Name}.");
				resultLog.AppendLine($"Set AccessControlEntry {role.Name} AccessLevel.{role.Access} for site {siteDefinitionToUpdate.Name}.<br/>");
			}
		}

		private void RemoveRoles(List<AccessControlEntry> existingEntries, EnvironmentSynchronizerSiteDefinition siteDefinitionToUpdate)
		{
			foreach (var role in siteDefinitionToUpdate.RemoveRoles)
			{
				RemoveRole(existingEntries, role, siteDefinitionToUpdate.Name);
			}
		}
		private void RemoveRole(List<AccessControlEntry> existingEntries, RemoveRoleDefinition removeRoleDefinition, string siteName)
		{
			var existingRole = existingEntries.Where(x => x.Name == removeRoleDefinition.Name).FirstOrDefault();
			if (existingRole != null)
			{
				existingEntries.Remove(existingRole);
				Logger.Information($"Remove AccessControlEntry {removeRoleDefinition.Name} AccessLevel.{existingRole.Access} for site {siteName}.");
				resultLog.AppendLine($"Remove AccessControlEntry {removeRoleDefinition.Name} AccessLevel.{existingRole.Access} for site {siteName}.<br/>");
			}
		}

    }
}
