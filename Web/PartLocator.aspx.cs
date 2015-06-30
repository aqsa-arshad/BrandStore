using System;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI.WebControls;
using System.Web.UI;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontCommon;

namespace AspDotNetStorefront
{
    public partial class PartLocator : SkinBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.QueryString["sku"] != null)
            {
                Session["PART_SKU"] = null;
                Response.Redirect("~/partlocator.aspx");
            }

            RequireSecurePage();
            SectionTitle = AppLogic.GetString("partlocator.title", SkinID, ThisCustomer.LocaleSetting);
            string Body = AppLogic.GetString("partlocator.content", SkinID, ThisCustomer.LocaleSetting);

            TitleLiteral.Text = SectionTitle;
            BodyLiteral.Text = Body;

            pnlContent.Controls.Add(LoadControl("~/Controls/PartLocator.ascx"));
        }
    }
}