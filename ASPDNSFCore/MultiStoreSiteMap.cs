// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Text;
using System.Xml;
using System.Data.SqlClient;
using System.Linq;
using System.Collections.Generic;

namespace AspDotNetStorefrontCore
{
    
    public class SiteMap
    {
        private static class Settings
        {
            internal static bool ShowCategories
            {
                get
                {
                    return AppLogic.IsAdminSite || AppLogic.AppConfigBool("SiteMap.ShowCategories");
                }
            }
            internal static bool ShowSections
            {
                get
                {
                    return AppLogic.IsAdminSite || AppLogic.AppConfigBool("SiteMap.ShowSections");
                }
            }
            internal static bool ShowLibraries
            {
                get
                {
                    return AppLogic.IsAdminSite || AppLogic.AppConfigBool("SiteMap.ShowLibraries");
                }
            }
            internal static bool ShowManufacturers
            {
                get
                {
                    return AppLogic.IsAdminSite || AppLogic.AppConfigBool("SiteMap.ShowManufacturers");
                }
            }
            internal static bool ShowTopics
            {
                get
                {
                    return AppLogic.IsAdminSite || AppLogic.AppConfigBool("SiteMap.ShowTopics");
                }
            }
            internal static bool ShowDocuments
            {
                get
                {
                    return AppLogic.IsAdminSite || AppLogic.AppConfigBool("SiteMap.ShowDocuments");
                }
            }
            internal static bool ShowProducts
            {
                get
                {
                    return AppLogic.IsAdminSite || AppLogic.AppConfigBool("SiteMap.ShowProducts");
                }
            }

            internal static bool ProductFiltering
            {
                get
                {
                    return AppLogic.GlobalConfigBool("AllowProductFiltering");
                }
            }
            internal static bool EntityFiltering
            {
                get
                {
                    return AppLogic.GlobalConfigBool("AllowEntityFiltering");
                }
            }
            internal static bool TopicFiltering
            {
                get
                {
                    return AppLogic.GlobalConfigBool("AllowTopicFiltering");
                }
            }
            
        }

        public SiteMap()
            :this(true)
        {}
        
        public SiteMap(bool _ShowCustomerService)
        {
            ShowCustomerService = _ShowCustomerService;
            mapDoc = new XmlDocument();
            BuildMap();
        }

        XmlDocument mapDoc;

        private string[] Entities
        {
            get
            {
                return new string[] {
                    "category",
                    "distributor",
                    "genre",
                    "manufacturer",
                    "section",
                    "vector",
                };
            }
        }

        private string CacheName
        {
            get
            {
                return string.Format("SiteMap_{0}", Customer.Current.LocaleSetting);
            }
        }
        private string _contents;
        public string Contents
        {
            get
            {
                if (AppLogic.CachingOn)
                {
                    return (string)HttpContext.Current.Cache[CacheName];
                }
                else
                {
                    return _contents;
                }
            }
            private set
            {
                if (AppLogic.CachingOn)
                {
                    HttpContext.Current.Cache[CacheName] = value;
                }
                else
                {
                    _contents = value;
                }
            }
        }

        private bool m_showcustomerservice;
        private bool ShowCustomerService
        {
            get { return m_showcustomerservice; }
            set { m_showcustomerservice = value; }
        }

        private XmlNode SiteMapRoot
        {
            get
            {
                return mapDoc.DocumentElement;
            }
        }

        private string getHeader(string group)
        {
            switch (group)
            {
                case "topic":
                    return "sitemap.aspx.2".StringResource();
                case "category":
                    return "admin.common.Categories".StringResource();
                case "manufacturer":
                    return "admin.common.Manufacturers".StringResource();
                case "section":
                    return "admin.common.Sections".StringResource();
                case "genre":
                    return "admin.common.Genres".StringResource();
            }
            return string.Empty;
        }
        
        private void BuildMap()
        {
            mapDoc.LoadXml("<SiteMap/>");

            if (SiteMap.Settings.ShowCategories)
            {
                writeEntity("category");
            }
            if (SiteMap.Settings.ShowManufacturers)
            {
                writeEntity("manufacturer");
            }
            if (SiteMap.Settings.ShowSections)
            {
                writeEntity("section");
            }
            writeEntity("genre");
            

            if (SiteMap.Settings.ShowTopics)
            {
                writeEntity("topic");
            }
            if (ShowCustomerService)
            {
                SiteMapRoot.AppendChild(CustomerServiceNode());
            }

            Contents = mapDoc.OuterXml;
        }

