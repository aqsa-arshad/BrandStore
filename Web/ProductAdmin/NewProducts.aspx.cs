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
using Telerik.Web.UI;

namespace AspDotNetStorefrontAdmin
{
    public partial class NewProducts : RadAjaxPage
    {
        public List<GridProduct> grdProducts;

        protected void Page_Load(object sender, EventArgs e)
        {
            int ProductID = CommonLogic.QueryStringUSInt("productid");

            if (ProductID > 0 && !IsPostBack)
            {
                grdProducts = new List<GridProduct>();

                GridProduct gp = new GridProduct(ProductID);
                grdProducts.Add(gp);

                grd.Datasource = grdProducts;
            }
            else
            {
                grdProducts = GridProduct.GetProducts(true);
                grd.Datasource = grdProducts;
            }
        }
    }
}
