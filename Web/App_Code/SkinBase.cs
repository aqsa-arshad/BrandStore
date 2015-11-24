// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontLayout;
using Vortx.MobileFramework;

namespace AspDotNetStorefront
{
    public class SkinBase : System.Web.UI.Page
    {
        #region Vortx - OPC Speed

        private ShoppingCart _OPCShoppingCart;
        public ShoppingCart OPCShoppingCart
        {
            get
            {
                if (_OPCShoppingCart == null)
                    RefreshCart();
                return _OPCShoppingCart;
            }
        }
        public Customer OPCCustomer
        {
            get
            {
                return Customer.Current;
            }
        }

        public void RefreshCart()
        {
            _OPCShoppingCart = new ShoppingCart(OPCCustomer.SkinID, OPCCustomer, CartTypeEnum.ShoppingCart, 0, false);
        }

        #endregion

        private String m_ErrorMsg = String.Empty;
        private bool m_Editing = false;
        private bool m_DataUpdated = false;
      
        private bool m_DesignMode = false;
        private Customer m_AdminCustomer = null; // will only be set if this thread/page request was started in impersonation mode. Will be set to the admin customer who is doing the impersonation 
        private String m_SectionTitle = String.Empty;
        private string m_TemplateName = string.Empty;
        private String m_SETitle = String.Empty;
        private String m_SEDescription = String.Empty;
        private String m_SEKeywords = String.Empty;
        private String m_SENoScript = String.Empty;
        private String m_TemplateFN = String.Empty;
        private bool m_DisableContents = false;
        private String m_GraphicsColor = String.Empty;
        private String m_ContentsBGColor = String.Empty;
        private String m_PageBGColor = String.Empty;
        private String m_IGD = String.Empty; // impersonation customer guid for admin phone order entry display of product pages, etc...

        private System.Collections.Generic.Dictionary<string, EntityHelper> m_EntityHelpers = new System.Collections.Generic.Dictionary<string, EntityHelper>();
        private Parser m_Parser;
        private LayoutData m_thislayout;

        public SkinBase()
        {
            if (AppLogic.IsAdminSite && AppLogic.OnLiveServer() && AppLogic.UseSSL() && !CommonLogic.IsSecureConnection())
            {
                if (AppLogic.RedirectLiveToWWW())
                {
                    HttpContext.Current.Response.Redirect("https://www." + AppLogic.LiveServer() + CommonLogic.ServerVariables("PATH_INFO") + "?" + CommonLogic.ServerVariables("QUERY_STRING"));
                }
                else
                {
                    HttpContext.Current.Response.Redirect("https://" + AppLogic.LiveServer() + CommonLogic.ServerVariables("PATH_INFO") + "?" + CommonLogic.ServerVariables("QUERY_STRING"));
                }
            }

            if (!AppLogic.IsAdminSite && AppLogic.OnLiveServer())
            {
                if (AppLogic.RedirectLiveToWWW())
                {
                    if (!HttpContext.Current.Request.Url.Host.StartsWith("www."))
                    {
                        if (CommonLogic.IsSecureConnection())
                        {
                            HttpContext.Current.Response.Redirect("https://www." + AppLogic.LiveServer() + CommonLogic.ServerVariables("PATH_INFO") + "?" + CommonLogic.ServerVariables("QUERY_STRING"));
                        }
                        else
                        {
                            HttpContext.Current.Response.Redirect("http://www." + AppLogic.LiveServer() + CommonLogic.ServerVariables("PATH_INFO") + "?" + CommonLogic.ServerVariables("QUERY_STRING"));
                        }
                    }
                }
            }
            m_DesignMode = (HttpContext.Current == null);

            
            if (!m_DesignMode)
            {
                m_EntityHelpers.Add("Category", AppLogic.CategoryStoreEntityHelper[0]);
                m_EntityHelpers.Add("Section", AppLogic.SectionStoreEntityHelper[0]);
                m_EntityHelpers.Add("Manufacturer", AppLogic.ManufacturerStoreEntityHelper[0]);
                m_EntityHelpers.Add("Distributor", AppLogic.DistributorStoreEntityHelper[0]);
                m_EntityHelpers.Add("Library", AppLogic.LibraryStoreEntityHelper[0]);
                m_EntityHelpers.Add("Genre", AppLogic.GenreStoreEntityHelper[0]);
                m_EntityHelpers.Add("Vector", AppLogic.VectorStoreEntityHelper[0]);
            }
        }

