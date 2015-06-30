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
using AspDotNetStorefront;
using AjaxControlToolkit;
using Telerik.Web.UI;
using AspDotNetStorefrontControls;

namespace AspDotNetStorefront
{
    public partial class EntityObjectListControl : BaseUserControl<DatabaseObjectCollection>
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

        public string TextBoxClientID
        {
            get
            {
                object savedValue = ViewState["TextBoxClientID"];
                if (null == savedValue) { return string.Empty; }

                return savedValue.ToString();
            }
            set
            {
                ViewState["TextBoxClientID"] = value;
            }
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

        private void BindData()
        {
            string alphaFilter = CurrentFilterType == FilterType.AlphaFilter ? CurrentFilter : string.Empty;
            string searchFilter = CurrentFilterType == FilterType.Search ? CurrentFilter : string.Empty;

            var mappedObjects = DatabaseObjectCollection.GetObjects(this.EntityType, alphaFilter, searchFilter, ctrlSearch.CurrentPageSize, this.CurrentPage);
            var grdMap = ctrlSearch.FindControl<RadGrid>("grdMap");
            grdMap.DataSource = mappedObjects;
            grdMap.DataBind();

            ctrlSearch.AllCount = mappedObjects.PageInfo.TotalCount;
            ctrlSearch.StartCount = mappedObjects.PageInfo.StartIndex;
            ctrlSearch.EndCount = mappedObjects.PageInfo.EndIndex;
            ctrlSearch.PageCount = mappedObjects.PageInfo.TotalPages;
            ctrlSearch.CurrentPage = mappedObjects.PageInfo.CurrentPage;

            this.Datasource = mappedObjects;
        }

        protected string ML_Localize(string text)
        {
            return XmlCommon.GetLocaleEntry(text, ThisCustomer.LocaleSetting, false);
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
            {
                //var mappedObjects = this.Datasource;
                //ResolveSelectedMapping(mappedObjects);

                //mappedObjects.SaveMapping();

                //var store = Store.GetStoreList().Find(s => s.StoreID == this.StoreId);
                //if (store != null)
                //{
                //    store.ResetEntityMappingsCache(this.EntityType);
                //}
            }

            BindData();
        }

        protected void grdMap_SortCommand(object source, GridSortCommandEventArgs e)
        {
        }

        protected void grdMap_ItemCreated(object sender, GridItemEventArgs e)
        {            
        }

        protected void grdMap_ItemDataBound(object sender, GridItemEventArgs e)
        {
            if (!string.IsNullOrEmpty(TextBoxClientID))
            {
                if (e.Item.ItemType == GridItemType.Item ||
                e.Item.ItemType == GridItemType.AlternatingItem)
                {
                    var lnkName = e.Item.FindControl<HyperLink>("lnkName");
                    var data = e.Item.DataItem as DatabaseObject;
                    if (lnkName != null && 
                        data != null)
                    {
                        string pushScript = "parent.window.EntityObjectListManager.pushData('{0}', '{1}');".FormatWith(this.TextBoxClientID, data.ID);
                        lnkName.Attributes.Add("onclick", pushScript);
                    }
                }
            }
        }

        protected void grdMap_DataBound(object sender, EventArgs e)
        {
        }
    }
}


