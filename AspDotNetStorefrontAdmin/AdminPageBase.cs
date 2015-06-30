// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
    /// <summary>
    /// Base class for all admin site pages
    /// </summary>
    public class AdminPageBase : System.Web.UI.Page
    {

        #region Private Variables
        private String m_ErrorMsg = String.Empty;
        private int m_skinID = 1;
        private String m_SectionTitle = String.Empty;
        private String m_LocaleSetting = Localization.GetDefaultLocale();
        private Customer m_ThisCustomer = ((AspDotNetStorefrontPrincipal)HttpContext.Current.User).ThisCustomer;
        private Dictionary<string, EntityHelper> m_EntityHelpers = new Dictionary<string, EntityHelper>();
        private Parser m_Parser;
        private bool m_Editing = false;
        private int m_EditingID = 0;
        private String m_EditingType = String.Empty;
        private bool m_DataUpdated = false;

        private static string m_EditingLocale;


        #endregion


        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public AdminPageBase()
        { }

        #endregion


        #region Methods

        /// <summary>
        /// Overrides the System.Web.UI.Page OnInit event handler
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            //Set the page title
            String ptitle = CommonLogic.GetThisPageName(false).Replace(".aspx", "");
            Page.Title = AppLogic.GetString("admin.title." + ptitle, this.SkinID, this.LocaleSetting);

            base.OnInit(e);
        }

        /// <summary>
        /// Overrides the System.Web.UI.Page OnPreInit event handler
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreInit(EventArgs e)
        {
            //Set the theme (currently only admin_default theme supported)
            //Themed pages require a head runat=server control
            Page.Theme = "Admin_Default";
            m_Parser = new Parser(m_EntityHelpers, m_skinID, m_ThisCustomer);
            base.OnPreInit(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            base.OnLoad(e);
        }


        #endregion


        #region Properties

        /// <summary>
        /// Gets or sets the section title used for breadcrumbs
        /// </summary>
        public String SectionTitle
        {
            get { return m_SectionTitle; }
            set { m_SectionTitle = value; }
        }

        /// <summary>
        /// Returns the customer object for the currently logged in admin user
        /// </summary>
        public Customer ThisCustomer
        {
            get { return m_ThisCustomer; }
        }

        /// <summary>
        /// Returns the Admin Site SkinID
        /// </summary>
        public new int SkinID
        {
            get { return m_skinID; }
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

        public int EditingID
        {
            get { return m_EditingID; }
            set { m_EditingID = value; }
        }

        public String EditingType
        {
            get { return m_EditingType; }
            set { m_EditingType = value; }
        }


        public Parser GetParser
        {
            get
            {
                return m_Parser;
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

        /// <summary>
        /// Returns the admin site locale setting
        /// </summary>
        public String LocaleSetting
        {
            get { return m_LocaleSetting; }
        }

        /// <summary>
        /// Determines the locale of items currently being edited
        /// </summary>
        public String EditingLocale
        {
            get { return m_EditingLocale; }
            set { m_EditingLocale = value; }
        }

        public Dictionary<string, EntityHelper> EntityHelpers
        {
            get { return m_EntityHelpers; }
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

        #endregion
    }
}
