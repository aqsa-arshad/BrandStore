// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontCore.ShippingCalculation;
using Microsoft.Web.Services3.Referral;
using Newtonsoft.Json;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for checkoutshipping.
    /// </summary>
    public partial class checkoutshipping : SkinBase
    {
        /// <summary>
        /// The cart
        /// </summary>
        ShoppingCart cart = null;
        /// <summary>
        /// Any shipping methods found
        /// </summary>
        bool AnyShippingMethodsFound = false;
        /// <summary>
        /// The shipping methods
        /// </summary>
        private ShippingMethodCollection ShippingMethods;
        /// <summary>
        /// The available fund value
        /// </summary>
        private decimal availableFundValue;
        /// <summary>
        /// The used fund value through shopping cart
        /// </summary>
        private decimal usedFundValueThroughShoppingCart;
        /// <summary>
        /// The is blu bucks
        /// </summary>
        public bool isBluBucks = false;
        /// <summary>
        /// The shipment charges paid
        /// </summary>
        private static String shipmentChargesPaid = string.Empty;
        /// <summary>
        /// The last shipping method value
        /// </summary>
        private decimal lastShippingMethodValue;
        /// <summary>
        /// The SQL
        /// </summary>
        String sql = string.Empty;

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event to initialize the page.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            cart = new ShoppingCart(SkinID, ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);
            ShippingMethods = cart.GetShippingMethods(ThisCustomer.PrimaryShippingAddress);
            //Sort the shipping methids according to display order            
            sql = "select Name, ShippingMethodID,DisplayOrder from ShippingMethod";
            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS(sql, dbconn))
                {
                    while (rs.Read())
                    {
                        int DisplayOrder = Convert.ToInt32(DB.RSFieldInt(rs, "DisplayOrder"));
                        int ShippingMethodID = DB.RSFieldInt(rs, "ShippingMethodID");
                        foreach (ShippingMethod method in ShippingMethods)
                        {
                            if (method.Id == ShippingMethodID)
                                method.DisplayOrder = DisplayOrder;
                        }
                    }
                }
            }
            ShippingMethods.Sort((x, y) => x.DisplayOrder.CompareTo(y.DisplayOrder));
            //end sorting                  

            if (ShippingMethods.Count > 0)
            {
                ShippingMethod sm = ShippingMethods.Find(x => x.Id == -1 || x.Id == -2);
                if ((ShippingMethods.Count == 1) && sm != null)
                {
                    Label1.Visible = false;
                    lblshippingmethodnote.Visible = false;
                    btnContinueCheckout.Enabled = false;
                }
                else
                {
                    AnyShippingMethodsFound = true;
                    Label1.Visible = true;
                    btnContinueCheckout.Enabled = true;
                }
            }
            else
            {
                Label1.Visible = false;
                lblshippingmethodnote.Visible = false;
                btnContinueCheckout.Enabled = false;
            }
            InitializeShippingMethodDisplayFormat(ShippingMethods);
            ctrlShippingMethods.DataSource = ShippingMethods;
            ctrlShippingMethods.DataBind();

            ctrlShoppingCart.DataSource = cart.CartItems;
            ctrlShoppingCart.DataBind();

            ctrlCartSummary.DataSource = cart;
            ctrlCartSummary.DataBind();

            InitializeOrderOptionControl();

            GatewayCheckoutByAmazon.CheckoutByAmazon checkoutByAmazon = new GatewayCheckoutByAmazon.CheckoutByAmazon();

            if (checkoutByAmazon.IsEnabled && checkoutByAmazon.IsCheckingOut)
            {
                pnlCBAAddressWidget.Visible = true;
                litCBAAddressWidget.Text = new GatewayCheckoutByAmazon.CheckoutByAmazon().RenderAddressWidgetWithRedirect("CBAAddressWidgetContainer", "checkoutshipping.aspx", new Guid(ThisCustomer.CustomerGUID), 300, 200);
                litCBAAddressWidgetInstruction.Text = "gw.checkoutbyamazon.display.4".StringResource();
            }          

            base.OnInit(e);
        }

        /// <summary>
        /// Initializes the order option control
        /// </summary>
        private void InitializeOrderOptionControl()
        {
            ctrlOrderOption.ThisCustomer = ThisCustomer;
            ctrlOrderOption.EditMode = false;
            ctrlOrderOption.Datasource = cart;

            if (cart.OrderOptions.Count > 0)
            {

                ctrlOrderOption.DataBind();
                ctrlOrderOption.Visible = true;
            }
            else
            {
                ctrlOrderOption.Visible = false;
            }
        }
        /// <summary>
        /// Handles the Click event of the btnback control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnback_Click(object sender, EventArgs e)
        {
            Response.Redirect("account.aspx?checkout=true");
        }

        /// <summary>
        /// Initializes the shipping method display format.
        /// </summary>
        /// <param name="shippingMethods">The shipping methods.</param>
        private void InitializeShippingMethodDisplayFormat(ShippingMethodCollection shippingMethods)
        {
            foreach (ShippingMethod shipMethod in shippingMethods)
            {
                string freightDisplayText = string.Empty;

                if (!string.IsNullOrEmpty(ThisCustomer.CurrencySetting))
                {
                    decimal shippingPromoDiscount = 0;

                    decimal calculatedFreight = AddVatIfApplicable(shipMethod.Freight - shippingPromoDiscount);
                    freightDisplayText = Localization.CurrencyStringForDisplayWithExchangeRate(calculatedFreight, ThisCustomer.CurrencySetting);
                    if (shipMethod.ShippingIsFree && Shipping.ShippingMethodIsInFreeList(shipMethod.Id))
                    {
                        freightDisplayText = string.Format("({0})", AppLogic.GetString("shoppingcart.aspx.16", SkinID, ThisCustomer.LocaleSetting));
                    }

                    if (cart.FreeShippingReason == Shipping.FreeShippingReasonEnum.AllDownloadItems || cart.FreeShippingReason == Shipping.FreeShippingReasonEnum.AllOrdersHaveFreeShipping)
                    {
                        freightDisplayText = string.Empty;
                    }

                    freightDisplayText += AddVatDetailsIfApplicable();
                }
                shipMethod.DisplayFormat = string.Format("{0} {1}", shipMethod.GetNameForDisplay(), freightDisplayText);
            }
        }

        /// <summary>
        /// Adds the vat details if applicable.
        /// </summary>
        /// <returns></returns>
        private string AddVatDetailsIfApplicable()
        {
            if (!AppLogic.AppConfigBool("VAT.Enabled"))
                return "";

            if (ThisCustomer.VATSettingReconciled == VATSettingEnum.ShowPricesInclusiveOfVAT)
                return " " + "setvatsetting.aspx.6".StringResource();
            else
                return " " + "setvatsetting.aspx.7".StringResource();
        }

        /// <summary>
        /// Adds the vat if applicable.
        /// </summary>
        /// <param name="freight">The freight.</param>
        /// <returns></returns>
        private decimal AddVatIfApplicable(Decimal freight)
        {
            if (AppLogic.AppConfigBool("VAT.Enabled") && ThisCustomer.VATSettingReconciled == VATSettingEnum.ShowPricesInclusiveOfVAT)
            {
                // add the VAT rate to the price
                freight = freight + (freight * (Prices.TaxRate(ThisCustomer, AppLogic.AppConfigNativeInt("ShippingTaxClassID")) / 100));
            }
            return freight;
        }
        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = -1;
            Response.AddHeader("pragma", "no-cache");

            ErrorMessage err;
            if (AppLogic.AppConfigBool("RequireOver13Checked") && !ThisCustomer.IsOver13)
            {
                err = new ErrorMessage(Server.HtmlEncode(AppLogic.GetString("checkout.over13required", ThisCustomer.SkinID, ThisCustomer.LocaleSetting)));
                Response.Redirect("shoppingcart.aspx?errormsg=" + err.MessageId);
            }

            bool phoneCustomer = ((HttpContext.Current.Items["IsBeingImpersonated"] != null) &&
                ((string)HttpContext.Current.Items["IsBeingImpersonated"] == "true"));

            bool paypalExpressCheckout = (ThisCustomer.ThisCustomerSession["paypalexpresspayerid"].ToString().Length == 0 &&
                ThisCustomer.ThisCustomerSession["paypalexpresstoken"].ToString().Length == 0);

            var checkoutController = CheckOutPageControllerFactory.CreateCheckOutPageController(ThisCustomer, cart);
            if (checkoutController.GetCheckoutType() == CheckOutType.SmartOPC)
            {
                if (!phoneCustomer && checkoutController.CanUseOnePageCheckout())
                    Response.Redirect(checkoutController.GetSmartOnePageCheckoutPage());
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
                    Response.Redirect("signin.aspx?checkout=true");//createaccount
                }
            }
            if (ThisCustomer.PrimaryBillingAddressID == 0 || ThisCustomer.PrimaryShippingAddressID == 0)
            {
                err = new ErrorMessage(Server.HtmlEncode(AppLogic.GetString("checkoutpayment.aspx.2", ThisCustomer.SkinID, ThisCustomer.LocaleSetting)));
                Response.Redirect("shoppingcart.aspx?resetlinkback=1&errormsg=" + err.MessageId);
            }

            SectionTitle = AppLogic.GetString("checkoutshipping.aspx.1", SkinID, ThisCustomer.LocaleSetting);
            cart = new ShoppingCart(SkinID, ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);

            cart.ValidProceedCheckout(); // will not come back from this if any issue. they are sent back to the cart page!

            GatewayCheckoutByAmazon.CheckoutByAmazon checkoutByAmazon = new GatewayCheckoutByAmazon.CheckoutByAmazon();
            if (checkoutByAmazon.IsCheckingOut)
            {
                if (checkoutByAmazon.GetDefaultShippingAddress() == null)
                    btnContinueCheckout.OnClientClick = "alert('" + "gw.checkoutbyamazon.display.3".StringResource() + "'); return false;";
            }

            if (!cart.IsAllDownloadComponents() && !cart.IsAllFreeShippingComponents() && !cart.IsAllSystemComponents() && (cart.HasMultipleShippingAddresses() || cart.HasGiftRegistryComponents()) && cart.TotalQuantity() <= AppLogic.MultiShipMaxNumItemsAllowed() && cart.CartAllowsShippingMethodSelection && cart.TotalQuantity() > 1 && !checkoutByAmazon.IsCheckingOut)
            {
                Response.Redirect("checkoutshippingmult.aspx");
            }

            //MOD GS - If entire cart is email gift cards...redirect to checkoutgiftcard.aspx
            if (cart.IsAllEmailGiftCards())
            {
                Response.Redirect("checkoutgiftcard.aspx");
            }

            if (AppLogic.AppConfigBool("SkipShippingOnCheckout") ||
                cart.IsAllSystemComponents() ||
                cart.IsAllDownloadComponents() ||
                cart.NoShippingRequiredComponents())
            {
                if (cart.ContainsGiftCard())
                {
                    Response.Redirect("checkoutgiftcard.aspx");
                }
                else
                {
                    if (ThisCustomer.ThisCustomerSession["PayPalExpressToken"] == "")
                    {
                        Response.Redirect("checkoutpayment.aspx");
                    }
                    else
                    {
                        Response.Redirect("checkoutreview.aspx?PaymentMethod=PAYPALEXPRESS");
                    }
                }
            }

            pnlSelectShipping.Visible = AppLogic.AppConfigBool("AllowAddressChangeOnCheckoutShipping") && AppLogic.AppConfigBool("AllowShipToDifferentThanBillTo") && !checkoutByAmazon.IsCheckingOut;

            ctrlShippingMethods.ErrorMessage = string.Empty;
            pnlErrorMsg.Visible = false;

            CartItem FirstCartItem = (CartItem)cart.CartItems[0];
            Address FirstItemShippingAddress = new Address();
            FirstItemShippingAddress.LoadByCustomer(ThisCustomer.CustomerID, FirstCartItem.ShippingAddressID, AddressTypes.Shipping);
            if (FirstItemShippingAddress.AddressID == 0)
            {
                err = new ErrorMessage(Server.HtmlEncode(AppLogic.GetString("checkoutshipping.aspx.10", ThisCustomer.SkinID, ThisCustomer.LocaleSetting)));
                Response.Redirect("shoppingcart.aspx?errormsg=" + err.MessageId);
            }

            if (!this.IsPostBack)
            {
                shipmentChargesPaid = string.Empty;

                if (!AppLogic.AppConfigBool("AllowMultipleShippingAddressPerOrder") && CommonLogic.QueryStringCanBeDangerousContent("dontupdateid").Length == 0)
                {
                    // force primary shipping address id to be active on all cart items (safety check):
                    DB.ExecuteSQL("update ShoppingCart set ShippingAddressID=(select ShippingAddressID from customer where CustomerID=" + ThisCustomer.CustomerID.ToString() + ") where CustomerID=" + ThisCustomer.CustomerID.ToString() + " and CartType=" + ((int)CartTypeEnum.ShoppingCart).ToString());
                    String QS = Request.QueryString["fillcontrols"];

                    if (QS == "true" || QS == "True")
                    {
                        Response.Redirect("checkoutshipping.aspx?dontupdateid=true&fillcontrols=true");
                    }
                    else
                        Response.Redirect("checkoutshipping.aspx?dontupdateid=true");
                }
                InitializePageContent();
                sql = String.Format("select ShipmentChargesPaid from dbo.ShoppingCart where ShoppingCartRecID={0} and CustomerID={1}", cart.CartItems.FirstOrDefault().ShoppingCartRecordID, ThisCustomer.CustomerID);
                using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
                {
                    dbconn.Open();
                    using (IDataReader rs = DB.GetRS(sql, dbconn))
                    {
                        if (rs.Read())
                        {
                            lastShippingMethodValue = DB.RSFieldDecimal(rs, "ShipmentChargesPaid");
                        }
                    }
                }
                if (!cart.ShippingIsFree)
                {
                    FundUpdate();
                }
            }
            else
            {
                pnlErrorMsg.Visible = false;
                if (CommonLogic.FormCanBeDangerousContent("btnContinueCheckout") != "")
                {
                    ProcessCheckOut();
                }
            }

            AppLogic.eventHandler("CheckoutShipping").CallEvent("&CheckoutShipping=true");
        }

        /// <summary>
        /// Handles the Change event of the ShippingCountry control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void ShippingCountry_Change(object sender, EventArgs e)
        {
            SetShippingStateList(ShippingCountry.SelectedValue);
        }

        /// <summary>
        /// Handles the OnClick event of the btnNewShipAddr control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        public void btnNewShipAddr_OnClick(object sender, System.EventArgs e)
        {
            Validate("shipping1");
            if (IsValid)
            {
                CreateShipAddress();
                InitializePageContent();

                if (ddlChooseShippingAddr.SelectedValue != "0")
                {
                    pnlNewShipAddr.Visible = false;
                    pnlCartAllowsShippingMethodSelection.Visible = true;
                    btnContinueCheckout.Visible = true;

                    ThisCustomer.PrimaryShippingAddressID = int.Parse(ddlChooseShippingAddr.SelectedValue);

                    btnContinueCheckout.Visible = true;
                    ShippingMethodCollection shipMethods = cart.GetShippingMethods(ThisCustomer.PrimaryShippingAddress);
                    InitializeShippingMethodDisplayFormat(shipMethods);
                    ctrlShippingMethods.DataSource = shipMethods;
                }
                else
                {
                    pnlNewShipAddr.Visible = true;
                    pnlCartAllowsShippingMethodSelection.Visible = false;
                    btnContinueCheckout.Visible = false;
                }

            }
            else
            {
                ctrlShippingMethods.ErrorMessage += "<div>" + AppLogic.GetString("checkoutshipping.aspx.23", SkinID, ThisCustomer.LocaleSetting) + "</div>";
                foreach (IValidator aValidator in this.Validators)
                {
                    if (!aValidator.IsValid)
                    {
                        ctrlShippingMethods.ErrorMessage += "<div> " + aValidator.ErrorMessage + "</div>";
                    }
                }
                pnlErrorMsg.Visible = true;
            }

        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlChooseShippingAddr control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        public void ddlChooseShippingAddr_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (ddlChooseShippingAddr.SelectedValue != "0")
            {
                pnlNewShipAddr.Visible = false;
                pnlCartAllowsShippingMethodSelection.Visible = true;
                DB.ExecuteSQL(String.Format("update dbo.Customer set ShippingAddressID = {0} where CustomerID={1} ", ddlChooseShippingAddr.SelectedValue, ThisCustomer.CustomerID.ToString()));
                DB.ExecuteSQL("update ShoppingCart set ShippingAddressID=" + ddlChooseShippingAddr.SelectedValue + " where CustomerID=" + ThisCustomer.CustomerID.ToString() + " and CartType=" + ((int)CartTypeEnum.ShoppingCart).ToString());
                ThisCustomer.PrimaryShippingAddressID = int.Parse(ddlChooseShippingAddr.SelectedValue);
                cart = new ShoppingCart(SkinID, ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);
                bool AnyShippingMethodsFound = false;
                String ShipMethods = cart.GetShippingMethodList(String.Empty, out AnyShippingMethodsFound);
                ShippingDisplay(AnyShippingMethodsFound);
                if (!cart.CartAllowsShippingMethodSelection || AnyShippingMethodsFound)
                {
                    btnContinueCheckout.Visible = true;
                    ShippingMethodCollection smcoll = cart.GetShippingMethods(ThisCustomer.PrimaryShippingAddress);
                    InitializeShippingMethodDisplayFormat(smcoll);
                    ctrlShippingMethods.DataSource = smcoll;
                }
                SetDebugInformation();
            }
            else
            {
                pnlNewShipAddr.Visible = true;
                pnlCartAllowsShippingMethodSelection.Visible = false;
                btnContinueCheckout.Visible = false;
            }
        }

        /// <summary>
        /// Handles the OnClick event of the lnkShowNewShipping control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void lnkShowNewShipping_OnClick(object sender, EventArgs e)
        {
            pnlNewShipAddr.Visible = !pnlNewShipAddr.Visible;
        }

        /// <summary>
        /// Initializes the content of the page.
        /// </summary>
        private void InitializePageContent()
        {
            JSPopupRoutines.Text = AppLogic.GetJSPopupRoutines();

            ShippingResidenceType.Items.Add(new ListItem(AppLogic.GetString("address.cs.55", SkinID, ThisCustomer.LocaleSetting), ((int)ResidenceTypes.Unknown).ToString()));
            ShippingResidenceType.Items.Add(new ListItem(AppLogic.GetString("address.cs.56", SkinID, ThisCustomer.LocaleSetting), ((int)ResidenceTypes.Residential).ToString()));
            ShippingResidenceType.Items.Add(new ListItem(AppLogic.GetString("address.cs.57", SkinID, ThisCustomer.LocaleSetting), ((int)ResidenceTypes.Commercial).ToString()));
            ShippingResidenceType.SelectedIndex = 1;

            if (ctrlShippingMethods.SelectedItem == null)
            {
                ctrlCartSummary.ShowTax = false;
                ctrlCartSummary.ShowShipping = false;
                ctrlCartSummary.ShowTotal = false;
            }
            if (CommonLogic.QueryStringNativeInt("ErrorMsg") > 0)
            {
                pnlErrorMsg.Visible = true;

                ErrorMessage e = new ErrorMessage(CommonLogic.QueryStringNativeInt("ErrorMsg"));

                ErrorMsgLabel.Text = e.Message;
            }

            if (!cart.ShippingIsFree && cart.MoreNeededToReachFreeShipping != 0.0M)
            {
                pnlGetFreeShippingMsg.Visible = true;
                GetFreeShippingMsg.Text = String.Format(AppLogic.GetString("checkoutshipping.aspx.2", SkinID, ThisCustomer.LocaleSetting), ThisCustomer.CurrencyString(cart.FreeShippingThreshold), CommonLogic.Capitalize(cart.FreeShippingMethod));
            }

            if (cart.ShippingIsFree)
            {
                lblshippingmethodnote.Visible = false;
                pnlFreeShippingMsg.Visible = true;
                String Reason = String.Empty;
                if (cart.FreeShippingReason == Shipping.FreeShippingReasonEnum.AllDownloadItems)
                {
                    Reason = AppLogic.GetString("checkoutshipping.aspx.5", SkinID, ThisCustomer.LocaleSetting);
                    btnContinueCheckout.Text = AppLogic.GetString("checkoutshipping.aspx.19", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                }
                if (cart.FreeShippingReason == Shipping.FreeShippingReasonEnum.AllFreeShippingItems)
                {
                    Reason = AppLogic.GetString("checkoutshipping.aspx.18", SkinID, ThisCustomer.LocaleSetting);
                    btnContinueCheckout.Text = AppLogic.GetString("checkoutshipping.aspx.19", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                }
                if (cart.FreeShippingReason == Shipping.FreeShippingReasonEnum.CouponHasFreeShipping)
                {
                    Reason = AppLogic.GetString("checkoutshipping.aspx.6", SkinID, ThisCustomer.LocaleSetting);
                    btnContinueCheckout.Text = AppLogic.GetString("checkoutshipping.aspx.19", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                }
                if (cart.FreeShippingReason == Shipping.FreeShippingReasonEnum.AllOrdersHaveFreeShipping)
                {
                    Reason = String.Format(AppLogic.GetString("checkoutshipping.aspx.7", SkinID, ThisCustomer.LocaleSetting), cart.FreeShippingMethod);
                    btnContinueCheckout.Text = AppLogic.GetString("checkoutshipping.aspx.19", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                }
                if (cart.FreeShippingReason == Shipping.FreeShippingReasonEnum.CustomerLevelHasFreeShipping)
                {
                    Reason = String.Format(AppLogic.GetString("checkoutshipping.aspx.8", SkinID, ThisCustomer.LocaleSetting), ThisCustomer.CustomerLevelName);
                    btnContinueCheckout.Text = AppLogic.GetString("checkoutshipping.aspx.19", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                }
                if (cart.FreeShippingReason == Shipping.FreeShippingReasonEnum.ExceedsFreeShippingThreshold)
                {
                    Reason = cart.FreeShippingMethod;
                    btnContinueCheckout.Text = AppLogic.GetString("checkoutshipping.aspx.19", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                }
                FreeShippingMsg.Text = Reason;
                //Added by tayyab in case of free shipping
                btnContinueCheckout.Enabled = true;
                btnContinueCheckout.Visible = true;
            }

            Addresses addrs = new Addresses();
            addrs.LoadCustomer(ThisCustomer.CustomerID);
            ddlChooseShippingAddr.Items.Clear();
            Hashtable ht = new Hashtable();
            // add their primary shipping address id FIRST:
            foreach (Address a in addrs)
            {
                if (a.AddressID == ThisCustomer.PrimaryShippingAddressID)
                {
                    string addrString = a.Address1 + " " + a.City + " " + a.State + " " + a.Country + " " + a.Zip;
                    while (addrString.IndexOf("  ") != -1)
                    {
                        addrString = addrString.Replace("  ", " ");
                    }
                    ht.Add(addrString, "true");
                    ddlChooseShippingAddr.Items.Add(new ListItem(addrString, a.AddressID.ToString()));
                }
            }
            // now add their remaining addresses, only if they are materially different from the primary shipping address
            foreach (Address a in addrs)
            {
                if (a.AddressID != ThisCustomer.PrimaryShippingAddressID)
                {
                    string addrString = a.Address1 + " " + a.City + " " + a.State + " " + a.Country + " " + a.Zip;
                    while (addrString.IndexOf("  ") != -1)
                    {
                        addrString = addrString.Replace("  ", " ");
                    }
                    if (!ht.Contains(addrString))
                    {
                        ht.Add(addrString, "true");
                        ddlChooseShippingAddr.Items.Add(new ListItem(addrString, a.AddressID.ToString()));
                    }
                }
            }

            //allow the customer to add a new address
            ddlChooseShippingAddr.Items.Add(new ListItem(AppLogic.GetString("checkoutshipping.aspx.22", ThisCustomer.SkinID, ThisCustomer.LocaleSetting), "0"));

            ddlChooseShippingAddr.SelectedValue = ThisCustomer.PrimaryShippingAddressID.ToString();
            ddlChooseShippingAddr.AutoPostBack = true;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select * from State  with (NOLOCK)  order by DisplayOrder,Name", con))
                {
                    ShippingState.DataSource = rs;
                    ShippingState.DataTextField = "Name";
                    ShippingState.DataValueField = "Abbreviation";
                    ShippingState.DataBind();
                }
            }

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select * from Country   with (NOLOCK)  order by DisplayOrder,Name", con))
                {
                    ShippingCountry.DataSource = rs;
                    ShippingCountry.DataTextField = "Name";
                    ShippingCountry.DataValueField = "Name";
                    ShippingCountry.DataBind();
                }
            }

            ShippingCountry.SelectedValue = "United States";


            pnlCartAllowsShippingMethodSelection.Visible = cart.CartAllowsShippingMethodSelection;
            ctrlShippingMethods.Visible = cart.CartAllowsShippingMethodSelection;

            if ((!cart.CartAllowsShippingMethodSelection || AnyShippingMethodsFound) || (!AnyShippingMethodsFound && !AppLogic.AppConfigBool("FreeShippingAllowsRateSelection") && cart.ShippingIsFree))
            {
                btnContinueCheckout.Visible = true;
            }

            ShippingDisplay(AnyShippingMethodsFound);

            CartSummary.Text = cart.DisplaySummary(true, false, false, false, false);

            SetDebugInformation();
            if (AppLogic.AppConfigBool("PayPal.Express.UseIntegratedCheckout"))
                ltPayPalIntegratedCheckout.Text = AspDotNetStorefrontGateways.Processors.PayPalController.GetExpressCheckoutIntegratedScript(false);
        }

        /// <summary>
        /// Sets the debug information.
        /// </summary>
        private void SetDebugInformation()
        {
            if ((AppLogic.AppConfigBool("RTShipping.DumpXMLOnCheckoutShippingPage") || AppLogic.AppConfigBool("RTShipping.DumpXMLOnCartPage")) && cart.ShipCalcID == Shipping.ShippingCalculationEnum.UseRealTimeRates)
            {
                StringBuilder tmpS = new StringBuilder(4096);
                tmpS.Append("<div class=\"divider\"></div>");

                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS("Select RTShipRequest,RTShipResponse from customer  with (NOLOCK)  where CustomerID=" + ThisCustomer.CustomerID.ToString(), con))
                    {
                        if (rs.Read())
                        {
                            String s = DB.RSField(rs, "RTShipRequest");
                            s = s.Replace("<?xml version=\"1.0\"?>", "");
                            try
                            {
                                s = XmlCommon.PrettyPrintXml("<roottag_justaddedfordisplayonthispage>" + s + "</roottag_justaddedfordisplayonthispage>");
                            }
                            catch
                            {
                                s = DB.RSField(rs, "RTShipRequest");
                            }
                            tmpS.Append("<b>" + AppLogic.GetString("shoppingcart.aspx.5", SkinID, ThisCustomer.LocaleSetting) + "</b><textarea rows=60 style=\"width: 100%\">" + s + "</textarea>");

                            s = DB.RSField(rs, "RTShipResponse");
                            try
                            {
                                s = XmlCommon.PrettyPrintXml("<roottag_justaddedfordisplayonthispage>" + s + "</roottag_justaddedfordisplayonthispage>");
                            }
                            catch
                            {
                                s = DB.RSField(rs, "RTShipResponse");
                            }
                            tmpS.Append("<b>" + AppLogic.GetString("shoppingcart.aspx.6", SkinID, ThisCustomer.LocaleSetting) + "</b><textarea rows=60 style=\"width: 100%\">" + s + "</textarea>");
                        }
                    }
                }

                DebugInfo.Text = tmpS.ToString();
            }
        }

        /// <summary>
        /// Shippings the display.
        /// </summary>
        /// <param name="AnyShippingMethodsFound">if set to <c>true</c> [any shipping methods found].</param>
        private void ShippingDisplay(bool AnyShippingMethodsFound)
        {
            if (cart.CartAllowsShippingMethodSelection)
            {
                GatewayCheckoutByAmazon.CheckoutByAmazon checkoutByAmazon = new GatewayCheckoutByAmazon.CheckoutByAmazon();
                if (Shipping.MultiShipEnabled() && cart.TotalQuantity() > 1 && cart.TotalQuantity() <= AppLogic.MultiShipMaxNumItemsAllowed() && !checkoutByAmazon.IsCheckingOut)
                {
                    lblMultiShipPrompt.Visible = true;
                    lblMultiShipPrompt.Text = "<div class=\"multiship-response\">" + String.Format(AppLogic.GetString("checkoutshipping.aspx.15", SkinID, ThisCustomer.LocaleSetting), "checkoutshippingmult.aspx") + "</div>";
                }
                else
                    lblMultiShipPrompt.Visible = false;

                Boolean CustomerLevelHasFreeShipping = false;
                if (ThisCustomer.CustomerLevelID > 0)
                {
                    using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                    {
                        con.Open();
                        using (IDataReader clvl = DB.GetRS("SELECT LevelHasFreeShipping FROM CustomerLevel WHERE CustomerLevelID=" + ThisCustomer.CustomerLevelID.ToString(), con))
                        {
                            while (clvl.Read())
                            {
                                CustomerLevelHasFreeShipping = DB.RSFieldBool(clvl, "LevelHasFreeShipping");
                            }
                        }
                    }
                }

                //If cart is all items that do not require shipping direct to checkoutpayment
                if (cart.NoShippingRequiredComponents())
                {
                    if (cart.ContainsGiftCard())
                    {
                        Response.Redirect("checkoutgiftcard.aspx");
                    }
                    else
                    {
                        Response.Redirect("checkoutpayment.aspx");
                    }
                }

                if (!AppLogic.AppConfigBool("FreeShippingAllowsRateSelection") && (cart.IsAllFreeShippingComponents() || (!AnyShippingMethodsFound && cart.ShippingIsFree) || CustomerLevelHasFreeShipping || cart.FreeShippingReason == Shipping.FreeShippingReasonEnum.ExceedsFreeShippingThreshold || cart.FreeShippingReason == Shipping.FreeShippingReasonEnum.CouponHasFreeShipping))
                {
                    ErrorMsgLabel.Text += "<div>" + cart.GetFreeShippingReason() + "</div>";
                    ctrlShippingMethods.Visible = false;
                    pnlErrorMsg.Visible = true;
                }
                else
                {
                    btnContinueCheckout.Text = AppLogic.GetString("checkoutshipping.aspx.13", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                }
            }
        }

        /// <summary>
        /// Processes the check out.
        /// </summary>
        private void ProcessCheckOut()
        {
            FundUpdate();
            if (!ThisCustomer.IsAdminUser)
            {
                if (!string.IsNullOrEmpty(shipmentChargesPaid))
                {
                    if (isBluBucks)
                    {
                        sql =
                            String.Format(
                                "update dbo.CustomerFund set AmountUsed={0} where CustomerID={1} and FundID={2}",
                                Math.Round(usedFundValueThroughShoppingCart + Convert.ToDecimal(shipmentChargesPaid), 2)
                                    .ToString(), ThisCustomer.CustomerID.ToString(), (int)FundType.BLUBucks);
                        DB.ExecuteSQL(sql);
                    }
                    else
                    {
                        sql =
                            String.Format(
                                "update dbo.CustomerFund set AmountUsed={0} where CustomerID={1} and FundID={2}",
                                Math.Round(usedFundValueThroughShoppingCart + Convert.ToDecimal(shipmentChargesPaid), 2)
                                    .ToString(), ThisCustomer.CustomerID.ToString(), (int)FundType.SOFFunds);
                        DB.ExecuteSQL(sql);
                    }
                }
            }
            String ShippingMethodIDFormField = string.Empty;
            bool hasSelected = ctrlShippingMethods.SelectedItem != null;

            if (hasSelected)
            {
                ShippingMethodIDFormField = ctrlShippingMethods.SelectedItem.Value;//CommonLogic.FormCanBeDangerousContent("ShippingMethodID").Replace(",", ""); // remember to remove the hidden field which adds a comma to the form post (javascript again)
            }
            if (ShippingMethodIDFormField.Length == 0 && (!cart.ShippingIsFree || CommonLogic.FormBool("RequireShippingSelection")))
            {
                ctrlShippingMethods.ErrorMessage = AppLogic.GetString("checkoutshipping.aspx.17", SkinID, ThisCustomer.LocaleSetting);
                pnlErrorMsg.Visible = true;
            }
            else
            {
                if (cart.IsEmpty())
                {
                    Response.Redirect("shoppingcart.aspx");
                }

                int ShippingMethodID = 0;
                String ShippingMethod = String.Empty;
                //if used fixed price rather then real time rates
                if (cart.ShipCalcID != Shipping.ShippingCalculationEnum.UseRealTimeRates)
                {
                    ShippingMethodID = Localization.ParseUSInt(ShippingMethodIDFormField);
                    ShippingMethod = Shipping.GetShippingMethodDisplayName(ShippingMethodID, null);
                }
                else
                {
                    if (ShippingMethodIDFormField.Length != 0 && ShippingMethodIDFormField.IndexOf('|') != -1)
                    {
                        String[] frmsplit = ShippingMethodIDFormField.Split('|');
                        ShippingMethodID = Localization.ParseUSInt(frmsplit[0]);
                        ShippingMethod = Shipping.GetFormattedRealTimeShippingMethodForDatabase(frmsplit[1], frmsplit[2], frmsplit[3]);
                    }
                }

                if (cart.ShippingIsFree && !AppLogic.AppConfigBool("FreeShippingAllowsRateSelection"))
                {
                    ShippingMethodID = 0;
                    String cartFreeShippingReason = cart.GetFreeShippingReason();
                    ShippingMethod = string.Format(AppLogic.GetString("shoppingcart.aspx.16", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + " : {0}", cartFreeShippingReason);
                }


                if (!cart.IsAllFreeShippingComponents() && !cart.ContainsRecurring())
                {
                    sql = String.Format("update dbo.ShoppingCart set ShippingMethodID={0}, ShippingMethod={1}, ShippingAddressID={4}, ShipmentChargesPaid={5} where CustomerID={2} and CartType={3}", ShippingMethodID.ToString(), DB.SQuote(ShippingMethod), ThisCustomer.CustomerID.ToString(), ((int)CartTypeEnum.ShoppingCart).ToString(), ddlChooseShippingAddr.SelectedValue, shipmentChargesPaid);
                    DB.ExecuteSQL(sql);
                }
                else
                {
                    for (int i = 0; i <= cart.CartItems.Count - 1; i++)
                    {
                        CartItem _CartItem = (CartItem)cart.CartItems[i];
                        int _CartRecID = DB.GetSqlN(String.Format("select ShoppingCartRecID N from ShoppingCart where CustomerID={0} and ProductID={1} and VariantID={2} and CartType={3}", ThisCustomer.CustomerID.ToString(), _CartItem.ProductID, _CartItem.VariantID, ((int)_CartItem.CartType).ToString()));

                        if (!_CartItem.FreeShipping || AppLogic.AppConfigBool("FreeShippingAllowsRateSelection"))
                        {
                            sql = String.Format("update dbo.ShoppingCart set ShippingMethodID={0}, ShippingMethod={1}, ShippingAddressID={4} where CustomerID={2} and CartType={3} and ShoppingCartRecID={5}", ShippingMethodID.ToString(), DB.SQuote(ShippingMethod), ThisCustomer.CustomerID.ToString(), ((int)CartTypeEnum.ShoppingCart).ToString(), ddlChooseShippingAddr.SelectedValue, _CartRecID.ToString());
                        }
                        else
                        {
                            sql = String.Format("update dbo.ShoppingCart set ShippingAddressID={0} where CustomerID={1} and CartType={2} and ShoppingCartRecID={3}", ddlChooseShippingAddr.SelectedValue, ThisCustomer.CustomerID.ToString(), ((int)_CartItem.CartType).ToString(), _CartRecID.ToString());
                        }
                        DB.ExecuteSQL(sql);
                    }
                }

                sql = String.Format("update dbo.Customer set ShippingAddressID = {0} where CustomerID={1} ", ddlChooseShippingAddr.SelectedValue, ThisCustomer.CustomerID.ToString());
                DB.ExecuteSQL(sql);

                if (cart.ContainsGiftCard())
                {
                    Response.Redirect("checkoutgiftcard.aspx");
                }
                else
                {
                    if (ThisCustomer.ThisCustomerSession["PayPalExpressToken"] == "")
                    {
                        Response.Redirect("checkoutpayment.aspx");
                    }
                    else
                    {
                        Response.Redirect("checkoutreview.aspx?PaymentMethod=PAYPALEXPRESS");
                    }
                }
            }
        }

        /// <summary>
        /// Creates the ship address.
        /// </summary>
        private void CreateShipAddress()
        {
            Address thisAddress = new Address();

            thisAddress.CustomerID = ThisCustomer.CustomerID;
            thisAddress.NickName = "";
            thisAddress.FirstName = ShippingFirstName.Text;
            thisAddress.LastName = ShippingLastName.Text;
            thisAddress.Company = ShippingCompany.Text;
            thisAddress.Address1 = ShippingAddress1.Text;
            thisAddress.Address2 = ShippingAddress2.Text;
            thisAddress.Suite = ShippingSuite.Text;
            thisAddress.City = ShippingCity.Text;
            thisAddress.State = ShippingState.SelectedValue;
            thisAddress.Zip = ShippingZip.Text;
            thisAddress.Country = ShippingCountry.SelectedValue;
            thisAddress.Phone = ShippingPhone.Text;
            thisAddress.ResidenceType = (ResidenceTypes)Convert.ToInt32(ShippingResidenceType.SelectedValue);

            thisAddress.InsertDB();
            int AddressID = thisAddress.AddressID;

            DB.ExecuteSQL(String.Format("update dbo.Customer set ShippingAddressID = {0} where CustomerID={1} ", AddressID.ToString(), ThisCustomer.CustomerID.ToString()));
            DB.ExecuteSQL("update ShoppingCart set ShippingAddressID=" + AddressID.ToString() + " where CustomerID=" + ThisCustomer.CustomerID.ToString() + " and CartType=" + ((int)CartTypeEnum.ShoppingCart).ToString());
            ThisCustomer.PrimaryShippingAddressID = AddressID;

            ShippingFirstName.Text = "";
            ShippingLastName.Text = "";
            ShippingCompany.Text = ""; ;
            ShippingAddress1.Text = "";
            ShippingAddress2.Text = "";
            ShippingSuite.Text = "";
            ShippingCity.Text = "";
            ShippingState.SelectedIndex = 0;
            ShippingZip.Text = "";
            ShippingCountry.SelectedIndex = 0;
            ShippingPhone.Text = "";
        }

        /// <summary>
        /// Sets the shipping state list.
        /// </summary>
        /// <param name="shippingCountry">The shipping country.</param>
        private void SetShippingStateList(string shippingCountry)
        {
            string sql = String.Empty;
            if (shippingCountry.Length > 0)
            {
                sql = "select s.* from dbo.State s  with (NOLOCK)  join dbo.country c on s.countryid = c.countryid where c.name = " + DB.SQuote(shippingCountry) + " order by s.DisplayOrder,s.Name";
            }
            else
            {
                sql = "select * from dbo.State   with (NOLOCK)  where countryid = 222 order by DisplayOrder,Name";
            }

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS(sql, con))
                {
                    ShippingState.DataSource = rs;
                    ShippingState.DataTextField = "Name";
                    ShippingState.DataValueField = "Abbreviation";
                    ShippingState.DataBind();
                }
            }

            if (ShippingState.Items.Count == 0)
            {
                ShippingState.Items.Insert(0, new ListItem("state.countrywithoutstates".StringResource(), "--"));
                ShippingState.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnContinueCheckout control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnContinueCheckout_Click(object sender, EventArgs e)
        {
            pnlErrorMsg.Visible = false;
            ProcessCheckOut();
        }

        /// <summary>
        /// Used to set the master page when using template switching or page-based templates
        /// </summary>
        /// <returns>
        /// The name of the template to use.  To utilize this you must override OverrideTemplate
        /// in a page that inherits from SkinBase where you're trying to change the master page
        /// </returns>
        protected override string OverrideTemplate()
        {
            String MasterHome = AppLogic.HomeTemplate();

            if (MasterHome.Trim().Length == 0)
            {

                MasterHome = "JeldWenTemplate";// "template";
            }

            if (MasterHome.EndsWith(".ascx"))
            {
                MasterHome = MasterHome.Replace(".ascx", ".master");
            }

            if (!MasterHome.EndsWith(".master", StringComparison.OrdinalIgnoreCase))
            {
                MasterHome = MasterHome + ".master";
            }

            if (!CommonLogic.FileExists(CommonLogic.SafeMapPath("~/App_Templates/Skin_" + base.SkinID.ToString() + "/" + MasterHome)))
            {
                //Change template name to JELD-WEN template by Tayyab on 07-09-2015
                MasterHome = "JeldWenTemplate";// "template.master";
            }

            return MasterHome;
        }

        /// <summary>
        /// Saves the shipment charges paid.
        /// </summary>
        /// <param name="shipmentCharges">The shipment charges.</param>
        [System.Web.Services.WebMethod]
        public static void SaveShipmentChargesPaid(String shipmentCharges)
        {
            try
            {
                shipmentChargesPaid = shipmentCharges;
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// Update the remaining funds used for shipping.
        /// </summary>
        private void FundUpdate()
        {
            if (AnyShippingMethodsFound)
            {
                if (
                    AuthenticationSSO.GetCustomerFund(ThisCustomer.CustomerID, (int) FundType.SOFFunds, false)
                        .AmountAvailable != 0 || (lastShippingMethodValue > 0 && (ThisCustomer.CustomerLevelID == 3 || ThisCustomer.CustomerLevelID == 7)))
                {
                    isBluBucks = false;
                    cbSOF.Visible = true;
                    lblSOF.Visible = true;
                    lblRemainingSOF.Visible = true;
                    lblRemainingSOFCaption.Visible = true;

                    availableFundValue =
                        AuthenticationSSO.GetCustomerFund(ThisCustomer.CustomerID, (int) FundType.SOFFunds, false)
                            .AmountAvailable;
                    usedFundValueThroughShoppingCart =
                        AuthenticationSSO.GetCustomerFund(ThisCustomer.CustomerID, (int) FundType.SOFFunds, false)
                            .AmountUsed;
                    if (lastShippingMethodValue > 0)
                    {
                        sql =
                            string.Format(
                                "update dbo.ShoppingCart set ShipmentChargesPaid={0} where CustomerID={1} and ShoppingCartRecID={2}",
                                0, ThisCustomer.CustomerID, cart.CartItems.FirstOrDefault().ShoppingCartRecordID);
                        DB.ExecuteSQL(sql);
                        availableFundValue = availableFundValue + lastShippingMethodValue;
                        usedFundValueThroughShoppingCart = usedFundValueThroughShoppingCart - lastShippingMethodValue;
                        AuthenticationSSO.UpdateCustomerFundAmountUsed(ThisCustomer.CustomerID, Convert.ToInt32(FundType.SOFFunds), Math.Round(usedFundValueThroughShoppingCart, 2));                       
                    }
                    lblRemainingSOF.Text = Math.Round(availableFundValue, 2).ToString();
                    hdnRemainingFundValue.Text = Math.Round(availableFundValue, 2).ToString();
                }
                else if (
                    AuthenticationSSO.GetCustomerFund(ThisCustomer.CustomerID, (int) FundType.BLUBucks, false)
                        .AmountAvailable != 0 || lastShippingMethodValue > 0)
                {
                    isBluBucks = true;
                    cbBluBucks.Visible = true;
                    lblBluBucks.Visible = true;
                    lblRemainingBluBucks.Visible = true;
                    lblRemainingBluBucksCaption.Visible = true;
                    availableFundValue =
                        AuthenticationSSO.GetCustomerFund(ThisCustomer.CustomerID, (int) FundType.BLUBucks, false)
                            .AmountAvailable;
                    usedFundValueThroughShoppingCart =
                        AuthenticationSSO.GetCustomerFund(ThisCustomer.CustomerID, (int) FundType.BLUBucks, false)
                            .AmountUsed;
                    if (lastShippingMethodValue > 0)
                    {
                        sql =
                            string.Format(
                                "update dbo.ShoppingCart set ShipmentChargesPaid={0} where CustomerID={1} and ShoppingCartRecID={2}",
                                0, ThisCustomer.CustomerID, cart.CartItems.FirstOrDefault().ShoppingCartRecordID);
                        DB.ExecuteSQL(sql);
                        availableFundValue = availableFundValue + lastShippingMethodValue;
                        usedFundValueThroughShoppingCart = usedFundValueThroughShoppingCart - lastShippingMethodValue;
                        AuthenticationSSO.UpdateCustomerFundAmountUsed(ThisCustomer.CustomerID, Convert.ToInt32(FundType.BLUBucks), Math.Round(usedFundValueThroughShoppingCart, 2));
                    }
                    lblRemainingBluBucks.Text = Math.Round(availableFundValue, 2).ToString();
                    hdnRemainingFundValue.Text = Math.Round(availableFundValue, 2).ToString();
                }
                hdnShippingMethod.Text = JsonConvert.SerializeObject(ShippingMethods);
            }
        }
    }

}
