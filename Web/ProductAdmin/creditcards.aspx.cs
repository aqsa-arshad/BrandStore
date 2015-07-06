// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using AspDotNetStorefrontCore;
using System.Web.UI.WebControls;
using System.Text;
using System.Data.SqlClient;

namespace AspDotNetStorefrontAdmin
{
    /// <summary>
    /// Summary description for creditcards
    /// </summary>
    public partial class creditcards : AdminPageBase
    {
        protected string selectSQL = "select * from CreditCardType";
        
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            if (!IsPostBack)
            {
                ViewState["Sort"] = "CardType";
                ViewState["SortOrder"] = "ASC";
                ViewState["SQLString"] = selectSQL;

                ShowAddPanel(false);
            }
        }

        protected void buildGridData()
        {
            string sql = ViewState["SQLString"].ToString();
            sql += " order by " + ViewState["Sort"].ToString() + " " + ViewState["SortOrder"].ToString();

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS(sql, dbconn))
                {
                    using (DataTable dt = new DataTable())
                    {
                        dt.Load(rs);
                        gMain.DataSource = dt;
                        gMain.DataBind();
                    }
                }
            }
        }

        protected void resetError(string error, bool isError)
        {
            string str = "<font class=\"noticeMsg\">" + AppLogic.GetString("admin.common.Notice", SkinID, LocaleSetting) + "</font>&nbsp;&nbsp;&nbsp;";
            if (isError)
                str = "<font class=\"errorMsg\">" + AppLogic.GetString("admin.common.Error", SkinID, LocaleSetting) + "</font>&nbsp;&nbsp;&nbsp;";

            if (error.Length > 0)
                str += error + "";
            else
                str = "";

            ltError.Text = str;
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
                ib.Attributes.Add("onClick", "javascript: return confirm('Confirm Delete?')");

                //Click to edit
                if ((e.Row.RowState == DataControlRowState.Normal) || (e.Row.RowState == DataControlRowState.Alternate))
                {
                    e.Row.Attributes.Add("ondblclick", "javascript:__doPostBack('gMain','Edit$" + e.Row.RowIndex + "')");
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
            StringBuilder sql = new StringBuilder(2500);
            sql.Append("delete from creditcardtype where CardTypeID=" + iden.ToString());
            try
            {
                DB.ExecuteSQL(sql.ToString());
                buildGridData();
                resetError("Item Deleted", false);
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format(AppLogic.GetString("admin.common.ErrUpdateDB", SkinID, LocaleSetting),ex.ToString()));
            }
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
                string name = ((TextBox)row.FindControl("txtName")).Text;
                
                StringBuilder sql = new StringBuilder(2500);

                sql.Append("update CreditCardType set ");
                sql.Append("CardType=" + DB.SQuote(name));
                sql.Append(" where CardTypeID=" + iden);

                try
                {
                    DB.ExecuteSQL(sql.ToString());
                    resetError(AppLogic.GetString("admin.common.ItemUpdated", SkinID, LocaleSetting), false);
                    gMain.EditIndex = -1;
                    ViewState["SQLString"] = selectSQL;
                    buildGridData();
                }
                catch (Exception ex)
                {
                    throw new Exception(String.Format(AppLogic.GetString("admin.creditcards.CouldntUpdateDatabase", SkinID, LocaleSetting),sql.ToString(),ex.ToString()));
                }
            }
        }
        protected void gMain_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gMain.EditIndex = e.NewEditIndex;

            buildGridData();
        }

        protected void btnInsert_Click(object sender, EventArgs e)
        {
            resetError("", false);

            ShowAddPanel(true);

            txtName.Text = "";
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            resetError("", false);
            StringBuilder sql = new StringBuilder();

            if (ValidInput())
            {
                string name = txtName.Text.Trim();

                // ok to add them:
                String NewGUID = DB.GetNewGUID();
                sql.Append("insert into CreditCardType(CardTypeGUID,CardType) values(");
                sql.Append(DB.SQuote(NewGUID) + ",");
                sql.Append(DB.SQuote(name));
                sql.Append(")");


                try
                {
                    DB.ExecuteSQL(sql.ToString());
                    resetError(AppLogic.GetString("admin.creditcards.CreditCardAdded", SkinID, LocaleSetting), false);
                    gMain.EditIndex = -1;
                    ShowAddPanel(false);
                }
                catch
                {
                    resetError(AppLogic.GetString("admin.creditcards.ExistingCreditCard", SkinID, LocaleSetting), true);
                    ShowAddPanel(true);
                }
            }
            else
            {
                resetError(AppLogic.GetString("admin.common.PlsInputAllReqFieldsPrompt", SkinID, LocaleSetting), true);
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            resetError("", false);

            ShowAddPanel(false);
        }

        protected bool ValidInput()
        {
            return true;
        }

        protected void ShowAddPanel(bool showAdd)
        {
            if (showAdd)
            {
                pnlAdd.Visible = true;
                pnlGrid.Visible = false;
            }
            else
            {
                pnlAdd.Visible = false;
                pnlGrid.Visible = true;

                buildGridData();
            }
        }
    }
}
