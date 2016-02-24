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
using System.Drawing.Design;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontControls
{
    /// <summary>
    /// Custom control used to display available payment methods
    /// </summary>
    [ToolboxData("<{0}:PaymentMethod runat=server></{0}:PaymentMethod>")]
    public class PaymentMethod : CompositeControl
    {

        #region Variable Declaration

        private RadioButton _rbCREDITCARD = new RadioButton();
        private RadioButton _rbPURCHASEORDER = new RadioButton();
        private RadioButton _rbCODMONEYORDER = new RadioButton();
        private RadioButton _rbCODCOMPANYCHECK = new RadioButton();
        private RadioButton _rbCODNET30 = new RadioButton();
        private RadioButton _rbPAYPAL = new RadioButton();
        private RadioButton _rbREQUESTQUOTE = new RadioButton();
        private RadioButton _rbCHECKBYMAIL = new RadioButton();
        private RadioButton _rbCOD = new RadioButton();
        private RadioButton _rbECHECK = new RadioButton();
        private RadioButton _rbCARDINALMYECHECK = new RadioButton();
        private RadioButton _rbMICROPAY = new RadioButton();
        private RadioButton _rbPAYPALEXPRESS = new RadioButton();
        private RadioButton _rbAMAZONSIMPLEPAY = new RadioButton();
        private RadioButton _rbMONEYBOOKERSQUICKCHECKOUT = new RadioButton();
        private RadioButton _rbSECURENETVAULT = new RadioButton();
        private RadioButton _rbPAYPALEMBEDDEDCHECKOUT = new RadioButton();

        private Image _imgCREDITCARD_AmericanExpress = new Image();
        private Image _imgCREDITCARD_Discover = new Image();
        private Image _imgCREDITCARD_MasterCard = new Image();
        private Image _imgCREDITCARD_Visa = new Image();
        private Image _imgCREDITCARD_Laser = new Image();
        private Image _imgCREDITCARD_Maestro = new Image();
        private Image _imgCREDITCARD_VisaDebit = new Image();
        private Image _imgCREDITCARD_VisaElectron = new Image();
        private Image _imgCREDITCARD_Jcb = new Image();
        private Image _imgCREDITCARD_Diners = new Image();
        private Image _imgPURCHASEORDER = new Image();
        private Image _imgCODMONEYORDER = new Image();
        private Image _imgCODCOMPANYCHECK = new Image();
        private Image _imgCODNET30 = new Image();
        private Image _imgPAYPAL = new Image();
        private Image _imgREQUESTQUOTE = new Image();
        private Image _imgCHECKBYMAIL = new Image();
        private Image _imgCOD = new Image();
        private Image _imgECHECK = new Image();
        private Image _imgCARDINALMYECHECK = new Image();
        private Image _imgMICROPAY = new Image();
        private Image _imgPAYPALEXPRESS = new Image();
        private Image _imgAMAZONSIMPLEPAY = new Image();
        private Image _imgMONEYBOOKERSQUICKCHECKOUT = new Image();
        private Image _imgSECURENETVAULT = new Image();
        private Image _imgPAYPALEMBEDDEDCHECKOUT = new Image();

        private Label _lblCREDITCARD = new Label();
        private Label _lblPURCHASEORDER = new Label();
        private Label _lblCODMONEYORDER = new Label();
        private Label _lblCODCOMPANYCHECK = new Label();
        private Label _lblCODNET30 = new Label();
        private Label _lblPAYPAL = new Label();
        private Label _lblREQUESTQUOTE = new Label();
        private Label _lblCHECKBYMAIL = new Label();
        private Label _lblCOD = new Label();
        private Label _lblECHECK = new Label();
        private Label _lblCARDINALMYECHECK = new Label();
        private Label _lblMICROPAY = new Label();
        private Label _lblPAYPALEXPRESS = new Label();
        private Label _lblAMAZONSIMPLEPAY = new Label();
        private Label _lblMONEYBOOKERSQUICKCHECKOUT = new Label();
        private Label _lblSECURENETVAULT = new Label();
        private Label _lblPAYPALEMBEDDEDCHECKOUT = new Label();

        private Label _msgPAYPAL = new Label();
        private Label _msgMICROPAY = new Label();
        private Label _msgPAYPALEXPRESS = new Label();
        private Label _msgAMAZONSIMPLEPAY = new Label();
        private Label _msgMONEYBOOKERSQUICKCHECKOUT = new Label();

        private const string SHOW_CREDITCARD = "ShowCREDITCARD";
        private const string SHOW_PURCHASEORDER = "ShowPURCHASEORDER";
        private const string SHOW_CODMONEYORDER = "ShowCODMONEYORDER";
        private const string SHOW_CODCOMPANYCHECK = "ShowCODCOMPANYCHECK";
        private const string SHOW_CODNET30 = "ShowCODNET30";
        private const string SHOW_PAYPAL = "ShowPAYPAL";
        private const string SHOW_REQUESTQUOTE = "ShowREQUESTQUOTE";
        private const string SHOW_CHECKBYMAIL = "ShowCHECKBYMAIL";
        private const string SHOW_COD = "ShowCOD";
        private const string SHOW_ECHECK = "ShowECHECK";
        private const string SHOW_CARDINALMYECHECK = "ShowCARDINALMYECHECK";
        private const string SHOW_MICROPAY = "ShowMICROPAY";
        private const string SHOW_MICROPAYMESSAGE = "ShowMICROPAYMessage";
        private const string SHOW_PAYPALEXPRESS = "ShowPAYPALEXPRESS";
        private const string SHOW_AMAZONSIMPLEPAY = "ShowAMAZONSIMPLEPAY";
        private const string SHOW_MONEYBOOKERSQUICKCHECKOUT = "ShowMONEYBOOKERSQUICKCHECKOUT";
        private const string SHOW_SECURENETVAULT = "ShowSECURENETVAULT";
        private const string SHOW_PAYPALEMBEDDEDCHECKOUT = "ShowPAYPALEMBEDDEDCHECKOUT";

        public event EventHandler PaymentMethodChanged;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentMethod"/> class.
        /// </summary>
        public PaymentMethod()
        {
            AssignClientReferenceIDs();
            AssignDefaults();
        }

        #endregion

        #region Properties

        #region PayPal

        /// <summary>
        /// Gets or sets the PAYPAL label.
        /// </summary>
        /// <value>The PAYPAL label.</value>
        [Browsable(true), Category("PAYPAL_SETTINGS")]
        public string PAYPALLabel
        {
            get { return _msgPAYPAL.Text; }
            set { _msgPAYPAL.Text = value; }
        }

        /// <summary>
        /// Gets or sets the PAYPALEXPRESS label.
        /// </summary>
        /// <value>The PAYPALEXPRESS label.</value>
        [Browsable(true), Category("PAYPAL_SETTINGS")]
        public string PAYPALEXPRESSLabel
        {
            get { return _msgPAYPALEXPRESS.Text; }
            set { _msgPAYPALEXPRESS.Text = value; }
        }

        /// <summary>
        /// Gets or sets the MONEYBOOKERSQUICKCHECKOUT label.
        /// </summary>
        /// <value>The MONEYBOOKERSQUICKCHECKOUT label.</value>
        [Browsable(true), Category("PAYPAL_SETTINGS")]
        public string MONEYBOOKERSQUICKCHECKOUTLabel
        {
            get { return _msgMONEYBOOKERSQUICKCHECKOUT.Text; }
            set { _msgMONEYBOOKERSQUICKCHECKOUT.Text = value; }
        }

        #endregion

        #region AMAZON

        [Browsable(true), Category("AMAZON_SETTINGS")]
        public string AMAZONSIMPLEPAYLabel
        {
            get { return _msgAMAZONSIMPLEPAY.Text; }
            set { _msgAMAZONSIMPLEPAY.Text = value; }
        }

        #endregion

        #region MicroPay

        /// <summary>
        /// Gets or sets the MICROPAY label.
        /// </summary>
        /// <value>The MICROPAY label.</value>
        [Browsable(true), Category("MICROPAY_SETTINGS")]
        public string MICROPAYLabel
        {
            get { return _msgMICROPAY.Text; }
            set { _msgMICROPAY.Text = value; }
        }

        #endregion

        #region PaymentMethods

        #region CHECKBOXLABELS

        /// <summary>
        /// Gets or sets the CREDITCARD caption.
        /// </summary>
        /// <value>The CREDITCARD caption.</value>
        [Browsable(true), Category("PM_LABEL")]
        public string CREDITCARDCaption
        {
            get { return _rbCREDITCARD.Text; }
            set { _rbCREDITCARD.Text = value; }
        }

        /// <summary>
        /// Gets or sets the PURCHASEORDER caption.
        /// </summary>
        /// <value>The PURCHASEORDER caption.</value>
        [Browsable(true), Category("PM_LABEL")]
        public string PURCHASEORDERCaption
        {
            get { return _rbPURCHASEORDER.Text; }
            set { _rbPURCHASEORDER.Text = value; }
        }

        /// <summary>
        /// Gets or sets the CODMONEYORDER caption.
        /// </summary>
        /// <value>The CODMONEYORDER caption.</value>
        [Browsable(true), Category("PM_LABEL")]
        public string CODMONEYORDERCaption
        {
            get { return _rbCODMONEYORDER.Text; }
            set { _rbCODMONEYORDER.Text = value; }
        }

        /// <summary>
        /// Gets or sets the CODCOMPANYCHECK caption.
        /// </summary>
        /// <value>The CODCOMPANYCHECK caption.</value>
        [Browsable(true), Category("PM_LABEL")]
        public string CODCOMPANYCHECKCaption
        {
            get { return _rbCODCOMPANYCHECK.Text; }
            set { _rbCODCOMPANYCHECK.Text = value; }
        }

        /// <summary>
        /// Gets or sets the CODNE T30 caption.
        /// </summary>
        /// <value>The CODNE T30 caption.</value>
        [Browsable(true), Category("PM_LABEL")]
        public string CODNET30Caption
        {
            get { return _rbCODNET30.Text; }
            set { _rbCODNET30.Text = value; }
        }

        /// <summary>
        /// Gets or sets the PAYPAL caption.
        /// </summary>
        /// <value>The PAYPAL caption.</value>
        [Browsable(true), Category("PM_LABEL")]
        public string PAYPALCaption
        {
            get { return _rbPAYPAL.Text; }
            set { _rbPAYPAL.Text = value; }
        }

        // <summary>
        /// Gets or sets the Amazon Simple Pay caption.
        /// </summary>
        /// <value>The Amazon Simple Pay caption.</value>
        [Browsable(true), Category("PM_LABEL")]
        public string AMAZONSIMPLEPAYCaption
        {
            get { return _rbAMAZONSIMPLEPAY.Text; }
            set { _rbAMAZONSIMPLEPAY.Text = value; }
        }

        /// <summary>
        /// Gets or sets the REQUESTQUOTE caption.
        /// </summary>
        /// <value>The REQUESTQUOTE caption.</value>
        [Browsable(true), Category("PM_LABEL")]
        public string REQUESTQUOTECaption
        {
            get { return _rbREQUESTQUOTE.Text; }
            set { _rbREQUESTQUOTE.Text = value; }
        }

        /// <summary>
        /// Gets or sets the CHECKBYMAIL caption.
        /// </summary>
        /// <value>The CHECKBYMAIL caption.</value>
        [Browsable(true), Category("PM_LABEL")]
        public string CHECKBYMAILCaption
        {
            get { return _rbCHECKBYMAIL.Text; }
            set { _rbCHECKBYMAIL.Text = value; }
        }

        /// <summary>
        /// Gets or sets the COD caption.
        /// </summary>
        /// <value>The COD caption.</value>
        [Browsable(true), Category("PM_LABEL")]
        public string CODCaption
        {
            get { return _rbCOD.Text; }
            set { _rbCOD.Text = value; }
        }

        /// <summary>
        /// Gets or sets the ECHECK caption.
        /// </summary>
        /// <value>The ECHECK caption.</value>
        [Browsable(true), Category("PM_LABEL")]
        public string ECHECKCaption
        {
            get { return _rbECHECK.Text; }
            set { _rbECHECK.Text = value; }
        }

        /// <summary>
        /// Gets or sets the CARDINALMYECHECK caption.
        /// </summary>
        /// <value>The CARDINALMYECHECK caption.</value>
        [Browsable(true), Category("PM_LABEL")]
        public string CARDINALMYECHECKCaption
        {
            get { return _rbCARDINALMYECHECK.Text; }
            set { _rbCARDINALMYECHECK.Text = value; }
        }

        /// <summary>
        /// Gets or sets the MICROPAY caption.
        /// </summary>
        /// <value>The MICROPAY caption.</value>
        [Browsable(true), Category("PM_LABEL")]
        public string MICROPAYCaption
        {
            get { return _rbMICROPAY.Text; }
            set { _rbMICROPAY.Text = value; }
        }

        /// <summary>
        /// Gets or sets the PAYPALEXPRESS caption.
        /// </summary>
        /// <value>The PAYPALEXPRESS caption.</value>
        [Browsable(true), Category("PM_LABEL")]
        public string PAYPALEXPRESSCaption
        {
            get { return _rbPAYPALEXPRESS.Text; }
            set { _rbPAYPALEXPRESS.Text = value; }
        }

        /// <summary>
        /// Gets or sets the MONEYBOOKERSQUICKCHECKOUT caption.
        /// </summary>
        /// <value>The MONEYBOOKERSQUICKCHECKOUT caption.</value>
        [Browsable(true), Category("PM_LABEL")]
        public string MONEYBOOKERSQUICKCHECKOUTCaption
        {
            get { return _rbMONEYBOOKERSQUICKCHECKOUT.Text; }
            set { _rbMONEYBOOKERSQUICKCHECKOUT.Text = value; }
        }

        /// <summary>
        /// Gets or sets the SECURENETVAULT caption.
        /// </summary>
        /// <value>The SECURENETVAULT caption.</value>
        [Browsable(true), Category("PM_LABEL")]
        public string SECURENETVAULTCaption
        {
            get { return _rbSECURENETVAULT.Text; }
            set { _rbSECURENETVAULT.Text = value; }
        }

        /// <summary>
        /// Gets or sets the PAYPALHOSTEDCHECKOUTCaption caption.
        /// </summary>
        /// <value>The PAYPALHOSTEDCHECKOUTCaption caption.</value>
        [Browsable(true), Category("PM_LABEL")]
        public string PAYPALHOSTEDCHECKOUTCaption
        {
            get { return _rbPAYPALEMBEDDEDCHECKOUT.Text; }
            set { _rbPAYPALEMBEDDEDCHECKOUT.Text = value; }
        }



        #endregion

        #region IMAGES

        /// <summary>
        /// Gets or sets the CREDITCARD image.
        /// </summary>
        /// <value>The CREDITCARD image.</value>
        [Browsable(true), Category("PM_IMAGE_URL"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string CREDITCARDImage_AmericanExpress
        {
            get { return _imgCREDITCARD_AmericanExpress.ImageUrl; }
            set { _imgCREDITCARD_AmericanExpress.ImageUrl = value; }
        }

        [Browsable(true), Category("PM_IMAGE_URL"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string CREDITCARDImage_Discover
        {
            get { return _imgCREDITCARD_Discover.ImageUrl; }
            set { _imgCREDITCARD_Discover.ImageUrl = value; }
        }

        [Browsable(true), Category("PM_IMAGE_URL"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string CREDITCARDImage_MasterCard
        {
            get { return _imgCREDITCARD_MasterCard.ImageUrl; }
            set { _imgCREDITCARD_MasterCard.ImageUrl = value; }
        }

        [Browsable(true), Category("PM_IMAGE_URL"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string CREDITCARDImage_Visa
        {
            get { return _imgCREDITCARD_Visa.ImageUrl; }
            set { _imgCREDITCARD_Visa.ImageUrl = value; }
        }

        [Browsable(true), Category("PM_IMAGE_URL"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string CREDITCARDImage_Laser
        {
            get { return _imgCREDITCARD_Laser.ImageUrl; }
            set { _imgCREDITCARD_Laser.ImageUrl = value; }
        }

        [Browsable(true), Category("PM_IMAGE_URL"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string CREDITCARDImage_Maestro
        {
            get { return _imgCREDITCARD_Maestro.ImageUrl; }
            set { _imgCREDITCARD_Maestro.ImageUrl = value; }
        }

        [Browsable(true), Category("PM_IMAGE_URL"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string CREDITCARDImage_VisaDebit
        {
            get { return _imgCREDITCARD_VisaDebit.ImageUrl; }
            set { _imgCREDITCARD_VisaDebit.ImageUrl = value; }
        }

        [Browsable(true), Category("PM_IMAGE_URL"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string CREDITCARDImage_VisaElectron
        {
            get { return _imgCREDITCARD_VisaElectron.ImageUrl; }
            set { _imgCREDITCARD_VisaElectron.ImageUrl = value; }
        }

        [Browsable(true), Category("PM_IMAGE_URL"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string CREDITCARDImage_Jcb
        {
            get { return _imgCREDITCARD_Jcb.ImageUrl; }
            set { _imgCREDITCARD_Jcb.ImageUrl = value; }
        }

        [Browsable(true), Category("PM_IMAGE_URL"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string CREDITCARDImage_Diners
        {
            get { return _imgCREDITCARD_Diners.ImageUrl; }
            set { _imgCREDITCARD_Diners.ImageUrl = value; }
        }

        /// <summary>
        /// Gets or sets the PURCHASEORDER image.
        /// </summary>
        /// <value>The PURCHASEORDER image.</value>
        [Browsable(true), Category("PM_IMAGE_URL"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string PURCHASEORDERImage
        {
            get { return _imgPURCHASEORDER.ImageUrl; }
            set { _imgPURCHASEORDER.ImageUrl = value; }
        }
        /// <summary>
        /// Gets or sets the CODMONEYORDER image.
        /// </summary>
        /// <value>The CODMONEYORDER image.</value>
        [Browsable(true), Category("PM_IMAGE_URL"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string CODMONEYORDERImage
        {
            get { return _imgCODMONEYORDER.ImageUrl; }
            set { _imgCODMONEYORDER.ImageUrl = value; }
        }
        /// <summary>
        /// Gets or sets the CODCOMPANYCHECK image.
        /// </summary>
        /// <value>The CODCOMPANYCHECK image.</value>
        [Browsable(true), Category("PM_IMAGE_URL"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string CODCOMPANYCHECKImage
        {
            get { return _imgCODCOMPANYCHECK.ImageUrl; }
            set { _imgCODCOMPANYCHECK.ImageUrl = value; }
        }
        /// <summary>
        /// Gets or sets the CODNE T30 image.
        /// </summary>
        /// <value>The CODNE T30 image.</value>
        [Browsable(true), Category("PM_IMAGE_URL"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string CODNET30Image
        {
            get { return _imgCODNET30.ImageUrl; }
            set { _imgCODNET30.ImageUrl = value; }
        }
        /// <summary>
        /// Gets or sets the PAYPAL image.
        /// </summary>
        /// <value>The PAYPAL image.</value>
        [Browsable(true), Category("PM_IMAGE_URL"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string PAYPALImage
        {
            get { return _imgPAYPAL.ImageUrl; }
            set { _imgPAYPAL.ImageUrl = value; }
        }
        [Browsable(true), Category("PM_IMAGE_URL"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string AMAZONSIMPLEPAYImage
        {
            get { return _imgAMAZONSIMPLEPAY.ImageUrl; }
            set { _imgAMAZONSIMPLEPAY.ImageUrl = value; }
        }
        /// <summary>
        /// Gets or sets the REQUESTQUOTE image.
        /// </summary>
        /// <value>The REQUESTQUOTE image.</value>
        [Browsable(true), Category("PM_IMAGE_URL"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string REQUESTQUOTEImage
        {
            get { return _imgREQUESTQUOTE.ImageUrl; }
            set { _imgREQUESTQUOTE.ImageUrl = value; }
        }
        /// <summary>
        /// Gets or sets the CHECKBYMAIL image.
        /// </summary>
        /// <value>The CHECKBYMAIL image.</value>
        [Browsable(true), Category("PM_IMAGE_URL"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string CHECKBYMAILImage
        {
            get { return _imgCHECKBYMAIL.ImageUrl; }
            set { _imgCHECKBYMAIL.ImageUrl = value; }
        }
        /// <summary>
        /// Gets or sets the COD image.
        /// </summary>
        /// <value>The COD image.</value>
        [Browsable(true), Category("PM_IMAGE_URL"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string CODImage
        {
            get { return _imgCOD.ImageUrl; }
            set { _imgCOD.ImageUrl = value; }
        }
        /// <summary>
        /// Gets or sets the ECHECK image.
        /// </summary>
        /// <value>The ECHECK image.</value>
        [Browsable(true), Category("PM_IMAGE_URL"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string ECHECKImage
        {
            get { return _imgECHECK.ImageUrl; }
            set { _imgECHECK.ImageUrl = value; }
        }
        /// <summary>
        /// Gets or sets the CARDINALMYECHECK image.
        /// </summary>
        /// <value>The CARDINALMYECHECK image.</value>
        [Browsable(true), Category("PM_IMAGE_URL"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string CARDINALMYECHECKImage
        {
            get { return _imgCARDINALMYECHECK.ImageUrl; }
            set { _imgCARDINALMYECHECK.ImageUrl = value; }
        }
        /// <summary>
        /// Gets or sets the MICROPAY image.
        /// </summary>
        /// <value>The MICROPAY image.</value>
        [Browsable(true), Category("PM_IMAGE_URL"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string MICROPAYImage
        {
            get { return _imgMICROPAY.ImageUrl; }
            set { _imgMICROPAY.ImageUrl = value; }
        }
        /// <summary>
        /// Gets or sets the PAYPALEXPRESS image.
        /// </summary>
        /// <value>The PAYPALEXPRESS image.</value>
        [Browsable(true), Category("PM_IMAGE_URL"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string PAYPALEXPRESSImage
        {
            get { return _imgPAYPALEXPRESS.ImageUrl; }
            set { _imgPAYPALEXPRESS.ImageUrl = value; }
        }

        /// <summary>
        /// Gets or sets the MONEYBOOKERSQUICKCHECKOUT image.
        /// </summary>
        /// <value>The MONEYBOOKERSQUICKCHECKOUT image.</value>
        [Browsable(true), Category("PM_IMAGE_URL"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string MONEYBOOKERSQUICKCHECKOUTImage
        {
            get { return _imgMONEYBOOKERSQUICKCHECKOUT.ImageUrl; }
            set { _imgMONEYBOOKERSQUICKCHECKOUT.ImageUrl = value; }
        }

        /// <summary>
        /// Gets or sets the SECURENETVAULT image.
        /// </summary>
        /// <value>The SECURENETVAULT image.</value>
        [Browsable(true), Category("PM_IMAGE_URL"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string SECURENETVAULTImage
        {
            get { return _imgSECURENETVAULT.ImageUrl; }
            set { _imgSECURENETVAULT.ImageUrl = value; }
        }

        /// <summary>
        /// Gets or sets the PAYPALEMBEDDEDCHECKOUT image.
        /// </summary>
        /// <value>The PAYPALEMBEDDEDCHECKOUT image.</value>
        [Browsable(true), Category("PM_IMAGE_URL"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string PAYPALEMBEDDEDCHECKOUTImage
        {
            get { return _imgPAYPALEMBEDDEDCHECKOUT.ImageUrl; }
            set { _imgPAYPALEMBEDDEDCHECKOUT.ImageUrl = value; }
        }



        #endregion

        #region VIEWSTATES

        /// <summary>
        /// Gets or sets a value indicating whether [show CREDITCARD].
        /// </summary>
        /// <value><c>true</c> if [show CREDITCARD]; otherwise, <c>false</c>.</value>
        [Browsable(true), Category("PM_SETTINGS")]
        public bool ShowCREDITCARD
        {
            get
            {
                object booleanValue = ViewState[SHOW_CREDITCARD];
                if (null == booleanValue) { return false; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState[SHOW_CREDITCARD] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show PURCHASEORDER].
        /// </summary>
        /// <value><c>true</c> if [show PURCHASEORDER]; otherwise, <c>false</c>.</value>
        [Browsable(true), Category("PM_SETTINGS")]
        public bool ShowPURCHASEORDER
        {
            get
            {
                object booleanValue = ViewState[SHOW_PURCHASEORDER];
                if (null == booleanValue) { return false; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState[SHOW_PURCHASEORDER] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show CODMONEYORDER].
        /// </summary>
        /// <value><c>true</c> if [show CODMONEYORDER]; otherwise, <c>false</c>.</value>
        [Browsable(true), Category("PM_SETTINGS")]
        public bool ShowCODMONEYORDER
        {
            get
            {
                object booleanValue = ViewState[SHOW_CODMONEYORDER];
                if (null == booleanValue) { return false; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState[SHOW_CODMONEYORDER] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show CODCOMPANYCHECK].
        /// </summary>
        /// <value><c>true</c> if [show CODCOMPANYCHECK]; otherwise, <c>false</c>.</value>
        [Browsable(true), Category("PM_SETTINGS")]
        public bool ShowCODCOMPANYCHECK
        {
            get
            {
                object booleanValue = ViewState[SHOW_CODCOMPANYCHECK];
                if (null == booleanValue) { return false; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState[SHOW_CODCOMPANYCHECK] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show CODNE T30].
        /// </summary>
        /// <value><c>true</c> if [show CODNET30]; otherwise, <c>false</c>.</value>
        [Browsable(true), Category("PM_SETTINGS")]
        public bool ShowCODNET30
        {
            get
            {
                object booleanValue = ViewState[SHOW_CODNET30];
                if (null == booleanValue) { return false; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState[SHOW_CODNET30] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show PAYPAL].
        /// </summary>
        /// <value><c>true</c> if [show PAYPAL]; otherwise, <c>false</c>.</value>
        [Browsable(true), Category("PM_SETTINGS")]
        public bool ShowPAYPAL
        {
            get
            {
                object booleanValue = ViewState[SHOW_PAYPAL];
                if (null == booleanValue) { return false; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState[SHOW_PAYPAL] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show AmazonSimplePay].
        /// </summary>
        /// <value><c>true</c> if [show AmazonSimplePay]; otherwise, <c>false</c>.</value>
        [Browsable(true), Category("PM_SETTINGS")]
        public bool ShowAmazonSimplePay
        {
            get
            {
                object booleanValue = ViewState[SHOW_AMAZONSIMPLEPAY];
                if (null == booleanValue) { return false; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState[SHOW_AMAZONSIMPLEPAY] = value;
                ChildControlsCreated = false;
            }
        }


        /// <summary>
        /// Gets or sets a value indicating whether [show REQUESTQUOTE].
        /// </summary>
        /// <value><c>true</c> if [show REQUESTQUOTE]; otherwise, <c>false</c>.</value>
        [Browsable(true), Category("PM_SETTINGS")]
        public bool ShowREQUESTQUOTE
        {
            get
            {
                object booleanValue = ViewState[SHOW_REQUESTQUOTE];
                if (null == booleanValue) { return false; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState[SHOW_REQUESTQUOTE] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show CHECKBYMAIL].
        /// </summary>
        /// <value><c>true</c> if [show CHECKBYMAIL]; otherwise, <c>false</c>.</value>
        [Browsable(true), Category("PM_SETTINGS")]
        public bool ShowCHECKBYMAIL
        {
            get
            {
                object booleanValue = ViewState[SHOW_CHECKBYMAIL];
                if (null == booleanValue) { return false; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState[SHOW_CHECKBYMAIL] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show COD].
        /// </summary>
        /// <value><c>true</c> if [show COD]; otherwise, <c>false</c>.</value>
        [Browsable(true), Category("PM_SETTINGS")]
        public bool ShowCOD
        {
            get
            {
                object booleanValue = ViewState[SHOW_COD];
                if (null == booleanValue) { return false; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState[SHOW_COD] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show ECHECK].
        /// </summary>
        /// <value><c>true</c> if [show ECHECK]; otherwise, <c>false</c>.</value>
        [Browsable(true), Category("PM_SETTINGS")]
        public bool ShowECHECK
        {
            get
            {
                object booleanValue = ViewState[SHOW_ECHECK];
                if (null == booleanValue) { return false; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState[SHOW_ECHECK] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show CARDINALMYECHECK].
        /// </summary>
        /// <value><c>true</c> if [show CARDINALMYECHECK]; otherwise, <c>false</c>.</value>
        [Browsable(true), Category("PM_SETTINGS")]
        public bool ShowCARDINALMYECHECK
        {
            get
            {
                object booleanValue = ViewState[SHOW_CARDINALMYECHECK];
                if (null == booleanValue) { return false; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState[SHOW_CARDINALMYECHECK] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show MICROPAY].
        /// </summary>
        /// <value><c>true</c> if [show MICROPAY]; otherwise, <c>false</c>.</value>
        [Browsable(true), Category("PM_SETTINGS")]
        public bool ShowMICROPAY
        {
            get
            {
                object booleanValue = ViewState[SHOW_MICROPAY];
                if (null == booleanValue) { return false; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState[SHOW_MICROPAY] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show MICROPAY message].
        /// </summary>
        /// <value><c>true</c> if [show MICROPAY message]; otherwise, <c>false</c>.</value>
        [Browsable(false)]
        public bool ShowMICROPAYMessage
        {
            get
            {
                object booleanValue = ViewState[SHOW_MICROPAYMESSAGE];
                if (null == booleanValue) { return false; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState[SHOW_MICROPAYMESSAGE] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show PAYPALEXPRESS].
        /// </summary>
        /// <value><c>true</c> if [show PAYPALEXPRESS]; otherwise, <c>false</c>.</value>
        [Browsable(true), Category("PM_SETTINGS")]
        public bool ShowPAYPALEXPRESS
        {
            get
            {
                object booleanValue = ViewState[SHOW_PAYPALEXPRESS];
                if (null == booleanValue) { return false; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState[SHOW_PAYPALEXPRESS] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show MONEYBOOKERSQUICKCHECKOUT].
        /// </summary>
        /// <value><c>true</c> if [show MONEYBOOKERSQUICKCHECKOUT]; otherwise, <c>false</c>.</value>
        [Browsable(true), Category("PM_SETTINGS")]
        public bool ShowMONEYBOOKERSQUICKCHECKOUT
        {
            get
            {
                object booleanValue = ViewState[SHOW_MONEYBOOKERSQUICKCHECKOUT];
                if (null == booleanValue) { return false; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState[SHOW_MONEYBOOKERSQUICKCHECKOUT] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show SECURENETVAULT].
        /// </summary>
        /// <value><c>true</c> if [show SECURENETVAULT]; otherwise, <c>false</c>.</value>
        [Browsable(true), Category("PM_SETTINGS")]
        public bool ShowSECURENETVAULT
        {
            get
            {
                object booleanValue = ViewState[SHOW_SECURENETVAULT];
                if (null == booleanValue) { return false; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState[SHOW_SECURENETVAULT] = value;
                ChildControlsCreated = false;
            }
        }
        [Browsable(true), Category("PM_SETTINGS")]
        public bool ShowPAYPALEMBEDDEDCHECKOUT
        {
            get
            {
                object booleanValue = ViewState[SHOW_PAYPALEMBEDDEDCHECKOUT];
                if (null == booleanValue) { return false; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState[SHOW_PAYPALEMBEDDEDCHECKOUT] = value;
                ChildControlsCreated = false;
            }
        }

        #endregion

        #region CHECKBOXES

        /// <summary>
        /// Gets or sets a value indicating whether [CREDITCARD checked].
        /// </summary>
        /// <value><c>true</c> if [CREDITCARD checked]; otherwise, <c>false</c>.</value>
        [Browsable(false)]
        public bool CREDITCARDChecked
        {
            get { return _rbCREDITCARD.Checked; }
            set { _rbCREDITCARD.Checked = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [PURCHASEORDER checked].
        /// </summary>
        /// <value><c>true</c> if [PURCHASEORDER checked]; otherwise, <c>false</c>.</value>
        [Browsable(false)]
        public bool PURCHASEORDERChecked
        {
            get { return _rbPURCHASEORDER.Checked; }
            set { _rbPURCHASEORDER.Checked = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [CODMONEYORDER checked].
        /// </summary>
        /// <value><c>true</c> if [CODMONEYORDER checked]; otherwise, <c>false</c>.</value>
        [Browsable(false)]
        public bool CODMONEYORDERChecked
        {
            get { return _rbCODMONEYORDER.Checked; }
            set { _rbCODMONEYORDER.Checked = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [CODCOMPANYCHECK checked].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [CODCOMPANYCHECK checked]; otherwise, <c>false</c>.
        /// </value>
        [Browsable(false)]
        public bool CODCOMPANYCHECKChecked
        {
            get { return _rbCODCOMPANYCHECK.Checked; }
            set { _rbCODCOMPANYCHECK.Checked = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [CODNE T30 checked].
        /// </summary>
        /// <value><c>true</c> if [CODNET30 checked]; otherwise, <c>false</c>.</value>
        [Browsable(false)]
        public bool CODNET30Checked
        {
            get { return _rbCODNET30.Checked; }
            set { _rbCODNET30.Checked = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [PAYPAL checked].
        /// </summary>
        /// <value><c>true</c> if [PAYPAL checked]; otherwise, <c>false</c>.</value>
        [Browsable(false)]
        public bool PAYPALChecked
        {
            get { return _rbPAYPAL.Checked; }
            set { _rbPAYPAL.Checked = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [AmazonSimplePay checked].
        /// </summary>
        /// <value><c>true</c> if [AmazonSimplePay checked]; otherwise, <c>false</c>.</value>
        [Browsable(false)]
        public bool AMAZONSIMPLEPAYChecked
        {
            get { return _rbAMAZONSIMPLEPAY.Checked; }
            set { _rbAMAZONSIMPLEPAY.Checked = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [REQUESTQUOTE checked].
        /// </summary>
        /// <value><c>true</c> if [REQUESTQUOTE checked]; otherwise, <c>false</c>.</value>
        [Browsable(false)]
        public bool REQUESTQUOTEChecked
        {
            get { return _rbREQUESTQUOTE.Checked; }
            set { _rbREQUESTQUOTE.Checked = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [CHECKBYMAIL checked].
        /// </summary>
        /// <value><c>true</c> if [CHECKBYMAIL checked]; otherwise, <c>false</c>.</value>
        [Browsable(false)]
        public bool CHECKBYMAILChecked
        {
            get { return _rbCHECKBYMAIL.Checked; }
            set { _rbCHECKBYMAIL.Checked = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [COD checked].
        /// </summary>
        /// <value><c>true</c> if [COD checked]; otherwise, <c>false</c>.</value>
        [Browsable(false)]
        public bool CODChecked
        {
            get { return _rbCOD.Checked; }
            set { _rbCOD.Checked = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [ECHECK checked].
        /// </summary>
        /// <value><c>true</c> if [ECHECK checked]; otherwise, <c>false</c>.</value>
        [Browsable(false)]
        public bool ECHECKChecked
        {
            get { return _rbECHECK.Checked; }
            set { _rbECHECK.Checked = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [CARDINALMYECHECK checked].
        /// </summary>
        /// <value><c>true</c> if [CARDINALMYECHECK checked]; otherwise, <c>false</c>.</value>
        [Browsable(false)]
        public bool CARDINALMYECHECKChecked
        {
            get { return _rbCARDINALMYECHECK.Checked; }
            set { _rbCARDINALMYECHECK.Checked = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [MICROPAY checked].
        /// </summary>
        /// <value><c>true</c> if [MICROPAY checked]; otherwise, <c>false</c>.</value>
        [Browsable(false)]
        public bool MICROPAYChecked
        {
            get { return _rbMICROPAY.Checked; }
            set { _rbMICROPAY.Checked = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [PAYPALEXPRESS checked].
        /// </summary>
        /// <value><c>true</c> if [PAYPALEXPRESS checked]; otherwise, <c>false</c>.</value>
        [Browsable(false)]
        public bool PAYPALEXPRESSChecked
        {
            get { return _rbPAYPALEXPRESS.Checked; }
            set { _rbPAYPALEXPRESS.Checked = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [MONEYBOOKERSQUICKCHECKOUT checked].
        /// </summary>
        /// <value><c>true</c> if [MONEYBOOKERSQUICKCHECKOUT checked]; otherwise, <c>false</c>.</value>
        [Browsable(false)]
        public bool MONEYBOOKERSQUICKCHECKOUTChecked
        {
            get { return _rbMONEYBOOKERSQUICKCHECKOUT.Checked; }
            set { _rbMONEYBOOKERSQUICKCHECKOUT.Checked = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [SECURENETVAULT checked].
        /// </summary>
        /// <value><c>true</c> if [SECURENETVAULT checked]; otherwise, <c>false</c>.</value>
        [Browsable(false)]
        public bool SECURENETVAULTChecked
        {
            get { return _rbSECURENETVAULT.Checked; }
            set { _rbSECURENETVAULT.Checked = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [PAYPALEMBEDDEDCHECKOUTChecked checked].
        /// </summary>
        /// <value><c>true</c> if [PAYPALEMBEDDEDCHECKOUTChecked checked]; otherwise, <c>false</c>.</value>
        [Browsable(false)]
        public bool PAYPALEMBEDDEDCHECKOUTChecked
        {
            get { return _rbPAYPALEMBEDDEDCHECKOUT.Checked; }
            set { _rbPAYPALEMBEDDEDCHECKOUT.Checked = value; }
        }



        [Browsable(false)]
        public bool HasPaymentMethodSelected
        {
            get
            {
                return AMAZONSIMPLEPAYChecked
                    || CARDINALMYECHECKChecked
                    || CHECKBYMAILChecked
                    || CODChecked
                    || CODCOMPANYCHECKChecked
                    || CODMONEYORDERChecked
                    || CODNET30Checked
                    || CREDITCARDChecked
                    || ECHECKChecked
                    || MICROPAYChecked
                    || MONEYBOOKERSQUICKCHECKOUTChecked
                    || PAYPALChecked
                    || PAYPALEXPRESSChecked
                    || PURCHASEORDERChecked
                    || REQUESTQUOTEChecked
                    || SECURENETVAULTChecked;
            }
        }


        #endregion

        #region NOTES

        /// <summary>
        /// Gets or sets the CREDITCARD note.
        /// </summary>
        /// <value>The CREDITCARD note.</value>
        [Browsable(true), Category("PM_NOTE")]
        public string CREDITCARDNote
        {
            get { return _lblCREDITCARD.Text; }
            set { _lblCREDITCARD.Text = value; }
        }

        /// <summary>
        /// Gets or sets the PURCHASEORDER note.
        /// </summary>
        /// <value>The PURCHASEORDER note.</value>
        [Browsable(true), Category("PM_NOTE")]
        public string PURCHASEORDERNote
        {
            get { return _lblPURCHASEORDER.Text; }
            set { _lblPURCHASEORDER.Text = value; }
        }

        /// <summary>
        /// Gets or sets the CODMONEYORDER note.
        /// </summary>
        /// <value>The CODMONEYORDER note.</value>
        [Browsable(true), Category("PM_NOTE")]
        public string CODMONEYORDERNote
        {
            get { return _lblCODMONEYORDER.Text; }
            set { _lblCODMONEYORDER.Text = value; }
        }

        /// <summary>
        /// Gets or sets the CODCOMPANYCHECK note.
        /// </summary>
        /// <value>The CODCOMPANYCHECK note.</value>
        [Browsable(true), Category("PM_NOTE")]
        public string CODCOMPANYCHECKNote
        {
            get { return _lblCODCOMPANYCHECK.Text; }
            set { _lblCODCOMPANYCHECK.Text = value; }
        }

        /// <summary>
        /// Gets or sets the CODNE T30 note.
        /// </summary>
        /// <value>The CODNE T30 note.</value>
        [Browsable(true), Category("PM_NOTE")]
        public string CODNET30Note
        {
            get { return _lblCODNET30.Text; }
            set { _lblCODNET30.Text = value; }
        }

        /// <summary>
        /// Gets or sets the PAYPAL note.
        /// </summary>
        /// <value>The PAYPAL note.</value>
        [Browsable(true), Category("PM_NOTE")]
        public string PAYPALNote
        {
            get { return _lblPAYPAL.Text; }
            set { _lblPAYPAL.Text = value; }
        }

        /// <summary>
        /// Gets or sets the AmazonSimplePay note.
        /// </summary>
        /// <value>The AmazonSimplePay note.</value>
        [Browsable(true), Category("PM_NOTE")]
        public string AMAZONSIMPLEPAYNote
        {
            get { return _lblAMAZONSIMPLEPAY.Text; }
            set { _lblAMAZONSIMPLEPAY.Text = value; }
        }

        /// <summary>
        /// Gets or sets the REQUESTQUOTE note.
        /// </summary>
        /// <value>The REQUESTQUOTE note.</value>
        [Browsable(true), Category("PM_NOTE")]
        public string REQUESTQUOTENote
        {
            get { return _lblREQUESTQUOTE.Text; }
            set { _lblREQUESTQUOTE.Text = value; }
        }

        /// <summary>
        /// Gets or sets the CHECKBYMAIL note.
        /// </summary>
        /// <value>The CHECKBYMAIL note.</value>
        [Browsable(true), Category("PM_NOTE")]
        public string CHECKBYMAILNote
        {
            get { return _lblCHECKBYMAIL.Text; }
            set { _lblCHECKBYMAIL.Text = value; }
        }

        /// <summary>
        /// Gets or sets the COD note.
        /// </summary>
        /// <value>The COD note.</value>
        [Browsable(true), Category("PM_NOTE")]
        public string CODNote
        {
            get { return _lblCOD.Text; }
            set { _lblCOD.Text = value; }
        }

        /// <summary>
        /// Gets or sets the ECHECK note.
        /// </summary>
        /// <value>The ECHECK note.</value>
        [Browsable(true), Category("PM_NOTE")]
        public string ECHECKNote
        {
            get { return _lblECHECK.Text; }
            set { _lblECHECK.Text = value; }
        }

        /// <summary>
        /// Gets or sets the CARDINALMYECHECK note.
        /// </summary>
        /// <value>The CARDINALMYECHECK note.</value>
        [Browsable(true), Category("PM_NOTE")]
        public string CARDINALMYECHECKNote
        {
            get { return _lblCARDINALMYECHECK.Text; }
            set { _lblCARDINALMYECHECK.Text = value; }
        }

        /// <summary>
        /// Gets or sets the MICROPAY note.
        /// </summary>
        /// <value>The MICROPAY note.</value>
        [Browsable(true), Category("PM_NOTE")]
        public string MICROPAYNote
        {
            get { return _lblMICROPAY.Text; }
            set { _lblMICROPAY.Text = value; }
        }

        /// <summary>
        /// Gets or sets the PAYPALEXPRESS note.
        /// </summary>
        /// <value>The PAYPALEXPRESS note.</value>
        [Browsable(true), Category("PM_NOTE")]
        public string PAYPALEXPRESSNote
        {
            get { return _lblPAYPALEXPRESS.Text; }
            set { _lblPAYPALEXPRESS.Text = value; }
        }

        /// <summary>
        /// Gets or sets the MONEYBOOKERSQUICKCHECKOUT note.
        /// </summary>
        /// <value>The MONEYBOOKERSQUICKCHECKOUT note.</value>
        [Browsable(true), Category("PM_NOTE")]
        public string MONEYBOOKERSQUICKCHECKOUTNote
        {
            get { return _lblMONEYBOOKERSQUICKCHECKOUT.Text; }
            set { _lblMONEYBOOKERSQUICKCHECKOUT.Text = value; }
        }

        /// <summary>
        /// Gets or sets the SECURENETVAULT note.
        /// </summary>
        /// <value>The SECURENETVAULT note.</value>
        [Browsable(true), Category("PM_NOTE")]
        public string SECURENETVAULTNote
        {
            get { return _lblSECURENETVAULT.Text; }
            set { _lblSECURENETVAULT.Text = value; }
        }

        /// <summary>
        /// Gets or sets the PAYPALEMBEDDEDCHECKOUTNote note.
        /// </summary>
        /// <value>The PAYPALEMBEDDEDCHECKOUTNote note.</value>
        [Browsable(true), Category("PM_NOTE")]
        public string PAYPALEMBEDDEDCHECKOUTNote
        {
            get { return _lblPAYPALEMBEDDEDCHECKOUT.Text; }
            set { _lblPAYPALEMBEDDEDCHECKOUT.Text = value; }
        }


        #endregion

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Assigns the client reference IDs.
        /// </summary>
        private void AssignClientReferenceIDs()
        {
            _rbCREDITCARD.ID = "rbCREDITCARD";
            _rbPURCHASEORDER.ID = "rbPURCHASEORDER";
            _rbCODMONEYORDER.ID = "rbCODMONEYORDER";
            _rbCODCOMPANYCHECK.ID = "rbCODCOMPANYCHECK";
            _rbCODNET30.ID = "rbCODNET30";
            _rbPAYPAL.ID = "rbPAYPAL";
            _rbREQUESTQUOTE.ID = "rbREQUESTQUOTE";
            _rbCHECKBYMAIL.ID = "rbCHECKBYMAIL";
            _rbCOD.ID = "rbCOD";
            _rbECHECK.ID = "rbECHECK";
            _rbCARDINALMYECHECK.ID = "rbCARDINALMYECHECK";
            _rbMICROPAY.ID = "rbMICROPAY";
            _rbPAYPALEXPRESS.ID = "rbPAYPALEXPRESS";
            _rbAMAZONSIMPLEPAY.ID = "rbAMAZONSIMPLEPAY";
            _rbMONEYBOOKERSQUICKCHECKOUT.ID = "rbMONEYBOOKERSQUICKCHECKOUT";
            _rbSECURENETVAULT.ID = "rbSECURENETVAULT";
            _rbPAYPALEMBEDDEDCHECKOUT.ID = "rbPAYPALEMBEDDEDCHECKOUT";

            _imgCREDITCARD_AmericanExpress.ID = "imgCREDITCARDAmericanExpress";
            _imgCREDITCARD_Discover.ID = "imgCREDITCARDDiscover";
            _imgCREDITCARD_MasterCard.ID = "imgCREDITCARDMasterCard";
            _imgCREDITCARD_Visa.ID = "imgCREDITCARDVisa";
            _imgCREDITCARD_Laser.ID = "imgCREDITCARDLaser";
            _imgCREDITCARD_Maestro.ID = "imgCREDITCARDMaestro";
            _imgCREDITCARD_VisaDebit.ID = "imgCREDITCARDVisaDebit";
            _imgCREDITCARD_VisaElectron.ID = "imgCREDITCARDVisaElectron";
            _imgCREDITCARD_Jcb.ID = "imgCREDITCARDJcb";
            _imgCREDITCARD_Diners.ID = "imgCREDITCARDDiners";
            _imgPURCHASEORDER.ID = "imgPURCHASEORDER";
            _imgCODMONEYORDER.ID = "imgCODMONEYORDER";
            _imgCODCOMPANYCHECK.ID = "imgCODCOMPANYCHECK";
            _imgCODNET30.ID = "imgCODNET30";
            _imgPAYPAL.ID = "imgPAYPAL";
            _imgREQUESTQUOTE.ID = "imgREQUESTQUOTE";
            _imgCHECKBYMAIL.ID = "imgCHECKBYMAIL";
            _imgCOD.ID = "imgCOD";
            _imgECHECK.ID = "imgECHECK";
            _imgCARDINALMYECHECK.ID = "imgCARDINALMYECHECK";
            _imgMICROPAY.ID = "imgMICROPAY";
            _imgPAYPALEXPRESS.ID = "imgPAYPALEXPRESS";
            _imgAMAZONSIMPLEPAY.ID = "imgAMAZONSIMPLEPAY";
            _imgMONEYBOOKERSQUICKCHECKOUT.ID = "imgMONEYBOOKERSQUICKCHECKOUT";

            _lblCREDITCARD.ID = "_lblCREDITCARD";
            _lblPURCHASEORDER.ID = "lblPURCHASEORDER";
            _lblCODMONEYORDER.ID = "lblCODMONEYORDER";
            _lblCODCOMPANYCHECK.ID = "lblCODCOMPANYCHECK";
            _lblCODNET30.ID = "lblCODNET30";
            _lblPAYPAL.ID = "lblPAYPAL";
            _lblREQUESTQUOTE.ID = "lblREQUESTQUOTE";
            _lblCHECKBYMAIL.ID = "lblCHECKBYMAIL";
            _lblCOD.ID = "lblCOD";
            _lblECHECK.ID = "lblECHECK";
            _lblCARDINALMYECHECK.ID = "lblCARDINALMYECHECK";
            _lblMICROPAY.ID = "lblMICROPAY";
            _lblPAYPALEXPRESS.ID = "lblPAYPALEXPRESS";
            _lblAMAZONSIMPLEPAY.ID = "lblAMAZONSIMPLEPAY";
            _lblMONEYBOOKERSQUICKCHECKOUT.ID = "MONEYBOOKERSQUICKCHECKOUT";
            _lblSECURENETVAULT.ID = "lblSECURENETVAULT";

            _msgPAYPAL.ID = "msgPAYPAL";
            _msgMICROPAY.ID = "msgMICROPAY";
            _msgPAYPALEXPRESS.ID = "msgPAYPALEXPRESS";
            _msgAMAZONSIMPLEPAY.ID = "msgAMAZONSIMPLEPAY";
            _msgMONEYBOOKERSQUICKCHECKOUT.ID = "msgMONEYBOOKERSQUICKCHECKOUT";
        }

        /// <summary>
        /// Assigns the defaults.
        /// </summary>
        private void AssignDefaults()
        {
            _rbCREDITCARD.GroupName = "PaymentSelection";
            _rbPURCHASEORDER.GroupName = "PaymentSelection";
            _rbCODMONEYORDER.GroupName = "PaymentSelection";
            _rbCODCOMPANYCHECK.GroupName = "PaymentSelection";
            _rbCODNET30.GroupName = "PaymentSelection";
            _rbPAYPAL.GroupName = "PaymentSelection";
            _rbREQUESTQUOTE.GroupName = "PaymentSelection";
            _rbCHECKBYMAIL.GroupName = "PaymentSelection";
            _rbCOD.GroupName = "PaymentSelection";
            _rbECHECK.GroupName = "PaymentSelection";
            _rbCARDINALMYECHECK.GroupName = "PaymentSelection";
            _rbMICROPAY.GroupName = "PaymentSelection";
            _rbPAYPALEXPRESS.GroupName = "PaymentSelection";
            _rbAMAZONSIMPLEPAY.GroupName = "PaymentSelection";
            _rbMONEYBOOKERSQUICKCHECKOUT.GroupName = "PaymentSelection";
            _rbSECURENETVAULT.GroupName = "PaymentSelection";
            _rbPAYPALEMBEDDEDCHECKOUT.GroupName = "PaymentSelection";

            _rbCREDITCARD.AutoPostBack = true;
            _rbPURCHASEORDER.AutoPostBack = true;
            _rbCODMONEYORDER.AutoPostBack = true;
            _rbCODCOMPANYCHECK.AutoPostBack = true;
            _rbCODNET30.AutoPostBack = true;
            _rbPAYPAL.AutoPostBack = true;
            _rbREQUESTQUOTE.AutoPostBack = true;
            _rbCHECKBYMAIL.AutoPostBack = true;
            _rbCOD.AutoPostBack = true;
            _rbECHECK.AutoPostBack = true;
            _rbCARDINALMYECHECK.AutoPostBack = true;
            _rbMICROPAY.AutoPostBack = true;
            _rbPAYPALEXPRESS.AutoPostBack = true;
            _rbAMAZONSIMPLEPAY.AutoPostBack = true;
            _rbMONEYBOOKERSQUICKCHECKOUT.AutoPostBack = true;
            _rbSECURENETVAULT.AutoPostBack = true;
            _rbPAYPALEMBEDDEDCHECKOUT.AutoPostBack = true;

            
            //_rbPURCHASEORDER.Attributes.Add("onclick", "javascript:setTimeout('__doPostBack(\'" + _rbPURCHASEORDER.ClientID.Replace("_", "$") + "\',\'\')', 0)");         

          
            //_rbCREDITCARD.Attributes.Add("onclick", "javascript:setTimeout('__doPostBack(\'" + _rbCREDITCARD.ClientID.Replace("_", "$") + "\',\'\')', 0)");
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            _rbCREDITCARD.CheckedChanged += PaymentMethod_CheckChanged;
            _rbPURCHASEORDER.CheckedChanged += PaymentMethod_CheckChanged;
            _rbCODMONEYORDER.CheckedChanged += PaymentMethod_CheckChanged;
            _rbCODCOMPANYCHECK.CheckedChanged += PaymentMethod_CheckChanged;
            _rbCODNET30.CheckedChanged += PaymentMethod_CheckChanged;
            _rbPAYPAL.CheckedChanged += PaymentMethod_CheckChanged;
            _rbREQUESTQUOTE.CheckedChanged += PaymentMethod_CheckChanged;
            _rbCHECKBYMAIL.CheckedChanged += PaymentMethod_CheckChanged;
            _rbCOD.CheckedChanged += PaymentMethod_CheckChanged;
            _rbECHECK.CheckedChanged += PaymentMethod_CheckChanged;
            _rbCARDINALMYECHECK.CheckedChanged += PaymentMethod_CheckChanged;
            _rbMICROPAY.CheckedChanged += PaymentMethod_CheckChanged;
            _rbPAYPALEXPRESS.CheckedChanged += PaymentMethod_CheckChanged;
            _rbAMAZONSIMPLEPAY.CheckedChanged += PaymentMethod_CheckChanged;
            _rbMONEYBOOKERSQUICKCHECKOUT.CheckedChanged += PaymentMethod_CheckChanged;
            _rbSECURENETVAULT.CheckedChanged += PaymentMethod_CheckChanged;
            _rbPAYPALEMBEDDEDCHECKOUT.CheckedChanged += PaymentMethod_CheckChanged;

            base.OnInit(e);
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
            Int32 paymentMethodsEnabled = 0;
            Action disablePaymentMethod = null;

            if (this.DesignMode)
            {
                this.ShowCREDITCARD = true;
                this.ShowECHECK = true;
                this.ShowMICROPAY = true;
                this.ShowPAYPAL = true;
                this.ShowPAYPALEXPRESS = true;
                this.ShowPURCHASEORDER = true;
                this.ShowREQUESTQUOTE = true;
                this.ShowCARDINALMYECHECK = true;
                this.ShowCHECKBYMAIL = true;
                this.ShowCOD = true;
                this.ShowCODCOMPANYCHECK = true;
                this.ShowCODMONEYORDER = true;
                this.ShowCODNET30 = true;
                this.ShowAmazonSimplePay = true;
                this.ShowMONEYBOOKERSQUICKCHECKOUT = true;
                this.ShowSECURENETVAULT = true;
                this.ShowPAYPALEMBEDDEDCHECKOUT = true;
            }

            this.Controls.Clear();

            this.Controls.Add(new LiteralControl("<div class='form payment-form'>"));
            if (this.ShowCREDITCARD)
            {
                paymentMethodsEnabled++;
                this.Controls.Add(new LiteralControl("<div class='form-group credit-card-group'>"));
             //   _rbCREDITCARD.Checked = true;                
                this.Controls.Add(_rbCREDITCARD);

                this.Controls.Add(new LiteralControl("<div class='cc-images'>"));
                List<String> cctypes = CardTypeDataSource.GetAcceptedCreditCardTypes(((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer);

                //No Need to add immages on client request.but need to keep this code for future use.
                //foreach (string type in cctypes)
                //{
                //    if (type.Equals("AMEX", StringComparison.InvariantCultureIgnoreCase) && !String.IsNullOrEmpty(_imgCREDITCARD_AmericanExpress.ImageUrl))
                //    {
                //        _imgCREDITCARD_AmericanExpress.CssClass = "cc-image";
                //        this.Controls.Add(_imgCREDITCARD_AmericanExpress);
                //    }
                //    else if (type.Equals("DISCOVER", StringComparison.InvariantCultureIgnoreCase) && !String.IsNullOrEmpty(_imgCREDITCARD_Discover.ImageUrl))
                //    {
                //        _imgCREDITCARD_Discover.CssClass = "cc-image";
                //        this.Controls.Add(_imgCREDITCARD_Discover);
                //    }
                //    else if (type.Equals("MasterCard", StringComparison.InvariantCultureIgnoreCase) && !String.IsNullOrEmpty(_imgCREDITCARD_MasterCard.ImageUrl))
                //    {
                //        _imgCREDITCARD_MasterCard.CssClass = "cc-image";
                //        this.Controls.Add(_imgCREDITCARD_MasterCard);
                //    }
                //    else if (type.Equals("VISA", StringComparison.InvariantCultureIgnoreCase) && !String.IsNullOrEmpty(_imgCREDITCARD_Visa.ImageUrl))
                //    {
                //        _imgCREDITCARD_Visa.CssClass = "cc-image";
                //        this.Controls.Add(_imgCREDITCARD_Visa);
                //    }
                //    else if (type.Equals("Laser", StringComparison.InvariantCultureIgnoreCase) && !String.IsNullOrEmpty(_imgCREDITCARD_Laser.ImageUrl))
                //    {
                //        _imgCREDITCARD_Laser.CssClass = "cc-image";
                //        this.Controls.Add(_imgCREDITCARD_Laser);
                //    }
                //    else if (type.Equals("Maestro", StringComparison.InvariantCultureIgnoreCase) && !String.IsNullOrEmpty(_imgCREDITCARD_Maestro.ImageUrl))
                //    {
                //        _imgCREDITCARD_Maestro.CssClass = "cc-image";
                //        this.Controls.Add(_imgCREDITCARD_Maestro);
                //    }
                //    else if (type.Equals("Visa Debit", StringComparison.InvariantCultureIgnoreCase) && !String.IsNullOrEmpty(_imgCREDITCARD_VisaDebit.ImageUrl))
                //    {
                //        _imgCREDITCARD_VisaDebit.CssClass = "cc-image";
                //        this.Controls.Add(_imgCREDITCARD_VisaDebit);
                //    }
                //    else if (type.Equals("Visa Electron", StringComparison.InvariantCultureIgnoreCase) && !String.IsNullOrEmpty(_imgCREDITCARD_VisaElectron.ImageUrl))
                //    {
                //        _imgCREDITCARD_VisaElectron.CssClass = "cc-image";
                //        this.Controls.Add(_imgCREDITCARD_VisaElectron);
                //    }
                //    else if (type.Equals("JCB", StringComparison.InvariantCultureIgnoreCase) && !String.IsNullOrEmpty(_imgCREDITCARD_Jcb.ImageUrl))
                //    {
                //        _imgCREDITCARD_Jcb.CssClass = "cc-image";
                //        this.Controls.Add(_imgCREDITCARD_Jcb);
                //    }
                //    else if (type.Equals("Diners", StringComparison.InvariantCultureIgnoreCase) && !String.IsNullOrEmpty(_imgCREDITCARD_Diners.ImageUrl))
                //    {
                //        _imgCREDITCARD_Diners.CssClass = "cc-image";
                //        this.Controls.Add(_imgCREDITCARD_Diners);
                //    }
                //}
                this.Controls.Add(new LiteralControl("</div>"));
                this.Controls.Add(new LiteralControl("</div>"));
                this.Controls.Add(new LiteralControl("<div class='form-group'>"));
                this.Controls.Add(_lblCREDITCARD);
                this.Controls.Add(new LiteralControl("</div>"));
            }
            if (this.ShowPURCHASEORDER)
            {
                paymentMethodsEnabled++;
                this.Controls.Add(new LiteralControl("<div class='form-group'>"));
                this.Controls.Add(_rbPURCHASEORDER);
                this.Controls.Add(new LiteralControl("</div>"));
                if (_imgPURCHASEORDER.ImageUrl.Length != 0)
                {
                    this.Controls.Add(_imgPURCHASEORDER);
                }
                this.Controls.Add(new LiteralControl("<div class='form-group'>"));
                this.Controls.Add(_lblPURCHASEORDER);
                this.Controls.Add(new LiteralControl("</div>"));
            }
            if (this.ShowCODMONEYORDER)
            {
                paymentMethodsEnabled++;
                this.Controls.Add(new LiteralControl("<div class='form-group'>"));
                this.Controls.Add(_rbCODMONEYORDER);
                this.Controls.Add(new LiteralControl("</div>"));
                if (_imgCODMONEYORDER.ImageUrl.Length != 0)
                {
                    this.Controls.Add(_imgCODMONEYORDER);
                }
                this.Controls.Add(new LiteralControl("<div class='form-group'>"));
                this.Controls.Add(_lblCODMONEYORDER);
                this.Controls.Add(new LiteralControl("</div>"));
            }
            if (this.ShowCODCOMPANYCHECK)
            {
                paymentMethodsEnabled++;
                this.Controls.Add(new LiteralControl("<div class='form-group'>"));
                this.Controls.Add(_rbCODCOMPANYCHECK);
                this.Controls.Add(new LiteralControl("</div>"));
                if (_imgCODCOMPANYCHECK.ImageUrl.Length != 0)
                {
                    this.Controls.Add(_imgCODCOMPANYCHECK);
                }
                this.Controls.Add(new LiteralControl("<div class='form-group'>"));
                this.Controls.Add(_lblCODCOMPANYCHECK);
                this.Controls.Add(new LiteralControl("</div>"));
            }
            if (this.ShowCODNET30)
            {
                paymentMethodsEnabled++;
                this.Controls.Add(new LiteralControl("<div class='form-group'>"));
                this.Controls.Add(_rbCODNET30);
                this.Controls.Add(new LiteralControl("</div>"));
                if (_imgCODNET30.ImageUrl.Length != 0)
                {
                    this.Controls.Add(_imgCODNET30);
                }
                this.Controls.Add(new LiteralControl("<div class='form-group'>"));
                this.Controls.Add(_lblCODNET30);
                this.Controls.Add(new LiteralControl("</div>"));
            }

            if (this.ShowAmazonSimplePay)
            {
                paymentMethodsEnabled++;
                this.Controls.Add(new LiteralControl("<div class='form-group'>"));
                this.Controls.Add(_rbAMAZONSIMPLEPAY);
                this.Controls.Add(new LiteralControl("</div>"));
                if (_imgAMAZONSIMPLEPAY.ImageUrl.Length != 0)
                {
                    this.Controls.Add(_imgAMAZONSIMPLEPAY);
                }
                this.Controls.Add(new LiteralControl("<div class='form-group'>"));
                this.Controls.Add(_lblAMAZONSIMPLEPAY);
                this.Controls.Add(new LiteralControl("</div>"));
            }
            if (this.ShowREQUESTQUOTE)
            {
                paymentMethodsEnabled++;
                this.Controls.Add(new LiteralControl("<div class='form-group'>"));
                this.Controls.Add(_rbREQUESTQUOTE);
                this.Controls.Add(new LiteralControl("</div>"));
                if (_imgREQUESTQUOTE.ImageUrl.Length != 0)
                {
                    this.Controls.Add(_imgREQUESTQUOTE);
                }
                this.Controls.Add(new LiteralControl("<div class='form-group'>"));
                this.Controls.Add(_lblREQUESTQUOTE);
                this.Controls.Add(new LiteralControl("</div>"));
            }
            if (this.ShowCHECKBYMAIL)
            {
                paymentMethodsEnabled++;
                this.Controls.Add(new LiteralControl("<div class='form-group'>"));
                this.Controls.Add(_rbCHECKBYMAIL);
                this.Controls.Add(new LiteralControl("</div>"));
                if (_imgCHECKBYMAIL.ImageUrl.Length != 0)
                {
                    this.Controls.Add(_imgCHECKBYMAIL);
                }
                this.Controls.Add(new LiteralControl("<div class='form-group'>"));
                this.Controls.Add(_lblCHECKBYMAIL);
                this.Controls.Add(new LiteralControl("</div>"));
            }
            if (this.ShowCOD)
            {
                paymentMethodsEnabled++;
                this.Controls.Add(new LiteralControl("<div class='form-group'>"));
                this.Controls.Add(_rbCOD);
                this.Controls.Add(new LiteralControl("</div>"));
                if (_imgCOD.ImageUrl.Length != 0)
                {
                    this.Controls.Add(_imgCOD);
                }
                this.Controls.Add(new LiteralControl("<div class='form-group'>"));
                this.Controls.Add(_lblCOD);
                this.Controls.Add(new LiteralControl("</div>"));
            }
            if (this.ShowECHECK)
            {
                paymentMethodsEnabled++;
                this.Controls.Add(new LiteralControl("<div class='form-group'>"));
                this.Controls.Add(_rbECHECK);
                this.Controls.Add(new LiteralControl("</div>"));
                if (_imgECHECK.ImageUrl.Length != 0)
                {
                    this.Controls.Add(_imgECHECK);
                }
                this.Controls.Add(new LiteralControl("<div class='form-group'>"));
                this.Controls.Add(_lblECHECK);
                this.Controls.Add(new LiteralControl("</div>"));
            }
            if (this.ShowCARDINALMYECHECK)
            {
                paymentMethodsEnabled++;
                this.Controls.Add(new LiteralControl("<div class='form-group'>"));
                this.Controls.Add(_rbCARDINALMYECHECK);
                this.Controls.Add(new LiteralControl("</div>"));
                if (_imgCARDINALMYECHECK.ImageUrl.Length != 0)
                {
                    this.Controls.Add(_imgCARDINALMYECHECK);
                }
                this.Controls.Add(new LiteralControl("<div class='form-group'>"));
                this.Controls.Add(_lblCARDINALMYECHECK);
                this.Controls.Add(new LiteralControl("</div>"));
            }
            if (this.ShowMICROPAY)
            {
                if (!(Customer.Current.CustomerLevelName == "Public" | Customer.Current.CustomerLevelID == 0))
                {
                    paymentMethodsEnabled++;
                    this.Controls.Add(new LiteralControl("<div class='form-group'>"));
                    this.Controls.Add(_rbMICROPAY);
                    this.Controls.Add(new LiteralControl("</div>"));
                    if (_imgMICROPAY.ImageUrl.Length != 0)
                    {
                        this.Controls.Add(_imgMICROPAY);
                    }
                    this.Controls.Add(new LiteralControl("<div class='form-group'>"));
                    this.Controls.Add(_lblMICROPAY);
                    this.Controls.Add(new LiteralControl("</div>"));
                }
            }

            if (this.ShowMONEYBOOKERSQUICKCHECKOUT)
            {
                paymentMethodsEnabled++;
                this.Controls.Add(new LiteralControl("<div class='form-group'>"));
                this.Controls.Add(_rbMONEYBOOKERSQUICKCHECKOUT);
                this.Controls.Add(new LiteralControl("</div>"));
                if (_imgMONEYBOOKERSQUICKCHECKOUT.ImageUrl.Length != 0)
                {
                    this.Controls.Add(_imgMONEYBOOKERSQUICKCHECKOUT);
                }
                this.Controls.Add(new LiteralControl("<div class='form-group'>"));
                this.Controls.Add(_lblMONEYBOOKERSQUICKCHECKOUT);
                this.Controls.Add(new LiteralControl("</div>"));
            }
            if (this.ShowSECURENETVAULT)
            {
                paymentMethodsEnabled++;
                this.Controls.Add(new LiteralControl("<div class='form-group'>"));
                this.Controls.Add(_rbSECURENETVAULT);
                this.Controls.Add(new LiteralControl("</div>"));
                if (_imgSECURENETVAULT.ImageUrl.Length != 0)
                {
                    this.Controls.Add(_imgSECURENETVAULT);
                }
                this.Controls.Add(new LiteralControl("<div class='form-group'>"));
                this.Controls.Add(_lblMONEYBOOKERSQUICKCHECKOUT);
                this.Controls.Add(new LiteralControl("</div>"));

            }

            HandlePayPalOptions(ref paymentMethodsEnabled, ref disablePaymentMethod);

            this.Controls.Add(new LiteralControl("</div>"));

            if (this.ShowMICROPAYMessage)
            {
                if (!(Customer.Current.CustomerLevelName == "Public" | Customer.Current.CustomerLevelID == 0))
                {
                    this.Controls.Add(new LiteralControl("<div class='page-row'>"));
                    this.Controls.Add(_msgMICROPAY);
                    this.Controls.Add(new LiteralControl("</div>"));
                }
            }

            if (this.MONEYBOOKERSQUICKCHECKOUTChecked && !String.IsNullOrEmpty(MONEYBOOKERSQUICKCHECKOUTLabel))
            {
                this.Controls.Add(new LiteralControl("<div class='page-row'>"));
                this.Controls.Add(_msgMONEYBOOKERSQUICKCHECKOUT);
                this.Controls.Add(new LiteralControl("</div>"));
            }

            if (this.PAYPALChecked)
            {
                this.Controls.Add(new LiteralControl("<div class='page-row'>"));
                this.Controls.Add(_msgPAYPAL);
                this.Controls.Add(new LiteralControl("</div>"));
            }

            if (this.PAYPALEXPRESSChecked)
            {
                this.Controls.Add(new LiteralControl("<div class='page-row'>"));
                this.Controls.Add(_msgPAYPALEXPRESS);
                this.Controls.Add(new LiteralControl("</div>"));
            }
        }

        private void HandlePayPalOptions(ref int paymentMethodsEnabled, ref Action disablePaymentMethod)
        {
            string payPalOptionSelected = null;

            string paymentRowTop = "";
            string paymentRowBottom = "";
            string paymentCellBottom = "";

            if (ShowPAYPALEMBEDDEDCHECKOUT)
            {
                PAYPALChecked = false;
                PAYPALEXPRESSChecked = false;

                paymentMethodsEnabled++;
                payPalOptionSelected = "embedded";
                disablePaymentMethod = new Action(() => _rbPAYPALEMBEDDEDCHECKOUT.Visible = _imgPAYPALEMBEDDEDCHECKOUT.Visible = false);

                this.Controls.Add(new LiteralControl(paymentRowTop));
                this.Controls.Add(_rbPAYPALEMBEDDEDCHECKOUT);
                this.Controls.Add(new LiteralControl(paymentCellBottom));
                if (_imgPAYPALEMBEDDEDCHECKOUT.ImageUrl.Length != 0)
                {
                    this.Controls.Add(_imgPAYPALEMBEDDEDCHECKOUT);
                }
                this.Controls.Add(new LiteralControl(paymentCellBottom));
                this.Controls.Add(_lblPAYPALEMBEDDEDCHECKOUT);
                this.Controls.Add(new LiteralControl(paymentRowBottom));
            }

            if (ShowPAYPALEXPRESS && string.IsNullOrEmpty(payPalOptionSelected))
            {
                PAYPALChecked = false;
                PAYPALEMBEDDEDCHECKOUTChecked = false;

                paymentMethodsEnabled++;
                payPalOptionSelected = "express";
                disablePaymentMethod = new Action(() => _rbPAYPALEXPRESS.Visible = _imgPAYPALEXPRESS.Visible = false);

                this.Controls.Add(new LiteralControl(paymentRowTop));
                this.Controls.Add(_rbPAYPALEXPRESS);
                this.Controls.Add(new LiteralControl(paymentCellBottom));
                if (_imgPAYPALEXPRESS.ImageUrl.Length != 0)
                {
                    this.Controls.Add(_imgPAYPALEXPRESS);
                }
                this.Controls.Add(new LiteralControl(paymentCellBottom));
                this.Controls.Add(_lblPAYPALEXPRESS);
                this.Controls.Add(new LiteralControl(paymentRowBottom));
            }

            if (ShowPAYPAL && string.IsNullOrEmpty(payPalOptionSelected))
            {
                PAYPALEXPRESSChecked = false;
                PAYPALEMBEDDEDCHECKOUTChecked = false;

                paymentMethodsEnabled++;
                payPalOptionSelected = "standard";
                disablePaymentMethod = new Action(() => _rbPAYPAL.Visible = _imgPAYPAL.Visible = false);

                this.Controls.Add(new LiteralControl(paymentRowTop));
                this.Controls.Add(_rbPAYPAL);
                this.Controls.Add(new LiteralControl(paymentCellBottom));
                if (_imgPAYPAL.ImageUrl.Length != 0)
                {
                    this.Controls.Add(_imgPAYPAL);
                }
                this.Controls.Add(new LiteralControl(paymentCellBottom));
                this.Controls.Add(_lblPAYPAL);
                this.Controls.Add(new LiteralControl(paymentRowBottom));
            }

            //if there is only one availible payment method and its pay pal then don't show the radio button or image
            if (paymentMethodsEnabled == 1 && disablePaymentMethod != null)
            {
                switch (payPalOptionSelected)
                {
                    case "embedded":
                        PAYPALEMBEDDEDCHECKOUTChecked = true;
                        break;
                    case "express":
                        PAYPALEXPRESSChecked = true;
                        break;
                    case "standard":
                        PAYPALChecked = true;
                        break;
                }

                disablePaymentMethod();
            }
        }

        /// <summary>
        /// Handles the CheckChanged event of the PaymentMethod control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void PaymentMethod_CheckChanged(object sender, EventArgs e)
        {
            OnPaymentMethodChanged(e);
        }

        /// <summary>
        /// Raises the <see cref="E:PaymentMethodChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnPaymentMethodChanged(EventArgs e)
        {
            if (PaymentMethodChanged != null)
            {
                PaymentMethodChanged(this, e);
            }
        }

        public void ClearSelection()
        {
            _rbCREDITCARD.Checked = false;
            _rbPURCHASEORDER.Checked = false;
            _rbCODMONEYORDER.Checked = false;
            _rbCODCOMPANYCHECK.Checked = false;
            _rbCODNET30.Checked = false;
            _rbPAYPAL.Checked = false;
            _rbREQUESTQUOTE.Checked = false;
            _rbCHECKBYMAIL.Checked = false;
            _rbCOD.Checked = false;
            _rbECHECK.Checked = false;
            _rbCARDINALMYECHECK.Checked = false;
            _rbMICROPAY.Checked = false;
            _rbPAYPALEXPRESS.Checked = false;
            _rbAMAZONSIMPLEPAY.Checked = false;
            _rbMONEYBOOKERSQUICKCHECKOUT.Checked = false;
            _rbSECURENETVAULT.Checked = false;
            _rbPAYPALEMBEDDEDCHECKOUT.Checked = false;
        }
        #endregion

    }
}
