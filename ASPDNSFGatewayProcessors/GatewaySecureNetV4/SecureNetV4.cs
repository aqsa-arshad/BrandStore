// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways;
using AspDotNetStorefrontGateways.SecureNetAPIv411;
using System.ServiceModel;

namespace AspDotNetStorefrontGateways.Processors
{
	/// <summary>
    /// Summary description for SecureNet.
	/// </summary>
	public class SecureNetV4 : GatewayProcessor
	{
        public override String ProcessCard(int OrderNumber, int CustomerID, Decimal OrderTotal, bool useLiveTransactions, TransactionModeEnum TransactionMode, Address UseBillingAddress, String CardExtraCode, Address UseShippingAddress, String CAVV, String ECI, String XID, out String AVSResult, out String AuthorizationResult, out String AuthorizationCode, out String AuthorizationTransID, out String TransactionCommandOut, out String TransactionResponse)
        {
            Customer c = null;
            Boolean isVault = String.IsNullOrEmpty(UseBillingAddress.CardNumber);
            if (isVault)
                c = new Customer(CustomerID);
            return this.ProcessCard(OrderNumber, CustomerID, OrderTotal, useLiveTransactions, TransactionMode, UseBillingAddress, CardExtraCode, UseShippingAddress, CAVV, ECI, XID, out AVSResult, out AuthorizationResult, out AuthorizationCode, out AuthorizationTransID, out TransactionCommandOut, out TransactionResponse, isVault, c);
        }
        
