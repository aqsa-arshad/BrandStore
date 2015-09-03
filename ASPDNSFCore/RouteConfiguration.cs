// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Configuration;

namespace AspDotNetStorefrontCore
{
    /// <summary>
    /// Allows access to the <RouteTable> section in Web.Config
    /// </summary>
    public class RouteSection : ConfigurationSection
    {
        [ConfigurationProperty("routes", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(RouteElementCollection))]
        public RouteElementCollection Routes
        {
            get { return (RouteElementCollection) this["routes"]; }
        }
    }

    public class RouteElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return (string) this["name"]; }
            set { this["name"] = value; }
        }

        [ConfigurationProperty("url", IsRequired = true)]
        public string Url
        {
            get { return (string) this["url"]; }
            set { this["url"] = value; }
        }

        [ConfigurationProperty("virtualPath", IsRequired = true)]
        public string VirtualPath
        {
            get { return (string) this["virtualPath"]; }
            set { this["virtualPath"] = value; }
        }

        [ConfigurationProperty("checkPhysicalUrlAccess", IsRequired = true)]
        public bool CheckPhysicalUrlAccess
        {
            get { return (bool) this["checkPhysicalUrlAccess"]; }
            set { this["checkPhysicalUrlAccess"] = value; }
        }

		[ConfigurationProperty("entityType", IsRequired = false)]
		public string EntityType
		{
			get { return (string)this["entityType"]; }
			set { this["entityType"] = value; }
		}
    }

    public class RouteElementCollection : ConfigurationElementCollection
    {
        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.AddRemoveClearMap; }
        }

        public RouteElement this[int index]
        {
            get { return (RouteElement) BaseGet(index); }
            set
            {
                if (BaseGet(index) != null) BaseRemoveAt(index);
                BaseAdd(index, value);
            }
        }

        public void Add(RouteElement element)
        {
            BaseAdd(element);
        }

        public void Clear()
        {
            BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new RouteElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((RouteElement) element).Url;
        }

        public void Remove(RouteElement element)
        {
            BaseRemove(element.Url);
        }

        public void Remove(string url)
        {
            BaseRemove(url);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }
    }
}
