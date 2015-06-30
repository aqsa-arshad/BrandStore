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
	/// <summary>
	/// Summary description for recentadditions.
	/// </summary>
	public partial class recentadditions : SkinBase
	{
		protected void Page_Load(object sender, System.EventArgs e)
		{
            if (AppLogic.AppConfigBool("GoNonSecureAgain"))
            {
                GoNonSecureAgain();
            }

            // this may be overwridden by the XmlPackage below!
            SectionTitle = AppLogic.GetString("recentadditions.aspx.1", SkinID, ThisCustomer.LocaleSetting);

            // set the Customer context, and set the SkinBase context, so meta tags to be set if they are not blank in the XmlPackage results
            XmlPackage1.SetContext = this;
		}

	}
}
