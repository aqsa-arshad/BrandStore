using AspDotNetStorefrontCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AspDotNetStorefront
{

    
    public partial class JeldWenMasterPage : MasterPageBase
    {

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

        /// <summary>
        /// 
        /// </summary>
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
        /// 
        /// </summary>
        private void ShowPostLoginControls()
        {
            divbeforelogin.Visible = false;
            divafterlogin.Visible = true;
        }
        /// <summary>
        /// 
        /// </summary>
        private void ShowPreLoginControls()
        {
            divbeforelogin.Visible = true;
            divafterlogin.Visible = false;
        }

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
                lblPageHeading.Text = string.Empty;
                pnlPageHeading.Visible = false;
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
            else
            {
                lblPageHeading.Text = string.Empty;
                pnlPageHeading.Visible = false;
            }
        }
    }
}