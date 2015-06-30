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
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;

namespace AspDotNetStorefrontControls
{
    /// <summary>
    /// Represent the ShippingAndEstimateTable Control
    /// </summary>
    [ToolboxData("<{0}:ShippingAndTaxEstimateTableControl runat=server></{0}:ShippingEstimatorControl>")]
    public class ShippingAndTaxEstimateTableControl : CompositeControl
    {
        #region Variable Declaration

        private Table _table = new Table();
        private Panel _pnlAddress = new Panel();
        private Label _lblHeader = new Label();
        private Label _lblShippingEstimateCaption = new Label();
        private Label _lblShippingEstimate = new Label();
        private Label _lblTaxEstimateCaption = new Label();
        private Label _lblGiftCardAppliedCaption = new Label();
        private Label _lblGiftCardAppliedEstimate = new Label();
        private Label _lblTaxEstimate = new Label();
        private Label _lblTotalEstimateCaption = new Label();
        private Label _lblTotalEstimate = new Label();
        private Unit _captionWidth = Unit.Percentage(50);
        private Unit _valueWidth = Unit.Percentage(50);

        private const string SETTINGS_CATEGORY = "ASPDNSF Settings";
        private const string SHOW_TAX = "ShowTax";
        private const string SHOW_GIFTCARDAPPLIED = "GiftCardApplied";

        #endregion

        #region Constructor

        public ShippingAndTaxEstimateTableControl()
        {
            AssignClientReferenceIDs();
            AssignDefaultCaptions();
        }

        #endregion

        #region Properties
       
