// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
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
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Xsl;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
    public partial class Reports : AdminPageBase
	{
		protected Customer cust;
        protected List<SqlParameter> parameters = new List<SqlParameter>();
        protected Boolean isMultiStore = false;

        protected void Page_Load(object sender, EventArgs e)
		{
            //Do we need to show the store selector?
            List<Store> storeList = Store.GetStoreList();
            isMultiStore = storeList.Count > 1;

			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");

			cust = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;

			dateStart.Culture = new CultureInfo(Localization.GetDefaultLocale());
            dateEnd.Culture = new CultureInfo(Localization.GetDefaultLocale());
			if(!IsPostBack)
			{
				dateStart.SelectedDate = DateTime.Now.AddMonths(-6);

				dateEnd.SelectedDate = DateTime.Now;

				dateStart.Culture = Thread.CurrentThread.CurrentUICulture;
				dateEnd.Culture = Thread.CurrentThread.CurrentUICulture;
				phReportResults.Visible = false;
                pnlSummary.Visible = false;

                using (SqlConnection dbc = new SqlConnection(DB.GetDBConn()))
                {
                    dbc.Open();
                    using (IDataReader rsd = DB.GetRS("select name from customreport with (NOLOCK)", dbc))
                    {
                        while (rsd.Read())
                        {
                            string name = DB.RSField(rsd, "name");
                            ListItem li = new ListItem(name, "vBuiltIn" + name);
                            RadioReportType.Items.Add(li);
                        }
                    }
                }
                BuildSummaryReport();

                PopulateAffiliateDropdown();
                PopulateCustomerLevelDropdown();
			}
		}

		#region Report Foundation
		private void InitCurrentReport(RadioButton reportChoice)
		{
            //Show the Store selector if multiple stores exist and it's not a custom report
            if (isMultiStore && !RadioReportType.SelectedValue.Contains("vBuiltIn"))
            {
                pnlStores.Visible = true;
                CheckStoreCount();
                ShowHidepnlStores();
            }

            //Report Step 2 - Show all of the spec panels your report uses, if any.
            if (reportChoice != null && reportChoice.Checked)
            {
                switch (reportChoice.ID)
                {
                    case "rblAbandonedCart":
                    case "rblBestsellers":
                    case "rblPromotions":
                    case "rblReferrals":
                    case "rblOrdersByDateRange":
                    case "rblOrdersByItem":
                        pnlDateSpecs.Visible = true;
                        break;
                    case "rblCustomers":
                        pnlCustomerType.Visible = true;
                        break;
                    case "rblAffiliates":
                        pnlDateSpecs.Visible = true;
                        pnlAffiliates.Visible = true;
                        break;
                    case "rblCustomersByProduct":
                        pnlProductId.Visible = true;
                        pnlDateSpecs.Visible = true;
                        break;
                    case "rblEmptyEntities":
                        pnlStores.Visible = false;
                        pnlEntityTypes.Visible = true;
                        break;
                    case "rblOrdersByEntity":
                        PopulateEntityDropdowns(); // Don't build these lists unless the option is chosen, as this may be slow!
                        pnlDateSpecs.Visible = true;
                        pnlEntities.Visible = true;
                        break;
                    case "rblInventoryLevels":
                        pnlQuantity.Visible = true;
                        break;
                    case "rblUnmappedProducts":
                        pnlEntityTypes.Visible = true;
                        break;
                    case "rblProductsByCustomerLevel":
                        pnlCustomerLevels.Visible = true;
                        break;
                }
            }
                
            btnReport.Visible = true;
		}

        private void BuildReport()
        {
            if (RadioReportType.SelectedValue.Contains("vBuiltIn"))
            {
                String reportname = RadioReportType.SelectedValue.Replace("vBuiltIn", "");
                BuildCustomReport(reportname);
                return;
            }

            if (isMultiStore)
                parameters.Add(new SqlParameter("@StoreID", ssOne.SelectedIndex));
            else
                parameters.Add(new SqlParameter("@StoreID", "0"));

            //Report Step 3 - Add your report's method to the list.
            if (rblAbandonedCart.Checked)
                BuildAbandonedCartReport();
            else if (rblAffiliates.Checked)
                BuildAffiliatesReport();
            else if (rblBestsellers.Checked)
                BuildBestSellersReport();
            else if (rblCustomers.Checked)
                BuildCustomerReport();
            else if (rblCustomersByProduct.Checked)
                BuildCustomersByProductReport();
            else if (rblEmptyEntities.Checked)
                BuildEmptyEntitiesReport();
            else if (rblInventoryLevels.Checked)
                BuildInventoryReport();
            else if (rblOrdersByEntity.Checked)
                BuildOrdersByEntityReport();
            else if (rblPromotions.Checked)
                BuildPromotionReport();
            else if (rblReferrals.Checked)
                BuildReferralsReport();
            else if (rblUnmappedProducts.Checked)
                BuildUnmappedProductsReport();
            else if (rblCurrentRecurring.Checked)
                BuildCurrentRecurringProductsReport();
            else if (rblCancelledRecurring.Checked)
                BuildCancelledRecurringProductsReport();
            else if (rblProductsByCustomerLevel.Checked)
                BuildProductsByCustomerLevelReport();
            else if (rblOrdersByDateRange.Checked)
                BuildOrdersByDateRangeReport();
            else if (rblOrdersByItem.Checked)
                BuildOrdersByItemReport();
        }

        private void BuildReportFromSql(string sql)
        {
            HtmlTable rTable = new HtmlTable();
            rTable.CellSpacing = 1;
            rTable.ID = "ReportResultsTable";

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS(sql, parameters.ToArray(), dbconn))
                {
                    HtmlTableRow headerrow = new HtmlTableRow();
                    for (int i = 0; i < rs.FieldCount; i++)
                    {
                        HtmlTableCell th = new HtmlTableCell();
                        th.Style["font-weight"] = "bold";
                        th.Style["font-size"] = "12px";
                        th.InnerText = rs.GetName(i);
                        headerrow.Cells.Add(th);
                    }
                    rTable.Rows.Add(headerrow);

                    while (rs.Read())
                    {
                        HtmlTableRow row = new HtmlTableRow();
                        for (int i = 0; i < rs.FieldCount; i++)
                        {
                            var td = new HtmlTableCell();
                            td.InnerText = XmlCommon.GetLocaleEntry(rs[i].ToString(), ThisCustomer.LocaleSetting, true);

                            if (i % 2 == 0)
                                td.Style["background-color"] = "#EBEBEB;";

                            row.Cells.Add(td);
                        }
                        rTable.Rows.Add(row);
                    }
                }
            }

            phReportResults.Controls.Add(rTable);
            phReportResults.Visible = true;
            btnSaveReport.Visible = true;
        }

        private void ShowHidepnlStores()
        {
            if (ddEntity.SelectedValue == "Stores" && rblUnmappedProducts.Checked)
            {
                pnlStores.Visible = false;
            }
            else
            {
                pnlStores.Visible = true;
            }
        }
        #endregion

        #region Build Lists
        private void PopulateAffiliateDropdown()
        {
            using (SqlConnection dbc = new SqlConnection(DB.GetDBConn()))
            {
                dbc.Open();
                using (IDataReader rsd = DB.GetRS("SELECT Name, AffiliateId FROM Affiliate WITH (NOLOCK) ORDER BY Name", dbc))
                {
                    while (rsd.Read())
                    {
                        string name = DB.RSField(rsd, "Name");
                        int Id = DB.RSFieldInt(rsd, "AffiliateId");
                        ListItem li = new ListItem(name, Id.ToString());
                        ddAffiliates.Items.Add(li);
                    }
                }
            }
        }

        private void PopulateEntityDropdowns()
        {
            if (ddCategories.Items.Count == 0)
            {
                ddCategories.Items.Clear();
            ddCategories.Items.Add(new ListItem("-- None --", "0"));
            using (SqlConnection dbc = new SqlConnection(DB.GetDBConn()))
            {
                dbc.Open();
                using (IDataReader rsd = DB.GetRS("SELECT Name, CategoryID FROM Category WITH (NOLOCK) ORDER BY Name", dbc))
                {
                    while (rsd.Read())
                    {
                        string name = DB.RSField(rsd, "Name");
                        int Id = DB.RSFieldInt(rsd, "CategoryID");
                        ListItem li = new ListItem(name, Id.ToString());
                        ddCategories.Items.Add(li);
                    }
                }
            }
            }

            if (ddManufacturers.Items.Count == 0)
            {
                ddManufacturers.Items.Clear();
                ddManufacturers.Items.Add(new ListItem("-- None --", "0"));
            using (SqlConnection dbc = new SqlConnection(DB.GetDBConn()))
            {
                dbc.Open();
                using (IDataReader rsd = DB.GetRS("SELECT Name, ManufacturerID FROM Manufacturer WITH (NOLOCK) ORDER BY Name", dbc))
                {
                    while (rsd.Read())
                    {
                        string name = DB.RSField(rsd, "Name");
                        int Id = DB.RSFieldInt(rsd, "ManufacturerID");
                        ListItem li = new ListItem(name, Id.ToString());
                        ddManufacturers.Items.Add(li);
                    }
                }
            }
            }

            if (ddSections.Items.Count == 0)
            {
                ddSections.Items.Clear();
                ddSections.Items.Add(new ListItem("-- None --", "0"));
            using (SqlConnection dbc = new SqlConnection(DB.GetDBConn()))
            {
                dbc.Open();
                using (IDataReader rsd = DB.GetRS("SELECT Name, SectionID FROM Section WITH (NOLOCK) ORDER BY Name", dbc))
                {
                    while (rsd.Read())
                    {
                        string name = DB.RSField(rsd, "Name");
                        int Id = DB.RSFieldInt(rsd, "SectionID");
                        ListItem li = new ListItem(name, Id.ToString());
                        ddSections.Items.Add(li);
                    }
                }
            }
        }
        }

        protected void PopulateDateParameters()
        {
            //Some checks to make sure we don't get bad dates
            dateStart.MinDate = (DateTime)SqlDateTime.MinValue;
            dateStart.MaxDate = (DateTime)SqlDateTime.MaxValue;

            dateEnd.MinDate = (DateTime)SqlDateTime.MinValue;
            dateEnd.MaxDate = (DateTime)SqlDateTime.MaxValue;

            if (dateStart.SelectedDate == null)
                dateStart.SelectedDate = (DateTime)SqlDateTime.MinValue;
            if (dateEnd.SelectedDate == null)
                dateEnd.SelectedDate = (DateTime)SqlDateTime.MaxValue;

            DateTime startDate = DateTime.Now;
            DateTime endDate = DateTime.Now;

            switch (rblRange.SelectedValue)
            {
                case "0":
                    {
                        if (dateStart.SelectedDate > dateEnd.SelectedDate) //Flip them
                        {
                            endDate = (DateTime)dateStart.SelectedDate;
                            dateStart.SelectedDate = dateEnd.SelectedDate;
                            dateEnd.SelectedDate = endDate;
                        }

                        startDate = (DateTime)dateStart.SelectedDate;
                        endDate = (DateTime)dateEnd.SelectedDate;

                        break;
                    }
                case "1":
                    {
                        startDate = DateTime.Today;
                        endDate = startDate.AddDays(1);
                        break;
                    }
                case "2":
                    {
                        startDate = DateTime.Today;
                        endDate = startDate;
                        break;
                    }
                case "3":
                    {
                        startDate = DateTime.Today.AddDays(-((int)DateTime.Today.DayOfWeek));
                        endDate = startDate.AddDays(6);
                        break;
                    }
                case "4":
                    {
                        startDate = DateTime.Today.AddDays(-((int)DateTime.Today.DayOfWeek) - 7);
                        endDate = startDate.AddDays(6);
                        break;
                    }
                case "5":
                    {
                        startDate = DateTime.Today.AddDays(1 - DateTime.Today.Day);
                        endDate = startDate.AddMonths(1);
                        break;
                    }
                case "6":
                    {
                        startDate = DateTime.Today.AddMonths(-1);
                        startDate = startDate.AddDays(1 - startDate.Day);
                        endDate = startDate.AddMonths(1);
                        break;
                    }
                case "7":
                    {
                        startDate = DateTime.Today.AddMonths(1 - DateTime.Today.Month);
                        startDate = startDate.AddDays(1 - startDate.Day);
                        endDate = startDate.AddYears(1);
                        break;
                    }
                case "8":
                    {
                        startDate = DateTime.Today.AddYears(-1);
                        startDate = startDate.AddMonths(1 - startDate.Month);
                        startDate = startDate.AddDays(1 - startDate.Day);
                        endDate = startDate.AddYears(1);
                        break;
                    }
            }

            if (startDate >= SqlDateTime.MinValue.Value && startDate <= SqlDateTime.MaxValue.Value)
                parameters.Add(new SqlParameter("@StartDate", Localization.ToDBShortDateString(startDate)));
            if (endDate >= SqlDateTime.MinValue.Value && endDate <= SqlDateTime.MaxValue.Value)
                parameters.Add(new SqlParameter("@EndDate", Localization.ToDBShortDateString(endDate.AddDays(1))));
        }

        private void PopulateCustomerLevelDropdown()
        {
            using (SqlConnection dbc = new SqlConnection(DB.GetDBConn()))
            {
                dbc.Open();
                using (IDataReader rsd = DB.GetRS("Select CustomerLevelID, Name FROM CustomerLevel WITH (NOLOCK) ORDER BY Name", dbc))
                {
                    while (rsd.Read())
                    {
                        string name = DB.RSFieldByLocale(rsd, "Name", ThisCustomer.LocaleSetting);
                        int Id = DB.RSFieldInt(rsd, "CustomerLevelID");
                        ListItem li = new ListItem(name, Id.ToString());
                        ddCustomerLevel.Items.Add(li);
                    }
                }
            }
        }
        #endregion

		#region State Control
		private void HideFilters()
		{
			//Input Parameter Step 2 - add your panel to the global hide logic (for report switching). 
            pnlCustomerType.Visible = false;
            pnlEntityTypes.Visible = false;
            pnlAffiliates.Visible = false;
			pnlDateSpecs.Visible = false;
            pnlSummary.Visible = false;
			btnSaveReport.Visible = false;
            pnlStores.Visible = false;
            pnlEntities.Visible = false;
            pnlQuantity.Visible = false;
            pnlProductId.Visible = false;
            pnlCustomerLevels.Visible = false;

			ResetError("", false);
		}

		protected void ResetError(string error, bool isError)
		{
			string str = AppLogic.GetString("admin.reports.notice", cust.LocaleSetting);

			if(isError && error.Length > 0)
                str = AppLogic.GetString("admin.reports.error", cust.LocaleSetting);
			else
				str = "";

			ltError.Text = str + error;
            ltError.Visible = true;
		}
		#endregion

		#region Event Handlers
        protected void ddEntity_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowHidepnlStores();
        }

		protected void BuiltInReport_Clicked(object sender, EventArgs e)
		{
            if (RadioReportType.SelectedIndex > -1)
                RadioReportType.ClearSelection();

			HideFilters();
            RadioButton reportChoice = sender as RadioButton;
			InitCurrentReport(reportChoice);
		}

        protected void CustomReport_Clicked(object sender, EventArgs e)
        {
            rblAbandonedCart.Checked =
            rblAffiliates.Checked =
            rblCustomers.Checked =
            rblEmptyEntities.Checked =
            rblInventoryLevels.Checked =
            rblOrdersByEntity.Checked =
            rblPromotions.Checked =
            rblReferrals.Checked =
            rblUnmappedProducts.Checked =
            rblCurrentRecurring.Checked =
            rblCancelledRecurring.Checked =
            rblOrdersByDateRange.Checked = 
            rblOrdersByItem.Checked = false;

            HideFilters();
            RadioButton reportChoice = sender as RadioButton;
            InitCurrentReport(reportChoice);
        }

		protected void BtnReport_Click(object sender, EventArgs e)
		{
            ltError.Visible = false;
			BuildReport();
		}

		protected void BtnSaveReport_click(object sender, EventArgs e)
		{
			BuildReport();

			try
			{
                MemoryStream tableStream = new MemoryStream();

                StreamWriter tableWriter = new StreamWriter(tableStream, System.Text.Encoding.UTF8);
                using (HtmlTextWriter htmlWriter = new HtmlTextWriter(tableWriter))
                {
                    tableWriter.Write("<root>");
                    phReportResults.RenderControl(htmlWriter);
                    tableWriter.Write("</root>");
                }
                tableWriter.Flush();

				string filepath = CommonLogic.SafeMapPath("../images") + "\\";
				string filename = "ReportExport_" + Localization.ToNativeDateTimeString(System.DateTime.Now).Replace(" ", "_").Replace(",", "_").Replace("/", "_").Replace(":", "_").Replace(".", "_");
                filename += ".csv";

                tableStream.Position = 0;
                StreamReader reader = new StreamReader(tableStream, System.Text.Encoding.UTF8);
                    
                XmlDocument xdoc = new XmlDocument();
                xdoc.Load(reader);
                
                string FullFilePath = filepath + filename;
                XslCompiledTransform xsl = new XslCompiledTransform();

                xsl.Load(CommonLogic.SafeMapPath("XmlPackages/ReportExportCSV.xslt"));

                using(StreamWriter sw = new StreamWriter(FullFilePath, false, System.Text.Encoding.UTF8))
                    xsl.Transform(xdoc, null, sw);

				Response.Clear();
				Response.ClearHeaders();
				Response.ClearContent();
                Response.AddHeader("content-disposition", "attachment; filename=" + filename);
				Response.BufferOutput = false;

				// Send the CSV
				Response.BufferOutput = false;
				Response.ContentType = "text/csv";
				Response.TransmitFile(FullFilePath);
				Response.Flush();
				Response.End();
			}
			catch(Exception)
			{
                ResetError(AppLogic.GetString("admin.reports.formaterror",cust.LocaleSetting), true);
			}
		}
		#endregion

		#region Build Custom Report Functions
		private HtmlTableRow GetRow(string[] s)
		{
			return GetRow(s, 0, 0);
		}

		private HtmlTableRow GetRow(string[] s, int leadingempties, int trailingempties)
		{
			HtmlTableRow row = new HtmlTableRow();
			for(int i = 0; i < leadingempties; i++)
			{
				HtmlTableCell c = new HtmlTableCell();
				c.InnerText = "";
				row.Cells.Add(c);
			}
			for(int i = 0; i < s.Length; i++)
			{
				HtmlTableCell c = new HtmlTableCell();
				c.InnerHtml = s[i];
				row.Cells.Add(c);
			}
			for(int i = 0; i < trailingempties; i++)
			{
				HtmlTableCell c = new HtmlTableCell();
				c.InnerText = "";
				row.Cells.Add(c);
			}
			return row;
		}

        private void BuildCustomReport(String reportname)
        {
            parameters.Add(new SqlParameter("@ReportName", reportname));

            using (SqlConnection dbc = new SqlConnection(DB.GetDBConn()))
            {
                dbc.Open();
                using (IDataReader rsd = DB.GetRS("select sqlcommand from customreport with (NOLOCK) where name = @ReportName", parameters.ToArray(), dbc))
                {
                    parameters.Clear();
                    if (rsd.Read())
                        BuildReportFromSql(DB.RSField(rsd, "sqlcommand"));
                }
            }
        }

        private static string DecimalWrapper(decimal d)
        {
            return Localization.CurrencyStringForDisplayWithoutExchangeRate(d).Replace(" (USD)", "");
        }

        private static HtmlTableRow AddRow(string cellonevalue, string celltwovalue)
        {
            HtmlTableRow trTotalSales = new HtmlTableRow();
            HtmlTableCell cellone = new HtmlTableCell();
            cellone.InnerText = cellonevalue;
            trTotalSales.Cells.Add(cellone);
            HtmlTableCell celltwo = new HtmlTableCell();
            celltwo.InnerText = celltwovalue;
            trTotalSales.Cells.Add(celltwo);
            return trTotalSales;
        }
        #endregion

        #region Build Built In Reports
        //Report Step 4 - Build your report function(s). 

        private void CheckStoreCount()
        {
            if (!isMultiStore || !rblUnmappedProducts.Checked)
            {
                ddEntity.Items.Remove(AppLogic.GetString("admin.reports.stores", "en-US"));
            }
            else
            {
                if (!ddEntity.Items.Contains(new ListItem("Stores")))
                {
                    ddEntity.Items.Add(new ListItem(AppLogic.GetString("admin.reports.stores", "en-US"), AppLogic.GetString("admin.reports.stores", "en-US")));
                }
            }
        }

        private void BuildAbandonedCartReport()
        {
            string sql = "Select c.Email "
		                            + ",c.FirstName "
		                            + ",c.LastName "
		                            + ",c.Phone "
		                            + ",Case When c.OkToEmail = 0 Then 'No' Else 'Yes' End [OkToEmail] "
		                            + ",sc.[CustomerID] "
		                            + ",sc.[ProductSKU] "
		                            + ",sc.[ProductPrice] "
		                            + ",sc.[ProductID] "
		                            + ",p.Name [ProductName] "
		                            + ",sc.[VariantID] "
		                            + ",pv.Name [VariantName] "
		                            + ",sc.[Quantity] "
		                            + ",sc.[CreatedOn] [CartCreatedOn] "
		                            + ",sc.[TextOption] "
		                            + ",sc.[BillingAddressID] "
		                            + ",sc.[ShippingAddressID] "
		                            + ",sc.[Notes] "
		                            + ",sc.[CustomerEntersPrice] "
		                            + ",ba.FirstName [BillingFirstName] "
		                            + ",ba.LastName [BillingLastName] "
		                            + ",ba.Company [BillingCompany] "
		                            + ",ba.Address1 [BillingAddress1] "
		                            + ",ba.Address2 [BillingAddress2] "
		                            + ",ba.Suite [BillingSuite] "
		                            + ",ba.City [BillingCity] "
		                            + ",ba.[State] [BillingState] "
		                            + ",ba.[Zip] [BillingZip] "
		                            + ",ba.Phone [BillingPhone] "
		                            + ",ba.Email [BillingEmail] "
		                            + ",sa.FirstName [ShippingFirstName] "
		                            + ",sa.LastName [ShippingLastName] "
		                            + ",sa.Company [ShippingCompany] "
		                            + ",sa.Address1 [ShippingAddress1] "
		                            + ",sa.Address2 [ShippingAddress2] "
		                            + ",sa.Suite [ShippingSuite] "
		                            + ",sa.City [ShippingCity] "
		                            + ",sa.[State] [ShippingState] "
		                            + ",sa.[Zip] [ShippingZip] "
		                            + ",sa.Phone [ShippingPhone] "
		                            + ",sa.Email [ShippingEmail] "
                            + "From ShoppingCart sc "
                            + "Inner Join [Customer] c On c.CustomerID = sc.CustomerID "
                            + "Inner Join Product p On p.ProductId = sc.ProductID "
                            + "Inner Join ProductVariant pv On pv.VariantId = sc.VariantId "
                            + "Left Join [Address] ba On ba.AddressID = sc.BillingAddressID "
                            + "Left Join [Address] sa On sa.AddressID = sc.ShippingAddressID "
                            + "Where sc.CartType = 0 "
                            + "and sc.CreatedOn >= @StartDate and sc.CreatedOn <= @EndDate "
                            + "and (sc.StoreID = @StoreID OR 0 = @StoreID) "
                            + "Order By sc.CreatedOn Desc, c.Email, c.CustomerID";

            PopulateDateParameters();
            BuildReportFromSql(sql);
        }

        private void BuildInventoryReport()
        {
            parameters.Add(new SqlParameter("@InventoryLevel", txtQuantity.Text.Trim()));

            string sql = "SELECT p.ProductID, p.Name AS 'Product Name', "
                    + "pv.Name AS 'Variant Name', "
                    + "CASE p.TrackInventoryBySizeAndColor "
                        + "WHEN 1 THEN i.Size "
                        + "ELSE '-' "
                    + "END AS Size, "
                    + "CASE p.TrackInventoryBySizeAndColor "
                        + "WHEN 1 THEN i.Color "
                        + "ELSE '-' "
                    + "END AS Color, "
                    + "CASE p.TrackInventoryBySizeAndColor "
                        + "WHEN 1 THEN ISNULL(i.Quan, 0) "
                        + "ELSE pv.Inventory "
                    + "END AS 'On Hand', "
                    + "CASE p.Published "
                        + "WHEN 1 THEN 'Yes' "
                        + "ELSE 'No' "
                    + "END AS 'Published?' "
                    + "FROM Product p "
                    + "LEFT JOIN ProductVariant pv ON p.ProductID = pv.ProductID "
                    + "LEFT JOIN Inventory i ON pv.VariantID = i.VariantID "
                    + "LEFT JOIN ProductStore ps ON p.ProductID = ps.ProductID "
                    + "WHERE (ISNULL(i.Quan, @InventoryLevel - 1) < @InventoryLevel) AND pv.Inventory < @InventoryLevel "
                    +  "AND (ps.StoreID = @StoreId OR 0 = @StoreId)";

            BuildReportFromSql(sql);
        }

        private void BuildCustomerReport()
        {
            string sql = "SELECT Email, FirstName AS 'First Name', LastName AS 'Last Name', CASE OkToEmail WHEN 1 THEN 'Yes' ELSE 'No' END AS 'Ok to email?' ";

            if (ddCustomerType.SelectedValue == "Registered" || ddCustomerType.SelectedValue == "Both")
            {
                parameters.Add(new SqlParameter("@IsRegistered", "1"));
                sql += ", RegisterDate FROM Customer WHERE IsRegistered=@IsRegistered "
                    + "AND (StoreID = @StoreID OR 0 = @StoreId) ";
            }
            else
            {
                parameters.Add(new SqlParameter("@IsRegistered", "0"));
                sql += "FROM Customer WHERE IsRegistered=@IsRegistered "
                    + "AND (StoreID = @StoreID OR 0 = @StoreId) ";
            }

            sql += " ORDER BY RegisterDate DESC";

            BuildReportFromSql(sql);
        }

        private void BuildPromotionReport()
        {
            string sql = "SELECT p.Name, SUM(CAST(pu.Complete AS Integer)) AS 'Times Used', "
                + "-SUM(pu.ShippingDiscountAmount) AS 'Shipping Discounts Given', "
                + "-SUM(pu.LineItemDiscountAmount) AS 'Line Item Discounts Given', "
                + "-SUM(pu.OrderDiscountAmount) AS 'Order Level Discounts Given', "
                + "CASE p.Active WHEN 1 THEN 'Yes' ELSE 'No' END AS 'Still Active?' "
                + "FROM Promotions p LEFT JOIN PromotionUsage pu ON P.Id = pu.PromotionId "
                + "LEFT JOIN Orders o ON o.OrderNumber = pu.OrderId "
                + "WHERE pu.Complete IS NOT NULL "
                    + "AND pu.Complete > 0 "
                    + "AND pu.DateApplied >= @StartDate "
                    + "AND pu.DateApplied <= @EndDate "
                    + "AND (o.StoreId = @StoreId OR 0 = @StoreId) "
                + "GROUP BY p.Name, p.Active";

            PopulateDateParameters();
            BuildReportFromSql(sql);
        }

        private void BuildUnmappedProductsReport()
        {
            string sql = string.Empty;
            switch (ddEntity.SelectedValue)
            {
                case "Stores":
                    {
                        sql = "SELECT p.Name, p.ProductID, "
                                + "CASE p.Published WHEN 1 THEN 'Yes' ELSE 'No' END AS 'Published?' "
                                + "FROM Product p "
                                + "WHERE NOT EXISTS "
                                + "(SELECT ProductID FROM ProductStore ps WHERE p.ProductID = ps.ProductID) AND p.IsSystem != 1";

                        break;
                    }
                default:
                    {
                        sql = "SELECT p.Name, p.ProductID, CASE p.Published WHEN 1 THEN 'Yes' ELSE 'No' END AS 'Published?' "
                    + "FROM Product p LEFT JOIN ProductStore ps ON p.ProductID = ps.ProductID "
                    + "WHERE p.ProductID NOT IN ";

            switch (ddEntity.SelectedValue)
            {
                case "Category":
                    {
                        sql += "(SELECT DISTINCT ProductID FROM ProductCategory) ";
                        break;
                    }
                case "Section":
                    {
                        sql += "(SELECT DISTINCT ProductID FROM ProductSection) ";
                        break;
                    }
                case "Manufacturer":
                    {
                        sql += "(SELECT DISTINCT ProductID FROM ProductManufacturer) ";
                        break;
                    }
            }
            sql += "AND p.IsSystem != 1 "
                    + "AND (ps.StoreId = @StoreId OR 0 = @StoreId)";
                        break;
                    }
            }
            BuildReportFromSql(sql);
        }


        private void BuildEmptyEntitiesReport()
        {
            string sql = String.Empty;

            switch (ddEntity.SelectedValue)
            {
                case "Category":
                {
                    sql = "SELECT Name, CategoryID FROM Category WHERE CategoryID NOT IN (SELECT DISTINCT CategoryID FROM ProductCategory)";
                    break;
                }
                case "Manufacturer":
                {
                    sql = "SELECT Name, ManufacturerID FROM Manufacturer WHERE ManufacturerID NOT IN (SELECT DISTINCT ManufacturerID FROM ProductManufacturer)";
                    break;
                }
                case "Section":
                {
                    sql = "SELECT Name, SectionID FROM Section WHERE SectionID NOT IN (SELECT DISTINCT SectionID FROM ProductSection)";
                    break;
                }
            }

            BuildReportFromSql(sql);
        }

        private void BuildAffiliatesReport()
        {
            parameters.Add(new SqlParameter("@AffiliateID", ddAffiliates.SelectedValue));

            string sql = "SELECT o.OrderNumber, o.OrderDate, o.OrderTotal "
                + "FROM Orders o LEFT JOIN Affiliate a ON o.AffiliateID = a.AffiliateID "
                + "WHERE a.AffiliateID = @AffiliateID "
                + "AND o.OrderDate >= @StartDate "
                + "AND o.OrderDate <= @EndDate "
                + "AND (o.StoreId = @StoreId OR 0 = @StoreId)";

            PopulateDateParameters();
            BuildReportFromSql(sql);
        }

        private void BuildReferralsReport()
        {
            string sql = "SELECT OrderNumber, Referrer, OrderDate FROM Orders "
                + "WHERE Referrer IS NOT Null "
                + "AND CAST(Referrer AS nvarchar(max)) != '' "
                + "AND OrderDate >= @StartDate "
                + "AND OrderDate <= @EndDate "
                + "AND (StoreID = @StoreId OR 0 = @StoreId)";

            PopulateDateParameters();
            BuildReportFromSql(sql);
        }

        private void BuildOrdersByEntityReport()
        {
            string entityClause;
            if (ddCategories.SelectedValue != "0")
            {
                parameters.Add(new SqlParameter("@CategoryID", ddCategories.SelectedValue));
                entityClause = "LEFT JOIN ProductCategory pc ON p.ProductID = pc.ProductId WHERE pc.CategoryID=@CategoryID ";
            }
            else if (ddManufacturers.SelectedValue != "0")
            {
                parameters.Add(new SqlParameter("@ManufacturerID", ddManufacturers.SelectedValue));
                entityClause = "LEFT JOIN ProductManufacturer pf ON p.ProductID = pf.ProductId WHERE pf.ManufacturerID=@ManufacturerID ";
            }
            else if (ddSections.SelectedValue != "0")
            {
                parameters.Add(new SqlParameter("@SectionID", ddSections.SelectedValue));
                entityClause = "LEFT JOIN ProductSection ps ON p.ProductID = ps.ProductId WHERE ps.SectionID=@SectionID ";
            }
            else
            {
                ResetError(AppLogic.GetString("admin.reports.entityerror", cust.LocaleSetting), true);
                return;
            }

            string sql = "SELECT os.OrderedProductName AS 'Product Name', "
                + "os.OrderedProductVariantName AS 'Variant Name', "
                + "o.OrderNumber, "
                + "os.Quantity, "
                + "o.OrderDate AS 'Order Date' "
                + "FROM Orders o "
                + "LEFT JOIN Orders_ShoppingCart os ON o.OrderNumber = os.OrderNumber "
                + "LEFT JOIN Product p ON os.ProductId = p.ProductID "
                + entityClause
                + "AND o.OrderDate >= @StartDate AND o.OrderDate <= @EndDate "
                + "AND (o.StoreId = @StoreId OR 0 = @StoreId) "
                + "ORDER BY os.ProductID";

            PopulateDateParameters();
            BuildReportFromSql(sql);
        }

        private void BuildBestSellersReport()
        {
            string sql = "SELECT TOP 10 b.Name AS 'Product Name', "
                + "b.VariantName AS 'Variant Name', "
                + "b.ChosenSize AS 'Size', "
                + "b.ChosenColor AS 'Color', "
                + "b.NumSales AS 'Number of Sales', "
                + "b.Dollars FROM (SELECT p.Name, "
                    + "pv.Name AS 'VariantName', "
                    + "s.ChosenSize, "
                    + "s.ChosenColor, "
                    + "s.NumSales, "
                    + "s.Dollars "
                + "FROM "
                    + "(SELECT ProductID, "
                    + "VariantID, "
                    + "ChosenSize, "
                    + "ChosenColor, "
                    + "SUM(Quantity) AS NumSales, "
                    + "SUM(OrderedProductPrice) AS Dollars "
                + " FROM Orders_ShoppingCart sc "
                    + "JOIN Orders o ON sc.OrderNumber = o.OrderNumber "
                    + "AND o.OrderDate >= @StartDate "
                    + "AND o.OrderDate <= @EndDate "
                    + "AND (o.StoreId = @StoreId or 0 = @StoreId) "
                    + "GROUP BY ProductID, VariantID, ChosenSize, ChosenColor) s "
                + "JOIN Product p ON s.ProductID = p.ProductID "
                + "JOIN Orders_ShoppingCart os ON p.ProductID = os.ProductID "
                + "JOIN ProductVariant pv ON p.ProductID = pv.ProductID "
                + "GROUP BY p.Name, pv.Name, s.ChosenColor, s.ChosenSize, s.NumSales, s.Dollars) b "
                + "ORDER BY Dollars DESC";

            PopulateDateParameters();
            BuildReportFromSql(sql);
        }

        private void BuildCustomersByProductReport()
        {
            parameters.Add(new SqlParameter("@ProductID", txtProductId.Text.Trim()));

            string sql = "SELECT c.Email, c.FirstName AS 'First Name', c.LastName AS 'Last Name', SUM(os.Quantity) AS 'Total Purchased' "
                + "FROM Customer c LEFT JOIN Orders o ON c.CustomerID = o.CustomerID "
                    + "LEFT JOIN Orders_ShoppingCart os ON os.OrderNumber = o.OrderNumber "
                + "WHERE os.ProductID = @ProductId "
                    + "AND o.OrderDate >= @StartDate "
                    + "AND o.OrderDate <= @EndDate "
                    + "AND (o.StoreID = @StoreId OR 0 = @StoreId) "
                + "GROUP BY c.Email, c.FirstName, c.LastName";

            PopulateDateParameters();
            BuildReportFromSql(sql);
        }

        private void BuildCurrentRecurringProductsReport()
        {
            parameters.Add(new SqlParameter("@CartType", ((int)CartTypeEnum.RecurringCart).ToString()));

            string sql = "SELECT c.email AS 'Customer Email', p.Name AS 'Product Name', pv.Name AS 'Variant', sc.Quantity AS 'QTY', CONVERT(varchar, sc.NextRecurringShipDate, 101) AS 'Next Shipping Date', sc.RecurringIndex AS '# of Past Shipments', "
                + "sc.OriginalRecurringOrderNumber AS 'Original Order Number' "
                    + "FROM ShoppingCart sc "
                        + "LEFT JOIN ProductVariant pv ON sc.VariantID = pv.VariantID "
                        + "LEFT JOIN Product p ON sc.ProductID = p.ProductID "
                        + "LEFT JOIN Customer c ON sc.CustomerID = c.CustomerID "
                    + "WHERE CartType=@CartType "
                    + "ORDER BY sc.NextRecurringShipDate";

            BuildReportFromSql(sql);
        }

        private void BuildCancelledRecurringProductsReport()
        {
            string sql = "SELECT c.Email, p.Name AS 'Product Name', pv.Name AS 'Variant', osc.OrderNumber AS 'Original Order #' "
                + "FROM Orders_ShoppingCart osc "
                    + "LEFT JOIN Product p ON osc.ProductID = p.ProductID "
                    + "LEFT JOIN ProductVariant pv ON osc.VariantID = pv.VariantID "
                    + "LEFT JOIN Customer c ON osc.CustomerID = c.CustomerID "
                + "WHERE osc.SubscriptionInterval IS NOT NULL "
                + "AND pv.IsRecurring = 1 "
                + "AND osc.OrderNumber NOT IN "
                    + "(SELECT OriginalRecurringOrderNumber FROM ShoppingCart WHERE OriginalRecurringOrderNumber IS NOT NULL)";

            BuildReportFromSql(sql);
        }

        private void BuildProductsByCustomerLevelReport()
        {
            parameters.Add(new SqlParameter("@CustomerLevelID", ddCustomerLevel.SelectedValue));
            string sql = "SELECT DISTINCT p.ProductID, p.Name, p.SKU, CASE p.Published WHEN 1 THEN 'Yes' ELSE 'No' END AS 'Published?', "
                + "CASE WHEN @StoreId = 0 THEN 'N/A' ELSE CAST(ps.StoreID AS VARCHAR(10)) END AS 'Store ID' "
                + "FROM Product p "
                    + "LEFT JOIN ProductCustomerLevel pcl ON p.ProductID = pcl.ProductID "
                    + "LEFT JOIN CustomerLevel cl ON pcl.CustomerLevelID = cl.CustomerLevelID "
                    + "LEFT JOIN ProductStore ps ON p.ProductID = ps.ProductID "
                + "WHERE cl.CustomerLevelID = @CustomerLevelID "
                    + "AND (ps.StoreID = @StoreId OR 0 = @StoreId)";

            BuildReportFromSql(sql);
        }

        private void BuildOrdersByDateRangeReport()
        {
            string sql = "SELECT DISTINCT o.OrderNumber,  "
	                        + "o.OrderDate,  "
	                        + "o.OrderSubTotal, " 
	                        + "o.OrderTax,  "
	                        + "o.OrderShippingCosts,  "
	                        + "o.OrderTotal, "
	                        + "SUM(os.Quantity) AS '# Items', "
	                        + "o.BillingFirstName + ' ' + o.BillingLastName AS Name, "
	                        + "o.PaymentMethod, "
	                        + "o.TransactionState, "
	                        + "s.Name AS 'Store' "
                        + "FROM Orders o "
	                        + "LEFT JOIN Orders_ShoppingCart os ON o.OrderNumber = os.OrderNumber "
	                        + "LEFT JOIN Store s ON o.StoreID = s.StoreID "
                        + "WHERE o.OrderDate >= @StartDate "
                            + "AND o.OrderDate <= @EndDate "
	                        + "AND (o.StoreID = @StoreId OR 0 = @StoreId) "
                        + "GROUP BY o.OrderNumber, o.OrderDate, o.OrderSubTotal, o.OrderTax, "
                            + "o.OrderShippingCosts, o.BillingFirstName + ' ' + o.BillingLastName, "
                            + "o.OrderTotal, o.PaymentMethod, o.TransactionState, s.Name "
                        + "ORDER BY OrderNumber ASC";

            PopulateDateParameters();
            BuildReportFromSql(sql);
        }

        private void BuildOrdersByItemReport()
        {
            string sql = "SELECT ProductID, "
                            + "CAST(OrderedProductName AS NVARCHAR(MAX)) AS 'Name',  "
	                        + "CAST(OrderedProductVariantName AS NVARCHAR(MAX)) AS 'Variant Name',  "
	                        + "CAST(OrderedProductSKU AS NVARCHAR(MAX)) AS 'SKU',  "
	                        + "SUM(Quantity) AS '# Sold',  "
	                        + "SUM(OrderedProductPrice) AS 'Total Value',  "
	                        + "s.Name AS 'Store'  "
                        + "FROM Orders_ShoppingCart os  "
	                        + "INNER JOIN Orders o ON os.OrderNumber = o.OrderNumber  "
                            + "INNER JOIN Store s ON o.StoreID = s.StoreID "
                        + "WHERE os.CreatedOn >= @StartDate  "
	                        + "AND os.CreatedOn <= @EndDate  "
	                        + "AND (o.StoreID = @StoreID OR 0 = @StoreID)  "
                        + "GROUP BY ProductID, "
                            + "CAST(OrderedProductName AS NVARCHAR(MAX)),  "
	                        + "s.Name,  "
	                        + "CAST(OrderedProductVariantName AS NVARCHAR(MAX)),  "
	                        + "CAST(OrderedProductSKU AS NVARCHAR(MAX)),  "
	                        + "Quantity,  "
                            + "OrderedProductPrice";
            
            PopulateDateParameters();
            BuildReportFromSql(sql);

        }

        private void BuildSummaryReport()
        {
            SqlConnection dbConn = DB.dbConn();
            dbConn.Open();

            litRegisteredCustomers.Text = DB.GetSqlN("SELECT COUNT(*) AS N FROM Customer WHERE IsRegistered=1", dbConn).ToString();
            litAnonymousCustomers.Text = DB.GetSqlN("SELECT COUNT(*) AS N FROM Customer WHERE IsRegistered=0", dbConn).ToString();
            litNumberOrders.Text = DB.GetSqlN("SELECT COUNT(*) AS N FROM Orders", dbConn).ToString();
            litOrderTotals.Text = Localization.CurrencyStringForDisplayWithoutExchangeRate(Math.Round(DB.GetSqlNDecimal("SELECT SUM(OrderTotal) AS N FROM Orders", dbConn), 2));
            litOrderSubtotals.Text = Localization.CurrencyStringForDisplayWithoutExchangeRate(Math.Round(DB.GetSqlNDecimal("SELECT SUM(OrderSubTotal) AS N FROM Orders", dbConn), 2));
            litOrderShipping.Text = Localization.CurrencyStringForDisplayWithoutExchangeRate(Math.Round(DB.GetSqlNDecimal("SELECT SUM(OrderShippingCosts) AS N FROM Orders", dbConn), 2));
            litOrderTax.Text = Localization.CurrencyStringForDisplayWithoutExchangeRate(Math.Round(DB.GetSqlNDecimal("SELECT SUM(OrderTax) AS N FROM Orders", dbConn), 2));
            litAverageOrder.Text = Localization.CurrencyStringForDisplayWithoutExchangeRate(Math.Round(DB.GetSqlNDecimal("SELECT SUM(OrderTotal)/COUNT(*) AS N FROM Orders", dbConn), 2));

            phReportResults.Visible = false;
            pnlSummary.Visible = true;
            btnSaveReport.Visible = false;
        }
        #endregion        
	}
}
