// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.ComponentModel;
using System.Xml.Linq;
using System.Data.SqlClient;
using System.Threading;
using System.Text;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontLayout;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for MasterPageBase
    /// </summary>
    public class MasterPageBase : System.Web.UI.MasterPage
    {
        #region Variable Declaration

        protected ContentPlaceHolder PageContent;
        private string SEName = string.Empty;
        protected Literal SectionTitle;
        protected Menu aspnetMenu;
        protected ScriptManager scrptMgr;
        protected PlaceHolder ctrlMinicart;

        #endregion

        #region Constructor

        public MasterPageBase() 
        {
        }

        #endregion
        
        #region Properties

        protected Customer ThisCustomer
        {
            get { return Customer.Current; }
        }

        /// <summary>
        /// Gets or sets the SectionTitle text
        /// </summary>
        public string SectionTitleText
        {
            get
            {
                if (SectionTitle != null)
                {
                    return SectionTitle.Text;
                }

                return string.Empty;
            }
            set
            {
                if (SectionTitle != null)
                {
                    SectionTitle.Text = value;
                }
            }
        }

        /// <summary>
        /// Gets the content area panel
        /// </summary>
        public Panel ContentPanel
        {
            get
            {
                Panel pnl = this.PageContent.FindControl("pnlContent") as Panel;
                return pnl;
            }
        }

        /// <summary>
        /// Gets whether to use ajax addtocart functionality for the minicart
        /// </summary>
        public bool ShowMiniCart 
        {
            get 
            {
                return AppLogic.AppConfigBool("Minicart.UseAjaxAddToCart") && ShowMiniCartOnThisPage(); 
            }
        }


        /// <summary>
        /// Determines the visibility of the minicart based on the current requested page
        /// </summary>
        /// <returns></returns>
        private bool ShowMiniCartOnThisPage()
        {
            bool allowed = true;
            string pageName = CommonLogic.GetThisPageName(false);

            if (pageName.StartsWithIgnoreCase("shoppingcart") ||
                pageName.StartsWithIgnoreCase("checkout") ||
                pageName.StartsWithIgnoreCase("cardinal") ||
                pageName.StartsWithIgnoreCase("addtocart") ||
                pageName.ContainsIgnoreCase("_process") ||
                pageName.StartsWithIgnoreCase("lat_"))
            {
                allowed = false;
            }

            return allowed;
        }

        #endregion

        #region Event Handlers
        /// <summary>
        /// Handles the PollVoteButtonClick event of the ctrlPoll control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void ctrlPoll_PollVoteButtonClick(object sender, EventArgs e)
        {
            PollControl ctrlPoll = sender as PollControl;

            if (ctrlPoll != null)
            {
                string pollAnswerID = HttpUtility.UrlEncode(ctrlPoll.SelectedValue);
                string pollID = HttpUtility.UrlEncode(ctrlPoll.PollID.ToString());
                string redirectUrl = string.Format("pollvote.aspx?PollID={0}&Poll_{0}={1}", pollID, pollAnswerID);

                Response.Redirect(redirectUrl);
            }
        }

        protected void InitializePollControl()
        {
            int categoryID = CommonLogic.QueryStringUSInt("CategoryID");
            int sectionID = CommonLogic.QueryStringUSInt("SectionID");

            // Search for the ID that you set in template.master
            PollControl ctrPoll = this.FindControl("ctrlPoll") as PollControl;

            // Check if valid first
            if (ctrPoll != null)
            {
                int customerID = ThisCustomer.CustomerID;
                string localleSetting = ThisCustomer.LocaleSetting;

                // Assign datasource,image,text,css. 
                ctrPoll.DataSource = Poll.Find(customerID, categoryID, sectionID, ThisCustomer.SkinID, 0, localleSetting, ThisCustomer.IsRegistered);
                ctrPoll.HeaderBGColor = AppLogic.AppConfig("HeaderBGColor");
                ctrPoll.PollClass = AppLogic.AppConfig("BoxFrameStyle");
                ctrPoll.HeaderImage = string.Format("~/App_Themes/Skin_{0}/images/todayspoll.gif", ThisCustomer.SkinID);
                ctrPoll.ButtonCssClass = "PollSubmit";
                ctrPoll.PollButtonText = AppLogic.GetString("polls.Vote", ThisCustomer.SkinID, localleSetting);
            }
        }
        #endregion

        #region Methods

        protected override void OnInit(EventArgs e)
        {           
            if (this.RequireScriptManager)
            {               
                // provide hookup for individual pages
                (this.Page as SkinBase).RegisterScriptAndServices(scrptMgr);                
            }
            Page.ClientScript.RegisterClientScriptInclude(Page.GetType(), "formvalidate", ResolveClientUrl("~/jscripts/formvalidate.js"));
            Page.ClientScript.RegisterClientScriptInclude(Page.GetType(), "core", ResolveClientUrl("~/jscripts/core.js"));
            
            InitializePollControl();
            LoadMiniCartIfEnabled();

			// dynamically add the preview control to the page
			var pageContent = this.FindControl("PageContent") as ContentPlaceHolder;
			var profile = HttpContext.Current.Profile;
			if(pageContent != null 
                && profile != null
                && profile.PropertyValues["PreviewSkinID"] != null
                && CommonLogic.IsInteger(profile.GetPropertyValue("PreviewSkinID").ToString()))
			{
                Literal previewStyleLit = new Literal() { Text = "<link runat=\"server\" rel=\"stylesheet\" href=\"App_Templates/Admin_Default/previewstyles.css\" type=\"text/css\">" };
                this.Page.Header.Controls.Add(previewStyleLit);

                Control previewControl = LoadControl("~/Controls/EndPreview.ascx") as Control;
				pageContent.Controls.Add(previewControl);
			}

            base.OnInit(e);
        }

        public void LoadMiniCartIfEnabled()
        {
            if (this.ShowMiniCart && 
                ctrlMinicart != null)
            {
                Control ctrl = LoadControl("~/controls/minicart.ascx");
                ctrl.AppRelativeTemplateSourceDirectory = "~/";
                ctrlMinicart.Controls.Add(ctrl);
            }            
        }

        /// <summary>
        /// Gets whether to require asp.net ajax script manager
        /// </summary>
        public virtual bool RequireScriptManager
        {
            get 
            {
                return (this.Page as SkinBase).RequireScriptManager;
            }
        }
        
        /// <summary>
        /// Overrides the OnPreRender method
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            SetupMenu();

            if (AppLogic.AppConfigBool("Layouts.Enabled"))
            {
                bool HasMappedLayout = false;

                bool IsTopic = (Page as SkinBase).IsTopicPage;
                bool IsEntity = (Page as SkinBase).IsEntityPage;
                bool IsProduct = (Page as SkinBase).IsProductPage;

                int PageID = (Page as SkinBase).PageID;

                LayoutMap lm = new LayoutMap();

                if(IsEntity)
                {
                    String EntityType = (Page as SkinBase).EntityType;

                    lm = new LayoutMap(EntityType, PageID);

                    if (lm.LayoutID > 0)
                    {
                        HasMappedLayout = true;
                        (Page as SkinBase).ThisLayout = new LayoutData(lm.LayoutID);
                    }
                }
                else if (IsTopic)
                {
                    lm = new LayoutMap("topic", PageID);

                    
                }
                else if (IsProduct)
                {
                    lm = new LayoutMap("product", PageID);
                }
                else
                {
                    String pName = CommonLogic.GetThisPageName(false);

                    lm = new LayoutMap(pName, PageID);
                }

                if (lm.LayoutID > 0)
                {
                    HasMappedLayout = true;
                    (Page as SkinBase).ThisLayout = new LayoutData(lm.LayoutID);
                }

                if(HasMappedLayout)
                {
                    LayoutData ThisLayout = (Page as SkinBase).ThisLayout;

                    if ((Page as SkinBase).ThisLayout != null)
                    {
                        bool exists = CommonLogic.FileExists(ThisLayout.LayoutFile);

                        if (!exists)
                        {
                            exists = ThisLayout.CreateLayoutControl();
                        }

                        if (exists)
                        {
                            Control ctrl = LoadControl("~/layouts/" + ThisLayout.LayoutID.ToString() + "/" + ThisLayout.Name + "_" + ThisLayout.Version.ToString() + ".ascx");

                            ContentPlaceHolder phContent = (ContentPlaceHolder)this.FindControl("PageContent");
                            phContent.Controls.Clear();

                            phContent.Controls.Add(ctrl);
                        }
                    }

                }
            }

            base.OnPreRender(e);
        }


        /// <summary>
        /// Set's up the Asp.net default Menu control for display inside our skinning engine
        /// </summary>
        private void SetupMenu()
        {
            if (aspnetMenu != null)
            {
                string randomId = Guid.NewGuid().ToString("N").Substring(0, 5);

                // make ready the scripts
                StringBuilder script = new StringBuilder();
                script.AppendFormat("<script type=\"text/javascript\" language=\"Javascript\">\n");
                script.AppendFormat("    function loadMenu_{0}() {{\n", randomId);

                string menuId = aspnetMenu.ClientID;

                SiteMapDataSource ds = new SiteMapDataSource();
                var prov = SiteMapProviderFactory.GetSiteMap(Customer.Current, aspnetMenu.MaximumDynamicDisplayLevels);
                prov.MaximumDynamicDisplayLevels = aspnetMenu.MaximumDynamicDisplayLevels;
                ds.Provider = prov;

                aspnetMenu.DataSource = ds;
                aspnetMenu.AppRelativeTemplateSourceDirectory = "~/";
                //aspnetMenu.MaximumDynamicDisplayLevels = AspDotNetStorefrontCommon.AppConfig.SiteDisplay.MaxMenuLevelSize;
                aspnetMenu.DataBind();

                // now for the custom asp.net Menu javascript
                script.AppendFormat("            if({0}_Data) {{\n", menuId);
                script.AppendFormat("                 {0}_Data.hoverClass  = '{0}_DynamicHoverStyle';\n", menuId);
                script.AppendFormat("                 {0}_Data.hoverHyperLinkClass  = '{0}_DynamicHoverStyle';\n", menuId);
                script.AppendFormat("                 {0}_Data.staticHoverClass = '{0}_StaticHoverStyle';\n", menuId);
                script.AppendFormat("                 {0}_Data.staticHoverHyperLinkClass = '{0}_StaticHoverStyle';\n", menuId);
                script.AppendFormat("            }}\n");

                // ending brace for loadMenu function
                script.AppendFormat("    }}\n");

                script.AppendFormat("    $window_addLoad(loadMenu_{0});\n", randomId);
                script.AppendFormat("</script>\n");

                Page.ClientScript.RegisterStartupScript(this.GetType(), randomId, script.ToString());

                // fix for safari browsers
                if (Request.UserAgent != null)
                {
                    if (Request.UserAgent.IndexOf("AppleWebKit") > 0)
                    {
                        Request.Browser.Adapters.Clear();
                    }
                }
            }

        }


        #endregion
    }
}


