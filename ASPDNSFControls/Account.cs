// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Linq;
using System.Collections;
using System.Globalization;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontCore.Validation;

namespace AspDotNetStorefrontControls
{
    /// <summary>
    /// Custom control used to handle address address information
    /// </summary>
    [ToolboxData("<{0}:Account runat=server></{0}:Account>")]
    public class Account : CompositeControl
    {

        #region Variable Declaration

        private Image _imgAccountSecurityImage = new Image();

        private Label _lblFirstName = new Label();
        private Label _lblLastName = new Label();
        private Label _lblDateOfBirth = new Label();
        private Label _lblEmail = new Label();
        private Label _lblReEnterEmail = new Label();
        private Label _lblPassword = new Label();
        private Label _lblPasswordNote = new Label();
        private Label _lblPasswordConfirm = new Label();
        private Label _lblPhone = new Label();
        private Label _lblVATRegistrationID = new Label();
        private Label _lblVATRegistrationIDInvalid = new Label();
        private Label _lblOver13 = new Label();
        private Label _lblOKToEmail = new Label();
        private Label _lblOKToEmailNote = new Label();
        private Label _lblSaveCC = new Label();
        private Label _lblSaveCCNote = new Label();
        private Label _lblSecurityCode = new Label();

        private TextBox _txtFirstName = new TextBox();
        private TextBox _txtLastName = new TextBox();
        private DropDownList _cboMonth = new DropDownList();
        private DropDownList _cboDay = new DropDownList();
        private DropDownList _cboYear = new DropDownList();
        private TextBox _txtEmail = new TextBox();
        private TextBox _txtReEnterEmail = new TextBox();
        private TextBox _txtPassword = new TextBox();
        private TextBox _txtPasswordConfirm = new TextBox();
        private TextBox _txtPhone = new TextBox();
        private TextBox _txtVATRegistrationID = new TextBox();
        private TextBox _txtSecurityCode = new TextBox();

        private RequiredFieldValidator _rfvFirstName = new RequiredFieldValidator();
        private RequiredFieldValidator _rfvLastName = new RequiredFieldValidator();
        private RequiredFieldValidator _rfvDOBMonth = new RequiredFieldValidator();
        private RequiredFieldValidator _rfvDOBDay = new RequiredFieldValidator();
        private RequiredFieldValidator _rfvDOBYear = new RequiredFieldValidator();
        private RequiredFieldValidator _rfvEmail = new RequiredFieldValidator();
        private RequiredFieldValidator _rfvReEnterEmail = new RequiredFieldValidator();
        private RequiredFieldValidator _rfvPassword = new RequiredFieldValidator();
        private RequiredFieldValidator _rfvPhone = new RequiredFieldValidator();
        private RequiredFieldValidator _rfvVATRegistrationID = new RequiredFieldValidator();
        private RequiredFieldValidator _rfvSecurityCode = new RequiredFieldValidator();

        private RegularExpressionValidator _revEmail = new RegularExpressionValidator();
        private CompareValidator _cfvEmail = new CompareValidator();

        private RegularExpressionValidator _revPhone = new RegularExpressionValidator();

        private CheckBox _chkOver13 = new CheckBox();
        private CheckBox _chkSaveCC = new CheckBox();

        private RadioButton _rbOKToEmailYes = new RadioButton();
        private RadioButton _rbOKToEmailNo = new RadioButton();

        private AspDotNetStorefrontControls.Validators.PasswordValidator _PasswordValidator = new AspDotNetStorefrontControls.Validators.PasswordValidator();
        private AspDotNetStorefrontControls.Validators.Over13Validator _Over13Validator = new AspDotNetStorefrontControls.Validators.Over13Validator();
        private AspDotNetStorefrontControls.Validators.SecurityCodeValidator _SecurityCodeValidator = new AspDotNetStorefrontControls.Validators.SecurityCodeValidator();

        private bool _ShowPassword = true;
        private bool _ShowPasswordReqVal = false;
        private bool _ShowVATRegistrationID = true;
        private bool _ShowVATRegistrationIDInvalid = false;
        private bool _ShowOver13 = true;
        private bool _ShowSaveCC = true;
        private bool _ShowSaveCCNote = false;
        private bool _ShowSecurityCode = false;
        private bool _ShowSecurityCodeValidators = false;
        private bool _ShowValidatorsInline = false;
        private bool _DisablePasswordAutocomplete = false;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Account"/> class.
        /// </summary>
        public Account()
        {
            AssignClientReferenceID();
            AssignDefaults();

            DOBMonthsDataSource();
            DOBYearDataSource();
            DOBDaysDataSource();
        }

        #endregion

        #region Properties

        #region Images

