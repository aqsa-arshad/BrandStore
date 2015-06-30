// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;
using Vortx.OnePageCheckout.Models;
using Vortx.OnePageCheckout.Models.PaymentMethods;
using Vortx.OnePageCheckout.Settings;
using Vortx.OnePageCheckout.Views;

namespace Vortx.OnePageCheckout
{
    public partial class OPCControls_OnePageCheckout :
        System.Web.UI.UserControl
    {
        public IModelFactory ModelFactory { get; set; }
        public IAccountModel AccountModel { get; set; }
        public IAccountView BillingAddressBookView { get; set; }
        public IShoppingCartModel ShoppingCartModel { get; set; }
        public IContentModel CustomerServiceModel { get; set; }
        public IPaymentModel PaymentModel { get; set; }
        public CreditCardPaymentModel CreditCardModel { get; set; }
        public IStringResourceProvider StringResourceProvider { get; set; }

        Customer AdnsfCustomer
        {
            get
            {
                return ((AspDotNetStorefront.SkinBase)Page).OPCCustomer;
            }
        }
        ShoppingCart AdnsfShoppingCart
        {
            get
            {
                return ((AspDotNetStorefront.SkinBase)Page).OPCShoppingCart;
            }
        }

        bool DisablePageRefreshOnAccountChange = false;
		bool CartContentsChanged = false;
        const string ACTIVE_PANEL_CSSCLASS = "active";

        #region Page Event Handlers

        protected void Page_Load(object sender, EventArgs e)
        {
            if (AdnsfShoppingCart.InventoryTrimmed)
            {
                Response.Redirect(string.Format("shoppingcart.aspx?InvTrimmed=true"));
            }

            this.ModelFactory = Vortx.OnePageCheckout.ObjectFactory.CreateModelFactory((AspDotNetStorefront.SkinBase)Page);
            ConfigurationProvider.DefaultProvider = Vortx.OnePageCheckout.ObjectFactory.CreateConfigurationFactory().CreateConfigurationProvider();

            this.Promotions.PromotionsChanged += Promotions_PromotionsChanged;
            this.CouponCode.CouponCodeChanged += CouponCode_CouponCodeChanged;

            this.StringResourceProvider = ModelFactory.CreateStringResourceProvider();			

            this.AccountModel = ModelFactory.CreateAccountModel(this.StringResourceProvider);
            this.AccountModel.CreateAccountCompleted += new CreateAccountHandler(AccountModel_CreateAccountCompleted);
            this.AccountModel.FindAccountCompleted += new FindAccountHandler(AccountModel_FindAccountCompleted);
            this.AccountModel.LogOnCompleted += new LogOnHandler(AccountModel_LogOnCompleted);
            this.AccountModel.LogOutCompleted += new LogOutHandler(AccountModel_LogOutCompleted);
            this.AccountModel.AccountChanged += new AccountChangedHandler(AccountModel_AccountChanged);
            this.AccountModel.BillingAddress.AddressChanged += new AddressChangedEventHandler(BillingAddressEditModel_AddressChanged);
            this.AccountModel.ShippingAddress.AddressChanged += new AddressChangedEventHandler(ShippingAddressEditModel_AddressChanged);
            this.AccountModel.PasswordChanged += new PasswordChangedHandler(AccountModel_PasswordChanged);
            this.AccountModel.BillingEqualsShippingChanged += new BillingEqualsShippingChangedHandler(AccountModel_BillingEqualsShippingChanged);
            this.AccountModel.RegisterCompleted += AccountModel_RegisterCompleted;

            this.LoginView.SetModel(this.AccountModel, this.StringResourceProvider);
            this.LoginView.SkipLogin += new EventHandler(LoginView_SkipLoginSelected);

            this.CreateAccountView.SetModel(this.AccountModel, this.StringResourceProvider);

            // Create shopping cart models
            this.ShoppingCartModel = ModelFactory.CreateShoppingCartModel(this.StringResourceProvider);
            this.ShoppingCartModel.AccountModel = this.AccountModel;
            this.ShoppingCartModel.ItemQuantityChanged += new ItemQuantityChangedHandler(ShoppingCartModel_ItemQuantityChanged);
            this.ShoppingCartModel.ItemRemoved += new ItemRemovedHandler(ShoppingCartModel_ItemRemoved);
            this.ShoppingCartModel.ShipMethodChanged += new ShipMethodChangedEventHandler(ShipMethodModel_ShipMethodChanged);

            // Create shopping cart views
            this.ShipMethodView.SetModel(this.ShoppingCartModel, this.StringResourceProvider);

            this.MiniCartView.SetModel(this.ShoppingCartModel, this.StringResourceProvider);

            this.MiniCartCartSummary.SetModel(this.ShoppingCartModel, this.StringResourceProvider);

            this.AddressBookView.SetModel(this.AccountModel, this.StringResourceProvider);
            this.AddressBookView.SetAddressType(Vortx.OnePageCheckout.Models.AddressType.Shipping);

            this.ShippingAddressEditView.SetModel(this.AccountModel.ShippingAddress, this.StringResourceProvider);
            this.ShippingAddressEditUKView.SetModel(this.AccountModel.ShippingAddress, this.StringResourceProvider);
            this.ShippingAddressNoZipEditView.SetModel(this.AccountModel.ShippingAddress, this.StringResourceProvider);

            this.ShippingAddressStaticView.SetModel(this.AccountModel.ShippingAddress, this.StringResourceProvider);
			this.ShippingAddressStaticView.AddressEdit += new AddressEditEventHandler(ShippingAddressStaticView_AddressEdit);

            // Create payment model
            this.PaymentModel = ModelFactory.CreatePaymentModel(this.AccountModel, this.StringResourceProvider);
            this.PaymentModel.ActivePaymentMethodChanged += new ActivePaymentMethodChangedHandler(PaymentMethodModel_ActivePaymentMethodChanged);
            this.PaymentModel.ProcessPaymentComplete += new ProcessPaymentCompleteHandler(PaymentModel_ProcessPaymentComplete);

            if (!ConfigurationProvider.DefaultProvider.ShowCreateAccount && !ConfigurationProvider.DefaultProvider.PasswordIsOptionalDuringCheckout)
                throw new Exception(StringResourceProvider.GetString("smartcheckout.aspx.129"));

            GatewayCheckoutByAmazon.CheckoutByAmazon checkoutByAmazon = new GatewayCheckoutByAmazon.CheckoutByAmazon();

            // Setup payment events
            PaymentMethodBaseModel paymentMethod = null;

			// Create payment views
			this.PaymentView.SetModel(this.PaymentModel, this.StringResourceProvider);
			
            if (checkoutByAmazon.IsEnabled && checkoutByAmazon.IsCheckingOut && (this.PaymentModel.ActivePaymentMethod == null || this.PaymentModel.ActivePaymentMethod.Name.ToLower() != PaymentType.CheckoutByAmazon.ToString().ToLower()))
            {
                this.PaymentModel.SetActivePaymentMethod(PaymentType.CheckoutByAmazon.ToString());
            }
            else
            {
                paymentMethod = this.PaymentModel.PaymentMethods.FirstOrDefault(pm => pm.Key == PaymentType.CreditCard).Value;
                if (paymentMethod != null)
                {
                    this.CreditCardModel = (CreditCardPaymentModel)paymentMethod;
                }
            }

            if (IsCheckOutByAmazon())
            {
                int shippingAddressId = 0;

                Amazon.CheckoutByAmazonService.ShippingAddress amazonShippingAddress = checkoutByAmazon.GetDefaultShippingAddress();
                if (amazonShippingAddress != null)
                {
                    shippingAddressId = checkoutByAmazon.SyncCBAAddress(AspDotNetStorefrontCore.Customer.Current.CustomerID, amazonShippingAddress);
                }

                this.AccountModel.PrimaryShippingAddressId = shippingAddressId.ToString();

                if (string.IsNullOrEmpty(this.AccountModel.PrimaryBillingAddressId) || this.AccountModel.PrimaryBillingAddressId == "0")
                {
                    this.AccountModel.PrimaryBillingAddressId = this.AccountModel.PrimaryShippingAddressId;
                }

                PaymentView.NotifyOrderDetailsChanged();
            }

			pnlPayPalIntegratedCheckout.Visible = AppLogic.AppConfigBool("PayPal.Express.UseIntegratedCheckout");

            // registered payment data changed events
            foreach (var kvp in this.PaymentModel.PaymentMethods)
            {
                var method = kvp.Value;
                method.PaymentDataChanged += new PaymentDataChangedHandler(PaymentModel_PaymentDataChanged);
            }

            this.AccountModel.PrimaryShippingAddressChanged += new PrimaryShippingAddressChangedHandler(AccountModel_PrimaryShippingAddressChanged);
            this.AccountModel.PrimaryBillingAddressChanged += new PrimaryBillingAddressChangedHandler(AccountModel_PrimaryBillingAddressChanged);

            this.LoginView.PaymentModel = this.PaymentModel;

            this.CustomerServiceModel = ModelFactory.CreateContentModel(this.StringResourceProvider);
            this.CustomerServiceModel.LoadContext(Vortx.OnePageCheckout.Settings.ConfigurationProvider.DefaultProvider.CustomerServiceTopic);
            this.CustomerServicePanel.SetModel(this.CustomerServiceModel, this.StringResourceProvider);
            this.CustomerServicePanel.Initialize();
            this.CustomerServicePanel.Show();
            this.CustomerServicePanel.BindView();

            if (ConfigurationProvider.DefaultProvider.RequireTermsAndConditions)
            {
                IContentModel termsModel = ModelFactory.CreateContentModel(this.StringResourceProvider);
                termsModel.LoadContext(ConfigurationProvider.DefaultProvider.TermsAndConditionsTopicName);
                ContentPanelTerms.SetModel(termsModel, this.StringResourceProvider);
                ContentPanelTerms.BindView();
            }

            if (!Page.IsPostBack)
            {
                this.InitializePage();

                string error = Request.QueryString["error"];
                if (error != null && error.Equals("true"))
                {
                    this.PaymentView.ShowError(StringResourceProvider.GetString("smartcheckout.aspx.130"));
                }

                Page.DataBind();
            }

            StylesheetLiteral.Text = "<link rel=\"stylesheet\" href=\"OPCControls/" + ConfigurationProvider.DefaultProvider.OPCStyleSheetName + "\" />";

            this.UpdatePanelOnePageCheckoutMain.Update();

            ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "registerBuySafeKickers", "try{WriteBuySafeKickers();}catch(err){}", true);
            if (ConfigurationProvider.DefaultProvider.BuySafeEnabled)
            {
                OPC_BuySafeWrap.Attributes.Add("style", "height:102px;");
            }
        }

