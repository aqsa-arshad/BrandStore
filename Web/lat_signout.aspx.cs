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
	/// Summary description for lat_signout.
	/// </summary>
	public partial class lat_signout : System.Web.UI.Page
	{
		protected void Page_Load(object sender, System.EventArgs e)
		{
            Customer ThisCustomer = Customer.Current;
            this.Title = AppLogic.GetString("AppConfig.AffiliateProgramName", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + " Signout";
            Profile.LATAffiliateID = string.Empty;
            lblSignoutSuccess.Text = AppLogic.GetString("AppConfig.AffiliateProgramName", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + " sign-out complete, please wait...";
			Response.AddHeader("REFRESH","1; URL=" + SE.MakeDriverLink("affiliate"));
		}

	}
}
