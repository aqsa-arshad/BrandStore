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
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;
using AjaxControlToolkit;
using Telerik.Web.UI;
using System.Data.SqlClient;
using System.Data;

namespace AspDotNetStorefrontAdmin.Controls
{
    public partial class TopicMapping : BaseUserControl<IEnumerable<Topic>>
    {
        private const string CURRENT_PAGE = "CurrentPage";
        private const string SORT_COLUMN = "SortColumn";
        private const string SORT_COLUMN_DIRECTION = "SortColumnDirection";
        private const string CURRENT_FILTER = "CurrentFilter";
        private const string CURRENT_FILTER_TYPE = "CurrentFilterType";
        private const int PAGE_SIZE = 15;
        private const string THIS_TOPICID = "EditingTopicID";

        public string ThisTopicID
        {
            get 
            {
                object savedValue = ViewState[THIS_TOPICID];
                if(null == savedValue) { return "0"; }

                return savedValue.ToString();
            }
            set 
            {
                ViewState[THIS_TOPICID] = value;
            }
        }

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

        protected void Page_Load(object sender, EventArgs e)
        {

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

        public void BindData()
        {
            PaginatedList<Topic> results = GetDatasource();

            this.Datasource = results;

            RadGrid grdT = ctrlSearch.FindControl<RadGrid>("grdTopics");
            grdT.DataSource = this.Datasource;
            grdT.DataBind();

            ctrlSearch.AllCount = results.TotalCount;
            ctrlSearch.StartCount = results.StartIndex;
            ctrlSearch.EndCount = results.EndIndex;
            ctrlSearch.PageCount = results.TotalPages;
            ctrlSearch.CurrentPage = results.CurrentPage;
        }

        public String LocalizeName(String topicName)
        {
            if (ThisCustomer == null)
            {
                ThisCustomer = AppLogic.GetCurrentCustomer();
            }

            return XmlCommon.GetLocaleEntry(topicName, ThisCustomer.LocaleSetting, true);
        }

        private PaginatedList<Topic> GetDatasource()
        {
            List<Topic> allTopics = Topic.GetTopics().Where(t => !t.TopicID.ToString().Equals(ThisTopicID)).ToList();

            PagedSearchTemplate<Topic> pagedSearch = new PagedSearchTemplate<Topic>();

            String filterString = this.CurrentFilter;
            if (CurrentFilterType.Equals(FilterType.Search))
            {
                Func<Topic, bool> searchFilter = (t) => t.TopicName.ContainsIgnoreCase(filterString);
                pagedSearch.Filters.Add(searchFilter);
            }
            else if (CurrentFilterType.Equals(FilterType.AlphaFilter))
            {
                Func<Topic, bool> alphaFilter = null;
                if (filterString == "[0-9]")
                {
                    alphaFilter = (t) => t.TopicName.StartsWithNumbers();
                }
                else
                {
                    alphaFilter = (t) => t.TopicName.StartsWithIgnoreCase(filterString);
                }
                pagedSearch.Filters.Add(alphaFilter);
            }

            // apply sorting if applicable
            pagedSearch.Sorter = SortRoutine;

            PaginatedList<Topic> results = pagedSearch.Search(allTopics, PAGE_SIZE, CurrentPage);
            return results;
        }
        private IEnumerable<Topic> SortRoutine(IEnumerable<Topic> source)
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
            Func<Topic, object> selector = null;

            switch (SortColumn)
            {
                case "TopicID":
                    selector = (t) => t.TopicID;
                    break;

                case "TopicName":
                    selector = (t) => t.TopicName;
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

        protected void grdTopics_ItemCommand(object source, GridCommandEventArgs e)
        {
            if (e.CommandName.EqualsIgnoreCase("SaveMapping"))
            {
                IEnumerable<Topic> ts = this.Datasource;
                ResolveSelectedMapping(ts);

                this.Save(ts);
            }

            // rebind for changes
            BindData();
        }

        private void ResolveSelectedMapping(IEnumerable<Topic> mappedTopics)
        {
            RadGrid grdT = ctrlSearch.FindControl<RadGrid>("grdTopics");
            IEnumerable<GridDataItem> editItems = grdT.Items.Cast<GridDataItem>().Where(item => item.ItemType == GridItemType.Item || item.ItemType == GridItemType.AlternatingItem);
            foreach (GridDataItem item in editItems)
            {
                HiddenField hdfTID = item.FindControl<HiddenField>("hdfTopicID");
                int tID = hdfTID.Value.ToNativeInt();

                // find the matching mapped object
                Topic mappedTopic = mappedTopics.FirstOrDefault(t => t.TopicID == tID);
                if (mappedTopic != null)
                {
                    CheckBox chkIsMapped = item.FindControl<CheckBox>("chkIsMapped");
                    mappedTopic.IsMapped = chkIsMapped.Checked;
                }
            }
        }

        private void Save(IEnumerable<Topic> mappedTopics)
        {
            SqlCommand cmd = new SqlCommand("aspdnsf_SaveTopicMap");
            cmd.CommandType = CommandType.StoredProcedure;

            SqlParameter pTopicID = new SqlParameter("@TopicID", SqlDbType.Int) { Value = ThisTopicID };
            SqlParameter pParentTopicID = new SqlParameter("@ParentTopicID", SqlDbType.Int);
            SqlParameter pMap = new SqlParameter("@Map", SqlDbType.Bit);

            cmd.Parameters.Add(pTopicID);
            cmd.Parameters.Add(pParentTopicID);
            cmd.Parameters.Add(pMap);


            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (cmd)
                {
                    cmd.Connection = con;
                    foreach (Topic t in mappedTopics)
                    {
                        pParentTopicID.Value = t.TopicID;
                        pMap.Value = t.IsMapped;

                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        protected void grdTopics_SortCommand(object source, GridSortCommandEventArgs e)
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
    }
}
