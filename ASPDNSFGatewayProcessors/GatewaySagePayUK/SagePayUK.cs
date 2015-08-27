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
using AspDotNetStorefrontGateways;


namespace AspDotNetStorefrontGateways.Processors
{

    /// <summary>
    /// Summary description for SagePayUK.
    /// </summary>
    public class SagePayUK : GatewayProcessor
    {
        private const String ProtocolVersion = "2.23";

        public SagePayUK() { }

        public override String CaptureOrder(Order o)
        {
            String result = AppLogic.ro_OK;
            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");

            o.CaptureTXCommand = "";
            o.CaptureTXResult = "";

            String TransID = o.AuthorizationPNREF;
            Decimal OrderTotal = o.OrderBalance;
            String AuthorizationCode = o.AuthorizationCode;

            o.CaptureTXCommand = TransID;

            if (TransID.Length == 0 || TransID == "0")
            {
                result = "Invalid or Empty Transaction ID";
            }
            else
            {
                try
                {
                    String[] TXInfo = TransID.Split('|');

                    ASCIIEncoding encoding = new ASCIIEncoding();
                    StringBuilder transactionCommand = new StringBuilder(4096);

                    transactionCommand.Append("VPSProtocol=" + ProtocolVersion);
                    transactionCommand.Append("&TxType=RELEASE");
                    transactionCommand.Append("&Vendor=" + AppLogic.AppConfig("SagePayUK.Vendor"));
                    transactionCommand.Append("&VendorTxCode=" + TXInfo[0]);
                    transactionCommand.Append("&VPSTxId=" + TXInfo[1]);
                    transactionCommand.Append("&SecurityKey=" + TXInfo[2]);
                    transactionCommand.Append("&TxAuthNo=" + AuthorizationCode);


                    byte[] data = encoding.GetBytes(transactionCommand.ToString());

                    // Prepare web request...
                    try
                    {
                        String AuthServer = CommonLogic.IIF(useLiveTransactions, AppLogic.AppConfig("SagePayUKURL.Live.Release"), AppLogic.AppConfig("SagePayUKURL.Test.Release"));
                        if (AppLogic.AppConfigBool("SagePayUK.UseSimulator"))
                        {
                            AuthServer = AppLogic.AppConfig("SagePayUKURL.Simulator.Release");
                        }

                        String rawResponseString = String.Empty;

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

                        try
                        {
                            myResponse = myRequest.GetResponse();
                            using (StreamReader sr = new StreamReader(myResponse.GetResponseStream()))
                            {
                                rawResponseString = sr.ReadToEnd();
                                sr.Close();
                            }
                            myResponse.Close();
                        }
                        catch { }

                        result = "ERROR: Error Calling Sage Pay Payment Gateway"; // This should get overwritten below.

                        String[] statusArray = rawResponseString.Split(new String[1] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries); // They use CRLF to seperate name-value pairs
                        for (int i = statusArray.GetLowerBound(0); i <= statusArray.GetUpperBound(0); i++)
                        {
                            String[] lasKeyPair = statusArray[i].Split('=');
                            switch (lasKeyPair[0].ToLowerInvariant())
                            {
                                case "status":
                                    /*
                                    “OK??Process executed without error. The DEFERRED payment was released.
                                    “MALFORMED??Input message was malformed ?normally will only occur during development. The StatusDetail (next field) will give more information
                                    “INVALID??Unable to authenticate you or find the transaction, or the data provided is invalid. If the Deferred payment was already released, an INVALID response is returned. See StatusDetail for more information.
                                    “ERROR??Only returned if there is a problem at SagePayUK.
                                    */
                                    if (lasKeyPair[1] == "OK")
                                    {
                                        result = AppLogic.ro_OK;
                                    }
                                    else
                                    {
                                        result = lasKeyPair[1];
                                    }
                                    break;
                                case "statusdetail":
                                    /*
                                    Human-readable text providing extra detail for the Status message.
                                    Always check StatusDetail if the Status is not OK
                                    */
                                    if (result != AppLogic.ro_OK)
                                    {
                                        result += ": " + lasKeyPair[1];
                                    }
                                    break;
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        result = "Error calling Sage Pay gateway. Msg=" + ex.Message;
                    }
                    o.CaptureTXResult = result;
                }
                catch (Exception ex)
                {
                    result = ex.Message;
                }
            }
            return result;
        }

        public override String VoidOrder(int OrderNumber)
        {
            String result = AppLogic.ro_OK;
            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");

            DB.ExecuteSQL("update orders set VoidTXCommand=NULL, VoidTXResult=NULL where OrderNumber=" + OrderNumber.ToString());
            String TransID = String.Empty;
            String AuthorizationCode = String.Empty;

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("select AuthorizationPNREF,AuthorizationCode from Orders  with (NOLOCK)  where OrderNumber=" + OrderNumber.ToString(), dbconn))
                {
                    if (rs.Read())
                    {
                        TransID = DB.RSField(rs, "AuthorizationPNREF");
                        AuthorizationCode = DB.RSField(rs, "AuthorizationCode");
                    }
                }
            }

            DB.ExecuteSQL("update orders set VoidTXCommand=" + DB.SQuote(TransID) + " where OrderNumber=" + OrderNumber.ToString());

            if (TransID.Length == 0 || TransID == "0")
            {
                result = "Invalid or Empty Transaction ID";
            }
            else
            {
                try
                {
                    String[] TXInfo = TransID.Split('|');

                    ASCIIEncoding encoding = new ASCIIEncoding();
                    StringBuilder transactionCommand = new StringBuilder(4096);

                    transactionCommand.Append("VPSProtocol=" + ProtocolVersion);
                    transactionCommand.Append("&TxType=ABORT");
                    transactionCommand.Append("&Vendor=" + AppLogic.AppConfig("SagePayUK.Vendor"));
                    transactionCommand.Append("&VendorTxCode=" + TXInfo[0]);
                    transactionCommand.Append("&VPSTxId=" + TXInfo[1]);
                    transactionCommand.Append("&SecurityKey=" + TXInfo[2]);
                    transactionCommand.Append("&TxAuthNo=" + AuthorizationCode);

                    byte[] data = encoding.GetBytes(transactionCommand.ToString());

                    // Prepare web request...
                    try
                    {
                        String AuthServer = CommonLogic.IIF(useLiveTransactions, AppLogic.AppConfig("SagePayUKURL.Live.Abort"), AppLogic.AppConfig("SagePayUKURL.Test.Abort"));
                        if (AppLogic.AppConfigBool("SagePayUK.UseSimulator"))
                        {
                            AuthServer = AppLogic.AppConfig("SagePayUKURL.Simulator.Abort");
                        }

                        String rawResponseString = String.Empty;

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

                        try
                        {
                            myResponse = myRequest.GetResponse();
                            using (StreamReader sr = new StreamReader(myResponse.GetResponseStream()))
                            {
                                rawResponseString = sr.ReadToEnd();
                                sr.Close();
                            }
                            myResponse.Close();
                        }
                        catch { }

                        result = "ERROR: Error Calling Sage Pay Payment Gateway"; // This should get overwritten below.

                        String[] statusArray = rawResponseString.Split(new String[1] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries); // They use CRLF to seperate name-value pairs
                        for (int i = statusArray.GetLowerBound(0); i <= statusArray.GetUpperBound(0); i++)
                        {
                            String[] lasKeyPair = statusArray[i].Split('=');
                            switch (lasKeyPair[0].ToLowerInvariant())
                            {
                                case "status":
                                    /*
                                    “OK??Process executed without error. The DEFERRED payment was aborted.
                                    “MALFORMED??Input message was malformed ?normally will only occur during development. The StatusDetail (next field) will give more information
                                    “INVALID??Unable to authenticate you or find the transaction, or the data provided is invalid. If the Deferred payment was already released, an INVALID response is returned. See StatusDetail for more information.
                                    “ERROR??Only returned if there is a problem at SagePayUK.
                                    */
                                    if (lasKeyPair[1] == "OK")
                                    {
                                        result = AppLogic.ro_OK;
                                    }
                                    else
                                    {
                                        result = lasKeyPair[1];
                                    }
                                    break;
                                case "statusdetail":
                                    /*
                                    Human-readable text providing extra detail for the Status message.
                                    Always check StatusDetail if the Status is not OK
                                    */
                                    if (result != AppLogic.ro_OK)
                                    {
                                        result += ": " + lasKeyPair[1];
                                    }
                                    break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        result = "Error calling Sage Pay gateway. Msg=" + ex.Message;
                    }
                    DB.ExecuteSQL("update orders set VoidTXResult=" + DB.SQuote(result) + " where OrderNumber=" + OrderNumber.ToString());

                }
                catch (Exception ex)
                {
                    result = ex.Message;
                }

            }
            return result;
        }

        // if RefundAmount == 0.0M, then then ENTIRE order amount will be refunded!
        public override String RefundOrder(int OriginalOrderNumber, int NewOrderNumber, decimal RefundAmount, String RefundReason, Address UseBillingAddress)
        {
            String result = AppLogic.ro_OK;
            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");

            DB.ExecuteSQL("update orders set RefundTXCommand=NULL, RefundTXResult=NULL where OrderNumber=" + OriginalOrderNumber.ToString());
            String TransID = String.Empty;
            String AuthorizationCode = String.Empty;
            Decimal OrderTotal = System.Decimal.Zero;

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("select * from orders   with (NOLOCK)  where OrderNumber=" + OriginalOrderNumber.ToString(), dbconn))
                {
                    if (rs.Read())
                    {
                        TransID = DB.RSField(rs, "AuthorizationPNREF");
                        AuthorizationCode = DB.RSField(rs, "AuthorizationCode");
                        OrderTotal = DB.RSFieldDecimal(rs, "OrderTotal");
                    }
                }
            }

            string vendorTxCode = OriginalOrderNumber.ToString() + "-" + System.Guid.NewGuid().ToString("N");

            DB.ExecuteSQL("update orders set RefundTXCommand=" + DB.SQuote(vendorTxCode + "|" + TransID) + " where OrderNumber=" + OriginalOrderNumber.ToString());

            if (TransID.Length == 0 || TransID == "0")
            {
                result = "Invalid or Empty Transaction ID";
            }
            else
            {
                try
                {
                    String[] TXInfo = TransID.Split('|');

                    ASCIIEncoding encoding = new ASCIIEncoding();
                    StringBuilder transactionCommand = new StringBuilder(4096);

                    transactionCommand.Append("VPSProtocol=" + ProtocolVersion);
                    transactionCommand.Append("&TxType=REFUND");
                    transactionCommand.Append("&Vendor=" + AppLogic.AppConfig("SagePayUK.Vendor"));
                    transactionCommand.Append("&VendorTxCode=" + vendorTxCode);
                    transactionCommand.Append("&Amount=" + Localization.CurrencyStringForGatewayWithoutExchangeRate(CommonLogic.IIF(RefundAmount == System.Decimal.Zero, OrderTotal, RefundAmount)));
                    transactionCommand.Append("&Currency=" + Localization.StoreCurrency());
                    if (RefundReason.Length != 0)
                    {
                        transactionCommand.Append("&Description=" + HttpContext.Current.Server.UrlEncode(RefundReason));
                    }
                    else
                    {
                        transactionCommand.Append("&Description=" + HttpContext.Current.Server.UrlEncode("Refund for Original Order " + OriginalOrderNumber.ToString()));
                    }
                   
                    transactionCommand.Append("&RelatedVPSTxId=" + TXInfo[1]);
                    transactionCommand.Append("&RelatedVendorTxCode=" + TXInfo[0]);
                    transactionCommand.Append("&RelatedSecurityKey=" + TXInfo[2]);
                    transactionCommand.Append("&RelatedTxAuthNo=" + AuthorizationCode);

                    byte[] data = encoding.GetBytes(transactionCommand.ToString());

                    // Prepare web request...
                    try
                    {
                        String AuthServer = CommonLogic.IIF(useLiveTransactions, AppLogic.AppConfig("SagePayUKURL.Live.Refund"), AppLogic.AppConfig("SagePayUKURL.Test.Refund"));
                        if (AppLogic.AppConfigBool("SagePayUK.UseSimulator"))
                        {
                            AuthServer = AppLogic.AppConfig("SagePayUKURL.Simulator.Refund");
                        }

                        String rawResponseString = String.Empty;

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

                        try
                        {
                            myResponse = myRequest.GetResponse();
                            using (StreamReader sr = new StreamReader(myResponse.GetResponseStream()))
                            {
                                rawResponseString = sr.ReadToEnd();
                                sr.Close();
                            }
                            myResponse.Close();
                        }
                        catch { }

                        result = "ERROR: Error Calling Sage Pay Payment Gateway"; // This should get overwritten below.

                        String VPSTxId = String.Empty;
                        String TxAuthNo = String.Empty;

                        String[] statusArray = rawResponseString.Split(new String[1] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries); // They use CRLF to seperate name-value pairs
                        for (int i = statusArray.GetLowerBound(0); i <= statusArray.GetUpperBound(0); i++)
                        {
                            String[] lasKeyPair = statusArray[i].Split('=');
                            switch (lasKeyPair[0].ToLowerInvariant())
                            {
                                case "status":
                                    /*
                                    “OK??The refund was authorised by the bank and funds will be returned to the customer.
                                    “NOTAUTHED??The refund was not authorised by SagePayUK or the acquiring bank. No funds will be returned to the card
                                    “MALFORMED??Input message was missing fields or badly formatted ?normally will only occur during development and vendor integration.
                                    “INVALID??Transaction was not registered because although the POST format was valid, some information supplied was invalid. E.g. incorrect vendor name or currency.
                                    “ERROR??A code-related error occurred which prevented the process from executing successfully.
                                    */
                                    if (lasKeyPair[1] == "OK")
                                    {
                                        result = AppLogic.ro_OK;
                                    }
                                    else
                                    {
                                        result = lasKeyPair[1];
                                    }
                                    break;
                                case "statusdetail":
                                    /*
                                    Human-readable text providing extra detail for the Status message.
                                    Always check StatusDetail if the Status is not OK
                                    */
                                    if (result != AppLogic.ro_OK)
                                    {
                                        result += ": " + lasKeyPair[1];
                                    }
                                    break;
                                case "vpstxid":
                                    /*
                                    SagePayUK ID to uniquely identify the Transaction on our system.
                                    */
                                    VPSTxId = lasKeyPair[1];
                                    break;
                                case "txauthno":
                                    /*
                                    The SagePayUK authorisation code (also called VPSAuthCode) for this transaction.
                                    */
                                    TxAuthNo = lasKeyPair[1];
                                    break;
                            }
                        }
                        if (result == AppLogic.ro_OK)
                        {
                            DB.ExecuteSQL("update orders set RefundTXResult=" + DB.SQuote(vendorTxCode + "|" + VPSTxId + "|" + TxAuthNo) + " where OrderNumber=" + OriginalOrderNumber.ToString());
                        }
                        else
                        {
                            DB.ExecuteSQL("update orders set RefundTXResult=" + DB.SQuote(result) + " where OrderNumber=" + OriginalOrderNumber.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        result = "Error calling Sage Pay gateway. Msg=" + ex.Message;
                    }
                }
                catch (Exception ex)
                {
                    result = ex.Message;
                }

            }
            return result;
        }

        // processes card in real time:
        public override String ProcessCard(int OrderNumber, int CustomerID, Decimal OrderTotal, bool useLiveTransactions, TransactionModeEnum TransactionMode, Address UseBillingAddress, String CardExtraCode, Address UseShippingAddress, String CAVV, String ECI, String XID, out String AVSResult, out String AuthorizationResult, out String AuthorizationCode, out String AuthorizationTransID, out String TransactionCommandOut, out String TransactionResponse)
        {
            String result = AppLogic.ro_OK;
            string countrycode = string.Empty;
           
            AuthorizationCode = String.Empty;
            AuthorizationResult = String.Empty;
            AVSResult = String.Empty;
            TransactionCommandOut = String.Empty;
            TransactionResponse = String.Empty;

            String signedPARes = String.Empty;

            CustomerSession cSession = new CustomerSession(CustomerID);

            String vendorTxCode = OrderNumber.ToString() + "-" + System.Guid.NewGuid().ToString("N");

            int CardTypeID = DB.GetSqlN("select CardTypeID N from CreditCardType where CardType = " + DB.SQuote(UseBillingAddress.CardType));
            bool Try3DSecure = CommonLogic.IntegerIsInIntegerList(CardTypeID, AppLogic.AppConfig("3DSECURE.CreditCardTypeIDs"));

            if (cSession["3Dsecure.PaRes"].Length != 0)
            {
                Try3DSecure = true; // If we have a PaRes, then we are doing 3D Secure, could be set up with SagePayUK.
                signedPARes = cSession["3Dsecure.PaRes"];
                // After grabbing it, clear out the session PaRes so it won't be re-used ever again.
                cSession["3Dsecure.PaRes"] = String.Empty;

                if (cSession["3DSecure.XID"].Length != 0)
                { // Reuse the original vendorTxCode
                    vendorTxCode = cSession["3DSecure.XID"];
                    cSession["3DSecure.XID"] = "";
                }
            }

            AuthorizationTransID = vendorTxCode + "||";
            
            ASCIIEncoding encoding = new ASCIIEncoding();
            StringBuilder transactionCommand = new StringBuilder(4096);

            transactionCommand.Append("VPSProtocol=" + ProtocolVersion);
            transactionCommand.Append("&TxType=" + CommonLogic.IIF(TransactionMode == TransactionModeEnum.auth, "DEFERRED", "PAYMENT"));
            transactionCommand.Append("&Vendor=" + AppLogic.AppConfig("SagePayUK.Vendor"));
            transactionCommand.Append("&VendorTxCode=" + vendorTxCode);
            transactionCommand.Append("&Amount=" + Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal));
            transactionCommand.Append("&Currency=" + Localization.StoreCurrency());
            transactionCommand.Append("&Description=" + HttpContext.Current.Server.UrlEncode(AppLogic.AppConfig("StoreName") + " Order " + OrderNumber.ToString()));
            transactionCommand.Append("&CardHolder=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.CardName));
            transactionCommand.Append("&CardNumber=" + UseBillingAddress.CardNumber);
            
            if (UseBillingAddress.CardStartDate != null && UseBillingAddress.CardStartDate.Length != 0 && UseBillingAddress.CardStartDate != "00")
            {
                //GFS - This was previously the way we were sending the start date over to SagePayUK, but due to format restrictions, we're now using the MMYY format.
                //transactionCommand.Append("&StartDate=" + UseBillingAddress.CardStartDate);
                transactionCommand.Append("&StartDate=" + UseBillingAddress.CardStartDate.Substring(0, 2) + UseBillingAddress.CardStartDate.Substring(4, 2));
            }
            transactionCommand.Append("&ExpiryDate=" + UseBillingAddress.CardExpirationMonth.PadLeft(2, '0') + UseBillingAddress.CardExpirationYear.ToString().Substring(2, 2));

            if (UseBillingAddress.CardIssueNumber.Length != 0)
            {
                transactionCommand.Append("&IssueNumber=" + UseBillingAddress.CardIssueNumber);
            }

            transactionCommand.Append("&CardType=" + FixCardType(UseBillingAddress.CardType));

            if (CardExtraCode.Trim().Length != 0)
            {
                transactionCommand.Append("&CV2=" + CardExtraCode);
                transactionCommand.Append("&ApplyAVSCV2=0"); // If AVS/CV2 enabled then check them. If rules apply, use rules.
            }

            transactionCommand.Append("&BillingSurname=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.LastName));
            transactionCommand.Append("&BillingFirstnames=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.FirstName));
            transactionCommand.Append("&BillingAddress1=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.Address1));
            if (UseBillingAddress.Address2.Length != 0)
            {
                transactionCommand.Append("&BillingAddress2=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.Address2));
            }
            transactionCommand.Append("&BillingCity=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.City));
            transactionCommand.Append("&BillingPostCode=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.Zip));
            countrycode = AppLogic.GetCountryTwoLetterISOCode(UseBillingAddress.Country);
            transactionCommand.Append("&BillingCountry=" + HttpContext.Current.Server.UrlEncode(countrycode));
            if (countrycode.Contains("US"))
            {
                transactionCommand.Append("&BillingState=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.State));
            }
            if (UseBillingAddress.Phone.Length != 0)
            {
                transactionCommand.Append("&BillingPhone=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.Phone));
            }

            if (UseShippingAddress != null)
            {
                transactionCommand.Append("&DeliverySurname=" + HttpContext.Current.Server.UrlEncode(UseShippingAddress.LastName));
                transactionCommand.Append("&DeliveryFirstnames=" + HttpContext.Current.Server.UrlEncode(UseShippingAddress.FirstName));
                transactionCommand.Append("&DeliveryAddress1=" + HttpContext.Current.Server.UrlEncode(UseShippingAddress.Address1));
                if (UseShippingAddress.Address2.Length != 0)
                {
                    transactionCommand.Append("&DeliveryAddress2=" + HttpContext.Current.Server.UrlEncode(UseShippingAddress.Address2));
                }
                transactionCommand.Append("&DeliveryCity=" + HttpContext.Current.Server.UrlEncode(UseShippingAddress.City));
                transactionCommand.Append("&DeliveryPostCode=" + HttpContext.Current.Server.UrlEncode(UseShippingAddress.Zip));
                countrycode = AppLogic.GetCountryTwoLetterISOCode(UseShippingAddress.Country);
                transactionCommand.Append("&DeliveryCountry=" + HttpContext.Current.Server.UrlEncode(countrycode));
                if (countrycode.Contains("US"))
                {
                    transactionCommand.Append("&DeliveryState=" + HttpContext.Current.Server.UrlEncode(UseShippingAddress.State));
                }
                if (UseShippingAddress.Phone.Length != 0)
                {
                    transactionCommand.Append("&DeliveryPhone=" + HttpContext.Current.Server.UrlEncode(UseShippingAddress.Phone));
                }
            }

            transactionCommand.Append("&ContactNumber=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.Phone));
            transactionCommand.Append("&CustomerName=" + HttpContext.Current.Server.UrlEncode((UseBillingAddress.FirstName + " " + UseBillingAddress.LastName).Trim()));
            transactionCommand.Append("&CustomerEMail=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.EMail));
            transactionCommand.Append("&ClientIPAddress=" + CommonLogic.CustomerIpAddress());

            String AuthServer = CommonLogic.IIF(useLiveTransactions, AppLogic.AppConfig("SagePayUKURL.Live.Purchase"), AppLogic.AppConfig("SagePayUKURL.Test.Purchase"));
            if (AppLogic.AppConfigBool("SagePayUK.UseSimulator"))
            {
                AuthServer = AppLogic.AppConfig("SagePayUKURL.Simulator.Purchase");
            }

            if (Try3DSecure)
            {
                if (signedPARes == String.Empty)
                {
                    transactionCommand.Append("&Apply3DSecure=1"); // 1 = Force 3D-Secure checks for this transaction only (if your account is 3D-enabled) and apply rules for authorisation.
                }
                else
                { // we are already enrolled and coming back with a 3D Secure transaction for round two
                    AuthServer = CommonLogic.IIF(useLiveTransactions, AppLogic.AppConfig("SagePayUKURL.Live.Callback"), AppLogic.AppConfig("SagePayUKURL.Test.Callback"));
                    if (AppLogic.AppConfigBool("SagePayUK.UseSimulator"))
                    {
                        AuthServer = AppLogic.AppConfig("SagePayUKURL.Simulator.Callback");
                    }
                    transactionCommand = new StringBuilder(4096); // start fresh
                    transactionCommand.Append("MD=" + cSession["3DSecure.MD"]);
                    transactionCommand.Append("&PARes=" + HttpContext.Current.Server.UrlEncode(signedPARes));
                }
            }

            byte[] data = encoding.GetBytes(transactionCommand.ToString());

            // Prepare web request...
            try
            {

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

                result = "ERROR: Error Calling Sage Pay Payment Gateway"; // This should get overwritten below.

                TransactionCommandOut = transactionCommand.ToString();

                String StatusDetail = String.Empty;
                String ThreeDSecureStatus = String.Empty;
                String RespCAVV = String.Empty;

                TransactionResponse = rawResponseString;
                String[] statusArray = rawResponseString.Split(new String[1] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries); // They use CRLF to seperate name-value pairs
                for (int i = statusArray.GetLowerBound(0); i <= statusArray.GetUpperBound(0); i++)
                {
                    String[] lasKeyPair = statusArray[i].Split(new char[] { '=' }, 2, StringSplitOptions.None);
                    switch (lasKeyPair[0].ToLowerInvariant())
                    {
                        case "status":
                            /*
                            “OK??The transaction was authorised by the bank and funds have been taken from the customer. 
                            “MALFORMED??Input message was missing fields or badly formatted ?normally will only occur during development and vendor integration.
                            “INVALID??Transaction was not registered because although the POST format was valid, some information supplied was invalid. E.g. incorrect vendor name or currency.
                            “ERROR??A code-related error occurred which prevented the process from executing successfully.
                            “NOTAUTHED??The transaction was not authorised by the acquiring bank. No funds could be taken from the card.
                            ”REJECTED??The VSP System rejected the transaction because of the rules you have set on your account. ** NEW **
                            ?DAUTH??Only returned if 3D-Authentication is available on your account AND the card and card issuer are part of the scheme. A Status of 3DAUTH only returns the StatusDetail, MD, PAReq, 3DSecureStatus and ACSURL fields. The other fields are returned with other Status codes only.
                            */
                            if (lasKeyPair[1] == "OK")
                            {
                                result = AppLogic.ro_OK;
                            }
                            else if (lasKeyPair[1] == "3DAUTH" && !Try3DSecure)
                            {//Override 3DS window if card is not applicable.
                                result = AppLogic.ro_OK;
                            }
                            else if (lasKeyPair[1] == "3DAUTH")
                            {
                                result = AppLogic.ro_3DSecure; // This is what triggers the 3D Secure IFRAME to be used.
                            }
                            else
                            {
                                result = lasKeyPair[1];
                            }
                            break;
                        case "statusdetail":
                        /*
                        Human-readable text providing extra detail for the Status message.
                        Always check StatusDetail if the Status is not OK
                        */
                            StatusDetail = lasKeyPair[1];
                            break;
                        case "vpstxid":
                        /*
                        SagePayUK ID to uniquely identify the Transaction on our system.
                        Not present when Status is 3DAUTH.
                        */
                            AuthorizationTransID = vendorTxCode + "|" + lasKeyPair[1];
                            break;
                        case "securitykey":
                        /*
                        Security key which VSP uses to generate an MD5 Hash to sign the transaction.
                        Not present when Status is 3DAUTH.
                        */
                            AuthorizationTransID += "|" + lasKeyPair[1];
                            break;
                        case "txauthno":
                        /*
                        The SagePayUK authorisation code (also called VPSAuthCode) for this transaction.
                        Only present if Status is OK.
                        */
                            AuthorizationCode = lasKeyPair[1];
                            break;
                        case "avscv2":
                        /*
                        Response from AVS and CV2 checks. Will be one of the following: “ALL MATCH? “SECURITY CODE MATCH ONLY? “ADDRESS MATCH ONLY? “NO DATA MATCHES?or “DATA NOT CHECKED?
                        Not present when Status is 3DAUTH.
                        */
                            AVSResult = lasKeyPair[1];
                            break;
                        case "3dsecurestatus":
                            ThreeDSecureStatus = lasKeyPair[1];
                            break;
                        case "cavv":
                        /*
                        The encoded result code from the 3D-Secure checks. Holds the Visa CAVV or the MasterCard UCAF depending on the card type used in the transaction.
                        Only present if the 3DSecureStatus field is OK AND the Status field is OK
                        */
                            RespCAVV = lasKeyPair[1];
                            break;
                        case "md":
                        /*
                        A unique reference for the 3D-Authentication attempt.
                        Only present if the Status field is 3DAUTH.
                        */
                            cSession["3DSecure.MD"] = lasKeyPair[1];
                            break;
                        case "acsurl":
                        /*
                        A fully qualified URL that points to the 3D-Authentication system at the Cardholder’s Issuing Bank.
                        Only present if the Status field is 3DAUTH.
                        */
                            cSession["3DSecure.ACSUrl"] = lasKeyPair[1];
                            break;
                        case "pareq":
                        /*
                        A Base64 encoded, encrypted message to be passed to the Issuing Bank as part of the 3D-Authentication.
                        Only present if the Status field is 3DAUTH.
                        */
                            cSession["3DSecure.PAReq"] = lasKeyPair[1];
                            break;
                    }
                }

                if (RespCAVV != String.Empty)
                { // 3D Secure successful
                    result = AppLogic.ro_OK;
                    AuthorizationResult = "CAVV: " + RespCAVV;
                    // encode it to store in the session, it will be decoded before being saved to the database
                    byte[] str = System.Text.Encoding.UTF8.GetBytes(ThreeDSecureStatus + ": " + StatusDetail); //Must Fully qualify this for VB
                    cSession["3DSecure.LookupResult"] = Convert.ToBase64String(str);
                } 
                else if (signedPARes != String.Empty)
                { // 3D Secure not successful since we didn't get a CAVV above.
                  // Depending on the SagePayUK processing rules, we might never get here.
                    if (result == AppLogic.ro_3DSecure || result == "OK")
                    {

                        /*  Possible values for ThreeDSecureStatus:
                        “NOTCHECKED?- No 3D Authentication was attempted for this transaction. Always returned if 3D-Secure is not active on your account.
                        “OK??The 3D-Authentication step completed successfully. If the Status field is 3DAUTH, this means the card is part of the scheme. If the Status field is OK too, then this indicates that the authorized transaction was also 3D-authenticated and a CAVV will be returned. Liability shift occurs. 
                        “NOAUTH??Returned with a Status of 3DAUTH. This means the card is not in the 3D-Secure scheme. 
                        “CANTAUTH?- Returned with a Status of 3DAUTH. This normally means the card Issuer is not part of the scheme. 
                        “NOTAUTHED??The cardholder failed to authenticate themselves with their Issuing Bank.
                        ”ATTEMPTONLY??The cardholder attempted to authenticate themselves but the process did not complete. A CAVV is returned anyway and liability shift occurs for Visa cards only. Check VSP Admin.
                        ”MALFORMED?”INVALID?”ERROR??These statuses indicate a problem with creating or receiving the 3D-Secure data. These should not occur on the live environment.
                        */
                        switch (ThreeDSecureStatus.ToUpperInvariant())
                        {
                            case "NOTCHECKED":
                            case "OK":
                            case "NOAUTH":
                            case "CANTAUTH":
                            case "ATTEMPTONLY":
                            case "NOTAUTHED":
                                result = AppLogic.ro_OK;
                                break;
                            default:
                                result = ThreeDSecureStatus + ": " + StatusDetail;
                                break;
                        }
                    }
                    // encode it to store in the session, it will be decoded before being saved to the database
                    byte[] str = System.Text.Encoding.UTF8.GetBytes(ThreeDSecureStatus + ": " + StatusDetail); //Have to fully qualify this for VB
                    cSession["3DSecure.LookupResult"] = Convert.ToBase64String(str);
                }

                if (result == AppLogic.ro_3DSecure)
                {
                    cSession["3DSecure.CustomerID"] = CustomerID.ToString();
                    cSession["3DSecure.OrderNumber"] = OrderNumber.ToString();
                    cSession["3DSecure.XID"] = vendorTxCode;
                    return result;  // Abort processing here and customer will be presented with 3D Secure IFRAME
                }

                if (result != AppLogic.ro_OK)
                {
                    switch (result.ToUpperInvariant())
                    {
                        case "ERROR":
                            result = "The transaction encountered an error. Please try again";
                            break;
                        case "INVALID":
                            result = "The card was not accepted. Please try again";
                            break;
                        case "NOTAUTHED":
                            result = "Your card was not authorized for that amount. Please try again";
                            break;
                        case "REJECTED":
                            result = "Your card was not not accepted. Please try again";
                            break;
                        default:
                            result += ": " + StatusDetail;
                            break;
                    }
                }
                else
                {
                    if (AuthorizationResult != String.Empty)
                    {
                        AuthorizationResult += System.Environment.NewLine;
                    }
                    AuthorizationResult += StatusDetail;
                }
            }
            catch (Exception ex)
            {
                result = "Error calling Sage Pay gateway. Msg=" + ex.Message;
            }

            return result;
        }

        private static string FixCardType(string CardType)
        {
            // fix card types for SagePayUK
            // valid values are: “VISA? ”MC? ”DELTA? “SOLO? “SWITCH? “UKE? “AMEX? “DC?or “JCB?
            switch (CardType.Replace(" ","").Trim().ToLowerInvariant())
            {
                case "diners":
                case "dinerscard":
                case "diner'scard":
                                                return "DC";
                case "mastercard":
                                                return "MC";
                case "switch/ukmaestro":
                case "ukmaestro":
                                                return "MAESTRO";
                case "delta/visadebit":
                case "visadebit":
                                                return "DELTA";
                case "visaelectron":
                                                return "UKE";
                case "americanexpress":
                                                return "AMEX";

            }
            return CardType.ToUpperInvariant().Trim();
        }

        public override String DisplayName(String LocaleSetting)
        {
            return "Sage Pay";
        }
    }

}
