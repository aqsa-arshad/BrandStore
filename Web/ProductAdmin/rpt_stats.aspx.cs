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
    /// Summary description for rpt_stats.
    /// </summary>
    public partial class rpt_stats : AdminPageBase
    {

        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            SectionTitle = "Reports - Summary Stats";
            RenderHtml();
        }

        private void RenderHtml()
        {
            StringBuilder writer = new StringBuilder();
            String StartDate = CommonLogic.FormCanBeDangerousContent("StartDate");
            String EndDate = CommonLogic.FormCanBeDangerousContent("EndDate");
            String EasyRange = CommonLogic.FormCanBeDangerousContent("EasyRange");
            String Day = CommonLogic.FormCanBeDangerousContent("Day");
            String Month = CommonLogic.FormCanBeDangerousContent("Month");
            String Year = CommonLogic.FormCanBeDangerousContent("Year");
            String AffiliateID = CommonLogic.FormCanBeDangerousContent("AffiliateID");

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

            writer.Append("<div align=\"left\">\n");
            writer.Append("<form method=\"GET\" action=\"" + AppLogic.AdminLinkUrl("rpt_stats.aspx") + "\" id=\"ReportForm\" name=\"ReportForm\" onsubmit=\"return (validateForm(this) && ReportForm_Validator(this))\">");
            writer.Append("  <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\">");
            writer.Append("    <tr>");
            writer.Append("      <td width=\"50%\" align=\"center\" class=\"tablenormal\"><b>Date Range:</b></td>");
            writer.Append("      <td width=\"50%\" align=\"center\" class=\"tablenormal\"><b>Qualifiers:</b></td>");
            writer.Append("    </tr>");
            writer.Append("    <tr>");
            writer.Append("      <td width=\"50%\" valign=\"top\" align=\"left\" class=\"ordercustomer\">");
            writer.Append("          <table border=\"0\" cellpadding=\"4\" cellspacing=\"0\" width=\"100%\">");
            writer.Append("            <tr>");
            writer.Append("              <td width=\"50%\">Start Date:</td>");
            writer.Append("              <td width=\"50%\"><input type=\"text\" id=\"StartDate\" name=\"StartDate\" size=\"11\" value=\"" + StartDate + "\">&nbsp;<button id=\"f_trigger_s\">...</button>");
           
            writer.Append("</td>");
            writer.Append("            </tr>");
            writer.Append("            <tr>");
            writer.Append("              <td width=\"50%\">End Date:</td>");
            writer.Append("              <td width=\"50%\"><input type=\"text\" id=\"EndDate\" name=\"EndDate\" size=\"11\" value=\"" + EndDate + "\">&nbsp;<button id=\"f_trigger_e\">...</button>");
           
            writer.Append("              </td>");
            writer.Append("            </tr>");
            writer.Append("          </table>");
            writer.Append("          <hr size=\"1\">");
            writer.Append("          <table border=\"0\" cellpadding=\"4\" cellspacing=\"0\" width=\"100%\">");
            writer.Append("            <tr>");
            writer.Append("              <td colspan=\"2\" align=\"center\" width=\"100%\"><input type=\"radio\" value=\"UseDatesAbove\" name=\"EasyRange\" " + CommonLogic.IIF(EasyRange == "UseDatesAbove" || EasyRange == "", "checked", "") + ">Use Dates Above</td>");
            writer.Append("            </tr>");
            writer.Append("            <tr>");
            writer.Append("              <td width=\"50%\"><input type=\"radio\" value=\"Today\" name=\"EasyRange\" " + CommonLogic.IIF(EasyRange == "Today", "checked", "") + ">Today</td>");
            writer.Append("              <td width=\"50%\"><input type=\"radio\" value=\"Yesterday\" name=\"EasyRange\" " + CommonLogic.IIF(EasyRange == "Yesterday", "checked", "") + ">Yesterday</td>");
            writer.Append("            </tr>");
            writer.Append("            <tr>");
            writer.Append("              <td width=\"50%\"><input type=\"radio\" value=\"ThisWeek\" name=\"EasyRange\" " + CommonLogic.IIF(EasyRange == "ThisWeek", "checked", "") + ">This Week</td>");
            writer.Append("              <td width=\"50%\"><input type=\"radio\" value=\"LastWeek\" name=\"EasyRange\" " + CommonLogic.IIF(EasyRange == "LastWeek", "checked", "") + ">Last Week</td>");
            writer.Append("            </tr>");
            writer.Append("            <tr>");
            writer.Append("              <td width=\"50%\"><input type=\"radio\" value=\"ThisMonth\" name=\"EasyRange\" " + CommonLogic.IIF(EasyRange == "ThisMonth", "checked", "") + ">This Month</td>");
            writer.Append("              <td width=\"50%\"><input type=\"radio\" value=\"LastMonth\" name=\"EasyRange\" " + CommonLogic.IIF(EasyRange == "LastMonth", "checked", "") + ">Last Month</td>");
            writer.Append("            </tr>");
            writer.Append("            <tr>");
            writer.Append("              <td width=\"50%\"><input type=\"radio\" value=\"ThisYear\" name=\"EasyRange\" " + CommonLogic.IIF(EasyRange == "ThisYear", "checked", "") + ">This Year</td>");
            writer.Append("              <td width=\"50%\"><input type=\"radio\" value=\"LastYear\" name=\"EasyRange\" " + CommonLogic.IIF(EasyRange == "LastYear", "checked", "") + ">Last Year</td>");
            writer.Append("            </tr>");
            writer.Append("          </table>");

            writer.Append("      </td>");
            writer.Append("      <td width=\"50%\" valign=\"top\" align=\"left\" class=\"ordercustomer\">");
            writer.Append("        <table border=\"0\" cellpadding=\"4\" cellspacing=\"0\" width=\"100%\">");

            writer.Append("          <tr>");
            writer.Append("            <td width=\"50%\">Affiliate:</td>");
            writer.Append("            <td width=\"50%\"><select size=\"1\" name=\"AffiliateID\">");
            writer.Append("                  <option value=\"-\" " + CommonLogic.IIF(AffiliateID == "" || AffiliateID == "-", "selected", "") + ">-</option>");

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select * from affiliate   with (NOLOCK)  where deleted in (0,1) order by displayorder,name", con))
                {
                    while (rs.Read())
                    {
                        writer.Append("<option value=\"" + DB.RSFieldInt(rs, "AffiliateID").ToString() + "\"" + CommonLogic.IIF(AffiliateID == DB.RSFieldInt(rs, "AffiliateID").ToString(), "selected", "") + ">" + DB.RSField(rs, "Name") + "</option>");
                    }
                }
            }

            writer.Append("              </select></td>");
            writer.Append("          </tr>");
            writer.Append("</table>");
            writer.Append("</td>");
            writer.Append("    </tr>");
            writer.Append("    <tr>");
            writer.Append("      <td style=\"border-top:solid 2px #1B427D; background-color:#ffffff\" width=\"100%\" valign=\"top\" align=\"center\" bgColor=\"#DFECFF\" colspan=\"2\">");
            writer.Append("        <input type=\"submit\" class=\"normalButtons\" value=\"Submit\" name=\"B1\"><input type=\"button\" class=\"normalButtons\" onClick=\"javascript:self.location='" + AppLogic.AdminLinkUrl("rpt_stats.aspx") + "';\" value=\"Reset\" name=\"B2\">");
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

            DateTime RangeStartDate = System.DateTime.MinValue;
            DateTime RangeEndDate = System.DateTime.MaxValue;

            String DateWhere = String.Empty;
            switch (EasyRange)
            {
                case "UseDatesAbove":
                    if (StartDate.Length != 0)
                    {
                        DateTime dt = Localization.ParseNativeDateTime(StartDate + " 12:00:00.000 AM");
                        DateWhere = " OrderDate>=" + DB.DateQuote(Localization.ToDBDateTimeString(dt));
                        RangeStartDate = Localization.ParseNativeDateTime(StartDate);
                    }
                    else
                    {
                        RangeStartDate = System.DateTime.MinValue; // will get min date returned from either query
                    }
                    if (EndDate.Length != 0)
                    {
                        DateTime dt = Localization.ParseNativeDateTime(EndDate + " 11:59:59.999 PM");
                        DateWhere += CommonLogic.IIF(DateWhere.Length != 0, " and ", "") + "OrderDate <=" + DB.DateQuote(Localization.ToDBDateTimeString(dt));
                        RangeEndDate = Localization.ParseNativeDateTime(EndDate);
                    }
                    else
                    {
                        RangeEndDate = System.DateTime.Now;
                    }
                    break;
                case "UseDatesBelow":
                    if (Day.Length != 0 && Day != "0")
                    {
                        DateWhere = " day(OrderDate)=" + Day + " ";
                    }
                    else
                        if (Month.Length != 0 && Month != "0")
                        {
                            if (DateWhere.Length != 0)
                            {
                                DateWhere += " and ";
                            }
                            DateWhere += " month(OrderDate)=" + Month + " ";
                        }
                    if (Year.Length != 0 && Year != "0")
                    {
                        if (DateWhere.Length != 0)
                        {
                            DateWhere += " and ";
                        }
                        DateWhere += " year(OrderDate)=" + Year + " ";
                    };
                    String DaySpec = CommonLogic.IIF(Day.Length == 0 || Day == "0", "1", Day);
                    String MonthSpec = CommonLogic.IIF(Month.Length == 0 || Month == "0", "1", Month);
                    String YearSpec = CommonLogic.IIF(Year.Length == 0 || Year == "0", System.DateTime.Now.Year.ToString(), Year.ToString());
                    RangeStartDate = Localization.ParseNativeDateTime(MonthSpec + "/" + DaySpec + "/" + YearSpec);
                    RangeEndDate = RangeStartDate;
                    break;
                case "Today":
                    DateWhere = "day(OrderDate)=" + System.DateTime.Now.Day.ToString() + " and month(OrderDate)=" + System.DateTime.Now.Month.ToString() + " and year(OrderDate)=" + System.DateTime.Now.Year.ToString();
                    RangeStartDate = System.DateTime.Now;
                    RangeEndDate = System.DateTime.Now;
                    break;
                case "Yesterday":
                    DateWhere = "day(OrderDate)=" + System.DateTime.Now.AddDays(-1).Day.ToString() + " and month(OrderDate)=" + System.DateTime.Now.AddDays(-1).Month.ToString() + " and year(OrderDate)=" + System.DateTime.Now.AddDays(-1).Year.ToString();
                    RangeStartDate = System.DateTime.Now.AddDays(-1);
                    RangeEndDate = System.DateTime.Now.AddDays(-1);
                    break;
                case "ThisWeek":
                    int DayOfWeek = (int)System.DateTime.Now.DayOfWeek;
                    System.DateTime weekstart = System.DateTime.Now.AddDays(-(DayOfWeek));
                    System.DateTime weekend = weekstart.AddDays(6);
                    int weekstartday = weekstart.DayOfYear;
                    int weekendday = weekend.DayOfYear;
                    DateWhere = "year(OrderDate)=" + System.DateTime.Now.Year.ToString() + " and (datepart(\"dy\",OrderDate)>=" + weekstartday.ToString() + " and datepart(\"dy\",OrderDate)<=" + weekendday.ToString() + ")";
                    RangeStartDate = weekstart;
                    RangeEndDate = weekend;
                    break;
                case "LastWeek":
                    int DayOfWeek2 = (int)System.DateTime.Now.DayOfWeek;
                    System.DateTime weekstart2 = System.DateTime.Now.AddDays(-(DayOfWeek2)).AddDays(-7);
                    System.DateTime weekend2 = weekstart2.AddDays(6);
                    int weekstartday2 = weekstart2.DayOfYear;
                    int weekendday2 = weekend2.DayOfYear;
                    DateWhere = "year(OrderDate)=" + System.DateTime.Now.Year.ToString() + " and (datepart(\"dy\",OrderDate)>=" + weekstartday2.ToString() + " and datepart(\"dy\",OrderDate)<=" + weekendday2.ToString() + ")";
                    RangeStartDate = weekstart2;
                    RangeEndDate = weekend2;
                    break;
                case "ThisMonth":
                    DateWhere = "month(OrderDate)=" + System.DateTime.Now.Month.ToString() + " and year(OrderDate)=" + System.DateTime.Now.Year.ToString();
                    RangeStartDate = Localization.ParseNativeDateTime(System.DateTime.Now.Month.ToString() + "/1/" + System.DateTime.Now.Year.ToString());
                    RangeEndDate = RangeStartDate.AddMonths(1).AddDays(-1);
                    break;
                case "LastMonth":
                    DateWhere = "month(OrderDate)=" + System.DateTime.Now.AddMonths(-1).Month.ToString() + " and year(OrderDate)=" + System.DateTime.Now.AddMonths(-1).Year.ToString();
                    RangeStartDate = Localization.ParseNativeDateTime(System.DateTime.Now.AddMonths(-1).Month.ToString() + "/1/" + System.DateTime.Now.AddMonths(-1).Year.ToString());
                    RangeEndDate = RangeStartDate.AddMonths(1).AddDays(-1);
                    break;
                case "ThisYear":
                    DateWhere = "year(OrderDate)=" + System.DateTime.Now.Year.ToString();
                    RangeStartDate = Localization.ParseUSDateTime("1/1/" + System.DateTime.Now.Year.ToString());
                    RangeEndDate = RangeStartDate.AddYears(1).AddDays(-1);
                    if (RangeEndDate > System.DateTime.Now)
                    {
                        RangeEndDate = System.DateTime.Now;
                    }
                    break;
                case "LastYear":
                    DateWhere = "year(OrderDate)=" + System.DateTime.Now.AddYears(-1).Year.ToString();
                    RangeStartDate = Localization.ParseUSDateTime("1/1/" + System.DateTime.Now.AddYears(-1).Year.ToString());
                    RangeEndDate = RangeStartDate.AddYears(1).AddDays(-1);
                    break;
            }
            if (DateWhere.Length != 0)
            {
                DateWhere = "(" + DateWhere + ")";
            }

            String WhereClause = DateWhere;
            if (WhereClause.Length == 0)
            {
                WhereClause = "1=1";
            }

            if (DateWhere.Length != 0)
            {
                if (AffiliateID == "-")
                {
                    AffiliateID = String.Empty;
                }
                int NumCustomers = DB.GetSqlN("select count(CustomerID) as N from customer   with (NOLOCK)  where " + WhereClause.Replace("OrderDate", "CreatedOn") + CommonLogic.IIF(AffiliateID.Length != 0, " and affiliateid=" + AffiliateID, ""));
                int NumAnonCustomers = DB.GetSqlN("select count(CustomerID) as N from customer   with (NOLOCK)  where EMail = '' and " + WhereClause.Replace("OrderDate", "CreatedOn") + CommonLogic.IIF(AffiliateID.Length != 0, " and affiliateid=" + AffiliateID, ""));
                int NumRegCustomers = DB.GetSqlN("select count(CustomerID) as N from customer   with (NOLOCK)  where EMail <> '' and " + WhereClause.Replace("OrderDate", "CreatedOn") + CommonLogic.IIF(AffiliateID.Length != 0, " and affiliateid=" + AffiliateID, ""));
                int NumOrderCustomers = DB.GetSqlN("select count(CustomerID) as N from customer   with (NOLOCK)  where EMail <> '' and customerid in (select distinct customerid from orders  with (NOLOCK)  ) and " + WhereClause.Replace("OrderDate", "CreatedOn") + CommonLogic.IIF(AffiliateID.Length != 0, " and affiliateid=" + AffiliateID, ""));
                int OrderN = DB.GetSqlN("select count(ordernumber) as N from orders   with (NOLOCK)  where " + WhereClause + CommonLogic.IIF(AffiliateID.Length != 0, " and affiliateid=" + AffiliateID, ""));
                decimal OrderTotal = DB.GetSqlNDecimal("select sum(ordertotal) as N from orders   with (NOLOCK)  where TransactionState in (" + DB.SQuote(AppLogic.ro_TXStateAuthorized) + "," + DB.SQuote(AppLogic.ro_TXStateCaptured) + ") and " + WhereClause + CommonLogic.IIF(AffiliateID.Length != 0, " and affiliateid=" + AffiliateID, ""));
                decimal OrderSubTotal = DB.GetSqlNDecimal("select sum(ordersubtotal) as N from orders   with (NOLOCK)  where TransactionState in (" + DB.SQuote(AppLogic.ro_TXStateAuthorized) + "," + DB.SQuote(AppLogic.ro_TXStateCaptured) + ") and " + WhereClause + CommonLogic.IIF(AffiliateID.Length != 0, " and affiliateid=" + AffiliateID, ""));
                decimal OrderTax = DB.GetSqlNDecimal("select sum(ordertax) as N from orders   with (NOLOCK)  where TransactionState in (" + DB.SQuote(AppLogic.ro_TXStateAuthorized) + "," + DB.SQuote(AppLogic.ro_TXStateCaptured) + ") and " + WhereClause + CommonLogic.IIF(AffiliateID.Length != 0, " and affiliateid=" + AffiliateID, ""));
                decimal OrderShipping = DB.GetSqlNDecimal("select sum(ordershippingcosts) as N from orders   with (NOLOCK)  where TransactionState in (" + DB.SQuote(AppLogic.ro_TXStateAuthorized) + "," + DB.SQuote(AppLogic.ro_TXStateCaptured) + ") and " + WhereClause + CommonLogic.IIF(AffiliateID.Length != 0, " and affiliateid=" + AffiliateID, ""));
                decimal OrderAvg = DB.GetSqlNDecimal("select avg(ordertotal) as N from orders   with (NOLOCK)  where TransactionState in (" + DB.SQuote(AppLogic.ro_TXStateAuthorized) + "," + DB.SQuote(AppLogic.ro_TXStateCaptured) + ") and " + WhereClause + CommonLogic.IIF(AffiliateID.Length != 0, " and affiliateid=" + AffiliateID, ""));
                decimal RevenuePerRegisteredCustomer = System.Decimal.Zero;
                if (NumRegCustomers > 0)
                {
                    RevenuePerRegisteredCustomer = (OrderTotal / NumRegCustomers);
                }
                decimal RevenuePerOrderingCustomer = System.Decimal.Zero;
                if (NumOrderCustomers > 0)
                {
                    RevenuePerOrderingCustomer = (OrderTotal / NumOrderCustomers);
                }

                writer.Append("<table width=\"50%\" cellpadding=\"4\" cellspacing=\"0\" border=\"0\" class=\"ordercustomer\">\n");
                writer.Append("<tr class=\"tablenormal\"><td align=\"left\"><b>Statistic</b></td><td align=\"center\"><b>Value</b></td></tr>\n");
                writer.Append("<tr><td align=\"left\"># of Anons</td><td align=\"center\">" + NumAnonCustomers.ToString() + "</td></tr>\n");
                writer.Append("<tr><td align=\"left\"># of Registered Customers</td><td align=\"center\">" + NumRegCustomers.ToString() + "</td></tr>\n");
                writer.Append("<tr><td align=\"left\"># of Registered Customers That Ordered</td><td align=\"center\">" + NumOrderCustomers.ToString() + "</td></tr>\n");
                if (NumRegCustomers > 0)
                {
                    writer.Append("<tr><td align=\"left\">Percentage of Registered Customers Who Ordered</td><td align=\"center\">" + String.Format("{0:0.00}", (((Decimal)NumOrderCustomers / (Decimal)NumRegCustomers) * 100)) + "%</td></tr>\n");
                }
                writer.Append("<tr><td align=\"left\"># Orders</td><td align=\"center\">" + OrderN.ToString() + "</td></tr>\n");
                writer.Append("<tr><td align=\"left\">Order Total</td><td align=\"center\">" + Localization.CurrencyStringForDBWithoutExchangeRate(OrderTotal) + "</td></tr>\n");
                writer.Append("<tr><td align=\"left\">Order SubTotal</td><td align=\"center\">" + Localization.CurrencyStringForDBWithoutExchangeRate(OrderSubTotal) + "</td></tr>\n");
                writer.Append("<tr><td align=\"left\">Order Tax</td><td align=\"center\">" + Localization.CurrencyStringForDBWithoutExchangeRate(OrderTax) + "</td></tr>\n");
                writer.Append("<tr><td align=\"left\">Order Shipping</td><td align=\"center\">" + Localization.CurrencyStringForDBWithoutExchangeRate(OrderShipping) + "</td></tr>\n");
                writer.Append("<tr><td align=\"left\">Average Order Size</td><td align=\"center\">" + Localization.CurrencyStringForDBWithoutExchangeRate(OrderAvg) + "</td></tr>\n");
                writer.Append("<tr><td align=\"left\">Revenue Per Registering Customer</td><td align=\"center\">" + Localization.CurrencyStringForDBWithoutExchangeRate(RevenuePerRegisteredCustomer) + "</td></tr>\n");
                writer.Append("<tr><td align=\"left\">Revenue Per Ordering Customer</td><td align=\"center\">" + Localization.CurrencyStringForDBWithoutExchangeRate(RevenuePerOrderingCustomer) + "</td></tr>\n");
                writer.Append("    <tr>");
                writer.Append("      <td valign=\"top\" align=\"center\" height=\"16px\" bgColor=\"#DFECFF\" colspan=\"2\">");
                writer.Append("    </td>");
                writer.Append("    </tr>");
                writer.Append("</table>\n");
            }
            writer.Append("</div>\n");
            writer.Append("</form>");
            ltContent.Text = writer.ToString();
        }

    }
}