        /// <summary>
        /// Compares the current user agent of the request against a list of 
        /// known mobile user agents stored in the database
        /// </summary>
        public bool CheckUserAgentForMobile
        {
            get
            {
                using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
                {
                    conn.Open();



                    String usrAgent = "";
                    if (!String.IsNullOrEmpty(HttpContext.Current.Request.UserAgent))
	                {
		                usrAgent = HttpContext.Current.Request.UserAgent.ToLowerInvariant();
	                }

                    using (IDataReader rs = DB.GetRS("select UserAgent from dbo.MobileDevice with(NOLOCK)", conn))
                    {
                        while (rs.Read())
                        {
                            String mDevice = DB.RSField(rs, "UserAgent").ToLowerInvariant();

                            if (usrAgent.Contains(mDevice))
                            {
                                rs.Close();
                                rs.Dispose();

                                return true;
                            }
                        }
                    }
                    conn.Close();
                    conn.Dispose();
                }

                return false;
            }
        }

        /// <summary>
        /// Used to set the master page when using template switching or page-based templates
        /// </summary>
        /// <returns>
        /// The name of the template to use.  To utilize this you must override OverrideTemplate 
        /// in a page that inherits from SkinBase where you're trying to change the master page
        /// </returns>
        protected virtual string OverrideTemplate()
        {
            return "template.master";
        }

        /// <summary>
        /// Determines which masterpage skin to use to render the site to the browsing customer
        /// </summary>
        /// <returns>The name of the file that is the masterpage</returns>
        private string GetTemplateName()
        {
            String templateName = String.Empty;

            templateName = CommonLogic.QueryStringCanBeDangerousContent("template");

            //First attempt to find a template with this explicit page name (eg. p-21-MyProduct.aspx)
            if (templateName == null || templateName.Length == 0)
            {
                // undocumented feature:
                templateName = AppLogic.AppConfig("Template" + CommonLogic.GetThisPageName(false));
            }
            //Next, try to find a generic template that corresponds with the type of page being viewed
 	 	 	if (templateName == null || templateName.Length == 0)
 	 	 	{
 	 	 	    if (IsProductPage)
 	 	 	    {
 	 	 	        templateName = AppLogic.AppConfig("TemplateShowProduct.aspx");
 	 	 	    }
 	 	 	    else if (IsEntityPage)
 	 	 	    {
 	 	 	        templateName = AppLogic.AppConfig("TemplateShow" + EntityType + ".aspx");
 	 	 	    }
 	 	 	}
 	 	 	// A page inheriting from SkinBase can Override OverrideTemplate() and set the template
 	 	 	// This is used on entity pages to set the template to the value specified on the edit entity page in the admin site
 	 	 	string overridingTemplate = OverrideTemplate();
 	 	 	if (overridingTemplate != null && overridingTemplate.Trim().Length > 0 && !overridingTemplate.Equals("template.master", StringComparison.OrdinalIgnoreCase))
 	 	 	{
 	 	 	    templateName = overridingTemplate;
 	 	 	}

            if (templateName == null || templateName.Length == 0)
            {
                templateName = "template";
            }

            // sanity check to ensure that OverrideTemplate wasn't overridden in a page and
            // then set to an empty string
            if (templateName.Trim().Length == 0 || !CommonLogic.FileExists(SkinRoot + templateName))
            {
                templateName = "template.master";
            }

            // only start looking for localized masterpages if the GlobalConfig AllowTemplateSwitchingByLocale
            // is set to true, so we don't unnecessarily check for multiple files on every page load
            if (AppLogic.GlobalConfigBool("AllowTemplateSwitchingByLocale"))
            {
                String templateLocaleName = null;
                String _url = String.Empty;
                
                // try customer locale:
                templateLocaleName = templateName.Replace(".master", "." + ThisCustomer.LocaleSetting + ".master");
                _url = Path.Combine(SkinRoot, templateLocaleName);
                m_TemplateFN = CommonLogic.SafeMapPath(_url);

                // if the file doesn't exist, check the default locale
                if (!CommonLogic.FileExists(m_TemplateFN))
                {
                    // try default store locale path:
                    templateLocaleName = templateName.Replace(".master", "." + Localization.GetDefaultLocale() + ".master");
                    _url = Path.Combine(SkinRoot, templateLocaleName);
                    m_TemplateFN = CommonLogic.SafeMapPath(_url);
                }

                // if we found a locale match, use the name of the localized masterpage
                if (CommonLogic.FileExists(m_TemplateFN))
                {
                    templateName = templateLocaleName;
                }
            }

            m_TemplateName = templateName;

            return templateName;
        }

