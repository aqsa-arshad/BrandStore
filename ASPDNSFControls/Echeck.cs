// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel.Design;
using System.Data.SqlClient;
using System.Data;


namespace AspDotNetStorefrontControls
{
    /// <summary>
    /// Custom control used for eCheck needed information
    /// </summary>
    [ToolboxData("<{0}:Echeck runat=server></{0}:Echeck>")]
    public class Echeck : CompositeControl
    {

        #region Variable Declaration

        private Label _lblECheckBankAccountName = new Label();
        private Label _lblECheckBankName1 = new Label();
        private Label _lblECheckBankName2 = new Label();
        private Label _lblECheckBankABACode1 = new Label();
        private Label _lblECheckBankABACode2 = new Label();
        private Label _lblECheckBankAccountNumber1 = new Label();
        private Label _lblECheckBankAccountNumber2 = new Label();
        private Label _lblECheckBankAccountType = new Label();
        private Label _lblECheckNote = new Label();

        private TextBox _txtECheckBankAccountName = new TextBox();
        private TextBox _txtECheckBankName = new TextBox();
        private TextBox _txtECheckBankABACode = new TextBox();
        private TextBox _txtECheckBankAccountNumber = new TextBox();

        private RequiredFieldValidator _rfvEcheckBankAccountName = new RequiredFieldValidator();
        private RequiredFieldValidator _rfvEcheckBankName = new RequiredFieldValidator();
        private RequiredFieldValidator _rfvEcheckBankABACode = new RequiredFieldValidator();
        private RequiredFieldValidator _rfvEcheckBankAccountNumber = new RequiredFieldValidator();

        private DropDownList _ddlECheckBankAccountType = new DropDownList();

        private Image _imgECheckBankABAImage1 = new Image();
        private Image _imgECheckBankABAImage2 = new Image();
        private Image _imgECheckBankAccountImage = new Image();

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Echeck"/> class.
        /// </summary>
        public Echeck()
        {
            AssignClientReferenceIDs();
            DisableAutoComplete();
            AssignDatasource();
            AssignDefaults();
        }

        #endregion

        #region Properties

        #region Labels

        /// <summary>
        /// Gets or sets the ECheck bank account name label.
        /// </summary>
        /// <value>The ECheck bank account name label.</value>
        [Browsable(true), Category("LABEL")]
        public String ECheckBankAccountNameLabel
        {
            get { return _lblECheckBankAccountName.Text; }
            set { _lblECheckBankAccountName.Text = value; }
        }

        /// <summary>
        /// Gets or sets the ECheck bank name label1.
        /// </summary>
        /// <value>The ECheck bank name label1.</value>
        [Browsable(true), Category("LABEL")]
        public String ECheckBankNameLabel1
        {
            get { return _lblECheckBankName1.Text; }
            set { _lblECheckBankName1.Text = value; }
        }

        /// <summary>
        /// Gets or sets the ECheck bank name label2.
        /// </summary>
        /// <value>The ECheck bank name label2.</value>
        [Browsable(true), Category("LABEL")]
        public String ECheckBankNameLabel2
        {
            get { return _lblECheckBankName2.Text; }
            set { _lblECheckBankName2.Text = value; }
        }

        /// <summary>
        /// Gets or sets the ECheck bank ABA code label1.
        /// </summary>
        /// <value>The ECheck bank ABA code label1.</value>
        [Browsable(true), Category("LABEL")]
        public String ECheckBankABACodeLabel1
        {
            get { return _lblECheckBankABACode1.Text; }
            set { _lblECheckBankABACode1.Text = value; }
        }

        /// <summary>
        /// Gets or sets the ECheck bank ABA code label2.
        /// </summary>
        /// <value>The ECheck bank ABA code label2.</value>
        [Browsable(true), Category("LABEL")]
        public String ECheckBankABACodeLabel2
        {
            get { return _lblECheckBankABACode2.Text; }
            set { _lblECheckBankABACode2.Text = value; }
        }

        /// <summary>
        /// Gets or sets the ECheck bank account number label1.
        /// </summary>
        /// <value>The ECheck bank account number label1.</value>
        [Browsable(true), Category("LABEL")]
        public String ECheckBankAccountNumberLabel1
        {
            get { return _lblECheckBankAccountNumber1.Text; }
            set { _lblECheckBankAccountNumber1.Text = value; }
        }

