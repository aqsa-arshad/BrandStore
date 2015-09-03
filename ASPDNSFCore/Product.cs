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
    public class Product
    {
        #region Private Variables

        private int m_productid;
        private String m_productguid;
        private String m_name;
        private String m_summary;
        private String m_description;
        private String m_sekeywords;
        private String m_sedescription;
        private String m_spectitle;
        private String m_misctext;
        private String m_swatchimagemap;
        private String m_isfeaturedteaser;
        private String m_froogledescription;
        private String m_setitle;
        private String m_senoscript;
        private String m_sealttext;
        private String m_sizeoptionprompt;
        private String m_coloroptionprompt;
        private String m_textoptionprompt;
        private int m_producttypeid;
        private int m_taxclassid;
        private String m_sku;
        private String m_manufacturerpartnumber;
        private int m_salespromptid;
        private string m_salesprompt;
        private String m_speccall;
        private bool m_specsinline;
        private bool m_isfeatured;
        private String m_xmlpackage;
        private int m_colwidth;
        private bool m_published;
        private bool m_wholesale;
        private bool m_requiresregistration;
        private int m_looks;
        private String m_notes;
        private int m_quantitydiscountid;
        private String m_relatedproducts;
        private String m_upsellproducts;
        private Decimal m_upsellproductdiscountpercentage;
        private String m_relateddocuments;
        private bool m_trackinventorybysizeandcolor;
        private bool m_trackinventorybysize;
        private bool m_trackinventorybycolor;
        private bool m_isakit;
        private bool m_showinproductbrowser;
        private bool m_isapack;
        private int m_packsize;
        private bool m_showbuybutton;
        private String m_requiresproducts;
        private bool m_hidepriceuntilcart;
        private bool m_iscalltoorder;
        private bool m_excludefrompricefeeds;
        private bool m_requirestextoption;
        private int m_textoptionmaxlength;
        private String m_sename;
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
        private bool m_issystem;
        private bool m_deleted;
        private DateTime m_createdon;
        private int m_pagesize;
        private String m_warehouselocation;
        private DateTime m_availablestartdate;
        private DateTime m_availablestopdate;
        private int m_skinid;
        private String m_templatename;

        private List<ProductVariant> m_productvariants;

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor for Product
        /// </summary>
        public Product()
        {
            Reset();
        }

        /// <summary>
        /// Constructor for Product that will populate the properties from the database
        /// </summary>
        /// <param name="ProductID">The ID of the product to retrieve data for</param>
        public Product(int ProductID)
        {
            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                using (IDataReader rs = DB.GetRS("select * from dbo.Product with(NOLOCK) where ProductID=" + ProductID.ToString(), conn))
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
        /// Constructor for Product that will populate the properties from the database
        /// </summary>
        /// <param name="rs">The IDataReader containing Product data</param>
        public Product(IDataReader rs)
        {
            LoadFromRS(rs);
        }

        #endregion

        #region Public Properties

        public int ProductID { get { return m_productid; } set { m_productid = value; } }
        public String ProductGUID { get { return m_productguid; } set { m_productguid = value; } }
        public String Name { get { return m_name; } set { m_name = value; } }
        public String LocaleName { get { return LocalizedValue(m_name); } }
        public String Summary { get { return m_summary; } set { m_summary = value; } }
        public String Description { get { return m_description; } set { m_description = value; } }
        public String LocaleDescription { get { return LocalizedValue(m_description); } }
        public String SEKeywords { get { return m_sekeywords; } set { m_sekeywords = value; } }
        public String SEDescription { get { return m_sedescription; } set { m_sedescription = value; } }
        public String SpecTitle { get { return m_spectitle; } set { m_spectitle = value; } }
        public String MiscText { get { return m_misctext; } set { m_misctext = value; } }
        public String SwatchImageMap { get { return m_swatchimagemap; } set { m_swatchimagemap = value; } }
        public String IsFeaturedTeaser { get { return m_isfeaturedteaser; } set { m_isfeaturedteaser = value; } }
        public String FroogleDescription { get { return m_froogledescription; } set { m_froogledescription = value; } }
        public String SETitle { get { return m_setitle; } set { m_setitle = value; } }
        public String SENoScript { get { return m_senoscript; } set { m_senoscript = value; } }
        public String SEAltText { get { return m_sealttext; } set { m_sealttext = value; } }
        public String SizeOptionPrompt { get { return m_sizeoptionprompt; } set { m_sizeoptionprompt = value; } }
        public String ColorOptionPrompt { get { return m_coloroptionprompt; } set { m_coloroptionprompt = value; } }
        public String TextOptionPrompt { get { return m_textoptionprompt; } set { m_textoptionprompt = value; } }
        public int ProductTypeID { get { return m_producttypeid; } set { m_producttypeid = value; } }
        public int TaxClassID { get { return m_taxclassid; } set { m_taxclassid = value; } }
        public String SKU { get { return m_sku; } set { m_sku = value; } }
        public String ManufacturerPartNumber { get { return m_manufacturerpartnumber; } set { m_manufacturerpartnumber = value; } }
        public int SalesPromptID { get { return m_salespromptid; } set { m_salespromptid = value; } }
        public String SalesPrompt
        {
            get
            {
                if (String.IsNullOrEmpty(m_salesprompt))
                {
                    m_salesprompt = DB.GetSqlS("select Name as S from dbo.SalesPrompt with(NOLOCK) where SalesPromptID=" + m_salespromptid.ToString());
                }
                return m_salesprompt;
            }
            set
            {
                m_salesprompt = value;
            }
        }
        public String SpecCall { get { return m_speccall; } set { m_speccall = value; } }
        public bool SpecsInline { get { return m_specsinline; } set { m_specsinline = value; } }
        public bool IsFeatured { get { return m_isfeatured; } set { m_isfeatured = value; } }
        public String XmlPackage { get { return m_xmlpackage; } set { m_xmlpackage = value; } }
        public int ColWidth { get { return m_colwidth; } set { m_colwidth = value; } }
        public bool Published { get { return m_published; } set { m_published = value; } }
        public bool Wholesale { get { return m_wholesale; } set { m_wholesale = value; } }
        public bool RequiresRegistration { get { return m_requiresregistration; } set { m_requiresregistration = value; } }
        public int Looks { get { return m_looks; } set { m_looks = value; } }
        public String Notes { get { return m_notes; } set { m_notes = value; } }
        public int QuantityDiscountID { get { return m_quantitydiscountid; } set { m_quantitydiscountid = value; } }
        public String RelatedProducts { get { return m_relatedproducts; } set { m_relatedproducts = value; } }
        public String UpsellProducts { get { return m_upsellproducts; } set { m_upsellproducts = value; } }
        public Decimal UpsellProductDiscountPercentage { get { return m_upsellproductdiscountpercentage; } set { m_upsellproductdiscountpercentage = value; } }
        public String RelatedDocuments { get { return m_relateddocuments; } set { m_relateddocuments = value; } }
        public bool TrackInventoryBySizeAndColor { get { return m_trackinventorybysizeandcolor; } set { m_trackinventorybysizeandcolor = value; } }
        public bool TrackInventoryBySize { get { return m_trackinventorybysize; } set { m_trackinventorybysize = value; } }
        public bool TrackInventoryByColor { get { return m_trackinventorybycolor; } set { m_trackinventorybycolor = value; } }
        public bool IsAKit { get { return m_isakit; } set { m_isakit = value; } }
        public bool ShowInProductBrowser { get { return m_showinproductbrowser; } set { m_showinproductbrowser = value; } }
        public bool IsAPack { get { return m_isapack; } set { m_isapack = value; } }
        public int PackSize { get { return m_packsize; } set { m_packsize = value; } }
        public bool ShowBuyButton { get { return m_showbuybutton; } set { m_showbuybutton = value; } }
        public String RequiresProducts { get { return m_requiresproducts; } set { m_requiresproducts = value; } }
        public bool HidePriceUntilCart { get { return m_hidepriceuntilcart; } set { m_hidepriceuntilcart = value; } }
        public bool IsCalltoOrder { get { return m_iscalltoorder; } set { m_iscalltoorder = value; } }
        public bool ExcludeFromPriceFeeds { get { return m_excludefrompricefeeds; } set { m_excludefrompricefeeds = value; } }
        public bool RequiresTextOption { get { return m_requirestextoption; } set { m_requirestextoption = value; } }
        public int TextOptionMaxLength { get { return m_textoptionmaxlength; } set { m_textoptionmaxlength = value; } }
        public String SEName { get { return m_sename; } set { m_sename = value; } }
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
        public bool IsSystem { get { return m_issystem; } set { m_issystem = value; } }
        public bool Deleted { get { return m_deleted; } set { m_deleted = value; } }
        public DateTime CreatedOn { get { return m_createdon; } set { m_createdon = value; } }
        public int PageSize { get { return m_pagesize; } set { m_pagesize = value; } }
        public String WarehouseLocation { get { return m_warehouselocation; } set { m_warehouselocation = value; } }
        public DateTime AvailableStartDate { get { return m_availablestartdate; } set { m_availablestartdate = value; } }
        public DateTime AvailableStopDate { get { return m_availablestopdate; } set { m_availablestopdate = value; } }
        public int SkinID { get { return m_skinid; } set { m_skinid = value; } }
        public String TemplateName { get { return m_templatename; } set { m_templatename = value; } }
        public List<ProductVariant> Variants { get { return m_productvariants; } set { m_productvariants = value; } }

        #endregion

        #region Public Methods

        /// <summary>
        /// Retrieves variants for the current product
        /// </summary>
        /// <param name="includeDeleted">Boolean value indicating whether to included soft-deleted (eg. Deleted=1) variants</param>
        /// <returns>A ProductVariant List of variants belonging to this product</returns>
        public void LoadVariants(bool includeDeleted)
        {
            m_productvariants = ProductVariant.GetVariants(m_productid, includeDeleted);
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
        /// Updates the product as published
        /// </summary>
        /// <returns>Boolean value indicating success</returns>
        public bool Publish()
        {
            bool success = true;

            try
            {
                m_published = true;
                DB.ExecuteSQL("update dbo.Product set Published=1 where ProductID=" + m_productid.ToString());
            }
            catch (Exception ex)
            {
                SysLog.LogException(ex, MessageTypeEnum.DatabaseException, MessageSeverityEnum.Error);
                success = false;
            }

            return success;
        }

        /// <summary>
        /// Updates the product as unpublished
        /// </summary>
        /// <returns>Boolean value indicating success</returns>
        public bool UnPublish()
        {
            bool success = true;

            try
            {
                m_published = false;
                DB.ExecuteSQL("update dbo.Product set Published=0 where ProductID=" + m_productid.ToString());
            }
            catch (Exception ex)
            {
                SysLog.LogException(ex, MessageTypeEnum.DatabaseException, MessageSeverityEnum.Error);
                success = false;
            }

            return success;
        }

        /// <summary>
        /// Creates the product in the database
        /// </summary>
        /// <returns>Boolean value indicating success</returns>
        public bool Create()
        {
            bool success = true;

            try
            {
                StringBuilder sql = new StringBuilder();

            /*7*/   sql.Append("insert into product(ProductGUID,Name,SEName,ContentsBGColor,PageBGColor,GraphicsColor,ImageFilenameOverride," +
            /*8*/   "ProductTypeID,Summary,Description,ExtensionData,ExtensionData2,ExtensionData3,ExtensionData4,ExtensionData5," +
            /*6*/   "ColorOptionPrompt,SizeOptionPrompt,RequiresTextOption,TextOptionPrompt,TextOptionMaxLength,FroogleDescription," +
            /*4*/   "RelatedProducts,UpsellProducts,UpsellProductDiscountPercentage,RequiresProducts," +
           /*10*/   "SEKeywords,SEDescription,SETitle,SENoScript,SEAltText,SKU,PageSize,ColWidth,XmlPackage,ManufacturerPartNumber," +
            /*8*/   "SalesPromptID,SpecTitle,SpecCall,Published,ShowBuyButton,IsCallToOrder,HidePriceUntilCart," +
            /*6*/   "ShowInProductBrowser,ExcludeFromPriceFeeds,IsAKit,IsAPack,PackSize,TrackInventoryBySizeAndColor," +
            /*7*/   "TrackInventoryBySize,TrackInventoryByColor,RequiresRegistration,SpecsInline,MiscText,SwatchImageMap,QuantityDiscountID," +
            /*3*/   "TaxClassID,AvailableStartDate,AvailableStopDate) values(");
           /*59*/

                sql.Append(DB.SQuote(m_productguid) + ",");
                sql.Append(DB.SQuote(m_name) + ",");
                sql.Append(DB.SQuote(m_sename) + ",");
                sql.Append(DB.SQuote(m_contentsbgcolor) + ",");
                sql.Append(DB.SQuote(m_pagebgcolor) + ",");
                sql.Append(DB.SQuote(m_graphicscolor) + ",");
                sql.Append(DB.SQuote(m_imagefilenameoverride) + ",");
                sql.Append(m_producttypeid.ToString() + ",");
                sql.Append(DB.SQuote(m_summary) + ",");
                sql.Append(DB.SQuote(m_description) + ",");
                sql.Append(DB.SQuote(m_extensiondata) + ",");
                sql.Append(DB.SQuote(m_extensiondata2) + ",");
                sql.Append(DB.SQuote(m_extensiondata3) + ",");
                sql.Append(DB.SQuote(m_extensiondata4) + ",");
                sql.Append(DB.SQuote(m_extensiondata5) + ",");
                sql.Append(DB.SQuote(m_coloroptionprompt) + ",");
                sql.Append(DB.SQuote(m_sizeoptionprompt) + ",");
                sql.Append(CommonLogic.IIF(m_requirestextoption, "1", "0") + ",");
                sql.Append(DB.SQuote(m_textoptionprompt) + ",");
                sql.Append(m_textoptionmaxlength.ToString() + ",");
                
                /*20*/

                sql.Append(DB.SQuote(m_froogledescription) + ",");
                sql.Append(DB.SQuote(m_relatedproducts) + ",");
                sql.Append(DB.SQuote(m_upsellproducts) + ",");
                sql.Append(DB.SQuote(m_upsellproductdiscountpercentage.ToString()) + ",");
                sql.Append(DB.SQuote(m_requiresproducts) + ",");
                sql.Append(DB.SQuote(m_sekeywords) + ",");
                sql.Append(DB.SQuote(m_sedescription) + ",");
                sql.Append(DB.SQuote(m_setitle) + ",");
                sql.Append(DB.SQuote(m_senoscript) + ",");
                sql.Append(DB.SQuote(m_sealttext) + ",");
                sql.Append(DB.SQuote(m_sku) + ",");
                sql.Append(m_pagesize.ToString() + ",");
                sql.Append(m_colwidth.ToString() + ",");
                sql.Append(DB.SQuote(m_xmlpackage) + ",");
                sql.Append(DB.SQuote(m_manufacturerpartnumber) + ",");
                sql.Append(m_salespromptid.ToString() + ",");
                sql.Append(DB.SQuote(m_spectitle) + ",");
                sql.Append(DB.SQuote(m_speccall) + ",");
                sql.Append(CommonLogic.IIF(m_published, "1", "0") + ",");
                
                /*40*/

                sql.Append(CommonLogic.IIF(m_showbuybutton, "1", "0") + ",");
                sql.Append(CommonLogic.IIF(m_iscalltoorder, "1", "0") + ",");
                sql.Append(CommonLogic.IIF(m_hidepriceuntilcart, "1", "0") + ",");
                sql.Append(CommonLogic.IIF(m_showinproductbrowser, "1", "0") + ",");
                sql.Append(CommonLogic.IIF(m_excludefrompricefeeds, "1", "0") + ",");
                sql.Append(CommonLogic.IIF(m_isakit, "1", "0") + ",");
                sql.Append(CommonLogic.IIF(m_isapack, "1", "0") + ",");
                sql.Append(m_packsize.ToString() + ",");
                sql.Append(CommonLogic.IIF(m_trackinventorybysizeandcolor, "1", "0") + ",");
                sql.Append(CommonLogic.IIF(m_trackinventorybysize, "1", "0") + ",");
                sql.Append(CommonLogic.IIF(m_trackinventorybycolor, "1", "0") + ",");
                sql.Append(CommonLogic.IIF(m_requiresregistration, "1", "0") + ",");
                sql.Append(CommonLogic.IIF(m_specsinline, "1", "0") + ",");
                sql.Append(DB.SQuote(m_misctext) + ",");
                sql.Append(DB.SQuote(m_swatchimagemap) + ",");
                sql.Append(m_quantitydiscountid.ToString() + ",");
                sql.Append(m_taxclassid.ToString() + ",");
                sql.Append(DB.SQuote(Localization.ToDBShortDateString(m_availablestartdate)) + ",");
                sql.Append(DB.SQuote(Localization.ToDBShortDateString(m_availablestopdate)));

                /*59*/

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
        /// Updates the product in the database with the properties from this product object
        /// </summary>
        /// <returns>Boolean value indicating success</returns>
        public bool Update()
        {
            bool success = true;

            try
            {
                StringBuilder sql = new StringBuilder();

                sql.Append("update dbo.Product set ");

                sql.Append("ProductGUID=" + DB.SQuote(m_productguid) + ",");
                sql.Append("Name=" + DB.SQuote(m_name) + ",");
                sql.Append("SEName=" + DB.SQuote(m_sename) + ",");
                sql.Append("ContentsBGColor=" + DB.SQuote(m_contentsbgcolor) + ",");
                sql.Append("PageBGColor=" + DB.SQuote(m_pagebgcolor) + ",");
                sql.Append("GraphicsColor=" + DB.SQuote(m_graphicscolor) + ",");
                sql.Append("ImageFilenameOverride=" + DB.SQuote(m_imagefilenameoverride) + ",");
                sql.Append("Summary=" + DB.SQuote(m_summary) + ",");
                sql.Append("Description=" + DB.SQuote(m_description) + ",");
                sql.Append("ExtensionData=" + DB.SQuote(m_extensiondata) + ",");
                sql.Append("ExtensionData2=" + DB.SQuote(m_extensiondata2) + ",");
                sql.Append("ExtensionData3=" + DB.SQuote(m_extensiondata3) + ",");
                sql.Append("ExtensionData4=" + DB.SQuote(m_extensiondata4) + ",");
                sql.Append("ExtensionData5=" + DB.SQuote(m_extensiondata5) + ",");
                sql.Append("ColorOptionPrompt=" + DB.SQuote(m_coloroptionprompt) + ",");
                sql.Append("SizeOptionPrompt=" + DB.SQuote(m_sizeoptionprompt) + ",");
                sql.Append("RequiresTextOption=" + CommonLogic.IIF(m_requirestextoption, "1", "0") + ",");
                sql.Append("TextOptionPrompt=" + DB.SQuote(m_textoptionprompt) + ",");
                sql.Append("TextOptionMaxLength=" + m_textoptionmaxlength + ",");
                sql.Append("FroogleDescription=" + DB.SQuote(m_froogledescription) + ",");
                sql.Append("RelatedProducts=" + DB.SQuote(m_relatedproducts) + ",");
                sql.Append("UpsellProducts=" + DB.SQuote(m_upsellproducts) + ",");
                sql.Append("UpsellProductDiscountPercentage=" + DB.SQuote(m_upsellproductdiscountpercentage.ToString()) + ",");
                sql.Append("RequiresProducts=" + DB.SQuote(m_requiresproducts) + ",");
                sql.Append("SEKeywords=" + DB.SQuote(m_sekeywords) + ",");
                sql.Append("SEDescription=" + DB.SQuote(m_sedescription) + ",");
                sql.Append("SETitle=" + DB.SQuote(m_setitle) + ",");
                sql.Append("SENoScript=" + DB.SQuote(m_senoscript) + ",");
                sql.Append("SEAltText=" + DB.SQuote(m_sealttext) + ",");
                sql.Append("SKU=" + DB.SQuote(m_sku) + ",");
                sql.Append("PageSize=" + m_pagesize.ToString() + ",");
                sql.Append("ColWidth=" + m_colwidth.ToString() + ",");
                sql.Append("XmlPackage=" + DB.SQuote(m_xmlpackage) + ",");
                sql.Append("ManufacturerPartNumber=" + DB.SQuote(m_manufacturerpartnumber) + ",");
                sql.Append("SalesPromptID=" + m_salespromptid.ToString() + ",");
                sql.Append("SpecTitle=" + DB.SQuote(m_spectitle) + ",");
                sql.Append("SpecCall=" + DB.SQuote(m_speccall) + ",");
                sql.Append("Published=" + CommonLogic.IIF(m_published, "1", "0") + ",");
                sql.Append("ShowBuyButton=" + CommonLogic.IIF(m_showbuybutton, "1", "0") + ",");
                sql.Append("IsCallToOrder=" + CommonLogic.IIF(m_iscalltoorder, "1", "0") + ",");
                sql.Append("HidePriceUntilCart=" + CommonLogic.IIF(m_hidepriceuntilcart, "1", "0") + ",");
                sql.Append("ShowInProductBrowser=" + CommonLogic.IIF(m_showinproductbrowser, "1", "0") + ",");
                sql.Append("ExcludeFromPriceFeeds=" + CommonLogic.IIF(m_excludefrompricefeeds, "1", "0") + ",");
                sql.Append("IsAKit=" + CommonLogic.IIF(m_isakit, "1", "0") + ",");
                sql.Append("IsAPack=" + CommonLogic.IIF(m_isapack, "1", "0") + ",");
                sql.Append("PackSize=" + m_packsize.ToString() + ",");
                sql.Append("TrackInventoryBySizeAndColor=" + CommonLogic.IIF(m_trackinventorybysizeandcolor, "1", "0") + ",");
                sql.Append("TrackInventoryBySize=" + CommonLogic.IIF(m_trackinventorybysize, "1", "0") + ",");
                sql.Append("TrackInventoryByColor=" + CommonLogic.IIF(m_trackinventorybycolor, "1", "0") + ",");
                sql.Append("RequiresRegistration=" + CommonLogic.IIF(m_requiresregistration, "1", "0") + ",");
                sql.Append("SpecsInline=" + CommonLogic.IIF(m_specsinline, "1", "0") + ",");
                sql.Append("MiscText=" + DB.SQuote(m_misctext) + ",");
                sql.Append("SwatchImageMap=" + DB.SQuote(m_swatchimagemap) + ",");
                sql.Append("QuantityDiscountID=" + m_quantitydiscountid + ",");
                sql.Append("TaxClassID=" + m_taxclassid.ToString() + ",");
                sql.Append("AvailableStartDate=" + DB.SQuote(Localization.ToDBShortDateString(m_availablestartdate)) + ",");
                sql.Append("AvailableStopDate=" + DB.SQuote(Localization.ToDBShortDateString(m_availablestopdate)));

                sql.Append(" where ProductID=" + m_productid.ToString());

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
        /// Soft Deletes the product (sets Deleted=1) in the database
        /// </summary>
        /// <returns>Boolean value indicating success</returns>
        public bool Delete()
        {
            bool success = true;

            try
            {
                m_deleted = true;
                DB.ExecuteSQL("update dbo.Product set Deleted=1 where ProductID=" + m_productid.ToString());
            }
            catch (Exception ex)
            {
                SysLog.LogException(ex, MessageTypeEnum.DatabaseException, MessageSeverityEnum.Error);
                success = false;
            }

            return success;
        }

        /// <summary>
        /// UnDeletes a soft deleted (set Deleted=0) product in the database
        /// </summary>
        /// <returns>Boolean value indicating success</returns>
        public bool UnDelete()
        {
            bool success = true;

            try
            {
                m_deleted = false;
                DB.ExecuteSQL("update dbo.Product set Deleted=0 where ProductID=" + m_productid.ToString());
            }
            catch (Exception ex)
            {
                SysLog.LogException(ex, MessageTypeEnum.DatabaseException, MessageSeverityEnum.Error);
                success = false;
            }

            return success;
        }

        /// <summary>
        /// Nukes the product (removes the database record) from the database
        /// </summary>
        /// <returns>Boolean value indicating success</returns>
        public bool Nuke()
        {
            bool success = true;

            try
            {
                DB.ExecuteSQL("delete dbo.Product where ProductID=" + m_productid.ToString());
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
        /// Resets all properties to default values
        /// </summary>
        public void Reset()
        {
            m_productid = 0;
            m_productguid = String.Empty;
            m_name = String.Empty;
            m_summary = String.Empty;
            m_description = String.Empty;
            m_sekeywords = String.Empty;
            m_sedescription = String.Empty;
            m_spectitle = String.Empty;
            m_misctext = String.Empty;
            m_swatchimagemap = String.Empty;
            m_isfeaturedteaser = String.Empty;
            m_froogledescription = String.Empty;
            m_setitle = String.Empty;
            m_senoscript = String.Empty;
            m_sealttext = String.Empty;
            m_sizeoptionprompt = String.Empty;
            m_coloroptionprompt = String.Empty;
            m_textoptionprompt = String.Empty;
            m_producttypeid = 0;
            m_taxclassid = 0;
            m_sku = String.Empty;
            m_manufacturerpartnumber = String.Empty;
            m_salespromptid = 0;
            m_speccall = String.Empty;
            m_specsinline = false;
            m_isfeatured = false;
            m_xmlpackage = String.Empty;
            m_colwidth = 0;
            m_published = false;
            m_wholesale = false;
            m_requiresregistration = false;
            m_looks = 0;
            m_notes = String.Empty;
            m_quantitydiscountid = 0;
            m_relatedproducts = String.Empty;
            m_upsellproducts = String.Empty;
            m_upsellproductdiscountpercentage = 0.00M;
            m_relateddocuments = String.Empty;
            m_trackinventorybysizeandcolor = false;
            m_trackinventorybysize = false;
            m_trackinventorybycolor = false;
            m_isakit = false;
            m_showinproductbrowser = false;
            m_isapack = false;
            m_packsize = 0;
            m_showbuybutton = false;
            m_requiresproducts = String.Empty;
            m_hidepriceuntilcart = false;
            m_iscalltoorder = false;
            m_excludefrompricefeeds = false;
            m_requirestextoption = false;
            m_textoptionmaxlength = 0;
            m_sename = String.Empty;
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
            m_issystem = false;
            m_deleted = false;
            m_createdon = DateTime.MinValue;
            m_pagesize = 0;
            m_warehouselocation = String.Empty;
            m_availablestartdate = DateTime.MinValue;
            m_availablestopdate = DateTime.MaxValue;
            m_skinid = 0;
            m_templatename = String.Empty;

            m_productvariants = new List<ProductVariant>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rs"></param>
        public void LoadFromRS(IDataReader rs)
        {
            m_productid = DB.RSFieldInt(rs, "ProductID");
            m_productguid = DB.RSFieldGUID(rs, "ProductGUID");
            m_name = DB.RSField(rs, "Name");
            m_summary = DB.RSField(rs, "Summary");
            m_description = DB.RSField(rs, "Description");
            m_sekeywords = DB.RSField(rs, "SEKeywords");
            m_sedescription = DB.RSField(rs, "SEDescription");
            m_spectitle = DB.RSField(rs, "SpecTitle");
            m_misctext = DB.RSField(rs, "MiscText");
            m_swatchimagemap = DB.RSField(rs, "SwatchImageMap");
            m_isfeaturedteaser = DB.RSField(rs, "IsFeaturedTeaser");
            m_froogledescription = DB.RSField(rs, "FroogleDescription");
            m_setitle = DB.RSField(rs, "SETitle");
            m_senoscript = DB.RSField(rs, "SENoScript");
            m_sealttext = DB.RSField(rs, "SEAltText");
            m_sizeoptionprompt = DB.RSField(rs, "SizeOptionPrompt");
            m_coloroptionprompt = DB.RSField(rs, "ColorOptionPrompt");
            m_textoptionprompt = DB.RSField(rs, "TextOptionPrompt");
            m_producttypeid = DB.RSFieldInt(rs, "ProductTypeID");
            m_taxclassid = DB.RSFieldInt(rs, "TaxClassID");
            m_sku = DB.RSField(rs, "SKU");
            m_manufacturerpartnumber = DB.RSField(rs, "ManufacturerPartNumber");
            m_salespromptid = DB.RSFieldInt(rs, "SalesPromptID");
            m_speccall = DB.RSField(rs, "SpecCall");
            m_specsinline = DB.RSFieldBool(rs, "SpecsInline");
            m_isfeatured = DB.RSFieldBool(rs, "IsFeatured");
            m_xmlpackage = DB.RSField(rs, "XmlPackage");
            m_colwidth = DB.RSFieldInt(rs, "ColWidth");
            m_published = DB.RSFieldBool(rs, "Published");
            m_wholesale = DB.RSFieldBool(rs, "Wholesale");
            m_requiresregistration = DB.RSFieldBool(rs, "RequiresRegistration");
            m_looks = DB.RSFieldInt(rs, "Looks");
            m_notes = DB.RSField(rs, "Notes");
            m_quantitydiscountid = DB.RSFieldInt(rs, "QuantityDiscountID");
            m_relatedproducts = DB.RSField(rs, "RelatedProducts");
            m_upsellproducts = DB.RSField(rs, "UpsellProducts");
            m_upsellproductdiscountpercentage = DB.RSFieldDecimal(rs, "UpsellProductDiscountPercentage");
            m_relateddocuments = DB.RSField(rs, "RelatedDocuments");
            m_trackinventorybysizeandcolor = DB.RSFieldBool(rs, "TrackInventoryBySizeAndColor");
            m_trackinventorybysize = DB.RSFieldBool(rs, "TrackInventoryBySize");
            m_trackinventorybycolor = DB.RSFieldBool(rs, "TrackInventoryByColor");
            m_isakit = DB.RSFieldBool(rs, "IsAKit");
            m_showinproductbrowser = DB.RSFieldBool(rs, "ShowInProductBrowser");
            m_isapack = DB.RSFieldBool(rs, "IsAPack");
            m_packsize = DB.RSFieldInt(rs, "PackSize");
            m_showbuybutton = DB.RSFieldBool(rs, "ShowBuyButton");
            m_requiresproducts = DB.RSField(rs, "RequiresProducts");
            m_hidepriceuntilcart = DB.RSFieldBool(rs, "HidePriceUntilCart");
            m_iscalltoorder = DB.RSFieldBool(rs, "IsCalltoOrder");
            m_excludefrompricefeeds = DB.RSFieldBool(rs, "ExcludeFromPriceFeeds");
            m_requirestextoption = DB.RSFieldBool(rs, "RequiresTextOption");
            m_textoptionmaxlength = DB.RSFieldInt(rs, "TextOptionMaxLength");
            m_sename = DB.RSField(rs, "SEName");
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
            m_issystem = DB.RSFieldBool(rs, "IsSystem");
            m_deleted = DB.RSFieldBool(rs, "Deleted");
            m_createdon = DB.RSFieldDateTime(rs, "CreatedOn");
            m_pagesize = DB.RSFieldInt(rs, "PageSize");
            m_warehouselocation = DB.RSField(rs, "WarehouseLocation");
            m_availablestartdate = DB.RSFieldDateTime(rs, "AvailableStartDate");
            m_availablestopdate = DB.RSFieldDateTime(rs, "AvailableStopDate");
            m_skinid = DB.RSFieldInt(rs, "SkinID");
            m_templatename = DB.RSField(rs, "TemplateName");

            //m_productvariants = GetVariants(true);
            m_productvariants = new List<ProductVariant>();
        }

        #endregion

        #region Static Methods

        public static List<Product> GetProducts(bool includeDeleted)
        {
            List<Product> allproducts = new List<Product>();

            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                using (IDataReader rs = DB.GetRS("select * from Product with(NOLOCK)" + CommonLogic.IIF(includeDeleted, String.Empty, " where Deleted=0"), conn))
                {
                    while (rs.Read())
                    {
                        Product p = new Product(rs);
                        allproducts.Add(p);
                    }

                    rs.Close();
                    rs.Dispose();
                }

                conn.Close();
                conn.Dispose();
            }

            return allproducts;
        }

        public static String DisplayStockHint(string sProductID, string sVariantID, string page, string className, string renderAsElement)
        {
            Customer ThisCustomer = AppLogic.GetCurrentCustomer();

            StringBuilder results = new StringBuilder();

            InputValidator IV = new InputValidator("DisplayProductStockHint");
            int productID = IV.ValidateInt("ProductID", sProductID);
            int variantID = IV.ValidateInt("VariantID", sVariantID);

            bool trackInventoryBySizeAndColor = AppLogic.ProductTracksInventoryBySizeAndColor(productID);

            bool probablyOutOfStock = AppLogic.ProbablyOutOfStock(productID, variantID, trackInventoryBySizeAndColor, page);

            bool displayOutOfStockOnProductPages = AppLogic.AppConfigBool("DisplayOutOfStockOnProductPages");
            bool displayOutOfStockOnEntityPage = AppLogic.AppConfigBool("DisplayOutOfStockOnEntityPages");

            if (renderAsElement == string.Empty)
            {
                if (probablyOutOfStock)
                {
                    // the css is always 3 set and you can customized
                    // and create new one just pass as parameter
                    //(Default - StockHint for instock - out-stock-hint and outstock - in-stock-hint)
                    results.AppendLine();

                    // display Out of Stock
                    if (page.Equals("Product", StringComparison.InvariantCultureIgnoreCase) && displayOutOfStockOnProductPages)
                    {
                        results.Append(AppLogic.GetString("OutofStock.DisplayOutOfStockOnProductPage", ThisCustomer.SkinID, ThisCustomer.LocaleSetting));
                    }
                    else if (page.Equals("Entity", StringComparison.InvariantCultureIgnoreCase) && displayOutOfStockOnEntityPage)
                    {
                        results.Append(AppLogic.GetString("OutofStock.DisplayOutOfStockOnEntityPage", ThisCustomer.SkinID, ThisCustomer.LocaleSetting));
                    }
                }
                else
                {
                    results.AppendLine();
                    // display "In Stock"
                    if (page.Equals("Product", StringComparison.InvariantCultureIgnoreCase) && displayOutOfStockOnProductPages)
                    {
                        results.Append(AppLogic.GetString("OutofStock.DisplayInStockOnProductPage", ThisCustomer.SkinID, ThisCustomer.LocaleSetting));
                    }
                    else if (page.Equals("Entity", StringComparison.InvariantCultureIgnoreCase) && displayOutOfStockOnEntityPage)
                    {
                        results.Append(AppLogic.GetString("OutofStock.DisplayInStockOnEntityPage", ThisCustomer.SkinID, ThisCustomer.LocaleSetting));
                    }
                }
            }
            else
            {
                if (probablyOutOfStock)
                {
                    // the css is always 3 set and you can customized
                    // and create new one just pass as parameter
                    //(Default - StockHint for instock - out-stock-hint and outstock - in-stock-hint)

					className = string.Format("{0} {1}", className, "out-stock-hint");
                    results.AppendLine();

                    // display Out of Stock
                    if (page.Equals("Product", StringComparison.InvariantCultureIgnoreCase) && displayOutOfStockOnProductPages)
                    {
                        results.AppendFormat("<{0} class=\"{1}\" >\n", renderAsElement, className);
                        results.Append(AppLogic.GetString("OutofStock.DisplayOutOfStockOnProductPage", ThisCustomer.SkinID, ThisCustomer.LocaleSetting));
                        results.AppendFormat("</{0}>\n", renderAsElement);
                    }
                    else if (page.Equals("Entity", StringComparison.InvariantCultureIgnoreCase) && displayOutOfStockOnEntityPage)
                    {
                        results.AppendFormat("<{0} class=\"{1}\" >\n", renderAsElement, className);
                        results.Append(AppLogic.GetString("OutofStock.DisplayOutOfStockOnEntityPage", ThisCustomer.SkinID, ThisCustomer.LocaleSetting));
                        results.AppendFormat("</{0}>\n", renderAsElement);
                    }
                }
                else
                {
					className = string.Format("{0} {1}", className, "in-stock-hint");
                    results.AppendLine();
                    // display "In Stock"
                    if (page.Equals("Product", StringComparison.InvariantCultureIgnoreCase) && displayOutOfStockOnProductPages)
                    {
                        results.AppendFormat("<{0} class=\"{1}\" >\n", renderAsElement, className);
                        results.Append(AppLogic.GetString("OutofStock.DisplayInStockOnProductPage", ThisCustomer.SkinID, ThisCustomer.LocaleSetting));
                        results.AppendFormat("</{0}>\n", renderAsElement);
                    }
                    else if (page.Equals("Entity", StringComparison.InvariantCultureIgnoreCase) && displayOutOfStockOnEntityPage)
                    {
                        results.AppendFormat("<{0} class=\"{1}\" >\n", renderAsElement, className);
                        results.Append(AppLogic.GetString("OutofStock.DisplayInStockOnEntityPage", ThisCustomer.SkinID, ThisCustomer.LocaleSetting));
                        results.AppendFormat("</{0}>\n", renderAsElement);
                    }
                }
            }
            results.AppendLine();

            return results.ToString().Trim();
        }
        #endregion
    }

    public class GridProduct
    {
        #region Private Variables

        private int m_productid;
        private String m_name;
        private int m_producttypeid;
        private String m_sku;
        private bool m_published;
        private bool m_deleted;

        private List<GridProductVariant> m_productvariants;

        #endregion

        #region Constructors
        /// <summary>
        /// Constructor for GridProduct that will populate the properties from the database
        /// </summary>
        /// <param name="ProductID">The ID of the product to retrieve data for</param>
        public GridProduct()
        {
            Reset();
        }

        /// <summary>
        /// Constructor for GridProduct that will populate the properties from the database
        /// </summary>
        /// <param name="ProductID">The ID of the product to retrieve data for</param>
        public GridProduct(int ProductID)
        {
            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                using (IDataReader rs = DB.GetRS("select ProductID, Name, ProductTypeID, SKU, IsFeatured, Published, Deleted from dbo.Product with(NOLOCK) where ProductID=" + ProductID.ToString(), conn))
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
        /// Constructor for GridProduct that will populate the properties from the database
        /// </summary>
        /// <param name="rs">The IDataReader containing product data</param>
        public GridProduct(IDataReader rs)
        {
            LoadFromRS(rs);
        }

        #endregion

        #region Public Properties

        public int ProductID { get { return m_productid; } set { m_productid = value; } }
        public String Name { get { return m_name; } set { m_name = value; } }
        public String LocaleName { get { return LocalizedValue(m_name); } }
        public int ProductTypeID { get { return m_producttypeid; } set { m_producttypeid = value; } }
        public String SKU { get { return m_sku; } set { m_sku = value; } }
        public bool Published { get { return m_published; } set { m_published = value; } }
        public bool Deleted { get { return m_deleted; } set { m_deleted = value; } }
        public List<GridProductVariant> Variants { get { return m_productvariants; } set { m_productvariants = value; } }

        #endregion

        #region Public Methods

        /// <summary>
        /// Retrieves variants for the current product
        /// </summary>
        /// <param name="includeDeleted">Boolean value indicating whether to included soft-deleted (eg. Deleted=1) variants</param>
        /// <returns>A ProductVariant List of variants belonging to this product</returns>
        public void LoadVariants(bool includeDeleted)
        {
            m_productvariants = GridProductVariant.GetVariants(m_productid, includeDeleted);
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
        /// Updates the product as published
        /// </summary>
        /// <returns>Boolean value indicating success</returns>
        public bool Publish()
        {
            bool success = true;

            try
            {
                m_published = true;
                DB.ExecuteSQL("update dbo.Product set Published=1 where ProductID=" + m_productid.ToString());
            }
            catch (Exception ex)
            {
                SysLog.LogException(ex, MessageTypeEnum.DatabaseException, MessageSeverityEnum.Error);
                success = false;
            }

            return success;
        }

        /// <summary>
        /// Updates the product as unpublished
        /// </summary>
        /// <returns>Boolean value indicating success</returns>
        public bool UnPublish()
        {
            bool success = true;

            try
            {
                m_published = false;
                DB.ExecuteSQL("update dbo.Product set Published=0 where ProductID=" + m_productid.ToString());
            }
            catch (Exception ex)
            {
                SysLog.LogException(ex, MessageTypeEnum.DatabaseException, MessageSeverityEnum.Error);
                success = false;
            }

            return success;
        }

        /// <summary>
        /// Soft Deletes the product (sets Deleted=1) in the database
        /// </summary>
        /// <returns>Boolean value indicating success</returns>
        public bool Delete()
        {
            bool success = true;

            try
            {
                m_deleted = true;
                DB.ExecuteSQL("update dbo.Product set Deleted=1 where ProductID=" + m_productid.ToString());
            }
            catch (Exception ex)
            {
                SysLog.LogException(ex, MessageTypeEnum.DatabaseException, MessageSeverityEnum.Error);
                success = false;
            }

            return success;
        }

        /// <summary>
        /// UnDeletes a soft deleted (set Deleted=0) product in the database
        /// </summary>
        /// <returns>Boolean value indicating success</returns>
        public bool UnDelete()
        {
            bool success = true;

            try
            {
                m_deleted = false;
                DB.ExecuteSQL("update dbo.Product set Deleted=0 where ProductID=" + m_productid.ToString());
            }
            catch (Exception ex)
            {
                SysLog.LogException(ex, MessageTypeEnum.DatabaseException, MessageSeverityEnum.Error);
                success = false;
            }

            return success;
        }

        /// <summary>
        /// Nukes the product (removes the database record) from the database
        /// </summary>
        /// <returns>Boolean value indicating success</returns>
        public bool Nuke()
        {
            bool success = true;

            try
            {
                DB.ExecuteSQL("delete dbo.Product where ProductID=" + m_productid.ToString());
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
        /// Resets all properties to default values
        /// </summary>
        public void Reset()
        {
            m_productid = 0;
            m_name = String.Empty;
            m_producttypeid = 0;
            m_sku = String.Empty;
            m_published = false;
            m_deleted = false;

            m_productvariants = new List<GridProductVariant>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rs"></param>
        public void LoadFromRS(IDataReader rs)
        {
            m_productid = DB.RSFieldInt(rs, "ProductID");
            m_name = DB.RSField(rs, "Name");
            m_producttypeid = DB.RSFieldInt(rs, "ProductTypeID");
            m_sku = DB.RSField(rs, "SKU");
            m_deleted = DB.RSFieldBool(rs, "Deleted");
            m_published = DB.RSFieldBool(rs, "Published");

            m_productvariants = new List<GridProductVariant>();
        }

        #endregion

        #region Static Methods

        public static List<GridProduct> GetProducts(bool includeDeleted)
        {
            List<GridProduct> allproducts = new List<GridProduct>();

            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                using (IDataReader rs = DB.GetRS("select ProductID, Name, ProductTypeID, SKU, IsFeatured, Published, Deleted from dbo.Product with(NOLOCK)" + CommonLogic.IIF(includeDeleted, String.Empty, " where Deleted=0"), conn))
                {
                    while (rs.Read())
                    {
                        GridProduct p = new GridProduct(rs);
                        allproducts.Add(p);
                    }

                    rs.Close();
                    rs.Dispose();
                }

                conn.Close();
                conn.Dispose();
            }

            return allproducts;
        }

        #endregion
    }
}