        protected virtual void FindLocaleStrings(Control c)
        {
            try
            {
                System.Web.UI.WebControls.Image i = c as System.Web.UI.WebControls.Image;
                if (i != null)
                {
                    if (i.ImageUrl.IndexOf("(!") >= 0)
                    {
                        i.ImageUrl = AppLogic.LocateImageURL(i.ImageUrl.Replace("(!SKINID!)", SkinID.ToString()).Replace("(!", string.Empty).Replace("!)", string.Empty), ThisCustomer.LocaleSetting);
                    }
                    if (i.AlternateText.IndexOf("(!") >= 0)
                    {
                        i.AlternateText = AppLogic.GetString(i.AlternateText, SkinID, ThisCustomer.LocaleSetting);
                    }
                }
                else
                {
                    System.Web.UI.WebControls.ImageButton b = c as System.Web.UI.WebControls.ImageButton;
                    if (b != null)
                    {
                        if (b.ImageUrl.IndexOf("(!") >= 0)
                       {
                            b.ImageUrl = AppLogic.LocateImageURL(b.ImageUrl.Replace("(!SKINID!)", SkinID.ToString()).Replace("(!", string.Empty).Replace("!)", string.Empty), ThisCustomer.LocaleSetting);
                        }
                    }
                    else
                    {
                        IEditableTextControl e = c as IEditableTextControl;
                        if (e != null)
                        {
                            if (!(e is ListControl))
                            {
                                e.Text = AppLogic.GetString(e.Text.Replace("(!SKINID!)", SkinID.ToString()).Replace("(!", "").Replace("!)", ""), SkinID, ThisCustomer.LocaleSetting);
                            }
                        }
                        else
                        {
                            IValidator v = c as IValidator;
                            if (v != null)
                            {
                                v.ErrorMessage = AppLogic.GetString(v.ErrorMessage.Replace("(!SKINID!)", SkinID.ToString()).Replace("(!", "").Replace("!)", ""), SkinID, ThisCustomer.LocaleSetting);
                            }
                            ITextControl t = c as ITextControl;
                            if (t != null)
                            {
                                t.Text = AppLogic.GetString(t.Text.Replace("(!SKINID!)", SkinID.ToString()).Replace("(!", "").Replace("!)", ""), SkinID, ThisCustomer.LocaleSetting);
                            }
                            Button b2 = c as Button;
                            if (b2 != null)
                            {
                                b2.Text = AppLogic.GetString(b2.Text.Replace("(!SKINID!)", SkinID.ToString()).Replace("(!", "").Replace("!)", ""), SkinID, ThisCustomer.LocaleSetting);
                            }
                            LinkButton l = c as LinkButton;
                            if (l != null)
                            {
                                l.Text = AppLogic.GetString(l.Text.Replace("(!SKINID!)", SkinID.ToString()).Replace("(!", "").Replace("!)", ""), SkinID, ThisCustomer.LocaleSetting);
                            }
                            HyperLink h = c as HyperLink;
                            if (h != null)
                            {
                                h.Text = AppLogic.GetString(h.Text.Replace("(!SKINID!)", SkinID.ToString()).Replace("(!", "").Replace("!)", ""), SkinID, ThisCustomer.LocaleSetting);
                            }
                            RadioButton r = c as RadioButton;
                            if (r != null)
                            {
                                r.Text = AppLogic.GetString(r.Text.Replace("(!SKINID!)", SkinID.ToString()).Replace("(!", "").Replace("!)", ""), SkinID, ThisCustomer.LocaleSetting);
                            }
                        }
                    }
                }
                if (c.HasControls())
                {
                    foreach (Control cx in c.Controls)
                    {
                        FindLocaleStrings(cx);
                    }
                }
            }
            catch { } // for admin site, a hack really, will fix with master pages
        }

        /// <summary>
        /// Gets the ContentArea panel
        /// </summary>
        /// <returns></returns>
        private Panel GetContentPanel()
        {
            // Get the prerequisite panel container through the MasterPage
            Panel pnlContent = (this.Master as MasterPageBase).ContentPanel;
            return pnlContent;
        }

        /// <summary>
        /// Gets the System.Web.UI.Page type
        /// </summary>
        /// <returns></returns>
        private Type GetPageType()
        {
            Type pageType = this.GetType();

            while (pageType != typeof(Page))
            {
                // go down the inheritance hierarchy
                pageType = pageType.BaseType;
            }

            return pageType;
        }


        private void ApplyTheme()
        {
            string chosenTheme = "Skin_" + this.SkinID.ToString();

            if (CommonLogic.IsStringNullOrEmpty(this.Theme) ||
                false == this.Theme.Equals(chosenTheme, StringComparison.InvariantCultureIgnoreCase))
            {
                this.Theme = chosenTheme;
            }
        }

        private void ParseLocaleStrings(Control container)
        {
            // handle localization routine
            foreach (Control c in container.Controls)
            {
                FindLocaleStrings(c);
            }
        }


