// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Web.Security;
using System.Data;
using System.Globalization;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront
{
	/// <summary>
	/// Summary description for signout.
	/// </summary>
	public partial class signout : System.Web.UI.Page
	{
		protected void Page_Load(object sender, System.EventArgs e)
		{
			Response.CacheControl="private";
			Response.Expires=0;
			Response.AddHeader("pragma", "no-cache");

			Title = AppLogic.AppConfig("StoreName") + " - Signout";

            String RedirectURL = CommonLogic.QueryStringCanBeDangerousContent("RedirectURL");
            if (RedirectURL.Length == 0)
            {
                RedirectURL = "default.aspx";
            }

			Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;
            if (ThisCustomer.IsAdminUser)
            {
                Security.LogEvent("Store Logout", "", ThisCustomer.CustomerID, ThisCustomer.CustomerID, Convert.ToInt32(ThisCustomer.CurrentSessionID));
            }

            ThisCustomer.Logout();

			FormsAuthentication.SignOut();
            
			Session.Clear();
			Session.Abandon();

            Response.AddHeader("REFRESH", "1; URL=" + RedirectURL);
		}
	}
}
