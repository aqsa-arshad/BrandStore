using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontCore.RateServiceWebReference;

public partial class controls_MarketingServicesDetailControl : System.Web.UI.UserControl
{
    protected void Page_Load(object sender, EventArgs e)
    {
        GetCategoryDescription();
    }

    private void GetCategoryDescription()
    {
        var results = new StringBuilder("");

        using (var conn = DB.dbConn())
        {
            conn.Open();
            var categoryId = AppLogic.AppConfig("MarketingServicesCategoryID");
            using (var rs = DB.GetRS("aspdnsf_MarketingServicesDetail " + categoryId, conn))
            {
                if (rs.Read())
                {
                    var pSummary = rs["Summary"] + "";
                    results.Append(pSummary );                   
                }
            }
        }
        results.Append("<div class=\"row\">");
        results.Append("<div class=\"col-md-6\">");
        results.Append("<button id=\"btnShopMarketing\" class=\"btn btn-md btn-block btn-primary tablet-btn\" type=\"button\">Shop Marketing Services</button >");
        results.Append("</div>");
        results.Append("</div>");
        ContentBox.InnerHtml = results.ToString();
    }
}