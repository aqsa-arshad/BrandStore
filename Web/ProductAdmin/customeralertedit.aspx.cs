using AspDotNetStorefrontAdmin;
using AspDotNetStorefrontCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AspDotNetStorefrontAdmin
{
    public partial class customeralertedit : AdminPageBase
    {
        /// <summary>
        /// Page Load Event
        /// </summary>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                txtAlertDate.LowerBoundDate = DateTime.Now;
                GetCustomerLevels();
                GetCustomerAlert(Request.QueryString["CustomerAlertID"]);
            }
        }

        /// <summary>
        /// Get All Active Customer Levels for dropdown list
        /// </summary>
        private void GetCustomerLevels()
        {
            using (var conn = DB.dbConn())
            {
                conn.Open();
                var query = "SELECT CustomerLevelID, Name FROM CustomerLevel WHERE Deleted = 0 ORDER BY CustomerLevelID";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandType = CommandType.Text;

                    IDataReader reader = cmd.ExecuteReader();
                    ddlCustomerLevel.DataSource = reader;
                    ddlCustomerLevel.DataTextField = "Name";
                    ddlCustomerLevel.DataValueField = "CustomerLevelID";
                    ddlCustomerLevel.DataBind();
                }
            }
        }

        /// <summary>
        /// Get Customer Alert by CustomerAlertID
        /// </summary>
        /// <param name="CustomerAlertID">CustomerAlertID</param>
        private void GetCustomerAlert(string CustomerAlertID)
        {
            if (!string.IsNullOrEmpty(CustomerAlertID))
            {
                try
                {
                    int customerAlertID = int.Parse(CustomerAlertID);

                    using (var conn = DB.dbConn())
                    {
                        conn.Open();
                        using (var cmd = new SqlCommand("aspdnsf_CustomerAlertSelectByCustomerAlertID", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@CustomerAlertID", customerAlertID);

                            IDataReader reader = cmd.ExecuteReader();
                            if (reader.Read())
                            {
                                hfCustomerAlertID.Value = reader["CustomerAlertID"].ToString();
                                ddlCustomerLevel.SelectedValue = reader["CustomerLevelID"].ToString();
                                ddlCustomerLevel.Enabled = false;
                                txtTitle.Text = reader["Title"].ToString();
                                txtDescription.Text = reader["Description"].ToString();
                                txtAlertDate.SelectedDate = Convert.ToDateTime(reader["AlertDate"]);

                                pnlEditAlert.Visible = true;
                                lblHeading.Text = "Editing Customer Alert: " + reader["Title"].ToString() + " (ID=" + reader["CustomerAlertID"].ToString() +")";
                            }
                        }
                    }
                }
                catch { }
            }
            else
            {
                lblHeading.Text = "Adding New Customer Alert:";
                pnlNewAlert.Visible = true;
            }
        }

        /// <summary>
        /// Add Button Event
        /// </summary>
        protected void btnAddAlert_Click(object sender, EventArgs e)
        {
            using (SqlConnection con = DB.dbConn())
            {
                using (var cmd = new SqlCommand("aspdnsf_CustomerAlertInsert", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@CustomerLevelID", ddlCustomerLevel.SelectedValue);
                    cmd.Parameters.AddWithValue("@Title", txtTitle.Text);
                    cmd.Parameters.AddWithValue("@Description", txtDescription.Text);
                    cmd.Parameters.AddWithValue("@AlertDate", txtAlertDate.VisibleDate);
                    cmd.Parameters.AddWithValue("@IsDeleted", 0);
                    cmd.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                    cmd.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);
                    cmd.Parameters.AddWithValue("@CreatedBy", ThisCustomer.EMail);
                    cmd.Parameters.AddWithValue("@ModifiedBy", ThisCustomer.EMail);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    Response.Redirect("customeralerts.aspx");
                }
            }
        }

        /// <summary>
        /// Update Button Event
        /// </summary>
        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            using (SqlConnection con = DB.dbConn())
            {
                using (var cmd = new SqlCommand("aspdnsf_CustomerAlertUpdate", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CustomerAlertID", hfCustomerAlertID.Value);
                    cmd.Parameters.AddWithValue("@CustomerLevelID", ddlCustomerLevel.SelectedValue);
                    cmd.Parameters.AddWithValue("@Title", txtTitle.Text);
                    cmd.Parameters.AddWithValue("@Description", txtDescription.Text);
                    cmd.Parameters.AddWithValue("@AlertDate", txtAlertDate.VisibleDate);
                    cmd.Parameters.AddWithValue("@IsDeleted", 0);
                    cmd.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);
                    cmd.Parameters.AddWithValue("@ModifiedBy", ThisCustomer.EMail);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    Response.Redirect("customeralerts.aspx");
                }
            }
        }
        
        /// <summary>
        /// Cancel Button Event
        /// </summary>        
        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("customeralerts.aspx");
        }

        /// <summary>
        /// Reset Button Event
        /// </summary>
        protected void btnReset_Click(object sender, EventArgs e)
        {
            GetCustomerAlert(Request.QueryString["CustomerAlertID"]);
        }
}
}