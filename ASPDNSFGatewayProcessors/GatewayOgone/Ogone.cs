// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Text;
using System.Web;
using System.Net;
using System.IO;
using System.Xml;
using System.Data;
using AspDotNetStorefrontCore;
using System.Data.SqlClient;

namespace AspDotNetStorefrontGateways.Processors
{
	/// <summary>
	/// Summary description for Ogone.
	/// </summary>
	public class Ogone : GatewayProcessor
	{
		public Ogone() {}

        public override String CaptureOrder(Order o)
        {
            int SkinID = 1;
            String Locale = ((AspDotNetStorefrontPrincipal)HttpContext.Current.User).ThisCustomer.LocaleSetting;

            String result = AppLogic.ro_OK;
            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");

            o.CaptureTXCommand = "";
            o.CaptureTXResult = "";

            String TransID = o.AuthorizationPNREF;
            Decimal OrderTotal = o.OrderBalance;

            ASCIIEncoding encoding = new ASCIIEncoding();
            StringBuilder transactionCommand = new StringBuilder(4096);

            transactionCommand.Append("PSPID=" + AppLogic.AppConfig("Ogone.PSPID"));
            transactionCommand.Append("&USERID=" + AppLogic.AppConfig("Ogone.USERID"));
            transactionCommand.Append("&PSWD=" + AppLogic.AppConfig("Ogone.PSWD"));
            transactionCommand.Append("&operation=SAS"); // capture
            transactionCommand.Append("&PAYID=" + TransID);
            transactionCommand.Append("&Withroot=Y"); // Adds a root element to our XML response. Possible values: ‘Y’ or empty.
            if (OrderTotal != System.Decimal.Zero)
            {
                // amount could have changed by admin user, so capture the current Order Total from the db:
                transactionCommand.Append("&amount=" + Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal).Replace(".", ""));
            }
            o.CaptureTXCommand = transactionCommand.ToString().Replace("PSWD=" + AppLogic.AppConfig("Ogone.PSWD"), "PSWD=****");

            byte[] data = encoding.GetBytes(transactionCommand.ToString());

            // Prepare web request...
            try
            {
                String AuthServer = CommonLogic.IIF(useLiveTransactions, AppLogic.AppConfig("Ogone.LiveServer"), AppLogic.AppConfig("Ogone.TestServer"));
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
                } while (!CallSuccessful && CurrentTry < MaxTries);

