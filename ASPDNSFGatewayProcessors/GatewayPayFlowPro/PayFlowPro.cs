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
using System.Xml;
using System.Xml.Serialization;
using System.Threading;
using System.Globalization;
using System.Text.RegularExpressions;
using AspDotNetStorefrontCore;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace AspDotNetStorefrontGateways.Processors
{
    /// <summary>
    /// Summary description for PayFlowPro.
    /// </summary>
    public class PayFlowPro : GatewayProcessor
    {
        public const String BN = "AspDotNetStoreFr_Cart_PRO2"; // Do not change this line or your paypal website calls may not work!

        public override string AdministratorSetupPrompt
        {
            get
            {
                return "(also enables PayPal Express Checkout) - See <a href='http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=paypalpayflowpro&type=manual' target='_blank'>Manual</a>.";
            }
        }

        public PayFlowPro() { }

        public override String CaptureOrder(Order o)
        {
            String result = String.Empty;

            result = AppLogic.ro_OK;

            o.CaptureTXCommand = "";
            o.CaptureTXResult = "";

            String TransID = Regex.Match(o.AuthorizationPNREF, "(?<=AUTH=)[0-9A-Z]+", RegexOptions.Compiled).ToString();

            if (String.IsNullOrEmpty(TransID) && !o.AuthorizationPNREF.Contains("="))
            { // We probably have an old Verisign order.
                TransID = o.AuthorizationPNREF;
            }

            Decimal OrderTotal = o.OrderBalance;

            StringBuilder transactionCommand = new StringBuilder(4096);

            transactionCommand.Append("TRXTYPE=D&TENDER=C&COMMENT1=" + CleanForNVP(AppLogic.AppConfig("StoreName") + " Capture"));
            transactionCommand.Append("&PWD=" + AppLogic.AppConfig("PayFlowPro.PWD"));
            transactionCommand.Append("&USER=" + AppLogic.AppConfig("PayFlowPro.USER"));
            transactionCommand.Append("&VENDOR=" + AppLogic.AppConfig("PayFlowPro.VENDOR"));
            transactionCommand.Append("&PARTNER=" + AppLogic.AppConfig("PayFlowPro.PARTNER"));
            transactionCommand.Append("&ORIGID=" + TransID);

            if (OrderTotal != System.Decimal.Zero)
            {
                // amount could have changed by admin user, so capture the current Order Total from the db:
                transactionCommand.Append("&CURRENCY=" + AppLogic.AppConfig("Localization.StoreCurrency"));
                transactionCommand.Append("&AMT=" + Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal));
            }

            string tmpTX = transactionCommand.ToString();
            // we want to zap the password from the being logged to the DB
            tmpTX = tmpTX.Replace(CommonLogic.ExtractToken(tmpTX, "PWD=", "&"), "****");
            o.CaptureTXCommand = tmpTX;

            try
            {
                int TO = AppLogic.AppConfigUSInt("PayFlowPro.Timeout");
                if (TO == 0)
                {
                    TO = 45;
                }
                String AuthServer = CommonLogic.IIF(AppLogic.AppConfigBool("UseLiveTransactions"), AppLogic.AppConfig("PayFlowPro.LiveURL"), AppLogic.AppConfig("PayFlowPro.TestURL"));

                String RequestID = System.Guid.NewGuid().ToString("N");
                ASCIIEncoding encoding = new ASCIIEncoding();
                byte[] data = encoding.GetBytes(transactionCommand.ToString());


                String rawResponseString = String.Empty;

                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(AuthServer);
                myRequest.Method = "POST";
                myRequest.ContentType = "text/namevalue";
                myRequest.Headers.Add("X-VPS-REQUEST-ID", RequestID);
                myRequest.Headers.Add("X-VPS-CLIENT-TIMEOUT", TO.ToString());
                myRequest.Headers.Add("X-VPS-VIT-CLIENT-CERTIFICATION-ID", "aspdotnetstorefront");
                myRequest.ContentLength = data.Length;
                Stream newStream = myRequest.GetRequestStream();
                // Send the data.
                newStream.Write(data, 0, data.Length);
                newStream.Close();
                // get the response
                WebResponse myResponse;

                String replyCode = String.Empty;
                String replyMsg = String.Empty;

                String PNREF = String.Empty;
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
                catch
                {
                    replyMsg = "ERROR CALLING GATEWAY!";
                }

                String[] statusArray = rawResponseString.Split('&');
                for (int i = statusArray.GetLowerBound(0); i <= statusArray.GetUpperBound(0); i++)
                {
                    String[] lasKeyPair = statusArray[i].Split('=');
                    switch (lasKeyPair[0].ToLowerInvariant())
                    {
                        case "result":
                            replyCode = lasKeyPair[1];
                            break;
                        case "pnref":
                            PNREF = lasKeyPair[1];
                            break;
                        case "respmsg":
                            replyMsg = lasKeyPair[1];
                            break;
                    }
                }

                o.CaptureTXResult = rawResponseString;

                if (replyCode == "0")
                {
                    result = AppLogic.ro_OK;
                    o.AuthorizationPNREF = o.AuthorizationPNREF + "|CAPTURE=" + PNREF;
                }
                else
                {
                    result = replyCode + ": " + replyMsg;
                }
            }
            catch
            {
                result = "NO RESPONSE FROM GATEWAY!";
            }

            return result;
        }

        public override String VoidOrder(int OrderNumber)
        {
            String result = String.Empty;

            result = AppLogic.ro_OK;

            DB.ExecuteSQL("update orders set VoidTXCommand=NULL, VoidTXResult=NULL where OrderNumber=" + OrderNumber.ToString());
            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");
            String TransID = String.Empty;

            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS("select * from orders   with (NOLOCK)  where OrderNumber=" + OrderNumber.ToString(), conn))
                {
                    if (rs.Read())
                    {
                        TransID = Regex.Match(DB.RSField(rs, "AuthorizationPNREF"), "(?<=AUTH=)[0-9A-Z]+", RegexOptions.Compiled).ToString();
                        if (String.IsNullOrEmpty(TransID) && !DB.RSField(rs, "AuthorizationPNREF").Contains("="))
                        { // We probably have an old Verisign order.
                            TransID = DB.RSField(rs, "AuthorizationPNREF");
                        }
                    }
                }
            }

            StringBuilder transactionCommand = new StringBuilder(4096);

            transactionCommand.Append("TRXTYPE=V&TENDER=C&COMMENT1=" + CleanForNVP(AppLogic.AppConfig("StoreName") + " Void"));
            transactionCommand.Append("&PWD=" + AppLogic.AppConfig("PayFlowPro.PWD"));
            transactionCommand.Append("&USER=" + AppLogic.AppConfig("PayFlowPro.USER"));
            transactionCommand.Append("&VENDOR=" + AppLogic.AppConfig("PayFlowPro.VENDOR"));
            transactionCommand.Append("&PARTNER=" + AppLogic.AppConfig("PayFlowPro.PARTNER"));
            transactionCommand.Append("&ORIGID=" + TransID);

            string tmpTX = transactionCommand.ToString();
            // we want to zap the password from the being logged to the DB
            tmpTX = tmpTX.Replace(CommonLogic.ExtractToken(tmpTX, "PWD=", "&"), "****");
            DB.ExecuteSQL("update orders set VoidTXCommand=" + DB.SQuote(tmpTX) + " where OrderNumber=" + OrderNumber.ToString());

            if (TransID.Length == 0 || TransID == "0")
            {
                result = "Invalid or Empty Transaction ID";
            }
            else
            {
                try
                {
                    int TO = AppLogic.AppConfigUSInt("PayFlowPro.Timeout");
                    if (TO == 0)
                    {
                        TO = 45;
                    }
                    String AuthServer = CommonLogic.IIF(AppLogic.AppConfigBool("UseLiveTransactions"), AppLogic.AppConfig("PayFlowPro.LiveURL"), AppLogic.AppConfig("PayFlowPro.TestURL"));

                    String RequestID = System.Guid.NewGuid().ToString("N");
                    ASCIIEncoding encoding = new ASCIIEncoding();
                    byte[] data = encoding.GetBytes(transactionCommand.ToString());

                    String rawResponseString = String.Empty;

                    HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(AuthServer);
                    myRequest.Method = "POST";
                    myRequest.ContentType = "text/namevalue";
                    myRequest.Headers.Add("X-VPS-REQUEST-ID", RequestID);
                    myRequest.Headers.Add("X-VPS-CLIENT-TIMEOUT", TO.ToString());
                    myRequest.Headers.Add("X-VPS-VIT-CLIENT-CERTIFICATION-ID", "aspdotnetstorefront");
                    myRequest.ContentLength = data.Length;
                    Stream newStream = myRequest.GetRequestStream();
                    // Send the data.
                    newStream.Write(data, 0, data.Length);
                    newStream.Close();
                    // get the response
                    WebResponse myResponse;

                    String replyCode = String.Empty;
                    String replyMsg = String.Empty;
                    String PNREF = String.Empty;
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
                    catch
                    {
                        replyMsg = "ERROR CALLING GATEWAY!";
                    }
                    String[] statusArray = rawResponseString.Split('&');
                    for (int i = statusArray.GetLowerBound(0); i <= statusArray.GetUpperBound(0); i++)
                    {
                        String[] lasKeyPair = statusArray[i].Split('=');
                        switch (lasKeyPair[0].ToLowerInvariant())
                        {
                            case "result":
                                replyCode = lasKeyPair[1];
                                break;
                            case "pnref":
                                PNREF = lasKeyPair[1];
                                break;
                            case "respmsg":
                                replyMsg = lasKeyPair[1];
                                break;
                        }
                    }

                    DB.ExecuteSQL("update orders set VoidTXResult=" + DB.SQuote(rawResponseString) + " where OrderNumber=" + OrderNumber.ToString());
                    if (replyCode == "0")
                    {
                        result = AppLogic.ro_OK;
                    }
                    else
                    {
                        result = replyCode + ": " + replyMsg;
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
            String result = String.Empty;

            result = AppLogic.ro_OK;

            DB.ExecuteSQL("update orders set RefundTXCommand=NULL, RefundTXResult=NULL where OrderNumber=" + OriginalOrderNumber.ToString());
            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");
            String TransID = String.Empty;
            Decimal OrderTotal = System.Decimal.Zero;

            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS("select * from orders   with (NOLOCK)  where OrderNumber=" + OriginalOrderNumber.ToString(), conn))
                {
                    if (rs.Read())
                    {
                        TransID = Regex.Match(DB.RSField(rs, "AuthorizationPNREF"), "(?<=CAPTURE=)[0-9A-Z]+", RegexOptions.Compiled).ToString();
                        if (String.IsNullOrEmpty(TransID) && !DB.RSField(rs, "AuthorizationPNREF").Contains("="))
                        { // We probably have an old Verisign order.
                            TransID = DB.RSField(rs, "AuthorizationPNREF");
                        }
                        OrderTotal = DB.RSFieldDecimal(rs, "OrderTotal");
                    }
                }
            }

            StringBuilder transactionCommand = new StringBuilder(4096);

            transactionCommand.Append("TRXTYPE=C&TENDER=C&COMMENT1=" + CleanForNVP(AppLogic.AppConfig("StoreName") + " Refund"));
            transactionCommand.Append("&PWD=" + AppLogic.AppConfig("PayFlowPro.PWD"));
            transactionCommand.Append("&USER=" + AppLogic.AppConfig("PayFlowPro.USER"));
            transactionCommand.Append("&VENDOR=" + AppLogic.AppConfig("PayFlowPro.VENDOR"));
            transactionCommand.Append("&PARTNER=" + AppLogic.AppConfig("PayFlowPro.PARTNER"));
            transactionCommand.Append("&ORIGID=" + TransID);
            if (RefundAmount != System.Decimal.Zero)
            {
                transactionCommand.Append("&AMT=" + Localization.CurrencyStringForGatewayWithoutExchangeRate(RefundAmount));
            }
            string tmpTX = transactionCommand.ToString();
            // we want to zap the password from the being logged to the DB
            tmpTX = tmpTX.Replace(CommonLogic.ExtractToken(tmpTX, "PWD=", "&"), "****");

            DB.ExecuteSQL("update orders set RefundTXCommand=" + DB.SQuote(tmpTX) + " where OrderNumber=" + OriginalOrderNumber.ToString());

            if (TransID.Length == 0 || TransID == "0")
            {
                result = "Invalid or Empty Transaction ID";
            }
            else
            {
                try
                {
                    int TO = AppLogic.AppConfigUSInt("PayFlowPro.Timeout");
                    if (TO == 0)
                    {
                        TO = 45;
                    }
                    String AuthServer = CommonLogic.IIF(AppLogic.AppConfigBool("UseLiveTransactions"), AppLogic.AppConfig("PayFlowPro.LiveURL"), AppLogic.AppConfig("PayFlowPro.TestURL"));

                    String RequestID = System.Guid.NewGuid().ToString("N");
                    ASCIIEncoding encoding = new ASCIIEncoding();
                    byte[] data = encoding.GetBytes(transactionCommand.ToString());


                    String rawResponseString = String.Empty;

                    HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(AuthServer);
                    myRequest.Method = "POST";
                    myRequest.ContentType = "text/namevalue";
                    myRequest.Headers.Add("X-VPS-REQUEST-ID", RequestID);
                    myRequest.Headers.Add("X-VPS-CLIENT-TIMEOUT", TO.ToString());
                    myRequest.Headers.Add("X-VPS-VIT-CLIENT-CERTIFICATION-ID", "aspdotnetstorefront");
                    myRequest.ContentLength = data.Length;
                    Stream newStream = myRequest.GetRequestStream();
                    // Send the data.
                    newStream.Write(data, 0, data.Length);
                    newStream.Close();
                    // get the response
                    WebResponse myResponse;

                    String replyCode = String.Empty;
                    String replyMsg = String.Empty;
                    String PNREF = String.Empty;
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
                    catch
                    {
                        replyMsg = "ERROR CALLING GATEWAY!";
                    }

                    String[] statusArray = rawResponseString.Split('&');
                    for (int i = statusArray.GetLowerBound(0); i <= statusArray.GetUpperBound(0); i++)
                    {
                        String[] lasKeyPair = statusArray[i].Split('=');
                        switch (lasKeyPair[0].ToLowerInvariant())
                        {
                            case "result":
                                replyCode = lasKeyPair[1];
                                break;
                            case "pnref":
                                PNREF = lasKeyPair[1];
                                break;
                            case "respmsg":
                                replyMsg = lasKeyPair[1];
                                break;
                        }
                    }

                    DB.ExecuteSQL("update orders set RefundTXResult=" + DB.SQuote(rawResponseString) + " where OrderNumber=" + OriginalOrderNumber.ToString());
                    if (replyCode == "0")
                    {
                        result = AppLogic.ro_OK;
                        DB.ExecuteSQL("update orders set AuthorizationPNREF=AuthorizationPNREF+'|REFUND=" + PNREF + "' where OrderNumber=" + OriginalOrderNumber.ToString());
                    }
                    else
                    {
                        result = replyCode + ": " + replyMsg;
                    }
                }
                catch
                {
                    result = "NO RESPONSE FROM GATEWAY!";
                }
            }

            return result;
        }

        // processes card in real time:
        public override String ProcessCard(int OrderNumber, int CustomerID, Decimal OrderTotal, bool useLiveTransactions, TransactionModeEnum TransactionMode, Address UseBillingAddress, String CardExtraCode, Address UseShippingAddress, String CAVV, String ECI, String XID, out String AVSResult, out String AuthorizationResult, out String AuthorizationCode, out String AuthorizationTransID, out String TransactionCommandOut, out String TransactionResponse)
        {
            String result = String.Empty;
            AuthorizationCode = String.Empty;
            AuthorizationResult = String.Empty;
            AuthorizationTransID = String.Empty;
            AVSResult = String.Empty;
            TransactionCommandOut = String.Empty;
            TransactionResponse = String.Empty;

            StringBuilder transactionCommand = new StringBuilder(4096);
            String rawResponseString = String.Empty;
            String replyCode = String.Empty;
            String responseCode = String.Empty;
            String authResponse = String.Empty;
            String approvalCode = String.Empty;
            String orderTotalString;

            orderTotalString = Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal);

            transactionCommand.Append("TRXTYPE=" + CommonLogic.IIF(TransactionMode == TransactionModeEnum.auth, "A", "S") + "&TENDER=C&COMMENT1=" + CleanForNVP("Order " + OrderNumber) + "&COMMENT2=" + CleanForNVP("CustomerID " + CustomerID.ToString()));
            transactionCommand.Append("&PWD=" + AppLogic.AppConfig("PayFlowPro.PWD"));
            transactionCommand.Append("&USER=" + AppLogic.AppConfig("PayFlowPro.USER"));
            transactionCommand.Append("&VENDOR=" + AppLogic.AppConfig("PayFlowPro.VENDOR"));
            transactionCommand.Append("&PARTNER=" + AppLogic.AppConfig("PayFlowPro.PARTNER"));

            //set the amount 

            transactionCommand.Append("&CURRENCY=" + AppLogic.AppConfig("Localization.StoreCurrency"));
            transactionCommand.Append("&AMT=" + orderTotalString);
            transactionCommand.Append("&INVNUM=" + OrderNumber.ToString());

            if (AppLogic.AppConfig("PayFlowPro.PARTNER").Equals("paypaluk", StringComparison.InvariantCultureIgnoreCase))
            {
                transactionCommand.Append("&BUTTONSOURCE=" + PayFlowProController.BN + "DP_UK");
            }
            else if (AppLogic.AppConfig("PayFlowPro.PARTNER").Equals("paypal", StringComparison.InvariantCultureIgnoreCase))
            {
                transactionCommand.Append("&BUTTONSOURCE=" + PayFlowProController.BN + "DP_US");
            }
            else
            {
                transactionCommand.Append("&BUTTONSOURCE=" + PayFlowProController.BN + "DP");
            }

            transactionCommand.Append("&ACCT=" + UseBillingAddress.CardNumber);
            transactionCommand.Append("&EXPDATE=" + UseBillingAddress.CardExpirationMonth.PadLeft(2, '0') + UseBillingAddress.CardExpirationYear.Substring(2, 2));

            if (UseBillingAddress.CardStartDate.Length != 0 && UseBillingAddress.CardStartDate != "00" && UseBillingAddress.CardStartDate != null)
            {
                transactionCommand.Append("&CARDSTART=" + UseBillingAddress.CardStartDate);
            }
            if (UseBillingAddress.CardIssueNumber.Length != 0)
            {
                transactionCommand.Append("&CARDISSUE=" + UseBillingAddress.CardIssueNumber);
            }


            //set the CSC code:
            if (CardExtraCode.Trim().Length != 0)
            {
                transactionCommand.Append("&CSC2MATCH=" + CardExtraCode);
                transactionCommand.Append("&CVV2=" + CardExtraCode);
            }

            if (UseShippingAddress != null)
            {
                transactionCommand.Append("&SHIPTOSTREET=" + CleanForNVP(UseShippingAddress.Address1));
                transactionCommand.Append("&SHIPTOCITY=" + CleanForNVP(UseShippingAddress.City));
                transactionCommand.Append("&SHIPTOSTATE=" + CleanForNVP(UseShippingAddress.State));
                transactionCommand.Append("&SHIPTOZIP=" + CleanForNVP(UseShippingAddress.Zip));
                transactionCommand.Append("&SHIPTOCOUNTRY=" + CleanForNVP(AppLogic.GetCountryTwoLetterISOCode(UseShippingAddress.Country))); //PayFlowPro documentation says it's SHIPTOCOUNTRY but support says it's COUNTRYCODE which is the one that worked for me
                transactionCommand.Append("&COUNTRYCODE=" + CleanForNVP(AppLogic.GetCountryTwoLetterISOCode(UseShippingAddress.Country))); //PayFlowPro documentation says it's SHIPTOCOUNTRY but support says it's COUNTRYCODE which is the one that worked for me
            }

            transactionCommand.Append("&FIRSTNAME=" + CleanForNVP(UseBillingAddress.FirstName));
            transactionCommand.Append("&LASTNAME=" + CleanForNVP(UseBillingAddress.LastName));
            transactionCommand.Append("&STREET=" + CleanForNVP(UseBillingAddress.Address1));
            transactionCommand.Append("&CITY=" + CleanForNVP(UseBillingAddress.City));
            transactionCommand.Append("&STATE=" + CleanForNVP(UseBillingAddress.State));
            transactionCommand.Append("&ZIP=" + CleanForNVP(UseBillingAddress.Zip));
            transactionCommand.Append("&COUNTRY=" + AppLogic.GetCountryTwoLetterISOCode(UseBillingAddress.Country));
            transactionCommand.Append("&CUSTIP=" + CleanForNVP(CommonLogic.CustomerIpAddress())); //cart.ThisCustomer.LastIPAddress);
            transactionCommand.Append("&EMAIL=" + CleanForNVP(UseBillingAddress.EMail));

            if (ECI.Length != 0)
            {
                transactionCommand.Append("&CAVV[" + CAVV.Length.ToString() + "]=" + CAVV);
                transactionCommand.Append("&ECI=" + ECI);
            }

            int TO = AppLogic.AppConfigUSInt("PayFlowPro.Timeout");
            if (TO == 0)
            {
                TO = 45;
            }

            String AuthServer = CommonLogic.IIF(AppLogic.AppConfigBool("UseLiveTransactions"), AppLogic.AppConfig("PayFlowPro.LiveURL"), AppLogic.AppConfig("PayFlowPro.TestURL"));


            String RequestID = System.Guid.NewGuid().ToString("N");
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] data = encoding.GetBytes(transactionCommand.ToString());

            int MaxTries = AppLogic.AppConfigUSInt("GatewayRetries") + 1;
            int CurrentTry = 0;
            bool CallSuccessful = false;
            do
            {
                CurrentTry++;
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(AuthServer);
                myRequest.Method = "POST";
                myRequest.ContentType = "text/namevalue";
                myRequest.Headers.Add("X-VPS-REQUEST-ID", RequestID);
                myRequest.Headers.Add("X-VPS-CLIENT-TIMEOUT", TO.ToString());
                myRequest.Headers.Add("X-VPS-VIT-CLIENT-CERTIFICATION-ID", "aspdotnetstorefront");
                myRequest.ContentLength = data.Length;
                try
                {
                    Stream newStream = myRequest.GetRequestStream();
                    // Send the data.
                    newStream.Write(data, 0, data.Length);
                    newStream.Close();
                    // get the response
                    WebResponse myResponse;
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

            result = "ERROR: Error Calling PayFlow Payment Gateway"; // This should get overwritten below.
            String curString = rawResponseString;

            String AVSAddr = String.Empty;
            String AVSZip = String.Empty;

            String[] statusArray = curString.Split('&');
            for (int i = statusArray.GetLowerBound(0); i <= statusArray.GetUpperBound(0); i++)
            {
                String[] lasKeyPair = statusArray[i].Split('=');
                switch (lasKeyPair[0].ToLowerInvariant())
                {
                    case "result":
                        replyCode = lasKeyPair[1];
                        break;
                    case "pnref":
                        responseCode = lasKeyPair[1];
                        break;
                    case "respmsg":
                        authResponse = lasKeyPair[1];
                        break;
                    case "authcode":
                        approvalCode = lasKeyPair[1];
                        break;
                    case "avsaddr":
                        AVSAddr = lasKeyPair[1];
                        break;
                    case "avszip":
                        AVSZip = lasKeyPair[1];
                        break;
                }
            }

            AuthorizationCode = approvalCode;
            AuthorizationResult = rawResponseString;
            AuthorizationTransID = CommonLogic.IIF(TransactionMode == TransactionModeEnum.auth, "AUTH=", "CAPTURE=") + responseCode;

            if (!String.IsNullOrEmpty(AVSZip))
            {
                if (!String.IsNullOrEmpty(AVSResult))
                {
                    AVSResult += ", ";
                }
                AVSResult += "Zip=" + AVSZip;
            }
            if (!String.IsNullOrEmpty(AVSAddr))
            {
                if (!String.IsNullOrEmpty(AVSResult))
                {
                    AVSResult += ", ";
                }
                AVSResult += "Addr=" + AVSAddr;
            }
            TransactionCommandOut = transactionCommand.ToString();

            // we want to zap the password from the being logged to the DB
            TransactionCommandOut = TransactionCommandOut.Replace(CommonLogic.ExtractToken(TransactionCommandOut, "PWD=", "&"), "****");
            TransactionResponse = rawResponseString;

            if (replyCode == "0")
            {
                result = AppLogic.ro_OK;
            }
            else
            {
                result = authResponse;
                if (result.Length == 0)
                {
                    result = "Unspecified Error";
                }
                result = result.Replace("account", "card");
                result = result.Replace("Account", "Card");
                result = result.Replace("ACCOUNT", "CARD");
            }

            return result;
        }

        // ----------------------------------------------------------------------------------
        // RECURRING BILLING ROUTINES
        // ----------------------------------------------------------------------------------

        // returns AppLogic.ro_OK if successful, along with the out RecurringSubscriptionID
        // returns error message if not successful
        // SubscriptionDescription may usually best be passed in as the product description of the first cart item
        public override string RecurringBillingCreateSubscription(String SubscriptionDescription, Customer ThisCustomer, Address UseBillingAddress, Address UseShippingAddress, Decimal RecurringAmount, DateTime StartDate, int RecurringInterval, DateIntervalTypeEnum RecurringIntervalType, int OriginalRecurringOrderNumber, string XID, IDictionary<string, string> TransactionContext,  out String RecurringSubscriptionID, out String RecurringSubscriptionCommand, out String RecurringSubscriptionResult)
        {
            String result = AppLogic.ro_OK;
            RecurringSubscriptionID = "TBD";
            RecurringSubscriptionCommand = String.Empty;
            RecurringSubscriptionResult = String.Empty;

            StringBuilder transactionCommand = new StringBuilder(4096);

            transactionCommand.Append("TRXTYPE=R"); // recurring billing API
            transactionCommand.Append("&ACTION=A"); // A = Add
            transactionCommand.Append("&PARTNER=" + AppLogic.AppConfig("PayFlowPro.PARTNER"));
            transactionCommand.Append("&VENDOR=" + AppLogic.AppConfig("PayFlowPro.VENDOR"));
            transactionCommand.Append("&USER=" + AppLogic.AppConfig("PayFlowPro.USER"));
            transactionCommand.Append("&PWD=" + AppLogic.AppConfig("PayFlowPro.PWD"));

            string tender = string.Empty;

            if (TransactionContext != null && TransactionContext.ContainsKey("TENDER"))
                tender = TransactionContext["TENDER"];


            if (tender.Equals("P"))
                transactionCommand.Append("&TENDER=P"); // P = PayPal
            else
                transactionCommand.Append("&TENDER=C"); // C = Credit Card

            transactionCommand.Append("&PROFILENAME=" + OriginalRecurringOrderNumber.ToString());
            transactionCommand.Append("&AMT=" + Localization.CurrencyStringForGatewayWithoutExchangeRate(RecurringAmount));
            if (UseBillingAddress != null && UseBillingAddress.CardNumber != null && UseBillingAddress.CardNumber.Length > 0)
            {
                if (UseBillingAddress.CardNumber != null)
                    transactionCommand.Append("&ACCT=" + UseBillingAddress.CardNumber);

                if (UseBillingAddress.CardExpirationMonth != null && UseBillingAddress.CardExpirationYear != null && UseBillingAddress.CardExpirationMonth.Length != 0 && UseBillingAddress.CardExpirationYear.Length != 0)
                    transactionCommand.Append("&EXPDATE=" + UseBillingAddress.CardExpirationMonth.PadLeft(2, '0') + UseBillingAddress.CardExpirationYear.Substring(2, 2));

                if (UseBillingAddress.CardStartDate.Length != 0 && UseBillingAddress.CardStartDate != "00" && UseBillingAddress.CardStartDate != null)
                    transactionCommand.Append("&CARDSTART=" + UseBillingAddress.CardStartDate);

                if (UseBillingAddress.CardIssueNumber.Length != 0)
                    transactionCommand.Append("&CARDISSUE=" + UseBillingAddress.CardIssueNumber);
            }

            if (tender.Equals("P"))
                transactionCommand.Append("&BAID=" + XID); // PayPal
            else
                transactionCommand.Append("&ORIGID=" + XID); // Credit Card

            if (UseShippingAddress != null)
            {
                transactionCommand.Append("&SHIPTOSTREET=" + CleanForNVP(UseShippingAddress.Address1));
                transactionCommand.Append("&SHIPTOCITY=" + CleanForNVP(UseShippingAddress.City));
                transactionCommand.Append("&SHIPTOSTATE=" + CleanForNVP(UseShippingAddress.State));
                transactionCommand.Append("&SHIPTOZIP=" + CleanForNVP(UseShippingAddress.Zip));
                transactionCommand.Append("&SHIPTOCOUNTRY=" + CleanForNVP(AppLogic.GetCountryTwoLetterISOCode(UseShippingAddress.Country))); //PayFlowPro documentation says it's SHIPTOCOUNTRY but support says it's COUNTRYCODE which is the one that worked for me
                transactionCommand.Append("&COUNTRYCODE=" + CleanForNVP(AppLogic.GetCountryTwoLetterISOCode(UseShippingAddress.Country))); //PayFlowPro documentation says it's SHIPTOCOUNTRY but support says it's COUNTRYCODE which is the one that worked for me
            }
            if (UseBillingAddress != null)
            {
                transactionCommand.Append("&FIRSTNAME=" + CleanForNVP(UseBillingAddress.FirstName));
                transactionCommand.Append("&LASTNAME=" + CleanForNVP(UseBillingAddress.LastName));
                transactionCommand.Append("&STREET=" + CleanForNVP(UseBillingAddress.Address1));
                transactionCommand.Append("&CITY=" + CleanForNVP(UseBillingAddress.City));
                transactionCommand.Append("&STATE=" + CleanForNVP(UseBillingAddress.State));
                transactionCommand.Append("&ZIP=" + CleanForNVP(UseBillingAddress.Zip));
                transactionCommand.Append("&COUNTRY=" + AppLogic.GetCountryTwoLetterISOCode(UseBillingAddress.Country));
                transactionCommand.Append("&CUSTIP=" + CleanForNVP(CommonLogic.CustomerIpAddress())); //cart.ThisCustomer.LastIPAddress);
                transactionCommand.Append("&EMAIL=" + CleanForNVP(UseBillingAddress.EMail));
            }

            // NOTE: since our recurring products have no sunset, set max gateway allowed total occurrences to provide maxmimum
            // length of subscription time
            // MAX interval supported by the gateway is 3 years
            switch (RecurringIntervalType)
            {
                case DateIntervalTypeEnum.Day:
                    result = "INVALID RECURRING INTERVAL TYPE [DAY], NOT SUPPORTED BY PAYFLOWPRO";
                    break;
                case DateIntervalTypeEnum.Week:
                    result = "INVALID RECURRING INTERVAL TYPE [WEEK], NOT SUPPORTED BY PAYFLOWPRO";
                    break;
                case DateIntervalTypeEnum.Month:
                    result = "INVALID RECURRING INTERVAL TYPE [MONTH], NOT SUPPORTED BY PAYFLOWPRO";
                    break;
                case DateIntervalTypeEnum.Year:
                    result = "INVALID RECURRING INTERVAL TYPE [YEAR], NOT SUPPORTED BY PAYFLOWPRO";
                    break;
                case DateIntervalTypeEnum.Weekly:
                    transactionCommand.Append("&PAYPERIOD=WEEK");
                    break;
                case DateIntervalTypeEnum.BiWeekly:
                    transactionCommand.Append("&PAYPERIOD=BIWK");
                    break;
                //case DateIntervalTypeEnum.SemiMonthly:
                //    transactionCommand.Append("&PAYPERIOD=SMMO");
                //    break;
                case DateIntervalTypeEnum.EveryFourWeeks:
                    transactionCommand.Append("&PAYPERIOD=FRWK");
                    break;
                case DateIntervalTypeEnum.Monthly:
                    transactionCommand.Append("&PAYPERIOD=MONT");
                    break;
                case DateIntervalTypeEnum.Quarterly:
                    transactionCommand.Append("&PAYPERIOD=QTER");
                    break;
                case DateIntervalTypeEnum.SemiYearly:
                    transactionCommand.Append("&PAYPERIOD=SMYR");
                    break;
                case DateIntervalTypeEnum.Yearly:
                    transactionCommand.Append("&PAYPERIOD=YEAR");
                    break;
                default:
                    transactionCommand.Append("&PAYPERIOD=MONT");
                    break;
            }
            if (result != AppLogic.ro_OK)
            {
                // error setting up interval, don't even call the gateway
                return result;
            }

            if (StartDate != null && StartDate.Month > 0 && StartDate.Year > 0)
                transactionCommand.Append("&START=" + StartDate.Month.ToString().PadLeft(2, '0') + StartDate.Day.ToString().PadLeft(2, '0') + StartDate.Year.ToString()); // MMDDYYYY

            transactionCommand.Append("&TERM=0"); // continue until canceled
            transactionCommand.Append("&CURRENCY=" + AppLogic.AppConfig("Localization.StoreCurrency"));
            transactionCommand.Append("&MAXFAILPAYMENTS=" + AppLogic.AppConfigNativeInt("PayflowPro.RecurringMaxFailPayments").ToString());

            if (UseBillingAddress != null)
                transactionCommand.Append("&EMAIL=" + CleanForNVP(CommonLogic.IIF(ThisCustomer.EMail.Length != 0, ThisCustomer.EMail, UseBillingAddress.EMail)));

            transactionCommand.Append("&DESC=" + CleanForNVP(SubscriptionDescription));

            if (UseBillingAddress != null)
            {
                transactionCommand.Append("&COMPANYNAME=" + CleanForNVP(UseBillingAddress.Company));
                transactionCommand.Append("&STREET=" + CleanForNVP(UseBillingAddress.Address1));
                transactionCommand.Append("&ZIP=" + CleanForNVP(UseBillingAddress.Zip));
            }

            RecurringSubscriptionCommand = transactionCommand.ToString();
            if (RecurringSubscriptionCommand.Length != 0)
            {
                if (UseBillingAddress != null && UseBillingAddress.CardNumber != null && UseBillingAddress.CardNumber.Length > 0)
                    RecurringSubscriptionCommand = RecurringSubscriptionCommand.Replace(UseBillingAddress.CardNumber, AppLogic.SafeDisplayCardNumber(UseBillingAddress.CardNumber, "Orders", 0));

                RecurringSubscriptionCommand = RecurringSubscriptionCommand.Replace(CommonLogic.ExtractToken(RecurringSubscriptionCommand, "PWD=", "&"), "****");
            }

            int TO = AppLogic.AppConfigUSInt("PayFlowPro.Timeout");
            if (TO == 0)
            {
                TO = 45;
            }

            String AuthServer = CommonLogic.IIF(AppLogic.AppConfigBool("UseLiveTransactions"), AppLogic.AppConfig("PayFlowPro.LiveURL"), AppLogic.AppConfig("PayFlowPro.TestURL"));

            String RequestID = System.Guid.NewGuid().ToString("N");
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] data = encoding.GetBytes(transactionCommand.ToString());
            String rawResponseString = String.Empty;

            int MaxTries = AppLogic.AppConfigUSInt("GatewayRetries") + 1;
            int CurrentTry = 0;
            bool CallSuccessful = false;
            do
            {
                CurrentTry++;
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(AuthServer);
                myRequest.Method = "POST";
                myRequest.ContentType = "text/namevalue";
                myRequest.Headers.Add("X-VPS-REQUEST-ID", RequestID);
                myRequest.Headers.Add("X-VPS-CLIENT-TIMEOUT", TO.ToString());
                myRequest.Headers.Add("X-VPS-VIT-CLIENT-CERTIFICATION-ID", "aspdotnetstorefront");
                myRequest.ContentLength = data.Length;
                try
                {
                    Stream newStream = myRequest.GetRequestStream();
                    // Send the data.
                    newStream.Write(data, 0, data.Length);
                    newStream.Close();
                    // get the response
                    WebResponse myResponse;
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

            RecurringSubscriptionResult = rawResponseString;

            if (!CallSuccessful)
            {
                result = "ERROR: Error Calling PayFlow Payment Gateway"; // This should get overwritten below.
            }
            else
            {
                String GRESULT = String.Empty;
                String GRESPMSG = String.Empty;
                String GPROFILEID = String.Empty;
                String GRPREF = String.Empty;

                String[] statusArray = rawResponseString.Split('&');
                for (int i = statusArray.GetLowerBound(0); i <= statusArray.GetUpperBound(0); i++)
                {
                    String[] lasKeyPair = statusArray[i].Split('=');
                    switch (lasKeyPair[0].ToLowerInvariant())
                    {
                        case "result":
                            GRESULT = lasKeyPair[1];
                            break;
                        case "respmsg":
                            GRESPMSG = lasKeyPair[1];
                            break;
                        case "profileid":
                            GPROFILEID = lasKeyPair[1];
                            break;
                        case "rpref":
                            GRPREF = lasKeyPair[1];
                            break;
                    }
                }

                if (GRESULT == "0")
                {
                    RecurringSubscriptionID = GPROFILEID;
                    result = AppLogic.ro_OK;
                }
                else
                {
                    result = GRESPMSG;

                    // Log failure
                    DB.ExecuteSQL("insert into FailedTransaction(CustomerID,OrderNumber,IPAddress,OrderDate,PaymentGateway,PaymentMethod,TransactionCommand,TransactionResult,CustomerEMailed,RecurringSubscriptionID) values(" +
                        ThisCustomer.CustomerID.ToString() + "," + OriginalRecurringOrderNumber.ToString() + "," +
                        DB.SQuote(ThisCustomer.LastIPAddress) + ",getdate()," + DB.SQuote(Gateway.ro_GWPAYFLOWPRO) + "," +
                        DB.SQuote(AppLogic.TransactionTypeEnum.RECURRING_AUTO.ToString()) + "," + DB.SQuote(RecurringSubscriptionCommand) + "," + DB.SQuote(rawResponseString) + ",0," + DB.SQuote(RecurringSubscriptionID) + ")");
                }
            }
            SysLog.LogMessage("Recurring profile creation status: {0}".FormatWith(result), string.Empty, MessageTypeEnum.Informational, MessageSeverityEnum.Alert);
            return result;
        }

        public override string RecurringBillingCancelSubscription(String RecurringSubscriptionID, int OriginalRecurringOrderNumber, IDictionary<string, string> TransactionContext)
        {
            String result = String.Empty;
            StringBuilder transactionCommand = new StringBuilder(4096);

            transactionCommand.Append("TRXTYPE=R"); // recurring billing API
            transactionCommand.Append("&ACTION=C"); // C = Cancel
            transactionCommand.Append("&ORIGPROFILEID=" + RecurringSubscriptionID);
            transactionCommand.Append("&PARTNER=" + AppLogic.AppConfig("PayFlowPro.PARTNER"));
            transactionCommand.Append("&VENDOR=" + AppLogic.AppConfig("PayFlowPro.VENDOR"));
            transactionCommand.Append("&USER=" + AppLogic.AppConfig("PayFlowPro.USER"));
            transactionCommand.Append("&PWD=" + AppLogic.AppConfig("PayFlowPro.PWD"));
            transactionCommand.Append("&TENDER=C");

            int TO = AppLogic.AppConfigUSInt("PayFlowPro.Timeout");
            if (TO == 0)
            {
                TO = 45;
            }

            String AuthServer = CommonLogic.IIF(AppLogic.AppConfigBool("UseLiveTransactions"), AppLogic.AppConfig("PayFlowPro.LiveURL"), AppLogic.AppConfig("PayFlowPro.TestURL"));

            String RequestID = System.Guid.NewGuid().ToString("N");
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] data = encoding.GetBytes(transactionCommand.ToString());
            String rawResponseString = String.Empty;

            int MaxTries = AppLogic.AppConfigUSInt("GatewayRetries") + 1;
            int CurrentTry = 0;
            bool CallSuccessful = false;
            do
            {
                CurrentTry++;
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(AuthServer);
                myRequest.Method = "POST";
                myRequest.ContentType = "text/namevalue";
                myRequest.Headers.Add("X-VPS-REQUEST-ID", RequestID);
                myRequest.Headers.Add("X-VPS-CLIENT-TIMEOUT", TO.ToString());
                myRequest.Headers.Add("X-VPS-VIT-CLIENT-CERTIFICATION-ID", "aspdotnetstorefront");
                myRequest.ContentLength = data.Length;
                try
                {
                    Stream newStream = myRequest.GetRequestStream();
                    // Send the data.
                    newStream.Write(data, 0, data.Length);
                    newStream.Close();
                    // get the response
                    WebResponse myResponse;

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

            if (!CallSuccessful)
            {
                result = "ERROR: Error Calling PayFlow Payment Gateway"; // This should get overwritten below.
            }
            else
            {
                String GRESULT = String.Empty;
                String GRESPMSG = String.Empty;

                String[] statusArray = rawResponseString.Split('&');
                for (int i = statusArray.GetLowerBound(0); i <= statusArray.GetUpperBound(0); i++)
                {
                    String[] lasKeyPair = statusArray[i].Split('=');
                    switch (lasKeyPair[0].ToLowerInvariant())
                    {
                        case "result":
                            GRESULT = lasKeyPair[1];
                            break;
                        case "respmsg":
                            GRESPMSG = lasKeyPair[1];
                            break;
                    }
                }

                if (GRESULT == "0" || GRESULT == "33")  // 33 means that it is already canceled, so that's okay too.
                {
                    result = AppLogic.ro_OK;
                }
                else
                {
                    result = GRESPMSG;

                    // Log failure
                    String TransactionCommandOut = transactionCommand.ToString();
                    TransactionCommandOut = TransactionCommandOut.Replace(CommonLogic.ExtractToken(TransactionCommandOut, "PWD=", "&"), "****");

                    Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)HttpContext.Current.User).ThisCustomer;
                    DB.ExecuteSQL("insert into FailedTransaction(CustomerID,OrderNumber,IPAddress,OrderDate,PaymentGateway,PaymentMethod,TransactionCommand,TransactionResult,CustomerEMailed,RecurringSubscriptionID) values(" +
                        ThisCustomer.CustomerID.ToString() + "," + OriginalRecurringOrderNumber.ToString() + "," +
                        DB.SQuote(ThisCustomer.LastIPAddress) + ",getdate()," + DB.SQuote(Gateway.ro_GWPAYFLOWPRO) + "," +
                        DB.SQuote(AppLogic.TransactionTypeEnum.RECURRING_AUTO.ToString()) + "," + DB.SQuote(TransactionCommandOut) + "," + DB.SQuote(rawResponseString) + ",0," + DB.SQuote(RecurringSubscriptionID) + ")");
                }
            }
            return result;
        }

        public override string RecurringBillingAddressUpdate(String RecurringSubscriptionID, int OriginalRecurringOrderNumber, Address UseBillingAddress)
        {
            String result = String.Empty;
            StringBuilder transactionCommand = new StringBuilder(4096);

            transactionCommand.Append("TRXTYPE=R"); // recurring billing API
            transactionCommand.Append("&ACTION=M"); // M = Modify
            transactionCommand.Append("&ORIGPROFILEID=" + RecurringSubscriptionID);
            transactionCommand.Append("&PARTNER=" + AppLogic.AppConfig("PayFlowPro.PARTNER"));
            transactionCommand.Append("&VENDOR=" + AppLogic.AppConfig("PayFlowPro.VENDOR"));
            transactionCommand.Append("&USER=" + AppLogic.AppConfig("PayFlowPro.USER"));
            transactionCommand.Append("&PWD=" + AppLogic.AppConfig("PayFlowPro.PWD"));
            transactionCommand.Append("&TENDER=C");
            // Only include card info if we have a card number since it is optional.
            if (!String.IsNullOrEmpty(UseBillingAddress.CardNumber) && !UseBillingAddress.CardNumber.StartsWith("*"))
            {
                transactionCommand.Append("&ACCT=" + UseBillingAddress.CardNumber);
                transactionCommand.Append("&EXPDATE=" + UseBillingAddress.CardExpirationMonth.PadLeft(2, '0') + UseBillingAddress.CardExpirationYear.Substring(2, 2));
                if (UseBillingAddress.CardStartDate.Length != 0 && UseBillingAddress.CardStartDate != "00" && UseBillingAddress.CardStartDate != null)
                {
                    transactionCommand.Append("&CARDSTART=" + UseBillingAddress.CardStartDate);
                }
                if (UseBillingAddress.CardIssueNumber.Length != 0)
                {
                    transactionCommand.Append("&CARDISSUE=" + UseBillingAddress.CardIssueNumber);
                }
            }
            transactionCommand.Append("&FIRSTNAME=" + CleanForNVP(UseBillingAddress.FirstName));
            transactionCommand.Append("&LASTNAME=" + CleanForNVP(UseBillingAddress.LastName));
            transactionCommand.Append("&STREET=" + CleanForNVP(UseBillingAddress.Address1));
            transactionCommand.Append("&CITY=" + CleanForNVP(UseBillingAddress.City));
            transactionCommand.Append("&STATE=" + CleanForNVP(UseBillingAddress.State));
            transactionCommand.Append("&ZIP=" + CleanForNVP(UseBillingAddress.Zip));
            transactionCommand.Append("&COUNTRY=" + AppLogic.GetCountryTwoLetterISOCode(UseBillingAddress.Country));
            transactionCommand.Append("&CUSTIP=" + CleanForNVP(CommonLogic.CustomerIpAddress())); //cart.ThisCustomer.LastIPAddress);
            transactionCommand.Append("&EMAIL=" + CleanForNVP(UseBillingAddress.EMail));

            int TO = AppLogic.AppConfigUSInt("PayFlowPro.Timeout");
            if (TO == 0)
            {
                TO = 45;
            }

            String AuthServer = CommonLogic.IIF(AppLogic.AppConfigBool("UseLiveTransactions"), AppLogic.AppConfig("PayFlowPro.LiveURL"), AppLogic.AppConfig("PayFlowPro.TestURL"));

            String RequestID = System.Guid.NewGuid().ToString("N");
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] data = encoding.GetBytes(transactionCommand.ToString());
            String rawResponseString = String.Empty;

            int MaxTries = AppLogic.AppConfigUSInt("GatewayRetries") + 1;
            int CurrentTry = 0;
            bool CallSuccessful = false;
            do
            {
                CurrentTry++;
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(AuthServer);
                myRequest.Method = "POST";
                myRequest.ContentType = "text/namevalue";
                myRequest.Headers.Add("X-VPS-REQUEST-ID", RequestID);
                myRequest.Headers.Add("X-VPS-CLIENT-TIMEOUT", TO.ToString());
                myRequest.Headers.Add("X-VPS-VIT-CLIENT-CERTIFICATION-ID", "aspdotnetstorefront");
                myRequest.ContentLength = data.Length;
                try
                {
                    Stream newStream = myRequest.GetRequestStream();
                    // Send the data.
                    newStream.Write(data, 0, data.Length);
                    newStream.Close();
                    // get the response
                    WebResponse myResponse;
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

            if (!CallSuccessful)
            {
                result = "ERROR: Error Calling PayFlow Payment Gateway"; // This should get overwritten below.
            }
            else
            {
                String GRESULT = String.Empty;
                String GRESPMSG = String.Empty;

                String[] statusArray = rawResponseString.Split('&');
                for (int i = statusArray.GetLowerBound(0); i <= statusArray.GetUpperBound(0); i++)
                {
                    String[] lasKeyPair = statusArray[i].Split('=');
                    switch (lasKeyPair[0].ToLowerInvariant())
                    {
                        case "result":
                            GRESULT = lasKeyPair[1];
                            break;
                        case "respmsg":
                            GRESPMSG = lasKeyPair[1];
                            break;
                    }
                }

                if (GRESULT == "0")
                {
                    result = AppLogic.ro_OK;
                }
                else
                {
                    result = GRESPMSG;

                    // Log failure
                    String TransactionCommandOut = transactionCommand.ToString();
                    TransactionCommandOut = TransactionCommandOut.Replace(CommonLogic.ExtractToken(TransactionCommandOut, "PWD=", "&"), "****");
                    if (!String.IsNullOrEmpty(UseBillingAddress.CardNumber) && TransactionCommandOut.Contains(UseBillingAddress.CardNumber))
                    {
                        TransactionCommandOut = TransactionCommandOut.Replace(UseBillingAddress.CardNumber, "****" + AppLogic.SafeDisplayCardNumberLast4(UseBillingAddress.CardNumber, "Address", UseBillingAddress.AddressID));
                    }

                    Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)HttpContext.Current.User).ThisCustomer;
                    DB.ExecuteSQL("insert into FailedTransaction(CustomerID,OrderNumber,IPAddress,OrderDate,PaymentGateway,PaymentMethod,TransactionCommand,TransactionResult,CustomerEMailed,RecurringSubscriptionID) values(" +
                        ThisCustomer.CustomerID.ToString() + "," + OriginalRecurringOrderNumber.ToString() + "," +
                        DB.SQuote(ThisCustomer.LastIPAddress) + ",getdate()," + DB.SQuote(Gateway.ro_GWPAYFLOWPRO) + "," +
                        DB.SQuote(AppLogic.TransactionTypeEnum.RECURRING_AUTO.ToString()) + "," + DB.SQuote(TransactionCommandOut) + "," + DB.SQuote(rawResponseString) + ",0," + DB.SQuote(RecurringSubscriptionID) + ")");
                }
            }
            return result;
        }

        public override String RecurringBillingGetStatusFile()
        {
            DateTime dtLastRun = Localization.ParseDBDateTime(AppLogic.AppConfig("Recurring.GatewayLastImportedDate"));
            DateTime dtStart = dtLastRun;
            if (dtStart.Equals(System.DateTime.MinValue))
            {
                dtStart = DateTime.Today.AddDays((double)-1); // Run report for yesterday
            }
            else
            {
                dtStart = dtLastRun.AddDays((double)1.0);
            }

            DateTime dtEnd = DateTime.Today.AddDays((double)-1); // Run report through yesterday

            return RecurringBillingGetStatusFile(dtStart, dtEnd);
        }

        private String RecurringBillingGetStatusFile(DateTime ReportStartDate, DateTime ReportEndDate)
        {
            // force midnight to midnight
            ReportStartDate = Localization.ParseNativeDateTime(ReportStartDate.Year.ToString("0000") + "-" + ReportStartDate.Month.ToString("00") + "-" + ReportStartDate.Day.ToString("00") + " 00:00:00");
            ReportEndDate = Localization.ParseNativeDateTime(ReportEndDate.Year.ToString("0000") + "-" + ReportEndDate.Month.ToString("00") + "-" + ReportEndDate.Day.ToString("00") + " 23:59:59");

            // apply offset
            ReportStartDate = ReportStartDate.AddHours(AppLogic.AppConfigNativeInt("Recurring.GatewayImportOffsetHours"));
            ReportEndDate = ReportEndDate.AddHours(AppLogic.AppConfigNativeInt("Recurring.GatewayImportOffsetHours"));

            String ResultXML = String.Empty;

            String start_date = ReportStartDate.Year.ToString("0000") + "-" + ReportStartDate.Month.ToString("00") + "-" + ReportStartDate.Day.ToString("00") + " " + ReportStartDate.Hour.ToString("00") + ":" + ReportStartDate.Minute.ToString("00") + ":" + ReportStartDate.Second.ToString("00");
            String end_date = ReportEndDate.Year.ToString("0000") + "-" + ReportEndDate.Month.ToString("00") + "-" + ReportEndDate.Day.ToString("00") + " " + ReportEndDate.Hour.ToString("00") + ":" + ReportEndDate.Minute.ToString("00") + ":" + ReportEndDate.Second.ToString("00");

            String reportName = AppLogic.AppConfig("PayFlowPro.Reporting.ReportName");
            if (String.IsNullOrEmpty(reportName))
            {
                reportName = "RecurringBillingReport";
            }
            int requestPageSize = 50;

            reportingEngineRequest rER = new reportingEngineRequest();

            AuthorizeRequest auth = new AuthorizeRequest();
            auth.partner = AppLogic.AppConfig("PayFlowPro.PARTNER");
            auth.password = AppLogic.AppConfig("PayFlowPro.PWD");
            auth.user = AppLogic.AppConfig("PayFlowPro.USER");
            auth.vendor = AppLogic.AppConfig("PayFlowPro.VENDOR");
            rER.authRequest = auth;

            runReportRequest rRR = new runReportRequest();
            rRR.pageSize = requestPageSize.ToString();
            rRR.rptName = reportName;

            reportParam[] myRptParams = new reportParam[2];
            reportParam rptParam1 = new reportParam();
            rptParam1.paramName = "start_date";
            rptParam1.paramValue = start_date;
            myRptParams[0] = rptParam1;

            reportParam rptParam2 = new reportParam();
            rptParam2.paramName = "end_date";
            rptParam2.paramValue = end_date;
            myRptParams[1] = rptParam2;

            rRR.rptParams = myRptParams;

            rER.RunRptReq = rRR;

            //Send request and get response
            string req = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>" + SerializeObject(rER);
            ResultXML = PayFlowProController.SendReceiveReporting(req);

            // Loop through all pages and all <reportDataRow> records on each page
            // ResultCode=0 is APPROVED, non-zero is DECLINED

            rER.RunRptReq = null;

            String responseCode = String.Empty;
            String responseMsg = String.Empty;
            String reportID = String.Empty;
            String statusCode = String.Empty;
            String statusMsg = String.Empty;

            responseCode = CommonLogic.ExtractToken(ResultXML, "<responseCode>", "</responseCode>");
            responseMsg = CommonLogic.ExtractToken(ResultXML, "<responseMsg>", "</responseMsg>");

            if (responseCode == "100") //Success
            {
                reportID = CommonLogic.ExtractToken(ResultXML, "<reportId>", "</reportId>");
                statusCode = CommonLogic.ExtractToken(ResultXML, "<statusCode>", "</statusCode>");
                statusMsg = CommonLogic.ExtractToken(ResultXML, "<statusMsg>", "</statusMsg>");
            }
            else
            {
                if (String.IsNullOrEmpty(responseCode))
                {
                    return "Error: Failed to communicate to Payflow Reporting Service";
                }
                else
                {
                    return "Error: [" + responseCode + "] " + responseMsg;
                }
            }

            if (statusCode != "2" && statusCode != "3")
            {
                return "Error: [" + statusCode + "] " + statusMsg;
            }

            int MaxStatusCheck = 10;
            int cntStatusCheck = 0;
            while (statusCode == "2" && cntStatusCheck < MaxStatusCheck)  // report is currently executing
            {
                cntStatusCheck++;
                Thread.Sleep(5000); // Wait 5 seconds and try again

                getResultsRequest resultrequest = new getResultsRequest();
                resultrequest.reportId = reportID;
                rER.ResultsRequest = resultrequest;

                //Send request and get response
                req = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>" + SerializeObject(rER);
                ResultXML = PayFlowProController.SendReceiveReporting(req);

                rER.ResultsRequest = null;

                responseCode = String.Empty;
                responseMsg = String.Empty;
                reportID = String.Empty;
                statusCode = String.Empty;
                statusMsg = String.Empty;

                responseCode = CommonLogic.ExtractToken(ResultXML, "<responseCode>", "</responseCode>");
                responseMsg = CommonLogic.ExtractToken(ResultXML, "<responseMsg>", "</responseMsg>");

                if (responseCode == "100") //Success
                {
                    reportID = CommonLogic.ExtractToken(ResultXML, "<reportId>", "</reportId>");
                    statusCode = CommonLogic.ExtractToken(ResultXML, "<statusCode>", "</statusCode>");
                    statusMsg = CommonLogic.ExtractToken(ResultXML, "<statusMsg>", "</statusMsg>");
                }
                else
                {
                    if (String.IsNullOrEmpty(responseCode))
                    {
                        return "Error: getResultsRequest attempt " + cntStatusCheck.ToString() + " failed to communicate to Payflow Reporting Service.";
                    }
                    else
                    {
                        return "Error: getResultsRequest attempt " + cntStatusCheck.ToString() + " [" + responseCode + "] " + responseMsg;
                    }
                }
            }

            if (statusCode != "3")
            {
                return "Error: [" + statusCode + "] " + statusMsg + ", ReportID=" + reportID;
            }

            getMetaDataRequest metadatarequest = new getMetaDataRequest();
            metadatarequest.reportId = reportID;
            rER.MetaDataRequest = metadatarequest;

            //Send request and get response
            req = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>" + SerializeObject(rER);
            ResultXML = PayFlowProController.SendReceiveReporting(req);

            rER.MetaDataRequest = null;

            int reportRows = 0;
            int reportPages = 0;
            int reportPageSize = 0;
            int reportColumns = 0;

            int reportColProfileID = -1; // column number of "Profile ID"
            int reportColResultCode = -1; // column number of "Result Code"
            int reportColRetryReason = -1; // column number of "Retry Reason"
            int reportColTransactionID = -1; // column number of "Transaction ID"
            int reportColTime = -1; // column number of "Time"

            String result = String.Empty;
            try
            {

                XmlDocument xmlResponseDoc = new XmlDocument();
                xmlResponseDoc.LoadXml(ResultXML);

                reportRows = Localization.ParseNativeInt(xmlResponseDoc.SelectSingleNode("//numberOfRows").InnerText);
                reportPages = Localization.ParseNativeInt(xmlResponseDoc.SelectSingleNode("//numberOfPages").InnerText);
                reportPageSize = Localization.ParseNativeInt(xmlResponseDoc.SelectSingleNode("//pageSize").InnerText);
                reportColumns = Localization.ParseNativeInt(xmlResponseDoc.SelectSingleNode("//numberOfColumns").InnerText);

                XmlNodeList nodesColumns = xmlResponseDoc.SelectNodes("//columnMetaData");

                for (int i = 0; i < nodesColumns.Count; i++)
                {
                    XmlNode N = nodesColumns[i];

                    switch (XmlCommon.XmlField(N, "dataName").ToUpperInvariant())
                    {
                        case "PROFILE ID":
                            reportColProfileID = XmlCommon.XmlAttributeNativeInt(N, "colNum");
                            break;
                        case "RESULT CODE":
                            reportColResultCode = XmlCommon.XmlAttributeNativeInt(N, "colNum");
                            break;
                        case "RETRY REASON":
                            reportColRetryReason = XmlCommon.XmlAttributeNativeInt(N, "colNum");
                            break;
                        case "TRANSACTION ID":
                            reportColTransactionID = XmlCommon.XmlAttributeNativeInt(N, "colNum");
                            break;
                        case "TIME":
                            reportColTime = XmlCommon.XmlAttributeNativeInt(N, "colNum");
                            break;
                    }
                }

                if (reportColProfileID < 0
                    || reportColResultCode < 0
                    || reportColRetryReason < 0
                    || reportColTransactionID < 0
                    || reportColTime < 0)
                {
                    return "Error: Required columns not found in report.";
                }

            }
            catch (Exception ex)
            {
                return "Error: Unrecognized getMetaDataRequest response. " + ex.Message;
            }


            // Now we can actually get the report data, page by page
            StringBuilder tmpS = new StringBuilder(4096);
            tmpS.Append("<RecurringBillingReport Gateway=\"" + Gateway.ro_GWPAYFLOWPRO + "\">");
            tmpS.Append("\n");
            int iRowTotal = 0;
            for (int iPage = 1; iPage <= reportPages; iPage++)
            {
                if (iRowTotal > reportRows)
                {
                    break;
                }


                getDataRequest datarequest = new getDataRequest();
                datarequest.pageNum = iPage.ToString();
                datarequest.reportId = reportID;
                rER.DataRequest = datarequest;

                //Send request and get response
                req = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>" + SerializeObject(rER);
                ResultXML = PayFlowProController.SendReceiveReporting(req);

                // Loop through all pages and all <reportDataRow> records on each page
                // ResultCode=0 is APPROVED, non-zero is DECLINED

                try
                {
                    XmlDocument xmlResponseDoc = new XmlDocument();
                    xmlResponseDoc.LoadXml(ResultXML);
                    XmlNodeList nodesRows = xmlResponseDoc.SelectNodes("//reportDataRow");

                    foreach (XmlNode nodeRow in nodesRows)
                    {
                        iRowTotal++;
                        String ThisProfileID = nodeRow.SelectSingleNode("columnData[@colNum=\"" + reportColProfileID.ToString() + "\"]").InnerText.Trim();
                        String ThisResultCode = nodeRow.SelectSingleNode("columnData[@colNum=\"" + reportColResultCode.ToString() + "\"]").InnerText.Trim();
                        String ThisRetryReason = nodeRow.SelectSingleNode("columnData[@colNum=\"" + reportColRetryReason.ToString() + "\"]").InnerText.Trim();
                        String ThisTransactionID = nodeRow.SelectSingleNode("columnData[@colNum=\"" + reportColTransactionID.ToString() + "\"]").InnerText.Trim();
                        String ThisTime = nodeRow.SelectSingleNode("columnData[@colNum=\"" + reportColTime.ToString() + "\"]").InnerText.Trim();

                        String ThisStatus = "DECLINED";
                        String Msg = ThisRetryReason;

                        if (ThisResultCode == "0")
                        {
                            ThisStatus = "APPROVED";
                            Msg = String.Empty;
                        }
                        if (!String.IsNullOrEmpty(ThisProfileID))
                        {
                            tmpS.Append(String.Format("<TX RecurringSubscriptionID=\"{0}\" Status=\"{1}\" Message=\"{2}\" PNREF=\"{3}\" Time=\"{4}\"/>\n", ThisProfileID, ThisStatus, XmlCommon.XmlEncodeAttribute(Msg), ThisTransactionID, ThisTime));
                        }
                    }
                }
                catch
                {
                    return "Error: Unrecognized getDataRequest response.";
                }
            }
            tmpS.Append("</RecurringBillingReport>");
            return XmlCommon.PrettyPrintXml(tmpS.ToString());
        }

        private string CleanForNVP(string strIn)
        { // PayFlow does NOT want it URL encoded, so we'll just zap the characters that will break things.
            return strIn.Replace("&", "").Replace("=", "");
        }

        private string SerializeObject(object obj2Serialize)
        {
            StringBuilder strRequest = new StringBuilder();
            XmlWriterSettings xs = new XmlWriterSettings();
            xs.OmitXmlDeclaration = true;
            XmlWriter xwRequest = XmlTextWriter.Create(strRequest, xs);
            XmlSerializer serRequest = new XmlSerializer(obj2Serialize.GetType());
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            serRequest.Serialize(xwRequest, obj2Serialize, ns);
            return strRequest.ToString();
        }

        public override string ProcessAutoBillStatusFile(String GW, string StatusFile, out string Results, RecurringOrderMgr OrderManager)
        {
            String Status = AppLogic.ro_OK;
            StringBuilder tmpS = new StringBuilder();
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(StatusFile);
                String Separator = String.Empty;
                foreach (XmlNode n in doc.SelectNodes("/RecurringBillingReport/TX"))
                {
                    tmpS.Append(Separator);
                    String TxSubID = XmlCommon.XmlAttribute(n, "RecurringSubscriptionID");
                    String TxStatus = XmlCommon.XmlAttribute(n, "Status").ToUpperInvariant();
                    String TxMsg = XmlCommon.XmlAttribute(n, "Message");
                    String TxID = XmlCommon.XmlAttribute(n, "PNREF");
                    String TxTime = XmlCommon.XmlAttribute(n, "Time");

                    DateTime dtTx = Localization.ParseNativeDateTime(TxTime);

                    int OrigOrdNumber = AppLogic.GetOriginalRecurringOrderNumberFromSubscriptionID(TxSubID);
                    tmpS.Append("Processing ID ");
                    tmpS.Append(TxSubID);
                    tmpS.Append(",");
                    tmpS.Append(TxStatus);
                    tmpS.Append("=");
                    try
                    {
                        String tmpStatus = String.Empty;

                        if (OrigOrdNumber == 0)
                        {
                            if (dtTx.Equals(DateTime.MinValue))
                            {
                                dtTx = DateTime.Now;
                            }
                            tmpStatus = "No Original Order Found";
                            if (TxID.Length != 0)
                            {
                                tmpStatus += ", PNREF=" + TxID;
                            }
                            DB.ExecuteSQL("insert into FailedTransaction(CustomerID,OrderNumber,IPAddress,OrderDate,PaymentGateway,PaymentMethod,TransactionCommand,TransactionResult,CustomerEMailed,RecurringSubscriptionID) values(" +
                                "0,0,'0.0.0.0'," + DB.DateQuote(dtTx) + "," + DB.SQuote(GW) + "," +
                                DB.SQuote(AppLogic.TransactionTypeEnum.RECURRING_AUTO.ToString()) + "," + DB.SQuote(AppLogic.ro_NotApplicable) + "," + DB.SQuote(tmpStatus) + ",0," + DB.SQuote(TxSubID) + ")");
                        }
                        else
                        {
                            if (TxStatus == "APPROVED")
                            {
                                int NewOrderNumber = 0;
                                tmpStatus = OrderManager.ProcessAutoBillApproved(OrigOrdNumber, TxID, dtTx, out NewOrderNumber);
                            }
                            else
                            {
                                tmpStatus = OrderManager.ProcessAutoBillDeclined(OrigOrdNumber, TxID, dtTx, TxSubID, TxMsg);
                            }
                            if (tmpStatus == AppLogic.ro_OK)
                            {
                                // mark this one as processed ok
                                // TBD
                            }
                            else
                            {
                                // mark this record as not processed
                                // TBD

                                int ProcessCustomerID = Order.GetOrderCustomerID(OrigOrdNumber);
                                Customer ProcessCustomer = new Customer(ProcessCustomerID, true);

                                if (dtTx.Equals(DateTime.MinValue))
                                {
                                    dtTx = DateTime.Now;
                                }
                                if (TxID.Length != 0)
                                {
                                    tmpStatus += ", PNREF=" + TxID;
                                }
                                DB.ExecuteSQL("insert into FailedTransaction(CustomerID,OrderNumber,IPAddress,OrderDate,PaymentGateway,PaymentMethod,TransactionCommand,TransactionResult,CustomerEMailed,RecurringSubscriptionID) values(" +
                                    ProcessCustomer.CustomerID.ToString() + "," + OrigOrdNumber.ToString() + "," +
                                    DB.SQuote(ProcessCustomer.LastIPAddress) + "," + DB.DateQuote(dtTx) + "," + DB.SQuote(GW) + "," +
                                    DB.SQuote(AppLogic.TransactionTypeEnum.RECURRING_AUTO.ToString()) + "," + DB.SQuote(AppLogic.ro_NotApplicable) + "," + DB.SQuote(tmpStatus) + ",0," + DB.SQuote(TxSubID) + ")");
                            }
                        }
                        tmpS.Append(tmpStatus);
                    }
                    catch (Exception ex)
                    {
                        tmpS.Append(ex.Message);
                    }
                    Separator = "\n";
                }
                Status = AppLogic.ro_OK;
            }
            catch (Exception ex)
            {
                Status = ex.Message;
            }
            Results = tmpS.ToString();
            return Status;
        }

        public override RecurringSupportType RecurringSupportType()
        {
            return Processors.RecurringSupportType.Normal;
        }

        public override List<DateIntervalTypeEnum> GetAllowedRecurringIntervals()
        {
            List<DateIntervalTypeEnum> includedTypes = new List<DateIntervalTypeEnum>();
            includedTypes.Add(DateIntervalTypeEnum.Weekly);
            includedTypes.Add(DateIntervalTypeEnum.BiWeekly);
            includedTypes.Add(DateIntervalTypeEnum.EveryFourWeeks);
            includedTypes.Add(DateIntervalTypeEnum.Monthly);
            includedTypes.Add(DateIntervalTypeEnum.Quarterly);
            includedTypes.Add(DateIntervalTypeEnum.SemiYearly);
            includedTypes.Add(DateIntervalTypeEnum.Yearly);
            return includedTypes;
        }

        public override string DisplayName(string localeSetting)
        {
            return "PayPal Payflow Pro";
        }
    }
}
