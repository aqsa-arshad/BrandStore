using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront
{
    public partial class SalesForcePopUpDesign : SkinBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {

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
    }
}