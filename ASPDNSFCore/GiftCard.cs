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
using System.Web.Security;
using System.Text;
using System.Xml;
using System.Collections;
using System.Globalization;

namespace AspDotNetStorefrontCore
{

    public enum GiftCardTypes : int
    {
        Unknown = 0,
        PhysicalGiftCard = 100,
        EMailGiftCard = 101,
        CertificateGiftCard = 102
    }

    public enum GiftCardUsageReasons : int
    {
        Unknown = 0,
        CreationEvent = 1, 
        UsedByCustomer = 2,
        FundsAddedByAdmin = 3,
        FundsRemovedByAdmin = 4
    }

    /// <summary>
    /// </summary>
    public class GiftCard
    {

        private int m_GiftCardID = 0;
        private string m_GiftCardGUID = String.Empty;
        private string m_SerialNumber = String.Empty;
        private int m_OrderNumber = 0;
        private int m_PurchasedByCustomerID = 0;
        private int m_ShoppingCartRecID = 0;
        private int m_ProductID = 0;
        private int m_VariantID = 0;
        private decimal m_InitialAmount = System.Decimal.Zero;
        private decimal m_Balance = System.Decimal.Zero;
        private DateTime m_ExpirationDate = System.DateTime.MinValue;
        private int m_GiftCardTypeID = 101;
        private string m_EMailName = String.Empty;
        private string m_EMailTo = String.Empty;
        private string m_EMailMessage = String.Empty;
        private string m_ValidForCustomers = String.Empty;
        private string m_ValidForProducts = String.Empty;
        private string m_ValidForManufacturers = String.Empty;
        private string m_ValidForCategories = String.Empty;
        private string m_ValidForSections = String.Empty;
        private string m_ExtensionData = String.Empty;
        private DateTime m_CreatedOn = System.DateTime.MinValue;
        private bool m_DisabledByAdministrator = false;
        private GiftCardTransactions m_giftcardransactions = null;

        public GiftCard()
        {
        }

        public GiftCard(int GiftCardID)
        {
            LoadFromDB(GiftCardID);
          
        }

        public GiftCard(string SerialNumber)
        {
            LoadFromDB(SerialNumber);
          
        }

        public string GiftCardKey(int CustomerID)
        {
            string result = CustomerID.ToString().ToUpperInvariant();
            string ticks = DateTime.Now.Ticks.ToString().ToUpperInvariant();
            result += "-" + ticks.Substring(1, 5);
            result += "-" + ticks.Substring(6, 5);
            result += "-" + ticks.Substring(11, 5);
            return result;
        }

        /// <summary>
        /// Updates GiftCard records to Order_ShoppingCart table
        /// </summary>
        /// <param name="OrderNumber">Order containing gift card</param>
        public static void SyncOrderNumber(int OrderNumber)
        {
            DB.ExecuteSQL(
                @"
                UPDATE GiftCard SET OrderNumber = @OrderNumber WHERE ShoppingCartRecID IN (
                SELECT ShoppingCartRecID FROM Orders_ShoppingCart WHERE OrderNumber = @OrderNumber)
                ",
                new SqlParameter[] { new SqlParameter("@OrderNumber", OrderNumber) })
                ;
        }


