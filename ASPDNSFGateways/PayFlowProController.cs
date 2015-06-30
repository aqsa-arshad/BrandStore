// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using AspDotNetStorefrontCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace AspDotNetStorefrontGateways.Processors
{
    /// <summary>
    /// Summary description for PayFlowPro.
    /// </summary>
    public static class PayFlowProController
    {
        public const String BN = "AspDotNetStoreFr_Cart_PRO2"; // Do not change this line or your paypal website calls may not work!

        public static PayPalEmbeddedCheckoutSecureTokenResponse GetFramedHostedCheckout(String returnUrl, String errorUrl, String cancelUrl, String notifyUrl, String silentPostUrl)
        {
            Customer thisCustomer = AspDotNetStorefrontCore.Customer.Current;
            ShoppingCart cart = new ShoppingCart(thisCustomer.SkinID, thisCustomer, CartTypeEnum.ShoppingCart, 0, false);
            return GetFramedHostedCheckout(cart, thisCustomer.PrimaryShippingAddress, returnUrl, errorUrl, cancelUrl, notifyUrl, silentPostUrl);
        }
        public static PayPalEmbeddedCheckoutSecureTokenResponse GetFramedHostedCheckout(ShoppingCart cart, Address ShippingAddress, String returnUrl, String errorUrl, String cancelUrl, String notifyUrl, String silentPostUrl)
        {
            StringBuilder transactionCommand = new StringBuilder(4096);

            // for eBay support with Auction Items
            Decimal OrderTotal = cart.Total(true);

            string sTrxType = "A";
            // eBay Auction Items require "Sale"
            if (AppLogic.TransactionModeIsAuthCapture() || AppLogic.AppConfigBool("PayPal.ForceCapture"))
                sTrxType = "S";

            transactionCommand.Append("TRXTYPE=" + sTrxType);
            transactionCommand.Append("&PWD=" + AppLogic.AppConfig("PayFlowPro.PWD"));
            transactionCommand.Append("&USER=" + AppLogic.AppConfig("PayFlowPro.USER"));
            transactionCommand.Append("&VENDOR=" + AppLogic.AppConfig("PayFlowPro.VENDOR"));
            transactionCommand.Append("&PARTNER=" + AppLogic.AppConfig("PayFlowPro.PARTNER"));

            //added for hosted checkout
            String secureTokenId = Guid.NewGuid().ToString().Replace("-", "");
            transactionCommand.Append("&SECURETOKENID=" + secureTokenId);

            if (cart.ContainsRecurring())
                transactionCommand.Append("&BILLINGTYPE=MerchantInitiatedBilling");

            transactionCommand.Append("&CREATESECURETOKEN=" + "Y");
            transactionCommand.Append("&DISABLERECEIPT=" + "TRUE");
            transactionCommand.Append("&BUTTONSOURCE=" + "AspDotNetStoreFront_Cart_PHS");
            transactionCommand.Append("&USER1=" + cart.ThisCustomer.CustomerID);
            transactionCommand.Append("&CUSTOM=" + cart.ThisCustomer.CustomerID);
            transactionCommand.Append("&COMMENT1=" + CleanForNVP(AppLogic.AppConfig("StoreName") + " Hosted Checkout"));
            //end added for hosted checkout

            transactionCommand.Append("&CURRENCY=" + AppLogic.AppConfig("Localization.StoreCurrency"));
            transactionCommand.Append("&AMT=" + Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal));
            transactionCommand.Append("&RETURNURL=" + returnUrl);
            transactionCommand.Append("&CANCELURL=" + cancelUrl);
            if (!string.IsNullOrEmpty(notifyUrl))
                transactionCommand.Append("&NOTIFYURL=" + notifyUrl);
            transactionCommand.Append("&ERRORURL=" + errorUrl);
            transactionCommand.Append("&SILENTPOSTURL=" + silentPostUrl);

            if (AppLogic.AppConfig("PayPal.Express.HeaderImage").Trim() != "")
            {
                transactionCommand.Append("&HDRIMG=" + AppLogic.AppConfig("PayPal.Express.HeaderImage").Trim());
            }

            if (AppLogic.AppConfig("PayPal.Express.PayFlowColor").Trim() != "")
            {
                transactionCommand.Append("&PAYFLOWCOLOUR=" + AppLogic.AppConfig("PayPal.Express.PayFlowColor").Trim());
            }

            if (ShippingAddress != null && ShippingAddress.EMail.Length > 0)
            {
                transactionCommand.Append("&EMAIL=" + ShippingAddress.EMail);
            }

            int TO = AppLogic.AppConfigUSInt("PayFlowPro.Timeout");
            if (TO == 0)
            {
                TO = 45;
            }

            String AuthServer = CommonLogic.IIF(AppLogic.AppConfigBool("UseLiveTransactions"), AppLogic.AppConfig("PayFlowPro.LiveURL"), AppLogic.AppConfig("PayFlowPro.TestURL"));


            String RequestID = System.Guid.NewGuid().ToString("N");
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] data = encoding.GetBytes(transactionCommand.ToString());
            String rawResponseString = String.Empty;

            int MaxTries = AppLogic.AppConfigUSInt("GatewayRetries") + 1;
            int CurrentTry = 0;
            bool CallSuccessful = false;
            do
            {
                CurrentTry++;
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(AuthServer);
                myRequest.Method = "POST";
                myRequest.ContentType = "text/namevalue";
                myRequest.Headers.Add("X-VPS-REQUEST-ID", RequestID);
                myRequest.Headers.Add("X-VPS-CLIENT-TIMEOUT", TO.ToString());
                myRequest.Headers.Add("X-VPS-VIT-CLIENT-CERTIFICATION-ID", "aspdotnetstorefront");
                myRequest.ContentLength = data.Length;
                try
                {
                    Stream newStream = myRequest.GetRequestStream();
                    // Send the data.
                    newStream.Write(data, 0, data.Length);
                    newStream.Close();
                    // get the response
                    WebResponse myResponse;
                    myResponse = myRequest.GetResponse();
                    using (StreamReader sr = new StreamReader(myResponse.GetResponseStream()))
                    {
                        rawResponseString = sr.ReadToEnd();
                        sr.Close();
                    }
                    myResponse.Close();
                    CallSuccessful = true;
                }
                catch
                {
                    CallSuccessful = false;
                }
            }
            while (!CallSuccessful && CurrentTry < MaxTries);

            PayPalEmbeddedCheckoutSecureTokenResponse response = new PayPalEmbeddedCheckoutSecureTokenResponse(rawResponseString);
            return response;

        }

		public static String StartEC(ShoppingCart cart, Address ShippingAddress, bool boolBypassOrderReview, IDictionary<string, string> checkoutOptions)
        {
            String sURL = String.Empty;

            StringBuilder transactionCommand = new StringBuilder(4096);

            // for eBay support with Auction Items
            bool bEBay = PayPalController.GetFirstAuctionSite(cart.CartItems).Equals("EBAY", StringComparison.InvariantCultureIgnoreCase);

            Decimal OrderTotal = cart.Total(true);

            string sTrxType = "A";
            // eBay Auction Items require "Sale"
            if (bEBay || AppLogic.TransactionModeIsAuthCapture() || AppLogic.AppConfigBool("PayPal.ForceCapture"))
            {
                sTrxType = "S";
            }

            transactionCommand.Append("TRXTYPE=" + sTrxType);
            transactionCommand.Append("&PWD=" + AppLogic.AppConfig("PayFlowPro.PWD"));
            transactionCommand.Append("&USER=" + AppLogic.AppConfig("PayFlowPro.USER"));
            transactionCommand.Append("&VENDOR=" + AppLogic.AppConfig("PayFlowPro.VENDOR"));
            transactionCommand.Append("&PARTNER=" + AppLogic.AppConfig("PayFlowPro.PARTNER"));
            transactionCommand.Append("&ACTION=S"); // S = set express checkout request
            transactionCommand.Append("&TENDER=P");
            if(cart.ContainsRecurring())
                transactionCommand.Append("&BILLINGTYPE=MerchantInitiatedBilling");

            if (bEBay)
            {
                transactionCommand.Append("&CHANNELTYPE=eBayItem");
            }

            transactionCommand.Append("&CURRENCY=" + AppLogic.AppConfig("Localization.StoreCurrency"));
            transactionCommand.Append("&AMT=" + Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal));

            if (AppLogic.AppConfigBool("PayPal.RequireConfirmedAddress"))
            {
                transactionCommand.Append("&REQCONFIRMSHIPPING=1");
            }
            else
            {
                transactionCommand.Append("&REQCONFIRMSHIPPING=0");

                if (ShippingAddress != null && !String.IsNullOrEmpty(ShippingAddress.City + ShippingAddress.State + ShippingAddress.Zip))
                { // if shipping address defined (not Anonymous)
                    transactionCommand.Append("&SHIPTOSTREET=" + CleanForNVP(ShippingAddress.Address1));
                    if (!String.IsNullOrEmpty(ShippingAddress.Address2))
                    {
                        transactionCommand.Append("&SHIPTOSTREET2=" + CleanForNVP(ShippingAddress.Address2));
                    }
                    transactionCommand.Append("&SHIPTOCITY=" + CleanForNVP(ShippingAddress.City));
                    transactionCommand.Append("&SHIPTOSTATE=" + CleanForNVP(ShippingAddress.State));
                    transactionCommand.Append("&SHIPTOCOUNTRY=" + CleanForNVP(AppLogic.GetCountryTwoLetterISOCode(ShippingAddress.Country)));
                    transactionCommand.Append("&SHIPTOZIP=" + CleanForNVP(ShippingAddress.Zip));
                    transactionCommand.Append("&ADDROVERRIDE=1");
                }
            }

            transactionCommand.Append("&RETURNURL=" + AppLogic.GetStoreHTTPLocation(true) + AppLogic.AppConfig("PayPal.Express.ReturnURL"));
            if (boolBypassOrderReview)
            {
                transactionCommand.Append("?BypassOrderReview=true");
            }
            transactionCommand.Append("&CANCELURL=" + AppLogic.GetStoreHTTPLocation(true) + AppLogic.AppConfig("PayPal.Express.CancelURL"));
            transactionCommand.Append("&LOCALECODE=" + AppLogic.AppConfig("PayPal.DefaultLocaleCode"));

            if (AppLogic.AppConfig("PayPal.Express.PageStyle").Trim() != "")
            {
                transactionCommand.Append("&PAGESTYLE=" + AppLogic.AppConfig("PayPal.Express.PageStyle").Trim());
            }

            if (AppLogic.AppConfig("PayPal.Express.HeaderImage").Trim() != "")
            {
                transactionCommand.Append("&HDRIMG=" + AppLogic.AppConfig("PayPal.Express.HeaderImage").Trim());
            }

            if (AppLogic.AppConfig("PayPal.Express.HeaderBackColor").Trim() != "")
            {
                transactionCommand.Append("&HDRBACKCOLOUR=" + AppLogic.AppConfig("PayPal.Express.HeaderBackColor").Trim());
            }

            if (AppLogic.AppConfig("PayPal.Express.HeaderBorderColor").Trim() != "")
            {
                transactionCommand.Append("&HDRBORDERCOLOR=" + AppLogic.AppConfig("PayPal.Express.HeaderBorderColor").Trim());
            }

            if (AppLogic.AppConfig("PayPal.Express.PayFlowColor").Trim() != "")
            {
                transactionCommand.Append("&PAYFLOWCOLOUR=" + AppLogic.AppConfig("PayPal.Express.PayFlowColor").Trim());
            }

            if (ShippingAddress != null && ShippingAddress.EMail.Length > 0)
            {
                transactionCommand.Append("&EMAIL=" + ShippingAddress.EMail);
            }


			if (checkoutOptions != null && checkoutOptions.ContainsKey("UserSelectedFundingSource"))
			{
				transactionCommand.Append("&USERSELECTEDFUNDINGSOURCE=BML");
				transactionCommand.Append("&SOLUTIONTYPE=SOLE");
				transactionCommand.Append("&VERSION=98");
			}


            int TO = AppLogic.AppConfigUSInt("PayFlowPro.Timeout");
            if (TO == 0)
            {
                TO = 45;
            }

            String AuthServer = CommonLogic.IIF(AppLogic.AppConfigBool("UseLiveTransactions"), AppLogic.AppConfig("PayFlowPro.LiveURL"), AppLogic.AppConfig("PayFlowPro.TestURL"));


            String RequestID = System.Guid.NewGuid().ToString("N");
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] data = encoding.GetBytes(transactionCommand.ToString());
            String rawResponseString = String.Empty;
            String result = String.Empty;

            int MaxTries = AppLogic.AppConfigUSInt("GatewayRetries") + 1;
            int CurrentTry = 0;
            bool CallSuccessful = false;
            do
            {
                CurrentTry++;
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(AuthServer);
                myRequest.Method = "POST";
                myRequest.ContentType = "text/namevalue";
                myRequest.Headers.Add("X-VPS-REQUEST-ID", RequestID);
                myRequest.Headers.Add("X-VPS-CLIENT-TIMEOUT", TO.ToString());
                myRequest.Headers.Add("X-VPS-VIT-CLIENT-CERTIFICATION-ID", "aspdotnetstorefront");
                myRequest.ContentLength = data.Length;
                try
                {
                    Stream newStream = myRequest.GetRequestStream();
                    // Send the data.
                    newStream.Write(data, 0, data.Length);
                    newStream.Close();
                    // get the response
                    WebResponse myResponse;
                    myResponse = myRequest.GetResponse();
                    using (StreamReader sr = new StreamReader(myResponse.GetResponseStream()))
                    {
                        rawResponseString = sr.ReadToEnd();
                        sr.Close();
                    }
                    myResponse.Close();
                    CallSuccessful = true;
                }
                catch
                {
                    CallSuccessful = false;
                }
            }
            while (!CallSuccessful && CurrentTry < MaxTries);

            result = "ERROR: Error Calling PayFlow Payment Gateway"; // This should get overwritten below.
            String curString = rawResponseString;

            String replyCode = String.Empty;
            String replyMsg = String.Empty;
            String replyToken = String.Empty;

            String[] statusArray = curString.Split('&');
            for (int i = statusArray.GetLowerBound(0); i <= statusArray.GetUpperBound(0); i++)
            {
                String[] lasKeyPair = statusArray[i].Split('=');
                switch (lasKeyPair[0].ToLowerInvariant())
                {
                    case "result":
                        replyCode = lasKeyPair[1];
                        break;
                    case "respmsg":
                        replyMsg = lasKeyPair[1];
                        break;
                    case "token":
                        replyToken = lasKeyPair[1];
                        break;
                }
            }

            if (replyCode == "0")
            {
                result = AppLogic.ro_OK;
            }
            else
            {
                result = "Error: [" + replyCode + "] " + replyMsg;
            }

            if (result == AppLogic.ro_OK)
            {
                if (AppLogic.AppConfigBool("UseLiveTransactions") == true)
                {
                    sURL = AppLogic.AppConfig("PayPal.Express.LiveURL");
                }
                else
                {
                    sURL = AppLogic.AppConfig("PayPal.Express.SandboxURL");
                }
                sURL += "?cmd=_express-checkout&token=" + replyToken;
                if (boolBypassOrderReview)
                {
                    sURL += "&useraction=commit";
                }

                // Set active payment method to PayPalExpress
                DB.ExecuteSQL(string.Format("UPDATE Address SET PaymentMethodLastUsed={0} WHERE AddressID={1}",
                    DB.SQuote(AppLogic.ro_PMPayPalExpress), cart.ThisCustomer.PrimaryBillingAddressID));
            }
            else
            {
                ErrorMessage e = new ErrorMessage(HttpContext.Current.Server.HtmlEncode(result));
                sURL = "shoppingcart.aspx?resetlinkback=1&errormsg=" + e.MessageId;
            }
            return sURL;
        }

        public static String GetECDetails(String PayPalToken, int CustomerID)
        {
            String ReturnValue = String.Empty;

            StringBuilder transactionCommand = new StringBuilder(4096);

            transactionCommand.Append("TRXTYPE=" + CommonLogic.IIF(AppLogic.TransactionModeIsAuthOnly(), "A", "S"));
            transactionCommand.Append("&PWD=" + AppLogic.AppConfig("PayFlowPro.PWD"));
            transactionCommand.Append("&USER=" + AppLogic.AppConfig("PayFlowPro.USER"));
            transactionCommand.Append("&VENDOR=" + AppLogic.AppConfig("PayFlowPro.VENDOR"));
            transactionCommand.Append("&PARTNER=" + AppLogic.AppConfig("PayFlowPro.PARTNER"));
            transactionCommand.Append("&ACTION=G"); // G = get express checkout details
            transactionCommand.Append("&TENDER=P");
            transactionCommand.Append("&TOKEN=" + PayPalToken);

            int TO = AppLogic.AppConfigUSInt("PayFlowPro.Timeout");
            if (TO == 0)
            {
                TO = 45;
            }

            String AuthServer = CommonLogic.IIF(AppLogic.AppConfigBool("UseLiveTransactions"), AppLogic.AppConfig("PayFlowPro.LiveURL"), AppLogic.AppConfig("PayFlowPro.TestURL"));


            String RequestID = System.Guid.NewGuid().ToString("N");
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] data = encoding.GetBytes(transactionCommand.ToString());
            String rawResponseString = String.Empty;

            int MaxTries = AppLogic.AppConfigUSInt("GatewayRetries") + 1;
            int CurrentTry = 0;
            bool CallSuccessful = false;
            do
            {
                CurrentTry++;
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(AuthServer);
                myRequest.Method = "POST";
                myRequest.ContentType = "text/namevalue";
                myRequest.Headers.Add("X-VPS-REQUEST-ID", RequestID);
                myRequest.Headers.Add("X-VPS-CLIENT-TIMEOUT", TO.ToString());
                myRequest.Headers.Add("X-VPS-VIT-CLIENT-CERTIFICATION-ID", "aspdotnetstorefront");
                myRequest.ContentLength = data.Length;
                try
                {
                    Stream newStream = myRequest.GetRequestStream();
                    // Send the data.
                    newStream.Write(data, 0, data.Length);
                    newStream.Close();
                    // get the response
                    WebResponse myResponse;
                    myResponse = myRequest.GetResponse();
                    using (StreamReader sr = new StreamReader(myResponse.GetResponseStream()))
                    {
                        rawResponseString = sr.ReadToEnd();
                        sr.Close();
                    }
                    myResponse.Close();
                    CallSuccessful = true;
                }
                catch
                {
                    CallSuccessful = false;
                }
            }
            while (!CallSuccessful && CurrentTry < MaxTries);

            String curString = rawResponseString + "&";

            String replyCode = String.Empty;

            String[] statusArray = curString.Split('&');
            for (int i = statusArray.GetLowerBound(0); i <= statusArray.GetUpperBound(0); i++)
            {
                String[] lasKeyPair = statusArray[i].Split('=');
                switch (lasKeyPair[0].ToLowerInvariant())
                {
                    case "result":
                        replyCode = lasKeyPair[1];
                        break;
                    case "respmsg":
                        break;
                }
            }

            if (replyCode == "0")
            {
                ReturnValue = CommonLogic.ExtractToken(curString, "PAYERID=", "&");
                if (String.IsNullOrEmpty(ReturnValue))
                {  // If we don't have a PayerID the transaction must be aborted.
                    return String.Empty;
                }

                string shippingCountry = CommonLogic.ExtractToken(curString, "SHIPTOCOUNTRY=", "&");

                if (shippingCountry.Length == 2)
                {
                    shippingCountry = AppLogic.GetCountryNameFromTwoLetterISOCode(shippingCountry);
                }

                Customer ThisCustomer = new Customer(CustomerID, true);
                if (!ThisCustomer.IsRegistered)
                {
                    ThisCustomer.UpdateCustomer(
                        /*CustomerLevelID*/ null,
                        /*EMail*/ CommonLogic.ExtractToken(curString, "EMAIL=", "&"),
                        /*SaltedAndHashedPassword*/ null,
                        /*SaltKey*/ null,
                        /*DateOfBirth*/ null,
                        /*Gender*/ null,
                        /*FirstName*/ CommonLogic.ExtractToken(curString, "FIRSTNAME=", "&"),
                        /*LastName*/ CommonLogic.ExtractToken(curString, "LASTNAME=", "&"),
                        /*Notes*/ null,
                        /*SkinID*/ null,
                        /*Phone*/ CommonLogic.IIF(CommonLogic.ExtractToken(curString, "PHONENUM=", "&").Length > 0, CommonLogic.ExtractToken(curString, "PHONENUM=", "&"), ""),
                        /*AffiliateID*/ null,
                        /*Referrer*/ null,
                        /*CouponCode*/ null,
                        /*OkToEmail*/ 0,
                        /*IsAdmin*/ null,
                        /*BillingEqualsShipping*/ null,
                        /*LastIPAddress*/ null,
                        /*OrderNotes*/ null,
                        /*SubscriptionExpiresOn*/ null,
                        /*RTShipRequest*/ null,
                        /*RTShipResponse*/ null,
                        /*OrderOptions*/ null,
                        /*LocaleSetting*/ null,
                        /*MicroPayBalance*/ null,
                        /*RecurringShippingMethodID*/ null,
                        /*RecurringShippingMethod*/ null,
                        /*BillingAddressID*/ null,
                        /*ShippingAddressID*/ null,
                        /*GiftRegistryGUID*/ null,
                        /*GiftRegistryIsAnonymous*/ null,
                        /*GiftRegistryAllowSearchByOthers*/ null,
                        /*GiftRegistryNickName*/ null,
                        /*GiftRegistryHideShippingAddresses*/ null,
                        /*CODCompanyCheckAllowed*/ null,
                        /*CODNet30Allowed*/ null,
                        /*ExtensionData*/ null,
                        /*FinalizationData*/ null,
                        /*Deleted*/ null,
                        /*Over13Checked*/ null,
                        /*CurrencySetting*/ null,
                        /*VATSetting*/ null,
                        /*VATRegistrationID*/ null,
                        /*StoreCCInDB*/ null,
                        /*IsRegistered*/ null,
                        /*LockedUntil*/ null,
                        /*AdminCanViewCC*/ null,
                        /*BadLogin*/ null,
                        /*Active*/ null,
                        /*PwdChangeRequired*/ null,
                        /*RegisterDate*/ null,
                        /*StoreId*/null
                    );

                    String PM = AppLogic.ro_PMPayPalExpress;

                    if (ThisCustomer.PrimaryBillingAddressID > 0)
                    {
                        Address UseBillingAddress = new Address();
                        UseBillingAddress.LoadByCustomer(ThisCustomer.CustomerID, ThisCustomer.PrimaryBillingAddressID, AddressTypes.Billing);
                        if (UseBillingAddress.PaymentMethodLastUsed != AppLogic.ro_PMPayPalExpress
                            && UseBillingAddress.PaymentMethodLastUsed != AppLogic.ro_PMPayPalExpressMark)
                        {
                            PM = AppLogic.ro_PMPayPalExpress;
                        }
                        else
                        {
                            PM = UseBillingAddress.PaymentMethodLastUsed;
                        }
                    }

                    // Anonymous Paypal Express Checkout order, use Paypal's address
                    Address ShippingAddress = new Address();
                    ShippingAddress.CustomerID = CustomerID;
                    ShippingAddress.PaymentMethodLastUsed = PM;

                    ShippingAddress.FirstName = CommonLogic.ExtractToken(curString, "FIRSTNAME=", "&");
                    ShippingAddress.LastName = CommonLogic.ExtractToken(curString, "LASTNAME=", "&");
                    ShippingAddress.Company = CommonLogic.ExtractToken(curString, "BUSINESS=", "&");
                    ShippingAddress.Address1 = CommonLogic.ExtractToken(curString, "SHIPTOSTREET=", "&");
                    ShippingAddress.Address2 = CommonLogic.ExtractToken(curString, "SHIPTOSTREET2=", "&");
                    ShippingAddress.Phone = CommonLogic.IIF(CommonLogic.ExtractToken(curString, "PHONENUM=", "&").Length > 0, CommonLogic.ExtractToken(curString, "PHONENUM=", "&"), "");
                    ShippingAddress.City = CommonLogic.ExtractToken(curString, "SHIPTOCITY=", "&");
                    ShippingAddress.State = CommonLogic.ExtractToken(curString, "SHIPTOSTATE=", "&");
                    ShippingAddress.Zip = CommonLogic.ExtractToken(curString, "SHIPTOZIP=", "&");
                    ShippingAddress.Country = shippingCountry;
                    ShippingAddress.InsertDB();

                    ShippingAddress.MakeCustomersPrimaryAddress(AddressTypes.Shipping);
                    ShippingAddress.MakeCustomersPrimaryAddress(AddressTypes.Billing);

                }
                else
                { // A registered and logged in Customer

                    Address UseBillingAddress = new Address();
                    UseBillingAddress.LoadByCustomer(ThisCustomer.CustomerID, ThisCustomer.PrimaryBillingAddressID, AddressTypes.Billing);
                    UseBillingAddress.ClearCCInfo();
                    if (UseBillingAddress.PaymentMethodLastUsed != AppLogic.ro_PMPayPalExpress
                        && UseBillingAddress.PaymentMethodLastUsed != AppLogic.ro_PMPayPalExpressMark
                        && UseBillingAddress.PaymentMethodLastUsed != AppLogic.ro_PMPayPalEmbeddedCheckout
                    )
                    {
                        UseBillingAddress.PaymentMethodLastUsed = AppLogic.ro_PMPayPalExpress;
                        if (!ThisCustomer.MasterShouldWeStoreCreditCardInfo)
                        {
                            UseBillingAddress.ClearCCInfo();
                        }
                        UseBillingAddress.UpdateDB();
                    }

                    string sql = String.Format("select top 1 AddressID as N from Address where Address1={0} and Address2={1} and City={2} and State={3} and Zip={4} and Country={5} and CustomerID={6}",
                        DB.SQuote(CommonLogic.ExtractToken(curString, "SHIPTOSTREET=", "&")), DB.SQuote(CommonLogic.ExtractToken(curString, "SHIPTOSTREET2=", "&")), DB.SQuote(CommonLogic.ExtractToken(curString, "SHIPTOCITY=", "&")), DB.SQuote(CommonLogic.ExtractToken(curString, "SHIPTOSTATE=", "&")),
                        DB.SQuote(CommonLogic.ExtractToken(curString, "SHIPTOZIP=", "&")), DB.SQuote(shippingCountry), CustomerID);
                    int ExistingAddressID = DB.GetSqlN(sql);

                    Address ShippingAddress = new Address();

                    if (ExistingAddressID == 0 || ThisCustomer.PrimaryShippingAddressID != ExistingAddressID)
                    {
                        string note = "Note: Customer selected Ship-To address at PayPal.com";
                        string ordernote = DB.GetSqlS("select OrderNotes S from Customer where CustomerID=" + ThisCustomer.CustomerID.ToString());
                        if (!ordernote.Contains(note))
                        {
                            ordernote += System.Environment.NewLine + note;
                            DB.ExecuteSQL("update Customer set OrderNotes=" + DB.SQuote(ordernote) + " where CustomerID=" + ThisCustomer.CustomerID.ToString());
                        }
                    }

                    if (ExistingAddressID == 0)
                    { // Does not exist
                        ShippingAddress.CustomerID = CustomerID;
                        if (ShippingAddress.PaymentMethodLastUsed != AppLogic.ro_PMPayPal && ShippingAddress.PaymentMethodLastUsed != AppLogic.ro_PMPayPalEmbeddedCheckout)
                        {
                            try
                            {
                                AppLogic.ValidatePM(AppLogic.ro_PMPayPal);
                                ShippingAddress.PaymentMethodLastUsed = AppLogic.ro_PMPayPal;
                            }
                            catch (Exception)
                            {
                                AppLogic.ValidatePM(AppLogic.ro_PMPayPalEmbeddedCheckout);
                                ShippingAddress.PaymentMethodLastUsed = AppLogic.ro_PMPayPalEmbeddedCheckout;
                            }
                        }
                        ShippingAddress.FirstName = CommonLogic.ExtractToken(curString, "FIRSTNAME=", "&");
                        ShippingAddress.LastName = CommonLogic.ExtractToken(curString, "LASTNAME=", "&");
                        ShippingAddress.Address1 = CommonLogic.ExtractToken(curString, "SHIPTOSTREET=", "&");
                        ShippingAddress.Address2 = CommonLogic.ExtractToken(curString, "SHIPTOSTREET2=", "&");
                        ShippingAddress.Phone = CommonLogic.IIF(CommonLogic.ExtractToken(curString, "PHONENUM=", "&").Length > 0, CommonLogic.ExtractToken(curString, "PHONENUM=", "&"), "");
                        ShippingAddress.City = CommonLogic.ExtractToken(curString, "SHIPTOCITY=", "&");
                        ShippingAddress.State = CommonLogic.ExtractToken(curString, "SHIPTOSTATE=", "&");
                        ShippingAddress.Zip = CommonLogic.ExtractToken(curString, "SHIPTOZIP=", "&");
                        ShippingAddress.Country = shippingCountry;
                        ShippingAddress.InsertDB();

                        ShippingAddress.MakeCustomersPrimaryAddress(AddressTypes.Shipping);
                    }
                    else
                    { // Exists already
                        ShippingAddress.LoadFromDB(ExistingAddressID);
                        ShippingAddress.MakeCustomersPrimaryAddress(AddressTypes.Shipping);
                    }
                }
            }
            return ReturnValue;
        }

        public static String ProcessEC(ShoppingCart cart, decimal OrderTotal, int OrderNumber, String PayPalToken, String PayerID, String TransactionMode, out String AuthorizationResult, out String AuthorizationTransID)
        {
            String result = String.Empty;

            AuthorizationResult = String.Empty;
            AuthorizationTransID = String.Empty;

            StringBuilder transactionCommand = new StringBuilder(4096);

            bool bEBay = PayPalController.GetFirstAuctionSite(cart.CartItems).Equals("EBAY", StringComparison.InvariantCultureIgnoreCase);

            string sTrxType = "A";
            // Ebay Auction requires Sale, not Auth
            if (bEBay || TransactionMode == AppLogic.ro_TXModeAuthCapture || AppLogic.AppConfigBool("PayPal.ForceCapture"))
            {
                sTrxType = "S";
            }

            transactionCommand.Append("TRXTYPE=" + sTrxType);
            transactionCommand.Append("&PWD=" + AppLogic.AppConfig("PayFlowPro.PWD"));
            transactionCommand.Append("&USER=" + AppLogic.AppConfig("PayFlowPro.USER"));
            transactionCommand.Append("&VENDOR=" + AppLogic.AppConfig("PayFlowPro.VENDOR"));
            transactionCommand.Append("&PARTNER=" + AppLogic.AppConfig("PayFlowPro.PARTNER"));

            transactionCommand.Append("&ACTION=D"); // D = do express checkout request
            transactionCommand.Append("&TENDER=P");

            transactionCommand.Append("&TOKEN=" + PayPalToken);
            transactionCommand.Append("&PAYERID=" + PayerID);

            transactionCommand.Append("&INVNUM=" + OrderNumber.ToString());
            transactionCommand.Append("&CUSTOM=" + cart.ThisCustomer.CustomerID.ToString());

            transactionCommand.Append("&NOTIFYURL=" + AppLogic.GetStoreHTTPLocation(true) + AppLogic.AppConfig("PayPal.NotificationURL"));

            if (AppLogic.AppConfig("PayFlowPro.PARTNER").Equals("paypaluk", StringComparison.InvariantCultureIgnoreCase))
            {
                transactionCommand.Append("&BUTTONSOURCE=" + PayFlowProController.BN + "EC_UK");
            }
            else if (AppLogic.AppConfig("PayFlowPro.PARTNER").Equals("paypal", StringComparison.InvariantCultureIgnoreCase))
            {
                transactionCommand.Append("&BUTTONSOURCE=" + PayFlowProController.BN + "EC_US");
            }
            else
            {
                transactionCommand.Append("&BUTTONSOURCE=" + PayFlowProController.BN + "EC");
            }

            if (bEBay)
            {
                PayPalItemList ppCart = new PayPalItemList(cart, true);
                decimal ppTotal = 0.0M;

                for (int x = 0; x < ppCart.Count; x++)
                {
                    PayPalItem ppItem = ppCart.Item(x);

                    transactionCommand.Append("&L_AMT" + (x + 1).ToString() + "=" + Localization.CurrencyStringForGatewayWithoutExchangeRate(ppItem.Amount));
                    transactionCommand.Append("&L_QTY" + (x + 1).ToString() + "=" + ppItem.Quantity.ToString());
                    transactionCommand.Append("&L_DESC" + (x + 1).ToString() + "=" + CleanForNVP(ppItem.Name));

                    if (ppItem.Site.Equals("EBAY", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (!String.IsNullOrEmpty(ppItem.TransactionID))
                        {
                            transactionCommand.Append("&L_TRANSACTIONID" + (x + 1).ToString() + "=" + ppItem.TransactionID);
                        }
                        transactionCommand.Append("&L_ITEMNUMBER" + (x + 1).ToString() + "=" + ppItem.ItemNumber);
                    }
                }


                if (ppCart.ShippingAmount > 0)
                {
                    transactionCommand.Append("&FREIGHTAMT=" + Localization.CurrencyStringForGatewayWithoutExchangeRate(ppCart.ShippingAmount));
                    ppTotal += Localization.ParseNativeDecimal(Localization.CurrencyStringForGatewayWithoutExchangeRate(ppCart.ShippingAmount));
                }

                if (ppCart.TaxAmount > 0)
                {
                    transactionCommand.Append("&TAXAMT=" + Localization.CurrencyStringForGatewayWithoutExchangeRate(ppCart.TaxAmount));
                    ppTotal += Localization.ParseNativeDecimal(Localization.CurrencyStringForGatewayWithoutExchangeRate(ppCart.TaxAmount));
                }

                transactionCommand.Append("&ITEMTOTAL=" + Localization.CurrencyStringForGatewayWithoutExchangeRate(ppCart.ItemTotal));

                ppTotal += Localization.ParseNativeDecimal(Localization.CurrencyStringForGatewayWithoutExchangeRate(ppCart.ItemTotal));
                transactionCommand.Append("&CURRENCY=" + AppLogic.AppConfig("Localization.StoreCurrency"));
                transactionCommand.Append("&AMT=" + Localization.CurrencyStringForGatewayWithoutExchangeRate(ppTotal));

            }
            else
            {
                transactionCommand.Append("&CURRENCY=" + AppLogic.AppConfig("Localization.StoreCurrency"));
                transactionCommand.Append("&AMT=" + Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal));
            }
            /*
CUSTOM
HANDLINGAMT
ITEM
L_TAXAMTn
ORDERDESC
SHIPTOCITY
SHIPTOCOUNTRY
SHIPTOSTATE
SHIPTOSTREET
SHIPTOZIP
             */

            int TO = AppLogic.AppConfigUSInt("PayFlowPro.Timeout");
            if (TO == 0)
            {
                TO = 45;
            }

            String AuthServer = CommonLogic.IIF(AppLogic.AppConfigBool("UseLiveTransactions"), AppLogic.AppConfig("PayFlowPro.LiveURL"), AppLogic.AppConfig("PayFlowPro.TestURL"));

            String RequestID = System.Guid.NewGuid().ToString("N");
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] data = encoding.GetBytes(transactionCommand.ToString());
            String rawResponseString = String.Empty;

            int MaxTries = AppLogic.AppConfigUSInt("GatewayRetries") + 1;
            int CurrentTry = 0;
            bool CallSuccessful = false;
            do
            {
                CurrentTry++;
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(AuthServer);
                myRequest.Method = "POST";
                myRequest.ContentType = "text/namevalue";
                myRequest.Headers.Add("X-VPS-REQUEST-ID", RequestID);
                myRequest.Headers.Add("X-VPS-CLIENT-TIMEOUT", TO.ToString());
                myRequest.Headers.Add("X-VPS-VIT-CLIENT-CERTIFICATION-ID", "aspdotnetstorefront");
                myRequest.ContentLength = data.Length;
                try
                {
                    Stream newStream = myRequest.GetRequestStream();
                    // Send the data.
                    newStream.Write(data, 0, data.Length);
                    newStream.Close();
                    // get the response
                    WebResponse myResponse;
                    myResponse = myRequest.GetResponse();
                    using (StreamReader sr = new StreamReader(myResponse.GetResponseStream()))
                    {
                        rawResponseString = sr.ReadToEnd();
                        sr.Close();
                    }
                    myResponse.Close();
                    CallSuccessful = true;
                }
                catch
                {
                    CallSuccessful = false;
                }
            }
            while (!CallSuccessful && CurrentTry < MaxTries);

            result = "ERROR: Error Calling PayFlow Payment Gateway"; // This should get overwritten below.
            String curString = rawResponseString;

            String replyCode = String.Empty;
            String replyMsg = String.Empty;
            String replyToken = String.Empty;
            String replyPendingReason = String.Empty;
            String replyPaymentType = String.Empty;
            String baid = String.Empty;

            String[] statusArray = curString.Split('&');
            for (int i = statusArray.GetLowerBound(0); i <= statusArray.GetUpperBound(0); i++)
            {
                String[] lasKeyPair = statusArray[i].Split('=');
                switch (lasKeyPair[0].ToLowerInvariant())
                {
                    case "result":
                        replyCode = lasKeyPair[1];
                        break;
                    case "respmsg":
                        replyMsg = lasKeyPair[1];
                        break;
                    case "token":
                        replyToken = lasKeyPair[1];
                        break;
                    case "pnref":
                        AuthorizationTransID = CommonLogic.IIF(!bEBay && AppLogic.TransactionModeIsAuthOnly(), "AUTH=", "CAPTURE=") + lasKeyPair[1];
                        break;
                    case "baid":
                        baid = lasKeyPair[1];
                        break;
                    case "paymenttype":
                        replyPaymentType = lasKeyPair[1];
                        break;
                    case "pendingreason":
                        replyPendingReason = lasKeyPair[1];
                        break;

                    /*
                    * AVSADDR=Y&
                    * */
                }
            }

            if (baid.Length > 0)
                AuthorizationTransID = baid;

            if (replyCode == "0")
            {
                result = AppLogic.ro_OK;
                AuthorizationResult = replyMsg;
            }
            else
            {
                result = "Error: [" + replyCode + "] " + replyMsg;
            }
            return result;
        }

        public static string CleanForNVP(string strIn)
        { // PayFlow does NOT want it URL encoded, so we'll just zap the characters that will break things.
            return strIn.Replace("&", "").Replace("=", "");
        }

        // ----------------------------------------------------------------------------------
        // RECURRING BILLING ROUTINES
        // ----------------------------------------------------------------------------------

        // returns AppLogic.ro_OK if successful, along with the out RecurringSubscriptionID
        // returns error message if not successful
        // SubscriptionDescription may usually best be passed in as the product description of the first cart item

        public static string RecurringBillingRestartPayment(String RecurringSubscriptionID, int OriginalRecurringOrderNumber)
        {
            String result = String.Empty;

            Order ord = new Order(OriginalRecurringOrderNumber);
            Customer cust = new Customer(ord.CustomerID, true);

            ShoppingCart cartRecur = new ShoppingCart(cust.SkinID, cust, CartTypeEnum.RecurringCart, OriginalRecurringOrderNumber,
                    false); // false will load recurring items that are not due yet, which we need to do here

            if (cartRecur.CartItems.Count == 0)
            {
                return "Failed. Could not load order items from database.";
            }

            Decimal CartTotalRecur = cartRecur.Total(true);
            Decimal RecurringAmount = CartTotalRecur - CommonLogic.IIF(cartRecur.Coupon.CouponType == CouponTypeEnum.GiftCard, CommonLogic.IIF(CartTotalRecur < cartRecur.Coupon.DiscountAmount, CartTotalRecur, cartRecur.Coupon.DiscountAmount), 0);

            String GAMT = Localization.CurrencyStringForGatewayWithoutExchangeRate(RecurringAmount);

            CartItem firstcartrecurringitem = ((CartItem)cartRecur.CartItems[0]);

            // we missed a shipment, so reset the schedule starting today and place a new order
            bool ResetSchedule = (firstcartrecurringitem.NextRecurringShipDate < System.DateTime.Now);

            DateTime NextRecurringShipDate = DateTime.MinValue;

            switch (firstcartrecurringitem.RecurringIntervalType)
            {
                case DateIntervalTypeEnum.Weekly:
                    NextRecurringShipDate = System.DateTime.Now.AddDays(7);
                    break;
                case DateIntervalTypeEnum.BiWeekly:
                    NextRecurringShipDate = System.DateTime.Now.AddDays(14);
                    break;
                case DateIntervalTypeEnum.EveryFourWeeks:
                    NextRecurringShipDate = System.DateTime.Now.AddDays(28);
                    break;
                case DateIntervalTypeEnum.Monthly:
                    NextRecurringShipDate = System.DateTime.Now.AddMonths(1);
                    break;
                case DateIntervalTypeEnum.Quarterly:
                    NextRecurringShipDate = System.DateTime.Now.AddMonths(3);
                    break;
                case DateIntervalTypeEnum.SemiYearly:
                    NextRecurringShipDate = System.DateTime.Now.AddMonths(6);
                    break;
                case DateIntervalTypeEnum.Yearly:
                    NextRecurringShipDate = System.DateTime.Now.AddYears(1);
                    break;
                default:
                    return "Failed. Could not calculate new schedule.";
            }

            String sNewStart = NextRecurringShipDate.Month.ToString("00")
                                + NextRecurringShipDate.Day.ToString("00")
                                + NextRecurringShipDate.Year.ToString("0000"); // Format is MMDDYYYY

            StringBuilder transactionCommand = new StringBuilder(4096);
            transactionCommand.Append("TRXTYPE=R"); // recurring billing API
            transactionCommand.Append("&ACTION=R"); // R = ReActivate
            transactionCommand.Append("&ORIGPROFILEID=" + RecurringSubscriptionID);
            if (ResetSchedule)
            {
                transactionCommand.Append("&START=" + sNewStart);
                transactionCommand.Append("&OPTIONALTRXAMT=" + GAMT);
                transactionCommand.Append("&OPTIONALTRX=S"); // S = Sale
            }
            transactionCommand.Append("&PARTNER=" + AppLogic.AppConfig("PayFlowPro.PARTNER"));
            transactionCommand.Append("&VENDOR=" + AppLogic.AppConfig("PayFlowPro.VENDOR"));
            transactionCommand.Append("&USER=" + AppLogic.AppConfig("PayFlowPro.USER"));
            transactionCommand.Append("&PWD=" + AppLogic.AppConfig("PayFlowPro.PWD"));
            transactionCommand.Append("&TENDER=C");

            String rawResponseString = SendReceiveNVP(transactionCommand.ToString());

            if (rawResponseString.Length == 0)
            {
                return "Error communicating with Gateway!";
            }

            String GRESULT = String.Empty;
            String GRESPMSG = String.Empty;
            String GTRXRESULT = String.Empty;
            String GTRXPNREF = String.Empty;
            String GTRXRESPMSG = String.Empty;

            String[] statusArray = rawResponseString.Split('&');
            for (int i = statusArray.GetLowerBound(0); i <= statusArray.GetUpperBound(0); i++)
            {
                String[] lasKeyPair = statusArray[i].Split('=');
                switch (lasKeyPair[0].ToLowerInvariant())
                {
                    case "result":
                        GRESULT = lasKeyPair[1];
                        break;
                    case "respmsg":
                        GRESPMSG = lasKeyPair[1];
                        break;
                    case "trxresult":
                        GTRXRESULT = lasKeyPair[1];
                        break;
                    case "trxrespmsg":
                        GTRXRESPMSG = lasKeyPair[1];
                        break;
                    case "trxpnref":
                        GTRXPNREF = lasKeyPair[1];
                        break;
                }
            }

            if (GRESULT == "0")
            {
                if (GTRXRESULT == "0")
                {
                    // Transaction approved, make the order if we missed the last one
                    if (ResetSchedule)
                    {
                        int NewOrderNumber = 0;
                        RecurringOrderMgr rmgr = new RecurringOrderMgr(null, null);
                        result = rmgr.ProcessRecurringOrder(OriginalRecurringOrderNumber, GTRXPNREF, DateTime.MinValue, out NewOrderNumber);

                        if (result == AppLogic.ro_OK)
                        {
                            // Reset NextRecurringShipDate based on interval starting Now.
                            DB.ExecuteSQL("update ShoppingCart set NextRecurringShipDate=" + DB.DateQuote(Localization.ToDBShortDateString(NextRecurringShipDate)) + " where OriginalRecurringOrderNumber=" + OriginalRecurringOrderNumber.ToString());

                            // Reset SubscriptionExpiresOn based on interval starting Now.
                            if (!AppLogic.AppConfigBool("SubscriptionExtensionOccursFromOrderDate"))
                            {
                                AppLogic.CustomerSubscriptionUpdate(NewOrderNumber, true);
                            }
                        }
                    }
                    else
                    {
                        result = AppLogic.ro_OK;
                    }
                }
                else
                {
                    result = GTRXRESPMSG;
                }
            }
            else
            {
                result = GRESPMSG;
            }

            if (result != AppLogic.ro_OK)
            {
                // Log failure
                String TransactionCommandOut = transactionCommand.ToString();
                TransactionCommandOut = TransactionCommandOut.Replace(CommonLogic.ExtractToken(TransactionCommandOut, "PWD=", "&"), "****");

                Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)HttpContext.Current.User).ThisCustomer;
                DB.ExecuteSQL("insert into FailedTransaction(CustomerID,OrderNumber,IPAddress,OrderDate,PaymentGateway,PaymentMethod,TransactionCommand,TransactionResult,CustomerEMailed,RecurringSubscriptionID) values(" +
                    ThisCustomer.CustomerID.ToString() + "," + OriginalRecurringOrderNumber.ToString() + "," +
                    DB.SQuote(ThisCustomer.LastIPAddress) + ",getdate()," + DB.SQuote(Gateway.ro_GWPAYFLOWPRO) + "," +
                    DB.SQuote(AppLogic.TransactionTypeEnum.RECURRING_AUTO.ToString()) + "," + DB.SQuote(TransactionCommandOut) + "," + DB.SQuote(rawResponseString) + ",0," + DB.SQuote(RecurringSubscriptionID) + ")");
            }

            return result;
        }

        public static string RecurringBillingRetryPayment(String RecurringSubscriptionID, int OriginalRecurringOrderNumber, String PaymentNumber)
        {
            String result = String.Empty;
            StringBuilder transactionCommand = new StringBuilder(4096);

            transactionCommand.Append("TRXTYPE=R"); // recurring billing API
            transactionCommand.Append("&ACTION=P"); // P = Payment
            transactionCommand.Append("&ORIGPROFILEID=" + RecurringSubscriptionID);
            if (PaymentNumber.Length > 0)
            {
                transactionCommand.Append("&PAYMENTNUM=" + PaymentNumber);
            }
            transactionCommand.Append("&PARTNER=" + AppLogic.AppConfig("PayFlowPro.PARTNER"));
            transactionCommand.Append("&VENDOR=" + AppLogic.AppConfig("PayFlowPro.VENDOR"));
            transactionCommand.Append("&USER=" + AppLogic.AppConfig("PayFlowPro.USER"));
            transactionCommand.Append("&PWD=" + AppLogic.AppConfig("PayFlowPro.PWD"));
            transactionCommand.Append("&TENDER=C");

            int TO = AppLogic.AppConfigUSInt("PayFlowPro.Timeout");
            if (TO == 0)
            {
                TO = 45;
            }

            String AuthServer = CommonLogic.IIF(AppLogic.AppConfigBool("UseLiveTransactions"), AppLogic.AppConfig("PayFlowPro.LiveURL"), AppLogic.AppConfig("PayFlowPro.TestURL"));

            String RequestID = System.Guid.NewGuid().ToString("N");
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] data = encoding.GetBytes(transactionCommand.ToString());
            String rawResponseString = String.Empty;

            int MaxTries = AppLogic.AppConfigUSInt("GatewayRetries") + 1;
            int CurrentTry = 0;
            bool CallSuccessful = false;
            do
            {
                CurrentTry++;
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(AuthServer);
                myRequest.Method = "POST";
                myRequest.ContentType = "text/namevalue";
                myRequest.Headers.Add("X-VPS-REQUEST-ID", RequestID);
                myRequest.Headers.Add("X-VPS-CLIENT-TIMEOUT", TO.ToString());
                myRequest.Headers.Add("X-VPS-VIT-CLIENT-CERTIFICATION-ID", "aspdotnetstorefront");
                myRequest.ContentLength = data.Length;
                try
                {
                    Stream newStream = myRequest.GetRequestStream();
                    // Send the data.
                    newStream.Write(data, 0, data.Length);
                    newStream.Close();
                    // get the response
                    WebResponse myResponse;
                    myResponse = myRequest.GetResponse();
                    using (StreamReader sr = new StreamReader(myResponse.GetResponseStream()))
                    {
                        rawResponseString = sr.ReadToEnd();
                        sr.Close();
                    }
                    myResponse.Close();
                    CallSuccessful = true;
                }
                catch
                {
                    CallSuccessful = false;
                }
            }
            while (!CallSuccessful && CurrentTry < MaxTries);

            if (!CallSuccessful)
            {
                result = "ERROR: Error Calling PayFlow Payment Gateway"; // This should get overwritten below.
            }
            else
            {

                String GRESULT = String.Empty;
                String GRESPMSG = String.Empty;
                String GTRXRESULT = String.Empty;
                String GTRXPNREF = String.Empty;
                String GTRXRESPMSG = String.Empty;

                String[] statusArray = rawResponseString.Split('&');
                for (int i = statusArray.GetLowerBound(0); i <= statusArray.GetUpperBound(0); i++)
                {
                    String[] lasKeyPair = statusArray[i].Split('=');
                    switch (lasKeyPair[0].ToLowerInvariant())
                    {
                        case "result":
                            GRESULT = lasKeyPair[1];
                            break;
                        case "respmsg":
                            GRESPMSG = lasKeyPair[1];
                            break;
                        case "trxresult":
                            GTRXRESULT = lasKeyPair[1];
                            break;
                        case "trxrespmsg":
                            GTRXRESPMSG = lasKeyPair[1];
                            break;
                        case "trxpnref":
                            GTRXPNREF = lasKeyPair[1];
                            break;
                    }
                }

                if (GRESULT == "0")
                {
                    if (GTRXRESULT == "0")
                    {
                        // Transaction approved, make the order
                        int NewOrderNumber = 0;
                        RecurringOrderMgr rmgr = new RecurringOrderMgr(null, null);
                        result = rmgr.ProcessRecurringOrder(OriginalRecurringOrderNumber, GTRXPNREF, DateTime.MinValue, out NewOrderNumber);
                    }
                    else
                    {
                        result = GTRXRESPMSG;
                    }
                }
                else
                {
                    result = GRESPMSG;
                }

                if (result != AppLogic.ro_OK)
                {
                    // Log failure
                    String TransactionCommandOut = transactionCommand.ToString();
                    TransactionCommandOut = TransactionCommandOut.Replace(CommonLogic.ExtractToken(TransactionCommandOut, "PWD=", "&"), "****");

                    Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)HttpContext.Current.User).ThisCustomer;
                    DB.ExecuteSQL("insert into FailedTransaction(CustomerID,OrderNumber,IPAddress,OrderDate,PaymentGateway,PaymentMethod,TransactionCommand,TransactionResult,CustomerEMailed,RecurringSubscriptionID) values(" +
                        ThisCustomer.CustomerID.ToString() + "," + OriginalRecurringOrderNumber.ToString() + "," +
                        DB.SQuote(ThisCustomer.LastIPAddress) + ",getdate()," + DB.SQuote(Gateway.ro_GWPAYFLOWPRO) + "," +
                        DB.SQuote(AppLogic.TransactionTypeEnum.RECURRING_AUTO.ToString()) + "," + DB.SQuote(TransactionCommandOut) + "," + DB.SQuote(rawResponseString) + ",0," + DB.SQuote(RecurringSubscriptionID) + ")");
                }
            }
            return result;
        }

        public static string RecurringBillingInquiry(String RecurringSubscriptionID, out DateTime StartDate, out DateTime NextPaymentDate, out decimal AggregateAmount, out String RecurringStatus, out String LatestPaymentIdentifier, out DateTime EndingDate)
        {
            String result = String.Empty;
            StringBuilder transactionCommand = new StringBuilder(4096);

            StartDate = DateTime.MinValue;
            NextPaymentDate = DateTime.MinValue;
            AggregateAmount = 0.0M;
            RecurringStatus = String.Empty;
            LatestPaymentIdentifier = String.Empty;
            EndingDate = DateTime.MinValue;

            transactionCommand.Append("TRXTYPE=R"); // recurring billing API
            transactionCommand.Append("&ACTION=I"); // I = Inquiry
            transactionCommand.Append("&ORIGPROFILEID=" + RecurringSubscriptionID);
            transactionCommand.Append("&PAYMENTHISTORY=N");  // N = No history, just profile details
            transactionCommand.Append("&PARTNER=" + AppLogic.AppConfig("PayFlowPro.PARTNER"));
            transactionCommand.Append("&VENDOR=" + AppLogic.AppConfig("PayFlowPro.VENDOR"));
            transactionCommand.Append("&USER=" + AppLogic.AppConfig("PayFlowPro.USER"));
            transactionCommand.Append("&PWD=" + AppLogic.AppConfig("PayFlowPro.PWD"));

            String responseNVP = String.Empty;
            responseNVP = SendReceiveNVP(transactionCommand.ToString());

            if (responseNVP.Length == 0)
            {
                return "Error communicating with Gateway!";
            }

            String GRESULT = String.Empty;
            String GRESPMSG = String.Empty;
            String GSTATUS = String.Empty;
            String GSTART = String.Empty;
            String GNEXTPAYMENT = String.Empty;
            String GEND = String.Empty;
            String GAGGREGATEAMT = String.Empty;

            String[] statusArray = responseNVP.Split('&');
            for (int i = statusArray.GetLowerBound(0); i <= statusArray.GetUpperBound(0); i++)
            {
                String[] lasKeyPair = statusArray[i].Split(new char[] { '=' }, 2, StringSplitOptions.None);
                switch (lasKeyPair[0].ToUpperInvariant())
                {
                    case "RESULT":
                        GRESULT = lasKeyPair[1];
                        break;
                    case "RESPMSG":
                        GRESPMSG = lasKeyPair[1];
                        break;
                    case "STATUS":
                        GSTATUS = lasKeyPair[1];
                        break;
                    case "START":
                        GSTART = lasKeyPair[1];
                        break;
                    case "NEXTPAYMENT":
                        GNEXTPAYMENT = lasKeyPair[1];
                        break;
                    case "END":
                        GEND = lasKeyPair[1];
                        break;
                    case "AGGREGATEAMT":
                        GAGGREGATEAMT = lasKeyPair[1];
                        break;
                }
            }

            if (GSTART.Length > 0)
            {
                GSTART = GSTART.Substring(0, 2) + "/" + GSTART.Substring(2, 2) + "/" + GSTART.Substring(4, 4);
            }
            StartDate = Localization.ParseUSDateTime(GSTART);

            if (GNEXTPAYMENT.Length > 0)
            {
                GNEXTPAYMENT = GNEXTPAYMENT.Substring(0, 2) + "/" + GNEXTPAYMENT.Substring(2, 2) + "/" + GNEXTPAYMENT.Substring(4, 4);
            }
            NextPaymentDate = Localization.ParseUSDateTime(GNEXTPAYMENT);

            if (GEND.Length > 0)
            {
                GEND = GEND.Substring(0, 2) + "/" + GEND.Substring(2, 2) + "/" + GEND.Substring(4, 4);
            }
            EndingDate = Localization.ParseUSDateTime(GEND);

            AggregateAmount = Localization.ParseNativeDecimal(GAGGREGATEAMT);
            RecurringStatus = GSTATUS;

            // Need to run it twice to get all the details. Once without history, once with history.
            transactionCommand = null;
            transactionCommand = new StringBuilder(4096);
            transactionCommand.Append("TRXTYPE=R"); // recurring billing API
            transactionCommand.Append("&ACTION=I"); // I = Inquiry
            transactionCommand.Append("&ORIGPROFILEID=" + RecurringSubscriptionID);
            transactionCommand.Append("&PAYMENTHISTORY=Y");  // Y = Payment history
            transactionCommand.Append("&PARTNER=" + AppLogic.AppConfig("PayFlowPro.PARTNER"));
            transactionCommand.Append("&VENDOR=" + AppLogic.AppConfig("PayFlowPro.VENDOR"));
            transactionCommand.Append("&USER=" + AppLogic.AppConfig("PayFlowPro.USER"));
            transactionCommand.Append("&PWD=" + AppLogic.AppConfig("PayFlowPro.PWD"));

            responseNVP = String.Empty;
            responseNVP = SendReceiveNVP(transactionCommand.ToString());

            if (responseNVP.Length == 0)
            {
                return "Error communicating with Gateway!";
            }

            int PaymentID = -1;
            int ThisPaymentID = -1;
            // We need to find the largest index "X" for all P_RESULTX's in the result.
            string rgx = @".*?&P_RESULT(?<PaymentID>\d+)=.*?";
            RegexOptions options = RegexOptions.Multiline | RegexOptions.Compiled;

            MatchCollection MatchCol = Regex.Matches(responseNVP, rgx, options);
            foreach (Match m in MatchCol)
            {
                ThisPaymentID = Localization.ParseNativeInt(m.Groups["PaymentID"].Value);
                if (ThisPaymentID > PaymentID)
                {
                    PaymentID = ThisPaymentID;
                }
            }
            LatestPaymentIdentifier = PaymentID.ToString();

            result = AppLogic.ro_OK;

            return result;
        }

        public static string RecurringBillingInquiryDisplay(String RecurringSubscriptionID)
        {
            String result = "Error";
            StringBuilder transactionCommand = new StringBuilder(4096);

            transactionCommand.Append("TRXTYPE=R"); // recurring billing API
            transactionCommand.Append("&ACTION=I"); // I = Inquiry
            transactionCommand.Append("&ORIGPROFILEID=" + RecurringSubscriptionID);
            transactionCommand.Append("&PAYMENTHISTORY=N");  // N = No history, just profile details
            transactionCommand.Append("&PARTNER=" + AppLogic.AppConfig("PayFlowPro.PARTNER"));
            transactionCommand.Append("&VENDOR=" + AppLogic.AppConfig("PayFlowPro.VENDOR"));
            transactionCommand.Append("&USER=" + AppLogic.AppConfig("PayFlowPro.USER"));
            transactionCommand.Append("&PWD=" + AppLogic.AppConfig("PayFlowPro.PWD"));

            String responseNVP = String.Empty;
            responseNVP = SendReceiveNVP(transactionCommand.ToString());

            if (responseNVP.Length == 0)
            {
                return "Error communicating with Gateway!";
            }

            String GRESULT = String.Empty;
            String GRESPMSG = String.Empty;
            String GSTATUS = String.Empty;
            String GSTART = String.Empty;
            String GNEXTPAYMENT = String.Empty;
            String GEND = String.Empty;
            String GAGGREGATEAMT = String.Empty;

            String[] statusArray = responseNVP.Split('&');
            for (int i = statusArray.GetLowerBound(0); i <= statusArray.GetUpperBound(0); i++)
            {
                String[] lasKeyPair = statusArray[i].Split(new char[] { '=' }, 2, StringSplitOptions.None);
                switch (lasKeyPair[0].ToUpperInvariant())
                {
                    case "RESULT":
                        GRESULT = lasKeyPair[1];
                        break;
                    case "RESPMSG":
                        GRESPMSG = lasKeyPair[1];
                        break;
                    case "STATUS":
                        GSTATUS = lasKeyPair[1];
                        break;
                    case "START":
                        GSTART = lasKeyPair[1];
                        break;
                    case "NEXTPAYMENT":
                        GNEXTPAYMENT = lasKeyPair[1];
                        break;
                    case "END":
                        GEND = lasKeyPair[1];
                        break;
                    case "AGGREGATEAMT":
                        GAGGREGATEAMT = lasKeyPair[1];
                        break;
                }
            }

            result = responseNVP.Replace("&", "");

            // Need to run it twice to get all the details. Once without history, once with history.
            transactionCommand = null;
            transactionCommand = new StringBuilder(4096);
            transactionCommand.Append("TRXTYPE=R"); // recurring billing API
            transactionCommand.Append("&ACTION=I"); // I = Inquiry
            transactionCommand.Append("&ORIGPROFILEID=" + RecurringSubscriptionID);
            transactionCommand.Append("&PAYMENTHISTORY=Y");  // Y = Payment history
            transactionCommand.Append("&PARTNER=" + AppLogic.AppConfig("PayFlowPro.PARTNER"));
            transactionCommand.Append("&VENDOR=" + AppLogic.AppConfig("PayFlowPro.VENDOR"));
            transactionCommand.Append("&USER=" + AppLogic.AppConfig("PayFlowPro.USER"));
            transactionCommand.Append("&PWD=" + AppLogic.AppConfig("PayFlowPro.PWD"));

            responseNVP = String.Empty;
            responseNVP = SendReceiveNVP(transactionCommand.ToString());
            result += "" + responseNVP.Replace("&", "");

            return result;
        }

        public static String SendReceiveNVP(String requestNVP)
        {
            string result = string.Empty;

            try
            {
                int TO = AppLogic.AppConfigUSInt("PayFlowPro.Timeout");
                if (TO == 0)
                {
                    TO = 45;
                }

                String AuthServer = CommonLogic.IIF(AppLogic.AppConfigBool("UseLiveTransactions"), AppLogic.AppConfig("PayFlowPro.LiveURL"), AppLogic.AppConfig("PayFlowPro.TestURL"));

                String RequestID = System.Guid.NewGuid().ToString("N");
                ASCIIEncoding encoding = new ASCIIEncoding();
                byte[] data = encoding.GetBytes(requestNVP);
                String rawResponseString = String.Empty;

                int MaxTries = AppLogic.AppConfigUSInt("GatewayRetries") + 1;
                int CurrentTry = 0;
                bool CallSuccessful = false;
                do
                {
                    HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(AuthServer);
                    myRequest.Method = "POST";
                    myRequest.ContentType = "text/namevalue";
                    myRequest.Headers.Add("X-VPS-REQUEST-ID", RequestID);
                    myRequest.Headers.Add("X-VPS-CLIENT-TIMEOUT", TO.ToString());
                    myRequest.Headers.Add("X-VPS-VIT-CLIENT-CERTIFICATION-ID", "aspdotnetstorefront");
                    myRequest.ContentLength = data.Length;
                    Stream newStream = myRequest.GetRequestStream();
                    // Send the data.
                    newStream.Write(data, 0, data.Length);
                    newStream.Close();
                    // get the response
                    WebResponse myResponse;

                    CurrentTry++;
                    try
                    {
                        myResponse = myRequest.GetResponse();
                        using (StreamReader sr = new StreamReader(myResponse.GetResponseStream()))
                        {
                            rawResponseString = sr.ReadToEnd();
                            sr.Close();
                        }
                        myResponse.Close();
                        CallSuccessful = true;
                    }
                    catch
                    {
                        CallSuccessful = false;
                    }
                }
                while (!CallSuccessful && CurrentTry < MaxTries);

                if (!CallSuccessful)
                {
                    result = "";
                }
                else
                {
                    result = rawResponseString;
                }
            }
            catch { }

            return result;
        }

        public static String SendReceiveReporting(String requestXML)
        {
            string result = string.Empty;

            int TO = AppLogic.AppConfigUSInt("PayFlowPro.Timeout");
            if (TO == 0)
            {
                TO = 45;
            }

            String AuthServer = CommonLogic.IIF(AppLogic.AppConfigBool("UseLiveTransactions"), AppLogic.AppConfig("PayFlowPro.Reporting.LiveURL"), AppLogic.AppConfig("PayFlowPro.Reporting.TestURL"));

            String RequestID = System.Guid.NewGuid().ToString("N");
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] data = encoding.GetBytes(requestXML);
            String rawResponseString = String.Empty;

            int MaxTries = AppLogic.AppConfigUSInt("GatewayRetries") + 1;
            int CurrentTry = 0;
            bool CallSuccessful = false;
            do
            {
                CurrentTry++;
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(AuthServer);
                myRequest.Method = "POST";
                myRequest.ContentType = "text/plain";
                myRequest.Headers.Add("X-VPS-REQUEST-ID", RequestID);
                myRequest.Headers.Add("X-VPS-CLIENT-TIMEOUT", TO.ToString());
                myRequest.Headers.Add("X-VPS-VIT-CLIENT-CERTIFICATION-ID", "aspdotnetstorefront");
                myRequest.ContentLength = data.Length;
                try
                {
                    Stream newStream = myRequest.GetRequestStream();
                    // Send the data.
                    newStream.Write(data, 0, data.Length);
                    newStream.Close();
                    // get the response
                    WebResponse myResponse;
                    myResponse = myRequest.GetResponse();
                    using (StreamReader sr = new StreamReader(myResponse.GetResponseStream()))
                    {
                        rawResponseString = sr.ReadToEnd();
                        sr.Close();
                    }
                    myResponse.Close();
                    CallSuccessful = true;
                }
                catch
                {
                    CallSuccessful = false;
                }
            }
            while (!CallSuccessful && CurrentTry < MaxTries);

            if (!CallSuccessful)
            {
                result = "";
            }
            else
            {
                result = rawResponseString;
            }

            return result;
        }

        public static Dictionary<string, string> GetParameterStringAsDictionary(string paramString, Boolean decodeValues)
        {
            Dictionary<string, string> returnedParameters = new Dictionary<string, string>();
            string[] pairs = paramString.Split('&');
            foreach (String param in pairs)
            {
                String[] kvp = param.ToUpper().Split('=');
                if (kvp.Length != 2)
                    continue;

                if (!returnedParameters.ContainsKey(kvp[0]))
                    returnedParameters.Add(kvp[0], (decodeValues ? HttpContext.Current.Server.UrlDecode(kvp[1]) : kvp[1]));
            }
            return returnedParameters;
        }

        public static bool TransactionMatchesPrice(string pnRef, decimal netTotal)
        {
            StringBuilder transactionCommand = new StringBuilder(4096);

            transactionCommand.Append("PWD=" + AppLogic.AppConfig("PayFlowPro.PWD"));
            transactionCommand.Append("&USER=" + AppLogic.AppConfig("PayFlowPro.USER"));
            transactionCommand.Append("&VENDOR=" + AppLogic.AppConfig("PayFlowPro.VENDOR"));
            transactionCommand.Append("&PARTNER=" + AppLogic.AppConfig("PayFlowPro.PARTNER"));

            transactionCommand.Append("&TRXTYPE=" + "I");
            transactionCommand.Append("&VERBOSITY=" + "HIGH");
            transactionCommand.Append("&ORIGID=" + pnRef);
            transactionCommand.Append("&COMMENT1=" + CleanForNVP(AppLogic.AppConfig("StoreName") + " Validation Inquery"));

            Dictionary<String, String> results = PayFlowProController.GetParameterStringAsDictionary(SendReceiveNVP(transactionCommand.ToString()), true);

            if (!results.ContainsKey("RESULT") || !results.ContainsKey("ORIGRESULT") || (!results.ContainsKey("ORIGPPREF") && !results.ContainsKey("ORIGPNREF")) || !results.ContainsKey("AMT"))
                return false;

            if (results["RESULT"].Trim() != "0" || results["ORIGRESULT"].Trim() != "0")
                return false;

            decimal returnedAmount;
            if (!decimal.TryParse(results["AMT"], out returnedAmount))
                return false;

            if (Math.Abs(netTotal - returnedAmount) > 0.05M)
                return false;

            return true;//all checks passed - valid transaction
        }

        private static string GetRawResult(string parameterString)
        {
            String authServer = CommonLogic.IIF(AppLogic.AppConfigBool("UseLiveTransactions"), AppLogic.AppConfig("PayFlowPro.LiveURL"), AppLogic.AppConfig("PayFlowPro.TestURL"));

            int timeOut = AppLogic.AppConfigUSInt("PayFlowPro.Timeout");
            if (timeOut == 0)
            {
                timeOut = 45;
            }

            String requestID = System.Guid.NewGuid().ToString("N");
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] data = encoding.GetBytes(parameterString);
            String rawResponseString = String.Empty;
            String result = String.Empty;

            int maxTries = AppLogic.AppConfigUSInt("GatewayRetries") + 1;
            int currentTry = 0;
            bool callSuccessful = false;
            do
            {
                currentTry++;
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(authServer);
                myRequest.Method = "POST";
                myRequest.ContentType = "text/namevalue";
                myRequest.Headers.Add("X-VPS-REQUEST-ID", requestID);
                myRequest.Headers.Add("X-VPS-CLIENT-TIMEOUT", timeOut.ToString());
                myRequest.Headers.Add("X-VPS-VIT-CLIENT-CERTIFICATION-ID", "aspdotnetstorefront");
                myRequest.ContentLength = data.Length;
                try
                {
                    Stream newStream = myRequest.GetRequestStream();
                    // Send the data.
                    newStream.Write(data, 0, data.Length);
                    newStream.Close();
                    // get the response
                    WebResponse myResponse;
                    myResponse = myRequest.GetResponse();
                    using (StreamReader sr = new StreamReader(myResponse.GetResponseStream()))
                    {
                        rawResponseString = sr.ReadToEnd();
                        sr.Close();
                    }
                    myResponse.Close();
                    callSuccessful = true;
                }
                catch
                {
                    callSuccessful = false;
                }
            }
            while (!callSuccessful && currentTry < maxTries);

            return rawResponseString;
        }
    }

    public class PayPalEmbeddedCheckoutSecureTokenResponse
    {
        #region Private Instance Variables
        private Dictionary<String, String> _Parameters;
        private Dictionary<String, String> Parameters
        {
            get
            {
                if (_Parameters == null)
                    _Parameters = new Dictionary<string, string>();

                return _Parameters;
            }
        }
        #endregion
        #region Public Instance Variables
        public int Result
        {
            get
            {
                int result;
                if (int.TryParse(GetKey("RESULT"), out result))
                    return result;
                return -1;
            }
            protected set
            {
                SetKey("RESULT", value.ToString());
            }
        }
        public String ResponseMessage
        {
            get
            {
                return GetKey("RESPMSG");
            }
            protected set
            {
                SetKey("RESPMSG", value);
            }
        }
        public String SecureToken
        {
            get
            {
                return GetKey("SecureToken");
            }
            protected set
            {
                SetKey("SecureToken", value);
            }
        }
        public String SecureTokenID
        {
            get
            {
                return GetKey("SecureTokenID");
            }
            protected set
            {
                SetKey("SecureTokenID", value);
            }
        }
        #endregion

        #region Constructor(s)
        public PayPalEmbeddedCheckoutSecureTokenResponse(string rawResponse)
        {
            //RESULT=0&SECURETOKEN=2OyZRMncLpUWD0JmuUsTfBA8o&SECURETOKENID=b08555cf64604310ae610df127d21fc7&RESPMSG=Approved
            List<string> returnedParameters = new List<string>();
            returnedParameters.AddRange(rawResponse.Split('&'));
            foreach (String param in returnedParameters)
            {
                String[] kvp = param.Split('=');
                if (kvp.Length != 2)
                    continue;

                switch (kvp[0].ToUpper())
                {
                    case "RESULT":
                        int r;
                        if (int.TryParse(kvp[1], out r))
                            Result = r;
                        else
                            Result = -1;
                        break;
                    case "SECURETOKEN":
                        SecureToken = kvp[1];
                        break;
                    case "SECURETOKENID":
                        SecureTokenID = kvp[1];
                        break;
                    case "RESPMSG":
                        ResponseMessage = kvp[1];
                        break;
                }
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns the HTML Markup for the PayPal Payments Hosted Checkout Frame
        /// </summary>
        /// <param name="width">Specifies the width set on the returned frame. If 0 then the width will be set to 100%.</param>
        /// <param name="height">Specifies the height set on the returned frame.</param>
        /// <returns></returns>
        public String GetFrameSrc(int width, int height)
        {
            bool isTest = !AppLogic.AppConfigBool("UseLiveTransactions");
            String authServer = CommonLogic.IIF(!isTest, AppLogic.AppConfig("PayFlowPro.LiveURL"), AppLogic.AppConfig("PayFlowPro.TestURL"));

            if (!authServer.EndsWith("/"))
                authServer += "/";

            String frameFormat = "<iframe src=\"{0}?SECURETOKEN={1}&SECURETOKENID={2}{5}\" width=\"{3}\" height=\"{4}\"></iframe>";

            return string.Format(frameFormat,
                "https://payflowlink.paypal.com/",
                HttpContext.Current.Server.UrlEncode(SecureToken),
                HttpContext.Current.Server.UrlEncode(SecureTokenID),
                (width == 0 ? "100%" : width.ToString()),
                height,
                "&TEMPLATE=MINLAYOUT" + (isTest ? "&MODE=TEST" : "")
                );
        }
        #endregion
        #region Private Methods
        private void SetKey(string key, string value)
        {
            if (Parameters.ContainsKey(key))
            {
                Parameters[key] = value;
                return;
            }
            Parameters.Add(key, value);
        }
        private String GetKey(string key)
        {
            if (Parameters.ContainsKey(key))
                return Parameters[key];
            return null;
        }
        #endregion
    }

    public class PayPalEmbeddedCheckoutCallBackProcessor
    {
        #region Private Instance Variables
        private Customer ThisCustomer;
        private Dictionary<string, string> ReturnedParams;
        private int Result
        {
            get
            {
                int i;
                string si = GetReturnedParameter("RESULT");

                if (si != null && int.TryParse(si, out i))
                    return i;
                return -1;
            }
        }
        private String ResponseMessage
        {
            get
            {
                return GetReturnedParameter("RESPMSG");
            }
        }
        private String Tender
        {
            get
            {
                return GetReturnedParameter("TENDER");
            }
        }
        private String PNREF
        {
            get
            {
                return GetReturnedParameter("PNREF");
            }
        }
        private String BAID
        {
            get
            {
                return GetReturnedParameter("BAID");
            }
        }
        private String PPREF
        {
            get
            {
                return GetReturnedParameter("PPREF");
            }
        }
        private Decimal Amount
        {
            get
            {
                decimal d;
                string sd = GetReturnedParameter("AMT");
                if (sd != null && decimal.TryParse(sd, out d))
                    return d;
                return -1;
            }
        }
        private String BillToEmail
        {
            get
            {
                return GetReturnedParameter("EMAIL", "BILLTOEMAIL");
            }
        }
        private String FirstName
        {
            get
            {
                return GetReturnedParameter("FIRSTNAME");
            }
        }
        private String LastName
        {
            get
            {
                return GetReturnedParameter("LASTNAME");
            }
        }
        private String InvoiceNumber
        {
            get
            {
                return GetReturnedParameter("INVNUM");
            }
        }
        private String ShipToCity
        {
            get
            {
                return GetReturnedParameter("SHIPTOCITY", "CITYTOSHIP");
            }
        }
        private String ShipToCountry
        {
            get
            {
                return GetReturnedParameter("SHIPTOCOUNTRY", "COUNTRYTOSHIP");
            }
        }
        private String ShipToEmail
        {
            get
            {
                return GetReturnedParameter("SHIPTOEMAIL", "EMAILTOSHIP");
            }
        }
        private String ShipToState
        {
            get
            {
                return GetReturnedParameter("SHIPTOSTATE", "STATETOSHIP");
            }
        }
        private String ShipToStreet
        {
            get
            {
                return GetReturnedParameter("SHIPTOSTREET", "ADDRESSTOSHIP");
            }
        }
        private String ShipToZip
        {
            get
            {
                return GetReturnedParameter("SHIPTOZIP", "ZIPTOSHIP");
            }
        }
        private String TransactionType
        {
            get
            {
                return GetReturnedParameter("TYPE", "TRXTYPE");
            }
        }
        private String ADNSFTransactionState
        {
            get
            {
                if (TransactionType != null && TransactionType.ToLower() == "a")
                    return AppLogic.ro_TXStateAuthorized;

                return AppLogic.ro_TXStateCaptured;
            }
        }
        #endregion

        #region Constructor(s)
        public PayPalEmbeddedCheckoutCallBackProcessor(Dictionary<String, String> returnedParameters, Customer thiscustomer)
        {
            this.ReturnedParams = returnedParameters;
            this.ThisCustomer = thiscustomer;
        }
        #endregion

        #region Public Methods
        public String ProcessCallBack()
        {
            String PM = AppLogic.CleanPaymentMethod(AppLogic.ro_PMPayPalEmbeddedCheckout);
            AppLogic.ValidatePM(PM); // this WILL throw a hard security exception on any problem!

            ShoppingCart cart = new ShoppingCart(1, ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);
            //recalculate total for verification
            decimal cartTotal = cart.Total(true);
            decimal netTotal = cartTotal - CommonLogic.IIF(cart.Coupon.CouponType == CouponTypeEnum.GiftCard, CommonLogic.IIF(cartTotal < cart.Coupon.DiscountAmount, cartTotal, cart.Coupon.DiscountAmount), 0);
            netTotal = Localization.ParseNativeDecimal(Localization.CurrencyStringForGatewayWithoutExchangeRate(netTotal));

            if (!ThisCustomer.HasCustomerRecord)
                throw new System.Security.SecurityException("Customer not signed in to complete paypal transaction.");

            if (Result != 0)
                return ReturnPaypalError(cart);

            ConfirmTransaction(netTotal); // this WILL throw a hard security exception on any problem!

            if (AppLogic.AppConfigBool("PayPal.RequireConfirmedAddress") || !ReturnedAddressMatches(ThisCustomer.PrimaryShippingAddress))
                cart = ApplyAddressToCart(cart);

            if (cart.IsEmpty())
            {
                ErrorMessage er = new ErrorMessage("Could not complete the transaction because the shopping cart was empty.");
                var checkoutController = CheckOutPageControllerFactory.CreateCheckOutPageController(ThisCustomer, cart);
                return checkoutController.GetCheckoutPaymentPage() + "?errormsg=" + er.MessageId;
            }

            //the callback is valid. make the order.
            int orderNumber = AppLogic.GetNextOrderNumber();
            try
            {
                ThisCustomer.PrimaryBillingAddress.PaymentMethodLastUsed = AppLogic.ro_PMPayPalEmbeddedCheckout;
                ThisCustomer.PrimaryBillingAddress.UpdateDB();

                IDictionary<string, string> transactionContext = new Dictionary<string, string>();
                transactionContext.Add("TENDER", Tender);

                string XID = string.Empty;

                if (BAID != null && BAID.Length > 0)
                    XID = BAID;
                else
                    XID = PNREF;

                //Process as AuthOnly first
                String status = Gateway.MakeOrder(AppLogic.ro_PMPayPalEmbeddedCheckout, AppLogic.ro_TXModeAuthOnly, cart, orderNumber, String.Empty, String.Empty, XID, String.Empty, transactionContext);

                if (status == AppLogic.ro_OK)
                { 
                    // Now, if paid for, process as Captured
                    if (ADNSFTransactionState == AppLogic.ro_TXStateAuthorized)
                    {
                        DB.ExecuteSQL("update orders set AuthorizationPNREF=" + DB.SQuote(PNREF) + " where OrderNumber=" + orderNumber.ToString());
                    }
                    if (ADNSFTransactionState == AppLogic.ro_TXStateCaptured)
                    {
                        Gateway.ProcessOrderAsCaptured(orderNumber);
                        DB.ExecuteSQL("update orders set AuthorizationPNREF=" + DB.SQuote(PNREF + "|CAPTURE=" + PNREF) + " where OrderNumber=" + orderNumber.ToString());
                    }
                    else if (ADNSFTransactionState == AppLogic.ro_TXStatePending)
                    {
                        DB.ExecuteSQL("update orders set TransactionState=" + DB.SQuote(AppLogic.ro_TXStatePending) + " where OrderNumber=" + orderNumber.ToString());
                    }

                    if (!string.IsNullOrEmpty(PPREF))
                    {
                        OrderTransactionCollection transactions = new OrderTransactionCollection(orderNumber);
                        transactions.AddTransaction(ADNSFTransactionState, null, null, null, PPREF, AppLogic.ro_PMPayPalEmbeddedCheckout, Gateway.ro_GWPAYFLOWPRO, 0);
                    }
                }

                if (Math.Abs(netTotal - Amount) > 0.05M) // allow 0.05 descrepency to allow minor rounding errors
                {
                    Order.MarkOrderAsFraud(orderNumber, true);
                    DB.ExecuteSQL("update orders set FraudedOn=getdate(), IsNew=1 where OrderNumber=" + orderNumber.ToString());
                }
            }
            catch // if we failed, did the IPN come back at the same time?
            {
                cart = new ShoppingCart(1, ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);
                if (cart.IsEmpty())
                {
                    orderNumber = DB.GetSqlN("select MAX(OrderNumber) N from dbo.orders where CustomerID = " + ThisCustomer.CustomerID.ToString());
                }
            }

            return "orderconfirmation.aspx?ordernumber=" + orderNumber + "&paymentmethod=PayPalEmbeddedCheckout";
        }

        private ShoppingCart ApplyAddressToCart(ShoppingCart cart)
        {
            ApplyAddressFromResponse(ThisCustomer);
            // Reload customer and cart so that we have the addresses right
            ThisCustomer = new Customer(ThisCustomer.CustomerID, true);
            cart = new ShoppingCart(1, ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);
            return cart;
        }

        private string ReturnPaypalError(ShoppingCart cart)
        {
            ErrorMessage er;
            if (!string.IsNullOrEmpty(ResponseMessage))
                er = new ErrorMessage(ResponseMessage);
            else
                er = new ErrorMessage("There was an error processing your payment.");

            var checkoutController = CheckOutPageControllerFactory.CreateCheckOutPageController(ThisCustomer, cart);
            return checkoutController.GetCheckoutPaymentPage() + "?errormsg=" + er.MessageId;
        }

        private void ConfirmTransaction(decimal netTotal)
        {
            if (!PayFlowProController.TransactionMatchesPrice(PNREF, netTotal))
                throw new System.Security.SecurityException("The PayPal transaction did not match your order total.");

        }
        #endregion
        #region Private Methods
        private void ApplyAddressFromResponse(Customer thisCustomer)
        {
            if (FirstName == null
                || LastName == null
                || ShipToCity == null
                || ShipToState == null
                || ShipToZip == null
                || ShipToCountry == null)
                return;

            Address newShippingAddress = new Address(thisCustomer.CustomerID);

            String[] StreetArray = ShipToStreet.Split(new string[1] { "\r\n" }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (StreetArray.Length > 1)
            {
                newShippingAddress.Address1 = StreetArray[0];
                newShippingAddress.Address2 = StreetArray[1];
            }
            else
            {
                newShippingAddress.Address1 = ShipToStreet;
            }

            newShippingAddress.FirstName = FirstName;
            newShippingAddress.LastName = LastName;
            newShippingAddress.City = ShipToCity;
            newShippingAddress.State = ShipToState;
            newShippingAddress.Zip = ShipToZip;
            newShippingAddress.Country = ShipToCountry;

            string sql = String.Format("select top 1 AddressID as N from Address where Address1={0} and Address2={1} and City={2} and State={3} and Zip={4} and Country={5} and FirstName={6} and LastName={7} and CustomerID={8}",
                DB.SQuote(newShippingAddress.Address1)
                , DB.SQuote(newShippingAddress.Address2)
                , DB.SQuote(newShippingAddress.City)
                , DB.SQuote(newShippingAddress.State)
                , DB.SQuote(newShippingAddress.Zip)
                , DB.SQuote(newShippingAddress.Country)
                , DB.SQuote(newShippingAddress.FirstName)
                , DB.SQuote(newShippingAddress.LastName)
                , thisCustomer.CustomerID);
            int ExistingAddressID = DB.GetSqlN(sql);

            if (ExistingAddressID == 0 || thisCustomer.PrimaryShippingAddressID != ExistingAddressID)
            {
                string note = "Note: Customer selected Ship-To address at PayPal.com";
                string ordernote = DB.GetSqlS("select OrderNotes S from Customer where CustomerID=" + thisCustomer.CustomerID.ToString());
                if (!ordernote.Contains(note))
                {
                    ordernote += System.Environment.NewLine + note;
                    DB.ExecuteSQL("update Customer set OrderNotes=" + DB.SQuote(ordernote) + " where CustomerID=" + thisCustomer.CustomerID.ToString());
                }
            }

            if (ExistingAddressID == 0)
            { // Does not exist
                newShippingAddress.InsertDB();
                newShippingAddress.MakeCustomersPrimaryAddress(AddressTypes.Shipping);
            }
            else
            { // Exists already
                newShippingAddress.LoadFromDB(ExistingAddressID);
                newShippingAddress.MakeCustomersPrimaryAddress(AddressTypes.Shipping);
            }
        }

        private Boolean ReturnedAddressMatches(Address a)
        {
            if (!String.IsNullOrEmpty(ShipToCity) && ShipToCity.ToLower() != a.City.ToLower().Trim()) return false;
            if (!String.IsNullOrEmpty(ShipToCountry) && ShipToCountry.ToLower() != a.Country.ToLower().Trim()) return false;
            if (!String.IsNullOrEmpty(ShipToEmail) && ShipToEmail.ToLower() != a.City.ToLower().Trim()) return false;
            if (!String.IsNullOrEmpty(ShipToState) && ShipToState.ToLower() != a.City.ToLower().Trim()) return false;
            if (!String.IsNullOrEmpty(ShipToStreet) && ShipToStreet.ToLower() != a.City.ToLower().Trim()) return false;
            if (!String.IsNullOrEmpty(ShipToZip) && ShipToZip.ToLower() != a.City.ToLower().Trim()) return false;
            return true;
        }

        /// <summary>
        /// Returns the parameter that matches any of the passed strings.
        /// </summary>
        /// <param name="values">Paramater name(s)</param>
        /// <returns></returns>
        private String GetReturnedParameter(params String[] values)
        {
            foreach (string s in values)
            {
                if (ReturnedParams.ContainsKey(s))
                    return ReturnedParams[s];
                else if (ReturnedParams.ContainsKey(s.ToLower()))
                    return ReturnedParams[s.ToLower()];
            }
            return null;
        }
        #endregion
    }

    [XmlRoot("reportingEngineRequest")]
    public class reportingEngineRequest
    {
        public reportingEngineRequest() { }

        [XmlElement("authRequest")]
        public AuthorizeRequest authRequest;

        [XmlElement("runReportRequest")]
        public runReportRequest RunRptReq;

        [XmlElement("getDataRequest")]
        public getDataRequest DataRequest;

        [XmlElement("getMetaDataRequest")]
        public getMetaDataRequest MetaDataRequest;

        [XmlElement("getResultsRequest")]
        public getResultsRequest ResultsRequest;

    }

    public class AuthorizeRequest
    {
        [XmlElement("user")]
        public string user;
        [XmlElement("vendor")]
        public string vendor;
        [XmlElement("partner")]
        public string partner;
        [XmlElement("password")]
        public string password;

    }

    public class getDataRequest
    {
        [XmlElement("reportId")]
        public string reportId;
        [XmlElement("pageNum")]
        public string pageNum;

    }

    public class getMetaDataRequest
    {
        [XmlElement("reportId")]
        public string reportId;
    }

    public class getResultsRequest
    {
        [XmlElement("reportId")]
        public string reportId;
    }

    public class runReportRequest
    {
        [XmlElement("reportName")]
        public string rptName;

        [XmlElement("reportParam")]
        public reportParam[] rptParams;

        [XmlElement("pageSize")]
        public string pageSize;
    }

    public class reportParam
    {
        [XmlElement("paramName")]
        public string paramName;

        [XmlElement("paramValue")]
        public string paramValue;
    }
}
