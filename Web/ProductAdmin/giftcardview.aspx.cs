// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
    public partial class giftcardview : AdminPageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            Customer cust = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;
            string temp = AppLogic.RunXmlPackage(AppLogic.AppConfig("XmlPackage.EmailGiftCardNotification"), new Parser(1, cust), cust, 1, String.Empty, "GiftCardID=" + CommonLogic.QueryStringNativeInt("iden"), true, true);
            ltView.Text = temp;
        }
    }
}
