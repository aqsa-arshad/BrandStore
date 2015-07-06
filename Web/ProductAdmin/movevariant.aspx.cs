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
	/// Summary description for movevariant
	/// </summary>
    public partial class movevariant : AdminPageBase
	{
		
		int ProductID;
		int VariantID;
		
		protected void Page_Load(object sender, System.EventArgs e)
		{
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache"); 
            

			ProductID = CommonLogic.QueryStringUSInt("ProductID");
			VariantID = CommonLogic.QueryStringUSInt("VariantID");

			if(CommonLogic.FormBool("IsSubmit"))
			{
				DB.ExecuteSQL("Update productvariant set ProductID=" + CommonLogic.FormCanBeDangerousContent("NewProductID") + " where VariantID=" + VariantID.ToString());
				Response.Redirect(AppLogic.AdminLinkUrl("variants.aspx")+ "?productid=" + ProductID.ToString());
			}
            SectionTitle = String.Format(AppLogic.GetString("admin.sectiontitle.movevariant", SkinID, LocaleSetting),AppLogic.AdminLinkUrl("variants.aspx"),ProductID.ToString());
            RenderHtml();
		}

		private void RenderHtml()
		{
            StringBuilder writer = new StringBuilder();
            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("select * from productvariant   with (NOLOCK)  where VariantID=" + VariantID.ToString(), dbconn))
                {
                    rs.Read();
                    writer.Append(String.Format(AppLogic.GetString("admin.movevariant.WithinProduct", SkinID, LocaleSetting),AppLogic.AdminLinkUrl("editproduct.aspx"),ProductID.ToString(),AppLogic.GetProductName(ProductID, LocaleSetting),AppLogic.GetProductSKU(ProductID),ProductID.ToString()));
                    writer.Append(String.Format(AppLogic.GetString("admin.movevariant.MovingVariant", SkinID, LocaleSetting),DB.RSFieldByLocale(rs, "Name", LocaleSetting),DB.RSField(rs, "SKUSuffix"),DB.RSFieldInt(rs, "VariantID").ToString()));
                }
            }
			

			writer.Append("<script type=\"text/javascript\">\n");
			writer.Append("function MoveForm_Validator(theForm)\n");
			writer.Append("{\n");
			writer.Append("submitonce(theForm);\n");
			writer.Append("if (theForm.NewProductID.selectedIndex < 1)\n");
			writer.Append("{\n");
            writer.Append("alert(\"" + AppLogic.GetString("admin.common.SelectProductMoveVariantPrompt", SkinID, LocaleSetting) + "\");\n");
			writer.Append("theForm.NewProductID.focus();\n");
			writer.Append("submitenabled(theForm);\n");
			writer.Append("return (false);\n");
			writer.Append("    }\n");
			writer.Append("return (true);\n");
			writer.Append("}\n");
			writer.Append("</script>\n");

            writer.Append("<p>" + AppLogic.GetString("admin.entityMoveVariant.Select", SkinID, LocaleSetting) + "</p>\n");
			writer.Append("<table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\">\n");
            writer.Append("<form action=\"" + AppLogic.AdminLinkUrl("movevariant.aspx") + "?productid=" + ProductID.ToString() + "&VariantID=" + VariantID.ToString() + "\" method=\"post\" id=\"MoveForm\" name=\"MoveForm\" onsubmit=\"return (validateForm(this) && MoveForm_Validator(this))\" onReset=\"return confirm('Do you want to reset all fields to their starting values?');\">\n");
			writer.Append("<input type=\"hidden\" name=\"IsSubmit\" value=\"true\">\n");
			writer.Append("              <tr valign=\"middle\">\n");
			writer.Append("                <td width=\"100%\" colspan=\"2\" align=\"left\">\n");
			writer.Append("                </td>\n");
			writer.Append("              </tr>\n");

			writer.Append("              <tr valign=\"middle\">\n");
            writer.Append("                <td align=\"right\" valign=\"middle\">*" + AppLogic.GetString("admin.entityMoveVariant.AssignToProduct", SkinID, LocaleSetting) + ":&nbsp;&nbsp;</td>\n");
			writer.Append("                <td align=\"left\">\n");
			writer.Append("<select size=\"1\" name=\"NewProductID\">\n");
            writer.Append(" <OPTION VALUE=\"0\" selected>" + AppLogic.GetString("admin.common.SelectOne", SkinID, LocaleSetting) + "</option>\n");

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rsst = DB.GetRS("select * from Product   with (NOLOCK)  where deleted=0 and published=1", dbconn))
                {
                    while (rsst.Read())
                    {
                        writer.Append("<option value=\"" + DB.RSFieldInt(rsst, "ProductID").ToString() + "\"");
                        writer.Append(">" + DB.RSFieldByLocale(rsst, "Name", LocaleSetting) + "</option>");
                    }
                }
            }
			writer.Append("</select>\n");
			writer.Append("</td>\n");
			writer.Append("</tr>\n");

			writer.Append("<tr>\n");
			writer.Append("<td></td><td align=\"left\"><br/>\n");
            writer.Append("<input type=\"submit\" class=\"normalButtons\" value=\"" + AppLogic.GetString("admin.common.Move", SkinID, LocaleSetting) + "\" name=\"submit\">\n");
			writer.Append("</td>\n");
			writer.Append("</tr>\n");
			writer.Append("</form>\n");
			writer.Append("</table>\n");
            ltContent.Text = writer.ToString();
		}

	}
}
