// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web;
using System.Xml;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontGateways.Processors
{
	/// <summary>
	/// Summary description for PayPal.
	/// </summary>
	public static class PayPalController
	{
        public const String BN = "AspDotNet" + "Storefront" + "_Cart"; // Do not change this line or your paypal website calls may not work!
        private const String API_VER = "98";
        //private const Boolean LogToErrorTable = false;

        // ProcessPaypal() is used for Express Checkout and PayPal payments.
        // Credit Card processing via Website Payments Pro is handled by ProcessCard(),
        // just like other credit card gateways.
        static public String ProcessPaypal(int OrderNumber, int CustomerID, Decimal OrderTotal, bool useLiveTransactions, String TransactionMode, Address UseBillingAddress, Address UseShippingAddress, String CAVV, String ECI, String XID, out String AVSResult, out String AuthorizationResult, out String AuthorizationCode, out String AuthorizationTransID, out String TransactionCommandOut, out String TransactionResponse)
        {
            String result = AppLogic.ro_OK;

            AuthorizationCode = String.Empty;
            AuthorizationResult = String.Empty;
            AuthorizationTransID = String.Empty;
            if (!String.IsNullOrEmpty(XID))
            {
                AuthorizationTransID = CommonLogic.IIF(TransactionMode == AppLogic.ro_TXModeAuthOnly, "AUTH=", "CAPTURE=") + XID;
            }
            AVSResult = String.Empty;
            TransactionCommandOut = String.Empty;
            TransactionResponse = String.Empty;
            return result;
        }

        static public string GetFirstAuctionSite(CartItemCollection cartItems)
        {
            String itemAuctionSite = String.Empty;

            for (int ci = 0; ci < cartItems.Count; ci++)
            {
                CartItem c = cartItems[ci];
                if (c.Quantity > 0)
                {
                    if (c.IsAuctionItem)
                    {
                        String Marketworks = DB.GetSqlS("select extensiondata S from product where productid=" + c.ProductID.ToString());
                        XmlDocument docExtensionData = new XmlDocument();

                        try
                        {
                            docExtensionData.LoadXml(Marketworks);
                            XmlNode node = docExtensionData.SelectSingleNode("/Marketworks/Marketplace");
                            if (node != null)
                            {
                                itemAuctionSite = node.InnerText;
                            }
                        }
                        catch { }

                        if (!String.IsNullOrEmpty(itemAuctionSite))
                        {
                            break;
                        }
                    }
                }
            }

            return itemAuctionSite;
        }

        static public String GetTransactionState(String PaymentStatus, String PendingReason)
        {
            String result = String.Empty;

            switch (PaymentStatus.ToLowerInvariant())
            {
                case "pending":
                    switch (PendingReason.ToLowerInvariant())
                    {
                        case "unilateral":
                            result = AppLogic.ro_TXStateCaptured;
                            break;
                        case "authorization":
                            result = AppLogic.ro_TXStateAuthorized;
                            break;
                        default:
                            result = AppLogic.ro_TXStatePending;
                            break;
                    }
                    break;
                case "processed":
                case "completed":
                case "canceled_reversal":
                    result = AppLogic.ro_TXStateCaptured;
                    break;
                case "denied":
                case "expired":
                case "failed":
                case "voided":
                    result = AppLogic.ro_TXStateVoided;
                    break;
                case "refunded":
                case "reversed":
                    result = AppLogic.ro_TXStateRefunded;
                    break;
                default:
                    result = AppLogic.ro_TXStateUnknown;
                    break;
            }
            return result;
        }

        static public ExpressAPIType GetAppropriateExpressType()
        {

            String APIUserName = AppLogic.AppConfig("PayPal.API.Username");
            String APIPassword = AppLogic.AppConfig("PayPal.API.Password");
            String APISigniture = AppLogic.AppConfig("PayPal.API.Signature");
            String APIAcceleratedBoardingEmail = AppLogic.AppConfig("PayPal.API.AcceleratedBoardingEmailAddress");
            // Even if the active gateway is PAYFLOWPRO, if the PayPal.API 
            // credentials are filled in we want to use the PayPal API.
            if (APIAcceleratedBoardingEmail.Length != 0)
                return ExpressAPIType.PayPalAcceleratedBording;
            else if (APIUserName.Length != 0 && APIPassword.Length != 0 && APISigniture.Length != 0)
                return ExpressAPIType.PayPalExpress;
            else if (AppLogic.ActivePaymentGatewayCleaned() == Gateway.ro_GWPAYFLOWPRO)
                return ExpressAPIType.PayFlowPro;
            else
                return ExpressAPIType.NoValidAPIType;
        }

		public static String StartEC(ShoppingCart cart, Address shippingAddress, bool boolBypassOrderReview, IDictionary<string, string> checkoutOptions)
        {
            PayPalAPISoapBinding IPayPalRefund;
            PayPalAPIAASoapBinding IPayPal;
            PayPalController.GetPaypalRequirements(out IPayPalRefund, out IPayPal);

            StringBuilder sURL = new StringBuilder();

            //Express checkout
            BasicAmountType ECOrderTotal = new BasicAmountType();
            AddressType ECShippingAddress = new AddressType();

            Decimal OrderTotal = cart.Total(true);

            ECOrderTotal.Value = Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal);

            if (shippingAddress != null)
            {
                ECShippingAddress.Street1 = shippingAddress.Address1;
                ECShippingAddress.Street2 = shippingAddress.Address2;
                ECShippingAddress.CityName = shippingAddress.City;
                ECShippingAddress.StateOrProvince = shippingAddress.State;
                ECShippingAddress.PostalCode = shippingAddress.Zip;
                ECShippingAddress.Country = (CountryCodeType)Enum.Parse(typeof(CountryCodeType), AppLogic.GetCountryTwoLetterISOCode(shippingAddress.Country), true);
                ECShippingAddress.CountrySpecified = true;
            }
            
            SetExpressCheckoutReq ECRequest = new SetExpressCheckoutReq();
            SetExpressCheckoutRequestType varECRequest = new SetExpressCheckoutRequestType();
            SetExpressCheckoutRequestDetailsType varECRequestDetails = new SetExpressCheckoutRequestDetailsType();
            SetExpressCheckoutResponseType ECResponse = new SetExpressCheckoutResponseType();
            
            if (cart.HasRecurringComponents())
            {
                //Have to send extra details on the SetExpressCheckoutReq or the token will be invalid for creating a recurring profile later
                BillingAgreementDetailsType varECRecurringAgreement = new BillingAgreementDetailsType();
                varECRecurringAgreement.BillingType = BillingCodeType.RecurringPayments;
                varECRecurringAgreement.BillingAgreementDescription = "Recurring order created on " + System.DateTime.Now.ToShortDateString() + " from " + AppLogic.AppConfig("StoreName");

                List<BillingAgreementDetailsType> ECRecurringAgreementList = new List<BillingAgreementDetailsType>();

                ECRecurringAgreementList.Add(varECRecurringAgreement);

                varECRequestDetails.BillingAgreementDetails = ECRecurringAgreementList.ToArray();
            }

            ECRequest.SetExpressCheckoutRequest = varECRequest;
            varECRequest.SetExpressCheckoutRequestDetails = varECRequestDetails;

            ECOrderTotal.currencyID = (CurrencyCodeType)Enum.Parse(typeof(CurrencyCodeType), AppLogic.AppConfig("Localization.StoreCurrency"), true);
            varECRequestDetails.OrderTotal = ECOrderTotal;

            if (AppLogic.AppConfigBool("PayPal.RequireConfirmedAddress"))
            {
                varECRequestDetails.ReqConfirmShipping = "1";
            }
            else
            {
                varECRequestDetails.ReqConfirmShipping = "0";
                if (ECShippingAddress != null && !String.IsNullOrEmpty(ECShippingAddress.CityName + ECShippingAddress.StateOrProvince + ECShippingAddress.PostalCode))
                { // if shipping address defined (not Anonymous)
                    varECRequestDetails.Address = ECShippingAddress;
                    varECRequestDetails.AddressOverride = "1";
                }
            }

            varECRequestDetails.ReturnURL = AppLogic.GetStoreHTTPLocation(true) + AppLogic.AppConfig("PayPal.Express.ReturnURL");
            if (boolBypassOrderReview)
            {
                varECRequestDetails.ReturnURL += "?BypassOrderReview=true";
            }
            varECRequestDetails.CancelURL = AppLogic.GetStoreHTTPLocation(true) + AppLogic.AppConfig("PayPal.Express.CancelURL");
            varECRequestDetails.LocaleCode = AppLogic.AppConfig("PayPal.DefaultLocaleCode");
            varECRequestDetails.PaymentAction = PaymentActionCodeType.Authorization;
            if (AppLogic.TransactionModeIsAuthCapture() || AppLogic.AppConfigBool("PayPal.ForceCapture") || PayPalController.GetAppropriateExpressType() == ExpressAPIType.PayPalAcceleratedBording)
            {
                varECRequestDetails.PaymentAction = PaymentActionCodeType.Sale;
            }

            // for eBay support with Auction Items
            if (PayPalController.GetFirstAuctionSite(cart.CartItems).ToUpperInvariant() == "EBAY")
            {
                // eBay Auction Items require "Sale"
                varECRequestDetails.PaymentAction = PaymentActionCodeType.Sale;
                varECRequestDetails.ChannelType = ChannelType.eBayItem;
            }
            varECRequestDetails.SolutionType = SolutionTypeType.Sole;
            varECRequestDetails.PaymentActionSpecified = true;

            varECRequest.Version = API_VER;

            if (AppLogic.AppConfig("PayPal.Express.PageStyle").Trim() != "")
            {
                varECRequestDetails.PageStyle = AppLogic.AppConfig("PayPal.Express.PageStyle").Trim();
            }

            if (AppLogic.AppConfig("PayPal.Express.HeaderImage").Trim() != "")
            {
                varECRequestDetails.cppheaderimage = AppLogic.AppConfig("PayPal.Express.HeaderImage").Trim();
            }

            if (AppLogic.AppConfig("PayPal.Express.HeaderBackColor").Trim() != "")
            {
                varECRequestDetails.cppheaderbackcolor = AppLogic.AppConfig("PayPal.Express.HeaderBackColor").Trim();
            }

            if (AppLogic.AppConfig("PayPal.Express.HeaderBorderColor").Trim() != "")
            {
                varECRequestDetails.cppheaderbordercolor = AppLogic.AppConfig("PayPal.Express.HeaderBorderColor").Trim();
            }

            if (AppLogic.AppConfig("PayPal.Express.PayFlowColor").Trim() != "")
            {
                varECRequestDetails.cpppayflowcolor = AppLogic.AppConfig("PayPal.Express.PayFlowColor").Trim();
            }

			if (checkoutOptions != null && checkoutOptions.ContainsKey("UserSelectedFundingSource") && checkoutOptions["UserSelectedFundingSource"] == "BML")
			{
				FundingSourceDetailsType fundingSourceDetails = new FundingSourceDetailsType();
				fundingSourceDetails.AllowPushFunding = "0";
				fundingSourceDetails.UserSelectedFundingSource = UserSelectedFundingSourceType.BML;
				fundingSourceDetails.UserSelectedFundingSourceSpecified = true;
				varECRequestDetails.FundingSourceDetails = fundingSourceDetails;
			}

            String result = String.Empty;
            try
            {
                ECResponse = IPayPal.SetExpressCheckout(ECRequest);

                if (ECResponse.Ack.ToString().StartsWith("success", StringComparison.InvariantCultureIgnoreCase))
                {
                    result = AppLogic.ro_OK;
                }
                else
                {
                    if (ECResponse.Errors != null)
                    {
                        bool first = true;
                        for (int ix = 0; ix < ECResponse.Errors.Length; ix++)
                        {
                            if (!first)
                            {
                                result += ", ";
                            }
                            result += "Error: [" + ECResponse.Errors[ix].ErrorCode + "] " + ECResponse.Errors[ix].LongMessage;
                            first = false;
                        }
                    }
                }

            }
            catch (Exception)
            {
                result = "Failed to start PayPal Express Checkout! Please try another payment method.";
            }

            if (result == AppLogic.ro_OK)
            {
				bool useIntegratedCheckout = AppLogic.AppConfigBool("PayPal.Express.UseIntegratedCheckout");

                if (AppLogic.AppConfigBool("UseLiveTransactions") == true)
                    sURL.Append(useIntegratedCheckout ? AppLogic.AppConfig("PayPal.Express.IntegratedCheckout.LiveURL") : AppLogic.AppConfig("PayPal.Express.LiveURL"));
                else
					sURL.Append(useIntegratedCheckout ? AppLogic.AppConfig("PayPal.Express.IntegratedCheckout.SandboxURL") : AppLogic.AppConfig("PayPal.Express.SandboxURL"));

                sURL.Append(useIntegratedCheckout ? "?token=" : "?cmd=_express-checkout&token=");
                sURL.Append(ECResponse.Token);

                if (boolBypassOrderReview)
                    sURL.Append("&useraction=commit");

				// Set active payment method to PayPalExpress
				DB.ExecuteSQL(string.Format("UPDATE Address SET PaymentMethodLastUsed={0} WHERE AddressID={1}",
					DB.SQuote(AppLogic.ro_PMPayPalExpress), cart.ThisCustomer.PrimaryBillingAddressID));
            }
            else
            {
                ErrorMessage e = new ErrorMessage(HttpContext.Current.Server.HtmlEncode(result));
                sURL.Append("shoppingcart.aspx?resetlinkback=1&errormsg=");
                sURL.Append(e.MessageId);
            }

            return sURL.ToString();
        }

        public static String GetECDetails(String PayPalToken, int CustomerID)
        {
            PayPalAPISoapBinding IPayPalRefund;
            PayPalAPIAASoapBinding IPayPal;
            PayPalController.GetPaypalRequirements(out IPayPalRefund, out IPayPal);
            String payerId = String.Empty;
            String addressStatus = String.Empty;

            GetExpressCheckoutDetailsReq ECRequest = new GetExpressCheckoutDetailsReq();
            GetExpressCheckoutDetailsRequestType varECRequest = new GetExpressCheckoutDetailsRequestType();
            GetExpressCheckoutDetailsResponseType ECResponse = new GetExpressCheckoutDetailsResponseType();
            GetExpressCheckoutDetailsResponseDetailsType varECResponse = new GetExpressCheckoutDetailsResponseDetailsType();

            ECRequest.GetExpressCheckoutDetailsRequest = varECRequest;
            ECResponse.GetExpressCheckoutDetailsResponseDetails = varECResponse;

            varECRequest.Token = PayPalToken;
            varECRequest.Version = API_VER;

            ECResponse = IPayPal.GetExpressCheckoutDetails(ECRequest);

            PayerInfoType PayerInfo = ECResponse.GetExpressCheckoutDetailsResponseDetails.PayerInfo;

            payerId = PayerInfo.PayerID;
            if (String.IsNullOrEmpty(payerId))
            {  // If we don't have a PayerID the transaction must be aborted.
                return String.Empty;
            }

            addressStatus = PayerInfo.Address.AddressStatus.ToString();
            bool requireConfirmedAddress = AppLogic.AppConfigBool("PayPal.Express.AVSRequireConfirmedAddress");
            //Is address AVS Confirmed or Unconfirmed or None?
            if (requireConfirmedAddress && !addressStatus.Equals("Confirmed", StringComparison.OrdinalIgnoreCase))
                return "AVSFAILED";


            Customer ThisCustomer = new Customer(CustomerID, true);
            if (!ThisCustomer.IsRegistered)
            {
                ThisCustomer.UpdateCustomer(
                    /*CustomerLevelID*/ null,
                    /*EMail*/ PayerInfo.Payer,
                    /*SaltedAndHashedPassword*/ null,
                    /*SaltKey*/ null,
                    /*DateOfBirth*/ null,
                    /*Gender*/ null,
                    /*FirstName*/ PayerInfo.PayerName.FirstName,
                    /*LastName*/ PayerInfo.PayerName.LastName,
                    /*Notes*/ null,
                    /*SkinID*/ null,
                    /*Phone*/ CommonLogic.IIF(PayerInfo.Address.Phone != null, PayerInfo.Address.Phone, ""),
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

                String BillingPhone = String.Empty;
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
                    BillingPhone = UseBillingAddress.Phone;
                }

                // Anonymous Paypal Express Checkout order, use Paypal's address
                Address ShippingAddress = new Address();
                ShippingAddress.CustomerID = CustomerID;
                ShippingAddress.PaymentMethodLastUsed = PM;
                String[] NameArray = PayerInfo.Address.Name.Split(new string[1] { " " }, 2, StringSplitOptions.RemoveEmptyEntries);
                String FirstName = String.Empty;
                String LastName = String.Empty;
                if (NameArray.Length > 1)
                {
                    FirstName = NameArray[0];
                    LastName = NameArray[1];
                }
                else
                {
                    LastName = PayerInfo.Address.Name;
                }
                ShippingAddress.FirstName = FirstName;
                ShippingAddress.LastName = LastName;
                ShippingAddress.Address1 = PayerInfo.Address.Street1;
				ShippingAddress.Address2 = PayerInfo.Address.Street2;
				ShippingAddress.Phone = String.IsNullOrEmpty(PayerInfo.Address.Phone) ? BillingPhone : PayerInfo.Address.Phone;
                ShippingAddress.City = PayerInfo.Address.CityName;
                ShippingAddress.State = AppLogic.GetStateAbbreviation(PayerInfo.Address.StateOrProvince, PayerInfo.Address.CountryName);
                ShippingAddress.Zip = PayerInfo.Address.PostalCode;
                ShippingAddress.Country = PayerInfo.Address.CountryName;
                ShippingAddress.InsertDB();

                ShippingAddress.MakeCustomersPrimaryAddress(AddressTypes.Shipping);

                Address BillingAddress = new Address();
                BillingAddress.CustomerID = CustomerID;
                BillingAddress.PaymentMethodLastUsed = PM;
                BillingAddress.FirstName = PayerInfo.PayerName.FirstName;
                BillingAddress.LastName = PayerInfo.PayerName.LastName;
                BillingAddress.Address1 = PayerInfo.Address.Street1;
                BillingAddress.Address2 = PayerInfo.Address.Street2;
				BillingAddress.Phone = String.IsNullOrEmpty(PayerInfo.Address.Phone) ? BillingPhone : PayerInfo.Address.Phone;
                BillingAddress.City = PayerInfo.Address.CityName;
                BillingAddress.State = AppLogic.GetStateAbbreviation(PayerInfo.Address.StateOrProvince, PayerInfo.Address.CountryName);
                BillingAddress.Zip = PayerInfo.Address.PostalCode;
                BillingAddress.Country = PayerInfo.Address.CountryName;
                BillingAddress.InsertDB();

                BillingAddress.MakeCustomersPrimaryAddress(AddressTypes.Billing);

            }
            else
            { // A registered and logged in Customer

                Address UseBillingAddress = new Address();
                UseBillingAddress.LoadByCustomer(ThisCustomer.CustomerID, ThisCustomer.PrimaryBillingAddressID, AddressTypes.Billing);
                UseBillingAddress.ClearCCInfo();
                if (UseBillingAddress.PaymentMethodLastUsed != AppLogic.ro_PMPayPalExpress
                    && UseBillingAddress.PaymentMethodLastUsed != AppLogic.ro_PMPayPalExpressMark)
                {
                    UseBillingAddress.PaymentMethodLastUsed = AppLogic.ro_PMPayPalExpress;
                    if (!ThisCustomer.MasterShouldWeStoreCreditCardInfo)
                    {
                        UseBillingAddress.ClearCCInfo();
                    }
                    UseBillingAddress.UpdateDB();
                }

                string sql = String.Format("select top 1 AddressID as N from Address where Address1={0} and Address2={1} and City={2} and State={3} and Zip={4} and Country={5} and CustomerID={6}",
                    DB.SQuote(PayerInfo.Address.Street1), DB.SQuote(PayerInfo.Address.Street2), DB.SQuote(PayerInfo.Address.CityName), DB.SQuote(PayerInfo.Address.StateOrProvince),
                    DB.SQuote(PayerInfo.Address.PostalCode), DB.SQuote(PayerInfo.Address.CountryName), CustomerID);
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
                    ShippingAddress.PaymentMethodLastUsed = AppLogic.ro_PMPayPalExpress;
                    String[] NameArray = PayerInfo.Address.Name.Split(new string[1] { " " }, 2, StringSplitOptions.RemoveEmptyEntries);
                    String FirstName = String.Empty;
                    String LastName = String.Empty;
                    if (NameArray.Length > 1)
                    {
                        FirstName = NameArray[0];
                        LastName = NameArray[1];
                    }
                    else
                    {
                        LastName = PayerInfo.Address.Name;
                    }
                    ShippingAddress.FirstName = FirstName;
                    ShippingAddress.LastName = LastName;
                    ShippingAddress.Address1 = PayerInfo.Address.Street1;
                    ShippingAddress.Address2 = PayerInfo.Address.Street2;
                    ShippingAddress.Phone = CommonLogic.IIF(PayerInfo.Address.Phone != null, PayerInfo.Address.Phone, UseBillingAddress.Phone);
                    ShippingAddress.City = PayerInfo.Address.CityName;
                    ShippingAddress.State = AppLogic.GetStateAbbreviation(PayerInfo.Address.StateOrProvince, PayerInfo.Address.CountryName);
                    ShippingAddress.Zip = PayerInfo.Address.PostalCode;
                    ShippingAddress.Country = PayerInfo.Address.CountryName;
                    ShippingAddress.InsertDB();

                    ShippingAddress.MakeCustomersPrimaryAddress(AddressTypes.Shipping);
                }
                else
                { // Exists already
                    ShippingAddress.LoadFromDB(ExistingAddressID);
                    ShippingAddress.MakeCustomersPrimaryAddress(AddressTypes.Shipping);
                }
            }

            return payerId;
        }

        public static String ProcessEC(ShoppingCart cart, decimal OrderTotal, int OrderNumber, String PayPalToken, String PayerID, String TransactionMode, out String AuthorizationResult, out String AuthorizationTransID)
        {
            PayPalAPISoapBinding IPayPalRefund;
            PayPalAPIAASoapBinding IPayPal;
            PayPalController.GetPaypalRequirements(out IPayPalRefund, out IPayPal);
            String result = String.Empty;

            AuthorizationResult = String.Empty;
            AuthorizationTransID = String.Empty;

            DoExpressCheckoutPaymentReq ECRequest = new DoExpressCheckoutPaymentReq();
            DoExpressCheckoutPaymentRequestType varECRequest = new DoExpressCheckoutPaymentRequestType();
            DoExpressCheckoutPaymentRequestDetailsType varECRequestDetails = new DoExpressCheckoutPaymentRequestDetailsType();
            DoExpressCheckoutPaymentResponseType ECResponse = new DoExpressCheckoutPaymentResponseType();
            DoExpressCheckoutPaymentResponseDetailsType varECResponse = new DoExpressCheckoutPaymentResponseDetailsType();

            ECRequest.DoExpressCheckoutPaymentRequest = varECRequest;
            varECRequest.DoExpressCheckoutPaymentRequestDetails = varECRequestDetails;
            ECResponse.DoExpressCheckoutPaymentResponseDetails = varECResponse;

            varECRequestDetails.Token = PayPalToken;
            varECRequestDetails.PayerID = PayerID;

            varECRequestDetails.PaymentAction = PaymentActionCodeType.Authorization;
            if (TransactionMode == AppLogic.ro_TXModeAuthCapture || AppLogic.AppConfigBool("PayPal.ForceCapture") || PayPalController.GetAppropriateExpressType() == ExpressAPIType.PayPalAcceleratedBording)
            {
                varECRequestDetails.PaymentAction = PaymentActionCodeType.Sale;
            }
            varECRequestDetails.PaymentActionSpecified = true;

            PaymentDetailsType ECPaymentDetails = new PaymentDetailsType();
            BasicAmountType ECPaymentOrderTotal = new BasicAmountType();
            ECPaymentOrderTotal.Value = Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal);
            ECPaymentOrderTotal.currencyID =
                (CurrencyCodeType)Enum.Parse(typeof(CurrencyCodeType), AppLogic.AppConfig("Localization.StoreCurrency"), true);
            ECPaymentDetails.InvoiceID = OrderNumber.ToString();
            ECPaymentDetails.Custom = cart.ThisCustomer.CustomerID.ToString();
            ECPaymentDetails.ButtonSource = PayPalController.BN + "_EC_US";
            ECPaymentDetails.NotifyURL = AppLogic.GetStoreHTTPLocation(true) + AppLogic.AppConfig("PayPal.NotificationURL");

            varECRequest.Version = API_VER;

            // for eBay support with Auction Items
            if (PayPalController.GetFirstAuctionSite(cart.CartItems).ToUpperInvariant() == "EBAY")
            {
                // eBay Auction Items require "Sale"
                varECRequestDetails.PaymentAction = PaymentActionCodeType.Sale;

                decimal ppTotal = 0.0M;
                PayPalItemList ppCart = new PayPalItemList(cart, true);

                PaymentDetailsItemType[] ECItems = new PaymentDetailsItemType[ppCart.Count];
                for (int i = 0; i < ppCart.Count; i++)
                {
                    PayPalItem ppItem = ppCart.Item(i);
                    PaymentDetailsItemType ECItem = new PaymentDetailsItemType();

                    BasicAmountType itemAmount = new BasicAmountType();
                    itemAmount.Value = Localization.CurrencyStringForGatewayWithoutExchangeRate(ppItem.Amount);
                    itemAmount.currencyID = (CurrencyCodeType)Enum.Parse(typeof(CurrencyCodeType), AppLogic.AppConfig("Localization.StoreCurrency"), true);
                    ECItem.Amount = itemAmount;
                    ECItem.Quantity = ppItem.Quantity.ToString();
                    ECItem.Name = ppItem.Name;

                    if (ppItem.Site.ToUpperInvariant() == "EBAY")
                    {
                        EbayItemPaymentDetailsItemType EbayItem = new EbayItemPaymentDetailsItemType();
                        if (!String.IsNullOrEmpty(ppItem.TransactionID))
                        {
                            EbayItem.AuctionTransactionId = ppItem.TransactionID;
                        }
                        EbayItem.ItemNumber = ppItem.ItemNumber;
                        ECItem.EbayItemPaymentDetailsItem = EbayItem;
                    }

                    ECItems[i] = ECItem;
                }


                if (ppCart.ShippingAmount > 0)
                {
                    BasicAmountType itemAmount = new BasicAmountType();
                    itemAmount.Value = Localization.CurrencyStringForGatewayWithoutExchangeRate(ppCart.ShippingAmount);
                    itemAmount.currencyID = (CurrencyCodeType)Enum.Parse(typeof(CurrencyCodeType), AppLogic.AppConfig("Localization.StoreCurrency"), true);
                    ECPaymentDetails.ShippingTotal = itemAmount;
                    ppTotal += Localization.ParseNativeDecimal(itemAmount.Value);
                }

                if (ppCart.TaxAmount > 0)
                {
                    BasicAmountType itemAmount = new BasicAmountType();
                    itemAmount.Value = Localization.CurrencyStringForGatewayWithoutExchangeRate(ppCart.TaxAmount);
                    itemAmount.currencyID = (CurrencyCodeType)Enum.Parse(typeof(CurrencyCodeType), AppLogic.AppConfig("Localization.StoreCurrency"), true);
                    ECPaymentDetails.TaxTotal = itemAmount;
                    ppTotal += Localization.ParseNativeDecimal(itemAmount.Value);
                }

                BasicAmountType itemTotal = new BasicAmountType();
                itemTotal.Value = Localization.CurrencyStringForGatewayWithoutExchangeRate(ppCart.ItemTotal);
                itemTotal.currencyID = (CurrencyCodeType)Enum.Parse(typeof(CurrencyCodeType), AppLogic.AppConfig("Localization.StoreCurrency"), true);
                ECPaymentDetails.ItemTotal = itemTotal;
                ppTotal += Localization.ParseNativeDecimal(itemTotal.Value);

                ECPaymentDetails.PaymentDetailsItem = ECItems;

                ECPaymentOrderTotal.Value = Localization.CurrencyStringForGatewayWithoutExchangeRate(ppTotal);

            }

            ECPaymentDetails.OrderTotal = ECPaymentOrderTotal;

            List<PaymentDetailsType> ECPaymentDetailsList = new List<PaymentDetailsType>();

            ECPaymentDetailsList.Add(ECPaymentDetails);

            varECRequestDetails.PaymentDetails = ECPaymentDetailsList.ToArray();

            ECResponse = IPayPal.DoExpressCheckoutPayment(ECRequest);

            if (ECResponse.Ack.ToString().StartsWith("success", StringComparison.InvariantCultureIgnoreCase))
            {
                AuthorizationTransID = CommonLogic.IIF(varECRequestDetails.PaymentAction == PaymentActionCodeType.Sale, "CAPTURE=", "AUTH=") + ECResponse.DoExpressCheckoutPaymentResponseDetails.PaymentInfo[0].TransactionID;
                result = AppLogic.ro_OK;
                AuthorizationResult = ECResponse.Ack.ToString();
            }
            else
            {
                if (ECResponse.Errors != null)
                {
                    bool first = true;
                    for (int ix = 0; ix < ECResponse.Errors.Length; ix++)
                    {
                        if (!first)
                        {
                            AuthorizationResult += ", ";
                        }
                        AuthorizationResult += ECResponse.Errors[ix].LongMessage;
                        first = false;
                    }
                }
                result = AuthorizationResult;
            }

            return result;
        }

        public static String MakeECRecurringProfile(ShoppingCart cart, int orderNumber, String payPalToken, String payerID, DateTime nextRecurringShipDate)
        {
            PayPalAPISoapBinding IPayPalRefund;
            PayPalAPIAASoapBinding IPayPal;
            PayPalController.GetPaypalRequirements(out IPayPalRefund, out IPayPal);
            String result = String.Empty;

            CreateRecurringPaymentsProfileReq ECRecurringRequest = new CreateRecurringPaymentsProfileReq();
            CreateRecurringPaymentsProfileRequestType varECRequest = new CreateRecurringPaymentsProfileRequestType();
            CreateRecurringPaymentsProfileRequestDetailsType varECRequestDetails = new CreateRecurringPaymentsProfileRequestDetailsType();
            CreateRecurringPaymentsProfileResponseType ECRecurringResponse = new CreateRecurringPaymentsProfileResponseType();
            
            //Re-Use the Internal Gateway Recurring Billing logic for calculating how much of the order is recurring
            ShoppingCart cartRecur = new ShoppingCart(cart.ThisCustomer.SkinID, cart.ThisCustomer, CartTypeEnum.RecurringCart, orderNumber, false);
            Decimal CartTotalRecur = Decimal.Round(cartRecur.Total(true), 2, MidpointRounding.AwayFromZero);
            Decimal RecurringAmount = CartTotalRecur - CommonLogic.IIF(cartRecur.Coupon.CouponType == CouponTypeEnum.GiftCard, CommonLogic.IIF(CartTotalRecur < cartRecur.Coupon.DiscountAmount, CartTotalRecur, cartRecur.Coupon.DiscountAmount), 0);

            DateIntervalTypeEnum ecRecurringIntervalType = cartRecur.CartItems[0].RecurringIntervalType;    //We currently only support 1 interval per recurring order, so grabbing the first as a default should be safe
            int ecRecurringInterval = cartRecur.CartItems[0].RecurringInterval;

            BasicAmountType ecRecurringAmount = new BasicAmountType();
            ecRecurringAmount.currencyID = (CurrencyCodeType)Enum.Parse(typeof(CurrencyCodeType), AppLogic.AppConfig("Localization.StoreCurrency"), true);
            ecRecurringAmount.Value = RecurringAmount.ToString();

            BillingPeriodDetailsType varECSchedulePaymentDetails = GetECRecurringPeriodDetails(ecRecurringIntervalType, ecRecurringInterval);
            varECSchedulePaymentDetails.Amount = ecRecurringAmount;
            varECSchedulePaymentDetails.TotalBillingCyclesSpecified = false;

            ScheduleDetailsType varECSchedule = new ScheduleDetailsType();
            //Need a better description, but it must match the one sent in StartEC
            varECSchedule.Description = "Recurring order created on " + System.DateTime.Now.ToShortDateString() +" from " + AppLogic.AppConfig("StoreName"); 
            varECSchedule.MaxFailedPayments = 0;    //Cancel the order if a recurrence fails
            varECSchedule.MaxFailedPaymentsSpecified = true;
            varECSchedule.AutoBillOutstandingAmount = AutoBillType.NoAutoBill;
            varECSchedule.AutoBillOutstandingAmountSpecified = true;
            varECSchedule.PaymentPeriod = varECSchedulePaymentDetails;

            RecurringPaymentsProfileDetailsType varECProfileDetails = new RecurringPaymentsProfileDetailsType();
            varECProfileDetails.SubscriberName = cart.ThisCustomer.FirstName + " " + cart.ThisCustomer.LastName;
            varECProfileDetails.BillingStartDate = nextRecurringShipDate;

            varECRequestDetails.ScheduleDetails = varECSchedule;
            varECRequestDetails.Token = payPalToken;
            varECRequestDetails.RecurringPaymentsProfileDetails = varECProfileDetails;

            if (cart.IsAllDownloadComponents())
            {
                PaymentDetailsItemType varECPaymentDetails = new PaymentDetailsItemType();
                varECPaymentDetails.ItemCategory = ItemCategoryType.Digital;
                varECPaymentDetails.ItemCategorySpecified = true;

                List<PaymentDetailsItemType> ECPaymentDetailsList = new List<PaymentDetailsItemType>();

                ECPaymentDetailsList.Add(varECPaymentDetails);

                varECRequestDetails.PaymentDetailsItem = ECPaymentDetailsList.ToArray();
            }

            varECRequest.Version = API_VER;
            varECRequest.CreateRecurringPaymentsProfileRequestDetails = varECRequestDetails;

            ECRecurringRequest.CreateRecurringPaymentsProfileRequest = varECRequest;

            ECRecurringResponse = IPayPal.CreateRecurringPaymentsProfile(ECRecurringRequest);
            
            if (ECRecurringResponse.Ack.ToString().StartsWith("success", StringComparison.InvariantCultureIgnoreCase))
            {
                result = AppLogic.ro_OK;
            }
            else
            {
                if (ECRecurringResponse.Errors != null)
                {
                    bool first = true;
                    for (int ix = 0; ix < ECRecurringResponse.Errors.Length; ix++)
                    {
                        if (!first)
                        {
                            result += ", ";
                        }
                        result += ECRecurringResponse.Errors[ix].LongMessage;
                        first = false;
                    }
                }
            }
            
            //Log the transaction
            OrderTransactionCollection ecRecurringOrderTransaction = new OrderTransactionCollection(orderNumber);
            ecRecurringOrderTransaction.AddTransaction("PayPal Express Checkout Recurring Profile Creation", 
                ECRecurringRequest.ToString(), 
                result, 
                payerID,    //PNREF = payerID
                (ECRecurringResponse.CreateRecurringPaymentsProfileResponseDetails.ProfileID == null ? "No ProfileID provided" : ECRecurringResponse.CreateRecurringPaymentsProfileResponseDetails.ProfileID),    //Code = ProfileID
                AppLogic.ro_PMPayPalExpress, 
                null, 
                RecurringAmount);

            return result;
        }

        public static String CancelECRecurringProfile(int OriginalOrderNumber)
        {
            PayPalAPISoapBinding IPayPalRefund;
            PayPalAPIAASoapBinding IPayPal;
            PayPalController.GetPaypalRequirements(out IPayPalRefund, out IPayPal);
            String profileID = String.Empty;
            String result = String.Empty;

            profileID = GetPPECProfileID(OriginalOrderNumber);

            if (profileID != String.Empty)
            {
                ManageRecurringPaymentsProfileStatusReq ECRecurringCancelRequest = new ManageRecurringPaymentsProfileStatusReq();
                ManageRecurringPaymentsProfileStatusRequestType varECRecurringRequest = new ManageRecurringPaymentsProfileStatusRequestType();
                ManageRecurringPaymentsProfileStatusRequestDetailsType varECRecurringRequestDetails = new ManageRecurringPaymentsProfileStatusRequestDetailsType();
                ManageRecurringPaymentsProfileStatusResponseType varECRecurringResponse = new ManageRecurringPaymentsProfileStatusResponseType();

                varECRecurringRequestDetails.Action = StatusChangeActionType.Cancel;
                varECRecurringRequestDetails.ProfileID = profileID;

                varECRecurringRequest.ManageRecurringPaymentsProfileStatusRequestDetails = varECRecurringRequestDetails;
                varECRecurringRequest.Version = API_VER;

                ECRecurringCancelRequest.ManageRecurringPaymentsProfileStatusRequest = varECRecurringRequest;

                varECRecurringResponse = IPayPal.ManageRecurringPaymentsProfileStatus(ECRecurringCancelRequest);

                if (varECRecurringResponse.Ack.ToString().StartsWith("success", StringComparison.InvariantCultureIgnoreCase))
                {
                    result = AppLogic.ro_OK;
                }
                else
                {
                    if (varECRecurringResponse.Errors != null)
                    {
                        bool first = true;
                        for (int ix = 0; ix < varECRecurringResponse.Errors.Length; ix++)
                        {
                            if (!first)
                            {
                                result += ", ";
                            }
                            result += varECRecurringResponse.Errors[ix].LongMessage;
                            first = false;
                        }
                    }
                }
            }
            else
            {
                result = "No matching ProfileID found for that order number";

                SysLog.LogMessage("An attempt was made to cancel a PayPal express recurring order with no matching ProfileID", 
                    "Original order ID was: " + OriginalOrderNumber.ToString(), 
                    MessageTypeEnum.Informational, 
                    MessageSeverityEnum.Alert);
            }

            return result;
        }

        public static void GetPaypalRequirements(out PayPalAPISoapBinding IPayPalRefund, out PayPalAPIAASoapBinding IPayPal)
        {
            IPayPal = new PayPalAPIAASoapBinding();
            IPayPalRefund = new PayPalAPISoapBinding();

            if (AppLogic.AppConfigBool("UseLiveTransactions"))
            {
                IPayPal.Url = AppLogic.AppConfig("PayPal.API.LiveURL");
            }
            else
            {
                IPayPal.Url = AppLogic.AppConfig("PayPal.API.TestURL");
            }
            IPayPalRefund.Url = IPayPal.Url;

            IPayPal.UserAgent = HttpContext.Current.Request.UserAgent;
            IPayPalRefund.UserAgent = IPayPal.UserAgent;

            UserIdPasswordType PayPalUser = new UserIdPasswordType();
            if (PayPalController.GetAppropriateExpressType() == ExpressAPIType.PayPalAcceleratedBording)
            {
                PayPalUser.Subject = AppLogic.AppConfig("PayPal.API.AcceleratedBoardingEmailAddress");
            }
            else
            {
                PayPalUser.Username = AppLogic.AppConfig("PayPal.API.Username");
                PayPalUser.Password = AppLogic.AppConfig("PayPal.API.Password");
                PayPalUser.Signature = AppLogic.AppConfig("PayPal.API.Signature");

                //Subject should be the Sellers e-mail address (if you are using 3-part API calls) with the correct account permissions. You also have 
                //set up permissions for this e-mail address for the "type" of transaction you want to allow.
                //This access changes are made in the Sandbox.
                //The name of the entity on behalf of which this profile is issuing calls
                //This is for Third-Party access
                // You have to set up Virtual Terminals and complete the Billing Agreement in the Sandbox before you can make Direct Payments
                PayPalUser.Subject = AppLogic.AppConfig("PayPal.API.MerchantEMailAddress");
            }



            CustomSecurityHeaderType CSecHeaderType = new CustomSecurityHeaderType();
            CSecHeaderType.Credentials = PayPalUser;
            CSecHeaderType.MustUnderstand = true;

            IPayPal.RequesterCredentials = CSecHeaderType;
            IPayPalRefund.RequesterCredentials = CSecHeaderType;
        }
        
        public static void Log(String value, String source)
        {
            DB.ExecuteSQL(String.Format("insert into ErrorLog (errorDt, source, errormsg) values (getdate(), {1}, {0})", DB.SQuote(value), DB.SQuote(source)));
        }

        public static BillingPeriodDetailsType GetECRecurringPeriodDetails(DateIntervalTypeEnum ecRecurringIntervalType, int ecRecurringInterval)
        {
            BillingPeriodDetailsType ecBillingPeriod = new BillingPeriodDetailsType();

            switch (ecRecurringIntervalType)
            {
                case DateIntervalTypeEnum.Day:
                    ecBillingPeriod.BillingFrequency = ecRecurringInterval;
                    ecBillingPeriod.BillingPeriod = BillingPeriodType.Day;
                    break;
                case DateIntervalTypeEnum.Week:
                    ecBillingPeriod.BillingFrequency = ecRecurringInterval;
                    ecBillingPeriod.BillingPeriod = BillingPeriodType.Week;
                    break;
                case DateIntervalTypeEnum.Month:
                    ecBillingPeriod.BillingFrequency = ecRecurringInterval;
                    ecBillingPeriod.BillingPeriod = BillingPeriodType.Month;
                    break;
                case DateIntervalTypeEnum.Year:
                    ecBillingPeriod.BillingFrequency = ecRecurringInterval;
                    ecBillingPeriod.BillingPeriod = BillingPeriodType.Year;
                    break;
                case DateIntervalTypeEnum.Weekly:
                    ecBillingPeriod.BillingFrequency = 1;
                    ecBillingPeriod.BillingPeriod = BillingPeriodType.Week;
                    break;
                case DateIntervalTypeEnum.BiWeekly:
                    ecBillingPeriod.BillingFrequency = 1;
                    ecBillingPeriod.BillingPeriod = BillingPeriodType.SemiMonth;
                    break;
                case DateIntervalTypeEnum.EveryFourWeeks:
                    ecBillingPeriod.BillingFrequency = 4;
                    ecBillingPeriod.BillingPeriod = BillingPeriodType.Week;
                    break;
                case DateIntervalTypeEnum.Monthly:
                    ecBillingPeriod.BillingFrequency = 1;
                    ecBillingPeriod.BillingPeriod = BillingPeriodType.Month;
                    break;
                case DateIntervalTypeEnum.Quarterly:
                    ecBillingPeriod.BillingFrequency = 3;
                    ecBillingPeriod.BillingPeriod = BillingPeriodType.Month;
                    break;
                case DateIntervalTypeEnum.SemiYearly:
                    ecBillingPeriod.BillingFrequency = 6;
                    ecBillingPeriod.BillingPeriod = BillingPeriodType.Month;
                    break;
                case DateIntervalTypeEnum.Yearly:
                    ecBillingPeriod.BillingFrequency = 1;
                    ecBillingPeriod.BillingPeriod = BillingPeriodType.Year;
                    break;
                default:    //Default to monthly like we do elsewhere
                    ecBillingPeriod.BillingFrequency = ecRecurringInterval;
                    ecBillingPeriod.BillingPeriod = BillingPeriodType.Month;
                    break;
            }

            return ecBillingPeriod;
        }

        public static string GetPPECProfileID(int OriginalOrderNumber)
        {
            String profileID = string.Empty;

            String profileIDSql = "SELECT TOP 1 Code FROM OrderTransaction WHERE OrderNumber = @OrderNumber AND TransactionCommand = @TransactionCommand AND PaymentMethod = @PaymentMethod";

            SqlParameter[] profileIDParams = { new SqlParameter("@OrderNumber", OriginalOrderNumber),
                                                 new SqlParameter("@TransactionCommand", "CreateRecurringPaymentsProfileReq"),
                                                 new SqlParameter("@PaymentMethod", AppLogic.ro_PMPayPalExpress) };

            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS(profileIDSql, profileIDParams, conn))
                {
                    while (rs.Read())
                    {
                        profileID = DB.RSField(rs, "Code");
                    }
                }
            }

            return profileID;
        }

		public static string GetExpressCheckoutIntegratedScript(bool isCartPage)
		{
			string expressCheckoutScript = @"<script>
												(function (d, s, id) {
													var js, ref = d.getElementsByTagName(s)[0];
													if (!d.getElementById(id)) {
														js = d.createElement(s); js.id = id; js.async = true;
														js.src = '//www.paypalobjects.com/js/external/paypal.js';
														ref.parentNode.insertBefore(js, ref);
													}
												}(document, 'script', 'paypal-js'));

												if (parent.location !== window.location) {
													top.window.location = window.location;
												}
											</script>";

			if(isCartPage)
				expressCheckoutScript += @"<script>
												function startCheckout() {
													PAYPAL.apps.Checkout.startFlow();
													document.forms[0].target='PPFrame';
													return true;
												}
											</script>";

			return expressCheckoutScript;
		}
    }

    public enum ExpressAPIType
    {
        PayFlowPro,
        PayPalExpress,
        PayPalAcceleratedBording,
        NoValidAPIType
    }    

    public class PayPalItemList : System.Collections.CollectionBase
    {
        public decimal ShippingAmount;
        public decimal TaxAmount;
        public decimal ItemTotal;

        public PayPalItemList(ShoppingCart cart, bool IncludeDiscounts)
        {
            bool bVATEnabled = AppLogic.AppConfigBool("VAT.Enabled");

            for (int ci = 0; ci < cart.CartItems.Count; ci++)
            {
                CartItem c = cart.CartItems[ci];
                if (c.Quantity > 0)
                {
                    PayPalItem ppi = new PayPalItem();
                    ppi.Name = HttpContext.Current.Server.HtmlEncode(AppLogic.MakeProperObjectName(c.ProductName, c.VariantName, cart.ThisCustomer.LocaleSetting));
                    ppi.Quantity = c.Quantity;

                    decimal PRsingle = c.Price;
                    bool RollIntoQtyOne = false;

                    decimal TaxRate = 0.0M;

                    TaxRate = CommonLogic.IIF(bVATEnabled && c.IsTaxable, (cart.ThisCustomer.TaxRate(c.TaxClassID) / 100.0M), 0.0M);

                    int Q = c.Quantity;
                    decimal PR = c.Price * (decimal)Q;
                    decimal VAT = 0.0M;

                    PRsingle += Decimal.Round(c.Price * TaxRate, 2, MidpointRounding.AwayFromZero);

                    if (AppLogic.AppConfigBool("VAT.RoundPerItem"))
                    {
                        VAT = Decimal.Round(c.Price * TaxRate, 2, MidpointRounding.AwayFromZero) * (decimal)Q;
                    }
                    else
                    {
                        VAT = Decimal.Round(PR * TaxRate, 2, MidpointRounding.AwayFromZero);
                        if (VAT > 0.0M && Q > 1)
                        {
                            RollIntoQtyOne = true;
                        }
                    }
                    PR += CommonLogic.IIF(bVATEnabled, VAT, 0.0M);

                    int ActiveDID = 0;
                    Decimal DIDPercent = 0.0M;
                    QuantityDiscount.QuantityDiscountType fixedPriceDID = QuantityDiscount.QuantityDiscountType.Percentage;
                    if (QuantityDiscount.CustomerLevelAllowsQuantityDiscounts(cart.ThisCustomer.CustomerLevelID))
                    {
                        DIDPercent = QuantityDiscount.GetQuantityDiscountTablePercentageForLineItem(c, out fixedPriceDID);
                        if (DIDPercent != 0.0M)
                        {
                            RollIntoQtyOne = true;
                            if (fixedPriceDID == QuantityDiscount.QuantityDiscountType.FixedAmount)
                            {
                                if (Currency.GetDefaultCurrency() == cart.ThisCustomer.CurrencySetting)
                                {
                                    PR = (PR - DIDPercent);
                                }
                                else
                                {
                                    DIDPercent = Decimal.Round(Currency.Convert(DIDPercent, Localization.StoreCurrency(), cart.ThisCustomer.CurrencySetting), 2, MidpointRounding.AwayFromZero);
                                    PR = (PR - DIDPercent);
                                }
                            }
                            else
                            {
                                PR = PR * ((100.0M - DIDPercent) / 100.0M);
                            }
                        }
                    }

                    ppi.Amount = PRsingle;

                    if (RollIntoQtyOne)
                    { // must combine into Qty 1 item if there is a qty discount due to rounding issues
                        ppi.Name += ": Qty " + c.Quantity.ToString();
                        if (ActiveDID != 0 && DIDPercent != 0.0M)
                        {
                            ppi.Name += " (" + Localization.CurrencyStringForDBWithoutExchangeRate(DIDPercent) + AppLogic.GetString("shoppingcart.cs.13", cart.SkinID, cart.ThisCustomer.LocaleSetting) + ")";
                        }
                        ppi.Quantity = 1;
                        ppi.Amount = PR;
                    }

                    // check if auction item (has marketworks extensiondata)
                    if (c.IsAuctionItem)
                    {
                        String Marketworks = DB.GetSqlS("select extensiondata S from product where productid=" + c.ProductID.ToString());
                        XmlDocument docExtensionData = new XmlDocument();

                        try
                        {
                            docExtensionData.LoadXml(Marketworks);
                            XmlNode node = docExtensionData.SelectSingleNode("/Marketworks/Marketplace");
                            if (node != null)
                            {
                                ppi.Site = node.InnerText;
                            }

                            node = docExtensionData.SelectSingleNode("/Marketworks/MarketplaceItemID");
                            if (node != null)
                            {
                                ppi.ItemNumber = node.InnerText;
                            }

                            node = docExtensionData.SelectSingleNode("/Marketworks/MarketplaceTransactionID");
                            if (node != null)
                            {
                                ppi.TransactionID = node.InnerText;
                            }
                        }
                        catch { }

                        ppi.BuyerID = DB.GetSqlS("select extensiondata S from customer where customerid=" + cart.ThisCustomer.CustomerID.ToString());
                    }

                    List.Add(ppi);
                    ItemTotal += ppi.Quantity * Localization.ParseNativeDecimal(Localization.CurrencyStringForGatewayWithoutExchangeRate(ppi.Amount));
                }
            }

            // Order Options
            if (cart.OrderOptions.Count > 0)
            {
                bool VATEnabled = AppLogic.AppConfigBool("VAT.Enabled");
                bool VATOn = (VATEnabled && cart.ThisCustomer.VATSettingReconciled == VATSettingEnum.ShowPricesInclusiveOfVAT);

                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS("Select * from orderoption  with (NOLOCK)  where orderoptionid in (" + CommonLogic.BuildCommaStringFromList(cart.OrderOptionsIDs) + ")", con))
                    {
                        while (rs.Read())
                        {
                            string optionName = DB.RSField(rs, "Name");
                            decimal optionCost = DB.RSFieldDecimal(rs, "Cost");
                            
                            decimal VAT = 0.0M;
                            if (VATOn)
                            {
                                VAT = Decimal.Round(optionCost * (cart.ThisCustomer.TaxRate(DB.RSFieldInt(rs, "TaxClassID")) / 100.0M), 2, MidpointRounding.AwayFromZero);
                                optionCost += VAT;
                            }

                            PayPalItem ppi = new PayPalItem();
                            ppi.Amount = optionCost;
                            ppi.Name = optionName;
                            ppi.Quantity = 1;
                            List.Add(ppi);
                            ItemTotal += ppi.Quantity * Localization.ParseNativeDecimal(Localization.CurrencyStringForGatewayWithoutExchangeRate(ppi.Amount));
                        }
                    }
                }   
	
            }

            if (IncludeDiscounts)
            {
                // Discounts
                decimal SubTotalWODiscount = cart.SubTotal(false, false, true, true);
                decimal SubTotalWDiscount = cart.SubTotal(true, false, true, true);
                decimal dSavings = SubTotalWODiscount - SubTotalWDiscount;
                dSavings -= 0.001M; // this seems to take care of a rounding issue

                if (dSavings > 0)
                {
                    PayPalItem ppi = new PayPalItem();
                    ppi.Amount = -1.0M * dSavings;
                    ppi.Name = AppLogic.GetString("order.cs.84", 1, cart.ThisCustomer.LocaleSetting);
                    ppi.Quantity = 1;
                    List.Add(ppi);
                    ItemTotal += ppi.Quantity * Localization.ParseNativeDecimal(Localization.CurrencyStringForGatewayWithoutExchangeRate(ppi.Amount));
                }

                // Gift Card
                decimal CartTotal = cart.Total(true);
                decimal NetTotal = CartTotal - CommonLogic.IIF(cart.Coupon.CouponType == CouponTypeEnum.GiftCard, CommonLogic.IIF(CartTotal < cart.Coupon.DiscountAmount, CartTotal, cart.Coupon.DiscountAmount), 0);
                decimal dGiftCardAmount = CartTotal - NetTotal;

                if (dGiftCardAmount > 0.0M)
                {
                    PayPalItem ppi = new PayPalItem();
                    ppi.Amount = -1.0M * dGiftCardAmount;
                    ppi.Name = AppLogic.GetString("order.cs.83", 1, cart.ThisCustomer.LocaleSetting);
                    ppi.Quantity = 1;
                    List.Add(ppi);
                    ItemTotal += ppi.Quantity * Localization.ParseNativeDecimal(Localization.CurrencyStringForGatewayWithoutExchangeRate(ppi.Amount));
                }
            }

            // Shipping
            ShippingAmount = cart.ShippingTotal(true, true);

            // Taxes
            if (!bVATEnabled)
            {
                TaxAmount = cart.TaxTotal();
            }
            else
            {
                TaxAmount = 0.0M;
            }

        }

        public void Add(PayPalItem PPItem)
        {
            List.Add(PPItem);
        }

        public PayPalItem Item(int Index)
        {
            return (PayPalItem)List[Index];
        }
    }

    public class PayPalItem
    {
        public string Name;
        public decimal Amount;
        public int Quantity;
        public string ItemNumber;
        public string TransactionID;
        public string Site;
        public string BuyerID;

        public PayPalItem()
        {
            Name = string.Empty;
            Amount = 0.0M;
            Quantity = 0;
            ItemNumber = string.Empty;
            TransactionID = string.Empty;
            Site = string.Empty;
            BuyerID = string.Empty;
        }

        public PayPalItem(string sName, decimal dAmount, int iQuantity)
        {
            Name = sName;
            Amount = dAmount;
            Quantity = iQuantity;
            ItemNumber = string.Empty;
            TransactionID = string.Empty;
            Site = string.Empty;
            BuyerID = string.Empty;
        }

        public PayPalItem(string sName, decimal dAmount, int iQuantity, string sItemNumber, string sTransactionID, string sSite, string sBuyerID)
        {
            Name = sName;
            Amount = dAmount;
            Quantity = iQuantity;
            ItemNumber = sItemNumber;
            TransactionID = sTransactionID;
            Site = sSite;
            BuyerID = sBuyerID;
        }
    }

}
