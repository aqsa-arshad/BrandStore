// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Web.Services.Protocols;
using System;
using System.Web;
using System.Security;
using System.Text;
using System.Web.SessionState;
using System.IO;
using System.Net;
using System.Data;
using System.Xml;
using System.Xml.Serialization;
using AspDotNetStorefrontCore;
using System.Collections;
using CyberSource.Clients;
using CyberSource.Clients.SoapWebReference;
using System.Data.SqlClient;

namespace AspDotNetStorefrontGateways.Processors
{
    /// <summary>
    /// For the CyberSource gateway you must have the following 
    /// System Requirements installed:
    ///   CyberSource Simple Order API for .NET Framework 2.0 SDK
    ///   Microsoft Web Services Enhancements (WSE) 3.0
    /// 
    /// Copy the files from the Cybersource installed \lib directory to Web\bin
    /// Copy the WSE installed Microsoft.Web.Services3.dll to Web\bin
    /// 
    /// The settings are in the Web.config file.
    /// 
    /// </summary>
    public class Cybersource : GatewayProcessor
    {
        public Cybersource() { }

        public override String CaptureOrder(Order o)
        {
            String result = AppLogic.ro_OK;

            o.CaptureTXCommand = "";
            o.CaptureTXResult = "";
            String TransID = o.AuthorizationPNREF;
            Decimal TotalAmount = o.OrderBalance;
            int CustomerID = o.CustomerID;

            Configuration conf = new Configuration();
            conf.KeysDirectory = AppLogic.AppConfig("CYBERSOURCE.keysDirectory");
            conf.KeyFilename = AppLogic.AppConfig("CYBERSOURCE.keyFilename");
            conf.MerchantID = AppLogic.AppConfig("CYBERSOURCE.merchantID");
            conf.ServerURL = CommonLogic.IIF(AppLogic.AppConfigBool("UseLiveTransactions"), AppLogic.AppConfig("CYBERSOURCE.LiveURL"), AppLogic.AppConfig("CYBERSOURCE.TestURL"));
            if (AppLogic.AppConfigBool("CYBERSOURCE.UsePIT"))
            {
                conf.ServerURL = AppLogic.AppConfig("CYBERSOURCE.PITURL");
            }

            RequestMessage request = new RequestMessage();

            request.clientApplication = "AspDotNetStorefront";
            request.clientApplicationVersion = AppLogic.AppConfig("StoreVersion");
            request.clientApplicationUser = CustomerID.ToString();

            request.merchantReferenceCode = "Order #: " + o.OrderNumber.ToString() + " " + Localization.ToNativeDateTimeString(System.DateTime.Now);

            request.merchantID = AppLogic.AppConfig("CYBERSOURCE.merchantID");

            request.ccCaptureService = new CCCaptureService();
            request.ccCaptureService.authRequestID = TransID;
            request.ccCaptureService.run = "true";

            PurchaseTotals ptotal = new PurchaseTotals();
            ptotal.currency = Localization.StoreCurrency();  // Currency REQUIRED
            ptotal.grandTotalAmount = Localization.CurrencyStringForGatewayWithoutExchangeRate(TotalAmount);
            request.purchaseTotals = ptotal; // Neccessary
            result = "ERROR: ";

            try
            {
                ReplyMessage reply = SoapClient.RunTransaction(conf, request);
                if (reply.ccCaptureReply.reasonCode == "100")
                {
                    result = AppLogic.ro_OK;
                }
                else
                {
                    result += GetReasonCodeDescription(reply.reasonCode);
                }



            }
            catch (SignException se)
            {
                result += String.Format("Failed to sign the request with error code {0} and message {1}.", DB.SQuote(se.ErrorCode.ToString()), DB.SQuote(se.Message));
            }
            catch (SoapHeaderException she)
            {
                result += String.Format("A SOAP header exception was returned with fault code {0} and message {1}.", DB.SQuote(she.Code.ToString()), DB.SQuote(she.Message));
            }
            catch (SoapBodyException sbe)
            {
                result += String.Format("A SOAP body exception was returned with fault code {0} and message {1}.", DB.SQuote(sbe.Code.ToString()), DB.SQuote(sbe.Message));
            }
            catch (WebException we)
            {
                result += String.Format("Failed to get a response with status {0} and message {1}", DB.SQuote(we.Status.ToString()), DB.SQuote(we.Message));
            }
            catch (Exception ex)
            {
                result += ex.Message;
            }
            o.CaptureTXResult = result;

            return result;
        }

