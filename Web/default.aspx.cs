// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Globalization;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontCommon;
using AspDotNetStorefrontControls;

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

            // this may be overwridden by the XmlPackage below!
            SectionTitle = String.Format(AppLogic.GetString("default.aspx.1", SkinID, ThisCustomer.LocaleSetting), AppLogic.AppConfig("StoreName"));

            Package1.PackageName = "page.default.xml.config";

            // set the Customer context, and set the SkinBase context, so meta tags to be set if they are not blank in the XmlPackage results
            Package1.SetContext = this;

            // unsupported feature:
            //System.Diagnostics.Trace.WriteLineIf(Config.TraceLevel.TraceVerbose, "Welcome to AspDotNetStorefront");

			PayPalAd homePageAd = new PayPalAd(PayPalAd.TargetPage.Home);
			if (homePageAd.Show)
				ltPayPalAd.Text = homePageAd.ImageScript;
        }

        protected override string OverrideTemplate()
        {
            String MasterHome = AppLogic.HomeTemplate();

            if (MasterHome.Trim().Length == 0)
            {
                MasterHome = "template";
            }

            if (MasterHome.EndsWith(".ascx"))
            {
                MasterHome = MasterHome.Replace(".ascx", ".master");
            }

            if(!MasterHome.EndsWith(".master", StringComparison.OrdinalIgnoreCase))
            {
                MasterHome = MasterHome + ".master";
            }

            if (!CommonLogic.FileExists(CommonLogic.SafeMapPath("~/App_Templates/Skin_" + base.SkinID.ToString() + "/" + MasterHome)))
            {
                MasterHome = "template.master";
            }

            return MasterHome;
        }
    }
}
