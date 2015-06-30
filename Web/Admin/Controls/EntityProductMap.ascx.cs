// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
#region Using Directives

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontControls;
using AjaxControlToolkit;
using Telerik.Web.UI;
using System.Text;
#endregion

namespace AspDotNetStorefrontAdmin.Controls
{
    public partial class EntityProductMap : BaseUserControl<IList<ProductEntityMapInfo>>
    {
        #region Fields

        private const String CURRENT_PAGE = "CurrentPage";
        private const String SORT_COLUMN = "SortColumn";
        private const String SORT_COLUMN_DIRECTION = "SortColumnDirection";
        private const String CURRENT_FILTER = "CurrentFilter";
        private const String CURRENT_FILTER_TYPE = "CurrentFilterType";
        private const Int32 PAGE_SIZE = 15;

        #endregion

        #region Properties

        public String CurrentFilter
        {
            get
            {
                Object savedValue = ViewState[CURRENT_FILTER];
                if (null == savedValue) { return String.Empty; }

                return savedValue.ToString();
            }
            set
            {
                ViewState[CURRENT_FILTER] = value;
            }
        }

        public Int32 CurrentPage
        {
            get { return null == ViewState[CURRENT_PAGE] ? 1 : (Int32)ViewState[CURRENT_PAGE]; }
            set { ViewState[CURRENT_PAGE] = value; }
        }

        public FilterType CurrentFilterType
        {
            get
            {
                Object intValue = ViewState[CURRENT_FILTER_TYPE];
                if (null == intValue) { return FilterType.Search; }

                return (FilterType)((Int32)intValue);
            }
            set
            {
                ViewState[CURRENT_FILTER_TYPE] = (Int32)value;
            }
        }

        public String EntityType
        {
            get
            {
                Object savedValue = ViewState["EntityType"];
                if (null == savedValue) { return String.Empty; }

                return savedValue.ToString();
            }
            set
            {
                ViewState["EntityType"] = value;
            }
        }

        public Int32 EntityId
        {
            get { return null == ViewState["EntityId"] ? 1 : (Int32)ViewState["EntityId"]; }
            set { ViewState["EntityId"] = value; }
        }

        /// <summary>
        /// Telerik compatible extension function for each individual bound item to get the actual type bound
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        protected T DataItemAs<T>(GridItem item) where T : class
        {
            return item.DataItem as T;
        }

        #endregion

        #region Overrides

        protected override void OnInit(EventArgs e)
        {

            //Build and register the RadWindow Script block
            string gridID = ctrlSearch.FindControl("grdMap").ClientID;

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
            sb.Append("     GetRadWindow().BrowserWindow.radopen(\"entityEditProducts.aspx?iden=\" + id + \"&entityName=CATEGORY\", \"rdwEditProduct\");              \r\n");
            sb.Append("     return false;                                                                                                       \r\n");
            sb.Append("}                                                                                                                        \r\n");

            sb.Append("</script>                                                                                                                \r\n");
            #endregion

            ClientScriptManager cs = Page.ClientScript;
            cs.RegisterClientScriptBlock(this.Page.GetType(), Guid.NewGuid().ToString(), sb.ToString());


            ctrlSearch.PageSizes = new List<Int32>(new Int32[] { 10, 15, 20, 25, 30, 40, 50 });
            if (!this.IsPostBack)
                ctrlSearch.CurrentPageSize = 10;



            base.OnInit(e);
        }

        public override bool UpdateChanges()
        {
            SaveSelectedMapping();
            return base.UpdateChanges();
        }
        #endregion

        #region Event Handlers

        protected void Page_Load(Object sender, EventArgs e)
        {
            if (IsPostBack)
                return;

            switch (this.EntityType.ToLower())
            {
                case "category":
                    break;

                case "section":
                    break;

                case "manufacturer":
                    ctrlSearch.FindControl<Label>("lblHelp").Text = "Updating a mapped product will remove it from any previously mapped manufacturers.";
                    break;

                case "distributor":
                    ctrlSearch.FindControl<Label>("lblHelp").Text = "Updating a mapped product will remove it from any previously mapped distributors.";
                    break;
            }
        }

