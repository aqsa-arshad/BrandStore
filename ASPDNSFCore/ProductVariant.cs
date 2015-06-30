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
using System.Text;

namespace AspDotNetStorefrontCore
{
    public class ProductVariant
    {
        #region Private Variables

        private int m_variantid;
        private String m_variantguid;
        private bool m_isdefault;
        private String m_name;
        private String m_description;
        private String m_sekeywords;
        private String m_sedescription;
        private String m_sealttext;
        private String m_colors;
        private String m_colorskumodifiers;
        private String m_sizes;
        private String m_sizeskumodifiers;
        private String m_froogledescription;
        private int m_productid;
        private String m_skusuffix;
        private String m_manufacturerpartnumber;
        private Decimal m_price;
        private Decimal m_saleprice;
        private Decimal m_weight;
        private Decimal m_msrp;
        private Decimal m_cost;
        private int m_points;
        private String m_dimensions;
        private int m_inventory;
        private int m_displayorder;
        private String m_notes;
        private bool m_istaxable;
        private bool m_isshipseparately;
        private bool m_isdownload;
        private String m_downloadlocation;
		private int m_downloadValidDays;
        private int m_freeshipping;
        private bool m_published;
        private bool m_wholesale;
        private bool m_issecureattachment;
        private bool m_isrecurring;
        private int m_recurringinterval;
        private int m_recurringintervaltype;
        private int m_subscriptioninterval;
        private int m_rewardpoints;
        private String m_sename;
        private String m_restrictedquantities;
        private int m_minimumquantity;
        private String m_extensiondata;
        private String m_extensiondata2;
        private String m_extensiondata3;
        private String m_extensiondata4;
        private String m_extensiondata5;
        private String m_contentsbgcolor;
        private String m_pagebgcolor;
        private String m_graphicscolor;
        private String m_imagefilenameoverride;
        private bool m_isimport;
        private bool m_deleted;
        private DateTime m_createdon;
        private int m_subscriptionintervaltype;
        private bool m_customerentersprice;
        private String m_customerenterspriceprompt;
        private int m_condition;


        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor for ProductVariant
        /// </summary>
        public ProductVariant()
        {
            Reset();
        }

        /// <summary>
        /// Constructor for ProductVariant that will populate the properties from the database
        /// </summary>
        /// <param name="ProductVariantID">The ID of the ProductVariant to retrieve data for</param>
        public ProductVariant(int ProductVariantID)
        {
            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                using (IDataReader rs = DB.GetRS("select * from dbo.ProductVariant with(NOLOCK) where VariantID = " + ProductVariantID.ToString(), conn))
                {
                    if (rs.Read())
                    {
                        Load(rs);
                    }

                    rs.Close();
                    rs.Dispose();
                }

                conn.Close();
                conn.Dispose();
            }
        }

        /// <summary>
        /// Constructor for ProductVariant that will populate the properties from the database
        /// </summary>
        /// <param name="rs"></param>
        public ProductVariant(IDataReader rs)
        {
            Load(rs);
        }

        #endregion

        #region Public Properties

