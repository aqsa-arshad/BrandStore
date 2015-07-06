// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontControls;
using System.Data;
using AjaxControlToolkit;
using Telerik.Web.UI;
using System.Text;
using System.Data.SqlClient;

namespace AspDotNetStorefrontAdmin.Controls
{
    public partial class ProductsGrid : BaseUserControl<IEnumerable<GridProduct>>
    {
        private const string CURRENT_PAGE = "CurrentPage";
        private const string SORT_COLUMN = "SortColumn";
        private const string SORT_COLUMN_DIRECTION = "SortColumnDirection";
        private const string CURRENT_FILTER = "CurrentFilter";
        private const string CURRENT_FILTER_TYPE = "CurrentFilterType";
        private const int PAGE_SIZE = 15;

        private string EntityType = "category";

        public string CurrentFilter
        {
            get
            {
                object savedValue = ViewState[CURRENT_FILTER];
                if (null == savedValue) { return string.Empty; }

                return savedValue.ToString();
            }
            set
            {
                ViewState[CURRENT_FILTER] = value;
            }
        }

        public FilterType CurrentFilterType
        {
            get
            {
                object intValue = ViewState[CURRENT_FILTER_TYPE];
                if (null == intValue) { return FilterType.Search; }

                return (FilterType)((int)intValue);
            }
            set
            {
                ViewState[CURRENT_FILTER_TYPE] = (int)value;
            }
        }

        public int CurrentPage
        {
            get { return null == ViewState[CURRENT_PAGE] ? 1 : (int)ViewState[CURRENT_PAGE]; }
            set { ViewState[CURRENT_PAGE] = value; }
        }

        public string SortColumn
        {
            get { return null == ViewState[SORT_COLUMN] ? string.Empty : (string)ViewState[SORT_COLUMN]; }
            set { ViewState[SORT_COLUMN] = value; }
        }

        public GridSortOrder SortColumnDirection
        {
            get { return null == ViewState[SORT_COLUMN_DIRECTION] ? GridSortOrder.Ascending : (GridSortOrder)ViewState[SORT_COLUMN_DIRECTION]; }
            set { ViewState[SORT_COLUMN_DIRECTION] = value; }
        }