        protected void chkShowSelectedOnly_CheckedChanged(Object sender, EventArgs e)
        {
            BindData();
        }

        protected void ctrlSearch_ContentCreated(Object sender, FilterEventArgs e)
        {
            BindData();
        }

        protected void ctrlSearch_Filter(Object sender, FilterEventArgs e)
        {
            CurrentFilter = e.Filter;
            CurrentFilterType = e.Type;
            CurrentPage = e.Page;

            BindData();
        }

        protected void grdMap_ItemCommand(Object source, GridCommandEventArgs e)
        {
            var txtProductId = e.Item.FindControl<HiddenField>("txtProductId");

            switch (e.CommandName.ToLower())
            {
                case "savemapping":
                    SaveSelectedMapping();
                    break;

                case "cloneproducts":
                    if (txtProductId == null)
                        return;

                    CloneProduct(txtProductId.Value.ToNativeInt());
                    break;

                case "deleteproducts":
                    if (txtProductId == null)
                        return;

                    DeleteProduct(txtProductId.Value.ToNativeInt());
                    break;

                case "undeleteproducts":
                    if (txtProductId == null)
                        return;

                    UndeleteProduct(txtProductId.Value.ToNativeInt());
                    break;

                case "nukeproducts":
                    if (txtProductId == null)
                        return;

                    NukeProduct(txtProductId.Value.ToNativeInt());
                    break;
            }

            BindData();
        }

        private void SaveSelectedMapping()
        {
            var grdMap = ctrlSearch.FindControl<RadGrid>("grdMap");
            var editItems = grdMap.Items.Cast<GridDataItem>().Where(item => item.ItemType == GridItemType.Item || item.ItemType == GridItemType.AlternatingItem);
            foreach (var item in editItems)
            {
                var txtProductId = item.FindControl<HiddenField>("txtProductId");
                Int32 productId = txtProductId.Value.ToNativeInt();

                ProductEntityMapInfo productMapInfo = item.DataItem as ProductEntityMapInfo;
                if (productMapInfo != null)
                {
                    var chkIsMapped = item.FindControl<CheckBox>("chkIsMapped");
                    productMapInfo.IsMapped = chkIsMapped.Checked;
                    SaveProductEntityInfo(productMapInfo);
                }
            }
        }

        protected void grdMap_SortCommand(Object source, GridSortCommandEventArgs e)
        {
        }

        protected void grdMap_ItemCreated(Object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType != GridItemType.Item && e.Item.ItemType != GridItemType.AlternatingItem)
                return;

            ProductEntityMapInfo productMapInfo = e.Item.DataItem as ProductEntityMapInfo;
            if (productMapInfo == null)
                return;

            var lnkDelete = e.Item.FindControl<LinkButton>("lnkDelete");
            lnkDelete.Visible = !productMapInfo.IsDeleted;

            var lnkUndelete = e.Item.FindControl<LinkButton>("lnkUndelete");
            lnkUndelete.Visible = productMapInfo.IsDeleted;

            string productID = e.Item.OwnerTableView.DataKeyValues[e.Item.ItemIndex]["ProductID"].ToString();

            LinkButton lnkName = (LinkButton)e.Item.FindControl("lnkName");
            if (lnkName != null)
            {
                lnkName.Attributes["onclick"] = String.Format("return ShowEditForm('{0}','{1}');", productID, e.Item.ItemIndex.ToString());
                lnkName.Attributes["src"] = "#";
                lnkName.Attributes["style"] = "cursor:pointer;";
            }
        }


        #endregion

        #region Private Methods

