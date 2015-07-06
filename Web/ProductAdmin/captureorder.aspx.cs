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
using System.Data.SqlClient;
using System.Globalization;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways;

namespace AspDotNetStorefrontAdmin
{
	/// <summary>
	/// Summary description for captureorder.
	/// </summary>
    public partial class captureorder : System.Web.UI.Page
	{
		protected void Page_Load(object sender, System.EventArgs e)
		{
            StringBuilder writer = new StringBuilder();
            Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;
			writer.Append("<div align=\"left\">");
			int ONX = CommonLogic.QueryStringUSInt("OrderNumber");
            Order ord = new Order(ONX, ThisCustomer.LocaleSetting);
            
            if (!ThisCustomer.IsAdminUser) // safety check
			{
                writer.Append("<b><font color=red>" + AppLogic.GetString("admin.common.PermissionDeniedUC", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "</b></font>");
			}
			else
			{
				writer.Append("<b>" + String.Format(AppLogic.GetString("admin.captureorder.CaptureOrder", ThisCustomer.SkinID, ThisCustomer.LocaleSetting),ONX.ToString()) + "</b><br/><br/>");
                String Status = Gateway.OrderManagement_DoCapture(ord, ThisCustomer.LocaleSetting);
                writer.Append(String.Format(AppLogic.GetString("admin.captureorder.CaptureStatus", ThisCustomer.SkinID, ThisCustomer.LocaleSetting),Status));
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
