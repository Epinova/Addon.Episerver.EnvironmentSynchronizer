using Addon.Episerver.EnvironmentSynchronizer.Configuration;
using EPiServer.DataAbstraction;
using EPiServer.Logging;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
#if NET10_0_OR_GREATER
using EPiServer.Applications;
using WebsiteDefinition = EPiServer.Applications.Application;
using WebsiteRepository = EPiServer.Applications.IApplicationRepository;
#else
using WebsiteDefinition = EPiServer.Web.SiteDefinition;
using WebsiteRepository = EPiServer.Web.ISiteDefinitionRepository;
#endif

namespace Addon.Episerver.EnvironmentSynchronizer.Synchronizers.SiteDefinitions
{
    [ServiceConfiguration(typeof(IEnvironmentSynchronizer))]
    public class SiteDefinitionSynchronizer : IEnvironmentSynchronizer
    {
#if NET10_0_OR_GREATER
        private const string WebsiteDefinitionType = "application";
#else
        private const string WebsiteDefinitionType = "site";
#endif
        private static readonly ILogger Logger = LogManager.GetLogger();
        private readonly WebsiteRepository _websiteRepository;
        private readonly IContentSecurityRepository _contentSecurityRepository;
        private readonly IConfigurationReader _configurationReader;
        private StringBuilder resultLog = new StringBuilder();

        public SiteDefinitionSynchronizer(
            WebsiteRepository websiteRepository,
            IContentSecurityRepository contentSecurityRepository,
            IConfigurationReader configurationReader)
        {
            Logger.Information("SiteDefinitionSynchronizer initialized.");
            _websiteRepository = websiteRepository;
            _contentSecurityRepository = contentSecurityRepository;
            _configurationReader = configurationReader;
        }

        public string Synchronize(string environmentName)
        {
            var syncConfiguration = _configurationReader.ReadConfiguration();

            if (syncConfiguration.SiteDefinitions == null || !syncConfiguration.SiteDefinitions.Any())
            {
                Logger.Information($"No {WebsiteDefinitionType} definitions found to synchronize.");
                resultLog.AppendLine($"No {WebsiteDefinitionType} definitions found to synchronize.<br />");
                return resultLog.ToString();
            }

            var stopwatch = Stopwatch.StartNew();

            try
            {
                var updatedSites = MergeSiteDefinitions(syncConfiguration.SiteDefinitions);

                Logger.Information($"Updated total of {updatedSites} {WebsiteDefinitionType}s.");
                resultLog.AppendLine($"Updated total of {updatedSites} {WebsiteDefinitionType}s.<br />");
            }
            catch (Exception ex)
            {
                Logger.Error($"An exception occured when trying to synchronize {WebsiteDefinitionType} definitions", ex);
                resultLog.AppendLine($"An exception occured when trying to synchronize {WebsiteDefinitionType} definitions: {ex.Message}<br />");
            }

            stopwatch.Stop();
            Logger.Information($"Synchronize {WebsiteDefinitionType} definitions took {stopwatch.ElapsedMilliseconds}ms.");

            return resultLog.ToString();
        }

        private int MergeSiteDefinitions(IEnumerable<EnvironmentSynchronizerSiteDefinition> siteDefinitionsToUpdate)
        {
            var updatedSites = 0;
#if NET10_0_OR_GREATER
            var existingSites = _websiteRepository.List().Where(application => application is IRoutableApplication);
#else
            var existingSites = _websiteRepository.List();
#endif

            foreach (var siteDefinitionToUpdate in siteDefinitionsToUpdate)
            {
                WebsiteDefinition site = GetExistingSiteDefinition(existingSites, siteDefinitionToUpdate);
                if (site != null)
                {
                    UpdateSiteDefinitionValues(site, siteDefinitionToUpdate);
                    updatedSites++;

                    UpdateSitePermissions(site, siteDefinitionToUpdate);
                }
                else
                {
                    Logger.Warning($"Could not find {WebsiteDefinitionType} {siteDefinitionToUpdate.Name}.");
                    resultLog.AppendLine($"Could not find {WebsiteDefinitionType} {siteDefinitionToUpdate.Name}.<br />");
                }
            }

            return updatedSites;
        }

        private void UpdateSiteDefinitionValues(WebsiteDefinition site, EnvironmentSynchronizerSiteDefinition siteDefinitionToUpdate)
        {
#if NET10_0_OR_GREATER
            var writableSite = (IRoutableApplication)site.CreateWritableClone();
            writableSite.Hosts.Clear();
            foreach (var host in siteDefinitionToUpdate.Hosts)
            {
                writableSite.Hosts.Add(host);
            }
            _websiteRepository.SaveAsync((Application)writableSite, CancellationToken.None).GetAwaiter().GetResult();

            if (((IRoutableApplication)site).IsDefault != siteDefinitionToUpdate.IsDefault)
            {
                _websiteRepository.MakeDefaultAsync(writableSite, siteDefinitionToUpdate.IsDefault, CancellationToken.None).GetAwaiter().GetResult();
            }
#else
			site = site.CreateWritableClone();
			if (!string.IsNullOrEmpty(siteDefinitionToUpdate.Name) && site.Name != siteDefinitionToUpdate.Name)
			{
				// Will set the name to the provided Name if Id exists and Name is specified and different from the found definition.
				site.Name = siteDefinitionToUpdate.Name;
			}
			site.SiteUrl = siteDefinitionToUpdate.SiteUrl;
			site.Hosts = siteDefinitionToUpdate.Hosts;

			_websiteRepository.Save(site);
#endif
            Logger.Information($"Updated {siteDefinitionToUpdate.Name} to {WebsiteDefinitionType} URL {siteDefinitionToUpdate.SiteUrl} and {siteDefinitionToUpdate.Hosts.Count} hostnames.");
            resultLog.AppendLine($"Updated {siteDefinitionToUpdate.Name} to {WebsiteDefinitionType} URL {siteDefinitionToUpdate.SiteUrl} and {siteDefinitionToUpdate.Hosts.Count} hostnames.<br />");

        }

