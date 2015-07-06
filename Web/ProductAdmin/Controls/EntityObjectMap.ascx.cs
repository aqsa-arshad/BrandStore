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
using AjaxControlToolkit;
using Telerik.Web.UI;

namespace AspDotNetStorefrontAdmin.Controls
{
    public partial class EntityObjectMap : BaseUserControl<AspDotNetStorefront.MappedObjectCollection>
    {
        private const string CURRENT_PAGE = "CurrentPage";
        private const string SORT_COLUMN = "SortColumn";
        private const string SORT_COLUMN_DIRECTION = "SortColumnDirection";
        private const string CURRENT_FILTER = "CurrentFilter";
        private const string CURRENT_FILTER_TYPE = "CurrentFilterType";
        private const int PAGE_SIZE = 15;

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

        public int CurrentPage
        {
            get { return null == ViewState[CURRENT_PAGE] ? 1 : (int)ViewState[CURRENT_PAGE]; }
            set { ViewState[CURRENT_PAGE] = value; }
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
        
        public string EntityType
        {
            get
            {
                object savedValue = ViewState["EntityType"];
                if (null == savedValue) { return string.Empty; }

                return savedValue.ToString();
            }
            set
            {
                ViewState["EntityType"] = value;
            }
        }

        public int StoreId
        {
            get { return null == ViewState["StoreId"] ? 1 : (int)ViewState["StoreId"]; }
            set { ViewState["StoreId"] = value; }
        }

        protected override void OnInit(EventArgs e)
        {
            ctrlSearch.PageSizes = new List<int>(new int[] { 10, 15, 20, 25, 30, 40, 50 });
            if (!this.IsPostBack)
            {
                ctrlSearch.CurrentPageSize = 10;
            }

            base.OnInit(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected string ML_Localize(string text)
        {
            String locale = "en-us";
            
            if (ThisCustomer != null && String.IsNullOrEmpty(ThisCustomer.LocaleSetting))
                locale = ThisCustomer.LocaleSetting;

            return XmlCommon.GetLocaleEntry(text, locale, false);
        }

        protected string Trim(String text, Int32 count)
        {
            if (text.Length <= count)
                return text;

            return text.Substring(0, count) + "...";
        }

        protected void ctrlSearch_ContentCreated(object sender, FilterEventArgs e)
        {
            // At this point our controls inside the SearcheableTemplate control
            // has been created, now let's initialize our grid for data

            BindData();
        }

        protected void ctrlSearch_Filter(object sender, FilterEventArgs e)
        {
            CurrentFilter = e.Filter;
            CurrentFilterType = e.Type;
            CurrentPage = e.Page;

            BindData();
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

        protected void grdMap_ItemCommand(object source, GridCommandEventArgs e)
        {
            if (e.CommandName.EqualsIgnoreCase("SaveMapping"))
                ResolveSelectedMapping();

            BindData();
        }

        private void ResolveSelectedMapping()
        {
            var grdMap = ctrlSearch.FindControl<RadGrid>("grdMap");
            var editItems = grdMap.Items.Cast<GridDataItem>().Where(item => item.ItemType == GridItemType.Item || item.ItemType == GridItemType.AlternatingItem);
            foreach (var item in editItems)
            {
                var hdfEntityId = item.FindControl<HiddenField>("hdfEntityId");
                int entityId = hdfEntityId.Value.ToNativeInt();

                var dataItems = item.DataItem as IGrouping<String, MappedObject>;
                foreach (var mappedObject in dataItems)
                {
                    var chkStores = item.FindControl<CheckBoxList>("chkStores");
                    ListItem listItem = chkStores.Items.FindByValue(mappedObject.StoreID.ToString());
                    if (listItem == null)
                        continue;

                    mappedObject.IsMapped = listItem.Selected;
                    mappedObject.Save();
                }
            }
        }

        protected void grdMap_SortCommand(object source, GridSortCommandEventArgs e)
        {
        }

        protected void grdMap_ItemCreated(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType != GridItemType.Item && e.Item.ItemType != GridItemType.AlternatingItem)
                return;

            var dataItem = e.Item.DataItem as IGrouping<String, MappedObject>;
            if (dataItem == null)
                return;

            var chkStores = e.Item.FindControl<CheckBoxList>("chkStores");
            if (chkStores == null)
                return;

            
            foreach (Store store in Store.GetStoreList())
            {
                ListItem listItem = new ListItem(Trim(store.Name, 10), store.StoreID.ToString());
                if (dataItem.Any(mo => mo.StoreID == store.StoreID && mo.IsMapped))
                    listItem.Selected = true;

                chkStores.Items.Add(listItem);
            }
        }

        public void BindData()
        {
            string alphaFilter = CurrentFilterType.Equals(FilterType.AlphaFilter) ? CurrentFilter : string.Empty;
            string searchFilter = CurrentFilterType.Equals(FilterType.Search) ? CurrentFilter : string.Empty;

            AspDotNetStorefront.MappedObjectCollection firstMO = null;

            IList<MappedObject> mappedObjects = new List<MappedObject>();
            foreach (Store store in Store.GetStoreList())
            {
                firstMO = AspDotNetStorefront.MappedObjectCollection.GetObjects(store.StoreID, this.EntityType, alphaFilter, searchFilter, ctrlSearch.CurrentPageSize, this.CurrentPage);
                foreach (MappedObject mappedObject in AspDotNetStorefront.MappedObjectCollection.GetObjects(store.StoreID, this.EntityType, alphaFilter, searchFilter, ctrlSearch.CurrentPageSize, this.CurrentPage))
                    mappedObjects.Add(mappedObject);
            }

            var mappedObjectGroups = mappedObjects.GroupBy(m1 => m1.Name);
            var grdMap = ctrlSearch.FindControl<RadGrid>("grdMap");
            grdMap.DataSource = mappedObjectGroups;
            grdMap.DataBind();

            if (firstMO == null)
                return;

            ctrlSearch.AllCount = firstMO.PageInfo.TotalCount;
            ctrlSearch.StartCount = firstMO.PageInfo.StartIndex;
            ctrlSearch.EndCount = firstMO.PageInfo.EndIndex;
            ctrlSearch.PageCount = firstMO.PageInfo.TotalPages;
            ctrlSearch.CurrentPage = firstMO.PageInfo.CurrentPage;
        }
    }
}


