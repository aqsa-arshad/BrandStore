// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways;

namespace AspDotNetStorefrontAdmin
{
    /// <summary>
    /// Summary description for orders.
    /// </summary>
    public partial class orders : AdminPageBase
    {
        private string OrderByFields = "IsNew desc, OrderDate desc";

        private int m_FirstOrderNumber = 0;

        protected List<SqlParameter> spa = new List<SqlParameter>();

        protected DataSet dsAffiliate = null;
        protected DataSet dsCouponCode = null;
        protected DataSet dsState = null;

        public string HeaderImage
        {
            get
            {
                return String.Format(AppLogic.LocateImageURL("~/App_Themes/Admin_Default/images/orders.gif"), SkinID);
            }
        }

        public string NewImage
        {
            get
            {
                return String.Format(AppLogic.LocateImageURL("~/App_Themes/Admin_Default/images/new.gif"), SkinID);
            }
        }

        public int FirstOrderNumber
        {
            get
            {
                return m_FirstOrderNumber;
            }
        }

        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            Page.Form.DefaultButton = btnSubmit.UniqueID;

            Image1.ImageUrl = HeaderImage;

            if (CommonLogic.QueryStringCanBeDangerousContent("OrderNumber").Length != 0 && Request.UrlReferrer.AbsoluteUri.IndexOf("orders.aspx") == -1)
            {
                m_FirstOrderNumber = CommonLogic.QueryStringUSInt("OrderNumber");
                txtOrderNumber.Text = m_FirstOrderNumber.ToString();
                rbNewOrdersOnly.SelectedValue = "0";
            }

            if (!Page.IsPostBack) // Only initialize once
            {
                DoLocalization();
                DB.ExecuteSQL("update orders set IsNew=0 where ParentOrderNumber IS NOT NULL and CartType<>" + ((int)CartTypeEnum.RecurringCart).ToString()); // any "ad hoc" orders should not be new. so this is a safety check to force that.

                ProductMatchRow.Visible = (AppLogic.NumProductsInDB < 250);
                if (ProductMatchRow.Visible)
                {
                    ProductMatch.Items.Add(new ListItem("ALL", "-"));

                    using (SqlConnection dbconn = DB.dbConn())
                    {
                        dbconn.Open();
                        using (IDataReader rs = DB.GetRS("select ProductID,Name from Product with (NOLOCK) order by convert(nvarchar(4000),Name),ProductID", dbconn))
                        {
                            while (rs.Read())
                            {
                                ProductMatch.Items.Add(new ListItem(DB.RSFieldByLocale(rs, "Name", Localization.GetDefaultLocale()), DB.RSFieldInt(rs, "ProductID").ToString()));
                            }
                        }
                    }
                }

                DateTime MinOrderDate = Localization.ParseUSDateTime("1/1/1990");

                using (SqlConnection dbconn = DB.dbConn())
                {
                    dbconn.Open();
                    using (IDataReader rsd = DB.GetRS("Select min(OrderDate) as MinDate from orders with (NOLOCK)", dbconn))
                    {
                        if (rsd.Read())
                        {
                            MinOrderDate = DB.RSFieldDateTime(rsd, "MinDate");
                        }
                    }
                }
                dateStart.SelectedDate = MinOrderDate;
                if (dateStart.SelectedDate == System.DateTime.MinValue)
                {
                    dateStart.SelectedDate = System.DateTime.Now;
                }
                dateEnd.SelectedDate = System.DateTime.Now;

                using (SqlConnection dbconn = DB.dbConn())
                {
                    dbconn.Open();
                    using (IDataReader rs = DB.GetRS("select AffiliateID,Name from affiliate   with (NOLOCK)  order by displayorder,name", dbconn))
                    {
                        ddAffiliate.DataValueField = "AffiliateID";
                        ddAffiliate.DataTextField = "Name";
                        ddAffiliate.DataSource = rs;
                        ddAffiliate.DataBind();
                        ListItem item = new ListItem("-", "0");
                        ddAffiliate.Items.Insert(0, item);
                    }
                }
                using (SqlConnection dbconn = DB.dbConn())
                {
                    dbconn.Open();
                    using (IDataReader rs = DB.GetRS("select CouponCode from Coupon   with (NOLOCK)  order by CouponCode", dbconn))
                    {
                        ddCouponCode.DataValueField = "CouponCode";
                        ddCouponCode.DataTextField = "CouponCode";
                        ddCouponCode.DataSource = rs;
                        ddCouponCode.DataBind();
                        ListItem item = new ListItem("-", "-");
                        ddCouponCode.Items.Insert(0, item);
                    }
                }
                using (SqlConnection dbconn = DB.dbConn())
                {
                    dbconn.Open();
                    using (IDataReader rs = DB.GetRS("select code, id from promotions with (NOLOCK)  order by code", dbconn))
                    {
                        ddPromotion.DataValueField = "id";
                        ddPromotion.DataTextField = "code";
                        ddPromotion.DataSource = rs;
                        ddPromotion.DataBind();
                        ListItem item = new ListItem("-", "-");
                        ddPromotion.Items.Insert(0, item);
                    }
                }
                using (SqlConnection dbconn = DB.dbConn())
                {
                    dbconn.Open();
                    using (IDataReader rs = DB.GetRS("select Abbreviation,Name from state   with (NOLOCK)  order by DisplayOrder,Name", dbconn))
                    {
                        ddShippingState.DataValueField = "Abbreviation";
                        ddShippingState.DataTextField = "Name";
                        ddShippingState.DataSource = rs;
                       ddShippingState.DataBind();
                        ListItem item = new ListItem(AppLogic.GetString("admin.orders.ShippingState.DefaultValue", SkinID, LocaleSetting), "-");
                        ddShippingState.Items.Insert(0, item);
                    }
                }

                LoadPaymentGatewayDDL();

                GenerateReport();
            }            
        }