                if (CallSuccessful)
                {
                    // rawResponseString now has gateway response
                    o.CaptureTXResult = rawResponseString;
                    XmlDocument Doc = new XmlDocument();
                    Doc.LoadXml(rawResponseString);
                    XmlNode Node = Doc.SelectSingleNode("/ogone/ncresponse");

                    String responseNCStatus = String.Empty;
                    String errorResponse = String.Empty;
                    String responseNCError = String.Empty;

                    if (Node != null)
                    {
                        responseNCStatus = XmlCommon.XmlAttribute(Node, "NCSTATUS");
                        errorResponse = XmlCommon.XmlAttribute(Node, "NCERRORPLUS");
                        responseNCError = XmlCommon.XmlAttribute(Node, "NCERROR");

                        if (responseNCStatus == "0")
                        {
                            result = AppLogic.ro_OK;
                        }
                        else
                        {
                            result = AppLogic.GetString("gw.ogone.admin.ncerror." + responseNCError, SkinID, Locale);
                            if (result == "gw.ogone.admin.ncerror." + responseNCError)
                            {
                                result = "Error [" + responseNCError + "] " + errorResponse;
                            }
                        }
                    }
                    else
                    {
                        result = AppLogic.GetString("gw.ogone.parsefailure", SkinID, Locale);
                    }
                }
                else
                {
                    result = AppLogic.GetString("gw.ogone.commsfailure", SkinID, Locale);
                }
            }
            catch
            {
                result = AppLogic.GetString("gw.ogone.exception", SkinID, Locale);
            }
            return result;
        }

        public override String VoidOrder(int OrderNumber)
        {
            int SkinID = 1;
            String Locale = ((AspDotNetStorefrontPrincipal)HttpContext.Current.User).ThisCustomer.LocaleSetting;

            String result = AppLogic.ro_OK;
            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");

            DB.ExecuteSQL("update orders set VoidTXCommand=NULL, VoidTXResult=NULL where OrderNumber=" + OrderNumber.ToString());

            String TransID = DB.GetSqlS("select AuthorizationPNREF S from Orders  with (NOLOCK)  where OrderNumber=" + OrderNumber.ToString());

            ASCIIEncoding encoding = new ASCIIEncoding();
            StringBuilder transactionCommand = new StringBuilder(4096);

            transactionCommand.Append("PSPID=" + AppLogic.AppConfig("Ogone.PSPID"));
            transactionCommand.Append("&USERID=" + AppLogic.AppConfig("Ogone.USERID"));
            transactionCommand.Append("&PSWD=" + AppLogic.AppConfig("Ogone.PSWD"));
            transactionCommand.Append("&operation=DES"); // delete authorization
            transactionCommand.Append("&PAYID=" + TransID);
            transactionCommand.Append("&Withroot=Y"); // Adds a root element to our XML response. Possible values: ‘Y’ or empty.

            DB.ExecuteSQL("update orders set VoidTXCommand=" + DB.SQuote(transactionCommand.ToString().Replace("PSWD=" + AppLogic.AppConfig("Ogone.PSWD"), "PSWD=****")) + " where OrderNumber=" + OrderNumber.ToString());

            byte[] data = encoding.GetBytes(transactionCommand.ToString());

            // Prepare web request...
            try
            {
                String AuthServer = CommonLogic.IIF(useLiveTransactions, AppLogic.AppConfig("Ogone.LiveServer"), AppLogic.AppConfig("Ogone.TestServer"));
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
                } while (!CallSuccessful && CurrentTry < MaxTries);

                if (CallSuccessful)
                {
                    // rawResponseString now has gateway response

                    DB.ExecuteSQL("update orders set VoidTXResult=" + DB.SQuote(rawResponseString) + " where OrderNumber=" + OrderNumber.ToString());

                    XmlDocument Doc = new XmlDocument();
                    Doc.LoadXml(rawResponseString);
                    XmlNode Node = Doc.SelectSingleNode("/ogone/ncresponse");

                    String responseNCStatus = String.Empty;
                    String errorResponse = String.Empty;
                    String responseNCError = String.Empty;

                    if (Node != null)
                    {
                        responseNCStatus = XmlCommon.XmlAttribute(Node, "NCSTATUS");
                        errorResponse = XmlCommon.XmlAttribute(Node, "NCERRORPLUS");
                        responseNCError = XmlCommon.XmlAttribute(Node, "NCERROR");

                        if (responseNCStatus == "0")
                        {
                            result = AppLogic.ro_OK;
                        }
                        else
                        {
                            result = AppLogic.GetString("gw.ogone.admin.ncerror." + responseNCError, SkinID, Locale);
                            if (result == "gw.ogone.admin.ncerror." + responseNCError)
                            {
                                result = "Error [" + responseNCError + "] " + errorResponse;
                            }
                        }
                    }
                    else
                    {
                        result = AppLogic.GetString("gw.ogone.parsefailure", SkinID, Locale);
                    }
                }
                else
                {
                    result = AppLogic.GetString("gw.ogone.commsfailure", SkinID, Locale);
                }
            }
            catch
            {
                result = AppLogic.GetString("gw.ogone.exception", SkinID, Locale);
            }
            return result;
        }

		// if RefundAmount == 0.0M, then then ENTIRE order amount will be refunded!
        public override String RefundOrder(int OriginalOrderNumber, int NewOrderNumber, decimal RefundAmount, String RefundReason, Address UseBillingAddress)
        {
            int SkinID = 1;
            String Locale = ((AspDotNetStorefrontPrincipal)HttpContext.Current.User).ThisCustomer.LocaleSetting;

            String result = AppLogic.ro_OK;
            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");

            DB.ExecuteSQL("update orders set RefundTXCommand=NULL, RefundTXResult=NULL where OrderNumber=" + OriginalOrderNumber.ToString());

            String TransID = String.Empty;
            Decimal OrderTotal = 0.0M;

            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS("select AuthorizationPNREF, OrderTotal from orders   with (NOLOCK)  where OrderNumber=" + OriginalOrderNumber.ToString(), conn))
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

            transactionCommand.Append("PSPID=" + AppLogic.AppConfig("Ogone.PSPID"));
            transactionCommand.Append("&USERID=" + AppLogic.AppConfig("Ogone.USERID"));
            transactionCommand.Append("&PSWD=" + AppLogic.AppConfig("Ogone.PSWD"));
            if (RefundAmount == System.Decimal.Zero || Localization.CurrencyStringForGatewayWithoutExchangeRate(RefundAmount) == Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal))
            {
                transactionCommand.Append("&operation=RFS"); // full/last refund   
            }
            else
            {
                transactionCommand.Append("&operation=RFD"); // partial refund
            }
            transactionCommand.Append("&PAYID=" + TransID);
            transactionCommand.Append("&Withroot=Y"); // Adds a root element to our XML response. Possible values: ‘Y’ or empty.
            if (RefundAmount == System.Decimal.Zero)
            {
                transactionCommand.Append("&amount=" + Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal).Replace(".", ""));
            }
            else
            {
                transactionCommand.Append("&amount=" + Localization.CurrencyStringForGatewayWithoutExchangeRate(RefundAmount).Replace(".", ""));
            }
            DB.ExecuteSQL("update orders set RefundTXCommand=" + DB.SQuote(transactionCommand.ToString().Replace("PSWD=" + AppLogic.AppConfig("Ogone.PSWD"), "PSWD=****")) + " where OrderNumber=" + OriginalOrderNumber.ToString());

            byte[] data = encoding.GetBytes(transactionCommand.ToString());

            // Prepare web request...
            try
            {
                String AuthServer = CommonLogic.IIF(useLiveTransactions, AppLogic.AppConfig("Ogone.LiveServer"), AppLogic.AppConfig("Ogone.TestServer"));
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
                } while (!CallSuccessful && CurrentTry < MaxTries);

                if (CallSuccessful)
                {
                    // rawResponseString now has gateway response

                    DB.ExecuteSQL("update orders set RefundTXResult=" + DB.SQuote(rawResponseString) + " where OrderNumber=" + OriginalOrderNumber.ToString());

                    XmlDocument Doc = new XmlDocument();
                    Doc.LoadXml(rawResponseString);
                    XmlNode Node = Doc.SelectSingleNode("/ogone/ncresponse");

                    String responseNCStatus = String.Empty;
                    String errorResponse = String.Empty;
                    String responseNCError = String.Empty;

                    if (Node != null)
                    {
                        responseNCStatus = XmlCommon.XmlAttribute(Node, "NCSTATUS");
                        errorResponse = XmlCommon.XmlAttribute(Node, "NCERRORPLUS");
                        responseNCError = XmlCommon.XmlAttribute(Node, "NCERROR");

                        if (responseNCStatus == "0")
                        {
                            result = AppLogic.ro_OK;
                        }
                        else
                        {
                            result = AppLogic.GetString("gw.ogone.admin.ncerror." + responseNCError, SkinID, Locale);
                            if (result == "gw.ogone.admin.ncerror." + responseNCError)
                            {
                                result = "Error [" + responseNCError + "] " + errorResponse;
                            }
                        }
                    }
                    else
                    {
                        result = AppLogic.GetString("gw.ogone.parsefailure", SkinID, Locale);
                    }
                }
                else
                {
                    result = AppLogic.GetString("gw.ogone.commsfailure", SkinID, Locale);
                }
            }
            catch
            {
                result = AppLogic.GetString("gw.ogone.exception", SkinID, Locale);
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

            if (AppLogic.AppConfigBool("Ogone.Use3TierMode"))
            {
                // Customer enters card details on Ogone.com
                AuthorizationTransID = XID;
            }
            else
            {
                // Customer entered card details on our store front

                ASCIIEncoding encoding = new ASCIIEncoding();
                StringBuilder transactionCommand = new StringBuilder(4096);
               
                String Amount = Localization.CurrencyStringForGatewayWithoutExchangeRate((OrderTotal)).Replace(".", "");
                String Operation = CommonLogic.IIF(TransactionMode == TransactionModeEnum.auth, "RES", "SAL");
                String SignatureSeed = OrderNumber.ToString() + Amount
                    + Localization.StoreCurrency() + UseBillingAddress.CardNumber
                    + AppLogic.AppConfig("Ogone.PSPID") + Operation;

                transactionCommand.Append("PSPID=" + AppLogic.AppConfig("Ogone.PSPID"));
                transactionCommand.Append("&USERID=" + AppLogic.AppConfig("Ogone.USERID"));
                transactionCommand.Append("&PSWD=" + AppLogic.AppConfig("Ogone.PSWD"));
                transactionCommand.Append("&operation=" + Operation);
                transactionCommand.Append("&ECI=7"); // 7 = E-commerce with SSL encryption
                if (AppLogic.AppConfig("Ogone.SHASignature").Length != 0)
                {
                    transactionCommand.Append("&SHASign=" + Ogone.Signature(SignatureSeed));
                }
                transactionCommand.Append("&orderID=" + OrderNumber.ToString());
                transactionCommand.Append("&currency=" + Localization.StoreCurrency());
                transactionCommand.Append("&amount=" + Amount); // OrderTotal * 100 (no decimals/punctuation)
                transactionCommand.Append("&CARDNO=" + UseBillingAddress.CardNumber);
                if (CardExtraCode.Length != 0)
                {
                    transactionCommand.Append("&CVC=" + CardExtraCode.Trim());
                }
                else if (UseBillingAddress.CardIssueNumber.Length != 0)
                {
                    transactionCommand.Append("&CVC=" + UseBillingAddress.CardIssueNumber);
                }
                else if (UseBillingAddress.CardStartDate != null && UseBillingAddress.CardStartDate.Length != 0 && UseBillingAddress.CardStartDate != "00")
                {
                    transactionCommand.Append("&CVC=" + UseBillingAddress.CardStartDate);
                }
                transactionCommand.Append("&ED=" + UseBillingAddress.CardExpirationMonth.PadLeft(2, '0') + "/" + UseBillingAddress.CardExpirationYear.Substring(2,2)); // MM/YY
                transactionCommand.Append("&ownertelno=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.Phone));
                transactionCommand.Append("&EMAIL=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.EMail));
                transactionCommand.Append("&REMOTE_ADDR=" + CommonLogic.CustomerIpAddress());
                transactionCommand.Append("&CN=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.FirstName + " " + UseBillingAddress.LastName));
                transactionCommand.Append("&Owneraddress=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.Address1));
                transactionCommand.Append("&ownertown=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.City));
                transactionCommand.Append("&OwnerZip=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.Zip));
                transactionCommand.Append("&ownercty=" + HttpContext.Current.Server.UrlEncode(AppLogic.GetCountryTwoLetterISOCode(UseBillingAddress.Country)));
                transactionCommand.Append("&Withroot=Y"); // Adds a root element to our XML response. Possible values: ‘Y’ or empty.

                byte[] data = encoding.GetBytes(transactionCommand.ToString());

                // Prepare web request...
                try
                {
                    String AuthServer = CommonLogic.IIF(useLiveTransactions, AppLogic.AppConfig("Ogone.LiveServerOrder"), AppLogic.AppConfig("Ogone.TestServerOrder"));
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
                    } while (!CallSuccessful && CurrentTry < MaxTries);

                    if (CallSuccessful)
                    {
                        // rawResponseString now has gateway response
                        TransactionResponse = rawResponseString;

                        XmlDocument Doc = new XmlDocument();
                        Doc.LoadXml(rawResponseString);
                        XmlNode Node = Doc.SelectSingleNode("/ogone/ncresponse");
                        String responseStatus = String.Empty;
                        String responseNCStatus = String.Empty;
                        String responseNCError = String.Empty;
                        String TransID = String.Empty;
                        String approvalCode = String.Empty;
                        String AVSCode = String.Empty;
                        String CVCode = String.Empty;
                        String ScoreCat = String.Empty;
                        String authResponse = String.Empty;
                        
                        if (Node != null)
                        {
                            responseStatus = XmlCommon.XmlAttribute(Node, "STATUS");
                            responseNCStatus = XmlCommon.XmlAttribute(Node, "NCSTATUS");
                            TransID = XmlCommon.XmlAttribute(Node, "PAYID");
                            approvalCode = XmlCommon.XmlAttribute(Node, "ACCEPTANCE");
                            AVSCode = XmlCommon.XmlAttribute(Node, "AAVCHECK");
                            CVCode = XmlCommon.XmlAttribute(Node, "CVCCHECK");
                            authResponse = XmlCommon.XmlAttribute(Node, "NCERRORPLUS");
                            responseNCError = XmlCommon.XmlAttribute(Node, "NCERROR");
                            ScoreCat = XmlCommon.XmlAttribute(Node, "SCO_CATEGORY");

                            AuthorizationCode = approvalCode;
                            AuthorizationResult = rawResponseString;
                            AuthorizationTransID = TransID;
                            AVSResult = AVSCode;
                            if (CVCode.Length > 0)
                            {
                                if (AVSResult.Length != 0)
                                {
                                    AVSResult += ", ";
                                }
                                AVSResult += "CV Result: " + CVCode;
                            }
                            if (ScoreCat.Length != 0)
                            {
                                if (AVSResult.Length != 0)
                                {
                                    AVSResult += ", ";
                                }
                                AVSResult += "Score: " + ScoreCat;
                            }
                            TransactionCommandOut = transactionCommand.ToString().Replace("PSWD=" + AppLogic.AppConfig("Ogone.PSWD"), "PSWD=****");

                            if (responseNCStatus == "0")
                            {
                                result = AppLogic.ro_OK;
                            }
                            else
                            {
                                result = AppLogic.GetString("gw.ogone.ncerror." + responseNCError, UseBillingAddress.SkinID, UseBillingAddress.LocaleSetting);
                                if (result == "gw.ogone.ncerror." + responseNCError)
                                {
                                    result = AppLogic.GetString("gw.ogone.cardfailed", UseBillingAddress.SkinID, UseBillingAddress.LocaleSetting);
                                }
                            }
                        }
                        else
                        {
                            result = AppLogic.GetString("gw.ogone.parsefailure", UseBillingAddress.SkinID, UseBillingAddress.LocaleSetting);
                        }
                    }
                    else
                    {
                        result = AppLogic.GetString("gw.ogone.commsfailure", UseBillingAddress.SkinID, UseBillingAddress.LocaleSetting);
                    }
                }
                catch
                {
                    result = AppLogic.GetString("gw.ogone.exception", UseBillingAddress.SkinID, UseBillingAddress.LocaleSetting);
                }
            }
            return result;
		}

        public static String Signature(String Seed)
        {
            String output = String.Empty;
            if (AppLogic.AppConfig("Ogone.SHASignature").Length == 0)
            {
                return output;
            }
            Byte[] clearBytes;
            Byte[] hashedBytes;
            clearBytes = System.Text.Encoding.UTF8.GetBytes(Seed + AppLogic.AppConfig("Ogone.SHASignature"));
            System.Security.Cryptography.SHA1CryptoServiceProvider sha1 = new System.Security.Cryptography.SHA1CryptoServiceProvider();
            sha1.ComputeHash(clearBytes);
            hashedBytes = sha1.Hash;
            sha1.Clear();
            output = BitConverter.ToString(hashedBytes).Replace("-", "").ToUpper();
            return output;
        }

        public override bool RequiresFinalization()
        {
            return AppLogic.AppConfigBool("Ogone.Use3TierMode");
        }

        public override string ProcessingPageRedirect()
        {
            if (AppLogic.AppConfigBool("Ogone.Use3TierMode"))
            {
                return "~/ogonepane.aspx";
            }

            return base.ProcessingPageRedirect();
        }

        public override string CreditCardPaneInfo(int SkinId, Customer ThisCustomer)
        {
            if (AppLogic.AppConfigBool("Ogone.Use3TierMode"))
            {
                return AppLogic.GetString("checkoutogone.aspx.1", SkinId, ThisCustomer.LocaleSetting);
            }

            return base.CreditCardPaneInfo(SkinId, ThisCustomer);
        }
	}
}
