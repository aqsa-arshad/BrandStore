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
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways;
using AspDotNetStorefrontGateways.Processors;
//
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore.ShippingCalculation;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for checkoutpayment.
    /// </summary>
    public partial class checkoutpayment : SkinBase
    {

        ShoppingCart cart = null;
        string GW = String.Empty;
        GatewayProcessor GWActual;
        bool useLiveTransactions = false;
        bool RequireTerms = false;
        string SelectedPaymentType = String.Empty;

        string AllowedPaymentMethods = String.Empty;
        decimal CartTotal = Decimal.Zero;
        decimal NetTotal = Decimal.Zero;
        bool FillControl = false;


        protected void Page_Init(object sender, EventArgs e)
        {
            // Set up CIM
            AspDotNetStorefrontGateways.Processors.AuthorizeNet authorizeNet = new AspDotNetStorefrontGateways.Processors.AuthorizeNet();
            // Need to set this here so it persists across loading viewstate
            ctrlCreditCardPanel.ShowCimSaveCard = authorizeNet.IsCimEnabled;

        }

        protected void Page_Load(object sender, EventArgs e)
        {
            //initially hide all error messages:
            if (this.IsPostBack)
            {
                if (ctrlPaymentMethod.CREDITCARDChecked)
                {
                    pnlErrorMsg.Visible = false;
                    
                }
                else
                {
                    valsumCreditCard.Visible = false;
                    valsumEcheck.Visible = false;
                    pnlCCTypeErrorMsg.Visible = false;
                    pnlCCExpDtErrorMsg.Visible = false;
                    CCTypeErrorMsgLabel.Visible = false;
                }
            }
            //valsumCreditCard.Visible = false;
            //valsumEcheck.Visible = false;
            //pnlCCTypeErrorMsg.Visible = false;
            //pnlCCExpDtErrorMsg.Visible = false;
            //CCTypeErrorMsgLabel.Visible = false;
            //CCExpDtErrorMsg.Visible = false;

         //   pnlErrorMsg.Attributes.Add("class", "hide-element");
            //End

            if (Request.QueryString["errormsg"] == "9" && !ctrlPaymentMethod.CREDITCARDChecked)
            {
                hdnPOChecked.Text = "1";
               
            }
            else
                hdnPOChecked.Text = "0";


            if (ctrlPaymentMethod.PURCHASEORDERChecked)
            {
                String PONumber = txtPO.Text.Trim();
                ErrorMessage err1;
                if (PONumber.Length == 0)
                {
                    hdnPOChecked.Text = "1";
                    err1 = new ErrorMessage(Server.HtmlEncode(AppLogic.GetString("checkoutpayment.aspx.21", ThisCustomer.SkinID, ThisCustomer.LocaleSetting)));
                    Response.Redirect("checkoutpayment.aspx?errormsg=" + err1.MessageId);
                }
            }

           
           
            // Set up CIM
            AspDotNetStorefrontGateways.Processors.AuthorizeNet authorizeNet = new AspDotNetStorefrontGateways.Processors.AuthorizeNet();
            PanelWallet.Visible = authorizeNet.IsCimEnabled;



            if (!this.IsPostBack)
            {
                AspDotNetStorefrontCore.Customer.Current.ThisCustomerSession["ActivePaymentProfileId"] = String.Empty;
                if (Request.UrlReferrer != null)
                {
                    if (Request.UrlReferrer.ToString().ToLower().Contains("shoppingcart"))
                    {
                        Session["hdnreferalurl"] = "ShoppingCart.aspx";

                    }
                    else if (Request.UrlReferrer.ToString().ToLower().Contains("checkoutshipping"))
                    {

                        Session["hdnreferalurl"] = "checkoutshipping.aspx?fillcontrols=true";
                    }
                    else if (Request.UrlReferrer.ToString().ToLower().Contains("checkoutreview"))
                    {

                        Session["hdnreferalurl"] = "checkoutshipping.aspx?fillcontrols=true";
                    }
                }
            }

            CimWalletSelector.PaymentProfileSelected += (o, l) =>
                {
                    ctrlPaymentMethod.ClearSelection();
                    ctrlPaymentMethod_OnPaymentMethodChanged(this, EventArgs.Empty);
                };

            // Set up discount
            if (cart.HasCoupon())
            {
                string CouponName = cart.Coupon.CouponCode;
                cart.ClearCoupon();
                cart = new ShoppingCart(SkinID, ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);
                cart.SetCoupon(CouponName, true);
            }

            Response.CacheControl = "private";
            Response.Expires = -1;
            Response.AddHeader("pragma", "no-cache");

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
            String PM = AppLogic.CleanPaymentMethod(CommonLogic.QueryStringCanBeDangerousContent("PaymentMethod").Trim());

            if (!ThisCustomer.IsRegistered)
            {
                bool boolAllowAnon = (AppLogic.AppConfigBool("PasswordIsOptionalDuringCheckout") && !cart.HasRecurringComponents());

                if (!boolAllowAnon && PM != "")
                {
                    if (PM == AppLogic.ro_PMPayPalExpress || PM == AppLogic.ro_PMPayPalExpressMark)
                    {
                        boolAllowAnon = AppLogic.AppConfigBool("PayPal.Express.AllowAnonCheckout");
                    }
                }

                if (!boolAllowAnon)
                {
                    Response.Redirect("createaccount.aspx?checkout=true");
                }
            }
            if (ThisCustomer.PrimaryBillingAddressID == 0 || (ThisCustomer.PrimaryShippingAddressID == 0 && !AppLogic.AppConfigBool("SkipShippingOnCheckout") && !cart.IsAllDownloadComponents() && !cart.IsAllSystemComponents() && !cart.NoShippingRequiredComponents() && !cart.IsAllEmailGiftCards()))
            {
                err = new ErrorMessage(Server.HtmlEncode(AppLogic.GetString("checkoutpayment.aspx.2", SkinID, ThisCustomer.LocaleSetting)));
                Response.Redirect("shoppingcart.aspx?resetlinkback=1&errormsg=" + err.MessageId);
            }

            SectionTitle = AppLogic.GetString("checkoutpayment.aspx.1", SkinID, ThisCustomer.LocaleSetting);

            if (cart.CartItems.Count == 0)
            {
                Response.Redirect("shoppingcart.aspx");
            }

            // re-validate all shipping info, as ANYTHING could have changed since last page:
            if (!cart.ShippingIsAllValid())
            {
                err = new ErrorMessage(Server.HtmlEncode(AppLogic.GetString("shoppingcart.cs.95", ThisCustomer.SkinID, ThisCustomer.LocaleSetting)));
                HttpContext.Current.Response.Redirect("shoppingcart.aspx?resetlinkback=1&errormsg=" + err.MessageId);
            }

            GW = AppLogic.ActivePaymentGatewayCleaned();
            GWActual = GatewayLoader.GetProcessor(GW);
            useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");
            RequireTerms = AppLogic.AppConfigBool("RequireTermsAndConditionsAtCheckout");

            //HERE WE WILL DO THE LOOKUP for the new supported Shipping2Payment mapping
            if (AppLogic.AppConfigBool("UseMappingShipToPayment"))
            {
                try
                {
                    int intCustomerSelectedShippingMethodID = cart.FirstItem().ShippingMethodID;

                    using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                    {
                        con.Open();
                        using (IDataReader rsReferencePMForSelectedShippingMethod = DB.GetRS("SELECT MappedPM FROM ShippingMethod WHERE ShippingMethodID=" + intCustomerSelectedShippingMethodID.ToString(), con))
                        {
                            while (rsReferencePMForSelectedShippingMethod.Read())
                            {
                                AllowedPaymentMethods = DB.RSField(rsReferencePMForSelectedShippingMethod, "MappedPM").ToUpperInvariant();
                            }
                        }
                    }

                    if (AllowedPaymentMethods.Length <= 0)
                    {
                        AllowedPaymentMethods = AppLogic.AppConfig("PaymentMethods").ToUpperInvariant();
                    }
                }
                catch
                {
                    AllowedPaymentMethods = AppLogic.AppConfig("PaymentMethods").ToUpperInvariant();
                }
            }
            else
            {
                AllowedPaymentMethods = AppLogic.AppConfig("PaymentMethods").ToUpperInvariant();
                if (AppLogic.MicropayIsEnabled() && !cart.HasSystemComponents())
                {
                    if (AllowedPaymentMethods.Length != 0)
                    {
                        AllowedPaymentMethods += ",";
                    }
                    AllowedPaymentMethods += AppLogic.ro_PMMicropay;
                }
            }

            SelectedPaymentType = CommonLogic.IIF(SelectedPaymentType == "" && ThisCustomer.RequestedPaymentMethod != "" && AppLogic.CleanPaymentMethod(AllowedPaymentMethods).IndexOf(ThisCustomer.RequestedPaymentMethod, StringComparison.InvariantCultureIgnoreCase) != -1, AppLogic.CleanPaymentMethod(ThisCustomer.RequestedPaymentMethod), "");
            CartTotal = cart.Total(true);
            NetTotal = CartTotal - CommonLogic.IIF(cart.Coupon.CouponType == CouponTypeEnum.GiftCard, CommonLogic.IIF(CartTotal < cart.Coupon.DiscountAmount, CartTotal, cart.Coupon.DiscountAmount), 0);


            if (!this.IsPostBack)
            {
                InitializePageContent();

            }


            ibAmazonSimplePay.ImageUrl = AppLogic.AppConfig("AMAZON.ButtonURL");

            GatewayCheckoutByAmazon.CheckoutByAmazon checkoutByAmazon = new GatewayCheckoutByAmazon.CheckoutByAmazon();
            if (checkoutByAmazon.IsEnabled && checkoutByAmazon.IsCheckingOut)
            {
                ctrlPaymentMethod.Visible = pnlCCPaneInfo.Visible = false; PanelWallet.Visible = false;
            }

            AppLogic.eventHandler("CheckoutPayment").CallEvent("&CheckoutPayment=true");
            // btnRequestEstimates_Click(null, null);


        }


        #region Web Form Designer generated code
        override protected void OnInit(EventArgs e)
        {
            //
            // CODEGEN: This call is required by the ASP.NET Web Form Designer.
            //
            InitializeComponent();
            cart = new ShoppingCart(SkinID, ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);
            if (cart.HasCoupon())
            {
                string CouponCode = cart.Coupon.CouponCode;
                cart.ClearCoupon();
                cart = new ShoppingCart(SkinID, ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);
                cart.SetCoupon(CouponCode, true);
                ctrlShoppingCart.DataSource = cart.CartItems;
                ctrlShoppingCart.DataBind();
                ctrlCartSummary.DataSource = cart;
                ctrlCartSummary.DataBind();
                InitializeOrderOptionControl();
                base.OnInit(e);
            }
            else
            {
                ctrlShoppingCart.DataSource = cart.CartItems;
                ctrlShoppingCart.DataBind();
                ctrlCartSummary.DataSource = cart;
                InitializeOrderOptionControl();
                base.OnInit(e);
            }
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnContinueCheckOut1.Click += new EventHandler(btnContinueCheckOut1_Click);

        }

        #endregion


        void btnContinueCheckOut1_Click(object sender, EventArgs e)
        {

            // ProcessPayment("CREDITCARD");
            btnContCheckout_Click(null, null);
        }


        /// <summary>
        /// Initializes the content of the page.
        /// </summary>
        private void InitializePageContent()
        {

            JSPopupRoutines.Text = AppLogic.GetJSPopupRoutines();

            // Set credit card pane to be visible if that payment method is allowed, and no other payment method
            // is trying to be shown: If UseMappingShipToPayment is not activated Credit Card will always be
            // the default payment option that shows expnanded to the customer.
            if (AppLogic.AppConfigBool("UseMappingShipToPayment"))
            {
                string[] strSplittedCurrentMappingsInDB = AllowedPaymentMethods.Split(new char[] { ',' });

                String PM = AppLogic.CleanPaymentMethod(strSplittedCurrentMappingsInDB[0]);
                if (PM == AppLogic.ro_PMMicropay)
                {
                    if (SelectedPaymentType.Length == 0 && AllowedPaymentMethods.IndexOf(AppLogic.ro_PMMicropay) != -1)
                    {
                        SelectedPaymentType = AppLogic.ro_PMMicropay;
                    }
                }
                else if (PM == AppLogic.ro_PMPurchaseOrder)
                {
                    if (SelectedPaymentType.Length == 0 && AllowedPaymentMethods.IndexOf(AppLogic.ro_PMPurchaseOrder) != -1)
                    {
                        SelectedPaymentType = AppLogic.ro_PMPurchaseOrder;
                    }
                }
                else if (PM == AppLogic.ro_PMCreditCard)
                {
                    if (SelectedPaymentType.Length == 0 && AllowedPaymentMethods.IndexOf(AppLogic.ro_PMCreditCard) != -1)
                    {
                        SelectedPaymentType = AppLogic.ro_PMCreditCard;
                    }
                }
                else if (PM == AppLogic.ro_PMPayPal)
                {
                    if (SelectedPaymentType.Length == 0 && AllowedPaymentMethods.IndexOf(AppLogic.ro_PMPayPal) != -1)
                    {
                        SelectedPaymentType = AppLogic.ro_PMPayPal;
                    }
                }
                else if (PM == AppLogic.ro_PMPayPalExpressMark)
                {
                    if (SelectedPaymentType.Length == 0 && AllowedPaymentMethods.IndexOf(AppLogic.ro_PMPayPalExpressMark) != -1)
                    {
                        SelectedPaymentType = AppLogic.ro_PMPayPalExpressMark;
                    }
                }
                else if (PM == AppLogic.ro_PMCOD)
                {
                    if (SelectedPaymentType.Length == 0 && AllowedPaymentMethods.IndexOf(AppLogic.ro_PMCOD) != -1)
                    {
                        SelectedPaymentType = AppLogic.ro_PMCOD;
                    }
                }
                else if (PM == AppLogic.ro_PMECheck)
                {
                    if (SelectedPaymentType.Length == 0 && AllowedPaymentMethods.IndexOf(AppLogic.ro_PMECheck) != -1)
                    {
                        SelectedPaymentType = AppLogic.ro_PMECheck;
                    }
                }
                else if (PM == AppLogic.ro_PMCheckByMail)
                {
                    if (SelectedPaymentType.Length == 0 && AllowedPaymentMethods.IndexOf(AppLogic.ro_PMCheckByMail) != -1)
                    {
                        SelectedPaymentType = AppLogic.ro_PMCheckByMail;
                    }
                }
                else if (PM == AppLogic.ro_PMRequestQuote)
                {
                    if (SelectedPaymentType.Length == 0 && AllowedPaymentMethods.IndexOf(AppLogic.ro_PMRequestQuote) != -1)
                    {
                        SelectedPaymentType = AppLogic.ro_PMRequestQuote;
                    }
                }
                else if (PM == AppLogic.ro_PMCODNet30)
                {
                    if (SelectedPaymentType.Length == 0 && AllowedPaymentMethods.IndexOf(AppLogic.ro_PMCODNet30) != -1)
                    {
                        SelectedPaymentType = AppLogic.ro_PMCODNet30;
                    }
                }
                else if (PM == AppLogic.ro_PMCODCompanyCheck)
                {
                    if (SelectedPaymentType.Length == 0 && AllowedPaymentMethods.IndexOf(AppLogic.ro_PMCODCompanyCheck) != -1)
                    {
                        SelectedPaymentType = AppLogic.ro_PMCODCompanyCheck;
                    }
                }
                else if (PM == AppLogic.ro_PMCODMoneyOrder)
                {
                    if (SelectedPaymentType.Length == 0 && AllowedPaymentMethods.IndexOf(AppLogic.ro_PMCODMoneyOrder) != -1)
                    {
                        SelectedPaymentType = AppLogic.ro_PMCODMoneyOrder;
                    }
                }
                else if (PM == AppLogic.ro_PMAmazonSimplePay)
                {
                    if (SelectedPaymentType.Length == 0 && AllowedPaymentMethods.IndexOf(AppLogic.ro_PMAmazonSimplePay) != -1)
                    {
                        SelectedPaymentType = AppLogic.ro_PMAmazonSimplePay;
                    }
                }
            }

            bool useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");

            if (CommonLogic.QueryStringNativeInt("ErrorMsg") > 0)
            {
                ErrorMessage e = new ErrorMessage(CommonLogic.QueryStringNativeInt("ErrorMsg"));
                pnlErrorMsg.Visible = true;
                ErrorMsgLabel.Text = Server.HtmlDecode(e.Message);
            }

            String XmlPackageName = AppLogic.AppConfig("XmlPackage.CheckoutPaymentPageHeader");
            if (XmlPackageName.Length != 0)
            {
                XmlPackage_CheckoutPaymentPageHeader.Text = AppLogic.RunXmlPackage(XmlPackageName, base.GetParser, ThisCustomer, SkinID, String.Empty, String.Empty, true, true);
            }

            if (NetTotal == System.Decimal.Zero && AppLogic.AppConfigBool("SkipPaymentEntryOnZeroDollarCheckout"))
            {
                NoPaymentRequired.Text = AppLogic.GetString("checkoutpayment.aspx.28", SkinID, ThisCustomer.LocaleSetting);
                pnlNoPaymentRequired.Visible = true;
                pnlPaymentOptions.Visible = false;
                pnlContCheckout.Visible = false;
                if (RequireTerms)
                {
                    StringBuilder s = new StringBuilder("");
                    s.Append(AppLogic.GetCheckoutTermsAndConditions(SkinID, ThisCustomer.LocaleSetting, base.GetParser, false));
                    NoPaymentRequired.Text = NoPaymentRequired.Text + "<br><br>" + s.ToString();  //Cannot concat types String and StringBuilder in VB
                    pnlRequireTerms.Visible = false;
                }

            }


            WritePaymentPanels();

            OrderSummary.Text = cart.DisplaySummary(true, true, true, true, false);
            AppLogic.GetButtonDisable(btnContinueCheckOut1);

        }

        /// <summary>
        /// Processes the post event.
        /// </summary>
        /// <param name="pm">The Payment Method.</param>
        private void ProcessPostEvent(string pm)
        {
            Address BillingAddress = new Address();
            BillingAddress.LoadByCustomer(ThisCustomer.CustomerID, ThisCustomer.PrimaryBillingAddressID, AddressTypes.Billing);
            BillingAddress.PaymentMethodLastUsed = pm;
            BillingAddress.UpdateDB();
            OrderSummary.Text = cart.DisplaySummary(true, true, true, true, false);
        }

        private void SetCreditCardPanelVisibility(Boolean IsVisible)
        {
            if (!IsVisible)
            {
                // pnlCCPane.Visible = CCPaneInfo.Visible = false;ctrlCreditCardPanel
                ctrlCreditCardPanel.Visible = CCPaneInfo.Visible = false;//Added this line to set credit card control visibility
                return;
            }

            if (NetTotal == System.Decimal.Zero && AppLogic.AppConfigBool("SkipPaymentEntryOnZeroDollarCheckout"))
            {
                pnlPaymentOptions.Visible = pnlContCheckout.Visible = false;
                return;
            }

            ctrlCreditCardPanel.Visible = true;
            pnlCCPaneInfo.Visible = false;

            string ccPaneData = GWActual == null ? null : GWActual.CreditCardPaneInfo(SkinID, ThisCustomer);
            GatewayCheckoutByAmazon.CheckoutByAmazon checkoutByAmazon = new GatewayCheckoutByAmazon.CheckoutByAmazon();

            if (!string.IsNullOrEmpty(ccPaneData))
            {
                CCPaneInfo.Text = ccPaneData;
                CCPaneInfo.Visible = true;
                //Make sure we don't hide the continue checkout button when checking out with amazon.  
                //CCPane might be selected because that was the last payment used by this customer.
                btnContCheckout.Visible = GWActual.ShowCheckoutButton || checkoutByAmazon.IsCheckingOut;
                // pnlCCPane.Visible = false;
                pnlCCPaneInfo.Visible = true;
            }
            else
            {
                pnlCCPaneInfo.Visible = false;
                if (GW != Gateway.ro_GWNETAXEPT)
                {
                    ctrlCreditCardPanel.Visible = true;
                }
            }
        }

        /// <summary>
        /// Writes the payment panels.
        /// </summary>
        private void WritePaymentPanels()
        {
            // When PAYPALPRO is active Gateway or PAYPALEXPRESS is available Payment Method
            // then we want to make the PayPal Express Mark available
            if ((AppLogic.ActivePaymentGatewayCleaned() == Gateway.ro_GWPAYPALPRO || AllowedPaymentMethods.IndexOf(AppLogic.ro_PMPayPalExpress) > -1)
                && AllowedPaymentMethods.IndexOf(AppLogic.ro_PMPayPalExpressMark) == -1)
            {
                if (AllowedPaymentMethods.Length != 0)
                {
                    AllowedPaymentMethods += ",";
                }
                AllowedPaymentMethods += AppLogic.ro_PMPayPalExpressMark;
            }

            if (AppLogic.SecureNetVaultIsEnabled() && !cart.HasRecurringComponents() && !cart.ContainsRecurring() && !cart.IsEmpty())
            {
                try
                {
                    List<CustomerVaultPayment> ds = SecureNetDataReport.GetCustomerVault(ThisCustomer.CustomerID).SavedPayments;
                    if (ds.Count > 0)
                    {
                        if (AllowedPaymentMethods.Length != 0)
                        {
                            AllowedPaymentMethods += ",";
                        }
                        AllowedPaymentMethods += AppLogic.ro_PMSecureNetVault;
                    }
                }
                catch { }
            }



            Address BillingAddress = new Address();
            BillingAddress.LoadByCustomer(ThisCustomer.CustomerID, ThisCustomer.PrimaryBillingAddressID, AddressTypes.Billing);
            bool EChecksAllowed = GWActual != null && GWActual.SupportsEChecks(); // let manual gw use echecks so site testing can occur
            bool POAllowed = AppLogic.CustomerLevelAllowsPO(ThisCustomer.CustomerLevelID);
            bool CODCompanyCheckAllowed = ThisCustomer.CODCompanyCheckAllowed;
            bool CODNet30Allowed = ThisCustomer.CODNet30Allowed;

            StringBuilder OrderFinalizationInstructions = new StringBuilder(4096);
            String OrderFinalizationXmlPackageName = AppLogic.AppConfig("XmlPackage.OrderFinalization");
            String OrderFinalizationXmlPackageFN = Server.MapPath("xmlpackages/" + OrderFinalizationXmlPackageName);

            if (CommonLogic.FileExists(OrderFinalizationXmlPackageFN))
            {
                OrderFinalizationInstructions.Append("<p align=\"left\"><b>" + AppLogic.GetString("checkoutreview.aspx.24", SkinID, ThisCustomer.LocaleSetting) + "</b></p>");
                OrderFinalizationInstructions.Append(AppLogic.RunXmlPackage(OrderFinalizationXmlPackageName, null, ThisCustomer, SkinID, string.Empty, string.Empty, false, false));
            }
            if (OrderFinalizationInstructions.Length != 0)
            {
                OrderFinalizationInstructions.Append("");
            }
            Finalization.Text = OrderFinalizationInstructions.ToString(); // set the no payment panel here, in case it is needed

            bool check = AllowedPaymentMethods.Split(',').Length <= 1;

            //If checkout by amazon we don't want to show normal payment methods
            GatewayCheckoutByAmazon.CheckoutByAmazon checkoutByAmazon = new GatewayCheckoutByAmazon.CheckoutByAmazon();
            if (!(checkoutByAmazon.IsEnabled && checkoutByAmazon.IsCheckingOut))
            {

                foreach (String PM in AllowedPaymentMethods.Split(','))
                {
                    String PMCleaned = AppLogic.CleanPaymentMethod(PM);
                    if (PMCleaned == AppLogic.ro_PMCreditCard)
                    {
                        pnlFinalization.Visible = Finalization.Text.Length != 0;
                        PMFinalization.Text = OrderFinalizationInstructions.ToString();

                        // You will fill the credit card info in their BBS UI Interface
                        if (GW == Gateway.ro_GWNETAXEPT)
                        {
                            SetCreditCardPanelVisibility(false);

                            ProcessPostEvent(AppLogic.CleanPaymentMethod(PM));

                            if (CommonLogic.QueryStringNativeInt("nexaxepterror") > 0)
                            {
                                ErrorMessage err = new ErrorMessage(CommonLogic.QueryStringNativeInt("nexaxepterror"));
                                if (err.Message.IndexOf(AppLogic.GetString("toc.aspx.6", ThisCustomer.SkinID, ThisCustomer.LocaleSetting)) != -1)
                                {
                                    if (AppLogic.AppConfigBool("NETAXEPT.Error.Setup"))
                                    {
                                        lblNetaxeptErrorMsg.Text = err.Message + "";
                                        lblNetaxeptErrorMsg.Visible = true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            bool fillcontrol = false;
                            if (Request.UrlReferrer != null)
                            {
                                String FromPage = Request.UrlReferrer.AbsolutePath;
                                String fill = Request.QueryString["fillcontrols"];

                                if (FromPage.Contains("checkoutshipping"))
                                {

                                    if (Request.UrlReferrer.ToString().Contains("fillcontrols"))
                                    {
                                        fillcontrol = true;
                                    }
                                    else
                                        fillcontrol = false;
                                }
                                else
                                    fillcontrol = true;
                            }

                            if (!IsPostBack & fillcontrol)
                            {
                                //ctrlPaymentMethod.CREDITCARDChecked = ((BillingAddress.PaymentMethodLastUsed == AppLogic.ro_PMCreditCard) || (BillingAddress.PaymentMethodLastUsed.Trim().Length <= 0 || check));
                                //SetCreditCardPanelVisibility(((BillingAddress.PaymentMethodLastUsed == AppLogic.ro_PMCreditCard) || (BillingAddress.PaymentMethodLastUsed.Trim().Length <= 0)));

                                // Display only non-CIM entries
                                if (!(BillingAddress.CardNumber.StartsWith("****") && AppLogic.GetCardExtraCodeFromSession(ThisCustomer) == "NA"))
                                {
                                    if (ctrlCreditCardPanel.CreditCardName == "")
                                    {
                                        //need commented line in future
                                        ctrlCreditCardPanel.CreditCardName = BillingAddress.CardName;
                                    }
                                    if (ctrlCreditCardPanel.CreditCardNumber == "")
                                    {
                                        //need commented line in future
                                        ctrlCreditCardPanel.CreditCardNumber = AppLogic.SafeDisplayCardNumber(BillingAddress.CardNumber, "Address", BillingAddress.AddressID);
                                    }
                                    if (ctrlCreditCardPanel.CreditCardVerCd == "")
                                    {
                                        //need commented line in future
                                        ctrlCreditCardPanel.CreditCardVerCd = AppLogic.SafeDisplayCardExtraCode(AppLogic.GetCardExtraCodeFromSession(ThisCustomer));
                                    }
                                    if (ctrlCreditCardPanel.CreditCardType == AppLogic.GetString("address.cs.32", SkinID, ThisCustomer.LocaleSetting))
                                    {
                                        //need commented line in future
                                        ctrlCreditCardPanel.CreditCardType = BillingAddress.CardType;
                                    }
                                    if (ctrlCreditCardPanel.CardExpMonth == AppLogic.GetString("address.cs.34", SkinID, ThisCustomer.LocaleSetting))
                                    {
                                        //need commented line in future
                                        ctrlCreditCardPanel.CardExpMonth = BillingAddress.CardExpirationMonth;
                                    }
                                    if (ctrlCreditCardPanel.CardExpYr == AppLogic.GetString("address.cs.35", SkinID, ThisCustomer.LocaleSetting))
                                    {
                                        //need commented line in future
                                        ctrlCreditCardPanel.CardExpYr = BillingAddress.CardExpirationYear;
                                    }
                                    if (!CommonLogic.IsStringNullOrEmpty(BillingAddress.CardStartDate))
                                    {
                                        if (ctrlCreditCardPanel.CardStartMonth == AppLogic.GetString("address.cs.34", SkinID, ThisCustomer.LocaleSetting))
                                        {
                                            ctrlCreditCardPanel.CardStartMonth = BillingAddress.CardStartDate.Substring(0, 2);
                                        }
                                        if (ctrlCreditCardPanel.CardStartYear == AppLogic.GetString("address.cs.35", SkinID, ThisCustomer.LocaleSetting))
                                        {
                                            ctrlCreditCardPanel.CardStartYear = BillingAddress.CardStartDate.Substring(2, 4);
                                        }
                                    }
                                    if (ctrlCreditCardPanel.CreditCardIssueNumber == "")
                                    {
                                        ctrlCreditCardPanel.CreditCardIssueNumber = BillingAddress.CardIssueNumber;
                                    }
                                }
                            }
                        }
                        ctrlPaymentMethod.ShowCREDITCARD = true;
                    }
                    else if (PMCleaned == AppLogic.ro_PMPurchaseOrder)
                    {
                        if (POAllowed)
                        {
                            ctrlPaymentMethod.ShowPURCHASEORDER = true;
                            PMFinalization.Text = OrderFinalizationInstructions.ToString();
                            pnlFinalization.Visible = Finalization.Text.Length != 0;

                            if (!IsPostBack)
                            {
                                ctrlPaymentMethod.PURCHASEORDERChecked = ((BillingAddress.PaymentMethodLastUsed == AppLogic.ro_PMPurchaseOrder) || check);
                                pnlPOPane.Visible = (BillingAddress.PaymentMethodLastUsed == AppLogic.ro_PMPurchaseOrder || check);
                            }

                            //Need these lines later
                            if (txtPO.Text == "")
                            {
                                txtPO.Text = BillingAddress.PONumber;
                            }
                        }
                    }
                    else if (PMCleaned == AppLogic.ro_PMCODMoneyOrder)
                    {
                        if (POAllowed)
                        {
                            ctrlPaymentMethod.ShowCODMONEYORDER = true;
                            PMFinalization.Text = OrderFinalizationInstructions.ToString();
                            pnlFinalization.Visible = Finalization.Text.Length != 0;

                            if (!IsPostBack)
                            {
                                ctrlPaymentMethod.CODMONEYORDERChecked = (BillingAddress.PaymentMethodLastUsed == AppLogic.ro_PMCODMoneyOrder || check);
                                pnlPOPane.Visible = (BillingAddress.PaymentMethodLastUsed == AppLogic.ro_PMCODMoneyOrder || check);
                            }

                            if (txtPO.Text == "")
                            {
                                txtPO.Text = BillingAddress.PONumber;
                            }
                        }
                    }
                    else if (PMCleaned == AppLogic.ro_PMCODCompanyCheck)
                    {
                        if (CODCompanyCheckAllowed)
                        {
                            ctrlPaymentMethod.ShowCODCOMPANYCHECK = true;
                            PMFinalization.Text = OrderFinalizationInstructions.ToString();
                            pnlFinalization.Visible = Finalization.Text.Length != 0;

                            if (!IsPostBack)
                            {
                                ctrlPaymentMethod.CODCOMPANYCHECKChecked = (BillingAddress.PaymentMethodLastUsed == AppLogic.ro_PMCODCompanyCheck || check);
                                pnlPOPane.Visible = (BillingAddress.PaymentMethodLastUsed == AppLogic.ro_PMCODCompanyCheck || check);
                            }

                            if (txtPO.Text == "")
                            {
                                txtPO.Text = BillingAddress.PONumber;
                            }
                        }
                    }
                    else if (PMCleaned == AppLogic.ro_PMCODNet30)
                    {
                        if (CODNet30Allowed)
                        {
                            ctrlPaymentMethod.ShowCODNET30 = true;
                            PMFinalization.Text = OrderFinalizationInstructions.ToString();
                            pnlFinalization.Visible = Finalization.Text.Length != 0;

                            if (!IsPostBack)
                            {
                                ctrlPaymentMethod.PAYPALChecked = (BillingAddress.PaymentMethodLastUsed == AppLogic.ro_PMCODNet30 || check);
                                pnlPOPane.Visible = (BillingAddress.PaymentMethodLastUsed == AppLogic.ro_PMCODNet30 || check);
                            }

                            if (txtPO.Text == "")
                            {
                                txtPO.Text = BillingAddress.PONumber;
                            }
                        }
                    }
                    else if (PMCleaned == AppLogic.ro_PMAmazonSimplePay)
                    {
                        ctrlPaymentMethod.ShowAmazonSimplePay = true;

                        if (!IsPostBack)
                        {
                            ctrlPaymentMethod.AMAZONSIMPLEPAYChecked = (BillingAddress.PaymentMethodLastUsed == AppLogic.ro_PMAmazonSimplePay || check);
                        }
                    }
                    else if (PMCleaned == AppLogic.ro_PMPayPal)
                    {
                        ctrlPaymentMethod.ShowPAYPAL = true;

                        if (!IsPostBack)
                        {
                            ctrlPaymentMethod.PAYPALChecked = (BillingAddress.PaymentMethodLastUsed == AppLogic.ro_PMPayPal || check);
                        }
                    }
                    else if (PMCleaned == AppLogic.ro_PMPayPalExpressMark)
                    {
                        if (!cart.IsEmpty())
                        {
                            ctrlPaymentMethod.ShowPAYPALEXPRESS = true;
                            PMFinalization.Text = OrderFinalizationInstructions.ToString();
                            pnlFinalization.Visible = Finalization.Text.Length != 0;

                            if (!IsPostBack)
                            {
                                ctrlPaymentMethod.PAYPALEXPRESSChecked = (BillingAddress.PaymentMethodLastUsed == AppLogic.ro_PMPayPalExpress || check);
                            }
                        }

                    }
                    else if (PMCleaned == AppLogic.ro_PMRequestQuote)
                    {
                        ctrlPaymentMethod.ShowREQUESTQUOTE = true;
                        PMFinalization.Text = OrderFinalizationInstructions.ToString();
                        pnlFinalization.Visible = Finalization.Text.Length != 0;

                        if (!IsPostBack)
                        {
                            ctrlPaymentMethod.REQUESTQUOTEChecked = (BillingAddress.PaymentMethodLastUsed == AppLogic.ro_PMRequestQuote || check);
                        }
                    }
                    else if (PMCleaned == AppLogic.ro_PMCheckByMail)
                    {
                        ctrlPaymentMethod.ShowCHECKBYMAIL = true;
                        PMFinalization.Text = OrderFinalizationInstructions.ToString();
                        pnlFinalization.Visible = Finalization.Text.Length != 0;

                        if (!IsPostBack)
                        {
                            ctrlPaymentMethod.CHECKBYMAILChecked = (BillingAddress.PaymentMethodLastUsed == AppLogic.ro_PMCheckByMail || check);
                        }
                    }
                    else if (PMCleaned == AppLogic.ro_PMCardinalMyECheck)
                    {
                        ctrlPaymentMethod.ShowCARDINALMYECHECK = true;
                        PMFinalization.Text = OrderFinalizationInstructions.ToString();
                        pnlFinalization.Visible = Finalization.Text.Length != 0;

                        if (!IsPostBack)
                        {
                            ctrlPaymentMethod.CARDINALMYECHECKChecked = (BillingAddress.PaymentMethodLastUsed == AppLogic.ro_PMCardinalMyECheck || check);
                        }
                    }
                    else if (PMCleaned == AppLogic.ro_PMCOD)
                    {
                        ctrlPaymentMethod.ShowCOD = true;
                        PMFinalization.Text = OrderFinalizationInstructions.ToString();
                        pnlFinalization.Visible = Finalization.Text.Length != 0;

                        if (!IsPostBack)
                        {
                            ctrlPaymentMethod.CODChecked = (BillingAddress.PaymentMethodLastUsed == AppLogic.ro_PMCOD || check);
                        }
                    }
                    else if (PMCleaned == AppLogic.ro_PMECheck)
                    {
                        if (EChecksAllowed)
                        {
                            pnlFinalization.Visible = Finalization.Text.Length != 0;
                            ctrlEcheck.ECheckBankABAImage1 = AppLogic.LocateImageURL("~/App_Themes/skin_" + SkinID.ToString() + "/images/check_aba.gif");
                            ctrlEcheck.ECheckBankABAImage2 = AppLogic.LocateImageURL("~/App_Themes/skin_" + SkinID.ToString() + "/images/check_aba.gif");
                            ctrlEcheck.ECheckBankAccountImage = AppLogic.LocateImageURL("~/App_Themes/skin_" + SkinID.ToString() + "/images/check_account.gif");
                            ctrlEcheck.ECheckNoteLabel = string.Format(AppLogic.GetString("address.cs.48", SkinID, ThisCustomer.LocaleSetting), AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/check_micr.gif"));

                            if (!IsPostBack)
                            {
                                ctrlPaymentMethod.ECHECKChecked = (BillingAddress.PaymentMethodLastUsed == AppLogic.ro_PMECheck || check);
                                pnlEcheckPane.Visible = (BillingAddress.PaymentMethodLastUsed == AppLogic.ro_PMECheck || check);

                                if (ctrlEcheck.ECheckBankAccountName == "")
                                {
                                    ctrlEcheck.ECheckBankAccountName = BillingAddress.ECheckBankAccountName;
                                }
                                if (ctrlEcheck.ECheckBankName == "")
                                {
                                    ctrlEcheck.ECheckBankName = BillingAddress.ECheckBankName;
                                }
                                if (ctrlEcheck.ECheckBankABACode == "")
                                {
                                    ctrlEcheck.ECheckBankABACode = AppLogic.SafeDisplayCardNumber(BillingAddress.ECheckBankABACode, "Address", BillingAddress.AddressID);
                                }
                                if (ctrlEcheck.ECheckBankAccountNumber == "")
                                {
                                    ctrlEcheck.ECheckBankAccountNumber = AppLogic.SafeDisplayCardNumber(BillingAddress.ECheckBankAccountNumber, "Address", BillingAddress.AddressID);
                                }
                            }

                            ctrlPaymentMethod.ShowECHECK = true;

                        }
                    }
                    else if (PMCleaned == AppLogic.ro_PMMicropay)
                    {
                        if (AppLogic.MicropayIsEnabled())
                        {
                            ctrlPaymentMethod.ShowMICROPAY = true;

                            if (ctrlPaymentMethod.MICROPAYChecked)
                            {
                                PMFinalization.Text = OrderFinalizationInstructions.ToString();
                                pnlFinalization.Visible = (ThisCustomer.MicroPayBalance >= NetTotal && PMFinalization.Text.Length != 0);
                                btnContCheckout.Visible = ThisCustomer.MicroPayBalance >= NetTotal;
                                ctrlPaymentMethod.ShowMICROPAYMessage = ThisCustomer.MicroPayBalance < NetTotal;
                                ctrlPaymentMethod.MICROPAYLabel = String.Format(AppLogic.GetString("checkoutpayment.aspx.26", SkinID, ThisCustomer.LocaleSetting), ThisCustomer.CurrencyString(ThisCustomer.MicroPayBalance));
                            }
                            else
                            {
                                btnContCheckout.Visible = true;
                                ctrlPaymentMethod.ShowMICROPAYMessage = false;
                            }

                            if (!IsPostBack)
                            {
                                ctrlPaymentMethod.MICROPAYChecked = (BillingAddress.PaymentMethodLastUsed == AppLogic.ro_PMMicropay || check);
                            }
                        }
                    }
                    else if (PMCleaned == AppLogic.ro_PMMoneybookersQuickCheckout)
                    {
                        if (!cart.HasRecurringComponents() && !cart.ContainsRecurring() && !cart.IsEmpty())
                        {
                            ctrlPaymentMethod.ShowMONEYBOOKERSQUICKCHECKOUT = true;
                            PMFinalization.Text = OrderFinalizationInstructions.ToString();
                            pnlFinalization.Visible = Finalization.Text.Length != 0;

                            if (!IsPostBack)
                            {
                                ctrlPaymentMethod.MONEYBOOKERSQUICKCHECKOUTChecked = (BillingAddress.PaymentMethodLastUsed == AppLogic.ro_PMMoneybookersQuickCheckout || check);
                            }
                        }
                    }
                    else if (PMCleaned == AppLogic.ro_PMPayPalEmbeddedCheckout)
                    {
                        ctrlPaymentMethod.ShowPAYPALEMBEDDEDCHECKOUT = true;

                        PMFinalization.Text = OrderFinalizationInstructions.ToString();
                        pnlFinalization.Visible = Finalization.Text.Length != 0;

                        if (!IsPostBack)
                        {
                            ctrlPaymentMethod.PAYPALEMBEDDEDCHECKOUTChecked = (BillingAddress.PaymentMethodLastUsed == AppLogic.ro_PMPayPalEmbeddedCheckout ||
                                BillingAddress.PaymentMethodLastUsed == AppLogic.ro_PMPayPalExpress ||
                                BillingAddress.PaymentMethodLastUsed == AppLogic.ro_PMPayPalExpressMark ||
                                BillingAddress.PaymentMethodLastUsed == AppLogic.ro_PMPayPal || check);
                        }
                    }
                    else if (PMCleaned == AppLogic.ro_PMSecureNetVault)
                    {
                        ctrlPaymentMethod.ShowSECURENETVAULT = true;

                        PMFinalization.Text = OrderFinalizationInstructions.ToString();
                        pnlFinalization.Visible = Finalization.Text.Length != 0;

                        if (!IsPostBack)
                        {
                            ctrlPaymentMethod.SECURENETVAULTChecked = (BillingAddress.PaymentMethodLastUsed == AppLogic.ro_PMSecureNetVault || check);
                        }
                    }
                    RequireTermsandConditions.Text = AppLogic.GetCheckoutTermsAndConditions(SkinID, ThisCustomer.LocaleSetting, base.GetParser, false);
                }


                if (ctrlPaymentMethod.ShowCREDITCARD && ctrlPaymentMethod.ShowPAYPALEMBEDDEDCHECKOUT)
                {
                    if (ctrlPaymentMethod.CREDITCARDChecked)
                        ctrlPaymentMethod.PAYPALEMBEDDEDCHECKOUTChecked = true;
                    ctrlPaymentMethod.ShowCREDITCARD = ctrlPaymentMethod.CREDITCARDChecked = false;
                }

            }

            SetCreditCardPanelVisibility(ctrlPaymentMethod.CREDITCARDChecked);

            pnlEcheckPane.Visible = ctrlPaymentMethod.ECHECKChecked;
            pnlPOPane.Visible = ctrlPaymentMethod.PURCHASEORDERChecked || ctrlPaymentMethod.CODMONEYORDERChecked || ctrlPaymentMethod.CODCOMPANYCHECKChecked || ctrlPaymentMethod.CODNET30Checked;

            if (!IsPostBack && ctrlPaymentMethod.MONEYBOOKERSQUICKCHECKOUTChecked)
            {
                pnlExternalPaymentMethod.Visible = true;
                PerformMoneybookersQuickCheckout();
            }

            if (!IsPostBack && ctrlPaymentMethod.SECURENETVAULTChecked)
            {
                pnlSecureNetVaultPayment.Visible = true;
                SetupSecureNetVaultPayment();
            }

            if (!IsPostBack && ctrlPaymentMethod.PAYPALEMBEDDEDCHECKOUTChecked)
            {
                pnlPayPalEmbeddedCheckout.Visible = true;
                PreformPayPalEmbeddedCheckout();
            }

            if (ErrorMsgLabel.Text.Length <= 0)
            {
                pnlErrorMsg.Visible = false;
            }

            Boolean GWRequiresFinalization = GWActual != null && GWActual.RequiresFinalization();

            if (
                    ctrlPaymentMethod.PAYPALChecked ||
                    (ThisCustomer.MicroPayBalance < NetTotal && ctrlPaymentMethod.MICROPAYChecked) ||
                    (ctrlPaymentMethod.CREDITCARDChecked && GWRequiresFinalization)
                )
            {
                pnlFinalization.Visible = false;
            }


            if (ctrlPaymentMethod.CREDITCARDChecked)
            {
                SetCreditCardPanelVisibility(true);
            }
            else
            {
                pnlCCPaneInfo.Visible = false;
            }
            if (ctrlPaymentMethod.CARDINALMYECHECKChecked == true)
            {
                //  pnlCardinaleCheckTopic.Visible = true;
            }
            else
            {
                pnlCardinaleCheckTopic.Visible = false;
            }
        }

        private void PreformPayPalEmbeddedCheckout()
        {
            btnContCheckout.Visible = false;
            string returnUrl = string.Format("{0}fp-paypalembeddedcheckoutok.aspx", AppLogic.GetStoreHTTPLocation(true));
            string errorUrl = string.Format("{0}fp-paypalembeddedcheckoutok.aspx", AppLogic.GetStoreHTTPLocation(true));
            string cancelUrl = string.Format("{0}fp-checkoutpayment.aspx", AppLogic.GetStoreHTTPLocation(true));
            string notifyUrl = string.Format("{0}paypalnotification.aspx", AppLogic.GetStoreHTTPLocation(true));
            string silentPostURL = string.Format("{0}paypalembeddedcheckoutok.aspx", AppLogic.GetStoreHTTPLocation(true));
            PayPalEmbeddedCheckoutSecureTokenResponse response = PayFlowProController.GetFramedHostedCheckout(cart, ThisCustomer.PrimaryShippingAddress, returnUrl, errorUrl, cancelUrl, notifyUrl, silentPostURL);

            if (response.Result != 0)
                throw new Exception(AppLogic.GetString("paypalpaymentsadvanced.configerror", ThisCustomer.LocaleSetting));

            Session["PayPalEmbeddedCheckoutSecureToken"] = response.SecureToken;
            Session["PayPalEmbeddedCheckoutSecureTokenId"] = response.SecureTokenID;

            string frameSrc = response.GetFrameSrc(0, 400);
            litPayPalEmbeddedCheckoutFrame.Text = frameSrc;
        }

        /// <summary>
        /// Processes the payment.
        /// </summary>
        /// <param name="PaymentMethod">The payment method.</param>
        private void ProcessPayment(string PaymentMethod)
        {
            if (NetTotal > System.Decimal.Zero || !AppLogic.AppConfigBool("SkipPaymentEntryOnZeroDollarCheckout"))
            {
                AppLogic.ValidatePM(CommonLogic.QueryStringCanBeDangerousContent("PaymentMethod")); // this WILL throw a hard security exception on any problem!
            }
            String PM = AppLogic.CleanPaymentMethod(PaymentMethod);

            ProcessPostEvent(PM);

            if (!ThisCustomer.IsRegistered)
            {
                bool boolAllowAnon = (AppLogic.AppConfigBool("PasswordIsOptionalDuringCheckout") && !cart.HasRecurringComponents());

                if (!boolAllowAnon && (PaymentMethod == AppLogic.ro_PMPayPalExpress || PaymentMethod == AppLogic.ro_PMPayPalExpressMark))
                {
                    boolAllowAnon = AppLogic.AppConfigBool("PayPal.Express.AllowAnonCheckout");
                }

                if (!boolAllowAnon)
                {
                    Response.Redirect("signin.aspx?checkout=true");//createaccount
                }
            }

            ErrorMessage err;
            if (ThisCustomer.PrimaryBillingAddressID == 0 || ThisCustomer.PrimaryShippingAddressID == 0)
            {
                err = new ErrorMessage(Server.HtmlEncode(AppLogic.GetString("checkoutpayment.aspx.2", SkinID, ThisCustomer.LocaleSetting)));
                Response.Redirect("shoppingcart.aspx?resetlinkback=1&errormsg=" + err.MessageId);
            }

            if (cart.IsEmpty())
            {
                Response.Redirect("shoppingcart.aspx?resetlinkback=1");
            }

            if (cart.InventoryTrimmed)
            {
                err = new ErrorMessage(Server.HtmlEncode(AppLogic.GetString("shoppingcart.aspx.3", SkinID, ThisCustomer.LocaleSetting)));
                Response.Redirect("shoppingcart.aspx?resetlinkback=1&errormsg=" + err.MessageId);
            }

            if (cart.RecurringScheduleConflict)
            {
                err = new ErrorMessage(Server.HtmlEncode(AppLogic.GetString("shoppingcart.aspx.19", SkinID, ThisCustomer.LocaleSetting)));
                Response.Redirect("shoppingcart.aspx?resetlinkback=1&errormsg=" + err.MessageId);
            }

            if (cart.HasCoupon() && !cart.CouponIsValid)
            {
                Response.Redirect("shoppingcart.aspx?resetlinkback=1&discountvalid=false");
            }

            if (!cart.MeetsMinimumOrderAmount(AppLogic.AppConfigUSDecimal("CartMinOrderAmount")))
            {
                Response.Redirect("shoppingcart.aspx?resetlinkback=1");
            }

            if (!cart.MeetsMinimumOrderQuantity(AppLogic.AppConfigUSInt("MinCartItemsBeforeCheckout")))
            {
                Response.Redirect("shoppingcart.aspx?resetlinkback=1");
            }

            if (cart.ExceedsMaximumOrderQuantity(AppLogic.AppConfigUSInt("MaxCartItemsBeforeCheckout")))
            {
                Response.Redirect("shoppingcart.aspx?resetlinkback=1");
            }

            // re-validate all shipping info, as ANYTHING could have changed since last page:
            if (!cart.ShippingIsAllValid())
            {
                err = new ErrorMessage(Server.HtmlEncode(AppLogic.GetString("shoppingcart.cs.95", ThisCustomer.SkinID, ThisCustomer.LocaleSetting)));
                HttpContext.Current.Response.Redirect("shoppingcart.aspx?resetlinkback=1&errormsg=" + err.MessageId);
            }

            Address BillingAddress = new Address();
            BillingAddress.LoadByCustomer(ThisCustomer.CustomerID, ThisCustomer.PrimaryBillingAddressID, AddressTypes.Billing);

            // ----------------------------------------------------------------
            // Get the finalization info (if any):
            // ----------------------------------------------------------------
            StringBuilder FinalizationXml = new StringBuilder(4096);
            FinalizationXml.Append("<root>");
            for (int i = 0; i < Request.Form.Count; i++)
            {
                String FieldName = Request.Form.Keys[i];
                String FieldVal = Request.Form[Request.Form.Keys[i]].Trim();
                if (FieldName != null && FieldName.StartsWith("finalization", StringComparison.InvariantCultureIgnoreCase) && !FieldName.EndsWith("_vldt", StringComparison.InvariantCultureIgnoreCase))
                {
                    FinalizationXml.Append("<field>");
                    FinalizationXml.Append("<" + XmlCommon.XmlEncode(FieldName) + ">");
                    FinalizationXml.Append(XmlCommon.XmlEncode(FieldVal));
                    FinalizationXml.Append("</" + XmlCommon.XmlEncode(FieldName) + ">");
                    FinalizationXml.Append("</field>");
                }
            }
            FinalizationXml.Append("</root>");
            DB.ExecuteSQL(String.Format("update customer set FinalizationData={0} where CustomerID={1}", DB.SQuote(FinalizationXml.ToString()), ThisCustomer.CustomerID.ToString()));

            // ----------------------------------------------------------------
            // Store the payment info (if required):
            // ----------------------------------------------------------------
            if (PaymentMethod.Length == 0)
            {
                err = new ErrorMessage(Server.HtmlEncode(AppLogic.GetString("checkoutpayment.aspx.20", ThisCustomer.SkinID, ThisCustomer.LocaleSetting)));
                Response.Redirect("checkoutpayment.aspx?errormsg=" + err.MessageId);
            }
            if (PM.StartsWith(AppLogic.ro_PMCreditCard))
            {
                String CardName = ctrlCreditCardPanel.CreditCardName.Trim();
                String CardNumber = ctrlCreditCardPanel.CreditCardNumber.Trim().Replace(" ", "");
                String CardExtraCode = ctrlCreditCardPanel.CreditCardVerCd.Trim().Replace(" ", "");
                String strCardType = ctrlCreditCardPanel.CreditCardType.Trim();
                String CardExpirationMonth = ctrlCreditCardPanel.CardExpMonth.Trim().Replace(" ", "");
                String CardExpirationYear = ctrlCreditCardPanel.CardExpYr.Trim().Replace(" ", "");

                String CardStartDate = ctrlCreditCardPanel.CardStartMonth.Replace(" ", "").PadLeft(2, '0') + ctrlCreditCardPanel.CardStartYear.Trim().Replace(" ", "");
                String CardIssueNumber = ctrlCreditCardPanel.CreditCardIssueNumber.Trim().Replace(" ", "");

                // Save CIM payment profile id
                if (CimWalletSelector.SelectedPaymentProfileId > 0)
                {
                    NetTotal = 1.0m;
                    var paymentProfile = GatewayAuthorizeNet.DataUtility.GetPaymentProfileWrapper(ThisCustomer.CustomerID, ThisCustomer.EMail, CimWalletSelector.SelectedPaymentProfileId);
                    CardNumber = paymentProfile.CreditCardNumberMasked;
                    CardExpirationYear = paymentProfile.ExpirationYear;
                    CardExpirationMonth = paymentProfile.ExpirationMonth;
                    strCardType = paymentProfile.CardType;
                    CardExtraCode = "NA";
                }

                if (CardNumber.StartsWith("*") && CimWalletSelector.SelectedPaymentProfileId <= 0)
                {
                    // Still obscured in the form so use the original
                    CardNumber = BillingAddress.CardNumber;
                }

                if (CardExtraCode.StartsWith("*"))
                {
                    // Still obscured in the form so use the original
                    CardExtraCode = AppLogic.GetCardExtraCodeFromSession(ThisCustomer);
                }

                if (AppLogic.AppConfigBool("ValidateCreditCardNumbers"))
                {
                    BillingAddress.PaymentMethodLastUsed = AppLogic.ro_PMCreditCard;
                    BillingAddress.CardName = CardName;
                    BillingAddress.CardExpirationMonth = CardExpirationMonth;
                    BillingAddress.CardExpirationYear = CardExpirationYear;
                    BillingAddress.CardStartDate = CommonLogic.IIF((CardStartDate == "00" || !CommonLogic.IsNumber(CardStartDate)), String.Empty, CardStartDate);
                    BillingAddress.CardIssueNumber = CardIssueNumber;

                    CardType Type = CardType.Parse(strCardType);
                    CreditCardValidator validator = new CreditCardValidator(CardNumber, Type);
                    bool isValid = validator.Validate();

                    BillingAddress.CardType = strCardType;
                    if (!isValid)
                    {
                        CardNumber = string.Empty;
                        // clear the card extra code
                        AppLogic.StoreCardExtraCodeInSession(ThisCustomer, string.Empty);
                    }
                    BillingAddress.CardNumber = CardNumber;
                    BillingAddress.UpdateDB();

                    if (!isValid)
                    {
                        if (NetTotal == System.Decimal.Zero && AppLogic.AppConfigBool("SkipPaymentEntryOnZeroDollarCheckout"))
                        {
                            //NOTE: Suppress error and allow zero amount to checkout
                        }
                        else
                        {
                            err = new ErrorMessage(Server.HtmlEncode(AppLogic.GetString("checkoutcard_process.aspx.3", 1, Localization.GetDefaultLocale())));
                            Response.Redirect("checkoutpayment.aspx?errormsg=" + err.MessageId);
                        }
                    }

                }

                // store in appropriate session, encrypted, so it can be used when the order is actually "entered"
                AppLogic.StoreCardExtraCodeInSession(ThisCustomer, CardExtraCode);


                if (NetTotal == System.Decimal.Zero && AppLogic.AppConfigBool("SkipPaymentEntryOnZeroDollarCheckout"))
                {
                    // remember their info:
                    BillingAddress.PaymentMethodLastUsed = AppLogic.ro_PMCreditCard;
                    BillingAddress.ClearCCInfo();
                    BillingAddress.UpdateDB();
                }
                else
                {
                    // remember their info:                    
                    BillingAddress.PaymentMethodLastUsed = AppLogic.ro_PMCreditCard;
                    BillingAddress.CardName = CardName;
                    BillingAddress.CardType = strCardType;
                    BillingAddress.CardNumber = CardNumber;
                    BillingAddress.CardExpirationMonth = CardExpirationMonth;
                    BillingAddress.CardExpirationYear = CardExpirationYear;
                    BillingAddress.CardStartDate = CommonLogic.IIF((CardStartDate == "00" || !CommonLogic.IsNumber(CardStartDate)), String.Empty, CardStartDate);
                    BillingAddress.CardIssueNumber = CardIssueNumber;
                    BillingAddress.UpdateDB();

                    if (CardNumber.Length == 0)
                    {
                        err = new ErrorMessage(Server.HtmlEncode(AppLogic.GetString("checkoutcard_process.aspx.1", 1, Localization.GetDefaultLocale())));
                        Response.Redirect("checkoutpayment.aspx?errormsg=" + err.MessageId);
                    }
                    if ((!AppLogic.AppConfigBool("CardExtraCodeIsOptional") && CardExtraCode.Length == 0))
                    {
                        err = new ErrorMessage(Server.HtmlEncode(AppLogic.GetString("checkoutcard_process.aspx.5", 1, Localization.GetDefaultLocale())));
                        Response.Redirect("checkoutpayment.aspx?errormsg=" + err.MessageId);
                    }

                    // Save card to CIM if selected
                    if (ctrlCreditCardPanel.CimSaveCard)
                    {
                        string errorMessage, errorCode;
                        Int64 saveCardProfileId = 0;

                        saveCardProfileId = GatewayAuthorizeNet.ProcessTools.SaveProfileAndPaymentProfile(
                            ThisCustomer.CustomerID, ThisCustomer.EMail, AspDotNetStorefrontCore.AppLogic.AppConfig("StoreName"),
                            saveCardProfileId, BillingAddress.AddressID, CardNumber, CardExtraCode, CardExpirationMonth, CardExpirationYear,
                            out errorMessage, out errorCode);

                        if (saveCardProfileId <= 0 && errorCode != "E00039")
                        {
                            err = new ErrorMessage(Server.HtmlEncode(errorMessage));
                            Response.Redirect("checkoutpayment.aspx?errormsg=" + err.MessageId);
                        }
                    }
                }
            }
            else if (PM == AppLogic.ro_PMPurchaseOrder)
            {
                String PONumber = txtPO.Text.Trim();
                if (PONumber.Length == 0)
                {
                    err = new ErrorMessage(Server.HtmlEncode(AppLogic.GetString("checkoutpayment.aspx.21", ThisCustomer.SkinID, ThisCustomer.LocaleSetting)));
                    Response.Redirect("checkoutpayment.aspx?errormsg=" + err.MessageId);
                }

                // remember their info:
                BillingAddress.PaymentMethodLastUsed = AppLogic.ro_PMPurchaseOrder;
                BillingAddress.PONumber = PONumber;
                if (!ThisCustomer.MasterShouldWeStoreCreditCardInfo)
                {
                    BillingAddress.ClearCCInfo();
                }
                BillingAddress.UpdateDB();
            }
            else if (PM == AppLogic.ro_PMCODMoneyOrder)
            {
                String PONumber = CommonLogic.FormCanBeDangerousContent("PONumber");
                if (PONumber.Length == 0)
                {
                    err = new ErrorMessage(Server.HtmlEncode(AppLogic.GetString("checkoutpayment.aspx.21", ThisCustomer.SkinID, ThisCustomer.LocaleSetting)));
                    Response.Redirect("checkoutpayment.aspx?errormsg=" + err.MessageId);
                }
                // remember their info:
                BillingAddress.PaymentMethodLastUsed = AppLogic.ro_PMCODMoneyOrder;
                BillingAddress.PONumber = PONumber;
                if (!ThisCustomer.MasterShouldWeStoreCreditCardInfo)
                {
                    BillingAddress.ClearCCInfo();
                }
                BillingAddress.UpdateDB();
            }
            else if (PM == AppLogic.ro_PMCODCompanyCheck)
            {
                String PONumber = txtPO.Text.Trim();
                if (PONumber.Length == 0)
                {
                    err = new ErrorMessage(Server.HtmlEncode(AppLogic.GetString("checkoutpayment.aspx.21", ThisCustomer.SkinID, ThisCustomer.LocaleSetting)));
                    Response.Redirect("checkoutpayment.aspx?errormsg=" + err.MessageId);
                }
                // remember their info:
                BillingAddress.PaymentMethodLastUsed = AppLogic.ro_PMCODCompanyCheck;
                BillingAddress.PONumber = txtPO.Text.Trim();
                if (!ThisCustomer.MasterShouldWeStoreCreditCardInfo)
                {
                    BillingAddress.ClearCCInfo();
                }
                BillingAddress.UpdateDB();
            }
            else if (PM == AppLogic.ro_PMCODNet30)
            {
                String PONumber = txtPO.Text.Trim();
                if (PONumber.Length == 0)
                {
                    err = new ErrorMessage(Server.HtmlEncode(AppLogic.GetString("checkoutpayment.aspx.21", ThisCustomer.SkinID, ThisCustomer.LocaleSetting)));
                    Response.Redirect("checkoutpayment.aspx?errormsg=" + err.MessageId);
                }
                // remember their info:
                BillingAddress.PaymentMethodLastUsed = AppLogic.ro_PMCODNet30;
                BillingAddress.PONumber = PONumber;
                if (!ThisCustomer.MasterShouldWeStoreCreditCardInfo)
                {
                    BillingAddress.ClearCCInfo();
                }
                BillingAddress.UpdateDB();
            }
            else if (PM == AppLogic.ro_PMPayPal)
            {
                return; // They need to click the button on this page to get sent to paypal.com
            }
            else if (PM == AppLogic.ro_PMRequestQuote)
            {
                // no action required here
                BillingAddress.PaymentMethodLastUsed = AppLogic.ro_PMRequestQuote;
                if (!ThisCustomer.MasterShouldWeStoreCreditCardInfo)
                {
                    BillingAddress.ClearCCInfo();
                }
                BillingAddress.UpdateDB();
            }
            else if (PM == AppLogic.ro_PMCheckByMail)
            {
                // no action required here
                BillingAddress.PaymentMethodLastUsed = AppLogic.ro_PMCheckByMail;
                if (!ThisCustomer.MasterShouldWeStoreCreditCardInfo)
                {
                    BillingAddress.ClearCCInfo();
                }
                BillingAddress.UpdateDB();
            }
            else if (PM == AppLogic.ro_PMCOD)
            {
                // no action required here
                BillingAddress.PaymentMethodLastUsed = AppLogic.ro_PMCOD;
                if (!ThisCustomer.MasterShouldWeStoreCreditCardInfo)
                {
                    BillingAddress.ClearCCInfo();
                }
                BillingAddress.UpdateDB();
            }
            else if (PM == AppLogic.ro_PMECheck)
            {
                string validFailureReason = string.Empty;
                if (!ctrlEcheck.Validate(out validFailureReason))
                {
                    err = new ErrorMessage(Server.HtmlEncode(validFailureReason));
                    Response.Redirect("checkoutpayment.aspx?errormsg=" + err.MessageId);
                }


                BillingAddress.PaymentMethodLastUsed = AppLogic.ro_PMECheck;
                BillingAddress.ECheckBankName = ctrlEcheck.ECheckBankName;

                if (!ctrlEcheck.ECheckBankABACode.StartsWith("*"))
                {
                    BillingAddress.ECheckBankABACode = ctrlEcheck.ECheckBankABACode;
                }

                if (!ctrlEcheck.ECheckBankAccountNumber.StartsWith("*"))
                {
                    BillingAddress.ECheckBankAccountNumber = ctrlEcheck.ECheckBankAccountNumber;
                }

                BillingAddress.ECheckBankAccountType = ctrlEcheck.ECheckBankAccountType;
                BillingAddress.ECheckBankAccountName = ctrlEcheck.ECheckBankAccountName;
                BillingAddress.UpdateDB();
            }
            else if (PM == AppLogic.ro_PMCardinalMyECheck)
            {
                String ACSUrl;
                String Payload;
                String TransID;
                String LookupResult;
                int OrderNumber = AppLogic.GetNextOrderNumber();
                if (Cardinal.MyECheckLookup(cart, OrderNumber, NetTotal, AppLogic.AppConfig("StoreName") + " Purchase", out ACSUrl, out Payload, out TransID, out LookupResult))
                {
                    BillingAddress.PaymentMethodLastUsed = AppLogic.ro_PMCardinalMyECheck;
                    if (!ThisCustomer.MasterShouldWeStoreCreditCardInfo)
                    {
                        BillingAddress.ClearCCInfo();
                    }
                    BillingAddress.UpdateDB();

                    ThisCustomer.ThisCustomerSession["Cardinal.LookupResult"] = LookupResult;
                    ThisCustomer.ThisCustomerSession["Cardinal.ACSUrl"] = ACSUrl;
                    ThisCustomer.ThisCustomerSession["Cardinal.Payload"] = Payload;
                    ThisCustomer.ThisCustomerSession["Cardinal.TransactionID"] = TransID;
                    ThisCustomer.ThisCustomerSession["Cardinal.OrderNumber"] = OrderNumber.ToString();

                    Response.Redirect("cardinalecheckform.aspx");
                }
                else
                {
                    // MyECheck transaction failed to start, return to checkoutpayment with error message
                    err = new ErrorMessage(Server.HtmlEncode(AppLogic.GetString("checkoutpayment.aspx.39", ThisCustomer.SkinID, ThisCustomer.LocaleSetting)));
                    Response.Redirect("checkoutpayment.aspx?errormsg=" + err.MessageId);
                }
            }
            else if (PM == AppLogic.ro_PMMicropay)
            {
                BillingAddress.PaymentMethodLastUsed = AppLogic.ro_PMMicropay;
                if (!ThisCustomer.MasterShouldWeStoreCreditCardInfo)
                {
                    BillingAddress.ClearCCInfo();
                }
                BillingAddress.UpdateDB();
            }
            else if (PM == AppLogic.ro_PMPayPalExpress || PM == AppLogic.ro_PMPayPalExpressMark)
            {
                BillingAddress.PaymentMethodLastUsed = PM;
                if (!ThisCustomer.MasterShouldWeStoreCreditCardInfo)
                {
                    BillingAddress.ClearCCInfo();
                }
                BillingAddress.UpdateDB();

                Address shippingAddress = new Address();
                shippingAddress.LoadByCustomer(ThisCustomer.CustomerID, ThisCustomer.PrimaryShippingAddressID, AddressTypes.Shipping);
                String sURL = Gateway.StartExpressCheckout(cart, shippingAddress, null);
                Response.Redirect(sURL);
            }
            else if (PM == AppLogic.ro_PMAmazonSimplePay)
            {
                BillingAddress.PaymentMethodLastUsed = PM;
                if (!ThisCustomer.MasterShouldWeStoreCreditCardInfo)
                {
                    BillingAddress.ClearCCInfo();
                }
                BillingAddress.UpdateDB();

                Response.Redirect("amazonpane.aspx");
            }
            else if (PM == AppLogic.ro_PMSecureNetVault)
            {
                BillingAddress.PaymentMethodLastUsed = PM;
                if (rblSecureNetVaultMethods.SelectedIndex < 0)
                {
                    lblSecureNetMessage.Text = "securenet.selectamethod".StringResource();
                    lblSecureNetMessage.Visible = true;
                    return;
                }
                AppLogic.StoreSelectedSecureNetVault(ThisCustomer, rblSecureNetVaultMethods.SelectedValue);
                if (!ThisCustomer.MasterShouldWeStoreCreditCardInfo)
                {
                    BillingAddress.ClearCCInfo();
                    BillingAddress.UpdateDB();
                }
            }

            Response.Redirect("checkoutreview.aspx?paymentmethod=" + Server.UrlEncode(PaymentMethod));
        }

        private void PerformMoneybookersQuickCheckout()
        {
            var gateway = GatewayLoader.GetProcessor("Moneybookers");

            if (gateway == null)
                throw new Exception("Skrill (Moneybookers) Quick Checkout requires the Moneybookers gateway.");

            var quickCheckout = ((IExternalPaymentMethodProvider)gateway).GetExternalPaymentMethod("Quick Checkout");

            if (quickCheckout == null)
                throw new Exception("The installed version of the Skrill (Moneybookers) gateway does not support Quick Checkout.");

            var quickCheckoutContext = quickCheckout.BeginCheckout(cart);

            btnContCheckout.Visible = false;

            Address billingAddress = new Address();
            billingAddress.LoadByCustomer(ThisCustomer.CustomerID, ThisCustomer.PrimaryBillingAddressID, AddressTypes.Billing);
            billingAddress.PaymentMethodLastUsed = AppLogic.ro_PMMoneybookersQuickCheckout;

            if (!ThisCustomer.MasterShouldWeStoreCreditCardInfo)
                billingAddress.ClearCCInfo();

            billingAddress.UpdateDB();

            if (quickCheckoutContext.RedirectUrl == null)
                throw new Exception("There was an error calling Skrill (Moneybookers) Quick Checkout. Please select a different payment method or contact customer support.");

            string frameSrc = quickCheckoutContext.RedirectUrl;

            if (quickCheckoutContext.RedirectParameters.Any())
            {
                string paramList = String.Join("&", quickCheckoutContext.RedirectParameters.Select(kvp => String.Format("{0}={1}", kvp.Key, kvp.Value)).ToArray());
                frameSrc = String.Format("{0}?{1}", frameSrc, paramList);
            }

            ExternalPaymentMethodFrame.Style.Add(HtmlTextWriterStyle.Height, "500px");	// Default for Moneybookers Quick Checkout
            ExternalPaymentMethodFrame.Style.Add(HtmlTextWriterStyle.Width, "100%");
            ExternalPaymentMethodFrame.Attributes.Add("src", frameSrc);
        }

        protected void ctrlPaymentMethod_OnPaymentMethodChanged(object sender, EventArgs e)
        {
            
            if (ctrlPaymentMethod.HasPaymentMethodSelected)
                CimWalletSelector.ClearSelection();

            WritePaymentPanels();
            pnlEcheckPane.Visible = ctrlPaymentMethod.ECHECKChecked;
            pnlPOPane.Visible = ctrlPaymentMethod.PURCHASEORDERChecked || ctrlPaymentMethod.CODMONEYORDERChecked || ctrlPaymentMethod.CODCOMPANYCHECKChecked || ctrlPaymentMethod.CODNET30Checked;
            pnlContCheckout.Visible = !ctrlPaymentMethod.AMAZONSIMPLEPAYChecked;
            pnlAmazonContCheckout.Visible = ctrlPaymentMethod.AMAZONSIMPLEPAYChecked;
            pnlSecureNetVaultPayment.Visible = ctrlPaymentMethod.SECURENETVAULTChecked;
            if (ctrlPaymentMethod.SECURENETVAULTChecked)
                SetupSecureNetVaultPayment();

            pnlExternalPaymentMethod.Visible = ctrlPaymentMethod.MONEYBOOKERSQUICKCHECKOUTChecked;
            btnContCheckout.Visible = !pnlExternalPaymentMethod.Visible;
            SetCreditCardPanelVisibility(ctrlPaymentMethod.CREDITCARDChecked && GW != Gateway.ro_GWNETAXEPT && !ctrlPaymentMethod.AMAZONSIMPLEPAYChecked);

            pnlSecureNetVaultPayment.Visible = ctrlPaymentMethod.SECURENETVAULTChecked;
            pnlPayPalEmbeddedCheckout.Visible = ctrlPaymentMethod.PAYPALEMBEDDEDCHECKOUTChecked;
            if (ctrlPaymentMethod.PAYPALEMBEDDEDCHECKOUTChecked)
                PreformPayPalEmbeddedCheckout();

            if (ctrlPaymentMethod.MONEYBOOKERSQUICKCHECKOUTChecked)
                PerformMoneybookersQuickCheckout();
          
        }

        private void SetupSecureNetVaultPayment()
        {
            if (AppLogic.SecureNetVaultIsEnabled())
            {
                if (rblSecureNetVaultMethods.Items.Count == 0)
                {
                    List<CustomerVaultPayment> ds = SecureNetDataReport.GetCustomerVault(ThisCustomer.CustomerID).SavedPayments;
                    rblSecureNetVaultMethods.Items.Clear();
                    foreach (CustomerVaultPayment payment in ds)
                    {
                        String rbText = String.Format("<strong>{0}</strong>: {1} <strong>{2}</strong>: {3}", "account.creditcardprompt".StringResource(), payment.CardNumberPadded, "account.expires".StringResource(), payment.ExpDateFormatted);
                        String rbValue = payment.PaymentId;
                        ListItem add = new ListItem(rbText, rbValue);
                        add.Selected = (rbValue == AppLogic.GetSelectedSecureNetVault(ThisCustomer));
                        rblSecureNetVaultMethods.Items.Add(add);
                    }
                    if (ds.Count == 0)
                    {
                        lblSecureNetMessage.Text = "securenet.nosavedmethods".StringResource();
                    }
                }
            }
        }

        protected void btnAmazonCheckout_Click(object sender, EventArgs e)
        {
            if (ctrlPaymentMethod.AMAZONSIMPLEPAYChecked)
            {
                ProcessPayment(AppLogic.ro_PMAmazonSimplePay);
            }
        }

        protected void btnContCheckout_Click(object sender, EventArgs e)
        {         
                   
            String PONumber = txtPO.Text.Trim();
            ErrorMessage err;
            if (PONumber.Length == 0)
            {
                err = new ErrorMessage(Server.HtmlEncode(AppLogic.GetString("checkoutpayment.aspx.21", ThisCustomer.SkinID, ThisCustomer.LocaleSetting)));
                Response.Redirect("checkoutpayment.aspx?errormsg=" + err.MessageId);
            }

            if (RequireTerms && CommonLogic.FormCanBeDangerousContent("TermsAndConditionsRead") == "")
            {
                bool bTermsRead = false;
                if (!bTermsRead)
                {
                    pnlErrorMsg.Visible = true;
                    ErrorMsgLabel.Text = Server.HtmlEncode(AppLogic.GetString("checkoutpayment.aspx.15", SkinID, ThisCustomer.LocaleSetting)).Replace("+", " ");
                    return;
                }
            }

            GatewayCheckoutByAmazon.CheckoutByAmazon checkoutByAmazon = new GatewayCheckoutByAmazon.CheckoutByAmazon();
            if (checkoutByAmazon.IsEnabled && checkoutByAmazon.IsCheckingOut)
            {
                ProcessPayment(GatewayCheckoutByAmazon.CheckoutByAmazon.CBA_Gateway_Identifier);
            }
            else if (CimWalletSelector.SelectedPaymentProfileId != 0)
            {
                ProcessPayment(AppLogic.ro_PMCreditCard);
            }
            else if (ctrlPaymentMethod.CREDITCARDChecked)
            {
                Address BillingAddress = new Address();
                BillingAddress.LoadByCustomer(ThisCustomer.CustomerID, ThisCustomer.PrimaryBillingAddressID, AddressTypes.Billing);
                BillingAddress.PaymentMethodLastUsed = AppLogic.ro_PMCreditCard;
                BillingAddress.UpdateDB();

                if (GWActual != null && !string.IsNullOrEmpty(GWActual.ProcessingPageRedirect()))
                {
                    Response.Redirect(GWActual.ProcessingPageRedirect());
                }
                else
                {
                    //Hide all error messages initially added by tayyab on 16-2-2015
                    valsumCreditCard.Visible = true;
                    valsumEcheck.Visible = true;
                    pnlCCTypeErrorMsg.Visible = true;
                    pnlCCExpDtErrorMsg.Visible = true;
                    CCTypeErrorMsgLabel.Visible = true;
                    //End

                    Page.Validate("creditcard");

                    if (ctrlCreditCardPanel.CreditCardType == AppLogic.GetString("address.cs.32", SkinID, ThisCustomer.LocaleSetting))
                    {
                        pnlCCTypeErrorMsg.Visible = true;
                    }
                    else { pnlCCTypeErrorMsg.Visible = false; }
                    if (ctrlCreditCardPanel.CardExpMonth == AppLogic.GetString("address.cs.34", SkinID, ThisCustomer.LocaleSetting) || ctrlCreditCardPanel.CardExpYr == AppLogic.GetString("address.cs.35", SkinID, ThisCustomer.LocaleSetting))
                    {
                        pnlCCExpDtErrorMsg.Visible = true;
                    }
                    else { pnlCCExpDtErrorMsg.Visible = false; }

                    if (Page.IsValid && !(pnlCCTypeErrorMsg.Visible || pnlCCExpDtErrorMsg.Visible))
                    {
                        ProcessPayment(AppLogic.ro_PMCreditCard);
                    }
                }
            }
            else if (ctrlPaymentMethod.PURCHASEORDERChecked)
            {
                ProcessPayment(AppLogic.ro_PMPurchaseOrder);
            }
            else if (ctrlPaymentMethod.CODMONEYORDERChecked)
            {
                ProcessPayment(AppLogic.ro_PMCODMoneyOrder);
            }
            else if (ctrlPaymentMethod.CODCOMPANYCHECKChecked)
            {
                ProcessPayment(AppLogic.ro_PMCODCompanyCheck);
            }
            else if (ctrlPaymentMethod.CODNET30Checked)
            {
                ProcessPayment(AppLogic.ro_PMCODNet30);
            }
            else if (ctrlPaymentMethod.PAYPALChecked)
            {
                Response.Redirect("paypalpane.aspx");
            }
            else if (ctrlPaymentMethod.REQUESTQUOTEChecked)
            {
                ProcessPayment(AppLogic.ro_PMRequestQuote);
            }
            else if (ctrlPaymentMethod.CHECKBYMAILChecked)
            {
                ProcessPayment(AppLogic.ro_PMCheckByMail);
            }
            else if (ctrlPaymentMethod.CODChecked)
            {
                ProcessPayment(AppLogic.ro_PMCOD);
            }
            else if (ctrlPaymentMethod.ECHECKChecked)
            {
                Page.Validate("echeck");
                if (Page.IsValid)
                {
                    ProcessPayment(AppLogic.ro_PMECheck);
                }
            }
            else if (ctrlPaymentMethod.CARDINALMYECHECKChecked)
            {
                ProcessPayment(AppLogic.ro_PMCardinalMyECheck);
            }
            else if (ctrlPaymentMethod.MICROPAYChecked)
            {
                ProcessPayment(AppLogic.ro_PMMicropay);
            }
            else if (ctrlPaymentMethod.PAYPALEXPRESSChecked)
            {
                ProcessPayment(AppLogic.ro_PMPayPalExpressMark);
            }
            else if (ctrlPaymentMethod.SECURENETVAULTChecked)
            {
                ProcessPayment(AppLogic.ro_PMSecureNetVault);
            }
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


        #region "Estimates"
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

                    // pnlShippingAndTaxEstimator.Controls.Add(ctrlEstimate);
                }
                // hide the estimate button
                ToggleShowHideEstimate();

                //Clean up the partial address that was added to get the estimate
                Address.DeleteFromDB(ThisCustomer.PrimaryShippingAddressID, ThisCustomer.CustomerID);
            }
        }

        private void ToggleShowHideEstimate()
        {
            if (AppLogic.AppConfigBool("ShowShippingAndTaxEstimate"))
            {
                bool estimateShown = ThisCustomer.ThisCustomerSession.SessionBool("ShowEstimateSelected");

                //btnRequestEstimates.Visible = !estimateShown;
                // btnRemoveEstimator.Visible = estimateShown;

                ctrlCartSummary.ShowShipping = !estimateShown;
                ctrlCartSummary.ShowTax = !estimateShown;
            }
            else
            {
                //  btnRequestEstimates.Visible = false;
                // btnRemoveEstimator.Visible = false;
            }
        }
        #endregion

        protected void btnback_Click(object sender, EventArgs e)
        {
            ErrorMessage err;
            Address BillingAddress = new Address();
            BillingAddress.LoadByCustomer(ThisCustomer.CustomerID, ThisCustomer.PrimaryBillingAddressID, AddressTypes.Billing);
            String PM = AppLogic.CleanPaymentMethod(AppLogic.ro_PMCreditCard);
            if (PM.StartsWith(AppLogic.ro_PMCreditCard))
            {
                String CardName = ctrlCreditCardPanel.CreditCardName.Trim();
                String CardNumber = ctrlCreditCardPanel.CreditCardNumber.Trim().Replace(" ", "");
                String CardExtraCode = ctrlCreditCardPanel.CreditCardVerCd.Trim().Replace(" ", "");
                String strCardType = ctrlCreditCardPanel.CreditCardType.Trim();
                String CardExpirationMonth = ctrlCreditCardPanel.CardExpMonth.Trim().Replace(" ", "");
                String CardExpirationYear = ctrlCreditCardPanel.CardExpYr.Trim().Replace(" ", "");

                String CardStartDate = ctrlCreditCardPanel.CardStartMonth.Replace(" ", "").PadLeft(2, '0') + ctrlCreditCardPanel.CardStartYear.Trim().Replace(" ", "");
                String CardIssueNumber = ctrlCreditCardPanel.CreditCardIssueNumber.Trim().Replace(" ", "");

                // Save CIM payment profile id
                if (CimWalletSelector.SelectedPaymentProfileId > 0)
                {
                    NetTotal = 1.0m;
                    var paymentProfile = GatewayAuthorizeNet.DataUtility.GetPaymentProfileWrapper(ThisCustomer.CustomerID, ThisCustomer.EMail, CimWalletSelector.SelectedPaymentProfileId);
                    CardNumber = paymentProfile.CreditCardNumberMasked;
                    CardExpirationYear = paymentProfile.ExpirationYear;
                    CardExpirationMonth = paymentProfile.ExpirationMonth;
                    strCardType = paymentProfile.CardType;
                    CardExtraCode = "NA";
                }

                //to do
                //if (CardNumber.StartsWith("*") && CimWalletSelector.SelectedPaymentProfileId <= 0)
                //{
                //    // Still obscured in the form so use the original
                //    //CardNumber = BillingAddress.CardNumber;
                //}

                //if (CardExtraCode.StartsWith("*"))
                //{
                //    // Still obscured in the form so use the original
                //    CardExtraCode = AppLogic.GetCardExtraCodeFromSession(ThisCustomer);
                //}

                if (AppLogic.AppConfigBool("ValidateCreditCardNumbers"))
                {
                    BillingAddress.PaymentMethodLastUsed = AppLogic.ro_PMCreditCard;
                    BillingAddress.CardName = CardName;
                    BillingAddress.CardExpirationMonth = CardExpirationMonth;
                    BillingAddress.CardExpirationYear = CardExpirationYear;
                    BillingAddress.CardStartDate = CommonLogic.IIF((CardStartDate == "00" || !CommonLogic.IsNumber(CardStartDate)), String.Empty, CardStartDate);
                    BillingAddress.CardIssueNumber = CardIssueNumber;

                    CardType Type = CardType.Parse(strCardType);
                    CreditCardValidator validator = new CreditCardValidator(CardNumber, Type);
                    bool isValid = validator.Validate();

                    BillingAddress.CardType = strCardType;
                    if (!isValid)
                    {
                        CardNumber = string.Empty;
                        // clear the card extra code
                        AppLogic.StoreCardExtraCodeInSession(ThisCustomer, string.Empty);
                    }
                    BillingAddress.CardNumber = CardNumber;
                    BillingAddress.UpdateDB();

                    if (!isValid)
                    {
                        if (NetTotal == System.Decimal.Zero && AppLogic.AppConfigBool("SkipPaymentEntryOnZeroDollarCheckout"))
                        {
                            //NOTE: Suppress error and allow zero amount to checkout
                        }
                        else
                        {
                            err = new ErrorMessage(Server.HtmlEncode(AppLogic.GetString("checkoutcard_process.aspx.3", 1, Localization.GetDefaultLocale())));
                            Response.Redirect("checkoutpayment.aspx?errormsg=" + err.MessageId);
                        }
                    }

                }

                // store in appropriate session, encrypted, so it can be used when the order is actually "entered"
                AppLogic.StoreCardExtraCodeInSession(ThisCustomer, CardExtraCode);


                if (NetTotal == System.Decimal.Zero && AppLogic.AppConfigBool("SkipPaymentEntryOnZeroDollarCheckout"))
                {
                    // remember their info:
                    BillingAddress.PaymentMethodLastUsed = AppLogic.ro_PMCreditCard;
                    BillingAddress.ClearCCInfo();
                    BillingAddress.UpdateDB();
                }
                else
                {
                    // remember their info:                    
                    BillingAddress.PaymentMethodLastUsed = AppLogic.ro_PMCreditCard;
                    BillingAddress.CardName = CardName;
                    BillingAddress.CardType = strCardType;
                    BillingAddress.CardNumber = CardNumber;
                    BillingAddress.CardExpirationMonth = CardExpirationMonth;
                    BillingAddress.CardExpirationYear = CardExpirationYear;
                    BillingAddress.CardStartDate = CommonLogic.IIF((CardStartDate == "00" || !CommonLogic.IsNumber(CardStartDate)), String.Empty, CardStartDate);
                    BillingAddress.CardIssueNumber = CardIssueNumber;
                    BillingAddress.UpdateDB();

                    //need in future currently it is disabled to go back to shipping methods
                    //if (CardNumber.Length == 0)
                    //{
                    //    err = new ErrorMessage(Server.HtmlEncode(AppLogic.GetString("checkoutcard_process.aspx.1", 1, Localization.GetDefaultLocale())));
                    //    Response.Redirect("checkoutpayment.aspx?errormsg=" + err.MessageId);
                    //}
                    //if ((!AppLogic.AppConfigBool("CardExtraCodeIsOptional") && CardExtraCode.Length == 0))
                    //{
                    //    err = new ErrorMessage(Server.HtmlEncode(AppLogic.GetString("checkoutcard_process.aspx.5", 1, Localization.GetDefaultLocale())));
                    //    Response.Redirect("checkoutpayment.aspx?errormsg=" + err.MessageId);
                    //}

                    // Save card to CIM if selected
                    if (ctrlCreditCardPanel.CimSaveCard)
                    {
                        string errorMessage, errorCode;
                        Int64 saveCardProfileId = 0;

                        saveCardProfileId = GatewayAuthorizeNet.ProcessTools.SaveProfileAndPaymentProfile(
                            ThisCustomer.CustomerID, ThisCustomer.EMail, AspDotNetStorefrontCore.AppLogic.AppConfig("StoreName"),
                            saveCardProfileId, BillingAddress.AddressID, CardNumber, CardExtraCode, CardExpirationMonth, CardExpirationYear,
                            out errorMessage, out errorCode);

                        if (saveCardProfileId <= 0 && errorCode != "E00039")
                        {
                            err = new ErrorMessage(Server.HtmlEncode(errorMessage));
                            Response.Redirect("checkoutpayment.aspx?errormsg=" + err.MessageId);
                        }
                    }
                }
            }
            if (Session["hdnreferalurl"] != "" & Session["hdnreferalurl"] != null)
            {
                Response.Redirect(Session["hdnreferalurl"].ToString());
            }
        }

        private void _txtCCNumber_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs evtargs)
        {

            evtargs.Handled = (!char.IsDigit(evtargs.KeyChar)) && (!char.IsControl(evtargs.KeyChar));

        }



    }
}
