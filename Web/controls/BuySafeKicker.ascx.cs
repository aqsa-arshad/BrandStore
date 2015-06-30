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
using AspDotNetStorefrontCore;
using System.Text.RegularExpressions;
using AspDotNetStorefront;

namespace AspDotNetStorefront
{
    public partial class BuySafeKicker : System.Web.UI.UserControl
    {
        public string KickerType { set; get; }
        public string WrapperClass { set; get; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(KickerType))
                KickerType = AppLogic.AppConfig("BuySafe.KickerType");

            if (AppLogic.GlobalConfigBool("BuySafe.Enabled") && AppLogic.GlobalConfig("BuySafe.Hash").Length != 0)
                litBuySafeKicker.Text = "<span class=\"buySAFE_Kicker_Wrapper"+CommonLogic.IIF(string.IsNullOrEmpty(WrapperClass), "", " " + WrapperClass)+"\"><span id=\"buySAFE_Kicker\" name=\"buySAFE_Kicker\" type=\"" + KickerType + "\"></span></span>";
            else
                litBuySafeKicker.Text = "";
        }

    }
}
