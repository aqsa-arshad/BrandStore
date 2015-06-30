// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Xsl;
using System.Web;
using System.Web.Caching;
using System.Globalization;
using System.Threading;
using System.Collections.Generic;

namespace AspDotNetStorefrontCore
{
    public class EntitySpecs
    {
        public String m_EntityXsl;
        public String m_EntityName;
        public String m_EntityNamePlural; // because english grammar is unpredictable!
        public bool m_HasParentChildRelationship;
        public bool m_HasDisplayOrder;
        public bool m_HasAddress;
        public String m_ObjectName;
        public String m_ObjectNamePlural; // because english grammar is unpredictable!
        public bool m_EntityObjectMappingIs1to1; // specify true for 1:1 Object:Entity mappings (where the Object table is assumed to have an EntityID column therefore)
        public bool m_HasIconPic;
        public bool m_HasMediumPic;
        public bool m_HasLargePic;

        public EntitySpecs(String XslName, String EntityName, String EntityNamePlural, bool HasParentChildRelationship, bool HasDisplayOrder, bool HasAddress, String ObjectName, String ObjectNamePlural, bool EntityObjectMappingIs1to1, bool HasIconPic, bool HasMediumPic, bool HasLargePic)
        {
            m_EntityXsl = XslName;
            m_EntityName = EntityName;
            m_EntityNamePlural = EntityNamePlural;
            m_HasParentChildRelationship = HasParentChildRelationship;
            m_HasDisplayOrder = HasDisplayOrder;
            m_HasAddress = HasAddress;
            m_ObjectName = ObjectName;
            m_ObjectNamePlural = ObjectNamePlural;
            m_EntityObjectMappingIs1to1 = EntityObjectMappingIs1to1;
            m_HasIconPic = HasIconPic;
            m_HasMediumPic = HasMediumPic;
            m_HasLargePic = HasLargePic;
        }
    }

    public class EntityDefinitions
    {

        static public readonly EntitySpecs readonly_CategoryEntitySpecs = new EntitySpecs("EntityMgr", "Category", "Categories", true, true, false, "Product", "Products", false, true, true, true);
        static public readonly EntitySpecs readonly_SectionEntitySpecs = new EntitySpecs("EntityMgr", "Section", "Sections", true, true, false, "Product", "Products", false, true, true, true);
        static public readonly EntitySpecs readonly_ManufacturerEntitySpecs = new EntitySpecs("EntityMgr", "Manufacturer", "Manufacturers", true, true, true, "Product", "Products", true, true, true, true);
        static public readonly EntitySpecs readonly_DistributorEntitySpecs = new EntitySpecs("EntityMgr", "Distributor", "Distributors", true, true, true, "Product", "Products", true, true, true, true);
        static public readonly EntitySpecs readonly_GenreEntitySpecs = new EntitySpecs("EntityMgr", "Genre", "Genres", true, true, false, "Product", "Products", false, true, true, true);
        static public readonly EntitySpecs readonly_VectorEntitySpecs = new EntitySpecs("EntityMgr", "Vector", "Vectors", true, true, false, "Product", "Products", false, true, true, true);
        static public readonly EntitySpecs readonly_AffiliateEntitySpecs = new EntitySpecs("EntityMgr", "Affiliate", "Affiliates", true, true, true, "Product", "Products", false, false, false, false);
        static public readonly EntitySpecs readonly_CustomerLevelEntitySpecs = new EntitySpecs("EntityMgr", "CustomerLevel", "CustomerLevels", false, true, false, "Product", "Products", false, false, false, false);
        static public readonly EntitySpecs readonly_LibraryEntitySpecs = new EntitySpecs("EntityMgr", "Library", "Libraries", true, true, false, "Document", "Documents", false, true, true, true);

        static public EntitySpecs LookupSpecs(String EntityName)
        {
            switch (EntityName.ToUpperInvariant())
            {
                case "CATEGORY":
                    return readonly_CategoryEntitySpecs;
                case "SECTION":
                case "DEPARTMENT":
                    return readonly_SectionEntitySpecs;
                case "MANUFACTURER":
                    return readonly_ManufacturerEntitySpecs;
                case "DISTRIBUTOR":
                    return readonly_DistributorEntitySpecs;
                case "GENRE":
                    return readonly_GenreEntitySpecs;
                case "VECTOR":
                    return readonly_VectorEntitySpecs;
                case "AFFILIATE":
                    return readonly_AffiliateEntitySpecs;
                case "CUSTOMERLEVEL":
                    return readonly_CustomerLevelEntitySpecs;
                case "LIBRARY":
                    return readonly_LibraryEntitySpecs;
            }
            return null;
        }
    }

    /// <summary>
    /// Summary description for  EntityHelper is a common set of routines that support
    /// a multi level table Parent-child table structure. Note that NOT ALL ROUTINES are semantically valid with all entity types!
    /// </summary>
    public class EntityHelper
    {
        private EntitySpecs m_EntitySpecs;
        private String m_TableName;
        private String m_NodeName;
        private String m_IDColumnName;
        private String m_CacheName;
        private int m_CacheMinutes;
        private String m_XmlPackageName;
        private bool m_OnlyPublishedEntitiesAndObjects;
        private int m_StoreID;

        // this is public, use with CARE! Make sure you know how this object works before using it!
        public HierarchicalTableMgr m_TblMgr;

        public EntityHelper(EntitySpecs eSpecs, int StoreID) : this(AppLogic.CacheDurationMinutes(), eSpecs, !AppLogic.IsAdminSite, StoreID) { }
        public EntityHelper(int CacheMinutes, EntitySpecs eSpecs, int StoreID) : this(CacheMinutes, eSpecs, !AppLogic.IsAdminSite, StoreID) { }

        public EntityHelper(int CacheMinutes, EntitySpecs eSpecs, bool PublishedOnly, int StoreID)
        {
            m_EntitySpecs = eSpecs;
            m_TableName = m_EntitySpecs.m_EntityName;
            m_NodeName = m_EntitySpecs.m_EntityName;
            m_IDColumnName = m_EntitySpecs.m_EntityName + "ID";
            m_CacheName = m_EntitySpecs.m_EntityName + "Mgr";
            m_XmlPackageName = m_EntitySpecs.m_EntityName + "Mgr";
            m_StoreID = StoreID;
            if (AppLogic.CachingOn)
            {
                m_CacheMinutes = CacheMinutes;
            }
            else
            {
                m_CacheMinutes = 0;
            }
            m_OnlyPublishedEntitiesAndObjects = PublishedOnly;
           
            m_TblMgr = new HierarchicalTableMgr(m_EntitySpecs.m_EntityName, "Entity", "EntityID", "EntityGUID", "Name", m_EntitySpecs.m_EntityXsl, m_CacheMinutes, 0, m_OnlyPublishedEntitiesAndObjects, m_StoreID);
        }

        public EntitySpecs GetEntitySpecs
        {
            get
            {
                return m_EntitySpecs;
            }
        }

