// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Web;
using System.Data;
using System.Security;
using System.Text;
using System.Web.SessionState;
using System.IO;
using System.Net;
using System.Xml;
using System.Globalization;
using AspDotNetStorefrontCore;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Threading;

namespace AspDotNetStorefrontGateways.Processors
{
    /// <summary>
    /// Summary description for ManualAuth.
    /// </summary>
    public class Manual : GatewayProcessor
    {
        public Manual() { }

        public override string CaptureOrder(Order o)
        {
            String result = AppLogic.ro_OK;

            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");
            o.CaptureTXCommand = "";
            o.CaptureTXResult = "";
            String TransID = o.AuthorizationPNREF;
            Decimal OrderTotal = o.OrderBalance;

            StringBuilder transactionCommand = new StringBuilder(4096);
            transactionCommand.Append("x_type=CAPTURE");
            transactionCommand.Append("&x_test_request=" + CommonLogic.IIF(useLiveTransactions, "FALSE", "TRUE"));
            transactionCommand.Append("&x_customer_ip=" + CommonLogic.CustomerIpAddress());
            transactionCommand.Append("&x_trans_id=" + TransID);
            transactionCommand.Append("&x_amount=" + Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal));
            
            String rawResponseString = "MANUAL GATEWAY SAID OK";
            o.CaptureTXCommand = transactionCommand.ToString();
            o.CaptureTXResult = rawResponseString;
            return result;
        }

