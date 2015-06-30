// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontCore.Validation;

namespace AspDotNetStorefront
{
    public partial class checkoutshippingmult : SkinBase
    {
        #region Variable Declaration

        private ShoppingCart cart = null;
        private List<Address> _customerShipToAddresses = new List<Address>();

        #endregion

        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.Expires = -1;
            Response.AddHeader("pragma", "no-cache");
            Response.AddHeader("Last-Modified", DateTime.Now.AddMinutes(-10).ToUniversalTime() + " GMT" );
            Response.AddHeader("Cache-Control", "no-store, no-cache, must-revalidate"); // HTTP/1.1
            Response.AddHeader("Cache-Control", "post-check=0, pre-check=0");
            Response.AddHeader("Pragma", "no-cache"); // HTTP/1.0
            ErrorMessage err;
            if (AppLogic.AppConfigBool("RequireOver13Checked") && !ThisCustomer.IsOver13)
            {
                err = new ErrorMessage(Server.HtmlEncode(AppLogic.GetString("checkout.over13required", ThisCustomer.SkinID, ThisCustomer.LocaleSetting)));
                Response.Redirect("shoppingcart.aspx?errormsg=" + err.MessageId);
            }

            RequireSecurePage();

            // -----------------------------------------------------------------------------------------------
            // NOTE ON PAGE LOAD LOGIC:
            // We are checking here for required elements to allowing the customer to stay on this page.
            // Many of these checks may be redundant, and they DO add a bit of overhead in terms of db calls, but ANYTHING really
            // could have changed since the customer was on the last page. Remember, the web is completely stateless. Assume this
            // page was executed by ANYONE at ANYTIME (even someone trying to break the cart). 
            // It could have been yesterday, or 1 second ago, and other customers could have purchased limitied inventory products, 
            // coupons may no longer be valid, etc, etc, etc...
            // -----------------------------------------------------------------------------------------------
            ThisCustomer.RequireCustomerRecord();

            if (!ThisCustomer.IsRegistered)
            {
                bool boolAllowAnon = (AppLogic.AppConfigBool("PasswordIsOptionalDuringCheckout") && !cart.HasRecurringComponents());
                if (!boolAllowAnon && ThisCustomer.PrimaryBillingAddressID > 0)
                {
                    Address BillingAddress = new Address();
                    BillingAddress.LoadByCustomer(ThisCustomer.CustomerID, ThisCustomer.PrimaryBillingAddressID, AddressTypes.Billing);
                    if (BillingAddress.PaymentMethodLastUsed == AppLogic.ro_PMPayPalExpress || BillingAddress.PaymentMethodLastUsed == AppLogic.ro_PMPayPalExpressMark)
                    {
                        boolAllowAnon = AppLogic.AppConfigBool("PayPal.Express.AllowAnonCheckout");
                    }
                }

                if (!boolAllowAnon)
                {
                    Response.Redirect("createaccount.aspx?checkout=true");
                }
            }
            if (ThisCustomer.PrimaryBillingAddressID == 0 || ThisCustomer.PrimaryShippingAddressID == 0)
            {
                err = new ErrorMessage(Server.HtmlEncode(AppLogic.GetString("checkoutpayment.aspx.2", SkinID, ThisCustomer.LocaleSetting))); //checkout not allowed without primary shipping/billing addy
                Response.Redirect("shoppingcart.aspx?resetlinkback=1&errormsg=" + err.MessageId);
            }

            SectionTitle = AppLogic.GetString("checkoutshippingmult.aspx.1", SkinID, ThisCustomer.LocaleSetting); //shipping options

            cart.ValidProceedCheckout(); // will not come back from this if any issue. they are sent back to the cart page!

