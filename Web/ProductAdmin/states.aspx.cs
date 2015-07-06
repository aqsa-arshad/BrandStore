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
    /// <summary>
    /// Summary description for states.
    /// </summary>
    public partial class states : AdminPageBase
    {
        protected string selectSQL = "select State.*,Country.Name as Country from State   with (NOLOCK)  left outer join Country  with (NOLOCK)  on State.CountryID=Country.CountryID";
        protected string defaultSort = "State.DisplayOrder,";
        int RowBoundIndex = 1;

        private ArrayList TaxClassIDs = new ArrayList();


        protected void Page_Load(object sender, EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            RowBoundIndex = 1;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader dr = DB.GetRS("SELECT * FROM TaxClass  with (NOLOCK)  ORDER BY DisplayOrder, Name", con))
                {
                    while (dr.Read())
                    {
                        TaxClassIDs.Add(DB.RSFieldInt(dr, "TaxClassID"));
                    }

                    dr.Close();
                    dr.Dispose();
                }

                con.Close();
                con.Dispose();
            }

            if (!IsPostBack)
            {
                DB.ExecuteSQL("delete from state where Name='NEW State'");

                loadScript(false);
                ViewState["Sort"] = "State.Name";
                ViewState["SortOrder"] = "ASC";
                ViewState["SQLString"] = selectSQL;

                ShowAddPanel(false);
            }
            RowBoundIndex = 1;

            ClientScriptManager cs = Page.ClientScript;

            StringBuilder tmpS = new StringBuilder(1024);
            tmpS.Append("function CopyTaxDown(theForm,TaxClassID,InitialFormField)\n");
            tmpS.Append("{\n");
            tmpS.Append("   if(confirm(\"Are you sure you want to copy this tax down to all states? This change will be immediate and irreversible.\")){\n");
            tmpS.Append("       var pat = 'TR_' + TaxClassID;\n");
            tmpS.Append("	    for (i = 0; i < theForm.length; i++)\n");
            tmpS.Append("	    {\n");
            tmpS.Append("		    var str = theForm.elements[i].name;\n");
            tmpS.Append("           if(str.substring(0,pat.length) == pat)\n");
            tmpS.Append("           {\n");
            tmpS.Append("               theForm.elements[i].value = document.getElementById(InitialFormField).value;\n");
            tmpS.Append("           }\n");
            tmpS.Append("	    }\n");
            tmpS.Append("       document.getElementById('" + btnUpdateOrder.ClientID + "').click();");
            tmpS.Append("	}\n");
            tmpS.Append("}\n");

            cs.RegisterClientScriptBlock(GetType(), Guid.NewGuid().ToString(), tmpS.ToString(), true);

        }

        protected void loadScript(bool load)
        {
            if (!load)
            {
                ltScript.Text = "";
            }
        }
        protected void buildGridData()
        {
            string sql = ViewState["SQLString"].ToString();
            sql += " order by " + defaultSort + " " + (ViewState["Sort"].ToString().ToLowerInvariant() == "displayorder" ? "Name" : ViewState["Sort"].ToString()) + " " + ViewState["SortOrder"].ToString();

            string temp = "<div style=\"text-align: center; width: 100%; float: left; font-weight: bold;\">Specify Tax Rates</div><div style=\"clear: both;\"></div>";

            int count = 0;

            using (DataTable dt = new DataTable())
            {
                using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
                {
                    conn.Open();
                    using (IDataReader rs = DB.GetRS(sql, conn))
                    {
                        dt.Load(rs);

                    }

                    using (IDataReader dr = DB.GetRS("SELECT * FROM TaxClass  with (NOLOCK)  ORDER BY DisplayOrder, Name", conn))
                    {
                        while (dr.Read())
                        {
                            count++;
                            temp += "<div style=\"width: %W%; float: left; white-space: wrap;\">" + XmlCommon.GetLocaleEntry(DB.RSField(dr, "Name"), LocaleSetting, false) + "</div>";
                        }
                    }

                    if (count > 0)
                    {
                        gMain.Columns[6].HeaderText = temp.Replace("%W%", ((100 / count) - 1) + "%");
                    }

                }

                gMain.DataSource = dt;
                gMain.DataBind();
            }
        }
        protected void resetError(string error, bool isError)
        {
            StringBuilder output = new StringBuilder();

            string str = "<font class=\"noticeMsg\">NOTICE:</font>&nbsp;&nbsp;&nbsp;";
            if (isError)
                str = "<font class=\"errorMsg\">ERROR:</font>&nbsp;&nbsp;&nbsp;";

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
                DataRowView myrow = (DataRowView)e.Row.DataItem;
                int iden = Localization.ParseNativeInt(myrow["StateID"].ToString());

                ImageButton ib = (ImageButton)e.Row.FindControl("imgDelete");
                ib.Attributes.Add("onClick", "javascript: return confirm('Confirm Delete?')");

                Literal ltTaxRate = (Literal)e.Row.FindControl("ltTaxRate");

                StringBuilder tmpS = new StringBuilder(1024);
                for (int i = 0; i < TaxClassIDs.Count; i++)
                {
                    int classID = (int)TaxClassIDs[i];
                    string rate = Localization.CurrencyStringForDBWithoutExchangeRate(AppLogic.StateTaxRatesTable.GetTaxRate(iden, classID));
                    tmpS.Append("<div style=\"width: (!W!); float: left; white-space: wrap;\">");

                    if (gMain.EditIndex != -1 && e.Row.RowIndex == gMain.EditIndex)
                    {
                        tmpS.Append("<input type=\"text\" id='TR_" + classID.ToString() + "_" + iden + "' name='TR_" + classID.ToString() + "_" + iden + "' class=\"textBox3\" value='" + rate + "'></input>%");
                    }
                    else
                    {
                        tmpS.Append("<input type=\"text\" id='TR_" + classID.ToString() + "_" + iden + "' name='TR_" + classID.ToString() + "_" + iden + "' class=\"textBox4\" value='" + rate + "' readonly></input>%");

                    }
                    if (RowBoundIndex == 1)
                    {
                        tmpS.Append("&nbsp;<img onClick=\"CopyTaxDown(document.forms[0]," + classID.ToString() + ",'TR_" + classID.ToString() + "_" + iden.ToString() + "')\" class=\"actionelement\" src=\"" + AppLogic.LocateImageURL("~/App_Themes/Admin_Default/images/downarrow.gif") + "\" border=\"0\" alt=\"Copy Rate Down\" runat=\"Server\">");
                    }
                    tmpS.Append("</div>");
                }

                if (tmpS.Length > 0)
                {
                    ltTaxRate.Text = tmpS.ToString().Replace("(!W!)", ((100 / TaxClassIDs.Count) - 1) + "%");
                }
                else
                {
                    ltTaxRate.Text = "No Tax Classes";
                }

                RowBoundIndex++;

                //Click to edit
                if ((e.Row.RowState == DataControlRowState.Normal) || (e.Row.RowState == DataControlRowState.Alternate))
                {
                    e.Row.Attributes.Add("ondblclick", "javascript:__doPostBack('gMain','Edit$" + e.Row.RowIndex + "')");
                }

                if ((e.Row.RowState & DataControlRowState.Edit) == DataControlRowState.Edit)
                {
                    string country = myrow["Country"].ToString();

                    DropDownList dd = (DropDownList)e.Row.FindControl("ddCountry");
                    RadioButtonList cb = (RadioButtonList)e.Row.FindControl("rblTax");

                    ListItem li = new ListItem(" - Select One -", "0");
                    dd.Items.Add(li);

                    using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                    {
                        con.Open();
                        using (IDataReader rsst = DB.GetRS("select * from Country   with (NOLOCK)  order by DisplayOrder,Name", con))
                        {
                            while (rsst.Read())
                            {
                                li = new ListItem(DB.RSField(rsst, "Name"), DB.RSFieldInt(rsst, "CountryID").ToString());
                                dd.Items.Add(li);
                            }
                        }
                    }

                    //match selection
                    foreach (ListItem liC in dd.Items)
                    {
                        if (liC.Text.Equals(country))
                        {
                            dd.ClearSelection();
                            liC.Selected = true;
                            break;
                        }
                    }

                    ClientScript.RegisterStartupScript(GetType(), "goToScript", "<script type=\"text/javascript\">location.href = '#a" + myrow["stateID"].ToString() + "';</script>");
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
                deleteRowPerm(iden);
            }
        }

        protected void deleteRowPerm(int iden)
        {
            try
            {
                AppLogic.StateTaxRatesTable.Remove(iden);
                DB.ExecuteSQL("delete from State where StateID=" + iden.ToString());
                buildGridData();
                resetError("Item Deleted", false);
            }
            catch (Exception ex)
            {
                throw new Exception("Couldn't delete from database: " + ex.ToString());
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
                string name = ((TextBox)row.FindControl("txtName")).Text.Trim();
                string abbr = ((TextBox)row.FindControl("txtAbbreviation")).Text.Trim();
                bool published = ((CheckBox)row.FindControl("cbPublished")).Checked;
                int country = Localization.ParseNativeInt(((DropDownList)row.FindControl("ddCountry")).SelectedValue);
                int order = Localization.ParseNativeInt(((TextBox)row.FindControl("txtOrder")).Text.Trim());

                // see if already exists:
                int N = DB.GetSqlN("select count(Name) as N from State   with (NOLOCK)  where StateID<>" + iden + " and lower(Name)=" + DB.SQuote(name.ToLowerInvariant()));
                if (N != 0)
                {
                    resetError("There is already another state with that name.", true);
                    return;
                }

                StringBuilder sql = new StringBuilder(4096);

                sql.Append("update State set ");
                sql.Append("Name=" + DB.SQuote(name) + ",");
                sql.Append("CountryID=" + country + ",");
                sql.Append("Published=" + CommonLogic.IIF(published, 1, 0) + ",");
                sql.Append("DisplayOrder=" + order + ",");
                sql.Append("Abbreviation=" + DB.SQuote(CommonLogic.Left(abbr, 5)));
                sql.Append(" where StateID=" + iden);

                try
                {
                    DB.ExecuteSQL(sql.ToString());
                    resetError("Item updated", false);
                    gMain.EditIndex = -1;
                    ViewState["SQLString"] = selectSQL;

                    //UpdateTaxOrder();
                    for (int i = 0; i <= Request.Form.Count - 1; i++)
                    {
                        //TR_CLASSID_STATEID
                        if (Request.Form.Keys[i].IndexOf("TR_") != -1)
                        {
                            String[] keys = Request.Form.Keys[i].Split('_');
                            int StateID = Localization.ParseUSInt(keys[2]);
                            int ClassID = Localization.ParseUSInt(keys[1]);
                            if (StateID == Localization.ParseUSInt(iden))
                            {
                                decimal tax = Decimal.Zero;
                                try
                                {
                                    tax = Localization.ParseNativeDecimal(Request.Form[Request.Form.Keys[i]]);
                                }
                                catch { }
                                StateTaxRate str = AppLogic.StateTaxRatesTable[StateID, ClassID];
                                try
                                {
                                    if (str == null)
                                    {
                                        AppLogic.StateTaxRatesTable.Add(StateID, ClassID, tax);
                                    }
                                    else
                                    {
                                        str.Update(tax);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    string err = ex.Message;
                                }
                            }
                        }
                    }

                    buildGridData();
                }
                catch (Exception ex)
                {
                    throw new Exception("Couldn't update database: " + sql.ToString() + ex.ToString());
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

            txtName.Text = "";
            txtAbbr.Text = "";
            txtOrder.Text = "1";

            ddCountry.Items.Clear();
            ddCountry.ClearSelection();
            ListItem li = new ListItem(" - Select One -", "0");
            ddCountry.Items.Add(li);

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rsst = DB.GetRS("select * from Country   with (NOLOCK)  order by DisplayOrder,Name", con))
                {
                    while (rsst.Read())
                    {
                        li = new ListItem(DB.RSField(rsst, "Name"), DB.RSFieldInt(rsst, "CountryID").ToString());
                        ddCountry.Items.Add(li);
                    }
                }
            }

            
        }

        protected void btnUpdateOrder_Click(object sender, EventArgs e)
        {
            UpdateTaxOrder();

            resetError("Display Order and Taxes updated.", false);
            buildGridData();
        }

        protected void UpdateTaxOrder()
        {
            for (int i = 0; i <= Request.Form.Count - 1; i++)
            {
                if (Request.Form.Keys[i].IndexOf("DisplayOrder_") != -1)
                {
                    String[] keys = Request.Form.Keys[i].Split('_');
                    int StateID = Localization.ParseUSInt(keys[1]);
                    int DispOrd = 1;
                    try
                    {
                        DispOrd = Localization.ParseUSInt(Request.Form[Request.Form.Keys[i]]);
                    }
                    catch { }
                    DB.ExecuteSQL("update State set DisplayOrder=" + DispOrd.ToString() + " where StateID=" + StateID.ToString());
                }
            }

            //handle taxes
            for (int i = 0; i <= Request.Form.Count - 1; i++)
            {
                //TR_CLASSID_STATEID
                if (Request.Form.Keys[i].IndexOf("TR_") != -1)
                {
                    String[] keys = Request.Form.Keys[i].Split('_');
                    int StateID = Localization.ParseUSInt(keys[2]);
                    int ClassID = Localization.ParseUSInt(keys[1]);
                    decimal tax = Decimal.Zero;
                    try
                    {
                        tax = Localization.ParseNativeDecimal(Request.Form[Request.Form.Keys[i]]);
                    }
                    catch { }
                    StateTaxRate ctr = AppLogic.StateTaxRatesTable[StateID, ClassID];
                    try
                    {
                        if (ctr == null)
                        {
                            AppLogic.StateTaxRatesTable.Add(StateID, ClassID, tax);
                        }
                        else
                        {
                            ctr.Update(tax);
                        }
                    }
                    catch (Exception ex)
                    {
                        string err = ex.Message;
                    }

                }
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            resetError("", false);
            StringBuilder sql = new StringBuilder();

            if (ValidInput())
            {
                string name = txtName.Text.Trim();
                string abbr = txtAbbr.Text.Trim();
                int published = CommonLogic.IIF(cbPublished.Checked == true, 1, 0);
                int country = Localization.ParseNativeInt(ddCountry.SelectedValue);
                int order = Localization.ParseNativeInt(txtOrder.Text.Trim());

                // ok to add them:
                String NewGUID = DB.GetNewGUID();
                sql.Append("insert into State(StateGUID,[Name],Published,Abbreviation,CountryID,DisplayOrder) values(");
                sql.Append(DB.SQuote(NewGUID) + ",");
                sql.Append(DB.SQuote(name) + ",");
                sql.Append(published + ",");
                sql.Append(DB.SQuote(abbr) + ",");
                sql.Append(country + ",");
                sql.Append(order + ")");

                try
                {
                    DB.ExecuteSQL(sql.ToString());
                    resetError("State added.", false);
                    ShowAddPanel(false);
                }
                catch
                {
                    resetError("State abbreviation already exists.", true);
                    ShowAddPanel(true);
                }
            }
            else
            {
                resetError("Please input all required fields.", true);
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

        protected bool publishedCheck(object PublishedValue)
        {
            bool statePublished = true;

            if (PublishedValue.ToString().Equals("1"))
            {
                statePublished = true;
            }
            else
            {
                statePublished = false;
            }

            return statePublished;
        }
    }
}