        public override String VoidOrder(int OrderNumber)
        {
            String result = AppLogic.ro_OK;

            DB.ExecuteSQL("update orders set VoidTXCommand=NULL, VoidTXResult=NULL where OrderNumber=" + OrderNumber.ToString());
            String TransID = String.Empty;
            int CustomerID = 0;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select AuthorizationPNREF,CustomerID from orders   with (NOLOCK)  where OrderNumber=" + OrderNumber.ToString(), con))
                {
                    if (rs.Read())
                    {
                        TransID = DB.RSField(rs, "AuthorizationPNREF");
                        CustomerID = DB.RSFieldInt(rs, "CustomerID");
                    }
                }
            }

            Configuration conf = new Configuration();

            conf.KeysDirectory = AppLogic.AppConfig("CYBERSOURCE.keysDirectory");
            conf.KeyFilename = AppLogic.AppConfig("CYBERSOURCE.keyFilename");
            conf.MerchantID = AppLogic.AppConfig("CYBERSOURCE.merchantID");
            conf.ServerURL = CommonLogic.IIF(AppLogic.AppConfigBool("UseLiveTransactions"), AppLogic.AppConfig("CYBERSOURCE.LiveURL"), AppLogic.AppConfig("CYBERSOURCE.TestURL"));
            if (AppLogic.AppConfigBool("CYBERSOURCE.UsePIT"))
            {
                conf.ServerURL = AppLogic.AppConfig("CYBERSOURCE.PITURL");
            }

            RequestMessage request = new RequestMessage();

            request.clientApplication = "AspDotNetStorefront";
            request.clientApplicationVersion = AppLogic.AppConfig("StoreVersion");
            request.clientApplicationUser = CustomerID.ToString();

            request.merchantReferenceCode = "Order #: " + OrderNumber.ToString() + " " + Localization.ToNativeDateTimeString(System.DateTime.Now);

            request.merchantID = AppLogic.AppConfig("CYBERSOURCE.merchantID");

            request.voidService = new VoidService();
            request.voidService.voidRequestID = TransID;
            request.voidService.run = "true";

            result = "ERROR: ";

            try
            {
                ReplyMessage reply = SoapClient.RunTransaction(conf, request);
                if (reply.voidReply.reasonCode == "100")
                {
                    result = AppLogic.ro_OK;
                }
                else
                {
                    result += GetReasonCodeDescription(reply.reasonCode);
                }
            }
            catch (SignException se)
            {
                result += String.Format(
                "Failed to sign the request with error code {0} and message {1}.", DB.SQuote(se.ErrorCode.ToString()), DB.SQuote(se.Message));
            }
            catch (SoapHeaderException she)
            {
                result += String.Format("A SOAP header exception was returned with fault code {0} and message {1}.", DB.SQuote(she.Code.ToString()), DB.SQuote(she.Message));
            }
            catch (SoapBodyException sbe)
            {
                result += String.Format("A SOAP body exception was returned with fault code {0} and message {1}.", DB.SQuote(sbe.Code.ToString()), DB.SQuote(sbe.Message));
            }
            catch (WebException we)
            {
                result += String.Format("Failed to get a response with status {0} and message {1}", DB.SQuote(we.Status.ToString()), DB.SQuote(we.Message));
            }
            catch (Exception ex)
            {
                result += ex.Message;
            }
            DB.ExecuteSQL("update orders set VoidTXResult=" + DB.SQuote(result) + " where OrderNumber=" + OrderNumber.ToString());

            return result;
        }

