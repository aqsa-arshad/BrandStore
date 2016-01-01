// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Xml.Serialization;

namespace AspDotNetStorefrontCore
{
    public enum VATSettingEnum
    {
        UnknownValue = 0,
        ShowPricesInclusiveOfVAT = 1,
        ShowPricesExclusiveOfVAT = 2
    }

    /// <summary>
    /// Customer object info is rebuilt off of session["CustomerID"]!!!! This is created in AppLogic.SessionStart or other places
    /// this is the ONLY data stored in session and is server farm safe
    /// </summary>
    [Serializable]
    public class Customer : IIdentity
    {

        // referrer is handled specially (to get the very first one!)
        /// <summary>
        /// The ReadOnly String for getting the string "Referrer";
        /// </summary>
        public static readonly String ro_ReferrerCookieName = "Referrer";

        #region Private Variables

        private int m_CustomerID;
        private bool m_HasCustomerRecord;
        private String m_CustomerGUID = String.Empty;
        private int m_CustomerLevelID;
        private String m_CustomerLevelName;
        private int m_AffiliateID;
        private String m_LocaleSetting;
        private String m_CurrencySetting;
        private VATSettingEnum m_VATSetting;
        private String m_VATRegistrationID;
        private String m_Phone;
        private String m_EMail;
        private bool m_OKToEMail;
        private bool m_CODCompanyCheckAllowed;
        private bool m_CODNet30Allowed;
        private String m_GiftRegistryGUID;
        private bool m_GiftRegistryIsAnonymous;
        private bool m_GiftRegistryAllowSearchByOthers;
        private String m_GiftRegistryNickName;
        private bool m_GiftRegistryHideShippingAddresses;
        private String m_Password; // salted and hashed (on retrieval from the db)
        private int m_SaltKey;
        private bool m_IsRegistered;
        private bool m_IsAdminUser;
        private bool m_IsAdminSuperUser;
        private decimal m_MicroPayBalance;
        private String m_FirstName;
        private String m_LastName;
        private String m_Notes;
        private String m_LastIPAddress;
        private bool m_SuppressCookies;
        private int m_RecurringShippingMethodID;
        private String m_RecurringShippingMethod;
        private DateTime m_SubscriptionExpiresOn;
        private int m_PrimaryBillingAddressID;
        private int m_PrimaryShippingAddressID;
        private int m_SkinID;
        private decimal m_LevelDiscountPct;
        private bool m_DiscountExtendedPrices;
        private string m_Roles = string.Empty;
        private int m_CurrentSessionID;
        private bool m_StoreCCInDB;
        private DateTime m_LockedUntil;
        private bool m_AdminCanViewCC;
        private DateTime m_LastActivity;
        private DateTime m_PwdChanged;
        private byte m_BadLoginCount;
        private DateTime m_LastBadLogin;
        private bool m_Active;
        private DateTime m_DateOfBirth;
        private bool m_Over13;
        private bool m_PwdChgRequired;
        private String m_CouponCode;
        private CustomerSession m_CustomerSession;
        private Address m_PrimaryBillingAddress;
        private Address m_PrimaryShippingAddress;
        private string m_RequestedPaymentMethod;
        private DateTime m_CreatedOn;
        SqlTransaction m_DBTrans = null;

        private bool m_DefaultCustLevel_DiscountExtendedPrices;
        private decimal m_DefaultCustLevel_LevelDiscountPct;
        private string m_DefaultCustLevel_CustomerLevelName;
        private int m_DefaultCustLevel_CustomerLevelID
        {
            get
            {
                if (AppLogic.AppConfigUSInt("DefaultCustomerLevelID") > 0)
                {
                    return AppLogic.AppConfigUSInt("DefaultCustomerLevelID");
                }
                else
                {
                    return 0;
                }
            }
        }

        private int m_FailedTransactionCount;


        private bool m_UseDBDataOnly = false;

        #endregion

