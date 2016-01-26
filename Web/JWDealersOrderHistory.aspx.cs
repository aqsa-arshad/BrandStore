using AspDotNetStorefrontCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AspDotNetStorefront
{
    public partial class JWDealersOrderHistory : SkinBase
    {
        protected static int PageCount;
        protected static int CurrentPageNumber;

        /// <summary>
        /// The m_ store loc
        /// </summary>
        public string m_StoreLoc = AppLogic.GetStoreHTTPLocation(true);

        /// <summary>
        /// The number of item shown on the Order history page
        /// </summary>
        private const int PageSize = 4;

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, System.EventArgs e)
        {
            RequireSecurePage();
            RequiresLogin(CommonLogic.GetThisPageName(false) + "?" + CommonLogic.ServerVariables("QUERY_STRING"));

            if (!this.IsPostBack)
            {
                ((System.Web.UI.WebControls.Label)Master.FindControl("lblPageHeading")).Text = "ORDER HISTORY";
                string accountId = Request.QueryString["AccountId"];

                if (!string.IsNullOrEmpty(accountId))
                {
                    List<SFDCSoapClient.Contact> lstContact = AuthenticationSSO.GetSubordinateDealers(accountId);
                    List<int> lstCustomerId = new List<int>();

                    if (lstContact.Count > 0)
                    {
                        foreach(SFDCSoapClient.Contact contact in lstContact)
                        {
                            int customerId = GetCustomerIdbyContactId(contact.Id);
                            if (customerId != 0 && !lstCustomerId.Contains(customerId))
                                lstCustomerId.Add(customerId);
                        }
                        //pnlFundsInformation.Visible = true;
                        //GetCustomerFunds(customer.CustomerID, customer.CustomerLevelID, customer.CustomerLevelName);
                        ((System.Web.UI.WebControls.Label)Master.FindControl("lblPageHeading")).Text = "ORDER HISTORY FOR " + lstContact[0].Account.Name;
                    }
                    hfCustomerID.Value = string.Join(",", lstCustomerId);
                    GetOrders(1, hfCustomerID.Value);
                }
            }
        }

        /// <summary>
        /// GetCustomerIdbyContactId
        /// </summary>
        /// <param name="contactId">contactId</param>
        /// <returns>customerId</returns>
        private int GetCustomerIdbyContactId(string contactId)
        {
            int customerId = 0;
            using (var conn = DB.dbConn())
            {
                conn.Open();
                var query = "select CustomerId from Customer where SFDCQueryParam = '" + contactId.Trim() + "' OR SFDCQueryParam = '" + contactId.Trim().Substring(0, contactId.Length - 3) + "'";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    IDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        int.TryParse(reader["CustomerId"].ToString(), out customerId);
                    }
                }
                return customerId;
            }
        }

        /// <summary>
        /// Get Customer Funds
        /// </summary>
        /// <param name="customerID">CustomerID</param>
        /// <param name="customerLevelID">CustomerLevelID</param>
        /// <param name="customerLevelName">CustomerLevelName</param>
        private void GetCustomerFunds(int customerID, int customerLevelID, string customerLevelName)
        {
            lblCustomerLevel.Text = customerLevelName;
            List<CustomerFund> lstCustomerFund = AuthenticationSSO.GetCustomerFund(customerID, false);

            if (AuthenticationSSO.IsInternalUser(customerLevelID))
            {
                lstCustomerFund.RemoveAll(x => x.FundID != (int)FundType.SOFFunds);
                lblBluBucksHeading.Visible = false;
                lblBluBucks.Visible = false;
            }
            else if (AuthenticationSSO.IsDealerUser(customerLevelID))
            {
                if (AuthenticationSSO.IsTrueBluDealerUser(customerLevelID))
                {
                    decimal BLUBucks = lstCustomerFund.Find(x => x.FundID == (int)FundType.BLUBucks) != null ? lstCustomerFund.Find(x => x.FundID == (int)FundType.BLUBucks).AmountAvailable : 0;
                    lblBluBucks.Text = Math.Round(BLUBucks, 2).ToString();
                }
                else
                {
                    lblBluBucksHeading.Visible = false;
                    lblBluBucks.Visible = false;
                }
                lstCustomerFund.RemoveAll(x => x.FundID == (int)FundType.BLUBucks);
                lstCustomerFund.RemoveAll(x => x.FundID == (int)FundType.SOFFunds);
            }
            rptAllCustomerFunds.DataSource = lstCustomerFund;
            rptAllCustomerFunds.DataBind();
        }

        /// <summary>
        /// Gets the orders.
        /// </summary>
        /// <param name="pageIndex">Index of the page.</param>
        public void GetOrders(int pageIndex, string customerID)
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
                        cmd.Parameters.AddWithValue("@CustomerID", customerID);
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
            lblOrderNotFound.Visible = (rptOrderhistory.Items.Count == 0);
        }

        /// <summary>
        /// Gets the payment status.
        /// </summary>
        /// <param name="PaymentMethod">The payment method.</param>
        /// <param name="CardNumber">The card number.</param>
        /// <param name="TransactionState">State of the transaction.</param>
        /// <param name="decOrderTotal">The decimal order total.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the shipping status.
        /// </summary>
        /// <param name="OrderNumber">The order number.</param>
        /// <param name="ShippedOn">The shipped on.</param>
        /// <returns></returns>
        public string GetShippingStatus(int OrderNumber, string ShippedOn)
        {
            String ShippingStatus = String.Empty;
            if (AppLogic.OrderHasShippableComponents(OrderNumber))
            {
                ShippingStatus = AppLogic.GetString(ShippedOn != "" ? "account.aspx.48" : "account.aspx.52", SkinID, ThisCustomer.LocaleSetting);
            }
            if (AppLogic.OrderHasDownloadComponents(OrderNumber, true))
            {
                ShippingStatus += string.Format("<div><a href=\"downloads.aspx\">{0}</a></div>", AppLogic.GetString("download.aspx.1", SkinID, ThisCustomer.LocaleSetting));
            }
            if (ShippingStatus.Contains("downloads.aspx") && ShippingStatus.Contains("Not Yet Shipped"))
                ShippingStatus = "Not Yet Shipped, Downloadable";
            else if (ShippingStatus.Contains("downloads.aspx"))
            {
                ShippingStatus = "Downloadable";
            }
            return ShippingStatus;
        }

        /// <summary>
        /// Used to set the master page when using template switching or page-based templates
        /// </summary>
        /// <returns>
        /// The name of the template to use.  To utilize this you must override OverrideTemplate
        /// in a page that inherits from SkinBase where you're trying to change the master page
        /// </returns> 
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

        /// <summary>
        /// Populates the pager.
        /// </summary>
        /// <param name="recordCount">The record count.</param>
        /// <param name="currentPage">The current page.</param>
        private void PopulatePager(int recordCount, int currentPage)
        {
            var dblPageCount = (double)((decimal)recordCount / Convert.ToDecimal(PageSize));
            PageCount = (int)Math.Ceiling(dblPageCount);
            var pages = new List<ListItem>();
            if (PageCount > 0)
            {
                if (CurrentPageNumber < currentPage)
                {
                    pages.Add(currentPage > 1
                        ? new ListItem("< " + "Previous", (currentPage - 1).ToString(), true)
                        : new ListItem("< " + "Previous", (currentPage - 1).ToString(), false));
                    pages.Add(new ListItem(currentPage + " of " + PageCount, string.Empty, false));
                    pages.Add(currentPage + 1 <= PageCount
                        ? new ListItem("Next" + " >", (currentPage + 1).ToString(), true)
                        : new ListItem("Next" + " >", string.Empty, false));
                }
                else
                {
                    pages.Add(currentPage - 1 < 1
                        ? new ListItem("< " + "Previous", (currentPage - 1).ToString(), false)
                        : new ListItem("< " + "Previous", (currentPage - 1).ToString(), true));
                    pages.Add(new ListItem(currentPage + " of " + PageCount, string.Empty, false));
                    pages.Add(currentPage + 1 <= PageCount
                        ? new ListItem("Next" + " >", (currentPage + 1).ToString(), true)
                        : new ListItem("Next" + " >", (currentPage + 1).ToString(), false));
                }
                CurrentPageNumber = currentPage;
            }
            rptPager.DataSource = pages;
            rptPager.DataBind();
        }

        /// <summary>
        /// Handles the Changed event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Changed(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty((sender as LinkButton).CommandArgument)) return;
            var pageIndex = int.Parse((sender as LinkButton).CommandArgument);
            GetOrders(pageIndex, hfCustomerID.Value);
        }
    }
}