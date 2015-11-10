using System.Activities.Expressions;
using AspDotNetStorefrontCore;
using System;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Class JeldWenMasterPage.
    /// </summary>
    public partial class JeldWenMasterPage : MasterPageBase
    {
        /// <summary>
        /// The m_ this customer
        /// </summary>
        private Customer m_ThisCustomer;
        /// <summary>
        /// Gets or sets the this customer.
        /// </summary>
        /// <value>The this customer.</value>
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

        protected override void OnInit(EventArgs e)
        {
            if (this.RequireScriptManager)
            {
                // provide hookup for individual pages
                (this.Page as SkinBase).RegisterScriptAndServices(scrptMgr);
            }
            base.OnInit(e);
        }

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                SetPageHeading();
                if (ThisCustomer.IsRegistered)
                {
                    ShowPostLoginControls();
                    this.hdnCustomerLevel.Text = ThisCustomer.CustomerLevelID.ToString();
                }
                else
                {
                    ShowPreLoginControls();
                    hdnCustomerLevel.Text = "-1";
                }
            }
        }

        /// <summary>
        /// Shows the post login controls.
        /// </summary>
        private void ShowPostLoginControls()
        {
            divbeforelogin.Visible = false;
            divafterlogin.Visible = true;
        }

        /// <summary>
        /// Shows the pre login controls.
        /// </summary>
        private void ShowPreLoginControls()
        {
            divbeforelogin.Visible = true;
            divafterlogin.Visible = false;
        }

        /// <summary>
        /// Sets the page heading.
        /// </summary>
        private void SetPageHeading()
        {
            var currentURL = Request.Url.AbsolutePath;
            if (currentURL.ToUpper().Contains("HOME"))
            {
                lblPageHeading.Text = string.Empty;
                pnlPageHeading.Visible = false;
            }
            else if (currentURL.ToUpper().Contains("SEARCH"))
            {
                lblPageHeading.Text = "SEARCH RESULTS";
                pnlPageHeading.Visible = true;
            }
            else if (currentURL.ToUpper().Contains("JWMYACCOUNT"))
            {
                lblPageHeading.Text = "MY ACCOUNT: " + ThisCustomer.FullName();
                pnlPageHeading.Visible = true;
                liMyAccount.Attributes.Add("class", "active");
            }
            else if (currentURL.ToUpper().Contains("JWMYADDRESSES"))
            {
                // Label will be loaded from Content Page w.r.t AddressType in QueryString
                pnlPageHeading.Visible = true;
            }
            else if (currentURL.ToUpper().Contains("JWADDADDRESSES"))
            {
                lblPageHeading.Text = "ADD/EDIT ADDRESS";
                pnlPageHeading.Visible = true;
            }

            else if (currentURL.ToUpper().Contains("SIGNIN"))
            {
                divlogin.Visible = false;
                separatorafterlogin.Visible = false;
            }

            else if (currentURL.ToUpper().Contains("CREATEACCOUNT"))
            {
                lblPageHeading.Text = "CREATE MY ACCOUNT";
                pnlPageHeading.Visible = true;
            }
            else if (currentURL.ToUpper().Contains("MARKETINGSERVICESDETAIL"))
            {
                lblPageHeading.Text = "About Marketing Services";
                pnlPageHeading.Visible = true;
            }
            else if (currentURL.ToUpper().Contains("C-"))
            {
                // Label & Back Link will be loaded from Content Page w.r.t Category in QueryString
                pnlPageHeading.Visible = true;
            }
            else if (currentURL.ToUpper().Contains("P-"))
            {
                // Label & Back Link will be loaded from Content Page w.r.t Category &  Sub-Category in QueryString
                pnlPageHeading.Visible = true;
            }
            else if (currentURL.ToUpper().Contains("JWSUPPORT"))
            {
                lblPageHeading.Text = "Support";
                pnlPageHeading.Visible = true;
                divbeforelogin.Visible = false;
                divafterlogin.Visible = false;
                divSideBarBeforeLogin.Visible = false;
                divSideBarAfterLogin.Visible = false;
                divJWsupport.Visible = true;
            }
            else if (currentURL.ToUpper().Contains("JWABOUTTRUEBLU"))
            {
                lblPageHeading.Text = "ABOUT True BLU™";
                pnlPageHeading.Visible = true;

            }
            else if (currentURL.ToUpper().Contains("JWTERMSANDCONDITIONS"))
            {
                lblPageHeading.Text = "Terms and Privacy Policy";
                pnlPageHeading.Visible = true;
            }
            else if (currentURL.ToUpper().Contains("ORDERHISTORY"))
            {
                if (ThisCustomer.CustomerLevelID == 4 || ThisCustomer.CustomerLevelID == 5 ||
                    ThisCustomer.CustomerLevelID == 6)
                {
                    //For Dealer lblPageHeading will be set in OrderHistory Page
                }
                else
                {
                    lblPageHeading.Text = "ORDER HISTORY";
                }
                pnlPageHeading.Visible = true;
            }
            else
            {
                lblPageHeading.Text = string.Empty;
                pnlPageHeading.Visible = false;
            }
        }
    }
}