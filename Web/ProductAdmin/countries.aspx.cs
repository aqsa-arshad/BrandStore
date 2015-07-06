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
    /// Summary description for countries.
    /// </summary>
    public partial class countries : AdminPageBase 
    {
        protected string selectSQL = "select * from Country with (NOLOCK) ";
        protected string defaultSort = "DisplayOrder,";
        protected int RowBoundIndex = 1;

        private ArrayList TaxClassIDs = new ArrayList();

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            RowBoundIndex = 1;

            //add tax class inputs

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader dr = DB.GetRS("SELECT * FROM TaxClass  with (NOLOCK)  ORDER BY DisplayOrder, Name", dbconn))
                {
                    while (dr.Read())
                    {
                        TaxClassIDs.Add(DB.RSFieldInt(dr, "TaxClassID"));
                    }
                }
            }

            if (!IsPostBack)
            {


                DB.ExecuteSQL("delete from country where [Name]='NEW Country'");

                loadScript(false);
                ViewState["Sort"] = "Name";
                ViewState["SortOrder"] = "ASC";
                ViewState["SQLString"] = selectSQL;

                ShowAddPanel(false);
            }
            RowBoundIndex = 1;

            ClientScriptManager cs = Page.ClientScript;

            StringBuilder tmpS = new StringBuilder(1024);
            tmpS.Append("function CopyTaxDown(theForm,TaxClassID,InitialFormField)\n");
            tmpS.Append("{\n");
            tmpS.Append("   if(confirm(\"Are you sure you want to copy this tax down to all countries? This change will be immediate and irreversible.\")){\n");
            tmpS.Append("       var pat = 'TR_' + TaxClassID;\n");
            tmpS.Append("	    for (i = 0; i < theForm.length; i++)\n");
            tmpS.Append("	    {\n");
            tmpS.Append("		    var str = theForm.elements[i].name;\n");
            tmpS.Append("           if(str.substring(0,pat.length) == pat)\n");
            tmpS.Append("           {\n");
            tmpS.Append("               theForm.elements[i].value = document.getElementById(InitialFormField).value;\n");
            tmpS.Append("           }\n");
            tmpS.Append("	    }\n");
            tmpS.Append("	    document.getElementById('"+btnUpdateOrder.ClientID+"').click();");
            tmpS.Append("	}\n");
            tmpS.Append("}\n");

            cs.RegisterClientScriptBlock(GetType(), Guid.NewGuid().ToString(), tmpS.ToString(), true);
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

        protected void BuildGridData()
        {
            StringBuilder tmpS = new StringBuilder(1024);
            tmpS.Append("<div style=\"text-align: center; width: 100%; float: left; font-weight: bold;\">Specify Tax Rates</div><div style=\"clear: both;\"></div>");
            int count = 0;

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader dr = DB.GetRS("SELECT * FROM TaxClass ORDER BY DisplayOrder, Name", dbconn))
                {
                    while (dr.Read())
                    {
                        count++;
                        tmpS.Append("<div style=\"width: (!W!); float: left; white-space: wrap;\">" + XmlCommon.GetLocaleEntry(DB.RSField(dr, "Name"), LocaleSetting, false));
                        tmpS.Append("</div>");
                    }
                }
            }

            if (count > 0)
            {
                gMain.Columns[11].HeaderText = tmpS.ToString().Replace("(!W!)", ((100 / count) - 1) + "%");
            }

            string sql = ViewState["SQLString"].ToString();
            sql += " order by " + defaultSort + " " + (ViewState["Sort"].ToString().Equals("displayorder", StringComparison.InvariantCultureIgnoreCase) ? "Name" : ViewState["Sort"].ToString()) + " " + ViewState["SortOrder"].ToString();

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

        public string Wrap(string word)
        {
            int size = 20;
            string wrapped = string.Empty;

            for (int ctr = 0; ctr < word.Length; ctr++)
            {
                wrapped += word[ctr].ToString();
                if (ctr > 0 && ctr % size == 0)
                {
                    wrapped += "<br />";
                }
            }

            return wrapped;
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
            BuildGridData();
        }

        protected void gMain_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DataRowView myrow = (DataRowView)e.Row.DataItem;
                int iden = Localization.ParseNativeInt(myrow["CountryID"].ToString());

                ImageButton ib = (ImageButton)e.Row.FindControl("imgDelete");
                ib.Attributes.Add("onClick", "javascript: return confirm('Confirm Delete?')");

                //add tax class inputs
                StringBuilder tmpS = new StringBuilder(1024);
                Literal ltTaxRate = (Literal)e.Row.FindControl("ltTaxRate");
                for (int i = 0; i < TaxClassIDs.Count; i++)
                {
                    int classID = (int)TaxClassIDs[i];
                    string rate = Localization.CurrencyStringForDBWithoutExchangeRate(AppLogic.CountryTaxRatesTable.GetTaxRate(iden, classID));


                    tmpS.Append("<div style=\"width: %W%;  float: left; white-space: wrap;\">");
                    if (gMain.EditIndex != -1 && e.Row.RowIndex == gMain.EditIndex)
                    {
                        tmpS.Append("<input type=\"text\" id='TR_" + classID + "_" + iden + "' name='TR_" + classID + "_" + iden + "' class=\"textBox3\" value='" + rate + "'></input>%");
                    }
                    else
                    {
                        tmpS.Append("<input type=\"text\" id='TR_" + classID + "_" + iden + "' name='TR_" + classID + "_" + iden + "' class=\"textBox4\" value='" + rate + "' readonly></input>%");

                    }
                    if (RowBoundIndex == 1)
                    {
                        tmpS.Append("&nbsp;<img class=\"actionelement\" onClick=\"CopyTaxDown(document.forms[0]," + classID.ToString() + ",'TR_" + classID.ToString() + "_" + iden.ToString() + "')\" src=\"" + AppLogic.LocateImageURL("~/App_Themes/Admin_Default/images/downarrow.gif") + "\" border=\"0\" alt=\"" + AppLogic.GetString("admin.common.CopyRateDown", SkinID, LocaleSetting) + "\" runat=\"Server\">");
                        
                    }
                    tmpS.Append("</div>");

                }
                RowBoundIndex++;

                if (TaxClassIDs.Count > 0)
                {
                    ltTaxRate.Text = tmpS.ToString().Replace("%W%", ((100 / TaxClassIDs.Count) - 1) + "%");

                }
                else
                {
                    ltTaxRate.Text = AppLogic.GetString("admin.common.NoTaxClasses", SkinID, LocaleSetting);
                }

                //Click to edit
                if ((e.Row.RowState == DataControlRowState.Normal) || (e.Row.RowState == DataControlRowState.Alternate))
                {
                    e.Row.Attributes.Add("ondblclick", "javascript:__doPostBack('gMain','Edit$" + e.Row.RowIndex + "')");
                }

                if ((e.Row.RowState & DataControlRowState.Edit) == DataControlRowState.Edit)
                {
                    ClientScript.RegisterStartupScript(GetType(), "goToScript", "<script type=\"text/javascript\">location.href = '#a" + myrow["CountryID"].ToString() + "';</script>");
                }
            }
        }
        protected void gMain_Sorting(object sender, GridViewSortEventArgs e)
        {
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
                gMain.EditIndex = -1;
                int iden = Localization.ParseNativeInt(e.CommandArgument.ToString());
                deleteRowPerm(iden);
            }
        }

        protected void deleteRowPerm(int iden)
        {
            try
            {
                AppLogic.CountryTaxRatesTable.Remove(iden);
                DB.ExecuteSQL("delete from Country where CountryID=" + iden.ToString());
                BuildGridData();
                resetError("Item Deleted", false);
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format(AppLogic.GetString("admin.common.ErrDeleteDB", SkinID, LocaleSetting),ex.ToString()));
            }
        }

        protected void gMain_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            resetError("", false);
            gMain.PageIndex = e.NewPageIndex;
            gMain.EditIndex = -1;
            BuildGridData();
        }
        protected void gMain_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            GridViewRow row = gMain.Rows[e.RowIndex];

            if (row != null)
            {
                string iden = row.Cells[1].Text.ToString();
                string name = ((TextBox)row.FindControl("txtName")).Text.Trim();
                string twoLetter = ((TextBox)row.FindControl("txt2LetterIso")).Text.Trim();
                string threeLetter = ((TextBox)row.FindControl("txt3LetterIso")).Text.Trim();
                string numericISO = ((TextBox)row.FindControl("txtNumericISOCode")).Text.Trim();
                bool published = ((CheckBox)row.FindControl("cbPublished")).Checked;
                bool postalCodeReq = ((CheckBox)row.FindControl("cbPostalCodeRequired")).Checked;
                string postalCodeRegEx = ((TextBox)row.FindControl("txtPostalCodeRegex")).Text.Trim();
                string postalCodeExample = ((TextBox)row.FindControl("txtPostalCodeExample")).Text.Trim();
                int order = Localization.ParseNativeInt(((TextBox)row.FindControl("txtOrder")).Text.Trim());

                // see if already exists:
                int N = DB.GetSqlN("select count(Name) as N from Country   with (NOLOCK)  where CountryID<>" + iden + " and lower(Name)=" + DB.SQuote(name.ToLowerInvariant()));
                if (N != 0)
                {
                    resetError(AppLogic.GetString("admin.countries.ExistingCountry", SkinID, LocaleSetting), true);
                    return;
                }

                StringBuilder sql = new StringBuilder(1024);

                sql.Append("update Country set ");
                sql.Append("Name=" + DB.SQuote(name) + ",");
                sql.Append("TwoLetterISOCode=" + DB.SQuote(CommonLogic.Left(twoLetter, 2)) + ",");
                sql.Append("ThreeLetterISOCode=" + DB.SQuote(CommonLogic.Left(threeLetter, 3)) + ",");
                sql.Append("NumericISOCode=" + DB.SQuote(CommonLogic.Left(numericISO, 3)) + ",");
                sql.Append("Published=" + CommonLogic.IIF(published, 1, 0) + ",");
                sql.Append("PostalCodeRequired=" + CommonLogic.IIF(postalCodeReq, 1, 0) + ",");
                sql.Append("PostalCodeRegex=" + DB.SQuote(postalCodeRegEx) + ",");
                sql.Append("PostalCodeExample=" + DB.SQuote(postalCodeExample) + ",");
                sql.Append("DisplayOrder=" + order);
                sql.Append(" where CountryID=" + iden.ToString());


                try
                {
                    DB.ExecuteSQL(sql.ToString());
                    resetError("Item updated", false);
                    gMain.EditIndex = -1;
                    ViewState["SQLString"] = selectSQL;



                    for (int i = 0; i <= Request.Form.Count - 1; i++)
                    {
                        //TR_CLASSID_STATEID
                        if (Request.Form.Keys[i].IndexOf("TR_") != -1)
                        {
                            String[] keys = Request.Form.Keys[i].Split('_');
                            int CountryID = Localization.ParseUSInt(keys[2]);
                            int ClassID = Localization.ParseUSInt(keys[1]);
                            decimal tax = Decimal.Zero;
                            if (CountryID == Localization.ParseUSInt(iden))
                            {
                                try
                                {
                                    tax = Localization.ParseNativeDecimal(Request.Form[Request.Form.Keys[i]]);
                                }
                                catch { }
                                CountryTaxRate ctr = AppLogic.CountryTaxRatesTable[CountryID, ClassID];
                                try
                                {
                                    if (ctr == null)
                                    {
                                        AppLogic.CountryTaxRatesTable.Add(CountryID, ClassID, tax);
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

                    BuildGridData();
                }
                catch (Exception ex)
                {
                    throw new Exception(String.Format(AppLogic.GetString("admin.common.ErrUpdateDB", SkinID, LocaleSetting), sql.ToString() + ex.ToString()));
                }
            }
        }
        protected void gMain_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gMain.EditIndex = e.NewEditIndex;

            loadScript(true);

            BuildGridData();
        }
        protected void btnInsert_Click(object sender, EventArgs e)
        {
            resetError("", false);

            gMain.EditIndex = -1;
            ShowAddPanel(true);

            txt2ISO.Text = "";
            txt3ISO.Text = "";
            txtName.Text = "";
            txtNumericISO.Text = "";
            txtOrder.Text = "1";
            
        }

        protected void btnUpdateOrder_Click(object sender, EventArgs e)
        {
            UpdateTaxOrder();

            resetError("Display Order and Taxes updated.", false);
            BuildGridData();
        }

        protected void UpdateTaxOrder()
        {
            for (int i = 0; i <= Request.Form.Count - 1; i++)
            {
                if (Request.Form.Keys[i].IndexOf("DisplayOrder_") != -1)
                {
                    String[] keys = Request.Form.Keys[i].Split('_');
                    int CountryID = Localization.ParseUSInt(keys[1]);
                    int DispOrd = 1;
                    try
                    {
                        DispOrd = Localization.ParseUSInt(Request.Form[Request.Form.Keys[i]]);
                    }
                    catch { }
                    DB.ExecuteSQL("update Country set DisplayOrder=" + DispOrd.ToString() + " where CountryID=" + CountryID.ToString());
                }
            }

            //handle taxes
            for (int i = 0; i <= Request.Form.Count - 1; i++)
            {
                //TR_CLASSID_STATEID
                if (Request.Form.Keys[i].IndexOf("TR_") != -1)
                {
                    String[] keys = Request.Form.Keys[i].Split('_');
                    int CountryID = Localization.ParseUSInt(keys[2]);
                    int ClassID = Localization.ParseUSInt(keys[1]);
                    decimal tax = Decimal.Zero;
                    try
                    {
                        tax = Localization.ParseNativeDecimal(Request.Form[Request.Form.Keys[i]]);
                    }
                    catch { }
                    CountryTaxRate ctr = AppLogic.CountryTaxRatesTable[CountryID, ClassID];
                    try
                    {
                        if (ctr == null)
                        {
                            AppLogic.CountryTaxRatesTable.Add(CountryID, ClassID, tax);
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
                string twoLetter = txt2ISO.Text.Trim();
                string threeLetter = txt3ISO.Text.Trim();
                string numericISO = txtNumericISO.Text.Trim();
                int published = CommonLogic.IIF(cbPublished.Checked == true, 1, 0);
                int postalCodeReq = CommonLogic.IIF(cbPostalCodeReq.Checked == true, 1, 0); 
                string postalCodeRegEx = txtPostalCodeRegEx.Text.Trim();
                string postalCodeExample = txtPostalCodeExample.Text.Trim();
                int order = Localization.ParseNativeInt(txtOrder.Text.Trim());

                // ok to add them:
                String NewGUID = DB.GetNewGUID();
                sql.Append("insert into Country(CountryGUID,[Name],TwoLetterISOCode,ThreeLetterISOCode,NumericISOCode,Published,PostalCodeRequired,PostalCodeRegEx,PostalCodeExample,DisplayOrder) values(");
                sql.Append(DB.SQuote(NewGUID) + ",");
                sql.Append(DB.SQuote(name) + ",");
                sql.Append(DB.SQuote(twoLetter) + ",");
                sql.Append(DB.SQuote(threeLetter) + ",");
                sql.Append(DB.SQuote(numericISO) + ",");
                sql.Append(published + ",");
                sql.Append(postalCodeReq + ",");
                sql.Append(DB.SQuote(postalCodeRegEx) + ",");
                sql.Append(DB.SQuote(postalCodeExample) + ",");
                sql.Append(order + ")");

                try
                {
                    DB.ExecuteSQL(sql.ToString());
                    resetError(AppLogic.GetString("admin.countries.CountryAdded", SkinID, LocaleSetting), false);
                    ShowAddPanel(false);
                }
                catch
                {
                    resetError(AppLogic.GetString("admin.countries.ExistingCountry", SkinID, LocaleSetting), true);
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
                loadScript(true);
                pnlAdd.Visible = true;
                pnlGrid.Visible = false;
            }
            else
            {
                loadScript(false);
                pnlAdd.Visible = false;
                pnlGrid.Visible = true;

                BuildGridData();
            }
        }

        protected bool publishedCheck(object PublishedValue)
        {
            bool countryPublished = true;

            if (PublishedValue.ToString().Equals("1"))
            {
                countryPublished = true;
            }
            else
            {
                countryPublished = false;
            }

            return countryPublished;
        }

        protected bool postalCodeCheck(object PostalCodeValue)
        {
            bool postalCodeRequired = true;

            if (PostalCodeValue.ToString().Equals("1"))
            {
                postalCodeRequired = true;
            }
            else
            {
                postalCodeRequired = false;
            }

            return postalCodeRequired;
        }
    }
}


