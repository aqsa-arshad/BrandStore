// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Net;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.UI.Design;
using System.Web.UI.Design.WebControls;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel.Design;
using AspDotNetStorefrontCore;
using System.Drawing.Design;

namespace AspDotNetStorefrontControls
{
    public enum CartDisplayMode
    {
        ShoppingCart = 0,
        WishList = 1,
        ReadOnly = 2,
        GiftRegistry = 3,
        MiniCart = 4
    }

    /// <summary>
    /// Provides a control that display the Shopping cart line items with quantity and price.
    /// </summary>
    [ToolboxData("<{0}:ShoppingCartControl runat=server></{0}:ShoppingCartControl>"),
    Designer(typeof(ShoppingControlDesigner))]
    public class ShoppingCartControl : CompositeControl
    {

        #region constant variables
        private const string SETTINGS_CATEGORY = "ASPDNSF Settings";
        private const string STATIC_HEADER_SEPARATOR_IMAGEURL = "StaticHeaderSeparatorImageUrl";
        private const string EMPTY_CART_TOPIC_NAME = "EmptyCartTopicName";
        private const string DEFAULT_EMPTY_CART_TOPIC = "EmptyCartText";
        #endregion

        #region Web Controls Instantiation
        private Image imgShoppingCartTab = new Image();
        private Label lblProductHeader = new Label();
        private Label lblQuantityHeader = new Label();
        private Label lblSubtotalHeader = new Label();
        Button btnMiniCartUpdate = new Button();
        #endregion

        #region Private Properties
        private ShoppingCartItemCollection _items = new ShoppingCartItemCollection();
        #endregion

        ShoppingCartControlItem item = null;

