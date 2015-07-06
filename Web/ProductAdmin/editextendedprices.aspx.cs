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
    /// Summary description for editextendedprices
    /// </summary>
    public partial class editextendedprices : AdminPageBase
    {

        int VariantID;
        int ProductID;
        String VariantName;
        String VariantSKUSuffix;
        String VariantLink;

        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");


            VariantID = CommonLogic.QueryStringUSInt("VariantID");
            ProductID = CommonLogic.QueryStringUSInt("ProductID");
            VariantName = AppLogic.GetVariantName(VariantID, LocaleSetting);
            VariantSKUSuffix = AppLogic.GetVariantSKUSuffix(VariantID);
            if (VariantName.Length == 0)
            {
                VariantName = AppLogic.GetString("admin.editextendedprices.UnnamedVariant", SkinID, LocaleSetting);
            }
            if (VariantSKUSuffix.Length == 0)
            {
                VariantSKUSuffix = String.Empty;
            }
            if (ProductID == 0)
            {
                ProductID = AppLogic.GetVariantProductID(VariantID);
            }

            if (CommonLogic.FormBool("IsSubmit"))
            {
                // start with clean slate, to make all adds easy:
                DB.ExecuteSQL("delete from extendedprice where VariantID=" + VariantID.ToString());
                for (int i = 0; i <= Request.Form.Count - 1; i++)
                {
                    String FieldName = Request.Form.Keys[i];
                    if (FieldName.IndexOf("_vldt") == -1 && FieldName.IndexOf("Price_") != -1)
                    {
                        // this field should be processed
                        decimal FieldVal = CommonLogic.FormUSDecimal(FieldName);
                        String[] Parsed = FieldName.Split('_');
                        int CustomerLevelID = Localization.ParseUSInt(Parsed[1]);
                        if (FieldVal != System.Decimal.Zero)
                        {
                            DB.ExecuteSQL("insert into ExtendedPrice(ExtendedPriceGUID,VariantID,CustomerLevelID,Price) values(" + DB.SQuote(DB.GetNewGUID()) + "," + VariantID.ToString() + "," + CustomerLevelID.ToString() + "," + Localization.CurrencyStringForDBWithoutExchangeRate(FieldVal) + ")");
                        }
                    }
                }
            }
            VariantLink = " <a href=\"" + AppLogic.AdminLinkUrl("entityEditProductVariant.aspx") + "?productid=" + ProductID.ToString() + "&VariantID=" + VariantID.ToString() + "&entityname=CATEGORY&EntityID=0\">Variant</a> ";
            SectionTitle = "<a href=\"" + AppLogic.AdminLinkUrl("variants.aspx") + "?productid=" + ProductID.ToString() + "\">" + AppLogic.GetString("admin.editextendedprices.Variants", SkinID, LocaleSetting) + "</a> - "+ VariantLink + AppLogic.GetString("admin.editextendedprices.ExtendedPrices", SkinID, LocaleSetting);
            RenderHtml();
        }

        private void RenderHtml()
        {
            StringBuilder writer = new StringBuilder();
            if (ErrorMsg.Length != 0)
            {
                writer.Append("<p align=\"left\"><b><font color=red>" + ErrorMsg + "</font></b></p>\n");
            }
            if (DataUpdated)
            {
                writer.Append("<p align=\"left\"><b><font color=blue>" + AppLogic.GetString("admin.editextendedprices.updated", SkinID, LocaleSetting) + "</font></b></p>\n");
            }
            writer.Append("<p align=\"left\"><b>" + AppLogic.GetString("admin.editextendedprices.EditingExtended", SkinID, LocaleSetting) + VariantLink + ": " + VariantName + " (Variant SKUSuffix=" + VariantSKUSuffix + ", VariantID=" + VariantID.ToString() + ")</b></p>\n");
            writer.Append("<p align=\"left\"><b>" + AppLogic.GetString("admin.editextendedprices.WithinProduct", SkinID, LocaleSetting) + " <a href=\"" + AppLogic.AdminLinkUrl("entityEditProducts.aspx") + "?iden=" + ProductID.ToString() + "&entityName=CATEGORY&entityFilterID=0\">" + AppLogic.GetProductName(ProductID, LocaleSetting) + "</a> (Product SKU=" + AppLogic.GetProductSKU(ProductID) + ", ProductID=" + ProductID.ToString() + ")</b></p>\n");
            writer.Append("<script type=\"text/javascript\">\n");
            writer.Append("function Form_Validator(theForm)\n");
            writer.Append("{\n");
            writer.Append("submitonce(theForm);\n");
            writer.Append("return (true);\n");
            writer.Append("}\n");
            writer.Append("</script>\n");
            writer.Append("<p align=\"left\"><br/><br/>" + AppLogic.GetString("admin.editextendedprices.ExtendedPricesforVariant", SkinID, LocaleSetting) + "</p>\n");
            writer.Append("<form action=\"" + AppLogic.AdminLinkUrl("editextendedprices.aspx") + "?productid=" + ProductID.ToString() + "&VariantID=" + VariantID.ToString() + "\" method=\"post\" id=\"Form1\" name=\"Form1\" onsubmit=\"return (validateForm(this) && Form_Validator(this))\" onReset=\"return confirm('" + AppLogic.GetString("admin.editextendedprices.MsgConfirmation", SkinID, LocaleSetting) + "');\">\n");
            writer.Append("<input type=\"hidden\" name=\"IsSubmit\" value=\"true\">\n");
            writer.Append("<table align=\"left\" cellpadding=\"4\" cellspacing=\"0\">\n");
            writer.Append("<tr><td><b>" + AppLogic.GetString("admin.editextendedprices.CustomerLevel", SkinID, LocaleSetting) + "</b></td><td><b>" + AppLogic.GetString("admin.editextendedprices.ExtendedPrice", SkinID, LocaleSetting) + "</b></td></tr>\n");
            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("select cl.*, ep.Price from ProductCustomerLevel pcl join CustomerLevel cl  with (NOLOCK)  on pcl.CustomerLevelID = cl.CustomerLevelID left join ExtendedPrice ep   with (NOLOCK)  on cl.CustomerLevelID = ep.CustomerLevelID and ep.VariantID = " + VariantID.ToString() + " where pcl.ProductID = " + ProductID.ToString() + " and cl.deleted=0 order by cl.DisplayOrder,cl.Name", dbconn))
                {
                    while (rs.Read())
                    {
                        writer.Append("<tr valign=\"middle\">\n");
                        writer.Append("<td align=\"left\" valign=\"middle\">" + AppLogic.GetString("admin.editextendedprices.PriceForLevel", SkinID, LocaleSetting) + "<b>" + DB.RSFieldByLocale(rs, "Name", LocaleSetting) + "</b>:&nbsp;&nbsp;</td>\n");
                        writer.Append("<td align=\"left\" valign=\"top\">\n");
                        writer.Append("<input maxLength=\"10\" size=\"10\" name=\"Price_" + DB.RSFieldInt(rs, "CustomerLevelID").ToString() + "\" value=\"" + (Localization.DecimalStringForDB(DB.RSFieldDecimal(rs, "Price"))) + "\">");
                        writer.Append("<input type=\"hidden\" name=\"Price_" + DB.RSFieldInt(rs, "CustomerLevelID").ToString() + "_vldt\" value=\"[req][number][blankalert=" + AppLogic.GetString("admin.editextendedprices.msgInformation1", SkinID, LocaleSetting) + "][invalidalert="+ AppLogic.GetString("admin.editextendedprices.msgInformation2", SkinID, LocaleSetting) +"]\">\n");
                        writer.Append("</td>\n");
                        writer.Append("</tr>\n");
                    }
                }
            }
            writer.Append("<tr>\n");
            writer.Append("<td align=\"left\" colspan=\"2\"><br/>\n");
            writer.Append("<input CssClass=\"normalButtons\" type=\"submit\" class=\"normalButtons\"value=\"Update\" name=\"submit\">\n");
            writer.Append("</td>\n");
            writer.Append("</tr>\n");
            writer.Append("</table>\n");
            writer.Append("</form>\n");
            ltContent.Text = writer.ToString();

        }

    }
}