			GatewayCheckoutByAmazon.CheckoutByAmazon checkoutByAmazon = new GatewayCheckoutByAmazon.CheckoutByAmazon();
            if (cart.IsAllDownloadComponents() || !Shipping.MultiShipEnabled() || cart.TotalQuantity() > AppLogic.MultiShipMaxNumItemsAllowed() || !cart.CartAllowsShippingMethodSelection || checkoutByAmazon.IsCheckingOut)
            {
                // not allowed then:
                err = new ErrorMessage(Server.HtmlEncode(AppLogic.GetString("checkoutshippingmult.aspx.12", SkinID, ThisCustomer.LocaleSetting)));
                Response.Redirect("shoppingcart.aspx?resetlinkback=1&errormsg=" + err.MessageId);
            }
			
            CartItem FirstCartItem = (CartItem)cart.CartItems[0];
            Address FirstItemShippingAddress = new Address();
            FirstItemShippingAddress.LoadByCustomer(ThisCustomer.CustomerID, FirstCartItem.ShippingAddressID, AddressTypes.Shipping);
            if (FirstItemShippingAddress.AddressID == 0)
            {
                // not allowed here anymore!
                err = new ErrorMessage(Server.HtmlEncode(AppLogic.GetString("checkoutshippingmult.aspx.10", SkinID, ThisCustomer.LocaleSetting)));
                Response.Redirect("shoppingcart.aspx?errormsg=" + err.MessageId);
            }

            if (!IsPostBack && CommonLogic.FormCanBeDangerousContent("update") == "" && CommonLogic.FormCanBeDangerousContent("continue") == "" && CommonLogic.QueryStringCanBeDangerousContent("setallprimary") == "")
            {
                UpdatepageContent();
            }

            if (CommonLogic.FormCanBeDangerousContent("update") != "" || CommonLogic.FormCanBeDangerousContent("continue") != "" || CommonLogic.QueryStringCanBeDangerousContent("setallprimary") != "")
            {
                ProcessCart();
            }
            JSPopupRoutines.Text = AppLogic.GetJSPopupRoutines();

            AppLogic.eventHandler("CheckoutShipping").CallEvent("&CheckoutShipping=true");
        }