        /// <summary>
        /// Gets or sets the querystring name for the search keyword
        /// </summary>
        [Browsable(true), Category("Appearance"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string StaticHeaderSeparatorImageUrl
        {
            get { return null == ViewState[STATIC_HEADER_SEPARATOR_IMAGEURL] ? string.Empty : (string)ViewState[STATIC_HEADER_SEPARATOR_IMAGEURL]; }
            set
            {
                try
                {
                    ViewState[STATIC_HEADER_SEPARATOR_IMAGEURL] = AppLogic.LocateImageURL(value);
                }
                catch (Exception)
                {
                    ViewState[STATIC_HEADER_SEPARATOR_IMAGEURL] = value;
                }
            }
        }

        [DefaultValue(DEFAULT_EMPTY_CART_TOPIC), Category("Appearance")]
        public string EmptyCartTopicName
        {
            get { return null == ViewState[EMPTY_CART_TOPIC_NAME] ? DEFAULT_EMPTY_CART_TOPIC : (string)ViewState[EMPTY_CART_TOPIC_NAME]; }
            set
            {
                ViewState[EMPTY_CART_TOPIC_NAME] = value;
            }
        }

        #region Overridden Methods
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

            //*********************//
            //*** USE TEMPLATES ***//
            //*********************//

            //if the page developer does not explicitly add a <HeaderTemplate> tag 
            //in ShoppingCartControl's declarative syntax     
            if (HeaderTemplate == null)
            {
                if (this.DataSource != null)
                {
                    CartItemCollection cic = this.DataSource;
                    if (cic.Count != 0)
                    {
                        if (this.DisplayMode == CartDisplayMode.MiniCart)
                        {
                            bool hasHeaderImage = !string.IsNullOrEmpty(StaticHeaderSeparatorImageUrl);

                            Controls.Add(new LiteralControl("        <div class='mini-cart-row mini-cart-header'>"));

                            Controls.Add(new LiteralControl("          <div class='mini-cart-column mini-cart-quantity'>"));
                            Controls.Add(lblQuantityHeader);
                            Controls.Add(new LiteralControl("          </div>"));

                            if (hasHeaderImage)
                            {
                                Image seperator = new Image();
                                seperator.ImageUrl = this.StaticHeaderSeparatorImageUrl;
                                TableCell cell = new TableCell();
                                cell.Controls.Add(seperator);
                                Controls.Add(cell);
                            }

                            Controls.Add(new LiteralControl("          <div class='mini-cart-column mini-cart-description'>"));
                            Controls.Add(lblProductHeader);
                            Controls.Add(new LiteralControl("          </div>"));

                            if (hasHeaderImage)
                            {
                                Image seperator2 = new Image();
                                seperator2.ImageUrl = this.StaticHeaderSeparatorImageUrl;
                                TableCell cell2 = new TableCell();
                                cell2.Controls.Add(seperator2);
                                Controls.Add(cell2);
                            }

                            Controls.Add(new LiteralControl("          <div class='mini-cart-column mini-cart-subtotal'>"));
                            Controls.Add(lblSubtotalHeader);
                            Controls.Add(new LiteralControl("          </div>"));

                            Controls.Add(new LiteralControl("        </div>"));
                        }
                        else
                        {
                            ITemplate defaultTemplate;

                            if (this.DisplayMode != CartDisplayMode.GiftRegistry && this.DisplayMode != CartDisplayMode.WishList)
                            {
                                defaultTemplate = new DefaultHeaderTemplate(ProductHeaderText, QuantityHeaderText, SubTotalHeaderText, DisplayMode);
                            }
                            else
                            {
                                defaultTemplate = new DefaultHeaderTemplate(ProductHeaderText, QuantityHeaderText, String.Empty, DisplayMode);
                            }

                            defaultTemplate.InstantiateIn(this);
                        }
                    }
                    else
                    {
                        Topic emptyCartTopic = new Topic(this.EmptyCartTopicName, Customer.Current.LocaleSetting);
                        ITemplate defaultTemplate = new DefaultHeaderTemplate(emptyCartTopic.Contents);
                        defaultTemplate.InstantiateIn(this);
                    }
                }
                else
                {
                    if (this.DisplayMode == CartDisplayMode.MiniCart)
                    {
                        Controls.Add(new LiteralControl("        <div class='mini-cart-row mini-cart-header'>"));

                        Controls.Add(new LiteralControl("          <div class='mini-cart-column mini-cart-quantity'>"));
                        Controls.Add(lblQuantityHeader);
                        Controls.Add(new LiteralControl("          </div>"));

                        Controls.Add(new LiteralControl("          <div class='mini-cart-column mini-cart-description'>"));
                        Controls.Add(lblProductHeader);
                        Controls.Add(new LiteralControl("          </div>"));

                        Controls.Add(new LiteralControl("          <div class='mini-cart-column mini-cart-subtotal'>"));
                        Controls.Add(lblSubtotalHeader);
                        Controls.Add(new LiteralControl("          </div>"));
                        Controls.Add(new LiteralControl("        </div>"));

                    }
                    else
                    {
                        ITemplate defaultTemplate = new DefaultHeaderTemplate("Product", "Quantity", "SubTotal", DisplayMode);
                        defaultTemplate.InstantiateIn(this);
                    }
                }

            }
            else
            {
                if (this.DataSource != null)
                {
                    CartItemCollection cic = (CartItemCollection)this.DataSource;
                    if (cic.Count != 0)
                    {
                        CustomTemplate headerTemplateItem = new CustomTemplate();
                        headerTemplateItem.ID = "headerTemplateItem";
                        HeaderTemplate.InstantiateIn(headerTemplateItem);

                        Controls.Add(new LiteralControl("        <div class='row-template-item'>"));
                        Controls.Add(headerTemplateItem);
                        Controls.Add(new LiteralControl("        </div>"));
                    }
                    else
                    {
                        Topic emptyCartTopic = new Topic(this.EmptyCartTopicName, Customer.Current.LocaleSetting);
                        ITemplate defaultTemplate = new DefaultHeaderTemplate(emptyCartTopic.Contents);
                        defaultTemplate.InstantiateIn(this);
                    }
                }
                else
                {
                    CustomTemplate headerTemplateItem = new CustomTemplate();
                    headerTemplateItem.ID = "headerTemplateItem";
                    HeaderTemplate.InstantiateIn(headerTemplateItem);

                    Controls.Add(new LiteralControl("        <div class='cart-row row-template-item'>"));
                    Controls.Add(headerTemplateItem);
                    Controls.Add(new LiteralControl("        </div>"));
                }
            }

            ShoppingCart cart = null;
            //ITEMS Template
            if (DataSource != null)
            {
                Items.Clear();

                CartItemCollection sortedCartItemForMinicart = new CartItemCollection();
                if (this.DisplayMode == CartDisplayMode.MiniCart)
                {
                    int counter = 0;
                    //first add non deleted cartType 
                    foreach (CartItem ci in this.DataSource.Where(n => (int)n.CartType != 101).OrderByDescending(o => o.CreatedOn))
                    {
                        if (this.MaxMinicartLatestItems > counter)
                        {
                            sortedCartItemForMinicart.Add(ci);
                        }
                        else
                        {
                            break;
                        }
                        counter++;
                    }

                    //then add the deleted type
                    foreach (CartItem ci in this.DataSource.Where(n => (int)n.CartType == 101).OrderByDescending(o => o.CreatedOn))
                    {
                        if (this.MaxMinicartLatestItems > counter)
                        {
                            sortedCartItemForMinicart.Add(ci);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                else
                {
                    sortedCartItemForMinicart = this.DataSource;
                }

                Panel pnlItems = new Panel();
                pnlItems.ID = "pnlCartItems";
                pnlItems.CssClass = "cart_items";

                foreach (CartItem cItem in sortedCartItemForMinicart)
                {
                    cart = cItem.ThisShoppingCart;

                    if (InventoryTrimmed)
                    {
                        pnlItems.Controls.Add(new LiteralControl("<span class=\"error-large\">" + "shoppingcart.aspx.3".StringResource() + "</span>"));
                    }

                    item = new ShoppingCartControlItem(this.LineItemSettings, this.MinicartSummarySetting);

                    item.CartItem = cItem;
                    item.AllowEdit = this.AllowEdit;
                    item.DisplayMode = this.DisplayMode;
                    if (cItem.ThisShoppingCart.CartType == CartTypeEnum.Deleted)
                    {
                        item.IsDeleted = true;
                    }
                    Items.Add(item);
                    pnlItems.Controls.Add(item);

                    if (this.DisplayMode == CartDisplayMode.MiniCart &&
                        cItem.MinimumQuantityUdpated)
                    {
                        pnlItems.Controls.Add(new LiteralControl("<div class=\"mini-cart-min-quantity\">"));
                        pnlItems.Controls.Add(new LiteralControl(this.LineItemSettings.MinimumQuantityNotificationText));
                        pnlItems.Controls.Add(new LiteralControl("</div>"));
                    }

                    //   pnlItems.Controls.Add(new LiteralControl("<div class='cart-row row-separator'></div>"));

                }

                Controls.Add(pnlItems);
            }


            if (this.DisplayMode == CartDisplayMode.MiniCart)
            {
                CartSummary crtsummaryMiniCart = new CartSummary();
                if (MinicartSummarySetting != null)
                {
                    crtsummaryMiniCart.CalculateShippingDuringCheckout = this.MinicartSummarySetting.CalculateShippingDuringCheckout; //false;
                    crtsummaryMiniCart.CalculateTaxDuringCheckout = this.MinicartSummarySetting.CalculateTaxDuringCheckout;//false;
                    crtsummaryMiniCart.ShowTotal = this.MinicartSummarySetting.ShowTotal;//true;
                    crtsummaryMiniCart.ShowShipping = false; //this.MinicartSummarySetting.ShowShipping;// false;
                    crtsummaryMiniCart.SkipShippingOnCheckout = this.MinicartSummarySetting.SkipShippingOnCheckout;//true;
                    crtsummaryMiniCart.CalculateShippingDuringCheckoutText = this.MinicartSummarySetting.CalculateShippingDuringCheckoutText;//AppLogic.GetString("shoppingcart.aspx.13", Customer.Current.SkinID, Customer.Current.LocaleSetting);
                    crtsummaryMiniCart.CalculateTaxDuringCheckoutText = this.MinicartSummarySetting.CalculateTaxDuringCheckoutText;//AppLogic.GetString("shoppingcart.aspx.15", Customer.Current.SkinID, Customer.Current.LocaleSetting);
                    crtsummaryMiniCart.LineItemDiscountCaption = this.MinicartSummarySetting.LineItemDiscountCaption;
                    crtsummaryMiniCart.SubTotalCaption = this.MinicartSummarySetting.SubTotalCaption; //AppLogic.GetString("shoppingcart.cs.96", Customer.Current.SkinID, Customer.Current.LocaleSetting);
                    crtsummaryMiniCart.SubTotalWithDiscountCaption = this.MinicartSummarySetting.SubTotalWithDiscountCaption; //AppLogic.GetString("shoppingcart.cs.97", Customer.Current.SkinID, Customer.Current.LocaleSetting);
                    crtsummaryMiniCart.ShippingCaption = this.MinicartSummarySetting.ShippingCaption; //AppLogic.GetString("shoppingcart.aspx.12", Customer.Current.SkinID, Customer.Current.LocaleSetting);                 
                    crtsummaryMiniCart.TaxCaption = this.MinicartSummarySetting.TaxCaption; //AppLogic.GetString("shoppingcart.aspx.14", Customer.Current.SkinID, Customer.Current.LocaleSetting);
                    crtsummaryMiniCart.ShippingVatExCaption = this.MinicartSummarySetting.ShippingVatExCaption; //AppLogic.GetString("setvatsetting.aspx.7", Customer.Current.SkinID, Customer.Current.LocaleSetting);
                    crtsummaryMiniCart.OrderDiscountCaption = this.MinicartSummarySetting.OrderDiscountCaption;
                    crtsummaryMiniCart.TotalCaption = this.MinicartSummarySetting.TotalCaption; //AppLogic.GetString("shoppingcart.cs.61", Customer.Current.SkinID, Customer.Current.LocaleSetting);
                    crtsummaryMiniCart.UseInAjaxMiniCart = this.MinicartSummarySetting.UseInAjaxMiniCart; //true;
                    crtsummaryMiniCart.ShowGiftCardTotal = this.MinicartSummarySetting.ShowGiftCardTotal;
                    crtsummaryMiniCart.GiftCardTotalCaption = this.MinicartSummarySetting.GiftCardTotalCaption;
                }
                crtsummaryMiniCart.DataSource = cart;

                btnMiniCartUpdate.Text = MinicartSummarySetting.MiniCartUpdateText;//LineItemSettings.MiniCartUpdateText;
                btnMiniCartUpdate.CommandName = "Update";
                btnMiniCartUpdate.ValidationGroup = "val";


                if (DataSource.Count != 0)
                {
                    Controls.Add(new LiteralControl("        <div class='cart-row mini-cart-summary'>"));
                    Controls.Add(crtsummaryMiniCart);
                    Controls.Add(new LiteralControl("        </div>"));
                }

                Controls.Add(new LiteralControl("        <div class='cart-row mini-cart-summary'>"));

                if (DataSource.Count == 0)
                {
                    Controls.Add(new LiteralControl("          <div class='opacity-zero'>"));
                }
                else
                {
                    Controls.Add(new LiteralControl("          <div class='mini-cart-wrap'>"));
                }

                if (DataSource.Count > 0)
                {
                    Controls.Add(btnMiniCartUpdate);
                }

                Controls.Add(new LiteralControl("          </div>"));

                if (DataSource.Count > 0)
                {
                    Controls.Add(new LiteralControl("          <div class='mini-cart-wrap'>"));
                    Controls.Add(new LiteralControl(string.Format("<a href='{1}'>{0}</a>", MinicartSummarySetting.MiniCartCheckoutText, ResolveUrl("~/shoppingcart.aspx"))));
                    Controls.Add(new LiteralControl("          </div>"));
                }
                Controls.Add(new LiteralControl("        </div>"));
            }
            else
            {

                if (FooterTemplate == null)
                {
                    ITemplate defaultTemplate = new DefaulFooterTemplate();
                    defaultTemplate.InstantiateIn(this);
                }
                else
                {
                    CustomTemplate footerTemplateItem = new CustomTemplate();
                    footerTemplateItem.ID = "footerTemplateItem";
                    FooterTemplate.InstantiateIn(footerTemplateItem);

                    Controls.Add(new LiteralControl("<div>"));
                    Controls.Add(footerTemplateItem);
                    Controls.Add(new LiteralControl("</div>"));
                }
            }
            //end outer box
        }

        /// <summary>
        /// Overloaded. Overridden. Binds a data source to the CompositeControl and all its child controls. 
        /// </summary>
        public override void DataBind()
        {
            Controls.Clear();
            CreateChildControls();
            ChildControlsCreated = true;

            base.DataBind();
        }

        /// <summary>
        /// Gets the HtmlTextWriterTag value that corresponds to this Web server control. This property is used primarily by control developers.(Inherited from WebControl.)
        /// </summary>
        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Div;
            }
        }

        /// <summary>
        /// Determines whether the event for the server control is passed up the page's UI server control hierarchy. (Inherited from Control.)
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="args">An EventArgs object that contains the event data</param>
        /// <returns>true if the event has been canceled; otherwise, false. The default is false. </returns>
        protected override bool OnBubbleEvent(object source, EventArgs args)
        {
            if (args is CommandEventArgs)
            {
                CommandEventArgs cArgs = args as CommandEventArgs;

                if (cArgs.CommandName.EqualsIgnoreCase("Delete") ||
                    cArgs.CommandName.EqualsIgnoreCase("Remove"))
                {
                    int id = Convert.ToInt32(cArgs.CommandArgument);
                    ItemEventArgs e = new ItemEventArgs() { ID = id };
                    OnItemDeleting(e);
                }
                else if (cArgs.CommandName.EqualsIgnoreCase("Update"))
                {
                    ItemEventArgs e = new ItemEventArgs { Qty = item.Quantity };
                    OnMiniCartItemUpdate(e);
                }
                else if (cArgs.CommandName.EqualsIgnoreCase("MoveToShoppingCart"))
                {
                    int id = Convert.ToInt32(cArgs.CommandArgument);
                    ItemEventArgs e = new ItemEventArgs() { ID = id };
                    OnMoveToShoppingCartInvoked(e);
                }
            }
            return false;
        }
        #endregion

        #region Event Handler

        public event EventHandler<ItemEventArgs> MoveToShoppingCartInvoked;

        protected virtual void OnMoveToShoppingCartInvoked(ItemEventArgs e)
        {
            if (MoveToShoppingCartInvoked != null)
            {
                MoveToShoppingCartInvoked(this, e);
            }
        }

        public event EventHandler<ItemEventArgs> ItemDeleting;

        /// <summary>
        /// The Event for Item Deleting
        /// </summary>
        /// <param name="e">The item event argument</param>
        protected virtual void OnItemDeleting(ItemEventArgs e)
        {
            if (ItemDeleting != null)
            {
                ItemDeleting(this, e);
            }
        }
        public event EventHandler<EventArgs> MiniCartItemUpdate;
        /// <summary>
        /// The Event for MiniCart Item Update
        /// </summary>
        /// <param name="e">The item event argument</param>
        protected virtual void OnMiniCartItemUpdate(EventArgs e)
        {
            if (MiniCartItemUpdate != null)
            {
                MiniCartItemUpdate(this, e);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Initialize default values for the controls
        /// </summary>
        private void IntializeControlsDefaultValues()
        {
            //Product Line Item Description Headers
            lblProductHeader.ID = "lblProductHeader";
            lblProductHeader.Text = ProductHeaderText;

            //Quantity Header
            lblQuantityHeader.ID = "lblQuantityHeader";
            lblQuantityHeader.Text = QuantityHeaderText;

            //Subtotal Header
            lblSubtotalHeader.ID = "lblSubtotalHeader";
            lblSubtotalHeader.Text = SubTotalHeaderText;

            btnMiniCartUpdate.ID = "btnMiniCartUpdate";

        }
        #endregion

        #region Controls Properties

        public Boolean InventoryTrimmed { get; set; }

        /// <summary>
        /// gets or sets the restricted quantities
        /// </summary>
        public string RestrictedQuantities
        {
            get
            {
                return item.RestrictedQuantities;
            }
            set
            {
                item.RestrictedQuantities = value;
            }
        }

        private int m_maxminicartlatestitems;

        /// <summary>
        /// Gets or sets the minicart maximum latest cart items count to be displayed
        /// </summary>
        [Browsable(true)]
        public int MaxMinicartLatestItems
        {
            get { return m_maxminicartlatestitems; }
            set { m_maxminicartlatestitems = value; }
        }

        private CartItemCollection m_datasource;

        /// <summary>
        /// Gets or sets the Datasource of the control
        /// </summary>
        [Browsable(false)]
        public CartItemCollection DataSource
        {
            get { return m_datasource; }
            set { m_datasource = value; }
        }


        /// <summary>
        /// Gets or sets the Line Item Description Header
        /// </summary>
        [Browsable(true),
        Bindable(true),
        Category(SETTINGS_CATEGORY),
        Description("Product Header Text"),
        DefaultValue("Product")]
        public String ProductHeaderText
        {
            get
            {
                if ((object)ViewState["ProductHeaderText"] == null)
                {
                    return string.Empty;
                }
                return ViewState["ProductHeaderText"].ToString();
            }
            set
            {
                ViewState["ProductHeaderText"] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// Gets or sets the Line Item Quantity Header
        /// </summary>
        [Browsable(true),
        Bindable(true),
        Category(SETTINGS_CATEGORY),
        Description("Quantity Header Text"),
        DefaultValue("Quantity")]
        public String QuantityHeaderText
        {
            get
            {
                if ((object)ViewState["QuantityHeaderText"] == null)
                {
                    return string.Empty;
                }
                return ViewState["QuantityHeaderText"].ToString();
            }
            set
            {
                ViewState["QuantityHeaderText"] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// Gets or sets the Line Item Extended Price Header
        /// </summary>
        [Browsable(true),
        Bindable(true),
        Category(SETTINGS_CATEGORY),
        Description("SubTotal Header Text"),
        DefaultValue("SubTotal")]
        public String SubTotalHeaderText
        {
            get
            {
                if ((object)ViewState["SubTotalHeaderText"] == null)
                {
                    return string.Empty;
                }
                return ViewState["SubTotalHeaderText"].ToString();
            }
            set
            {
                ViewState["SubTotalHeaderText"] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// Displays the quantity textbox and delete button of line item if set to true, otherwise, the quantity will be displayed as a label and the delete button will be hidden.
        /// </summary>
        [Browsable(true),
        Bindable(true),
        Category(SETTINGS_CATEGORY),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public bool AllowEdit
        {
            get
            {
                object booleanValue = ViewState["AllowEdit"];
                if (null == booleanValue) { return true; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState["AllowEdit"] = value;
                ChildControlsCreated = false;
            }
        }

        [DefaultValue(0)]
        public CartDisplayMode DisplayMode
        {
            get
            {
                object savedValue = ViewState["DisplayMode"];
                if (null == savedValue) { return CartDisplayMode.ShoppingCart; }

                return (CartDisplayMode)savedValue;
            }
            set
            {
                ViewState["DisplayMode"] = value;
                ChildControlsCreated = false;
            }
        }

        private bool RenderAsMinicart()
        {
            return this.DisplayMode == CartDisplayMode.MiniCart;
        }

        private bool RenderAsWishList()
        {
            return this.DisplayMode == CartDisplayMode.WishList;
        }

        private bool RenderAsGiftRegistry()
        {
            return this.DisplayMode == CartDisplayMode.GiftRegistry;
        }

        private bool RenderReadOnly()
        {
            return this.DisplayMode == CartDisplayMode.ReadOnly;
        }

        private ITemplate m_headertabtemplate;

        /// <summary>
        /// Gets or set the Header Tab Template of the control
        /// </summary>
        [Browsable(false),
        Description("The ShoppingCart Control's Header Tab Template."),
        TemplateContainer(typeof(ShoppingCartControlItem)),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public ITemplate HeaderTabTemplate
        {
            get { return m_headertabtemplate; }
            set { m_headertabtemplate = value; }
        }

        private ITemplate m_headertemplate;

        /// <summary>
        /// Gets or set the Header Template of the control
        /// </summary>
        [Browsable(false),
        Description("The ShoppingCart Control's Header Template."),
        TemplateContainer(typeof(ShoppingCartControlItem)),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public ITemplate HeaderTemplate
        {
            get { return m_headertemplate; }
            set { m_headertemplate = value; }
        }

        private ITemplate m_footertemplate;

        /// <summary>
        /// Gets or set the Footer Template of the control
        /// </summary>
        [Browsable(false),
        Description("The ShoppingCart Control's Footer Template."),
        TemplateContainer(typeof(ShoppingCartControlItem)),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public ITemplate FooterTemplate
        {
            get { return m_footertemplate; }
            set { m_footertemplate = value; }
        }

        /// <summary>
        /// Gets the ShoppingCart item collection
        /// </summary>
        public ShoppingCartItemCollection Items
        {
            get { return _items; }
        }

        private LineItemSettings m_lineitemsettings;

        /// <summary>
        /// Gets or sets the shopping cart line item settings.
        /// </summary>
        [Browsable(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public LineItemSettings LineItemSettings
        {
            get { return m_lineitemsettings; }
            set { m_lineitemsettings = value; }
        }

        private MinicartSummarySettings m_minicartsummarysetting;

        [Browsable(true),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public MinicartSummarySettings MinicartSummarySetting
        {
            get { return m_minicartsummarysetting; }
            set { m_minicartsummarysetting = value; }
        }

        #endregion
    }

    #region class for events
    /// <summary>
    /// Contains event data for shoppingcart control
    /// </summary>
    public class ItemEventArgs : EventArgs
    {
        private int m_id;
        private int m_qty;
        private List<CartItem> m_cartitemcollection;

        public int ID
        {
            get { return m_id; }
            set { m_id = value; }
        }

        public int Qty
        {
            get { return m_qty; }
            set { m_qty = value; }
        }

        public List<CartItem> CartItemCollection
        {
            get { return m_cartitemcollection; }
            set { m_cartitemcollection = value; }
        }
    }
    #endregion

    #region Class for Data Items
    /// <summary>
    /// Defines the databound items for the shoppingcart control
    /// </summary>
    [ToolboxItem(false)]
    public class ShoppingCartControlItem : CompositeControl, INamingContainer, IDataItemContainer
    {
        #region Constructors
        public ShoppingCartControlItem()
        {
            AllowEdit = true;
        }

        public ShoppingCartControlItem(LineItemSettings _lineItemSettings, MinicartSummarySettings _minicartSummarySettings)
        {
            LineItemSetting = _lineItemSettings;
            MinicartSummarySetting = _minicartSummarySettings;
        }
        #endregion

        #region Controls Declaration

        ITextControl txtQuantity = null;
        RegularExpressionValidator vreQuantity = null;
        DropDownList cboQuantity = new DropDownList();
        Label lblSubTotal = new Label();
        Label lblpricewithBluBucksUsed = new Label();
        Label lblpricewithCategoryFundUsed = new Label();
        Label lblItemPrice = new Label();
        Label lblVatDisplay = new Label();
        Label lblQtyDiscount = new Label();
        Label recurringFrequencyLabel = new Label();
        DropDownList variantList = new DropDownList();
        LinkButton lnkDelete = new LinkButton();
        LinkButton lnkUpdate = new LinkButton();
        CartItem cItem = null;

        private ShoppingCartLineItemDescriptionControl lineItemDescription = new ShoppingCartLineItemDescriptionControl();

        #endregion

        #region IDataItemContainer Members

        /// <summary>
        /// Gets or sets the cart item
        /// </summary>
        public CartItem CartItem
        {
            get { return cItem; }
            set
            {
                cItem = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// Gets the shoppingcart record id
        /// </summary>
        public int ShoppingCartRecId
        {
            get
            {
                return cItem.ShoppingCartRecordID;
            }
        }

        object IDataItemContainer.DataItem
        {
            get { throw new NotImplementedException(); }
        }

        int IDataItemContainer.DataItemIndex
        {
            get { throw new NotImplementedException(); }
        }

        int IDataItemContainer.DisplayIndex
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets the HtmlTextWriterTag value that corresponds to this Web server control. This property is used primarily by control developers.(Inherited from WebControl.)
        /// </summary>
        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Div;
            }
        }

        /// <summary>
        /// Gets or sets the Quantity
        /// </summary>
        public int Quantity
        {
            get
            {
                int qty = 0;
                if (this.cItem.RestrictedQuantities.Count == 0)
                {
                    qty = ValueAsInt(txtQuantity.Text);
                }
                else
                {
                    qty = int.Parse(this.RestrictedQuantities);
                }
                return qty;
            }
            set
            {
                if (this.cItem.RestrictedQuantities.Count == 0)
                {
                    txtQuantity.Text = value.ToString();
                }
                else
                {
                    this.RestrictedQuantities = value.ToString();
                }

            }
        }

        /// <summary>
        /// Gets or sets the Recurring Frequency
        /// </summary>
        public int RecurringVariantId
        {
            get
            {
                return int.Parse(variantList.SelectedValue);
            }

            set
            {
                variantList.SelectedValue = value.ToString();
            }
        }

        /// <summary>
        /// Displays the quantity textbox and delete button of line item if set to true, otherwise, the quantity will be displayed as a label and the delete button will be hidden.
        /// </summary>
        public bool AllowEdit
        {
            get
            {
                object booleanValue = ViewState["AllowEdit"];
                if (null == booleanValue) { return true; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState["AllowEdit"] = value;
                ChangeMode();
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// Gets or set the value indicating whether the control is used as a minicart.
        /// </summary>
        [DefaultValue(0)]
        public CartDisplayMode DisplayMode
        {
            get
            {
                object savedValue = ViewState["DisplayMode"];
                if (null == savedValue) { return CartDisplayMode.ShoppingCart; }

                return (CartDisplayMode)savedValue;
            }
            set
            {
                ViewState["DisplayMode"] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// Gets the item notes in the line item description control
        /// </summary>
        public string ItemNotes
        {
            get
            {
                return lineItemDescription.Notes;
            }
        }

        private bool _isDeleted = false;
        public bool IsDeleted
        {
            get
            {
                return _isDeleted;
            }
            set
            {
                _isDeleted = value;
                ChildControlsCreated = false;
            }
        }
        #endregion

        public bool ShowVariantDropdown
        {
            get
            {
                List<ProductVariant> cItemVariants = new List<ProductVariant>(ProductVariant.GetVariants(this.cItem.ProductID, false));
                return cItemVariants.Where(pv => pv.IsRecurring == true).Count() > 0;
            }
        }

        #region Methods
        /// <summary>
        /// handles instantiation of Quantity display whether a textbox or a label will be used depending on the value of the AllowEdit property
        /// </summary>
        private void ChangeMode()
        {
            if (this.AllowEdit)
            {
                txtQuantity = new TextBox();
            }
            else
            {
                txtQuantity = new Label();
            }
        }

        /// <summary>
        /// Parse the string parameter into integer
        /// </summary>
        /// <param name="s">string value to be parsed</param>
        /// <returns>returns an integer</returns>
        private int ValueAsInt(string s)
        {
            return Localization.ParseNativeInt(s);
        }

        /// <summary>
        /// gets or sets the restricted quantities
        /// </summary>
        public string RestrictedQuantities
        {
            get
            {
                return cboQuantity.SelectedValue;
            }
            set
            {
                cboQuantity.SelectedValue = value;
            }
        }

        #endregion

        #region Overridden Method
        /// <summary>
        /// Creates the child controls in a control derived from System.Web.UI.WebControls.CompositeControl.
        /// </summary>
        protected override void CreateChildControls()
        {
            if (cItem.ProductID > 0)
            {
                txtQuantity.Text = cItem.Quantity.ToString();

                if (this.AllowEdit)
                {
                    (txtQuantity as TextBox).CssClass = "form-control item-quantity";
                    (txtQuantity as TextBox).MaxLength = 6;
                    if (this.DisplayMode == CartDisplayMode.MiniCart &&
                        cItem.CartType == CartTypeEnum.Deleted)
                    {
                        (txtQuantity as TextBox).ReadOnly = true;
                    }
                    else
                    {
                        (txtQuantity as TextBox).ReadOnly = false;

                        vreQuantity = new RegularExpressionValidator();
                        vreQuantity.ControlToValidate = (txtQuantity as TextBox).ID = "txtQuantity";
                        vreQuantity.ID = "vreQuantity";
                        vreQuantity.ValidationExpression = "^\\d+$";
                        vreQuantity.ErrorMessage = "common.cs.80".StringResource();
                        vreQuantity.Text = "*";
                        vreQuantity.EnableClientScript = true;
                        vreQuantity.Display = ValidatorDisplay.Dynamic;
                        vreQuantity.ValidationGroup = "val";
                    }
                }

                if (this.DisplayMode == CartDisplayMode.MiniCart)
                {
                    if (CartItem.CartType == CartTypeEnum.Deleted)
                    {
                        lnkDelete.Text = this.MinicartSummarySetting.MiniCartUndoText;
                    }
                    else
                    {
                        lnkDelete.Text = this.MinicartSummarySetting.MiniCartRemoveText;
                    }
                    lnkDelete.CommandName = "Delete";
                    // lnkUpdate.CommandName = "Update";
                    lnkUpdate.Text = "Update";
                }
                else
                {
                    lnkDelete.Text = "shoppingcart.cs.107".StringResource();
                    lnkDelete.CommandName = "Delete";

                    //  lnkUpdate.CommandName = "Update";
                    lnkUpdate.Text = "Update";
                }
                lnkDelete.CommandArgument = cItem.ShoppingCartRecordID.ToString();
                //  lnkUpdate.CommandArgument = cItem.ShoppingCartRecordID.ToString();

                //Line Item Description
                if (LineItemSetting != null)
                {
                    lineItemDescription.LineItemDescriptionSettings = LineItemSetting;
                }
                lineItemDescription.DataSource = this.CartItem;
                lineItemDescription.AllowEdit = this.AllowEdit;
                lineItemDescription.IsMiniCart = this.DisplayMode == CartDisplayMode.MiniCart;

                if (this.DisplayMode != CartDisplayMode.GiftRegistry)
                {
                    lblSubTotal.Text = cItem.RegularPriceRateDisplayFormat.ToString(); //.ExtPriceRateDisplayFormat.ToString();
                    lblVatDisplay.Text = cItem.VatRateDisplayFormat;
                    lblQtyDiscount.Text = cItem.LineItemQuantityDiscount;
                    lblpricewithBluBucksUsed.Text = Math.Round(cItem.pricewithBluBuksUsed, 2).ToString();
                    lblpricewithCategoryFundUsed.Text = "$" + Math.Round(cItem.pricewithategoryFundUsed, 2).ToString();
                    lblItemPrice.Text = Math.Round(cItem.Price, 2).ToString();

                }
                else
                {
                    lblSubTotal.Text = String.Empty;
                    lblVatDisplay.Text = String.Empty;
                    lblQtyDiscount.Text = String.Empty;
                }

                Controls.Add(new LiteralControl("<tr>"));
                Controls.Add(new LiteralControl("<td class='td-40-percent'>"));//<span class='normal-heading black-color' id='ctl00_PageContent_ctrlShoppingCart_lblProductHeader'>Item</span>
                //Controls.Add(new LiteralControl("<td class='cart-column cart-column-subtotal'><span class='normal-heading black-color' id='ctl00_PageContent_ctrlShoppingCart_lblSubtotalHeader'>Payment</span></td>"));
                //Controls.Add(new LiteralControl("<td class='cart-column cart-column-edit'><span class='normal-heading black-color' id='ctl00_PageContent_ctrlShoppingCart_lblQuantityHeader'>Quantity</span></td>"));
                //Controls.Add(new LiteralControl("</tr>"));
                //Controls.Add(new LiteralControl("<tr>"));
                //NOTE: Added hidden field for js to check for overstock; display message, but allow user to proceed as per business rules
                var inventory = AppLogic.GetInventory(cItem.ProductID, cItem.VariantID, cItem.ChosenSize, cItem.ChosenColor);
                HiddenField hiddenInventory = new HiddenField
                {
                    ID = "Inventory",
                    Value = inventory.ToString()
                };
                Controls.Add(hiddenInventory);
                //END

                if (this.DisplayMode != CartDisplayMode.MiniCart)
                {
                    //LineItemDescription
                    // Controls.Add(new LiteralControl("    <td class='cart-column cart-column-description'>"));
                    Controls.Add(lineItemDescription);
                    Controls.Add(new LiteralControl("    </td>"));
                    //payment sub total here
                    Controls.Add(new LiteralControl("<td class='td-35-percent'>"));//<span class='normal-heading black-color' id='ctl00_PageContent_ctrlShoppingCart_lblSubtotalHeader'>Payment</span>
                    //  Controls.Add(new LiteralControl("<td class='cart-column cart-column-subtotal'>"));

                    // Controls.Add(new LiteralControl("   <td class='cart-row'>"));

                    if (this.DisplayMode == CartDisplayMode.MiniCart &&
                        CartItem.CartType == CartTypeEnum.Deleted)
                    {
                        // Controls.Add(new LiteralControl("     <td class='cart-column cart-price cart-column-price opacity-twenty'>"));
                    }
                    else
                    {
                        // Controls.Add(new LiteralControl("     <td class=''>"));
                    }

                    if (this.DisplayMode == CartDisplayMode.WishList || this.DisplayMode == CartDisplayMode.GiftRegistry)
                    {
                        int itemInventory = AppLogic.GetInventory(cItem.ProductID, cItem.VariantID, cItem.ChosenSize, cItem.ChosenColor);

                        //if (itemInventory > 0)
                        //{
                        Button btnMoveToCart = new Button();

                        if (this.DisplayMode == CartDisplayMode.WishList)
                        {
                            btnMoveToCart.Text = LineItemSetting.MoveFromWishListText;
                        }
                        else
                        {
                            btnMoveToCart.Text = LineItemSetting.MoveFromGiftRegistryText;
                        }

                        btnMoveToCart.ID = "btnMoveToCart";
                        btnMoveToCart.CssClass = "button call-to-action button-move-to-cart";
                        btnMoveToCart.CommandName = "MoveToShoppingCart";
                        btnMoveToCart.CommandArgument = cItem.ShoppingCartRecordID.ToString();
                        Controls.Add(btnMoveToCart);
                        //}
                        //else
                        //{
                        //    Literal ltUnavailableMessage = new Literal();
                        //    ltUnavailableMessage.ID = "ltUnavailableMessage";
                        //    ltUnavailableMessage.Text = AppLogic.GetString("shoppingcart.outofstock", Customer.Current.LocaleSetting);
                        //    Controls.Add(ltUnavailableMessage);
                        //}
                    }
                    else
                    {
                        if (String.IsNullOrEmpty(cItem.FundName))
                            cItem.FundName = "";
                        else
                            cItem.FundName = cItem.FundName + " used: ";
                        
                        String CreditPrice = lblSubTotal.Text;
                        System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("en-US");
                        Controls.Add(new LiteralControl("<span class='normal-heading black-color' id='ctl00_PageContent_ctrlShoppingCart_lblSubtotalHeader'>Payment</span>"));
                        Decimal RegularPrices = Convert.ToDecimal(lblSubTotal.Text.Replace("$", "")) + Convert.ToDecimal(lblpricewithCategoryFundUsed.Text.Replace("$", "")) + Convert.ToDecimal(lblpricewithBluBucksUsed.Text.Replace("$", ""));
                        lblSubTotal.Text = "<span id='" + "spregularprice_" + cItem.ShoppingCartRecordID.ToString() + "' class='hide-element spregularprice'><b>Regular Price: </b>$" + Math.Round(RegularPrices, 2).ToString() + "</span>";
                        if (Convert.ToDecimal(lblpricewithCategoryFundUsed.Text.Replace("$", "")) > 0)
                            lblSubTotal.Text += "<span id='" + "spfunddiscountprice1_" + cItem.ShoppingCartRecordID.ToString() + "' class='hide-element spfunddiscountprice'>" + cItem.FundName + lblpricewithCategoryFundUsed.Text + "</span>";
                       if (Convert.ToDecimal(lblpricewithBluBucksUsed.Text.Replace("$", "")) > 0)
                            lblSubTotal.Text += "<span id='" + "spblubucksprice1_" + cItem.ShoppingCartRecordID.ToString() + "' class='hide-element spblubucksprice'>BLU Bucks used: " + lblpricewithBluBucksUsed.Text + "" + "</span>";
                        lblSubTotal.Text += "<span id='" + "spcreditprice_" + cItem.ShoppingCartRecordID.ToString() + "' ><b>Your Price: </b>" + CreditPrice + "</span>";
                        lblSubTotal.Text += "<span id='" + "spItemPrice_" + cItem.ShoppingCartRecordID.ToString() + "' class='hide-element spItemPrice'><b>Item Price: </b>$" + lblItemPrice.Text.ToString() + "</span>" + "<span id='" + "spItemFundId_" + cItem.ShoppingCartRecordID.ToString() + "' class='hide-element spItemFundId'>" + cItem.FundID.ToString() + "</span>" + "<span id='" + "spItemProductCategoryId_" + cItem.ShoppingCartRecordID.ToString() + "' class='hide-element spItemProductCategoryId'>" + cItem.ProductCategoryID.ToString() + "</span>" + "<span id='" + "spBluBucksPercentageUsed_" + cItem.ShoppingCartRecordID.ToString() + "' class='hide-element spBluBucksPercentageUsed'>" + cItem.BluBucksPercentageUsed.ToString() + "</span>" + "<span id='" + "spItemQuantity_" + cItem.ShoppingCartRecordID.ToString() + "' class='hide-element spItemQuantity'>" + cItem.Quantity.ToString() + "</span>";
                        lblSubTotal.Text += "<span id='" + "spfunddiscountprice_" + cItem.ShoppingCartRecordID.ToString() + "' class='hide-element'>" + cItem.FundName + lblpricewithCategoryFundUsed.Text + "</span>";
                        lblSubTotal.Text += "<span id='" + "spblubucksprice_" + cItem.ShoppingCartRecordID.ToString() + "' class='hide-element'>BLU Bucks used: " + lblpricewithBluBucksUsed.Text + "" + "</span>";
                        lblSubTotal.Text += "<span id='" + "spInventory_" + cItem.ShoppingCartRecordID.ToString() + "' class='hide-element'>" + cItem.Inventory + "" + "</span>";

                        Controls.Add(lblSubTotal);
                        Controls.Add(new LiteralControl("        </td>"));
                    }
                    Controls.Add(new LiteralControl("                 </td>"));
                    Controls.Add(new LiteralControl("             </td>"));
                    Controls.Add(new LiteralControl("        </td>"));
                    Controls.Add(new LiteralControl("</td>"));
                    //end payment sub total 
                    Controls.Add(new LiteralControl("<td class='td-25-percent'><span class='normal-heading black-color' id='ctl00_PageContent_ctrlShoppingCart_lblQuantityHeader'>Quantity</span>"));
                    //Controls.Add(new LiteralControl("    <td class='cart-column cart-column-edit'>"));
                    if (this.AllowEdit)
                    {
                        //   Controls.Add(new LiteralControl("     <td class='edit-wrap'>"));
                    }
                    else
                    {
                        //  Controls.Add(new LiteralControl("      <td>")); 
                    }

                    //check for restricted quantity
                    if (this.CartItem.RestrictedQuantities.Count == 0 || !this.AllowEdit)
                    {
                        Controls.Add(txtQuantity as Control);
                        if (this.AllowEdit)
                        {
                            Controls.Add(vreQuantity);
                        }
                    }
                    else
                    {
                        // build dropdown for restricted quantities                    
                        BuildRestrictedQuantities();
                        Controls.Add(cboQuantity);

                        Controls.Add(new LiteralControl("  <div class='shopping-cart-links'>"));
                        Controls.Add(lnkDelete);
                        Controls.Add(new LiteralControl("<font>|</font>"));
                        lnkUpdate.CssClass = "lnkUpdateItem";
                        lnkUpdate.ID = cItem.ShoppingCartRecordID.ToString();
                        Controls.Add(lnkUpdate);
                        Controls.Add(new LiteralControl("  </div>"));
                    }


                    //   Controls.Add(new LiteralControl("    </td>")); 

                    if (this.DisplayMode != CartDisplayMode.MiniCart && this.DisplayMode != CartDisplayMode.GiftRegistry)
                    {
                        if (this.AllowEdit && this.CartItem.RestrictedQuantities.Count == 0)
                        {
                            Controls.Add(new LiteralControl("  <div class='shopping-cart-links'>"));
                            Controls.Add(lnkDelete);
                            Controls.Add(new LiteralControl("<font>|</font>"));
                            lnkUpdate.CssClass = "lnkUpdateItem";
                            lnkUpdate.ID = cItem.ShoppingCartRecordID.ToString();
                            Controls.Add(lnkUpdate);
                            Controls.Add(new LiteralControl("  </div>"));
                        }


                    }

                    Controls.Add(new LiteralControl("  </td>"));
                }
                else
                {
                    Controls.Add(new LiteralControl(" <td class='td-25-percent'>"));
                    if (this.AllowEdit)
                    {
                        if (this.DisplayMode == CartDisplayMode.MiniCart &&
                            CartItem.CartType == CartTypeEnum.Deleted)
                        {
                            Controls.Add(new LiteralControl("   <td class='left opacity-twenty'>"));
                        }
                        else
                        {
                            Controls.Add(new LiteralControl("   <td class='left'>"));
                        }
                    }
                    else
                    {
                        Controls.Add(new LiteralControl("        <td>"));
                    }

                    if (txtQuantity is TextBox)
                    {
                        TextBox txt = txtQuantity as TextBox;
                        txt.CssClass = "form-control item-quantity";
                        txt.MaxLength = 6;
                    }

                    Controls.Add(txtQuantity as Control);
                    if (this.AllowEdit)
                    {
                        Controls.Add(vreQuantity);
                    }
                    Controls.Add(new LiteralControl("            </td>"));

                    if (this.DisplayMode != CartDisplayMode.MiniCart)
                    {
                        if (this.AllowEdit)
                        {
                            Controls.Add(new LiteralControl("            <td class='delete-wrap'>"));
                            Controls.Add(lnkDelete);
                            Controls.Add(new LiteralControl("            </td>"));
                        }
                    }
                    Controls.Add(new LiteralControl("          </td>"));

                    if (this.DisplayMode == CartDisplayMode.MiniCart &&
                        CartItem.CartType == CartTypeEnum.Deleted)
                    {
                        Controls.Add(new LiteralControl("          <td class='cart-column cart-column-description opacity-twenty'>"));
                    }
                    else
                    {
                        Controls.Add(new LiteralControl("          <td class='cart-column cart-column-description'>"));
                    }
                    Controls.Add(lineItemDescription);
                    Controls.Add(new LiteralControl("          </td>"));
                }

                //Controls.Add(new LiteralControl("<div class='cart-column cart-column-subtotal'>"));

                //Controls.Add(new LiteralControl("   <div class='cart-row'>"));

                //if (this.DisplayMode == CartDisplayMode.MiniCart &&
                //    CartItem.CartType == CartTypeEnum.Deleted)
                //{
                //    Controls.Add(new LiteralControl("     <div class='cart-column cart-price cart-column-price opacity-twenty'>"));
                //}
                //else
                //{
                //    Controls.Add(new LiteralControl("     <div class='cart-column cart-price cart-column-price'>"));
                //}

                //if (this.DisplayMode == CartDisplayMode.WishList || this.DisplayMode == CartDisplayMode.GiftRegistry)
                //{
                //    int itemInventory = AppLogic.GetInventory(cItem.ProductID, cItem.VariantID, cItem.ChosenSize, cItem.ChosenColor);

                //    //if (itemInventory > 0)
                //    //{
                //    Button btnMoveToCart = new Button();

                //    if (this.DisplayMode == CartDisplayMode.WishList)
                //    {
                //        btnMoveToCart.Text = LineItemSetting.MoveFromWishListText;
                //    }
                //    else
                //    {
                //        btnMoveToCart.Text = LineItemSetting.MoveFromGiftRegistryText;
                //    }

                //    btnMoveToCart.ID = "btnMoveToCart";
                //    btnMoveToCart.CssClass = "button call-to-action button-move-to-cart";
                //    btnMoveToCart.CommandName = "MoveToShoppingCart";
                //    btnMoveToCart.CommandArgument = cItem.ShoppingCartRecordID.ToString();
                //    Controls.Add(btnMoveToCart);
                //    //}
                //    //else
                //    //{
                //    //    Literal ltUnavailableMessage = new Literal();
                //    //    ltUnavailableMessage.ID = "ltUnavailableMessage";
                //    //    ltUnavailableMessage.Text = AppLogic.GetString("shoppingcart.outofstock", Customer.Current.LocaleSetting);
                //    //    Controls.Add(ltUnavailableMessage);
                //    //}
                //}
                //else
                //{
                //    Controls.Add(lblSubTotal);
                //}


                if (this.DisplayMode == CartDisplayMode.MiniCart)
                {
                    Controls.Add(new LiteralControl("              <td class='delete-wrap'>"));
                    Controls.Add(lnkDelete);
                    Controls.Add(new LiteralControl("              </td>"));
                }

                if (this.DisplayMode != CartDisplayMode.MiniCart)
                {
                    if (cItem.ThisShoppingCart.VatEnabled && !(cItem.ThisShoppingCart.VatIsInclusive))
                    {
                        Controls.Add(new LiteralControl("               <td class='vat-wrap'>"));
                        Controls.Add(lblVatDisplay);
                        Controls.Add(new LiteralControl("              </td>"));
                    }

                    if (QuantityDiscount.CustomerLevelAllowsQuantityDiscounts(cItem.ThisCustomer.CustomerLevelID))
                    {
                        Controls.Add(new LiteralControl("               <td class='cart-small quantity-discount-wrap'>"));
                        Controls.Add(lblQtyDiscount);
                        Controls.Add(new LiteralControl("              </td>"));
                    }
                }
                //Controls.Add(new LiteralControl("                 </div>"));
                //Controls.Add(new LiteralControl("             </div>"));
                //Controls.Add(new LiteralControl("        </div>")); 
                //Controls.Add(new LiteralControl("</div>"));  
                //Recurring frequency editor
                List<ProductVariant> cItemVariants = new List<ProductVariant>(ProductVariant.GetVariants(this.cItem.ProductID, false));

                if (this.DisplayMode != CartDisplayMode.MiniCart
                    && ShowVariantDropdown
                    && AppLogic.AppConfigBool("AllowRecurringFrequencyChangeInCart")
                    && CommonLogic.GetThisPageName(false).ToLowerInvariant().Equals("shoppingcart.aspx"))
                {
                    decimal basePrice = Prices.VariantPriceLookup(Customer.Current, cItem.VariantID);

                    foreach (ProductVariant pv in cItemVariants)
                    {
                        if (pv.Inventory > AppLogic.AppConfigNativeInt("HideProductsWithLessThanThisInventoryLevel") || !AppLogic.AppConfigBool("Inventory.LimitCartToQuantityOnHand"))
                        {
                            ListItem itemToAdd = new ListItem(pv.LocaleName, pv.VariantID.ToString());
                            variantList.Items.Add(itemToAdd);
                        }
                    }

                    variantList.SelectedValue = cItem.VariantID.ToString();

                    recurringFrequencyLabel.Text = AppLogic.GetString("shoppingcart.recurring.label", Customer.Current.LocaleSetting);
                    recurringFrequencyLabel.ID = "lblRecurringFrequency";

                    Controls.Add(new LiteralControl("<td class='recurring-manager'>"));
                    Controls.Add(new LiteralControl("<td class='recurring-manager-row'>"));
                    Controls.Add(new LiteralControl("<td class='recurring-manager-label'>"));
                    Controls.Add(recurringFrequencyLabel);
                    Controls.Add(new LiteralControl("</td"));
                    Controls.Add(new LiteralControl("<td class='recurring-manager-variant'>"));
                    Controls.Add(variantList);
                    Controls.Add(new LiteralControl("</td"));
                    Controls.Add(new LiteralControl("</td>"));
                    Controls.Add(new LiteralControl("</td>"));
                }
            }
        }

        #endregion

        #region Methods
        private void BuildRestrictedQuantities()
        {
            string DELETE_QTY = "DELETE";
            List<int> quantities = cItem.RestrictedQuantities;
            string defaultSelectedValue = DELETE_QTY;
            cboQuantity.ID = "txtQuantity";
            cboQuantity.CssClass = "item-quantity";
            // cboQuantity.Items.Add(new ListItem(DELETE_QTY, "0"));
            foreach (int qty in quantities)
            {
                cboQuantity.Items.Add(new ListItem(qty.ToString(), qty.ToString()));

                if (qty.Equals(cItem.Quantity))
                {
                    defaultSelectedValue = qty.ToString();
                }
            }

            cboQuantity.SelectedValue = defaultSelectedValue;
            ChildControlsCreated = false;
        }
        #endregion


        #region LineItem Properties

        private LineItemSettings m_lineitemsetting;

        /// <summary>
        /// Gets or sets the shopping cart line item settings.
        /// </summary>
        public LineItemSettings LineItemSetting
        {
            get { return m_lineitemsetting; }
            set { m_lineitemsetting = value; }
        }

        #endregion

        private MinicartSummarySettings m_minicartsymmarysetting;

        /// <summary>
        /// Gets or sets the shopping cart line item settings.
        /// </summary>
        public MinicartSummarySettings MinicartSummarySetting
        {
            get { return m_minicartsymmarysetting; }
            set { m_minicartsymmarysetting = value; }
        }
    }
    #endregion

    /// <summary>
    /// Defines the property settings for minincart line item
    /// </summary>
    public class MinicartSummarySettings
    {
        private bool m_useinajaxminicart;

        [Browsable(true), PersistenceMode(PersistenceMode.Attribute)]
        public bool UseInAjaxMiniCart
        {
            get { return m_useinajaxminicart; }
            set { m_useinajaxminicart = value; }
        }

        private bool m_calculateshippingduringcheckout;

        [Browsable(true), PersistenceMode(PersistenceMode.Attribute)]
        public bool CalculateShippingDuringCheckout
        {
            get { return m_calculateshippingduringcheckout; }
            set { m_calculateshippingduringcheckout = value; }
        }

        private bool m_calculatetaxduringcheckout;

        [Browsable(true), PersistenceMode(PersistenceMode.Attribute)]
        public bool CalculateTaxDuringCheckout
        {
            get { return m_calculatetaxduringcheckout; }
            set { m_calculatetaxduringcheckout = value; }
        }

        private bool m_showtotal;

        [Browsable(true), PersistenceMode(PersistenceMode.Attribute)]
        public bool ShowTotal
        {
            get { return m_showtotal; }
            set { m_showtotal = value; }
        }

        private bool m_showshipping;

        [Browsable(true), PersistenceMode(PersistenceMode.Attribute)]
        public bool ShowShipping
        {
            get { return m_showshipping; }
            set { m_showshipping = value; }
        }

        private bool m_skipshippingoncheckout;

        [Browsable(true), PersistenceMode(PersistenceMode.Attribute)]
        public bool SkipShippingOnCheckout
        {
            get { return m_skipshippingoncheckout; }
            set { m_skipshippingoncheckout = value; }
        }

        private bool m_showgiftcardtotal;

        [Browsable(true), PersistenceMode(PersistenceMode.Attribute)]
        public bool ShowGiftCardTotal
        {
            get { return m_showgiftcardtotal; }
            set { m_showgiftcardtotal = value; }
        }

        private string m_giftcardtotalcaption;

        [Browsable(true), PersistenceMode(PersistenceMode.Attribute)]
        public string GiftCardTotalCaption
        {
            get { return m_giftcardtotalcaption; }
            set { m_giftcardtotalcaption = value; }
        }

        private string m_calculateshippingduringcheckouttext;

        [Browsable(true), PersistenceMode(PersistenceMode.Attribute)]
        public string CalculateShippingDuringCheckoutText
        {
            get { return m_calculateshippingduringcheckouttext; }
            set { m_calculateshippingduringcheckouttext = value; }
        }

        private string m_calculatetaxduringcheckouttext;

        [Browsable(true), PersistenceMode(PersistenceMode.Attribute)]
        public string CalculateTaxDuringCheckoutText
        {
            get { return m_calculatetaxduringcheckouttext; }
            set { m_calculatetaxduringcheckouttext = value; }
        }

        private string m_LineItemDiscountCaption;

        [Browsable(true), PersistenceMode(PersistenceMode.Attribute)]
        public string LineItemDiscountCaption
        {
            get { return m_LineItemDiscountCaption; }
            set { m_LineItemDiscountCaption = value; }
        }

        private string m_subtotalcaption;

        [Browsable(true), PersistenceMode(PersistenceMode.Attribute)]
        public string SubTotalCaption
        {
            get { return m_subtotalcaption; }
            set { m_subtotalcaption = value; }
        }

        private string m_subtotalwithdiscountcaption;

        [Browsable(true), PersistenceMode(PersistenceMode.Attribute)]
        public string SubTotalWithDiscountCaption
        {
            get { return m_subtotalwithdiscountcaption; }
            set { m_subtotalwithdiscountcaption = value; }
        }

        private string m_shippingcaption;

        [Browsable(true), PersistenceMode(PersistenceMode.Attribute)]
        public string ShippingCaption
        {
            get { return m_shippingcaption; }
            set { m_shippingcaption = value; }
        }

        private string m_taxcaption;

        [Browsable(true), PersistenceMode(PersistenceMode.Attribute)]
        public string TaxCaption
        {
            get { return m_taxcaption; }
            set { m_taxcaption = value; }
        }

        private string m_shippingvatexcaption;

        [Browsable(true), PersistenceMode(PersistenceMode.Attribute)]
        public string ShippingVatExCaption
        {
            get { return m_shippingvatexcaption; }
            set { m_shippingvatexcaption = value; }
        }

        private string m_OrderDiscountCaption;

        [Browsable(true), PersistenceMode(PersistenceMode.Attribute)]
        public string OrderDiscountCaption
        {
            get { return m_OrderDiscountCaption; }
            set { m_OrderDiscountCaption = value; }
        }

        private string m_totalcaption;

        [Browsable(true), PersistenceMode(PersistenceMode.Attribute)]
        public string TotalCaption
        {
            get { return m_totalcaption; }
            set { m_totalcaption = value; }
        }

        private string m_buttonupdatetext;

        [Browsable(true), PersistenceMode(PersistenceMode.Attribute)]
        public string ButtonUpdateText
        {
            get { return m_buttonupdatetext; }
            set { m_buttonupdatetext = value; }
        }

        private string m_minicartcheckouttext;

        /// <summary>
        /// Gets or set the stringresource for Minicart Checkout Link
        /// </summary>
        [Browsable(true), PersistenceMode(PersistenceMode.Attribute)]
        public string MiniCartCheckoutText
        {
            get { return m_minicartcheckouttext; }
            set { m_minicartcheckouttext = value; }
        }

        private string m_minicartupdatetext;

        /// <summary>
        /// Gets or set the stringresource for Minicart Checkout Link
        /// </summary>
        [Browsable(true), PersistenceMode(PersistenceMode.Attribute)]
        public string MiniCartUpdateText
        {
            get { return m_minicartupdatetext; }
            set { m_minicartupdatetext = value; }
        }

        private string m_minicartremovetext;

        /// <summary>
        /// Gets or set the stringresource for Minicart Checkout Link
        /// </summary>
        [Browsable(true), PersistenceMode(PersistenceMode.Attribute)]
        public string MiniCartRemoveText
        {
            get { return m_minicartremovetext; }
            set { m_minicartremovetext = value; }
        }

        private string m_minicartundotext;

        /// <summary>
        /// Gets or set the stringresource for Minicart Checkout Link
        /// </summary>
        [Browsable(true), PersistenceMode(PersistenceMode.Attribute)]
        public string MiniCartUndoText
        {
            get { return m_minicartundotext; }
            set { m_minicartundotext = value; }
        }

        private string m_minicartviewcarttext;

        /// <summary>
        /// Gets or set the stringresource for Minicart Checkout Link
        /// </summary>
        [Browsable(true), PersistenceMode(PersistenceMode.Attribute)]
        public string MiniCartViewCartText
        {
            get { return m_minicartviewcarttext; }
            set { m_minicartviewcarttext = value; }
        }
    }


    #region Class for LineItemSettings
    /// <summary>
    /// Defines the property settings for shoppingcart line item
    /// </summary>
    public class LineItemSettings
    {
        private bool m_vatenabled;
        /// <summary>
        /// Gets or set the value indicating whether the VAT is enabled 
        /// </summary>
        [Browsable(true), PersistenceMode(PersistenceMode.Attribute)]
        public bool VatEnabled
        {
            get { return m_vatenabled; }
            set { m_vatenabled = value; }
        }

        private bool m_showpicsincart;
        /// <summary>
        /// Gets or set the value indicating whether the Product picture will be displayed in the line item description.
        /// </summary>
        [Browsable(true), PersistenceMode(PersistenceMode.Attribute)]
        public bool ShowPicsInCart
        {
            get { return m_showpicsincart; }
            set { m_showpicsincart = value; }
        }

        private bool m_linktoproductpageincart;

        /// <summary>
        /// Gets or set the value indicating whether the product name will have a link back to product page
        /// </summary>
        [Browsable(true), PersistenceMode(PersistenceMode.Attribute)]
        public bool LinkToProductPageInCart
        {
            get { return m_linktoproductpageincart; }
            set { m_linktoproductpageincart = value; }
        }

        private int m_itemnotescolumns;

        /// <summary>
        /// Gets or set the number of columns of the Item Notes textbox
        /// </summary>
        [Browsable(true), PersistenceMode(PersistenceMode.Attribute)]
        public int ItemNotesColumns
        {
            get { return m_itemnotescolumns; }
            set { m_itemnotescolumns = value; }
        }

        private int m_itemnotesrows;
        /// <summary>
        /// Gets or sets the number of rows of the Item Notes textbox
        /// </summary>
        [Browsable(true), PersistenceMode(PersistenceMode.Attribute)]
        public int ItemNotesRows
        {
            get { return m_itemnotesrows; }
            set { m_itemnotesrows = value; }
        }

        private bool m_allowshoppingcartitemnotes;

        /// <summary>
        /// Gets or sets the value indication whether the Item Notes is allowed in the shoppingcart control.
        /// </summary>
        [Browsable(true), PersistenceMode(PersistenceMode.Attribute)]
        public bool AllowShoppingCartItemNotes
        {
            get { return m_allowshoppingcartitemnotes; }
            set { m_allowshoppingcartitemnotes = value; }
        }

        private bool m_showeditbuttonincartforkitproducts;
        /// <summary>
        /// Gets or sets the value indicating whether the EDIT button for kit product will be displayed in the line item description.
        /// </summary>
        [Browsable(true), PersistenceMode(PersistenceMode.Attribute)]
        public bool ShowEditButtonInCartForKitProducts
        {
            get { return m_showeditbuttonincartforkitproducts; }
            set { m_showeditbuttonincartforkitproducts = value; }
        }

        private bool m_showmultishipaddressunderitemdescription;
        /// <summary>
        /// Gets or sets the value indicating whether the Multi shipping address will be displayed in the line item description.
        /// </summary>
        [Browsable(true), PersistenceMode(PersistenceMode.Attribute)]
        public bool ShowMultiShipAddressUnderItemDescription
        {
            get { return m_showmultishipaddressunderitemdescription; }
            set { m_showmultishipaddressunderitemdescription = value; }
        }

        #region StringResources Properties

        private string m_skucaption;
        /// <summary>
        /// Gets or set the stringresource for SKU Caption
        /// </summary>
        [Browsable(true), PersistenceMode(PersistenceMode.Attribute)]
        public string SKUCaption
        {
            get { return m_skucaption; }
            set { m_skucaption = value; }
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

        private string m_giftregistrycaption;

        /// <summary>
        /// Gets or set the stringresource for Gift registry caption
        /// </summary>
        [Browsable(true), PersistenceMode(PersistenceMode.Attribute)]
        public string GiftRegistryCaption
        {
            get { return m_giftregistrycaption; }
            set { m_giftregistrycaption = value; }
        }

        private string m_itemnotescaption;

        /// <summary>
        /// Gets or set the stringresource for Item Notes Caption
        /// </summary>
        [Browsable(true), PersistenceMode(PersistenceMode.Attribute)]
        public string ItemNotesCaption
        {
            get { return m_itemnotescaption; }
            set { m_itemnotescaption = value; }
        }

        private string m_minimumquantitynotificationtext;

        /// <summary>
        /// Gets or set the stringresource for minimum quantity notification
        /// </summary>
        [Browsable(true), PersistenceMode(PersistenceMode.Attribute)]
        public string MinimumQuantityNotificationText
        {
            get { return m_minimumquantitynotificationtext; }
            set { m_minimumquantitynotificationtext = value; }
        }

        private string m_movefromwishlisttext;
        [Browsable(true), PersistenceMode(PersistenceMode.Attribute)]
        public string MoveFromWishListText
        {
            get { return m_movefromwishlisttext; }
            set { m_movefromwishlisttext = value; }
        }

        private string m_movefromgiftregistrytext;

        [Browsable(true), PersistenceMode(PersistenceMode.Attribute)]
        public string MoveFromGiftRegistryText
        {
            get { return m_movefromgiftregistrytext; }
            set { m_movefromgiftregistrytext = value; }
        }

        #endregion

    }
    #endregion

    #region Class for Smart Tag Items
    /// <summary>
    /// Defines a list of items used to create a smart tag panel for shoppingcart control
    /// </summary>
    public class ShoppingCartControlActionList : DesignerActionList
    {
        #region Constructor

        private ShoppingCartControl ctrlShoppingCart;

        public ShoppingCartControlActionList(ShoppingCartControl ctrl)
            : base(ctrl) { ctrlShoppingCart = ctrl; }

        #endregion

        /// <summary>
        /// Returns the collection of DesignerActionItem objects contained in the list.
        /// </summary>
        /// <returns>A DesignerActionItem array that contains the items in this list</returns>
        public override DesignerActionItemCollection GetSortedActionItems()
        {
            DesignerActionItemCollection actionItems = new DesignerActionItemCollection();

            actionItems.Add(new DesignerActionHeaderItem("WWW.AspDotNetStoreFront.com"));

            return actionItems;
        }

    }
    #endregion

    #region Class for Item Collection
    /// <summary>
    /// Collection of ShoppingCartControlItem
    /// </summary>
    public class ShoppingCartItemCollection : List<ShoppingCartControlItem> { }
    #endregion

    #region Class for Design Time Support
    /// <summary>
    /// Extends design-time behavior for the shoppingcart controls
    /// </summary>
    public class ShoppingControlDesigner : CompositeControlDesigner
    {
        private DesignerActionListCollection m_ActionList;
        private TemplateGroupCollection _templateGroupCollection = null;

        #region Template Editing Support
        /// <summary>
        /// Overridden. Initializes the designer with the specified IComponent object. 
        /// </summary>
        /// <param name="component"></param>
        public override void Initialize(IComponent component)
        {
            base.Initialize(component);
            SetViewFlags(ViewFlags.TemplateEditing, true);
        }

        /// <summary>
        /// Gets a collection of template groups, each containing one or more template definitions.(Inherited from ControlDesigner.)
        /// </summary>
        public override TemplateGroupCollection TemplateGroups
        {
            get
            {
                if (_templateGroupCollection == null)
                {
                    _templateGroupCollection = new TemplateGroupCollection();

                    TemplateGroup _templateGroup = default(TemplateGroup);
                    TemplateDefinition _templateDefinition = default(TemplateDefinition);

                    ShoppingCartControl _shoppingcartControl = (ShoppingCartControl)Component;

                    //initialize template group
                    _templateGroup = new TemplateGroup("Shopping Cart Templates");

                    //initialize template definition

                    //header tab template
                    _templateDefinition = new TemplateDefinition(this, "Header Tab Template", _shoppingcartControl, "HeaderTabTemplate", true);
                    _templateGroup.AddTemplateDefinition(_templateDefinition);

                    //header template
                    _templateDefinition = new TemplateDefinition(this, "Header Template", _shoppingcartControl, "HeaderTemplate", true);
                    _templateGroup.AddTemplateDefinition(_templateDefinition);

                    //footer template
                    _templateDefinition = new TemplateDefinition(this, "Footer Template", _shoppingcartControl, "FooterTemplate", true);
                    _templateGroup.AddTemplateDefinition(_templateDefinition);

                    //add the group to the group collection
                    _templateGroupCollection.Add(_templateGroup);

                }
                return _templateGroupCollection;
            }
        }
        #endregion

        #region Smart Tag Support

        /// <summary>
        /// Gets the action list collection for the control designer.(Inherited from ControlDesigner.)
        /// </summary>
        public override DesignerActionListCollection ActionLists
        {
            get
            {
                m_ActionList = new DesignerActionListCollection();
                ShoppingCartControl ctrlShoppingCart = (ShoppingCartControl)Component;
                m_ActionList.Add(new ShoppingCartControlActionList(ctrlShoppingCart));

                return m_ActionList;
            }
        }

        #endregion

        #region Auto Formatting
        /// <summary>
        /// Gets the collection of predefined automatic formatting schemes to display in the Auto Format dialog box for the associated control at design time. (Inherited from ControlDesigner.)
        /// </summary>
        public override DesignerAutoFormatCollection AutoFormats
        {
            get
            {
                DesignerAutoFormatCollection m_autoformat = new DesignerAutoFormatCollection();
                m_autoformat.Add(new NoFormatLayout());

                return m_autoformat;
            }
        }
        #endregion
    }
    #endregion

    #region Class for Formats
    /// <summary>
    ///  A shoppingcart designer autoformat object that defines no format 
    /// </summary>
    public class NoFormatLayout : DesignerAutoFormat
    {
        public NoFormatLayout() : base("No Format") { }

        /// <summary>
        /// Applies the associated formatting to the specified control. 
        /// </summary>
        /// <param name="control"></param>
        public override void Apply(Control control)
        {
            ShoppingCartControl ctrlShoppingCart = (ShoppingCartControl)control;

        }

    }
    #endregion

    #region Class for Default Header Template

    /// <summary>
    /// Default Template for ShoppingCart's Header
    /// </summary>
    internal sealed class DefaultHeaderTemplate : CompositeControl, INamingContainer, ITemplate
    {
        private string m_productheadertext;

        /// <summary>
        /// Gets or sets the product header text
        /// </summary>
        public string ProductHeaderText
        {
            get { return m_productheadertext; }
            set { m_productheadertext = value; }
        }

        private string m_quantityheadertext;

        /// <summary>
        /// Gets or sets the quantity header text
        /// </summary>
        public string QuantityHeaderText
        {
            get { return m_quantityheadertext; }
            set { m_quantityheadertext = value; }
        }

        private string m_subtotalheadertext;

        /// <summary>
        /// Gets or sets the Subtotal header text
        /// </summary>
        public string SubTotalHeaderText
        {
            get { return m_subtotalheadertext; }
            set { m_subtotalheadertext = value; }
        }

        private string m_recurringfrequencylabeltext;

        /// <summary>
        /// Gets or sets the Subtotal header text
        /// </summary>
        public string RecurringFrequencyLabelText
        {
            get { return m_recurringfrequencylabeltext; }
            set { m_recurringfrequencylabeltext = value; }
        }

        private string m_cartmessage;

        /// <summary>
        /// Gets or sets the cart notification message
        /// </summary>
        public string CartMessage
        {
            get { return m_cartmessage; }
            set { m_cartmessage = value; }
        }

        /// <summary>
        /// Gets or set the value indicating whether the control is used as a minicart.
        /// </summary>
        [DefaultValue(0)]
        public CartDisplayMode DisplayMode
        {
            get;
            set;
        }

        #region Constructors
        public DefaultHeaderTemplate() { }

        public DefaultHeaderTemplate(string cartMessage)
        {
            CartMessage = cartMessage;
        }

        public DefaultHeaderTemplate(string prodHeaderText, string qtyHeaderText, string subTotHeaderText, CartDisplayMode displayMode)
        {
            ProductHeaderText = prodHeaderText;
            QuantityHeaderText = qtyHeaderText;
            SubTotalHeaderText = subTotHeaderText;
            DisplayMode = displayMode;
        }
        #endregion

        #region ITemplate Members
        /// <summary>
        /// Defines the control object where the child controls and templates belongs.
        /// </summary>
        /// <param name="container"></param>
        public void InstantiateIn(Control container)
        {
            if (CartMessage == string.Empty || CartMessage == null)
            {
                Label lblProductHeader = new Label();
                lblProductHeader.ID = "lblProductHeader";
                lblProductHeader.Text = ProductHeaderText;

                Label lblQuantityHeader = new Label();
                lblQuantityHeader.ID = "lblQuantityHeader";
                lblQuantityHeader.Text = QuantityHeaderText;

                Label lblSubtotalHeader = new Label();
                lblSubtotalHeader.ID = "lblSubtotalHeader";
                lblSubtotalHeader.Text = SubTotalHeaderText;

                container.Controls.Add(new LiteralControl("        <div class='cart-row cart-header'>"));

                container.Controls.Add(new LiteralControl("          <div class='cart-column cart-column-description'>"));
                container.Controls.Add(lblProductHeader);
                container.Controls.Add(new LiteralControl("          </div>"));

                container.Controls.Add(new LiteralControl("          <div class='cart-column cart-column-subtotal'>"));
                container.Controls.Add(lblSubtotalHeader);
                container.Controls.Add(new LiteralControl("          </div>"));

                container.Controls.Add(new LiteralControl("          <div class='cart-column cart-column-edit'>"));
                container.Controls.Add(lblQuantityHeader);
                container.Controls.Add(new LiteralControl("          </div>"));


                container.Controls.Add(new LiteralControl("        </div>"));
            }
            else
            {
                Literal lblCartMessage = new Literal();
                lblCartMessage.ID = "lblCartMessage";
                lblCartMessage.Text = CartMessage;

                container.Controls.Add(new LiteralControl("        <div class='cart-row cart-empty'>"));
                container.Controls.Add(lblCartMessage);
                container.Controls.Add(new LiteralControl("        </div>"));


            }
        }
        #endregion
    }

    #endregion

    #region Class Default Footer Template

    /// <summary>
    /// default template for shoppingcart's footer
    /// </summary>
    internal sealed class DefaulFooterTemplate : ITemplate
    {

        #region ITemplate Members
        /// <summary>
        /// Defines the control object where the child controls and templates belongs.
        /// </summary>
        /// <param name="container"></param>
        public void InstantiateIn(Control container)
        {
            container.Controls.Add(new LiteralControl("    <div class='cart-row'>"));
            container.Controls.Add(new LiteralControl("    </div>"));
        }
        #endregion
    }


    #endregion

    #region Class Custom Template
    /// <summary>
    /// template for shoppingcart's databound items
    /// </summary>
    [ToolboxItem(false)]
    public class CustomTemplate : WebControl, INamingContainer, IDataItemContainer
    {
        #region IDataItemContainer Members

        /// <summary>
        /// Gets the HtmlTextWriterTag value that corresponds to this Web server control. This property is used primarily by control developers.(Inherited from WebControl.)
        /// </summary>
        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Div;
            }
        }

        object IDataItemContainer.DataItem
        { get { throw new NotImplementedException(); } }

        int IDataItemContainer.DataItemIndex
        { get { throw new NotImplementedException(); } }

        int IDataItemContainer.DisplayIndex
        { get { throw new NotImplementedException(); } }

        #endregion
    }
    #endregion

}
