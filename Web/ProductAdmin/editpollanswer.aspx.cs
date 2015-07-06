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
    /// Summary description for editpollanswer
    /// </summary>
    public partial class editpollanswer : AdminPageBase
    {

        int PollID;
        int PollAnswerID;

        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");


            PollID = CommonLogic.QueryStringUSInt("PollID");
            PollAnswerID = 0;


            if (CommonLogic.QueryStringCanBeDangerousContent("PollAnswerID").Length != 0 && CommonLogic.QueryStringCanBeDangerousContent("PollAnswerID") != "0")
            {
                Editing = true;
                PollAnswerID = Localization.ParseUSInt(CommonLogic.QueryStringCanBeDangerousContent("PollAnswerID"));
            }
            else
            {
                Editing = false;
            }
            if (PollID == 0)
            {
                Response.Redirect(AppLogic.AdminLinkUrl("polls.aspx"));
            }


            if (CommonLogic.FormBool("IsSubmit"))
            {

                StringBuilder sql = new StringBuilder(2500);
                if (!Editing)
                {
                    // ok to add:
                    String NewGUID = DB.GetNewGUID();
                    sql.Append("insert into PollAnswer(PollAnswerGUID,PollID,Name) values(");
                    sql.Append(DB.SQuote(NewGUID) + ",");
                    sql.Append(PollID.ToString() + ",");
                    sql.Append(DB.SQuote(AppLogic.FormLocaleXml("Name")));
                    sql.Append(")");
                    DB.ExecuteSQL(sql.ToString());

                    using (SqlConnection dbconn = DB.dbConn())
                    {
                        dbconn.Open();
                        using (IDataReader rs = DB.GetRS("select PollAnswerID from PollAnswer   with (NOLOCK)  where deleted=0 and PollAnswerGUID=" + DB.SQuote(NewGUID), dbconn))
                        {
                            rs.Read();
                            PollAnswerID = DB.RSFieldInt(rs, "PollAnswerID");
                            Editing = true;
                        }
                    }
                    DataUpdated = true;
                }
                else
                {
                    // ok to update:
                    sql.Append("update PollAnswer set ");
                    sql.Append("PollID=" + PollID.ToString() + ",");
                    sql.Append("Name=" + DB.SQuote(AppLogic.FormLocaleXml("Name")));
                    sql.Append(" where PollAnswerID=" + PollAnswerID.ToString());
                    DB.ExecuteSQL(sql.ToString());
                    DataUpdated = true;
                    Editing = true;
                }
            }
            SectionTitle = "<a href=\"pollanswers.aspx" + "?Pollid=" + PollID.ToString() + "\">" + AppLogic.GetString("admin.editpollanswer.PollAnswers", SkinID, LocaleSetting) + "</a> - " + AppLogic.GetString("admin.editpollanswer.ManagePollAnswers", SkinID, LocaleSetting) + "";
            RenderHtml();
        }

        private void RenderHtml()
        {
            StringBuilder writer = new StringBuilder();
            writer.Append("<script type='text/javascript' src='Scripts/tabs.js'></script>");
            
            if (DataUpdated)
            {
                writer.Append("<p><b><font color=blue>" + AppLogic.GetString("admin.editCreditCard.Updated", SkinID, LocaleSetting) + "</font></b></p>\n");
            }

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("select * from PollAnswer   with (NOLOCK)  where PollAnswerID=" + PollAnswerID.ToString(), dbconn))
                {
                    if (rs.Read())
                    {
                        Editing = true;
                    }

                    if (ErrorMsg.Length != 0)
                    {
                        writer.Append("<p align=\"left\"><b><font color=red>" + ErrorMsg + "</font></b></p>\n");
                    }


                    if (ErrorMsg.Length == 0)
                    {

                        writer.Append("<p align=\"left\"><b>" + AppLogic.GetString("admin.editpollanswer.WithinPoll", SkinID, LocaleSetting) + "<a href=\"" + AppLogic.AdminLinkUrl("editPolls.aspx") + "?Pollid=" + PollID.ToString() + "\">" + AppLogic.GetPollName(PollID, LocaleSetting) + "</a> " + String.Format(AppLogic.GetString("admin.editpollanswer.PollID", SkinID, LocaleSetting),PollID.ToString()) + "</b</p>\n");
                        if (Editing)
                        {
                            writer.Append("<p align=\"left\"><b>" + String.Format(AppLogic.GetString("admin.editpollanswer.EditingPollAnswer", SkinID, LocaleSetting),DB.RSFieldByLocale(rs, "Name", LocaleSetting),DB.RSFieldInt(rs, "PollAnswerID").ToString()) + "</b></p>\n");
                        }
                        else
                        {
                            writer.Append("<p align=\"left\"><b>" + AppLogic.GetString("admin.editpollanswer.AddNewPollAnswer", SkinID, LocaleSetting) + ":</p></b>\n");
                        }

                        writer.Append("<script type=\"text/javascript\">\n");
                        writer.Append("function Form_Validator(theForm)\n");
                        writer.Append("{\n");
                        writer.Append("submitonce(theForm);\n");
                        writer.Append("return (true);\n");
                        writer.Append("}\n");
                        writer.Append("</script>\n");

                        writer.Append("<p align=\"left\">" + AppLogic.GetString("admin.editpollanswer.PollAnswerInfo", SkinID, LocaleSetting) + "</p>\n");
                        writer.Append("<form action=\"" + AppLogic.AdminLinkUrl("editpollanswer.aspx") + "?Pollid=" + PollID.ToString() + "&PollAnswerID=" + PollAnswerID.ToString() + "&edit=" + Editing.ToString() + "\" method=\"post\" id=\"Form1\" name=\"Form1\" onsubmit=\"return (validateForm(this) && Form_Validator(this))\" onReset=\"return confirm('" + AppLogic.GetString("admin.common.ResetAllFieldsPrompt", SkinID, LocaleSetting) + "');\">\n");
                        writer.Append("<input type=\"hidden\" name=\"IsSubmit\" value=\"true\">\n");
                        writer.Append("<table width=\"100%\" cellpadding=\"4\" cellspacing=\"0\">\n");
                        writer.Append("              <tr valign=\"middle\">\n");
                        writer.Append("                <td width=\"100%\" colspan=\"2\" align=\"left\">\n");
                        writer.Append("                </td>\n");
                        writer.Append("              </tr>\n");

                        writer.Append("<tr>\n");
                        writer.Append("<td></td><td align=\"left\" valign=\"top\"><br/>\n");
                        if (Editing)
                        {
                            writer.Append("<input type=\"submit\" value=\"" + AppLogic.GetString("admin.common.Update", SkinID, LocaleSetting) + "\" name=\"submit\" class=\"normalButtons\">\n");
                            writer.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<input type=\"reset\"  class=\"normalButtons\" value=\"" + AppLogic.GetString("admin.common.Reset", SkinID, LocaleSetting) + "\" name=\"reset\">\n");
                        }
                        else
                        {
                            writer.Append("<input type=\"submit\" value=\"" + AppLogic.GetString("admin.common.AddNew", SkinID, LocaleSetting) + "\" name=\"submit\" class=\"normalButtons\">\n");
                        }
                        writer.Append("        </td>\n");
                        writer.Append("      </tr>\n");

                        writer.Append("              <tr valign=\"middle\">\n");
                        writer.Append("                <td width=\"25%\" align=\"right\" valign=\"middle\">" + AppLogic.GetString("admin.editpollanswer.AnswerText", SkinID, LocaleSetting) + ":&nbsp;&nbsp;</td>\n");
                        writer.Append("                <td align=\"left\" valign=\"top\">\n");
                        writer.Append(AppLogic.GetLocaleEntryFields(DB.RSField(rs, "Name"), "Name", false, true, true, AppLogic.GetString("admin.editpollanswer.EnterAnswerText", SkinID, LocaleSetting), 100, 30, 0, 0, false));

                        writer.Append("                	</td>\n");
                        writer.Append("              </tr>\n");


                        writer.Append("<tr>\n");
                        writer.Append("<td></td><td align=\"left\" valign=\"top\"><br/>\n");
                        if (Editing)
                        {
                            writer.Append("<input type=\"submit\" value=\"" + AppLogic.GetString("admin.common.Update", SkinID, LocaleSetting) + "\" name=\"submit\" class=\"normalButtons\">\n");
                            writer.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<input type=\"reset\"  class=\"normalButtons\" value=\"" + AppLogic.GetString("admin.common.Reset", SkinID, LocaleSetting) + "\" name=\"reset\">\n");
                        }
                        else
                        {
                            writer.Append("<input type=\"submit\" value=\"" + AppLogic.GetString("admin.common.AddNew", SkinID, LocaleSetting) + "\" name=\"submit\" class=\"normalButtons\">\n");
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
