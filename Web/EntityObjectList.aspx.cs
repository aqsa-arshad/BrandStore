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

namespace AspDotNetStorefront
{
    public partial class EntityObjectListPage : System.Web.UI.Page
    {
        protected override void OnInit(EventArgs e)
        {
            // TBD: 
            // Deny non-Admin users

            ctrlObjectList.ThisCustomer = Customer.Current;
            string entityType = Request.QueryStringCanBeDangerousContent("entityType");

            ctrlObjectList.EntityType = entityType;
            ctrlObjectList.TextBoxClientID = Request.QueryStringCanBeDangerousContent("TextBoxClientID");
            this.Title = "{0} Search".FormatWith(entityType);

            base.OnInit(e);
        }
    }
}