        protected void Promotions_PromotionsChanged(object sender, EventArgs e)
        {
			this.SavePaymentView();			
			this.RefreshCartObject();
            this.ShowCurrentPageState();
        }

        protected void CouponCode_CouponCodeChanged(object sender, EventArgs e)
        {
			this.SavePaymentView();
			this.RefreshCartObject();
            this.ShowCurrentPageState();
        }

		protected void SubmitOrder_OnClick(object sender, EventArgs e)
        {
            if (this.ShoppingCartModel.Total > 0)
            {
                this.PaymentView.SaveViewToModel();
            }
            else
            {
                // Force CC payment on zero dollar checkout
                PaymentMethodBaseModel method = this.PaymentModel.PaymentMethods
                    .FirstOrDefault(pm => pm.Key == PaymentType.CreditCard).Value;
                this.PaymentModel.SetActivePaymentMethod(method.MethodId);
            }

            if (Page.IsValid)
            {
                this.PaymentModel.ProcessPayment();
            }
        }

        protected void EmailOptInYes_CheckedChanged(object sender, EventArgs e)
        {
            this.AccountModel.AllowEmail = EmailOptInYes.Checked;
            this.AccountModel.Save();
		}

        protected void EmailOptInNo_CheckedChanged(object sender, EventArgs e)
		{
            this.AccountModel.AllowEmail = !EmailOptInNo.Checked;
            this.AccountModel.Save();
		}

        protected void btnRefreshCBAAddress_Click(Object sender, EventArgs e)
        {
            GatewayCheckoutByAmazon.CheckoutByAmazon checkoutByAmazon = new GatewayCheckoutByAmazon.CheckoutByAmazon();
            checkoutByAmazon.BeginCheckout(new Guid(AspDotNetStorefrontCore.Customer.Current.CustomerGUID), false, false);
            this.ShowCurrentPageState();
        }