        public String ProcessCard(int OrderNumber, int CustomerID, Decimal OrderTotal, bool useLiveTransactions, TransactionModeEnum TransactionMode, Address UseBillingAddress, String CardExtraCode, Address UseShippingAddress, String CAVV, String ECI, String XID, out String AVSResult, out String AuthorizationResult, out String AuthorizationCode, out String AuthorizationTransID, out String TransactionCommandOut, out String TransactionResponse, Boolean IsVaultTransaction, Customer ThisCustomer)
        {
            if (IsVaultTransaction && ThisCustomer == null)
                throw new ArgumentException("Customer Object required for vault transactions.");

            String result = AppLogic.ro_OK;
            AuthorizationCode = String.Empty;
            AuthorizationResult = String.Empty;
            AuthorizationTransID = String.Empty;
            AVSResult = String.Empty;
            TransactionCommandOut = String.Empty;
            TransactionResponse = String.Empty;

            GatewayClient client = SecureNetController.GetGatewayClient();

            TRANSACTION oT = SecureNetController.GetTransactionWithDefaults();

            if (IsVaultTransaction)
            {
                //vault info
                oT.CUSTOMERID = ThisCustomer.CustomerID.ToString();
                oT.PAYMENTID = AppLogic.GetSelectedSecureNetVault(ThisCustomer);
            }
            else
            {
                //Credit Card Info
                oT.CARD = new CARD();
                oT.CARD.CARDCODE = CardExtraCode;
                oT.CARD.CARDNUMBER = UseBillingAddress.CardNumber;
                oT.CARD.EXPDATE = UseBillingAddress.CardExpirationMonth.PadLeft(2, '0') + UseBillingAddress.CardExpirationYear.ToString().Substring(2, 2); //MMYY
            }


            //Billing Address Info
            oT.CUSTOMER_BILL = new CUSTOMER_BILL();
            oT.CUSTOMER_BILL.ADDRESS = UseBillingAddress.Address1;
            oT.CUSTOMER_BILL.CITY = UseBillingAddress.City;
            oT.CUSTOMER_BILL.ZIP = UseBillingAddress.Zip;
            oT.CUSTOMER_BILL.STATE = UseBillingAddress.State;
            oT.CUSTOMER_BILL.COMPANY = UseBillingAddress.Company;
            oT.CUSTOMER_BILL.COUNTRY = UseBillingAddress.Country;
            oT.CUSTOMER_BILL.EMAIL = UseBillingAddress.EMail;
            oT.CUSTOMER_BILL.FIRSTNAME = UseBillingAddress.FirstName;
            oT.CUSTOMER_BILL.LASTNAME = UseBillingAddress.LastName;
            oT.CUSTOMER_BILL.PHONE = UseBillingAddress.Phone;

            //Shipping Address Info
            if (UseShippingAddress != null)
            {
                oT.CUSTOMER_SHIP = new CUSTOMER_SHIP();
                oT.CUSTOMER_SHIP.ADDRESS = UseShippingAddress.Address1;
                oT.CUSTOMER_SHIP.CITY = UseShippingAddress.City;
                oT.CUSTOMER_SHIP.ZIP = UseShippingAddress.Zip;
                oT.CUSTOMER_SHIP.STATE = UseShippingAddress.State;
                oT.CUSTOMER_SHIP.COMPANY = UseShippingAddress.Company;
                oT.CUSTOMER_SHIP.COUNTRY = UseShippingAddress.Country;
                oT.CUSTOMER_SHIP.FIRSTNAME = UseShippingAddress.FirstName;
                oT.CUSTOMER_SHIP.LASTNAME = UseShippingAddress.LastName;
            }

            //todo - look into adding cartitems

            //Transaction Information
            oT.AMOUNT = OrderTotal;
            oT.CODE = CommonLogic.IIF(AppLogic.TransactionModeIsAuthOnly(), SecureNetController.GetTypeCodeString(SecureNetTransactionTypeCode.AUTH_ONLY), SecureNetController.GetTypeCodeString(SecureNetTransactionTypeCode.AUTH_CAPTURE));
            oT.METHOD = SecureNetMethod.CC.ToString();
            oT.ORDERID = OrderNumber.ToString();
            oT.CUSTOMERIP = CommonLogic.CustomerIpAddress();
            oT.INVOICENUM = OrderNumber.ToString();
            oT.INVOICEDESC = AppLogic.AppConfig("StoreName");

            //pasing unused integers as zeros as defined in the securenet docs
            oT.TOTAL_INSTALLMENTCOUNT = 0;
            oT.OVERRIDE_FROM = 0;
            oT.INSTALLMENT_SEQUENCENUM = 0;
            oT.RETAIL_LANENUM = 0;
            oT.CASHBACK_AMOUNT = 0;

            if (IsVaultTransaction)
                oT.TRANSACTION_SERVICE = 1;
            else
                oT.TRANSACTION_SERVICE = 0;

            //MPI for 3D Secure
            oT.MPI = new MPI();

            if (!String.IsNullOrEmpty(ECI))
                oT.MPI.AUTHINDICATOR = ECI;

            if (!String.IsNullOrEmpty(CAVV))
                oT.MPI.AUTHVALUE = CAVV;

            GATEWAYRESPONSE oG = client.ProcessTransaction(oT);

            if (oG.TRANSACTIONRESPONSE.RESPONSE_CODE == "1")
            {
                AuthorizationTransID = oG.TRANSACTIONRESPONSE.TRANSACTIONID.ToString();
                AuthorizationCode = "Response Code: " + oG.TRANSACTIONRESPONSE.RESPONSE_CODE + ", Reason Code: " + oG.TRANSACTIONRESPONSE.RESPONSE_REASON_CODE;

                if (!String.IsNullOrEmpty(oG.TRANSACTIONRESPONSE.AVS_RESULT_CODE))
                {
                    AVSResult = oG.TRANSACTIONRESPONSE.AVS_RESULT_CODE;
                }

                if (!String.IsNullOrEmpty(oG.TRANSACTIONRESPONSE.CARD_CODE_RESPONSE_CODE))
                {
                    if (AVSResult.Length > 0)
                    {
                        AVSResult += ", ";
                    }
                    AVSResult += "ExtraCode: " + oG.TRANSACTIONRESPONSE.CARD_CODE_RESPONSE_CODE;
                }

                //if (!String.IsNullOrEmpty(oTr.CAVV_Response_Code))
                if (!String.IsNullOrEmpty(oG.TRANSACTIONRESPONSE.CAVV_RESPONSE_CODE))
                {
                    if (AVSResult.Length > 0)
                    {
                        AVSResult += ", ";
                    }
                    AVSResult += "CAVV: " + oG.TRANSACTIONRESPONSE.CAVV_RESPONSE_CODE;
                }

                AuthorizationResult = oG.TRANSACTIONRESPONSE.RESPONSE_REASON_TEXT + ", Approval Code: " + oG.TRANSACTIONRESPONSE.AUTHCODE;
                result = AppLogic.ro_OK;
            }
            else
            {
                AuthorizationResult = "Error: [" + oG.TRANSACTIONRESPONSE.RESPONSE_CODE + "] " + oG.TRANSACTIONRESPONSE.RESPONSE_REASON_TEXT;
                result = oG.TRANSACTIONRESPONSE.RESPONSE_REASON_TEXT;
            }

            TransactionCommandOut = this.GetXMLSerializedObject(oT);
            TransactionResponse = this.GetXMLSerializedObject(oG);

            if (!IsVaultTransaction && AppLogic.SecureNetVaultIsEnabled() && result == AppLogic.ro_OK)
            {
                if (ThisCustomer == null)
                    ThisCustomer = new Customer(CustomerID);
                if (ThisCustomer.SecureNetVaultMasterShouldWeStoreCreditCardInfo)
                {
                    try
                    {
                        SecureNetVault vault = new SecureNetVault(ThisCustomer);
                        vault.AddCreditCardToCustomerVault(UseBillingAddress.CardName, UseBillingAddress.CardNumber, CardExtraCode, UseBillingAddress.CardType, UseBillingAddress.CardExpirationMonth, UseBillingAddress.CardExpirationYear);
                    }
                    catch { }
                }
            }

            if (IsVaultTransaction && result == AppLogic.ro_OK)
                AppLogic.ClearSelectedSecureNetVaultInSession(ThisCustomer);

            return result;
        }

