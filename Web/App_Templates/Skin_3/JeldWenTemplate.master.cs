using AspDotNetStorefrontCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AspDotNetStorefront
{
    public partial class MasterPageBase : MasterPage
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
    }
}