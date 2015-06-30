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
using AspDotNetStorefrontEncrypt;
using System.Collections.Generic;
using AspDotNetStorefrontGateways.Processors;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for paypalexpressok.
    /// </summary>
    public partial class paypalembeddedcheckoutok : SkinBase
    {
        private int QSResultCode
        {
            get
            {
                int rcode = -1;
                if (Request.QueryString["result"] == null)
                    return rcode;

                if (!int.TryParse(Request.QueryString["result"], out rcode))
                    return -1;

                return rcode;
            }
        }
        private string QSResponseMessage
        {
            get
            {
                if (Request.QueryString["respmsg"] == null)
                    return "There was an error processing your payment. Please contact customer support.";

                AppLogic.CheckForScriptTag(Request.QueryString["respmsg"]);

                return Server.UrlDecode(Request.QueryString["respmsg"]);
            }
        }

        protected void Page_Load(object sender, System.EventArgs e)
        {
            Customer thisCustomer;
            int customerId = 0;
            String postData = String.IsNullOrEmpty(Request.Form.ToString()) ? Request.QueryString.ToString() : Request.Form.ToString();
            if (Request.Form["USER1"] != null && Request.Form["USER1"] != "" && int.TryParse(Request.Form["USER1"], out customerId)) //silent post
            {
                thisCustomer = new Customer(customerId, true);
                PayPalEmbeddedCheckoutCallBackProcessor processor = new PayPalEmbeddedCheckoutCallBackProcessor(PayFlowProController.GetParameterStringAsDictionary(postData, true), thisCustomer);
                string redirectPage = processor.ProcessCallBack();
            }
            else if (!String.IsNullOrEmpty(Request.Form.ToString())) //notification (ipn)
            {
            }
            else // customer returning to site
            {
                if (QSResultCode == 0)
                {
                    int OrderNumber = DB.GetSqlN("select MAX(OrderNumber) N from dbo.orders where CustomerID = " + ThisCustomer.CustomerID.ToString());
                    Response.Redirect("orderconfirmation.aspx?ordernumber=" + OrderNumber.ToString() + "&paymentmethod=PayPalEmbeddedCheckout", true);
                    return;
                }
                ErrorMessage er = new ErrorMessage(QSResponseMessage);
                ShoppingCart cart = new ShoppingCart(SkinID, ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);
                var checkoutController = CheckOutPageControllerFactory.CreateCheckOutPageController(ThisCustomer, cart);
                Response.Redirect(checkoutController.GetCheckoutPaymentPage() + "?ErrorMsg=" + er.MessageId, true);
            }
        }
    }
}
