// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for wishlist.
    /// </summary>
    public partial class wishlist : SkinBase
    {
        protected void Page_Load(object sender, System.EventArgs e)
        {
            this.RequireCustomerRecord();
            SectionTitle = AppLogic.GetString("wishlist.aspx.1", SkinID, ThisCustomer.LocaleSetting);

            // make sure we don't have "JUST" deleted or unpublished items hanging around
            ShoppingCart.ClearDeletedAndUnPublishedProducts(ThisCustomer.CustomerID);

            if (!IsPostBack)
            {
                string ReturnURL = CommonLogic.QueryStringCanBeDangerousContent("ReturnUrl");
                AppLogic.CheckForScriptTag(ReturnURL);
                ViewState["ReturnURL"] = ReturnURL;
            }

            WishlistControl wl = (WishlistControl)LoadControl("~/Controls/Wishlist.ascx");
            pnlContent.Controls.Add(wl);
        }
    }
}
