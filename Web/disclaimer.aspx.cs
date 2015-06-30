// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Data;
using System.Globalization;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for disclaimer.
    /// </summary>
    public partial class disclaimer : System.Web.UI.Page
    {
        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            if (IsPostBack)
            {
                Profile.SiteDisclaimerAccepted = CommonLogic.GetNewGUID();
                Panel1.Visible = false;
                Response.AddHeader("REFRESH", "1; URL=" + ReturnURL.Text);
                AppLogic.SetCookie("SiteDisclaimerAccepted", Profile.SiteDisclaimerAccepted, new TimeSpan(1,0,0,0));
            }
            else
            {
                DisclaimerContents.Text = new Topic("SiteDisclaimer", 1).Contents;

                Profile.SiteDisclaimerAccepted = string.Empty;

                ReturnURL.Text = CommonLogic.QueryStringCanBeDangerousContent("ReturnURL");
                AppLogic.CheckForScriptTag(ReturnURL.Text);
                if (ReturnURL.Text.Length == 0)
                {
                    ReturnURL.Text = AppLogic.AppConfig("SiteDisclaimerAgreedPage");
                    if (ReturnURL.Text.Length == 0)
                    {
                        if (CommonLogic.QueryStringBool("checkout"))
                        {
                            ReturnURL.Text = "shoppingcart.aspx?checkout=true";
                        }
                        else
                        {
                            ReturnURL.Text = "default.aspx";
                        }
                    }
                }
                // if disclaimer was already accepted, just send them on their way:
                if (Profile.SiteDisclaimerAccepted.Length != 0 || AppLogic.IsAdminSite)
                {
                    Response.Redirect(ReturnURL.Text);
                }
            }
        }

        protected void DoNotAgreeButton_Click(object sender, EventArgs e)
        {
            Profile.SiteDisclaimerAccepted = string.Empty;
            Response.Redirect(AppLogic.AppConfig("SiteDisclaimerNotAgreedURL"));
        }
    }
}