        public void UpdateSitePermissions(WebsiteDefinition website, EnvironmentSynchronizerSiteDefinition siteDefinitionToUpdate)
        {
            Logger.Debug($"UpdateSitePermissions");
#if NET10_0_OR_GREATER
            var siteStartPageContentLink = ((IRoutableApplication)website).EntryPoint;
#else
			var siteStartPageContentLink = website.StartPage;
#endif

            if (siteStartPageContentLink != null &&
                (siteDefinitionToUpdate.ForceLogin ||
                 (siteDefinitionToUpdate.SetRoles != null && siteDefinitionToUpdate.SetRoles.Any()) ||
                 (siteDefinitionToUpdate.RemoveRoles != null && siteDefinitionToUpdate.RemoveRoles.Any())))
            {
                IContentSecurityDescriptor securityDescriptor = (IContentSecurityDescriptor)_contentSecurityRepository.Get(siteStartPageContentLink).CreateWritableClone();

                if (securityDescriptor != null)
                {
                    if (securityDescriptor.IsInherited)
                    {
                        securityDescriptor.IsInherited = false;
                    }

                    var existingEntries = GetExistingAce(securityDescriptor);

                    if (siteDefinitionToUpdate.SetRoles != null && siteDefinitionToUpdate.SetRoles.Any())
                    {
                        SetRoles(existingEntries, siteDefinitionToUpdate);
                    }

                    if (siteDefinitionToUpdate.RemoveRoles != null && siteDefinitionToUpdate.RemoveRoles.Any())
                    {
                        RemoveRoles(existingEntries, siteDefinitionToUpdate);
                    }

                    if (siteDefinitionToUpdate.ForceLogin)
                    {
                        Logger.Debug($"Start ForceLogin.");
                        RemoveRole(existingEntries, new RemoveRoleDefinition { Name = "Everyone" }, siteDefinitionToUpdate.Name);
                    }

                    SetAce(securityDescriptor, existingEntries);

                    _contentSecurityRepository.Save(siteStartPageContentLink, securityDescriptor, SecuritySaveType.Replace);
                    _contentSecurityRepository.Save(siteStartPageContentLink, securityDescriptor, SecuritySaveType.ReplaceChildPermissions);

                }
                else
                {
                    Logger.Error($"Could not get a security descriptor from {WebsiteDefinitionType} {website.Name} startpage.");
                }
            }
        }

        private WebsiteDefinition GetExistingSiteDefinition(IEnumerable<WebsiteDefinition> existingSites, EnvironmentSynchronizerSiteDefinition siteDefinitionToUpdate)
        {
#if NET10_0_OR_GREATER
            return existingSites.FirstOrDefault(application => application.Name == siteDefinitionToUpdate.Name);
#else
            WebsiteDefinition siteDefinition = null;
            if (siteDefinitionToUpdate.Id != Guid.Empty)
            {
                // Update the definition if it doesn't have the same value for SiteUrl.
                siteDefinition = existingSites.FirstOrDefault(s => s.Id == siteDefinitionToUpdate.Id);
            }
            else
            {
                // Update the definition if it doesn't have the same value for SiteUrl.
                siteDefinition = existingSites.FirstOrDefault(s => s.Name == siteDefinitionToUpdate.Name);
            }

            return siteDefinition;
#endif
        }

        private List<AccessControlEntry> GetExistingAce(IContentSecurityDescriptor securityDescriptor)
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
            Logger.Debug($"Start SetRoles.");
            foreach (var role in siteDefinitionToUpdate.SetRoles)
            {
                var existingRole = existingEntries.Where(x => x.Name == role.Name).FirstOrDefault();
                if (existingRole != null)
                {
                    existingEntries.Remove(existingRole);
                    Logger.Debug($"RemoveRole {existingRole.Name}.");
                }
                existingEntries.Add(new AccessControlEntry(role.Name, role.Access, SecurityEntityType.Role));
                Logger.Debug($"SetRole {role.Name} {role.Access}.");
                Logger.Information($"Set AccessControlEntry {role.Name} AccessLevel.{role.Access} for {WebsiteDefinitionType} {siteDefinitionToUpdate.Name}.");
                resultLog.AppendLine($"Set AccessControlEntry {role.Name} AccessLevel.{role.Access} for {WebsiteDefinitionType} {siteDefinitionToUpdate.Name}.<br/>");
            }
        }

        private void RemoveRoles(List<AccessControlEntry> existingEntries, EnvironmentSynchronizerSiteDefinition siteDefinitionToUpdate)
        {
            Logger.Debug($"Start RemoveRoles.");
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
                Logger.Debug($"RemoveRole {existingRole.Name}.");
                Logger.Information($"Remove AccessControlEntry {removeRoleDefinition.Name} AccessLevel.{existingRole.Access} for {WebsiteDefinitionType} {siteName}.");
                resultLog.AppendLine($"Remove AccessControlEntry {removeRoleDefinition.Name} AccessLevel.{existingRole.Access} for {WebsiteDefinitionType} {siteName}.<br/>");
            }
        }

    }
}
