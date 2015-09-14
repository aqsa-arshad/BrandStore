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
    /// <summary>
    /// customeralerts class to Manage Customer Alerts from Admin Section
    /// </summary>
    public partial class customeralerts : AdminPageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            SectionTitle = AppLogic.GetString("admin.menu.CustomerAlerts", SkinID, LocaleSetting);
            GetCustomerAlerts();
        }
        private void GetCustomerAlerts()
        {
            List<CustomerAlert> lstCustomerAlert = new List<CustomerAlert>();

            using (SqlConnection conn = DB.dbConn())
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
                }
                rptCustomerAlerts.DataSource = lstCustomerAlert;
                rptCustomerAlerts.DataBind();
            }
        }
    }
}