        /// <summary>
        /// Gets or sets the ECheck bank account number label2.
        /// </summary>
        /// <value>The ECheck bank account number label2.</value>
        [Browsable(true), Category("LABEL")]
        public String ECheckBankAccountNumberLabel2
        {
            get { return _lblECheckBankAccountNumber2.Text; }
            set { _lblECheckBankAccountNumber2.Text = value; }
        }

        /// <summary>
        /// Gets or sets the ECheck bank account type label.
        /// </summary>
        /// <value>The ECheck bank account type label.</value>
        [Browsable(true), Category("LABEL")]
        public String ECheckBankAccountTypeLabel
        {
            get { return _lblECheckBankAccountType.Text; }
            set { _lblECheckBankAccountType.Text = value; }
        }

        /// <summary>
        /// Gets or sets the ECheck note label.
        /// </summary>
        /// <value>The ECheck note label.</value>
        [Browsable(true), Category("LABEL")]
        public String ECheckNoteLabel
        {
            get { return _lblECheckNote.Text; }
            set { _lblECheckNote.Text = value; }
        }

        #endregion

        /// <summary>
        /// Gets or sets the name of the ECheck bank account.
        /// </summary>
        /// <value>The name of the ECheck bank account.</value>
        [Browsable(false)]
        public String ECheckBankAccountName
        {
            get { return _txtECheckBankAccountName.Text.Trim(); }
            set { _txtECheckBankAccountName.Text = value; }
        }

        /// <summary>
        /// Gets or sets the name of the ECheck bank.
        /// </summary>
        /// <value>The name of the ECheck bank.</value>
        [Browsable(false)]
        public string ECheckBankName
        {
            get { return _txtECheckBankName.Text.Trim(); }
            set { _txtECheckBankName.Text = value; }
        }

        /// <summary>
        /// Gets or sets the ECheck bank ABA code.
        /// </summary>
        /// <value>The ECheck bank ABA code.</value>
        [Browsable(false)]
        public string ECheckBankABACode
        {
            get { return _txtECheckBankABACode.Text.Trim(); }
            set { _txtECheckBankABACode.Text = value; }
        }

        /// <summary>
        /// Gets or sets the ECheck bank account number.
        /// </summary>
        /// <value>The ECheck bank account number.</value>
        [Browsable(false)]
        public string ECheckBankAccountNumber
        {
            get { return _txtECheckBankAccountNumber.Text.Trim(); }
            set { _txtECheckBankAccountNumber.Text = value; }
        }

        /// <summary>
        /// Gets or sets the type of the ECheck bank account.
        /// </summary>
        /// <value>The type of the ECheck bank account.</value>
        [Browsable(false)]
        public string ECheckBankAccountType
        {
            get
            {
                if (null != _ddlECheckBankAccountType.SelectedItem)
                {
                    return _ddlECheckBankAccountType.SelectedValue;
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
                    _ddlECheckBankAccountType.SelectedValue = value;
                }
                catch
                {
                    _ddlECheckBankAccountType.SelectedValue = null;
                }
            }
        }

        #region Images

