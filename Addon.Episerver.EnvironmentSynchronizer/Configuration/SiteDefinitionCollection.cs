using System.Configuration;

namespace Addon.Episerver.EnvironmentSynchronizer.Configuration
{
	public class SiteDefinitionCollection : ConfigurationElementCollection, System.Collections.ICollection
	{
		public override ConfigurationElementCollectionType CollectionType
		{
			get { return ConfigurationElementCollectionType.AddRemoveClearMap; }
		}

		protected override ConfigurationElement CreateNewElement()
		{
			return new SiteDefinitionElement();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((SiteDefinitionElement)element).Name;
		}

		public void AddElement(SiteDefinitionElement newElement)
		{
			BaseAdd(newElement);
		}

		public new SiteDefinitionElement this[string index]
		{
			get { return (SiteDefinitionElement)BaseGet(index); }
		}
	}
}