        protected void Over13Checkbox_CheckedChanged(object sender, EventArgs e)
        {
			this.AccountModel.Over13 = this.Over13Checkbox.Checked;
            ShowCurrentPageState();
        }

        protected void TermsCheckbox_CheckedChanged(object sender, EventArgs e)
        {
			this.SavePaymentView(); 
			this.AccountModel.TermsAndConditionsSelected = this.TermsCheckbox.Checked;
            ShowCurrentPageState();
        }

        #endregion

        #region Private State Control Methods

        // Steps

        /*
         * Enter Email
		 * Re-Enter Email (optional)
         * Enter Password
         * Enter Shipping Address
         * Choose Shipping Method
         * Choose Payment Method
         * Choose Email Opt In
         * Create Account
         * Submit Order
         * */

        protected void ShowCurrentPageState()
        {
            InitializePageState();

            // Login
            if (!this.AccountModel.IsRegistered && !this.AccountModel.HaveUsername)
            {
                ShowLogin();
                return;
            }

			//Re-enter email address
			if (!this.AccountModel.IsRegistered && this.AccountModel.HaveUsername && !this.AccountModel.HaveConfirmationEmail && AppLogic.AppConfigBool("RequireEmailConfirmation"))
			{
				ShowReEnterEmail();
				return;
			}

            // Enter Password
            if (this.AccountModel.AccountFound)
            {
                ShowEnterPassword();
                return;
            }

            // we are registered and logged in, but need a password change
            if (this.AccountModel.IsRegistered && this.AccountModel.PasswordChangeRequired)
            {
                this.ShowPasswordChangeRequired();
                return;
            }

            // we are logged in/guest at this point, so Edit address if needed
            if (!IsCheckOutByAmazon()
                && (this.ShoppingCartModel.ShippingRequired || this.ShoppingCartModel.ZeroCostAndNoShipping)
                && String.IsNullOrEmpty(this.AccountModel.ShippingAddress.Country))
            {
                this.ShowEditShippingAddress();
                return;
            }

            // if we require shipping, then show shipping methods
            if (!IsShippingSelected())
            {
                this.ShowChooseShippingMethod();
                return;
            }

            // If account needs to indicate over 13
            if (AccountModel.IsRegistered)
                PanelCheckboxOver13.Visible = false;

            Over13Checkbox.Checked = this.AccountModel.Over13;
            if (Over13Checkbox.Checked == false && !AccountModel.IsRegistered && ConfigurationProvider.DefaultProvider.RequireOver13Checked)
            {
                ShowOver13();
                return;
            }
            else
            {
                PanelCheckboxOver13.Enabled = true;
            }

            TermsCheckbox.Checked = this.AccountModel.TermsAndConditionsSelected;
            if (!TermsCheckbox.Checked && ConfigurationProvider.DefaultProvider.RequireTermsAndConditions)
            {
                ShowTerms();
                return;
            }
            else
            {
                PanelTerms.Enabled = true;
            }

            // If we require payment, then show payment options
            if (this.ShoppingCartModel.Total > 0)
            {
                this.PaymentView.Show();
                PanelPaymentAndBilling.Visible = true;
				
                // make sure we have a payment method
                if (this.PaymentModel.ActivePaymentMethod == null)
                {
                    this.ShowChoosePaymentMethod();
                    return;
                }

                // if the active payment method is CC and we don't have data
                if (!this.PaymentModel.ActivePaymentMethod.HavePaymentData)
                {
                    // Check if a SecureNet option has been chosen
                    if (PaymentModel.ActivePaymentMethod.PaymentType == PaymentType.CreditCard && PaymentModel.PaymentMethods.ContainsKey(PaymentType.SecureNet))
                    {
                        if (!PaymentModel.PaymentMethods[PaymentType.SecureNet].HavePaymentData)
                        {
                            this.ShowChoosePaymentMethod();
                            return;
                        }
                    }
                    // Check if an Authorize.Net CIM option has been chosen
                    else if (new AspDotNetStorefrontGateways.Processors.AuthorizeNet().IsCimEnabled == false || String.IsNullOrEmpty(AspDotNetStorefrontCore.Customer.Current.ThisCustomerSession["ActivePaymentProfileId"]))
                    {
                        this.ShowChoosePaymentMethod();
                        return;
                    }
                }
            }
            else
            {
                this.PaymentView.Hide();
                PanelPaymentAndBilling.Visible = false;
            }

            if (PaymentModel.ActivePaymentMethod is MoneybookersQuickCheckoutPaymentModel)
            {
                //CreateAccountView.ManualAccountCreationEnabled = true;
                CreateAccountView.AccountCreatedDisplayStringResource = "smartcheckout.aspx.147";
            }

            // if we are already registered, go to the submit order state
            // we check RegisterAccountSelected to make sure this customer
            // isn't already in the process of creating an account.
            if (!this.AccountModel.RegisterAccountSelected && this.AccountModel.IsRegistered)
            {
                this.ShowSubmitOrder();
                return;
            }

            // paypal guest express customers don't get to fill in extra stuff, simply complete checkout.
            if (IsPayPalExpressCheckout())
            {
                this.ShowSubmitOrder();
                return;
            }
            
            // show e-mail opt in if not selected
            if (ConfigurationProvider.DefaultProvider.ShowEmailPreferencesOnCheckout && !this.AccountModel.AllowEmailSelected)
            {
                this.ShowChooseEmailOptIn();
                return;
            }

            if (ConfigurationProvider.DefaultProvider.ShowCreateAccount && ShouldShowCreateAccount())
            {
                if (ConfigurationProvider.DefaultProvider.PasswordIsOptionalDuringCheckout)
                {
                    // if the customer has not selected yes/no to register, then ask them
                    if (!this.AccountModel.RegisterAccountSelected)
                    {
                        this.ShowCreateAccount();
                        return;
                    }

                    // if the customer has selected 'yes' to registration, then make sure they have a password
                    if (this.AccountModel.RegisterAccount && string.IsNullOrEmpty(this.AccountModel.Password))
                    {
                        this.ShowCreateAccount();
                        return;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(this.AccountModel.Password))
                    {
                        this.ShowCreateAccount();
                        return;
                    }
                }
            }

            // at this point we have all the data, so submit the order
            this.ShowSubmitOrder();
        }

