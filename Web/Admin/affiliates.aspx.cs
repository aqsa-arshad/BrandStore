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
    /// <summary>
    /// Summary description for Affiliates
    /// </summary>
    public partial class Affiliates : AdminPageBase
    {
        protected string selectSQL = "select * from Affiliate";
        protected decimal Balance = System.Decimal.Zero;

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");
            Page.Form.DefaultButton = btnSearch.UniqueID;
            Page.Form.DefaultFocus = txtSearch.ClientID;

            if (!IsPostBack)
            {
                string query = CommonLogic.QueryStringCanBeDangerousContent("searchfor").Trim();
                AppLogic.CheckForScriptTag(query);
                Balance = System.Decimal.Zero;
                if (CommonLogic.QueryStringCanBeDangerousContent("Balance").Length > 0)
                {
                    try
                    {
                        Balance = CommonLogic.QueryStringUSDecimal("Balance");
                    }
                    catch { }
                }
                if (Balance != System.Decimal.Zero)
                {
                    resetError("<b>Showing ALL affiliates with account balances >= " + ThisCustomer.CurrencyString(Balance) + "</b>", false);
                }

                loadTree();

                ViewState["SQLString"] = selectSQL;

                //set page settings
                if (ThisCustomer.ThisCustomerSession.Session("AffiliatesSort").Length == 0)
                {
                    ViewState["Sort"] = "Company";
                }
                else
                {
                    ViewState["Sort"] = ThisCustomer.ThisCustomerSession.Session("AffiliatesSort");
                }
                if (ThisCustomer.ThisCustomerSession.Session("AffiliatesOrder").Length == 0)
                {
                    ViewState["SortOrder"] = "ASC";
                }
                else
                {
                    ViewState["SortOrder"] = ThisCustomer.ThisCustomerSession.Session("AffiliatesOrder");
                }
                if (ThisCustomer.ThisCustomerSession.Session("AffiliatesSearch").Length != 0)
                {
                    query = ThisCustomer.ThisCustomerSession.Session("AffiliatesSearch");
                }
                if (ThisCustomer.ThisCustomerSession.Session("AffiliatesTree").Length != 0)
                {
                    treeMain.FindNode(ThisCustomer.ThisCustomerSession.Session("AffiliatesTree")).Selected = true;
                }

                resultFilter(query);

                txtSearch.Attributes.Add("onKeyPress", "javascript:if (event.keyCode == 13) __doPostBack('btnSearch','')");
            }
        }

        private void loadTree()
        {
            try
            {
                string index = "#ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                treeMain.Nodes.Clear();
                treeMain.Nodes.Add(new TreeNode("All", "All", AppLogic.LocateImageURL("~/App_Themes/Admin_Default/images/icons/dot.gif")));

                foreach (char c in index)
                {
                    TreeNode myNode = new TreeNode();
                    myNode.Text = c.ToString();
                    myNode.ImageUrl = AppLogic.LocateImageURL("~/App_Themes/Admin_Default/images/icons/dot.gif");
                    treeMain.Nodes.Add(myNode);
                }
            }
            catch (Exception ex)
            {
                resetError(ex.ToString(), true);
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            ViewState["IsInsert"] = false;
            gMain.EditIndex = -1;
            resetError("", false);
            gMain.PageIndex = 0;

            resultFilter("");
        }

        protected void resultFilter(string SearchFor)
        {
            String sql = selectSQL + " with (NOLOCK) ";
            String WhereClause = " (parentAffiliateid=0 or parentAffiliateid is null) and deleted=0 ";

            //balance
            if (Balance > Decimal.Zero)
            {
                if (WhereClause.Length != 0)
                {
                    WhereClause += " and ";
                }
                WhereClause += " AffiliateID IN (SELECT T.AAID FROM (SELECT DISCTINCT AA.AffiliateID AS AAID, SUM(AA.Amount) AS AASum FROM AffiliateActivity AA GROUP BY AA.AffiliateID) T WHERE T.AASum => " + Balance + ")";
            }

            //search
            if (SearchFor.Length == 0)
            {
                SearchFor = txtSearch.Text.Trim();
                ThisCustomer.ThisCustomerSession.SetVal("AffiliatesSearch", SearchFor);
            }

            if (SearchFor.Length != 0)
            {
                if (WhereClause.Length != 0)
                {
                    WhereClause += " and ";
                }
                WhereClause += " (Name like " + DB.SQuote("%" + SearchFor + "%") +  " or FirstName like " + DB.SQuote("%" + SearchFor + "%") + " or LastName like " + DB.SQuote("%" + SearchFor + "%") + " or [Company] like " + DB.SQuote("%" + SearchFor + "%") + ")";
            }

            //Node filter
            string Index = "";
            for (int i = 0; i < treeMain.Nodes.Count; i++)
            {
                if (treeMain.Nodes[i].Selected)
                {
                    Index = treeMain.Nodes[i].Value;

                    ThisCustomer.ThisCustomerSession.SetVal("AffiliatesTree", treeMain.Nodes[i].Value);

                    break;
                }
            }

            if (!Index.Equals("All"))
            {
                if (WhereClause.Length != 0)
                {
                    WhereClause += " and ";
                }
                WhereClause += " ([Company] like " + DB.SQuote(Index + "%") + " or Name like " + DB.SQuote(Index + "%") + ")";
            }
            if (Index.Equals("#"))
            {
                if (WhereClause.Length != 0)
                {
                    WhereClause += " and ";
                }
                WhereClause += " (Name like " + DB.SQuote("0%") + ")";
                WhereClause += " OR (Name like " + DB.SQuote("1%") + ")";
                WhereClause += " OR (Name like " + DB.SQuote("2%") + ")";
                WhereClause += " OR (Name like " + DB.SQuote("3%") + ")";
                WhereClause += " OR (Name like " + DB.SQuote("4%") + ")";
                WhereClause += " OR (Name like " + DB.SQuote("5%") + ")";
                WhereClause += " OR (Name like " + DB.SQuote("6%") + ")";
                WhereClause += " OR (Name like " + DB.SQuote("7%") + ")";
                WhereClause += " OR (Name like " + DB.SQuote("8%") + ")";
                WhereClause += " OR (Name like " + DB.SQuote("9%") + ")";
                WhereClause += " OR (Name like " + DB.SQuote("10%") + ")";
            }

            if (WhereClause.Length != 0)
            {
                sql += " where " + WhereClause;
            }

            //set states
            ViewState["SQLString"] = sql.ToString();
            sql += " order by " + ViewState["Sort"].ToString() + " " + ViewState["SortOrder"].ToString();

            ThisCustomer.ThisCustomerSession.SetVal("AffiliatesSort", ViewState["Sort"].ToString());
            ThisCustomer.ThisCustomerSession.SetVal("AffiliatesOrder", ViewState["SortOrder"].ToString());

            //remember page
            if (ThisCustomer.ThisCustomerSession.SessionNativeInt("AffiliatesPage") > 0)
            {
                gMain.PageIndex = ThisCustomer.ThisCustomerSession.SessionNativeInt("AffiliatesPage");
            }

            //build grid
            buildGridData(buildGridData(sql));

            ((TextBox)Form.FindControl("txtSearch")).Text = SearchFor;
        }

        protected void treeMain_SelectedNodeChanged(object sender, EventArgs e)
        {
            ViewState["IsInsert"] = false;
            gMain.EditIndex = -1;
            resetError("", false);
            gMain.PageIndex = 0;

            resultFilter("");
        }

        protected DataTable buildGridData()
        {
            ltNotice.Text = String.Format(AppLogic.GetString("admin.affiliates.Notice", SkinID, LocaleSetting),DB.GetSqlN("select count(affiliateid) as N from affiliate   with (NOLOCK)  where deleted=0").ToString());

            string sql = ViewState["SQLString"].ToString();
            sql += " order by " + ViewState["Sort"].ToString() + " " + ViewState["SortOrder"].ToString();

            return buildGridData(sql);
        }

        protected DataTable buildGridData(string sql)
        {
            DataTable dt = new DataTable();

            dt.Columns.Add("AffiliateID");
            dt.Columns.Add("Name");
            dt.Columns.Add("EMail");
            dt.Columns.Add("URL");
            dt.Columns.Add("TrackingOnly");

            //do the parent null or 0 
            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
                using (IDataReader dr = DB.GetRS(sql, dbconn))
                {
                    while (dr.Read())
                    {
                        //for each affiliate call the subroutine with table and indent
                        DataRow drow = dt.NewRow();
                        drow[0] = DB.RSFieldInt(dr, "AffiliateID");
                        drow[1] = DB.RSField(dr, "Name");
                        drow[2] = DB.RSField(dr, "EMail");
                        drow[3] = DB.RSField(dr, "URL");
                        drow[4] = DB.RSFieldBool(dr, "TrackingOnly");
                        dt.Rows.Add(drow);

                        //subroutine will add the subs calling itself with new affiliate IDs
                        GetAffiliates(dt, DB.RSFieldInt(dr, "AffiliateID"), 1);
                    }
                }
            }
            return dt;
        }

        private void GetAffiliates(DataTable dt, int ParentAffiliateID, int level)
        {
            String Indent = String.Empty;
            for (int i = 0; i < level; i++)
            {
                Indent += "<div style='float:left; text-decoration:none; height: 20px;'>&nbsp;&nbsp;|&nbsp;</div>";
            }

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select AffiliateID, Name, EMail, URL, TrackingOnly from Affiliate   with (NOLOCK)  where parentAffiliateid=" + ParentAffiliateID.ToString() + " and deleted=0 order by DisplayOrder,Name", con))
                {
                    while (rs.Read())
                    {
                        int affiliateID = DB.RSFieldInt(rs, "AffiliateID");
                        string name = DB.RSField(rs, "Name");
                        string email = DB.RSField(rs, "Email");
                        string url = DB.RSField(rs, "URL");
                        bool trackingUrl = DB.RSFieldBool(rs, "TrackingOnly");

                        DataRow drow = dt.NewRow();
                        drow[0] = affiliateID; 
                        drow[1] = Indent + name; 
                        drow[2] = email; 
                        drow[3] = url; 
                        drow[4] = trackingUrl;
                        dt.Rows.Add(drow);

                        if ((DB.GetSqlN("select COUNT(*) AS N from Affiliate   with (NOLOCK)  where parentAffiliateid=" + ParentAffiliateID.ToString() + " and deleted=0")) > 0)
                        {
                            GetAffiliates(dt, affiliateID, level + 1);
                        }
                    }
                }
            }
        }

        protected void buildGridData(DataTable dt)
        {
            gMain.DataSource = dt;
            gMain.DataBind();

            try
            {
                for (int i = 0; i < gMain.HeaderRow.Cells.Count; i++)
                {
                    if (gMain.Columns[i].SortExpression.Equals(ViewState["Sort"].ToString()))
                    {
                        Image arrow = new Image();
                        if (ViewState["SortOrder"].ToString().ToLowerInvariant().Equals("asc"))
                        {
                            arrow.ImageUrl = AppLogic.LocateImageURL("~/App_Themes/Admin_Default/images/icons/asc.gif");
                        }
                        else
                        {
                            arrow.ImageUrl = AppLogic.LocateImageURL("~/App_Themes/Admin_Default/images/icons/desc.gif");
                        }
                        gMain.HeaderRow.Cells[i].Controls.Add(arrow);
                    }
                }
            }
            catch { }
        }

        protected void resetError(string error, bool isError)
        {
            string str = "<font class=\"noticeMsg\">"+ AppLogic.GetString("admin.common.Notice", SkinID, LocaleSetting)+ "</font>&nbsp;&nbsp;&nbsp;";
            if (isError)
                str = "<font class=\"errorMsg\">" + AppLogic.GetString("admin.common.Error", SkinID, LocaleSetting) + "</font>&nbsp;&nbsp;&nbsp;";

            if (error.Length > 0)
                str += error + "";
            else
                str = "";

            ltError.Text = str;
        }

        protected void btnAdd_Click(object sender, EventArgs e)
        {
            Response.Redirect(AppLogic.AdminLinkUrl("editAffiliates.aspx?iden=0"));
        }

        protected void gMain_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DataRowView myrow = (DataRowView)e.Row.DataItem;

                ImageButton ib = (ImageButton)e.Row.FindControl("imgDelete");
                ib.Attributes.Add("onClick", "javascript: return confirm('Confirm Delete?')");

                if (Localization.ParseBoolean(myrow["TrackingOnly"].ToString()) || Localization.ParseNativeInt(myrow["TrackingOnly"].ToString()) == 1)
                {
                    Literal ltName = (Literal)e.Row.FindControl("ltName");
                    ltName.Text = "&nbsp;&nbsp;(Ad Tracking Affiliate)";

                    Literal ltShipTo = (Literal)e.Row.FindControl("ltShipTo");
                    ltShipTo.Text = AppLogic.ro_NotApplicable;
                }
                else
                {
                    Literal ltShipTo = (Literal)e.Row.FindControl("ltShipTo");
                    ltShipTo.Text = AppLogic.AffiliateMailingAddress(Localization.ParseNativeInt(myrow["AffiliateID"].ToString()), "<br/>");
                }

                Literal ltEMail = (Literal)e.Row.FindControl("ltEMail");
                ltEMail.Text = "<a href=\"mailto:" + myrow["EMail"].ToString() + "\">" + myrow["EMail"].ToString() + "</a>";
            }
        }

        protected void gMain_Sorting(object sender, GridViewSortEventArgs e)
        {
            gMain.EditIndex = -1;
            ViewState["Sort"] = e.SortExpression.ToString();
            ViewState["SortOrder"] = (ViewState["SortOrder"].ToString() == "ASC" ? "DESC" : "ASC");
            buildGridData(buildGridData());
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

        protected void deleteRow(int ID)
        {
            DB.ExecuteSQL("update Affiliate set deleted=1, EMail='deleted_' + EMail where AffiliateID=" + ID);

            loadTree();
            buildGridData(buildGridData());
            resetError("Affiliate deleted.", false);
        }

        protected void gMain_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            resetError("", false);
            gMain.PageIndex = e.NewPageIndex;
            gMain.EditIndex = -1;
            buildGridData(buildGridData());
            ThisCustomer.ThisCustomerSession.SetVal("AffiliatesPage", e.NewPageIndex.ToString());
        }
    }
}
