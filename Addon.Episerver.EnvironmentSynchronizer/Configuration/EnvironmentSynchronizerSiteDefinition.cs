using EPiServer.Web;

namespace Addon.Episerver.EnvironmentSynchronizer.Configuration
{
	public class EnvironmentSynchronizerSiteDefinition : SiteDefinition
	{
		public bool ForceLogin { get; set; }
		//public IList<EnvironmentSynchronizerRoleDefinition> Roles { get; set; }
	}

	//public class EnvironmentSynchronizerRoleDefinition
	//{
	//	public string Name { get; set;}

	//	//public AccessLevel Access { get; set;}
	//}
}
