// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Web;
using System.Web.Configuration;
using System.Xml;

namespace XsltObjects
{
    public class ExtensionConfiguration
    {
        private Hashtable _Extensions = new Hashtable();
        private string _DefaultExtension;

        public static ExtensionConfiguration GetExtensionConfiguration(string strExtension)
        {
            //return ((ExtensionConfiguration)(System.Web.Configuration.WebConfigurationManager.GetSection("system.web/" + strExtension)));
            return (ExtensionConfiguration)System.Web.Configuration.WebConfigurationManager.GetSection("system.web/" + strExtension);
        }

        internal void LoadValuesFromConfigurationXml(XmlNode node)
        {
            XmlAttributeCollection attributeCollection = node.Attributes;
            _DefaultExtension = attributeCollection["defaultExtension"].Value;
            foreach (XmlNode child in node.ChildNodes)
            {
                if ("extensions".Equals(child.Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    GetExtensions(child);
                }
            }
        }

        internal void GetExtensions(XmlNode node)
        {
            foreach (XmlNode extension in node.ChildNodes)
            {
                if ("add".Equals(extension.Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    Extensions.Add(extension.Attributes["name"].Value, new Extension(extension.Attributes));
                }
                else if ("remove".Equals(extension.Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    Extensions.Remove(extension.Attributes["name"].Value);
                }
                else if ("clear".Equals(extension.Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    Extensions.Clear();
                }
            }
        }

        public string DefaultExtension
        {
            get
            {
                return _DefaultExtension;
            }
        }

        public Hashtable Extensions
        {
            get
            {
                return _Extensions;
            }
        }
    }
}