        public int VariantID { get { return m_variantid; } set { m_variantid = value; } }
        public String VariantGUID { get { return m_variantguid; } set { m_variantguid = value; } }
        public bool IsDefault { get { return m_isdefault; } set { m_isdefault = value; } }
        public String Name { get { return m_name; } set { m_name = value; } }
        public String LocaleName { get { return LocalizedValue(m_name); } }
        public String Description { get { return m_description; } set { m_description = value; } }
        public String LocaleDescription { get { return LocalizedValue(m_description); } }
        public String SEKeywords { get { return m_sekeywords; } set { m_sekeywords = value; } }
        public String SEDescription { get { return m_sedescription; } set { m_sedescription = value; } }
        public String SEAltText { get { return m_sealttext; } set { m_sealttext = value; } }
        public String Colors { get { return m_colors; } set { m_colors = value; } }
        public String ColorSKUModifiers { get { return m_colorskumodifiers; } set { m_colorskumodifiers = value; } }
        public String Sizes { get { return m_sizes; } set { m_sizes = value; } }
        public String SizeSKUModifiers { get { return m_sizeskumodifiers; } set { m_sizeskumodifiers = value; } }
        public String FroogleDescription { get { return m_froogledescription; } set { m_froogledescription = value; } }
        public int ProductID { get { return m_productid; } set { m_productid = value; } }
        public String SKUSuffix { get { return m_skusuffix; } set { m_skusuffix = value; } }
        public String ManufacturerPartNumber { get { return m_manufacturerpartnumber; } set { m_manufacturerpartnumber = value; } }
        public Decimal Price { get { return m_price; } set { m_price = value; } }
        public Decimal SalePrice { get { return m_saleprice; } set { m_saleprice = value; } }
        public Decimal Weight { get { return m_weight; } set { m_weight = value; } }
        public Decimal MSRP { get { return m_msrp; } set { m_msrp = value; } }
        public Decimal Cost { get { return m_cost; } set { m_cost = value; } }
        public int Points { get { return m_points; } set { m_points = value; } }
        public String Dimensions { get { return m_dimensions; } set { m_dimensions = value; } }
        public int Inventory { get { return m_inventory; } set { m_inventory = value; } }
        public int DisplayOrder { get { return m_displayorder; } set { m_displayorder = value; } }
        public String Notes { get { return m_notes; } set { m_notes = value; } }
        public bool IsTaxable { get { return m_istaxable; } set { m_istaxable = value; } }
        public bool IsShipSeparately { get { return m_isshipseparately; } set { m_isshipseparately = value; } }
        public bool IsDownload { get { return m_isdownload; } set { m_isdownload = value; } }
        public String DownloadLocation { get { return m_downloadlocation; } set { m_downloadlocation = value; } }
		public int DownloadValidDays { get { return m_downloadValidDays; } set { m_downloadValidDays = value; } }
        public int FreeShipping { get { return m_freeshipping; } set { m_freeshipping = value; } }
        public bool Published { get { return m_published; } set { m_published = value; } }
        public bool Wholesale { get { return m_wholesale; } set { m_wholesale = value; } }
        public bool IsSecureAttachment { get { return m_issecureattachment; } set { m_issecureattachment = value; } }
        public bool IsRecurring { get { return m_isrecurring; } set { m_isrecurring = value; } }
        public int RecurringInterval { get { return m_recurringinterval; } set { m_recurringinterval = value; } }
        public int RecurringIntervalType { get { return m_recurringintervaltype; } set { m_recurringintervaltype = value; } }
        public int SubscriptionInterval { get { return m_subscriptioninterval; } set { m_subscriptioninterval = value; } }
        public int RewardPoints { get { return m_rewardpoints; } set { m_rewardpoints = value; } }
        public String SEName { get { return m_sename; } set { m_sename = value; } }
        public String RestrictedQuantities { get { return m_restrictedquantities; } set { m_restrictedquantities = value; } }
        public int MinimumQuantity { get { return m_minimumquantity; } set { m_minimumquantity = value; } }
        public String ExtensionData { get { return m_extensiondata; } set { m_extensiondata = value; } }
        public String ExtensionData2 { get { return m_extensiondata2; } set { m_extensiondata2 = value; } }
        public String ExtensionData3 { get { return m_extensiondata3; } set { m_extensiondata3 = value; } }
        public String ExtensionData4 { get { return m_extensiondata4; } set { m_extensiondata4 = value; } }
        public String ExtensionData5 { get { return m_extensiondata5; } set { m_extensiondata5 = value; } }
        public String ContentsBGColor { get { return m_contentsbgcolor; } set { m_contentsbgcolor = value; } }
        public String PageBGColor { get { return m_pagebgcolor; } set { m_pagebgcolor = value; } }
        public String GraphicsColor { get { return m_graphicscolor; } set { m_graphicscolor = value; } }
        public String ImageFilenameOverride { get { return m_imagefilenameoverride; } set { m_imagefilenameoverride = value; } }
        public bool IsImport { get { return m_isimport; } set { m_isimport = value; } }
        public bool Deleted { get { return m_deleted; } set { m_deleted = value; } }
        public DateTime CreatedOn { get { return m_createdon; } set { m_createdon = value; } }
        public int SubscriptionIntervalType { get { return m_subscriptionintervaltype; } set { m_subscriptionintervaltype = value; } }
        public bool CustomerEntersPrice { get { return m_customerentersprice; } set { m_customerentersprice = value; } }
        public String CustomerEntersPricePrompt { get { return m_customerenterspriceprompt; } set { m_customerenterspriceprompt = value; } }
        public int Condition { get { return m_condition; } set { m_condition = value; } }
        public bool IsFirstVariantAdded { get { return (DB.GetSqlN("select count(VariantID) as N from ProductVariant  with (NOLOCK)  where ProductID=" + m_productid.ToString() + " and Deleted=0") == 0); } }

        #endregion

        #region Public Methods

        /// <summary>
        /// Determines a localized value for the current locale for any product property when multiple locales are being used
        /// </summary>
        /// <param name="val">The unlocalized string value to localize</param>
        /// <returns>A localized string based on the current locale</returns>
        public String LocalizedValue(String val)
        {
            Customer ThisCustomer = AppLogic.GetCurrentCustomer();

            String LocalValue = val;

            if (ThisCustomer != null)
            {
                LocalValue = XmlCommon.GetLocaleEntry(val, ThisCustomer.LocaleSetting, true);
            }
            else
            {
                LocalValue = XmlCommon.GetLocaleEntry(val, Localization.GetDefaultLocale(), true);
            }

            return LocalValue;
        }

        /// <summary>
        /// Updates the variant as the default variant of the product
        /// </summary>
        /// <returns></returns>
        public bool MakeDefault()
        {
            bool success = true;

            try
            {
                m_isdefault = true;

                // reset all other variants of the product
                DB.ExecuteSQL("update dbo.ProductVariant set IsDefault=0 where ProductID=" + m_productid.ToString());

                // update the current variant to be the only default
                DB.ExecuteSQL("update dbo.ProductVariant set IsDefault=1 where VariantID=" + m_variantid.ToString());
            }
            catch (Exception ex)
            {
                SysLog.LogException(ex, MessageTypeEnum.DatabaseException, MessageSeverityEnum.Error);
                success = false;
            }

            return success;
        }

        /// <summary>
        /// Updates the variant as published
        /// </summary>
        /// <returns>Boolean value indicating success</returns>
        public bool Publish()
        {
            bool success = true;

            try
            {
                m_published = true;
                DB.ExecuteSQL("update dbo.ProductVariant set Published=1 where VariantID=" + m_variantid.ToString());
            }
            catch (Exception ex)
            {
                SysLog.LogException(ex, MessageTypeEnum.DatabaseException, MessageSeverityEnum.Error);
                success = false;
            }

            return success;
        }

        /// <summary>
        /// Updates the variant as unpublished
        /// </summary>
        /// <returns>Boolean value indicating success</returns>
        public bool UnPublish()
        {
            bool success = true;

            try
            {
                m_published = false;
                DB.ExecuteSQL("update dbo.ProductVariant set Published=0 where VariantID=" + m_variantid.ToString());
            }
            catch (Exception ex)
            {
                SysLog.LogException(ex, MessageTypeEnum.DatabaseException, MessageSeverityEnum.Error);
                success = false;
            }

            return success;
        }

