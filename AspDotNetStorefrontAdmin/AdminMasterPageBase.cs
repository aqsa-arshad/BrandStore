// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AspDotNetStorefrontCore;
using System.Web;
using System.Web.UI.WebControls;

namespace AspDotNetStorefrontAdmin
{
    /// <summary>
    /// Base class for Master Page Templates using in the admin site
    /// This file contains logic for performing common routines such
    /// as generating menu item
    /// </summary>
    public class AdminMasterPageBase : System.Web.UI.MasterPage
    {
        public Customer ThisCustomer
        {
            get { return ((AspDotNetStorefrontPrincipal)HttpContext.Current.User).ThisCustomer; }
        }
    }
}
