// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Security;
using System.Text;
using System.Web.SessionState;
using System.IO;
using System.Net;
using System.Data;
using System.Xml;
using System.Security.Cryptography.X509Certificates;
using AspDotNetStorefrontCore;
using System.Data.SqlClient;

namespace AspDotNetStorefrontGateways.Processors
{

    public class MyPolicy : ICertificatePolicy
    {
        public bool CheckValidationResult(ServicePoint srvPoint, X509Certificate certificate, WebRequest request, int certificateProblem)
        {
            return true;
        }
    }

    /// <summary>
    /// Summary description for PayFuse.
    /// </summary>
    public class PayFuse : GatewayProcessor
    {
        public PayFuse() { }

        public override String CaptureOrder(Order o)
        {
            String result = AppLogic.ro_OK;

            StringBuilder transactionCommand = new StringBuilder(4096);
            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");
            o.CaptureTXCommand = "";
            o.CaptureTXResult = "";

            String TransID = o.AuthorizationPNREF;
            Decimal OrderTotal = o.OrderBalance;

            transactionCommand.Append("<EngineDocList>");
            transactionCommand.Append("<DocVersion>1.0</DocVersion>");
            transactionCommand.Append("<EngineDoc>");
            transactionCommand.Append("<ContentType>OrderFormDoc</ContentType>");
            transactionCommand.Append("<User>");
            transactionCommand.Append("<Name>" + AppLogic.AppConfig("PayFuse.UserID") + "</Name>");
            transactionCommand.Append("<Password>" + AppLogic.AppConfig("PayFuse.Password") + "</Password>");
            transactionCommand.Append("<Alias>" + AppLogic.AppConfig("PayFuse.Alias") + "</Alias>");
            transactionCommand.Append("</User>");
            transactionCommand.Append("<Instructions>");
            transactionCommand.Append("<Pipeline>Payment</Pipeline>");
            transactionCommand.Append("</Instructions>");
            transactionCommand.Append("<OrderFormDoc>");
            transactionCommand.Append("<Mode>" + CommonLogic.IIF(AppLogic.AppConfigBool("UseLiveTransactions"), "P", "Y") + "</Mode>");
            transactionCommand.Append("<Id>" + TransID.ToString() + "</Id>");
            transactionCommand.Append("<Transaction>");
            transactionCommand.Append("<Type>PostAuth</Type>");
            transactionCommand.Append("<CurrentTotals>");
            transactionCommand.Append("<Totals>");
            transactionCommand.Append("<Total DataType=\"Money\" Currency=\"" + Localization.StoreCurrencyNumericCode() + "\">" + Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal) + "</Total>");
            transactionCommand.Append("</Totals>");
            transactionCommand.Append("</CurrentTotals>");
            transactionCommand.Append("</Transaction>");
            transactionCommand.Append("</OrderFormDoc>");
            transactionCommand.Append("</EngineDoc>");
            transactionCommand.Append("</EngineDocList>");

            o.CaptureTXCommand = transactionCommand.ToString();

            if (TransID.Length == 0 || TransID == "0")
            {
                result = "Invalid or Empty Transaction ID";
            }
            else
            {

                try
                {
                    ASCIIEncoding encoding = new ASCIIEncoding();
                    byte[] data = encoding.GetBytes("XmlPostVar=" + HttpContext.Current.Server.UrlEncode(transactionCommand.ToString()));

                    // Prepare web request...
                    String AuthServer = CommonLogic.IIF(useLiveTransactions, AppLogic.AppConfig("PayFuse.LIVE_SERVER"), AppLogic.AppConfig("PayFuse.TEST_SERVER"));
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

                    XmlDocument responseXml = new XmlDocument();
                    String replyCode = String.Empty;
                    String authResponseMsg = String.Empty;
                    try
                    {
                        //Make sure it's good Xml
                        responseXml.LoadXml(rawResponseString.Trim());
                        //Have good Xml. Lets make it pretty
                        rawResponseString = XmlCommon.FormatXml(responseXml);
                    }
                    catch
                    {
                        authResponseMsg = "GARBLED RESPONSE FROM THE GATEWAY";
                    }

                    if (authResponseMsg.Length == 0)
                    {
                        try
                        {
                            replyCode = responseXml.SelectSingleNode("//CcErrCode").InnerText;
                            authResponseMsg = responseXml.SelectSingleNode("//CcReturnMsg").InnerText + " " + responseXml.SelectSingleNode("//Notice").InnerText;
                        }
                        catch
                        {
                            authResponseMsg = "Could not find CcErrCode In Gateway Response";
                        }
                    }

                    o.CaptureTXResult = rawResponseString;
                   
                    if (replyCode == "1")
                    {
                        result = AppLogic.ro_OK;
                    }
                    else
                    {
                        result = authResponseMsg;
                    }
                }
                catch
                {
                    result = "NO RESPONSE FROM GATEWAY!";
                }
            }
            return result;
        }