        private void InitializePageState()
        {
            this.Promotions.Visible = AppLogic.AppConfigBool("Promotions.Enabled");
            this.CouponCode.Visible = AppLogic.AppConfigBool("GiftCards.Enabled");

            this.LoginView.Show();
            this.LoginView.BindView();
            this.LoginView.Disable();

            if (this.ShoppingCartModel.ShippingRequired)
            {
                this.PanelShippingMethod.Visible = true;
                this.PanelShippingAddressWrap.Visible = true;
                ShipAddressHeaderContent.Text = StringResourceProvider.GetString("smartcheckout.aspx.132");

                this.ShipMethodView.Show();
                this.ShipMethodView.BindView();
                this.ShipMethodView.Disable();
            }
            else
            {
                this.PanelShippingMethod.Visible = false;

                // New customers with zero-cost no-ship orders are still required to enter a shipping address.
                if (this.ShoppingCartModel.ZeroCostAndNoShipping && !this.AccountModel.IsRegistered)
                {
                    this.PanelShippingAddressWrap.Visible = true;
                }

				// Since we're just collecting an address, display a different label and hide delivery instructions
				this.ShipAddressHeaderContent.Text = this.StringResourceProvider.GetString("smartcheckout.aspx.154");
				this.AccountModel.ShippingAddress.ShowAddressNotes = false;

                this.ShipMethodView.Hide();
            }

            this.PaymentView.Show();
            this.PaymentView.BindView();
            this.PaymentView.Disable();

            this.ShippingAddressStaticView.Hide();
            this.ShippingAddressEditView.Hide();
            this.ShippingAddressEditUKView.Hide();
            this.ShippingAddressNoZipEditView.Hide();

            PanelTerms.Visible = ConfigurationProvider.DefaultProvider.RequireTermsAndConditions;
            PanelTerms.Enabled = false;

            PanelCheckboxOver13.Enabled = false;

			this.InitializeEmailPreference();

			if (!this.AccountModel.RegisterAccountSelected && this.AccountModel.IsRegistered)
            {
                this.CreateAccountView.Hide();
                this.PanelCreateAccount.Visible = false;

                if (this.ShoppingCartModel.ShippingRequired || this.ShoppingCartModel.ZeroCostAndNoShipping)
                {
                    this.AddressBookView.Show();
                    this.AddressBookView.BindView();
                    this.AddressBookView.Disable();

                    this.HyperLinkShippingAddressBook.Enabled = true;
                    this.HyperLinkShippingAddressBook.Visible = true;

					this.InitializeShippingAddress();
                }
            }
            else
            {
				this.InitializeShippingAddress();

                if (!ConfigurationProvider.DefaultProvider.ShowCreateAccount &&
                    ConfigurationProvider.DefaultProvider.PasswordIsOptionalDuringCheckout ||
                    !ShouldShowCreateAccount())
                {
                    this.CreateAccountView.Hide();
                    this.PanelCreateAccount.Visible = false;
                }
                else
                {
                    this.CreateAccountView.Show();
                    this.CreateAccountView.BindView();
                    this.CreateAccountView.Disable();
                    this.PanelCreateAccount.Visible = true;
                }

                this.HyperLinkShippingAddressBook.Enabled = false;
                this.HyperLinkShippingAddressBook.Visible = false;

                this.AddressBookView.Hide();

                PanelCheckboxOver13.Visible = ConfigurationProvider.DefaultProvider.RequireOver13Checked;
            }

            // Show / Hide payment screen if zero dollar order
            PanelPaymentAndBilling.Visible = this.ShoppingCartModel.Total > 0;

            this.MiniCartCartSummary.BindView();

            // if amazon payments, show widgets, and disable shipping address editor
            GatewayCheckoutByAmazon.CheckoutByAmazon checkoutByAmazon = new GatewayCheckoutByAmazon.CheckoutByAmazon();

            var amazonPaymentModel = this.PaymentModel.PaymentMethods.FirstOrDefault(pm => pm.Key == PaymentType.CheckoutByAmazon).Value;
            if (amazonPaymentModel != null)
            {
                var paymentMethod = (CheckOutByAmazonPaymentModel)amazonPaymentModel;
                LitCheckOutByAmazoneShipping.Text = paymentMethod.RenderAddressWidget(new Guid(AspDotNetStorefrontCore.Customer.Current.CustomerGUID));
                LitAmazonPaymentWidget.Text = paymentMethod.RenderWalletWidget();
            }

            if (IsCheckOutByAmazon())
            {
                PanelCheckOutByAmazonShipping.Visible = true;
                PanelShippingAddressWrap.Visible = false;
                ShippingAddressEditView.Hide();
                ShippingAddressEditUKView.Hide();
                ShippingAddressStaticView.Hide();
                this.ShippingAddressEditView.Visible = false;

                if (checkoutByAmazon.GetDefaultShippingAddress() == null)
                {
                    SubmitOrder.OnClientClick = "alert('" + this.StringResourceProvider.GetString("gw.checkoutbyamazon.display.3") + "'); return false;";
                }
            }

            this.SubmitOrder.Visible = false;
            this.SubmitOrder.Enabled = false;

            this.Promotions.Initialize();
            this.CouponCode.Initialize();

			this.InitializeEmailPreference();
        }

        private void ShowLogin()
        {
            this.LoginView.Enable();
            this.LoginView.BindView();

            this.ActivatePanelClass(PanelAccount, ACTIVE_PANEL_CSSCLASS);
        }

		private void ShowReEnterEmail()
		{
			this.LoginView.Enable();
			this.LoginView.BindView();

			this.ActivatePanelClass(PanelAccount, ACTIVE_PANEL_CSSCLASS);
		}

        private void ShowEnterPassword()
        {
            this.LoginView.Enable();
            this.LoginView.BindView();
            this.LoginView.ShowAccountFound();

            this.ActivatePanelClass(PanelAccount, ACTIVE_PANEL_CSSCLASS);
        }

        private void ShowPasswordChangeRequired()
        {
            this.LoginView.Disable();
            this.ActivatePanelClass(PanelAccount, ACTIVE_PANEL_CSSCLASS);
        }

