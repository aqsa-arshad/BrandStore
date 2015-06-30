// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront
{

    public partial class engine : SkinBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (AppLogic.AppConfigBool("GoNonSecureAgain"))
            {
                GoNonSecureAgain();
            }
            // set the Customer context, and set the SkinBase context, so meta tags t be set if they are not blank in the XmlPackage results
            Package1.SetContext = this;
            if (Package1.PackageName.Length == 0)
            {
                String PN = CommonLogic.QueryStringCanBeDangerousContent("PackageName");
                if (PN.Length == 0)
                {
                    PN = CommonLogic.QueryStringCanBeDangerousContent("XmlPackage");
                }
                if (PN.Length == 0)
                {
                    PN = CommonLogic.QueryStringCanBeDangerousContent("Package");
                }
                PN = PN.ToLowerInvariant();
                AppLogic.CheckForScriptTag(PN);
                if (PN.Length == 0)
                {
                    Package1.PackageName = "helloworld.xml.config";
                }
                else
                {
                    Package1.PackageName = PN;
                }
            }

            //Make sure the engine can even render this package
            XmlPackage2 engineCheck = new XmlPackage2(Package1.PackageName, ThisCustomer.SkinID);
            if (!engineCheck.AllowEngine)
                throw new Exception("This XmlPackage is not allowed to be run from the engine.  Set the package element's allowengine attribute to true to enable this package to run.");

            if (Package1.ContentType.Length != 0)
            {
                Page.Response.ContentType = Package1.ContentType;
            }
        }
    }
}
