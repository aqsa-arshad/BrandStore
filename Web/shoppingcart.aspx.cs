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
using AspDotNetStorefrontCore.ShippingCalculation;
using AspDotNetStorefrontGateways;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for ShoppingCartPage.
    /// </summary>
    [PageType("shoppingcart")]
    public partial class ShoppingCartPage : SkinBase
    {
        List<CustomerFund> CustomerFunds = new List<CustomerFund>();
        Decimal productcategoryfund = 0;
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
            //btnContinueShoppingBottom.Click += new EventHandler(btnContinueShoppingBottom_Click);
            btnCheckOutNowTop.Click += new EventHandler(btnCheckOutNowTop_Click);
            btnCheckOutNowBottom.Click += new EventHandler(btnCheckOutNowBottom_Click);
            btnInternationalCheckOutNowTop.Click += new EventHandler(btnInternationalCheckOutNowTop_Click);
            btnInternationalCheckOutNowBottom.Click += new EventHandler(btnInternationalCheckOutNowBottom_Click);
            btnQuickCheckoutTop.Click += new EventHandler(btnQuickCheckoutTop_Click);
            btnQuickCheckoutBottom.Click += new EventHandler(btnQuickCheckoutBottom_Click);
            btnUpdateShoppingCart.Click += new EventHandler(btnUpdateCart_Click);
            // btnUpdateCartOrderOptions.Click += new EventHandler(btnUpdateCart_Click);
            //  btnUpdateGiftCard.Click += new EventHandler(btnUpdateCart_Click);
            // btnUpdateCartOrderNotes.Click += new EventHandler(btnUpdateCart_Click);
            btnUpdateCartUpsells.Click += new EventHandler(btnUpdateCart_Click);
        }
        #endregion

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

        protected void Page_Load(object sender, System.EventArgs e)        {

            GetPercentageRatio(ThisCustomer.CustomerLevelID.ToString());

            Updatecartcounttotalonmenue();
            Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            Response.Cache.SetNoStore();

            hdncustomerlevel.Text = ThisCustomer.CustomerLevelID.ToString();

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

            //HACK: Refresh the page if coming from the wishlist so like items are combined and bound to correctly
            if (Request.QueryString["refresh"] != null && Convert.ToBoolean(Request.QueryString["refresh"].ToString()))
            {
                Response.Redirect("shoppingcart.aspx");
                Response.End();
            }

            var checkOutType = CheckOutPageControllerFactory.CreateCheckOutPageController(ThisCustomer, cart).GetCheckoutType();
            bool onePageCheckout = checkOutType == CheckOutType.SmartOPC;

            if (onePageCheckout)
            {
                // don't need here, it's redundant with the regular checkout button:
                btnQuickCheckoutTop.Visible = false;
                btnQuickCheckoutBottom.Visible = false;
            }
            else
            {
                btnQuickCheckoutTop.Visible = AppLogic.AppConfigBool("QuickCheckout.Enabled");
                btnQuickCheckoutBottom.Visible = AppLogic.AppConfigBool("QuickCheckout.Enabled");

            }

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

                InitializePageContent(checkOutType);
                SetFundUsedByCategory(); //need this later
                InitializeShippingAndEstimateControl();


                SetSessionValue("FireEvent");
            }
            else
            {
                pnlOrderOptions.Visible = !cart.IsEmpty();
                pnlUpsellProducts.Visible = !cart.IsEmpty();
                //pnlGiftCard.Visible = !cart.IsEmpty() && cart.GiftCardsEnabled;
                // pnlPromotion.Visible = !cart.IsEmpty() && cart.PromotionsEnabled;
                //pnlOrderNotes.Visible = !AppLogic.AppConfigBool("DisallowOrderNotes") && !cart.IsEmpty();
                btnCheckOutNowBottom.Visible = btnCheckOutNowTop.Visible = (!cart.IsEmpty() && AppLogic.AllowRegularCheckout(cart));
                btnRequestEstimates.Visible = !cart.IsEmpty();
                pnlSubTotals.Visible = !cart.IsEmpty();

                String FireEvent = GetSessionValue("FireEvent");
                if (FireEvent == "1")
                {
                    btnaddtocart_Click(null, null);
                    SetSessionValue("FireEvent");
                }
            }

            if (!ThisCustomer.IsRegistered || ThisCustomer.PrimaryShippingAddressID <= 0)
            {
                InitializeShippingAndEstimateControl();
            }

            String CurrentCoupon = String.Empty;
            //if (cart.Coupon.CouponCode.Length != 0 && String.IsNullOrEmpty(txtGiftCard.Text))
            //    CurrentCoupon = cart.Coupon.CouponCode;
            //else if (cart.CartItems.CouponList.Count == 1)
            //    CurrentCoupon = cart.CartItems.CouponList[0].CouponCode;

            //if (CurrentCoupon.Length > 0 && txtGiftCard.Text.Length == 0)
            //    txtGiftCard.Text = CurrentCoupon;

            //btnRemoveGiftCard.Visible = txtGiftCard.Text.Length != 0;

            //lblPromotionError.Text = String.Empty;
            if (!IsPostBack)
                BindPromotions();

            if (cart.CartItems.Count == 0)
            {
                btnUpdateShoppingCart.CssClass = "hide-element";
            }

            if (!IsPostBack)
                GetBluBucksAndCategoryFundsForCustomer(true);
            else
                GetBluBucksAndCategoryFundsForCustomer(false);


        }

        private void GetBluBucksAndCategoryFundsForCustomer(bool fromload)
        {
            CustomerFunds = AuthenticationSSO.GetCustomerFund(ThisCustomer.CustomerID);
            if (CustomerFunds.Count > 0)
            {
                GeAllFundsOfCustomer(fromload);
            }


        }

        private void GeAllFundsOfCustomer(bool fromload)
        {
            //Get all funds amount of customer
            hdnBluBucktsPoints.Text = getfundamount(Convert.ToInt32(FundType.BLUBucks));
            ppointscount.InnerText = "You have " + Math.Round(Convert.ToDecimal(hdnBluBucktsPoints.Text), 2) + " BLU™ Bucks you can use to purchase your items.";
            hdnsoffundamount.Text = getfundamount(Convert.ToInt32(FundType.SOFFunds));
            hdndirectmailfundamount.Text = getfundamount(Convert.ToInt32(FundType.DirectMailFunds));
            hdndisplayfundamount.Text = getfundamount(Convert.ToInt32(FundType.DisplayFunds));
            hdnliteraturefundamount.Text = getfundamount(Convert.ToInt32(FundType.LiteratureFunds));
            hdnpopfundamount.Text = getfundamount(Convert.ToInt32(FundType.POPFunds));
            //end get all funds amount of customer          
            hdncustomerlevel.Text = ThisCustomer.CustomerLevelID.ToString();           



        }
        private string getfundamount(int FundID)
        {
            string fundamount = "0";
            CustomerFund tempfund = CustomerFunds.Find(x => x.FundID == FundID);
            if (tempfund != null)
                fundamount = tempfund.AmountAvailable.ToString();

            return Math.Round(Convert.ToDecimal(fundamount), 2).ToString();
        }

        void btnContinueShoppingTop_Click(object sender, EventArgs e)
        {
            // UpdateCartQuantity();
            ContinueShopping();
        }
        void btnContinueShoppingBottom_Click(object sender, EventArgs e)
        {
            ContinueShopping();
        }
        void btnCheckOutNowTop_Click(object sender, EventArgs e)
        {
            // SetFundUsedByCategory(); //remove this later if required

            string errormessage = AppLogic.GetString("FundLimitExceeds", SkinID, ThisCustomer.LocaleSetting.ToString());
            string script = "alert('" + errormessage + "')";

            if (validateuserfunds())
            {
                ProcessCart(true, false, false);
                InitializeShippingAndEstimateControl();
            }
            else
            {
                System.Web.UI.ClientScriptManager cs = this.ClientScript;
                cs.RegisterClientScriptBlock(this.GetType(), "alertMessage", script, true);
            }
        }

        private bool validateuserfunds()
        {
            if (AuthenticationSSO.IsDealerUser(ThisCustomer.CustomerLevelID) || AuthenticationSSO.IsInternalUser(ThisCustomer.CustomerLevelID))
                return AuthenticationSSO.ValidateCustomerFund(ThisCustomer.CustomerID);
            else
                return true;
        }
        void btnCheckOutNowBottom_Click(object sender, EventArgs e)
        {
            // SetFundUsedByCategory(); //remove this later if required

            string errormessage = AppLogic.GetString("FundLimitExceeds", SkinID, ThisCustomer.LocaleSetting.ToString());
            string script = "alert('" + errormessage + "')";
            if (validateuserfunds())
            {
                ProcessCart(true, false, false);
                InitializeShippingAndEstimateControl();
            }
            else
            {
                System.Web.UI.ClientScriptManager cs = this.ClientScript;
                cs.RegisterClientScriptBlock(this.GetType(), "alertMessage", script, true);
            }
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

        void btnUpdateCart_Click(object sender, EventArgs e)
        {
            UpdateCart();
        }
        public void btnaddtocart_Click(object sender, EventArgs e)
        {
            UpdateCart();

        }
    
        public  void  GetPercentageRatio(string CustomerLevelID)
        {        
            
            hdnBudgetPercentageRation_Cat1.Text = GetRatio(CustomerLevelID, "1");
            hdnBudgetPercentageRation_Cat2.Text = GetRatio(CustomerLevelID, "2");
            hdnBudgetPercentageRation_Cat3.Text = GetRatio(CustomerLevelID, "3");
            hdnBudgetPercentageRation_Cat4.Text = GetRatio(CustomerLevelID, "4");
            hdnBudgetPercentageRation_Cat5.Text = GetRatio(CustomerLevelID, "5");
            hdnBudgetPercentageRation_Cat6.Text = GetRatio(CustomerLevelID, "6");         
            
             
        }

        public string GetRatio(string CustomerLevelID, string ProductCategoryID)
        {
            BudgetPercentageRatio FundPercentage = AuthenticationSSO.GetBudgetPercentageRatio(Convert.ToInt32(CustomerLevelID), Convert.ToInt32(ProductCategoryID));
            return FundPercentage.BudgetPercentageValue.ToString();
        }

        #region "Session related"
        public void btnaddtocartforsalesrep_Click(object sender, EventArgs e)
        {
            UpdateCart();
        }
        [System.Web.Services.WebMethod(EnableSession = true)]
        public static void SaveValuesInSession(String ProductCategoryFundUsed, String BluBucksUsed, String currentrecordid)
        {
            try
            {
                System.Web.HttpContext.Current.Session["ProductCategoryFundUsed"] = ProductCategoryFundUsed;
                System.Web.HttpContext.Current.Session["BluBucksUsed"] = BluBucksUsed;
                System.Web.HttpContext.Current.Session["currentrecordid"] = currentrecordid;

            }
            catch (Exception ex)
            {

            }
        }

        [System.Web.Services.WebMethod(EnableSession = true)]
        public static void Firebtnaddtocartclickevent(String FireEvent)
        {
            try
            {
                System.Web.HttpContext.Current.Session["FireEvent"] = FireEvent;
            }

            catch (Exception ex)
            {

            }

        }

        private String GetSessionValue(String ParamName)
        {
            var value = System.Web.HttpContext.Current.Session[ParamName];
            if (value != null)
                return Convert.ToString(value);
            else
                return "0";
        }

        private void SetSessionValue(String ParamName)
        {
            try
            {
                System.Web.HttpContext.Current.Session[ParamName] = "0";
            }

            catch (Exception ex)
            {

            }

        }

        #endregion

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
            //cart.SetCoupon(txtGiftCard.Text.ToUpperInvariant(), true);

            String ProductCategoryFundUsed = GetSessionValue("ProductCategoryFundUsed");
            String BluBucksUsed = GetSessionValue("BluBucksUsed");
            String currentrecordid = GetSessionValue("currentrecordid");
            String GLcode = (optionsRadioYes.Checked ? "Yes" : "No");
            UpdateCurrentItemFundsUsed(ProductCategoryFundUsed, BluBucksUsed, currentrecordid, GLcode);//Added By Tayyab on 10-01-2016
            UpdateCartQuantity(currentrecordid);

            ctrlOrderOption.UpdateChanges();
            ProcessCart(false, false, false);
            InitializePageContent(CheckOutPageControllerFactory.CreateCheckOutPageController(ThisCustomer, cart).GetCheckoutType());
            InitializeShippingAndEstimateControl();
            InitializeShoppingCartControl();
            SetFundUsedByCategory(); //need this later
            GetBluBucksAndCategoryFundsForCustomer(true);
            Updatecartcounttotalonmenue();
        }

        public void SetFundUsedByCategory()
        {
            #region "Update summary control"

            //Update amount used for each category funds
            Decimal SofFundsUsedTotal = 0, DirectMailFundsUsedTotal = 0, DisplayFundsUsedTotal = 0, LiteratureFundsUsedTotal = 0, PopFundsUsedTotal = 0, BluBucksFundsUsedTotal = 0;
            foreach (CartItem cItem in cart.CartItems)
            {
                if (cItem.FundID == 2)
                    SofFundsUsedTotal = Math.Round((Convert.ToDecimal(SofFundsUsedTotal) + Convert.ToDecimal(cItem.CategoryFundUsed)), 2);
                else if (cItem.FundID == 3)
                    DirectMailFundsUsedTotal = Math.Round((Convert.ToDecimal(DirectMailFundsUsedTotal) + Convert.ToDecimal(cItem.CategoryFundUsed)), 2);
                else if (cItem.FundID == 4)
                    DisplayFundsUsedTotal = Math.Round((Convert.ToDecimal(DisplayFundsUsedTotal) + Convert.ToDecimal(cItem.CategoryFundUsed)), 2);
                else if (cItem.FundID == 5)
                    LiteratureFundsUsedTotal = Math.Round((Convert.ToDecimal(LiteratureFundsUsedTotal) + Convert.ToDecimal(cItem.CategoryFundUsed)), 2);
                else if (cItem.FundID == 6)
                    PopFundsUsedTotal = Math.Round((Convert.ToDecimal(PopFundsUsedTotal) + Convert.ToDecimal(cItem.CategoryFundUsed)), 2);

                BluBucksFundsUsedTotal = Math.Round((Convert.ToDecimal(BluBucksFundsUsedTotal) + Convert.ToDecimal(cItem.pricewithBluBuksUsed)), 2);

            }

            //Blu Bucks used total  
            if (BluBucksFundsUsedTotal > 0)
            {
                BluBucksFundsUsedTotal = Math.Round(BluBucksFundsUsedTotal + cart.CartItems.FirstOrDefault().ShipmentChargesPaid, 2);
            }
            //End Blu Bucks Used Total

            //Sof used total                  
            if (SofFundsUsedTotal > 0)
            {
                SofFundsUsedTotal = Math.Round(SofFundsUsedTotal + cart.CartItems.FirstOrDefault().ShipmentChargesPaid, 2);
            }
            //End Sof Used Total

            AuthenticationSSO.UpdateCustomerFundAmountUsed(ThisCustomer.CustomerID, Convert.ToInt32(FundType.BLUBucks), BluBucksFundsUsedTotal);
            AuthenticationSSO.UpdateCustomerFundAmountUsed(ThisCustomer.CustomerID, Convert.ToInt32(FundType.SOFFunds), SofFundsUsedTotal);
            AuthenticationSSO.UpdateCustomerFundAmountUsed(ThisCustomer.CustomerID, Convert.ToInt32(FundType.DirectMailFunds), DirectMailFundsUsedTotal);
            AuthenticationSSO.UpdateCustomerFundAmountUsed(ThisCustomer.CustomerID, Convert.ToInt32(FundType.DisplayFunds), DisplayFundsUsedTotal);
            AuthenticationSSO.UpdateCustomerFundAmountUsed(ThisCustomer.CustomerID, Convert.ToInt32(FundType.LiteratureFunds), LiteratureFundsUsedTotal);
            AuthenticationSSO.UpdateCustomerFundAmountUsed(ThisCustomer.CustomerID, Convert.ToInt32(FundType.POPFunds), PopFundsUsedTotal);
            //End Update Funds
            #endregion
        }

        public void InitializePageContent(CheckOutType checkoutType)
        {
            int AgeCartDays = AppLogic.AppConfigUSInt("AgeCartDays");
            if (AgeCartDays == 0)
            {
                AgeCartDays = 7;
            }

            ShoppingCart.Age(ThisCustomer.CustomerID, AgeCartDays, CartTypeEnum.ShoppingCart);

            cart = new ShoppingCart(SkinID, ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);
            // shoppingcartaspx8.Text = AppLogic.GetString("shoppingcart.aspx.8", SkinID, ThisCustomer.LocaleSetting);
            //shoppingcartaspx10.Text = AppLogic.GetString("shoppingcart.aspx.10", SkinID, ThisCustomer.LocaleSetting);
            //shoppingcartaspx11.Text = AppLogic.GetString("shoppingcart.aspx.11", SkinID, ThisCustomer.LocaleSetting);
            //shoppingcartaspx9.Text = AppLogic.GetString("shoppingcart.aspx.9", SkinID, ThisCustomer.LocaleSetting);
            //shoppingcartcs31.Text = AppLogic.GetString("shoppingcart.cs.117", SkinID, ThisCustomer.LocaleSetting);
            btnUpdateShoppingCart.Text = AppLogic.GetString("shoppingcart.cs.110", SkinID, ThisCustomer.LocaleSetting);
            //btnUpdateCartOrderOptions.Text = AppLogic.GetString("shoppingcart.cs.110", SkinID, ThisCustomer.LocaleSetting);
            //btnUpdateGiftCard.Text = AppLogic.GetString("shoppingcart.cs.110", SkinID, ThisCustomer.LocaleSetting);
            //btnUpdateCartOrderNotes.Text = AppLogic.GetString("shoppingcart.cs.110", SkinID, ThisCustomer.LocaleSetting);
            btnUpdateCartUpsells.Text = AppLogic.GetString("shoppingcart.cs.110", SkinID, ThisCustomer.LocaleSetting);
            //lblOrderNotes.Text = AppLogic.GetString("shoppingcart.cs.66", SkinID, ThisCustomer.LocaleSetting);
            btnContinueShoppingTop.Text = AppLogic.GetString("shoppingcart.cs.62", SkinID, ThisCustomer.LocaleSetting);
            //btnContinueShoppingBottom.Text = AppLogic.GetString("shoppingcart.cs.62", SkinID, ThisCustomer.LocaleSetting);
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
            //lblErrorMessage.Text = CommonLogic.IIF(!cart.IsEmpty() && (reqOver13 && !ThisCustomer.IsOver13 && ThisCustomer.IsRegistered), AppLogic.GetString("Over13OnCheckout", SkinID, ThisCustomer.LocaleSetting), String.Empty);

            btnCheckOutNowBottom.Enabled = btnCheckOutNowTop.Enabled;

            //divPayPalExpressTop.Visible = false;
            divPayPalExpressBottom.Visible = false;

            Decimal MinOrderAmount = AppLogic.AppConfigUSDecimal("CartMinOrderAmount");

            if (!cart.IsEmpty())
            {
                // Enable PayPalExpress if using PayPalPro or PayPal Express is an active payment method.
                bool IncludePayPalExpress = false;

                if (AppLogic.AppConfigBool("PayPal.Express.ShowOnCartPage") && cart.MeetsMinimumOrderAmount(MinOrderAmount) && !cart.RecurringScheduleConflict)
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
                    //	btnPayPalExpressCheckoutTop.ImageUrl = AppLogic.AppConfig("PayPal.Express.ButtonImageURL");
                    //btnPayPalExpressCheckoutBottom.ImageUrl = btnPayPalExpressCheckoutTop.ImageUrl;
                    //divPayPalExpressTop.Visible = true;
                    divPayPalExpressBottom.Visible = true;

                    //btnPayPalBillMeLaterTop.ImageUrl = AppLogic.AppConfig("PayPal.Express.BillMeLaterButtonURL");
                    //ltBillMeLaterMessageTop.Text = AppLogic.AppConfig("PayPal.Express.BillMeLaterMarketingMessage");
                    //ltBillMeLaterMessageBottom.Text = ltBillMeLaterMessageTop.Text;

                    //btnPayPalBillMeLaterBottom.ImageUrl = btnPayPalBillMeLaterTop.ImageUrl;
                    //btnPayPalBillMeLaterTop.Visible = AppLogic.AppConfigBool("PayPal.Express.ShowBillMeLaterButton");

                    //ltBillMeLaterMessageTop.Visible = btnPayPalBillMeLaterTop.Visible;
                    //btnPayPalBillMeLaterBottom.Visible = btnPayPalBillMeLaterTop.Visible;
                    //ltBillMeLaterMessageBottom.Visible = btnPayPalBillMeLaterTop.Visible;

                    if (ThisCustomer.IsRegistered && AppLogic.AppConfigBool("PayPal.Express.UseIntegratedCheckout") && AppLogic.AppConfig("PayPal.API.Signature").Length > 0)
                    {
                        //ltPayPalIntegratedCheckout.Text = AspDotNetStorefrontGateways.Processors.PayPalController.GetExpressCheckoutIntegratedScript(true);
                        //btnPayPalExpressCheckoutTop.Attributes.Add("onClick", "return startCheckout();");
                    }
                }
            }

            //divAmazonCheckoutTop.Visible = false;
            divAmazonCheckoutBottom.Visible = false;

            GatewayCheckoutByAmazon.CheckoutByAmazon cba = new GatewayCheckoutByAmazon.CheckoutByAmazon();

            // Clear out any cba checkouts that haven't finished.
            if (cba.IsEnabled)
                cba.ResetCheckout(ThisCustomer.CustomerID);

            // If cba is enabled and the cart allows checkout in it's current state, then render the checkout buttons.
            Boolean allowCheckoutByAmazon = !cart.IsEmpty() && !cart.ContainsRecurring() && cart.MeetsMinimumOrderAmount(MinOrderAmount) && ThisCustomer.ThisCustomerSession["IGD"].Length == 0;
            if (allowCheckoutByAmazon && cba.IsEnabled)
            {
                //divAmazonCheckoutTop.Visible =
                divAmazonCheckoutBottom.Visible = true;

                //ltAmazonCheckoutButtonTop.Text = cba.RenderCheckoutButton("CBAWidgetContainer1", new Guid(ThisCustomer.CustomerGUID), true);
                ltAmazonCheckoutButtonBottom.Text = cba.RenderCheckoutButton("CBAWidgetContainer2", new Guid(ThisCustomer.CustomerGUID), true);
            }

            //if (divPayPalExpressTop.Visible || divAmazonCheckoutTop.Visible)
            //{
            //    divAltCheckoutsTop.Visible = true;
            //    divAltCheckoutsBottom.Visible = true;
            //}
            //else
            //{
            //    divAltCheckoutsTop.Visible = false;
            //    divAltCheckoutsBottom.Visible = false;
            //}

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

            ltValidationScript.Text = html.ToString();

            ltJsPopupRoutines.Text = AppLogic.GetJSPopupRoutines();
            String XmlPackageName = AppLogic.AppConfig("XmlPackage.ShoppingCartPageHeader");
            if (XmlPackageName.Length != 0)
            {
                ltShoppingCartHeaderXmlPackage.Text = AppLogic.RunXmlPackage(XmlPackageName, base.GetParser, ThisCustomer, SkinID, String.Empty, String.Empty, true, true);
            }

            //pnlShippingInformation.Visible = (!AppLogic.AppConfigBool("SkipShippingOnCheckout") && !cart.IsAllFreeShippingComponents() && !cart.IsAllSystemComponents());
            //pnlAddresBookLink.Visible = ThisCustomer.IsRegistered;

            btnCheckOutNowTop.Visible = btnCheckOutNowBottom.Visible = (!cart.IsEmpty() && AppLogic.AllowRegularCheckout(cart));

            if (!cart.IsEmpty() && cart.HasCoupon() && !cart.CouponIsValid)
            {
                //pnlCouponError.Visible = true;
                //lblCouponError.Text = cart.CouponStatusMessage + " (" + Server.HtmlEncode(CommonLogic.IIF(cart.Coupon.CouponCode.Length != 0, cart.Coupon.CouponCode, ThisCustomer.CouponCode)) + ")";
                cart.ClearCoupon();
                //txtGiftCard.Text = String.Empty;
            }

            //if (!String.IsNullOrEmpty(errorMessage.Message) || lblErrorMessage.Text.Length > 0)
            //{
            //    pnlErrorMessage.Visible = true;
            //    lblErrorMessage.Text += errorMessage.Message;
            //}

            //if (cart.RecurringScheduleConflict)
            //{
            //    pnlRecurringScheduleConflictError.Visible = true;
            //    lblRecurringScheduleConflictError.Text = AppLogic.GetString("shoppingcart.aspx.19", SkinID, ThisCustomer.LocaleSetting);
            //}

            //if (CommonLogic.QueryStringBool("minimumQuantitiesUpdated"))
            //{
            //    pnlMinimumQuantitiesUpdatedError.Visible = true;
            //    lblMinimumQuantitiesUpdatedError.Text = AppLogic.GetString("shoppingcart.aspx.7", SkinID, ThisCustomer.LocaleSetting);
            //}

            //if (!cart.MeetsMinimumOrderAmount(MinOrderAmount))
            //{
            //    pnlMinimumOrderAmountError.Visible = true;
            //    lblMinimumOrderAmountError.Text = String.Format(AppLogic.GetString("shoppingcart.aspx.4", SkinID, ThisCustomer.LocaleSetting), ThisCustomer.CurrencyString(MinOrderAmount));
            //}

            //int MinQuantity = AppLogic.AppConfigUSInt("MinCartItemsBeforeCheckout");
            //if (!cart.MeetsMinimumOrderQuantity(MinQuantity))
            //{
            //    pnlMinimumOrderQuantityError.Visible = true;
            //    lblMinimumOrderQuantityError.Text = String.Format(AppLogic.GetString("shoppingcart.cs.20", SkinID, ThisCustomer.LocaleSetting), MinQuantity.ToString(), MinQuantity.ToString());
            //}

            //int MaxQuantity = AppLogic.AppConfigUSInt("MaxCartItemsBeforeCheckout");
            //if (cart.ExceedsMaximumOrderQuantity(MaxQuantity))
            //{
            //    pnlMaximumOrderQuantityError.Visible = true;
            //    lblMaximumOrderQuantityError.Text = String.Format(AppLogic.GetString("shoppingcart.cs.119", SkinID, ThisCustomer.LocaleSetting), MaxQuantity.ToString());
            //}

            if (AppLogic.MicropayIsEnabled() && AppLogic.AppConfigBool("Micropay.ShowTotalOnTopOfCartPage"))
            {
                //pnlMicropayEnabledNotice.Visible = true;
                // pnlMicropayEnabledNotice.Visible = !(ThisCustomer.CustomerLevelName == "Public" | ThisCustomer.CustomerLevelID == 0);
                //lblMicropayEnabledNotice.Text = String.Format(AppLogic.GetString("account.aspx.10", ThisCustomer.SkinID, ThisCustomer.LocaleSetting), AppLogic.GetString("account.aspx.11", ThisCustomer.SkinID, ThisCustomer.LocaleSetting), ThisCustomer.CurrencyString(ThisCustomer.MicroPayBalance));
            }

            pnlSubTotals.Visible = !cart.IsEmpty();

            if (!cart.IsEmpty())
            {
                if (cart.AllOrderOptions.Count > 0)
                {
                    pnlOrderOptions.Visible = true;
                }
                else
                {
                    pnlOrderOptions.Visible = false;
                }


                string upsellproductlist = GetUpsellProducts(cart);
                if (upsellproductlist.Length > 0)
                {
                    ltUpsellProducts.Text = upsellproductlist;
                    btnUpdateCartUpsells.Visible = true;
                    pnlUpsellProducts.Visible = true;
                }
                else
                {
                    pnlUpsellProducts.Visible = false;
                }

                //if (cart.GiftCardsEnabled)
                //{
                //    if (txtGiftCard.Text.Length == 0)
                //        txtGiftCard.Text = cart.Coupon.CouponCode;

                //    btnRemoveGiftCard.Visible = txtGiftCard.Text.Length != 0;
                //    pnlGiftCard.Visible = true;
                //}
                //else
                //{
                //    pnlGiftCard.Visible = false;
                //}

                // pnlPromotion.Visible = cart.PromotionsEnabled;

                //if (!AppLogic.AppConfigBool("DisallowOrderNotes"))
                //{
                //    txtOrderNotes.Text = cart.OrderNotes;
                //    pnlOrderNotes.Visible = true;
                //}
                //else
                //{
                //    pnlOrderNotes.Visible = false;
                //}

            }
            else
            {
                pnlOrderOptions.Visible = false;
                pnlUpsellProducts.Visible = false;
                //pnlGiftCard.Visible = pnlPromotion.Visible = false;
                //pnlOrderNotes.Visible = false;
            }

            if (AppLogic.AppConfigBool("SkipShippingOnCheckout") || cart.IsAllFreeShippingComponents() || cart.IsAllSystemComponents())
            {
                ctrlCartSummary.ShowShipping = false;
            }


            if (!cart.HasTaxableComponents() || AppLogic.CustomerLevelHasNoTax(ThisCustomer.CustomerLevelID))
            {
                ctrlCartSummary.ShowTax = false;
            }

            String XmlPackageName2 = AppLogic.AppConfig("XmlPackage.ShoppingCartPageFooter");
            if (XmlPackageName2.Length != 0)
            {
                ltShoppingCartFooterXmlPackage.Text = AppLogic.RunXmlPackage(XmlPackageName2, base.GetParser, ThisCustomer, SkinID, String.Empty, String.Empty, true, true);
            }

            // handle international checkout buttons now (see internationalcheckout.com).
            if (btnCheckOutNowTop.Visible)
            {
                btnInternationalCheckOutNowTop.Visible = CheckOutPageControllerFactory.CreateCheckOutPageController(ThisCustomer, cart)
                    .CanUseInternationalCheckout();
                btnInternationalCheckOutNowBottom.Visible = btnInternationalCheckOutNowTop.Visible;
            }

            //if (cart.ShippingThresHoldIsDefinedButFreeShippingMethodIDIsNot)
            //{
            //    //pnlErrorMessage.Visible = true;
            //    //lblErrorMessage.Text += Server.HtmlEncode(AppLogic.GetString("shoppingcart.aspx.21", SkinID, ThisCustomer.LocaleSetting));
            //}

            btnRemoveEstimator.Visible = false;

            ToggleShowHideEstimate();

            PayPalAd cartPageAd = new PayPalAd(PayPalAd.TargetPage.Cart);
            //if (cartPageAd.Show)
            //    ltPayPalAd.Text = cartPageAd.ImageScript;
        }

        private void HandleInventoryTrimmed()
        {
            //pnlInventoryTrimmedError.Visible = true;
            //if (cart.TrimmedReason == InventoryTrimmedReason.RestrictedQuantities || TrimmedEarlyReason == InventoryTrimmedReason.RestrictedQuantities)
            //    lblInventoryTrimmedError.Text = AppLogic.GetString("shoppingcart.aspx.33", SkinID, ThisCustomer.LocaleSetting);
            //else if (cart.TrimmedReason == InventoryTrimmedReason.MinimumQuantities || TrimmedEarlyReason == InventoryTrimmedReason.MinimumQuantities)
            //    lblInventoryTrimmedError.Text = AppLogic.GetString("shoppingcart.aspx.7", SkinID, ThisCustomer.LocaleSetting);
            //else
            //    lblInventoryTrimmedError.Text = AppLogic.GetString("shoppingcart.aspx.3", SkinID, ThisCustomer.LocaleSetting);
        }

        private void InitializeShippingAndEstimateControl()
        {
            bool showEstimates = AppLogic.AppConfigBool("ShowShippingAndTaxEstimate");

            if (ThisCustomer.ThisCustomerSession.SessionBool("ShowEstimateSelected") && showEstimates)
            {
                btnRequestEstimates_Click(this, EventArgs.Empty);
            }
            else
            {
                ToggleShowHideEstimate();

                //Set it to false, in case ShowShippingAndTaxEstimate appconfig was turn off in Admin
                pnlShippingAndTaxEstimator.Visible = false;
            }

            if (!ThisCustomer.IsRegistered || ThisCustomer.PrimaryShippingAddressID <= 0)
            {
                ctrlEstimateAddress.CaptionWidth = Unit.Percentage(0);
                ctrlEstimateAddress.ValueWidth = Unit.Percentage(70);
                ctrlEstimateAddress.Header = AppLogic.GetString("checkoutshipping.AddressControl.Header", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                ctrlEstimateAddress.CountryCaption = AppLogic.GetString("checkoutshipping.AddressControl.Country", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                ctrlEstimateAddress.StateCaption = AppLogic.GetString("checkoutshipping.AddressControl.State", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                ctrlEstimateAddress.ZipCaption = AppLogic.GetString("checkoutshipping.AddressControl.PostalCode", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                ctrlEstimateAddress.CityCaption = AppLogic.GetString("checkoutshipping.AddressControl.City", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                ctrlEstimateAddress.GetEstimateCaption = AppLogic.GetString("checkoutshipping.AddressControl.GetEstimateCaption", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                ctrlEstimateAddress.RequireZipCodeErrorMessage = AppLogic.GetString("checkoutshipping.AddressControl.ErrorMessage", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                ctrlEstimateAddress.HideZipCodeValidation();
            }

            if (btnRequestEstimates.Visible)
            {
                pnlShippingAndTaxEstimator.Visible = false;
            }

            btnRequestEstimates.Text = AppLogic.GetString("checkoutshipping.AddressControl.GetShippingAndTaxEstimates", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
        }

        private void ToggleShowHideEstimate()
        {
            if (AppLogic.AppConfigBool("ShowShippingAndTaxEstimate"))
            {
                bool estimateShown = ThisCustomer.ThisCustomerSession.SessionBool("ShowEstimateSelected");

                btnRequestEstimates.Visible = !estimateShown;
                btnRemoveEstimator.Visible = estimateShown;

                ctrlCartSummary.ShowShipping = !estimateShown;
                ctrlCartSummary.ShowTax = !estimateShown;
            }
            else
            {
                btnRequestEstimates.Visible = false;
                btnRemoveEstimator.Visible = false;
            }
        }

        protected void btnRemoveEstimator_Click(object sender, EventArgs e)
        {
            if (ThisCustomer.ThisCustomerSession.SessionBool("ShowEstimateSelected"))
            {
                ThisCustomer.ThisCustomerSession.ClearVal("ShowEstimateSelected");
            }

            btnRemoveEstimator.Text = AppLogic.GetString("checkoutshipping.estimator.control.remove", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
            pnlShippingAndTaxEstimator.Visible = false;

            ToggleShowHideEstimate();

            InitializePageContent(CheckOutPageControllerFactory.CreateCheckOutPageController(ThisCustomer, cart).GetCheckoutType());
        }

        protected void btnRequestEstimates_Click(object sender, EventArgs e)
        {
            if (ThisCustomer.IsRegistered && ThisCustomer.PrimaryShippingAddressID > 0)
            {
                SetupShippingAndEstimateControl(ctrlEstimate, ThisCustomer);
                ctrlEstimate.Visible = true;
            }
            else
            {
                IShippingCalculation activeShippingCalculation = cart.GetActiveShippingCalculation();

                // check whether the current shipping calculation logic requires zip code
                ctrlEstimateAddress.RequirePostalCode = activeShippingCalculation.RequirePostalCode;
                ctrlEstimateAddress.GetEstimateCaption = AppLogic.GetString("checkoutshipping.AddressControl.GetEstimateCaption", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                ctrlEstimateAddress.Visible = true;
                if (!ThisCustomer.IsRegistered && AppLogic.AppConfigBool("AvalaraTax.Enabled"))
                {
                    EstimateAddressControl_RequestEstimateButtonClicked(sender, e);
                }
            }

            ThisCustomer.ThisCustomerSession.SetVal("ShowEstimateSelected", true.ToString(), DateTime.MaxValue);

            pnlShippingAndTaxEstimator.Visible = true;
            ToggleShowHideEstimate();

        }

        protected void EstimateAddressControl_RequestEstimateButtonClicked(object sender, EventArgs e)
        {
            ShippingAndTaxEstimatorAddressControl addressControl = sender as ShippingAndTaxEstimatorAddressControl;

            if (addressControl != null)
            {
                // anonymous customer, extract address info from the post args
                Address NewAddress = new Address();
                NewAddress.Country = addressControl.Country;
                NewAddress.City = addressControl.City;
                NewAddress.State = addressControl.State;
                NewAddress.Zip = addressControl.Zip;
                NewAddress.InsertDB(ThisCustomer.CustomerID);
                ThisCustomer.PrimaryShippingAddressID = NewAddress.AddressID;

                IShippingCalculation activeShippingCalculation = cart.GetActiveShippingCalculation();

                if ((activeShippingCalculation.RequirePostalCode == false) ||
                    activeShippingCalculation.RequirePostalCode && addressControl.ValidateZipCode())
                {
                    ShippingAndTaxEstimateTableControl ctrlEstimate = new ShippingAndTaxEstimateTableControl();
                    SetupShippingAndEstimateControl(ctrlEstimate, ThisCustomer);

                    pnlShippingAndTaxEstimator.Controls.Add(ctrlEstimate);
                }
                // hide the estimate button
                ToggleShowHideEstimate();

                //Clean up the partial address that was added to get the estimate
                Address.DeleteFromDB(ThisCustomer.PrimaryShippingAddressID, ThisCustomer.CustomerID);
            }
        }

        public string GetUpsellProducts(ShoppingCart cart)
        {
            StringBuilder UpsellProductList = new StringBuilder(1024);
            StringBuilder results = new StringBuilder("");

            // ----------------------------------------------------------------------------------------
            // WRITE OUT UPSELL PRODUCTS:
            // ----------------------------------------------------------------------------------------
            if (AppLogic.AppConfigBool("ShowUpsellProductsOnCartPage"))
            {
                foreach (CartItem c in cart.CartItems)
                {
                    if (UpsellProductList.Length != 0)
                    {
                        UpsellProductList.Append(",");
                    }
                    UpsellProductList.Append(c.ProductID.ToString());
                }
                if (UpsellProductList.Length != 0)
                {
                    // get list of all upsell products for those products now in the cart:
                    String sql = "select UpsellProducts from Product  with (NOLOCK)  where ProductID in (" + UpsellProductList.ToString() + ")";

                    using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
                    {
                        dbconn.Open();
                        using (IDataReader rs = DB.GetRS(sql, dbconn))
                        {
                            UpsellProductList.Remove(0, UpsellProductList.Length);
                            while (rs.Read())
                            {
                                if (DB.RSField(rs, "UpsellProducts").Length != 0)
                                {
                                    if (UpsellProductList.Length != 0)
                                    {
                                        UpsellProductList.Append(",");
                                    }
                                    UpsellProductList.Append(DB.RSField(rs, "UpsellProducts"));
                                }
                            }
                        }

                    }

                    if (UpsellProductList.Length != 0)
                    {
                        int ShowN = AppLogic.AppConfigUSInt("UpsellProductsLimitNumberOnCart");
                        if (ShowN == 0)
                        {
                            ShowN = 10;
                        }
                        String S = String.Empty;
                        try
                        {
                            S = AppLogic.GetUpsellProductsBoxExpandedForCart(UpsellProductList.ToString(), ShowN, true, String.Empty, AppLogic.AppConfig("RelatedProductsFormat").Equals("GRID", StringComparison.InvariantCultureIgnoreCase), SkinID, ThisCustomer);
                        }
                        catch { }
                        if (S.Length != 0)
                        {
                            results.Append(S);
                        }
                    }
                }
            }
            return results.ToString();
        }

        private void UpdateCartQuantity(String currentrecordid)
        {
            int quantity = 0;
            int sRecID = 0;
            string itemNotes;
            int recurringVariantID = 0;

            for (int i = 0; i < ctrlShoppingCart.Items.Count; i++)
            {
                quantity = ctrlShoppingCart.Items[i].Quantity;
                sRecID = ctrlShoppingCart.Items[i].ShoppingCartRecId;
                itemNotes = ctrlShoppingCart.Items[i].ItemNotes;

              
                try
                {

                    if (Convert.ToInt32(currentrecordid) == sRecID)
                    {
                        if (AppLogic.AppConfigBool("AllowRecurringFrequencyChangeInCart") && ctrlShoppingCart.Items[i].ShowVariantDropdown)
                        {
                            recurringVariantID = ctrlShoppingCart.Items[i].RecurringVariantId;

                            if (recurringVariantID != 0)
                                UpdateRecurringFrequency(sRecID, recurringVariantID, quantity);
                        }

                        cart.SetItemQuantity(sRecID, quantity);


                        cart.SetItemNotes(sRecID, CommonLogic.CleanLevelOne(itemNotes));
                    }
                }
                catch (Exception ex)
                {
                }
            }

            Updatecartcounttotalonmenue();


        }

        private void UpdateCurrentItemFundsUsed(String ProductCategoryFundUsed, String BluBucksUsed, String currentrecordid, String GLcode)
        {
            //String ProductCategoryFundUsed = GetSessionValue("ProductCategoryFundUsed");
            //String BluBucksUsed = GetSessionValue("BluBucksUsed");
            //String currentrecordid = GetSessionValue("currentrecordid");
            //String GLcode = String.IsNullOrEmpty(txtGLcode.Text) ? "" : txtGLcode.Text;
            try
            {
                cart.SetItemFundsUsed(Convert.ToInt32(currentrecordid), Convert.ToDecimal(ProductCategoryFundUsed), Convert.ToDecimal(BluBucksUsed), GLcode);
            }
            catch (Exception ex)
            {
            }
            SetSessionValue("ProductCategoryFundUsed");
            SetSessionValue("BluBucksUsed");
            SetSessionValue("currentrecordid");

        }



        private void Updatecartcounttotalonmenue()
        {
            System.Web.UI.HtmlControls.HtmlGenericControl shopping_cart;
            shopping_cart = (System.Web.UI.HtmlControls.HtmlGenericControl)Master.FindControl("shopping_cart");
            if (shopping_cart != null)
            {
                ShoppingCart objshoppingcart = new ShoppingCart(SkinID, ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);
                if (objshoppingcart != null)
                {
                    if (objshoppingcart.CartItems.Count > 0)
                    {
                        int quantity = 0;
                        foreach (CartItem citem in objshoppingcart.CartItems)
                        {
                            quantity = quantity + citem.Quantity;
                        }
                        shopping_cart.InnerText = "SHOPPING CART - " + quantity.ToString() + " -";
                    }
                    else
                    {
                        shopping_cart.InnerText = "SHOPPING CART";
                    }
                }
            }
            //end code to update total
        }
        private void UpdateRecurringFrequency(int shoppingCartRecID, int newVariantID, int quantity)
        {
            CartItem itemToUpdate = cart.CartItems.FirstOrDefault(ci => ci.ShoppingCartRecordID == shoppingCartRecID);

            if (itemToUpdate.VariantID != newVariantID) //If these 2 are different then the customer has actually chosen a different option.  Otherwise, skip it.
            {
                //Re-add the same item with just a couple of differences
                cart.AddItem(ThisCustomer, ThisCustomer.PrimaryShippingAddressID, itemToUpdate.ProductID, newVariantID, quantity, itemToUpdate.ChosenColor, itemToUpdate.ChosenColorSKUModifier, itemToUpdate.ChosenSize, itemToUpdate.ChosenSizeSKUModifier, itemToUpdate.TextOption, CartTypeEnum.ShoppingCart, true, false, 0, 0);

                //Delete the old item
                cart.SetItemQuantity(shoppingCartRecID, 0);

                //Same 'hack' as for deleting items - refresh the page to update the cart display
                Response.Redirect("shoppingcart.aspx");
            }
        }

        public void ProcessCart(bool DoingFullCheckout, bool ForceOnePageCheckout, bool InternationalCheckout)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            ThisCustomer.RequireCustomerRecord();
            CartTypeEnum cte = CartTypeEnum.ShoppingCart;
            if (CommonLogic.QueryStringCanBeDangerousContent("CartType").Length != 0)
            {
                cte = (CartTypeEnum)CommonLogic.QueryStringUSInt("CartType");
            }
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
            // UpdateCartQuantity();

            // save coupon code, no need to reload cart object
            // will update customer record also:
            if (cte == CartTypeEnum.ShoppingCart)
            {
                //cart.SetCoupon(txtGiftCard.Text, true);

                // kind of backwards, but if DisallowOrderNotes is false, then
                // allow order notes
                //if (!AppLogic.AppConfigBool("DisallowOrderNotes"))
                //{
                //    SqlParameter sp = new SqlParameter("@OrderNotes", SqlDbType.NText);
                //    sp.Value = txtOrderNotes.Text.Trim();
                //    SqlParameter[] spa = { sp };
                //    ThisCustomer.UpdateCustomer(spa);
                //}

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
                                int NewRecID = cart.AddItem(ThisCustomer, ThisCustomer.PrimaryShippingAddressID, ProductID, VariantID, 1, String.Empty, String.Empty, String.Empty, String.Empty, String.Empty, CartTypeEnum.ShoppingCart, true, false, 0, 0);
                                Decimal PR = AppLogic.GetUpsellProductPrice(0, ProductID, ThisCustomer.CustomerLevelID);
                                SqlParameter[] spa = { DB.CreateSQLParameter("@Price", SqlDbType.Decimal, 10, PR, ParameterDirection.Input), DB.CreateSQLParameter("@CartRecID", SqlDbType.Int, 4, NewRecID, ParameterDirection.Input) };
                                DB.ExecuteSQL("update shoppingcart set IsUpsell=1, ProductPrice=@Price where ShoppingCartRecID=@CartRecID", spa);

                            }
                        }
                    }
                    InitializeShoppingCartControl();
                }

                if (cart.CheckInventory(ThisCustomer.CustomerID))
                {
                    //lblErrorMessage.Text += Server.HtmlEncode(AppLogic.GetString("shoppingcart_process.aspx.1", SkinID, ThisCustomer.LocaleSetting));
                    // inventory got adjusted, send them back to the cart page to confirm the new values!
                }
            }

            if (cte == CartTypeEnum.WishCart)
            {
                Response.Redirect("wishlist.aspx");
            }
            if (cte == CartTypeEnum.GiftRegistryCart)
            {
                Response.Redirect("giftregistry.aspx");
            }

            cart.ClearShippingOptions();
            if (DoingFullCheckout)
            {
                bool validated = true;
                if (!cart.MeetsMinimumOrderAmount(AppLogic.AppConfigUSDecimal("CartMinOrderAmount")))
                {
                    validated = false;
                }

                if (!cart.MeetsMinimumOrderQuantity(AppLogic.AppConfigUSInt("MinCartItemsBeforeCheckout")))
                {
                    validated = false;
                }

                if (cart.ExceedsMaximumOrderQuantity(AppLogic.AppConfigUSInt("MaxCartItemsBeforeCheckout")))
                {
                    validated = false;
                }

                if (cart.HasCoupon() && !cart.CouponIsValid)
                {
                    validated = false;
                }

                if (validated)
                {

                    Response.Redirect(cart.PageToBeginCheckout(ForceOnePageCheckout, InternationalCheckout));
                }
                InitializePageContent(CheckOutPageControllerFactory.CreateCheckOutPageController(ThisCustomer, cart).GetCheckoutType());
            }

            //Make sure promotions is updated when the cart changes
            BindPromotions();
        }

        private void ClearErrors()
        {
            //lblCouponError.Text =
            //    lblErrorMessage.Text =
            //    lblInventoryTrimmedError.Text =
            //    lblRecurringScheduleConflictError.Text =
            //    lblMinimumQuantitiesUpdatedError.Text =
            //    lblMinimumOrderAmountError.Text =
            //    lblMinimumOrderQuantityError.Text =
            //    lblMaximumOrderQuantityError.Text = String.Empty;

            //pnlCouponError.Visible = false;
            //pnlPromoError.Visible = false;
        }

        private void ContinueShopping()
        {
            if (AppLogic.AppConfig("ContinueShoppingURL") == "")
            {
                if (ViewState["ReturnURL"].ToString() == "")
                {
                    Response.Redirect("default.aspx");
                }
                else
                {
                    Response.Redirect(ViewState["ReturnURL"].ToString());
                }
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

            if (CommonLogic.CookieCanBeDangerousContent("PayPalExpressToken", false).Length == 0)
            {
                if (!ThisCustomer.IsRegistered)
                {
                    bool paypalAnonymousAllowed = AppLogic.AppConfigBool("PasswordIsOptionalDuringCheckout") && AppLogic.AppConfigBool("PayPal.Express.AllowAnonCheckout");
                    if (cart.HasRecurringComponents() || !paypalAnonymousAllowed)
                        Response.Redirect("signin.aspx?ReturnUrl='shoppingcart.aspx'");
                }

                if (cart == null)
                {
                    cart = new ShoppingCart(SkinID, ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);
                }

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

        private void SetupShippingAndEstimateControl(ShippingAndTaxEstimateTableControl ctrlEstimate, Customer thisCustomer)
        {
            try
            {
                //Appconfig that need to look for
                bool skipShippingOnCheckout = AppLogic.AppConfigBool("SkipShippingOnCheckout");
                bool freeShippingAllowsRateSelection = AppLogic.AppConfigBool("FreeShippingAllowsRateSelection");
                bool vatEnable = AppLogic.AppConfigBool("VAT.Enabled");

                ShoppingCart cart = new ShoppingCart(1, thisCustomer, CartTypeEnum.ShoppingCart, 0, false);
                //Collect the available shipping method
                ShippingMethodCollection availableShippingMethods = cart.GetShippingMethods(thisCustomer.PrimaryShippingAddress, AppLogic.AppConfigBool("ShowInStorePickupInShippingEstimator"));

                //Initialize the caption of the control
                string shippingEstimateCaption = AppLogic.GetString("checkoutshipping.ShippingEstimateCaption", thisCustomer.SkinID, thisCustomer.LocaleSetting);
                ctrlEstimate.HeaderCaption = AppLogic.GetString("checkoutshipping.estimator.control.header", thisCustomer.SkinID, thisCustomer.LocaleSetting);
                ctrlEstimate.ShippingEstimateCaption = shippingEstimateCaption;
                ctrlEstimate.TaxEstimateCaption = AppLogic.GetString("checkoutshipping.TaxEstimateCaption", thisCustomer.SkinID, thisCustomer.LocaleSetting);
                ctrlEstimate.TotalEstimateCaption = AppLogic.GetString("checkoutshipping.TotalEstimateCaption", thisCustomer.SkinID, thisCustomer.LocaleSetting);
                ctrlEstimate.CaptionWidth = Unit.Percentage(50);
                ctrlEstimate.ValueWidth = Unit.Percentage(50);

                string inc = string.Empty;
                string lowestfreightName = string.Empty;
                decimal shippingTaxAmount = decimal.Zero;
                decimal SubTotal = cart.SubTotal(true, false, true, true, true, false);

                // Promotions: Line Item and Shipping discounts happen in the individual calculations so all that's left is to apply order level discounts.
                var orderDiscount = 0.00M;
                if (cart.CartItems.HasDiscountResults)
                    orderDiscount = cart.CartItems.DiscountResults.Sum(dr => dr.OrderTotal);

                SubTotal = SubTotal + orderDiscount;

                // Promotions: Because multiple promotions can be applied, it's possible to get a negative value, which should be caught and zeroed out.
                if (SubTotal < 0)
                    SubTotal = 0;

                decimal estimatedTax = decimal.Zero;
                decimal estimatedTotal = decimal.Zero;
                decimal estimatedShippingtotalWithTax = decimal.Zero;
                decimal lowestFreight = decimal.Zero;
                decimal result = decimal.Zero;
                bool lowestFreightMethodShippingIsFree = false;
                //If the vat is inclusive or exclusive
                bool vatInclusive = AppLogic.VATIsEnabled() && thisCustomer.VATSettingReconciled == VATSettingEnum.ShowPricesInclusiveOfVAT;

                //The lowest shipping method cost
                ShippingMethod lowestFreightMethod = availableShippingMethods.LowestFreight;

                if (vatEnable)
                {
                    //if VAT.Enabled is true remove the ':' at the end
                    //to have format like this 'Shipping (ex vat):' instead of 'Shipping: (ex vat)'
                    int count = shippingEstimateCaption.Length - 1;
                    ctrlEstimate.ShippingEstimateCaption = shippingEstimateCaption.Remove(count);
                }

                bool isAllFreeShippingComponents = cart.IsAllFreeShippingComponents();
                bool isAllDownloadComponents = cart.IsAllDownloadComponents();
                bool isAllEmailGiftCards = cart.IsAllEmailGiftCards();
                decimal freeShippingThreshold = AppLogic.AppConfigNativeDecimal("FreeShippingThreshold");
                bool isQualifiedForFreeShippingThreshold = freeShippingThreshold > 0 && freeShippingThreshold <= SubTotal;

                //Set the value for lowest freight and name
                if (!isAllFreeShippingComponents && !cart.ShippingIsFree && !skipShippingOnCheckout
                    || freeShippingAllowsRateSelection)
                {
                    if (availableShippingMethods.Count == 0)
                    { lowestFreight = 0; }
                    else
                    {
                        lowestFreight = lowestFreightMethod.Freight;
                        lowestFreightMethodShippingIsFree = lowestFreightMethod.ShippingIsFree;
                        lowestfreightName = lowestFreightMethod.GetNameForDisplay();
                    }

                    if (lowestFreight < 0)
                    { lowestFreight = 0; }

                    if (isQualifiedForFreeShippingThreshold && lowestFreightMethodShippingIsFree)
                    {
                        lowestFreight = 0;
                    }
                }


                //Computation of tax and shipping cost for non register customer
                if (!thisCustomer.IsRegistered || ThisCustomer.PrimaryShippingAddressID <= 0)
                {
                    decimal totalProduct = decimal.Zero;
                    decimal TaxShippingTotal = decimal.Zero;

                    //taxes for shipping
                    Decimal CountryShippingTaxRate = AppLogic.GetCountryTaxRate(thisCustomer.PrimaryShippingAddress.Country, AppLogic.AppConfigUSInt("ShippingTaxClassID"));
                    Decimal ZipShippingTaxRate = AppLogic.ZipTaxRatesTable.GetTaxRate(thisCustomer.PrimaryShippingAddress.Zip, AppLogic.AppConfigUSInt("ShippingTaxClassID"));
                    Decimal StateShippingTaxRate = AppLogic.GetStateTaxRate(thisCustomer.PrimaryShippingAddress.State, AppLogic.AppConfigUSInt("ShippingTaxClassID"));

                    foreach (CartItem ci in cart.CartItems)
                    {
                        Decimal StateTaxRate = AppLogic.GetStateTaxRate(thisCustomer.PrimaryShippingAddress.State, ci.TaxClassID);
                        Decimal CountryTaxRate = AppLogic.GetCountryTaxRate(thisCustomer.PrimaryShippingAddress.Country, ci.TaxClassID);
                        Decimal ZipTaxRate = AppLogic.ZipTaxRatesTable.GetTaxRate(thisCustomer.PrimaryShippingAddress.Zip, ci.TaxClassID);
                        Decimal DIDPercent = 0.0M;
                        Decimal DiscountedItemPrice = ci.Price * ci.Quantity;
                        QuantityDiscount.QuantityDiscountType fixedPriceDID = QuantityDiscount.QuantityDiscountType.Percentage;

                        //Handle the quantity discount
                        DIDPercent = QuantityDiscount.GetQuantityDiscountTablePercentageForLineItem(ci, out fixedPriceDID);
                        if (DIDPercent != 0.0M)
                        {
                            if (fixedPriceDID == QuantityDiscount.QuantityDiscountType.FixedAmount)
                            {
                                if (Currency.GetDefaultCurrency() == thisCustomer.CurrencySetting)
                                {
                                    DiscountedItemPrice = (ci.Price - DIDPercent) * ci.Quantity;

                                }
                                else
                                {
                                    DIDPercent = Decimal.Round(Currency.Convert(DIDPercent, Localization.StoreCurrency(), thisCustomer.CurrencySetting), 2, MidpointRounding.AwayFromZero);
                                    DiscountedItemPrice = (ci.Price - DIDPercent) * ci.Quantity;

                                }
                            }
                            else
                            {
                                DiscountedItemPrice = ((100.0M - DIDPercent) / 100.0M) * (ci.Price * ci.Quantity);
                            }
                        }

                        //Handle the coupon
                        if ((cart.GetCoupon().CouponType == CouponTypeEnum.OrderCoupon)
                            || (cart.GetCoupon().CouponType == CouponTypeEnum.ProductCoupon))
                        {
                            decimal discountPercent = cart.GetCoupon().DiscountPercent;
                            decimal discountAmount = cart.GetCoupon().DiscountAmount;

                            discountPercent = DiscountedItemPrice * (discountPercent / 100);
                            DiscountedItemPrice = DiscountedItemPrice - discountPercent;
                            DiscountedItemPrice = DiscountedItemPrice - (discountAmount / cart.CartItems.Count);

                        }

                        //Handle Gift Card
                        if (cart.Coupon.CouponType == CouponTypeEnum.GiftCard)
                        {
                            decimal giftCardAmount = cart.Coupon.DiscountAmount;
                            if (estimatedTotal > giftCardAmount)
                            {
                                estimatedTotal -= giftCardAmount;
                            }
                            else
                            {
                                giftCardAmount = estimatedTotal;
                                estimatedTotal = decimal.Zero;
                            }
                            ctrlEstimate.ShowGiftCardApplied = true;
                            ctrlEstimate.GiftCardAppliedCaption = AppLogic.GetString("checkoutshipping.estimator.control.GiftCardApplied", thisCustomer.SkinID, thisCustomer.LocaleSetting);
                            ctrlEstimate.GiftCardAppliedEstimate = Localization.CurrencyStringForDisplayWithExchangeRate(giftCardAmount, thisCustomer.CurrencySetting);
                        }

                        //Making sure to set it zero if DiscountedItemPrice becomes less than zero
                        if (DiscountedItemPrice < 0)
                        {
                            DiscountedItemPrice = 0;
                        }
                        if (ci.IsTaxable)
                        {
                            totalProduct = ZipTaxRate + CountryTaxRate + StateTaxRate;
                            totalProduct = (totalProduct / 100) * DiscountedItemPrice;
                            result += totalProduct;
                            estimatedTax = result;
                        }
                        //This will handle the order option
                        if (cart.OrderOptions.Count > 0)
                        {
                            //Then add it to the estimated tax
                            //JH - removed the following addtion as it doesn't make sense. Estimate tax should not have order option totals included
                            //estimatedTax += Prices.OrderOptionTotal(ThisCustomer, cart.OrderOptions);

                            int orderOptionTaxClassID = 0;
                            decimal estimatedTaxOnOrderOption = decimal.Zero;
                            decimal StateTaxRateForOrderOption = decimal.Zero;
                            decimal CountryTaxRateForOrderOption = decimal.Zero;
                            decimal ZipTaxRateForOrderOption = decimal.Zero;
                            decimal orderOptioncost = decimal.Zero;

                            foreach (int s_optionId in cart.OrderOptions.Select(o => o.ID))
                            {
                                //Check if it selected then apply the tax
                                if (cart.OptionIsSelected(s_optionId, cart.OrderOptions))
                                {
                                    //We need to get the cost per order option so we can compute
                                    //the tax base on taxclass id
                                    using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
                                    {
                                        conn.Open();
                                        string query = string.Format("Select TaxClassID,Cost from OrderOption WHERE OrderOptionID = {0}", s_optionId.ToString());

                                        using (IDataReader orderOptionreader = DB.GetRS(query, conn))
                                        {
                                            if (orderOptionreader.Read())
                                            {
                                                orderOptionTaxClassID = DB.RSFieldInt(orderOptionreader, "TaxClassID");
                                                orderOptioncost = DB.RSFieldDecimal(orderOptionreader, "Cost");
                                            }
                                        }

                                    }
                                    //Base on the taxclass id and address
                                    //JH - updated state tax rate to newer function calls
                                    //StateTaxRateForOrderOption = AppLogic.GetStateTaxRate(orderOptionTaxClassID, thisCustomer.PrimaryShippingAddress.State);
                                    StateTaxRateForOrderOption = AppLogic.GetStateTaxRate(thisCustomer.PrimaryShippingAddress.State, orderOptionTaxClassID);
                                    CountryTaxRateForOrderOption = AppLogic.GetCountryTaxRate(thisCustomer.PrimaryShippingAddress.Country, orderOptionTaxClassID);
                                    ZipTaxRateForOrderOption = AppLogic.ZipTaxRatesTable.GetTaxRate(thisCustomer.PrimaryShippingAddress.Zip, orderOptionTaxClassID);

                                    //Total first the tax base on the address 
                                    estimatedTaxOnOrderOption = StateTaxRateForOrderOption + CountryTaxRateForOrderOption + ZipTaxRateForOrderOption;
                                    //Then apply it to orderoption cost
                                    estimatedTaxOnOrderOption = (estimatedTaxOnOrderOption / 100) * orderOptioncost;
                                    //Then add it to the estimated tax
                                    estimatedTax += estimatedTaxOnOrderOption;
                                }
                            }
                        }

                        //Set it to zero if customerlevel has no tax
                        if (AppLogic.CustomerLevelHasNoTax(thisCustomer.CustomerLevelID))
                        {
                            estimatedTax = decimal.Zero;
                        }
                    }

                    if (!thisCustomer.IsRegistered && AppLogic.AppConfigBool("AvalaraTax.Enabled"))
                    {
                        estimatedTax = Prices.TaxTotal(ThisCustomer, cart.CartItems, estimatedShippingtotalWithTax, cart.OrderOptions);
                    }

                    TaxShippingTotal = lowestFreight;
                    if (StateShippingTaxRate != System.Decimal.Zero
                        || CountryShippingTaxRate != System.Decimal.Zero
                        || ZipShippingTaxRate != System.Decimal.Zero)
                    {
                        if (thisCustomer.CurrencySetting != Localization.GetPrimaryCurrency())
                        {
                            TaxShippingTotal = decimal.Round(Currency.Convert(TaxShippingTotal, Localization.StoreCurrency(), thisCustomer.CurrencySetting), 2, MidpointRounding.AwayFromZero);
                        }
                        estimatedTax += ((StateShippingTaxRate + CountryShippingTaxRate + ZipShippingTaxRate) / 100.0M) * TaxShippingTotal;//st;

                    }

                    estimatedTotal = estimatedTax + lowestFreight + SubTotal;

                }
                //Register Customer
                else
                {
                    if (isAllFreeShippingComponents &&
                        !freeShippingAllowsRateSelection &&
                        !skipShippingOnCheckout ||
                        isAllDownloadComponents ||
                        skipShippingOnCheckout ||
                        isAllEmailGiftCards)
                    {
                        estimatedTax = cart.TaxTotal();

                        if (vatInclusive)
                        {
                            estimatedTotal = SubTotal;
                        }
                        else
                        {
                            estimatedTotal = SubTotal + estimatedTax;
                        }

                        // apply gift card if any
                        if (cart.Coupon.CouponType == CouponTypeEnum.GiftCard)
                        {
                            decimal giftCardAmount = cart.Coupon.DiscountAmount;
                            if (estimatedTotal > giftCardAmount)
                            {
                                estimatedTotal -= giftCardAmount;
                            }
                            else
                            {
                                giftCardAmount = estimatedTotal;
                                estimatedTotal = decimal.Zero;
                            }
                            ctrlEstimate.ShowGiftCardApplied = true;
                            ctrlEstimate.GiftCardAppliedCaption = AppLogic.GetString("checkoutshipping.estimator.control.GiftCardApplied", thisCustomer.SkinID, thisCustomer.LocaleSetting);
                            ctrlEstimate.GiftCardAppliedEstimate = Localization.CurrencyStringForDisplayWithExchangeRate(giftCardAmount, thisCustomer.CurrencySetting);
                        }
                        else
                        {
                            //always set it false, in case user update it
                            ctrlEstimate.ShowGiftCardApplied = false;
                        }
                    }
                    else
                    {
                        decimal taxTotal;

                        if (AppLogic.CustomerLevelHasNoTax(thisCustomer.CustomerLevelID))
                        {
                            estimatedTax = decimal.Zero;
                            taxTotal = decimal.Zero;
                            shippingTaxAmount = decimal.Zero;
                            estimatedTotal = SubTotal + lowestFreight;
                        }
                        else if (vatInclusive)
                        {
                            // zero out the shipping total for now so that we can get the breakdown
                            decimal subTotalTaxAmount = Prices.TaxTotal(cart.ThisCustomer, cart.CartItems, System.Decimal.Zero, cart.OrderOptions);

                            int shippingTaxID = AppLogic.AppConfigUSInt("ShippingTaxClassID");
                            decimal shippingTaxRate = thisCustomer.TaxRate(shippingTaxID);
                            shippingTaxAmount = decimal.Round(lowestFreight * (shippingTaxRate / 100.0M), 2, MidpointRounding.AwayFromZero);

                            taxTotal = subTotalTaxAmount + shippingTaxAmount;
                            estimatedTax = taxTotal;
                            estimatedTotal = SubTotal + lowestFreight + shippingTaxAmount;
                        }
                        else
                        {
                            taxTotal = Prices.TaxTotal(cart.ThisCustomer, cart.CartItems, lowestFreight, cart.OrderOptions);
                            estimatedTax = taxTotal;
                            estimatedTotal = SubTotal + lowestFreight + taxTotal;
                        }

                        // apply gift card if any
                        if (cart.Coupon.CouponType == CouponTypeEnum.GiftCard)
                        {
                            decimal giftCardAmount = cart.Coupon.DiscountAmount;
                            if (estimatedTotal > giftCardAmount)
                            {
                                estimatedTotal -= giftCardAmount;
                            }
                            else
                            {
                                giftCardAmount = estimatedTotal;
                                estimatedTotal = decimal.Zero;
                            }
                            ctrlEstimate.ShowGiftCardApplied = true;
                            ctrlEstimate.GiftCardAppliedCaption = AppLogic.GetString("checkoutshipping.estimator.control.GiftCardApplied", thisCustomer.SkinID, thisCustomer.LocaleSetting);
                            ctrlEstimate.GiftCardAppliedEstimate = Localization.CurrencyStringForDisplayWithExchangeRate(giftCardAmount, thisCustomer.CurrencySetting);
                        }
                        else
                        {
                            //always set it false, in case user update it
                            ctrlEstimate.ShowGiftCardApplied = false;
                        }
                    }

                }


                //Assigning of value to the control
                if (isAllDownloadComponents
                  || availableShippingMethods.Count == 0
                  || skipShippingOnCheckout
                  || lowestFreightMethodShippingIsFree)
                {
                    string NoShippingRequire = string.Empty;
                    string shippingName = string.Empty;
                    if (cart.ShippingIsFree && !isAllDownloadComponents && !isAllEmailGiftCards && !cart.NoShippingRequiredComponents())
                    {
                        NoShippingRequire = AppLogic.GetString("checkoutshipping.estimator.control.FreeShipping", thisCustomer.SkinID, thisCustomer.LocaleSetting);
                    }
                    else if (string.IsNullOrEmpty(lowestfreightName))
                    {
                        NoShippingRequire = "N/A";
                    }
                    else
                    {
                        NoShippingRequire = AppLogic.GetString("checkoutshipping.estimator.control.Shipping", thisCustomer.SkinID, thisCustomer.LocaleSetting);
                    }

                    if (lowestFreightMethodShippingIsFree)
                    {
                        shippingName = " (" + lowestfreightName + ")";
                    }

                    if (!vatInclusive || !thisCustomer.IsRegistered)
                    {
                        if (vatEnable && !vatInclusive)
                        {
                            inc = " (" + AppLogic.GetString("setvatsetting.aspx.7", thisCustomer.SkinID, thisCustomer.LocaleSetting) + "):";

                        }

                        ctrlEstimate.ShippingEstimateCaption += inc + shippingName;

                        if (ctrlEstimate.ShippingEstimateCaption.LastIndexOf(":").Equals(-1))
                        {
                            ctrlEstimate.ShippingEstimateCaption += ":";
                        }

                        ctrlEstimate.ShippingEstimate = NoShippingRequire;
                        ctrlEstimate.TaxEstimate = Localization.CurrencyStringForDisplayWithExchangeRate(estimatedTax, thisCustomer.CurrencySetting);

                    }
                    else
                    {
                        if (vatEnable)
                        {
                            inc = " (" + AppLogic.GetString("setvatsetting.aspx.6", thisCustomer.SkinID, thisCustomer.LocaleSetting) + "):";
                        }
                        ctrlEstimate.ShowTax = false;
                        ctrlEstimate.ShippingEstimateCaption += inc + shippingName;
                        ctrlEstimate.ShippingEstimate = NoShippingRequire;
                    }
                }
                else if (lowestfreightName == "FREE SHIPPING (All Orders Have Free Shipping)"
                     || isAllFreeShippingComponents && !freeShippingAllowsRateSelection
                     || cart.ShippingIsFree && !freeShippingAllowsRateSelection
                     || isQualifiedForFreeShippingThreshold && lowestFreightMethodShippingIsFree)
                {
                    string Free = AppLogic.GetString("checkoutshipping.estimator.control.FreeShipping", thisCustomer.SkinID, thisCustomer.LocaleSetting);

                    if (thisCustomer.IsRegistered && vatInclusive)
                    {
                        if (vatEnable)
                        {
                            inc = " (" + AppLogic.GetString("setvatsetting.aspx.6", thisCustomer.SkinID, thisCustomer.LocaleSetting) + "):";
                        }
                        ctrlEstimate.ShippingEstimate = Free;
                        ctrlEstimate.ShippingEstimateCaption += inc;
                        ctrlEstimate.ShowTax = false;
                        ctrlEstimate.TaxEstimate = Localization.CurrencyStringForDisplayWithExchangeRate(estimatedTax, thisCustomer.CurrencySetting);
                    }
                    else
                    {
                        //Seperate tax and shipping cost even it is inclusive mode
                        //if non register so user will not be confused on the total computation
                        if (vatEnable && !vatInclusive)
                        {
                            inc = " (" + AppLogic.GetString("setvatsetting.aspx.7", thisCustomer.SkinID, thisCustomer.LocaleSetting) + "):";

                        }

                        ctrlEstimate.ShippingEstimateCaption += inc;

                        if (ctrlEstimate.ShippingEstimateCaption.LastIndexOf(":").Equals(-1))
                        {
                            ctrlEstimate.ShippingEstimateCaption += ":";
                        }

                        ctrlEstimate.ShippingEstimate = Free;
                        ctrlEstimate.ShowTax = true;
                        ctrlEstimate.TaxEstimate = Localization.CurrencyStringForDisplayWithExchangeRate(estimatedTax, thisCustomer.CurrencySetting);
                    }
                }
                else
                {
                    if (!vatInclusive)
                    {
                        if (vatEnable)
                        {
                            inc = "(" + AppLogic.GetString("setvatsetting.aspx.7", thisCustomer.SkinID, thisCustomer.LocaleSetting) + "):";

                        }
                        string shippingText = string.Format(" {0} ({1})", inc, lowestfreightName);
                        ctrlEstimate.ShippingEstimateCaption += shippingText;
                        ctrlEstimate.ShippingEstimate = Localization.CurrencyStringForDisplayWithExchangeRate(lowestFreight, thisCustomer.CurrencySetting);
                        ctrlEstimate.TaxEstimate = Localization.CurrencyStringForDisplayWithExchangeRate(estimatedTax, thisCustomer.CurrencySetting);
                        ctrlEstimate.TotalEstimate = Localization.CurrencyStringForDisplayWithExchangeRate(estimatedTotal, thisCustomer.CurrencySetting);
                    }
                    else
                    {
                        if (vatEnable)
                        {
                            inc = "(" + AppLogic.GetString("setvatsetting.aspx.6", thisCustomer.SkinID, thisCustomer.LocaleSetting) + "):";
                        }

                        if (thisCustomer.IsRegistered)
                        {
                            estimatedShippingtotalWithTax = (lowestFreight + shippingTaxAmount);
                        }
                        else
                        {
                            estimatedShippingtotalWithTax = (lowestFreight + shippingTaxAmount + estimatedTax);
                        }

                        string shippingText = string.Format(" {0} ({1})", inc, lowestfreightName);
                        ctrlEstimate.ShippingEstimateCaption += shippingText;
                        ctrlEstimate.ShippingEstimate = Localization.CurrencyStringForDisplayWithExchangeRate(estimatedShippingtotalWithTax, thisCustomer.CurrencySetting);
                        ctrlEstimate.ShowTax = false;
                    }
                }
                ctrlEstimate.TotalEstimate = Localization.CurrencyStringForDisplayWithExchangeRate(estimatedTotal, thisCustomer.CurrencySetting);
            }
            catch
            {
                ctrlEstimate.ShippingEstimate = "--";
                ctrlEstimate.TaxEstimate = "--";
                ctrlEstimate.TotalEstimate = "--";
                ctrlEstimate.HeaderCaption = AppLogic.GetString("checkoutshipping.estimator.control.header", thisCustomer.SkinID, thisCustomer.LocaleSetting);
                ctrlEstimate.ShippingEstimateCaption = AppLogic.GetString("checkoutshipping.ShippingEstimateCaption", thisCustomer.SkinID, thisCustomer.LocaleSetting);
                ctrlEstimate.TaxEstimateCaption = AppLogic.GetString("checkoutshipping.TaxEstimateCaption", thisCustomer.SkinID, thisCustomer.LocaleSetting);
                ctrlEstimate.TotalEstimateCaption = AppLogic.GetString("checkoutshipping.TotalEstimateCaption", thisCustomer.SkinID, thisCustomer.LocaleSetting);
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
            //txtGiftCard.Text = "";
            //btnRemoveGiftCard.Visible = false;
            UpdateCart();
        }

        private void BindPromotions()
        {
            //rptPromotions.DataSource = cart.DiscountResults.Select(dr => dr.Promotion);
            //rptPromotions.DataBind();
        }

        protected void rptPromotions_ItemDataBound(Object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
                return;

            Promotions.IPromotion promotion = e.Item.DataItem as Promotions.IPromotion;
            if (promotion == null)
                return;
        }

        protected void rptPromotions_ItemCommand(Object sender, RepeaterCommandEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
                return;

            PromotionManager.ClearPromotionUsages(ThisCustomer.CustomerID, e.CommandArgument.ToString(), true);
            UpdateCart();
            BindPromotions();
        }

        protected void btnAddPromotion_Click(Object sender, EventArgs e)
        {
            //String promotionCode = txtPromotionCode.Text.ToLower().Trim();
            //	txtPromotionCode.Text = String.Empty;
            //lblPromotionError.Text = String.Empty;

            //IEnumerable<IPromotionValidationResult> validationResults = PromotionManager.ValidatePromotion(promotionCode, PromotionManager.CreateRuleContext(cart));
            //if (validationResults.Count() > 0 && validationResults.Any(vr => !vr.IsValid))
            //{
            //    InitializeShippingAndEstimateControl();
            //    //pnlPromoError.Visible = true;
            //    foreach (var reason in validationResults.Where(vr => !vr.IsValid).SelectMany(vr => vr.Reasons))
            //    {
            //        String message = reason.MessageKey.StringResource();
            //        if (reason.ContextItems != null && reason.ContextItems.Any())
            //            foreach (var item in reason.ContextItems)
            //                message = message.Replace(String.Format("{{{0}}}", item.Key), item.Value.ToString());

            //        //lblPromotionError.Text += String.Format("<div class='promotion-reason'>{0}</div>", message);
            //    }
            //    return;
            //}
            //else
            //{
            //    //PromotionManager.AssignPromotion(ThisCustomer.CustomerID, promotionCode);
            //}

            UpdateCart();
            BindPromotions();
        }

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
    }
}
