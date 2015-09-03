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

namespace AspDotNetStorefrontControls
{   
    /// <summary>
    /// Represent ShippingAndTaxEstimatorControl that holds ShippingAndTaxEstimateTableControl and ShippingAndTaxEstimatorAddressControl
    /// </summary>
    public class ShippingAndTaxEstimatorControl : CompositeControl
    {
        #region Variable Declaration

        private Panel _pnlMain = new Panel();
        private Panel _pnlLoading = new Panel();
        private Panel _pnlEstimate = new Panel();
        private Image _imgLoading = new Image();
        private ShippingAndTaxEstimatorAddressControl _ctrlAddress = new ShippingAndTaxEstimatorAddressControl();

        private const string SHOW_ADDRESS_CONTROL = "ShowAddress";
        private const string SETTINGS_CATEGORY = "ASPDNSF Settings";

        #endregion

        #region Constructor

        public ShippingAndTaxEstimatorControl()
        {
            AssignClientReferenceIds();
        }

        #endregion

        #region Properties

        [Browsable(true), Category(SETTINGS_CATEGORY)]
        public Image LoadingImage
        {
            get { return _imgLoading; }
            set { _imgLoading = value; }
        }

        [Browsable(true), Category(SETTINGS_CATEGORY)]
        public bool ShowAddressControl
        {
            get
            {
                object booleanValue = ViewState[SHOW_ADDRESS_CONTROL];
                if (null == booleanValue) { return true; } 

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState[SHOW_ADDRESS_CONTROL] = value;
                ChildControlsCreated = false;
            }
        }

        public ShippingAndTaxEstimatorAddressControl AddressControl
        {
            get { return _ctrlAddress; }
        }

        #endregion


        #region Methods
        /// <summary>
        /// Assign Reference id
        /// </summary>
        private void AssignClientReferenceIds()
        {
            _pnlMain.ID = "Main";
            _pnlLoading.ID = "Loading";
            _pnlEstimate.ID = "Estimate";
            _ctrlAddress.ID = "Address";
        }
        /// <summary>
        /// Create child contro0l
        /// </summary>
        protected override void CreateChildControls()
        {
            _pnlMain.Width = Unit.Percentage(100);
            _pnlLoading.Width = Unit.Percentage(100);
            _pnlEstimate.Width = Unit.Percentage(100);
            
            if (this.ShowAddressControl)
            {
                this.Controls.Add(_ctrlAddress);
                _pnlLoading.Style["display"] = "none";
                this.Controls.Add(new LiteralControl(""));
            }

            _pnlLoading.Attributes["align"] = "center";
            _pnlLoading.Controls.Add(_imgLoading);
            _pnlMain.Controls.Add(_pnlLoading);
            _pnlMain.Controls.Add(_pnlEstimate);

            this.Controls.Add(_pnlMain);

            base.CreateChildControls();
        }

        #endregion

    }
}

