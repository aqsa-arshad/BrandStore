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
	/// Summary description for ShippingMethodZones.
	/// </summary>
    public partial class ShippingMethodZones : AdminPageBase
	{

		int ShippingMethodID = 0;
		String ShippingMethodName = String.Empty;
        bool IsUpdated = false;
		
		protected void Page_Load(object sender, System.EventArgs e)
		{
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            ShippingMethodID = CommonLogic.QueryStringUSInt("ShippingMethodID");
			if(ShippingMethodID == 0)
			{
				Response.Redirect(AppLogic.AdminLinkUrl("shippingmethods.aspx"));
			}
			ShippingMethodName = Shipping.GetShippingMethodName(ShippingMethodID,LocaleSetting);
            SectionTitle = "<a href=\"" + AppLogic.AdminLinkUrl("shippingmethods.aspx") + "\">Shipping Methods</a> - Setting Allowed Zones for Shipping Method: " + ShippingMethodName;
		
			if(CommonLogic.FormBool("IsSubmit"))
			{
                IsUpdated = true;
				DB.ExecuteSQL("delete from ShippingMethodToZoneMap where ShippingMethodID=" + ShippingMethodID.ToString());
				foreach(String s in CommonLogic.FormCanBeDangerousContent("ZoneList").Split(','))
				{
					if(s.Trim().Length != 0)
					{
						DB.ExecuteSQL("insert ShippingMethodToZoneMap(ShippingMethodID,ShippingZoneID) values(" + ShippingMethodID.ToString() + "," + s + ")");
					}
				}
			}

			if(CommonLogic.QueryStringCanBeDangerousContent("clearall").Length != 0)
			{
				DB.ExecuteSQL("delete from ShippingMethodToZoneMap where ShippingMethodID=" + ShippingMethodID.ToString());
			}
			if(CommonLogic.QueryStringCanBeDangerousContent("allowall").Length != 0)
			{
				DB.ExecuteSQL("delete from ShippingMethodToZoneMap where ShippingMethodID=" + ShippingMethodID.ToString());
				DB.ExecuteSQL("insert into ShippingMethodToZoneMap(ShippingMethodID,ShippingZoneID) select " + ShippingMethodID.ToString() + ",ShippingZoneID from ShippingZone");
			}
            RenderHtml();
		}

		private void RenderHtml()
		{
            StringBuilder writer = new StringBuilder();
			if(CommonLogic.QueryStringCanBeDangerousContent("DeleteID").Length != 0)
			{
				// delete the record:
				DB.ExecuteSQL("delete from ShippingZone where ShippingZoneID=" + CommonLogic.QueryStringCanBeDangerousContent("DeleteID"));
			}

			if(DB.GetSqlN("select count(*) as N from ShippingZone with (NOLOCK)") == 0)
			{
				writer.Append("<p><b><font color=red>No Shipping Zones are defined!</font></b></p>");
			}
			else
			{

                if (IsUpdated)
                {
                    writer.Append("<p><strong>NOTICE: </strong> Item Updated</p>");
                }

                writer.Append("<form method=\"POST\" action=\"" + AppLogic.AdminLinkUrl("shippingmethodzones.aspx") + "?shippingmethodid=" + ShippingMethodID.ToString() + "\">\n");
				writer.Append("<input type=\"hidden\" name=\"IsSubmit\" value=\"true\">\n");
                writer.Append("<p align=\"left\">Check the Shipping Zones that you want to <b>ALLOW</b> for this shipping method.&nbsp;&nbsp;&nbsp;&nbsp;<a href=\"" + AppLogic.AdminLinkUrl("shippingmethodzones.aspx") + "?shippingmethodid=" + ShippingMethodID.ToString() + "&allowall=true\">ALLOW ALL</a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a href=\"" + AppLogic.AdminLinkUrl("shippingmethodzones.aspx") + "?shippingmethodid=" + ShippingMethodID.ToString() + "&clearall=true\">CLEAR ALL</a><p>");

                writer.Append("<p align=\"left\"><input class=\"normalButtons\" type=\"submit\" value=\"Update\"><p>");
				writer.Append("<table border=\"0\" cellpadding=\"2\" border=\"0\" cellspacing=\"0\" >\n");

                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS("select ShippingZone.ShippingZoneID,ShippingZone.Name,ShippingMethodToZoneMap.ShippingMethodID from ShippingZone  with (NOLOCK)  left outer join ShippingMethodToZoneMap  with (NOLOCK)  on ShippingZone.ShippingZoneID=ShippingMethodToZoneMap.ShippingZoneID and ShippingMethodToZoneMap.ShippingMethodID=" + ShippingMethodID.ToString() + " order by displayorder,name", con))
                    {
                        while (rs.Read())
                        {
                            bool AllowedForThisZone = DB.RSFieldInt(rs, "ShippingMethodID") != 0;
                            writer.Append("<tr class=\"table-row2\" >");
                            writer.Append("<td>");
                            writer.Append(DB.RSFieldByLocale(rs, "Name", LocaleSetting));
                            writer.Append("</td>");
                            writer.Append("<td>");
                            writer.Append("<input type=\"checkbox\" name=\"ZoneList\" value=\"" + DB.RSFieldInt(rs, "ShippingZoneID").ToString() + "\" " + CommonLogic.IIF(AllowedForThisZone, " checked ", "") + ">");
                            writer.Append("</td>");
                            writer.Append("</tr>\n");
                        }
                    }
                }
                writer.Append("</table>");

                writer.Append("<p align=\"left\"><input class=\"normalButtons\" type=\"submit\" value=\"Update\"><p>");
				writer.Append("</form>\n");
			}
            ltContent.Text = writer.ToString();
		}

	}
}
