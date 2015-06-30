// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using AspDotNetStorefrontCore;
using System.Collections.Generic;
using System.IO;
using System.Data.SqlClient;
using System.Xml;
using System.Xml.XPath;

namespace AspDotNetStorefront
{
    public class Resource
    {
        private string _name;
        private string _url;
        private string _description;
        private string _title;
        private double _distance;

        public Resource(string name, string url, string description)
        {
            Name = name;
            URL = url;
            Description = description;
        }

        public Resource(string name, string url, string description, string title)
        {
            Name = name;
            URL = url;
            Description = description;
            Title = title;
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public string URL
        {
            get { return _url; }
            set { _url = value; }
        }

        public double Distance
        {
            get { return _distance; }
            set { _distance = value; }
        }
    }

    /// <summary>
    /// Summary description for PageNotFound
    /// </summary>
    public partial class PageNotFound : SkinBase
    {
        #region Variable Declaration

        private List<Resource> _resources = new List<Resource>();

        #endregion

        protected override void OnInit(EventArgs e)
        {
            Topic pageNotFoundTopic = new Topic("PageNotFound");
            litTopicNotFound.Text = pageNotFoundTopic.Contents;

            DetermineIntendedPage();

            base.OnInit(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.StatusCode = 404;
        }

        /// <summary>
        /// Get the querystring by aspxerrorpath and determine the intended page
        /// </summary>
        private void DetermineIntendedPage()
        {
            //If this appconfig is set to false, suggestion link will not appear. Default is true
            bool showPageNotFoundSuggestedLink = AppLogic.AppConfigBool("Show404SuggestionLinks");

            if (showPageNotFoundSuggestedLink)
            {
                String intendedUrl = CommonLogic.QueryStringCanBeDangerousContent("aspxerrorpath");

                //We might not always be sent here with a querystring
                if (CommonLogic.IsStringNullOrEmpty(intendedUrl))
                    intendedUrl = CommonLogic.GetThisPageName(false);

                string intendedPage = Path.GetFileNameWithoutExtension(intendedUrl);

                LoadResources();
                MakeSuggestions(intendedPage);
            }
        }

        /// <summary>
        /// Create and filter the suggestion link in a list
        /// </summary>
        /// <param name="intendedPage"></param>
        private void MakeSuggestions(string intendedPage)
        {
            //Set it to desire likeness of words using Levenshtein Distance Algorithm
            //.60 to .70 is the suggested distance, default is .60
            double showPageNotFoundDistance = AppLogic.AppConfigNativeDouble("404.ComparisonDistance");

            // Default value is 5
            int numberOfSuggestedLinks = AppLogic.AppConfigNativeInt("404.NumberOfSuggestedLinks");

            intendedPage = intendedPage.ToLowerInvariant();

            List<Resource> matchingResources = new List<Resource>();

            for (int ctr = 0; ctr < this._resources.Count; ctr++)
            {
                Resource resource = _resources[ctr];

                //Compute for the distance using Levenshtein Distance Algorithm
                double distance = LevenshteinDistance.CalcDistance(resource.Name, intendedPage); // both in lower case
                //Divide it by resource name length for more accuracy
                double meanDistancePerLetter = (distance / resource.Name.Length);
                resource.Distance = meanDistancePerLetter;

                if (meanDistancePerLetter <= showPageNotFoundDistance)
                {
                    matchingResources.Add(resource);
                }
            }

            // Sort to get the best 
            matchingResources.Sort(CompareByDistance);

            int count = 0;

            if (matchingResources.Count > numberOfSuggestedLinks)
            {
                count = matchingResources.Count - numberOfSuggestedLinks;
            }

            // remove the excess base on the set limit in 404.NumberOfSuggestedLinks appconfig 
            matchingResources.RemoveRange(0, count);

            if (matchingResources.Count > 0)
            {
                DisplaySuggestions(matchingResources);
            }
        }

        /// <summary>
        /// Display the suggestion in datalist with description or title for topics
        /// </summary>
        /// <param name="matchingResources"></param>
        private void DisplaySuggestions(List<Resource> matchingResources)
        {
            DataList1.DataSource = matchingResources;
            DataList1.DataBind();
        }

        /// <summary>
        /// Load the resources to a list<> for topic,product, manufacturer,category,section
        /// </summary>
        private void LoadResources()
        {
            // By default the value is blank, it will suggest valid store site pages related to 
            // product, category, manufacturer, section, and topic
            string visibleSuggestions = AppLogic.AppConfig("404.VisibleSuggestions");

            if (CommonLogic.IsStringNullOrEmpty(visibleSuggestions))
            {
                visibleSuggestions = "product, category, manufacturer, section, topic";
            }

            bool showProduct = CommonLogic.StringInCommaDelimitedStringList("product", visibleSuggestions);
            if (showProduct)
            {
                LoadProducts();
            }

            bool showCategories = CommonLogic.StringInCommaDelimitedStringList("category", visibleSuggestions);
            if (showCategories)
            {
                LoadCategories();
            }

            bool showManufacturer = CommonLogic.StringInCommaDelimitedStringList("manufacturer", visibleSuggestions);
            if (showManufacturer)
            {
                LoadManufacturers();
            }

            bool showSection = CommonLogic.StringInCommaDelimitedStringList("section", visibleSuggestions);
            if (showSection)
            {
                LoadSections();
            }

            bool showTopics = CommonLogic.StringInCommaDelimitedStringList("topic", visibleSuggestions);
            if (showTopics)
            {
                LoadTopics();
            }
        }

        private void LoadCategories()
        {
            LoadEntity("Category");
        }

        private void LoadManufacturers()
        {
            LoadEntity("Manufacturer");
        }

        private void LoadSections()
        {
            LoadEntity("Section");
        }

        /// <summary>
        /// Load entity(category, manufacturer,section)
        /// </summary>
        /// <param name="entityName">Entity name(category, manufacturer,section)</param>
        private void LoadEntity(string entityName)
        {
            EntityHelper entity = AppLogic.LookupHelper(entityName, 0);
            if (entity != null)
            {
                XmlDocument entityDoc = entity.m_TblMgr.XmlDoc;
                if (entityDoc != null)
                {
                    XPathNavigator nav = entityDoc.CreateNavigator();
                    ExtractEntity(entityName, nav.Select("root/Entity"));
                }
            }
        }

        /// <summary>
        /// Load the product from database to a list
        /// </summary>
        private void LoadProducts()
        {

            try 
            {
                // load Product
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();

                    int hideProductsWithLessThanThisInventoryLevel = AppLogic.AppConfigNativeInt("HideProductsWithLessThanThisInventoryLevel");
					bool hideProducts = hideProductsWithLessThanThisInventoryLevel != -1;

                    // Use this query, as it's faster unless HideProductsWithLessThanThisInventoryLevel appconfig != -1
                    string sql = "SELECT ProductID,Name,SEName, Description FROM Product with (NOLOCK) WHERE Deleted = 0 and Published =1 and Getdate() between IsNull(availablestartdate,'1/1/1900') and IsNull(availablestopdate,'1/1/2999')";

                    if (hideProducts)
                    {
                        sql = "SELECT  Product.ProductID, Product.Name, Product.SEName, Product.Description, Product.TrackInventoryBySizeAndColor, ProductVariant.VariantID, ProductVariant.Inventory";
                        sql += " FROM  Product with (NOLOCK) INNER JOIN";
                        sql += " ProductVariant ON Product.ProductID = ProductVariant.ProductID where ProductVariant.IsDefault = 1 and Product.Deleted = 0 and Product.Published =1 and Getdate() between IsNull(Product.availablestartdate,'1/1/1900') and IsNull(Product.availablestopdate,'1/1/2999')";

                    }

                    using (IDataReader rs = DB.GetRS(sql, con))
                    {
                        while (rs.Read())
                        {
                            if (hideProducts)
                            {
                                bool trackInventoryBySizeAndColor = DB.RSFieldBool(rs, "TrackInventoryBySizeAndColor");
                                int variantId = DB.RSFieldInt(rs, "VariantID");
                                int inventoryOfDefaultVariant = DB.RSFieldInt(rs, "Inventory");

                                // We must filter the suggested product if HideProductsWithLessThanThisInventoryLevel appconfig != -1
                                // based on the inventory of default variant of the product. 
                                if (inventoryOfDefaultVariant < hideProductsWithLessThanThisInventoryLevel && !trackInventoryBySizeAndColor)
                                    continue;
                       
								// We must filter the suggested product if HideProductsWithLessThanThisInventoryLevel appconfig != -1
                                // and TrackInventoryBySizeAndColor is true based on total quantity of the attribute for variant
                                if (trackInventoryBySizeAndColor && MustFilter(variantId, hideProductsWithLessThanThisInventoryLevel))
									continue;
                            }

                            int productID = DB.RSFieldInt(rs, "productID");
                            string name = DB.RSFieldByLocale(rs, "Name", Localization.GetDefaultLocale());
                            string seName = DB.RSFieldByLocale(rs, "SeName", Localization.GetDefaultLocale());
                            string title = string.Empty;
                            string description = DB.RSFieldByLocale(rs, "Description", Localization.GetDefaultLocale());
                            string url = SE.MakeProductLink(productID, seName);

                            _resources.Add(new Resource(name, url, description, title));

                        }
                    }
                }
            }
            catch 
            {
            }
        }
        /// <summary>
        /// Load topic from a database to a list
        /// </summary>
        private void LoadTopics()
        {
            // load topics
            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("SELECT Name, Title FROM Topic with (NOLOCK) WHERE ShowInSiteMap = 1 and skinid =" + ThisCustomer.SkinID, con))
                {
                    while (rs.Read())
                    {
                        string name = DB.RSFieldByLocale(rs, "Name", ThisCustomer.LocaleSetting);
                        string description = string.Empty;
                        string title = DB.RSFieldByLocale(rs, "Title", ThisCustomer.LocaleSetting);
                        string url = SE.MakeDriverLink(name);
                        
                        _resources.Add(new Resource(name, url, description, title));

                    }
                }
            }
        }

