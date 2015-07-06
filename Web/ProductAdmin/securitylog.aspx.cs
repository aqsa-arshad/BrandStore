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
using AspDotNetStorefrontCore;
using System.Data.SqlClient;

namespace AspDotNetStorefrontAdmin
{

    public partial class securitylog : AdminPageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Server.ScriptTimeout = 5000000;

            SectionTitle = "Security Log";

            if (!ThisCustomer.IsAdminSuperUser)
            {
                lterror.Visible = true;
                lterror.Text = AppLogic.GetString("admin.securitylog.1", SkinID, LocaleSetting);
                trLog.Visible = false;
            }
            else
            {
                lterror.Visible = false;
                trLog.Visible = true;
            }
            if (!IsPostBack)
            {
                LoadGridView();
                Security.AgeSecurityLog();
            }
            Page.Form.DefaultButton = Refresh.UniqueID;
        }

        private void LoadGridView()
        {

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select SecurityAction Action,Description,ActionDate Date,UpdatedBy CustomerID,c.EMail from SecurityLog with (NOLOCK) left outer join Customer c with (NOLOCK) on SecurityLog.UpdatedBy=c.CustomerID order by ActionDate desc", con))
                {
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
        protected void GridView1_RowDataBound1(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                foreach (TableCell c in e.Row.Cells)
                {
                    String DD = c.Text;
                    // do in place decrypt:
                    String DDDecrypted = Security.UnmungeString(DD);
                    if (DDDecrypted.StartsWith(Security.ro_DecryptFailedPrefix, StringComparison.InvariantCultureIgnoreCase))
                    {
                        DDDecrypted = c.Text;
                    }
                    c.Text = DDDecrypted;
                    if (c.Text.Length > 70)
                    {
                        c.Text = CommonLogic.WrapString(c.Text, 70, "<br/>");
                    }
                }
            }
        }

        protected void GridView1_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            GridView1.PageIndex = e.NewPageIndex;
            GridView1.EditIndex = -1;
            LoadGridView();
        }

    }
}
