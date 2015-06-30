// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using AspDotNetStorefrontCore;
using System.Data.SqlClient;
using Vortx.MobileFramework;

namespace AspDotNetStorefront
{

    public partial class mobiledesktopwarning : SkinBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;

            MobileHelper.RedirectPageWhenMobileIsDisabled("~/", ThisCustomer);

            Page.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Expires = 0;
            Response.Cache.SetNoStore();
            Response.AppendHeader("Pragma", "no-cache");
            Response.ExpiresAbsolute = DateTime.Now.Subtract(new TimeSpan(1, 0, 0, 0));
            Response.CacheControl = "no-cache";
            Title = "Leaving Mobile Friendly Site";

            WarningMessage.Text = string.Format(AppLogic.GetString("Mobile.mobiledesktopwarning.aspx.1", ThisCustomer.SkinID, ThisCustomer.LocaleSetting), AppLogic.AppConfig("StoreName"));
            ContinueButton.Text = AppLogic.GetString("Mobile.mobiledesktopwarning.aspx.2", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
            CancelButton.Text = AppLogic.GetString("Mobile.mobiledesktopwarning.aspx.3", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);

        }
        protected void btn_ContinueButtonClick(object sender, EventArgs e)
        {
            Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;
            MobileHelper.setForceDesktopCookie(ThisCustomer);
            string TargetPage = CommonLogic.QueryStringCanBeDangerousContent("targetpage");
            Response.Redirect(TargetPage);
        }
        protected void btn_CancelButtonClick(object sender, EventArgs e)
        {
            string CurrentPage = CommonLogic.QueryStringCanBeDangerousContent("currentpage");
            if (string.IsNullOrEmpty(CurrentPage) || CurrentPage == "js")
            {
                CurrentPage = "default.aspx";
            }
            Response.Redirect(CurrentPage);
        }
    }

}
