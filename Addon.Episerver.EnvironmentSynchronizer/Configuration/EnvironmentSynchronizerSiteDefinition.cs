using EPiServer.Web;

namespace Addon.Episerver.EnvironmentSynchronizer.Configuration
{
	public class EnvironmentSynchronizerSiteDefinition : SiteDefinition
	{
		public bool ForceLogin { get; set; }
	}
}
