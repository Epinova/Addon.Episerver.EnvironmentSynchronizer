using Addon.Episerver.EnvironmentSynchronizer.DynamicData;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using EPiServer.Logging;

namespace Addon.Episerver.EnvironmentSynchronizer.Configuration
{
    public static class ServiceCollectionExtensions
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        public static IServiceCollection AddEnvironmentSynchronization(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IConfigurationReader, ConfigurationReader>();
            services.AddSingleton<EnvironmentSynchronizationManager>();
            services.AddSingleton<EnvironmentSynchronizationStore>();

            try
            {
                services.AddSingleton(configuration.GetSection(EnvironmentSynchronizerOptions.EnvironmentSynchronizer).Get<EnvironmentSynchronizerOptions>());
            }
            catch (ArgumentNullException argNullEx)
            {
                if (argNullEx.Message.Contains("Value cannot be null. (Parameter 'implementationInstance')"))
				{
                    Logger.Error("Addon.EpiServer.EnvironmentSynchronizer tried to load configuration from section EnvironmentSynchronization from appsettings.json looks like it is missing.", argNullEx);
                }
                throw;
            }

            return services;
        }
    }
}
