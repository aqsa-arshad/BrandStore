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
using AspDotNetStorefrontControls;
using AjaxControlToolkit;
using Telerik.Web.UI;

namespace AspDotNetStorefrontAdmin
{
    public partial class MappedObjectPage : Page
    {
        protected override void OnInit(EventArgs e)
        {
            ctrlMap.ThisCustomer = Customer.Current;
            ctrlMap.EntityType = Request.QueryStringCanBeDangerousContent("EntityType");

            Title = "Store Mapping - " + Request.QueryStringCanBeDangerousContent("EntityType");

            base.OnInit(e);
        }
    }
}