        /// <summary>
        /// Creates the variant in the database
        /// </summary>
        /// <returns>Boolean value indicating success</returns>
        public bool Create()
        {
            bool success = true;

            try
            {
                StringBuilder sql = new StringBuilder();

                sql.Append("insert into dbo.ProductVariant(VariantGUID,ProductID,Name,ContentsBGColor,PageBGColor,GraphicsColor," +
                    "ImageFilenameOverride,SEAltText,IsDefault,Description,RestrictedQuantities,FroogleDescription,Price,SalePrice," +
                    "MSRP,Cost,Points,MinimumQuantity,SKUSuffix,ManufacturerPartNumber,Weight,Dimensions,Inventory,SubscriptionInterval," +
                    "SubscriptionIntervalType,Published,CustomerEntersPrice,CustomerEntersPricePrompt,IsRecurring,RecurringInterval," +
                    "RecurringIntervalType,Colors,ColorSKUModifiers,Sizes,SizeSKUModifiers,IsTaxable,IsShipSeparately,IsDownload," +
                    "FreeShipping,DownloadLocation,Condition) values(");

                sql.Append(DB.SQuote(m_variantguid) + ",");
                sql.Append(m_productid.ToString() + ",");
                sql.Append(DB.SQuote(m_name) + ",");
                sql.Append(DB.SQuote(m_contentsbgcolor) + ",");
                sql.Append(DB.SQuote(m_pagebgcolor) + ",");
                sql.Append(DB.SQuote(m_graphicscolor) + ",");
                sql.Append(DB.SQuote(m_imagefilenameoverride) + ",");
                sql.Append(DB.SQuote(m_sealttext) + ",");
                sql.Append(CommonLogic.IIF(IsFirstVariantAdded, "1", "0") + ",");
                sql.Append(DB.SQuote(m_description) + ",");
                sql.Append(DB.SQuote(m_restrictedquantities) + ",");
                sql.Append(DB.SQuote(m_froogledescription) + ",");
                sql.Append(DB.SQuote(m_price.ToString()) + ",");
                sql.Append(DB.SQuote(m_saleprice.ToString()) + ",");
                sql.Append(DB.SQuote(m_msrp.ToString()) + ",");
                sql.Append(DB.SQuote(m_cost.ToString()) + ",");
                sql.Append(m_points.ToString() + ",");
                sql.Append(m_minimumquantity.ToString() + ",");
                sql.Append(DB.SQuote(m_skusuffix) + ",");
                sql.Append(DB.SQuote(m_manufacturerpartnumber) + ",");
                sql.Append(DB.SQuote(m_weight.ToString()) + ",");
                sql.Append(DB.SQuote(m_dimensions) + ",");
                sql.Append(m_inventory.ToString() + ",");
                sql.Append(m_subscriptioninterval.ToString() + ",");
                sql.Append(m_subscriptionintervaltype.ToString() + ",");
                sql.Append(CommonLogic.IIF(m_published, "1", "0") + ",");
                sql.Append(CommonLogic.IIF(m_customerentersprice, "1", "0") + ",");
                sql.Append(DB.SQuote(m_customerenterspriceprompt) + ",");
                sql.Append(CommonLogic.IIF(m_isrecurring, "1", "0") + ",");
                sql.Append(m_recurringinterval.ToString() + ",");
                sql.Append(m_recurringintervaltype.ToString() + ",");
                sql.Append(DB.SQuote(m_colors) + ",");
                sql.Append(DB.SQuote(m_colorskumodifiers) + ",");
                sql.Append(DB.SQuote(m_sizes) + ",");
                sql.Append(DB.SQuote(m_sizeskumodifiers) + ",");
                sql.Append(CommonLogic.IIF(m_istaxable, "1", "0") + ",");
                sql.Append(CommonLogic.IIF(m_isshipseparately, "1", "0") + ",");
                sql.Append(CommonLogic.IIF(m_isdownload, "1", "0") + ",");
                sql.Append(m_freeshipping.ToString() + ",");
                sql.Append(DB.SQuote(m_downloadlocation) + ",");
				sql.Append(m_downloadValidDays + ",");
                sql.Append(m_condition.ToString());

                sql.Append(")");

                DB.ExecuteSQL(sql.ToString());
            }
            catch (Exception ex)
            {
                SysLog.LogException(ex, MessageTypeEnum.DatabaseException, MessageSeverityEnum.Error);
                success = false;
            }

            return success;
        }

