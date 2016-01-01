using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;
using AspDotNetStorefront;
using System.Collections.Generic;

public partial class controls_TrueBlueUserInfo : System.Web.UI.UserControl
{
    Customer ThisCustomer;
    List<CustomerFund> lstCustomerFund = new List<CustomerFund>();

    protected void Page_Load(object sender, EventArgs e)
    {
        String WelcomeHeading = String.Empty;
        if (ThisCustomer == null)
        {
            ThisCustomer = (Page as AspDotNetStorefront.SkinBase).ThisCustomer;
        }

        WelcomeHeading = " Hi," + " " + ThisCustomer.FirstName.Trim() + " " + ThisCustomer.LastName.Trim();
        WelcomeHeadingAfterUserLogin.InnerText = WelcomeHeading;
        getCustomerfund();
    }

    private void getCustomerfund()
    {
        string customerLevel = "BLU Unlimited";
        lstCustomerFund = AuthenticationSSO.GetCustomerFund(ThisCustomer.CustomerID);
        lblCustomerLevel.Text = "Level: " + ((ThisCustomer.CustomerLevelName).Equals(customerLevel) ? "Partners" : ThisCustomer.CustomerLevelName);
        lblDealerLevel.Text = ((ThisCustomer.CustomerLevelName).Equals(customerLevel) ? "Partners" : ThisCustomer.CustomerLevelName.Replace("BLU",""));
        rptCustomerFunds.DataSource = lstCustomerFund;
        rptCustomerFunds.DataBind();
    }
    
}