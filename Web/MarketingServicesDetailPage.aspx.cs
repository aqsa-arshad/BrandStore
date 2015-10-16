using System;
using AspDotNetStorefront;
using AspDotNetStorefrontCore;

public partial class MarketingServicesDetailPage : SkinBase
{
    protected void Page_Load(object sender, EventArgs e)
    {
    }

    protected override string OverrideTemplate()
    {
        var masterHome = AppLogic.HomeTemplate();
        if (masterHome.Trim().Length == 0)
        {
            masterHome = "JeldWenTemplate";
        }
        if (masterHome.EndsWith(".ascx"))
        {
            masterHome = masterHome.Replace(".ascx", ".master");
        }
        if (!masterHome.EndsWith(".master", StringComparison.OrdinalIgnoreCase))
        {
            masterHome = masterHome + ".master";
        }
        if (!CommonLogic.FileExists(CommonLogic.SafeMapPath("~/App_Templates/Skin_" + SkinID + "/" + masterHome)))
        {
            masterHome = "JeldWenTemplate";
        }
        return masterHome;
    }
}