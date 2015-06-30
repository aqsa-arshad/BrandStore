// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	/// <summary>
	/// Summary description for sitemap2.
	/// </summary>
    public partial class sitemap2 : AdminPageBase
	{

		protected void Page_Load(object sender, System.EventArgs e)
		{
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            System.Collections.Generic.Dictionary<string, EntityHelper> eh = AppLogic.MakeEntityHelpers();
            Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;
			SiteMap1.LoadXml(new SiteMapComponentArt(eh,1,ThisCustomer).Contents);
		}
	}
}