        /// <summary>
        /// The ReadOnly String for getting the string "LocaleSetting";
        /// </summary>
        public static readonly String ro_LocaleSettingCookieName = "LocaleSetting";
        /// <summary>
        /// The ReadOnly String for getting the string "CurrencySetting";
        /// </summary>
        public static readonly String ro_CurrencySettingCookieName = "CurrencySetting";
        /// <summary>
        /// The ReadOnly String for getting the string "AffiliateID";
        /// </summary>
        public static readonly String ro_AffiliateCookieName = "AffiliateID";
        /// <summary>
        /// The ReadOnly String for getting the string "VATSettingID";
        /// </summary>
        public static readonly String ro_VATSettingCookieName = "VATSettingID";

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Customer"/> class.
        /// </summary>
        /// <param name="CustomerID">The customerID.</param>
        public Customer(int CustomerID)
            : this(CustomerID, false)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Customer"/> class.
        /// </summary>
        /// <param name="SuppressCookies">if set to <c>true</c> [suppress cookies].</param>
        public Customer(bool SuppressCookies)
            : this(0, SuppressCookies)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Customer"/> class.
        /// </summary>
        /// <param name="CustomerID">The customerID.</param>
        /// <param name="SuppressCookies">if set to <c>true</c> [suppress cookies].</param>
        public Customer(int CustomerID, bool SuppressCookies)
            : this(CustomerID, SuppressCookies, false)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Customer"/> class.
        /// </summary>
        /// <param name="CustomerID">The customerID.</param>
        /// <param name="SuppressCookies">if set to <c>true</c> [suppress cookies].</param>
        /// <param name="UseDBDataOnly">if set to <c>true</c> [use DB data only].</param>
        public Customer(int CustomerID, bool SuppressCookies, bool UseDBDataOnly)
            : this(null, CustomerID, SuppressCookies, UseDBDataOnly)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Customer"/> class.
        /// </summary>
        /// <param name="CustomerGUID">The customer GUID.</param>
        public Customer(Guid CustomerGUID)
            : this(CustomerGUID, false)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Customer"/> class.
        /// </summary>
        /// <param name="CustomerGUID">The customer GUID.</param>
        /// <param name="SuppressCookies">if set to <c>true</c> [suppress cookies].</param>
        public Customer(Guid CustomerGUID, bool SuppressCookies)
            : this(CustomerGUID, SuppressCookies, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Customer"/> class.
        /// </summary>
        /// <param name="CustomerGUID">The customer GUID.</param>
        /// <param name="SuppressCookies">if set to <c>true</c> [suppress cookies].</param>
        /// <param name="UseDBDataOnly">if set to <c>true</c> [use DB data only].</param>
        public Customer(Guid CustomerGUID, bool SuppressCookies, bool UseDBDataOnly)
            : this(null, CustomerGUID, SuppressCookies, UseDBDataOnly)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Customer"/> class.
        /// </summary>
        /// <param name="email">The email.</param>
        public Customer(string email)
            : this(email, false)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Customer"/> class.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="SuppressCookies">if set to <c>true</c> [suppress cookies].</param>
        public Customer(string email, bool SuppressCookies)
            : this(email, SuppressCookies, false)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Customer"/> class.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="SuppressCookies">if set to <c>true</c> [suppress cookies].</param>
        /// <param name="UseDBDataOnly">if set to <c>true</c> [use DB data only].</param>
        public Customer(string email, bool SuppressCookies, bool UseDBDataOnly)
            : this(null, email, SuppressCookies, UseDBDataOnly)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Customer"/> class.
        /// </summary>
        /// <param name="DBTrans">The DataBase transaction.</param>
        /// <param name="CustomerID">The customer ID.</param>
        public Customer(SqlTransaction DBTrans, int CustomerID)
            : this(DBTrans, CustomerID, false)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Customer"/> class.
        /// </summary>
        /// <param name="DBTrans">The DataBase transaction.</param>
        /// <param name="SuppressCookies">if set to <c>true</c> [suppress cookies].</param>
        public Customer(SqlTransaction DBTrans, bool SuppressCookies)
            : this(DBTrans, 0, SuppressCookies)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Customer"/> class.
        /// </summary>
        /// <param name="DBTrans">The DataBase trans.</param>
        /// <param name="CustomerID">The customer ID.</param>
        /// <param name="SuppressCookies">if set to <c>true</c> [suppress cookies].</param>
        public Customer(SqlTransaction DBTrans, int CustomerID, bool SuppressCookies)
            : this(DBTrans, CustomerID, SuppressCookies, false)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Customer"/> class.
        /// </summary>
        /// <param name="DBTrans">The DataBase trans.</param>
        /// <param name="CustomerID">The customer ID.</param>
        /// <param name="SuppressCookies">if set to <c>true</c> [suppress cookies].</param>
        /// <param name="UseDBDataOnly">if set to <c>true</c> [use DB data only].</param>
        public Customer(SqlTransaction DBTrans, int CustomerID, bool SuppressCookies, bool UseDBDataOnly)
        {
            m_DBTrans = DBTrans;
            m_UseDBDataOnly = UseDBDataOnly;
            m_SuppressCookies = SuppressCookies;
            Init(CustomerID);
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="Customer"/> class.
        /// </summary>
        /// <param name="DBTrans">The DataBase trans.</param>
        /// <param name="CustomerGUID">The customer GUID.</param>
        public Customer(SqlTransaction DBTrans, Guid CustomerGUID)
            : this(DBTrans, CustomerGUID, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Customer"/> class.
        /// </summary>
        /// <param name="DBTrans">The DataBase trans.</param>
        /// <param name="CustomerGUID">The customer GUID.</param>
        /// <param name="SuppressCookies">if set to <c>true</c> [suppress cookies].</param>
        public Customer(SqlTransaction DBTrans, Guid CustomerGUID, bool SuppressCookies)
            : this(DBTrans, CustomerGUID, SuppressCookies, false)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Customer"/> class.
        /// </summary>
        /// <param name="DBTrans">The DataBase trans.</param>
        /// <param name="CustomerGUID">The customer GUID.</param>
        /// <param name="SuppressCookies">if set to <c>true</c> [suppress cookies].</param>
        /// <param name="UseDBDataOnly">if set to <c>true</c> [use DB data only].</param>
        public Customer(SqlTransaction DBTrans, Guid CustomerGUID, bool SuppressCookies, bool UseDBDataOnly)
        {
            m_DBTrans = DBTrans;
            m_UseDBDataOnly = UseDBDataOnly;
            m_SuppressCookies = SuppressCookies;
            Init(CustomerGUID);
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="Customer"/> class.
        /// </summary>
        /// <param name="DBTrans">The DataBase transaction.</param>
        /// <param name="email">The email.</param>
        public Customer(SqlTransaction DBTrans, string email)
            : this(DBTrans, email, false)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Customer"/> class.
        /// </summary>
        /// <param name="DBTrans">The DataBase transaction.</param>
        /// <param name="email">The email.</param>
        /// <param name="SuppressCookies">if set to <c>true</c> [suppress cookies].</param>
        public Customer(SqlTransaction DBTrans, string email, bool SuppressCookies)
            : this(DBTrans, email, SuppressCookies, false)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Customer"/> class.
        /// </summary>
        /// <param name="DBTrans">The Database transaction.</param>
        /// <param name="email">The email.</param>
        /// <param name="SuppressCookies">if set to <c>true</c> [suppress cookies].</param>
        /// <param name="UseDBDataOnly">if set to <c>true</c> [use DB data only].</param>
        public Customer(SqlTransaction DBTrans, string email, bool SuppressCookies, bool UseDBDataOnly)
            : this(DBTrans, email, SuppressCookies, UseDBDataOnly, false)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Customer"/> class.
        /// </summary>
        /// <param name="DBTrans">The Database transaction.</param>
        /// <param name="email">The email.</param>
        /// <param name="SuppressCookies">if set to <c>true</c> [suppress cookies].</param>
        /// <param name="UseDBDataOnly">if set to <c>true</c> [use DB data only].</param>
        public Customer(SqlTransaction DBTrans, string email, bool SuppressCookies, bool UseDBDataOnly, bool AdminOnly)
        {
            m_DBTrans = DBTrans;
            m_UseDBDataOnly = UseDBDataOnly;
            m_SuppressCookies = SuppressCookies;
            Init(email, AdminOnly);
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Sets the primary shipping address for shopping cart.
        /// </summary>
        /// <param name="CustomerID">The customer ID.</param>
        /// <param name="OldPrimaryShippingAddressID">The old primary shipping address ID.</param>
        /// <param name="NewPrimaryShippingAddressID">The new primary shipping address ID.</param>
        static public void SetPrimaryShippingAddressForShoppingCart(int CustomerID, int OldPrimaryShippingAddressID, int NewPrimaryShippingAddressID)
        {
            DB.ExecuteSQL("update shoppingcart set ShippingAddressID=" + NewPrimaryShippingAddressID.ToString() + " where ShippingAddressID=" + OldPrimaryShippingAddressID.ToString() + " and CustomerID=" + CustomerID.ToString() + " and CartType in (" + ((int)CartTypeEnum.ShoppingCart).ToString() + "," + ((int)CartTypeEnum.RecurringCart).ToString() + ")");
        }

        /// <summary>
        /// Creates the customer record.
        /// </summary>
        /// <param name="Email">The email.</param>
        /// <param name="Password">The password.</param>
        /// <param name="SkinID">The skin ID.</param>
        /// <param name="AffiliateID">The affiliate ID.</param>
        /// <param name="Referrer">The referrer.</param>
        /// <param name="IsAdmin">The is admin.</param>
        /// <param name="LastIPAddress">The last IP address.</param>
        /// <param name="LocaleSetting">The locale setting.</param>
        /// <param name="Over13Checked">Is over13 checked.</param>
        /// <param name="CurrencySetting">The currency setting.</param>
        /// <param name="VATSetting">The VAT setting.</param>
        /// <param name="VATRegistrationID">The VAT registration ID.</param>
        /// <param name="CustomerLevelID">The customer level ID.</param>
        /// <returns></returns>
        static public int CreateCustomerRecord(string Email, string Password, object SkinID, object AffiliateID, string Referrer, object IsAdmin, string LastIPAddress, string LocaleSetting, object Over13Checked, string CurrencySetting, object VATSetting, String VATRegistrationID, object CustomerLevelID)
        {
            int CustomerID = 0;


            SqlConnection cn = new SqlConnection(DB.GetDBConn());
            cn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = cn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "dbo.aspdnsf_insCustomer";

            cmd.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@Password", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@SkinID", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@AffiliateID", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@Referrer", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@IsAdmin", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@LastIPAddress", SqlDbType.VarChar, 40));
            cmd.Parameters.Add(new SqlParameter("@LocaleSetting", SqlDbType.NVarChar, 20));
            cmd.Parameters.Add(new SqlParameter("@Over13Checked", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@CurrencySetting", SqlDbType.NVarChar, 20));
            cmd.Parameters.Add(new SqlParameter("@VATSetting", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@VATRegistrationID", SqlDbType.NVarChar, 100));
            cmd.Parameters.Add(new SqlParameter("@CustomerLevelID", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@CustomerID", SqlDbType.Int, 4)).Direction = ParameterDirection.Output;

            if (Email == null)
            {
                cmd.Parameters["@Email"].Value = DBNull.Value;
            }
            else
            {
                cmd.Parameters["@Email"].Value = Email;
            }

            if (Password == null)
            {
                cmd.Parameters["@Password"].Value = DBNull.Value;
            }
            else
            {
                cmd.Parameters["@Password"].Value = Password;
            }

            if (SkinID == null)
            {
                cmd.Parameters["@SkinID"].Value = DBNull.Value;
            }
            else
            {
                cmd.Parameters["@SkinID"].Value = SkinID;
            }

            if (AffiliateID == null)
            {
                cmd.Parameters["@AffiliateID"].Value = DBNull.Value;
            }
            else
            {
                cmd.Parameters["@AffiliateID"].Value = AffiliateID;
            }

            if (CustomerLevelID == null)
            {
                cmd.Parameters["@CustomerLevelID"].Value = 0;//JH soft default customer level should apply when 0
            }
            else
            {
                cmd.Parameters["@CustomerLevelID"].Value = CustomerLevelID;
            }


            if (Referrer == null)
            {
                cmd.Parameters["@Referrer"].Value = DBNull.Value;
            }
            else
            {
                cmd.Parameters["@Referrer"].Value = Referrer;
            }

            if (IsAdmin == null)
            {
                cmd.Parameters["@IsAdmin"].Value = DBNull.Value;
            }
            else
            {
                cmd.Parameters["@IsAdmin"].Value = IsAdmin;
            }

            if (LastIPAddress == null)
            {
                cmd.Parameters["@LastIPAddress"].Value = DBNull.Value;
            }
            else
            {
                cmd.Parameters["@LastIPAddress"].Value = LastIPAddress;
            }

            if (LocaleSetting == null)
            {
                cmd.Parameters["@LocaleSetting"].Value = DBNull.Value;
            }
            else
            {
                cmd.Parameters["@LocaleSetting"].Value = LocaleSetting;
            }

            if (Over13Checked == null)
            {
                cmd.Parameters["@Over13Checked"].Value = DBNull.Value;
            }
            else
            {
                cmd.Parameters["@Over13Checked"].Value = Over13Checked;
            }

            if (CurrencySetting == null)
            {
                cmd.Parameters["@CurrencySetting"].Value = DBNull.Value;
            }
            else
            {
                cmd.Parameters["@CurrencySetting"].Value = CurrencySetting;
            }

            if (VATSetting == null)
            {
                cmd.Parameters["@VATSetting"].Value = DBNull.Value;
            }
            else
            {
                cmd.Parameters["@VATSetting"].Value = VATSetting;
            }

            if (VATRegistrationID == null)
            {
                cmd.Parameters["@VATRegistrationID"].Value = "";
            }
            else
            {
                cmd.Parameters["@VATRegistrationID"].Value = VATRegistrationID;
            }

            try
            {
                cmd.ExecuteNonQuery();
                CustomerID = Int32.Parse(cmd.Parameters["@CustomerID"].Value.ToString());
            }
            catch { }

            cn.Close();
            cmd.Dispose();
            cn.Dispose();

            return CustomerID;
        }

        public static bool EmailInUse(string email, int CustomerID, Boolean ExcludeUnregisteredUsers)
        {
            return (email != "" && DB.GetSqlN(string.Format("select count(*) as N from customer   with (NOLOCK)  where EMail = {0} and CustomerID <> {1} and ({2} = 0 or StoreID = {3}) {4}", DB.SQuote(email), CustomerID, CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowCustomerFiltering") == true, 1, 0), AppLogic.StoreID(), CommonLogic.IIF(ExcludeUnregisteredUsers, "and IsRegistered > 0", String.Empty))) > 0);
        }

        public static bool NewEmailPassesDuplicationRules(string email, int CustomerID, Boolean IsAnonymous)
        {
            if (AppLogic.GlobalConfigBool("AllowCustomerDuplicateEMailAddresses"))
                return true;

            if (AppLogic.GlobalConfigBool("Anonymous.AllowAlreadyRegisteredEmail") && IsAnonymous)
                return true;

            return !Customer.EmailInUse(email, CustomerID, true);
        }

        /// <summary>
        /// Updates the customer static.
        /// </summary>
        /// <param name="CustomerID">The customerID.</param>
        /// <param name="CustomerLevelID">The customerlevelID.</param>
        /// <param name="Email">The email.</param>
        /// <param name="SaltedAndHashedPassword">The salted and hashed password.</param>
        /// <param name="SaltKey">The salt key.</param>
        /// <param name="DateOfBirth">The date of birth.</param>
        /// <param name="Gender">The gender.</param>
        /// <param name="FirstName">The first name.</param>
        /// <param name="LastName">The last name.</param>
        /// <param name="Notes">The notes.</param>
        /// <param name="SkinID">The skin ID.</param>
        /// <param name="Phone">The phone.</param>
        /// <param name="AffiliateID">The affiliate ID.</param>
        /// <param name="Referrer">The referrer.</param>
        /// <param name="CouponCode">The coupon code.</param>
        /// <param name="OkToEmail">The ok to email.</param>
        /// <param name="IsAdmin">The is admin.</param>
        /// <param name="BillingEqualsShipping">The billing equals shipping.</param>
        /// <param name="LastIPAddress">The last IP address.</param>
        /// <param name="OrderNotes">The order notes.</param>
        /// <param name="SubscriptionExpiresOn">The subscription expires on.</param>
        /// <param name="RTShipRequest">The RT ship request.</param>
        /// <param name="RTShipResponse">The RT ship response.</param>
        /// <param name="OrderOptions">The order options.</param>
        /// <param name="LocaleSetting">The locale setting.</param>
        /// <param name="MicroPayBalance">The micro pay balance.</param>
        /// <param name="RecurringShippingMethodID">The recurring shipping method ID.</param>
        /// <param name="RecurringShippingMethod">The recurring shipping method.</param>
        /// <param name="BillingAddressID">The billing address ID.</param>
        /// <param name="ShippingAddressID">The shipping address ID.</param>
        /// <param name="GiftRegistryGUID">The gift registry GUID.</param>
        /// <param name="GiftRegistryIsAnonymous">The gift registry is anonymous.</param>
        /// <param name="GiftRegistryAllowSearchByOthers">The gift registry allow search by others.</param>
        /// <param name="GiftRegistryNickName">Name of the gift registry nick.</param>
        /// <param name="GiftRegistryHideShippingAddresses">The gift registry hide shipping addresses.</param>
        /// <param name="CODCompanyCheckAllowed">The COD company check allowed.</param>
        /// <param name="CODNet30Allowed">The COD net30 allowed.</param>
        /// <param name="ExtensionData">The extension data.</param>
        /// <param name="FinalizationData">The finalization data.</param>
        /// <param name="Deleted">Is deleted.</param>
        /// <param name="Over13Checked">Is over13 checked.</param>
        /// <param name="CurrencySetting">The currency setting.</param>
        /// <param name="VATSetting">The VAT setting.</param>
        /// <param name="VATRegistrationID">The VAT registration ID.</param>
        /// <param name="StoreCCInDB">The store CC in DB.</param>
        /// <param name="IsRegistered">The is registered.</param>
        /// <param name="LockedUntil">Is locked until.</param>
        /// <param name="AdminCanViewCC">The admin can view CC.</param>
        /// <param name="BadLogin">The bad login.</param>
        /// <param name="Active">Is active.</param>
        /// <param name="PwdChangeRequired">The Password change required.</param>
        /// <param name="RegisterDate">The register date.</param>
        /// <returns></returns>
        public static string UpdateCustomerStatic(int CustomerID, object CustomerLevelID, string Email, string SaltedAndHashedPassword, object SaltKey, string DateOfBirth, string Gender, string FirstName, string LastName, string Notes, object SkinID, string Phone, object AffiliateID, string Referrer, string CouponCode, object OkToEmail, object IsAdmin, object BillingEqualsShipping, string LastIPAddress, string OrderNotes, string SubscriptionExpiresOn, string RTShipRequest, string RTShipResponse, string OrderOptions, string LocaleSetting, object MicroPayBalance, object RecurringShippingMethodID, string RecurringShippingMethod, object BillingAddressID, object ShippingAddressID, string GiftRegistryGUID, object GiftRegistryIsAnonymous, object GiftRegistryAllowSearchByOthers, string GiftRegistryNickName, object GiftRegistryHideShippingAddresses, object CODCompanyCheckAllowed, object CODNet30Allowed, string ExtensionData, string FinalizationData, object Deleted, object Over13Checked, string CurrencySetting, object VATSetting, String VATRegistrationID, object StoreCCInDB, object IsRegistered, object LockedUntil, object AdminCanViewCC, object BadLogin, object Active, object PwdChangeRequired, object RegisterDate)
        {

            AppLogic.eventHandler("UpdateCustomer").CallEvent("&UpdateCustomer=true&UpdatedCustomerID=" + CustomerID.ToString());

            string err = String.Empty;
            SqlConnection cn = new SqlConnection(DB.GetDBConn());
            cn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = cn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "dbo.aspdnsf_updCustomer";

            if (SaltKey != null && (int)SaltKey == Security.ro_SaltKeyIsInvalid)
            {
                SaltKey = null;
            }

            cmd.Parameters.Add(new SqlParameter("@CustomerID", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@RegisterDate", SqlDbType.VarChar, 30));
            cmd.Parameters.Add(new SqlParameter("@CustomerLevelID", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@Password", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@SaltKey", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@DateOfBirth", SqlDbType.VarChar, 30));
            cmd.Parameters.Add(new SqlParameter("@Gender", SqlDbType.NVarChar, 2));
            cmd.Parameters.Add(new SqlParameter("@FirstName", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@LastName", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@Notes", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@SkinID", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@Phone", SqlDbType.NVarChar, 50));
            cmd.Parameters.Add(new SqlParameter("@AffiliateID", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@Referrer", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@CouponCode", SqlDbType.NVarChar, 100));
            cmd.Parameters.Add(new SqlParameter("@OkToEmail", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@IsAdmin", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@BillingEqualsShipping", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@LastIPAddress", SqlDbType.VarChar, 40));
            cmd.Parameters.Add(new SqlParameter("@OrderNotes", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@SubscriptionExpiresOn", SqlDbType.VarChar, 30));
            cmd.Parameters.Add(new SqlParameter("@RTShipRequest", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@RTShipResponse", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@OrderOptions", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@LocaleSetting", SqlDbType.NVarChar, 20));
            cmd.Parameters.Add(new SqlParameter("@MicroPayBalance", SqlDbType.Decimal, 8));
            cmd.Parameters.Add(new SqlParameter("@RecurringShippingMethodID", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@RecurringShippingMethod", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@BillingAddressID", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@ShippingAddressID", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@GiftRegistryGUID", SqlDbType.VarChar, 40));
            cmd.Parameters.Add(new SqlParameter("@GiftRegistryIsAnonymous", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@GiftRegistryAllowSearchByOthers", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@GiftRegistryNickName", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@GiftRegistryHideShippingAddresses", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@CODCompanyCheckAllowed", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@CODNet30Allowed", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@ExtensionData", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@FinalizationData", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@Deleted", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@Over13Checked", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@CurrencySetting", SqlDbType.NVarChar, 20));
            cmd.Parameters.Add(new SqlParameter("@VATSetting", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@VATRegistrationID", SqlDbType.NVarChar, 100));
            cmd.Parameters.Add(new SqlParameter("@StoreCCInDB", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@IsRegistered", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@LockedUntil", SqlDbType.VarChar, 30));
            cmd.Parameters.Add(new SqlParameter("@AdminCanViewCC", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@BadLogin", SqlDbType.SmallInt, 2));
            cmd.Parameters.Add(new SqlParameter("@Active", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@PwdChangeRequired", SqlDbType.TinyInt, 1));

            cmd.Parameters["@CustomerID"].Value = CustomerID;

            if (CustomerLevelID == null) cmd.Parameters["@CustomerLevelID"].Value = DBNull.Value;
            else cmd.Parameters["@CustomerLevelID"].Value = CustomerLevelID;

            if (RegisterDate == null) cmd.Parameters["@RegisterDate"].Value = DBNull.Value;
            else cmd.Parameters["@RegisterDate"].Value = RegisterDate;

            if (Email == null) cmd.Parameters["@Email"].Value = DBNull.Value;
            else cmd.Parameters["@Email"].Value = Email;

            if (SaltedAndHashedPassword == null) cmd.Parameters["@Password"].Value = DBNull.Value;
            else cmd.Parameters["@Password"].Value = SaltedAndHashedPassword;

            if (SaltKey == null) cmd.Parameters["@SaltKey"].Value = DBNull.Value;
            else cmd.Parameters["@SaltKey"].Value = SaltKey;

            if (DateOfBirth == null) cmd.Parameters["@DateOfBirth"].Value = DBNull.Value;
            else cmd.Parameters["@DateOfBirth"].Value = DateOfBirth;

            if (Gender == null) cmd.Parameters["@Gender"].Value = DBNull.Value;
            else cmd.Parameters["@Gender"].Value = Gender;

            if (FirstName == null) cmd.Parameters["@FirstName"].Value = DBNull.Value;
            else cmd.Parameters["@FirstName"].Value = FirstName;

            if (LastName == null) cmd.Parameters["@LastName"].Value = DBNull.Value;
            else cmd.Parameters["@LastName"].Value = LastName;

            if (Notes == null) cmd.Parameters["@Notes"].Value = DBNull.Value;
            else cmd.Parameters["@Notes"].Value = Notes;

            if (SkinID == null) cmd.Parameters["@SkinID"].Value = DBNull.Value;
            else cmd.Parameters["@SkinID"].Value = SkinID;

            if (Phone == null) cmd.Parameters["@Phone"].Value = DBNull.Value;
            else cmd.Parameters["@Phone"].Value = Phone;

            if (AffiliateID == null) cmd.Parameters["@AffiliateID"].Value = DBNull.Value;
            else cmd.Parameters["@AffiliateID"].Value = AffiliateID;

            if (Referrer == null) cmd.Parameters["@Referrer"].Value = DBNull.Value;
            else cmd.Parameters["@Referrer"].Value = Referrer;

            if (CouponCode == null) cmd.Parameters["@CouponCode"].Value = DBNull.Value;
            else cmd.Parameters["@CouponCode"].Value = CouponCode;

            if (OkToEmail == null) cmd.Parameters["@OkToEmail"].Value = DBNull.Value;
            else cmd.Parameters["@OkToEmail"].Value = OkToEmail;

            if (IsAdmin == null) cmd.Parameters["@IsAdmin"].Value = DBNull.Value;
            else cmd.Parameters["@IsAdmin"].Value = IsAdmin;

            if (BillingEqualsShipping == null) cmd.Parameters["@BillingEqualsShipping"].Value = DBNull.Value;
            else cmd.Parameters["@BillingEqualsShipping"].Value = BillingEqualsShipping;

            if (LastIPAddress == null) cmd.Parameters["@LastIPAddress"].Value = DBNull.Value;
            else cmd.Parameters["@LastIPAddress"].Value = LastIPAddress;

            if (OrderNotes == null) cmd.Parameters["@OrderNotes"].Value = DBNull.Value;
            else cmd.Parameters["@OrderNotes"].Value = OrderNotes;

            if (SubscriptionExpiresOn == null) cmd.Parameters["@SubscriptionExpiresOn"].Value = DBNull.Value;
            else cmd.Parameters["@SubscriptionExpiresOn"].Value = SubscriptionExpiresOn;

            if (RTShipRequest == null) cmd.Parameters["@RTShipRequest"].Value = DBNull.Value;
            else cmd.Parameters["@RTShipRequest"].Value = RTShipRequest;

            if (RTShipResponse == null) cmd.Parameters["@RTShipResponse"].Value = DBNull.Value;
            else cmd.Parameters["@RTShipResponse"].Value = RTShipResponse;

            if (OrderOptions == null) cmd.Parameters["@OrderOptions"].Value = DBNull.Value;
            else cmd.Parameters["@OrderOptions"].Value = OrderOptions;

            if (LocaleSetting == null) cmd.Parameters["@LocaleSetting"].Value = DBNull.Value;
            else cmd.Parameters["@LocaleSetting"].Value = LocaleSetting;

            if (MicroPayBalance == null) cmd.Parameters["@MicroPayBalance"].Value = DBNull.Value;
            else cmd.Parameters["@MicroPayBalance"].Value = MicroPayBalance;

            if (RecurringShippingMethodID == null) cmd.Parameters["@RecurringShippingMethodID"].Value = DBNull.Value;
            else cmd.Parameters["@RecurringShippingMethodID"].Value = RecurringShippingMethodID;

            if (RecurringShippingMethod == null) cmd.Parameters["@RecurringShippingMethod"].Value = DBNull.Value;
            else cmd.Parameters["@RecurringShippingMethod"].Value = RecurringShippingMethod;

            if (BillingAddressID == null) cmd.Parameters["@BillingAddressID"].Value = DBNull.Value;
            else cmd.Parameters["@BillingAddressID"].Value = BillingAddressID;

            if (ShippingAddressID == null) cmd.Parameters["@ShippingAddressID"].Value = DBNull.Value;
            else cmd.Parameters["@ShippingAddressID"].Value = ShippingAddressID;

            if (GiftRegistryGUID == null) cmd.Parameters["@GiftRegistryGUID"].Value = DBNull.Value;
            else cmd.Parameters["@GiftRegistryGUID"].Value = GiftRegistryGUID;

            if (GiftRegistryIsAnonymous == null) cmd.Parameters["@GiftRegistryIsAnonymous"].Value = DBNull.Value;
            else cmd.Parameters["@GiftRegistryIsAnonymous"].Value = GiftRegistryIsAnonymous;

            if (GiftRegistryAllowSearchByOthers == null) cmd.Parameters["@GiftRegistryAllowSearchByOthers"].Value = DBNull.Value;
            else cmd.Parameters["@GiftRegistryAllowSearchByOthers"].Value = GiftRegistryAllowSearchByOthers;

            if (GiftRegistryNickName == null) cmd.Parameters["@GiftRegistryNickName"].Value = DBNull.Value;
            else cmd.Parameters["@GiftRegistryNickName"].Value = GiftRegistryNickName;

            if (GiftRegistryHideShippingAddresses == null) cmd.Parameters["@GiftRegistryHideShippingAddresses"].Value = DBNull.Value;
            else cmd.Parameters["@GiftRegistryHideShippingAddresses"].Value = GiftRegistryHideShippingAddresses;

            if (CODCompanyCheckAllowed == null) cmd.Parameters["@CODCompanyCheckAllowed"].Value = DBNull.Value;
            else cmd.Parameters["@CODCompanyCheckAllowed"].Value = CODCompanyCheckAllowed;

            if (CODNet30Allowed == null) cmd.Parameters["@CODNet30Allowed"].Value = DBNull.Value;
            else cmd.Parameters["@CODNet30Allowed"].Value = CODNet30Allowed;

            if (ExtensionData == null) cmd.Parameters["@ExtensionData"].Value = DBNull.Value;
            else cmd.Parameters["@ExtensionData"].Value = ExtensionData;

            if (FinalizationData == null) cmd.Parameters["@FinalizationData"].Value = DBNull.Value;
            else cmd.Parameters["@FinalizationData"].Value = FinalizationData;

            if (Deleted == null) cmd.Parameters["@Deleted"].Value = DBNull.Value;
            else cmd.Parameters["@Deleted"].Value = Deleted;

            if (Over13Checked == null) cmd.Parameters["@Over13Checked"].Value = DBNull.Value;
            else cmd.Parameters["@Over13Checked"].Value = Over13Checked;

            if (CurrencySetting == null) cmd.Parameters["@CurrencySetting"].Value = DBNull.Value;
            else cmd.Parameters["@CurrencySetting"].Value = CurrencySetting;

            if (VATSetting == null) cmd.Parameters["@VATSetting"].Value = DBNull.Value;
            else cmd.Parameters["@VATSetting"].Value = VATSetting;

            if (VATRegistrationID == null) cmd.Parameters["@VATRegistrationID"].Value = DBNull.Value;
            else cmd.Parameters["@VATRegistrationID"].Value = VATRegistrationID;

            if (StoreCCInDB == null) cmd.Parameters["@StoreCCInDB"].Value = DBNull.Value;
            else cmd.Parameters["@StoreCCInDB"].Value = StoreCCInDB;

            if (IsRegistered == null) cmd.Parameters["@IsRegistered"].Value = DBNull.Value;
            else cmd.Parameters["@IsRegistered"].Value = IsRegistered;

            if (LockedUntil == null) cmd.Parameters["@LockedUntil"].Value = DBNull.Value;
            else cmd.Parameters["@LockedUntil"].Value = Localization.ToDBDateTimeString((DateTime)LockedUntil);

            if (AdminCanViewCC == null) cmd.Parameters["@AdminCanViewCC"].Value = DBNull.Value;
            else cmd.Parameters["@AdminCanViewCC"].Value = AdminCanViewCC;

            if (BadLogin == null)
            {
                cmd.Parameters["@BadLogin"].Value = 0;
            }
            else
            {
                if (Convert.ToInt16(BadLogin) < -1 || Convert.ToInt16(BadLogin) > 1) BadLogin = 0;
                cmd.Parameters["@BadLogin"].Value = BadLogin;
            }

            if (Active == null | (Convert.ToInt16(Active) != 0 && Convert.ToInt16(Active) != 1)) cmd.Parameters["@Active"].Value = DBNull.Value;
            else cmd.Parameters["@Active"].Value = Active;

            if (PwdChangeRequired == null) cmd.Parameters["@PwdChangeRequired"].Value = DBNull.Value;
            else cmd.Parameters["@PwdChangeRequired"].Value = PwdChangeRequired;


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

        /// <summary>
        /// Updates the customer static.
        /// </summary>
        /// <param name="Email">The email.</param>
        /// <param name="CustomerLevelID">The customer level ID.</param>
        /// <param name="SaltedAndHashedPassword">The salted and hashed password.</param>
        /// <param name="SaltKey">The salt key.</param>
        /// <param name="DateOfBirth">The date of birth.</param>
        /// <param name="Gender">The gender.</param>
        /// <param name="FirstName">The first name.</param>
        /// <param name="LastName">The last name.</param>
        /// <param name="Notes">The notes.</param>
        /// <param name="SkinID">The skin ID.</param>
        /// <param name="Phone">The phone.</param>
        /// <param name="AffiliateID">The affiliate ID.</param>
        /// <param name="Referrer">The referrer.</param>
        /// <param name="CouponCode">The coupon code.</param>
        /// <param name="OkToEmail">The ok to email.</param>
        /// <param name="IsAdmin">The is admin.</param>
        /// <param name="BillingEqualsShipping">The billing equals shipping.</param>
        /// <param name="LastIPAddress">The last IP address.</param>
        /// <param name="OrderNotes">The order notes.</param>
        /// <param name="SubscriptionExpiresOn">The subscription expires on.</param>
        /// <param name="RTShipRequest">The RT ship request.</param>
        /// <param name="RTShipResponse">The RT ship response.</param>
        /// <param name="OrderOptions">The order options.</param>
        /// <param name="LocaleSetting">The locale setting.</param>
        /// <param name="MicroPayBalance">The micro pay balance.</param>
        /// <param name="RecurringShippingMethodID">The recurring shipping method ID.</param>
        /// <param name="RecurringShippingMethod">The recurring shipping method.</param>
        /// <param name="BillingAddressID">The billing address ID.</param>
        /// <param name="ShippingAddressID">The shipping address ID.</param>
        /// <param name="GiftRegistryGUID">The gift registry GUID.</param>
        /// <param name="GiftRegistryIsAnonymous">The gift registry is anonymous.</param>
        /// <param name="GiftRegistryAllowSearchByOthers">The gift registry allow search by others.</param>
        /// <param name="GiftRegistryNickName">Name of the gift registry nick.</param>
        /// <param name="GiftRegistryHideShippingAddresses">The gift registry hide shipping addresses.</param>
        /// <param name="CODCompanyCheckAllowed">The COD company check allowed.</param>
        /// <param name="CODNet30Allowed">The COD net30 allowed.</param>
        /// <param name="ExtensionData">The extension data.</param>
        /// <param name="FinalizationData">The finalization data.</param>
        /// <param name="Deleted">Is deleted.</param>
        /// <param name="Over13Checked">Is over13 checked.</param>
        /// <param name="CurrencySetting">The currency setting.</param>
        /// <param name="VATSetting">The VAT setting.</param>
        /// <param name="VATRegistrationID">The VAT registration ID.</param>
        /// <param name="StoreCCInDB">The store CC in DB.</param>
        /// <param name="IsRegistered">The is registered.</param>
        /// <param name="LockedUntil">Is locked until.</param>
        /// <param name="AdminCanViewCC">The admin can view CC.</param>
        /// <param name="BadLogin">The bad login.</param>
        /// <param name="Active">Is active.</param>
        /// <param name="PwdChangeRequired">The Password change required.</param>
        /// <param name="RegisterDate">The register date.</param>
        /// <returns></returns>
        public static string UpdateCustomerStatic(string Email, object CustomerLevelID, string SaltedAndHashedPassword, object SaltKey, string DateOfBirth, string Gender, string FirstName, string LastName, string Notes, object SkinID, string Phone, object AffiliateID, string Referrer, string CouponCode, object OkToEmail, object IsAdmin, object BillingEqualsShipping, string LastIPAddress, string OrderNotes, string SubscriptionExpiresOn, string RTShipRequest, string RTShipResponse, string OrderOptions, string LocaleSetting, object MicroPayBalance, object RecurringShippingMethodID, string RecurringShippingMethod, object BillingAddressID, object ShippingAddressID, string GiftRegistryGUID, object GiftRegistryIsAnonymous, object GiftRegistryAllowSearchByOthers, string GiftRegistryNickName, object GiftRegistryHideShippingAddresses, object CODCompanyCheckAllowed, object CODNet30Allowed, string ExtensionData, string FinalizationData, object Deleted, object Over13Checked, string CurrencySetting, object VATSetting, string VATRegistrationID, object StoreCCInDB, object IsRegistered, object LockedUntil, object AdminCanViewCC, object BadLogin, object Active, object PwdChangeRequired, object RegisterDate)
        {
            AppLogic.eventHandler("UpdateCustomer").CallEvent("&UpdateCustomer=true");

            string err = String.Empty;
            SqlConnection cn = new SqlConnection(DB.GetDBConn());
            cn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = cn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "dbo.aspdnsf_updCustomerByEmail";

            if (SaltKey != null && (int)SaltKey == Security.ro_SaltKeyIsInvalid)
            {
                SaltKey = null;
            }

            cmd.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@CustomerLevelID", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@RegisterDate", SqlDbType.VarChar, 30));
            cmd.Parameters.Add(new SqlParameter("@Password", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@SaltKey", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@DateOfBirth", SqlDbType.VarChar, 30));
            cmd.Parameters.Add(new SqlParameter("@Gender", SqlDbType.NVarChar, 2));
            cmd.Parameters.Add(new SqlParameter("@FirstName", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@LastName", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@Notes", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@SkinID", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@Phone", SqlDbType.NVarChar, 50));
            cmd.Parameters.Add(new SqlParameter("@AffiliateID", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@Referrer", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@CouponCode", SqlDbType.NVarChar, 100));
            cmd.Parameters.Add(new SqlParameter("@OkToEmail", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@IsAdmin", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@BillingEqualsShipping", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@LastIPAddress", SqlDbType.VarChar, 40));
            cmd.Parameters.Add(new SqlParameter("@OrderNotes", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@SubscriptionExpiresOn", SqlDbType.VarChar, 30));
            cmd.Parameters.Add(new SqlParameter("@RTShipRequest", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@RTShipResponse", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@OrderOptions", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@LocaleSetting", SqlDbType.NVarChar, 20));
            cmd.Parameters.Add(new SqlParameter("@MicroPayBalance", SqlDbType.Decimal, 8));
            cmd.Parameters.Add(new SqlParameter("@RecurringShippingMethodID", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@RecurringShippingMethod", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@BillingAddressID", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@ShippingAddressID", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@GiftRegistryGUID", SqlDbType.VarChar, 40));
            cmd.Parameters.Add(new SqlParameter("@GiftRegistryIsAnonymous", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@GiftRegistryAllowSearchByOthers", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@GiftRegistryNickName", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@GiftRegistryHideShippingAddresses", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@CODCompanyCheckAllowed", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@CODNet30Allowed", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@ExtensionData", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@FinalizationData", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@Deleted", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@Over13Checked", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@CurrencySetting", SqlDbType.NVarChar, 20));
            cmd.Parameters.Add(new SqlParameter("@VATSetting", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@VATRegistrationID", SqlDbType.NVarChar, 100));
            cmd.Parameters.Add(new SqlParameter("@StoreCCInDB", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@IsRegistered", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@LockedUntil", SqlDbType.VarChar, 30));
            cmd.Parameters.Add(new SqlParameter("@AdminCanViewCC", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@BadLogin", SqlDbType.SmallInt, 2));
            cmd.Parameters.Add(new SqlParameter("@Active", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@PwdChangeRequired", SqlDbType.TinyInt, 1));


            cmd.Parameters["@Email"].Value = Email;

            if (CustomerLevelID == null) cmd.Parameters["@CustomerLevelID"].Value = DBNull.Value;
            else cmd.Parameters["@CustomerLevelID"].Value = CustomerLevelID;

            if (RegisterDate == null) cmd.Parameters["@RegisterDate"].Value = DBNull.Value;
            else cmd.Parameters["@RegisterDate"].Value = RegisterDate;

            if (SaltedAndHashedPassword == null) cmd.Parameters["@Password"].Value = DBNull.Value;
            else cmd.Parameters["@Password"].Value = SaltedAndHashedPassword;

            if (SaltKey == null) cmd.Parameters["@SaltKey"].Value = DBNull.Value;
            else cmd.Parameters["@SaltKey"].Value = SaltKey;

            if (DateOfBirth == null) cmd.Parameters["@DateOfBirth"].Value = DBNull.Value;
            else cmd.Parameters["@DateOfBirth"].Value = DateOfBirth;

            if (Gender == null) cmd.Parameters["@Gender"].Value = DBNull.Value;
            else cmd.Parameters["@Gender"].Value = Gender;

            if (FirstName == null) cmd.Parameters["@FirstName"].Value = DBNull.Value;
            else cmd.Parameters["@FirstName"].Value = FirstName;

            if (LastName == null) cmd.Parameters["@LastName"].Value = DBNull.Value;
            else cmd.Parameters["@LastName"].Value = LastName;

            if (Notes == null) cmd.Parameters["@Notes"].Value = DBNull.Value;
            else cmd.Parameters["@Notes"].Value = Notes;

            if (SkinID == null) cmd.Parameters["@SkinID"].Value = DBNull.Value;
            else cmd.Parameters["@SkinID"].Value = SkinID;

            if (Phone == null) cmd.Parameters["@Phone"].Value = DBNull.Value;
            else cmd.Parameters["@Phone"].Value = Phone;

            if (AffiliateID == null) cmd.Parameters["@AffiliateID"].Value = DBNull.Value;
            else cmd.Parameters["@AffiliateID"].Value = AffiliateID;

            if (Referrer == null) cmd.Parameters["@Referrer"].Value = DBNull.Value;
            else cmd.Parameters["@Referrer"].Value = Referrer;

            if (CouponCode == null) cmd.Parameters["@CouponCode"].Value = DBNull.Value;
            else cmd.Parameters["@CouponCode"].Value = CouponCode;

            if (OkToEmail == null) cmd.Parameters["@OkToEmail"].Value = DBNull.Value;
            else cmd.Parameters["@OkToEmail"].Value = OkToEmail;

            if (IsAdmin == null) cmd.Parameters["@IsAdmin"].Value = DBNull.Value;
            else cmd.Parameters["@IsAdmin"].Value = IsAdmin;

            if (BillingEqualsShipping == null) cmd.Parameters["@BillingEqualsShipping"].Value = DBNull.Value;
            else cmd.Parameters["@BillingEqualsShipping"].Value = BillingEqualsShipping;

            if (LastIPAddress == null) cmd.Parameters["@LastIPAddress"].Value = DBNull.Value;
            else cmd.Parameters["@LastIPAddress"].Value = LastIPAddress;

            if (OrderNotes == null) cmd.Parameters["@OrderNotes"].Value = DBNull.Value;
            else cmd.Parameters["@OrderNotes"].Value = OrderNotes;

            if (SubscriptionExpiresOn == null) cmd.Parameters["@SubscriptionExpiresOn"].Value = DBNull.Value;
            else cmd.Parameters["@SubscriptionExpiresOn"].Value = SubscriptionExpiresOn;

            if (RTShipRequest == null) cmd.Parameters["@RTShipRequest"].Value = DBNull.Value;
            else cmd.Parameters["@RTShipRequest"].Value = RTShipRequest;

            if (RTShipResponse == null) cmd.Parameters["@RTShipResponse"].Value = DBNull.Value;
            else cmd.Parameters["@RTShipResponse"].Value = RTShipResponse;

            if (OrderOptions == null) cmd.Parameters["@OrderOptions"].Value = DBNull.Value;
            else cmd.Parameters["@OrderOptions"].Value = OrderOptions;

            if (LocaleSetting == null) cmd.Parameters["@LocaleSetting"].Value = DBNull.Value;
            else cmd.Parameters["@LocaleSetting"].Value = LocaleSetting;

            if (MicroPayBalance == null) cmd.Parameters["@MicroPayBalance"].Value = DBNull.Value;
            else cmd.Parameters["@MicroPayBalance"].Value = MicroPayBalance;

            if (RecurringShippingMethodID == null) cmd.Parameters["@RecurringShippingMethodID"].Value = DBNull.Value;
            else cmd.Parameters["@RecurringShippingMethodID"].Value = RecurringShippingMethodID;

            if (RecurringShippingMethod == null) cmd.Parameters["@RecurringShippingMethod"].Value = DBNull.Value;
            else cmd.Parameters["@RecurringShippingMethod"].Value = RecurringShippingMethod;

            if (BillingAddressID == null) cmd.Parameters["@BillingAddressID"].Value = DBNull.Value;
            else cmd.Parameters["@BillingAddressID"].Value = BillingAddressID;

            if (ShippingAddressID == null) cmd.Parameters["@ShippingAddressID"].Value = DBNull.Value;
            else cmd.Parameters["@ShippingAddressID"].Value = ShippingAddressID;

            if (GiftRegistryGUID == null) cmd.Parameters["@GiftRegistryGUID"].Value = DBNull.Value;
            else cmd.Parameters["@GiftRegistryGUID"].Value = GiftRegistryGUID;

            if (GiftRegistryIsAnonymous == null) cmd.Parameters["@GiftRegistryIsAnonymous"].Value = DBNull.Value;
            else cmd.Parameters["@GiftRegistryIsAnonymous"].Value = GiftRegistryIsAnonymous;

            if (GiftRegistryAllowSearchByOthers == null) cmd.Parameters["@GiftRegistryAllowSearchByOthers"].Value = DBNull.Value;
            else cmd.Parameters["@GiftRegistryAllowSearchByOthers"].Value = GiftRegistryAllowSearchByOthers;

            if (GiftRegistryNickName == null) cmd.Parameters["@GiftRegistryNickName"].Value = DBNull.Value;
            else cmd.Parameters["@GiftRegistryNickName"].Value = GiftRegistryNickName;

            if (GiftRegistryHideShippingAddresses == null) cmd.Parameters["@GiftRegistryHideShippingAddresses"].Value = DBNull.Value;
            else cmd.Parameters["@GiftRegistryHideShippingAddresses"].Value = GiftRegistryHideShippingAddresses;

            if (CODCompanyCheckAllowed == null) cmd.Parameters["@CODCompanyCheckAllowed"].Value = DBNull.Value;
            else cmd.Parameters["@CODCompanyCheckAllowed"].Value = CODCompanyCheckAllowed;

            if (CODNet30Allowed == null) cmd.Parameters["@CODNet30Allowed"].Value = DBNull.Value;
            else cmd.Parameters["@CODNet30Allowed"].Value = CODNet30Allowed;

            if (ExtensionData == null) cmd.Parameters["@ExtensionData"].Value = DBNull.Value;
            else cmd.Parameters["@ExtensionData"].Value = ExtensionData;

            if (FinalizationData == null) cmd.Parameters["@FinalizationData"].Value = DBNull.Value;
            else cmd.Parameters["@FinalizationData"].Value = FinalizationData;

            if (Deleted == null) cmd.Parameters["@Deleted"].Value = DBNull.Value;
            else cmd.Parameters["@Deleted"].Value = Deleted;

            if (Over13Checked == null) cmd.Parameters["@Over13Checked"].Value = DBNull.Value;
            else cmd.Parameters["@Over13Checked"].Value = Over13Checked;

            if (CurrencySetting == null) cmd.Parameters["@CurrencySetting"].Value = DBNull.Value;
            else cmd.Parameters["@CurrencySetting"].Value = CurrencySetting;

            if (VATSetting == null) cmd.Parameters["@VATSetting"].Value = DBNull.Value;
            else cmd.Parameters["@VATSetting"].Value = VATSetting;

            if (VATRegistrationID == null) cmd.Parameters["@VATRegistrationID"].Value = DBNull.Value;
            else cmd.Parameters["@VATRegistrationID"].Value = VATRegistrationID;

            if (StoreCCInDB == null) cmd.Parameters["@StoreCCInDB"].Value = DBNull.Value;
            else cmd.Parameters["@StoreCCInDB"].Value = StoreCCInDB;

            if (IsRegistered == null) cmd.Parameters["@IsRegistered"].Value = DBNull.Value;
            else cmd.Parameters["@IsRegistered"].Value = IsRegistered;

            if (LockedUntil == null) cmd.Parameters["@LockedUntil"].Value = DBNull.Value;
            else cmd.Parameters["@LockedUntil"].Value = Localization.ToDBDateTimeString((DateTime)LockedUntil);

            if (AdminCanViewCC == null) cmd.Parameters["@AdminCanViewCC"].Value = DBNull.Value;
            else cmd.Parameters["@AdminCanViewCC"].Value = AdminCanViewCC;

            if (BadLogin == null)
            {
                cmd.Parameters["@BadLogin"].Value = 0;
            }
            else
            {
                if (Convert.ToInt16(BadLogin) < -1 || Convert.ToInt16(BadLogin) > 1) BadLogin = 0;
                cmd.Parameters["@BadLogin"].Value = BadLogin;
            }

            if (Active == null | (Convert.ToInt16(Active) != 0 && Convert.ToInt16(Active) != 1)) cmd.Parameters["@Active"].Value = DBNull.Value;
            else cmd.Parameters["@Active"].Value = Active;

            if (PwdChangeRequired == null) cmd.Parameters["@PwdChangeRequired"].Value = DBNull.Value;
            else cmd.Parameters["@PwdChangeRequired"].Value = PwdChangeRequired;


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

        /// <summary>
        /// Updates the customer static.
        /// </summary>
        /// <param name="CustomerID">The customerID.</param>
        /// <param name="spa">The sql parameter.</param>
        /// <returns></returns>
        public static string UpdateCustomerStatic(int CustomerID, SqlParameter[] spa)
        {

            string err = String.Empty;
            try
            {
                SqlParameter sp = DB.CreateSQLParameter("@CustomerID", SqlDbType.Int, 4, CustomerID, ParameterDirection.Input);
                spa = DB.CreateSQLParameterArray(spa, sp);

                DB.ExecuteStoredProcInt("dbo.aspdnsf_updCustomer", spa);
            }
            catch (Exception ex)
            {
                err = ex.Message;
            }
            return err;


        }


        /// <summary>
        /// Gets the name of the customerid supplied.
        /// </summary>
        /// <param name="CustomerID">The customer ID.</param>
        /// <returns>Returns the Customer name</returns>
        static public String GetName(int CustomerID)
        {
            String tmpS = String.Empty;

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("Select firstname,lastname from customer   with (NOLOCK)  where CustomerID=" + CustomerID.ToString(), dbconn))
                {
                    if (rs.Read())
                    {
                        tmpS = (DB.RSField(rs, "FirstName") + " " + DB.RSField(rs, "LastName")).Trim();
                    }
                    return tmpS;
                }
            }
        }

        /// <summary>
        /// Determines whether [has at least one address] [the specified customer ID].
        /// </summary>
        /// <param name="CustomerID">The customer ID.</param>
        /// <returns>
        /// 	<c>true</c> if [has at least one address] [the specified customer ID]; otherwise, <c>false</c>.
        /// </returns>
        static public bool HasAtLeastOneAddress(int CustomerID)
        {
            return (DB.GetSqlN("select count(AddressID) as N from Address  with (NOLOCK)  where CustomerID=" + CustomerID.ToString()) > 0);
        }

        /// <summary>
        /// Ships to address.
        /// </summary>
        /// <param name="Checkout">if set to <c>true</c> [checkout].</param>
        /// <param name="IncludePhone">if set to <c>true</c> [include phone].</param>
        /// <param name="CustomerID">The customer ID.</param>
        /// <param name="separator">The separator.</param>
        /// <returns></returns>
        static public String ShipToAddress(bool Checkout, bool IncludePhone, int CustomerID, String separator)
        {
            Address ShippingAddress = new Address();
            ShippingAddress.LoadByCustomer(CustomerID, AddressTypes.Shipping);
            return ShippingAddress.DisplayHTML(IncludePhone);
        }

        /// <summary>
        /// Bills to address.
        /// </summary>
        /// <param name="Checkout">if set to <c>true</c> [checkout].</param>
        /// <param name="IncludePhone">if set to <c>true</c> [include phone].</param>
        /// <param name="CustomerID">The customer ID.</param>
        /// <param name="separator">The separator.</param>
        /// <returns></returns>
        static public String BillToAddress(bool Checkout, bool IncludePhone, int CustomerID, String separator)
        {
            Address BillingAddress = new Address();
            BillingAddress.LoadByCustomer(CustomerID, AddressTypes.Billing);
            return BillingAddress.DisplayHTML(IncludePhone);
        }

        /// <summary>
        /// Gets the Billing information.
        /// </summary>
        /// <param name="CustomerID">The customer ID.</param>
        /// <param name="separator">The separator.</param>
        /// <returns>Returns the billing information</returns>
        static public String BillingInformation(int CustomerID, String separator)
        {
            Address BillingAddress = new Address();
            BillingAddress.LoadByCustomer(CustomerID, AddressTypes.Billing);
            return BillingAddress.DisplayCardString(separator);
        }

        /// <summary>
        /// Gets the CustomerID from E mail.
        /// </summary>
        /// <param name="EMail">The E mail.</param>
        /// <returns>Returns the CustomerID from the EMail supplied.</returns>
        static public int GetIDFromEMail(String EMail)
        {
            int tmpS = 0;

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("Select CustomerID from customer   with (NOLOCK)  where deleted=0 and EMail=" + DB.SQuote(EMail.ToLowerInvariant()), dbconn))
                {
                    if (rs.Read())
                    {
                        tmpS = DB.RSFieldInt(rs, "CustomerID");
                    }
                    return tmpS;
                }
            }
        }

        /// <summary>
        /// Gets the CustomerGUID.
        /// </summary>
        /// <param name="customerID">The customer ID.</param>
        /// <returns>Returns the CustomerGUID</returns>
        static public String GetGUID(int customerID)
        {
            String tmpS = String.Empty;

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("Select CustomerGUID from customer   with (NOLOCK)  where customerid=" + customerID.ToString(), dbconn))
                {
                    if (rs.Read())
                    {
                        tmpS = DB.RSFieldGUID(rs, "CustomerGUID");
                    }
                    return tmpS;
                }
            }
        }

        /// <summary>
        /// Determines whether the specified customer ID has orders.
        /// </summary>
        /// <param name="CustomerID">The customer ID.</param>
        /// <returns>
        /// 	<c>true</c> if the specified customer ID has orders; otherwise, <c>false</c>.
        /// </returns>
        static public bool HasOrders(int CustomerID)
        {
            return (DB.GetSqlN("select count(ordernumber) as N from orders   with (NOLOCK)  where customerid=" + CustomerID.ToString()) > 0);
        }

        /// <summary>
        /// Determines whether [has used coupon] [the specified customer ID].
        /// </summary>
        /// <param name="CustomerID">The customer ID.</param>
        /// <param name="CouponCode">The coupon code.</param>
        /// <returns>
        /// 	<c>true</c> if [has used coupon] [the specified customer ID]; otherwise, <c>false</c>.
        /// </returns>
        static public bool HasUsedCoupon(int CustomerID, String CouponCode)
        {
            return (DB.GetSqlN("select count(ordernumber) as N from orders   with (NOLOCK)  where customerid=" + CustomerID.ToString() + " and lower(CouponCode)=" + DB.SQuote(CouponCode.ToLowerInvariant())) != 0);
        }

        /// <summary>
        /// Gets the customer level ID.
        /// </summary>
        /// <param name="CustomerID">The customer ID.</param>
        /// <returns>Returns the CustomerLevelID</returns>
        static public int GetCustomerLevelID(int CustomerID)
        {
            int tmpS = 0;

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("Select CustomerLevelID from customer where customerlevelid in (select customerlevelid from customerlevel where deleted=0) and deleted=0 and CustomerID=" + CustomerID.ToString(), dbconn))
                {
                    if (rs.Read())
                    {
                        tmpS = DB.RSFieldInt(rs, "CustomerLevelID");
                    }
                    return tmpS;
                }
            }
        }

        /// <summary>
        /// Gets the Customer username.
        /// </summary>
        /// <param name="CustomerID">The customer ID.</param>
        /// <returns>Returns the Customer Username</returns>
        public static String GetUsername(int CustomerID)
        {

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("Select FirstName,LastName from customer where customerid=" + CustomerID.ToString(), dbconn))
                {
                    String uname = String.Empty;
                    if (rs.Read())
                    {
                        uname = (DB.RSField(rs, "FirstName") + "" + DB.RSField(rs, "LastName")).Trim();
                    }
                    return uname;
                }
            }
        }

        /// <summary>
        /// Check if the User is a SuperUser.
        /// </summary>
        /// <param name="CustomerID">The customerID.</param>
        /// <returns>Returns TRUE if the user is a SuperUser otherwise FALSE.</returns>
        static public bool StaticIsAdminSuperUser(int CustomerID)
        {
            if (CustomerID == 0)
            {
                return false;
            }
            bool tmp = false;

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("select IsAdmin from Customer  with (NOLOCK)  where CustomerID=" + CustomerID.ToString(), dbconn))
                {
                    if (rs.Read())
                    {
                        tmp = ((DB.RSFieldTinyInt(rs, "IsAdmin") & 2) != 0);
                    }
                    return tmp;
                }
            }
        }

        /// <summary>
        /// Check if the User is a AdminUser. 
        /// </summary>
        /// <param name="CustomerID">The customerID.</param>
        /// <returns>Returns TRUE if the user is a AdminUser otherwise FALSE.</returns>
        static public bool StaticIsAdminUser(int CustomerID)
        {
            if (CustomerID == 0)
            {
                return false;
            }
            bool tmp = false;

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("select IsAdmin from customer  with (NOLOCK)  where CustomerID=" + CustomerID.ToString(), dbconn))
                {
                    if (rs.Read())
                    {
                        tmp = ((DB.RSFieldTinyInt(rs, "IsAdmin") & 1) != 0);
                    }
                    return tmp;
                }
            }
        }

        /// <summary>
        /// Gets the name of the customer level.
        /// </summary>
        /// <param name="CustomerLevelID">The customerlevelID.</param>
        /// <param name="LocaleSetting">The locale setting.</param>
        /// <returns>Returns the CustomerLevelName</returns>
        static public String GetCustomerLevelName(int CustomerLevelID, String LocaleSetting)
        {
            String tmpS = String.Empty;
            if (CustomerLevelID != 0)
            {
                using (SqlConnection dbconn = DB.dbConn())
                {
                    dbconn.Open();
                    using (IDataReader rs = DB.GetRS("Select Name from CustomerLevel  with (NOLOCK)  where CustomerLevelID=" + CustomerLevelID.ToString(), dbconn))
                    {
                        if (rs.Read())
                        {
                            tmpS = DB.RSFieldByLocale(rs, "Name", LocaleSetting);
                        }
                    }
                }
            }
            return tmpS;
        }

        /// <summary>
        /// Gets the customer primary shipping address ID.
        /// </summary>
        /// <param name="CustomerID">The customer ID.</param>
        /// <returns>Returns the customer primary shipping address ID.</returns>
        static public int GetCustomerPrimaryShippingAddressID(int CustomerID)
        {
            int tmp = 0;
            if (CustomerID != 0)
            {
                using (SqlConnection dbconn = DB.dbConn())
                {
                    dbconn.Open();
                    using (IDataReader rs = DB.GetRS("Select ShippingAddressID from Customer  with (NOLOCK)  where CustomerID=" + CustomerID.ToString(), dbconn))
                    {
                        if (rs.Read())
                        {
                            tmp = DB.RSFieldInt(rs, "ShippingAddressID");
                        }
                    }
                }
            }
            return tmp;
        }

        /// <summary>
        // returns true if this address belongs to this customer
        /// </summary>
        /// <param name="CustomerID">The customerID.</param>
        /// <param name="AddressID">The addressID.</param>
        /// <returns>Returns TRUE if this address belongs to this customer otherwise FALSE.</returns>
        static public bool OwnsThisAddress(int CustomerID, int AddressID)
        {
            return (DB.GetSqlN("select count(*) as N from Address  with (NOLOCK)  where CustomerID=" + CustomerID.ToString() + " and AddressID=" + AddressID.ToString()) > 0);
        }

        /// <summary>
        /// Check if this address belong to this customer.
        /// </summary>
        /// <param name="CustomerID">The customer ID.</param>
        /// <param name="OrderNumber">The order number.</param>
        /// <returns>Returns TRUE if this address belongs to this customer otherwise FALSE.</returns>
        static public bool OwnsThisOrder(int CustomerID, int OrderNumber)
        {
            return (DB.GetSqlN("select count(*) as N from Orders  with (NOLOCK)  where CustomerID=" + CustomerID.ToString() + " and OrderNumber=" + OrderNumber.ToString()) > 0);
        }

        /// <summary>
        /// Makes a new customer record and set SESSION parameters.
        /// </summary>
        /// <param name="CustomerID">The customerID.</param>
        /// <param name="CustomerGUID">The customerGUID.</param>
        public static void MakeAnonCustomerRecord(out int CustomerID, out String CustomerGUID)
        {

            String CookiePrefix = CommonLogic.IIF(AppLogic.IsAdminSite, "Admin", "");

            int AffiliateID = int.Parse(CommonLogic.IIF(CommonLogic.IsInteger(HttpContext.Current.Profile.GetPropertyValue(CookiePrefix + ro_AffiliateCookieName).ToString()), HttpContext.Current.Profile.GetPropertyValue(CookiePrefix + ro_AffiliateCookieName).ToString(), "0"));

            String LocaleSetting = Localization.ValidateLocaleSetting(HttpContext.Current.Profile.GetPropertyValue(CookiePrefix + ro_LocaleSettingCookieName).ToString());
            if (LocaleSetting.Length == 0 || AppLogic.IsAdminSite)
            {
                LocaleSetting = Localization.GetDefaultLocale();
            }

            String CurrencySetting = Currency.ValidateCurrencySetting(HttpContext.Current.Profile.GetPropertyValue(CookiePrefix + ro_CurrencySettingCookieName).ToString());
            if (CurrencySetting.Length == 0 || AppLogic.IsAdminSite)
            {
                CurrencySetting = Localization.GetPrimaryCurrency();
            }

            int VATSetting = int.Parse(CommonLogic.IIF(CommonLogic.IsInteger(HttpContext.Current.Profile.GetPropertyValue(CookiePrefix + ro_VATSettingCookieName).ToString()), HttpContext.Current.Profile.GetPropertyValue(CookiePrefix + ro_VATSettingCookieName).ToString(), "0"));
            if (VATSetting == 0 || AppLogic.IsAdminSite)
            {
                VATSetting = AppLogic.AppConfigUSInt("VAT.DefaultSetting");
            }

            CustomerID = CreateCustomerRecord(null, null, null, AffiliateID, HttpContext.Current.Profile.GetPropertyValue(ro_ReferrerCookieName).ToString(), null, CommonLogic.CustomerIpAddress(), LocaleSetting, null, CurrencySetting, VATSetting, null, null);
            CustomerGUID = StaticGetCustomerGUID(CustomerID);
            AppLogic.eventHandler("CreateCustomer").CallEvent("&CreateCustomer=true&CreatedCustomerID=" + CustomerID.ToString());
        }

        /// <summary>
        /// Gets the CustomerGUID by the specified CustomerID.
        /// </summary>
        /// <param name="CustomerID">The customerID.</param>
        /// <returns>Returns CustomerGUID</returns>
        static public String StaticGetCustomerGUID(int CustomerID)
        {
            String tmpS = String.Empty;

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("select CustomerGUID from Customer  with (NOLOCK)  where CustomerID=" + CustomerID.ToString(), dbconn))
                {
                    if (rs.Read())
                    {
                        tmpS = DB.RSFieldGUID(rs, "CustomerGUID");
                    }
                    return tmpS;
                }
            }
        }

        /// <summary>
        /// Locks the account.
        /// </summary>
        /// <param name="CustomerID">The customerID.</param>
        /// <param name="LockIt">if set to <c>true</c> [lock it].</param>
        public static void LockAccount(int CustomerID, bool LockIt)
        {
            if (LockIt)
            {
                DB.ExecuteSQL("update customer set Active=0 where CustomerID=" + CustomerID.ToString());
            }
            else
            {
                DB.ExecuteSQL("update customer set Active=1 where CustomerID=" + CustomerID.ToString());
            }
        }

        /// <summary>
        /// Validates the skin.
        /// </summary>
        /// <param name="SkinID">The skinID.</param>
        /// <returns>Returns the validated skin</returns>
        public static String ValidateSkin(String SkinID)
        {
            if (SkinID.Length != 0 && CommonLogic.IsInteger(SkinID))
            {
                return SkinID;
            }
            return AppLogic.GetStoreSkinID(AppLogic.StoreID()).ToString();
        }

        /// <summary>
        /// Gets the default affiliate.
        /// </summary>
        /// <returns>Returns the default affiliate</returns>
        public static String GetDefaultAffiliate()
        {
            return "0";
        }

        /// <summary>
        /// Validates the affiliate.
        /// </summary>
        /// <param name="AffiliateID">The affiliate ID.</param>
        /// <returns>Returns the validated affiliate.</returns>
        public static String ValidateAffiliate(String AffiliateID)
        {
            // choosing to only validate that it's an integer, not that it actually is defined as an affiliate
            // if you wanted to, you could validate against the affilate table using the code shown below, but we're
            // choosing not to do that right now, as customers use affiliateid flags in a number of various ways.
            if (AffiliateID.Length != 0 && CommonLogic.IsInteger(AffiliateID))
            {
                return AffiliateID;
            }
            return "0";
        }

        /// <summary>
        /// Gets the default VAT setting.
        /// </summary>
        /// <returns>Returns the default VAT setting.</returns>
        public static String GetDefaultVATSetting()
        {
            return AppLogic.AppConfigUSInt("VAT.DefaultSetting").ToString();
        }

        /// <summary>
        /// Validates the VAT setting.
        /// </summary>
        /// <param name="VATSettingID">The VAT setting ID.</param>
        /// <returns>Returns the validated VAT setting.</returns>
        public static String ValidateVATSetting(String VATSettingID)
        {
            if (!AppLogic.AppConfigBool("VAT.AllowCustomerToChooseSetting"))
            {
                return AppLogic.AppConfigUSInt("VAT.DefaultSetting").ToString();
            }
            if (VATSettingID == "1" || VATSettingID == "2")
            {
                return VATSettingID;
            }
            return AppLogic.AppConfigUSInt("VAT.DefaultSetting").ToString();
        }

        /// <summary>
        /// Validates the VAT setting as enum.
        /// </summary>
        /// <param name="VATSettingID">The VAT setting ID.</param>
        /// <returns>Returns the validated VAT Setting as Enum.</returns>
        public static VATSettingEnum ValidateVATSettingAsEnum(String VATSettingID)
        {
            if (VATSettingID == "1" || VATSettingID == "2")
            {
                return (VATSettingEnum)(System.Int32.Parse(VATSettingID));
            }
            return (VATSettingEnum)AppLogic.AppConfigUSInt("VAT.DefaultSetting");
        }

        /// <summary>
        /// Creates the new anonymous customer object.
        /// </summary>
        /// <returns>Returns the new anonymous customer object.</returns>
        public static Customer CreateNewAnonCustomerObject()
        {
            int CustomerID = 0;
            String CustomerGUID = String.Empty;
            Customer.MakeAnonCustomerRecord(out CustomerID, out CustomerGUID);
            Customer NewCustomer = new Customer(CustomerID, true);
            return NewCustomer;
        }

        #endregion


        #region Private Methods

        /// <summary>
        /// Initilizes the customer fields.
        /// </summary>
        /// <param name="rs">The sql reader.</param>
        private void Init(IDataReader rs)
        {
            m_CustomerID = 0;
            m_AffiliateID = 0;
            m_LocaleSetting = Localization.GetDefaultLocale();
            m_CurrencySetting = Localization.GetPrimaryCurrency();
            m_VATSetting = (VATSettingEnum)AppLogic.AppConfigUSInt("VAT.DefaultSetting");
            m_VATRegistrationID = String.Empty;
            m_LastIPAddress = CommonLogic.CustomerIpAddress();
            m_CustomerLevelID = 0;
            m_CustomerLevelName = String.Empty;
            m_HasCustomerRecord = false;
            m_Phone = String.Empty;
            m_EMail = String.Empty;
            m_Notes = String.Empty;
            m_OKToEMail = false;
            m_CODCompanyCheckAllowed = false;
            m_CODNet30Allowed = false;
            m_CouponCode = String.Empty;
            m_GiftRegistryGUID = String.Empty;
            m_GiftRegistryIsAnonymous = false;
            m_GiftRegistryAllowSearchByOthers = false;
            m_GiftRegistryNickName = String.Empty;
            m_GiftRegistryHideShippingAddresses = false;
            m_Password = String.Empty;
            m_SaltKey = Security.ro_SaltKeyIsInvalid;
            m_CreatedOn = System.DateTime.MinValue;
            m_IsRegistered = false;
            m_IsAdminUser = false;
            m_IsAdminSuperUser = false;
            m_FirstName = String.Empty;
            m_LastName = String.Empty;
            m_MicroPayBalance = System.Decimal.Zero;
            m_SubscriptionExpiresOn = System.DateTime.MinValue;
            m_RecurringShippingMethodID = 0;
            m_RecurringShippingMethod = String.Empty;
            m_PrimaryBillingAddressID = 0;
            m_PrimaryShippingAddressID = 0;
            m_CurrentSessionID = 0;
            m_StoreCCInDB = false;
            m_LockedUntil = DateTime.MinValue;
            m_AdminCanViewCC = false;
            m_PwdChanged = DateTime.MinValue;
            m_BadLoginCount = 0;
            m_LastBadLogin = DateTime.MinValue;
            m_Active = true;
            m_Roles = String.Empty;
            m_SkinID = AppLogic.GetStoreSkinID(StoreID);

            if (rs != null && rs.Read())
            {
                m_HasCustomerRecord = true;
                m_CustomerID = DB.RSFieldInt(rs, "CustomerID");
                m_AffiliateID = DB.RSFieldInt(rs, "AffiliateID");
                m_CurrencySetting = DB.RSField(rs, "CurrencySetting");
                m_LocaleSetting = DB.RSField(rs, "LocaleSetting");
                m_VATSetting = (VATSettingEnum)DB.RSFieldInt(rs, "VATSetting");
                m_VATRegistrationID = DB.RSField(rs, "VATRegistrationID");
                m_CreatedOn = DB.RSFieldDateTime(rs, "CreatedOn");
                m_Notes = DB.RSField(rs, "Notes");
                m_Phone = DB.RSField(rs, "Phone");
                m_EMail = DB.RSField(rs, "EMail");
                m_DateOfBirth = DB.RSFieldDateTime(rs, "DateOfBirth");
                m_Over13 = DB.RSFieldBool(rs, "Over13Checked");
                m_OKToEMail = DB.RSFieldBool(rs, "OKToEMail");
                m_CODCompanyCheckAllowed = DB.RSFieldBool(rs, "CODCompanyCheckAllowed");
                m_CODNet30Allowed = DB.RSFieldBool(rs, "CODNet30Allowed");
                m_CouponCode = DB.RSField(rs, "CouponCode");
                m_GiftRegistryGUID = DB.RSFieldGUID(rs, "GiftRegistryGUID");
                m_GiftRegistryIsAnonymous = DB.RSFieldBool(rs, "GiftRegistryIsAnonymous");
                m_GiftRegistryAllowSearchByOthers = DB.RSFieldBool(rs, "GiftRegistryAllowSearchByOthers");
                m_GiftRegistryNickName = DB.RSField(rs, "GiftRegistryNickName");
                m_GiftRegistryHideShippingAddresses = DB.RSFieldBool(rs, "GiftRegistryHideShippingAddresses");
                m_Password = DB.RSField(rs, "Password"); // retreive hashed and salted pwd
                m_SaltKey = DB.RSFieldInt(rs, "SaltKey");
                m_IsRegistered = DB.RSFieldBool(rs, "IsRegistered");
                m_FirstName = DB.RSField(rs, "FirstName");
                m_LastName = DB.RSField(rs, "LastName");
                m_IsAdminUser = DB.RSFieldBool(rs, "IsAdmin");
                m_IsAdminSuperUser = DB.RSFieldBool(rs, "IsSuperAdmin");
                m_CustomerLevelID = DB.RSFieldInt(rs, "CustomerLevelID");
                m_LevelDiscountPct = DB.RSFieldDecimal(rs, "LevelDiscountPercent");
                m_DiscountExtendedPrices = DB.RSFieldBool(rs, "LevelDiscountsApplyToExtendedPrices");
                m_CustomerGUID = DB.RSFieldGUID(rs, "CustomerGUID");
                m_MicroPayBalance = DB.RSFieldDecimal(rs, "MicroPayBalance");
                m_SubscriptionExpiresOn = DB.RSFieldDateTime(rs, "SubscriptionExpiresOn");
                m_PrimaryBillingAddressID = DB.RSFieldInt(rs, "BillingAddressID");
                m_PrimaryShippingAddressID = DB.RSFieldInt(rs, "ShippingAddressID");
                m_CustomerLevelName = GetCustomerLevelName(m_CustomerLevelID, m_LocaleSetting);
                m_RecurringShippingMethodID = DB.RSFieldInt(rs, "RecurringShippingMethodID");
                m_RecurringShippingMethod = DB.RSField(rs, "RecurringShippingMethod");
                m_CurrentSessionID = DB.RSFieldInt(rs, "CustomerSessionID");
                m_StoreCCInDB = DB.RSFieldBool(rs, "StoreCCInDB");
                m_LockedUntil = DB.RSFieldDateTime(rs, "LockedUntil");
                m_AdminCanViewCC = DB.RSFieldBool(rs, "AdminCanViewCC");
                m_LastActivity = DB.RSFieldDateTime(rs, "LastActivity");
                m_PwdChanged = DB.RSFieldDateTime(rs, "PwdChanged");
                m_BadLoginCount = DB.RSFieldByte(rs, "BadLoginCount");
                m_LastBadLogin = DB.RSFieldDateTime(rs, "LastBadLogin");
                m_Active = DB.RSFieldBool(rs, "Active");
                m_PwdChgRequired = DB.RSFieldBool(rs, "PwdChangeRequired");
                m_RequestedPaymentMethod = DB.RSField(rs, "RequestedPaymentMethod");
                StoreID = DB.RSFieldInt(rs, "StoreID");
                StoreName = DB.RSField(rs, "StoreName");
                m_LastIPAddress = DB.RSField(rs, "LastIPAddress");

                //Find Failed Transactions
                using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
                {
                    conn.Open();

                    SqlParameter sp1 = new SqlParameter("@CustomerID", m_CustomerID);
                    SqlParameter[] spa = { sp1 };

                    m_FailedTransactionCount = DB.ExecuteStoredProcInt("[dbo].[aspdnsf_getFailedTransactionCount]", spa);

                    conn.Close();
                    conn.Dispose();
                }

                //Get Roles
                if (IsRegistered)
                {
                    m_Roles += "Free";
                }

                // Admins and super users rule!
                if (IsAdminUser)
                {
                    m_Roles += CommonLogic.IIF(m_Roles.Length == 0, "", ",") + "Admin";
                }
                if (IsAdminSuperUser)
                {
                    m_Roles += CommonLogic.IIF(m_Roles.Length == 0, "", ",") + "SuperAdmin";
                }

                //Check Subscriber Expiration
                if (SubscriptionExpiresOn.AddDays((double)AppLogic.AppConfigNativeInt("SubscriptionExpiredGracePeriod")).CompareTo(DateTime.Now) > 0)
                {
                    m_Roles += CommonLogic.IIF(m_Roles.Length == 0, "", ",") + "Subscriber";
                }
                else if (SubscriptionExpiresOn == System.DateTime.MinValue)
                {
                    m_Roles += CommonLogic.IIF(m_Roles.Length == 0, "", ",") + "NoSubscription";
                }
                else
                {
                    m_Roles += CommonLogic.IIF(m_Roles.Length == 0, "", ",") + "SubscriptionExpired";
                }

                if (IsRegistered)
                {
                    m_Roles += CommonLogic.IIF(m_Roles.Length == 0, "", ",") + "Registered";
                }
                else
                {
                    m_Roles += CommonLogic.IIF(m_Roles.Length == 0, "", ",") + "NotRegistered";
                }

                if (AppLogic.IPIsRestricted(LastIPAddress))
                {
                    m_Roles += CommonLogic.IIF(m_Roles.Length == 0, "", ",") + "IPRestricted";
                }

                if (CustomerLevelID != 0)
                {
                    m_Roles += CommonLogic.IIF(m_Roles.Length == 0, "", ",") + CustomerLevelName.Trim();
                }

                // only set dynamic product roles if NOT running via WSI:
                if (m_DBTrans == null)
                {
                    string sql = "select Name Role from CustomerLevel  with (NOLOCK)  where CustomerLevelID=" + this.CustomerLevelID.ToString();
                    sql += " UNION ";
                    sql += "select OrderedProductSKU Role from orders_ShoppingCart os  with (NOLOCK)  join Orders o  with (NOLOCK)  on o.OrderNumber= os.OrderNumber where o.CustomerID=" + this.CustomerID.ToString() + " and TransactionState=" + DB.SQuote(AppLogic.ro_TXStateCaptured);

                    Hashtable ht = new Hashtable();

                    SqlConnection con = null;
                    IDataReader dr = null;
                    try
                    {
                        string query = sql;
                        if (m_DBTrans != null)
                        {
                            // if a transaction was passed, we should use the transaction objects connection
                            dr = DB.GetRS(query, m_DBTrans);
                        }
                        else
                        {
                            // otherwise create it
                            con = new SqlConnection(DB.GetDBConn());
                            con.Open();
                            dr = DB.GetRS(query, con);
                        }

                        using (dr)
                        {
                            while (dr.Read())
                            {
                                string role = DB.RSField(dr, "Role").Trim();
                                if (role != "" && !ht.ContainsKey(role))
                                {
                                    m_Roles += CommonLogic.IIF(m_Roles.Length == 0, "", ",") + role;
                                    ht.Add(role, "true");
                                }
                            }
                        }
                    }
                    catch { throw; }
                    finally
                    {
                        // we can't dispose of the connection if it's part of a transaction
                        if (con != null && m_DBTrans == null)
                        {
                            // here it's safe to dispose since we created the connection ourself
                            con.Dispose();
                        }

                        // make sure we won't reference this again in code
                        dr = null;
                        con = null;
                    }


                }
            }

            if (AppLogic.IsAdminSite)
            {
                m_SkinID = 1; // forced!!
            }

            this.EnsurePublishedCurrency(m_CurrencySetting);
            SetDefaultCustomerLevelData();
        }

        private void EnsurePublishedCurrency(string CurrentCurrency)
        {
            ArrayList publishedCurrencies = Currency.getCurrencyList();
            foreach (ListItemClass item in publishedCurrencies)
                if (CurrentCurrency.Equals(Currency.GetCurrencyCode(item.Value), StringComparison.InvariantCultureIgnoreCase))
                    return;

            String DefaultCurrency = AppLogic.GetLocaleDefaultCurrency(this.LocaleSetting);

            this.UpdateCustomer(new SqlParameter[] { new SqlParameter("CurrencySetting", DefaultCurrency) });
            m_CurrencySetting = DefaultCurrency;
        }

        private void SetDefaultCustomerLevelData()
        {
            string sql = "select * from dbo.CustomerLevel where CustomerLevelID = @CustomerLevelID";
            SqlParameter[] spa = { DB.CreateSQLParameter("@CustomerLevelID", SqlDbType.Int, 4, AppLogic.AppConfigUSInt("DefaultCustomerLevelID"), ParameterDirection.Input) };

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader dr = DB.GetRS(sql, spa, dbconn))
                {
                    if (dr.Read())
                    {
                        m_DefaultCustLevel_DiscountExtendedPrices = DB.RSFieldBool(dr, "LevelDiscountsApplyToExtendedPrices");
                        m_DefaultCustLevel_LevelDiscountPct = DB.RSFieldDecimal(dr, "LevelDiscountPercent");
                        m_DefaultCustLevel_CustomerLevelName = DB.RSFieldByLocale(dr, "Name", Localization.GetDefaultLocale());
                    }
                }
            }
        }

        /// <summary>
        /// Initilizes the specified customer ID.
        /// </summary>
        /// <param name="CustomerID">The customer ID.</param>
        private void Init(int CustomerID)
        {
            String sql = "exec aspdnsf_GetCustomerByID " + CustomerID.ToString();

            SqlConnection con = null;
            IDataReader rs = null;
            try
            {
                string query = sql;
                if (m_DBTrans != null)
                {
                    // if a transaction was passed, we should use the transaction objects connection
                    rs = DB.GetRS(query, m_DBTrans);
                }
                else
                {
                    // otherwise create it
                    con = new SqlConnection(DB.GetDBConn());
                    con.Open();
                    rs = DB.GetRS(query, con);
                }

                using (rs)
                {
                    Init(rs);
                }
            }
            catch { throw; }
            finally
            {
                // we can't dispose of the connection if it's part of a transaction
                if (con != null && m_DBTrans == null)
                {
                    // here it's safe to dispose since we created the connection ourself
                    con.Dispose();
                }

                // make sure we won't reference this again in code
                rs = null;
                con = null;
            }



        }

        /// <summary>
        /// Initilizes the Customer by the specified customer GUID.
        /// </summary>
        /// <param name="CustomerGuid">The customer GUID.</param>
        private void Init(Guid CustomerGuid)
        {
            String sql = "exec dbo.aspdnsf_GetCustomerByGUID " + DB.SQuote(CustomerGuid.ToString());

            SqlConnection con = null;
            IDataReader rs = null;
            try
            {
                string query = sql;
                if (m_DBTrans != null)
                {
                    // if a transaction was passed, we should use the transaction objects connection
                    rs = DB.GetRS(query, m_DBTrans);
                }
                else
                {
                    // otherwise create it
                    con = new SqlConnection(DB.GetDBConn());
                    con.Open();
                    rs = DB.GetRS(query, con);
                }

                using (rs)
                {
                    Init(rs);
                }
            }
            catch { throw; }
            finally
            {
                // we can't dispose of the connection if it's part of a transaction
                if (con != null && m_DBTrans == null)
                {
                    // here it's safe to dispose since we created the connection ourself
                    con.Dispose();
                }

                // make sure we won't reference this again in code
                rs = null;
                con = null;
            }


        }

        /// <summary>
        /// Initilizes the Customer by the specified customer GUID.
        /// </summary>
        /// <param name="email">The email.</param>
        private void Init(string email, bool adminOnly)
        {
            String sql = string.Format("exec dbo.aspdnsf_GetCustomerByEmail {0},{1},{2},{3}", DB.SQuote(email), CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowCustomerFiltering") == true, 1, 0), AppLogic.StoreID(), CommonLogic.IIF(adminOnly, 1, 0));

            SqlConnection con = null;
            IDataReader rs = null;
            try
            {
                string query = sql;
                if (m_DBTrans != null)
                {
                    // if a transaction was passed, we should use the transaction objects connection
                    rs = DB.GetRS(query, m_DBTrans);
                }
                else
                {
                    // otherwise create it
                    con = new SqlConnection(DB.GetDBConn());
                    con.Open();
                    rs = DB.GetRS(query, con);
                }

                using (rs)
                {
                    Init(rs);
                }
            }
            catch { throw; }
            finally
            {
                // we can't dispose of the connection if it's part of a transaction
                if (con != null && m_DBTrans == null)
                {
                    // here it's safe to dispose since we created the connection ourself
                    con.Dispose();
                }

                // make sure we won't reference this again in code
                rs = null;
                con = null;
            }


        }


        /// <summary>
        /// Refreshes this instance.
        /// </summary>
        private void refresh()
        {
            String sql = "exec aspdnsf_GetCustomerByID " + m_CustomerID.ToString();

            SqlConnection con = null;
            IDataReader rs = null;
            try
            {
                string query = sql;
                if (m_DBTrans != null)
                {
                    // if a transaction was passed, we should use the transaction objects connection
                    rs = DB.GetRS(query, m_DBTrans);
                }
                else
                {
                    // otherwise create it
                    con = new SqlConnection(DB.GetDBConn());
                    con.Open();
                    rs = DB.GetRS(query, con);
                }

                using (rs)
                {
                    Init(rs);
                }
            }
            catch { throw; }
            finally
            {
                // we can't dispose of the connection if it's part of a transaction
                if (con != null && m_DBTrans == null)
                {
                    // here it's safe to dispose since we created the connection ourself
                    con.Dispose();
                }

                // make sure we won't reference this again in code
                rs = null;
                con = null;
            }


        }

        #endregion


        #region Public Methods

        /// <summary>
        /// Updates the customer.
        /// </summary>
        /// <param name="spa">The sql parameter.</param>
        /// <returns></returns>
        public string UpdateCustomer(SqlParameter[] spa)
        {
            return Customer.UpdateCustomerStatic(m_CustomerID, spa);
        }

        /// <summary>
        /// Updates the customer.
        /// </summary>
        /// <param name="CustomerLevelID">The customer level ID.</param>
        /// <param name="EMail">The E mail.</param>
        /// <param name="SaltedAndHashedPassword">The salted and hashed password.</param>
        /// <param name="SaltKey">The salt key.</param>
        /// <param name="DateOfBirth">The date of birth.</param>
        /// <param name="Gender">The gender.</param>
        /// <param name="FirstName">The first name.</param>
        /// <param name="LastName">The last name.</param>
        /// <param name="Notes">The notes.</param>
        /// <param name="SkinID">The skinID.</param>
        /// <param name="Phone">The phone.</param>
        /// <param name="AffiliateID">The affiliate ID.</param>
        /// <param name="Referrer">The referrer.</param>
        /// <param name="CouponCode">The coupon code.</param>
        /// <param name="OkToEmail">Is ok to email.</param>
        /// <param name="IsAdmin">The is admin.</param>
        /// <param name="BillingEqualsShipping">Is billing equals shipping.</param>
        /// <param name="LastIPAddress">The last IP address.</param>
        /// <param name="OrderNotes">The order notes.</param>
        /// <param name="SubscriptionExpiresOn">The subscription expires on.</param>
        /// <param name="RTShipRequest">The RTship request.</param>
        /// <param name="RTShipResponse">The RTship response.</param>
        /// <param name="OrderOptions">The order options.</param>
        /// <param name="LocaleSetting">The locale setting.</param>
        /// <param name="MicroPayBalance">The micropay balance.</param>
        /// <param name="RecurringShippingMethodID">The recurring shipping method ID.</param>
        /// <param name="RecurringShippingMethod">The recurring shipping method.</param>
        /// <param name="BillingAddressID">The billing address ID.</param>
        /// <param name="ShippingAddressID">The shipping address ID.</param>
        /// <param name="GiftRegistryGUID">The gift registry GUID.</param>
        /// <param name="GiftRegistryIsAnonymous">The gift registry is anonymous.</param>
        /// <param name="GiftRegistryAllowSearchByOthers">The gift registry allow search by others.</param>
        /// <param name="GiftRegistryNickName">Name of the gift registry nick.</param>
        /// <param name="GiftRegistryHideShippingAddresses">The gift registry hide shipping addresses.</param>
        /// <param name="CODCompanyCheckAllowed">The COD company check allowed.</param>
        /// <param name="CODNet30Allowed">The COD net30 allowed.</param>
        /// <param name="ExtensionData">The extension data.</param>
        /// <param name="FinalizationData">The finalization data.</param>
        /// <param name="Deleted">Is deleted.</param>
        /// <param name="Over13Checked">Is over13 checked.</param>
        /// <param name="CurrencySetting">The currency setting.</param>
        /// <param name="VATSetting">The VAT setting.</param>
        /// <param name="VATRegistrationID">The VAT registration ID.</param>
        /// <param name="StoreCCInDB">Store Credit Card in DataBase.</param>
        /// <param name="IsRegistered">Is registered.</param>
        /// <param name="LockedUntil">Is locked until.</param>
        /// <param name="AdminCanViewCC">The admin can view CC.</param>
        /// <param name="BadLogin">Only pass value -1, 0, 1:  -1 clears bad login count, zero does nothing, 1 increments the count</param>
        /// <param name="Active">The active.</param>
        /// <param name="PwdChangeRequired">The password change required.</param>
        /// <param name="RegisterDate">The register date.</param>
        /// <returns></returns>
        public string UpdateCustomer(
            object CustomerLevelID,
            string EMail,
            string SaltedAndHashedPassword,
            object SaltKey,
            string DateOfBirth,
            string Gender,
            string FirstName,
            string LastName,
            string Notes,
            object SkinID,
            string Phone,
            object AffiliateID,
            string Referrer,
            string CouponCode,
            object OkToEmail,
            object IsAdmin,
            object BillingEqualsShipping,
            string LastIPAddress,
            string OrderNotes,
            string SubscriptionExpiresOn,
            string RTShipRequest,
            string RTShipResponse,
            string OrderOptions,
            string LocaleSetting,
            object MicroPayBalance,
            object RecurringShippingMethodID,
            string RecurringShippingMethod,
            object BillingAddressID,
            object ShippingAddressID,
            string GiftRegistryGUID,
            object GiftRegistryIsAnonymous,
            object GiftRegistryAllowSearchByOthers,
            string GiftRegistryNickName,
            object GiftRegistryHideShippingAddresses,
            object CODCompanyCheckAllowed,
            object CODNet30Allowed,
            string ExtensionData,
            string FinalizationData,
            object Deleted,
            object Over13Checked,
            string CurrencySetting,
            object VATSetting,
            string VATRegistrationID,
            object StoreCCInDB,
            object IsRegistered,
            object LockedUntil,
            object AdminCanViewCC,
            object BadLogin,
            object Active,
            object PwdChangeRequired,
            object RegisterDate,
            object StoreId
            )
        {
            AppLogic.eventHandler("UpdateCustomer").CallEvent("&UpdateCustomer=true&UpdatedCustomerID=" + CustomerID.ToString());

            string err = String.Empty;
            SqlConnection cn = new SqlConnection(DB.GetDBConn());
            cn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = cn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "dbo.aspdnsf_updCustomer";

            if (SaltKey != null && (int)SaltKey == Security.ro_SaltKeyIsInvalid)
            {
                SaltKey = null;
            }

            cmd.Parameters.Add(new SqlParameter("@CustomerID", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@RegisterDate", SqlDbType.VarChar, 30));
            cmd.Parameters.Add(new SqlParameter("@CustomerLevelID", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@Password", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@SaltKey", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@DateOfBirth", SqlDbType.VarChar, 30));
            cmd.Parameters.Add(new SqlParameter("@Gender", SqlDbType.NVarChar, 2));
            cmd.Parameters.Add(new SqlParameter("@FirstName", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@LastName", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@Notes", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@SkinID", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@Phone", SqlDbType.NVarChar, 50));
            cmd.Parameters.Add(new SqlParameter("@AffiliateID", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@Referrer", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@CouponCode", SqlDbType.NVarChar, 100));
            cmd.Parameters.Add(new SqlParameter("@OkToEmail", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@IsAdmin", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@BillingEqualsShipping", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@LastIPAddress", SqlDbType.VarChar, 40));
            cmd.Parameters.Add(new SqlParameter("@OrderNotes", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@SubscriptionExpiresOn", SqlDbType.VarChar, 30));
            cmd.Parameters.Add(new SqlParameter("@RTShipRequest", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@RTShipResponse", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@OrderOptions", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@LocaleSetting", SqlDbType.NVarChar, 20));
            cmd.Parameters.Add(new SqlParameter("@MicroPayBalance", SqlDbType.Decimal, 8));
            cmd.Parameters.Add(new SqlParameter("@RecurringShippingMethodID", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@RecurringShippingMethod", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@BillingAddressID", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@ShippingAddressID", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@GiftRegistryGUID", SqlDbType.VarChar, 40));
            cmd.Parameters.Add(new SqlParameter("@GiftRegistryIsAnonymous", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@GiftRegistryAllowSearchByOthers", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@GiftRegistryNickName", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@GiftRegistryHideShippingAddresses", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@CODCompanyCheckAllowed", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@CODNet30Allowed", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@ExtensionData", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@FinalizationData", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@Deleted", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@Over13Checked", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@CurrencySetting", SqlDbType.NVarChar, 20));
            cmd.Parameters.Add(new SqlParameter("@VATSetting", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@VATRegistrationID", SqlDbType.NVarChar, 100));
            cmd.Parameters.Add(new SqlParameter("@StoreCCInDB", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@IsRegistered", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@LockedUntil", SqlDbType.VarChar, 30));
            cmd.Parameters.Add(new SqlParameter("@AdminCanViewCC", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@BadLogin", SqlDbType.SmallInt, 2));
            cmd.Parameters.Add(new SqlParameter("@Active", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@PwdChangeRequired", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@StoreID", SqlDbType.Int, 4));

            cmd.Parameters["@CustomerID"].Value = this.m_CustomerID;

            if (CustomerLevelID == null) cmd.Parameters["@CustomerLevelID"].Value = DBNull.Value;
            else cmd.Parameters["@CustomerLevelID"].Value = CustomerLevelID;

            if (RegisterDate == null) cmd.Parameters["@RegisterDate"].Value = DBNull.Value;
            else cmd.Parameters["@RegisterDate"].Value = RegisterDate;

            if (EMail == null) cmd.Parameters["@Email"].Value = DBNull.Value;
            else cmd.Parameters["@Email"].Value = EMail;

            if (SaltedAndHashedPassword == null) cmd.Parameters["@Password"].Value = DBNull.Value;
            else cmd.Parameters["@Password"].Value = SaltedAndHashedPassword;

            if (SaltKey == null) cmd.Parameters["@SaltKey"].Value = DBNull.Value;
            else cmd.Parameters["@SaltKey"].Value = SaltKey;

            if (DateOfBirth == null) cmd.Parameters["@DateOfBirth"].Value = DBNull.Value;
            else cmd.Parameters["@DateOfBirth"].Value = DateOfBirth;

            if (Gender == null) cmd.Parameters["@Gender"].Value = DBNull.Value;
            else cmd.Parameters["@Gender"].Value = Gender;

            if (FirstName == null) cmd.Parameters["@FirstName"].Value = DBNull.Value;
            else cmd.Parameters["@FirstName"].Value = FirstName;

            if (LastName == null) cmd.Parameters["@LastName"].Value = DBNull.Value;
            else cmd.Parameters["@LastName"].Value = LastName;

            if (Notes == null) cmd.Parameters["@Notes"].Value = DBNull.Value;
            else cmd.Parameters["@Notes"].Value = Notes;

            if (SkinID == null) cmd.Parameters["@SkinID"].Value = DBNull.Value;
            else cmd.Parameters["@SkinID"].Value = SkinID;

            if (Phone == null) cmd.Parameters["@Phone"].Value = DBNull.Value;
            else cmd.Parameters["@Phone"].Value = Phone;

            if (AffiliateID == null) cmd.Parameters["@AffiliateID"].Value = DBNull.Value;
            else cmd.Parameters["@AffiliateID"].Value = AffiliateID;

            if (Referrer == null) cmd.Parameters["@Referrer"].Value = DBNull.Value;
            else cmd.Parameters["@Referrer"].Value = Referrer;

            if (CouponCode == null) cmd.Parameters["@CouponCode"].Value = DBNull.Value;
            else cmd.Parameters["@CouponCode"].Value = CouponCode;

            if (OkToEmail == null) cmd.Parameters["@OkToEmail"].Value = DBNull.Value;
            else cmd.Parameters["@OkToEmail"].Value = OkToEmail;

            if (IsAdmin == null) cmd.Parameters["@IsAdmin"].Value = DBNull.Value;
            else cmd.Parameters["@IsAdmin"].Value = IsAdmin;

            if (BillingEqualsShipping == null) cmd.Parameters["@BillingEqualsShipping"].Value = DBNull.Value;
            else cmd.Parameters["@BillingEqualsShipping"].Value = BillingEqualsShipping;

            if (LastIPAddress == null) cmd.Parameters["@LastIPAddress"].Value = DBNull.Value;
            else cmd.Parameters["@LastIPAddress"].Value = LastIPAddress;

            if (OrderNotes == null) cmd.Parameters["@OrderNotes"].Value = DBNull.Value;
            else cmd.Parameters["@OrderNotes"].Value = OrderNotes;

            if (SubscriptionExpiresOn == null) cmd.Parameters["@SubscriptionExpiresOn"].Value = DBNull.Value;
            else cmd.Parameters["@SubscriptionExpiresOn"].Value = SubscriptionExpiresOn;

            if (RTShipRequest == null) cmd.Parameters["@RTShipRequest"].Value = DBNull.Value;
            else cmd.Parameters["@RTShipRequest"].Value = RTShipRequest;

            if (RTShipResponse == null) cmd.Parameters["@RTShipResponse"].Value = DBNull.Value;
            else cmd.Parameters["@RTShipResponse"].Value = RTShipResponse;

            if (OrderOptions == null) cmd.Parameters["@OrderOptions"].Value = DBNull.Value;
            else cmd.Parameters["@OrderOptions"].Value = OrderOptions;

            if (LocaleSetting == null) cmd.Parameters["@LocaleSetting"].Value = DBNull.Value;
            else cmd.Parameters["@LocaleSetting"].Value = LocaleSetting;

            if (MicroPayBalance == null) cmd.Parameters["@MicroPayBalance"].Value = DBNull.Value;
            else cmd.Parameters["@MicroPayBalance"].Value = MicroPayBalance;

            if (RecurringShippingMethodID == null) cmd.Parameters["@RecurringShippingMethodID"].Value = DBNull.Value;
            else cmd.Parameters["@RecurringShippingMethodID"].Value = RecurringShippingMethodID;

            if (RecurringShippingMethod == null) cmd.Parameters["@RecurringShippingMethod"].Value = DBNull.Value;
            else cmd.Parameters["@RecurringShippingMethod"].Value = RecurringShippingMethod;

            if (BillingAddressID == null) cmd.Parameters["@BillingAddressID"].Value = DBNull.Value;
            else cmd.Parameters["@BillingAddressID"].Value = BillingAddressID;

            if (ShippingAddressID == null) cmd.Parameters["@ShippingAddressID"].Value = DBNull.Value;
            else cmd.Parameters["@ShippingAddressID"].Value = ShippingAddressID;

            if (GiftRegistryGUID == null) cmd.Parameters["@GiftRegistryGUID"].Value = DBNull.Value;
            else cmd.Parameters["@GiftRegistryGUID"].Value = GiftRegistryGUID;

            if (GiftRegistryIsAnonymous == null) cmd.Parameters["@GiftRegistryIsAnonymous"].Value = DBNull.Value;
            else cmd.Parameters["@GiftRegistryIsAnonymous"].Value = GiftRegistryIsAnonymous;

            if (GiftRegistryAllowSearchByOthers == null) cmd.Parameters["@GiftRegistryAllowSearchByOthers"].Value = DBNull.Value;
            else cmd.Parameters["@GiftRegistryAllowSearchByOthers"].Value = GiftRegistryAllowSearchByOthers;

            if (GiftRegistryNickName == null) cmd.Parameters["@GiftRegistryNickName"].Value = DBNull.Value;
            else cmd.Parameters["@GiftRegistryNickName"].Value = GiftRegistryNickName;

            if (GiftRegistryHideShippingAddresses == null) cmd.Parameters["@GiftRegistryHideShippingAddresses"].Value = DBNull.Value;
            else cmd.Parameters["@GiftRegistryHideShippingAddresses"].Value = GiftRegistryHideShippingAddresses;

            if (CODCompanyCheckAllowed == null) cmd.Parameters["@CODCompanyCheckAllowed"].Value = DBNull.Value;
            else cmd.Parameters["@CODCompanyCheckAllowed"].Value = CODCompanyCheckAllowed;

            if (CODNet30Allowed == null) cmd.Parameters["@CODNet30Allowed"].Value = DBNull.Value;
            else cmd.Parameters["@CODNet30Allowed"].Value = CODNet30Allowed;

            if (ExtensionData == null) cmd.Parameters["@ExtensionData"].Value = DBNull.Value;
            else cmd.Parameters["@ExtensionData"].Value = ExtensionData;

            if (FinalizationData == null) cmd.Parameters["@FinalizationData"].Value = DBNull.Value;
            else cmd.Parameters["@FinalizationData"].Value = FinalizationData;

            if (Deleted == null) cmd.Parameters["@Deleted"].Value = DBNull.Value;
            else cmd.Parameters["@Deleted"].Value = Deleted;

            if (Over13Checked == null) cmd.Parameters["@Over13Checked"].Value = DBNull.Value;
            else cmd.Parameters["@Over13Checked"].Value = Over13Checked;

            if (CurrencySetting == null) cmd.Parameters["@CurrencySetting"].Value = DBNull.Value;
            else cmd.Parameters["@CurrencySetting"].Value = CurrencySetting;

            if (VATSetting == null) cmd.Parameters["@VATSetting"].Value = DBNull.Value;
            else cmd.Parameters["@VATSetting"].Value = VATSetting;

            if (VATRegistrationID == null) cmd.Parameters["@VATRegistrationID"].Value = DBNull.Value;
            else cmd.Parameters["@VATRegistrationID"].Value = VATRegistrationID;

            if (StoreCCInDB == null) cmd.Parameters["@StoreCCInDB"].Value = DBNull.Value;
            else cmd.Parameters["@StoreCCInDB"].Value = StoreCCInDB;

            if (IsRegistered == null) cmd.Parameters["@IsRegistered"].Value = DBNull.Value;
            else cmd.Parameters["@IsRegistered"].Value = IsRegistered;

            if (LockedUntil == null || !CommonLogic.IsDate(LockedUntil.ToString())) cmd.Parameters["@LockedUntil"].Value = DBNull.Value;
            else cmd.Parameters["@LockedUntil"].Value = Localization.ToDBDateTimeString((DateTime)LockedUntil);

            if (AdminCanViewCC == null) cmd.Parameters["@AdminCanViewCC"].Value = DBNull.Value;
            else cmd.Parameters["@AdminCanViewCC"].Value = AdminCanViewCC;

            if (BadLogin == null)
            {
                cmd.Parameters["@BadLogin"].Value = 0;
            }
            else
            {
                if (Convert.ToInt16(BadLogin) < -1 || Convert.ToInt16(BadLogin) > 1) BadLogin = 0;
                cmd.Parameters["@BadLogin"].Value = BadLogin;
            }

            if (Active == null | (Convert.ToInt16(Active) != 0 && Convert.ToInt16(Active) != 1)) cmd.Parameters["@Active"].Value = DBNull.Value;
            else cmd.Parameters["@Active"].Value = Active;

            if (PwdChangeRequired == null) cmd.Parameters["@PwdChangeRequired"].Value = DBNull.Value;
            else cmd.Parameters["@PwdChangeRequired"].Value = PwdChangeRequired;

            if (StoreId == null) cmd.Parameters["@StoreID"].Value = DBNull.Value;
            else cmd.Parameters["@StoreID"].Value = StoreId;

            try
            {
                cmd.ExecuteNonQuery();
                refresh();
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

        /// <summary>
        /// Validates the primary addresses.
        /// </summary>
        public void ValidatePrimaryAddresses()
        {
            // sanity checker on address book for customer (invoked on account page):
            if (PrimaryBillingAddressID != 0)
            {
                bool PrimaryBillingAddressFound = (DB.GetSqlN(String.Format("select count(*) as N from Address with (NOLOCK) where CustomerID={0} and AddressID={1}", CustomerID.ToString(), PrimaryBillingAddressID.ToString())) > 0);
                if (!PrimaryBillingAddressFound)
                {
                    int AlternateBillingAddressID = 0;
                    // try to find ANY other customer address (that has credit card info) to use in place of the one being deleted, if required:

                    using (SqlConnection dbconn = DB.dbConn())
                    {
                        dbconn.Open();
                        using (IDataReader rs = DB.GetRS(String.Format("select top 1 AddressID from Address with (NOLOCK) where CardNumber IS NOT NULL and CustomerID={0}", CustomerID.ToString()), dbconn))
                        {
                            if (rs.Read())
                            {
                                AlternateBillingAddressID = DB.RSFieldInt(rs, "AddressID");
                            }
                        }
                    }
                    int BackupAddressID = 0;
                    // try to find ANY other customer address as further backup, if required:

                    using (SqlConnection dbconn = DB.dbConn())
                    {
                        dbconn.Open();
                        using (IDataReader rs = DB.GetRS(String.Format("select top 1 AddressID from Address with (NOLOCK) where CustomerID={0}", CustomerID.ToString()), dbconn))
                        {
                            if (rs.Read())
                            {
                                BackupAddressID = DB.RSFieldInt(rs, "AddressID");
                            }
                        }
                    }
                    if (AlternateBillingAddressID == 0)
                    {
                        AlternateBillingAddressID = BackupAddressID;
                    }

                    DB.ExecuteSQL(String.Format("update Customer set BillingAddressID={0} where CustomerID={1}", AlternateBillingAddressID.ToString(), CustomerID.ToString()));
                }
            }

            if (PrimaryShippingAddressID != 0)
            {
                bool PrimaryShippingAddressFound = (DB.GetSqlN(String.Format("select count(*) as N from Address with (NOLOCK) where CustomerID={0} and AddressID={1}", CustomerID.ToString(), PrimaryShippingAddressID.ToString())) > 0);
                if (!PrimaryShippingAddressFound)
                {
                    int AlternateShippingAddressID = 0;
                    // try to find ANY other customer address (that does not have credit card info) to use in place of the one being deleted, if required:

                    using (SqlConnection dbconn = DB.dbConn())
                    {
                        dbconn.Open();
                        using (IDataReader rs = DB.GetRS(String.Format("select top 1 AddressID from Address with (NOLOCK) where CardNumber IS NULL and CustomerID={0}", CustomerID.ToString()), dbconn))
                        {
                            if (rs.Read())
                            {
                                AlternateShippingAddressID = DB.RSFieldInt(rs, "AddressID");
                            }
                        }
                    }
                    int BackupAddressID = 0;
                    // try to find ANY other customer address as further backup, if required:

                    using (SqlConnection dbconn = DB.dbConn())
                    {
                        dbconn.Open();
                        using (IDataReader rs = DB.GetRS(String.Format("select top 1 AddressID from Address with (NOLOCK) where CustomerID={0}", CustomerID.ToString()), dbconn))
                        {
                            if (rs.Read())
                            {
                                BackupAddressID = DB.RSFieldInt(rs, "AddressID");
                            }
                        }
                    }
                    if (AlternateShippingAddressID == 0)
                    {
                        AlternateShippingAddressID = BackupAddressID;
                    }

                    DB.ExecuteSQL(String.Format("update Customer set ShippingAddressID={0} where CustomerID={1}", AlternateShippingAddressID.ToString(), CustomerID.ToString()));

                    // update any cart shipping addresses (all types, regular cart, wish list, recurring cart, and gift registry) that match the one being deleted:
                    String sql = String.Format("update ShoppingCart set ShippingAddressID={0} where ShippingAddressID={1}", AlternateShippingAddressID.ToString(), PrimaryShippingAddressID.ToString());
                    DB.ExecuteSQL(sql);
                }
            }
        }

        /// <summary>
        /// Check if this customer owns the address
        /// </summary>
        /// <param name="AddressID">The address ID.</param>
        /// <returns>Returns TRUE if this customer owns the address otherwise FALSE.</returns>
        public bool OwnsThisAddress(int AddressID)
        {
            return Customer.OwnsThisAddress(CustomerID, AddressID);
        }


        /// <summary>
        /// Gets the Customer's Full Name.
        /// </summary>
        /// <returns>Returns teh Customer's Full Name.</returns>
        public String FullName()
        {
            return (m_FirstName + " " + m_LastName).Trim();
        }

        /// <summary>
        /// Determines whether [has at least one address].
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [has at least one address]; otherwise, <c>false</c>.
        /// </returns>
        public bool HasAtLeastOneAddress()
        {
            return Customer.HasAtLeastOneAddress(m_CustomerID);
        }

        /// <summary>
        /// Determines whether this instance has orders.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if this instance has orders; otherwise, <c>false</c>.
        /// </returns>
        public bool HasOrders()
        {
            return Customer.HasOrders(m_CustomerID);
        }

        /// <summary>
        /// Gets the Customer's Billing Information.
        /// </summary>
        /// <param name="separator">The separator.</param>
        /// <returns>Returns the Customer's Billing Information.</returns>
        public String BillingInformation(String separator)
        {
            return BillingInformation(m_CustomerID, separator);
        }

        /// <summary>
        /// Gets the Customer's Shipping Address.
        /// </summary>
        /// <param name="Checkout">if set to <c>true</c> [checkout].</param>
        /// <param name="IncludePhone">if set to <c>true</c> [include phone].</param>
        /// <param name="separator">The separator.</param>
        /// <returns>Returns the Customer's Shipping Address.</returns>
        public String ShipToAddress(bool Checkout, bool IncludePhone, String separator)
        {
            return ShipToAddress(Checkout, IncludePhone, m_CustomerID, separator);
        }

        /// <summary>
        /// Gets the Customer's Billing Address.
        /// </summary>
        /// <param name="Checkout">if set to <c>true</c> [checkout].</param>
        /// <param name="IncludePhone">if set to <c>true</c> [include phone].</param>
        /// <param name="separator">The separator.</param>
        /// <returns>Returns the Customer's Billing Address.</returns>
        public String BillToAddress(bool Checkout, bool IncludePhone, String separator)
        {
            return BillToAddress(Checkout, IncludePhone, m_CustomerID, separator);
        }

        /// <summary>
        /// Determines whether [has used coupon] [the specified coupon code].
        /// </summary>
        /// <param name="CouponCode">The coupon code.</param>
        /// <returns>
        /// 	<c>true</c> if [has used coupon] [the specified coupon code]; otherwise, <c>false</c>.
        /// </returns>
        public bool HasUsedCoupon(String CouponCode)
        {
            return (DB.GetSqlN("select count(ordernumber) as N from orders   with (NOLOCK)  where customerid=" + m_CustomerID.ToString() + " and lower(CouponCode)=" + DB.SQuote(CouponCode.ToLowerInvariant())) != 0);
        }

        /// <summary>
        /// Sets the locale.
        /// </summary>
        /// <param name="LocaleSetting">The locale setting.</param>
        public void SetLocale(String LocaleSetting)
        {
            LocaleSetting = Localization.CheckLocaleSettingForProperCase(LocaleSetting);
            if (LocaleSetting.Length == 0)
            {
                LocaleSetting = Localization.GetDefaultLocale();
            }

            if (m_HasCustomerRecord)
            {
                DB.ExecuteSQL("update customer set LocaleSetting=" + DB.SQuote(LocaleSetting) + " where customerid=" + m_CustomerID.ToString());
            }
            m_LocaleSetting = LocaleSetting;
        }


        /// <summary>
        /// Sets the currency.
        /// </summary>
        /// <param name="CurrencySetting">The currency setting.</param>
        public void SetCurrency(String CurrencySetting)
        {
            CurrencySetting = Localization.CheckCurrencySettingForProperCase(CurrencySetting);
            if (CurrencySetting.Length == 0)
            {
                CurrencySetting = Localization.GetPrimaryCurrency();
            }
            if (!m_SuppressCookies && !AppLogic.IsAdminSite)
            {
                HttpContext.Current.Profile.SetPropertyValue(CommonLogic.IIF(AppLogic.IsAdminSite, "Admin", "") + ro_CurrencySettingCookieName, CurrencySetting);
            }
            if (m_HasCustomerRecord)
            {
                DB.ExecuteSQL("update customer set CurrencySetting=" + DB.SQuote(CurrencySetting) + " where CustomerID=" + m_CustomerID.ToString());
            }
            m_CurrencySetting = CurrencySetting;
        }

        /// <summary>
        /// Sets the VAT setting.
        /// </summary>
        /// <param name="VATSetting">The VAT setting.</param>
        public void SetVATSetting(VATSettingEnum VATSetting)
        {

            if (m_HasCustomerRecord)
            {
                DB.ExecuteSQL("update customer set VATSetting=" + ((int)VATSetting).ToString() + " where CustomerID=" + m_CustomerID.ToString());
            }
            m_VATSetting = VATSetting;
        }

        /// <summary>
        /// Sets the VAT registration ID.
        /// </summary>
        /// <param name="VATRegistrationID">The VAT registration ID.</param>
        public void SetVATRegistrationID(String VATRegistrationID)
        {
            if (m_HasCustomerRecord)
            {
                DB.ExecuteSQL("update customer set VATRegistrationID=" + DB.SQuote(VATRegistrationID) + " where CustomerID=" + m_CustomerID.ToString());
            }
            m_VATRegistrationID = VATRegistrationID;
        }

        /// <summary>
        /// If the customer object is not based on a database record then one will be created
        /// </summary>
        public void RequireCustomerRecord()
        {
            if (!m_HasCustomerRecord)
            {
                MakeAnonCustomerRecord(out m_CustomerID, out m_CustomerGUID);

                FormsAuthentication.SetAuthCookie(m_CustomerGUID, AppLogic.AppConfigBool("Anonymous.PersistCookie"));
                HttpCookie Cookie = HttpContext.Current.Response.Cookies[FormsAuthentication.FormsCookieName];
                if (AppLogic.AppConfigBool("Anonymous.PersistCookie"))
                    Cookie.Expires = DateTime.Now.Add(new TimeSpan(1000, 0, 0, 0));
                Init(m_CustomerID); // must reload customer data now that we have a record for it
            }
        }

        /// <summary>
        /// Returns the license status of the current Customer
        /// </summary>
        /// <returns></returns>
        public bool IsLicensedUser()
        {
            if (!IsRegistered)
            {
                return false;
            }
            int N = DB.GetSqlN("select count(*) as N from Orders  with (NOLOCK)  where TransactionState=" + DB.SQuote(AppLogic.ro_TXStateCaptured) + " and CustomerID=" + m_CustomerID.ToString());
            return (N != 0);
        }

        /// <summary>
        /// Checks the password to make sure it doesn't match any of the last x passwords used where x is the value in the AppConfig NumPreviouslyUsedPwds
        /// </summary>
        /// <param name="NewClearTextPassword"></param>
        /// <returns>True is the password has been used, false otherwise</returns>
        public bool PwdPreviouslyUsed(string NewClearTextPassword)
        {
            bool pwdused = false;

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader dr = DB.GetRS("select top " + AppLogic.AppConfigNativeInt("NumPreviouslyUsedPwds").ToString() + " OldPwd, SaltKey from dbo.passwordlog where customerid = " + CustomerID.ToString() + " order by ChangeDt desc", dbconn))
                {
                    while (dr.Read())
                    {
                        Password p = new Password(NewClearTextPassword, DB.RSFieldInt(dr, "SaltKey"));
                        if (p.SaltedPassword == DB.RSField(dr, "OldPwd"))
                        {
                            pwdused = true;
                        }
                    }
                    return pwdused;
                }
            }
        }

        /// <summary>
        /// Validates the password and returns true if it matches the current password
        /// </summary>
        /// <param name="TestPwd"></param>
        /// <returns></returns>
        public bool CheckLogin(string TestPwd)
        {
            Password pwd = new Password(TestPwd, m_SaltKey);
            if (m_Password == pwd.SaltedPassword)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Clears out the customer session and clears all customer cookies
        /// </summary>
        public void Logout()
        {
            ThisCustomerSession.Clear();
            ClearAllCustomerProfile();
        }

        /// <summary>
        /// Removes all cookies created by the store
        /// </summary>
        static public void ClearAllCustomerProfile()
        {
            if (HttpContext.Current.Profile != null)
            {
                HttpContext.Current.Profile.SetPropertyValue("SiteDisclaimerAccepted", String.Empty);
            }

            String[] ClearCookieNames = {"StatsView","ViewStatsSelectedIndex","SelectedChartsView","CompareStatsBy","ChartType", 
                                          "YearCompareSelectedYear1","YearCompareSelectedYear2","MonthCompareSelectedYear1","MonthCompareSelectedYear2", 
                                          "MonthCompareSelectedMonth1","MonthCompareSelectedMonth2","WeekCompareSelectedYear1","WeekCompareSelectedYear2", 
                                          "WeekCompareSelectedMonth1","WeekCompareSelectedMonth2","WeekCompareSelectedWeek1","WeekCompareSelectedWeek2", 
                                          "CategoryFilterID","SectionFilterID","ManufacturerFilterID","DistributorFilterID","GenreFilterID","VectorFilterID", 
                                          "Master","SkinID","Toolbars","AffiliateID","VATSettingID","LocaleSetting","CurrencySetting","LastViewedEntityName", 
                                          "LastViewedEntityInstanceID","LastViewedEntityInstanceName","LATAffiliateID","SiteDisclaimerAccepted", 
                                          "AdminVATSettingID","AdminLocaleSetting","AdminCurrencySetting","Referrer" };
            foreach (String s in ClearCookieNames)
            {
                if (HttpContext.Current.Profile.GetPropertyValue(s).ToString() != string.Empty)
                {
                    HttpContext.Current.Profile.SetPropertyValue(s, string.Empty);
                }
            }
        }

        /// <summary>
        /// Returns the tax rate for the specified tax class for the Customer's curent shipping address, if the TaxCalcMode AppConfig is set to "billing" the rate is for the billing address
        /// </summary>
        /// <param name="TaxClassID">The item tax class ID</param>
        /// <returns></returns>
        public Decimal TaxRate(int TaxClassID)
        {
            if ("billing".Equals(AppLogic.AppConfig("TaxCalcMode"), StringComparison.InvariantCultureIgnoreCase))
            {
                return TaxRate(PrimaryBillingAddress, TaxClassID);
            }
            else
            {
                return TaxRate(PrimaryShippingAddress, TaxClassID);
            }
        }

        /// <summary>
        /// Returns the tax rate for the specified tax class and address
        /// </summary>
        /// <param name="useAddress">The customer address to calculate taxes for</param>
        /// <param name="TaxClassID">The item tax class id</param>
        /// <returns></returns>
        public Decimal TaxRate(Address useAddress, int TaxClassID)
        {
            if (LevelHasNoTax || IsVatExempt())
                return 0;
            else
                return Prices.TaxRate(useAddress, TaxClassID, this);
        }

        /// <summary>
        /// Sets the shipping address for all cart items for the current customer with the OldPrimaryShippingAddressID to the NewPrimaryShippingAddressID
        /// </summary>
        /// <param name="OldPrimaryShippingAddressID"></param>
        /// <param name="NewPrimaryShippingAddressID"></param>
        public void SetPrimaryShippingAddressForShoppingCart(int OldPrimaryShippingAddressID, int NewPrimaryShippingAddressID)
        {
            SetPrimaryShippingAddressForShoppingCart(m_CustomerID, OldPrimaryShippingAddressID, NewPrimaryShippingAddressID);
        }

        /// <summary>
        /// Checks if the customer level id includes free shipping
        /// </summary>
        /// <returns>returns true if level includes free shipping</returns>
        public bool LevelHasFreeShipping()
        {
            bool hasFreeShipping = false;
            if (CustomerLevelID > 0)
            {
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    string query = "SELECT LevelHasFreeShipping FROM CustomerLevel WHERE CustomerLevelID=" + this.CustomerLevelID.ToString();
                    using (IDataReader clvl = DB.GetRS(query, con))
                    {
                        while (clvl.Read())
                        {
                            hasFreeShipping = DB.RSFieldBool(clvl, "LevelHasFreeShipping");
                        }
                    }
                }
            }

            return hasFreeShipping;
        }

        /// <summary>
        /// Clears any failed transactions in the failed transactions table linked to the provided customer id
        /// </summary>
        public static void ClearFailedTransactions(int CustomerID)
        {
            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                DB.ExecuteSQL("exec [dbo].[aspdnsf_delFailedTransactionsByCustomer] @CustomerID=" + DB.SQuote(CustomerID.ToString()), conn);

                conn.Close();
                conn.Dispose();
            }
        }

        public void SetPrimaryAddress(int addressID, AddressTypes addrType)
        {
            if (!OwnsThisAddress(addressID))
            {
                return;
            }
            else
            {
                string addressField = String.Empty;

                if (addrType == AddressTypes.Billing)
                {
                    addressField = "BillingAddressID";
                    m_PrimaryBillingAddressID = addressID;
                    m_PrimaryBillingAddress = new Address(addressID);
                }
                else
                {
                    addressField = "ShippingAddressID";
                    m_PrimaryShippingAddressID = addressID;
                    m_PrimaryShippingAddress = new Address(addressID);
                }

                string sql = String.Format("UPDATE Customer SET {0} = {1} WHERE CustomerID = {2}", addressField, addressID.ToString(), m_CustomerID.ToString());

                using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
                {
                    conn.Open();

                    DB.ExecuteSQL(sql, conn);

                    conn.Close();
                    conn.Dispose();
                }
            }
        }

        #endregion


        #region Public Properties
        // uses customer's current currency setting to control output display format:
        // exchange rate WILL be applied on input amt
        // input amt is assumed to be in the store's PRIMARY CURRENCY!
        public String CurrencyString(decimal amt)
        {
            return Localization.CurrencyStringForDisplayWithExchangeRate(amt, CurrencySetting);
        }

        /// <summary>
        /// Gets the customerID.
        /// </summary>
        /// <value>The customerID.</value>
        public int CustomerID
        {
            get
            {
                return m_CustomerID;
            }
        }

        /// <summary>
        /// Gets the date the data was created on.
        /// </summary>
        /// <value>The date the data was created on.</value>
        public DateTime CreatedOn
        {
            get
            {
                return m_CreatedOn;
            }
        }

        /// <summary>
        /// Gets or sets the VAT setting RAW.
        /// </summary>
        /// <value>The VAT setting RAW.</value>
        public VATSettingEnum VATSettingRAW
        {
            get
            {
                VATSettingEnum vat = (VATSettingEnum)AppLogic.AppConfigUSInt("VAT.DefaultSetting");

                if (m_HasCustomerRecord)
                {
                    return m_VATSetting;
                }
                else if (HttpContext.Current.Profile != null)
                {
                    if (HttpContext.Current.Profile.GetPropertyValue(ro_VATSettingCookieName).ToString() != "")
                    {
                        string value = HttpContext.Current.Profile.GetPropertyValue(ro_VATSettingCookieName).ToString();
                        int iVal = 0;
                        if (int.TryParse(value, out iVal))
                        {
                            vat = (VATSettingEnum)iVal;
                        }
                    }
                }


                return vat;
            }
            set
            {
                if (m_HasCustomerRecord)
                {
                    SqlParameter sp1 = DB.CreateSQLParameter("@VATSetting", SqlDbType.Int, 4, value, ParameterDirection.Input);
                    SqlParameter[] spa = { sp1 };
                    string retval = this.UpdateCustomer(spa);
                    if (retval == string.Empty)
                    {
                        m_VATSetting = value;
                    }
                }
                if (!AppLogic.IsAdminSite)
                {
                    // the call could have been made on the BeginRequest and specified at the querystring
                    // at that point, the Profile is null.
                    // The only way to use a querystring to set this profile's property is 
                    // to go through the setvatsettting.aspx?locale={whatevervat}
                    // if the call is coming from the BeginRequest, we shouldn't set this property yet
                    // and let the code in the setvatsetting.aspx set the current customer's property
                    // at that point on the page_load of the page, the Profile is already instantiated
                    if (HttpContext.Current.Profile != null)
                    {
                        HttpContext.Current.Profile.SetPropertyValue(Customer.ro_VATSettingCookieName, ((int)value).ToString());
                    }
                }
            }
        }

        /// <summary>
        /// this is the one to USE for the customer, after we have checked all store appconfigs, and default values
        /// the VATSettingRAW is the customer's raw setting (what is in their db record!)
        /// this property gives the actual VATSetting value to use when displaying any data.
        /// </summary>
        /// <value>The VAT setting reconciled.</value>
        public VATSettingEnum VATSettingReconciled
        {
            get
            {
                VATSettingEnum xvat = (VATSettingEnum)AppLogic.AppConfigUSInt("VAT.DefaultSetting");
                if (AppLogic.AppConfigBool("VAT.AllowCustomerToChooseSetting"))
                {
                    xvat = VATSettingRAW;
                }
                return xvat;
            }
        }



        /// <summary>
        /// Gets the VAT registration ID.
        /// </summary>
        /// <value>The VAT registration ID.</value>
        public String VATRegistrationID
        {
            get
            {
                return m_VATRegistrationID;
            }
        }

        /// <summary>
        /// Gets or sets the primary billing address ID.
        /// </summary>
        /// <value>The primary billing address ID.</value>
        public int PrimaryBillingAddressID
        {
            get
            {
                return m_PrimaryBillingAddressID;
            }
            set
            {
                m_PrimaryBillingAddressID = value;
                m_PrimaryBillingAddress = new Address(m_CustomerID, AddressTypes.Billing);
                m_PrimaryBillingAddress.LoadFromDB(m_PrimaryBillingAddressID);
            }
        }

        /// <summary>
        /// Gets or sets the primary shipping address ID.
        /// </summary>
        /// <value>The primary shipping address ID.</value>
        public int PrimaryShippingAddressID
        {
            get
            {
                return m_PrimaryShippingAddressID;
            }

            set
            {
                m_PrimaryShippingAddressID = value;
                m_PrimaryShippingAddress = new Address(m_CustomerID, AddressTypes.Shipping);
                m_PrimaryShippingAddress.LoadFromDB(m_PrimaryShippingAddressID);
            }
        }

        /// <summary>
        /// Gets the date of birth.
        /// </summary>
        /// <value>The date of birth.</value>
        public DateTime DateOfBirth
        {
            get { return m_DateOfBirth; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is over13.
        /// </summary>
        /// <value><c>true</c> if this instance is over13; otherwise, <c>false</c>.</value>
        public bool IsOver13
        {
            get { return m_Over13; }
        }
        private bool _IsImpersonated = false;

        public bool IsImpersonated
        { get { return _IsImpersonated; } set { _IsImpersonated = value; } }

        public int SkinID
        {
            get
            {
                return m_SkinID;
            }
            set
            {
                m_SkinID = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is admin user.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is admin user; otherwise, <c>false</c>.
        /// </value>
        public bool IsAdminUser
        {
            get
            {
                return m_IsAdminUser;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is admin super user.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is admin super user; otherwise, <c>false</c>.
        /// </value>
        public bool IsAdminSuperUser
        {
            get
            {
                return m_IsAdminSuperUser;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [OK to E mail].
        /// </summary>
        /// <value><c>true</c> if [OK to E mail]; otherwise, <c>false</c>.</value>
        public bool OKToEMail
        {
            get
            {
                return m_OKToEMail;
            }
            set
            {
                m_OKToEMail = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether [COD company check allowed].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [COD company check allowed]; otherwise, <c>false</c>.
        /// </value>
        public bool CODCompanyCheckAllowed
        {
            get
            {
                return m_CODCompanyCheckAllowed;
            }
        }

        /// <summary>
        /// Gets a value indicating whether [COD net30 allowed].
        /// </summary>
        /// <value><c>true</c> if [COD net30 allowed]; otherwise, <c>false</c>.</value>
        public bool CODNet30Allowed
        {
            get
            {
                return m_CODNet30Allowed;
            }
        }

        /// <summary>
        /// Gets a value indicating whether [gift registry is anonymous].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [gift registry is anonymous]; otherwise, <c>false</c>.
        /// </value>
        public bool GiftRegistryIsAnonymous
        {
            get
            {
                return m_GiftRegistryIsAnonymous;
            }
        }

        /// <summary>
        /// Gets a value indicating whether [gift registry allow search by others].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [gift registry allow search by others]; otherwise, <c>false</c>.
        /// </value>
        public bool GiftRegistryAllowSearchByOthers
        {
            get
            {
                return m_GiftRegistryAllowSearchByOthers;
            }
        }

        /// <summary>
        /// Gets or sets the coupon code.
        /// </summary>
        /// <value>The coupon code.</value>
        public String CouponCode
        {
            get
            {
                return m_CouponCode;
            }
            set
            {
                m_CouponCode = value;
            }
        }

        /// <summary>
        /// Gets the name of the gift registry nick.
        /// </summary>
        /// <value>The name of the gift registry nick.</value>
        public String GiftRegistryNickName
        {
            get
            {
                return m_GiftRegistryNickName;
            }
        }

        /// <summary>
        /// Gets the gift registry GUID.
        /// </summary>
        /// <value>The gift registry GUID.</value>
        public String GiftRegistryGUID
        {
            get
            {
                return m_GiftRegistryGUID;
            }
        }

        /// <summary>
        /// Gets a value indicating whether [gift registry hide shipping addresses].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [gift registry hide shipping addresses]; otherwise, <c>false</c>.
        /// </value>
        public bool GiftRegistryHideShippingAddresses
        {
            get
            {
                return m_GiftRegistryHideShippingAddresses;
            }
        }

        /// <summary>
        /// Gets the last IP address.
        /// </summary>
        /// <value>The last IP address.</value>
        public String LastIPAddress
        {
            get
            {
                return m_LastIPAddress;
            }
        }

        /// <summary>
        /// Gets or sets the phone.
        /// </summary>
        /// <value>The phone.</value>
        public String Phone
        {
            get
            {
                return m_Phone;
            }
            set
            {
                m_Phone = value;
            }
        }

        /// <summary>
        /// Gets or sets the affiliate ID.
        /// </summary>
        /// <value>The affiliate ID.</value>
        public int AffiliateID
        {
            get
            {
                if (m_HasCustomerRecord || AppLogic.IsAdminSite)
                {
                    return m_AffiliateID;
                }
                else if (HttpContext.Current.Profile != null)
                {
                    if (HttpContext.Current.Profile.GetPropertyValue(ro_AffiliateCookieName).ToString() != "")
                    {
                        return int.Parse(HttpContext.Current.Profile.GetPropertyValue(ro_AffiliateCookieName).ToString());
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    return 0;
                }
            }
            set
            {
                if (m_HasCustomerRecord)
                {
                    SqlParameter sp1 = DB.CreateSQLParameter("@AffiliateID", SqlDbType.Int, 4, value, ParameterDirection.Input);
                    SqlParameter[] spa = { sp1 };
                    string retval = this.UpdateCustomer(spa);
                    if (retval == string.Empty)
                    {
                        m_AffiliateID = value;
                    }
                }
                if (!AppLogic.IsAdminSite)
                {
                    if (HttpContext.Current.Profile != null)
                    {
                        HttpContext.Current.Profile.SetPropertyValue(Customer.ro_AffiliateCookieName, value.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is registered.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is registered; otherwise, <c>false</c>.
        /// </value>
        public bool IsRegistered
        {
            get
            {
                return m_IsRegistered;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has customer record.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has customer record; otherwise, <c>false</c>.
        /// </value>
        public bool HasCustomerRecord
        {
            get
            {
                return m_HasCustomerRecord;
            }
        }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>The password.</value>
        public String Password
        {
            get
            {
                return m_Password;
            }
            set
            {
                m_Password = value;
            }
        }

        /// <summary>
        /// Sets the clear password.
        /// </summary>
        /// <value>The clear password.</value>
        public String ClearPassword
        {
            set
            {
                m_Password = new AspDotNetStorefrontCore.Password(value, SaltKey).SaltedPassword;
            }
        }

        private int m_storeid;
        private string m_storename;

        public int StoreID
        {
            get { return m_storeid; }
            set { m_storeid = value; }
        }

        public string StoreName
        {
            get { return m_storename; }
            set { m_storename = value; }
        }

        // returns true if the customer has any active recurring billing orders:
        /// Determines whether [has active recurring orders] [the specified count only those without subscription I ds].
        /// </summary>
        /// <param name="CountOnlyThoseWithoutSubscriptionIDs">if set to <c>true</c> [count only those without subscription I ds].</param>
        /// <returns>
        /// 	<c>true</c> if [has active recurring orders] [the specified count only those without subscription I ds]; otherwise, <c>false</c>.
        /// </returns>
        public bool HasActiveRecurringOrders(bool CountOnlyThoseWithoutSubscriptionIDs)
        {
            if (CountOnlyThoseWithoutSubscriptionIDs)
            {
                return (DB.GetSqlN("select count(*) as N from ShoppingCart  with (NOLOCK)  where RecurringSubscriptionID='' and CartType=" + ((int)CartTypeEnum.RecurringCart).ToString() + " and CustomerID=" + CustomerID.ToString()) > 0);
            }
            else
            {
                return (DB.GetSqlN("select count(*) as N from ShoppingCart  with (NOLOCK)  where CartType=" + ((int)CartTypeEnum.RecurringCart).ToString() + " and CustomerID=" + CustomerID.ToString()) > 0);
            }
        }



        /// <summary>
        /// Inserts all products that has been viewed
        /// </summary>
        /// <param name="ProductID">The product id of the recently viewed product</param>
        public void LogProductView(int productID)
        {
            if (m_IsRegistered ||
                HttpContext.Current.Items["OriginalSessionID"] != null)
            {
                string viewingCustomerID = "";
                if (m_IsRegistered)
                {
                    viewingCustomerID = CustomerID.ToString();
                }
                else
                {
                    viewingCustomerID = HttpContext.Current.Items["OriginalSessionID"].ToString();
                }

                if (productID != 0)
                {
                    using (SqlConnection cn = new SqlConnection(DB.GetDBConn()))
                    {
                        cn.Open();
                        using (SqlCommand cmd = new SqlCommand())
                        {
                            cmd.Connection = cn;
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandText = "dbo.aspdnsf_insProductView";


                            SqlParameter paramCustomerGuid = new SqlParameter("@CustomerViewID", SqlDbType.NVarChar);
                            SqlParameter paramProductId = new SqlParameter("@ProductID", SqlDbType.Int);
                            SqlParameter paramViewDate = new SqlParameter("@ViewDate", SqlDbType.DateTime);

                            paramCustomerGuid.Value = viewingCustomerID;
                            paramProductId.Value = productID;
                            paramViewDate.Value = DateTime.Now;

                            cmd.Parameters.Add(paramCustomerGuid);
                            cmd.Parameters.Add(paramProductId);
                            cmd.Parameters.Add(paramViewDate);

                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Replaces the product view of the unknown customer
        /// </summary>               
        public void ReplaceProductViewFromAnonymous()
        {
            if (HttpContext.Current.Items["OriginalSessionID"] != null)
            {
                DB.ExecuteSQL(string.Format("aspdnsf_updProductView {0}, {1}", DB.SQuote(HttpContext.Current.Items["OriginalSessionID"].ToString()), DB.SQuote(this.CustomerID.ToString())));
            }
        }

        /// <summary>
        /// Gets the salt key.
        /// </summary>
        /// <value>The salt key.</value>
        public int SaltKey
        {
            get { return m_SaltKey; }
        }

        /// <summary>
        /// Gets the name of the customer level.
        /// </summary>
        /// <value>The name of the customer level.</value>
        public String CustomerLevelName
        {
            get
            {
                if ((!HasCustomerRecord || m_CustomerLevelID == 0) && !AppLogic.IsAdminSite && AppLogic.AppConfigUSInt("DefaultCustomerLevelID") > 0)
                {
                    if (m_DefaultCustLevel_CustomerLevelName == null)
                        SetDefaultCustomerLevelData();
                    if (m_DefaultCustLevel_CustomerLevelName == null)
                        return m_CustomerLevelName;
                    return m_DefaultCustLevel_CustomerLevelName;
                }
                else
                {
                    return m_CustomerLevelName;
                }
            }
        }

        public Boolean UsingSoftDefaultCustomerLevel
        {
            get
            {
                return m_CustomerLevelID != CustomerLevelID;
            }
        }

        /// <summary>
        /// Gets or sets the E mail.
        /// </summary>
        /// <value>The E mail.</value>
        public String EMail
        {
            get
            {
                return m_EMail;
            }
            set
            {
                m_EMail = value;
            }
        }

        /// <summary>
        /// Gets the customer GUID.
        /// </summary>
        /// <value>The customer GUID.</value>
        public String CustomerGUID
        {
            get
            {
                return m_CustomerGUID;
            }
        }

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        /// <value>The first name.</value>
        public String FirstName
        {
            get
            {
                return m_FirstName;
            }
            set
            {
                m_FirstName = value;
            }
        }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        /// <value>The last name.</value>
        public String LastName
        {
            get
            {
                return m_LastName;
            }
            set
            {
                m_LastName = value;
            }
        }

        /// <summary>
        /// Gets the customer level ID.
        /// </summary>
        /// <value>The customer level ID.</value>
        public int CustomerLevelID
        {
            get
            {
                //JH - broader default customer level ID support for Multi-Store - default customer level will not set the customer level on that customer, just use it for the session.
                if ((!HasCustomerRecord || m_CustomerLevelID == 0) && !AppLogic.IsAdminSite && AppLogic.AppConfigUSInt("DefaultCustomerLevelID") > 0)
                {
                    return m_DefaultCustLevel_CustomerLevelID;
                }
                else
                {
                    return m_CustomerLevelID;
                }
            }
        }

        /// <summary>
        /// Gets the real locale.
        /// </summary>
        /// <value>The real locale.</value>
        public string RealLocale
        {
            get { return m_LocaleSetting; }
        }

        /// <summary>
        /// Gets or sets the locale setting.
        /// </summary>
        /// <value>The locale setting.</value>
        public String LocaleSetting
        {
            get
            {
                if (AppLogic.IsAdminSite)
                {
                    return Localization.GetDefaultLocale();
                }

                if (m_HasCustomerRecord && m_LocaleSetting.Trim() != "")
                {
                    return m_LocaleSetting;
                }
                else if (HttpContext.Current.Profile != null)
                {
                    if (HttpContext.Current.Profile.GetPropertyValue(ro_LocaleSettingCookieName).ToString() != "")
                    {
                        return HttpContext.Current.Profile.GetPropertyValue(ro_LocaleSettingCookieName).ToString();
                    }
                    else
                    {
                        return Localization.GetDefaultLocale();
                    }
                }
                else
                {
                    return Localization.GetDefaultLocale();
                }
            }
            set
            {
                if (value != "" && value != null)
                {
                    if (m_HasCustomerRecord)
                    {
                        SqlParameter sp1 = DB.CreateSQLParameter("@LocaleSetting", SqlDbType.NVarChar, 10, value, ParameterDirection.Input);
                        SqlParameter[] spa = { sp1 };
                        string retval = this.UpdateCustomer(spa);
                        if (retval == string.Empty)
                        {
                            m_LocaleSetting = value;
                        }
                    }
                    if (!AppLogic.IsAdminSite)
                    {
                        // the call could have been made on the BeginRequest and specified at the querystring
                        // at that point, the Profile is null.
                        // The only way to use a querystring to set this profile's property is 
                        // to go through the localesetting.aspx?locale={whateverlocale}
                        // if the call is coming from the BeginRequest, we shouldn't set this property yet
                        // and let the code in the localesetting.aspx set the current customer's property
                        // at that point on the page_load of the page, the Profile is already instantiated
                        if (HttpContext.Current.Profile != null)
                        {
                            HttpContext.Current.Profile.SetPropertyValue(Customer.ro_LocaleSettingCookieName, value);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the notes.
        /// </summary>
        /// <value>The notes.</value>
        public String Notes
        {
            get
            {
                return m_Notes;
            }
        }

        /// <summary>
        /// Gets or sets the currency setting.
        /// </summary>
        /// <value>The currency setting.</value>
        public String CurrencySetting
        {
            get
            {
                if (AppLogic.IsAdminSite)
                {
                    return Localization.GetPrimaryCurrency();
                }

                if (m_HasCustomerRecord && m_CurrencySetting.Trim() != "")
                {
                    return m_CurrencySetting;
                }
                else if (HttpContext.Current.Profile != null)
                {
                    if (HttpContext.Current.Profile.GetPropertyValue(ro_CurrencySettingCookieName).ToString() != "")
                    {
                        return HttpContext.Current.Profile.GetPropertyValue(ro_CurrencySettingCookieName).ToString();
                    }
                    else
                    {
                        return Localization.GetPrimaryCurrency(false);
                    }
                }
                else
                {
                    return Localization.GetPrimaryCurrency(false);
                }
            }
            set
            {
                if (value != "" && value != null)
                {
                    if (m_HasCustomerRecord)
                    {

                        SqlParameter sp1 = DB.CreateSQLParameter("@CurrencySetting", SqlDbType.NVarChar, 10, value, ParameterDirection.Input);
                        SqlParameter[] spa = { sp1 };
                        string retval = this.UpdateCustomer(spa);
                        if (retval == string.Empty)
                        {
                            m_CurrencySetting = value;
                        }
                    }
                    if (!AppLogic.IsAdminSite)
                    {
                        // the call could have been made on the BeginRequest and specified at the querystring
                        // at that point, the Profile is null.
                        // The only way to use a querystring to set this profile's property is 
                        // to go through the setcurrency.aspx?locale={whatevercurrencycode}
                        // if the call is coming from the BeginRequest, we shouldn't set this property yet
                        // and let the code in the setcurrency.aspx set the current customer's property
                        // at that point on the page_load of the page, the Profile is already instantiated
                        if (HttpContext.Current.Profile != null)
                        {
                            HttpContext.Current.Profile.SetPropertyValue(Customer.ro_CurrencySettingCookieName, value);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the micropay balance.
        /// </summary>
        /// <value>The micropay balance.</value>
        public decimal MicroPayBalance
        {
            get
            {
                return m_MicroPayBalance;
            }
        }

        /// <summary>
        /// Gets the subscription expires on.
        /// </summary>
        /// <value>The subscription expires on.</value>
        public DateTime SubscriptionExpiresOn
        {
            get
            {
                return m_SubscriptionExpiresOn;
            }
        }

        /// <summary>
        /// Gets a value indicating whether [discount extended prices].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [discount extended prices]; otherwise, <c>false</c>.
        /// </value>
        public bool DiscountExtendedPrices
        {
            get
            {
                if ((!HasCustomerRecord || m_CustomerLevelID == 0) && !AppLogic.IsAdminSite && AppLogic.AppConfigUSInt("DefaultCustomerLevelID") > 0)
                {
                    return m_DefaultCustLevel_DiscountExtendedPrices;
                }
                else
                {
                    return m_DiscountExtendedPrices;
                }
            }
        }

        /// <summary>
        /// Gets the level discount percentage.
        /// </summary>
        /// <value>The level discount percentage.</value>
        public decimal LevelDiscountPct
        {
            get
            {
                if ((!HasCustomerRecord || m_CustomerLevelID == 0) && !AppLogic.IsAdminSite && AppLogic.AppConfigUSInt("DefaultCustomerLevelID") > 0)
                {
                    return m_DefaultCustLevel_LevelDiscountPct;
                }
                else
                {
                    return m_LevelDiscountPct;
                }
            }
        }

        /// <summary>
        /// Gets the roles.
        /// </summary>
        /// <value>The roles.</value>
        public string Roles
        {
            get { return m_Roles; }
        }

        // low level flag set by customer preference.
        // this is NOT the final determination of whether CC info should be stored for this customer.
        /// <summary>
        /// Gets a value indicating whether [store CC in DB].
        /// </summary>
        /// <value><c>true</c> if [store CC in DB]; otherwise, <c>false</c>.</value>
        public bool StoreCCInDB
        {
            get { return m_StoreCCInDB; }
        }

        // this method is the MASTER routine which should be used to determine if CC info is being stored for a customer.
        // this method takes into account all store appconfig settings, and recurring billing considerations
        /// <summary>
        /// Gets a value indicating whether [master should we store credit card info].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [master should we store credit card info]; otherwise, <c>false</c>.
        /// </value>
        public bool MasterShouldWeStoreCreditCardInfo
        {
            get
            {
                bool SaveCC = false;
                bool HasRecurring = HasActiveRecurringOrders(true);
                bool UseGatewayInternalBilling = AppLogic.AppConfigBool("Recurring.UseGatewayInternalBilling");
                if (!HasRecurring)
                {
                    if (AppLogic.StoreCCInDB())
                    {
                        SaveCC = m_StoreCCInDB; //Use the customer's preference
                    }
                    else
                    {
                        SaveCC = false;  //Don't store the card number, period
                    }
                }
                else
                {
                    //Customer has recurring orders, so lets see if we need the card numbet or not
                    if (!UseGatewayInternalBilling) //We aren't using the gateway's recurring order feature so the card number MUST be stored
                    {
                        SaveCC = true;
                    }
                    else if (UseGatewayInternalBilling && AppLogic.StoreCCInDB()) //Card number is not required, but admin allows it to be stored
                    {
                        SaveCC = m_StoreCCInDB; //Use the customer's settings here
                    }
                    else //WCard number is not required and the admin does NOT allow storing cards
                    {
                        SaveCC = false; //Don't store the card number, period
                    }
                }
                return SaveCC;
            }
        }

        public bool SecureNetVaultMasterShouldWeStoreCreditCardInfo
        {
            get
            {
                return AppLogic.SecureNetVaultIsEnabled() && this.IsRegistered && m_StoreCCInDB;
            }
        }


        /// <summary>
        /// Gets the datetime of lockeduntil.
        /// </summary>
        /// <value>The locked until.</value>
        public DateTime LockedUntil
        {
            get { return m_LockedUntil; }
        }

        /// <summary>
        /// Gets a value indicating whether [admin can view CC].
        /// </summary>
        /// <value><c>true</c> if [admin can view CC]; otherwise, <c>false</c>.</value>
        public bool AdminCanViewCC
        {
            get { return m_AdminCanViewCC; }
        }

        /// <summary>
        /// Gets the current session ID.
        /// </summary>
        /// <value>The current session ID.</value>
        public int CurrentSessionID
        {
            get { return m_CurrentSessionID; }
        }

        /// <summary>
        /// Gets the last activity date.
        /// </summary>
        /// <value>The last activity date.</value>
        public DateTime LastActivity
        {
            get { return m_LastActivity; }
        }

        /// <summary>
        /// Gets the password changed date.
        /// </summary>
        /// <value>The password changed date.</value>
        public DateTime PwdChanged
        {
            get { return m_PwdChanged; }
        }

        /// <summary>
        /// Gets the bad login count.
        /// </summary>
        /// <value>The bad login count.</value>
        public byte BadLoginCount
        {
            get { return m_BadLoginCount; }
        }

        /// <summary>
        /// Gets the last bad login.
        /// </summary>
        /// <value>The last bad login.</value>
        public DateTime LastBadLogin
        {
            get { return m_LastBadLogin; }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Customer"/> is active.
        /// </summary>
        /// <value><c>true</c> if active; otherwise, <c>false</c>.</value>
        public bool Active
        {
            get { return m_Active; }
        }

        /// <summary>
        /// Gets a value indicating whether [PWD change required].
        /// </summary>
        /// <value><c>true</c> if [PWD change required]; otherwise, <c>false</c>.</value>
        public bool PwdChangeRequired
        {
            get { return m_PwdChgRequired; }
        }

        /// <summary>
        /// Gets the this customer session.
        /// </summary>
        /// <value>The this customer session.</value>
        public CustomerSession ThisCustomerSession
        {
            get
            {
                if (!HasCustomerRecord)
                {
                    m_CustomerSession = new CustomerSession();
                }
                else if (m_CurrentSessionID == -1)
                {
                    m_CustomerSession = CustomerSession.CreateCustomerSession(m_CustomerID, "", "", m_LastIPAddress);
                    m_CurrentSessionID = m_CustomerSession.SessionID;
                }
                else
                {
                    if (m_CustomerSession == null)
                    {
                        m_CustomerSession = new CustomerSession(m_CurrentSessionID, false);
                        m_CurrentSessionID = m_CustomerSession.SessionID;
                    }
                }
                return m_CustomerSession;
            }
        }

        /// <summary>
        /// Gets the primary billing address.
        /// </summary>
        /// <value>The primary billing address.</value>
        public Address PrimaryBillingAddress
        {
            get
            {
                if (m_PrimaryBillingAddress == null)
                {
                    m_PrimaryBillingAddress = new Address(m_CustomerID, AddressTypes.Billing);
                    m_PrimaryBillingAddress.LoadFromDB(m_PrimaryBillingAddressID);
                }
                return m_PrimaryBillingAddress;
            }
        }

        /// <summary>
        /// Gets the primary shipping address.
        /// </summary>
        /// <value>The primary shipping address.</value>
        public Address PrimaryShippingAddress
        {
            get
            {
                if (m_PrimaryShippingAddress == null)
                {
                    m_PrimaryShippingAddress = new Address(m_CustomerID, AddressTypes.Shipping);
                    m_PrimaryShippingAddress.LoadFromDB(m_PrimaryShippingAddressID);
                }
                return m_PrimaryShippingAddress;
            }
        }

        /// <summary>
        /// Gets the requested payment method.
        /// </summary>
        /// <value>The requested payment method.</value>
        public string RequestedPaymentMethod
        {
            get { return m_RequestedPaymentMethod; }
        }

        /// <summary>
        /// Returns the number of failed transactions this customer has attempted to process
        /// </summary>
        public int FailedTransactionCount
        {
            get { return m_FailedTransactionCount; }
        }

        #endregion


        #region IIdentity Members
        /// <summary>
        /// Gets a value that indicates whether the user has been authenticated.
        /// </summary>
        /// <value></value>
        /// <returns>true if the user was authenticated; otherwise, false.
        /// </returns>
        [XmlIgnore]
        public bool IsAuthenticated
        {
            get
            {
                return ((this.CustomerGUID != null) && (!this.CustomerGUID.Equals("")));
            }
        }

        /// <summary>
        /// Gets the name of the current user.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The name of the user on whose behalf the code is running.
        /// </returns>
        [XmlIgnore]
        public string Name
        {
            get
            {
                return this.CustomerGUID;
            }
        }

        /// <summary>
        /// Gets the type of authentication used.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The type of authentication used to identify the user.
        /// </returns>
        [XmlIgnore]
        public string AuthenticationType
        {
            get
            {
                return "Forms";
            }
        }
        #endregion

        /// <summary>
        /// Gets the current customer.
        /// </summary>
        /// <value>The current customer.</value>
        public static Customer Current
        {
            get
            {
                HttpContext ctx = HttpContext.Current;
                if (null != ctx)
                {
                    return (ctx.User as AspDotNetStorefrontPrincipal).ThisCustomer;
                }

                return null;
            }
        }

        public bool LevelHasNoTax
        {
            get { return AppLogic.CustomerLevelHasNoTax(CustomerLevelID); }
        }

        public bool SuppressCookies
        {
            get { return m_SuppressCookies; }
        }

        internal Boolean IsVatExempt()
        {
            if (!AppLogic.AppConfigBool("VAT.Enabled"))
                return false;

            return !String.IsNullOrEmpty(this.VATRegistrationID);
        }
    }

    public class GridCustomer
    {
        #region Private Variables
        private int m_customerid;
        private String m_name;
        private int m_admin;
        private int m_customerlevelid;
        private DateTime m_subscriptionexpires;
        private DateTime m_createdon;
        private String m_email;
        private bool m_oktoemail;
        private Address m_billingaddress;
        private bool m_isregistered;
        private DateTime m_lockeduntil;
        private bool m_active;
        private bool m_deleted;
        #endregion

        #region Public Properties

        public int CustomerID
        {
            get { return m_customerid; }
            set { m_customerid = value; }
        }

        public String Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        public int Admin
        {
            get { return m_admin; }
            set { m_admin = value; }
        }

        public int CustomerLevelID
        {
            get { return m_customerlevelid; }
            set { m_customerlevelid = value; }
        }

        public DateTime SubscriptionExpires
        {
            get { return m_subscriptionexpires; }
            set { m_subscriptionexpires = value; }
        }

        public DateTime CreatedOn
        {
            get { return m_createdon; }
            set { m_createdon = value; }
        }

        public String Email
        {
            get { return m_email; }
            set { m_email = value; }
        }

        public bool OkToEmail
        {
            get { return m_oktoemail; }
            set { m_oktoemail = value; }
        }

        public Address BillingAddress
        {
            get
            {
                if (m_billingaddress == null)
                {
                    m_billingaddress = new Address(m_customerid, AddressTypes.Billing);
                }

                return m_billingaddress;
            }

        }

        public bool IsRegistered
        {
            get { return m_isregistered; }
            set { m_isregistered = false; }
        }

        public DateTime LockedUntil
        {
            get { return m_lockeduntil; }
            set { m_lockeduntil = value; }
        }

        public bool Active
        {
            get { return m_active; }
            set { m_active = value; }
        }

        public bool Deleted
        {
            get { return m_deleted; }
            set { m_deleted = value; }
        }

        #endregion

        #region Constructors

        public GridCustomer()
        {
            m_customerid = 0;
            m_name = String.Empty;
            m_admin = 0;
            m_customerlevelid = 0;
            m_subscriptionexpires = DateTime.MinValue;
            m_createdon = DateTime.MinValue;
            m_email = String.Empty;
            m_oktoemail = false;
            m_billingaddress = null;
            m_isregistered = false;
            m_lockeduntil = DateTime.MaxValue;
            m_active = false;
            m_deleted = false;
        }

        public GridCustomer(int CustomerID)
        {
            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                using (IDataReader rs = DB.GetRS("select CustomerID, FirstName, LastName, IsAdmin, CustomerLevelID, SubscriptionExpiresOn, Email, OkToEmail, CreatedOn, IsRegistered, LockedUntil, Active, Deleted from dbo.Customer with(NOLOCK) where CustomerID=" + CustomerID.ToString(), conn))
                {
                    if (rs.Read())
                    {
                        m_customerid = DB.RSFieldInt(rs, "CustomerID");
                        m_name = DB.RSField(rs, "FirstName") + " " + DB.RSField(rs, "LastName");
                        m_admin = DB.RSFieldTinyInt(rs, "IsAdmin");
                        m_customerlevelid = DB.RSFieldInt(rs, "CustomerLevelID");
                        m_subscriptionexpires = DB.RSFieldDateTime(rs, "SubscriptionExpiresOn");
                        m_createdon = DB.RSFieldDateTime(rs, "CreatedOn");
                        m_email = DB.RSField(rs, "Email");
                        m_oktoemail = DB.RSFieldTinyInt(rs, "OkToEmail").Equals(1);
                        m_billingaddress = null;
                        m_isregistered = DB.RSFieldTinyInt(rs, "IsRegistered").Equals(1);
                        m_lockeduntil = DB.RSFieldDateTime(rs, "LockedUntil");
                        m_active = DB.RSFieldTinyInt(rs, "Active").Equals(1);
                        m_deleted = DB.RSFieldTinyInt(rs, "Deleted").Equals(1);
                    }

                    rs.Close();
                    rs.Dispose();
                }
                conn.Close();
                conn.Dispose();
            }
        }

        #endregion

        #region Static Methods
        /// <summary>
        /// This fuction is EXTREMELY inefficent and depricated. Unless you need ALL customer records, use the CustomerSearch class.
        /// </summary>
        /// <returns>List of Customers</returns>
        public static List<GridCustomer> GetCustomers()
        {
            List<GridCustomer> grdCustomers = new List<GridCustomer>();

            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                using (IDataReader rs = DB.GetRS("select CustomerID from dbo.Customer with(NOLOCK)", conn))
                {
                    while (rs.Read())
                    {
                        GridCustomer gc = new GridCustomer(DB.RSFieldInt(rs, "CustomerID"));
                        grdCustomers.Add(gc);
                    }
                    rs.Close();
                    rs.Dispose();
                }
                conn.Close();
                conn.Dispose();
            }

            return grdCustomers;
        }

        #endregion
    }

    public class CustomerSearch
    {
        #region Public Instance Variables
        public CustomerSortField SortField { set; get; }

        public Boolean IncludeDeleted { get; set; }

        public Boolean IncludeUnregistered { get; set; }

        public Boolean LockedOnly { get; set; }

        public String SearchTerm { get; set; }

        public CustomerAlphaFilter AlphaFilter { get; set; }
        #endregion
        #region Constructors
        public CustomerSearch()
        {
            this.SortField = CustomerSortField.CustomerID;
            this.IncludeDeleted = false;
            this.IncludeUnregistered = false;
            this.LockedOnly = false;
            this.AlphaFilter = CustomerAlphaFilter.NONE;
        }
        #endregion
        #region Public Methods
        public PaginatedList<GridCustomer> Search(int pageSize, int currentPage)
        {
            PaginatedList<GridCustomer> customers = new PaginatedList<GridCustomer>();
            string sql = this.BuildCustomerSql(pageSize, currentPage);
            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                using (IDataReader rs = DB.GetRS(sql, conn))
                {
                    while (rs.Read())
                    {
                        //todo: build customers from current set rather than individually
                        GridCustomer gc = new GridCustomer(DB.RSFieldInt(rs, "CustomerID"));
                        customers.Add(gc);
                    }
                }
            }
            customers.TotalCount = RowCount();
            customers.PageSize = pageSize;
            customers.CurrentPage = currentPage;
            decimal pages = (decimal)customers.TotalCount / (decimal)pageSize;
            customers.TotalPages = (int)Math.Ceiling(pages);
            customers.StartIndex = (currentPage - 1) * pageSize + 1;
            customers.EndIndex = currentPage * pageSize;
            return customers;
        }
        #endregion
        #region Private Methods
        private int RowCount()
        {
            string sql = "select count(*) as count from customer where " + WhereCluse();
            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                using (IDataReader rs = DB.GetRS(sql, conn))
                {
                    if (rs.Read())
                    {
                        return DB.RSFieldInt(rs, "count");
                    }
                }
            }
            return 0;
        }

        private string BuildCustomerSql(int pageSize, int currentPage)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine("DECLARE @PageNum AS INT;");
            sql.AppendLine("DECLARE @PageSize AS INT;");
            sql.AppendLine("SET @PageNum = " + currentPage + ";");
            sql.AppendLine("SET @PageSize =  " + pageSize + ";");
            sql.AppendLine("WITH CustomerPaged AS");
            sql.AppendLine("(");
            sql.AppendLine("    SELECT ROW_NUMBER() OVER(" + OrderByClause() + ") AS RowNum");
            sql.AppendLine("          ,CustomerID, FirstName, LastName, IsAdmin, CustomerLevelId,SubscriptionExpiresOn, Createdon, email, oktoemail, billingaddressid, lockeduntil, active, deleted, IsRegistered");
            sql.AppendLine("      FROM dbo.Customer with (NOLOCK)");
            sql.AppendLine("      where " + WhereCluse());
            sql.AppendLine(")");

            sql.AppendLine("SELECT * ");
            sql.AppendLine("  FROM CustomerPaged with (NOLOCK)");
            sql.AppendLine(" WHERE RowNum BETWEEN (@PageNum - 1) * @PageSize + 1 AND @PageNum * @PageSize");
            sql.AppendLine(" " + OrderByClause() + ";");

            return sql.ToString();
        }

        private string OrderByClause()
        {
            switch (this.SortField)
            {
                case CustomerSortField.Name:
                    return " ORDER BY lastname, firstname, customerid ";
                case CustomerSortField.CustomerLevelID:
                    return " ORDER BY customerlevelid desc, customerid ";
                case CustomerSortField.Admin:
                    return " ORDER BY isadmin desc, customerid ";
                case CustomerSortField.Email:
                    return " ORDER BY email, customerid ";
                case CustomerSortField.CustomerID:
                default:
                    return " ORDER BY customerid ";
            }
        }

        private string WhereCluse()
        {
            StringBuilder where = new StringBuilder();
            where.Append(" (1=1) ");
            if (!this.IncludeDeleted)
            {
                where.Append(" and deleted = 0 ");
            }
            if (!this.IncludeUnregistered)
            {
                where.Append(" and IsRegistered = 1 ");
            }
            if (this.LockedOnly)
            {
                where.Append(" and LockedUntil > getdate() ");
            }
            if (!string.IsNullOrEmpty(this.SearchTerm))
            {
                where.Append(" and " + SearchClause(this.SearchTerm) + " ");
            }
            if (!(AlphaFilter == CustomerAlphaFilter.NONE))
            {
                where.Append(" and " + AlphaFilterClause(this.AlphaFilter) + " ");
            }
            return where.ToString();
        }

        private string AlphaFilterClause(CustomerAlphaFilter filter)
        {
            switch (filter)
            {
                case CustomerAlphaFilter.NONE:
                    return "";
                case CustomerAlphaFilter.NUMBERS:
                    return " '0123456789' like '%' + SUBSTRING(lastname, 1, 1) + '%' ";
                default:
                    return " " + DB.SQuote(filter.ToString()) + " like '%' + SUBSTRING(lastname, 1, 1) + '%' ";
            }
        }

        private string SearchClause(String search)
        {
            string liketerm = "'%' + " + DB.SQuote(search) + " + '%'";
            Int32 searchId;
            string idsearchstring = CommonLogic.IIF(Int32.TryParse(search, out searchId), " or customerid = " + searchId, "");
            return String.Format(" (firstname like {0} or lastname like {0} or email like {0}" + idsearchstring + ") ", liketerm);
        }
        #endregion
        #region Public Enums for Search Preferences
        public enum CustomerSortField
        {
            Name,
            CustomerLevelID,
            Admin,
            Email,
            CustomerID
        }
        public enum CustomerAlphaFilter
        {
            NONE, NUMBERS, A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z
        }
        #endregion
    }

    #region Public Enums for Customer Level
    public enum UserType
    {
        STOREADMINISTRATOR = 1,
        BUDGETADMINISTRATOR = 2,
        SALESREPS = 3,
        BLUELITE = 4,
        BLUPREMIER = 5,
        BLUAUTHORIZED = 6,
        INTERNAL = 7,
        PUBLIC = 8,
        POTENTIAL = 9,
        HOMEDEPOT = 10,
        LOWES = 11,
        MENARDS = 12,
        BLUUNLIMITED = 13
    }
    #endregion
}
