using AspDotNetStorefrontCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class CustomerAlerts : System.Web.UI.UserControl
{
    private Customer m_ThisCustomer;
    public Customer ThisCustomer
    {
        get
        {
            if (m_ThisCustomer == null)
                m_ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;

            return m_ThisCustomer;
        }
        set
        {
            m_ThisCustomer = value;
        }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Page.IsPostBack)
        {
            lblCustomerName.Text = "Customer Name: " + ThisCustomer.FullName();
            lblCustomerID.Text = "Customer ID: " + ThisCustomer.CustomerID.ToString();
            GetCustomerAlerts();
        }
    }

    private void GetCustomerAlerts()
    {
        using (var conn = DB.dbConn())
        {
            conn.Open();
            using (var cmd = new SqlCommand("aspdnsf_CustomerAlertSelectByCustomerIDAlertDate", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CustomerID", ThisCustomer.CustomerID);
                cmd.Parameters.AddWithValue("@AlertDate", DateTime.Now);

                IDataReader idr = cmd.ExecuteReader();
                rptCustomerAlerts.DataSource = idr;
                rptCustomerAlerts.DataBind();
            }
        }
    }
    protected void rptCustomerAlerts_ItemCommand(object source, RepeaterCommandEventArgs e)
    {
        int CustomerAlertID = Convert.ToInt32(e.CommandArgument);
        const string readSP = "aspdnsf_CustomerAlertRead";
        const string deleteSP = "aspdnsf_CustomerAlertDelete";

        if (e.CommandName == "Delete")
        {
            UpdateCustomerAlert(CustomerAlertID, deleteSP);
        }
        else if (e.CommandName == "Read")
        {
            UpdateCustomerAlert(CustomerAlertID, readSP);
        }
        GetCustomerAlerts();
    }

    private void UpdateCustomerAlert(int CustomerAlertID, string spName)
    {
        using (var conn = DB.dbConn())
        {
            conn.Open();
            using (var cmd = new SqlCommand(spName, conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CustomerAlertID", CustomerAlertID);
                cmd.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);
                cmd.Parameters.AddWithValue("@ModifiedBy", ThisCustomer.EMail);

                IDataReader idr = cmd.ExecuteReader();
                rptCustomerAlerts.DataSource = idr;
                rptCustomerAlerts.DataBind();
            }
        }
    }
}