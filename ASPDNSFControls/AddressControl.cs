// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.IO;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontCore.Validation;


namespace AspDotNetStorefrontControls
{
    /// <summary>
    /// Custom control used to get or return address
    /// </summary>
    public class AddressControl : CompositeControl, IPostBackDataHandler
    {
        #region Variable Declaration

        protected const string ALIGN_ATTRIBUTE = "align";
        protected const string ALIGN_RIGHT = "right";
        protected const string ALIGN_LEFT = "left";

        private const string APPEARANCE_CATEGORY = "Address Appearance";
        private const string CAPTION_WIDTH = "CaptionWidth";
        private const string INPUT_WIDTH = "InputWidth";
        private const string STATE_TEMP = "StateTemporary";
        private const string TABLE_STYLE = "Table Style";
        private const string TABLE_CSS_CLASS = "TableClass";
        private const string SHOW_FIRST_NAME = "Show First Name";
        private const string SHOW_LAST_NAME = "Show Last Name";
        private const string SHOW_NICK_NAME = "Show NickName";
        private const string SHOW_ADDRESS1 = "ShowAddress1";
        private const string SHOW_ADDRESS2 = "ShowAddress2";
        private const string SHOW_PHONE_NUMBER = "ShowPhoneNumber";
        private const string SHOW_COMPANY = "ShowCompany";
        private const string SHOW_SUITE = "ShowSuite";
        private const string REGISTER_COUNTRIES = "Register Countries";
        private const string SHOW_RESIDENCE_TYPE = "ShowResidenceType";
        private const string SHOW_CITY = "ShowCity";
        private const string SHOW_COUNTRY = "ShowCountry";
        private const string SHOW_STATE = "ShowState";
        private const string SHOW_ZIP = "ShowZip";

        // ERROR messages...
        protected const string VALIDATORS_CATEGORY = "Validators Category";
        protected const string VALIDATION_GROUP = "Validation Group";
        private const string REQUIRED_FIRST_NAME_ERROR_MESSAGE = "RequiredFirstNameErrorMessage";
        private const string REQUIRED_LAST_NAME_ERROR_MESSAGE = "RequiredLastNameErrorMessage";
        private const string REQUIRED_ACCOUNT_NAME_ERROR_MESSAGE = "RequiredAccountNameErrorMessage";
        private const string REQUIRED_ADDRESS1_ERROR_MESSAGE = "RequiredAddress1ErrorMessage";
        private const string REQUIRED_PHONE_ERROR_MESSAGE = "RequiredPhoneErrorMessage";
        private const string VALIDATION_PHONE_ERROR_MESSAGE = "RequiredPhoneErrorMessage";
        private const string VALIDATION_ADDRESS1_ERROR_MESSAGE = "ValidationAddress1Message";
        private const string VALIDATION_ZIPCODE_ERROR_MESSAGE = "ValidationZipCodeMessage";

        private const string FIRST_NAME_MAXIMUM_CHARACTER_LENGTH = "FirstNameMaximumCharacterLength";
        private const string LAST_NAME_MAXIMUM_CHARACTER_LENGTH = "LastNameMaximumCharacterLength";
        private const string ACCOUNT_NAME_MAXIMUM_CHARACTER_LENGTH = "AccountNameMaximumCharacterLength";
        private const string ADDRESS_MAXIMUM_CHARACTER_LENGTH = "AddressMaximumCharacterLength";
        private const string PHONE_MAXIMUM_CHARACTER_LENGTH = "PhoneMaximumCharacterLength";

        private const string FIRST_NAME_MAXIMUM_CHARACTER_LENGTH_ERROR_MESSAGE = "FirstNameMaximumCharacterLengthErrorMessage";
        private const string LAST_NAME_MAXIMUM_CHARACTER_LENGTH_ERROR_MESSAGE = "LastNameMaximumCharacterLengthErrorMessage";
        private const string ACCOUNT_NAME_MAXIMUM_CHARACTER_LENGTH_ERROR_MESSAGE = "AccountNameMaximumCharacterLengthErrorMessage";
        private const string ADDRESS_MAXIMUM_CHARACTER_LENGTH_ERROR_MESSAGE = "AddressMaximumCharacterLengthErrorMessage";
        private const string PHONE_MAXIMUM_CHARACTER_LENGTH_ERROR_MESSAGE = "PhoneMaximumCharacterLengthErrorMessage";

        private const string CITY_REQUIRED_ERROR_MESSAGE = "CityRequiredErrorMessage";
        private const string CITY_MAXIMUM_CHARACTER_LENGTH = "CityMaximumCharacterLength";
        private const string CITY_MAXIMUM_CHARACTER_LENGTH_ERROR_MESSAGE = "CityMaximumCharacterLengthErrorMessage";

        private const string ZIP_REQUIRED_ERROR_MESSAGE = "ZipRequiredErrorMessage";

        private const string POSTAL_CODE_REQUIRED_ERROR_MESSAGE = "PostalCodeRequiredErrorMessage";
        private const string POSTAL_CODE_MAXIMUM_CHARACTER_LENGTH = "PostalCodeMaximumCharacterLength";
        private const string POSTAL_CODE_MAXIMUM_CHARACTER_LENGTH_ERROR_MESSAGE = "PostalCodeMaximumCharacterLengthErrorMessage";

        private const string POBOX_ADDRESS_NOT_ALLOWED_ERROR_MESSAGE = "POBoxAddressNotAllowedMessage";

        private const string ENABLE_VALIDATION = "EnableValidation";

        // -------------------------->


        private Label _lblFirstName = new Label();
        private TextBox _txtFirstName = new TextBox();
        private Label _lblLastName = new Label();
        private TextBox _txtLastName = new TextBox();
        private Label _lblNickName = new Label();
        private TextBox _txtNickName = new TextBox();
        private Label _lblAddress1Caption = new Label();
        private TextBox _txtAddress1 = new TextBox();
        private Label _lblAddress2Caption = new Label();
        private TextBox _txtAddress2 = new TextBox();
        private Label _lblResidenceType = new Label();
        private DropDownList _cboResidenceType = new DropDownList();
        private Label _lblCityCaption = new Label();
        private TextBox _txtCity = new TextBox();
        private Label _lblCountryCaption = new Label();
        private DropDownList _cboCountry = new DropDownList();
        private Label _lblStateCaption = new Label();
        private DropDownList _cboState = new DropDownList();
        private Label _lblZipCaption = new Label();
        private TextBox _txtZip = new TextBox();
        private Label _lblCompanyCaption = new Label();
        private TextBox _txtCompany = new TextBox();
        private Label _lblSuiteCaption = new Label();
        private TextBox _txtSuite = new TextBox();
        private Label _lblPhoneNumberCaption = new Label();
        private TextBox _txtPhoneNumber = new TextBox();

        private readonly string RESIDENCE_TYPE_RESIDENTIAL = "Residential";
        private readonly string RESIDENCE_TYPE_COMMERCIAL = "Commercial";

        private RequiredFieldValidator _rfvFirstName = new RequiredFieldValidator();
        private RequiredFieldValidator _rfvLastName = new RequiredFieldValidator();
        private RequiredFieldValidator _rfvPhone = new RequiredFieldValidator();
        private RequiredFieldValidator _rfvAddress1 = new RequiredFieldValidator();
        private RequiredFieldValidator _rfvCity = new RequiredFieldValidator();
        private RequiredFieldValidator _rfvZip = new RequiredFieldValidator();