        private XmlNode ExtractXmlNode(XPathNavigator nav)
        {
            return (nav as IHasXmlNode).GetNode();
        }

        /// <summary>
        /// Extract entity ID, name,description to the list
        /// </summary>
        /// <param name="entityName">Entity name(category,section,manufacturer)</param>
        /// <param name="entityIterator"></param>
        private void ExtractEntity(string entityName, XPathNodeIterator entityIterator)
        {
            while (entityIterator.MoveNext())
            {
                XmlNode entityNode = ExtractXmlNode(entityIterator.Current);

                int entityId = XmlCommon.XmlFieldNativeInt(entityNode, "EntityID");
                string name = XmlCommon.XmlFieldByLocale(entityNode, "Name", ThisCustomer.LocaleSetting);
                string description = XmlCommon.XmlFieldByLocale(entityNode, "Description", ThisCustomer.LocaleSetting);
                string url = SE.MakeEntityLink(entityName, entityId, name);

                _resources.Add(new Resource(name, url, description));

                // recurse
                ExtractEntity(entityName, entityNode.CreateNavigator().Select("Entity"));
            }
        }

        /// <summary>
        /// Filter the prdouct base on the HideProductsWithLessThanThisInventoryLevel and total quantity of the attribute for variant
        /// </summary>
        /// <param name="variantId">variant Id</param>
        /// <param name="hideProductsWithLessThanThisInventoryLevel">Appconfig HideProductsWithLessThanThisInventoryLevel</param>
        /// <returns>True or false</returns>
        private bool MustFilter(int variantId, int hideProductsWithLessThanThisInventoryLevel)
        {
            int inventoryLevel = 0;

            try
            {
                //this will query the inventory base on total quantity of the attribute for variant
                using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
                {
                    conn.Open();
                    using (IDataReader bySizesAndColorReader = DB.GetRS("SELECT SUM(Quan) as TotalQuantity from Inventory  WITH (NOLOCK)  WHERE VariantID=" + variantId.ToString(), conn))
                    {
                        if (bySizesAndColorReader.Read())
                        {
                            inventoryLevel = DB.RSFieldInt(bySizesAndColorReader, "TotalQuantity");

                            if (inventoryLevel < hideProductsWithLessThanThisInventoryLevel)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            catch 
            {
                return false;
            }

            return false;
        }

        /// <summary>
        /// Compare 2 integer
        /// </summary>
        /// <param name="a">distance a</param>
        /// <param name="b">distance b</param>
        /// <returns></returns>
        private static int CompareByDistance(Resource a, Resource b)
        {
            if (a.Distance < b.Distance)
                return -1;
            else if (a.Distance < b.Distance)
                return 1;
            else
                return 0;
        }

        /// <summary>
        /// Set stringresources to the title of the datalist
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void DataList1_ItemDataBound(object sender, DataListItemEventArgs e)
        {
            // Assign stringresource
            if (e.Item.ItemType == ListItemType.Header)
            {
                Label lbltext = (Label)e.Item.FindControl("SuggestionTitleMessage");
                lbltext.Text = AppLogic.GetString("PageNotFound.SuggestionTitleMessage", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
            }
        }
    }



    /// <summary>
    /// Class designed to use the Levenshtein Distance Algorithm
    /// </summary>
    public class LevenshteinDistance
    {
        /// <summary>
        /// Compute Levenshtein distance. 
        /// </summary>
        /// <returns>Distance between the two strings.
        /// The larger the number, the bigger the difference.
        /// </returns>
        public static int CalcDistance(string s, string t)
        {
            int n = s.Length; //length of s
            int m = t.Length; //length of t
            int[,] d = new int[n + 1, m + 1]; // matrix
            int cost; // cost
            // Step 1
            if (n == 0) return m;
            if (m == 0) return n;
            // Step 2
            for (int i = 0; i < n; d[i, 0] = ++i) ;
            for (int j = 0; j < m; d[0, j] = ++j) ;
            // Step 3
            for (int i = 1; i <= n; i++)
            {
                //Step 4
                for (int j = 1; j <= m; j++)
                {
                    // Step 5
                    cost = (t.Substring(j - 1, 1) == s.Substring(i - 1, 1) ? 0 : 1);
                    // Step 6
                    d[i, j] = System.Math.Min(System.Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m];
        }
    }
}
