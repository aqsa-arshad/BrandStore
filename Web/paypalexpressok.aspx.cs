// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways;
using System;
using System.Web;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for paypalexpressok.
    /// </summary>
    public partial class paypalexpressok : System.Web.UI.Page
    {
        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;

            String PayPalToken = CommonLogic.QueryStringCanBeDangerousContent("token");
            if(PayPalToken == "")
                PayPalToken = CommonLogic.QueryStringCanBeDangerousContent("TOKEN");
            AppLogic.CheckForScriptTag(PayPalToken);
            
            String ECDetails = Gateway.GetExpressCheckoutDetails(PayPalToken, ThisCustomer.CustomerID);
            String payerId = String.Empty;

            if (String.IsNullOrEmpty(ECDetails))
            { // If nothing returned, abort the transaction.
                Response.Redirect(AppLogic.AppConfig("PayPal.Express.CancelURL"));
                return;
            }
            else if (ECDetails.Equals("AVSFAILED", StringComparison.OrdinalIgnoreCase))
            {
                //CancelURL is defaulted to shoppingcart.aspx?resetlinkback=1
                ErrorMessage errorUnconfirmedAddress = new ErrorMessage(HttpUtility.HtmlEncode("paypal.express.avsconfirmedaddress.error".StringResource()));
                Response.Redirect(AppLogic.AppConfig("PayPal.Express.CancelURL") + "&ErrorMsg=" + errorUnconfirmedAddress.MessageId);
                return;
            }
            else
            {
                payerId = ECDetails;
            }
           
            if (!ThisCustomer.IsRegistered && !ThisCustomer.IsOver13)
            { // Set the Over13Checked flag since PayPal is permitting the order process, so they don't get rejected.
                DB.ExecuteSQL("update Customer set Over13Checked=1 where CustomerID=" + ThisCustomer.CustomerID.ToString());
            }

            if (ThisCustomer.PrimaryBillingAddress.PaymentMethodLastUsed == AppLogic.ro_PMPayPalEmbeddedCheckout)
            {
                int OrderNumber = DB.GetSqlN("select MAX(OrderNumber) N from dbo.orders where CustomerID = " + ThisCustomer.CustomerID.ToString());
                Response.Redirect("fp-orderconfirmation.aspx?ordernumber=" + OrderNumber.ToString() + "&paymentmethod=PayPalEmbeddedCheckout");
                return;
            }

            ThisCustomer.ThisCustomerSession.SetVal("PayPalExpressToken", Security.MungeString(PayPalToken), DateTime.Now.AddHours(3));
            ThisCustomer.ThisCustomerSession.SetVal("PayPalExpressPayerID", Security.MungeString(payerId), DateTime.Now.AddHours(3));

            Address BillingAddress = new Address();
            BillingAddress.LoadByCustomer(ThisCustomer.CustomerID, ThisCustomer.PrimaryBillingAddressID, AddressTypes.Billing);
            if (BillingAddress.PaymentMethodLastUsed == AppLogic.ro_PMPayPalExpressMark)
            {
                String strnext = "checkoutreview.aspx?paymentmethod=" + Server.UrlEncode(AppLogic.ro_PMPayPalExpressMark);

                if (CommonLogic.QueryStringBool("BypassOrderReview"))
                {
                    strnext += "&useraction=commit";
                }

                ShoppingCart cart = new ShoppingCart(ThisCustomer.SkinID, ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);
                // re-validate all shipping info
                if (!cart.ShippingIsAllValid())
                {
                    strnext = "checkoutshipping.aspx";
                }

                Response.Redirect(strnext);
            }
            else
            {
                Response.Redirect("checkoutshipping.aspx");
            }
        }

    }
}
