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
using System.Text;
using System.Web;
using AspDotNetStorefrontCommon;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontGateways.Processors
{
    /// <summary>
    /// Summary description for IDeposit.
    /// </summary>
    public class IDeposit : GatewayProcessor
    {
        public IDeposit() { }

        public override String CaptureOrder(Order o)
        {
            String result = AppLogic.ro_OK;

            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");
            o.CaptureTXCommand = "";
            o.CaptureTXResult = "";
            String TransID = o.AuthorizationPNREF;
            int CustomerID = o.CustomerID;
            Decimal OrderTotal = o.OrderBalance;

            String Merchant_User_Name = GetIDepositAppConfig("USERNAME");
            
            ASCIIEncoding encoding = new ASCIIEncoding();
            StringBuilder transactionCommand = new StringBuilder(4096);

            transactionCommand.Append("Transaction_Type=PRE_AUTH_COMPLETE");
            transactionCommand.Append("&Merchant_User_Name=" + Merchant_User_Name);
            transactionCommand.Append("&Charge_Amount=" + Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal));
            transactionCommand.Append("&Transaction_Id=" + TransID);

            o.CaptureTXCommand = transactionCommand.ToString();

            if (TransID.Length == 0 || TransID == "0")
            {
                result = "Failed. Invalid or Empty Transaction ID";
            }
            else
            {
                try
                {
                    byte[] data = encoding.GetBytes(transactionCommand.ToString());

                    // Prepare web request...
                    String AuthServer = GetIDepositAppConfig("URL");
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
                    String rawResponseString = String.Empty;
                    try
                    {
                        myResponse = myRequest.GetResponse();
                        using (StreamReader sr = new StreamReader(myResponse.GetResponseStream()))
                        {
                            rawResponseString = sr.ReadToEnd();
                            // Close and clean up the StreamReader
                            sr.Close();
                        }
                        myResponse.Close();
                    }
                    catch
                    {
                        rawResponseString = "";
                    }

                    // rawResponseString now has gateway response
                    String authNum = CommonLogic.ExtractToken(rawResponseString, "<AuthorizationNumber>", "</AuthorizationNumber>");
                    String transactionID = CommonLogic.ExtractToken(rawResponseString, "<TransactionId>", "</TransactionId>");
                    String AVSStatus = CommonLogic.ExtractToken(rawResponseString, "<AVSStatus>", "</AVSStatus>");
                    String CVStatus = CommonLogic.ExtractToken(rawResponseString, "<CVStatus>", "</CVStatus>");
                    String returnCode = CommonLogic.ExtractToken(rawResponseString, "<ReturnCode>", "</ReturnCode>");
                    String responseStatus = CommonLogic.ExtractToken(rawResponseString, "<Status>", "</Status>");
                    String statusMsg = CommonLogic.ExtractToken(rawResponseString, "<StatusMessage>", "</StatusMessage>");

                    o.CaptureTXResult = rawResponseString;
                    

                    if (returnCode == "1")
                    {
                        result = AppLogic.ro_OK;
                    }
                    else if (returnCode == "0")
                    {
                        result = "Failed. " + statusMsg;
                    }
                    else
                    {
                        result = "Failed. Error calling iDeposit.net gateway. " + statusMsg;
                    }

                }
                catch
                {
                    result = "Failed. NO RESPONSE FROM GATEWAY!";
                }

            }
            return result;
        }

        public override String VoidOrder(int OrderNumber)
        {
            String result = AppLogic.ro_OK;

            DB.ExecuteSQL("update orders set VoidTXCommand=NULL, VoidTXResult=NULL where OrderNumber=" + OrderNumber.ToString());
            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");
            String TransID = String.Empty;
            int CustomerID = 0;
            Decimal OrderTotal = Decimal.Zero;

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("select AuthorizationPNREF,CustomerID,OrderTotal from Orders  with (NOLOCK)  where OrderNumber=" + OrderNumber.ToString(), dbconn))
                {
                    if (rs.Read())
                    {
                        TransID = DB.RSField(rs, "AuthorizationPNREF");
                        CustomerID = DB.RSFieldInt(rs, "CustomerID");
                        OrderTotal = DB.RSFieldDecimal(rs, "OrderTotal");
                    }
                }
            }

            String Merchant_User_Name = GetIDepositAppConfig("USERNAME");
            
            ASCIIEncoding encoding = new ASCIIEncoding();
            StringBuilder transactionCommand = new StringBuilder(4096);

            transactionCommand.Append("Transaction_Type=VOID");
            transactionCommand.Append("&Merchant_User_Name=" + Merchant_User_Name);
            transactionCommand.Append("&Charge_Amount=" + Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal));
            transactionCommand.Append("&Transaction_Id=" + TransID);

            DB.ExecuteSQL("update orders set VoidTXCommand=" + DB.SQuote(transactionCommand.ToString()) + " where OrderNumber=" + OrderNumber.ToString());

            if (TransID.Length == 0 || TransID == "0")
            {
                result = "Failed. Invalid or Empty Transaction ID";
            }
            else
            {
                try
                {
                    byte[] data = encoding.GetBytes(transactionCommand.ToString());

                    // Prepare web request...
                    String AuthServer = GetIDepositAppConfig("URL");
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
                    String rawResponseString = String.Empty;
                    try
                    {
                        myResponse = myRequest.GetResponse();
                        using (StreamReader sr = new StreamReader(myResponse.GetResponseStream()))
                        {
                            rawResponseString = sr.ReadToEnd();
                            // Close and clean up the StreamReader
                            sr.Close();
                        }
                        myResponse.Close();
                    }
                    catch
                    {
                        rawResponseString = "";
                    }

                    // rawResponseString now has gateway response
                    String authNum = CommonLogic.ExtractToken(rawResponseString, "<AuthorizationNumber>", "</AuthorizationNumber>");
                    String transactionID = CommonLogic.ExtractToken(rawResponseString, "<TransactionId>", "</TransactionId>");
                    String AVSStatus = CommonLogic.ExtractToken(rawResponseString, "<AVSStatus>", "</AVSStatus>");
                    String CVStatus = CommonLogic.ExtractToken(rawResponseString, "<CVStatus>", "</CVStatus>");
                    String returnCode = CommonLogic.ExtractToken(rawResponseString, "<ReturnCode>", "</ReturnCode>");
                    String responseStatus = CommonLogic.ExtractToken(rawResponseString, "<Status>", "</Status>");
                    String statusMsg = CommonLogic.ExtractToken(rawResponseString, "<StatusMessage>", "</StatusMessage>");

                    DB.ExecuteSQL("update orders set VoidTXResult=" + DB.SQuote(rawResponseString) + " where OrderNumber=" + OrderNumber.ToString());

                    if (returnCode == "1")
                    {
                        result = AppLogic.ro_OK;
                    }
                    else if (returnCode == "0")
                    {
                        result = "Failed. " + statusMsg;
                    }
                    else
                    {
                        result = "Failed. Error calling iDeposit.net gateway. " + statusMsg;
                    }

                }
                catch
                {
                    result = "Failed. NO RESPONSE FROM GATEWAY!";
                }

            }
            return result;
        }

        // if RefundAmount == 0.0M, then then ENTIRE order amount will be refunded!
        public override String RefundOrder(int OriginalOrderNumber, int NewOrderNumber, decimal RefundAmount, String RefundReason, Address UseBillingAddress)
        {

            String result = AppLogic.ro_OK;
            int OrderNumber = 0;
            if (NewOrderNumber == 0)
            {
                OrderNumber = OriginalOrderNumber;
            }
            else
            {
                OrderNumber = NewOrderNumber;
            }

            DB.ExecuteSQL("update orders set RefundTXCommand=NULL, RefundTXResult=NULL where OrderNumber=" + OriginalOrderNumber.ToString());
            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");
            String TransID = String.Empty;
            int CustomerID = 0;
            Decimal OrderTotal = Decimal.Zero;
            Decimal RefundTotal = Decimal.Zero;
            String CardNumber = String.Empty;
            String CardExpM = String.Empty;
            String CardExpY = String.Empty;

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("select * from Orders  with (NOLOCK)  where OrderNumber=" + OriginalOrderNumber.ToString(), dbconn))
                {
                    if (rs.Read())
                    {
                        TransID = DB.RSField(rs, "AuthorizationPNREF");
                        CustomerID = DB.RSFieldInt(rs, "CustomerID");
                        OrderTotal = DB.RSFieldDecimal(rs, "OrderTotal");
                        CardNumber = Security.UnmungeString(DB.RSField(rs, "CardNumber"), rs[AppLogic.AppConfig("OrdersCCSaltField")].ToString());
                        if (CardNumber.StartsWith(Security.ro_DecryptFailedPrefix, StringComparison.InvariantCultureIgnoreCase))
                        {
                            CardNumber = DB.RSField(rs, "CardNumber");
                        }
                        CardExpM = DB.RSField(rs, "CardExpirationMonth");
                        CardExpY = DB.RSField(rs, "CardExpirationYear").Substring(2, 2);
                        //For full refunds a value of zero is passed, so use the order total
                        RefundTotal = CommonLogic.IIF(RefundAmount > 0, RefundAmount, OrderTotal);
                    }
                }
            }

            if (CardNumber == AppLogic.ro_CCNotStoredString || CardNumber.Length == 0)
            {
                return "Failed. To process Refunds with iDeposit using the store front, you must store the credit card numbers in the database. See AppConfig variable StoreCCInDB.";
            }

            String Merchant_User_Name = GetIDepositAppConfig("USERNAME");
            String Merchant_Password = GetIDepositAppConfig("PASSWORD");
            String Clerk_Id = GetIDepositAppConfig("CLERKID");

            ASCIIEncoding encoding = new ASCIIEncoding();
            StringBuilder transactionCommand = new StringBuilder(4096);

            transactionCommand.Append("Transaction_Type=RETURN");
            transactionCommand.Append("&Merchant_User_Name=" + Merchant_User_Name);
            transactionCommand.Append("&Merchant_Password=" + Merchant_Password);
            transactionCommand.Append("&Charge_Amount=" + Localization.CurrencyStringForGatewayWithoutExchangeRate(RefundTotal));
            transactionCommand.Append("&Transaction_Id=" + TransID);
            transactionCommand.Append("&Credit_Card_Number=" + CardNumber);
            transactionCommand.Append("&Credit_Card_Exp_Date=" + CardExpM + CardExpY);
            transactionCommand.Append("&Tracking_Number=" + OrderNumber.ToString());
            transactionCommand.Append("&Clerk_Id=" + HttpContext.Current.Server.UrlEncode(Clerk_Id));
            transactionCommand.Append("&Station_Id=" + CommonLogic.CustomerIpAddress());
            transactionCommand.Append("&Comments=" + HttpContext.Current.Server.UrlEncode(AppLogic.AppConfig("StoreName") + " Order " + OrderNumber.ToString() + " Refund Reason: " + RefundReason));

            String PasswordToken = "Merchant_Password=" + Merchant_Password;
            String PasswordTokenReplacement = "Merchant_Password=" + "*".PadLeft(Merchant_Password.Length, '*');
            String CardToken = String.Format("Credit_Card_Number={0}", CardNumber);
            String CardTokenReplacement = String.Format("Credit_Card_Number={0}", "x".PadLeft(CardNumber.Length, 'x'));
            DB.ExecuteSQL("update orders set RefundTXCommand=" + DB.SQuote(transactionCommand.ToString().Replace(CardToken, CardTokenReplacement).Replace(PasswordToken, PasswordTokenReplacement)) + " where OrderNumber=" + OriginalOrderNumber.ToString());

            if (TransID.Length == 0 || TransID == "0")
            {
                result = "Failed. Invalid or Empty Transaction ID";
            }
            else
            {
                try
                {
                    byte[] data = encoding.GetBytes(transactionCommand.ToString());

                    // Prepare web request...
                    String AuthServer = GetIDepositAppConfig("URL");
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
                    String rawResponseString = String.Empty;
                    try
                    {
                        myResponse = myRequest.GetResponse();
                        using (StreamReader sr = new StreamReader(myResponse.GetResponseStream()))
                        {
                            rawResponseString = sr.ReadToEnd();
                            // Close and clean up the StreamReader
                            sr.Close();
                        }
                        myResponse.Close();
                    }
                    catch
                    {
                        rawResponseString = "";
                    }

                    // rawResponseString now has gateway response
                    String authNum = CommonLogic.ExtractToken(rawResponseString, "<AuthorizationNumber>", "</AuthorizationNumber>");
                    String transactionID = CommonLogic.ExtractToken(rawResponseString, "<TransactionId>", "</TransactionId>");
                    String AVSStatus = CommonLogic.ExtractToken(rawResponseString, "<AVSStatus>", "</AVSStatus>");
                    String CVStatus = CommonLogic.ExtractToken(rawResponseString, "<CVStatus>", "</CVStatus>");
                    String returnCode = CommonLogic.ExtractToken(rawResponseString, "<ReturnCode>", "</ReturnCode>");
                    String responseStatus = CommonLogic.ExtractToken(rawResponseString, "<Status>", "</Status>");
                    String statusMsg = CommonLogic.ExtractToken(rawResponseString, "<StatusMessage>", "</StatusMessage>");

                    DB.ExecuteSQL("update orders set RefundTXResult=" + DB.SQuote(rawResponseString) + " where OrderNumber=" + OrderNumber.ToString());

                    if (returnCode == "1")
                    {
                        result = AppLogic.ro_OK;
                    }
                    else if (returnCode == "0")
                    {
                        result = "Failed. " + statusMsg;
                    }
                    else
                    {
                        result = "Failed. Error calling iDeposit.net gateway. " + statusMsg;
                    }

                }
                catch
                {
                    result = "Failed. NO RESPONSE FROM GATEWAY!";
                }

            }
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

            String Merchant_User_Name = GetIDepositAppConfig("USERNAME");
            String Clerk_Id = GetIDepositAppConfig("CLERKID");


            ASCIIEncoding encoding = new ASCIIEncoding();
            StringBuilder transactionCommand = new StringBuilder(4096);

            transactionCommand.Append("Transaction_Type=" + CommonLogic.IIF(TransactionMode == TransactionModeEnum.auth, "PRE_AUTH", "SALE"));

            transactionCommand.Append("&Merchant_User_Name=" + Merchant_User_Name);
            transactionCommand.Append("&Comments=" + HttpContext.Current.Server.UrlEncode(AppLogic.AppConfig("StoreName") + " Order " + OrderNumber.ToString()));
            transactionCommand.Append("&Charge_Amount=" + Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal));
            transactionCommand.Append("&Credit_Card_Type=" + GetCardTypeFieldValue(UseBillingAddress.CardType));
            transactionCommand.Append("&Credit_Card_Number=" + UseBillingAddress.CardNumber);
            if (CardExtraCode.Length != 0)
            {
                transactionCommand.Append("&CV_Security_Code=" + CardExtraCode.Trim());
            }
            transactionCommand.Append("&Credit_Card_Exp_Date=" + UseBillingAddress.CardExpirationMonth.PadLeft(2, '0') + UseBillingAddress.CardExpirationYear.ToString().Substring(2, 2));
            transactionCommand.Append("&Tracking_Number=" + OrderNumber.ToString());
            transactionCommand.Append("&CardHolder_Name=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.CardName));
            transactionCommand.Append("&AVS_Street=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.Address1));
            transactionCommand.Append("&AVS_Zip_Code=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.Zip));
            transactionCommand.Append("&Clerk_Id=" + HttpContext.Current.Server.UrlEncode(Clerk_Id));
            transactionCommand.Append("&Station_Id=" + CommonLogic.CustomerIpAddress());


            byte[] data = encoding.GetBytes(transactionCommand.ToString());

            // Prepare web request...
            try
            {
                String AuthServer = GetIDepositAppConfig("URL");
                String rawResponseString = String.Empty;

                int MaxTries = AppLogic.AppConfigUSInt("GatewayRetries") + 1;
                int CurrentTry = 0;
                bool CallSuccessful = false;
                do
                {
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

                    CurrentTry++;
                    try
                    {
                        myResponse = myRequest.GetResponse();
                        using (StreamReader sr = new StreamReader(myResponse.GetResponseStream()))
                        {
                            rawResponseString = sr.ReadToEnd();
                            sr.Close();
                        }
                        myResponse.Close();
                        CallSuccessful = true;
                    }
                    catch
                    {
                        CallSuccessful = false;
                    }
                }
                while (!CallSuccessful && CurrentTry < MaxTries);


                // rawResponseString now has gateway response
                TransactionResponse = rawResponseString;

                String authNum = CommonLogic.ExtractToken(rawResponseString, "<AuthorizationNumber>", "</AuthorizationNumber>");
                String transactionID = CommonLogic.ExtractToken(rawResponseString, "<TransactionId>", "</TransactionId>");
                String AVSStatus = CommonLogic.ExtractToken(rawResponseString, "<AVSStatus>", "</AVSStatus>");
                String CVStatus = CommonLogic.ExtractToken(rawResponseString, "<CVStatus>", "</CVStatus>");
                String returnCode = CommonLogic.ExtractToken(rawResponseString, "<ReturnCode>", "</ReturnCode>");
                String responseStatus = CommonLogic.ExtractToken(rawResponseString, "<Status>", "</Status>");
                String statusMsg = CommonLogic.ExtractToken(rawResponseString, "<StatusMessage>", "</StatusMessage>");

                AuthorizationTransID = transactionID;
                AuthorizationCode = authNum;
                AVSResult = AVSStatus;
                if (CVStatus.Length > 0 && CardExtraCode.Length != 0)
                {
                    AVSResult += ", CV Result: " + CVStatus;
                }
                AuthorizationResult = responseStatus;
                
                TransactionCommandOut = transactionCommand.ToString();


                if (returnCode == "1")
                {
                    result = AppLogic.ro_OK;
                }
                else if (returnCode == "0")
                {
                    result = statusMsg;
                }
                else
                {
                    result = "Error calling iDeposit.net gateway. Please retry your order in a few minutes or select another checkout payment option.";
                }
            }
            catch
            {
                result = "Error calling iDeposit.net gateway. Please retry your order in a few minutes or select another checkout payment option.";
            }
            return result;
        }

        static String GetCardTypeFieldValue(String CardType)
        {
            switch (CardType.ToUpperInvariant())
            {
                case "VISA":
                    return "MC_CARD_VISA";
                case "MASTERCARD":
                case "EUROCARD":
                    return "MC_CARD_MC";
                case "AMEX":
                case "AMERICANEXPRESS":
                case "AMERICAN EXPRESS":
                    return "MC_CARD_AMEX";
                case "DISCOVER":
                    return "MC_CARD_DISC";
                default:
                    // unknown card type
                    return "";
            }
        }
        static String GetIDepositAppConfig(String config)
        {
            String ConfigName = "IDEPOSIT_";
            if (AppLogic.AppConfigBool("UseLiveTransactions"))
            {
                ConfigName += "LIVE_";
            }
            else
            {
                ConfigName += "TEST_";
            }

            ConfigName += config;

            String val = AppLogic.AppConfig(ConfigName);
            if (val.Trim().Equals("REGISTRY", StringComparison.InvariantCultureIgnoreCase))
            {
                WindowsRegistry reg = new WindowsRegistry(AppLogic.AppConfig("EncryptKey.RegistryLocation"));
                val = reg.Read(ConfigName);
                reg = null;
            }
            return val;
        }

        public override bool RequiresCCForFurtherProcessing()
        {
            return true;
        }
    }
}