        protected override void OnPreInit(EventArgs e)
        {
            if (HttpContext.Current != null)
            {
                //Have to call GetPropertyValue once before you actually need it to initialize the PropertyValues collection
                HttpContext.Current.Profile.GetPropertyValue("SkinID").ToString();

                #region SkinID
                //If it's mobile, bypass all the rest
                if (!AppLogic.IsAdminSite && MobileHelper.isMobile())
                {
                    MobileHelper.SetCustomerToMobileSkinId(ThisCustomer);
                    SkinID = ThisCustomer.SkinID;
                }
                else
                {
                    //SkinId querystring overrides everything but mobile
                    if (CommonLogic.QueryStringUSInt("skinid") > 0)
                    {
                        SkinID = CommonLogic.QueryStringUSInt("skinid");

                        //Customer has a querystring so save this to the profile.
                        if (HttpContext.Current.Profile != null)
                            HttpContext.Current.Profile.SetPropertyValue("SkinID", this.SkinID.ToString());
                    }
					//Check to see if we are previewing the skin
					else if (CommonLogic.QueryStringUSInt("previewskinid") > 0)
					{
						SkinID = CommonLogic.QueryStringUSInt("previewskinid");

						//Customer has a preview querystring so save this to the profile.
						if (HttpContext.Current.Profile != null)
							HttpContext.Current.Profile.SetPropertyValue("PreviewSkinID", this.SkinID.ToString());
					}
					//Use the preview profile value if we have one
					else if (HttpContext.Current.Profile != null
                        && HttpContext.Current.Profile.PropertyValues["PreviewSkinID"] != null
                        && CommonLogic.IsInteger(HttpContext.Current.Profile.GetPropertyValue("PreviewSkinID").ToString()))
					{
						int skinFromProfile = int.Parse(HttpContext.Current.Profile.GetPropertyValue("PreviewSkinID").ToString());
						if (skinFromProfile > 0)
						{
							SkinID = skinFromProfile;
						}
					}
                    //Pull the skinid from the current profile
                    else if (HttpContext.Current.Profile != null && CommonLogic.IsInteger(HttpContext.Current.Profile.GetPropertyValue("SkinID").ToString()))
                    {
                        int skinFromProfile = int.Parse(HttpContext.Current.Profile.GetPropertyValue("SkinID").ToString());
                        if (skinFromProfile > 0)
                        {
                            SkinID = skinFromProfile;
                        }
                    }
                }

                //Now save the skinID to the customer record.  This is not used OOB.
                if (ThisCustomer.SkinID != this.SkinID)
                {
                    ThisCustomer.SkinID = this.SkinID;
                    ThisCustomer.UpdateCustomer(new SqlParameter[] { new SqlParameter("SkinID", this.SkinID) });
                }
                #endregion

                if (CommonLogic.QueryStringUSInt("affiliateid") > 0)
                {
                    HttpContext.Current.Profile.SetPropertyValue("AffiliateID", CommonLogic.QueryStringUSInt("affiliateid").ToString());
                }

                if (HttpContext.Current.Request.UrlReferrer != null && HttpContext.Current.Request.UrlReferrer.Authority != HttpContext.Current.Request.Url.Authority)
                {
                    HttpContext.Current.Profile.SetPropertyValue("Referrer", HttpContext.Current.Request.UrlReferrer.ToString());
                }

                // don't fire disclaimer logic on admin pages
                if (!AppLogic.IsAdminSite && CommonLogic.QueryStringCanBeDangerousContent("ReturnURL").IndexOf(AppLogic.AppConfig("AdminDir")) == -1 && (AppLogic.AppConfigBool("SiteDisclaimerRequired") && CommonLogic.CookieCanBeDangerousContent("SiteDisclaimerAccepted", true).Length == 0))
                {
                    String ThisPageURL = CommonLogic.GetThisPageName(true) + "?" + CommonLogic.ServerVariables("QUERY_STRING");
                    Response.Redirect("disclaimer.aspx?returnURL=" + Server.UrlEncode(ThisPageURL));
                }

                #region Impersonation
                bool IGDQueryClear = false;
                m_IGD = CommonLogic.QueryStringCanBeDangerousContent("IGD").Trim();
                if (m_IGD.Length == 0 && CommonLogic.ServerVariables("QUERY_STRING").IndexOf("IGD=") != -1)
                {
                    m_IGD = String.Empty; // there was IGD={blank} in the query string, so forcefully clear IGD!
                    IGDQueryClear = true;
                }
                bool IsStartOfImpersonation = m_IGD.Length != 0; // the url invocation starts the impersonation only!

                if (!IGDQueryClear && m_IGD.Length == 0)
                {
                    if (ThisCustomer.IsAdminUser)
                    {
                        // pull out the impersonation IGD from the customer session, if any
                        m_IGD = ThisCustomer.ThisCustomerSession["IGD"];
                    }
                }

                if (IGDQueryClear)
                {
                    // forcefully clear any IGD for this customer, just to be safe!
                    ThisCustomer.ThisCustomerSession["IGD"] = "";
                    ThisCustomer.ThisCustomerSession["IGD_EDITINGORDER"] = "";
                }

                Customer PhoneCustomer = null;
                if (m_IGD.Length != 0)
                {
                    if (ThisCustomer.IsAdminUser)
                    {
                        try
                        {
                            Guid IGD = new Guid(m_IGD);
                            PhoneCustomer = new Customer(IGD);
                            PhoneCustomer.IsImpersonated = true;
                        }
                        catch
                        {
                            ThisCustomer.ThisCustomerSession["IGD"] = "";
                            ThisCustomer.ThisCustomerSession["IGD_EDITINGORDER"] = "";
                            m_IGD = string.Empty;
                        }
                    }
                    if (PhoneCustomer != null && PhoneCustomer.HasCustomerRecord)
                    {
                        int ImpersonationTimeoutInMinutes = AppLogic.AppConfigUSInt("ImpersonationTimeoutInMinutes");
                        if (ImpersonationTimeoutInMinutes == 0)
                        {
                            ImpersonationTimeoutInMinutes = 20;
                        }
                        if (PhoneCustomer.ThisCustomerSession.LastActivity >= DateTime.Now.AddMinutes(-ImpersonationTimeoutInMinutes))
                        {
                            ThisCustomer.ThisCustomerSession["IGD"] = IGD;
                            m_AdminCustomer = ThisCustomer; // save the owning admin user doing the impersonation here
                            ThisCustomer = PhoneCustomer; // build the impersonation customer the phone order customer
                            bool IsAdmin = CommonLogic.ApplicationBool("IsAdminSite");

                            if (!HttpContext.Current.Items.Contains("IsBeingImpersonated"))
                            {
                                HttpContext.Current.Items.Add("IsBeingImpersonated", "true");
                            }
                        }
                        else
                        {
                            if (HttpContext.Current.Items.Contains("IsBeingImpersonated"))
                            {
                                HttpContext.Current.Items["IsBeingImpersonated"] = "false";
                            }
                            ThisCustomer.ThisCustomerSession["IGD"] = "";
                            ThisCustomer.ThisCustomerSession["IGD_EDITINGORDER"] = "";
                            m_IGD = string.Empty;
                            //Response.Redirect("t-phoneordertimeout.aspx");
                            Response.Redirect(SE.MakeDriverLink("phoneordertimeout"));
                        }
                    }
                }
                #endregion

                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(Localization.GetDefaultLocale());
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(ThisCustomer.LocaleSetting);

                m_TemplateName = GetTemplateName();

                if (!AppLogic.IsAdminSite)
                {
                    ThisCustomer = MobileRedirectController.SkinBaseHook(SkinID, ThisCustomer);
                    if (SkinID == Vortx.Data.Config.MobilePlatform.SkinId && MobileHelper.isMobile())
                        m_TemplateName = "template.master";
                }

				//needs to come after the mobile check
                m_Parser = new Parser(m_EntityHelpers, SkinID, ThisCustomer);

                String SkinDirectory = String.Empty;
                String PageTheme = String.Empty;

                SkinDirectory = "Skin_" + this.SkinID.ToString();
                PageTheme = "Skin_" + this.SkinID.ToString();

                if (!m_TemplateName.EndsWith(".master", StringComparison.OrdinalIgnoreCase))
                {
                    m_TemplateName = m_TemplateName + ".master";
                }

                this.MasterPageFile = "~/App_Templates/" + SkinDirectory + "/" + m_TemplateName;
                this.Theme = PageTheme;

                if (!CommonLogic.FileExists(this.MasterPageFile))
                {
                    
                    this.SkinID = AppLogic.DefaultSkinID();

                    m_TemplateName = "template.master";
                    SkinDirectory = "Skin_" + this.SkinID.ToString();
                    PageTheme = "Skin_" + this.SkinID.ToString();

                    this.MasterPageFile = "~/App_Templates/" + SkinDirectory + "/" + m_TemplateName;
                    this.Theme = PageTheme;
                }
            }

            base.OnPreInit(e);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
        } 