        private void writeEntity(String group)
        {
            SiteMapEntity[] entities;
            if (group.ToLowerInvariant() != "topic")
            {
                entities = NestedSiteMapEntity.GetEntities(group);
            }
            else
            {
                entities = SiteMapEntity.GetTopics(group);
            }
            if (entities.Length == 0)
            {
                return;
            }
            XmlElement cat = mapDoc.CreateElement("node");
            XmlAttribute txt = mapDoc.CreateAttribute("Text");
            txt.Value = getHeader(group);
            cat.Attributes.Append(txt);
            this.SiteMapRoot.AppendChild(cat);

            if (group.Equals("topic", StringComparison.OrdinalIgnoreCase))
            {
                foreach (SiteMapEntity ent in entities)
                {
                    cat.AppendChild(ent.ToSiteMapTopicNode(mapDoc));
                }
            }
            else
            {
                foreach (SiteMapEntity ent in entities)
                {
                    cat.AppendChild(ent.ToSiteMapNode(mapDoc));
                }
            }
        }

        private XmlNode CustomerServiceNode()
        {
            XmlNode cat = SiteMapNode(
                "menu.CustomerService".StringResource(),
                //AppLogic.ResolveUrl("~/t-service.aspx"),
                AppLogic.ResolveUrl(SE.MakeDriverLink("service")),
                mapDoc
                );
            
            
            cat.AppendChild(
                SiteMapNode(
                    "menu.YourAccount".StringResource(), 
                    AppLogic.ResolveUrl("~/account.aspx"), 
                    mapDoc
                ));

            cat.AppendChild(
                 SiteMapNode(
                     "menu.OrderHistory".StringResource(),
                     AppLogic.ResolveUrl("~/account.aspx"),
                     mapDoc
                 ));

            cat.AppendChild(
                 SiteMapNode(
                     "menu.PolicyReturns".StringResource(),
                     //AppLogic.ResolveUrl("~/t-returns.aspx"),
                     AppLogic.ResolveUrl(SE.MakeDriverLink("returns")),
                     mapDoc
                 ));

            cat.AppendChild(
                 SiteMapNode(
                     "menu.Shipping".StringResource(),
                     //AppLogic.ResolveUrl("~/t-shipping.aspx"),
                     AppLogic.ResolveUrl(SE.MakeDriverLink("shipping")),
                     mapDoc
                 ));

            cat.AppendChild(
                 SiteMapNode(
                     "menu.Contact".StringResource(),
                     AppLogic.ResolveUrl("~/contactus.aspx"),
                     mapDoc
                 ));

            cat.AppendChild(
                 SiteMapNode(
                     "menu.PolicyPrivacy".StringResource(),
                     //AppLogic.ResolveUrl("~/t-privacy.aspx"),
                     AppLogic.ResolveUrl(SE.MakeDriverLink("privacy")),
                     mapDoc
                 ));

            cat.AppendChild(
                 SiteMapNode(
                     "menu.PolicySecurity".StringResource(),
                     //AppLogic.ResolveUrl("~/t-security.aspx"),
                     AppLogic.ResolveUrl(SE.MakeDriverLink("security")),
                     mapDoc
                 ));
           
            return cat;

        }

        private static XmlNode SiteMapNode(string Text, string URL, XmlDocument context)
        {
            XmlElement eleNode = context.CreateElement("node");
            XmlAttribute atrText = context.CreateAttribute("Text");
            atrText.Value = Text;
            eleNode.Attributes.Append(atrText);
            XmlAttribute atrURL = context.CreateAttribute("NavigateUrl");
            atrURL.Value = URL;
            eleNode.Attributes.Append(atrURL);
            return eleNode;
        }

        internal class SiteMapProduct : SiteMapEntity
        {
            
            internal override string EntityType
            {
                get
                {
                    return "product";
                }
                set
                {
                    base.EntityType = value;
                }
            }

