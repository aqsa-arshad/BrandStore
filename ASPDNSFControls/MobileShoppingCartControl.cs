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
    /// <summary>
    /// Provides a control that display the Shopping cart line items with quantity and price.
    /// </summary>
    [ToolboxData("<{0}:MobileShoppingCartControl runat=server></{0}:ShoppingCartControl>"),
    Designer(typeof(ShoppingControlDesigner))]
    public class MobileShoppingCartControl : CompositeControl
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
        private Label lblDeleteHeader = new Label();

        Button btnMiniCartUpdate = new Button();
        #endregion

        #region Private Properties
        private MobileShoppingCartItemCollection _items = new MobileShoppingCartItemCollection();
        #endregion

        MobileShoppingCartControlItem item = null;

        /// <summary>
        /// Gets or sets the querystring name for the search keyword
        /// </summary>
        [Browsable(true), Category("Appearance"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string StaticHeaderSeparatorImageUrl
        {
            get { return null == ViewState[STATIC_HEADER_SEPARATOR_IMAGEURL] ? string.Empty : (string)ViewState[STATIC_HEADER_SEPARATOR_IMAGEURL]; }
            set
            {
                ViewState[STATIC_HEADER_SEPARATOR_IMAGEURL] = value;
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


            //outer box
            string style = string.Empty;
            style = "shopping_cart";

            Controls.Add(new LiteralControl("<table style='width: 100%' cellpadding='0' cellspacing='0' border='0'>"));
            Controls.Add(new LiteralControl("  <tr>"));

            Controls.Add(new LiteralControl("    <td class=\"" + style + "\" >"));


            //if the page developer does not explicitly add a <HeaderTemplate> tag 
            //in ShoppingCartControl's declarative syntax     
            if (HeaderTemplate == null)
            {
                if (this.DesignMode)
                {
                    ITemplate defaultTemplate = new MobileDefaultHeaderTemplate("Product", "Quantity", "SubTotal", "Delete");
                    defaultTemplate.InstantiateIn(this);
                }
                else
                {
                    if (this.DataSource != null)
                    {
                        CartItemCollection cic = this.DataSource;
                        if (cic.Count != 0)
                        {
                            ITemplate defaultTemplate = new MobileDefaultHeaderTemplate("Product", "Quantity", "SubTotal", "Delete");
                            defaultTemplate.InstantiateIn(this);
                        }
                        else
                        {
                            Topic emptyCartTopic = new Topic(this.EmptyCartTopicName);
                            ITemplate defaultTemplate = new MobileDefaultHeaderTemplate(emptyCartTopic.Contents);
                            defaultTemplate.InstantiateIn(this);
                        }
                    }
                    else
                    {
                        ITemplate defaultTemplate = new MobileDefaultHeaderTemplate("Product", "Quantity", "SubTotal", "Delete");
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

                        Controls.Add(new LiteralControl("      <table style='width : 100%;' cellpadding='0' cellspacing='0' border='0'>"));
                        Controls.Add(new LiteralControl("        <tr>"));
                        Controls.Add(new LiteralControl("          <td>"));
                        Controls.Add(headerTemplateItem);
                        Controls.Add(new LiteralControl("          </td>"));
                        Controls.Add(new LiteralControl("        </tr>"));
                        Controls.Add(new LiteralControl("      </table>"));
                    }
                    else
                    {
                        Topic emptyCartTopic = new Topic(this.EmptyCartTopicName);
                        ITemplate defaultTemplate = new MobileDefaultHeaderTemplate(emptyCartTopic.Contents);
                        defaultTemplate.InstantiateIn(this);
                    }
                }
                else
                {
                    CustomTemplate headerTemplateItem = new CustomTemplate();
                    headerTemplateItem.ID = "headerTemplateItem";
                    HeaderTemplate.InstantiateIn(headerTemplateItem);

                    Controls.Add(new LiteralControl("      <table style='width : 100%;' cellpadding='0' cellspacing='0' border='0'>"));
                    Controls.Add(new LiteralControl("        <tr>"));
                    Controls.Add(new LiteralControl("          <td>"));
                    Controls.Add(headerTemplateItem);
                    Controls.Add(new LiteralControl("          </td>"));
                    Controls.Add(new LiteralControl("        </tr>"));
                    Controls.Add(new LiteralControl("      </table>"));
                }
            }

            ShoppingCart cart = null;
            
            //ITEMS Template
            if (DataSource != null)
            {
                Items.Clear();
                int alternateItem = 0;
                CartItemCollection sortedCartItemForMinicart = new CartItemCollection();
                sortedCartItemForMinicart = this.DataSource;
                

                Panel pnlItems = new Panel();
                pnlItems.ID = "pnlCartItems";
                pnlItems.CssClass = "cart_items";

                foreach (CartItem cItem in sortedCartItemForMinicart)
                {
                    cart = cItem.ThisShoppingCart;
                    if (alternateItem > 0)
                    {
                        // alternating separator
                        pnlItems.Controls.Add(new LiteralControl("<div style='border-top: solid 1px #444444'></div>"));
                    }

                    item = new MobileShoppingCartControlItem(this.LineItemSettings);

                    item.CartItem = cItem;
                    item.AllowEdit = this.AllowEdit;
                    item.DisplayMode = this.DisplayMode;
                    if (cItem.ThisShoppingCart.CartType == CartTypeEnum.Deleted)
                    {
                        item.IsDeleted = true;
                    }
                    Items.Add(item);
                    pnlItems.Controls.Add(item);

                    alternateItem++;
                }

                Controls.Add(pnlItems);
            }

            if (DesignMode)
            {

                Controls.Add(new LiteralControl("<table style='width: 100%;' cellpadding='0' cellspacing='0' border='0'>"));

                Controls.Add(new LiteralControl("  <tr>"));

                //dummy line item description
                Controls.Add(new LiteralControl("    <td style='width:60%' valign='top'>"));
                Controls.Add(new LiteralControl("      <table style='width: 100%;' cellpadding='0' cellspacing='0' border='0'>"));

                Controls.Add(new LiteralControl("        <tr>"));
                Controls.Add(new LiteralControl("          <td>Product Name : Unbound</td>"));
                Controls.Add(new LiteralControl("        </tr>"));

                Controls.Add(new LiteralControl("        <tr>"));
                Controls.Add(new LiteralControl("          <td>SKU : Unbound</td>"));
                Controls.Add(new LiteralControl("        </tr>"));

                Controls.Add(new LiteralControl("        <tr>"));
                Controls.Add(new LiteralControl("          <td>Size : Unbound</td>"));
                Controls.Add(new LiteralControl("        </tr>"));

                Controls.Add(new LiteralControl("        <tr>"));
                Controls.Add(new LiteralControl("          <td>Color : Unbound</td>"));
                Controls.Add(new LiteralControl("        </tr>"));

                Controls.Add(new LiteralControl("      </table>"));
                Controls.Add(new LiteralControl("    </td>"));

                //dummy quantity & delete button
                TextBox txtQuantity = new TextBox();
                txtQuantity.Text = "Unbound";

                LinkButton lnkDelete = new LinkButton();
                lnkDelete.Text = "shoppingcart.cs.107".StringResource();

                Controls.Add(new LiteralControl("    <td style='width:15%' valign='top'>"));
                Controls.Add(txtQuantity);
                Controls.Add(lnkDelete);
                Controls.Add(new LiteralControl("    </td>"));

                //dummy extended price
                Controls.Add(new LiteralControl("    <td style='width:25%;' align='right' valign='top'>Unbound"));
                Controls.Add(new LiteralControl("    </td>"));

                Controls.Add(new LiteralControl("  </tr>"));


                Controls.Add(new LiteralControl("</table>"));


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
            Controls.Add(new LiteralControl("    </td>"));
            Controls.Add(new LiteralControl("  </tr>"));
            Controls.Add(new LiteralControl("</table>"));
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
            lblProductHeader.CssClass = "shoppingcartheader";

            //Quantity Header
            lblQuantityHeader.ID = "lblQuantityHeader";
            lblQuantityHeader.Text = QuantityHeaderText;
            lblQuantityHeader.CssClass = "shoppingcartheader";

            //Subtotal Header
            lblSubtotalHeader.ID = "lblSubtotalHeader";
            lblSubtotalHeader.Text = SubTotalHeaderText;
            lblSubtotalHeader.CssClass = "shoppingcartheader";

            //Delete Header
            lblDeleteHeader.ID = "lblDeleteHeader";
            lblDeleteHeader.Text = DeleteHeaderText;
            lblDeleteHeader.CssClass = "shoppingcartheader";

        }
        #endregion

        #region Controls Properties

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
        /// Gets or sets the Deleted Header
        /// </summary>
        [Browsable(true),
        Bindable(true),
        Category(SETTINGS_CATEGORY),
        Description("Delete Header Text"),
        DefaultValue("Delete")]
        public String DeleteHeaderText
        {
            get
            {
                if ((object)ViewState["DeleteHeaderText"] == null)
                {
                    return string.Empty;
                }
                return ViewState["DeleteHeaderText"].ToString();
            }
            set
            {
                ViewState["DeleteHeaderText"] = value;
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

        private bool RenderAsWishList()
        {
            return this.DisplayMode == CartDisplayMode.WishList;
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
        TemplateContainer(typeof(MobileShoppingCartControlItem)),
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
        TemplateContainer(typeof(MobileShoppingCartControlItem)),
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
        TemplateContainer(typeof(MobileShoppingCartControlItem)),
        PersistenceMode(PersistenceMode.InnerProperty)]
        public ITemplate FooterTemplate
        {
            get { return m_footertemplate; }
            set { m_footertemplate = value; }
        }

        /// <summary>
        /// Gets the ShoppingCart item collection
        /// </summary>
        public MobileShoppingCartItemCollection Items
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

        #endregion
    }

    #region Class for Item Collection
    /// <summary>
    /// Collection of ShoppingCartControlItem
    /// </summary>
    public class MobileShoppingCartItemCollection : List<MobileShoppingCartControlItem> { }
    #endregion

    #region Class for Data Items
    /// <summary>
    /// Defines the databound items for the shoppingcart control
    /// </summary>
    [ToolboxItem(false)]
    public class MobileShoppingCartControlItem : CompositeControl, INamingContainer, IDataItemContainer
    {
        #region Constructors
        public MobileShoppingCartControlItem()
        {
            AllowEdit = true;
        }

        public MobileShoppingCartControlItem(LineItemSettings _lineItemSettings)
        {
            LineItemSetting = _lineItemSettings;
        }
        #endregion

        #region Controls Declaration

        ITextControl txtQuantity = null;
        Button btnDelete = new Button();
        DropDownList cboQuantity = new DropDownList();
        Label lblSubTotal = new Label();
        Label lblVatDisplay = new Label();
        Label lblQtyDiscount = new Label();
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

        #region IDataItemContainer Members

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

        #endregion

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
                    (txtQuantity as TextBox).Columns = 4;
                    (txtQuantity as TextBox).MaxLength = 4;
                    (txtQuantity as TextBox).ReadOnly = false;
                    (txtQuantity as TextBox).CssClass = "mobileinput";
                }

                //Line Item Description
                if (LineItemSetting != null)
                {
                    lineItemDescription.LineItemDescriptionSettings = LineItemSetting;
                }

                lineItemDescription.DataSource = this.CartItem;
                lineItemDescription.AllowEdit = this.AllowEdit;
                lineItemDescription.IsMiniCart = false;

                lblSubTotal.Text = cItem.ExtPriceRateDisplayFormat.ToString();
                lblVatDisplay.Text = cItem.VatRateDisplayFormat;
                lblQtyDiscount.Text = cItem.LineItemQuantityDiscount;

                lblSubTotal.CssClass = "shoppingcartproductprice";
                lblVatDisplay.CssClass = "shoppingcartproductprice";
                lblQtyDiscount.CssClass = "shoppingcartproductprice";

                btnDelete.CssClass = "shoppingcartdeletebutton";
                btnDelete.CommandName = "Delete";
                btnDelete.CommandArgument = cItem.ShoppingCartRecordID.ToString();

                Controls.Add(new LiteralControl("      <table style='width : 100%; padding-top: 10px; padding-bottom: 10px' cellpadding='0' cellspacing='0' border='0'>"));
                Controls.Add(new LiteralControl("        <tr>"));

                if (this.DisplayMode != CartDisplayMode.MiniCart)
                {
                    //LineItemDescription
                    Controls.Add(new LiteralControl("          <td style='width: 50%;' valign='top'>"));
                    Controls.Add(lineItemDescription);
                    Controls.Add(new LiteralControl("          </td>"));

                    Controls.Add(new LiteralControl("          <td style='width: 15%;' align='left' valign='top'>"));
                    Controls.Add(new LiteralControl("            <div>"));
                    
                    //check for restricted quantity
                    if (this.CartItem.RestrictedQuantities.Count == 0 || !this.AllowEdit)
                    {
                        Controls.Add(txtQuantity as Control);
                    }
                    else
                    {
                        // build dropdown for restricted quantities                    
                        BuildRestrictedQuantities();
                        Controls.Add(cboQuantity);
                    }


                    Controls.Add(new LiteralControl("            </div>"));

                    Controls.Add(new LiteralControl("          </td>"));
                }

                Controls.Add(new LiteralControl("          <td style='width: 25%; padding-right:5px;' align='right' valign='top'>"));
                Controls.Add(new LiteralControl("            <table style='width:100% padding-right:5px;' cellpadding='0' cellspacing='0' border='0'>"));
                Controls.Add(new LiteralControl("              <tr>"));

                Controls.Add(new LiteralControl("                <td style='padding-top:5px; padding-right:5px;' align='right' valign='top'>"));


                Controls.Add(lblSubTotal);
                

                Controls.Add(new LiteralControl("                </td>"));
                Controls.Add(new LiteralControl("              </tr>"));

                if (this.DisplayMode != CartDisplayMode.MiniCart)
                {
                    if (cItem.ThisShoppingCart.VatEnabled)
                    {
                        Controls.Add(new LiteralControl("               <tr>"));
                        Controls.Add(new LiteralControl("                <td align='right'>"));
                        Controls.Add(lblVatDisplay);
                        Controls.Add(new LiteralControl("                </td>"));
                        Controls.Add(new LiteralControl("              </tr>"));
                    }

                    if (QuantityDiscount.CustomerLevelAllowsQuantityDiscounts(cItem.ThisCustomer.CustomerLevelID))
                    {
                        Controls.Add(new LiteralControl("               <tr>"));
                        Controls.Add(new LiteralControl("                <td align='right'>"));
                        Controls.Add(lblQtyDiscount);
                        Controls.Add(new LiteralControl("                </td>"));
                        Controls.Add(new LiteralControl("              </tr>"));
                    }
                }

                Controls.Add(new LiteralControl("            </table>"));

                Controls.Add(new LiteralControl("          </td>"));

                // Delete column

                Controls.Add(new LiteralControl("          <td style='width: 10%; padding-top:5px;' valign='top' align='right'>"));
                Controls.Add(btnDelete);
                Controls.Add(new LiteralControl("          </td>"));

                Controls.Add(new LiteralControl("        </tr>"));
                Controls.Add(new LiteralControl("      </table>"));
            }
        }

        #endregion

        #region Methods
        private void BuildRestrictedQuantities()
        {
            string DELETE_QTY = "DELETE";
            List<int> quantities = cItem.RestrictedQuantities;
            string defaultSelectedValue = DELETE_QTY;

            cboQuantity.Items.Add(new ListItem(DELETE_QTY, "0"));
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

        private MinicartSummarySettings m_minicartsummarysetting;

        /// <summary>
        /// Gets or sets the shopping cart line item settings.
        /// </summary>
        public MinicartSummarySettings MinicartSummarySetting
        {
            get { return m_minicartsummarysetting; }
            set { m_minicartsummarysetting = value; }
        }
    }
    #endregion

    /// <summary>
    /// Default Template for ShoppingCart's Header
    /// </summary>
    internal sealed class MobileDefaultHeaderTemplate : ITemplate
    {
        private string m_mobileproductheadertext;

        /// <summary>
        /// Gets or sets the product header text
        /// </summary>
        public string MobileProductHeaderText
        {
            get { return m_mobileproductheadertext; }
            set { m_mobileproductheadertext = value; }
        }

        private string m_mobilequantityheadertext;

        /// <summary>
        /// Gets or sets the quantity header text
        /// </summary>
        public string MobileQuantityHeaderText
        {
            get { return m_mobilequantityheadertext; }
            set { m_mobilequantityheadertext = value; }
        }

        private string m_mobilesubtotalheadertext;

        /// <summary>
        /// Gets or sets the Subtotal header text
        /// </summary>
        public string MobileSubTotalHeaderText
        {
            get { return m_mobilesubtotalheadertext; }
            set { m_mobilesubtotalheadertext = value; }
        }

        private string m_mobiledeleteheadertext;

        /// <summary>
        /// Gets or sets the Delete header text
        /// </summary>
        public string MobileDeleteHeaderText
        {
            get { return m_mobiledeleteheadertext; }
            set { m_mobiledeleteheadertext = value; }
        }

        private string m_mobilecartmessage;

        /// <summary>
        /// Gets or sets the cart notification message
        /// </summary>
        public string MobileCartMessage
        {
            get { return m_mobilecartmessage; }
            set { m_mobilecartmessage = value; }
        }

        #region Constructors
        public MobileDefaultHeaderTemplate() { }

        public MobileDefaultHeaderTemplate(string cartMessage)
        {
            MobileCartMessage = cartMessage;
        }

        public MobileDefaultHeaderTemplate(string prodHeaderText, string qtyHeaderText, string subTotHeaderText, string delHeaderText)
        {
            MobileProductHeaderText = prodHeaderText;
            MobileQuantityHeaderText = qtyHeaderText;
            MobileSubTotalHeaderText = subTotHeaderText;
            MobileDeleteHeaderText = delHeaderText;
        }
        #endregion

        #region ITemplate Members
        /// <summary>
        /// Defines the control object where the child controls and templates belongs.
        /// </summary>
        /// <param name="container"></param>
        public void InstantiateIn(Control container)
        {
            if (MobileCartMessage == string.Empty || MobileCartMessage == null)
            {
                Label lblProductHeader = new Label();
                lblProductHeader.ID = "lblProductHeader";
                lblProductHeader.Text = MobileProductHeaderText;
                lblProductHeader.CssClass = "shoppingcartheader";

                Label lblQuantityHeader = new Label();
                lblQuantityHeader.ID = "lblQuantityHeader";
                lblQuantityHeader.Text = MobileQuantityHeaderText;
                lblQuantityHeader.CssClass = "shoppingcartheader";

                Label lblSubtotalHeader = new Label();
                lblSubtotalHeader.ID = "lblSubtotalHeader";
                lblSubtotalHeader.Text = MobileSubTotalHeaderText;
                lblSubtotalHeader.CssClass = "shoppingcartheader";

                Label lblDeleteHeader = new Label();
                lblDeleteHeader.ID = "lblDeleteHeader";
                lblDeleteHeader.Text = MobileDeleteHeaderText;
                lblDeleteHeader.CssClass = "shoppingcartheader";

                container.Controls.Add(new LiteralControl("      <table style='width : 100%; border-bottom: solid 1px #444444' cellpadding='0' cellspacing='0' border='0'>"));
                container.Controls.Add(new LiteralControl("        <tr>"));

                container.Controls.Add(new LiteralControl("          <td style='width: 50%; padding-bottom:5px; font-weight:bold'>"));
                container.Controls.Add(lblProductHeader);
                container.Controls.Add(new LiteralControl("          </td>"));

                container.Controls.Add(new LiteralControl("          <td style='width: 15%; padding-bottom:5px; font-weight:bold' align='left'>"));
                container.Controls.Add(lblQuantityHeader);
                container.Controls.Add(new LiteralControl("          </td>"));

                container.Controls.Add(new LiteralControl("          <td style='width: 25%; padding-bottom:5px; padding-right:10px; font-weight:bold' align='right'>"));
                container.Controls.Add(lblSubtotalHeader);           
                container.Controls.Add(new LiteralControl("          </td>"));

                container.Controls.Add(new LiteralControl("          <td style='width: 10%; padding-bottom:5px; font-weight:bold' align='right'>"));
                container.Controls.Add(lblDeleteHeader);
                container.Controls.Add(new LiteralControl("          </td>"));

                container.Controls.Add(new LiteralControl("        </tr>"));
                container.Controls.Add(new LiteralControl("      </table>"));
            }
            else
            {
                Literal lblCartMessage = new Literal();
                lblCartMessage.ID = "lblCartMessage";
                lblCartMessage.Text = MobileCartMessage;

                container.Controls.Add(new LiteralControl("      <table style='width : 100%;' cellpadding='0' cellspacing='0' border='0'>"));
                container.Controls.Add(new LiteralControl("        <tr>"));

                container.Controls.Add(new LiteralControl("          <td style='width: 100%; padding-top: 20px; padding-bottom: 20px'>"));
                container.Controls.Add(lblCartMessage);
                container.Controls.Add(new LiteralControl("          </td>"));

                container.Controls.Add(new LiteralControl("        </tr>"));
                container.Controls.Add(new LiteralControl("      </table>"));
            }
        }
        #endregion
    }
}
