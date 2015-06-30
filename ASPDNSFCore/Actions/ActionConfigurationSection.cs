// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace AspDotNetStorefrontCore.Actions
{
    /// <summary>
    /// 
    /// </summary>
    public class ActionConfigurationSection : ConfigurationSection
    {
        #region Methods

        private static void Initialize()
        {
            ActionConfigurationSection config = Current;
        }

        /// <summary>
        /// 
        /// </summary>
        public static ActionConfigurationSection Current
        {
            get
            {
                try
                {
                    return System.Configuration.ConfigurationManager.GetSection("aspdnsf.action") as ActionConfigurationSection;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        [ConfigurationProperty("handlers", IsDefaultCollection = false)]
        public ActionHandlerCollection Handlers
        {

            get
            {
                ActionHandlerCollection handlerCollection = (ActionHandlerCollection)base["handlers"];
                return handlerCollection;
            }
        }


        #endregion

    }

    public class ActionHandlerCollection : ConfigurationElementCollection
    {
        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.AddRemoveClearMap;
            }
        }

        new public ActionHandlerConfiguration this[string name]
        {
            get
            {
                return (ActionHandlerConfiguration)BaseGet(name);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ActionHandlerConfiguration();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as ActionHandlerConfiguration).Name;
        }
    }

    public class ActionHandlerConfiguration : ConfigurationElement
    {
        #region Properties

        [ConfigurationProperty("name",
            IsRequired = true,
            IsKey = true)]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }

        [ConfigurationProperty("type",
            IsRequired = true,
            IsKey = false)]
        public string Type
        {
            get { return (string)this["type"]; }
            set { this["type"] = value; }
        }

        #endregion

    }
}
