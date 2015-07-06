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
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	/// <summary>
	/// Summary description for signout.
	/// </summary>
	public partial class signout : System.Web.UI.Page
	{
        Customer ThisCustomer;
 
		protected void Page_Load(object sender, System.EventArgs e)
		{
            ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;
            Security.LogEvent("Admin Logout", "", ThisCustomer.CustomerID, ThisCustomer.CustomerID, Convert.ToInt32(ThisCustomer.CurrentSessionID));
            
            Address BillingAddress = new Address();
            // Ensure CC and CCVV2 is cleared upon customer logout.
            BillingAddress.LoadByCustomer(ThisCustomer.CustomerID, ThisCustomer.PrimaryBillingAddressID, AddressTypes.Billing);
            if (!ThisCustomer.MasterShouldWeStoreCreditCardInfo)
            {
                BillingAddress.ClearCCInfo();
            }
            BillingAddress.UpdateDB();
            AppLogic.ClearCardExtraCodeInSession(ThisCustomer); 
            

            ThisCustomer.ThisCustomerSession.Clear();

			Response.CacheControl="private";
			Response.Expires=0;
			Response.AddHeader("pragma", "no-cache");

            ThisCustomer.Logout();

			Session.Clear();
			Session.Abandon();
			FormsAuthentication.SignOut();

            Response.Write("<html>\n");
			Response.Write("<head>\n");
			Response.Write("<title>AspDotNetStorefront Admin - Signout</title>\n");
			Response.Write("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">\n");
            Response.Write("<link rel=\"stylesheet\" href=\""+ AppLogic.AdminLinkUrl("~/App_Themes/Admin_Default/style.css") +"\" type=\"text/css\">\n");
			Response.Write("</head>\n");
			Response.Write("<body bgcolor=\"#FFFFFF\" topmargin=\"0\" marginheight=\"0\" bottommargin=\"0\" marginwidth=\"0\" rightmargin=\"0\">\n");
			Response.Write("<table width=\"100%\" height=\"100%\" cellpadding=\"0\" cellspacing=\"0\">\n");
			Response.Write("<tr><td width=\"100%\" height=\"100%\" align=\"center\" valign=\"middle\">\n");
            Response.Write("<img src=\"" + AppLogic.LocateImageURL("~/App_Themes/Admin_Default/images/signout.jpg") + "\">\n");
			Response.Write("</td></tr>\n");
			Response.Write("</table>\n");
			Response.Write("<script type=\"text/javascript\">\n");
            Response.Write("top.location='" + AppLogic.AdminLinkUrl("default.aspx") + "';\n");
			Response.Write("</script>\n");
			Response.Write("</body>\n");
			Response.Write("</html>\n");

		}

	}
}
