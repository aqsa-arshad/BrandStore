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
            }
            else if (currentURL.ToUpper().Contains("JWMYADDRESSES"))
            {
                lblPageHeading.Text = "MY ADDRESSES";
                pnlPageHeading.Visible = true;
            }
            else if (currentURL.ToUpper().Contains("JWADDADDRESSES"))
            {
                lblPageHeading.Text = "ADD/EDIT ADDRESS";
                pnlPageHeading.Visible = true;
            }

            else if (currentURL.ToUpper().Contains("SIGNIN"))
            {
                lblPageHeading.Text = "CHECK OUT";
                pnlPageHeading.Visible = true;
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
            else
            {
                lblPageHeading.Text = string.Empty;
                pnlPageHeading.Visible = false;
            }
        }
    }
}