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
    /// Summary description for rpt_orders.
    /// </summary>
    public partial class rpt_orders : AdminPageBase
    {

        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");
            
            SectionTitle = AppLogic.GetString("admin.sectiontitle.rpt_orders", SkinID, LocaleSetting);
			RenderHtml();
        }

		private void RenderHtml()
        {
            StringBuilder writer = new StringBuilder();
            String StartDate = CommonLogic.FormCanBeDangerousContent("StartDate");
            String EndDate = CommonLogic.FormCanBeDangerousContent("EndDate");
            String AffiliateID = CommonLogic.FormCanBeDangerousContent("AffiliateID");
            String Gender = CommonLogic.FormCanBeDangerousContent("Gender");
            String GroupBy = CommonLogic.FormCanBeDangerousContent("GroupBy");
            String CouponCode = CommonLogic.FormCanBeDangerousContent("CouponCode");
            String PromotionCode = CommonLogic.FormCanBeDangerousContent("PromotionCode");
            String TransactionState = CommonLogic.FormCanBeDangerousContent("TransactionState");
            String TransactionType = CommonLogic.FormCanBeDangerousContent("TransactionType");
            String ProductMatch = CommonLogic.FormCanBeDangerousContent("ProductMatch");
            String ShippingState = CommonLogic.FormCanBeDangerousContent("ShippingState");
            String EasyRange = CommonLogic.FormCanBeDangerousContent("EasyRange");
            String DateType = CommonLogic.FormCanBeDangerousContent("DateType");
            String Day = CommonLogic.FormCanBeDangerousContent("Day");
            String Month = CommonLogic.FormCanBeDangerousContent("Month");
            String Year = CommonLogic.FormCanBeDangerousContent("Year");
            String ReportType = CommonLogic.FormCanBeDangerousContent("ReportType");

            if (StartDate.Length == 0)
            {
                DateTime DefaultDate = DateTime.Today.AddMonths(-1);
                StartDate = Localization.ToThreadCultureShortDateString(DefaultDate);
            }
            if (EndDate.Length == 0)
            {
                EndDate = Localization.ToThreadCultureShortDateString(System.DateTime.Now);
            }

            if (EasyRange.Length == 0)
            {
                EasyRange = "UseDatesAbove";
            }
            if (DateType.Length == 0)
            {
                DateType = "OrderDate";
            }
            if (GroupBy.Length == 0)
            {
                GroupBy = "Day";
            }
            if (ReportType.Length == 0)
            {
                ReportType = "Table";
            }

            // make sure group by matches easyrange:
            switch (EasyRange)
            {
                case "UseDatesAbove":
                    // all options ok
                    break;
                case "Today":
                    GroupBy = "Day";
                    break;
                case "Yesterday":
                    GroupBy = "Day";
                    break;
                case "ThisWeek":
                    GroupBy = "Day";
                    break;
                case "LastWeek":
                    GroupBy = "Day";
                    break;
                case "ThisMonth":
                    if (GroupBy == "Year")
                    {
                        GroupBy = "Day";
                    }
                    break;
                case "LastMonth":
                    if (GroupBy == "Year")
                    {
                        GroupBy = "Day";
                    }
                    break;
                case "ThisYear":
                    break;
                case "LastYear":
                    break;
            }

            writer.Append("  <!-- calendar stylesheet -->\n");
            writer.Append("  <link rel=\"stylesheet\" type=\"text/css\" media=\"all\" href=\"jscalendar/calendar-win2k-cold-1.css\" title=\"win2k-cold-1\" />\n");
            writer.Append("\n");
            writer.Append("  <!-- main calendar program -->\n");
            writer.Append("  <script type=\"text/javascript\" src=\"jscalendar/calendar.js\"></script>\n");
            writer.Append("\n");
            writer.Append("  <!-- language for the calendar -->\n");
            writer.Append("  <script type=\"text/javascript\" src=\"jscalendar/lang/" + Localization.JSCalendarLanguageFile() + "\"></script>\n");
            writer.Append("\n");
            writer.Append("  <!-- the following script defines the Calendar.setup helper function, which makes\n");
            writer.Append("       adding a calendar a matter of 1 or 2 lines of code. -->\n");
            writer.Append("  <script type=\"text/javascript\" src=\"jscalendar/calendar-setup.js\"></script>\n");

            writer.Append("<script type=\"text/javascript\">\n");
            writer.Append("function ReportForm_Validator(theForm)\n");
            writer.Append("{\n");
            writer.Append("submitonce(theForm);\n");
            writer.Append("return (true);\n");
            writer.Append("}\n");
            writer.Append("</script>\n");

            writer.Append("<form method=\"GET\" action=\"" + AppLogic.AdminLinkUrl("rpt_orders.aspx") + "\" id=\"ReportForm\" name=\"ReportForm\" onsubmit=\"return (validateForm(this) && ReportForm_Validator(this))\">");
            writer.Append("  <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\">");
            writer.Append("    <tr class=\"tablenormal\">");
            writer.Append("      <td width=\"25%\" align=\"center\"><b>" + AppLogic.GetString("admin.rpt_customers.DateRange", SkinID, LocaleSetting) + "</b></td>");
            writer.Append("      <td width=\"25%\" align=\"center\"><b>" + AppLogic.GetString("admin.rpt_customers.DateRange", SkinID, LocaleSetting) + "</b></td>");
            writer.Append("      <td width=\"25%\" align=\"center\"><b>" + AppLogic.GetString("admin.orders.ReportType", SkinID, LocaleSetting) + "</b></td>");
            writer.Append("    </tr>");
            writer.Append("    <tr>");
            writer.Append("      <td width=\"25%\" valign=\"top\" align=\"left\" class=\"ordercustomer\">");
            writer.Append("          <table border=\"0\" cellpadding=\"4\" cellspacing=\"0\" width=\"100%\">");
            writer.Append("            <tr>");
            writer.Append("              <td width=\"50%\">Start Date:</td>");
            writer.Append("              <td width=\"50%\"><input type=\"text\" id=\"StartDate\" name=\"StartDate\" size=\"11\" value=\"" + StartDate + "\">&nbsp;<button id=\"f_trigger_s\">" + AppLogic.GetString("admin.rpt_customers.button.ellipses", SkinID, LocaleSetting) + "</button>");
            
            writer.Append("</td>");
            writer.Append("            </tr>");
            writer.Append("            <tr>");
            writer.Append("              <td width=\"50%\">End Date:</td>");
            writer.Append("              <td width=\"50%\"><input type=\"text\" id=\"EndDate\" name=\"EndDate\" size=\"11\" value=\"" + EndDate + "\">&nbsp;<button id=\"f_trigger_e\">" + AppLogic.GetString("admin.rpt_customers.button.ellipses", SkinID, LocaleSetting) + "</button>");
            
            writer.Append("              </td>");
            writer.Append("            </tr>");
            writer.Append("            <tr>");
            writer.Append("              <td width=\"50%\">Use:</td>");
            writer.Append("              <td width=\"50%\"><input type=\"radio\" name=\"DateType\" value=\"OrderDate\" " + CommonLogic.IIF(DateType == "OrderDate" || DateType == "", "checked", "") + ">Order Date&nbsp;&nbsp;<input type=\"radio\" name=\"DateType\" value=\"TransactionDate\" " + CommonLogic.IIF(DateType == "TransactionDate", "checked", "") + ">Transaction Date");
            writer.Append("              </td>");
            writer.Append("            </tr>");
            writer.Append("          </table>");
            writer.Append("          <hr size=\"1\">");
            writer.Append("          <table border=\"0\" cellpadding=\"4\" cellspacing=\"0\" width=\"100%\">");
            writer.Append("            <tr>");
            writer.Append("              <td colspan=\"2\" align=\"center\" width=\"100%\"><input type=\"radio\" value=\"UseDatesAbove\" name=\"EasyRange\" " + CommonLogic.IIF(EasyRange == "UseDatesAbove" || EasyRange == "", "checked", "") + ">" + AppLogic.GetString("admin.common.UseDatesAbove", SkinID, LocaleSetting) + "</td>");
            writer.Append("            </tr>");
            writer.Append("            <tr>");
            writer.Append("              <td width=\"50%\"><input type=\"radio\" value=\"Today\" name=\"EasyRange\" " + CommonLogic.IIF(EasyRange == "Today", "checked", "") + ">" + AppLogic.GetString("admin.common.Today", SkinID, LocaleSetting) + "</td>");
            writer.Append("              <td width=\"50%\"><input type=\"radio\" value=\"Yesterday\" name=\"EasyRange\" " + CommonLogic.IIF(EasyRange == "Yesterday", "checked", "") + ">" + AppLogic.GetString("admin.common.Yesterday", SkinID, LocaleSetting) + "</td>");
            writer.Append("            </tr>");
            writer.Append("            <tr>");
            writer.Append("              <td width=\"50%\"><input type=\"radio\" value=\"ThisWeek\" name=\"EasyRange\" " + CommonLogic.IIF(EasyRange == "ThisWeek", "checked", "") + ">" + AppLogic.GetString("admin.common.ThisWeek", SkinID, LocaleSetting) + "</td>");
            writer.Append("              <td width=\"50%\"><input type=\"radio\" value=\"LastWeek\" name=\"EasyRange\" " + CommonLogic.IIF(EasyRange == "LastWeek", "checked", "") + ">" + AppLogic.GetString("admin.common.LastWeek", SkinID, LocaleSetting) + "</td>");
            writer.Append("            </tr>");
            writer.Append("            <tr>");
            writer.Append("              <td width=\"50%\"><input type=\"radio\" value=\"ThisMonth\" name=\"EasyRange\" " + CommonLogic.IIF(EasyRange == "ThisMonth", "checked", "") + ">" + AppLogic.GetString("admin.common.ThisMonth", SkinID, LocaleSetting) + "</td>");
            writer.Append("              <td width=\"50%\"><input type=\"radio\" value=\"LastMonth\" name=\"EasyRange\" " + CommonLogic.IIF(EasyRange == "LastMonth", "checked", "") + ">" + AppLogic.GetString("admin.common.LastMonth", SkinID, LocaleSetting) + "</td>");
            writer.Append("            </tr>");
            writer.Append("            <tr>");
            writer.Append("              <td width=\"50%\"><input type=\"radio\" value=\"ThisYear\" name=\"EasyRange\" " + CommonLogic.IIF(EasyRange == "ThisYear", "checked", "") + ">" + AppLogic.GetString("admin.common.ThisYear", SkinID, LocaleSetting) + "</td>");
            writer.Append("              <td width=\"50%\"><input type=\"radio\" value=\"LastYear\" name=\"EasyRange\" " + CommonLogic.IIF(EasyRange == "LastYear", "checked", "") + ">" + AppLogic.GetString("admin.common.LastYear", SkinID, LocaleSetting) + "</td>");
            writer.Append("            </tr>");
            writer.Append("          </table>");
            writer.Append("      </td>");
            writer.Append("      <td width=\"25%\" valign=\"top\" align=\"left\">");
            writer.Append("        <table border=\"0\" cellpadding=\"4\" cellspacing=\"0\" width=\"100%\">");
            writer.Append("          <tr>");
            writer.Append("            <td width=\"50%\">" + AppLogic.GetString("admin.order.Affiliate", SkinID, LocaleSetting) + "</td>");
            writer.Append("            <td width=\"50%\"><select size=\"1\" name=\"AffiliateID\">");
            writer.Append("                  <option value=\"-\" " + CommonLogic.IIF(AffiliateID == "" || AffiliateID == "-", "selected", "") + ">-</option>");


            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("select * from affiliate   with (NOLOCK)  where deleted in (0,1) order by displayorder,name", dbconn))
                {
                    while (rs.Read())
                    {
                        writer.Append("<option value=\"" + DB.RSFieldInt(rs, "AffiliateID").ToString() + "\"" + CommonLogic.IIF(AffiliateID == DB.RSFieldInt(rs, "AffiliateID").ToString(), "selected", "") + ">" + DB.RSField(rs, "Name") + "</option>");
                    }
                }
            }
            writer.Append("              </select></td>");
            writer.Append("          </tr>");
            writer.Append("          <tr>");
            writer.Append("            <td width=\"50%\">" + AppLogic.GetString("admin.common.Gender", SkinID, LocaleSetting) + "</td>");
            writer.Append("            <td width=\"50%\"><select size=\"1\" name=\"Gender\">");
            writer.Append("                  <option value=\"-\" " + CommonLogic.IIF(Gender == "" || Gender == "-", "selected", "") + ">-</option>");
            writer.Append("                <option value=\"M\"" + CommonLogic.IIF(Gender == "M", "selected", "") + ">" + AppLogic.GetString("admin.common.Male", SkinID, LocaleSetting) + "</option>");
            writer.Append("                <option value=\"F\"" + CommonLogic.IIF(Gender == "F", "selected", "") + ">" + AppLogic.GetString("admin.common.Female", SkinID, LocaleSetting) + "</option>");
            writer.Append("              </select></td>");
            writer.Append("          </tr>");
            writer.Append("          <tr>");
            writer.Append("            <td width=\"50%\">" + AppLogic.GetString("admin.order.CouponCode", SkinID, LocaleSetting) + "</td>");
            writer.Append("            <td width=\"50%\"><select size=\"1\" name=\"CouponCode\">");
            writer.Append("                  <option value=\"-\" " + CommonLogic.IIF(CouponCode == "" || CouponCode == "-", "selected", "") + ">-</option>");
            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("select * from Coupon   with (NOLOCK)  order by CouponCode", dbconn))
                {
                    while (rs.Read())
                    {
                        writer.Append("<option value=\"" + DB.RSField(rs, "CouponCode").Replace("\"", "").Replace("'", "") + "\"" + CommonLogic.IIF(CouponCode == DB.RSField(rs, "CouponCode"), "selected", "") + ">" + Server.HtmlEncode(DB.RSField(rs, "CouponCode")) + "</option>");
                    }
                }
            }
            writer.Append("              </select></td>");
            writer.Append("          </tr>");



            writer.Append("          <tr>");
            writer.Append("            <td width=\"50%\">" + AppLogic.GetString("admin.order.PromotionCode", SkinID, LocaleSetting) + ":</td>");
            writer.Append("            <td width=\"50%\"><select size=\"1\" name=\"PromotionCode\">");
            writer.Append("                  <option value=\"-\" " + CommonLogic.IIF(PromotionCode == "" || PromotionCode == "-", "selected", "") + ">-</option>");
            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("select id, code from Promotions with (NOLOCK)  order by Code", dbconn))
                {
                    while (rs.Read())
                    {
                        writer.Append("<option value=\"" + DB.RSFieldInt(rs, "id") + "\"" + CommonLogic.IIF(PromotionCode == DB.RSFieldInt(rs, "id").ToString(), "selected", "") + ">" + Server.HtmlEncode(DB.RSField(rs, "Code")) + "</option>");
                    }
                }
            }
            writer.Append("              </select></td>");
            writer.Append("          </tr>");





            writer.Append("          <tr>");
            writer.Append("            <td width=\"50%\">" + AppLogic.GetString("admin.order.ShipToState", SkinID, LocaleSetting) + "</td>");
            writer.Append("            <td width=\"50%\">");
            writer.Append("<select size=\"1\" name=\"ShippingState\">");
            writer.Append("<option value=\"-\">-</option>");
            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select * from state  with (NOLOCK)  order by DisplayOrder,Name", con))
                {
                    while (rs.Read())
                    {
                        writer.Append("<option value=\"" + DB.RSField(rs, "Abbreviation") + "\"" + CommonLogic.IIF(DB.RSField(rs, "Abbreviation") == ShippingState, " selected", String.Empty) + ">" + DB.RSField(rs, "Name") + "</option>");
                    }
                }
            }

            writer.Append("</select>");

            writer.Append("              </td>");
            writer.Append("          </tr>");

            writer.Append("          <tr>");
            writer.Append("            <td width=\"50%\">" + AppLogic.GetString("admin.order.TransactionState", SkinID, LocaleSetting) + "</td>");
            writer.Append("            <td width=\"50%\"><select size=\"1\" name=\"TransactionState\">");
            writer.Append("                <option value=\"-\">" + AppLogic.GetString("admin.common.All", SkinID, LocaleSetting) + "</option>");
            writer.Append("                <option value=\"" + AppLogic.ro_TXStateAuthorized + "\" " + CommonLogic.IIF(TransactionState == AppLogic.ro_TXStateAuthorized, "selected", "") + ">" + AppLogic.ro_TXStateAuthorized + "</option>");
            writer.Append("                <option value=\"" + AppLogic.ro_TXStateCaptured + "\"" + CommonLogic.IIF(TransactionState == "" || TransactionState == "-" || TransactionState == AppLogic.ro_TXStateCaptured, "selected", "") + ">" + AppLogic.ro_TXStateCaptured + "</option>");
            writer.Append("                <option value=\"" + AppLogic.ro_TXStateVoided + "\"" + CommonLogic.IIF(TransactionState == AppLogic.ro_TXStateVoided, "selected", "") + ">" + AppLogic.ro_TXStateVoided + "</option>");
            writer.Append("                <option value=\"" + AppLogic.ro_TXStateForceVoided + "\"" + CommonLogic.IIF(TransactionState == AppLogic.ro_TXStateForceVoided, "selected", "") + ">" + AppLogic.ro_TXStateForceVoided + "</option>");
            writer.Append("                <option value=\"" + AppLogic.ro_TXStateRefunded + "\"" + CommonLogic.IIF(TransactionState == AppLogic.ro_TXStateRefunded, "selected", "") + ">" + AppLogic.ro_TXStateRefunded + "</option>");
            writer.Append("                <option value=\"" + AppLogic.ro_TXStateFraud + "\"" + CommonLogic.IIF(TransactionState == AppLogic.ro_TXStateFraud, "selected", "") + ">" + AppLogic.ro_TXStateFraud + "</option>");
            writer.Append("              </select></td>");
            writer.Append("          </tr>");

            writer.Append("          <tr>");
            writer.Append("            <td width=\"50%\">" + AppLogic.GetString("admin.order.TransactionType", SkinID, LocaleSetting) + "</td>");
            writer.Append("            <td width=\"50%\"><select size=\"1\" name=\"TransactionType\">");
            writer.Append("                <option value=\"-\">" + AppLogic.GetString("admin.common.All", SkinID, LocaleSetting) + "</option>");
            writer.Append("                <option value=\"UNKNOWN\" " + CommonLogic.IIF(TransactionType == "UNKNOWN", "selected", "") + ">" + AppLogic.GetString("admin.order.TransactionTypeUnknown", SkinID, LocaleSetting) + "</option>");
            writer.Append("                <option value=\"CHARGE\" " + CommonLogic.IIF(TransactionType == "CHARGE", "selected", "") + ">" + AppLogic.GetString("admin.order.TransactionTypeCharge", SkinID, LocaleSetting) + "</option>");
            writer.Append("                <option value=\"CREDIT\" " + CommonLogic.IIF(TransactionType == "CREDIT", "selected", "") + ">" + AppLogic.GetString("admin.order.TransactionTypeCredit", SkinID, LocaleSetting) + "</option>");
            writer.Append("                <option value=\"RECURRING_AUTO\" " + CommonLogic.IIF(TransactionType == "RECURRING_AUTO", "selected", "") + ">" + AppLogic.GetString("admin.rpt_orders.RecurringBilling", SkinID, LocaleSetting) + "</option>");
            writer.Append("              </select></td>");
            writer.Append("          </tr>");

            if (AppLogic.NumProductsInDB < 250)
            {
                writer.Append("          <tr>");
                writer.Append("            <td width=\"50%\">" + AppLogic.GetString("admin.rpt_orders.ProductMatch", SkinID, LocaleSetting) + "</td>");
                writer.Append("            <td width=\"50%\"><select size=\"1\" name=\"ProductMatch\">");
                writer.Append("                <option value=\"-\">" + AppLogic.GetString("admin.common.All", SkinID, LocaleSetting) + "</option>");


                using (SqlConnection dbconn = DB.dbConn())
                {
                    dbconn.Open();
                    using (IDataReader rsx = DB.GetRS("select ProductID,Name from Product with (NOLOCK) order by Name,ProductID", dbconn))
                    {
                        while (rsx.Read())
                        {
                            writer.Append("<option value=\"" + DB.RSFieldInt(rsx, "ProductID").ToString() + "\" " + CommonLogic.IIF(ProductMatch == DB.RSFieldInt(rsx, "ProductID").ToString(), "selected", "") + ">" + DB.RSField(rsx, "Name") + "</option>");
                        }
                    }
                }
                writer.Append("              </select></td>");
                writer.Append("          </tr>");
            }

            writer.Append("        </table>");
            writer.Append("        </td>");
            writer.Append("      <td width=\"25%\" valign=\"top\" align=\"left\" class=\"ordercustomer\">");
            writer.Append("        <table border=\"0\" cellpadding=\"4\" cellspacing=\"0\" width=\"100%\">");
            writer.Append("          <tr>");
            writer.Append("            <td width=\"100%\"><b>" + AppLogic.GetString("admin.rpt_customers.GroupDataBy", SkinID, LocaleSetting) + "</b><br/> <input type=\"radio\" value=\"" + AppLogic.GetString("admin.common.Day", SkinID, LocaleSetting) + "\" name=\"GroupBy\"  " + CommonLogic.IIF(GroupBy == "Day" || GroupBy == "", "checked", "") + ">" + AppLogic.GetString("admin.common.Day", SkinID, LocaleSetting) + "</td>");
            writer.Append("          </tr>");
            writer.Append("          <tr>");
            writer.Append("            <td width=\"100%\"> <input type=\"radio\" value=\"" + AppLogic.GetString("admin.common.Month", SkinID, LocaleSetting) + "\" name=\"GroupBy\" " + CommonLogic.IIF(GroupBy == "Month", "checked", "") + ">" + AppLogic.GetString("admin.common.Month", SkinID, LocaleSetting) + "</td>");
            writer.Append("          </tr>");
            writer.Append("          <tr>");
            writer.Append("            <td width=\"100%\"> <input type=\"radio\" value=\"" + AppLogic.GetString("admin.common.Year", SkinID, LocaleSetting) + "\" name=\"GroupBy\" " + CommonLogic.IIF(GroupBy == "Year", "checked", "") + ">" + AppLogic.GetString("admin.common.Year", SkinID, LocaleSetting) + "</td>");
            writer.Append("          </tr>");
            writer.Append("        </table>");

            writer.Append("        </td>");
            writer.Append("    </tr>");
            writer.Append("    <tr>");
            writer.Append("      <td width=\"100%\" valign=\"middle\" align=\"center\" bgcolor=\"#199EE3\" height=\"25px\" colspan=\"3\">");
            writer.Append("        <input type=\"submit\" class=\"normalButtons\" value=\"" + AppLogic.GetString("admin.common.Submit", SkinID, LocaleSetting) + "\" name=\"B1\">&nbsp;<input class=\"normalButtons\" type=\"button\" onClick=\"javascript:self.location='" + AppLogic.AdminLinkUrl("rpt_orders.aspx") + "';\" value=\"Reset\" name=\"B2\">");
            writer.Append("      </td>");
            writer.Append("    </tr>");
            writer.Append("  </table>");

            writer.Append("\n<script type=\"text/javascript\">\n");
            writer.Append("    Calendar.setup({\n");
            writer.Append("        inputField     :    \"StartDate\",      // id of the input field\n");
            writer.Append("        ifFormat       :    \"" + Localization.JSCalendarDateFormatSpec() + "\",       // format of the input field\n");
            writer.Append("        showsTime      :    false,            // will display a time selector\n");
            writer.Append("        button         :    \"f_trigger_s\",   // trigger for the calendar (button ID)\n");
            writer.Append("        singleClick    :    true            // Single-click mode\n");
            writer.Append("    });\n");
            writer.Append("    Calendar.setup({\n");
            writer.Append("        inputField     :    \"EndDate\",      // id of the input field\n");
            writer.Append("        ifFormat       :    \"" + Localization.JSCalendarDateFormatSpec() + "\",       // format of the input field\n");
            writer.Append("        showsTime      :    false,            // will display a time selector\n");
            writer.Append("        button         :    \"f_trigger_e\",   // trigger for the calendar (button ID)\n");
            writer.Append("        singleClick    :    true            // Single-click mode\n");
            writer.Append("    });\n");
            writer.Append("</script>\n");

            DateTime SpecifiedStartDate = System.DateTime.Today.AddDays(-7);
            DateTime SpecifiedEndDate = System.DateTime.Today;
            if (StartDate.Length != 0)
            {
                DateTime dt = Localization.ParseNativeDateTime(StartDate + " 12:00:00.000 AM");
                SpecifiedStartDate = Localization.ParseNativeDateTime(StartDate);
            }
            else
            {
                SpecifiedStartDate = System.DateTime.MinValue; // will get min date returned from either query
            }
            if (EndDate.Length != 0)
            {
                DateTime dt = Localization.ParseNativeDateTime(EndDate + " 11:59:59.999 PM");
                SpecifiedEndDate = Localization.ParseNativeDateTime(EndDate);
            }
            else
            {
                SpecifiedEndDate = System.DateTime.Now;
            }
            //SpecifiedEndDate = SpecifiedEndDate.AddDays(1);

            DateTime ResultingStartDate;
            DateTime ResultingEndDate;

            String DateWhere = DB.GetTimeSpanSql(EasyRange, SpecifiedStartDate, SpecifiedEndDate.AddDays(1), "^", false, out ResultingStartDate, out ResultingEndDate);
            ResultingEndDate = ResultingEndDate.AddDays(-1);

            String WhereClause = DateWhere;

            String Series1Name = String.Empty;
            String Series2Name = String.Empty;

            String SelectFields = String.Empty;
            String GroupByFields = String.Empty;
            String OrderByFields = String.Empty;
            String DateFormat = String.Empty;
            String GroupByIncrement = String.Empty;
            switch (GroupBy)
            {
                case "Day":
                    SelectFields = "datepart(\"dy\",^) as [Day], Year(^) as [Year]";
                    GroupByFields = "Year(^), datepart(\"dy\",^)";
                    OrderByFields = "Year(^) asc, datepart(\"dy\",^) asc";
                    DateFormat = "mm-dd-yyyy";
                    GroupByIncrement = "0";
                    break;
                case "Month":
                    SelectFields = "month(^) as [Month], Year(^) as [Year]";
                    GroupByFields = "Year(^), month(^)";
                    OrderByFields = "Year(^) asc, month(^) asc";
                    DateFormat = "mm-yyyy";
                    GroupByIncrement = "2";
                    break;
                case "Year":
                    SelectFields = "Year(^) as [Year]";
                    GroupByFields = "Year(^)";
                    OrderByFields = "Year(^) asc";
                    DateFormat = "yyyy";
                    GroupByIncrement = "3";
                    break;
            }

            String GeneralWhere = String.Empty;
            if (AffiliateID != "-" && AffiliateID.Length != 0)
            {
                if (GeneralWhere.Length != 0)
                {
                    GeneralWhere += " and ";
                }
                GeneralWhere += "AffiliateID=" + AffiliateID;
            }
            if (Gender != "-" && Gender.Length != 0)
            {
                if (GeneralWhere.Length != 0)
                {
                    GeneralWhere += " and ";
                }
                GeneralWhere += "customerid in (select distinct customerid from customer   with (NOLOCK)  where upper(Gender)=" + DB.SQuote(Gender.ToUpperInvariant()) + ")";
            }
            if (CouponCode != "-" && CouponCode.Length != 0)
            {
                if (GeneralWhere.Length != 0)
                {
                    GeneralWhere += " and ";
                }
                GeneralWhere += "upper(CouponCode)=" + DB.SQuote(CouponCode.ToUpperInvariant());
            }
            if (PromotionCode != "-" && PromotionCode.Length != 0)
            {
                int promotionID;
                if (int.TryParse(PromotionCode, out promotionID))
                {
                    if (GeneralWhere.Length != 0)
                        GeneralWhere += " and";
                    GeneralWhere += " ordernumber in (select orderid from promotionusage where promotionid = " + promotionID + ") ";
                }
            }
            if (ShippingState != "-" && ShippingState.Length != 0)
            {
                if (GeneralWhere.Length != 0)
                {
                    GeneralWhere += " and ";
                }
                GeneralWhere += "upper(ShippingState)=" + DB.SQuote(ShippingState.ToUpperInvariant());
            }
            if (TransactionState.Length != 0 && TransactionState != "-")
            {
                if (GeneralWhere.Length != 0)
                {
                    GeneralWhere += " and ";
                }
                GeneralWhere += "TransactionState=" + DB.SQuote(TransactionState);
            }
            if (TransactionType.Length != 0 && TransactionType != "-")
            {
                if (GeneralWhere.Length != 0)
                {
                    GeneralWhere += " and ";
                }
                AppLogic.TransactionTypeEnum tt = (AppLogic.TransactionTypeEnum)Enum.Parse(typeof(AppLogic.TransactionTypeEnum), TransactionType, true);
                GeneralWhere += "TransactionType=" + (int)tt;
            }
            if (ProductMatch.Length != 0 && ProductMatch != "-")
            {
                if (GeneralWhere.Length != 0)
                {
                    GeneralWhere += " and ";
                }
                GeneralWhere += "OrderNumber in (select ordernumber from orders_shoppingcart where ProductID=" + ProductMatch + ")";
            }
            if (GeneralWhere.Length != 0)
            {
                GeneralWhere = "(" + GeneralWhere + ")";
            }

            String UseDateField = "OrderDate";
            if (DateType == "TransactionDate")
            {
                // use transaction date, matched to type of transaction being requested
                if (TransactionState == AppLogic.ro_TXStateAuthorized)
                {
                    UseDateField = "AuthorizedOn";
                }
                if (TransactionState == AppLogic.ro_TXStateCaptured)
                {
                    UseDateField = "CapturedOn";
                }
                if (TransactionState == AppLogic.ro_TXStateVoided)
                {
                    UseDateField = "VoidedOn";
                }
                if (TransactionState == AppLogic.ro_TXStateRefunded)
                {
                    UseDateField = "RefundedOn";
                }
                if (TransactionState == AppLogic.ro_TXStateFraud)
                {
                    UseDateField = "FraudedOn";
                }
                if (TransactionState == AppLogic.ro_TXStatePending)
                {
                    UseDateField = "OrderDate"; // we have no "pendingon" date
                }
            }

            if (DateWhere.Length != 0)
            {
                String DS1SQL = "select count(OrderNumber) as N, Sum(OrderSubTotal) as SubTotal, Sum(OrderTotal) as Total, Sum(OrderTax) as Tax, Sum(OrderShippingCosts) as Shipping, " + SelectFields + " from orders   with (NOLOCK)  where 1=1 " + CommonLogic.IIF(GeneralWhere.Length != 0, " and " + GeneralWhere, "") + CommonLogic.IIF(WhereClause.Length != 0, " and " + WhereClause, "") + " group by " + GroupByFields + " order by " + OrderByFields;
                DS1SQL = DS1SQL.Replace("^", UseDateField);
                if (AppLogic.AppConfigBool("Admin_ShowReportSQL"))
                {
                    writer.Append("<p align=\"left\">DS1SQL=" + DS1SQL + "</p>\n");
                }

                DataTable dt = new DataTable();
                try
                {
                    using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                    {
                        con.Open();
                        using (IDataReader rs = DB.GetRS(DS1SQL, con))
                        {
                            dt.Load(rs);
                        }
                    }

                    if (dt.Rows.Count == 0)
                    {
                        writer.Append("<p align=\"left\"><b>" + AppLogic.GetString("admin.rpt_orders.NoDataFound", SkinID, LocaleSetting) + "</b></p>\n");
                    }
                    else
                    {
                        int DS1SumN = 0;
                        decimal DS1SumSubTotal = System.Decimal.Zero;
                        decimal DS1SumTotal = System.Decimal.Zero;
                        decimal DS1SumTax = System.Decimal.Zero;
                        decimal DS1SumShipping = System.Decimal.Zero;
                        int DS1NumRecs = dt.Rows.Count;
                        int MaxNumRecs = DS1NumRecs;
                        foreach (DataRow row in dt.Rows)
                        {
                            DS1SumN += DB.RowFieldInt(row, "N");
                            DS1SumSubTotal += DB.RowFieldDecimal(row, "SubTotal");
                            DS1SumTotal += DB.RowFieldDecimal(row, "Total");
                            DS1SumTax += DB.RowFieldDecimal(row, "Tax");
                            DS1SumShipping += DB.RowFieldDecimal(row, "Shipping");
                        }
                        // set range start date, if necessary:

                        DateTime MinCustomerDate = System.DateTime.MinValue;
                        using (SqlConnection dbconn = DB.dbConn())
                        {
                            dbconn.Open();
                            using (IDataReader rsd = DB.GetRS("select min(OrderDate) as ResultingStartDate from orders  with (NOLOCK)  ", dbconn))
                            {
                                if (rsd.Read())
                                {
                                    MinCustomerDate = DB.RSFieldDateTime(rsd, "ResultingStartDate");
                                }
                                else
                                {
                                    MinCustomerDate = new DateTime(2003, 1, 1); // we need SOME value!
                                }
                            }
                        }
                        if (ResultingStartDate == System.DateTime.MinValue)
                        {
                            ResultingStartDate = MinCustomerDate;
                        }
                        if (ResultingStartDate < MinCustomerDate)
                        {
                            ResultingStartDate = MinCustomerDate;
                        }
                        String DateSeries = String.Empty;
                        String DS1ValuesN = String.Empty;
                        String DS1ValuesSubTotal = String.Empty;
                        String DS1ValuesTotal = String.Empty;
                        String DS1ValuesTax = String.Empty;
                        String DS1ValuesShipping = String.Empty;

                        int NumBuckets = 0;
                        // determine how many "buckets" are in the date series:
                        switch (GroupBy)
                        {
                            case "Day":
                                for (DateTime yy = ResultingStartDate; yy <= ResultingEndDate; yy = yy.AddDays(1))
                                {
                                    NumBuckets++;
                                }
                                break;
                            case "Month":
                                for (DateTime yy = new DateTime(ResultingStartDate.Year, ResultingStartDate.Month, 1); yy <= new DateTime(ResultingEndDate.Year, ResultingEndDate.Month, 1); yy = yy.AddMonths(1))
                                {
                                    NumBuckets++;
                                }
                                break;
                            case "Year":
                                for (DateTime yy = new DateTime(ResultingStartDate.Year, 1, 1); yy <= new DateTime(ResultingEndDate.Year, 1, 1); yy = yy.AddYears(1))
                                {
                                    NumBuckets++;
                                }
                                break;
                        }

                        // COMPOSE FULL DATE and RANGE and SUM SERIES:
                        int ds1_idx = 0;
                        int[] SumsN = new int[NumBuckets];
                        decimal[] SumsSubTotal = new decimal[NumBuckets];
                        decimal[] SumsTotal = new decimal[NumBuckets];
                        decimal[] SumsTax = new decimal[NumBuckets];
                        decimal[] SumsShipping = new decimal[NumBuckets];
                        for (int i = SumsN.GetLowerBound(0); i <= SumsN.GetUpperBound(0); i++)
                        {
                            SumsN[i] = 0;
                            SumsSubTotal[i] = System.Decimal.Zero;
                            SumsTotal[i] = System.Decimal.Zero;
                            SumsTax[i] = System.Decimal.Zero;
                            SumsShipping[i] = System.Decimal.Zero;
                        }
                        int SumBucketIdx = 0;
                        switch (GroupBy)
                        {
                            case "Day":
                                for (DateTime yy = ResultingStartDate; yy <= ResultingEndDate; yy = yy.AddDays(1))
                                {
                                    if (DateSeries.Length != 0)
                                    {
                                        DateSeries += "|";
                                        DS1ValuesN += "|";
                                        DS1ValuesSubTotal += "|";
                                        DS1ValuesTotal += "|";
                                        DS1ValuesTax += "|";
                                        DS1ValuesShipping += "|";
                                    }
                                    DateSeries += Localization.ToThreadCultureShortDateString(yy);
                                    if (ds1_idx < DS1NumRecs)
                                    {
                                        DataRow ds1Row = dt.Rows[ds1_idx];
                                        int dy1 = DB.RowFieldInt(ds1Row, "Day");
                                        DateTime dt1 = new DateTime(DB.RowFieldInt(ds1Row, "Year"), 1, 1);
                                        dt1 = dt1.AddDays(dy1 - 1);
                                        if (dt1.Month == yy.Month && dt1.Day == yy.Day && dt1.Year == yy.Year)
                                        {
                                            DS1ValuesN += DB.RowFieldInt(ds1Row, "N").ToString();
                                            DS1ValuesSubTotal += Localization.CurrencyStringForDBWithoutExchangeRate(DB.RowFieldDecimal(ds1Row, "SubTotal"));
                                            DS1ValuesTotal += Localization.CurrencyStringForDBWithoutExchangeRate(DB.RowFieldDecimal(ds1Row, "Total"));
                                            DS1ValuesTax += Localization.CurrencyStringForDBWithoutExchangeRate(DB.RowFieldDecimal(ds1Row, "Tax"));
                                            DS1ValuesShipping += Localization.CurrencyStringForDBWithoutExchangeRate(DB.RowFieldDecimal(ds1Row, "Shipping"));
                                            SumsN[SumBucketIdx] += DB.RowFieldInt(ds1Row, "N");
                                            SumsSubTotal[SumBucketIdx] += DB.RowFieldDecimal(ds1Row, "SubTotal");
                                            SumsTotal[SumBucketIdx] += DB.RowFieldDecimal(ds1Row, "Total");
                                            SumsTax[SumBucketIdx] += DB.RowFieldDecimal(ds1Row, "Tax");
                                            SumsShipping[SumBucketIdx] += DB.RowFieldDecimal(ds1Row, "Shipping");
                                            ds1_idx++;
                                        }
                                        else
                                        {
                                            DS1ValuesN += "0";
                                            DS1ValuesSubTotal += "0.0";
                                            DS1ValuesTotal += "0.0";
                                            DS1ValuesTax += "0.0";
                                            DS1ValuesShipping += "0.0";
                                        }
                                    }
                                    else
                                    {
                                        DS1ValuesN += "0";
                                        DS1ValuesSubTotal += "0.0";
                                        DS1ValuesTotal += "0.0";
                                        DS1ValuesTax += "0.0";
                                        DS1ValuesShipping += "0.0";
                                    }
                                    SumBucketIdx++;
                                }
                                break;
                            case "Month":
                                for (DateTime yy = new DateTime(ResultingStartDate.Year, ResultingStartDate.Month, 1); yy <= new DateTime(ResultingEndDate.Year, ResultingEndDate.Month, 1); yy = yy.AddMonths(1))
                                {
                                    if (DateSeries.Length != 0)
                                    {
                                        DateSeries += "|";
                                        DS1ValuesN += "|";
                                        DS1ValuesSubTotal += "|";
                                        DS1ValuesTotal += "|";
                                        DS1ValuesTax += "|";
                                        DS1ValuesShipping += "|";
                                    }
                                    DateSeries += yy.Month.ToString() + "-" + yy.Year.ToString();
                                    if (ds1_idx < DS1NumRecs)
                                    {
                                        DataRow ds1Row = dt.Rows[ds1_idx];
                                        DateTime dt1 = new DateTime(DB.RowFieldInt(ds1Row, "Year"), DB.RowFieldInt(ds1Row, "Month"), 1);
                                        if (dt1.Month == yy.Month && dt1.Year == yy.Year)
                                        {
                                            DS1ValuesN += DB.RowFieldInt(ds1Row, "N").ToString();
                                            DS1ValuesSubTotal += Localization.CurrencyStringForDBWithoutExchangeRate(DB.RowFieldDecimal(ds1Row, "SubTotal"));
                                            DS1ValuesTotal += Localization.CurrencyStringForDBWithoutExchangeRate(DB.RowFieldDecimal(ds1Row, "Total"));
                                            DS1ValuesTax += Localization.CurrencyStringForDBWithoutExchangeRate(DB.RowFieldDecimal(ds1Row, "Tax"));
                                            DS1ValuesShipping += Localization.CurrencyStringForDBWithoutExchangeRate(DB.RowFieldDecimal(ds1Row, "Shipping"));
                                            SumsN[SumBucketIdx] += DB.RowFieldInt(ds1Row, "N");
                                            SumsSubTotal[SumBucketIdx] += DB.RowFieldDecimal(ds1Row, "SubTotal");
                                            SumsTotal[SumBucketIdx] += DB.RowFieldDecimal(ds1Row, "Total");
                                            SumsTax[SumBucketIdx] += DB.RowFieldDecimal(ds1Row, "Tax");
                                            SumsShipping[SumBucketIdx] += DB.RowFieldDecimal(ds1Row, "Shipping");
                                            ds1_idx++;
                                        }
                                        else
                                        {
                                            DS1ValuesN += "0";
                                            DS1ValuesSubTotal += "0.0";
                                            DS1ValuesTotal += "0.0";
                                            DS1ValuesTax += "0.0";
                                            DS1ValuesShipping += "0.0";
                                        }
                                    }
                                    else
                                    {
                                        DS1ValuesN += "0";
                                        DS1ValuesSubTotal += "0.0";
                                        DS1ValuesTotal += "0.0";
                                        DS1ValuesTax += "0.0";
                                        DS1ValuesShipping += "0.0";
                                    }
                                    SumBucketIdx++;
                                }
                                break;
                            case "Year":
                                for (DateTime yy = new DateTime(ResultingStartDate.Year, 1, 1); yy <= new DateTime(ResultingEndDate.Year, 1, 1); yy = yy.AddYears(1))
                                {
                                    if (DateSeries.Length != 0)
                                    {
                                        DateSeries += "|";
                                        DS1ValuesN += "|";
                                        DS1ValuesSubTotal += "|";
                                        DS1ValuesTotal += "|";
                                        DS1ValuesTax += "|";
                                        DS1ValuesShipping += "|";
                                    }
                                    DateSeries += yy.Year.ToString();
                                    if (ds1_idx < DS1NumRecs)
                                    {
                                        DataRow ds1Row = dt.Rows[ds1_idx];
                                        DateTime dt1 = new DateTime(DB.RowFieldInt(ds1Row, "Year"), 1, 1);
                                        if (dt1.Year == yy.Year)
                                        {
                                            DS1ValuesN += DB.RowFieldInt(ds1Row, "N").ToString();
                                            DS1ValuesSubTotal += Localization.CurrencyStringForDBWithoutExchangeRate(DB.RowFieldDecimal(ds1Row, "SubTotal"));
                                            DS1ValuesTotal += Localization.CurrencyStringForDBWithoutExchangeRate(DB.RowFieldDecimal(ds1Row, "Total"));
                                            DS1ValuesTax += Localization.CurrencyStringForDBWithoutExchangeRate(DB.RowFieldDecimal(ds1Row, "Tax"));
                                            DS1ValuesShipping += Localization.CurrencyStringForDBWithoutExchangeRate(DB.RowFieldDecimal(ds1Row, "Shipping"));
                                            SumsN[SumBucketIdx] += DB.RowFieldInt(ds1Row, "N");
                                            SumsSubTotal[SumBucketIdx] += DB.RowFieldDecimal(ds1Row, "SubTotal");
                                            SumsTotal[SumBucketIdx] += DB.RowFieldDecimal(ds1Row, "Total");
                                            SumsTax[SumBucketIdx] += DB.RowFieldDecimal(ds1Row, "Tax");
                                            SumsShipping[SumBucketIdx] += DB.RowFieldDecimal(ds1Row, "Shipping");
                                            ds1_idx++;
                                        }
                                        else
                                        {
                                            DS1ValuesN += "0";
                                            DS1ValuesSubTotal += "0.0";
                                            DS1ValuesTotal += "0.0";
                                            DS1ValuesTax += "0.0";
                                            DS1ValuesShipping += "0.0";
                                        }
                                    }
                                    else
                                    {
                                        DS1ValuesN += "0";
                                        DS1ValuesSubTotal += "0.0";
                                        DS1ValuesTotal += "0.0";
                                        DS1ValuesTax += "0.0";
                                        DS1ValuesShipping += "0.0";
                                    }
                                    SumBucketIdx++;
                                }
                                break;
                        }

                        writer.Append("<p align=\"left\"><b>" + AppLogic.GetString("admin.rpt_orders.NumberOfOrders", SkinID, LocaleSetting) + " " + DS1SumN.ToString() + "</b></p>\n");
                        if (DS1SumN > 0)
                        {
                            String ReportTitle = AppLogic.GetString("admin.rpt_orders.OrderReport", SkinID, LocaleSetting) + "|" + Localization.ToThreadCultureShortDateString(ResultingStartDate) + " - " + Localization.ToThreadCultureShortDateString(ResultingEndDate) + "|" + AppLogic.GetString("admin.common.GroupBy", SkinID, LocaleSetting) + " " + GroupBy;
                            String ReportTitleN = AppLogic.GetString("admin.rpt_orders.NumberOfOrdersReport", SkinID, LocaleSetting) + "|" + Localization.ToThreadCultureShortDateString(ResultingStartDate) + " - " + Localization.ToThreadCultureShortDateString(ResultingEndDate) + "|" + AppLogic.GetString("admin.common.GroupBy", SkinID, LocaleSetting) + " " + GroupBy;
                            String ReportTitleSubTotal = AppLogic.GetString("admin.rpt_orders.OrderSubTotalReportSum", SkinID, LocaleSetting) + Localization.CurrencyStringForDBWithoutExchangeRate(DS1SumSubTotal) + "|" + Localization.ToThreadCultureShortDateString(ResultingStartDate) + " - " + Localization.ToThreadCultureShortDateString(ResultingEndDate) + "|" + AppLogic.GetString("admin.common.GroupBy", SkinID, LocaleSetting) + " " + GroupBy;
                            String ReportTitleTotal = AppLogic.GetString("admin.rpt_orders.OrderSubTotalReportSum", SkinID, LocaleSetting) + Localization.CurrencyStringForDBWithoutExchangeRate(DS1SumTotal) + "|" + Localization.ToThreadCultureShortDateString(ResultingStartDate) + " - " + Localization.ToThreadCultureShortDateString(ResultingEndDate) + "|" + AppLogic.GetString("admin.common.GroupBy", SkinID, LocaleSetting) + " " + GroupBy;
                            String ReportTitleTax = AppLogic.GetString("admin.rpt_orders.TaxReportSum", SkinID, LocaleSetting) + Localization.CurrencyStringForDBWithoutExchangeRate(DS1SumTax) + "|" + Localization.ToThreadCultureShortDateString(ResultingStartDate) + " - " + Localization.ToThreadCultureShortDateString(ResultingEndDate) + "|" + AppLogic.GetString("admin.common.GroupBy", SkinID, LocaleSetting) + " " + GroupBy;
                            String ReportTitleShipping = AppLogic.GetString("admin.rpt_orders.ShippingReportSum", SkinID, LocaleSetting) + Localization.CurrencyStringForDBWithoutExchangeRate(DS1SumShipping) + "|" + Localization.ToThreadCultureShortDateString(ResultingStartDate) + " - " + Localization.ToThreadCultureShortDateString(ResultingEndDate) + "|" + AppLogic.GetString("admin.common.GroupBy", SkinID, LocaleSetting) + " " + GroupBy;
                            // Append OUT THE TABLE:
                            String[] DD = DateSeries.Split('|');

                            String[] S1N = DS1ValuesN.Split('|');
                            String[] S1SubTotal = DS1ValuesSubTotal.Split('|');
                            String[] S1Total = DS1ValuesTotal.Split('|');
                            String[] S1Tax = DS1ValuesTax.Split('|');
                            String[] S1Shipping = DS1ValuesShipping.Split('|');

                            if (NumBuckets > 60)
                            {
                                // VERTICAL:
                                writer.Append("<p align=\"center\"><b>" + ReportTitle + "</b></p>\n");
                                writer.Append("<table border=\"1\" cellpadding=\"4\" cellspacing=\"0\">\n");
                                writer.Append("  <tr>\n");
                                writer.Append("    <td bgcolor=\"#" + AppLogic.AppConfig("LightCellColor") + "\"><b>" + AppLogic.GetString("admin.common.Date", SkinID, LocaleSetting) + "</b></td>\n");
                                writer.Append("    <td align=\"center\" bgcolor=\"#" + AppLogic.AppConfig("LightCellColor") + "\"><b>" + AppLogic.GetString("admin.common.NumberOfOrders", SkinID, LocaleSetting) + "</b></td>\n");
                                writer.Append("    <td align=\"center\" bgcolor=\"#" + AppLogic.AppConfig("LightCellColor") + "\"><b>" + AppLogic.GetString("admin.rpt_orders.SubTotal", SkinID, LocaleSetting) + "</b></td>\n");
                                writer.Append("    <td align=\"center\" bgcolor=\"#" + AppLogic.AppConfig("LightCellColor") + "\"><b>" + AppLogic.GetString("admin.rpt_orders.Tax", SkinID, LocaleSetting) + "</b></td>\n");
                                writer.Append("    <td align=\"center\" bgcolor=\"#" + AppLogic.AppConfig("LightCellColor") + "\"><b>" + AppLogic.GetString("admin.common.Shipping", SkinID, LocaleSetting) + "</b></td>\n");
                                writer.Append("    <td align=\"center\" bgcolor=\"#" + AppLogic.AppConfig("LightCellColor") + "\"><b>" + AppLogic.GetString("admin.common.Total", SkinID, LocaleSetting) + "</b></td>\n");
                                writer.Append("  </tr>\n");

                                writer.Append("  <tr>\n");
                                writer.Append("    <td bgcolor=\"#" + AppLogic.AppConfig("LightCellColor") + "\"><b>" + AppLogic.GetString("admin.common.Total", SkinID, LocaleSetting) + "</b></td>\n");
                                writer.Append("    <td align=\"center\" bgcolor=\"#" + AppLogic.AppConfig("LightCellColor") + "\"><b>" + DS1SumN.ToString() + "</b></td>\n");
                                writer.Append("    <td align=\"center\" bgcolor=\"#" + AppLogic.AppConfig("LightCellColor") + "\"><b>" + Localization.CurrencyStringForDisplayWithoutExchangeRate(DS1SumSubTotal) + "</b></td>\n");
                                writer.Append("    <td align=\"center\" bgcolor=\"#" + AppLogic.AppConfig("LightCellColor") + "\"><b>" + Localization.CurrencyStringForDisplayWithoutExchangeRate(DS1SumTax) + "</b></td>\n");
                                writer.Append("    <td align=\"center\" bgcolor=\"#" + AppLogic.AppConfig("LightCellColor") + "\"><b>" + Localization.CurrencyStringForDisplayWithoutExchangeRate(DS1SumShipping) + "</b></td>\n");
                                writer.Append("    <td align=\"center\" bgcolor=\"#" + AppLogic.AppConfig("LightCellColor") + "\"><b>" + Localization.CurrencyStringForDisplayWithoutExchangeRate(DS1SumTotal) + "</b></td>\n");
                                writer.Append("  </tr>\n");

                                for (int row = 0; row < NumBuckets; row++)
                                {
                                    writer.Append("  <tr>\n");
                                    writer.Append("    <td>" + DD[row] + "</td>\n");
                                    writer.Append("    <td align=\"center\" >" + CommonLogic.IIF(S1N[row] == "0", "&nbsp;", S1N[row]) + "</td>\n");
                                    writer.Append("    <td align=\"center\" >" + CommonLogic.IIF(S1N[row] == "0", "&nbsp;", S1SubTotal[row]) + "</td>\n");
                                    writer.Append("    <td align=\"center\" >" + CommonLogic.IIF(S1N[row] == "0", "&nbsp;", S1Tax[row]) + "</td>\n");
                                    writer.Append("    <td align=\"center\" >" + CommonLogic.IIF(S1N[row] == "0", "&nbsp;", S1Shipping[row]) + "</td>\n");
                                    writer.Append("    <td align=\"center\" >" + CommonLogic.IIF(S1N[row] == "0", "&nbsp;", S1Total[row]) + "</td>\n");
                                    writer.Append("  </tr>\n");
                                }
                                writer.Append("  <tr>\n");
                                writer.Append("    <td bgcolor=\"#" + AppLogic.AppConfig("LightCellColor") + "\"><b>" + AppLogic.GetString("admin.common.Total", SkinID, LocaleSetting) + "</b></td>\n");
                                writer.Append("    <td align=\"center\" bgcolor=\"#" + AppLogic.AppConfig("LightCellColor") + "\"><b>" + DS1SumN.ToString() + "</b></td>\n");
                                writer.Append("    <td align=\"center\" bgcolor=\"#" + AppLogic.AppConfig("LightCellColor") + "\"><b>" + Localization.CurrencyStringForDisplayWithoutExchangeRate(DS1SumSubTotal) + "</b></td>\n");
                                writer.Append("    <td align=\"center\" bgcolor=\"#" + AppLogic.AppConfig("LightCellColor") + "\"><b>" + Localization.CurrencyStringForDisplayWithoutExchangeRate(DS1SumTax) + "</b></td>\n");
                                writer.Append("    <td align=\"center\" bgcolor=\"#" + AppLogic.AppConfig("LightCellColor") + "\"><b>" + Localization.CurrencyStringForDisplayWithoutExchangeRate(DS1SumShipping) + "</b></td>\n");
                                writer.Append("    <td align=\"center\" bgcolor=\"#" + AppLogic.AppConfig("LightCellColor") + "\"><b>" + Localization.CurrencyStringForDisplayWithoutExchangeRate(DS1SumTotal) + "</b></td>\n");
                                writer.Append("  </tr>\n");
                                writer.Append("</table>\n");
                            }
                            else
                            {
                                // HORIZONTAL:

                                // Number Of Orders Table:
                                writer.Append("<p align=\"center\"><b>" + ReportTitle + "</b></p>\n");
                                writer.Append("<table border=\"1\" cellpadding=\"4\" cellspacing=\"0\">\n");

                                writer.Append("  <tr>\n");
                                writer.Append("    <td bgcolor=\"#" + AppLogic.AppConfig("LightCellColor") + "\">&nbsp;</td>\n");
                                for (int row = 0; row < NumBuckets; row++)
                                {
                                    writer.Append("    <td align=\"center\" bgcolor=\"#" + AppLogic.AppConfig("LightCellColor") + "\"><b>" + DD[row] + "</b></td>\n");
                                }
                                writer.Append("    <td align=\"center\" bgcolor=\"#FFFFCC\"><b>" + AppLogic.GetString("admin.common.Total", SkinID, LocaleSetting) + "</b></td>\n");
                                writer.Append("  </tr>\n");

                                // Number of Orders
                                writer.Append("  <tr>\n");
                                writer.Append("    <td align=\"center\" bgcolor=\"#" + AppLogic.AppConfig("LightCellColor") + "\"><b>" + AppLogic.GetString("admin.common.NumberOfOrders", SkinID, LocaleSetting) + "</b></td>\n");
                                for (int row = 0; row < NumBuckets; row++)
                                {
                                    writer.Append("    <td align=\"center\" >" + CommonLogic.IIF(S1N[row] == "0", "&nbsp;", S1N[row]) + "</td>\n");
                                }
                                writer.Append("    <td align=\"center\" bgcolor=\"#FFFFCC\"><b>" + DS1SumN.ToString() + "</b></td>\n");
                                writer.Append("  </tr>\n");

                                // SubTotals
                                writer.Append("  <tr>\n");
                                writer.Append("    <td align=\"center\" bgcolor=\"#" + AppLogic.AppConfig("LightCellColor") + "\"><b>" + AppLogic.GetString("admin.rpt_orders.SubTotal", SkinID, LocaleSetting) + "</b></td>\n");
                                for (int row = 0; row < NumBuckets; row++)
                                {
                                    writer.Append("    <td align=\"center\" >" + CommonLogic.IIF(S1SubTotal[row] == "0", "&nbsp;", Localization.CurrencyStringForDisplayWithoutExchangeRate(Localization.ParseDBDecimal(S1SubTotal[row]))) + "</td>\n");
                                }
                                writer.Append("    <td align=\"center\" bgcolor=\"#FFFFCC\"><b>" + Localization.CurrencyStringForDisplayWithoutExchangeRate(DS1SumSubTotal) + "</b></td>\n");
                                writer.Append("  </tr>\n");

                                // Tax
                                writer.Append("  <tr>\n");
                                writer.Append("    <td align=\"center\" bgcolor=\"#" + AppLogic.AppConfig("LightCellColor") + "\"><b>" + AppLogic.GetString("admin.rpt_orders.Tax", SkinID, LocaleSetting) + "</b></td>\n");
                                for (int row = 0; row < NumBuckets; row++)
                                {
                                    writer.Append("    <td align=\"center\" >" + CommonLogic.IIF(S1Tax[row] == "0", "&nbsp;", Localization.CurrencyStringForDisplayWithoutExchangeRate(Localization.ParseDBDecimal(S1Tax[row]))) + "</td>\n");
                                }
                                writer.Append("    <td align=\"center\" bgcolor=\"#FFFFCC\"><b>" + Localization.CurrencyStringForDisplayWithoutExchangeRate(DS1SumTax) + "</b></td>\n");
                                writer.Append("  </tr>\n");

                                // Shipping
                                writer.Append("  <tr>\n");
                                writer.Append("    <td align=\"center\" bgcolor=\"#" + AppLogic.AppConfig("LightCellColor") + "\"><b>Shipping</b></td>\n");
                                for (int row = 0; row < NumBuckets; row++)
                                {
                                    writer.Append("    <td align=\"center\" >" + CommonLogic.IIF(S1Shipping[row] == "0", "&nbsp;", Localization.CurrencyStringForDisplayWithoutExchangeRate(Localization.ParseDBDecimal(S1Shipping[row]))) + "</td>\n");
                                }
                                writer.Append("    <td align=\"center\" bgcolor=\"#FFFFCC\"><b>" + Localization.CurrencyStringForDisplayWithoutExchangeRate(DS1SumShipping) + "</b></td>\n");
                                writer.Append("  </tr>\n");

                                // Totals
                                writer.Append("  <tr>\n");
                                writer.Append("    <td align=\"center\" bgcolor=\"#" + AppLogic.AppConfig("LightCellColor") + "\"><b>Totals</b></td>\n");
                                for (int row = 0; row < NumBuckets; row++)
                                {
                                    writer.Append("    <td align=\"center\" >" + CommonLogic.IIF(S1Total[row] == "0", "&nbsp;", Localization.CurrencyStringForDisplayWithoutExchangeRate(Localization.ParseDBDecimal(S1Total[row]))) + "</td>\n");
                                }
                                writer.Append("    <td align=\"center\" bgcolor=\"#FFFFCC\"><b>" + Localization.CurrencyStringForDisplayWithoutExchangeRate(DS1SumTotal) + "</b></td>\n");
                                writer.Append("  </tr>\n");

                                writer.Append("</table>\n");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorMsg = CommonLogic.GetExceptionDetail(ex, "<br/>");
                    writer.Append("<p align=\"left\"><b><font color=\"red\">" + ErrorMsg + "</font></b></p>\n");
                }
                finally
                {
                    dt.Dispose();
                }

            }
            writer.Append("</form>");
            ltContent.Text = writer.ToString();
        }
    }
}
