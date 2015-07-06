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
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{ 

public partial class entityProducts : AdminPageBase
{
    private EntityHelper entity;
    private string eName;
    private int eID;
    private EntitySpecs eSpecs;
    private int SiteID = 1;
    protected string selectSQL = "select * from Product";

    protected void Page_Load(object sender, EventArgs e)
    {
        Response.CacheControl = "private";
        Response.Expires = 0;
        Response.AddHeader("pragma", "no-cache");

        eID = CommonLogic.QueryStringNativeInt("EntityFilterID");
        eName = CommonLogic.QueryStringCanBeDangerousContent("EntityName");
        eSpecs = EntityDefinitions.LookupSpecs(eName);

        switch (eName.ToUpperInvariant())
        {
            case "SECTION":
                ltPreEntity.Text = AppLogic.GetString("admin.common.SectionApos", SkinID, LocaleSetting);
                entity = new EntityHelper(EntityDefinitions.readonly_SectionEntitySpecs, 0);
                break;
            case "MANUFACTURER":
                ltPreEntity.Text = AppLogic.GetString("admin.common.ManufacturerApos", SkinID, LocaleSetting);
                entity = new EntityHelper(EntityDefinitions.readonly_ManufacturerEntitySpecs, 0);
                break;
            case "DISTRIBUTOR":
                ltPreEntity.Text = AppLogic.GetString("admin.common.DistributorApos", SkinID, LocaleSetting);
                entity = new EntityHelper(EntityDefinitions.readonly_DistributorEntitySpecs, 0);
                break;
            case "GENRE":
                ltPreEntity.Text = AppLogic.GetString("admin.common.GenreApos", SkinID, LocaleSetting);
                entity = new EntityHelper(EntityDefinitions.readonly_GenreEntitySpecs, 0);
                break;
            case "VECTOR":
                ltPreEntity.Text = AppLogic.GetString("admin.common.VectorApos", SkinID, LocaleSetting);
                entity = new EntityHelper(EntityDefinitions.readonly_VectorEntitySpecs, 0);
                break;
            case "LIBRARY":
                ltPreEntity.Text = AppLogic.GetString("admin.common.LibraryApos", SkinID, LocaleSetting);
                entity = new EntityHelper(EntityDefinitions.readonly_LibraryEntitySpecs, 0);
                break;
            default:
                ltPreEntity.Text = AppLogic.GetString("admin.common.CategoryApos", SkinID, LocaleSetting);
                entity = new EntityHelper(EntityDefinitions.readonly_CategoryEntitySpecs, 0);
                break;
        }

        if (!IsPostBack)
        {
            ltEntity.Text = entity.GetEntityBreadcrumb6(eID, LocaleSetting);

            string query = CommonLogic.QueryStringCanBeDangerousContent("searchfor");

            loadTree();
            loadTypes();

            ViewState["SQLString"] = selectSQL;

            //set page settings
            if (ThisCustomer.ThisCustomerSession.Session("ProductsSort").Length == 0)
            {
                ViewState["Sort"] = "Name";
            }
            else
            {
                ViewState["Sort"] = ThisCustomer.ThisCustomerSession.Session("ProductsSort");
            }
            if (ThisCustomer.ThisCustomerSession.Session("ProductsOrder").Length == 0)
            {
                ViewState["SortOrder"] = "ASC";
            }
            else
            {
                ViewState["SortOrder"] = ThisCustomer.ThisCustomerSession.Session("ProductsOrder");
            }
            if (ThisCustomer.ThisCustomerSession.Session("ProductsSearch").Length != 0)
            {
                query = ThisCustomer.ThisCustomerSession.Session("ProductsSearch");
            }
            if (ThisCustomer.ThisCustomerSession.Session("ProductsTree").Length != 0)
            {
                treeMain.FindNode(ThisCustomer.ThisCustomerSession.Session("ProductsTree")).Selected = true;
            }
            if (ThisCustomer.ThisCustomerSession.Session("ProductsType").Length != 0)
            {
                ddTypes.Items.FindByValue(ThisCustomer.ThisCustomerSession.Session("ProductsType")).Selected = true;
            }

            resultFilter(query);

            txtSearch.Attributes.Add("onKeyPress", "javascript:if (event.keyCode == 13) __doPostBack('btnSearch','')");

            if (AppLogic.MaxProductsExceeded())
            {
                btnAdd.Enabled = false;
                btnAdd.CssClass = "normalButtonsDisabled";
                ltError.Text = "<font class=\"errorMsg\">" + AppLogic.GetString("admin.entityProducts.ErrorMsg", SkinID, LocaleSetting) + "</font>&nbsp;&nbsp;&nbsp;";
            }
            else
            {
                btnAdd.Enabled = true;
                btnAdd.CssClass = "normalButtons";
            }
        }
    }

    private void loadTree()
    {
        try
        {
            string index = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            treeMain.Nodes.Clear();
            treeMain.Nodes.Add(new TreeNode("All", "All", AppLogic.LocateImageURL ("~/App_Themes/Admin_Default/images/icons/dot.gif")));

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

    private void loadTypes()
    {
        try
        {
            ddTypes.Items.Clear();
            ddTypes.Items.Add(new ListItem("All Product Types", "0"));

            string prodTypeSql = string.Empty;
            prodTypeSql = "select count(*) as N from ProductType   with (NOLOCK); select * from ProductType   with (NOLOCK)  order by DisplayOrder,Name";

            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS(prodTypeSql, dbconn))
                {
                    if (rs.Read() && DB.RSFieldInt(rs, "N") > 0)
                    {
                        if (rs.NextResult())
                        {
                            while (rs.Read())
                            {
                                ListItem myNode = new ListItem();
                                myNode.Value = DB.RSFieldInt(rs, "ProductTypeID").ToString();
                                myNode.Text = DB.RSFieldByLocale(rs, "Name", Localization.GetDefaultLocale());
                                ddTypes.Items.Add(myNode);
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            resetError(ex.ToString(), true);
        }
    }

    private int _entityId = 0;
    private string _entityName = string.Empty;

    protected void resultFilter(string SearchFor)
    {
        int CategoryFilterID = CommonLogic.QueryStringUSInt("CategoryFilterID");
        int SectionFilterID = CommonLogic.QueryStringUSInt("SectionFilterID");
        int ProductTypeFilterID = CommonLogic.QueryStringUSInt("ProductTypeFilterID");
        int ManufacturerFilterID = CommonLogic.QueryStringUSInt("ManufacturerFilterID");
        int DistributorFilterID = CommonLogic.QueryStringUSInt("DistributorFilterID");
        int GenreFilterID = CommonLogic.QueryStringUSInt("GenreFilterID");
        int VectorFilterID = CommonLogic.QueryStringUSInt("VectorFilterID");
        int AffiliateFilterID = CommonLogic.QueryStringUSInt("AffiliateFilterID");
        int CustomerLevelFilterID = CommonLogic.QueryStringUSInt("CustomerLevelFilterID");

        String ENCleaned = CommonLogic.QueryStringCanBeDangerousContent("EntityName").Trim().ToUpperInvariant();

        // kludge for now, during conversion to properly entity/object setup:
        if (ENCleaned == "CATEGORY")
        {
            CategoryFilterID = CommonLogic.QueryStringUSInt("EntityFilterID");
            SectionFilterID = 0;
            ProductTypeFilterID = Localization.ParseNativeInt(ddTypes.SelectedValue);
            ManufacturerFilterID = 0;
            DistributorFilterID = 0;
            GenreFilterID = 0;
            VectorFilterID = 0;
            AffiliateFilterID = 0;
            CustomerLevelFilterID = 0;
            
            _entityId = CategoryFilterID;
            _entityName = ENCleaned;
        }
        if (ENCleaned == "SECTION")
        {
            CategoryFilterID = 0;
            SectionFilterID = CommonLogic.QueryStringUSInt("EntityFilterID");
            ProductTypeFilterID = Localization.ParseNativeInt(ddTypes.SelectedValue);
            ManufacturerFilterID = 0;
            DistributorFilterID = 0;
            GenreFilterID = 0;
            VectorFilterID = 0;
            AffiliateFilterID = 0;
            CustomerLevelFilterID = 0;

            _entityId = SectionFilterID;
            _entityName = ENCleaned;
        }
        if (ENCleaned == "MANUFACTURER")
        {
            CategoryFilterID = 0;
            SectionFilterID = 0;
            ProductTypeFilterID = Localization.ParseNativeInt(ddTypes.SelectedValue);
            ManufacturerFilterID = CommonLogic.QueryStringUSInt("EntityFilterID");
            DistributorFilterID = 0;
            GenreFilterID = 0;
            VectorFilterID = 0;
            AffiliateFilterID = 0;
            CustomerLevelFilterID = 0;

            _entityId = ManufacturerFilterID;
            _entityName = ENCleaned;
        }
        if (ENCleaned == "DISTRIBUTOR")
        {
            CategoryFilterID = 0;
            SectionFilterID = 0;
            ProductTypeFilterID = Localization.ParseNativeInt(ddTypes.SelectedValue);
            ManufacturerFilterID = 0;
            DistributorFilterID = CommonLogic.QueryStringUSInt("EntityFilterID");
            GenreFilterID = 0;
            VectorFilterID = 0;
            AffiliateFilterID = 0;
            CustomerLevelFilterID = 0;

            _entityId = DistributorFilterID;
            _entityName = ENCleaned;
        }
        if (ENCleaned == "GENRE")
        {
            CategoryFilterID = 0;
            SectionFilterID = 0;
            ProductTypeFilterID = Localization.ParseNativeInt(ddTypes.SelectedValue);
            ManufacturerFilterID = 0;
            DistributorFilterID = 0;
            GenreFilterID = CommonLogic.QueryStringUSInt("EntityFilterID");
            VectorFilterID = 0;
            AffiliateFilterID = 0;
            CustomerLevelFilterID = 0;

            _entityId = GenreFilterID;
            _entityName = ENCleaned;
        }
        if (ENCleaned == "VECTOR")
        {
            CategoryFilterID = 0;
            SectionFilterID = 0;
            ProductTypeFilterID = Localization.ParseNativeInt(ddTypes.SelectedValue);
            ManufacturerFilterID = 0;
            DistributorFilterID = 0;
            GenreFilterID = 0;
            VectorFilterID = CommonLogic.QueryStringUSInt("EntityFilterID");
            AffiliateFilterID = 0;
            CustomerLevelFilterID = 0;

            _entityId = VectorFilterID;
            _entityName = ENCleaned;
        }
        if (ENCleaned == "AFFILIATE")
        {
            CategoryFilterID = 0;
            SectionFilterID = 0;
            ProductTypeFilterID = Localization.ParseNativeInt(ddTypes.SelectedValue);
            ManufacturerFilterID = 0;
            DistributorFilterID = 0;
            GenreFilterID = 0;
            VectorFilterID = 0;
            AffiliateFilterID = CommonLogic.QueryStringUSInt("EntityFilterID");
            CustomerLevelFilterID = 0;

            _entityId = AffiliateFilterID;
            _entityName = ENCleaned;
        }
        if (ENCleaned == "CUSTOMERLEVEL")
        {
            CategoryFilterID = 0;
            SectionFilterID = 0;
            ProductTypeFilterID = Localization.ParseNativeInt(ddTypes.SelectedValue);
            ManufacturerFilterID = 0;
            DistributorFilterID = 0;
            GenreFilterID = 0;
            VectorFilterID = 0;
            AffiliateFilterID = 0;
            CustomerLevelFilterID = CommonLogic.QueryStringUSInt("EntityFilterID");

            _entityId = CustomerLevelFilterID;
            _entityName = ENCleaned;
        }
        // end kludge

        //search
        if (SearchFor.Length == 0)
        {
            SearchFor = txtSearch.Text;
            ThisCustomer.ThisCustomerSession.SetVal("ProductsSearch", txtSearch.Text);
        }

        //Node filter
        string Index = "";
        for (int i = 0; i < treeMain.Nodes.Count; i++)
        {
            if (treeMain.Nodes[i].Selected)
            {
                Index = treeMain.Nodes[i].Value;

                ThisCustomer.ThisCustomerSession.SetVal("ProductsTree", treeMain.Nodes[i].Value);

                break;
            }
        }


        //Type filter
        string typepName = ddTypes.SelectedValue;
        ThisCustomer.ThisCustomerSession.SetVal("ProductsType", typepName);


        ThisCustomer.ThisCustomerSession.SetVal("ProductsSort", ViewState["Sort"].ToString());
        ThisCustomer.ThisCustomerSession.SetVal("ProductsOrder", ViewState["SortOrder"].ToString());

        //remember page
        if (ThisCustomer.ThisCustomerSession.SessionNativeInt("ProductsPage") > 0)
        {
            gMain.PageIndex = ThisCustomer.ThisCustomerSession.SessionNativeInt("ProductsPage");
        }

        ProductCollection products = new ProductCollection();
        products.CategoryID = CategoryFilterID;
        products.SectionID = SectionFilterID;
        products.ManufacturerID = ManufacturerFilterID;
        products.DistributorID = DistributorFilterID;
        products.GenreID = GenreFilterID;
        products.VectorID = VectorFilterID;
        products.AffiliateID = AffiliateFilterID;
        products.ProductTypeID = ProductTypeFilterID;
        products.PublishedOnly = false;
        products.SearchDescriptionAndSummaryFields = false;
        products.SearchMatch = SearchFor;

        if (!Index.Equals("All"))
        {
            products.SearchIndex = Index;
        }

        products.SortBy = ViewState["Sort"].ToString();
        products.SortOrder = ViewState["SortOrder"].ToString();

        DataSet dsProducts = products.LoadFromDBEntity();

        //build grid
        buildGridData(dsProducts);

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

    protected void ddTypes_SelectedIndexChanged(object sender, EventArgs e)
    {
        ViewState["IsInsert"] = false;
        gMain.EditIndex = -1;
        resetError("", false);
        gMain.PageIndex = 0;

        resultFilter("");
    }

    protected void buildGridData(DataSet ds)
    {
        gMain.DataSource = ds;
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

        if (AppLogic.MaxProductsExceeded())
        {
            gMain.Columns[7].Visible = false;
        }

        ds.Dispose();
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

    protected void btnAdd_Click(object sender, EventArgs e)
    {
        Response.Redirect(AppLogic.AdminLinkUrl("entityEditProducts.aspx") + "?iden=0&entityName=" + eName + "&entityFilterID=" + eID);
    }

    protected void gMain_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            DataRowView myrow = (DataRowView)e.Row.DataItem;

            //set delete confirms
            ImageButton iD = (ImageButton)e.Row.FindControl("imgDelete");
            iD.Attributes.Add("onClick", "javascript: return confirm('" + String.Format(AppLogic.GetString("admin.common.SoftDeleteProductsQuestion", SkinID, LocaleSetting),myrow["ProductID"].ToString()));
            ImageButton iN = (ImageButton)e.Row.FindControl("imgNuke");
            iN.Attributes.Add("onClick", "javascript: return confirm('" + String.Format(AppLogic.GetString("admin.common.NukeProductQuestion", SkinID, LocaleSetting),myrow["ProductID"].ToString()));
            LinkButton lCc = (LinkButton)e.Row.FindControl("lnkClone");
            lCc.Attributes.Add("onClick", "javascript: return confirm('" + AppLogic.GetString("admin.entityProducts.CloneQuestion", SkinID, LocaleSetting) + "'");

            if (Localization.ParseBoolean(myrow["IsSystem"].ToString()))
            {
                e.Row.Cells[8].Text = "System Product"; // this type of product can only be deleted in the db!
                iD.Visible = false;
                e.Row.Cells[9].Text = "System Product"; // this type of product can only be deleted in the db!
                iN.Visible = false;
            }

            Literal ltName = (Literal)e.Row.FindControl("ltName");
            ltName.Text = ("<a href=\"" + AppLogic.AdminLinkUrl("entityEditProducts.aspx") + "?iden=" + myrow["ProductID"].ToString() + "&entityName=" + eName + "&entityFilterID=" + eID + "\">");
            ltName.Text += (XmlCommon.GetLocaleEntry(myrow["Name"].ToString(), LocaleSetting, true));
            ltName.Text += ("</a>");

            Literal ltImage = (Literal)e.Row.FindControl("ltImage");
            String Image1URL = AppLogic.LookupImage("Product", Localization.ParseNativeInt(myrow["ProductID"].ToString()), "icon", SiteID, LocaleSetting);
            ltImage.Text = "<img src=\"" + Image1URL + "\" width=\"25\" border=\"0\" align=\"absmiddle\">";

            //Variants
            ltName.Text += ("&nbsp;(<a href=\"" + AppLogic.AdminLinkUrl("entityProductVariantsOverview.aspx") + "?ProductID=" + myrow["ProductID"].ToString() + "&entityname=" + eName + "&EntityID=" + eID + "\">" + AppLogic.GetString("admin.common.Variants", SkinID, LocaleSetting) + "</a>)</div>");

            Literal ltInventory = (Literal)e.Row.FindControl("ltInventory");
            if (Localization.ParseBoolean(myrow["IsSystem"].ToString()))
            {
                ltInventory.Text = "System Product"; // this type of product can only be deleted in the db!
            }
            else
            {
                if (AppLogic.ProductTracksInventoryBySizeAndColor(Localization.ParseNativeInt(myrow["ProductID"].ToString())))
                {
                    ltInventory.Text = ("<a href=\"" + AppLogic.AdminLinkUrl("entityProductVariantsOverview.aspx") + "?ProductID=" + myrow["ProductID"].ToString() + "&entityname=" + eName + "&EntityID=" + eID + "\">" + AppLogic.GetString("admin.common.Inventory", SkinID, LocaleSetting) + "</a>\n");
                }
                else
                {
                    ltInventory.Text = (myrow["Inventory"].ToString());
                }
            }

            LinkButton lC = (LinkButton)e.Row.FindControl("lnkClone");
            if (Localization.ParseBoolean(myrow["IsSystem"].ToString()))
            {
                e.Row.Cells[7].Text = AppLogic.GetString("admin.entityProducts.SystemProduct", SkinID, LocaleSetting); // this type of product can only be deleted in the db!
                lC.Visible = false;
            }

            Literal ltRating = (Literal)e.Row.FindControl("ltRating");
            if (Localization.ParseBoolean(myrow["IsSystem"].ToString()))
            {
                ltRating.Text = (AppLogic.GetString("admin.entityProducts.SystemProduct", SkinID, LocaleSetting)); // this type of product can only be deleted in the db!
            }
            else
            {
                int NumRatings = DB.GetSqlN("select count(*) as N from rating   with (NOLOCK)  where productid=" + myrow["ProductID"].ToString());
                ltRating.Text = ("<a href=\"javascript:;\" onclick=\"window.open('" + AppLogic.AdminLinkUrl("entityProductRatings.aspx") + "?Productid=" + myrow["ProductID"].ToString() + "','" + AppLogic.GetString("admin.entityProducts.Rating", SkinID, LocaleSetting) + "','height=325, width=500, resizable=no, scrollbars=yes, toolbar=no, status=yes, location=no, directories=no, menubar=yes, alwaysRaised=yes');\">" + String.Format(AppLogic.GetString("Token", SkinID, LocaleSetting),NumRatings.ToString()) + "</a>\n");
            }
        }
    }

    protected void gMain_Sorting(object sender, GridViewSortEventArgs e)
    {
        gMain.EditIndex = -1;
        ViewState["Sort"] = e.SortExpression.ToString();
        ViewState["SortOrder"] = (ViewState["SortOrder"].ToString() == "ASC" ? "DESC" : "ASC");
        resultFilter("");
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
        else if (e.CommandName == "NukeItem")
        {
            gMain.EditIndex = -1;
            int iden = Localization.ParseNativeInt(e.CommandArgument.ToString());
            nukeRow(iden);
        }
        else if (e.CommandName == "CloneItem")
        {
            gMain.EditIndex = -1;
            int iden = Localization.ParseNativeInt(e.CommandArgument.ToString());
            cloneRow(iden);
        }

        resultFilter("");
    }

    protected void cloneRow(int iden)
    {
        if (iden != 0)
        {
            int NewProductID = 0;
            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("aspdnsf_CloneProduct " + iden.ToString(),dbconn))
                {
                    if (rs.Read())
                    {
                        NewProductID = DB.RSFieldInt(rs, "ProductID");
                    }
                }
            }
         
            if (NewProductID != 0)
            {
                resultFilter("");
                resetError(AppLogic.GetString("admin.entityProducts.ProductCloned", SkinID, LocaleSetting), false);
            }
        }
    }

    private const string ENTITY_FRAME_MENU = "entityMenu";

    private void ForceReloadMenuList()
    {
        if (!string.IsNullOrEmpty(_entityName) && 
            !(_entityId == 0))
        {
            StringBuilder attachReloadScript = new StringBuilder();
            attachReloadScript.Append("<script language=\"Javascript\">");
            attachReloadScript.AppendFormat(
                "window.onload = function(){{ if(parent.{0}){{parent.{0}.document.location.href='" + AppLogic.AdminLinkUrl("entityMenu.aspx") +"?entityName={1}';}} }};", 
                ENTITY_FRAME_MENU,
                _entityName
            );
            attachReloadScript.Append("</script>");

            Page.ClientScript.RegisterClientScriptBlock(
                GetType(), 
                Guid.NewGuid().ToString(), 
                attachReloadScript.ToString()
            );
        }
    }

    protected void deleteRow(int iden)
    {
        if (0 < DB.GetSqlN("select count(*) N from ShoppingCart  with (NOLOCK)  where ProductID=" + iden.ToString() + " and CartType=" + ((int)CartTypeEnum.RecurringCart).ToString()))
        {
            resetError(AppLogic.GetString("admin.common.DeleteNAInfoRecurring", SiteID, LocaleSetting), true);
            return;
        }

        DB.ExecuteSQL("delete from ShoppingCart where productid=" + iden.ToString());
        DB.ExecuteSQL("delete from kitcart where productid=" + iden.ToString());

        DB.ExecuteSQL("update Product set deleted=1 where ProductID=" + iden.ToString());

        DB.ExecuteSQL("delete from ProductAffiliate where productid=" + iden.ToString());
        DB.ExecuteSQL("delete from ProductCategory where productid=" + iden.ToString());
        DB.ExecuteSQL("delete from ProductCustomerLevel where productid=" + iden.ToString());
        DB.ExecuteSQL("delete from ProductDistributor where productid=" + iden.ToString());
        DB.ExecuteSQL("delete from ProductGenre where productid=" + iden.ToString());
        DB.ExecuteSQL("delete from ProductVector where productid=" + iden.ToString());
        DB.ExecuteSQL("delete from ProductLocaleSetting where productid=" + iden.ToString());
        DB.ExecuteSQL("delete from ProductManufacturer where productid=" + iden.ToString());
        DB.ExecuteSQL("delete from ProductSection where productid=" + iden.ToString());
        
        /******* end modification ****************/

        string treeT = treeMain.SelectedValue;
        string typeT = ddTypes.SelectedValue;
        loadTree();
        loadTypes();

        //take user back
        foreach (ListItem li in ddTypes.Items)
        {
            if (li.Value.ToUpperInvariant().Equals(typeT.ToUpperInvariant()))
            {
                ddTypes.ClearSelection();
                li.Selected = true;
            }
        }
        foreach (TreeNode node in treeMain.Nodes)
        {
            if (node.Value.Equals(treeT))
            {
                node.Selected = true;
            }
        }

        resultFilter("");

        /******* start ****************/
        ForceReloadMenuList();
        /******* end modification ****************/

        resetError(AppLogic.GetString("admin.entityProducts.ProductDeleted", SkinID, LocaleSetting), false);
    }
    protected void nukeRow(int iden)
    {
        if (0 < DB.GetSqlN("select count(*) N from ShoppingCart  with (NOLOCK)  where ProductID=" + iden.ToString() + " and CartType=" + ((int)CartTypeEnum.RecurringCart).ToString()))
        {
            resetError(AppLogic.GetString("admin.common.DeleteNAInfoRecurring", SiteID, LocaleSetting), true);
            return;
        }

        DB.ExecuteLongTimeSQL("aspdnsf_NukeProduct " + iden.ToString(), 120);

        string treeT = treeMain.SelectedValue;
        loadTree();
        foreach (TreeNode node in treeMain.Nodes)
        {
            if (node.Value.Equals(treeT))
            {
                node.Selected = true;
            }
        }

        resultFilter("");

        resetError(AppLogic.GetString("admin.entityProducts.ProductNuked", SkinID, LocaleSetting), false);
    }

    protected void gMain_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        resetError("", false);
        gMain.PageIndex = e.NewPageIndex;
        gMain.EditIndex = -1;
        ThisCustomer.ThisCustomerSession.SetVal("ProductsPage", e.NewPageIndex.ToString());
        resultFilter("");
    }
}
}
