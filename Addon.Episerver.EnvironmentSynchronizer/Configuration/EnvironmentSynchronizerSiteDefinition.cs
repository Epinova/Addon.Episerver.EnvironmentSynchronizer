using EPiServer.Security;
using EPiServer.Web;
using System.Collections.Generic;

namespace Addon.Episerver.EnvironmentSynchronizer.Configuration
{
	public class EnvironmentSynchronizerSiteDefinition : SiteDefinition
	{
		public bool ForceLogin { get; set; }
		public IEnumerable<SetRoleDefinition> SetRoles { get; set; }
		public IEnumerable<RemoveRoleDefinition> RemoveRoles { get; set; }
	}

	public class SetRoleDefinition
	{
		public string Name { get; set; }

		public AccessLevel Access { get; set;}
	}

	public class RemoveRoleDefinition
	{
		public string Name { get; set; }
	}

}