        public static GiftCard CreateGiftCard(int PurchasedByCustomerID,  string SerialNumber, object OrderNumber,  int ShoppingCartRecID,  object ProductID,  object VariantID,  object InitialAmount,  object ExpirationDate, object Balance,  object GiftCardTypeID,  string EMailName,  string EMailTo,  string EMailMessage,  string ValidForCustomers,  string ValidForProducts,  string ValidForManufacturers,  string ValidForCategories,  string ValidForSections,  string ExtensionData)
        {

            int GiftCardID = 0;

            string err = String.Empty;
            SqlConnection cn = new SqlConnection(DB.GetDBConn());
            cn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = cn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "dbo.aspdnsf_CreateGiftCard";

            cmd.Parameters.Add(new SqlParameter("@SerialNumber", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@PurchasedByCustomerID", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@OrderNumber", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@ShoppingCartRecID", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@ProductID", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@VariantID", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@InitialAmount", SqlDbType.Decimal, 8));
            cmd.Parameters.Add(new SqlParameter("@Balance", SqlDbType.Decimal, 8));
            cmd.Parameters.Add(new SqlParameter("@ExpirationDate", SqlDbType.DateTime, 8));
            cmd.Parameters.Add(new SqlParameter("@GiftCardTypeID", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@EMailName", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@EMailTo", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@EMailMessage", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@ValidForCustomers", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@ValidForProducts", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@ValidForManufacturers", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@ValidForCategories", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@ValidForSections", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@ExtensionData", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@GiftCardID", SqlDbType.Int, 4)).Direction = ParameterDirection.Output;

            cmd.Parameters["@PurchasedByCustomerID"].Value = PurchasedByCustomerID;

            if (SerialNumber == null) cmd.Parameters["@SerialNumber"].Value = DBNull.Value;
            else cmd.Parameters["@SerialNumber"].Value = SerialNumber;

            if (OrderNumber == null) cmd.Parameters["@OrderNumber"].Value = DBNull.Value;
            else cmd.Parameters["@OrderNumber"].Value = OrderNumber;

            cmd.Parameters["@ShoppingCartRecID"].Value = ShoppingCartRecID;

            if (ProductID == null) cmd.Parameters["@ProductID"].Value = DBNull.Value;
            else cmd.Parameters["@ProductID"].Value = ProductID;

            if (VariantID == null) cmd.Parameters["@VariantID"].Value = DBNull.Value;
            else cmd.Parameters["@VariantID"].Value = VariantID;

            if (InitialAmount == null) cmd.Parameters["@InitialAmount"].Value = DBNull.Value;
            else cmd.Parameters["@InitialAmount"].Value = InitialAmount;

            if (Balance == null) cmd.Parameters["@Balance"].Value = DBNull.Value;
            else cmd.Parameters["@Balance"].Value = Balance;

            if (ExpirationDate == null || !CommonLogic.IsDate(ExpirationDate.ToString())) cmd.Parameters["@ExpirationDate"].Value = DBNull.Value;
            else cmd.Parameters["@ExpirationDate"].Value = DateTime.Parse(ExpirationDate.ToString());

            if (GiftCardTypeID == null) cmd.Parameters["@GiftCardTypeID"].Value = DBNull.Value;
            else cmd.Parameters["@GiftCardTypeID"].Value = GiftCardTypeID;

            if (EMailName == null) cmd.Parameters["@EMailName"].Value = DBNull.Value;
            else cmd.Parameters["@EMailName"].Value = EMailName;

            if (EMailTo == null) cmd.Parameters["@EMailTo"].Value = DBNull.Value;
            else cmd.Parameters["@EMailTo"].Value = EMailTo;

            if (EMailMessage == null) cmd.Parameters["@EMailMessage"].Value = DBNull.Value;
            else cmd.Parameters["@EMailMessage"].Value = EMailMessage;

            if (ValidForCustomers == null) cmd.Parameters["@ValidForCustomers"].Value = DBNull.Value;
            else cmd.Parameters["@ValidForCustomers"].Value = ValidForCustomers;

            if (ValidForProducts == null) cmd.Parameters["@ValidForProducts"].Value = DBNull.Value;
            else cmd.Parameters["@ValidForProducts"].Value = ValidForProducts;

            if (ValidForManufacturers == null) cmd.Parameters["@ValidForManufacturers"].Value = DBNull.Value;
            else cmd.Parameters["@ValidForManufacturers"].Value = ValidForManufacturers;

            if (ValidForCategories == null) cmd.Parameters["@ValidForCategories"].Value = DBNull.Value;
            else cmd.Parameters["@ValidForCategories"].Value = ValidForCategories;

            if (ValidForSections == null) cmd.Parameters["@ValidForSections"].Value = DBNull.Value;
            else cmd.Parameters["@ValidForSections"].Value = ValidForSections;

            if (ExtensionData == null) cmd.Parameters["@ExtensionData"].Value = DBNull.Value;
            else cmd.Parameters["@ExtensionData"].Value = ExtensionData;


            try
            {
                cmd.ExecuteNonQuery();
                GiftCardID = Int32.Parse(cmd.Parameters["@GiftCardID"].Value.ToString());
            }
            catch (Exception ex)
            {
                err = ex.Message;
            }

            cn.Close();
            cmd.Dispose();
            cn.Dispose();

            if (GiftCardID > 0)
            {
                GiftCard g = new GiftCard(GiftCardID);
                return g;
            }
            return null;
        }

