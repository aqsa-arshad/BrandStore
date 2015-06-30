// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel.Design;
using System.Data.SqlClient;
using System.Data;
using AspDotNetStorefrontCore;


namespace AspDotNetStorefrontControls
{
    /// <summary>
    /// Custom control used for credit card needed information
    /// </summary>
    [ToolboxData("<{0}:CreditCardPanel runat=server></{0}:CreditCardPanel>")]
    public class CreditCardPanel : CompositeControl
    { 

        #region Variable Declaration

        private Label _lblHeader = new Label();
        private Label _lblCCName = new Label();
        private Label _lblCCNumber = new Label();
        private Label _lblCCVerCd = new Label();
        private Label _lblCCType = new Label();
        private Label _lblExpDt = new Label();
        private Label _lblNoSpaces = new Label();
        private Label _lblCCStartDt = new Label();
        private Label _lblCCIssueNum = new Label();
        private Label _lblCCIssueNumNote = new Label();
		private Label _lblCimSaveCard = new Label();

        private TextBox _txtCCName = new TextBox();
        private TextBox _txtCCNumber = new TextBox();
        private TextBox _txtCCVerCd = new TextBox();
        private TextBox _txtCCIssueNum = new TextBox();

        private RequiredFieldValidator _rfvCCName = new RequiredFieldValidator();
        private RequiredFieldValidator _rfvCCNumber = new RequiredFieldValidator();
        private RequiredFieldValidator _rfvCCVerCd = new RequiredFieldValidator();

        private DropDownList _ddlCCType = new DropDownList();
        private DropDownList _ddlCCExpMonth = new DropDownList();
        private DropDownList _ddlCCExpYr = new DropDownList();
        private DropDownList _ddlCCStartMonth = new DropDownList();
        private DropDownList _ddlCCStartYr = new DropDownList();

		private Panel _pnlWhatsThisContainer = new Panel();
		private HyperLink _hlnkWhat = new HyperLink();

		private CheckBox _chkCimSaveCard = new CheckBox();

        private bool _boolShowCCVerCd = true;
        private bool _boolShowCCVerCdReqVal = true;
        private bool _boolShowCCStartDtFields = true;
		private bool _boolShowCimSaveCard = true;

        private bool _boolDesignMode = (HttpContext.Current == null);

        private bool _showValidatorsInline = false;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CreditCardPanel"/> class.
        /// </summary>
        public CreditCardPanel()
        {            
            AssignClientReferenceIDs();
            DisableAutoComplete();
            AssignDatasource();
            AssignDefaults();

            _hlnkWhat.NavigateUrl = "javascript:void(0);";
			_pnlWhatsThisContainer.CssClass += " whatsThisLink ";
        }

        #endregion

        #region Properties

        #region Labels

        /// <summary>
        /// Gets or sets the header caption.
        /// </summary>
        /// <value>The header caption.</value>
        [Browsable(true), Category("LABELS")]
        public String HeaderCaption
        {
            get { return _lblHeader.Text; }
            set { _lblHeader.Text = value; }
        }

        /// <summary>
        /// Gets or sets the credit card name caption.
        /// </summary>
        /// <value>The credit card name caption.</value>
        [Browsable(true), Category("LABELS")]
        public String CreditCardNameCaption
        {
            get { return _lblCCName.Text; }
            set { _lblCCName.Text = value; }
        }

        /// <summary>
        /// Gets or sets the credit card number caption.
        /// </summary>
        /// <value>The credit card number caption.</value>
        [Browsable(true), Category("LABELS")]
        public String CreditCardNumberCaption
        {
            get { return _lblCCNumber.Text; }
            set { _lblCCNumber.Text = value; }
        }

        /// <summary>
        /// Gets or sets the credit card verification code caption.
        /// </summary>
        /// <value>The credit card verification code caption.</value>
        [Browsable(true), Category("LABELS")]
        public String CreditCardVerCdCaption
        {
            get { return _lblCCVerCd.Text; }
            set { _lblCCVerCd.Text = value; }
        }

