// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary descripption for NetaxeptCheckout.
    /// This page will process the transaction string that they have provided and redirect to their BBS UI Interface
    /// </summary>
    public partial class NetaxeptCheckout : System.Web.UI.Page
    {
        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            loadingshipping.ImageUrl = AppLogic.SkinImage("loadingshipping.gif");

            // We redirect the customer if he doesnt have customer record
            // this add a security, i8f try to access this page in the url
            if (!Customer.Current.HasCustomerRecord)
            {
                Response.Redirect("default.aspx");
            }

            AspDotNetStorefrontGateways.Processors.Netaxept netaxeptBBS = new AspDotNetStorefrontGateways.Processors.Netaxept();
            string setUp = string.Empty;

            ShoppingCart cart = new ShoppingCart(Customer.Current.SkinID, Customer.Current, CartTypeEnum.ShoppingCart, 0, false);
            
            // Lets get the order total amount
            Decimal CartTotal = cart.Total(true);
            Decimal OrderTotal = CartTotal - CommonLogic.IIF(cart.Coupon.CouponType == CouponTypeEnum.GiftCard, CommonLogic.IIF(CartTotal < cart.Coupon.DiscountAmount, CartTotal, cart.Coupon.DiscountAmount), 0);
           
            // The designated url for BBS Hosted UI
            string url = string.Empty;

            // Determine to go live or test.
            if (AppLogic.AppConfigBool("UseLiveTransactions"))
            {
                url = AppLogic.AppConfig("NETAXEPT.Live_UI"); // use live 
            }
            else
            {
                url = AppLogic.AppConfig("NETAXEPT.Test_UI"); // use test
            }

            // This is the return transaction string when calling their setup 
            // To start off a payment operation, a setup-call needs to be made
            // This is always the first operation made for a transaction.
            setUp = netaxeptBBS.GetTransactionString(cart, Customer.Current, OrderTotal);

            if (setUp.IndexOf(AppLogic.GetString("toc.aspx.6", Customer.Current.SkinID, Customer.Current.LocaleSetting)) != -1)
            {

                // This will show the setup error in the checkoutpayment page
                if (AppLogic.AppConfigBool("NETAXEPT.Error.Setup"))
                {
                    ErrorMessage err = new ErrorMessage(Server.HtmlEncode(setUp));

                    Response.Redirect("CheckoutPayment.aspx?nexaxepterror=" + err.MessageId);
                }
            }
            else
            {

                // We'll put the transaction string in the literal
                // so it will be included on form data
                ltlNexaxept.Text = setUp;
            }


            btnNetaxept.PostBackUrl = url;
            frmNetaxept.Action = url;
           

        }
    }
}
