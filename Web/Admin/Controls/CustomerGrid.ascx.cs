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
using Telerik.Web.UI;

namespace AspDotNetStorefrontAdmin.Controls
{
    /// <summary>
    /// Class for customer data that inherits from the BaseUserControl class, providing a foundation of properties 
    /// that can be set and utilized for controls that require the paging, sorting, searching, and display of large
    /// subsets of data
    /// </summary>
    public partial class CustomerGrid : BaseUserControl<IEnumerable<GridCustomer>>
    {
        #region Private Variables

        private const string CURRENT_PAGE = "CurrentPage";
        private const string SORT_COLUMN = "SortColumn";
        private const string SORT_COLUMN_DIRECTION = "SortColumnDirection";
        private const string CURRENT_FILTER = "CurrentFilter";
        private const string CURRENT_FILTER_TYPE = "CurrentFilterType";
        private const int PAGE_SIZE = 15;

        #endregion

        #region Public Search Properties

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

        #endregion

        #region Page Events

        /// <summary>
        /// Event that fires on page initialization
        /// </summary>
        /// <param name="e">EventArgs</param>
        protected override void OnInit(EventArgs e)
        {
            if (ThisCustomer == null)
            {
                ThisCustomer = AppLogic.GetCurrentCustomer();
            }

            // Hide the add panel OnInit to prevent PageLoad flicker
            HideModalPanelByDefault(pnlAddCustomer);

            base.OnInit(e);
        }

        /// <summary>
        /// Event that fires on page load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">EventArgs</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                SortColumn = "Name";
                CurrentPage = 1;
                CurrentFilter = string.Empty;
                CurrentFilterType = FilterType.None;
            }

            this.ctrlAddCustomer.CustomerAdded += new EventHandler(Customer_Added);
            this.ctrlAddCustomer.CustomerUpdated += new EventHandler(Customer_Updated);