        private string GetXMLSerializedObject(Object o)
        {
            String ret = XmlCommon.SerializeObject(o, o.GetType());
            string storeappconfigtomunge;
            List<int> storeids = new List<int>();
            storeids.Add(0);
            foreach (Store s in Store.GetStoreList())
            {
                if (!storeids.Contains(s.StoreID))
                    storeids.Add(s.StoreID);
            }
            foreach (int sid in storeids)
            {
                storeappconfigtomunge = AppLogic.AppConfig("SecureNet.Key", sid, true);
                if (!String.IsNullOrEmpty(storeappconfigtomunge))
                    ret = ret.Replace(">" + storeappconfigtomunge + "<", ">*****<");
            }
            return ret;
        }

        public override String CaptureOrder(Order o)
		{
            String result = AppLogic.ro_OK;
            o.CaptureTXCommand = "";
            o.CaptureTXResult = "";

            String TransID = o.AuthorizationPNREF;

            if (TransID.Length == 0 || TransID == "0")
            {
                result = "Invalid or Empty Transaction ID";
            }
            else
            {
                try
                {
                    GatewayClient client = SecureNetController.GetGatewayClient();

                    TRANSACTION oT = SecureNetController.GetTransactionWithDefaults();

                    oT.CODE = SecureNetController.GetTypeCodeString(SecureNetTransactionTypeCode.PRIOR_AUTH_CAPTURE);

                    oT.AMOUNT = o.OrderBalance;
                    oT.METHOD = SecureNetMethod.CC.ToString();

                    oT.REF_TRANSID = TransID;
                    oT.ORDERID = o.OrderNumber.ToString();

                    String cardnumber = DB.GetSqlS("select Last4 S from Orders  with (NOLOCK)  where OrderNumber=" + o.OrderNumber.ToString());
                    if (!String.IsNullOrEmpty(cardnumber))
                    {
                        oT.CARD = new CARD();
                        oT.CARD.CARDNUMBER = cardnumber;
                        oT.CARD.EXPDATE = o.CardExpirationMonth.PadLeft(2, '0') + o.CardExpirationYear.ToString().Substring(2, 2); //MMYY
                    }

                    oT.INVOICENUM = o.OrderNumber.ToString();
                    oT.INVOICEDESC = AppLogic.AppConfig("StoreName");

                    GATEWAYRESPONSE oTr = client.ProcessTransaction(oT);

                    if (oTr.TRANSACTIONRESPONSE.RESPONSE_CODE == "1") // 1=Approved, 2=Declined, 3=Error
                    {
                        result = AppLogic.ro_OK;
                    }
                    else
                    {
                        result = "Error: [" + oTr.TRANSACTIONRESPONSE.RESPONSE_CODE + "] " + oTr.TRANSACTIONRESPONSE.RESPONSE_REASON_TEXT;
                    }
                    o.CaptureTXCommand = this.GetXMLSerializedObject(oT);
                    o.CaptureTXResult = this.GetXMLSerializedObject(oTr);
                }
                catch
                {
                    result = "NO RESPONSE FROM GATEWAY!";
                }
            }
            return result;
		}

