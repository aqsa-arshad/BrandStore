// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.ComponentModel;
using AspDotNetStorefrontCore;
using System.Data;
using System.Data.SqlClient;

namespace AspDotNetStorefrontControls
{
    /// <summary>
    /// Represent ShippingAndTaxEstimatorAddress Control
    /// </summary>
    public class ShippingAndTaxEstimatorAddressControl : CompositeControl
    {
        #region Variable Declaration

        private Table _template = new Table();
        private Label _lblHeader = new Label();
        private Label _lblCountry = new Label();
        private DropDownList _cboCountry = new DropDownList();
        private DropDownList _cboState = new DropDownList();
        private Label _lblCity = new Label();
        private TextBox _txtCity = new TextBox();
        private Label _lblState = new Label();
        private Label _lblZip = new Label();
        private TextBox _txtZip = new TextBox();
        private Button _btnGetEstimate = new Button();
        private Label _lblErrorMessage = new Label();
        private Label requireZipCode = new Label();

        private Unit _captionWidth = Unit.Percentage(50);
        private Unit _valueWidth = Unit.Percentage(50);

        private const string SETTINGS_CATEGORY = "ASPDNSF Settings";
        private const string REQUIRE_POSTAL_CODE = "RequirePostalCode";

        #endregion

        #region Constructor

        public ShippingAndTaxEstimatorAddressControl()
        {
            AssignClientReferenceIds();
            AssignDefualts();
            SetDisplayDefaults();

            if (!this.DesignMode && 
                HttpContext.Current != null) // make sure this doesn't fire up during design time
            {
                _btnGetEstimate.Click += new EventHandler(_btnGetEstimate_Click);
                AssignDatasource();
            }

            requireZipCode.Visible = false;
        }

        private void AssignDatasource()
        {
            //Assign datasource on the country
            List<string> countries = new List<string>();
            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
                using (IDataReader reader = DB.GetRS("SELECT Name FROM Country WITH (NOLOCK) WHERE Published = 1 ORDER BY DisplayOrder", dbconn))
                {
                    while (reader.Read())
                    {
                        countries.Add(DB.RSField(reader, "Name"));
                    }
                }
            }

            this._cboCountry.DataSource = countries;
            this._cboCountry.DataBind();
            this._cboCountry.AutoPostBack = true;

            //Assign datasource on the state
            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();

                string selectedCountry = _cboCountry.SelectedValue;
                if (!CommonLogic.IsStringNullOrEmpty(CommonLogic.FormCanBeDangerousContent("ctrlEstimateAddress$Country")))
                {
                    selectedCountry = CommonLogic.FormCanBeDangerousContent("ctrlEstimateAddress$Country");
                }

                if (!CommonLogic.IsStringNullOrEmpty(CommonLogic.FormCanBeDangerousContent("ctl00$PageContent$ctrlEstimateAddress$Country")))
                {
                    selectedCountry = CommonLogic.FormCanBeDangerousContent("ctl00$PageContent$ctrlEstimateAddress$Country");
                }

                string stateQuery = "select s.* from State s  with (NOLOCK)  join country c  with (NOLOCK)  on s.countryid = c.countryid where c.name = " + DB.SQuote(selectedCountry) + " order by s.DisplayOrder,s.Name";
                using (IDataReader statesReader = DB.GetRS(stateQuery, dbconn))
                {
                    this._cboState.DataSource = statesReader;
                    this._cboState.DataTextField = "Name";
                    this._cboState.DataValueField = "Abbreviation";
                    this._cboState.DataBind();
                }
                //disable the state dropdownlist
                if (this._cboState.Items.Count < 1)
                {
                    _lblState.Visible = false;
                    _cboState.Visible = false;

                }

                