        private void BindData()
        {
            if (this.EntityId <= 0)
                return;

            String alphaFilter = CurrentFilterType.Equals(FilterType.AlphaFilter) ? CurrentFilter : String.Empty;
            String searchFilter = CurrentFilterType.Equals(FilterType.Search) ? CurrentFilter : String.Empty;

            PagingInfo pagingInfo;
            IList<ProductEntityMapInfo> productEntityMapInfos = LoadProductEntityInfos(ctrlSearch.CurrentPageSize, CurrentPage, alphaFilter, searchFilter, chkShowSelectedOnly.Checked, out pagingInfo);

            var grdMap = ctrlSearch.FindControl<RadGrid>("grdMap");
            grdMap.DataSource = productEntityMapInfos;
            grdMap.DataBind();

            ctrlSearch.AllCount = pagingInfo.TotalCount;
            ctrlSearch.StartCount = pagingInfo.StartIndex;
            ctrlSearch.EndCount = pagingInfo.EndIndex;
            ctrlSearch.PageCount = pagingInfo.TotalPages;
            ctrlSearch.CurrentPage = pagingInfo.CurrentPage;

            this.Datasource = productEntityMapInfos;
        }

        private IList<ProductEntityMapInfo> LoadProductEntityInfos(Int32 pageSize, Int32 currentPage, String alphaFilter, String searchFilter, Boolean showSelectedOnly, out PagingInfo pagingInfo)
        {
            pagingInfo = new PagingInfo();
            pagingInfo.PageSize = pageSize;
            pagingInfo.CurrentPage = currentPage;

            String entityMapTableName = String.Empty;
            String entityIdName = String.Empty;
            GetEntityTableInfo(this.EntityType, out entityMapTableName, out entityIdName);

            IList<ProductEntityMapInfo> productEntityMapInfos = new List<ProductEntityMapInfo>();
            using (SqlConnection connection = new SqlConnection(DB.GetDBConn()))
            {
                connection.Open();

                String sqlCount = "select count(*) as N from (select ov.Name, (select count(*) from {0} where ProductId = ov.Id and {1} = {2}) as IsMapped from ObjectView ov where ov.EntityType = 'Product') ov where 0=0".FormatWith(entityMapTableName, entityIdName, this.EntityId);

                if (!String.IsNullOrEmpty(alphaFilter))
                    sqlCount += " and (ov.[Name] like {0} + '%')".FormatWith(DB.SQuote(alphaFilter));

                if (!String.IsNullOrEmpty(searchFilter))
                    sqlCount += " and (ov.[Name] like '%' + {0} + '%')".FormatWith(DB.SQuote(searchFilter));

                if (showSelectedOnly)
                    sqlCount += " and IsMapped = 1";

                pagingInfo.TotalCount = DB.GetSqlN(sqlCount);
                if (pagingInfo.TotalCount > 0 && pagingInfo.TotalCount < pagingInfo.PageSize)
                    pagingInfo.PageSize = pagingInfo.TotalCount;

                if (pagingInfo.PageSize != 0)
                {
                    double totalPages = (double)pagingInfo.TotalCount / pagingInfo.PageSize;
                    pagingInfo.TotalPages = (int)Math.Ceiling((double)pagingInfo.TotalCount / pagingInfo.PageSize);
                }
                else
                    pagingInfo.TotalPages = 0;

                if (pagingInfo.CurrentPage < 1)
                    pagingInfo.CurrentPage = 1;

                if (pagingInfo.CurrentPage > pagingInfo.TotalPages)
                    pagingInfo.CurrentPage = pagingInfo.TotalPages;

                pagingInfo.StartIndex = (pagingInfo.PageSize * pagingInfo.CurrentPage) - pagingInfo.PageSize + 1;
                pagingInfo.EndIndex = pagingInfo.StartIndex + pagingInfo.PageSize - 1;

                String sqlRecords = "select RowNumber, ProductId, Name, IsMapped from (select ROW_NUMBER() over (partition by EntityType order by Name) as RowNumber, ov.Id as ProductId, ov.Name, (select count(*) from {0} where ProductId = ov.Id and {1} = @EntityId) as IsMapped from ObjectView ov where ov.EntityType = 'Product' ".FormatWith(entityMapTableName, entityIdName, showSelectedOnly ? 1 : 0);

                if (showSelectedOnly)
                    sqlRecords += " and (select count(*) from {0} where ProductId = ov.Id and {1} = @EntityId) = 1".FormatWith(entityMapTableName, entityIdName); ;

                sqlRecords += " and (@AlphaFilter IS NULL OR (ov.[Name] like @AlphaFilter + '%')) and (@SearchFilter IS NULL OR (ov.[Name] like '%' + @SearchFilter + '%'))) ov where 0 = 0 ".FormatWith(entityMapTableName, entityIdName, showSelectedOnly ? 1 : 0);

                sqlRecords += " and (RowNumber BETWEEN @StartIndex AND @EndIndex)";

                sqlRecords += " order by Name";

                IDataReader reader = DB.GetRS(sqlRecords,
                    new SqlParameter[]
                    {
                        new SqlParameter("@EntityId", SqlDbType.Int) { Value = this.EntityId },
                        new SqlParameter("@StartIndex", SqlDbType.Int) { Value = pagingInfo.StartIndex },
                        new SqlParameter("@EndIndex", SqlDbType.Int) { Value = pagingInfo.EndIndex },
                        new SqlParameter("@AlphaFilter", SqlDbType.NVarChar, 30) { Value = alphaFilter },
                        new SqlParameter("@SearchFilter", SqlDbType.NVarChar, 30) { Value = searchFilter }
                    }, connection);

                while (reader.Read())
                {
                    Int32 productId = DB.RSFieldInt(reader, "ProductId");
                    Int32 deletedInt = DB.GetSqlN("select top 1 cast(Deleted as int) as N from Product where ProductId = {0}".FormatWith(productId));

                    productEntityMapInfos.Add(new ProductEntityMapInfo
                    {
                        EntityId = this.EntityId,
                        EntityType = this.EntityType,
                        ProductId = productId,
                        Name = DB.RSField(reader, "Name"),
                        IsMapped = DB.RSFieldBool(reader, "IsMapped"),
                        IsDeleted = deletedInt == 1
                    });
                }
            }

            return productEntityMapInfos;
        }

