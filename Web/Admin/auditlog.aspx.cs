// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{

    public partial class auditlog : AdminPageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Server.ScriptTimeout = 5000000;

            if (!ThisCustomer.IsAdminUser)
            {
                Response.Redirect(AppLogic.AdminLinkUrl("default.aspx"));
            }
            if (!IsPostBack)
            {
                LoadGridView();
            }
        }

        private void LoadGridView()
        {
            int CustomerID = CommonLogic.QueryStringNativeInt("CustomerID");
            string sWhere = string.Empty;
            if (CustomerID > 0)
            {
                sWhere = "where UpdatedCustomerID=" + CustomerID.ToString();
                Customer TargetCustomer = new Customer(CustomerID, true, true);
                lblCustomer.Text = TargetCustomer.FirstName + " " + TargetCustomer.LastName + " (" + CustomerID.ToString() + ")";
            }
            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
                using(IDataReader rs =DB.GetRS("select ActionDate, CustomerID, UpdatedCustomerID, OrderNumber, Description, Details, PagePath, AuditGroup from AuditLog with (NOLOCK) " + sWhere + " order by ActionDate desc",dbconn))
                {
                    // put in datatable , datareader does not support paging
                    using (DataTable dt = new DataTable())
                    {
                        dt.Load(rs);
                        GridView1.DataSource = dt;
                        GridView1.DataBind();
                    }   
                }
            }
        }

        protected void Refresh_Click(object sender, EventArgs e)
        {
            LoadGridView();
        }
 
        protected void GridView1_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            GridView1.PageIndex = e.NewPageIndex;
            GridView1.EditIndex = -1;
            LoadGridView();
        }

    }
}