                if (!CountryRequiresZip(selectedCountry))
                {
                    this.RequirePostalCode = false;
                    _lblZip.Visible = false;
                    _txtZip.Visible = false;
                }

            }
        }

        private Boolean CountryRequiresZip(String selectedCountry)
        {
            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
                string postalRequiredQuery = "select postalcoderequired from country where name = " + DB.SQuote(selectedCountry);
                Boolean postCodeRequired = true;
                using (IDataReader postRequiredReader = DB.GetRS(postalRequiredQuery, dbconn))
                {
                    if (postRequiredReader.Read())
                        postCodeRequired = DB.RSFieldBool(postRequiredReader, "postalcoderequired");
                }
                return postCodeRequired;
            }
        }

        #endregion

        #region Properties

        [Browsable(true), Category(SETTINGS_CATEGORY)]
        public string CountryCaption
        {
            get { return _lblCountry.Text; }
            set { _lblCountry.Text = value; }
        }

        [Browsable(true), Category(SETTINGS_CATEGORY)]
        public string CityCaption
        {
            get { return _lblCity.Text; }
            set { _lblCity.Text = value; }
        }

        [Browsable(true), Category(SETTINGS_CATEGORY)]
        public string StateCaption
        {
            get { return _lblState.Text; }
            set { _lblState.Text = value; }
        }


        [Browsable(true), Category(SETTINGS_CATEGORY)]
        public string ZipCaption
        {
            get { return _lblZip.Text; }
            set { _lblZip.Text = value; }
        }

        [Browsable(true), Category(SETTINGS_CATEGORY)]
        public string Header
        {
            get { return _lblHeader.Text; }
            set { _lblHeader.Text = value; }
        }

        [Browsable(true), Category(SETTINGS_CATEGORY)]
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

        [Browsable(true), Category(SETTINGS_CATEGORY)]
        public string State
        {
            get
            {
                if (null == _cboState.SelectedItem) { return string.Empty; }
                return _cboState.SelectedValue;
            }
            set
            {
                try
                {
                    _cboState.SelectedValue = value;
                }
                catch
                {
                    _cboState.SelectedValue = null;
                }
            }
        }

        [Browsable(true), Category(SETTINGS_CATEGORY)]
        public string City
        {
            get { return _txtCity.Text; }
            set { _txtCity.Text = value; }
        }

        [Browsable(true), Category(SETTINGS_CATEGORY)]
        public string Zip
        {
            get { return _txtZip.Text; }
            set { _txtZip.Text = value; }
        }

        [Browsable(true), Category(SETTINGS_CATEGORY)]
        public Unit CaptionWidth
        {
            get { return _captionWidth; }
            set { _captionWidth = value; }
        }

        [Browsable(true), Category(SETTINGS_CATEGORY)]
        public Unit ValueWidth
        {
            get { return _valueWidth; }
            set { _valueWidth = value; }
        }

        [Browsable(true), Category(SETTINGS_CATEGORY)]
        public string GetEstimateCaption
        {
            get { return _btnGetEstimate.Text; }
            set { _btnGetEstimate.Text = value; }
        }

        [Browsable(true), Category(SETTINGS_CATEGORY)]
        public bool RequirePostalCode
        {
            get
            {
                object booleanValue = ViewState[REQUIRE_POSTAL_CODE];
                if (null == booleanValue) { return false; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState[REQUIRE_POSTAL_CODE] = value;
                ChildControlsCreated = false;

            }
        }

        public string RequireZipCodeErrorMessage
        {
            get
            {

                return requireZipCode.Text;
            }
            set
            {
                requireZipCode.Text = value;

            }
        }

        #endregion

        #region Methods

        private void AssignClientReferenceIds()
        {
            _cboCountry.ID = "Country";
            _txtCity.ID = "City";
            _cboState.ID = "State";
            _txtZip.ID = "Zip";
            _btnGetEstimate.ID = "GetEstimateButton";
            _lblErrorMessage.ID = "ErrorMessage";
        }

        private void AssignDefualts()
        {
            _lblCountry.Text = "Country:";
            _lblCity.Text = "City:";
            _lblState.Text = "State:";
            _lblZip.Text = "Zip:";
        }
        /// <summary>
        /// Set default values 
        /// </summary>
        private void SetDisplayDefaults()
        {
            _template.Width = Unit.Percentage(100);

            _txtCity.Columns = 34;
            _txtCity.MaxLength = 50;

            _txtZip.Columns = 14;
            _txtZip.MaxLength = 10;
        }
        /// <summary>
        /// Create child control
        /// </summary>
        protected override void CreateChildControls()
        {
            _template = new Table();

            _template.CssClass = "shipping-tax-estimator-address";
            TableBuilder builder = new TableBuilder(_template);

            // header            
            builder.NewRow();
            TableCell tcHeader = builder.AddCell(_lblHeader, Unit.Percentage(100), HorizontalAlign.Center, VerticalAlign.Middle);
            tcHeader.ColumnSpan = 2;
            tcHeader.CssClass = "Header";

            // country
            builder.NewRow();
            builder.AddCell(_lblCountry, this.CaptionWidth).CssClass = "Caption";
            builder.AddCell(_cboCountry, this.ValueWidth).CssClass = "Value";

            // city
            builder.NewRow();
            builder.AddCell(_lblCity, this.CaptionWidth).CssClass = "Caption";
            builder.AddCell(_txtCity, this.ValueWidth).CssClass = "Value";

            // state
            builder.NewRow();
            builder.AddCell(_lblState, this.CaptionWidth).CssClass = "Caption";
            builder.AddCell(_cboState, this.ValueWidth).CssClass = "Value";

            // zip
            builder.NewRow();
            builder.AddCell(_lblZip, this.CaptionWidth).CssClass = "Caption";

            TableCell zipCell = builder.AddCell(_txtZip, this.ValueWidth);
            zipCell.CssClass = "Value";

            if (this.RequirePostalCode)
            {
                zipCell.Controls.Add(new LiteralControl("&nbsp;"));

                requireZipCode.ForeColor = System.Drawing.Color.Red;

                zipCell.Controls.Add(requireZipCode);

            }

            // error message goes here
            builder.NewRow();
            TableCell tcErrorMessage = builder.AddCell(_lblErrorMessage, Unit.Percentage(100));
            tcErrorMessage.CssClass = "ErrorMessage";
            tcErrorMessage.ColumnSpan = 2;

            builder.NewRow();
            TableCell tcGetEstimate = builder.AddCell(_btnGetEstimate, Unit.Percentage(100), HorizontalAlign.Right, VerticalAlign.Top);
            tcGetEstimate.ColumnSpan = 2;

            Controls.Add(_template);

            base.CreateChildControls();
        }

        #endregion

        public void HideZipCodeValidation()
        {
            requireZipCode.Visible = false;
        }

        public bool ValidateZipCode()
        {
            if (!CountryRequiresZip(Country))
                return true;

            if (this.RequirePostalCode &&
                CommonLogic.IsStringNullOrEmpty(_txtZip.Text))
            {
                requireZipCode.Visible = true;

                return false;
            }

            return true;

        }

        private void _btnGetEstimate_Click(object sender, EventArgs e)
        {
            OnRequestEstimateButtonClicked();
        }

        public event EventHandler RequestEstimateButtonClicked;

        protected virtual void OnRequestEstimateButtonClicked()
        {
            if (RequestEstimateButtonClicked != null)
            {
                RequestEstimateButtonClicked(this, EventArgs.Empty);
            }
        }
    }
}