            private int m_mappingid;
            public int MappingID
            {
                get { return m_mappingid; }
                set { m_mappingid = value; }
            }
            private string m_mappingentity;
            public string MappingEntity
            {
                get { return m_mappingentity; }
                set { m_mappingentity = value; }
            }
        }

        internal class SiteMapEntity
        {
            private static string retrieveSQL
            {
                get
                {
                    return
@"SELECT [{0}ID] AS ID, [Name], [Name] as [SEName] FROM {0}
WHERE [{0}ID] IN (SELECT [EntityID] FROM EntityStore WHERE StoreID = @StoreID AND EntityType='{0}') OR @StoreID IS NULL AND ShowInSiteMap = 1";
                }
            }

            private int m_entityid;
            private string m_entitytype;
            private string m_name;
            private string m_sename;

            internal int EntityID
            {
                get { return m_entityid; }
                set { m_entityid = value; }
            }
            internal virtual string EntityType
            {
                get { return m_entitytype; }
                set { m_entitytype = value; }
            }
            internal string Name
            {
                get { return m_name; }
                set { m_name = value; }
            }
            internal string SEName
            {
                get { return m_sename; }
                set { m_sename = value; }
            }

            protected static SqlCommand getEntitySQL(string EntityType)
            {
                if (EntityType == "topic")
                {
                    throw new ArgumentException("Topic is not an entity.");
                }

                EntityType = EntityType.ToLowerInvariant();
                SqlCommand cmdGetEntities = new SqlCommand(string.Format(retrieveSQL, EntityType));
                cmdGetEntities.Parameters.Add(
                    new SqlParameter("@StoreID", DBNull.Value));

                return cmdGetEntities;
            }

            public virtual XmlNode ToSiteMapTopicNode(XmlDocument context)
            {
                String tName = XmlCommon.GetLocaleEntry(Name, Customer.Current.LocaleSetting, true);

                Topic t = new Topic(tName, Customer.Current.LocaleSetting);

                return SiteMapNode(t.SectionTitle, SE.MakeDriverLink(tName), context);
            }

            public virtual XmlNode ToSiteMapNode(XmlDocument context)
            {
                return SiteMapNode(Name, SE.MakeEntityLink(EntityType, EntityID, SEName), context);
            }

            internal static SiteMapEntity[] GetEntities(string EntityType)
            {
                List<SiteMapEntity> _list = new List<SiteMapEntity>();

                SqlCommand getCommand = getEntitySQL(EntityType);
                Action<System.Data.IDataReader> readEntities = rd =>
                {
                    while (rd.Read())
                    {
                        SiteMapEntity entity = new SiteMapEntity();
                        entity.EntityID = rd.FieldInt("ID");
                        entity.Name = XmlCommon.GetLocaleEntry(rd.Field("Name"), Customer.Current.LocaleSetting, false);
                        entity.SEName = rd.Field("SEName");
                        entity.EntityType = EntityType;
                        _list.Add(entity);
                    }
                };
                DB.UseDataReader(getCommand, readEntities);
                return _list.ToArray();
            }

            internal static SiteMapEntity[] GetTopics(string group)
            {
                List<SiteMapEntity> _list = new List<SiteMapEntity>();
                SqlCommand getCommand = new SqlCommand("select * from Topic where Deleted = 0 and Published=1 and ShowInSiteMap = 1 "+CommonLogic.IIF(SiteMap.Settings.TopicFiltering, " and (storeid = 0 or storeid ="+AppLogic.StoreID()+")", "")+" order by storeid desc, DisplayOrder desc, name");
                Action<System.Data.IDataReader> readTopics = rd =>
                {
                    while (rd.Read())
                    {
                        SiteMapEntity entity = new SiteMapEntity();
                        entity.EntityID = rd.FieldInt("TopicID");
                        entity.Name = XmlCommon.GetLocaleEntry(rd.Field("Name"), Customer.Current.LocaleSetting, false);
                        entity.SEName = XmlCommon.GetLocaleEntry(rd.Field("Name"), Customer.Current.LocaleSetting, false);
                        entity.EntityType = "topic";
                        if (_list.FirstOrDefault(t => t.Name == entity.Name) == null)
                        {
                            _list.Add(entity);
                        }
                    }
                };
                DB.UseDataReader(getCommand, readTopics);

                return _list.ToArray();
            }

