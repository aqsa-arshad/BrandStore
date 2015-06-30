// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI.WebControls;
using System.Web.UI;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontCommon;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for signin.
    /// </summary>
    public partial class signin : SkinBase
    {
        protected void Page_Load(object sender, System.EventArgs e)
        {
            RequireSecurePage();
            SectionTitle = AppLogic.GetString("signin.aspx.1", SkinID, ThisCustomer.LocaleSetting);

            pnlContent.Controls.Add(LoadControl("~/Controls/Signin.ascx"));
        }
    }
}
