// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	/// <summary>
	/// Summary description for producttypes.
	/// </summary>
    public partial class producttypes : AdminPageBase
	{
        protected string selectSQL = "select * from ProductType ";
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

                BuildGridData();
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

        protected void BuildGridData()
        {
            string sql = ViewState["SQLString"].ToString();
            sql += " order by " + ViewState["Sort"].ToString() + " " + ViewState["SortOrder"].ToString() + ", DisplayOrder";            
            
            using (DataTable dt = new DataTable())
            {
                using (SqlConnection conn = DB.dbConn())
                {
                    conn.Open();
                    using (IDataReader rs = DB.GetRS(sql, conn))
                    {
                        dt.Load(rs);
                    }
                }

                // add custom column
                dt.Columns.Add("EditName");

                // populate custom column
                foreach(DataRow dr in dt.Rows)
                {
                    dr["EditName"] = AppLogic.GetLocaleEntryFields(dr[2].ToString(), "Name", false, true, true, "Please enter the Product Type name", 100, 25, 0, 0, false);
                }

                // bind
                gMain.DataSource = dt;
                gMain.DataBind();
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

            ltError.Text = str ;
        }

        protected void gMain_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            if (Localization.ParseBoolean(ViewState["IsInsert"].ToString()))
            {
                GridViewRow row = gMain.Rows[e.RowIndex];
                if (row != null)
                {
                    int iden = Localization.ParseNativeInt(row.Cells[1].Text.ToString());
                }
            }

            ViewState["IsInsert"] = false;
            ViewState["SQLString"] = selectSQL;

            gMain.EditIndex = -1;
            BuildGridData();
        }
        protected void gMain_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                ImageButton ib = (ImageButton)e.Row.FindControl("imgDelete");
                ib.Attributes.Add("onClick", "javascript: return confirm('" + AppLogic.GetString("admin.common.ConfirmDeletion", SkinID, LocaleSetting) + "')");

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
            ViewState["IsInsert"] = false;
            gMain.EditIndex = -1;
            ViewState["Sort"] = e.SortExpression.ToString();
            ViewState["SortOrder"] = (ViewState["SortOrder"].ToString() == "ASC" ? "DESC" : "ASC");
            BuildGridData();
        }
        protected void gMain_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            resetError("", false);

            if (e.CommandName == "DeleteItem")
            {
                ViewState["IsInsert"] = false;
                gMain.EditIndex = -1;
                int iden = Localization.ParseNativeInt(e.CommandArgument.ToString());
                deleteRow(iden, true);
            }
        }
        
        protected void deleteRow(int iden, bool refreshGrid)
        {
            StringBuilder sql = new StringBuilder(2500);
            sql.Append("delete from ProductType where ProductTypeID=" + iden);
            try
            {
                DB.ExecuteSQL(sql.ToString());
                if (refreshGrid)
                {
                    BuildGridData();
                }
                resetError("Item Deleted", false);
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format(AppLogic.GetString("admin.common.ErrDeleteDB", SkinID, LocaleSetting),ex.ToString()));
            }
        }

        protected void gMain_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            ViewState["IsInsert"] = false;
            resetError("", false);
            gMain.PageIndex = e.NewPageIndex;
            gMain.EditIndex = -1;
            BuildGridData();
        }
        protected void gMain_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            bool IsInserted = (bool)ViewState["IsInsert"];
            GridViewRow row = gMain.Rows[e.RowIndex];

            if (row != null)
            {
                string iden = row.Cells[1].Text.ToString();
                string name = AppLogic.FormLocaleXml("Name");

                // see if this appconfig already exists:
                int N = DB.GetSqlN("select count(Name) as N from producttype   with (NOLOCK)  where producttypeid<>" + iden + " and lower(Name)=" + DB.SQuote(name.ToLowerInvariant()));
                if (N != 0)
                {
                    resetError(AppLogic.GetString("admin.producttypes.ExistingProductType", SkinID, LocaleSetting), true);
                    return;
                }

                StringBuilder sql = new StringBuilder();

                if (!IsInserted)
                {
                    sql.Append("update producttype set ");
                    sql.Append("Name=" + DB.SQuote(name));
                    sql.Append(" where producttypeID=" + iden);
                }
                else
                {
                    String NewGUID = DB.GetNewGUID();
                    sql.Append("insert into producttype(producttypeGUID,Name) values(");
                    sql.Append(DB.SQuote(NewGUID) + ",");
                    sql.Append(DB.SQuote(name));
                    sql.Append(")");
                }

                try
                {
                    DB.ExecuteSQL(sql.ToString());
                    resetError(AppLogic.GetString("admin.common.ItemUpdated", SkinID, LocaleSetting), false);
                    gMain.EditIndex = -1;
                    ViewState["SQLString"] = selectSQL;
                    BuildGridData();
                }
                catch (Exception ex)
                {
                    throw new Exception(String.Format(AppLogic.GetString("admin.creditcards.CouldntUpdateDatabase", SkinID, LocaleSetting),sql.ToString(),ex.ToString()));
                }
            }
        }
        protected void gMain_RowEditing(object sender, GridViewEditEventArgs e)
        {
            ViewState["IsInsert"] = false;
            gMain.EditIndex = e.NewEditIndex;

            loadScript(true);

            BuildGridData();
        }
        protected void btnInsert_Click(object sender, EventArgs e)
        {
            ViewState["IsInsert"] = false;
            gMain.EditIndex = -1;
            StringBuilder sql = new StringBuilder();

            // see if this name is already there:
            string name = "NEW Product Type";
            int N = DB.GetSqlN("select count(producttypeid) as N from producttype   with (NOLOCK)  where lower(Name)=" + DB.SQuote(name.ToLowerInvariant()));
            if (N != 0)
            {
                name = name + DateTime.Now.Millisecond;
            }

            // ok to add them:
            String NewGUID = DB.GetNewGUID();
            sql.Append("insert into producttype(producttypeGUID,Name) values(");
            sql.Append(DB.SQuote(NewGUID) + ",");
            sql.Append(DB.SQuote(name));
            sql.Append(")");

            try
            {
                DB.ExecuteSQL(sql.ToString());

                ViewState["SQLString"] = selectSQL;
                ViewState["Sort"] = "producttypeID";
                ViewState["SortOrder"] = "DESC";

                gMain.EditIndex = 0;
                loadScript(true);

                int productTypeID = 0;
                BuildGridData();

                string queryNewAdded = selectSQL + " where [Name]=" + DB.SQuote(name) + " ";

                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader reader = DB.GetRS(queryNewAdded, con))
                    {
                        // we're assuming the topmost record is the newest based on descending order
                        if (reader.Read())
                        {
                            productTypeID = DB.RSFieldInt(reader, "ProductTypeID");
                        }
                    }
                }

                deleteRow(productTypeID, false);

                resetError(AppLogic.GetString("admin.producttypes.ProductTypeAdded", SkinID, LocaleSetting), false);
                ViewState["IsInsert"] = true;
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format(AppLogic.GetString("admin.common.ErrUpdateDB", SkinID, LocaleSetting),ex.ToString()));
            }
        }
	}
}
