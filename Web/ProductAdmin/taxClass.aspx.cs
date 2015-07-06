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
using System.Text;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{

    public partial class taxClass : AdminPageBase 
    {
        protected string selectSQL = "select * from TaxClass";
        protected Customer cust;

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            cust = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;

            if (!IsPostBack)
            {
                loadScript(false);
                ViewState["Sort"] = "Name";
                ViewState["SortOrder"] = "ASC";
                ViewState["SQLString"] = selectSQL;

                ShowAddPanel(false);
            }
        }

        protected void loadScript(bool load)
        {
            if (load)
            {
                if (AppLogic.NumLocaleSettingsInstalled() > 1)
                {
                    ltScript.Text += "<script type='text/javascript' src='Scripts/tabs.js'></script>";
                }
            }
            else
            {
                ltScript.Text = "";
            }
        }

        protected void buildGridData()
        {
            string sql = ViewState["SQLString"].ToString();
            sql += " order by " + ViewState["Sort"].ToString() + " " + ViewState["SortOrder"].ToString();

            using (DataTable dt = new DataTable())
            {
                using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
                {
                    conn.Open();
                    using (IDataReader rs = DB.GetRS(sql, conn))
                    {
                        dt.Load(rs);

                        if (dt.Rows.Count > 0)
                        {
                            dt.Columns.Add("EditName");
                            foreach (DataRow dr in dt.Rows)
                            {
                                dr["EditName"] = AppLogic.GetLocaleEntryFields(dr[2].ToString(), "Name", false, true, true, "Please enter the " + AppLogic.GetString("AppConfig.CategoryPromptSingular", 0, cust.LocaleSetting).ToLowerInvariant() + " name", 100, 25, 0, 0, false);
                            }
                        }
                    }
                }

                gMain.DataSource = dt;
                gMain.DataBind();
            }
        }
        protected void resetError(string error, bool isError)
        {
            StringBuilder output = new StringBuilder(400);
 
            string str = "<font class=\"noticeMsg\">NOTICE:</font>&nbsp;&nbsp;&nbsp;";
            if (isError)
                str = "<font class=\"errorMsg\">ERROR:</font>&nbsp;&nbsp;&nbsp;";

            if (error.Length > 0)
                str += error + "";
            else
                str = "";

            ltError.Text = str;
            Response.Write(str);

        }

        protected void gMain_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            ViewState["SQLString"] = selectSQL;

            gMain.EditIndex = -1;
            buildGridData();
        }
        protected void gMain_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                ImageButton ib = (ImageButton)e.Row.FindControl("imgDelete");
                ib.Attributes.Add("onClick", "javascript: return confirm('Confirm Delete? This will delete all TaxRates for this Tax Class also!')");

                //Click to edit
                if ((e.Row.RowState == DataControlRowState.Normal) || (e.Row.RowState == DataControlRowState.Alternate))
                {
                    e.Row.Attributes.Add("ondblclick", "javascript:__doPostBack('gMain','Edit$" + e.Row.RowIndex + "')");
                }

                //Localization
                if ((e.Row.RowState == DataControlRowState.Normal) || (e.Row.RowState == DataControlRowState.Alternate))
                {
                    e.Row.Cells[2].Text = XmlCommon.GetLocaleEntry(((Literal)e.Row.FindControl("ltName")).Text, cust.LocaleSetting, false);
                }
            }
        }
        protected void gMain_Sorting(object sender, GridViewSortEventArgs e)
        {
            gMain.EditIndex = -1;
            ViewState["Sort"] = e.SortExpression.ToString();
            ViewState["SortOrder"] = (ViewState["SortOrder"].ToString() == "ASC" ? "DESC" : "ASC");
            buildGridData();
        }
        protected void gMain_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            resetError("", false);

            if (e.CommandName == "DeleteItem")
            {
                gMain.EditIndex = -1;
                int iden = Localization.ParseNativeInt(e.CommandArgument.ToString());
                deleteRow(iden);
            }
        }
        
        protected void deleteRow(int iden)
        {
            DB.ExecuteSQL("DELETE FROM StateTaxRate WHERE TaxClassID=" + iden);
            DB.ExecuteSQL("DELETE FROM CountryTaxRate WHERE TaxClassID=" + iden);
            DB.ExecuteSQL("DELETE FROM ZipTaxRate WHERE TaxClassID=" + iden);
            DB.ExecuteSQL("DELETE FROM TaxClass where TaxClassID=" + iden.ToString());

            buildGridData();
            resetError("Item Deleted", false);
        }

        protected void gMain_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            resetError("", false);
            gMain.PageIndex = e.NewPageIndex;
            gMain.EditIndex = -1;
            buildGridData();
        }
        protected void gMain_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            GridViewRow row = gMain.Rows[e.RowIndex];

            if (row != null)
            {
                string iden = row.Cells[1].Text.ToString();
                string name = AppLogic.FormLocaleXml("Name");

                // see if this appconfig already exists:
                String countSql = "SELECT COUNT(Name) AS N FROM TaxCLASS WITH (NOLOCK) WHERE TaxClassID<>@CountTaxClassID AND LOWER(Name)=@CountName";
                SqlParameter[] countSpa = { new SqlParameter("@CountTaxClassID", iden),
                                              new SqlParameter("@CountName", name) };

                int N = DB.GetSqlN(countSql, countSpa);
                if (N != 0)
                {
                    resetError("There is already another tax class with that name.", true);
                    return;
                }

                String updateSql = "UPDATE TaxClass SET Name=@UpdateName, TaxCode=@TaxCode, DisplayOrder=@DisplayOrder WHERE TaxClassID=@UpdateTaxClassID";

                SqlParameter[] updateSpa = { new SqlParameter("@UpdateName", name),
                                         new SqlParameter("@TaxCode", ((TextBox)row.FindControl("txtTaxCode")).Text),
                                         new SqlParameter("@DisplayOrder", ((TextBox)row.FindControl("txtDisplayOrder")).Text),
                                         new SqlParameter("@UpdateTaxClassID", iden) };

                try
                {
                    DB.ExecuteSQL(updateSql, updateSpa);
                    resetError("Item updated", false);
                    gMain.EditIndex = -1;
                    ViewState["SQLString"] = selectSQL;
                    buildGridData();
                }
                catch (Exception ex)
                {
                    throw new Exception("Couldn't update database: " + updateSql.ToString() + ex.ToString());
                }
            }
        }
        protected void gMain_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gMain.EditIndex = e.NewEditIndex;
            
            loadScript(true);

            buildGridData();
        }
        protected void btnInsert_Click(object sender, EventArgs e)
        {
            resetError("", false);

            gMain.EditIndex = -1;
            ShowAddPanel(true);

            txtTaxCode.Text = "";
            ltTaxClass.Text = AppLogic.GetLocaleEntryFields("", "Name", false, true, true, "Please enter the Tax Class", 100, 25, 0, 0, false);
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            resetError("", false);
            String sql = "INSERT INTO TaxClass(TaxClassGUID, Name, TaxCode, DisplayOrder) VALUES(@Guid, @Name, @TaxCode, @DisplayOrder)";

            if (Page.IsValid)
            {
                if (ValidInput())
                {
                    // ok to add them:
                    string tax = txtTaxCode.Text.Trim();
                    string displayOrder = txtDisplayOrder.Text.Trim();

                    SqlParameter[] spa = { new SqlParameter("@Guid", DB.GetNewGUID()),
                                             new SqlParameter("@Name", AppLogic.FormLocaleXml("Name")),
                                             new SqlParameter("@TaxCode", txtTaxCode.Text.Trim()),
                                             new SqlParameter("@DisplayOrder", txtDisplayOrder.Text.Trim()) };

                    try
                    {
                        DB.ExecuteSQL(sql, spa);
                        resetError(AppLogic.GetString("admin.taxclass.added", ThisCustomer.LocaleSetting), false);
                        ShowAddPanel(false);
                    }
                    catch
                    {
                        resetError(AppLogic.GetString("admin.taxclass.adderror", ThisCustomer.LocaleSetting), true);
                        ShowAddPanel(true);
                    }
                }
                else
                {
                    resetError(AppLogic.GetString("admin.taxclass.requirederror", ThisCustomer.LocaleSetting), true);
                }
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            resetError("", false);

            ShowAddPanel(false);
        }

        protected bool ValidInput()
        {
            if (AppLogic.FormLocaleXml("Name").Equals("<ml></ml>"))
            {
                return false;
            }

            return true;
        }

        protected void ShowAddPanel(bool showAdd)
        {
            if (showAdd)
            {
                loadScript(true);
                pnlAdd.Visible = true;
                pnlGrid.Visible = false;
            }
            else
            {
                loadScript(false);
                pnlAdd.Visible = false;
                pnlGrid.Visible = true;

                buildGridData();
            }
        }
    }
}
