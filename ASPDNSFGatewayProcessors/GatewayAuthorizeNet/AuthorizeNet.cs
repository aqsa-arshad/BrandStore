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
using System.Xml;
using AspDotNetStorefrontCommon;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways;
using AspDotNetStorefrontGateways.Processors;
using System.Collections.Generic;

namespace AspDotNetStorefrontGateways.Processors
{
    public class AuthorizeNet : GatewayProcessor
    {
		public Boolean IsCimEnabled
		{
			get
			{
				return AppLogic.AppConfig("PaymentGateway").ToLower() == "authorizenet"
					&& AppLogic.AppConfigBool("AUTHORIZENET_Cim_Enabled")
					&& !String.IsNullOrEmpty(AppLogic.AppConfig("AUTHORIZENET_X_Login"))
					&& !String.IsNullOrEmpty(AppLogic.AppConfig("AUTHORIZENET_X_Tran_Key"));
			}
		}

        public override string AdministratorSetupPrompt
        {
            get
            {
                return "Now includes CIM";
            }
        }


        public override string CaptureOrder(Order o)
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

            String X_Login = AppLogic.AppConfig("AUTHORIZENET_X_LOGIN");

            if (X_Login.Trim().Equals("REGISTRY", StringComparison.InvariantCultureIgnoreCase))
            {
                WindowsRegistry reg = new WindowsRegistry(AppLogic.AppConfig("EncryptKey.RegistryLocation"));
                X_Login = reg.Read("AUTHORIZENET_X_LOGIN");
                reg = null;
            }

            String X_TranKey = AppLogic.AppConfig("AUTHORIZENET_X_TRAN_KEY");
            if (X_TranKey.Trim().Equals("REGISTRY", StringComparison.InvariantCultureIgnoreCase))
            {
                WindowsRegistry reg = new WindowsRegistry(AppLogic.AppConfig("EncryptKey.RegistryLocation"));
                X_TranKey = reg.Read("AUTHORIZENET_X_TRAN_KEY");
                reg = null;
            }

            transactionCommand.Append("&x_login=" + X_Login);
            transactionCommand.Append("&x_tran_key=" + X_TranKey);
            transactionCommand.Append("&x_version=" + AppLogic.AppConfig("AUTHORIZENET_X_VERSION"));
            transactionCommand.Append("&x_test_request=" + CommonLogic.IIF(useLiveTransactions, "FALSE", "TRUE"));
            transactionCommand.Append("&x_method=" + AppLogic.AppConfig("AUTHORIZENET_X_METHOD"));
            transactionCommand.Append("&x_delim_Data=" + AppLogic.AppConfig("AUTHORIZENET_X_DELIM_DATA"));
            transactionCommand.Append("&x_delim_Char=" + AppLogic.AppConfig("AUTHORIZENET_X_DELIM_CHAR"));
            transactionCommand.Append("&x_encap_char=" + AppLogic.AppConfig("AUTHORIZENET_X_ENCAP_CHAR"));
            transactionCommand.Append("&x_relay_response=" + AppLogic.AppConfig("AUTHORIZENET_X_RELAY_RESPONSE"));
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
                String AuthServer = CommonLogic.IIF(useLiveTransactions, AppLogic.AppConfig("AUTHORIZENET_LIVE_SERVER"), AppLogic.AppConfig("AUTHORIZENET_TEST_SERVER"));
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
                    rawResponseString = "0|||Error Calling AuthorizeNet Payment Gateway||||||||";
                }

                // rawResponseString now has gateway response
                String[] statusArray = rawResponseString.Split(AppLogic.AppConfig("AUTHORIZENET_X_DELIM_CHAR").ToCharArray());
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

        public override string VoidOrder(int OrderNumber)
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

            String X_Login = AppLogic.AppConfig("AUTHORIZENET_X_LOGIN");
            if (X_Login.Trim().Equals("REGISTRY", StringComparison.InvariantCultureIgnoreCase))
            {
                WindowsRegistry reg = new WindowsRegistry(AppLogic.AppConfig("EncryptKey.RegistryLocation"));
                X_Login = reg.Read("AUTHORIZENET_X_LOGIN");
                reg = null;
            }

            String X_TranKey = AppLogic.AppConfig("AUTHORIZENET_X_TRAN_KEY");
            if (X_TranKey.Trim().Equals("REGISTRY", StringComparison.InvariantCultureIgnoreCase))
            {
                WindowsRegistry reg = new WindowsRegistry(AppLogic.AppConfig("EncryptKey.RegistryLocation"));
                X_TranKey = reg.Read("AUTHORIZENET_X_TRAN_KEY");
                reg = null;
            }


