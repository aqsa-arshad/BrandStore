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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;


namespace AspDotNetStorefrontCore
{
    /// <summary>
    /// Summary description for Order.
    /// </summary>
    /// 

    public struct OrderItemShipping
    {
        public String m_ShippingTrackingNumber;
        public String m_ShippedVIA;
        public int m_ShippingMethodID; // set from FIRST order item. Not valid for multi-ship orders
        public String m_ShippingMethod; // set from FIRST order item. Not valid for multi-ship orders
        public int m_ShippingCalculationID;
    }

    public struct AddressInfo
    {
        public String m_NickName;
        public String m_FirstName;
        public String m_LastName;
        public String m_Company;
        public ResidenceTypes m_ResidenceType;
        public String m_Address1;
        public String m_Address2;
        public String m_Suite;
        public String m_City;
        public String m_State;
        public String m_Zip;
        public String m_Country;
        public String m_Phone;
        public String m_EMail;
    }

    public class OrderTransaction
    {
        public int OrderTransactionID { get; protected set; }
        public int OrderNumber { get; protected set; }
        public String TransactionType { get; protected set; }
        public String TransactionCommand { get; protected set; }
        public String TransactionResult { get; protected set; }
        public String PNREF { get; protected set; }
        public String Code { get; protected set; }
        public String PaymentMethod { get; protected set; }
        public String PaymentGateway { get; protected set; }
        public decimal Amount { get; protected set; }

        public OrderTransaction(int orderTransactionID)
        {
            this.OrderTransactionID = orderTransactionID;
            LoadFromDB();
        }

        public OrderTransaction(int orderTransactionID, int orderNumber, String transactionType, String transactionCommand, String transactionResult, String pnref, String code, String paymentMethod, String paymentGateway, decimal amount)
        {
            this.OrderTransactionID = orderTransactionID;
            this.OrderNumber = orderNumber;
            this.TransactionType = transactionType;
            this.TransactionCommand = transactionCommand;
            this.TransactionResult = transactionResult;
            this.PNREF = pnref;
            this.Code = code;
            this.PaymentMethod = paymentMethod;
            this.PaymentGateway = paymentGateway;
            this.Amount = amount;
        }

        private void LoadFromDB()
        {
            if (OrderTransactionID < 1)
            {
                Clear();
                return;
            }
            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select * from OrderTransaction where OrderTransactionID = {0}".FormatWith(OrderTransactionID), con))
                {
                    if (!rs.Read())
                    {
                        this.Clear();
                        return;
                    }
                    OrderTransactionID = DB.RSFieldInt(rs, "OrderTransactionID");
                    OrderNumber = DB.RSFieldInt(rs, "OrderNumber");
                    TransactionType = DB.RSField(rs, "TransactionType");
                    TransactionCommand = DB.RSField(rs, "TransactionCommand");
                    TransactionResult = DB.RSField(rs, "TransactionResult");
                    PNREF = DB.RSField(rs, "PNREF");
                    Code = DB.RSField(rs, "Code");
                    PaymentMethod = DB.RSField(rs, "PaymentMethod");
                    PaymentGateway = DB.RSField(rs, "PaymentGateway");
                    Amount = DB.RSFieldDecimal(rs, "Amount");
                }
            }
        }

        private void Clear()
        {
            OrderTransactionID = OrderNumber = 0;
            TransactionType =
                TransactionCommand =
                TransactionResult =
                PNREF =
                Code =
                PaymentMethod =
                PaymentGateway = null;
            Amount = 0;
        }

        public static int LookupOrderNumber(String transactionType, String transactionCommand, String transactionResult, String pnref, String code, String paymentMethod, String paymentGateway)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("select ordernumber as N from ordertransaction where 1=1 ");
            if (transactionType != null)
                sql.Append(" and transactiontype = " + DB.SQuote(transactionType));
            if (transactionCommand != null)
                sql.Append(" and transactionCommand = " + DB.SQuote(transactionCommand));
            if (transactionResult != null)
                sql.Append(" and transactionResult = " + DB.SQuote(transactionResult));
            if (pnref != null)
                sql.Append(" and pnref = " + DB.SQuote(pnref));
            if (code != null)
                sql.Append(" and code = " + DB.SQuote(code));
            if (paymentMethod != null)
                sql.Append(" and paymentMethod = " + DB.SQuote(paymentMethod));
            if (paymentGateway != null)
                sql.Append(" and paymentGateway = " + DB.SQuote(paymentGateway));

            sql.Append(" order by ordernumber desc");

