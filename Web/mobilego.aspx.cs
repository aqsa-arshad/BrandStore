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

    public partial class mobilego : SkinBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            MobileHelper.setForceMobileCookie(ThisCustomer);
            string ReturnURL = CommonLogic.QueryStringCanBeDangerousContent("returnurl");
            AppLogic.CheckForScriptTag(ReturnURL);

            if (ReturnURL.Length > 3)
            {
                Response.Redirect(ReturnURL);
            }
            else
            {
                Response.Redirect("default.aspx");
            }
        }
    }

}
