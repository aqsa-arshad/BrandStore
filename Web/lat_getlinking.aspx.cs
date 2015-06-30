// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Data;
using System.Text;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront
{
	/// <summary>
	/// Summary description for lat_getlinking.
	/// </summary>
	public partial class lat_getlinking : SkinBase
	{
		protected void Page_Load(object sender, System.EventArgs e)
		{
			Response.CacheControl="private";
			Response.Expires=0;
			Response.AddHeader("pragma", "no-cache");
			Response.Redirect("lat_driver.aspx?topic=affiliate_linking");
		}
	}
}
