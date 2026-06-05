using Addon.Episerver.EnvironmentSynchronizer.Configuration;
using EPiServer.Core;
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
using EPiServer.Applications;

namespace Addon.Episerver.EnvironmentSynchronizer.Synchronizers.SiteDefinitions
{
    [ServiceConfiguration(typeof(IEnvironmentSynchronizer))]
    public class SiteDefinitionSynchronizer : IEnvironmentSynchronizer
    {
        private const string WebsiteDefinitionType = "application";
        private static readonly ILogger Logger = LogManager.GetLogger();
        private readonly IApplicationRepository _applicationRepository;
        private readonly IContentSecurityRepository _contentSecurityRepository;
        private readonly IConfigurationReader _configurationReader;
        private StringBuilder resultLog = new StringBuilder();

        public SiteDefinitionSynchronizer(
            IApplicationRepository applicationRepository,
            IContentSecurityRepository contentSecurityRepository,
            IConfigurationReader configurationReader)
        {
            Logger.Information("SiteDefinitionSynchronizer initialized.");
            _applicationRepository = applicationRepository;
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
            var existingSites = _applicationRepository.List().Where(application => application is IRoutableApplication);

            foreach (var siteDefinitionToUpdate in siteDefinitionsToUpdate)
            {
                Application site = GetExistingSiteDefinition(existingSites, siteDefinitionToUpdate);
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

        private void UpdateSiteDefinitionValues(Application site, EnvironmentSynchronizerSiteDefinition siteDefinitionToUpdate)
        {
            var writableApplication = site.CreateWritableClone();
            var writableSite = (IRoutableApplication)writableApplication;

            if (!string.IsNullOrEmpty(siteDefinitionToUpdate.Name) && 
                !string.Equals(writableApplication.DisplayName, siteDefinitionToUpdate.Name, StringComparison.Ordinal))
            {
                writableApplication.DisplayName = siteDefinitionToUpdate.Name;
            }

            var configuredHosts = (siteDefinitionToUpdate.Hosts ?? []).ToList();
            var supportedHosts = GetSupportedHosts(site, configuredHosts);

            if (configuredHosts.Count > 0 && supportedHosts.Count == 0)
            {
                Logger.Error($"Refusing to clear hosts for {WebsiteDefinitionType} {siteDefinitionToUpdate.Name}: configuration provided {configuredHosts.Count} host(s) but none are supported by {site.GetType().Name}. Existing hosts left unchanged.");
                resultLog.AppendLine($"Refusing to clear hosts for {WebsiteDefinitionType} {siteDefinitionToUpdate.Name}: none of the configured hosts are supported. Existing hosts left unchanged.<br />");
            }
            else
            {
                writableSite.Hosts.Clear();

                foreach (var host in supportedHosts)
                {
                    writableSite.Hosts.Add(host);
                }
            }
            _applicationRepository.SaveAsync(writableApplication, CancellationToken.None).GetAwaiter().GetResult();

            if (siteDefinitionToUpdate.IsDefault is not null &&
                ((IRoutableApplication)site).IsDefault != siteDefinitionToUpdate.IsDefault.Value)
            {
                _applicationRepository.MakeDefaultAsync(writableSite, siteDefinitionToUpdate.IsDefault.Value, CancellationToken.None).GetAwaiter().GetResult();
            }
            Logger.Information($"Updated {siteDefinitionToUpdate.Name} {WebsiteDefinitionType} with {supportedHosts.Count} hostnames.");
            resultLog.AppendLine($"Updated {siteDefinitionToUpdate.Name} {WebsiteDefinitionType} with {supportedHosts.Count} hostnames.<br />");

        }

        private List<ApplicationHost> GetSupportedHosts(Application site, IEnumerable<ApplicationHost> hosts)
        {
            var supportedHosts = new List<ApplicationHost>();

            foreach (var host in hosts ?? [])
            {
                if (IsSupportedHostType(site, host.Type))
                {
                    supportedHosts.Add(host);
                }
                else
                {
                    Logger.Warning($"Skipping host {host.Authority} with type {host.Type} because it is not supported by {site.GetType().Name} {site.Name}.");
                }
            }

            return supportedHosts;
        }

        private static bool IsSupportedHostType(Application site, ApplicationHostType hostType)
        {
            if (site is Website)
            {
                return hostType == ApplicationHostType.Primary ||
                       hostType == ApplicationHostType.Preview ||
                       hostType == ApplicationHostType.Media;
            }

            if (site is InProcessWebsite)
            {
                return hostType == ApplicationHostType.Primary ||
                       hostType == ApplicationHostType.Default ||
                       hostType == ApplicationHostType.Edit ||
                       hostType == ApplicationHostType.RedirectPermanent ||
                       hostType == ApplicationHostType.RedirectTemporary ||
                       hostType == ApplicationHostType.Media;
            }

            return false;
        }

        public void UpdateSitePermissions(Application website, EnvironmentSynchronizerSiteDefinition siteDefinitionToUpdate)
        {
            Logger.Debug($"UpdateSitePermissions");
            var siteStartPageContentLink = ((IRoutableApplication)website).EntryPoint;

            if (!ContentReference.IsNullOrEmpty(siteStartPageContentLink) &&
                (siteDefinitionToUpdate.ForceLogin ||
                 (siteDefinitionToUpdate.SetRoles != null && siteDefinitionToUpdate.SetRoles.Any()) ||
                 (siteDefinitionToUpdate.RemoveRoles != null && siteDefinitionToUpdate.RemoveRoles.Any())))
            {
                var existingDescriptor = _contentSecurityRepository.Get(siteStartPageContentLink);

                if (existingDescriptor != null)
                {
                    var securityDescriptor = (IContentSecurityDescriptor)existingDescriptor.CreateWritableClone();

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

        private Application GetExistingSiteDefinition(IEnumerable<Application> existingSites, EnvironmentSynchronizerSiteDefinition siteDefinitionToUpdate)
        {
            if (string.IsNullOrEmpty(siteDefinitionToUpdate.Id))
            {
                return existingSites.FirstOrDefault(application => string.Equals(application.DisplayName, siteDefinitionToUpdate.Name, StringComparison.OrdinalIgnoreCase));
            }

            return existingSites.FirstOrDefault(application => string.Equals(application.Name, siteDefinitionToUpdate.Id, StringComparison.Ordinal));
        }

        private List<AccessControlEntry> GetExistingAce(IContentSecurityDescriptor securityDescriptor)
        {
            return securityDescriptor.Entries.ToList();
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
                var existingRole = existingEntries.FirstOrDefault(x => x.Name == role.Name);
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
            var existingRole = existingEntries.FirstOrDefault(x => x.Name == removeRoleDefinition.Name);
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
