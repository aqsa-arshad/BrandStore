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
            using (var cmd = new SqlCommand("aspdnsf_CustomerAlertStatusSelectByCustomerIDAlertDate", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CustomerID", ThisCustomer.CustomerID);
                cmd.Parameters.AddWithValue("@CustomerLevelID", ThisCustomer.CustomerLevelID);
                cmd.Parameters.AddWithValue("@AlertDate", DateTime.Now);

                IDataReader idr = cmd.ExecuteReader();
                rptCustomerAlerts.DataSource = idr;
                rptCustomerAlerts.DataBind();
            }
        }
    }
    protected void rptCustomerAlerts_ItemCommand(object source, RepeaterCommandEventArgs e)
    {
        int customerAlertStatusID = Convert.ToInt32(e.CommandArgument);
        const string readSP = "aspdnsf_CustomerAlertStatusRead";
        const string deleteSP = "aspdnsf_CustomerAlertStatusDelete";

        if (e.CommandName == "Delete")
        {
            UpdateCustomerAlert(customerAlertStatusID, deleteSP);
        }
        else if (e.CommandName == "Read")
        {
            UpdateCustomerAlert(customerAlertStatusID, readSP);
        }
        GetCustomerAlerts();
    }

    private void UpdateCustomerAlert(int customerAlertStatusID, string spName)
    {
        using (var conn = DB.dbConn())
        {
            conn.Open();
            using (var cmd = new SqlCommand(spName, conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CustomerAlertStatusID", customerAlertStatusID);
                cmd.ExecuteNonQuery();
            }
        }
    }
}