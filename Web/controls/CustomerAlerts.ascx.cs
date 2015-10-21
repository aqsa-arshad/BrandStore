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

    /// <summary>
    /// Page Load Event
    /// </summary>
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Page.IsPostBack)
        {
            GetCustomerAlerts();
        }
    }

    /// <summary>
    /// Get All Customer Alerts for Admin
    /// </summary>
    private void GetCustomerAlerts()
    {
        try
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
        catch (Exception ex)
        {
            SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
            ex.Message + ((ex.InnerException != null && string.IsNullOrEmpty(ex.InnerException.Message)) ? " :: " + ex.InnerException.Message : ""),
            MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
        }
    }

    /// <summary>
    /// Repeater ItemCommand Event
    /// </summary>
    protected void rptCustomerAlerts_ItemCommand(object source, RepeaterCommandEventArgs e)
    {
        try
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
        catch (Exception ex)
        {
            SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
            ex.Message + ((ex.InnerException != null && string.IsNullOrEmpty(ex.InnerException.Message)) ? " :: " + ex.InnerException.Message : ""),
            MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
        }
    }

    /// <summary>
    /// Update Customer Alert
    /// </summary>
    /// <param name="customerAlertStatusID">customerAlertStatusID</param>
    /// <param name="spName">spName</param>
    private void UpdateCustomerAlert(int customerAlertStatusID, string spName)
    {
        try
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
        catch (Exception ex)
        {
            SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
            ex.Message + ((ex.InnerException != null && string.IsNullOrEmpty(ex.InnerException.Message)) ? " :: " + ex.InnerException.Message : ""),
            MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
        }

    }


}