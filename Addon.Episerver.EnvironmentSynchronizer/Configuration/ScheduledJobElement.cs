using System.Configuration;

namespace Addon.Episerver.EnvironmentSynchronizer.Configuration
{
    public class ScheduledJobElement : ConfigurationElement
    {
	    [ConfigurationProperty("Id", IsRequired = false)]
	    public string Id
	    {
		    get => (string)this["Id"];
		    set => this["Id"] = value;
	    }

	    [ConfigurationProperty("Name", IsRequired = true)]
        [StringValidator(InvalidCharacters = "~!@#$%^&()[]{}/;'\"|\\")]
        public string Name
        {
            get => (string)this["Name"];
            set => this["Name"] = value;
        }

        [ConfigurationProperty("IsEnabled", IsRequired = true)]
        public bool IsEnabled
        {
	        get => (bool)this["IsEnabled"];
	        set => this["IsEnabled"] = value;
        }
    }
}