            private static SqlCommand getTopicSQL()
            {
                throw new NotImplementedException();
            }
        }

        internal class NestedSiteMapEntity : SiteMapEntity
        {
            private static string retrieveSQL
            {
                get
                {
                    return
@"SELECT [{0}ID] AS ID, [Name], [SEName], [Parent{0}ID] AS ParentID FROM {0}
WHERE ([{0}ID] IN (SELECT [EntityID] FROM EntityStore WHERE StoreID = @StoreID AND EntityType='{0}') OR @StoreID IS NULL) AND Published = 1 AND Deleted = 0 ORDER BY DisplayOrder";
                }
            }
            private SqlCommand cmdGetProduct()
            {

                SqlCommand xCmd = new SqlCommand(string.Format(
@"SELECT prod.ProductID, [Name], [SEName] 
FROM Product AS prod 
INNER JOIN Product{0} AS pm ON pm.ProductID = prod.ProductID
WHERE pm.{0}ID = @MapID AND (
Prod.ProductID IN (SELECT ProductID FROM ProductStore WHERE StoreID = @StoreID) OR @StoreID IS NULL) 
AND Published = 1 and deleted = 0
ORDER BY DisplayOrder
"
                    , EntityType));
                xCmd.Parameters.Add(new SqlParameter("@MapID", EntityID));
                xCmd.Parameters.Add(new SqlParameter("@StoreID", DBNull.Value));
                return xCmd;
            }
            
            protected new static SqlCommand getEntitySQL(string EntityType)
            {
                EntityType = EntityType.ToLowerInvariant();
                SqlCommand cmdGetEntities = new SqlCommand(string.Format(retrieveSQL, EntityType));
                cmdGetEntities.Parameters.Add(
                    new SqlParameter("@StoreID", DBNull.Value));

                if (
                    SiteMap.Settings.EntityFiltering && (
                    EntityType == "category" ||
                    EntityType == "manufacturer" ||
                    EntityType == "section" ||
                    EntityType == "vector" ||
                    EntityType == "genre" ||
                    EntityType == "distributor")
                )
                {
                    cmdGetEntities.Parameters["@StoreID"].Value = AppLogic.StoreID();
                }

                return cmdGetEntities;
            }
            public new static NestedSiteMapEntity[] GetEntities(string EntityType)
            {
                Dictionary<int, NestedSiteMapEntity> _list = new Dictionary<int, NestedSiteMapEntity>();

                SqlCommand getCommand = getEntitySQL(EntityType);
                Action<System.Data.IDataReader> readEntities = rd =>
                {
                    while (rd.Read())
                    {
                        NestedSiteMapEntity entity = new NestedSiteMapEntity();
                        entity.EntityID = rd.FieldInt("ID");
                        entity.Name = XmlCommon.GetLocaleEntry(rd.Field("Name"), Customer.Current.LocaleSetting, false);
                        entity.SEName = rd.Field("SEName");
                        entity.ParentEntityID = rd.FieldInt("ParentID");
                        entity.EntityType = EntityType;
                        entity.GetProducts();
                        _list.Add(entity.EntityID, entity);
                    }
                };
                DB.UseDataReader(getCommand, readEntities);


                return OrganizeEntities(_list).ToArray();
            }

            private static List<NestedSiteMapEntity> OrganizeEntities(Dictionary<int, NestedSiteMapEntity> entities)
            {
                foreach (NestedSiteMapEntity ent in
                    entities.Values.Where(e => e.ParentEntityID != 0))
                {
                    if (entities.ContainsKey(ent.ParentEntityID) && entities[ent.ParentEntityID] != null)
                    {
                        List<NestedSiteMapEntity> _children = new List<NestedSiteMapEntity>(entities[ent.ParentEntityID].Children);
                        _children.Add(ent);
                        entities[ent.ParentEntityID].Children = _children.ToArray();
                    }
                }
                return new List<NestedSiteMapEntity>(entities.Values.Where(e => e.ParentEntityID == 0));

            }
            public NestedSiteMapEntity()
            {
                Children = new NestedSiteMapEntity[] { };
                Products = new SiteMapProduct[] { };
            }

