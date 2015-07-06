// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;

namespace AspDotNetStorefrontAdmin
{
	/// <summary>
	/// Summary description for clearcookies.
	/// </summary>
    public partial class clearcookies : System.Web.UI.Page
	{
		protected void Page_Load(object sender, System.EventArgs e)
		{
			Response.CacheControl="private";
			Response.Expires=0;
			Response.AddHeader("pragma", "no-cache");

			for(int i = 0; i < Request.Cookies.Count; i++)
			{
				String cookie = Request.Cookies.Keys[i];
				Response.Write("Clearing cookie \"" + cookie + "\"...<br/>");
				Response.Cookies[cookie].Expires = System.DateTime.Now.AddDays(-1);
			}
		}
	}
}
