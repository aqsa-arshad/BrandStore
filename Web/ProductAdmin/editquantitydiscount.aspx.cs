// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Globalization;
using System.Text;
using AspDotNetStorefrontCore;
using System.Data.SqlClient;

namespace AspDotNetStorefrontAdmin
{
    /// <summary>
    /// Summary description for editquantitydiscount
    /// </summary>
    public partial class editquantitydiscount : AdminPageBase
    {

        int QuantityDiscountID;

        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");


            QuantityDiscountID = 0;

            if (CommonLogic.QueryStringCanBeDangerousContent("QuantityDiscountID").Length != 0 && CommonLogic.QueryStringCanBeDangerousContent("QuantityDiscountID") != "0")
            {
                Editing = true;
                QuantityDiscountID = Localization.ParseUSInt(CommonLogic.QueryStringCanBeDangerousContent("QuantityDiscountID"));
            }
            else
            {
                Editing = false;
            }

            if (CommonLogic.FormBool("IsSubmit"))
            {
                StringBuilder sql = new StringBuilder(2500);
                if (!Editing)
                {
                    // ok to add:
                    String NewGUID = DB.GetNewGUID();
                    sql.Append("insert into quantitydiscount(QuantityDiscountGUID,Name) values(");
                    sql.Append(DB.SQuote(NewGUID) + ",");
                    sql.Append(DB.SQuote(AppLogic.FormLocaleXml("Name")));
                    sql.Append(")");
                    DB.ExecuteSQL(sql.ToString());

                    using (SqlConnection dbconn = DB.dbConn())
                    {
                        dbconn.Open();
                        using (IDataReader rs = DB.GetRS("select QuantityDiscountID from quantitydiscount   with (NOLOCK)  where QuantityDiscountGUID=" + DB.SQuote(NewGUID), dbconn))
                        {
                            rs.Read();
                            QuantityDiscountID = DB.RSFieldInt(rs, "QuantityDiscountID");
                            Editing = true;
                        }
                    }
                    DataUpdated = true;
                    Response.Redirect(AppLogic.AdminLinkUrl("editquantitydiscounttable.aspx") + "?QuantityDiscountID=" + QuantityDiscountID.ToString());
                }
                else
                {
                    // ok to update:
                    sql.Append("update quantitydiscount set ");
                    sql.Append("Name=" + DB.SQuote(AppLogic.FormLocaleXml("Name")));
                    sql.Append(" where QuantityDiscountID=" + QuantityDiscountID.ToString());
                    DB.ExecuteSQL(sql.ToString());
                    DataUpdated = true;
                    Editing = true;
                }
            }
            SectionTitle = "<a href=\"" + AppLogic.AdminLinkUrl("quantitydiscounts.aspx") + "\">" + AppLogic.GetString("admin.menu.QuantityDiscounts", SkinID, LocaleSetting) + "</a> - " + AppLogic.GetString("admin.editquantitydiscount.ManageQuantityDiscounts", SkinID, LocaleSetting);
			RenderHtml();
        }

		private void RenderHtml()
        {
            StringBuilder writer = new StringBuilder();
            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("select * from QuantityDiscount   with (NOLOCK)  where QuantityDiscountID=" + QuantityDiscountID.ToString(), dbconn))
                {
                    if (rs.Read())
                    {
                        Editing = true;
                    }

                    if (ErrorMsg.Length != 0)
                    {
                        writer.Append("<p><b><font color=red>" + ErrorMsg + "</font></b></p>\n");
                    }
                    if (DataUpdated)
                    {
                        writer.Append("<p align=\"left\"><b><font color=blue>" + AppLogic.GetString("admin.editCreditCard.Updated", SkinID, LocaleSetting) + "</font></b></p>\n");
                    }



                    if (ErrorMsg.Length == 0)
                    {

                        if (Editing)
                        {
                            writer.Append("<b>" + String.Format(AppLogic.GetString("admin.editquantitydiscount.EditingQuantityDiscounts", SkinID, LocaleSetting),DB.RSFieldByLocale(rs, "Name", LocaleSetting),DB.RSFieldInt(rs, "QuantityDiscountID").ToString()) + "<br/><br/></b>\n");
                        }
                        else
                        {
                            writer.Append("<div style=\"height:17;padding-top:3px;\" class=\"tablenormal\">" + AppLogic.GetString("admin.editquantitydiscount.AddNewQuantityDiscount", SkinID, LocaleSetting) + "</div><br/></b>\n");
                        }

                        writer.Append("<script type=\"text/javascript\">\n");
                        writer.Append("function QuantityDiscountForm_Validator(theForm)\n");
                        writer.Append("{\n");
                        writer.Append("submitonce(theForm);\n");
                        writer.Append("return (true);\n");
                        writer.Append("}\n");
                        writer.Append("</script>\n");

                        writer.Append("<p>" + AppLogic.GetString("admin.editquantitydiscount.QuantityDiscountInfo", SkinID, LocaleSetting) + "</p>\n");
                        writer.Append("<form action=\"" + AppLogic.AdminLinkUrl("editquantitydiscount.aspx") + "?QuantityDiscountID=" + QuantityDiscountID.ToString() + "&edit=" + Editing.ToString() + "\" method=\"post\" id=\"QuantityDiscountForm\" name=\"QuantityDiscountForm\" onsubmit=\"return (validateForm(this) && QuantityDiscountForm_Validator(this))\" onReset=\"return confirm('Do you want to reset all fields to their starting values?');\">\n");
                        writer.Append("<input type=\"hidden\" name=\"IsSubmit\" value=\"true\">\n");
                        writer.Append("<table width=\"100%\" cellpadding=\"4\" cellspacing=\"0\">\n");
                        writer.Append("              <tr valign=\"middle\">\n");
                        writer.Append("                <td width=\"100%\" colspan=\"2\" align=\"left\">\n");
                        writer.Append("                </td>\n");
                        writer.Append("              </tr>\n");
                        writer.Append("              <tr valign=\"middle\">\n");
                        writer.Append("                <td width=\"25%\" align=\"right\" valign=\"middle\">*" + AppLogic.GetString("admin.common.Name", SkinID, LocaleSetting) + ":&nbsp;&nbsp;</td>\n");
                        writer.Append("                <td align=\"left\" valign=\"top\">\n");
                        writer.Append(AppLogic.GetLocaleEntryFields(DB.RSField(rs, "Name"), "Name", false, true, true, AppLogic.GetString("admin.editquantitydiscount.EnterQuantityDiscount", SkinID, LocaleSetting), 100, 30, 0, 0, false));
                        
                        writer.Append("                	</td>\n");
                        writer.Append("              </tr>\n");

                        writer.Append("<tr>\n");
                        writer.Append("<td></td><td align=\"left\" valign=\"top\"><br/>\n");
                        if (Editing)
                        {
                            writer.Append("<input type=\"submit\" value=\"" + AppLogic.GetString("admin.common.Update", SkinID, LocaleSetting) + "\" name=\"submit\">\n");
                        }
                        else
                        {
                            writer.Append("<input type=\"submit\" class=\"normalButtons\" value=\"" + AppLogic.GetString("admin.common.AddNew", SkinID, LocaleSetting) + "\" name=\"submit\">\n");
                        }
                        writer.Append("        </td>\n");
                        writer.Append("      </tr>\n");
                        writer.Append("  </table>\n");
                        writer.Append("</form>\n");

                    }
                }
            }
            ltContent.Text = writer.ToString();
        }

    }
}
