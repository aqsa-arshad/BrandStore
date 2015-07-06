// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using AspDotNetStorefrontCore;
using System.Web.UI;
using System.IO;
using AspDotNetStorefrontAdmin;
namespace AspDotNetStorefrontAdmin
{
    /// <summary>
    /// Summary description for users.
    /// </summary>
    public partial class users : AdminPageBase
    {

        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");
            SectionTitle = "Manage Admin Users";
            RenderHtml();
        }

        private void RenderHtml()
        {
            StringBuilder writer = new StringBuilder();
            if (ThisCustomer.IsAdminSuperUser)
            {
                if (CommonLogic.QueryStringCanBeDangerousContent("RemoveAdminID").Length != 0)
                {
                    // remove admin rights from this user:
                    DB.ExecuteSQL("update customer set IsAdmin=0 where CustomerID=" + CommonLogic.QueryStringCanBeDangerousContent("RemoveAdminID"));
                }

                if (CommonLogic.FormBool("IsSubmit"))
                {
                    // add admin rights to this user:
                    DB.ExecuteSQL("update customer set IsAdmin=1 where deleted=0 and EMail=" + DB.SQuote(CommonLogic.FormCanBeDangerousContent("EMail").ToLowerInvariant().Trim()));
                }

                writer.Append("<p>The following users are store administrators:</p>\n");

                string SuperuserFilter = CommonLogic.IIF(ThisCustomer.IsAdminSuperUser, String.Empty, String.Format(" CustomerID not in ({0}) and ", AppLogic.AppConfig("Admin_Superuser")));

                writer.Append("  <table border=\"0\" cellpadding=\"2\" border=\"0\" cellspacing=\"0\" width=\"100%\">\n");
                writer.Append("    <tr class=\"gridHeader\">\n");
                writer.Append("      <td ><b>ID</b></td>\n");
                writer.Append("      <td ><b>E-Mail</b></td>\n");
                writer.Append("      <td align=\"center\"><b>First Name</b></td>\n");
                writer.Append("      <td align=\"center\"><b>Last Name</b></td>\n");
                writer.Append("      <td align=\"center\"><b>Remove Admin Rights</b></td>\n");
                writer.Append("    </tr>\n");

                using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
                {
                    conn.Open();
                    using (IDataReader rs = DB.GetRS("select * from customer   with (NOLOCK)  where deleted=0 and " + SuperuserFilter.ToString() + " IsAdmin=1 order by EMail", conn))
                    {
                        int i = 0;
                        while (rs.Read())
                        {
                            i++;
                            if (i % 2 == 0)
                            {
                                writer.Append("    <tr class=\"table-row2\">\n");
                            }
                            else
                            {
                                writer.Append("    <tr class=\"table-alternatingrow2\">\n");
                            }

                            writer.Append("      <td >" + DB.RSFieldInt(rs, "CustomerID").ToString() + "</td>\n");
                            writer.Append("      <td ><a href=\"" + AppLogic.AdminLinkUrl("cst_account.aspx") + "?customerid=" + DB.RSFieldInt(rs, "CustomerID").ToString() + "\">" + CommonLogic.IIF(Customer.StaticIsAdminSuperUser(DB.RSFieldInt(rs, "CustomerID")), "*", "") + DB.RSField(rs, "EMail") + "</a></td>\n");
                            writer.Append("      <td >" + DB.RSField(rs, "FirstName") + "</td>\n");
                            writer.Append("      <td >" + DB.RSField(rs, "LastName") + "</td>\n");
                            if (Customer.StaticIsAdminSuperUser(DB.RSFieldInt(rs, "CustomerID")))
                            {
                                writer.Append("<td align=\"center\">Admin SuperUser</td>");
                            }
                            else
                            {
                                writer.Append("      <td align=\"center\"><input type=\"button\" value=\"Remove Admin Rights\" name=\"RemoveAdmin_" + DB.RSFieldInt(rs, "CustomerID").ToString() + "\" onClick=\"RemoveAdmin(" + DB.RSFieldInt(rs, "CustomerID").ToString() + ")\"></td>\n");
                            }
                            writer.Append("    </tr>\n");

                        }

                    }


                }

                writer.Append("  </table>\n");

                writer.Append("<script type=\"text/javascript\">\n");
                writer.Append("function UserForm_Validator(theForm)\n");
                writer.Append("{\n");
                writer.Append("submitonce(theForm);\n");
                writer.Append("return (true);\n");
                writer.Append("}\n");
                writer.Append("</script>\n");

                writer.Append("<form action=\"" + AppLogic.AdminLinkUrl("users.aspx") + "\" method=\"post\" id=\"UserForm\" name=\"UserForm\" onsubmit=\"return (validateForm(this) && UserForm_Validator(this))\" onReset=\"return confirm('Do you want to reset all fields to their starting values?');\">\n");
                writer.Append("<table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\">\n");
                writer.Append("<input type=\"hidden\" name=\"IsSubmit\" value=\"true\">\n");
                writer.Append("              <tr valign=\"middle\">\n");
                writer.Append("                <td width=\"30%\" align=\"right\" valign=\"top\">*Assign Admin Privileges To User:&nbsp;&nbsp;</td>\n");
                writer.Append("                <td align=\"left\" valign=\"top\">\n");
                writer.Append("                	<input maxLength=\"100\" size=\"30\" name=\"EMail\" value=\"\" onkeypress=\"javascript:return WebForm_FireDefaultButton(event, 'btnsubmit')\"><br/>Enter the e-mail address of the user you want to make a store administrator.<br/>This customer record must already existing in the database<br/>If you need to create a new customer record first, please do that first.\n");
                writer.Append("                	<input type=\"hidden\" name=\"EMail_vldt\" value=\"[req][blankalert=Please enter the e-mail address of the user you want to set admin privileges for. This customer record must already exist. If you need to a new customer record first, select the User -> Add New Customer menu option!]\">\n");
                writer.Append("                	</td>\n");
                writer.Append("              </tr>\n");
                writer.Append("<tr>\n");
                writer.Append("<td></td><td align=\"left\"><br/>\n");
                writer.Append("<input type=\"submit\" class=\"normalButtons\" value=\"Add New Admin\" id=\"btnsubmit\" name=\"submit\">\n");
                writer.Append("        </td>\n");
                writer.Append("      </tr>\n");
                writer.Append("  </table>\n");
                writer.Append("</form>\n");

                writer.Append("<script type=\"text/javascript\">\n");
                writer.Append("function RemoveAdmin(id)\n");
                writer.Append("{\n");
                writer.Append("if(confirm('Are you sure you want to remove the admin rights of customer: ' + id + ' (this does not delete their user record, just their admin rights)'))\n");
                writer.Append("{\n");
                writer.Append("self.location = '" + AppLogic.AdminLinkUrl("users.aspx") + "?RemoveAdminId=' + id;\n");
                writer.Append("}\n");
                writer.Append("}\n");
                writer.Append("</SCRIPT>\n");
            }
            else
            {
                writer.Append("<p><b>INSUFFICIENT PERMISSIONS</b></p>");
            }
            ltContent.Text = writer.ToString();
        }

    }
}