        private void ShowEditShippingAddress()
        {
            this.ShipMethodView.Disable();
            this.PaymentView.Disable();
            this.CreateAccountView.Disable();
            this.SubmitOrder.Visible = false;
            this.EmailOptInNo.Enabled = false;
            this.EmailOptInYes.Enabled = false;

            this.ShippingAddressStaticView.Hide();
            if (ConfigurationProvider.DefaultProvider.AddressLocale.Equals("UK"))
            {
                this.ShippingAddressEditUKView.Show();
                this.ShippingAddressEditUKView.BindView();
                this.ShippingAddressEditUKView.Enable();
            }
            else
            {
                //default is US
                if (ConfigurationProvider.DefaultProvider.UseZipcodeService)
                {
                    this.ShippingAddressEditView.Show();
                    this.ShippingAddressEditView.BindView();
                    this.ShippingAddressEditView.Enable();
                }
                else
                {
                    this.ShippingAddressNoZipEditView.Show();
                    this.ShippingAddressNoZipEditView.BindView();
                    this.ShippingAddressNoZipEditView.Enable();
                }
            }

            this.ActivatePanelClass(PanelShippingAddressWrap, ACTIVE_PANEL_CSSCLASS);
        }

        private void ShowChooseShippingMethod()
        {
			if (!CartContentsChanged && Settings.AppSettingsProvider.DefaultProvider.DefaultShippingMethodId > 0 && string.IsNullOrEmpty(this.ShoppingCartModel.ShippingMethodId))
            {
                this.ShoppingCartModel.ShippingMethodId = Settings.AppSettingsProvider.DefaultProvider.DefaultShippingMethodId.ToString();
                return;
            }

            this.ActivatePanelClass(PanelShippingMethod, ACTIVE_PANEL_CSSCLASS);
            this.ShippingAddressStaticView.Enable();
            this.ShipMethodView.Enable();
        }

        private void ShowOver13()
        {
            ShippingAddressStaticView.Enable();
            ShipMethodView.Enable();
            PanelCheckboxOver13.Enabled = true;
            ActivatePanelClass(PanelCheckboxOver13, ACTIVE_PANEL_CSSCLASS);
        }

        private void ShowTerms()
        {
            ShippingAddressStaticView.Enable();
            ShipMethodView.Enable();
            PanelTerms.Enabled = true;
            ActivatePanelClass(PanelTerms, ACTIVE_PANEL_CSSCLASS);
        }

        private void ShowChoosePaymentMethod()
        {
            this.ActivatePanelClass(PanelPaymentAndBilling, ACTIVE_PANEL_CSSCLASS);
            this.ShippingAddressStaticView.Enable();
            this.ShipMethodView.Enable();
            this.PaymentView.Enable();
            this.EmailOptInNo.Enabled = false;
            this.EmailOptInYes.Enabled = false;
        }

        private void ShowChooseEmailOptIn()
        {
            this.ShippingAddressStaticView.Enable();
            this.ShipMethodView.Enable();
            this.PaymentView.Enable();

            if (this.ShoppingCartModel.Total > 0 &&
                this.PaymentModel.ActivePaymentMethod.PaymentType == PaymentType.CreditCard &&
                string.IsNullOrEmpty(this.CreditCardModel.CVV2) &&
                !IsTwoCheckoutPayment() &&
                !IsWorldPayPayment())
            {
                this.ActivatePanelClass(PanelPaymentAndBilling, ACTIVE_PANEL_CSSCLASS);
            }
            else
            {
                this.EmailOptInYes.Enabled = true;
                this.EmailOptInNo.Enabled = true;

                this.ActivatePanelClass(PanelEmailOptIn, ACTIVE_PANEL_CSSCLASS);
                if (this.ShoppingCartModel.Total == 0)
                    this.PaymentView.Hide();
            }
        }

        private void ShowCreateAccount()
        {
            this.ShippingAddressStaticView.Enable();
            this.ShipMethodView.Enable();
            this.PaymentView.Enable();

			EmailOptInNo.Enabled = true;
			EmailOptInYes.Enabled = true;
            EmailOptInNo.Checked = !this.AccountModel.AllowEmail;
            EmailOptInYes.Checked = this.AccountModel.AllowEmail;
            
            if (this.ShoppingCartModel.Total > 0 &&
                this.PaymentModel.ActivePaymentMethod.PaymentType == PaymentType.CreditCard &&
                string.IsNullOrEmpty(this.CreditCardModel.CVV2) &&
                !IsTwoCheckoutPayment() &&
                !IsWorldPayPayment())
            {
                this.ActivatePanelClass(PanelPaymentAndBilling, ACTIVE_PANEL_CSSCLASS);
            }
            else
            {
                this.ActivatePanelClass(PanelCreateAccount, ACTIVE_PANEL_CSSCLASS);

                this.CreateAccountView.Show();
                this.CreateAccountView.BindView();
                this.CreateAccountView.Enable();
            }
        }

        private void ShowSubmitOrder()
        {
            ActivatePanelClass(PanelSubmit, ACTIVE_PANEL_CSSCLASS);

            this.ShippingAddressStaticView.Enable();
            this.ShipMethodView.Enable();
            this.PaymentView.Enable();

			EmailOptInNo.Enabled = true;
			EmailOptInYes.Enabled = true;
			EmailOptInNo.Checked = !this.AccountModel.AllowEmail;
			EmailOptInYes.Checked = this.AccountModel.AllowEmail;

            if (!this.AccountModel.IsRegistered)
            {
                this.CreateAccountView.Show();
                this.CreateAccountView.BindView();
                this.CreateAccountView.Enable();
            }

            if (IsMoneybookersQuickCheckout())
            {
                PanelCompleteMoneybookersQuickCheckout.Visible = true;
                SubmitOrder.Visible = false;
                return;
            }

            if (IsPayPalEmbeddedCheckout())
            {
                SubmitOrder.Visible = false;
                return;
            }

            this.SubmitOrder.Visible = true;
            this.SubmitOrder.Enabled = true;

            this.UpdatePanelOnePageCheckoutMain.Update();
        }

