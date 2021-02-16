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
            return string.Equals(environmentName, KnownEnvironmentNames.PreProduction);
        }

        public static bool IsIntegrationEnvironment(string environmentName)
        {
            return string.Equals(environmentName, KnownEnvironmentNames.Integration);
        }
    }
}
