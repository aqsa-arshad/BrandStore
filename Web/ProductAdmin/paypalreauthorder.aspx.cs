// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Text;
using System.Data.SqlClient;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways;
using AspDotNetStorefrontGateways.Processors;

namespace AspDotNetStorefrontAdmin
{
	/// <summary>
    /// Summary description for paypalreauthorder.
	/// </summary>
	public partial class paypalreauthorder : System.Web.UI.Page
	{
		protected void Page_Load(object sender, System.EventArgs e)
		{
            StringBuilder writer = new StringBuilder();
			Response.CacheControl="private";
			Response.Expires=0;
			Response.AddHeader("pragma", "no-cache");
            Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;
			writer.Append("<div align=\"left\">");
			int ONX = CommonLogic.QueryStringUSInt("OrderNumber");

            if (!ThisCustomer.IsAdminUser) // safety check
			{
                writer.Append("<b><font color=red>" + AppLogic.GetString("admin.common.PermissionDeniedUC", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "</b></font>");
			}
			else
			{
				writer.Append(String.Format(AppLogic.GetString("admin.paypalreauthorder.ReAuthorizeOrder", ThisCustomer.SkinID, ThisCustomer.LocaleSetting),ONX.ToString()));

                using (SqlConnection conn = DB.dbConn())
                {
                    conn.Open();
                    using (IDataReader rs = DB.GetRS("Select * from orders   with (NOLOCK)  where ordernumber=" + ONX.ToString(), conn))
                    {
                        if (rs.Read())
                        {
                            String PM = AppLogic.CleanPaymentMethod(DB.RSField(rs, "PaymentMethod"));
                            if (DB.RSFieldDateTime(rs, "CapturedOn") == System.DateTime.MinValue)
                            {
                                if (DB.RSField(rs, "TransactionState") == AppLogic.ro_TXStateAuthorized)
                                {
                                    String Status = String.Empty;
                                    String GW = AppLogic.CleanPaymentGateway(DB.RSField(rs, "PaymentGateway"));
                                    if (PM == AppLogic.ro_PMPayPal || PM == AppLogic.ro_PMPayPalExpress)
                                    {
                                        GW = Gateway.ro_GWPAYPAL;
                                    }
                                    PayPal PayPalGW = new PayPal();
                                    Status = PayPalGW.ReAuthorizeOrder(ONX);
                                    writer.Append(String.Format(AppLogic.GetString("admin.paypalreauthorder.ReAuthorizeResponse", ThisCustomer.SkinID, ThisCustomer.LocaleSetting),Status));
                                    writer.Append("<script type=\"text/javascript\">\n");
                                    writer.Append("opener.window.location.reload();");
                                    writer.Append("</script>\n");
                                }
                                else
                                {
                                    writer.Append(String.Format(AppLogic.GetString("admin.paypalreauthorder.NotAuth", ThisCustomer.SkinID, ThisCustomer.LocaleSetting),DB.RSField(rs, "TransactionState")));
                                }
                            }
                            else
                            {
                                writer.Append(String.Format(AppLogic.GetString("admin.paypalreauthorder.CapturedOn", ThisCustomer.SkinID, ThisCustomer.LocaleSetting),Localization.ToNativeDateTimeString(DB.RSFieldDateTime(rs, "CapturedOn"))));
                            }
                        }
                        else
                        {
                            writer.Append("<b><font color=red>" + AppLogic.GetString("admin.common.OrderNotFoundUC", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "</b></font>");
                        }
                        rs.Close();
                    }
                }				
			}
			writer.Append("</div>");
			writer.Append("<p align=\"center\"><a href=\"javascript:self.close();\">" + AppLogic.GetString("admin.common.Close", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "</a></p>");
            ltContent.Text = writer.ToString();
		}
	}
}