        private void UpdatepageContent()
        {
            if (CommonLogic.QueryStringNativeInt("ErrorMsg") > 0)
            {
                ErrorMessage e = new ErrorMessage(CommonLogic.QueryStringNativeInt("ErrorMsg"));
                ErrorMsgLabel.Text = "<p align=\"left\"><span class=\"errorLg\">" + Server.HtmlEncode(e.Message) + "</span></p>";
                pnlErrorMsg.Visible = true;
            }
            else
            {
                pnlErrorMsg.Visible = false;
            }

            //write out header package is it exists
            String XmlPackageName = AppLogic.AppConfig("XmlPackage.CheckoutShippingMultPageHeader");
            if (XmlPackageName.Length != 0)
            {
                XmlPackage_CheckoutShippingPageHeader.Text = AppLogic.RunXmlPackage(XmlPackageName, base.GetParser, ThisCustomer, SkinID, String.Empty, String.Empty, true, true);
            }


            //
            if (!cart.ShippingIsFree && cart.MoreNeededToReachFreeShipping != 0.0M)
            {
                GetFreeShipping.Text = "<div class=\"FreeShippingThresholdPrompt\">";
                GetFreeShipping.Text += String.Format(AppLogic.GetString("checkoutshippingmult.aspx.2", SkinID, ThisCustomer.LocaleSetting), ThisCustomer.CurrencyString(cart.FreeShippingThreshold), CommonLogic.Capitalize(cart.FreeShippingMethod));
                GetFreeShipping.Text += " ";
                GetFreeShipping.Text += "</div>";
                pnlGetFreeShipping.Visible = true;
            }
            else
            {
                pnlGetFreeShipping.Visible = false;
            }

            checkoutshippingmultaspx16.Text = AppLogic.GetString("checkoutshippingmult.aspx.16", SkinID, ThisCustomer.LocaleSetting);
            checkoutshippingmultaspx18.Text = String.Format(AppLogic.GetString("checkoutshippingmult.aspx.18", SkinID, ThisCustomer.LocaleSetting), "account.aspx?checkout=true", "checkoutshippingmult.aspx?setallprimary=true");


            String XmlPackageName2 = AppLogic.AppConfig("XmlPackage.CheckoutShippingMultPageFooter");
            if (XmlPackageName2.Length != 0)
            {
                XmlPackage_CheckoutShippingMultPageFooter.Text = AppLogic.RunXmlPackage(XmlPackageName2, base.GetParser, ThisCustomer, SkinID, String.Empty, String.Empty, true, true);
            }

        }
        private void ProcessCart()
        {
            bool ContinueCheckout = (CommonLogic.FormCanBeDangerousContent("continue") != "");
            if (cart.IsEmpty())
            {
                Response.Redirect("shoppingcart.aspx");
            }

            if (CommonLogic.QueryStringBool("setallprimary"))
            {
                cart.ResetAllAddressToPrimaryShippingAddress();
                if (ContinueCheckout)
                {
                    Response.Redirect("checkoutshippingmult2.aspx");
                }
                else
                {
                    Response.Redirect("checkoutshippingmult.aspx");
                }
            }

            Hashtable NewAddresses = new Hashtable();
            Hashtable AddressIDs = new Hashtable();

            StringBuilder xmlDoc = new StringBuilder(4096);
            xmlDoc.Append("<root>");

            // add NEW address blocks, if necessary:
            foreach (CartItem c in cart.CartItems)
            {
                if (!c.IsDownload && c.Shippable && !GiftCard.s_IsEmailGiftCard(c.ProductID) && c.SKU != AppLogic.ro_PMMicropay)
                {
                    for (int i = 1; i <= c.Quantity; i++)
                    {
                        int ThisAddressID = 0;
                        String ThisID = c.ShoppingCartRecordID.ToString() + "_" + i.ToString();
                        String ShipToType = CommonLogic.FormCanBeDangerousContent("ShipToType_" + ThisID);
                        switch (ShipToType.ToUpperInvariant())
                        {
                            case "NEWADDRESS":
                                {
                                    Address addr = new Address();
                                    addr.CustomerID = ThisCustomer.CustomerID;
                                    addr.NickName = CommonLogic.FormCanBeDangerousContent("AddressNickName_" + ThisID);
                                    addr.FirstName = CommonLogic.FormCanBeDangerousContent("AddressFirstName_" + ThisID);
                                    addr.LastName = CommonLogic.FormCanBeDangerousContent("AddressLastName_" + ThisID);
                                    addr.Address1 = CommonLogic.FormCanBeDangerousContent("AddressAddress1_" + ThisID);
                                    addr.Address2 = CommonLogic.FormCanBeDangerousContent("AddressAddress2_" + ThisID);
                                    addr.Company = CommonLogic.FormCanBeDangerousContent("AddressCompany_" + ThisID);
                                    addr.Suite = CommonLogic.FormCanBeDangerousContent("AddressSuite_" + ThisID);
                                    addr.City = CommonLogic.FormCanBeDangerousContent("AddressCity_" + ThisID);
                                    addr.State = CommonLogic.FormCanBeDangerousContent("AddressState_" + ThisID);
                                    addr.Zip = CommonLogic.FormCanBeDangerousContent("AddressZip_" + ThisID);
                                    addr.Country = CommonLogic.FormCanBeDangerousContent("AddressCountry_" + ThisID);
                                    addr.Phone = CommonLogic.FormCanBeDangerousContent("AddressPhone_" + ThisID);

                                    // did we add this address already?
                                    if (NewAddresses.ContainsKey(addr.Address1))
                                    {
                                        ThisAddressID = System.Int32.Parse(NewAddresses[addr.Address1].ToString());
                                    }
                                    else
                                    {
                                        addr.AddressType = AddressTypes.Shipping;
                                        addr.InsertDB();
                                        NewAddresses.Add(addr.Address1, addr.AddressID.ToString());
                                        ThisAddressID = addr.AddressID;
                                    }
                                    break;
                                }
                            case "GIFTREGISTRYADDRESS":
                                {
                                    int GiftCustomerID = c.GiftRegistryForCustomerID;
                                    ThisAddressID = AppLogic.GiftRegistryShippingAddressID(GiftCustomerID);
                                    break;
                                }
                            case "EXISTINGADDRESS":
                            case "":
                                {
                                    ThisAddressID = CommonLogic.FormUSInt(ThisID);
                                    break;
                                }
                        }
                        if (ThisAddressID > 0)
                        {
                            xmlDoc.Append(String.Format("<row cartid=\"{0}\" addressid=\"{1}\" />", c.ShoppingCartRecordID.ToString(), ThisAddressID.ToString()));
                        }
                        else
                        {
                            UpdatepageContent();
                            ErrorMsgLabel.Text = AppLogic.GetString("checkoutshippingmult.aspx.27", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                            pnlErrorMsg.Visible = true;
                            return;
                        }
                    }
                }
            }
            xmlDoc.Append("</root>");

            cart.SetAddressesToXmlSpec(xmlDoc.ToString());

            if (!ContinueCheckout)
            {
                UpdatepageContent();
            }
            else
            {
                Response.Redirect("checkoutshippingmult2.aspx");
            }
        }
        #region OnInit        
        /// <summary>
        /// Overrides the OnInit method
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            cart = new ShoppingCart(SkinID, ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);
			
            LoadCustomerShipToAddresses();
            InitializeDataSource();
			
            base.OnInit(e);
        }
        #endregion

