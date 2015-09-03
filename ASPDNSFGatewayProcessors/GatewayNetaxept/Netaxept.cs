// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------


namespace AspDotNetStorefrontGateways.Processors
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Linq;
    using System.ServiceModel;
    using System.Text;
    using System.Web.Services.Protocols;
    using System.Xml;
    using System.Xml.XPath;
    using AspDotNetStorefrontCore;
    using AspDotNetStorefrontGateways.Processors.NetaxeptAPI; 

    /// <summary>
    /// This is the class that encapsulate the transaction(auth,capture,void,authcapture)
    /// </summary>
    public class Netaxept : GatewayProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Netaxept"/> class.
        /// </summary>
        public Netaxept()
        {
        }
        
        /// <summary>
        /// Captures the order.
        /// </summary>
        /// <param name="o">The sales order.</param>
        /// <returns> Status of the transaction </returns>
        public override String CaptureOrder(Order o)
        {
            // merchant account
            string token = AppLogic.AppConfig("NETAXEPT.Merchant_Token");
            string merchantID = AppLogic.AppConfig("NETAXEPT.Merchant_Id");

            string responseText = string.Empty;
            string responseCode = string.Empty;
            
            // get the transaction string that was created of calling setup in the GetHTMLTicket
            string transactionString = Customer.Current.ThisCustomerSession["Nextaxept_TransactionString"];

            string sql = string.Format("SELECT TransactionCommand FROM Orders WHERE OrderNumber = {0}", o.OrderNumber.ToString());

            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS(sql,conn))
                {
                   if (rs.Read())
                   {
                       transactionString = DB.RSField(rs, "TransactionCommand");
                   }
                }
            }
            // This is the transaction string needed for capture
            string transID = o.AuthorizationPNREF;

            TokenService service = new TokenService();

            // the server url
            string url = string.Empty;
            if (AppLogic.AppConfigBool("UseLiveTransactions"))
            {
                url = AppLogic.AppConfig("NETAXEPT.Live_Server"); // use live 
            }
            else
            {
                url = AppLogic.AppConfig("NETAXEPT.Test_Server"); // use test
            }

            service.Url = url;
           
            try
            {
                Result result = null;
                Result authResult = null;
             
                result = service.ProcessSetup(token, merchantID, transactionString);
             
                if (result.ResponseCode.Equals("OK", StringComparison.OrdinalIgnoreCase))
                {
                    authResult = service.Capture(token, merchantID, transID, string.Empty, RemoveSeperatorFromAmount(o.OrderBalance), string.Empty);
                }

                // ResponseCode is the response from BBS or issuer
                o.CaptureTXResult = authResult.ResponseCode;               
            }
            catch (Exception ex)
            {
               SoapException se = ex as SoapException;

                if (se != null)
                {
                    GetStatusCodeMessage(se, out responseText, out responseCode);
                    o.CaptureTXResult = responseCode;

                    if (!string.IsNullOrEmpty(responseText))
                    {
                        return responseText;
                    }
                }
              
                return ex.Message;
            }

            return AppLogic.ro_OK; 
        }

        /// <summary>
        /// Voids the order.
        /// </summary>
        /// <param name="orderNumber">The Order Number.</param>
        /// <returns>Status of the transaction</returns>
        public override String VoidOrder(int orderNumber)
        {
            // merchant account
            string token = AppLogic.AppConfig("NETAXEPT.Merchant_Token");
            string merchantID = AppLogic.AppConfig("NETAXEPT.Merchant_Id");          

            string responseText = string.Empty;
            string responseCode = string.Empty;
            string transactionString = Customer.Current.ThisCustomerSession["Nextaxept_TransactionString"];

            string transID = String.Empty;
            string transactionState = string.Empty;
            decimal orderTotal = 0;

            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS("SELECT AuthorizationPNREF, TransactionState, TransactionCommand, OrderTotal FROM Orders   with (NOLOCK)  where OrderNumber=" + orderNumber.ToString(), conn))
                {
                    if (rs.Read())
                    {
                        orderTotal = DB.RSFieldInt(rs, "OrderTotal");
                        transactionState = DB.RSField(rs, "TransactionState");
                        transID = DB.RSField(rs, "AuthorizationPNREF");
                        transactionString = DB.RSField(rs, "TransactionCommand");
                    }
                }
            }

            // Only orders that is not captured can be void
            if (transactionState.Equals("CAPTURED", StringComparison.OrdinalIgnoreCase))
            {
                return AppLogic.GetString("Netaxept.VoidedCaptureError", Customer.Current.SkinID, Customer.Current.LocaleSetting);
            }

            TokenService service = new TokenService();

            // the server url
            string url = string.Empty;
            if (AppLogic.AppConfigBool("UseLiveTransactions"))
            {
                url = AppLogic.AppConfig("NETAXEPT.Live_Server"); // use live 
            }
            else
            {
                url = AppLogic.AppConfig("NETAXEPT.Test_Server"); // use test
            }

            service.Url = url;

            try
            {
                Result result = null;
                Result authResult = null;

                result = service.ProcessSetup(token, merchantID, transactionString);

                authResult = service.Annul(token, merchantID, transID, string.Empty, string.Empty);

                DB.ExecuteSQL(string.Format("UPDATE Orders set VoidTXResult='{0}' where OrderNumber=" + orderNumber.ToString(), authResult.ResponseCode));               
            }
            catch (Exception ex)
            {
                SoapException se = ex as SoapException;

                if (se != null)
                {
                    GetStatusCodeMessage(se, out responseText, out responseCode);
                    DB.ExecuteSQL(string.Format("UPDATE Orders set VoidTXResult='{0}' WHERE OrderNumber=" + orderNumber.ToString(), responseCode));

                    if (!string.IsNullOrEmpty(responseText))
                    {
                        return responseText;
                    }
                }
              
                return ex.Message;
            }

            return AppLogic.ro_OK; 
        }

        /// <summary>
        /// Refunds the order.
        /// </summary>
        /// <param name="originalOrderNumber">The original order number.</param>
        /// <param name="newOrderNumber">The new order number.</param>
        /// <param name="refundAmount">The refund amount.</param>
        /// <param name="refundReason">The refund reason.</param>
        /// <param name="useBillingAddress">The use billing address.</param>
        /// <returns> Status of the transaction </returns>
        public override String RefundOrder(int originalOrderNumber, int newOrderNumber, decimal refundAmount, string refundReason, Address useBillingAddress)
        {
            // merchant account
            string token = AppLogic.AppConfig("NETAXEPT.Merchant_Token");
            string merchantID = AppLogic.AppConfig("NETAXEPT.Merchant_Id");

            string responseText = string.Empty;
            string responseCode = string.Empty;

            // get the transaction string that was created of calling setup in the GetHTMLTicket
            string transactionString = Customer.Current.ThisCustomerSession["Nextaxept_TransactionString"];

            string transID = string.Empty;
            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS("SELECT AuthorizationPNREF, OrderTotal, transactionCommand FROM Orders   with (NOLOCK)  where OrderNumber=" + originalOrderNumber.ToString(), conn))
                {
                    if (rs.Read())
                    {
                        transID = DB.RSField(rs, "AuthorizationPNREF");
                        transactionString = DB.RSField(rs, "TransactionCommand");
                        
                        if (refundAmount == 0M)
                        {
                            refundAmount = DB.RSFieldDecimal(rs, "OrderTotal");                            
                        }
                    }
                }
            }

            TokenService service = new TokenService();

            // the server url
            string url = string.Empty;
            if (AppLogic.AppConfigBool("UseLiveTransactions"))
            {
                url = AppLogic.AppConfig("NETAXEPT.Live_Server"); // use live 
            }
            else
            {
                url = AppLogic.AppConfig("NETAXEPT.Test_Server"); // use test
            }

            service.Url = url;

            try
            {
                Result result = null;
                Result authResult = null;

                result = service.ProcessSetup(token, merchantID, transactionString);

                if (result.ResponseCode.Equals("OK", StringComparison.OrdinalIgnoreCase))
                {
                    authResult = service.Credit(token, merchantID, transID, refundReason,  RemoveSeperatorFromAmount(refundAmount));
                }

                DB.ExecuteSQL(string.Format("UPDATE Orders set RefundTXresult='{0}' where OrderNumber=" + originalOrderNumber.ToString(), authResult.ResponseCode));
            }
            catch (Exception ex)
            {
                SoapException se = ex as SoapException;

                if (se != null)
                {
                    GetStatusCodeMessage(se, out responseText, out responseCode);
                    DB.ExecuteSQL(string.Format("UPDATE Orders set RefundTXresult='{0}' WHERE OrderNumber=" + originalOrderNumber.ToString(), responseCode));
                   
                    if (!string.IsNullOrEmpty(responseText))
                    {
                        return responseText;
                    }
                }

                return ex.Message;
            }

            return AppLogic.ro_OK; 
        }

        /// <summary>
        /// Processes the card.
        /// </summary>
        /// <param name="orderNumber">The order number.</param>
        /// <param name="customerID">The customer ID.</param>
        /// <param name="orderTotal">The order total.</param>
        /// <param name="useLiveTransactions">if set to <c>true</c> [use live transactions].</param>
        /// <param name="transactionMode">The transaction mode.</param>
        /// <param name="useBillingAddress">The use billing address.</param>
        /// <param name="cardExtraCode">The card extra code.</param>
        /// <param name="useShippingAddress">The use shipping address.</param>
        /// <param name="CAVV">The cardholder authentication verification value.</param>
        /// <param name="ECI">The electronic commerce international .</param>
        /// <param name="XID">The transaction ID.</param>
        /// <param name="AVSResult">The avsResult.</param>
        /// <param name="authorizationResult">The authorization result.</param>
        /// <param name="authorizationCode">The authorization code.</param>
        /// <param name="authorizationTransID">The authorization trans ID.</param>
        /// <param name="transactionCommandOut">The transaction command out.</param>
        /// <param name="transactionResponse">The transaction response.</param>
        /// <returns> Status of the transaction </returns>
        public override String ProcessCard(int orderNumber, int customerID, decimal orderTotal, bool useLiveTransactions, TransactionModeEnum transactionMode, Address useBillingAddress, string cardExtraCode, Address useShippingAddress, string cavv, string eci, string transactionID, out string avsResult, out string authorizationResult, out string authorizationCode, out string authorizationTransID, out string transactionCommandOut, out string transactionResponse)
        {
            avsResult = "N/A";  // not supported
            authorizationResult = string.Empty;
            authorizationCode = string.Empty; 
            transactionCommandOut = string.Empty;
            transactionResponse = string.Empty;
            authorizationTransID = string.Empty;
        
            // Merchant account
            string token = AppLogic.AppConfig("NETAXEPT.Merchant_Token");
            string merchantID = AppLogic.AppConfig("NETAXEPT.Merchant_Id");
            
            // get the transaction string that was created of calling setup in the GetHTMLTicket
            string transactionString = Customer.Current.ThisCustomerSession["Nextaxept_TransactionString"];

            TokenService service = new TokenService();

            // the server url
            string url = string.Empty;
            if (AppLogic.AppConfigBool("UseLiveTransactions"))
            {
                url = AppLogic.AppConfig("NETAXEPT.Live_Server"); // use live 
            }
            else
            {
                url = AppLogic.AppConfig("NETAXEPT.Test_Server"); // use test
            }

            service.Url = url;

            Result result = null;
            Result authResult = null; 

            try
            {
                result = service.ProcessSetup(token, merchantID, transactionString);

                // We will use this for the transactionString that BBS provided to ProcessSetup
                transactionCommandOut = transactionString;
                
                // Response from BBS or issuer
                authorizationResult = result.ResponseCode;
               
                authorizationCode = result.AuthorizationCode;
               
                if (result.ResponseCode.Equals("OK", StringComparison.OrdinalIgnoreCase))
                {
                    if (transactionMode == TransactionModeEnum.auth)
                    {
                        // Auth - Allows you to authorize the availability of funds for a transaction 
                        //        but delay the capture of funds until a later time. 
                        //        This method has one additional parameter: batchReconRef (mandatory).
                        //        In standard implementation this should be empty.
                        authResult = service.Auth(token, merchantID, result.TransactionId, string.Empty);                        
                        
                        // The value returned should be the same as passed in
                        authorizationTransID = authResult.TransactionId;
                       
                        // Textual description of condition (usually available if an error occurs)
                        transactionResponse = authResult.ResponseText;                      
                    }
                    else
                    {
                        // sale - Allows you to authorize and capture at the same time.
                        //       This method has two additional parameters: transactionReconRef and batchReconRef
                        authResult = service.Sale(token, merchantID, result.TransactionId, string.Empty, string.Empty);
                        
                        authorizationTransID = authResult.TransactionId;
                        
                        // Textual description of condition (usually available if an error occurs)
                        transactionResponse = authResult.ResponseText;
                    }
                }
            }
            catch (Exception ex)
            {
                SoapException se = ex as SoapException;
           
                if (se != null)
                {
                    GetStatusCodeMessage(se, out transactionResponse, out authorizationResult);
                }

                return "Error:" + ex.Message;
            }  
          
            return AppLogic.ro_OK;
        }

        /// <summary>
        /// Gets the status code message.
        /// </summary>
        /// <param name="ex">The exception</param>
        /// <param name="responseText">The response text.</param>
        /// <param name="responseCode">The response code.</param>
        public static void GetStatusCodeMessage(SoapException ex, out string responseText, out string responseCode)
        {
            responseText = string.Empty;
            responseCode = string.Empty;

            if (ex.Detail["BBSException"] != null)
            {
                try 
                {
                    responseCode = ex.Detail["BBSException"]["Result"]["ResponseCode"].InnerXml;
                    responseText = ex.Detail["BBSException"]["Result"]["ResponseText"].InnerXml;   
                }
                catch 
                { 
                }
            }

            if (string.IsNullOrEmpty(responseText))
            {
                responseText = ex.Message;
            }
        }

        /// <summary>
        /// The transaction string that have provided by BBS UI Interface.
        /// To start off a payment operation, a setup-call needs to be made
        ///  This is always the first operation made for a transaction.
        /// </summary>
        /// <param name="cart">The shopping cart.</param>
        /// <param name="thisCustomer">The customer.</param>
        /// <param name="totalAmount">The total amount.</param>
        /// <returns> The transaction string </returns>
        public string GetTransactionString(ShoppingCart cart, Customer thisCustomer, decimal totalAmount)
        {
            // Merchant account
            string token = AppLogic.AppConfig("NETAXEPT.Merchant_Token");
            string merchantID = AppLogic.AppConfig("NETAXEPT.Merchant_Id");
            
            TokenService service = new TokenService();
            SetupRequest request = new SetupRequest();

            string responseCode = string.Empty;
            string responseText = string.Empty;
            
            // the server url
            string url = string.Empty;
            if (AppLogic.AppConfigBool("UseLiveTransactions"))
            {
                url = AppLogic.AppConfig("NETAXEPT.Live_Server"); // use live 
            }
            else
            {
                url = AppLogic.AppConfig("NETAXEPT.Test_Server"); // use test
            }

            service.Url = url;

            string orderNumber = AppLogic.GetNextOrderNumber().ToString();
           
            // setup request from parameters
            // Send order details to BBS
           
            // Set total amount that customer has to pay for the order. Do not use a seperator. Instead
            // use: 123,50 Norwegian kroner = "12350"
            request.Amount = RemoveSeperatorFromAmount(totalAmount);

            // Set currency for total amount
            request.CurrencyCode = thisCustomer.CurrencySetting.ToUpperInvariant();

            // Set order id (Ordrenummer). This is the store's order id.
            request.OrderNumber = orderNumber; 

            // Set the order description. You have up to 4096 characters available.
            request.OrderDescription = cart.OrderNotes; 

            // Set customers e-mail & phone (optional)
            request.CustomerEmail = thisCustomer.EMail;
            request.CustomerPhoneNumber = thisCustomer.PrimaryBillingAddress.Phone;

            // Set the description of this transaction. (optional)
            request.Description = string.Empty;

            // Language in BBS hosted UI
            request.Language = this.ReplaceLocalle(thisCustomer.LocaleSetting);

            // After payment information at BBS, customer will be redirected to:
            request.RedirectUrl = AppLogic.GetStoreHTTPLocation(AppLogic.AppConfigBool("UseSSL")) + AppLogic.AppConfig("NETAXEPT.MerchantSettings.RedirectUrl");

            // Set type of service: B = BBS hosted, M = Merchant hosted, C = Callcenter
            request.ServiceType = "B";

            // Set a unique id (up to 32 characters) for the transaction. 
            // This ID must be saved for later use when you want to "CAPTURE" or "CREDIT" the transaction. 
            request.TransactionId = Guid.NewGuid().ToString().Replace("-", string.Empty);

            // Set the session ID. This is typically used to recognize the returning customer if there is no cookie in the users browser. (optional)
            request.SessionId = thisCustomer.CustomerGUID.Replace("-", string.Empty);

            // this will hold the hidden input element
            // which we'll inject into the form
            // required to postback to nextApi
            string ticket = string.Empty;

            try
            {
                ticket = service.Setup(token, merchantID, request);
            }
            catch (Exception ex)
            {
                SoapException se = ex as SoapException;

                if (se != null)
                {
                    GetStatusCodeMessage(se, out responseText, out responseCode);
                }

                string sql = "INSERT INTO FailedTransaction(CustomerID,OrderNumber,IPAddress,OrderDate,PaymentGateway,PaymentMethod,TransactionCommand,TransactionResult) values(" + thisCustomer.CustomerID + "," + orderNumber + "," + DB.SQuote(cart.ThisCustomer.LastIPAddress) + ",getdate()," + DB.SQuote(Gateway.ro_GWNETAXEPT) + "," + DB.SQuote(AppLogic.ro_PMCreditCard) + "," + DB.SQuote("N/A") + "," + DB.SQuote(responseText) + ")";
                DB.ExecuteSQL(sql);
                return AppLogic.GetString("toc.aspx.6", thisCustomer.SkinID, thisCustomer.LocaleSetting) + ex.Message;
            }
          
            // Create a session that will handle the order number
            thisCustomer.ThisCustomerSession["Nextaxept_OrderNumber"] = orderNumber;
           
            return ticket;            
        }

        /// <summary>
        /// Removes the seperator from amount.
        /// </summary>
        /// <param name="totalAmount">The total amount.</param>
        /// <returns>The amount without seperator</returns>
        private static string RemoveSeperatorFromAmount(decimal totalAmount)
        {
            NumberFormatInfo nfi = new CultureInfo("en-US").NumberFormat;
            string total = totalAmount.ToString("#.00", nfi).Replace(".", string.Empty);

            return total;
        }

        /// <summary>
        /// Replace to locale to BBS UI Interface valid locale
        /// </summary>
        /// <param name="locale">The locale</param>
        /// <returns>BBS UI Interface valid locale</returns>
        private string ReplaceLocalle(string locale)
        {
            // The bbs gateway expecting a locale of no-NO
            if (locale.Equals("nb-NO", StringComparison.InvariantCultureIgnoreCase) ||
                locale.Equals("nn-NO", StringComparison.InvariantCultureIgnoreCase))
            {
                locale = "no_NO";
            }

            string bbsFormatLocal = locale.Replace("-", "_");
            
            return bbsFormatLocal;
        }

        public override string ProcessingPageRedirect()
        {
            return "~/NetaxeptCheckout.aspx";
        }
    }
}
