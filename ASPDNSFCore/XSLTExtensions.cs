// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Globalization;
using System.Web;
using System.Data;
using System.Collections;
using System.Configuration;
using AspDotNetStorefrontEncrypt;
using System.Xml.XPath;

using System.Collections.Generic;

namespace AspDotNetStorefrontCore
{
    class XSLTExtensions : XSLTExtensionBase
    {
        public XSLTExtensions(Customer cust, int SkinID)
            : base(cust, SkinID)
        {
        }
    }
}
