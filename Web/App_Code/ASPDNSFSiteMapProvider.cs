// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Xml.Serialization;
using System.IO;
using System.Text;
using System.Collections.Generic;
using AspDotNetStorefrontCommon;
using System.Text.RegularExpressions;
using System.Web.Caching;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Default SiteMapProvider for this site
    /// </summary>
    public class ASPDNSFSiteMapProvider : StaticSiteMapProvider
    {
        #region Variable Declaration

        private static object _mutex = new object();
        private SiteMapNode _root = null;
        private const int DEFAULT_SKINID = 1;
        private string _localeSetting = string.Empty;
        private const string SECTION = "section";
        private const string MANUFACTURER = "manufacturer";
        private const string CATEGORY = "category";
        private const string DISTRIBUTOR = "distributor";
        private const string GENRE = "genre";
        private const string VECTOR = "vector";
        private Customer m_thiscustomer;
        private string m_xmlpackage;
        private int m_maxlevels;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the current customer context
        /// </summary>
        public Customer ThisCustomer
        {
            get { return m_thiscustomer; }
            set { m_thiscustomer = value; }
        }

        /// <summary>
        /// Gets or sets the maximum dynamic display levels
        /// </summary>
        public int MaximumDynamicDisplayLevels
        {
            get { return m_maxlevels; }
            set { m_maxlevels = value; }
        }

        /// <summary>
        /// Gets or sets the xmlpackage to use for the sitemap
        /// </summary>
        public string XmlPackage
        {
            get { return m_xmlpackage; }
            set { m_xmlpackage = value; }
        }

        #endregion

        #region Constructor

        

        /// <summary>
        /// Constructor for ASPDNSFSiteMapProvider
        /// </summary>
        public ASPDNSFSiteMapProvider(Customer cust)
        {
            this.ThisCustomer = cust;
        }

        #endregion



        #region Methods

        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection attributes)
        {
            XmlPackage = attributes["xmlPackage"];
            if (CommonLogic.IsStringNullOrEmpty(XmlPackage))
            {
                throw new ArgumentNullException("XmlPackage attribute not specified!!!");
            }

            base.Initialize(name, attributes);
        }

        #region BuildSiteMap
        /// <summary>
        /// Loads the site map information from persistent storage and builds it in memory.
        /// </summary>
        /// <returns></returns>
        public override SiteMapNode BuildSiteMap()
        {
            lock (_mutex)
            {
                if (null == _root)
                {
                    BuildSiteMapTree();
                }

                return _root;
            }
        }
        #endregion

        #region Flush
        /// <summary>
        /// Flushes the current cache so that it will rebuild the tree again
        /// </summary>
        public void Flush()
        {
            lock (_mutex)
            {
                this.Clear();
                _root = null;
            }
        }
        #endregion

        #region BuildSiteMapTree
        /// <summary>
        /// Core function to build the sitemap tree together with it's hierarchy
        /// </summary>
        private void BuildSiteMapTree()
        {
            XPathNavigator nav = GetMenuXml();
            XPathNodeIterator homeIterator = nav.Select("siteMap/siteMapNode");

            if (homeIterator.MoveNext())
            {
                XPathNavigator navHome = homeIterator.Current;
                _root = NewSiteMapNode(navHome);
                BuildSiteMapTree(navHome, _root);
            }
            else
            {
                throw new InvalidOperationException("Home SiteMap root element missing!!!");
            }

            AddNode(_root);
        }

        #endregion

        #region BuildSiteMapTree
        /// <summary>
        /// Core function to build the sitemap tree together with it's hierarchy
        /// </summary>
        /// <param name="nav">The XPathNavigator object</param>
        /// <param name="parent">The parent node</param>
        private void BuildSiteMapTree(XPathNavigator nav, SiteMapNode parent)
        {
            XPathNodeIterator enumerator = nav.Select("siteMapNode");
            while (enumerator.MoveNext())
            {
                XPathNavigator current = enumerator.Current;

                if (IsEntityNode(current))
                {
                    SiteMapNode entityNode = NewSiteMapNode(current);
                    AddNode(entityNode, parent);
                    LoadEntity(ExtractType(current), entityNode);
                }
                else
                {
                    SiteMapNode node = NewSiteMapNode(current);
                    AddNode(node, parent);


                    BuildSiteMapTree(current, node);
                }
            }
        }
        #endregion

        #region NewSiteMapNode
        /// <summary>
        /// Returns a new instance of SiteMapNode using the title and url attributes
        /// </summary>
        /// <param name="nav">The XPathNavigator object</param>
        /// <returns></returns>
        private SiteMapNode NewSiteMapNode(XPathNavigator nav)
        {
            string title = ExtractTitle(nav);
            string url = nav.GetAttribute("url", string.Empty);

            return new SiteMapNode(this, NewKey(), url, title);
        }
        #endregion

        #region NewKey
        /// <summary>
        /// Returns a random autogenerated key
        /// </summary>
        /// <returns></returns>
        private string NewKey()
        {
            return Guid.NewGuid().ToString();
        }
        #endregion

        #region GetMenuXml
        /// <summary>
        /// Returns an XmlNavigator object from the loaded menu xml package
        /// </summary>
        /// <returns></returns>
        private XPathNavigator GetMenuXml()
        {
            string menuXml = AppLogic.RunXmlPackage(XmlPackage, null, ThisCustomer, ThisCustomer.SkinID, string.Empty, string.Empty, false, false);

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
        #endregion

        private string StringResourceMatch(Match match)
        {
            String l = match.Groups[1].Value;
            string s = HttpUtility.HtmlEncode(AppLogic.GetString(l, DEFAULT_SKINID, ThisCustomer.LocaleSetting));
            if (s == null || s.Length == 0 || s == l)
            {
                s = match.Value;
            }
            return XmlCommon.XmlEncode(s);
        }

        #region GetRootNodeCore
        /// <summary>
        /// Gets the root node
        /// </summary>
        /// <returns></returns>
        protected override SiteMapNode GetRootNodeCore()
        {
            return BuildSiteMap();
        }
        #endregion

        private bool IsEntityNode(XPathNavigator nav)
        {
            string entity = ExtractType(nav);

            return entity.Equals(SECTION, StringComparison.InvariantCultureIgnoreCase) ||
                    entity.Equals(MANUFACTURER, StringComparison.InvariantCultureIgnoreCase) ||
                    entity.Equals(CATEGORY, StringComparison.InvariantCultureIgnoreCase) ||
                    entity.Equals(DISTRIBUTOR, StringComparison.InvariantCultureIgnoreCase) ||
                    entity.Equals(GENRE, StringComparison.InvariantCultureIgnoreCase) ||
                    entity.Equals(VECTOR, StringComparison.InvariantCultureIgnoreCase);
        }

        private string ExtractTitle(XPathNavigator nav)
        {
            return nav.GetAttribute("title", string.Empty);
        }

        private string ExtractType(XPathNavigator nav)
        {
            return nav.GetAttribute("type", string.Empty);
        }

        private void LoadEntity(string entityName, SiteMapNode parentNode)
        {
            LinkItemCollection items = LinkItemCollection.GetAllFirstLevel(entityName, AppLogic.AppConfigNativeInt("MaxMenuSize"), ThisCustomer.LocaleSetting);

            if (items != null)
            {
                ExtractEntity(items, parentNode, 1);
            }
        }

        private void ExtractEntity(LinkItemCollection productLinks, SiteMapNode parentNode, int level)
        {
            foreach (ProductMappingLinkItem item in productLinks)
            {
                string name = item.Name;
                string url = item.Url;

                SiteMapNode newNode = new SiteMapNode(this, NewKey(), url, name);
                AddNode(newNode, parentNode);

                // querying child entities are expensive
                // we only check for it depending on the 
                // defined maximum dynamic display levels
                if (level < this.MaximumDynamicDisplayLevels)
                {
                    item.LoadChildren();

                    if (item.HasChildren)
                    {
                        ExtractEntity(item.ChildItems, newNode, level + 1);
                    }
                }
            }
        }

        #endregion

    }
}

