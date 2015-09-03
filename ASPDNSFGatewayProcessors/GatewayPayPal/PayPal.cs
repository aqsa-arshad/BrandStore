// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Data.SqlClient;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways;
using AspDotNetStorefrontGateways.Processors;

namespace AspDotNetStorefrontGateways.Processors
{
    /// <summary>
    /// Summary description for PayPal.
    /// </summary>
    public class PayPal : GatewayProcessor
    {
        public const String BN = "AspDotNet" + "Storefront" + "_Cart"; // Do not change this line or your paypal website calls may not work!
        private const String API_VER = "60";
        //private const Boolean LogToErrorTable = false;
        private PayPalAPISoapBinding IPayPalRefund;
        private PayPalAPIAASoapBinding IPayPal;

        // DoDirectPaymentRequest
        public DoDirectPaymentRequestType PaymentRequest;
        public DoDirectPaymentReq DDPReq;
        public TransactionSearchReq request;

        public override string AdministratorSetupPrompt
        {
            get
            {
                return "See <a href='http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=paypalpaymentspro&type=manual' target='_blank'>Manual</a>.";
            }
        }


        public PayPal()
        {
            PayPalController.GetPaypalRequirements(out IPayPalRefund, out IPayPal);
        }

        public override string CaptureOrder(Order o)
        {
            String result = String.Empty;

            o.CaptureTXCommand = "";
            o.CaptureTXResult = "";

            // check for ReauthorizationID first, if doesn't exist, use original AuthorizationID
            String TransID = Regex.Match(o.AuthorizationPNREF, "(?<=REAU=)[0-9A-Z]+", RegexOptions.Compiled).ToString();
            if (TransID.Length == 0)
            {
                TransID = Regex.Match(o.AuthorizationPNREF, "(?<=AUTH=)[0-9A-Z]+", RegexOptions.Compiled).ToString();
            }

            Decimal OrderTotal = o.OrderBalance;

            if (TransID.Length == 0 || TransID == "0")
            {
                result = "Invalid or Empty Transaction ID";
            }
            else
            {
                try
                {
                    DoCaptureReq CaptureReq = new DoCaptureReq();
                    DoCaptureRequestType CaptureRequestType = new DoCaptureRequestType();
                    DoCaptureResponseType CaptureResponse;

                    BasicAmountType totalAmount = new BasicAmountType();
                    totalAmount.Value = Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal);
                    totalAmount.currencyID = (CurrencyCodeType)Enum.Parse(typeof(CurrencyCodeType), AppLogic.AppConfig("Localization.StoreCurrency"), true);

                    CaptureRequestType.Amount = totalAmount;
                    CaptureRequestType.AuthorizationID = TransID;

                    CaptureRequestType.CompleteType = CompleteCodeType.Complete;
                    CaptureRequestType.Version = API_VER;

                    CaptureReq.DoCaptureRequest = CaptureRequestType;

                    o.CaptureTXCommand = XmlCommon.SerializeObject(CaptureReq, CaptureReq.GetType()); //"Not Available For PayPal";

                    CaptureResponse = (DoCaptureResponseType)IPayPal.DoCapture(CaptureReq);

                    //if (LogToErrorTable)
                    //{
                    //    PayPalController.Log(XmlCommon.SerializeObject(CaptureReq, CaptureReq.GetType()), "DoCapture Request");
                    //    PayPalController.Log(XmlCommon.SerializeObject(CaptureResponse, CaptureResponse.GetType()), "DoCapture Response"); 
                    //}

                    o.CaptureTXResult = XmlCommon.SerializeObject(CaptureResponse, CaptureResponse.GetType());

                    if (CaptureResponse != null && CaptureResponse.Ack.ToString().StartsWith("success", StringComparison.InvariantCultureIgnoreCase))
                    {
                        result = AppLogic.ro_OK;
                        String CaptureTransID = CaptureResponse.DoCaptureResponseDetails.PaymentInfo.TransactionID;
                        o.AuthorizationPNREF = o.AuthorizationPNREF + "|CAPTURE=" + CaptureTransID;
                    }
                    else
                    {
                        if (CaptureResponse.Errors != null)
                        {
                            bool first = true;
                            for (int ix = 0; ix < CaptureResponse.Errors.Length; ix++)
                            {
                                if (!first)
                                {
                                    result += ", ";
                                }
                                result += "Error: [" + CaptureResponse.Errors[ix].ErrorCode + "] " + CaptureResponse.Errors[ix].LongMessage;
                                first = false;
                            }
                        }
                    }
                }
                catch
                {
                    result = "NO RESPONSE FROM GATEWAY!";
                }
            }
            return result;
        }

