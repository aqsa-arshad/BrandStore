using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

public partial class controls_MarketingServices : System.Web.UI.UserControl
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
                    var pDescription = rs["Description"] + "";
                    results.Append(pDescription);
                }
            }
        }
        results.Append("<button id=\"btnShowMarketingServicesDetail\" class=\"btn btn-md btn-primary btn-block\" type=\"button\">More</button>");
        MarketingServiceSection.InnerHtml = results.ToString();
    }
}