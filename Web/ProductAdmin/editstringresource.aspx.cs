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
using System.Web;
using System.IO;
using AspDotNetStorefrontCore;
using System.Data.SqlClient;

namespace AspDotNetStorefrontAdmin
{
    /// <summary>
    /// Summary LocaleSetting for editstringresource
    /// </summary>
    public partial class editstringresource : AdminPageBase
    {

        String SearchFor;
        String ShowLocaleSetting;
        String BeginsWith;
        int StringResourceID;

        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");


            SearchFor = CommonLogic.QueryStringCanBeDangerousContent("SearchFor");
            ShowLocaleSetting = Localization.CheckLocaleSettingForProperCase(CommonLogic.QueryStringCanBeDangerousContent("ShowLocaleSetting"));
            BeginsWith = CommonLogic.QueryStringCanBeDangerousContent("BeginsWith");
            StringResourceID = 0;

            if (CommonLogic.QueryStringCanBeDangerousContent("StringResourceID").Length != 0 && CommonLogic.QueryStringCanBeDangerousContent("StringResourceID") != "0")
            {
                Editing = true;
                StringResourceID = Localization.ParseUSInt(CommonLogic.QueryStringCanBeDangerousContent("StringResourceID"));
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
                    sql.Append("insert into StringResource(StringResourceGUID,Name,LocaleSetting,ConfigValue) values(");
                    sql.Append(DB.SQuote(NewGUID) + ",");
                    sql.Append(DB.SQuote(CommonLogic.FormCanBeDangerousContent("Name")) + ",");
                    sql.Append(DB.SQuote(Localization.CheckLocaleSettingForProperCase(CommonLogic.FormCanBeDangerousContent("LocaleSetting"))) + ",");
                    sql.Append(DB.SQuote(CommonLogic.FormCanBeDangerousContent("ConfigValue")));
                    sql.Append(")");
                    DB.ExecuteSQL(sql.ToString());

                    using (SqlConnection dbconn = DB.dbConn())
                    {
                        dbconn.Open();
                        using (IDataReader rs = DB.GetRS("select StringResourceID from StringResource   with (NOLOCK)  where StringResourceGUID=" + DB.SQuote(NewGUID), dbconn))
                        {
                            rs.Read();
                            StringResourceID = DB.RSFieldInt(rs, "StringResourceID");
                            Editing = true;
                        }
                    }
                    DataUpdated = true;
                }
                else
                {
                    // ok to update:
                    sql.Append("update StringResource set ");
                    sql.Append("Name=" + DB.SQuote(CommonLogic.FormCanBeDangerousContent("Name")) + ",");
                    sql.Append("LocaleSetting=" + DB.SQuote(Localization.CheckLocaleSettingForProperCase(CommonLogic.FormCanBeDangerousContent("LocaleSetting"))) + ",");
                    sql.Append("ConfigValue=" + DB.SQuote(CommonLogic.FormCanBeDangerousContent("ConfigValue")));
                    sql.Append(" where StringResourceID=" + StringResourceID.ToString());
                    DB.ExecuteSQL(sql.ToString());
                    DataUpdated = true;
                    Editing = true;
                }
            }
            RenderHtml();
        }

        private void RenderHtml()
        {
            StringBuilder writer = new StringBuilder();
            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("select * from StringResource   with (NOLOCK)  where StringResourceID=" + StringResourceID.ToString(), dbconn))
                {
                    if (rs.Read())
                    {
                        Editing = true;
                    }

                    if (ErrorMsg.Length != 0)
                    {
                        writer.Append("<p align=\"left\"><b><font color=red>" + ErrorMsg + "</font></b></p>\n");
                    }
                    if (DataUpdated)
                    {
                        writer.Append("<p align=\"left\"><b><font color=blue>" + AppLogic.GetString("admin.editstringresource.Updated", SkinID, LocaleSetting) + "</font></b></p>\n");
                    }


                    if (ErrorMsg.Length == 0)
                    {

                        if (Editing)
                        {
                            writer.Append("<div class=\"tablenormal\">" + AppLogic.GetString("admin.editstringresource.Edit", SkinID, LocaleSetting) + " " + DB.RSField(rs, "Name") + " (ID=" + DB.RSFieldInt(rs, "StringResourceID").ToString() + ")</div>\n");
                        }
                        else
                        {
                            writer.Append("<div style=\"height:19;padding-top:5px;\" class=\"tablenormal\">" + AppLogic.GetString("admin.editstringresource.New", SkinID, LocaleSetting) + "</div>\n");
                        }

                        writer.Append("<script type=\"text/javascript\">\n");
                        writer.Append("function Form_Validator(theForm)\n");
                        writer.Append("{\n");
                        writer.Append("submitonce(theForm);\n");
                        writer.Append("return (true);\n");
                        writer.Append("}\n");
                        writer.Append("</script>\n");

                        writer.Append(AppLogic.GetString("admin.editstringresource.msg", SkinID, LocaleSetting));
                        writer.Append("<form enctype=\"multipart/form-data\" action=\"" + AppLogic.AdminLinkUrl("editStringResource.aspx") + "?ShowLocaleSetting=" + Server.UrlEncode(ShowLocaleSetting) + "&beginsWith=" + Server.UrlEncode(BeginsWith) + "&searchfor=" + Server.UrlEncode(SearchFor) + "&StringResourceID=" + StringResourceID.ToString() + "&edit=" + Editing.ToString() + "\" method=\"post\" id=\"Form1\" name=\"Form1\" onsubmit=\"return (validateForm(this) && Form_Validator(this))\" onReset=\"return confirm('" + AppLogic.GetString("admin.common.ResetAllFieldsPrompt", SkinID, LocaleSetting) + "');\">\n");
                        writer.Append("<input type=\"hidden\" name=\"IsSubmit\" value=\"true\">\n");
                        writer.Append("<table width=\"100%\" cellpadding=\"4\" cellspacing=\"0\">\n");
                        writer.Append("              <tr valign=\"middle\">\n");
                        writer.Append("                <td width=\"100%\" colspan=\"2\" align=\"left\">\n");
                        writer.Append("                </td>\n");
                        writer.Append("              </tr>\n");
                        writer.Append("              <tr valign=\"middle\">\n");
                        writer.Append("                <td width=\"25%\" align=\"right\" valign=\"middle\">" + AppLogic.GetString("admin.editstringresource.Name", SkinID, LocaleSetting) + "</td>\n");
                        writer.Append("                <td align=\"left\" valign=\"top\">\n");
                        writer.Append("                	<input maxLength=\"100\" size=\"50\" name=\"Name\" value=\"" + CommonLogic.IIF(Editing, Server.HtmlEncode(DB.RSField(rs, "Name")), "") + "\">\n");
                        writer.Append("                	<input type=\"hidden\" name=\"Name_vldt\" value=\"[req][blankalert=" + AppLogic.GetString("admin.editstringresource.NameError", SkinID, LocaleSetting) + "]\">\n");
                        writer.Append("                	</td>\n");
                        writer.Append("              </tr>\n");

                        writer.Append("              <tr valign=\"middle\">\n");
                        writer.Append("                <td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.editstringresource.LocaleSetting", SkinID, LocaleSetting) + "</td>\n");
                        writer.Append("                <td align=\"left\" valign=\"top\">\n");

                        StringBuilder tmpS = new StringBuilder(4096);                        

                        using (SqlConnection dbconn2 = DB.dbConn())
                        {
                            dbconn2.Open();
                            using (IDataReader rs2 = DB.GetRS("select * from LocaleSetting   with (NOLOCK)  order by displayorder,description", dbconn2))
                            {
                                tmpS.Append("<!-- COUNTRY SELECT LIST -->\n");
                                tmpS.Append("<select size=\"1\" id=\"LocaleSetting\" name=\"LocaleSetting\">");

                                while (rs2.Read())
                                {
                                    tmpS.Append("<option value=\"" + DB.RSField(rs2, "Name") + "\" " + CommonLogic.IIF(DB.RSField(rs, "LocaleSetting") == DB.RSField(rs2, "Name"), " selected ", "") + ">" + DB.RSField(rs2, "Name") + "</option>");
                                }

                                tmpS.Append("</select>");
                                tmpS.Append("<!-- END COUNTRY SELECT LIST -->\n");
                            }
                        }

                        writer.Append(tmpS);

                        writer.Append("                </td>\n");
                        writer.Append("              </tr>\n");

                        writer.Append("              <tr valign=\"middle\">\n");
                        writer.Append("                <td width=\"25%\" align=\"right\" valign=\"middle\">" + AppLogic.GetString("admin.editstringresource.Value", SkinID, LocaleSetting) + "</td>\n");
                        writer.Append("                <td align=\"left\" valign=\"top\">\n");
                        writer.Append("                	<input maxLength=\"2500\" size=\"150\" name=\"ConfigValue\" value=\"" + CommonLogic.IIF(Editing, Server.HtmlEncode(DB.RSField(rs, "ConfigValue")), "") + "\">\n");
                        writer.Append("                	</td>\n");
                        writer.Append("              </tr>\n");

                        writer.Append("<tr>\n");
                        writer.Append("<td></td><td align=\"left\" valign=\"top\"><br/>\n");
                        if (Editing)
                        {
                            writer.Append("<input type=\"submit\" class=\"normalButtons\" value=" + AppLogic.GetString("admin.common.Update", SkinID, LocaleSetting) + " name=\"submit\">\n");
                            writer.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<input type=\"reset\" class=\"normalButtons\" value=" + AppLogic.GetString("admin.common.Reset", SkinID, LocaleSetting) + " name=\"reset\">\n");
                        }
                        else
                        {
                            writer.Append("<input type=\"submit\" class=\"normalButtons\" value=" + AppLogic.GetString("admin.common.AddNew", SkinID, LocaleSetting) + " name=\"submit\">\n");
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
