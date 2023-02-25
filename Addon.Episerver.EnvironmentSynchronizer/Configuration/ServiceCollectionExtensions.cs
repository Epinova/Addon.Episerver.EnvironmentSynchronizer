using Addon.Episerver.EnvironmentSynchronizer.DynamicData;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Addon.Episerver.EnvironmentSynchronizer.Configuration
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEnvironmentSynchronization(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IConfigurationReader, ConfigurationReader>();
            services.AddSingleton<EnvironmentSynchronizationManager>();
            services.AddSingleton<EnvironmentSynchronizationStore>();
            services.AddSingleton(configuration.GetSection(EnvironmentSynchronizerOptions.EnvironmentSynchronizer).Get<EnvironmentSynchronizerOptions>());

            return services;
        }
    }
}