        private void InitializePage()
        {
            // Initialize views
            this.LoginView.Initialize();
            this.AddressBookView.Initialize();

            if (ConfigurationProvider.DefaultProvider.AddressLocale.Equals("UK"))
            {
                this.ShippingAddressEditUKView.Initialize();
                this.ShippingAddressEditView.Hide();
                this.ShippingAddressNoZipEditView.Hide();
            }
            else
            {
                this.ShippingAddressEditUKView.Hide();
                if (ConfigurationProvider.DefaultProvider.UseZipcodeService)
                {
                    this.ShippingAddressEditView.Initialize();
                    this.ShippingAddressNoZipEditView.Hide();
                }
                else
                {
                    this.ShippingAddressEditView.Hide();
                    this.ShippingAddressNoZipEditView.Initialize();
                }
            }

            this.ShippingAddressStaticView.Initialize();
            this.ShipMethodView.Initialize();
            this.PaymentView.Initialize();
            this.CreateAccountView.Initialize();

            this.MiniCartView.Initialize();
            this.MiniCartView.Show();
            this.MiniCartView.BindView();

            this.MiniCartCartSummary.Initialize();
            this.MiniCartCartSummary.Show();
            this.MiniCartCartSummary.BindView();

            // Refresh page state
            ShowCurrentPageState();
        }

        private void ActivatePanelClass(Panel panelActive, string className)
        {
            // remove class from other panels
            WebUtility.PageUtility.RemoveClass(PanelAccount, className);
            WebUtility.PageUtility.RemoveClass(PanelShippingAddressWrap, className);
            WebUtility.PageUtility.RemoveClass(PanelShippingMethod, className);
            WebUtility.PageUtility.RemoveClass(PanelCheckboxOver13, className);
            WebUtility.PageUtility.RemoveClass(PanelTerms, className);
            WebUtility.PageUtility.RemoveClass(PanelPaymentAndBilling, className);
            WebUtility.PageUtility.RemoveClass(PanelCreateAccount, className);
            WebUtility.PageUtility.RemoveClass(PanelSubmit, className);
            WebUtility.PageUtility.RemoveClass(PanelEmailOptIn, className);

            WebUtility.PageUtility.AddClass(panelActive, className);
        }

        #endregion

        #region Model Events

        protected void AccountModel_BillingEqualsShippingChanged(object source, EventArgs args)
        {
			// refresh the addresses
            ConfigureAddresses();

			// refresh the payment model, because the address data changed, and payment details are stored on the address records
			PaymentView.SetModel(this.PaymentModel, this.StringResourceProvider);

			ShowCurrentPageState();
		}

        protected void AccountModel_PrimaryBillingAddressChanged(object sender, EventArgs args)
        {            
            ShowCurrentPageState();
        }

        protected void AccountModel_PrimaryShippingAddressChanged(object sender, EventArgs args)
        {
            // no need to refresh page state, because the ShippingMethodChanged event will fire

			// invalidate selected shipping method
            if (Settings.AppSettingsProvider.DefaultProvider.DefaultShippingMethodId > 0)
            {
                this.ShoppingCartModel.ShippingMethodId = Settings.AppSettingsProvider.DefaultProvider.DefaultShippingMethodId.ToString();
            }
            else
            {
                this.ShoppingCartModel.ShippingMethodId = string.Empty;
            }
        }
		
		protected void ShippingAddressEditModel_AddressChanged(object sender, AddressChangedEventArgs addressChanged)
		{
			PaymentView.NotifyOrderDetailsChanged();
		}

		protected void BillingAddressEditModel_AddressChanged(object sender, AddressChangedEventArgs addressChanged)
		{
			PaymentView.NotifyOrderDetailsChanged();
		}
		
        protected void AccountModel_LogOutCompleted(object sender, EventArgs args)
        {
			this.PaymentModel.NotifyContext(this.AdnsfCustomer, this.AdnsfShoppingCart);

			this.ShoppingCartModel.ShippingMethodId = string.Empty;
			
			// Refresh page state
            this.ShowCurrentPageState();
        }

        protected void AccountModel_LogOnCompleted(object sender, LogOnEventArgs args)
        {
            if (args.Successful)
            {
                // Redirect to smartcheckout to refresh context
                Response.Redirect("~/smartcheckout.aspx");
            }

            // show login state
            this.ShowLogin();

            // Set the login view state
            if (String.IsNullOrEmpty(args.ErrorMessage))
            {
                this.LoginView.BindView(false);
            }
            else
            {
                this.LoginView.ShowError(args.ErrorMessage);
            }
        }

        protected void AccountModel_CreateAccountCompleted(object sender, CreateAccountEventArgs args)
        {
            if (args.AlreadyExists || !args.Successful)
            {
                this.AccountModel.Username = string.Empty;
                this.AccountModel.Password = string.Empty;

                // Refresh page state
                this.ShowCurrentPageState();

                this.LoginView.ShowError(args.ErrorMessage);
            }
            else if (args.Successful)
            {
                // Refresh page state
                this.ShowCurrentPageState();
            }
        }

        protected void AccountModel_RegisterCompleted(object sender, EventArgs args)
        {
			this.SavePaymentView(); 
			this.ShowCurrentPageState();
        }

        protected void AccountModel_FindAccountCompleted(object sender, FindAccountEventArgs args)
        {
            if (args.Successful)
            {
                // need to enter password now, because we found the account
                this.ShowEnterPassword();
            }
            else
            {
                CreateGuestAccount();
            }
        }

        protected void AccountModel_AccountChanged(object sender, EventArgs args)
        {
            // Refresh page state
            if (!DisablePageRefreshOnAccountChange)
			{
				this.SavePaymentView();

                this.ShowCurrentPageState();
            }
        }

        protected void AccountModel_PasswordChanged(object sender, PasswordChangedEventArgs args)
        {
            if (args.Successful)
            {
                // Refresh page state
                this.ShowCurrentPageState();
            }
        }

        protected void ShoppingCartModel_ItemRemoved(object source, EventArgs args)
        {
			this.CartContentsChanged = true;
			this.ShipMethodView.CartContentsChanged = true; 
            if (this.ShoppingCartModel.ShoppingCartItems.Count() <= 0)
            {
                Response.Redirect("~/shoppingcart.aspx");
            }
			this.SavePaymentView();
            this.PaymentView.NotifyOrderDetailsChanged();
            this.MiniCartCartSummary.BindView();
            this.MiniCartView.BindView();			
			this.ShoppingCartModel.ShippingMethodId = string.Empty;			
        }

