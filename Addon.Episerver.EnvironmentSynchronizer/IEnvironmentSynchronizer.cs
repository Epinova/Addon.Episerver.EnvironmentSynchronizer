namespace Addon.Episerver.EnvironmentSynchronizer
{
    /// <summary>
    /// Used to implement an environment Synchronizer that can set a given environment into a known state, for instance after a database synchronization between environments.
    /// </summary>
    public interface IEnvironmentSynchronizer
    {
        /// <summary>
        /// Synchronize the current environment given the environment name.
        /// </summary>
        /// <param name="environmentName"></param>
        string Synchronize(string environmentName);
    }
}
