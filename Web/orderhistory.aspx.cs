// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Data;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.Expressions;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for orderhistory.
    /// </summary>
    public partial class orderhistory : SkinBase
    {
        public string m_StoreLoc = AppLogic.GetStoreHTTPLocation(true);
        private const int PageSize = 4;

        protected void Page_Load(object sender, System.EventArgs e)
        {
            if (ThisCustomer.CustomerLevelID == 4 || ThisCustomer.CustomerLevelID == 5 || ThisCustomer.CustomerLevelID == 6)
            {
                ((System.Web.UI.WebControls.Label)Master.FindControl("lblPageHeading")).Text = "ORDER HISTORY FOR " + GetDealerName(ThisCustomer.CustomerID);
            }
            if (!this.IsPostBack)
            {
                GetOrders(1);
            }
        }

        public void GetOrders(int pageIndex)
        {
            string[] trxStates = { AppLogic.ro_TXStateAuthorized, AppLogic.ro_TXStateCaptured, AppLogic.ro_TXStatePending };
            try
            {
                using (var conn = DB.dbConn())
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("aspdnsf_GetAllOrders", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@PageIndex", pageIndex);
                        cmd.Parameters.AddWithValue("@PageSize", PageSize);
                        cmd.Parameters.Add("@RecordCount", SqlDbType.Int, 4);
                        cmd.Parameters["@RecordCount"].Direction = ParameterDirection.Output;
                        cmd.Parameters.AddWithValue("@TransactionState", String.Join(",", trxStates));
                        cmd.Parameters.AddWithValue("@CustomerID", ThisCustomer.CustomerID);
                        cmd.Parameters.AddWithValue("@AllowCustomerFiltering",
                            CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowCustomerFiltering") == true, 1, 0));
                        cmd.Parameters.AddWithValue("@StoreID", AppLogic.StoreID());

                        IDataReader reader = cmd.ExecuteReader();
                        rptOrderhistory.DataSource = reader;
                        rptOrderhistory.DataBind();

                        reader.Close();
                        conn.Close();

                        var recordCount = Convert.ToInt32(cmd.Parameters["@RecordCount"].Value);
                        PopulatePager(recordCount, pageIndex);
                    }
                }
            }
            catch (Exception ex)
            {
                SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                ex.Message + ((ex.InnerException != null && string.IsNullOrEmpty(ex.InnerException.Message)) ? " :: " + ex.InnerException.Message : ""),
                MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
            }
            accountaspx55.Visible = (rptOrderhistory.Items.Count == 0);
        }

        public string GetPaymentStatus(string PaymentMethod, string CardNumber, string TransactionState, object decOrderTotal)
        {
            decimal OrderTotal = Convert.ToDecimal(decOrderTotal);
            if (OrderTotal == Decimal.Zero)
            {
                return AppLogic.GetString("order.cs.16", SkinID, ThisCustomer.LocaleSetting);
            }
            String PaymentStatus = String.Empty;
            if (PaymentMethod.Length != 0)
            {
                PaymentStatus = AppLogic.GetString("account.aspx.43", SkinID, ThisCustomer.LocaleSetting) + " " + PaymentMethod.Replace(AppLogic.ro_PMMicropay, AppLogic.GetString("account.aspx.11", SkinID, ThisCustomer.LocaleSetting)) + "";
            }
            else
            {
                PaymentStatus = AppLogic.GetString("account.aspx.43", SkinID, ThisCustomer.LocaleSetting) + " " + CommonLogic.IIF(CardNumber.StartsWith(AppLogic.ro_PMPayPal, StringComparison.InvariantCultureIgnoreCase), AppLogic.GetString("account.aspx.44", SkinID, ThisCustomer.LocaleSetting), AppLogic.GetString("account.aspx.45", SkinID, ThisCustomer.LocaleSetting)) + "";
            }
            PaymentStatus += CommonLogic.IIF(TransactionState == AppLogic.ro_TXStatePending, "<span style=\"color:red;\">" + TransactionState + "</span>", TransactionState);
            return PaymentStatus;
        }

        public string GetShippingStatus(int OrderNumber, string ShippedOn, string ShippedVIA, string ShippingTrackingNumber, string TransactionState, string DownloadEMailSentOn)
        {
            String ShippingStatus = String.Empty;
            if (AppLogic.OrderHasShippableComponents(OrderNumber))
            {
                if (ShippedOn != "")
                {
                    ShippingStatus = AppLogic.GetString("account.aspx.48", SkinID, ThisCustomer.LocaleSetting);
                    if (ShippedVIA.Length != 0)
                    {
                        ShippingStatus += " " + AppLogic.GetString("account.aspx.49", SkinID, ThisCustomer.LocaleSetting) + " " + ShippedVIA;
                    }

                    ShippingStatus += " " + AppLogic.GetString("account.aspx.50", SkinID, ThisCustomer.LocaleSetting) + " " + Localization.ParseNativeDateTime(ShippedOn).ToString(new CultureInfo(ThisCustomer.LocaleSetting));
                    if (ShippingTrackingNumber.Length != 0)
                    {
                        ShippingStatus += " " + AppLogic.GetString("account.aspx.51", SkinID, ThisCustomer.LocaleSetting) + " ";

                        String TrackURL = Shipping.GetTrackingURL(ShippingTrackingNumber);
                        if (TrackURL.Length != 0)
                        {
                            ShippingStatus += "<a href=\"" + TrackURL + "\" target=\"_blank\">" + ShippingTrackingNumber + "</a>";
                        }
                        else
                        {
                            ShippingStatus += ShippingTrackingNumber;
                        }
                    }
                }
                else
                {
                    ShippingStatus = AppLogic.GetString("account.aspx.52", SkinID, ThisCustomer.LocaleSetting);
                }
            }
            if (AppLogic.OrderHasDownloadComponents(OrderNumber, true))
            {
                ShippingStatus += string.Format("<div><a href=\"downloads.aspx\">{0}</a></div>", AppLogic.GetString("download.aspx.1", SkinID, ThisCustomer.LocaleSetting));
            }
            if (ShippingStatus.Contains("downloads.aspx") && ShippingStatus.Contains("Not Yet Shipped"))
                ShippingStatus = "Not Yet Shipped Downloadable";
            else if (ShippingStatus.Contains("downloads.aspx"))
            {
                ShippingStatus = "Downloadable";
            }
            return ShippingStatus;
        }

        protected override string OverrideTemplate()
        {
            var masterHome = AppLogic.HomeTemplate();
            if (masterHome.Trim().Length == 0)
            {
                masterHome = "JeldWenTemplate";
            }
            if (masterHome.EndsWith(".ascx"))
            {
                masterHome = masterHome.Replace(".ascx", ".master");
            }
            if (!masterHome.EndsWith(".master", StringComparison.OrdinalIgnoreCase))
            {
                masterHome = masterHome + ".master";
            }
            if (!CommonLogic.FileExists(CommonLogic.SafeMapPath("~/App_Templates/Skin_" + SkinID + "/" + masterHome)))
            {
                masterHome = "JeldWenTemplate";
            }
            return masterHome;
        }

        private void PopulatePager(int recordCount, int currentPage)
        {
            var dblPageCount = (double)((decimal)recordCount / Convert.ToDecimal(PageSize));
            var pageCount = (int)Math.Ceiling(dblPageCount);
            var pages = new List<ListItem>();
            if (pageCount > 0)
            {
                for (int i = 1; i <= pageCount; i++)
                {
                    if (i == currentPage)
                    {
                        //pages.Add(new ListItem("<b>" + i.ToString() + "</b>", i.ToString(), false));
                        pages.Add(new ListItem("<b>" + i.ToString() + "</b>", i.ToString(), false));
                    }
                    else
                        pages.Add(new ListItem(i.ToString(), i.ToString(), true));
                }
            }
            rptPager.DataSource = pages;
            rptPager.DataBind();
        }

        protected void Page_Changed(object sender, EventArgs e)
        {
            var pageIndex = int.Parse((sender as LinkButton).CommandArgument);
            GetOrders(pageIndex);
        }

        private static string GetDealerName(int customerId)
        {
            var customerName = string.Empty;
            try
            {
                using (var conn = DB.dbConn())
                {
                    conn.Open();
                    var query = "select FirstName + ', ' + LastName as CustomerName from Customer where CustomerID = " +
                                customerId;
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        IDataReader reader = cmd.ExecuteReader();
                        if (reader.Read())
                            customerName = reader["CustomerName"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                SysLog.LogMessage(
                    System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " +
                    System.Reflection.MethodBase.GetCurrentMethod().Name,
                    ex.Message +
                    ((ex.InnerException != null && string.IsNullOrEmpty(ex.InnerException.Message))
                        ? " :: " + ex.InnerException.Message
                        : ""),
                    MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
            }
            return customerName;
        }

    }
}
