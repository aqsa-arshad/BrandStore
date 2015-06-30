// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways.SagePaymentsAPI;
using System.Data.SqlClient;
using System.Xml.Linq;

namespace AspDotNetStorefrontGateways.Processors
{
    /// <summary>
    /// Summary description for SagePayments.
    /// </summary>
    class SagePayments : GatewayProcessor
    {

        string M_id;
        string M_key;

        public SagePayments() 
        {
            M_id = AppLogic.AppConfig("SagePayments.MERCHANT_ID");
            M_key = AppLogic.AppConfig("SagePayments.MERCHANT_KEY");
        }
        /// <summary>
        /// Captures the transaction that was previuosly authorized
        /// </summary>
        /// <param name="o">The sales order</param>
        /// <returns>Status</returns>
		public override String CaptureOrder(Order o)
		{
            String result = AppLogic.ro_OK;

            o.CaptureTXCommand = "";
            o.CaptureTXResult = "";

            String TransID = o.AuthorizationPNREF;
            Decimal TotalAmount = o.OrderBalance;

            TRANSACTION_PROCESSING tp = new TRANSACTION_PROCESSING();
            if (AppLogic.AppConfigBool("UseLiveTransactions"))
            {
                tp.Url = AppLogic.AppConfig("SagePayments.ServiceURL");
            }
            else
            {
                tp.Url = AppLogic.AppConfig("SagePayments.ServiceURLTEST");
            }
            DataSet ds = tp.BANKCARD_PRIOR_AUTH_SALE(M_id, M_key, TotalAmount.ToString("0.00"), "", "", "", TransID);

            o.CaptureTXCommand = ds.GetXml();

			if (ds.Tables[0].Rows[0]["APPROVAL_INDICATOR"].ToString() == "A")
			{
				result = AppLogic.ro_OK;
			}
			else
			{
				result = ds.Tables[0].Rows[0]["MESSAGE"].ToString();
			}

            return result;
        }
        
        /// <summary>
        /// Voids the sales order
        /// </summary>
        /// <param name="OrderNumber">The order number</param>
        /// <returns>Status</returns>
		public override String VoidOrder(int OrderNumber)
		{
            String result = AppLogic.ro_OK;

			String TransID = String.Empty;

            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS("select AuthorizationPNREF from orders   with (NOLOCK)  where OrderNumber=" + OrderNumber.ToString(), conn))
                {
                    if (rs.Read())
                    {
                        TransID = DB.RSField(rs, "AuthorizationPNREF");
                    }                    
                }
            }            

            TRANSACTION_PROCESSING tp = new TRANSACTION_PROCESSING();
            if (AppLogic.AppConfigBool("UseLiveTransactions"))
            {
                tp.Url = AppLogic.AppConfig("SagePayments.ServiceURL");
            }
            else
            {
                tp.Url = AppLogic.AppConfig("SagePayments.ServiceURLTEST");
            }
            DataSet ds = tp.BANKCARD_VOID(M_id, M_key, TransID);

			DB.ExecuteSQL("update orders set VoidTXResult=" + DB.SQuote(ds.GetXml()) + " where OrderNumber=" + OrderNumber.ToString());

			if (ds.Tables[0].Rows[0]["APPROVAL_INDICATOR"].ToString() == "A")
			{
				result = AppLogic.ro_OK;
			}
			else
			{
				result = ds.Tables[0].Rows[0]["MESSAGE"].ToString();
			}