        public string CreateContent()
        {
            StringBuilder tmpS = new StringBuilder(25000);

            //Create a literal to contain RenderContents
            StringWriter sw = new StringWriter();
            HtmlTextWriter htw = this.CreateHtmlTextWriter(sw);
            RenderContents(htw);
            htw.Flush();
            tmpS.Append(sw.ToString());

            htw.Close();
            sw.Dispose();

            return tmpS.ToString();
        }

        /// <summary>
        /// Responsible for replacing localized strings in child controls
        /// </summary>
        /// <param name="controls"></param>
        private void IterateControls(ControlCollection controls)
        {
            for (int i = 0; i < controls.Count; i++)
            {
                Control c = controls[i];
                if (c.ID != "PageContent")
                {
                    ProcessControl(c, true);
                }
            }
        }

        private void ProcessControl(Control ctl, bool includeChildren)
        {
            IEditableTextControl e = ctl as IEditableTextControl;
            if (e != null)
            {
                e.Text = ReplaceTokens(e.Text);
            }
            else
            {
                ITextControl t = ctl as ITextControl;
                if (t != null)
                {
                    t.Text = ReplaceTokens(t.Text);
                }
            }
            IValidator v = ctl as IValidator;
            if (v != null)
            {
                v.ErrorMessage = ReplaceTokens(v.ErrorMessage);
            }

            //
            // TODO: Add other controls which might have replaceable properties here
            //
            if (includeChildren && ctl.HasControls())
            {
                IterateControls(ctl.Controls);
            }
        }

