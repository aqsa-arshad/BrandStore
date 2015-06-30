// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront
{
	/// <summary>
	/// Summary description for yahooindex.
	/// </summary>
	public partial class yahooindex : System.Web.UI.Page
	{
		protected void Page_Load(object sender, System.EventArgs e)
		{
			Response.CacheControl="private";
			Response.Expires=0;
			Response.AddHeader("pragma", "no-cache");

            Response.Write(AppLogic.CategoryStoreEntityHelper[0].GetEntityYahooSiteMap(0, Localization.GetDefaultLocale(), true, true));
            Response.Write(AppLogic.SectionStoreEntityHelper[0].GetEntityYahooSiteMap(0, Localization.GetDefaultLocale(), true, true));
            Response.Write(AppLogic.ManufacturerStoreEntityHelper[0].GetEntityYahooSiteMap(0, Localization.GetDefaultLocale(), true, true));
            Response.Write(AppLogic.DistributorStoreEntityHelper[0].GetEntityYahooSiteMap(0, Localization.GetDefaultLocale(), true, true));
        }

	}
}
