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

namespace AspDotNetStorefrontAdmin
{
	/// <summary>
	/// Summary description for movevariant
	/// </summary>
    public partial class entityMoveVariant : AdminPageBase
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
			}

            LoadContent();
		}

        protected void LoadContent()
		{
            string str = "";
            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
                using( IDataReader rs = DB.GetRS("select * from productvariant   with (NOLOCK)  where VariantID=" + VariantID.ToString(),dbconn))
                {
                    rs.Read();
                    str += ("<b>" + String.Format(AppLogic.GetString("admin.entityMoveVariant.WithinProduct", SkinID, LocaleSetting),AppLogic.GetProductName(ProductID, LocaleSetting),AppLogic.GetProductSKU(ProductID),ProductID.ToString()) + "<br/></b>\n");
                    str += ("<b>" + String.Format(AppLogic.GetString("admin.entityMoveVariant.MovingVariant", SkinID, LocaleSetting),DB.RSFieldByLocale(rs, "Name", LocaleSetting),DB.RSField(rs, "SKUSuffix"),DB.RSFieldInt(rs, "VariantID").ToString()) + "<br/><br/></b>\n");
                }
            }
			str += ("<script type=\"text/javascript\">\n");
			str += ("function MoveForm_Validator(theForm)\n");
			str += ("{\n");
			str += ("submitonce(theForm);\n");
			str += ("if (theForm.NewProductID.selectedIndex < 1)\n");
			str += ("{\n");
            str += ("alert(\"" + AppLogic.GetString("admin.common.SelectProductMoveVariantPrompt", SkinID, LocaleSetting) + "\");\n");
			str += ("theForm.NewProductID.focus();\n");
			str += ("submitenabled(theForm);\n");
			str += ("return (false);\n");
			str += ("    }\n");
			str += ("return (true);\n");
			str += ("}\n");
			str += ("</script>\n");

            str += ("<p>" + AppLogic.GetString("admin.entityMoveVariant.Select", SkinID, LocaleSetting) + ":</p>\n");
			str += ("<table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\">\n");
            str += ("<form action=\"" + AppLogic.AdminLinkUrl("movevariant.aspx") + "?productid=" + ProductID.ToString() + "&VariantID=" + VariantID.ToString() + "\" method=\"post\" id=\"MoveForm\" name=\"MoveForm\" onsubmit=\"return (validateForm(this) && MoveForm_Validator(this))\" onReset=\"return confirm('" + AppLogic.GetString("admin.common.ResetAllFieldsPrompt", SkinID, LocaleSetting) + "');\">\n");
			str += ("<input type=\"hidden\" name=\"IsSubmit\" value=\"true\">\n");
			str += ("              <tr valign=\"middle\">\n");
			str += ("                <td width=\"100%\" colspan=\"2\" align=\"left\">\n");
			str += ("                </td>\n");
			str += ("              </tr>\n");

			str += ("              <tr valign=\"middle\">\n");
            str += ("                <td align=\"right\" valign=\"middle\">*" + AppLogic.GetString("admin.entityMoveVariant.AssignToProduct", SkinID, LocaleSetting) + ":&nbsp;&nbsp;</td>\n");
			str += ("                <td align=\"left\">\n");
			str += ("<select size=\"1\" name=\"NewProductID\">\n");
            str += (" <OPTION VALUE=\"0\" selected>" + AppLogic.GetString("admin.common.SelectOne", SkinID, LocaleSetting) + "</option>\n");

            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
                using(IDataReader rsst = DB.GetRS("select * from Product   with (NOLOCK)  where deleted=0 and published=1",dbconn))
                {
                    while (rsst.Read())
                    {
                        str += ("<option value=\"" + DB.RSFieldInt(rsst, "ProductID").ToString() + "\"");
                        str += (">" + DB.RSFieldByLocale(rsst, "Name", LocaleSetting) + "</option>");
                    }
                }
            }
           
			str += ("</select>\n");
			str += ("</td>\n");
			str += ("</tr>\n");

			str += ("<tr>\n");
			str += ("<td></td><td align=\"left\"><br/>\n");
            str += ("<input type=\"submit\" value=\"admin.common.Move\" name=\"submit\" class=\"normalButtons\">\n");
			str += ("</td>\n");
			str += ("</tr>\n");
			str += ("</form>\n");
			str += ("</table>\n");

            ltContent.Text = str;
		}

	}
}