        /// <summary>
        /// Determines which products belong to an entity or group of entities
        /// </summary>
        /// <param name="ForEntityIDList">Comma separated string of entity ids to find products for</param>
        /// <param name="AffiliateID">Affiliate ID (int) of the current customer</param>
        /// <param name="CustomerLevelID">CustomerLevel ID (int) of the current customer</param>
        /// <returns></returns>
        public List<int> GetProductList(String ForEntityIDList, int AffiliateID, int CustomerLevelID)
        {
            if (ForEntityIDList.Length == 0)
            {
                return new List<int>();
            }

            String sql = "select ProductID from dbo.ProductEntity  with (NOLOCK)  where EntityType = " + DB.SQuote(m_EntitySpecs.m_EntityName) + " and EntityID in (" + ForEntityIDList + ")";
            List<int> pList = new List<int>();

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS(sql, dbconn))
                {
                    while (rs.Read())
                    {
                        pList.Add(DB.RSFieldInt(rs, "ProductID"));
                    }
                }
            }
            return pList;
        }

        // returns comma separate list of the specified entity and all sub-entities (e.g. 1,2,35,17,34) (e.g. a list of all entity id's in this tree, in no particular order
        public String GetProductCommaSeparatedList(String ForEntityIDList, int AffiliateID, int CustomerLevelID)
        {
            if (ForEntityIDList.Length == 0)
            {
                return String.Empty;
            }
            String sql = "select ProductID from dbo.ProductEntity  with (NOLOCK)  where EntityType = " + DB.SQuote(m_EntitySpecs.m_EntityName) + " and EntityID in (" + ForEntityIDList + ")";
            StringBuilder tmpS = new StringBuilder(1024);
            String Seperator = String.Empty;

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS(sql, dbconn))
                {
                    while (rs.Read())
                    {
                        tmpS.Append(Seperator);
                        tmpS.Append(DB.RSFieldInt(rs, "ProductID").ToString());
                        Seperator = ",";
                    }
                }
            }
            return tmpS.ToString();
        }


        // returns comma separate list of the specified entity and all sub-entities (e.g. 1,2,35,17,34) (e.g. a list of all entity id's in this tree, in no particular order
        public String GetEntityCommaSeparatedList(String ForEntityIDList, int AffiliateID, int CustomerLevelID)
        {
            String FullEntityList = String.Empty;
            String Seperator = String.Empty;
            String CN = Regex.Replace(ForEntityIDList, "\\s+", "", RegexOptions.Compiled);
            if (CN.Length != 0)
            {
                foreach (String s in CN.Split(','))
                {
                    FullEntityList += Seperator;
                    FullEntityList += GetEntityCommaSeparatedList(System.Int32.Parse(s), AffiliateID, CustomerLevelID);
                    Seperator = ",";
                }
            }
            return FullEntityList;
        }

        // returns comma separate list of the specified entity and all sub-entities (e.g. 1,2,35,17,34) (e.g. a list of all entity id's in this tree, in no particular order
        public String GetEntityCommaSeparatedList(int ForParentEntityID, int AffiliateID, int CustomerLevelID)
        {
            StringBuilder tmpS = new StringBuilder(1024);

            tmpS.Append(ForParentEntityID.ToString());
            XmlNode n;
            if (ForParentEntityID == 0)
            {
                n = m_TblMgr.ResetToRootNode();
            }
            else
            {
                n = m_TblMgr.SetContext(ForParentEntityID);
            }

            if (n != null && m_TblMgr.HasChildren(n))
            {
                n = m_TblMgr.MoveFirstChild(n);
                while (n != null)
                {
                    int ThisID = m_TblMgr.CurrentID(n);
                    if (tmpS.Length != 0)
                    {
                        tmpS.Append(",");
                    }
                    tmpS.Append(ThisID.ToString());
                    if (m_TblMgr.HasChildren(n))
                    {
                        tmpS.Append(GetEntityCommaSeparatedList(ThisID, AffiliateID, CustomerLevelID));
                    }
                    n = m_TblMgr.MoveNextSibling(n, false);
                }
            }
            return tmpS.ToString();
        }

        /// <summary>
        /// Gets a integer list of the specified entity and all sub-entities, in no particular order
        /// </summary>
        /// <param name="ForEntityIDList">ID of entity to retrieve sub-entities for</param>
        /// <param name="AffiliateID">Affiliate ID of the current customer</param>
        /// <param name="CustomerLevelID">CustomerLevel ID of the current customer</param>
        /// <returns>An integer list containing</returns>
        public List<int> GetEntityList(String ForEntityIDList, int AffiliateID, int CustomerLevelID)
        {
            List<int> FullEntityList = new List<int>();
            List<int> SubList = new List<int>();

            String CN = Regex.Replace(ForEntityIDList, "\\s+", "", RegexOptions.Compiled);
            if (CN.Length != 0)
            {
                foreach (String s in CN.Split(','))
                {
                    FullEntityList.Add(System.Int32.Parse(s));
                   
                    SubList = GetEntityList(System.Int32.Parse(s), AffiliateID, CustomerLevelID);

                    foreach (int sEntityID in SubList)
                    {
                        FullEntityList.Add(sEntityID);
                    }
                }
            }
            return FullEntityList;
        }

        // returns list of the specified entity and all sub-entities (e.g. 1,2,35,17,34) (e.g. a list of all entity id's in this tree, in no particular order
        public List<int> GetEntityList(int ForParentEntityID, int AffiliateID, int CustomerLevelID)
        {
            return GetEntityList(ForParentEntityID, AffiliateID, CustomerLevelID, new List<int>());
        }

        // returns list of the specified entity and all sub-entities (e.g. 1,2,35,17,34) (e.g. a list of all entity id's in this tree, in no particular order
        public List<int> GetEntityList(int ForParentEntityID, int AffiliateID, int CustomerLevelID, List<int> pList)
        {
            List<int> entityIDList = new List<int>();

            if (pList.Count > 0)
            {
                entityIDList = pList;
            }

            entityIDList.Add(ForParentEntityID);
            
            XmlNode n;
            if (ForParentEntityID == 0)
            {
                n = m_TblMgr.ResetToRootNode();
            }
            else
            {
                n = m_TblMgr.SetContext(ForParentEntityID);
            }

            if (n != null && m_TblMgr.HasChildren(n))
            {
                n = m_TblMgr.MoveFirstChild(n);
                while (n != null)
                {
                    int ThisID = m_TblMgr.CurrentID(n);
                    
                    entityIDList.Add(ThisID);
                    
                    if (m_TblMgr.HasChildren(n))
                    {
                        List<int> tmpList = GetEntityList(ThisID, AffiliateID, CustomerLevelID, entityIDList);
                    }
                    n = m_TblMgr.MoveNextSibling(n, false);
                }
            }
            return entityIDList;
        }

        public String GetEntityYahooSiteMap(int ForParentEntityID, String LocaleSetting, bool AllowCaching, bool RecurseChildren)
        {
            String CacheName = String.Format("GetEntityYahooSiteMap_{0}_{1}_{2}_{3}", m_EntitySpecs.m_EntityName, ForParentEntityID.ToString(), LocaleSetting, RecurseChildren.ToString());
            if (AppLogic.CachingOn)
            {
                String Menu = (String)HttpContext.Current.Cache.Get(CacheName);
                if (Menu != null)
                {
                    return Menu;
                }
            }
            String StoreLoc = AppLogic.GetStoreHTTPLocation(false);

            StringWriter tmpS = new StringWriter();
            String XslFile = "EntityYahooSiteMap";
            XslCompiledTransform xForm;
            string XslFilePath = CommonLogic.SafeMapPath("EntityHelper/" + XslFile + ".xslt");
            xForm = (XslCompiledTransform)HttpContext.Current.Cache.Get(XslFilePath);
            if (xForm == null)
            {
                xForm = new XslCompiledTransform(false);
                xForm.Load(XslFilePath);
                HttpContext.Current.Cache.Insert(XslFilePath, xForm, new CacheDependency(XslFilePath));
            }
            XsltArgumentList xslArgs = new XsltArgumentList();
            xslArgs.AddParam("entity", "", m_EntitySpecs.m_EntityName);
            xslArgs.AddParam("ForParentEntityID", "", ForParentEntityID);
            xslArgs.AddParam("StoreLoc", "", StoreLoc);
            xForm.Transform(m_TblMgr.XmlDoc, xslArgs, tmpS);
            if (AppLogic.AppConfigBool("XmlPackage.DumpTransform"))
            {
                try // don't let logging crash the site
                {
                    StreamWriter sw = File.CreateText(CommonLogic.SafeMapPath(String.Format("{0}images/{1}_{2}_{3}.xfrm.xml", CommonLogic.IIF(AppLogic.IsAdminSite, "../", ""), XslFile, m_EntitySpecs.m_EntityName, CommonLogic.IIF(AppLogic.IsAdminSite, "admin", "store"))));
                    sw.WriteLine(XmlCommon.PrettyPrintXml(tmpS.ToString()));
                    sw.Close();
                }
                catch { }
            }

            if (AppLogic.CachingOn)
            {
                HttpContext.Current.Cache.Insert(CacheName, tmpS.ToString(), null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }
            return tmpS.ToString();
        }

        public String GetEntityYahooObjectList(int EntityID, String LocaleSetting, int AffiliateID, int CustomerLevelID)
        {
            String CacheName = String.Format("GetEntityYahooObjectList_{0}_{1}_{2}_{3}", EntityID.ToString(), LocaleSetting, AffiliateID.ToString(), CustomerLevelID.ToString());
            if (AppLogic.CachingOn)
            {
                String s = (String)HttpContext.Current.Cache.Get(CacheName);
                if (s != null)
                {
                    return s;
                }
            }

            String StoreLoc = AppLogic.GetStoreHTTPLocation(false);
            StringBuilder tmpS = new StringBuilder(1000);
            String sql = GetListProductsSQL(EntityID, AffiliateID, CustomerLevelID, true, true, false);

            String XslFile = "EntityYahooObjectList";
            XslCompiledTransform xForm;
            string XslFilePath = CommonLogic.SafeMapPath("EntityHelper/" + XslFile + ".xslt");
            xForm = (XslCompiledTransform)HttpContext.Current.Cache.Get(XslFilePath);
            if (xForm == null)
            {
                xForm = new XslCompiledTransform(false);
                xForm.Load(XslFilePath);
                HttpContext.Current.Cache.Insert(XslFilePath, xForm, new CacheDependency(XslFilePath));
            }
            XsltArgumentList xslArgs = new XsltArgumentList();
            xslArgs.AddParam("entityPrefix", "", m_EntitySpecs.m_EntityName.Substring(0, 1).ToLowerInvariant());
            xslArgs.AddParam("objectPrefix", "", m_EntitySpecs.m_ObjectName.Substring(0, 1).ToLowerInvariant());
            xslArgs.AddParam("storeBaseUrl", "", StoreLoc);
            xslArgs.AddParam("EntityID", "", EntityID.ToString());

            String objXml = String.Empty;
            int rows = DB.GetXml(sql, "root", "object", ref objXml);
            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml(objXml);
            MemoryStream ms = new MemoryStream();

            xForm.Transform(xdoc, xslArgs, ms);
            ms.Position = 0;
            StreamReader sr = new StreamReader(ms);
            string m_FinalResult = sr.ReadToEnd();
            ms.Close();
            sr.Dispose();



            if (AppLogic.CachingOn)
            {
                HttpContext.Current.Cache.Insert(CacheName, m_FinalResult, null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }
            return m_FinalResult;
        }

        public String GetEntityBreadcrumb6(int EntityID, String LocaleSetting)
        {
            String CacheName = String.Format("GetEntityBreadcrumb6_{0}_{1}_{2}_{3}", m_EntitySpecs.m_EntityName, EntityID.ToString(), LocaleSetting, AppLogic.IsAdminSite.ToString());
            if (AppLogic.CachingOn)
            {
                String Menu = (String)HttpContext.Current.Cache.Get(CacheName);
                if (Menu != null)
                {
                    if (CommonLogic.ApplicationBool("DumpSQL"))
                    {
                        HttpContext.Current.Response.Write("Cache Hit Found!\n");
                    }
                    return Menu;
                }
            }
            String tmpS = String.Empty;
            String URL = String.Empty;
            XmlNode n = m_TblMgr.SetContext(EntityID);
            if (EntityID != 0 && n != null)
            {
                n = m_TblMgr.MoveParent(n);
                while (n != null)
                {
                    if (AppLogic.IsAdminSite)
                    {
                        URL = String.Format("entityEdit.aspx?EntityName={0}&EntityID={1}", m_EntitySpecs.m_EntityName, m_TblMgr.CurrentID(n).ToString());
                    }
                    else
                    {
                        URL = SE.MakeEntityLink(m_EntitySpecs.m_EntityName, m_TblMgr.CurrentID(n), m_TblMgr.CurrentField(n, "SEName"));
                    }
                    tmpS = String.Format("<a onclick=\"javascript:top.frames['entityMenu'].location.href = 'entityMenu.aspx?entityName=" + m_EntitySpecs.m_EntityName + "&entityID=" + m_TblMgr.CurrentID(n).ToString() + "';\" href=\"{0}\">{1}</a> &rarr; ", URL, m_TblMgr.CurrentName(n, LocaleSetting)) + tmpS;
                    n = m_TblMgr.MoveParent(n);
                }
                n = m_TblMgr.SetContext(EntityID);
                if (AppLogic.IsAdminSite)
                {
                    URL = String.Format("entityEdit.aspx?EntityName={0}&EntityID={1}", m_EntitySpecs.m_EntityName, m_TblMgr.CurrentID(n).ToString());
                }
                else
                {
                    URL = SE.MakeEntityLink(m_EntitySpecs.m_EntityName, m_TblMgr.CurrentID(n), m_TblMgr.CurrentField(n, "SEName"));
                }
                tmpS += String.Format("<a href=\"{0}\">{1} ({2})</a>", URL, m_TblMgr.CurrentName(n, LocaleSetting), m_TblMgr.CurrentID(n));
            }

            if (AppLogic.CachingOn)
            {
                HttpContext.Current.Cache.Insert(CacheName, tmpS, null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }
            return tmpS;
        }

        public String GetEntityBreadcrumb(int EntityID, String LocaleSetting)
        {
            String CacheName = String.Format("GetEntityBreadcrumb_{0}_{1}_{2}_{3}", m_EntitySpecs.m_EntityName, EntityID.ToString(), LocaleSetting, AppLogic.IsAdminSite.ToString());
            if (AppLogic.CachingOn)
            {
                String Menu = (String)HttpContext.Current.Cache.Get(CacheName);
                if (Menu != null)
                {
                    if (CommonLogic.ApplicationBool("DumpSQL"))
                    {
                        HttpContext.Current.Response.Write("Cache Hit Found!\n");
                    }
                    return Menu;
                }
            }
            String tmpS = String.Empty;
            String URL = String.Empty;
            XmlNode n = m_TblMgr.SetContext(EntityID);
            if (EntityID != 0 && n != null)
            {
                n = m_TblMgr.MoveParent(n);
                while (n != null)
                {
                    if (AppLogic.IsAdminSite)
                    {
                        URL = String.Format("newentities.aspx?entityname={0}&{1}ID={2}", m_EntitySpecs.m_EntityName, m_EntitySpecs.m_EntityName, m_TblMgr.CurrentID(n).ToString());
                    }
                    else
                    {
                        URL = SE.MakeEntityLink(m_EntitySpecs.m_EntityName, m_TblMgr.CurrentID(n), m_TblMgr.CurrentField(n, "SEName"));
                    }
                    tmpS = String.Format("<a href=\"{0}\">{1}</a> &rarr; ", URL, m_TblMgr.CurrentName(n, LocaleSetting)) + tmpS;
                    n = m_TblMgr.MoveParent(n);
                }
                n = m_TblMgr.SetContext(EntityID);
                if (AppLogic.IsAdminSite)
                {
                    URL = String.Format("newentities.aspx?entityname={0}&{1}id={2}", m_EntitySpecs.m_EntityName, m_EntitySpecs.m_EntityName, m_TblMgr.CurrentID(n).ToString());
                }
                else
                {
                    URL = SE.MakeEntityLink(m_EntitySpecs.m_EntityName, m_TblMgr.CurrentID(n), m_TblMgr.CurrentField(n, "SEName"));
                }
                tmpS += String.Format("<a href=\"{0}\">{1}</a>", URL, m_TblMgr.CurrentName(n, LocaleSetting));
            }

            if (AppLogic.CachingOn)
            {
                HttpContext.Current.Cache.Insert(CacheName, tmpS, null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }
            return tmpS;
        }

        // returns list like this:
        // <option value="1" >Category 1</option>
        // <option value="13">Category 1 -> Category 1-1</option>
        // <option value="14">Category 1 -> Category 1-2</option>
        // <option value="27">Category 1 -> Category 1-2 -> Category 1-2-1</option> 
        // <option value="28">Category 1 -> Category 1-2 -> Category 1-2-2</option> 
        // <option value="15">Category 1 -> Category 1-3</option>
        // <option value="2" >Category 2</option>
        // <option value="23">Category 2 -> Category 2-1</option>
        // <option value="24">Category 2 -> Category 2-2</option>
        // <option value="25">Category 2 -> Category 2-3</option>
        // etc...
        public String GetEntitySelectList(int ForParentEntityID, String Prefix, int FilterEntityID, String LocaleSetting, bool AllowCaching)
        {
            String CacheName = String.Format("GetEntitySelectList_{0}_{1}_{2}_{3}_{4}_{5}", m_EntitySpecs.m_EntityName, ForParentEntityID.ToString(), Prefix, FilterEntityID.ToString(), LocaleSetting, AppLogic.IsAdminSite.ToString());
            if (AppLogic.CachingOn)
            {
                String Menu = (String)HttpContext.Current.Cache.Get(CacheName);
                if (Menu != null)
                {
                    if (CommonLogic.ApplicationBool("DumpSQL"))
                    {
                        HttpContext.Current.Response.Write("Cache Hit Found!\n");
                    }
                    return Menu;
                }
            }

            StringWriter tmpS = new StringWriter();
            String XslFile = "EntitySelectList";
            XslCompiledTransform xForm;
            string XslFilePath = CommonLogic.SafeMapPath("EntityHelper/" + XslFile + ".xslt");
            xForm = (XslCompiledTransform)HttpContext.Current.Cache.Get(XslFilePath);
            if (xForm == null)
            {
                xForm = new XslCompiledTransform(false);
                xForm.Load(XslFilePath);
                HttpContext.Current.Cache.Insert(XslFilePath, xForm, new CacheDependency(XslFilePath));
            }
            XsltArgumentList xslArgs = new XsltArgumentList();
            xslArgs.AddParam("ForParentEntityID", "", ForParentEntityID);
            xslArgs.AddParam("filterID", "", FilterEntityID);
            xslArgs.AddParam("custlocale", "", LocaleSetting);
            xslArgs.AddParam("deflocale", "", Localization.GetDefaultLocale());
            xslArgs.AddParam("adminsite", "", AppLogic.IsAdminSite);
            xForm.Transform(m_TblMgr.XmlDoc, xslArgs, tmpS);
            if (AppLogic.AppConfigBool("XmlPackage.DumpTransform"))
            {
                try // don't let logging crash the site
                {
                    StreamWriter sw = File.CreateText(CommonLogic.SafeMapPath(String.Format("{0}images/{1}_{2}_{3}.xfrm.xml", CommonLogic.IIF(AppLogic.IsAdminSite, "../", ""), XslFile, m_EntitySpecs.m_EntityName, CommonLogic.IIF(AppLogic.IsAdminSite, "admin", "store"))));
                    sw.WriteLine(XmlCommon.PrettyPrintXml(tmpS.ToString()));
                    sw.Close();
                }
                catch { }
            }

            if (AppLogic.CachingOn)
            {
                HttpContext.Current.Cache.Insert(CacheName, tmpS.ToString(), null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }
            return tmpS.ToString();
        }


        public ArrayList GetEntityArrayList(int ForParentEntityID, String Prefix, int FilterEntityID, String LocaleSetting, bool AllowCaching)
        {
            ArrayList al;
            String CacheName = String.Format("GetEntityArrayList_{0}_{1}_{2}_{3}_{4}_{5}", m_EntitySpecs.m_EntityName, ForParentEntityID.ToString(), Prefix, FilterEntityID.ToString(), LocaleSetting, AppLogic.IsAdminSite.ToString());
            if (AppLogic.CachingOn)
            {
                al = (ArrayList)HttpContext.Current.Cache.Get(CacheName);
                if (al != null)
                {
                    if (CommonLogic.ApplicationBool("DumpSQL"))
                    {
                        HttpContext.Current.Response.Write("Cache Hit Found!\n");
                    }
                    return al;
                }
            }

            al = new ArrayList();

            StringWriter tmpS = new StringWriter();
            String XslFile = "EntityArrayListXML";
            XslCompiledTransform xForm;
            string XslFilePath = CommonLogic.SafeMapPath("EntityHelper/" + XslFile + ".xslt");
            xForm = (XslCompiledTransform)HttpContext.Current.Cache.Get(XslFilePath);
            if (xForm == null)
            {
                xForm = new XslCompiledTransform(false);
                xForm.Load(XslFilePath);
                HttpContext.Current.Cache.Insert(XslFilePath, xForm, new CacheDependency(XslFilePath));
            }
            XsltArgumentList xslArgs = new XsltArgumentList();
            xslArgs.AddParam("ForParentEntityID", "", ForParentEntityID);
            xslArgs.AddParam("filterID", "", FilterEntityID);
            xslArgs.AddParam("custlocale", "", LocaleSetting);
            xslArgs.AddParam("deflocale", "", Localization.GetDefaultLocale());
            xslArgs.AddParam("adminsite", "", AppLogic.IsAdminSite);
            xForm.Transform(m_TblMgr.XmlDoc, xslArgs, tmpS);

            XmlDocument returnedXML = new XmlDocument();
            returnedXML.LoadXml(tmpS.ToString());

            XmlNodeList entityNodes = returnedXML.SelectNodes("/Entities/Entity");

            foreach (XmlNode n in entityNodes)
            {
                try
                {
                    XmlNode idNode = n.SelectNodes("EntityId")[0];
                    XmlNode nameNode = n.SelectNodes("EntityName")[0];
                    int entityId;
                    if (int.TryParse(idNode.InnerText, out entityId) && !string.IsNullOrEmpty(nameNode.InnerText))
                    {
                        ListItemClass li = new ListItemClass();
                        li.Value = entityId;
                        li.Item = Security.HtmlEncode(nameNode.InnerText);
                        al.Add(li);
                    }
                }
                catch (Exception)
                {
                }
            }


            //char s1 = '~';
            //char s2 = '|';
            //string[] strA = tmpS.ToString().Split(s1);
            //foreach (string str in strA)
            //{
            //    string[] temp = str.Split(s2);
            //    if (temp.Length > 1)
            //    {
            //        ListItemClass li = new ListItemClass();
            //        li.Value = Localization.ParseNativeInt(temp[0].ToString());
            //        li.Item = temp[1].ToString();
            //        al.Add(li);
            //    }
            //}

            if (AppLogic.AppConfigBool("XmlPackage.DumpTransform"))
            {
                try // don't let logging crash the site
                {
                    StreamWriter sw = File.CreateText(CommonLogic.SafeMapPath(String.Format("{0}images/{1}_{2}_{3}.xfrm.xml", CommonLogic.IIF(AppLogic.IsAdminSite, "../", ""), XslFile, m_EntitySpecs.m_EntityName, CommonLogic.IIF(AppLogic.IsAdminSite, "admin", "store"))));
                    sw.WriteLine(XmlCommon.PrettyPrintXml(tmpS.ToString()));
                    sw.Close();
                }
                catch { }
            }

            if (AppLogic.CachingOn)
            {
                HttpContext.Current.Cache.Insert(CacheName, al, null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }
            return al;
        }

        public String GetEntityULList(int ForParentEntityID, String LocaleSetting, int AffiliateID, int CustomerLevelID, bool AllowCaching, bool IncludeObjects, bool IncludeLinks, String CssClassName, bool RecurseChildren, int OnlyExpandForThisChildID, String Prefix)
        {
            String CacheName = String.Format("GetEntityULList_{0}_{1}_{2}_{3}_{4}_{5}_{6}_{7}_{8}_{9}_{10}_{11}", m_EntitySpecs.m_EntityName, ForParentEntityID.ToString(), LocaleSetting, AppLogic.IsAdminSite.ToString(), IncludeObjects, IncludeLinks, CssClassName, RecurseChildren.ToString(), OnlyExpandForThisChildID.ToString(), Prefix, AffiliateID.ToString(), CustomerLevelID.ToString());
            if (AppLogic.CachingOn)
            {
                String Menu = (String)HttpContext.Current.Cache.Get(CacheName);
                if (Menu != null)
                {
                    if (CommonLogic.ApplicationBool("DumpSQL"))
                    {
                        HttpContext.Current.Response.Write("Cache Hit Found!\n");
                    }
                    return Menu;
                }
            }

            XmlDocument xdoc = new XmlDocument();
            if (IncludeObjects)
            {
                string sql = GetListProductsSQL(0, AffiliateID, CustomerLevelID, true, true, false);
                string xml = String.Empty;
                int rows = DB.GetXml(sql, "objects", "object", ref xml);
                xdoc.LoadXml(m_TblMgr.XmlDoc.InnerXml.Replace("</root>", xml + "</root>"));
            }
            else
            {
                if (m_TblMgr.XmlDoc.HasChildNodes)
                {
                    xdoc = m_TblMgr.XmlDoc;
                }
            }

            Prefix = Prefix + Prefix;
            StringWriter tmpS = new StringWriter();
            String XslFile = "EntityULList";
            XslCompiledTransform xForm;
            string XslFilePath = CommonLogic.SafeMapPath("EntityHelper/" + XslFile + ".xslt");
            xForm = (XslCompiledTransform)HttpContext.Current.Cache.Get(XslFilePath);
            if (xForm == null)
            {
                xForm = new XslCompiledTransform(false);
                xForm.Load(XslFilePath);
                HttpContext.Current.Cache.Insert(XslFilePath, xForm, new CacheDependency(XslFilePath));
            }
            XsltArgumentList xslArgs = new XsltArgumentList();
            xslArgs.AddParam("entity", "", m_EntitySpecs.m_EntityName);
            xslArgs.AddParam("ForParentEntityID", "", ForParentEntityID);
            xslArgs.AddParam("IncludeLinks", "", IncludeLinks);
            xslArgs.AddParam("AffiliateID", "", AffiliateID);
            xslArgs.AddParam("IncludeObjects", "", IncludeObjects);
            xslArgs.AddParam("CssClassName", "", CssClassName);
            xslArgs.AddParam("RecurseChildren", "", RecurseChildren);
            xslArgs.AddParam("OnlyExpandForThisChildID", "", OnlyExpandForThisChildID);
            xslArgs.AddParam("Prefix", "", Prefix);
            xslArgs.AddParam("custlocale", "", LocaleSetting);
            xslArgs.AddParam("deflocale", "", Localization.GetDefaultLocale());
            xslArgs.AddParam("adminsite", "", AppLogic.IsAdminSite);
            xslArgs.AddParam("objName", "", m_EntitySpecs.m_ObjectName.ToLowerInvariant());
            xForm.Transform(xdoc, xslArgs, tmpS);
            if (AppLogic.AppConfigBool("XmlPackage.DumpTransform"))
            {
                try // don't let logging crash the site
                {
                    StreamWriter sw = File.CreateText(CommonLogic.SafeMapPath(String.Format("{0}images/{1}_{2}_{3}.xfrm.xml", CommonLogic.IIF(AppLogic.IsAdminSite, "../", ""), XslFile, m_EntitySpecs.m_EntityName, CommonLogic.IIF(AppLogic.IsAdminSite, "admin", "store"))));
                    sw.WriteLine(XmlCommon.PrettyPrintXml(tmpS.ToString()));
                    sw.Close();
                }
                catch { }
            }

            if (AppLogic.CachingOn)
            {
                HttpContext.Current.Cache.Insert(CacheName, tmpS.ToString(), null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }
            return tmpS.ToString();

        }

        public String GetEntityULListPhone(int ForParentEntityID, String LocaleSetting, int AffiliateID, int CustomerLevelID, bool AllowCaching, bool IncludeObjects, bool IncludeLinks, String CssClassName, bool RecurseChildren, int OnlyExpandForThisChildID, String Prefix, String IGD)
        {
            String CacheName = String.Format("GetEntityULListPhone_{0}_{1}_{2}_{3}_{4}_{5}_{6}_{7}_{8}_{9}_{10}_{11}_{12}", m_EntitySpecs.m_EntityName, ForParentEntityID.ToString(), LocaleSetting, AppLogic.IsAdminSite.ToString(), IncludeObjects, IncludeLinks, CssClassName, RecurseChildren.ToString(), OnlyExpandForThisChildID.ToString(), Prefix, AffiliateID.ToString(), CustomerLevelID.ToString(), IGD);
            if (AppLogic.CachingOn)
            {
                String Menu = (String)HttpContext.Current.Cache.Get(CacheName);
                if (Menu != null)
                {
                    if (CommonLogic.ApplicationBool("DumpSQL"))
                    {
                        HttpContext.Current.Response.Write("Cache Hit Found!\n");
                    }
                    return Menu;
                }
            }

            XmlDocument xdoc = new XmlDocument();
            if (IncludeObjects)
            {
                string sql = GetListProductsSQL(0, AffiliateID, CustomerLevelID, true, true, false);
                string xml = String.Empty;
                int rows = DB.GetXml(sql, "objects", "object", ref xml);
                xdoc.LoadXml(m_TblMgr.XmlDoc.InnerXml.Replace("</root>", xml + "</root>"));
            }
            else
            {
                if (m_TblMgr.XmlDoc.InnerXml.Length != 0)
                {
                    xdoc.LoadXml(m_TblMgr.XmlDoc.InnerXml);
                }
            }

            Prefix = Prefix + Prefix;
            StringWriter tmpS = new StringWriter();
            String XslFile = "EntityULListPhone";
            XslCompiledTransform xForm;
            string XslFilePath = CommonLogic.SafeMapPath("EntityHelper/" + XslFile + ".xslt");
            xForm = (XslCompiledTransform)HttpContext.Current.Cache.Get(XslFilePath);
            if (xForm == null)
            {
                xForm = new XslCompiledTransform(false);
                xForm.Load(XslFilePath);
                HttpContext.Current.Cache.Insert(XslFilePath, xForm, new CacheDependency(XslFilePath));
            }
            XsltArgumentList xslArgs = new XsltArgumentList();
            xslArgs.AddParam("entity", "", m_EntitySpecs.m_EntityName);
            xslArgs.AddParam("ForParentEntityID", "", ForParentEntityID);
            xslArgs.AddParam("IncludeLinks", "", IncludeLinks);
            xslArgs.AddParam("AffiliateID", "", AffiliateID);
            xslArgs.AddParam("IncludeObjects", "", IncludeObjects);
            xslArgs.AddParam("CssClassName", "", CssClassName);
            xslArgs.AddParam("RecurseChildren", "", RecurseChildren);
            xslArgs.AddParam("OnlyExpandForThisChildID", "", OnlyExpandForThisChildID);
            xslArgs.AddParam("Prefix", "", Prefix);
            xslArgs.AddParam("custlocale", "", LocaleSetting);
            xslArgs.AddParam("deflocale", "", Localization.GetDefaultLocale());
            xslArgs.AddParam("adminsite", "", AppLogic.IsAdminSite);
            xslArgs.AddParam("objName", "", m_EntitySpecs.m_ObjectName.ToLowerInvariant());
            xslArgs.AddParam("IGD", "", IGD);
            xForm.Transform(xdoc, xslArgs, tmpS);
            if (AppLogic.AppConfigBool("XmlPackage.DumpTransform"))
            {
                try // don't let logging crash the site
                {
                    StreamWriter sw = File.CreateText(CommonLogic.SafeMapPath(String.Format("{0}images/{1}_{2}_{3}.xfrm.xml", CommonLogic.IIF(AppLogic.IsAdminSite, "../", ""), XslFile, m_EntitySpecs.m_EntityName, CommonLogic.IIF(AppLogic.IsAdminSite, "admin", "store"))));
                    sw.WriteLine(XmlCommon.PrettyPrintXml(tmpS.ToString()));
                    sw.Close();
                }
                catch { }
            }

            if (AppLogic.CachingOn)
            {
                HttpContext.Current.Cache.Insert(CacheName, tmpS.ToString(), null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }
            return tmpS.ToString();

        }


        public String GetEntityComponentArtNode(int ForParentEntityID, String LocaleSetting, int AffiliateID, int CustomerLevelID, bool AllowCaching, bool ShowObjects)
        {
            String CacheName = String.Format("GetEntityComponentArtNode_{0}_{1}_{2}_{3}_{4}_{5}_{6}", m_EntitySpecs.m_EntityName, ForParentEntityID.ToString(), LocaleSetting, AppLogic.IsAdminSite.ToString(), ShowObjects.ToString(), AffiliateID.ToString(), CustomerLevelID.ToString());
            if (AppLogic.CachingOn)
            {
                String Menu = (String)HttpContext.Current.Cache.Get(CacheName);
                if (Menu != null)
                {
                    if (CommonLogic.ApplicationBool("DumpSQL"))
                    {
                        HttpContext.Current.Response.Write("Cache Hit Found!\n");
                    }
                    return Menu;
                }
            }

            StringBuilder tmpS = new StringBuilder(4096);

            XmlNode n;
            if (ForParentEntityID == 0)
            {
                n = m_TblMgr.ResetToRootNode();
            }
            else
            {
                n = m_TblMgr.SetContext(ForParentEntityID);
            }

            if (n != null && m_TblMgr.HasChildren(n))
            {
                n = m_TblMgr.MoveFirstChild(n);
                while (n != null)
                {
                    int ThisID = m_TblMgr.CurrentID(n);
                    String URL = String.Empty;
                    if (AppLogic.IsAdminSite)
                    {
                        URL = String.Format("newentities.aspx?entityname={0}&{1}id={2}", m_EntitySpecs.m_EntityName, m_EntitySpecs.m_EntityName, ThisID.ToString());
                    }
                    else
                    {
                        URL = String.Format(SE.MakeEntityLink(m_EntitySpecs.m_EntityName, ThisID, m_TblMgr.CurrentField(n, "SEName")));
                    }
                    tmpS.Append("<node Text=\"" + XmlCommon.XmlEncodeAttribute(m_TblMgr.CurrentName(n, LocaleSetting)) + "\" NavigateUrl=\"" + XmlCommon.XmlEncodeAttribute(URL) + "\">\n");
                    if (AppLogic.IsAdminSite || ShowObjects)
                    {
                        tmpS.Append(GetEntityComponentArtObjectList(ThisID, LocaleSetting, AffiliateID, CustomerLevelID));
                    }
                    if (m_TblMgr.HasChildren(n))
                    {
                        tmpS.Append(GetEntityComponentArtNode(ThisID, LocaleSetting, AffiliateID, CustomerLevelID, AllowCaching, ShowObjects));
                    }
                    tmpS.Append("</node>");
                    n = m_TblMgr.MoveNextSibling(n, false);
                }
            }
            if (AppLogic.CachingOn)
            {
                HttpContext.Current.Cache.Insert(CacheName, tmpS.ToString(), null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }
            return tmpS.ToString();
        }

        public String GetEntityPhoneOrderNode(int ForParentEntityID, String LocaleSetting, int AffiliateID, int CustomerLevelID, bool AllowCaching, bool ShowObjects, String IGD)
        {
            String CacheName = String.Format("GetEntityPhoneOrderNode_{0}_{1}_{2}_{3}_{4}_{5}_{6}_{7}", m_EntitySpecs.m_EntityName, ForParentEntityID.ToString(), LocaleSetting, AppLogic.IsAdminSite.ToString(), ShowObjects.ToString(), AffiliateID.ToString(), CustomerLevelID.ToString(), IGD);
            if (AppLogic.CachingOn)
            {
                String Menu = (String)HttpContext.Current.Cache.Get(CacheName);
                if (Menu != null)
                {
                    if (CommonLogic.ApplicationBool("DumpSQL"))
                    {
                        HttpContext.Current.Response.Write("Cache Hit Found!\n");
                    }
                    return Menu;
                }
            }

            StringBuilder tmpS = new StringBuilder(4096);

            XmlNode n;
            if (ForParentEntityID == 0)
            {
                n = m_TblMgr.ResetToRootNode();
            }
            else
            {
                n = m_TblMgr.SetContext(ForParentEntityID);
            }

            if (n != null && m_TblMgr.HasChildren(n))
            {
                n = m_TblMgr.MoveFirstChild(n);
                while (n != null)
                {
                    int ThisID = m_TblMgr.CurrentID(n);
                    tmpS.Append("<node Text=\"" + XmlCommon.XmlEncodeAttribute(m_TblMgr.CurrentName(n, LocaleSetting)) + "\">\n");
                    if (ShowObjects)
                    {
                        tmpS.Append(GetEntityPhoneOrderObjectList(ThisID, LocaleSetting, AffiliateID, CustomerLevelID, IGD));
                    }
                    if (m_TblMgr.HasChildren(n))
                    {
                        tmpS.Append(GetEntityPhoneOrderNode(ThisID, LocaleSetting, AffiliateID, CustomerLevelID, AllowCaching, ShowObjects, IGD));
                    }
                    tmpS.Append("</node>");
                    n = m_TblMgr.MoveNextSibling(n, false);
                }
            }
            if (AppLogic.CachingOn)
            {
                HttpContext.Current.Cache.Insert(CacheName, tmpS.ToString(), null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }
            return tmpS.ToString();
        }

        public String GetEntityGoogleSiteMap(int ForParentEntityID, String LocaleSetting, bool AllowCaching, bool RecurseChildren)
        {
            String CacheName = String.Format("GetEntityGoogleSiteMap_{0}_{1}_{2}_{3}", m_EntitySpecs.m_EntityName, ForParentEntityID.ToString(), LocaleSetting, RecurseChildren.ToString());
            if (AppLogic.CachingOn)
            {
                String Menu = (String)HttpContext.Current.Cache.Get(CacheName);
                if (Menu != null)
                {
                    return Menu;
                }
            }
            String StoreLoc = AppLogic.GetStoreHTTPLocation(false);

            StringWriter tmpS = new StringWriter();
            String XslFile = "EntityGoogleSiteMap";
            XslCompiledTransform xForm;
            string XslFilePath = CommonLogic.SafeMapPath("EntityHelper/" + XslFile + ".xslt");
            xForm = (XslCompiledTransform)HttpContext.Current.Cache.Get(XslFilePath);
            if (xForm == null)
            {
                xForm = new XslCompiledTransform(false);
                xForm.Load(XslFilePath);
                HttpContext.Current.Cache.Insert(XslFilePath, xForm, new CacheDependency(XslFilePath));
            }
            XsltArgumentList xslArgs = new XsltArgumentList();
            xslArgs.AddParam("entity", "", m_EntitySpecs.m_EntityName);
            xslArgs.AddParam("ForParentEntityID", "", ForParentEntityID);
            xslArgs.AddParam("StoreLoc", "", StoreLoc);
            xForm.Transform(m_TblMgr.XmlDoc, xslArgs, tmpS);
            if (AppLogic.AppConfigBool("XmlPackage.DumpTransform"))
            {
                try // don't let logging crash the site
                {
                    StreamWriter sw = File.CreateText(CommonLogic.SafeMapPath(String.Format("{0}images/{1}_{2}_{3}.xfrm.xml", CommonLogic.IIF(AppLogic.IsAdminSite, "../", ""), XslFile, m_EntitySpecs.m_EntityName, CommonLogic.IIF(AppLogic.IsAdminSite, "admin", "store"))));
                    sw.WriteLine(XmlCommon.PrettyPrintXml(tmpS.ToString()));
                    sw.Close();
                }
                catch { }
            }

            if (AppLogic.CachingOn)
            {
                HttpContext.Current.Cache.Insert(CacheName, tmpS.ToString(), null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }
            return tmpS.ToString();
        }

        public String GetEntityRorSiteMap(int ForParentEntityID, String LocaleSetting, bool AllowCaching, bool RecurseChildren)
        {
            String CacheName = String.Format("GetEntityRorSiteMap_{0}_{1}_{2}_{3}", m_EntitySpecs.m_EntityName, ForParentEntityID.ToString(), LocaleSetting, RecurseChildren.ToString());
            if (AppLogic.CachingOn)
            {
                String Menu = (String)HttpContext.Current.Cache.Get(CacheName);
                if (Menu != null)
                {
                    return Menu;
                }
            }
            String StoreLoc = AppLogic.GetStoreHTTPLocation(false);

            StringWriter tmpS = new StringWriter();
            String XslFile = "EntityRorSiteMap";
            XslCompiledTransform xForm;
            string XslFilePath = CommonLogic.SafeMapPath("EntityHelper/" + XslFile + ".xslt");
            xForm = (XslCompiledTransform)HttpContext.Current.Cache.Get(XslFilePath);
            if (xForm == null)
            {
                xForm = new XslCompiledTransform(false);
                xForm.Load(XslFilePath);
                HttpContext.Current.Cache.Insert(XslFilePath, xForm, new CacheDependency(XslFilePath));
            }
            XsltArgumentList xslArgs = new XsltArgumentList();
            xslArgs.AddParam("entity", "", m_EntitySpecs.m_EntityName);
            xslArgs.AddParam("ForParentEntityID", "", ForParentEntityID);
            xslArgs.AddParam("StoreLoc", "", StoreLoc);
            xForm.Transform(m_TblMgr.XmlDoc, xslArgs, tmpS);
            if (AppLogic.AppConfigBool("XmlPackage.DumpTransform"))
            {
                try // don't let logging crash the site
                {
                    StreamWriter sw = File.CreateText(CommonLogic.SafeMapPath(String.Format("{0}images/{1}_{2}_{3}.xfrm.xml", CommonLogic.IIF(AppLogic.IsAdminSite, "../", ""), XslFile, m_EntitySpecs.m_EntityName, CommonLogic.IIF(AppLogic.IsAdminSite, "admin", "store"))));
                    sw.WriteLine(XmlCommon.PrettyPrintXml(tmpS.ToString()));
                    sw.Close();
                }
                catch { }
            }

            if (AppLogic.CachingOn)
            {
                HttpContext.Current.Cache.Insert(CacheName, tmpS.ToString(), null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }
            return tmpS.ToString();
        }

        public String GetListProductsSQL(int EntityInstanceID, int AffiliateID, int CustomerLevelID, bool AllowKits, bool AllowPacks, bool OrderByLooks)
        {
            String OneVariantSQL = String.Empty;
            String FilterSQL = String.Empty;
            String OrderBySQL = String.Empty;
            String FinalSQL = String.Empty;

            FinalSQL = "exec aspdnsf_GetSimpleObjectEntityList @entityname=" + DB.SQuote(m_EntitySpecs.m_EntityName);

            if (EntityInstanceID != 0)
            {
                FinalSQL += ", @entityid=" + EntityInstanceID.ToString();
            }

            if (AppLogic.AppConfigBool("Filter" + m_EntitySpecs.m_ObjectName + "sByAffiliate"))
            {
                FinalSQL += ", @affiliateid=" + AffiliateID.ToString();
            }
            if (AppLogic.AppConfigBool("Filter" + m_EntitySpecs.m_ObjectName + "sByCustomerLevel"))
            {
                FinalSQL += ", @customerlevelid=" + CustomerLevelID.ToString();

            }

            if (!AllowKits)
            {
                FinalSQL += ", @AllowKits=0";
            }

            if (!AllowPacks)
            {
                FinalSQL += ", @AllowPacks=0";
            }
            if (!AppLogic.IsAdminSite)
            {
                FinalSQL += ", @PublishedOnly=1";
            }

            if (OrderByLooks)
            {
                FinalSQL += ", @OrderByLooks=1";
            }

            return FinalSQL;
        }


        private String GetEntityComponentArtObjectList(int EntityID, String LocaleSetting, int AffiliateID, int CustomerLevelID)
        {
            String CacheName = String.Format("GetEntityComponentArtObjectList_{0}_{1}_{2}_{3}", EntityID.ToString(), LocaleSetting, AffiliateID.ToString(), CustomerLevelID.ToString());
            if (AppLogic.CachingOn)
            {
                String s = (String)HttpContext.Current.Cache.Get(CacheName);
                if (s != null)
                {
                    if (CommonLogic.ApplicationBool("DumpSQL"))
                    {
                        HttpContext.Current.Response.Write("Cache Hit Found!\n");
                    }
                    return s;
                }
            }

            StringBuilder tmpS = new StringBuilder(1000);

            if (AppLogic.AppConfigBool("sitemap.showproducts"))
            {
                String sql = GetListProductsSQL(EntityID, AffiliateID, CustomerLevelID, true, true, false);

                using (SqlConnection dbconn = DB.dbConn())
                {
                    dbconn.Open();
                    using (IDataReader rs = DB.GetRS(sql, dbconn))
                    {
                        while (rs.Read())
                        {
                            String URL = String.Empty;
                            if (AppLogic.IsAdminSite)
                            {
                                URL = String.Format("newentities.aspx?entityname={0}&{1}id={2}", m_EntitySpecs.m_ObjectName, m_EntitySpecs.m_ObjectName, DB.RSFieldInt(rs, "ObjectID").ToString());
                            }
                            else
                            {
                                URL = SE.MakeObjectAndEntityLink(m_EntitySpecs.m_ObjectName, m_EntitySpecs.m_EntityName, DB.RSFieldInt(rs, "ObjectID"), EntityID, DB.RSField(rs, "SEName"));
                            }
                            tmpS.Append("<node Text=\"" + XmlCommon.XmlEncodeAttribute(DB.RSFieldByLocale(rs, "Name", LocaleSetting)) + "\" NavigateUrl=\"" + XmlCommon.XmlEncodeAttribute(URL) + "\" />\n");
                        }
                    }
                }

            }

            if (AppLogic.CachingOn)
            {
                HttpContext.Current.Cache.Insert(CacheName, tmpS.ToString(), null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }

            return tmpS.ToString();
        }

        private String GetEntityPhoneOrderObjectList(int EntityID, String LocaleSetting, int AffiliateID, int CustomerLevelID, String IGD)
        {
            String CacheName = String.Format("GetEntityPhoneOrderObjectList_{0}_{1}_{2}_{3}_{4}_{5}", m_EntitySpecs.m_EntityName, EntityID.ToString(), LocaleSetting, AffiliateID.ToString(), CustomerLevelID.ToString(), IGD);
            if (AppLogic.CachingOn)
            {
                String s = (String)HttpContext.Current.Cache.Get(CacheName);
                if (s != null)
                {
                    if (CommonLogic.ApplicationBool("DumpSQL"))
                    {
                        HttpContext.Current.Response.Write("Cache Hit Found!\n");
                    }
                    return s;
                }
            }

            StringBuilder tmpS = new StringBuilder(1000);
            String sql = GetListProductsSQL(EntityID, AffiliateID, CustomerLevelID, true, true, false);

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS(sql, dbconn))
                {
                    while (rs.Read())
                    {
                        String URL = "javascript:parent.RightPanel2Frame.location.href='../showproduct.aspx?IGD=" + IGD + "&amp;productid=" + DB.RSFieldInt(rs, "ObjectID").ToString() + "';javascript:void(0);";
                        tmpS.Append("<node Text=\"" + XmlCommon.XmlEncodeAttribute(DB.RSFieldByLocale(rs, "Name", LocaleSetting)) + "\" NavigateUrl=\"" + URL + "\"/>\n");
                    }
                }
            }

            if (AppLogic.CachingOn)
            {
                HttpContext.Current.Cache.Insert(CacheName, tmpS.ToString(), null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }

            return tmpS.ToString();
        }

        public String GetEntityGoogleObjectList(int EntityID, String LocaleSetting, int AffiliateID, int CustomerLevelID)
        {
            String CacheName = String.Format("GetEntityGoogleObjectList_{0}_{1}_{2}_{3}_{4}", m_EntitySpecs.m_EntityName, EntityID.ToString(), LocaleSetting, AffiliateID.ToString(), CustomerLevelID.ToString());
            if (AppLogic.CachingOn)
            {
                String s = (String)HttpContext.Current.Cache.Get(CacheName);
                if (s != null)
                {
                    return s;
                }
            }

            String StoreLoc = AppLogic.GetStoreHTTPLocation(false, false);
            StringBuilder tmpS = new StringBuilder(1000);
            String sql = GetListProductsSQL(EntityID, AffiliateID, CustomerLevelID, true, true, false);

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS(sql, dbconn))
                {
                    while (rs.Read())
                    {
                        tmpS.Append("<url>");
                        tmpS.Append("<loc>" + StoreLoc + SE.MakeObjectAndEntityLink(m_EntitySpecs.m_ObjectName, m_EntitySpecs.m_EntityName, DB.RSFieldInt(rs, "ObjectID"), EntityID, DB.RSField(rs, "SEName")) + "</loc> ");
                        tmpS.Append("<changefreq>" + AppLogic.AppConfig("GoogleSiteMap.ObjectChangeFreq") + "</changefreq> ");
                        tmpS.Append("<priority>" + AppLogic.AppConfig("GoogleSiteMap.ObjectPriority") + "</priority> ");
                        tmpS.Append("</url>");
                    }
                }
            }

            if (AppLogic.CachingOn)
            {
                HttpContext.Current.Cache.Insert(CacheName, tmpS.ToString(), null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }

            return tmpS.ToString();
        }

        public String GetEntityRorObjectList(int EntityID, String LocaleSetting, int AffiliateID, int CustomerLevelID)
        {
            String CacheName = String.Format("GetEntityRorObjectList_{0}_{1}_{2}_{3}_{4}", m_EntitySpecs.m_EntityName, EntityID.ToString(), LocaleSetting, AffiliateID.ToString(), CustomerLevelID.ToString());
            if (AppLogic.CachingOn)
            {
                String s = (String)HttpContext.Current.Cache.Get(CacheName);
                if (s != null)
                {
                    return s;
                }
            }

            String StoreLoc = AppLogic.GetStoreHTTPLocation(false);
            StringBuilder tmpS = new StringBuilder(1000);
            ProductCollection products = new ProductCollection();
            products.PageSize = 0;
            products.PageNum = 1;
            products.ReturnAllVariants = false;
            products.PublishedOnly = true;
            products.AffiliateID = 0;
            products.CustomerLevelID = 0;
            products.LocaleSetting = Localization.GetDefaultLocale();
            products.AddEntityFilter(m_EntitySpecs.m_EntityName, EntityID);
            DataSet dsProducts = products.LoadFromDB();
            int NumProducts = products.NumProducts;

            if (dsProducts.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in dsProducts.Tables[0].Rows)
                {
                    tmpS.Append("<item>");
                    tmpS.Append("   <title>" + XmlCommon.XmlEncode(DB.RowField(row, "Name")) + "</title>");
                    tmpS.Append("   <link>" + StoreLoc + SE.MakeObjectAndEntityLink(m_EntitySpecs.m_ObjectName, m_EntitySpecs.m_EntityName, DB.RowFieldInt(row, "ProductID"), EntityID, DB.RowField(row, "SEName")) + "</link> ");
                    tmpS.Append("   <description>" + XmlCommon.XmlEncode(DB.RowField(row, "Description")) + "</description>");
                    tmpS.Append("   <ror:keywords>" + XmlCommon.XmlEncode(DB.RowField(row, "SEKeywords")) + "</ror:keywords>");
                    tmpS.Append("   <ror:image>" + StoreLoc + XmlCommon.XmlEncode(AppLogic.LookupImage("Product", DB.RowFieldInt(row, "ProductID"), "medium", 1, Localization.GetDefaultLocale())) + "</ror:image>");
                    tmpS.Append("   <ror:imageSmall>" + StoreLoc + XmlCommon.XmlEncode(AppLogic.LookupImage("Product", DB.RowFieldInt(row, "ProductID"), "icon", 1, Localization.GetDefaultLocale())) + "</ror:imageSmall>");
                    tmpS.Append("   <ror:type>Product</ror:type>");
                    tmpS.Append("   <ror:price>" + Localization.CurrencyStringForGatewayWithoutExchangeRate(DB.RowFieldDecimal(row, "Price")) + "</ror:price>");
                    tmpS.Append("   <ror:currency>" + Localization.StoreCurrency() + "</ror:currency>");
                    tmpS.Append("   <ror:available>yes</ror:available>");
                    tmpS.Append("</item>");
                }
            }
            dsProducts.Dispose();

            if (AppLogic.CachingOn)
            {
                HttpContext.Current.Cache.Insert(CacheName, tmpS.ToString(), null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }

            return tmpS.ToString();
        }

        public String GetEntityPromptSingular(int SkinID, String LocaleSetting)
        {
            return CommonLogic.IIF(AppLogic.GetString("AppConfig." + m_EntitySpecs.m_EntityName + "PromptSingular", SkinID, LocaleSetting).Length != 0, AppLogic.GetString("AppConfig." + m_EntitySpecs.m_EntityName + "PromptSingular", SkinID, LocaleSetting), m_EntitySpecs.m_EntityName);
        }

        public String GetEntityPromptPlural(int SkinID, String LocaleSetting)
        {
            return CommonLogic.IIF(AppLogic.GetString("AppConfig." + m_EntitySpecs.m_EntityName + "PromptPlural", SkinID, LocaleSetting).Length != 0, AppLogic.GetString("AppConfig." + m_EntitySpecs.m_EntityName + "PromptPlural", SkinID, LocaleSetting), m_EntitySpecs.m_EntityNamePlural);
        }

        //low level accessor: no "store" logic applied:
        // if RecurseUp is set, it will look UP the entity hierarchy for all parent entities also
        public int GetEntityQuantityDiscountID(int EntityID, bool RecurseUp)
        {
            int tmp = 0;
            if (EntityID != 0)
            {
                XmlNode n = m_TblMgr.SetContext(EntityID);
                if (n != null)
                {
                    tmp = m_TblMgr.CurrentFieldInt(n, "QuantityDiscountID");
                    if (tmp == 0 && RecurseUp)
                    {
                        XmlNode parn = m_TblMgr.MoveParent(n);
                        if (parn != null)
                        {
                            return GetEntityQuantityDiscountID(m_TblMgr.CurrentID(parn), RecurseUp);
                        }
                    }
                }
            }
            return tmp;
        }

        //low level accessor: no "store" logic applied:
        // if RecurseUp is set, it will look UP the entity hierarchy for all parent entities also
        public int GetEntityTaxClassID(int EntityID, bool RecurseUp)
        {
            int tmp = 0;
            if (EntityID != 0)
            {
                XmlNode n = m_TblMgr.SetContext(EntityID);
                if (n != null)
                {
                    tmp = m_TblMgr.CurrentFieldInt(n, "TaxClassID");
                    if (tmp == 0 && RecurseUp)
                    {
                        XmlNode parn = m_TblMgr.MoveParent(n);
                        if (parn != null)
                        {
                            return GetEntityTaxClassID(m_TblMgr.CurrentID(parn), RecurseUp);
                        }
                    }
                }
            }
            return tmp;
        }

        public int GetParentEntity(int EntityID)
        {
            int tmp = 0;
            if (EntityID != 0)
            {
                XmlNode n = m_TblMgr.SetContext(EntityID);
                if (n != null)
                {
                    n = m_TblMgr.MoveParent(n);
                    if (n != null)
                    {
                        tmp = m_TblMgr.CurrentID(n);
                    }
                }
            }
            return tmp;
        }

        public int GetRootEntity(int RootOrSubEntityID)
        {
            int tmp = 0;
            if (RootOrSubEntityID != 0)
            {
                XmlNode n = m_TblMgr.SetContext(RootOrSubEntityID);
                while (n != null && !m_TblMgr.IsRootLevel(n))
                {
                    n = m_TblMgr.MoveParent(n);
                }
                tmp = m_TblMgr.CurrentID(n);
            }
            return tmp;
        }


        public int GetEntityDisplayOrder(int EntityID)
        {
            int tmp = 0;
            if (EntityID != 0)
            {
                XmlNode n = m_TblMgr.SetContext(EntityID);
                if (n != null)
                {
                    tmp = m_TblMgr.CurrentFieldInt(n, "DisplayOrder");
                }
            }
            return tmp;
        }

        public String GetParentEntityName(int EntityID, String LocaleSetting)
        {
            return GetEntityName(GetParentEntity(EntityID), LocaleSetting);
        }

        public bool EntityHasSubs(int EntityID)
        {
            if (EntityID != 0)
            {
                XmlNode n = m_TblMgr.SetContext(EntityID);
                if (n != null)
                {
                    return m_TblMgr.HasChildren(n);
                }
            }
            return false;
        }

        public String GetEntityBox(int EntityID, bool subCatsOnly, int showNum, bool showPics, String teaser, int SkinID, String LocaleSetting)
        {
            String CacheName = String.Format("EntityBox_{0}_{1}_{2}_{3}_{4}_{5}_{6}_{7}_{8}", m_EntitySpecs.m_EntityName, EntityID.ToString(), subCatsOnly.ToString(), showNum.ToString(), showPics.ToString(), teaser, SkinID.ToString(), LocaleSetting, AppLogic.IsAdminSite.ToString());
            if (AppLogic.CachingOn)
            {
                String ObjectsMenu = (String)HttpContext.Current.Cache.Get(CacheName);
                if (ObjectsMenu != null)
                {
                    if (CommonLogic.ApplicationBool("DumpSQL"))
                    {
                        HttpContext.Current.Response.Write("Cache Hit Found!\n");
                    }
                    return ObjectsMenu;
                }
            }

            StringBuilder tmpS = new StringBuilder(10000);
            tmpS.Append("<table width=\"171\" cellpadding=\"2\" cellspacing=\"0\" border=\"0\" style=\"border-style: solid; border-width: 0px; border-color: #" + AppLogic.AppConfig("HeaderBGColor") + "\">\n");
            tmpS.Append("<tr><td align=\"left\" valign=\"top\">\n");
            tmpS.Append("<a href=\"" + SE.MakeEntityLink(m_EntitySpecs.m_EntityName, EntityID, "") + "\"><img src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/kits.gif") + "\" border=\"0\" /></a>");
            tmpS.Append("<table width=\"100%\" cellpadding=\"4\" cellspacing=\"0\" border=\"0\" style=\"" + AppLogic.AppConfig("BoxFrameStyle") + "\">\n");
            tmpS.Append("<tr><td align=\"" + CommonLogic.IIF(showPics, "center", "left") + "\" valign=\"top\">\n");

            tmpS.Append("<p align=\"" + CommonLogic.IIF(showPics, "center", "left") + "\"><b>" + teaser + "</b></p>\n");

            if (subCatsOnly)
            {
                using (SqlConnection dbconn = DB.dbConn())
                {
                    dbconn.Open();
                    
                    string sqlSpecs = "select * from " + m_EntitySpecs.m_EntityName + "  with (NOLOCK)  where deleted=0 " + CommonLogic.IIF(m_OnlyPublishedEntitiesAndObjects, "and Published=1", "") + " and Parent" + m_IDColumnName + "=" + EntityID.ToString() + " order by DisplayOrder,Name";
                   
                    using (IDataReader rs = DB.GetRS(sqlSpecs, dbconn))
                    {
                        int i = 1;
                       
                        while (rs.Read())
                        {
                            if (i > showNum)
                            {
                                tmpS.Append("<img height=\"8\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/redarrow.gif") + "\" />&nbsp;<a href=\"" + SE.MakeEntityLink(m_EntitySpecs.m_EntityName, EntityID, DB.RSField(rs, "SEName")) + "\">more...</a>");
                                break;
                            }
                            tmpS.Append("<img height=\"8\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/redarrow.gif") + "\" />&nbsp;<a href=\"" + SE.MakeEntityLink(m_EntitySpecs.m_EntityName, DB.RSFieldInt(rs, m_IDColumnName), DB.RSField(rs, "SEName")) + "\">");
                            String ImgUrl = AppLogic.LookupImage(m_EntitySpecs.m_EntityName, DB.RSFieldInt(rs, m_IDColumnName), "icon", SkinID, LocaleSetting);
                            if (ImgUrl.Length != 0)
                            {
                                System.Drawing.Size size = CommonLogic.GetImagePixelSize(ImgUrl);
                                if (showPics)
                                {
                                    tmpS.Append("<img src=\"" + ImgUrl + "\" width=\"" + CommonLogic.IIF(size.Width >= 155, 155, size.Width).ToString() + "\" border=\"0\" />");
                                }
                            }
                            tmpS.Append(DB.RSFieldByLocale(rs, "Name", LocaleSetting) + "</a>");
                            tmpS.Append("");
                            if (showPics)
                            {
                                tmpS.Append("");
                            }
                            i++;
                        }
                    }
                }
            }
            else
            {
                using (SqlConnection dbconn = DB.dbConn())
                {
                    dbconn.Open();

                    string sqlDisp = "select p.*,DO.DisplayOrder from " + m_EntitySpecs.m_ObjectName + " P   with (NOLOCK)  left outer join " + m_EntitySpecs.m_EntityName + "DisplayOrder DO   with (NOLOCK)  on p." + m_EntitySpecs.m_ObjectName + "ID=do." + m_EntitySpecs.m_ObjectName + "ID and do." + m_IDColumnName + "=" + EntityID.ToString() + " where p.Deleted=0 " + CommonLogic.IIF(m_OnlyPublishedEntitiesAndObjects, "and p.Published=1", "") + " and p." + m_EntitySpecs.m_ObjectName + "ID in (select distinct " + m_EntitySpecs.m_ObjectName + "ID from " + m_EntitySpecs.m_ObjectName + "" + m_EntitySpecs.m_EntityName + "  with (NOLOCK)  where " + m_IDColumnName + "=" + EntityID.ToString() + ") order by do.displayorder";

                    using (IDataReader rs = DB.GetRS(sqlDisp, dbconn))
                    {
                        int i = 1;

                        while (rs.Read())
                        {
                            if (i > showNum)
                            {
                                tmpS.Append("<img height=\"8\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/redarrow.gif") + "\" />&nbsp;<a href=\"" + SE.MakeEntityLink(m_EntitySpecs.m_EntityName, EntityID, "") + "\">more...</a>");
                                break;
                            }
                            tmpS.Append("<img height=\"8\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/redarrow.gif") + "\" />&nbsp;<a href=\"" + SE.MakeObjectLink(m_EntitySpecs.m_ObjectName, DB.RSFieldInt(rs, "ObjectID"), DB.RSField(rs, "SEName")) + "\">");
                            String ImgUrl = AppLogic.LookupImage(m_EntitySpecs.m_ObjectName, DB.RSFieldInt(rs, "ObjectID"), "icon", SkinID, LocaleSetting);
                            if (ImgUrl.Length != 0)
                            {
                                System.Drawing.Size size = CommonLogic.GetImagePixelSize(ImgUrl);
                                if (showPics)
                                {
                                    tmpS.Append("<img src=\"" + ImgUrl + "\" width=\"" + CommonLogic.IIF(size.Width >= 155, 155, size.Width).ToString() + "\" border=\"0\" />");
                                }
                            }
                            tmpS.Append(DB.RSFieldByLocale(rs, "Name", LocaleSetting) + "</a>");
                            tmpS.Append("");
                            if (showPics)
                            {
                                tmpS.Append("");
                            }
                            i++;     
                        }
                    }
                }
            }

            tmpS.Append("</td></tr>\n");
            tmpS.Append("</table>\n");
            tmpS.Append("</td></tr>\n");
            tmpS.Append("</table>\n");
            if (AppLogic.CachingOn)
            {
                HttpContext.Current.Cache.Insert(CacheName, tmpS.ToString(), null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }
            return tmpS.ToString();
        }

        public String GetEntityBrowseBox(int SkinID, String LocaleSetting)
        {
            String CacheName = String.Format("GetEntityBrowseBox_{0}_{1}_{2}_{3}", m_EntitySpecs.m_EntityName, SkinID.ToString(), LocaleSetting, AppLogic.IsAdminSite.ToString());
            if (AppLogic.CachingOn)
            {
                String Menu = (String)HttpContext.Current.Cache.Get(CacheName);
                if (Menu != null)
                {
                    if (CommonLogic.ApplicationBool("DumpSQL"))
                    {
                        HttpContext.Current.Response.Write("Cache Hit Found!\n");
                    }
                    return Menu;
                }
            }

            StringBuilder tmpS = new StringBuilder(10000);

            XmlNode n = m_TblMgr.SetContextToFirstRootLevelNode();
            bool anyFound = false;
            if (n != null)
            {
                tmpS.Append("<table cellpadding=\"2\" cellspacing=\"0\" border=\"0\" style=\"border-style: solid; border-width: 0px; border-color: #" + AppLogic.AppConfig("HeaderBGColor") + "\">\n");
                tmpS.Append("<tr><td align=\"left\" valign=\"top\">\n");
                tmpS.Append("<img src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/browseby" + m_EntitySpecs.m_EntityName + ".gif") + "\" border=\"0\" />");
                tmpS.Append("<table width=\"100%\" cellpadding=\"4\" cellspacing=\"0\" border=\"0\" style=\"" + AppLogic.AppConfig("BoxFrameStyle") + "\">\n");
                tmpS.Append("<tr><td align=\"left\" valign=\"top\">\n");

                while (n != null)
                {
                    tmpS.Append("<img height=\"8\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/redarrow.gif") + "\" />&nbsp;<a href=\"" + SE.MakeEntityLink(m_EntitySpecs.m_EntityName, m_TblMgr.CurrentID(n), m_TblMgr.CurrentField(n, "SEName")) + "\">" + m_TblMgr.CurrentName(n, LocaleSetting) + "</a>");
                    anyFound = true;
                    n = m_TblMgr.MoveNextSibling(n, false);
                }

                tmpS.Append("</td></tr>\n");
                tmpS.Append("</table>\n");
                tmpS.Append("</td></tr>\n");
                tmpS.Append("</table>\n");
            }
            if (AppLogic.CachingOn)
            {
                HttpContext.Current.Cache.Insert(CacheName, tmpS.ToString(), null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }
            return CommonLogic.IIF(anyFound, tmpS.ToString(), String.Empty);
        }


        public String GetEntityBoxExpanded(int EntityID, int showNum, bool showPics, String teaser, int SkinID, String LocaleSetting)
        {
            String CacheName = String.Format("GetEntityBoxExpanded_{0}_{1}_{2}_{3}_{4}_{5}_{6}_{7}", m_EntitySpecs.m_EntityName, EntityID.ToString(), showNum.ToString(), showPics.ToString(), teaser, SkinID.ToString(), LocaleSetting, AppLogic.IsAdminSite.ToString());
            if (AppLogic.CachingOn)
            {
                String Menu = (String)HttpContext.Current.Cache.Get(CacheName);
                if (Menu != null)
                {
                    if (CommonLogic.ApplicationBool("DumpSQL"))
                    {
                        HttpContext.Current.Response.Write("Cache Hit Found!\n");
                    }
                    return Menu;
                }
            }

            StringBuilder tmpS = new StringBuilder(10000);
            tmpS.Append("<table width=\"100%\" cellpadding=\"2\" cellspacing=\"0\" border=\"0\" style=\"border-style: solid; border-width: 0px; border-color: #" + AppLogic.AppConfig("HeaderBGColor") + "\">\n");
            tmpS.Append("<tr><td align=\"left\" valign=\"top\">\n");
            tmpS.Append("<a href=\"" + SE.MakeEntityLink(m_EntitySpecs.m_EntityName, EntityID, "") + "\"><img src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/kitsexpanded.gif") + "\" border=\"0\" /></a>");
            tmpS.Append("<table width=\"100%\" cellpadding=\"4\" cellspacing=\"0\" border=\"0\" style=\"" + AppLogic.AppConfig("BoxFrameStyle") + "\">\n");
            tmpS.Append("<tr><td align=\"left\" valign=\"top\">\n");

            tmpS.Append("<p><b>" + teaser + "</b></p>\n");

            tmpS.Append("<table width=\"100%\" cellpadding=\"2\" cellspacing=\"0\" border=\"0\">\n");
            int i = 1;

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();

                string sqlDisp = "select p.*,DO.DisplayOrder from " + m_EntitySpecs.m_ObjectName + "   with (NOLOCK)  left outer join " + m_EntitySpecs.m_EntityName + "DisplayOrder do on p." + m_EntitySpecs.m_ObjectName + "ID=do." + m_EntitySpecs.m_ObjectName + "ID and do." + m_IDColumnName + "=" + EntityID.ToString() + " and p.deleted=0 " + CommonLogic.IIF(m_OnlyPublishedEntitiesAndObjects, "and p.Published=1", "") + " and p." + m_EntitySpecs.m_ObjectName + "ID in (select distinct " + m_EntitySpecs.m_ObjectName + "ID from " + m_EntitySpecs.m_ObjectName + "" + m_EntitySpecs.m_EntityName + "  with (NOLOCK)  where " + m_IDColumnName + "=" + EntityID.ToString() + ") order by do.displayorder";

                using (IDataReader rs = DB.GetRS(sqlDisp, dbconn))
                {
                    while (rs.Read())
                    {
                        if (i > showNum)
                        {
                            tmpS.Append("<tr><td " + CommonLogic.IIF(showPics, "colspan=\"2\"", "") + "><hr size=\"1\" class=\"LightCellText\"/><a href=\"" + SE.MakeEntityLink(m_EntitySpecs.m_EntityName, EntityID, "") + "\">more...</a></td></tr>");
                            break;
                        }
                        if (i > 1)
                        {
                            tmpS.Append("<tr><td " + CommonLogic.IIF(showPics, "colspan=\"2\"", "") + "><hr size=\"1\" class=\"LightCellText\"/></td></tr>");
                        }
                        tmpS.Append("<tr>");
                        String ImgUrl = AppLogic.LookupImage(m_EntitySpecs.m_ObjectName, DB.RSFieldInt(rs, "ObjectID"), "icon", SkinID, LocaleSetting);
                        if (showPics)
                        {
                            tmpS.Append("<td align=\"left\" valign=\"top\">\n");
                            System.Drawing.Size size = CommonLogic.GetImagePixelSize(ImgUrl);
                            tmpS.Append("<a href=\"" + SE.MakeObjectLink(m_EntitySpecs.m_ObjectName, DB.RSFieldInt(rs, "ObjectID"), DB.RSField(rs, "SEName")) + "\">");
                            if (size.Width > 100)
                            {
                                size.Width = 100;
                            }
                            if (size.Height > 100)
                            {
                                size.Height = 100;
                            }
                            tmpS.Append("<img align=\"left\" src=\"" + ImgUrl + "\" height=\"" + size.Height.ToString() + "\" width=\"" + size.Width.ToString() + "\" border=\"0\" />");
                            tmpS.Append("</a>");
                            tmpS.Append("</td>");
                        }

                        tmpS.Append("<td align=\"left\" valign=\"top\">\n");
                        tmpS.Append("<b class=\"a4\">");
                        tmpS.Append("<a href=\"" + SE.MakeObjectLink(m_EntitySpecs.m_ObjectName, DB.RSFieldInt(rs, "ObjectID"), DB.RSField(rs, "SEName")) + "\">" + DB.RSFieldByLocale(rs, "Name", LocaleSetting));
                        if (DB.RSField(rs, "Summary").Length != 0)
                        {
                            tmpS.Append(": " + DB.RSField(rs, "Summary"));
                        }
                        tmpS.Append("</a></b>\n");
                        if (DB.RSField(rs, "Description").Length != 0)
                        {
                            String tmpD = DB.RSField(rs, "Description");
                            if (AppLogic.ReplaceImageURLFromAssetMgr)
                            {
                                tmpD = tmpD.Replace("../images", "images");
                            }
                            tmpS.Append("<span class=\"a2\">" + tmpD + "</span>\n");
                        }
                        tmpS.Append("<div class=\"a1\" style=\"PADDING-BOTTOM: 10px\">\n");
                        tmpS.Append("<a href=\"" + SE.MakeObjectLink(m_EntitySpecs.m_ObjectName, DB.RSFieldInt(rs, "ObjectID"), DB.RSField(rs, "SEName")) + "\">");
                        tmpS.Append(AppLogic.GetString("common.cs.49", SkinID, Thread.CurrentThread.CurrentUICulture.Name));
                        tmpS.Append("</a>");
                        tmpS.Append("</div>\n");
                        tmpS.Append("</td>");
                        tmpS.Append("</tr>");
                        i++;
                    } 
                }
            }
            tmpS.Append("</table>\n");

            tmpS.Append("</td></tr>\n");
            tmpS.Append("</table>\n");
            tmpS.Append("</td></tr>\n");
            tmpS.Append("</table>\n");
            if (AppLogic.CachingOn)
            {
                HttpContext.Current.Cache.Insert(CacheName, tmpS.ToString(), null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }
            return tmpS.ToString();
        }

        public String GetEntityName(int EntityID, String LocaleSetting)
        {
            String tmp = String.Empty;
            if (EntityID != 0)
            {
                XmlNode n = m_TblMgr.SetContext(EntityID);
                if (n != null)
                {
                    tmp = m_TblMgr.CurrentFieldByLocale(n, "Name", LocaleSetting);
                }
            }
            return tmp;
        }

        public int GetEntityID(String Name, String LocaleSetting)
        {
            int id = 0;
            if (Name.Length != 0)
            {
                XmlNode n = m_TblMgr.SetContext(Name);
                if (n != null)
                {
                    id = m_TblMgr.CurrentID(n);
                }
            }
            return id;
        }

        public String OpenCubeMenu(int ForParentEntityID, int SkinID, String LocaleSetting)
        {
            String CacheName = String.Format("OpenCubeMenu{0}_{1}_{2}_{3}_{4}", m_EntitySpecs.m_EntityName, ForParentEntityID.ToString(), SkinID.ToString(), LocaleSetting, AppLogic.IsAdminSite.ToString());
            if (AppLogic.CachingOn)
            {
                String Menu = (String)HttpContext.Current.Cache.Get(CacheName);
                if (Menu != null)
                {

                    if (CommonLogic.ApplicationBool("DumpSQL"))
                    {

                        HttpContext.Current.Response.Write("Cache Hit Found!\n");
                    }
                    return Menu;
                }
            }

            StringBuilder tmpS = new StringBuilder(4096);

            XmlNode n;
            if (ForParentEntityID == 0)
            {
                n = m_TblMgr.ResetToRootNode();
            }
            else
            {
                n = m_TblMgr.SetContext(ForParentEntityID);
            }

            if (ForParentEntityID == 0 && !m_TblMgr.HasChildren(n))
            {
                // NO ENTITIES IN THE DB, RETURN EMPTY STRING
            }
            else
            {
                if (ForParentEntityID == 0)
                {
                    tmpS.Append("<li style=\"width:110px;\"><a href=\"" + CommonLogic.IIF(AppLogic.IsAdminSite, m_EntitySpecs.m_EntityNamePlural.ToLowerInvariant() + ".aspx", "#") + "\">" + AppLogic.GetString("AppConfig.{0}PromptPlural".FormatWith(m_EntitySpecs.m_EntityName), LocaleSetting) + "</a>\n");
                }
                if (n != null && m_TblMgr.HasChildren(n))
                {
                    if (ForParentEntityID == 0)
                    {
                        tmpS.Append("<div><ul style=\"width:140px;top:0px;left:0px;\"><div>\n");
                    }
                    else
                    {
                        tmpS.Append("<div><ul style=\"width:140px;top:-18px;left:135px;\"><div>\n");
                    }
                    n = m_TblMgr.MoveFirstChild(n);
                    while (n != null)
                    {
                        int ThisID = m_TblMgr.CurrentID(n);
                        String URL = String.Empty;
                        if (AppLogic.IsAdminSite)
                        {
                            URL = String.Format("newentities.aspx?entityname={0}&{1}id={2}", m_EntitySpecs.m_EntityName, m_EntitySpecs.m_EntityName, ThisID.ToString());
                        }
                        else
                        {
                            URL = SE.MakeEntityLink(m_EntitySpecs.m_EntityName, ThisID, "");
                        }
                        tmpS.Append("<li><a " + CommonLogic.IIF(AppLogic.IsAdminSite, " target=\"content\"", "") + " href=\"" + XmlCommon.XmlEncodeAttribute(URL) + "\">" + m_TblMgr.CurrentName(n, Thread.CurrentThread.CurrentUICulture.Name) + "</a>\n");
                        if (!AppLogic.AppConfigBool("Limit" + m_EntitySpecs.m_EntityName + "MenuToOneLevel") && m_TblMgr.HasChildren(n))
                        {
                            tmpS.Append(OpenCubeMenu(ThisID, SkinID, LocaleSetting));
                        }
                        n = m_TblMgr.MoveNextSibling(n, false);
                    }
                    tmpS.Append("</div></ul></div>\n");
                }
                if (ForParentEntityID == 0)
                {
                    tmpS.Append("</li>\n");
                }
            }

            if (AppLogic.CachingOn)
            {

                HttpContext.Current.Cache.Insert(CacheName, tmpS.ToString(), null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }
            return tmpS.ToString();
        }

        public String OpenCubeMenuVertical(int ForParentEntityID, int SkinID, String LocaleSetting)
        {
            String CacheName = String.Format("OpenCubeMenuVertical{0}_{1}_{2}_{3}_{4}", m_EntitySpecs.m_EntityName, ForParentEntityID.ToString(), SkinID.ToString(), LocaleSetting, AppLogic.IsAdminSite.ToString());
            if (AppLogic.CachingOn)
            {
                String Menu = (String)HttpContext.Current.Cache.Get(CacheName);
                if (Menu != null)
                {

                    if (CommonLogic.ApplicationBool("DumpSQL"))
                    {

                        HttpContext.Current.Response.Write("Cache Hit Found!\n");
                    }
                    return Menu;
                }
            }

            StringBuilder tmpS = new StringBuilder(4096);

            XmlNode n;
            if (ForParentEntityID == 0)
            {
                n = m_TblMgr.ResetToRootNode();
            }
            else
            {
                n = m_TblMgr.SetContext(ForParentEntityID);
            }

            if (ForParentEntityID == 0 && !m_TblMgr.HasChildren(n))
            {
                // NO ENTITIES IN THE DB, RETURN EMPTY STRING
            }
            else
            {
                if (ForParentEntityID == 0)
                {
                    tmpS.Append("<li style=\"width:175px;\"><a href=\"" + CommonLogic.IIF(AppLogic.IsAdminSite, m_EntitySpecs.m_EntityNamePlural.ToLowerInvariant() + ".aspx", "#") + "\">" + AppLogic.GetString("AppConfig.{0}PromptPlural".FormatWith(m_EntitySpecs.m_EntityName), LocaleSetting) + "</a>\n");
                }
                if (n != null && m_TblMgr.HasChildren(n))
                {
                    if (ForParentEntityID == 0)
                    {
                        tmpS.Append("<div><ul style=\"width:175px;top:-20px;left:173px;\"><div>\n");
                    }
                    else
                    {
                        tmpS.Append("<div><ul style=\"width:175px;top:-20px;left:173px;\"><div>\n");
                    }
                    n = m_TblMgr.MoveFirstChild(n);
                    while (n != null)
                    {
                        int ThisID = m_TblMgr.CurrentID(n);
                        String URL = String.Empty;
                        if (AppLogic.IsAdminSite)
                        {
                            URL = String.Format("newentities.aspx?entityname={0}&{1}id={2}", m_EntitySpecs.m_EntityName, m_EntitySpecs.m_EntityName, ThisID.ToString());
                        }
                        else
                        {
                            URL = SE.MakeEntityLink(m_EntitySpecs.m_EntityName, ThisID, "");
                        }
                        tmpS.Append("<li><a " + CommonLogic.IIF(AppLogic.IsAdminSite, " target=\"content\"", "") + " href=\"" + XmlCommon.XmlEncodeAttribute(URL) + "\">" + m_TblMgr.CurrentName(n, Thread.CurrentThread.CurrentUICulture.Name) + "</a>\n");
                        if (!AppLogic.AppConfigBool("Limit" + m_EntitySpecs.m_EntityName + "MenuToOneLevel") && m_TblMgr.HasChildren(n))
                        {
                            tmpS.Append(OpenCubeMenuVertical(ThisID, SkinID, LocaleSetting));
                        }
                        n = m_TblMgr.MoveNextSibling(n, false);
                    }
                    tmpS.Append("</div></ul></div>\n");
                }
                if (ForParentEntityID == 0)
                {
                    tmpS.Append("</li>\n");
                }
            }

            if (AppLogic.CachingOn)
            {

                HttpContext.Current.Cache.Insert(CacheName, tmpS.ToString(), null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }
            return tmpS.ToString();
        }

        public String OpenCubeTree(int ForParentEntityID, int SkinID, String LocaleSetting, int SelectedID)
        {
            String CacheName = String.Format("OpenCubeTree_{0}_{1}_{2}_{3}_{4}_{5}", m_EntitySpecs.m_EntityName, ForParentEntityID.ToString(), SkinID.ToString(), LocaleSetting, AppLogic.IsAdminSite.ToString(), SelectedID.ToString());
            if (AppLogic.CachingOn)
            {
                String Menu = (String)HttpContext.Current.Cache.Get(CacheName);
                if (Menu != null)
                {

                    if (CommonLogic.ApplicationBool("DumpSQL"))
                    {

                        HttpContext.Current.Response.Write("Cache Hit Found!\n");
                    }
                    return Menu;
                }
            }

            StringBuilder tmpS = new StringBuilder(4096);

            XmlNode n;
            if (ForParentEntityID == 0)
            {
                n = m_TblMgr.ResetToRootNode();
            }
            else
            {
                n = m_TblMgr.SetContext(ForParentEntityID);
            }

            if (ForParentEntityID == 0 && !m_TblMgr.HasChildren(n))
            {
                // NO ENTITIES IN THE DB, RETURN EMPTY STRING
            }
            else
            {
                if (ForParentEntityID == 0)
                {
                    tmpS.Append("<li " + CommonLogic.IIF(CommonLogic.QueryStringUSInt(m_EntitySpecs.m_EntityName + "ID") != 0, " expanded=1", "") + "><span>" + AppLogic.GetString("AppConfig.{0}PromptPlural".FormatWith(m_EntitySpecs.m_EntityName), LocaleSetting) + "</span>\n");
                }
                if (n != null && m_TblMgr.HasChildren(n))
                {
                    tmpS.Append("<ul>\n");
                    n = m_TblMgr.MoveFirstChild(n);
                    while (n != null)
                    {
                        int ThisID = m_TblMgr.CurrentID(n);
                        String URL = String.Empty;
                        if (AppLogic.IsAdminSite)
                        {
                            URL = String.Format("newentities.aspx?entityname={0}&{1}id={2}", m_EntitySpecs.m_EntityName, m_EntitySpecs.m_EntityName, ThisID.ToString());
                        }
                        else
                        {
                            URL = SE.MakeEntityLink(m_EntitySpecs.m_EntityName, ThisID, "");
                        }
                        tmpS.Append("<li " + CommonLogic.IIF(ThisID == SelectedID || m_TblMgr.ContainsChild(n, SelectedID), " expanded=1", "") + "><a " + CommonLogic.IIF(AppLogic.IsAdminSite, " target=\"content\"", "") + " href=\"" + XmlCommon.XmlEncodeAttribute(URL) + "\">" + m_TblMgr.CurrentName(n, LocaleSetting) + "</a>\n");
                        if (!AppLogic.AppConfigBool("Limit" + m_EntitySpecs.m_EntityName + "MenuToOneLevel") && m_TblMgr.HasChildren(n))
                        {
                            tmpS.Append(OpenCubeTree(ThisID, SkinID, LocaleSetting, SelectedID));
                        }
                        n = m_TblMgr.MoveNextSibling(n, false);
                    }
                    tmpS.Append("</ul>\n");
                }
                if (ForParentEntityID == 0)
                {
                    tmpS.Append("</li>\n");
                }
            }

            if (AppLogic.CachingOn)
            {
                HttpContext.Current.Cache.Insert(CacheName, tmpS.ToString(), null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }
            return tmpS.ToString();
        }

        public String ComponentArtTree(int ForParentEntityID, int SkinID, String LocaleSetting, int SelectedID)
        {
            String CacheName = String.Format("ComponentArtTree_{0}_{1}_{2}_{3}_{4}_{5}", m_EntitySpecs.m_EntityName, ForParentEntityID.ToString(), SkinID.ToString(), LocaleSetting, AppLogic.IsAdminSite.ToString(), SelectedID.ToString());
            if (AppLogic.CachingOn)
            {
                String Menu = (String)HttpContext.Current.Cache.Get(CacheName);
                if (Menu != null)
                {

                    if (CommonLogic.ApplicationBool("DumpSQL"))
                    {

                        HttpContext.Current.Response.Write("Cache Hit Found!\n");
                    }
                    return Menu;
                }
            }

            StringWriter tmpS = new StringWriter();

            if (m_TblMgr.XmlDoc.SelectNodes("//Entity").Count != 0)
            {
                String XslFile = "ComponentArtTree";
                XslCompiledTransform xForm;
                string XslFilePath = CommonLogic.SafeMapPath("EntityHelper/" + XslFile + ".xslt");
                xForm = (XslCompiledTransform)HttpContext.Current.Cache.Get(XslFilePath);
                if (xForm == null)
                {
                    xForm = new XslCompiledTransform(false);
                    xForm.Load(XslFilePath);
                    HttpContext.Current.Cache.Insert(XslFilePath, xForm, new CacheDependency(XslFilePath));
                }
                XsltArgumentList xslArgs = new XsltArgumentList();
                xslArgs.AddParam("entity", "", m_EntitySpecs.m_EntityName);
                xslArgs.AddParam("ForParentEntityID", "", ForParentEntityID);
                xslArgs.AddParam("entityDispName", "", XmlCommon.XmlEncodeAttribute(AppLogic.GetString("AppConfig." + m_EntitySpecs.m_EntityName + "PromptPlural", SkinID, LocaleSetting)));
                xslArgs.AddParam("adminsite", "", AppLogic.IsAdminSite);
                xslArgs.AddParam("custlocale", "", LocaleSetting);
                xslArgs.AddParam("deflocale", "", Localization.GetDefaultLocale());
                xslArgs.AddParam("expandID", "", SelectedID);
                xForm.Transform(m_TblMgr.XmlDoc, xslArgs, tmpS);
                if (AppLogic.AppConfigBool("XmlPackage.DumpTransform"))
                {
                    try // don't let logging crash the site
                    {
                        StreamWriter sw = File.CreateText(CommonLogic.SafeMapPath(String.Format("{0}images/{1}_{2}_{3}.xfrm.xml", CommonLogic.IIF(AppLogic.IsAdminSite, "../", ""), XslFile, m_EntitySpecs.m_EntityName, CommonLogic.IIF(AppLogic.IsAdminSite, "admin", "store"))));
                        sw.WriteLine(XmlCommon.PrettyPrintXml(tmpS.ToString()));
                        sw.Close();
                    }
                    catch { }
                }
            }

            if (AppLogic.CachingOn)
            {
                HttpContext.Current.Cache.Insert(CacheName, tmpS.ToString(), null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }
            return tmpS.ToString();

        }

        public String ComponentArtTreeEntityMenu(int ForParentEntityID, int SkinID, String LocaleSetting, int SelectedID)
        {
            String CacheName = String.Format("ComponentArtTreeEntityMenu_{0}_{1}_{2}_{3}_{4}_{5}", m_EntitySpecs.m_EntityName, ForParentEntityID.ToString(), SkinID.ToString(), LocaleSetting, AppLogic.IsAdminSite.ToString(), SelectedID.ToString());
            if (AppLogic.CachingOn)
            {
                String Menu = (String)HttpContext.Current.Cache.Get(CacheName);
                if (Menu != null)
                {

                    if (CommonLogic.ApplicationBool("DumpSQL"))
                    {

                        HttpContext.Current.Response.Write("Cache Hit Found!\n");
                    }
                    return Menu;
                }
            }

            StringWriter tmpS = new StringWriter();

            {
                String XslFile = "ComponentArtTreeEntityMenu";
                XslCompiledTransform xForm;
                string XslFilePath = CommonLogic.SafeMapPath("EntityHelper/" + XslFile + ".xslt");
                xForm = (XslCompiledTransform)HttpContext.Current.Cache.Get(XslFilePath);
                if (xForm == null)
                {
                    xForm = new XslCompiledTransform(false);
                    xForm.Load(XslFilePath);
                    HttpContext.Current.Cache.Insert(XslFilePath, xForm, new CacheDependency(XslFilePath));
                }
                XsltArgumentList xslArgs = new XsltArgumentList();
                xslArgs.AddParam("entity", "", m_EntitySpecs.m_EntityName);
                xslArgs.AddParam("ForParentEntityID", "", ForParentEntityID);
                xslArgs.AddParam("entityDispName", "", XmlCommon.XmlEncodeAttribute(AppLogic.GetString("AppConfig." + m_EntitySpecs.m_EntityName + "PromptPlural", SkinID, LocaleSetting)));
                xslArgs.AddParam("adminsite", "", AppLogic.IsAdminSite);
                xslArgs.AddParam("custlocale", "", LocaleSetting);
                xslArgs.AddParam("deflocale", "", Localization.GetDefaultLocale());
                xslArgs.AddParam("expandID", "", SelectedID);
                xForm.Transform(m_TblMgr.XmlDoc, xslArgs, tmpS);
                if (AppLogic.AppConfigBool("XmlPackage.DumpTransform"))
                {
                    try // don't let logging crash the site
                    {
                        StreamWriter sw = File.CreateText(CommonLogic.SafeMapPath(String.Format("{0}images/{1}_{2}_{3}.xfrm.xml", CommonLogic.IIF(AppLogic.IsAdminSite, "../", ""), XslFile, m_EntitySpecs.m_EntityName, CommonLogic.IIF(AppLogic.IsAdminSite, "admin", "store"))));
                        sw.WriteLine(XmlCommon.PrettyPrintXml(tmpS.ToString()));
                        sw.Close();
                    }
                    catch { }
                }
            }

            if (AppLogic.CachingOn)
            {
                HttpContext.Current.Cache.Insert(CacheName, tmpS.ToString(), null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }
            return tmpS.ToString();

        }

        public String PlainEntityList(int ForParentEntityID, int SkinID, String LocaleSetting)
        {
            String CacheName = String.Format("PlainEntityList_{0}_{1}_{2}_{3}_{4}", m_EntitySpecs.m_EntityName, ForParentEntityID.ToString(), SkinID.ToString(), LocaleSetting, AppLogic.IsAdminSite.ToString());
            if (AppLogic.CachingOn)
            {
                String Menu = (String)HttpContext.Current.Cache.Get(CacheName);
                if (Menu != null)
                {

                    if (CommonLogic.ApplicationBool("DumpSQL"))
                    {

                        HttpContext.Current.Response.Write("Cache Hit Found!\n");
                    }
                    return Menu;
                }
            }

            StringWriter tmpS = new StringWriter();
            String XslFile = "PlainEntityList";
            XslCompiledTransform xForm;
            string XslFilePath = CommonLogic.SafeMapPath("EntityHelper/" + XslFile + ".xslt");
            xForm = (XslCompiledTransform)HttpContext.Current.Cache.Get(XslFilePath);
            if (xForm == null)
            {
                xForm = new XslCompiledTransform(false);
                xForm.Load(XslFilePath);
                HttpContext.Current.Cache.Insert(XslFilePath, xForm, new CacheDependency(XslFilePath));
            }
            XsltArgumentList xslArgs = new XsltArgumentList();
            xslArgs.AddParam("entity", "", m_EntitySpecs.m_EntityName);
            xslArgs.AddParam("ForParentEntityID", "", ForParentEntityID);
            xslArgs.AddParam("ShowArrowInPlainList", "", AppLogic.AppConfigBool("ShowArrowInPlainList"));
            xslArgs.AddParam("imgSrc", "", AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/redarrow.gif"));
            xslArgs.AddParam("recurse", "", !AppLogic.AppConfigBool("LimitPlain" + m_EntitySpecs.m_EntityName + "ListToOneLevel"));
            xslArgs.AddParam("custlocale", "", LocaleSetting);
            xslArgs.AddParam("deflocale", "", Localization.GetDefaultLocale());
            xslArgs.AddParam("adminsite", "", AppLogic.IsAdminSite);
            xForm.Transform(m_TblMgr.XmlDoc, xslArgs, tmpS);
            if (AppLogic.AppConfigBool("XmlPackage.DumpTransform"))
            {
                try // don't let logging crash the site
                {
                    StreamWriter sw = File.CreateText(CommonLogic.SafeMapPath(String.Format("{0}images/{1}_{2}_{3}.xfrm.xml", CommonLogic.IIF(AppLogic.IsAdminSite, "../", ""), XslFile, m_EntitySpecs.m_EntityName, CommonLogic.IIF(AppLogic.IsAdminSite, "admin", "store"))));
                    sw.WriteLine(XmlCommon.PrettyPrintXml(tmpS.ToString()));
                    sw.Close();
                }
                catch { }
            }

            if (AppLogic.CachingOn)
            {

                HttpContext.Current.Cache.Insert(CacheName, tmpS.ToString(), null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }
            return tmpS.ToString();
        }

        public int GetNumEntityObjects(int EntityID, bool includeKits, bool includePacks)
        {
            return DB.GetSqlN("select count(*) as N from " + m_EntitySpecs.m_ObjectName + "  with (NOLOCK)  where Deleted=0 " + CommonLogic.IIF(m_OnlyPublishedEntitiesAndObjects, "and Published=1", "") + " " + CommonLogic.IIF(includeKits, "", " and IsAKit=0") + CommonLogic.IIF(includePacks, "", " and IsAPack=0") + " and " + m_EntitySpecs.m_ObjectName + "ID in (select distinct " + m_EntitySpecs.m_ObjectName + "ID from " + m_EntitySpecs.m_ObjectName + "" + m_EntitySpecs.m_EntityName + " where " + m_IDColumnName + "=" + EntityID.ToString() + ")");
        }

        public bool EntityHasVisibleObjects(int EntityID)
        {
            bool tmp = false;
            if (EntityID != 0)
            {
                XmlNode n = m_TblMgr.SetContext(EntityID);
                if (n != null)
                {
                    tmp = m_TblMgr.CurrentFieldInt(n, "Num" + m_EntitySpecs.m_ObjectNamePlural) > 0;
                }
            }
            return tmp;
        }

        public String GetAdvancedEntityBrowseBox(Customer ThisCustomer, int SkinID)
        {
            String s = m_EntitySpecs.m_EntityName.ToUpperInvariant();
            int SourceEntityID = CommonLogic.QueryStringUSInt(m_IDColumnName);

            // are we on a product page, or send product email page?
            string currentPage = CommonLogic.GetThisPageName(false).ToLowerInvariant();

            if (SourceEntityID == 0 && (currentPage == "showproduct.aspx" || currentPage == "emailproduct.aspx"))
            {
                String SourceEntity = CommonLogic.CookieCanBeDangerousContent("LastViewedEntityName", true).ToUpperInvariant();

                // only pull it from the same entity being created
                if (s == SourceEntity)
                {
                    SourceEntityID = CommonLogic.CookieUSInt("LastViewedEntityInstanceID");

                    // validate that visible node id is actually valid for this product:
                    if (SourceEntityID != 0)
                    {
                        String sqlx = "select count(*) as N from dbo.ProductEntity  with (NOLOCK)  where EntityType = " + DB.SQuote(SourceEntity) + " and ProductID=" + CommonLogic.QueryStringUSInt("ProductID").ToString() + " and EntityID=" + SourceEntityID.ToString();

                        if (DB.GetSqlN(sqlx) == 0) SourceEntityID = 0;  // visible node id is not valid for this product
                    }
                }
            }

            StringBuilder tmpS = new StringBuilder(4096);
            tmpS.Append("<div id=\"vertmenustyle1\">");
            tmpS.Append(this.GetEntityULList(0, ThisCustomer.LocaleSetting, ThisCustomer.AffiliateID, ThisCustomer.CustomerLevelID, true, false, true, "sitemapul", false, SourceEntityID, "&nbsp;&nbsp;"));
            tmpS.Append("</div>");
            return tmpS.ToString();
        }

        // returns the "next" Entity, after the specified Entity
        // "next" is defined as either the Entity that is next higher display order, or same display order and next highest alphabetical order
        public int GetNextEntity(int EntityID, bool Circular)
        {
            int tmp = 0;
            if (EntityID != 0)
            {
                XmlNode n = m_TblMgr.SetContext(EntityID);
                if (n != null)
                {
                    n = m_TblMgr.MoveNextSibling(n, Circular);
                    if (n != null)
                    {
                        tmp = m_TblMgr.CurrentID(n);
                    }
                }
            }
            return tmp;
        }

        // returns the "previous" Entity, after the specified Entity
        // "previous" is defined as either the Entity that is next lower display order, or same display order and next lowest alphabetical order
        public int GetPreviousEntity(int EntityID, bool Circular)
        {
            int tmp = 0;
            if (EntityID != 0)
            {
                XmlNode n = m_TblMgr.SetContext(EntityID);
                if (n != null)
                {
                    n = m_TblMgr.MovePreviousSibling(n, Circular);
                    if (n != null)
                    {
                        tmp = m_TblMgr.CurrentID(n);
                    }
                }
            }
            return tmp;
        }

        public String GetObjectEntities(int ObjectID, bool ForObjectBrowser)
        {
            String sql = "select " + m_IDColumnName + " from " + m_EntitySpecs.m_ObjectName + "" + m_EntitySpecs.m_EntityName + "  with (NOLOCK)  where " + m_EntitySpecs.m_ObjectName + "ID=" + ObjectID.ToString() + CommonLogic.IIF(ForObjectBrowser, " and " + m_IDColumnName + " in (select " + m_IDColumnName + " from " + m_EntitySpecs.m_EntityName + "  with (NOLOCK)  where Deleted=0 " + CommonLogic.IIF(m_OnlyPublishedEntitiesAndObjects, "and Published=1", "") + " and ShowIn" + m_EntitySpecs.m_ObjectName + "Browser<>0)", "");
            StringBuilder tmpS = new StringBuilder(1000);

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS(sql, dbconn))
                {
                    while (rs.Read())
                    {
                        if (tmpS.Length != 0)
                        {
                            tmpS.Append(",");
                        }
                        tmpS.Append(DB.RSFieldInt(rs, m_IDColumnName).ToString());
                    }
                }
            }
            return tmpS.ToString();
        }

        public static ArrayList GetProductEntityList(int ProductID, String EntityName)
        {
            ArrayList al = new ArrayList();
            if (EntityName.Length == 0)
            {
                EntityName = EntityDefinitions.readonly_CategoryEntitySpecs.m_EntityName;
            }

            string sql = "SELECT EntityID FROM dbo.ProductEntity WHERE EntityType = " + DB.SQuote(EntityName) + " and ProductID=" + ProductID + " ORDER BY DisplayOrder";

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS(sql, dbconn))
                {
                    while (rs.Read())
                    {
                        al.Add(DB.RSFieldInt(rs, "EntityID"));
                    }
                }
            }
            return al;
        }

        public static int GetProductsFirstEntity(int ProductID, String EntityName)
        {
            int tmp = 0;
            if (EntityName.Length == 0)
            {
                EntityName = EntityDefinitions.readonly_CategoryEntitySpecs.m_EntityName;
            }

            string sql = string.Format("select top 1 a.EntityID from productentity a with (nolock) inner join (select distinct a.entityid, a.EntityType from productentity a with (nolock) left join EntityStore b with (nolock) on a.EntityID = b.EntityID where ({0} = 0 or b.StoreID = {1})) b " +
                "on a.EntityID = b.EntityID and a.EntityType=b.EntityType where ProductID = {2} and a.EntityType = {3} ORDER BY DisplayOrder", CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowEntityFiltering") == true, 1, 0), AppLogic.StoreID(), ProductID, DB.SQuote(EntityName));

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS(sql, dbconn))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSFieldInt(rs, "EntityID");
                    }
                }
            }
            return tmp;
        }

    }
}
