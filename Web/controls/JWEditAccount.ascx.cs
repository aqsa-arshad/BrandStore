using AspDotNetStorefrontCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AspDotNetStorefront
{
    public partial class JWEditAccount : System.Web.UI.UserControl
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
        /// Page Load Event
        /// </summary>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                LoadFormData();
            }
        }

        private void LoadFormData()
        {
 
        }
    }
}