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
using System.Collections.Generic;

namespace AspDotNetStorefrontGateways.Processors
{
    public class Skipjack : GatewayProcessor
    {
        public Skipjack() { }

        // Transaction status for different transactions (void, refund)
        public override String CaptureOrder(Order o)
        {
            // With SkipJack configured for Automatic Settlement the storefront should
            //   have AppConfig TransationMode set to AUTH CAPTURE.
            // With SkipJack configured for Manual Settlement the storefront should
            //   have AppConfig TransationMode set to AUTH.

            Object isApproved = String.Empty;
            String result = AppLogic.ro_OK;
            o.CaptureTXCommand = "";
            o.CaptureTXResult = "";
            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");
            String TransID = o.AuthorizationPNREF;
            Decimal OrderTotal = o.OrderBalance;

            o.CaptureTXCommand = o.OrderNumber + " " + TransID;

            ASCIIEncoding encoding = new ASCIIEncoding();
            StringBuilder transactionCommand = new StringBuilder(4096);

            if (useLiveTransactions)
            {
                transactionCommand.Append("szSerialNumber=" + AppLogic.AppConfig("Skipjack.LiveSerialNumber"));
            }
            else
            {
                transactionCommand.Append("szSerialNumber=" + AppLogic.AppConfig("Skipjack.TestSerialNumber"));
            }
            if (AppLogic.AppConfig("Skipjack.DeveloperSerialNumber").Length != 0)
            {
                transactionCommand.Append("&szDeveloperSerialNumber=" + AppLogic.AppConfig("Skipjack.DeveloperSerialNumber"));
            }
            transactionCommand.Append("&szOrderNumber=" + o.OrderNumber.ToString());
            transactionCommand.Append("&szTransactionId=" + TransID);
            transactionCommand.Append("&szDesiredStatus=SETTLE");
            transactionCommand.Append("&szAmount=" + Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal));
            transactionCommand.Append("&szForceSettlement=" + CommonLogic.IIF(AppLogic.AppConfigBool("Skipjack.ForceSettlement"), "1", "0"));

            String AuthServer = String.Empty;
            String StatusRecord = String.Empty;
            String ResponseRecord = String.Empty;

            byte[] data = encoding.GetBytes(transactionCommand.ToString());

            try
            {
                if (useLiveTransactions)
                {
                    AuthServer = AppLogic.AppConfig("Skipjack.LiveChangeURL");
                }
                else
                {
                    AuthServer = AppLogic.AppConfig("Skipjack.TestChangeURL");
                }

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


                if (!String.IsNullOrEmpty(rawResponseString.ToString()))
                {
                    if (AppLogic.AppConfigBool("SkipJack.InsertAdvanceFormComma"))
                        rawResponseString = rawResponseString.Replace(Environment.NewLine, "," + Environment.NewLine); // stick a comma at the end of the title row to throw off indexes FOR PARSER TESTING ONLY

                    //Dictionary<String, String> response = ParseResponse(rawResponseString.ToString());
                    string[] recordArray = rawResponseString.ToString().Replace(Environment.NewLine, "|").Split('|');
                    StatusRecord = recordArray[0];
                    ResponseRecord = recordArray[1];

                    o.CaptureTXResult = ResponseRecord; 

                    string[] statusArray = StatusRecord.Replace("\"", "").Split(',');

                    if (statusArray[1] != "0")
                    {
                        result = "Error: " + ResponseRecord;
                    }
                    else
                    {
                        string[] responseArray = ResponseRecord.Replace("\"", "").Split(',');
                        if (responseArray[3] == "SUCCESSFUL")
                        {
                            result = AppLogic.ro_OK;
                        }
                        else
                        {
                            result = "Failed: " + ResponseRecord;
                        }
                    }
                }
                else
                {
                    result = "NO RESPONSE FROM SKIPJACK GATEWAY!";
                }
            }
            catch
            {
                result = "NO RESPONSE FROM SKIPJACK GATEWAY!";
            }
            return result;
        }