        private IEnumerable<GridProduct> SortRoutine(IEnumerable<GridProduct> source)
        {
            // check to see if we have sorting specified
            if (string.IsNullOrEmpty(SortColumn))
            {
                return source;
            }

            // delegate function for our sort routine
            // which will be determined by whatever column we have currently configured
            // the delegate will return the appropriate column to sort by
            // which will then be used by the Linq Orderby or OrderByDescending methods
            Func<GridProduct, object> selector = null;

            switch (SortColumn)
            {
                case "ProductID":
                    selector = (p) => p.ProductID;
                    break;

                case "LocaleName":
                    selector = (p) => p.LocaleName;
                    break;

                case "SKU":
                    selector = (p) => p.SKU;
                    break;

                case "Published":
                    selector = (p) => p.Published;
                    break;
            }

            if (this.SortColumnDirection.Equals(GridSortOrder.Ascending))
            {
                return source.OrderBy(selector);
            }
            else
            {
                return source.OrderByDescending(selector);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            EntityType = CommonLogic.QueryStringCanBeDangerousContent("entityname");

            if (!IsPostBack)
            {
                ViewState.Add("ExpandedItemIndex", "");

                SortColumn = "LocaleName";
                CurrentPage = 1;
                CurrentFilter = string.Empty;
                CurrentFilterType = FilterType.None;
            }

            AddRootAddButton();
            UpdateStatus(false, "", false);
        }

        private void UpdateStatus(Boolean enabled, string status, Boolean isError)
        {
            litUpdateStatus.Text = "";
            litUpdateStatus.Visible = enabled;
            if (enabled)
            {
                String msgtemplate = "<b style=\"color:"+CommonLogic.IIF(isError, "red", "green")+";\">{0}</b>";
                litUpdateStatus.Text = string.Format(msgtemplate, status);
            }
        }

        /// <summary>
        /// Method to catch bubbled event from the entity control when a new entity is added
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Product_Added(object sender, EventArgs e)
        {
            int pID = 0;

            if (CommonLogic.IsInteger(sender.ToString()))
            {
                pID = int.Parse(sender.ToString());
            }

            if (pID != 0)
            {
                GridProduct gp = new GridProduct(pID);

                (base.Datasource as List<GridProduct>).Add(gp);

                BindData(false);

                updatePanelSearch.Update();
            }
        }

        /// <summary>
        /// Method to catch bubbled event from the entity control when an entity is updated
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Product_Updated(object sender, EventArgs e)
        {
            int pID = 0;

            if (CommonLogic.IsInteger(sender.ToString()))
            {
                pID = int.Parse(sender.ToString());
            }

            if (pID != 0)
            {
                int itemToUpdate = (base.Datasource as List<GridProduct>).FindIndex(p => p.ProductID.Equals(pID));

                (base.Datasource as List<GridProduct>)[itemToUpdate] = new GridProduct(pID);

                BindData(false);

                updatePanelSearch.Update();
            }
        }


        protected void AddRootAddButton()
        {
            AlphaPaging ap = ctrlSearch.FindControl<AlphaPaging>("ctrlAlphaPaging");

            if (ap.FindControl<HyperLink>("lnkAddProduct") == null)
            {
                HyperLink hl = new HyperLink();
                hl.ID = "lnkAddProduct";
                hl.Text = "Add Product";
                hl.ImageUrl = "~/App_Themes/Admin_Default/images/add.png";
                hl.ToolTip = AppLogic.GetString("admin.productgrid.AddNewProduct", ThisCustomer.LocaleSetting);
                hl.Attributes["style"] = "cursor:pointer;";
                hl.Attributes["src"] = "#";
                hl.Attributes["onclick"] = "return ShowInsertForm();";

                ap.Controls.Add(hl);
            }
        }

        public override void DataBind()
        {
            BindData(false);
            base.DataBind();
        }

        protected void ctrlSearch_Filter(object sender, FilterEventArgs e)
        {
            CurrentFilter = e.Filter.Trim();
            CurrentFilterType = e.Type;
            CurrentPage = e.Page;

            AddRootAddButton();

            BindData(false);
        }

        protected T DataItemAs<T>(GridItem item) where T : class
        {
            return item.DataItem as T;
        }

        private PaginatedList<GridProduct> GetDatasource()
        {
            List<GridProduct> allProducts = base.Datasource as List<GridProduct>;

            PagedSearchTemplate<GridProduct> pagedSearch = new PagedSearchTemplate<GridProduct>();

            var filterString = this.CurrentFilter;
            if (CurrentFilterType.Equals(FilterType.Search))
            {
                Func<GridProduct, bool> searchFilter = (e) => e.LocaleName.ContainsIgnoreCase(filterString) || e.SKU.ContainsIgnoreCase(filterString) || e.ProductID == filterString.ToNativeInt();
                pagedSearch.Filters.Add(searchFilter);
            }
            else if (CurrentFilterType.Equals(FilterType.AlphaFilter))
            {
                Func<GridProduct, bool> alphaFilter = null;
                if (filterString == "[0-9]")
                {
                    alphaFilter = (e) => e.LocaleName.StartsWithNumbers();
                }
                else
                {
                    alphaFilter = (e) => e.LocaleName.StartsWithIgnoreCase(filterString);
                }
                pagedSearch.Filters.Add(alphaFilter);
            }

            // apply sorting if applicable
            pagedSearch.Sorter = SortRoutine;

            var results = pagedSearch.Search(allProducts, PAGE_SIZE, CurrentPage);
            return results;
        }

        protected override void OnInit(EventArgs e)
        {
            if (ThisCustomer == null)
            {
                ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;
            }

            //Build and register the RadWindow Script block
            string gridID = ctrlSearch.FindControl("grdProducts").ClientID;
            string ajxID = radAjaxMgr.ClientID;

            StringBuilder sb = new StringBuilder();

            #region RadWindow Javascript Functions
            sb.Append("<script type=\"text/javascript\">                                                                                        \r\n");
            /*Instantiates a new modale window with the Edit Product page in edit mode*/
            sb.Append("function ShowEditForm(id, rowIndex)                                                                                      \r\n");
            sb.Append("{                                                                                                                        \r\n");
            sb.Append("     var grid = $find(\"" + gridID + "\");                                                                               \r\n");
            sb.Append("                                                                                                                         \r\n");
            sb.Append("     var rowControl = grid.get_masterTableView().get_dataItems()[rowIndex].get_element();                                \r\n");
            sb.Append("     grid.get_masterTableView().selectItem(rowControl, true);                                                            \r\n");
            sb.Append("                                                                                                                         \r\n");
            sb.Append("     window.radopen(\"entityEditProducts.aspx?iden=\" + id + \"&entityName=CATEGORY\", \"rdwEditProduct\");              \r\n");
            sb.Append("     return false;                                                                                                       \r\n");
            sb.Append("}                                                                                                                        \r\n");
            /*Instantiates a new modal window with the Edit Product page in insert mode*/
            sb.Append("function ShowInsertForm()                                                                                                \r\n");
            sb.Append("{                                                                                                                        \r\n");
            sb.Append("     window.radopen(\"entityEditProducts.aspx?entityName=CATEGORY\", \"rdwEditProduct\");                                \r\n");
            sb.Append("     return false;                                                                                                       \r\n");
            sb.Append("}                                                                                                                        \r\n");
            /*Instantiates a new modal window with the Edit Variant page in edit mode*/
            sb.Append("function VariantShowEditForm(id, rowIndex, parentID)                                                                     \r\n");
            sb.Append("{                                                                                                                        \r\n");
            sb.Append("     window.radopen(\"entityEditProductVariant.aspx?VariantID=\" + id + \"&entityName=CATEGORY&ProductID=\" + parentID, \"rdwEditProduct\");        \r\n");
            sb.Append("     return false;                                                                                                       \r\n");
            sb.Append("}                                                                                                                        \r\n");
            /*Instantiates a new modal window with the Edit Variant page in insert mode*/
            sb.Append("function VariantShowInsertForm(parentID)                                                                                 \r\n");
            sb.Append("{                                                                                                                        \r\n");
            sb.Append("     window.radopen(\"entityEditProductVariant.aspx?entityName=CATEGORY&ProductID=\" + parentID, \"rdwEditProduct\");          \r\n");
            sb.Append("     return false;                                                                                                       \r\n");
            sb.Append("}                                                                                                                        \r\n");
            /*Accepts a callback from the modal window to update the grid*/
            sb.Append("function refreshGrid(arg)                                                                                                \r\n");
            sb.Append("{                                                                                                                        \r\n");
            sb.Append("     if(!arg)                                                                                                            \r\n");
            sb.Append("     {                                                                                                                   \r\n");
            sb.Append("             $find(\"" + ajxID + "\").ajaxRequest(arg);                                                                  \r\n");
            sb.Append("     }                                                                                                                   \r\n");
            sb.Append("     else                                                                                                                \r\n");
            sb.Append("     {                                                                                                                   \r\n");
            sb.Append("             $find(\"" + ajxID + "\").ajaxRequest(arg);                                                                  \r\n");
            sb.Append("     }                                                                                                                   \r\n");
            sb.Append("}                                                                                                                        \r\n");

            sb.Append("</script>                                                                                                                \r\n");
            #endregion

            ClientScriptManager cs = Page.ClientScript;

            cs.RegisterClientScriptBlock(this.Page.GetType(), Guid.NewGuid().ToString(), sb.ToString());

            base.OnInit(e);
        }

        public void rdwEditProduct_OnClientCommand(object sender, EventArgs e)
        {
            String test = string.Empty;
        }


        private void BindData(bool fromNeedDataSource)
        {
            var results = GetDatasource();

            ctrlSearch.AllCount = results.TotalCount;
            ctrlSearch.StartCount = results.StartIndex;
            ctrlSearch.EndCount = results.EndIndex;
            ctrlSearch.PageCount = results.TotalPages;
            ctrlSearch.CurrentPage = results.CurrentPage;

            RadGrid grdProducts = ctrlSearch.FindControl<RadGrid>("grdProducts");

            grdProducts.DataSource = results;

            if (!fromNeedDataSource)
            {
                grdProducts.DataBind();
            }
        }

        protected void grdProducts_ItemCommand(object source, GridCommandEventArgs e)
        {
            if (e.CommandName.EqualsIgnoreCase("PublishProduct"))
            {
                int pID = int.Parse(e.CommandArgument.ToString());
                GridProduct p = new GridProduct(pID);
                p.Publish();

                UpdatePublishedColumn(e.Item, true, true);

                int prodIndex = (base.Datasource as List<GridProduct>).FindIndex(prod => prod.ProductID.Equals(pID));

                (base.Datasource as List<GridProduct>)[prodIndex] = p;

                BindData(false);
            }
            else if (e.CommandName.EqualsIgnoreCase("UnPublishProduct"))
            {
                int pID = int.Parse(e.CommandArgument.ToString());
                GridProduct p = new GridProduct(pID);
                p.UnPublish();

                UpdatePublishedColumn(e.Item, false, true);

                int prodIndex = (base.Datasource as List<GridProduct>).FindIndex(prod => prod.ProductID.Equals(pID));

                (base.Datasource as List<GridProduct>)[prodIndex] = p;

                BindData(false);
            }
            else if (e.CommandName.EqualsIgnoreCase("PublishProductVariant"))
            {
                int pvID = int.Parse(e.CommandArgument.ToString());
                GridProductVariant pv = new GridProductVariant(pvID);
                pv.Publish();

                UpdatePublishedColumn(e.Item, true, false);
            }
            else if (e.CommandName.EqualsIgnoreCase("UnPublishProductVariant"))
            {
                int pvID = int.Parse(e.CommandArgument.ToString());
                GridProductVariant pv = new GridProductVariant(pvID);
                pv.UnPublish();

                UpdatePublishedColumn(e.Item, false, false);
            }
            else if (e.CommandName.EqualsIgnoreCase("DeleteProduct"))
            {
                int pID = int.Parse(e.CommandArgument.ToString());
                GridProduct p = new GridProduct(pID);
                p.Delete();

                UpdateDeletedColumn(e.Item, true, true);

                int prodIndex = (base.Datasource as List<GridProduct>).FindIndex(prod => prod.ProductID.Equals(pID));

                (base.Datasource as List<GridProduct>)[prodIndex] = p;

                BindData(false);
            }
            else if (e.CommandName.EqualsIgnoreCase("UnDeleteProduct"))
            {
                int pID = int.Parse(e.CommandArgument.ToString());
                GridProduct p = new GridProduct(pID);
                p.UnDelete();

                UpdateDeletedColumn(e.Item, false, true);

                int prodIndex = (base.Datasource as List<GridProduct>).FindIndex(prod => prod.ProductID.Equals(pID));

                (base.Datasource as List<GridProduct>)[prodIndex] = p;

                BindData(false);
            }
            else if (e.CommandName.EqualsIgnoreCase("DeleteProductVariant"))
            {
                int pvID = int.Parse(e.CommandArgument.ToString());
                GridProductVariant pv = new GridProductVariant(pvID);
                pv.Delete();

                UpdateDeletedColumn(e.Item, true, false);
            }
            else if (e.CommandName.EqualsIgnoreCase("UnDeleteProductVariant"))
            {
                int pvID = int.Parse(e.CommandArgument.ToString());
                GridProductVariant pv = new GridProductVariant(pvID);
                pv.UnDelete();

                UpdateDeletedColumn(e.Item, false, false);
            }
            else if (e.CommandName.EqualsIgnoreCase("MakeDefaultProductVariant"))
            {
                int pvID = int.Parse(e.CommandArgument.ToString());
                GridProductVariant pv = new GridProductVariant(pvID);
                pv.MakeDefault();

                ClearOtherDefaults(e.Item);
                UpdateDefaultColumn(e.Item, true);
            }
            else if (e.CommandName.EqualsIgnoreCase("ExpandCollapse"))
            {
                if (e.Item.Expanded)
                {
                    //Item is already expanding, which means we are collapsing it now
                    ViewState["ExpandedItemIndex"] = "-1";
                }
                else
                {
                    ViewState["ExpandedItemIndex"] = e.Item.ItemIndexHierarchical;
                }

                AddRootAddButton();
            }
            if (e.CommandName.EqualsIgnoreCase("CloneProduct"))
            {
                int pID = int.Parse(e.CommandArgument.ToString());
                if (CloneProduct(pID))
                {
                    UpdateStatus(true, "Product " + pID.ToString() + " has been cloned and the new product has been set to unpublished.", false);
                }
                else
                {
                    UpdateStatus(true, "Product " + pID.ToString() + " could not be cloned.", true);
                }
                base.Datasource = GridProduct.GetProducts(true);
                BindData(false);
            }
            if (e.CommandName.EqualsIgnoreCase("NukeProduct"))
            {
                int pID = int.Parse(e.CommandArgument.ToString());
                if (NukeProduct(pID))
                    UpdateStatus(true, "Product " +  pID.ToString() + " has been nuked.", false);
                else
                    UpdateStatus(true, "Product " + pID.ToString() + " could not be nuked because it is in active shopping carts.", true);
                base.Datasource = GridProduct.GetProducts(true);
                BindData(false);
            }
            updatePanelStatus.Update();
        }

        private void ClearOtherDefaults(GridItem gi)
        {
            GridTableView gtv = gi.OwnerTableView;

            foreach (GridItem tableviewitem in gtv.Items)
            {
                LinkButton lb = tableviewitem.FindControl<LinkButton>("cmdDefaultProductVariant");

                if (lb != null)
                {
                    lb.Text = AppLogic.GetString("admin.productvariant.makedefault", ThisCustomer.LocaleSetting);
                    lb.Enabled = true;
                }
            }

        }

        protected void grdProducts_SortCommand(object source, GridSortCommandEventArgs e)
        {
            // The RadGrid has a bug in it's sorting display on the header
            // we'll just do the sorting manually

            // if we are in the master products grid
            if (e.Item.OwnerTableView.Name.EqualsIgnoreCase("Master"))
            {
                e.Canceled = true;

                this.SortColumn = e.SortExpression;

                switch (this.SortColumnDirection)
                {
                    case GridSortOrder.Ascending:
                        this.SortColumnDirection = GridSortOrder.Descending;
                        break;

                    case GridSortOrder.None:
                    case GridSortOrder.Descending:
                        this.SortColumnDirection = GridSortOrder.Ascending;
                        break;
                }

                BindData(false);
            }
            else // we're in the variant detail tables
            {

            }
        }

        protected void grdProducts_NeedDataSource(object source, GridNeedDataSourceEventArgs e)
        {
            if (!e.IsFromDetailTable)
            {
                BindData(true);
            }
        }

        protected void grdProducts_DetailTableDataBind(object source, GridDetailTableDataBindEventArgs e)
        {
            RadGrid grdProducts = ctrlSearch.FindControl<RadGrid>("grdProducts");

            GridDataItem dataItem = (GridDataItem)e.DetailTableView.ParentItem;

            int pID = int.Parse(dataItem.GetDataKeyValue("ProductID").ToString());

            GridProduct p = new GridProduct(pID);

            if (p.Variants.Count() == 0)
            {
                p.LoadVariants(true);
            }

            e.DetailTableView.DataSource = p.Variants;
        }

        protected void grdProducts_ItemCreated(object sender, GridItemEventArgs e)
        {
            RadGrid grdProducts = ctrlSearch.FindControl<RadGrid>("grdProducts");
            GridItem gi = e.Item;

            if (e.Item is GridDataItem)
            {
                if (e.Item.OwnerTableView.Name.EqualsIgnoreCase("Master"))
                {
                    //Parent product grid
                    string productID = e.Item.OwnerTableView.DataKeyValues[e.Item.ItemIndex]["ProductID"].ToString();

                    HyperLink lnkEdit = (HyperLink)e.Item.FindControl("lnkEdit");
                    HyperLink lnkAddVariant = (HyperLink)e.Item.FindControl("lnkAddVariant");
                    HyperLink lnkName = (HyperLink)e.Item.FindControl("lnkName");

                    if (lnkEdit != null)
                    {
                        lnkEdit.ImageUrl = "~/App_Themes/Admin_Default/images/edit.png";
                        lnkEdit.Attributes["src"] = "#";
                        lnkEdit.Attributes["style"] = "cursor:pointer;";
                        lnkEdit.Attributes["onclick"] = String.Format("return ShowEditForm('{0}','{1}');", productID, e.Item.ItemIndex);
                        lnkEdit.ToolTip = AppLogic.GetString("admin.productgrid.EditProduct", ThisCustomer.LocaleSetting);

                        lnkAddVariant.ImageUrl = "~/App_Themes/Admin_Default/images/add.png";
                        lnkAddVariant.ToolTip = AppLogic.GetString("admin.productgrid.AddNewVariant", ThisCustomer.LocaleSetting);
                        lnkAddVariant.Attributes["onclick"] = String.Format("return VariantShowInsertForm('{0}');", productID);
                        lnkAddVariant.Attributes["style"] = "cursor:pointer;";
                        lnkAddVariant.Attributes["src"] = "#";

                        lnkName.Attributes["src"] = "#";
                        lnkName.Attributes["onClick"] = String.Format("return ShowEditForm('{0}','{1}');", productID, e.Item.ItemIndex);
                        lnkName.Attributes["style"] = "cursor:pointer;";

                    }
                }
                else if (e.Item.OwnerTableView.Name.EqualsIgnoreCase("ProductVariants"))
                {
                    //We are working with variants
                    GridProductVariant item = (GridProductVariant)e.Item.DataItem;

                    if (item != null)
                    {
                        string productID = item.ProductID.ToString();
                        string variantID = item.VariantID.ToString();

                        HyperLink lnkEditVariant = (HyperLink)e.Item.FindControl("lnkEditVariant");

                        if (lnkEditVariant != null)
                        {
                            lnkEditVariant.ImageUrl = "~/App_Themes/Admin_Default/images/edit.png";
                            lnkEditVariant.Attributes["src"] = "#";
                            lnkEditVariant.Attributes["onclick"] = String.Format("return VariantShowEditForm('{0}','{1}','{2}');", variantID, e.Item.ItemIndex, productID);
                            lnkEditVariant.Attributes["style"] = "cursor:pointer;";
                            lnkEditVariant.ToolTip = AppLogic.GetString("admin.productgrid.EditVariant", ThisCustomer.LocaleSetting);
                        }
                    }
                }
            }


            if (gi.ItemType == GridItemType.Item || gi.ItemType == GridItemType.AlternatingItem)
            {
                // we have a product variant grid row
                if (gi.OwnerTableView.Name.EqualsIgnoreCase("ProductVariants"))
                {
                    GridProductVariant pv = gi.DataItem as GridProductVariant;

                    if (pv != null)
                    {
                        UpdatePublishedColumn(gi, pv.Published, false);
                        UpdateDefaultColumn(gi, pv.IsDefault);
                        UpdateDeletedColumn(gi, pv.Deleted, false);

                        LinkButton lnkName = (LinkButton)gi.FindControl("lnkName");

                        if (lnkName != null)
                        {
                            lnkName.Attributes["onclick"] = String.Format("return VariantShowEditForm('{0}','{1}',{2});", pv.VariantID.ToString(), gi.ItemIndex.ToString(), pv.ProductID.ToString());
                            lnkName.Attributes["src"] = "#";
                            lnkName.Attributes["style"] = "cursor:pointer;";

                        }
                    }
                }
                else  // we have a product grid row
                {
                    GridProduct p = gi.DataItem as GridProduct;

                    if (p != null)
                    {
                        UpdatePublishedColumn(gi, p.Published, true);
                        UpdateDeletedColumn(gi, p.Deleted, true);

                        HyperLink lnkName = (HyperLink)gi.FindControl("lnkName");

                        if (lnkName != null)
                        {
                            lnkName.Attributes["onclick"] = String.Format("return ShowEditForm('{0}','{1}');", p.ProductID.ToString(), gi.ItemIndex.ToString());
                            lnkName.Attributes["src"] = "#";
                            lnkName.Attributes["style"] = "cursor:pointer;";

                        }
                    }
                }
            }
        }


        protected void radAjaxMgr_OnAjaxRequest(object sender, AjaxRequestEventArgs e)
        {
            int editedID = 0;
            string editedEntity = string.Empty;

            try
            {
                String[] returnArgs = e.Argument.Split(',');
                editedID = Convert.ToInt32(returnArgs[1]);

                editedEntity = returnArgs[0];
            }
            catch
            {
                //Nothing to do
                return;
            }

            if (editedID != 0)
            {
                if (editedEntity.EqualsIgnoreCase("product"))
                {
                    GridProduct prod = new GridProduct(editedID);

                    try
                    {
                        int prodIndex = (base.Datasource as List<GridProduct>).FindIndex(p => p.ProductID.Equals(editedID));

                        (base.Datasource as List<GridProduct>)[prodIndex] = prod;
                    }
                    catch
                    {
                        //New product... Add it to the list
                        (base.Datasource as List<GridProduct>).Add(prod);
                    }

                    BindData(false);
                }
                else
                {
                    //working with variants
                    BindData(false);

                    string expandedIndex = ViewState["ExpandedItemIndex"].ToString();

                    RadGrid grdProducts = ctrlSearch.FindControl<RadGrid>("grdProducts");

                    foreach (GridDataItem gi in grdProducts.Items)
                    {
                        if (gi.ItemIndexHierarchical == expandedIndex)
                        {
                            gi.Expanded = true;
                        }
                    }
                }
            }

        }


        protected void grdProducts_PreRender(object sender, EventArgs e)
        {

        }


        private void HideModalPanelByDefault(Panel pnl)
        {
            // we can't set the style declaratively
            // and we need the container to be a panel
            // so that we can assign the DefaultButton property

            // hide the div by default so that upon first load there won't be a sudden
            // flicker by the hiding of the div on browser page load
            pnl.Style["display"] = "none";
        }


        private void UpdateDefaultColumn(GridItem gi, bool Default)
        {
            LinkButton lb = gi.FindControl<LinkButton>("cmdDefaultProductVariant");

            if (lb != null)
            {
                if (Default)
                {
                    lb.Text = AppLogic.GetString("admin.common.Yes", ThisCustomer.LocaleSetting);
                    lb.Enabled = false;
                }
                else
                {
                    lb.Text = AppLogic.GetString("admin.productvariant.makedefault", ThisCustomer.LocaleSetting);
                    lb.Enabled = true;
                }
            }
        }

        private void UpdateDeletedColumn(GridItem gi, bool Deleted, bool IsProduct)
        {
            if (IsProduct)
            {
                ImageButton imgbtn = gi.FindControl<ImageButton>("imgDeleteProduct");

                if (Deleted)
                {
                    imgbtn.ImageUrl = "~/App_Themes/Admin_Default/images/undelete.png";
                    imgbtn.ToolTip = "UnDelete Product";
                    imgbtn.CommandName = "UnDeleteProduct";
                    imgbtn.OnClientClick = "return confirm('Are you sure you want to undelete this product?');";
                }
                else
                {
                    imgbtn.ImageUrl = "~/App_Themes/Admin_Default/images/delete.png";
                    imgbtn.ToolTip = "Delete Product";
                    imgbtn.CommandName = "DeleteProduct";
                    imgbtn.OnClientClick = "return confirm('Are you sure you want to delete this product?');";
                }
            }
            else
            {
                ImageButton imgbtn = gi.FindControl<ImageButton>("imgDeleteProductVariant");

                if (Deleted)
                {
                    imgbtn.ImageUrl = "~/App_Themes/Admin_Default/images/undelete.png";
                    imgbtn.ToolTip = "UnDelete Product Variant";
                    imgbtn.CommandName = "UnDeleteProductVariant";
                    imgbtn.OnClientClick = "return confirm('Are you sure you want to undelete this product variant?');";
                }
                else
                {
                    imgbtn.ImageUrl = "~/App_Themes/Admin_Default/images/delete.png";
                    imgbtn.ToolTip = "Delete Product Variant";
                    imgbtn.CommandName = "DeleteProductVariant";
                    imgbtn.OnClientClick = "return confirm('Are you sure you want to delete this product variant?');";
                }
            }
        }

        /// <summary>
        /// Determines the appropriate action and text for the published column of the entity RadGrid
        /// </summary>
        /// <param name="gi">The GridItem of the current row in the RadGrid</param>
        /// <param name="Published">A boolean value indicating the published status of the entity in the current row</param>
        /// <param name="IsProduct">A boolean value indicating whether we're working with a product or a product variant</param>
        private void UpdatePublishedColumn(GridItem gi, bool Published, bool IsProduct)
        {
            // it's a product
            if (IsProduct)
            {
                LinkButton lb = gi.FindControl<LinkButton>("cmdPublishProduct");

                if (lb != null)
                {
                    if (Published)
                    {
                        lb.Text = AppLogic.GetString("admin.productgrid.unpublish", ThisCustomer.LocaleSetting);
                        lb.CommandName = "UnPublishProduct";
                    }
                    else
                    {
                        lb.Text = AppLogic.GetString("admin.productgrid.publish", ThisCustomer.LocaleSetting);
                        lb.CommandName = "PublishProduct";
                    }
                }
            }
            else // it's a variant
            {
                LinkButton lb = gi.FindControl<LinkButton>("cmdPublishProductVariant");

                if (lb != null)
                {
                    if (Published)
                    {
                        lb.Text = AppLogic.GetString("admin.productgrid.unpublish", ThisCustomer.LocaleSetting);
                        lb.CommandName = "UnPublishProductVariant";
                    }
                    else
                    {
                        lb.Text = AppLogic.GetString("admin.productgrid.publish", ThisCustomer.LocaleSetting);
                        lb.CommandName = "PublishProductVariant";
                    }
                }
            }
        }

        private Boolean NukeProduct(int productID)
        {
            if (0 < DB.GetSqlN("select count(*) N from ShoppingCart  with (NOLOCK)  where ProductID=" + productID.ToString() + " and CartType=" + ((int)CartTypeEnum.RecurringCart).ToString()))
            {
                return false;
            }
            DB.ExecuteLongTimeSQL("aspdnsf_NukeProduct " + productID.ToString(), 120);
            return true;
        }

        private static Boolean CloneProduct(int productID)
        {
            if (productID != 0)
            {
                using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
                {
                    dbconn.Open();
                    using (IDataReader rs = DB.GetRS("aspdnsf_CloneProduct " + productID.ToString(), dbconn))
                    {
                        if (rs.Read())
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