        protected void ShoppingCartModel_ItemQuantityChanged(object source, EventArgs args)
		{
			this.CartContentsChanged = true;
			this.ShipMethodView.CartContentsChanged = true;
			if (this.ShoppingCartModel.ShoppingCartItems.Count() <= 0)
            {
                Response.Redirect("~/shoppingcart.aspx");
            }
			this.SavePaymentView();
            this.PaymentView.NotifyOrderDetailsChanged();			
			this.ShoppingCartModel.ShippingMethodId = string.Empty;
        }

        protected void ShipMethodModel_ShipMethodChanged(object sender, EventArgs eventArgs)
        {
			this.SavePaymentView();

			this.RefreshCartObject();			

			this.PaymentView.NotifyOrderDetailsChanged();
			
            this.ShowCurrentPageState();

            this.MiniCartCartSummary.BindView();

            this.MiniCartView.BindView();
        }

        protected void PaymentMethodModel_ActivePaymentMethodChanged(object source, EventArgs args)
        {
            if (!(PaymentModel.ActivePaymentMethod is Vortx.OnePageCheckout.Models.PaymentMethods.Adnsf9200.CheckOutByAmazonPayment))
            {
                GatewayCheckoutByAmazon.CheckoutByAmazon checkoutByAmazon = new GatewayCheckoutByAmazon.CheckoutByAmazon();
                checkoutByAmazon.ResetCheckout(AspDotNetStorefrontCore.Customer.Current.CustomerID);
            }
            else
            {
                PanelCheckOutByAmazonShipping.Visible = true;
                LitAmazonPaymentWidget.Visible = true;
            }
            
			ShowCurrentPageState();
        }

        protected void PaymentModel_ProcessPaymentComplete(object source, ProcessPaymentCompleteEventArgs args)
        {
            if (args.Success)
            {
                // reset T&C after an order successfully completes
                this.AccountModel.TermsAndConditionsSelected = false;

                this.PaymentView.BindView(args.OrderNumber);
            }
            else if (args.Message != null && args.Message.Equals(AspDotNetStorefrontCore.AppLogic.ro_CardinalCommerce))
            {
                Response.Redirect("cardinalform.aspx");// this will eventually come "back" to us in cardinal_process.aspx after going through banking system pages
            }
            else if (args.Message != null && args.Message.Equals(ConfigurationProvider.DefaultProvider.ro_3DSecure))
            {
                Response.Redirect("secureform.aspx");
            }
            else
            {
                this.PaymentView.ShowError(args.Message);
            }
        }

        protected void PaymentModel_PaymentDataChanged(object source, EventArgs args)
        {
            this.ShowCurrentPageState();
        }

        #endregion

        #region View Events

        protected void LoginView_SkipLoginSelected(object sender, EventArgs args)
        {
            CreateGuestAccount();
        }

        protected void ShippingAddressStaticView_AddressEdit(object sender, EventArgs args)
        {
            // edit the shipping address
            this.ShowEditShippingAddress();
        }

        #endregion

        #region Private Methods

        void ConfigurePrimaryBillingAddress()
        {
			IAddressModel address = this.AccountModel.AddressBook
                        .FirstOrDefault(abm => abm.AddressId != this.AccountModel.PrimaryShippingAddressId);
            if (address != null)
            {
                // set the billing address id to the first non-shipping address found.                    
                this.AccountModel.PrimaryBillingAddressId = address.AddressId;
            }
			else
			{
				this.AccountModel.PrimaryBillingAddressId = "0";
			}
		}

        void ConfigureAddresses()
        {
            if (this.ShoppingCartModel.ShippingRequired)
            {
                // if billing equals shipping then set them to the same address
                if (this.AccountModel.BillingEqualsShipping)
                {
                    this.AccountModel.PrimaryBillingAddressId = this.AccountModel.PrimaryShippingAddressId;
                }
                else // otherwise select a billing address
                {
                    ConfigurePrimaryBillingAddress();
                }
            }
            else
            {
                // if shipping isn't required, make sure we have a billing address anyway.
                if (this.AccountModel.PrimaryShippingAddressId == "0")
                {
                    ConfigurePrimaryBillingAddress();
                }

                // make sure there is a shipping address id, even if shipping isn't required.
                // Some payment methods require shipping address details
                this.AccountModel.PrimaryShippingAddressId = this.AccountModel.PrimaryBillingAddressId;
            }
        }

        bool IsShortenedCheckout()
        {
            return IsPayPalExpressCheckout() || IsCheckOutByAmazon();
        }

        bool IsPayPalExpressCheckout()
        {
            var payPalExpressCheckout = false;
            if (this.PaymentModel.ActivePaymentMethod != null)
            {
                var paymentType = this.PaymentModel.ActivePaymentMethod.PaymentType;
                if (paymentType == PaymentType.PayPalExpress)
                {
                    payPalExpressCheckout = ((PaypalExpressPaymentModel)this.PaymentModel.ActivePaymentMethod).ExpressLoginComplete;
                }
            }
            return payPalExpressCheckout;
        }

        bool IsCheckOutByAmazon()
        {
            return new GatewayCheckoutByAmazon.CheckoutByAmazon().IsCheckingOut;
        }

        bool IsMoneybookersQuickCheckout()
        {
            if (PaymentModel.ActivePaymentMethod == null)
                return false;

            return PaymentModel.ActivePaymentMethod.PaymentType == PaymentType.MoneybookersQuickCheckout;
        }

        bool IsPayPalEmbeddedCheckout()
        {
            if (PaymentModel.ActivePaymentMethod == null)
                return false;

            return PaymentModel.ActivePaymentMethod.PaymentType == PaymentType.PayPalEmbeddedCheckout;
        }

        void ResetMoneybookersQuickCheckout()
        {
            if (PaymentModel == null || !(PaymentModel.ActivePaymentMethod is MoneybookersQuickCheckoutPaymentModel))
                return;
        }

        void CreateGuestAccount()
        {
            this.AccountModel.Over13 = Settings.ConfigurationProvider.DefaultProvider.RequireOver13Checked ? false : true;
            this.AccountModel.Email = this.AccountModel.Username;
            this.AccountModel.PersistLogOn = true;
            this.AccountModel.FirstName = String.Empty;
            this.AccountModel.LastName = String.Empty;

            // Create an anon account
            this.AccountModel.CreateAccount();
        }