        public override String VoidOrder(int OrderNumber)
        {
            Object isApproved = String.Empty;
            String result = AppLogic.ro_OK;
            DB.ExecuteSQL("update orders set VoidTXCommand=NULL, VoidTXResult=NULL where OrderNumber=" + OrderNumber.ToString());
            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");
            String TransID = String.Empty;
            Decimal OrderTotal = System.Decimal.Zero;

            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
                using(IDataReader rs = DB.GetRS("select * from Orders  with (NOLOCK)  where OrderNumber=" + OrderNumber.ToString(),dbconn))
                {
                    if (rs.Read())
                    {
                        TransID = DB.RSField(rs, "AuthorizationPNREF");
                        OrderTotal = DB.RSFieldDecimal(rs, "OrderTotal");
                    }
                
                }
            
            }
         
            DB.ExecuteSQL("update orders set VoidTXCommand=" + DB.SQuote(OrderNumber + " " + TransID) + " where OrderNumber=" + OrderNumber.ToString());

            ASCIIEncoding encoding = new ASCIIEncoding();
            StringBuilder transactionCommand = new StringBuilder(4096);

            if (useLiveTransactions)
            {
                transactionCommand.Append("szSerialNumber=" + AppLogic.AppConfig("Skipjack.LiveSerialNumber"));
            }
            else
            {
                transactionCommand.Append("szSerialNumber=" + AppLogic.AppConfig("Skipjack.TestSerialNumber"));
            }
            if (AppLogic.AppConfig("Skipjack.DeveloperSerialNumber").Length != 0)
            {
                transactionCommand.Append("&szDeveloperSerialNumber=" + AppLogic.AppConfig("Skipjack.DeveloperSerialNumber"));
            }
            transactionCommand.Append("&szOrderNumber=" + OrderNumber.ToString());
            transactionCommand.Append("&szTransactionId=" + TransID);
            transactionCommand.Append("&szDesiredStatus=DELETE");

            String AuthServer = String.Empty;
            String StatusRecord = String.Empty;
            String ResponseRecord = String.Empty;

            byte[] data = encoding.GetBytes(transactionCommand.ToString());

