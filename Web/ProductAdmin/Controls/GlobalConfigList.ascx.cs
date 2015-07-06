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
using System.Text;

namespace AspDotNetStorefrontAdmin.Controls
{
    public partial class GlobalConfigList : BaseUserControl<IEnumerable<GlobalConfig>>
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

        private IEnumerable<string> m_configgroups;
        protected IEnumerable<string> ConfigGroups
        {
            get { return m_configgroups; }
            private set { m_configgroups = value; }
        }

        private List<Store> m_stores;
        public List<Store> Stores
        {
            get { return m_stores; }
            set { m_stores = value; }
        }

        private void InitializeConfigGroups()
        {
            string[] groups = { "All" }; // all for now filter
            var allGroups = AppLogic.GlobalConfigTable.GetDistinctGroupNames(); // GlobalConfigCollection.get.GetDistinctAppConfigGroups(1);

            ConfigGroups = allGroups;

            cboConfigGroups.DataSource = allGroups = groups.Concat(allGroups);
            cboConfigGroups.DataBind();
        }

        private IEnumerable<GlobalConfig> SortRoutine(IEnumerable<GlobalConfig> source)
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
            Func<GlobalConfig, object> selector = null;

            switch (SortColumn)
            {

                case "Name":
                    selector = (config) => config.Name;
                    break;

                case "Description":
                    selector = (config) => config.Description;
                    break;

                case "GroupName":
                    selector = (config) => config.GroupName;
                    break;

                case "ConfigValue":
                    selector = (config) => config.ConfigValue;
                    break;
            }

            if (this.SortColumnDirection == GridSortOrder.Ascending)
            {
                return source.OrderBy(selector);
            }
            else
            {
                return source.OrderByDescending(selector);
            }
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

        private void BindData()
        {
            var results = GetDatasource();

            this.Datasource = results;

            RadGrid grdConfigs = ctrlSearch.FindControl<RadGrid>("grdConfigs");
            grdConfigs.DataSource = this.Datasource;
            grdConfigs.DataBind();

            ctrlSearch.AllCount = results.TotalCount;
            ctrlSearch.StartCount = results.StartIndex;
            ctrlSearch.EndCount = results.EndIndex;
            ctrlSearch.PageCount = results.TotalPages;
            ctrlSearch.CurrentPage = results.CurrentPage;
        }


        private PaginatedList<GlobalConfig> GetDatasource()
        {
            GlobalConfigCollection allConfigs = AppLogic.GlobalConfigTable;

            PagedSearchTemplate<GlobalConfig> pagedSearch = new PagedSearchTemplate<GlobalConfig>();

            // 0 index is the All, therefore no filter
            if (cboConfigGroups.SelectedIndex > 0)
            {
                string group = cboConfigGroups.SelectedValue;
                Func<GlobalConfig, bool> groupFilter = (config) => config.GroupName.EqualsIgnoreCase(group);
                pagedSearch.Filters.Add(groupFilter);
            }

            var filterString = this.CurrentFilter;
            if (CurrentFilterType == FilterType.Search)
            {
                Func<GlobalConfig, bool> searchFilter = (config) => config.Name.ContainsIgnoreCase(filterString);
                pagedSearch.Filters.Add(searchFilter);
            }
            else if (CurrentFilterType == FilterType.AlphaFilter)
            {
                Func<GlobalConfig, bool> alphaFilter = null;
                if (filterString == "[0-9]")
                {
                    alphaFilter = (config) => config.Name.StartsWithNumbers();
                }
                else
                {
                    alphaFilter = (config) => config.Name.StartsWithIgnoreCase(filterString);
                }
                pagedSearch.Filters.Add(alphaFilter);
            }

            // apply sorting if applicable
            pagedSearch.Sorter = SortRoutine;

            var results = pagedSearch.Search(allConfigs, PAGE_SIZE, CurrentPage);
            return results;
        }

        protected void grdAppConfigs_ItemCommand(object source, GridCommandEventArgs e)
        {
            if (e.CommandName.EqualsIgnoreCase("UpdateConfig"))
            {
                var ctrlAppConfig = e.Item.FindControl<BaseUserControl<GlobalConfig>>("ctrlConfig");
                ctrlAppConfig.UpdateChanges();
            }

            // rebind for changes...
            BindData();
        }

        protected void grdAppConfigs_SortCommand(object source, GridSortCommandEventArgs e)
        {
            // The RadGrid has a bug in it's sorting display on the header
            // we'll just do the sorting manually
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

            BindData();
        }

        protected override void OnInit(EventArgs e)
        {
            InitializeConfigGroups();

            base.OnInit(e);
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


        protected void grdAppConfigs_ItemCreated(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.Item ||
                e.Item.ItemType == GridItemType.AlternatingItem)
            {
                Panel pnlConfig = e.Item.FindControl<Panel>("pnlConfig");
                HideModalPanelByDefault(pnlConfig);

                var extConfigPanelEdit = e.Item.FindControl<ModalPopupExtender>("extConfigPanelEdit");
                if (extConfigPanelEdit != null)
                {
                    // attach the double-click event
                    e.Item.Attributes["ondblclick"] = "$find('{0}').show();".FormatWith(extConfigPanelEdit.ClientID);
                }
            }
        }

        protected string TrimText(string text, int max)
        {
            if (text.Length > max)
            {
                return text.Substring(0, max) + "...";
            }
            else
            {
                return text;
            }
        }

        protected void ctrlSearch_ContentCreated(object sender, FilterEventArgs e)
        {
            // This is the point wherein we're ready to bind the data
            // Sort by name by default
            // until configured otherwise    
            if (!this.IsPostBack)
            {
                SortColumn = "Name";
                CurrentPage = 1;
                CurrentFilter = string.Empty;
                CurrentFilterType = FilterType.None;
            }
           
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

        private void ResetFilters()
        {
            // reset filters
            this.CurrentFilter = string.Empty;
            this.CurrentFilterType = FilterType.None;
            this.CurrentPage = 1;

            ctrlSearch.ResetFilters(); // reset filters for display
        }

        protected void cboConfigGroups_SelectedIndexChanged(object sender, EventArgs e)
        {
            ResetFilters();
            BindData();
        }
    }
}