        void RefreshCartObject()
        {
            var skinBase = ((AspDotNetStorefront.SkinBase)Page);
            skinBase.RefreshCart();

            var customer = skinBase.OPCCustomer;
            var cart = skinBase.OPCShoppingCart;

            this.ShoppingCartModel.NotifyContext(customer, cart);
            this.AccountModel.NotifyContext(customer, cart);
            this.PaymentModel.NotifyContext(customer, cart);
        }

        bool IsShippingSelected()
        {
            bool isShippingSelected = false;
			if (!this.ShoppingCartModel.ShippingRequired)
			{
				isShippingSelected = true;
			}
			// If shipping is required and shipping method ID is not empty
			else if (!String.IsNullOrEmpty(this.ShoppingCartModel.ShippingMethodId))
			{
				// if shipping is free, but customer cannot choose the method, then move forward
				if (AdnsfShoppingCart.ShippingIsFree && !AppLogic.AppConfigBool("FreeShippingAllowsRateSelection"))
				{
					isShippingSelected = true;
				}

				// otherwise, make sure the selected shipping method exists in the available methods
				if (this.ShoppingCartModel.ShippingMethods.Any(sm => sm.Identifier == this.ShoppingCartModel.ShippingMethodId))
				{
					isShippingSelected = true;
				}
			}
			else if (this.AdnsfShoppingCart.ShipCalcID == Shipping.ShippingCalculationEnum.UseRealTimeRates 
					&& !String.IsNullOrEmpty(this.ShoppingCartModel.LastShippingMethodError)
					&& AppLogic.AppConfigBool("Vortx.OnePageCheckout.AllowRTShipping.NoMethodSelected"))
			{
				isShippingSelected = true;
			}

            return isShippingSelected;
        }
                
        bool IsTwoCheckoutPayment()
        {
            if (this.PaymentModel.ActivePaymentMethod != null && this.PaymentModel.ActivePaymentMethod.PaymentType == PaymentType.CreditCard)
            {
                if (AspDotNetStorefrontCore.AppLogic.ActivePaymentGatewayCleaned() ==
                    AspDotNetStorefrontGateways.Gateway.ro_GWTWOCHECKOUT)
                {
                    return true;
                }
            }
            return false;
        }

        bool IsWorldPayPayment()
        {
            if (this.PaymentModel.ActivePaymentMethod != null && this.PaymentModel.ActivePaymentMethod.PaymentType == PaymentType.CreditCard)
            {
                if (AspDotNetStorefrontCore.AppLogic.ActivePaymentGatewayCleaned() ==
                    (new AspDotNetStorefrontGateways.Processors.Worldpay()).GatewayIdentifier)
                {
                    return true;
                }
            }
            return false;
        }

        bool ShouldShowCreateAccount()
        {
            // always show create account if we allow duplicates
            if (AppLogic.GlobalConfigBool("AllowCustomerDuplicateEMailAddresses"))
                return true;

            // skip create account if this email is already in use and we allow already registered email for guest checkout            
            if (AppLogic.GlobalConfigBool("Anonymous.AllowAlreadyRegisteredEmail"))            
                return !Customer.EmailInUse(this.AccountModel.Username, int.Parse(this.AccountModel.AccountId), true);
            else
                return true; // this scenario should never happen because the check for duplicate
            //email addresses happens earlier in the process when duplicates are not allowed
        }

		void SavePaymentView()
		{
			if (this.PaymentModel.ActivePaymentMethod != null && 
				this.PaymentModel.ActivePaymentMethod.PaymentType == PaymentType.CreditCard &&
				!string.IsNullOrEmpty(((CreditCardPaymentModel)this.PaymentModel.ActivePaymentMethod).CCNumber))
			{
				if (this.ShoppingCartModel.Total > 0)
				{
					this.PaymentView.SaveViewToModel();
				}
			}
		}

		void InitializeEmailPreference()
		{
			if (!this.AccountModel.RegisterAccountSelected && this.AccountModel.IsRegistered)
			{
				this.PanelEmailOptIn.Visible = false;
			}
			else
			{
				litEmailPrefYes.Text = this.StringResourceProvider.GetString("smartcheckout.aspx.4");
				litEmailPrefNo.Text = this.StringResourceProvider.GetString("smartcheckout.aspx.5");

				this.EmailOptInYes.Enabled = false;
				this.EmailOptInNo.Enabled = false;
				this.EmailOptInYes.Checked = this.AccountModel.AllowEmailSelected && this.AccountModel.AllowEmail;
				this.EmailOptInNo.Checked = this.AccountModel.AllowEmailSelected && !this.AccountModel.AllowEmail;
				this.PanelEmailOptIn.Visible = ConfigurationProvider.DefaultProvider.ShowEmailPreferencesOnCheckout;
			}
		}

		void InitializeShippingAddress()
		{
			if (this.ShoppingCartModel.ShippingRequired || this.ShoppingCartModel.ZeroCostAndNoShipping)
			{
				if (string.IsNullOrEmpty(this.AccountModel.ShippingAddress.Country))
				{
					if (ConfigurationProvider.DefaultProvider.AddressLocale.Equals("UK"))
					{
						this.ShippingAddressEditUKView.Initialize();
						this.ShippingAddressEditUKView.Show();
						this.ShippingAddressEditUKView.BindView();
						this.ShippingAddressEditUKView.Disable();
					}
					else
					{
						if (ConfigurationProvider.DefaultProvider.UseZipcodeService)
						{
							this.ShippingAddressEditView.Initialize();
							this.ShippingAddressEditView.Show();
							this.ShippingAddressEditView.BindView();
							this.ShippingAddressEditView.Disable();
						}
						else
						{
							this.ShippingAddressNoZipEditView.Initialize();
							this.ShippingAddressNoZipEditView.Show();
							this.ShippingAddressNoZipEditView.BindView();
							this.ShippingAddressNoZipEditView.Disable();
						}
					}
				}
				else
				{
					this.ShippingAddressStaticView.Show();
					this.ShippingAddressStaticView.BindView();
					this.ShippingAddressStaticView.Disable();
				}
			}
		}

        #endregion
    }
}
