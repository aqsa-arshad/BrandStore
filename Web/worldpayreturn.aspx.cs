// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Globalization;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for worldpayreturn.
    /// </summary>
    public partial class worldpayreturn : System.Web.UI.Page
    {

        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            bool ByPassWorldPay = CommonLogic.QueryStringBool("bypass");
            bool NoAutoRefresh = CommonLogic.QueryStringCanBeDangerousContent("refresh").Equals("FALSE", StringComparison.InvariantCultureIgnoreCase);

            String tmpS = CommonLogic.GetFormInput(false, "|");
            String tmpS2 = CommonLogic.GetQueryStringInput(false, "|"); // for debug!
            String TransStatus = CommonLogic.FormCanBeDangerousContent("TransStatus");
            int CustomerID = CommonLogic.FormNativeInt("CartID");

            if (ByPassWorldPay)
            {
                throw new ArgumentException("WorldPay ByPass No Longer Supported");
            }

            if (TransStatus.Length == 0 || CustomerID == 0)
            {
                throw new ArgumentException("WorldPay did NOT return any Form Post information. Please contact WorldPay!!");
            }
            if (TransStatus != "Y")
            {
                String ReturnURL = AppLogic.GetStoreHTTPLocation(true) + "shoppingcart.aspx";
                //if (AppLogic.AppConfigBool("WorldPay_OnCancelAutoRedirectToCart"))
                //{
                //    Response.AddHeader("REFRESH", "1; URL=" + ReturnURL);
                //}
                //Response.Write("<html><head><title>WorldPay Checkout Canceled - Please Wait</title></head><body>");
                Response.Write("<html><head><title>WorldPay Checkout Canceled - Please Wait</title>");
                if (AppLogic.AppConfigBool("WorldPay_OnCancelAutoRedirectToCart"))
                {
                    Response.Write("<meta http-equiv=\"refresh\" content=\"1;url=" + ReturnURL + "\">");
                }
                Response.Write("</head><body>");
                if (!AppLogic.AppConfigBool("WorldPay_OnCancelAutoRedirectToCart"))
                {
                    Topic t = new Topic("WorldPayCancel");
                    Response.Write(t.Contents.Replace("(!SKINID!)", CommonLogic.CookieUSInt("SkinID").ToString())); // only way to get skin is through users' cookie
                    Response.Write("<p align=\"left\"><b>" + AppLogic.GetString("worldpayreturn.aspx.1", 1, Localization.GetDefaultLocale()) + " <a href=\"" + ReturnURL + "\">" + String.Format(AppLogic.GetString("worldpayreturn.aspx.1", 1, Localization.GetDefaultLocale()), AppLogic.GetString("AppConfig.CartPrompt", 1, Localization.GetDefaultLocale()).ToLowerInvariant()) + "</a></b></p>");
                }
                if (AppLogic.AppConfigBool("WorldPay_OnCancelAutoRedirectToCart"))
                {
                    Response.Write("<p>If you are not redirected automatically within a few seconds. Please click <a href=\"" + ReturnURL + "\">here</a> </p>");
                }
                Response.Write("</body></html>");
            }
            else
            {
                Customer ThisCustomer = new Customer(CustomerID, true);

                // need these later in processcard, don't like passing via session, but it should be safe, and is easiest thing to do
                // worldpay structure requires this, so it can work like our other payment gateways
                ThisCustomer.ThisCustomerSession["WorldPay_CartID"] = CommonLogic.IIF(CommonLogic.FormCanBeDangerousContent("CartID").Length == 0, CustomerID.ToString(), CommonLogic.FormCanBeDangerousContent("CartID"));
                ThisCustomer.ThisCustomerSession["WorldPay_TransID"] = CommonLogic.FormCanBeDangerousContent("TransID");
                ThisCustomer.ThisCustomerSession["WorldPay_FuturePayID"] = CommonLogic.FormCanBeDangerousContent("FuturePayID");
                ThisCustomer.ThisCustomerSession["WorldPay_TransStatus"] = TransStatus;
                ThisCustomer.ThisCustomerSession["WorldPay_TransTime"] = CommonLogic.FormCanBeDangerousContent("TransTime");
                ThisCustomer.ThisCustomerSession["WorldPay_AuthAmount"] = CommonLogic.FormCanBeDangerousContent("AuthAmount");
                ThisCustomer.ThisCustomerSession["WorldPay_AuthCurrency"] = CommonLogic.FormCanBeDangerousContent("AuthCurrency");
                ThisCustomer.ThisCustomerSession["WorldPay_RawAuthMessage"] = CommonLogic.FormCanBeDangerousContent("RawAuthMessage");
                ThisCustomer.ThisCustomerSession["WorldPay_RawAuthCode"] = CommonLogic.FormCanBeDangerousContent("RawAuthCode");
                ThisCustomer.ThisCustomerSession["WorldPay_CallbackPW"] = CommonLogic.FormCanBeDangerousContent("CallbackPW");
                ThisCustomer.ThisCustomerSession["WorldPay_CardType"] = CommonLogic.FormCanBeDangerousContent("CardType");
                ThisCustomer.ThisCustomerSession["WorldPay_CountryMatch"] = CommonLogic.FormCanBeDangerousContent("CountryMatch");
                ThisCustomer.ThisCustomerSession["WorldPay_AVS"] = CommonLogic.FormCanBeDangerousContent("AVS");

                if (CustomerID != 0)
                {
                    // MakeOrder ALWAYS Returns OK, because WorldPay will never return without a C for cancel or Y for success, and the C was handled above
                    int OrderNumber = AppLogic.GetNextOrderNumber();

                    ShoppingCart cart = new ShoppingCart(1, ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);

                    Address UseBillingAddress = new Address();
                    UseBillingAddress.LoadByCustomer(ThisCustomer.CustomerID, ThisCustomer.PrimaryBillingAddressID, AddressTypes.Billing);

                    // make sure their addresss record is updated to match a worldpay checkout:
                    UseBillingAddress.ClearCCInfo();
                    UseBillingAddress.PaymentMethodLastUsed = AppLogic.ro_PMCreditCard;
                    UseBillingAddress.UpdateDB();

                    String status = Gateway.MakeOrder(String.Empty, AppLogic.TransactionMode(), cart, OrderNumber, String.Empty, String.Empty, String.Empty, String.Empty);

                    if (status.Equals(AppLogic.ro_OK, StringComparison.InvariantCultureIgnoreCase) == false)
                    {
                        throw new ArgumentException("Unknown WorldPay Callback Page Error: " + status);
                    }

                    String ReturnURL = AppLogic.GetStoreHTTPLocation(true) + "orderconfirmation.aspx?ordernumber=" + OrderNumber.ToString() + "&paymentmethod=Credit+Card";
                    Response.Write("<html><head><title>WorldPay Checkout Successful - Please Wait</title>");
                    if (!NoAutoRefresh)
                    {
                        Response.Write("<meta http-equiv=\"refresh\" content=\"1;url=" + ReturnURL + "\">");
                    }
                    Response.Write("</head><body>");
                    Topic t = new Topic("WorldPaySuccess", ThisCustomer.LocaleSetting, ThisCustomer.SkinID, null);
                    Response.Write(t.Contents.Replace("(!SKINID!)", ThisCustomer.SkinID.ToString()));
                    Response.Write("<p align=\"left\"><b>" + AppLogic.GetString("worldpayreturn.aspx.3", 1, Localization.GetDefaultLocale()) + " <a href=\"" + ReturnURL + "\">" + AppLogic.GetString("worldpayreturn.aspx.4", 1, Localization.GetDefaultLocale()) + "</a></b></p>");
                    Response.Write("</body></html>");
                }
                else
                {
                    Response.Write("<html><head><title>WorldPay Checkout Error</title></head><body>");
                    Response.Write(AppLogic.GetString("worldpayreturn.aspx.5", 1, Localization.GetDefaultLocale()));
                    Response.Write("</body></html>");
                }
            }
        }


    }
}
