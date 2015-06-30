// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Text;
using System.Globalization;
using AspDotNetStorefrontCore;
using System.Data.SqlClient;

namespace AspDotNetStorefrontAdmin
{
	/// <summary>
	/// Summary description for polls.
	/// </summary>
    public partial class polls : AdminPageBase
	{
		
		protected void Page_Load(object sender, System.EventArgs e)
		{
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache"); 
			SectionTitle = "Manage Polls";
			RenderHtml();
		}

		private void RenderHtml()
		{
            StringBuilder writer = new StringBuilder();
			if(CommonLogic.QueryStringCanBeDangerousContent("DeleteID").Length != 0)
			{
				// delete the record:
				DB.ExecuteSQL("update Poll set deleted=1 where PollID=" + CommonLogic.QueryStringCanBeDangerousContent("DeleteID"));
			}

			if(CommonLogic.FormBool("IsSubmit"))
			{
				for(int i = 0; i<=Request.Form.Count-1; i++)
				{
					if(Request.Form.Keys[i].IndexOf("DisplayOrder_") != -1)
					{
						String[] keys = Request.Form.Keys[i].Split('_');
						int PollID = Localization.ParseUSInt(keys[1]);
						int DispOrd = 1;
						try
						{
							DispOrd = Localization.ParseUSInt(Request.Form[Request.Form.Keys[i]]);
						}
						catch {}
						DB.ExecuteSQL("update Poll set DisplayOrder=" + DispOrd.ToString() + " where PollID=" + PollID.ToString());
					}
				}
			}

            writer.Append("<form method=\"POST\" action=\"" + AppLogic.AdminLinkUrl("polls.aspx") + "\">\n");
			writer.Append("<input type=\"hidden\" name=\"IsSubmit\" value=\"true\">\n");
            writer.Append("<p align=\"left\"><input type=\"button\" value=\"" + AppLogic.GetString("admin.polls.AddNew", SkinID, LocaleSetting) + "\" class=\"normalButtons\" name=\"AddNew\" onClick=\"self.location='" + AppLogic.AdminLinkUrl("editpolls.aspx") + "';\"><p>");
			writer.Append("  <table border=\"0\" cellpadding=\"0\" border=\"0\" cellspacing=\"0\" width=\"100%\">\n");
			writer.Append("    <tr bgcolor=\"" + AppLogic.AppConfig("LightCellColor") + "\">\n");
            writer.Append("    <tr class=\"tablenormal\">\n");
            writer.Append("      <td align=\"left\" valign=\"middle\">" + AppLogic.GetString("admin.common.ID", SkinID, LocaleSetting) + "</td>\n");
            writer.Append("      <td align=\"left\" valign=\"middle\">" + AppLogic.GetString("admin.common.Poll", SkinID, LocaleSetting) + "</td>\n");
            writer.Append("      <td align=\"left\" valign=\"middle\">" + AppLogic.GetString("admin.common.ExpiresOn", SkinID, LocaleSetting) + "</td>\n");
            writer.Append("      <td align=\"left\" valign=\"middle\">" + AppLogic.GetString("admin.polls.NumVotes", SkinID, LocaleSetting) + "</td>\n");
            writer.Append("      <td align=\"left\" valign=\"middle\">" + AppLogic.GetString("admin.common.DisplayOrder", SkinID, LocaleSetting) + "</td>\n");
            writer.Append("      <td align=\"left\" valign=\"middle\">" + AppLogic.GetString("admin.polls.ManageAnswers", SkinID, LocaleSetting) + "</td>\n");
            writer.Append("      <td align=\"left\" valign=\"middle\">" + AppLogic.GetString("admin.polls.ReviewVotes", SkinID, LocaleSetting) + "</td>\n");
            writer.Append("      <td align=\"left\" valign=\"middle\">" + AppLogic.GetString("admin.polls.DeletePoll", SkinID, LocaleSetting) + "</td>\n");
            writer.Append("    </tr>\n");

            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS("select * from Poll   with (NOLOCK)  where deleted=0 order by DisplayOrder,Name", conn))
                {
                    while (rs.Read())
                    {
                        writer.Append("    <tr class=\"tabletdnormal\">\n");
                        writer.Append("      <td >" + DB.RSFieldInt(rs, "PollID").ToString() + "</td>\n");
                        writer.Append("<td>\n");
                        writer.Append("<a href=\"" + AppLogic.AdminLinkUrl("editpolls.aspx") + "?Pollid=" + DB.RSFieldInt(rs, "PollID").ToString() + "\">");
                        writer.Append(DB.RSFieldByLocale(rs, "Name", LocaleSetting));
                        writer.Append("</a>");
                        writer.Append("</td>\n");
                        writer.Append("<td align=\"left\" valign=\"middle\">" + Localization.ToThreadCultureShortDateString(DB.RSFieldDateTime(rs, "ExpiresOn")) + "</td>");
                        writer.Append("<td align=\"left\" valign=\"middle\">" + DB.GetSqlN("select count(*) as N from PollVotingRecord   with (NOLOCK)  where pollanswerid in (select distinct pollanswerid from pollanswer where deleted=0) and PollID=" + DB.RSFieldInt(rs, "PollID").ToString()).ToString() + "</td>");
                        writer.Append("      <td align=\"left\" valign=\"middle\"><input size=2 type=\"text\" name=\"DisplayOrder_" + DB.RSFieldInt(rs, "PollID").ToString() + "\" value=\"" + DB.RSFieldInt(rs, "DisplayOrder").ToString() + "\"></td>\n");
                        writer.Append("      <td align=\"left\" valign=\"middle\"><input type=\"button\" class=\"normalButtons\" value=\"" + AppLogic.GetString("admin.polls.ManageAnswers", SkinID, LocaleSetting) + "\" name=\"ManageAnswers_" + DB.RSFieldInt(rs, "PollID").ToString() + "\" onClick=\"self.location='" + AppLogic.AdminLinkUrl("pollanswers.aspx") + "?Pollid=" + DB.RSFieldInt(rs, "PollID").ToString() + "'\"></td>\n");
                        writer.Append("      <td align=\"left\" valign=\"middle\"><input type=\"button\" class=\"normalButtons\" value=\"" + AppLogic.GetString("admin.polls.ReviewVotes", SkinID, LocaleSetting) + "\" name=\"ReviewVotes_" + DB.RSFieldInt(rs, "PollID").ToString() + "\" onClick=\"self.location='" + AppLogic.AdminLinkUrl("managepoll.aspx") + "?Pollid=" + DB.RSFieldInt(rs, "PollID").ToString() + "'\"></td>\n");
                        writer.Append("      <td align=\"left\" valign=\"middle\"><input type=\"button\" class=\"normalButtons\" value=\"" + AppLogic.GetString("admin.common.Delete", SkinID, LocaleSetting) + "\" name=\"Delete_" + DB.RSFieldInt(rs, "PollID").ToString() + "\" onClick=\"DeletePoll(" + DB.RSFieldInt(rs, "PollID").ToString() + ")\"></td>\n");
                        writer.Append("    </tr>\n");
                    }
                }
            }

