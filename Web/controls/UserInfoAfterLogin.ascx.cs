﻿using System;
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
    string WelcomeHeading;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (ThisCustomer == null)
        {
            ThisCustomer = (Page as AspDotNetStorefront.SkinBase).ThisCustomer;
        }
        WelcomeHeading=" HI"+" "+ThisCustomer.FirstName.Trim()+" "+ThisCustomer.LastName.Trim();
        UserInfo.InnerText = WelcomeHeading;
    }
}