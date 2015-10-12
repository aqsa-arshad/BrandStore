using System;
using AspDotNetStorefront;
using AspDotNetStorefrontCore;

public partial class MarketingServicesDetailPage : SkinBase
{
    protected void Page_Load(object sender, EventArgs e)
    {
        pnlContent.Controls.Add(LoadControl("~/Controls/MarketingServicesDetailControl.ascx"));
    }

    protected override string OverrideTemplate()
    {
        String MasterHome = AppLogic.HomeTemplate();

        if (MasterHome.Trim().Length == 0)
        {

            MasterHome = "JeldWenTemplate";// "template";
        }

        if (MasterHome.EndsWith(".ascx"))
        {
            MasterHome = MasterHome.Replace(".ascx", ".master");
        }

        if (!MasterHome.EndsWith(".master", StringComparison.OrdinalIgnoreCase))
        {
            MasterHome = MasterHome + ".master";
        }

        if (!CommonLogic.FileExists(CommonLogic.SafeMapPath("~/App_Templates/Skin_" + base.SkinID.ToString() + "/" + MasterHome)))
        {
            //Change template name to JELD-WEN template by Tayyab on 07-09-2015
            MasterHome = "JeldWenTemplate";// "template.master";
        }

        return MasterHome;
    }
}