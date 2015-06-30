// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Data;
using System.Globalization;
using System.Text;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront
{
	/// <summary>
	/// Summary description for reorder.
	/// </summary>
	public partial class reorder : System.Web.UI.Page
	{
        private Customer ThisCustomer
        {
            get { return Customer.Current; }
        }

		protected void Page_Load(object sender, System.EventArgs e)
		{
            // currently viewing user must be logged in to view receipts:
            if (!ThisCustomer.IsRegistered)
            {
                Response.Redirect("signin.aspx?returnurl=reorder.aspx?" + Server.UrlEncode(CommonLogic.ServerVariables("QUERY_STRING")));
            }

            this.Title = AppLogic.GetString("reorder.aspx.1", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);

            int OrderNumber = CommonLogic.QueryStringUSInt("OrderNumber");

            // are we allowed to view?
            // if currently logged in user is not the one who owns the order, and this is not an admin user who is logged in, reject the reorder:
            if (ThisCustomer.CustomerID != Order.GetOrderCustomerID(OrderNumber) && !ThisCustomer.IsAdminUser)
            {
                Response.Redirect(SE.MakeDriverLink("ordernotfound"));
            }

            StringBuilder output = new StringBuilder();

            if (OrderNumber == 0)
            {
                output.Append("<p>" + String.Format(AppLogic.GetString("reorder.aspx.2", ThisCustomer.SkinID, ThisCustomer.LocaleSetting), "account.aspx") + "</p>");
            }
            String StatusMsg = String.Empty;
            if (Order.BuildReOrder(null, ThisCustomer, OrderNumber, out StatusMsg))
            {
                Response.Redirect("shoppingcart.aspx");
            }
            else
            {
                output.Append("<p>" + AppLogic.GetString("reorder.aspx.6", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "</p>");
                output.Append("<p>Error: " + StatusMsg + "</p>");
                output.Append("<p>" + String.Format(AppLogic.GetString("reorder.aspx.2", ThisCustomer.SkinID, ThisCustomer.LocaleSetting), "account.aspx", AppLogic.GetString("AppConfig.CartPrompt", ThisCustomer.SkinID, ThisCustomer.LocaleSetting)) + "</p>");
            }

            litOutput.Text = output.ToString();
		}
	}
}