            try
            {
                if (useLiveTransactions)
                {
                    AuthServer = AppLogic.AppConfig("Skipjack.LiveChangeURL");
                }
                else
                {
                    AuthServer = AppLogic.AppConfig("Skipjack.TestChangeURL");
                }

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



                if (!String.IsNullOrEmpty(rawResponseString.ToString()))
                {
                    if (AppLogic.AppConfigBool("SkipJack.InsertAdvanceFormComma"))
                        rawResponseString = rawResponseString.Replace(Environment.NewLine, "," + Environment.NewLine); // stick a comma at the end of the title row to throw off indexes FOR PARSER TESTING ONLY

                    string[] recordArray = rawResponseString.ToString().Replace(Environment.NewLine, "|").Split('|');
                    StatusRecord = recordArray[0];
                    ResponseRecord = recordArray[1];

                    DB.ExecuteSQL("update orders set VoidTXResult=" + DB.SQuote(ResponseRecord) + " where OrderNumber=" + OrderNumber.ToString());

                    string[] statusArray = StatusRecord.Replace("\"", "").Split(',');

                    if (statusArray[1] != "0")
                    {
                        result = "Error: " + ResponseRecord;
                    }
                    else
                    {
                        string[] responseArray = ResponseRecord.Replace("\"", "").Split(',');
                        if (responseArray[3] == "SUCCESSFUL")
                        {
                            result = AppLogic.ro_OK;
                        }
                        else
                        {
                            result = "Failed: " + ResponseRecord;
                        }
                    }
                }
                else
                {
                    result = "NO RESPONSE FROM SKIPJACK GATEWAY!";
                }
            }
            catch
            {
                result = "NO RESPONSE FROM SKIPJACK GATEWAY!";
            }
            return result;
        }


        public override String RefundOrder(int OriginalOrderNumber, int OrderNumber, decimal RefundAmount, String RefundReason, Address UseBillingAddress)
        {
            Object isApproved = String.Empty;
            String result = AppLogic.ro_OK;
            DB.ExecuteSQL("update orders set RefundTXCommand=NULL, RefundTXResult=NULL where OrderNumber=" + OriginalOrderNumber.ToString());
            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");
            String TransID = String.Empty;
            Decimal OrderTotal = System.Decimal.Zero;

            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
                using(IDataReader rs = DB.GetRS("select * from orders  with (NOLOCK)  where OrderNumber=" + OriginalOrderNumber.ToString(),dbconn))
                {
                    if (rs.Read())
                    {
                        TransID = DB.RSField(rs, "AuthorizationPNREF");
                        OrderTotal = DB.RSFieldDecimal(rs, "OrderTotal");
                    }
                
                }
            
            }
           
            DB.ExecuteSQL("update orders set RefundTXCommand=" + DB.SQuote(OrderNumber + " " + TransID) + " where OrderNumber=" + OriginalOrderNumber.ToString());

            ASCIIEncoding encoding = new ASCIIEncoding();
            StringBuilder transactionCommand = new StringBuilder(4096);

            if (useLiveTransactions)
            {
                transactionCommand.Append("szSerialNumber=" + AppLogic.AppConfig("Skipjack.LiveSerialNumber"));
            }
            else
            {
                transactionCommand.Append("szSerialNumber=" + AppLogic.AppConfig("Skipjack.TestSerialNumber"));
            }
            if (AppLogic.AppConfig("Skipjack.DeveloperSerialNumber").Length != 0)
            {
                transactionCommand.Append("&szDeveloperSerialNumber=" + AppLogic.AppConfig("Skipjack.DeveloperSerialNumber"));
            }
            transactionCommand.Append("&szOrderNumber=" + OrderNumber.ToString());
            transactionCommand.Append("&szTransactionId=" + TransID);
            transactionCommand.Append("&szDesiredStatus=CREDIT");
            if (RefundAmount != System.Decimal.Zero)
            {
                transactionCommand.Append("&szAmount=" + Localization.CurrencyStringForGatewayWithoutExchangeRate(RefundAmount));
            }
            transactionCommand.Append("&szForceSettlement=" + CommonLogic.IIF(AppLogic.AppConfigBool("Skipjack.ForceSettlement"), "1", "0"));

            String AuthServer = String.Empty;
            String StatusRecord = String.Empty;
            String ResponseRecord = String.Empty;

            byte[] data = encoding.GetBytes(transactionCommand.ToString());

            try
            {

                if (useLiveTransactions)
                {
                    AuthServer = AppLogic.AppConfig("Skipjack.LiveChangeURL");
                }
                else
                {
                    AuthServer = AppLogic.AppConfig("Skipjack.TestChangeURL");
                }

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

                if (!String.IsNullOrEmpty(rawResponseString.ToString()))
                {
                    string[] recordArray = rawResponseString.ToString().Replace(Environment.NewLine, "|").Split('|');
                    StatusRecord = recordArray[0];
                    ResponseRecord = recordArray[1];

                    DB.ExecuteSQL("update orders set RefundTXResult=" + DB.SQuote(ResponseRecord) + " where OrderNumber=" + OriginalOrderNumber.ToString());

                    string[] statusArray = StatusRecord.Replace("\"", "").Split(',');

                    if (statusArray[1] != "0")
                    {
                        result = "Error: " + ResponseRecord;
                    }
                    else
                    {
                        string[] responseArray = ResponseRecord.Replace("\"", "").Split(',');
                        if (responseArray[3] == "SUCCESSFUL")
                        {
                            result = AppLogic.ro_OK;
                        }
                        else
                        {
                            if (responseArray[4] == "Status Mismatch")
                            {
                                result = "Failed: Refund not permitted on this transaction.";
                            }
                            else
                            {
                                result = "Failed: " + ResponseRecord;
                            }
                        }
                    }
                }
                else
                {
                    result = "NO RESPONSE FROM SKIPJACK GATEWAY!";
                }
            }
            catch
            {
                result = "NO RESPONSE FROM SKIPJACK GATEWAY!";
            }
            return result;
        }

        public override String ProcessCard(int OrderNumber, int CustomerID, Decimal OrderTotal, bool useLiveTransactions, TransactionModeEnum TransactionMode, Address UseBillingAddress, String CardExtraCode, Address UseShippingAddress, String CAVV, String ECI, String XID, out String AVSResult, out String AuthorizationResult, out String AuthorizationCode, out String AuthorizationTransID, out String TransactionCommandOut, out String TransactionResponse)
        {

            // With SkipJack configured for Automatic Settlement the storefront should
            //   have AppConfig TransationMode set to AUTH CAPTURE.
            // With SkipJack configured for Manual Settlement the storefront should
            //   have AppConfig TransationMode set to AUTH.
          
            bool HasEmail = false;
            String result = AppLogic.ro_OK;
            ASCIIEncoding encoding = new ASCIIEncoding();
            StringBuilder transactionCommand = new StringBuilder(4096);                       
            transactionCommand.Append("sjname=" + HttpContext.Current.Server.UrlEncode((UseBillingAddress.FirstName + " " + UseBillingAddress.LastName).Trim()));
            if (UseBillingAddress.EMail.Length != 0)
            {
                HasEmail = true;
                transactionCommand.Append("&Email=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.EMail));
            }
            else if (UseShippingAddress.EMail.Length != 0)
            {
                HasEmail = true;
                transactionCommand.Append("&Email=" + HttpContext.Current.Server.UrlEncode(UseShippingAddress.EMail));
            }
            else if (HasEmail == false)
            {
                string Email = AppLogic.SkipJackEmail(CustomerID);
                transactionCommand.Append("&Email=" + HttpContext.Current.Server.UrlEncode(Email));
            }          
            
            transactionCommand.Append("&Streetaddress=" + HttpContext.Current.Server.UrlEncode((UseBillingAddress.Address1 + " " + UseBillingAddress.Address2).Trim()));
            transactionCommand.Append("&City=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.City));
            transactionCommand.Append("&State=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.State));
            transactionCommand.Append("&ZipCode=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.Zip));
            transactionCommand.Append("&Ordernumber=" + OrderNumber.ToString());
            transactionCommand.Append("&Accountnumber=" + UseBillingAddress.CardNumber);
            transactionCommand.Append("&Month=" + UseBillingAddress.CardExpirationMonth);
            transactionCommand.Append("&Year=" + UseBillingAddress.CardExpirationYear);
            if (CardExtraCode.Length != 0)
            {
                transactionCommand.Append("&CVV2=" + CardExtraCode);
            }

            // The Skipjack.SerialNumber for the ASPDotNetStoreFront needs to be added to the AppConfig table in the db
            if (useLiveTransactions)
            {
                transactionCommand.Append("&SerialNumber=" + AppLogic.AppConfig("Skipjack.LiveSerialNumber"));
            }
            else
            {
                transactionCommand.Append("&SerialNumber=" + AppLogic.AppConfig("Skipjack.TestSerialNumber"));
            }
            if (AppLogic.AppConfig("Skipjack.DeveloperSerialNumber").Length != 0)
            {
                transactionCommand.Append("&DeveloperSerialNumber=" + AppLogic.AppConfig("Skipjack.DeveloperSerialNumber"));
            }
            transactionCommand.Append("&TransactionAmount=" + Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal));
            transactionCommand.Append("&OrderString=" + HttpContext.Current.Server.UrlEncode((OrderNumber.ToString() + "~" + AppLogic.AppConfig("StoreName") + "Order~" + Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal) + "~1~N~||")));
            transactionCommand.Append("&Country=" + AppLogic.GetCountryTwoLetterISOCode(UseBillingAddress.Country));

            if (UseShippingAddress != null)
            {
                if (String.IsNullOrEmpty(UseShippingAddress.Phone))
                {
                    transactionCommand.Append("&ShipToPhone=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.Phone));
                }
                else
                {
                    transactionCommand.Append("&ShipToPhone=" + HttpContext.Current.Server.UrlEncode(UseShippingAddress.Phone));
                }
                transactionCommand.Append("&StreetAddress=" + HttpContext.Current.Server.UrlEncode((UseShippingAddress.FirstName + " " + UseShippingAddress.LastName).Trim()));
                transactionCommand.Append("&ShipToStreetAddress=" + HttpContext.Current.Server.UrlEncode(UseShippingAddress.Address1));
                transactionCommand.Append("&ShipToCity=" + HttpContext.Current.Server.UrlEncode(UseShippingAddress.City));
                transactionCommand.Append("&ShipToState=" + HttpContext.Current.Server.UrlEncode(UseShippingAddress.State));
                transactionCommand.Append("&ShipToZipcode=" + HttpContext.Current.Server.UrlEncode(UseShippingAddress.Zip));
            }
            else
            {
                transactionCommand.Append("&ShipToPhone=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.Phone));
                transactionCommand.Append("&StreetAddress=" + HttpContext.Current.Server.UrlEncode((UseBillingAddress.FirstName + " " + UseBillingAddress.LastName).Trim()));
                transactionCommand.Append("&ShipToStreetAddress=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.Address1));
                transactionCommand.Append("&ShipToCity=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.City));
                transactionCommand.Append("&ShipToState=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.State));
                transactionCommand.Append("&ShipToZipcode=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.Zip));
            }

            byte[] data = encoding.GetBytes(transactionCommand.ToString());

            // Prepare web request...
            Boolean TransactionCleared = false;

            AVSResult = String.Empty;
            String ResponseString = String.Empty;
            AuthorizationCode = String.Empty;
            AuthorizationResult = String.Empty;
            AuthorizationTransID = String.Empty;
            String AuthorizationDeclinedMsg = String.Empty;
            TransactionCommandOut = string.Empty;
            TransactionResponse = String.Empty;
            String AuthServer = String.Empty;

            try
            {
                if (useLiveTransactions)
                {
                    AuthServer = AppLogic.AppConfig("Skipjack.LiveServer");
                }
                else
                {
                    AuthServer = AppLogic.AppConfig("Skipjack.TestServer");
                }

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

                ResponseString = rawResponseString.ToString();
                if (AppLogic.AppConfigBool("SkipJack.InsertAdvanceFormComma"))
                    ResponseString = ResponseString.Replace(Environment.NewLine, "," + Environment.NewLine); // stick a comma at the end of the title row to throw off indexes FOR PARSER TESTING ONLY
                
                AuthorizationDeclinedMsg = AuthorizationDeclinedMsg + ResponseString;

                Dictionary<String, String> response = ParseResponse(ResponseString);

                bool WasApproved = false;
                String ReturnCode = String.Empty;

                foreach (KeyValuePair<String, String> kvp in response)
                {
                    switch (kvp.Key)
                    {
                        case "szIsApproved":
                            if (kvp.Value == "1")
                            {
                                WasApproved = true;
                            }
                            else
                            {
                                WasApproved = false;
                            }
                            break;
                        case "AUTHCODE":
                            AuthorizationCode = kvp.Value;
                            break;
                        case "szReturnCode":
                            ReturnCode = kvp.Value;
                            if (kvp.Value == "1")
                            {
                                TransactionCleared = true;
                            }
                            else
                            {
                                TransactionCleared = false;
                            }
                            break;
                        case "szAVSResponseCode":
                            AVSResult = kvp.Value;
                            break;
                        case "szAVSResponseMessage":
                            AuthorizationResult = kvp.Value;
                            break;
                        case "szAuthorizationDeclinedMessage":
                            AuthorizationDeclinedMsg = kvp.Value;
                            break;
                        case "szTransactionFileName":
                            AuthorizationTransID = kvp.Value;
                            break;
                    }
                }

                TransactionCommandOut = transactionCommand.ToString();
                TransactionResponse = rawResponseString;

                if (!TransactionCleared)
                {
                    result = "Transaction Failed with code " + ReturnCode + ": " + GetReturnCodeMessage(ReturnCode);
                }
                else
                {
                    if (WasApproved)
                    {
                        result = AppLogic.ro_OK;
                    }
                    else
                    {
                        result = "Error: " + AuthorizationDeclinedMsg;
                    }
                }
            }
            catch
            {
                result = "Error calling SkipJack gateway. Please retry your order in a few minutes or select another checkout payment option.";
            }
            TransactionResponse += Environment.NewLine + result;

            return result;
        }

        static private String GetReturnCodeMessage(String code)
        {
            String msg = "Unknown Failure";

            switch (code)
            {
                case "-1":
                    msg = "Error in request";
                    break;
                case "0":
                    msg = "Communication Failure. Check Transaction Status before retrying transaction.";
                    break;
                case "1":
                    msg = "Success";
                    break;
                case "-35":
                    msg = "Error invalid credit card number";
                    break;
                case "-37":
                    msg = "Error failed communication";
                    break;
                case "-39":
                    msg = "Error length serial number";
                    break;
                case "-51":
                    msg = "Error length zip code";
                    break;
                case "-52":
                    msg = "Error length shipto zip code";
                    break;
                case "-53":
                    msg = "Error length expiration date";
                    break;
                case "-54":
                    msg = "Error length account number date";
                    break;
                case "-55":
                    msg = "Error length street address";
                    break;
                case "-56":
                    msg = "Error length shipto street address";
                    break;
                case "-57":
                    msg = "Error length transaction amount";
                    break;
                case "-58":
                    msg = "Error length name";
                    break;
                case "-59":
                    msg = "Error length location";
                    break;
                case "-60":
                    msg = "Error length state";
                    break;
                case "-61":
                    msg = "Error length shipto state";
                    break;
                case "-62":
                    msg = "Error length order string";
                    break;
                case "-64":
                    msg = "Error invalid phone number";
                    break;
                case "-65":
                    msg = "Error empty name";
                    break;
                case "-66":
                    msg = "Error empty email";
                    break;
                case "-67":
                    msg = "Error empty street address";
                    break;
                case "-68":
                    msg = "Error empty city";
                    break;
                case "-69":
                    msg = "Error empty state";
                    break;
                case "-79":
                    msg = "Error length customer name";
                    break;
                case "-80":
                    msg = "Error length shipto customer name";
                    break;
                case "-81":
                    msg = "Error length customer location";
                    break;
                case "-82":
                    msg = "Error length customer state";
                    break;
                case "-83":
                    msg = "Error length shipto phone";
                    break;
                case "-84":
                    msg = "Pos error duplicate ordernumber";
                    break;
                case "-85":
                    msg = "Pos error airline leg info invalid";
                    break;
                case "-86":
                    msg = "Pos error airline ticket info invalid";
                    break;
                case "-87":
                    msg = "Pos check error routing number must be 9 numeric digits";
                    break;
                case "-88":
                    msg = "Pos error check account number missing or invalid";
                    break;
                case "-89":
                    msg = "Pos error check MICR missing or invalid";
                    break;
                case "-90":
                    msg = "Pos error check number missing or invalid";
                    break;
                case "-91":
                    msg = "Pos_error_CVV2";
                    break;
                case "-92":
                    msg = "Pos_error_Error_Approval_Code";
                    break;
                case "-93":
                    msg = "Pos_error_Blind_Credits_Not_Allowed";
                    break;
                case "-94":
                    msg = "Pos_error_Blind_Credits_Failed";
                    break;
                case "-95":
                    msg = "Pos_error_Voice_Authorizations_Not_Allowed";
                    break;
                case "-96":
                    msg = "Pos Error Voice Authorizations Failed";
                    break;
                case "-97":
                    msg = "Pos Error Fraud Rejection";
                    break;
                case "-98":
                    msg = "Pos Error Invalid Discount Amount";
                    break;
                case "-99":
                    msg = "Pos Error Invalid Pin Block";
                    break;
                case "-100":
                    msg = "Pos Error Invalid Key Serial Number";
                    break;
                case "-101":
                    msg = "Pos Error Invalid Authentication Data";
                    break;
                case "-102":
                    msg = "Pos Error Authentication Data Not Allowed";
                    break;
                case "-103":
                    msg = "Pos Error Invalid Birth Date";
                    break;
                case "-104":
                    msg = "Pos Error Invalid Identification Type";
                    break;
                case "-105":
                    msg = "Pos Error Invalid Track Data";
                    break;
                case "-106":
                    msg = "Pos Error Invalid Account Type";
                    break;
                case "-107":
                    msg = "Pos Error Invalid Sequence Number";
                    break;
                case "-108":
                    msg = "Pos Error Invalid Transaction ID";
                    break;
                case "-109":
                    msg = "Pos Error Invalid From Account Type";
                    break;
                case "-110":
                    msg = "Pos Error Invalid To Account Type";
                    break;
                case "-112":
                    msg = "Pos Error Invalid Auth Option";
                    break;
                case "-113":
                    msg = "Pos Error Transaction Failed";
                    break;
                case "-114":
                    msg = "Pos Error Invalid Incoming Eci";
                    break;
            }

            return msg;
        }

        public override bool SupportsPostProcessingEdits()
        {
            return false;
        }

        public Dictionary<String, String> ParseResponse(String Response)
        {
            Dictionary<String, String> ret = new Dictionary<string, string>();
            char splitchar = '\xFF';
            String[] lines = Response.Split('\n');
            lines[0] = lines[0].Replace("\",\"", splitchar.ToString()).Replace(",", "").Replace("\"", "");
            lines[1] = lines[1].Replace("\",\"", splitchar.ToString()).Replace(",", "").Replace("\"", "");
            String[] headers = lines[0].Split(splitchar);
            String[] values = lines[1].Split(splitchar);
            for (int i = 0; i < headers.Length && i < values.Length; i++)
                if (!ret.ContainsKey(headers[i]))
                    ret.Add(headers[i], values[i]);

            return ret;
        }
    }

}
