// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Data;
using System.Data.SqlClient;
using System.Text;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
    /// <summary>
    /// Summary description for customerlevels.
    /// </summary>
    public partial class customerlevels : AdminPageBase
    {

        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            if (CommonLogic.QueryStringCanBeDangerousContent("DeleteID").Length != 0)
            {
                // delete the level:

                // clear the carts for all customers whose customer levels are being reassigned. This is to ensure their produce pricing is correct
                // their current cart can have customer level pricing, not retail pricing, and this prevents that:
                DB.ExecuteSQL("delete from shoppingcart where CartType in (" + ((int)CartTypeEnum.ShoppingCart).ToString() + "," + ((int)CartTypeEnum.GiftRegistryCart).ToString() + "," + ((int)CartTypeEnum.WishCart).ToString() + ") and customerid in (select customerid from customer where CustomerLevelID=" + CommonLogic.QueryStringCanBeDangerousContent("DeleteID") + ")");
                DB.ExecuteSQL("delete from kitcart where CartType in (" + ((int)CartTypeEnum.ShoppingCart).ToString() + "," + ((int)CartTypeEnum.GiftRegistryCart).ToString() + "," + ((int)CartTypeEnum.WishCart).ToString() + ") and customerid in (select customerid from customer where CustomerLevelID=" + CommonLogic.QueryStringCanBeDangerousContent("DeleteID") + ")");

                DB.ExecuteSQL("delete from ExtendedPrice where CustomerLevelID=" + CommonLogic.QueryStringCanBeDangerousContent("DeleteID"));
                DB.ExecuteSQL("update Customer set CustomerLevelID=0 where CustomerLevelID=" + CommonLogic.QueryStringCanBeDangerousContent("DeleteID"));
                DB.ExecuteSQL("delete from CustomerLevel where CustomerLevelID=" + CommonLogic.QueryStringCanBeDangerousContent("DeleteID"));
            }
            SectionTitle = AppLogic.GetString("admin.menu.CustomerLevels", SkinID, LocaleSetting);
            RenderHtml();
        }

        private void RenderHtml()
        {
            StringBuilder writer = new StringBuilder();
            writer.Append("<form method=\"POST\" action=\"" + AppLogic.AdminLinkUrl("customerlevels.aspx") + "\">\n");
            writer.Append("<input type=\"hidden\" name=\"IsSubmit\" value=\"true\">\n");
            writer.Append("<table border=\"0\" cellpadding=\"0\" border=\"0\" cellspacing=\"0\" width=\"100%\">\n");
            writer.Append("<tr class=\"table-header\">\n");
            writer.Append("<td width=\"10%\">" + AppLogic.GetString("admin.common.ID", SkinID, LocaleSetting) + "</td>\n");
            writer.Append("<td align=\"left\" valign=\"middle\">" + AppLogic.GetString("admin.tabs.Description", SkinID, LocaleSetting) + "</td>\n");
            writer.Append("<td width=\"20%\" align=\"left\" valign=\"middle\">" + AppLogic.GetString("admin.currencies.ViewCustomerLevel", SkinID, LocaleSetting) + "</td>\n");
            writer.Append("<td width=\"10%\" align=\"left\" valign=\"middle\">" + AppLogic.GetString("admin.common.Edit", SkinID, LocaleSetting) + "</td>\n");
            writer.Append("<td width=\"10%\" align=\"left\" valign=\"middle\">" + AppLogic.GetString("admin.common.Delete", SkinID, LocaleSetting) + "</td>\n");
            writer.Append("</tr>\n");

            int i = 0;           
            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("select * from CustomerLevel   with (NOLOCK)  where deleted=0 order by DisplayOrder,Name", dbconn))
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
                        writer.Append("<td width=\"10%\" align=\"left\" valign=\"middle\">" + DB.RSFieldInt(rs, "CustomerLevelID").ToString() + "</td>\n");
                        writer.Append("<td><a href=\"" + AppLogic.AdminLinkUrl("editCustomerLevel.aspx") + "?CustomerLevelID=" + DB.RSFieldInt(rs, "CustomerLevelID").ToString() + "\">" + DB.RSFieldByLocale(rs, "Name", LocaleSetting) + "</a></td>\n");
                        writer.Append("<td width=\"20%\" align=\"left\" valign=\"middle\"><input class=\"normalButtons\" type=\"button\" value=\"" + AppLogic.GetString("admin.customerlevels.ShowCustomers", SkinID, LocaleSetting) + "\" name=\"Edit_" + DB.RSFieldInt(rs, "CustomerLevelID").ToString() + "\" onClick=\"self.location='" + AppLogic.AdminLinkUrl("showCustomerLevel.aspx") + "?CustomerLevelID=" + DB.RSFieldInt(rs, "CustomerLevelID").ToString() + "'\"></td>\n");
                        writer.Append("<td width=\"10%\" align=\"left\" valign=\"middle\"><input class=\"normalButtons\" type=\"button\" value=\"" + AppLogic.GetString("admin.common.Edit", SkinID, LocaleSetting) + "\" name=\"Edit_" + DB.RSFieldInt(rs, "CustomerLevelID").ToString() + "\" onClick=\"self.location='" + AppLogic.AdminLinkUrl("editCustomerLevel.aspx") + "?CustomerLevelID=" + DB.RSFieldInt(rs, "CustomerLevelID").ToString() + "'\"></td>\n");
                        writer.Append("<td width=\"10%\" align=\"left\" valign=\"middle\"><input class=\"normalButtons\" type=\"button\" value=\"" + AppLogic.GetString("admin.common.Delete", SkinID, LocaleSetting) + "\" name=\"Delete_" + DB.RSFieldInt(rs, "CustomerLevelID").ToString() + "\" onClick=\"DeleteCustomerLevel(" + DB.RSFieldInt(rs, "CustomerLevelID").ToString() + ")\"></td>\n");
                        writer.Append("</tr>\n");
                        i++;
                    }
                }
            }


            writer.Append(" </table>\n");
            writer.Append("<p align=\"left\"><input class=\"normalButtons\" type=\"button\" value=\"" + AppLogic.GetString("admin.customerlevels.AddNewCustomerLevel", SkinID, LocaleSetting) + "\" name=\"AddNew\" onClick=\"self.location='" + AppLogic.AdminLinkUrl("editCustomerLevel.aspx") + "';\"></p>\n");
            writer.Append("</form>\n");

            writer.Append("</center></b>\n");

            writer.Append("<script type=\"text/javascript\">\n");
            writer.Append("function DeleteCustomerLevel(id)\n");
            writer.Append("{\n");
            writer.Append("if(confirm('" + AppLogic.GetString("admin.customerlevels.DeleteCustomerLevel", SkinID, LocaleSetting) + " ' + id))\n");
            writer.Append("{\n");
            writer.Append("self.location = '" + AppLogic.AdminLinkUrl("CustomerLevels.aspx") + "?deleteid=' + id;\n");
            writer.Append("}\n");
            writer.Append("}\n");
            writer.Append("</SCRIPT>\n");
            ltContent.Text = writer.ToString();
        }

    }
}
