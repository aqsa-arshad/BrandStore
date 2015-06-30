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
    /// Summary description for editcustomerlevel
    /// </summary>
    public partial class editcustomerlevel : AdminPageBase
    {

        int CustomerLevelID;

        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");
            CustomerLevelID = 0;

            if (CommonLogic.QueryStringCanBeDangerousContent("CustomerLevelID").Length != 0 && CommonLogic.QueryStringCanBeDangerousContent("CustomerLevelID") != "0")
            {
                Editing = true;
                CustomerLevelID = Localization.ParseUSInt(CommonLogic.QueryStringCanBeDangerousContent("CustomerLevelID"));
            }
            else
            {
                Editing = false;
            }

            if (CommonLogic.FormBool("IsSubmit"))
            {

                if (ErrorMsg.Length == 0)
                {
                    StringBuilder sql = new StringBuilder(2500);
                    if (!Editing)
                    {
                        // ok to add them:
                        String NewGUID = DB.GetNewGUID();
                        sql.Append("insert into CustomerLevel(CustomerLevelGUID,Name,LevelDiscountPercent,LevelDiscountAmount,LevelHasFreeShipping,LevelAllowsQuantityDiscounts,LevelAllowsPO,LevelHasNoTax,LevelAllowsCoupons,LevelDiscountsApplyToExtendedPrices) values(");
                        sql.Append(DB.SQuote(NewGUID) + ",");
                        sql.Append(DB.SQuote(AppLogic.FormLocaleXml("Name")) + ",");
                        sql.Append(Localization.DecimalStringForDB(CommonLogic.FormUSDecimal("LevelDiscountPercent")) + ",");
                        sql.Append(Localization.CurrencyStringForDBWithoutExchangeRate(CommonLogic.FormUSDecimal("LevelDiscountAmount")) + ",");
                        sql.Append(CommonLogic.FormCanBeDangerousContent("LevelHasFreeShipping") + ",");
                        sql.Append(CommonLogic.FormCanBeDangerousContent("LevelAllowsQuantityDiscounts") + ",");
                        sql.Append(CommonLogic.FormCanBeDangerousContent("LevelAllowsPO") + ",");
                        sql.Append(CommonLogic.FormCanBeDangerousContent("LevelHasNoTax") + ",");
                        sql.Append(CommonLogic.FormCanBeDangerousContent("LevelAllowsCoupons") + ",");
                        sql.Append(CommonLogic.FormCanBeDangerousContent("LevelDiscountsApplyToExtendedPrices"));
                        sql.Append(")");
                        DB.ExecuteSQL(sql.ToString());


                        using (SqlConnection dbconn = DB.dbConn())
                        {
                            dbconn.Open();
                            using (IDataReader rs = DB.GetRS("select CustomerLevelID from CustomerLevel   with (NOLOCK)  where deleted=0 and CustomerLevelGUID=" + DB.SQuote(NewGUID), dbconn))
                            {
                                rs.Read();
                                CustomerLevelID = DB.RSFieldInt(rs, "CustomerLevelID");
                                Editing = true;
                            }
                        }
                        DataUpdated = true;
                        Response.Redirect("customerlevels.aspx", true);
                    }
                    else
                    {
                        // ok to update:
                        sql.Append("update CustomerLevel set ");
                        sql.Append("Name=" + DB.SQuote(AppLogic.FormLocaleXml("Name")) + ",");
                        sql.Append("LevelDiscountPercent=" + CommonLogic.IIF(CommonLogic.FormCanBeDangerousContent("LevelDiscountPercent").Trim() == "", "0", CommonLogic.FormCanBeDangerousContent("LevelDiscountPercent")) + ",");
                        sql.Append("LevelDiscountAmount=" + CommonLogic.IIF(CommonLogic.FormCanBeDangerousContent("LevelDiscountAmount").Trim() == "", "0", CommonLogic.FormCanBeDangerousContent("LevelDiscountAmount")) + ",");
                        sql.Append("LevelHasFreeShipping=" + CommonLogic.FormCanBeDangerousContent("LevelHasFreeShipping") + ",");
                        sql.Append("LevelAllowsQuantityDiscounts=" + CommonLogic.FormCanBeDangerousContent("LevelAllowsQuantityDiscounts") + ",");
                        sql.Append("LevelAllowsPO=" + CommonLogic.FormCanBeDangerousContent("LevelAllowsPO") + ",");
                        sql.Append("LevelHasNoTax=" + CommonLogic.FormCanBeDangerousContent("LevelHasNoTax") + ",");
                        sql.Append("LevelAllowsCoupons=" + CommonLogic.FormCanBeDangerousContent("LevelAllowsCoupons") + ",");
                        sql.Append("LevelDiscountsApplyToExtendedPrices=" + CommonLogic.FormCanBeDangerousContent("LevelDiscountsApplyToExtendedPrices"));
                        sql.Append(" where CustomerLevelID=" + CustomerLevelID.ToString());
                        DB.ExecuteSQL(sql.ToString());
                        DataUpdated = true;
                        Editing = true;
                    }
                }

            }
            SectionTitle = "<a href=\"" + AppLogic.AdminLinkUrl("CustomerLevels.aspx") + "\">CustomerLevels</a> - Manage Customer Levels";
            RenderHtml();
        }

        private void RenderHtml()
        {
            StringBuilder writer = new StringBuilder();
            writer.Append("<script type='text/javascript' src='Scripts/tabs.js'></script>");

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("select * from CustomerLevel   with (NOLOCK)  where CustomerLevelID=" + CustomerLevelID.ToString(), dbconn))
                {
                    if (rs.Read())
                    {
                        Editing = true;
                    }
                    if (DataUpdated)
                    {
                        writer.Append("<p align=\"left\"><b><font color=blue>" + AppLogic.GetString("admin.editCreditCard.Updated", SkinID, LocaleSetting) + "</font></b></p>\n");
                    }


                    if (ErrorMsg.Length != 0)
                    {
                        writer.Append("<p align=\"left\"><b><font color=red>" + ErrorMsg + "</font></b></p>\n");
                    }


                    if (ErrorMsg.Length == 0)
                    {

                        if (Editing)
                        {
                            writer.Append("<p align=\"left\"><b>" + String.Format(AppLogic.GetString("admin.editCustomerLevel.EditingCLevel", SkinID, LocaleSetting), DB.RSFieldByLocale(rs, "Name", LocaleSetting),DB.RSFieldInt(rs, "CustomerLevelID").ToString()) + "</p></b>\n");
                        }
                        else
                        {
                            writer.Append("<p align=\"left\"><b>" + AppLogic.GetString("admin.editCustomerLevel.AddingCLevel", SkinID, LocaleSetting) + "</p></b>\n");
                        }

                        writer.Append("<script type=\"text/javascript\">\n");
                        writer.Append("function Form_Validator(theForm)\n");
                        writer.Append("{\n");
                        writer.Append("submitonce(theForm);\n");
                        writer.Append("return (true);\n");
                        writer.Append("}\n");
                        writer.Append("</script>\n");

                        writer.Append("<p align=\"left\">" + AppLogic.GetString("admin.editCustomerLevel.EnterCLevelInfo", SkinID, LocaleSetting) + "</p>\n");
                        writer.Append("<p align=\"left\"><b><font color=blue>" + AppLogic.GetString("admin.editCustomerLevel.Warning", SkinID, LocaleSetting) + "</font></b></p>\n");
                        writer.Append("<form enctype=\"multipart/form-data\" action=\"" + AppLogic.AdminLinkUrl("editCustomerLevel.aspx") + "?CustomerLevelID=" + CustomerLevelID.ToString() + "&edit=" + Editing.ToString() + "\" method=\"post\" id=\"Form1\" name=\"Form1\" onsubmit=\"return (validateForm(this) && Form_Validator(this))\" onReset=\"return confirm('" + AppLogic.GetString("admin.common.ResetAllFieldsPrompt", SkinID, LocaleSetting) + "');\">\n");
                        writer.Append("<input type=\"hidden\" name=\"IsSubmit\" value=\"true\">\n");
                        writer.Append("<table width=\"100%\" cellpadding=\"4\" cellspacing=\"0\">\n");
                        writer.Append("              <tr valign=\"middle\">\n");
                        writer.Append("                <td width=\"100%\" colspan=\"3\" align=\"left\">\n");
                        writer.Append("                </td>\n");
                        writer.Append("              </tr>\n");
                        writer.Append("              <tr valign=\"middle\">\n");
                        writer.Append("                <td width=\"25%\" align=\"right\" valign=\"middle\">*" + AppLogic.GetString("admin.common.Name", SkinID, LocaleSetting) + ":&nbsp;&nbsp;</td>\n");
                        writer.Append("                <td align=\"left\" valign=\"top\">\n");
                        writer.Append(AppLogic.GetLocaleEntryFields(DB.RSField(rs, "Name"), "Name", false, true, true, "Please enter the customer level name", 100, 18, 0, 0, false));
                        
                        writer.Append("                	</td>\n");
                        writer.Append("                <td align=\"left\" valign=\"top\">\n");
                        writer.Append("<small>" + AppLogic.GetString("admin.editCustomerLevel.Notification", SkinID, LocaleSetting) + "</small>");
                        writer.Append("                	</td>\n");
                        writer.Append("              </tr>\n");

                        writer.Append("              <tr valign=\"top\">\n");
						writer.Append("                <td width=\"25%\" align=\"right\" valign=\"top\">*" + AppLogic.GetString("admin.editCustomerLevel.LevelDiscountPercent", SkinID, LocaleSetting) + ":&nbsp;&nbsp;</td>\n");
                        writer.Append("                <td align=\"left\" valign=\"top\">\n");
                        writer.Append("                	<input maxLength=\"100\" size=\"30\" name=\"LevelDiscountPercent\" value=\"" + CommonLogic.IIF(Editing, Localization.CurrencyStringForDBWithoutExchangeRate(DB.RSFieldDecimal(rs, "LevelDiscountPercent")), "") + "\">\n");
                        
                        writer.Append("                	</td>\n");
                        writer.Append("                <td align=\"left\" valign=\"top\">\n");
                        writer.Append("<small>" + AppLogic.GetString("admin.editCustomerLevel.NotificationValue", SkinID, LocaleSetting) + "</small>");
                        writer.Append("                	</td>\n");
                        writer.Append("              </tr>\n");

                        writer.Append("              <tr valign=\"top\">\n");
                        writer.Append("                <td width=\"25%\" align=\"right\" valign=\"top\">*" + AppLogic.GetString("admin.editCustomerLevel.LevelDiscountAmount", SkinID, LocaleSetting) + "</td>\n");
                        writer.Append("                <td align=\"left\" valign=\"top\">\n");
                        writer.Append("                	<input maxLength=\"100\" size=\"30\" name=\"LevelDiscountAmount\" value=\"" + CommonLogic.IIF(Editing, Localization.CurrencyStringForDBWithoutExchangeRate(DB.RSFieldDecimal(rs, "LevelDiscountAmount")), "") + "\">\n");
                        
                        writer.Append("                	</td>\n");
                        writer.Append("                <td align=\"left\" valign=\"top\">\n");
                        writer.Append("<small>" + AppLogic.GetString("admin.editCustomerLevel.NotificationValues", SkinID, LocaleSetting) + "</small>");
                        writer.Append("                	</td>\n");
                        writer.Append("              </tr>\n");

                        writer.Append("              <tr valign=\"top\">\n");
                        writer.Append("                <td align=\"right\" valign=\"top\">*" + AppLogic.GetString("admin.editCustomerLevel.LevelIncludesFreeShipping", SkinID, LocaleSetting) + "&nbsp;&nbsp;</td>\n");
                        writer.Append("                <td align=\"left\" valign=\"top\">\n");
                        writer.Append(AppLogic.GetString("admin.common.Yes", SkinID, LocaleSetting) + "&nbsp;<INPUT TYPE=\"RADIO\" NAME=\"LevelHasFreeShipping\" value=\"1\" " + CommonLogic.IIF(Editing, CommonLogic.IIF(DB.RSFieldBool(rs, "LevelHasFreeShipping"), " checked ", ""), "") + ">\n");
                        writer.Append(AppLogic.GetString("admin.common.No", SkinID, LocaleSetting) + "&nbsp;<INPUT TYPE=\"RADIO\" NAME=\"LevelHasFreeShipping\" value=\"0\" " + CommonLogic.IIF(Editing, CommonLogic.IIF(DB.RSFieldBool(rs, "LevelHasFreeShipping"), "", " checked "), " checked ") + ">\n");
                        writer.Append("                	</td>\n");
                        writer.Append("                <td align=\"left\" valign=\"top\">\n");
						writer.Append("<small>" + AppLogic.GetString("admin.editCustomerLevel.IfYesFreeShipping", SkinID, LocaleSetting) + "</small>");
                        writer.Append("                </td>\n");
                        writer.Append("              </tr>\n");

                        writer.Append("              <tr valign=\"top\">\n");
                        writer.Append("                <td align=\"right\" valign=\"top\">*" + AppLogic.GetString("admin.editCustomerLevel.LevelAllowsQuantityDiscounts", SkinID, LocaleSetting) + "</td>\n");
                        writer.Append("                <td align=\"left\" valign=\"top\">\n");
                        writer.Append(AppLogic.GetString("admin.common.Yes", SkinID, LocaleSetting) + "&nbsp;<INPUT TYPE=\"RADIO\" NAME=\"LevelAllowsQuantityDiscounts\" value=\"1\" " + CommonLogic.IIF(Editing, CommonLogic.IIF(DB.RSFieldBool(rs, "LevelAllowsQuantityDiscounts"), " checked ", ""), "") + ">\n");
                        writer.Append(AppLogic.GetString("admin.common.No", SkinID, LocaleSetting) + "&nbsp;<INPUT TYPE=\"RADIO\" NAME=\"LevelAllowsQuantityDiscounts\" value=\"0\" " + CommonLogic.IIF(Editing, CommonLogic.IIF(DB.RSFieldBool(rs, "LevelAllowsQuantityDiscounts"), "", " checked "), " checked ") + ">\n");
                        writer.Append("                	</td>\n");
                        writer.Append("                <td align=\"left\" valign=\"top\">\n");
                        writer.Append("<small>" + AppLogic.GetString("admin.editCustomerLevel.IfYesQuantityDiscount", SkinID, LocaleSetting) + "</small>");
                        writer.Append("                </td>\n");
                        writer.Append("              </tr>\n");

                        writer.Append("              <tr valign=\"top\">\n");
                        writer.Append("                <td align=\"right\" valign=\"top\">*Level Allows Purchase Orders:&nbsp;&nbsp;</td>\n");
                        writer.Append("                <td align=\"left\" valign=\"top\">\n");
                        writer.Append("Yes&nbsp;<INPUT TYPE=\"RADIO\" NAME=\"LevelAllowsPO\" value=\"1\" " + CommonLogic.IIF(Editing, CommonLogic.IIF(DB.RSFieldBool(rs, "LevelAllowsPO"), " checked ", ""), "") + ">\n");
                        writer.Append("No&nbsp;<INPUT TYPE=\"RADIO\" NAME=\"LevelAllowsPO\" value=\"0\" " + CommonLogic.IIF(Editing, CommonLogic.IIF(DB.RSFieldBool(rs, "LevelAllowsPO"), "", " checked "), " checked ") + ">\n");
                        writer.Append("                	</td>\n");
                        writer.Append("                <td align=\"left\" valign=\"top\">\n");
                        writer.Append("                </td>\n");
                        writer.Append("              </tr>\n");

                        writer.Append("              <tr valign=\"top\">\n");
                        writer.Append("                <td align=\"right\" valign=\"top\">*" + AppLogic.GetString("admin.editCustomerLevel.LevelNoTaxOnOrders", SkinID, LocaleSetting) + "&nbsp;&nbsp;</td>\n");
                        writer.Append("                <td align=\"left\" valign=\"top\">\n");
                        writer.Append(AppLogic.GetString("admin.common.Yes", SkinID, LocaleSetting) + "&nbsp;<INPUT TYPE=\"RADIO\" NAME=\"LevelHasNoTax\" value=\"1\" " + CommonLogic.IIF(Editing, CommonLogic.IIF(DB.RSFieldBool(rs, "LevelHasNoTax"), " checked ", ""), "") + ">\n");
                        writer.Append(AppLogic.GetString("admin.common.No", SkinID, LocaleSetting) + "&nbsp;<INPUT TYPE=\"RADIO\" NAME=\"LevelHasNoTax\" value=\"0\" " + CommonLogic.IIF(Editing, CommonLogic.IIF(DB.RSFieldBool(rs, "LevelHasNoTax"), "", " checked "), " checked ") + ">\n");
                        writer.Append("                	</td>\n");
                        writer.Append("                <td align=\"left\" valign=\"top\">\n");
                        writer.Append("<small>" + AppLogic.GetString("admin.editCustomerLevel.IfYesCustomerOrders", SkinID, LocaleSetting) + "</small>");
                        writer.Append("                </td>\n");
                        writer.Append("              </tr>\n");

                        writer.Append("              <tr valign=\"top\">\n");
                        writer.Append("                <td align=\"right\" valign=\"top\">*" + AppLogic.GetString("admin.editCustomerLevel.LevelAllowsCouponsOnOrders", SkinID, LocaleSetting) + "&nbsp;&nbsp;</td>\n");
                        writer.Append("                <td align=\"left\" valign=\"top\">\n");
                        writer.Append(AppLogic.GetString("admin.common.Yes", SkinID, LocaleSetting) + "&nbsp;<INPUT TYPE=\"RADIO\" NAME=\"LevelAllowsCoupons\" value=\"1\" " + CommonLogic.IIF(Editing, CommonLogic.IIF(DB.RSFieldBool(rs, "LevelAllowsCoupons"), " checked ", ""), "") + ">\n");
                        writer.Append(AppLogic.GetString("admin.common.No", SkinID, LocaleSetting) + "&nbsp;<INPUT TYPE=\"RADIO\" NAME=\"LevelAllowsCoupons\" value=\"0\" " + CommonLogic.IIF(Editing, CommonLogic.IIF(DB.RSFieldBool(rs, "LevelAllowsCoupons"), "", " checked "), " checked ") + ">\n");
                        writer.Append("                	</td>\n");
                        writer.Append("                <td align=\"left\" valign=\"top\">\n");
                        writer.Append("<small>" + AppLogic.GetString("admin.editCustomerLevel.IfYesCustomerCoupon", SkinID, LocaleSetting) + "</small>");
                        writer.Append("                </td>\n");
                        writer.Append("              </tr>\n");

                        writer.Append("              <tr valign=\"top\">\n");
                        writer.Append("                <td align=\"right\" valign=\"top\">*" + AppLogic.GetString("admin.editCustomerLevel.LevelDiscountExtendedPricing", SkinID, LocaleSetting) + "&nbsp;&nbsp;</td>\n");
                        writer.Append("                <td align=\"left\" valign=\"top\">\n");
                        writer.Append(AppLogic.GetString("admin.common.Yes", SkinID, LocaleSetting) + "&nbsp;<INPUT TYPE=\"RADIO\" NAME=\"LevelDiscountsApplyToExtendedPrices\" value=\"1\" " + CommonLogic.IIF(Editing, CommonLogic.IIF(DB.RSFieldBool(rs, "LevelDiscountsApplyToExtendedPrices"), " checked ", ""), "") + ">\n");
                        writer.Append(AppLogic.GetString("admin.common.No", SkinID, LocaleSetting) + "&nbsp;<INPUT TYPE=\"RADIO\" NAME=\"LevelDiscountsApplyToExtendedPrices\" value=\"0\" " + CommonLogic.IIF(Editing, CommonLogic.IIF(DB.RSFieldBool(rs, "LevelDiscountsApplyToExtendedPrices"), "", " checked "), " checked ") + ">\n");
                        writer.Append("                	</td>\n");
                        writer.Append("                <td align=\"left\" valign=\"top\">\n");
                        writer.Append("<small>" + AppLogic.GetString("admin.editCustomerLevel.IfYesDiscountAmount", SkinID, LocaleSetting) + "</small>");
                        writer.Append("                </td>\n");
                        writer.Append("              </tr>\n");

                        writer.Append("<tr>\n");
                        writer.Append("<td></td><td align=\"left\" valign=\"top\"><br/>\n");
                        if (Editing)
                        {
                            writer.Append("<input class=\"normalButtons\" type=\"submit\" value=\"" + AppLogic.GetString("admin.common.Update", SkinID, LocaleSetting) + "\" name=\"submit\">\n");
                            writer.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<input type=\"reset\" class=\"normalButtons\" value=\"" + AppLogic.GetString("admin.common.Reset", SkinID, LocaleSetting) + "\" name=\"reset\">\n");
                        }
                        else
                        {
                            writer.Append("<input class=\"normalButtons\" type=\"submit\" value=\"" + AppLogic.GetString("admin.common.AddNew", SkinID, LocaleSetting) + "\" name=\"submit\">\n");
                        }
                        writer.Append("        </td>\n");
                        writer.Append("      </tr>\n");
                        writer.Append("  </table>\n");
                        writer.Append("</form>\n");

                    }
                }
            }
            ltContent.Text = writer.ToString();
        }

    }
}