        public static string UpdateCard(int GiftCardID, string SerialNumber, object OrderNumber, object InitialAmount, object Balance, object DisabledByAdministrator, object ExpirationDate, string EMailName, string EMailTo, string EMailMessage, string ValidForCustomers, string ValidForProducts, string ValidForManufacturers, string ValidForCategories, string ValidForSections, string ExtensionData)
        {

            string err = String.Empty;
            SqlConnection cn = new SqlConnection(DB.GetDBConn());
            cn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = cn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "dbo.aspdnsf_updGiftCard";

            cmd.Parameters.Add(new SqlParameter("@GiftCardID", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@SerialNumber", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@OrderNumber", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@InitialAmount", SqlDbType.Decimal, 8));
            cmd.Parameters.Add(new SqlParameter("@Balance", SqlDbType.Decimal, 8));
            cmd.Parameters.Add(new SqlParameter("@ExpirationDate", SqlDbType.VarChar, 30));
            cmd.Parameters.Add(new SqlParameter("@EMailName", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@EMailTo", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@EMailMessage", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@ValidForCustomers", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@ValidForProducts", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@ValidForManufacturers", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@ValidForCategories", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@ValidForSections", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@DisabledByAdministrator", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@ExtensionData", SqlDbType.NText));

            cmd.Parameters["@GiftCardID"].Value = GiftCardID;

            if (SerialNumber == null) cmd.Parameters["@SerialNumber"].Value = DBNull.Value;
            else cmd.Parameters["@SerialNumber"].Value = SerialNumber;

            if (OrderNumber == null) cmd.Parameters["@OrderNumber"].Value = DBNull.Value;
            else cmd.Parameters["@OrderNumber"].Value = OrderNumber;

            if (InitialAmount == null) cmd.Parameters["@InitialAmount"].Value = DBNull.Value;
            else cmd.Parameters["@InitialAmount"].Value = InitialAmount;

            if (Balance == null) cmd.Parameters["@Balance"].Value = DBNull.Value;
            else cmd.Parameters["@Balance"].Value = Balance;

            if (ExpirationDate == null) cmd.Parameters["@ExpirationDate"].Value = DBNull.Value;
            else cmd.Parameters["@ExpirationDate"].Value = ExpirationDate;

            if (EMailName == null) cmd.Parameters["@EMailName"].Value = DBNull.Value;
            else cmd.Parameters["@EMailName"].Value = EMailName;

            if (EMailTo == null) cmd.Parameters["@EMailTo"].Value = DBNull.Value;
            else cmd.Parameters["@EMailTo"].Value = EMailTo;

            if (EMailMessage == null) cmd.Parameters["@EMailMessage"].Value = DBNull.Value;
            else cmd.Parameters["@EMailMessage"].Value = EMailMessage;

            if (ValidForCustomers == null) cmd.Parameters["@ValidForCustomers"].Value = DBNull.Value;
            else cmd.Parameters["@ValidForCustomers"].Value = ValidForCustomers;

            if (ValidForProducts == null) cmd.Parameters["@ValidForProducts"].Value = DBNull.Value;
            else cmd.Parameters["@ValidForProducts"].Value = ValidForProducts;

            if (ValidForManufacturers == null) cmd.Parameters["@ValidForManufacturers"].Value = DBNull.Value;
            else cmd.Parameters["@ValidForManufacturers"].Value = ValidForManufacturers;

            if (ValidForCategories == null) cmd.Parameters["@ValidForCategories"].Value = DBNull.Value;
            else cmd.Parameters["@ValidForCategories"].Value = ValidForCategories;

            if (ValidForSections == null) cmd.Parameters["@ValidForSections"].Value = DBNull.Value;
            else cmd.Parameters["@ValidForSections"].Value = ValidForSections;

            if (DisabledByAdministrator == null) cmd.Parameters["@DisabledByAdministrator"].Value = DBNull.Value;
            else cmd.Parameters["@DisabledByAdministrator"].Value = DisabledByAdministrator;

            if (ExtensionData == null) cmd.Parameters["@ExtensionData"].Value = DBNull.Value;
            else cmd.Parameters["@ExtensionData"].Value = ExtensionData;

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

        public static void DeleteGiftCardsInCart(int ShoppingCartRecID)
        {
            DB.ExecuteSQL("DELETE GiftCard WHERE ShoppingCartRecID = " + ShoppingCartRecID.ToString());
        }

        public static bool AddTransaction(int GiftCardID, decimal amount, int UsedByCustomerID, int OrderNumber)
        {
            GiftCardUsageTransaction ut = GiftCardUsageTransaction.CreateTransaction(GiftCardID, GiftCardUsageReasons.UsedByCustomer, UsedByCustomerID, OrderNumber, amount, "");
            return (ut != null);
        }


        public string UpdateCard(string SerialNumber, object OrderNumber, object InitialAmount, object Balance, object DisabledByAdministrator, object ExpirationDate, string EMailName, string EMailTo, string EMailMessage, string ValidForCustomers, string ValidForProducts, string ValidForManufacturers, string ValidForCategories, string ValidForSections, string ExtensionData)
        {

            string err = String.Empty;
            SqlConnection cn = new SqlConnection(DB.GetDBConn());
            cn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = cn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "dbo.aspdnsf_updGiftCard";

            cmd.Parameters.Add(new SqlParameter("@GiftCardID", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@SerialNumber", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@OrderNumber", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@InitialAmount", SqlDbType.Decimal, 8));
            cmd.Parameters.Add(new SqlParameter("@Balance", SqlDbType.Decimal, 8));
            cmd.Parameters.Add(new SqlParameter("@ExpirationDate", SqlDbType.VarChar, 30));
            cmd.Parameters.Add(new SqlParameter("@EMailName", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@EMailTo", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@EMailMessage", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@ValidForCustomers", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@ValidForProducts", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@ValidForManufacturers", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@ValidForCategories", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@ValidForSections", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@DisabledByAdministrator", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@ExtensionData", SqlDbType.NText));

            cmd.Parameters["@GiftCardID"].Value = this.m_GiftCardID;

            if (SerialNumber == null) cmd.Parameters["@SerialNumber"].Value = DBNull.Value;
            else cmd.Parameters["@SerialNumber"].Value = SerialNumber;

            if (OrderNumber == null) cmd.Parameters["@OrderNumber"].Value = DBNull.Value;
            else cmd.Parameters["@OrderNumber"].Value = OrderNumber;

            if (InitialAmount == null) cmd.Parameters["@InitialAmount"].Value = DBNull.Value;
            else cmd.Parameters["@InitialAmount"].Value = InitialAmount;

            if (Balance == null) cmd.Parameters["@Balance"].Value = DBNull.Value;
            else cmd.Parameters["@Balance"].Value = Balance;

            if (ExpirationDate == null) cmd.Parameters["@ExpirationDate"].Value = DBNull.Value;
            else cmd.Parameters["@ExpirationDate"].Value = ExpirationDate;

            if (EMailName == null) cmd.Parameters["@EMailName"].Value = DBNull.Value;
            else cmd.Parameters["@EMailName"].Value = EMailName;

            if (EMailTo == null) cmd.Parameters["@EMailTo"].Value = DBNull.Value;
            else cmd.Parameters["@EMailTo"].Value = EMailTo;

            if (EMailMessage == null) cmd.Parameters["@EMailMessage"].Value = DBNull.Value;
            else cmd.Parameters["@EMailMessage"].Value = EMailMessage;

            if (ValidForCustomers == null) cmd.Parameters["@ValidForCustomers"].Value = DBNull.Value;
            else cmd.Parameters["@ValidForCustomers"].Value = ValidForCustomers;

            if (ValidForProducts == null) cmd.Parameters["@ValidForProducts"].Value = DBNull.Value;
            else cmd.Parameters["@ValidForProducts"].Value = ValidForProducts;

            if (ValidForManufacturers == null) cmd.Parameters["@ValidForManufacturers"].Value = DBNull.Value;
            else cmd.Parameters["@ValidForManufacturers"].Value = ValidForManufacturers;

            if (ValidForCategories == null) cmd.Parameters["@ValidForCategories"].Value = DBNull.Value;
            else cmd.Parameters["@ValidForCategories"].Value = ValidForCategories;

            if (ValidForSections == null) cmd.Parameters["@ValidForSections"].Value = DBNull.Value;
            else cmd.Parameters["@ValidForSections"].Value = ValidForSections;

            if (DisabledByAdministrator == null) cmd.Parameters["@DisabledByAdministrator"].Value = DBNull.Value;
            else cmd.Parameters["@DisabledByAdministrator"].Value = DisabledByAdministrator;

            if (ExtensionData == null) cmd.Parameters["@ExtensionData"].Value = DBNull.Value;
            else cmd.Parameters["@ExtensionData"].Value = ExtensionData;

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
            RefreshCard();
            return err;

        }

        public void SerializeGiftCard()
        {
            //physical gift cards cannot be serialized
            if (this.m_GiftCardTypeID != (int)GiftCardTypes.PhysicalGiftCard)
            {
                Customer c = new Customer(m_PurchasedByCustomerID);
                string SerialNumber = String.Empty;
                string GiftCardXml = String.Empty;
                XmlDocument xdoc = new XmlDocument();

                try
                {
                    GiftCardXml = AppLogic.RunXmlPackage("giftcardassignment.xml.config", null, c, 1, String.Empty, String.Empty, false, true);
                    xdoc.LoadXml(GiftCardXml);
                    SerialNumber = xdoc.SelectSingleNode("//CardNumber").InnerText;
                }
                catch
                {
                    SerialNumber = CommonLogic.GetNewGUID();
                }
                UpdateCard(SerialNumber, null, null, null, null, null, null, null, null, null, null, null, null, null, null);
            }
        }

        public bool AddTransaction(decimal amount, int UsedByCustomerID, int OrderNumber)
        {
            GiftCardUsageTransaction ut = GiftCardUsageTransaction.CreateTransaction(this.GiftCardID, GiftCardUsageReasons.UsedByCustomer, UsedByCustomerID, OrderNumber, amount, "");
            RefreshCard();
            m_giftcardransactions = null;
            return (ut != null);
        }

        public void LoadFromDB(int GiftCardID)
        {
            Clear();

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS(String.Format("select GiftCard.* FROM GiftCard with (NOLOCK) where GiftCardID={0}", GiftCardID.ToString()), dbconn))
                {
                    if (rs.Read())
                    {
                        m_GiftCardID = DB.RSFieldInt(rs, "GiftCardID");
                        m_GiftCardGUID = DB.RSFieldGUID(rs, "GiftCardGUID");
                        m_SerialNumber = DB.RSField(rs, "SerialNumber");
                        m_OrderNumber = DB.RSFieldInt(rs, "OrderNumber");
                        m_PurchasedByCustomerID = DB.RSFieldInt(rs, "PurchasedByCustomerID");
                        m_ShoppingCartRecID = DB.RSFieldInt(rs, "ShoppingCartRecID");
                        m_ProductID = DB.RSFieldInt(rs, "ProductID");
                        m_VariantID = DB.RSFieldInt(rs, "VariantID");
                        m_InitialAmount = DB.RSFieldDecimal(rs, "InitialAmount");
                        m_Balance = DB.RSFieldDecimal(rs, "Balance");
                        m_ExpirationDate = DB.RSFieldDateTime(rs, "ExpirationDate");
                        m_EMailName = DB.RSField(rs, "EMailName");
                        m_EMailTo = DB.RSField(rs, "EMailTo");
                        m_EMailMessage = DB.RSField(rs, "EMailMessage");
                        m_ValidForCustomers = DB.RSField(rs, "ValidForCustomers");
                        m_ValidForProducts = DB.RSField(rs, "ValidForProducts");
                        m_ValidForManufacturers = DB.RSField(rs, "ValidForManufacturers");
                        m_ValidForCategories = DB.RSField(rs, "ValidForCategories");
                        m_ValidForSections = DB.RSField(rs, "ValidForSections");
                        m_ExtensionData = DB.RSField(rs, "ExtensionData");
                        m_CreatedOn = DB.RSFieldDateTime(rs, "CreatedOn");
                        m_GiftCardTypeID = DB.RSFieldInt(rs, "GiftCardTypeID");
                        m_DisabledByAdministrator = DB.RSFieldBool(rs, "DisabledByAdministrator");
                    }
                    else
                    {
                        Clear();
                    }
                }
            }
        }

        public void LoadFromDB(string SerialNumber)
        {
            Clear();

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS(String.Format("select GiftCard.* FROM GiftCard with (NOLOCK) where SerialNumber={0}", DB.SQuote(SerialNumber)), dbconn))
                {
                    if (rs.Read())
                    {
                        m_GiftCardID = DB.RSFieldInt(rs, "GiftCardID");
                        m_GiftCardGUID = DB.RSFieldGUID(rs, "GiftCardGUID");
                        m_SerialNumber = DB.RSField(rs, "SerialNumber");
                        m_OrderNumber = DB.RSFieldInt(rs, "OrderNumber");
                        m_PurchasedByCustomerID = DB.RSFieldInt(rs, "PurchasedByCustomerID");
                        m_ShoppingCartRecID = DB.RSFieldInt(rs, "ShoppingCartRecID");
                        m_ProductID = DB.RSFieldInt(rs, "ProductID");
                        m_VariantID = DB.RSFieldInt(rs, "VariantID");
                        m_InitialAmount = DB.RSFieldDecimal(rs, "InitialAmount");
                        m_Balance = DB.RSFieldDecimal(rs, "Balance");
                        m_ExpirationDate = DB.RSFieldDateTime(rs, "ExpirationDate");
                        m_EMailName = DB.RSField(rs, "EMailName");
                        m_EMailTo = DB.RSField(rs, "EMailTo");
                        m_EMailMessage = DB.RSField(rs, "EMailMessage");
                        m_ValidForCustomers = DB.RSField(rs, "ValidForCustomers");
                        m_ValidForProducts = DB.RSField(rs, "ValidForProducts");
                        m_ValidForManufacturers = DB.RSField(rs, "ValidForManufacturers");
                        m_ValidForCategories = DB.RSField(rs, "ValidForCategories");
                        m_ValidForSections = DB.RSField(rs, "ValidForSections");
                        m_ExtensionData = DB.RSField(rs, "ExtensionData");
                        m_CreatedOn = DB.RSFieldDateTime(rs, "CreatedOn");
                        m_GiftCardTypeID = DB.RSFieldInt(rs, "GiftCardTypeID");
                        m_DisabledByAdministrator = DB.RSFieldBool(rs, "DisabledByAdministrator");
                    }
                    else
                    {
                        Clear();
                    }
                }
            }
        }

        public void UpdateDB()
        {
            string sql = String.Format("update GiftCard set GiftCardGUID={1},SerialNumber={2},OrderNumber={3},PurchasedByCustomerID={4},ShoppingCartRecID={5},ProductID={6},VariantID={7},InitialAmount={8},Balance={9},ExpirationDate={10},IsEMailGiftCard={11},EMailName={12},EMailTo={13},EMailMessage={14},ValidForCustomers={15},ValidForProducts={16},ValidForManufacturers={17},ValidForCategories={18},ValidForSections={19},ExtensionData={20},GiftCardTypeID={21},DisabledByAdministrator={22} where GiftCardID={0}", GiftCardValues);
            DB.ExecuteSQL(sql);
        }

        public void SendGiftCardEmail()
        {
            if (this.GiftCardTypeID == (int)GiftCardTypes.EMailGiftCard)
            {
                Customer c = new Customer(this.PurchasedByCustomerID);
                AppLogic.SendMail(String.Format(AppLogic.GetString("giftcard.cs.1", 1, c.LocaleSetting), c.FullName(), AppLogic.AppConfig("StoreName")), AppLogic.RunXmlPackage(AppLogic.AppConfig("XmlPackage.EmailGiftCardNotification"), null, c, 1, "", "GiftCardID="+this.GiftCardID.ToString(), false, false), true, AppLogic.AppConfig("MailMe_FromAddress"), AppLogic.AppConfig("MailMe_FromName"), this.EMailTo, this.EMailName, "", AppLogic.MailServer());
            }
        }


        public void RefreshCard()
        {
            LoadFromDB(this.GiftCardID);
        }

        /// <summary>
        /// Adds an GiftCard to the GiftCard Table
        /// </summary>
        private void InsertDB()
        {
            string GiftCardGUID = CommonLogic.GetNewGUID();
            string sql = String.Format("insert into GiftCard(GiftCardGUID,PurchasedByCustomerID) values({0},{1})", DB.SQuote(GiftCardGUID), "0");
            DB.ExecuteSQL(sql);

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS(String.Format("select GiftCardID from GiftCard with (NOLOCK) where GiftCardGUID={0}", DB.SQuote(GiftCardGUID)), dbconn))
                {
                    rs.Read();
                    this.m_GiftCardID = DB.RSFieldInt(rs, "GiftCardID");
                    UpdateDB();
                }
            }
        }



        #region Public Properties
        public GiftCardTransactions GiftCardTransactions
        {
            get 
            {
                if (m_giftcardransactions == null)
                {
                    m_giftcardransactions = new GiftCardTransactions(m_GiftCardID);
                }
                return m_giftcardransactions; 
            }
        }

        public void Clear()
        {
            m_GiftCardID = 0;
            m_GiftCardGUID = String.Empty;
            m_SerialNumber = String.Empty;
            m_OrderNumber = 0;
            m_PurchasedByCustomerID = 0;
            m_ShoppingCartRecID = 0;
            m_ProductID = 0;
            m_VariantID = 0;
            m_GiftCardTypeID = 101;
            m_InitialAmount = System.Decimal.Zero;
            m_Balance = System.Decimal.Zero;
            m_ExpirationDate = System.DateTime.MinValue;
            m_EMailName = String.Empty;
            m_EMailTo = String.Empty;
            m_EMailMessage = String.Empty;
            m_ValidForCustomers = String.Empty;
            m_ValidForProducts = String.Empty;
            m_ValidForManufacturers = String.Empty;
            m_ValidForCategories = String.Empty;
            m_ValidForSections = String.Empty;
            m_ExtensionData = String.Empty;
            m_CreatedOn = System.DateTime.MinValue;
            m_DisabledByAdministrator = false;
        }


        /// <summary>
        /// Creates an array of GiftCard sql parameters that can be used by String.Format to build SQL statements.
        /// </summary>
        /// <returns>object[]</returns>
        private object[] GiftCardValues
        {
            get
            {
                object[] values = new object[] 
		          {
			        m_GiftCardID.ToString(),
                    DB.SQuote(m_GiftCardGUID),
                    DB.SQuote(m_SerialNumber),
                    m_OrderNumber.ToString(),
                    m_PurchasedByCustomerID.ToString(),
                    m_ShoppingCartRecID.ToString(),
                    m_ProductID.ToString(),
                    m_VariantID.ToString(),
                    Localization.CurrencyStringForDBWithoutExchangeRate(m_InitialAmount),
                    Localization.CurrencyStringForDBWithoutExchangeRate(m_Balance),
                    DB.SQuote(Localization.DateStringForDB(m_ExpirationDate)),
                    DB.SQuote(m_EMailName),
                    DB.SQuote(m_EMailTo),
                    DB.SQuote(m_EMailMessage),
                    DB.SQuote(m_ValidForCustomers),
                    DB.SQuote(m_ValidForProducts),
                    DB.SQuote(m_ValidForManufacturers),
                    DB.SQuote(m_ValidForCategories),
                    DB.SQuote(m_ValidForSections),
                    DB.SQuote(m_ExtensionData),
                    m_GiftCardTypeID.ToString(),
                    (m_DisabledByAdministrator ? 1 : 0).ToString()
		          };
                        return values;
            }
        }
        

        public int GiftCardID
        {
            get { return m_GiftCardID; }
        }

        public String GUID
        {
            get { return m_GiftCardGUID; }
        }

        public string SerialNumber
        {
            get { return m_SerialNumber; }
            set { m_SerialNumber = value; }
        }

        public int PurchasedByCustomerID
        {
            get { return m_PurchasedByCustomerID; }
            set { m_PurchasedByCustomerID = value; }
        }

        public int ShoppingCartRecID
        {
            get { return m_ShoppingCartRecID; }
            set { m_ShoppingCartRecID = value; }
        }

        public int OrderNumber
        {
            get { return m_OrderNumber; }
            set { m_OrderNumber = value; }
        }

        public int ProductID
        {
            get { return m_ProductID; }
            set { m_ProductID = value; }
        }

        public int VariantID
        {
            get { return m_VariantID; }
            set { m_VariantID = value; }
        }

        public decimal InitialAmount
        {
            get { return m_InitialAmount; }
            set { m_InitialAmount = value; }
        }

        public decimal Balance
        {
            get { return m_Balance; }
            set { m_Balance = value; }
        }

        public DateTime ExpirationDate
        {
            get { return m_ExpirationDate; }
            set { m_ExpirationDate = value; }
        }

        public string EMailName
        {
            get { return m_EMailName; }
            set { m_EMailName = value; }
        }

        public string EMailTo
        {
            get { return m_EMailTo; }
            set { m_EMailTo = value; }
        }

        public string EMailMessage
        {
            get { return m_EMailMessage; }
            set { m_EMailMessage = value; }
        }

        public string ValidForCustomers
        {
            get { return m_ValidForCustomers; }
            set { m_ValidForCustomers = value; }
        }

        public string ValidForProducts
        {
            get { return m_ValidForProducts; }
            set { m_ValidForProducts = value; }
        }

        public string ValidForManufacturers
        {
            get { return m_ValidForManufacturers; }
            set { m_ValidForManufacturers = value; }
        }

        public string ValidForCategories
        {
            get { return m_ValidForCategories; }
            set { m_ValidForCategories = value; }
        }

        public string ValidForSections
        {
            get { return m_ValidForSections; }
            set { m_ValidForSections = value; }
        }

        public string ExtensionData
        {
            get { return m_ExtensionData; }
            set { m_ExtensionData = value; }
        }

        public DateTime CreatedOn
        {
            get { return m_CreatedOn; }
            set { m_CreatedOn = value; }
        }

        public int GiftCardTypeID
        {
            get { return m_GiftCardTypeID; }
            set { m_GiftCardTypeID = value; }
        }

        public bool IsPhysicalGiftCard
        {
            get { return m_GiftCardTypeID == (int)GiftCardTypes.PhysicalGiftCard; }
        }

        public bool IsCertificateGiftCard
        {
            get { return m_GiftCardTypeID == (int)GiftCardTypes.CertificateGiftCard; }
        }

        public bool IsEMailGiftCard
        {
            get { return m_GiftCardTypeID == (int)GiftCardTypes.EMailGiftCard; }
        }

        public bool DisabledByAdministrator
        {
            get { return m_DisabledByAdministrator; }
            set { m_DisabledByAdministrator = value; }
        }
        #endregion

        // STATIC HELPER METHODS

        public static void ProcessOrder(Customer ActiveCustomer, int OrderNumber)
        {
            Order ord = new Order(OrderNumber, ActiveCustomer.LocaleSetting);
            bool UseLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");
            String StoreName = AppLogic.AppConfig("StoreName");
            String MailServer = AppLogic.MailServer();

            GiftCard card;

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("select * from giftcard where OrderNumber=" + OrderNumber.ToString(), dbconn))
                {
                    while (rs.Read())
                    {
                        card = new GiftCard();

                        card.PurchasedByCustomerID = DB.RSFieldInt(rs, "PurchasedByCustomerID");
                        card.ShoppingCartRecID = DB.RSFieldInt(rs, "ShoppingCartRecID");
                        card.ProductID = DB.RSFieldInt(rs, "ProductID");
                        card.VariantID = DB.RSFieldInt(rs, "VariantID");
                        card.EMailName = DB.RSField(rs, "EmailName");
                        card.EMailTo = DB.RSField(rs, "EmailTo");
                        card.EMailMessage = DB.RSField(rs, "EmailMessage");
                        card.InitialAmount = DB.RSFieldDecimal(rs, "InitialAmount");
                        card.Balance = DB.RSFieldDecimal(rs, "Balance");
                        card.ValidForProducts = DB.RSField(rs, "ValidForProducts");
                        card.ValidForSections = DB.RSField(rs, "ValidForSections");
                        card.ValidForManufacturers = DB.RSField(rs, "ValidForManufacturers");
                        card.ValidForCustomers = DB.RSField(rs, "ValidForCustomers");
                        card.ValidForCategories = DB.RSField(rs, "ValidForCategories");
                        card.GiftCardTypeID = DB.RSFieldInt(rs, "GiftCardTypeID");

                        card.OrderNumber = DB.RSFieldInt(rs, "OrderNumber");
                        card.SerialNumber = DB.RSField(rs, "SerialNumber");

                        card.DisabledByAdministrator = DB.RSFieldBool(rs, "DisabledByAdministrator");

                        String SubjectGiftCard = String.Empty;

                        if (UseLiveTransactions)
                        {
                            SubjectGiftCard = String.Format(AppLogic.GetString("giftcard.cs.1", ord.SkinID, ord.LocaleSetting), card.EMailName, StoreName);
                        }
                        else
                        {
                            SubjectGiftCard = String.Format(AppLogic.GetString("giftcard.cs.2", ord.SkinID, ord.LocaleSetting), card.EMailName, StoreName);
                        }
                        string CardBody = AppLogic.RunXmlPackage("notification.giftcard.html.xml.config", new Parser(ord.SkinID, ActiveCustomer), ActiveCustomer, ord.SkinID, String.Empty, "GiftCardID=" + card.GiftCardID.ToString(), true, true);
                        try
                        {
                            AppLogic.SendMail(SubjectGiftCard, CardBody, true, AppLogic.AppConfig("ReceiptEMailFrom"), AppLogic.AppConfig("ReceiptEMailFromName"), card.EMailTo, card.EMailName, String.Empty, MailServer);
                        }
                        catch { }
                    }
                }
            }
        }

        public static string s_GiftCardKey(int CustomerID)
        {
            string result = CustomerID.ToString().ToUpperInvariant();
            string ticks = DateTime.Now.Ticks.ToString().ToUpperInvariant();
            result += "-" + ticks.Substring(1, 5);
            result += "-" + ticks.Substring(6, 5);
            result += "-" + ticks.Substring(11, 5);
            return result;
        }
        
        public static bool s_IsGiftCard(int ProductID)
        {
            string GiftIDs = AppLogic.AppConfig("GiftCard.PhysicalProductTypeIDs").Trim() + "," + AppLogic.AppConfig("GiftCard.EmailProductTypeIDs").Trim() + "," + AppLogic.AppConfig("GiftCard.CertificateProductTypeIDs").Trim();
            return (0 != DB.GetSqlN("select count(*) as N from Product where ProductID=" + ProductID.ToString() + " and ProductTypeID in (" + GiftIDs + ")"));
        }

        public static bool s_IsPhysicalGiftCard(int ProductID)
        {
            return (0 != DB.GetSqlN("select count(*) as N from Product where ProductID=" + ProductID.ToString() + " and ProductTypeID in (" + AppLogic.AppConfig("GiftCard.PhysicalProductTypeIDs").Trim() + ")"));
        }

        public static bool s_IsEmailGiftCard(int ProductID)
        {
            return (0 != DB.GetSqlN("select count(*) as N from Product where ProductID=" + ProductID.ToString() + " and ProductTypeID in (" + AppLogic.AppConfig("GiftCard.EmailProductTypeIDs").Trim() + ")"));
        }

        public static bool s_IsCertificateGiftCard(int ProductID)
        {
            return (0 != DB.GetSqlN("select count(*) as N from Product where ProductID=" + ProductID.ToString() + " and ProductTypeID in (" + AppLogic.AppConfig("GiftCard.CertificateProductTypeIDs").Trim() + ")"));
        }

        public static string s_GetCardType(int type)
        {
            string temp = "";

            switch(type)
            {
                case 100:
                    temp = "Physical";
                    break;
                case 101:
                    temp = "E-Mail";
                    break;
                case 102:
                    temp = "Certificate";
                    break;
                default:
                    temp = "Other";
                    break;
            }

            return temp;
        }
    }