        private void SaveProductEntityInfo(ProductEntityMapInfo productMapInfo)
        {
            String entityMapTableName = String.Empty;
            String entityIdName = String.Empty;
            GetEntityTableInfo(this.EntityType, out entityMapTableName, out entityIdName);

            SqlParameter[] spa = { new SqlParameter("@ProductID", productMapInfo.ProductId), new SqlParameter("@EntityID", productMapInfo.EntityId) };

            if (productMapInfo.IsMapped)
            {
                if (this.EntityType.ToLower() == "manufacturer")
                {
                    SqlParameter[] manufacturerSpa = { new SqlParameter("@ProductID", productMapInfo.ProductId), new SqlParameter("@EntityID", productMapInfo.EntityId) };
                    SqlCommand manufacturerCmd = new SqlCommand();
                    manufacturerCmd.Parameters.AddRange(manufacturerSpa);
                    manufacturerCmd.CommandText = "delete from ProductManufacturer where ProductId = @ProductID AND ManufacturerID != @EntityID";
                    DB.ExecuteSQL(manufacturerCmd);
                }
                else if (this.EntityType.ToLower() == "distributor")
                {
                    SqlParameter[] distributorSpa = { new SqlParameter("@ProductID", productMapInfo.ProductId), new SqlParameter("@EntityID", productMapInfo.EntityId) };
                    SqlCommand distributorCmd = new SqlCommand();
                    distributorCmd.Parameters.AddRange(distributorSpa);
                    distributorCmd.CommandText = "delete from ProductDistributor where ProductId = @ProductID AND DistributorID != @EntityID";
                    DB.ExecuteSQL(distributorCmd);
                }

                if (DB.GetSqlN("select count(*) as N from {0} where {1} = @EntityID and ProductId = @ProductID".FormatWith(entityMapTableName, entityIdName), spa) == 0)
                {
                    SqlParameter[] insertSpa = { new SqlParameter("@ProductID", productMapInfo.ProductId), new SqlParameter("@EntityID", productMapInfo.EntityId) };
                    SqlCommand insertCmd = new SqlCommand();
                    insertCmd.Parameters.AddRange(insertSpa);
                    insertCmd.CommandText = "insert into {0} (ProductId, {1}, DisplayOrder, CreatedOn) values (@ProductID, @EntityID, 0, getdate())".FormatWith(entityMapTableName, entityIdName);
                    DB.ExecuteSQL(insertCmd);
                }
            }
            else
            {
                DB.ExecuteSQL("delete from {0} where {1} = @EntityID and ProductId = @ProductID".FormatWith(entityMapTableName, entityIdName), spa);
            }
        }

