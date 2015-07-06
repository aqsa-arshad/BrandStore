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
    /// Summary description for adjustcharge.
    /// </summary>
    public partial class adjustcharge : System.Web.UI.Page
    {
        protected void Page_Load(object sender, System.EventArgs e)
        {
            StringBuilder writer = new StringBuilder();
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");
            Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;
            int ONX = CommonLogic.QueryStringUSInt("OrderNumber");
            writer.Append("<div style=\"margin-left: 10px;\" align=\"left\">");
            if (!ThisCustomer.IsAdminUser)
            {
                writer.Append("<b><font color=red>"+ AppLogic.GetString("admin.adjustcharge.PermissionDenied", 1, ThisCustomer.LocaleSetting) + "</b></font>");
            }
            else
            {
                if (CommonLogic.FormBool("IsSubmit"))
                {
                    if (CommonLogic.FormCanBeDangerousContent("OrderTotal").Trim().Length != 0)
                    {
                        try
                        {
                            Decimal NewOrderTotal = CommonLogic.FormNativeDecimal("OrderTotal");
                            if (NewOrderTotal != 0.0M)
                            {
                                DB.ExecuteSQL(String.Format("update orders set CustomerServiceNotes={0}, OrderTotal={1} where OrderNumber={2}", DB.SQuote(CommonLogic.FormCanBeDangerousContent("CustomerServiceNotes")), Localization.CurrencyStringForDBWithoutExchangeRate(NewOrderTotal), ONX.ToString()));
                            }
                        }
                        catch { }
                    }
                    writer.Append("\n<script type=\"text/javascript\">\n");
                    writer.Append("opener.window.location.reload();\n");
                    writer.Append("self.close();\n");
                    writer.Append("</script>\n");
                }
                else
                {
                    Decimal OrderTotal = 0.0M;
                    String CustomerServiceNotes = String.Empty;

                    using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
                    {
                        dbconn.Open();
                        using(IDataReader rs = DB.GetRS(String.Format("select * from Orders with (NOLOCK) where OrderNumber={0}", ONX.ToString()),dbconn))
                        {
                            if (rs.Read())
                            {
                                OrderTotal = DB.RSFieldDecimal(rs, "OrderTotal");
                                CustomerServiceNotes = DB.RSField(rs, "CustomerServiceNotes");
                            }
                        }
                    }
                    writer.Append("<script type=\"text/javascript\">\n");
                    writer.Append("function AdjustChargeForm_Validator(theForm)\n");
                    writer.Append("	{\n");
                    writer.Append("	submitonce(theForm);\n");
                    writer.Append("	return (true);\n");
                    writer.Append("	}\n");
                    writer.Append("</script>\n");
                    writer.Append("<b>"+ AppLogic.GetString("admin.adjustcharge.lbl1",1 ,ThisCustomer.LocaleSetting)+ ONX.ToString() + "</b><br/><br/>");
                    writer.Append("<form id=\"AdjustChargeForm\" name=\"AdjustChargeForm\" method=\"POST\" action=\"" + AppLogic.AdminLinkUrl("adjustcharge.aspx") + "?OrderNumber=" + ONX.ToString() + "\" onsubmit=\"return (validateForm(this) && AdjustChargeForm_Validator(this))\" >");
                    writer.Append("<input type=\"hidden\" name=\"IsSubmit\" value=\"true\">\n");
                    writer.Append("<p>"+ AppLogic.GetString("admin.adjustcharge.lbl2",1 ,ThisCustomer.LocaleSetting)+ "<br/><br/>" + AppLogic.GetString("admin.adjustcharge.lbl3",1 ,ThisCustomer.LocaleSetting) +"</p>");
                    writer.Append("<p>" + AppLogic.GetString("admin.adjustcharge.lbl4", 1, ThisCustomer.LocaleSetting) + "<input type=\"text\" name=\"OrderTotal\" onkeypress=\"javascript:return WebForm_FireDefaultButton(event, 'btnadjust')\" value=\"" + Localization.CurrencyStringForDBWithoutExchangeRate(OrderTotal) + "\"><input type=\"hidden\" name=\"OrderTotal_vldt\" value=\"[req][number][blankalert=" + AppLogic.GetString("admin.adjustcharge.requiredmsg",1, ThisCustomer.LocaleSetting) + "][invalidalert=" + AppLogic.GetString("admin.adjustcharge.invalidmsg", 1, ThisCustomer.LocaleSetting) + "]\"></p>");
                    writer.Append("<p>" + AppLogic.GetString("admin.adjustcharge.CustomerSeverNotes",1, ThisCustomer.LocaleSetting) + "<br/>");
                    writer.Append("<textarea id=\"CustomerServiceNotes\" name=\"CustomerServiceNotes\" rows=\"20\" cols=\"50\">" + Server.HtmlEncode(CustomerServiceNotes) + "</textarea>");
                    writer.Append("<p><input type=\"submit\" class=\"normalButtons\" id=\"btnadjust\" value=\"Submit\" name=\"B1\"><input type=\"button\" class=\"normalButtons\" value=\"Cancel\" name=\"B2\" onClick=\"javascript:self.close()\"></p>");
                    writer.Append("</form>");
                }
            }
            writer.Append("</div>");
            ltContent.Text = writer.ToString();
        }

    }
}
