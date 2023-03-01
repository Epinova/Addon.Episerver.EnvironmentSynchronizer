using Addon.Episerver.EnvironmentSynchronizer.Configuration;
using EPiServer.Core.Internal;
using EPiServer.DataAbstraction;
using EPiServer.Logging;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
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

                    if (siteDefinitionToUpdate.ForceLogin)
                    {
                        // Will remove Everyone user group access.
                        SetForceLogin(site, siteDefinitionToUpdate);
                    }
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

        private void SetForceLogin(SiteDefinition site, EnvironmentSynchronizerSiteDefinition siteDefinitionToUpdate)
		{
			var siteStartPageContentLink = site.StartPage;
			if (siteStartPageContentLink != null)
			{
				IContentSecurityDescriptor securityDescriptor = (IContentSecurityDescriptor)_contentSecurityRepository.Get(siteStartPageContentLink).CreateWritableClone();

                if (securityDescriptor != null)
                {
					if (securityDescriptor.IsInherited)
					{
						securityDescriptor.IsInherited = false;
					}

					var foundEveryoneRead = false;
					var existingEntries = new List<AccessControlEntry>();
					foreach (var entry in securityDescriptor.Entries)
					{
						Logger.Information($"Found AccessControlEntry {entry.Name}-{entry.Access} for site {siteDefinitionToUpdate.Name}.");
						if (entry.Name == "Everyone")
						{
							foundEveryoneRead = true;
						}
						else
						{
							existingEntries.Add(entry);
						}
					}

					securityDescriptor.Clear();

					foreach (var entry in existingEntries)
					{
						securityDescriptor.AddEntry(entry);
					}

					if (foundEveryoneRead)
					{
						Logger.Information($"Remove AccessControlEntry Everyone AccessLevel.Read for site {siteDefinitionToUpdate.Name}.");
						resultLog.AppendLine($"Remove AccessControlEntry Everyone AccessLevel.Read for site {siteDefinitionToUpdate.Name}.<br/>");
					}
					//else
					//{
					//	securityDescriptor.AddEntry(new AccessControlEntry("Everyone", AccessLevel.Read, SecurityEntityType.Role));

					//	Logger.Information($"Set AccessControlEntry Everyone-Read for site {siteDefinitionToUpdate.Name}.");
					//	resultLog.AppendLine($"Set AccessControlEntry Everyone-Read for site {siteDefinitionToUpdate.Name}.<br/>");
					//}

					_contentSecurityRepository.Save(siteStartPageContentLink, securityDescriptor, SecuritySaveType.Replace);
					_contentSecurityRepository.Save(siteStartPageContentLink, securityDescriptor, SecuritySaveType.ReplaceChildPermissions);

				}


				//securityDescriptor.Clear();

				//foreach (var role in siteDefinitionsToUpdate.Roles)

				//securityDescriptor.AddEntry(new AccessControlEntry("Administrators", AccessLevel.FullAccess, SecurityEntityType.Role));
				//securityDescriptor.AddEntry(new AccessControlEntry("WebAdmins", AccessLevel.FullAccess, SecurityEntityType.Role));
				//securityDescriptor.AddEntry(new AccessControlEntry("WebEditors", AccessLevel.Read | AccessLevel.Create | AccessLevel.Edit | AccessLevel.Delete | AccessLevel.Publish, SecurityEntityType.Role));

				//securityDescriptor.AddEntry(new AccessControlEntry("Administrators", AccessLevel.FullAccess, SecurityEntityType.Role));
				//securityDescriptor.AddEntry(new AccessControlEntry("WebAdmins", AccessLevel.FullAccess, SecurityEntityType.Role));
				//securityDescriptor.AddEntry(new AccessControlEntry("WebEditors", AccessLevel.Read | AccessLevel.Create | AccessLevel.Edit | AccessLevel.Delete | AccessLevel.Publish, SecurityEntityType.Role));
				//securityDescriptor.AddEntry(new AccessControlEntry("Everyone", AccessLevel.Read, SecurityEntityType.Role));

				//}
				//Logger.Information($"Updated {siteDefinitionToUpdate.Name} to AccessControl Administrators({AccessLevel.FullAccess}.");
				//resultLog.AppendLine($"Updated {siteDefinitionToUpdate.Name} to AccessControl Administrators({AccessLevel.FullAccess}.<br />");

			}
		}
    }
}