            ASCIIEncoding encoding = new ASCIIEncoding();
            StringBuilder transactionCommand = new StringBuilder(4096);
            transactionCommand.Append("x_type=VOID");
            transactionCommand.Append("&x_login=" + X_Login);
            transactionCommand.Append("&x_tran_key=" + X_TranKey);
            transactionCommand.Append("&x_version=" + AppLogic.AppConfig("AUTHORIZENET_X_VERSION"));
            transactionCommand.Append("&x_test_request=" + CommonLogic.IIF(useLiveTransactions, "FALSE", "TRUE"));
            transactionCommand.Append("&x_method=" + AppLogic.AppConfig("AUTHORIZENET_X_METHOD"));
            transactionCommand.Append("&x_delim_Data=" + AppLogic.AppConfig("AUTHORIZENET_X_DELIM_DATA"));
            transactionCommand.Append("&x_delim_Char=" + AppLogic.AppConfig("AUTHORIZENET_X_DELIM_CHAR"));
            transactionCommand.Append("&x_encap_char=" + AppLogic.AppConfig("AUTHORIZENET_X_ENCAP_CHAR"));
            transactionCommand.Append("&x_relay_response=" + AppLogic.AppConfig("AUTHORIZENET_X_RELAY_RESPONSE"));
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
                    String AuthServer = CommonLogic.IIF(useLiveTransactions, AppLogic.AppConfig("AUTHORIZENET_LIVE_SERVER"), AppLogic.AppConfig("AUTHORIZENET_TEST_SERVER"));
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
                        rawResponseString = "0|||Error Calling Authorize.Net Payment Gateway||||||||";
                    }

                    // rawResponseString now has gateway response
                    String[] statusArray = rawResponseString.Split(AppLogic.AppConfig("AUTHORIZENET_X_DELIM_CHAR").ToCharArray());
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

        public override string RefundOrder(int OriginalOrderNumber, int NewOrderNumber, decimal RefundAmount, string RefundReason, Address UseBillingAddress)
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

            String X_Login = AppLogic.AppConfig("AUTHORIZENET_X_LOGIN");
            if (X_Login.Trim().Equals("REGISTRY", StringComparison.InvariantCultureIgnoreCase))
            {
                WindowsRegistry reg = new WindowsRegistry(AppLogic.AppConfig("EncryptKey.RegistryLocation"));
                X_Login = reg.Read("AUTHORIZENET_X_LOGIN");
                reg = null;
            }

            String X_TranKey = AppLogic.AppConfig("AUTHORIZENET_X_TRAN_KEY");
            if (X_TranKey.Trim().Equals("REGISTRY", StringComparison.InvariantCultureIgnoreCase))
            {
                WindowsRegistry reg = new WindowsRegistry(AppLogic.AppConfig("EncryptKey.RegistryLocation"));
                X_TranKey = reg.Read("AUTHORIZENET_X_TRAN_KEY");
                reg = null;
            }

            ASCIIEncoding encoding = new ASCIIEncoding();
            StringBuilder transactionCommand = new StringBuilder(4096);
            transactionCommand.Append("x_type=CREDIT");
            transactionCommand.Append("&x_login=" + X_Login);
            transactionCommand.Append("&x_tran_key=" + X_TranKey);
            transactionCommand.Append("&x_version=" + AppLogic.AppConfig("AUTHORIZENET_X_VERSION"));
            transactionCommand.Append("&x_test_request=" + CommonLogic.IIF(useLiveTransactions, "FALSE", "TRUE"));
            transactionCommand.Append("&x_method=" + AppLogic.AppConfig("AUTHORIZENET_X_METHOD"));
            transactionCommand.Append("&x_delim_Data=" + AppLogic.AppConfig("AUTHORIZENET_X_DELIM_DATA"));
            transactionCommand.Append("&x_delim_Char=" + AppLogic.AppConfig("AUTHORIZENET_X_DELIM_CHAR"));
            transactionCommand.Append("&x_encap_char=" + AppLogic.AppConfig("AUTHORIZENET_X_ENCAP_CHAR"));
            transactionCommand.Append("&x_relay_response=" + AppLogic.AppConfig("AUTHORIZENET_X_RELAY_RESPONSE"));
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
            transactionCommand.Append("&x_email=" + HttpContext.Current.Server.UrlEncode(BillingEMail));
            transactionCommand.Append("&x_email_customer=false");
            transactionCommand.Append("&x_customer_ip=" + CommonLogic.CustomerIpAddress());
            transactionCommand.Append("&x_card_num=" + Last4);

            transactionCommand.Append("&x_description=" + HttpContext.Current.Server.UrlEncode(RefundReason));
            transactionCommand.Append("&x_first_name=" + HttpContext.Current.Server.UrlEncode(BillingFirstName));
            transactionCommand.Append("&x_last_name=" + HttpContext.Current.Server.UrlEncode(BillingLastName));
            transactionCommand.Append("&x_company=" + HttpContext.Current.Server.UrlEncode(BillingCompany));
            transactionCommand.Append("&x_address=" + HttpContext.Current.Server.UrlEncode(BillingAddress1));
            transactionCommand.Append("&x_city=" + HttpContext.Current.Server.UrlEncode(BillingCity));
            transactionCommand.Append("&x_state=" + HttpContext.Current.Server.UrlEncode(BillingState));
            transactionCommand.Append("&x_zip=" + HttpContext.Current.Server.UrlEncode(BillingZip));
            transactionCommand.Append("&x_country=" + HttpContext.Current.Server.UrlEncode(BillingCountry));
            transactionCommand.Append("&x_phone=" + HttpContext.Current.Server.UrlEncode(BillingPhone));

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
                    String AuthServer = CommonLogic.IIF(useLiveTransactions, AppLogic.AppConfig("AUTHORIZENET_LIVE_SERVER"), AppLogic.AppConfig("AUTHORIZENET_TEST_SERVER"));
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
                        rawResponseString = "0|||Error Calling Authorize.Net Payment Gateway||||||||";
                    }

                    // rawResponseString now has gateway response
                    String[] statusArray = rawResponseString.Split(AppLogic.AppConfig("AUTHORIZENET_X_DELIM_CHAR").ToCharArray());
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

        public override string ProcessCard(int OrderNumber, int CustomerID, decimal OrderTotal, bool useLiveTransactions, TransactionModeEnum TransactionMode, Address UseBillingAddress, string CardExtraCode, Address UseShippingAddress, string CAVV, string ECI, string XID, out string AVSResult, out string AuthorizationResult, out string AuthorizationCode, out string AuthorizationTransID, out string TransactionCommandOut, out string TransactionResponse)
        {
			String result = AppLogic.ro_OK;
			AuthorizationCode = String.Empty;
			AuthorizationResult = String.Empty;
			AuthorizationTransID = String.Empty;
			AVSResult = String.Empty;
			TransactionCommandOut = String.Empty;
			TransactionResponse = String.Empty;
			Customer thisCustomer = Customer.Current;

			// Switch to CIM if enabled
			if (IsCimEnabled && !String.IsNullOrEmpty(thisCustomer.ThisCustomerSession["ActivePaymentProfileId"]))
			{
				Int64 profileId = Int64.Parse(thisCustomer.ThisCustomerSession["ActivePaymentProfileId"]);
				
				result = GatewayAuthorizeNet.ProcessTools.ProcessCard(OrderNumber, CustomerID, OrderTotal, profileId,
					TransactionMode.ToString(), useLiveTransactions, out  AVSResult, out  AuthorizationResult,
					out  AuthorizationCode, out  AuthorizationTransID, out TransactionCommandOut, out TransactionResponse);
				
				if(result == AppLogic.ro_OK)
					thisCustomer.ThisCustomerSession["ActivePaymentProfileId"] = string.Empty;
				
				return result;
			}

            String X_Login = AppLogic.AppConfig("AUTHORIZENET_X_LOGIN");
            if (X_Login.Trim().Equals("REGISTRY", StringComparison.InvariantCultureIgnoreCase))
            {
                WindowsRegistry reg = new WindowsRegistry(AppLogic.AppConfig("EncryptKey.RegistryLocation"));
                X_Login = reg.Read("AUTHORIZENET_X_LOGIN");
                reg = null;
            }

            String X_TranKey = AppLogic.AppConfig("AUTHORIZENET_X_TRAN_KEY");
            if (X_TranKey.Trim().Equals("REGISTRY", StringComparison.InvariantCultureIgnoreCase))
            {
                WindowsRegistry reg = new WindowsRegistry(AppLogic.AppConfig("EncryptKey.RegistryLocation"));
                X_TranKey = reg.Read("AUTHORIZENET_X_TRAN_KEY");
                reg = null;
            }

            ASCIIEncoding encoding = new ASCIIEncoding();
            StringBuilder transactionCommand = new StringBuilder(4096);

            transactionCommand.Append("x_type=" + CommonLogic.IIF(TransactionMode == TransactionModeEnum.auth, "AUTH_ONLY", "AUTH_CAPTURE"));

            transactionCommand.Append("&x_login=" + X_Login);
            transactionCommand.Append("&x_tran_key=" + X_TranKey);
            transactionCommand.Append("&x_version=" + AppLogic.AppConfig("AUTHORIZENET_X_VERSION"));
            transactionCommand.Append("&x_test_request=" + CommonLogic.IIF(useLiveTransactions, "FALSE", "TRUE"));
            transactionCommand.Append("&x_merchant_email=" + HttpContext.Current.Server.UrlEncode(AppLogic.AppConfig("AUTHORIZENET_X_Email")));
            transactionCommand.Append("&x_description=" + HttpContext.Current.Server.UrlEncode(AppLogic.AppConfig("StoreName") + " Order " + OrderNumber.ToString()));

            transactionCommand.Append("&x_method=" + AppLogic.AppConfig("AUTHORIZENET_X_METHOD"));

            transactionCommand.Append("&x_delim_Data=" + AppLogic.AppConfig("AUTHORIZENET_X_DELIM_DATA"));
            transactionCommand.Append("&x_delim_Char=" + AppLogic.AppConfig("AUTHORIZENET_X_DELIM_CHAR"));
            transactionCommand.Append("&x_encap_char=" + AppLogic.AppConfig("AUTHORIZENET_X_ENCAP_CHAR"));
            transactionCommand.Append("&x_relay_response=" + AppLogic.AppConfig("AUTHORIZENET_X_RELAY_RESPONSE"));

            transactionCommand.Append("&x_email_customer=" + AppLogic.AppConfig("AUTHORIZENET_X_Email_CUSTOMER"));
            transactionCommand.Append("&x_recurring_billing=" + AppLogic.AppConfig("AUTHORIZENET_X_RECURRING_BILLING"));

            transactionCommand.Append("&x_amount=" + Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal));
            transactionCommand.Append("&x_card_num=" + UseBillingAddress.CardNumber);
            if (CardExtraCode.Length != 0)
            {
                transactionCommand.Append("&x_card_code=" + CardExtraCode.Trim());
            }

            transactionCommand.Append("&x_exp_date=" + UseBillingAddress.CardExpirationMonth.PadLeft(2, '0') + "/" + UseBillingAddress.CardExpirationYear);
            transactionCommand.Append("&x_phone=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.Phone));
            transactionCommand.Append("&x_fax=");
            transactionCommand.Append("&x_customer_tax_id=");
            transactionCommand.Append("&x_cust_id=" + CustomerID.ToString());
            transactionCommand.Append("&x_invoice_num=" + OrderNumber.ToString());
            transactionCommand.Append("&x_email=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.EMail));
            transactionCommand.Append("&x_customer_ip=" + CommonLogic.CustomerIpAddress());

            transactionCommand.Append("&x_first_name=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.FirstName));
            transactionCommand.Append("&x_last_name=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.LastName));
            transactionCommand.Append("&x_company=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.Company));
            transactionCommand.Append("&x_address=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.Address1));
            transactionCommand.Append("&x_city=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.City));
            transactionCommand.Append("&x_state=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.State));
            transactionCommand.Append("&x_zip=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.Zip));
            transactionCommand.Append("&x_country=" + HttpContext.Current.Server.UrlEncode(UseBillingAddress.Country));

            if (UseShippingAddress != null)
            {
                transactionCommand.Append("&x_ship_to_first_name=" + HttpContext.Current.Server.UrlEncode(UseShippingAddress.FirstName));
                transactionCommand.Append("&x_ship_to_last_name=" + HttpContext.Current.Server.UrlEncode(UseShippingAddress.LastName));
                transactionCommand.Append("&x_ship_to_company=" + HttpContext.Current.Server.UrlEncode(UseShippingAddress.Company));
                transactionCommand.Append("&x_ship_to_address=" + HttpContext.Current.Server.UrlEncode(UseShippingAddress.Address1));
                transactionCommand.Append("&x_ship_to_city=" + HttpContext.Current.Server.UrlEncode(UseShippingAddress.City));
                transactionCommand.Append("&x_ship_to_state=" + HttpContext.Current.Server.UrlEncode(UseShippingAddress.State));
                transactionCommand.Append("&x_ship_to_zip=" + HttpContext.Current.Server.UrlEncode(UseShippingAddress.Zip));
                transactionCommand.Append("&x_ship_to_country=" + HttpContext.Current.Server.UrlEncode(UseShippingAddress.Country));
            }

            transactionCommand.Append("&x_customer_ip=" + CommonLogic.CustomerIpAddress());

            if (ECI.Length != 0)
            {
                transactionCommand.Append("&x_authentication_indicator=" + ECI);
                transactionCommand.Append("&x_cardholder_authentication_value=" + CAVV);
            }

            byte[] data = encoding.GetBytes(transactionCommand.ToString());

            // Prepare web request...

                String AuthServer = CommonLogic.IIF(useLiveTransactions, AppLogic.AppConfig("AUTHORIZENET_LIVE_SERVER"), AppLogic.AppConfig("AUTHORIZENET_TEST_SERVER"));
                String rawResponseString = String.Empty;

                int MaxTries = AppLogic.AppConfigUSInt("GatewayRetries") + 1;
                int CurrentTry = 0;

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

                        myResponse = myRequest.GetResponse();
                        using (StreamReader sr = new StreamReader(myResponse.GetResponseStream()))
                        {
                            rawResponseString = sr.ReadToEnd();
                            sr.Close();
                        }
                        myResponse.Close();
                        //CallSuccessful = true;


                // rawResponseString now has gateway response
                TransactionResponse = rawResponseString;
                String[] statusArray = rawResponseString.Split(AppLogic.AppConfig("AUTHORIZENET_X_DELIM_CHAR").ToCharArray());
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


            return result;
        }

        public override string ProcessECheck(int OrderNumber, int CustomerID, decimal OrderTotal, Address UseBillingAddress, Address UseShippingAddress, out string AVSResult, out string AuthorizationResult, out string AuthorizationCode, out string AuthorizationTransID, out string TransactionCommandOut, out string TransactionResponse)
        {
            String result = AppLogic.ro_OK;
            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");

            ASCIIEncoding encoding = new ASCIIEncoding();
            StringBuilder transactionCommand = new StringBuilder(4096);

            transactionCommand.Append("x_method=ECHECK");
            transactionCommand.Append("&x_type=AUTH_CAPTURE"); // eCHECKS only support AUTH_CAPTURE
            transactionCommand.Append("&x_echeck_type=WEB");

            String X_TranKey = AppLogic.AppConfig("AUTHORIZENET_X_TRAN_KEY");
            if (X_TranKey.Trim().Equals("REGISTRY", StringComparison.InvariantCultureIgnoreCase))
            {
                WindowsRegistry reg = new WindowsRegistry(AppLogic.AppConfig("EncryptKey.RegistryLocation"));
                X_TranKey = reg.Read("AUTHORIZENET_X_TRAN_KEY");
                reg = null;
            }

            transactionCommand.Append("&x_login=" + AppLogic.AppConfig("AUTHORIZENET_X_LOGIN"));
            transactionCommand.Append("&x_tran_key=" + X_TranKey);
            transactionCommand.Append("&x_version=" + AppLogic.AppConfig("AUTHORIZENET_X_VERSION"));
            transactionCommand.Append("&x_merchant_EMail=" + AppLogic.AppConfig("AUTHORIZENET_X_Email"));
            transactionCommand.Append("&x_description=" + AppLogic.AppConfig("StoreName") + " Order " + OrderNumber.ToString());

            transactionCommand.Append("&x_delim_Data=" + AppLogic.AppConfig("AUTHORIZENET_X_DELIM_DATA"));
            transactionCommand.Append("&x_delim_Char=" + AppLogic.AppConfig("AUTHORIZENET_X_DELIM_CHAR"));
            transactionCommand.Append("&x_encap_char=" + AppLogic.AppConfig("AUTHORIZENET_X_ENCAP_CHAR"));
            transactionCommand.Append("&x_relay_response=" + AppLogic.AppConfig("AUTHORIZENET_X_RELAY_RESPONSE"));

            transactionCommand.Append("&x_email_customer=" + AppLogic.AppConfig("AUTHORIZENET_X_Email_CUSTOMER"));
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
                String AuthServer = CommonLogic.IIF(useLiveTransactions, AppLogic.AppConfig("AUTHORIZENET_LIVE_SERVER"), AppLogic.AppConfig("AUTHORIZENET_TEST_SERVER"));
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
                    rawResponseString = "0|||Error Calling Authorize.NetPayment Gateway||||||||";
                }

                // rawResponseString now has gateway response
                TransactionResponse = rawResponseString;
                String[] statusArray = rawResponseString.Split(AppLogic.AppConfig("AUTHORIZENET_X_DELIM_CHAR").ToCharArray());
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
                result = "Error calling Authorize.net gateway. Please retry your order in a few minutes or select another checkout payment option.";
            }
            return result;
        }

        public override string RecurringBillingCreateSubscription(string SubscriptionDescription, Customer ThisCustomer, Address UseBillingAddress, Address UseShippingAddress, decimal RecurringAmount, DateTime StartDate, int RecurringInterval, DateIntervalTypeEnum RecurringIntervalType, int OriginalRecurringOrderNumber, string XID, IDictionary<string, string> TransactionContext, out string RecurringSubscriptionID, out string RecurringSubscriptionCommand, out string RecurringSubscriptionResult)
        {
            String result = AppLogic.ro_OK;
            RecurringSubscriptionID = String.Empty;
            RecurringSubscriptionCommand = String.Empty;
            RecurringSubscriptionResult = String.Empty;
            String ResultCode = String.Empty;

            String X_Login = AppLogic.AppConfig("AUTHORIZENET_X_LOGIN");
            if (X_Login.Trim().Equals("REGISTRY", StringComparison.InvariantCultureIgnoreCase))
            {
                WindowsRegistry reg = new WindowsRegistry(AppLogic.AppConfig("EncryptKey.RegistryLocation"));
                X_Login = reg.Read("AUTHORIZENET_X_LOGIN");
                reg = null;
            }

            String X_TranKey = AppLogic.AppConfig("AUTHORIZENET_X_TRAN_KEY");
            if (X_TranKey.Trim().Equals("REGISTRY", StringComparison.InvariantCultureIgnoreCase))
            {
                WindowsRegistry reg = new WindowsRegistry(AppLogic.AppConfig("EncryptKey.RegistryLocation"));
                X_TranKey = reg.Read("AUTHORIZENET_X_TRAN_KEY");
                reg = null;
            }
            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");
            String AuthServer = CommonLogic.IIF(useLiveTransactions, AppLogic.AppConfig("Recurring.AUTHORIZENET_AUTOBILL_LIVE_SERVER"), AppLogic.AppConfig("Recurring.AUTHORIZENET_AUTOBILL_TEST_SERVER"));

            // This is the API interface object.
            AuthorizeNetRecurringAPI.ARBCreateSubscriptionRequest request = new AuthorizeNetRecurringAPI.ARBCreateSubscriptionRequest();

            // Include authentication information
            request.merchantAuthentication = new AuthorizeNetRecurringAPI.MerchantAuthentication();
            request.merchantAuthentication.name = X_Login;
            request.merchantAuthentication.transactionKey = X_TranKey;
            request.refId = OriginalRecurringOrderNumber.ToString();

            // Populate the subscription request with data
            AuthorizeNetRecurringAPI.ARBSubscription sub = new AuthorizeNetRecurringAPI.ARBSubscription();
            AuthorizeNetRecurringAPI.CreditCard creditCard = new AuthorizeNetRecurringAPI.CreditCard();

            sub.name = AppLogic.AppConfig("StoreName") + " - Order " + OriginalRecurringOrderNumber.ToString();

            creditCard.cardNumber = UseBillingAddress.CardNumber;
            creditCard.expirationDate = UseBillingAddress.CardExpirationYear + "-" + UseBillingAddress.CardExpirationMonth.PadLeft(2, '0');  // required format for API is YYYY-MM

            sub.payment = new AuthorizeNetRecurringAPI.Payment();
            sub.payment.item = creditCard;

            sub.billTo = new AuthorizeNetRecurringAPI.NameAndAddress();
            sub.billTo.firstName = UseBillingAddress.FirstName;
            sub.billTo.lastName = UseBillingAddress.LastName;
            sub.billTo.company = UseBillingAddress.Company;
            sub.billTo.address = UseBillingAddress.Address1;
            sub.billTo.city = UseBillingAddress.City;
            sub.billTo.state = UseBillingAddress.State;
            sub.billTo.zip = UseBillingAddress.Zip;
            sub.billTo.country = UseBillingAddress.Country;

            sub.shipTo = new AuthorizeNetRecurringAPI.NameAndAddress();
            sub.shipTo.firstName = UseShippingAddress.FirstName;
            sub.shipTo.lastName = UseShippingAddress.LastName;
            sub.shipTo.company = UseShippingAddress.Company;
            sub.shipTo.address = UseShippingAddress.Address1;
            sub.shipTo.city = UseShippingAddress.City;
            sub.shipTo.state = UseShippingAddress.State;
            sub.shipTo.zip = UseShippingAddress.Zip;
            sub.shipTo.country = UseShippingAddress.Country;

            sub.order = new AuthorizeNetRecurringAPI.Order();
            sub.order.invoiceNumber = OriginalRecurringOrderNumber.ToString();
            sub.order.description = SubscriptionDescription;
            sub.orderSpecified = true;

            sub.customer = new AuthorizeNetRecurringAPI.Customer();
            sub.customer.email = CommonLogic.IIF(ThisCustomer.EMail.Length != 0, ThisCustomer.EMail, UseBillingAddress.EMail);
            sub.customer.id = ThisCustomer.CustomerID.ToString();
            sub.customer.phoneNumber = UseBillingAddress.Phone;
            sub.customer.type = CommonLogic.IIF(UseBillingAddress.ResidenceType == ResidenceTypes.Residential, "individual", "business");
            sub.customerSpecified = true;

            sub.paymentSchedule = new AuthorizeNetRecurringAPI.PaymentSchedule();
            sub.paymentSchedule.startDate = StartDate.Year.ToString() + "-" + StartDate.Month.ToString().PadLeft(2, '0') + "-" + StartDate.Day.ToString().PadLeft(2, '0');   // Required format is YYYY-MM-DD

            sub.amount = RecurringAmount;
            sub.amountSpecified = true;

            // NOTE: since our recurring products have no sunset, set max gateway allowed total occurrences to provide maxmimum
            // length of subscription time
            switch (RecurringIntervalType)
            {
                case DateIntervalTypeEnum.Day:
                    if (RecurringInterval < 7)
                    {
                        result = "The minimum interval for Authorize.Net Recurring Billing is 7 days";
                        return result;
                    }
                    sub.paymentSchedule.PaymentScheduleInterval.length = RecurringInterval;
                    sub.paymentSchedule.PaymentScheduleInterval.unit = AuthorizeNetRecurringAPI.SubscriptionUnitType.days;
                    sub.paymentSchedule.totalOccurrences = 9999; // According to docs, set to 9999 for no end date
                    sub.paymentSchedule.totalOccurrencesSpecified = true;
                    break;
                case DateIntervalTypeEnum.Week:
                    sub.paymentSchedule.PaymentScheduleInterval.length = RecurringInterval * 7;
                    sub.paymentSchedule.PaymentScheduleInterval.unit = AuthorizeNetRecurringAPI.SubscriptionUnitType.days;
                    sub.paymentSchedule.totalOccurrences = 9999;
                    sub.paymentSchedule.totalOccurrencesSpecified = true;
                    break;
                case DateIntervalTypeEnum.Month:
                    sub.paymentSchedule.PaymentScheduleInterval.length = RecurringInterval;
                    sub.paymentSchedule.PaymentScheduleInterval.unit = AuthorizeNetRecurringAPI.SubscriptionUnitType.months;
                    sub.paymentSchedule.totalOccurrences = 9999;
                    sub.paymentSchedule.totalOccurrencesSpecified = true;
                    break;
                case DateIntervalTypeEnum.Year:
                    sub.paymentSchedule.PaymentScheduleInterval.length = RecurringInterval * 12;
                    sub.paymentSchedule.PaymentScheduleInterval.unit = AuthorizeNetRecurringAPI.SubscriptionUnitType.months;
                    sub.paymentSchedule.totalOccurrences = 9999;
                    sub.paymentSchedule.totalOccurrencesSpecified = true;
                    break;
                case DateIntervalTypeEnum.Weekly:
                    RecurringInterval = 1;
                    sub.paymentSchedule.PaymentScheduleInterval.length = RecurringInterval * 7;
                    sub.paymentSchedule.PaymentScheduleInterval.unit = AuthorizeNetRecurringAPI.SubscriptionUnitType.days;
                    sub.paymentSchedule.totalOccurrences = 9999;
                    sub.paymentSchedule.totalOccurrencesSpecified = true;
                    break;
                case DateIntervalTypeEnum.BiWeekly:
                    RecurringInterval = 2;
                    sub.paymentSchedule.PaymentScheduleInterval.length = RecurringInterval * 7;
                    sub.paymentSchedule.PaymentScheduleInterval.unit = AuthorizeNetRecurringAPI.SubscriptionUnitType.days;
                    sub.paymentSchedule.totalOccurrences = 9999;
                    sub.paymentSchedule.totalOccurrencesSpecified = true;
                    break;
                //case DateIntervalTypeEnum.SemiMonthly:
                //    RecurringInterval = 2;
                //    sub.paymentSchedule.PaymentScheduleInterval.length = RecurringInterval * 7;
                //    sub.paymentSchedule.PaymentScheduleInterval.unit = AuthorizeNetRecurringAPI.SubscriptionUnitType.days;
                //    sub.paymentSchedule.totalOccurrences = 9999;
                //    sub.paymentSchedule.totalOccurrencesSpecified = true;
                //    break;
                case DateIntervalTypeEnum.EveryFourWeeks:
                    RecurringInterval = 4;
                    sub.paymentSchedule.PaymentScheduleInterval.length = RecurringInterval * 7;
                    sub.paymentSchedule.PaymentScheduleInterval.unit = AuthorizeNetRecurringAPI.SubscriptionUnitType.days;
                    sub.paymentSchedule.totalOccurrences = 9999;
                    sub.paymentSchedule.totalOccurrencesSpecified = true;
                    break;
                case DateIntervalTypeEnum.Monthly:
                    RecurringInterval = 1;
                    sub.paymentSchedule.PaymentScheduleInterval.length = RecurringInterval;
                    sub.paymentSchedule.PaymentScheduleInterval.unit = AuthorizeNetRecurringAPI.SubscriptionUnitType.months;
                    sub.paymentSchedule.totalOccurrences = 9999;
                    sub.paymentSchedule.totalOccurrencesSpecified = true;
                    break;
                case DateIntervalTypeEnum.Quarterly:
                    RecurringInterval = 3;
                    sub.paymentSchedule.PaymentScheduleInterval.length = RecurringInterval;
                    sub.paymentSchedule.PaymentScheduleInterval.unit = AuthorizeNetRecurringAPI.SubscriptionUnitType.months;
                    sub.paymentSchedule.totalOccurrences = 9999;
                    sub.paymentSchedule.totalOccurrencesSpecified = true;
                    break;
                case DateIntervalTypeEnum.SemiYearly:
                    RecurringInterval = 6;
                    sub.paymentSchedule.PaymentScheduleInterval.length = RecurringInterval;
                    sub.paymentSchedule.PaymentScheduleInterval.unit = AuthorizeNetRecurringAPI.SubscriptionUnitType.months;
                    sub.paymentSchedule.totalOccurrences = 9999;
                    sub.paymentSchedule.totalOccurrencesSpecified = true;
                    break;
                case DateIntervalTypeEnum.Yearly:
                    RecurringInterval = 12;
                    sub.paymentSchedule.PaymentScheduleInterval.length = RecurringInterval;
                    sub.paymentSchedule.PaymentScheduleInterval.unit = AuthorizeNetRecurringAPI.SubscriptionUnitType.months;
                    sub.paymentSchedule.totalOccurrences = 9999;
                    sub.paymentSchedule.totalOccurrencesSpecified = true;
                    break;
                default:
                    RecurringInterval = 1;
                    sub.paymentSchedule.PaymentScheduleInterval.length = RecurringInterval;
                    sub.paymentSchedule.PaymentScheduleInterval.unit = AuthorizeNetRecurringAPI.SubscriptionUnitType.months;
                    sub.paymentSchedule.totalOccurrences = 9999;
                    sub.paymentSchedule.totalOccurrencesSpecified = true;
                    break;
            }
            sub.paymentSchedule.intervalSpecified = true;

            // Any free trial?
            sub.paymentSchedule.trialOccurrences = 0;
            sub.paymentSchedule.trialOccurrencesSpecified = false;
            sub.trialAmount = System.Decimal.Zero;
            sub.trialAmountSpecified = false; // we don't support free trials at this time

            request.subscription = sub;

            RecurringSubscriptionCommand = XmlCommon.SerializeObject(request, request.GetType());
            // wipe sensitive data:
            if (RecurringSubscriptionCommand.Length != 0)
            {
                RecurringSubscriptionCommand = RecurringSubscriptionCommand.Replace(UseBillingAddress.CardNumber, AppLogic.SafeDisplayCardNumber(UseBillingAddress.CardNumber, "Orders", 0));
                RecurringSubscriptionCommand = RecurringSubscriptionCommand.Replace(X_TranKey, "*".PadLeft(X_TranKey.Length, '*'));
            }

            // The response type will normally be ARBCreateSubscriptionResponse.
            // However, in the case of an error such as an XML parsing error, the response type will be ErrorResponse.

            object response = null;
            XmlDocument xmldoc = null;
            result = AuthorizeNetRecurringAPI.APIHelper.PostRequest(request, AuthServer, out xmldoc);

            if (xmldoc != null)
            {
                RecurringSubscriptionResult = xmldoc.InnerXml;
                // wipe sensitive data:
                if (RecurringSubscriptionResult.Length != 0)
                {
                    RecurringSubscriptionResult = RecurringSubscriptionResult.Replace(UseBillingAddress.CardNumber, AppLogic.SafeDisplayCardNumber(UseBillingAddress.CardNumber, "Orders", 0));
                    RecurringSubscriptionResult = RecurringSubscriptionResult.Replace(X_TranKey, "*".PadLeft(X_TranKey.Length, '*'));
                }

            }

            if (result.Equals(AppLogic.ro_OK, StringComparison.InvariantCultureIgnoreCase))
            {
                result = AuthorizeNetRecurringAPI.APIHelper.ProcessXmlResponse(xmldoc, out response);
            }

            if (result.Equals(AppLogic.ro_OK, StringComparison.InvariantCultureIgnoreCase))
            {
                result = AuthorizeNetRecurringAPI.APIHelper.ProcessResponse(response, out ResultCode, out RecurringSubscriptionID);
            }

            if (result != AppLogic.ro_OK)
            {
                // Log failure
                DB.ExecuteSQL("insert into FailedTransaction(CustomerID,OrderNumber,IPAddress,OrderDate,PaymentGateway,PaymentMethod,TransactionCommand,TransactionResult,CustomerEMailed,RecurringSubscriptionID) values(" +
                    ThisCustomer.CustomerID.ToString() + "," + OriginalRecurringOrderNumber.ToString() + "," +
                    DB.SQuote(ThisCustomer.LastIPAddress) + ",getdate()," + DB.SQuote("AUTHORIZENET") + "," +
                    DB.SQuote(AppLogic.TransactionTypeEnum.RECURRING_AUTO.ToString()) + "," + DB.SQuote(RecurringSubscriptionCommand) + "," + DB.SQuote(RecurringSubscriptionResult) + ",0," + DB.SQuote(RecurringSubscriptionID) + ")");
            }

            return result;
        }

        public override string RecurringBillingCancelSubscription(string RecurringSubscriptionID, int OriginalRecurringOrderNumber, IDictionary<string, string> TransactionContext)
        {
            String result = AppLogic.ro_OK;

            String X_Login = AppLogic.AppConfig("AUTHORIZENET_X_LOGIN");
            if (X_Login.Trim().Equals("REGISTRY", StringComparison.InvariantCultureIgnoreCase))
            {
                WindowsRegistry reg = new WindowsRegistry(AppLogic.AppConfig("EncryptKey.RegistryLocation"));
                X_Login = reg.Read("AUTHORIZENET_X_LOGIN");
                reg = null;
            }

            String X_TranKey = AppLogic.AppConfig("AUTHORIZENET_X_TRAN_KEY");
            if (X_TranKey.Trim().Equals("REGISTRY", StringComparison.InvariantCultureIgnoreCase))
            {
                WindowsRegistry reg = new WindowsRegistry(AppLogic.AppConfig("EncryptKey.RegistryLocation"));
                X_TranKey = reg.Read("AUTHORIZENET_X_TRAN_KEY");
                reg = null;
            }
            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");
            String AuthServer = CommonLogic.IIF(useLiveTransactions, AppLogic.AppConfig("Recurring.AUTHORIZENET_AUTOBILL_LIVE_SERVER"), AppLogic.AppConfig("Recurring.AUTHORIZENET_AUTOBILL_TEST_SERVER"));

            // This is the API interface object.
            AuthorizeNetRecurringAPI.ARBCancelSubscriptionRequest request = new AuthorizeNetRecurringAPI.ARBCancelSubscriptionRequest();

            // Include authentication information
            request.merchantAuthentication = new AuthorizeNetRecurringAPI.MerchantAuthentication();
            request.merchantAuthentication.name = X_Login;
            request.merchantAuthentication.transactionKey = X_TranKey;
            request.refId = OriginalRecurringOrderNumber.ToString();

            // Populate the subscription request with data
            AuthorizeNetRecurringAPI.ARBSubscription sub = new AuthorizeNetRecurringAPI.ARBSubscription();
            AuthorizeNetRecurringAPI.CreditCard creditCard = new AuthorizeNetRecurringAPI.CreditCard();
            request.subscriptionId = RecurringSubscriptionID;

            // The response type will normally be ARBCancelSubscriptionResponse.
            // However, in the case of an error such as an XML parsing error, the response type will be ErrorResponse.

            object response = null;
            XmlDocument xmldoc = null;
            result = AuthorizeNetRecurringAPI.APIHelper.PostRequest(request, AuthServer, out xmldoc);

            if (result.Equals(AppLogic.ro_OK, StringComparison.InvariantCultureIgnoreCase))
            {
                result = AuthorizeNetRecurringAPI.APIHelper.ProcessXmlResponse(xmldoc, out response);
            }

            if (result.Equals(AppLogic.ro_OK, StringComparison.InvariantCultureIgnoreCase))
            {
                String tmpResultCode = String.Empty; // not used or needed here
                String tmpRecurringSubscriptionID = String.Empty; // not used or needed here
                result = AuthorizeNetRecurringAPI.APIHelper.ProcessResponse(response, out tmpResultCode, out tmpRecurringSubscriptionID);
            }

            return result;
        }

        public override string RecurringBillingAddressUpdate(string RecurringSubscriptionID, int OriginalRecurringOrderNumber, Address UseBillingAddress)
        {
            String result = AppLogic.ro_OK;

            String X_Login = AppLogic.AppConfig("AUTHORIZENET_X_LOGIN");
            if (X_Login.Trim().Equals("REGISTRY", StringComparison.InvariantCultureIgnoreCase))
            {
                WindowsRegistry reg = new WindowsRegistry(AppLogic.AppConfig("EncryptKey.RegistryLocation"));
                X_Login = reg.Read("AUTHORIZENET_X_LOGIN");
                reg = null;
            }

            String X_TranKey = AppLogic.AppConfig("AUTHORIZENET_X_TRAN_KEY");
            if (X_TranKey.Trim().Equals("REGISTRY", StringComparison.InvariantCultureIgnoreCase))
            {
                WindowsRegistry reg = new WindowsRegistry(AppLogic.AppConfig("EncryptKey.RegistryLocation"));
                X_TranKey = reg.Read("AUTHORIZENET_X_TRAN_KEY");
                reg = null;
            }
            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");
            String AuthServer = CommonLogic.IIF(useLiveTransactions, AppLogic.AppConfig("Recurring.AUTHORIZENET_AUTOBILL_LIVE_SERVER"), AppLogic.AppConfig("Recurring.AUTHORIZENET_AUTOBILL_TEST_SERVER"));

            // This is the API interface object.
            AuthorizeNetRecurringAPI.ARBUpdateSubscriptionRequest request = new AuthorizeNetRecurringAPI.ARBUpdateSubscriptionRequest();

            // Include authentication information
            request.merchantAuthentication = new AuthorizeNetRecurringAPI.MerchantAuthentication();
            request.merchantAuthentication.name = X_Login;
            request.merchantAuthentication.transactionKey = X_TranKey;
            request.refId = OriginalRecurringOrderNumber.ToString();

            // Populate the subscription request with test data
            AuthorizeNetRecurringAPI.ARBSubscription sub = new AuthorizeNetRecurringAPI.ARBSubscription();
            AuthorizeNetRecurringAPI.CreditCard creditCard = new AuthorizeNetRecurringAPI.CreditCard();
            request.subscriptionId = RecurringSubscriptionID;

            creditCard.cardNumber = UseBillingAddress.CardNumber;
            creditCard.expirationDate = UseBillingAddress.CardExpirationYear + "-" + UseBillingAddress.CardExpirationMonth.PadLeft(2, '0');  // required format for API is YYYY-MM
            sub.payment = new AuthorizeNetRecurringAPI.Payment();
            sub.payment.item = creditCard;

            sub.billTo = new AuthorizeNetRecurringAPI.NameAndAddress();
            sub.billTo.firstName = UseBillingAddress.FirstName;
            sub.billTo.lastName = UseBillingAddress.LastName;
            sub.billTo.company = UseBillingAddress.Company;
            sub.billTo.address = UseBillingAddress.Address1;
            sub.billTo.city = UseBillingAddress.City;
            sub.billTo.state = UseBillingAddress.State;
            sub.billTo.zip = UseBillingAddress.Zip;
            sub.billTo.country = UseBillingAddress.Country;

            request.subscription = sub;

            // The response type will normally be ARBUpdateSubscriptionResponse.
            // However, in the case of an error such as an XML parsing error, the response type will be ErrorResponse.

            object response = null;
            XmlDocument xmldoc = null;
            result = AuthorizeNetRecurringAPI.APIHelper.PostRequest(request, AuthServer, out xmldoc);

            if (result.Equals(AppLogic.ro_OK, StringComparison.InvariantCultureIgnoreCase))
            {
                result = AuthorizeNetRecurringAPI.APIHelper.ProcessXmlResponse(xmldoc, out response);
            }

            if (result.Equals(AppLogic.ro_OK, StringComparison.InvariantCultureIgnoreCase))
            {
                String tmpResultCode = String.Empty; // not used or needed here
                String tmpRecurringSubscriptionID = String.Empty; // not used or needed here
                result = AuthorizeNetRecurringAPI.APIHelper.ProcessResponse(response, out tmpResultCode, out tmpRecurringSubscriptionID);
            }

            return result;
        }

        public override string ProcessAutoBillStatusFile(String GW, string StatusFile, out string Results, RecurringOrderMgr OrderManager)
        {
            String Status = AppLogic.ro_OK;
            StringBuilder tmpS = new StringBuilder();
            // Authorize.net provides two CSV files in an email. 
            try
            {
                DataTable dt = CsvParser.Parse(StatusFile, true);
                String Separator = String.Empty;
                foreach (DataRow row in dt.Rows)
                {
                    tmpS.Append(Separator);
                    String TxSubID = row["SubscriptionID"].ToString();
                    String TxStatus = row["RespCode"].ToString();
                    String TxMsg = row["RespText"].ToString();
                    String TxID = row["TransactionID"].ToString();
                    DateTime dtTx = System.DateTime.MinValue; // timestamp not provided by Authorize.net

                    if (TxSubID == "SubscriptionID" || TxSubID == "")
                    {
                        // This will only happen if someone merged the contents of multiple CSV files
                        // together and left the column headers in the middle of the combined file
                        // so skip this row.
                        continue;
                    }

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
                            if (TxStatus == "1") // Approved
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

        public override string RecurringBillingGetStatusFile()
        {
            return "AUTHORIZE.NET DOES NOT SUPPORT GET FILE. YOU MUST COPY AND PASTE IT FROM YOUR E-MAIL";
        }

        public override bool SupportsEChecks()
        {
            return true;
        }

        public override RecurringSupportType RecurringSupportType()
        {
            return Processors.RecurringSupportType.Extended;
        }

        public override List<DateIntervalTypeEnum> GetAllowedRecurringIntervals()
        {
            List<DateIntervalTypeEnum> excludedTypes = new List<DateIntervalTypeEnum>();
            excludedTypes.Add(DateIntervalTypeEnum.Day);
            excludedTypes.Add(DateIntervalTypeEnum.Week);
            excludedTypes.Add(DateIntervalTypeEnum.Month);
            excludedTypes.Add(DateIntervalTypeEnum.Year);
            return excludedTypes;
        }
    }
}
