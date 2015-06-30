// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontControls;

namespace AspDotNetStorefrontAdmin.Controls
{
    public partial class AddressEdit : BaseControl
    {
        #region Private variables

        int m_CustomerID;
        private DetailsViewMode m_detailsviewmode = DetailsViewMode.ReadOnly;

        #endregion

        #region Events

        /// <summary>
        /// Default Page_Load event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            sqlAddressList.ConnectionString = DB.GetDBConn();
        }

        public void BindDetails()
        {
            dtlAddressList.ChangeMode(CurrentDetailsViewMode);

            dtlAddressList.DataBind();
        }

        /// <summary>
        /// Event for close button that is ultimately responsible for closing the edit address control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnClosePopup_Click(object sender, EventArgs e)
        {
            // raise close event so parent can hide this panel
            RaiseCloseAddressEvent();
        }

        /// <summary>
        /// Fires when the Country dropdown changes
        /// Used to re-populate the state list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ddlCountry_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            DropDownList CountryDropDown = (DropDownList)dtlAddressList.FindControl("ddlCountry");
            DropDownList StateDropDown = (DropDownList)dtlAddressList.FindControl("ddlState");
            DataRowView thisRow = (DataRowView)dtlAddressList.DataItem;
            List<State> stateList = new List<State>();

            int SelectedCountryID = AppLogic.GetCountryID(CountryDropDown.SelectedItem.Text);

            stateList = State.GetAllStateForCountry(SelectedCountryID, ThisCustomer.LocaleSetting);

            StateDropDown.Items.Clear();

            foreach (State s in stateList)
            {
                StateDropDown.Items.Add(new ListItem(s.Name, s.Abbreviation));
            }

