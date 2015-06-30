// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Data;
using System.Security;
using System.Text;
using System.Web.SessionState;
using System.IO;
using System.Net;
using System.Globalization;
using AspDotNetStorefrontCore;
using System.Data.SqlClient;

namespace AspDotNetStorefrontGateways.Processors
{
    /// <summary>
    /// Summary description for NetBilling.
    /// </summary>
    public class NetBilling : GatewayProcessor
    {
        public NetBilling() { }

        public override String CaptureOrder(Order o)
        {
            String result = AppLogic.ro_OK;

            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");
            o.CaptureTXCommand = "";
            o.CaptureTXResult = "";

            String TransID = o.AuthorizationPNREF;
            Decimal OrderTotal = o.OrderBalance;
            String CardNumber = Security.UnmungeString(o.CardNumber, o.OrdersCCSaltField);
            if (CardNumber.StartsWith(Security.ro_DecryptFailedPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                CardNumber = o.CardNumber;
            }

            ASCIIEncoding encoding = new ASCIIEncoding();
            StringBuilder transactionCommand = new StringBuilder(4096);

            transactionCommand.Append("tran_type=D");
            transactionCommand.Append("&account_id=" + AppLogic.AppConfig("NetBilling.Account_ID"));
            transactionCommand.Append("&orig_id=" + TransID);

            o.CaptureTXCommand = transactionCommand.ToString();

            if (TransID.Length == 0 || TransID == "0")
            {
                result = "Invalid or Empty Transaction ID";
            }
            else if (CardNumber.Length == 0 || CardNumber == AppLogic.ro_CCNotStoredString)
            {
                result = "Credit Card Number Not Found or Empty";
            }
            else
            {
                try
                {

                    byte[] data = encoding.GetBytes(transactionCommand.ToString());

                    // Prepare web request...
                    String AuthServer = CommonLogic.IIF(useLiveTransactions, AppLogic.AppConfig("NetBilling.LIVE_SERVER"), AppLogic.AppConfig("NetBilling.TEST_SERVER"));
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
                    String[] statusArray = rawResponseString.Split('&');
                    String resultCode = String.Empty;
                    String replyMsg = String.Empty;
                    for (int i = statusArray.GetLowerBound(0); i <= statusArray.GetUpperBound(0); i++)
                    {
                        String[] lasKeyPair = statusArray[i].Split('=');
                        switch (lasKeyPair[0].ToLowerInvariant())
                        {
                            case "status_code":
                                resultCode = lasKeyPair[1];
                                break;
                            case "auth_msg":
                                replyMsg = lasKeyPair[1];
                                break;
                        }
                    }
                    String sql = String.Empty;

                    o.CaptureTXResult = rawResponseString;
                    
                    if (resultCode != "0" && resultCode != "F" && resultCode != "D")
                    {
                        result = AppLogic.ro_OK;
                    }
                    else
                    {
                        result = replyMsg;
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
            String result = "VOID not supported by NetBilling Gateway";
            return result;
        }

        // if RefundAmount == 0.0M, then then ENTIRE order amount will be refunded!
        public override String RefundOrder(int OriginalOrderNumber, int NewOrderNumber, decimal RefundAmount, String RefundReason, Address UseBillingAddress)
        {
            String result = AppLogic.ro_OK;

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

            ASCIIEncoding encoding = new ASCIIEncoding();
            StringBuilder transactionCommand = new StringBuilder(4096);

            if (RefundAmount != System.Decimal.Zero)
            {
                return "Partial Refunds Not Implemented in NetBilling Gateway API";
            }

            transactionCommand.Append("tran_type=R");
            transactionCommand.Append("&account_id=" + AppLogic.AppConfig("NetBilling.Account_ID"));
            transactionCommand.Append("&orig_id=" + TransID);

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
                    String AuthServer = CommonLogic.IIF(useLiveTransactions, AppLogic.AppConfig("NetBilling.LIVE_SERVER"), AppLogic.AppConfig("NetBilling.TEST_SERVER"));
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
                    String[] statusArray = rawResponseString.Split('&');
                    String resultCode = String.Empty;
                    String replyMsg = String.Empty;
                    for (int i = statusArray.GetLowerBound(0); i <= statusArray.GetUpperBound(0); i++)
                    {
                        String[] lasKeyPair = statusArray[i].Split('=');
                        switch (lasKeyPair[0].ToLowerInvariant())
                        {
                            case "status_code":
                                resultCode = lasKeyPair[1];
                                break;
                            case "auth_msg":
                                replyMsg = lasKeyPair[1];
                                break;
                        }
                    }
                    String sql = String.Empty;

                    DB.ExecuteSQL("update orders set RefundTXResult=" + DB.SQuote(rawResponseString) + " where OrderNumber=" + OriginalOrderNumber.ToString());
                    if (resultCode != "0" && resultCode != "F" && resultCode != "D")
                    {
                        result = AppLogic.ro_OK;
                    }
                    else
                    {
                        result = replyMsg;
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

            transactionCommand.Append("tran_type=" + CommonLogic.IIF(TransactionMode == TransactionModeEnum.auth, "A", "S"));

            transactionCommand.Append("&account_id=" + AppLogic.AppConfig("NetBilling.Account_ID"));
            transactionCommand.Append("&site_tag=" + AppLogic.AppConfig("NetBilling.Site_Tag"));
            transactionCommand.Append("&pay_type=" + AppLogic.AppConfig("NetBilling.Pay_Type"));

            transactionCommand.Append("&amount=" + Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal));
            transactionCommand.Append("&card_number=" + UseBillingAddress.CardNumber);
            if (CardExtraCode.Trim().Length != 0)
            {
                transactionCommand.Append("&card_cvv2=" + CardExtraCode);
            }

            transactionCommand.Append("&card_expire=" + UseBillingAddress.CardExpirationMonth.PadLeft(2, '0') + UseBillingAddress.CardExpirationYear.Substring(2, 2));
            transactionCommand.Append("&user_data=" + CustomerID.ToString());
            transactionCommand.Append("&description=" + HttpContext.Current.Server.UrlEncode(OrderNumber.ToString()));
            transactionCommand.Append("&cust_phone=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.Phone));

            if (CAVV.Length != 0 || ECI.Length != 0)
            {
                transactionCommand.Append("&3ds_cavv=" + CAVV);
                transactionCommand.Append("&3ds_xid=" + ECI);
            }

            if (AppLogic.AppConfigBool("NetBilling.VERIFY_ADDRESSES"))
            {
                transactionCommand.Append("&bill_name1=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.FirstName));
                transactionCommand.Append("&bill_name2=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.LastName));
                transactionCommand.Append("&bill_street=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.Address1));
                transactionCommand.Append("&bill_city=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.City));
                transactionCommand.Append("&bill_state=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.State));
                transactionCommand.Append("&bill_zip=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.Zip));
                transactionCommand.Append("&bill_country=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.Country));
            }

            if (UseShippingAddress != null)
            {
                transactionCommand.Append("&ship_name1=" + HttpContext.Current.Server.UrlEncode(UseShippingAddress.FirstName));
                transactionCommand.Append("&ship_name2=" + HttpContext.Current.Server.UrlEncode(UseShippingAddress.LastName));
                transactionCommand.Append("&ship_street=" + HttpContext.Current.Server.UrlEncode(UseShippingAddress.Address1));
                transactionCommand.Append("&ship_city=" + HttpContext.Current.Server.UrlEncode(UseShippingAddress.City));
                transactionCommand.Append("&ship_state=" + HttpContext.Current.Server.UrlEncode(UseShippingAddress.State));
                transactionCommand.Append("&ship_zip=" + HttpContext.Current.Server.UrlEncode(UseShippingAddress.Zip));
                transactionCommand.Append("&ship_country=" + HttpContext.Current.Server.UrlEncode(UseShippingAddress.Country));
            }

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
                String AuthServer = CommonLogic.IIF(useLiveTransactions, AppLogic.AppConfig("NetBilling.LIVE_SERVER"), AppLogic.AppConfig("NetBilling.TEST_SERVER"));
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

                bool AVSOK = true;

                String[] statusArray = rawResponseString.Split('&');
                String resultCode = String.Empty;
                for (int i = statusArray.GetLowerBound(0); i <= statusArray.GetUpperBound(0); i++)
                {
                    String[] lasKeyPair = statusArray[i].Split('=');
                    switch (lasKeyPair[0].ToLowerInvariant())
                    {
                        case "trans_id":
                            AuthorizationTransID = lasKeyPair[1];
                            break;
                        case "status_code":
                            resultCode = lasKeyPair[1];
                            break;
                        case "auth_msg":
                            AuthorizationResult = lasKeyPair[1];
                            break;
                        case "auth_code":
                            AuthorizationCode = lasKeyPair[1];
                            break;
                        case "avs_code":
                            AVSResult = lasKeyPair[1];
                            break;
                    }
                }

                TransactionCommandOut = transactionCommand.ToString();

                if ((resultCode != "0" && resultCode != "F" && resultCode != "D") && AVSOK)
                {
                    result = AppLogic.ro_OK;
                }
                else
                {
                    result = AuthorizationResult;
                    if (result.Length == 0)
                    {
                        result = "Unspecified Error";
                    }
                    result = result.Replace("account", "card");
                    result = result.Replace("Account", "Card");
                    result = result.Replace("ACCOUNT", "CARD");
                }
            }
            catch (Exception ex)
            {
                String s = ex.Message;
                result = "Error calling NetBilling gateway(" + s + "). Please retry your order in a few minutes";
            }
            return result;
        }

    }
}