        /// <summary>
        /// Gets or sets the credit card type caption.
        /// </summary>
        /// <value>The credit card type caption.</value>
        [Browsable(true), Category("LABELS")]
        public String CreditCardTypeCaption
        {
            get { return _lblCCType.Text; }
            set { _lblCCType.Text = value; }
        }

        /// <summary>
        /// Gets or sets the credit card expiration date caption.
        /// </summary>
        /// <value>The credit card expiration date caption.</value>
        [Browsable(true), Category("LABELS")]
        public String CreditCardExpDtCaption
        {
            get { return _lblExpDt.Text; }
            set { _lblExpDt.Text = value; }
        }

        /// <summary>
        /// Gets or sets the credit card no spaces caption.
        /// </summary>
        /// <value>The credit card no spaces caption.</value>
        [Browsable(true), Category("LABELS")]
        public String CreditCardNoSpacesCaption
        {
            get { return _lblNoSpaces.Text; }
            set { _lblNoSpaces.Text = value; }
        }


        /// <summary>
        /// Gets or sets the what is this.
        /// </summary>
        /// <value>The what is this.</value>
        [Browsable(true), Category("LABELS")]
        public String WhatIsThis
        {
            get { return _hlnkWhat.Text; }
            set { _hlnkWhat.Text = value; }
        }

        /// <summary>
        /// Gets or sets the credit card start date caption.
        /// </summary>
        /// <value>The credit card start date caption.</value>
        [Browsable(true), Category("LABELS")]
        public String CreditCardStartDtCaption
        {
            get { return _lblCCStartDt.Text; }
            set { _lblCCStartDt.Text = value; }
        }

        /// <summary>
        /// Gets or sets the credit card issue number caption.
        /// </summary>
        /// <value>The credit card issue number caption.</value>
        [Browsable(true), Category("LABELS")]
        public String CreditCardIssueNumCaption
        {
            get { return _lblCCIssueNum.Text; }
            set { _lblCCIssueNum.Text = value; }
        }

        /// <summary>
        /// Gets or sets the credit card issue num note.
        /// </summary>
        /// <value>The credit card issue num note.</value>
        [Browsable(true), Category("LABELS")]
        public String CreditCardIssueNumNote
        {
            get { return _lblCCIssueNumNote.Text; }
            set { _lblCCIssueNumNote.Text = value; }
        }

		/// <summary>
		/// Gets or sets the CIM save card caption.
		/// </summary>
		/// <value>The CIM save card caption.</value>
		[Browsable(true), Category("LABELS")]
		public String CimSaveCardCaption
		{
			get { return _lblCimSaveCard.Text; }
			set { _lblCimSaveCard.Text = value; }
		}

        #endregion

        #region Validations

        /// <summary>
        /// Gets or sets the credit card name required field error message.
        /// </summary>
        /// <value>The credit card name required field error message.</value>
        [Browsable(true), Category("ERROR_MESSAGE")]
        public string CCNameReqFieldErrorMessage
        {
            get { return _rfvCCName.ErrorMessage; }
            set { _rfvCCName.ErrorMessage = value; }
        }

        /// <summary>
        /// Gets or sets the credit card number required field error message.
        /// </summary>
        /// <value>The credit card number required field error message.</value>
        [Browsable(true), Category("ERROR_MESSAGE")]
        public string CCNumberReqFieldErrorMessage
        {
            get { return _rfvCCNumber.ErrorMessage; }
            set { _rfvCCNumber.ErrorMessage = value; }
        }

        /// <summary>
        /// Gets or sets the credit card verification code required field error message.
        /// </summary>
        /// <value>The credit card verification code required field error message.</value>
        [Browsable(true), Category("ERROR_MESSAGE")]
        public string CCVerCdReqFieldErrorMessage
        {
            get { return _rfvCCVerCd.ErrorMessage; }
            set { _rfvCCVerCd.ErrorMessage = value; }
        }

