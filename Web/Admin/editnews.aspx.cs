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
    /// Summary NewsCopy for editnews
    /// </summary>
    public partial class editnews : AdminPageBase
    {

        int NewsID;

        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache"); 
            

            NewsID = 0;
            if (CommonLogic.QueryStringCanBeDangerousContent("NewsID").Length != 0 && CommonLogic.QueryStringCanBeDangerousContent("NewsID") != "0")
            {
                Editing = true;
                NewsID = Localization.ParseUSInt(CommonLogic.QueryStringCanBeDangerousContent("NewsID"));
            }
            else
            {
                Editing = false;
            }

            if (CommonLogic.FormBool("IsSubmit"))
            {
                StringBuilder sql = new StringBuilder(2500);
                DateTime dt = System.DateTime.Now.AddMonths(6);
                if (CommonLogic.FormCanBeDangerousContent("ExpiresOn").Length > 0)
                {
                    dt = Localization.ParseNativeDateTime(CommonLogic.FormCanBeDangerousContent("ExpiresOn"));
                }
                if (!Editing)
                {
                    // ok to add them:
                    String NewGUID = DB.GetNewGUID();
                    sql.Append("insert into news(NewsGUID,ExpiresOn,Headline,NewsCopy,Published) values(");
                    sql.Append(DB.SQuote(NewGUID) + ",");
                    sql.Append(DB.DateQuote(Localization.ToDBDateTimeString(dt)) + ",");
                    sql.Append(DB.SQuote(AppLogic.FormLocaleXml("Headline")) + ",");
                    sql.Append(DB.SQuote(AppLogic.FormLocaleXml("NewsCopy")) + ",");
                    sql.Append(CommonLogic.FormCanBeDangerousContent("Published"));
                    sql.Append(")");
                    DB.ExecuteSQL(sql.ToString());

                    using (SqlConnection dbconn = DB.dbConn())
                    {
                        dbconn.Open();
                        using (IDataReader rs = DB.GetRS("select NewsID from news  with (NOLOCK)  where deleted=0 and NewsGUID=" + DB.SQuote(NewGUID), dbconn))
                        {
                            rs.Read();
                            NewsID = DB.RSFieldInt(rs, "NewsID");
                            Editing = true;
                        }
                    }
                    DataUpdated = true;
                }
                else
                {
                    // ok to update:
                    sql.Append("update news set ");
                    sql.Append("Headline=" + DB.SQuote(AppLogic.FormLocaleXml("Headline")) + ",");
                    sql.Append("NewsCopy=" + DB.SQuote(AppLogic.FormLocaleXml("NewsCopy")) + ",");
                    sql.Append("ExpiresOn=" + DB.DateQuote(Localization.ToDBDateTimeString(dt)) + ",");
                    sql.Append("Published=" + CommonLogic.FormCanBeDangerousContent("Published"));
                    sql.Append(" where NewsID=" + NewsID.ToString());
                    DB.ExecuteSQL(sql.ToString());
                    DataUpdated = true;
                    Editing = true;
                }
            }
            SectionTitle = "<a href=\"" + AppLogic.AdminLinkUrl("news.aspx") + "\">" + AppLogic.GetString("admin.default.News", SkinID, LocaleSetting) + "</a> - " + String.Format(AppLogic.GetString("admin.editnews.ManageNews", SkinID, LocaleSetting),CommonLogic.IIF(DataUpdated, " (Updated)", ""));
            RenderHtml();
        }

        private void RenderHtml()
        {
            StringBuilder writer = new StringBuilder();

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("select * from News  with (NOLOCK)  where NewsID=" + NewsID.ToString(), dbconn))
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
                            writer.Append("<b>" + String.Format(AppLogic.GetString("admin.editnews.EditingNews", SkinID, LocaleSetting),DB.RSFieldInt(rs, "NewsID").ToString()) + "<br/><br/></b>\n");
                        }
                        else
                        {
                            writer.Append("<div style=\"height:17;padding-top:3px;\" class=\"tablenormal\">" + AppLogic.GetString("admin.editnews.AddingNews", SkinID, LocaleSetting) + ":</div><br/></b>\n");
                        }

                        writer.Append("  <!-- calendar stylesheet -->\n");
                        writer.Append("  <link rel=\"stylesheet\" type=\"text/css\" media=\"all\" href=\"jscalendar/calendar-win2k-cold-1.css\" title=\"win2k-cold-1\" />\n");
                        writer.Append("\n");
                        writer.Append("  <!-- main calendar program -->\n");
                        writer.Append("  <script type=\"text/javascript\" src=\"jscalendar/calendar.js\"></script>\n");
                        writer.Append("\n");
                        writer.Append("  <!-- language for the calendar -->\n");
                        writer.Append("  <script type=\"text/javascript\" src=\"jscalendar/lang/" + Localization.JSCalendarLanguageFile() + "\"></script>\n");
                        writer.Append("\n");
                        writer.Append("  <!-- the following script defines the Calendar.setup helper function, which makes\n");
                        writer.Append("       adding a calendar a matter of 1 or 2 lines of code. -->\n");
                        writer.Append("  <script type=\"text/javascript\" src=\"jscalendar/calendar-setup.js\"></script>\n");

                        writer.Append("<script type=\"text/javascript\">\n");
                        writer.Append("function Form_Validator(theForm)\n");
                        writer.Append("{\n");
                        writer.Append("submitonce(theForm);\n");
                        writer.Append("return (true);\n");
                        writer.Append("}\n");
                        writer.Append("</script>\n");

                        if (AppLogic.NumLocaleSettingsInstalled() > 1)
                        {
                            writer.Append("<script type='text/javascript' src='Scripts/tabs.js'></script>");
                        }

                        writer.Append("<p>" + AppLogic.GetString("admin.editnews.NewsInfo", SkinID, LocaleSetting) + "</p>\n");
                        writer.Append("<form enctype=\"multipart/form-data\" action=\"" + AppLogic.AdminLinkUrl("editnews.aspx") + "?NewsID=" + NewsID.ToString() + "&edit=" + Editing.ToString() + "\" method=\"post\" id=\"Form1\" name=\"Form1\" onsubmit=\"return (validateForm(document.forms[0]) && Form_Validator(document.forms[0]))\" onReset=\"return confirm('" + AppLogic.GetString("admin.common.ResetAllFieldsPrompt", SkinID, LocaleSetting) + "');\">\n");
                        writer.Append("<input type=\"hidden\" name=\"IsSubmit\" value=\"true\">\n");
                        writer.Append("<table width=\"100%\" cellpadding=\"4\" cellspacing=\"0\">\n");
                        writer.Append("              <tr valign=\"middle\">\n");
                        writer.Append("                <td width=\"100%\" colspan=\"2\" align=\"left\">\n");
                        writer.Append("                </td>\n");
                        writer.Append("              </tr>\n");

                        writer.Append("              <tr valign=\"middle\">\n");
                        writer.Append("                <td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.editnews.Headline", SkinID, LocaleSetting) + ":&nbsp;&nbsp;</td>\n");
                        writer.Append("                <td align=\"left\" valign=\"top\">\n");
                        writer.Append(AppLogic.GetLocaleEntryFields(DB.RSField(rs, "Headline"),"Headline", false, true, true, AppLogic.GetString("admin.editnews.EnterHeadline", SkinID, LocaleSetting), 100, 50, 0, 0, false));
                        
                        writer.Append("                </td>\n");
                        writer.Append("              </tr>\n");

                        writer.Append("              <tr valign=\"middle\">\n");
                        writer.Append("                <td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.editnews.NewsCopy", SkinID, LocaleSetting) + ":&nbsp;&nbsp;</td>\n");
                        writer.Append("                <td align=\"left\" valign=\"top\">\n");
                        writer.Append(AppLogic.GetLocaleEntryFields(DB.RSField(rs, "NewsCopy"),"NewsCopy", true, true, false, "", 0, 0, AppLogic.AppConfigUSInt("Admin_TextareaHeight"), AppLogic.AppConfigUSInt("Admin_TextareaWidth"), true));
                        
                        writer.Append("                </td>\n");
                        writer.Append("              </tr>\n");

                        writer.Append("              <tr valign=\"middle\">\n");
                        writer.Append("                <td width=\"25%\" align=\"right\" valign=\"middle\">*Expiration Date:&nbsp;&nbsp;</td>\n");
                        writer.Append("                <td align=\"left\" valign=\"top\">\n");
                        writer.Append("                	<input maxLength=\"100\" size=\"30\" name=\"ExpiresOn\" value=\"" + CommonLogic.IIF(Editing, Localization.ToThreadCultureShortDateString(DB.RSFieldDateTime(rs, "ExpiresOn")), Localization.ToThreadCultureShortDateString(System.DateTime.Now.AddMonths(1))) + "\">&nbsp;<img src=\"" + AppLogic.LocateImageURL("skins/skin_" + SkinID.ToString() + "/images/calendar.gif") + "\" class=\"actionelement\" align=\"absmiddle\" id=\"f_trigger_s\">&nbsp; <small>(" + Localization.ShortDateFormat() + ")</small>\n");
                        writer.Append("                	<input type=\"hidden\" name=\"ExpiresOn_vldt\" value=\"[req][blankalert=Please enter the expiration date (e.g. " + Localization.ShortDateFormat() + ")]\">\n");
                        writer.Append("                	</td>\n");
                        writer.Append("              </tr>\n");

                        writer.Append("              <tr valign=\"middle\">\n");
                        writer.Append("                <td align=\"right\" valign=\"middle\">*" + AppLogic.GetString("admin.currencies.Published", SkinID, LocaleSetting) + ":&nbsp;&nbsp;</td>\n");
                        writer.Append("                <td align=\"left\" valign=\"top\">\n");
                        writer.Append(AppLogic.GetString("admin.common.Yes", SkinID, LocaleSetting) + "&nbsp;<INPUT TYPE=\"RADIO\" NAME=\"Published\" value=\"1\" " + CommonLogic.IIF(Editing, CommonLogic.IIF(DB.RSFieldBool(rs, "Published"), " checked ", ""), " checked ") + ">\n");
                        writer.Append(AppLogic.GetString("admin.common.No", SkinID, LocaleSetting) + "&nbsp;<INPUT TYPE=\"RADIO\" NAME=\"Published\" value=\"0\" " + CommonLogic.IIF(Editing, CommonLogic.IIF(DB.RSFieldBool(rs, "Published"), "", " checked "), "") + ">\n");
                        writer.Append("                </td>\n");
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


                        writer.Append("\n<script type=\"text/javascript\">\n");
                        writer.Append("    Calendar.setup({\n");
                        writer.Append("        inputField     :    \"ExpiresOn\",      // id of the input field\n");
                        writer.Append("        ifFormat       :    \"" + Localization.JSCalendarDateFormatSpec() + "\",       // format of the input field\n");
                        writer.Append("        showsTime      :    false,            // will display a time selector\n");
                        writer.Append("        button         :    \"f_trigger_s\",   // trigger for the calendar (button ID)\n");
                        writer.Append("        singleClick    :    true            // double-click mode\n");
                        writer.Append("    });\n");
                        writer.Append("</script>\n");
                    }
                }
            }
            ltContent.Text = writer.ToString();
        }
    }
}