        public override string VoidOrder(int OrderNumber)
        {
            String result = AppLogic.ro_OK;

            DB.ExecuteSQL("update orders set VoidTXCommand=NULL, VoidTXResult=NULL where OrderNumber=" + OrderNumber.ToString());
            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");
            String TransID = String.Empty;
            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS("select AuthorizationPNREF from Orders  with (NOLOCK)  where OrderNumber=" + OrderNumber.ToString(), conn))
                {
                    if (rs.Read())
                    {
                        TransID = DB.RSField(rs, "AuthorizationPNREF");
                    }                    
                }
            }

            StringBuilder transactionCommand = new StringBuilder(4096);
            transactionCommand.Append("x_type=VOID");
            transactionCommand.Append("&x_test_request=" + CommonLogic.IIF(useLiveTransactions, "FALSE", "TRUE"));
            transactionCommand.Append("&x_customer_ip=" + CommonLogic.CustomerIpAddress());
            transactionCommand.Append("&x_trans_id=" + TransID);

            DB.ExecuteSQL("update orders set VoidTXCommand=" + DB.SQuote(transactionCommand.ToString()) + " where OrderNumber=" + OrderNumber.ToString());

            if (TransID.Length == 0 || TransID == "0")
            {
                result = "Invalid or Empty Transaction ID";
            }
            else
            {
                String rawResponseString = "MANUAL GATEWAY SAID OK";
                DB.ExecuteSQL("update orders set VoidTXResult=" + DB.SQuote(rawResponseString) + " where OrderNumber=" + OrderNumber.ToString());
            }
            return result;
        }

        // if RefundAmount == 0.0M, then then ENTIRE order amount will be refunded!
        public override string RefundOrder(int OriginalOrderNumber, int NewOrderNumber, decimal RefundAmount, String RefundReason, Address UseBillingAddress)
        {
            String result = AppLogic.ro_OK;

            DB.ExecuteSQL("update orders set RefundTXCommand=NULL, RefundTXResult=NULL where OrderNumber=" + OriginalOrderNumber.ToString());
            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");
            String TransID = String.Empty;
            String Last4 = String.Empty;
            Decimal OrderTotal = System.Decimal.Zero;

            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS("select AuthorizationPNREF,Last4,OrderTotal from orders   with (NOLOCK)  where OrderNumber=" + OriginalOrderNumber.ToString(), conn))
                {
                    if (rs.Read())
                    {
                        TransID = DB.RSField(rs, "AuthorizationPNREF");
                        Last4 = DB.RSField(rs, "Last4");
                        OrderTotal = DB.RSFieldDecimal(rs, "OrderTotal");
                    }                    
                }
            }

            StringBuilder transactionCommand = new StringBuilder(4096);
            transactionCommand.Append("x_type=CREDIT");
            transactionCommand.Append("&x_test_request=" + CommonLogic.IIF(useLiveTransactions, "FALSE", "TRUE"));
            transactionCommand.Append("&x_trans_id=" + TransID);
            if (RefundAmount == System.Decimal.Zero)
            {
                transactionCommand.Append("&x_amount=" + Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal));
            }
            else
            {
                transactionCommand.Append("&x_amount=" + Localization.CurrencyStringForGatewayWithoutExchangeRate(RefundAmount));
            }
            transactionCommand.Append("&x_customer_ip=" + CommonLogic.CustomerIpAddress());
            transactionCommand.Append("&x_card_num=" + Last4);

            DB.ExecuteSQL("update orders set RefundTXCommand=" + DB.SQuote(transactionCommand.ToString()) + " where OrderNumber=" + OriginalOrderNumber.ToString());

            if (TransID.Length == 0 || TransID == "0")
            {
                result = "Invalid or Empty Transaction ID";
            }
            else if (Last4.Length == 0)
            {
                result = "Credit Card Number (Last4) Not Found or Empty";
            }
            else
            {

                String rawResponseString = "MANUAL GATEWAY SAID OK";
                DB.ExecuteSQL("update orders set RefundTXResult=" + DB.SQuote(rawResponseString) + " where OrderNumber=" + OriginalOrderNumber.ToString());

            }
            return result;
        }

        public override string ProcessCard(int OrderNumber, int CustomerID, Decimal OrderTotal, bool useLiveTransactions, TransactionModeEnum TransactionMode, Address UseBillingAddress, String CardExtraCode, Address UseShippingAddress, String CAVV, String ECI, String XID, out String AVSResult, out String AuthorizationResult, out String AuthorizationCode, out String AuthorizationTransID, out String TransactionCommandOut, out String TransactionResponse)
        {
            String result = AppLogic.ro_OK;
            AVSResult = string.Empty;
            AuthorizationResult = string.Empty;
            AuthorizationCode = string.Empty;
            AuthorizationTransID = string.Empty;
            TransactionCommandOut = string.Empty;
            TransactionResponse = string.Empty;

            if (AppLogic.AppConfigBool("ValidateCreditCardNumbers"))
            {
                CardType cardType = CardType.ParseFromNumber(UseBillingAddress.CardNumber.ToString());
                if (cardType == null)
                {
                    return AppLogic.GetString("checkoutcard_process.aspx.3", Customer.Current.LocaleSetting);
                }

                CreditCardValidator validator = new CreditCardValidator(UseBillingAddress.CardNumber.ToString(), cardType);

                bool isValidCC = validator.Validate();
                if (!isValidCC)
                {
                    return AppLogic.GetString("checkoutcard_process.aspx.3", Customer.Current.LocaleSetting);
                }

                bool isValidCCExpiration = validator.CheckCCExpiration(Convert.ToInt32(UseBillingAddress.CardExpirationMonth), Convert.ToInt32(UseBillingAddress.CardExpirationYear));
                if (!isValidCCExpiration)
                {
                    return AppLogic.GetString("checkoutcard_process.aspx.6", Customer.Current.LocaleSetting);
                }

				bool isValidCVV = AppLogic.AppConfigBool("CardExtraCodeIsOptional") || validator.ValidateCVV(UseBillingAddress.CardNumber.ToString(), CardExtraCode);
                if (!isValidCVV)
                {
                    return AppLogic.GetString("checkoutcard_process.aspx.7", Customer.Current.LocaleSetting);
                }
            }

            StringBuilder transactionCommand = new StringBuilder(4096);

            transactionCommand.Append("x_type=" + CommonLogic.IIF(TransactionMode == TransactionModeEnum.auth, "AUTH_ONLY", "AUTH_CAPTURE"));

            transactionCommand.Append("&x_test_request=" + CommonLogic.IIF(useLiveTransactions, "FALSE", "TRUE"));
            transactionCommand.Append("&x_description=" + HttpContext.Current.Server.UrlEncode(AppLogic.AppConfig("StoreName") + " Order " + OrderNumber.ToString()));

            transactionCommand.Append("&x_amount=" + Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal));
            transactionCommand.Append("&x_card_num=" + UseBillingAddress.CardNumber);
            if (CardExtraCode.Length != 0)
            {
                transactionCommand.Append("&x_card_code=" + CardExtraCode.Trim());
            }

            transactionCommand.Append("&x_exp_date=" + UseBillingAddress.CardExpirationMonth.PadLeft(2, '0') + "/" + UseBillingAddress.CardExpirationYear);
            transactionCommand.Append("&x_phone=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.Phone));
            transactionCommand.Append("&x_fax=");
            transactionCommand.Append("&x_customer_tax_id=");
            transactionCommand.Append("&x_cust_id=" + CustomerID.ToString());
            transactionCommand.Append("&x_invoice_num=" + OrderNumber.ToString());
            transactionCommand.Append("&x_email=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.EMail));
            transactionCommand.Append("&x_customer_ip=" + CommonLogic.CustomerIpAddress());

            transactionCommand.Append("&x_first_name=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.FirstName));
            transactionCommand.Append("&x_last_name=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.LastName));
            transactionCommand.Append("&x_company=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.Company));
            transactionCommand.Append("&x_address=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.Address1));
            transactionCommand.Append("&x_city=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.City));
            transactionCommand.Append("&x_state=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.State));
            transactionCommand.Append("&x_zip=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.Zip));
            transactionCommand.Append("&x_country=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.Country));

            if (UseShippingAddress != null)
            {
                transactionCommand.Append("&x_ship_to_first_name=" + HttpContext.Current.Server.UrlEncode(UseShippingAddress.FirstName));
                transactionCommand.Append("&x_ship_to_last_name=" + HttpContext.Current.Server.UrlEncode(UseShippingAddress.LastName));
                transactionCommand.Append("&x_ship_to_company=" + HttpContext.Current.Server.UrlEncode(UseShippingAddress.Company));
                transactionCommand.Append("&x_ship_to_address=" + HttpContext.Current.Server.UrlEncode(UseShippingAddress.Address1));
                transactionCommand.Append("&x_ship_to_city=" + HttpContext.Current.Server.UrlEncode(UseShippingAddress.City));
                transactionCommand.Append("&x_ship_to_state=" + HttpContext.Current.Server.UrlEncode(UseShippingAddress.State));
                transactionCommand.Append("&x_ship_to_zip=" + HttpContext.Current.Server.UrlEncode(UseShippingAddress.Zip));
                transactionCommand.Append("&x_ship_to_country=" + HttpContext.Current.Server.UrlEncode(UseShippingAddress.Country));
            }

            transactionCommand.Append("&x_customer_ip=" + CommonLogic.CustomerIpAddress());

            if (CAVV.Length != 0 || ECI.Length != 0)
            {
                transactionCommand.Append("&x_authentication_indicator=" + ECI);
                transactionCommand.Append("&x_cardholder_authentication_value=" + CAVV);
            }

            String rawResponseString = "MANUAL GATEWAY SAID OK";

            AuthorizationCode = "0";
            AuthorizationResult = rawResponseString;
            AuthorizationTransID = CommonLogic.GetNewGUID();
            AVSResult = AppLogic.ro_OK;
            TransactionCommandOut = transactionCommand.ToString();
            TransactionResponse = String.Empty;

            return result;
        }

        // ----------------------------------------------------------------------------------
        // RECURRING BILLING ROUTINES
        // MANUAL GATEWAY ALWAYS APPROVES EVERY CALL!! THESE ARE JUST FOR EMULATION TESTING
        // THEY SIMULATE HOW AUTHORIZE.NET DOES IT'S RECURRING BILLING API
        // ----------------------------------------------------------------------------------

        // returns AppLogic.ro_OK if successful, along with the out RecurringSubscriptionID
        // returns error message if not successful
        // SubscriptionDescription may usually best be passed in as the product description of the first cart item
        public override string RecurringBillingCreateSubscription(String SubscriptionDescription, Customer ThisCustomer, Address UseBillingAddress, Address UseShippingAddress, Decimal RecurringAmount, DateTime StartDate, int RecurringInterval, DateIntervalTypeEnum RecurringIntervalType, int OriginalRecurringOrderNumber, string XID, IDictionary<string, string> TransactionContext, out String RecurringSubscriptionID, out String RecurringSubscriptionCommand, out String RecurringSubscriptionResult)
        {
            String result = AppLogic.ro_OK;
            RecurringSubscriptionID = String.Empty;
            RecurringSubscriptionCommand = String.Empty;
            RecurringSubscriptionResult = String.Empty;
            String ResultCode = String.Empty;

            String X_Login = AppLogic.ro_NotApplicable;
            String X_TranKey = AppLogic.ro_NotApplicable;
            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");
            String AuthServer = "manualgatewaysimulation"; // this short circuits the post/response call, and returns a "sample" approved XmlDoc for all requests, properly formatted.

            // This is the API interface object.
            AuthorizeNetRecurringAPI.ARBCreateSubscriptionRequest request = new AuthorizeNetRecurringAPI.ARBCreateSubscriptionRequest();

            // Include authentication information
            request.merchantAuthentication = new AuthorizeNetRecurringAPI.MerchantAuthentication();
            request.merchantAuthentication.name = X_Login;
            request.merchantAuthentication.transactionKey = X_TranKey;
            request.refId = OriginalRecurringOrderNumber.ToString();

            // Populate the subscription request with data
            AuthorizeNetRecurringAPI.ARBSubscription sub = new AuthorizeNetRecurringAPI.ARBSubscription();
            AuthorizeNetRecurringAPI.CreditCard creditCard = new AuthorizeNetRecurringAPI.CreditCard();

            sub.name = AppLogic.AppConfig("StoreName") + " - Order " + OriginalRecurringOrderNumber.ToString();

            creditCard.cardNumber = UseBillingAddress.CardNumber;
            creditCard.expirationDate = UseBillingAddress.CardExpirationYear + "-" + UseBillingAddress.CardExpirationMonth.PadLeft(2, '0');  // required format for API is YYYY-MM

            sub.payment = new AuthorizeNetRecurringAPI.Payment();
            sub.payment.item = creditCard;

            sub.billTo = new AuthorizeNetRecurringAPI.NameAndAddress();
            sub.billTo.firstName = UseBillingAddress.FirstName;
            sub.billTo.lastName = UseBillingAddress.LastName;
            sub.billTo.company = UseBillingAddress.Company;
            sub.billTo.address = UseBillingAddress.Address1;
            sub.billTo.city = UseBillingAddress.City;
            sub.billTo.state = UseBillingAddress.State;
            sub.billTo.zip = UseBillingAddress.Zip;
            sub.billTo.country = UseBillingAddress.Country;

            sub.shipTo = new AuthorizeNetRecurringAPI.NameAndAddress();
            sub.shipTo.firstName = UseShippingAddress.FirstName;
            sub.shipTo.lastName = UseShippingAddress.LastName;
            sub.shipTo.company = UseShippingAddress.Company;
            sub.shipTo.address = UseShippingAddress.Address1;
            sub.shipTo.city = UseShippingAddress.City;
            sub.shipTo.state = UseShippingAddress.State;
            sub.shipTo.zip = UseShippingAddress.Zip;
            sub.shipTo.country = UseShippingAddress.Country;

            sub.order = new AuthorizeNetRecurringAPI.Order();
            sub.order.invoiceNumber = OriginalRecurringOrderNumber.ToString();
            sub.order.description = SubscriptionDescription;
            sub.orderSpecified = true;

            sub.customer = new AuthorizeNetRecurringAPI.Customer();
            sub.customer.email = CommonLogic.IIF(ThisCustomer.EMail.Length != 0, ThisCustomer.EMail, UseBillingAddress.EMail);
            sub.customer.id = ThisCustomer.CustomerID.ToString();
            sub.customer.phoneNumber = UseBillingAddress.Phone;
            sub.customer.type = CommonLogic.IIF(UseBillingAddress.ResidenceType == ResidenceTypes.Residential, "individual", "business");
            sub.customerSpecified = true;

            sub.paymentSchedule = new AuthorizeNetRecurringAPI.PaymentSchedule();
            sub.paymentSchedule.startDate = StartDate.Year.ToString() + "-" + StartDate.ToString().PadLeft(2, '0') + "-" + StartDate.Day.ToString().PadLeft(2, '0');   // Required format is YYYY-MM-DD

            sub.amount = RecurringAmount;
            sub.amountSpecified = true;

            // NOTE: since our recurring products have no sunset, set max gateway allowed total occurrences to provide maxmimum
            // length of subscription time
            // MAX interval supported by the gateway is 3 years
            switch (RecurringIntervalType)
            {
                case DateIntervalTypeEnum.Day:
                    if (RecurringInterval < 7)
                    {
                        result = "The minimum interval for Authorize.Net Recurring Billing is 7 days";
                        return result;
                    }
                    sub.paymentSchedule.PaymentScheduleInterval.length = RecurringInterval;
                    sub.paymentSchedule.PaymentScheduleInterval.unit = AuthorizeNetRecurringAPI.SubscriptionUnitType.days;
                    sub.paymentSchedule.totalOccurrences = (int)((365 * 3) / RecurringInterval);
                    sub.paymentSchedule.totalOccurrencesSpecified = true;
                    break;
                case DateIntervalTypeEnum.Week:
                    sub.paymentSchedule.PaymentScheduleInterval.length = RecurringInterval * 7;
                    sub.paymentSchedule.PaymentScheduleInterval.unit = AuthorizeNetRecurringAPI.SubscriptionUnitType.days;
                    sub.paymentSchedule.totalOccurrences = (int)((365 * 3) / (RecurringInterval * 7));
                    sub.paymentSchedule.totalOccurrencesSpecified = true;
                    break;
                case DateIntervalTypeEnum.Month:
                    sub.paymentSchedule.PaymentScheduleInterval.length = RecurringInterval;
                    sub.paymentSchedule.PaymentScheduleInterval.unit = AuthorizeNetRecurringAPI.SubscriptionUnitType.months;
                    sub.paymentSchedule.totalOccurrences = (int)((12 * 3) / RecurringInterval);
                    sub.paymentSchedule.totalOccurrencesSpecified = true;
                    break;
                case DateIntervalTypeEnum.Year:
                    sub.paymentSchedule.PaymentScheduleInterval.length = RecurringInterval * 12;
                    sub.paymentSchedule.PaymentScheduleInterval.unit = AuthorizeNetRecurringAPI.SubscriptionUnitType.months;
                    sub.paymentSchedule.totalOccurrences = (int)((12 * 3) / (RecurringInterval * 12));
                    sub.paymentSchedule.totalOccurrencesSpecified = true;
                    break;
                case DateIntervalTypeEnum.Weekly:
                    RecurringInterval = 1;
                    sub.paymentSchedule.PaymentScheduleInterval.length = RecurringInterval * 7;
                    sub.paymentSchedule.PaymentScheduleInterval.unit = AuthorizeNetRecurringAPI.SubscriptionUnitType.days;
                    sub.paymentSchedule.totalOccurrences = (int)((365 * 3) / (RecurringInterval * 7));
                    sub.paymentSchedule.totalOccurrencesSpecified = true;
                    break;
                case DateIntervalTypeEnum.BiWeekly:
                    RecurringInterval = 2;
                    sub.paymentSchedule.PaymentScheduleInterval.length = RecurringInterval * 7;
                    sub.paymentSchedule.PaymentScheduleInterval.unit = AuthorizeNetRecurringAPI.SubscriptionUnitType.days;
                    sub.paymentSchedule.totalOccurrences = (int)((365 * 3) / (RecurringInterval * 7));
                    sub.paymentSchedule.totalOccurrencesSpecified = true;
                    break;
                //case DateIntervalTypeEnum.SemiMonthly:
                //    RecurringInterval = 2;
                //    sub.paymentSchedule.PaymentScheduleInterval.length = RecurringInterval * 7;
                //    sub.paymentSchedule.PaymentScheduleInterval.unit = AuthorizeNetRecurringAPI.SubscriptionUnitType.days;
                //    sub.paymentSchedule.totalOccurrences = (int)((365 * 3) / (RecurringInterval * 7));
                //    sub.paymentSchedule.totalOccurrencesSpecified = true;
                //    break;
                case DateIntervalTypeEnum.EveryFourWeeks:
                    RecurringInterval = 4;
                    sub.paymentSchedule.PaymentScheduleInterval.length = RecurringInterval * 7;
                    sub.paymentSchedule.PaymentScheduleInterval.unit = AuthorizeNetRecurringAPI.SubscriptionUnitType.days;
                    sub.paymentSchedule.totalOccurrences = (int)((365 * 3) / (RecurringInterval * 7));
                    sub.paymentSchedule.totalOccurrencesSpecified = true;
                    break;
                case DateIntervalTypeEnum.Monthly:
                    RecurringInterval = 1;
                    sub.paymentSchedule.PaymentScheduleInterval.length = RecurringInterval;
                    sub.paymentSchedule.PaymentScheduleInterval.unit = AuthorizeNetRecurringAPI.SubscriptionUnitType.months;
                    sub.paymentSchedule.totalOccurrences = (int)((12 * 3) / RecurringInterval);
                    sub.paymentSchedule.totalOccurrencesSpecified = true;
                    break;
                case DateIntervalTypeEnum.Quarterly:
                    RecurringInterval = 3;
                    sub.paymentSchedule.PaymentScheduleInterval.length = RecurringInterval;
                    sub.paymentSchedule.PaymentScheduleInterval.unit = AuthorizeNetRecurringAPI.SubscriptionUnitType.months;
                    sub.paymentSchedule.totalOccurrences = (int)((12 * 3) / RecurringInterval);
                    sub.paymentSchedule.totalOccurrencesSpecified = true;
                    break;
                case DateIntervalTypeEnum.SemiYearly:
                    RecurringInterval = 6;
                    sub.paymentSchedule.PaymentScheduleInterval.length = RecurringInterval;
                    sub.paymentSchedule.PaymentScheduleInterval.unit = AuthorizeNetRecurringAPI.SubscriptionUnitType.months;
                    sub.paymentSchedule.totalOccurrences = (int)((12 * 3) / RecurringInterval);
                    sub.paymentSchedule.totalOccurrencesSpecified = true;
                    break;
                case DateIntervalTypeEnum.Yearly:
                    RecurringInterval = 12;
                    sub.paymentSchedule.PaymentScheduleInterval.length = RecurringInterval;
                    sub.paymentSchedule.PaymentScheduleInterval.unit = AuthorizeNetRecurringAPI.SubscriptionUnitType.months;
                    sub.paymentSchedule.totalOccurrences = (int)((12 * 3) / RecurringInterval);
                    sub.paymentSchedule.totalOccurrencesSpecified = true;
                    break;
                default:
                    RecurringInterval = 1;
                    sub.paymentSchedule.PaymentScheduleInterval.length = RecurringInterval;
                    sub.paymentSchedule.PaymentScheduleInterval.unit = AuthorizeNetRecurringAPI.SubscriptionUnitType.months;
                    sub.paymentSchedule.totalOccurrences = (int)((12 * 3) / RecurringInterval);
                    sub.paymentSchedule.totalOccurrencesSpecified = true;
                    break;
            }
            sub.paymentSchedule.intervalSpecified = true;

            // Any free trial?
            sub.paymentSchedule.trialOccurrences = 0;
            sub.paymentSchedule.trialOccurrencesSpecified = true;
            sub.trialAmount = System.Decimal.Zero;
            sub.trialAmountSpecified = false; // we don't support free trials at this time

            request.subscription = sub;

            RecurringSubscriptionCommand = XmlCommon.SerializeObject(request, request.GetType());
            // wipe sensitive data:
            if (RecurringSubscriptionCommand.Length != 0)
            {
                RecurringSubscriptionCommand = RecurringSubscriptionCommand.Replace(UseBillingAddress.CardNumber, AppLogic.SafeDisplayCardNumber(UseBillingAddress.CardNumber, "Orders", 0));
                RecurringSubscriptionCommand = RecurringSubscriptionCommand.Replace(X_TranKey, "*".PadLeft(X_TranKey.Length, '*'));
            }

            // The response type will normally be ARBCreateSubscriptionResponse.
            // However, in the case of an error such as an XML parsing error, the response type will be ErrorResponse.

            object response = null;
            XmlDocument xmldoc = null;
            result = AuthorizeNetRecurringAPI.APIHelper.PostRequest(request, AuthServer, out xmldoc);

            if (xmldoc != null)
            {
                RecurringSubscriptionResult = xmldoc.InnerXml;
                // wipe sensitive data:
                if (RecurringSubscriptionResult.Length != 0)
                {
                    RecurringSubscriptionResult = RecurringSubscriptionResult.Replace(UseBillingAddress.CardNumber, AppLogic.SafeDisplayCardNumber(UseBillingAddress.CardNumber, "Orders", 0));
                    RecurringSubscriptionResult = RecurringSubscriptionResult.Replace(X_TranKey, "*".PadLeft(X_TranKey.Length, '*'));
                }
            }
            
            if (result.Equals(AppLogic.ro_OK, StringComparison.InvariantCultureIgnoreCase))
            {
                result = AuthorizeNetRecurringAPI.APIHelper.ProcessXmlResponse(xmldoc, out response);
            }

            if (result.Equals(AppLogic.ro_OK, StringComparison.InvariantCultureIgnoreCase))
            {
                result = AuthorizeNetRecurringAPI.APIHelper.ProcessResponse(response, out ResultCode, out RecurringSubscriptionID);
            }

            return result;
        }

        public override string RecurringBillingCancelSubscription(String RecurringSubscriptionID, int OriginalRecurringOrderNumber, IDictionary<string, string> TransactionContext)
        {
            String result = AppLogic.ro_OK;

            String X_Login = AppLogic.ro_NotApplicable;
            String X_TranKey = AppLogic.ro_NotApplicable;
            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");
            String AuthServer = "manualgatewaysimulation"; // this short circuits the post/response call, and returns a "sample" approved XmlDoc for all requests, properly formatted.

            // This is the API interface object.
            AuthorizeNetRecurringAPI.ARBCancelSubscriptionRequest request = new AuthorizeNetRecurringAPI.ARBCancelSubscriptionRequest();

            // Include authentication information
            request.merchantAuthentication = new AuthorizeNetRecurringAPI.MerchantAuthentication();
            request.merchantAuthentication.name = X_Login;
            request.merchantAuthentication.transactionKey = X_TranKey;
            request.refId = OriginalRecurringOrderNumber.ToString();

            // Populate the subscription request with data
            AuthorizeNetRecurringAPI.ARBSubscription sub = new AuthorizeNetRecurringAPI.ARBSubscription();
            AuthorizeNetRecurringAPI.CreditCard creditCard = new AuthorizeNetRecurringAPI.CreditCard();
            request.subscriptionId = RecurringSubscriptionID;

            // The response type will normally be ARBCancelSubscriptionResponse.
            // However, in the case of an error such as an XML parsing error, the response type will be ErrorResponse.

            object response = null;
            XmlDocument xmldoc = null;
            result = AuthorizeNetRecurringAPI.APIHelper.PostRequest(request, AuthServer, out xmldoc);

            if (result.Equals(AppLogic.ro_OK, StringComparison.InvariantCultureIgnoreCase))
            {
                result = AuthorizeNetRecurringAPI.APIHelper.ProcessXmlResponse(xmldoc, out response);
            }

            if (result.Equals(AppLogic.ro_OK, StringComparison.InvariantCultureIgnoreCase))
            {
                String tmpResultCode = String.Empty; // not used or needed here
                String tmpRecurringSubscriptionID = String.Empty; // not used or needed here
                result = AuthorizeNetRecurringAPI.APIHelper.ProcessResponse(response, out tmpResultCode, out tmpRecurringSubscriptionID);
            }

            return result;
        }

        public override string RecurringBillingAddressUpdate(String RecurringSubscriptionID, int OriginalRecurringOrderNumber, Address UseBillingAddress)
        {
            String result = AppLogic.ro_OK;

            String X_Login = AppLogic.ro_NotApplicable;
            String X_TranKey = AppLogic.ro_NotApplicable;
            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");
            String AuthServer = "manualgatewaysimulation"; // this short circuits the post/response call, and returns a "sample" approved XmlDoc for all requests, properly formatted.

            // This is the API interface object.
            AuthorizeNetRecurringAPI.ARBUpdateSubscriptionRequest request = new AuthorizeNetRecurringAPI.ARBUpdateSubscriptionRequest();

            // Include authentication information
            request.merchantAuthentication = new AuthorizeNetRecurringAPI.MerchantAuthentication();
            request.merchantAuthentication.name = X_Login;
            request.merchantAuthentication.transactionKey = X_TranKey;
            request.refId = OriginalRecurringOrderNumber.ToString();

            // Populate the subscription request with test data
            AuthorizeNetRecurringAPI.ARBSubscription sub = new AuthorizeNetRecurringAPI.ARBSubscription();
            AuthorizeNetRecurringAPI.CreditCard creditCard = new AuthorizeNetRecurringAPI.CreditCard();
            request.subscriptionId = RecurringSubscriptionID;

            creditCard.cardNumber = UseBillingAddress.CardNumber;
            creditCard.expirationDate = UseBillingAddress.CardExpirationYear + "-" + UseBillingAddress.CardExpirationMonth.PadLeft(2, '0');  // required format for API is YYYY-MM
            sub.payment = new AuthorizeNetRecurringAPI.Payment();
            sub.payment.item = creditCard;

            sub.billTo = new AuthorizeNetRecurringAPI.NameAndAddress();
            sub.billTo.firstName = UseBillingAddress.FirstName;
            sub.billTo.lastName = UseBillingAddress.LastName;

            request.subscription = sub;

            // The response type will normally be ARBUpdateSubscriptionResponse.
            // However, in the case of an error such as an XML parsing error, the response type will be ErrorResponse.

            object response = null;
            XmlDocument xmldoc = null;
            result = AuthorizeNetRecurringAPI.APIHelper.PostRequest(request, AuthServer, out xmldoc);

            if (result.Equals(AppLogic.ro_OK, StringComparison.InvariantCultureIgnoreCase))
            {
                result = AuthorizeNetRecurringAPI.APIHelper.ProcessXmlResponse(xmldoc, out response);
            }

            if (result.Equals(AppLogic.ro_OK, StringComparison.InvariantCultureIgnoreCase))
            {
                String tmpResultCode = String.Empty; // not used or needed here
                String tmpRecurringSubscriptionID = String.Empty; // not used or needed here
                result = AuthorizeNetRecurringAPI.APIHelper.ProcessResponse(response, out tmpResultCode, out tmpRecurringSubscriptionID);
            }

            return result;
        }

        public override String RecurringBillingGetStatusFile()
        {
            StringBuilder tmpS = new StringBuilder(4096);
            // just find any AutoBill recurring orders due to day, and return a random status (APPROVED, DECLINED)
            tmpS.Append("<RecurringBillingReport Gateway=\"MANUAL\">");
            tmpS.Append("\n");
            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS("select * from ShoppingCart  with (NOLOCK)  where RecurringSubscriptionID<>'' and NextRecurringShipDate<=" + DB.SQuote(Localization.DateStringForDB(System.DateTime.Now)), conn))
                {
                    while (rs.Read())
                    {
                        String ThisStatus = "APPROVED";
                        String Msg = String.Empty;
                        if (CommonLogic.GetRandomNumber(1, 10) < 3)
                        {
                            // decline some randomly
                            ThisStatus = "DECLINED";
                            Msg = "Random Decline";
                        }
                        tmpS.Append(String.Format("<TX RecurringSubscriptionID=\"{0}\" Status=\"{1}\" Message=\"{2}\"/>\n", DB.RSField(rs, "RecurringSubscriptionID"), ThisStatus, XmlCommon.XmlEncodeAttribute(Msg)));
                    }                    
                }
            }                       
            tmpS.Append("</RecurringBillingReport>");
            return XmlCommon.PrettyPrintXml(tmpS.ToString());
        }

        public override string ProcessAutoBillStatusFile(String GW, string StatusFile, out string Results, RecurringOrderMgr OrderManager)
        {
            String Status = AppLogic.ro_OK;
            StringBuilder tmpS = new StringBuilder();
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(StatusFile);
                String Separator = String.Empty;
                foreach (XmlNode n in doc.SelectNodes("/RecurringBillingReport/TX"))
                {
                    tmpS.Append(Separator);
                    String TxSubID = XmlCommon.XmlAttribute(n, "RecurringSubscriptionID");
                    String TxStatus = XmlCommon.XmlAttribute(n, "Status").ToUpperInvariant();
                    String TxMsg = XmlCommon.XmlAttribute(n, "Message");
                    int OrigOrdNumber = AppLogic.GetOriginalRecurringOrderNumberFromSubscriptionID(TxSubID);
                    tmpS.Append("Processing ID ");
                    tmpS.Append(TxSubID);
                    tmpS.Append(",");
                    tmpS.Append(TxStatus);
                    tmpS.Append("=");
                    try
                    {
                        String tmpStatus = String.Empty;
                        if (TxStatus == "APPROVED")
                        {
                            int NewOrderNumber = 0;
                            tmpStatus = OrderManager.ProcessAutoBillApproved(OrigOrdNumber, String.Empty, DateTime.MinValue, out NewOrderNumber);
                        }
                        else
                        {
                            tmpStatus = OrderManager.ProcessAutoBillDeclined(OrigOrdNumber, String.Empty, DateTime.MinValue, TxSubID, TxMsg);
                        }
                        if (tmpStatus == AppLogic.ro_OK)
                        {
                            // mark this one as processed ok
                            // TBD
                        }
                        else
                        {
                            // mark this record as not processed
                            // TBD
                        }
                        tmpS.Append(tmpStatus);
                    }
                    catch (Exception ex)
                    {
                        tmpS.Append(ex.Message);
                    }
                    Separator = "\n";
                }
                Status = AppLogic.ro_OK;
            }
            catch (Exception ex)
            {
                Status = ex.Message;
            }
            Results = tmpS.ToString();
            return Status;
        }

        public override RecurringSupportType RecurringSupportType()
        {
            return Processors.RecurringSupportType.Normal;
        }


        public override String DisplayName(String LocaleSetting)
        {
            return "Manual (No Gateway)";
        }
    }
}