        private void LoadPaymentGatewayDDL()
        {
            ddPaymentMethod.Items.Add(new ListItem(AppLogic.GetString("admin.orders.PaymentMethod.AllTypes", SkinID, LocaleSetting), "-"));
            ddPaymentMethod.Items.Add(new ListItem(AppLogic.GetString("admin.orders.PaymentMethod.CreditCard", SkinID, LocaleSetting), "CREDITCARD"));
            ddPaymentMethod.Items.Add(new ListItem(AppLogic.GetString("admin.orders.PaymentMethod.GoogleCheckout", SkinID, LocaleSetting), "GOOGLECHECKOUT"));
            ddPaymentMethod.Items.Add(new ListItem(AppLogic.GetString("admin.orders.PaymentMethod.CheckoutByAmazon", SkinID, LocaleSetting), "CHECKOUTBYAMAZON"));
            ddPaymentMethod.Items.Add(new ListItem(AppLogic.GetString("admin.orders.PaymentMethod.PayPal", SkinID, LocaleSetting), "PAYPAL"));
            ddPaymentMethod.Items.Add(new ListItem(AppLogic.GetString("admin.orders.PaymentMethod.PayPalExpress", SkinID, LocaleSetting), "PAYPALEXPRESS"));
            ddPaymentMethod.Items.Add(new ListItem(AppLogic.GetString("admin.orders.PaymentMethod.PurchaseOrder", SkinID, LocaleSetting), "PURCHASEORDER"));
            ddPaymentMethod.Items.Add(new ListItem(AppLogic.GetString("admin.orders.PaymentMethod.RequestQuote", SkinID, LocaleSetting), "REQUESTQUOTE"));
            ddPaymentMethod.Items.Add(new ListItem(AppLogic.GetString("admin.orders.PaymentMethod.Check", SkinID, LocaleSetting), "CHECKBYMAIL"));
            ddPaymentMethod.Items.Add(new ListItem(AppLogic.GetString("admin.orders.PaymentMethod.COD", SkinID, LocaleSetting), "COD"));
            ddPaymentMethod.Items.Add(new ListItem(AppLogic.GetString("admin.orders.PaymentMethod.CODMoneyOrder", SkinID, LocaleSetting), "CODMONEYORDER"));
            ddPaymentMethod.Items.Add(new ListItem(AppLogic.GetString("admin.orders.PaymentMethod.CODCompanyCheck", SkinID, LocaleSetting), "CODCOMPANYCHECK"));
            ddPaymentMethod.Items.Add(new ListItem(AppLogic.GetString("admin.orders.PaymentMethod.CODNet30", SkinID, LocaleSetting), "CODNET30"));
            ddPaymentMethod.Items.Add(new ListItem(AppLogic.GetString("admin.orders.PaymentMethod.ECheck", SkinID, LocaleSetting), "ECHECK"));
            ddPaymentMethod.Items.Add(new ListItem(AppLogic.GetString("admin.orders.PaymentMethod.CardinalMyECheck", SkinID, LocaleSetting), "CARDINALMYECHECK"));
            ddPaymentMethod.Items.Add(new ListItem(AppLogic.GetString("admin.orders.PaymentMethod.MicroPay", SkinID, LocaleSetting), "MICROPAY"));
            ddPaymentMethod.SelectedIndex = 0;
        }
                
