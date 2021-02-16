using Addon.Episerver.EnvironmentSynchronizer.Models;

namespace Addon.Episerver.EnvironmentSynchronizer.Configuration
{
	public interface IConfigurationReader
	{
		SynchronizationData ReadConfiguration();
	}
}