            if (thisRow != null)
            {
                string strSelectedState = thisRow.Row["State"].ToString();

                if (StateDropDown.Items.FindByValue(strSelectedState) != null)
                {
                    StateDropDown.Items.FindByValue(strSelectedState);
                }
            }
        }

        /// <summary>
        /// Event handler for the Make Primary Billing Address button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnMakeBilling_OnClick(object sender, EventArgs e)
        {
            if (dtlAddressList.DataKey.Value != null && Convert.ToInt32(dtlAddressList.DataKey.Value) != 0)
            {
                Customer ThisCustomer = new Customer(m_CustomerID);

                int addressID = Convert.ToInt32(dtlAddressList.DataKey.Value);

                ThisCustomer.SetPrimaryAddress(addressID, AddressTypes.Billing);

                lblPrimaryChanged.Text = AppLogic.GetString("admin.primarybillingchanged", ThisCustomer.LocaleSetting);
                lblPrimaryChanged.Visible = true;

                RaiseDataChangedEvent();
            }           
            
        }

        /// <summary>
        /// Event handler for the Make Primary Shipping Address button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnMakeShipping_OnClick(object sender, EventArgs e)
        {
            if (dtlAddressList.DataKey.Value != null && Convert.ToInt32(dtlAddressList.DataKey.Value) != 0)
            {
                Customer ThisCustomer = new Customer(m_CustomerID);

                int addressID = Convert.ToInt32(dtlAddressList.DataKey.Value);

                ThisCustomer.SetPrimaryAddress(addressID, AddressTypes.Shipping);

                lblPrimaryChanged.Text = AppLogic.GetString("admin.primaryshippingchanged", ThisCustomer.LocaleSetting);
                lblPrimaryChanged.Visible = true;

                RaiseDataChangedEvent();

            }         
        }

        /// <summary>
        /// Event fires when the Address DetailsView finishes changing modes
        /// eg. goes from edit mode to read-only mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void dtlAddressList_OnModeChanged(object sender, EventArgs e)
        {
            btnMakeBilling.Visible = (dtlAddressList.CurrentMode == DetailsViewMode.ReadOnly);
            btnMakeShipping.Visible = (dtlAddressList.CurrentMode == DetailsViewMode.ReadOnly);
        }

        /// <summary>
        /// Handles the Address DetailsView OnDataBound event
        /// </summary>
        /// <param name="Sender"></param>
        /// <param name="e"></param>
        protected void dtlAddressList_OnDataBound(object sender, EventArgs e)
        {
            //Populate the Country and State Select Lists
            if (dtlAddressList.CurrentMode != DetailsViewMode.ReadOnly)
            {
                DropDownList CountryDropDown = (DropDownList)dtlAddressList.FindControl("ddlCountry");
                DropDownList StateDropDown = (DropDownList)dtlAddressList.FindControl("ddlState");
                DataRowView thisRow = (DataRowView)dtlAddressList.DataItem;
                string strSelectedCountry = String.Empty;
                string strSelectedState = String.Empty;

                //Populate the country select list as it is needed to populate the state
                //Make sure that the row contains the dropdown
                if (CountryDropDown != null)
                {
                    List<Country> ctryList = Country.GetAll();

                    foreach (Country c in ctryList)
                    {
                        CountryDropDown.Items.Add(new ListItem(c.Name, c.ID.ToString()));
                    }

                    if (thisRow != null)
                    {
                        strSelectedCountry = thisRow.Row["Country"].ToString();

                        //Check to make sure the value exists in the dropdown before trying to set it as the selected item
                        if (!String.IsNullOrEmpty(strSelectedCountry))
                        {
                            CountryDropDown.Items.FindByText(strSelectedCountry).Selected = true;
                        }
                    }
                    else
                    {
                        strSelectedCountry = CountryDropDown.Items[0].Text;
                    }
                }

                //Repeat to populate the state dropdown
                if (StateDropDown != null)
                {
                    int CountryID = AppLogic.GetCountryID(strSelectedCountry);

                    List<State> stateList = State.GetAllStateForCountry(CountryID, ThisCustomer.LocaleSetting);

                    foreach (State s in stateList)
                    {
                        StateDropDown.Items.Add(new ListItem(s.Name, s.Abbreviation));
                    }

                    if (thisRow != null)
                    {
                        strSelectedState = thisRow.Row["State"].ToString();

                        if (!String.IsNullOrEmpty(strSelectedState) && StateDropDown.Items.FindByValue(strSelectedState) != null)
                        {
                            StateDropDown.Items.FindByValue(strSelectedState).Selected = true;
                        }
                    }

                }

                //Populate the Residence Type Dropdown
                DropDownList ddlResidenceType = (DropDownList)dtlAddressList.FindControl("ddlResidenceType");

                if (ddlResidenceType != null)
                {
                    ddlResidenceType.DataSource = Enum.GetNames(typeof(ResidenceTypes));
                    ddlResidenceType.DataBind();

                    if (thisRow != null)
                    {
                        int SelectedResidenceType = Convert.ToInt16(thisRow.Row["ResidenceType"]);

                        if (ddlResidenceType.Items[SelectedResidenceType] != null)
                        {
                            ddlResidenceType.Items[SelectedResidenceType].Selected = true;
                        }
                    }
                }

            }
            else
            {
                DataRowView thisRow = (DataRowView)dtlAddressList.DataItem;

                if (thisRow != null)
                {
                    Label lblRes = (Label)dtlAddressList.FindControl("lblResidenceType");
                    int rType = 0;

                    if (lblRes != null)
                    {
                        try
                        {
                            rType = Convert.ToInt16(thisRow.Row["ResidenceType"]);
                        }
                        catch { }

                        lblRes.Text = Enum.GetName(typeof(ResidenceTypes), rType);
                    }
                }
            }

        }

        /// <summary>
        /// Event fires after an item is inserted through the details view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void dtlAddressList_OnInserted(Object sender, DetailsViewInsertedEventArgs e)
        {
            RaiseDataChangedEvent();

        }

        /// <summary>
        /// Event fires after an item is updated through the details view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void dtlAddressList_OnUpdated(Object sender, DetailsViewUpdatedEventArgs e)
        {
            RaiseDataChangedEvent();
        }

        /// <summary>
        /// Event fires after an item is deleted through the details view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void dtlAddressList_OnDeleted(Object sender, DetailsViewDeletedEventArgs e)
        {
            RaiseDataChangedEvent();
        }

        /// <summary>
        /// Handles the Address DetailsView OnItemInserting Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void dtlAddressList_OnItemInserting(object sender, DetailsViewInsertEventArgs e)
        {
            sqlAddressList.InsertParameters.Add(new Parameter("CustomerID", System.Data.DbType.Int32, m_CustomerID.ToString()));

            DropDownList CountryDropDown = (DropDownList)dtlAddressList.FindControl("ddlCountry");
            string SelectedCountry = CountryDropDown.SelectedItem.Text;

            DropDownList StateDropDown = (DropDownList)dtlAddressList.FindControl("ddlState");
            string SelectedState = StateDropDown.SelectedValue;

            DropDownList ResidenceDropDown = (DropDownList)dtlAddressList.FindControl("ddlResidenceType");
            int SelectedResidenceType = ResidenceDropDown.SelectedIndex;

            sqlAddressList.InsertParameters.Add(new Parameter("Country", System.Data.DbType.String, SelectedCountry));
            sqlAddressList.InsertParameters.Add(new Parameter("State", System.Data.DbType.String, SelectedState));
            sqlAddressList.InsertParameters.Add(new Parameter("ResidenceType", System.Data.DbType.Int16, SelectedResidenceType.ToString()));
        }

        /// <summary>
        /// Handles the Address DetailsView OnItemUpdating Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void dtlAddressList_OnItemUpdating(object sender, DetailsViewUpdateEventArgs e)
        {
            DropDownList CountryDropDown = (DropDownList)dtlAddressList.FindControl("ddlCountry");
            string SelectedCountry = CountryDropDown.SelectedItem.Text;

            DropDownList StateDropDown = (DropDownList)dtlAddressList.FindControl("ddlState");
            string SelectedState = StateDropDown.SelectedValue;

            DropDownList ResidenceDropDown = (DropDownList)dtlAddressList.FindControl("ddlResidenceType");
            int SelectedResidenceType = ResidenceDropDown.SelectedIndex;

            sqlAddressList.UpdateParameters.Add(new Parameter("Country", System.Data.DbType.String, SelectedCountry));
            sqlAddressList.UpdateParameters.Add(new Parameter("State", System.Data.DbType.String, SelectedState));
            sqlAddressList.UpdateParameters.Add(new Parameter("ResidenceType", System.Data.DbType.Int16, SelectedResidenceType.ToString()));

        }

        /// <summary>
        /// Handles the select event for the SQL Datasource object
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void sqlAddressList_OnSelecting(object sender, SqlDataSourceCommandEventArgs e)
        {
            e.Command.Parameters["@CustomerID"].Value = m_CustomerID.ToString();
        }


        #endregion

        #region Public properties

        /// <summary>
        /// Sets the customer ID addresses are to be loaded for
        /// </summary>
        public int CustomerID
        {
            get { return m_CustomerID; }
            set { m_CustomerID = value; }
        }

        public DetailsViewMode CurrentDetailsViewMode
        {
            get { return m_detailsviewmode; }
            set { m_detailsviewmode = value; }
        }

        #endregion

        #region Methods

        private void RaiseDataChangedEvent()
        {
            if (this.DataChanged != null)
            {
                this.DataChanged(this, new EventArgs());
            }
        }

        private void RaiseCloseAddressEvent()
        {
            this.CloseAddress(this, new EventArgs());
        }

        #endregion

        #region EventHandlers

        public event EventHandler DataChanged;
		public event EventHandler CloseAddress;

        #endregion

    }
}