        // if RefundAmount == 0.0M, then then ENTIRE order amount will be refunded!
        public override String RefundOrder(int OriginalOrderNumber, int NewOrderNumber, decimal RefundAmount, String RefundReason, AspDotNetStorefrontCore.Address UseBillingAddress)
        {
            String result = AppLogic.ro_OK; 

            DB.ExecuteSQL("update orders set RefundTXCommand=NULL, RefundTXResult=NULL where OrderNumber=" + OriginalOrderNumber.ToString());
            String TransID = String.Empty;
            int CustomerID = 0;
            decimal TotalAmount = 0;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select AuthorizationPNREF,OrderTotal,CustomerID from orders   with (NOLOCK)  where OrderNumber=" + OriginalOrderNumber.ToString(), con))
                {
                    if (rs.Read())
                    {
                        TransID = DB.RSField(rs, "AuthorizationPNREF");
                        TotalAmount = DB.RSFieldDecimal(rs, "OrderTotal");
                        CustomerID = DB.RSFieldInt(rs, "CustomerID");
                    }
                }
            }

            Configuration conf = new Configuration();
            conf.KeysDirectory = AppLogic.AppConfig("CYBERSOURCE.keysDirectory");
            conf.KeyFilename = AppLogic.AppConfig("CYBERSOURCE.keyFilename");
            conf.MerchantID = AppLogic.AppConfig("CYBERSOURCE.merchantID");
            conf.ServerURL = CommonLogic.IIF(AppLogic.AppConfigBool("UseLiveTransactions"), AppLogic.AppConfig("CYBERSOURCE.LiveURL"), AppLogic.AppConfig("CYBERSOURCE.TestURL"));
            if (AppLogic.AppConfigBool("CYBERSOURCE.UsePIT"))
            {
                conf.ServerURL = AppLogic.AppConfig("CYBERSOURCE.PITURL");
            }

            RequestMessage request = new RequestMessage();

            request.clientApplication = "AspDotNetStorefront";
            request.clientApplicationVersion = AppLogic.AppConfig("StoreVersion");
            request.clientApplicationUser = CustomerID.ToString();

            request.merchantReferenceCode = "Order #: " + OriginalOrderNumber.ToString() + " " + Localization.ToNativeDateTimeString(System.DateTime.Now);

            request.merchantID = AppLogic.AppConfig("CYBERSOURCE.merchantID");

            request.ccCreditService = new CCCreditService();
            request.ccCreditService.captureRequestID = TransID;
            request.ccCreditService.run = "true";

            if (RefundAmount == System.Decimal.Zero)
            {
                RefundAmount = TotalAmount;
            }

            PurchaseTotals ptotal = new PurchaseTotals();
            ptotal.currency = Localization.StoreCurrency();  // Currency REQUIRED
            ptotal.grandTotalAmount = Localization.CurrencyStringForGatewayWithoutExchangeRate(CommonLogic.IIF(RefundAmount > TotalAmount, TotalAmount, RefundAmount));
            request.purchaseTotals = ptotal; // Neccessary

            result = "ERROR: ";

            try
            {
                ReplyMessage reply = SoapClient.RunTransaction(conf, request);
                if (reply.ccCreditReply.reasonCode == "100")
                {
                    result = AppLogic.ro_OK;
                }
                else
                {
                    result += GetReasonCodeDescription(reply.reasonCode);
                }

            }
            catch (SignException se)
            {
                result += String.Format("Failed to sign the request with error code {0} and message {1}.", DB.SQuote(se.ErrorCode.ToString()), DB.SQuote(se.Message));
            }
            catch (SoapHeaderException she)
            {
                result += String.Format("A SOAP header exception was returned with fault code {0} and message {1}.", DB.SQuote(she.Code.ToString()), DB.SQuote(she.Message));
            }
            catch (SoapBodyException sbe)
            {
                result += String.Format("A SOAP body exception was returned with fault code {0} and message {1}.", DB.SQuote(sbe.Code.ToString()), DB.SQuote(sbe.Message));
            }
            catch (WebException we)
            {
                result += String.Format("Failed to get a response with status {0} and message {1}", DB.SQuote(we.Status.ToString()), DB.SQuote(we.Message));
            }
            catch (Exception ex)
            {
                result += ex.Message;
            }
            DB.ExecuteSQL("update orders set RefundTXResult=" + DB.SQuote(result) + " where OrderNumber=" + OriginalOrderNumber.ToString());