			writer.Append("    <tr>\n");
			writer.Append("      <td colspan=\"4\" align=\"left\"></td>\n");
            writer.Append("      <td align=\"left\" valign=\"middle\" height=\"25px\"><input type=\"submit\" class=\"normalButtons\" value=\"" + AppLogic.GetString("admin.common.Update", SkinID, LocaleSetting) + "\" name=\"Submit\"></td>\n");
			writer.Append("      <td colspan=\"3\"></td>\n");
			writer.Append("    </tr>\n");
			writer.Append("  </table>\n");
            writer.Append("<p align=\"left\"><input type=\"button\" value=\"" + AppLogic.GetString("admin.polls.AddNew", SkinID, LocaleSetting) + "\" class=\"normalButtons\" name=\"AddNew\" onClick=\"self.location='" + AppLogic.AdminLinkUrl("editpolls.aspx") + "';\"><p>");
			writer.Append("</form>\n");

			writer.Append("</center></b>\n");

			writer.Append("<script type=\"text/javascript\">\n");
			writer.Append("function DeletePoll(id)\n");
			writer.Append("{\n");
            writer.Append("if(confirm('" + AppLogic.GetString("admin.polls.ConfirmDelete", SkinID, LocaleSetting) + " ' + id))\n");
			writer.Append("{\n");
            writer.Append("self.location = '" + AppLogic.AdminLinkUrl("polls.aspx") + "?deleteid=' + id;\n");
			writer.Append("}\n");
			writer.Append("}\n");
			writer.Append("</SCRIPT>\n");
            ltContent.Text = writer.ToString();
		}
	}
}