        /// <summary>
        /// Method responsible for setting dynamic page properties, as well as replacing parser tokens in child controls
        /// </summary>
        /// <param name="writer"></param>
        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            if (this.SETitle.Length == 0)
            {
                this.SETitle = AppLogic.AppConfig("SE_MetaTitle");
            }
            if (this.SEDescription.Length == 0)
            {
                this.SEDescription = AppLogic.AppConfig("SE_MetaDescription");
            }
            if (this.SEKeywords.Length == 0)
            {
                this.SEKeywords = AppLogic.AppConfig("SE_MetaKeywords");
            }
            if (m_SENoScript.Length == 0)
            {
                m_SENoScript = AppLogic.AppConfig("SE_MetaNoScript");
            }

            if (AppLogic.GlobalConfigBool("ShowStoreHint"))
            {
                var stores = Store.GetStoreList();
                Store selectedStore = null;
                selectedStore = stores.FirstOrDefault(store => store.StoreID.Equals(AppLogic.StoreID()));
                if (selectedStore != null)
                {
                    this.SETitle = "[{0}] - ".FormatWith(selectedStore.Name) + this.SETitle;
                }
            }

            //IterateControls(Controls);
            base.Render(writer);
        }

        private const string OLD_TEMPLATE_EXTENSION = ".ascx";
        private const string MASTER_TEMPLATE_EXTENSION = ".master";        

        private string ReplaceTokens(String s)
        {
            if (s.IndexOf("(!") == -1)
            {
                return s;
            }
            String tmpS = String.Empty;
            // process SKIN specific tokens here only:
            s = s.Replace("(!SECTION_TITLE!)", SectionTitle);
            if (SectionTitle.Length != 0)
            {
                s = s.Replace("(!SUPERSECTIONTITLE!)", "<div align=\"left\"><span class=\"SectionTitleText\">" + SectionTitle + "</span><small>&nbsp;</small></div>");
                if (AppLogic.IsAdminSite)
                {
                    s = s.Replace("class=\"SectionTitleText\"", "class=\"breadCrumb3\"");
                }
            }
            else
            {
                s = s.Replace("(!SUPERSECTIONTITLE!)", "");
            }

            s = s.Replace("(!METATITLE!)", CommonLogic.IIF(CommonLogic.StringIsAlreadyHTMLEncoded(m_SETitle), m_SETitle, HttpContext.Current.Server.HtmlEncode(m_SETitle)));
            s = s.Replace("(!METADESCRIPTION!)", CommonLogic.IIF(CommonLogic.StringIsAlreadyHTMLEncoded(m_SEDescription), m_SEDescription, HttpContext.Current.Server.HtmlEncode(m_SEDescription)));
            s = s.Replace("(!METAKEYWORDS!)", CommonLogic.IIF(CommonLogic.StringIsAlreadyHTMLEncoded(m_SEKeywords), m_SEKeywords, HttpContext.Current.Server.HtmlEncode(m_SEKeywords)));
            s = s.Replace("(!SENOSCRIPT!)", m_SENoScript);
            if (m_GraphicsColor.Length == 0)
            {
                m_GraphicsColor = AppLogic.AppConfig("GraphicsColorDefault");
                if (m_GraphicsColor.Length == 0)
                {
                    m_GraphicsColor = String.Empty;
                }
            }
            if (m_ContentsBGColor.Length == 0)
            {
                m_ContentsBGColor = AppLogic.AppConfig("ContentsBGColorDefault");
                if (m_ContentsBGColor.Length == 0)
                {
                    m_ContentsBGColor = "#FFFFFF";
                }
            }
            if (m_PageBGColor.Length == 0)
            {
                m_PageBGColor = AppLogic.AppConfig("PageBGColorDefault");
                if (m_PageBGColor.Length == 0)
                {
                    m_PageBGColor = "#FFFFFF";
                }
            }
            s = s.Replace("(!GRAPHICSCOLOR!)", m_GraphicsColor);
            s = s.Replace("(!CONTENTSBGCOLOR!)", m_ContentsBGColor);
            s = s.Replace("(!PAGEBGCOLOR!)", m_PageBGColor);
            s = GetParser.ReplaceTokens(s);
            return s;
        }

        protected virtual void RenderContents(System.Web.UI.HtmlTextWriter writer) { }

        public LayoutData ThisLayout
        {
            get { return m_thislayout; }
            set { m_thislayout = value; }
        }

