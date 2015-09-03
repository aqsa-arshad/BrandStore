// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontControls
{
    public class SearcheableTemplate : CompositeControl
    {
        private Search thissearch = new Search();
        private NumericSequencePaging ctrlNumericPagingTop = new NumericSequencePaging();
        private NumericSequencePaging ctrlNumericPagingBottom = new NumericSequencePaging();
        private NumericRangePaging ctrlRangePaging = new NumericRangePaging();
        private AlphaPaging ctrlAlphaPaging = new AlphaPaging();
        private List<int> thesePageSizes = new List<int>();
        private DropDownList cboPageSizes = new DropDownList();

        public event EventHandler<FilterEventArgs> Filter;        

        private List<KeyValuePair<string, string>> alphaFilters = new List<KeyValuePair<string, string>>();
        private const string CURRENT_FILTER = "CurrentFilter";
        private const string CURRENT_FILTER_TYPE = "CurrentFilterType";        
        private const string PAGE_COMMAND = "PageCommand";
        private const string ALL_COUNT = "AllCount";
        //private const string PAGE_COUNT = "PageCount";
        //private const string CURRENT_PAGE = "CurrentPage";
        private const string START_COUNT = "StartCount";
        private const string END_COUNT = "EndCount";

        public SearcheableTemplate()
        {
            thissearch.ID = "ctrlSearch";
            ctrlNumericPagingTop.ID = "ctrlNumericPagingTop";
            ctrlNumericPagingBottom.ID = "ctrlNumericPagingBottom";
            ctrlRangePaging.ID = "ctrlRangePaging";
            ctrlAlphaPaging.ID = "ctrlAlphaPaging";
            cboPageSizes.ID = "cboPageSizes";

            thesePageSizes = new List<int>();
        }

        private ITemplate m_headertemplate;

        [Browsable(false),
        Description("The header template"),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public virtual ITemplate HeaderTemplate
        {
            get { return m_headertemplate; }
            set { m_headertemplate = value; }
        }

        private ITemplate m_contenttemplate;

        [Browsable(false),
        Description("The content area template"),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public virtual ITemplate ContentTemplate
        {
            get { return m_contenttemplate; }
            set { m_contenttemplate = value; }
        }

        private ITemplate m_contentheadertemplate;

        [Browsable(false),
        Description("The content area template"),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public virtual ITemplate ContentHeaderTemplate
        {
            get { return m_contentheadertemplate; }
            set { m_contentheadertemplate = value; }
        }

        [Browsable(true)]
        public int PageCount
        {
            get
            {
                return ctrlNumericPagingTop.PageCount;
            }
            set
            {
                ctrlNumericPagingTop.PageCount = value;
                ctrlNumericPagingBottom.PageCount = value;
            }
        }


        [Browsable(true)]
        public int CurrentPage
        {
            get
            {
                return ctrlNumericPagingTop.CurrentPage;
            }
            set
            {
                ctrlNumericPagingTop.CurrentPage = value;
                ctrlNumericPagingBottom.CurrentPage = value;
            }
        }

        [Browsable(true)]
        public int AllCount
        {
            get
            {
                return ctrlRangePaging.AllCount;
            }
            set
            {
                ctrlRangePaging.AllCount = value;
            }
        }

        [Browsable(true)]
        public int StartCount
        {
            get
            {
                return ctrlRangePaging.StartCount;
            }
            set
            {
                ctrlRangePaging.StartCount = value;
            }
        }

        [Browsable(true)]
        public int EndCount
        {
            get
            {
                return ctrlRangePaging.EndCount;
            }
            set
            {
                ctrlRangePaging.EndCount = value;
            }
        }

        [Browsable(true)]
        public int AlphaGrouping
        {
            get
            {
                return ctrlAlphaPaging.AlphaGrouping;
            }
            set
            {
                ctrlAlphaPaging.AlphaGrouping = value;
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

        [Browsable(true)]
        public FilterType CurrentFilterType
        {
            get
            {
                object intValue = ViewState[CURRENT_FILTER_TYPE];
                if (null == intValue) { return 0; }

                return (FilterType)((int)intValue);
            }
            set
            {
                ViewState[CURRENT_FILTER_TYPE] = (int)value;
            }
        }

        public List<int> PageSizes 
        {
            get { return thesePageSizes; }
            set 
            {
                thesePageSizes = value;
                if (thesePageSizes.Count > 0)
                {
                    BuildPageSizes();
                }

                ChildControlsCreated = false;
            }
        }

        [Browsable(true)]
        public int CurrentPageSize
        {
            get
            {
                object intValue = ViewState["CurrentPageSize"];
                if (null == intValue) { return 0; }

                return (int)intValue;
            }
            set
            {
                ViewState["CurrentPageSize"] = (int)value;
            }
        }

        private void BuildPageSizes()
        {
            cboPageSizes.Items.Clear();
            foreach (int pageSize in thesePageSizes)
            {
                ListItem lSize = new ListItem(pageSize.ToString());
                lSize.Selected = pageSize.Equals(CurrentPageSize);
                cboPageSizes.Items.Add(lSize);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty)]
        public Search Search
        {
            get { return this.thissearch; }
        }

        protected override HtmlTextWriterTag TagKey
        {
            get { return HtmlTextWriterTag.Div; }
        }

        private void ClearSearchFilter()
        {
            thissearch.SearchText = string.Empty;
        }

        private void ClearAlphaFilter()
        {
            this.CurrentFilter = string.Empty;
            ctrlAlphaPaging.Highlight = false;
        }

        protected override void OnInit(EventArgs e)
        {
            thissearch.SearchInvoked += new EventHandler(search_SearchInvoked);
            cboPageSizes.AutoPostBack = true;
            cboPageSizes.SelectedIndexChanged += new EventHandler(cboPageSizes_SelectedIndexChanged);

            base.OnInit(e);
        }

        void cboPageSizes_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.CurrentPageSize = cboPageSizes.SelectedValue.ToNativeInt();
            this.CurrentPage = 1;

            FilterEventArgs args = new FilterEventArgs(this.CurrentFilterType, this.CurrentFilter, this.CurrentPage);
            OnFilter(args);
        }

        void search_SearchInvoked(object sender, EventArgs e)
        {
            ClearAlphaFilter();

            string searchFilter = thissearch.SearchText;

            this.CurrentFilter = searchFilter;
            this.CurrentFilterType = FilterType.Search;

            FilterEventArgs args = new FilterEventArgs(this.CurrentFilterType, this.CurrentFilter, this.CurrentPage);
            OnFilter(args);

            this.CurrentPage = 1;

            OnSearchInvoked(e);

        }

        public event EventHandler SearchInvoked;

        protected void OnSearchInvoked(EventArgs e)
        {
            if (SearchInvoked != null)
            {
                SearchInvoked(this, e);
            }
        }

        protected override bool OnBubbleEvent(object source, EventArgs args)
        {
            if (args is CommandEventArgs)
            {
                CommandEventArgs e = args as CommandEventArgs;
                if (e.CommandName == AlphaPaging.ALPHA_FILTER_COMMAND)
                {
                    ClearSearchFilter();

                    string thisfilt = e.CommandArgument.ToString();

                    this.CurrentFilter = thisfilt;
                    ctrlAlphaPaging.CurrentFilter = thisfilt;
                    ctrlAlphaPaging.Highlight = true;

                    this.CurrentFilterType = FilterType.AlphaFilter;

                    // reset the current page
                    this.CurrentPage = 1;

                    OnFilter(new FilterEventArgs(this.CurrentFilterType, this.CurrentFilter, this.CurrentPage));
                }
                else if (e.CommandName == PAGE_COMMAND)
                {
                    int navigateToPage = int.Parse(e.CommandArgument.ToString());
                    this.CurrentPage = navigateToPage;

                    FilterEventArgs fArgs = new FilterEventArgs(this.CurrentFilterType, this.CurrentFilter, this.CurrentPage);
                    fArgs.Page = navigateToPage;

                    OnFilter(fArgs);
                }
            }

            return base.OnBubbleEvent(source, args);
        }

        protected virtual void OnFilter(FilterEventArgs e)
        {
            if (Filter != null)
            {
                Filter(this, e);
            }
        }

        public void ResetFilters()
        {
            ClearSearchFilter();
            ClearAlphaFilter();
            CurrentPage = 1;
        }

        private Color DEFAULT_COLOR = Color.FromArgb(204, 204, 204);
        private const string INNERBORDER_COLOR = "InnerBorderColor";

        public Color InnerBorderColor
        {
            get 
            {
                object val = ViewState["InnerBorderColor"];
                if (val == null)
                {
                    return DEFAULT_COLOR;
                }

                return (Color)val;
            }
            set 
            {
                ViewState["InnerBorderColor"] = value;
            }
        }

        public event EventHandler<FilterEventArgs> ContentCreated;

        protected void OnContentCreated(FilterEventArgs e)
        {
            if (ContentCreated != null)
            {
                ContentCreated(this, e);
            }
        }

        protected override void CreateChildControls()
        {
            Controls.Clear();

            Controls.Add(new LiteralControl("<table id=\"ctlContainer\" style=\"width:100%;\" border=\"0\" cellspacing=\"0\" cellspadding=\"0\" >\n"));

            if (this.HeaderTemplate != null)
            {
                // header template
                Controls.Add(new LiteralControl("    <tr>\n"));
                Controls.Add(new LiteralControl("        <td colspan=\"2\" align=\"left\" valign=\"top\" >\n"));

                this.HeaderTemplate.InstantiateIn(this);
                
                Controls.Add(new LiteralControl("        </td>\n"));
                Controls.Add(new LiteralControl("    </tr>\n"));
            }

            // search area
            Controls.Add(new LiteralControl("    <tr>\n"));
            Controls.Add(new LiteralControl("        <td colspan=\"2\" align=\"left\" valign=\"top\" >\n"));            

            if (PageSizes.Count > 0)
            {
                Controls.Add(new LiteralControl("    <table>\n"));
                Controls.Add(new LiteralControl("    <tr>\n"));
                Controls.Add(new LiteralControl("        <td align=\"left\" valign=\"top\" >\n"));
                Controls.Add(thissearch);
                Controls.Add(new LiteralControl("        </td>\n"));
                Controls.Add(new LiteralControl("        <td align=\"left\" valign=\"top\" >\n"));
                Controls.Add(new LiteralControl("        &nbsp;Page Size:&nbsp"));
                Controls.Add(cboPageSizes);
                Controls.Add(new LiteralControl("        </td>\n"));
                Controls.Add(new LiteralControl("    </tr>\n"));
                Controls.Add(new LiteralControl("    </table>\n"));
            }
            else
            {
                Controls.Add(thissearch);
            }

            Controls.Add(new LiteralControl("        </td>\n"));
            Controls.Add(new LiteralControl("    </tr>\n"));

            if (this.ContentHeaderTemplate != null)
            {
                Controls.Add(new LiteralControl("    <tr>\n"));
                Controls.Add(new LiteralControl("        <td colspan=\"2\" align=\"left\" valign=\"top\" >\n"));

                this.ContentHeaderTemplate.InstantiateIn(this);

                Controls.Add(new LiteralControl("        </td>\n"));
                Controls.Add(new LiteralControl("    </tr>\n"));
            }
           
            Controls.Add(new LiteralControl("    <tr>\n"));
            Controls.Add(new LiteralControl("        <td colspan=\"2\" align=\"left\" valign=\"top\" >\n"));
            // Renders:
            // Showing 1 - X of N links
            Controls.Add(ctrlRangePaging);
            // Renders:
            // First | 1, 2, 3, x... Last
            Controls.Add(ctrlNumericPagingTop);
            Controls.Add(new LiteralControl("        </td>\n"));            
            Controls.Add(new LiteralControl("    </tr>\n"));

            // alpha filters area
            Controls.Add(new LiteralControl("    <tr>\n"));
            Controls.Add(new LiteralControl("        <td align=\"left\" valign=\"top\" class=\"alpha_filters_column\" >\n"));

            // Alpha paging area
            ctrlAlphaPaging.CssClass = "alpha_filters";
            Controls.Add(ctrlAlphaPaging);

            Controls.Add(new LiteralControl("        </td>\n"));
            // the treeview control area
            Controls.Add(new LiteralControl("        <td align=\"left\" valign=\"top\" class=\"grid_results_column\" >\n"));

            if (this.ContentTemplate != null)
            {
                this.ContentTemplate.InstantiateIn(this);

                FilterEventArgs fArgs = new FilterEventArgs(this.CurrentFilterType, this.CurrentFilter, this.CurrentPage);
                OnContentCreated(fArgs);
            }
            
            Controls.Add(new LiteralControl("        </td>\n"));
            Controls.Add(new LiteralControl("    </tr>\n"));

            // paging area
            // page info
            Controls.Add(new LiteralControl("    <tr>\n"));
            Controls.Add(new LiteralControl("        <td colspan=\"2\" align=\"left\" valign=\"top\" >\n"));
            Controls.Add(new LiteralControl("           <div id=\"divPageCtl\">\n"));

            // Renders:
            // First | 1, 2, 3 ... >> Last
            Controls.Add(ctrlNumericPagingBottom);

            Controls.Add(new LiteralControl("           </div>\n"));
            Controls.Add(new LiteralControl("        </td>\n"));
            Controls.Add(new LiteralControl("    </tr>\n"));

            Controls.Add(new LiteralControl("</table>\n"));

            base.CreateChildControls();
        }

    }

    public enum FilterType
    {
        None = -1,
        Search = 1,
        AlphaFilter = 2
    }

    public class FilterEventArgs : EventArgs
    {
        public FilterEventArgs(FilterType type, string value, int page)
        {
            this.Type = type;
            this.Filter = value;
            this.Page = page;
        }

        private FilterType m_type;
        private string m_filter;
        private int m_page;

        public FilterType Type
        {
            get { return m_type; }
            set { m_type = value; }
        }

        public string Filter
        {
            get { return m_filter; }
            set { m_filter = value; }
        }

        public int Page
        {
            get { return m_page; }
            set { m_page = value; }
        }
    }

}