        #region LoadCustomerShipToAddresses
        /// <summary>
        /// Loads the Customer's shipping addresses
        /// </summary>
        private void LoadCustomerShipToAddresses()
        {
            Addresses allAddresses = new Addresses();
            allAddresses.LoadCustomer(ThisCustomer.CustomerID);
            
            int addressWithPOCount = 0;
            foreach (Address anyAddress in allAddresses)
            {
                if (!AppLogic.AppConfigBool("DisallowShippingToPOBoxes"))
                {
                    _customerShipToAddresses.Add(anyAddress);
                    //skip to next iteration in foreach
                    continue;
                }

				if (!(new POBoxAddressValidator()).IsValid(anyAddress))
                {
                    addressWithPOCount++;
                }
                //add address to dropdown if not a PO box
                else _customerShipToAddresses.Add(anyAddress);  
            }
            if (addressWithPOCount > 0)
            {
                //notify customer that some addresses are not shown because of POBox.
                //lblPOBoxError.Visible = true;
                //lblPOBoxError.Text = "createaccount_process.aspx.3".StringResource();
            }
                
        }

        #endregion

        #region ReLoadCustomerShipToAddresses        
        /// <summary>
        /// Reloads the shipping addresses
        /// </summary>
        private void ReLoadCustomerShipToAddresses()
        {
            _customerShipToAddresses.Clear();
            LoadCustomerShipToAddresses();
        }
        #endregion
        #region InitializeDataSource
        /// <summary>
        /// Initializes the ctrlCartItemAddresses Datasource
        /// </summary>
        private void InitializeDataSource()
        {
            ctrlCartItemAddresses.DataSource = cart.GetIndividualShippableCartItemsPerQuantity();
            ctrlCartItemAddresses.DataBind();
        }
        #endregion
        /// <summary>
        /// Toggles the visibility of controls in a particular container
        /// </summary>
        /// <param name="container"></param>
        /// <param name="controlID"></param>
        /// <param name="visible"></param>
        private void ShowHideContainedControl(Control container, string controlID, bool visible)
        {
            Control ctrl = container.FindControl(controlID);
            if (ctrl != null)
            {
                ctrl.Visible = visible;
            }
        }
        #region ctrlCartItemAddresses_ItemCommand
        /// <summary>
        /// Event Handler for the ItemCommand event of the Repeater Control
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        protected void ctrlCartItemAddresses_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            HtmlInputHidden addressOption = e.Item.FindControl("ShipAddressOption") as HtmlInputHidden;    
            if (e.CommandName == "UseExistingAddress")
            {
                ShowHideContainedControl(e.Item, "cboShipToAddress", true);

                ShowHideContainedControl(e.Item, "pnlAddNewAddress", false);
                ShowHideContainedControl(e.Item, "ctrlAddress", false);
                ShowHideContainedControl(e.Item, "btnSaveNewAddress", false);
                ShowHideContainedControl(e.Item, "lnkCancelAddNew", false);
                ShowHideContainedControl(e.Item, "lnkAddNewAddress", true);
                ShowHideContainedControl(e.Item, "lnkAddNewAddress", true);
                ShowHideContainedControl(e.Item, "lnkUseGiftRefistryAddress", true);
                ShowHideContainedControl(e.Item, "lnkUseExistingAddress", false);
                ShowHideContainedControl(e.Item, "giftShipType", false);
                addressOption.Value = Boolean.TrueString;
            }
            else if (e.CommandName == "UseGiftRegistryAddress")
            {                
                addressOption.Value = Boolean.FalseString;
                ShowHideContainedControl(e.Item, "lnkUseGiftRefistryAddress", false);
                ShowHideContainedControl(e.Item, "lnkUseExistingAddress", true);
                ShowHideContainedControl(e.Item, "giftShipType", true);
                

                ShowHideContainedControl(e.Item, "cboShipToAddress", false);               
                ShowHideContainedControl(e.Item, "pnlAddNewAddress", false);
                ShowHideContainedControl(e.Item, "ctrlAddress", false);
                ShowHideContainedControl(e.Item, "btnSaveNewAddress", false);
                ShowHideContainedControl(e.Item, "lnkCancelAddNew", false);
                ShowHideContainedControl(e.Item, "lnkAddNewAddress", true);
            }
            else if (e.CommandName == "AddNewAddress")
            {
                addressOption.Value = Boolean.TrueString;
                ShowHideContainedControl(e.Item, "giftShipType", false);
                ShowHideContainedControl(e.Item, "pnlAddNewAddress", true);
                ShowHideContainedControl(e.Item, "ctrlAddress", true);
                ShowHideContainedControl(e.Item, "btnSaveNewAddress", true);
                ShowHideContainedControl(e.Item, "lnkCancelAddNew", true);
                ShowHideContainedControl(e.Item, "lnkAddNewAddress", false);

                AddressControl ctrlAddress = e.Item.FindControl("ctrlAddress") as AddressControl;
                if(ctrlAddress != null)
                {
                    SetupAddressControl(ctrlAddress);
                    FindLocaleStrings(ctrlAddress);
                }
            }
            else if (e.CommandName == "CancelAddNew")
            {
                addressOption.Value = Boolean.TrueString;
                ShowHideContainedControl(e.Item, "giftShipType", false);
                ShowHideContainedControl(e.Item, "pnlAddNewAddress", false);
                ShowHideContainedControl(e.Item, "ctrlAddress", false);
                ShowHideContainedControl(e.Item, "btnSaveNewAddress", false);
                ShowHideContainedControl(e.Item, "lnkCancelAddNew", false);
                ShowHideContainedControl(e.Item, "lnkAddNewAddress", true);
            }
            else if (e.CommandName == "SaveNewAddress")
            {
                AddressControl ctrlAddress = e.Item.FindControl("ctrlAddress") as AddressControl;
                ctrlAddress.CountryIDToValidateZipCode = AppLogic.GetCountryID(ctrlAddress.Country);

                Page.Validate("SaveAddress");
                if (Page.IsValid)
                {
                    if (ctrlAddress != null)
                    {
                        Address addr = new Address();
                        addr.CustomerID = ThisCustomer.CustomerID;
                        addr.NickName = ctrlAddress.NickName;
                        addr.FirstName = ctrlAddress.FirstName;
                        addr.LastName = ctrlAddress.LastName;
                        addr.Address1 = ctrlAddress.Address1;
                        addr.Address2 = ctrlAddress.Address2;
                        addr.Company = ctrlAddress.Company;
                        // TBD: Suite is not yet Mapped!!!
                        addr.Suite = string.Empty;
                        addr.City = ctrlAddress.City;
                        addr.State = ctrlAddress.State;
                        addr.Zip = ctrlAddress.ZipCode;
                        addr.Country = ctrlAddress.Country;
                        addr.Phone = ctrlAddress.PhoneNumber;

                        addr.AddressType = AddressTypes.Shipping;
                        addr.InsertDB();

                        ReLoadCustomerShipToAddresses();

                        // rebind all controls
                        InitializeDataSource();
                    }

                    addressOption.Value = Boolean.TrueString;
                    ShowHideContainedControl(e.Item, "giftShipType", false);
                    ShowHideContainedControl(e.Item, "pnlAddNewAddress", false);
                    ShowHideContainedControl(e.Item, "ctrlAddress", false);
                    ShowHideContainedControl(e.Item, "btnSaveNewAddress", false);
                    ShowHideContainedControl(e.Item, "lnkCancelAddNew", false);
                    ShowHideContainedControl(e.Item, "lnkAddNewAddress", true);
                }
            }


        }
        #endregion              
        #region SetupAddressControl
        /// <summary>
        /// Sets the country and state dropdown datasource
        /// </summary>
        /// <param name="ctrlAddress"></param>
        private void SetupAddressControl(AddressControl ctrlAddress)
        {
            List<string> countries = new List<string>();
            using (SqlConnection con = DB.dbConn())
            {
                con.Open();
                using (IDataReader reader = DB.GetRS("SELECT Name FROM Country", con))
                {
                    while (reader.Read())
                    {
                        countries.Add(DB.RSField(reader, "Name"));
                    }
                }
            }

            ctrlAddress.CountryDataSource = countries;

            // populate the available states for this country
            //LoadStateForCountry(ctrlAddress, ctrlAddress.Country);
            ctrlAddress.StateDataSource = State.GetAllStateForCountry(AppLogic.GetCountryID(ctrlAddress.Country), ThisCustomer.LocaleSetting);

        }
        #endregion

        #region ctrlCartItemAddresses_ItemCreated
        /// <summary>
        /// Event Handler for the ItemCreated event of the Repeater control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ctrlCartItemAddresses_ItemCreated(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item ||
                e.Item.ItemType == ListItemType.AlternatingItem)
            {
                AddressControl ctrlAddress = e.Item.FindControl("ctrlAddress") as AddressControl;
                ctrlAddress.SelectedCountryChanged += new EventHandler(ctrlAddress_SelectedCountryChanged);
            }
        }
        #endregion

        private void ctrlAddress_SelectedCountryChanged(object sender, EventArgs e)
        {
            AddressControl ctrlAddress = sender as AddressControl;
            //LoadStateForCountry(ctrlAddress, ctrlAddress.Country);
            //Assign Datasource for the state dropdown
            ctrlAddress.StateDataSource = State.GetAllStateForCountry(AppLogic.GetCountryID(ctrlAddress.Country), ThisCustomer.LocaleSetting);
            ctrlAddress.ShowZip = AppLogic.GetCountryPostalCodeRequired(AppLogic.GetCountryID(ctrlAddress.Country));
        }

        #region ctrlCartItemAddresses_ItemDataBound
        /// <summary>
        /// Event Handler for the ItemDataBound event of the Repeater control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ctrlCartItemAddresses_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || 
                e.Item.ItemType == ListItemType.AlternatingItem)
            {
                CartItem cartItem = e.Item.DataItem as CartItem;                
                Literal giftShipType = e.Item.FindControl("giftShipType") as Literal;
                LinkButton lnkUseExistingAddress = e.Item.FindControl("lnkUseExistingAddress") as LinkButton;
                LinkButton lnkUseGiftRefistryAddress = e.Item.FindControl("lnkUseGiftRefistryAddress") as LinkButton;
                if (cartItem.GiftRegistryForCustomerID != 0)
                {                    
                    giftShipType.Text = AppLogic.GiftRegistryDisplayName(cartItem.GiftRegistryForCustomerID, true, ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                    lnkUseExistingAddress.Visible = false;
                    lnkUseGiftRefistryAddress.Visible = true;
                    giftShipType.Visible = false;
                }
                else
                {
                    lnkUseExistingAddress.Visible = false;
                    lnkUseGiftRefistryAddress.Visible = false;
                    giftShipType.Visible = false;
                }

				DropDownList cboShipToAddress = e.Item.FindControl("cboShipToAddress") as DropDownList;
				if (cboShipToAddress != null)
				{
					foreach (Address shippingAddress in _customerShipToAddresses)
					{
						string value = shippingAddress.AddressID.ToString();
						string displayText = string.Format("{0} {1}", shippingAddress.FirstName, shippingAddress.LastName);
                        if (string.IsNullOrEmpty(displayText.Replace(" ", String.Empty)))
                            continue;

						cboShipToAddress.Items.Add(new ListItem(displayText, value));
					}

					if (cartItem != null)
					{
						cboShipToAddress.SelectedValue = cartItem.ShippingAddressID.ToString();
					}
				}

                if (cartItem.IsDownload || cartItem.IsSystem || !cartItem.Shippable || GiftCard.s_IsEmailGiftCard(cartItem.ProductID))
                {
                    Panel pnlNoShipping = e.Item.FindControl("pnlNoShipping") as Panel;
                    pnlNoShipping.Visible = true;

                    Label lblNoShipping = e.Item.FindControl("lblNoShipping") as Label;
					lblNoShipping.Text = AppLogic.GetString("checkoutshippingmult.aspx.23", SkinID, ThisCustomer.LocaleSetting);

                    Panel pnlselectAddress = e.Item.FindControl("pnlselectAddress") as Panel;
                    pnlselectAddress.Visible = false;

                }
                else
                {
                    Panel pnlNoShipping = e.Item.FindControl("pnlNoShipping") as Panel;
                    pnlNoShipping.Visible = false;

                    Panel pnlselectAddress = e.Item.FindControl("pnlselectAddress") as Panel;
                    pnlselectAddress.Visible = true;
                }
            }
        }
        #endregion

        #region btnContinueCheckOut_Click
        /// <summary>
        /// Handles the continue checkout button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnContinueCheckOut_Click(object sender, EventArgs e)
        {
			ProcessCartItemAddresses();
            Response.Redirect("checkoutshippingmult2.aspx");
        }
        #endregion

        #region ProcessCartItemAddresses        
        /// <summary>
        /// Processes individual line items in the address select list and reconciles it with the ShoppingCart line items
        /// </summary>
        private void ProcessCartItemAddresses()
        {
            /*******************************************************************************
            * 1. CartItem and address not modified
            *     - Leave as is
            * 2. CartItem and address are modified
            *     - Find out if this is the only CartItem
            *	      - Qty = 1 : only modify the shipping address
            *	      - Qty > 1 : Add another line item cloning the original line item
            *******************************************************************************/

            // Data-Structure that will hold all shippingAddress selected for a particular shipping address
            // Key = The address id
            // Value = Collection of CartItems marked to ship to that Address
            Dictionary<int, CartItemCollection> itemsPerAddress = new Dictionary<int, CartItemCollection>();

            CartItemCollection individualBoundItems = ctrlCartItemAddresses.DataSource as CartItemCollection;

            Dictionary<int, KitComposition> Compositions = new Dictionary<int, KitComposition>();

            for (int ctr = 0; ctr < individualBoundItems.Count; ctr++)
            {
                CartItem boundItem = individualBoundItems[ctr];

                //gather compositions so that we can reference the original later
                if (boundItem.IsAKit && !Compositions.ContainsKey(boundItem.ShoppingCartRecordID))
                    Compositions.Add(boundItem.ShoppingCartRecordID, KitComposition.FromCart(ThisCustomer, CartTypeEnum.ShoppingCart, boundItem.ShoppingCartRecordID));

                if (boundItem.Shippable)
                {
                    // get the selected address id from the address drop down
                    int preferredAddress = 0;
                    DropDownList cboShipToAddress = ctrlCartItemAddresses.Items[ctr].FindControl("cboShipToAddress") as DropDownList;
                    HtmlInputHidden ihiddenAddressOption = ctrlCartItemAddresses.Items[ctr].FindControl("ShipAddressOption") as HtmlInputHidden;
                    if (!Boolean.Parse(ihiddenAddressOption.Value))
                    {
                       preferredAddress = AppLogic.GiftRegistryShippingAddressID(boundItem.GiftRegistryForCustomerID);
                    }
                    else
                    {
                    if (cboShipToAddress != null)
                    {
                        if (int.TryParse(cboShipToAddress.SelectedValue, out preferredAddress))
                        {
                            // check to see if the address was modified
                            if (boundItem.ShippingAddressID != preferredAddress)
                            {
                                if (!itemsPerAddress.ContainsKey(preferredAddress))
                                {
                                    itemsPerAddress.Add(preferredAddress, new CartItemCollection());
                                }

                                CartItemCollection itemsInThisAddress = itemsPerAddress[preferredAddress];

                                // check if we duplicates
                                if (!itemsInThisAddress.Contains(boundItem))
                                {
                                    // first line item found for this address
                                    itemsInThisAddress.Add(boundItem);
                                    boundItem.MoveableQuantity = 1;
                                }
                                else
                                {
                                    // we have multiple line items that are marked
                                    // to ship to this address, increment
                                    boundItem.MoveableQuantity++;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    // non-shippable item
                    // ship this to primary if not yet set..

                    // get the original cartItem from the ShoppingCart line items
                    CartItem originalCartItem = cart.CartItems.Find(boundItem.ShoppingCartRecordID);
                    if (originalCartItem != null)
                    {
                        cart.SetItemAddress(originalCartItem.ShoppingCartRecordID, ThisCustomer.PrimaryShippingAddressID);
                    }
                }
            }

            foreach (int preferredAddress in itemsPerAddress.Keys)
            {
                foreach (CartItem item in itemsPerAddress[preferredAddress])
                {
                    // get the original cartItem from the ShoppingCart line items
                    CartItem originalCartItem = cart.CartItems.Find(item.ShoppingCartRecordID);
                    if (originalCartItem == null)
                        continue;

                    // reset the quantity for the original line item that got sliced
                    int resetQuantity = originalCartItem.Quantity - item.MoveableQuantity;

                    KitComposition itemKitComposition = null;
                    if (item.IsAKit && Compositions.ContainsKey(item.ShoppingCartRecordID))
                    {
                        itemKitComposition = Compositions[item.ShoppingCartRecordID];
                    }

                    if (resetQuantity == 0 || item.IsGift)
                    {
                        cart.SetItemAddress(originalCartItem.ShoppingCartRecordID, preferredAddress);
                    }
                    else
                    {
                        cart.SetItemQuantity(originalCartItem.ShoppingCartRecordID, resetQuantity);

                        // now duplicate the line item but for this shipping address
                        cart.AddItem(ThisCustomer,
                            preferredAddress,
                            item.ProductID,
                            item.VariantID,
                            item.MoveableQuantity,
                            item.ChosenColor,
                            item.ChosenColorSKUModifier,
                            item.ChosenSize,
                            item.ChosenSizeSKUModifier,
                            item.TextOption,
                            CartTypeEnum.ShoppingCart,
                            false,
                            false,
                            item.GiftRegistryForCustomerID,
                            item.CustomerEntersPrice ? item.Price : decimal.Zero,
                            itemKitComposition,
                            item.IsGift);
                    }
                    
                }
            }
        }

        #endregion
    }
}