        private Customer m_ThisCustomer;
        public Customer ThisCustomer
        {
            get
            {
                if (m_ThisCustomer == null)
                    m_ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;

                return m_ThisCustomer;
            }
            set
            {
                m_ThisCustomer = value;
            }
        }

        public Customer AdminImpersonatingCustomer
        {
            get
            {
                return m_AdminCustomer;
            }
        }

        public bool IsInImpersonation
        {
            get
            {
                return m_AdminCustomer != null;
            }
        }

        // returns old ordernumber (the one being edited) if this was an impersonation "edit order" page or 0 if not an edit impersonation
        public int EditingOrderImpersonation
        {
            get
            {
                int EditingOrderNumber = 0;
                if (m_AdminCustomer != null)
                {
                    EditingOrderNumber = m_AdminCustomer.ThisCustomerSession.SessionUSInt("IGD_EDITINGORDER");
                }
                return EditingOrderNumber;
            }
        }

        public Parser GetParser
        {
            get
            {
                return m_Parser;
            }
        }

        /// <summary>
        /// Gets or sets the SectionTitle text
        /// </summary>
        public String SectionTitle
        {
            get
            {
                if (AppLogic.IsAdminSite)
                {
                    return m_SectionTitle;
                }
                else
                {
                    if (this.Master is MasterPageBase)
                    {
                        return (this.Master as MasterPageBase).SectionTitleText;
                    }

                    return string.Empty;
                }
            }
            set
            {
                if (AppLogic.IsAdminSite)
                {
                    m_SectionTitle = value;
                }
                else
                {
                    if (this.Master is MasterPageBase)
                    {
                        (this.Master as MasterPageBase).SectionTitleText = value;
                    }
                }
            }
        }

        public String ErrorMsg
        {
            get
            {
                return m_ErrorMsg;
            }
            set
            {
                m_ErrorMsg = value;
            }
        }

        public String SETitle
        {
            get
            {
                if (AppLogic.IsAdminSite)
                {
                    return m_SETitle;
                }
                else
                {
                    if (CommonLogic.IsStringNullOrEmpty(this.Title))
                    {
                        return string.Empty;
                    }

                    return this.Title;
                }
            }
            set
            {
                if (AppLogic.IsAdminSite)
                {
                    m_SETitle = value;
                }
                else
                {
                    this.Title = value;
                }
            }
        }

        public String IGD
        {
            get
            {
                return m_IGD;
            }
        }

        private const string META_KEYWORDS = "keywords";
        private const string META_DESCRIPTION = "description";

        public String SEKeywords
        {
            get
            {
                if (AppLogic.IsAdminSite)
                {
                    return m_SEKeywords;
                }
                else
                {
                    return GetMetaContent(META_KEYWORDS);
                }
            }
            set
            {
                if (AppLogic.IsAdminSite)
                {
                    m_SEKeywords = value;
                }
                else
                {
                    SetMetaContent(META_KEYWORDS, value);
                }
            }
        }

        public String SEDescription
        {
            get
            {
                if (AppLogic.IsAdminSite)
                {
                    return m_SEDescription;
                }
                else
                {
                    return GetMetaContent(META_DESCRIPTION);
                }
            }
            set
            {
                if (AppLogic.IsAdminSite)
                {
                    m_SEDescription = value;
                }
                else
                {
                    SetMetaContent(META_DESCRIPTION, value);
                }
            }
        }

        public String SENoScript
        {
            get
            {
                return m_SENoScript;
            }
            set
            {
                m_SENoScript = value;
            }
        }

        public String ContentsBGColor
        {
            get
            {
                return m_ContentsBGColor;
            }
            set
            {
                m_ContentsBGColor = value;
            }
        }

        public String PageBGColor
        {
            get
            {
                return m_PageBGColor;
            }
            set
            {
                m_PageBGColor = value;
            }
        }

        public String GraphicsColor
        {
            get
            {
                return m_GraphicsColor;
            }
            set
            {
                m_GraphicsColor = value;
            }
        }

        public bool Editing
        {
            get
            {
                return m_Editing;
            }
            set
            {
                m_Editing = value;
            }
        }

        public bool DisableContents
        {
            get
            {
                return m_DisableContents;
            }
            set
            {
                m_DisableContents = value;
            }
        }

        public bool DataUpdated
        {
            get
            {
                return m_DataUpdated;
            }
            set
            {
                m_DataUpdated = value;
            }
        }

        private int m_SkinID = 0;
        new public int SkinID
        {
            get
            {
                if (m_SkinID == 0)
                    m_SkinID = AppLogic.GetStoreSkinID(AppLogic.StoreID());

                return m_SkinID;
            }
            set
            {
                m_SkinID = value;
            }
        }