            return result;
        }

		// if RefundAmount == 0.0M, then then ENTIRE order amount will be refunded!
        /// <summary>
        /// Refunds the order
        /// </summary>
        /// <param name="OriginalOrderNumber"></param>
        /// <param name="NewOrderNumber">The new order number</param>
        /// <param name="RefundAmount">The refund amount</param>
        /// <param name="RefundReason">the reson</param>
        /// <param name="UseBillingAddress">The billing address to use</param>
        /// <returns>Status</returns>
        public override String RefundOrder(int OriginalOrderNumber, int NewOrderNumber, decimal RefundAmount, String RefundReason, Address UseBillingAddress)
        {
            String result = AppLogic.ro_OK;

            String TransID = String.Empty;
            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS("select AuthorizationPNREF, OrderTotal from orders   with (NOLOCK)  where OrderNumber=" + OriginalOrderNumber.ToString(), conn))
                {
                    if (rs.Read())
                    {
                        TransID = DB.RSField(rs, "AuthorizationPNREF");
                        if (RefundAmount == 0M)
                        {
                            RefundAmount = DB.RSFieldDecimal(rs, "OrderTotal");
                        }
                    }                    
                }
            }

            TRANSACTION_PROCESSING tp = new TRANSACTION_PROCESSING();
            if (AppLogic.AppConfigBool("UseLiveTransactions"))
            {
                tp.Url = AppLogic.AppConfig("SagePayments.ServiceURL");
            }
            else
            {
                tp.Url = AppLogic.AppConfig("SagePayments.ServiceURLTEST");
            }
            DataSet ds = tp.BANKCARD_CREDIT(M_id, M_key, RefundAmount.ToString("0.00"), TransID);

			DB.ExecuteSQL("update orders set RefundTXResult=" + DB.SQuote(ds.GetXml()) + " where OrderNumber=" + OriginalOrderNumber.ToString());

			if (ds.Tables[0].Rows[0]["APPROVAL_INDICATOR"].ToString() == "A")
			{
				result = AppLogic.ro_OK;
			}
			else
			{
				result = ds.Tables[0].Rows[0]["MESSAGE"].ToString();
			}

            return result;
        }

        /// <summary>
        /// Authorizes and/or captures the order thru this gateway
        /// </summary>
        /// <param name="OrderNumber">The order number</param>
        /// <param name="CustomerID">The customer id</param>
        /// <param name="OrderTotal">The order total</param>
        /// <param name="useLiveTransactions">Whether to use live transaction</param>
        /// <param name="TransactionMode">The transaction mode</param>
        /// <param name="UseBillingAddress">The billing address to use</param>
        /// <param name="CardExtraCode">The card extra code</param>
        /// <param name="UseShippingAddress">The shipping address to use</param>
        /// <param name="CAVV">The CAVV</param>
        /// <param name="ECI">The ECI</param>
        /// <param name="XID">The XID</param>
        /// <param name="AVSResult">The AVS result</param>
        /// <param name="AuthorizationResult">The Authorozation result</param>
        /// <param name="AuthorizationCode">The Auhtorization code</param>
        /// <param name="AuthorizationTransID">The Authorixation transaction id</param>
        /// <param name="TransactionCommandOut">The transaction command sent to the gateway</param>
        /// <param name="TransactionResponse">The transaction detail receive from the gateway</param>
        /// <returns>Status</returns>
        public override String ProcessCard(int OrderNumber, int CustomerID, Decimal OrderTotal, bool useLiveTransactions, TransactionModeEnum TransactionMode, Address UseBillingAddress, string CardExtraCode, Address UseShippingAddress, String CAVV, String ECI, String XID, out String AVSResult, out String AuthorizationResult, out String AuthorizationCode, out String AuthorizationTransID, out String TransactionCommandOut, out String TransactionResponse)
		{
            DataSet ds = null;
			String result = AppLogic.ro_OK;
            TransactionCommandOut = "";

            TRANSACTION_PROCESSING tp = new TRANSACTION_PROCESSING();
            if (AppLogic.AppConfigBool("UseLiveTransactions"))
            {
                tp.Url = AppLogic.AppConfig("SagePayments.ServiceURL");
            }
            else
            {
                tp.Url = AppLogic.AppConfig("SagePayments.ServiceURLTEST");
            }

			ds = ProcessServiceTransaction(tp, UseBillingAddress, UseShippingAddress, CardExtraCode, CustomerID, OrderTotal, OrderNumber, TransactionMode, ref TransactionCommandOut);

			AVSResult = ds.Tables[0].Rows[0]["AVS_INDICATOR"].ToString();
			AuthorizationResult = ds.Tables[0].Rows[0]["MESSAGE"].ToString();
			AuthorizationCode = ds.Tables[0].Rows[0]["CODE"].ToString();
			AuthorizationTransID = ds.Tables[0].Rows[0]["REFERENCE"].ToString(); ;
            TransactionResponse = ds.GetXml();

            if (ds.Tables[0].Rows[0]["APPROVAL_INDICATOR"].ToString() != "A")
            {
                //  Declined
                result = AuthorizationResult;
            }

			return result;
		}

        private DataSet ProcessServiceTransaction(TRANSACTION_PROCESSING transactionProcessor, Address billingAddress, Address shippingAddress, 
			string cardExtraCode, int customerId, decimal orderTotal, int orderNumber, TransactionModeEnum transactionMode, ref string transactionCommand)
        {

            // This is not the real transaction command, we are just building up a list of key value pairs being passed into the web service method and kicking them out as xml.
			transactionCommand = GetProcessCardCommand(billingAddress, shippingAddress, cardExtraCode, customerId.ToString(), orderTotal.ToString("0.00"), "", "", orderNumber.ToString(), transactionMode).ToString();


            if (transactionMode == TransactionModeEnum.auth) //  AUTH_ONLY
            {
                return transactionProcessor.BANKCARD_AUTHONLY(M_id, M_key, billingAddress.FirstName + ' ' + billingAddress.LastName, billingAddress.Address1, billingAddress.City, billingAddress.State, billingAddress.Zip,
                    billingAddress.Country, billingAddress.EMail, billingAddress.CardNumber, billingAddress.CardExpirationMonth.PadLeft(2, '0') + billingAddress.CardExpirationYear.ToString().Substring(2, 2),
                    cardExtraCode, customerId.ToString(), orderTotal.ToString("0.00"), "", "", orderNumber.ToString(), billingAddress.Phone, "", shippingAddress.FirstName + ' ' + shippingAddress.LastName,
                    shippingAddress.Address1, shippingAddress.City, shippingAddress.State, shippingAddress.Zip, shippingAddress.Country);
            }
            else //  AUTH_CAPTURE
            {
                return transactionProcessor.BANKCARD_SALE(M_id, M_key, billingAddress.FirstName + ' ' + billingAddress.LastName, billingAddress.Address1, billingAddress.City, billingAddress.State, billingAddress.Zip,
                    billingAddress.Country, billingAddress.EMail, billingAddress.CardNumber, billingAddress.CardExpirationMonth.PadLeft(2, '0') + billingAddress.CardExpirationYear.ToString().Substring(2, 2),
                    cardExtraCode, customerId.ToString(), orderTotal.ToString("0.00"), "", "", orderNumber.ToString(), billingAddress.Phone, "", shippingAddress.FirstName + ' ' + shippingAddress.LastName,
                    shippingAddress.Address1, shippingAddress.City, shippingAddress.State, shippingAddress.Zip, shippingAddress.Country);
            }

        }

		private XDocument GetProcessCardCommand(Address billingAddress, Address shippingAddress, string cardExtraCode, string customerId, string orderTotal, string orderShipping, string orderTax, string orderNumber, TransactionModeEnum transactionMode)
        {
            string cardNumber = AppLogic.SafeDisplayCardNumber(billingAddress.CardNumber, "", 0);           

            return new XDocument(
                new XElement("Transaction", new XAttribute("TransactionMode", transactionMode.ToString())
                    , new XElement("M_ID", M_id)
                    , new XElement("M_KEY", M_key) 
                    , new XElement("C_NAME", billingAddress.FirstName + ' ' + billingAddress.LastName) 
                    , new XElement("C_ADDRESS", billingAddress.Address1) 
                    , new XElement("C_CITY", billingAddress.City) 
                    , new XElement("C_STATE", billingAddress.State) 
                    , new XElement("C_ZIP", billingAddress.Zip) 
                    , new XElement("C_COUNTRY", billingAddress.Country) 
                    , new XElement("C_EMAIL", billingAddress.EMail) 
                    , new XElement("C_CARDNUMBER", cardNumber) 
                    , new XElement("C_EXP", billingAddress.CardExpirationMonth.PadLeft(2, '0') + billingAddress.CardExpirationYear.ToString().Substring(2, 2))
					, new XElement("C_CVV", cardExtraCode) 
                    , new XElement("T_CUSTOMER_NUMBER", customerId) 
                    , new XElement("T_AMT", orderTotal) 
                    , new XElement("T_SHIPPING", orderShipping) 
                    , new XElement("T_TAX", orderTax) 
                    , new XElement("T_ORDERNUM", orderNumber) 
                    , new XElement("C_TELEPHONE", billingAddress.Phone) 
                    , new XElement("C_FAX", "") 
                    , new XElement("C_SHIP_NAME", shippingAddress.FirstName + ' ' + shippingAddress.LastName) 
                    , new XElement("C_SHIP_ADDRESS", shippingAddress.Address1) 
                    , new XElement("C_SHIP_CITY", shippingAddress.City) 
                    , new XElement("C_SHIP_STATE", shippingAddress.State) 
                    , new XElement("C_SHIP_ZIP", shippingAddress.Zip) 
                    , new XElement("C_SHIP_COUNTRY", shippingAddress.Country)
                    ));
        }

        public override String DisplayName(String LocaleSetting)
        {
            return "Sage Payments";
        }
    }
}
