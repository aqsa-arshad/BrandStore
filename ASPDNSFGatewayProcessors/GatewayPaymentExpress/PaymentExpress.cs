// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontGateways.Processors
{
    /// <summary>
    /// Implements the Payment Express gateway
    /// </summary>
    public class PaymentExpress: GatewayProcessor
    {
        public override string ProcessCard(int OrderNumber, int CustomerID, decimal OrderTotal, bool useLiveTransactions, TransactionModeEnum TransactionMode, Address UseBillingAddress, string CardExtraCode, Address UseShippingAddress, string CAVV, string ECI, string XID, out string AVSResult, out string AuthorizationResult, out string AuthorizationCode, out string AuthorizationTransID, out string TransactionCommandOut, out string TransactionResponse)
        {
            AVSResult = string.Empty;            // AVS Result Code not returned from this gateway
            AuthorizationResult = string.Empty;  // assigned to empty strings in case a web error returns prematurely
            AuthorizationCode = string.Empty;
            AuthorizationTransID = string.Empty;
            TransactionResponse = string.Empty;

            // payment express request and API credentials
            pe.ChargeRequest chargeReq = new pe.ChargeRequest();
            chargeReq.PostUsername = AppLogic.AppConfig("PaymentExpress.Username");
            chargeReq.PostPassword = AppLogic.AppConfig("PaymentExpress.Password");

            // charge amount
            chargeReq.Amount = Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal);
            chargeReq.InputCurrency = Localization.StoreCurrency();

            // card details
            chargeReq.CardHolderName = UseBillingAddress.CardName;
            chargeReq.CardNumber = UseBillingAddress.CardNumber;
            chargeReq.DateStart = UseBillingAddress.CardStartDate;
            chargeReq.DateExpiry = UseBillingAddress.CardExpirationMonth.PadLeft(2, '0') + UseBillingAddress.CardExpirationYear.Substring(2, 2);
            chargeReq.Cvc2 = CardExtraCode;

            // address verification
            chargeReq.AvsAction = "1"; // Attempt AVS check
            chargeReq.AvsStreetAddress = UseBillingAddress.Address1;
            chargeReq.AvsPostCode = UseBillingAddress.Zip;

            // charge request details
            chargeReq.TxnType = TransactionMode == TransactionModeEnum.auth ? "Auth" : "Purchase";
            chargeReq.TxnId = OrderNumber.ToString();
            chargeReq.MerchantReference = AppLogic.AppConfig("StoreName") + " Order " + OrderNumber.ToString() + (TransactionMode == TransactionModeEnum.auth ? " Auth" : " Purchase");

            // serialize ChargeRequest class
            XmlSerializer serChargeRequest = new XmlSerializer(typeof(pe.ChargeRequest));
            StringWriter swChargeRequest = new StringWriter();
            serChargeRequest.Serialize(swChargeRequest, chargeReq);
            string req = swChargeRequest.ToString();
            TransactionCommandOut = req;

            // send request to payment express
            string resp;
            string sendResult = SendToPaymentExpress(req, out resp);

            // Purge the card number from the response
            resp = Regex.Replace(resp, "<CardNumber>.*</CardNumber>", "<CardNumber></CardNumber>");
            TransactionResponse = resp;

            // if communication was not a success, an error message is returned
            if (sendResult != AppLogic.ro_OK) return sendResult;

            // deserialize the xml response into a ChargeResponse object
            pe.ChargeResponse response = new pe.ChargeResponse();
            XmlSerializer serResponse = new XmlSerializer(typeof(pe.ChargeResponse));
            StringReader srResponse = new StringReader(resp);

            try
            {
                response = (pe.ChargeResponse)serResponse.Deserialize(srResponse);
            }
            catch (InvalidOperationException e)  // invalid xml, or no reply received from Payment Express
            {
                return "Could not parse response from Payment Express server: " + e.Message + " Response received: " + resp;
            }

            srResponse.Close();

            // Check the response object for fault
            if (response.Success != null && response.Success == "1")
            {
                AVSResult = string.Empty;  // AVS Result Code not returned from this gateway
                if (response.Transaction != null)
                {
                    AuthorizationResult = response.Transaction.Success;
                    AuthorizationCode = response.Transaction.AuthCode;
                    AuthorizationTransID = (TransactionMode == TransactionModeEnum.auth ? "AUTH=" : "CAPTURE=") + response.Transaction.DpsTxnRef;
                }

                return AppLogic.ro_OK;
            }
            else
            {
                if (response.Transaction != null)
                {
                    return response.Transaction.CardHolderHelpText;
                }
                else
                {
                    return "Transaction failed";
                }
            }
        }

        /// <summary>
        /// Performs a credit card auth or auth/capture transaction using the Payment Express gateway
        /// </summary>
        //public static string ProcessCard(int OrderNumber, int CustomerID, decimal OrderTotal, bool useLiveTransactions, string TransactionMode, Address UseBillingAddress,
        //    string CardExtraCode, Address UseShippingAddress, string CAVV, string ECI, string XID, out string AVSResult, out string AuthorizationResult,
        //    out string AuthorizationCode, out string AuthorizationTransID, out string TransactionCommandOut, out string TransactionResponse)
        //{
            
        //}

        /// <summary>
        /// Captures a previously-authorized transaction using the Payment Express gateway
        /// </summary>
        public override string CaptureOrder(Order o)
        {
            // payment express request and API credentials
            pe.CaptureRequest captureReq = new pe.CaptureRequest();
            captureReq.PostUsername = AppLogic.AppConfig("PaymentExpress.Username");
            captureReq.PostPassword = AppLogic.AppConfig("PaymentExpress.Password");

            // capture amount
            captureReq.Amount = Localization.CurrencyStringForGatewayWithoutExchangeRate(o.Total(true));
            captureReq.MerchantReference = AppLogic.AppConfig("StoreName") + " Order " + o.OrderNumber.ToString() + " Capture";

            // capture request details
            captureReq.DpsTxnRef = Regex.Match(o.AuthorizationPNREF, @"(?<=AUTH=)\w+").ToString();

            // serialize CaptureRequest class
            XmlSerializer serCaptureRequest = new XmlSerializer(typeof(pe.CaptureRequest));
            StringWriter swCaptureRequest = new StringWriter();
            serCaptureRequest.Serialize(swCaptureRequest, captureReq);
            string req = swCaptureRequest.ToString();

            // send request to payment express
            string resp;
            string sendResult = SendToPaymentExpress(req, out resp);

            // if communication was not a success, an error message is returned
            if (sendResult != AppLogic.ro_OK) return sendResult;

            // deserialize the xml response into a CaptureResponse object
            pe.CaptureResponse response = new pe.CaptureResponse();
            XmlSerializer serResponse = new XmlSerializer(typeof(pe.CaptureResponse));
            StringReader srResponse = new StringReader(resp);

            try
            {
                response = (pe.CaptureResponse) serResponse.Deserialize(srResponse);
            }
            catch (InvalidOperationException e)  // invalid xml, or no reply received from Payment Express
            {
                return "Could not parse response from Payment Express server: " + e.Message + " Response received: " + resp;
            }

            srResponse.Close();

            // Check the response object for fault
            if (response.Success != null && response.Success == "1")
            {
                o.AuthorizationPNREF += "|CAPTURE=" + response.Transaction.DpsTxnRef;
                return AppLogic.ro_OK;
            }
            else
            {
                if (response.Transaction != null)
                {
                    return response.Transaction.CardHolderHelpText;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Performs a refund transaction using the Payment Express gateway
        /// </summary>
        public override string RefundOrder(int originalOrderNumber, int newOrderNumber, decimal refundAmount, string refundReason, Address useBillingAddress)
        {
            // retrieve transaction details from db
            string transId = string.Empty;
            decimal orderTotal = 0;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select AuthorizationPNREF, OrderTotal from dbo.orders with (NOLOCK) where OrderNumber = " + originalOrderNumber.ToString(), con))
                {
                    if (rs.Read())
                    {
                        transId = Regex.Match(DB.RSField(rs, "AuthorizationPNREF"), @"(?<=CAPTURE=)\w+").ToString();
                        orderTotal = DB.RSFieldDecimal(rs, "OrderTotal");
                    }
                }
            }

            // payment express request and API credentials
            pe.RefundRequest refundReq = new pe.RefundRequest();
            refundReq.PostUsername = AppLogic.AppConfig("PaymentExpress.Username");
            refundReq.PostPassword = AppLogic.AppConfig("PaymentExpress.Password");

            // refund amount
            if (refundAmount == 0)
            {
                refundReq.Amount = Localization.CurrencyStringForGatewayWithoutExchangeRate(orderTotal);
            }
            else
            {
                refundReq.Amount = Localization.CurrencyStringForGatewayWithoutExchangeRate(refundAmount);
            }

            refundReq.MerchantReference = AppLogic.AppConfig("StoreName") + " Order " + originalOrderNumber.ToString() + " Refund";

            // refund request details
            refundReq.DpsTxnRef = transId;

            // serialize RefundRequest class
            XmlSerializer serRefundRequest = new XmlSerializer(typeof(pe.RefundRequest));
            StringWriter swRefundRequest = new StringWriter();
            serRefundRequest.Serialize(swRefundRequest, refundReq);
            string req = swRefundRequest.ToString();

            // send request to payment express
            string resp;
            string sendResult = SendToPaymentExpress(req, out resp);

            // if communication was not a success, an error message is returned
            if (sendResult != AppLogic.ro_OK) return sendResult;

            // deserialize the xml response into a RefundResponse object
            pe.RefundResponse response = new pe.RefundResponse();
            XmlSerializer serResponse = new XmlSerializer(typeof(pe.RefundResponse));
            StringReader srResponse = new StringReader(resp);

            try
            {
                response = (pe.RefundResponse) serResponse.Deserialize(srResponse);
            }
            catch (InvalidOperationException e)  // invalid xml, or no reply received from Payment Express
            {
                return "Could not parse response from Payment Express server: " + e.Message + " Response received: " + resp;
            }

            srResponse.Close();

            // Check the response object for fault
            if (response.Success != null && response.Success == "1")
            {
                DB.ExecuteSQL("update orders set AuthorizationPNREF=AuthorizationPNREF+'|REFUND=" + response.Transaction.DpsTxnRef + "' where OrderNumber=" + originalOrderNumber.ToString());
                return AppLogic.ro_OK;
            }
            else
            {
                if (response.Transaction != null)
                {
                    return response.Transaction.CardHolderHelpText;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Performs a void transaction using the Payment Express gateway
        /// </summary>
        public override string VoidOrder(int OrderNumber)
        {
            return "Payment Express does not support Void via their API. Void the payment at the Payment Express website, then mark the order as voided using 'Force Void'.";
        }

        /// <summary>
        /// Sends an XML request to Payment Express, and receives a response back
        /// </summary>
        private static string SendToPaymentExpress(string req, out string resp)
        {
            resp = string.Empty;

            // payment express endpoint
            string postUrl = AppLogic.AppConfig("PaymentExpress.Url");
            if (postUrl.Length == 0) postUrl = "https://www.paymentexpress.com/pxpost.aspx";

            // Send xml rate request to Payment Express server
            WebRequest webReq = WebRequest.Create(postUrl);
            webReq.Method = "POST";
            webReq.ContentType = "application/x-www-form-urlencoded";

            // Transmit the request to Payment Express
            byte[] data = System.Text.Encoding.ASCII.GetBytes(req);
            webReq.ContentLength = data.Length;
            Stream requestStream;

            try
            {
                requestStream = webReq.GetRequestStream();
            }
            catch (WebException e)  // could not connect to Payment Express endpoint
            {
                return "Tried to reach Payment Express Server (" + postUrl + "): " + e.Message;
            }

            requestStream.Write(data, 0, data.Length);
            requestStream.Close();

            // get the response from Payment Express
            WebResponse webResp = null;

            try
            {
                webResp = webReq.GetResponse();
            }
            catch (WebException e)  // could not receive a response from Payment Express endpoint
            {
                return "No response from Payment Express Server (" + postUrl + "): " + e.Message;
            }

            using (StreamReader sr = new StreamReader(webResp.GetResponseStream()))
            {
                resp = sr.ReadToEnd();
                sr.Close();
            }
            webResp.Close();

            return AppLogic.ro_OK;
        }
    }

    namespace pe
    {
        [XmlRoot(ElementName="Txn")]
        public partial class ChargeRequest
        {
            public string PostUsername;
            public string PostPassword;
            public string CardHolderName;
            public string CardNumber;
            public string Amount;
            public string Cvc2;
            public string DateStart;
            public string DateExpiry;
            public string IssueNumber;
            public string InputCurrency;
            public string TxnType;
            public string TxnId;
            public string EnableAvsData;
            public string AvsAction;
            public string AvsStreetAddress;
            public string AvsPostCode;
            public string MerchantReference;
        }

        [XmlRoot(ElementName="Txn")]
        public partial class ChargeResponse
        {
            public string ReCo;
            public string ResponseText;
            public string HelpText;
            public string Success;
            public string TxnRef;
            public TxnTransaction Transaction;
        }

        [XmlRoot(ElementName = "Txn")]
        public partial class CaptureRequest
        {
            public string PostUsername;
            public string PostPassword;
            public string DpsTxnRef;
            public string Amount;
            public string InputCurrency;
            public string TxnType = "Complete";
            public string MerchantReference;
        }

        [XmlRoot(ElementName = "Txn")]
        public partial class CaptureResponse
        {
            public string ReCo;
            public string ResponseText;
            public string HelpText;
            public string Success;
            public string TxnRef;
            public TxnTransaction Transaction;
        }

        [XmlRoot(ElementName = "Txn")]
        public partial class RefundRequest
        {
            public string PostUsername;
            public string PostPassword;
            public string DpsTxnRef;
            public string Amount;
            public string InputCurrency;
            public string TxnType = "Refund";
            public string MerchantReference;
        }

        [XmlRoot(ElementName = "Txn")]
        public partial class RefundResponse
        {
            public string ReCo;
            public string ResponseText;
            public string HelpText;
            public string Success;
            public string TxnRef;
            public TxnTransaction Transaction;
        }

        public partial class TxnTransaction
        {
            public string Authorized;
            public string MerchantReference;
            public string Cvc2;
            public string CardName;
            public string Retry;
            public string StatusRequired;
            public string AuthCode;
            public string Amount;
            public string InputCurrencyId;
            public string InputCurrencyName;
            public string CurrencyId;
            public string CurrencyName;
            public string CurrencyRate;
            public string CardHolderName;
            public string DateSettlement;
            public string TxnType;
            public string CardNumber;
            public string DateExpiry;
            public string ProductId;
            public string AcquirerTime;
            public string TestMode;
            public string CardId;
            public string CardHolderResponseText;
            public string CardHolderHelpText;
            public string CardHolderResponseDescription;
            public string MerchantResponseText;
            public string MerchantHelpText;
            public string MerchantResponseDescription;
            public string groupAccount;
            public string DpsTxnRef;
            public string AllowRetry;
            public string DpsBillingId;
            public string BillingId;
            public string TransactionId;
            public TxnTransactionAcquirer Acquirer;
            public TxnTransactionAcquirerDate AcquirerDate;
            public TxnTransactionAcquirerId AcquirerId;
            public string Success;
            public string Reco;
            public string Responsetext;
        }

        public partial class TxnTransactionAcquirer
        {
            public string Value;
        }

        public partial class TxnTransactionAcquirerDate
        {
            public string Value;
        }

        public partial class TxnTransactionAcquirerId
        {
            public string Value;
        }
    }
}
