// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using AspDotNetStorefrontCore;
using System.Linq;

namespace AspDotNetStorefront
{
	/// <summary>
	/// Summary description for sitemap2.
	/// </summary>
	public partial class sitemap2 : SkinBase
	{

		protected void Page_Load(object sender, System.EventArgs e)
		{
			if(AppLogic.AppConfigBool("GoNonSecureAgain"))
			{
				GoNonSecureAgain();
			}
			SectionTitle = AppLogic.GetString("sitemap.aspx.1",SkinID,ThisCustomer.LocaleSetting);
			//SiteMap1.LoadXml(new SiteMapComponentArt(base.EntityHelpers,SkinID,ThisCustomer).Contents);

            Boolean showCustomerService = AppLogic.IsAdminSite || AppLogic.AppConfigBool("SiteMap.ShowCustomerService");
            SiteMap1.LoadXml(new SiteMap(showCustomerService).Contents);
		}
	}
}
