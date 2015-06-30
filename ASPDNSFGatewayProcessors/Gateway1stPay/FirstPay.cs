// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Xml;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontGateways.Processors
{
    public class FirstPay : GatewayProcessor
    {

        /// <summary>
        /// Summary description for 1stPay.
        /// </summary>
        #region Valid Response Status Codes
        private static HttpStatusCode[] ValidStatusCodes = {    HttpStatusCode.Accepted 
                                                                , HttpStatusCode.Ambiguous
                                                                , HttpStatusCode.Continue
                                                                , HttpStatusCode.Created
                                                                , HttpStatusCode.Found
                                                                , HttpStatusCode.Moved
                                                                , HttpStatusCode.MovedPermanently
                                                                , HttpStatusCode.MultipleChoices
                                                                , HttpStatusCode.NotModified
                                                                , HttpStatusCode.OK
                                                                , HttpStatusCode.Redirect
                                                                , HttpStatusCode.RedirectKeepVerb
                                                                , HttpStatusCode.RedirectMethod
                                                                , HttpStatusCode.SeeOther
                                                                , HttpStatusCode.TemporaryRedirect
                                                           };
        #endregion

        #region Instance Variables

        public override bool ShowCheckoutButton
        {
            get { return false; }
        }

        private bool EmailMerchant;

        //Transaction Header
        private int transaction_center_id;
        private EmbeddedType? embedded;
        private TransactionType? operation_type;
        private string gateway_id;
        public string cim_ref_num;
        private bool cim_on;
        private string processor_id; 
        private string security_hash
        {
            get
            {
                return GetSecurityHash();
            }
        }
        private const bool prevent_partial = true; // don't allow partial payments
        private const bool Readonly = true;
        private string sandboxProcessor;        

        //Transaction Details
        public static int order_id
        {
            get
            {
                try
                {
                    return HttpContext.Current.Session["firstPayOrderNumber"] == null ? 0 : (int)HttpContext.Current.Session["firstPayOrderNumber"];
                }
                catch
                {
                    return 0;
                }
            }
            set
            {
                try
                {
                    HttpContext.Current.Session["firstPayOrderNumber"] = value;
                }
                catch { }
            }
        }
        public decimal total;
        
        //Level 2 fields
        private bool level_ii_on;
        public decimal tax_amount;        
        private string shipping_zip;

        //Card Holder Billing Info
        public string name;
        public string street;
        public string street2;
        public string city;
        public string state;
        public string zip;
        public string country;
        public string email;
        public string phone;

        //Response Handling
        private const bool respond_inline = false;
        private const bool respond_hidden = false;
        private string url_redirect;

        private bool email_customer; //enable/disable emails to the customer directly from the payment gateway.
        public string merchant_email; //address for transaction emails to be sent to the merchant
        public string header_email_receipt; // email header for the receipt that will send out            
        public string footer_email_receipt; // email footer for the receipt that will send out
        
        #endregion

        private enum EmbeddedType
        {
            no = 0
            , frame
        }

        public enum TransactionType
        {
            //Payment Portal Api Types
            ecom_auth
            , ecom_sale
            //Unsupported Operation Types
            //, moto_auth 
            //, moto_sale 
            //, retail_auth
            //, retail_sale
            //, retail_credit
            //, ach_debit
            //, ach_credit
            //, cim_insert

            //Xml Api Types           
            , credit            
            , @void
            , settle
            , query
            //Unsupported Operation Types
            //, ach_debit
            //, ach_credit
            //, retail_auth
            //, retail_sale
            //, cim_auth
            //, cim_sale
            //, cim_edit
            //, cim_delete
            //, retail_alone_credit
            //, cim_ach_debit
            //, cim_ach_credit
            //, reauth
            //, resale
            //, recurring_modify
        }

        public FirstPay()
        {
            Initialize();   
        }

        private void Initialize()
        {
            EmailMerchant = AppLogic.AppConfigBool("1stPay.AdminTransactionEmail.Enable");
            email_customer = AppLogic.AppConfigBool("1stPay.CustomerTransactionEmail.Enable");
            transaction_center_id = AppLogic.AppConfigNativeInt("1stPay.TransactionCenterId");
            embedded = EmbeddedType.frame;
            gateway_id = AppLogic.AppConfig("1stPay.GatewayId");
            processor_id = AppLogic.AppConfig("1stPay.ProcessorId");
            header_email_receipt = AppLogic.AppConfig("1stPay.EmailHeader");
            footer_email_receipt = AppLogic.AppConfig("1stPay.EmailFooter");
            merchant_email = AppLogic.AppConfig("GotOrderEMailTo").Replace(",", ";");
            url_redirect = string.Format("{0}fp-firstpayembeddedcheckoutok.aspx", AppLogic.GetStoreHTTPLocation(true));
            sandboxProcessor = AppLogic.AppConfig("1stPay.TestProccessor");
            level_ii_on = AppLogic.AppConfigBool("1stPay.Level2.Enable");
            cim_on = AppLogic.AppConfigBool("1stPay.Cim.Enable");
        }

        #region Display Output Methods

        public string GetFramedHostedCheckout(Customer thisCustomer)
        {
            ShoppingCart cart = new ShoppingCart(thisCustomer.SkinID, thisCustomer, CartTypeEnum.ShoppingCart, 0, false);
            return GetFramedHostedCheckout(cart);
        }

        public string GetFramedHostedCheckout(ShoppingCart cart)
        {
            //fill in code here to display error when none USD currency.
            if ("USD" != Currency.GetDefaultCurrency())
                return "This gateway only supports US Dollars.";

            string response;
            string AuthServer = AppLogic.AppConfig("1stPay.PaymentModuleURL");
            decimal cartTotal = cart.Total(true);
            total = cartTotal - CommonLogic.IIF(cart.Coupon.CouponType == CouponTypeEnum.GiftCard
                                                                , CommonLogic.IIF(cartTotal < cart.Coupon.DiscountAmount, cartTotal, cart.Coupon.DiscountAmount)
                                                                , 0);
            email = cart.ThisCustomer.EMail;            
            operation_type = TransactionType.ecom_auth;

            //Try to load up the address to be passed into the iframe for the customer
            Address address = new Address(cart.ThisCustomer.CustomerID);

            if (cart.CartItems.Count > 0 && cart.CartItems[0].BillingAddressID > 0)
                address.LoadFromDB(cart.CartItems[0].BillingAddressID);
            else
                address.LoadFromDB(cart.FirstItemShippingAddressID());
            
            //Load up default
            if (address.AddressID < 1)
                address.LoadFromDB(cart.ThisCustomer.PrimaryBillingAddressID);

            if (address.AddressID > 0)
            {
                name = (address.FirstName + " " + address.LastName).Trim();
                street = address.Address1;
                street2 = address.Address2;
                city = address.City;
                state = address.State;
                zip = address.Zip;
                country = address.Country;
                phone = address.Phone;
            }

            if (cim_on)            
                cim_ref_num = cart.ThisCustomer.CustomerGUID;            

            if (level_ii_on)
            {
                //Level 2 fields wants the shipping rather than billing zip so load that up.
                Address shipAddress = new Address(cart.ThisCustomer.CustomerID);
                shipAddress.LoadFromDB(cart.FirstItemShippingAddressID());

                //Load up default
                if (shipAddress.AddressID < 1)
                    shipAddress.LoadFromDB(cart.ThisCustomer.PrimaryShippingAddressID);

                tax_amount = cart.TaxTotal();
                shipping_zip = shipAddress.Zip;
            }
                       
            string RequestID = System.Guid.NewGuid().ToString("N");
            string rawResponseString;

            int MaxTries = AppLogic.AppConfigUSInt("GatewayRetries") + 1;
            int CurrentTry = 0;
            bool CallSuccessful = false;

            //Make sure the server is up.
            do
            {
                CurrentTry++;
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(AuthServer + string.Format("?transaction_center_id={0}&embedded={1}&operation_type={2}&respond_inline={3}"
                                                                                                        , transaction_center_id
                                                                                                        , embedded
                                                                                                        , operation_type
                                                                                                        , Convert.ToInt32(respond_inline)
                                                                                                        ));
                myRequest.Method = "POST";
                myRequest.ContentType = "text/namevalue";
                myRequest.ContentLength = 0;
                myRequest.Timeout = 30000;
                try
                {
                    HttpWebResponse myResponse;
                    myResponse = (HttpWebResponse)myRequest.GetResponse();
                    using (StreamReader sr = new StreamReader(myResponse.GetResponseStream()))
                    {
                        rawResponseString = sr.ReadToEnd();
                        sr.Close();
                    }
                    myResponse.Close();

                    CallSuccessful = ValidStatusCodes.Any(vsc => myResponse.StatusCode == vsc);
                }
                catch
                {
                    CallSuccessful = false;
                }
            }
            while (!CallSuccessful && CurrentTry < MaxTries);

            if (CallSuccessful && order_id < 1)
                order_id = AppLogic.GetNextOrderNumber();


            if (CallSuccessful)
            {
                StringBuilder transactionCommand = BuildPortalTransactionCommand();
                OrderTransactionCollection transactions = new OrderTransactionCollection(order_id);
                
                //If we haven't already logged the transaction command for this same order/iframe url combo then add it to the transaction log.
                if (!transactions.Transactions.Any(t => t.TransactionType == (AppLogic.TransactionModeIsAuthCapture() ? AppLogic.ro_TXModeAuthCapture : AppLogic.ro_TXModeAuthOnly)
                                                    && t.TransactionCommand == transactionCommand.ToString()
                                                    && t.PaymentGateway == DisplayName(cart.ThisCustomer.LocaleSetting)
                                                    && t.Amount == total
                                            ))
                {
                    transactions.AddTransaction(AppLogic.TransactionModeIsAuthCapture() ? AppLogic.ro_TXModeAuthCapture : AppLogic.ro_TXModeAuthOnly, transactionCommand.ToString(), null, null, null, AppLogic.ro_PMCreditCard, DisplayName(cart.ThisCustomer.LocaleSetting), total);
                }

                response = GetFrameSrc(0, 500, transactionCommand.ToString());
            }
            else
                response = "Unable to connect to the server, please try again later.";
            
            return response;
        }

        private string GetFrameSrc(int width, int height, string transactionCommand)
        {
            bool isTest = !AppLogic.AppConfigBool("UseLiveTransactions");
            string authServer = AppLogic.AppConfig("1stPay.PaymentModuleURL");
            
            if (!authServer.EndsWith("/"))
                authServer += "/";
            
            string frameFormat = "<iframe src='{0}?{1}' width='{2}' height='{3}'></iframe>";

            return string.Format(frameFormat,
                authServer,
                transactionCommand,
                (width == 0 ? "100%" : width.ToString()),
                height
                );
        }

        public override string CreditCardPaneInfo(int skinId, Customer thisCustomer)
        {
            string returnVal = GetFramedHostedCheckout(thisCustomer);
            return returnVal;
        }

        #endregion

        #region Transaction Methods

        public override string CaptureOrder(Order order)
        {
            string result = AppLogic.ro_OK;
            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");
            Customer ThisCustomer = new Customer(order.CustomerID, true);

            order.CaptureTXCommand = "";
            order.CaptureTXResult = "";

            string TransID = order.AuthorizationPNREF;
            Decimal OrderTotal = order.OrderBalance;
            string AuthorizationCode = order.AuthorizationCode;

            order.CaptureTXCommand = TransID;

            if (string.IsNullOrEmpty(TransID) || TransID.Length == 0 || TransID == "0")
            {
                result = "Invalid or Empty Transaction ID";
            }
            else
            {
                try
                {
                    string[] TXInfo = TransID.Split('|');

                    Encoding encoding = System.Text.Encoding.GetEncoding(1252);
                    FirstPayXmlCommand transactionCommand = new FirstPayXmlCommand(transaction_center_id.ToString(), gateway_id, processor_id);
                    
                    transactionCommand.Settle(TXInfo[0], Localization.CurrencyStringForGatewayWithoutExchangeRate(OrderTotal));

                    string rawResponseString = "";
                    FirstPayXmlResponse xmlResponse;

                    if(GetXmlResponse(transactionCommand.InnerXml, out rawResponseString, out xmlResponse))
                    {
                        string responseCode = "";
                        string responseError = "";
                        string authResponse = "";
                        string responseReferenceNumber = "";

                        if (xmlResponse.Fields.Count > 0)
                        {
                            responseCode = xmlResponse.Fields.ContainsKey("status1") ? xmlResponse.Fields["status1"] : "";
                            authResponse = xmlResponse.Fields.ContainsKey("response1") ? xmlResponse.Fields["response1"] : "";
                            responseError = xmlResponse.Fields.ContainsKey("error1") ? xmlResponse.Fields["error1"] : "";
                            responseReferenceNumber = xmlResponse.Fields.ContainsKey("reference_number1") ? xmlResponse.Fields["reference_number1"] : "";
                        }

                        if (responseCode == "1")
                        {
                            result = AppLogic.ro_OK;
                        }
                        else if (responseCode == "2")
                        {
                            result = "REJECTED";
                            if (authResponse.Length > 0)
                            {
                                result += ". " + authResponse;
                            }
                        }
                        else if (responseCode == "0")
                        {
                            result = "Error: " + authResponse + " | " + responseError;
                        }
                        else
                        {
                            result = "System Error: " + rawResponseString;
                        }

                        OrderTransactionCollection transactions = new OrderTransactionCollection(order.OrderNumber);
                        transactions.AddTransaction(AppLogic.ro_TXStateCaptured, transactionCommand.InnerXml.Replace(gateway_id, "***"), rawResponseString, responseReferenceNumber, responseCode, AppLogic.ro_PMCreditCard, DisplayName(ThisCustomer.LocaleSetting), OrderTotal);

                    }
                    else
                    {
                        result = "Error calling 1stPay gateway.";
                    }
                    order.CaptureTXResult = result;
                    order.CaptureTXCommand = transactionCommand.InnerXml.Replace(gateway_id, "***");
                }
                catch (Exception ex)
                {
                    result = ex.Message;
                }
            }
            return result;
        }

        public override string VoidOrder(int orderNumber)
        {
            string result = AppLogic.ro_OK;
            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");

            //Will add parameters to this as we get data back from other querys etc. So I don't have to repeat code
            List<SqlParameter> sqlParams = new List<SqlParameter>();
            sqlParams.Add(new SqlParameter("@OrderNumber", orderNumber));
           
            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (SqlCommand cmd = new SqlCommand("update orders set VoidTXCommand=NULL, VoidTXResult=NULL where OrderNumber=@OrderNumber;", dbconn))
                {
                    cmd.Parameters.AddRange(sqlParams.ToArray());
                    cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();
                }
            }

            string TransID = "";
            string AuthorizationCode = "";
            Order order = new Order(orderNumber);
            Customer ThisCustomer = new Customer(order.CustomerID);

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (SqlCommand cmd = new SqlCommand("select AuthorizationPNREF,AuthorizationCode from Orders  with (NOLOCK)  where OrderNumber=@OrderNumber;", dbconn))
                {
                    cmd.Parameters.AddRange(sqlParams.ToArray());
                    using (IDataReader rs = cmd.ExecuteReader())
                    {
                        if (rs.Read())
                        {
                            TransID = DB.RSField(rs, "AuthorizationPNREF");
                            AuthorizationCode = DB.RSField(rs, "AuthorizationCode");
                        }
                    }
                    cmd.Parameters.Clear();
                }
            }

            sqlParams.Add(new SqlParameter("@TransId", TransID));
            
            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (SqlCommand cmd = new SqlCommand("update orders set VoidTXCommand=@TransId where OrderNumber=@OrderNumber;", dbconn))
                {
                    cmd.Parameters.AddRange(sqlParams.ToArray());
                    cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();
                }
            }

            if (TransID.Length == 0 || TransID == "0")
            {
                result = "Invalid or Empty Transaction ID";
            }
            else
            {
                try
                {
                    string[] TXInfo = TransID.Split('|');

                    Encoding encoding = System.Text.Encoding.GetEncoding(1252);
                    FirstPayXmlCommand transactionCommand = new FirstPayXmlCommand(transaction_center_id.ToString(), gateway_id, processor_id);

                    transactionCommand.Void(TXInfo[0]);

                    string rawResponseString = "";
                    FirstPayXmlResponse xmlResponse;

                    if(GetXmlResponse(transactionCommand.InnerXml, out rawResponseString, out xmlResponse))
                    {
                        string responseCode = "";
                        string responseError = "";
                        string authResponse = "";
                        string responseReferenceNumber = "";
                        

                        if (xmlResponse.Fields.Count > 0)
                        {
                            responseCode = xmlResponse.Fields.ContainsKey("status1") ? xmlResponse.Fields["status1"] : "";
                            authResponse = xmlResponse.Fields.ContainsKey("response1") ? xmlResponse.Fields["response1"] : "";
                            responseError = xmlResponse.Fields.ContainsKey("error1") ? xmlResponse.Fields["error1"] : "";
                            responseReferenceNumber = xmlResponse.Fields.ContainsKey("reference_number1") ? xmlResponse.Fields["reference_number1"] : "";
                        }

                        if (responseCode == "1")
                        {
                            result = AppLogic.ro_OK;
                        }
                        else if (responseCode == "2")
                        {
                            result = "REJECTED";
                            if (authResponse.Length > 0)
                            {
                                result += ". " + authResponse;
                            }
                        }
                        else if (responseCode == "0")
                        {
                            result = "Error: " + authResponse + " | " + responseError;
                        }
                        else
                        {
                            result = "System Error: " + rawResponseString;
                        }

                        OrderTransactionCollection transactions = new OrderTransactionCollection(order.OrderNumber);
                        transactions.AddTransaction(AppLogic.ro_TXStateVoided, transactionCommand.InnerXml.Replace(gateway_id, "***"), rawResponseString, responseReferenceNumber, responseCode, AppLogic.ro_PMCreditCard, DisplayName(ThisCustomer.LocaleSetting), order.OrderBalance);

                    }
                    else
                    {
                        result = "Error calling 1stPay gateway.";
                    }
                    sqlParams.Add(new SqlParameter("@Result", result));
                    sqlParams.Add(new SqlParameter("@Command", transactionCommand.InnerXml.Replace(gateway_id, "***")));
                                        
                    using (SqlConnection dbconn = DB.dbConn())
                    {
                        dbconn.Open();
                        using (SqlCommand cmd = new SqlCommand("update orders set VoidTXResult=@Result, VoidTXCommand=@Command where OrderNumber=@OrderNumber;", dbconn))
                        {
                            cmd.Parameters.AddRange(sqlParams.ToArray());
                            cmd.ExecuteNonQuery();
                            cmd.Parameters.Clear();
                        }
                    }

                }
                catch (Exception ex)
                {
                    result = ex.Message;
                }

            }
            return result;
        }

        // if RefundAmount == 0.0M, then then ENTIRE order amount will be refunded!
        public override string RefundOrder(int originalOrderNumber, int newOrderNumber, decimal refundAmount, string refundReason, Address useBillingAddress)
        {
            string result = AppLogic.ro_OK;
            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");

            //Will add parameters to this as we get data back from other querys etc. So I don't have to repeat code
            List<SqlParameter> sqlParams = new List<SqlParameter>();
            sqlParams.Add(new SqlParameter("@OrderNumber", originalOrderNumber));

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (SqlCommand cmd = new SqlCommand("update orders set RefundTXCommand=NULL, RefundTXResult=NULL where OrderNumber=@OrderNumber;", dbconn))
                {
                    cmd.Parameters.AddRange(sqlParams.ToArray());
                    cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();
                }
            }

            string TransID = "";
            string AuthorizationCode = "";
            Decimal OrderTotal = System.Decimal.Zero;
            Order order = new Order(originalOrderNumber);
            Customer ThisCustomer = new Customer(order.CustomerID);

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (SqlCommand cmd = new SqlCommand("select * from orders with (NOLOCK)  where OrderNumber=@OrderNumber;", dbconn))
                {
                    cmd.Parameters.AddRange(sqlParams.ToArray());
                    using (IDataReader rs = cmd.ExecuteReader())
                    {
                        if (rs.Read())
                        {
                            TransID = DB.RSField(rs, "AuthorizationPNREF");
                            AuthorizationCode = DB.RSField(rs, "AuthorizationCode");
                            OrderTotal = DB.RSFieldDecimal(rs, "OrderTotal");
                        }
                    }
                    cmd.Parameters.Clear();
                }
            }

            if (TransID.Length == 0 || TransID == "0")
            {
                result = "Invalid or Empty Transaction ID";
            }
            else
            {
                try
                {
                    string[] TXInfo = TransID.Split('|');

                    Encoding encoding = System.Text.Encoding.GetEncoding(1252);
                    FirstPayXmlCommand transactionCommand = new FirstPayXmlCommand(transaction_center_id.ToString(), gateway_id, processor_id);

                    transactionCommand.Credit(TXInfo[0], Localization.CurrencyStringForGatewayWithoutExchangeRate(refundAmount == System.Decimal.Zero ? OrderTotal : refundAmount));

                    string rawResponseString = "";
                    FirstPayXmlResponse xmlResponse;

                    if(GetXmlResponse(transactionCommand.InnerXml, out rawResponseString, out xmlResponse))
                    {

                        string responseCode = "";
                        string responseError = "";
                        string authResponse = "";
                        string responseReferenceNumber = "";

                        if (xmlResponse.Fields.Count > 0)
                        {
                            responseCode = xmlResponse.Fields.ContainsKey("status1") ? xmlResponse.Fields["status1"] : "";
                            authResponse = xmlResponse.Fields.ContainsKey("response1") ? xmlResponse.Fields["response1"] : "";
                            responseError = xmlResponse.Fields.ContainsKey("error1") ? xmlResponse.Fields["error1"] : "";
                            responseReferenceNumber = xmlResponse.Fields.ContainsKey("reference_number1") ? xmlResponse.Fields["reference_number1"] : "";
                        }

                        if (responseCode == "1")
                        {
                            result = AppLogic.ro_OK;
                        }
                        else if (responseCode == "2")
                        {
                            result = "REJECTED";
                            if (authResponse.Length > 0)
                            {
                                result += ". " + authResponse;
                                result += " : This order may not have settled yet, try void instead.";
                            }
                        }
                        else if (responseCode == "0")
                        {
                            result = "Error: " + authResponse + " | " + responseError;
                        }
                        else
                        {
                            result = "System Error: " + rawResponseString;
                        }

                        OrderTransactionCollection transactions = new OrderTransactionCollection(order.OrderNumber);
                        transactions.AddTransaction(AppLogic.ro_TXStateRefunded, transactionCommand.InnerXml.Replace(gateway_id, "***"), rawResponseString, responseReferenceNumber, responseCode, AppLogic.ro_PMCreditCard, DisplayName(ThisCustomer.LocaleSetting), order.OrderBalance);

                        sqlParams.Add(new SqlParameter("@Result", result));
                        sqlParams.Add(new SqlParameter("@Command", transactionCommand.InnerXml.Replace(gateway_id, "***")));                        

                        if (result == AppLogic.ro_OK)
                        {   
                            using (SqlConnection dbconn = DB.dbConn())
                            {
                                dbconn.Open();
                                using (SqlCommand cmd = new SqlCommand("update orders set RefundTXResult=@Result, RefundTXCommand=@Command where OrderNumber=@OrderNumber;", dbconn))
                                {
                                    cmd.Parameters.AddRange(sqlParams.ToArray());
                                    cmd.ExecuteNonQuery();
                                    cmd.Parameters.Clear();
                                }
                            }
                        }
                        else
                        {                            
                            using (SqlConnection dbconn = DB.dbConn())
                            {
                                dbconn.Open();
                                using (SqlCommand cmd = new SqlCommand("update orders set RefundTXResult=@Result, RefundTXCommand=@Command where OrderNumber=@OrderNumber;", dbconn))
                                {
                                    cmd.Parameters.AddRange(sqlParams.ToArray());
                                    cmd.ExecuteNonQuery();
                                    cmd.Parameters.Clear();
                                }
                            }
                        }
                    }
                    else
                    {
                        result = "Error calling 1stPay gateway.";
                    }
                }
                catch (Exception ex)
                {
                    result = ex.Message;
                }

            }
            return result;
        }

        //Settle should be run here if site is setup for auth/capture, Payment Module will always be processing a transactions as an Auth
        public override string ProcessCard(int orderNumber, int customerID, Decimal orderTotal, bool useLiveTransactions, TransactionModeEnum transactionMode, Address useBillingAddress, string cardExtraCode, Address useShippingAddress, string CAVV, string ECI, string XID, out string AVSResult, out string authorizationResult, out string authorizationCode, out string authorizationTransID, out string transactionCommandOut, out string transactionResponse)
        {
            Customer ThisCustomer = new Customer(customerID, true);
            authorizationResult = AVSResult = authorizationCode = authorizationTransID = transactionCommandOut = transactionResponse = "";
            string approvalCode = "";
            string AVSCode = AVSResult = "";
            string CVCode = "";
            string responseCode = "";
            string responseError = "";
            string responseReferenceNumber = "";
            string authResponse = "";
            string TransID = "";
            string TransStatus = "";
            string result = AppLogic.ro_OK;

            if (ThisCustomer != null && ThisCustomer.CustomerID > 0)
            {
                if (ThisCustomer.IsAdminUser && (AppLogic.ExceedsFailedTransactionsThreshold(ThisCustomer) || AppLogic.IPIsRestricted(ThisCustomer.LastIPAddress)))
                {
                    return AppLogic.GetString("gateway.FailedTransactionThresholdExceeded", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                }
            }

            Encoding encoding = System.Text.Encoding.GetEncoding(1252);
            FirstPayXmlCommand transactionCommand = new FirstPayXmlCommand(transaction_center_id.ToString(), gateway_id, processor_id);
            if (transactionMode == TransactionModeEnum.authcapture)
                transactionCommand.Settle(XID, Localization.CurrencyStringForGatewayWithoutExchangeRate(orderTotal));
            else
                transactionCommand.Query(orderNumber.ToString(), DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

            string rawResponseString = "";
            FirstPayXmlResponse xmlResponse;

            if (GetXmlResponse(transactionCommand.InnerXml, out rawResponseString, out xmlResponse))
            {

                if (xmlResponse.Fields.Count > 0)
                {
                    responseCode = xmlResponse.Fields.ContainsKey("status1") ? xmlResponse.Fields["status1"] : (xmlResponse.Fields.ContainsKey("trans_status1") ? xmlResponse.Fields["trans_status1"] : "");
                    authResponse = xmlResponse.Fields.ContainsKey("response1") ? xmlResponse.Fields["response1"] : "";
                    responseError = xmlResponse.Fields.ContainsKey("error1") ? xmlResponse.Fields["error1"] : (xmlResponse.Fields.ContainsKey("error") ? xmlResponse.Fields["error"] : "");
                    TransStatus = xmlResponse.Fields.ContainsKey("trans_status1") ? xmlResponse.Fields["trans_status1"] : null;
                    responseReferenceNumber = xmlResponse.Fields.ContainsKey("reference_number1") ? xmlResponse.Fields["reference_number1"] : "";
                }

                // rawResponseString now has gateway response
                transactionResponse = rawResponseString;

                authorizationCode = approvalCode;
                authorizationResult = rawResponseString;
                authorizationTransID = TransID;
                AVSResult = AVSCode;
                if (CVCode.Length > 0)
                {
                    AVSResult += ", CV Result: " + CVCode;
                }
                transactionCommandOut = transactionCommand.InnerXml.Replace(gateway_id, "***");

                if (responseCode == "1" && (string.IsNullOrEmpty(TransStatus) || TransStatus == "1"))
                {
                    result = AppLogic.ro_OK;
                }
                else if (responseCode == "2")
                {
                    result = "DECLINED";
                    if (authResponse.Length > 0)
                    {
                        result += ". " + authResponse;
                    }
                }
                else if (responseCode == "0")
                {
                    result = "Error: " + authResponse + " | " + responseError;
                }
                else
                {
                    result = "System Error: " + rawResponseString;
                }

                if (transactionMode == TransactionModeEnum.authcapture)
                {
                    OrderTransactionCollection transactions = new OrderTransactionCollection(orderNumber);
                    transactions.AddTransaction(AppLogic.ro_TXStateCaptured, transactionCommand.InnerXml.Replace(gateway_id, "***"), rawResponseString, responseReferenceNumber, responseCode, AppLogic.ro_PMCreditCard, DisplayName(ThisCustomer.LocaleSetting), orderTotal);
                }
            }
            else
            {
                result = "Error calling 1stPay gateway.";
            }
            if (result != AppLogic.ro_OK)
            {
                string IP = "";
                SqlParameter[] sqlParams = {new SqlParameter("@CustomerID", customerID)
                                                , new SqlParameter("@OrderNumber", orderNumber)
                                                , new SqlParameter("@IP", IP)
                                                , new SqlParameter("@Gateway", DisplayName(ThisCustomer.LocaleSetting))
                                                , new SqlParameter("@PaymentMethod", AppLogic.ro_PMCreditCard)
                                                , new SqlParameter("@Command", transactionCommandOut)
                                                , new SqlParameter("@Result", transactionResponse)
                                           };

                if (ThisCustomer != null)
                {
                    IP = ThisCustomer.LastIPAddress;
                }
                string sql = "insert into FailedTransaction(CustomerID,OrderNumber,IPAddress,OrderDate,PaymentGateway,PaymentMethod,TransactionCommand,TransactionResult) "
                                + "values(@CustomerID,@OrderNumber,@IP,getdate(),@Gateway,@PaymentMethod,@Command,@Result)";

                using (SqlConnection dbconn = DB.dbConn())
                {
                    dbconn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, dbconn))
                    {
                        cmd.Parameters.AddRange(sqlParams.ToArray());
                        cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                    }
                }
            }

            return result;
        }

        public FirstPayXmlResponse QueryGatewayForOrder(int orderNumber)
        {
            string result = AppLogic.ro_OK;
            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");

            try
            {
                Encoding encoding = System.Text.Encoding.GetEncoding(1252);
                FirstPayXmlCommand transactionCommand = new FirstPayXmlCommand(transaction_center_id.ToString(), gateway_id, processor_id);

                transactionCommand.Query(orderNumber.ToString(), DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

                string rawResponseString = "";
                FirstPayXmlResponse xmlResponse;

                if (GetXmlResponse(transactionCommand.InnerXml, out rawResponseString, out xmlResponse))
                {

                    return xmlResponse;
                }
            
            }
            catch { }

            
            return null;
        }

        #endregion
       
        #region Helper Methods

        public bool GetXmlResponse(string transactionCommand, out string response, out FirstPayXmlResponse xmlResponse)
        {
            Encoding encoding = System.Text.Encoding.GetEncoding(1252);
            bool returnVal = false;
            response = null;
            xmlResponse = null;
            byte[] data = encoding.GetBytes(transactionCommand);

            // Prepare web request...
            try
            {
                string AuthServer = AppLogic.AppConfig("1stPay.XmlURL");
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(AuthServer);
                myRequest.ContentLength = data.Length;
                myRequest.Method = "POST";

                Stream newStream = myRequest.GetRequestStream();
                // Send the data.
                newStream.Write(data, 0, data.Length);
                newStream.Close();
                // get the response
                HttpWebResponse myResponse;
                myResponse = (HttpWebResponse)myRequest.GetResponse();

                using (StreamReader sr = new StreamReader(myResponse.GetResponseStream(), encoding))
                {
                    response = sr.ReadToEnd();
                    // Close and clean up the StreamReader
                    sr.Close();
                }
                myResponse.Close();

                XmlDocument Doc = new XmlDocument();
                // Zap the DOCTYPE so we don't try to find a corresponding DTD.
                string t1 = "<!DOCTYPE Response SYSTEM";
                string t2 = ">";
                string doctype = t1 + CommonLogic.ExtractToken(response, t1, t2) + t2;
                Doc.LoadXml(response.Replace(doctype, ""));

                xmlResponse = new FirstPayXmlResponse(Doc);
                returnVal = true;                
            }
            catch { }
            return returnVal;
        }

        public override string DisplayName(string localeSetting)
        {
            return "1stPay";
        }
        
        private StringBuilder BuildPortalTransactionCommand()
        {
            StringBuilder transactionCommand = new StringBuilder(4096);
            bool loadAddress = true;

            transactionCommand.Append(string.Format("transaction_center_id={0}", transaction_center_id.ToString()));
            transactionCommand.Append(string.Format("&processor_id={0}", processor_id));
            transactionCommand.Append(string.Format("&embedded={0}", embedded.ToString()));
            transactionCommand.Append(string.Format("&operation_type={0}", operation_type.ToString()));
            transactionCommand.Append(string.Format("&order_id={0}", order_id));   

            if (level_ii_on)
            {
                transactionCommand.Append(string.Format("&level_ii_on={0}", Convert.ToInt32(level_ii_on)));
                transactionCommand.Append(string.Format("&tax_amount={0}", Localization.CurrencyStringForGatewayWithoutExchangeRate(tax_amount)));
                transactionCommand.Append(string.Format("&shipping_zip={0}", HttpUtility.UrlEncode(shipping_zip)));
            }

            transactionCommand.Append(string.Format("&total={0}", Localization.CurrencyStringForGatewayWithoutExchangeRate(total)));
            transactionCommand.Append(string.Format("&security_hash={0}", security_hash));

            if (loadAddress && !string.IsNullOrEmpty(zip)) // zip is required
            {
                transactionCommand.Append(string.Format("&name={0}", HttpUtility.UrlEncode(name)));
                transactionCommand.Append(string.Format("&street={0}", HttpUtility.UrlEncode(street)));
                transactionCommand.Append(string.Format("&street2={0}", HttpUtility.UrlEncode(street2)));
                transactionCommand.Append(string.Format("&city={0}", HttpUtility.UrlEncode(city)));
                transactionCommand.Append(string.Format("&state={0}", HttpUtility.UrlEncode(state)));
                transactionCommand.Append(string.Format("&zip={0}", HttpUtility.UrlEncode(zip)));
                transactionCommand.Append(string.Format("&country={0}", HttpUtility.UrlEncode(country)));
                transactionCommand.Append(string.Format("&phone={0}", HttpUtility.UrlEncode(phone)));
            }

            transactionCommand.Append(string.Format("&respond_inline={0}", Convert.ToInt32(respond_inline)));
            transactionCommand.Append(string.Format("&respond_hidden={0}", Convert.ToInt32(respond_hidden)));
            transactionCommand.Append(string.Format("&url_redirect={0}", HttpUtility.UrlEncode(url_redirect)));
            if (email_customer)
            {
                transactionCommand.Append(string.Format("&email_customer={0}", Convert.ToInt32(email_customer)));
                transactionCommand.Append(string.Format("&email={0}", HttpUtility.UrlEncode(email)));
            }

            if(EmailMerchant)
                transactionCommand.Append(string.Format("&merchant_email={0}", HttpUtility.UrlEncode(merchant_email)));
            if(!string.IsNullOrEmpty(header_email_receipt))
                transactionCommand.Append(string.Format("&header_email_receipt={0}", HttpUtility.UrlEncode(header_email_receipt)));
            if (!string.IsNullOrEmpty(footer_email_receipt))
                transactionCommand.Append(string.Format("&footer_email_receipt={0}", HttpUtility.UrlEncode(footer_email_receipt)));

            if (cim_on)
            {
                transactionCommand.Append(string.Format("&cim_on={0}", Convert.ToInt32(cim_on)));
                transactionCommand.Append(string.Format("&cim_ref_num={0}", HttpUtility.UrlEncode(cim_ref_num)));

            }

            return transactionCommand;
        }
                
        private string GetSecurityHash()
        {
            string hashInput = string.Format("{0}{1}{2}{3}{4}{5}"
                                                , transaction_center_id
                                                , embedded
                                                , operation_type
                                                , Localization.CurrencyStringForGatewayWithoutExchangeRate(total)
                                                , HttpUtility.UrlEncode(cim_ref_num)
                                                , gateway_id.ToUpper()
                                                );

            // step 1, calculate MD5 hash from input
            MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] inputBytes = System.Text.Encoding.Default.GetBytes(hashInput);
            byte[] hash = md5.ComputeHash(inputBytes);
            
            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }

            return sb.ToString();
        }

        public bool ConfirmTransaction(int orderNumber, decimal orderTotal, string referenceNum)
        {
            string error;
            List<FirstPayTransaction> transactions;
            FirstPayTransaction transaction;
            FirstPayXmlResponse response = QueryGatewayForOrder(orderNumber);             

            if (response == null)
                return false;
            
            transactions = response.GetTransactionsFromQuery();
            
            //Check to see if we have a transaction that matches our order number, reference number from first pay, and order total
            transaction = transactions.FirstOrDefault(t => t.ReferenceNumber.ToString() == referenceNum && t.OrderId == orderNumber.ToString() && t.Amount == orderTotal);

            //If we have no matching transaction or the transaction had a failed status then bail
            if (transaction == null || transaction.Status == 0)
                return false;

            error = response.Fields.ContainsKey("error") ? response.Fields["error"] : "";
            
            //At this point this should always be true but incase there is an error we'll catch it here (query should not return the error node and transactions)
            bool OrderIsValid = string.IsNullOrEmpty(error);

            //make sure our transaction was not voided or credited
            OrderIsValid = OrderIsValid && transaction.CreditVoid.ToLower() == "none";

            return OrderIsValid;                             
        }

        #endregion
    }    

}