        /// <summary>
        /// Updates the varaint in the database with the properties from this variant object
        /// </summary>
        /// <returns>Boolean value indicating success</returns>
        public bool Update()
        {
            bool success = true;

            try
            {
                StringBuilder sql = new StringBuilder();

                sql.Append("update dbo.ProductVariant set ");

                sql.Append("VariantGUID=" + DB.SQuote(m_variantguid) + ",");
                sql.Append("Name=" + DB.SQuote(m_name) + ",");
                sql.Append("ContentsBGColor=" + DB.SQuote(m_contentsbgcolor) + ",");
                sql.Append("PageBGColor=" + DB.SQuote(m_pagebgcolor) + ",");
                sql.Append("GraphicsColor=" + DB.SQuote(m_graphicscolor) + ",");
                sql.Append("ImageFilenameOverride=" + DB.SQuote(m_imagefilenameoverride) + ",");
                sql.Append("SEAltText=" + DB.SQuote(m_sealttext) + ",");
                sql.Append("Description=" + DB.SQuote(m_description) + ",");
                sql.Append("RestrictedQuantities=" + DB.SQuote(m_restrictedquantities) + ",");
                sql.Append("FroogleDescription=" + DB.SQuote(m_froogledescription) + ",");
                sql.Append("Price=" + DB.SQuote(m_price.ToString()) + ",");
                sql.Append("SalePrice=" + DB.SQuote(m_saleprice.ToString()) + ",");
                sql.Append("MSRP=" + DB.SQuote(m_msrp.ToString()) + ",");
                sql.Append("Cost=" + DB.SQuote(m_cost.ToString()) + ",");
                sql.Append("Points=" + m_points.ToString() + ",");
                sql.Append("MinimumQuantity=" + m_minimumquantity.ToString() + ",");
                sql.Append("SkuSuffix=" + DB.SQuote(m_skusuffix) + ",");
                sql.Append("ManufacturerPartNumber=" + DB.SQuote(m_manufacturerpartnumber) + ",");
                sql.Append("Weight=" + DB.SQuote(m_weight.ToString()) + ",");
                sql.Append("Dimensions=" + DB.SQuote(m_dimensions) + ",");
                sql.Append("Inventory=" + m_inventory.ToString() + ",");
                sql.Append("SubscriptionInterval=" + m_subscriptioninterval.ToString() + ",");
                sql.Append("SubscriptionIntervalType=" + m_subscriptionintervaltype.ToString() + ",");
                sql.Append("Published=" + CommonLogic.IIF(m_published, "1", "0") + ",");
                sql.Append("CustomerEntersPrice=" + CommonLogic.IIF(m_customerentersprice, "1", "0") + ",");
                sql.Append("CustomerEntersPricePrompt=" + DB.SQuote(m_customerenterspriceprompt) + ",");
                sql.Append("IsRecurring=" + CommonLogic.IIF(m_isrecurring, "1", "0") + ",");
                sql.Append("RecurringInterval=" + m_recurringinterval.ToString() + ",");
                sql.Append("RecurringIntervalType=" + m_recurringintervaltype.ToString() + ",");
                sql.Append("Colors=" + DB.SQuote(m_colors) + ",");
                sql.Append("ColorSkuModifiers=" + DB.SQuote(m_colorskumodifiers) + ",");
                sql.Append("Sizes=" + DB.SQuote(m_sizes) + ",");
                sql.Append("SizeSkuModifiers=" + DB.SQuote(m_sizeskumodifiers) + ",");
                sql.Append("IsTaxable=" + CommonLogic.IIF(m_istaxable, "1", "0") + ",");
                sql.Append("IsShipSeparately=" + CommonLogic.IIF(m_isshipseparately, "1", "0") + ",");
                sql.Append("IsDownload=" + CommonLogic.IIF(m_isdownload, "1", "0") + ",");
                sql.Append("FreeShipping=" + m_freeshipping.ToString() + ",");
                sql.Append("DownloadLocation=" + DB.SQuote(m_downloadlocation) + ",");
				sql.Append("DownloadValidDays=" + m_downloadValidDays + ",");
                sql.Append("Condition=" + m_condition.ToString());

                sql.Append(" where VariantID=" + m_variantid.ToString());

                DB.ExecuteSQL(sql.ToString());
            }
            catch (Exception ex)
            {
                SysLog.LogException(ex, MessageTypeEnum.DatabaseException, MessageSeverityEnum.Error);
                success = false;
            }

            return success;
        }

        /// <summary>
        /// Soft Deletes the variant (sets Deleted=1) in the database
        /// </summary>
        /// <returns>Boolean value indicating success</returns>
        public bool Delete()
        {
            bool success = true;

            try
            {
                m_deleted = true;
                DB.ExecuteSQL("update dbo.ProductVariant set Deleted=1 where VariantID=" + m_variantid.ToString());
            }
            catch (Exception ex)
            {
                SysLog.LogException(ex, MessageTypeEnum.DatabaseException, MessageSeverityEnum.Error);
                success = false;
            }

            return success;
        }

        /// <summary>
        /// UnDeletes a soft deleted (set Deleted=0) variant in the database
        /// </summary>
        /// <returns>Boolean value indicating success</returns>
        public bool UnDelete()
        {
            bool success = true;

            try
            {
                m_deleted = false;
                DB.ExecuteSQL("update dbo.ProductVariant set Deleted=0 where VariantID=" + m_variantid.ToString());
            }
            catch (Exception ex)
            {
                SysLog.LogException(ex, MessageTypeEnum.DatabaseException, MessageSeverityEnum.Error);
                success = false;
            }

            return success;
        }

        /// <summary>
        /// Nukes the variant (removes the database record) from the database
        /// </summary>
        /// <returns>Boolean value indicating success</returns>
        public bool Nuke()
        {
            bool success = true;

            try
            {
                DB.ExecuteSQL("delete dbo.ProductVariant where VariantID=" + m_variantid.ToString());
                Reset();
            }
            catch (Exception ex)
            {
                SysLog.LogException(ex, MessageTypeEnum.DatabaseException, MessageSeverityEnum.Error);
                success = false;
            }

            return success;
        }

