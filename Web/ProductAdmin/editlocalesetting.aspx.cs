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
    /// Summary description for editLocaleSetting
    /// </summary>
    public partial class editLocaleSetting : AdminPageBase
    {

        int LocaleSettingID;

        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");


            LocaleSettingID = 0;

            if (CommonLogic.QueryStringCanBeDangerousContent("LocaleSettingID").Length != 0 && CommonLogic.QueryStringCanBeDangerousContent("LocaleSettingID") != "0")
            {
                Editing = true;
                LocaleSettingID = Localization.ParseUSInt(CommonLogic.QueryStringCanBeDangerousContent("LocaleSettingID"));
            }
            else
            {
                Editing = false;
            }

            if (CommonLogic.FormBool("IsSubmit"))
            {
                String redirectlink = "<a href=\"javascript:history.back(-1);\">go back</a>";
                if (Editing)
                {
                    // see if this LocaleSetting already exists:
                    int N = DB.GetSqlN("select count(name) as N from LocaleSetting   with (NOLOCK)  where LocaleSettingID<>" + LocaleSettingID.ToString() + " and Name=" + DB.SQuote(CommonLogic.FormCanBeDangerousContent("Name")));
                    if (N != 0)
                    {
                        ErrorMsg = "<p><b><font color=red>" + AppLogic.GetString("admin.common.Error", SkinID, LocaleSetting) + "<br/><br/></font><blockquote>" + String.Format(AppLogic.GetString("admin.editlocalesetting.ExistingLocale", SkinID, LocaleSetting),redirectlink) + "</b></blockquote></p>";
                    }
                }
                else
                {
                    // see if this name is already there:
                    int N = DB.GetSqlN("select count(name) as N from LocaleSetting   with (NOLOCK)  where Name=" + DB.SQuote(CommonLogic.FormCanBeDangerousContent("Name")));
                    if (N != 0)
                    {
                        ErrorMsg = "<p><b><font color=red>ERROR:<br/><br/></font><blockquote>" + String.Format(AppLogic.GetString("admin.editlocalesetting.ExistingLocale", SkinID, LocaleSetting),redirectlink) + "</b></blockquote></p>";
                    }
                }

                if (ErrorMsg.Length == 0)
                {
                    StringBuilder sql = new StringBuilder(2500);
                    if (!Editing)
                    {
                        // ok to add them:
                        String NewGUID = DB.GetNewGUID();
                        sql.Append("insert into LocaleSetting(LocaleSettingGUID,Name,Description,DefaultCurrencyID) values(");
                        sql.Append(DB.SQuote(NewGUID) + ",");
                        sql.Append(DB.SQuote(CommonLogic.Left(CommonLogic.FormCanBeDangerousContent("Name"), 10)) + ",");
                        sql.Append(DB.SQuote(CommonLogic.Left(CommonLogic.FormCanBeDangerousContent("Description"), 100)) + ",");
                        sql.Append(Currency.GetCurrencyID(CommonLogic.FormCanBeDangerousContent("DefaultCurrency")).ToString());
                        sql.Append(")");
                        DB.ExecuteSQL(sql.ToString());

                        using (SqlConnection dbconn = DB.dbConn())
                        {
                            dbconn.Open();
                            using (IDataReader rs = DB.GetRS("select LocaleSettingID from LocaleSetting   with (NOLOCK)  where LocaleSettingGUID=" + DB.SQuote(NewGUID), dbconn))
                            {
                                rs.Read();
                                LocaleSettingID = DB.RSFieldInt(rs, "LocaleSettingID");
                                Editing = true;
                            }
                        }
                        DataUpdated = true;
                        AppLogic.UpdateNumLocaleSettingsInstalled();
                    }
                    else
                    {
                        // ok to update:
                        sql.Append("update LocaleSetting set ");
                        sql.Append("Name=" + DB.SQuote(CommonLogic.Left(CommonLogic.FormCanBeDangerousContent("Name"), 10)) + ",");
                        sql.Append("Description=" + DB.SQuote(CommonLogic.Left(CommonLogic.FormCanBeDangerousContent("Description"), 100)) + ",");
                        sql.Append("DefaultCurrencyID=" + Currency.GetCurrencyID(CommonLogic.FormCanBeDangerousContent("DefaultCurrency")).ToString());
                        sql.Append(" where LocaleSettingID=" + LocaleSettingID.ToString());
                        DB.ExecuteSQL(sql.ToString());
                        DataUpdated = true;
                        Editing = true;
                    }
                }
            }
            SectionTitle = "<a href=\"" + AppLogic.AdminLinkUrl("localesettings.aspx") + "\">" + AppLogic.GetString("admin.menu.LocaleSettings", SkinID, LocaleSetting) + "</a> - " + String.Format(AppLogic.GetString("admin.editlocalesetting.ManageLocales", SkinID, LocaleSetting), CommonLogic.IIF(DataUpdated, " (Updated)", ""));
            RenderHtml();
        }

        private void RenderHtml()
        {
            StringBuilder writer = new StringBuilder();
            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("select * from LocaleSetting   with (NOLOCK)  where LocaleSettingID=" + LocaleSettingID.ToString(), dbconn))
                {
                    if (rs.Read())
                    {
                        Editing = true;
                    }

                    if (ErrorMsg.Length != 0)
                    {
                        writer.Append("<p><b><font color=red>" + ErrorMsg + "</font></b></p>\n");
                    }

                    if (ErrorMsg.Length == 0)
                    {

                        if (Editing)
                        {
                            writer.Append("<b>" + String.Format(AppLogic.GetString("admin.editlocalesetting.EditingLocale", SkinID, LocaleSetting),DB.RSField(rs, "Name"),DB.RSFieldInt(rs, "LocaleSettingID").ToString()) + "<br/><br/></b>\n");
                        }
                        else
                        {
                            writer.Append("<div style=\"height:17;padding-top:3px;\" class=\"tablenormal\">" + AppLogic.GetString("admin.editlocalesetting.AddNewLocale", SkinID, LocaleSetting) + ":<div/><br/></b>\n");
                        }

                        writer.Append("<script type=\"text/javascript\">\n");
                        writer.Append("function Form_Validator(theForm)\n");
                        writer.Append("{\n");
                        writer.Append("submitonce(theForm);\n");
                        writer.Append("return (true);\n");
                        writer.Append("}\n");
                        writer.Append("</script>\n");

                        writer.Append("<p>" + AppLogic.GetString("admin.editlocalesetting.LocaleInfo", SkinID, LocaleSetting) + "</p>\n");
                        writer.Append("<form action=\"" + AppLogic.AdminLinkUrl("editlocalesetting.aspx") + "?LocaleSettingID=" + LocaleSettingID.ToString() + "&edit=" + Editing.ToString() + "\" method=\"post\" id=\"Form1\" name=\"Form1\" onsubmit=\"return (validateForm(this) && Form_Validator(this))\" onReset=\"return confirm('" + AppLogic.GetString("admin.common.ResetAllFieldsPrompt", SkinID, LocaleSetting) + "');\">\n");
                        writer.Append("<input type=\"hidden\" name=\"IsSubmit\" value=\"true\">\n");
                        writer.Append("<table width=\"100%\" cellpadding=\"4\" cellspacing=\"0\">\n");
                        writer.Append("<tr valign=\"middle\">\n");
                        writer.Append("<td width=\"100%\" colspan=\"2\" align=\"left\">\n");
                        writer.Append("</td>\n");
                        writer.Append("</tr>\n");

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
                        writer.Append("</td>\n");
                        writer.Append("</tr>\n");

                        writer.Append("<tr valign=\"middle\">\n");
                        writer.Append("<td width=\"25%\" align=\"right\" valign=\"middle\">*" + AppLogic.GetString("admin.orderframe.LocaleSetting", SkinID, LocaleSetting) + "&nbsp;&nbsp;</td>\n");
                        writer.Append("<td align=\"left\" valign=\"top\">\n");
                        writer.Append("<input maxLength=\"10\" size=\"10\" name=\"Name\" value=\"" + CommonLogic.IIF(Editing, Server.HtmlEncode(DB.RSField(rs, "Name")), "") + "\"> " + AppLogic.GetString("admin.editlocalesetting.Sample", SkinID, LocaleSetting) + "\n");
                        writer.Append("<input type=\"hidden\" name=\"Name_vldt\" value=\"[req][blankalert=" + AppLogic.GetString("admin.editlocalesetting.EnterLocale", SkinID, LocaleSetting) + "]\">\n");
                        writer.Append("</td>\n");
                        writer.Append("</tr>\n");

                        writer.Append("<tr valign=\"middle\">\n");
                        writer.Append("<td width=\"25%\" align=\"right\" valign=\"middle\">*" + AppLogic.GetString("admin.common.Description", SkinID, LocaleSetting) + ":&nbsp;&nbsp;</td>\n");
                        writer.Append("<td align=\"left\" valign=\"top\">\n");
                        writer.Append("<input maxLength=\"100\" size=\"50\" name=\"Description\" value=\"" + CommonLogic.IIF(Editing, Server.HtmlEncode(DB.RSField(rs, "Description")), "") + "\"> " + AppLogic.GetString("admin.editlocalesetting.SampleDescription", SkinID, LocaleSetting) + "\n");
                        writer.Append("<input type=\"hidden\" name=\"Description_vldt\" value=\"[req][blankalert=" + AppLogic.GetString("admin.editlocalesetting.EnterDescription", SkinID, LocaleSetting) + "]\">\n");
                        writer.Append("</td>\n");
                        writer.Append("</tr>\n");

                        writer.Append("<tr valign=\"middle\">\n");
                        writer.Append("<td width=\"25%\" align=\"right\" valign=\"middle\">*" + AppLogic.GetString("admin.editlocalesetting.DefaultCurrency", SkinID, LocaleSetting) + ":&nbsp;&nbsp;</td>\n");
                        writer.Append("<td align=\"left\" valign=\"top\">\n");
                        String DefCur = Localization.GetPrimaryCurrency();
                        if (Editing)
                        {
                            DefCur = Currency.GetCurrencyCode(DB.RSFieldInt(rs, "DefaultCurrencyID"));
                        }
                        String CurrencySelectList = Currency.GetSelectList("DefaultCurrency", String.Empty, String.Empty, DefCur);
                        writer.Append(CurrencySelectList);
                        writer.Append("</td>\n");
                        writer.Append("</tr>\n");

                        writer.Append("<tr>\n");
                        writer.Append("<td></td><td align=\"left\" valign=\"top\"><br/>\n");
                        if (Editing)
                        {
                            writer.Append("<input type=\"submit\" value=\"" + AppLogic.GetString("admin.common.Update", SkinID, LocaleSetting) + "\" name=\"submit\">\n");
                            writer.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<input type=\"reset\" value=\"" + AppLogic.GetString("admin.common.Reset", SkinID, LocaleSetting) + "\" name=\"reset\">\n");
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
