using System.Configuration;

namespace Addon.Episerver.EnvironmentSynchronizer.Configuration
{
    public class SiteDefinitionElement : ConfigurationElement
    {
        [ConfigurationProperty("Id", IsRequired = false, IsKey = false)]
        [StringValidator(InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|\\")]
        public string Id
        {
            get => (string)this["Id"];
            set => this["Id"] = value;
        }

        [ConfigurationProperty("Name", IsRequired = true)]
        public string Name
        {
            get => (string)this["Name"];
            set => this["Name"] = value;
        }

        [ConfigurationProperty("SiteUrl", IsRequired = true)]
        public string SiteUrl
        {
	        get => (string)this["SiteUrl"];
	        set => this["SiteUrl"] = value;
        }

        [ConfigurationProperty("hosts", IsRequired = true)]
        [ConfigurationCollection(typeof(HostCollection), AddItemName = "host")]
        public HostCollection Hosts
        {
	        get => (HostCollection)base["hosts"];
        }
    }
}