        private Validators.AddressValidator _cvAddress1;
        private Validators.ZipCodeValidator _cvZipCode = new AspDotNetStorefrontControls.Validators.ZipCodeValidator();

        private RegularExpressionValidator _regxPhone = new RegularExpressionValidator();

        private bool _showValidatorsInline = false;
        private bool _allowEdit = true;

        #endregion

        #region Events

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="AddressControl"/> class.
        /// </summary>
        public AddressControl()
        {
            _cvAddress1 = new AspDotNetStorefrontControls.Validators.AddressValidator();
            AssignClientReferenceIds();
            SetDefaults();
            SetBehaviors();
            SetTextSizes();

        }

        /// <summary>
        /// Assigns the client reference ids.
        /// </summary>
        private void AssignClientReferenceIds()
        {
            _txtNickName.ID = "NickName";
            _txtFirstName.ID = "FirstName";
            _txtLastName.ID = "LastName";
            _cboResidenceType.ID = "ResidenceType";
            _cboCountry.ID = "Country";
            _txtAddress1.ID = "Address1";
            _txtAddress2.ID = "Address2";
            _txtCity.ID = "City";
            _cboState.ID = "State";
            _cboCountry.ID = "Country";
            _txtZip.ID = "Zip";
            _txtCompany.ID = "Company";
            _txtSuite.ID = "Suite";
            _txtPhoneNumber.ID = "Phone";

            _txtNickName.CssClass = "form-control";
            _txtFirstName.CssClass = "form-control";
            _txtLastName.CssClass = "form-control";
            _cboResidenceType.CssClass = "form-control";
            _cboCountry.CssClass = "form-control";
            _txtAddress1.CssClass = "form-control";
            _txtAddress2.CssClass = "form-control";
            _txtCity.CssClass = "form-control";
            _cboState.CssClass = "form-control";
            _cboCountry.CssClass = "form-control";
            _txtZip.CssClass = "form-control";
            _txtCompany.CssClass = "form-control";
            _txtSuite.CssClass = "form-control";
            _txtPhoneNumber.CssClass = "form-control";
        }

        /// <summary>
        /// Sets the country dropdown behaviors.
        /// </summary>
        private void SetBehaviors()
        {
            // make it automatically postback whenever the user changes the selected item
            _cboCountry.AutoPostBack = true;
            _cboCountry.SelectedIndexChanged += new EventHandler(cboCountry_SelectedIndexChanged);
        }

        /// <summary>
        /// Sets the textbox sizes.
        /// </summary>
        private void SetTextSizes()
        {
            _txtFirstName.Columns = 20;
            _txtFirstName.MaxLength = 100;

            _txtLastName.Columns = 20;
            _txtLastName.MaxLength = 100;

            _txtPhoneNumber.Columns = 20;
            _txtPhoneNumber.MaxLength = 25;

            _txtCompany.Columns = 34;
            _txtCompany.MaxLength = 100;

            _txtAddress1.Columns = 25;
            _txtAddress1.MaxLength = 100;

            _txtAddress2.Columns = 25;
            _txtAddress2.MaxLength = 100;

            _txtCity.Columns = 34;
            _txtCity.MaxLength = 50;

            _txtSuite.Columns = 20;
            _txtSuite.MaxLength = 50;

            _txtZip.Columns = 14;
            _txtZip.MaxLength = 10;
        }

        /// <summary>
        /// Gets or sets the country data source.
        /// </summary>
        /// <value>The country data source.</value>
        public IEnumerable CountryDataSource
        {
            get { return _cboCountry.DataSource as IEnumerable; }
            set
            {
                _cboCountry.DataSource = value;
                _cboCountry.DataBind();
            }
        }

        private List<State> _stateDataSource = null;

        /// <summary>
        /// Gets or sets the state data source.
        /// </summary>
        /// <value>The state data source.</value>
        public List<State> StateDataSource
        {
            get { return _stateDataSource; }
            set
            {

                _cboState.Items.Clear();
                foreach (State s in value)
                {
                    _cboState.Items.Add(new ListItem(s.Name, s.Abbreviation));
                }
                ChildControlsCreated = false;
            }
        }

        public event EventHandler SelectedCountryChanged;

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cboCountry control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void cboCountry_SelectedIndexChanged(object sender, EventArgs e)
        {
            OnSelectedCountryChanged(e);
        }

        /// <summary>
        /// Raises the <see cref="E:SelectedCountryChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnSelectedCountryChanged(EventArgs e)
        {
            if (SelectedCountryChanged != null)
            {
                SelectedCountryChanged(this, e);
            }
            base.RaiseBubbleEvent(this, e);
        }

