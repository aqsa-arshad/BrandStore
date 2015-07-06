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
    /// Summary description for rpt_EMails.
    /// </summary>
    public partial class rpt_EMails : AdminPageBase
    {

        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            SectionTitle = "Reports - Customer E-Mails";
            RenderHtml();
        }

        private void RenderHtml()
        {
            StringBuilder writer = new StringBuilder();
            string SuperuserFilter = CommonLogic.IIF(ThisCustomer.IsAdminSuperUser, String.Empty, " Customer.IsAdmin!=3 and ");

            String StartDate = CommonLogic.FormCanBeDangerousContent("StartDate");
            String EndDate = CommonLogic.FormCanBeDangerousContent("EndDate");
            String AffiliateID = CommonLogic.FormCanBeDangerousContent("AffiliateID");
            String Gender = CommonLogic.FormCanBeDangerousContent("Gender");
            String CouponCode = CommonLogic.FormCanBeDangerousContent("CouponCode");
            String WithOrders = CommonLogic.FormCanBeDangerousContent("WithOrders");
            String EasyRange = CommonLogic.FormCanBeDangerousContent("EasyRange");
            String Day = CommonLogic.FormCanBeDangerousContent("Day");
            String Month = CommonLogic.FormCanBeDangerousContent("Month");
            String Year = CommonLogic.FormCanBeDangerousContent("Year");
            String CustomerType = CommonLogic.FormCanBeDangerousContent("CustomerType");

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
            if (CustomerType.Length == 0)
            {
                CustomerType = "AllCustomers";
            }

            // reset date range here, to ensure new orders are visible:
            if (StartDate.Length == 0)
            {
                DateTime DefaultDate = DateTime.Today.AddMonths(-1);
                StartDate = Localization.ToThreadCultureShortDateString(DefaultDate);
            }
            if (EndDate.Length == 0)
            {
                EndDate = Localization.ToThreadCultureShortDateString(System.DateTime.Now.AddDays(1));
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

            writer.Append("<form method=\"GET\" action=\"" + AppLogic.AdminLinkUrl("rpt_EMails.aspx") + "\" id=\"ReportForm\" name=\"ReportForm\" onsubmit=\"return (validateForm(this) && ReportForm_Validator(this))\">");
            writer.Append("  <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\">");
            writer.Append("    <tr class=\"tablenormal\">");
            writer.Append("      <td width=\"25%\" align=\"center\"><b>Date Range:</b></td>");
            writer.Append("      <td width=\"25%\" align=\"center\"><b>Customer Qualifiers:</b></td>");
            writer.Append("    </tr>");
            writer.Append("    <tr>");
            writer.Append("      <td width=\"25%\" valign=\"top\" align=\"left\" class=\"ordercustomer\">");
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
            writer.Append("              <td colspan=\"2\" align=\"left\" width=\"100%\"><input type=\"radio\" value=\"UseDatesAbove\" name=\"EasyRange\" " + CommonLogic.IIF(EasyRange == "UseDatesAbove" || EasyRange == "", "checked", "") + ">Use Dates Above</td>");
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
            writer.Append("      <td width=\"25%\" valign=\"top\" align=\"left\" class=\"ordercustomer\">");
            writer.Append("        <table border=\"0\" cellpadding=\"4\" cellspacing=\"0\" width=\"100%\">");

            writer.Append("          <tr>");
            writer.Append("            <td width=\"50%\">Affiliate:</td>");
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
            writer.Append("            <td width=\"50%\">Gender:</td>");
            writer.Append("            <td width=\"50%\"><select size=\"1\" name=\"Gender\">");
            writer.Append("                  <option value=\"-\" " + CommonLogic.IIF(Gender == "" || Gender == "-", "selected", "") + ">-</option>");
            writer.Append("                <option value=\"M\"" + CommonLogic.IIF(Gender == "M", "selected", "") + ">Male</option>");
            writer.Append("                <option value=\"F\"" + CommonLogic.IIF(Gender == "F", "selected", "") + ">Female</option>");
            writer.Append("              </select></td>");
            writer.Append("          </tr>");
            writer.Append("          <tr>");
            writer.Append("            <td width=\"50%\">Coupon Code:</td>");
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
            writer.Append("            <td width=\"50%\">With Orders:</td>");
            writer.Append("            <td width=\"50%\">");
            writer.Append("                <input type=\"radio\" name=\"WithOrders\" value=\"No\"" + CommonLogic.IIF(WithOrders == "No" || WithOrders.Length == 0, " checked ", "") + ">No&nbsp;&nbsp;&nbsp;&nbsp;");
            writer.Append("                <input type=\"radio\" name=\"WithOrders\" value=\"Yes\"" + CommonLogic.IIF(WithOrders == "Yes", " checked ", "") + ">Yes");
            writer.Append("                <input type=\"radio\" name=\"WithOrders\" value=\"Invert\"" + CommonLogic.IIF(WithOrders == "Invert", " checked ", "") + ">Without Orders");
            writer.Append("              </td>");
            writer.Append("          </tr>");
            writer.Append("        </table>");
            writer.Append("        </td>");
            writer.Append("    </tr>");
            writer.Append("    <tr>");
            writer.Append("      <td style=\"border-top:solid 2px #1B427D; background-color:#ffffff\" width=\"100%\" valign=\"middle\" align=\"center\" bgcolor=\"#dfecff\" height=\"25px\" colspan=\"2\">");
            writer.Append("        <input type=\"submit\" class=\"normalButtons\" value=\"submit\" name=\"B1\">&nbsp;<input class=\"normalButtons\" type=\"button\" onClick=\"javascript:self.location='" + AppLogic.AdminLinkUrl("rpt_EMails.aspx") + "';\" value=\"Reset\" name=\"B2\">");
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
                        DateWhere = " CreatedOn>=" + DB.DateQuote(Localization.ToDBDateTimeString(dt));
                        RangeStartDate = Localization.ParseNativeDateTime(StartDate);
                    }
                    else
                    {
                        RangeStartDate = System.DateTime.MinValue; // will get min date returned from either query
                    }
                    if (EndDate.Length != 0)
                    {
                        DateTime dt = Localization.ParseNativeDateTime(EndDate + " 11:59:59.999 PM");
                        DateWhere += CommonLogic.IIF(DateWhere.Length != 0, " and ", "") + "CreatedOn <=" + DB.DateQuote(Localization.ToDBDateTimeString(dt));
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
                        DateWhere = " day(CreatedOn)=" + Day + " ";
                    }
                    if (Month.Length != 0 && Month != "0")
                    {
                        if (DateWhere.Length != 0)
                        {
                            DateWhere += " and ";
                        }
                        DateWhere += " month(CreatedOn)=" + Month + " ";
                    }
                    if (Year.Length != 0 && Year != "0")
                    {
                        if (DateWhere.Length != 0)
                        {
                            DateWhere += " and ";
                        }
                        DateWhere += " year(CreatedOn)=" + Year + " ";
                    };
                    String DaySpec = CommonLogic.IIF(Day.Length == 0 || Day == "0", "1", Day);
                    String MonthSpec = CommonLogic.IIF(Month.Length == 0 || Month == "0", "1", Month);
                    String YearSpec = CommonLogic.IIF(Year.Length == 0 || Year == "0", System.DateTime.Now.Year.ToString(), Year);
                    RangeStartDate = Localization.ParseNativeDateTime(MonthSpec + "/" + DaySpec + "/" + YearSpec);
                    RangeEndDate = RangeStartDate;
                    break;
                case "Today":
                    DateWhere = "day(CreatedOn)=" + System.DateTime.Now.Day.ToString() + " and month(CreatedOn)=" + System.DateTime.Now.Month.ToString() + " and year(CreatedOn)=" + System.DateTime.Now.Year.ToString();
                    RangeStartDate = System.DateTime.Now;
                    RangeEndDate = System.DateTime.Now;
                    break;
                case "Yesterday":
                    DateWhere = "day(CreatedOn)=" + System.DateTime.Now.AddDays(-1).Day.ToString() + " and month(CreatedOn)=" + System.DateTime.Now.AddDays(-1).Month.ToString() + " and year(CreatedOn)=" + System.DateTime.Now.AddDays(-1).Year.ToString();
                    RangeStartDate = System.DateTime.Now.AddDays(-1);
                    RangeEndDate = System.DateTime.Now.AddDays(-1);
                    break;
                case "ThisWeek":
                    int DayOfWeek = (int)System.DateTime.Now.DayOfWeek;
                    System.DateTime weekstart = System.DateTime.Now.AddDays(-(DayOfWeek));
                    System.DateTime weekend = weekstart.AddDays(6);
                    int weekstartday = weekstart.DayOfYear;
                    int weekendday = weekend.DayOfYear;
                    DateWhere = "year(CreatedOn)=" + System.DateTime.Now.Year.ToString() + " and (datepart(\"dy\",CreatedOn)>=" + weekstartday.ToString() + " and datepart(\"dy\",CreatedOn)<=" + weekendday.ToString() + ")";
                    RangeStartDate = weekstart;
                    RangeEndDate = weekend;
                    break;
                case "LastWeek":
                    int DayOfWeek2 = (int)System.DateTime.Now.DayOfWeek;
                    System.DateTime weekstart2 = System.DateTime.Now.AddDays(-(DayOfWeek2)).AddDays(-7);
                    System.DateTime weekend2 = weekstart2.AddDays(6);
                    int weekstartday2 = weekstart2.DayOfYear;
                    int weekendday2 = weekend2.DayOfYear;
                    DateWhere = "year(CreatedOn)=" + System.DateTime.Now.Year.ToString() + " and (datepart(\"dy\",CreatedOn)>=" + weekstartday2.ToString() + " and datepart(\"dy\",CreatedOn)<=" + weekendday2.ToString() + ")";
                    RangeStartDate = weekstart2;
                    RangeEndDate = weekend2;
                    break;
                case "ThisMonth":
                    DateWhere = "month(CreatedOn)=" + System.DateTime.Now.Month.ToString() + " and year(CreatedOn)=" + System.DateTime.Now.Year.ToString();
                    RangeStartDate = Localization.ParseNativeDateTime(System.DateTime.Now.Month.ToString() + "/1/" + System.DateTime.Now.Year.ToString());
                    RangeEndDate = RangeStartDate.AddMonths(1).AddDays(-1);
                    break;
                case "LastMonth":
                    DateWhere = "month(CreatedOn)=" + System.DateTime.Now.AddMonths(-1).Month.ToString() + " and year(CreatedOn)=" + System.DateTime.Now.AddMonths(-1).Year.ToString();
                    RangeStartDate = Localization.ParseNativeDateTime(System.DateTime.Now.AddMonths(-1).Month.ToString() + "/1/" + System.DateTime.Now.AddMonths(-1).Year.ToString());
                    RangeEndDate = RangeStartDate.AddMonths(1).AddDays(-1);
                    break;
                case "ThisYear":
                    DateWhere = "year(CreatedOn)=" + System.DateTime.Now.Year.ToString();
                    RangeStartDate = Localization.ParseUSDateTime("1/1/" + System.DateTime.Now.Year.ToString());
                    RangeEndDate = RangeStartDate.AddYears(1).AddDays(-1);
                    if (RangeEndDate > System.DateTime.Now)
                    {
                        RangeEndDate = System.DateTime.Now;
                    }
                    break;
                case "LastYear":
                    DateWhere = "year(CreatedOn)=" + System.DateTime.Now.AddYears(-1).Year.ToString();
                    RangeStartDate = Localization.ParseUSDateTime("1/1/" + System.DateTime.Now.AddYears(-1).Year.ToString());
                    RangeEndDate = RangeStartDate.AddYears(1).AddDays(-1);
                    break;
            }
            if (DateWhere.Length != 0)
            {
                DateWhere = "(" + DateWhere + ")";
            }


            String WhereClause = DateWhere;
            String GeneralWhere = String.Empty;
            String RegOnlyWhere = String.Empty;
            if (AffiliateID != "-" && AffiliateID.Length != 0)
            {
                if (GeneralWhere.Length != 0)
                {
                    GeneralWhere += " and ";
                }
                GeneralWhere += "AffiliateID=" + AffiliateID.ToString();
            }
            if (Gender != "-" && Gender.Length != 0)
            {
                if (GeneralWhere.Length != 0)
                {
                    GeneralWhere += " and ";
                }
                GeneralWhere += "upper(Gender)=" + DB.SQuote(Gender.ToUpperInvariant());
            }
            if (CouponCode != "-" && CouponCode.Length != 0)
            {
                if (GeneralWhere.Length != 0)
                {
                    GeneralWhere += " and ";
                }
                GeneralWhere += "upper(CouponCode)=" + DB.SQuote(CouponCode.ToUpperInvariant());
            }
            if (WithOrders == "Yes")
            {
                if (RegOnlyWhere.Length != 0)
                {
                    RegOnlyWhere += " and ";
                }
                RegOnlyWhere += "customerid in (select distinct customerid from orders  with (NOLOCK)  )";
            }
            if (WithOrders == "Invert")
            {
                if (RegOnlyWhere.Length != 0)
                {
                    RegOnlyWhere += " and ";
                }
                RegOnlyWhere += "customerid not in (select distinct customerid from orders  with (NOLOCK)  )";
            }
            if (GeneralWhere.Length != 0)
            {
                GeneralWhere = "(" + GeneralWhere + ")";
            }
            if (RegOnlyWhere.Length != 0)
            {
                RegOnlyWhere = "(" + RegOnlyWhere + ")";
            }

            if (DateWhere.Length != 0)
            {
                String sql = "select EMail from Customer  with (NOLOCK)  where " + SuperuserFilter.ToString() + " EMail <> '' " + CommonLogic.IIF(RegOnlyWhere.Length != 0, " and " + RegOnlyWhere, "") + CommonLogic.IIF(GeneralWhere.Length != 0, " and " + GeneralWhere, "") + CommonLogic.IIF(WhereClause.Length != 0, " and " + WhereClause, "") + " order by createdon desc";
                if (AppLogic.AppConfigBool("Admin_ShowReportSQL"))
                {
                    writer.Append("<p align=\"left\">SQL=" + sql + "</p>\n");
                }


                using (SqlConnection dbconn = DB.dbConn())
                {
                    dbconn.Open();
                    using (IDataReader rs = DB.GetRS(sql, dbconn))
                    {
                        while (rs.Read())
                        {
                            writer.Append(DB.RSField(rs, "EMail") + "<br/>");
                        }
                    }
                }
            }
            writer.Append("</form>");
            ltContent.Text = writer.ToString();
        }

    }
}
