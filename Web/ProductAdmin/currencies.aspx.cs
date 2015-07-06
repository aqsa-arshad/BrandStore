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
    /// Summary description for currencies.
    /// </summary>
    public partial class currencies : AdminPageBase
    {

        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");
            SectionTitle = AppLogic.GetString("admin.sectiontitle.currencies", SkinID, LocaleSetting);

            if (CommonLogic.QueryStringCanBeDangerousContent("update").Length != 0)
            {
                Currency.GetLiveRates();
                Response.Redirect(AppLogic.AdminLinkUrl("currencies.aspx")); // THROW away any table edits. a live rate check overrides those!
            }
            RenderHtml();
        }

        private void RenderHtml()
        {
            StringBuilder writer = new StringBuilder();
            if (CommonLogic.QueryStringCanBeDangerousContent("DeleteID").Length != 0)
            {
                // handle delete:
                DB.ExecuteSQL("delete from Currency where CurrencyID=" + CommonLogic.QueryStringCanBeDangerousContent("DeleteID"));
            }

            if (CommonLogic.FormCanBeDangerousContent("IsSubmit").Length != 0)
            {
                // handle updates:

                AppLogic.SetAppConfig("Localization.CurrencyFeedUrl", CommonLogic.FormCanBeDangerousContent("CurrencyFeedUrl").Trim());
                AppLogic.SetAppConfig("Localization.CurrencyFeedXmlPackage", CommonLogic.FormCanBeDangerousContent("CurrencyFeedXmlPackage").Trim());
                AppLogic.SetAppConfig("Localization.CurrencyFeedBaseRateCurrencyCode", CommonLogic.FormCanBeDangerousContent("CurrencyFeedBaseRateCurrencyCode").Trim());

                using (SqlConnection dbconn = DB.dbConn())
                {
                    dbconn.Open();
                    using (IDataReader rs = DB.GetRS("Select * from currency with (NOLOCK)", dbconn))
                    {
                        while (rs.Read())
                        {
                            int ID = DB.RSFieldInt(rs, "CurrencyID");
                            String Name = CommonLogic.FormCanBeDangerousContent("Name_" + ID.ToString());
                            String CurrencyCode = CommonLogic.FormCanBeDangerousContent("CurrencyCode_" + ID.ToString());
                            String Symbol = CommonLogic.FormCanBeDangerousContent("Symbol_" + ID.ToString());
                            Decimal ExchangeRate = CommonLogic.FormUSDecimal("ExchangeRate_" + ID.ToString());
                            String DisplayLocaleFormat = CommonLogic.FormCanBeDangerousContent("DisplayLocaleFormat_" + ID.ToString());
                            String DisplaySpec = CommonLogic.FormCanBeDangerousContent("DisplaySpec_" + ID.ToString());
                            bool Published = (CommonLogic.FormCanBeDangerousContent("Published_" + ID.ToString()).Length != 0);
                            int DisplayOrder = CommonLogic.FormUSInt("DisplayOrder_" + ID.ToString());
                            DB.ExecuteSQL("update Currency set Name=" + DB.SQuote(Name) + ", WasLiveRate=0, CurrencyCode=" + DB.SQuote(CurrencyCode) + ", Symbol=" + DB.SQuote(Symbol) + ", ExchangeRate=" + Localization.DecimalStringForDB(ExchangeRate) + ", DisplayLocaleFormat=" + DB.SQuote(DisplayLocaleFormat) + ", DisplaySpec=" + DB.SQuote(DisplaySpec) + ", Published=" + CommonLogic.IIF(Published, "1", "0") + ", DisplayOrder=" + DisplayOrder.ToString() + ", LastUpdated=getdate() where CurrencyID=" + ID.ToString());
                        }
                    }
                }

                // handle new add:
                if (CommonLogic.FormCanBeDangerousContent("Name_0").Trim().Length != 0)
                {
                    String Name = CommonLogic.FormCanBeDangerousContent("Name_0");
                    String CurrencyCode = CommonLogic.FormCanBeDangerousContent("CurrencyCode_0");
                    String Symbol = CommonLogic.FormCanBeDangerousContent("Symbol_0");
                    Decimal ExchangeRate = CommonLogic.FormNativeDecimal("ExchangeRate_0");
                    String DisplayLocaleFormat = CommonLogic.FormCanBeDangerousContent("DisplayLocaleFormat_0");
                    String DisplaySpec = CommonLogic.FormCanBeDangerousContent("DisplaySpec_0");
                    bool Published = (CommonLogic.FormCanBeDangerousContent("Published_0").Length != 0);
                    int DisplayOrder = CommonLogic.FormUSInt("DisplayOrder_0");
                    DB.ExecuteSQL("insert Currency(Name,CurrencyCode,Symbol,ExchangeRate,WasLiveRate,DisplayLocaleFormat,DisplaySpec,Published,DisplayOrder) values(" + DB.SQuote(Name) + "," + DB.SQuote(CurrencyCode) + "," + DB.SQuote(Symbol) + "," + Localization.DecimalStringForDB(ExchangeRate) + ",0," + DB.SQuote(DisplayLocaleFormat) + "," + DB.SQuote(DisplaySpec) + "," + CommonLogic.IIF(Published, "1", "0") + "," + DisplayOrder.ToString() + ")");
                }
            }
            Currency.FlushCache();

            writer.Append("<script type=\"text/javascript\">\n");
            writer.Append("function Form_Validator(theForm)\n");
            writer.Append("{\n");
            writer.Append("submitonce(theForm);\n");
            writer.Append("return (true);\n");
            writer.Append("}\n");
            writer.Append("</script>\n");

            writer.Append("<p align=\"left\"><input type=\"button\" class=\"normalButtons\" value=\"" + AppLogic.GetString("admin.currencies.GetLiveRates", SkinID, LocaleSetting) + "\" onClick=\"javascript:self.location='" + AppLogic.AdminLinkUrl("currencies.aspx") + "?update=true';\"></p>\n");
            writer.Append("<form method=\"POST\" action=\"" + AppLogic.AdminLinkUrl("currencies.aspx") + "\" onsubmit=\"alert('" + AppLogic.GetString("admin.currencies.Notification", SkinID, LocaleSetting) + "');return (validateForm(document.forms[0]) && Form_Validator(document.forms[0]))\" onReset=\"return confirm('" + AppLogic.GetString("admin.common.ResetAllFieldsPrompt", SkinID, LocaleSetting) + "');\">\n");
            writer.Append("<input type=\"hidden\" name=\"IsSubmit\" value=\"true\"/>\n");

            writer.Append("<table>");
            writer.Append("<tr><td style=\"border-style: solid; border-width: 1px;\" width=\"280\"><b>" + String.Format(AppLogic.GetString("admin.currencies.CurrencyFeedUrl", SkinID, LocaleSetting),CommonLogic.IIF(AppLogic.AppConfig("Localization.CurrencyFeedUrl").Length != 0, " (<a href=\"" + AppLogic.AppConfig("Localization.CurrencyFeedUrl") + "\" target=\"_blank\">test</a>)", "")) + "</b></td><td style=\"border-style: solid; border-width: 1px;\" ><input type=\"text\" size=\"60\" id=\"CurrencyFeedUrl\" name=\"CurrencyFeedUrl\" value=\"" + AppLogic.AppConfig("Localization.CurrencyFeedUrl") + "\"></td><td style=\"border-style: solid; border-width: 1px;\" ><small>" + AppLogic.GetString("admin.currencies.EmptyString", SkinID, LocaleSetting) + "</small></td></tr>");
            writer.Append("<tr><td style=\"border-style: solid; border-width: 1px;\" width=\"280\"><b>" + AppLogic.GetString("admin.currencies.CurrencyFeedBaseCurrencyCode", SkinID, LocaleSetting) + "</b></td><td style=\"border-style: solid; border-width: 1px;\" ><input type=\"text\" size=\"3\" id=\"CurrencyFeedBaseRateCurrencyCode\" name=\"CurrencyFeedBaseRateCurrencyCode\" value=\"" + AppLogic.AppConfig("Localization.CurrencyFeedBaseRateCurrencyCode") + "\"></td><td style=\"border-style: solid; border-width: 1px;\" ><small>" + AppLogic.GetString("admin.currencies.CurrencyCodeValidity", SkinID, LocaleSetting) + "</small></td></tr>");
            writer.Append("<tr><td style=\"border-style: solid; border-width: 1px;\" width=\"280\"><b>" + AppLogic.GetString("admin.currencies.CurrencyFeedXmlPackage", SkinID, LocaleSetting) + "</b></td><td style=\"border-style: solid; border-width: 1px;\" ><input type=\"text\" size=\"40\" id=\"CurrencyFeedXmlPackage\" name=\"CurrencyFeedXmlPackage\" value=\"" + AppLogic.AppConfig("Localization.CurrencyFeedXmlPackage") + "\"></td><td style=\"border-style: solid; border-width: 1px;\" ><small>" + AppLogic.GetString("admin.currencies.EmptyString", SkinID, LocaleSetting) + "</small></td></tr>");
            writer.Append("</table>");

            writer.Append("<p align=\"left\">");
            writer.Append("<b>Test Conversion</b> ");
            Decimal SourceAmount = CommonLogic.FormNativeDecimal("SourceAmount");
            if (SourceAmount == System.Decimal.Zero)
            {
                SourceAmount = 1.00M;
            }
            writer.Append(AppLogic.GetString("admin.currencies.Amount", SkinID, LocaleSetting) + " <input type=\"text\" size=\"8\" id=\"SourceAmount\" name=\"SourceAmount\" value=\"" + Localization.CurrencyStringForDBWithoutExchangeRate(SourceAmount) + "\">");
            String SourceCurrency = CommonLogic.FormCanBeDangerousContent("SourceCurrency");
            writer.Append("&nbsp;&nbsp;" + AppLogic.GetString("admin.systemlog.Source", SkinID, LocaleSetting) + " " + Currency.GetSelectList("SourceCurrency", String.Empty, String.Empty, SourceCurrency));
            String TargetCurrency = CommonLogic.FormCanBeDangerousContent("TargetCurrency");
            writer.Append("&nbsp;&nbsp;" + AppLogic.GetString("admin.currencies.Target", SkinID, LocaleSetting) + " " + Currency.GetSelectList("TargetCurrency", String.Empty, String.Empty, TargetCurrency));
            if (SourceCurrency.Length != 0 && TargetCurrency.Length != 0)
            {
                Decimal TargetAmount = Currency.Convert(SourceAmount, SourceCurrency, TargetCurrency);
                writer.Append("&nbsp;&nbsp;" + AppLogic.GetString("admin.currencies.Result", SkinID, LocaleSetting) + " <input type=\"text\" size=\"8\" id=\"TargetAmount\" name=\"TargetAmount\" value=\"" + Currency.ToString(TargetAmount, TargetCurrency) + "\" READONLY/>");
            }
            writer.Append("&nbsp;&nbsp;<input class=\"normalButtons\" type=\"submit\" value=\"" + AppLogic.GetString("admin.currencies.UpdateAndConvert", SkinID, LocaleSetting) + "\" name=\"Submit\"/>");
            writer.Append("</p>");

            writer.Append("<p align=\"left\"><input class=\"normalButtons\" type=\"submit\" value=\"" + AppLogic.GetString("admin.common.Update", SkinID, LocaleSetting) + "\" name=\"Submit\"/></p>\n");

            writer.Append("  <table border=\"0\" cellpadding=\"0\" border=\"0\" cellspacing=\"0\" width=\"100%\">");
            writer.Append("    <tr class=\"table-header\">\n");
            writer.Append("</td>");
            writer.Append("      <td ><b>" + AppLogic.GetString("admin.common.ID", SkinID, LocaleSetting) + "</b></td>\n");
            writer.Append("      <td ><b>*" + AppLogic.GetString("admin.common.Name", SkinID, LocaleSetting) + "</b></td>\n");
            writer.Append("      <td ><b>*" + AppLogic.GetString("admin.currencies.Code", SkinID, LocaleSetting) + "</b></td>\n");
            writer.Append("      <td ><b>" + AppLogic.GetString("admin.currencies.Symbol", SkinID, LocaleSetting) + "</b></td>\n");
            writer.Append("      <td ><b>" + AppLogic.GetString("admin.currencies.ExchangeRate", SkinID, LocaleSetting) + "</b></td>\n");
            writer.Append("      <td ><b>" + AppLogic.GetString("admin.currencies.DisplayLocale", SkinID, LocaleSetting) + "</b></td>\n");
            writer.Append("      <td ><b>" + AppLogic.GetString("admin.currencies.DisplaySpec", SkinID, LocaleSetting) + "</b></td>\n");
            writer.Append("      <td ><b>" + AppLogic.GetString("admin.currencies.Published", SkinID, LocaleSetting) + "</b></td>\n");
            writer.Append("      <td ><b>" + AppLogic.GetString("admin.currencies.LastUpdatedOn", SkinID, LocaleSetting) + "</b></td>\n");
            writer.Append("      <td ><b>*" + AppLogic.GetString("admin.common.DisplayOrder", SkinID, LocaleSetting) + "</b></td>\n");
            writer.Append("      <td align=\"center\"><b>" + AppLogic.GetString("admin.common.Delete", SkinID, LocaleSetting) + "</b></td>\n");
            writer.Append("    </tr>\n");

            string style;
            int counter = 0;

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("select * from Currency  with (NOLOCK)  order by published desc, displayorder,name", dbconn))
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
                        int ID = DB.RSFieldInt(rs, "CurrencyID");
                        writer.Append("<tr class=" + style + ">\n");
                        writer.Append("<td>" + ID.ToString() + "</td>\n");
                        writer.Append("<td><input type=\"text\" size=\"30\" id=\"Name_" + ID.ToString() + "\" name=\"Name_" + ID.ToString() + "\" value=\"" + DB.RSField(rs, "Name").ToString() + "\"/></td>\n");
                        writer.Append("<td><input type=\"text\" size=\"4\" id=\"CurrencyCode_" + ID.ToString() + "\" name=\"CurrencyCode_" + ID.ToString() + "\" value=\"" + DB.RSField(rs, "CurrencyCode").ToString() + "\"/><input type=\"hidden\" id=\"CurrencyCode_" + ID.ToString() + "_vldt\" name=\"CurrencyCode_" + ID.ToString() + "_vldt\" value=\"[req]\"/></td>\n");
                        writer.Append("<td><input type=\"text\" size=\"5\" id=\"Symbol_" + ID.ToString() + "\" name=\"Symbol_" + ID.ToString() + "\" value=\"" + DB.RSField(rs, "Symbol").ToString() + "\"/></td>\n");
                        writer.Append("<td>");
                        String RTX = Localization.DecimalStringForDB(DB.RSFieldDecimal(rs, "ExchangeRate"));
                        if (DB.RSFieldDecimal(rs, "ExchangeRate") == System.Decimal.Zero && DB.RSFieldBool(rs, "Published"))
                        {
                            RTX = String.Empty; // force entry for all published currencies, 0.0 exchange rate is totally invalid!
                        }
                        writer.Append("<input type=\"text\" size=\"6\" id=\"ExchangeRate_" + ID.ToString() + "\" name=\"ExchangeRate_" + ID.ToString() + "\" value=\"" + RTX + "\"/>" + CommonLogic.IIF(DB.RSFieldBool(rs, "WasLiveRate"), " (Live)", ""));
                        writer.Append("<input type=\"hidden\" id=\"ExchangeRate_" + ID.ToString() + "_vldt\" name=\"ExchangeRate_" + ID.ToString() + "_vldt\" value=\"[req][number][blankalert=" + AppLogic.GetString("admin.currencies.EnterExchangeRate", SkinID, LocaleSetting) + "][invalidalert=" + AppLogic.GetString("admin.currencies.ValidDollarAmount", SkinID, LocaleSetting) + "]\"/>");
                        writer.Append("</td>\n");
                        writer.Append("<td><input type=\"text\" id=\"DisplayLocaleFormat_" + ID.ToString() + "\" name=\"DisplayLocaleFormat_" + ID.ToString() + "\" value=\"" + DB.RSField(rs, "DisplayLocaleFormat").ToString() + "\"/></td>\n");
                        writer.Append("<td><input type=\"text\" id=\"DisplaySpec_" + ID.ToString() + "\" name=\"DisplaySpec_" + ID.ToString() + "\" value=\"" + DB.RSField(rs, "DisplaySpec").ToString() + "\"/></td>\n");
                        writer.Append("<td><input type=\"checkbox\" id=\"Published_" + ID.ToString() + "\" name=\"Published_" + ID.ToString() + "\" " + CommonLogic.IIF(DB.RSFieldBool(rs, "Published"), " checked=\"checked\" ", "") + "/></td>\n");
                        writer.Append("<td>" + Localization.ToNativeDateTimeString(DB.RSFieldDateTime(rs, "LastUpdated")) + "</td>\n");
                        writer.Append("<td align=\"center\"><input size=\"2\" type=\"text\" name=\"DisplayOrder_" + ID.ToString() + "\" value=\"" + DB.RSFieldInt(rs, "DisplayOrder").ToString() + "\"/></td>\n");
                        writer.Append("<td align=\"center\"><input class=\"normalButtons\" type=\"button\" value=\"" + AppLogic.GetString("admin.common.Delete", SkinID, LocaleSetting) + "\" name=\"Delete_" + ID.ToString() + "\" onClick=\"DeleteCurrency(" + ID.ToString() + ")\"/></td>\n");
                        writer.Append("</tr>\n");
                        counter++;
                    }
                }
            }

            writer.Append("<tr>\n");
            writer.Append("<td>" + AppLogic.GetString("admin.currencies.AddNew", SkinID, LocaleSetting) + "</td>\n");
            writer.Append("<td><input type=\"text\" size=\"30\" id=\"Name_0\" name=\"Name_0\"/></td>\n");
            writer.Append("<td><input type=\"text\" size=\"4\" id=\"CurrencyCode_0\" name=\"CurrencyCode_0\"/></td>\n");
            writer.Append("<td><input type=\"text\" size=\"5\" id=\"Symbol_0\" name=\"Symbol_0\"/></td>\n");
            writer.Append("<td><input type=\"text\" size=\"6\" id=\"ExchangeRate_0\" name=\"ExchangeRate_0\"/></td>\n");
            writer.Append("<td><input type=\"text\" id=\"DisplayLocaleFormat_0\" name=\"DisplayLocaleFormat_0\"/></td>\n");
            writer.Append("<td><input type=\"text\" id=\"DisplaySpec_0\" name=\"DisplaySpec_0\"/></td>\n");
            writer.Append("<td><input type=\"checkbox\" id=\"Published_0\" name=\"Published_0\"/></td>\n");
            writer.Append("<td>&nbsp;</td>\n");
            writer.Append("<td align=\"center\"><input size=\"2\" type=\"text\" name=\"DisplayOrder_0\"/></td>\n");
            writer.Append("<td align=\"center\">&nbsp;</td>\n");
            writer.Append("</tr>\n");

            writer.Append("</table>\n");
            writer.Append("<p align=\"left\"><input class=\"normalButtons\" type=\"submit\" value=\"" + AppLogic.GetString("admin.currencies.UpdateChangesAbove", SkinID, LocaleSetting) + "\" name=\"Submit\"/></p>\n");
            writer.Append("</form>\n");

            writer.Append("<script type=\"text/javascript\">\n");
            writer.Append("function DeleteCurrency(id)\n");
            writer.Append("{\n");
            writer.Append("if(confirm('" + AppLogic.GetString("admin.currencies.DeleteCurrency", SkinID, LocaleSetting) + " ' + id))\n");
            writer.Append("{\n");
            writer.Append("self.location = '" + AppLogic.AdminLinkUrl("currencies.aspx") + "?deleteid=' + id;\n");
            writer.Append("}\n");
            writer.Append("}\n");
            writer.Append("</SCRIPT>\n");

            writer.Append("<hr size=\"1\">");
            writer.Append("<b>" + AppLogic.GetString("admin.currencies.XmlPackageDoc", SkinID, LocaleSetting) + "</b><br/>");
            writer.Append("<textarea style=\"width: 100%\" rows=\"60\">" + XmlCommon.PrettyPrintXml(Currency.m_LastRatesResponseXml) + "</textarea>");
            writer.Append("<b>" + AppLogic.GetString("admin.currencies.TransformMasterXml", SkinID, LocaleSetting) + "</b><br/>");
            writer.Append("<textarea style=\"width: 100%\" rows=\"60\">" + XmlCommon.PrettyPrintXml(Currency.m_LastRatesTransformedXml) + "</textarea>");
            ltContent.Text = writer.ToString();
        }
    }
}
