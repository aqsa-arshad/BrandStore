// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{

    public partial class giftcards : AdminPageBase
    {
        protected string selectSQL = "SELECT G.*, C.FirstName, C.LastName from GiftCard G with (NOLOCK) LEFT OUTER JOIN Customer C with (NOLOCK) ON G.PurchasedByCustomerID = C.CustomerID ";

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            //temporarily hide, these are not a supported feature yet
            divForFilters.Visible = false;


            if (!IsPostBack)
            {
                string query = CommonLogic.QueryStringCanBeDangerousContent("searchfor").Trim();

                loadGroups();
                ViewState["Sort"] = "G.CreatedOn";
                ViewState["SortOrder"] = "DESC";
                ViewState["SQLString"] = selectSQL;

                if (query.Length > 0)
                {
                    resultFilter(query);
                }
                else
                {
                    retrieveFilters();
                    resultFilter("");
                }

                txtSearch.Attributes.Add("onKeyPress", "javascript:if (event.keyCode == 13) __doPostBack('btnSearch','')");
            }
        }

        private void loadGroups()
        {
            try
            {
                ddStatus.Items.Clear();//hard coded
                ddTypes.Items.Clear();//from giftcard class

                // types
                ddTypes.Items.Add(new ListItem(AppLogic.GetString("admin.giftcards.ShowAll", SkinID, LocaleSetting), "0"));
                ddTypes.Items.Add(new ListItem(AppLogic.GetString("admin.common.Certificate", SkinID, LocaleSetting), ((int)GiftCardTypes.CertificateGiftCard).ToString()));
                ddTypes.Items.Add(new ListItem(AppLogic.GetString("admin.common.E-Mail", SkinID, LocaleSetting), ((int)GiftCardTypes.EMailGiftCard).ToString()));
                ddTypes.Items.Add(new ListItem(AppLogic.GetString("admin.common.Physical", SkinID, LocaleSetting), ((int)GiftCardTypes.PhysicalGiftCard).ToString()));

                //status
                ddStatus.Items.Add(new ListItem(AppLogic.GetString("admin.giftcards.ShowAll", SkinID, LocaleSetting), "0"));
                ddStatus.Items.Add(new ListItem(AppLogic.GetString("admin.giftcards.Expired", SkinID, LocaleSetting), "1"));
                ddStatus.Items.Add(new ListItem(AppLogic.GetString("admin.common.Active", SkinID, LocaleSetting), "2"));
                ddStatus.Items.Add(new ListItem(AppLogic.GetString("admin.giftcards.UsedOnce", SkinID, LocaleSetting), "3"));
                ddStatus.Items.Add(new ListItem(AppLogic.GetString("admin.common.Disabled", SkinID, LocaleSetting), "4"));

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
            String sql = selectSQL + " ";
            String temp = "";
            String WhereClause = String.Empty;

            //search
            if (SearchFor.Length == 0)
            {
                SearchFor = txtSearch.Text.Trim();
                if (SearchFor.Length != 0)
                {
                    if (ddSearch.SelectedValue == "1")
                    {
                        temp = " EMailTo LIKE " + DB.SQuote("%" + SearchFor + "%") + " ";
                    }
                    else if (ddSearch.SelectedValue == "2")
                    {
                        temp = " (LastName LIKE " + DB.SQuote("%" + SearchFor + "%") + " OR FirstName LIKE " + DB.SQuote("%" + SearchFor + "%") + ")";
                    }
                    else if (ddSearch.SelectedValue == "3")
                    {
                        temp = " SerialNumber LIKE " + DB.SQuote("%" + SearchFor + "%") + " ";
                    }
                    if (WhereClause.Length == 0)
                    {
                        WhereClause += " WHERE " + temp;
                    }
                    else
                    {
                        WhereClause += " AND " + temp;
                    }
                }
            }
            else
            {
                temp = " SerialNumber LIKE " + DB.SQuote("%" + SearchFor + "%") + " ";
                if (WhereClause.Length == 0)
                {
                    WhereClause += " WHERE " + temp;
                }
                else
                {
                    WhereClause += " AND " + temp;
                }
            }

            //Types
            if (ddTypes.SelectedValue != "0")
            {
                temp = " GiftCardTypeID=" + ddTypes.SelectedValue;
                if (WhereClause.Length == 0)
                {
                    WhereClause += " WHERE " + temp;
                }
                else
                {
                    WhereClause += " AND " + temp;
                }
            }

            //Status
            int status = Localization.ParseNativeInt(ddStatus.SelectedValue);
            if (status != 0)
            {
                if (status == 1)
                {
                    temp = " ExpirationDate < getdate()";
                }
                else if (status == 2)
                {
                    temp = " ExpirationDate >= getdate()";
                }
                else if (status == 3)
                {
                    temp = " GiftCardID in (select distinct GiftCardID from GiftCardUsage with (NOLOCK)) ";
                }
                else if (status == 4)
                {
                    temp = " DisabledByAdministrator = 1";
                }

                if (WhereClause.Length == 0)
                {
                    WhereClause += " WHERE " + temp;
                }
                else
                {
                    WhereClause += " AND " + temp;
                }
            }

            sql += WhereClause;

            //set states
            ViewState["SQLString"] = sql.ToString();
            sql += " ORDER BY " + ViewState["Sort"].ToString() + " " + ViewState["SortOrder"].ToString();
            
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

            //if need to return to page
            setFilters(ddStatus.SelectedValue, ddTypes.SelectedValue, ViewState["Sort"].ToString(), ViewState["SortOrder"].ToString(), SearchFor);

            txtSearch.Text = SearchFor;
        }

        protected void treeMain_SelectedNodeChanged(object sender, EventArgs e)
        {
            ViewState["IsInsert"] = false;
            gMain.EditIndex = -1;
            resetError("", false);
            gMain.PageIndex = 0;

            resultFilter("");
        }

        protected void ddTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            preFilter();
        }

        protected void ddStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            preFilter();
        }
        protected void ddForProduct_SelectedIndexChanged(object sender, EventArgs e)
        {
            preFilter();
        }
        protected void ddForCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            preFilter();
        }
        protected void ddForSection_SelectedIndexChanged(object sender, EventArgs e)
        {
            preFilter();
        }
        protected void ddForManufacturer_SelectedIndexChanged(object sender, EventArgs e)
        {
            preFilter();
        }

        protected void preFilter()
        {
            ViewState["IsInsert"] = false;
            gMain.EditIndex = -1;
            resetError("", false);
            gMain.PageIndex = 0;

            resultFilter("");
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
                str = "<font class=\"errorMsg\">" + AppLogic.GetString("admin.common.Error",SkinID, LocaleSetting) + "</font>&nbsp;&nbsp;&nbsp;";

            if (error.Length > 0)
                str += error + "";
            else
                str = "";

            ltError.Text = str;
        }

        protected void gMain_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DataRowView myrow = (DataRowView)e.Row.DataItem;

                LinkButton action = (LinkButton)e.Row.FindControl("lnkAction");
                if (myrow["DisabledByAdministrator"].ToString() == "1")
                {
                    action.CommandArgument = "0|" + myrow["GiftCardID"].ToString();
                    action.Attributes.Add("onClick", "javascript: return confirm('" + AppLogic.GetString("admin.giftcard.Enable", SkinID, LocaleSetting) + "')");
                    action.Text = AppLogic.GetString("admin.common.CapsEnable", SkinID, LocaleSetting);
                }
                else
                {
                    action.CommandArgument = "1|" + myrow["GiftCardID"].ToString();
                    action.Attributes.Add("onClick", "javascript: return confirm('" + AppLogic.GetString("admin.giftcard.Disable", SkinID, LocaleSetting) + "')");
                    action.Text = AppLogic.GetString("admin.common.CapsDisable", SkinID, LocaleSetting);
                }

                int type = Localization.ParseNativeInt(myrow["GiftCardTypeID"].ToString());
                Literal lt = (Literal)e.Row.FindControl("ltCardType");
                lt.Text = GiftCard.s_GetCardType(type);
                if (((int)GiftCardTypes.EMailGiftCard) == type)
                {
                    lt.Text += "&nbsp;<a href=\"javascript:;\" onclick=\"window.open('" + AppLogic.AdminLinkUrl("giftcardview.aspx") + "?iden=" + myrow["GiftCardID"].ToString() + "','View','width=500,height=300,resizable=yes, toolbar=no, scrollbars=yes, status=yes, location=no, directories=no, menubar=no, alwaysRaised=yes');\">View</a>";
                }

                string amount = Localization.CurrencyStringForDisplayWithoutExchangeRate(Localization.ParseNativeCurrency(myrow["InitialAmount"].ToString()));
                Literal ltAmount = (Literal)e.Row.FindControl("ltInitialAmount");
                ltAmount.Text = amount;

                string balance = Localization.CurrencyStringForDisplayWithoutExchangeRate(Localization.ParseNativeCurrency(myrow["Balance"].ToString()));
                Literal ltBalance = (Literal)e.Row.FindControl("ltBalance");
                ltBalance.Text = balance;
            }
        }
        protected void gMain_Sorting(object sender, GridViewSortEventArgs e)
        {
            ViewState["IsInsert"] = false;
            gMain.EditIndex = -1;
            ViewState["Sort"] = e.SortExpression.ToString();
            ViewState["SortOrder"] = (ViewState["SortOrder"].ToString() == "ASC" ? "DESC" : "ASC");
            buildGridData();
        }
        protected void gMain_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            resetError("", false);

            if (e.CommandName == "ItemAction")
            {
                ViewState["IsInsert"] = false;
                gMain.EditIndex = -1;
                string temp = e.CommandArgument.ToString();
                int action = Localization.ParseNativeInt(temp.Substring(0, 1));
                int iden = Localization.ParseNativeInt(temp.Substring(temp.IndexOf("|") + 1));
                updateRow(iden, action);
            }
        }
        protected void updateRow(int iden, int action)
        {
            string msg = string.Empty;
            try
            {
                DB.ExecuteSQL("UPDATE GiftCard SET DisabledByAdministrator=" + action + " WHERE GiftCardID=" + iden.ToString());
                if (action == 1)
                {
                    msg = AppLogic.GetString("admin.giftcard.Disabled", SkinID, LocaleSetting);
                }
                else
                {
                    msg = AppLogic.GetString("admin.giftcard.Enabled", SkinID, LocaleSetting);
                }
                loadGroups();
                buildGridData();
                resetError(msg, false);
            }
            catch (Exception ex)
            {
                throw new Exception(AppLogic.GetString("admin.giftcard.CannotUpdate", SkinID, LocaleSetting) + " " + ex.ToString());
            }
        }

        protected void gMain_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            ViewState["IsInsert"] = false;
            resetError("", false);
            gMain.PageIndex = e.NewPageIndex;
            gMain.EditIndex = -1;
            buildGridData();
        }

        protected void btnInsert_Click(object sender, EventArgs e)
        {
            ViewState["IsInsert"] = false;
            gMain.EditIndex = -1;

            ViewState["Sort"] = "GiftCardID";
            ViewState["SortOrder"] = "DESC";

            //create new gift card
            Response.Redirect(AppLogic.AdminLinkUrl("editgiftcard.aspx")+ "?iden=0");
        }
        protected void btnReset_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            
            ddStatus.ClearSelection();
            ddTypes.ClearSelection();

            preFilter();
        }

        protected void setFilters(string status, string types, string sort, string order, string search)
        {
            ThisCustomer.ThisCustomerSession.SetVal("GCStatus", status);
            ThisCustomer.ThisCustomerSession.SetVal("GCTypes", types);
            ThisCustomer.ThisCustomerSession.SetVal("GCSort", sort);
            ThisCustomer.ThisCustomerSession.SetVal("GCOrder", order);
            ThisCustomer.ThisCustomerSession.SetVal("GCSearch", search);
        }

        protected void retrieveFilters()
        {
            //set states
            if (ThisCustomer.ThisCustomerSession.Session("GCSearch").Length > 0)
            {
                txtSearch.Text = ThisCustomer.ThisCustomerSession.Session("GCSearch");
            }
            if (ThisCustomer.ThisCustomerSession.Session("GCStatus").Length > 0)
            {
                ddStatus.Items.FindByValue(ThisCustomer.ThisCustomerSession.Session("GCStatus")).Selected = true;
            }
            if (ThisCustomer.ThisCustomerSession.Session("GCTypes").Length > 0)
            {
                ddTypes.Items.FindByValue(ThisCustomer.ThisCustomerSession.Session("GCTypes")).Selected = true;
            }
            if (ThisCustomer.ThisCustomerSession.Session("GCSort").Length > 0)
            {
                string temp = ThisCustomer.ThisCustomerSession.Session("GCSort");
                ViewState["Sort"] = temp;
            }
            if (ThisCustomer.ThisCustomerSession.Session("GCOrder").Length > 0)
            {
                string temp = ThisCustomer.ThisCustomerSession.Session("GCOrder");
                ViewState["SortOrder"] = temp;
            }
        }
    }
}