            private int m_parententityid;
            int ParentEntityID
            {
                get { return m_parententityid; }
                set { m_parententityid = value; }
            }

            internal NestedSiteMapEntity[] m_children;
            NestedSiteMapEntity[] Children
            {
                get { return m_children; }
                set { m_children = value; }
            }

            private SiteMapProduct[] m_products;
            internal SiteMapProduct[] Products
            {
                get { return m_products; }
                set { m_products = value; }
            }

            internal void GetProducts()
            {
                SqlCommand retCmd = cmdGetProduct();
                List<SiteMapProduct> xList = new List<SiteMapProduct>();
                if (SiteMap.Settings.ProductFiltering)
                {
                    retCmd.Parameters["@StoreID"].Value = AppLogic.StoreID();
                }

                Action<System.Data.IDataReader> readEntities = rd =>
                {
                    while (rd.Read())
                    {
                        SiteMapProduct prd = new SiteMapProduct();
                        prd.EntityID = rd.FieldInt("ProductID");
                        prd.MappingEntity = this.EntityType;
                        prd.Name = XmlCommon.GetLocaleEntry(rd.Field("Name"), Customer.Current.LocaleSetting, false);
                        prd.SEName = rd.Field("SEName");
                        xList.Add(prd);
                    }
                };
                DB.UseDataReader(retCmd, readEntities);
                Products = xList.ToArray();
            }

            public override XmlNode ToSiteMapNode(XmlDocument context)
            {
                XmlNode node = SiteMapNode(Name, SE.MakeEntityLink(EntityType, EntityID, SEName), context);

                if (SiteMap.Settings.ShowProducts)
                {
                    foreach (SiteMapProduct prod in Products)
                    {
                        node.AppendChild(prod.ToSiteMapNode(context));
                    }
                    foreach (NestedSiteMapEntity ent in Children)
                    {
                        node.AppendChild(ent.ToSiteMapNode(context));
                    }
                }
                return node;
            }
        }

        
    }

    public static class GoogleSiteMap
    {
        #region google entities
        public static string GetGoogleEntitySiteMap(string entityType)
        {
            SiteMap.NestedSiteMapEntity[] entityMap = SiteMap.NestedSiteMapEntity.GetEntities(entityType);
            return GoogleSiteMapNodes(entityMap);
        }

        private static string GoogleSiteMapNodes(SiteMap.NestedSiteMapEntity[] MapEntities)
        {
            StringBuilder sb = new StringBuilder();
            foreach (SiteMap.NestedSiteMapEntity nse in MapEntities)
            {
                sb.Append(GoogleSiteMapEntityXML(nse.EntityID, nse.EntityType));
                sb.Append(GoogleSiteMapNodes(nse.m_children));
            }
            return sb.ToString();
        }

        private static string GoogleSiteMapEntityXML(int EntityID, string EntityType)
        {
            return string.Format("<sitemap><loc>" + AppLogic.GetStoreHTTPLocation(false) + "googleentity.aspx?entityname={0}&amp;entityid={1}</loc></sitemap>\n", EntityType, EntityID.ToString());
        } 
        #endregion

        public static string GetGoogleEntityProductURLNodes(string entityType, int EntityID)
        {
            SiteMap.NestedSiteMapEntity entity = new SiteMap.NestedSiteMapEntity();
            StringBuilder sb = new StringBuilder();
            entity.EntityType = entityType;
            entity.EntityID = EntityID;
            entity.GetProducts();
            foreach (SiteMap.SiteMapProduct smp in entity.Products)
            {
                sb.Append(GoogleProductXML(smp));
            }
            return sb.ToString();
        }

        private static string GoogleProductXML(SiteMap.SiteMapProduct product)
        {
            string link = SE.MakeProductLink(product.EntityID, product.SEName);
            if (!link.StartsWith("/"))
                link = "/" + link;
            return string.Format("<url><loc>" + AppLogic.GetStoreHTTPLocation(false, false) + "{0}</loc><changefreq>{1}</changefreq><priority>{2}</priority></url>\n", link, AppLogic.AppConfig("GoogleSiteMap.ObjectChangeFreq"), AppLogic.AppConfig("GoogleSiteMap.ObjectPriority"));
        }
    }
}
