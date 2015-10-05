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
using System.Data.SqlClient;
using System.Configuration;

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
           
            if (ThisCustomer.IsAuthenticated)
            {
                Response.Redirect("home.aspx");
            }            
            SectionTitle = String.Format(AppLogic.GetString("default.aspx.1", SkinID, ThisCustomer.LocaleSetting), AppLogic.AppConfig("StoreName"));
              


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
}
