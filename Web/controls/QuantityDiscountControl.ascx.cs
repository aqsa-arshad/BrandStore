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

public partial class controls_QuantityDiscountControl : System.Web.UI.UserControl
{
    protected void Page_Load(object sender, EventArgs e)
    {
    
    }

    // To display the discounted quantity.
    protected string ShowDiscountQuantity(object LowQuantity, object HighQuantity)
    {
        return LowQuantity.ToString() + CommonLogic.IIF(Convert.ToBoolean(Convert.ToInt32(HighQuantity) > 9999), "+", "-" + HighQuantity.ToString());
    }

    // To display the discounted amount.
    protected string ShowDiscountAmount(object QuantityDiscountID, object DiscountPercent)
    {
         Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)HttpContext.Current.User).ThisCustomer;
         if (QuantityDiscount.isFixedQuantityDiscount(Convert.ToInt32(QuantityDiscountID)))
         {
             return Localization.CurrencyStringForDisplayWithExchangeRate((Decimal)DiscountPercent, ThisCustomer.CurrencySetting);
         }
         else
         {
             return Convert.ToDecimal(DiscountPercent).ToString("N" + AppLogic.AppConfigNativeInt("QuantityDiscount.PercentDecimalPlaces")) + "%";
         }
    }
}
