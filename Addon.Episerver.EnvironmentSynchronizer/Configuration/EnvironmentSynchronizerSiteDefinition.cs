using System;
using System.Collections.Generic;
using EPiServer.Security;
#if NET10_0_OR_GREATER
using EPiServer.Applications;
#else
using EPiServer.Web;
#endif

namespace Addon.Episerver.EnvironmentSynchronizer.Configuration
{
#if NET10_0_OR_GREATER
    public class EnvironmentSynchronizerSiteDefinition
    {
        public string Name { get; set; }

        public Uri SiteUrl { get; set; }

        public IList<ApplicationHost> Hosts { get; set; }

        public bool IsDefault { get; set; }
#else
	public class EnvironmentSynchronizerSiteDefinition : SiteDefinition
	{
#endif
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
