using System.Drawing;
using AjaxControlToolkit.HTMLEditor.ToolbarButton;
using AspDotNetStorefrontCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Data;
using System.Data.SqlClient;
using System.Configuration;


namespace AspDotNetStorefrontAdmin
{
    /// <summary>
    /// customeralerts class to Manage Customer Alerts from Admin Section
    /// </summary>
    public partial class customeralerts : AdminPageBase
    {
        private const int PageSize = 10;

        /// <summary>
        /// Page Load Event
        /// </summary>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                GetCustomerAlerts(1);
            }
        }

        /// <summary>
        /// Get All Customer Alerts for Admin
        /// </summary>
        /// <param name="pageIndex">pageIndex</param>
        private void GetCustomerAlerts(int pageIndex)
        {
            using (var conn = DB.dbConn())
            {
                conn.Open();
                using (var cmd = new SqlCommand("aspdnsf_CustomerAlertSelectAll", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PageIndex", pageIndex);
                    cmd.Parameters.AddWithValue("@PageSize", PageSize);
                    cmd.Parameters.Add("@RecordCount", SqlDbType.Int, 4);
                    cmd.Parameters["@RecordCount"].Direction = ParameterDirection.Output;

                    IDataReader reader = cmd.ExecuteReader();
                    rptCustomerAlerts.DataSource = reader;
                    rptCustomerAlerts.DataBind();
                    
                    reader.Close();
                    conn.Close();

                    var recordCount = Convert.ToInt32(cmd.Parameters["@RecordCount"].Value);
                    PopulatePager(recordCount, pageIndex);
                }
            }
        }

        /// <summary>
        /// Populate Pager w.r.t Current Page Index
        /// </summary>
        /// <param name="recordCount">recordCount</param>
        /// <param name="currentPage">currentPage</param>
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
                        pages.Add(new ListItem("<b>" + i.ToString() + "</b>", i.ToString(), false));
                    else
                        pages.Add(new ListItem(i.ToString(), i.ToString(), true));
                }
            }
            rptPager.DataSource = pages;
            rptPager.DataBind();
        }

        /// <summary>
        /// Repeater Page_Changed Event
        /// </summary>
        protected void Page_Changed(object sender, EventArgs e)
        {
            var pageIndex = int.Parse((sender as LinkButton).CommandArgument);
            GetCustomerAlerts(pageIndex);
        }

        /// <summary>
        /// Repeater ItemCommand Event
        /// </summary>
        protected void rptCustomerAlerts_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int customerAlertID = Convert.ToInt32(e.CommandArgument);
            if (e.CommandName == "Edit")
            {
                Response.Redirect("customeralertedit.aspx?CustomerAlertID=" + customerAlertID);
            }
            else if (e.CommandName == "Delete")
            {
                DeleteCustomerAlert(customerAlertID);
                GetCustomerAlerts(1);
            }
        }

        /// <summary>
        /// Delete All Customer Alert and Customer Alert Statuses
        /// </summary>
        /// <param name="CustomerAlertID">CustomerAlertID</param>
        private void DeleteCustomerAlert(int customerAlertID)
        {
            using (var conn = DB.dbConn())
            {
                conn.Open();
                using (var cmd = new SqlCommand("aspdnsf_CustomerAlertDelete", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CustomerAlertID", customerAlertID);
                    cmd.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);
                    cmd.Parameters.AddWithValue("@ModifiedBy", ThisCustomer.EMail);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Add New Alert Event
        /// </summary>
        protected void btnAddNewAlert_Click(object sender, EventArgs e)
        {
            Response.Redirect("customeralertedit.aspx");
        }
    }
}