            AddRootAddButton();
        }

        #endregion

        #region Grid Events

        /// <summary>
        /// Method that acts as the event handler when an event requiring additional logic originates
        /// from the customer grid
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e">GridCommandEventArgs</param>
        protected void grdCustomers_ItemCommand(object source, GridCommandEventArgs e)
        {
            if (e.CommandName.EqualsIgnoreCase("EditCustomer"))
            {
                // a customer name was clicked and we should open edit customer control
                // initialized to edit the customer that was clicked
                int cID = int.Parse(e.CommandArgument.ToString());

                ctrlAddCustomer.TargetCustomerID = cID;
                ctrlAddCustomer.AddMode = false;
                ctrlAddCustomer.InitializeControl();

                UpdatePanel upCustomer = ctrlAddCustomer.FindControl<UpdatePanel>("updCustomerEdit");
                upCustomer.Update();

                extCustomerPanel.Show();
            }
            else if (e.CommandName.EqualsIgnoreCase("SetAdmin"))
            {
                // the set admin button was clicked so set the customer for that record
                // to be an admin
                int cID = int.Parse(e.CommandArgument.ToString());

                DB.ExecuteSQL("update dbo.Customer set IsAdmin=1 where CustomerID=" + cID.ToString());

                BindData(false);

                updatePanelSearch.Update();
            }
            else if (e.CommandName.EqualsIgnoreCase("SetSuperAdmin"))
            {
                // the set super admin button was clicked so set the customer for that record
                // to be a super admin
                int cID = int.Parse(e.CommandArgument.ToString());

                DB.ExecuteSQL("update dbo.Customer set IsAdmin=3 where CustomerID=" + cID.ToString());

                BindData(false);

                updatePanelSearch.Update();
            }
            else if (e.CommandName.EqualsIgnoreCase("ClearAdmin"))
            {
                // the clear admin button was clicked for a customer that was an admin
                // so remove admin status from the customer for that record
                int cID = int.Parse(e.CommandArgument.ToString());

                DB.ExecuteSQL("update dbo.Customer set IsAdmin=0 where CustomerID=" + cID.ToString());

                BindData(false);

                updatePanelSearch.Update();
            }
            else if (e.CommandName.EqualsIgnoreCase("ClearSuperAdmin"))
            {
                // the clear super admin button was clicked for a customer that was a super admin
                // so remove super admin status from the customer for that record and set them as
                // a regular admin
                int cID = int.Parse(e.CommandArgument.ToString());

                DB.ExecuteSQL("update dbo.Customer set IsAdmin=1 where CustomerID=" + cID.ToString());

                BindData(false);

                updatePanelSearch.Update();
            }
            else if (e.CommandName.EqualsIgnoreCase("DeletePolls"))
            {
                // the delete poll votes button was clicked to delete all poll votes for the customer
                int cID = int.Parse(e.CommandArgument.ToString());

                DB.ExecuteSQL("delete from PollVotingRecord where CustomerID=" + cID.ToString());
            }
            else if (e.CommandName.EqualsIgnoreCase("Delete"))
            {
                // the delete button was clicked so update the customer for the record with the necessary
                // deleted data (this is a soft delete so don't completely remove from the database) and remove
                // the customer from the datasource for the grid
                int cID = int.Parse(e.CommandArgument.ToString());

                AppLogic.eventHandler("DeleteCustomer").CallEvent("&DeleteCustomer=true&DeletedCustomerID=" + cID.ToString());
                DB.ExecuteSQL("update Customer set deleted=1, EMail=" + DB.SQuote("deleted_") + "+EMail where CustomerID=" + cID.ToString());

                BindData(false);

                updatePanelSearch.Update();
            }
            else if (e.CommandName.EqualsIgnoreCase("Nuke"))
            {
                // the nuke button was clicked so remove the customer from the datasource for the grid and from the database
                int cID = int.Parse(e.CommandArgument.ToString());

                AppLogic.NukeCustomer(cID, false);

                BindData(false);
            }
            else if (e.CommandName.EqualsIgnoreCase("NukeBan"))
            {
                // the nuke and ban button was clicked so remove the customer from the datasource for the grid and from the
                // database, and ban their IP address
                int cID = int.Parse(e.CommandArgument.ToString());

                AppLogic.NukeCustomer(cID, true);

                BindData(false);
            }
        }

        /// <summary>
        /// Event that fires when a column header is clicked, indicating that the user would like to order the results
        /// by the values in that column.  The column can be sorted in ascending or descending order.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e">GridSortCommandEventArgs</param>
        protected void grdCustomers_SortCommand(object source, GridSortCommandEventArgs e)
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

            BindData(false);
        }

        /// <summary>
        /// Event that fires when a grid is loaded onto a page but does not yet have a datasource.  This event
        /// is called directly from the grid itself.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e">GridNeedDataSourceEventArgs</param>
        protected void grdCustomers_NeedDataSource(object source, GridNeedDataSourceEventArgs e)
        {
            BindData(true);
        }

        /// <summary>
        /// Event that fires when an item is created in the grid.  Responseible for setting the values of
        /// the various controls in the different columns of the grid.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">GridItemEventArgs</param>
        protected void grdCustomers_ItemCreated(object sender, GridItemEventArgs e)
        {
            RadGrid grdCustomers = ctrlSearch.FindControl<RadGrid>("grdCustomers");
            GridItem gi = e.Item;

            if (gi.ItemType == GridItemType.Item || gi.ItemType == GridItemType.AlternatingItem)
            {
                GridCustomer gc = gi.DataItem as GridCustomer;

                if (gc != null)
                {
                    // Customer ID column
                    LinkButton lbtnID = gi.FindControl<LinkButton>("lbtnID");
                    lbtnID.Text = gc.CustomerID.ToString();

                    Literal ltCreated = gi.FindControl<Literal>("ltCreatedOn");
                    ltCreated.Text = Localization.ToThreadCultureShortDateString(gc.CreatedOn);

                    // Customer Name column
                    LinkButton lbtnName = gi.FindControl<LinkButton>("lbtnName");
                    lbtnName.Text = gc.Name;

                    // Order History column
                    LinkButton lbtnOrderHistory = gi.FindControl<LinkButton>("lbtnOrderHistory");

                    if (Customer.HasOrders(gc.CustomerID))
                    {
                        lbtnOrderHistory.Text = AppLogic.GetString("admin.common.View", ThisCustomer.LocaleSetting);
                        lbtnOrderHistory.PostBackUrl = "cst_history.aspx?Customerid=" + gc.CustomerID;
                    }
                    else
                    {
                        lbtnOrderHistory.Text = AppLogic.GetString("admin.common.None", ThisCustomer.LocaleSetting);
                        lbtnOrderHistory.PostBackUrl = String.Empty;
                        lbtnOrderHistory.Enabled = false;
                    }

                    // Admin column
                    Literal ltIsAdmin = gi.FindControl<Literal>("ltIsAdmin");
                    Button btnSetAdmin = gi.FindControl<Button>("btnSetAdmin");
                    Button btnSetSuperAdmin = gi.FindControl<Button>("btnSetSuperAdmin");

                    if (gc.Admin > 0)
                    {
                        ltIsAdmin.Text = AppLogic.GetString("admin.common.Yes", ThisCustomer.LocaleSetting) + CommonLogic.IIF(gc.Admin.Equals(3), " (SuperUser)", "");

                        if (gc.Admin.Equals(3))
                        {
                            btnSetAdmin.Visible = false;

                            if (ThisCustomer.IsAdminSuperUser)
                            {
                                btnSetSuperAdmin.Text = AppLogic.GetString("admin.customers.ClearSuperAdmin", ThisCustomer.LocaleSetting);
                                btnSetSuperAdmin.Visible = true;
                                btnSetSuperAdmin.CommandName = "ClearSuperAdmin";
                                btnSetSuperAdmin.CommandArgument = gc.CustomerID.ToString();
                            }
                            else
                            {
                                btnSetSuperAdmin.Visible = false;
                            }
                        }
                        else
                        {
                            if (ThisCustomer.IsAdminSuperUser)
                            {
                                btnSetAdmin.Text = AppLogic.GetString("admin.customers.ClearAdmin", ThisCustomer.LocaleSetting);
                                btnSetAdmin.Visible = true;
                                btnSetAdmin.CommandName = "ClearAdmin";
                                btnSetAdmin.CommandArgument = gc.CustomerID.ToString();

                                btnSetSuperAdmin.Text = AppLogic.GetString("admin.customers.SetSuperAdmin", ThisCustomer.LocaleSetting);
                                btnSetSuperAdmin.Visible = true;
                                btnSetSuperAdmin.CommandName = "SetSuperAdmin";
                                btnSetSuperAdmin.CommandArgument = gc.CustomerID.ToString();
                            }
                            else
                            {
                                btnSetSuperAdmin.Visible = false;
                                btnSetAdmin.Visible = false;
                            }
                        }
                    }
                    else
                    {
                        ltIsAdmin.Text = AppLogic.GetString("admin.common.No", ThisCustomer.LocaleSetting);

                        if (ThisCustomer.IsAdminSuperUser)
                        {
                            btnSetAdmin.Text = AppLogic.GetString("admin.customers.SetAdmin", ThisCustomer.LocaleSetting);
                            btnSetAdmin.Visible = true;
                            btnSetAdmin.CommandName = "SetAdmin";
                            btnSetAdmin.CommandArgument = gc.CustomerID.ToString();

                            btnSetSuperAdmin.Text = AppLogic.GetString("admin.customers.SetSuperAdmin", ThisCustomer.LocaleSetting);
                            btnSetSuperAdmin.Visible = true;
                            btnSetSuperAdmin.CommandName = "SetSuperAdmin";
                            btnSetSuperAdmin.CommandArgument = gc.CustomerID.ToString();
                        }
                        else
                        {
                            btnSetSuperAdmin.Visible = false;
                            btnSetAdmin.Visible = false;
                        }
                    }

                    // Subscription column
                    Literal ltSubscriptionExpiresOn = gi.FindControl<Literal>("ltSubscriptionExpiresOn");
                    ltSubscriptionExpiresOn.Text = CommonLogic.IIF(gc.SubscriptionExpires.Equals(DateTime.MinValue), AppLogic.GetString("admin.common.NA", ThisCustomer.LocaleSetting), Localization.ParseLocaleDateTime(gc.SubscriptionExpires.ToString(), ThisCustomer.LocaleSetting).ToShortDateString());

                    // Email column
                    Literal ltEmailMailTo = gi.FindControl<Literal>("ltEmailMailTo");
                    Literal ltOkToEmail = gi.FindControl<Literal>("ltOkToEmail");

                    ltEmailMailTo.Text = "<a href=\"mailto:"+gc.Email+"\">" + gc.Email + "</a>";
                    ltOkToEmail.Text = "OkToEmail:" + CommonLogic.IIF(gc.OkToEmail, AppLogic.GetString("admin.common.Yes", ThisCustomer.LocaleSetting), AppLogic.GetString("admin.common.No", ThisCustomer.LocaleSetting));

                    // billing address column
                    Literal ltAddress = gi.FindControl<Literal>("ltAddress");

                    ltAddress.Text = Customer.BillToAddress(false, true, gc.CustomerID, "<br/>");

                    // nuke column
                    Button btnNuke = gi.FindControl<Button>("btnNuke");
                    btnNuke.OnClientClick = "return confirm('" + AppLogic.GetString("admin.customers.NukeCustomerConfirmation", ThisCustomer.LocaleSetting).Replace(@"'", @"\'").Replace("\\n", " ").Replace("\n", " ") + "');";

                    // nuke and ban column
                    Button btnNukeBan = gi.FindControl<Button>("btnNukeBan");
                    btnNukeBan.OnClientClick = "return confirm('" + AppLogic.GetString("admin.customers.NukeCustomerConfirmation", ThisCustomer.LocaleSetting).Replace(@"'", @"\'").Replace("\\n", " ").Replace("\n", " ") + "');";
                }
            }
        }

        #endregion

        #region Search and Filter

        

        /// <summary>
        /// Event that fires when a filter changes occurs that requires the set of data in the grid
        /// to be altered or reorganized
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">FilterEventArgs</param>
        protected void ctrlSearch_Filter(object sender, FilterEventArgs e)
        {
            CurrentFilter = e.Filter;
            CurrentFilterType = e.Type;
            CurrentPage = e.Page;

            AddRootAddButton();

            BindData(false);
        }

        #endregion

        #region Events

        /// <summary>
        /// Method to catch bubbled event from the customer control when a new customer is added
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">EventArgs</param>
        private void Customer_Added(object sender, EventArgs e)
        {
            int cID = 0;

            if (CommonLogic.IsInteger(sender.ToString()))
            {
                cID = int.Parse(sender.ToString());
            }

            if (cID != 0)
            {
                BindData(false);
                updatePanelSearch.Update();
            }
        }

        /// <summary>
        /// Method to catch bubbled event from the customer control when a customer is updated
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">EventArgs</param>
        private void Customer_Updated(object sender, EventArgs e)
        {
            int cID = 0;

            if (CommonLogic.IsInteger(sender.ToString()))
            {
                cID = int.Parse(sender.ToString());
            }

            if (cID != 0)
            {
                BindData(false);

                updatePanelSearch.Update();
            }
        }


        /// <summary>
        /// Click event for the button that initiates the addition of a new customer by initializing the editcustomer control
        /// and opening it in a modal popup
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">ImageClickEventArgs</param>
        protected void imgbtnAdd_Click(object sender, ImageClickEventArgs e)
        {
            ctrlAddCustomer.TargetCustomerID = 0;
            ctrlAddCustomer.AddMode = true;
            ctrlAddCustomer.InitializeControl();

            UpdateCustomerPopup();

            extCustomerPanel.Show();
        }

        /// <summary>
        /// Click event the button responsible for cancelling the add/edit of a customer in the editcustomer control
        /// in the modal popup
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">EventArgs</param>
        protected void btnCancelAddCustomer_Click(object sender, EventArgs e)
        {
            Panel addrPnl = ctrlAddCustomer.FindControl<Panel>("");
            addrPnl.Visible = false;

            UpdateCustomerPopup();

            extCustomerPanel.Hide();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the result set for the grid, sets the search control properties for pages and results,
        /// then sets the datasource of the grid and if not from a grid event, databinds the grid.
        /// </summary>
        /// <param name="fromNeedDataSource">Boolean value indictating whether the BindData call originated
        /// from the RadGrid itself (true) or from a manual call (false).</param>
        private void BindData(bool useCachedData)
        {
            var results = GetDatasource();

            ctrlSearch.AllCount = results.TotalCount;
            ctrlSearch.StartCount = results.StartIndex;
            ctrlSearch.EndCount = results.EndIndex;
            ctrlSearch.PageCount = results.TotalPages;
            ctrlSearch.CurrentPage = results.CurrentPage;

            RadGrid grdCustomers = ctrlSearch.FindControl<RadGrid>("grdCustomers");

            grdCustomers.DataSource = results;

            if (!useCachedData)
            {
                grdCustomers.DataBind();
            }
            
        }

        /// <summary>
        /// Creates a subset of customers to be used as the current page of the grid, taking into account Registered and Deleted
        /// status, current page, search term, selected alpha filter (if any), and sort column and direction
        /// </summary>
        /// <returns>A PaginatedList of GridCustomer to be used as the datasource of the grid</returns>
        private PaginatedList<GridCustomer> GetDatasource()
        {
            CustomerSearch pagedSearch = new CustomerSearch();

            String filterString = this.CurrentFilter;
            switch (CurrentFilterType)
            {
                case FilterType.Search:
                    pagedSearch.SearchTerm = filterString;
                    break;
                case FilterType.AlphaFilter:
                    if (filterString == "[0-9]")
                    {
                        pagedSearch.AlphaFilter = CustomerSearch.CustomerAlphaFilter.NUMBERS;
                    }
                    else
                    {
                        if (String.IsNullOrEmpty(filterString))
                        {
                            filterString = "None";
                        }
                        pagedSearch.AlphaFilter = (CustomerSearch.CustomerAlphaFilter) Enum.Parse(typeof(CustomerSearch.CustomerAlphaFilter), filterString.ToUpper());
                    }
                    break;
            }

            try
            {
                pagedSearch.SortField = (CustomerSearch.CustomerSortField) Enum.Parse(typeof(CustomerSearch.CustomerSortField), SortColumn);
            }
            catch (Exception)
            {
                pagedSearch.SortField = CustomerSearch.CustomerSortField.CustomerID;
            }

            PaginatedList<GridCustomer> results = pagedSearch.Search(PAGE_SIZE, CurrentPage);

            return results;
        }
        
        /// <summary>
        /// Generic method that takes a GridItem and casts it a specified object or type
        /// </summary>
        /// <typeparam name="T">Generic type to cast a GridItem to</typeparam>
        /// <param name="item">The GridItem being cast</param>
        /// <returns>The GridItem as object or type, T</returns>
        protected T DataItemAs<T>(GridItem item) where T : class
        {
            return item.DataItem as T;
        }

        /// <summary>
        /// Hides a panel intended to be used as a modal popup by setting the display style to "none".  This
        /// should be called from OnInit (prior to PageLoad in the page lifecycle) to prevent the modal panel
        /// from flickering as it loads and then is hidden by the modal popup extender.
        /// </summary>
        /// <param name="pnl">The Panel control of the panel that needs hiding</param>
        private void HideModalPanelByDefault(Panel pnl)
        {
            pnl.Style["display"] = "none";
        }

        /// <summary>
        /// Programmatically creates an ImageButton that is added to the left-side alpha paging control
        /// which when clicked opens the editcustomer control to create a new customer in a modal popup
        /// </summary>
        protected void AddRootAddButton()
        {
            AlphaPaging ap = ctrlSearch.FindControl<AlphaPaging>("ctrlAlphaPaging");

            if (ap.FindControl<ImageButton>("imgbtnAddRoot") == null)
            {
                ImageButton imgbtnAdd = new ImageButton();
                imgbtnAdd.ID = "imgbtnAddRoot";
                imgbtnAdd.ImageUrl = "~/App_Themes/Admin_Default/images/add.png";
                imgbtnAdd.Click += new ImageClickEventHandler(this.imgbtnAdd_Click);
                imgbtnAdd.ToolTip = AppLogic.GetString("admin.common.Add", ThisCustomer.LocaleSetting) + " " + AppLogic.GetString("admin.common.Customer", ThisCustomer.LocaleSetting);
                imgbtnAdd.CommandName = "AddCustomer";

                ap.Controls.Add(imgbtnAdd);
            }
        }

        /// <summary>
        /// Updates the update panel of the edit customer control
        /// </summary>
        protected void UpdateCustomerPopup()
        {
            UpdatePanel upCustomer = ctrlAddCustomer.FindControl<UpdatePanel>("updCustomerEdit");

            if (upCustomer != null)
            {
                upCustomer.Update();
            }
        }

        #endregion
    }
}
