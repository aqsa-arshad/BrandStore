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
        private static CustomerAlert customerObj;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                AlertDateVal.SelectedDate = DateTime.Now;
                AlertDateVal.LowerBoundDate = DateTime.Now;
                SectionTitle = AppLogic.GetString("admin.menu.CustomerAlerts", SkinID, LocaleSetting);
                BindCustomers();
                GetCustomersPageWise(1);
            }
        }

        private void GetCustomersPageWise(int pageIndex)
        {
            var lstCustomerAlert = new List<CustomerAlert>();
            using (var conn = DB.dbConn())
            {
                using (var cmd = new SqlCommand("aspdnsf_GetCustomersAlertPageWise", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PageIndex", pageIndex);
                    cmd.Parameters.AddWithValue("@PageSize", PageSize);
                    cmd.Parameters.Add("@RecordCount", SqlDbType.Int, 4);
                    cmd.Parameters["@RecordCount"].Direction = ParameterDirection.Output;
                    conn.Open();
                    IDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        lstCustomerAlert.Add(new CustomerAlert()
                        {
                            CustomerAlertID = Convert.ToInt32(reader["CustomerAlertID"]),
                            CustomerID = Convert.ToInt32(reader["CustomerID"]),
                            CustomerName = reader["CustomerName"].ToString(),
                            Title = reader["Title"].ToString(),
                            Description = reader["Description"].ToString(),
                            AlertDate = Convert.ToDateTime(reader["AlertDate"]),
                            IsRead = Convert.ToBoolean(reader["IsRead"]),
                            IsDeleted = Convert.ToBoolean(reader["IsDeleted"]),
                            CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                            ModifiedDate = Convert.ToDateTime(reader["ModifiedDate"]),
                            CreatedBy = reader["CreatedBy"].ToString(),
                            ModifiedBy = reader["ModifiedBy"].ToString()
                        }
                            );
                    }
                    rptCustomerAlerts.DataSource = lstCustomerAlert;
                    rptCustomerAlerts.DataBind();
                    Session["CustomerList"] = lstCustomerAlert;
                    reader.Close();
                    conn.Close();
                    var recordCount = Convert.ToInt32(cmd.Parameters["@RecordCount"].Value);
                    PopulatePager(recordCount, pageIndex);
                }
            }

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
                        pages.Add(new ListItem("<b>" + i.ToString() + "</b>", i.ToString(), false));
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
            GetCustomersPageWise(pageIndex);
        }

        private void BindCustomers()
        {
            using (var conn = DB.dbConn())
            {
                var ds = new DataSet();

                var command = "select CustomerID,FirstName + ' ' + LastName as FullName from Customer where FirstName IS NOT NULL";
                conn.Open();

                var da = new SqlDataAdapter(command, conn);
                da.Fill(ds, "Customer");

                CustomerDropDownList.DataSource = ds.Tables[0];
                CustomerDropDownList.DataTextField = "FullName";
                CustomerDropDownList.DataValueField = "CustomerID";
                CustomerDropDownList.DataBind();
            }
        }

        private void GetCustomerAlerts()
        {
            var lstCustomerAlert = new List<CustomerAlert>();

            using (var conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader reader = DB.GetRS("aspdnsf_CustomerAlertSelectAll", conn))
                {
                    while (reader.Read())
                    {
                        lstCustomerAlert.Add(new CustomerAlert()
                        {
                            CustomerAlertID = Convert.ToInt32(reader["CustomerAlertID"]),
                            CustomerID = Convert.ToInt32(reader["CustomerID"]),
                            CustomerName = reader["CustomerName"].ToString(),
                            Title = reader["Title"].ToString(),
                            Description = reader["Description"].ToString(),
                            AlertDate = Convert.ToDateTime(reader["AlertDate"]),
                            IsRead = Convert.ToBoolean(reader["IsRead"]),
                            IsDeleted = Convert.ToBoolean(reader["IsDeleted"]),
                            CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                            ModifiedDate = Convert.ToDateTime(reader["ModifiedDate"]),
                            CreatedBy = reader["CreatedBy"].ToString(),
                            ModifiedBy = reader["ModifiedBy"].ToString()
                        }
                            );
                    }
                    Session["CustomerList"] = lstCustomerAlert;
                    rptCustomerAlerts.DataSource = lstCustomerAlert;
                    rptCustomerAlerts.DataBind();
                }
            }
        }

        protected void btnAddAlert_Click(object sender, EventArgs e)
        {
            using (SqlConnection con = DB.dbConn())
            {
                using (var cmd = new SqlCommand("aspdnsf_CustomerAlertInsert", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@CustomerID", SqlDbType.Int).Value = CustomerDropDownList.SelectedValue;
                    cmd.Parameters.Add("@Title", SqlDbType.VarChar).Value = TitleVal.Text;
                    cmd.Parameters.Add("@Description", SqlDbType.VarChar).Value = DescriptionVal.Text;
                    cmd.Parameters.Add("@AlertDate", SqlDbType.DateTime).Value = AlertDateVal.VisibleDate;
                    cmd.Parameters.Add("@IsRead", SqlDbType.Bit).Value = 0;
                    cmd.Parameters.Add("@IsDeleted", SqlDbType.Bit).Value = 0;
                    cmd.Parameters.Add("@CreatedDate", SqlDbType.DateTime).Value = DateTime.Now;
                    cmd.Parameters.Add("@ModifiedDate", SqlDbType.DateTime).Value = DateTime.Now;
                    cmd.Parameters.Add("@CreatedBy", SqlDbType.VarChar).Value = ThisCustomer.FirstName + " " + ThisCustomer.LastName;
                    cmd.Parameters.Add("@ModifiedBy", SqlDbType.VarChar).Value = ThisCustomer.FirstName + " " + ThisCustomer.LastName;

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            PageReload();
        }

        protected void btnEdit_Click(object sender, EventArgs e)
        {
            var lstCustomerAlert = (List<CustomerAlert>)Session["CustomerList"];
            var btnEdit = (Button)sender;
            customerObj = lstCustomerAlert.Find(x => x.CustomerAlertID.ToString() == btnEdit.CommandArgument);

            CustomerDropDownList.Enabled = false;
            DescriptionVal.Text = customerObj.Description;
            TitleVal.Text = customerObj.Title;
            AlertDateVal.SelectedDate = customerObj.AlertDate;
            CustomerDropDownList.SelectedItem.Text = customerObj.CustomerName;
            btnAddAlert.Visible = false;
            btnUpdate.Visible = true;
        }

        protected void btnUpdateAlert_Click(object sender, EventArgs e)
        {
            using (SqlConnection con = DB.dbConn())
            {
                using (var cmd = new SqlCommand("aspdnsf_CustomerAlertUpdate", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@CustomerAlertID", SqlDbType.Int).Value = customerObj.CustomerAlertID;
                    cmd.Parameters.Add("@Title", SqlDbType.VarChar).Value = TitleVal.Text;
                    cmd.Parameters.Add("@Description", SqlDbType.VarChar).Value = DescriptionVal.Text;
                    cmd.Parameters.Add("@AlertDate", SqlDbType.DateTime).Value = AlertDateVal.VisibleDate;
                    cmd.Parameters.Add("@IsRead", SqlDbType.Bit).Value = customerObj.IsRead;
                    cmd.Parameters.Add("@IsDeleted", SqlDbType.Bit).Value = customerObj.IsDeleted != true && customerObj.IsDeleted;
                    cmd.Parameters.Add("@CreatedDate", SqlDbType.DateTime).Value = customerObj.CreatedDate;
                    cmd.Parameters.Add("@ModifiedDate", SqlDbType.DateTime).Value = DateTime.Now;
                    cmd.Parameters.Add("@CreatedBy", SqlDbType.VarChar).Value = customerObj.CreatedBy;
                    cmd.Parameters.Add("@ModifiedBy", SqlDbType.VarChar).Value = ThisCustomer.FirstName + " " + ThisCustomer.LastName;

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            btnAddAlert.Visible = true;
            btnUpdate.Visible = false;
            CustomerDropDownList.Enabled = true;
            PageReload();
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            var lstCustomerAlert = (List<CustomerAlert>)Session["CustomerList"];
            var btnEdit = (Button)sender;
            customerObj = lstCustomerAlert.Find(x => x.CustomerAlertID.ToString() == btnEdit.CommandArgument);

            using (SqlConnection con = DB.dbConn())
            {
                using (var cmd = new SqlCommand("aspdnsf_CustomerAlertDelete", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@CustomerAlertID", SqlDbType.Int).Value = customerObj.CustomerAlertID;
                    cmd.Parameters.Add("@ModifiedDate", SqlDbType.DateTime).Value = DateTime.Now;
                    cmd.Parameters.Add("@ModifiedBy", SqlDbType.VarChar).Value = ThisCustomer.FirstName + " " + ThisCustomer.LastName;

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            //GetCustomerAlerts();
            GetCustomersPageWise(1);
        }

        private void PageReload()
        {
            //GetCustomerAlerts();
            GetCustomersPageWise(1);
            TitleVal.Text = string.Empty;
            DescriptionVal.Text = string.Empty;
            AlertDateVal.SelectedDate = DateTime.Now;
        }

    }
}