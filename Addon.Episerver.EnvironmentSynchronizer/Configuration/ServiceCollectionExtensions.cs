using Addon.Episerver.EnvironmentSynchronizer.DynamicData;
using Microsoft.Extensions.DependencyInjection;
using Addon.Episerver.EnvironmentSynchronizer.InitializationModule;

namespace Addon.Episerver.EnvironmentSynchronizer.Configuration
{
	public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEnvironmentSynchronization(this IServiceCollection services)
        {
            services.AddSingleton<IConfigurationReader, ConfigurationReader>();
			services.AddSingleton<IEnvironmentSynchronizationManager, EnvironmentSynchronizationManager>();
			services.AddSingleton<IEnvironmentSynchronizationStore, EnvironmentSynchronizationStore>();
			services.AddSingleton<IInitializationExecuter, InitializationExecuter>();
			services.AddOptions<EnvironmentSynchronizerOptions>().BindConfiguration(EnvironmentSynchronizerOptions.EnvironmentSynchronizer);

			return services;
        }
    }
}
