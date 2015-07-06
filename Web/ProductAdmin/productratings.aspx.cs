// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Globalization;
using System.Text;
using System.Web;
using System.IO;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
    /// <summary>
    /// Summary description for productratings
    /// </summary>
    public partial class productratings : AdminPageBase
    {
        private int ProductID;
        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");
            ProductID = CommonLogic.QueryStringUSInt("ProductID");
            if (ProductID == 0)
            {
                Response.Redirect(AppLogic.AdminLinkUrl("products.aspx"));
            }
            if (CommonLogic.QueryStringCanBeDangerousContent("DeleteID").Length != 0)
            {
                // delete the rating:
                Ratings.DeleteRating(CommonLogic.QueryStringUSInt("DeleteID"));
            }
            SectionTitle = String.Format(AppLogic.GetString("admin.sectiontitle.productratings", SkinID, LocaleSetting),"<a href=\"" + AppLogic.AdminLinkUrl("products.aspx") + "\">","</a>",AppLogic.GetProductName(ProductID, LocaleSetting));
            RenderHtml();
        }

        private void RenderHtml()
        {
            StringBuilder writer = new StringBuilder();
            writer.Append(Ratings.Display(ThisCustomer, ProductID, 0, 0, 0, SkinID));
            ltContent.Text = writer.ToString();
        }

    }
}
