// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Data;
using System.Globalization;

using AspDotNetStorefrontCore;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Moneybookers 3-D Secure processing.
    /// </summary>
	public partial class SecureFormMoneybookers : SkinBase
    {
        private void Page_Load(object sender, System.EventArgs e)
        {
            SectionTitle = "Order - Credit Card Verification:";
            if (ShoppingCart.CartIsEmpty(ThisCustomer.CustomerID, CartTypeEnum.ShoppingCart))
                Response.Redirect("shoppingcart.aspx");
        }
    }
}
