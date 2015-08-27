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
using System.Web.SessionState;
using System.IO;
using System.Net;
using AspDotNetStorefrontCore;
using System.Data.SqlClient;

namespace AspDotNetStorefrontGateways.Processors
{
    /// <summary>
    /// Summary description for Paymentech.
    /// </summary>
    public class JetPay : GatewayProcessor
    {
        public JetPay() { }

        public override String CaptureOrder(Order o)
        {
            String result = AppLogic.ro_OK;

            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");
            o.CaptureTXCommand = "";
            o.CaptureTXResult = "";
            String TransID = o.AuthorizationPNREF;

            String CardNum = Security.UnmungeString(o.CardNumber, o.OrdersCCSaltField);
            if (CardNum.StartsWith(Security.ro_DecryptFailedPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                CardNum = o.CardNumber;
            }
            String CardExpM = o.CardExpirationMonth;
            String CardExpY = o.CardExpirationYear.Substring(2, 2);
            String ApprvCode = o.AuthorizationCode;
            decimal TotalAmount = o.OrderBalance;

            ASCIIEncoding encoding = new ASCIIEncoding();
            StringBuilder transactionCommand = new StringBuilder(4096);

            transactionCommand.Append("<JetPay><TransactionType>CAPT</TransactionType>\n");
            transactionCommand.Append("<MerchantID>" + AppLogic.AppConfig("JETPAY_MERCHANTID") + "</MerchantID>\n");
            transactionCommand.Append("<TransactionID>" + TransID + "</TransactionID>\n");
            transactionCommand.Append("<CardNum>" + CardNum + "</CardNum>");
            transactionCommand.Append("<CardExpMonth>" + CardExpM + "</CardExpMonth>");
            transactionCommand.Append("<CardExpYear>" + CardExpY + "</CardExpYear>");
            transactionCommand.Append("<Approval>" + ApprvCode + "</Approval>");
            transactionCommand.Append("<TotalAmount>" + Localization.CurrencyStringForGatewayWithoutExchangeRate(TotalAmount).Replace(".", "").Replace(",", "") + "</TotalAmount>");
            transactionCommand.Append("</JetPay>");

            o.CaptureTXCommand = transactionCommand.ToString();

            byte[] data = encoding.GetBytes(transactionCommand.ToString());

            // Prepare web request...
            String AuthServer = CommonLogic.IIF(useLiveTransactions, AppLogic.AppConfig("JETPAY_LIVE_SERVER"), AppLogic.AppConfig("JETPAY_TEST_SERVER"));
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(AuthServer);
            myRequest.Headers.Add("MIME-Version", "1.0");
            myRequest.Headers.Add("Request-number", "1");
            myRequest.Headers.Add("Content-transfer-encoding", "text");
            myRequest.Headers.Add("Document-type", "Request");
            myRequest.ContentType = "text/xml";
            myRequest.ContentLength = data.Length;
            myRequest.Method = "POST";
            Stream newStream = myRequest.GetRequestStream();
            // Send the data.
            newStream.Write(data, 0, data.Length);
            newStream.Close();
            // get the response
            WebResponse myResponse;
            myResponse = myRequest.GetResponse();
            String rawResponseString = String.Empty;
            using (StreamReader sr = new StreamReader(myResponse.GetResponseStream()))
            {
                rawResponseString = sr.ReadToEnd();
                // Close and clean up the StreamReader
                sr.Close();
            }
            myResponse.Close();

            // rawResponseString now has gateway response

            String sql = String.Empty;
            String replyCode = CommonLogic.ExtractToken(rawResponseString, "<ActionCode>", "</ActionCode>");
            String approvalCode = CommonLogic.ExtractToken(rawResponseString, "<Approval>", "</Approval>");
            String authResponse = CommonLogic.ExtractToken(rawResponseString, "<ResponseText>", "</ResponseText>");
            TransID = CommonLogic.ExtractToken(rawResponseString, "<TransactionID>", "</TransactionID>");

            o.CaptureTXResult = rawResponseString;
           
            if (Convert.ToInt32(replyCode) == 0)
            {
                result = AppLogic.ro_OK;
            }
            else
            {
                result = authResponse;
            }
            return result;
        }

        public override String VoidOrder(int OrderNumber)
        {
            String result = AppLogic.ro_OK;

            DB.ExecuteSQL("update orders set VoidTXCommand=NULL, VoidTXResult=NULL where OrderNumber=" + OrderNumber.ToString());
            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");
            String TransID = String.Empty;
            String CardNum = String.Empty;
            String ApprvCode = String.Empty;
            decimal TotalAmount = 0;

            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS("select * from orders   with (NOLOCK)  where OrderNumber=" + OrderNumber.ToString(), conn))
                {
                    if (rs.Read())
                    {
                        TransID = DB.RSField(rs, "AuthorizationPNREF");
                        ApprvCode = DB.RSField(rs, "AuthorizationCode");
                        TotalAmount = DB.RSFieldDecimal(rs, "OrderTotal");
                        CardNum = Security.UnmungeString(DB.RSField(rs, "CardNumber"), rs[AppLogic.AppConfig("OrdersCCSaltField")].ToString());
                        if (CardNum.StartsWith(Security.ro_DecryptFailedPrefix, StringComparison.InvariantCultureIgnoreCase))
                        {
                            CardNum = DB.RSField(rs, "CardNumber");
                        }
                    }
                }
            }
            
