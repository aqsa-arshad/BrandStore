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


public partial class controls_UserInfoAfterLogin : System.Web.UI.UserControl
{
    Customer ThisCustomer;
    String WelcomeHeading = String.Empty;
    protected void Page_Load(object sender, EventArgs e)
    {
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
        if (ThisCustomer.CustomerLevelID == (int)UserType.SALESREPS)
        {
            decimal SAFAmount = AuthenticationSSO.GetCustomerFund(ThisCustomer.CustomerID,(int)FundType.SOFFunds).AmountAvailable;
            lblSOF.Text = "Sales Funds = " + String.Format("{0:C}", SAFAmount);
        }
    }
}