        [Browsable(true), Category(SETTINGS_CATEGORY)]
        public bool ShowTax
        {
            get
            {
                object booleanValue = ViewState[SHOW_TAX];
                if (null == booleanValue) { return true; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState[SHOW_TAX] = value;
                ChildControlsCreated = false;
            }
        }

        [Browsable(true), Category(SETTINGS_CATEGORY)]
        public bool ShowGiftCardApplied
        {
            get
            {
                object booleanValue = ViewState[SHOW_GIFTCARDAPPLIED];
                if (null == booleanValue) { return false; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState[SHOW_GIFTCARDAPPLIED] = value;
                ChildControlsCreated = false;
            }
        }

        public string GiftCardAppliedCaption
        {
            get { return _lblGiftCardAppliedCaption.Text; }
            set { _lblGiftCardAppliedCaption.Text = value; }
        }

        public string GiftCardAppliedEstimate
        {
            get { return _lblGiftCardAppliedEstimate.Text; }
            set { _lblGiftCardAppliedEstimate.Text = value; }
        }


        [Browsable(true), Category(SETTINGS_CATEGORY)]
        public string HeaderCaption
        {
            get { return _lblHeader.Text; }
            set
            {
                _lblHeader.Text = value;
                this.ChildControlsCreated = false;
            }
        }

        [Browsable(true), Category(SETTINGS_CATEGORY)]
        public string ShippingEstimateCaption
        {
            get { return _lblShippingEstimateCaption.Text; }
            set
            {
                _lblShippingEstimateCaption.Text = value;
                this.ChildControlsCreated = false;
            }
        }

        [Browsable(true), Category(SETTINGS_CATEGORY)]
        public string ShippingEstimate
        {
            get { return _lblShippingEstimate.Text; }
            set
            {
                _lblShippingEstimate.Text = value;
                this.ChildControlsCreated = false;
            }
        }

        [Browsable(true), Category(SETTINGS_CATEGORY)]
        public string TaxEstimateCaption
        {
            get { return _lblTaxEstimateCaption.Text; }
            set
            {
                _lblTaxEstimateCaption.Text = value;
                this.ChildControlsCreated = false;
            }
        }

        [Browsable(true), Category(SETTINGS_CATEGORY)]
        public string TaxEstimate
        {
            get { return _lblTaxEstimate.Text; }
            set
            {

                _lblTaxEstimate.Text = value;
                this.ChildControlsCreated = false;
            }
        }

        [Browsable(true), Category(SETTINGS_CATEGORY)]
        public string TotalEstimateCaption
        {
            get { return _lblTotalEstimateCaption.Text; }
            set
            {
                _lblTotalEstimateCaption.Text = value;
                this.ChildControlsCreated = false;
            }
        }

        [Browsable(true), Category(SETTINGS_CATEGORY)]
        public string TotalEstimate
        {
            get { return _lblTotalEstimate.Text; }
            set
            {
                _lblTotalEstimate.Text = value;
                this.ChildControlsCreated = false;
            }
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

        public IList<AspDotNetStorefront.Promotions.IDiscountResult> promotions { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// Assign caption to the control
        /// </summary>
        private void AssignDefaultCaptions()
        {
            _lblHeader.Text = "{Header goes here}";
            _lblShippingEstimateCaption.Text = "{Shipping Estimate Caption}";
            _lblShippingEstimate.Text = "{Shipping Estimate}";
            _lblTaxEstimateCaption.Text = "{Tax Estimate Caption}";
            _lblGiftCardAppliedCaption.Text = "{GiftCardApplied Caption}";
            _lblTaxEstimate.Text = "{Tax Estimate}";
            _lblTotalEstimateCaption.Text = "{Total Estimate Caption}";
            _lblTotalEstimate.Text = "{Total Estimate}";
        }

        private void AssignClientReferenceIDs()
        {
        }
        /// <summary>
        /// Create child control
        /// </summary>
        protected override void CreateChildControls()
        {
            _table.CssClass = "shipping-tax-estimator";

            TableRow tr = new TableRow();
            TableCell tc = new TableCell();
            tc.ColumnSpan = 2;
            tc.Controls.Add(_lblHeader);
            _lblHeader.Width = Unit.Percentage(100);
            tc.CssClass = "header";
            tr.Cells.Add(tc);
            _table.Rows.Add(tr);

            // the shiping estimate row            
            tr = new TableRow();
            tc = new TableCell();
            tc.CssClass = "caption";
            tc.Width = this.CaptionWidth;
            tc.Controls.Add(_lblShippingEstimateCaption);
            tr.Controls.Add(tc);

            tc = new TableCell();
            tc.CssClass = "value";
            tc.Width = this.ValueWidth;
            tc.Controls.Add(_lblShippingEstimate);
            tr.Controls.Add(tc);

            _table.Rows.Add(tr);

            // the Tax estimate row            
            if (this.ShowTax)
            {
                tr = new TableRow();
                tc = new TableCell();
                tc.CssClass = "caption";
                tc.Width = this.CaptionWidth;
                tc.Controls.Add(_lblTaxEstimateCaption);
                tr.Controls.Add(tc);

                tc = new TableCell();
                tc.CssClass = "value";
                tc.Width = this.ValueWidth;
                tc.Controls.Add(_lblTaxEstimate);
                tr.Controls.Add(tc);

                _table.Rows.Add(tr);
            }

            if (this.ShowGiftCardApplied)
            {
                // the Total estimate row            
                tr = new TableRow();
                tc = new TableCell();
                tc.CssClass = "caption";
                tc.Width = this.CaptionWidth;
                tc.Controls.Add(_lblGiftCardAppliedCaption);
                tr.Controls.Add(tc);

                tc = new TableCell();
                tc.CssClass = "value";
                tc.Width = this.ValueWidth;
                tc.Controls.Add(_lblGiftCardAppliedEstimate);
                tr.Controls.Add(tc);

                _table.Rows.Add(tr);
            }

            if (this.promotions != null && promotions.Any())
            {
                foreach (AspDotNetStorefront.Promotions.IDiscountResult result in promotions)
                {
                    Label usageLabel = new Label();
                    Label discountLabel = new Label();

                    usageLabel.Text = result.Promotion.UsageText;
                    discountLabel.Text = "-" + result.TotalDiscount.ToString("c");

                    tr = new TableRow();
                    tc = new TableCell();
                    tc.CssClass = "caption";
                    tc.Width = this.CaptionWidth;
                    tc.Controls.Add(usageLabel);
                    tr.Controls.Add(tc);

                    tc = new TableCell();
                    tc.CssClass = "value";
                    tc.Width = this.ValueWidth;
                    tc.Controls.Add(discountLabel);
                    tr.Controls.Add(tc);

                    _table.Rows.Add(tr);
                }
            }

            // the Total estimate row            
            tr = new TableRow();
            tc = new TableCell();
            tc.CssClass = "caption";
            tc.Width = this.CaptionWidth;
            tc.Controls.Add(_lblTotalEstimateCaption);
            tr.Controls.Add(tc);

            tc = new TableCell();
            tc.CssClass = "value";
            tc.Width = this.ValueWidth;
            tc.Controls.Add(_lblTotalEstimate);
            tr.Controls.Add(tc);

            _table.Rows.Add(tr);

            this.Controls.Add(_table);

            base.CreateChildControls();
        }

        /// <summary>
        /// Renders the html output
        /// </summary>
        /// <param name="writer"></param>
        public override void RenderControl(HtmlTextWriter writer)
        {
            this.EnsureChildControls();
            base.RenderControl(writer);
        }

        #endregion

    }
}

