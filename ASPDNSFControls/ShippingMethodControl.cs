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
using AspDotNetStorefrontCore.ShippingCalculation;
using System.Globalization;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontControls
{
    /// <summary>
    /// Provides a control that display the Shipping Method List in a radio button list.
    /// </summary>
    [ToolboxData("<{0}:ShippingMethodControl runat=server></{0}:ShippingMethodControl>"),
    Designer(typeof(ShippingMethodControlDesigner))]
    public class ShippingMethodControl : CompositeControl
    {
        #region constant variables
        private const string SETTINGS_CATEGORY = "ASPDNSF Settings";
        private const string PROPERTY_SELECTEDITEM = "Gets the selected item.";
        private const string PROPERTY_SELECTEDINDEX = "Gets or sets the selected index of the control.";
        private const string PROPERTY_DATASOURCE = "Gets the shipping methods Listitemcollection of the control.";
        private const string PROPERTY_DATAVALUEFIELD = "Gets or ets the data value field member";
        private const string PROPERTY_DATATEXTFIELD = "Gets or sets the data text field member";
        private const string PROPERTY_SHOWCONTROL = "Gets or sets a value that indicates whether the control is redered on the page.";
        private const string PROPERTY_CONTROLHEADER = "The string resource value to be displayed at the header of the control";
        private const string PROPERTY_ERRORMESSAGE = "Get or sets Error text message";
        #endregion

        #region Controls constant value
        private const string DEFAULT_DESIGNERTEXT = "UNBOUND Shipping Method Control List";
        #endregion

        private string DEFAULT_NOSHIPPINGDEFINEDTEXT = string.Empty;

        #region Web Controls Instantiation
        private Label lblHeader = new Label();
        private RadioButtonList rblShippingMethods = new RadioButtonList();
        private Label lblErrorMessage = new Label();
        private Label lblDesignerMessage = new Label();
        #endregion

        #region overridden methods
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
            Controls.Add(AddLiteral("<div class='form shipping-methods-form'>"));

            //ERROR MESSAGE
            if (ErrorMessage != string.Empty)
            {
                Controls.Add(AddLiteral("  <div class='error-wrap'><div class='error-large'>"));
                lblErrorMessage.Text = ErrorMessage;
                Controls.Add(lblErrorMessage);
                Controls.Add(AddLiteral(" </div></div>"));
            }

            //HEADER
            //Controls.Add(AddLiteral("  <div class='group-header form-header shipping-header'>"));                        
            //lblHeader.Text = HeaderText;
            //Controls.Add(lblHeader);
            //Controls.Add(AddLiteral("  </div>"));

            //SHIPPINGMETHODLIST
            Controls.Add(AddLiteral("  <div class='form-group'>"));
            if (rblShippingMethods.Items.Count > 0)
            {
                if (rblShippingMethods.SelectedIndex == -1)
                {
                    rblShippingMethods.SelectedIndex = 0;
                }
                Controls.Add(rblShippingMethods);
            }

            //DESIGNER TEXT            

            if (rblShippingMethods.Items.Count == 0 && HeaderText == string.Empty)
            {
                if (!this.DesignMode)
                {
                    DEFAULT_NOSHIPPINGDEFINEDTEXT = AppLogic.GetString("checkoutshipping.aspx.12", Customer.Current.SkinID, Customer.Current.LocaleSetting);
                }

                rblShippingMethods.Items.Add(new ListItem(this.DesignMode ? DEFAULT_DESIGNERTEXT : DEFAULT_NOSHIPPINGDEFINEDTEXT));
                Controls.Add(rblShippingMethods);
            }

            if (rblShippingMethods.Items.Count == 1 &&
                rblShippingMethods.SelectedItem.Text.Contains(
                    AppLogic.GetString("checkoutshipping.aspx.12", Customer.Current.SkinID, Customer.Current.LocaleSetting)))
            {
                rblShippingMethods.SelectedItem.Text = AppLogic.GetString("checkoutshipping.aspx.12", Customer.Current.SkinID, Customer.Current.LocaleSetting);
                Controls.Add(AddLiteral("<a id=\"ctl00_PageContent_lnkChangeShipping\" class=\"underline-link\" href=\"javascript:self.location='JWMyAddresses.aspx?Checkout=True&amp;AddressType=2&amp;returnURL=checkoutshipping.aspx%dontupdateid%3dTrue'\">Change Shipping Address</a>"));
                
            }

            Controls.Add(AddLiteral("  </div>"));
            Controls.Add(AddLiteral("</div>"));
        }
        #endregion

        #region Methods
        /// <summary>
        /// put html tag into a literal control
        /// </summary>
        /// <param name="htmltags">html tags</param>
        /// <returns>returns literal control</returns>
        private LiteralControl AddLiteral(string htmltags)
        {
            return new LiteralControl(htmltags);
        }

        /// <summary>
        /// Initialize default values for the controls
        /// </summary>
        private void IntializeControlsDefaultValues()
        {
            //RADIOBUTTONLIST
            rblShippingMethods.ID = "ctrlShippingMethods";

            //HEADER
            lblHeader.ID = "lblHeader";

            //DESIGNER TEXT
            lblDesignerMessage.ID = "lblDesignerMessage";
            lblDesignerMessage.Text = DEFAULT_DESIGNERTEXT;

            //ERROR MESSAGE
            lblErrorMessage.ID = "lblErrorMessage";
            lblErrorMessage.Text = string.Empty;
        }

        /// <summary>
        /// Adds the datasource items into the shipping method radio button list
        /// </summary>
        public void BindData()
        {
            if (this.DataSource != null)
            {
                bool isRealTimeShipping = this.DataSource.Exists(i => i.IsRealTime);

                if (isRealTimeShipping)
                {
                    rblShippingMethods.DataSource = null;
                    rblShippingMethods.Items.Clear();

                    foreach (ShippingMethod method in this.DataSource)
                    {
                        ListItem liMethod = new ListItem() { Text = method.DisplayFormat };
                        liMethod.Value = Shipping.GetFormattedRealTimeShippingMethodForSelectList(method);

                        rblShippingMethods.Items.Add(liMethod);
                    }
                }
                else
                {
                    rblShippingMethods.DataSource = this.DataSource;
                    rblShippingMethods.DataTextField = DataTextField;
                    rblShippingMethods.DataValueField = DataValueField;
                    rblShippingMethods.DataBind();
                }

                if (rblShippingMethods.Items.Count == 1)
                {
                    rblShippingMethods.SelectedIndex = 0;
                }
            }

        }
        #endregion

        #region Controls Properties

        ShippingMethodCollection _dataSource = new ShippingMethodCollection();

        /// <summary>
        /// The Datasource of the control
        /// </summary>
        [Browsable(true), Category(SETTINGS_CATEGORY), Description(PROPERTY_DATASOURCE)]
        public ShippingMethodCollection DataSource
        {
            get
            {
                return _dataSource;
            }
            set
            {
                _dataSource = value;
                BindData();

                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// The data text to be displayed on the shipping method radio button list
        /// </summary>
        [Browsable(true), Category(SETTINGS_CATEGORY),
        Description(PROPERTY_DATATEXTFIELD),
        DefaultValue("DisplayFormat")]
        public string DataTextField
        {
            get
            {
                if ((object)ViewState["DisplayFormat"] == null)
                {
                    return "DisplayFormat";
                }
                return ViewState["DisplayFormat"].ToString();
            }
            set
            {
                ViewState["DisplayFormat"] = value;
                rblShippingMethods.DataTextField = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// The data value of the shipping method radio button list
        /// </summary>
        [Browsable(true), Category(SETTINGS_CATEGORY),
        Description(PROPERTY_DATAVALUEFIELD),
        DefaultValue("ID")]
        public string DataValueField
        {
            get
            {
                if ((object)ViewState["ID"] == null)
                {
                    return "ID";
                }
                return ViewState["ID"].ToString();
            }
            set
            {
                ViewState["ID"] = value;
                rblShippingMethods.DataValueField = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// The selected item in the shipping method radio button list
        /// </summary>
        [Browsable(false), Category(SETTINGS_CATEGORY), Description(PROPERTY_DATASOURCE)]
        public ListItem SelectedItem
        {
            get
            {
                EnsureChildControls();
                return rblShippingMethods.SelectedItem;
            }
            set { }
        }

        /// <summary>
        /// The index of the selected item in the shipping method radio button list
        /// </summary>
        [Browsable(false), Category(SETTINGS_CATEGORY), Description(PROPERTY_SELECTEDINDEX)]
        public int SelectedIndex
        {
            get
            {
                EnsureChildControls();
                return rblShippingMethods.SelectedIndex;
            }
            set
            {
                ViewState["SelectedIndex"] = value;
                rblShippingMethods.SelectedIndex = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// The display text at the top of the shipping method radio button list
        /// </summary>
        [Browsable(true), Category(SETTINGS_CATEGORY), Description(PROPERTY_CONTROLHEADER)]
        public String HeaderText
        {
            get
            {
                if ((object)ViewState["HeaderText"] == null)
                {
                    return string.Empty;
                }
                return ViewState["HeaderText"].ToString();
            }
            set
            {
                ViewState["HeaderText"] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// The Error message that will be displayed at the top of the shipping method list
        /// </summary>
        [Browsable(false), Category(SETTINGS_CATEGORY), Description(PROPERTY_ERRORMESSAGE)]
        public String ErrorMessage
        {
            get
            {
                if ((object)ViewState["ErrorMessage"] == null)
                {
                    return string.Empty;
                }
                return ViewState["ErrorMessage"].ToString();
            }
            set
            {
                ViewState["ErrorMessage"] = value;
                ChildControlsCreated = false;
            }
        }

        #endregion
    }
}
