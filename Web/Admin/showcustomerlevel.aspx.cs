// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	/// <summary>
	/// Summary description for showcustomerlevel.
	/// </summary>
    public partial class showcustomerlevel : AdminPageBase
	{
		
		private int CustomerLevelID;
		private String CustomerLevelName;

		protected void Page_Load(object sender, System.EventArgs e)
		{
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            CustomerLevelID = CommonLogic.QueryStringUSInt("CustomerLevelID");
			CustomerLevelName = Customer.GetCustomerLevelName(CustomerLevelID,LocaleSetting);
			if(CommonLogic.QueryStringCanBeDangerousContent("DeleteID").Length != 0)
			{
				// remove this level from this customer:
				DB.ExecuteSQL("update Customer set CustomerLevelID=0 where CustomerID=" + CommonLogic.QueryStringCanBeDangerousContent("DeleteID"));
			}

			if(CommonLogic.FormBool("IsSubmit"))
			{
				String EMail = CommonLogic.FormCanBeDangerousContent("EMail");
				if(EMail.Length != 0)
				{
					int CustomerID = Customer.GetIDFromEMail(EMail);
					if(CustomerID == 0)
					{
						if(CommonLogic.IsInteger(CommonLogic.FormCanBeDangerousContent("EMail")))
						{
							CustomerID = Localization.ParseUSInt(CommonLogic.FormCanBeDangerousContent("EMail")); // in case they just entered a customer id into the field.
						}
					}
					if(CustomerID != 0)
					{
						// clear the carts for this customer. This is to ensure their produce pricing is correct
						// their current cart can have customer level pricing, not retail pricing, and this prevents that:
						DB.ExecuteSQL("delete from shoppingcart where customerid=" + CustomerID.ToString());
						DB.ExecuteSQL("delete from kitcart where customerid=" + CustomerID.ToString());

						DB.ExecuteSQL("Update customer set CustomerLevelID=" + CustomerLevelID.ToString() + " where CustomerID=" + CustomerID.ToString());
					}
					else
					{
						ErrorMsg = "That customer e-mail was not found in the database";
					}
				}
			}
            SectionTitle = "<a href=\"" + AppLogic.AdminLinkUrl("CustomerLevels.aspx") + "\">CustomerLevels</a> - Show Customer Level: " + CustomerLevelName;
            RenderHtml();
		}

		private void RenderHtml()
		{
            StringBuilder writer = new StringBuilder();
            writer.Append("<p><b>There are " + DB.GetSqlN("select count(CustomerID) as N from Customer  with (NOLOCK)  where EMail <> '' and Deleted=0 and CustomerLevelID=" + CustomerLevelID.ToString()).ToString() + " registered customers in this customer level</b></p>");

            String SearchFor = CommonLogic.FormCanBeDangerousContent("SearchFor");
            writer.Append("<form method=\"GET\" action=\"" + AppLogic.AdminLinkUrl("showcustomerlevel.aspx") + "\">");
            writer.Append("<input type=\"hidden\" name=\"CustomerLevelID\" value=\"" + CustomerLevelID.ToString() + "\">");
            String BeginsWith = CommonLogic.IIF(CommonLogic.QueryStringCanBeDangerousContent("BeginsWith").Length == 0, "A", CommonLogic.QueryStringCanBeDangerousContent("BeginsWith"));
            String alpha = "%#ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            for (int i = 1; i <= alpha.Length; i++)
            {
                if (BeginsWith[0] == alpha[i - 1])
                {
                    writer.Append(alpha[i - 1] + "&nbsp;");
                }
                else
                {
                    writer.Append("<a href=\"" + AppLogic.AdminLinkUrl("showcustomerlevel.aspx") + "?CustomerLevelID=" + CustomerLevelID.ToString() + "&BeginsWith=" + Server.UrlEncode("" + alpha[i - 1]) + "\">" + alpha[i - 1] + "</a>&nbsp;");
                }
            }
            writer.Append("&nbsp;&nbsp;Search For: <input type=\"text\" onkeypress=\"javascript:return WebForm_FireDefaultButton(event, 'btnsearch')\" name=\"SearchFor\" value=\"" + SearchFor + "\"><input type=\"submit\" id=\"btnsearch\" name=\"search\" value=\"submit\" class=\"normalButtons\">");
            writer.Append("</form>");


            String SQL = String.Empty;
            string SuperuserFilter = (ThisCustomer.IsAdminSuperUser) ? String.Empty : String.Format(" Customer.CustomerID not in ({0}) and ", AppLogic.GetSuperAdminCustomerIDs().ToString());

            if (SearchFor.Length != 0)
            {
                int CID = 0;
                if (CommonLogic.IsInteger(SearchFor))
                {
                    CID = Localization.ParseUSInt(SearchFor);
                }
                SQL = "select * from Customer  with (NOLOCK)  where " + SuperuserFilter + " Customer.EMail <> '' and Customer.deleted=0 and (Customer.LastName like " + DB.SQuote("%" + SearchFor + "%") + CommonLogic.IIF(CID != 0, " or Customer.CustomerID=" + CID.ToString(), "") + " or Customer.Firstname like " + DB.SQuote("%" + SearchFor + "%") + " or Customer.EMail like " + DB.SQuote("%" + SearchFor + "%") + ")";
            }
            else
            {
                SQL = "select * from Customer  with (NOLOCK)  where " + SuperuserFilter + " Customer.EMail <> '' and Customer.deleted=0 and Customer.LastName like " + DB.SQuote(BeginsWith + "%");
            }
            SQL += " and CustomerLevelID=" + CustomerLevelID.ToString();
            String OrderBySQL = " order by LastName, FirstName";
            SQL = SQL + OrderBySQL;

            // ------------------------------------------------------------
            // Setup Paging Vars:
            // ------------------------------------------------------------
            int PageSize = 50;
            int PageNum = CommonLogic.QueryStringUSInt("PageNum");
            if (PageNum == 0)
            {
                PageNum = 1;
            }

            String FinalSQL = SQL;
            int NumPages = 1;
            bool ShowAll = (CommonLogic.QueryStringCanBeDangerousContent("show").Equals("ALL", StringComparison.InvariantCultureIgnoreCase));
            if (!ShowAll)
            {
                FinalSQL = String.Format("aspdnsf_PageQuery {0},'',{1},{2},{3}", DB.SQuote(SQL), PageNum.ToString(), PageSize.ToString(), "0"); //paging stats last!
            }            

            using (DataTable dt = new DataTable())
            {
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS(FinalSQL, con))
                    {
                        dt.Load(rs);

                        if (!ShowAll)
                        {
                            if (rs.NextResult() && rs.Read())
                            {
                                NumPages = DB.RSFieldInt(rs, "Pages");
                            }
                        }
                    }
                }

                // ---------------------------------------------------
                // Append paging info:
                // ---------------------------------------------------
                if (NumPages > 1 || ShowAll)
                {
                    writer.Append("<p class=\"PageNumber\" align=\"left\">");
                    if (CommonLogic.QueryStringCanBeDangerousContent("show").Equals("ALL", StringComparison.InvariantCultureIgnoreCase))
                    {
                        writer.Append("Click <a class=\"PageNumber\" href=\"" + AppLogic.AdminLinkUrl("showcustomerlevel.aspx") + "?CustomerLevelID=" + CustomerLevelID.ToString() + "&BeginsWith=" + BeginsWith + "&SearchFor=" + Server.UrlEncode(SearchFor) + "&pagenum=1\">here</a> to turn paging back on.");
                    }
                    else
                    {
                        writer.Append("Page: ");
                        for (int u = 1; u <= NumPages; u++)
                        {
                            if (u == PageNum)
                            {
                                writer.Append(u.ToString() + " ");
                            }
                            else
                            {
                                writer.Append("<a class=\"PageNumber\" href=\"" + AppLogic.AdminLinkUrl("showcustomerlevel.aspx") + "?CustomerLevelID=" + CustomerLevelID.ToString() + "&BeginsWith=" + BeginsWith + "&SearchFor=" + Server.UrlEncode(SearchFor) + "&pagenum=" + u.ToString() + "\">" + u.ToString() + "</a> ");
                            }
                        }
                        writer.Append(" <a class=\"PageNumber\" href=\"" + AppLogic.AdminLinkUrl("showcustomerlevel.aspx") + "?CustomerLevelID=" + CustomerLevelID.ToString() + "&BeginsWith=" + BeginsWith + "&SearchFor=" + Server.UrlEncode(SearchFor) + "&show=all\">all</a>");
                    }
                    writer.Append("</p>\n");
                }

                writer.Append("<script type=\"text/javascript\">\n");
                writer.Append("function Form_Validator(theForm)\n");
                writer.Append("{\n");
                writer.Append("submitonce(theForm);\n");
                writer.Append("return (true);\n");
                writer.Append("}\n");
                writer.Append("</script>\n");

                writer.Append("<form method=\"POST\" action=\"" + AppLogic.AdminLinkUrl("showcustomerlevel.aspx") + "?customerlevelid=" + CustomerLevelID.ToString() + "\"  id=\"Form1\" name=\"Form1\" onsubmit=\"return (validateForm(this) && Form_Validator(this))\" onReset=\"return confirm('Do you want to reset all fields to their starting values?');\">\n");
                writer.Append("<input type=\"hidden\" name=\"IsSubmit\" value=\"true\">\n");
                writer.Append("<table border=\"0\" cellpadding=\"0\" border=\"0\" cellspacing=\"0\" width=\"100%\">\n");
                writer.Append("<tr class=\"table-header\">\n");
                writer.Append("<td width=\"10%\"><b>Customer ID</b></td>\n");
                writer.Append("<td ><b>Name</b></td>\n");
                writer.Append("<td ><b>EMail</b></td>\n");
                writer.Append("<td align=\"center\"><b>Clear Level for this Customer</b></td>\n");
                writer.Append("</tr>\n");

                int rowcount = dt.Rows.Count;

                for (int i = 0; i < rowcount; i++)
                {
                    DataRow row = dt.Rows[i];

                    if (i % 2 == 0)
                    {
                        writer.Append("<tr class=\"table-row2\">\n");
                    }
                    else
                    {
                        writer.Append("<tr class=\"table-alternatingrow2\">\n");
                    }
                    writer.Append("<td width=\"10%\">" + DB.RowFieldInt(row, "CustomerID").ToString() + "</td>\n");
                    writer.Append("<td >" + (DB.RowField(row, "FirstName") + " " + DB.RowField(row, "LastName")).Trim() + "</td>\n");
                    writer.Append("<td >" + DB.RowField(row, "EMail") + "</td>\n");
                    writer.Append("<td width=\"10%\" align=\"center\"><input type=\"button\" value=\"Clear Level\" name=\"Delete_" + DB.RowFieldInt(row, "CustomerID").ToString() + "\" onClick=\"DeleteCustomerLevel(" + DB.RowFieldInt(row, "CustomerID").ToString() + ")\" class=\"normalButtons\"></td>\n");
                    writer.Append("</tr>\n");
                }

                    writer.Append(" </table>\n");
                    writer.Append("<p align=\"left\">Enter CustomerID or EMail to add to this level: ");
                    writer.Append("<input type=\"text\" name=\"EMail\" size=\"37\" maxlength=\"100\" onkeypress=\"javascript:return WebForm_FireDefaultButton(event, 'btnadd')\">");
                    writer.Append("<input type=\"hidden\" name=\"EMail_vldt\" value=\"[req][blankalert=Please enter a valid customer e-mail address.  You must know this in advance, and type it in here][invalidalert=Please enter a valid customer e-mail address]\">");
                    writer.Append("<input type=\"submit\" id=\"btnadd\" value=\"Add Customer To This Level\" name=\"Submit\" class=\"normalButtons\">");
                    writer.Append("</p>\n");
                    writer.Append("</form>\n");

                    writer.Append("</center></b>\n");

                    writer.Append("<script type=\"text/javascript\">\n");
                    writer.Append("function DeleteCustomerLevel(id)\n");
                    writer.Append("{\n");
                    writer.Append("if(confirm('Are you sure you clear the level for customer id: ' + id + '. NOTE: this will NOT delete their customer record'))\n");
                    writer.Append("{\n");
                    writer.Append("self.location = '" + AppLogic.AdminLinkUrl("showcustomerlevel.aspx") + "?CustomerLevelID=" + CustomerLevelID.ToString() + "&beginswith=" + BeginsWith + "&searchfor=" + Server.UrlEncode(SearchFor) + "&deleteid=' + id;\n");
                    writer.Append("}\n");
                    writer.Append("}\n");
                    writer.Append("</SCRIPT>\n");
                
            }
            ltContent.Text = writer.ToString();
		}


	}
}