        private void GetEntityTableInfo(String entityType, out String entityMapTableName, out String entityMapIdName)
        {
            entityMapTableName =
                entityMapIdName = String.Empty;

            entityMapTableName = "Product{0}".FormatWith(entityType);
            entityMapIdName = "{0}Id".FormatWith(entityType);
        }

        protected void CloneProduct(Int32 productId)
        {
            DB.ExecuteSQL("aspdnsf_CloneProduct " + productId.ToString());
        }

        protected void DeleteProduct(Int32 productId)
        {
            DB.GetSqlN("select count(*) N from ShoppingCart  with (NOLOCK)  where ProductID=" + productId.ToString() + " and CartType=" + ((int)CartTypeEnum.RecurringCart).ToString());

            DB.ExecuteSQL("delete from ShoppingCart where productid=" + productId.ToString());
            DB.ExecuteSQL("delete from kitcart where productid=" + productId.ToString());
            DB.ExecuteSQL("update Product set deleted=1 where ProductID=" + productId.ToString());

            DB.ExecuteSQL("delete from ProductAffiliate where productid=" + productId.ToString());
            DB.ExecuteSQL("delete from ProductCategory where productid=" + productId.ToString());
            DB.ExecuteSQL("delete from ProductCustomerLevel where productid=" + productId.ToString());
            DB.ExecuteSQL("delete from ProductDistributor where productid=" + productId.ToString());
            DB.ExecuteSQL("delete from ProductGenre where productid=" + productId.ToString());
            DB.ExecuteSQL("delete from ProductVector where productid=" + productId.ToString());
            DB.ExecuteSQL("delete from ProductLocaleSetting where productid=" + productId.ToString());
            DB.ExecuteSQL("delete from ProductManufacturer where productid=" + productId.ToString());
            DB.ExecuteSQL("delete from ProductSection where productid=" + productId.ToString());
        }

        protected void UndeleteProduct(Int32 productId)
        {
            DB.ExecuteSQL("update Product set Deleted = 0 where ProductId = " + productId.ToString());
        }

        protected void NukeProduct(Int32 productId)
        {
            DB.GetSqlN("select count(*) N from ShoppingCart  with (NOLOCK)  where ProductID=" + productId.ToString() + " and CartType=" + ((int)CartTypeEnum.RecurringCart).ToString());
            DB.ExecuteLongTimeSQL("aspdnsf_NukeProduct " + productId.ToString(), 120);
        }

        #endregion

        #region Protected Methods

        protected String ML_Localize(String text)
        {
            String localeSetting = Localization.GetDefaultLocale();
            if (ThisCustomer != null)
                localeSetting = ThisCustomer.LocaleSetting;

            return XmlCommon.GetLocaleEntry(text, localeSetting, false);
        }

        #endregion
    }

    public class ProductEntityMapInfo
    {
        #region Properties

        public String EntityType { get; set; }

        public Int32 EntityId { get; set; }

        public Int32 ProductId { get; set; }

        public String Name { get; set; }

        public Boolean IsMapped { get; set; }

        public Boolean IsDeleted { get; set; }

        #endregion
    }
}
