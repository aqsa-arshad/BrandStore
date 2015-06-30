// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.ComponentModel;
using System.Net;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontControls
{
    /// <summary>
    /// Provides a control that display the Order SubTotal, Tax, Shipping Cost and Order Total.
    /// </summary>
    [ToolboxData("<{0}:CartSummary runat=server></{0}:CartSummary>")]
    public class CartSummary : CompositeControl
    {
        #region constant variables
        private const string SETTINGS_CATEGORY = "ASPDNSF Settings";

        #endregion

        #region private properties
        private ShoppingCart _dataSource = null;
        #endregion

        #region Web Controls Instantiation

        private Label lblSubTotalNoDiscountCaption = new Label();
        private Label lblSubTotalNoDiscount = new Label();

        private Label lblSubTotalCaption = new Label();
        private Label lblSubTotal = new Label();

        private Label lblLineItemDiscountCaption = new Label();
        private Label lblLineItemDiscount = new Label();

        private Label lblShippingCostCaption = new Label();
        private Label lblShippingCost = new Label();

        private Label lblShippingVatDisplay = new Label();
        private Label lblShippingMethod = new Label();

        private Label lblTaxCaption = new Label();
        private Label lblTax = new Label();

        private Label lblOrderDiscountCaption = new Label();
        private Label lblOrderDiscount = new Label();

        private Label lblOrderSubtotalCaption = new Label();
        private Label lblOrderSubtotal = new Label();

        private Label lblTotalCaption = new Label();
        private Label lblTotal = new Label();

        private Label lblGiftCardTotalCaption = new Label();
        private Label lblGiftCardTotal = new Label();
        
        #endregion

        /// <summary>
        /// Creates the child controls in a control derived from System.Web.UI.WebControls.CompositeControl.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();

            IntializeControlsDefaultValues();
            AssignDataSourceContentToControls(DataSource);


            /********************************/
            /******** CREATE CONTROLS *******/
            /********************************/

            if (DesignMode || DataSource != null && !DataSource.IsEmpty())
            {
                //SubTotal
                if (ShowSubTotal || UseInAjaxMiniCart)
                {
                    Controls.Add(new LiteralControl("        <div class='page-row text-right cart-summary-subtotal'>"));
                    Controls.Add(new LiteralControl("          <span class='cart-label'>"));
                    Controls.Add(lblSubTotalNoDiscountCaption);
                    Controls.Add(new LiteralControl("          </span>"));
                    Controls.Add(new LiteralControl("          <span class='cart-value cart-price'>"));
                    Controls.Add(lblSubTotalNoDiscount);
                    Controls.Add(new LiteralControl("          </span>"));
                    Controls.Add(new LiteralControl("        </div>"));

                    if (_dataSource.DiscountResults.Sum(dr => dr.LineItemTotal) < 0)
                    {
                        Controls.Add(new LiteralControl("        <div class='page-row text-right cart-summary-discount'>"));
                        Controls.Add(new LiteralControl("          <span class='cart-label'>"));
                        Controls.Add(lblLineItemDiscountCaption);
                        Controls.Add(new LiteralControl("          </span>"));
                        Controls.Add(new LiteralControl("           <span class='cart-value'>"));
                        Controls.Add(lblLineItemDiscount);
                        Controls.Add(new LiteralControl("          </span>"));
                        Controls.Add(new LiteralControl("        </div>"));
                    }

                    if (AppLogic.AppConfigBool("Debug.DisplayOrderSummaryDiagnostics"))
                    {
                        Controls.Add(new LiteralControl("        <div class='page-row text-right cart-summary-diagnostics'>"));
                        Controls.Add(new LiteralControl("          <span class='cart-label'>"));
                        Controls.Add(lblSubTotalCaption);
                        Controls.Add(new LiteralControl("          </span>"));
                        Controls.Add(new LiteralControl("          <span class='cart-value'>"));
                        Controls.Add(lblSubTotal);
                        Controls.Add(new LiteralControl("          </span>"));
                        Controls.Add(new LiteralControl("        </div>"));
                    }
                }

                //Shipping
                if (ShowShipping && !UseInAjaxMiniCart)
                {
                    Controls.Add(new LiteralControl("       <div class='page-row text-right cart-summary-shipping'>"));
  
                    //shipping caption
                    Controls.Add(new LiteralControl("                <span class='cart-shipping-cost-caption'>"));
                    Controls.Add(lblShippingCostCaption);
                    Controls.Add(new LiteralControl("                </span>"));

                    //vat display
                    if (DesignMode || DataSource.VatEnabled)
                    {
                        Controls.Add(new LiteralControl("                <span class='cart-shipping-cost-vat'>"));
                        Controls.Add(lblShippingVatDisplay);
                        Controls.Add(new LiteralControl("                </span>"));
                    }

                    //shipping method
                    if (DesignMode || !DataSource.WithMultipleShippingAddresses && !DataSource.WithGiftRegistryConponents && DataSource.FirstCartItem.ShippingMethod.Length != 0 || UseInAjaxMiniCart)
                    {
                        Controls.Add(new LiteralControl("                <span class='cart-shipping-method'>"));
                        Controls.Add(lblShippingMethod);
                        Controls.Add(new LiteralControl("                </span>"));
                    }

                    Controls.Add(new LiteralControl("          <span class='cart-shipping-cost'>"));
                    Controls.Add(lblShippingCost);
                    Controls.Add(new LiteralControl("          </span>"));
                    Controls.Add(new LiteralControl("        </div>"));
                }

                //Tax            
                if (ShowTax && !UseInAjaxMiniCart)
               { 
                    if (DesignMode || DataSource != null)
                    {
                        if (DesignMode || !this.DataSource.VatIsInclusive)
                        {
                            Controls.Add(new LiteralControl("         <div class='page-row text-right cart-summary-tax'>"));
                            Controls.Add(new LiteralControl("           <span class='cart-label'>"));
                            Controls.Add(lblTaxCaption);
                            Controls.Add(new LiteralControl("          </span>"));
                            Controls.Add(new LiteralControl("          <span class='cart-label'>"));
                            Controls.Add(lblTax);
                            Controls.Add(new LiteralControl("          </span>"));
                            Controls.Add(new LiteralControl("        </div>"));
                        }
                    }
                }
                
                //Total
                if (ShowTotal && !UseInAjaxMiniCart)
                {
                    if (_dataSource.DiscountResults.Sum(dr => dr.OrderTotal) < 0)
                    {
                        if (AppLogic.AppConfigBool("Debug.DisplayOrderSummaryDiagnostics"))
                        {
                            Controls.Add(new LiteralControl("        <div>"));
                            Controls.Add(new LiteralControl("          <span>"));
                            Controls.Add(lblOrderSubtotalCaption);
                            Controls.Add(new LiteralControl("          </span>"));
                            Controls.Add(new LiteralControl("          <span>"));
                            Controls.Add(lblOrderSubtotal);
                            Controls.Add(new LiteralControl("          </span>"));
                            Controls.Add(new LiteralControl("        </div>"));
                        }

                        Controls.Add(new LiteralControl("        <div>"));
                        Controls.Add(new LiteralControl("          <span>"));
                        Controls.Add(lblOrderDiscountCaption);
                        Controls.Add(new LiteralControl("          </span>"));
                        Controls.Add(new LiteralControl("          <span>"));
                        Controls.Add(lblOrderDiscount);
                        Controls.Add(new LiteralControl("          </span>"));
                        Controls.Add(new LiteralControl("        </div>"));
                    }

                    if (ShowGiftCardTotal && DataSource.Coupon.CouponType == CouponTypeEnum.GiftCard)
                    {
                        Controls.Add(new LiteralControl("         <div class='page-row text-right cart-summary-giftcard'>"));
                        Controls.Add(new LiteralControl("          <span class='cart-label'>"));
                        Controls.Add(lblGiftCardTotalCaption);
                        Controls.Add(new LiteralControl("          </span>"));
                        Controls.Add(new LiteralControl("          <span class='cart-value'>"));
                        Controls.Add(lblGiftCardTotal);
                        Controls.Add(new LiteralControl("          </span>"));
                        Controls.Add(new LiteralControl("        </div>"));
                    }

                    Controls.Add(new LiteralControl("        <div class='page-row text-right cart-summary-total'>"));
                    Controls.Add(new LiteralControl("          <span class='cart-label'>"));
                    Controls.Add(lblTotalCaption);
                    Controls.Add(new LiteralControl("          </span>"));
                    Controls.Add(new LiteralControl("         <span class='cart-value'>"));
                    Controls.Add(lblTotal);
                    Controls.Add(new LiteralControl("          </span>"));
                    Controls.Add(new LiteralControl("        </div>"));
                }

                // Promotions
                if (AppLogic.AppConfigBool("Debug.DisplayOrderSummaryDiagnostics") && _dataSource.DiscountResults.Any())
                {
                    Controls.Add(new LiteralControl("        <div>DEBUG: Promotion Calculations</div>"));

                    foreach (AspDotNetStorefront.Promotions.IDiscountResult result in DataSource.DiscountResults)
                    {
                        Controls.Add(new LiteralControl("        <div class='page-row text-right cart-summary-promotion'>"));
                        Controls.Add(new LiteralControl("          <span class='cart-label'>"));
                        Label usageLabel = new Label();
                        usageLabel.Text = result.Promotion.Code;
                        Controls.Add(usageLabel);
                        Controls.Add(new LiteralControl("          </span>"));
                        Controls.Add(new LiteralControl("          <span class='cart-value'>"));
                        Label discountLabel = new Label();
                        discountLabel.Text = "-" + result.TotalDiscount.ToString("c");
                        Controls.Add(discountLabel);
                        Controls.Add(new LiteralControl("          </span>"));
                        Controls.Add(new LiteralControl("        </div>"));
                    }
                }
            }
        }

        #region Helper Methods


        /// <summary>
        /// Initialize default values for the controls
        /// </summary>
        private void IntializeControlsDefaultValues()
        {
            //Subtotal
            lblSubTotalNoDiscountCaption.ID = "lblSubTotalNoDiscountCaption";
            lblSubTotalNoDiscountCaption.Text = SubTotalCaption;

            lblSubTotalNoDiscount.ID = "lblSubTotalNoDiscount";
            if (DesignMode)
            {
                lblSubTotalNoDiscount.Text = "0.00";
            }

            lblLineItemDiscountCaption.ID = "lblLineItemDiscountCaption";
            lblLineItemDiscountCaption.Text = LineItemDiscountCaption;

            lblLineItemDiscount.ID = "lblLineItemDiscount";
            if (DesignMode)
            {
                lblLineItemDiscount.Text = "0.00";
            }

            lblSubTotalCaption.ID = "lblSubTotalCaption";
            lblSubTotalCaption.Text = SubTotalCaption;

            lblSubTotal.ID = "lblSubTotal";
            if (DesignMode)
            {
                lblSubTotal.Text = "0.00";
            }

            //Shipping Cost
            lblShippingCostCaption.ID = "lblShippingCaption";
            lblShippingCostCaption.Text = ShippingCaption;

            lblShippingCost.ID = "lblShippingCost";
            if (DesignMode)
            {
                lblShippingCost.Text = "0.00";
            }

            //Tax
            lblTaxCaption.ID = "lblTaxCaption";
            lblTaxCaption.Text = TaxCaption;

            lblTax.ID = "lblTax";
            if (DesignMode)
            {
                lblTax.Text = "0.00";
            }

            // Order Discount
            lblOrderSubtotalCaption.ID = "lblOrderDiscountCaption";
            lblOrderSubtotalCaption.Text = OrderDiscountCaption;

            lblOrderDiscountCaption.ID = "lblOrderDiscountCaption";
            lblOrderDiscountCaption.Text = OrderDiscountCaption;

            //Total
            lblTotalCaption.ID = "lblTotalCaption";
            lblTotalCaption.Text = TotalCaption;

            //GiftCard
            lblGiftCardTotalCaption.ID = "lblGiftCardTotalCaption";
            lblGiftCardTotalCaption.Text = GiftCardTotalCaption; 

            lblGiftCardTotal.ID = "lblGiftCardTotal";


            lblTotal.ID = "lblTotal";
            if (DesignMode)
            {
                lblTotal.Text = "0.00";
            }
        }
        
        /// <summary>
        /// Assigns the datasource items into the control
        /// </summary>
        /// <param name="cart">The shoppingcart instance</param>
        private void AssignDataSourceContentToControls(ShoppingCart cart)
        {
            if (DataSource != null && !cart.IsEmpty())
            {
                //SubTotal
                Decimal subTotalNoDiscount = cart.SubTotal(false, false, true, true, true, true);
                Decimal lineItemDiscount = cart.DiscountResults.Sum(dr => dr.LineItemTotal);
                Decimal subTotalDiscount = subTotalNoDiscount + lineItemDiscount;

                lblSubTotalNoDiscount.Text = Localization.CurrencyStringForDisplayWithExchangeRate(subTotalNoDiscount, cart.ThisCustomer.CurrencySetting);
                lblLineItemDiscount.Text = Localization.CurrencyStringForDisplayWithExchangeRate(lineItemDiscount, cart.ThisCustomer.CurrencySetting);
                lblSubTotal.Text = Localization.CurrencyStringForDisplayWithExchangeRate(subTotalDiscount, cart.ThisCustomer.CurrencySetting);

                if (CalculateShippingDuringCheckout)
                {
                    lblShippingCost.Text = CalculateShippingDuringCheckoutText;
                }
                else
                {
                    //Shipping
                    lblShippingCostCaption.Text = ShippingCaption;
                    lblShippingCost.Text = cart.FreightVatIncRateDisplayFormat;

                    string vatDisplay = string.Empty;
                    if (cart.VatEnabled)
                    {
                        vatDisplay = ShippingVatExCaption;
                        if (cart.VatIsInclusive)
                        {
                            vatDisplay = ShippingVatInCaption;
                        }
                    }

                    lblShippingVatDisplay.Text = string.Format("({0})", vatDisplay);

                    if (!UseInAjaxMiniCart)
                    {
                        if (!DataSource.WithMultipleShippingAddresses && !DataSource.WithGiftRegistryConponents && DataSource.FirstCartItem.ShippingMethodID != 0 && DataSource.FirstCartItem.ShippingMethod.Length != 0)
                        {
                            string shippingDisplay = DataSource.FirstCartItem.ShippingMethod;
                            if (shippingDisplay.IndexOf('|') != -1)
                            {
                                string[] method = shippingDisplay.Split('|');
                                shippingDisplay = method[0].Trim();

                            }
                            lblShippingMethod.Text = string.Format("({0})", shippingDisplay);
                        }
                    }
                    else
                    {

                        if (!string.IsNullOrEmpty(cart.LowestfreightName) &&
                            !(cart.IsAllFreeShippingComponents() || cart.ShippingIsFree || cart.ThisCustomer.LevelHasFreeShipping() || 
                              cart.FreeShippingReason == Shipping.FreeShippingReasonEnum.ExceedsFreeShippingThreshold || cart.FreeShippingReason == Shipping.FreeShippingReasonEnum.CouponHasFreeShipping))
                        {
                            lblShippingMethod.Text = string.Format("({0})", cart.LowestfreightName);
                        }
                       
                        if (lblShippingCost.Text.IndexOf("0") != -1)
                        {
                            if (cart.VatIsInclusive)
                            {
                                lblShippingCost.Text = cart.EstimatedShippingTotalDisplay;
                            }
                            else
                            {
                                lblShippingCost.Text = cart.LowestFreightDisplay;
                            }
                        }
                    }

                }


                //Tax
                if (CalculateTaxDuringCheckout)
                {
                    lblTax.Text = CalculateTaxDuringCheckoutText;
                }
                else
                {

                    lblTaxCaption.Text = TaxCaption;
                    lblTax.Text = cart.TaxRateDisplayFormat;

                    if (UseInAjaxMiniCart)
                    {
                        lblTax.Text = CalculateTaxDuringCheckoutText;
                    }


                }

                if (cart.SubTotalRate == cart.SubTotalDiscountIncRate)
                {
                    lblSubTotalCaption.Text = SubTotalCaption;
                }
                else
                {
                    lblSubTotalCaption.Text = SubTotalWithDiscountCaption;
                }


                //GiftCard
                if (this.ShowGiftCardTotal && DataSource.Coupon.CouponType == CouponTypeEnum.GiftCard)
                {
                    lblGiftCardTotal.Text = Localization.CurrencyStringForDisplayWithExchangeRate(-cart.GiftCardTotal, cart.ThisCustomer.CurrencySetting);
                }

                // Order Discount
                Decimal orderTotal = cart.GetTotalRate(true, true); // This is the final discounted order total
                Decimal orderDiscount = cart.DiscountResults.Sum(dr => dr.OrderTotal);
                Decimal orderTotalNoDiscount = orderTotal - orderDiscount;

                lblOrderSubtotalCaption.Text = SubTotalCaption;
                lblOrderSubtotal.Text = Localization.CurrencyStringForDisplayWithExchangeRate(orderTotalNoDiscount, cart.ThisCustomer.CurrencySetting);

                lblOrderDiscountCaption.Text = OrderDiscountCaption;
                lblOrderDiscount.Text = Localization.CurrencyStringForDisplayWithExchangeRate(orderDiscount, cart.ThisCustomer.CurrencySetting);

                //Total
                if (UseInAjaxMiniCart)
                {
                    lblTotalCaption.Text = TotalCaption;
                    lblTotal.Text = cart.EstimatedTotalDisplay;
                }
                else
                {
                    lblTotalCaption.Text = TotalCaption;
                    lblTotal.Text = cart.TotalRateDisplayFormat;
                }
            }
        }



        #endregion

        #region Controls Properties

        [Browsable(false)]
        public ShoppingCart DataSource
        {
            get
            {
                return _dataSource;
            }
            set
            {
                _dataSource = value;
                ChildControlsCreated = false;
            }
        }

        [Browsable(true),
        Category(SETTINGS_CATEGORY),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public bool UseInAjaxMiniCart
        {
            get
            {
                object booleanValue = ViewState["UseInAjaxMiniCart"];
                if (null == booleanValue) { return false; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState["UseInAjaxMiniCart"] = value;
                ChildControlsCreated = false;
            }
        }

        [Browsable(true),
        Category(SETTINGS_CATEGORY),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public bool ShowSubTotal
        {
            get
            {
                object booleanValue = ViewState["ShowSubTotal"];
                if (null == booleanValue) { return true; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState["ShowSubTotal"] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// Gets or sets the value indicating whether the Shipping Cost will be displayed on the summary
        /// </summary>
        [Browsable(true),
        Category(SETTINGS_CATEGORY),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public bool ShowShipping
        {
            get
            {
				
                if (SkipShippingOnCheckout)
                {
                    return false;
                }
                object booleanValue = ViewState["ShowShipping"];
                if (null == booleanValue) { return true; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState["ShowShipping"] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// Gets or sets the value indicating whether the Tax will be displayed on the summary
        /// </summary>
        [Browsable(true),
        Category(SETTINGS_CATEGORY),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public bool ShowTax
        {
            get
            {
                object booleanValue = ViewState["ShowTax"];
                if (null == booleanValue) { return true; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState["ShowTax"] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// Gets or sets the value indicating whether the Total will be displayed on the summary
        /// </summary>
        [Browsable(true),
        Category(SETTINGS_CATEGORY),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public bool ShowTotal
        {
            get
            {
                object booleanValue = ViewState["ShowTotal"];
                if (null == booleanValue) { return true; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState["ShowTotal"] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// Gets or sets the value indicating whether the Total will be displayed on the summary
        /// </summary>
        [Browsable(true),
        Category(SETTINGS_CATEGORY),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public bool IncludeTaxInSubtotal
        {
            get
            {
                object booleanValue = ViewState["IncludeTaxInSubtotal"];
                if (null == booleanValue) { return false; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState["IncludeTaxInSubtotal"] = value;
                ChildControlsCreated = false;
            }
        }


        /// <summary>
        /// Gets or sets the value indicating whether the Gift Card Total will be displayed on the summary
        /// </summary>
        [Browsable(true),
        Category(SETTINGS_CATEGORY),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public bool ShowGiftCardTotal
        {
            get
            {
                object booleanValue = ViewState["ShowGiftCardTotal"];
                if (null == booleanValue) { return false; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState["ShowGiftCardTotal"] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// Gets or sets the value indicating whether the shipping will display the corresponding stringresource text "calculated during checkout".
        /// </summary>
        [Browsable(true),
        Category(SETTINGS_CATEGORY),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public bool CalculateShippingDuringCheckout
        {
            get
            {
                object booleanValue = ViewState["CalculateShippingDuringCheckout"];
                if (null == booleanValue) { return false; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState["CalculateShippingDuringCheckout"] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// a stringresource value equivalent to "calculated during checkout"
        /// </summary>
        [Browsable(true),
        Category(SETTINGS_CATEGORY),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public string CalculateShippingDuringCheckoutText
        {
            get
            {
                if ((object)ViewState["CalculateShippingDuringCheckoutText"] == null)
                {
                    return string.Empty;
                }
                return ViewState["CalculateShippingDuringCheckoutText"].ToString();
            }
            set
            {
                ViewState["CalculateShippingDuringCheckoutText"] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// Gets or sets the value indicating whether the tax will display the corresponding stringresource text "calculated during checkout".
        /// </summary>
        [Browsable(true),
        Category(SETTINGS_CATEGORY),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public bool CalculateTaxDuringCheckout
        {
            get
            {
                object booleanValue = ViewState["CalculateTaxDuringCheckout"];
                if (null == booleanValue) { return false; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState["CalculateTaxDuringCheckout"] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// a stringresource value equivalent to "calculated during checkout"
        /// </summary>
        [Browsable(true),
        Category(SETTINGS_CATEGORY),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public string CalculateTaxDuringCheckoutText
        {
            get
            {
                if ((object)ViewState["CalculateTaxDuringCheckoutText"] == null)
                {
                    return string.Empty;
                }
                return ViewState["CalculateTaxDuringCheckoutText"].ToString();
            }
            set
            {
                ViewState["CalculateTaxDuringCheckoutText"] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// The line item discount caption
        /// </summary>
        [Browsable(true),
        Category(SETTINGS_CATEGORY),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public string LineItemDiscountCaption
        {
            get
            {
                if ((object)ViewState["LineItemDiscountCaption"] == null)
                {
                    return string.Empty;
                }
                return ViewState["LineItemDiscountCaption"].ToString();
            }
            set
            {
                ViewState["LineItemDiscountCaption"] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// The subtotal caption
        /// </summary>
        [Browsable(true),
        Category(SETTINGS_CATEGORY),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public string SubTotalCaption
        {
            get
            {
                if ((object)ViewState["SubTotalCaption"] == null)
                {
                    return string.Empty;
                }
                return ViewState["SubTotalCaption"].ToString();
            }
            set
            {
                ViewState["SubTotalCaption"] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// The subtotal with discount caption
        /// </summary>
        [Browsable(true),
        Category(SETTINGS_CATEGORY),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public string SubTotalWithDiscountCaption
        {
            get
            {
                if ((object)ViewState["SubTotalWithDiscountCaption"] == null)
                {
                    return string.Empty;
                }
                return ViewState["SubTotalWithDiscountCaption"].ToString();
            }
            set
            {
                ViewState["SubTotalWithDiscountCaption"] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// The shipping caption
        /// </summary>
        [Browsable(true),
        Category(SETTINGS_CATEGORY),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public string ShippingCaption
        {
            get
            {
                if ((object)ViewState["ShippingCaption"] == null)
                {
                    return string.Empty;
                }
                return ViewState["ShippingCaption"].ToString();
            }
            set
            {
                ViewState["ShippingCaption"] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// The VAT inclusive shipping display caption
        /// </summary>
        [Browsable(true),
        Category(SETTINGS_CATEGORY),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public string ShippingVatInCaption
        {
            get
            {
                if ((object)ViewState["ShippingVatInCaption"] == null)
                {
                    return string.Empty;
                }
                return ViewState["ShippingVatInCaption"].ToString();
            }
            set
            {
                ViewState["ShippingVatInCaption"] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// The VAT exclusive shipping display caption
        /// </summary>
        [Browsable(true),
        Category(SETTINGS_CATEGORY),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public string ShippingVatExCaption
        {
            get
            {
                if ((object)ViewState["ShippingVatExCaption"] == null)
                {
                    return string.Empty;
                }
                return ViewState["ShippingVatExCaption"].ToString();
            }
            set
            {
                ViewState["ShippingVatExCaption"] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// The shipping method display caption
        /// </summary>
        [Browsable(true),
        Category(SETTINGS_CATEGORY),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public string ShippingMethodCaption
        {
            get
            {
                if ((object)ViewState["ShippingMethodCaption"] == null)
                {
                    return string.Empty;
                }
                return ViewState["ShippingMethodCaption"].ToString();
            }
            set
            {
                ViewState["ShippingMethodCaption"] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// The Tax caption
        /// </summary>
        [Browsable(true),
        Category(SETTINGS_CATEGORY),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public string TaxCaption
        {
            get
            {
                if ((object)ViewState["TaxCaption"] == null)
                {
                    return string.Empty;
                }
                return ViewState["TaxCaption"].ToString();
            }
            set
            {
                ViewState["TaxCaption"] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// The Order Discount Caption
        /// </summary>
        [Browsable(true),
        Category(SETTINGS_CATEGORY),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public string OrderDiscountCaption
        {
            get
            {
                if ((object)ViewState["OrderDiscountCaption"] == null)
                {
                    return string.Empty;
                }
                return ViewState["OrderDiscountCaption"].ToString();
            }
            set
            {
                ViewState["OrderDiscountCaption"] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// The Total Caption
        /// </summary>
        [Browsable(true),
        Category(SETTINGS_CATEGORY),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public string TotalCaption
        {
            get
            {
                if ((object)ViewState["TotalCaption"] == null)
                {
                    return string.Empty;
                }
                return ViewState["TotalCaption"].ToString();
            }
            set
            {
                ViewState["TotalCaption"] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// The GiftCard Total Caption
        /// </summary>
        [Browsable(true),
        Category(SETTINGS_CATEGORY),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public string GiftCardTotalCaption
        {
            get
            {
                if ((object)ViewState["GiftCardTotalCaption"] == null)
                {
                    return string.Empty;
                }
                return ViewState["GiftCardTotalCaption"].ToString();
            }
            set
            {
                ViewState["GiftCardTotalCaption"] = value;
                ChildControlsCreated = false;
            }
        }


        #region Appconfig based properties

        /// <summary>
        /// Gets or set the value indicating whether the shipping checkout pages will be bypassed
        /// </summary>
        [Browsable(true),
        Category(SETTINGS_CATEGORY),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public bool SkipShippingOnCheckout
        {
            get
            {
                //JH modified fix from ashland trunk
                if (ViewState["SkipShippingOnCheckout"] == null)
                {
                    SkipShippingOnCheckout = AppLogic.AppConfigBool("SkipShippingOnCheckout");
                    return AppLogic.AppConfigBool("SkipShippingOnCheckout");
                }
                //end JH
                object booleanValue = ViewState["SkipShippingOnCheckout"];
                if (null == booleanValue) { return true; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState["SkipShippingOnCheckout"] = value;
                ChildControlsCreated = false;
            }
        }

        #endregion

        #endregion
    }
}