    public enum GiftCardCollectionFilterType:int
    {
        ShoppingCartID = 1, 
        OrderNumber = 2,
        PurchasingCustomerID = 3,
        UsingCustomerID = 4
    }

    public class GiftCards : IEnumerable
    {
        public ArrayList m_GiftCards;

        public GiftCards(int ID, GiftCardCollectionFilterType IDType)
        {
            m_GiftCards = new ArrayList();
            string sql = "select GiftCardID FROM dbo.GiftCard ";
            switch (IDType)
            {
                case GiftCardCollectionFilterType.ShoppingCartID:
                    sql += "WHERE ShoppingCartRecID = " + ID.ToString();
                    break;
                case GiftCardCollectionFilterType.OrderNumber:
                    sql += "WHERE OrderNumber = " + ID.ToString();
                    break;
                case GiftCardCollectionFilterType.PurchasingCustomerID:
                    sql += "WHERE PurchasedByCustomerID = " + ID.ToString();
                    break;
                case GiftCardCollectionFilterType.UsingCustomerID:
                    sql = "select distinct GiftCard.GiftCardID FROM dbo.GiftCard join dbo.GiftCardUsage on GiftCard.GiftCardID = GiftCardUsage.GiftCardID WHERE GiftCardUsage.UsedByCustomerID = " + ID.ToString() + " and GiftCard.Balance > 0 and GiftCard.DisabledByAdministrator = 0";
                    break;
            }

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader dr = DB.GetRS(sql, dbconn))
                {
                    while (dr.Read())
                    {
                        this.Add(new GiftCard(dr.GetInt32(0)));
                    }
                }
            }
        }