        public string SkinRoot
        {
            get
            {
                if (AppLogic.IsAdminSite)
                {
                    return "~/App_Templates/Admin_Default/";
                }
                else
                {
                    return String.Format("~/App_Templates/Skin_{0}/", this.SkinID);
                }
            }
        }

        public System.Collections.Generic.Dictionary<string, EntityHelper> EntityHelpers
        {
            get
            {
                return m_EntityHelpers;
            }
        }

        public string SkinImages
        {
            get
            {
                return String.Format("skins/skin_{0}/images/", this.SkinID);
            }
        }

        public static void RequireSecurePage()
        {
            AppLogic.RequireSecurePage();
        }

        public void GoNonSecureAgain()
        {
            if (AppLogic.UseSSL() && AppLogic.OnLiveServer() && CommonLogic.IsSecureConnection() && !IsInImpersonation)
            {
                
                // let's check the Items collection
                // wherein we stashed the original requested page before the url has been rewritten
                // during begin-request
                HttpContext ctx = HttpContext.Current ;
                if (ctx != null &&
                    ctx.Items["RequestedPage"] != null)
                {
                    string originalUrl = ctx.Items["RequestedPage"].ToString();
                    string redirectUrl = AppLogic.GetStoreHTTPLocation(false) + originalUrl;

                    if (ctx.Items["RequestedQuerystring"] != null)
                    {
                        string originalQueryString = ctx.Items["RequestedQuerystring"].ToString();
                        if (!string.IsNullOrEmpty(originalQueryString))
                        {
                            redirectUrl += originalQueryString;
                        }
                    }

                    ctx.Response.Redirect(redirectUrl);
                }
                else
                {
                    // regular non-url rewrited format
                    ctx.Response.Redirect(AppLogic.GetStoreHTTPLocation(false) + CommonLogic.GetThisPageName(false) + "?" + CommonLogic.ServerVariables("QUERY_STRING"));
                }     
            }
        }

        public void RequireCustomerRecord()
        {
            if (!m_ThisCustomer.HasCustomerRecord)
            {
                m_ThisCustomer.RequireCustomerRecord();
            }
        }

        public void RequiresLogin(String ReturnURL)
        {
            bool Checkout = CommonLogic.QueryStringBool("checkout");
            if (!m_ThisCustomer.IsRegistered)
            {
                Response.Redirect("signin.aspx?checkout=" + Checkout.ToString().ToLower() +"&returnurl=" + Server.UrlEncode(ReturnURL));
            }
        }

        public void SetMetaTags(String SETitle, String SEKeywords, String SEDescription, String SENoScript)
        {
            this.SETitle = SETitle;
            this.SEDescription = SEKeywords;
            this.SEKeywords = SEDescription;
            m_SENoScript = SENoScript;
        }

        private string GetMetaContent(string name)
        {
            if (this.Header != null)
            {
                foreach (Control c in this.Header.Controls)
                {
                    HtmlMeta meta = c as HtmlMeta;
                    if (meta != null &&
                        meta.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return meta.Content;
                    }
                }
            }

            return string.Empty;
        }

        private void SetMetaContent(string name, string value)
        {
            if (this.Header != null)
            {
                foreach (Control c in this.Header.Controls)
                {
                    HtmlMeta meta = c as HtmlMeta;
                    if (meta != null &&
                        meta.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        meta.Content = value;
                    }
                }
            }
        }

        /// <summary>
        /// When overridden provides central access to determine if a page is a product page or not
        /// </summary>
        public virtual bool IsProductPage
        {
            get { return false; }
        }

        /// <summary>
        /// When overridden provides central access to determine if a page is an entity page or not
        /// </summary>
        public virtual bool IsEntityPage
        {
            get { return false; }
        }

        /// <summary>
        /// When overridden provides central access to the type of entity page
        /// </summary>
        public virtual String EntityType
        {
            get { return String.Empty; }
        }

        /// <summary>
        /// When overridden provides central access to determine if a page is a topic page or not
        /// </summary>
        public virtual bool IsTopicPage
        {
            get { return false; }
        }

        /// <summary>
        /// When overridden provides central access to the ID of the entity or object type for quick edits
        /// </summary>
        public virtual int PageID
        {
            get { return 0; }
        }

        /// <summary>
        /// Gets whether to require asp.net ajax script manager
        /// </summary>
        public virtual bool RequireScriptManager
        {
            get
            {
                // true if layouts are enabled and an admin user is logged in
                // in order to use quick edits
                if (ThisCustomer.IsAdminUser && AppLogic.AppConfigBool("Layouts.Enabled"))
                {
                    return true;
                }

                // false by default since not all pages require this functionality
                return false;
            }
        }


        /// <summary>
        /// If overridden, provides access to the script manager to render script and services references
        /// </summary>
        /// <param name="scrptMgr"></param>
        public virtual void RegisterScriptAndServices(ScriptManager scrptMgr) { }
    }
}
