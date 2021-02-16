namespace Addon.Episerver.EnvironmentSynchronizer
{
    /// <summary>
    /// Implement this interface to handle how you resolve the name for a given environment.
    /// </summary>
    public interface IEnvironmentNameSource
    {
        /// <summary>
        /// The name of the current environment.
        /// </summary>
        /// <returns>The name of the current environment.</returns>
        string GetCurrentEnvironmentName();
    }
}
