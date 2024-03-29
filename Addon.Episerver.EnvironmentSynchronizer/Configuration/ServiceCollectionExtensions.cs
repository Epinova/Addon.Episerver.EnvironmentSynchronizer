﻿using Addon.Episerver.EnvironmentSynchronizer.DynamicData;
using Microsoft.Extensions.DependencyInjection;
using Addon.Episerver.EnvironmentSynchronizer.InitializationModule;

namespace Addon.Episerver.EnvironmentSynchronizer.Configuration
{
	public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEnvironmentSynchronization(this IServiceCollection services)
        {
            services.AddSingleton<IConfigurationReader, ConfigurationReader>();
			services.AddTransient<IEnvironmentSynchronizationManager, EnvironmentSynchronizationManager>(); // Need to be Transient. Otherwise it will pile up result when run schedule jobs.
			services.AddTransient<IEnvironmentSynchronizationStore, EnvironmentSynchronizationStore>(); // Need to be Transient to be able to store information to database without thread problem.
			services.AddSingleton<IInitializationExecuter, InitializationExecuter>();
			services.AddOptions<EnvironmentSynchronizerOptions>().BindConfiguration(EnvironmentSynchronizerOptions.EnvironmentSynchronizer);

			return services;
        }
    }
}
