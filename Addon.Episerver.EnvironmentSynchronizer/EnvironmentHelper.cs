namespace Addon.Episerver.EnvironmentSynchronizer
{
    public class EnvironmentHelper
    {
        public static bool IsProductionEnvironment(string environmentName)
        {
            return string.Equals(environmentName, KnownEnvironmentNames.Production);
        }

        public static bool IsPreProductionEnvironment(string environmentName)
        {
            return string.Equals(environmentName, KnownEnvironmentNames.Preproduction);
        }

        public static bool IsIntegrationEnvironment(string environmentName)
        {
            return string.Equals(environmentName, KnownEnvironmentNames.Integration);
        }
    }
}
