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
    /// Summary description for taxzips.
    /// </summary>
    public partial class taxzips : AdminPageBase
    {
        protected string selectSQL = "select distinct z.ZipCode, c.Name, z.CountryID from ZipTaxRate z, Country c with (NOLOCK) where z.CountryId = c.CountryId";
        protected string defaultSort = "";
        protected Customer cust;
        int RowBoundIndex = 1;

        private ArrayList TaxClassIDs = new ArrayList();
        private int someCountryRequirePostalCode = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            cust = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader dr = DB.GetRS("SELECT * FROM TaxClass with (NOLOCK) ORDER BY DisplayOrder, Name", con))
                {
                    while (dr.Read())
                    {
                        TaxClassIDs.Add(DB.RSFieldInt(dr, "TaxClassID"));
                    }
                }
            }


            if (!IsPostBack)
            {
                ShowAddPanel(false);

                loadScript(false);
                ViewState["Sort"] = "ZipCode";
                ViewState["SortOrder"] = "ASC";
                ViewState["SQLString"] = selectSQL;

                buildGridData();
                DataTable datasource = buildCountryData();                
                ddlCountry.DataSource = datasource;
                ddlCountry.DataTextField = "Name";
                ddlCountry.DataValueField = "CountryID";
                ddlCountry.DataBind();
            }

            RowBoundIndex = 1;

            ClientScriptManager cs = Page.ClientScript;

            StringBuilder tmpS = new StringBuilder(1024);
            tmpS.Append("function CopyTaxDown(theForm,TaxClassID,InitialFormField)\n");
            tmpS.Append("{\n");
            tmpS.Append("   var pat = 'TR_' + TaxClassID;\n");
            tmpS.Append("	for (i = 0; i < theForm.length; i++)\n");
            tmpS.Append("	{\n");
            tmpS.Append("		var str = theForm.elements[i].name;\n");
            tmpS.Append("       if(str.substring(0,pat.length) == pat)\n");
            tmpS.Append("       {\n");
            tmpS.Append("           theForm.elements[i].value = document.getElementById(InitialFormField).value;\n");
            tmpS.Append("       }\n");
            tmpS.Append("	}\n");
            tmpS.Append("}\n");

            cs.RegisterClientScriptBlock(GetType(), Guid.NewGuid().ToString(), tmpS.ToString(), true);

            bool ZipCodeVisible = false;
            someCountryRequirePostalCode = DB.GetSqlN("SELECT COUNT(postalcoderequired) N from country with (NOLOCK) where postalcoderequired = 1 and published = 1");

            if (someCountryRequirePostalCode > 0)
            {
                ZipCodeVisible = AppLogic.GetCountryPostalCodeRequired(Convert.ToInt32(ddlCountry.SelectedValue));
            }
            else
            {
                resetError("You currently do not have country that requires a postal code", true);
            }
            
            trZipCode.Visible = ZipCodeVisible;
            trTaxRate.Visible = ZipCodeVisible;
            btnUpdateOrder.Enabled = ZipCodeVisible;
            btnInsert.Enabled = ZipCodeVisible;                        
        }

        protected void loadScript(bool load)
        {
            if (load)
            {
                if (AppLogic.NumLocaleSettingsInstalled() > 1)
                {
                }
            }
            else
            {
                ltScript.Text = "";
            }
        }

        protected DataTable buildCountryData()
        {
            string sSql = "select * from country  with (NOLOCK)  where Published = 1 and PostalCodeRequired=1 order by DisplayOrder,Name";

            using (DataTable dt = new DataTable())
            {
                using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
                {
                    conn.Open();

                    using (IDataReader rs = DB.GetRS(sSql, conn))
                    {
                        dt.Load(rs);

                        rs.Close();
                        rs.Dispose();
                    }

                    conn.Close();
                    conn.Dispose();
                }

                return dt;
            }

        }

        protected void buildGridData()
        {
            string sql = ViewState["SQLString"].ToString();
            sql += " order by " + defaultSort + " " + ViewState["Sort"].ToString() + " " + ViewState["SortOrder"].ToString();

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

                    using (IDataReader rs = DB.GetRS("SELECT * FROM TaxClass  with (NOLOCK)  ORDER BY DisplayOrder, Name", conn))
                    {
                        while (rs.Read())
                        {
                            count++;
                            temp += "<div style=\"width: %W%; float: left; white-space: wrap;\">" + XmlCommon.GetLocaleEntry(DB.RSField(rs, "Name"), cust.LocaleSetting, false) + "</div>";
                        }
                    }
                }

                if (count > 0)
                {
                    gMain.Columns[3].HeaderText = temp.Replace("%W%", ((100 / count) - 1) + "%");
                }

                gMain.DataSource = dt;
                gMain.DataBind();
            }
        }

        protected void resetError(string error, bool isError)
        {
            string str = "<font class=\"noticeMsg\">NOTICE:</font>&nbsp;&nbsp;&nbsp;";
            if (isError)
                str = "<font class=\"errorMsg\">ERROR:</font>&nbsp;&nbsp;&nbsp;";

            if (error.Length > 0)
                str += error + "";
            else
                str = "";

           ltError.Text = str.ToString();
        }

        protected void gMain_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DataRowView myrow = (DataRowView)e.Row.DataItem;
                string ZipCode = myrow["ZipCode"].ToString();

                int CountryID = AppLogic.GetCountryID(myrow["name"].ToString());

                ImageButton ib = (ImageButton)e.Row.FindControl("imgDelete");
                ib.Attributes.Add("onClick", "javascript: return confirm('Confirm Delete?')");

                Literal ltTaxRate = (Literal)e.Row.FindControl("ltTaxRate");

                StringBuilder tmpS = new StringBuilder(1024);
                for (int i = 0; i < TaxClassIDs.Count; i++)
                {
                    int classID = (int)TaxClassIDs[i];
                    string rate = Localization.CurrencyStringForDBWithoutExchangeRate(AppLogic.ZipTaxRatesTable.GetTaxRate(ZipCode, classID, CountryID));
                    tmpS.Append("<div style=\"width: (!W!); float: left; white-space: wrap;\">");

                    if (gMain.EditIndex != -1 && e.Row.RowIndex == gMain.EditIndex)
                    {
                        tmpS.Append("<input type=\"text\" id='TR_" + classID.ToString() + "_" + ZipCode + "_" + e.Row.RowIndex + "' name='TR_" + classID.ToString() + "_" + ZipCode + "_" + e.Row.RowIndex + "' class=\"textBox3\" value='" + rate + "'></input>%");
                    }
                    else
                    {
                        tmpS.Append("<input type=\"text\" id='TR_" + classID.ToString() + "_" + ZipCode + "_" + e.Row.RowIndex + "' name='TR_" + classID.ToString() + "_" + ZipCode + "_" + e.Row.RowIndex + "' class=\"textBox4\" value='" + rate + "' readonly></input>%");

                    }
                    if (RowBoundIndex == 1)
                    {
                        tmpS.Append("&nbsp;<img onClick=\"CopyTaxDown(document.forms[0]," + classID.ToString() + ",'TR_" + classID.ToString() + "_" + ZipCode + "_" + e.Row.RowIndex + "')\" class=\"actionelement\" src=\"" + AppLogic.LocateImageURL("~/App_Themes/Admin_Default/images/downarrow.gif") + "\" border=\"0\" alt=\"Copy Rate Down\">");
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

                    ClientScript.RegisterStartupScript(GetType(), "goToScript", "<script type=\"text/javascript\">location.href = '#a" + myrow["ZipCode"].ToString() + "';</script>");
                    //set the original zip for editing
                    ViewState["OriginalZip"] = myrow["ZipCode"].ToString();
                    ViewState["OriginalCountryID"] = myrow["Name"].ToString();

                    DropDownList ddl = (DropDownList)e.Row.FindControl("ddlCountry");

                    ListItem li = new ListItem();

                    using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
                    {
                        conn.Open();
                        using (IDataReader rs = DB.GetRS("select * from Country WITH (NOLOCK) order by DisplayOrder,Name", conn))
                        {
                            while (rs.Read())
                            {
                                li = new ListItem(DB.RSField(rs, "Name"), DB.RSFieldInt(rs, "CountryID").ToString());
                                ddl.Items.Add(li);
                            }

                            rs.Close();
                            rs.Dispose();
                        }

                        conn.Close();
                        conn.Dispose();
                    }

                    foreach (ListItem liC in ddl.Items)
                    {
                        if (liC.Text.Equals(ViewState["OriginalCountryID"].ToString()))
                        {
                            ddl.ClearSelection();
                            liC.Selected = true;
                            break;
                        }
                    }


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
                String[] keys = e.CommandArgument.ToString().Split('|');
                string ZipCode = keys[0];
                string CountryID = keys[1];
                deleteRowPerm(ZipCode, AppLogic.GetCountryID(CountryID));
            }
        }

        protected void deleteRowPerm(string ZipCode, int CountryID)
        {
            StringBuilder sql = new StringBuilder(2500);
            sql.Append("delete from ZipTaxRate where ZipCode=" + DB.SQuote(ZipCode) + " AND CountryID=" + CountryID);
            try
            {
                DB.ExecuteSQL(sql.ToString());

                // synchronize with the cached tax rates
                AppLogic.ZipTaxRatesTable.RemoveAll(ZipCode, CountryID);
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

        protected void btnInsert_Click(object sender, EventArgs e)
        {
            resetError("", false);
            gMain.EditIndex = -1;
            txtTax.Text = "";
            txtZip.Text = "";
            ShowAddPanel(true);
        }

        protected void gMain_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            //if new item and cancel, must delete
            DB.ExecuteSQL("DELETE FROM ZipTaxRate WHERE ZipCode='ZIP'");

            ViewState["SQLString"] = selectSQL;

            gMain.EditIndex = -1;
            buildGridData();
        }

        protected void gMain_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            GridViewRow row = gMain.Rows[e.RowIndex];

            if (row != null)
            {
                string orig = ViewState["OriginalZip"].ToString();
                string zip = (((TextBox)row.FindControl("txtZip")).Text.Trim()).ToString();
                string countryid = ((DropDownList)row.FindControl("ddlCountry")).SelectedValue.Trim();
                string OriginalCountryID = AppLogic.GetCountryID(ViewState["OriginalCountryID"].ToString()).ToString();

                StringBuilder sql = new StringBuilder(1024);

                //make sure no duplicates
                if (!orig.Equals(zip))
                {
                    int count = DB.GetSqlN(String.Format("SELECT count(*) AS N FROM ZipTaxRate WHERE ZipCode = {0} and CountryId = {1}", DB.SQuote(zip), CommonLogic.IIF(someCountryRequirePostalCode > 0, Convert.ToInt32(countryid), -1)));
                    if (count > 0)
                    {
                        resetError("Duplicate Zip Code exists", true);
                        return;
                    }
                }

                try
                {
                    resetError("Item updated", false);
                    gMain.EditIndex = -1;
                    ViewState["SQLString"] = selectSQL;

                    for (int i = 0; i <= Request.Form.Count - 1; i++)
                    {
                        //TR_CLASSID_ZipCode
                        if (Request.Form.Keys[i].IndexOf("TR_") != -1)
                        {
                            String[] keys = Request.Form.Keys[i].Split('_');
                            string ZipCode = keys[2];
                            int ClassID = Localization.ParseUSInt(keys[1]);
                            decimal taxrate = Decimal.Zero;
                            if (ZipCode == orig && e.RowIndex == Convert.ToInt32(keys[3]))
                            {
                                try
                                {
                                    taxrate = Localization.ParseNativeDecimal(Request.Form[Request.Form.Keys[i]]);
                                }
                                catch { }

                                ZipTaxRate ztr3 = AppLogic.ZipTaxRatesTable[zip, ClassID, Convert.ToInt32(countryid)];
                                try
                                {
                                    if (ztr3 != null)
                                    {
                                        if (Convert.ToInt32(countryid) != Convert.ToInt32(OriginalCountryID))
                                        {
                                            resetError("Zip Code, Tax Class ID and Country ID already exists.", false);
                                        }
                                        else
                                        {
                                            ztr3.Update(taxrate, Convert.ToInt32(countryid));
                                        }
                                    }
                                    else
                                    {
                                        bool validZipFormat = true;
                                        if ((Convert.ToInt32(countryid) != Convert.ToInt32(OriginalCountryID)) || (orig != zip))
                                        {
                                            if (Convert.ToInt32(countryid) != Convert.ToInt32(OriginalCountryID))
                                            {
                                                validZipFormat = AppLogic.ValidatePostalCode(zip, Convert.ToInt32(countryid));
                                            }
                                            if (validZipFormat)
                                            {
                                                ZipTaxRates ztr2 = new ZipTaxRates();
                                                foreach (ZipTaxRate ztr in ztr2.All)
                                                {
                                                    if (ztr.ZipCode == orig && ztr.TaxClassID == ClassID && ztr.CountryID == Convert.ToInt32(OriginalCountryID))
                                                    {
                                                        ztr.Update(taxrate, zip, Convert.ToInt32(countryid), Convert.ToInt32(OriginalCountryID));
                                                        AppLogic.ZipTaxRatesTable.AddNewRate(ztr.ZipTaxID, zip, ClassID, taxrate, Convert.ToInt32(countryid));
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                resetError(AppLogic.GetCountryPostalErrorMessage(Convert.ToInt32(countryid), cust.SkinID, cust.LocaleSetting), true);
                                            }
                                        }
                                        else
                                        {
                                            AppLogic.ZipTaxRatesTable.Add(ZipCode, ClassID, taxrate, Convert.ToInt32(countryid));
                                        }
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

        protected void btnUpdateOrder_Click(object sender, EventArgs e)
        {
            for (int i = 0; i <= Request.Form.Count - 1; i++)
            {
                //TR_CLASSID_ZipCode
                if (Request.Form.Keys[i].IndexOf("TR_") != -1)
                {
                    String[] keys = Request.Form.Keys[i].Split('_');
                    string ZipCode = keys[2];
                    int ClassID = Localization.ParseUSInt(keys[1]);
                    decimal taxrate = Decimal.Zero;
                    GridViewRow row = gMain.Rows[Convert.ToInt32(keys[3])];
                    HiddenField hdfCountryID = (HiddenField)row.FindControl("hdfCountryID");
                    int CountryID = Convert.ToInt32(hdfCountryID.Value);

                    resetError("Items updated", false);

                    try
                    {
                        taxrate = Localization.ParseNativeDecimal(Request.Form[Request.Form.Keys[i]]);
                    }
                    catch { }
                    ZipTaxRate ztr = AppLogic.ZipTaxRatesTable[ZipCode, ClassID, CountryID];
                    try
                    {
                        if (ztr == null)
                        {
                            AppLogic.ZipTaxRatesTable.Add(ZipCode, ClassID, taxrate, CountryID);
                        }
                        else
                        {
                            
                            ztr.Update(taxrate, CountryID);
                        }
                    }
                    catch (Exception ex)
                    {
                        string err = ex.Message;
                    }
            }
            }


            buildGridData();
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            resetError("", false);
            StringBuilder sql = new StringBuilder(2500);

            string zip = txtZip.Text.Trim();
            decimal tax = Localization.ParseNativeDecimal(txtTax.Text.Trim());
            string countryId = ddlCountry.SelectedValue.Trim();

            if (!CommonLogic.IsStringNullOrEmpty(countryId))
            {
                if (AppLogic.ValidatePostalCode(zip, Convert.ToInt32(countryId)))
                {
                    // ok to add them:
                    ZipTaxRate ztr = AppLogic.ZipTaxRatesTable[CommonLogic.Left(zip, 10), AppLogic.AppConfigNativeInt("Admin_DefaultTaxClassID"), Convert.ToInt32(countryId)];
                    if (ztr == null)
                    {
                        AppLogic.ZipTaxRatesTable.Add(CommonLogic.Left(zip, 10), AppLogic.AppConfigNativeInt("Admin_DefaultTaxClassID"), tax, Convert.ToInt32(countryId));
                        resetError("Zip Code added.", false);
                    }
                    else
                    {
                        ztr.Update(tax, Convert.ToInt32(countryId));
                        resetError("Zip Code already exists and was updated.", false);
                    }
                }
                else
                {
                    resetError(AppLogic.GetCountryPostalErrorMessage(Convert.ToInt32(countryId), cust.SkinID, cust.LocaleSetting), true);
                }
            }
            buildGridData();

            ShowAddPanel(false);
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            resetError("", false);

            ShowAddPanel(false);
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
            }
        }
    }
}
