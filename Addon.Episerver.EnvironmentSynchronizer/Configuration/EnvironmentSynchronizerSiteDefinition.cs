using System.Collections.Generic;
using EPiServer.Security;
using EPiServer.Applications;

namespace Addon.Episerver.EnvironmentSynchronizer.Configuration
{
    public class EnvironmentSynchronizerSiteDefinition
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public IList<ApplicationHost> Hosts { get; set; }

        public bool? IsDefault { get; set; }

        public bool ForceLogin { get; set; }

        public IEnumerable<SetRoleDefinition> SetRoles { get; set; }

        public IEnumerable<RemoveRoleDefinition> RemoveRoles { get; set; }
    }

    public class SetRoleDefinition
    {
        public string Name { get; set; }

        public AccessLevel Access { get; set; }
    }

    public class RemoveRoleDefinition
    {
        public string Name { get; set; }
    }
}