            return DB.GetSqlN(sql.ToString());
        }
    }

    public class OrderTransactionCollection
    {
        private int m_OrderNumber;
        public List<OrderTransaction> Transactions { get; protected set; }

        public OrderTransactionCollection(int orderNumber)
        {
            m_OrderNumber = orderNumber;
            LoadFromDB(orderNumber);
        }

        private void LoadFromDB(int orderNumber)
        {
            Transactions = new List<OrderTransaction>();

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select * from OrderTransaction where orderNumber = {0}".FormatWith(orderNumber), con))
                {
                    while (rs.Read())
                    {
                        Transactions.Add(new OrderTransaction(
                            DB.RSFieldInt(rs, "OrderTransactionID"),
                            DB.RSFieldInt(rs, "orderNumber"),
                            DB.RSField(rs, "TransactionType"),
                            DB.RSField(rs, "TransactionCommand"),
                            DB.RSField(rs, "TransactionResult"),
                            DB.RSField(rs, "PNREF"),
                            DB.RSField(rs, "Code"),
                            DB.RSField(rs, "PaymentMethod"),
                            DB.RSField(rs, "PaymentGateway"),
                            DB.RSFieldDecimal(rs, "Amount")
                        ));
                    }
                }
            }
        }

        public void AddTransaction(String transactionType, String transactionCommand, String transactionResult, String pnref, String code, String paymentMethod, String paymentGateway, decimal amount)
        {
            string insertStatement = "INSERT INTO [OrderTransaction]([orderNumber],[TransactionType],[TransactionCommand],[TransactionResult],[PNREF],[Code],[PaymentMethod],[PaymentGateway],[Amount]) VALUES ({0},{1},{2},{3},{4},{5},{6},{7},{8})";
			DB.ExecuteSQL(insertStatement.FormatWith(m_OrderNumber, CheckNullAndQuote(transactionType), CheckNullAndQuote(transactionCommand), CheckNullAndQuote(transactionResult), CheckNullAndQuote(pnref), CheckNullAndQuote(code), CheckNullAndQuote(paymentMethod), CheckNullAndQuote(paymentGateway), CheckNullAndQuote(Localization.CurrencyStringForDBWithoutExchangeRate(amount))));
            LoadFromDB(m_OrderNumber);
        }

        private string CheckNullAndQuote(string dbString)
        {
            if (dbString == null)
                return "NULL";

            return DB.SQuote(dbString);
        }
    }

    public class Order
    {
        private int m_CustomerID;
        private CartTypeEnum m_CartType;
        private int m_OrderNumber;
        private bool m_IsNew;
        private bool m_IsEmpty;
        private Decimal m_OrderWeight;
        private String m_TransactionState;
        private AppLogic.TransactionTypeEnum m_TransactionType; // just int here to avoid circular ref to gateways dll
        private String m_AVSResult;
        private DateTime m_AuthorizedOn;
        private DateTime m_CapturedOn;
        private DateTime m_VoidedOn;
        private DateTime m_RefundedOn;
        private DateTime m_FraudedOn;
        private DateTime m_EditedOn;
        private String m_PaymentGateway;
        private String m_PaymentMethod;
        private String m_OrderNotes;
        private String m_FinalizationData;
        private String m_OrderOptions;
        private String m_PONumber;
        private String m_LocaleSetting;
        private String m_LastIPAddress;
        private String m_ViewInLocaleSetting;
        private DateTime m_ReceiptEMailSentOn;
        private DateTime m_ShippedOn;
        private DateTime m_DownloadEMailSentOn;
        private DateTime m_DistributorEMailSentOn;
        private String m_CustomerServiceNotes;
        private int m_ParentOrderNumber;
        private int m_RelatedOrderNumber;
        private String m_ChildOrderNumbers;
        private bool m_AlreadyConfirmed;

        private String m_RecurringSubscriptionID;

        private String m_ShippingTrackingNumber;
        private String m_ShippedVIA;
        private int m_ShippingMethodID;
        private String m_ShippingMethod;
        private int m_ShippingCalculationID;

        private CouponObject m_Coupon = new CouponObject();
        private CartItemCollection m_CartItems = new CartItemCollection();
        private AddressInfo m_ShippingAddress;
        private Decimal m_ShippingTotal;
        private AddressInfo m_BillingAddress;
        private DateTime m_OrderDate;
        private Decimal m_Total;
        private Decimal m_SubTotal;
        private Decimal m_TaxTotal;
        private String m_CardType;
        private String m_CardNumber;
        private String m_CardName;
        private String m_CardExpirationMonth;
        private String m_CardExpirationYear;
        private String m_CardStartDate;
        private String m_CardIssueNumber;
        private String m_Last4;

        private Decimal m_MaxMindFraudScore;
        private String m_MaxMindDetails; // xml fragment

        private String m_ECheckBankABACode;
        private String m_ECheckBankAccountNumber;
        private String m_ECheckBankAccountName;
        private String m_ECheckBankAccountType;
        private String m_ECheckBankName;

        private int m_AffiliateID;
        private String m_EMail;
        private int m_SkinID;
        private String m_StoreVersion;
        private int m_LevelID;
        private String m_LevelName;
        private decimal m_LevelDiscountAmount;
        private Decimal m_LevelDiscountPercent;
        private bool m_LevelHasFreeShipping;
        private bool m_LevelAllowsQuantityDiscounts;
        private bool m_LevelHasNoTax;
        private bool m_LevelAllowsCoupons;
        private bool m_LevelDiscountsApplyToExtendedPrices;

        private string m_CaptureTXResult;
        private string m_CaptureTXCommand;
        private string m_RefundTXResult;
        private string m_RefundTXCommand;
        private string m_AuthorizationPNREF;
        private string m_AuthorizationCode;
        private string m_OrdersCCSaltField;

        private OrderTransactionCollection m_TransactionCollection;
        public OrderTransactionCollection TransactionCollection
        {
            get
            {
                if (m_TransactionCollection == null)
                    RefreshTransactionCollection();
                return m_TransactionCollection;
            }
        }

        public void RefreshTransactionCollection()
        {
            m_TransactionCollection = new OrderTransactionCollection(OrderNumber);
        }

        private string m_ReceiptHtml = string.Empty;

        public Order() { } // for serialization ONLY!

        public Order(int OrderNumber) : this(OrderNumber, Localization.GetDefaultLocale()) { }

        public Order(int OrderNumber, String ViewInLocaleSetting)
        {
            m_OrderNumber = OrderNumber;
            m_ViewInLocaleSetting = ViewInLocaleSetting;
            LoadFromDB();
        }

        public Order(String OrderNumber, String ViewInLocaleSetting)
        {
            m_OrderNumber = Localization.ParseUSInt(OrderNumber);
            m_ViewInLocaleSetting = ViewInLocaleSetting;
            LoadFromDB();
        }

        public static string UpdateOrder(int ordernumber, SqlParameter[] spa)
        {
            string err = String.Empty;
            
            SqlConnection cn = new SqlConnection(DB.GetDBConn());
            cn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = cn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "dbo.aspdnsf_updOrders";

            SqlParameter sqlparam = new SqlParameter("@OrderNUmber", SqlDbType.Int, 4);
            sqlparam.Value = ordernumber;
            cmd.Parameters.Add(sqlparam);
            foreach (SqlParameter sp in spa)
            {
                cmd.Parameters.Add(sp);
            }
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                err = ex.Message;
            }

            cn.Close();
            cmd.Dispose();
            cn.Dispose();
            return err;

        }

        public static int GetOrderStoreID(int OrderNumber)
        {
            int StoreId = 0;
            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("SELECT StoreID FROM Orders with (NOLOCK) WHERE OrderNumber ='" + OrderNumber.ToString() + "';", con))
                {
                    if (rs.Read())
                    {
                        StoreId = DB.RSFieldInt(rs, "StoreId");

                    }
                }
            }
            return StoreId;
        }

        public string UpdateOrder(SqlParameter[] spa)
        {
            return Order.UpdateOrder(m_OrderNumber, spa);
        }

        private void LoadFromDB()
        {
            m_IsEmpty = true;

            m_CartItems = new CartItemCollection();
            m_Coupon = new CouponObject();
            int i = 0;
            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("dbo.aspdnsf_getOrder " + m_OrderNumber.ToString(), con))
                {
                    while (rs.Read())
                    {
                        m_SkinID = DB.RSFieldInt(rs, "SkinID");
                        if (m_SkinID == 0)
                        {
                            m_SkinID = 1; // fix it up to something!
                        }
                        m_IsEmpty = false;

                        if (i == 0)
                        {

                            if (m_ViewInLocaleSetting == null)
                            {
                                m_ViewInLocaleSetting = DB.RSField(rs, "LocaleSetting");
                            }

                            if (m_ViewInLocaleSetting.Length == 0)
                            {
                                m_ViewInLocaleSetting = DB.RSField(rs, "LocaleSetting");
                            }

                            m_MaxMindFraudScore = DB.RSFieldDecimal(rs, "MaxMindFraudScore");
                            m_MaxMindDetails = DB.RSField(rs, "MaxMindDetails");

                            m_Total = DB.RSFieldDecimal(rs, "OrderTotal");
                            m_SubTotal = DB.RSFieldDecimal(rs, "OrderSubtotal");
                            m_TaxTotal = DB.RSFieldDecimal(rs, "OrderTax");
                            m_ShippingTotal = DB.RSFieldDecimal(rs, "OrderShippingCosts");
                            m_OrderDate = DB.RSFieldDateTime(rs, "OrderDate");
                            m_PaymentGateway = DB.RSField(rs, "PaymentGateway");
                            m_CardType = DB.RSField(rs, "CardType");
                            m_CardNumber = DB.RSField(rs, "CardNumber");
                            m_CardName = DB.RSField(rs, "CardName");
                            m_CardExpirationMonth = DB.RSField(rs, "CardExpirationMonth");
                            m_CardExpirationYear = DB.RSField(rs, "CardExpirationYear");
                            m_CardStartDate = DB.RSField(rs, "CardStartDate");
                            m_CardIssueNumber = Security.UnmungeString(DB.RSField(rs, "CardIssueNumber"));

                            m_ECheckBankAccountName = DB.RSField(rs, "ECheckBankAccountName");
                            m_ECheckBankAccountType = DB.RSField(rs, "ECheckBankAccountType");
                            m_ECheckBankName = DB.RSField(rs, "ECheckBankName");

                            string saltKey = StaticGetSaltKey(m_OrderNumber);

                            string eCheckABACode = DB.RSField(rs, "ECheckBankABACode");
                            string eCheckABACodeUnMunged = Security.UnmungeString(eCheckABACode, saltKey);

                            if (eCheckABACodeUnMunged.StartsWith(Security.ro_DecryptFailedPrefix, StringComparison.InvariantCultureIgnoreCase))
                            {
                                // Failed decryption, must be clear text
                                m_ECheckBankABACode = eCheckABACode;
                            }
                            else
                            {
                                // decryption successful, must be already encrypted
                                m_ECheckBankABACode = eCheckABACodeUnMunged;
                            }

                            string eCheckBankAccountNumber = DB.RSField(rs, "ECheckBankAccountNumber");
                            string eCheckBankAccountNumberUnMunged = Security.UnmungeString(eCheckBankAccountNumber, saltKey);

                            if (eCheckBankAccountNumberUnMunged.StartsWith(Security.ro_DecryptFailedPrefix, StringComparison.InvariantCultureIgnoreCase))
                            {
                                // Failed decryption, must be clear text
                                m_ECheckBankAccountNumber = eCheckBankAccountNumber;
                            }
                            else
                            {
                                // decryption successful, must be already encrypted
                                m_ECheckBankAccountNumber = eCheckBankAccountNumberUnMunged;
                            }


                            m_ParentOrderNumber = DB.RSFieldInt(rs, "ParentOrderNumber");
                            m_RelatedOrderNumber = DB.RSFieldInt(rs, "RelatedOrderNumber");
                            m_AffiliateID = DB.RSFieldInt(rs, "AffiliateID");
                            m_CustomerID = DB.RSFieldInt(rs, "CustomerID");
                            m_EMail = DB.RSField(rs, "EMail");
                            m_CartType = (CartTypeEnum)DB.RSFieldInt(rs, "CartType");

                            m_OrderWeight = DB.RSFieldDecimal(rs, "OrderWeight");
                            m_TransactionState = DB.RSField(rs, "TransactionState");
                            m_TransactionType = (AppLogic.TransactionTypeEnum)DB.RSFieldInt(rs, "TransactionType");
                            m_AVSResult = DB.RSField(rs, "AVSResult");

                            m_AuthorizedOn = DB.RSFieldDateTime(rs, "AuthorizedOn");
                            m_CapturedOn = DB.RSFieldDateTime(rs, "CapturedOn");
                            m_VoidedOn = DB.RSFieldDateTime(rs, "VoidedOn");
                            m_RefundedOn = DB.RSFieldDateTime(rs, "RefundedOn");
                            m_FraudedOn = DB.RSFieldDateTime(rs, "FraudedOn");
                            m_EditedOn = DB.RSFieldDateTime(rs, "EditedOn");

                            m_AlreadyConfirmed = DB.RSFieldBool(rs, "AlreadyConfirmed");

                            m_PaymentMethod = DB.RSField(rs, "PaymentMethod");
                            m_OrderNotes = DB.RSField(rs, "OrderNotes");
                            m_FinalizationData = DB.RSField(rs, "FinalizationData");
                            m_OrderOptions = DB.RSField(rs, "OrderOptions");
                            m_PONumber = DB.RSField(rs, "PONumber");
                            m_LocaleSetting = Localization.CheckLocaleSettingForProperCase(DB.RSField(rs, "LocaleSetting"));
                            m_LastIPAddress = DB.RSField(rs, "LastIPAddress");
                            m_ReceiptEMailSentOn = DB.RSFieldDateTime(rs, "ReceiptEMailSentOn");
                            m_ShippedOn = DB.RSFieldDateTime(rs, "ShippedOn");
                            m_DownloadEMailSentOn = DB.RSFieldDateTime(rs, "DownloadEMailSentOn");
                            m_DistributorEMailSentOn = DB.RSFieldDateTime(rs, "DistributorEMailSentOn");
                            m_ShippingTrackingNumber = DB.RSField(rs, "ShippingTrackingNumber");
                            m_RecurringSubscriptionID = DB.RSField(rs, "RecurringSubscriptionID");
                            m_ShippedVIA = DB.RSField(rs, "ShippedVIA");
                            m_CustomerServiceNotes = DB.RSField(rs, "CustomerServiceNotes");

                            m_Coupon.m_couponcode = DB.RSField(rs, "CouponCode");
                            m_Coupon.m_description = DB.RSField(rs, "CouponDescription");
                            m_Coupon.m_expirationdate = System.DateTime.MinValue; // DB.RSFieldDateTime(rs,"CouponExpirationDate");
                            m_Coupon.m_discountamount = DB.RSFieldDecimal(rs, "CouponDiscountAmount");
                            m_Coupon.m_discountpercent = DB.RSFieldDecimal(rs, "CouponDiscountPercent");
                            m_Coupon.m_discountincludesfreeshipping = DB.RSFieldBool(rs, "CouponIncludesFreeShipping");
                            m_Coupon.m_coupontype = (CouponTypeEnum)DB.RSFieldInt(rs, "CouponType");

                            m_ShippingAddress.m_NickName = String.Empty; // TBD DB.RSField(rs,"ShippingNickName");
                            m_ShippingAddress.m_FirstName = DB.RSField(rs, "ShippingFirstName");
                            m_ShippingAddress.m_LastName = DB.RSField(rs, "ShippingLastName");
                            m_ShippingAddress.m_Company = DB.RSField(rs, "ShippingCompany");
                            m_ShippingAddress.m_ResidenceType = (ResidenceTypes)DB.RSFieldInt(rs, "ShippingResidenceType");
                            m_ShippingAddress.m_Address1 = DB.RSField(rs, "ShippingAddress1");
                            m_ShippingAddress.m_Address2 = DB.RSField(rs, "ShippingAddress2");
                            m_ShippingAddress.m_Suite = DB.RSField(rs, "ShippingSuite");
                            m_ShippingAddress.m_City = DB.RSField(rs, "ShippingCity");
                            m_ShippingAddress.m_State = DB.RSField(rs, "ShippingState");
                            m_ShippingAddress.m_Zip = DB.RSField(rs, "ShippingZip");
                            m_ShippingAddress.m_Country = DB.RSField(rs, "ShippingCountry");
                            m_ShippingAddress.m_Phone = DB.RSField(rs, "ShippingPhone");
                            m_ShippingAddress.m_EMail = DB.RSField(rs, "EMail");

                            m_BillingAddress.m_NickName = String.Empty; // TBD DB.RSField(rs,"BillingNickName");
                            m_BillingAddress.m_FirstName = DB.RSField(rs, "BillingFirstName");
                            m_BillingAddress.m_LastName = DB.RSField(rs, "BillingLastName");
                            m_BillingAddress.m_Company = DB.RSField(rs, "BillingCompany");
                            m_BillingAddress.m_ResidenceType = ResidenceTypes.Unknown;
                            m_BillingAddress.m_Address1 = DB.RSField(rs, "BillingAddress1");
                            m_BillingAddress.m_Address2 = DB.RSField(rs, "BillingAddress2");
                            m_BillingAddress.m_Suite = DB.RSField(rs, "BillingSuite");
                            m_BillingAddress.m_City = DB.RSField(rs, "BillingCity");
                            m_BillingAddress.m_State = DB.RSField(rs, "BillingState");
                            m_BillingAddress.m_Zip = DB.RSField(rs, "BillingZip");
                            m_BillingAddress.m_Country = DB.RSField(rs, "BillingCountry");
                            m_BillingAddress.m_Phone = DB.RSField(rs, "BillingPhone");
                            m_BillingAddress.m_EMail = DB.RSField(rs, "EMail");

                            m_ShippingCalculationID = DB.RSFieldInt(rs, "ShippingCalculationID");
                            m_ShippingMethodID = DB.RSFieldInt(rs, "ShippingMethodID");
                            m_ShippingMethod = DB.RSFieldByLocale(rs, "ShippingMethod", ViewInLocaleSetting);
                            if (m_ShippingMethod.Length == 0)
                            {
                                m_ShippingMethod = Shipping.GetShippingMethodDisplayName(m_ShippingMethodID, ViewInLocaleSetting); // for old order compatibility
                            }
                            m_StoreVersion = DB.RSField(rs, "StoreVersion");
                            m_LevelID = DB.RSFieldInt(rs, "LevelID");
                            m_LevelName = DB.RSField(rs, "LevelName");
                            m_LevelDiscountAmount = DB.RSFieldDecimal(rs, "LevelDiscountAmount");
                            m_LevelDiscountPercent = DB.RSFieldDecimal(rs, "LevelDiscountPercent");
                            m_LevelHasFreeShipping = DB.RSFieldBool(rs, "LevelHasFreeShipping");
                            m_LevelAllowsQuantityDiscounts = DB.RSFieldBool(rs, "LevelAllowsQuantityDiscounts");
                            m_LevelHasNoTax = DB.RSFieldBool(rs, "LevelHasNoTax");
                            m_LevelAllowsCoupons = DB.RSFieldBool(rs, "LevelAllowsCoupons");
                            m_LevelDiscountsApplyToExtendedPrices = DB.RSFieldBool(rs, "LevelDiscountsApplyToExtendedPrices");


                            m_OrdersCCSaltField = rs[AppLogic.AppConfig("OrdersCCSaltField")].ToString();
                            m_CaptureTXResult = DB.RSField(rs, "CaptureTXResult");
                            m_CaptureTXCommand = DB.RSField(rs, "CaptureTXCommand");
                            m_RefundTXResult = DB.RSField(rs, "RefundTXResult");
                            m_RefundTXCommand = DB.RSField(rs, "RefundTXCommand");
                            m_AuthorizationPNREF = DB.RSField(rs, "AuthorizationPNREF");
                            m_AuthorizationCode = DB.RSField(rs, "AuthorizationCode");
                            m_ReceiptHtml = DB.RSField(rs, "ReceiptHtml");
                        }

                        m_IsNew = DB.RSFieldBool(rs, "IsNew");
                        CartItem newItem = new CartItem();
                        newItem.ShoppingCartRecordID = DB.RSFieldInt(rs, "ShoppingCartRecID");
                        newItem.ProductID = DB.RSFieldInt(rs, "ProductID");
                        newItem.VariantID = DB.RSFieldInt(rs, "VariantID");
                        newItem.GiftRegistryForCustomerID = DB.RSFieldInt(rs, "GiftRegistryForCustomerID");
                        newItem.ProductName = DB.RSFieldByLocale(rs, "OrderedProductName", ViewInLocaleSetting);
                        newItem.VariantName = DB.RSFieldByLocale(rs, "OrderedProductVariantName", ViewInLocaleSetting);
                        newItem.SKU = DB.RSField(rs, "OrderedProductSKU");
                        newItem.Quantity = DB.RSFieldInt(rs, "Quantity");

                        newItem.ShippingMethodID = DB.RSFieldInt(rs, "ShippingMethodID");
                        newItem.ShippingMethod = DB.RSFieldByLocale(rs, "ShippingMethod", ViewInLocaleSetting);
                        if (newItem.ShippingMethod.Length == 0)
                        {
							newItem.ShippingMethod = Shipping.GetShippingMethodDisplayName(newItem.ShippingMethodID, ViewInLocaleSetting); // for old order compatibility
                        }

                        newItem.ChosenColor = DB.RSFieldByLocale(rs, "ChosenColor", ViewInLocaleSetting);
                        newItem.ChosenColorSKUModifier = DB.RSField(rs, "ChosenColorSKUModifier");
                        newItem.ChosenSize = DB.RSFieldByLocale(rs, "ChosenSize", ViewInLocaleSetting);
                        newItem.ChosenSizeSKUModifier = DB.RSField(rs, "ChosenSizeSKUModifier");
                        newItem.TextOption = DB.RSField(rs, "TextOption");
                        newItem.SizeOptionPrompt = DB.RSFieldByLocale(rs, "SizeOptionPrompt", ViewInLocaleSetting);
                        newItem.ColorOptionPrompt = DB.RSFieldByLocale(rs, "ColorOptionPrompt", ViewInLocaleSetting);
                        newItem.TextOptionPrompt = DB.RSFieldByLocale(rs, "TextOptionPrompt", ViewInLocaleSetting);
                        newItem.CustomerEntersPricePrompt = DB.RSFieldByLocale(rs, "CustomerEntersPricePrompt", ViewInLocaleSetting);
                        if (newItem.SizeOptionPrompt.Length == 0)
                        {
                            newItem.SizeOptionPrompt = AppLogic.GetString("AppConfig.SizeOptionPrompt", m_SkinID, ViewInLocaleSetting);
                        }
                        if (newItem.ColorOptionPrompt.Length == 0)
                        {
                            newItem.ColorOptionPrompt = AppLogic.GetString("AppConfig.ColorOptionPrompt", m_SkinID, ViewInLocaleSetting);
                        }
                        if (newItem.TextOptionPrompt.Length == 0)
                        {
                            newItem.TextOptionPrompt = AppLogic.GetString("shoppingcart.cs.25", m_SkinID, ViewInLocaleSetting);
                        }
                        if (newItem.CustomerEntersPricePrompt.Length == 0)
                        {
                            newItem.CustomerEntersPricePrompt = AppLogic.GetString("AppConfig.CustomerEntersPricePrompt", m_SkinID, ViewInLocaleSetting);
                        }
                        newItem.ManufacturerPartNumber = DB.RSField(rs, "OrderedProductManufacturerPartNumber");
                        newItem.Weight = DB.RSFieldDecimal(rs, "OrderedProductWeight");
                        newItem.SubscriptionInterval = DB.RSFieldInt(rs, "SubscriptionInterval");
                        newItem.SubscriptionIntervalType = (DateIntervalTypeEnum)DB.RSFieldInt(rs, "SubscriptionIntervalType");
                        newItem.Price = DB.RSFieldDecimal(rs, "OrderedProductPrice");
                        newItem.CustomerEntersPrice = DB.RSFieldBool(rs, "CustomerEntersPrice");
                        newItem.QuantityDiscountID = DB.RSFieldInt(rs, "OrderedProductQuantityDiscountID");
                        newItem.QuantityDiscountName = DB.RSFieldByLocale(rs, "OrderedProductQuantityDiscountName", ViewInLocaleSetting);
                        newItem.QuantityDiscountPercent = DB.RSFieldDecimal(rs, "OrderedProductQuantityDiscountPercent");
                        newItem.IsTaxable = DB.RSFieldBool(rs, "IsTaxable");
                        newItem.TaxClassID = DB.RSFieldInt(rs, "TaxClassID");
                        newItem.TaxRate = DB.RSFieldDecimal(rs, "TaxRate");
                        newItem.IsShipSeparately = DB.RSFieldBool(rs, "IsShipSeparately");
                        newItem.IsDownload = DB.RSFieldBool(rs, "IsDownload");
                        newItem.DownloadLocation = DB.RSField(rs, "DownloadLocation");

                        newItem.FreeShipping = DB.RSFieldTinyInt(rs, "FreeShipping") == 1;
                        newItem.Shippable = DB.RSFieldTinyInt(rs, "FreeShipping") != 2;

                        newItem.DistributorID = DB.RSFieldInt(rs, "DistributorID");
                        newItem.Notes = DB.RSField(rs, "Notes");
                        newItem.OrderShippingDetail = DB.RSField(rs, "ShippingDetail");
                        newItem.IsAKit = DB.RSFieldBool(rs, "IsAKit");
                        newItem.ShippingMethodID = DB.RSFieldInt(rs, "CartItemShippingMethodID");
                        newItem.ShippingMethod = DB.RSField(rs, "CartItemShippingMethod");

                        string detailXml = DB.RSField(rs, "ShippingDetail");
                        newItem.ShippingAddressID = DB.RSFieldInt(rs, "ShippingAddressID");
                        if (detailXml.Length != 0)
                        {
                            newItem.ShippingDetail = new Address();
                            newItem.ShippingDetail.AsXml = detailXml;
                        }
                        m_CartItems.Add(newItem);

                        i = i + 1;
                    }
                  
                    if (m_ViewInLocaleSetting == null || m_ViewInLocaleSetting.Length == 0)
                    {
                        m_ViewInLocaleSetting = m_LocaleSetting;
                    }
                }
            }

            // get child order #'s, if any:
            m_ChildOrderNumbers = String.Empty;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS(String.Format("select OrderNumber from orders with (NOLOCK) where ParentOrderNumber={0}", OrderNumber.ToString()), con))
                {
                    while (rs.Read())
                    {
                        if (m_ChildOrderNumbers.Length != 0)
                        {
                            m_ChildOrderNumbers += ",";
                        }
                        m_ChildOrderNumbers += DB.RSFieldInt(rs, "OrderNumber").ToString();
                    }
                }
            }
        }

        public CouponObject GetCoupon()
        {
            return m_Coupon;
        }

        public Decimal SubTotal(bool includeDiscount)
        {
            // NOTE: when the order record was created the total fields already reflect all discounts, quantities, coupons, and levels:
            return m_SubTotal;
        }

        public Decimal TaxTotal(bool includeDiscount)
        {
            // NOTE: when the order record was created the total fields already reflect all discounts, quantities, coupons, and levels:
            return m_TaxTotal;
        }

        public Decimal ShippingTotal(bool includeDiscount)
        {
            // NOTE: when the order record was created the total fields already reflect all discounts, quantities, coupons, and levels:
            return m_ShippingTotal;
        }

        public Decimal Total(bool includeDiscount)
        {
            return m_Total;
        }

        public bool IsAKit(int ProductID, int ShoppingCartRecID) // remember, we CANNOT use anything except from order tables in the order object!
        {
            return DB.GetSqlN("select count(*) as N from orders_kitcart  with (NOLOCK)  where OrderNumber=" + OrderNumber.ToString() + " and ShoppingCartRecID=" + ShoppingCartRecID.ToString() + " and CustomerID=" + m_CustomerID.ToString() + " and ProductID=" + ProductID.ToString()) > 0;
        }

        public String GetLineItemDescription(CartItem c)
        {
            StringBuilder tmpS = new StringBuilder(4096);

            tmpS.Append("<b>");
            tmpS.Append(AppLogic.MakeProperObjectName(c.ProductName, c.VariantName, ViewInLocaleSetting));
            tmpS.Append("</b>");

            tmpS.Append("");
            tmpS.Append(AppLogic.GetString("showproduct.aspx.21", SkinID, ViewInLocaleSetting) + " " + c.SKU); // this sku is already the fully composed sku

            if (c.GiftRegistryForCustomerID != 0)
            {
                tmpS.Append("");
                tmpS.Append(String.Format(AppLogic.GetString("shoppingcart.cs.92", SkinID, ViewInLocaleSetting), AppLogic.GiftRegistryDisplayName(c.GiftRegistryForCustomerID, false, SkinID, ViewInLocaleSetting)));
            }

            if (c.ChosenSize.Length != 0)
            {
                tmpS.Append("");
                tmpS.Append(c.SizeOptionPrompt);
                tmpS.Append(":&nbsp;");
                tmpS.Append(c.ChosenSize);
            }

            if (c.ChosenColor.Length != 0)
            {
                tmpS.Append("");
                tmpS.Append(c.ColorOptionPrompt);
                tmpS.Append(":&nbsp;");
                tmpS.Append(c.ChosenColor);
            }

            if (c.TextOption.Length != 0)
            {
                if (c.TextOption.IndexOf("\n") != -1)
                {
                    tmpS.Append("");
                    tmpS.Append(c.TextOptionPrompt);
                    tmpS.Append(":");
                    tmpS.Append(XmlCommon.GetLocaleEntry(c.TextOption, ViewInLocaleSetting, true).Replace("\n", ""));
                }
                else
                {
                    tmpS.Append("");
                    tmpS.Append(c.TextOptionPrompt);
                    tmpS.Append(":&nbsp;");
                    tmpS.Append(XmlCommon.GetLocaleEntry(c.TextOption, ViewInLocaleSetting, true));
                }
            }

            if (c.IsDownload && !c.IsSystem)
            {
                tmpS.Append("");
                tmpS.Append(AppLogic.GetString("shoppingcart.cs.84", m_SkinID, ViewInLocaleSetting));
            }
            if (!c.IsDownload && c.FreeShipping && !c.IsSystem)
            {
                tmpS.Append("");
                tmpS.Append(AppLogic.GetString("shoppingcart.cs.104", m_SkinID, ViewInLocaleSetting));
            }

            bool IsAKit = AppLogic.IsAKit(c.ProductID);
            if (IsAKit)
            {
                StringBuilder tmp = new StringBuilder(4096);
                bool first = true;

                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rsx = DB.GetRS("select * from orders_kitcart where ShoppingCartRecID=" + c.ShoppingCartRecordID.ToString() + " and OrderNumber=" + m_OrderNumber.ToString() + " and CustomerID=" + m_CustomerID.ToString(), con))
                    {
                        while (rsx.Read())
                        {
                            if (!first)
                            {
                                tmp.Append("");
                            }
                            tmp.Append("&nbsp;&nbsp;-&nbsp;");
                            if (!AppLogic.AppConfigBool("HideKitQuantity") || DB.RSFieldInt(rsx, "Quantity") > 1)
                            {
                                tmp.Append("(");
                                tmp.Append(DB.RSFieldInt(rsx, "Quantity").ToString());
                                tmp.Append(") ");
                            }
                            tmp.Append(DB.RSFieldByLocale(rsx, "KitItemName", ViewInLocaleSetting));
                            if (DB.RSField(rsx, "TextOption").Length > 0)
                            {
                                tmpS.Append("&nbsp;");
                                if (DB.RSField(rsx, "TextOption").StartsWith("images/orders/image/", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    tmp.Append(": <a target=\"_blank\" href=\"" + DB.RSField(rsx, "TextOption") + "\">" + AppLogic.GetString("shoppingcart.cs.1000", m_SkinID, ViewInLocaleSetting) + "</a>");
                                }
                                else
                                {
                                    tmp.Append(": " + DB.RSField(rsx, "TextOption"));
                                }
                            }
                            first = false;
                        }
                    }
                }

                tmpS.Append("<div style=\"margin-left: 10px;\">");
                tmpS.Append(tmp.ToString());
                tmpS.Append("</div>");
            }

            if (this.HasMultipleShippingAddresses() && !c.IsDownload && !c.IsSystem)
            {
                if (!IsAKit)
                {
                    tmpS.Append("");
                }
                tmpS.Append(AppLogic.GetString("shoppingcart.cs.87", m_SkinID, ViewInLocaleSetting));
                tmpS.Append(" ");
                if (c.GiftRegistryForCustomerID != 0 && !Customer.OwnsThisAddress(m_CustomerID, c.ShippingAddressID))
                {
                    tmpS.Append(AppLogic.GetString("checkoutshippingmult.aspx.15", m_SkinID, ViewInLocaleSetting));
                }
                else
                {
                    tmpS.Append("<div style=\"margin-left: 10px;\">");
                    tmpS.Append(c.ShippingDetail);
                    tmpS.Append("</div>");
                    tmpS.Append("<div>");
                    tmpS.Append("<b>");
                    tmpS.Append(AppLogic.GetString("order.cs.68", m_SkinID, ViewInLocaleSetting));
                    tmpS.Append("</b> ");
                    tmpS.Append(c.ShippingMethod);
                    tmpS.Append("</div>");
                }
            }

            if (AppLogic.AppConfigBool("AllowShoppingCartItemNotes") && !c.IsSystem)
            {
                if (c.Notes.Length != 0)
                {
                    tmpS.Append("");
                    tmpS.Append(AppLogic.GetString("shoppingcart.cs.86", m_SkinID, ViewInLocaleSetting));
                    tmpS.Append("");
                    tmpS.Append(HttpContext.Current.Server.HtmlEncode(c.Notes));
                }
            }
            return tmpS.ToString();
        }

        public String GetDistributorNotificationPackageToUse(int ForDistributorID)
        {
            String PackageName = String.Empty;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select NotificationXmlPackage from Distributor  with (NOLOCK)  where DistributorID=" + ForDistributorID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        PackageName = DB.RSField(rs, "NotificationXmlPackage");
                    }
                }
            }

            if (PackageName.Length == 0)
            {
                PackageName = AppLogic.AppConfig("XmlPackage.DefaultDistributorNotification");
            }
            if (PackageName.Length == 0)
            {
                PackageName = "notification.distributor.xml.config";
            }
            return PackageName;
        }

        public String DistributorNotification(int ForDistributorID)
        {            
            String PackageName = GetDistributorNotificationPackageToUse(ForDistributorID);                
            return AppLogic.RunXmlPackage(PackageName, null, null, AppLogic.GetStoreSkinID(GetOrderStoreID(OrderNumber)), string.Empty, "ordernumber=" + OrderNumber.ToString() + "&distributorid=" + ForDistributorID.ToString(), false, false);
        }

        public String ShippedNotification()
        {
            String PackageName = AppLogic.AppConfig("XmlPackage.OrderShipped");
            if (PackageName.Length != 0)
            {
                return AppLogic.RunXmlPackage(PackageName, null, null, AppLogic.GetStoreSkinID(GetOrderStoreID(OrderNumber)), string.Empty, "ordernumber=" + OrderNumber.ToString(), false, false);
            }
            return string.Empty;
        }

        public String AdminNotification()
        {
            String PackageName = AppLogic.AppConfig("XmlPackage.NewOrderAdminNotification");
            if (PackageName.Length != 0)
            {
                return AppLogic.RunXmlPackage(PackageName, null, null, AppLogic.GetStoreSkinID(GetOrderStoreID(OrderNumber)), string.Empty, "ordernumber=" + OrderNumber.ToString(), false, false);
            }
            return string.Empty;
        }

        // returns true if this order has any items which are download items:
        public bool HasDownloadComponents(bool RequireDownloadLocationAlso)
        {
            //v3_9 Check the packs as well
            if (AppLogic.OrderHasDownloadComponents(m_OrderNumber, RequireDownloadLocationAlso))
            {
                return true;
            }
            foreach (CartItem c in m_CartItems)
            {
                if (c.IsDownload)
                {
                    if (RequireDownloadLocationAlso)
                    {
                        if (c.DownloadLocation.Length != 0)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
                // this item is not a download item, or is download but doesn't have a downloadlocation, so move to next item
            }
            return false;
        }


        // returns true if this order has any items which are download items:
        public bool HasFreeShippingComponents()
        {
            foreach (CartItem c in m_CartItems)
            {
                if (c.FreeShipping)
                {
                    return true;
                }
            }
            return false;
        }

        // returns true if this order has any items which are download items:
        public bool HasRecurringComponents()
        {
            return m_CartItems.ContainsRecurring;
            //foreach (CartItem c in m_CartItems)
            //{
            //    if (c.IsRecurring)
            //    {
            //        return true;
            //    }
            //}
            //return false;
        }

        // returns true if this order has any items which are drop ship items:
        public bool HasDistributorComponents()
        {
            foreach (CartItem c in m_CartItems)
            {
                if (c.DistributorID != 0)
                {
                    return true;
                }
            }
            return false;
        }

        // returns true if this order has ONLY download items:
        public bool IsAllDownloadComponents()
        {
            foreach (CartItem c in m_CartItems)
            {
                if (!c.IsDownload)
                {
                    return false;
                }
            }
            return true;
        }

        // returns true if this order has ONLY free shipping items:
        public bool IsAllFreeShippingComponents()
        {
            foreach (CartItem c in m_CartItems)
            {
                if (!c.FreeShipping)
                {
                    return false;
                }
            }
            return true;
        }

        // returns true if this order has ONLY system items:
        public bool IsAllSystemComponents()
        {
            foreach (CartItem c in m_CartItems)
            {
                if (!c.FreeShipping)
                {
                    return false;
                }
            }
            return true;
        }

        public bool isAllDistributorComponents()
        {
            foreach (CartItem c in m_CartItems)
            {
                if (c.DistributorID == 0)
                {
                    return false;
                }
            }
            return true;
        }
        // returns true if this order has any download items that have download locations:
        public bool ThereAreDownloadFilesSpecified()
        {
            foreach (CartItem c in m_CartItems)
            {
                if (c.IsDownload && c.DownloadLocation.Trim().Length != 0)
                {
                    return true;
                }
            }
            return false;
        }

        public bool HasMultipleShippingAddresses()
        {
            return m_CartItems.HasMultipleShippingAddresses;
        }

        public void FinalizeGiftRegistryComponents()
        {
            if (AppLogic.AppConfigBool("DecrementGiftRegistryOnOrder") && HasGiftRegistryComponents())
            {
                // decrement gift registry owner items based on this purchase:
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS("Select * from orders_shoppingcart  with (NOLOCK)  where OrderNumber=" + this.OrderNumber.ToString() + " and GiftRegistryForCustomerID<>0", con))
                    {
                        while (rs.Read())
                        {
                            int GiftRegistryForCustomerID = DB.RSFieldInt(rs, "GiftRegistryForCustomerID");
                            int ProductID = DB.RSFieldInt(rs, "ProductID");
                            int VariantID = DB.RSFieldInt(rs, "VariantID");
                            int Q = DB.RSFieldInt(rs, "Quantity");
                            String ChosenColor = DB.RSField(rs, "ChosenColor");
                            String ChosenSize = DB.RSField(rs, "ChosenSize");
                            String TextOption = DB.RSField(rs, "TextOption");
                            int OwnersShoppingCartRecID = 0;
                            String sqlx = String.Format("select ShoppingCartRecID from shoppingcart where CartType={0} and ProductID={1} and VariantID={2} and ChosenColor like {3} and ChosenSize like {4} and TextOption like {5} and CustomerID={6}", ((int)CartTypeEnum.GiftRegistryCart).ToString(), ProductID.ToString(), VariantID.ToString(), DB.SQuote("%" + ChosenColor + "%"), DB.SQuote("%" + ChosenSize + "%"), DB.SQuote("%" + TextOption + "%"), GiftRegistryForCustomerID.ToString());

                            using (SqlConnection gfCon = new SqlConnection(DB.GetDBConn()))
                            {
                                gfCon.Open();
                                using (IDataReader gfRs = DB.GetRS(sqlx, gfCon))
                                {
                                    if (gfRs.Read())
                                    {
                                        OwnersShoppingCartRecID = DB.RSFieldInt(gfRs, "ShoppingCartRecID");
                                    }
                                }
                            }

                            if (GiftRegistryForCustomerID != 0 && OwnersShoppingCartRecID != 0)
                            {
                                String UpdateQSQL = String.Format("update ShoppingCart set Quantity=Quantity-{0} where ShoppingCartRecID={1}", Q.ToString(), OwnersShoppingCartRecID.ToString());
                                DB.ExecuteSQL(UpdateQSQL);
                            }
                            DB.ExecuteSQL(String.Format("delete from ShoppingCart where Quantity<=0 and CartType={0} and CustomerID={1}", ((int)CartTypeEnum.GiftRegistryCart), GiftRegistryForCustomerID.ToString()));
                        }
                    }
                }
            }
        }

        // returns true if this cart has any items that were purchased for another person's Gift Registry:
        public bool HasGiftRegistryComponents()
        {
            return m_CartItems.HasGiftRegistryComponents;
        }

        public int NumAtThisShippingAddress(int ShippingAddressID)
        {
            if (ShippingAddressID == 0)
            {
                return 1;
            }
            int i = 0;
            foreach (CartItem c in m_CartItems)
            {
                if (c.ShippingAddressID == ShippingAddressID)
                {
                    i++;
                }
            }
            return i;
        }


        private void SaveReceipt(String receiptText)
        {
            DB.ExecuteSQL("update Orders set ReceiptHtml=" + DB.SQuote(receiptText) + " where OrderNumber=" + m_OrderNumber.ToString());
        }

        public String Receipt(Customer ViewingCustomer, bool ShowOnlineLink)
        {
            return Receipt(ViewingCustomer, ShowOnlineLink, false);
        }

        /// <summary>
        /// Generate order receipt report.
        /// </summary>
        /// <param name="ViewingCustomer">Current customer info who is login.</param>
        /// <param name="ShowOnlineLink"> If receipt is sent through email value should be 'true' to show online link.</param>
        /// <param name="disallowCacheing">Forces the receipt to be regenerated rather than pulling from the database.</param>
        /// <returns>Generated receipt report.</returns>
        public String Receipt(Customer ViewingCustomer, bool ShowOnlineLink, bool disallowCacheing)
        {
            if (!CustomerCanViewRecipt(ViewingCustomer))
                return string.Empty;

            if (disallowCacheing || ReceiptEMailSentOn == DateTime.MinValue || String.IsNullOrEmpty(m_ReceiptHtml))
            {
                var shipmentChargesPaid = 0m;
                var customerLevelID = GetOrderCustomerLevelID(OrderNumber);
                var lblSOFFundsTotal = String.Empty;
                var lblDirectMailFundsTotal = String.Empty;
                var lblDisplayFundsTotal = String.Empty;
                var lblLiteratureFundsTotal = String.Empty;
                var lblPOPFundsTotal = String.Empty;
                var lblBluBucks = String.Empty;
                decimal bluBucksUsed = 0m;

                var lstFund = Enumerable.Repeat(0m, 7).ToList();
                using (var conn = DB.dbConn())
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("aspdnsf_GetOrderDetail", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ORDERNUMBER", OrderNumber);
                        IDataReader reader = cmd.ExecuteReader();

                        if (reader.Read())
                        {
                            shipmentChargesPaid = Convert.ToDecimal(reader["ShipmentChargesPaid"].ToString());
                            for (var i = 2; i < 7; i++)
                            {
                                if (Convert.ToDecimal(reader[i.ToString()].ToString()) != 0)
                                {
                                    lstFund[i] =
                                        lstFund[i] + Convert.ToDecimal(reader[i.ToString()].ToString());

                                    if (lstFund[i] != 0 && i == (int)FundType.SOFFunds)
                                    {
                                        lblSOFFundsTotal = string.Format(CultureInfo.GetCultureInfo(m_ViewInLocaleSetting), AppLogic.AppConfig("CurrencyFormat"), lstFund[i]);
                                        if (shipmentChargesPaid > 0)
                                        {
                                            lblSOFFundsTotal = string.Format(CultureInfo.GetCultureInfo(m_ViewInLocaleSetting), AppLogic.AppConfig("CurrencyFormat"), lstFund[i] + shipmentChargesPaid);
                                        }
                                    }
                                    else if (lstFund[i] != 0 && i == (int)FundType.DirectMailFunds)
                                    {
                                        lblDirectMailFundsTotal = string.Format(CultureInfo.GetCultureInfo(m_ViewInLocaleSetting), AppLogic.AppConfig("CurrencyFormat"), lstFund[i]);
                                    }
                                    else if (lstFund[i] != 0 && i == (int)FundType.DisplayFunds)
                                    {
                                        lblDisplayFundsTotal = string.Format(CultureInfo.GetCultureInfo(m_ViewInLocaleSetting), AppLogic.AppConfig("CurrencyFormat"), lstFund[i]);
                                    }
                                    else if (lstFund[i] != 0 && i == (int)FundType.LiteratureFunds)
                                    {
                                        lblLiteratureFundsTotal = string.Format(CultureInfo.GetCultureInfo(m_ViewInLocaleSetting), AppLogic.AppConfig("CurrencyFormat"), lstFund[i]);
                                    }
                                    else if (lstFund[i] != 0 && i == (int)FundType.POPFunds)
                                    {
                                        lblPOPFundsTotal = string.Format(CultureInfo.GetCultureInfo(m_ViewInLocaleSetting), AppLogic.AppConfig("CurrencyFormat"), lstFund[i]);
                                    }
                                }
                            }
                        }
                    }
                }
                //get the Blu Bucks used for a perticular order number
                using (var conn = DB.dbConn())
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("select SUM(BluBucksUsed) from Orders_ShoppingCart where OrderNumber=" + OrderNumber, conn))
                    {
                        bluBucksUsed = Convert.ToDecimal(cmd.ExecuteScalar());
                        if (shipmentChargesPaid > 0 && bluBucksUsed > 0)
                        {
                            bluBucksUsed = bluBucksUsed + shipmentChargesPaid;
                        }
                    }
                }
                if (bluBucksUsed <= 0 && string.IsNullOrEmpty(lblSOFFundsTotal))
                {
                    if ((customerLevelID == (int)UserType.SALESREPS || customerLevelID == (int)UserType.INTERNAL) &&
                        shipmentChargesPaid > 0)
                    {
                        lblSOFFundsTotal = string.Format(CultureInfo.GetCultureInfo(m_ViewInLocaleSetting),
                            AppLogic.AppConfig("CurrencyFormat"), shipmentChargesPaid);
                    }

                    if ((customerLevelID == (int)UserType.BLUELITE || customerLevelID == (int)UserType.BLUPREMIER ||
                     customerLevelID == (int)UserType.BLUAUTHORIZED || customerLevelID == (int)UserType.BLUUNLIMITED) &&
                        shipmentChargesPaid > 0)
                    {
                        bluBucksUsed = shipmentChargesPaid;
                    }
                }
                lblBluBucks = Math.Round(bluBucksUsed, 2).ToString();
                var invoiceFee = Localization.CurrencyStringForDisplayWithExchangeRate(Convert.ToDecimal(AppLogic.AppConfig("Invoice.fee")), ViewingCustomer.CurrencySetting);

                var p = new XmlPackage2(AppLogic.AppConfig("XmlPackage.OrderReceipt"), null, SkinID, String.Empty, "ShowOnlineLink" + ShowOnlineLink + "&shipmentChargesPaid=" + shipmentChargesPaid + "&ordernumber=" + OrderNumber + "&BBCredit=" + lblBluBucks + "&SOFunds=" + lblSOFFundsTotal + "&DirectMailFunds=" + lblDirectMailFundsTotal + "&DisplayFunds=" + lblDisplayFundsTotal + "&LiteratureFunds=" + lblLiteratureFundsTotal + "&POPFunds=" + lblPOPFundsTotal + "&Invoicefee=" + invoiceFee);
                var result = p.TransformString();
                if (!disallowCacheing && !result.Contains(new Topic("InvalidRequest", ViewingCustomer.LocaleSetting, 1).Contents))
                    SaveReceipt(result);
                return result;
            }
            return m_ReceiptHtml;
        }

        private Boolean CustomerCanViewRecipt(Customer ViewingCustomer)
        {
            return ViewingCustomer.CustomerID == this.CustomerID || ViewingCustomer.IsAdminUser;
        }

        public String ToXml(Customer ViewingCustomer)
        {
            String PackageName = AppLogic.AppConfig("XmlPackage.OrderAsXml");
            String result = AppLogic.RunXmlPackage(PackageName, null, ViewingCustomer, SkinID, String.Empty, "ordernumber=" + OrderNumber.ToString(), false, true);
            return result;
        }

        // used only for recurring items shipment table population, NOT for receipts, distributor notifications, etc...
        public String GetPackingList(String FieldSeparator, String LineBreak)
        {
            StringBuilder tmpS = new StringBuilder(4096);
            bool first = true;
            foreach (CartItem c in m_CartItems)
            {
                if (!first)
                {
                    tmpS.Append(LineBreak);
                }
                tmpS.Append(XmlCommon.GetLocaleEntry(c.ProductName, ViewInLocaleSetting, true));
                if (c.TextOption.Length != 0)
                {
                    if (c.TextOption.IndexOf("\n") != -1)
                    {
                        tmpS.Append("");
                        tmpS.Append(AppLogic.GetString("shoppingcart.cs.25", m_SkinID, ViewInLocaleSetting));
                        tmpS.Append("");
                        tmpS.Append(XmlCommon.GetLocaleEntry(c.TextOption, ViewInLocaleSetting, true).Replace("\n", ""));
                    }
                    else
                    {
                        tmpS.Append(" (" + AppLogic.GetString("shoppingcart.cs.25", m_SkinID, ViewInLocaleSetting) + " " + XmlCommon.GetLocaleEntry(c.TextOption, ViewInLocaleSetting, true) + ") ");
                    }
                }

                if (this.IsAKit(c.ProductID,c.ShoppingCartRecordID))
                {
                    tmpS.Append(":");

                    using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                    {
                        con.Open();
                        using (IDataReader rscust = DB.GetRS("select * from orders_kitcart where ShoppingCartRecID=" + c.ShoppingCartRecordID.ToString() + " and OrderNumber=" + m_OrderNumber.ToString() + " and CustomerID=" + m_CustomerID.ToString(), con))
                        {
                            while (rscust.Read())
                            {
                                tmpS.Append("");
                                tmpS.Append("<small>");
                                tmpS.Append("&nbsp;&nbsp;-&nbsp;(" + DB.RSFieldInt(rscust, "Quantity").ToString() + ")&nbsp;");
                                tmpS.Append(DB.RSField(rscust, "KitItemName") + ", ");
                                tmpS.Append("</small>");
                            }
                        }
                    }
                }
                

                tmpS.Append(FieldSeparator);
                tmpS.Append(c.SKU + FieldSeparator);
                tmpS.Append((CommonLogic.IIF(XmlCommon.GetLocaleEntry(c.ChosenColor, ViewInLocaleSetting, true).Length == 0, "--", XmlCommon.GetLocaleEntry(c.ChosenColor , ViewInLocaleSetting, true))) + FieldSeparator);
                tmpS.Append((CommonLogic.IIF(XmlCommon.GetLocaleEntry(c.ChosenSize, ViewInLocaleSetting, true).Length == 0, "--", XmlCommon.GetLocaleEntry(c.ChosenSize, ViewInLocaleSetting, true))) + FieldSeparator);
                tmpS.Append(c.Quantity);
                first = false;
            }
            return tmpS.ToString();
        }

        public int SubscriptionTotalDays()
        {
            int totalDays = 0;
            foreach (CartItem c in CartItems)
            {
                int SubscriptionInterval = 0;
                DateIntervalTypeEnum SubscriptionIntervalType = DateIntervalTypeEnum.Monthly;
                AppLogic.GetSubscriptionInterval(c.VariantID, out SubscriptionInterval, out SubscriptionIntervalType);
                if (SubscriptionInterval != 0)
                {
                    SubscriptionInterval = SubscriptionInterval * c.Quantity;
                    switch (SubscriptionIntervalType)
                    {
                        case DateIntervalTypeEnum.Day:
                            totalDays += SubscriptionInterval;
                            break;
                        case DateIntervalTypeEnum.Week:
                            totalDays += (SubscriptionInterval * 7);
                            break;
                        case DateIntervalTypeEnum.Month:
                            totalDays += (System.DateTime.Now.AddMonths(SubscriptionInterval) - System.DateTime.Now).Days;
                            break;
                        case DateIntervalTypeEnum.Year:
                            totalDays += (System.DateTime.Now.AddYears(SubscriptionInterval) - System.DateTime.Now).Days;
                            break;
                        case DateIntervalTypeEnum.Weekly:
                            totalDays += 7;
                            break;
                        case DateIntervalTypeEnum.BiWeekly:
                            totalDays += 14;
                            break;
                        case DateIntervalTypeEnum.EveryFourWeeks:
                            totalDays += 28;
                            break;
                        case DateIntervalTypeEnum.Monthly:
                            totalDays += (System.DateTime.Now.AddMonths(1) - System.DateTime.Now).Days;
                            break;
                        case DateIntervalTypeEnum.Quarterly:
                            totalDays += (System.DateTime.Now.AddMonths(3) - System.DateTime.Now).Days;
                            break;
                        case DateIntervalTypeEnum.SemiYearly:
                            totalDays += (System.DateTime.Now.AddMonths(6) - System.DateTime.Now).Days;
                            break;
                        case DateIntervalTypeEnum.Yearly:
                            totalDays += (System.DateTime.Now.AddYears(1) - System.DateTime.Now).Days;
                            break;
                        default:
                            totalDays += (System.DateTime.Now.AddMonths(SubscriptionInterval) - System.DateTime.Now).Days;
                            break;
                    }
                }
            }
            return totalDays;
        }

        public int CustomerID
        {
            get
            {
                return m_CustomerID;
            }
        }

        public int ShippingMethodID
        {
            get
            {
                return m_ShippingMethodID;
            }
        }

        public Decimal OrderWeight
        {
            get
            {
                return m_OrderWeight;
            }
        }

        public String LocaleSetting
        {
            get
            {
                return m_LocaleSetting;
            }
        }

        public String LastIPAddress
        {
            get
            {
                return m_LastIPAddress;
            }
        }

        public String ViewInLocaleSetting
        {
            get
            {
                if (m_ViewInLocaleSetting == null || m_ViewInLocaleSetting.Length == 0)
                {
                    m_ViewInLocaleSetting = m_LocaleSetting;
                }
                return m_ViewInLocaleSetting;
            }
        }

        public int OrderNumber
        {
            get
            {
                return m_OrderNumber;
            }
        }

        public int ParentOrderNumber
        {
            get
            {
                return m_ParentOrderNumber;
            }
        }

        public int RelatedOrderNumber
        {
            get
            {
                return m_RelatedOrderNumber;
            }
            set
            {
                m_RelatedOrderNumber = value;
                DB.ExecuteSQL("update Orders set RelatedOrderNumber=" + m_RelatedOrderNumber.ToString() + " where OrderNumber=" + m_OrderNumber.ToString());
            }
        }

        public int SkinID
        {
            get
            {
                return m_SkinID;
            }
        }

        public String PaymentMethod
        {
            get
            {
                return m_PaymentMethod;
            }
        }

        public String EMail
        {
            get
            {
                return m_EMail;
            }
        }

        public String PaymentGateway
        {
            get
            {
                return m_PaymentGateway;
            }
        }

        public Decimal MaxMindFraudScore
        {
            get
            {
                return m_MaxMindFraudScore;
            }
        }

        public String MaxMindDetails // XmlFragment
        {
            get
            {
                return m_MaxMindDetails;
            }
        }

        public CartItemCollection CartItems
        {
            get
            {
                return m_CartItems;
            }
            set
            {
                m_CartItems = value;
            }
        }

        public String OrderOptions
        {
            get
            {
                return m_OrderOptions;
            }
        }

        public DateTime ReceiptEMailSentOn
        {
            get
            {
                return m_ReceiptEMailSentOn;
            }
        }

        public DateTime DownloadEMailSentOn
        {
            get
            {
                return m_DownloadEMailSentOn;
            }
        }

        public DateTime ShippedOn
        {
            get
            {
                return m_ShippedOn;
            }
        }

        public DateTime DistributorEMailSentOn
        {
            get
            {
                return m_DistributorEMailSentOn;
            }
        }

        public DateTime AuthorizedOn
        {
            get
            {
                return m_AuthorizedOn;
            }
        }

        public DateTime CapturedOn
        {
            get
            {
                return m_CapturedOn;
            }
        }

        public DateTime VoidedOn
        {
            get
            {
                return m_VoidedOn;
            }
        }

        public DateTime RefundedOn
        {
            get
            {
                return m_RefundedOn;
            }
        }

        public DateTime FraudedOn
        {
            get
            {
                return m_FraudedOn;
            }
        }

        public DateTime EditedOn
        {
            get
            {
                return m_EditedOn;
            }
            set
            {
                m_EditedOn = value;
                DB.ExecuteSQL("update Orders set EditedOn=" + DB.SQuote(Localization.DateStringForDB(m_EditedOn)) + " where OrderNumber=" + m_OrderNumber.ToString());
            }
        }

        public AddressInfo ShippingAddress
        {
            get
            {
                return m_ShippingAddress;
            }
        }

        public string CustomerServiceNotes
        {
            get { return m_CustomerServiceNotes; }
        }

        public string FinalizationData
        {
            get { return m_FinalizationData; }
        }

        public bool IsNew
        {
            get { return m_IsNew; }
        }

        public bool LevelAllowsCoupons
        {
            get { return m_LevelAllowsCoupons; }
        }

        public bool LevelAllowsQuantityDiscounts
        {
            get { return m_LevelAllowsQuantityDiscounts; }
        }

        public decimal LevelDiscountAmount
        {
            get { return m_LevelDiscountAmount; }
        }

        public decimal LevelDiscountPercent
        {
            get { return m_LevelDiscountPercent; }
        }

        public bool LevelDiscountsApplyToExtendedPrices
        {
            get { return m_LevelDiscountsApplyToExtendedPrices; }
        }

        public bool LevelHasFreeShipping
        {
            get { return m_LevelHasFreeShipping; }
        }

        public bool LevelHasNoTax
        {
            get { return m_LevelHasNoTax; }
        }

        public int LevelID
        {
            get { return m_LevelID; }
        }

        public string LevelName
        {
            get { return m_LevelName; }
        }

        public string OrderNotes
        {
            get { return m_OrderNotes; }
        }

        public AddressInfo BillingAddress
        {
            get
            {
                return m_BillingAddress;
            }
        }
        public String ShippingTrackingNumber
        {
            get
            {
                return m_ShippingTrackingNumber;
            }
        }

        public String ShippingMethod
        {
            get
            {
                return m_ShippingMethod;
            }
        }

        public String ChildOrderNumbers
        {
            get
            {
                return m_ChildOrderNumbers;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return m_IsEmpty;
            }
        }

        public DateTime OrderDate
        {
            get
            {
                return m_OrderDate;
            }
        }

        //v7  Added input InhibitEmail. Used for Google Checkout since Google sends the email instead of us.
        static public void MarkOrderAsShipped(int OrderNumber, String ShippedVIA, String ShippingTrackingNumber, DateTime ShippedOn, bool IsRecurring, System.Collections.Generic.Dictionary<string, EntityHelper> EntityHelpers, Parser UseParser, bool InhibitEmail)
        {
            DB.ExecuteSQL("Update orders set IsNew=0, ReadyToShip=1, ShippedVIA=" + DB.SQuote(ShippedVIA) + ", ShippingTrackingNumber=" + DB.SQuote(ShippingTrackingNumber) + ", ShippedOn=" + DB.DateQuote(Localization.ToDBDateTimeString(ShippedOn)) + " where OrderNumber=" + OrderNumber.ToString());

            if (!InhibitEmail)
            {
                bool OKToSend = false;
                int orderStoreId = Order.GetOrderStoreID(OrderNumber);

                String MailServer = AppLogic.AppConfig("MailMe_Server", orderStoreId, true);
                if (IsRecurring)
                {
                    if (AppLogic.AppConfigBool("Recurring.SendShippedEMailToCustomer", orderStoreId, true) && MailServer.Length != 0 && MailServer != AppLogic.ro_TBD)
                    {
                        OKToSend = true;
                    }
                }
                else
                {
                    if (AppLogic.AppConfigBool("SendShippedEMailToCustomer", orderStoreId, true) && MailServer.Length != 0 && MailServer != AppLogic.ro_TBD)
                    {
                        OKToSend = true;
                    }
                }
                if (OKToSend)
                {
                    try
                    {
                        Order order = new Order(OrderNumber, null);
                        // try to send "shipped on" EMail
                        String SubjectShipped = String.Format(AppLogic.GetString("common.cs.9", order.SkinID, order.LocaleSetting), AppLogic.AppConfig("StoreName", orderStoreId, true));
                        if (IsRecurring)
                        {
                            SubjectShipped += AppLogic.GetString("common.cs.10", order.SkinID, order.LocaleSetting);
                        }

                        String BodyShipped = order.ShippedNotification();
                        if (MailServer.Length != 0 && 
                            MailServer.Equals(AppLogic.ro_TBD, StringComparison.InvariantCultureIgnoreCase) == false)
                        {
                            AppLogic.SendMail(SubjectShipped, BodyShipped + AppLogic.AppConfig("MailFooter", orderStoreId, true), true, AppLogic.AppConfig("ReceiptEMailFrom", orderStoreId, true), AppLogic.AppConfig("ReceiptEMailFromName", orderStoreId, true), order.EMail, order.EMail, String.Empty, MailServer);
                        }
                    }
                    catch { }
                }
            }

        }

        static public bool OrderHasCleared(int OrderNumber)
        {
            bool tmp = false;
            if (OrderNumber != 0)
            {
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS("Select TransactionState from orders   with (NOLOCK)  where ordernumber=" + OrderNumber.ToString(), con))
                    {
                        if (rs.Read())
                        {
                            tmp = DB.RSField(rs, "TransactionState") == AppLogic.ro_TXStateCaptured;
                        }
                    }
                }
            }
            return tmp;
        }

        public bool TransactionIsCaptured()
        {
            return m_TransactionState == AppLogic.ro_TXStateCaptured;
        }

        /// <summary>
        /// Returns true if this order is Authorized.
        /// </summary>
        /// <returns></returns>
        public bool TransactionIsAuth()
        {
            return m_TransactionState == AppLogic.ro_TXStateAuthorized;
        }

        public bool TransactionIsRecurringAutoBill()
        {
            return m_TransactionType == AppLogic.TransactionTypeEnum.RECURRING_AUTO;
        }

        public string CaptureTXResult
        {
            get { return m_CaptureTXResult; }
            set
            {
                SqlParameter sp1 = new SqlParameter("@CaptureTXResult", SqlDbType.NText, 1000000000);
                sp1.Value = value;
                SqlParameter[] spa = { sp1 };
                string retval = this.UpdateOrder(spa);
                if (retval == string.Empty)
                {
                    m_CaptureTXResult = value;
                }

            }
        }

        public string CaptureTXCommand
        {
            get { return m_CaptureTXCommand; }
            set
            {
                SqlParameter sp1 = new SqlParameter("@CaptureTXCommand", SqlDbType.NText, 1000000000);
                sp1.Value = value;
                SqlParameter[] spa = { sp1 };
                string retval = this.UpdateOrder(spa);
                if (retval == string.Empty)
                {
                    m_CaptureTXCommand = value;
                }

            }
        }

        public string RefundTXResult
        {
            get { return m_RefundTXResult; }
            set
            {
                SqlParameter sp1 = new SqlParameter("@RefundTXResult", SqlDbType.NText, 1000000000);
                sp1.Value = value;
                SqlParameter[] spa = { sp1 };
                string retval = this.UpdateOrder(spa);
                if (retval == string.Empty)
                {
                    m_RefundTXResult = value;
                }

            }
        }

        public string RefundTXCommand
        {
            get { return m_RefundTXCommand; }
            set
            {
                SqlParameter sp1 = new SqlParameter("@RefundTXCommand", SqlDbType.NText, 1000000000);
                sp1.Value = value;
                SqlParameter[] spa = { sp1 };
                string retval = this.UpdateOrder(spa);
                if (retval == string.Empty)
                {
                    m_RefundTXCommand = value;
                }

            }
        }
        
        public string AuthorizationPNREF
        {
            get { return m_AuthorizationPNREF; }
            set
            {
                SqlParameter sp1 = new SqlParameter("@AuthorizationPNREF", SqlDbType.NVarChar, 100);
                sp1.Value = value;
                SqlParameter[] spa = { sp1 };
                string retval = this.UpdateOrder(spa);
                if (retval == string.Empty)
                {
                    m_AuthorizationPNREF = value;
                }

            }
        }

        public string CardExpirationMonth
        {
            get { return m_CardExpirationMonth; }
            set
            {
                SqlParameter sp1 = new SqlParameter("@CardExpirationMonth", SqlDbType.NVarChar, 10);
                sp1.Value = value;
                SqlParameter[] spa = { sp1 };
                string retval = this.UpdateOrder(spa);
                if (retval == string.Empty)
                {
                    m_CardExpirationMonth = value;
                }

            }
        }

        public string CardExpirationYear
        {
            get { return m_CardExpirationYear; }
            set
            {
                SqlParameter sp1 = new SqlParameter("@CardExpirationYear", SqlDbType.NVarChar, 10);
                sp1.Value = value;
                SqlParameter[] spa = { sp1 };
                string retval = this.UpdateOrder(spa);
                if (retval == string.Empty)
                {
                    m_CardExpirationYear = value;
                }

            }
        }

        public string AuthorizationCode
        {
            get { return m_AuthorizationCode; }
            set
            {
                SqlParameter sp1 = new SqlParameter("@AuthorizationCode", SqlDbType.NVarChar, 100);
                sp1.Value = value;
                SqlParameter[] spa = { sp1 };
                string retval = this.UpdateOrder(spa);
                if (retval == string.Empty)
                {
                    m_AuthorizationCode = value;
                }

            }
        }

        public string CardNumber
        {
            get { return m_CardNumber; }
            set
            {
                SqlParameter sp1 = new SqlParameter("@CardNumber", SqlDbType.NText, 1000000000);
                sp1.Value = value;
                SqlParameter[] spa = { sp1 };
                string retval = this.UpdateOrder(spa);
                if (retval == string.Empty)
                {
                    m_CardNumber = value;
                }

            }
        }

        public string OrdersCCSaltField
        {
            get { return m_OrdersCCSaltField; }
        }

        public string TransactionState
        {
            get { return m_TransactionState; }
            set
            {
                SqlParameter sp1 = new SqlParameter("@TransactionState", SqlDbType.NVarChar, 20);
                sp1.Value = value;
                SqlParameter[] spa = { sp1 };
                string retval = this.UpdateOrder(spa);
                if (retval == string.Empty)
                {
                    m_TransactionState = value;
                }

            }
        }

        public AppLogic.TransactionTypeEnum TransactionType
        {
            get { return m_TransactionType; }
        }
        
        public string Last4
        {
            get { return m_Last4; }
            set
            {
                SqlParameter sp1 = new SqlParameter("@Last4", SqlDbType.NVarChar, 4);
                sp1.Value = value;
                SqlParameter[] spa = { sp1 };
                string retval = this.UpdateOrder(spa);
                if (retval == string.Empty)
                {
                    m_Last4 = value;
                }

            }
        }


        public String RecurringSubscriptionID
        {
            get
            {
                return m_RecurringSubscriptionID;
            }
        }
        
        public string CardName
        {
            get { return m_CardName; }
            set
            {
                SqlParameter sp1 = new SqlParameter("@CardName", SqlDbType.NVarChar, 100);
                sp1.Value = value;
                SqlParameter[] spa = { sp1 };
                string retval = this.UpdateOrder(spa);
                if (retval == string.Empty)
                {
                    m_CardName = value;
                }

            }
        }


        /*
        public string temp
        {
            get { return m_temp; }
            set
            {
                SqlParameter sp1 = new SqlParameter("@temp", SqlDbType.NVarChar, 100);
                sp1.Value = value;
                SqlParameter[] spa = { sp1 };
                string retval = this.UpdateOrder(spa);
                if (retval == string.Empty)
                {
                    m_temp = value;
                }

            }
        }
        */
        public decimal OrderBalance
        {
            get
            {
                decimal t = Total(true);
                return t - CommonLogic.IIF(this.GetCoupon().m_coupontype == CouponTypeEnum.GiftCard, CommonLogic.IIF(t < this.GetCoupon().m_discountamount, t, this.GetCoupon().m_discountamount), 0);
            }
        }

        public bool AlreadyConfirmed
        {
            get
            {
                return m_AlreadyConfirmed;
            }
        }
        
        public int AffiliateID
        {
            get
            {
                return m_AffiliateID;
            }
        }

        public String AffiliateName
        {
            get
            {
                String tmpS = String.Empty;
                if (AffiliateID > 0)
                {
                    EntityHelper AffiliateHelper = AppLogic.LookupHelper(EntityDefinitions.readonly_AffiliateEntitySpecs.m_EntityName, 0);
                    tmpS = AffiliateHelper.GetEntityName(AffiliateID, Localization.GetDefaultLocale());
                }
                return tmpS;
            }
        }

        public static void DeleteAnyOrderDownloadFiles(int OrderNumber)
        {
            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("Select CustomerID from Orders  with (NOLOCK)  where OrderNumber=" + OrderNumber.ToString(), con))
                {
                    if (rs.Read())
                    {
                        int CustomerID = DB.RSFieldInt(rs, "CustomerID");
                        String ThisOrderDownloadDir = CommonLogic.SafeMapPath(CommonLogic.IIF(AppLogic.IsAdminSite, "../", "") + "orderdownloads/" + OrderNumber.ToString() + "_" + CustomerID.ToString());
                        if (System.IO.Directory.Exists(ThisOrderDownloadDir))
                        {
                            Directory.Delete(ThisOrderDownloadDir, true);
                        }
                    }
                }
            }
        }

        public static int GetOrderCustomerID(int OrderNumber)
        {
            int tmp = 0;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("Select CustomerID from Orders  with (NOLOCK)  where OrderNumber=" + OrderNumber.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSFieldInt(rs, "CustomerID");
                    }
                }
            }

            return tmp;
        }


        /// <summary>
        /// Gets the customer level identifier of the specific order.
        /// </summary>
        /// <param name="OrderNumber">The order number.</param>
        /// <returns></returns>
        public static int GetOrderCustomerLevelID(int OrderNumber)
        {
            int tmp = 0;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                
                using (IDataReader rs = DB.GetRS("Select CustomerLevelID from Customer where CustomerID = (Select CustomerID from Orders with (NOLOCK)  where OrderNumber=" + OrderNumber.ToString() + ")", con))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSFieldInt(rs, "CustomerLevelID");
                    }
                }
            }

            return tmp;
        }

        public bool ContainsGiftCard()
        {
            return m_CartItems.ContainsGiftCard;
        }

        public static void MarkOrderAsFraud(int OrderNumber, bool SetFraudStateOn)
        {
            int CouponType = 0;
            string CouponCode = "";
            decimal CouponDiscountAmount = 0.0M;
            int CustomerID = 0;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("Select CustomerID, CouponType, CouponCode, CouponDiscountAmount from Orders  with (NOLOCK)  where OrderNumber=" + OrderNumber.ToString(), con))
                {
                    if (rs.Read())
                    {
                        CustomerID = DB.RSFieldInt(rs, "CustomerID");
                        CouponType = DB.RSFieldInt(rs, "CouponType");
                        CouponCode = DB.RSField(rs, "CouponCode");
                        CouponDiscountAmount = DB.RSFieldDecimal(rs, "CouponDiscountAmount");
                    }
                }
            }

            // force this customer to be logged out:
            DB.ExecuteSQL("update Customer set CustomerGUID=newid() where CustomerID=" + CustomerID.ToString());

            // force this customer's subscription to be invalid:
            DB.ExecuteSQL("update Customer set SubscriptionExpiresOn=NULL where CustomerID=" + CustomerID.ToString());
            
            // make sure inventory was restored. safe to call repeatedly. proc protects against deducting twice
            DB.ExecuteSQL("aspdnsf_AdjustInventory " + OrderNumber.ToString() + "," + CommonLogic.IIF(SetFraudStateOn,"1","-1"));
            DB.ExecuteSQL("aspdnsf_MarkOrderAsFraud " + OrderNumber.ToString() + "," + CommonLogic.IIF(SetFraudStateOn, "1", "0"));
            if (SetFraudStateOn)
            {
                Order.DeleteAnyOrderDownloadFiles(OrderNumber);

                //Invalidate GiftCards ordered on this order
                GiftCards GCs = new GiftCards(OrderNumber, GiftCardCollectionFilterType.OrderNumber);
                foreach (GiftCard gc in GCs)
                {
                    gc.GiftCardTransactions.Add(GiftCardUsageTransaction.CreateTransaction(gc.GiftCardID, GiftCardUsageReasons.FundsRemovedByAdmin, 0, 0, gc.Balance, ""));
                    gc.UpdateCard(null, null, null, null, 1, null, null, null, null, null, null, null, null, null, null);
                }

                //remove any balance remianing on the coupon used in paying for the order
                if ((CouponTypeEnum)CouponType == CouponTypeEnum.GiftCard)
                {
                    GiftCard gc = new GiftCard(CouponCode);
                    if (gc.GiftCardID != 0)
                    {
                        gc.UpdateCard(null, null, null, null, 1, null, null, null, null, null, null, null, null, null, null);
                    }
                }

                // lock this customer's account:
                Customer.LockAccount(CustomerID,true);
            }
            else
            {
                //Restore GiftCard 
                GiftCards GCs = new GiftCards(OrderNumber, GiftCardCollectionFilterType.OrderNumber);
                foreach (GiftCard gc in GCs)
                {
                    gc.GiftCardTransactions.Add(GiftCardUsageTransaction.CreateTransaction(gc.GiftCardID, GiftCardUsageReasons.FundsAddedByAdmin, 0, 0, gc.InitialAmount, ""));
                    gc.UpdateCard(null, null, null, null, 0, null, null, null, null, null, null, null, null, null, null);
                }

                //remove any balance remianing on the coupon used in paying for the order
                if ((CouponTypeEnum)CouponType == CouponTypeEnum.GiftCard)
                {
                    GiftCard gc = new GiftCard(CouponCode);
                    if (gc.GiftCardID != 0)
                    {
                        gc.UpdateCard(null, null, null, null, 0, null, null, null, null, null, null, null, null, null, null);
                    }
                }

                // unlock this customer's account:
                Customer.LockAccount(CustomerID, false);

            }

        }

        public static void AdminDeleteOrphanedOrders()
        {
            DB.ExecuteSQL("delete from orders where ordernumber not in (select ordernumber from orders_ShoppingCart) and ordernumber not in (select ordernumber from orders_kitcart)");
            DB.ExecuteSQL("delete from orders_kitcart where ordernumber not in (select ordernumber from orders)");
            DB.ExecuteSQL("delete from orders_ShoppingCart where ordernumber not in (select ordernumber from orders)");
        }

        public static String StaticGetSaltKey(int OrderNumber)
        {
            String tmp = String.Empty;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select " + AppLogic.AppConfig("OrdersCCSaltField") + " from Orders  with (NOLOCK)  where OrderNumber=" + OrderNumber.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmp = rs[AppLogic.AppConfig("OrdersCCSaltField")].ToString();
                    }
                }
            }

            return tmp;
        }

        static public bool BuildReOrder(System.Collections.Generic.Dictionary<string, EntityHelper> EntityHelpers, Customer ThisCustomer, int OrderNumber, out String status)
        {
            status = AppLogic.ro_OK;
            Order ord = new Order(OrderNumber, ThisCustomer.LocaleSetting);
            if (ord.IsEmpty)
            {
                status = AppLogic.GetString("reorder.aspx.3", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                return false;
            }

            if (ord.HasRecurringComponents())
            {
                status = AppLogic.GetString("reorder.aspx.4", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                return false;
            }

            // ------------------------------------------------------------------------------------------------------------
            // ok, try to rebuild the old order as best we can, need to consider:
            //
            // * product & variant definition mods
            // * address change mods
            // * shipping method mods
            // * kit changes
            // * price changes
            // * multi-ship order types
            // * product changes
            // * sale price changes
            // * extended price changes
            // * customer level changes
            // * inventory changes/out of stock issues etc (ignore inventory issues for now!)
            // * how do we handle a prior coupon used also (ignoring coupons for now!)
            //
            // Remember, since last time it was ordered. NOT A SINGLE thing in the old order may even be valid anymore!
            // 
            // we are going to edit the order object in memory first, NOT in the db yet!
            // ------------------------------------------------------------------------------------------------------------

            ShoppingCart cart = new ShoppingCart(ord.SkinID, ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);
            if (AppLogic.AppConfigBool("Reorder.ClearCartBeforeAddingReorderItems"))
            {
                cart.ClearContents();
            }

            try
            {
                foreach (CartItem c in ord.CartItems)
                {
                    if (!AppLogic.ProductHasBeenDeleted(c.ProductID) && !AppLogic.VariantHasBeenDeleted(c.VariantID) && !c.IsRecurring)
                    {
                        // NOTE:
                        //  Let's use the customer's shipping address this time
                        Address address = new Address(ThisCustomer.CustomerID);
                        address.LoadByCustomer(ThisCustomer.CustomerID, AddressTypes.Shipping);
                        int ShipAddrID = address.AddressID;

                        if (AppLogic.IsAKit(c.ProductID))
                        {
                            // not supported yet!
                            String tmp = DB.GetNewGUID();

                            KitComposition composition = KitComposition.FromOrder(ThisCustomer, OrderNumber, c.ShoppingCartRecordID);
                            cart.AddItem(ThisCustomer, ShipAddrID, c.ProductID, c.VariantID, c.Quantity, tmp, c.ChosenColorSKUModifier, c.ChosenSize, c.ChosenSizeSKUModifier, c.TextOption, CartTypeEnum.ShoppingCart, true, false, c.GiftRegistryForCustomerID, System.Decimal.Zero, composition);

                            int NewRecID = 0;
                            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                            {
                                con.Open();
                                using (IDataReader rs = DB.GetRS("select ShoppingCartRecID from ShoppingCart where ChosenColor=" + DB.SQuote(tmp) + " and CustomerID=" + ThisCustomer.CustomerID.ToString(), con))
                                {
                                    rs.Read();
                                    NewRecID = DB.RSFieldInt(rs, "ShoppingCartRecID");
                                }
                            }

                            if (composition == null)
                            {
                                String sql = String.Format("insert KitCart(CustomerID,ShoppingCartRecID,ProductID,VariantID,KitGroupID,KitItemID,TextOption,Quantity,CartType,OriginalRecurringOrderNumber,KitGroupTypeID,InventoryVariantID,InventoryVariantColor,InventoryVariantSize) select {0},{1},ProductID,VariantID,KitGroupID,KitItemID,TextOption,Quantity,{2},0,KitGroupTypeID,InventoryVariantID,InventoryVariantColor,InventoryVariantSize from Orders_KitCart where ShoppingCartRecID={3}", ThisCustomer.CustomerID.ToString(), NewRecID.ToString(), ((int)CartTypeEnum.ShoppingCart).ToString(), c.ShoppingCartRecordID.ToString());
                                DB.ExecuteSQL(sql);
                            }

                            DB.ExecuteSQL("update ShoppingCart set ProductPrice=" + Localization.CurrencyStringForDBWithoutExchangeRate(AppLogic.GetKitTotalPrice(ThisCustomer.CustomerID, ThisCustomer.CustomerLevelID, c.ProductID, c.VariantID, NewRecID)) + ", ChosenColor=" + DB.SQuote("") + " where ShoppingCartRecID=" + NewRecID.ToString());
                        }
                        else
                        {
                            //area to get fund and bucks used with original order for this reorder option
                            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                            {
                                
                                con.Open();
                                using (IDataReader rs = DB.GetRS("select CategoryFundUsed,BluBucksUsed,CategoryFundType,BluBucksPercentageUsed,ProductCategoryId,GLcode,SOFCode from Orders_ShoppingCart where OrderNumber=" + OrderNumber + " and CustomerID=" + ThisCustomer.CustomerID.ToString() + " and ProductID=" + c.ProductID.ToString() + " and VariantID=" + c.VariantID.ToString(), con))
                                {
                                    rs.Read();
                                    c.CategoryFundUsed = DB.RSFieldDecimal(rs, "CategoryFundUsed");
                                    c.BluBuksUsed = DB.RSFieldDecimal(rs, "BluBucksUsed");
                                    c.FundID = DB.RSFieldInt(rs, "CategoryFundType");
                                    c.BluBucksPercentageUsed = DB.RSFieldDecimal(rs, "BluBucksPercentageUsed");
                                    c.ProductCategoryID = DB.RSFieldInt(rs, "ProductCategoryId");
                                    c.GLcode = DB.RSField(rs, "GLcode");
                                    c.SOFCode = DB.RSField(rs, "SOFCode");
                                }
                            }
                            //end area
                            cart.AddItem(ThisCustomer, ShipAddrID, c.ProductID, c.VariantID, c.Quantity, c.ChosenColor, c.ChosenColorSKUModifier, c.ChosenSize, c.ChosenSizeSKUModifier, c.TextOption, CartTypeEnum.ShoppingCart, true, false, c.GiftRegistryForCustomerID, System.Decimal.Zero, c.BluBuksUsed, c.CategoryFundUsed, c.FundID, c.BluBucksPercentageUsed, c.ProductCategoryID, c.GLcode, c.SOFCode);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                status = CommonLogic.GetExceptionDetail(ex, "");
                return false;
            }

            return true;
        }

        static public bool OrderExists(int ordernumber)
        {
            return DB.GetSqlN("select OrderNumber N from dbo.orders where OrderNumber = " + ordernumber.ToString()) > 0;
        }
        
        public bool IsEditable()
        {
            if (!TransactionStateAllowsEdits())
                return false;

            if(ContainsGiftCard() || HasGiftRegistryComponents() || HasMultipleShippingAddresses() || HasRecurringComponents())
            {
                return false;
            }
            if (!EditedOn.Equals(System.DateTime.MinValue) || !DownloadEMailSentOn.Equals(System.DateTime.MinValue) || !ShippedOn.Equals(System.DateTime.MinValue) || !DistributorEMailSentOn.Equals(System.DateTime.MinValue))
            {
                return false;
            }
            String PM = AppLogic.CleanPaymentMethod(PaymentMethod);
            if (PM == AppLogic.ro_PMPayPal || PaymentMethod == AppLogic.ro_PMPayPalExpress || PM.EqualsIgnoreCase("CHECKOUTBYAMAZON"))
            {
                return false;
            }
            // passed all tests, so ok to edit:
            return true;
        }

        private bool TransactionStateAllowsEdits()
        {
            if (TransactionState == AppLogic.ro_TXStateAuthorized)
                return true;

            if (TransactionState == AppLogic.ro_TXStateCaptured && AppLogic.AppConfigBool("OrderEditing.AllowEditingCapturedOrders"))
                return true;

            return false;
        }

        public bool HasBeenEdited
        {
            get { return !EditedOn.Equals(System.DateTime.MinValue); }
        }


        public string RegenerateReceipt(Customer ViewingCustomer)
        {
            String Status = AppLogic.ro_OK;
            String PackageName = AppLogic.AppConfig("XmlPackage.OrderReceipt");
                string runtimeParams = string.Format("ordernumber={0}&ShowOnlineLink={1}", OrderNumber.ToString(), false);
                String result = AppLogic.RunXmlPackage(PackageName, null, ViewingCustomer, AppLogic.GetStoreSkinID(GetOrderStoreID(OrderNumber)), string.Empty, runtimeParams, false, true);
                if (!result.Contains(new Topic("InvalidRequest", ViewingCustomer.LocaleSetting, 1).Contents))
                    SaveReceipt(result);
            return Status;
        }
    }
}
