// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web.UI;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Xml.XPath;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;

namespace AspDotNetStorefrontCore
{
    public interface ILinkItem
    {
        int ID { get; set; }
        string Name { get; set; }
        string Url { get; set; }
        ILinkItem Parent { get; set; }
        LinkItemCollection ChildItems { get; }
    }

    public abstract class EntityHierarchyData : ILinkItem
    {
        public abstract int ID { get; set; }

        public abstract string Name { get; set; }

        public abstract string Url { get; set; }

        public virtual ILinkItem Parent { get; set; }

        public abstract LinkItemCollection ChildItems { get; }

        public IHierarchicalEnumerable GetChildren()
        {
            return this.ChildItems;
        }

        public IHierarchyData GetParent()
        {
            return this.Parent as IHierarchyData;
        }

        public bool HasChildren
        {
            get { return ChildItems.Count > 0; }
        }

        public object Item
        {
            get { return this; }
        }

        public string Path
        {
            get { return this.ID.ToString(); }
        }

        public virtual string Type
        {
            get { return this.GetType().ToString(); }
            set { }
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
    
    public class ProductMappingLinkItem : EntityHierarchyData
    {
        public static int ROOT_ID = 0;

        private string _url = string.Empty;
        
        public override int ID { get; set; }
        public bool Selected { get; set; }

        public override string Url
        {
            get
            {
                if (AppLogic.IsAdminSite)
                {
                    if (_url == string.Empty)
                    {
                        return string.Format("entity.aspx?entityname={0}&entityid={1}", this.Type, this.ID);
                    }
                    else
                    {
                        return _url;
                    }
                }
                else
                {
                    return SE.MakeEntityLink(this.Type, this.ID, this.SEName);
                }
            }
            set
            {
                _url = value;
            }

        }
        
        public override string Name { get; set; }
        
        public string SEName { get; set; }
        
        public override string Type { get; set; }
        
        public int ParentEntityID { get; set; }
        
        public int DisplayOrder { get; set; }

        internal LinkItemCollection _children = new LinkItemCollection();

        /// <summary>
        /// Gets the child entities.
        /// </summary>
        /// <value>The child entities.</value>
        public override LinkItemCollection ChildItems
        {
            get
            {
                return _children;
            }
        }

        public static ProductMappingLinkItem Find(int id, string type)
        {
            return Find(type, id);
        }

        /// <summary>
        /// Finds the specified node id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="type">The entity type.</param>
        /// <param name="locale">The locale.</param>
        /// <returns></returns>
        public static ProductMappingLinkItem Find(string type, int id)
        {
            ProductMappingLinkItem found = null;

            Action<IDataReader> readAction = (rs) =>
            {
                while (rs.Read())
                {
                    found = new ProductMappingLinkItem();
                    found.Name = XmlCommon.GetLocaleEntry(rs.Field("Name"), Customer.Current.LocaleSetting, true);
                    found.SEName = rs.Field("SEName");
                    found.Type = rs.Field("Type");
                    found.ParentEntityID = rs.FieldInt("ParentEntityID");
                    found.DisplayOrder = rs.FieldInt("DisplayOrder");
                    found.ID = rs.FieldInt("ID");
                }
            };

            string query = string.Empty;
            switch (type.ToLowerInvariant())
            {
                case "section":
                    query = string.Format("SELECT A.SectionID ID, Name, SEName, 'Section' [Type], ParentSectionID ParentEntityID, DisplayOrder FROM Section A WITH (NOLOCK) INNER JOIN (SELECT DISTINCT " +
                        "SectionID EntityID FROM Section A WITH (NOLOCK) LEFT JOIN EntityStore B WITH (NOLOCK) ON A.SectionID = B.EntityID AND EntityType = 'Section' WHERE ({0} = 0 or StoreID = {1})) B " +
                        "ON A.SectionID = B.EntityID  WHERE SectionID = {2} ORDER BY DisplayOrder, Name ASC", CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowEntityFiltering") == true, 1, 0), AppLogic.StoreID(), id);
                    break;

                case "category":
                    query = string.Format("SELECT A.CategoryID ID, Name, SEName, 'Category' [Type], ParentCategoryID ParentEntityID, DisplayOrder FROM Category A WITH (NOLOCK) INNER JOIN (SELECT DISTINCT " +
                        "CategoryID EntityID FROM Category A WITH (NOLOCK) LEFT JOIN EntityStore B WITH (NOLOCK) ON A.CategoryID = B.EntityID AND EntityType = 'Category' WHERE ({0} = 0 or StoreID = {1})) B " +
                        "ON A.CategoryID = B.EntityID  WHERE CategoryID = {2} ORDER BY DisplayOrder, Name ASC", CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowEntityFiltering") == true, 1, 0), AppLogic.StoreID(), id);
                    break;

                case "manufacturer":
                    query = string.Format("SELECT A.ManufacturerID ID, Name, SEName, 'Manufacturer' [Type], ParentManufacturerID ParentEntityID, DisplayOrder FROM Manufacturer A WITH (NOLOCK) INNER JOIN (SELECT " +
                        "DISTINCT ManufacturerID EntityID FROM Manufacturer A WITH (NOLOCK) LEFT JOIN EntityStore B WITH (NOLOCK) ON A.ManufacturerID = B.EntityID AND EntityType = 'Manufacturer' WHERE ({0} = 0 " +
                        "or StoreID = {1})) B ON A.ManufacturerID = B.EntityID  WHERE ManufacturerID = {2} ORDER BY DisplayOrder, Name ASC", CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowEntityFiltering") == true, 1, 0), AppLogic.StoreID(), id);
                    break;

                case "vector":
                    query = string.Format("SELECT A.VectorID ID, Name, SEName, 'Vector' [Type], ParentVectorID ParentEntityID, DisplayOrder FROM Vector A WITH (NOLOCK) INNER JOIN (SELECT DISTINCT " +
                        "VectorID EntityID FROM Vector A WITH (NOLOCK) LEFT JOIN EntityStore B WITH (NOLOCK) ON A.VectorID = B.EntityID AND EntityType = 'Vector' WHERE ({0} = 0 or StoreID = {1})) B " +
                        "ON A.VectorID = B.EntityID  WHERE VectorID = {2} ORDER BY DisplayOrder, Name ASC", CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowEntityFiltering") == true, 1, 0), AppLogic.StoreID(), id);
                    break;

                case "genre":
                    query = string.Format("SELECT A.GenreID ID, Name, SEName, 'Genre' [Type], ParentGenreID ParentEntityID, DisplayOrder FROM Genre A WITH (NOLOCK) INNER JOIN (SELECT DISTINCT " +
                        "GenreID EntityID FROM Genre A WITH (NOLOCK) LEFT JOIN EntityStore B WITH (NOLOCK) ON A.GenreID = B.EntityID AND EntityType = 'Genre' WHERE ({0} = 0 or StoreID = {1})) B " +
                        "ON A.GenreID = B.EntityID  WHERE GenreID = {2} ORDER BY DisplayOrder, Name ASC", CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowEntityFiltering") == true, 1, 0), AppLogic.StoreID(), id);
                    break;

                case "distributor":
                    query = string.Format("SELECT A.DistributorID ID, Name, SEName, 'Distributor' [Type], ParentDistributorID ParentEntityID, DisplayOrder FROM Distributor A WITH (NOLOCK) INNER JOIN (SELECT " +
                        "DISTINCT DistributorID EntityID FROM Distributor A WITH (NOLOCK) LEFT JOIN EntityStore B WITH (NOLOCK) ON A.DistributorID = B.EntityID AND EntityType = 'Distributor' WHERE ({0} = 0 " +
                        "or StoreID = {1})) B ON A.DistributorID = B.EntityID  WHERE DistributorID = {2} ORDER BY DisplayOrder, Name ASC", CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowEntityFiltering") == true, 1, 0), AppLogic.StoreID(), id);
                    break;
            }

            if (query.Length > 0)
            {
                DB.UseDataReader(query, readAction);
            }

            return found;
        }

        private LinkItemCollection _products = new LinkItemCollection();

        public LinkItemCollection Products
        {
            get
            {
                return _products;
            }
        }

        public void LoadChildren()
        {
            _children = LinkItemCollection.GetChildren(this);
        }

    }

    //[Table(Name = "Topic")]
    //public class TopicLinkItem : EntityHierarchyData
    //{
    //    private string _url = string.Empty;

        
    //    public override int ID { get; set; }
        
    //    public override string Name { get; set; }
        
    //    public string Title { get; set; }
        
    //    public bool ShowInSiteMap { get; set; }
        
    //    public bool Deleted { get; set; }
        
    //    public int DisplayOrder { get; set; }

    //    public override string Url
    //    {
    //        get
    //        {
    //            return SE.MakeDriverLink(XmlCommon.GetLocaleEntry(this.Name, Localization.GetWebConfigLocale(), false));
    //        }
    //        set
    //        {
    //            _url = value;
    //        }
    //    }

    //    LinkItemCollection _childItems = new LinkItemCollection();

    //    public override LinkItemCollection ChildItems
    //    {
    //        get
    //        {
    //            return _childItems;
    //        }
    //    }

    //    public static TopicLinkItem Root()
    //    {
    //        TopicLinkItem root = new TopicLinkItem();
    //        root.Name = "Topic";
    //        root._childItems = LinkItemCollection.GetTopics(root);

    //        return root;
    //    }

    //}

   
    public class StaticEntity : EntityHierarchyData
    {
        private LinkItemCollection _childItems = new LinkItemCollection();
        private string _url = string.Empty;

        public StaticEntity(string name, string url)
        {
            this.Name = name;
            _url = url;
        }

        public override int ID
        {
            get
            {
                return 0;
            }
            set
            {
            }
        }

        public override string Name { get; set; }

        public override string Url
        {
            get { return _url; }
            set { _url = value; }
        }

        public override LinkItemCollection ChildItems
        {
            get { return _childItems; }
        }

    }

    public class LinkItemCollection : List<ILinkItem>, IHierarchicalEnumerable
    {

        #region IHierarchicalEnumerable Members

        public IHierarchyData GetHierarchyData(object enumeratedItem)
        {
            return enumeratedItem as IHierarchyData;
        }

        #endregion

        #region From Sitemap

        public string XMLPackage { get; set; }

        private const int DEFAULT_SKINID = 1;

        private static XPathNavigator GetMenuXml(string xmlPackageName)
        {
            Customer thisCustomer = Customer.Current;
            string menuXml = AppLogic.RunXmlPackage(xmlPackageName, null, thisCustomer, DEFAULT_SKINID, string.Empty, string.Empty, false, false);

            if (CommonLogic.IsStringNullOrEmpty(menuXml))
            {
                throw new InvalidOperationException("Menu xml cannot be empty string!!!");
            }

            XmlDocument doc = new XmlDocument();

            try
            {
                Regex pattern = new Regex(@"\(!(.*?)!\)");
                MatchEvaluator findAndReplaceStringResourced = new MatchEvaluator(StringResourceMatch);

                menuXml = pattern.Replace(menuXml, findAndReplaceStringResourced);
                doc.LoadXml(menuXml);
            }
            catch { throw new InvalidOperationException("Invalid Menu Xml!!!"); }

            return doc.CreateNavigator();
        }

        private static string StringResourceMatch(Match match)
        {
            String l = match.Groups[1].Value;
            string s = HttpUtility.HtmlEncode(AppLogic.GetString(l, DEFAULT_SKINID, Customer.Current.LocaleSetting));
            if (s == null || s.Length == 0 || s == l)
            {
                s = match.Value;
            }
            return XmlCommon.XmlEncode(s);
        }

        #endregion

        //public static LinkItemCollection GetTopics()
        //{
        //    return GetTopics(null);
        //}

        //public static LinkItemCollection GetTopics(TopicLinkItem root)
        //{
        //    LinkItemCollection all = new LinkItemCollection();

        //    Action<DataContext> queryContext = db => 
        //    {
        //        Table<TopicLinkItem> entities = db.GetTable<TopicLinkItem>();
        //        var query =
        //            from e in entities
        //            //where e.Deleted == 0 && e.ShowInSiteMap == 1                
        //            where e.Deleted == false && e.ShowInSiteMap == true
        //            orderby e.DisplayOrder, e.Name ascending
        //            select e;

        //        foreach (var e in query)
        //        {
        //            e.Parent = root;
        //            all.Add(e);
        //        }
        //    };

        //    DB.UseDataContext(queryContext);

        //    return all;
        //}

        public static string NoSearchFilter = string.Empty;
        public static int ReturnAllRecords = 0;

        /// <summary>
        /// Gets all first level entities.
        /// </summary>
        /// <param name="type">The entity type.</param>
        /// <param name="locale">The customer locale.</param>
        /// <returns></returns>
        public static LinkItemCollection GetAllFirstLevel(string type, int top, string locale)
        {
            LinkItemCollection all = new LinkItemCollection();       

            Action<IDataReader> readAction = (rs) =>
            {
                while (rs.Read())
                {
                    ProductMappingLinkItem entity = new ProductMappingLinkItem();
                    entity.Name = XmlCommon.GetLocaleEntry(rs.Field("Name"), Customer.Current.LocaleSetting, true);
                    entity.SEName = rs.Field("SEName");
                    entity.Type = rs.Field("Type");
                    entity.ParentEntityID = rs.FieldInt("ParentEntityID");
                    entity.DisplayOrder = rs.FieldInt("DisplayOrder");
                    entity.ID = rs.FieldInt("ID");
                    all.Add(entity);
                }
            };

            string topQuery = string.Empty;
            if (top != ReturnAllRecords)
            {
                topQuery = "TOP {0}".FormatWith(top);
            }

            string query = string.Empty;
            switch (type.ToLowerInvariant())
            {
                case "section":
                    query = string.Format("SELECT {2} A.SectionID ID, Name, SEName, 'Section' [Type], ParentSectionID ParentEntityID, DisplayOrder FROM Section A WITH (NOLOCK) INNER JOIN (SELECT DISTINCT SectionID " +
                        "EntityID FROM Section A WITH (NOLOCK) LEFT JOIN EntityStore B WITH (NOLOCK) ON A.SectionID = B.EntityID AND EntityType = 'Section' WHERE ({0} = 0 or StoreID = {1})) B ON A.SectionID = B.EntityID " +
                        "WHERE ParentSectionID = 0" + CommonLogic.IIF(AppLogic.IsAdminSite, String.Empty, "and Published=1 and Deleted=0") + " order by DisplayOrder, Name asc", CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowEntityFiltering") == true, 1, 0), AppLogic.StoreID(), topQuery);
                    break;

                case "category":
                    query = string.Format("SELECT {2} A.CategoryID ID, Name, SEName, 'Category' [Type], ParentCategoryID ParentEntityID, DisplayOrder FROM Category A WITH (NOLOCK) INNER JOIN " +
                        "(SELECT DISTINCT CategoryID EntityID FROM Category A WITH (NOLOCK) LEFT JOIN EntityStore B WITH (NOLOCK) ON A.CategoryID = B.EntityID AND EntityType = 'Category' " +
                        "WHERE ({0} = 0 or StoreID = {1})) B ON A.CategoryID = B.EntityID WHERE ParentCategoryID = 0 " + CommonLogic.IIF(AppLogic.IsAdminSite, String.Empty, "and Published=1 and Deleted=0") + " order by DisplayOrder, Name asc", CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowEntityFiltering") == true, 1, 0), AppLogic.StoreID(), topQuery);
                    break;

                case "manufacturer":
                    query = string.Format("SELECT {2} A.ManufacturerID ID, Name, SEName, 'Manufacturer' [Type], ParentManufacturerID ParentEntityID, DisplayOrder FROM Manufacturer A WITH (NOLOCK) " +
                        "INNER JOIN (SELECT DISTINCT ManufacturerID EntityID FROM Manufacturer A WITH (NOLOCK) LEFT JOIN EntityStore B WITH (NOLOCK) ON A.ManufacturerID = B.EntityID AND EntityType = 'Manufacturer' " +
                        "WHERE ({0} = 0 or StoreID = {1})) B ON A.ManufacturerID = B.EntityID WHERE ParentManufacturerID = 0 " + CommonLogic.IIF(AppLogic.IsAdminSite, String.Empty, "and Published=1 and Deleted=0") + " order by DisplayOrder, Name asc", CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowEntityFiltering") == true, 1, 0), AppLogic.StoreID(), topQuery);
                    break;

                case "vector":
                    query = string.Format("SELECT {2} A.VectorID ID, Name, SEName, 'Vector' [Type], ParentVectorID ParentEntityID, DisplayOrder FROM Vector A WITH (NOLOCK) INNER JOIN (SELECT DISTINCT VectorID " +
                        "EntityID FROM Vector A WITH (NOLOCK) LEFT JOIN EntityStore B WITH (NOLOCK) ON A.VectorID = B.EntityID AND EntityType = 'Vector' WHERE ({0} = 0 or StoreID = {1})) B ON A.VectorID = B.EntityID " +
                        "WHERE ParentVectorID = 0" + CommonLogic.IIF(AppLogic.IsAdminSite, String.Empty, "and Published=1 and Deleted=0") + " order by DisplayOrder, Name asc", CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowEntityFiltering") == true, 1, 0), AppLogic.StoreID(), topQuery);
                    break;

                case "genre":
                    query = string.Format("SELECT {2} A.GenreID ID, Name, SEName, 'Genre' [Type], ParentGenreID ParentEntityID, DisplayOrder FROM Genre A WITH (NOLOCK) INNER JOIN " +
                        "(SELECT DISTINCT GenreID EntityID FROM Genre A WITH (NOLOCK) LEFT JOIN EntityStore B WITH (NOLOCK) ON A.GenreID = B.EntityID AND EntityType = 'Genre' " +
                        "WHERE ({0} = 0 or StoreID = {1})) B ON A.GenreID = B.EntityID WHERE ParentGenreID = 0 " + CommonLogic.IIF(AppLogic.IsAdminSite, String.Empty, "and Published=1 and Deleted=0") + " order by DisplayOrder, Name asc", CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowEntityFiltering") == true, 1, 0), AppLogic.StoreID(), topQuery);
                    break;

                case "distributor":
                    query = string.Format("SELECT {2} A.DistributorID ID, Name, SEName, 'Distributor' [Type], ParentDistributorID ParentEntityID, DisplayOrder FROM Distributor A WITH (NOLOCK) " +
                        "INNER JOIN (SELECT DISTINCT DistributorID EntityID FROM Distributor A WITH (NOLOCK) LEFT JOIN EntityStore B WITH (NOLOCK) ON A.DistributorID = B.EntityID AND EntityType = 'Distributor' " +
                        "WHERE ({0} = 0 or StoreID = {1})) B ON A.DistributorID = B.EntityID WHERE ParentDistributorID = 0 " + CommonLogic.IIF(AppLogic.IsAdminSite, String.Empty, "and Published=1 and Deleted=0") + " order by DisplayOrder, Name asc", CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowEntityFiltering") == true, 1, 0), AppLogic.StoreID(), topQuery);
                    break;
            }

            if (query.Length > 0)
            {
                DB.UseDataReader(query, readAction);
            }

            return all;
        }

        public static LinkItemCollection BuildFrom(int id, string type)
        {
            ProductMappingLinkItem node = ProductMappingLinkItem.Find(id, type);
            if (node != null)
            {
                return BuildFrom(node, type);
            }

            // return empty collection
            return new LinkItemCollection();
        }

        public static LinkItemCollection BuildFrom(ProductMappingLinkItem node, string type)
        {
            node.Selected = true;
            BuildTree(node);

            // find the rootest                 
            while (node.Parent != null)
            {
                node = node.Parent as ProductMappingLinkItem;
            }

            // and start the reference to it
            return node.ChildItems;
        }

        /// <summary>
        /// Builds the entity tree.
        /// </summary>
        /// <param name="current">The current node.</param>
        private static void BuildTree(ProductMappingLinkItem current)
        {
            ProductMappingLinkItem parent = null;
            string queryTree = string.Format("aspdnsf_GetEntityTree @entity = {0}, @entityid = {1}, @storeid = {2}, @filterentity = {3}", DB.SQuote(current.Type), current.ID, AppLogic.StoreID(), AppLogic.GlobalConfigBool("AllowEntityFiltering"));

            using (SqlConnection con = DB.dbConn())
            {
                con.Open();
                using (IDataReader rs = DB.GetRS(queryTree, con))
                {
                    // parent
                    if (rs.Read())
                    {
                        parent = new ProductMappingLinkItem();
                        parent.Type = current.Type;
                        parent.ID = DB.RSFieldInt(rs, "ID");
                        parent.Name = DB.RSFieldByLocale(rs, "Name", Customer.Current.LocaleSetting);
                    }
                    else
                    {
                        ProductMappingLinkItem root = new ProductMappingLinkItem();
                        root.ID = ProductMappingLinkItem.ROOT_ID;
                        root.Name = "Root";
                        root.Type = current.Type;

                        parent = root;
                    }

                    // sibling
                    if (rs.NextResult())
                    {
                        while (rs.Read())
                        {
                            int id = DB.RSFieldInt(rs, "ID");
                            if (current.ID == id)
                            {
                                current.Parent = parent;
                                parent.Selected = true;
                                parent.ChildItems.Add(current);
                            }
                            else
                            {
                                ProductMappingLinkItem node = new ProductMappingLinkItem();

                                node.Type = current.Type;
                                node.ID = id;
                                node.Name = DB.RSFieldByLocale(rs, "Name", Customer.Current.LocaleSetting);
                                if (string.IsNullOrEmpty(node.Name))
                                {
                                    node.Name = DB.RSField(rs, "Name");
                                }
                                node.Parent = parent;

                                parent.ChildItems.Add(node);
                            }
                        }
                    }
                }
            }

            if (parent != null && parent.ID != ProductMappingLinkItem.ROOT_ID)
            {
                BuildTree(parent);
            }
        }

        internal static LinkItemCollection GetChildren(ProductMappingLinkItem parent)
        {
            LinkItemCollection children = new LinkItemCollection();


            Action<IDataReader> readAction = (rs) =>
           {
               while (rs.Read())
               {
                   ProductMappingLinkItem child = new ProductMappingLinkItem();
                   child.Name = XmlCommon.GetLocaleEntry(rs.Field("Name"), Customer.Current.LocaleSetting, true);
                   child.Parent = parent;
                   child.Type = rs.Field("Type");
                   child.SEName = rs.Field("SEName");
                   child.ParentEntityID = rs.FieldInt("ParentEntityID");
                   child.DisplayOrder = rs.FieldInt("DisplayOrder");
                   child.ID = rs.FieldInt("ID");
                   children.Add(child);
               }
           };

            string query = string.Empty;
            switch (parent.Type.ToLowerInvariant())
            {
                case "section":
                    query = string.Format("SELECT A.SectionID ID, Name, SEName, 'Section' [Type], ParentSectionID ParentEntityID, DisplayOrder FROM Section A WITH (NOLOCK) INNER JOIN (SELECT DISTINCT " +
                        "SectionID EntityID FROM Section A WITH (NOLOCK) LEFT JOIN EntityStore B WITH (NOLOCK) ON A.SectionID = B.EntityID AND EntityType = 'Section' WHERE ({0} = 0 or StoreID = {1})) B " +
                        "ON A.SectionID = B.EntityID WHERE ParentSectionID = {2} AND Published=1 AND Deleted=0 ORDER BY DisplayOrder, Name ASC", CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowEntityFiltering") == true, 1, 0), AppLogic.StoreID(), parent.ID);
                    break;

                case "category":
                    query = string.Format("SELECT A.CategoryID ID, Name, SEName, 'Category' [Type], ParentCategoryID ParentEntityID, DisplayOrder FROM Category A WITH (NOLOCK) INNER JOIN (SELECT DISTINCT " +
                        "CategoryID EntityID FROM Category A WITH (NOLOCK) LEFT JOIN EntityStore B WITH (NOLOCK) ON A.CategoryID = B.EntityID AND EntityType = 'Category' WHERE ({0} = 0 or StoreID = {1})) B " +
                        "ON A.CategoryID = B.EntityID WHERE ParentCategoryID = {2} AND Published=1 AND Deleted=0 ORDER BY DisplayOrder, Name ASC", CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowEntityFiltering") == true, 1, 0), AppLogic.StoreID(), parent.ID);
                    break;

                case "manufacturer":
                    query = string.Format("SELECT A.ManufacturerID ID, Name, SEName, 'Manufacturer' [Type], ParentManufacturerID ParentEntityID, DisplayOrder FROM Manufacturer A WITH (NOLOCK) INNER JOIN (SELECT " +
                        "DISTINCT ManufacturerID EntityID FROM Manufacturer A WITH (NOLOCK) LEFT JOIN EntityStore B WITH (NOLOCK) ON A.ManufacturerID = B.EntityID AND EntityType = 'Manufacturer' WHERE ({0} = 0 or " +
                        "StoreID = {1})) B ON A.ManufacturerID = B.EntityID WHERE ParentManufacturerID = {2} AND Published=1 AND Deleted=0 ORDER BY DisplayOrder, Name ASC", CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowEntityFiltering") == true, 1, 0), AppLogic.StoreID(), parent.ID);
                    break;

                case "vector":
                    query = string.Format("SELECT A.VectorID ID, Name, SEName, 'Vector' [Type], ParentVectorID ParentEntityID, DisplayOrder FROM Vector A WITH (NOLOCK) INNER JOIN (SELECT DISTINCT " +
                        "VectorID EntityID FROM Vector A WITH (NOLOCK) LEFT JOIN EntityStore B WITH (NOLOCK) ON A.VectorID = B.EntityID AND EntityType = 'Vector' WHERE ({0} = 0 or StoreID = {1})) B " +
                        "ON A.VectorID = B.EntityID WHERE ParentVectorID = {2} AND Published=1 AND Deleted=0 ORDER BY DisplayOrder, Name ASC", CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowEntityFiltering") == true, 1, 0), AppLogic.StoreID(), parent.ID);
                    break;

                case "genre":
                    query = string.Format("SELECT A.GenreID ID, Name, SEName, 'Genre' [Type], ParentGenreID ParentEntityID, DisplayOrder FROM Genre A WITH (NOLOCK) INNER JOIN (SELECT DISTINCT " +
                        "GenreID EntityID FROM Genre A WITH (NOLOCK) LEFT JOIN EntityStore B WITH (NOLOCK) ON A.GenreID = B.EntityID AND EntityType = 'GenreID' WHERE ({0} = 0 or StoreID = {1})) B " +
                        "ON A.GenreID = B.EntityID WHERE ParentGenreID = {2} AND Published=1 AND Deleted=0 ORDER BY DisplayOrder, Name ASC", CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowEntityFiltering") == true, 1, 0), AppLogic.StoreID(), parent.ID);
                    break;

                case "distributor":
                    query = string.Format("SELECT A.DistributorID ID, Name, SEName, 'Distributor' [Type], ParentDistributorID ParentEntityID, DisplayOrder FROM Distributor A WITH (NOLOCK) INNER JOIN (SELECT " +
                        "DISTINCT DistributorID EntityID FROM Distributor A WITH (NOLOCK) LEFT JOIN EntityStore B WITH (NOLOCK) ON A.DistributorID = B.EntityID AND EntityType = 'Distributor' WHERE ({0} = 0 or " +
                        "StoreID = {1})) B ON A.DistributorID = B.EntityID WHERE ParentDistributorID = {2} AND Published=1 AND Deleted=0 ORDER BY DisplayOrder, Name ASC", CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowEntityFiltering") == true, 1, 0), AppLogic.StoreID(), parent.ID);
                    break;
            }

            if (query.Length > 0)
            {
                DB.UseDataReader(query, readAction);
            }

            return children;
        }

        //public static LinkItemCollection GetAll(string type)
        //{
        //    return GetAll(type, ReturnAllRecords);
        //}

        //public static LinkItemCollection GetAll(string type, string searchFilter)
        //{
        //    return GetAll(type, searchFilter, ReturnAllRecords);
        //}

        //public static LinkItemCollection GetAll(string type, int top)
        //{
        //    return GetAll(type, NoSearchFilter, top);
        //}

        //public static LinkItemCollection GetAll(string type, string searchFilter, int top)
        //{
        //    int pages = 0;
        //    return GetAll(type, searchFilter, NoSearchFilter, 1, 1, true, out pages);
        //}

        private int _pageSize = 0;
        private int _pageCount = 0;
        private int _currentPage = 0;
        private int _allCount = 0;
        private int _startPage = 0;
        private int _endPage = 0;

        public int PageSize
        {
            get { return _pageSize; }
        }

        public int PageCount
        {
            get { return _pageCount; }
        }

        public int CurrentPage
        {
            get { return _currentPage; }
        }

        public int AllCount
        {
            get { return _allCount; }
        }

        public int StartPage
        {
            get { return _startPage; }
        }

        public int EndPage
        {
            get { return _endPage; }
        }

        //public static LinkItemCollection GetAll(string type,
        //    string searchFilter,
        //    string alphaFilter,
        //    int pageSize,
        //    int page,
        //    bool firstLevel,
        //    out int pages
        //    )
        //{
        //    pages = 0;

        //    LinkItemCollection all = new LinkItemCollection();

        //    Action<DataContext> queryContext = db => 
        //    {
        //        Table<ProductMappingLinkItem> entities = db.GetTable<ProductMappingLinkItem>();
        //        var query =
        //            from e in entities
        //            where e.Type == type
        //            orderby e.DisplayOrder, e.Name ascending
        //            select e;

        //        if (firstLevel)
        //        {
        //            query = query.Where(e => e.ParentEntityID == ProductMappingLinkItem.ROOT_ID) as IOrderedQueryable<ProductMappingLinkItem>;
        //        }

        //        if (searchFilter != NoSearchFilter)
        //        {
        //            query = query.Where(e => e.Name.Contains(searchFilter)) as IOrderedQueryable<ProductMappingLinkItem>;
        //        }

        //        if (alphaFilter != NoSearchFilter)
        //        {
        //            query = query.Where(e => e.Name.StartsWith(alphaFilter)) as IOrderedQueryable<ProductMappingLinkItem>;
        //        }

        //        int count = 0;                

        //        if (pageSize > 1)
        //        {
        //            count = query.Count();
        //            int startAt = 0;

        //            if (count <= pageSize)
        //            {
        //                pageSize = count;
        //                page = 1;
        //            }

        //            startAt = (page - 1) * pageSize;

        //            query = query.Skip(startAt).Take(pageSize) as IOrderedQueryable<ProductMappingLinkItem>;

        //            all._allCount = count;
        //            all._pageSize = pageSize;

        //            if (count > 0)
        //            {
        //                all._pageCount = count / pageSize;

        //                int remainder = count % pageSize;
        //                if (remainder > 0)
        //                {
        //                    all._pageCount += 1;
        //                }

        //                all._currentPage = page; // selected page...

        //                all._startPage = startAt + 1; // start at 1 not 0
        //                all._endPage = startAt + pageSize;
        //            }
        //        }

        //        foreach (var e in query)
        //        {
        //            e.Name = XmlCommon.GetLocaleEntry(e.Name, Customer.Current.LocaleSetting, true);
        //            all.Add(e);
        //        }
        //    };

        //    DB.UseDataContext(queryContext);

        //    return all;
        //}
    }
}

