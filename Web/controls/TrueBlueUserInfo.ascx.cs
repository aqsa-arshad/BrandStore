using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

public partial class controls_TrueBlueUserInfo : System.Web.UI.UserControl
{
    Customer ThisCustomer;
    
    protected void Page_Load(object sender, EventArgs e)
    {
        String WelcomeHeading = String.Empty;
        if (ThisCustomer == null)
        {
            ThisCustomer = (Page as AspDotNetStorefront.SkinBase).ThisCustomer;
        }
        WelcomeHeading = " Hi," + " " + ThisCustomer.FirstName.Trim() + " " + ThisCustomer.LastName.Trim();
        WelcomeHeadingAfterUserLogin.InnerText = WelcomeHeading;
    }
}