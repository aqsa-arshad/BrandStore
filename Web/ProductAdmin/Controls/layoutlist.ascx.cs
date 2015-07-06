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
using AspDotNetStorefrontLayout;
using AspDotNetStorefrontControls;
using AjaxControlToolkit;
using Telerik.Web.UI;
using System.Web.UI.HtmlControls;
using AspDotNetStorefrontAdmin.Controls;
using AspDotNetStorefront;

namespace AspDotNetStorefrontAdmin.Controls
{
    public partial class LayoutList : BaseUserControl<IEnumerable<LayoutData>>
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

        private void InitializeAddNewLayout()
        {
            var newLayout = new LayoutData(0, new Guid(DB.GetNewGUID()), String.Empty, String.Empty, String.Empty, 1, DateTime.Now, DateTime.Now);
            ctrlAddLayout.Datasource = newLayout;
            ctrlAddLayout.AddMode = true;
            ctrlAddLayout.ThisCustomer = ThisCustomer;
            ctrlAddLayout.DataBind();
        }

        private IEnumerable<LayoutData> SortRoutine(IEnumerable<LayoutData> source)
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
            Func<LayoutData, object> selector = null;

            switch (SortColumn)
            {
                case "LayoutID":
                    selector = (layout) => layout.LayoutID;
                    break;

                case "Name":
                    selector = (layout) => layout.Name;
                    break;

                case "Description":
                    selector = (layout) => layout.Description;
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
        /// <typeparam name="T">The type of object to case the GridItem to</typeparam>
        /// <param name="item">The GridItem to retrieve the DataItem from</param>
        /// <returns>The cast object of type T from the GridItem.DataItem</returns>
        protected T DataItemAs<T>(GridItem item) where T : class
        {
            return item.DataItem as T;
        }

        private void BindData()
        {
            var results = GetDatasource();

            this.Datasource = results;

            ctrlSearch.AllCount = results.TotalCount;
            ctrlSearch.StartCount = results.StartIndex;
            ctrlSearch.EndCount = results.EndIndex;
            ctrlSearch.PageCount = results.TotalPages;
            ctrlSearch.CurrentPage = results.CurrentPage;

            Table tblList = ctrlSearch.FindControl<Table>("tblListItems");
            //Table tblList = (Table)tblListItems;

            tblList.Rows.Clear();

            int curCol = 1;

            TableRow tr = new TableRow();

            foreach (LayoutData ld in results)
            {
                TableCell tc = AddLayoutCell(ld, curCol, tblList.Rows.Count);
                if (curCol <= 5)
                {
                    tr.Cells.Add(tc);
                }
                else
                {
                    curCol = 1;
                    tblList.Rows.Add(tr);
                    
                    tr = new TableRow();

                    tr.Cells.Add(tc);
                }

                curCol++;
            }

            if (tr.Cells.Count > 0)
            {
                do
                {
                    TableCell tc = new TableCell();
                    tr.Cells.Add(tc);
                    tblList.Rows.Add(tr);
                } while (tr.Cells.Count < 6);
            }
        }

        private TableCell AddLayoutCell(LayoutData ld, int curCol, int curRow)
        {
            LayoutItem li = (LayoutItem)LoadControl("~/" + AppLogic.AdminDir() + "/controls/LayoutItem.ascx");
            li.ID = "li_" + curRow.ToString() + "_" + curCol.ToString();
            li.Datasource = ld;
            li.DataBind();
            li.ThisCustomer = (Page as AdminPageBase).ThisCustomer;

            ImageButton cbtn = li.FindControl<ImageButton>("imgbtnClone");
            ImageButton dbtn = li.FindControl<ImageButton>("imgbtnDelete");
            Button sbtn = li.FindControl<Button>("btnSaveEditedLayout");

            // ScriptManager from layouts.aspx
            // TableCell.TableRow.Table.ControlSearch.UpdatePanel.SearchableTemplate.UpdatePanel.LayoutList.UpdatePanel.LayoutPage
            ScriptManager sm = Page.Master.FindControl<ScriptManager>("scrptMgr");

            //UpdatePanel up = Parent.Parent.FindControl<UpdatePanel>("pnlUpdateLayout");

            //AsyncPostBackTrigger apbt = new AsyncPostBackTrigger();
            //apbt.ControlID = "ctrlLayoutList$ctrlSearch$" + cbtn.ClientID;
            //up.Triggers.Add(apbt);

            //apbt = new AsyncPostBackTrigger();
            //apbt.ControlID = "ctrlLayoutList$ctrlSearch$" + li.ID + "$imgbtnDelete";
            //up.Triggers.Add(apbt);

            //apbt = new AsyncPostBackTrigger();
            //apbt.ControlID = "ctrlLayoutList$ctrlSearch$" + li.ID + "$btnSaveEditedLayout";
            //up.Triggers.Add(apbt);

            // register the buttons
            sm.RegisterAsyncPostBackControl(cbtn);
            sm.RegisterAsyncPostBackControl(dbtn);
            sm.RegisterAsyncPostBackControl(sbtn);


            TableCell tc = new TableCell();
            tc.Controls.Add(li);

            
            return tc;
        }

        public override void DataBind()
        {
            BindData();

            base.DataBind();
        }

        private PaginatedList<LayoutData> GetDatasource()
        {
            List<LayoutData> allLayouts = LayoutData.GetLayouts();

            PagedSearchTemplate<LayoutData> pagedSearch = new PagedSearchTemplate<LayoutData>();

            var filterString = this.CurrentFilter;
            if (CurrentFilterType == FilterType.Search)
            {
                Func<LayoutData, bool> searchFilter = (layout) => layout.Name.ContainsIgnoreCase(filterString);
                pagedSearch.Filters.Add(searchFilter);
            }
            else if (CurrentFilterType == FilterType.AlphaFilter)
            {
                Func<LayoutData, bool> alphaFilter = null;
                if (filterString == "[0-9]")
                {
                    alphaFilter = (layout) => layout.Name.StartsWithNumbers();
                }
                else
                {
                    alphaFilter = (layout) => layout.Name.StartsWithIgnoreCase(filterString);
                }
                pagedSearch.Filters.Add(alphaFilter);
            }

            // apply sorting if applicable
            pagedSearch.Sorter = SortRoutine;

            var results = pagedSearch.Search(allLayouts, PAGE_SIZE, CurrentPage);
            return results;
        }

        protected override void OnInit(EventArgs e)
        {
            if (!IsPostBack)
            {
                SortColumn = "LayoutID";
                CurrentPage = 1;
                CurrentFilter = string.Empty;
                CurrentFilterType = FilterType.None;
           

                //BindData();
            }
            InitializeAddNewLayout();
            HideModalPanelByDefault(pnlAddLayout);

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

        private void ResetFilters()
        {
            // reset filters
            this.CurrentFilter = string.Empty;
            this.CurrentFilterType = FilterType.None;
            this.CurrentPage = 1;

            //ctrlSearch.ResetFilters(); // reset filters for display
        }



        protected void btnSaveNewLayout_Click(object sender, EventArgs e)
        {
            if (ctrlAddLayout.UpdateChanges())
            {
                var newlyAddedLayout = ctrlAddLayout.Datasource;
                this.CurrentFilter = newlyAddedLayout.Name;
                this.CurrentFilterType = FilterType.Search;

                InitializeAddNewLayout();
                extLayoutPanel.Hide();

                BindData();
            }

            if (ctrlAddLayout.HasErrors)
            {
                extLayoutPanel.Show();
            }
        }
    }
}

