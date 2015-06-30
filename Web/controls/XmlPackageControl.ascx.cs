// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront
{

    public partial class XmlPackageControl : System.Web.UI.UserControl
    {
        private String m_PackageName = String.Empty;
        private String m_RuntimeParams = String.Empty;
        private Customer m_ThisCustomer = null;
        private bool m_DesignMode = false;
        private String m_SectionTitle = String.Empty;
        private String m_SETitle = String.Empty;
        private String m_SEKeywords = String.Empty;
        private String m_SEDescription = String.Empty;
        private String m_SENoScript = String.Empty;
        private String m_ContentType = String.Empty;

        // these are set to false, to make "most" page control invocations shorter to create (as "most" instances don't care about these!)
        private bool m_EnforcePassword = false;
        private bool m_EnforceSubscription = false;
        private bool m_EnforceDisclaimer = false;
        private bool m_AllowSEPropogation = false;
        private bool m_ReplaceTokens = false;

        private SkinBase m_SkinBase = null; // if not null, this control will set the page metatags to the results from the XmlPackage, IF those xmlpackage results are not "empty strings"

        protected void Page_Load(object sender, EventArgs e)
        {
            m_ThisCustomer = Customer.Current;

            // Load Child Controls
            Literal Contents = new Literal();
            Contents.ID = "Contents";
            Contents.EnableViewState = false;
            Contents.Mode = LiteralMode.PassThrough;
            Contents.Text = "<b>XmlPackage</b>";

            this.Controls.Add(Contents);

            Panel Panel1 = new Panel();
            Panel1.Style.Add(HtmlTextWriterStyle.Width, "100%");

            Literal Debug1 = new Literal();
            Debug1.EnableViewState = false;
            Debug1.Mode = LiteralMode.PassThrough;

            Literal Debug2 = new Literal();
            Debug2.EnableViewState = false;
            Debug2.Mode = LiteralMode.PassThrough;

            Literal Debug3 = new Literal();
            Debug3.EnableViewState = false;
            Debug3.Mode = LiteralMode.PassThrough;

            Panel1.Controls.Add(Debug1);
            Panel1.Controls.Add(Debug2);
            Panel1.Controls.Add(Debug3);

            this.Controls.Add(Panel1);

            m_DesignMode = (HttpContext.Current == null);
            if (m_DesignMode)
            {
                if (PackageName.Length != 0)
                {
                    Contents.Text = "XmlPackage: " + PackageName;
                }
                else
                {
                    Contents.Text = "XmlPackage";
                }
            }
            else
            {
                using (XmlPackage2 m_P = new XmlPackage2(PackageName, ThisCustomer, ThisCustomer.SkinID, String.Empty, RuntimeParams, String.Empty, true))
                {
                    try
                    {
                        if (Page == null)
                        {
                            Page = HttpContext.Current.CurrentHandler as SkinBase;
                        }

                        String s = AppLogic.RunXmlPackage(m_P, Page.GetParser, Page.ThisCustomer, Page.ThisCustomer.SkinID, m_ReplaceTokens, true);
                        Contents.Text = s;

                        m_SectionTitle = m_P.SectionTitle;
                        m_SETitle = m_P.SETitle;
                        m_SEKeywords = m_P.SEKeywords;
                        m_SEDescription = m_P.SEDescription;
                        m_SENoScript = m_P.SENoScript;
                    }
                    catch (Exception ex)
                    {
                        Contents.Text = CommonLogic.GetExceptionDetail(ex, "");
                    }
                    if (Page != null && m_AllowSEPropogation)
                    {
                        if (m_SectionTitle.Length != 0)
                        {
                            Page.SectionTitle = m_SectionTitle;
                        }
                        if (m_SETitle.Length != 0)
                        {
                            Page.SETitle = m_SETitle;
                        }
                        if (m_SEKeywords.Length != 0)
                        {
                            Page.SEKeywords = m_SEKeywords;
                        }
                        if (m_SEDescription.Length != 0)
                        {
                            Page.SEDescription = m_SEDescription;
                        }
                        if (m_SENoScript.Length != 0)
                        {
                            Page.SENoScript = m_SENoScript;
                        }
                    }
                    if (m_P != null && (AppLogic.AppConfigBool("XmlPackage.DumpTransform") || m_P.IsDebug))
                    {
                        Panel1.Visible = true;
                        Debug1.Text = "<div><b>" + m_P.URL + "</b><textarea READONLY style=\"width: 100%\" rows=\"50\">" + XmlCommon.PrettyPrintXml(m_P.PackageDocument.InnerXml) + "</textarea></div>";
                        Debug2.Text = "<div><b>" + PackageName + "_store.runtime.xml</b><textarea READONLY cols=\"80\" rows=\"50\">" + CommonLogic.ReadFile(CommonLogic.IIF(AppLogic.IsAdminSite, "../", "") + "images/" + PackageName + "_" + CommonLogic.IIF(AppLogic.IsAdminSite, "admin", "store") + ".runtime.xml", true) + "</textarea></div>";
                        Debug3.Text = "<div><b>" + PackageName + "_store.xfrm.xml</b><textarea READONLY cols=\"80\" rows=\"50\">" + CommonLogic.ReadFile(CommonLogic.IIF(AppLogic.IsAdminSite, "../", "") + "images/" + PackageName + "_" + CommonLogic.IIF(AppLogic.IsAdminSite, "admin", "store") + ".xfrm.xml", true) + "</textarea></div>";
                    }
                    else
                    {
                        Panel1.Visible = false;
                    }
                }
            }
        }

        public AspDotNetStorefront.SkinBase SetContext
        {
            set
            {
                m_SkinBase = value;
                m_ThisCustomer = m_SkinBase.ThisCustomer;
            }
        }

        public new AspDotNetStorefront.SkinBase Page
        {
            get
            {
                return m_SkinBase;
            }
            set
            {
                m_SkinBase = value;
            }
        }

        public String PackageName
        {
            get
            {
                return m_PackageName;
            }
            set
            {
                m_PackageName = value;
            }
        }

        public String RuntimeParams
        {
            get
            {
                return m_RuntimeParams;
            }
            set
            {
                m_RuntimeParams = value;
            }
        }

        public string ContentType
        {
            get
            {
                return m_ContentType;
            }
            set
            {
                m_ContentType = value;
            }
        }

        public Customer ThisCustomer
        {
            get
            {
                return m_ThisCustomer;
            }
            set
            {
                m_ThisCustomer = value;
            }
        }

        public String SectionTitle
        {
            get
            {
                return m_SectionTitle;
            }
        }

        public String SETitle
        {
            get
            {
                return m_SETitle;
            }
        }

        public String SEKeywords
        {
            get
            {
                return m_SEKeywords;
            }
        }

        public String SEDescription
        {
            get
            {
                return m_SEDescription;
            }
        }

        public String SENoScript
        {
            get
            {
                return m_SENoScript;
            }
        }

        public bool EnforcePassword
        {
            get
            {
                return m_EnforcePassword;
            }
            set
            {
                m_EnforcePassword = value;
            }
        }

        public bool EnforceSubscription
        {
            get
            {
                return m_EnforceSubscription;
            }
            set
            {
                m_EnforceSubscription = value;
            }
        }


        public bool ReplaceTokens
        {
            get
            {
                return m_ReplaceTokens;
            }
            set
            {
                m_ReplaceTokens = value;
            }
        }
        
        public bool EnforceDisclaimer
        {
            get
            {
                return m_EnforceDisclaimer;
            }
            set
            {
                m_EnforceDisclaimer = value;
            }
        }

        public bool AllowSEPropogation
        {
            get
            {
                return m_AllowSEPropogation;
            }
            set
            {
                m_AllowSEPropogation = value;
            }
        }

    }

}
