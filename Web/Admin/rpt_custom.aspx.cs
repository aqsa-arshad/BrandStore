// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Text;
using AspDotNetStorefrontCore;
using System.Data.SqlClient;

namespace AspDotNetStorefrontAdmin
{

    public partial class rpt_custom : AdminPageBase
    {
        private Customer cust;
        private String ActiveReport;

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            cust = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;
            ActiveReport = String.Empty;

            if (!IsPostBack)
            {
                loadReports();
            }
        }

        protected void ddReports_SelectedIndexChanged(object sender, EventArgs e)
        {
            ActiveReport = ddReports.SelectedValue;
            if (ActiveReport.Length != 0 && ActiveReport != "--SELECT REPORT--")
            {
                RunReport(ActiveReport);
            }
        }

        private void RunReport(String ReportName)
        {
            try
            {
                if (ReportName.Length != 0)
                {
                    String sql = string.Empty;
                    using (SqlConnection conn = DB.dbConn())
                    {
                        conn.Open();
                        using (IDataReader rs = DB.GetRS("Select * from CustomReport  with (NOLOCK)  where lower(name)=" + DB.SQuote(ReportName.ToLowerInvariant()), conn))
                        {
                            if (rs.Read())
                            {
                                ResultsPanel.Visible = true;
                                sql = DB.RSField(rs, "SQLCommand");
                                
                            }
                        }
                    }

                    using (SqlConnection conn = DB.dbConn())
                    {
                        conn.Open();
                        using (IDataReader rs = DB.GetRS(sql, conn))
                        {
                            using (DataTable dt = new DataTable())
                            {
                                dt.Load(rs);
                                ResultsGrid.DataSource = dt;
                                ResultsGrid.DataBind();
                            }
                        }
                    }
                }
                else
                {
                    ResultsPanel.Visible = false;
                }
            }
            catch (Exception ex)
            {
                ltError.Text = CommonLogic.GetExceptionDetail(ex, "<br/>");
            }
        }

        private void loadReports()
        {
            ddReports.Items.Clear();
            ddReports.Items.Add("--SELECT REPORT--");

            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS("select distinct Name from CustomReport  with (NOLOCK)  order by Name", conn))
                {
                    while (rs.Read())
                    {
                        ListItem myNode = new ListItem();
                        myNode.Value = DB.RSField(rs, "Name");
                        ddReports.Items.Add(myNode);
                    }
                }
            }
        }

    }
}