        /// <summary>
        /// Loads data from the database and populates the properties of the variant
        /// </summary>
        /// <param name="rs">IDataReader containing variant data</param>
        public void Load(IDataReader rs)
        {
            m_variantid = DB.RSFieldInt(rs, "VariantID");
            m_variantguid = DB.RSFieldGUID(rs, "VariantGUID");
            m_isdefault = DB.RSFieldBool(rs, "IsDefault");
            m_name = DB.RSField(rs, "Name");
            m_description = DB.RSField(rs, "Description");
            m_sekeywords = DB.RSField(rs, "SEKeywords");
            m_sedescription = DB.RSField(rs, "SEDescription");
            m_sealttext = DB.RSField(rs, "SEAltText");
            m_colors = DB.RSField(rs, "Colors");
            m_colorskumodifiers = DB.RSField(rs, "ColorSKUModifiers");
            m_sizes = DB.RSField(rs, "Sizes");
            m_sizeskumodifiers = DB.RSField(rs, "SizeSKUModifiers");
            m_froogledescription = DB.RSField(rs, "FroogleDescription");
            m_productid = DB.RSFieldInt(rs, "ProductID");
            m_skusuffix = DB.RSField(rs, "SKUSuffix");
            m_manufacturerpartnumber = DB.RSField(rs, "ManufacturerPartNumber");
            m_price = DB.RSFieldDecimal(rs, "Price");
            m_saleprice = DB.RSFieldDecimal(rs, "SalePrice");
            m_weight = DB.RSFieldDecimal(rs, "Weight");
            m_msrp = DB.RSFieldDecimal(rs, "MSRP");
            m_cost = DB.RSFieldDecimal(rs, "Cost");
            m_points = DB.RSFieldInt(rs, "Points");
            m_dimensions = DB.RSField(rs, "Dimensions");
            m_inventory = DB.RSFieldInt(rs, "Inventory");
            m_displayorder = DB.RSFieldInt(rs, "DisplayOrder");
            m_notes = DB.RSField(rs, "Notes");
            m_istaxable = DB.RSFieldBool(rs, "IsTaxable");
            m_isshipseparately = DB.RSFieldBool(rs, "IsShipSeparately");
            m_isdownload = DB.RSFieldBool(rs, "IsDownload");
            m_downloadlocation = DB.RSField(rs, "DownloadLocation");
			m_downloadValidDays = DB.RSFieldInt(rs, "DownloadValidDays");
            m_freeshipping = DB.RSFieldTinyInt(rs, "FreeShipping");
            m_published = DB.RSFieldBool(rs, "Published");
            m_wholesale = DB.RSFieldBool(rs, "Wholesale");
            m_issecureattachment = DB.RSFieldBool(rs, "IsSecureAttachment");
            m_isrecurring = DB.RSFieldBool(rs, "IsRecurring");
            m_recurringinterval = DB.RSFieldInt(rs, "RecurringInterval");
            m_recurringintervaltype = DB.RSFieldInt(rs, "RecurringIntervalType");
            m_subscriptioninterval = DB.RSFieldInt(rs, "SubscriptionInterval");
            m_rewardpoints = DB.RSFieldInt(rs, "RewardPoints");
            m_sename = DB.RSField(rs, "SEName");
            m_restrictedquantities = DB.RSField(rs, "RestrictedQuantities");
            m_minimumquantity = DB.RSFieldInt(rs, "MinimumQuantity");
            m_extensiondata = DB.RSField(rs, "ExtensionData");
            m_extensiondata2 = DB.RSField(rs, "ExtensionData2");
            m_extensiondata3 = DB.RSField(rs, "ExtensionData3");
            m_extensiondata4 = DB.RSField(rs, "ExtensionData4");
            m_extensiondata5 = DB.RSField(rs, "ExtensionData5");
            m_contentsbgcolor = DB.RSField(rs, "ContentsBGColor");
            m_pagebgcolor = DB.RSField(rs, "PageBGColor");
            m_graphicscolor = DB.RSField(rs, "GraphicsColor");
            m_imagefilenameoverride = DB.RSField(rs, "ImageFilenameOverride");
            m_isimport = DB.RSFieldBool(rs, "IsImport");
            m_deleted = DB.RSFieldBool(rs, "Deleted");
            m_createdon = DB.RSFieldDateTime(rs, "CreatedOn");
            m_subscriptionintervaltype = DB.RSFieldInt(rs, "SubscriptionIntervalType");
            m_customerentersprice = DB.RSFieldBool(rs, "CustomerEntersPrice");
            m_customerenterspriceprompt = DB.RSField(rs, "CustomerEntersPricePrompt");
            m_condition = DB.RSFieldTinyInt(rs, "Condition");

        }

        /// <summary>
        /// Resets all properties to default values
        /// </summary>
        public void Reset()
        {
            m_variantid = 0;
            m_variantguid = String.Empty;
            m_isdefault = false;
            m_name = String.Empty;
            m_description = String.Empty;
            m_sekeywords = String.Empty;
            m_sedescription = String.Empty;
            m_sealttext = String.Empty;
            m_colors = String.Empty;
            m_colorskumodifiers = String.Empty;
            m_sizes = String.Empty;
            m_sizeskumodifiers = String.Empty;
            m_froogledescription = String.Empty;
            m_productid = 0;
            m_skusuffix = String.Empty;
            m_manufacturerpartnumber = String.Empty;
            m_price = 0.00M;
            m_saleprice = 0.00M;
            m_weight = 0.00M;
            m_msrp = 0.00M;
            m_cost = 0.00M;
            m_points = 0;
            m_dimensions = String.Empty;
            m_inventory = 0;
            m_displayorder = 0;
            m_notes = String.Empty;
            m_istaxable = false;
            m_isshipseparately = false;
            m_isdownload = false;
            m_downloadlocation = String.Empty;
            m_freeshipping = 0;
            m_published = false;
            m_wholesale = false;
            m_issecureattachment = false;
            m_isrecurring = false;
            m_recurringinterval = 0;
            m_recurringintervaltype = 0;
            m_subscriptioninterval = 0;
            m_rewardpoints = 0;
            m_sename = String.Empty;
            m_restrictedquantities = String.Empty;
            m_minimumquantity = 0;
            m_extensiondata = String.Empty;
            m_extensiondata2 = String.Empty;
            m_extensiondata3 = String.Empty;
            m_extensiondata4 = String.Empty;
            m_extensiondata5 = String.Empty;
            m_contentsbgcolor = String.Empty;
            m_pagebgcolor = String.Empty;
            m_graphicscolor = String.Empty;
            m_imagefilenameoverride = String.Empty;
            m_isimport = false;
            m_deleted = false;
            m_createdon = DateTime.MinValue;
            m_subscriptionintervaltype = 0;
            m_customerentersprice = false;
            m_customerenterspriceprompt = String.Empty;
            m_condition = 0;
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Retrieves a list of all variants for a specific product with the option to include soft deleted variants
        /// </summary>
        /// <param name="ProductID">The ID of the product to retrieve variants for</param>
        /// <param name="includeDeleted">Boolean value indicating whether soft deleted variants should be included</param>
        /// <returns>A ProductVariant List of variants for a product</returns>
        public static List<ProductVariant> GetVariants(int ProductID, bool includeDeleted)
        {
            List<ProductVariant> variants = new List<ProductVariant>();

            String sql = "select * from dbo.ProductVariant with(NOLOCK) where ProductID=" + ProductID.ToString() + CommonLogic.IIF(includeDeleted, String.Empty, " and Deleted=0");

            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                using (IDataReader rs = DB.GetRS(sql, conn))
                {

                    while (rs.Read())
                    {
                        ProductVariant pv = new ProductVariant(rs);

                        variants.Add(pv);
                    }

                    rs.Close();
                    rs.Dispose();
                }

                conn.Close();
                conn.Dispose();
            }

            return variants;
        }

