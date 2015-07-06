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
using System.Data.SqlClient;

namespace AspDotNetStorefrontAdmin
{
	/// <summary>
	/// Summary description for popuprt.
	/// </summary>
	public partial class popuprt : System.Web.UI.Page
	{
		protected void Page_Load(object sender, System.EventArgs e)
		{
            StringBuilder writer = new StringBuilder();
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");
            Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;			
			writer.Append("<div style=\"margin-left: 10px;\">");
			if(!ThisCustomer.IsAdminUser)
			{
                writer.Append("<b><font color=red>" + AppLogic.GetString("admin.common.PermissionDeniedUC", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "</b></font>");
			}
			else
			{
                using (SqlConnection conn = DB.dbConn())
                {
                    conn.Open();
                    using (IDataReader rs = DB.GetRS("Select * from orders   with (NOLOCK)  where ordernumber=" + CommonLogic.QueryStringUSInt("OrderNumber").ToString(), conn))
                    {
                        if (rs.Read())
                        {
                            String r1 = DB.RSField(rs, "RTShipRequest");
                            String r2 = DB.RSField(rs, "RTShipResponse");
                            String rqst = String.Empty;
                            try
                            {
                                rqst = XmlCommon.PrettyPrintXml(r1);
                            }
                            catch
                            {
                                rqst = r1;
                            }
                            String resp = String.Empty;
                            try
                            {
                                resp = XmlCommon.PrettyPrintXml(r2);
                            }
                            catch
                            {
                                resp = r2;
                            }
                            writer.Append("<b>" + AppLogic.GetString("admin.popuprt.RTShippingRequest", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + " </b><br/><br/><textarea rows=\"20\" style=\"width: 90%\">" + Server.HtmlEncode(r1) + "</textarea><br/><br/>");
                            writer.Append("<b>" + AppLogic.GetString("admin.popuprt.RTShippingResponse", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + " </b><br/><br/><textarea rows=\"35\" style=\"width: 90%\">" + Server.HtmlEncode(r2) + "</textarea><br/><br/>");
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
            ltContent.Text = writer.ToString();
        }

	}
}
