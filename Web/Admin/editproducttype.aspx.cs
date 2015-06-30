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
    /// Summary description for editproducttype
    /// </summary>
    public partial class editproducttype : AdminPageBase
    {

        int producttypeID;

        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");


            producttypeID = 0;

            if (CommonLogic.QueryStringCanBeDangerousContent("producttypeID").Length != 0 && CommonLogic.QueryStringCanBeDangerousContent("producttypeID") != "0")
            {
                Editing = true;
                producttypeID = Localization.ParseUSInt(CommonLogic.QueryStringCanBeDangerousContent("producttypeID"));
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
                    // ok to add them:
                    String NewGUID = DB.GetNewGUID();
                    sql.Append("insert into producttype(producttypeGUID,Name) values(");
                    sql.Append(DB.SQuote(NewGUID) + ",");
                    sql.Append(DB.SQuote(AppLogic.FormLocaleXml("Name")));
                    sql.Append(")");
                    DB.ExecuteSQL(sql.ToString());

                    using (SqlConnection dbconn = DB.dbConn())
                    {
                        dbconn.Open();
                        using (IDataReader rs = DB.GetRS("select producttypeID from producttype   with (NOLOCK)  where producttypeGUID=" + DB.SQuote(NewGUID), dbconn))
                        {
                            rs.Read();
                            producttypeID = DB.RSFieldInt(rs, "producttypeID");
                            Editing = true;
                        }
                    }
                    DataUpdated = true;
                }
                else
                {
                    // ok to update:
                    sql.Append("update producttype set ");
                    sql.Append("Name=" + DB.SQuote(AppLogic.FormLocaleXml("Name")));
                    sql.Append(" where producttypeID=" + producttypeID.ToString());
                    DB.ExecuteSQL(sql.ToString());
                    DataUpdated = true;
                    Editing = true;
                }
            }
            SectionTitle = "<a href=\"" + AppLogic.AdminLinkUrl("producttypes.aspx") + "\">" + AppLogic.GetString("admin.editproducttype.ProductTypes", SkinID, LocaleSetting) + "</a> - " + AppLogic.GetString("admin.menu.ProductTypes", SkinID, LocaleSetting);
            RenderHtml();
        }

        private void RenderHtml()
        {
            StringBuilder writer = new StringBuilder();
            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("select * from producttype   with (NOLOCK)  where producttypeID=" + producttypeID.ToString(), dbconn))
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
                            writer.Append("<b>" + String.Format(AppLogic.GetString("admin.editproducttype.EditingProductType", SkinID, LocaleSetting),DB.RSFieldByLocale(rs, "Name", LocaleSetting)) + "<br/><br/></b>\n");
                        }
                        else
                        {
                            writer.Append("<div style=\"height:17;padding-top:3px;\" class=\"tablenormal\">" + AppLogic.GetString("admin.editproducttype.AddNewProductType", SkinID, LocaleSetting) + "</div><br/></b>\n");
                        }

                        writer.Append("<script type=\"text/javascript\">\n");
                        writer.Append("function Form_Validator(theForm)\n");
                        writer.Append("{\n");
                        writer.Append("submitonce(theForm);\n");
                        writer.Append("return (true);\n");
                        writer.Append("}\n");
                        writer.Append("</script>\n");

                        writer.Append("<script type='text/javascript' src='Scripts/tabs.js'></script>");

                        writer.Append("<p>" + AppLogic.GetString("admin.editproducttype.ProductTypeInfo", SkinID, LocaleSetting) + "</p>\n");
                        writer.Append("<form enctype=\"multipart/form-data\" action=\"" + AppLogic.AdminLinkUrl("editproducttype.aspx") + "?producttypeID=" + producttypeID.ToString() + "&edit=" + Editing.ToString() + "\" method=\"post\" id=\"Form1\" name=\"Form1\" onsubmit=\"return (validateForm(this) && Form_Validator(this))\" onReset=\"return confirm('Do you want to reset all fields to their starting values?');\">\n");
                        writer.Append("<input type=\"hidden\" name=\"IsSubmit\" value=\"true\">\n");
                        writer.Append("<table width=\"100%\" cellpadding=\"4\" cellspacing=\"0\">\n");
                        writer.Append("              <tr valign=\"middle\">\n");
                        writer.Append("                <td width=\"100%\" colspan=\"2\" align=\"left\">\n");
                        writer.Append("                </td>\n");
                        writer.Append("              </tr>\n");
                        writer.Append("              <tr valign=\"middle\">\n");
                        writer.Append("                <td width=\"25%\" align=\"right\" valign=\"middle\">*" + AppLogic.GetString("admin.qbproduct.aspx.ProductType", SkinID, LocaleSetting) + ":&nbsp;&nbsp;</td>\n");
                        writer.Append("                <td align=\"left\" valign=\"top\">\n");
                        writer.Append(AppLogic.GetLocaleEntryFields(DB.RSField(rs, "Name"), "Name", false, true, true, AppLogic.GetString("admin.editproducttype.EnterProductType", SkinID, LocaleSetting), 100, 30, 0, 0, false));
                        
                        writer.Append("                	</td>\n");
                        writer.Append("              </tr>\n");

                        writer.Append("<tr>\n");
                        writer.Append("<td></td><td align=\"left\" valign=\"top\"><br/>\n");
                        if (Editing)
                        {
                            writer.Append("<input type=\"submit\" value=\"" + AppLogic.GetString("admin.common.Update", SkinID, LocaleSetting) + "\" name=\"submit\">\n");
                            writer.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<input type=\"reset\" class=\"CPButton\" value=\"" + AppLogic.GetString("admin.common.Reset", SkinID, LocaleSetting) + "\" name=\"reset\">\n");
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
