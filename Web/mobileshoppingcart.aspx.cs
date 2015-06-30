// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using AspDotNetStorefront.Promotions;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways;
using Vortx.MobileFramework;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for ShoppingCartPage.
    /// </summary>
    public partial class MobileShoppingCartPage : SkinBase
    {
        private ErrorMessage _errorMessage;
        private ErrorMessage errorMessage
        {
            get 
            {
                if (_errorMessage == null)
                    _errorMessage = new ErrorMessage(CommonLogic.QueryStringNativeInt("ErrorMsg"));
                
                return _errorMessage;
            }
        }

        private ShoppingCart _cart = null;
        private ShoppingCart cart
        {
            get { return _cart; }
            set 
            { 
                _cart = value;
                InventoryTrimmedEarly = _cart.InventoryTrimmed || InventoryTrimmedEarly;
                if (_cart.TrimmedReason != InventoryTrimmedReason.None)
                    TrimmedEarlyReason = _cart.TrimmedReason;
            }
        }
        bool VATEnabled = false;
        bool VATOn = false;
        int CountryID = 0;
        int StateID = 0;
        string ZipCode = string.Empty;

        protected void Page_Load(object sender, System.EventArgs e)
        {
            MobileHelper.RedirectPageWhenMobileIsDisabled("~/shoppingcart.aspx", ThisCustomer);

            this.RequireCustomerRecord();
            RequireSecurePage();
            SectionTitle = AppLogic.GetString("AppConfig.CartPrompt", SkinID, ThisCustomer.LocaleSetting);
            ClearErrors();

            if (cart.InventoryTrimmed || this.InventoryTrimmed)
                HandleInventoryTrimmed();

            if (this.cart.CartItems.Select(ci => ci.ShippingAddressID).Distinct().Count() > 1)
			{
                this.cart.ApplyShippingRules();
				UpdateCart();
			}
            this.cart.ConsolidateCartItems();

            VATEnabled = AppLogic.AppConfigBool("VAT.Enabled");
            VATOn = (VATEnabled && ThisCustomer.VATSettingReconciled == VATSettingEnum.ShowPricesInclusiveOfVAT);

            if (VATEnabled)
            {
                CountryID = AppLogic.AppConfigUSInt("VAT.CountryID");
                StateID = 0;
                ZipCode = string.Empty;

                if (ThisCustomer.IsRegistered)
                {
                    if (ThisCustomer.PrimaryShippingAddress.CountryID > 0)
                    {
                        CountryID = ThisCustomer.PrimaryShippingAddress.CountryID;
                    }
                    if (ThisCustomer.PrimaryShippingAddress.StateID > 0)
                    {
                        StateID = ThisCustomer.PrimaryShippingAddress.StateID;
                    }
                    if (ThisCustomer.PrimaryShippingAddress.Zip.Trim().Length != 0)
                    {
                        ZipCode = ThisCustomer.PrimaryShippingAddress.Zip.Trim();
                    }
                }
            }

            if (!this.IsPostBack)
            {
                string ReturnURL = CommonLogic.QueryStringCanBeDangerousContent("ReturnUrl");
                AppLogic.CheckForScriptTag(ReturnURL);
                ViewState["ReturnURL"] = ReturnURL;
                InitializePageContent();
            }
            else
            {
                pnlOrderOptions.Visible = !cart.IsEmpty();
                pnlCoupon.Visible = !cart.IsEmpty() && cart.GiftCardsEnabled;
                pnlPromotion.Visible = !cart.IsEmpty() && cart.PromotionsEnabled;
                pnlOrderNotes.Visible = !AppLogic.AppConfigBool("DisallowOrderNotes") && !cart.IsEmpty();
                btnCheckOutNowBottom.Visible = !cart.IsEmpty();
                btnCheckOutNowTop.Visible = !cart.IsEmpty();
            }
            String CurrentCoupon = String.Empty;
            if (cart.Coupon.CouponCode.Length != 0 && String.IsNullOrEmpty(CouponCode.Text))
                CurrentCoupon = cart.Coupon.CouponCode;
            else if (cart.CartItems.CouponList.Count == 1)
                CurrentCoupon = cart.CartItems.CouponList[0].CouponCode;

            if (CurrentCoupon.Length > 0 && CouponCode.Text.Length == 0)
                CouponCode.Text = CurrentCoupon;

            btnRemoveCoupon.Visible = CouponCode.Text.Length != 0;
            
            lblPromotionError.Text = String.Empty;
            if (!IsPostBack)
                BindPromotions();
        }

        #region Web Form Designer generated code
        override protected void OnInit(EventArgs e)
        {
            //
            // CODEGEN: This call is required by the ASP.NET Web Form Designer.
            //
            InitializeComponent();
            InitializeShoppingCartControl();
            InitializeOrderOptionControl();

            base.OnInit(e);
        }

        private void InitializeShoppingCartControl()
        {
            cart = new ShoppingCart(SkinID, ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);

            ctrlShoppingCart.DataSource = cart.CartItems;
            ctrlShoppingCart.DataBind();

            ctrlCartSummary.DataSource = cart;
            ctrlCartSummary.DataBind();
        }

        /// <summary>
        /// Initializes the order option control
        /// </summary>
        private void InitializeOrderOptionControl()
        {
            ctrlOrderOption.ThisCustomer = ThisCustomer;
            ctrlOrderOption.EditMode = true;
            ctrlOrderOption.Datasource = cart;

            if (cart.AllOrderOptions.Count > 0)
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnContinueShoppingTop.Click += new EventHandler(btnContinueShoppingTop_Click);
            btnContinueShoppingBottom.Click += new EventHandler(btnContinueShoppingBottom_Click);
            btnCheckOutNowTop.Click += new EventHandler(btnCheckOutNowTop_Click);
            btnCheckOutNowBottom.Click += new EventHandler(btnCheckOutNowBottom_Click);
            btnUpdateCart1.Click += UpdateCart_Clicked;
            btnUpdateCart2.Click += UpdateCart_Clicked;
            btnUpdateCart3.Click += UpdateCart_Clicked;
			btnUpdateCart4.Click += UpdateCart_Clicked;
		}
        #endregion

        void btnContinueShoppingTop_Click(object sender, EventArgs e)
        {
            ContinueShopping();
        }

        void btnContinueShoppingBottom_Click(object sender, EventArgs e)
        {
            ContinueShopping();
        }

        void btnCheckOutNowTop_Click(object sender, EventArgs e)
        {
            UpdateCartQuantity();
            ProcessCart(true, false, false);
        }

        void btnCheckOutNowBottom_Click(object sender, EventArgs e)
        {
            UpdateCartQuantity();
            ProcessCart(true, false, false);
        }

        void btnInternationalCheckOutNowTop_Click(object sender, EventArgs e)
        {
            ProcessCart(true, false, true);
        }

        void btnInternationalCheckOutNowBottom_Click(object sender, EventArgs e)
        {
            ProcessCart(true, false, true);
        }

        void btnQuickCheckoutTop_Click(object sender, EventArgs e)
        {
            ProcessCart(true, true, false);
        }

        void btnQuickCheckoutBottom_Click(object sender, EventArgs e)
        {
            ProcessCart(true, true, false);
        }

        private bool InventoryTrimmedEarly = false;

        private bool InventoryTrimmed
        {
            get
            {
                if (InventoryTrimmedEarly)
                    return true;
                else if (Request.QueryString["InvTrimmed"] != null)
                    return CommonLogic.QueryStringBool("InvTrimmed");
                else
                    return false;
            }
            
        }

        private InventoryTrimmedReason TrimmedEarlyReason;

		protected void UpdateCart_Clicked(object sender, EventArgs e)
		{
			UpdateCart();
		}

        private void UpdateCart()
        {
            if (cart.InventoryTrimmed || this.InventoryTrimmed)
            {
                HandleInventoryTrimmed();

                cart = new ShoppingCart(SkinID, ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);
                ctrlCartSummary.DataSource = cart;
                Response.Redirect(string.Format("shoppingcart.aspx?InvTrimmed=true"));
            }
            cart = new ShoppingCart(SkinID, ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);
            cart.SetCoupon(CouponCode.Text.ToUpperInvariant(), true);

			foreach(string code in txtPromotionCode.Text.Split(' '))
				PromotionManager.AssignPromotion(ThisCustomer.CustomerID, code);

            UpdateCartQuantity();
            ctrlOrderOption.UpdateChanges();
            ProcessCart(false, false, false);
			InitializePageContent();
            InitializeShoppingCartControl();
        }

        public void InitializePageContent()
        {
            int AgeCartDays = AppLogic.AppConfigUSInt("AgeCartDays");
            if (AgeCartDays == 0)
                AgeCartDays = 7;

            ShoppingCart.Age(ThisCustomer.CustomerID, AgeCartDays, CartTypeEnum.ShoppingCart);

            cart = new ShoppingCart(SkinID, ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);
            shoppingcartaspx8.Text = AppLogic.GetString("shoppingcart.aspx.8", SkinID, ThisCustomer.LocaleSetting);
            shoppingcartaspx10.Text = AppLogic.GetString("shoppingcart.aspx.10", SkinID, ThisCustomer.LocaleSetting);
            shoppingcartaspx9.Text = AppLogic.GetString("shoppingcart.aspx.9", SkinID, ThisCustomer.LocaleSetting);

            shoppingcartcs31.Text = AppLogic.GetString("shoppingcart.cs.117", SkinID, ThisCustomer.LocaleSetting);
            btnUpdateCart1.Text = AppLogic.GetString("shoppingcart.cs.110", SkinID, ThisCustomer.LocaleSetting);
            btnUpdateCart2.Text = AppLogic.GetString("shoppingcart.cs.110", SkinID, ThisCustomer.LocaleSetting);
            btnUpdateCart3.Text = AppLogic.GetString("shoppingcart.cs.110", SkinID, ThisCustomer.LocaleSetting);
            btnUpdateCart4.Text = AppLogic.GetString("shoppingcart.cs.110", SkinID, ThisCustomer.LocaleSetting);
            lblOrderNotes.Text = AppLogic.GetString("shoppingcart.cs.66", SkinID, ThisCustomer.LocaleSetting);
            btnContinueShoppingTop.Text = AppLogic.GetString("shoppingcart.cs.62", SkinID, ThisCustomer.LocaleSetting);
            btnContinueShoppingBottom.Text = AppLogic.GetString("shoppingcart.cs.62", SkinID, ThisCustomer.LocaleSetting);
            btnCheckOutNowTop.Text = AppLogic.GetString("shoppingcart.cs.111", SkinID, ThisCustomer.LocaleSetting);
            btnCheckOutNowBottom.Text = AppLogic.GetString("shoppingcart.cs.111", SkinID, ThisCustomer.LocaleSetting);

            bool reqOver13 = AppLogic.AppConfigBool("RequireOver13Checked");
            btnCheckOutNowTop.Enabled = !cart.IsEmpty() && !cart.RecurringScheduleConflict && (!reqOver13 || (reqOver13 && ThisCustomer.IsOver13)) || !ThisCustomer.IsRegistered;
            if (btnCheckOutNowTop.Enabled && AppLogic.MicropayIsEnabled()
                && !AppLogic.AppConfigBool("MicroPay.HideOnCartPage") && cart.CartItems.Count == 1
                && cart.HasMicropayProduct() && ((CartItem)cart.CartItems[0]).Quantity == 0)
            {
                // We have only one item and it is the Micropay Product and the Quantity is zero
                // Don't allow checkout
                btnCheckOutNowTop.Enabled = false;
            }
            ErrorMsgLabel.Text = CommonLogic.IIF(!cart.IsEmpty() && (reqOver13 && !ThisCustomer.IsOver13 && ThisCustomer.IsRegistered), AppLogic.GetString("Over13OnCheckout", SkinID, ThisCustomer.LocaleSetting), String.Empty);

			btnCheckOutNowBottom.Enabled = btnCheckOutNowTop.Enabled;

            PayPalExpressSpan.Visible = false;
            PayPalExpressSpan2.Visible = false;

            Decimal MinOrderAmount = AppLogic.AppConfigUSDecimal("CartMinOrderAmount");

            if (!cart.IsEmpty() && !cart.ContainsRecurringAutoShip)
            {
                // Enable PayPalExpress if using PayPalPro or PayPal Express is an active payment method.
                bool IncludePayPalExpress = false;

                if (AppLogic.AppConfigBool("PayPal.Express.ShowOnCartPage") && cart.MeetsMinimumOrderAmount(MinOrderAmount))
                {
                    if (AppLogic.ActivePaymentGatewayCleaned() == Gateway.ro_GWPAYPALPRO)
                    {
                        IncludePayPalExpress = true;
                    }
                    else
                    {
                        foreach (String PM in AppLogic.AppConfig("PaymentMethods").ToUpperInvariant().Split(','))
                        {
                            String PMCleaned = AppLogic.CleanPaymentMethod(PM);
                            if (PMCleaned == AppLogic.ro_PMPayPalExpress)
                            {
                                IncludePayPalExpress = true;
                                break;
                            }
                        }
                    }
                }

                if (IncludePayPalExpress)
                {
                    if(AppLogic.AppConfigExists("Mobile.PayPal.Express.ButtonImageURL"))
                        btnPayPalExpressCheckout.ImageUrl = AppLogic.AppConfig("Mobile.PayPal.Express.ButtonImageURL");
                    else
                        btnPayPalExpressCheckout.ImageUrl = AppLogic.AppConfig("PayPal.Express.ButtonImageURL");

					btnPayPalExpressCheckout2.ImageUrl = btnPayPalExpressCheckout.ImageUrl;

					if (AppLogic.AppConfigExists("Mobile.PayPal.Express.BillMeLaterButtonURL"))
						btnPayPalBillMeLaterCheckout.ImageUrl = AppLogic.AppConfig("Mobile.PayPal.Express.BillMeLaterButtonURL");
					else
						btnPayPalBillMeLaterCheckout.ImageUrl = AppLogic.AppConfig("PayPal.Express.BillMeLaterButtonURL");

					btnPayPalBillMeLaterCheckout.Visible = btnPayPalBillMeLaterCheckout2.Visible = AppLogic.AppConfigBool("PayPal.Express.ShowBillMeLaterButton");
					btnPayPalBillMeLaterCheckout2.ImageUrl = btnPayPalBillMeLaterCheckout.ImageUrl;

                    PayPalExpressSpan.Visible = true;
                    PayPalExpressSpan2.Visible = true;
                }
            }

			AlternativeCheckouts.Visible = PayPalExpressSpan.Visible;
			AlternativeCheckouts2.Visible = PayPalExpressSpan2.Visible;

			if (!AppLogic.AppConfigBool("Mobile.ShowAlternateCheckouts"))
            {
                AlternativeCheckouts.Visible =
                AlternativeCheckouts2.Visible = false;
            }

            Shipping.ShippingCalculationEnum ShipCalcID = Shipping.GetActiveShippingCalculationID();

            StringBuilder html = new StringBuilder("");
            html.Append("<script type=\"text/javascript\">\n");
            html.Append("function Cart_Validator(theForm)\n");
            html.Append("{\n");
            String cartJS = CommonLogic.ReadFile("jscripts/shoppingcart.js", true);
            foreach (CartItem c in cart.CartItems)
            {
                html.Append(cartJS.Replace("%SKU%", c.ShoppingCartRecordID.ToString()));
            }
            html.Append("return(true);\n");
            html.Append("}\n");
            html.Append("</script>\n");

            ValidationScript.Text = html.ToString();

            JSPopupRoutines.Text = AppLogic.GetJSPopupRoutines();

            ShippingInformation.Visible = (!AppLogic.AppConfigBool("SkipShippingOnCheckout") && !cart.IsAllFreeShippingComponents() && !cart.IsAllSystemComponents());
            AddresBookLlink.Visible = ThisCustomer.IsRegistered;

            btnCheckOutNowTop.Visible = (!cart.IsEmpty());

            if (!cart.IsEmpty() && cart.HasCoupon() && !cart.CouponIsValid)
            {
                pnlCouponError.Visible = true;
                CouponError.Text = cart.CouponStatusMessage + " (" + Server.HtmlEncode(CommonLogic.IIF(cart.Coupon.CouponCode.Length != 0, cart.Coupon.CouponCode, ThisCustomer.CouponCode)) + ")";
                cart.ClearCoupon();
            }
            
            if (!String.IsNullOrEmpty(errorMessage.Message) || ErrorMsgLabel.Text.Length > 0)
            {
                pnlErrorMsg.Visible = true;
                ErrorMsgLabel.Text += errorMessage.Message;
            }

            if (cart.InventoryTrimmed || this.InventoryTrimmed)
            {
                pnlInventoryTrimmedError.Visible = true;
                if (cart.TrimmedReason == InventoryTrimmedReason.RestrictedQuantities || TrimmedEarlyReason == InventoryTrimmedReason.RestrictedQuantities)
                    InventoryTrimmedError.Text = AppLogic.GetString("shoppingcart.aspx.33", SkinID, ThisCustomer.LocaleSetting);
                else
                    InventoryTrimmedError.Text = AppLogic.GetString("shoppingcart.aspx.3", SkinID, ThisCustomer.LocaleSetting);
                
                cart = new ShoppingCart(SkinID, ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);
                ctrlShoppingCart.DataSource = cart.CartItems;
                ctrlCartSummary.DataSource = cart;
            }

            if (cart.RecurringScheduleConflict)
            {
                pnlRecurringScheduleConflictError.Visible = true;
                RecurringScheduleConflictError.Text = AppLogic.GetString("shoppingcart.aspx.19", SkinID, ThisCustomer.LocaleSetting);
            }

            if (CommonLogic.QueryStringBool("minimumQuantitiesUpdated"))
            {
                pnlMinimumQuantitiesUpdatedError.Visible = true;
                MinimumQuantitiesUpdatedError.Text = AppLogic.GetString("shoppingcart.aspx.7", SkinID, ThisCustomer.LocaleSetting);
            }

            if (!cart.MeetsMinimumOrderAmount(MinOrderAmount))
            {
                pnlMeetsMinimumOrderAmountError.Visible = true;
                MeetsMinimumOrderAmountError.Text = String.Format(AppLogic.GetString("shoppingcart.aspx.4", SkinID, ThisCustomer.LocaleSetting), ThisCustomer.CurrencyString(MinOrderAmount));
            }

            int MinQuantity = AppLogic.AppConfigUSInt("MinCartItemsBeforeCheckout");
            if (!cart.MeetsMinimumOrderQuantity(MinQuantity))
            {
                pnlMeetsMinimumOrderQuantityError.Visible = true;
                MeetsMinimumOrderQuantityError.Text = String.Format(AppLogic.GetString("shoppingcart.cs.20", SkinID, ThisCustomer.LocaleSetting), MinQuantity.ToString(), MinQuantity.ToString());
            }

            int MaxQuantity = AppLogic.AppConfigUSInt("MaxCartItemsBeforeCheckout");
            if (cart.ExceedsMaximumOrderQuantity(MaxQuantity))
            {
                pnlExceedsMaximumOrderQuantityError.Visible = true;
                ExceedsMaximumOrderQuantityError.Text = String.Format(AppLogic.GetString("shoppingcart.cs.119", SkinID, ThisCustomer.LocaleSetting), MaxQuantity.ToString());
            }

            if (AppLogic.MicropayIsEnabled() && AppLogic.AppConfigBool("Micropay.ShowTotalOnTopOfCartPage"))
            {
                pnlMicropay_EnabledError.Visible = true;
                Micropay_EnabledError.Text = "<div align=\"left\">" + String.Format(AppLogic.GetString("account.aspx.10", ThisCustomer.SkinID, ThisCustomer.LocaleSetting), AppLogic.GetString("account.aspx.11", ThisCustomer.SkinID, ThisCustomer.LocaleSetting), ThisCustomer.CurrencyString(ThisCustomer.MicroPayBalance)) + "</div>";
            }

            if (!cart.IsEmpty())
            {
                pnlOrderOptions.Visible  = cart.AllOrderOptions.Count > 0;

                if (cart.GiftCardsEnabled)
                {
                    if (CouponCode.Text.Length == 0)
                        CouponCode.Text = cart.Coupon.CouponCode;

                    btnRemoveCoupon.Visible = CouponCode.Text.Length != 0;
                    pnlCoupon.Visible = true;
                }
                else
                {
                    pnlCoupon.Visible = false;
                }

                pnlPromotion.Visible = cart.PromotionsEnabled;

                if (!AppLogic.AppConfigBool("DisallowOrderNotes"))
                {
                    OrderNotes.Text = cart.OrderNotes;
                    pnlOrderNotes.Visible = true;
                }
                else
                {
                    pnlOrderNotes.Visible = false;
                }

                btnCheckOutNowBottom.Visible = true;
            }
            else
            {
                pnlOrderOptions.Visible = false;
                pnlCoupon.Visible = false;
                pnlOrderNotes.Visible = false;
                btnCheckOutNowBottom.Visible = false;
            }

            if (AppLogic.AppConfigBool("SkipShippingOnCheckout") || cart.IsAllFreeShippingComponents() || cart.IsAllSystemComponents())
            {
                ctrlCartSummary.ShowShipping = false;
            }

            if (!cart.HasTaxableComponents() || AppLogic.CustomerLevelHasNoTax(ThisCustomer.CustomerLevelID))
            {
                ctrlCartSummary.ShowTax = false;
            }

            if (cart.ShippingThresHoldIsDefinedButFreeShippingMethodIDIsNot)
            {
                pnlErrorMsg.Visible = true;
                ErrorMsgLabel.Text += Server.HtmlEncode(AppLogic.GetString("shoppingcart.aspx.21", SkinID, ThisCustomer.LocaleSetting));
            }
        }

        private void UpdateCartQuantity()
        {
            int quantity = 0;
            int sRecID = 0;
            string itemNotes;

            for (int i = 0; i < ctrlShoppingCart.Items.Count; i++)
            {
                quantity = ctrlShoppingCart.Items[i].Quantity;
                sRecID = ctrlShoppingCart.Items[i].ShoppingCartRecId;
                itemNotes = ctrlShoppingCart.Items[i].ItemNotes;

                cart.SetItemQuantity(sRecID, quantity);
                cart.SetItemNotes(sRecID, CommonLogic.CleanLevelOne(itemNotes));
            }
        }

        private void HandleInventoryTrimmed()
        {
            pnlInventoryTrimmedError.Visible = true;
            if (cart.TrimmedReason == InventoryTrimmedReason.RestrictedQuantities || TrimmedEarlyReason == InventoryTrimmedReason.RestrictedQuantities)
                InventoryTrimmedError.Text = AppLogic.GetString("shoppingcart.aspx.33", SkinID, ThisCustomer.LocaleSetting);
            else if (cart.TrimmedReason == InventoryTrimmedReason.MinimumQuantities || TrimmedEarlyReason == InventoryTrimmedReason.MinimumQuantities)
                InventoryTrimmedError.Text = AppLogic.GetString("shoppingcart.aspx.7", SkinID, ThisCustomer.LocaleSetting);
            else
                InventoryTrimmedError.Text = AppLogic.GetString("shoppingcart.aspx.3", SkinID, ThisCustomer.LocaleSetting);
        }

        public void ProcessCart(bool DoingFullCheckout, bool ForceOnePageCheckout, bool InternationalCheckout)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            ThisCustomer.RequireCustomerRecord();
            CartTypeEnum cte = CartTypeEnum.ShoppingCart;
            if (CommonLogic.QueryStringCanBeDangerousContent("CartType").Length != 0)
                cte = (CartTypeEnum)CommonLogic.QueryStringUSInt("CartType");

			cart = new ShoppingCart(1, ThisCustomer, cte, 0, false);

            if (cart.InventoryTrimmed || this.InventoryTrimmed)
            {
                HandleInventoryTrimmed();
                return; //Bail and warn the customer that their cart changed before putting them into checkout
            }

            if (cart.IsEmpty())
            {
                cart.ClearCoupon();
                // can't have this at this point:
                switch (cte)
                {
                    case CartTypeEnum.ShoppingCart:
                        Response.Redirect("shoppingcart.aspx");
                        break;
                    case CartTypeEnum.WishCart:
                        Response.Redirect("wishlist.aspx");
                        break;
                    case CartTypeEnum.GiftRegistryCart:
                        Response.Redirect("giftregistry.aspx");
                        break;
                    default:
                        Response.Redirect("shoppingcart.aspx");
                        break;
                }
            }

            // update cart quantities:
            UpdateCartQuantity();

            // save coupon code, no need to reload cart object
            // will update customer record also:
            if (cte == CartTypeEnum.ShoppingCart)
            {
                cart.SetCoupon(CouponCode.Text, true);

                // kind of backwards, but if DisallowOrderNotes is false, then
 	 	 	    // allow order notes
 	 	 	    if (!AppLogic.AppConfigBool("DisallowOrderNotes"))
 	 	 	    {
 	 	 	        if (OrderNotes.Text.Trim().Length > 0)
 	 	 	        {
 	 	 	            SqlParameter sp = new SqlParameter("@OrderNotes", SqlDbType.NText);
 	 	 	            sp.Value = OrderNotes.Text.Trim();
 	 	 	            SqlParameter[] spa = { sp };
 	 	 	            ThisCustomer.UpdateCustomer(spa);
 	 	 	        }
 	 	 	    }

                // rebind the cart summary control to handle coupon
                ctrlCartSummary.DataSource = cart;
 
                // check for upsell products
                if (CommonLogic.FormCanBeDangerousContent("Upsell").Length != 0)
                {
                    foreach (String s in CommonLogic.FormCanBeDangerousContent("Upsell").Split(','))
                    {
                        int ProductID = Localization.ParseUSInt(s);
                        if (ProductID != 0)
                        {
                            int VariantID = AppLogic.GetProductsDefaultVariantID(ProductID);
                            if (VariantID != 0)
                            {
                                int NewRecID = cart.AddItem(ThisCustomer, ThisCustomer.PrimaryShippingAddressID, ProductID, VariantID, 1, String.Empty, String.Empty, String.Empty, String.Empty, String.Empty, CartTypeEnum.ShoppingCart, true, false, 0, System.Decimal.Zero);
                                Decimal PR = AppLogic.GetUpsellProductPrice(0, ProductID, ThisCustomer.CustomerLevelID);
                                SqlParameter[] spa = { DB.CreateSQLParameter("@Price", SqlDbType.Decimal, 10, PR, ParameterDirection.Input), DB.CreateSQLParameter("@CartRecID", SqlDbType.Int, 4, NewRecID, ParameterDirection.Input) };
                                DB.ExecuteSQL("update shoppingcart set IsUpsell=1, ProductPrice=@Price where ShoppingCartRecID=@CartRecID", spa);

                            }
                        }
                    }
                }

                if (cart.CheckInventory(ThisCustomer.CustomerID))
                {
                    ErrorMsgLabel.Text += Server.HtmlEncode(AppLogic.GetString("shoppingcart_process.aspx.1", SkinID, ThisCustomer.LocaleSetting));
                    // inventory got adjusted, send them back to the cart page to confirm the new values!
                }
            }

            if (cte == CartTypeEnum.WishCart)
                Response.Redirect("wishlist.aspx");
            if (cte == CartTypeEnum.GiftRegistryCart)
                Response.Redirect("giftregistry.aspx");

            cart.ClearShippingOptions();
            if (DoingFullCheckout)
            {
				bool validated = 
					cart.MeetsMinimumOrderAmount(AppLogic.AppConfigUSDecimal("CartMinOrderAmount"))
					&& cart.MeetsMinimumOrderQuantity(AppLogic.AppConfigUSInt("MinCartItemsBeforeCheckout"))
                    && !cart.ExceedsMaximumOrderQuantity(AppLogic.AppConfigUSInt("MaxCartItemsBeforeCheckout"))
					&& (!cart.HasCoupon() || cart.CouponIsValid);

                if (validated)
                {
                    AppLogic.eventHandler("BeginCheckout").CallEvent("&BeginCheckout=true");

                    if (InternationalCheckout)
                        Response.Redirect("internationalcheckout.aspx");

                    if ((ThisCustomer.IsRegistered || ThisCustomer.EMail.Length != 0) && (ThisCustomer.Password.Length == 0 || ThisCustomer.PrimaryBillingAddressID == 0 || ThisCustomer.PrimaryShippingAddressID == 0 || !ThisCustomer.HasAtLeastOneAddress()))
                        Response.Redirect("createaccount.aspx?checkout=true");

                    if (!ThisCustomer.IsRegistered || ThisCustomer.PrimaryBillingAddressID == 0 || ThisCustomer.PrimaryShippingAddressID == 0 || !ThisCustomer.HasAtLeastOneAddress())
                    {
                        Response.Redirect("createaccount.aspx?checkout=true");
                    }
                    else
                    {
                        if (AppLogic.AppConfigBool("SkipShippingOnCheckout") || cart.IsAllSystemComponents() || cart.IsAllDownloadComponents())
                        {
                            if (cart.ContainsGiftCard())
                                Response.Redirect("checkoutgiftcard.aspx");
                            else
                                Response.Redirect("checkoutpayment.aspx");
                        }

                        if ((cart.HasMultipleShippingAddresses() || cart.HasGiftRegistryComponents()) && cart.TotalQuantity() <= AppLogic.MultiShipMaxNumItemsAllowed() && cart.CartAllowsShippingMethodSelection)
                            Response.Redirect("checkoutshippingmult.aspx");
                        else
                            Response.Redirect("checkoutshipping.aspx");
                    }
                }
                InitializePageContent();
            }

            //Make sure promotions is updated when the cart changes
            BindPromotions();
        }

        private void ClearErrors()
        {
            CouponError.Text = 
                ErrorMsgLabel.Text = 
                InventoryTrimmedError.Text = 
                RecurringScheduleConflictError.Text = 
                MinimumQuantitiesUpdatedError.Text = 
                MeetsMinimumOrderAmountError.Text = 
                MeetsMinimumOrderQuantityError.Text = 
                ExceedsMaximumOrderQuantityError.Text =
                Micropay_EnabledError.Text = String.Empty;
        }

        private void ContinueShopping()
        {
            if (AppLogic.AppConfig("ContinueShoppingURL") == "")
            {
                if (ViewState["ReturnURL"].ToString() == "")
                    Response.Redirect("default.aspx");
                else
                    Response.Redirect(ViewState["ReturnURL"].ToString());
            }
            else
            {
                Response.Redirect(AppLogic.AppConfig("ContinueShoppingURL"));
            }
        }
        private bool DeleteButtonExists(string s)
        {
            return s == "bt_Delete";
        }

		protected void btnPayPalExpressCheckout_Click(object sender, CommandEventArgs e)
        {
            ProcessCart(false, false, false);

            if (CommonLogic.CookieCanBeDangerousContent("PayPalExpressToken", false) == "")
            {
                if (!ThisCustomer.IsRegistered)
                {
                    if (cart.HasRecurringComponents() || (!AppLogic.AppConfigBool("PasswordIsOptionalDuringCheckout") && !AppLogic.AppConfigBool("PayPal.Express.AllowAnonCheckout")))
                        Response.Redirect("signin.aspx?ReturnUrl='shoppingcart.aspx'");
                    else
                        Response.Redirect("checkoutanon.aspx?checkout=true&checkouttype=ppec");
                }
                
				if (cart == null)
                    cart = new ShoppingCart(SkinID, ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);

                string url = String.Empty;
				Dictionary<string, string> checkoutOptions = new Dictionary<string, string>();

				if (e.CommandArgument.Equals("ppbml"))
					checkoutOptions.Add("UserSelectedFundingSource", "BML");

				if (ThisCustomer.IsRegistered && ThisCustomer.PrimaryShippingAddressID != 0)
				{
					Address shippingAddress = new Address();
					shippingAddress.LoadByCustomer(ThisCustomer.CustomerID, ThisCustomer.PrimaryShippingAddressID, AddressTypes.Shipping);
					url = Gateway.StartExpressCheckout(cart, shippingAddress, checkoutOptions);
				}
				else
				{
					url = Gateway.StartExpressCheckout(cart, null, checkoutOptions);
				}
                Response.Redirect(url);
            }
            else
            {
                Response.Redirect("checkoutshipping.aspx");
            }
        }

        protected void ctrlShoppingCart_ItemDeleting(object sender, ItemEventArgs e)
        {
            cart.SetItemQuantity(e.ID, 0);
            Response.Redirect("shoppingcart.aspx");
        }

        protected void btnRemoveCoupon_Click(object sender, EventArgs e)
        {
            cart.SetCoupon("", true);
            CouponCode.Text = "";
            btnRemoveCoupon.Visible = false;
            UpdateCart();
        }

        private void BindPromotions()
        {
            repeatPromotions.DataSource = cart.DiscountResults.Select(dr => dr.Promotion);
            repeatPromotions.DataBind();
        }

        protected void repeatPromotions_ItemDataBound(Object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
                return;

            Promotions.IPromotion promotion = e.Item.DataItem as Promotions.IPromotion;
            if (promotion == null)
                return;
        }

        protected void repeatPromotions_ItemCommand(Object sender, RepeaterCommandEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
                return;

            PromotionManager.ClearPromotionUsages(ThisCustomer.CustomerID, e.CommandArgument.ToString(), true);
            UpdateCart();
            BindPromotions();
        }

        protected void btnAddPromotion_Click(Object sender, EventArgs e)
        {
            String promotionCode = txtPromotionCode.Text.ToLower().Trim();
            txtPromotionCode.Text = String.Empty;
            lblPromotionError.Text = String.Empty;

            IEnumerable<IPromotionValidationResult> validationResults = PromotionManager.ValidatePromotion(promotionCode, PromotionManager.CreateRuleContext(cart));
            if (validationResults.Count() > 0 && validationResults.Any(vr => !vr.IsValid))
            {
                foreach (var reason in validationResults.Where(vr => !vr.IsValid).SelectMany(vr => vr.Reasons))
                {
                    String message = reason.MessageKey.StringResource();
                    if (reason.ContextItems != null && reason.ContextItems.Any())
                        foreach (var item in reason.ContextItems)
                            message = message.Replace(String.Format("{{{0}}}", item.Key), item.Value.ToString());

                    lblPromotionError.Text += String.Format("<div class='promotion-reason error-wrap'>{0}</div>", message);
                }
                return;
            }
            else
            {
                PromotionManager.AssignPromotion(ThisCustomer.CustomerID, promotionCode);
            }

            UpdateCart();
            BindPromotions();
        }
    }
}
