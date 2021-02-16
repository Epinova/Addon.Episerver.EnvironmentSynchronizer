using System.Linq;
using EPiServer.Data.Dynamic;
using EPiServer.Logging;
using EPiServer.ServiceLocation;

namespace Addon.Episerver.EnvironmentSynchronizer.DynamicData
{
	[ServiceConfiguration(ServiceType = typeof(IEnvironmentSynchronizationStore))]
	public class EnvironmentSynchronizationStore: IEnvironmentSynchronizationStore
	{
		private static readonly ILogger Logger = LogManager.GetLogger();
		private readonly DynamicDataStore _store;

		public EnvironmentSynchronizationStore()
		{
			_store = DynamicDataStoreFactory.Instance.GetStore(typeof(EnvironmentSynchronizationStamp));
			if (_store == null)
			{
				_store = DynamicDataStoreFactory.Instance.CreateStore(typeof(EnvironmentSynchronizationStamp));
				Logger.Information("Create data store for 'EnvironmentSynchronizationStamp'.");
			}
		}

		public EnvironmentSynchronizationStamp GetStamp()
		{
			var stamp = _store.Items<EnvironmentSynchronizationStamp>().FirstOrDefault();
			return stamp;
		}

		public void SetStamp(EnvironmentSynchronizationStamp stamp)
		{
			var existingStamp = _store.Items<EnvironmentSynchronizationStamp>().FirstOrDefault();
			if (existingStamp != null)
			{
				existingStamp.TimeStamp = stamp.TimeStamp;
				existingStamp.Environment = stamp.Environment;
				_store.Save(existingStamp);
			}
			else
			{
				_store.Save(stamp);
			}
			Logger.Information("Saved environment synchronization stamp to data store.");
		}
	}
}
