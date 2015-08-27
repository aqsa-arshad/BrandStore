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
using System.Globalization;
using AspDotNetStorefrontCore;
using System.Data.SqlClient;

namespace AspDotNetStorefrontGateways.Processors
{
    /// <summary>
    /// Summary description for HSBC.
    /// </summary>
    public class HSBC : GatewayProcessor
    {
        public HSBC() { }

        public override string AdministratorSetupPrompt
        {
            get
            {
                return "<a href='http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=hsbc&type=learnmore' target=\'blank'>Learn More...</a>";
            }
        }

        public override String CaptureOrder(Order o)
        {
            String result = AppLogic.ro_OK;

            StringBuilder transactionCommand = new StringBuilder(4096);
            o.CaptureTXCommand = "";
            o.CaptureTXResult = "";

            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");

            String TransID = o.AuthorizationPNREF;
            Decimal OrderTotal = o.OrderBalance;

            if (TransID.Length == 0)
            {
                result = String.Format("Invalid or Empty Transaction ID: {0}", TransID);
            }
            else
            {

                transactionCommand.Append("<?xml version=\"1.0\"  encoding=\"UTF-8\"?>");
                transactionCommand.Append("<EngineDocList>");
                transactionCommand.Append("<DocVersion>" + AppLogic.AppConfig("HSBC.DocVersion") + "</DocVersion>");
                transactionCommand.Append("<EngineDoc>");
                transactionCommand.Append("<ContentType>OrderFormDoc</ContentType>");
                transactionCommand.Append("<User>");
                if (AppLogic.AppConfig("HSBC.ClientID").Length != 0)
                {
                    transactionCommand.Append("<ClientId DataType=\"S32\">" + AppLogic.AppConfig("HSBC.ClientID") + "</ClientId>");
                }
                else
                {
                    transactionCommand.Append("<Alias>" + AppLogic.AppConfig("HSBC.ClientAlias") + "</Alias>");
                }
                transactionCommand.Append("<Name>" + AppLogic.AppConfig("HSBC.ClientName") + "</Name>");
                transactionCommand.Append("<Password>" + AppLogic.AppConfig("HSBC.ClientPassword") + "</Password>");
                transactionCommand.Append("</User>");
                transactionCommand.Append("<Instructions>");
                transactionCommand.Append("<Pipeline>" + AppLogic.AppConfig("HSBC.Pipeline") + "</Pipeline>");
                transactionCommand.Append("</Instructions>");
                transactionCommand.Append("<OrderFormDoc>");
                transactionCommand.Append("<Mode>" + CommonLogic.IIF(AppLogic.AppConfigBool("UseLiveTransactions"), AppLogic.AppConfig("HSBC.Mode.Live"), AppLogic.AppConfig("HSBC.Mode.Test")) + "</Mode>");
                transactionCommand.Append("<Id>" + TransID + "</Id>");
                transactionCommand.Append("<Transaction>");
                transactionCommand.Append("<Type>PostAuth</Type>");
                if (OrderTotal != System.Decimal.Zero)
                {
                    // amount could have changed by admin user, so capture the current Order Total from the db:
                    transactionCommand.Append("<CurrentTotals>");
                    transactionCommand.Append("<Totals>");
                    transactionCommand.Append("<Total DataType=\"Money\" Currency=\"" + Localization.StoreCurrencyNumericCode() + "\">" + Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal).Replace(".", "") + "</Total>");
                    transactionCommand.Append("</Totals>");
                    transactionCommand.Append("</CurrentTotals>");
                }
                transactionCommand.Append("</Transaction>");
                transactionCommand.Append("</OrderFormDoc>");
                transactionCommand.Append("</EngineDoc>");
                transactionCommand.Append("</EngineDocList>");

                o.CaptureTXCommand = transactionCommand.ToString();

                try
                {
                    ASCIIEncoding encoding = new ASCIIEncoding();
                    byte[] data = encoding.GetBytes(transactionCommand.ToString());

                    // Prepare web request...
                    String AuthServer = CommonLogic.IIF(useLiveTransactions, AppLogic.AppConfig("HSBC.Live.Server"), AppLogic.AppConfig("HSBC.Test.Server"));
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
                        sr.Close();
                    }
                    myResponse.Close();

                    // rawResponseString now has gateway response
                    o.CaptureTXResult = rawResponseString;
                    XmlDocument responseXml = new XmlDocument();
                    responseXml.LoadXml(rawResponseString);

                    XmlNode n = responseXml.SelectSingleNode("//Overview");
                    if (n != null)
                    {
                        String respStatus = XmlCommon.XmlField(n, "TransactionStatus");
                        String respErrCode = XmlCommon.XmlField(n, "CcErrCode");
                        String respMsg = XmlCommon.XmlField(n, "CcReturnMsg");
                        String respNotice = XmlCommon.XmlField(n, "Notice");

                        if (respStatus.Equals("C", StringComparison.InvariantCultureIgnoreCase) || 
                            respStatus.Equals("A", StringComparison.InvariantCultureIgnoreCase))
                        {
                            result = AppLogic.ro_OK;
                        }
                        else
                        {
                            result = respMsg;
                            if (respErrCode.Length != 0)
                            {
                                result += " [" + respErrCode + "]";
                            }
                            result += " " + respNotice;
                            result = result.Trim();
                        }
                    }
                    else
                    {
                        result = "Unrecognized response from HSBC gateway.";
                    }
                }
                catch
                {
                    result = "NO RESPONSE FROM HSBC GATEWAY!";
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
            String CaptureTXResult = String.Empty;

            Decimal OrderTotal = System.Decimal.Zero;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select * from orders where OrderNumber=" + OrderNumber.ToString(), con))
                {
                    if (rs.Read())
                    {
                        TransID = DB.RSField(rs, "AuthorizationPNREF");
                        OrderTotal = DB.RSFieldDecimal(rs, "OrderTotal");
                    }
                }
            }

