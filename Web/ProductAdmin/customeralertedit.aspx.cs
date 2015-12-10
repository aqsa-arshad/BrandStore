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
                    lstCustomerLevel.DataSource = reader;
                    lstCustomerLevel.DataTextField = "Name";
                    lstCustomerLevel.DataValueField = "CustomerLevelID";
                    lstCustomerLevel.DataBind();
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

                    using (var con = DB.dbConn())
                    {
                        con.Open();

                        // Get Customer Alert Data
                        using (var cmd = new SqlCommand("aspdnsf_CustomerAlertSelectByCustomerAlertID", con))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@CustomerAlertID", customerAlertID);

                            IDataReader reader = cmd.ExecuteReader();
                            if (reader.Read())
                            {
                                hfCustomerAlertID.Value = reader["CustomerAlertID"].ToString();
                                txtTitle.Text = reader["Title"].ToString();
                                txtDescription.Text = reader["Description"].ToString();
                                txtAlertDate.SelectedDate = Convert.ToDateTime(reader["AlertDate"]);

                                pnlEditAlert.Visible = true;
                                lblHeading.Text = "Editing Customer Alert: " + reader["Title"].ToString() + " (ID=" + reader["CustomerAlertID"].ToString() + ")";
                            }
                            reader.Close();
                        }

                        // Get Selected CustomerLevel
                        using (var cmd = new SqlCommand("aspdnsf_CustomerAlertCustomerLevelMappingSelectByCustomerAlertID", con))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@CustomerAlertID", customerAlertID);

                            IDataReader reader = cmd.ExecuteReader();
                            while (reader.Read())
                                lstCustomerLevel.Items.FindByValue(reader["CustomerLevelID"].ToString()).Selected = true;

                            reader.Close();
                        }
                    }                    
                }
                catch (Exception ex)
                    {
                        Response.Write(ex.Message);
                    }
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
                con.Open();
                int CustomerAlertID;

                using (var cmd = new SqlCommand("aspdnsf_CustomerAlertInsert", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Title", txtTitle.Text);
                    cmd.Parameters.AddWithValue("@Description", txtDescription.Text);
                    cmd.Parameters.AddWithValue("@AlertDate", txtAlertDate.VisibleDate);
                    cmd.Parameters.AddWithValue("@IsDeleted", 0);
                    cmd.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                    cmd.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);
                    cmd.Parameters.AddWithValue("@CreatedBy", ThisCustomer.EMail);
                    cmd.Parameters.AddWithValue("@ModifiedBy", ThisCustomer.EMail);
                    cmd.Parameters.Add("@CustomerAlertID", SqlDbType.Int).Direction = ParameterDirection.Output;

                    cmd.ExecuteNonQuery();
                    CustomerAlertID = (int)cmd.Parameters["@CustomerAlertID"].Value; 
                }

                //Insert Selected Customer Level
                foreach (ListItem item in lstCustomerLevel.Items)
                {
                    if (item.Selected)
                    {
                        using (var cmd = new SqlCommand("aspdnsf_CustomerAlertCustomerLevelMappingInsert", con))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            cmd.Parameters.AddWithValue("@CustomerAlertID", CustomerAlertID);
                            cmd.Parameters.AddWithValue("@CustomerLevelID", item.Value);

                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }

            Response.Redirect("customeralerts.aspx");
        }

        /// <summary>
        /// Update Button Event
        /// </summary>
        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            using (SqlConnection con = DB.dbConn())
            {
                con.Open();

                using (var cmd = new SqlCommand("aspdnsf_CustomerAlertUpdate", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CustomerAlertID", hfCustomerAlertID.Value);
                    cmd.Parameters.AddWithValue("@Title", txtTitle.Text);
                    cmd.Parameters.AddWithValue("@Description", txtDescription.Text);
                    cmd.Parameters.AddWithValue("@AlertDate", txtAlertDate.VisibleDate);
                    cmd.Parameters.AddWithValue("@IsDeleted", 0);
                    cmd.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);
                    cmd.Parameters.AddWithValue("@ModifiedBy", ThisCustomer.EMail);

                    cmd.ExecuteNonQuery();
                }

                //Insert Selected Customer Level
                foreach (ListItem item in lstCustomerLevel.Items)
                {
                    if (item.Selected)
                    {
                        using (var cmd = new SqlCommand("aspdnsf_CustomerAlertCustomerLevelMappingInsert", con))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            cmd.Parameters.AddWithValue("@CustomerAlertID", hfCustomerAlertID.Value);
                            cmd.Parameters.AddWithValue("@CustomerLevelID", item.Value);

                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }

            Response.Redirect("customeralerts.aspx");
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