        /// <summary>
        /// Gets or sets the credit card name validation group.
        /// </summary>
        /// <value>The credit card name validation group.</value>
        [Browsable(true), Category("VALIDATION_GROUP")]
        public string CCNameValGrp
        {
            get { return _rfvCCName.ValidationGroup; }
            set { _rfvCCName.ValidationGroup = value; }
        }

        /// <summary>
        /// Gets or sets the credit card number validation group.
        /// </summary>
        /// <value>The credit card number validation group.</value>
        [Browsable(true), Category("VALIDATION_GROUP")]
        public string CCNumberValGrp
        {
            get { return _rfvCCNumber.ValidationGroup; }
            set { _rfvCCNumber.ValidationGroup = value; }
        }

        /// <summary>
        /// Gets or sets the credit card verification code validation group.
        /// </summary>
        /// <value>The credit card verification code validation group.</value>
        [Browsable(true), Category("VALIDATION_GROUP")]
        public string CCVerCdValGrp
        {
            get { return _rfvCCVerCd.ValidationGroup; }
            set { _rfvCCVerCd.ValidationGroup = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show validators inline].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [show validators inline]; otherwise, <c>false</c>.
        /// </value>
        [Browsable(true), Category("VALIDATION_SETTINGS")]
        public bool ShowValidatorsInline
        {
            get { return _showValidatorsInline; }
            set { _showValidatorsInline = value; }
        }

        #endregion

        /// <summary>
        /// Gets or sets the name of the credit card.
        /// </summary>
        /// <value>The name of the credit card.</value>
        [Browsable(false)]
        public string CreditCardName
        {
            get { return _txtCCName.Text; }
            set { _txtCCName.Text = value; }
        }

        /// <summary>
        /// Gets or sets the credit card number.
        /// </summary>
        /// <value>The credit card number.</value>
        [Browsable(false)]
        public string CreditCardNumber
        {
            get { return _txtCCNumber.Text; }
            set { _txtCCNumber.Text = value; }
        }

        /// <summary>
        /// Gets or sets the credit card ver cd.
        /// </summary>
        /// <value>The credit card ver cd.</value>
        [Browsable(false)]
        public string CreditCardVerCd
        {
            get { return _txtCCVerCd.Text; }
            set { _txtCCVerCd.Text = value; }
        }

        /// <summary>
        /// Gets or sets the type of the credit card.
        /// </summary>
        /// <value>The type of the credit card.</value>
        [Browsable(false)]
        public string CreditCardType
        {
            get
            {
                if (null != _ddlCCType.SelectedItem)
                {
                    return _ddlCCType.SelectedValue;
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                try
                {
                    _ddlCCType.SelectedValue = value;
                }
                catch
                {
                    _ddlCCType.SelectedValue = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the card exp month.
        /// </summary>
        /// <value>The card exp month.</value>
        [Browsable(false)]
        public string CardExpMonth
        {
            get
            {
                if (null != _ddlCCExpMonth.SelectedItem)
                {
                    return _ddlCCExpMonth.SelectedValue;
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                try
                {
                    _ddlCCExpMonth.SelectedValue = value;
                }
                catch
                {
                    _ddlCCExpMonth.SelectedValue = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the card exp yr.
        /// </summary>
        /// <value>The card exp yr.</value>
        [Browsable(false)]
        public string CardExpYr
        {
            get
            {
                if (null != _ddlCCExpYr.SelectedItem)
                {
                    return _ddlCCExpYr.SelectedValue;
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                try
                {
                    _ddlCCExpYr.SelectedValue = value;
                }
                catch
                {
                    _ddlCCExpYr.SelectedValue = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the card start month.
        /// </summary>
        /// <value>The card start month.</value>
        [Browsable(false)]
        public string CardStartMonth
        {
            get
            {
                if (null != _ddlCCStartMonth.SelectedItem)
                {
                    return _ddlCCStartMonth.SelectedValue;
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                try
                {
                    _ddlCCStartMonth.SelectedValue = value;
                }
                catch
                {
                    _ddlCCStartMonth.SelectedValue = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the card start year.
        /// </summary>
        /// <value>The card start year.</value>
        [Browsable(false)]
        public string CardStartYear
        {
            get
            {
                if (null != _ddlCCStartYr.SelectedItem)
                {
                    return _ddlCCStartYr.SelectedValue;
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                try
                {
                    _ddlCCStartYr.SelectedValue = value;
                }
                catch
                {
                    _ddlCCStartYr.SelectedValue = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the credit card issue number.
        /// </summary>
        /// <value>The credit card issue number.</value>
        [Browsable(false)]
        public string CreditCardIssueNumber
        {
            get { return _txtCCIssueNum.Text; }
            set { _txtCCIssueNum.Text = value; }
        }

		/// <summary>
		/// Gets or sets whether to save to card to CIM.
		/// </summary>
		/// <value>Set to true to save the card to CIM.</value>
		[Browsable(false)]
		public bool CimSaveCard
		{
			get { return _chkCimSaveCard.Checked; }
			set { _chkCimSaveCard.Checked = value; }
		}

        /// <summary>
        /// Gets or sets a value indicating whether [show credit card vetification code].
        /// </summary>
        /// <value><c>true</c> if [show credit card verification code]; otherwise, <c>false</c>.</value>
        [Browsable(true), Category("SETTINGS")]
        public bool ShowCCVerCd
        {
            get { return _boolShowCCVerCd; }
            set { _boolShowCCVerCd = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show credit card verification code required field validator].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [show credit card verification code required field validator]; otherwise, <c>false</c>.
        /// </value>
        [Browsable(true), Category("SETTINGS")]
        public bool ShowCCVerCdReqVal
        {
            get { return _boolShowCCVerCdReqVal; }
            set { _boolShowCCVerCdReqVal = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show credit card start dt fields].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [show credit card start dt fields]; otherwise, <c>false</c>.
        /// </value>
        [Browsable(true), Category("SETTINGS")]
        public bool ShowCCStartDtFields
        {
            get { return _boolShowCCStartDtFields; }
            set { _boolShowCCStartDtFields = value; }
        }

		/// <summary>
		/// Gets or sets a value indicating whether [show CIM save card checkbox].
		/// </summary>
		/// <value>
		/// 	<c>true</c> if [show CIM save card checkbox]; otherwise, <c>false</c>.
		/// </value>
		[Browsable(true), Category("SETTINGS")]
		public bool ShowCimSaveCard
		{
			get { return _boolShowCimSaveCard; }
			set { _boolShowCimSaveCard = value; }
		}

		public bool CimSaveCardEnabled
		{
			get { return _chkCimSaveCard.Enabled; }
			set { _chkCimSaveCard.Enabled = value; }
		}
		
		public string InitJS
        {
            get
            {
				return "new ToolTip('{0}', 'card-code-tooltip', '<iframe width=400 height=370 frameborder=0 marginheight=2 marginwidth=2 scrolling=no src=\"" + AppLogic.LocateImageURL("App_Themes/Skin_" + Customer.Current.SkinID + "/images/verificationnumber.gif") + "\"></iframe>');";
            }
        }

        public string WhatsThisClientID
        {
            get { return _hlnkWhat.ClientID; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Assigns the client reference IDs.
        /// </summary>
        private void AssignClientReferenceIDs()
        {
            _lblHeader.ID = "lblHeader";
            _lblCCName.ID = "lblCCName";
            _lblCCNumber.ID = "lblCCNum";
            _lblCCVerCd.ID = "lblCCVerCd";
            _lblCCType.ID = "lblCCType";
            _lblExpDt.ID = "lblExpDt";
            _lblNoSpaces.ID = "lblNoSpaces";
            _lblCCStartDt.ID = "lblCCStartDt";
            _lblCCIssueNum.ID = "lblCCIssueNum";
            _lblCCIssueNumNote.ID = "_lblCCIssueNumNote";
			_lblCimSaveCard.ID = "lblCimSaveCard";

            _txtCCName.ID = "txtCCName";
            _txtCCNumber.ID = "txtCCNumber";
            _txtCCVerCd.ID = "txtCCVerCd";
            _txtCCIssueNum.ID = "txtCCIssueNum";

            _ddlCCType.ID = "ddlCCType";
            _ddlCCExpMonth.ID = "ddlCCExpMonth";
            _ddlCCExpYr.ID = "ddlCCExpYr";
            _ddlCCStartMonth.ID = "ddlCCStartMonth";
            _ddlCCStartYr.ID = "ddlCCStartYr";

            _hlnkWhat.ID = "hlnkWhat";

			_chkCimSaveCard.ID = "chkCimSaveCard";

            _txtCCName.CssClass = "form-control card-name";
            _txtCCNumber.CssClass = "form-control card-number";
            _txtCCVerCd.CssClass = "form-control card-ccv";
            _txtCCIssueNum.CssClass = "form-control card-issue-number";
            _ddlCCType.CssClass = "form-control card-type";
            _ddlCCExpMonth.CssClass = "form-control card-expiration-month";
			_ddlCCExpYr.CssClass = "form-control card-expiration-year";
			_ddlCCStartMonth.CssClass = "form-control card-start-month";
			_ddlCCStartYr.CssClass = "form-control card-start-year";
        }

        /// <summary>
        /// Disables the auto complete.
        /// </summary>
        private void DisableAutoComplete()
        {
            _txtCCNumber.AutoCompleteType = AutoCompleteType.Disabled;
            _txtCCVerCd.AutoCompleteType = AutoCompleteType.Disabled;
            _txtCCIssueNum.AutoCompleteType = AutoCompleteType.Disabled;

            _txtCCNumber.Attributes["autocomplete"] = "off";
            _txtCCVerCd.Attributes["autocomplete"] = "off";
            _txtCCIssueNum.Attributes["autocomplete"] = "off";
        }
        

        /// <summary>
        /// Assigns the datasource.
        /// </summary>
        private void AssignDatasource()
        {
            if (!_boolDesignMode)
            {
                Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;

                List<string> CCTypes = CardTypeDataSource.GetAcceptedCreditCardTypes(ThisCustomer);

                this._ddlCCType.DataSource = CCTypes;
                this._ddlCCType.DataBind();
                this._ddlCCType.AutoPostBack = false;

                List<string> Month = new List<string>();

                Month.Add(AppLogic.GetString("address.cs.34", ThisCustomer.SkinID, ThisCustomer.LocaleSetting));

                for (int i = 1; i <= 12; i++)
                {
                    Month.Add(i.ToString().PadLeft(2, '0'));
                }

                this._ddlCCExpMonth.DataSource = Month;
                this._ddlCCExpMonth.DataBind();
                this._ddlCCExpMonth.AutoPostBack = false;

                List<string> StartMonth = new List<string>();

                StartMonth.Add(AppLogic.GetString("address.cs.34", ThisCustomer.SkinID, ThisCustomer.LocaleSetting));

                for (int i = 1; i <= 12; i++)
                {
                    StartMonth.Add(i.ToString().PadLeft(2, '0'));
                }

                this._ddlCCStartMonth.DataSource = StartMonth;
                this._ddlCCStartMonth.DataBind();
                this._ddlCCStartMonth.AutoPostBack = false;

                List<string> Year = new List<string>();

                Year.Add(AppLogic.GetString("address.cs.35", ThisCustomer.SkinID, ThisCustomer.LocaleSetting));

                for (int y = 0; y <= 10; y++)
                {
                    Year.Add((System.DateTime.Now.Year + y).ToString());
                }

                this._ddlCCExpYr.DataSource = Year;
                this._ddlCCExpYr.DataBind();
                this._ddlCCExpYr.AutoPostBack = false;

                List<string> StartYear = new List<string>();

                StartYear.Add(AppLogic.GetString("address.cs.35", ThisCustomer.SkinID, ThisCustomer.LocaleSetting));

                for (int y = -4; y <= 10; y++)
                {
                    StartYear.Add((System.DateTime.Now.Year + y).ToString());
                }

                this._ddlCCStartYr.DataSource = StartYear;
                this._ddlCCStartYr.DataBind();
                this._ddlCCStartYr.AutoPostBack = false;
            }
        } 

        /// <summary>
        /// Assigns the defaults.
        /// </summary>
        private void AssignDefaults()
        {
            _txtCCName.MaxLength = 100;
            _txtCCNumber.MaxLength = 20;
            _txtCCVerCd.MaxLength = 10;

            _rfvCCName.ControlToValidate = _txtCCName.ID;
            _rfvCCNumber.ControlToValidate = _txtCCNumber.ID;
            _rfvCCVerCd.ControlToValidate = _txtCCVerCd.ID;
                        
        }

        /// <summary>
        /// Gets the <see cref="T:System.Web.UI.HtmlTextWriterTag"/> value that corresponds to this Web server control. This property is used primarily by control developers.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// One of the <see cref="T:System.Web.UI.HtmlTextWriterTag"/> enumeration values.
        /// </returns>
        protected override HtmlTextWriterTag TagKey
        {
            get { return HtmlTextWriterTag.Div; }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            this.Controls.Clear();
            this.Controls.Add(new LiteralControl("<div class='form credit-card-form'>"));
            this.Controls.Add(new LiteralControl("<div class='form-text'>"));
            this.Controls.Add(_lblHeader);
            this.Controls.Add(new LiteralControl("</div>"));
            this.Controls.Add(new LiteralControl("<script type=\"text/javascript\" language=\"Javascript\" src=\"jscripts/tooltip.js\" >\n"));
            this.Controls.Add(new LiteralControl("</script>\n"));

            this.Controls.Add(new LiteralControl("<div class='form-group'>"));
            this.Controls.Add(new LiteralControl("<label>"));
            this.Controls.Add(_lblCCName);
            this.Controls.Add(new LiteralControl("</label>"));
            this.Controls.Add(_txtCCName);
            this.Controls.Add(new LiteralControl(" "));
            if (!ShowValidatorsInline)
            {
                _rfvCCName.Display = ValidatorDisplay.None;
            }
            this.Controls.Add(_rfvCCName);
            this.Controls.Add(new LiteralControl("</div>"));

            this.Controls.Add(new LiteralControl("<div class='form-group'>"));
            this.Controls.Add(new LiteralControl("<label>"));
            this.Controls.Add(_lblCCNumber);
            this.Controls.Add(new LiteralControl("</label>"));
            this.Controls.Add(_txtCCNumber);
            this.Controls.Add(new LiteralControl("<div class='form-text'>"));
            this.Controls.Add(_lblNoSpaces);
            this.Controls.Add(new LiteralControl("</div>"));
            if (!ShowValidatorsInline)
            {
                _rfvCCNumber.Display = ValidatorDisplay.None;
            }
            this.Controls.Add(_rfvCCNumber);
            this.Controls.Add(new LiteralControl("</div>"));


            if (_boolShowCCVerCd)
            {
                this.Controls.Add(new LiteralControl("<div class='form-group'>"));
                this.Controls.Add(new LiteralControl("<label>"));
                this.Controls.Add(_lblCCVerCd);
                this.Controls.Add(new LiteralControl("</label>"));
                this.Controls.Add(_txtCCVerCd);

                if (this.DesignMode == false)
                {
					this.Controls.Add(_pnlWhatsThisContainer);
					_pnlWhatsThisContainer.Controls.Add(new LiteralControl("<div class='form-text'>"));
					_pnlWhatsThisContainer.Controls.Add(_hlnkWhat);
					_pnlWhatsThisContainer.Controls.Add(new LiteralControl("<script type=\"text/javascript\" language=\"Javascript\" >\n"));
					_pnlWhatsThisContainer.Controls.Add(new LiteralControl("$window_addLoad( \n"));
					_pnlWhatsThisContainer.Controls.Add(new LiteralControl(" function() { \n"));
					_pnlWhatsThisContainer.Controls.Add(new LiteralControl("    " + string.Format(InitJS, WhatsThisClientID) + "\n"));
					_pnlWhatsThisContainer.Controls.Add(new LiteralControl(" }\n"));
					_pnlWhatsThisContainer.Controls.Add(new LiteralControl(") \n"));
					_pnlWhatsThisContainer.Controls.Add(new LiteralControl("</script>\n"));
					_pnlWhatsThisContainer.Controls.Add(new LiteralControl("</div>"));
                }

                if (!_boolShowCCVerCdReqVal)
                {
                    if (!ShowValidatorsInline)
                    {
                        _rfvCCVerCd.Display = ValidatorDisplay.None;
                    }
                    this.Controls.Add(_rfvCCVerCd);
                }
                this.Controls.Add(new LiteralControl("</div>")); 
            }

            this.Controls.Add(new LiteralControl("<div class='form-group'>"));
            this.Controls.Add(new LiteralControl("<label>"));
            this.Controls.Add(_lblCCType);
            this.Controls.Add(new LiteralControl("</label>"));
            this.Controls.Add(_ddlCCType);
            this.Controls.Add(new LiteralControl("</div>"));

            this.Controls.Add(new LiteralControl("<div class='form-group month-year'>"));
            this.Controls.Add(new LiteralControl("<label>"));
            this.Controls.Add(_lblExpDt);
            this.Controls.Add(new LiteralControl("</label>"));
            this.Controls.Add(_ddlCCExpMonth);
            this.Controls.Add(_ddlCCExpYr);
            this.Controls.Add(new LiteralControl("</div>"));
            if (_boolShowCCStartDtFields)
            {
                this.Controls.Add(new LiteralControl("<div class='form-group month-year'>"));
                this.Controls.Add(new LiteralControl("<label>"));
                this.Controls.Add(_lblCCStartDt);
                this.Controls.Add(new LiteralControl("</label>"));
                this.Controls.Add(_ddlCCStartMonth);
                this.Controls.Add(_ddlCCStartYr);
                this.Controls.Add(new LiteralControl("</div>"));

                this.Controls.Add(new LiteralControl("<div class='form-group'>"));
                this.Controls.Add(new LiteralControl("<label>"));
                this.Controls.Add(_lblCCIssueNum);
                this.Controls.Add(new LiteralControl("</label>"));
                this.Controls.Add(_txtCCIssueNum);
                this.Controls.Add(new LiteralControl("<div class='form-text'>"));
                this.Controls.Add(_lblCCIssueNumNote);
                this.Controls.Add(new LiteralControl("</div>"));
                this.Controls.Add(new LiteralControl("</div>"));
            }

			if(_boolShowCimSaveCard)
			{
                this.Controls.Add(new LiteralControl("<div class='form-group'>"));
                this.Controls.Add(new LiteralControl("<label>"));
				this.Controls.Add(_lblCimSaveCard);
                this.Controls.Add(new LiteralControl("</label>"));
				this.Controls.Add(_chkCimSaveCard);
                this.Controls.Add(new LiteralControl("</div>"));
			}

            this.Controls.Add(new LiteralControl("</div>"));
        }
        
        #endregion


        public void Clear()
        {
            CreditCardName =
                        CreditCardNumber =
                        CreditCardVerCd =
                        CreditCardType = "";
            
			_ddlCCExpMonth.SelectedIndex =
                _ddlCCExpYr.SelectedIndex =
                _ddlCCStartYr.SelectedIndex =
                _ddlCCStartMonth.SelectedIndex =
                _ddlCCType.SelectedIndex = -1;
			
			_chkCimSaveCard.Checked = false;
        }

		protected override void Render(HtmlTextWriter writer)
		{
			base.Render(writer);
		}
    }
}