            if (TransID.Length == 0)
            {
                result = String.Format("Invalid or Empty Transaction ID: {0}", TransID);
            }
            else
            {

                transactionCommand.Append("<?xml version=\"1.0\"  encoding=\"UTF-8\"?>");
                transactionCommand.Append("<EngineDocList>");
                transactionCommand.Append("<DocVersion>" + AppLogic.AppConfig("HSBC.DocVersion") + "</DocVersion>");
                transactionCommand.Append("<EngineDoc>");
                transactionCommand.Append("<ContentType>OrderFormDoc</ContentType>");
                transactionCommand.Append("<User>");
                if (AppLogic.AppConfig("HSBC.ClientID").Length != 0)
                {
                    transactionCommand.Append("<ClientId DataType=\"S32\">" + AppLogic.AppConfig("HSBC.ClientID") + "</ClientId>");
                }
                else
                {
                    transactionCommand.Append("<Alias>" + AppLogic.AppConfig("HSBC.ClientAlias") + "</Alias>");
                }
                transactionCommand.Append("<Name>" + AppLogic.AppConfig("HSBC.ClientName") + "</Name>");
                transactionCommand.Append("<Password>" + AppLogic.AppConfig("HSBC.ClientPassword") + "</Password>");
                transactionCommand.Append("</User>");
                transactionCommand.Append("<Instructions>");
                transactionCommand.Append("<Pipeline>" + AppLogic.AppConfig("HSBC.Pipeline") + "</Pipeline>");
                transactionCommand.Append("</Instructions>");
                transactionCommand.Append("<OrderFormDoc>");
                transactionCommand.Append("<Mode>" + CommonLogic.IIF(AppLogic.AppConfigBool("UseLiveTransactions"), AppLogic.AppConfig("HSBC.Mode.Live"), AppLogic.AppConfig("HSBC.Mode.Test")) + "</Mode>");
                transactionCommand.Append("<Id>" + TransID + "</Id>");
                transactionCommand.Append("<Transaction>");
                transactionCommand.Append("<Type>Void</Type>");
                transactionCommand.Append("</Transaction>");
                transactionCommand.Append("</OrderFormDoc>");
                transactionCommand.Append("</EngineDoc>");
                transactionCommand.Append("</EngineDocList>");

                DB.ExecuteSQL("update orders set VoidTXCommand=" + DB.SQuote(transactionCommand.ToString()) + " where OrderNumber=" + OrderNumber.ToString());

                try
                {
                    ASCIIEncoding encoding = new ASCIIEncoding();
                    byte[] data = encoding.GetBytes(transactionCommand.ToString());

                    // Prepare web request...
                    String AuthServer = CommonLogic.IIF(useLiveTransactions, AppLogic.AppConfig("HSBC.Live.Server"), AppLogic.AppConfig("HSBC.Test.Server"));
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
                        sr.Close();
                    }
                    myResponse.Close();

                    // rawResponseString now has gateway response
                    DB.ExecuteSQL("update orders set VoidTXResult=" + DB.SQuote(rawResponseString) + " where OrderNumber=" + OrderNumber.ToString());
                    XmlDocument responseXml = new XmlDocument();
                    responseXml.LoadXml(rawResponseString);

                    XmlNode n = responseXml.SelectSingleNode("//Overview");
                    if (n != null)
                    {
                        String respStatus = XmlCommon.XmlField(n, "TransactionStatus");
                        String respErrCode = XmlCommon.XmlField(n, "CcErrCode");
                        String respMsg = XmlCommon.XmlField(n, "CcReturnMsg");
                        String respNotice = XmlCommon.XmlField(n, "Notice");

                        if (respStatus.Equals("V", StringComparison.InvariantCultureIgnoreCase) || 
                            respStatus.Equals("A", StringComparison.InvariantCultureIgnoreCase))
                        {
                            result = AppLogic.ro_OK;
                        }
                        else
                        {
                            result = respMsg;
                            if (respErrCode.Length != 0)
                            {
                                result += " [" + respErrCode + "]";
                            }
                            result += " " + respNotice;
                            result = result.Trim();
                        }
                    }
                    else
                    {
                        result = "Unrecognized response from HSBC gateway.";
                    }
                }
                catch
                {
                    result = "NO RESPONSE FROM HSBC GATEWAY!";
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

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select * from orders where OrderNumber=" + OriginalOrderNumber.ToString(), con))
                {
                    if (rs.Read())
                    {
                        TransID = DB.RSField(rs, "AuthorizationPNREF");
                        OrderTotal = DB.RSFieldDecimal(rs, "OrderTotal");
                    }
                }
            }

