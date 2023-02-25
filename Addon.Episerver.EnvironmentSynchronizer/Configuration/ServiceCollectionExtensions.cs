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
            var envSyncOptions = configuration.GetSection(EnvironmentSynchronizerOptions.EnvironmentSynchronizer).Get<EnvironmentSynchronizerOptions>();

            try
            {
                services.AddSingleton(configuration.GetSection("EnvironmentSynchronizerOptions").Get<EnvironmentSynchronizerOptions>());
            }
            catch (ArgumentNullException argNullEx)
            {
                if (argNullEx.Message.Contains("Value cannot be null. (Parameter 'implementationInstance')"))
                {
                    Logger.Error("Addon.EpiServer.EnvironmentSynchronizer tried to load configuration from section EnvironmentSynchronization from appsettings.json looks like it is missing.", argNullEx);
                }
                throw;
            }

            try
			{
                services.AddSingleton<IConfigurationReader, ConfigurationReader>();
            } catch (Exception ex)
			{
                Logger.Error("AddEnvironmentSynchronization:AddSingleton<IConfigurationReader>", ex);
                throw;
            }
            try
            {
                services.AddSingleton<EnvironmentSynchronizationManager>();
            }
            catch (Exception ex)
            {
                Logger.Error("AddEnvironmentSynchronization:AddSingleton<EnvironmentSynchronizationManager>", ex);
                throw;
            }
            try
            {
                services.AddSingleton<EnvironmentSynchronizationStore>();
            }
            catch (Exception ex)
            {
                Logger.Error("AddEnvironmentSynchronization:AddSingleton<EnvironmentSynchronizationStore>", ex);
                throw;
            }

            
            

            return services;
        }
    }
}
