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
using System.Data.SqlClient;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	/// <summary>
	/// Summary description for managepoll.
	/// </summary>
    public partial class managepoll : AdminPageBase
	{
		
		protected void Page_Load(object sender, System.EventArgs e)
		{
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");
            SectionTitle = "<a href=\"" + AppLogic.AdminLinkUrl("polls.aspx") + "\">Manage Polls</a> - Review Customer Votes";
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
				DB.ExecuteSQL("delete from PollVotingRecord where PollVotingRecordID=" + CommonLogic.QueryStringCanBeDangerousContent("DeleteID"));
			}

			if(CommonLogic.QueryStringCanBeDangerousContent("BanID").Length != 0)
			{
				DB.ExecuteSQL("delete from PollVotingRecord where CustomerID=" + CommonLogic.QueryStringCanBeDangerousContent("BanID"));
			}

            writer.Append(String.Format(AppLogic.GetString("admin.managepoll.ReviewVotes", SkinID, LocaleSetting),AppLogic.AdminLinkUrl("editPolls.aspx"),PollID.ToString(),PollName,PollID.ToString()));
            			
			writer.Append("<input type=\"hidden\" name=\"IsSubmit\" value=\"true\">\n");
			writer.Append("  <table border=\"0\" cellpadding=\"0\" border=\"0\" cellspacing=\"0\" width=\"100%\">\n");
            writer.Append("    <tr class=\"table-header\">\n");
            writer.Append("      <td><b>" + AppLogic.GetString("admin.common.RecordID", SkinID, LocaleSetting) + "</b></td>\n");
            writer.Append("      <td><b>" + AppLogic.GetString("admin.managepoll.AnsweredOn", SkinID, LocaleSetting) + "</b></td>\n");
            writer.Append("      <td><b>" + AppLogic.GetString("admin.common.CustomerID", SkinID, LocaleSetting) + "</b></td>\n");
            writer.Append("      <td><b>" + AppLogic.GetString("admin.common.CustomerEmail", SkinID, LocaleSetting) + "</b></td>\n");
            writer.Append("      <td><b>" + AppLogic.GetString("admin.common.CustomerName", SkinID, LocaleSetting) + "</b></td>\n");
            writer.Append("      <td><b>" + AppLogic.GetString("admin.managepoll.AnswerPicked", SkinID, LocaleSetting) + "</b></td>\n");
            writer.Append("      <td align=\"center\"><b>" + AppLogic.GetString("admin.common.Delete", SkinID, LocaleSetting) + "</b></td>\n");
            writer.Append("      <td align=\"center\"><b>" + AppLogic.GetString("admin.managepoll.BanCustomer", SkinID, LocaleSetting) + "</b></td>\n");
			writer.Append("    </tr>\n");

            int i = 0; 
            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("SELECT  Customer.EMail, Customer.FirstName AS CFN, Customer.LastName AS CLN, Customer.FirstName + ' ' + Customer.LastName AS Name, PollVotingRecord.PollVotingRecordID,PollVotingRecord.CreatedOn, PollVotingRecord.PollID, PollAnswer.Name AS AnswerName, PollVotingRecord.PollAnswerID, PollVotingRecord.CustomerID FROM (PollVotingRecord LEFT OUTER JOIN Customer ON PollVotingRecord.CustomerID = Customer.CustomerID) LEFT OUTER JOIN PollAnswer ON PollVotingRecord.PollAnswerID = PollAnswer.PollAnswerID where PollVotingRecord.PollID=" + PollID.ToString() + " order by PollVotingRecord.CreatedOn desc", dbconn))
                {
                    while (rs.Read())
                    {
                        if (i % 2 == 0)
                        {
                            writer.Append("    <tr class=\"table-row2\">\n");
                        }
                        else
                        {
                            writer.Append("    <tr class=\"table-alternatingrow2\">\n");
                        }
                        writer.Append("      <td>" + DB.RSFieldInt(rs, "PollVotingRecordID").ToString() + "</td>\n");
                        writer.Append("      <td>" + DB.RSFieldDateTime(rs, "CreatedOn").ToString() + "</td>\n");
                        writer.Append("      <td>" + DB.RSFieldInt(rs, "CustomerID").ToString() + "</td>\n");
                        writer.Append("      <td>" + DB.RSField(rs, "EMail") + "</td>\n");
                        writer.Append("      <td>" + (DB.RSField(rs, "CFN") + " " + DB.RSField(rs, "CLN")).Trim() + "</td>\n");
                        writer.Append("      <td>" + DB.RSField(rs, "AnswerName") + "</td>\n");
                        writer.Append("      <td align=\"center\"><input type=\"button\" class=\"normalButtons\" value=\"" + AppLogic.GetString("admin.common.Delete", SkinID, LocaleSetting) + "\" name=\"Delete_" + DB.RSFieldInt(rs, "PollVotingRecordID").ToString() + "\" onClick=\"DeleteVote(" + DB.RSFieldInt(rs, "PollVotingRecordID").ToString() + ")\"></td>\n");
                        writer.Append("      <td align=\"center\"><input type=\"button\" class=\"normalButtons\" value=\"" + AppLogic.GetString("admin.common.Ban", SkinID, LocaleSetting) + "\" name=\"BanCustomer_" + DB.RSFieldInt(rs, "CustomerID").ToString() + "\" onClick=\"BanCustomer(" + DB.RSFieldInt(rs, "CustomerID").ToString() + ")\"></td>\n");
                        writer.Append("    </tr>\n");
                        i++;
                    }
                }
            }

			writer.Append("  </table>\n");
			writer.Append("<script type=\"text/javascript\">\n");
			writer.Append("function DeleteVote(id)\n");
			writer.Append("{\n");
            writer.Append("if(confirm('" + AppLogic.GetString("admin.managepoll.ConfirmVoteDelete", SkinID, LocaleSetting) + "'))\n");
			writer.Append("{\n");
            writer.Append("self.location = '" + AppLogic.AdminLinkUrl("managepoll.aspx") + "?Pollid=" + PollID.ToString() + "&deleteid=' + id;\n");
			writer.Append("}\n");
			writer.Append("}\n");
			writer.Append("function BanCustomer(id)\n");
			writer.Append("{\n");
            writer.Append("if(confirm('" + AppLogic.GetString("admin.managepoll.RemoveAllVotes", SkinID, LocaleSetting) + "'))\n");
			writer.Append("{\n");
            writer.Append("self.location = '" + AppLogic.AdminLinkUrl("managepoll.aspx") + "?Pollid=" + PollID.ToString() + "&banid=' + id;\n");
			writer.Append("}\n");
			writer.Append("}\n");
			writer.Append("</SCRIPT>\n");
            ltContent.Text = writer.ToString();
		}

	}
}