        private void GenerateReport()
        {
            string sql = "select OrderNumber, OrderDate, IsNew from orders   with (NOLOCK)  where " + WhereClause() + DateClause() + " order by " + OrderByFields;
            bool hasOrders = false;

            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS(sql, spa.ToArray(), dbconn))
                {
                    dlSelected.DataSource = rs;
                    dlSelected.DataBind();
                }
            }

            hasOrders = dlSelected.Items.Count > 0;

            if (hasOrders)
            {
                // NOTE:
                //  Because the use of datareaders, we removed getting the first row before assigning the datasource
                //  to the datalist above, since the datasource is already bound, we will then need to query
                //  the first order number on the datalist items. Here we assume the control holding the 
                //  order number info is a hyperlink control
                HyperLink lnkOrderNumber = dlSelected.Items[0].FindControl("hlOrderNumber") as HyperLink;
                if (lnkOrderNumber != null)
                {
                    string text = lnkOrderNumber.Text;
                    m_FirstOrderNumber = int.Parse(text);
                }
            }

            if (hasOrders)
            {             
                orderframe.Attributes["src"] = string.Format(AppLogic.AdminLinkUrl("orderframe.aspx")+ "?ordernumber={0}", m_FirstOrderNumber);
            }
            
            lblError.Text = String.Empty;
            if (AppLogic.AppConfigBool("Admin_ShowReportSQL"))
            {
                lblError.Text = "SQL=" + sql;
            }

            pnlBulkPrintingReport.Visible = BulkPrintingReport.Checked;
            pnlRegularReport.Visible = RegularReport.Checked;
            pnlSummaryReport.Visible = SummaryReport.Checked;

            if (hasOrders)
            {
                if (RegularReport.Checked)
                {
                    // don't have to do anything here
                }
                if (BulkPrintingReport.Checked)
                {
                    String summarySQL = "select Count(OrderNumber) as N from orders  with (NOLOCK)  where " + WhereClause() + DateClause() + "; " + "select OrderNumber, Shippedon, IsPrinted,ReadyToShip from orders  with (NOLOCK)  where " + WhereClause() + DateClause() + " order by OrderNumber";

                    using (SqlConnection dbconn = DB.dbConn())
                    {
                        dbconn.Open();
                        using (IDataReader rs = DB.GetRS(summarySQL, spa.ToArray(), dbconn))
                        {
                            if (rs.Read() && DB.RSFieldInt(rs, "N") > 0)
                            {
                                if (rs.NextResult())
                                {
                                    StringBuilder tmpS = new StringBuilder(4096);

                                    tmpS.Append("<script type=\"text/javascript\">\n");
                                    tmpS.Append("   function checkUncheckAll(theElement, ElementName)\n");
                                    tmpS.Append("{\n");
                                    tmpS.Append("	var chkb = theElement.form.PrintNow;\n");
                                    tmpS.Append("	var chkb0 = theElement.form.checkall;\n");
                                    tmpS.Append("	for (var i=0; i < chkb.length; i++)\n");
                                    tmpS.Append("	{\n");
                                    tmpS.Append("		chkb[i].checked = chkb0.checked;\n");
                                    tmpS.Append("	}\n");
                                    tmpS.Append("    }\n");
                                    tmpS.Append("</script>\n");
                                    tmpS.Append("<table cellpadding=\"4\" cellspacing=\"0\" border=\"0\" style=\"border-width: 1px; border-style: solid;\">");
                                    tmpS.Append("<tr class=\"table-header\">");
                                    tmpS.Append("<td align=\"center\"><b><nobr>"+AppLogic.GetString("admin.orders.BulkPrinting.OrderNumber", SkinID, LocaleSetting)+"</nobr></b></td>");
                                    tmpS.Append("<td align=\"center\"><b><nobr>" + AppLogic.GetString("admin.orders.BulkPrinting.OrderDate", SkinID, LocaleSetting) + "</nobr></b></td>");
                                    tmpS.Append("<td align=\"center\"><b><nobr>" + AppLogic.GetString("admin.orders.BulkPrinting.OrderTotal", SkinID, LocaleSetting) + "</nobr></b></td>");
                                    tmpS.Append("<td align=\"left\"><b><nobr>" + AppLogic.GetString("admin.orders.BulkPrinting.ShipToAddress", SkinID, LocaleSetting) + "</nobr></b></td>");
                                    tmpS.Append("<td align=\"left\"><b><nobr>" + AppLogic.GetString("admin.orders.BulkPrinting.Items", SkinID, LocaleSetting) + "</nobr></b></td>");
                                    tmpS.Append("<td align=\"center\"><b><nobr>" + AppLogic.GetString("admin.orders.BulkPrinting.IsShipped", SkinID, LocaleSetting) + "</nobr></b></td>");
                                    tmpS.Append("<td align=\"center\"><b><nobr>" + AppLogic.GetString("admin.orders.BulkPrinting.IsPrinted", SkinID, LocaleSetting) + "</nobr></b></td>");
                                    tmpS.Append("<td align=\"center\"><b><nobr>" + AppLogic.GetString("admin.orders.BulkPrinting.PrintItNow", SkinID, LocaleSetting) + "</nobr></b><br/>check all<br/><input type=\"checkbox\" id=\"checkall\" name=\"checkall\" onclick=\"checkUncheckAll(this,'PrintNow');\"/></td>");
                                    tmpS.Append("</tr>");
                                    int ctr = 0;
                                    while (rs.Read())
                                    {
                                        int ONX = DB.RSFieldInt(rs, "OrderNumber");
                                        Order ord = new Order(ONX, LocaleSetting);
                                        String ShipAddr = (ord.ShippingAddress.m_FirstName + " " + ord.ShippingAddress.m_LastName).Trim() + "<br/>";
                                        ShipAddr += ord.ShippingAddress.m_Address1;
                                        if (ord.ShippingAddress.m_Address2.Length != 0)
                                        {
                                            ShipAddr += "<br/>" + ord.ShippingAddress.m_Address2;
                                        }
                                        if (ord.ShippingAddress.m_Suite.Length != 0)
                                        {
                                            ShipAddr += ", " + ord.ShippingAddress.m_Suite;
                                        }
                                        ShipAddr += "<br/>" + ord.ShippingAddress.m_City + ", " + ord.ShippingAddress.m_State + " " + ord.ShippingAddress.m_Zip;
                                        ShipAddr += "<br/>" + ord.ShippingAddress.m_Country.ToUpper(CultureInfo.InvariantCulture);
                                        ShipAddr += "<br/>" + ord.ShippingAddress.m_Phone;

                                        ctr++;
                                        if (ctr % 2 == 0)
                                        {
                                            tmpS.Append("<tr class=\"table-alternatingrow2\">");
                                        }
                                        else
                                        {
                                            tmpS.Append("<tr class=\"table-row2\">");
                                        }
                                        tmpS.Append("<td align=\"center\" valign=\"top\" style=\"border-width: 1px; border-style: solid;\">");
                                        tmpS.Append(ONX.ToString());
                                        tmpS.Append("</td>");
                                        tmpS.Append("<td align=\"center\" valign=\"top\" style=\"border-width: 1px; border-style: solid;\">");
                                        tmpS.Append(Localization.ToNativeDateTimeString(ord.OrderDate));
                                        tmpS.Append("</td>");
                                        tmpS.Append("<td align=\"center\" valign=\"top\" style=\"border-width: 1px; border-style: solid;\">");
                                        tmpS.Append(ThisCustomer.CurrencyString(ord.Total(true)));
                                        tmpS.Append("</td>");
                                        tmpS.Append("<td align=\"left\" valign=\"top\" style=\"border-width: 1px; border-style: solid;\">");
                                        tmpS.Append(ShipAddr);
                                        tmpS.Append("</td>");
                                        tmpS.Append("<td align=\"left\" valign=\"top\" style=\"border-width: 1px; border-style: solid;\">");
                                        bool first = true;
                                        foreach (CartItem c in ord.CartItems)
                                        {
                                            if (!first)
                                            {
                                                tmpS.Append("<br/><br/>");
                                            }
                                            tmpS.Append("(" + c.Quantity.ToString() + ") ");
                                            tmpS.Append(ord.GetLineItemDescription(c));
                                            first = false;
                                        }

                                        tmpS.Append("</td>");
                                        tmpS.Append("<td align=\"center\" valign=\"top\" style=\"border-width: 1px; border-style: solid;\">");
                                        tmpS.Append(CommonLogic.IIF(DB.RSFieldDateTime(rs, "ShippedOn") == System.DateTime.MinValue, "No", Localization.ToNativeDateTimeString(DB.RSFieldDateTime(rs, "ShippedOn"))));
                                        tmpS.Append("</td>");
                                        tmpS.Append("<td align=\"center\" valign=\"top\" style=\"border-width: 1px; border-style: solid;\">");

                                        tmpS.Append(CommonLogic.IIF(DB.RSFieldBool(rs, "IsPrinted"), "Yes", "No"));
                                        tmpS.Append("</td>");
                                        tmpS.Append("<td align=\"center\" valign=\"top\" style=\"border-width: 1px; border-style: solid;\">");
                                        tmpS.Append("<input type=\"checkbox\" id=\"PrintNow\" name=\"PrintNow\" value=\"" + ONX.ToString() + "\" " + CommonLogic.IIF(!DB.RSFieldBool(rs, "IsPrinted"), " checked ", "") + ">");
                                        tmpS.Append("</td>");
                                        tmpS.Append("</tr>");
                                    }
                                    tmpS.Append("<tr>");
                                    tmpS.Append("<td colspan=\"7\">&nbsp;</td>");

                                    tmpS.Append("<td>");
                                    tmpS.Append("<div style=\"display:none;\"><input type=\"checkbox\" id=\"PrintNow\" name=\"PrintNow\" value=\"0\"></div>");
                                    tmpS.Append("<input type=\"button\" value=\"" + AppLogic.GetString("admin.orders.BulkPrinting.PrintReceipts", SkinID, LocaleSetting) + "\" class=\"normalButtons\" onClick=\"OpenPrintWindow();\">");
                                    tmpS.Append("</td>");
                                    tmpS.Append("</tr>");
                                    tmpS.Append("</table>");

                                    tmpS.Append("<script type=\"text/javascript\">\n");
                                    tmpS.Append("	function OpenPrintWindow()\n");
                                    tmpS.Append("	{\n");
                                    tmpS.Append("	alert('" + AppLogic.GetString("admin.orders.BulkPrinting.AlertWindow", SkinID, LocaleSetting) + "');\n");
                                    tmpS.Append("	var Orders = '';\n");
                                    tmpS.Append("	var chkb = document.getElementsByName('PrintNow');\n");
                                    tmpS.Append("	for (var i=0; i < chkb.length; i++)\n");
                                    tmpS.Append("	{\n");

                                    tmpS.Append("		if (chkb[i].checked)\n");
                                    tmpS.Append("		{\n");
                                    tmpS.Append("			if(i > 0)\n");
                                    tmpS.Append("			{\n");
                                    tmpS.Append("				Orders = Orders + ',';\n");
                                    tmpS.Append("			}\n");
                                    tmpS.Append("			if(chkb[i].value != '0') Orders = Orders + chkb[i].value;\n");
                                    tmpS.Append("		}\n");
                                    tmpS.Append("	}\n");

                                    tmpS.Append("	if(Orders == '')\n");
                                    tmpS.Append("	{\n");
                                    tmpS.Append("		alert('" + AppLogic.GetString("admin.common.NothingToPrint", SkinID, LocaleSetting) + "');\n");
                                    tmpS.Append("	}\n");
                                    tmpS.Append("	else\n");
                                    tmpS.Append("	{\n");
                                    tmpS.Append("		window.open('"+ AppLogic.AdminLinkUrl("printreceipts.aspx") + "?ordernumbers=' + Orders,'ASPDNSF_ML" + CommonLogic.GetRandomNumber(1, 100000).ToString() + "','height=600,width=800,top=0,left=0,status=yes,toolbar=yes,menubar=yes,scrollbars=yes,location=yes')\n");
                                    tmpS.Append("	}\n");
                                    tmpS.Append("	}\n");
                                    tmpS.Append("</SCRIPT>\n");

                                    Literal4.Text = tmpS.ToString();
                                }
                            }
                        }
                    }
                }
                if (SummaryReport.Checked)
                {
                    // doing summary report:
                    String SummaryReportFields = AppLogic.AppConfig("OrderSummaryReportFields");
                    if (SummaryReportFields.Length == 0)
                    {
                        SummaryReportFields = "*";
                    }

                    String summarySQL = "select " + SummaryReportFields + " from orders  with (NOLOCK)  where " + WhereClause() + DateClause() + " order by " + OrderByFields;

                    using (SqlConnection dbconn = DB.dbConn())
                    {
                        dbconn.Open();
                        using (IDataReader rs = DB.GetRS(summarySQL, spa.ToArray(), dbconn))
                        {
                            using (DataTable dt = new DataTable())
                            {
                                dt.Load(rs);

                                // unencrypt some data in the ds:
                                int SummaryCardNumberFieldIndex = -1;
                                int col = 0;
                                foreach (String s in SummaryReportFields.Split(','))
                                {
                                    if (s.Trim().Equals("CARDNUMBER", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        SummaryCardNumberFieldIndex = col;
                                    }
                                    col++;
                                }

                                if (SummaryCardNumberFieldIndex != -1)
                                {
                                    foreach (DataRow row in dt.Rows)
                                    {
                                        String s = row[SummaryCardNumberFieldIndex].ToString();
                                        if (ThisCustomer.AdminCanViewCC)
                                        {
                                            if (s.Length != 0)
                                            {
                                                try
                                                {
                                                    s = Security.UnmungeString(s, Order.StaticGetSaltKey(DB.RowFieldInt(row, "OrderNumber")));
                                                    if (!s.StartsWith(Security.ro_DecryptFailedPrefix, StringComparison.InvariantCultureIgnoreCase))
                                                    {
                                                        row[SummaryCardNumberFieldIndex] = s;
                                                    }
                                                }
                                                catch { }
                                            }
                                        }
                                        else
                                        {
                                            row[SummaryCardNumberFieldIndex] = "Cannot View";
                                        }
                                    }
                                }

                                SummaryGrid.DataSource = dt;
                                SummaryGrid.DataBind();
                            }
                        }
                    } 
                }
            }
            else
            {
                lblError.Text += "<br/><br/>" + AppLogic.GetString("admin.common.NoOrdersFound", SkinID, LocaleSetting);

                // remind the merchant that a frequent cause of zero results is having 'New Orders Only' selected
                if (rbNewOrdersOnly.SelectedValue == "1")
                {
                    lblError.Text += " " + AppLogic.GetString("admin.common.NoOrdersFoundReminder", SkinID, LocaleSetting);
                }

                pnlBulkPrintingReport.Visible = false;
                pnlRegularReport.Visible = false;
                pnlSummaryReport.Visible = false;
            }

            //Page.DataBind();
        }

        private void btnReset_Click(object sender, System.EventArgs e)
        {
            txtOrderNumber.Text = String.Empty;
            txtCustomerID.Text = String.Empty;
            txtEMail.Text = String.Empty;
            txtCreditCardNumber.Text = String.Empty;
            txtCustomerName.Text = String.Empty;
            txtCompany.Text = String.Empty;
            ddPaymentMethod.SelectedIndex = 0;
            TransactionState.SelectedIndex = 0;
            TransactionType.SelectedIndex = 0;
            ProductMatch.SelectedIndex = 0;
            ddAffiliate.SelectedIndex = 0;
            ddCouponCode.SelectedIndex = 0;
            ddPromotion.SelectedIndex = 0;
            ddShippingState.SelectedIndex = 0;
            dateStart.Clear();
            dateEnd.Clear();
            rbRange.Checked = true;
            rbNewOrdersOnly.SelectedValue = "1";
            GenerateReport();
        }

        /// <summary>
        /// Calculates the Where clause for the date portion of the search.
        /// </summary>
        public string DateClause()
        {
            string result = String.Empty;
            DateTime startDate = DateTime.Now;
            DateTime endDate = DateTime.Now;

            if (rbRange.Checked) //Use Dates above Range
            {
                
                if (dateStart.SelectedDate.CompareTo(dateEnd.SelectedDate) > 0) //Flip them
                {
                    endDate = dateStart.SelectedDate;
                    dateStart.SelectedDate = dateEnd.SelectedDate;
                    dateEnd.SelectedDate = endDate;
                }

                startDate = dateStart.SelectedDate;

				if(startDate < System.Data.SqlTypes.SqlDateTime.MinValue.Value)
					startDate = System.Data.SqlTypes.SqlDateTime.MinValue.Value;

                endDate = dateEnd.SelectedDate;

				if(endDate < System.Data.SqlTypes.SqlDateTime.MinValue.Value)
					endDate = System.Data.SqlTypes.SqlDateTime.MinValue.Value;
            }
            else
            {
                switch (rbEasyRange.SelectedValue)
                {
                    case "Today":
                        {
                            startDate = DateTime.Today;
                            endDate = startDate;
                            break;
                        }
                    case "Yesterday":
                        {
                            startDate = DateTime.Today.AddDays(-1);
                            endDate = startDate;
                            break;
                        }
                    case "ThisWeek":
                        {
                            startDate = DateTime.Today.AddDays(-((int)DateTime.Today.DayOfWeek));
                            endDate = startDate.AddDays(6);
                            break;
                        }
                    case "LastWeek":
                        {
                            startDate = DateTime.Today.AddDays(-((int)DateTime.Today.DayOfWeek) - 7);
                            endDate = startDate.AddDays(6);
                            break;
                        }
                    case "ThisMonth":
                        {
                            startDate = DateTime.Today.AddDays(1 - DateTime.Today.Day);
                            endDate = startDate.AddMonths(1);
                            break;
                        }
                    case "LastMonth":
                        {
                            startDate = DateTime.Today.AddMonths(-1);
                            startDate = startDate.AddDays(1 - startDate.Day);
                            endDate = startDate.AddMonths(1);
                            break;
                        }
                    case "ThisYear":
                        {
                            startDate = DateTime.Today.AddMonths(1 - DateTime.Today.Month);
                            startDate = startDate.AddDays(1 - startDate.Day);
                            endDate = startDate.AddYears(1);
                            break;
                        }
                    case "LastYear":
                        {
                            startDate = DateTime.Today.AddYears(-1);
                            startDate = startDate.AddMonths(1 - startDate.Month);
                            startDate = startDate.AddDays(1 - startDate.Day);
                            endDate = startDate.AddYears(1);
                            break;
                        }
                }
            }

            spa.Add(new SqlParameter("@StartDate", Localization.ToDBShortDateString(startDate)));
            spa.Add(new SqlParameter("@EndDate", Localization.ToDBShortDateString(endDate.AddDays(1))));

			if (TransactionDateType.Checked && TransactionState.SelectedIndex > 0)
            {
                // use transaction date, matched to type of transaction being requested
                if (TransactionState.SelectedValue == AppLogic.ro_TXStateAuthorized)
                {
                    result = " and ((AuthorizedOn >= @StartDate) and (AuthorizedOn < @EndDate))";
                }
                if (TransactionState.SelectedValue == AppLogic.ro_TXStateCaptured)
                {
                    result = " and ((CapturedOn >= @StartDate) and (CapturedOn < @EndDate))";
                }
                if (TransactionState.SelectedValue == AppLogic.ro_TXStateVoided)
                {
                    result = " and ((VoidedOn >= @StartDate) and (VoidedOn < @EndDate))";
                }
                if (TransactionState.SelectedValue == AppLogic.ro_TXStateForceVoided)
                {
                    result = " and ((VoidedOn >= @StartDate) and (VoidedOn < @EndDate))";
                }
                if (TransactionState.SelectedValue == AppLogic.ro_TXStateRefunded)
                {
                    result = " and ((RefundedOn >= @StartDate) and (RefundedOn < @EndDate))";
                }
                if (TransactionState.SelectedValue == AppLogic.ro_TXStateFraud)
                {
                    result = " and ((FraudedOn >= @StartDate) and (FraudedOn < @EndDate))";
                }
                if (TransactionState.SelectedValue == AppLogic.ro_TXStatePending)
                {
                    result = " and ((OrderDate >= @StartDate) and (OrderDate < @EndDate))"; // we have no "pendingon" date
                }
            }
            else
            {
                result = " and ((OrderDate >= @StartDate) and (OrderDate < @EndDate))";
            }

            return result;
        }

        /// <summary>
        /// Creates the Where Clause based on the Qualification fields.
        /// </summary>
        public string WhereClause()
        {
            spa.Clear();

            string result = "1=1";
            string sQuery = " and ({0}={1})";

            if (ddAffiliate.SelectedItem != null)
            {
                if (ddAffiliate.SelectedValue != "0" && ddAffiliate.SelectedItem.Text.Length != 0)
                {
                    spa.Add(new SqlParameter("@AffiliateID", ddAffiliate.SelectedValue));
                    result += String.Format(sQuery, "AffiliateID", "@AffiliateID");
                }
            }
            if (ddCouponCode.SelectedItem != null)
            {
                if (ddCouponCode.SelectedValue != "-" && ddCouponCode.SelectedItem.Text.Length != 0)
                {
                    spa.Add(new SqlParameter("@CouponCode", ddCouponCode.SelectedValue));
                    result += String.Format(sQuery, "CouponCode", "@CouponCode");
                }
            }
            if (ddPromotion.SelectedItem != null && ddPromotion.SelectedValue != "-" && ddPromotion.SelectedItem.Text.Length != 0)
            {
                int promotionID;
                if (int.TryParse(ddPromotion.SelectedValue, out promotionID))
                {
                    spa.Add(new SqlParameter("@PromotionID", promotionID));
                    result += " and ordernumber in (select orderid from promotionusage where promotionid = @PromotionID) ";
                }
            }
            if (ddShippingState.SelectedItem != null)
            {
                if (ddShippingState.SelectedValue != "-" && ddShippingState.SelectedItem.Text.Length != 0)
                {
                    spa.Add(new SqlParameter("@ShippingState", ddShippingState.SelectedValue));
                    result += String.Format(sQuery, "ShippingState", "@ShippingState");
                }
            }
            if (rbNewOrdersOnly.SelectedValue == "1")
            {
                spa.Add(new SqlParameter("@IsNew", 1));
                result += String.Format(sQuery, "IsNew", "@IsNew");
            }
            if (txtEMail.Text.Trim().Length != 0)
            {
                spa.Add(new SqlParameter("@Email", "%" + txtEMail.Text.Trim() + "%"));
                result += " and (EMail like @Email) ";
            }
            if (txtCustomerID.Text.Trim().Length != 0)
            {
                spa.Add(new SqlParameter("@CustomerID", txtCustomerID.Text.Trim()));
                result += String.Format(sQuery, "CustomerID", "@CustomerID");
            }
            if (txtOrderNumber.Text.Trim().Length != 0)
            {
                spa.Add(new SqlParameter("@OrderNumber", "%" + txtOrderNumber.Text.Trim() + "%"));
                result += " and (OrderNumber like @OrderNumber or AuthorizationPNREF like @OrderNumber or RecurringSubscriptionID like @OrderNumber )";
            }
            if (txtCreditCardNumber.Text.Trim().Length != 0)
            {
                spa.Add(new SqlParameter("@CardNumber", Security.MungeString(txtCreditCardNumber.Text.Trim(), Order.StaticGetSaltKey(0))));
                result += " and ((convert(nvarchar(4000),CardNumber)=@CardNumber)";
                if (txtCreditCardNumber.Text.Trim().Length == 4)
                {
                    spa.Add(new SqlParameter("@Last4", txtCreditCardNumber.Text.Trim()));
                    result += " or (convert(nvarchar(4000),Last4)=@Last4)";
                }
                result += ")";
            }
            if (txtCustomerName.Text.Trim().Length != 0)
            {
                spa.Add(new SqlParameter("@CustomerName", "%" + txtCustomerName.Text.Trim() + "%"));
                result += " and ((FirstName + ' ' + LastName) like @CustomerName)";
                result += "or ((FirstName) like @CustomerName)";
                result += "or ((LastName) like @CustomerName)";
            }
            if (txtCompany.Text.Trim().Length != 0)
            {
                spa.Add(new SqlParameter("@CompanyName", "%" + txtCompany.Text.Trim() + "%"));
                result += " and (ShippingCompany like @CompanyName or BillingCompany like @CompanyName)";
            }
            if (TransactionState.SelectedValue != "-")
            {
                spa.Add(new SqlParameter("@TransactionState", TransactionState.SelectedValue));
                result += String.Format(sQuery, "TransactionState", "@TransactionState");
            }
            if (TransactionType.SelectedValue != "-")
            {
                spa.Add(new SqlParameter("@TransactionType", (AppLogic.TransactionTypeEnum)Enum.Parse(typeof(AppLogic.TransactionTypeEnum), TransactionType.SelectedValue, true)));
                result += String.Format(sQuery, "TransactionType", "@TransactionType");
            }

            if (ProductMatchRow.Visible)
            {
                if ( ProductMatch.SelectedValue != "-" )
                {
                    spa.Add(new SqlParameter("@ProductID", ProductMatch.SelectedValue));
                    result += " and OrderNumber in (select ordernumber from orders_shoppingcart where productid=@ProductID)";
                }
            }

            if (!String.IsNullOrEmpty(txtProductSKU.Text))
            {
                spa.Add(new SqlParameter("@OrderedProductSku", txtProductSKU.Text.Trim()));
                result += " and OrderNumber in (select distinct ordernumber from orders_shoppingcart where OrderedProductSKU = @OrderedProductSku) ";
            }

            if (!string.IsNullOrEmpty(txtPriceRangeHigh.Text) && !string.IsNullOrEmpty(txtPriceRangeLow.Text))
            {
                decimal highprice;
                decimal lowprice;
                if (Decimal.TryParse(txtPriceRangeHigh.Text.Replace("$", "").Trim(), out highprice) && Decimal.TryParse(txtPriceRangeLow.Text.Replace("$", "").Trim(), out lowprice))
                {
                    if (lowprice > highprice)
                    {
                        decimal holder = highprice;
                        highprice = lowprice;
                        lowprice = holder;
                    }

                    spa.Add(new SqlParameter("@LowPrice", lowprice));
                    spa.Add(new SqlParameter("@HighPrice", highprice));
                    result += " and ordertotal >= @LowPrice and ordertotal <= @HighPrice ";
                }
            }

            if (ddPaymentMethod.SelectedValue != "-")
            {
                String PM = AppLogic.CleanPaymentMethod(ddPaymentMethod.SelectedValue);
                spa.Add(new SqlParameter("@PaymentMethod", ddPaymentMethod.SelectedValue));

                if (PM == AppLogic.ro_PMCreditCard)
                {
                    spa.Add(new SqlParameter("@PaymentGateway", AppLogic.ro_PMPayPal));
                    result += " and (PaymentMethod=@PaymentMethod or (PaymentGateway is not null and upper(PaymentGateway)=@PaymentGateway))";
                }
				if(PM == Gateway.ro_GWGOOGLECHECKOUT)
                {
                    result += String.Format(" and (PaymentGateway is not null and PaymentGateway LIKE 'Google%')");
                }
                if (PM == "CHECKOUTBYAMAZON")
                {
                    result += " and (PaymentMethod = @PaymentMethod)";
                }
                else if (PM == AppLogic.ro_PMPayPal)
                {
                    spa.Add(new SqlParameter("@PaymentGateway", AppLogic.ro_PMPayPal));
                    result += " and (PaymentMethod=@PaymentMethod or upper(PaymentGateway)=@PaymentGateway)";
                }
                else if (PM == AppLogic.ro_PMPayPalExpress)
                {
                    spa.Add(new SqlParameter("@PaymentGateway", AppLogic.ro_PMPayPalExpress));
                    result += " and (PaymentMethod=@PaymentMethod or upper(PaymentGateway)=@PaymentGateway)";
                }
                else if (PM == AppLogic.ro_PMPurchaseOrder)
                {
                    result += String.Format(sQuery, "PaymentMethod", "@PaymentMethod");
                }
                else if (PM == AppLogic.ro_PMCODMoneyOrder)
                {
                    result += String.Format(sQuery, "PaymentMethod", "@PaymentMethod");
                }
                else if (PM == AppLogic.ro_PMCODCompanyCheck)
                {
                    result += String.Format(sQuery, "PaymentMethod", "@PaymentMethod");
                }
                else if (PM == AppLogic.ro_PMCODNet30)
                {
                    result += String.Format(sQuery, "PaymentMethod", "@PaymentMethod");
                }
                else if (PM == AppLogic.ro_PMRequestQuote)
                {
                    result += " and  (PaymentMethod=@PaymentMethod or QuotECheckout<>0)";
                }
                else if (PM == AppLogic.ro_PMCheckByMail)
                {
                    result += String.Format(sQuery, "PaymentMethod", "@PaymentMethod");
                }
                else if (PM == AppLogic.ro_PMCOD)
                {
                    result += String.Format(sQuery, "PaymentMethod", "@PaymentMethod");
                }
                else if (PM == AppLogic.ro_PMECheck)
                {
                    result += String.Format(sQuery, "PaymentMethod", "@PaymentMethod");
                }
                else if (PM == AppLogic.ro_PMCardinalMyECheck)
                {
                    result += String.Format(sQuery, "PaymentMethod", "@PaymentMethod");
                }
                else if (PM == AppLogic.ro_PMMicropay)
                {
                    result += String.Format(sQuery, "PaymentMethod", "@PaymentMethod");
                }
            }

            if (ssOne.SelectedIndex != 0)
            {
                spa.Add(new SqlParameter("@StoreId", ssOne.SelectedIndex));
                result += String.Format(sQuery, "StoreId", "@StoreId");
            }

            return result;
        }

        private void DoLocalization()
        {
            SectionTitle = AppLogic.GetString("admin.menu.OrderManage", SkinID, LocaleSetting); 
            dateStart.Culture = Thread.CurrentThread.CurrentUICulture;
            dateEnd.Culture = Thread.CurrentThread.CurrentUICulture;
            dateStart.ClearDateText = AppLogic.GetString("txtClearDate", SkinID, LocaleSetting);
            dateEnd.ClearDateText = AppLogic.GetString("txtClearDate", SkinID, LocaleSetting);
            dateStart.GoToTodayText = AppLogic.GetString("txtTodaysDate", SkinID, LocaleSetting);
            dateEnd.GoToTodayText = AppLogic.GetString("txtTodaysDate", SkinID, LocaleSetting);
        }

        private void BulkPrintingGrid_ItemCommand(object source, System.Web.UI.WebControls.DataGridCommandEventArgs e)
        {

        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            GenerateReport();
        }

        private void SummaryGrid_ItemDataBound(object sender, System.Web.UI.WebControls.DataGridItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                // convert the long data fields to scrolling textarea fields, for compactness:
                foreach (TableCell c in e.Item.Cells)
                {
                    if (c.Text.Length > 50)
                    {
                        c.Text = "<textarea READONLY rows=\"12\" cols=\"50\">" + c.Text + "</textarea>";
                    }
                }
            }
            else if (e.Item.ItemType == ListItemType.Footer)
            {
            }
        }


        #region Web Form Designer generated code
        override protected void OnInit(EventArgs e)
        {
            //
            // CODEGEN: This call is required by the ASP.NET Web Form Designer.
            //
            InitializeComponent();
            base.OnInit(e);
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            SummaryGrid.ItemDataBound += new System.Web.UI.WebControls.DataGridItemEventHandler(SummaryGrid_ItemDataBound);

        }
        #endregion
    }
}
