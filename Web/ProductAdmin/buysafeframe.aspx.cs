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
using AspDotNetStorefrontBuySafe;

public partial class Admin_buysafeframe : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Boolean isCallback = CommonLogic.QueryStringBool("callback");

        pnlActivationForm.Visible = !isCallback;
        pnlActivationSuccess.Visible = isCallback;

        if (isCallback)
        {
            GlobalConfig ActivationSubmitted = GlobalConfig.getGlobalConfig("BuySafe.ActivationSubmitted");
            if (ActivationSubmitted == null)
            {
                GlobalConfig.CreateGlobalConfig("BuySafe.ActivationSubmitted", "BUYSAFE", "If true the lead activation form will not be displayed.", "true", "boolean");
            }
            else
            {
                ActivationSubmitted.ConfigValue = "true";
                ActivationSubmitted.Save();
            }
            GlobalConfig.LoadGlobalConfigs();
        }
        else if (!AppLogic.GlobalConfigBool("BuySafe.ActivationSubmitted"))
        {
            List<Store> stores = Store.GetStoreList(true);
            Store s = BuySafeController.GetDefaultStore(stores);
            litAccountHash.Text = AppLogic.GlobalConfig("BuySafe.Hash");
            litStoreUrl.Text = s.ProductionURI;
            litCompanyName.Text = s.Name;
            litReturnURL.Text = Request.Url.AbsoluteUri + "?callback=true";
        }
        else
        {
            pnlActivationSuccess.Visible = pnlActivationForm.Visible = false;
        }
    }
}
