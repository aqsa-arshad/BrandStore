// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for _default.
    /// </summary>
    [PageType("home")]
    public partial class _default : SkinBase
    {
        protected void Page_Load(object sender, System.EventArgs e)
        {
            if (CommonLogic.ServerVariables("HTTP_HOST").IndexOf(AppLogic.LiveServer(), StringComparison.InvariantCultureIgnoreCase) != -1 &&
                CommonLogic.ServerVariables("HTTP_HOST").IndexOf("WWW", StringComparison.InvariantCultureIgnoreCase) == -1)
            {
                if (AppLogic.RedirectLiveToWWW())
                {
                    Response.Redirect("http://www." + AppLogic.LiveServer().ToLowerInvariant());
                }
            }
            if (AppLogic.AppConfigBool("GoNonSecureAgain"))
            {
                GoNonSecureAgain();
            }
            if (ThisCustomer.IsRegistered)
            {
                Response.Redirect("home.aspx");
            }
            // this may be overwridden by the XmlPackage below!
            SectionTitle = String.Format(AppLogic.GetString("default.aspx.1", SkinID, ThisCustomer.LocaleSetting), AppLogic.AppConfig("StoreName"));
        }

        protected override string OverrideTemplate()
        {
            var masterHome = AppLogic.HomeTemplate();
            if (masterHome.Trim().Length == 0)
            {
                masterHome = "JeldWenTemplate";// "template";
            }
            if (masterHome.EndsWith(".ascx"))
            {
                masterHome = masterHome.Replace(".ascx", ".master");
            }
            if (!masterHome.EndsWith(".master", StringComparison.OrdinalIgnoreCase))
            {
                masterHome = masterHome + ".master";
            }
            if (!CommonLogic.FileExists(CommonLogic.SafeMapPath("~/App_Templates/Skin_" + base.SkinID.ToString() + "/" + masterHome)))
            {                
                masterHome = "JeldWenTemplate";
            }
            return masterHome;
        }

        protected void FeaturePrdSeeAll_Click(object sender, EventArgs e)
        {
            Response.Redirect("c-1-promotional-items.aspx");
        }
    }
}
