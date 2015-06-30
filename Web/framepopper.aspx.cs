// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for rssfeed.
    /// </summary>
    public partial class framepopper : System.Web.UI.Page
    {
        private void Page_Load(object sender, System.EventArgs e)
        {
            string target = CommonLogic.QueryStringCanBeDangerousContent("Target");
            if (string.IsNullOrEmpty(target))
                Response.Redirect("default.aspx", true);

            string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            String redirect = Request.Url.ToString().ToLower().Replace("/fp-" + target, "/" + target);
            AppLogic.CheckForScriptTag(redirect);


            litJScript.Text = "<script type=\"text/javascript\"> \n"
                                + "parent.location='" + redirect + "'; \n"
                                + "</script>";
            litLink.Text = "<a href=\"" + redirect + "\" >click here to continue</a>";
        }
    }
}
