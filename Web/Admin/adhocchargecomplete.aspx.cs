// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Text;
using System.Data;
using System.Globalization;
using AspDotNetStorefrontCore;

public partial class Admin_adhocchargecomplete : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        StringBuilder writer = new StringBuilder();
        Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;
        
        //This wil update and refresh the parent window                 
        writer.Append("<p><b><font color=blue>" + AppLogic.GetString("adhoccharge.aspx.2", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + CommonLogic.QueryStringCanBeDangerousContent("ordernumber") + "</font></b></p>");
        writer.Append("<p><a href=\"javascript:self.close();\">" + AppLogic.GetString("admin.common.Close", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "</a></p>");
        ltContent.Text = writer.ToString();
    }
}