        public void Add(GiftCard giftcard)
        {
            m_GiftCards.Add(giftcard);
        }

        public void Remove(GiftCard giftcard)
        {
            m_GiftCards.Remove(giftcard);
        }

        public int Count
        {
            get { return m_GiftCards.Count; }
        }

        public IEnumerator GetEnumerator()
        {
            return new GiftCardsEnumerator(this);
        }
    }


    public class GiftCardsEnumerator : IEnumerator
    {
	    private int position = -1;
        private GiftCards giftcards;

        public GiftCardsEnumerator(GiftCards giftcards)
	    {
            this.giftcards = giftcards;
	    }

	    public bool MoveNext()
	    {
            if (position < giftcards.m_GiftCards.Count - 1)
		    {
			    position++;
			    return true;
		    }
		    else
		    {
			    return false;
		    }
	    }

	    public void Reset()
	    {
		    position = -1;
	    }

	    public object Current
	    {
		    get
		    {
                return giftcards.m_GiftCards[position];
		    }
	    }
    }

    public class GiftCardUsageTransaction
    {
        private int m_Giftcardusageid;
        private string m_Giftcardusageguid;
        private int m_Giftcardid;
        private GiftCardUsageReasons m_UsageType;
        private int m_Usedbycustomerid;
        private int m_Ordernumber;
        private decimal m_Amount;
        private string m_Extensiondata;
        private DateTime m_Createdon;


        GiftCardUsageTransaction(int GiftCardTransactionID)
        {

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader dr = DB.GetRS("aspdnsf_getGiftCardUsage " + GiftCardTransactionID.ToString(), dbconn))
                {
                    if (dr.Read())
                    {
                        m_Giftcardusageid = DB.RSFieldInt(dr, "GiftCardUsageID");
                        m_Giftcardusageguid = DB.RSFieldGUID(dr, "GiftCardUsageGUID");
                        m_Giftcardid = DB.RSFieldInt(dr, "GiftCardID");
                        m_UsageType = (GiftCardUsageReasons)Enum.Parse(typeof(GiftCardUsageReasons), DB.RSFieldInt(dr, "UsageTypeID").ToString());
                        m_Usedbycustomerid = DB.RSFieldInt(dr, "UsedByCustomerID");
                        m_Ordernumber = DB.RSFieldInt(dr, "OrderNumber");
                        m_Amount = DB.RSFieldDecimal(dr, "Amount");
                        m_Extensiondata = DB.RSField(dr, "ExtensionData");
                        m_Createdon = DB.RSFieldDateTime(dr, "CreatedOn");
                    }
                }
            }
        }

        public GiftCardUsageTransaction(int GiftCardUsageID, string GiftCardUsageGUID, int GiftCardID, int UsageTypeID, int UsedByCustomerID, int OrderNumber, decimal Amount, string ExtensionData, DateTime CreatedOn)
        {
            m_Giftcardusageid = GiftCardUsageID;
            m_Giftcardusageguid = GiftCardUsageGUID;
            m_Giftcardid = GiftCardID;
            m_UsageType = (GiftCardUsageReasons)Enum.Parse(typeof(GiftCardUsageReasons), UsageTypeID.ToString());
            m_Usedbycustomerid = UsedByCustomerID;
            m_Ordernumber = OrderNumber;
            m_Amount = Amount;
            m_Extensiondata = ExtensionData;
            m_Createdon = CreatedOn;

        }

        public static GiftCardUsageTransaction CreateTransaction(int GiftCardID, GiftCardUsageReasons UsageReason, int UsedByCustomerID, int OrderNumber, decimal Amount, string ExtensionData)
        {

            int GiftCardUsageID = 0;

            string err = String.Empty;
            SqlConnection cn = new SqlConnection(DB.GetDBConn());
            cn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = cn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "dbo.aspdnsf_insGiftCardUsage";

            cmd.Parameters.Add(new SqlParameter("@GiftCardID", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@UsageTypeID", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@UsedByCustomerID", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@OrderNumber", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@Amount", SqlDbType.Decimal, 8));
            cmd.Parameters.Add(new SqlParameter("@ExtensionData", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@GiftCardUsageID", SqlDbType.Int, 4)).Direction = ParameterDirection.Output;

            cmd.Parameters["@GiftCardID"].Value = GiftCardID;
            cmd.Parameters["@UsageTypeID"].Value = (int)UsageReason;
            cmd.Parameters["@UsedByCustomerID"].Value = UsedByCustomerID;
            cmd.Parameters["@OrderNumber"].Value = OrderNumber;
            cmd.Parameters["@Amount"].Value = Amount;
            cmd.Parameters["@ExtensionData"].Value = ExtensionData;


            try
            {
                cmd.ExecuteNonQuery();
                GiftCardUsageID = Int32.Parse(cmd.Parameters["@GiftCardUsageID"].Value.ToString());
            }
            catch (Exception ex)
            {
                err = ex.Message;
            }

            cn.Close();
            cmd.Dispose();
            cn.Dispose();

            if (GiftCardID > 0)
            {
                GiftCardUsageTransaction g = new GiftCardUsageTransaction(GiftCardUsageID);
                return g;
            }
            return null;
        }

        public int GiftCardUsageID
        {
            get { return m_Giftcardusageid; }
        }

        public string GiftCardUsageGUID
        {
            get { return m_Giftcardusageguid; }
        }

        public int GiftCardID
        {
            get { return m_Giftcardid; }
        }

        public GiftCardUsageReasons UsageTypeID
        {
            get { return m_UsageType; }
        }

        public int UsedByCustomerID
        {
            get { return m_Usedbycustomerid; }
        }

        public int OrderNumber
        {
            get { return m_Ordernumber; }
        }

        public decimal Amount
        {
            get { return m_Amount; }
        }

        public string ExtensionData
        {
            get { return m_Extensiondata; }
        }

        public DateTime CreatedOn
        {
            get { return m_Createdon; }
        }

    }

    public class GiftCardTransactions : IEnumerable
    {
        public ArrayList m_Transactions;
        public GiftCardTransactions(int GiftCardID)
	    {
            m_Transactions = new ArrayList();

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader dr = DB.GetRS("aspdnsf_getGiftCardUsage " + GiftCardID.ToString(), dbconn))
                {
                    while (dr.Read())
                    {
                        this.Add(new GiftCardUsageTransaction(DB.RSFieldInt(dr, "GiftCardUsageID"), DB.RSFieldGUID(dr, "GiftCardUsageGUID"), DB.RSFieldInt(dr, "GiftCardID"), DB.RSFieldInt(dr, "UsageTypeID"), DB.RSFieldInt(dr, "UsedByCustomerID"), DB.RSFieldInt(dr, "OrderNumber"), DB.RSFieldDecimal(dr, "Amount"), DB.RSField(dr, "ExtensionData"), DB.RSFieldDateTime(dr, "CreatedOn")));
                    }
                }
            }
	    }

        public void Add(GiftCardUsageTransaction transaction)
	    {
            m_Transactions.Add(transaction);
	    }

        public void Remove(GiftCardUsageTransaction transaction)
	    {
            m_Transactions.Remove(transaction);
	    }

        public int Count 
        {
            get { return m_Transactions.Count; }
        }

        public IEnumerator GetEnumerator()
        {
            return new GiftCardTransactionsEnumerator(this);
        }
    }

    class GiftCardTransactionsEnumerator:IEnumerator
    {
	    private int position = -1;
        private GiftCardTransactions transactions;

        public GiftCardTransactionsEnumerator(GiftCardTransactions transactions)
	    {
            this.transactions = transactions;
	    }

	    public bool MoveNext()
	    {
            if (position < transactions.m_Transactions.Count - 1)
		    {
			    position++;
			    return true;
		    }
		    else
		    {
			    return false;
		    }
	    }

	    public void Reset()
	    {
		    position = -1;
	    }

	    public object Current
	    {
		    get
		    {
                return transactions.m_Transactions[position];
		    }
	    }
    }
}
