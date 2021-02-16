using System;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Addon.Episerver.EnvironmentSynchronizer.DynamicData;

namespace Addon.Episerver.EnvironmentSynchronizer
{
	public interface IEnvironmentSynchronizationManager
	{
		string Synchronize();

		string Synchronize(string environmentName);

		string GetEnvironmentName();
	}

	[ServiceConfiguration(ServiceType = typeof(IEnvironmentSynchronizationManager))]
    public class EnvironmentSynchronizationManager : IEnvironmentSynchronizationManager
    {
        private static readonly ILogger Logger = LogManager.GetLogger();
        private readonly IEnumerable<IEnvironmentSynchronizer> _environmentSynchronizers;
        private readonly IEnvironmentSynchronizationStore _environmentSynchronizationStore;

        public EnvironmentSynchronizationManager(
            IEnumerable<IEnvironmentSynchronizer> environmentSynchronizers, IEnvironmentSynchronizationStore environmentSynchronizationStore)
        {
            _environmentSynchronizers = environmentSynchronizers;
            _environmentSynchronizationStore = environmentSynchronizationStore;
        }

        public string Synchronize()
        {
	        var resultLog = new StringBuilder();
            string environmentName = GetEnvironmentName();

            resultLog.AppendLine(Synchronize(environmentName));

            return resultLog.ToString();
        }

        public string Synchronize(string environmentName)
        {
            var resultLog = new StringBuilder();
            Logger.Information($"Starting environment synchronization for environment named: {environmentName}");
            resultLog.AppendLine($"Starting environment synchronization for environment named: {environmentName}<br />");

            if (_environmentSynchronizers is null || !_environmentSynchronizers.Any())
            {
	            Logger.Information($"No synchronizers found.");
	            resultLog.AppendLine($"No synchronizers found.<br />");
            }

            foreach (var environmentSynchronizer in _environmentSynchronizers)
            {
	            resultLog.AppendLine(environmentSynchronizer.Synchronize(environmentName) + "<br />");
            }

            Logger.Information($"Finished environment synchronization for environment named: {environmentName}");

            var environmentSynchronizationStamp = new EnvironmentSynchronizationStamp
            {
                TimeStamp = DateTime.Now,
                Environment = environmentName
            };
            _environmentSynchronizationStore.SetStamp(environmentSynchronizationStamp);

            return resultLog.ToString();
        }

        public string GetEnvironmentName()
        {
	        ServiceLocator.Current.TryGetExistingInstance<IEnvironmentNameSource>(out var environmentNameSource);
	        var environmentName = environmentNameSource != null ? environmentNameSource.GetCurrentEnvironmentName() : string.Empty;

	        if (string.IsNullOrEmpty(environmentName))
	        {
                environmentName = ConfigurationManager.AppSettings["episerver:EnvironmentName"];
            }

	        return environmentName;
        }
    }
}
