// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Data;
using System.Text;
using System.Globalization;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways;
namespace AspDotNetStorefrontAdmin
{
    public partial class amazontransaction : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            StringBuilder writer = new StringBuilder();
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;
            writer.Append("<div align=\"left\">");
            int ONX = CommonLogic.QueryStringUSInt("OrderNumber");
            Order ord = new Order(ONX, ThisCustomer.LocaleSetting);
            Customer c = new Customer(ord.CustomerID);

            if (!ThisCustomer.IsAdminUser) // safety check
            {
                writer.Append("<b><font color=red>"+ AppLogic.GetString("admin.common.PermissionDeniedUC", ThisCustomer.SkinID, ThisCustomer.LocaleSetting)+ "</b></font>");
            }
            else
            {
                writer.Append(AppLogic.GetString("admin.amazontransaction.UpdateTransaction", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + ONX.ToString() + "</b><br/><br/>");
                String Status = Gateway.OrderManagement_UpdateTransaction(ord, ThisCustomer.LocaleSetting);
                writer.Append("Status: " + Status);
                if (Status == AppLogic.ro_OK)
                {
                    writer.Append("<script type=\"text/javascript\">\n");
                    writer.Append("opener.window.location.reload();");
                    writer.Append("</script>\n");
                }
            }
            writer.Append("</div>");
            writer.Append("<p align=\"center\"><a href=\"javascript:self.close();\">" + AppLogic.GetString("admin.common.Close", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "</a></p>");
            ltContent.Text = writer.ToString();
        }
    }
}