            return result;
        }

        public override String ProcessCard(int OrderNumber, int CustomerID, Decimal OrderTotal, bool useLiveTransactions, TransactionModeEnum TransactionMode, AspDotNetStorefrontCore.Address UseBillingAddress, String CardExtraCode, AspDotNetStorefrontCore.Address UseShippingAddress, String CAVV, String ECI, String XID, out String AVSResult, out String AuthorizationResult, out String AuthorizationCode, out String AuthorizationTransID, out String TransactionCommandOut, out String TransactionResponse)
        {

            AVSResult = "N/A";
            AuthorizationResult = "N/A";
            AuthorizationCode = "N/A";
            AuthorizationTransID = "N/A";
            TransactionCommandOut = "N/A";
            TransactionResponse = String.Empty;

            String signedPARes = String.Empty;
            String result = AppLogic.ro_OK;

            CustomerSession cSession = new CustomerSession(CustomerID);

            if (cSession["3Dsecure.PaRes"].Length != 0)
            {
                signedPARes = cSession["3Dsecure.PaRes"];
                // After grabbing it, clear out the session PaRes so it won't be re-used ever again.
                cSession["3Dsecure.PaRes"] = String.Empty;
            }

            Configuration conf = new Configuration();
            conf.KeysDirectory = AppLogic.AppConfig("CYBERSOURCE.keysDirectory");
            conf.KeyFilename = AppLogic.AppConfig("CYBERSOURCE.keyFilename");
            conf.MerchantID = AppLogic.AppConfig("CYBERSOURCE.merchantID");
            conf.ServerURL = CommonLogic.IIF(useLiveTransactions, AppLogic.AppConfig("CYBERSOURCE.LiveURL"), AppLogic.AppConfig("CYBERSOURCE.TestURL"));
            if (AppLogic.AppConfigBool("CYBERSOURCE.UsePIT"))
            {
                conf.ServerURL = AppLogic.AppConfig("CYBERSOURCE.PITURL");
            }


            RequestMessage request = new RequestMessage();

            request.clientApplication = "AspDotNetStorefront";
            request.clientApplicationVersion = AppLogic.AppConfig("StoreVersion");
            request.clientApplicationUser = CustomerID.ToString();

            request.merchantReferenceCode = "Order # " + OrderNumber.ToString() + " " + Localization.ToNativeDateTimeString(System.DateTime.Now);

            int CardTypeID = DB.GetSqlN("select CardTypeID N from CreditCardType where CardType = " + DB.SQuote(UseBillingAddress.CardType));
            bool Try3DSecure = CommonLogic.IntegerIsInIntegerList(CardTypeID, AppLogic.AppConfig("3DSECURE.CreditCardTypeIDs"));

            if (Try3DSecure)
            {
                if (signedPARes == String.Empty)
                {
                    request.payerAuthEnrollService = new PayerAuthEnrollService();
                    request.payerAuthEnrollService.run = "true";
                    if (AppLogic.AppConfig("CYBERSOURCE.paCountryCode") != "")
                    {
                        request.payerAuthEnrollService.countryCode = AppLogic.AppConfig("CYBERSOURCE.paCountryCode");
                    }
                    if (AppLogic.AppConfig("CYBERSOURCE.paMerchantName") != "")
                    {
                        request.payerAuthEnrollService.merchantName = AppLogic.AppConfig("CYBERSOURCE.paMerchantName");
                    }
                    if (AppLogic.AppConfig("CYBERSOURCE.paMerchantURL") != "")
                    {
                        request.payerAuthEnrollService.merchantURL = AppLogic.AppConfig("CYBERSOURCE.paMerchantURL");
                    }
                    request.payerAuthEnrollService.httpAccept = CommonLogic.ServerVariables("HTTP_ACCEPT");
                    request.payerAuthEnrollService.httpUserAgent = CommonLogic.ServerVariables("HTTP_USER_AGENT");
                }
                else
                {
                    request.payerAuthValidateService = new PayerAuthValidateService();
                    request.payerAuthValidateService.signedPARes = signedPARes;
                    request.payerAuthValidateService.run = "true";
                }
            }

            request.ccAuthService = new CCAuthService();
            request.ccAuthService.run = "true";

            if (CAVV.Trim().Length != 0)
            { // only gets set as a result of 3D Secure processing
                if (GetCardTypeFieldValue(UseBillingAddress.CardType) == "002")
                {  // for MasterCard
                    request.ccAuthService.xid = XID;
                    request.ccAuthService.cavv = CAVV;
                }
            }

            request.merchantID = AppLogic.AppConfig("CYBERSOURCE.merchantID");

            if (TransactionMode == TransactionModeEnum.authcapture)
            {
                request.ccCaptureService = new CCCaptureService();
                request.ccCaptureService.run = "true";
            }

            BillTo billTo = new BillTo();
            billTo.firstName = UseBillingAddress.FirstName;
            billTo.lastName = UseBillingAddress.LastName;
            billTo.company = UseBillingAddress.Company;
            billTo.street1 = UseBillingAddress.Address1;
            billTo.street2 = UseBillingAddress.Address2;
            billTo.city = UseBillingAddress.City;
            billTo.state = UseBillingAddress.State;
            billTo.postalCode = UseBillingAddress.Zip;
            billTo.country = AppLogic.GetCountryTwoLetterISOCode(UseBillingAddress.Country);
            billTo.phoneNumber = UseBillingAddress.Phone.PadRight(6, '1');
            billTo.email = CommonLogic.IIF(UseBillingAddress.EMail.Length > 2, UseBillingAddress.EMail, "null@cybersource.com");
            billTo.ipAddress = CommonLogic.CustomerIpAddress();
            request.billTo = billTo;

            if (UseShippingAddress != null)
            {
                ShipTo ShipTo = new ShipTo();
                ShipTo.firstName = UseShippingAddress.FirstName;
                ShipTo.lastName = UseShippingAddress.LastName;
                ShipTo.company = UseShippingAddress.Company;
                ShipTo.street1 = UseShippingAddress.Address1;
                ShipTo.street2 = UseShippingAddress.Address2;
                ShipTo.city = UseShippingAddress.City;
                ShipTo.state = UseShippingAddress.State;
                ShipTo.postalCode = UseShippingAddress.Zip;
                ShipTo.country = AppLogic.GetCountryTwoLetterISOCode(UseShippingAddress.Country);
                ShipTo.phoneNumber = UseShippingAddress.Phone.PadRight(6, '1');
                ShipTo.email = CommonLogic.IIF(UseShippingAddress.EMail.Length > 2, UseShippingAddress.EMail, "null@cybersource.com");
                request.shipTo = ShipTo;
            }

            Card card = new Card();
            card.accountNumber = UseBillingAddress.CardNumber;
            if (CardExtraCode.Trim().Length != 0)
            {
                card.cvIndicator = "1";
                card.cvNumber = CardExtraCode;
            }
            else
            {
                card.cvIndicator = "0";
            }

			if(!String.IsNullOrEmpty(UseBillingAddress.CardStartDate) && UseBillingAddress.CardStartDate != "00")
			{
				card.startMonth = UseBillingAddress.CardStartDate.Substring(0, 2);
				card.startYear = UseBillingAddress.CardStartDate.Substring(4, 2);
			}

            if (UseBillingAddress.CardIssueNumber.Length != 0)
            {
                card.issueNumber = UseBillingAddress.CardIssueNumber;
            }
            card.expirationMonth = UseBillingAddress.CardExpirationMonth;
            card.expirationYear = UseBillingAddress.CardExpirationYear;

            if (Try3DSecure)
            {
                card.cardType = GetCardTypeFieldValue(UseBillingAddress.CardType);
                if (card.cardType == "000")
                {
                    return " Error in configuration. Card type " + UseBillingAddress.CardType + " is not recognized by the gateway.";
                }
            }

            request.card = card;

            request.item = new Item[1];
            Item the_item = new Item();
            the_item.id = "0";
            the_item.unitPrice = Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal);
            request.item[0] = the_item;

            PurchaseTotals ptotal = new PurchaseTotals();
            ptotal.currency = Localization.StoreCurrency();  // Currency REQUIRED
            ptotal.grandTotalAmount = Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal); // Not needed - can use instead of item price, I think it's better..
            request.purchaseTotals = ptotal; // Neccessary

            result = "ERROR: ";


            try
            {
				ReplyMessage reply = SoapClient.RunTransaction(conf, request);

                if (Try3DSecure && request.payerAuthEnrollService != null)
                {
                    if (request.payerAuthEnrollService.run == "true")
                    { // we have some data that needs stored
                        string LookupResult = String.Empty;
                        if (reply.payerAuthEnrollReply != null 
                            && reply.payerAuthEnrollReply.paReq != null 
                            && reply.payerAuthEnrollReply.paReq.Length > 0)
                        { // will be null if card not enrolled
                            // the paReq comes back encoded, Streamline requires it to be decoded.
                            string sPAReq = CommonLogic.UnzipBase64DataToString(reply.payerAuthEnrollReply.paReq);
                            LookupResult += "paReq=" + XmlCommon.PrettyPrintXml(sPAReq) + System.Environment.NewLine;
                        }
                        if (reply.payerAuthEnrollReply != null
                            && reply.payerAuthEnrollReply.proofXML != null)
                        {
                            /****************************************************************
                             *  Store the complete proofXML whenever it is                  *
                             *  returned. If you ever need to show proof of                 *
                             *  enrollment checking, you will need to parse the string      *
                             *  for the information required by the card association.       *
                             ****************************************************************/
                            LookupResult += "proofXML data =";
                            LookupResult += System.Environment.NewLine + XmlCommon.PrettyPrintXml(reply.payerAuthEnrollReply.proofXML);
                        }
                        if (LookupResult != String.Empty)
                        { // encode it to store in the session, it will be decoded before being saved to the database
                            byte[] str = Encoding.UTF8.GetBytes(LookupResult);
                            cSession["3DSecure.LookupResult"] = Convert.ToBase64String(str);
                        }
                    }
                }

                if (reply.decision == "REJECT" && reply.reasonCode == "475")
                { // card enrolled, must perform 3D Secure processing (reasonCode == 475)
                    
                    cSession["3DSecure.CustomerID"] = CustomerID.ToString();
                    cSession["3DSecure.OrderNumber"] = OrderNumber.ToString();
                    cSession["3DSecure.MD"] = OrderNumber.ToString();
                    cSession["3DSecure.ACSUrl"] = reply.payerAuthEnrollReply.acsURL;
                    cSession["3DSecure.paReq"] = reply.payerAuthEnrollReply.paReq;
                    cSession["3DSecure.XID"] = reply.payerAuthEnrollReply.xid;
                    cSession.UpdateCustomerSession(null, null);
                    result = AppLogic.ro_3DSecure; // This is what triggers the 3D Secure IFRAME to be used.
                    return result;
                }

                if (reply.decision == "ACCEPT" || reply.decision == "REVIEW")
                {
                    result = AppLogic.ro_OK;
                    if (AppLogic.TransactionModeIsAuthCapture())
                    {
                        AVSResult = reply.ccAuthReply.avsCode;
                        AuthorizationResult = reply.ccCaptureReply.reasonCode;
                        AuthorizationCode = reply.ccAuthReply.authorizationCode;
                        AuthorizationTransID = reply.requestID;
                    }
                    else
                    {
                        AVSResult = reply.ccAuthReply.avsCode;
                        AuthorizationResult = reply.reasonCode;
                        AuthorizationCode = reply.ccAuthReply.authorizationCode;
                        AuthorizationTransID = reply.requestID;
                    }
                    if (signedPARes.Length > 0)
                    {
                        if (reply.payerAuthValidateReply != null)
                        {
                            if (reply.payerAuthValidateReply.ucafAuthenticationData != null)
                            { // MasterCard SecureCode
                                AuthorizationResult += System.Environment.NewLine + "CAVV: " + reply.payerAuthValidateReply.ucafAuthenticationData;
                                AuthorizationResult += System.Environment.NewLine + "ECI: " + reply.payerAuthValidateReply.ucafCollectionIndicator;
                            }
                            else
                            { // Visa VBV
                                AuthorizationResult += System.Environment.NewLine + "CAVV: " + reply.payerAuthValidateReply.cavv;
                                AuthorizationResult += System.Environment.NewLine + "ECI: " + reply.payerAuthValidateReply.eci;
                            }
                        }
                        AuthorizationResult += System.Environment.NewLine + "signedPARes: ";
                        // Streamline requires saving the decoded PARes to the database
                        string sPARes = CommonLogic.UnzipBase64DataToString(signedPARes);

                        // zap the signature since it is long and we don't need it
                        String t1 = "<Signature ";
                        String t2 = "</Signature>";
                        String sig = t1 + CommonLogic.ExtractToken(sPARes, t1, t2) + t2;
                        AuthorizationResult += System.Environment.NewLine + XmlCommon.PrettyPrintXml(sPARes.Replace(sig, ""));
                    }
                }
                else
                {
                    
                    result = "Your transaction was NOT approved, reason code: " + reply.reasonCode + ". ";
                    if (reply.reasonCode == "476" && reply.payerAuthValidateReply != null)
                    {
                        result += reply.payerAuthValidateReply.authenticationStatusMessage 
                                + ". Please try another payment method.";
                    }

                    else
                    {
                        result += GetReasonCodeDescription(reply.reasonCode);

                        if (reply.missingField != null)
                        {
                            foreach (string fieldname in reply.missingField)
                            {
                                result += "[" + fieldname + "]";
                            }
                        }

                        if (reply.invalidField != null)
                        {
                            foreach (string fieldname in reply.invalidField)
                            {
                                result += "[" + fieldname + "]";
                            }
                        }
                    }
                }
            }
            catch (SignException se)
            {
                result += "Error calling Cybersource gateway. Please retry your order in a few minutes or select another checkout payment option. "
                    + String.Format("Failed to sign the request with error code {0} and message {1}.", DB.SQuote(se.ErrorCode.ToString()), DB.SQuote(se.Message));
            }
            catch (SoapHeaderException she)
            {
                result += String.Format("A SOAP header exception was returned with fault code {0} and message {1}.", DB.SQuote(she.Code.ToString()), DB.SQuote(she.Message));
            }
            catch (SoapBodyException sbe)
            {
                result += String.Format("A SOAP body exception was returned with fault code {0} and message {1}.", DB.SQuote(sbe.Code.ToString()), DB.SQuote(sbe.Message));
            }
            catch (WebException we)
            {
                result += String.Format("Failed to get a response with status {0} and mmessage {1}", DB.SQuote(we.Status.ToString()), DB.SQuote(we.Message));
            }
            catch (Exception ex)
            {
                // See requirements at the top of this file.
                result += "Error calling Cybersource gateway. Please retry your order in a few minutes or select another checkout payment option.";
                result += " Error message: Make sure the required components for Cybersource are installed on the server. " + ex.Message;
                result += " <> " + ex.ToString();
            }
            return result;
        }

        static String GetCardTypeFieldValue(String CardType)
        {
            switch (CardType.ToUpperInvariant())
            {
                /* List based on Cybersource documentation for Payer Authentication. */
                case "VISA":
                    return "001";
                case "MASTERCARD":
                case "EUROCARD":
                    return "002";
                case "AMEX":
                case "AMERICANEXPRESS":
                case "AMERICAN EXPRESS":
                    return "003";
                case "DISCOVER":
                    return "004";
                case "DINERS":
                case "DINERSCLUB":
                case "DINERS CLUB":
                    return "005";
                case "CARTEBLANCHE":
                case "CARTE BLANCHE":
                    return "006";
                case "JCB":
                    return "007";
                case "OPTIMA":
                    return "008";
                case "ENROUTE":
                    return "0014";
                case "JAL":
                    return "021";
                case "MAESTRO":
                    return "024";
                case "SOLO":
                    return "032";
                case "VISAELECTRON":
                case "VISA ELECTRON":
                    return "033";
                case "DANKORT":
                    return "034";
                case "LASER":
                    return "035";
                case "CARTEBLEUE":
                case "CARTE BLEUE":
                    return "036";
                case "CARTASI":
                case "CARTA SI":
                    return "037";
                case "UATP":
                    return "038";
                default:
                    // unknown card type
                    return "000";
            }
        }

        static String GetReasonCodeDescription(String code)
        {
            switch (code)
            {
                /* List based on Cybersource documentation. */
                case "101":
                    return "The request is missing one or more required fields.";
                case "102":
                    return "One or more fields in the request contains invalid data.";
                case "150":
                    return "General system failure.";
                case "151":
                    return "The request was received but there was a server timeout. This error does not include timeouts between the client and the server.";
                case "152":
                    return "The request was received, but a service did not finish running in time.";
                case "200":
                    return "The authorization request was approved by the issuing bank but declined by CyberSource because it did not pass the Address Verification Service (AVS) check.";
                case "201":
                    return "The issuing bank has questions about the request. You do not receive an authorization code programmatically, but you might receive one verbally by calling the processor.";
                case "202":
                    return "Expired card. You might also receive this if the expiration date you provided does not match the date the issuing bank has on file.";
                case "203":
                    return "General decline of the card. No other information provided by the issuing bank.";
                case "204":
                    return "Insufficient funds in the account.";
                case "205":
                    return "Stolen or lost card.";
                case "207":
                    return "Issuing bank unavailable.";
                case "208":
                    return "Inactive card or card not authorized for card-not-present transactions.";
                case "209":
                    return "American Express Card Identification Digits (CID) did not match.";
                case "210":
                    return "The card has reached the credit limit.";
                case "211":
                    return "Invalid card verification number.";
                case "221":
                    return "The customer matched an entry on the processor’s negative file.";
                case "230":
                    return "The authorization request was approved by the issuing bank but declined by CyberSource because it did not pass the card verification (CV) check.";
                case "231":
                    return "Invalid account number.";
                case "232":
                    return "The card type is not accepted by the payment processor.";
                case "233":
                    return "General decline by the processor.";
                case "234":
                    return "There is a problem with your CyberSource merchant configuration.";
                case "235":
                    return "The requested amount exceeds the originally authorized amount. Occurs, for example, if you try to capture an amount larger than the original authorization amount.";
                case "236":
                    return "Processor failure.";
                case "237":
                    return "The authorization has already been reversed.";
                case "238":
                    return "The authorization has already been captured.";
                case "239":
                    return "The requested transaction amount must match the previous transaction amount.";
                case "240":
                    return "The card type sent is invalid or does not correlate with the credit card number.";
                case "241":
                    return "The request ID is invalid.";
                case "242":
                    return "You requested a capture, but there is no corresponding, unused authorization record. Occurs if there was not a previously successful authorization request or if the previously successful authorization has already been used by another capture request.";
                case "246":
                    return "The capture or credit is not voidable because the capture or credit information has already been submitted to your processor. Or, you requested a void for a type of transaction that cannot be voided.";
                case "247":
                    return "You requested a credit for a capture that was previously voided.";
                case "250":
                    return "The request was received, but there was a timeout at the payment processor.";
                case "476":
                    return "The Card Authentication failed. Please try another payment method.";
                default:
                    return "";
            }
        }

        static public void LogObject(Object pObject, System.Type objectType)
        {

            FileStream fs = new FileStream(CommonLogic.SafeMapPath("/cybersourcelog.txt"), FileMode.Append, FileAccess.Write, FileShare.Read | FileShare.Delete);
            StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);

            if (objectType.ToString().Contains("Reply"))
            {
                sw.WriteLine(System.Environment.NewLine + System.Environment.NewLine + "**** Reply: {0:G} ****", DateTime.Now);
            }
            else
            {
                sw.WriteLine(System.Environment.NewLine + System.Environment.NewLine + "**** Request: {0:G} ****", DateTime.Now);
            }
            XmlSerializer xs = new XmlSerializer(objectType);
            xs.Serialize(sw, pObject);

            sw.Close();
            fs.Close();
        }

        static public void LogTime(String msg)
        {
            FileStream fs = new FileStream(CommonLogic.SafeMapPath("/cybersourcelogID.txt"), FileMode.Append, FileAccess.Write, FileShare.Read | FileShare.Delete);
            StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
            sw.AutoFlush = true;
            sw.WriteLine(System.Environment.NewLine + "**** {0:G} **** {1}", DateTime.Now, msg);
            sw.Close();
            fs.Close();
        }

    }
}
