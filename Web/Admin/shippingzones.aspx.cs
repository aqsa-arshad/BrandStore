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
	/// Summary description for ShippingZones.
	/// </summary>
    public partial class ShippingZones : AdminPageBase
	{
		
		protected void Page_Load(object sender, System.EventArgs e)
		{
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache"); 

			SectionTitle = "Manage Shipping Zones";
            RenderHtml();
        }

		private void RenderHtml()
        {
            StringBuilder writer = new StringBuilder(); 
            if (CommonLogic.QueryStringCanBeDangerousContent("DeleteID").Length != 0)
			{
				// delete the Zone:
				DB.ExecuteSQL("delete from ShippingWeightByZone where ShippingZoneID=" + CommonLogic.QueryStringCanBeDangerousContent("DeleteID"));
				DB.ExecuteSQL("delete from ShippingTotalByZone where ShippingZoneID=" + CommonLogic.QueryStringCanBeDangerousContent("DeleteID"));
				DB.ExecuteSQL("delete from ShippingZone where ShippingZoneID=" + CommonLogic.QueryStringCanBeDangerousContent("DeleteID"));
			}

            writer.Append("<form Method=\"POST\" action=\"" + AppLogic.AdminLinkUrl("ShippingZones.aspx") + "\">\n");
            writer.Append("<input type=\"hidden\" name=\"IsSubmit\" value=\"true\">\n");
            writer.Append("  <table border=\"0\" cellpadding=\"0\" border=\"0\" cellspacing=\"0\" width=\"100%\">");
            writer.Append("    <tr class=\"table-header\">\n");
            writer.Append("      <td width=\"5%\" align=\"center\"><b>ID</b></td>\n");
            writer.Append("      <td align=\"left\"><b>Zone</b></td>\n");
            writer.Append("      <td align=\"left\"><b>Country</b></td>\n");
            writer.Append("      <td align=\"left\" width=\"50%\" align=\"left\"><b>ZipCodes</b></td>\n");
            writer.Append("      <td align=\"center\"><b>Edit</b></td>\n");
            writer.Append("      <td align=\"center\"><b>Delete</b></td>\n");
            writer.Append("    </tr>\n");
            string style;
            int counter = 0;
            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select sz.*,c.Name as CName from ShippingZone sz, Country  c with (NOLOCK) where deleted=0 and sz.CountryID = c.CountryID  order by sz.DisplayOrder,sz.Name", con))
                {
                    while (rs.Read())
                    {
                        if (counter % 2 == 0)
                        {
                            style = "\"table-row2\"";
                        }
                        else
                        {
                            style = "\"table-alternatingrow2\"";
                        }
                        writer.Append("    <tr class=" + style + ">\n");
                        writer.Append("      <td width=\"5%\"  align=\"center\">" + DB.RSFieldInt(rs, "ShippingZoneID").ToString() + "</td>\n");
                        writer.Append("      <td align=\"left\"><a href=\"" + AppLogic.AdminLinkUrl("editShippingZone.aspx") + "?ShippingZoneID=" + DB.RSFieldInt(rs, "ShippingZoneID").ToString() + "\">" + DB.RSFieldByLocale(rs, "Name", LocaleSetting) + "</a></td>\n");
                        writer.Append("      <td  align=\"left\">" + DB.RSField(rs, "CName").ToString() + "</td>\n");
                        writer.Append("      <td align=\"left\" width=\"50%\"  align=\"center\">" + DB.RSField(rs, "ZipCodes") + "</td>\n");
                        writer.Append("      <td align=\"center\"><input type=\"button\" class=\"normalButtons\" value=\"Edit\" name=\"Edit_" + DB.RSFieldInt(rs, "ShippingZoneID").ToString() + "\" onClick=\"self.location='" + AppLogic.AdminLinkUrl("editShippingZone.aspx") + "?ShippingZoneID=" + DB.RSFieldInt(rs, "ShippingZoneID").ToString() + "'\"></td>\n");
                        writer.Append("      <td align=\"center\"><input type=\"button\" class=\"normalButtons\" value=\"Delete\" name=\"Delete_" + DB.RSFieldInt(rs, "ShippingZoneID").ToString() + "\" onClick=\"DeleteShippingZone(" + DB.RSFieldInt(rs, "ShippingZoneID").ToString() + ")\"></td>\n");
                        writer.Append("    </tr>\n");

                        counter++;
                    }

                    rs.Close();
                    rs.Dispose();
                }

                con.Close();
                con.Dispose();
            }

			writer.Append("</table>\n");
            writer.Append("<input type=\"button\" class=\"normalButtons\" value=\"Add New Shipping Zone\" name=\"AddNew\" onClick=\"self.location='" + AppLogic.AdminLinkUrl("editShippingZone.aspx") + "';\">\n");
			writer.Append("</form>\n");

			writer.Append("</center></b>\n");

			writer.Append("<script type=\"text/javascript\">\n");
			writer.Append("function DeleteShippingZone(id)\n");
			writer.Append("{\n");
			writer.Append("if(confirm('Are you sure you want to delete Shipping Zone: ' + id))\n");
			writer.Append("{\n");
            writer.Append("self.location = '" + AppLogic.AdminLinkUrl("ShippingZones.aspx") + "?deleteid=' + id;\n");
			writer.Append("}\n");
			writer.Append("}\n");
			writer.Append("</SCRIPT>\n");
            ltContent.Text = writer.ToString();
        }

	}
}