        /// <summary>
        /// Gets or sets the security image.
        /// </summary>
        /// <value>The security image.</value>
        [Browsable(true), Category("IMAGE"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string SecurityImage
        {
            get { return _imgAccountSecurityImage.ImageUrl; }
            set { _imgAccountSecurityImage.ImageUrl = value; }
        }

        #endregion

        #region Labels

        /// <summary>
        /// Gets or sets the first name caption.
        /// </summary>
        /// <value>The first name caption.</value>
        [Browsable(true), Category("LABELS")]
        public string FirstNameCaption
        {
            get { return _lblFirstName.Text; }
            set { _lblFirstName.Text = value; }
        }

        [Browsable(true), Category("LABELS")]
        public string DateOfBirthCaption
        {
            get { return _lblDateOfBirth.Text; }
            set { _lblDateOfBirth.Text = value; }
        }

        /// <summary>
        /// Gets or sets the last name caption.
        /// </summary>
        /// <value>The last name caption.</value>
        [Browsable(true), Category("LABELS")]
        public string LastNameCaption
        {
            get { return _lblLastName.Text; }
            set { _lblLastName.Text = value; }
        }

        /// <summary>
        /// Gets or sets the email caption.
        /// </summary>
        /// <value>The email caption.</value>
        [Browsable(true), Category("LABELS")]
        public string EmailCaption
        {
            get { return _lblEmail.Text; }
            set { _lblEmail.Text = value; }
        }

        [Browsable(true), Category("LABELS")]
        public string EmailReenterCaption
        {
            get { return _lblReEnterEmail.Text; }
            set { _lblReEnterEmail.Text = value; }
        }

        /// <summary>
        /// Gets or sets the password caption.
        /// </summary>
        /// <value>The password caption.</value>
        [Browsable(true), Category("LABELS")]
        public string PasswordCaption
        {
            get { return _lblPassword.Text; }
            set { _lblPassword.Text = value; }
        }

        /// <summary>
        /// Gets or sets the password note.
        /// </summary>
        /// <value>The password note.</value>
        [Browsable(true), Category("LABELS")]
        public string PasswordNote
        {
            get { return _lblPasswordNote.Text; }
            set { _lblPasswordNote.Text = value; }
        }

        /// <summary>
        /// Gets or sets the password confirm caption.
        /// </summary>
        /// <value>The password confirm caption.</value>
        [Browsable(true), Category("LABELS")]
        public string PasswordConfirmCaption
        {
            get { return _lblPasswordConfirm.Text; }
            set { _lblPasswordConfirm.Text = value; }
        }

        /// <summary>
        /// Gets or sets the phone caption.
        /// </summary>
        /// <value>The phone caption.</value>
        [Browsable(true), Category("LABELS")]
        public string PhoneCaption
        {
            get { return _lblPhone.Text; }
            set { _lblPhone.Text = value; }
        }

        /// <summary>
        /// Gets or sets the VAT registration ID caption.
        /// </summary>
        /// <value>The VAT registration ID caption.</value>
        [Browsable(true), Category("LABELS")]
        public string VATRegistrationIDCaption
        {
            get { return _lblVATRegistrationID.Text; }
            set { _lblVATRegistrationID.Text = value; }
        }

        /// <summary>
        /// Gets or sets the VAT registration ID invalid caption.
        /// </summary>
        /// <value>The VAT registration ID invalid caption.</value>
        [Browsable(true), Category("LABELS")]
        public string VATRegistrationIDInvalidCaption
        {
            get { return _lblVATRegistrationIDInvalid.Text; }
            set { _lblVATRegistrationIDInvalid.Text = value; }
        }

        /// <summary>
        /// Gets or sets the over13 caption.
        /// </summary>
        /// <value>The over13 caption.</value>
        [Browsable(true), Category("LABELS")]
        public string Over13Caption
        {
            get { return _lblOver13.Text; }
            set { _lblOver13.Text = value; }
        }

        /// <summary>
        /// Gets or sets the OK to email caption.
        /// </summary>
        /// <value>The OK to email caption.</value>
        [Browsable(true), Category("LABELS")]
        public string OKToEmailCaption
        {
            get { return _lblOKToEmail.Text; }
            set { _lblOKToEmail.Text = value; }
        }

        /// <summary>
        /// Gets or sets the OK to email note.
        /// </summary>
        /// <value>The OK to email note.</value>
        [Browsable(true), Category("LABELS")]
        public string OKToEmailNote
        {
            get { return _lblOKToEmailNote.Text; }
            set { _lblOKToEmailNote.Text = value; }
        }

        /// <summary>
        /// Gets or sets the save Credit Card caption.
        /// </summary>
        /// <value>The save CC caption.</value>
        [Browsable(true), Category("LABELS")]
        public string SaveCCCaption
        {
            get { return _lblSaveCC.Text; }
            set { _lblSaveCC.Text = value; }
        }

        /// <summary>
        /// Gets or sets the save Credit Card note.
        /// </summary>
        /// <value>The save CC note.</value>
        [Browsable(true), Category("LABELS")]
        public string SaveCCNote
        {
            get { return _lblSaveCCNote.Text; }
            set { _lblSaveCCNote.Text = value; }
        }

        /// <summary>
        /// Gets or sets the OK to email yes caption.
        /// </summary>
        /// <value>The OK to email yes caption.</value>
        [Browsable(true), Category("LABELS")]
        public string OKToEmailYesCaption
        {
            get { return _rbOKToEmailYes.Text; }
            set { _rbOKToEmailYes.Text = value; }
        }

        /// <summary>
        /// Gets or sets the OK to email no caption.
        /// </summary>
        /// <value>The OK to email no caption.</value>
        [Browsable(true), Category("LABELS")]
        public string OKToEmailNoCaption
        {
            get { return _rbOKToEmailNo.Text; }
            set { _rbOKToEmailNo.Text = value; }
        }

        /// <summary>
        /// Gets or sets the security code caption.
        /// </summary>
        /// <value>The security code caption.</value>
        [Browsable(true), Category("LABELS")]
        public string SecurityCodeCaption
        {
            get { return _lblSecurityCode.Text; }
            set { _lblSecurityCode.Text = value; }
        }

        #endregion

        #region Inputs

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

        [Browsable(false)]
        public string DOBMonth
        {
            get { return _cboMonth.SelectedValue; }
            set { _cboMonth.SelectedValue = value; }
        }

        [Browsable(false)]
        public string DOBDay
        {
            get { return _cboDay.SelectedValue; }
            set { _cboDay.SelectedValue = value; }
        }

        [Browsable(false)]
        public string DOBYear
        {
            get { return _cboYear.SelectedValue; }
            set { _cboYear.SelectedValue = value; }
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
        /// Gets or sets the email.
        /// </summary>
        /// <value>The email.</value>
        [Browsable(false)]
        public string Email
        {
            get { return _txtEmail.Text; }
            set { _txtEmail.Text = value; }
        }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>The password.</value>
        [Browsable(false)]
        public string Password
        {
            get { return _txtPassword.Text; }
            set { _txtPassword.Text = value; }
        }

        /// <summary>
        /// Gets or sets the password confirm.
        /// </summary>
        /// <value>The password confirm.</value>
        [Browsable(false)]
        public string PasswordConfirm
        {
            get { return _txtPasswordConfirm.Text; }
            set { _txtPasswordConfirm.Text = value; }
        }

        /// <summary>
        /// Gets or sets the phone number.
        /// </summary>
        /// <value>The phone number.</value>
        [Browsable(false)]
        public string Phone
        {
            get { return _txtPhone.Text; }
            set { _txtPhone.Text = value; }
        }

        /// <summary>
        /// Gets or sets the VAT registration ID.
        /// </summary>
        /// <value>The VAT registration ID.</value>
        [Browsable(false)]
        public string VATRegistrationID
        {
            get { return _txtVATRegistrationID.Text; }
            set { _txtVATRegistrationID.Text = value; }
        }

        /// <summary>
        /// Gets or sets the security code.
        /// </summary>
        /// <value>The security code.</value>
        [Browsable(false)]
        public string SecurityCode
        {
            get { return _txtSecurityCode.Text; }
            set { _txtSecurityCode.Text = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Account"/> is over13.
        /// </summary>
        /// <value><c>true</c> if over13; otherwise, <c>false</c>.</value>
        [Browsable(false)]
        public bool Over13
        {
            get { return _chkOver13.Checked; }
            set { _chkOver13.Checked = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [save Credit Card].
        /// </summary>
        /// <value><c>true</c> if [save Creadit Card]; otherwise, <c>false</c>.</value>
        [Browsable(false)]
        public bool SaveCC
        {
            get { return _chkSaveCC.Checked; }
            set { _chkSaveCC.Checked = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [OK to email yes].
        /// </summary>
        /// <value><c>true</c> if [OK to email yes]; otherwise, <c>false</c>.</value>
        [Browsable(false)]
        public bool OKToEmailYes
        {
            get { return _rbOKToEmailYes.Checked; }
            set { _rbOKToEmailYes.Checked = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [OK to email no].
        /// </summary>
        /// <value><c>true</c> if [OK to email no]; otherwise, <c>false</c>.</value>
        [Browsable(false)]
        public bool OKToEmailNo
        {
            get { return _rbOKToEmailNo.Checked; }
            set { _rbOKToEmailNo.Checked = value; }
        }

        #endregion

        #region Settings

        [Browsable(true), Category("SETTINGS")]
        public Boolean DisablePasswordAutocomplete
        {
            get { return _DisablePasswordAutocomplete; }
            set
            {
                _DisablePasswordAutocomplete = value;
                if (value)
                {
                    DisableAutocomplete(_txtPassword);
                    DisableAutocomplete(_txtPasswordConfirm);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show password].
        /// </summary>
        /// <value><c>true</c> if [show password]; otherwise, <c>false</c>.</value>
        [Browsable(true), Category("SETTINGS")]
        public bool ShowPassword
        {
            get { return _ShowPassword; }
            set { _ShowPassword = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show password required field validator].
        /// </summary>
        /// <value><c>true</c> if [show password required field validator]; otherwise, <c>false</c>.</value>
        [Browsable(true), Category("SETTINGS")]
        public bool ShowPasswordReqVal
        {
            get { return _ShowPasswordReqVal; }
            set { _ShowPasswordReqVal = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show VAT registration ID].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [show VAT registration ID]; otherwise, <c>false</c>.
        /// </value>
        [Browsable(true), Category("SETTINGS")]
        public bool ShowVATRegistrationID
        {
            get { return _ShowVATRegistrationID; }
            set { _ShowVATRegistrationID = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show VAT registration ID invalid].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [show VAT registration ID invalid]; otherwise, <c>false</c>.
        /// </value>
        [Browsable(true), Category("SETTINGS")]
        public bool ShowVATRegistrationIDInvalid
        {
            get { return _ShowVATRegistrationIDInvalid; }
            set { _ShowVATRegistrationIDInvalid = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show over13].
        /// </summary>
        /// <value><c>true</c> if [show over13]; otherwise, <c>false</c>.</value>
        [Browsable(true), Category("SETTINGS")]
        public bool ShowOver13
        {
            get { return _ShowOver13; }
            set { _ShowOver13 = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show save Credit Card].
        /// </summary>
        /// <value><c>true</c> if [show save Credit Card]; otherwise, <c>false</c>.</value>
        [Browsable(true), Category("SETTINGS")]
        public bool ShowSaveCC
        {
            get { return _ShowSaveCC; }
            set { _ShowSaveCC = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show save Credit Card note].
        /// </summary>
        /// <value><c>true</c> if [show save Credit Card note]; otherwise, <c>false</c>.</value>
        [Browsable(true), Category("SETTINGS")]
        public bool ShowSaveCCNote
        {
            get { return _ShowSaveCCNote; }
            set { _ShowSaveCCNote = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show security code].
        /// </summary>
        /// <value><c>true</c> if [show security code]; otherwise, <c>false</c>.</value>
        [Browsable(true), Category("SETTINGS")]
        public bool ShowSecurityCode
        {
            get { return _ShowSecurityCode; }
            set { _ShowSecurityCode = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show security code validators].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [show security code validators]; otherwise, <c>false</c>.
        /// </value>
        [Browsable(true), Category("SETTINGS")]
        public bool ShowSecurityCodeValidators
        {
            get { return _ShowSecurityCodeValidators; }
            set { _ShowSecurityCodeValidators = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show validators inline].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [show validators inline]; otherwise, <c>false</c>.
        /// </value>
        [Browsable(true), Category("SETTINGS")]
        public bool ShowValidatorsInline
        {
            get { return _ShowValidatorsInline; }
            set { _ShowValidatorsInline = value; }
        }

        #endregion

        #region Textbox
        /// <summary>
        /// Gets or sets the property of textbox first name
        /// </summary>
        /// <value>The property of textbox first name</value>
        [Browsable(false), Category("TEXTBOX")]
        public TextBox txtFirstName
        {
            get { return _txtFirstName; }
            set { _txtFirstName = value; }
        }

        /// <summary>
        /// Gets or sets the property of textbox last name
        /// </summary>
        /// <value>The property of textbox last name</value>
        [Browsable(false), Category("TEXTBOX")]
        public TextBox txtLastName
        {
            get { return _txtLastName; }
            set { _txtLastName = value; }
        }

        [Browsable(false), Category("DROPDOWNLIST")]
        public DropDownList ddMonth
        {
            get { return _cboMonth; }
            set { _cboMonth = value; }
        }

        [Browsable(false), Category("DROPDOWNLIST")]
        public DropDownList ddDay
        {
            get { return _cboDay; }
            set { _cboDay = value; }
        }

        [Browsable(false), Category("DROPDOWNLIST")]
        public DropDownList ddYear
        {
            get { return _cboYear; }
            set { _cboYear = value; }
        }


        /// <summary>
        /// Gets or sets the property of textbox password.
        /// </summary>
        /// <value>The property of textbox password.</value>
        [Browsable(false), Category("TEXTBOX")]
        public TextBox txtPassword
        {
            get { return _txtPassword; }
            set { _txtPassword = value; }
        }

        /// <summary>
        /// Gets or sets the property of textbox password confirm.
        /// </summary>
        /// <value>The property of textbox password confirm.</value>
        [Browsable(false), Category("TEXTBOX")]
        public TextBox txtPasswordConfirm
        {
            get { return _txtPasswordConfirm; }
            set { _txtPasswordConfirm = value; }
        }

        /// <summary>
        /// Gets or sets the property of textbox security code
        /// </summary>
        [Browsable(false), Category("TEXTBOX")]
        public TextBox txtSecurityCode
        {
            get { return _txtSecurityCode; }
            set { _txtSecurityCode = value; }
        }

        /// <summary>
        /// Gets or sets the property of label security code
        /// </summary>
        [Browsable(false), Category("LABEL")]
        public Label lblSecurityCode
        {
            get { return _lblSecurityCode; }
            set { _lblSecurityCode = value; }
        }

        /// <summary>
        /// Gets or sets the property of security image
        /// </summary>
        [Browsable(false), Category("IMAGE")]
        public Image imgAccountSecurityImage
        {
            get { return _imgAccountSecurityImage; }
            set { _imgAccountSecurityImage = value; }
        }

        #endregion

        #region Validators

        /// <summary>
        /// Gets or sets the first name required field error message.
        /// </summary>
        /// <value>The first name required field error message.</value>
        [Browsable(true), Category("ERROR_MESSAGE")]
        public string FirstNameReqFieldErrorMessage
        {
            get { return _rfvFirstName.ErrorMessage; }
            set { _rfvFirstName.ErrorMessage = value; }
        }

        /// <summary>
        /// Gets or sets the last name required field error message.
        /// </summary>
        /// <value>The last name required field error message.</value>
        [Browsable(true), Category("ERROR_MESSAGE")]
        public string LastNameReqFieldErrorMessage
        {
            get { return _rfvLastName.ErrorMessage; }
            set { _rfvLastName.ErrorMessage = value; }
        }

        /// <summary>
        /// Gets or sets the day of birth name required field error message.
        /// </summary>
        /// <value>The last name required field error message.</value>
        [Browsable(true), Category("ERROR_MESSAGE")]
        public string DayOfBirthReqFieldErrorMessage
        {
            get { return _rfvDOBDay.ErrorMessage; }
            set { _rfvDOBDay.ErrorMessage = value; }
        }

        /// <summary>
        /// Gets or sets the day of birth name required field error message.
        /// </summary>
        /// <value>The last name required field error message.</value>
        [Browsable(true), Category("ERROR_MESSAGE")]
        public string MonthOfBirthReqFieldErrorMessage
        {
            get { return _rfvDOBMonth.ErrorMessage; }
            set { _rfvDOBMonth.ErrorMessage = value; }
        }

        /// <summary>
        /// Gets or sets the day of birth name required field error message.
        /// </summary>
        /// <value>The last name required field error message.</value>
        [Browsable(true), Category("ERROR_MESSAGE")]
        public string YearOfBirthReqFieldErrorMessage
        {
            get { return _rfvDOBYear.ErrorMessage; }
            set { _rfvDOBYear.ErrorMessage = value; }
        }

        /// <summary>
        /// Gets or sets the email required field error message.
        /// </summary>
        /// <value>The email required field error message.</value>
        [Browsable(true), Category("ERROR_MESSAGE")]
        public string EmailReqFieldErrorMessage
        {
            get { return _rfvEmail.ErrorMessage; }
            set { _rfvEmail.ErrorMessage = value; }
        }

        /// <summary>
        /// Gets or sets the email confirmation comparison failed error message.
        /// </summary>
        /// <value>The email required field error message.</value>
        [Browsable(true), Category("ERROR_MESSAGE")]
        public string EmailCompareFieldErrorMessage
        {
            get { return _cfvEmail.ErrorMessage; }
            set { _cfvEmail.ErrorMessage = value; }
        }

        /// <summary>
        /// Gets or sets the email confirmation required error message.
        /// </summary>
        /// <value>The email required field error message.</value>
        [Browsable(true), Category("ERROR_MESSAGE")]
        public string ReEnterEmailReqFieldErrorMessage
        {
            get { return _rfvReEnterEmail.ErrorMessage; }
            set { _rfvReEnterEmail.ErrorMessage = value; }
        }

        /// <summary>
        /// Gets or sets the password required field error message.
        /// </summary>
        /// <value>The password required field error message.</value>
        [Browsable(true), Category("ERROR_MESSAGE")]
        public string PasswordReqFieldErrorMessage
        {
            get { return _rfvPassword.ErrorMessage; }
            set { _rfvPassword.ErrorMessage = value; }
        }

        /// <summary>
        /// Gets or sets the phone required field error message.
        /// </summary>
        /// <value>The phone required field error message.</value>
        [Browsable(true), Category("ERROR_MESSAGE")]
        public string PhoneReqFieldErrorMessage
        {
            get { return _rfvPhone.ErrorMessage; }
            set { _rfvPhone.ErrorMessage = value; }
        }

        /// <summary>
        /// Gets or sets the security code required field error message.
        /// </summary>
        /// <value>The security code required field error message.</value>
        [Browsable(true), Category("ERROR_MESSAGE")]
        public string SecurityCodeReqFieldErrorMessage
        {
            get { return _rfvSecurityCode.ErrorMessage; }
            set { _rfvSecurityCode.ErrorMessage = value; }
        }

        /// <summary>
        /// Gets or sets the email reg ex error message.
        /// </summary>
        /// <value>The email reg ex error message.</value>
        [Browsable(true), Category("ERROR_MESSAGE")]
        public string EmailRegExErrorMessage
        {
            get { return _revEmail.ErrorMessage; }
            set { _revEmail.ErrorMessage = value; }
        }

        /// <summary>
        /// Gets or sets the phone reg ex error message.
        /// </summary>
        /// <value>The phone reg ex error message.</value>
        [Browsable(true), Category("ERROR_MESSAGE")]
        public string PhoneRegExErrorMessage
        {
            get { return _revPhone.ErrorMessage; }
            set { _revPhone.ErrorMessage = value; }
        }

        /// <summary>
        /// Gets or sets the first name required field validation group.
        /// </summary>
        /// <value>The first name required field validation group.</value>
        [Browsable(true), Category("VALIDATION_GROUP")]
        public string FirstNameReqFieldValGrp
        {
            get { return _rfvFirstName.ValidationGroup; }
            set { _rfvFirstName.ValidationGroup = value; }
        }

        /// <summary>
        /// Gets or sets the last name required field validation group.
        /// </summary>
        /// <value>The last name required field validation group.</value>
        [Browsable(true), Category("VALIDATION_GROUP")]
        public string LastNameReqFieldValGrp
        {
            get { return _rfvLastName.ValidationGroup; }
            set { _rfvLastName.ValidationGroup = value; }
        }

        /// <summary>
        /// Gets or sets the last name required field validation group.
        /// </summary>
        /// <value>The last name required field validation group.</value>
        [Browsable(true), Category("VALIDATION_GROUP")]
        public string DayOfBirthReqFieldValGrp
        {
            get { return _rfvDOBDay.ValidationGroup; }
            set { _rfvDOBDay.ValidationGroup = value; }
        }

        /// <summary>
        /// Gets or sets the last name required field validation group.
        /// </summary>
        /// <value>The last name required field validation group.</value>
        [Browsable(true), Category("VALIDATION_GROUP")]
        public string MonthOfBirthReqFieldValGrp
        {
            get { return _rfvDOBMonth.ValidationGroup; }
            set { _rfvDOBMonth.ValidationGroup = value; }
        }

        /// <summary>
        /// Gets or sets the last name required field validation group.
        /// </summary>
        /// <value>The last name required field validation group.</value>
        [Browsable(true), Category("VALIDATION_GROUP")]
        public string YearOfBirthReqFieldValGrp
        {
            get { return _rfvDOBYear.ValidationGroup; }
            set { _rfvDOBYear.ValidationGroup = value; }
        }

        /// <summary>
        /// Gets or sets the email required field validation group.
        /// </summary>
        /// <value>The email required field validation group.</value>
        [Browsable(true), Category("VALIDATION_GROUP")]
        public string EmailReqFieldValGrp
        {
            get { return _rfvEmail.ValidationGroup; }
            set { _rfvEmail.ValidationGroup = value; }
        }

        /// <summary>
        /// Gets or sets the email confirmation failed validation group.
        /// </summary>
        /// <value>The email required field validation group.</value>
        [Browsable(true), Category("VALIDATION_GROUP")]
        public string EmailCompareFieldValGrp
        {
            get { return _cfvEmail.ValidationGroup; }
            set { _cfvEmail.ValidationGroup = value; }
        }

        /// <summary>
        /// Gets or sets the email confirmation required field validation group.
        /// </summary>
        /// <value>The email required field validation group.</value>
        [Browsable(true), Category("VALIDATION_GROUP")]
        public string ReEnterEmailReqFieldValGrp
        {
            get { return _rfvReEnterEmail.ValidationGroup; }
            set { _rfvReEnterEmail.ValidationGroup = value; }
        }

        /// <summary>
        /// Gets or sets the password required field validation group.
        /// </summary>
        /// <value>The password required field validation group.</value>
        [Browsable(true), Category("VALIDATION_GROUP")]
        public string PasswordReqFieldValGrp
        {
            get { return _rfvPassword.ValidationGroup; }
            set { _rfvPassword.ValidationGroup = value; }
        }

        /// <summary>
        /// Gets or sets the phone required field validation group.
        /// </summary>
        /// <value>The phone required field validation group.</value>
        [Browsable(true), Category("VALIDATION_GROUP")]
        public string PhoneReqFieldValGrp
        {
            get { return _rfvPhone.ValidationGroup; }
            set { _rfvPhone.ValidationGroup = value; }
        }

        /// <summary>
        /// Gets or sets the security code required field validation group.
        /// </summary>
        /// <value>The security code required field validation group.</value>
        [Browsable(true), Category("VALIDATION_GROUP")]
        public string SecurityCodeReqFieldValGrp
        {
            get { return _rfvSecurityCode.ValidationGroup; }
            set { _rfvSecurityCode.ValidationGroup = value; }
        }

        /// <summary>
        /// Gets or sets the email reg ex validation group.
        /// </summary>
        /// <value>The email reg ex validation group.</value>
        [Browsable(true), Category("VALIDATION_GROUP")]
        public string EmailRegExValGrp
        {
            get { return _revEmail.ValidationGroup; }
            set { _revEmail.ValidationGroup = value; }
        }

        /// <summary>
        /// Gets or sets the phone reg ex validation group.
        /// </summary>
        /// <value>The phone reg ex validation group.</value>
        [Browsable(true), Category("VALIDATION_GROUP")]
        public string PhoneRegExValGrp
        {
            get { return _revPhone.ValidationGroup; }
            set { _revPhone.ValidationGroup = value; }
        }

        /// <summary>
        /// Gets or sets the password validate validation group.
        /// </summary>
        /// <value>The password validate validation group.</value>
        [Browsable(true), Category("VALIDATION_GROUP")]
        public string PasswordValidateValGrp
        {
            get { return _PasswordValidator.ValidationGroup; }
            set { _PasswordValidator.ValidationGroup = value; }
        }

        /// <summary>
        /// Gets or sets the over13 validation group.
        /// </summary>
        /// <value>The over13 validation group.</value>
        [Browsable(true), Category("VALIDATION_GROUP")]
        public string Over13ValGrp
        {
            get { return _Over13Validator.ValidationGroup; }
            set { _Over13Validator.ValidationGroup = value; }
        }

        /// <summary>
        /// Gets or sets the security code validation group.
        /// </summary>
        /// <value>The security code validation group.</value>
        [Browsable(true), Category("VALIDATION_GROUP")]
        public string SecurityCodeValGrp
        {
            get { return _SecurityCodeValidator.ValidationGroup; }
            set { _SecurityCodeValidator.ValidationGroup = value; }
        }

        /// <summary>
        /// Gets or sets the password validate.
        /// </summary>
        /// <value>The password validate.</value>
        [Browsable(false)]
        public string PasswordValidate
        {
            get { return _PasswordValidator.Password; }
            set { _PasswordValidator.Password = value; }
        }

        /// <summary>
        /// Gets or sets the password confirm validate.
        /// </summary>
        /// <value>The password confirm validate.</value>
        [Browsable(false)]
        public string PasswordConfirmValidate
        {
            get { return _PasswordValidator.PasswordConfirm; }
            set { _PasswordValidator.PasswordConfirm = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [over13 check].
        /// </summary>
        /// <value><c>true</c> if [over13 check]; otherwise, <c>false</c>.</value>
        [Browsable(false)]
        public string RequireOver13ErrorMessage
        {
            get { return _Over13Validator.ErrorMessage; }
            set { _Over13Validator.ErrorMessage = value; }
        }

        /// <summary>
        /// Gets or sets the password required field validator.
        /// </summary>
        /// <value>The password required field validator.</value>
        [Browsable(false)]
        public RequiredFieldValidator PasswordReqFieldValidator
        {
            get { return _rfvPassword; }
            set { _rfvPassword = value; }
        }

        /// <summary>
        /// Gets or sets the password validator.
        /// </summary>
        /// <value>The password validator.</value>
        [Browsable(false)]
        public AspDotNetStorefrontControls.Validators.PasswordValidator PasswordValidator
        {
            get { return _PasswordValidator; }
            set { _PasswordValidator = value; }
        }

        /// <summary>
        /// Gets or sets the email regular expression.
        /// </summary>
        /// <value>The email regular expression.</value>
        [Browsable(true), Category("VALIDATION_EXPRESSION")]
        public string EmailRegEx
        {
            get { return _revEmail.ValidationExpression; }
            set { _revEmail.ValidationExpression = value; }
        }

        /// <summary>
        /// Gets or sets the phone regular expression.
        /// </summary>
        /// <value>The phone regular expression.</value>
        [Browsable(true), Category("VALIDATION_EXPRESSION")]
        public string PhoneRegEx
        {
            get { return _revPhone.ValidationExpression; }
            set { _revPhone.ValidationExpression = value; }
        }

        #endregion



        #endregion

        #region Methods

        public void DOBDaysDataSource()
        {
            _cboDay.Items.Clear();
            for (int i = 1; i <= 31; i++)
            {
                _cboDay.Items.Add(new ListItem(i.ToString(), i.ToString()));
            }

            _cboDay.Items.Insert(0, new ListItem("Day", "0"));
        }

        public void DOBYearDataSource()
        {
            _cboYear.Items.Clear();
            for (int i = System.DateTime.Now.Year; i >= 1900; i--)
            {
                _cboYear.Items.Add(new ListItem(i.ToString(), i.ToString()));
            }
            _cboYear.Items.Insert(0, new ListItem("Year", "0"));
        }

        public void DOBMonthsDataSource()
        {
            _cboMonth.Items.Clear();
            string[] months = new string[11];
            months = CultureInfo.CurrentCulture.DateTimeFormat.MonthNames;

            for (int i = 0; i < months.Length; i++)
            {
                if (!string.IsNullOrEmpty(months[i].ToString()))
                {
                    _cboMonth.Items.Add(new ListItem(months[i], (i + 1).ToString()));
                }
            }
            _cboMonth.Items.Insert(0, new ListItem("Month", "0"));
        }

        /// <summary>
        /// Assigns the client reference ID.
        /// </summary>
        private void AssignClientReferenceID()
        {
            _txtFirstName.ID = "txtFirstName";
            _txtLastName.ID = "txtLastName";
            _cboMonth.ID = "ddMonth";
            _cboDay.ID = "ddDay";
            _cboYear.ID = "ddYear";
            _txtEmail.ID = "txtEmail";
            _txtReEnterEmail.ID = "txtReEnterEmail";
            _txtPassword.ID = "txtPassword";
            _txtPasswordConfirm.ID = "txtPasswordConfirm";
            _txtPhone.ID = "txtPhone";
            _txtVATRegistrationID.ID = "txtAccountVAT";
            _txtSecurityCode.ID = "txtSecurityCode";

            _chkOver13.ID = "chkOver13";
            _chkSaveCC.ID = "chkSaveCC";

            _txtPassword.CssClass = "form-control";
            _txtPasswordConfirm.CssClass = "form-control";
            _txtFirstName.CssClass = "form-control";
            _txtLastName.CssClass = "form-control";
            _txtEmail.CssClass = "form-control";
            _txtPhone.CssClass = "form-control";
            _txtReEnterEmail.CssClass = "form-control"; 
        }

        public static void DisableAutocomplete(TextBox tb)
        {
            if (tb == null)
                return;

            tb.Attributes.Add("autocomplete", "off");
            tb.AutoCompleteType = AutoCompleteType.Disabled;
        }

        /// <summary>
        /// Assigns the defaults.
        /// </summary>
        private void AssignDefaults()
        {
            _txtPassword.TextMode = TextBoxMode.Password;
            _txtPasswordConfirm.TextMode = TextBoxMode.Password;

            if (DisablePasswordAutocomplete)
            {
                DisableAutocomplete(_txtPassword);
                DisableAutocomplete(_txtPasswordConfirm);
            }

            _txtFirstName.MaxLength = 50;
            _txtLastName.MaxLength = 50;
            _txtEmail.MaxLength = 100;
            _txtReEnterEmail.MaxLength = 100;
            _txtPassword.MaxLength = 50;
            _txtPasswordConfirm.MaxLength = 50;
            _txtPhone.MaxLength = 25;
            _txtVATRegistrationID.MaxLength = 20;

            _rfvFirstName.ControlToValidate = _txtFirstName.ID;
            _rfvLastName.ControlToValidate = _txtLastName.ID;
            _rfvEmail.ControlToValidate = _txtEmail.ID;
            _cfvEmail.ControlToValidate = _txtReEnterEmail.ID;
            _cfvEmail.ControlToCompare = _txtEmail.ID;
            _rfvReEnterEmail.ControlToValidate = _txtReEnterEmail.ID;
            _rfvPassword.ControlToValidate = _txtPassword.ID;
            _rfvPassword.ID = "rfvPassword";
            _rfvPhone.ControlToValidate = _txtPhone.ID;
            _rfvSecurityCode.ControlToValidate = _txtSecurityCode.ID;

            _revEmail.ControlToValidate = _txtEmail.ID;
            _revPhone.ControlToValidate = _txtPhone.ID;

            _rfvDOBDay.ControlToValidate = _cboDay.ID;
            _rfvDOBMonth.ControlToValidate = _cboMonth.ID;
            _rfvDOBYear.ControlToValidate = _cboYear.ID;

            _rfvDOBDay.InitialValue = "0";
            _rfvDOBMonth.InitialValue = "0";
            _rfvDOBYear.InitialValue = "0";

            _PasswordValidator.ID = "PasswordValidator";
            _PasswordValidator.ControlToValidate = _txtPassword.ID;
            //_Over13Validator.ControlToValidate = _chkOver13.ID; // _txtPassword.ID;
            _Over13Validator.ControlInstanceToValidate = _chkOver13;
            _SecurityCodeValidator.ControlToValidate = _txtSecurityCode.ID;

            _rbOKToEmailYes.GroupName = "OKToEmail";
            _rbOKToEmailNo.GroupName = "OKToEmail";

            _rbOKToEmailYes.ID = "rbOKToEmailYes";
            _rbOKToEmailNo.ID = "rbOKToEmailNo";

            _rfvFirstName.Display = ValidatorDisplay.Dynamic;
            _rfvLastName.Display = ValidatorDisplay.Dynamic;
            _rfvEmail.Display = ValidatorDisplay.Dynamic;
            _rfvReEnterEmail.Display = ValidatorDisplay.Dynamic;
            _rfvPassword.Display = ValidatorDisplay.Dynamic;
            _rfvPhone.Display = ValidatorDisplay.Dynamic;
            _rfvSecurityCode.Display = ValidatorDisplay.Dynamic;
            _revEmail.Display = ValidatorDisplay.Dynamic;
            _revPhone.Display = ValidatorDisplay.Dynamic;
            _rfvDOBDay.Display = ValidatorDisplay.Dynamic;
            _rfvDOBMonth.Display = ValidatorDisplay.Dynamic;
            _rfvDOBYear.Display = ValidatorDisplay.Dynamic;

            _revEmail.ValidationExpression = new EmailAddressValidator().GetValidationRegExString();
            _revPhone.ValidationExpression = new PhoneValidator().GetValidationRegExString();
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

            #region "First Column"
           

       //     this.Controls.Add(new LiteralControl("<div class='col-md-5'>"));

            // [Start] - First Name
            this.Controls.Add(new LiteralControl("<div class='form-group'>"));
            this.Controls.Add(new LiteralControl("<label>"));
            this.Controls.Add(_lblFirstName);
            this.Controls.Add(new LiteralControl("</label>"));
            this.Controls.Add(_txtFirstName);
            if (!_ShowValidatorsInline)
            {
                _rfvFirstName.Display = ValidatorDisplay.None;
            }
            this.Controls.Add(_rfvFirstName);
            this.Controls.Add(new LiteralControl("</div>"));
            // [End] - First Name

            // [Start] - Last Name
            this.Controls.Add(new LiteralControl("<div class='form-group'>"));
            this.Controls.Add(new LiteralControl("<label>"));
            this.Controls.Add(_lblLastName);
            this.Controls.Add(new LiteralControl("</label>"));
            this.Controls.Add(_txtLastName);
            if (!_ShowValidatorsInline)
            {
                _rfvLastName.Display = ValidatorDisplay.None;
            }
            this.Controls.Add(_rfvLastName);
            this.Controls.Add(new LiteralControl("</div>"));
            // [End] - Last Name

            // [Start] - Email
            this.Controls.Add(new LiteralControl("<div class='form-group'>"));
            this.Controls.Add(new LiteralControl("<label>"));
            this.Controls.Add(_lblEmail);
            this.Controls.Add(new LiteralControl("</label>"));

            this.Controls.Add(_txtEmail);
            _rfvEmail.Display = ValidatorDisplay.Dynamic;
            _revEmail.Display = ValidatorDisplay.Dynamic;
            if (!_ShowValidatorsInline)
            {
                _rfvEmail.Display = ValidatorDisplay.None;
                _revEmail.Display = ValidatorDisplay.None;
            }
            this.Controls.Add(_rfvEmail);
            this.Controls.Add(_revEmail);
            this.Controls.Add(new LiteralControl("</div>"));
            // [End] - Email

            // [Start] - Phone
            this.Controls.Add(new LiteralControl("<div class='form-group'>"));
            this.Controls.Add(new LiteralControl("<label>"));
            this.Controls.Add(_lblPhone);
            this.Controls.Add(new LiteralControl("</label>"));
            this.Controls.Add(_txtPhone);
            if (!_ShowValidatorsInline)
            {
                _rfvPhone.Display = ValidatorDisplay.None;
                _revPhone.Display = ValidatorDisplay.None;
            }
            this.Controls.Add(_rfvPhone);
            this.Controls.Add(_revPhone);
            this.Controls.Add(new LiteralControl("</div>"));
            // [End] - Phone

           // this.Controls.Add(new LiteralControl("</div>"));

            #endregion

            #region "Second Column"
            this.Controls.Add(new LiteralControl("<div class='col-md-5 pull-right'>"));

            //// [Start] - Phone
            //this.Controls.Add(new LiteralControl("<div class='form-group'>"));
            //this.Controls.Add(new LiteralControl("<label>"));
            //this.Controls.Add(_lblPhone);
            //this.Controls.Add(new LiteralControl("</label>"));
            //this.Controls.Add(_txtPhone);
            //if (!_ShowValidatorsInline)
            //{
            //    _rfvPhone.Display = ValidatorDisplay.None;
            //    _revPhone.Display = ValidatorDisplay.None;
            //}
            //this.Controls.Add(_rfvPhone);
            //this.Controls.Add(_revPhone);
            //this.Controls.Add(new LiteralControl("</div>"));
            //// [End] - Phone

            if (_ShowPassword)
            {
                // [Start] - Password
                this.Controls.Add(new LiteralControl("<div class='form-group'>"));
                this.Controls.Add(new LiteralControl("<label>"));
                this.Controls.Add(_lblPassword);
                this.Controls.Add(new LiteralControl("</label>"));
                this.Controls.Add(_txtPassword);
                if (!_ShowValidatorsInline)
                {
                    _rfvPassword.Display = ValidatorDisplay.None;
                }
                _PasswordValidator.Display = ValidatorDisplay.None;
                if (_ShowPasswordReqVal)
                {
                    this.Controls.Add(_rfvPassword);
                }
                this.Controls.Add(_PasswordValidator);
                this.Controls.Add(new LiteralControl("</div>"));
                // [End] - Password

                // [Start] - Confirm Password
                this.Controls.Add(new LiteralControl("<div class='form-group'>"));
                this.Controls.Add(new LiteralControl("<label>"));
                this.Controls.Add(_lblPasswordConfirm);
                this.Controls.Add(new LiteralControl("</label>"));

                this.Controls.Add(_txtPasswordConfirm);
                this.Controls.Add(new LiteralControl("</div>"));
                // [End] - Confirm Password
            }
            this.Controls.Add(new LiteralControl("</div>"));

            #endregion

        }


        #endregion

    }
}