            transactionCommand.Append("<?xml version=\"1.0\"  encoding=\"UTF-8\"?>");
            transactionCommand.Append("<EngineDocList>");
            transactionCommand.Append("<DocVersion>" + AppLogic.AppConfig("HSBC.DocVersion") + "</DocVersion>");
            transactionCommand.Append("<EngineDoc>");
            transactionCommand.Append("<ContentType>OrderFormDoc</ContentType>");
            transactionCommand.Append("<User>");
            if (AppLogic.AppConfig("HSBC.ClientID").Length != 0)
            {
                transactionCommand.Append("<ClientId DataType=\"S32\">" + AppLogic.AppConfig("HSBC.ClientID") + "</ClientId>");
            }
            else
            {
                transactionCommand.Append("<Alias>" + AppLogic.AppConfig("HSBC.ClientAlias") + "</Alias>");
            }
            transactionCommand.Append("<Name>" + AppLogic.AppConfig("HSBC.ClientName") + "</Name>");
            transactionCommand.Append("<Password>" + AppLogic.AppConfig("HSBC.ClientPassword") + "</Password>");
            transactionCommand.Append("</User>");
            transactionCommand.Append("<Instructions>");
            transactionCommand.Append("<Pipeline>" + AppLogic.AppConfig("HSBC.Pipeline") + "</Pipeline>");
            transactionCommand.Append("</Instructions>");
            transactionCommand.Append("<OrderFormDoc>");
            transactionCommand.Append("<Mode>" + CommonLogic.IIF(AppLogic.AppConfigBool("UseLiveTransactions"), AppLogic.AppConfig("HSBC.Mode.Live"), AppLogic.AppConfig("HSBC.Mode.Test")) + "</Mode>");
            transactionCommand.Append("<Id>" + TransID + "</Id>");
            transactionCommand.Append("<Transaction>");
            transactionCommand.Append("<Type>Credit</Type>");
            if (RefundAmount != System.Decimal.Zero)
            {
                transactionCommand.Append("<CurrentTotals>");
                transactionCommand.Append("<Totals>");
                transactionCommand.Append("<Total DataType=\"Money\" Currency=\"" + Localization.StoreCurrencyNumericCode() + "\">" + Localization.CurrencyStringForGatewayWithoutExchangeRate(RefundAmount).Replace(".", "") + "</Total>");
                transactionCommand.Append("</Totals>");
                transactionCommand.Append("</CurrentTotals>");
            }
            transactionCommand.Append("</Transaction>");
            transactionCommand.Append("</OrderFormDoc>");
            transactionCommand.Append("</EngineDoc>");
            transactionCommand.Append("</EngineDocList>");

