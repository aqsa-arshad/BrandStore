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
	/// Summary description for pollanswers.
	/// </summary>
    public partial class pollanswers : AdminPageBase
	{
		
		protected void Page_Load(object sender, System.EventArgs e)
		{
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");
            SectionTitle = "<a href=\"" + AppLogic.AdminLinkUrl("polls.aspx") + "\">Manage Polls</a> - Add/Edit Answers";
            RenderHtml();
		}

		private void RenderHtml()
		{
            StringBuilder writer = new StringBuilder();
			int PollID = CommonLogic.QueryStringUSInt("PollID");
			if(PollID == 0)
			{
				Response.Redirect(AppLogic.AdminLinkUrl("polls.aspx"));
			}

			String PollName = AppLogic.GetPollName(PollID,LocaleSetting);

			if(CommonLogic.QueryStringCanBeDangerousContent("DeleteID").Length != 0)
			{
				// delete the mfg:
				DB.ExecuteSQL("update PollAnswer set deleted=1 where PollAnswerID=" + CommonLogic.QueryStringCanBeDangerousContent("DeleteID"));
			}

			if(CommonLogic.FormBool("IsSubmit"))
			{
				for(int i = 0; i<=Request.Form.Count-1; i++)
				{
					if(Request.Form.Keys[i].IndexOf("DisplayOrder_") != -1)
					{
						String[] keys = Request.Form.Keys[i].Split('_');
						int PollAnswerID = Localization.ParseUSInt(keys[1]);
						int DispOrd = 1;
						try
						{
							DispOrd = Localization.ParseUSInt(Request.Form[Request.Form.Keys[i]]);
						}
						catch {}
						DB.ExecuteSQL("update PollAnswer set DisplayOrder=" + DispOrd.ToString() + " where PollAnswerID=" + PollAnswerID.ToString());
					}
				}
			}

            writer.Append("<p align=\"left\"" + String.Format(AppLogic.GetString("admin.pollanswers.EditinAnswers", SkinID, LocaleSetting),AppLogic.AdminLinkUrl("editpolls.aspx"),PollID.ToString(),PollName,PollID.ToString()) + "</p>\n");

            writer.Append("<form id=\"Form1\" name=\"Form1\" method=\"POST\" action=\"" + AppLogic.AdminLinkUrl("pollanswers.aspx") + "?Pollid=" + PollID.ToString() + "\">\n");
			writer.Append("<input type=\"hidden\" name=\"IsSubmit\" value=\"true\">\n");
			writer.Append("  <table border=\"0\" cellpadding=\"0\" border=\"0\" cellspacing=\"0\" width=\"100%\">\n");
            writer.Append("    <tr class=\"table-header\">\n");
			writer.Append("      <td><b>ID</b></td>\n");
			writer.Append("      <td><b>Answer</b></td>\n");
            writer.Append("      <td align=\"center\"><b>" + AppLogic.GetString("admin.common.DisplayOrder", SkinID, LocaleSetting) + "</b></td>\n");
            writer.Append("      <td align=\"center\"><b>" + AppLogic.GetString("admin.common.Edit", SkinID, LocaleSetting) + "</b></td>\n");
            writer.Append("      <td align=\"center\"><b>" + AppLogic.GetString("admin.common.Delete", SkinID, LocaleSetting) + "</b></td>\n");
			writer.Append("    </tr>\n");

            int counter = 0;
            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS("select * from PollAnswer   with (NOLOCK)  where deleted=0 and PollID=" + PollID.ToString() + " order by DisplayOrder,Name", conn))
                {
                    while (rs.Read())
                    {
                        if (counter % 2 == 0)
                        {
                            writer.Append("    <tr class=\"table-row2\">\n");
                        }
                        else
                        {
                            writer.Append("    <tr class=\"table-alternatingrow2\">\n");
                        }
                        writer.Append("      <td >" + DB.RSFieldInt(rs, "PollAnswerID").ToString() + "</td>\n");
                        writer.Append("      <td >");
                        writer.Append("<a href=\"" + AppLogic.AdminLinkUrl("editpollanswer.aspx") + "?Pollid=" + PollID.ToString() + "&PollAnswerid=" + DB.RSFieldInt(rs, "PollAnswerID").ToString() + "\">" + CommonLogic.IIF(DB.RSFieldByLocale(rs, "Name", LocaleSetting).Length == 0, "(Unnamed Variant)", DB.RSFieldByLocale(rs, "Name", LocaleSetting)) + "</a>");
                        writer.Append("</td>\n");
                        writer.Append("      <td align=\"center\"><input size=2 type=\"text\" name=\"DisplayOrder_" + DB.RSFieldInt(rs, "PollAnswerID").ToString() + "\" value=\"" + DB.RSFieldInt(rs, "DisplayOrder").ToString() + "\"></td>\n");
                        writer.Append("      <td align=\"center\"><input type=\"button\" class=\"normalButtons\" value=\"" + AppLogic.GetString("admin.common.Edit", SkinID, LocaleSetting) + "\" name=\"Edit_" + DB.RSFieldInt(rs, "PollAnswerID").ToString() + "\" onClick=\"self.location='" + AppLogic.AdminLinkUrl("editpollanswer.aspx") + "?Pollid=" + PollID.ToString() + "&PollAnswerid=" + DB.RSFieldInt(rs, "PollAnswerID").ToString() + "'\"></td>\n");
                        writer.Append("      <td align=\"center\"><input type=\"button\" class=\"normalButtons\" value=\"" + AppLogic.GetString("admin.common.Delete", SkinID, LocaleSetting) + "\" name=\"Delete_" + DB.RSFieldInt(rs, "PollAnswerID").ToString() + "\" onClick=\"DeleteAnswer(" + DB.RSFieldInt(rs, "PollAnswerID").ToString() + ")\"></td>\n");
                        writer.Append("    </tr>\n");
                        counter++;
                    }
                }
            }

			writer.Append("    <tr>\n");
			writer.Append("      <td colspan=\"2\" align=\"left\"></td>\n");
            writer.Append("      <td align=\"center\" ><input type=\"submit\" value=\"" + AppLogic.GetString("admin.common.Update", SkinID, LocaleSetting) + "\" name=\"Submit\" class=\"normalButtons\"></td>\n");
			writer.Append("      <td colspan=\"2\"></td>\n");
			writer.Append("    </tr>\n");
			writer.Append("    <tr>\n");
			writer.Append("      <td colspan=\"5\" height=5></td>\n");
			writer.Append("    </tr>\n");
			writer.Append("  </table>\n");
            writer.Append(" <input type=\"button\" class=\"normalButtons\" value=\"" + AppLogic.GetString("admin.pollanswers.AddNew", SkinID, LocaleSetting) + "\" name=\"AddNew\" onClick=\"self.location='" + AppLogic.AdminLinkUrl("editpollanswer.aspx") + "?Pollid=" + PollID.ToString() + "';\">\n");
			writer.Append("</form>\n");

			writer.Append("<script type=\"text/javascript\">\n");
			writer.Append("function DeleteAnswer(id)\n");
			writer.Append("{\n");
            writer.Append("if(confirm('" + AppLogic.GetString("admin.pollanswers.ConfirmDelete", SkinID, LocaleSetting) + "' + id))\n");
			writer.Append("{\n");
            writer.Append("self.location = '" + AppLogic.AdminLinkUrl("pollanswers.aspx") + "?Pollid=" + PollID.ToString() + "&deleteid=' + id;\n");
			writer.Append("}\n");
			writer.Append("}\n");
			writer.Append("</SCRIPT>\n");
            ltContent.Text = writer.ToString();
		}

	}
}
