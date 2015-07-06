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
    public partial class quantitydiscounts : AdminPageBase
    {
        protected Customer cust;

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            cust = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;

            if (!IsPostBack)
            {
                ViewState["EditingQD"] = false;
                ViewState["EditingQDID"] = "0";
                ViewState["UseBoxes"] = false;

                loadTree();
                loadScript(false);
                resetForm();
                phMain.Visible = false;

                btnDelete.Attributes.Add("onClick", "return confirm('Confirm Delete');");
            }
        }

        private void loadTree()
        {
            try
            {
                treeMain.Nodes.Clear();

                using (SqlConnection conn = DB.dbConn())
                {
                    conn.Open();
                    using (IDataReader rs = DB.GetRS("select * from QuantityDiscount   with (NOLOCK)  order by DisplayOrder,Name", conn))
                    {
                        while (rs.Read())
                        {
                            TreeNode myNode = new TreeNode();
                            myNode.Text = Server.HtmlEncode(DB.RSFieldByLocale(rs, "Name", cust.LocaleSetting));
                            myNode.Value = DB.RSFieldInt(rs, "QuantityDiscountID").ToString();
                            myNode.ImageUrl = AppLogic.LocateImageURL("~/App_Themes/Admin_Default/images/icons/dot.gif");
                            treeMain.Nodes.Add(myNode);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                resetError(ex.ToString(), true);
            }
        }

        protected void treeMain_SelectedNodeChanged(object sender, EventArgs e)
        {
            ltValid.Text = "";
            resetForm();
            ViewState["EditingQD"] = true;
            loadScript(true);

            resetError("", false);
            phMain.Visible = true;

            string index = treeMain.SelectedNode.Value;
            ViewState["EditingQDID"] = index;

            btnInsert.Visible = true;
            btnInsert.Text = "Insert NEW Values";
            divInitial.Visible = false;
            divInitialDD.Visible = false;
            ViewState["UseBoxes"] = false;

            getQDDetails();
        }

        protected void getQDDetails()
        {
            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS("select * from QuantityDiscount   with (NOLOCK)  where QuantityDiscountID=" + ViewState["EditingQDID"].ToString(), conn))
                {
                    if (!rs.Read())
                    {
                        rs.Close();
                        resetError("Unable to retrieve data.", true);
                        return;
                    }

                    //editing QD
                    ltMode.Text = "Editing";
                    btnSubmit.Text = "Update Discount Table Name";
                    btnDelete.Visible = true;

                    ltName.Text = AppLogic.GetLocaleEntryFields(DB.RSField(rs, "Name"), "Name", false, true, true, "Please enter the quantity discount table name", 100, 30, 0, 0, false);
                    ddlDscntType.SelectedIndex = DB.RSFieldTinyInt(rs, "DiscountType");
                    showDiscountGrid();
                }
            }
        }

        private void showDiscountGrid()
        {
            divGrid.Visible = true;
            buildGridData("LowQuantity");
        }
        private void hideDiscountGrid()
        {
            divGrid.Visible = false;
        }

        protected void resetError(string error, bool isError)
        {
            string str = "<font class=\"noticeMsg\">NOTICE:</font>&nbsp;&nbsp;&nbsp;";
            if (isError)
            {
                str = "<font class=\"errorMsg\">ERROR:</font>&nbsp;&nbsp;&nbsp;";
            }

            if (error.Length > 0)
            {
                str += error + "";
            }
            else
            {
                str = "";
            }

            ltError.Text = str;
        }
        protected void btnAdd_Click(object sender, EventArgs e)
        {
            ltValid.Text = "";
            ViewState["EditingQD"] = false;
            ViewState["EditingQDID"] = "0";
            loadScript(true);
            phMain.Visible = true;
            btnDelete.Visible = false;
            resetForm();
            loadTree();
            //new QD
            ltMode.Text = "Adding a";
            btnSubmit.Text = "Add Discount Table";

            divInitialDD.Visible = true;
            ViewState["UseBoxes"] = true;
            btnInsert.Visible = false;

            hideDiscountGrid();

        }

        protected void loadScript(bool load)
        {
            if (load)
            {
                if (AppLogic.NumLocaleSettingsInstalled() > 1)
                {
                    ltScript1.Text = "<script type='text/javascript' src='Scripts/tabs.js'></script>";
                }
            }
            else
            {
                ltScript.Text = "";
                ltStyles.Text = "";
            }
        }

        protected bool validateInput()
        {
            string frmName = AppLogic.FormLocaleXml("Name");
            if (frmName.Equals("<ml></ml>") || string.IsNullOrEmpty(frmName))
            {
                string temp = "Please enter the quantity discount table name! <script type=\"text/javascript\">alert('Please enter the quantity discount table name!');</script>";
                resetError(temp, true);
                return false;
            }
            return true;
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            ltValid.Text = "";
            bool Editing = Localization.ParseBoolean(ViewState["EditingQD"].ToString());
            string QDID = ViewState["EditingQDID"].ToString();
            IDataReader rs;

            if (validateInput())
            {
                divInitialDD.Visible = false;

                try
                {
                    StringBuilder sql = new StringBuilder(2500);
                    if (!Editing)
                    {
                        // ok to add:

                        ResetTxtControls();

                        String NewGUID = DB.GetNewGUID();
                        sql.Append("insert into quantitydiscount(QuantityDiscountGUID,Name,DiscountType) values(");
                        sql.Append(DB.SQuote(NewGUID) + ",");
                        sql.Append(DB.SQuote(AppLogic.FormLocaleXml("Name")) + ",");
                        sql.Append(ddlDscntType.SelectedValue);
                        sql.Append(")");

                        DB.ExecuteSQL(sql.ToString());
                        resetError("Discount Table added.", false);

                        using (SqlConnection conn = DB.dbConn())
                        {
                            conn.Open();
                            using (rs = DB.GetRS("select QuantityDiscountID from QuantityDiscount   with (NOLOCK)  where QuantityDiscountGUID=" + DB.SQuote(NewGUID), conn))
                            {
                                rs.Read();
                                QDID = DB.RSFieldInt(rs, "QuantityDiscountID").ToString();
                            }
                        }

                        ViewState["EditingQD"] = true;
                        ViewState["EditingQDID"] = QDID;
                        resetForm();
                        loadTree();
                        getQDDetails();

                        btnInsert.Visible = true;
                        btnInsert.Text = "Insert Initial Values";

                        //based on the number of initial values selected, display boxes
                        divInitial.Visible = true;
                        int boxes = Localization.ParseUSInt(ddValues.SelectedValue);
                        if (boxes == 3)
                        {
                            tr4.Visible = false;
                            tr5.Visible = false;
                        }
                        if (boxes == 1)
                        {
                            tr2.Visible = false;
                            tr3.Visible = false;
                            tr4.Visible = false;
                            tr5.Visible = false;
                        }
                    }
                    else
                    {
                        // ok to update:
                        sql.Append("update quantitydiscount set ");
                        sql.Append("Name=" + DB.SQuote(AppLogic.FormLocaleXml("Name")) + ",");
                        sql.Append("DiscountType=" + ddlDscntType.SelectedValue);
                        sql.Append(" where QuantityDiscountID=" + QDID.ToString());
                        DB.ExecuteSQL(sql.ToString());

                        resetError("Discount Table name updated.", false);
                        loadTree();
                        getQDDetails();

                        btnInsert.Visible = true;
                        btnInsert.Text = "Insert NEW Values";
                        divInitial.Visible = false;
                    }
                }
                catch (Exception ex)
                {
                    resetError("Error occurred: " + ex.ToString(), true);
                }

                divInitialDD.Visible = false;
            }
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            ltValid.Text = "";
            string QDID = ViewState["EditingQDID"].ToString();


            //update entity table
            DB.ExecuteSQL("update dbo.category set QuantityDiscountID=0 where QuantityDiscountID=" + QDID);
            DB.ExecuteSQL("update dbo.section set QuantityDiscountID=0 where QuantityDiscountID=" + QDID);
            DB.ExecuteSQL("update dbo.genre set QuantityDiscountID=0 where QuantityDiscountID=" + QDID);
            DB.ExecuteSQL("update dbo.manufacturer set QuantityDiscountID=0 where QuantityDiscountID=" + QDID);
            DB.ExecuteSQL("update dbo.library set QuantityDiscountID=0 where QuantityDiscountID=" + QDID);
            DB.ExecuteSQL("update dbo.distributor set QuantityDiscountID=0 where QuantityDiscountID=" + QDID);
            DB.ExecuteSQL("update dbo.vector set QuantityDiscountID=0 where QuantityDiscountID=" + QDID);

            //update product table
            DB.ExecuteSQL("update dbo.product set QuantityDiscountID=0 where QuantityDiscountID=" + QDID);
            DB.ExecuteSQL("delete from quantitydiscounttable where quantitydiscountid=" + QDID);
            DB.ExecuteSQL("delete from QuantityDiscount where QuantityDiscountID=" + QDID);

            phMain.Visible = false;
            hideDiscountGrid();
            loadTree();
            loadScript(false);
            ViewState["EditingQD"] = false;
            ViewState["EditingQDID"] = "0";
            resetError("Discount Table deleted.", false);
        }

        protected void resetForm()
        {
            hideDiscountGrid();
            ltName.Text = AppLogic.GetLocaleEntryFields("", "Name", false, true, true, "Please enter the quantity discount table name", 100, 30, 0, 0, false);
        }

        protected bool validateForm()
        {
            bool valid = true;
            string temp = "";

            if ((AppLogic.FormLocaleXml("Name").Length == 0) || (AppLogic.FormLocaleXml("Name").Equals("<ml></ml>")))
            {
                valid = false;
                temp += "Please fill out the Name";
            }

            if (!valid)
            {
                ltName.Text = AppLogic.GetLocaleEntryFields(AppLogic.FormLocaleXml("Name"), "Name", false, true, true, "Please enter the quantity discount table name", 100, 30, 0, 0, false);

                ltValid.Text = "<script type=\"text/javascript\">alert('" + temp + "');</script>";
            }

            return valid;
        }

        protected void buildGridData(string order)
        {
            string sql = "select * from QuantityDiscountTable   with (NOLOCK)  where QuantityDiscountID=" + ViewState["EditingQDID"].ToString() + " order by " + order;

            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS(sql, conn))
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
        protected void gMain_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            //if new item and cancel, must delete
            if (Localization.ParseBoolean(ViewState["IsInsert"].ToString()))
            {
                GridViewRow row = gMain.Rows[e.RowIndex];
                if (row != null)
                {
                    int iden = Localization.ParseNativeInt(row.Cells[1].Text.ToString());
                    deleteRowPerm(iden);
                }
            }

            ViewState["IsInsert"] = false;

            gMain.EditIndex = -1;
            buildGridData("LowQuantity");
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
            if ((e.Row.RowState & DataControlRowState.Edit) == DataControlRowState.Edit)
            {
                DataRowView myrow = (DataRowView)e.Row.DataItem;

                TextBox txt = (TextBox)e.Row.FindControl("txtHigh");
                txt.Attributes.Add("onKeyPress", "javascript:if (event.keyCode == 13) return false;");
                txt = (TextBox)e.Row.FindControl("txtLow");
                txt.Attributes.Add("onKeyPress", "javascript:if (event.keyCode == 13) return false;");
                txt = (TextBox)e.Row.FindControl("txtPercent");
                txt.Attributes.Add("onKeyPress", "javascript:if (event.keyCode == 13) return false;");
            }
        }
        protected void gMain_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            resetError("", false);

            if (e.CommandName == "DeleteItem")
            {
                ViewState["IsInsert"] = false;
                gMain.EditIndex = -1;
                int iden = Localization.ParseNativeInt(e.CommandArgument.ToString());
                deleteRowPerm(iden);
            }
        }
        protected void deleteRowPerm(int iden)
        {
            StringBuilder sql = new StringBuilder(2500);
            sql.Append("DELETE FROM QuantityDiscountTable where QuantityDiscountTableID=" + iden.ToString());
            try
            {
                DB.ExecuteSQL(sql.ToString());
                buildGridData("LowQuantity");
                resetError("Discount Table Item Deleted", false);
            }
            catch (Exception ex)
            {
                throw new Exception("Couldn't delete from database: " + ex.ToString());
            }
        }
        protected void gMain_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            ViewState["IsInsert"] = false;
            GridViewRow row = gMain.Rows[e.RowIndex];

            if (row != null)
            {
                string iden = row.Cells[1].Text.ToString();
                TextBox low = (TextBox)row.FindControl("txtLow");
                TextBox high = (TextBox)row.FindControl("txtHigh");
                TextBox percent = (TextBox)row.FindControl("txtPercent");

                if (Localization.ParseUSInt(low.Text) > Localization.ParseUSInt(high.Text))
                {
                    high.Text = (Localization.ParseUSInt(low.Text) + 1).ToString();
                }

                StringBuilder sql = new StringBuilder(2500);

                sql.Append("update QuantityDiscountTable set ");
                sql.Append("LowQuantity=" + Localization.IntStringForDB(Localization.ParseUSInt(low.Text)) + ",");
                sql.Append("HighQuantity=" + Localization.IntStringForDB(Localization.ParseUSInt(high.Text)) + ",");
                sql.Append("DiscountPercent=" + Localization.DecimalStringForDB(Localization.ParseUSDecimal(percent.Text)));
                sql.Append(" where QuantityDiscountTableID=" + iden);
                DB.ExecuteSQL(sql.ToString());

                resetError("Quantity Table Item updated", false);

                try
                {
                    DB.ExecuteSQL(sql.ToString());
                    gMain.EditIndex = -1;
                    DB.ExecuteSQL("Update QuantityDiscountTable set HighQuantity=999999 where HighQuantity=0 and LowQuantity<>0");
                    buildGridData("LowQuantity");
                }
                catch
                {
                    resetError("Couldn't update values.", true);
                }
            }
        }
        protected void gMain_RowEditing(object sender, GridViewEditEventArgs e)
        {
            ViewState["IsInsert"] = false;
            gMain.EditIndex = e.NewEditIndex;

            loadScript(true);

            buildGridData("LowQuantity");
        }
        protected void btnInsert_Click(object sender, EventArgs e)
        {
            ViewState["IsInsert"] = false;
            bool initial = Localization.ParseBoolean(ViewState["UseBoxes"].ToString());

            gMain.EditIndex = -1;
            StringBuilder sql = new StringBuilder(2500);

            string QuantityDiscountID = ViewState["EditingQDID"].ToString();
            String NewGUID = DB.GetNewGUID();

            if (!initial)
            {
                try
                {
                    DB.ExecuteSQL("insert into QuantityDiscountTable(QuantityDiscountTableGUID,QuantityDiscountID,LowQuantity,HighQuantity,DiscountPercent,CreatedOn) values(" + DB.SQuote(NewGUID) + "," + QuantityDiscountID.ToString() + "," + Localization.IntStringForDB(0) + "," + Localization.IntStringForDB(999999) + "," + Localization.DecimalStringForDB(Localization.ParseUSDecimal("0.00")) + "," + DB.DateQuote(Localization.ToDBDateTimeString(System.DateTime.Now)) + ")");
                    gMain.EditIndex = 0;
                    buildGridData("QuantityDiscountTableID DESC");
                    resetError("Quantity Discount Item added.", false);
                    ViewState["IsInsert"] = true;


                    GridViewRow row = gMain.Rows[0];
                    RangeValidator PctValidator = (RangeValidator)row.FindControl("RangeValidator2");

                }
                catch (Exception ex)
                {
                    throw new Exception("Couldn't update database: " + ex.ToString());
                }
            }
            else
            {
                //get initial box values
                int boxes = Localization.ParseUSInt(ddValues.SelectedValue);

                int low1 = Localization.ParseUSInt(txtLow1.Text);
                int high1 = Localization.ParseUSInt(txtHigh1.Text);
                decimal percent1 = Localization.ParseUSDecimal(txtPercent1.Text);
                int low2 = Localization.ParseUSInt(txtLow2.Text);
                int high2 = Localization.ParseUSInt(txtHigh2.Text);
                decimal percent2 = Localization.ParseUSDecimal(txtPercent2.Text);
                int low3 = Localization.ParseUSInt(txtLow3.Text);
                int high3 = Localization.ParseUSInt(txtHigh3.Text);
                decimal percent3 = Localization.ParseUSDecimal(txtPercent3.Text);
                int low4 = Localization.ParseUSInt(txtLow4.Text);
                int high4 = Localization.ParseUSInt(txtHigh4.Text);
                decimal percent4 = Localization.ParseUSDecimal(txtPercent4.Text);
                int low5 = Localization.ParseUSInt(txtLow5.Text);
                int high5 = Localization.ParseUSInt(txtHigh5.Text);
                decimal percent5 = Localization.ParseUSDecimal(txtPercent5.Text);

                //insert first
                if (low1 != 0 || high1 != 0)
                {
                    DB.ExecuteSQL("insert into QuantityDiscountTable(QuantityDiscountTableGUID,QuantityDiscountID,LowQuantity,HighQuantity,DiscountPercent,CreatedOn) values(" + DB.SQuote(DB.GetNewGUID()) + "," + QuantityDiscountID.ToString() + "," + Localization.IntStringForDB(low1) + "," + Localization.IntStringForDB(high1) + "," + Localization.DecimalStringForDB(percent1) + "," + DB.DateQuote(Localization.ToDBDateTimeString(System.DateTime.Now)) + ")");
                }

                if (boxes > 1)
                {
                    //insert second and third
                    if (low2 != 0 || high2 != 0)
                    {
                        DB.ExecuteSQL("insert into QuantityDiscountTable(QuantityDiscountTableGUID,QuantityDiscountID,LowQuantity,HighQuantity,DiscountPercent,CreatedOn) values(" + DB.SQuote(DB.GetNewGUID()) + "," + QuantityDiscountID.ToString() + "," + Localization.IntStringForDB(low2) + "," + Localization.IntStringForDB(high2) + "," + Localization.DecimalStringForDB(percent2) + "," + DB.DateQuote(Localization.ToDBDateTimeString(System.DateTime.Now)) + ")");
                    }
                    if (low3 != 0 || high3 != 0)
                    {
                        DB.ExecuteSQL("insert into QuantityDiscountTable(QuantityDiscountTableGUID,QuantityDiscountID,LowQuantity,HighQuantity,DiscountPercent,CreatedOn) values(" + DB.SQuote(DB.GetNewGUID()) + "," + QuantityDiscountID.ToString() + "," + Localization.IntStringForDB(low3) + "," + Localization.IntStringForDB(high3) + "," + Localization.DecimalStringForDB(percent3) + "," + DB.DateQuote(Localization.ToDBDateTimeString(System.DateTime.Now)) + ")");
                    }
                }

                if (boxes > 3)
                {
                    //insert fourth and fifth
                    if (low4 != 0 || high4 != 0)
                    {
                        DB.ExecuteSQL("insert into QuantityDiscountTable(QuantityDiscountTableGUID,QuantityDiscountID,LowQuantity,HighQuantity,DiscountPercent,CreatedOn) values(" + DB.SQuote(DB.GetNewGUID()) + "," + QuantityDiscountID.ToString() + "," + Localization.IntStringForDB(low4) + "," + Localization.IntStringForDB(high4) + "," + Localization.DecimalStringForDB(percent4) + "," + DB.DateQuote(Localization.ToDBDateTimeString(System.DateTime.Now)) + ")");
                    }
                    if (low5 != 0 || high5 != 0)
                    {
                        DB.ExecuteSQL("insert into QuantityDiscountTable(QuantityDiscountTableGUID,QuantityDiscountID,LowQuantity,HighQuantity,DiscountPercent,CreatedOn) values(" + DB.SQuote(DB.GetNewGUID()) + "," + QuantityDiscountID.ToString() + "," + Localization.IntStringForDB(low5) + "," + Localization.IntStringForDB(high5) + "," + Localization.DecimalStringForDB(percent5) + "," + DB.DateQuote(Localization.ToDBDateTimeString(System.DateTime.Now)) + ")");
                    }
                }
                divInitial.Visible = false;

                gMain.EditIndex = -1;
                DB.ExecuteSQL("Update QuantityDiscountTable set HighQuantity=999999 where HighQuantity=0 and LowQuantity<>0");

                buildGridData("LowQuantity");
                resetError("Quantity Discount Items added.", false);
                ViewState["UseBoxes"] = false;
                btnInsert.Text = "Insert NEW Values";
            }
        }

        private void ResetTxtControls()
        {
            txtLow1.Text = string.Empty;
            txtHigh1.Text = string.Empty;
            txtPercent1.Text = string.Empty;
            txtLow2.Text = string.Empty;
            txtHigh2.Text = string.Empty;
            txtPercent2.Text = string.Empty;
            txtLow3.Text = string.Empty;
            txtHigh3.Text = string.Empty;
            txtPercent3.Text = string.Empty;
            txtLow4.Text = string.Empty;
            txtHigh4.Text = string.Empty;
            txtPercent4.Text = string.Empty;
            txtLow5.Text = string.Empty;
            txtHigh5.Text = string.Empty;
            txtPercent5.Text = string.Empty;
        }

        public string DiscountUpperBound()
        {
            if (ddlDscntType.SelectedValue == "0")
            {
                return "100";
            }
            else
            {
                return int.MaxValue.ToString();
            }
        }
    }
}
