// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Text;
using System.Data;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for secureprocessHSBC.
    /// 3-D Secure processing.
    /// </summary>
    public partial class secureprocessHSBC : System.Web.UI.Page
    {
        private void Page_Load(object sender, System.EventArgs e)
        {

            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            Response.Cache.SetAllowResponseInBrowserHistory(false);

            Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;
            ThisCustomer.RequireCustomerRecord();

            int CustomerID = ThisCustomer.CustomerID;

            String CcpaResultsCode = CommonLogic.FormCanBeDangerousContent("CcpaResultsCode");
            String XID = CommonLogic.FormCanBeDangerousContent("XID");
            String CAVV = CommonLogic.FormCanBeDangerousContent("CAVV");
            String ECI = CommonLogic.FormCanBeDangerousContent("ECI");
            String MerchantData = CommonLogic.FormCanBeDangerousContent("MD");
            String TransactionStatus = CommonLogic.FormCanBeDangerousContent("TransactionStatus");
            int OrderNumber = ThisCustomer.ThisCustomerSession.SessionUSInt("3Dsecure.OrderNumber");

            String ReturnURL = String.Empty;
            ErrorMessage err;

			if (ReturnURL.Length == 0 && MerchantData != ThisCustomer.ThisCustomerSession["3Dsecure.MD"])
            {
                err = new ErrorMessage(Server.HtmlEncode(AppLogic.GetString("secureprocess.aspx.1", 1, Localization.GetDefaultLocale())));
				ReturnURL = "checkoutpayment.aspx?error=1&errormsg=" + err.MessageId;
            }

            if (ReturnURL.Length == 0 && ShoppingCart.CartIsEmpty(CustomerID, CartTypeEnum.ShoppingCart))
            {
                ReturnURL = "ShoppingCart.aspx";
            }

            if (ReturnURL.Length == 0 && OrderNumber == 0)
            {
                err = new ErrorMessage(Server.HtmlEncode(AppLogic.GetString("secureprocess.aspx.1", 1, Localization.GetDefaultLocale())));
                ReturnURL = "checkoutpayment.aspx?error=1&errormsg=" + err.MessageId;
            }

            if (ReturnURL.Length == 0)
            {
                err = new ErrorMessage(Server.HtmlEncode(AppLogic.GetString("secureprocess.aspx.3", 1, Localization.GetDefaultLocale())));
                
                switch (CcpaResultsCode)
                {
                    case "5":
                        /*
                        The cardholder has failed payer authentication.         Do not submit the transaction - it will
                        Prompt the cardholder for an alternate card.            be rejected. Prompt for a new card.
                        */
                        ReturnURL = "checkoutpayment.aspx?error=1&errormsg=" + err.MessageId;
                        break;
                    case "6":
                        /*
                        Signature validation of the results from the ACS        Do not submit the transaction - it will
                        failed. Prompt the cardholder for an alternate card.    be rejected. Prompt for a new card.
                        */
                        ReturnURL = "checkoutpayment.aspx?error=1&errormsg=" + err.MessageId;
                        break;
                }
            }

            ShoppingCart cart = new ShoppingCart(1, ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);
            if (ReturnURL.Length == 0)
            {
                // We need CcpaResultsCode in ProcessCard
                ThisCustomer.ThisCustomerSession.SetVal("3DSecure.HSBCPASResult", CcpaResultsCode);

                String status = Gateway.MakeOrder(String.Empty, AppLogic.TransactionMode(), cart, OrderNumber, CAVV, ECI, XID, String.Empty);

                // The session may have changed in MakeOrder, so get the latest values from the DB
                CustomerSession cSession = new CustomerSession(ThisCustomer.CustomerID);

                if (status == AppLogic.ro_OK)
                {
                    if (cSession["3DSecure.LookupResult"].Length > 0)
                    {
                        // the data in this session variable will be encoded, so decode it before saving to the database
                        byte[] decodedBytes = Convert.FromBase64String(cSession["3DSecure.LookupResult"]);
                        String LookupResult = Encoding.UTF8.GetString(decodedBytes);
                        if (TransactionStatus.Length != 0)
                        {
                            LookupResult = "TransactionStatus=" + TransactionStatus + "\r\n" + LookupResult;
                        }
                        DB.ExecuteSQL("update orders set CardinalLookupResult=" + DB.SQuote(LookupResult) + " where OrderNumber=" + OrderNumber.ToString());
                        cSession["3DSecure.LookupResult"] = String.Empty;
                        // at this point we are done with the session altogether
                        CustomerSession.StaticClear(ThisCustomer.CustomerID);
                    }
                    ReturnURL = "orderconfirmation.aspx?ordernumber=" + OrderNumber.ToString() + "&paymentmethod=Credit+Card";
                }
                else
                {
                    err = new ErrorMessage(Server.HtmlEncode(status));
                    ReturnURL = "checkoutpayment.aspx?error=1&errormsg=" + err.MessageId;
                }
            }

            if (ReturnURL.Length == 0)
            {
                err = new ErrorMessage(Server.HtmlEncode(AppLogic.GetString("secureprocess.aspx.2", 1, Localization.GetDefaultLocale())));
                ReturnURL = "checkoutpayment.aspx?error=1&errormsg=" + err.MessageId;
            }

			ReturnURL = GetSmartOPCReturnURL(ReturnURL, ThisCustomer, cart);

            ThisCustomer.ThisCustomerSession["3DSecure.CustomerID"] = String.Empty;
            ThisCustomer.ThisCustomerSession["3DSecure.OrderNumber"] = String.Empty;
            ThisCustomer.ThisCustomerSession["3DSecure.XID"] = String.Empty;
            ThisCustomer.ThisCustomerSession["3DSecure.MD"] = String.Empty;
            ThisCustomer.ThisCustomerSession["3DSecure.HSBCPASResult"] = String.Empty;
            ThisCustomer.ThisCustomerSession["3DSecure.HSBCAmountRaw"] = String.Empty;
            ThisCustomer.ThisCustomerSession["3DSecure.HSBCAmount"] = String.Empty;

            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");
            Response.Write("<html><head><title>3-D Secure Process</title></head><body>");
            Response.Write("<script type=\"text/javascript\">\n");
            Response.Write("top.location='" + ReturnURL + "';\n");
            Response.Write("</SCRIPT>\n");
            Response.Write("<div align=\"center\">" + String.Format(AppLogic.GetString("secureprocess.aspx.6", 1, Localization.GetDefaultLocale()), ReturnURL) + "</div>");
            Response.Write("</body></html>");

        }

		private string GetSmartOPCReturnURL(string returnURL, Customer ThisCustomer, ShoppingCart Cart)
		{
			if (string.IsNullOrEmpty(returnURL))
				return returnURL;

			bool phoneCustomer = ((HttpContext.Current.Items["IsBeingImpersonated"] != null) &&
	((string)HttpContext.Current.Items["IsBeingImpersonated"] == "true"));

            var checkoutController = CheckOutPageControllerFactory.CreateCheckOutPageController(ThisCustomer, Cart);
			if (checkoutController.GetCheckoutType() == CheckOutType.SmartOPC)
			{
				// if we are using SmartOPC, and there is an error,
				// then return the user to shoppingcart.aspx so the checkout controller can make a decision on how to proceed.
				if (!phoneCustomer)
				{
					if (returnURL.IndexOf("errormsg") >= 0)
					{
						return returnURL.Replace("checkoutpayment.aspx", "shoppingcart.aspx");
					}
				}
			}

			return returnURL;
		}
    }
}
