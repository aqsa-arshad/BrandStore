// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Xml;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for ShowEntityPage. 
    /// 
    /// NOTE: the sql creation statements neeed to be cleaned up. they are pretty unreadible and inefficient at the moment due to substitutions
    /// 
    /// </summary>
    public class ShowEntityPage
    {
        private EntitySpecs m_EntitySpecs;

        private int m_EntityInstanceID;
        private String m_EntityInstanceName;
        private EntityHelper m_EntityHelper;
        private SkinBase m_SkinBase;
        private String m_ResourcePrefix;
        private String m_XmlPackage;
        private XmlNode n;
        private XmlNode m_ThisEntityNodeContext;

        private int m_SectionFilterID = 0;
        private int m_CategoryFilterID = 0;
        private int m_ManufacturerFilterID = 0;
        private int m_DistributorFilterID = 0;
        private int m_GenreFilterID = 0;
        private int m_VectorFilterID = 0;
        private int m_ProductTypeFilterID = 0;

        private bool m_URLValidated = true;

        private string m_PageOutput;

        /// <summary>
        /// Gets a value indicating whether this instance is table Order add to cart post back.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is table order add to cart post back; otherwise, <c>false</c>.
        /// </value>
        public bool isTableOrderAddToCartPostBack
        {
            get
            {
                return "TableOrderAddToCart".Equals(CommonLogic.FormCanBeDangerousContent("__EVENTTARGET"),
                    StringComparison.InvariantCultureIgnoreCase);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is an add to cart post back from an entity page (eg. entity.simpleproductlist.xml.config)
        /// </summary>
        public bool IsAddToCartPostBack
        {
            get
            {
                return "AddToCart".Equals(CommonLogic.FormCanBeDangerousContent("__EVENTTARGET"),
                    StringComparison.InvariantCultureIgnoreCase);
            }
        }

        public ShowEntityPage(EntitySpecs eSpecs, SkinBase sb)
        {
            m_EntitySpecs = eSpecs;
            m_SkinBase = sb;
            m_EntityHelper = AppLogic.LookupHelper(m_EntitySpecs.m_EntityName, 0);
            m_ResourcePrefix = String.Format("show{0}.aspx.", m_EntitySpecs.m_EntityName.ToLowerInvariant());
            m_EntityInstanceID = CommonLogic.QueryStringUSInt(m_EntityHelper.GetEntitySpecs.m_EntityName + "ID");
        }

        public int GetActiveEntityID
        {
            get { return m_EntityInstanceID; }
        }

        public EntityHelper GetActiveEntityHelper
        {
            get { return m_EntityHelper; }
        }

        public XmlNode GetActiveEntityNodeContext
        {
            get { return m_ThisEntityNodeContext; }
        }

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        public void Page_Load(object sender, EventArgs e)
        {
            if (AppLogic.AppConfigBool("GoNonSecureAgain"))
            {
                m_SkinBase.GoNonSecureAgain();
            }

            n = m_EntityHelper.m_TblMgr.SetContext(m_EntityInstanceID);
            //Determine if the entity is map to the current store.
            if (n == null)
            {
                HttpContext.Current.Server.Transfer("pagenotfound.aspx");
            }
            //Checking for multi store.
            CachelessStore store = new CachelessStore();
            store.StoreID = AppLogic.StoreID();
            MappedObject map = store.GetMapping(m_EntitySpecs.m_EntityName, m_EntityInstanceID);

            if (AppLogic.GlobalConfigBool("AllowEntityFiltering") == true && !map.IsMapped)
            {
                HttpContext.Current.Server.Transfer("pagenotfound.aspx");
            }
            m_ThisEntityNodeContext = n;

            String SENameINURL = CommonLogic.QueryStringCanBeDangerousContent("SEName");
            if (SENameINURL.Equals(XmlCommon.XmlField(GetActiveEntityNodeContext, "SEName"), StringComparison.InvariantCultureIgnoreCase) == false)
            {
                string QS = BuildQueryString();
                String NewURL = string.Format("{0}{1}{2}", AppLogic.GetStoreHTTPLocation(false, false), SE.MakeEntityLink(m_EntityHelper.GetEntitySpecs.m_EntityName, m_EntityInstanceID, XmlCommon.XmlField(GetActiveEntityNodeContext, "SEName")), QS);
                HttpContext.Current.Response.Write("<html><head><title>Object Moved</title></head><body><b>Object moved to <a href=\"" + NewURL + "\">HERE</a></b></body></html>");
                HttpContext.Current.Response.Status = "301 Moved Permanently";
                HttpContext.Current.Response.AddHeader("Location", NewURL);
                m_URLValidated = false;
            }

            if (m_URLValidated)
            {
                m_CategoryFilterID = CommonLogic.QueryStringUSInt("CategoryFilterID");
                m_SectionFilterID = CommonLogic.QueryStringUSInt("SectionFilterID");
                m_ProductTypeFilterID = CommonLogic.QueryStringUSInt("ProductTypeFilterID");
                m_ManufacturerFilterID = CommonLogic.QueryStringUSInt("ManufacturerFilterID");
                m_DistributorFilterID = CommonLogic.QueryStringUSInt("DistributorFilterID");
                m_GenreFilterID = CommonLogic.QueryStringUSInt("GenreFilterID");
                m_VectorFilterID = CommonLogic.QueryStringUSInt("VectorFilterID");

                if (CommonLogic.QueryStringCanBeDangerousContent("CategoryFilterID").Length == 0)
                {
                    if (CommonLogic.QueryStringCanBeDangerousContent("ResetFilters").Length == 0 && AppLogic.AppConfigBool("PersistFilters") && CommonLogic.CookieUSInt("CategoryFilterID") != 0)
                    {
                        m_CategoryFilterID = CommonLogic.CookieUSInt("CategoryFilterID");
                    }
                }

                if (CommonLogic.QueryStringCanBeDangerousContent("SectionFilterID").Length == 0)
                {
                    if (CommonLogic.QueryStringCanBeDangerousContent("ResetFilters").Length == 0 && AppLogic.AppConfigBool("PersistFilters") && CommonLogic.CookieUSInt("SectionFilterID") != 0)
                    {
                        m_SectionFilterID = CommonLogic.CookieUSInt("SectionFilterID");
                    }
                }

                if (CommonLogic.QueryStringCanBeDangerousContent("ProductTypeFilterID").Length == 0)
                {
                    if (CommonLogic.QueryStringCanBeDangerousContent("ResetFilters").Length == 0 && AppLogic.AppConfigBool("PersistFilters") && CommonLogic.CookieUSInt("ProductTypeFilterID") != 0)
                    {
                        m_ProductTypeFilterID = CommonLogic.CookieUSInt("ProductTypeFilterID");
                    }
                    if (m_ProductTypeFilterID != 0 &&
                        !AppLogic.ProductTypeHasVisibleProducts(m_ProductTypeFilterID))
                    {
                        m_ProductTypeFilterID = 0;
                    }
                }

                if (CommonLogic.QueryStringCanBeDangerousContent("ManufacturerFilterID").Length == 0)
                {
                    if (CommonLogic.QueryStringCanBeDangerousContent("ResetFilters").Length == 0 && AppLogic.AppConfigBool("PersistFilters") &&
                        CommonLogic.CookieUSInt("ManufacturerFilterID") != 0)
                    {
                        m_ManufacturerFilterID = CommonLogic.CookieUSInt("ManufacturerFilterID");
                    }
                }

                if (CommonLogic.QueryStringCanBeDangerousContent("DistributorFilterID").Length == 0)
                {
                    if (CommonLogic.QueryStringCanBeDangerousContent("ResetFilters").Length == 0 && AppLogic.AppConfigBool("PersistFilters") &&
                        CommonLogic.CookieUSInt("DistributorFilterID") != 0)
                    {
                        m_DistributorFilterID = CommonLogic.CookieUSInt("DistributorFilterID");
                    }
                }

                if (CommonLogic.QueryStringCanBeDangerousContent("GenreFilterID").Length == 0)
                {
                    if (CommonLogic.QueryStringCanBeDangerousContent("ResetFilters").Length == 0 && AppLogic.AppConfigBool("PersistFilters") &&
                        CommonLogic.CookieUSInt("GenreFilterID") != 0)
                    {
                        m_GenreFilterID = CommonLogic.CookieUSInt("GenreFilterID");
                    }
                }

                if (CommonLogic.QueryStringCanBeDangerousContent("VectorFilterID").Length == 0)
                {
                    if (CommonLogic.QueryStringCanBeDangerousContent("ResetFilters").Length == 0 && AppLogic.AppConfigBool("PersistFilters") &&
                        CommonLogic.CookieUSInt("VectorFilterID") != 0)
                    {
                        m_VectorFilterID = CommonLogic.CookieUSInt("VectorFilterID");
                    }
                }

                if (CommonLogic.QueryStringCanBeDangerousContent("ResetFilters").Length != 0)
                {
                    m_CategoryFilterID = 0;
                    m_SectionFilterID = 0;
                    m_ManufacturerFilterID = 0;
                    m_DistributorFilterID = 0;
                    m_GenreFilterID = 0;
                    m_VectorFilterID = 0;
                    m_ProductTypeFilterID = 0;
                }

                if (AppLogic.AppConfigBool("PersistFilters"))
                {
                    HttpContext.Current.Profile.SetPropertyValue("CategoryFilterID", m_CategoryFilterID.ToString());
                    HttpContext.Current.Profile.SetPropertyValue("SectionFilterID", m_SectionFilterID.ToString());
                    HttpContext.Current.Profile.SetPropertyValue("ManufacturerFilterID", m_ManufacturerFilterID.ToString());
                    HttpContext.Current.Profile.SetPropertyValue("DistributorFilterID", m_DistributorFilterID.ToString());
                    HttpContext.Current.Profile.SetPropertyValue("GenreFilterID", m_GenreFilterID.ToString());
                    HttpContext.Current.Profile.SetPropertyValue("VectorFilterID", m_VectorFilterID.ToString());
                    HttpContext.Current.Profile.SetPropertyValue("ProductTypeFilterID", m_ProductTypeFilterID.ToString());                    
                }

                m_EntityInstanceName = m_EntityHelper.m_TblMgr.CurrentName(n, m_SkinBase.ThisCustomer.LocaleSetting);

                HttpContext.Current.Profile.SetPropertyValue("LastViewedEntityName", m_EntitySpecs.m_EntityName);
                HttpContext.Current.Profile.SetPropertyValue("LastViewedEntityInstanceID", m_EntityInstanceID.ToString());
                HttpContext.Current.Profile.SetPropertyValue("LastViewedEntityInstanceName", m_EntityInstanceName);               

                #region Vortx Mobile Xml Package Modification
                m_XmlPackage = Vortx.MobileFramework.MobileXmlPackageController.XmlPackageHook(m_EntityHelper.m_TblMgr.CurrentField(n, "XmlPackage").ToLowerInvariant(),m_SkinBase.ThisCustomer);
                #endregion
                if (m_XmlPackage.Length == 0)
                {
                    m_XmlPackage = AppLogic.ro_DefaultEntityXmlPackage; // provide a default for backwards compatibility
                }


                String RunTimeParms = String.Format("EntityName={0}&EntityID={1}", m_EntitySpecs.m_EntityName, m_EntityInstanceID.ToString());

                RunTimeParms += String.Format("&CatID={0}", CommonLogic.IIF(m_EntitySpecs.m_EntityName.Trim().Equals("CATEGORY", StringComparison.InvariantCultureIgnoreCase), m_EntityInstanceID.ToString(), m_CategoryFilterID.ToString()));
                RunTimeParms += String.Format("&SecID={0}", CommonLogic.IIF(m_EntitySpecs.m_EntityName.Trim().Equals("SECTION", StringComparison.InvariantCultureIgnoreCase), m_EntityInstanceID.ToString(), m_SectionFilterID.ToString()));
                RunTimeParms += String.Format("&ManID={0}", CommonLogic.IIF(m_EntitySpecs.m_EntityName.Trim().Equals("MANUFACTURER", StringComparison.InvariantCultureIgnoreCase), m_EntityInstanceID.ToString(), m_ManufacturerFilterID.ToString()));
                RunTimeParms += String.Format("&DistID={0}", CommonLogic.IIF(m_EntitySpecs.m_EntityName.Trim().Equals("DISTRIBUTOR", StringComparison.InvariantCultureIgnoreCase), m_EntityInstanceID.ToString(), m_DistributorFilterID.ToString()));
                RunTimeParms += String.Format("&GenreID={0}", CommonLogic.IIF(m_EntitySpecs.m_EntityName.Trim().Equals("GENRE", StringComparison.InvariantCultureIgnoreCase), m_EntityInstanceID.ToString(), m_GenreFilterID.ToString()));
                RunTimeParms += String.Format("&VectorID={0}", CommonLogic.IIF(m_EntitySpecs.m_EntityName.Trim().Equals("VECTOR", StringComparison.InvariantCultureIgnoreCase), m_EntityInstanceID.ToString(), m_VectorFilterID.ToString()));
                RunTimeParms += String.Format("&ProductTypeFilterID={0}", m_ProductTypeFilterID.ToString());

                // CacheEntityPageHTML is an UNSUPPORTED and UNDOCUMENTED AppConfig
                // caching does NOT honor cross entity filtering, or other filters. Use it only on high traffic sites
                // with entity pages that do NOT vary by params other than those used in the CacheName string below.
                // if you are showing prices, they will remain the same during the cache duration (AppLogic.CacheDurationMinutes setting, usually 1 hr)
                String CacheName = String.Empty;


                m_SkinBase.SETitle = m_EntityHelper.m_TblMgr.CurrentFieldByLocale(n, "SETitle", m_SkinBase.ThisCustomer.LocaleSetting);
                if (m_SkinBase.SETitle.Length == 0)
                {
                    m_SkinBase.SETitle = Security.HtmlEncode(AppLogic.AppConfig("StoreName") + " - " + m_EntityInstanceName);
                }
                m_SkinBase.SEDescription = m_EntityHelper.m_TblMgr.CurrentFieldByLocale(n, "SEDescription", m_SkinBase.ThisCustomer.LocaleSetting);
                if (m_SkinBase.SEDescription.Length == 0)
                {
                    m_SkinBase.SEDescription = Security.HtmlEncode(m_EntityInstanceName);
                }
                m_SkinBase.SEKeywords = m_EntityHelper.m_TblMgr.CurrentFieldByLocale(n, "SEKeywords", m_SkinBase.ThisCustomer.LocaleSetting);
                if (m_SkinBase.SEKeywords.Length == 0)
                {
                    m_SkinBase.SEKeywords = Security.HtmlEncode(m_EntityInstanceName);
                }
                m_SkinBase.SENoScript = m_EntityHelper.m_TblMgr.CurrentFieldByLocale(n, "SENoScript", m_SkinBase.ThisCustomer.LocaleSetting);

                m_SkinBase.SectionTitle = Breadcrumb.GetEntityBreadcrumb(m_EntityInstanceID, m_EntityInstanceName, m_EntitySpecs.m_EntityName, m_SkinBase.ThisCustomer);

                 if (m_URLValidated)
                {
                    m_PageOutput = "<!-- XmlPackage: " + m_XmlPackage + " -->\n";
                    if (m_XmlPackage.Length == 0)
                    {
                        m_PageOutput += "<p><b><font color=red>XmlPackage format was chosen, but no XmlPackage was specified!</font></b></p>";
                    }
                    else
                    {
                        String s = null;
                        if (AppLogic.AppConfigBool("CacheEntityPageHTML"))
                        {
                            CacheName = String.Format("CacheEntityPageHTML|{0}|{1}|{2}|{3}|{4}|{5}|{6}",
                                                      m_EntitySpecs.m_EntityName,
                                                      m_EntityInstanceID.ToString(),
                                                      m_SkinBase.ThisCustomer.CustomerLevelID.ToString(),
                                                      m_SkinBase.ThisCustomer.LocaleSetting,
                                                      CommonLogic.QueryStringUSInt("PageNum").ToString(),
                                                      m_SkinBase.ThisCustomer.AffiliateID.ToString(),
													  Vortx.MobileFramework.MobileHelper.isMobile() ? "Mobile" : "Desktop"
                                );
							
                            s = (String)HttpContext.Current.Cache.Get(CacheName);
                            if (s != null)
                            {
                                s = "<!-- CacheEntityPageHTML -->" + s;
                            }
                            m_SkinBase.SectionTitle = (String)HttpContext.Current.Cache.Get(CacheName + "|SectionTitle");
                            m_SkinBase.SETitle = (String)HttpContext.Current.Cache.Get(CacheName + "|SETitle");
                            m_SkinBase.SEDescription = (String)HttpContext.Current.Cache.Get(CacheName + "|SEDescription");
                            m_SkinBase.SEKeywords = (String)HttpContext.Current.Cache.Get(CacheName + "|SEKeywords");
                            m_SkinBase.SENoScript = (String)HttpContext.Current.Cache.Get(CacheName + "|SENoScript");
                            if (m_SkinBase.SectionTitle == null)
                            {
                                m_SkinBase.SectionTitle = String.Empty;
                            }
                            if (m_SkinBase.SETitle == null)
                            {
                                m_SkinBase.SETitle = String.Empty;
                            }
                            if (m_SkinBase.SEDescription == null)
                            {
                                m_SkinBase.SEDescription = String.Empty;
                            }
                            if (m_SkinBase.SEKeywords == null)
                            {
                                m_SkinBase.SEKeywords = String.Empty;
                            }
                            if (m_SkinBase.SENoScript == null)
                            {
                                m_SkinBase.SENoScript = String.Empty;
                            }
                        }
                        if (s == null ||s.Length == 0)
                        {
                            using (XmlPackage2 p = new XmlPackage2(m_XmlPackage, m_SkinBase.ThisCustomer, m_SkinBase.SkinID, "", RunTimeParms, String.Empty, true))
                            {
                                s = AppLogic.RunXmlPackage(p, m_SkinBase.GetParser, m_SkinBase.ThisCustomer, m_SkinBase.SkinID, true, true);
                                if (p.SectionTitle != "")
                                {
                                    m_SkinBase.SectionTitle = p.SectionTitle;
                                }
                                if (p.SETitle != "")
                                {
                                    m_SkinBase.SETitle = p.SETitle;
                                }
                                if (p.SEDescription != "")
                                {
                                    m_SkinBase.SEDescription = p.SEDescription;
                                }
                                if (p.SEKeywords != "")
                                {
                                    m_SkinBase.SEKeywords = p.SEKeywords;
                                }
                                if (p.SENoScript != "")
                                {
                                    m_SkinBase.SENoScript = p.SENoScript;
                                }
                                if (AppLogic.AppConfigBool("CacheEntityPageHTML"))
                                {
                                    HttpContext.Current.Cache.Insert(CacheName, s, null, DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
                                    HttpContext.Current.Cache.Insert(CacheName + "|SectionTitle", p.SectionTitle, null, DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
                                    HttpContext.Current.Cache.Insert(CacheName + "|SETitle", p.SETitle, null, DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
                                    HttpContext.Current.Cache.Insert(CacheName + "|SEDescription", p.SEDescription, null, DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
                                    HttpContext.Current.Cache.Insert(CacheName + "|SEKeywords", p.SEKeywords, null, DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
                                    HttpContext.Current.Cache.Insert(CacheName + "|SENoScript", p.SENoScript, null, DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
                                }
                            }
                        }
                        m_PageOutput += s;
                    }
                }
            }
            AppLogic.eventHandler("ViewEntityPage").CallEvent("&ViewEntityPage=true");

            //check if the postback was caused by the TableOrderAddToCart button
            if (m_SkinBase.IsPostBack && isTableOrderAddToCartPostBack)
            {
                HandleTableOrderAddToCart();
            }

        }

        private string BuildQueryString()
        {
            HttpRequest rqst = HttpContext.Current.Request;
            StringBuilder QStr = new StringBuilder("?", 1024);
            bool first = true;
            string EntityIDName = string.Format("{0}id", m_EntityHelper.GetEntitySpecs.m_EntityName.ToLower());
            for (int i = 0; i < rqst.QueryString.Count; i++)
            {
                string key = rqst.QueryString.GetKey(i);
                if (String.Compare(key, EntityIDName, true) != 0 && String.Compare(key, "sename", true) != 0)
                {
                    if (!first)
                    {
                        QStr.Append("&");
                    }
                    QStr.AppendFormat("{0}={1}", key, rqst.QueryString[i]);
                    first = false;
                }
            }
            return CommonLogic.IIF(QStr.Length > 1, QStr.ToString(), "");
        }
        
        /// <summary>
        /// Handles the table order add to cart which was handled before by the tableorder_process.aspx .
        /// </summary>
        private void HandleTableOrderAddToCart()
        {
            CartTypeEnum CartType = CartTypeEnum.ShoppingCart;

            m_SkinBase.ThisCustomer.RequireCustomerRecord();
            int CustomerID = m_SkinBase.ThisCustomer.CustomerID;

            ShoppingCart cart = new ShoppingCart(1, m_SkinBase.ThisCustomer, CartType, 0, false);
            for (int i = 0; i < HttpContext.Current.Request.Form.Count; i++)
            {
                String FieldName = HttpContext.Current.Request.Form.Keys[i];
                String FieldVal = HttpContext.Current.Request.Form[HttpContext.Current.Request.Form.Keys[i]];
                if (FieldName.ToUpperInvariant().IndexOf("_VLDT") == -1 && (FieldName.ToLowerInvariant().StartsWith("price_") || FieldName.ToLowerInvariant().StartsWith("qty_")) && FieldVal.Trim().Length != 0)
                {
                    try // ignore errors, just add what items we can:
                    {
                        decimal CustomerEnteredPrice = Decimal.Zero;
                        int Qty = 0;
                        if (FieldName.ToLowerInvariant().StartsWith("price_"))
                        {
                            CustomerEnteredPrice = Currency.ConvertToBaseCurrency(Localization.ParseLocaleDecimal(FieldVal, m_SkinBase.ThisCustomer.LocaleSetting), m_SkinBase.ThisCustomer.CurrencySetting);
                        }
                        else
                        {
                            Qty = Localization.ParseNativeInt(FieldVal);
                        }
                        String[] flds = FieldName.Split('_');
                        int ProductID = Localization.ParseUSInt(flds[1]);
                        int VariantID = Localization.ParseUSInt(flds[2]);
                        int ColorIdx = Localization.ParseUSInt(flds[3]);
                        int SizeIdx = Localization.ParseUSInt(flds[4]);
                        if (Qty == 0)
                        {
                            Qty = 1;
                        }

                        String ChosenColor = String.Empty;
                        String ChosenSize = String.Empty;
                        String ChosenColorSKUModifier = String.Empty;
                        String ChosenSizeSKUModifier = String.Empty;

                        if (ColorIdx > -1 || SizeIdx > -1)
                        {
                            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
                            {
                                dbconn.Open();
                                using (IDataReader rs = DB.GetRS("select * from productvariant where VariantID=" + VariantID.ToString(), dbconn))
                                {
                                    rs.Read();
                                    ChosenColor = DB.RSFieldByLocale(rs, "Colors", Localization.GetDefaultLocale()).Split(',')[ColorIdx].Trim();
                                    if (DB.RSField(rs, "ColorSKUModifiers").Length != 0)
                                    {
                                        ChosenColorSKUModifier = DB.RSField(rs, "ColorSKUModifiers").Split(',')[ColorIdx].Trim();
                                    }

                                    ChosenSize = DB.RSFieldByLocale(rs, "Sizes", Localization.GetDefaultLocale()).Split(',')[SizeIdx].Trim();
                                    if (DB.RSField(rs, "SizeSKUModifiers").Length != 0)
                                    {
                                        ChosenSizeSKUModifier = DB.RSField(rs, "SizeSKUModifiers").Split(',')[SizeIdx].Trim();
                                    }
                                }
                            }
                        }

                        String TextOption = String.Empty;
                        cart.AddItem(m_SkinBase.ThisCustomer, m_SkinBase.ThisCustomer.PrimaryShippingAddressID , ProductID, VariantID, Qty, ChosenColor, ChosenColorSKUModifier, ChosenSize, ChosenSizeSKUModifier, TextOption, CartType, false, false, 0, CustomerEnteredPrice);
                    }
                    catch { }
                }
            }
            cart = null;


            String ReturnURL = m_SkinBase.Page.Request.Url.ToString();  
            if (AppLogic.AppConfig("AddToCartAction").Equals("STAY", StringComparison.InvariantCultureIgnoreCase) && ReturnURL.Length != 0)
            {
                HttpContext.Current.Response.Redirect(ReturnURL);
            }
            else
            {
                String previousEntityPage = SE.MakeObjectLink("Category", m_EntityInstanceID, String.Empty);
                String url = CommonLogic.IIF(CartType.Equals(CartTypeEnum.WishCart), "wishlist.aspx", "ShoppingCart.aspx?add=true&returnurl=" + previousEntityPage);
                HttpContext.Current.Response.Redirect(url);
            }
        }

        public void RenderContents(HtmlTextWriter writer)
        {
            writer.Write(m_PageOutput);
        }

        public string GetOutput()
        {
            return m_PageOutput;
        }
    }
}
