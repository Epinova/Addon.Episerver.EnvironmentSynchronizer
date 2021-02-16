using System;
using System.Configuration;
using EPiServer.Web;

namespace Addon.Episerver.EnvironmentSynchronizer.Configuration
{
    public class HostElement : ConfigurationElement
    {
        [ConfigurationProperty("Name", IsRequired = true)]
        [StringValidator(InvalidCharacters = "~!@#$%^&()[]{}/;'\"|\\")]
        public string Name
        {
            get => (string)this["Name"];
            set => this["Name"] = value;
        }

        [ConfigurationProperty("UseSecureConnection", IsRequired = false)]
        public bool UseSecureConnection
        {
	        get => (bool)this["UseSecureConnection"];
	        set => this["UseSecureConnection"] = value;
        }

        [ConfigurationProperty("Type", DefaultValue = HostDefinitionType.Undefined, IsRequired = false)]
        public HostDefinitionType Type
        {
	        get => (HostDefinitionType) this["Type"];
	        set => this["Type"] = value;
        }

        [ConfigurationProperty("Language", IsRequired = false)]
        public string Language
        {
	        get => (string)this["Language"];
	        set => this["Language"] = value;
        }

    }
}