            DB.ExecuteSQL("update orders set RefundTXCommand=" + DB.SQuote(transactionCommand.ToString()) + " where OrderNumber=" + OriginalOrderNumber.ToString());

            if (TransID.Length == 0)
            {
                result = String.Format("Invalid or Empty Transaction ID: {0}", TransID);
            }
            else
            {
                try
                {
                    ASCIIEncoding encoding = new ASCIIEncoding();
                    byte[] data = encoding.GetBytes(transactionCommand.ToString());

                    // Prepare web request...
                    String AuthServer = CommonLogic.IIF(useLiveTransactions, AppLogic.AppConfig("HSBC.Live.Server"), AppLogic.AppConfig("HSBC.Test.Server"));
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
                        sr.Close();
                    }
                    myResponse.Close();

                    // rawResponseString now has gateway response

                    String ReplyCode = String.Empty;
                    String authResponse = String.Empty;

                    XmlDocument responseXml = new XmlDocument();

                    try
                    {
                        //Make sure it's good Xml
                        responseXml.LoadXml(rawResponseString.Trim());

                        //Have good Xml. Lets make it pretty
                        rawResponseString = XmlCommon.FormatXml(responseXml);

                        XmlNode n = responseXml.SelectSingleNode("//Overview");
                        if (n != null)
                        {
                            String respStatus = XmlCommon.XmlField(n, "TransactionStatus");
                            String respErrCode = XmlCommon.XmlField(n, "CcErrCode");
                            String respMsg = XmlCommon.XmlField(n, "CcReturnMsg");
                            String respNotice = XmlCommon.XmlField(n, "Notice");

                            if (respStatus.Equals("A", StringComparison.InvariantCultureIgnoreCase))
                            {
                                result = AppLogic.ro_OK;
                            }
                            else
                            {
                                result = respMsg;
                                if (respErrCode.Length != 0)
                                {
                                    result += " [" + respErrCode + "]";
                                }
                                result += " " + respNotice;
                                result = result.Trim();
                            }
                        }
                        else
                        {
                            result = "Unrecognized response from HSBC gateway.";
                        }
                    }
                    catch
                    {
                        throw new ArgumentException("HSBC Unexpected Response: " + rawResponseString); // HSBC returns HTML sometimes, or other crap, so we just have to dump it out here
                    }

                    DB.ExecuteSQL("update orders set RefundTXResult=" + DB.SQuote(rawResponseString) + " where OrderNumber=" + OriginalOrderNumber.ToString());
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

            AVSResult = String.Empty;
            AuthorizationCode = String.Empty;
            AuthorizationResult = String.Empty;
            AuthorizationTransID = String.Empty;
            TransactionCommandOut = String.Empty;
            TransactionResponse = String.Empty;

            ASCIIEncoding encoding = new ASCIIEncoding();
            StringBuilder transactionCommand = new StringBuilder(4096);

            String BillingISOCode = String.Empty;
            String ShippingISOCode = String.Empty;

            try
            {
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS("select NumericISOCode from Country where CountryID=" + UseBillingAddress.CountryID, con))
                    {
                        if (rs.Read())
                        {
                            BillingISOCode = rs["NumericISOCode"].ToString();
                        }
                        else
                        {
                            BillingISOCode = "0";
                        }
                    }
                }

            }
            catch { BillingISOCode = "0"; }

