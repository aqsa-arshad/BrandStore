// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront
{

    public partial class manufacturers : SkinBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (AppLogic.AppConfigBool("GoNonSecureAgain"))
            {
                GoNonSecureAgain();
            }
            // set the Customer context, and set the SkinBase context, so meta tags t be set if they are not blank in the XmlPackage results
            Package1.SetContext = this;
        }
    }
}
