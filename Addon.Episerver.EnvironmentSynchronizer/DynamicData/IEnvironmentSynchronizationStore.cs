namespace Addon.Episerver.EnvironmentSynchronizer.DynamicData
{
	public interface IEnvironmentSynchronizationStore
	{
		EnvironmentSynchronizationStamp GetStamp();

		void SetStamp(EnvironmentSynchronizationStamp stamp);
	}
}