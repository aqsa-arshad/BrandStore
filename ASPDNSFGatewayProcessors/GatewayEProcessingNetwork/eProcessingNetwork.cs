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
using AspDotNetStorefrontCommon;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontGateways.Processors
{
    /// <summary>
    /// Summary description for eProcessingNetwork.
    /// </summary>
    public class eProcessingNetwork : GatewayProcessor
    {
        public eProcessingNetwork() { }

        public override String CaptureOrder(Order o)
        {
            String result = AppLogic.ro_OK;

            o.CaptureTXCommand = "";
            o.CaptureTXResult = "";
            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");
            String TransID = o.AuthorizationPNREF;
            Decimal OrderTotal = o.OrderBalance;

            ASCIIEncoding encoding = new ASCIIEncoding();
            StringBuilder transactionCommand = new StringBuilder(4096);
            transactionCommand.Append("x_type=PRIOR_AUTH_CAPTURE");

            String X_Login = AppLogic.AppConfig("eProcessingNetwork_X_LOGIN");
            if (X_Login.Trim().Equals("REGISTRY", StringComparison.InvariantCultureIgnoreCase))
            {
                WindowsRegistry reg = new WindowsRegistry(AppLogic.AppConfig("EncryptKey.RegistryLocation"));
                X_Login = reg.Read("eProcessingNetwork_X_LOGIN");
                reg = null;
            }

            String X_TranKey = AppLogic.AppConfig("eProcessingNetwork_X_TRAN_KEY");
            if (X_TranKey.Trim().Equals("REGISTRY", StringComparison.InvariantCultureIgnoreCase))
            {
                WindowsRegistry reg = new WindowsRegistry(AppLogic.AppConfig("EncryptKey.RegistryLocation"));
                X_TranKey = reg.Read("eProcessingNetwork_X_TRAN_KEY");
                reg = null;
            }

            transactionCommand.Append("&x_login=" + X_Login);
            transactionCommand.Append("&x_tran_key=" + X_TranKey);
            transactionCommand.Append("&x_version=" + AppLogic.AppConfig("eProcessingNetwork_X_VERSION"));
            transactionCommand.Append("&x_test_request=" + CommonLogic.IIF(useLiveTransactions, "FALSE", "TRUE"));
            transactionCommand.Append("&x_method=" + AppLogic.AppConfig("eProcessingNetwork_X_METHOD"));
            transactionCommand.Append("&x_delim_Data=" + AppLogic.AppConfig("eProcessingNetwork_X_DELIM_DATA"));
            transactionCommand.Append("&x_delim_Char=" + AppLogic.AppConfig("eProcessingNetwork_X_DELIM_CHAR"));
            transactionCommand.Append("&x_encap_char=" + AppLogic.AppConfig("eProcessingNetwork_X_ENCAP_CHAR"));
            transactionCommand.Append("&x_relay_response=" + AppLogic.AppConfig("eProcessingNetwork_X_RELAY_RESPONSE"));
            transactionCommand.Append("&x_customer_ip=" + CommonLogic.CustomerIpAddress());
            transactionCommand.Append("&x_trans_id=" + TransID);
            if (OrderTotal != System.Decimal.Zero)
            {
                // amount could have changed by admin user, so capture the current Order Total from the db:
                transactionCommand.Append("&x_amount=" + Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal));
            }
            o.CaptureTXCommand = transactionCommand.ToString().Replace(X_TranKey, "*".PadLeft(X_TranKey.Length));
            
            try
            {
                byte[] data = encoding.GetBytes(transactionCommand.ToString());

                // Prepare web request...
                String AuthServer = CommonLogic.IIF(useLiveTransactions, AppLogic.AppConfig("eProcessingNetwork_LIVE_SERVER"), AppLogic.AppConfig("eProcessingNetwork_TEST_SERVER"));
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
                    rawResponseString = "0|||Error Calling eProcessingNetwork Payment Gateway||||||||";
                }

                // rawResponseString now has gateway response
                String[] statusArray = rawResponseString.Split(AppLogic.AppConfig("eProcessingNetwork_X_DELIM_CHAR").ToCharArray());
                // this seems to be a new item where auth.net is returing quotes around each parameter, so strip them out:
                for (int i = statusArray.GetLowerBound(0); i <= statusArray.GetUpperBound(0); i++)
                {
                    statusArray[i] = statusArray[i].Trim('\"');
                }

                String sql = String.Empty;
                String replyCode = statusArray[0];

                o.CaptureTXResult = rawResponseString;
                
                if (replyCode == "1")
                {
                    result = AppLogic.ro_OK;
                }
                else
                {
                    result = statusArray[3];
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
            String result = AppLogic.ro_OK;

            DB.ExecuteSQL("update orders set VoidTXCommand=NULL, VoidTXResult=NULL where OrderNumber=" + OrderNumber.ToString());
            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");
            String TransID = String.Empty;
            int CustomerID = 0;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select AuthorizationPNREF,CustomerID from Orders  with (NOLOCK)  where OrderNumber=" + OrderNumber.ToString(), con))
                {
                    if (rs.Read())
                    {
                        TransID = DB.RSField(rs, "AuthorizationPNREF");
                        CustomerID = DB.RSFieldInt(rs, "CustomerID");
                    }
                }
            }

            String X_Login = AppLogic.AppConfig("eProcessingNetwork_X_LOGIN");
            if (X_Login.Trim().Equals("REGISTRY", StringComparison.InvariantCultureIgnoreCase))
            {
                WindowsRegistry reg = new WindowsRegistry(AppLogic.AppConfig("EncryptKey.RegistryLocation"));
                X_Login = reg.Read("eProcessingNetwork_X_LOGIN");
                reg = null;
            }

            String X_TranKey = AppLogic.AppConfig("eProcessingNetwork_X_TRAN_KEY");
            if (X_TranKey.Trim().Equals("REGISTRY", StringComparison.InvariantCultureIgnoreCase))
            {
                WindowsRegistry reg = new WindowsRegistry(AppLogic.AppConfig("EncryptKey.RegistryLocation"));
                X_TranKey = reg.Read("eProcessingNetwork_X_TRAN_KEY");
                reg = null;
            }


            ASCIIEncoding encoding = new ASCIIEncoding();
            StringBuilder transactionCommand = new StringBuilder(4096);
            transactionCommand.Append("x_type=VOID");
            transactionCommand.Append("&x_login=" + X_Login);
            transactionCommand.Append("&x_tran_key=" + X_TranKey);
            transactionCommand.Append("&x_version=" + AppLogic.AppConfig("eProcessingNetwork_X_VERSION"));
            transactionCommand.Append("&x_test_request=" + CommonLogic.IIF(useLiveTransactions, "FALSE", "TRUE"));
            transactionCommand.Append("&x_method=" + AppLogic.AppConfig("eProcessingNetwork_X_METHOD"));
            transactionCommand.Append("&x_delim_Data=" + AppLogic.AppConfig("eProcessingNetwork_X_DELIM_DATA"));
            transactionCommand.Append("&x_delim_Char=" + AppLogic.AppConfig("eProcessingNetwork_X_DELIM_CHAR"));
            transactionCommand.Append("&x_encap_char=" + AppLogic.AppConfig("eProcessingNetwork_X_ENCAP_CHAR"));
            transactionCommand.Append("&x_relay_response=" + AppLogic.AppConfig("eProcessingNetwork_X_RELAY_RESPONSE"));
            transactionCommand.Append("&x_customer_ip=" + CommonLogic.CustomerIpAddress());
            transactionCommand.Append("&x_trans_id=" + TransID);

            DB.ExecuteSQL("update orders set VoidTXCommand=" + DB.SQuote(transactionCommand.ToString().Replace(X_TranKey, "*".PadLeft(X_TranKey.Length))) + " where OrderNumber=" + OrderNumber.ToString());

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
                    String AuthServer = CommonLogic.IIF(useLiveTransactions, AppLogic.AppConfig("eProcessingNetwork_LIVE_SERVER"), AppLogic.AppConfig("eProcessingNetwork_TEST_SERVER"));
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
                        rawResponseString = "0|||Error Calling eProcessing Network Payment Gateway||||||||";
                    }

                    // rawResponseString now has gateway response
                    String[] statusArray = rawResponseString.Split(AppLogic.AppConfig("eProcessingNetwork_X_DELIM_CHAR").ToCharArray());
                    // this seems to be a new item where auth.net is returing quotes around each parameter, so strip them out:
                    for (int i = statusArray.GetLowerBound(0); i <= statusArray.GetUpperBound(0); i++)
                    {
                        statusArray[i] = statusArray[i].Trim('\"');
                    }

                    String sql = String.Empty;
                    String replyCode = statusArray[0].Replace(":", "");

                    DB.ExecuteSQL("update orders set VoidTXResult=" + DB.SQuote(rawResponseString) + " where OrderNumber=" + OrderNumber.ToString());
                    if (replyCode == "1")
                    {
                        result = AppLogic.ro_OK;
                    }
                    else
                    {
                        result = statusArray[3];
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

            DB.ExecuteSQL("update orders set RefundTXCommand=NULL, RefundTXResult=NULL where OrderNumber=" + OriginalOrderNumber.ToString());
            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");
            String TransID = String.Empty;
            String Last4 = String.Empty;
            int CustomerID = 0;
            Decimal OrderTotal = System.Decimal.Zero;
            String BillingLastName = String.Empty;
            String BillingFirstName = String.Empty;
            String BillingCompany = String.Empty;
            String BillingAddress1 = String.Empty;
            String BillingAddress2 = String.Empty;
            String BillingSuite = String.Empty;
            String BillingCity = String.Empty;
            String BillingState = String.Empty;
            String BillingZip = String.Empty;
            String BillingCountry = String.Empty;
            String BillingPhone = String.Empty;
            String BillingEMail = String.Empty;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select * from orders   with (NOLOCK)  where OrderNumber=" + OriginalOrderNumber.ToString(), con))
                {
                    if (rs.Read())
                    {
                        TransID = DB.RSField(rs, "AuthorizationPNREF");
                        Last4 = DB.RSField(rs, "Last4");
                        OrderTotal = DB.RSFieldDecimal(rs, "OrderTotal");
                        CustomerID = DB.RSFieldInt(rs, "CustomerID");
                        BillingLastName = DB.RSField(rs, "BillingLastName");
                        BillingFirstName = DB.RSField(rs, "BillingFirstName");
                        BillingCompany = DB.RSField(rs, "BillingCompany");
                        BillingAddress1 = DB.RSField(rs, "BillingAddress1");
                        BillingAddress2 = DB.RSField(rs, "BillingAddress2");
                        BillingSuite = DB.RSField(rs, "BillingSuite");
                        BillingCity = DB.RSField(rs, "BillingCity");
                        BillingState = DB.RSField(rs, "BillingState");
                        BillingZip = DB.RSField(rs, "BillingZip");
                        BillingCountry = DB.RSField(rs, "BillingCountry");
                        BillingPhone = DB.RSField(rs, "BillingPhone");
                        BillingEMail = DB.RSField(rs, "EMail");
                    }
                }
            }

            String X_Login = AppLogic.AppConfig("eProcessingNetwork_X_LOGIN");
            if (X_Login.Trim().Equals("REGISTRY", StringComparison.InvariantCultureIgnoreCase))
            {
                WindowsRegistry reg = new WindowsRegistry(AppLogic.AppConfig("EncryptKey.RegistryLocation"));
                X_Login = reg.Read("eProcessingNetwork_X_LOGIN");
                reg = null;
            }

            String X_TranKey = AppLogic.AppConfig("eProcessingNetwork_X_TRAN_KEY");
            if (X_TranKey.Trim().Equals("REGISTRY", StringComparison.InvariantCultureIgnoreCase))
            {
                WindowsRegistry reg = new WindowsRegistry(AppLogic.AppConfig("EncryptKey.RegistryLocation"));
                X_TranKey = reg.Read("eProcessingNetwork_X_TRAN_KEY");
                reg = null;
            }

            ASCIIEncoding encoding = new ASCIIEncoding();
            StringBuilder transactionCommand = new StringBuilder(4096);
            transactionCommand.Append("x_type=CREDIT");
            transactionCommand.Append("&x_login=" + X_Login);
            transactionCommand.Append("&x_tran_key=" + X_TranKey);
            transactionCommand.Append("&x_version=" + AppLogic.AppConfig("eProcessingNetwork_X_VERSION"));
            transactionCommand.Append("&x_test_request=" + CommonLogic.IIF(useLiveTransactions, "FALSE", "TRUE"));
            transactionCommand.Append("&x_method=" + AppLogic.AppConfig("eProcessingNetwork_X_METHOD"));
            transactionCommand.Append("&x_delim_Data=" + AppLogic.AppConfig("eProcessingNetwork_X_DELIM_DATA"));
            transactionCommand.Append("&x_delim_Char=" + AppLogic.AppConfig("eProcessingNetwork_X_DELIM_CHAR"));
            transactionCommand.Append("&x_encap_char=" + AppLogic.AppConfig("eProcessingNetwork_X_ENCAP_CHAR"));
            transactionCommand.Append("&x_relay_response=" + AppLogic.AppConfig("eProcessingNetwork_X_RELAY_RESPONSE"));
            transactionCommand.Append("&x_trans_id=" + TransID);
            if (RefundAmount == System.Decimal.Zero)
            {
                transactionCommand.Append("&x_amount=" + Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal));
            }
            else
            {
                transactionCommand.Append("&x_amount=" + Localization.CurrencyStringForGatewayWithoutExchangeRate(RefundAmount));
            }
            transactionCommand.Append("&x_cust_id=" + CustomerID.ToString());
            transactionCommand.Append("&x_invoice_num=" + OriginalOrderNumber.ToString());
            transactionCommand.Append("&x_email=" + Security.UrlEncode(BillingEMail));
            transactionCommand.Append("&x_email_customer=false");
            transactionCommand.Append("&x_customer_ip=" + CommonLogic.CustomerIpAddress());
            transactionCommand.Append("&x_card_num=" + Last4);

            transactionCommand.Append("&x_description=" + Security.UrlEncode(RefundReason));
            transactionCommand.Append("&x_first_name=" + Security.UrlEncode(BillingFirstName));
            transactionCommand.Append("&x_last_name=" + Security.UrlEncode(BillingLastName));
            transactionCommand.Append("&x_company=" + Security.UrlEncode(BillingCompany));
            transactionCommand.Append("&x_address=" + Security.UrlEncode(BillingAddress1));
            transactionCommand.Append("&x_city=" + Security.UrlEncode(BillingCity));
            transactionCommand.Append("&x_state=" + Security.UrlEncode(BillingState));
            transactionCommand.Append("&x_zip=" + Security.UrlEncode(BillingZip));
            transactionCommand.Append("&x_country=" + Security.UrlEncode(BillingCountry));

            DB.ExecuteSQL("update orders set RefundTXCommand=" + DB.SQuote(transactionCommand.ToString().Replace(X_TranKey, "*".PadLeft(X_TranKey.Length))) + " where OrderNumber=" + OriginalOrderNumber.ToString());

            if (TransID.Length == 0 || TransID == "0")
            {
                result = "Invalid or Empty Transaction ID";
            }
            else if (Last4.Length == 0)
            {
                result = "Credit Card Number (Last4) Not Found or Empty";
            }
            else
            {
                try
                {

                    byte[] data = encoding.GetBytes(transactionCommand.ToString());

                    // Prepare web request...
                    String AuthServer = CommonLogic.IIF(useLiveTransactions, AppLogic.AppConfig("eProcessingNetwork_LIVE_SERVER"), AppLogic.AppConfig("eProcessingNetwork_TEST_SERVER"));
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
                        rawResponseString = "0|||Error Calling eProcessing Network Payment Gateway||||||||";
                    }

                    // rawResponseString now has gateway response
                    String[] statusArray = rawResponseString.Split(AppLogic.AppConfig("eProcessingNetwork_X_DELIM_CHAR").ToCharArray());
                    // this seems to be a new item where auth.net is returing quotes around each parameter, so strip them out:
                    for (int i = statusArray.GetLowerBound(0); i <= statusArray.GetUpperBound(0); i++)
                    {
                        statusArray[i] = statusArray[i].Trim('\"');
                    }

                    String sql = String.Empty;
                    String replyCode = statusArray[0].Replace(":", "");

                    DB.ExecuteSQL("update orders set RefundTXResult=" + DB.SQuote(rawResponseString) + " where OrderNumber=" + OriginalOrderNumber.ToString());
                    if (replyCode == "1")
                    {
                        result = AppLogic.ro_OK;
                    }
                    else
                    {
                        result = statusArray[3];
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
            AuthorizationCode = String.Empty;
            AuthorizationResult = String.Empty;
            AuthorizationTransID = String.Empty;
            AVSResult = String.Empty;
            TransactionCommandOut = String.Empty;
            TransactionResponse = String.Empty;

            String X_Login = AppLogic.AppConfig("eProcessingNetwork_X_LOGIN");
            if (X_Login.Trim().Equals("REGISTRY", StringComparison.InvariantCultureIgnoreCase))
            {
                WindowsRegistry reg = new WindowsRegistry(AppLogic.AppConfig("EncryptKey.RegistryLocation"));
                X_Login = reg.Read("eProcessingNetwork_X_LOGIN");
                reg = null;
            }

            String X_TranKey = AppLogic.AppConfig("eProcessingNetwork_X_TRAN_KEY");
            if (X_TranKey.Trim().Equals("REGISTRY", StringComparison.InvariantCultureIgnoreCase))
            {
                WindowsRegistry reg = new WindowsRegistry(AppLogic.AppConfig("EncryptKey.RegistryLocation"));
                X_TranKey = reg.Read("eProcessingNetwork_X_TRAN_KEY");
                reg = null;
            }

            ASCIIEncoding encoding = new ASCIIEncoding();
            StringBuilder transactionCommand = new StringBuilder(4096);

            transactionCommand.Append("x_type=" + CommonLogic.IIF(TransactionMode == TransactionModeEnum.auth, "AUTH_ONLY", "AUTH_CAPTURE"));

            transactionCommand.Append("&x_login=" + X_Login);
            transactionCommand.Append("&x_tran_key=" + X_TranKey);
            transactionCommand.Append("&x_version=" + AppLogic.AppConfig("eProcessingNetwork_X_VERSION"));
            transactionCommand.Append("&x_test_request=" + CommonLogic.IIF(useLiveTransactions, "FALSE", "TRUE"));
            transactionCommand.Append("&x_merchant_email=" + Security.UrlEncode(AppLogic.AppConfig("eProcessingNetwork_X_Email")));
            transactionCommand.Append("&x_description=" + Security.UrlEncode(AppLogic.AppConfig("StoreName") + " Order " + OrderNumber.ToString()));

            transactionCommand.Append("&x_method=" + AppLogic.AppConfig("eProcessingNetwork_X_METHOD"));

            transactionCommand.Append("&x_delim_Data=" + AppLogic.AppConfig("eProcessingNetwork_X_DELIM_DATA"));
            transactionCommand.Append("&x_delim_Char=" + AppLogic.AppConfig("eProcessingNetwork_X_DELIM_CHAR"));
            transactionCommand.Append("&x_encap_char=" + AppLogic.AppConfig("eProcessingNetwork_X_ENCAP_CHAR"));
            transactionCommand.Append("&x_relay_response=" + AppLogic.AppConfig("eProcessingNetwork_X_RELAY_RESPONSE"));

            transactionCommand.Append("&x_email_customer=" + AppLogic.AppConfig("eProcessingNetwork_X_Email_CUSTOMER"));
            transactionCommand.Append("&x_recurring_billing=" + AppLogic.AppConfig("eProcessingNetwork_X_RECURRING_BILLING"));

            transactionCommand.Append("&x_amount=" + Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal));
            transactionCommand.Append("&x_card_num=" + UseBillingAddress.CardNumber);
            if (CardExtraCode.Length != 0)
            {
                transactionCommand.Append("&x_card_code=" + CardExtraCode.Trim());
            }

            transactionCommand.Append("&x_exp_date=" + UseBillingAddress.CardExpirationMonth.PadLeft(2, '0') + "/" + UseBillingAddress.CardExpirationYear);
            transactionCommand.Append("&x_phone=" + Security.UrlEncode(UseBillingAddress.Phone));
            transactionCommand.Append("&x_fax=");
            transactionCommand.Append("&x_customer_tax_id=");
            transactionCommand.Append("&x_cust_id=" + CustomerID.ToString());
            transactionCommand.Append("&x_invoice_num=" + OrderNumber.ToString());
            transactionCommand.Append("&x_email=" + Security.UrlEncode(UseBillingAddress.EMail));
            transactionCommand.Append("&x_customer_ip=" + CommonLogic.CustomerIpAddress());

            transactionCommand.Append("&x_first_name=" + Security.UrlEncode(UseBillingAddress.FirstName));
            transactionCommand.Append("&x_last_name=" + Security.UrlEncode(UseBillingAddress.LastName));
            transactionCommand.Append("&x_company=" + Security.UrlEncode(UseBillingAddress.Company));
            transactionCommand.Append("&x_address=" + Security.UrlEncode(UseBillingAddress.Address1));
            transactionCommand.Append("&x_city=" + Security.UrlEncode(UseBillingAddress.City));
            transactionCommand.Append("&x_state=" + Security.UrlEncode(UseBillingAddress.State));
            transactionCommand.Append("&x_zip=" + Security.UrlEncode(UseBillingAddress.Zip));
            transactionCommand.Append("&x_country=" + Security.UrlEncode(UseBillingAddress.Country));

            if (UseShippingAddress != null)
            {
                transactionCommand.Append("&x_ship_to_first_name=" + Security.UrlEncode(UseShippingAddress.FirstName));
                transactionCommand.Append("&x_ship_to_last_name=" + Security.UrlEncode(UseShippingAddress.LastName));
                transactionCommand.Append("&x_ship_to_company=" + Security.UrlEncode(UseShippingAddress.Company));
                transactionCommand.Append("&x_ship_to_address=" + Security.UrlEncode(UseShippingAddress.Address1));
                transactionCommand.Append("&x_ship_to_city=" + Security.UrlEncode(UseShippingAddress.City));
                transactionCommand.Append("&x_ship_to_state=" + Security.UrlEncode(UseShippingAddress.State));
                transactionCommand.Append("&x_ship_to_zip=" + Security.UrlEncode(UseShippingAddress.Zip));
                transactionCommand.Append("&x_ship_to_country=" + Security.UrlEncode(UseShippingAddress.Country));
            }

            transactionCommand.Append("&x_customer_ip=" + CommonLogic.CustomerIpAddress());

            if (ECI.Length != 0)
            {
                transactionCommand.Append("&x_authentication_indicator=" + ECI);
                transactionCommand.Append("&x_cardholder_authentication_value=" + CAVV);
            }

            byte[] data = encoding.GetBytes(transactionCommand.ToString());

            // Prepare web request...
            try
            {
                String AuthServer = CommonLogic.IIF(useLiveTransactions, AppLogic.AppConfig("eProcessingNetwork_LIVE_SERVER"), AppLogic.AppConfig("eProcessingNetwork_TEST_SERVER"));
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
                        rawResponseString = "0|||Error Calling eProcessing Network Payment Gateway||||||||";
                    }
                }
                while (!CallSuccessful && CurrentTry < MaxTries);


                // rawResponseString now has gateway response
                TransactionResponse = rawResponseString;
                String[] statusArray = rawResponseString.Split(AppLogic.AppConfig("eProcessingNetwork_X_DELIM_CHAR").ToCharArray());
                // this seems to be a new item where auth.net is returing quotes around each parameter, so strip them out:
                for (int i = statusArray.GetLowerBound(0); i <= statusArray.GetUpperBound(0); i++)
                {
                    statusArray[i] = statusArray[i].Trim('\"');
                }

                String sql = String.Empty;
                String replyCode = statusArray[0].Replace(":", "");
                String responseCode = statusArray[2];
                String approvalCode = statusArray[4];
                String authResponse = statusArray[3];
                String TransID = statusArray[6];

                AuthorizationCode = statusArray[4];
                AuthorizationResult = rawResponseString;
                AuthorizationTransID = statusArray[6];
                AVSResult = statusArray[5];
                TransactionCommandOut = transactionCommand.ToString().Replace(X_TranKey, "*".PadLeft(X_TranKey.Length));

                if (replyCode == "1")
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
                    else
                    {
                        result = result.Replace("account", "card");
                        result = result.Replace("Account", "Card");
                        result = result.Replace("ACCOUNT", "CARD");
                    }
                }
            }
            catch
            {
                result = "Error calling eProcessing Network gateway. Please retry your order in a few minutes or select another checkout payment option.";
            }
            return result;
        }


        public override String ProcessECheck(int OrderNumber, int CustomerID, Decimal OrderTotal, Address UseBillingAddress, Address UseShippingAddress, out String AVSResult, out String AuthorizationResult, out String AuthorizationCode, out String AuthorizationTransID, out String TransactionCommandOut, out String TransactionResponse)
        {
            String result = AppLogic.ro_OK;
            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");

            ASCIIEncoding encoding = new ASCIIEncoding();
            StringBuilder transactionCommand = new StringBuilder(4096);

            transactionCommand.Append("x_method=ECHECK");
            transactionCommand.Append("&x_type=AUTH_CAPTURE"); // eCHECKS only support AUTH_CAPTURE
            transactionCommand.Append("&x_echeck_type=WEB");

            String X_TranKey = AppLogic.AppConfig("eProcessingNetwork_X_TRAN_KEY");
            if (X_TranKey.Trim().Equals("REGISTRY", StringComparison.InvariantCultureIgnoreCase))
            {
                WindowsRegistry reg = new WindowsRegistry(AppLogic.AppConfig("EncryptKey.RegistryLocation"));
                X_TranKey = reg.Read("eProcessingNetwork_X_TRAN_KEY");
                reg = null;
            }

            transactionCommand.Append("&x_login=" + AppLogic.AppConfig("eProcessingNetwork_X_LOGIN"));
            transactionCommand.Append("&x_tran_key=" + X_TranKey);
            transactionCommand.Append("&x_version=" + AppLogic.AppConfig("eProcessingNetwork_X_VERSION"));
            transactionCommand.Append("&x_merchant_EMail=" + AppLogic.AppConfig("eProcessingNetwork_X_Email"));
            transactionCommand.Append("&x_description=" + AppLogic.AppConfig("StoreName") + " Order " + OrderNumber.ToString());

            transactionCommand.Append("&x_delim_Data=" + AppLogic.AppConfig("eProcessingNetwork_X_DELIM_DATA"));
            transactionCommand.Append("&x_delim_Char=" + AppLogic.AppConfig("eProcessingNetwork_X_DELIM_CHAR"));
            transactionCommand.Append("&x_encap_char=" + AppLogic.AppConfig("eProcessingNetwork_X_ENCAP_CHAR"));
            transactionCommand.Append("&x_relay_response=" + AppLogic.AppConfig("eProcessingNetwork_X_RELAY_RESPONSE"));

            transactionCommand.Append("&x_email_customer=" + AppLogic.AppConfig("eProcessingNetwork_X_Email_CUSTOMER"));
            transactionCommand.Append("&x_recurring_billing=NO"); // for echecks

            transactionCommand.Append("&x_amount=" + Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal));
            transactionCommand.Append("&x_bank_aba_code=" + UseBillingAddress.ECheckBankABACode);
            transactionCommand.Append("&x_bank_acct_num=" + UseBillingAddress.ECheckBankAccountNumber);
            transactionCommand.Append("&x_bank_acct_type=" + UseBillingAddress.ECheckBankAccountType);
            transactionCommand.Append("&x_bank_name=" + UseBillingAddress.ECheckBankName);
            transactionCommand.Append("&x_bank_acct_name=" + UseBillingAddress.ECheckBankAccountName);
            transactionCommand.Append("&x_customer_organization_type=" + CommonLogic.IIF(UseBillingAddress.ECheckBankAccountType == "BUSINESS CHECKING", "B", "I"));

            transactionCommand.Append("&x_phone=" + UseBillingAddress.Phone);
            transactionCommand.Append("&x_fax=");
            transactionCommand.Append("&x_customer_tax_id=");
            transactionCommand.Append("&x_cust_id=" + CustomerID.ToString());
            transactionCommand.Append("&x_invoice_num=" + OrderNumber.ToString());
            transactionCommand.Append("&x_email=" + UseBillingAddress.EMail);
            transactionCommand.Append("&x_customer_ip=" + CommonLogic.CustomerIpAddress());

            transactionCommand.Append("&x_first_name=" + UseBillingAddress.FirstName);
            transactionCommand.Append("&x_last_name=" + UseBillingAddress.LastName);
            transactionCommand.Append("&x_company=" + UseBillingAddress.Company);
            transactionCommand.Append("&x_address=" + UseBillingAddress.Address1);
            transactionCommand.Append("&x_city=" + UseBillingAddress.City);
            transactionCommand.Append("&x_state=" + UseBillingAddress.State);
            transactionCommand.Append("&x_zip=" + UseBillingAddress.Zip);
            transactionCommand.Append("&x_country=" + UseBillingAddress.Country);

            if (UseShippingAddress != null)
            {
                transactionCommand.Append("&x_ship_to_first_name=" + UseShippingAddress.FirstName);
                transactionCommand.Append("&x_ship_to_last_name=" + UseShippingAddress.LastName);
                transactionCommand.Append("&x_ship_to_company=" + UseShippingAddress.Company);
                transactionCommand.Append("&x_ship_to_address=" + UseShippingAddress.Address1);
                transactionCommand.Append("&x_ship_to_city=" + UseShippingAddress.City);
                transactionCommand.Append("&x_ship_to_state=" + UseShippingAddress.State);
                transactionCommand.Append("&x_ship_to_zip=" + UseShippingAddress.Zip);
                transactionCommand.Append("&x_ship_to_country=" + UseShippingAddress.Country);
            }

            transactionCommand.Append("&x_customer_ip=" + CommonLogic.CustomerIpAddress());

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
                String AuthServer = CommonLogic.IIF(useLiveTransactions, AppLogic.AppConfig("eProcessingNetwork_LIVE_SERVER"), AppLogic.AppConfig("eProcessingNetwork_TEST_SERVER"));
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
                    rawResponseString = "0|||Error Calling eProcessing Network Payment Gateway||||||||";
                }

                // rawResponseString now has gateway response
                TransactionResponse = rawResponseString;
                String[] statusArray = rawResponseString.Split(AppLogic.AppConfig("eProcessingNetwork_X_DELIM_CHAR").ToCharArray());
                // this seems to be a new item where auth.net is returing quotes around each parameter, so strip them out:
                for (int i = statusArray.GetLowerBound(0); i <= statusArray.GetUpperBound(0); i++)
                {
                    statusArray[i] = statusArray[i].Trim('\"');
                }

                String sql = String.Empty;
                String replyCode = statusArray[0].Replace(":", "");
                String responseCode = statusArray[2];
                String approvalCode = statusArray[4];
                String authResponse = statusArray[3];
                String TransID = statusArray[6];

                AuthorizationCode = statusArray[4];
                AuthorizationResult = rawResponseString;
                AuthorizationTransID = statusArray[6];
                AVSResult = statusArray[5];
                TransactionCommandOut = transactionCommand.ToString().Replace(X_TranKey, "*".PadLeft(X_TranKey.Length));

                if (replyCode == "1")
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
                }
            }
            catch
            {
                result = "Error calling eProcessing Network gateway. Please retry your order in a few minutes or select another checkout payment option.";
            }
            return result;
        }

        public override bool SupportsEChecks()
        {
            return true;
        }
    }
}
