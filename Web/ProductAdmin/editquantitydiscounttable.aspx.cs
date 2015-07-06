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
    /// Summary description for editquantitydiscounttable.
    /// </summary>
    public partial class editquantitydiscounttable : AdminPageBase
    {

        int QuantityDiscountID;
        String QuantityDiscountName;

        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache"); 
            

            QuantityDiscountID = CommonLogic.QueryStringUSInt("QuantityDiscountID");
            QuantityDiscountName = QuantityDiscount.GetQuantityDiscountName(QuantityDiscountID, LocaleSetting);
            if (CommonLogic.FormBool("IsSubmitByCount"))
            {
                // check for new row addition:
                int Low0 = CommonLogic.FormUSInt("Low_0");
                int High0 = CommonLogic.FormUSInt("High_0");
                String NewGUID = DB.GetNewGUID();
                int NewRowID = 0;

                if (Low0 != 0 || High0 != 0)
                {
                    // add the new row if necessary:
                    Decimal Discount = CommonLogic.FormUSDecimal("Rate_0_" + QuantityDiscountID.ToString());
                    DB.ExecuteSQL("insert into QuantityDiscountTable(QuantityDiscountTableGUID,QuantityDiscountID,LowQuantity,HighQuantity,DiscountPercent) values(" + DB.SQuote(NewGUID) + "," + QuantityDiscountID.ToString() + "," + Localization.IntStringForDB(Low0) + "," + Localization.IntStringForDB(High0) + "," + Localization.DecimalStringForDB(Discount) + ")");
                }

                using (SqlConnection dbconn = DB.dbConn())
                {
                    dbconn.Open();
                    using (IDataReader rs = DB.GetRS("Select QuantityDiscountTableID from QuantityDiscountTable   with (NOLOCK)  where QuantityDiscountTableGUID=" + DB.SQuote(NewGUID), dbconn))
                    {
                        rs.Read();
                        NewRowID = DB.RSFieldInt(rs, "QuantityDiscountTableID");
                    }
                }

                // update existing rows:
                for (int i = 0; i <= Request.Form.Count - 1; i++)
                {
                    String FieldName = Request.Form.Keys[i];
                    if (FieldName.IndexOf("_0_") == -1 && FieldName != "Low_0" && FieldName != "High_0" && FieldName.IndexOf("_vldt") == -1 && (FieldName.IndexOf("Rate_") != -1 || FieldName.IndexOf("Low_") != -1 || FieldName.IndexOf("High_") != -1))
                    {
                        Decimal FieldVal = CommonLogic.FormUSDecimal(FieldName);
                        // this field should be processed
                        String[] Parsed = FieldName.Split('_');
                        if (FieldName.IndexOf("Rate_") != -1)
                        {
                            // update discount:
                            DB.ExecuteSQL("Update QuantityDiscountTable set DiscountPercent=" + Localization.DecimalStringForDB(FieldVal) + " where QuantityDiscountTableID=" + Parsed[1]);
                        }
                        if (FieldName.IndexOf("Low_") != -1)
                        {
                            // update low value:
                            DB.ExecuteSQL("Update QuantityDiscountTable set LowQuantity=" + FieldVal.ToString() + " where QuantityDiscountTableID=" + DB.SQuote(Parsed[1]));
                        }
                        if (FieldName.IndexOf("High_") != -1)
                        {
                            // update high value:
                            DB.ExecuteSQL("Update QuantityDiscountTable set HighQuantity=" + FieldVal.ToString() + " where QuantityDiscountTableID=" + DB.SQuote(Parsed[1]));
                        }
                    }
                }
                DB.ExecuteSQL("Update QuantityDiscountTable set HighQuantity=999999 where HighQuantity=0 and LowQuantity<>0");
            }

            if (CommonLogic.QueryStringCanBeDangerousContent("deleteByCountid").Length != 0)
            {
                DB.ExecuteSQL("delete from QuantityDiscountTable where QuantityDiscountTableID=" + CommonLogic.QueryStringCanBeDangerousContent("deleteByCountid"));
            }
            SectionTitle = "<a href=\"" + AppLogic.AdminLinkUrl("quantitydiscounts.aspx") + "\">" + AppLogic.GetString("admin.editquantitydiscounttable.QuantityDiscounts", SkinID, LocaleSetting) + "</a> - " + String.Format(AppLogic.GetString("admin.editquantitydiscounttable.ManageQuantityDiscountTable", SkinID, LocaleSetting),QuantityDiscountName);
            RenderHtml();
        }

        private void RenderHtml()
        {
            StringBuilder writer = new StringBuilder();
            writer.Append("<script type=\"text/javascript\">\n");
            writer.Append("function ByCountForm_Validator(theForm)\n");
            writer.Append("{\n");
            writer.Append("submitonce(theForm);\n");
            writer.Append("return (true);\n");
            writer.Append("}\n");
            writer.Append("</script>\n");

            writer.Append("<div style=\"height:17;padding-top:3px;\" class=\"tablenormal\">" + String.Format(AppLogic.GetString("admin.editquantitydiscounttable.DiscountQuantityTable", SkinID, LocaleSetting),QuantityDiscountName.ToUpperInvariant()) + "</div><br/>\n");

            writer.Append("<form action=\"" + AppLogic.AdminLinkUrl("editquantitydiscounttable.aspx") + "?quantitydiscountid=" + QuantityDiscountID.ToString() + "\" method=\"post\" id=\"ByCountForm\" name=\"ByCountForm\" onsubmit=\"return (validateForm(this) && ByCountForm_Validator(this))\" onReset=\"return confirm('" + AppLogic.GetString("admin.common.ResetAllFieldsPrompt", SkinID, LocaleSetting) + "');\">\n");
            writer.Append("<input type=\"hidden\" name=\"IsSubmitByCount\" value=\"true\">\n");

            writer.Append("<table width=\"100%\" cellpadding=\"4\" cellspacing=\"0\" border=\"1\">\n");
            writer.Append("<tr bgcolor=\"#FFFFDD\"><td colspan=3 align=\"center\"><b>" + AppLogic.GetString("admin.editquantitydiscounttable.OrderQuantity", SkinID, LocaleSetting) + "</b></td><td align=\"center\"><b>" + AppLogic.GetString("admin.editquantitydiscounttable.PercentDiscount", SkinID, LocaleSetting) + "</b></td></tr>\n");
            writer.Append("<tr>\n");
            writer.Append("<td align=\"center\" bgcolor=\"#CCFFFF\"><b>" + AppLogic.GetString("admin.common.Delete", SkinID, LocaleSetting) + "</b></td>\n");
            writer.Append("<td align=\"center\" bgcolor=\"#CCFFFF\"><b>" + AppLogic.GetString("admin.common.Low", SkinID, LocaleSetting) + "</b></td>\n");
            writer.Append("<td align=\"center\" bgcolor=\"#CCFFFF\"><b>" + AppLogic.GetString("admin.common.High", SkinID, LocaleSetting) + "</b></td>\n");
            writer.Append("<td align=\"center\" bgcolor=\"#CCFFFF\"><b>" + AppLogic.GetString("admin.editquantitydiscounttable.DiscountPercentage", SkinID, LocaleSetting) + "</b></td>\n");
            writer.Append("</tr>\n");                       

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("select * from QuantityDiscountTable   with (NOLOCK)  where QuantityDiscountID=" + QuantityDiscountID.ToString() + " order by LowQuantity", dbconn))
                {
                    while (rs.Read())
                    {
                        writer.Append("<tr>\n");
                        writer.Append("<td align=\"center\" bgcolor=\"#CCFFFF\"><input type=\"Button\" name=\"Delete\" value=\"X\" onClick=\"self.location='" + AppLogic.AdminLinkUrl("editquantitydiscounttable.aspx") + "?quantitydiscountid=" + QuantityDiscountID.ToString() + "&deleteByCountid=" + DB.RSFieldInt(rs, "QuantityDiscountTableID").ToString() + "'\"></td>\n");
                        writer.Append("<td align=\"center\" bgcolor=\"#CCFFFF\">\n");
                        writer.Append("<input maxLength=\"10\" size=\"10\" name=\"Low_" + DB.RSFieldInt(rs, "QuantityDiscountTableID").ToString() + "\" value=\"" + DB.RSFieldInt(rs, "LowQuantity").ToString() + "\">\n");
                        writer.Append("<input type=\"hidden\" name=\"Low_" + DB.RSFieldInt(rs, "QuantityDiscountTableID").ToString() + "_vldt\" value=\"[number][blankalert=" + AppLogic.GetString("admin.editquantitydiscounttable.EnterOrderAmount", SkinID, LocaleSetting) + "][invalidalert=" + AppLogic.GetString("admin.common.MoneyValueWithoutDollarSignPrompt", SkinID, LocaleSetting) + "]\">\n");
                        writer.Append("</td>\n");
                        writer.Append("<td align=\"center\" bgcolor=\"#CCFFFF\">\n");
                        writer.Append("<input maxLength=\"10\" size=\"10\" name=\"High_" + DB.RSFieldInt(rs, "QuantityDiscountTableID").ToString() + "\" value=\"" + DB.RSFieldInt(rs, "HighQuantity").ToString() + "\">\n");
                        writer.Append("<input type=\"hidden\" name=\"High_" + DB.RSFieldInt(rs, "QuantityDiscountTableID").ToString() + "_vldt\" value=\"[number][blankalert=" + AppLogic.GetString("admin.common.EndingOrderAmount", SkinID, LocaleSetting) + "][invalidalert=" + AppLogic.GetString("admin.common.MoneyValueWithoutDollarSignPrompt", SkinID, LocaleSetting) + "]\">\n");
                        writer.Append("</td>\n");
                        writer.Append("<td align=\"center\" bgcolor=\"#CCFFFF\">\n");
                        writer.Append("<input maxLength=\"10\" size=\"10\" name=\"Rate_" + DB.RSFieldInt(rs, "QuantityDiscountTableID").ToString() + "\" value=\"" + Localization.CurrencyStringForDBWithoutExchangeRate(DB.RSFieldDecimal(rs, "DiscountPercent")) + "\">\n");
                        writer.Append("<input type=\"hidden\" name=\"Rate_" + DB.RSFieldInt(rs, "QuantityDiscountTableID").ToString() + "_vldt\" value=\"[number][blankalert=" + AppLogic.GetString("admin.editquantitydiscounttable.EnterDiscountPercent", SkinID, LocaleSetting) + "][invalidalert=" + AppLogic.GetString("admin.common.MoneyValueWithoutDollarSignPrompt", SkinID, LocaleSetting) + "]\">\n");
                        writer.Append("</td>\n");
                        writer.Append("</tr>\n");
                    }

                    writer.Append("<tr>\n");
                    writer.Append("<td align=\"center\" bgcolor=\"#CCFFFF\">" + AppLogic.GetString("admin.editquantitydiscounttable.AddNewRowHere", SkinID, LocaleSetting) + ":</td>\n");
                    writer.Append("<td align=\"center\" bgcolor=\"#CCFFFF\">\n");
                    writer.Append("<input maxLength=\"10\" size=\"10\" name=\"Low_0\" value=\"\">\n");
                    writer.Append("<input type=\"hidden\" name=\"Low_0_vldt\" value=\"[int][blankalert=" + AppLogic.GetString("admin.editquantitydiscounttable.EnterOrderQuantity", SkinID, LocaleSetting) + "][invalidalert=" + AppLogic.GetString("admin.editquantitydiscounttable.EnterInteger", SkinID, LocaleSetting) + "]\">\n");
                    writer.Append("</td>\n");
                    writer.Append("<td align=\"center\" bgcolor=\"#CCFFFF\">\n");
                    writer.Append("<input maxLength=\"10\" size=\"10\" name=\"High_0\" value=\"\">\n");
                    writer.Append("<input type=\"hidden\" name=\"High_0_vldt\" value=\"[int][blankalert=" + AppLogic.GetString("admin.editquantitydiscounttable.EndingOrderQuantity", SkinID, LocaleSetting) + "][invalidalert=" + AppLogic.GetString("admin.editquantitydiscounttable.EnterInteger", SkinID, LocaleSetting) + "]\">\n");
                    writer.Append("</td>\n");
                    writer.Append("<td align=\"center\" bgcolor=\"#CCFFFF\">\n");
                    writer.Append("<input maxLength=\"10\" size=\"10\" name=\"Rate_0_" + QuantityDiscountID.ToString() + "\" value=\"\">\n");
                    writer.Append("<input type=\"hidden\" name=\"Rate_0_" + QuantityDiscountID.ToString() + "_vldt\" value=\"[number][blankalert=" + AppLogic.GetString("admin.editquantitydiscounttable.EnterPercentDiscount", SkinID, LocaleSetting) + "][invalidalert=" + AppLogic.GetString("admin.common.MoneyValueWithoutDollarSignPrompt", SkinID, LocaleSetting) + "]\">\n");
                    writer.Append("</td>\n");
                    writer.Append("</tr>\n");
                    writer.Append("</table>\n");
                    writer.Append("<p align=\"left\"><input type=\"submit\" class=\"normalButtons\" value=\"" + AppLogic.GetString("admin.common.Update", SkinID, LocaleSetting) + "\" name=\"submit\"></p>\n");

                    writer.Append("</form>\n");
                }
            }
            ltContent.Text = writer.ToString();
        }
    }
}