        #endregion
    }

    public class GridProductVariant
    {
        #region Private Variables

        private int m_variantid;
        private bool m_isdefault;
        private String m_name;
        private String m_description;
        private String m_productName;
        private int m_productid;
        private String m_skusuffix;
        private Decimal m_price;
        private Decimal m_saleprice;
        private int m_inventory;
        private bool m_published;
        private bool m_deleted;
        private bool m_trackInventoryBySizeAndColor;

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor for ProductVariant
        /// </summary>
        public GridProductVariant()
        {
            Reset();
        }

        /// <summary>
        /// Constructor for ProductVariant that will populate the properties from the database
        /// </summary>
        /// <param name="ProductVariantID">The ID of the ProductVariant to retrieve data for</param>
        public GridProductVariant(int VariantID)
        {
            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                using (IDataReader rs = DB.GetRS("select VariantID, IsDefault, Name, Description, ProductID, SkuSuffix, Price, SalePrice, Inventory, Published, Deleted from dbo.ProductVariant with(NOLOCK) where VariantID = " + VariantID.ToString(), conn))
                {
                    if (rs.Read())
                    {
                        LoadFromRS(rs);
                    }

                    rs.Close();
                    rs.Dispose();
                }

                conn.Close();
                conn.Dispose();
            }
        }

        /// <summary>
        /// Constructor for ProductVariant that will populate the properties from the database
        /// </summary>
        /// <param name="rs"></param>
        public GridProductVariant(IDataReader rs)
        {
            LoadFromRS(rs);
        }

        #endregion

        #region Public Properties

        public int VariantID { get { return m_variantid; } set { m_variantid = value; } }
        public bool IsDefault { get { return m_isdefault; } set { m_isdefault = value; } }
        public String Name { get { return m_name; } set { m_name = value; } }
        public String LocaleName { get { return LocalizedValue(m_name); } }
        public String Description { get { return m_description; } set { m_description = value; } }
        public int ProductID { get { return m_productid; } set { m_productid = value; } }
        public String SKUSuffix { get { return m_skusuffix; } set { m_skusuffix = value; } }
        public Decimal Price { get { return m_price; } set { m_price = value; } }
        public Decimal SalePrice { get { return m_saleprice; } set { m_saleprice = value; } }
        public int Inventory { get { return m_inventory; } set { m_inventory = value; } }
        public bool Published { get { return m_published; } set { m_published = value; } }
        public bool Deleted { get { return m_deleted; } set { m_deleted = value; } }
        public bool TrackInventoryBySizeAndColor { get { return m_trackInventoryBySizeAndColor; } set { m_trackInventoryBySizeAndColor = value; } }
        public String ProductName { get { return m_productName; } set { m_productName = value; } }

        public bool IsFirstVariantAdded { get { return (DB.GetSqlN("select count(VariantID) as N from ProductVariant  with (NOLOCK)  where ProductID=" + m_productid.ToString() + " and Deleted=0") == 0); } }

        #endregion

        #region Public Methods

        /// <summary>
        /// Writes the currently set properties to the database
        /// </summary>
        public void Commit()
        {
            SqlParameter[] spa = {
                                     new SqlParameter("@variantID", m_variantid),
                                     new SqlParameter("@name", m_name),
                                     new SqlParameter("@description", m_description),
                                     new SqlParameter("@price", Localization.CurrencyStringForDBWithoutExchangeRate(m_price)),
                                     new SqlParameter("@salePrice", Localization.CurrencyStringForDBWithoutExchangeRate(m_saleprice)),
                                     new SqlParameter("@skuSuffix", m_skusuffix),
                                     new SqlParameter("@Inventory", m_inventory),
                                     new SqlParameter("@Published", (m_published == false ? 0 : 1))
                                 };
            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                DB.ExecuteStoredProcVoid("[dbo].[aspdnsf_updGridProductVariant]", spa, conn);

                conn.Close();
                conn.Dispose();
            }



        }

		/// <summary>
        /// Determines a localized value for the current locale for any product property when multiple locales are being used
        /// </summary>
        /// <param name="val">The unlocalized string value to localize</param>
        /// <returns>A localized string based on the current locale</returns>
        public String LocalizedValue(String val)
        {
            Customer ThisCustomer = AppLogic.GetCurrentCustomer();

            String LocalValue = val;

            if (ThisCustomer != null)
            {
                LocalValue = XmlCommon.GetLocaleEntry(val, ThisCustomer.LocaleSetting, true);
            }
            else
            {
                LocalValue = XmlCommon.GetLocaleEntry(val, Localization.GetDefaultLocale(), true);
            }

            return LocalValue;
        }

        /// <summary>
        /// Updates the variant as the default variant of the product
        /// </summary>
        /// <returns></returns>
        public bool MakeDefault()
        {
            bool success = true;

            try
            {
                m_isdefault = true;

                // reset all other variants of the product
                DB.ExecuteSQL("update dbo.ProductVariant set IsDefault=0 where ProductID=" + m_productid.ToString());

                // update the current variant to be the only default
                DB.ExecuteSQL("update dbo.ProductVariant set IsDefault=1 where VariantID=" + m_variantid.ToString());
            }
            catch (Exception ex)
            {
                SysLog.LogException(ex, MessageTypeEnum.DatabaseException, MessageSeverityEnum.Error);
                success = false;
            }

            return success;
        }

        /// <summary>
        /// Updates the variant as published
        /// </summary>
        /// <returns>Boolean value indicating success</returns>
        public bool Publish()
        {
            bool success = true;

            try
            {
                m_published = true;
                DB.ExecuteSQL("update dbo.ProductVariant set Published=1 where VariantID=" + m_variantid.ToString());
            }
            catch (Exception ex)
            {
                SysLog.LogException(ex, MessageTypeEnum.DatabaseException, MessageSeverityEnum.Error);
                success = false;
            }

            return success;
        }

        /// <summary>
        /// Updates the variant as unpublished
        /// </summary>
        /// <returns>Boolean value indicating success</returns>
        public bool UnPublish()
        {
            bool success = true;

            try
            {
                m_published = false;
                DB.ExecuteSQL("update dbo.ProductVariant set Published=0 where VariantID=" + m_variantid.ToString());
            }
            catch (Exception ex)
            {
                SysLog.LogException(ex, MessageTypeEnum.DatabaseException, MessageSeverityEnum.Error);
                success = false;
            }

            return success;
        }

        /// <summary>
        /// Soft Deletes the variant (sets Deleted=1) in the database
        /// </summary>
        /// <returns>Boolean value indicating success</returns>
        public bool Delete()
        {
            bool success = true;

            try
            {
                m_deleted = true;
                DB.ExecuteSQL("update dbo.ProductVariant set Deleted=1 where VariantID=" + m_variantid.ToString());
            }
            catch (Exception ex)
            {
                SysLog.LogException(ex, MessageTypeEnum.DatabaseException, MessageSeverityEnum.Error);
                success = false;
            }

            return success;
        }

        /// <summary>
        /// UnDeletes a soft deleted (set Deleted=0) variant in the database
        /// </summary>
        /// <returns>Boolean value indicating success</returns>
        public bool UnDelete()
        {
            bool success = true;

            try
            {
                m_deleted = false;
                DB.ExecuteSQL("update dbo.ProductVariant set Deleted=0 where VariantID=" + m_variantid.ToString());
            }
            catch (Exception ex)
            {
                SysLog.LogException(ex, MessageTypeEnum.DatabaseException, MessageSeverityEnum.Error);
                success = false;
            }

            return success;
        }

        /// <summary>
        /// Nukes the variant (removes the database record) from the database
        /// </summary>
        /// <returns>Boolean value indicating success</returns>
        public bool Nuke()
        {
            bool success = true;

            try
            {
                DB.ExecuteSQL("delete dbo.ProductVariant where VariantID=" + m_variantid.ToString());
                Reset();
            }
            catch (Exception ex)
            {
                SysLog.LogException(ex, MessageTypeEnum.DatabaseException, MessageSeverityEnum.Error);
                success = false;
            }

            return success;
        }

        /// <summary>
        /// Loads data from the database and populates the properties of the variant
        /// </summary>
        /// <param name="rs">IDataReader containing variant data</param>
        public void LoadFromRS(IDataReader rs)
        {
            m_variantid = DB.RSFieldInt(rs, "VariantID");
            m_isdefault = DB.RSFieldBool(rs, "IsDefault");
            m_name = DB.RSField(rs, "Name");
            m_description = DB.RSField(rs, "Description");
            m_productid = DB.RSFieldInt(rs, "ProductID");
            m_skusuffix = DB.RSField(rs, "SKUSuffix");
            m_price = DB.RSFieldDecimal(rs, "Price");
            m_saleprice = DB.RSFieldDecimal(rs, "SalePrice");
            m_inventory = DB.RSFieldInt(rs, "Inventory");
            m_published = DB.RSFieldBool(rs, "Published");
            m_deleted = DB.RSFieldBool(rs, "Deleted");
            m_trackInventoryBySizeAndColor = DB.RSFieldBool(rs, "TrackInventoryBySizeAndColor");
            m_productName = DB.RSField(rs, "ProductName");
        }

        /// <summary>
        /// Resets all properties to default values
        /// </summary>
        public void Reset()
        {
            m_variantid = 0;
            m_isdefault = false;
            m_name = String.Empty;
            m_productid = 0;
            m_skusuffix = String.Empty;
            m_price = 0.00M;
            m_saleprice = 0.00M;
            m_inventory = 0;
            m_published = false;
            m_deleted = false;
            m_trackInventoryBySizeAndColor = false;
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Retrieves a list of all variants for a specific product with the option to include soft deleted variants
        /// </summary>
        /// <param name="ProductID">The ID of the product to retrieve variants for</param>
        /// <param name="includeDeleted">Boolean value indicating whether soft deleted variants should be included</param>
        /// <returns>A ProductVariant List of variants for a product</returns>
        public static List<GridProductVariant> GetVariants(int ProductID, bool includeDeleted)
        {
            List<GridProductVariant> variants = new List<GridProductVariant>();

            String sql = "select pv.VariantID, pv.IsDefault, pv.Name, pv.Description, pv.ProductID, pv.SkuSuffix, pv.Price, pv.SalePrice, pv.Inventory, pv.Published, pv.Deleted, p.TrackInventoryBySizeAndColor, p.Name as ProductName " +
                "from dbo.ProductVariant pv with(NOLOCK) join Product p on p.ProductID = pv.ProductID where " +
                "pv.ProductID=" + ProductID.ToString() + CommonLogic.IIF(includeDeleted, String.Empty, " and pv.Deleted=0");

            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                using (IDataReader rs = DB.GetRS(sql, conn))
                {
                    while (rs.Read())
                    {
                        GridProductVariant pv = new GridProductVariant(rs);

                        variants.Add(pv);
                    }

                    rs.Close();
                    rs.Dispose();
                }

                conn.Close();
                conn.Dispose();
            }

            return variants;
        }


        /// <summary>
        /// Retrieves a list of all product variants for all products
        /// </summary>
        /// <param name="includeDeleted">Determines if soft deleted variants are included</param>
        /// <param name="entityType">EntityType to filter by.  Use EntityType.Unknown to disable filtering</param>
        /// <param name="entityID">Entity ID to filter on.  This is ignored if EntityType is EntityType.Unknown</param>
        /// <returns></returns>
        public static List<GridProductVariant> GetAllVariants(bool includeDeleted, AppLogic.EntityType entityType, int entityID)
        {
            List<GridProductVariant> variants = new List<GridProductVariant>();

            SqlParameter filterEntityType = new SqlParameter("@FilterEntityType", SqlDbType.Int);
            SqlParameter filterEntityID = new SqlParameter("@FilterEntityID", SqlDbType.Int);
            SqlParameter isDeleted = new SqlParameter("@Deleted", SqlDbType.Int);

            if (entityType == AppLogic.EntityType.Unknown)
            {
                filterEntityType.Value = DBNull.Value;
                filterEntityID.Value = DBNull.Value;
            }
            else
            {
                filterEntityType.Value = (int)entityType;

                if (entityID == 0)
                {
                    filterEntityID.Value = DBNull.Value;
                }
                else
                {
                    filterEntityID.Value = entityID;
                }
            }

            isDeleted.Value = (includeDeleted ? 1 : 0);

            SqlParameter[] spa = { filterEntityID, filterEntityType, isDeleted };

            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                using (IDataReader rs = DB.ExecuteStoredProcReader("[dbo].[aspdnsf_getProductVariants]", spa, conn))
                {
                    while (rs.Read())
                    {
                        GridProductVariant pv = new GridProductVariant(rs);

                        variants.Add(pv);
                    }

                    rs.Close();
                    rs.Dispose();
                }

                conn.Close();
                conn.Dispose();
            }

            return variants;

        }


        #region ObjectDataSource Methods

        /// <summary>
        /// Retrieves a list of variants for the current page for controls using an object datasource
        /// </summary>
        /// <returns></returns>
        public static List<GridProductVariant> GetVariantsPaged(int maximumRows, int startRowIndex, int EntityType, int EntityID)
        {
            List<GridProductVariant> variants = new List<GridProductVariant>();

            SqlParameter spMaxRows = new SqlParameter("@pageSize", maximumRows);
            SqlParameter spStartIndex = new SqlParameter("@startIndex", startRowIndex);
            SqlParameter spEntityType = new SqlParameter("@entityFilterType", EntityType);
            SqlParameter spEntityID = new SqlParameter("@entityFilterID", EntityID);

            SqlParameter[] spa = { spMaxRows, spStartIndex, spEntityType, spEntityID };

            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                using (IDataReader rs = DB.ExecuteStoredProcReader("[dbo].[aspdnsf_getVariantsPaged]", spa, conn))
                {
                    while (rs.Read())
                    {
                        GridProductVariant pv = new GridProductVariant(rs);

                        variants.Add(pv);
                    }

                    rs.Close();
                    rs.Dispose();
                }

                conn.Close();
                conn.Dispose();
            }

            return variants;
        }

        /// <summary>
        /// Returns a count of all variants that match the provided criteria
        /// An entityType or entityID of zero will return a count of all variants
        /// Entity Type:
        /// 1 = Category
        /// 2 = Department/Section
        /// 3 = Manufacturer
        /// 4 = Distributor
        /// </summary>
        /// <returns></returns>
        public static int GetVariantCount(int entityType, int entityID)
        {
            string Sql = "SELECT COUNT(*) AS N FROM ProductVariant WHERE Deleted = 0";

            if (entityID > 0 && entityType > 0)
            {
                string entityName = Enum.GetName(typeof(AppLogic.EntityType), entityType);

                Sql += " AND ProductID IN (SELECT ProductID FROM Product" + entityName + " WHERE " + entityName + "ID = " + entityID.ToString() + ")";
                //Example:  AND ProductID IN (SELECT ProductID FROM ProductCategory WHERE CategoryID = 1)
            }

            int i = 0;
			//checking for active connection is handled in DB.cs
			i = DB.GetSqlN(Sql);

            return i;
        }


        #endregion

        #endregion
    }

	public class InventoryItem
	{
		public int VariantId { get; private set; }
		public string Size { get; private set; }
		public string Color { get; private set; }
		public string WarehouseLocation { get; private set; }
		public string FullSku { get; private set; }
		public string VendorId { get; private set; }
		public decimal WeightDelta { get; private set; }
		public string GTIN { get; private set; }
		public int Inventory { get; private set; }

		public InventoryItem(string size, string color, int productId, int variantId)
		{
			string warehouseLocation = string.Empty;
			string fullSku = string.Empty;
			string vendorId = string.Empty;
			decimal weightDelta = decimal.Zero;
			string gtin = string.Empty;

			int inventory = AppLogic.GetInventory(productId, variantId, size, color, true, true, true, out warehouseLocation, out fullSku, out vendorId, out weightDelta, out gtin);

			VariantId = variantId;
			Size = size;
			Color = color;
			WarehouseLocation = warehouseLocation;
			FullSku = fullSku;
			VendorId = vendorId;
			WeightDelta = weightDelta;
			GTIN = gtin;
			Inventory = inventory;
		}
	}

}
