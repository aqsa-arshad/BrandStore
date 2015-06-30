// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for receipt.
    /// </summary>
    public partial class receipt : System.Web.UI.Page
    {
        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            SkinBase.RequireSecurePage();

            int OrderNumber = CommonLogic.QueryStringUSInt("OrderNumber");
            int OrderCustomerID = Order.GetOrderCustomerID(OrderNumber);

            Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;// who is logged in now viewing this page:

            // currently viewing user must be logged in to view receipts:
            if (!ThisCustomer.IsRegistered)
            {
                Response.Redirect("signin.aspx?returnurl=receipt.aspx?" + Server.UrlEncode(CommonLogic.ServerVariables("QUERY_STRING")));
            }

            // are we allowed to view?
            // if currently logged in user is not the one who owns the order, and this is not an admin user who is logged in, reject the view:
            if (ThisCustomer.CustomerID != OrderCustomerID && !ThisCustomer.IsAdminUser)
            {
                Response.Redirect(SE.MakeDriverLink("ordernotfound"));
            }

            //For multi store checking
            //Determine if customer is allowed to view orders from other store.
            if (!ThisCustomer.IsAdminUser && AppLogic.StoreID() != AppLogic.GetOrdersStoreID(OrderNumber) && AppLogic.GlobalConfigBool("AllowCustomerFiltering") == true )
            {
                Response.Redirect(SE.MakeDriverLink("ordernotfound"));
            }

            Order o = new Order(OrderNumber, ThisCustomer.LocaleSetting);

			if (o.PaymentMethod != null && o.PaymentMethod.ToLower() == GatewayCheckoutByAmazon.CheckoutByAmazon.CBA_Gateway_Identifier.ToLower())
			{
				GatewayCheckoutByAmazon.CheckoutByAmazon checkoutByAmazon = new GatewayCheckoutByAmazon.CheckoutByAmazon();
				Response.Write(checkoutByAmazon.RenderOrderDetailWidget(o.OrderNumber));
			}
			else
				Response.Write(o.Receipt(ThisCustomer, false));
        }

    }
}