        public override string VoidOrder(int OrderNumber)
        {
            String result = String.Empty;

            DB.ExecuteSQL("update orders set VoidTXCommand=NULL, VoidTXResult=NULL where OrderNumber=" + OrderNumber.ToString());
            String TransID = String.Empty;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select * from orders   with (NOLOCK)  where OrderNumber=" + OrderNumber.ToString(), con))
                {
                    if (rs.Read())
                    {
                        // If you are voiding a transaction that has been reauthorized, 
                        // use the ID from the original authorization, and not the reauthorization.
                        TransID = Regex.Match(DB.RSField(rs, "AuthorizationPNREF"), "(?<=AUTH=)[0-9A-Z]+", RegexOptions.Compiled).ToString();
                    }
                }
            }

            if (TransID.Length == 0 || TransID == "0")
            {
                result = "Invalid or Empty Transaction ID";
            }
            else
            {
                try
                {

                    DoVoidReq VoidReq = new DoVoidReq();
                    DoVoidRequestType VoidRequestType = new DoVoidRequestType();
                    DoVoidResponseType VoidResponse;

                    VoidRequestType.AuthorizationID = TransID;
                    VoidRequestType.Version = API_VER;

                    VoidReq.DoVoidRequest = VoidRequestType;

                    VoidResponse = (DoVoidResponseType)IPayPal.DoVoid(VoidReq);


                    //if (LogToErrorTable)
                    //{
                    //    PayPalController.Log(XmlCommon.SerializeObject(VoidReq, VoidReq.GetType()), "DoVoid Request");
                    //    PayPalController.Log(XmlCommon.SerializeObject(VoidResponse, VoidResponse.GetType()), "DoVoid Response"); 
                    //}

                    DB.ExecuteSQL("update orders set VoidTXCommand=" + DB.SQuote(XmlCommon.SerializeObject(VoidReq, VoidReq.GetType()))
                        + ", VoidTXResult=" + DB.SQuote(XmlCommon.SerializeObject(VoidResponse, VoidResponse.GetType())) + " where OrderNumber=" + OrderNumber.ToString());

                    if (VoidResponse != null && VoidResponse.Ack.ToString().StartsWith("success", StringComparison.InvariantCultureIgnoreCase))
                    {
                        result = AppLogic.ro_OK;
                    }
                    else
                    {
                        if (VoidResponse.Errors != null)
                        {
                            bool first = true;
                            for (int ix = 0; ix < VoidResponse.Errors.Length; ix++)
                            {
                                if (!first)
                                {
                                    result += ", ";
                                }
                                result += "Error: [" + VoidResponse.Errors[ix].ErrorCode + "] " + VoidResponse.Errors[ix].LongMessage;
                                first = false;
                            }
                        }
                    }

                }
                catch
                {
                    result = "NO RESPONSE FROM GATEWAY!";
                }
            }
            return result;
        }

        // if RefundAmount == 0.0M, then then ENTIRE order amount will be refunded!
        public override string RefundOrder(int OriginalOrderNumber, int NewOrderNumber, decimal RefundAmount, String RefundReason, Address UseBillingAddress)
        {
            String result = String.Empty;

            DB.ExecuteSQL("update orders set RefundTXCommand=NULL, RefundTXResult=NULL where OrderNumber=" + OriginalOrderNumber.ToString());
            String TransID = String.Empty;
            Decimal OrderTotal = System.Decimal.Zero;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select * from Orders  with (NOLOCK)  where OrderNumber=" + OriginalOrderNumber.ToString(), con))
                {
                    if (rs.Read())
                    {
                        TransID = Regex.Match(DB.RSField(rs, "AuthorizationPNREF"), "(?<=CAPTURE=)[0-9A-Z]+", RegexOptions.Compiled).ToString();
                        OrderTotal = DB.RSFieldDecimal(rs, "OrderTotal");
                    }
                }
            }

            if (TransID.Length == 0 || TransID == "0")
            {
                result = "Invalid or Empty Transaction ID";
            }
            else
            {
                try
                {

                    RefundTransactionReq RefundReq = new RefundTransactionReq();
                    RefundTransactionRequestType RefundRequestType = new RefundTransactionRequestType();
                    RefundTransactionResponseType RefundResponse;
                    BasicAmountType BasicAmount = new BasicAmountType();

                    RefundRequestType.TransactionID = TransID;
                    RefundRequestType.Version = API_VER;

                    BasicAmount.currencyID = (CurrencyCodeType)Enum.Parse(typeof(CurrencyCodeType), AppLogic.AppConfig("Localization.StoreCurrency"), true);

                    //If partial refund set value ( like 1.95). If FULL Refund leave it empty. The transactionID will take care of the amount
                    if (OrderTotal == RefundAmount || RefundAmount == 0.0M)
                    {
                        RefundRequestType.RefundType = RefundType.Full;
                    }
                    else
                    {
                        BasicAmount.Value = Localization.CurrencyStringForGatewayWithoutExchangeRate(RefundAmount);
                        RefundRequestType.RefundType = RefundType.Partial;
                    }
                    RefundRequestType.Amount = BasicAmount;
                    RefundRequestType.RefundTypeSpecified = true;

                    if (!String.IsNullOrEmpty(RefundReason))
                    {
                        RefundRequestType.Memo = RefundReason;
                    }

                    RefundReq.RefundTransactionRequest = RefundRequestType;

                    DB.ExecuteSQL("update orders set RefundTXCommand=" + DB.SQuote(XmlCommon.SerializeObject(RefundRequestType, RefundRequestType.GetType())) + " where OrderNumber=" + OriginalOrderNumber.ToString());

                    RefundResponse = (RefundTransactionResponseType)IPayPalRefund.RefundTransaction(RefundReq);


                    //if (LogToErrorTable)
                    //{
                    //    PayPalController.Log(XmlCommon.SerializeObject(RefundReq, RefundReq.GetType()), "RefundTransaction Request");
                    //    PayPalController.Log(XmlCommon.SerializeObject(RefundResponse, RefundResponse.GetType()), "RefundTransaction Response"); 
                    //}

                    DB.ExecuteSQL("update orders set RefundTXCommand=" + DB.SQuote(XmlCommon.SerializeObject(RefundReq, RefundReq.GetType()))
                        + ", RefundTXResult=" + DB.SQuote(XmlCommon.SerializeObject(RefundResponse, RefundResponse.GetType())) + " where OrderNumber=" + OriginalOrderNumber.ToString());

                    String RefundTXResult = String.Empty;
                    if (RefundResponse != null && RefundResponse.Ack.ToString().StartsWith("success", StringComparison.InvariantCultureIgnoreCase))
                    {
                        result = AppLogic.ro_OK;
                        String RefundTransID = RefundResponse.RefundTransactionID;
                        DB.ExecuteSQL("update orders set AuthorizationPNREF=AuthorizationPNREF+'|REFUND=" + RefundTransID + "' where OrderNumber=" + OriginalOrderNumber.ToString());
                    }
                    else
                    {
                        if (RefundResponse.Errors != null)
                        {
                            bool first = true;
                            for (int ix = 0; ix < RefundResponse.Errors.Length; ix++)
                            {
                                if (!first)
                                {
                                    result += ", ";
                                }
                                result += "Error: [" + RefundResponse.Errors[ix].ErrorCode + "] " + RefundResponse.Errors[ix].LongMessage;
                                first = false;
                            }

                            result += "Note: If you are using Accelerated Boarding for PayPal Express you will not be able to modify orders until you sign up for a full account and enter your API credentials.";
                        }
                    }

                }
                catch
                {
                    result = "NO RESPONSE FROM GATEWAY!";
                }
            }
            return result;
        }

        // ProcessCard() is used for Credit Card processing via Website Payments Pro,
        // just like other credit card gateways.
        // ProcessPaypal() is used for Express Checkout and PayPal payments.
        public override string ProcessCard(int OrderNumber, int CustomerID, decimal OrderTotal, bool useLiveTransactions, TransactionModeEnum TransactionMode, Address UseBillingAddress, string CardExtraCode, Address UseShippingAddress, string CAVV, string ECI, string XID, out string AVSResult, out string AuthorizationResult, out string AuthorizationCode, out string AuthorizationTransID, out string TransactionCommandOut, out string TransactionResponse)
        {
            String result = AppLogic.ro_OK;

            AuthorizationCode = String.Empty;
            AuthorizationResult = String.Empty;
            AuthorizationTransID = String.Empty;
            AVSResult = String.Empty;
            TransactionCommandOut = String.Empty;
            TransactionResponse = String.Empty;
            try
            {
                // the request details object contains all payment details 
                DoDirectPaymentRequestDetailsType RequestDetails = new DoDirectPaymentRequestDetailsType();

                // define the payment action to 'Sale'
                // (another option is 'Authorization', which would be followed later with a DoCapture API call)
                RequestDetails.PaymentAction = (PaymentActionCodeType)CommonLogic.IIF(AppLogic.TransactionModeIsAuthOnly(), (int)PaymentActionCodeType.Authorization, (int)PaymentActionCodeType.Sale);

                // define the total amount and currency for the transaction
                PaymentDetailsType PaymentDetails = new PaymentDetailsType();

                BasicAmountType totalAmount = new BasicAmountType();
                totalAmount.Value = Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal);
                totalAmount.currencyID = (CurrencyCodeType)Enum.Parse(typeof(CurrencyCodeType), AppLogic.AppConfig("Localization.StoreCurrency"), true);
                PaymentDetails.OrderTotal = totalAmount;
                PaymentDetails.InvoiceID = OrderNumber.ToString();
                PaymentDetails.ButtonSource = PayPal.BN + "_DP_US";
                PaymentDetails.OrderDescription = AppLogic.AppConfig("StoreName");

                // define the credit card to be used

                CreditCardDetailsType creditCard = new CreditCardDetailsType();
                creditCard.CreditCardNumber = UseBillingAddress.CardNumber;
                creditCard.ExpMonth = Localization.ParseUSInt(UseBillingAddress.CardExpirationMonth);
                creditCard.ExpYear = Localization.ParseUSInt(UseBillingAddress.CardExpirationYear);
                creditCard.ExpMonthSpecified = true;
                creditCard.ExpYearSpecified = true;
                creditCard.CVV2 = CardExtraCode;
                creditCard.CreditCardType = (CreditCardTypeType)Enum.Parse(typeof(CreditCardTypeType), UseBillingAddress.CardType, true);
                creditCard.CreditCardTypeSpecified = true;

                PayerInfoType cardHolder = new PayerInfoType();
                PersonNameType oPersonNameType = new PersonNameType();
                oPersonNameType.FirstName = UseBillingAddress.FirstName;
                oPersonNameType.LastName = UseBillingAddress.LastName;
                oPersonNameType.MiddleName = String.Empty;
                oPersonNameType.Salutation = String.Empty;
                oPersonNameType.Suffix = String.Empty;
                cardHolder.PayerName = oPersonNameType;

                AddressType PayerAddress = new AddressType();
                PayerAddress.Street1 = UseBillingAddress.Address1;
                PayerAddress.CityName = UseBillingAddress.City;
                PayerAddress.StateOrProvince = UseBillingAddress.State;
                PayerAddress.PostalCode = UseBillingAddress.Zip;
                PayerAddress.Country = (CountryCodeType)Enum.Parse(typeof(CountryCodeType), AppLogic.GetCountryTwoLetterISOCode(UseBillingAddress.Country), true);
                PayerAddress.CountrySpecified = true;

                if (UseShippingAddress != null)
                {
                    AddressType shippingAddress = new AddressType();
                    shippingAddress.Name = (UseShippingAddress.FirstName + " " + UseShippingAddress.LastName).Trim();
                    shippingAddress.Street1 = UseShippingAddress.Address1;
                    shippingAddress.Street2 = UseShippingAddress.Address2 + CommonLogic.IIF(UseShippingAddress.Suite != "", " Ste " + UseShippingAddress.Suite, "");
                    shippingAddress.CityName = UseShippingAddress.City;
                    shippingAddress.StateOrProvince = UseShippingAddress.State;
                    shippingAddress.PostalCode = UseShippingAddress.Zip;
                    shippingAddress.Country = (CountryCodeType)Enum.Parse(typeof(CountryCodeType), AppLogic.GetCountryTwoLetterISOCode(UseShippingAddress.Country), true);
                    shippingAddress.CountrySpecified = true;
                    PaymentDetails.ShipToAddress = shippingAddress;
                }

                cardHolder.Address = PayerAddress;
                creditCard.CardOwner = cardHolder;

                RequestDetails.CreditCard = creditCard;
                RequestDetails.PaymentDetails = PaymentDetails;
                RequestDetails.IPAddress = CommonLogic.CustomerIpAddress(); // cart.ThisCustomer.LastIPAddress;

                if (RequestDetails.IPAddress == "::1")
                    RequestDetails.IPAddress = "127.0.0.1";

                // instantiate the actual request object
                PaymentRequest = new DoDirectPaymentRequestType();
                PaymentRequest.Version = API_VER;
                PaymentRequest.DoDirectPaymentRequestDetails = RequestDetails;
                DDPReq = new DoDirectPaymentReq();
                DDPReq.DoDirectPaymentRequest = PaymentRequest;

                DoDirectPaymentResponseType responseDetails = (DoDirectPaymentResponseType)IPayPal.DoDirectPayment(DDPReq);

                //if (LogToErrorTable)
                //{
                //    PayPalController.Log(XmlCommon.SerializeObject(DDPReq, DDPReq.GetType()), "DoDirectPayment Request");
                //    PayPalController.Log(XmlCommon.SerializeObject(responseDetails, responseDetails.GetType()), "DoDirectPayment Response"); 
                //}

                if (responseDetails != null && responseDetails.Ack.ToString().StartsWith("success", StringComparison.InvariantCultureIgnoreCase))
                {
                    AuthorizationTransID = CommonLogic.IIF(TransactionMode.ToString().ToLower() == AppLogic.ro_TXModeAuthOnly.ToLower(), "AUTH=", "CAPTURE=") + responseDetails.TransactionID.ToString();
                    AuthorizationCode = responseDetails.CorrelationID;
                    AVSResult = responseDetails.AVSCode;
                    result = AppLogic.ro_OK;
                    AuthorizationResult = responseDetails.Ack.ToString() + "|AVSCode=" + responseDetails.AVSCode.ToString() + "|CVV2Code=" + responseDetails.CVV2Code.ToString();
                }
                else
                {
                    if (responseDetails.Errors != null)
                    {
                        String Separator = String.Empty;
                        for (int ix = 0; ix < responseDetails.Errors.Length; ix++)
                        {
                            AuthorizationResult += Separator;
                            AuthorizationResult += responseDetails.Errors[ix].LongMessage;// record failed TX
                            TransactionResponse += Separator;
                            try
                            {
                                TransactionResponse += String.Format("|{0},{1},{2}|", responseDetails.Errors[ix].ShortMessage, responseDetails.Errors[ix].ErrorCode, responseDetails.Errors[ix].LongMessage); // record failed TX
                            }
                            catch { }
                            Separator = ", ";
                        }
                    }
                    result = AuthorizationResult;
                    // just store something here, as there is no other way to get data out of this gateway about the failure for logging in failed transaction table
                }
            }
            catch
            {
                result = "Transaction Failed";
            }
            return result;
        }

        public String ReAuthorizeOrder(int OrderNumber)
        {
            // Once the PayPal honor period (3 days) is over, PayPal no longer ensures that 100%
            // of the funds will be available. A ReAuthorize will start a new settle period.

            String result = String.Empty;

            String PNREF = String.Empty;
            String TransID = String.Empty;
            Decimal OrderTotal = 0;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select * from orders   with (NOLOCK)  where OrderNumber=" + OrderNumber.ToString(), con))
                {
                    if (rs.Read())
                    {
                        PNREF = DB.RSField(rs, "AuthorizationPNREF");
                        TransID = Regex.Match(PNREF, "(?<=AUTH=)[0-9A-Z]+", RegexOptions.Compiled).ToString();
                        OrderTotal = DB.RSFieldDecimal(rs, "OrderTotal");
                    }
                }
            }

            if (TransID.Length == 0 || TransID == "0")
            {
                result = "Invalid or Empty Transaction ID";
            }
            else
            {
                try
                {
                    BasicAmountType totalAmount = new BasicAmountType();
                    totalAmount.Value = Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal);
                    totalAmount.currencyID = (CurrencyCodeType)Enum.Parse(typeof(CurrencyCodeType), AppLogic.AppConfig("Localization.StoreCurrency"), true);

                    DoReauthorizationRequestType Reauth = new DoReauthorizationRequestType();
                    Reauth.Amount = totalAmount;
                    Reauth.AuthorizationID = TransID;
                    Reauth.Version = API_VER;

                    DoReauthorizationReq ReauthReq = new DoReauthorizationReq();

                    ReauthReq.DoReauthorizationRequest = Reauth;

                    DoReauthorizationResponseType ReauthResponse;
                    ReauthResponse = (DoReauthorizationResponseType)IPayPal.DoReauthorization(ReauthReq);

                    //if (LogToErrorTable)
                    //{
                    //    PayPalController.Log(XmlCommon.SerializeObject(ReauthReq, ReauthReq.GetType()), "DoReauthorization Request");
                    //    PayPalController.Log(XmlCommon.SerializeObject(ReauthResponse, ReauthResponse.GetType()), "DoReauthorization Response"); 
                    //}

                    if (ReauthResponse != null && ReauthResponse.Ack.ToString().StartsWith("success", StringComparison.InvariantCultureIgnoreCase))
                    {
                        result = AppLogic.ro_OK;
                        DB.ExecuteSQL("update orders set AuthorizedOn=getdate(), AuthorizationPNREF=AuthorizationPNREF+'|REAU=" + ReauthResponse.AuthorizationID + "' where OrderNumber=" + OrderNumber.ToString());
                    }
                    else
                    {
                        if (ReauthResponse.Errors != null)
                        {
                            bool first = true;
                            for (int ix = 0; ix < ReauthResponse.Errors.Length; ix++)
                            {
                                if (!first)
                                {
                                    result += ", ";
                                }
                                result += "Error: [" + ReauthResponse.Errors[ix].ErrorCode + "] " + ReauthResponse.Errors[ix].LongMessage;
                                first = false;
                            }
                        }
                    }

                }
                catch
                {
                    result = "NO RESPONSE FROM GATEWAY!";
                }
            }
            return result;
        }

        public override bool SupportsPostProcessingEdits()
        {
            return false;
        }

        public override string DisplayName(string localeSetting)
        {
            return "PayPal Payments Pro";
        }

        public override string RecurringBillingCreateSubscription(String SubscriptionDescription, Customer ThisCustomer, Address UseBillingAddress, Address UseShippingAddress, Decimal RecurringAmount, DateTime StartDate, int RecurringInterval, DateIntervalTypeEnum RecurringIntervalType, int OriginalRecurringOrderNumber, string XID, IDictionary<string, string> TransactionContext, out String RecurringSubscriptionID, out String RecurringSubscriptionCommand, out String RecurringSubscriptionResult)
        {
            string result = string.Empty;

            try
            {
                //Re-Use the Internal Gateway Recurring Billing logic for calculating how much of the order is recurring
                ShoppingCart recurringCart = new ShoppingCart(ThisCustomer.SkinID, ThisCustomer, CartTypeEnum.RecurringCart, OriginalRecurringOrderNumber, false);

                CreditCardDetailsType creditCard = new CreditCardDetailsType();

                if (UseBillingAddress.CardNumber != null && UseBillingAddress.CardNumber.Length > 0)
                {
                    creditCard.CreditCardNumber = UseBillingAddress.CardNumber;
                    creditCard.ExpMonth = Localization.ParseUSInt(UseBillingAddress.CardExpirationMonth);
                    creditCard.ExpYear = Localization.ParseUSInt(UseBillingAddress.CardExpirationYear);
                    creditCard.ExpMonthSpecified = true;
                    creditCard.ExpYearSpecified = true;
                    creditCard.CVV2 = XID;
                    creditCard.CreditCardType = (CreditCardTypeType)Enum.Parse(typeof(CreditCardTypeType), UseBillingAddress.CardType, true);
                    creditCard.CreditCardTypeSpecified = true;
                }
                else
                {
                    creditCard.CreditCardTypeSpecified = false;
                }

                BasicAmountType recurringAmount = new BasicAmountType();
                recurringAmount.currencyID = (CurrencyCodeType)Enum.Parse(typeof(CurrencyCodeType), AppLogic.AppConfig("Localization.StoreCurrency"), true);
                recurringAmount.Value = RecurringAmount.ToString();

                DateIntervalTypeEnum recurringIntervalType = recurringCart.CartItems[0].RecurringIntervalType; //We currently only support 1 interval per recurring order, so grabbing the first as a default should be safe
                int recurringInterval = recurringCart.CartItems[0].RecurringInterval;

                BillingPeriodDetailsType billingPeriodDetails = PayPalController.GetECRecurringPeriodDetails(recurringIntervalType, recurringInterval);
                billingPeriodDetails.Amount = recurringAmount;
                billingPeriodDetails.TotalBillingCyclesSpecified = false;

                ScheduleDetailsType scheduleDetails = new ScheduleDetailsType();
                scheduleDetails.Description = string.Format("Recurring order created on {0} from {1}", System.DateTime.Now.ToShortDateString(), AppLogic.AppConfig("StoreName"));
                scheduleDetails.MaxFailedPayments = 0;
                scheduleDetails.MaxFailedPaymentsSpecified = true;
                scheduleDetails.AutoBillOutstandingAmount = AutoBillType.NoAutoBill;
                scheduleDetails.AutoBillOutstandingAmountSpecified = true;
                scheduleDetails.PaymentPeriod = billingPeriodDetails;

                RecurringPaymentsProfileDetailsType profileDetails = new RecurringPaymentsProfileDetailsType();
                profileDetails.SubscriberName = ThisCustomer.FirstName + " " + ThisCustomer.LastName;
                profileDetails.BillingStartDate = StartDate;

                CreateRecurringPaymentsProfileRequestDetailsType profileRequestDetails = new CreateRecurringPaymentsProfileRequestDetailsType();
                profileRequestDetails.ScheduleDetails = scheduleDetails;
                profileRequestDetails.RecurringPaymentsProfileDetails = profileDetails;
                profileRequestDetails.CreditCard = creditCard;

                if (!(UseBillingAddress.CardNumber != null && UseBillingAddress.CardNumber.Length > 0))
                    profileRequestDetails.Token = XID;

                if (recurringCart.IsAllDownloadComponents())
                {
                    PaymentDetailsItemType paymentDetailsItem = new PaymentDetailsItemType();
                    paymentDetailsItem.ItemCategory = ItemCategoryType.Digital;
                    paymentDetailsItem.ItemCategorySpecified = true;

                    List<PaymentDetailsItemType> paymentDetailsList = new List<PaymentDetailsItemType>();
                    paymentDetailsList.Add(paymentDetailsItem);

                    profileRequestDetails.PaymentDetailsItem = paymentDetailsList.ToArray();
                }

                CreateRecurringPaymentsProfileRequestType profileRequest = new CreateRecurringPaymentsProfileRequestType();
                profileRequest.Version = API_VER;
                profileRequest.CreateRecurringPaymentsProfileRequestDetails = profileRequestDetails;

                CreateRecurringPaymentsProfileReq request = new CreateRecurringPaymentsProfileReq();
                request.CreateRecurringPaymentsProfileRequest = profileRequest;

                CreateRecurringPaymentsProfileResponseType profileResponse = new CreateRecurringPaymentsProfileResponseType();
                profileResponse = IPayPal.CreateRecurringPaymentsProfile(request);

                if (profileResponse != null && profileResponse.Ack.ToString().StartsWith("success", StringComparison.InvariantCultureIgnoreCase))
                {
                    result = AppLogic.ro_OK;
                }
                else
                {
                    if (profileResponse.Errors != null)
                    {
                        bool first = true;
                        for (int ix = 0; ix < profileResponse.Errors.Length; ix++)
                        {
                            if (!first)
                            {
                                result += ", ";
                            }
                            result += profileResponse.Errors[ix].LongMessage;
                            first = false;
                        }
                    }
                }

                RecurringSubscriptionID = (profileResponse.CreateRecurringPaymentsProfileResponseDetails.ProfileID == null ? "No ProfileID provided" : profileResponse.CreateRecurringPaymentsProfileResponseDetails.ProfileID);
                RecurringSubscriptionCommand = string.Empty;
                RecurringSubscriptionResult = (profileResponse.CreateRecurringPaymentsProfileResponseDetails.DCCProcessorResponse == null ? "No response provided" : profileResponse.CreateRecurringPaymentsProfileResponseDetails.DCCProcessorResponse);

                //Log the transaction
                OrderTransactionCollection ecRecurringOrderTransaction = new OrderTransactionCollection(OriginalRecurringOrderNumber);
                ecRecurringOrderTransaction.AddTransaction("PayPal Express Checkout Recurring Profile Creation",
                    request.ToString(),
                    result,
                    string.Empty,
                    (profileResponse.CreateRecurringPaymentsProfileResponseDetails.ProfileID == null ? "No ProfileID provided" : profileResponse.CreateRecurringPaymentsProfileResponseDetails.ProfileID),
                    AppLogic.ro_PMPayPalExpress,
                    null,
                    RecurringAmount);
            }
            catch
            {
                result = "Recurring Profile Creation Failed.";
                RecurringSubscriptionID = string.Empty;
                RecurringSubscriptionCommand = string.Empty;
                RecurringSubscriptionResult = result;
            }
            return result;
        }

        public override string RecurringBillingCancelSubscription(String RecurringSubscriptionID, int OriginalRecurringOrderNumber, IDictionary<string, string> TransactionContext)
        {
            string profileID = string.Empty;
            string result = string.Empty;

            profileID = PayPalController.GetPPECProfileID(OriginalRecurringOrderNumber);

            if (profileID != string.Empty)
            {
                ManageRecurringPaymentsProfileStatusRequestDetailsType recurringRequestDetails = new ManageRecurringPaymentsProfileStatusRequestDetailsType();
                recurringRequestDetails.Action = StatusChangeActionType.Cancel;
                recurringRequestDetails.ProfileID = profileID;

                ManageRecurringPaymentsProfileStatusRequestType recurringRequest = new ManageRecurringPaymentsProfileStatusRequestType();
                recurringRequest.ManageRecurringPaymentsProfileStatusRequestDetails = recurringRequestDetails;
                recurringRequest.Version = API_VER;

                ManageRecurringPaymentsProfileStatusReq profileStatusRequest = new ManageRecurringPaymentsProfileStatusReq();
                profileStatusRequest.ManageRecurringPaymentsProfileStatusRequest = recurringRequest;

                ManageRecurringPaymentsProfileStatusResponseType recurringResponse = new ManageRecurringPaymentsProfileStatusResponseType();
                recurringResponse = IPayPal.ManageRecurringPaymentsProfileStatus(profileStatusRequest);

                if (recurringResponse != null && recurringResponse.Ack.ToString().StartsWith("success", StringComparison.InvariantCultureIgnoreCase))
                {
                    result = AppLogic.ro_OK;
                }
                else
                {
                    if (recurringResponse.Errors != null)
                    {
                        bool first = true;
                        for (int ix = 0; ix < recurringResponse.Errors.Length; ix++)
                        {
                            if (!first)
                            {
                                result += ", ";
                            }
                            result += recurringResponse.Errors[ix].LongMessage;
                            first = false;
                        }
                    }
                }
            }
            else
            {
                result = "No matching Profile ID found for that order number";

                SysLog.LogMessage("An attempt was made to cancel a PayPal recurring order with no matching Profile ID",
                    "Original order ID was: " + OriginalRecurringOrderNumber.ToString(),
                    MessageTypeEnum.Informational,
                    MessageSeverityEnum.Alert);
            }

            return result;
        }
    }
}
