using System;
using EPiServer.Logging;
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

    public class EnvironmentSynchronizationManager : IEnvironmentSynchronizationManager
    {
        private static readonly ILogger Logger = LogManager.GetLogger();
        private readonly IEnumerable<IEnvironmentSynchronizer> _environmentSynchronizers;
        private readonly IEnvironmentSynchronizationStore _environmentSynchronizationStore;
        private readonly IEnvironmentNameSource _environmentNameSource;

        public EnvironmentSynchronizationManager(
            IEnumerable<IEnvironmentSynchronizer> environmentSynchronizers,
            IEnvironmentSynchronizationStore environmentSynchronizationStore,
            IEnvironmentNameSource environmentNameSource = null)
        {
            _environmentSynchronizers = environmentSynchronizers;
            _environmentSynchronizationStore = environmentSynchronizationStore;
            _environmentNameSource = environmentNameSource;
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

			Logger.Information($"--------------------------------------------");
			foreach (var environmentSynchronizer in _environmentSynchronizers)
            {
				Logger.Information($"Synchronize {environmentSynchronizer.GetType()}.");
				resultLog.AppendLine(environmentSynchronizer.Synchronize(environmentName) + "<br />");
				Logger.Information($"--------------------------------------------");
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
	        var environmentName = _environmentNameSource != null ? _environmentNameSource.GetCurrentEnvironmentName() : string.Empty;

	        if (string.IsNullOrEmpty(environmentName))
	        {
                environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            }

	        return environmentName;
        }
    }
}
