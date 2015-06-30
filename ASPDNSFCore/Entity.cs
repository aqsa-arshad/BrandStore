// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace AspDotNetStorefrontCore
{
    public class Entity
    {
        #region Private Variables

        private String m_entitytype;
        private int m_id;
        private String m_guid;
        private String m_name;
        private String m_summary;
        private String m_description;
        private String m_sekeywords;
        private String m_sedescription;
        private String m_displayprefix;
        private String m_setitle;
        private String m_senoscript;
        private String m_sealttext;
        private int m_parentid;
        private int m_colwidth;
        private bool m_sortbylooks;
        private int m_displayorder;
        private String m_xmlpackage;
        private bool m_published;
        private bool m_wholesale;
        private int m_quantitydiscountid;
        private String m_sename;
        private String m_extensiondata;
        private String m_imagefilenameoverride;
        private bool m_isimport;
        private bool m_deleted;
        private DateTime m_createdon;
        private int m_pagesize;
        private int m_taxclassid;
        private int m_skinid;
        private String m_templatename;
        private Address m_entityaddress;


        #endregion

        #region Constructors

        public Entity()
        {
            m_entitytype = String.Empty;
            m_id = 0;
            m_guid = String.Empty;
            m_name = String.Empty;
            m_summary = String.Empty;
            m_description = String.Empty;
            m_sekeywords = String.Empty;
            m_sedescription = String.Empty;
            m_displayprefix = String.Empty;
            m_setitle = String.Empty;
            m_senoscript = String.Empty;
            m_sealttext = String.Empty;
            m_parentid = 0;
            m_colwidth = 0;
            m_sortbylooks = false;
            m_displayorder = 0;
            m_xmlpackage = String.Empty;
            m_published = false;
            m_wholesale = false;
            m_quantitydiscountid = 0;
            m_sename = String.Empty;
            m_extensiondata = String.Empty;
            m_imagefilenameoverride = String.Empty;
            m_isimport = false;
            m_deleted = false;
            m_createdon = DateTime.MinValue;
            m_pagesize = 0;
            m_taxclassid = 0;
            m_skinid = 0;
            m_templatename = String.Empty;
            m_entityaddress = new Address();
        }

        public Entity(IDataReader rs, String EntityToLoad)
        {
            m_entitytype = EntityToLoad;
            m_id = DB.RSFieldInt(rs, "{0}ID".FormatWith(EntityToLoad));
            m_guid = DB.RSFieldGUID(rs, "{0}GUID".FormatWith(EntityToLoad));
            m_name = DB.RSField(rs, "Name");
            m_summary = DB.RSField(rs, "Summary");
            m_description = DB.RSField(rs, "Description");
            m_sekeywords = DB.RSField(rs, "SEKeywords");
            m_sedescription = DB.RSField(rs, "SEDescription");
            m_displayprefix = DB.RSField(rs, "DisplayPrefix");
            m_setitle = DB.RSField(rs, "SETitle");
            m_senoscript = DB.RSField(rs, "SENoScript");
            m_sealttext = DB.RSField(rs, "SEAltText");
            m_parentid = DB.RSFieldInt(rs, "Parent{0}ID".FormatWith(EntityToLoad));
            m_colwidth = DB.RSFieldInt(rs, "ColWidth");
            m_sortbylooks = DB.RSFieldBool(rs, "SortByLooks");
            m_displayorder = DB.RSFieldInt(rs, "DisplayOrder");
            m_xmlpackage = DB.RSField(rs, "XmlPackage");
            m_published = DB.RSFieldBool(rs, "Published");
            m_wholesale = DB.RSFieldBool(rs, "Wholesale");
            m_quantitydiscountid = DB.RSFieldInt(rs, "QuantityDiscountID");
            m_sename = DB.RSField(rs, "SEName");
            m_extensiondata = DB.RSField(rs, "ExtensionData");
            m_imagefilenameoverride = DB.RSField(rs, "ImageFilenameOverride");
            m_isimport = DB.RSFieldBool(rs, "IsImport");
            m_deleted = DB.RSFieldBool(rs, "Deleted");
            m_createdon = DateTime.MinValue;
            m_pagesize = DB.RSFieldInt(rs, "PageSize");
            m_taxclassid = DB.RSFieldInt(rs, "TaxClassID");
            m_skinid = DB.RSFieldInt(rs, "SkinID");
            m_templatename = DB.RSField(rs, "TemplateName");
            if (HasAddress)
            {
                m_entityaddress = new Address();

                m_entityaddress.Address1 = DB.RSField(rs, "Address1");
                m_entityaddress.Address2 = DB.RSField(rs, "Address2");
                m_entityaddress.Suite = DB.RSField(rs, "Suite");
                m_entityaddress.City = DB.RSField(rs, "City");
                m_entityaddress.State = DB.RSField(rs, "State");
                m_entityaddress.Zip = DB.RSField(rs, "ZipCode");
                m_entityaddress.Country = DB.RSField(rs, "Country");
                m_entityaddress.Phone = DB.RSField(rs, "Phone");
                m_entityaddress.Fax = DB.RSField(rs, "Fax");
                m_entityaddress.Url = DB.RSField(rs, "URL");
                m_entityaddress.EMail = DB.RSField(rs, "EMail");
            }
        }

        public Entity(int EntityIDToLoad, String EntityTypeToLoad)
        {
            m_entitytype = LoadEntityType(EntityTypeToLoad);

            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS("select * from dbo.{0} with(NOLOCK) where {0}ID=".FormatWith(m_entitytype) + EntityIDToLoad.ToString(), conn))
                {
                    if (rs.Read())
                    {
                        m_id = DB.RSFieldInt(rs, "{0}ID".FormatWith(m_entitytype));
                        m_guid = DB.RSFieldGUID(rs, "{0}GUID".FormatWith(m_entitytype));
                        m_name = DB.RSField(rs, "Name");
                        m_summary = DB.RSField(rs, "Summary");
                        m_description = DB.RSField(rs, "Description");
                        m_sekeywords = DB.RSField(rs, "SEKeywords");
                        m_sedescription = DB.RSField(rs, "SEDescription");
                        m_displayprefix = DB.RSField(rs, "DisplayPrefix");
                        m_setitle = DB.RSField(rs, "SETitle");
                        m_senoscript = DB.RSField(rs, "SENoScript");
                        m_sealttext = DB.RSField(rs, "SEAltText");
                        m_parentid = DB.RSFieldInt(rs, "Parent{0}ID".FormatWith(m_entitytype));
                        m_colwidth = DB.RSFieldInt(rs, "ColWidth");
                        m_sortbylooks = DB.RSFieldBool(rs, "SortByLooks");
                        m_displayorder = DB.RSFieldInt(rs, "DisplayOrder");
                        m_xmlpackage = DB.RSField(rs, "XmlPackage");
                        m_published = DB.RSFieldBool(rs, "Published");
                        m_wholesale = DB.RSFieldBool(rs, "Wholesale");
                        m_quantitydiscountid = DB.RSFieldInt(rs, "QuantityDiscountID");
                        m_sename = DB.RSField(rs, "SEName");
                        m_extensiondata = DB.RSField(rs, "ExtensionData");
                        m_imagefilenameoverride = DB.RSField(rs, "ImageFilenameOverride");
                        m_isimport = DB.RSFieldBool(rs, "IsImport");
                        m_deleted = DB.RSFieldBool(rs, "Deleted");
                        m_createdon = DateTime.MinValue;
                        m_pagesize = DB.RSFieldInt(rs, "PageSize");
                        m_taxclassid = DB.RSFieldInt(rs, "TaxClassID");
                        m_skinid = DB.RSFieldInt(rs, "SkinID");
                        m_templatename = DB.RSField(rs, "TemplateName");
                        if (HasAddress)
                        {
                            m_entityaddress = new Address();

                            m_entityaddress.Address1 = DB.RSField(rs, "Address1");
                            m_entityaddress.Address2 = DB.RSField(rs, "Address2");
                            m_entityaddress.Suite = DB.RSField(rs, "Suite");
                            m_entityaddress.City = DB.RSField(rs, "City");
                            m_entityaddress.State = DB.RSField(rs, "State");
                            m_entityaddress.Zip = DB.RSField(rs, "ZipCode");
                            m_entityaddress.Country = DB.RSField(rs, "Country");
                            m_entityaddress.Phone = DB.RSField(rs, "Phone");
                            m_entityaddress.Fax = DB.RSField(rs, "Fax");
                            m_entityaddress.Url = DB.RSField(rs, "URL");
                            m_entityaddress.EMail = DB.RSField(rs, "EMail");
                        }
                    }

                    rs.Close();
                    rs.Dispose();
                }
                conn.Close();
                conn.Dispose();
            }
        }

        #endregion

        #region Public Properties

        public String EntityType { get { return LoadEntityType(m_entitytype); } set { m_entitytype = value; } }
        public int ID { get { return m_id; } set { m_id = value; } }
        public String GUID { get { return m_guid; } set { m_guid = value; } }
        public String Name { get { return m_name; } set { m_name = value; } }
        public String LocaleName { get { return LocalizedValue(m_name); } }
        public String Summary { get { return m_summary; } set { m_summary = value; } }
        public String Description { get { return m_description; } set { m_description = value; } }
        public String SEKeywords { get { return m_sekeywords; } set { m_sekeywords = value; } }
        public String SEDescription { get { return m_sedescription; } set { m_sedescription = value; } }
        public String DisplayPrefix { get { return m_displayprefix; } set { m_displayprefix = value; } }
        public String SETitle { get { return m_setitle; } set { m_setitle = value; } }
        public String SENoScript { get { return m_senoscript; } set { m_senoscript = value; } }
        public String SEAltText { get { return m_sealttext; } set { m_sealttext = value; } }
        public int ParentID { get { return m_parentid; } set { m_parentid = value; } }
        public int ColWidth { get { return m_colwidth; } set { m_colwidth = value; } }
        public bool SortByLooks { get { return m_sortbylooks; } set { m_sortbylooks = value; } }
        public int DisplayOrder { get { return m_displayorder; } set { m_displayorder = value; } }
        public String XmlPackage { get { return m_xmlpackage; } set { m_xmlpackage = value; } }
        public bool Published { get { return m_published; } set { m_published = value; } }
        public bool Wholesale { get { return m_wholesale; } set { m_wholesale = value; } }
        public int QuantityDiscountID { get { return m_quantitydiscountid; } set { m_quantitydiscountid = value; } }
        public String SEName { get { return m_sename; } set { m_sename = value; } }
        public String ExtensionData { get { return m_extensiondata; } set { m_extensiondata = value; } }
        public String ImageFilenameOverride { get { return m_imagefilenameoverride; } set { m_imagefilenameoverride = value; } }
        public bool IsImport { get { return m_isimport; } set { m_isimport = value; } }
        public bool Deleted { get { return m_deleted; } set { m_deleted = value; } }
        public DateTime CreatedOn { get { return m_createdon; } set { m_createdon = value; } }
        public int PageSize { get { return m_pagesize; } set { m_pagesize = value; } }
        public int TaxClassID { get { return m_taxclassid; } set { m_taxclassid = value; } }
        public int SkinID { get { return m_skinid; } set { m_skinid = value; } }
        public String TemplateName { get { return m_templatename; } set { m_templatename = value; } }
        public bool HasAddress { get { return m_entitytype.EqualsIgnoreCase("Affiliate") || m_entitytype.EqualsIgnoreCase("Distributor") || m_entitytype.EqualsIgnoreCase("Manufacturer"); } }
        public Address EntityAddress { get { return m_entityaddress; } set { m_entityaddress = value; } }
        public bool HasChildren { get { if (m_id.Equals(0)) { return false; } return DB.GetSqlN("select count(*) as N from dbo.{0} with(NOLOCK) where Parent{0}ID=".FormatWith(EntityType) + m_id.ToString()) > 0; } }
        public int NumChildren { get { return DB.GetSqlN("select count(*) as N from dbo.{0} with(NOLOCK) where Parent{0}ID=".FormatWith(EntityType) + m_id.ToString()); } }
        #endregion

        #region Public Methods

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
        /// Updates the entity as published
        /// </summary>
        /// <returns>Boolean value indicating success</returns>
        public bool Publish()
        {
            bool success = true;

            try
            {
                m_published = true;
                DB.ExecuteSQL("update dbo.{0} set Published=1 where {0}ID=".FormatWith(EntityType) + m_id.ToString());
            }
            catch (Exception ex)
            {
                SysLog.LogException(ex, MessageTypeEnum.DatabaseException, MessageSeverityEnum.Error);
                success = false;
            }

            return success;
        }

        /// <summary>
        /// Updates the entity as unpublished
        /// </summary>
        /// <returns>Boolean value indicating success</returns>
        public bool UnPublish()
        {
            bool success = true;

            try
            {
                m_published = false;
                DB.ExecuteSQL("update dbo.{0} set Published=0 where {0}ID=".FormatWith(EntityType) + m_id.ToString());
            }
            catch (Exception ex)
            {
                SysLog.LogException(ex, MessageTypeEnum.DatabaseException, MessageSeverityEnum.Error);
                success = false;
            }

            return success;
        }

        /// <summary>
        /// Creates the entity in the database
        /// </summary>
        /// <returns>Boolean value indicating success</returns>
        public bool Create()
        {
            bool success = true;

            try
            {
                StringBuilder sql = new StringBuilder();

                sql.Append("insert into dbo." + m_entitytype + "(" + m_entitytype + "GUID,Name,SEName," +
                            CommonLogic.IIF(HasAddress,"Address1,Address2,Suite,City,State,ZipCode,Country,Phone,FAX,URL,EMail,","") + 
                            "TemplateName,SkinID,ImageFilenameOverride," +
                            "Parent" + m_entitytype + "ID," +
                            "Summary,Description,ExtensionData,SEKeywords,SEDescription,SETitle,SENoScript,SEAltText,Published," +
                            "PageSize,ColWidth,XmlPackage,SortByLooks,QuantityDiscountID) values(");

                sql.Append(DB.SQuote(m_guid) + ",");
                sql.Append(DB.SQuote(m_name) + ",");
                sql.Append(DB.SQuote(m_sename) + ",");
                if (HasAddress)
                {
                    sql.Append(DB.SQuote(m_entityaddress.Address1) + ",");
                    sql.Append(DB.SQuote(m_entityaddress.Address2) + ",");
                    sql.Append(DB.SQuote(m_entityaddress.Suite) + ",");
                    sql.Append(DB.SQuote(m_entityaddress.City) + ",");
                    sql.Append(DB.SQuote(m_entityaddress.State) + ",");
                    sql.Append(DB.SQuote(m_entityaddress.Zip) + ",");
                    sql.Append(DB.SQuote(m_entityaddress.Country) + ",");
                    sql.Append(DB.SQuote(m_entityaddress.Phone) + ",");
                    sql.Append(DB.SQuote(m_entityaddress.Fax) + ",");
                    sql.Append(DB.SQuote(m_entityaddress.Url) + ",");
                    sql.Append(DB.SQuote(m_entityaddress.EMail) + ",");
                }
                sql.Append(DB.SQuote(m_templatename) + ",");
                sql.Append(m_skinid.ToString() + ",");
                sql.Append(DB.SQuote(m_imagefilenameoverride) + ",");
                sql.Append(m_parentid.ToString() + ",");
                sql.Append(DB.SQuote(m_summary) + ",");
                sql.Append(DB.SQuote(m_description) + ",");
                sql.Append(DB.SQuote(m_extensiondata) + ",");
                sql.Append(DB.SQuote(m_sekeywords) + ",");
                sql.Append(DB.SQuote(m_sedescription) + ",");                
                sql.Append(DB.SQuote(m_setitle) + ",");                
                sql.Append(DB.SQuote(m_senoscript) + ",");               
                sql.Append(DB.SQuote(m_sealttext) + ",");
                sql.Append(CommonLogic.IIF(m_published, 1, 0).ToString() + ",");
                sql.Append(m_pagesize.ToString() + ",");
                sql.Append(m_colwidth.ToString() + ",");
                sql.Append(DB.SQuote(m_xmlpackage.ToLowerInvariant()) + ",");                
                sql.Append(CommonLogic.IIF(m_sortbylooks, 1, 0).ToString() + ",");
                sql.Append(m_quantitydiscountid.ToString());
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
        /// Updates the entity in the database with the properties in the current entity object
        /// </summary>
        /// <returns>Boolean value indicating success</returns>
        public bool Update()
        {
            bool success = true;

            try
            {
                StringBuilder sql = new StringBuilder();

                // ok to update:
                sql.Append("update dbo." + m_entitytype + " set ");
                sql.Append("Name=" + DB.SQuote(m_name) + ",");
                sql.Append("SEName=" + DB.SQuote(m_sename) + ",");

                if (HasAddress)
                {
                    //Address1,Address2,Suite,City,State,ZipCode,Country,Phone,FAX,URL,EMail

                    sql.Append("Address1=" + DB.SQuote(m_entityaddress.Address1) + ",");
                    sql.Append("Address2=" + DB.SQuote(m_entityaddress.Address2) + ",");
                    sql.Append("Suite=" + DB.SQuote(m_entityaddress.Suite) + ",");
                    sql.Append("City=" + DB.SQuote(m_entityaddress.City) + ",");
                    sql.Append("State=" + DB.SQuote(m_entityaddress.State) + ",");
                    sql.Append("ZipCode=" + DB.SQuote(m_entityaddress.Zip) + ",");
                    sql.Append("Country=" + DB.SQuote(m_entityaddress.Country) + ",");
                    sql.Append("Phone=" + DB.SQuote(m_entityaddress.Phone) + ",");
                    sql.Append("FAX=" + DB.SQuote(m_entityaddress.Fax) + ",");
                    sql.Append("URL=" + DB.SQuote(m_entityaddress.Url) + ",");
                    sql.Append("EMail=" + DB.SQuote(m_entityaddress.EMail) + ",");
                }

                sql.Append("TemplateName=" + DB.SQuote(m_templatename) + ",");
                sql.Append("SkinID=" + m_skinid.ToString() + ",");
                sql.Append("ImageFilenameOverride=" + DB.SQuote(m_imagefilenameoverride) + ",");
                sql.Append("Parent" + m_entitytype + "ID=" + m_parentid.ToString() + ",");
                sql.Append("Summary=" + DB.SQuote(m_summary) + ",");
                sql.Append("Description=" + DB.SQuote(m_description) + ",");
                sql.Append("ExtensionData=" + DB.SQuote(m_extensiondata) + ",");

                sql.Append("SEKeyWords=" + DB.SQuote(m_sekeywords) + ",");
                sql.Append("SEDescription=" + DB.SQuote(m_sedescription) + ",");
                sql.Append("SETitle=" + DB.SQuote(m_setitle) + ",");
                sql.Append("SENoScript=" + DB.SQuote(m_senoscript) + ",");
                sql.Append("SEAltText=" + DB.SQuote(m_sealttext) + ",");

                sql.Append("Published=" + CommonLogic.IIF(m_published, 1, 0).ToString() + ",");
                sql.Append("PageSize=" + m_pagesize.ToString() + ",");
                sql.Append("ColWidth=" + m_colwidth.ToString() + ",");
                sql.Append("XmlPackage=" + DB.SQuote(m_xmlpackage) + ",");

                sql.Append("SortByLooks=" + CommonLogic.IIF(m_sortbylooks, 1, 0).ToString() + ",");
                sql.Append("QuantityDiscountID=" + m_quantitydiscountid.ToString());
                sql.Append(" where " + m_entitytype + "ID=" + m_id.ToString());

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
        /// Soft Deletes the entity (sets Deleted=1) in the database
        /// </summary>
        /// <returns>Boolean value indicating success</returns>
        public bool Delete()
        {
            bool success = true;

            try
            {
                m_deleted = true;
                DB.ExecuteSQL("update dbo.{0} set Deleted=1 where {0}ID=".FormatWith(EntityType) + m_id.ToString());
            }
            catch (Exception ex)
            {
                SysLog.LogException(ex, MessageTypeEnum.DatabaseException, MessageSeverityEnum.Error);
                success = false;
            }

            return success;
        }

        /// <summary>
        /// UnDeletes a soft deleted (set Deleted=0) the entity in the database
        /// </summary>
        /// <returns>Boolean value indicating success</returns>
        public bool UnDelete()
        {
            bool success = true;

            try
            {
                m_deleted = false;
                DB.ExecuteSQL("update dbo.{0} set Deleted=0 where {0}ID=".FormatWith(EntityType) + m_id.ToString());
            }
            catch (Exception ex)
            {
                SysLog.LogException(ex, MessageTypeEnum.DatabaseException, MessageSeverityEnum.Error);
                success = false;
            }

            return success;
        }

        /// <summary>
        /// Nukes the entity (removes the database record) from the database
        /// </summary>
        /// <returns>Boolean value indicating success</returns>
        public bool Nuke()
        {
            bool success = true;

            try
            {
                DB.ExecuteSQL("delete dbo.{0} where {0}ID=".FormatWith(EntityType) + m_id.ToString());
                ResetEntity();
            }
            catch (Exception ex)
            {
                SysLog.LogException(ex, MessageTypeEnum.DatabaseException, MessageSeverityEnum.Error);
                success = false;
            }

            return success;
        }

        public void ResetEntity()
        {
            m_entitytype = String.Empty;
            m_id = 0;
            m_guid = String.Empty;
            m_name = String.Empty;
            m_summary = String.Empty;
            m_description = String.Empty;
            m_sekeywords = String.Empty;
            m_sedescription = String.Empty;
            m_displayprefix = String.Empty;
            m_setitle = String.Empty;
            m_senoscript = String.Empty;
            m_sealttext = String.Empty;
            m_parentid = 0;
            m_colwidth = 0;
            m_sortbylooks = false;
            m_displayorder = 0;
            m_xmlpackage = String.Empty;
            m_published = false;
            m_wholesale = false;
            m_quantitydiscountid = 0;
            m_sename = String.Empty;
            m_extensiondata = String.Empty;
            m_imagefilenameoverride = String.Empty;
            m_isimport = false;
            m_deleted = false;
            m_createdon = DateTime.MinValue;
            m_pagesize = 0;
            m_taxclassid = 0;
            m_skinid = 0;
            m_templatename = String.Empty;
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Determines all root-level (have a ParentEntityID of 0) entities 
        /// </summary>
        /// <param name="EntityToLoad">The EntityType - Category|Manufacturer|Section|Distributor|Genre|Vector - to load</param>
        /// <returns>An Entity List containing all root-level entities</returns>
        public static List<Entity> GetRootLevelEntities(String EntityToLoad)
        {
            String EntityType = LoadEntityType(EntityToLoad);

            List<Entity> eiList = new List<Entity>();

            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                using (IDataReader rs = DB.GetRS("select * from dbo.{0} with(NOLOCK) where Parent{0}ID=0".FormatWith(EntityType), conn))
                {
                    while (rs.Read())
                    {
                        Entity e = new Entity(rs, EntityType);
                        eiList.Add(e);
                    }
                }
            }

            return eiList;
        }

        public static bool ExistsInEntityStore(int StoreID, int EntityID, string EntityType)   
        {
            bool Exists = false;
            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS("SELECT * from EntityStore WITH(NOLOCK) where StoreID='" + StoreID.ToString() + "' AND EntityID='" + EntityID.ToString() + "' AND EntityType='" + EntityType + "';", conn))
                {
                    if (rs.Read())
                    {
                        Exists = true;                     
                    }
                }
            }
            return Exists;
        }

        public static String LoadEntityType(String EntityToLoad)
        {
            String EntityType = "Category";

            switch (EntityToLoad.ToLowerInvariant())
            {
                case "manufacturer":
                    EntityType = "Manufacturer";
                    break;
                case "distributor":
                    EntityType = "Distributor";
                    break;
                case "section":
                case "department":
                    EntityType = "Section";
                    break;
                case "vector":
                    EntityType = "Vector";
                    break;
                case "genre":
                    EntityType = "Genre";
                    break;
                case "category":
                default:
                    EntityType = "Category";
                    break;

            }

            return EntityType;
        }

        public static List<Entity> GetChildrenEntities(String EntityToLoad, int ParentEntityID)
        {
            String EntityType = LoadEntityType(EntityToLoad);

            List<Entity> eiList = new List<Entity>();

            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                using (IDataReader rs = DB.GetRS("select * from dbo.{0} with(NOLOCK) where Parent{0}ID=".FormatWith(EntityType) + ParentEntityID.ToString(), conn))
                {
                    while (rs.Read())
                    {
                        Entity e = new Entity(rs, EntityType);
                        eiList.Add(e);      
                    }
                }
            }

            return eiList;
        }

        public static List<Entity> GetRecursivePath(int EntityID, String EntityToLoad)
        {
            Entity e = new Entity(EntityID, EntityToLoad);

            return GetRecursivePath(e);
        }

        public static List<Entity> GetRecursivePath(Entity e)
        {
            List<Entity> RecursiveList = new List<Entity>();

            if (e.ParentID > 0)
            {
                do
                {
                    RecursiveList.Add(e);
                    e = new Entity(e.ParentID, e.EntityType);
                } while (e.ParentID > 0); 
            }

            RecursiveList.Add(e);
            RecursiveList.Reverse();

            return RecursiveList;
        }

        #endregion
    }

    public class GridEntity
    {
        #region Private Variables

        private String m_entitytype;
        private int m_id;
        private String m_name;
        private int m_parentid;
        private int m_displayorder;
        private bool m_published;
        private bool m_deleted;


        #endregion

        #region Constructors

        public GridEntity()
        {
            m_entitytype = String.Empty;
            m_id = 0;
            m_name = String.Empty;
            m_parentid = 0;
            m_published = false;
            m_deleted = false;
            m_displayorder = 1;
        }

        public GridEntity(IDataReader rs, String EntityToLoad)
        {
            m_entitytype = EntityToLoad;
            m_id = DB.RSFieldInt(rs, "{0}ID".FormatWith(EntityToLoad));
            m_name = DB.RSField(rs, "Name");
            m_parentid = DB.RSFieldInt(rs, "Parent{0}ID".FormatWith(EntityToLoad));
            m_published = DB.RSFieldBool(rs, "Published");
            m_deleted = DB.RSFieldBool(rs, "Deleted");
            m_displayorder = DB.RSFieldInt(rs, "DisplayOrder");
        }

        public GridEntity(int EntityIDToLoad, String EntityTypeToLoad)
        {
            m_entitytype = LoadEntityType(EntityTypeToLoad);

            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS("select * from dbo.{0} with(NOLOCK) where {0}ID=".FormatWith(m_entitytype) + EntityIDToLoad.ToString(), conn))
                {
                    if (rs.Read())
                    {
                        m_id = DB.RSFieldInt(rs, "{0}ID".FormatWith(m_entitytype));
                        m_name = DB.RSField(rs, "Name");
                        m_parentid = DB.RSFieldInt(rs, "Parent{0}ID".FormatWith(m_entitytype));
                        m_published = DB.RSFieldBool(rs, "Published");
                        m_deleted = DB.RSFieldBool(rs, "Deleted");
                        m_displayorder = DB.RSFieldInt(rs, "DisplayOrder");
                    }

                    rs.Close();
                    rs.Dispose();
                }
                conn.Close();
                conn.Dispose();
            }
        }

        #endregion

        #region Public Properties

        public String EntityType { get { return LoadEntityType(m_entitytype); } set { m_entitytype = value; } }
        public int ID { get { return m_id; } set { m_id = value; } }
        public String Name { get { return m_name; } set { m_name = value; } }
        public String LocaleName { get { return LocalizedValue(m_name); } }
        public int ParentID { get { return m_parentid; } set { m_parentid = value; } }
        public int DisplayOrder { get { return m_displayorder; } set { m_displayorder = value; } }
        public bool Published { get { return m_published; } set { m_published = value; } }
        public bool Deleted { get { return m_deleted; } set { m_deleted = value; } }
        public bool HasChildren { get { if (m_id.Equals(0)) { return false; } return DB.GetSqlN("select count(*) as N from dbo.{0} with(NOLOCK) where Parent{0}ID=".FormatWith(EntityType) + m_id.ToString()) > 0; } }
        public int NumChildren { get { return DB.GetSqlN("select count(*) as N from dbo.{0} with(NOLOCK) where Parent{0}ID=".FormatWith(EntityType) + m_id.ToString()); } }

        #endregion

        #region Public Methods

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
        /// Updates the entity as published
        /// </summary>
        /// <returns>Boolean value indicating success</returns>
        public bool Publish()
        {
            bool success = true;

            try
            {
                m_published = true;
                DB.ExecuteSQL("update dbo.{0} set Published=1 where {0}ID=".FormatWith(EntityType) + m_id.ToString());
            }
            catch (Exception ex)
            {
                SysLog.LogException(ex, MessageTypeEnum.DatabaseException, MessageSeverityEnum.Error);
                success = false;
            }

            return success;
        }

        /// <summary>
        /// Updates the entity as unpublished
        /// </summary>
        /// <returns>Boolean value indicating success</returns>
        public bool UnPublish()
        {
            bool success = true;

            try
            {
                m_published = false;
                DB.ExecuteSQL("update dbo.{0} set Published=0 where {0}ID=".FormatWith(EntityType) + m_id.ToString());
            }
            catch (Exception ex)
            {
                SysLog.LogException(ex, MessageTypeEnum.DatabaseException, MessageSeverityEnum.Error);
                success = false;
            }

            return success;
        }

        /// <summary>
        /// Soft Deletes the entity (sets Deleted=1) in the database
        /// </summary>
        /// <returns>Boolean value indicating success</returns>
        public bool Delete()
        {
            bool success = true;

            try
            {
                m_deleted = true;
                DB.ExecuteSQL("update dbo.{0} set Deleted=1 where {0}ID=".FormatWith(EntityType) + m_id.ToString());
            }
            catch (Exception ex)
            {
                SysLog.LogException(ex, MessageTypeEnum.DatabaseException, MessageSeverityEnum.Error);
                success = false;
            }

            return success;
        }

        /// <summary>
        /// UnDeletes a soft deleted (set Deleted=0) the entity in the database
        /// </summary>
        /// <returns>Boolean value indicating success</returns>
        public bool UnDelete()
        {
            bool success = true;

            try
            {
                m_deleted = false;
                DB.ExecuteSQL("update dbo.{0} set Deleted=0 where {0}ID=".FormatWith(EntityType) + m_id.ToString());
            }
            catch (Exception ex)
            {
                SysLog.LogException(ex, MessageTypeEnum.DatabaseException, MessageSeverityEnum.Error);
                success = false;
            }

            return success;
        }


        /// <summary>
        /// Commits the current properties on the GridEntityObject to the database
        /// </summary>
        public void Commit()
        {
            StringBuilder sql = new StringBuilder();

            sql.Append("update dbo.{0}                                          \r\n");
            sql.Append("set published = " + (m_published ? "1" : "0") + ",      \r\n");
            sql.Append("    name = " + m_name + ",                              \r\n");
            sql.Append("    parentid = " + m_parentid.ToString() + ",           \r\n");
            sql.Append("    deleted = " + (m_deleted ? "1" : "0") + ",          \r\n");
            sql.Append("    displayorder = " + m_displayorder.ToString() + "    \r\n");
            sql.Append("where {0}ID = " + m_id.ToString());

            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                DB.ExecuteSQL(sql.ToString().FormatWith(m_entitytype), conn);

                conn.Close();
                conn.Dispose();
            }
        }

        #endregion

        #region Static Methods

        public static String LoadEntityType(String EntityToLoad)
        {
            String EntityType = "Category";

            switch (EntityToLoad.ToLowerInvariant())
            {
                case "manufacturer":
                    EntityType = "Manufacturer";
                    break;
                case "distributor":
                    EntityType = "Distributor";
                    break;
                case "section":
                case "department":
                    EntityType = "Section";
                    break;
                case "vector":
                    EntityType = "Vector";
                    break;
                case "genre":
                    EntityType = "Genre";
                    break;
                case "category":
                default:
                    EntityType = "Category";
                    break;

            }

            return EntityType;
        }

        /// <summary>
        /// Determines all root-level (have a ParentEntityID of 0) entities 
        /// </summary>
        /// <param name="EntityToLoad">The EntityType - Category|Manufacturer|Section|Distributor|Genre|Vector - to load</param>
        /// <returns>An Entity List containing all root-level entities</returns>
        public static List<GridEntity> GetRootLevelEntities(String EntityToLoad)
        {
            String EntityType = LoadEntityType(EntityToLoad);

            List<GridEntity> eiList = new List<GridEntity>();

            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                using (IDataReader rs = DB.GetRS("select * from dbo.{0} with(NOLOCK) where Parent{0}ID=0".FormatWith(EntityType), conn))
                {
                    while (rs.Read())
                    {
                        GridEntity e = new GridEntity(rs, EntityType);
                        eiList.Add(e);
                    }
                }
            }

            return eiList;
        }

        /// <summary>
        /// Loads all entities of a given entity type 
        /// </summary>
        /// <param name="EntityToLoad">The EntityType - Category|Manufacturer|Section|Distributor - to load</param>
        /// <returns>An Entity List containing all root-level entities</returns>
        public static List<GridEntity> GetAllEntitiesOfType(String EntityToLoad)
        {
            String EntityType = LoadEntityType(EntityToLoad);

            List<GridEntity> eiList = new List<GridEntity>();

            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                using (IDataReader rs = DB.GetRS("select * from dbo.{0} with(NOLOCK) where Deleted=0".FormatWith(EntityType), conn))
                {
                    while (rs.Read())
                    {
                        GridEntity e = new GridEntity(rs, EntityType);
                        eiList.Add(e);
                    }
                }
            }

            return eiList;
        }

        /// <summary>
        /// Determines the children entities belonging to an entity
        /// </summary>
        /// <param name="EntityToLoad">The EntityType - Category|Manufacturer|Section|Distribotur|Genre|Vector - to find children of</param>
        /// <param name="ParentEntityID">The ID of the entity to retrieve children for</param>
        /// <returns>A GridEntity List containing the immediate children of an entity</returns>
        public static List<GridEntity> GetChildrenEntities(String EntityToLoad, int ParentEntityID)
        {
            String EntityType = LoadEntityType(EntityToLoad);

            List<GridEntity> eiList = new List<GridEntity>();

            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                using (IDataReader rs = DB.GetRS("select * from dbo.{0} with(NOLOCK) where Parent{0}ID=".FormatWith(EntityType) + ParentEntityID.ToString(), conn))
                {
                    while (rs.Read())
                    {
                        GridEntity e = new GridEntity(rs, EntityType);
                        eiList.Add(e);
                    }
                }
            }

            return eiList;
        }

        #endregion

    }
}
