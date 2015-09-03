// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Security;
using System.Text;
using System.Web.SessionState;
using System.IO;
using System.Net;
using System.Globalization;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontGateways.Processors
{
    /// <summary>
    /// Summary description for TRANSACTIONCENTRAL.
    /// </summary>
    public class TransactionCentral : GatewayProcessor
    {
        public TransactionCentral() { }

        public override String CaptureOrder(Order o)
        {
            String result = "CAPTURE IS NOT SUPPORTED FOR TRANSACTION CENTRAL GATEWAY";
            return result;
        }

        public override string AdministratorSetupPrompt
        {
            get
            {
                return "(Same as MerchantAnywhere.)";
            }
        }

        public override String VoidOrder(int OrderNumber)
        {
            String result = AppLogic.ro_OK;

            DB.ExecuteSQL("update orders set VoidTXCommand=NULL, VoidTXResult=NULL where OrderNumber=" + OrderNumber.ToString());
            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");
            String TransID = String.Empty;

            Decimal OrderTotal = System.Decimal.Zero;
            
            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
                using(IDataReader rs = DB.GetRS("select * from orders where OrderNumber=" + OrderNumber.ToString(),dbconn))
                {
                    if (rs.Read())
                    {
                        TransID = DB.RSField(rs, "AuthorizationPNREF");
                        OrderTotal = DB.RSFieldDecimal(rs, "OrderTotal");
                    }
                
                }
            
            }
            
            ASCIIEncoding encoding = new ASCIIEncoding();
            StringBuilder transactionCommand = new StringBuilder(4096);

            transactionCommand.Append("MerchantID=" + AppLogic.AppConfig("TRANSACTIONCENTRAL_MERCHANTID"));
            transactionCommand.Append("&RegKey=" + AppLogic.AppConfig("TRANSACTIONCENTRAL_REGKEY"));
            transactionCommand.Append("&TransID=" + TransID.ToString());
            transactionCommand.Append("&CreditAmount=" + Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal));
            transactionCommand.Append("&CCRURL="); // we want immediate postback

            DB.ExecuteSQL("update orders set VoidTXCommand=" + DB.SQuote(transactionCommand.ToString()) + " where OrderNumber=" + OrderNumber.ToString());

            if (TransID.Length == 0 || TransID == "0")
            {
                result = "Invalid or Empty Transaction ID";
            }
            else
            {
                try
                {
                    byte[] data = encoding.GetBytes(transactionCommand.ToString());

                    // Prepare web request...
                    String AuthServer = AppLogic.AppConfig("TRANSACTIONCENTRAL_VOID_SERVER");
                    HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(AuthServer);
                    myRequest.Method = "POST";
                    myRequest.ContentType = "application/x-www-form-urlencoded";
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
                    String[] statusArray = CommonLogic.ExtractBody(rawResponseString).Split('&');

                    String TransIDResponse = String.Empty;
                    String TransType = String.Empty;
                    String Auth = String.Empty;
                    String Notes = String.Empty;

                    foreach (String s in statusArray)
                    {
                        String[] Keys = s.Split('=');
                        switch (Keys[0].Trim().Replace("\t", "").ToUpperInvariant())
                        {
                            case "TRANSID":
                                TransIDResponse = Keys[1];
                                break;
                            case "AUTH":
                                Auth = Keys[1];
                                break;
                            case "TRANSTYPE":
                                TransType = Keys[1];
                                break;
                            case "NOTES":
                                Notes = Keys[1];
                                break;
                        }
                    }

                    DB.ExecuteSQL("update orders set VoidTXResult=" + DB.SQuote(rawResponseString) + " where OrderNumber=" + OrderNumber.ToString());

                    if (Auth.Trim().ToUpperInvariant() != "DECLINED")
                    {
                        result = AppLogic.ro_OK;
                    }
                    else
                    {
                        result = Notes;
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
            // they seem to use the identical calls for both void & refund (seems strange to us)
            String result = AppLogic.ro_OK;

            DB.ExecuteSQL("update orders set RefundTXCommand=NULL, RefundTXResult=NULL where OrderNumber=" + OriginalOrderNumber.ToString());
            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");
            String TransID = String.Empty;

            Decimal OrderTotal = System.Decimal.Zero;
           
            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
                using(IDataReader rs = DB.GetRS("select * from orders where OrderNumber=" + OriginalOrderNumber.ToString(),dbconn))
                {
                    
                    if (rs.Read())
                    {
                        TransID = DB.RSField(rs, "AuthorizationPNREF");
                        OrderTotal = DB.RSFieldDecimal(rs, "OrderTotal");
                    }
                }
            
            }
            
            ASCIIEncoding encoding = new ASCIIEncoding();
            StringBuilder transactionCommand = new StringBuilder(4096);

            transactionCommand.Append("MerchantID=" + AppLogic.AppConfig("TRANSACTIONCENTRAL_MERCHANTID"));
            transactionCommand.Append("&RegKey=" + AppLogic.AppConfig("TRANSACTIONCENTRAL_REGKEY"));
            transactionCommand.Append("&TransID=" + TransID.ToString());
            if (RefundAmount == System.Decimal.Zero)
            {
                transactionCommand.Append("&CreditAmount=" + Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal));
            }
            else
            {
                transactionCommand.Append("&CreditAmount=" + Localization.CurrencyStringForGatewayWithoutExchangeRate(RefundAmount));
            }
            transactionCommand.Append("&CCRURL="); // we want immediate postback

            DB.ExecuteSQL("update orders set RefundTXCommand=" + DB.SQuote(transactionCommand.ToString()) + " where OrderNumber=" + OriginalOrderNumber.ToString());

            if (TransID.Length == 0 || TransID == "0")
            {
                result = "Invalid or Empty Transaction ID";
            }
            else
            {
                try
                {
                    byte[] data = encoding.GetBytes(transactionCommand.ToString());

                    // Prepare web request...
                    String AuthServer = AppLogic.AppConfig("TRANSACTIONCENTRAL_VOID_SERVER");
                    HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(AuthServer);
                    myRequest.Method = "POST";
                    myRequest.ContentType = "application/x-www-form-urlencoded";
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
                    String[] statusArray = CommonLogic.ExtractBody(rawResponseString).Split('&');

                    String TransIDResponse = String.Empty;
                    String TransType = String.Empty;
                    String Auth = String.Empty;
                    String Notes = String.Empty;

                    foreach (String s in statusArray)
                    {
                        String[] Keys = s.Split('=');
                        switch (Keys[0].Trim().Replace("\t", "").ToUpperInvariant())
                        {
                            case "TRANSID":
                                TransIDResponse = Keys[1];
                                break;
                            case "AUTH":
                                Auth = Keys[1];
                                break;
                            case "TRANSTYPE":
                                TransType = Keys[1];
                                break;
                            case "NOTES":
                                Notes = Keys[1];
                                break;
                        }
                    }

                    DB.ExecuteSQL("update orders set RefundTXResult=" + DB.SQuote(rawResponseString) + " where OrderNumber=" + OriginalOrderNumber.ToString());

                    if (Auth.Trim().ToUpperInvariant() != "DECLINED")
                    {
                        result = AppLogic.ro_OK;
                    }
                    else
                    {
                        result = Notes;
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

            transactionCommand.Append("MerchantID=" + AppLogic.AppConfig("TRANSACTIONCENTRAL_MERCHANTID"));
            transactionCommand.Append("&RegKey=" + AppLogic.AppConfig("TRANSACTIONCENTRAL_REGKEY"));
            transactionCommand.Append("&Amount=" + Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal));
            transactionCommand.Append("&REFID=" + OrderNumber.ToString());
            transactionCommand.Append("&AccountNo=" + UseBillingAddress.CardNumber);
            transactionCommand.Append("&CCMonth=" + UseBillingAddress.CardExpirationMonth.PadLeft(2, '0'));
            transactionCommand.Append("&CCYear=" + UseBillingAddress.CardExpirationYear);
            transactionCommand.Append("&NameOnAccount=" + HttpContext.Current.Server.UrlEncode((UseBillingAddress.FirstName + " " + UseBillingAddress.LastName)));
            transactionCommand.Append("&AVSADDR=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.Address1));
            transactionCommand.Append("&AVSZIP=" + UseBillingAddress.Zip);
            transactionCommand.Append("&CCRURL="); // we want immediate postback
            if (CardExtraCode.Trim().Length != 0)
            {
                transactionCommand.Append("&CVV2=" + CardExtraCode);
            }
            transactionCommand.Append("&USER1=" + CustomerID.ToString());
            transactionCommand.Append("&USER2=" + OrderNumber.ToString());
            transactionCommand.Append("&USER3=" + HttpContext.Current.Server.UrlEncode(AppLogic.AppConfig("StoreName")));

            byte[] data = encoding.GetBytes(transactionCommand.ToString());

            // Prepare web request...
            AuthorizationCode = String.Empty;
            AuthorizationResult = String.Empty;
            AuthorizationTransID = String.Empty;
            AVSResult = String.Empty;
            TransactionCommandOut = String.Empty;
            TransactionResponse = String.Empty;
            try
            {
                String AuthServer = CommonLogic.IIF(useLiveTransactions, AppLogic.AppConfig("TRANSACTIONCENTRAL_LIVE_SERVER"), AppLogic.AppConfig("TRANSACTIONCENTRAL_TEST_SERVER"));
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(AuthServer);
                myRequest.Method = "POST";
                myRequest.ContentType = "application/x-www-form-urlencoded";
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

                AuthorizationCode = String.Empty;
                AuthorizationResult = String.Empty;
                AuthorizationTransID = String.Empty;
                AVSResult = String.Empty;
                TransactionCommandOut = transactionCommand.ToString();

                // rawResponseString now has gateway response
                TransactionResponse = rawResponseString;
                String[] statusArray = CommonLogic.ExtractBody(rawResponseString).Split('&');

                foreach (String s in statusArray)
                {
                    String[] Keys = s.Split('=');
                    switch (Keys[0].Trim().Replace("\t", "").ToUpperInvariant())
                    {
                        case "TRANSID":
                            AuthorizationTransID = Keys[1];
                            break;
                        case "REFNO":
                            break;
                        case "AUTH":
                            AuthorizationCode = Keys[1];
                            break;
                        case "AVSCODE":
                            AVSResult = Keys[1];
                            break;
                        case "CVV2RESPONSEMSG":
                            // not used
                            break;
                        case "NOTES":
                            AuthorizationResult = Keys[1];
                            break;
                    }
                }

                if (AuthorizationCode.Trim().ToUpperInvariant() != "DECLINED" && AuthorizationCode.Trim().Length != 0)
                {
                    result = AppLogic.ro_OK;
                }
                else
                {
                    result = AuthorizationResult;
                    if (result.Length == 0)
                    {
                        result = "Unspecified Error[" + HttpContext.Current.Server.HtmlEncode(rawResponseString.ToLowerInvariant().Replace("<html>", "").Replace("</html>", "")) + "]";
                    }
                    result = result.Replace("account", "card");
                    result = result.Replace("Account", "Card");
                    result = result.Replace("ACCOUNT", "CARD");
                }
            }
            catch
            {
                result = "Error calling TransactionCentral gateway. Please retry your order in a few minutes";
            }
            return result;
        }

    }
}
