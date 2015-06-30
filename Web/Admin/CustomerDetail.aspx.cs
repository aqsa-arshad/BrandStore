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
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
    public partial class Admin_CustomerDetail : AdminPageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                int customerid = CommonLogic.QueryStringNativeInt("customerid");
                bool editing = customerid > 0;
                if (editing)
                {
                    editcustomer1.TargetCustomerID = customerid;
                    editcustomer1.AddMode = false;
                    editcustomer1.InitializeControl();
                }
                else
                {
                    editcustomer1.TargetCustomerID = 0;
                    editcustomer1.AddMode = true;
                    editcustomer1.InitializeControl();
                }
            }
            
        }
    }
}
