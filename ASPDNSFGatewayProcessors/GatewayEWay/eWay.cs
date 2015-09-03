// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Web;
using System.Security;
using System.Text;
using System.Xml;
using System.Web.SessionState;
using System.IO;
using System.Net;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontGateways.Processors
{
    /// <summary>
    /// Summary description for eWay.
    /// </summary>
    public class eWay : GatewayProcessor
    {
        public eWay() { }

        public override String CaptureOrder(Order o)
        {
            String result = "eWay Gateway does not support delayed CAPTURE calls";
            return result;
        }

        public override String VoidOrder(int OrderNumber)
        {
            String result = "eWay Gateway does not support VOID calls";
            return result;
        }

        // if RefundAmount == 0.0M, then then ENTIRE order amount will be refunded!
        public override String RefundOrder(int OriginalOrderNumber, int NewOrderNumber, decimal RefundAmount, String RefundReason, Address UseBillingAddress)
        {
            String result = "eWay Gateway does not support REFUND calls";
            return result;
        }

        public override String ProcessCard(int OrderNumber, int CustomerID, Decimal OrderTotal, bool useLiveTransactions, TransactionModeEnum TransactionMode, Address UseBillingAddress, String CardExtraCode, Address UseShippingAddress, String CAVV, String ECI, String XID, out String AVSResult, out String AuthorizationResult, out String AuthorizationCode, out String AuthorizationTransID, out String TransactionCommandOut, out String TransactionResponse)
        {
            String result = AppLogic.ro_OK;

            AuthorizationCode = String.Empty;
            AuthorizationResult = String.Empty;
            AuthorizationTransID = String.Empty;
            AVSResult = String.Empty;
            TransactionCommandOut = String.Empty;
            TransactionResponse = String.Empty;

            String EMail = UseBillingAddress.EMail;

            StringBuilder transactionCommand = new StringBuilder(4096);
            transactionCommand.Append("<ewaygateway>");
            transactionCommand.Append("<ewayCustomerID>" + CommonLogic.IIF(useLiveTransactions,AppLogic.AppConfig("eWay.Live.CustomerID"),AppLogic.AppConfig("eWay.Test.CustomerID")) + "</ewayCustomerID>");
            transactionCommand.Append("<ewayTotalAmount>" + Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal).Replace(".","") + "</ewayTotalAmount>"); // must go in cents
            transactionCommand.Append("<ewayCustomerFirstName>" + XmlCommon.XmlEncode(UseBillingAddress.FirstName) + "</ewayCustomerFirstName>");
            transactionCommand.Append("<ewayCustomerLastName>" + XmlCommon.XmlEncode(UseBillingAddress.LastName) + " </ewayCustomerLastName>");
            transactionCommand.Append("<ewayCustomerEmail>" + XmlCommon.XmlEncode(EMail) + "</ewayCustomerEmail>");
            transactionCommand.Append("<ewayCustomerAddress>" + XmlCommon.XmlEncode(UseBillingAddress.Address1) + "</ewayCustomerAddress>");
            transactionCommand.Append("<ewayCustomerPostcode>" + XmlCommon.XmlEncode(UseBillingAddress.Zip) + "</ewayCustomerPostcode>");
            transactionCommand.Append("<ewayCustomerInvoiceDescription>" + XmlCommon.XmlEncode(AppLogic.AppConfig("StoreName") + " Order #" + OrderNumber.ToString()) + "</ewayCustomerInvoiceDescription>");
            transactionCommand.Append("<ewayCustomerInvoiceRef>" + OrderNumber.ToString() + "</ewayCustomerInvoiceRef>");
            transactionCommand.Append("<ewayCardHoldersName>" + XmlCommon.XmlEncode(UseBillingAddress.CardName) + "</ewayCardHoldersName>");
            transactionCommand.Append("<ewayCardNumber>" + UseBillingAddress.CardNumber + "</ewayCardNumber>");
            transactionCommand.Append("<ewayCardExpiryMonth>" + UseBillingAddress.CardExpirationMonth.PadLeft(2, '0') + "</ewayCardExpiryMonth>");
            transactionCommand.Append("<ewayCardExpiryYear>" + UseBillingAddress.CardExpirationYear.Substring(2, 2) + "</ewayCardExpiryYear>");
            transactionCommand.Append("<ewayTrxnNumber>" + OrderNumber.ToString() + "</ewayTrxnNumber>");
            transactionCommand.Append("<ewayCVN>" + CardExtraCode.Trim() + "</ewayCVN>");
            transactionCommand.Append("<ewayOption1></ewayOption1>");
            transactionCommand.Append("<ewayOption2></ewayOption2>");
            transactionCommand.Append("<ewayOption3></ewayOption3>");
            transactionCommand.Append("</ewaygateway>");

            TransactionCommandOut = transactionCommand.ToString();

            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] data = encoding.GetBytes(transactionCommand.ToString());

            // Prepare web request...
            String AuthServer = CommonLogic.IIF(useLiveTransactions, AppLogic.AppConfig("eWay.Live.URL"), AppLogic.AppConfig("eWay.Test.URL"));
            if (CardExtraCode.Trim().Length == 0)
            {
                AuthServer = AuthServer.Replace("_cvn", "");
            }

            WebResponse myResponse;
            String rawResponseString = String.Empty;
            try
            {
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(AuthServer);
                myRequest.ContentType = "text/xml;charset=\"utf-8\"";
                myRequest.Accept = "text/xml";
                myRequest.Method = "POST";
                myRequest.ContentLength = data.Length;
                Stream newStream = myRequest.GetRequestStream();
                // Send the data.
                newStream.Write(data, 0, data.Length);
                newStream.Close();
                // get the response
                myResponse = myRequest.GetResponse();
                StreamReader sr = new StreamReader(myResponse.GetResponseStream());
                rawResponseString = sr.ReadToEnd();
                sr.Close();
                myResponse.Close();
            }
            catch
            {
                result = "NO RESPONSE FROM EWAY GATEWAY!";
            }

            // rawResponseString now has gateway response
            TransactionResponse = rawResponseString;

            XmlDocument Doc = new XmlDocument();
            Doc.LoadXml(rawResponseString);
            XmlNode Node = Doc.SelectSingleNode("ewayResponse");

            if (Node != null)
            {
                AuthorizationTransID = XmlCommon.XmlField(Node, "ewayTrxnReference");
                AuthorizationCode = XmlCommon.XmlField(Node, "ewayAuthCode");
                AVSResult = "Not Available";
                AuthorizationResult = XmlCommon.XmlField(Node, "ewayTrxnStatus");
                TransactionResponse = XmlCommon.XmlField(Node, "ewayTrxnError");
                if (AuthorizationResult.ToLower() == "true")
                {
                    result = AppLogic.ro_OK;
                }
                else
                {
                    result = TransactionResponse;
                }
            }
            else
            {
                result = "Error calling eWay Gatway, No Xml Document Returned";
            }

            return result;
        }

        public override bool SupportsPostProcessingEdits()
        {
            return false;
        }
    }
}