        /// <summary>
        /// Sets the controls default.
        /// </summary>
        private void SetDefaults()
        {
            Address1Caption = "{Address1:} ";
            Address2Caption = "{Address2:} ";
            ResidenceTypeCaption = "{Residence Type:} ";
            CityCaption = "{City:} ";
            StateCaption = "{State:} ";
            ZipCaption = "{Zip:} ";
            CountryCaption = "{Country:} ";
            FirstNameCaption = "{FirstName} ";
            LastNameCaption = "{Last Name} ";
            NickNameCaption = "{Nick Name:} ";
            PhoneNumberCaption = "{Phone Number:} ";
            CompanyCaption = "{Company:} ";
            SuiteCaption = "{Suite:} ";

            ShowCity = true;
            ShowState = true;
            ShowZip = true;
            ShowCountry = true;
            ShowFirstName = true;
            ShowLastName = true;
            ShowPhoneNumber = true;

            _cboResidenceType.Items.Add(new ListItem(RESIDENCE_TYPE_RESIDENTIAL));
            _cboResidenceType.Items.Add(new ListItem(RESIDENCE_TYPE_COMMERCIAL));

            _rfvFirstName.ControlToValidate = _txtFirstName.ID;
            _rfvLastName.ControlToValidate = _txtLastName.ID;
            _rfvPhone.ControlToValidate = _txtPhoneNumber.ID;
            _regxPhone.ControlToValidate = _txtPhoneNumber.ID;
            _rfvAddress1.ControlToValidate = _txtAddress1.ID;
            _rfvCity.ControlToValidate = _txtCity.ID;
            _rfvZip.ControlToValidate = _txtZip.ID;
            _cvAddress1.ControlToValidate = _txtAddress1.ID;
            _cvZipCode.ControlToValidate = _txtZip.ID;

            _rfvFirstName.Display = ValidatorDisplay.Dynamic;
            _rfvLastName.Display = ValidatorDisplay.Dynamic;
            _rfvPhone.Display = ValidatorDisplay.Dynamic;
            _regxPhone.Display = ValidatorDisplay.Dynamic;
            _rfvAddress1.Display = ValidatorDisplay.Dynamic;
            _rfvCity.Display = ValidatorDisplay.Dynamic;
            _rfvZip.Display = ValidatorDisplay.Dynamic;
            _cvAddress1.Display = ValidatorDisplay.Dynamic;
            _cvZipCode.Display = ValidatorDisplay.Dynamic;

            _regxPhone.ValidationExpression = new PhoneValidator().GetValidationRegExString();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the first name caption.
        /// </summary>
        /// <value>The first name caption.</value>
        [Browsable(true), Category(APPEARANCE_CATEGORY)]
        public string FirstNameCaption
        {
            get { return _lblFirstName.Text; }
            set { _lblFirstName.Text = value; }
        }

        /// <summary>
        /// Gets or sets the last name caption.
        /// </summary>
        /// <value>The last name caption.</value>
        [Browsable(true), Category(APPEARANCE_CATEGORY)]
        public string LastNameCaption
        {
            get { return _lblLastName.Text; }
            set { _lblLastName.Text = value; }
        }

        /// <summary>
        /// Gets or sets the nick name caption.
        /// </summary>
        /// <value>The nick name caption.</value>
        [Browsable(true), Category(APPEARANCE_CATEGORY)]
        public string NickNameCaption
        {
            get { return _lblNickName.Text; }
            set { _lblNickName.Text = value; }
        }

        /// <summary>
        /// Gets or sets the address type caption.
        /// </summary>
        /// <value>The address type caption.</value>
        [Browsable(true), Category(APPEARANCE_CATEGORY)]
        public string ResidenceTypeCaption
        {
            get { return _lblResidenceType.Text; }
            set { _lblResidenceType.Text = value; }
        }

        /// <summary>
        /// Gets or sets the phone number caption.
        /// </summary>
        /// <value>The phone number caption.</value>
        [Browsable(true), Category(APPEARANCE_CATEGORY)]
        public string PhoneNumberCaption
        {
            get { return _lblPhoneNumberCaption.Text; }
            set { _lblPhoneNumberCaption.Text = value; }
        }

        /// <summary>
        /// Gets or sets the company caption.
        /// </summary>
        /// <value>The company caption.</value>
        [Browsable(true), Category(APPEARANCE_CATEGORY)]
        public string CompanyCaption
        {
            get { return _lblCompanyCaption.Text; }
            set { _lblCompanyCaption.Text = value; }
        }

        /// <summary>
        /// Gets or sets the suite caption.
        /// </summary>
        /// <value>The suite caption.</value>
        [Browsable(true), Category(APPEARANCE_CATEGORY)]
        public string SuiteCaption
        {
            get { return _lblSuiteCaption.Text; }
            set { _lblSuiteCaption.Text = value; }
        }

        /// <summary>
        /// Gets or sets the city caption.
        /// </summary>
        /// <value>The city caption.</value>
        [Browsable(true), Category(APPEARANCE_CATEGORY)]
        public string CityCaption
        {
            get { return _lblCityCaption.Text; }
            set { _lblCityCaption.Text = value; }
        }

        /// <summary>
        /// Gets or sets the state caption.
        /// </summary>
        /// <value>The state caption.</value>
        [Browsable(true), Category(APPEARANCE_CATEGORY)]
        public string StateCaption
        {
            get { return _lblStateCaption.Text; }
            set { _lblStateCaption.Text = value; }
        }

        /// <summary>
        /// Gets or sets the zip caption.
        /// </summary>
        /// <value>The zip caption.</value>
        [Browsable(true), Category(APPEARANCE_CATEGORY)]
        public string ZipCaption
        {
            get { return _lblZipCaption.Text; }
            set { _lblZipCaption.Text = value; }
        }

        /// <summary>
        /// Gets or sets the country caption.
        /// </summary>
        /// <value>The country caption.</value>
        [Browsable(true), Category(APPEARANCE_CATEGORY)]
        public string CountryCaption
        {
            get { return _lblCountryCaption.Text; }
            set { _lblCountryCaption.Text = value; }
        }

        /// <summary>
        /// Gets or sets the address1 caption.
        /// </summary>
        /// <value>The address1 caption.</value>
        [Browsable(true), Category(APPEARANCE_CATEGORY)]
        public string Address1Caption
        {
            get { return _lblAddress1Caption.Text; }
            set { _lblAddress1Caption.Text = value; }
        }

        /// <summary>
        /// Gets or sets the address2 caption.
        /// </summary>
        /// <value>The address2 caption.</value>
        [Browsable(true), Category(APPEARANCE_CATEGORY)]
        public string Address2Caption
        {
            get { return _lblAddress2Caption.Text; }
            set { _lblAddress2Caption.Text = value; }
        }

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        /// <value>The first name.</value>
        [Browsable(false)]
        public string FirstName
        {
            get { return _txtFirstName.Text; }
            set { _txtFirstName.Text = value; }
        }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        /// <value>The last name.</value>
        [Browsable(false)]
        public string LastName
        {
            get { return _txtLastName.Text; }
            set { _txtLastName.Text = value; }
        }

        /// <summary>
        /// Gets or sets the nickname.
        /// </summary>
        /// <value>The nickname.</value>
        [Browsable(false)]
        public string NickName
        {
            get { return _txtNickName.Text; }
            set { _txtNickName.Text = value; }
        }

        /// <summary>
        /// Gets or sets the country selected value.
        /// </summary>
        /// <value>The country.</value>
        [Browsable(false)]
        public string Country
        {
            get
            {
                if (null == _cboCountry.SelectedItem) { return string.Empty; }
                return _cboCountry.SelectedValue;
            }
            set
            {
                try
                {
                    _cboCountry.SelectedValue = value;
                }
                catch
                {
                    _cboCountry.SelectedValue = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the country data text field.
        /// </summary>
        /// <value>The country data text field.</value>
        [Browsable(false)]
        public string CountryDataTextField
        {
            get
            {
                return _cboCountry.DataTextField;
            }
            set
            {
                _cboCountry.DataTextField = value;
                _cboCountry.DataBind();
            }

        }

        /// <summary>
        /// Gets or sets the country data value field.
        /// </summary>
        /// <value>The country data value field.</value>
        [Browsable(false)]
        public string CountryDataValueField
        {
            get
            {
                return _cboCountry.DataValueField;
            }
            set
            {
                _cboCountry.DataValueField = value;
                _cboCountry.DataBind();
            }
        }

        /// <summary>
        /// Gets or sets the company.
        /// </summary>
        /// <value>The company.</value>
        [Browsable(false)]
        public string Company
        {
            get { return _txtCompany.Text; }
            set { _txtCompany.Text = value; }
        }

        /// <summary>
        /// Gets or sets the address1.
        /// </summary>
        /// <value>The address1.</value>
        [Browsable(false)]
        public string Address1
        {
            get { return _txtAddress1.Text; }
            set { _txtAddress1.Text = value; }
        }

        /// <summary>
        /// Gets or sets the address2.
        /// </summary>
        /// <value>The address2.</value>
        [Browsable(false)]
        public string Address2
        {
            get { return _txtAddress2.Text; }
            set { _txtAddress2.Text = value; }
        }

        /// <summary>
        /// Gets or sets the type of the address.
        /// </summary>
        /// <value>The type of the address.</value>
        [Browsable(false)]
        public string ResidenceType
        {
            get
            {
                return _cboResidenceType.SelectedValue;
            }
            set
            {
                _cboResidenceType.SelectedValue = value;
            }
        }

        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        /// <value>The city.</value>
        [Browsable(false)]
        public string City
        {
            get
            {
                return _txtCity.Text;
            }
            set
            {
                _txtCity.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the suite.
        /// </summary>
        /// <value>The suite.</value>
        [Browsable(false)]
        public string Suite
        {
            get
            {
                return _txtSuite.Text;
            }
            set
            {
                _txtSuite.Text = value;
            }
        }

        [Browsable(false)]
        public string State
        {
            get
            {
                return _cboState.SelectedValue;
            }
            set
            {
                _cboState.SelectedValue = value;
            }
        }

        /// <summary>
        /// Gets or sets the state data text field.
        /// </summary>
        /// <value>The state data text field.</value>
        [Browsable(false)]
        public string StateDataTextField
        {
            get
            {
                return _cboState.DataTextField;
            }
            set
            {
                _cboState.DataTextField = value;
                _cboState.DataBind();
            }

        }

        /// <summary>
        /// Gets or sets the state data value field.
        /// </summary>
        /// <value>The state data value field.</value>
        [Browsable(false)]
        public string StateDataValueField
        {
            get
            {
                return _cboState.DataValueField;
            }
            set
            {
                _cboState.DataValueField = value;
                _cboState.DataBind();
            }
        }

        /// <summary>
        /// Gets or sets the zip code.
        /// </summary>
        /// <value>The zip code.</value>
        [Browsable(false)]
        public string ZipCode
        {
            get
            {
                return _txtZip.Text;
            }
            set
            {
                _txtZip.Text = value;
            }
        }


        /// <summary>
        /// Gets or sets the phone number.
        /// </summary>
        /// <value>The phone number.</value>
        [Browsable(false)]
        public string PhoneNumber
        {
            get { return _txtPhoneNumber.Text; }
            set { _txtPhoneNumber.Text = value; }
        }

        /// <summary>
        /// Gets or sets the table class.
        /// </summary>
        /// <value>The table class.</value>
        [Browsable(true), Category(APPEARANCE_CATEGORY)]
        public string TableClass
        {
            get { return null == ViewState[TABLE_CSS_CLASS] ? string.Empty : (string)ViewState[TABLE_CSS_CLASS]; }
            set { ViewState[TABLE_CSS_CLASS] = value; }
        }

        /// <summary>
        /// Gets or sets the table style.
        /// </summary>
        /// <value>The table style.</value>
        [Browsable(true), Category(APPEARANCE_CATEGORY)]
        public string TableStyle
        {
            get { return null == ViewState[TABLE_STYLE] ? string.Empty : (string)ViewState[TABLE_STYLE]; }
            set { ViewState[TABLE_STYLE] = value; }
        }


        /// <summary>
        /// Gets or sets a value indicating whether [show first name].
        /// </summary>
        /// <value><c>true</c> if [show first name]; otherwise, <c>false</c>.</value>
        [Browsable(true), Category(APPEARANCE_CATEGORY)]
        public bool ShowFirstName
        {
            get
            {
                object booleanValue = ViewState[SHOW_FIRST_NAME];
                if (null == booleanValue) { return true; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState[SHOW_FIRST_NAME] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show last name].
        /// </summary>
        /// <value><c>true</c> if [show last name]; otherwise, <c>false</c>.</value>
        [Browsable(true), Category(APPEARANCE_CATEGORY)]
        public bool ShowLastName
        {
            get
            {
                object booleanValue = ViewState[SHOW_LAST_NAME];
                if (null == booleanValue) { return true; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState[SHOW_LAST_NAME] = value;

                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show nick name].
        /// </summary>
        /// <value><c>true</c> if [show nick name]; otherwise, <c>false</c>.</value>
        [Browsable(true), Category(APPEARANCE_CATEGORY)]
        public bool ShowNickName
        {
            get
            {
                object booleanValue = ViewState[SHOW_NICK_NAME];
                if (null == booleanValue) { return true; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState[SHOW_NICK_NAME] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show address1].
        /// </summary>
        /// <value><c>true</c> if [show address1]; otherwise, <c>false</c>.</value>
        [Browsable(true), Category(APPEARANCE_CATEGORY)]
        public bool ShowAddress1
        {
            get
            {
                object booleanValue = ViewState[SHOW_ADDRESS1];
                if (null == booleanValue) { return true; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState[SHOW_ADDRESS1] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show address2].
        /// </summary>
        /// <value><c>true</c> if [show address2]; otherwise, <c>false</c>.</value>
        [Browsable(true), Category(APPEARANCE_CATEGORY)]
        public bool ShowAddress2
        {
            get
            {
                object booleanValue = ViewState[SHOW_ADDRESS2];
                if (null == booleanValue) { return true; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState[SHOW_ADDRESS2] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show address type].
        /// </summary>
        /// <value><c>true</c> if [show address type]; otherwise, <c>false</c>.</value>
        [Browsable(true), Category(APPEARANCE_CATEGORY)]
        public bool ShowResidenceType
        {
            get
            {
                object booleanValue = ViewState[SHOW_RESIDENCE_TYPE];
                if (null == booleanValue) { return true; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState[SHOW_RESIDENCE_TYPE] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show phone number].
        /// </summary>
        /// <value><c>true</c> if [show phone number]; otherwise, <c>false</c>.</value>
        [Browsable(true), Category(APPEARANCE_CATEGORY)]
        public bool ShowPhoneNumber
        {
            get
            {
                object booleanValue = ViewState[SHOW_PHONE_NUMBER];
                if (null == booleanValue) { return true; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState[SHOW_PHONE_NUMBER] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show company].
        /// </summary>
        /// <value><c>true</c> if [show company]; otherwise, <c>false</c>.</value>
        [Browsable(true), Category(APPEARANCE_CATEGORY)]
        public bool ShowCompany
        {
            get
            {
                object booleanValue = ViewState[SHOW_COMPANY];
                if (null == booleanValue) { return true; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState[SHOW_COMPANY] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show suite].
        /// </summary>
        /// <value><c>true</c> if [show suite]; otherwise, <c>false</c>.</value>
        [Browsable(true), Category(APPEARANCE_CATEGORY)]
        public bool ShowSuite
        {
            get
            {
                object booleanValue = ViewState[SHOW_SUITE];
                if (null == booleanValue) { return true; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState[SHOW_SUITE] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show city].
        /// </summary>
        /// <value><c>true</c> if [show city]; otherwise, <c>false</c>.</value>
        [Browsable(true), Category(APPEARANCE_CATEGORY)]
        public bool ShowCity
        {
            get
            {
                object booleanValue = ViewState[SHOW_CITY];
                if (null == booleanValue) { return true; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState[SHOW_CITY] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show country].
        /// </summary>
        /// <value><c>true</c> if [show country]; otherwise, <c>false</c>.</value>
        [Browsable(true), Category(APPEARANCE_CATEGORY)]
        public bool ShowCountry
        {
            get
            {
                object booleanValue = ViewState[SHOW_COUNTRY];
                if (null == booleanValue) { return true; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState[SHOW_COUNTRY] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show state].
        /// </summary>
        /// <value><c>true</c> if [show state]; otherwise, <c>false</c>.</value>
        [Browsable(true), Category(APPEARANCE_CATEGORY)]
        public bool ShowState
        {
            get
            {
                object booleanValue = ViewState[SHOW_STATE];
                if (null == booleanValue) { return true; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState[SHOW_STATE] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show zip].
        /// </summary>
        /// <value><c>true</c> if [show zip]; otherwise, <c>false</c>.</value>
        [Browsable(true), Category(APPEARANCE_CATEGORY)]
        public bool ShowZip
        {
            get
            {
                object booleanValue = ViewState[SHOW_ZIP];
                if (null == booleanValue) { return true; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState[SHOW_ZIP] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// Gets or sets the first name required error message.
        /// </summary>
        /// <value>The first name required error message.</value>
        [Browsable(true), Category(VALIDATORS_CATEGORY)]
        public string FirstNameRequiredErrorMessage
        {
            get
            {
                object savedValue = ViewState[REQUIRED_FIRST_NAME_ERROR_MESSAGE];
                if (null == savedValue) { return string.Empty; }

                return savedValue.ToString();
            }
            set
            {
                ViewState[REQUIRED_FIRST_NAME_ERROR_MESSAGE] = value;
                _rfvFirstName.ErrorMessage = value;
            }
        }


        /// <summary>
        /// Gets or sets the last name required error message.
        /// </summary>
        /// <value>The last name required error message.</value>
        [Browsable(true), Category(VALIDATORS_CATEGORY)]
        public string LastNameRequiredErrorMessage
        {
            get
            {
                object savedValue = ViewState[REQUIRED_LAST_NAME_ERROR_MESSAGE];
                if (null == savedValue) { return string.Empty; }

                return savedValue.ToString();
            }
            set
            {
                ViewState[REQUIRED_LAST_NAME_ERROR_MESSAGE] = value;
                _rfvLastName.ErrorMessage = value;
            }
        }

        /// <summary>
        /// Gets or sets the account name required error message.
        /// </summary>
        /// <value>The account name required error message.</value>
        [Browsable(true), Category(VALIDATORS_CATEGORY)]
        public string AccountNameRequiredErrorMessage
        {
            get
            {
                object savedValue = ViewState[REQUIRED_ACCOUNT_NAME_ERROR_MESSAGE];
                if (null == savedValue) { return string.Empty; }

                return savedValue.ToString();
            }
            set
            {
                ViewState[REQUIRED_ACCOUNT_NAME_ERROR_MESSAGE] = value;
            }
        }

        /// <summary>
        /// Gets or sets the address1 required error message.
        /// </summary>
        /// <value>The address1 required error message.</value>
        [Browsable(true), Category(VALIDATORS_CATEGORY)]
        public string Address1RequiredErrorMessage
        {
            get
            {
                object savedValue = ViewState[REQUIRED_ADDRESS1_ERROR_MESSAGE];
                if (null == savedValue) { return string.Empty; }

                return savedValue.ToString();
            }
            set
            {
                ViewState[REQUIRED_ADDRESS1_ERROR_MESSAGE] = value;
                _rfvAddress1.ErrorMessage = value;
            }
        }

        /// <summary>
        /// Gets or sets the address1 validation error message.
        /// </summary>
        /// <value>The address1 validation error message.</value>
        [Browsable(true), Category(VALIDATORS_CATEGORY)]
        public string Address1ValidationErrorMessage
        {
            get
            {
                object savedValue = ViewState[VALIDATION_ADDRESS1_ERROR_MESSAGE];
                if (null == savedValue) { return string.Empty; }

                return savedValue.ToString();
            }
            set
            {
                ViewState[VALIDATION_ADDRESS1_ERROR_MESSAGE] = value;
                _cvAddress1.ErrorMessage = value;
            }
        }


        /// <summary>
        /// Gets or sets the Zip Code validation error message.
        /// </summary>
        /// <value>The Zip Code validation error message.</value>
        [Browsable(true), Category(VALIDATORS_CATEGORY)]
        public string ZipCodeValidationErrorMessage
        {
            get
            {
                object savedValue = ViewState[VALIDATION_ZIPCODE_ERROR_MESSAGE];
                if (null == savedValue) { return string.Empty; }

                return savedValue.ToString();
            }
            set
            {
                ViewState[VALIDATION_ZIPCODE_ERROR_MESSAGE] = value;
                _cvZipCode.ErrorMessage = value;
            }
        }

        /// <summary>
        /// Gets or sets the phone required error message.
        /// </summary>
        /// <value>The phone required error message.</value>
        [Browsable(true), Category(VALIDATORS_CATEGORY)]
        public string PhoneRequiredErrorMessage
        {
            get
            {
                object savedValue = ViewState[REQUIRED_PHONE_ERROR_MESSAGE];
                if (null == savedValue) { return string.Empty; }

                return savedValue.ToString();
            }
            set
            {
                ViewState[REQUIRED_PHONE_ERROR_MESSAGE] = value;
                _rfvPhone.ErrorMessage = value;
            }
        }

        /// <summary>
        /// Gets or sets the phone validation error message.
        /// </summary>
        /// <value>The phone validation error message.</value>
        [Browsable(true), Category(VALIDATORS_CATEGORY)]
        public string PhoneValidationErrorMessage
        {
            get
            {
                object savedValue = ViewState[VALIDATION_PHONE_ERROR_MESSAGE];
                if (null == savedValue) { return string.Empty; }

                return savedValue.ToString();
            }
            set
            {
                ViewState[VALIDATION_PHONE_ERROR_MESSAGE] = value;
                _regxPhone.ErrorMessage = value;
            }
        }

        /// <summary>
        /// Gets or sets the city required error message.
        /// </summary>
        /// <value>The city required error message.</value>
        [Browsable(true), Category(VALIDATORS_CATEGORY)]
        public string CityRequiredErrorMessage
        {
            get
            {
                object savedValue = ViewState[CITY_REQUIRED_ERROR_MESSAGE];
                if (null == savedValue) { return string.Empty; }

                return savedValue.ToString();
            }
            set
            {
                ViewState[CITY_REQUIRED_ERROR_MESSAGE] = value;
                _rfvCity.ErrorMessage = value;
            }
        }

        /// <summary>
        /// Gets or sets the zip required error message.
        /// </summary>
        /// <value>The zip required error message.</value>
        [Browsable(true), Category(VALIDATORS_CATEGORY)]
        public string ZipRequiredErrorMessage
        {
            get
            {
                object savedValue = ViewState[ZIP_REQUIRED_ERROR_MESSAGE];
                if (null == savedValue) { return string.Empty; }

                return savedValue.ToString();
            }
            set
            {
                ViewState[ZIP_REQUIRED_ERROR_MESSAGE] = value;
                _rfvZip.ErrorMessage = value;
            }
        }

        /// <summary>
        /// Gets or sets the first length of the name maximum character.
        /// </summary>
        /// <value>The first length of the name maximum character.</value>
        [Browsable(true), Category(VALIDATORS_CATEGORY)]
        public int FirstNameMaximumCharacterLength
        {
            get
            {
                object savedValue = ViewState[FIRST_NAME_MAXIMUM_CHARACTER_LENGTH];

                if (null == savedValue || !(savedValue is int))
                {
                    return int.MaxValue;
                }

                return (int)savedValue;
            }
            set
            {
                ViewState[FIRST_NAME_MAXIMUM_CHARACTER_LENGTH] = value;
            }
        }

        /// <summary>
        /// Gets or sets the last length of the name maximum character.
        /// </summary>
        /// <value>The last length of the name maximum character.</value>
        [Browsable(true), Category(VALIDATORS_CATEGORY)]
        public int LastNameMaximumCharacterLength
        {
            get
            {
                object savedValue = ViewState[LAST_NAME_MAXIMUM_CHARACTER_LENGTH];

                if (null == savedValue || !(savedValue is int))
                {
                    return int.MaxValue;
                }

                return (int)savedValue;
            }
            set
            {
                ViewState[LAST_NAME_MAXIMUM_CHARACTER_LENGTH] = value;
            }
        }

        /// <summary>
        /// Gets or sets the length of the account name maximum character.
        /// </summary>
        /// <value>The length of the account name maximum character.</value>
        [Browsable(true), Category(VALIDATORS_CATEGORY)]
        public int AccountNameMaximumCharacterLength
        {
            get
            {
                object savedValue = ViewState[ACCOUNT_NAME_MAXIMUM_CHARACTER_LENGTH];

                if (null == savedValue || !(savedValue is int))
                {
                    return int.MaxValue;
                }

                return (int)savedValue;
            }
            set
            {
                ViewState[ACCOUNT_NAME_MAXIMUM_CHARACTER_LENGTH] = value;
            }
        }

        /// <summary>
        /// Gets or sets the length of the address maximum character.
        /// </summary>
        /// <value>The length of the address maximum character.</value>
        [Browsable(true), Category(VALIDATORS_CATEGORY)]
        public int AddressMaximumCharacterLength
        {
            get
            {
                object savedValue = ViewState[ADDRESS_MAXIMUM_CHARACTER_LENGTH];

                if (null == savedValue || !(savedValue is int))
                {
                    return int.MaxValue;
                }

                return (int)savedValue;
            }
            set
            {
                ViewState[ADDRESS_MAXIMUM_CHARACTER_LENGTH] = value;
            }
        }

        /// <summary>
        /// Gets or sets the length of the phone maximum character.
        /// </summary>
        /// <value>The length of the phone maximum character.</value>
        [Browsable(true), Category(VALIDATORS_CATEGORY)]
        public int PhoneMaximumCharacterLength
        {
            get
            {
                object savedValue = ViewState[PHONE_MAXIMUM_CHARACTER_LENGTH];

                if (null == savedValue || !(savedValue is int))
                {
                    return int.MaxValue;
                }

                return (int)savedValue;
            }
            set
            {
                ViewState[PHONE_MAXIMUM_CHARACTER_LENGTH] = value;
            }
        }

        /// <summary>
        /// Gets or sets the first name maximum character length error message.
        /// </summary>
        /// <value>The first name maximum character length error message.</value>
        [Browsable(true), Category(VALIDATORS_CATEGORY)]
        public string FirstNameMaximumCharacterLengthErrorMessage
        {
            get
            {
                object savedValue = ViewState[FIRST_NAME_MAXIMUM_CHARACTER_LENGTH_ERROR_MESSAGE];
                if (null == savedValue) { return string.Empty; }

                return savedValue.ToString();
            }
            set
            {
                ViewState[FIRST_NAME_MAXIMUM_CHARACTER_LENGTH_ERROR_MESSAGE] = value;
            }
        }

        /// <summary>
        /// Gets or sets the last name maximum character length error message.
        /// </summary>
        /// <value>The last name maximum character length error message.</value>
        [Browsable(true), Category(VALIDATORS_CATEGORY)]
        public string LastNameMaximumCharacterLengthErrorMessage
        {
            get
            {
                object savedValue = ViewState[LAST_NAME_MAXIMUM_CHARACTER_LENGTH_ERROR_MESSAGE];
                if (null == savedValue) { return string.Empty; }

                return savedValue.ToString();
            }
            set
            {
                ViewState[LAST_NAME_MAXIMUM_CHARACTER_LENGTH_ERROR_MESSAGE] = value;
            }
        }

        /// <summary>
        /// Gets or sets the account name maximum character length error message.
        /// </summary>
        /// <value>The account name maximum character length error message.</value>
        [Browsable(true), Category(VALIDATORS_CATEGORY)]
        public string AccountNameMaximumCharacterLengthErrorMessage
        {
            get
            {
                object savedValue = ViewState[ACCOUNT_NAME_MAXIMUM_CHARACTER_LENGTH_ERROR_MESSAGE];
                if (null == savedValue) { return string.Empty; }

                return savedValue.ToString();
            }
            set
            {
                ViewState[ACCOUNT_NAME_MAXIMUM_CHARACTER_LENGTH_ERROR_MESSAGE] = value;
            }
        }

        /// <summary>
        /// Gets or sets the address maximum character length error message.
        /// </summary>
        /// <value>The address maximum character length error message.</value>
        [Browsable(true), Category(VALIDATORS_CATEGORY)]
        public string AddressMaximumCharacterLengthErrorMessage
        {
            get
            {
                object savedValue = ViewState[ADDRESS_MAXIMUM_CHARACTER_LENGTH_ERROR_MESSAGE];
                if (null == savedValue) { return string.Empty; }

                return savedValue.ToString();
            }
            set
            {
                ViewState[ADDRESS_MAXIMUM_CHARACTER_LENGTH_ERROR_MESSAGE] = value;
            }
        }

        /// <summary>
        /// Gets or sets the phone maximum character length error message.
        /// </summary>
        /// <value>The phone maximum character length error message.</value>
        [Browsable(true), Category(VALIDATORS_CATEGORY)]
        public string PhoneMaximumCharacterLengthErrorMessage
        {
            get
            {
                object savedValue = ViewState[PHONE_MAXIMUM_CHARACTER_LENGTH_ERROR_MESSAGE];
                if (null == savedValue) { return string.Empty; }

                return savedValue.ToString();
            }
            set
            {
                ViewState[PHONE_MAXIMUM_CHARACTER_LENGTH_ERROR_MESSAGE] = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum character length of the city.
        /// </summary>
        /// <value>The maximum character length of the city.</value>
        [Browsable(true), Category(VALIDATORS_CATEGORY)]
        public int CityMaximumCharacterLength
        {
            get
            {
                object savedValue = ViewState[CITY_MAXIMUM_CHARACTER_LENGTH];

                if (null == savedValue || !(savedValue is int))
                {
                    return int.MaxValue;
                }

                return (int)savedValue;
            }
            set
            {
                ViewState[CITY_MAXIMUM_CHARACTER_LENGTH] = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum character length of the postal code.
        /// </summary>
        /// <value>The maximum character length of the postal code.</value>
        [Browsable(true), Category(VALIDATORS_CATEGORY)]
        public int PostalCodeMaximumCharacterLength
        {
            get
            {
                object savedValue = ViewState[POSTAL_CODE_MAXIMUM_CHARACTER_LENGTH];

                if (null == savedValue || !(savedValue is int))
                {
                    return int.MaxValue;
                }

                return (int)savedValue;
            }
            set
            {
                ViewState[POSTAL_CODE_MAXIMUM_CHARACTER_LENGTH] = value;
            }
        }

        /// <summary>
        /// Gets or sets the city maximum character length error message.
        /// </summary>
        /// <value>The city maximum character length error message.</value>
        [Browsable(true), Category(VALIDATORS_CATEGORY)]
        public string CityMaximumCharacterLengthErrorMessage
        {
            get
            {
                object savedValue = ViewState[CITY_MAXIMUM_CHARACTER_LENGTH_ERROR_MESSAGE];
                if (null == savedValue) { return string.Empty; }

                return savedValue.ToString();
            }
            set
            {
                ViewState[CITY_MAXIMUM_CHARACTER_LENGTH_ERROR_MESSAGE] = value;
            }
        }

        /// <summary>
        /// Gets or sets the postal code maximum character length error message.
        /// </summary>
        /// <value>The postal code maximum character length error message.</value>
        [Browsable(true), Category(VALIDATORS_CATEGORY)]
        public string PostalCodeMaximumCharacterLengthErrorMessage
        {
            get
            {
                object savedValue = ViewState[POSTAL_CODE_MAXIMUM_CHARACTER_LENGTH_ERROR_MESSAGE];
                if (null == savedValue) { return string.Empty; }

                return savedValue.ToString();
            }
            set
            {
                ViewState[POSTAL_CODE_MAXIMUM_CHARACTER_LENGTH_ERROR_MESSAGE] = value;
            }
        }

        /// <summary>
        /// Gets or sets the PO box address not allowed error message.
        /// </summary>
        /// <value>The PO box address not allowed error message.</value>
        [Browsable(true), Category(VALIDATORS_CATEGORY)]
        public string POBoxAddressNotAllowedErrorMessage
        {
            get
            {
                object savedValue = ViewState[POBOX_ADDRESS_NOT_ALLOWED_ERROR_MESSAGE];
                if (null == savedValue) { return string.Empty; }

                return savedValue.ToString();
            }
            set
            {
                ViewState[POBOX_ADDRESS_NOT_ALLOWED_ERROR_MESSAGE] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show validators inline].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [show validators inline]; otherwise, <c>false</c>.
        /// </value>
        [Browsable(true), Category(VALIDATORS_CATEGORY)]
        public bool ShowValidatorsInline
        {
            get { return _showValidatorsInline; }
            set { _showValidatorsInline = value; }
        }

        /// <summary>
        /// Gets or sets the first name required field validation group.
        /// </summary>
        /// <value>The first name required field validation group.</value>
        [Browsable(true), Category(VALIDATION_GROUP)]
        public string FirstNameReqFieldValGrp
        {
            get { return _rfvFirstName.ValidationGroup; }
            set { _rfvFirstName.ValidationGroup = value; }
        }

        /// <summary>
        /// Gets or sets the last name required field validation group.
        /// </summary>
        /// <value>The last name required field validation group.</value>
        [Browsable(true), Category(VALIDATION_GROUP)]
        public string LastNameReqFieldValGrp
        {
            get { return _rfvLastName.ValidationGroup; }
            set { _rfvLastName.ValidationGroup = value; }
        }

        /// <summary>
        /// Gets or sets the phone number required field validation group.
        /// </summary>
        /// <value>The phone number required field validation group.</value>
        [Browsable(true), Category(VALIDATION_GROUP)]
        public string PhoneNumberReqFieldValGrp
        {
            get { return _rfvPhone.ValidationGroup; }
            set { _rfvPhone.ValidationGroup = value; }
        }

        /// <summary>
        /// Gets or sets the phone number validation field validation group.
        /// </summary>
        /// <value>The phone number validation field validation group.</value>
        [Browsable(true), Category(VALIDATION_GROUP)]
        public string PhoneNumberRegxFieldValGrp
        {
            get { return _regxPhone.ValidationGroup; }
            set { _regxPhone.ValidationGroup = value; }
        }

        /// <summary>
        /// Gets or sets the address1 required field validation group.
        /// </summary>
        /// <value>The address1 required field validation group.</value>
        [Browsable(true), Category(VALIDATION_GROUP)]
        public string Address1ReqFieldValGrp
        {
            get { return _rfvAddress1.ValidationGroup; }
            set { _rfvAddress1.ValidationGroup = value; }
        }

        /// <summary>
        /// Gets or sets the address1 custom validation group.
        /// </summary>
        /// <value>The address1 custom validation group.</value>
        [Browsable(true), Category(VALIDATION_GROUP)]
        public string Address1CustomValGrp
        {
            get { return _cvAddress1.ValidationGroup; }
            set { _cvAddress1.ValidationGroup = value; }
        }

        /// <summary>
        /// Gets or sets the zip code custom validation group.
        /// </summary>
        /// <value>The zip code custom validation group.</value>
        [Browsable(true), Category(VALIDATION_GROUP)]
        public string ZipCodeCustomValGrp
        {
            get { return _cvZipCode.ValidationGroup; }
            set { _cvZipCode.ValidationGroup = value; }
        }

        /// <summary>
        /// Gets or sets the zip code validate.
        /// </summary>
        /// <value>The zip code validate.</value>
        [Browsable(false)]
        public int CountryIDToValidateZipCode
        {
            get { return _cvZipCode.CountryID; }
            set { _cvZipCode.CountryID = value; }
        }


        /// <summary>
        /// Gets or sets the city required field validation group.
        /// </summary>
        /// <value>The city required field validation group.</value>
        [Browsable(true), Category(VALIDATION_GROUP)]
        public string CityReqFieldValGrp
        {
            get { return _rfvCity.ValidationGroup; }
            set { _rfvCity.ValidationGroup = value; }
        }


        /// <summary>
        /// Gets or sets the zip required field validation group.
        /// </summary>
        /// <value>The zip required field validation group.</value>
        [Browsable(true), Category(VALIDATION_GROUP)]
        public string ZipReqFieldValGrp
        {
            get { return _rfvZip.ValidationGroup; }
            set { _rfvZip.ValidationGroup = value; }
        }


        /// <summary>
        /// Gets or sets the value indicating whether the control is editable
        /// </summary>
        public bool AllowEdit
        {
            get { return _allowEdit; }
            set
            {
                _allowEdit = value;
                CheckEditMode(value);
            }
        }

        public AddressTypes AddressType
        {
            get;
            set;
        }
        #endregion

        /// <summary>
        /// Overrides the TagKey to render the html element container to div instead of the default which is span
        /// </summary>
        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Div;
            }
        }
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            _cvAddress1.AddressType = AddressType;
        }
        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();

            CheckEditMode(this.AllowEdit);

            // main table container
            this.Controls.Add(new LiteralControl("<div class='form address-control'>\n"));

            if (this.ShowNickName)
            {
                Controls.Add(new LiteralControl("<div class='form-group nick-name'>\n"));
                Controls.Add(new LiteralControl("<label>"));
                Controls.Add(_lblNickName);
                Controls.Add(new LiteralControl("</label>"));
                Controls.Add(_txtNickName);
                Controls.Add(new LiteralControl("</div>"));
            }

            if (this.ShowFirstName)
            {
                Controls.Add(new LiteralControl("<div class='form-group first-name'>\n"));
                Controls.Add(new LiteralControl("<label>"));
                Controls.Add(_lblFirstName);
                Controls.Add(new LiteralControl("</label>"));
                Controls.Add(_txtFirstName);
                if (!ShowValidatorsInline)
                {
                    _rfvFirstName.Display = ValidatorDisplay.None;
                }
                Controls.Add(_rfvFirstName);
                Controls.Add(new LiteralControl("</div>"));
            }

            if (this.ShowLastName)
            {
                Controls.Add(new LiteralControl("<div class='form-group last-name'>\n"));
                Controls.Add(new LiteralControl("<label>"));
                Controls.Add(_lblLastName);
                Controls.Add(new LiteralControl("</label>"));
                Controls.Add(_txtLastName);
                if (!ShowValidatorsInline)
                {
                    _rfvLastName.Display = ValidatorDisplay.None;
                }
                Controls.Add(_rfvLastName);
                Controls.Add(new LiteralControl("</div>"));
            }

            if (this.ShowPhoneNumber)
            {
                Controls.Add(new LiteralControl("<div class='form-group phone-number'>\n"));
                Controls.Add(new LiteralControl("<label>"));
                Controls.Add(_lblPhoneNumberCaption);
                Controls.Add(new LiteralControl("</label>"));
                Controls.Add(_txtPhoneNumber);
                if (!ShowValidatorsInline)
                {
                    _rfvPhone.Display = ValidatorDisplay.None;
                    _regxPhone.Display = ValidatorDisplay.None;
                }
                Controls.Add(_rfvPhone);
                Controls.Add(_regxPhone);
                Controls.Add(new LiteralControl("</div>"));
            }

            if (this.ShowCompany)
            {
                Controls.Add(new LiteralControl("<div class='form-group company'>\n"));
                Controls.Add(new LiteralControl("<label>"));
                Controls.Add(_lblCompanyCaption);
                Controls.Add(new LiteralControl("</label>"));
                Controls.Add(_txtCompany);
                Controls.Add(new LiteralControl("</div>"));
            }

            if (this.ShowResidenceType)
            {
                Controls.Add(new LiteralControl("<div class='form-group residence-type'>\n"));
                Controls.Add(new LiteralControl("<label>"));
                Controls.Add(_lblResidenceType);
                Controls.Add(new LiteralControl("</label>"));
                Controls.Add(_cboResidenceType);
                Controls.Add(new LiteralControl("</div>"));
            }

            if (this.ShowAddress1)
            {
                Controls.Add(new LiteralControl("<div class='form-group address-one'>\n"));
                Controls.Add(new LiteralControl("<label>"));
                Controls.Add(_lblAddress1Caption);
                Controls.Add(new LiteralControl("</label>"));
                Controls.Add(_txtAddress1);
                if (!ShowValidatorsInline)
                {
                    _rfvAddress1.Display = ValidatorDisplay.None;
                    _cvAddress1.Display = ValidatorDisplay.None;
                }
                Controls.Add(_rfvAddress1);
                Controls.Add(_cvAddress1);
                Controls.Add(new LiteralControl("</div>"));
            }

            if (this.ShowAddress2)
            {
                Controls.Add(new LiteralControl("<div class='form-group address-two'>\n"));
                Controls.Add(new LiteralControl("<label>"));
                Controls.Add(_lblAddress2Caption);
                Controls.Add(new LiteralControl("</label>"));
                Controls.Add(_txtAddress2);
                Controls.Add(new LiteralControl("</div>"));
            }

            if (this.ShowSuite)
            {
                Controls.Add(new LiteralControl("<div class='form-group suite'>\n"));
                Controls.Add(new LiteralControl("<label>"));
                Controls.Add(_lblSuiteCaption);
                Controls.Add(new LiteralControl("</label>"));
                Controls.Add(_txtSuite);
                Controls.Add(new LiteralControl("</div>"));
            }
            if (this.ShowCountry)
            {
                Controls.Add(new LiteralControl("<div class='form-group country'>\n"));
                Controls.Add(new LiteralControl("<label>"));
                Controls.Add(_lblCountryCaption);
                Controls.Add(new LiteralControl("</label>"));
                Controls.Add(_cboCountry);
                Controls.Add(new LiteralControl("</div>"));
            }
			if(this.ShowCity)
			{
				Controls.Add(new LiteralControl("<div class='form-group city'>\n"));
				Controls.Add(new LiteralControl("<label>"));
				Controls.Add(_lblCityCaption);
				Controls.Add(new LiteralControl("</label>"));
				Controls.Add(_txtCity);
				if(!ShowValidatorsInline)
				{
					_rfvCity.Display = ValidatorDisplay.None;
				}
				Controls.Add(_rfvCity);
				Controls.Add(new LiteralControl("</div>"));
			}
            if (this.ShowState)
            {
                Controls.Add(new LiteralControl("<div class='form-group state'>\n"));
                Controls.Add(new LiteralControl("<label>"));
                Controls.Add(_lblStateCaption);
                Controls.Add(new LiteralControl("</label>"));
                Controls.Add(_cboState);
                Controls.Add(new LiteralControl("</div>"));
            }
           


            if (this.ShowZip)
            {
                Controls.Add(new LiteralControl("<div class='form-group zip-code'>\n"));
                Controls.Add(new LiteralControl("<label>"));
                Controls.Add(_lblZipCaption);
                Controls.Add(new LiteralControl("</label>"));
                Controls.Add(_txtZip);
                if (!ShowValidatorsInline)
                {
                    _rfvZip.Display = ValidatorDisplay.None;
                    _cvZipCode.Display = ValidatorDisplay.None;
                }
                Controls.Add(_rfvZip);
                Controls.Add(_cvZipCode);
                Controls.Add(new LiteralControl("</div>"));
            }

            this.Controls.Add(new LiteralControl("</div>"));
            this.Controls.Add(new LiteralControl("<div class='clear'></div>"));
        }

        private void CheckEditMode(bool allowEdit)
        {

            _txtFirstName.Enabled = allowEdit;
            _txtLastName.Enabled = allowEdit;
            _txtNickName.Enabled = allowEdit;
            _txtAddress1.Enabled = allowEdit;
            _txtAddress2.Enabled = allowEdit;
            _cboResidenceType.Enabled = allowEdit;
            _txtCity.Enabled = allowEdit;
            _cboCountry.Enabled = allowEdit;
            _cboState.Enabled = allowEdit;
            _txtZip.Enabled = allowEdit;
            _txtCompany.Enabled = allowEdit;
            _txtSuite.Enabled = allowEdit;
            _txtPhoneNumber.Enabled = allowEdit;

        }

        #region IPostBackDataHandler Members

        /// <summary>
        /// When implemented by a class, processes postback data for an ASP.NET server control.
        /// </summary>
        /// <param name="postDataKey">The key identifier for the control.</param>
        /// <param name="postCollection">The collection of all incoming name values.</param>
        /// <returns>
        /// true if the server control's state changes as a result of the postback; otherwise, false.
        /// </returns>
        public bool LoadPostData(string postDataKey, System.Collections.Specialized.NameValueCollection postCollection)
        {
            return true;
        }

        public void RaisePostDataChangedEvent()
        {
        }

        #endregion
    }

}