        /// <summary>
        /// Gets or sets the ECheck bank ABA image1.
        /// </summary>
        /// <value>The ECheck bank ABA image1.</value>
        [Browsable(true), Category("IMAGE")]
        public string ECheckBankABAImage1
        {
            get { return _imgECheckBankABAImage1.ImageUrl; }
            set
            {
                _imgECheckBankABAImage1.ImageUrl = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// Gets or sets the ECheck bank ABA image2.
        /// </summary>
        /// <value>The ECheck bank ABA image2.</value>
        [Browsable(true), Category("IMAGE")]
        public string ECheckBankABAImage2
        {
            get { return _imgECheckBankABAImage2.ImageUrl; }
            set { _imgECheckBankABAImage2.ImageUrl = value; }
        }

        /// <summary>
        /// Gets or sets the ECheck bank account image.
        /// </summary>
        /// <value>The ECheck bank account image.</value>
        [Browsable(true), Category("IMAGE")]
        public string ECheckBankAccountImage
        {
            get { return _imgECheckBankAccountImage.ImageUrl; }
            set { _imgECheckBankAccountImage.ImageUrl = value; }
        }

        #endregion

        #region Validations

        /// <summary>
        /// Gets or sets the bank account name required field error message.
        /// </summary>
        /// <value>The bank account name required field error message.</value>
        [Browsable(true), Category("ERROR_MESSAGE")]
        public string BankAccountNameReqFieldErrorMessage
        {
            get { return _rfvEcheckBankAccountName.ErrorMessage; }
            set { _rfvEcheckBankAccountName.ErrorMessage = value; }
        }

        /// <summary>
        /// Gets or sets the bank name required field error message.
        /// </summary>
        /// <value>The bank name required field error message.</value>
        [Browsable(true), Category("ERROR_MESSAGE")]
        public string BankNameReqFieldErrorMessage
        {
            get { return _rfvEcheckBankName.ErrorMessage; }
            set { _rfvEcheckBankName.ErrorMessage = value; }
        }

        /// <summary>
        /// Gets or sets the bank ABA code required field error message.
        /// </summary>
        /// <value>The bank ABA code required field error message.</value>
        [Browsable(true), Category("ERROR_MESSAGE")]
        public string BankABACodeReqFieldErrorMessage
        {
            get { return _rfvEcheckBankABACode.ErrorMessage; }
            set { _rfvEcheckBankABACode.ErrorMessage = value; }
        }

        /// <summary>
        /// Gets or sets the bank account number required field error message.
        /// </summary>
        /// <value>The bank account number required field error message.</value>
        [Browsable(true), Category("ERROR_MESSAGE")]
        public string BankAccountNumberReqFieldErrorMessage
        {
            get { return _rfvEcheckBankAccountNumber.ErrorMessage; }
            set { _rfvEcheckBankAccountNumber.ErrorMessage = value; }
        }

        /// <summary>
        /// Gets or sets the bank account name required field validation group.
        /// </summary>
        /// <value>The bank account name required field validation group.</value>
        [Browsable(true), Category("VALIDATION_GROUP")]
        public string BankAccountNameReqFieldValGrp
        {
            get { return _rfvEcheckBankAccountName.ValidationGroup; }
            set { _rfvEcheckBankAccountName.ValidationGroup = value; }
        }

        /// <summary>
        /// Gets or sets the bank name required field validation group.
        /// </summary>
        /// <value>The bank name required field validation group.</value>
        [Browsable(true), Category("VALIDATION_GROUP")]
        public string BankNameReqFieldValGrp
        {
            get { return _rfvEcheckBankName.ValidationGroup; }
            set { _rfvEcheckBankName.ValidationGroup = value; }
        }

        /// <summary>
        /// Gets or sets the bank ABA code required field validation group.
        /// </summary>
        /// <value>The bank ABA code required field validation group.</value>
        [Browsable(true), Category("VALIDATION_GROUP")]
        public string BankABACodeReqFieldValGrp
        {
            get { return _rfvEcheckBankABACode.ValidationGroup; }
            set { _rfvEcheckBankABACode.ValidationGroup = value; }
        }

        /// <summary>
        /// Gets or sets the bank account number required field validation group.
        /// </summary>
        /// <value>The bank account number required field validation group.</value>
        [Browsable(true), Category("VALIDATION_GROUP")]
        public string BankAccountNumberReqFieldValGrp
        {
            get { return _rfvEcheckBankAccountNumber.ValidationGroup; }
            set { _rfvEcheckBankAccountNumber.ValidationGroup = value; }
        }

        #endregion

        #endregion

        #region Methods

        private string _ValidationFailureMessage = string.Empty;
        private Regex rxABANumber = new Regex("[*0-9]{9}");
        private Regex rxAccountNumber = new Regex("[*0-9]{6,20}");

        private string GetString(string key)
        {
            return AspDotNetStorefrontCore.AppLogic.GetString(
                key,
                AspDotNetStorefrontCore.Customer.Current.SkinID,
                AspDotNetStorefrontCore.Customer.Current.LocaleSetting);
        }
        /// <summary>
        /// Validates the controls input fields
        /// </summary>
        public bool Validate()
        {
            if (ECheckBankName == string.Empty)
            {

                _ValidationFailureMessage = GetString("ECheck.Error.BankNameRequired");
                return false;
            }
            if (ECheckBankAccountName == string.Empty)
            {
                _ValidationFailureMessage = GetString("ECheck.Error.AccountNameRequired");
                return false;
            }
            if (!rxABANumber.IsMatch(ECheckBankABACode))
            {
                _ValidationFailureMessage = GetString("ECheck.Error.InvalidABA");
                return false;
            }
            if (!rxAccountNumber.IsMatch(ECheckBankAccountNumber))
            {
                _ValidationFailureMessage = GetString("ECheck.Error.InvalidAccountNumber");
                return false;
            }
            return true;

        }

        /// <summary>
        /// Validates the controls input fields.
        /// </summary>
        /// <param name="ValidationFailureMessage">Pass-thru variable that recieves the validation failure message</param>
        public bool Validate(out string ValidationFailureMessage)
        {
            bool retVal = Validate();
            ValidationFailureMessage = _ValidationFailureMessage;
            return retVal;
        }

        /// <summary>
        /// Assigns the client reference IDs.
        /// </summary>
        private void AssignClientReferenceIDs()
        {
            _lblECheckBankAccountName.ID = "lblECheckBankAccountName";
            _lblECheckBankName1.ID = "lblECheckBankName1";
            _lblECheckBankName2.ID = "lblECheckBankName2";
            _lblECheckBankABACode1.ID = "lblECheckBankABACode1";
            _lblECheckBankABACode2.ID = "lblECheckBankABACode2";
            _lblECheckBankAccountNumber1.ID = "lblECheckBankAccountNumber1";
            _lblECheckBankAccountNumber2.ID = "lblECheckBankAccountNumber2";
            _lblECheckBankAccountType.ID = "lblECheckBankAccountType";
            _lblECheckNote.ID = "lblECheckNote";

            _txtECheckBankAccountName.ID = "txtECheckBankAccountName";
            _txtECheckBankName.ID = "txtECheckBankName";
            _txtECheckBankABACode.ID = "txtECheckBankABACode";
            _txtECheckBankAccountNumber.ID = "txtECheckBankAccountNumber";

            _ddlECheckBankAccountType.ID = "ddlECheckBankAccountType";

            _imgECheckBankABAImage1.ID = "imgECheckBankABAImage1";
            _imgECheckBankABAImage2.ID = "imgECheckBankABAImage2";
            _imgECheckBankAccountImage.ID = "imgECheckBankAccountImage";

            _txtECheckBankAccountName.CssClass = "form-control";
            _txtECheckBankName.CssClass = "form-control";
            _txtECheckBankABACode.CssClass = "form-control";
            _txtECheckBankAccountNumber.CssClass = "form-control";

            _ddlECheckBankAccountType.CssClass = "form-control";
        }

        /// <summary>
        /// Disables the auto complete.
        /// </summary>
        private void DisableAutoComplete()
        {
            _txtECheckBankABACode.AutoCompleteType = AutoCompleteType.Disabled;
            _txtECheckBankAccountNumber.AutoCompleteType = AutoCompleteType.Disabled;

            _txtECheckBankABACode.Attributes["autocomplete"] = "off";
            _txtECheckBankAccountNumber.Attributes["autocomplete"] = "off";
        }

        /// <summary>
        /// Assigns the datasource.
        /// </summary>
        private void AssignDatasource()
        {
            List<string> AccountTypes = new List<string>();

            AccountTypes.Add("CHECKING");
            AccountTypes.Add("SAVINGS");
            AccountTypes.Add("BUSINESS CHECKING");

            this._ddlECheckBankAccountType.DataSource = AccountTypes;
            this._ddlECheckBankAccountType.DataBind();
            this._ddlECheckBankAccountType.AutoPostBack = false;

        }

        /// <summary>
        /// Assigns the defaults.
        /// </summary>
        private void AssignDefaults()
        {
            _txtECheckBankABACode.Width = Unit.Percentage(20);
            _txtECheckBankAccountNumber.Width = Unit.Percentage(30);

            _txtECheckBankAccountName.MaxLength = 50;
            _txtECheckBankName.MaxLength = 50;
            _txtECheckBankABACode.MaxLength = 9;
            _txtECheckBankAccountNumber.MaxLength = 25;

            _rfvEcheckBankAccountName.ControlToValidate = _txtECheckBankAccountName.ID;
            _rfvEcheckBankName.ControlToValidate = _txtECheckBankName.ID;
            _rfvEcheckBankABACode.ControlToValidate = _txtECheckBankABACode.ID;
            _rfvEcheckBankAccountNumber.ControlToValidate = _txtECheckBankAccountNumber.ID;
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
            this.Controls.Add(new LiteralControl("<div class='form e-check-form'>"));

            this.Controls.Add(new LiteralControl("<div class='form-group e-check-account'>"));
            this.Controls.Add(new LiteralControl("<label>"));
            this.Controls.Add(_lblECheckBankAccountName);
            this.Controls.Add(new LiteralControl("</label>"));
            this.Controls.Add(_txtECheckBankAccountName);
            _rfvEcheckBankAccountName.Display = ValidatorDisplay.None;
            this.Controls.Add(_rfvEcheckBankAccountName);
            this.Controls.Add(new LiteralControl("</div>"));

            this.Controls.Add(new LiteralControl("<div class='form-group e-check-bank'>"));
            this.Controls.Add(new LiteralControl("<label>"));
            this.Controls.Add(_lblECheckBankName1);
            this.Controls.Add(new LiteralControl("</label>"));
            this.Controls.Add(_txtECheckBankName);
            _rfvEcheckBankName.Display = ValidatorDisplay.None;
            this.Controls.Add(_rfvEcheckBankName);
            this.Controls.Add(new LiteralControl("<span class='form-text e-check-bank-two'>"));
            this.Controls.Add(_lblECheckBankName2);
            this.Controls.Add(new LiteralControl("</span>"));
            this.Controls.Add(new LiteralControl("</div>"));



            this.Controls.Add(new LiteralControl("<div class='form-group e-check-aba'>"));
            this.Controls.Add(new LiteralControl("<label>"));
            this.Controls.Add(_lblECheckBankABACode1);
            this.Controls.Add(new LiteralControl("</label>"));
            this.Controls.Add(new LiteralControl("<div class='form-text-wrap'>"));
            this.Controls.Add(_imgECheckBankABAImage1);
            this.Controls.Add(_txtECheckBankABACode);
            this.Controls.Add(_imgECheckBankABAImage2);
            this.Controls.Add(new LiteralControl("<span class='form-text e-check-aba-image-two'>"));
            this.Controls.Add(_lblECheckBankABACode2);
            this.Controls.Add(new LiteralControl("</span>"));
            this.Controls.Add(new LiteralControl("</div>"));
            _rfvEcheckBankABACode.Display = ValidatorDisplay.None;
            this.Controls.Add(_rfvEcheckBankABACode);
            this.Controls.Add(new LiteralControl("</div>"));



            this.Controls.Add(new LiteralControl("<div class='form-group e-check-account-two'>"));
            this.Controls.Add(new LiteralControl("<label>"));
            this.Controls.Add(_lblECheckBankAccountNumber1);
            this.Controls.Add(new LiteralControl("</label>"));
            this.Controls.Add(new LiteralControl("<div class='form-text-wrap'>"));
            this.Controls.Add(_txtECheckBankAccountNumber);
            this.Controls.Add(_imgECheckBankAccountImage);
            this.Controls.Add(new LiteralControl("<span class='form-text e-check-account-image'>"));
            this.Controls.Add(_lblECheckBankAccountNumber2);
            this.Controls.Add(new LiteralControl("</span>"));
            this.Controls.Add(new LiteralControl("</div>"));
            _rfvEcheckBankAccountNumber.Display = ValidatorDisplay.None;
            this.Controls.Add(_rfvEcheckBankAccountNumber);
            this.Controls.Add(new LiteralControl("</div>"));


            this.Controls.Add(new LiteralControl("<div class='form-group e-check-account-type'>"));
            this.Controls.Add(new LiteralControl("<label>"));
            this.Controls.Add(_lblECheckBankAccountType);
            this.Controls.Add(new LiteralControl("</label>"));
            this.Controls.Add(_ddlECheckBankAccountType);
            this.Controls.Add(new LiteralControl("</div>"));

            this.Controls.Add(new LiteralControl("<span class='form-text e-check-note'>"));
            this.Controls.Add(_lblECheckNote);
            this.Controls.Add(new LiteralControl("</span>"));

            this.Controls.Add(new LiteralControl("</div>"));

        }

        #endregion

    }
}
