// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{

    public partial class stringresourcepage : AdminPageBase
    {
        protected Customer cust;
        protected string selectSQL = "select * from StringResource ";

        private const int CURRENT_STORE_ID = 1;

        private List<Store> m_stores;
        public List<Store> Stores
        {
            get { return m_stores; }
            set { m_stores = value; }
        }

        private void InitializeStores()
        {
            Stores = Store.GetStoreList();

            if (Stores.Count > 1)
            {
                pnlMultiStore.Visible = true;
                cboStores.DataSource = Stores;
                cboStores.DataValueField = "Value";
                cboStores.DataTextField = "Text";
                cboStores.DataBind();
                //InitializeStoreSelection();
            }
        }

        public IEnumerable<ListItem> BindableStores
        {
            get
            {
                var defaultStore = Stores.FirstOrDefault(store => store.IsDefault);
                var datasource = new List<ListItem>();
                datasource.Add(new ListItem("All", "0")); //defaultStore.StoreID.ToString()));

                foreach (var store in Stores)
                {
                    string storeName = store.IsDefault ? store.Name + "(Default)" : store.Name;
                    datasource.Add(new ListItem(storeName, store.StoreID.ToString()));
                }

                return datasource;
            }
        }

        protected override void OnInit(EventArgs e)
        {
            InitializeStores();
            base.OnInit(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {            
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");            

            cust = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;

            if (!IsPostBack)
            {
                string query = CommonLogic.QueryStringCanBeDangerousContent("searchfor");
                string locale = CommonLogic.QueryStringCanBeDangerousContent("ShowLocaleSetting");

                loadTree();
                loadLocales();
                ViewState["Sort"] = "Name, StoreId";
                ViewState["SortOrder"] = "ASC";
                ViewState["SQLString"] = selectSQL;

                if ((query.Length > 0) || (locale.Length > 0))
                {
                    pnlAdd.Visible = false;
                    pnlGrid.Visible = true;
                    resultFilter(query, locale);
                }
                else
                {
                    ShowAddPanel(false);
                }
            }

            string menulocale = string.Empty;

            if (ddLocales.Items.Count < 3)
            {
                menulocale = Localization.GetDefaultLocale();
                divActions.Visible = true;
            }
            else
            {
                menulocale = ddLocales.SelectedValue;
            }

            if (!menulocale.Equals("Reset") && !menulocale.Equals("NEW String"))
            {
                divActions.Visible = true;
                                
                if (menulocale.Equals("en-US"))
                {
                    btnShowMissing.Visible = false;
                }
                else
                {
                    btnShowMissing.Visible = true;
                }

                //Confirmations
                if (DB.GetSqlN("select count(*) as N from StringResource with (NOLOCK) where LocaleSetting=" + DB.SQuote(menulocale)) == 0)
                {
                    btnLoadExcelServer.Text = "Load from Excel File On Server";
                    btnUploadExcel.Text = "Load from Excel File On Your PC";
                    btnClearLocale.Visible = false;
                    btnShowMissing.Visible = false;
                    btnShowModified.Visible = false;
                }
                else
                {
                    btnLoadExcelServer.Text = "ReLoad from Excel File On Server";
                    btnUploadExcel.Text = "ReLoad from Excel File On Your PC";
                    btnClearLocale.Visible = true;
                    btnShowMissing.Visible = true;
                    btnShowModified.Visible = true;
                }

                btnLoadExcelServer.Attributes.Add("onclick", "return confirm('Are you sure you want to reload all strings for the " + menulocale + " locale from the /StringResources/strings." + menulocale + ".xls file?')");
                btnClearLocale.Attributes.Add("onclick", "return confirm('Are you sure you want to delete all strings in the " + menulocale + " locale from the database?')");
                btnUploadExcel.Attributes.Add("onclick", "return confirm('Are you sure you want to upload and load an excel file with all strings for the " + menulocale + " locale?')");
            }
            else
            {
                divActions.Visible = false;
            }
        }

        protected void loadScript(bool load)
        {
            if (load)
            {
                
            }
            else
            {
                
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

        private void loadLocales()
        {
            try
            {
                int localeCount = DB.GetSqlN("select count(Name) as N from LocaleSetting with (NOLOCK)");
                ddLocales.Items.Clear();
                if (localeCount > 1)
                {
                    ddLocales.Items.Add(new ListItem("ALL LOCALES", "Reset"));
                }

                using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
                {
                    conn.Open();
                    using (IDataReader rs = DB.GetRS("select Name from LocaleSetting  with (NOLOCK)  order by DisplayOrder,Description",conn))
                    {
                        while (rs.Read())
                        {
                            ListItem myNode = new ListItem();
                            myNode.Value = DB.RSField(rs, "Name");
                            ddLocales.Items.Add(myNode);
                            
                        }
                    }
                    
                }

                if (ddLocales.Items.Count < 3)
                {
                    tblLocale.Visible = false;
                }
            }
            catch (Exception ex)
            {
                resetError(ex.ToString(), true);
            }

            if (ddLocales.Items.Count < 3)
            {
                tblLocale.Visible = false;
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            gMain.EditIndex = -1;
            resetError("", false);

            resultFilter("", "");
        }

        private const int NO_STORE_FILTERING_FILTER = 0;

        private const string STORE_FILTER = "StoreFilter";
        private const string DUPLICATED_STRING_RESOURCE = "DuplicatedStringResource";
        private const string DUPLICATED_FROM_STRING_RESOURCE = "DuplicatedFromStringResource";

        public int StoreFilter
        {
            get { return null == ViewState[STORE_FILTER] ? NO_STORE_FILTERING_FILTER : (int)ViewState[STORE_FILTER]; }
            set { ViewState[STORE_FILTER] = value; }
        }

        public int DuplicatedStringResource
        {
            get { return null == ViewState[DUPLICATED_STRING_RESOURCE] ? -1 : (int)ViewState[DUPLICATED_STRING_RESOURCE]; }
            set { ViewState[DUPLICATED_STRING_RESOURCE] = value; }
        }

        public int DuplicatedFromStringResource
        {
            get { return null == ViewState[DUPLICATED_FROM_STRING_RESOURCE] ? -1 : (int)ViewState[DUPLICATED_FROM_STRING_RESOURCE]; }
            set { ViewState[DUPLICATED_FROM_STRING_RESOURCE] = value; }
        }

        protected void resultFilter(string SearchFor, string locale)
        {
            if (SearchFor.Length == 0)
            {
                SearchFor = txtSearch.Text.Trim();
            }
            if (locale.Length == 0)
            {
                if (ddLocales.Items.Count < 3)
                {
                    locale = Localization.GetDefaultLocale();
                }
                else
                {
                    locale = ddLocales.SelectedValue;
                }
            }

            String sql = selectSQL + " with (NOLOCK) ";
            String WhereClause = String.Empty;

            //search filter
            if (SearchFor.Length != 0)
            {
                if (WhereClause.Length != 0)
                {
                    WhereClause += " and ";
                }
                WhereClause += " (Name like " + DB.SQuote("%" + SearchFor + "%") + " or ConfigValue like " + DB.SQuote("%" + SearchFor + "%") + ")";
            }

            //locale filter
            if ((locale.Length != 0) && (!locale.Equals("Reset")))
            {
                if (WhereClause.Length != 0)
                {
                    WhereClause += " and ";
                }
                WhereClause += " LocaleSetting like " + DB.SQuote(locale);
            }

            if (StoreFilter != NO_STORE_FILTERING_FILTER)
            {
                if (WhereClause.Length != 0)
                {
                    WhereClause += " and ";
                }
                WhereClause += " StoreId = " + StoreFilter.ToString();
            }

            //starts with filter
            string Index = "";
            for (int i = 0; i < treeMain.Nodes.Count; i++)
            {
                if (treeMain.Nodes[i].Selected)
                {
                    Index = treeMain.Nodes[i].Value;
                    break;
                }
            }
            if (Index.Length > 0)
            {
                if (!Index.Equals("All"))
                {
                    if (WhereClause.Length != 0)
                    {
                        WhereClause += " and ";
                    }
                    WhereClause += " Name like " + DB.SQuote(Index + "%");
                }
                if (Index.Equals("#"))
                {
                    if (WhereClause.Length != 0)
                    {
                        WhereClause += " and ";
                    }
                    WhereClause += " Name like (" + DB.SQuote("0%") + ")";
                    WhereClause += " OR Name like (" + DB.SQuote("1%") + ")";
                    WhereClause += " OR Name like (" + DB.SQuote("2%") + ")";
                    WhereClause += " OR Name like (" + DB.SQuote("3%") + ")";
                    WhereClause += " OR Name like (" + DB.SQuote("4%") + ")";
                    WhereClause += " OR Name like (" + DB.SQuote("5%") + ")";
                    WhereClause += " OR Name like (" + DB.SQuote("6%") + ")";
                    WhereClause += " OR Name like (" + DB.SQuote("7%") + ")";
                    WhereClause += " OR Name like (" + DB.SQuote("8%") + ")";
                    WhereClause += " OR Name like (" + DB.SQuote("9%") + ")";
                    WhereClause += " OR Name like (" + DB.SQuote("10%") + ")";
                }
            }

            //Super admin filter
            if (!cust.IsAdminSuperUser)
            {
                if (WhereClause.Length != 0)
                {
                    WhereClause += " and ";
                }
                WhereClause += " UPPER(Name) <> 'ADMIN_SUPERUSER' ";
            }
            if (WhereClause.Length != 0)
            {
                sql += " where " + WhereClause;
            }

            ViewState["SQLString"] = sql.ToString();
            sql += " order by " + ViewState["Sort"].ToString() + " " + ViewState["SortOrder"].ToString();

            buildGridData(sql);

            txtSearch.Text = SearchFor;
            ddLocales.SelectedIndex = -1;
            if (ddLocales.Items.Count > 2)
            {
                ddLocales.Items.FindByValue(locale).Selected = true;
            }
        }

        protected void ddLocales_SelectedIndexChanged(object sender, EventArgs e)
        {
            DuplicatedFromStringResource = -1;
            DuplicatedStringResource = -1;

            resetError("", false);
            resultFilter("", "");
        }

        protected void cboStores_SelectedIndexChanged(object sender, EventArgs e)
        {
            DuplicatedFromStringResource = -1;
            DuplicatedStringResource = -1;

            StoreFilter = cboStores.SelectedValue.ToNativeInt();
            resetError("", false);
            resultFilter("", "");
        }

        protected void treeMain_SelectedNodeChanged(object sender, EventArgs e)
        {
            DuplicatedFromStringResource = -1;
            DuplicatedStringResource = -1;

            gMain.EditIndex = -1;
            resetError("", false);

            resultFilter("", "");
        }

        protected void buildGridData(string sql)
        {
            //We need datatable for paging, dtareader does not support paging
            using (DataTable dt = new DataTable())
            {
                using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
                {
                    conn.Open();
                    using (IDataReader rs = DB.GetRS(sql, conn))
                    {
                        dt.Load(rs);
                        gMain.DataSource = dt;
                        gMain.DataBind();
                    }
                }
            }
        }
        
        protected void buildGridData()
        {
            string sql = ViewState["SQLString"].ToString();
            sql += " order by " + ViewState["Sort"].ToString() + " " + ViewState["SortOrder"].ToString();

            buildGridData(sql);
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

            ltError.Text = str;
        }

        protected void gMain_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            resetError("", false);

            gMain.EditIndex = -1;
            buildGridData();
        }
        protected void gMain_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if ((e.Row.RowState & DataControlRowState.Edit) == DataControlRowState.Edit)
            {
                DataRowView myrow = (DataRowView)e.Row.DataItem;
                string locale = myrow["LocaleSetting"].ToString();
                DropDownList dd = (DropDownList)e.Row.FindControl("ddLocale");

                using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
                {
                    conn.Open();
                    using (IDataReader rs = DB.GetRS("select * from LocaleSetting   with (NOLOCK)  order by displayorder,description", conn))
                    {
                        while (rs.Read())
                        {
                            dd.Items.Add(new ListItem(rs["Name"].ToString(), rs["Name"].ToString()));
                            if (rs["Name"].ToString().Equals(locale, StringComparison.InvariantCultureIgnoreCase))
                            {
                                dd.Items[dd.Items.Count - 1].Selected = true;
                            }

                        }
                    }
                }

                // bind the default value for the stores
                if (Stores.Count > 1)
                {
                    var cboEditStores = e.Row.FindControl<DropDownList>("cboEditStores");
                    cboEditStores.SelectedValue = myrow["StoreId"].ToString();
                }
            }

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
            resetError("", false);

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
            sql.Append("delete from StringResource where StringResourceID=" + iden.ToString());
            try
            {
                DB.ExecuteSQL(sql.ToString());
                buildGridData();
                resetError("Item Deleted", false);
            }
            catch (Exception ex)
            {
                throw new Exception("Couldn't update database: " + ex.ToString());
            }
        }

        protected void gMain_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            resetError("", false);
            gMain.PageIndex = e.NewPageIndex;
            gMain.EditIndex = -1;
            buildGridData();
        }

        protected bool CurrentRowWasDuplicatedFrom(DataRowView currentRow)
        {
            return (int)currentRow["StringResourceId"] == DuplicatedFromStringResource;
        }

        protected bool CurrentRowIsDuplicate(DataRowView currentRow)
        {
            return (int)currentRow["StringResourceId"] == DuplicatedStringResource;
        }

        protected void gMain_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            GridViewRow row = gMain.Rows[e.RowIndex];

            if (row != null)
            {
                string iden = row.Cells[2].Text.ToString(); // the StringResourceId column
                TextBox txtName = (TextBox)row.FindControl("txtName");
                TextBox txtValue = (TextBox)row.FindControl("txtValue");
                DropDownList ddLocale = (DropDownList)row.FindControl("ddLocale");
                StringBuilder sql = new StringBuilder(2500);

                int stringResourceId = iden.ToNativeInt();
                string name = txtName.Text;
                string selectedLocale = ddLocale.SelectedValue;
                string value = txtValue.Text;
                int prevStoreId = row.FindControl<HiddenField>("hdfPrevStoreId").Value.ToNativeInt();

                var defaultStore = Stores.FirstOrDefault(store => store.IsDefault);
                int storeId = 1; // default
                if (Stores.Count > 1)
                {
                    var cboEditStores = row.FindControl<DropDownList>("cboEditStores");
                    if (cboEditStores.SelectedIndex == 0) // All means also the default store
                    {
                        storeId = defaultStore.StoreID;
                    }
                    else
                    {
                        storeId = cboEditStores.SelectedValue.ToNativeInt();
                    }
                }
                else
                {
                    storeId = row.FindControl<Label>("lblStoreId").Text.ToNativeInt();
                }

                var owningStore = Stores.FirstOrDefault(store => store.StoreID == storeId);

                // check if the user specified a different storeid
                if(storeId != prevStoreId)
                {
                    // check if we have a previous string resource with that storeid+name+locale
                    var prevStringResource = StringResourceManager.GetStringResource(prevStoreId, selectedLocale, name);
                    var newStoreString = StringResourceManager.GetStringResource(storeId, selectedLocale, name);

                    // check if we have a duplicate on the destination store
                    if (newStoreString != null)
                    {
                        // just update that one instead
                        newStoreString.Update(name, selectedLocale, value);

                        string updateNotice = string.Format("Item [{0}] updated for store: {1} ({2})", name, owningStore.Name, owningStore.StoreID);
                        resetError(updateNotice, false);
                        gMain.EditIndex = -1;

                        // nuke the other store string
                        if (prevStringResource != null)
                        {
                            // nuke the previous one
                            prevStringResource.Owner.Remove(prevStringResource);
                        }
                    }
                    else
                    {
                        // create a copy of that string resource for this store
                        newStoreString = StringResource.Create(storeId, name, selectedLocale, value);

                        var storeStrings = StringResourceManager.GetStringResources(storeId);
                        storeStrings.Add(newStoreString);

                        DuplicatedStringResource = newStoreString.StringResourceID;
                        if(prevStringResource != null)
                        {
                            DuplicatedFromStringResource = prevStringResource.StringResourceID;
                        }

                        string updateNotice = string.Format("Item [{0}] duplicated for store: {1} ({2})", name, owningStore.Name, owningStore.StoreID);
                        resetError(updateNotice, false);
                        gMain.EditIndex = -1;
                    }
                }
                else
                {
                    // find if there's an existing string resource with that name+locale+storeid pair
                    var dupString = StringResourceManager.GetStringResource(storeId, selectedLocale, name);
                    if (dupString != null && dupString.StringResourceID != stringResourceId)
                    {
                        // prompt for error editing duplicate in same Store Strings
                        resetError("Another string exists with that Name and Locale combination.", true);
                        return;
                    }

                    // just edit the current string resource
                    var str = StringResourceManager.GetStringResource(storeId, stringResourceId);
                    if (str != null)
                    {
                        str.Update(name, selectedLocale, value);
                        resetError("Item updated", false);
                        gMain.EditIndex = -1;
                    }
                    else
                    {
                        resetError("Item could not be found in collection", true);
                    }
                }

                resultFilter("", Localization.CheckLocaleSettingForProperCase(selectedLocale));

            }
        }
        protected void gMain_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gMain.EditIndex = e.NewEditIndex;

            buildGridData();

        }        

        protected void btnLoadExcelServer_Click(object sender, EventArgs e)
        {
            resetError("", false);

            string selectedLocale = ddLocales.SelectedValue;

            string stringResourceFilePath = string.Empty;
            bool stringResourceFileExists = StringResourceManager.CheckStringResourceExcelFileExists(selectedLocale);
            if (stringResourceFileExists)
            {
                Response.Redirect(string.Format(AppLogic.AdminLinkUrl("importstringresourcefile1.aspx")+ "?showlocalesetting={0}&master=true", selectedLocale));
            }
            else
            {
                resetError(string.Format("String Resource File {0} not found!!!",stringResourceFilePath), true);
            }
        }

        protected void btnClearLocale_Click(object sender, EventArgs e)
        {
            resetError("", false);
            DB.ExecuteSQL("delete from StringResource where LocaleSetting=" + DB.SQuote(ddLocales.SelectedValue));
            //AppLogic.LoadStringResourcesFromDB(false);
            resetError("Locale Cleared.", false);
            buildGridData();
            btnClearLocale.Visible = false;
            btnShowMissing.Visible = false;
            btnShowModified.Visible = false;
        }
        protected void btnUploadExcel_Click(object sender, EventArgs e)
        {
            Response.Redirect(AppLogic.AdminLinkUrl("importstringresourcefile1.aspx") + "?showlocalesetting=" + ddLocales.SelectedValue);
        }
        protected void btnShowMissing_Click(object sender, EventArgs e)
        {
            Response.Redirect(AppLogic.AdminLinkUrl("stringresourcerpt.aspx") +"?reporttype=missing&ShowLocaleSetting=" + ddLocales.SelectedValue);
        }
        protected void btnShowModified_Click(object sender, EventArgs e)
        {
            Response.Redirect(AppLogic.AdminLinkUrl("stringresourcerpt.aspx") +"?reporttype=modified&ShowLocaleSetting=" + ddLocales.SelectedValue);
        }

        protected void btnInsert_Click(object sender, EventArgs e)
        {
            resetError("", false);

            gMain.EditIndex = -1;
            ShowAddPanel(true);

            txtDescription.Text = "";
            txtName.Text = "";

            ddLocale.Items.Clear();
            ddLocale.Items.Add(new ListItem("- Select -", "0"));

            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS("select * from LocaleSetting   with (NOLOCK)  order by displayorder,description",conn))
                {
                    while (rs.Read())
                    {
                        ddLocale.Items.Add(new ListItem(rs["Name"].ToString(), rs["Name"].ToString()));
                    }
                }
            }
            try
            {
                if (ddLocales.SelectedValue != "Reset")
                {
                    ddLocale.SelectedValue = ddLocales.SelectedValue;
                }                    
            }
            catch { }

            if (ddLocale.Items.Count < 3)
            {
                trLocale.Visible = false;
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            resetError("", false);
            StringBuilder sql = new StringBuilder();

            if (ValidInput())
            {
                string name = txtName.Text.Trim();
                string value = txtDescription.Text.Trim();
                string locale = ddLocale.SelectedValue;

                if (locale.Equals(string.Empty))
                {
                    locale = Localization.GetDefaultLocale();
                }

                var storeId = Stores[0].StoreID;
                if (Stores.Count > 1)
                {
                    storeId = cboStoreAddString.SelectedValue.ToNativeInt();
                }


                AspDotNetStorefrontCore.StringResource sr = StringResourceManager.GetStringResource(storeId, name, locale);  //Must fully qualify this for VB
                if (sr == null)
                {
                    var stringResources = StringResourceManager.GetStringResources(storeId);                    

                    string err = stringResources.Add(storeId, name, locale, value);
                    if (err == string.Empty)
                    {
                        resetError("String Resource added.", false);
                        ShowAddPanel(false);
                    }
                    else
                    {
                        resetError("String Resource was not added.  The following error occurred: " + err, true);
                        ShowAddPanel(true);
                    }
                }
                else
                {
                    resetError("String Resource already exists.", true);
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

                if (Stores.Count > 1)
                {
                    trStore.Visible = true;

                    cboStoreAddString.DataSource = Stores;
                    cboStoreAddString.DataValueField = "StoreID";
                    cboStoreAddString.DataTextField = "Name";
                    cboStoreAddString.DataBind();
                }
                else
                {
                    trStore.Visible = false;
                }
            }
            else
            {
                loadScript(false);
                pnlAdd.Visible = false;
                pnlGrid.Visible = true;

                trStore.Visible = false;

                buildGridData();
            }
        }
    }
}