            ASCIIEncoding encoding = new ASCIIEncoding();
            StringBuilder transactionCommand = new StringBuilder(4096);

            transactionCommand.Append("<JetPay><TransactionType>VOID</TransactionType>\n");
            transactionCommand.Append("<MerchantID>" + AppLogic.AppConfig("JETPAY_MERCHANTID") + "</MerchantID>\n");
            transactionCommand.Append("<TransactionID>" + TransID + "</TransactionID>\n");
            transactionCommand.Append("<CardNum>" + CardNum + "</CardNum>");
            transactionCommand.Append("<Approval>" + ApprvCode + "</Approval>");
            transactionCommand.Append("<TotalAmount>" + Localization.CurrencyStringForGatewayWithoutExchangeRate(TotalAmount).Replace(".", "").Replace(",", "") + "</TotalAmount>");
            transactionCommand.Append("</JetPay>");

            DB.ExecuteSQL("update orders set VoidTXCommand=" + DB.SQuote(transactionCommand.ToString()) + " where OrderNumber=" + OrderNumber.ToString());

            byte[] data = encoding.GetBytes(transactionCommand.ToString());

            // Prepare web request...
            String AuthServer = CommonLogic.IIF(useLiveTransactions, AppLogic.AppConfig("JETPAY_LIVE_SERVER"), AppLogic.AppConfig("JETPAY_TEST_SERVER"));
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(AuthServer);
            myRequest.Headers.Add("MIME-Version", "1.0");
            myRequest.Headers.Add("Request-number", "1");
            myRequest.Headers.Add("Content-transfer-encoding", "text");
            myRequest.Headers.Add("Document-type", "Request");
            myRequest.ContentType = "text/xml";
            myRequest.ContentLength = data.Length;
            myRequest.Method = "POST";
            Stream newStream = myRequest.GetRequestStream();
            // Send the data.
            newStream.Write(data, 0, data.Length);
            newStream.Close();
            // get the response
            WebResponse myResponse;
            myResponse = myRequest.GetResponse();
            String rawResponseString = String.Empty;
            using (StreamReader sr = new StreamReader(myResponse.GetResponseStream()))
            {
                rawResponseString = sr.ReadToEnd();
                // Close and clean up the StreamReader
                sr.Close();
            }
            myResponse.Close();

            // rawResponseString now has gateway response

            String sql = String.Empty;
            String replyCode = CommonLogic.ExtractToken(rawResponseString, "<ActionCode>", "</ActionCode>");
            String approvalCode = CommonLogic.ExtractToken(rawResponseString, "<Approval>", "</Approval>");
            String authResponse = CommonLogic.ExtractToken(rawResponseString, "<ResponseText>", "</ResponseText>");
            TransID = CommonLogic.ExtractToken(rawResponseString, "<TransactionID>", "</TransactionID>");


            DB.ExecuteSQL("update orders set VoidTXResult=" + DB.SQuote(rawResponseString) + " where OrderNumber=" + OrderNumber.ToString());

            if (Convert.ToInt32(replyCode) == 0)
            {
                result = AppLogic.ro_OK;
            }
            else
            {
                result = authResponse;
            }
            return result;
        }

