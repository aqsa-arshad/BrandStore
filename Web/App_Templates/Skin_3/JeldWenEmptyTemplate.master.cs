using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront
{
    public partial class JeldWenEmptyTemplate : MasterPageBase
    {
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
        protected void Page_Load(object sender, EventArgs e)
        {

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
    }
}