// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways;

namespace AspDotNetStorefront
{
	/// <summary>
	/// Summary description for twocheckout_return.
	/// </summary>
	public partial class ogone_return : SkinBase
	{
		protected void Page_Load(object sender, System.EventArgs e)
		{
			// canceled or not approved:
            ErrorMessage err = new ErrorMessage(Server.HtmlEncode(AppLogic.GetString("ogone_return.aspx.1", SkinID, ThisCustomer.LocaleSetting)));
			Response.Redirect("shoppingcart.aspx?errormsg=" + err.MessageId);
		}
	}
}
