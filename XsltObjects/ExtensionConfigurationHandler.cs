// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Configuration;

namespace XsltObjects
{
    public class ExtensionConfigurationHandler : IConfigurationSectionHandler
    {
        #region IConfigurationSectionHandler Members

        public object Create(object parent, object configContext, System.Xml.XmlNode section)
        {
            ExtensionConfiguration objExtensionConfiguration = new ExtensionConfiguration();
            objExtensionConfiguration.LoadValuesFromConfigurationXml(section);
            return objExtensionConfiguration;
        }

        #endregion
    }
}