        // if RefundAmount == 0.0M, then then ENTIRE order amount will be refunded!
        public override String RefundOrder(int OriginalOrderNumber, int NewOrderNumber, decimal RefundAmount, String RefundReason, Address UseBillingAddress)
        {
            String result = AppLogic.ro_OK;

            DB.ExecuteSQL("update orders set RefundTXCommand=NULL, RefundTXResult=NULL where OrderNumber=" + OriginalOrderNumber.ToString());

            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");
            String TransID = String.Empty;
            String CardNum = String.Empty;
            String CardExpM = String.Empty;
            String CardExpY = String.Empty;
            String ApprvCode = String.Empty;
            String BillingLastName = String.Empty;
            String BillingFirstName = String.Empty;
            String BillingAddress1 = String.Empty;
            String BillingCity = String.Empty;
            String BillingState = String.Empty;
            String BillingZip = String.Empty;
            String BillingPhone = String.Empty;
            String BillingEmail = String.Empty;
            decimal TotalAmount = 0;

            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS("select * from orders   with (NOLOCK)  where OrderNumber=" + OriginalOrderNumber.ToString(), conn))
                {
                    if (rs.Read())
                    {
                        TransID = DB.RSField(rs, "AuthorizationPNREF");
                        CardNum = Security.UnmungeString(DB.RSField(rs, "CardNumber"), rs[AppLogic.AppConfig("OrdersCCSaltField")].ToString());
                        if (CardNum.StartsWith(Security.ro_DecryptFailedPrefix, StringComparison.InvariantCultureIgnoreCase))
                        {
                            CardNum = "";
                        }
                        if (String.IsNullOrEmpty(CardNum))
                        {
                            return "JetPay requires that you store credit card numbers to refund orders. To store credit card numbers set the StoreCCInDB app config.You will not be able to refund orders taken before storing card numbers.";
                        }
                        CardExpM = DB.RSField(rs, "CardExpirationMonth");
                        CardExpY = DB.RSField(rs, "CardExpirationYear").Substring(2, 2);
                        ApprvCode = DB.RSField(rs, "AuthorizationCode");
                        TotalAmount = DB.RSFieldDecimal(rs, "OrderTotal");
                        BillingLastName = DB.RSField(rs, "BillingLastName");
                        BillingFirstName = DB.RSField(rs, "BillingFirstName");
                        BillingAddress1 = DB.RSField(rs, "BillingAddress1");
                        BillingCity = DB.RSField(rs, "BillingCity");
                        BillingState = DB.RSField(rs, "BillingState");
                        BillingZip = DB.RSField(rs, "BillingZip");
                        BillingPhone = DB.RSField(rs, "BillingPhone");
                        BillingEmail = DB.RSField(rs, "Email");
                    }                    
                }
            }           

            ASCIIEncoding encoding = new ASCIIEncoding();
            StringBuilder transactionCommand = new StringBuilder(4096);

            transactionCommand.Append("<JetPay><TransactionType>CREDIT</TransactionType>\n");
            transactionCommand.Append("<MerchantID>" + AppLogic.AppConfig("JETPAY_MERCHANTID") + "</MerchantID>\n");
            transactionCommand.Append("<TransactionID>" + NewOrderNumber.ToString().PadLeft(18, '0') + "</TransactionID>\n");
            transactionCommand.Append("<CardNum>" + CardNum + "</CardNum>");
           
            transactionCommand.Append("<CardExpMonth>" + CardExpM + "</CardExpMonth>");
            transactionCommand.Append("<CardExpYear>" + CardExpY + "</CardExpYear>");
            transactionCommand.Append("<CardName>" + BillingFirstName + " " + BillingLastName + "</CardName>");
            transactionCommand.Append("<TotalAmount>" + Localization.CurrencyStringForGatewayWithoutExchangeRate(RefundAmount).Replace(",", "").Replace(".", "") + "</TotalAmount>");
            transactionCommand.Append("<BillingAddress>" + XmlCommon.XmlEncode(BillingAddress1) + "</BillingAddress>");
            transactionCommand.Append("<BillingCity>" + XmlCommon.XmlEncode(BillingCity) + "</BillingCity>");
            transactionCommand.Append("<BillingStateProv>" + BillingState + "</BillingStateProv>");
            transactionCommand.Append("<BillingPostalCode>" + BillingZip + "</BillingPostalCode>");
          
            transactionCommand.Append("<BillingPhone>" + BillingPhone + "</BillingPhone>");
            transactionCommand.Append("<Email>" + BillingEmail + "</Email>");
            transactionCommand.Append("<UDField1>" + XmlCommon.XmlEncode(RefundReason) + "</UDField1>");
            transactionCommand.Append("</JetPay>");

            String CardToken = String.Format("<CardNum>{0}</CardNum>", CardNum);
            String CardTokenReplacement = String.Format("<CardNum>{0}</CardNum>", AppLogic.SafeDisplayCardNumber(CardNum, "Orders",OriginalOrderNumber));

            DB.ExecuteSQL("update orders set RefundTXCommand=" + DB.SQuote(transactionCommand.ToString().Replace(CardToken, CardTokenReplacement)) + " where OrderNumber=" + OriginalOrderNumber.ToString());

            byte[] data = encoding.GetBytes(transactionCommand.ToString());

            // Prepare web request...
            String AuthServer = CommonLogic.IIF(useLiveTransactions, AppLogic.AppConfig("JETPAY_LIVE_SERVER"), AppLogic.AppConfig("JETPAY_TEST_SERVER"));
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(AuthServer);
            myRequest.Headers.Add("MIME-Version", "1.0");
            myRequest.Headers.Add("Request-number", "1");
            myRequest.Headers.Add("Content-transfer-encoding", "text");
            myRequest.Headers.Add("Document-type", "Request");
            myRequest.ContentType = "text/xml";
            myRequest.ContentLength = data.Length;
            myRequest.Method = "POST";
            Stream newStream = myRequest.GetRequestStream();
            // Send the data.
            newStream.Write(data, 0, data.Length);
            newStream.Close();
            // get the response
            WebResponse myResponse;
            myResponse = myRequest.GetResponse();
            String rawResponseString = String.Empty;
            using (StreamReader sr = new StreamReader(myResponse.GetResponseStream()))
            {
                rawResponseString = sr.ReadToEnd();
                // Close and clean up the StreamReader
                sr.Close();
            }
            myResponse.Close();

            // rawResponseString now has gateway response

            String sql = String.Empty;
            String replyCode = CommonLogic.ExtractToken(rawResponseString, "<ActionCode>", "</ActionCode>");
            //String responseCode = CommonLogic.ExtractToken(rawResponseString,"<RespCode>","</RespCode>");
            String approvalCode = CommonLogic.ExtractToken(rawResponseString, "<Approval>", "</Approval>");
            String authResponse = CommonLogic.ExtractToken(rawResponseString, "<ResponseText>", "</ResponseText>");
            String ErrMsg = CommonLogic.ExtractToken(rawResponseString, "<ErrMsg>", "</ErrMsg>");
            TransID = CommonLogic.ExtractToken(rawResponseString, "<TransactionID>", "</TransactionID>");


            DB.ExecuteSQL("update orders set RefundTXResult=" + DB.SQuote(rawResponseString) + " where OrderNumber=" + OriginalOrderNumber.ToString());
            if (Convert.ToInt32(replyCode) == 0)
            {
                result = AppLogic.ro_OK;
            }
            else
            {
                result = authResponse;
            }
            return result;
        }

        public override String ProcessCard(int OrderNumber, int CustomerID, Decimal OrderTotal, bool useLiveTransactions, TransactionModeEnum TransactionMode, Address UseBillingAddress, String CardExtraCode, Address UseShippingAddress, String CAVV, String ECI, String XID, out String AVSResult, out String AuthorizationResult, out String AuthorizationCode, out String AuthorizationTransID, out String TransactionCommandOut, out String TransactionResponse)
        {
            String result = AppLogic.ro_OK;

            ASCIIEncoding encoding = new ASCIIEncoding();
            StringBuilder transactionCommand = new StringBuilder(4096);

            transactionCommand.Append("<JetPay><TransactionType>" + CommonLogic.IIF(TransactionMode == TransactionModeEnum.auth, "AUTHONLY", "SALE") + "</TransactionType>\n");
            transactionCommand.Append("<MerchantID>" + AppLogic.AppConfig("JETPAY_MERCHANTID") + "</MerchantID>\n");
            transactionCommand.Append("<TransactionID>" + OrderNumber.ToString().PadLeft(18, '0') + "</TransactionID>\n");
            transactionCommand.Append("<CardNum>" + UseBillingAddress.CardNumber + "</CardNum>");
            transactionCommand.Append("<CVV2>" + CardExtraCode + "</CVV2>");
            transactionCommand.Append("<CardExpMonth>" + UseBillingAddress.CardExpirationMonth.PadLeft(2, '0') + "</CardExpMonth>");
            transactionCommand.Append("<CardExpYear>" + UseBillingAddress.CardExpirationYear.ToString().Substring(2, 2) + "</CardExpYear>");
            transactionCommand.Append("<CardName>" + UseBillingAddress.FirstName + " " + UseBillingAddress.LastName + "</CardName>");
            transactionCommand.Append("<TotalAmount>" + Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal).Replace(",", "").Replace(".", "") + "</TotalAmount>");
            transactionCommand.Append("<BillingAddress>" + XmlCommon.XmlEncode(UseBillingAddress.Address1) + "</BillingAddress>");
            transactionCommand.Append("<BillingCity>" + XmlCommon.XmlEncode(UseBillingAddress.City) + "</BillingCity>");
            transactionCommand.Append("<BillingStateProv>" + XmlCommon.XmlEncode(UseBillingAddress.State) + "</BillingStateProv>");
            transactionCommand.Append("<BillingPostalCode>" + XmlCommon.XmlEncode(UseBillingAddress.Zip) + "</BillingPostalCode>");
            //transactionCommand.Append("<BillingCountry>" + UseBillingAddress.Country + "</BillingCountry>");
            transactionCommand.Append("<BillingPhone>" + XmlCommon.XmlEncode(UseBillingAddress.Phone) + "</BillingPhone>");
            transactionCommand.Append("<Email>" + XmlCommon.XmlEncode(UseBillingAddress.EMail) + "</Email>");
            transactionCommand.Append("</JetPay>");

            byte[] data = encoding.GetBytes(transactionCommand.ToString());

            // Prepare web request...
            String AuthServer = CommonLogic.IIF(useLiveTransactions, AppLogic.AppConfig("JETPAY_LIVE_SERVER"), AppLogic.AppConfig("JETPAY_TEST_SERVER"));
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(AuthServer);
            myRequest.Headers.Add("MIME-Version", "1.0");
            myRequest.Headers.Add("Request-number", "1");
            myRequest.Headers.Add("Content-transfer-encoding", "text");
            myRequest.Headers.Add("Document-type", "Request");
            myRequest.ContentType = "text/xml";
            myRequest.ContentLength = data.Length;
            myRequest.Method = "POST";
            Stream newStream = myRequest.GetRequestStream();
            // Send the data.
            newStream.Write(data, 0, data.Length);
            newStream.Close();
            // get the response
            WebResponse myResponse;
            myResponse = myRequest.GetResponse();
            String rawResponseString = String.Empty;
            using (StreamReader sr = new StreamReader(myResponse.GetResponseStream()))
            {
                rawResponseString = sr.ReadToEnd();
                // Close and clean up the StreamReader
                sr.Close();
            }
            myResponse.Close();

            // rawResponseString now has gateway response
            TransactionResponse = rawResponseString;

            String sql = String.Empty;
            String replyCode = CommonLogic.ExtractToken(rawResponseString, "<ActionCode>", "</ActionCode>");
            //String responseCode = CommonLogic.ExtractToken(rawResponseString,"<RespCode>","</RespCode>");
            String approvalCode = CommonLogic.ExtractToken(rawResponseString, "<Approval>", "</Approval>");
            String authResponse = CommonLogic.ExtractToken(rawResponseString, "<ResponseText>", "</ResponseText>");
            String ErrMsg = CommonLogic.ExtractToken(rawResponseString, "<ErrMsg>", "</ErrMsg>");
            String TransID = CommonLogic.ExtractToken(rawResponseString, "<TransactionID>", "</TransactionID>");

            int idx = authResponse.IndexOf(">");
            if (idx != -1)
            {
                // pick only text out:
                authResponse = authResponse.Substring(idx + 1, authResponse.Length - idx - 1);
            }

            AuthorizationCode = approvalCode;
            AuthorizationResult = rawResponseString;
            AuthorizationTransID = TransID;
            AVSResult = String.Empty;
            TransactionCommandOut = transactionCommand.ToString();
            TransactionResponse = String.Empty;

            if (Convert.ToInt32(replyCode) == 0)
            {
                result = AppLogic.ro_OK;
            }
            else
            {
                if (ErrMsg.Trim().Length > 0)
                {
                    result = "System Error: " + authResponse + ErrMsg;  //you'll only get one of these messages back
                }
                else
                {
                    result = "DECLINED: " + authResponse;
                }
            }
            return result;
        }

        public override bool RequiresCCForFurtherProcessing()
        {
            return true;
        }
    }
}