        public override String VoidOrder(int OrderNumber)
		{
            String result = String.Empty;

            DB.ExecuteSQL("update orders set VoidTXCommand=NULL, VoidTXResult=NULL where OrderNumber=" + OrderNumber.ToString());
            String TransID = String.Empty;
            decimal OrderTotal = 0.0M;
            string Last4 = string.Empty;
            string CardExpirationMonth = string.Empty;
            string CardExpirationYear = string.Empty;

            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("select * from orders   with (NOLOCK)  where OrderNumber=" + OrderNumber.ToString(), dbconn))
                {
                    if (rs.Read())
                    {
                        // If you are voiding a transaction that has been reauthorized, 
                        // use the ID from the original authorization, and not the reauthorization.
                        TransID = DB.RSField(rs, "AuthorizationPNREF");
                        OrderTotal = DB.RSFieldDecimal(rs, "OrderTotal");
                        Last4 = DB.RSField(rs, "Last4");
                        CardExpirationMonth = DB.RSField(rs, "CardExpirationMonth");
                        CardExpirationYear = DB.RSField(rs, "CardExpirationYear");
                    }
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

                    GatewayClient client = SecureNetController.GetGatewayClient();
                    TRANSACTION oT = SecureNetController.GetTransactionWithDefaults();

                    // AUTH_CAPTURE, AUTH_ONLY, CREDIT, VOID, PRIOR_AUTH_CAPTURE

                    oT.CODE = SecureNetController.GetTypeCodeString(SecureNetTransactionTypeCode.VOID);
                    oT.METHOD = SecureNetMethod.CC.ToString();

                    oT.REF_TRANSID = TransID;
                    oT.ORDERID = OrderNumber.ToString();
                    oT.AMOUNT = OrderTotal;

                    if (!String.IsNullOrEmpty(Last4))
                    {
                        oT.CARD = new CARD();
                        oT.CARD.CARDNUMBER = Last4;
                        oT.CARD.EXPDATE = CardExpirationMonth.PadLeft(2, '0') + CardExpirationYear.ToString().Substring(2, 2); //MMYY
                    }

                    GATEWAYRESPONSE oTr = client.ProcessTransaction(oT);


                    if (oTr.TRANSACTIONRESPONSE.RESPONSE_CODE == "1") // 1=Approved, 2=Declined, 3=Error
                    {
                        result = AppLogic.ro_OK;
                    }
                    else
                    {
                        result = "Error: [" + oTr.TRANSACTIONRESPONSE.RESPONSE_CODE + "] " + oTr.TRANSACTIONRESPONSE.RESPONSE_REASON_TEXT;
                    }

                    DB.ExecuteSQL("update orders set VoidTXCommand=" + DB.SQuote(this.GetXMLSerializedObject(oT))
                        + ", VoidTXResult=" + DB.SQuote(this.GetXMLSerializedObject(oTr)) + " where OrderNumber=" + OrderNumber.ToString());
                }
                catch
                {
                    result = "NO RESPONSE FROM GATEWAY!";
                }
            }
            return result;
		}

        public override String RefundOrder(int OriginalOrderNumber, int NewOrderNumber, decimal RefundAmount, String RefundReason, Address UseBillingAddress)
        {
            String result = String.Empty;

            DB.ExecuteSQL("update orders set RefundTXCommand=NULL, RefundTXResult=NULL where OrderNumber=" + OriginalOrderNumber.ToString());
            String TransID = String.Empty;
            Decimal OrderTotal = System.Decimal.Zero;
            string Last4 = string.Empty;
            string CardExpirationMonth = string.Empty;
            string CardExpirationYear = string.Empty;

            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("select * from Orders  with (NOLOCK)  where OrderNumber=" + OriginalOrderNumber.ToString(), dbconn))
                {
                    if (rs.Read())
                    {
                        TransID = DB.RSField(rs, "AuthorizationPNREF");
                        OrderTotal = DB.RSFieldDecimal(rs, "OrderTotal");
                        Last4 = DB.RSField(rs, "Last4");
                        CardExpirationMonth = DB.RSField(rs, "CardExpirationMonth");
                        CardExpirationYear = DB.RSField(rs, "CardExpirationYear");
                    }
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
                    GatewayClient client = SecureNetController.GetGatewayClient();
                    TRANSACTION oT = SecureNetController.GetTransactionWithDefaults();

                    // AUTH_CAPTURE, AUTH_ONLY, CREDIT, VOID, PRIOR_AUTH_CAPTURE
                    oT.CODE = SecureNetController.GetTypeCodeString(SecureNetTransactionTypeCode.CREDIT);

                    //If partial refund set value ( like 1.95). If FULL Refund leave it empty. The transactionID will take care of the amount
                    if (OrderTotal == RefundAmount || RefundAmount == 0.0M)
                        oT.AMOUNT = OrderTotal;
                    else
                        oT.AMOUNT = RefundAmount;

                    if (!String.IsNullOrEmpty(Last4))
                    {
                        oT.CARD = new CARD();
                        oT.CARD.CARDNUMBER = Last4;
                        oT.CARD.EXPDATE = CardExpirationMonth.PadLeft(2, '0') + CardExpirationYear.ToString().Substring(2, 2); //MMYY
                    }

                    if (!String.IsNullOrEmpty(RefundReason))
                        oT.NOTE = RefundReason;

                    oT.METHOD = SecureNetMethod.CC.ToString();

                    oT.REF_TRANSID = TransID;
                    oT.ORDERID = OriginalOrderNumber.ToString()+ "REFUND" + DateTime.Today.ToShortDateString().Replace("/", "");

                    GATEWAYRESPONSE oTr = client.ProcessTransaction(oT);

                    if (oTr.TRANSACTIONRESPONSE.RESPONSE_CODE == "1") // 1=Approved, 2=Declined, 3=Error
                    {
                        result = AppLogic.ro_OK;
                    }
                    else
                    {
                        result = "Error: [" + oTr.TRANSACTIONRESPONSE.RESPONSE_CODE + "] " + oTr.TRANSACTIONRESPONSE.RESPONSE_REASON_TEXT;
                    }

                    DB.ExecuteSQL("update orders set RefundTXCommand=" + DB.SQuote(this.GetXMLSerializedObject(oT))
                        + ", RefundTXResult=" + DB.SQuote(this.GetXMLSerializedObject(oTr)) + " where OrderNumber=" + OriginalOrderNumber.ToString());

                }
                catch
                {
                    result = "NO RESPONSE FROM GATEWAY!";
                }
            }
            return result;
        }

        //public override IConfigurationAtom GetConfigurationAtom()
        //{
        //    List<AppConfigAtomInfo> configs = new List<AppConfigAtomInfo>();

        //    AppConfig SecureNetID = AppConfigManager.GetAppConfig(0, "SecureNet.ID");
        //    if (SecureNetID != null)
        //        configs.Add(new AppConfigAtomInfo(SecureNetID, true, string.Empty));

        //    AppConfig SecureNetKey = AppConfigManager.GetAppConfig(0, "SecureNet.Key");
        //    if (SecureNetKey != null)
        //        configs.Add(new AppConfigAtomInfo(SecureNetKey, true, "Your SecureNet key. Note that this is used for both SecureNet versions."));

        //    AppConfig SecureNetV4UseTestMode = AppConfigManager.GetAppConfig(0, "SecureNetV4.UseTestMode");
        //    if (SecureNetV4UseTestMode != null)
        //        configs.Add(new AppConfigAtomInfo(SecureNetV4UseTestMode, false, string.Empty));

        //    AppConfig SecureNetV4VaultEnabled = AppConfigManager.GetAppConfig(0, "SecureNetV4.VaultEnabled");
        //    if (SecureNetV4VaultEnabled != null)
        //        configs.Add(new AppConfigAtomInfo(SecureNetV4VaultEnabled, false, string.Empty));

        //    return new ConfigurationAtom(configs, "");
        //}
	}
}