            try
            {
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS("select NumericISOCode from Country where CountryID=" + UseShippingAddress.CountryID, con))
                    {
                        if (rs.Read())
                        {
                            ShippingISOCode = rs["NumericISOCode"].ToString();
                        }
                        else
                        {
                            ShippingISOCode = "0";
                        }
                    }
                }

            }
            catch { ShippingISOCode = "0"; }

            CustomerSession cSession = new CustomerSession(CustomerID);
            int CardTypeID = DB.GetSqlN("select CardTypeID N from CreditCardType where CardType = " + DB.SQuote(UseBillingAddress.CardType));
            bool Try3DSecure = CommonLogic.IntegerIsInIntegerList(CardTypeID, AppLogic.AppConfig("3DSECURE.CreditCardTypeIDs"));

            if (Try3DSecure && cSession["3Dsecure.HSBCPASResult"].Length == 0)
            {
                // This is round one, we need to have the customer submit the form to The HSBC PAS (Payer Authentication Service).
                cSession["3Dsecure.HSBCAmountRaw"] = Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal).Replace(".", "");
                cSession["3Dsecure.HSBCAmount"] = Localization.CurrencyStringForDisplayWithoutExchangeRate(OrderTotal);
                cSession["3DSecure.CustomerID"] = CustomerID.ToString();
                cSession["3DSecure.OrderNumber"] = OrderNumber.ToString();
                cSession["3DSecure.MD"] = OrderNumber.ToString();
                return AppLogic.ro_3DSecure;
            }

            String TransactionAuthentication = String.Empty;
            if (Try3DSecure && cSession["3Dsecure.HSBCPASResult"].Length != 0)
            {
                // This is round two, we need to submit the authorization request using the PAS details.

                String CcpaResultsCode = cSession["3Dsecure.HSBCPASResult"];
                cSession["3Dsecure.HSBCPASResult"] = String.Empty;
                switch (CcpaResultsCode)
                {
                    case "0":
                        /*
                        Payer authentication was successful. The XID,       Set PayerSecurityLevel to 2.
                        CAVV, and ECI indicator should be extracted         Set PayerAuthenticationCode to the
                        and sent with the authorisation request. A          returned CAVV
                        result code of 0 indicates the transaction is       Set PayerTxnId to the returned XID.
                        eligible for Chargeback protection.                 Set CardholderPresentCode to 13.
                         */
                        TransactionAuthentication = "<PayerSecurityLevel>2</PayerSecurityLevel>";
                        TransactionAuthentication += "<PayerAuthenticationCode>" + CAVV +"</PayerAuthenticationCode>";
                        TransactionAuthentication += "<PayerTxnId>" + XID + "</PayerTxnId>";
                        TransactionAuthentication += "<CardholderPresentCode>13</CardholderPresentCode>";
                        break;
                    case "1":
                        /*
                        The cardholder’s card was not within a participating    Set PayerSecurityLevel to 5.
                        BIN range. Payer authentication was not performed.      Set CardholderPresentCode to 13.
                         */
                        TransactionAuthentication = "<PayerSecurityLevel>5</PayerSecurityLevel>";
                        TransactionAuthentication += "<CardholderPresentCode>13</CardholderPresentCode>";
                        break;
                    case "2":
                        /*
                        The cardholder was in a participating BIN range, but    Set PayerSecurityLevel to 1.
                        was not enrolled in 3-D Secure. Payer authentication    Set CardholderPresentCode to 13.
                        was not performed.
                        */
                        TransactionAuthentication = "<PayerSecurityLevel>1</PayerSecurityLevel>";
                        TransactionAuthentication += "<CardholderPresentCode>13</CardholderPresentCode>";
                        break;
                    case "3":
                        /*
                        The cardholder was not enrolled in 3-D Secure.          Set PayerSecurityLevel to 6.
                        However, the cardholder was authenticated using the     Set PayerAuthenticationCode to the
                        3-D Secure attempt server. The XID, CAVV, and ECI       returned CAVV
                        indicator should be extracted and sent with the         Set PayerTxnId to the returned XID.
                        authorisation request. A result code of 3 indicates the Set CardholderPresentCode to 13.
                        transaction is eligible for Chargeback protection.
                        */
                        TransactionAuthentication = "<PayerSecurityLevel>6</PayerSecurityLevel>";
                        TransactionAuthentication += "<PayerAuthenticationCode>" + CAVV + "</PayerAuthenticationCode>";
                        TransactionAuthentication += "<PayerTxnId>" + XID + "</PayerTxnId>";
                        TransactionAuthentication += "<CardholderPresentCode>13</CardholderPresentCode>";
                        break;
                    case "4":
                        /*
                        The cardholder was enrolled in 3-D Secure. A PARes      Try 3-D Secure again or set
                        has not yet been received for this transaction.         PayerSecurityLevel to 4.
                        */
                        TransactionAuthentication = "<PayerSecurityLevel>4</PayerSecurityLevel>";
                        break;
                    case "7":
                        /*
                        The ACS was unable to provide authentication            Set PayerSecurityLevel to 4.
                        results. The transaction should proceed as a standard
                        e-commerce transaction.
                        */
                        TransactionAuthentication = "<PayerSecurityLevel>4</PayerSecurityLevel>";
                        break;
                    case "8":
                        /*
                        The CCPA failed to communicate with the                 Try 3-D Secure again or set
                        Directory Server.                                       PayerSecurityLevel to 4.
                        */
                        TransactionAuthentication = "<PayerSecurityLevel>4</PayerSecurityLevel>";
                        break;
                    case "9":
                        /*
                        The CCPA was unable to interpret the results from       Try 3-D Secure again or set
                        payer authentication or enrolment verification.         PayerSecurityLevel to 4.
                        */
                        TransactionAuthentication = "<PayerSecurityLevel>4</PayerSecurityLevel>";
                        break;
                    case "10":
                        /*
                        The CCPA failed to locate or access configuration       Try 3-D Secure again or set
                        information for this merchant.                          PayerSecurityLevel to 4.
                        */
                        TransactionAuthentication = "<PayerSecurityLevel>4</PayerSecurityLevel>";
                        break;
                    case "11":
                        /*
                        Data submitted or configured in the CCPA has failed     Try 3-D Secure again, check input
                        validation checks.                                      fields, or set PayerSecurityLevel to 4.
                        */
                        TransactionAuthentication = "<PayerSecurityLevel>4</PayerSecurityLevel>";
                        break;
                    case "12":
                        /*
                        Unexpected system error from CCPA.                      Try 3-D Secure again or set
                                                                                PayerSecurityLevel to 4.
                        */
                        TransactionAuthentication = "<PayerSecurityLevel>4</PayerSecurityLevel>";
                        break;
                    case "14":
                        /*
                        Indicates that card submitted is not recognised, or the     Try 3-D Secure again or proceed as
                        PAS does not support the card type.                         standard e-commerce transaction
                                                                                    (i.e. set CardholderPresentCode to 7).
                        */
                        break;
                }

            }
            transactionCommand.Append("<?xml version=\"1.0\"  encoding=\"UTF-8\"?>");
            transactionCommand.Append("<EngineDocList>");
            transactionCommand.Append("<DocVersion>" + AppLogic.AppConfig("HSBC.DocVersion") + "</DocVersion>");
            transactionCommand.Append("<EngineDoc>");
            transactionCommand.Append("<ContentType>OrderFormDoc</ContentType>");
            transactionCommand.Append("<User>");
            if (AppLogic.AppConfig("HSBC.ClientID").Length != 0)
            {
                transactionCommand.Append("<ClientId DataType=\"S32\">" + AppLogic.AppConfig("HSBC.ClientID") + "</ClientId>");
            }
            else
            {
                transactionCommand.Append("<Alias>" + AppLogic.AppConfig("HSBC.ClientAlias") + "</Alias>");
            }
            transactionCommand.Append("<Name>" + AppLogic.AppConfig("HSBC.ClientName") + "</Name>");
            transactionCommand.Append("<Password>" + AppLogic.AppConfig("HSBC.ClientPassword") + "</Password>");
            transactionCommand.Append("</User>");
            transactionCommand.Append("<Instructions>");
            transactionCommand.Append("<Pipeline>" + AppLogic.AppConfig("HSBC.Pipeline") + "</Pipeline>");
            transactionCommand.Append("</Instructions>");
            transactionCommand.Append("<OrderFormDoc>");
            transactionCommand.Append("<Mode>" + CommonLogic.IIF(AppLogic.AppConfigBool("UseLiveTransactions"), AppLogic.AppConfig("HSBC.Mode.Live"), AppLogic.AppConfig("HSBC.Mode.Test")) + "</Mode>");
            transactionCommand.Append("<Consumer>");
            transactionCommand.Append("<PaymentMech>");
            transactionCommand.Append("<Type>CreditCard</Type>");
            transactionCommand.Append("<CreditCard>");
            transactionCommand.Append("<Number>" + UseBillingAddress.CardNumber + "</Number>");
			transactionCommand.AppendFormat("<Expires DataType=\"ExpirationDate\" Locale=\"{0}\">{1}/{2}</Expires>", Localization.StoreCurrencyNumericCode(), UseBillingAddress.CardExpirationMonth.PadLeft(2, '0'), UseBillingAddress.CardExpirationYear.Substring(UseBillingAddress.CardExpirationYear.Length - 2).PadLeft(2, '0'));
            if (CardExtraCode.Length != 0)
            {
                transactionCommand.Append("<Cvv2Indicator>1</Cvv2Indicator>");
                transactionCommand.Append("<Cvv2Val>" + CardExtraCode.Trim() + "</Cvv2Val>");
            }
            if (UseBillingAddress.CardIssueNumber.Length != 0)
            {
                transactionCommand.Append("<IssueNum>" + UseBillingAddress.CardIssueNumber + "</IssueNum>");
            }
            if (UseBillingAddress.CardStartDate.Length != 0)
            {
				transactionCommand.AppendFormat("<StartDate DataType=\"StartDate\">{0}/{1}</StartDate>", UseBillingAddress.CardStartDate.Substring(0, 2), UseBillingAddress.CardStartDate.Substring(UseBillingAddress.CardStartDate.Length - 2, 2));
            }
            transactionCommand.Append("<ExchangeType>1</ExchangeType>");

            transactionCommand.Append("</CreditCard>");
            transactionCommand.Append("</PaymentMech>");
            transactionCommand.Append("<Email>" + XmlCommon.XmlEncode(UseBillingAddress.EMail) + "</Email>");

            transactionCommand.Append("<BillTo>");
            transactionCommand.Append("<Location>");
            transactionCommand.Append("<Email>" + XmlCommon.XmlEncode(UseBillingAddress.EMail) + "</Email>");
            transactionCommand.Append("<TelVoice>" + XmlCommon.XmlEncode(UseBillingAddress.Phone) + "</TelVoice>");
            transactionCommand.Append("<Address>");
            transactionCommand.Append("<City>" + XmlCommon.XmlEncode(UseBillingAddress.City) + "</City>");
            transactionCommand.Append("<Company>" + XmlCommon.XmlEncode(UseBillingAddress.Company) + "</Company>");
            transactionCommand.Append("<Country>" + XmlCommon.XmlEncode(BillingISOCode) + "</Country>");
            transactionCommand.Append("<FirstName>" + XmlCommon.XmlEncode(UseBillingAddress.FirstName) + "</FirstName>");
            transactionCommand.Append("<LastName>" + XmlCommon.XmlEncode(UseBillingAddress.LastName) + "</LastName>");
            transactionCommand.Append("<PostalCode>" + XmlCommon.XmlEncode(UseBillingAddress.Zip) + "</PostalCode>");
            transactionCommand.Append("<StatProv>" + XmlCommon.XmlEncode(UseBillingAddress.State) + "</StatProv>");
            transactionCommand.Append("<Street1>" + XmlCommon.XmlEncode(UseBillingAddress.Address1) + "</Street1>");
            transactionCommand.Append("<Street2>" + XmlCommon.XmlEncode(UseBillingAddress.Address2) + "</Street2>");
            transactionCommand.Append("<Street3>" + XmlCommon.XmlEncode(UseBillingAddress.Suite) + "</Street3>");
            transactionCommand.Append("</Address>");
            transactionCommand.Append("</Location>");
            transactionCommand.Append("</BillTo>");

            transactionCommand.Append("<ShipTo>");
            transactionCommand.Append("<Location>");
            transactionCommand.Append("<Email>" + XmlCommon.XmlEncode(UseShippingAddress.EMail) + "</Email>");
            transactionCommand.Append("<TelVoice>" + XmlCommon.XmlEncode(UseShippingAddress.Phone) + "</TelVoice>");
            transactionCommand.Append("<Address>");
            transactionCommand.Append("<City>" + XmlCommon.XmlEncode(UseShippingAddress.City) + "</City>");
            transactionCommand.Append("<Company>" + XmlCommon.XmlEncode(UseShippingAddress.Company) + "</Company>");
            transactionCommand.Append("<Country>" + XmlCommon.XmlEncode(ShippingISOCode) + "</Country>");
            transactionCommand.Append("<FirstName>" + XmlCommon.XmlEncode(UseShippingAddress.FirstName) + "</FirstName>");
            transactionCommand.Append("<LastName>" + XmlCommon.XmlEncode(UseShippingAddress.LastName) + "</LastName>");
            transactionCommand.Append("<PostalCode>" + XmlCommon.XmlEncode(UseShippingAddress.Zip) + "</PostalCode>");
            transactionCommand.Append("<StatProv>" + XmlCommon.XmlEncode(UseShippingAddress.State) + "</StatProv>");
            transactionCommand.Append("<Street1>" + XmlCommon.XmlEncode(UseShippingAddress.Address1) + "</Street1>");
            transactionCommand.Append("<Street2>" + XmlCommon.XmlEncode(UseShippingAddress.Address2) + "</Street2>");
            transactionCommand.Append("<Street3>" + XmlCommon.XmlEncode(UseShippingAddress.Suite) + "</Street3>");
            transactionCommand.Append("</Address>");
            transactionCommand.Append("</Location>");
            transactionCommand.Append("</ShipTo>");

            transactionCommand.Append("</Consumer>");
            transactionCommand.Append("<Transaction>");
            transactionCommand.Append("<InvNumber>" + OrderNumber.ToString() + "</InvNumber>");
            transactionCommand.Append("<Type>" + CommonLogic.IIF(TransactionMode == TransactionModeEnum.auth, "PreAuth", "Auth") + "</Type>");
            transactionCommand.Append("<CurrentTotals>");
            transactionCommand.Append("<Totals>");
            transactionCommand.Append("<Total DataType=\"Money\" Currency=\"" + Localization.StoreCurrencyNumericCode() + "\">" + Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal).Replace(".", "") + "</Total>");
            transactionCommand.Append("</Totals>");
            transactionCommand.Append("</CurrentTotals>");
            if (TransactionAuthentication.Length != 0)
            {
                transactionCommand.Append(TransactionAuthentication);
            }
            transactionCommand.Append("</Transaction>");
            transactionCommand.Append("</OrderFormDoc>");
            transactionCommand.Append("</EngineDoc>");
            transactionCommand.Append("</EngineDocList>");

            byte[] data = encoding.GetBytes(transactionCommand.ToString());

            // Prepare web request...
            String AuthServer = CommonLogic.IIF(useLiveTransactions, AppLogic.AppConfig("HSBC.Live.Server"), AppLogic.AppConfig("HSBC.Test.Server"));

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
                // Close and clean up the StreamReader
                sr.Close();
                //Close the Response
                myResponse.Close();
            }
            catch
            {
                result = "NO RESPONSE FROM HSBC GATEWAY!";
            }

            // rawResponseString now has gateway response
            TransactionResponse = rawResponseString;

            AVSResult = String.Empty;

            String replyCode = String.Empty;
            String approvalCode = String.Empty;
            String authResponse = String.Empty;
            String TransID = String.Empty;

            XmlDocument responseXml = new XmlDocument();
            try
            {
                responseXml.LoadXml(rawResponseString);

                XmlNode n = responseXml.SelectSingleNode("//Overview");
                replyCode = XmlCommon.XmlField(n,"TransactionStatus");
                String CcErrCode = XmlCommon.XmlField(n, "CcErrCode");
                approvalCode = XmlCommon.XmlField(n, "AuthCode");
                authResponse = XmlCommon.XmlField(n, "Notice");

                if (TransactionAuthentication.Length != 0)
                {
                    // encode it to store in the session, it will be decoded before being saved to the database
                    byte[] str = System.Text.Encoding.UTF8.GetBytes(TransactionAuthentication);
                    cSession["3DSecure.LookupResult"] = Convert.ToBase64String(str);
                }

                if (authResponse.Length == 0)
                {
					XmlNode n2 = CommonLogic.IsNull(responseXml.SelectSingleNode("//Message/Text"), responseXml.SelectSingleNode("//Transaction/CardProcResp/CcReturnMsg"));
                    if (n2 != null)
                    {
                        authResponse = n2.InnerText;
                    }
                }
                TransID = XmlCommon.XmlField(n, "OrderId");
                try
                {
                    AVSResult = "AVSDisplay: " + responseXml.SelectSingleNode("//AvsDisplay").InnerText + ", CVV2Resp: " + responseXml.SelectSingleNode("//Cvv2Resp").InnerText + ", FraudConfidence: " + responseXml.SelectSingleNode("//FraudConfidence").InnerText;
                    AVSResult = AVSResult.Substring(0, 50); // our db is 50 max!
                }
                catch { }
            }
            catch
            {
                throw new ArgumentException("HSBC Unexpected Response: " + rawResponseString);
            }

            AuthorizationCode = approvalCode;
            AuthorizationResult = rawResponseString;
            AuthorizationTransID = TransID;
            TransactionCommandOut = transactionCommand.ToString();
            TransactionResponse = authResponse;

            if (replyCode.Equals("A", StringComparison.InvariantCultureIgnoreCase) == false && 
                replyCode.Equals("C", StringComparison.InvariantCultureIgnoreCase) == false)
            {
                result = authResponse;
                if (result.Length == 0)
                {
                    result = "Unspecified Error";
                }
            }
            return result;
        }
    }
}
