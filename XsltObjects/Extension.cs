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
using System.Xml;

namespace XsltObjects
{
    public class Extension
    {
        private string _ExtensionName;
        private string _ExtensionType;
        private NameValueCollection _ExtensionAttributes = new NameValueCollection();

        public Extension(XmlAttributeCollection Attributes)
        {
            _ExtensionName = Attributes["name"].Value;
            _ExtensionType = Attributes["type"].Value;

            foreach (XmlAttribute attribute in Attributes)
            {
                if (attribute.Name != "name" && attribute.Name != "type")
                {
                    _ExtensionAttributes.Add(attribute.Name, attribute.Value);
                }
            }
        }

        public string Name
        {
            get
            {
                return _ExtensionName;
            }
        }

        public string Type
        {
            get
            {
                return _ExtensionType;
            }
        }

        public NameValueCollection Attributes
        {
            get
            {
                return _ExtensionAttributes;
            }
        }
    }
}