        public override String VoidOrder(int OrderNumber)
        {
            String result = AppLogic.ro_OK;

            StringBuilder transactionCommand = new StringBuilder(4096);
            DB.ExecuteSQL("update orders set VoidTXCommand=NULL, VoidTXResult=NULL where OrderNumber=" + OrderNumber.ToString());
            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");
            String TransID = String.Empty;
            Decimal OrderTotal = System.Decimal.Zero;

            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS("select * from orders where OrderNumber=" + OrderNumber.ToString(), conn))
                {
                    if (rs.Read())
                    {
                        TransID = DB.RSField(rs, "AuthorizationPNREF");
                        OrderTotal = DB.RSFieldDecimal(rs, "OrderTotal");
                    }                    
                }
            }
            
            transactionCommand.Append("<EngineDocList>");
            transactionCommand.Append("<DocVersion>1.0</DocVersion>");
            transactionCommand.Append("<EngineDoc>");
            transactionCommand.Append("<ContentType>OrderFormDoc</ContentType>");
            transactionCommand.Append("<User>");
            transactionCommand.Append("<Name>" + AppLogic.AppConfig("PayFuse.UserID") + "</Name>");
            transactionCommand.Append("<Password>" + AppLogic.AppConfig("PayFuse.Password") + "</Password>");
            transactionCommand.Append("<Alias>" + AppLogic.AppConfig("PayFuse.Alias") + "</Alias>");
            transactionCommand.Append("</User>");
            transactionCommand.Append("<Instructions>");
            transactionCommand.Append("<Pipeline>Payment</Pipeline>");
            transactionCommand.Append("</Instructions>");
            transactionCommand.Append("<OrderFormDoc>");
            transactionCommand.Append("<Mode>" + CommonLogic.IIF(AppLogic.AppConfigBool("UseLiveTransactions"), "P", "Y") + "</Mode>");
            transactionCommand.Append("<Id>" + TransID.ToString() + "</Id>");
            transactionCommand.Append("<Transaction>");
            transactionCommand.Append("<Type>Void</Type>");
            transactionCommand.Append("</Transaction>");
            transactionCommand.Append("</OrderFormDoc>");
            transactionCommand.Append("</EngineDoc>");
            transactionCommand.Append("</EngineDocList>");

            DB.ExecuteSQL("update orders set VoidTXCommand=" + DB.SQuote(transactionCommand.ToString()) + " where OrderNumber=" + OrderNumber.ToString());

            if (TransID.Length == 0 || TransID == "0")
            {
                result = "Invalid or Empty Transaction ID";
            }
            else
            {

                try
                {
                    ASCIIEncoding encoding = new ASCIIEncoding();
                    byte[] data = encoding.GetBytes("XmlPostVar=" + HttpContext.Current.Server.UrlEncode(transactionCommand.ToString()));

                    // Prepare web request...
                    String AuthServer = CommonLogic.IIF(useLiveTransactions, AppLogic.AppConfig("PayFuse.LIVE_SERVER"), AppLogic.AppConfig("PayFuse.TEST_SERVER"));
                    HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(AuthServer);
                    myRequest.ContentType = "text/xml;charset=\"utf-8\"";
                    myRequest.Accept = "text/xml";
                    myRequest.Method = "POST";
                    //myRequest.Headers.Add ("MIME-Version", "1.0");
                    //myRequest.Headers.Add ("Request-number", "1");
                    //myRequest.Headers.Add ("Content-transfer-encoding", "text");
                    //myRequest.Headers.Add ("Document-type", "Request");
                    //myRequest.ContentType = "text/Xml";
                    myRequest.ContentLength = data.Length;
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

                    XmlDocument responseXml = new XmlDocument();
                    String replyCode = String.Empty;
                    String authResponseMsg = String.Empty;
                    try
                    {
                        //Make sure it's good Xml
                        responseXml.LoadXml(rawResponseString.Trim());
                        //Have good Xml. Lets make it pretty
                        rawResponseString = XmlCommon.FormatXml(responseXml);
                    }
                    catch
                    {
                        authResponseMsg = "GARBLED RESPONSE FROM THE GATEWAY";
                    }

                    if (authResponseMsg.Length == 0)
                    {
                        try
                        {
                            replyCode = responseXml.SelectSingleNode("//CcErrCode").InnerText;
                            authResponseMsg = responseXml.SelectSingleNode("//CcReturnMsg").InnerText + " " + responseXml.SelectSingleNode("//Notice").InnerText;
                        }
                        catch
                        {
                            authResponseMsg = "Could not find CcErrCode In Gateway Response";
                        }
                    }

                    DB.ExecuteSQL("update orders set VoidTXResult=" + DB.SQuote(rawResponseString) + " where OrderNumber=" + OrderNumber.ToString());
                    if (replyCode == "1")
                    {
                        result = AppLogic.ro_OK;
                    }
                    else
                    {
                        result = authResponseMsg;
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
        public override String RefundOrder(int OriginalOrderNumber, int NewOrderNumber, decimal RefundAmount, String RefundReason, Address UseBillingAddress)
        {
            String result = AppLogic.ro_OK;

            StringBuilder transactionCommand = new StringBuilder(4096);
            DB.ExecuteSQL("update orders set RefundTXCommand=NULL, RefundTXResult=NULL where OrderNumber=" + OriginalOrderNumber.ToString());
            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");
            String TransID = String.Empty;
            Decimal OrderTotal = System.Decimal.Zero;

            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS("select * from orders where OrderNumber=" + OriginalOrderNumber.ToString(), conn))
                {
                    if (rs.Read())
                    {
                        TransID = DB.RSField(rs, "AuthorizationPNREF");
                        OrderTotal = DB.RSFieldDecimal(rs, "OrderTotal");
                    }                    
                }
            }

            transactionCommand.Append("<EngineDocList>");
            transactionCommand.Append("<DocVersion>1.0</DocVersion>");
            transactionCommand.Append("<EngineDoc>");
            transactionCommand.Append("<ContentType>OrderFormDoc</ContentType>");
            transactionCommand.Append("<User>");
            transactionCommand.Append("<Name>" + AppLogic.AppConfig("PayFuse.UserID") + "</Name>");
            transactionCommand.Append("<Password>" + AppLogic.AppConfig("PayFuse.Password") + "</Password>");
            transactionCommand.Append("<Alias>" + AppLogic.AppConfig("PayFuse.Alias") + "</Alias>");
            transactionCommand.Append("</User>");
            transactionCommand.Append("<Instructions>");
            transactionCommand.Append("<Pipeline>Payment</Pipeline>");
            transactionCommand.Append("</Instructions>");
            transactionCommand.Append("<OrderFormDoc>");
            transactionCommand.Append("<Mode>" + CommonLogic.IIF(AppLogic.AppConfigBool("UseLiveTransactions"), "P", "Y") + "</Mode>");
            transactionCommand.Append("<Id>" + TransID.ToString() + "</Id>");
            transactionCommand.Append("<Transaction>");
            transactionCommand.Append("<Type>Credit</Type>");
            transactionCommand.Append("<CurrentTotals>");
            transactionCommand.Append("<Totals>");
            if (RefundAmount == System.Decimal.Zero)
            {
                transactionCommand.Append("<Total DataType=\"Money\" Currency=\"" + Localization.StoreCurrencyNumericCode() + "\">" + Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal) + "</Total>");
            }
            else
            {
                transactionCommand.Append("<Total DataType=\"Money\" Currency=\"" + Localization.StoreCurrencyNumericCode() + "\">" + Localization.CurrencyStringForGatewayWithoutExchangeRate(RefundAmount) + "</Total>");
            }
            transactionCommand.Append("</Totals>");
            transactionCommand.Append("</CurrentTotals>");
            transactionCommand.Append("</Transaction>");
            transactionCommand.Append("</OrderFormDoc>");
            transactionCommand.Append("</EngineDoc>");
            transactionCommand.Append("</EngineDocList>");

            DB.ExecuteSQL("update orders set RefundTXCommand=" + DB.SQuote(transactionCommand.ToString()) + " where OrderNumber=" + OriginalOrderNumber.ToString());

            if (TransID.Length == 0 || TransID == "0")
            {
                result = "Invalid or Empty Transaction ID";
            }
            else
            {

                try
                {
                    ASCIIEncoding encoding = new ASCIIEncoding();
                    byte[] data = encoding.GetBytes("XmlPostVar=" + HttpContext.Current.Server.UrlEncode(transactionCommand.ToString()));

                    // Prepare web request...
                    String AuthServer = CommonLogic.IIF(useLiveTransactions, AppLogic.AppConfig("PayFuse.LIVE_SERVER"), AppLogic.AppConfig("PayFuse.TEST_SERVER"));
                    HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(AuthServer);
                    myRequest.ContentType = "text/xml;charset=\"utf-8\"";
                    myRequest.Accept = "text/xml";
                    myRequest.Method = "POST";
                    //myRequest.Headers.Add ("MIME-Version", "1.0");
                    //myRequest.Headers.Add ("Request-number", "1");
                    //myRequest.Headers.Add ("Content-transfer-encoding", "text");
                    //myRequest.Headers.Add ("Document-type", "Request");
                    //myRequest.ContentType = "text/Xml";
                    myRequest.ContentLength = data.Length;
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

                    XmlDocument responseXml = new XmlDocument();
                    String replyCode = String.Empty;
                    String authResponseMsg = String.Empty;
                    try
                    {
                        //Make sure it's good Xml
                        responseXml.LoadXml(rawResponseString.Trim());
                        //Have good Xml. Lets make it pretty
                        rawResponseString = XmlCommon.FormatXml(responseXml);
                    }
                    catch
                    {
                        authResponseMsg = "GARBLED RESPONSE FROM THE GATEWAY";
                    }

                    if (authResponseMsg.Length == 0)
                    {
                        try
                        {
                            replyCode = responseXml.SelectSingleNode("//CcErrCode").InnerText;
                            authResponseMsg = responseXml.SelectSingleNode("//CcReturnMsg").InnerText + " " + responseXml.SelectSingleNode("//Notice").InnerText;
                        }
                        catch
                        {
                            authResponseMsg = "Could not find CcErrCode In Gateway Response";
                        }
                    }

                    DB.ExecuteSQL("update orders set RefundTXResult=" + DB.SQuote(rawResponseString) + " where OrderNumber=" + OriginalOrderNumber.ToString());
                    if (replyCode == "1")
                    {
                        result = AppLogic.ro_OK;
                    }
                    else
                    {
                        result = authResponseMsg;
                    }
                }
                catch
                {
                    result = "NO RESPONSE FROM GATEWAY!";
                }
            }
            return result;
        }

        public override String ProcessCard(int OrderNumber, int CustomerID, Decimal OrderTotal, bool useLiveTransactions, TransactionModeEnum TransactionMode, Address UseBillingAddress, String CardExtraCode, Address UseShippingAddress, String CAVV, String ECI, String XID, out String AVSResult, out String AuthorizationResult, out String AuthorizationCode, out String AuthorizationTransID, out String TransactionCommandOut, out String TransactionResponse)
        {
            String result = AppLogic.ro_OK;

            ASCIIEncoding encoding = new ASCIIEncoding();
            StringBuilder transactionCommand = new StringBuilder(4096);

            AuthorizationCode = String.Empty;
            AuthorizationResult = String.Empty;
            AuthorizationTransID = String.Empty;
            AVSResult = String.Empty;
            TransactionCommandOut = String.Empty;
            TransactionResponse = String.Empty;

            transactionCommand.Append("<EngineDocList>");
            transactionCommand.Append("<DocVersion>1.0</DocVersion>");
            transactionCommand.Append("<EngineDoc>");
            transactionCommand.Append("<ContentType>OrderFormDoc</ContentType>");
            transactionCommand.Append("<User>");
            transactionCommand.Append("<Name>" + AppLogic.AppConfig("PayFuse.UserID") + "</Name>");
            transactionCommand.Append("<Password>" + AppLogic.AppConfig("PayFuse.Password") + "</Password>");
            transactionCommand.Append("<Alias>" + AppLogic.AppConfig("PayFuse.Alias") + "</Alias>");
            transactionCommand.Append("</User>");
            transactionCommand.Append("<Instructions>");
            transactionCommand.Append("<Pipeline>Payment</Pipeline>");
            transactionCommand.Append("</Instructions>");
            transactionCommand.Append("<OrderFormDoc>");
            transactionCommand.Append("<Mode>" + CommonLogic.IIF(AppLogic.AppConfigBool("UseLiveTransactions"), "P", "Y") + "</Mode>");
            transactionCommand.Append("<Comments>" + XmlCommon.XmlEncode("Order Number: " + OrderNumber.ToString() + ", CustomerID=" + CustomerID.ToString()) + "</Comments>");
            transactionCommand.Append("<Consumer>");
            transactionCommand.Append("<EMail>" + XmlCommon.XmlEncode(UseBillingAddress.EMail) + "</EMail>");
            transactionCommand.Append("<PaymentMech>");
            transactionCommand.Append("<CreditCard>");
            transactionCommand.Append("<Number>" + UseBillingAddress.CardNumber + "</Number>");
            transactionCommand.Append("<Expires DataType=\"ExpirationDate\" Locale=\"" + Localization.StoreCurrencyNumericCode() + "\">" + UseBillingAddress.CardExpirationMonth.PadLeft(2, '0') + "/" + UseBillingAddress.CardExpirationYear.ToString().Substring(2, 2) + "</Expires>");
            if (CardExtraCode.Trim().Length != 0)
            {
                transactionCommand.Append("<Cvv2Val>" + CardExtraCode + "</Cvv2Val>");
                transactionCommand.Append("<Cvv2Indicator>1</Cvv2Indicator>");
            }

            transactionCommand.Append("</CreditCard>");
            transactionCommand.Append("</PaymentMech>");
            transactionCommand.Append("<BillTo>");
            transactionCommand.Append("<Location>");
            transactionCommand.Append("<TelVoice>" + XmlCommon.XmlEncode(UseBillingAddress.Phone) + "</TelVoice>");
            transactionCommand.Append("<TelFax/>");
            transactionCommand.Append("<Address>");
            transactionCommand.Append("<Name>" + XmlCommon.XmlEncode((UseBillingAddress.FirstName + " " + UseBillingAddress.LastName)) + "</Name>");
            transactionCommand.Append("<Street1>" + XmlCommon.XmlEncode(UseBillingAddress.Address1) + "</Street1>");
            transactionCommand.Append("<Street2>" + XmlCommon.XmlEncode(UseBillingAddress.Address2) + "</Street2>");
            transactionCommand.Append("<City>" + XmlCommon.XmlEncode(UseBillingAddress.City) + "</City>");
            transactionCommand.Append("<StateProv>" + XmlCommon.XmlEncode(UseBillingAddress.State) + "</StateProv>");
            transactionCommand.Append("<PostalCode>" + XmlCommon.XmlEncode(UseBillingAddress.Zip) + "</PostalCode>");
            transactionCommand.Append("<Country>" + XmlCommon.XmlEncode(Localization.StoreCurrencyNumericCode()) + "</Country>");
            transactionCommand.Append("<Company>" + XmlCommon.XmlEncode(UseBillingAddress.Company) + "</Company>");
            transactionCommand.Append("</Address>");
            transactionCommand.Append("</Location>");
            transactionCommand.Append("</BillTo>");

            if (UseShippingAddress != null)
            {
                transactionCommand.Append("<ShipTo>");
                transactionCommand.Append("<Location>");
                transactionCommand.Append("<TelVoice>" + XmlCommon.XmlEncode(UseShippingAddress.Phone) + "</TelVoice>");
                transactionCommand.Append("<TelFax/>");
                transactionCommand.Append("<Address>");
                transactionCommand.Append("<Name>" + XmlCommon.XmlEncode((UseShippingAddress.FirstName + " " + UseShippingAddress.LastName)) + "</Name>");
                transactionCommand.Append("<Street1>" + XmlCommon.XmlEncode(UseShippingAddress.Address1) + "</Street1>");
                transactionCommand.Append("<Street2>" + XmlCommon.XmlEncode(UseShippingAddress.Address2) + "</Street2>");
                transactionCommand.Append("<City>" + XmlCommon.XmlEncode(UseShippingAddress.City) + "</City>");
                transactionCommand.Append("<StateProv>" + XmlCommon.XmlEncode(UseShippingAddress.State) + "</StateProv>");
                transactionCommand.Append("<PostalCode>" + XmlCommon.XmlEncode(UseShippingAddress.Zip) + "</PostalCode>");
                transactionCommand.Append("<Country>" + XmlCommon.XmlEncode(Localization.StoreCurrencyNumericCode()) + "</Country>");
                transactionCommand.Append("<Company>" + XmlCommon.XmlEncode(UseShippingAddress.Company) + "</Company>");
                transactionCommand.Append("</Address>");
                transactionCommand.Append("</Location>");
                transactionCommand.Append("</ShipTo>");
            }

            transactionCommand.Append("</Consumer>");
            transactionCommand.Append("<Transaction>");
            transactionCommand.Append("<Type>" + CommonLogic.IIF(TransactionMode == TransactionModeEnum.auth, "PreAuth", "Auth") + "</Type>");
            transactionCommand.Append("<CurrentTotals>");
            transactionCommand.Append("<Totals>");
            transactionCommand.Append("<Total DataType=\"Money\" Currency=\"" + Localization.StoreCurrencyNumericCode() + "\">" + Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal).Replace(".", "").Replace("$", "").Replace(",", "") + "</Total>");
            transactionCommand.Append("</Totals>");
            transactionCommand.Append("</CurrentTotals>");
            transactionCommand.Append("</Transaction>");
            transactionCommand.Append("</OrderFormDoc>");
            transactionCommand.Append("</EngineDoc>");
            transactionCommand.Append("</EngineDocList>");

            // Is the command good Xml
            XmlDocument cmdXml = new XmlDocument();
            try
            {
                cmdXml.LoadXml(transactionCommand.ToString());
            }
            catch
            {
                return "Transaction command Xml is not valid.";
            }
            //Have good Xml. Lets make it pretty
            transactionCommand.Length = 0; //Clear the builder
            transactionCommand.Append(XmlCommon.FormatXml(cmdXml));
            cmdXml = null;


            byte[] data = encoding.GetBytes("XmlPostVar=" + HttpContext.Current.Server.UrlEncode(transactionCommand.ToString()));

            // Prepare web request...
            //System.Net.ServicePointManager.CertificatePolicy = new MyPolicy();
            String AuthServer = CommonLogic.IIF(useLiveTransactions, AppLogic.AppConfig("PayFuse.LIVE_SERVER"), AppLogic.AppConfig("PayFuse.TEST_SERVER"));
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(AuthServer);
            if (!useLiveTransactions)
            {
                // must provide a port:
            }
            myRequest.ContentType = "text/xml;charset=\"utf-8\"";
            myRequest.Method = "POST";
            myRequest.Headers.Add("Content-transfer-encoding", "text");
            myRequest.Headers.Add("Document-type", "Request");
            myRequest.ContentType = "text/Xml";
            myRequest.ContentLength = data.Length;
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

            XmlDocument responseXml = new XmlDocument();
            String replyCode = String.Empty;
            String authResponseMsg = String.Empty;
            try
            {
                //Make sure it's good Xml
                responseXml.LoadXml(rawResponseString.Trim());
                //Have good Xml. Lets make it pretty
                rawResponseString = XmlCommon.FormatXml(responseXml);
            }
            catch
            {
                authResponseMsg = "GARBLED RESPONSE FROM THE GATEWAY";
            }

            AuthorizationResult = rawResponseString;

            if (authResponseMsg.Length == 0)
            {
                try
                {
                    replyCode = responseXml.SelectSingleNode("//CcErrCode").InnerText;
                    authResponseMsg = responseXml.SelectSingleNode("//CcReturnMsg").InnerText + " " + responseXml.SelectSingleNode("//Notice").InnerText;
                }
                catch
                {
                    authResponseMsg = "Could not find CcErrCode In Gateway Response";
                }
            }

            try
            {
                AuthorizationCode = responseXml.SelectSingleNode("//AuthCode").InnerText;
            }
            catch { }

            try
            {
                AuthorizationTransID = responseXml.SelectSingleNode("//TransactionId").InnerText;
            }
            catch { }

            AVSResult = String.Empty;
            TransactionCommandOut = transactionCommand.ToString();

            if (replyCode == "1")
            {
                result = AppLogic.ro_OK;
            }
            else
            {
                result = authResponseMsg;
              
            }
            return result;
        }

    }
}
