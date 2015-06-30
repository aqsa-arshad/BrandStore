// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Data;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront
{
	/// <summary>
	/// Summary description for orderhistory.
	/// </summary>
	public partial class orderhistory : System.Web.UI.Page
	{
		
		protected void Page_Load(object sender, System.EventArgs e)
		{
			// this is an obsolete page, send to the new one:
			Response.Redirect("account.aspx");
		}

	}
}
