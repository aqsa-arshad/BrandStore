// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using AspDotNetStorefrontCommon;
using AspDotNetStorefrontCore;
using Vortx.MobileFramework;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Codebehind Page for InvalidRequest.Aspx, which is responsible for handling unhandled exceptions that occur on the site
    /// </summary>
    public partial class _InvalidRequest : SkinBase
    {
        protected void Page_Load(object sender, System.EventArgs e)
        {
            MobileHelper.RedirectPageWhenMobileIsDisabled("~/googletopics.aspx", ThisCustomer);

            string errorCode = Security.HtmlEncode(CommonLogic.QueryStringCanBeDangerousContent("errorCode"));

            if (errorCode.Length == 0)
            {
                divErrorCode.Visible = false;
            }
            else
            {
                divErrorCode.Visible = true;
                lblErrorCode.Text = errorCode;
            }

            if (ThisCustomer.IsAdminUser)
            {
                divAdminMessage.Visible = true;
            }
            else
            {
                divAdminMessage.Visible = false;
            }

            ltErrorTopic.Text = new Topic("InvalidRequest", ThisCustomer.LocaleSetting, 1).Contents;

        }
    }
}
