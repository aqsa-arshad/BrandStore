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
    /// Provides a control that display the cart individual line item description.
    /// </summary>
    [ToolboxData("<{0}:ShoppingCartLineItemDescriptionControl runat=server></{0}:ShoppingCartLineItemDescriptionControl>")]
    public class ShoppingCartLineItemDescriptionControl : CompositeControl
    {
        #region constant variables
        private const string SETTINGS_CATEGORY = "ASPDNSF Settings";
        #endregion

        #region Web Controls Instantiation

        Image imgProductPic = new Image();

        private Label lblSKUCaption = new Label();
        private Label lblImgLinkText = new Label();
        private Label lblGiftRegistryCaption = new Label();
        private Label lblSizeCaption = new Label();
        private Label lblColorCaption = new Label();
        private Label lblTextOptionCaption = new Label();
        private Label lblItemNotesCaption = new Label();
        private Label lblProductDescription = new Label();

        private HyperLink lnkProductName = new HyperLink();

        private Label lblSKU = new Label();
        private Label lblGiftRegistry = new Label();
        private Label lblSize = new Label();
        private Label lblColor = new Label();
        private Literal ltTextOption = new Literal();
        private TextBox txtItemNotes = new TextBox();
        private Label lblShipping = new Label();
        private Label lblRecurring = new Label();
        private Label lblPromotionHeading = new Label();

        //for kit
        private HyperLink lnkEditKit = new HyperLink();
        private Label lblKitName;
        private Label lblKitTextOption;

        #endregion

        /// <summary>
        /// Creates the child controls in a control derived from System.Web.UI.WebControls.CompositeControl.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();

            IntializeControlsDefaultValues();

            /********************************/
            /******** CREATE CONTROLS *******/
            /********************************/

            //ADD LITERALS

            CartItem cItem = null;

            if (this.DataSource != null)
            {
                cItem = DataSource;
                AssignDataSourceContentToControls(cItem);
            }

            //product image
            if (LineItemDescriptionSettings.ShowPicsInCart)
            {
                if (this.IsMiniCart)
                {
                    Controls.Add(new LiteralControl("    <div class='mini-cart-product-image'>"));
                }
                else
                {
                    Controls.Add(new LiteralControl("    <div class='row'><div class='col-md-5'><div class='primary-img-box'>"));
                }
                imgProductPic.Attributes["class"] = "img-responsive";
                Controls.Add(imgProductPic);
                Controls.Add(new LiteralControl("    </div></div>"));
            }

            //line item description quantity and subtotals

            Controls.Add(new LiteralControl("     <div class='cart-title col-md-5'> <span class='normal-heading black-color'>"));
            Controls.Add(lnkProductName);
            Controls.Add(new LiteralControl("     </span>"));
         //   Controls.Add(new LiteralControl("     </div>"));

            if (!this.IsMiniCart)
            {
                //SKU
                if (!cItem.IsSystem)
                {
                    Controls.Add(new LiteralControl("     <span>"));
                    Controls.Add(lblSKUCaption);
                    Controls.Add(lblSKU);
                    Controls.Add(new LiteralControl("      </span>"));
                    Controls.Add(new LiteralControl("      </span>"));                    
                }

                //GiftRegistry
                if (cItem.GiftRegistryForCustomerID != 0)
                {
                    Controls.Add(new LiteralControl("      <div class='cart-small cart-gift-registry'>"));
                    Controls.Add(lblGiftRegistry);
                    Controls.Add(new LiteralControl("      </div>"));
                }

                //Size
				if(cItem.ChosenSize.Length != 0)
                {
                    //Controls.Add(new LiteralControl("      <div class='cart-small cart-size'>"));
                    //Controls.Add(lblSizeCaption);
                    //Controls.Add(new LiteralControl(": "));
                    //Controls.Add(lblSize);
                    //Controls.Add(new LiteralControl("      </div>"));
                }

                //Color
                if (cItem.ChosenColor.Length != 0)
                {
                    //Controls.Add(new LiteralControl("      <div class='cart-small cart-color'>"));
                    //Controls.Add(lblColorCaption);
                    //Controls.Add(new LiteralControl(": "));
                    //Controls.Add(lblColor);
                    //Controls.Add(new LiteralControl("      </div>"));
                }

                //Text Option
                if (cItem.TextOption.Length != 0)
                {
                    Controls.Add(new LiteralControl("      <div class='cart-small cart-text-option'>"));
                    Controls.Add(lblTextOptionCaption);
					Controls.Add(new LiteralControl(": "));
                    Controls.Add(ltTextOption);
                    Controls.Add(new LiteralControl("      </div>"));
                }

                //Promotion
                var discountResult = cItem.ThisShoppingCart.DiscountResults.Where(dr => dr.DiscountedItems.Any(di => di.ShoppingCartRecordId == cItem.ShoppingCartRecordID)).FirstOrDefault();
                if (discountResult != null)
                {
                    Controls.Add(new LiteralControl("    <div class='cart-small cart-promo'>"));
                    Controls.Add(lblPromotionHeading);
                    Controls.Add(new LiteralControl("    </div>"));
                }

                //Recurring
                if (cItem.IsRecurring && cItem.RecurringDisplay.Length != 0 && !AppLogic.AppConfigBool("AllowRecurringFrequencyChangeInCart"))
                {
                    Controls.Add(new LiteralControl("    <div class='cart-small cart-recurring'>"));
                    Controls.Add(lblRecurring);
                    Controls.Add(new LiteralControl("    </div>"));
                }

                //Kit Details
                if (cItem.IsAKit)
                {
                    Controls.Add(new LiteralControl("    <div class='cart-small cart-kit'>"));
                    if (LineItemDescriptionSettings.ShowEditButtonInCartForKitProducts)
                    {
                        lnkEditKit.NavigateUrl = cItem.KitEditURL;
                        Controls.Add(new LiteralControl("      <div class='cart-kit-edit'>"));
                        Controls.Add(lnkEditKit);
                        Controls.Add(new LiteralControl("      </div>"));
                    }

                    foreach (KitCartItem kci in cItem.KitComposition.Compositions)
                    {
                        lblKitName = new Label();
						lblKitName.Text = string.Format("- {0}", kci.Name);
						lblKitName.CssClass = "kit-item-name";

                        //kit name
                        Controls.Add(new LiteralControl("      <div class='cart-kit-textoption'>"));
                        Controls.Add(lblKitName);

                        //kit text option
                        if (kci.TextOption.Length > 0)
                        {
                            bool isImage = kci.ContentIsImage;

                            if (isImage)
                            {
                                Controls.Add(new LiteralControl(": <a class='cart-kit-imagelink' href=" + kci.TextOption + " target=\"_blank\">" + LineItemDescriptionSettings.ImageLabelText + "</a>"));
                            }
                            else
                            {
                                lblKitTextOption = new Label();
                                lblKitTextOption.Text = string.Format(": {0}", kci.TextOption);
                            }

                            Controls.Add(new LiteralControl("      <span class='kit-text-option-value'>"));
                            if (!isImage)
                            {
								Controls.Add(lblKitTextOption);
                            }
                            
                            Controls.Add(new LiteralControl("      </span>"));
                        }
						Controls.Add(new LiteralControl("      </div>"));
                    }

                    Controls.Add(new LiteralControl("    </div>"));
                }

                //Controls.Add(new LiteralControl("    <div class='cart-shipping'>"));
                //Controls.Add(lblShipping);
                //Controls.Add(new LiteralControl("    </div>"));

                if (cItem.ProductDescription.Length > 180)
                {
                    cItem.ProductDescription = cItem.ProductDescription.Substring(0, 180) + "...";
                }
                lblProductDescription.Text = cItem.ProductDescription;                
                Controls.Add(new LiteralControl("     <p style='word-wrap:break-word'"));
                Controls.Add(lblProductDescription);
                Controls.Add(new LiteralControl("     </p>"));
                Controls.Add(new LiteralControl("     </div></div>"));

                //Item Notes              
                if (LineItemDescriptionSettings.AllowShoppingCartItemNotes && !cItem.IsSystem
                        && cItem.CartType == CartTypeEnum.ShoppingCart && !DisAllowItemNotes)
                {
                    Controls.Add(new LiteralControl("      <div class='form-group cart-small cart-item-notes'>"));
                    Controls.Add(new LiteralControl("       <label>"));
                    Controls.Add(lblItemNotesCaption);
                    Controls.Add(new LiteralControl("       </label>"));
                    Controls.Add(txtItemNotes);
                    Controls.Add(new LiteralControl("      </div>"));
                }
            }
        }

        #region Helper Methods
        /// <summary>
        /// Initialize default values for the controls
        /// </summary>
        private void IntializeControlsDefaultValues()
        {
            //Product Name
            lnkProductName.CssClass = "black-color underline-no";
            

            //Product image
            imgProductPic.ID = "imgProductPic";

            if (this.IsMiniCart)
            {
                imgProductPic.Height = AppLogic.AppConfigNativeInt("MiniCartMaxIconHeight");
                imgProductPic.Width = AppLogic.AppConfigNativeInt("MiniCartMaxIconWidth");
            }

            //product name
            lnkProductName.ID = "lnkProductName";

            //SKU
            lblSKUCaption.ID = "lblSKUCaption";
            lblSKUCaption.Text = "showproduct.aspx.21";
            lblSKUCaption.CssClass = "inline-text";
            lblSKU.ID = "lblSKU";
            lblSKU.CssClass = "inline-text";
        
            //gift registry
            lblGiftRegistryCaption.ID = "lblGiftRegistryCaption";
            lblGiftRegistryCaption.Text = "shoppingcart.cs.92";
            lblGiftRegistryCaption.CssClass = "cart-label";
            lblGiftRegistry.ID = "lblGiftRegistry";
          
            //size
            lblSizeCaption.ID = "lblSizeCaption";
            lblSizeCaption.CssClass = "cart-label";
            lblSize.ID = "lblSize";
           
            //color
            lblColorCaption.ID = "lblColorCaption";
            lblColorCaption.CssClass = "cart-label";
            lblColor.ID = "lblColor";
            
            //text option
            lblTextOptionCaption.ID = "lblTextOptionCaption";
            lblTextOptionCaption.CssClass = "cart-label";
            ltTextOption.ID = "ltTextOption";
           
            //recurring 
            lblRecurring.ID = "lblRecurring";
            
            //kit details
            lnkEditKit.ID = "lnkEditKit";
            lnkEditKit.Text = "Edit";

            lblShipping.ID = "lblShipping";

            //item notes
            lblItemNotesCaption.ID = "lblItemNotesCaption";
            lblItemNotesCaption.CssClass = "cart-label";
            txtItemNotes.ID = "txtItemNotes";
            txtItemNotes.TextMode = TextBoxMode.MultiLine;
            txtItemNotes.CssClass = "form-control item-notes";

            //promotion
            lblPromotionHeading.ID = "lblPromotionHeading";
            lblPromotionHeading.CssClass = "cart-label";

        }

        /// <summary>
        /// Assign the Datasource items into the controls.
        /// </summary>
        /// <param name="cItem"></param>
        private void AssignDataSourceContentToControls(CartItem cItem)
        {
            //Product Image
            if (LineItemDescriptionSettings.ShowPicsInCart)
            {
                imgProductPic.ImageUrl = cItem.ProductPicURL;
                if (cItem.SEAltText != null)
                    imgProductPic.AlternateText = XmlCommon.GetLocaleEntry(cItem.SEAltText, Customer.Current.LocaleSetting, false);
            }

            //Product Name
            lnkProductName.Text = cItem.ProperProductName;
            lnkProductName.Attributes["class"] = "black-color";
            if (LineItemDescriptionSettings != null)
            {
                if (LineItemDescriptionSettings.LinkToProductPageInCart && !cItem.IsSystem && !cItem.IsAuctionItem)
                {
                    lnkProductName.NavigateUrl = SE.MakeProductLink(cItem.ProductID, "");
                }

                if (!this.IsMiniCart)
                {

                    //SKU
                    if (!cItem.IsSystem)
                    {
                        if (LineItemDescriptionSettings.SKUCaption != null)
                        {
                            lblSKUCaption.Text = LineItemDescriptionSettings.SKUCaption;
                        }
                        lblSKU.Text = cItem.SKU;
                    }

                    //GiftRegistry 
                    if (cItem.GiftRegistryForCustomerID != 0)
                    {
                        if (LineItemDescriptionSettings.GiftRegistryCaption != null)
                        {
                            lblGiftRegistryCaption.Text = LineItemDescriptionSettings.GiftRegistryCaption;
                        }
                        lblGiftRegistry.Text = string.Format(lblGiftRegistryCaption.Text, cItem.GiftRegistryDisplayName);
                    }

                    //Size
                    if (cItem.ChosenSize.Length != 0)
                    {
                        lblSizeCaption.Text = cItem.SizeOptionPrompt;
                        lblSize.Text = cItem.ChosenSizeDisplayFormat;
                    }

                    //Color
                    if (cItem.ChosenColor.Length != 0)
                    {
                        lblColorCaption.Text = cItem.ColorOptionPrompt;
                        lblColor.Text = cItem.ChosenColorDisplayFormat;
                    }

                    //Shipping display
                    cItem.ShowMultiShipAddressUnderItemDescription = LineItemDescriptionSettings.ShowMultiShipAddressUnderItemDescription;
                    lblShipping.Text = cItem.LineItemShippingDisplay;


                    //Text Option
                    if (cItem.TextOption.Length != 0)
                    {
                        lblTextOptionCaption.Text = cItem.TextOptionPrompt;
                        ltTextOption.Text = cItem.TextOptionDisplayFormat;
                    }

                    //Recurring
                    if (cItem.IsRecurring && cItem.RecurringDisplay.Length != 0)
                    {
                        lblRecurring.Text = cItem.RecurringDisplay;
                    }

                    //Item Notes 
                    if (LineItemDescriptionSettings.AllowShoppingCartItemNotes &&
                        !cItem.IsSystem &&
                        cItem.ThisShoppingCart.CartType == CartTypeEnum.ShoppingCart &&
                        !DisAllowItemNotes)
                    {
                        txtItemNotes.ReadOnly = !AllowEdit;
                        lblItemNotesCaption.Text = LineItemDescriptionSettings.ItemNotesCaption;
                        txtItemNotes.Columns = LineItemDescriptionSettings.ItemNotesColumns;
                        txtItemNotes.Rows = LineItemDescriptionSettings.ItemNotesRows;
                        txtItemNotes.Text = cItem.Notes;
                    }

                    var discountResult = cItem.ThisShoppingCart.DiscountResults.Where(dr => dr.DiscountedItems.Any(di => di.ShoppingCartRecordId == cItem.ShoppingCartRecordID)).FirstOrDefault();
                    if (discountResult != null)
                        lblPromotionHeading.Text = discountResult.Promotion.UsageText;
                }
            }
        }
        #endregion

        #region Controls Properties

        private CartItem m_datasource;
        /// <summary>
        /// Gets or sets the datasource
        /// </summary>
        [Browsable(false)]
        public CartItem DataSource
        {
            get { return m_datasource; }
            set { m_datasource = value; }
        }

        private bool m_isminicart;

        /// <summary>
        /// Gets or set the value indicating whether the control is used as a minicart.
        /// </summary>
        public bool IsMiniCart
        {
            get { return m_isminicart; }
            set { m_isminicart = value; }
        }

        private string m_imagelabeltext;

        /// <summary>
        /// Gets or set the stringresource for Image link text
        /// </summary>
        [Browsable(true), PersistenceMode(PersistenceMode.Attribute)]
        public string ImageLabelText
        {
            get { return m_imagelabeltext; }
            set { m_imagelabeltext = value; }
        }

        private bool m_allowedit;

        /// <summary>
        /// Gets or sets the value indicating whether the control will allow editing
        /// </summary>
        [Browsable(false)]
        public bool AllowEdit
        {
            get { return m_allowedit; }
            set { m_allowedit = value; }
        }

        private LineItemSettings m_lineitemdescriptionsettings;

        /// <summary>
        /// Gets or set the line item description settings
        /// </summary>
        [Browsable(false)]
        public LineItemSettings LineItemDescriptionSettings
        {
            get { return m_lineitemdescriptionsettings; }
            set { m_lineitemdescriptionsettings = value; }
        }

        private bool m_disallowitemnotes;

        /// <summary>
        /// Gets or set the value indicating whether the Item notes is disallowed
        /// </summary>
        [Browsable(false)]
        public bool DisAllowItemNotes
        {
            get { return m_disallowitemnotes; }
            set { m_disallowitemnotes = value; }
        }

        /// <summary>
        /// Gets the value of the textbox Item notes in the controls collection
        /// </summary>
        [Browsable(false)]
        public string Notes
        {
            get { return txtItemNotes.Text; }
        }

        #endregion
    }
}
