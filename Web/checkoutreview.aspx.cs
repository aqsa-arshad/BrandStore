// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways;
using Vortx.OnePageCheckout.Models;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Globalization;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for checkoutreview.
    /// </summary>
    public partial class checkoutreview : SkinBase
    {

        ShoppingCart cart = null;
        String PaymentMethod = String.Empty;
        String PM = String.Empty;
        decimal CartTotal = Decimal.Zero;
        decimal NetTotal = Decimal.Zero;

        protected void Page_Load(object sender, System.EventArgs e)
        {
            if (cart.HasCoupon())
            {
                string CouponName = cart.Coupon.CouponCode;
                cart.ClearCoupon();
                cart = new ShoppingCart(SkinID, ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);
                cart.SetCoupon(CouponName, true);
            }

            if (!this.IsPostBack)
            {
                if (Request.UrlReferrer != null)
                {
                    if (Request.UrlReferrer.ToString().ToLower().Contains("checkoutpayment"))
                    {

                        Session["hdnreferalurl"] = "checkoutpayment.aspx";
                    }
                    else if (Request.UrlReferrer.ToString().ToLower().Contains("checkoutshipping"))
                    {
                        Session["hdnreferalurl"] = "checkoutshipping.aspx";
                    }
                    else if (Request.UrlReferrer.ToString().ToLower().Contains("shoppingcart"))
                    {
                        Session["hdnreferalurl"] = "shoppingcart.aspx";
                    }
                }
            }
            // check the invoice fee available
            lblInvoiceValue.Text = AppLogic.AppConfig("Invoice.fee");
            Address BillingAddress = new Address();
            BillingAddress.LoadFromDB(ThisCustomer.PrimaryBillingAddressID);
            if (BillingAddress.PaymentMethodLastUsed.Equals(AppLogic.ro_PMCreditCard))
            {
            lblInvoiceFeehdn.Text ="false";
            }
            else
            {
             lblInvoiceFeehdn.Text ="true";
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

            PaymentMethod = CommonLogic.QueryStringCanBeDangerousContent("PaymentMethod").Trim();
            PM = AppLogic.CleanPaymentMethod(PaymentMethod);

            if (!ThisCustomer.IsRegistered)
            {
                bool boolAllowAnon = (AppLogic.AppConfigBool("PasswordIsOptionalDuringCheckout") && !cart.HasRecurringComponents());

                if (!boolAllowAnon && (PM == AppLogic.ro_PMPayPalExpress || PM == AppLogic.ro_PMPayPalExpressMark))
                {
                    boolAllowAnon = AppLogic.AppConfigBool("PayPal.Express.AllowAnonCheckout");
                }

                if (!boolAllowAnon)
                {
                    Response.Redirect("signin.aspx?checkout=true");//createaccount
                }
            }
            if (ThisCustomer.PrimaryBillingAddressID == 0 || (ThisCustomer.PrimaryShippingAddressID == 0 && !AppLogic.AppConfigBool("SkipShippingOnCheckout") && !cart.IsAllDownloadComponents() && !cart.IsAllSystemComponents()))
            {
                err = new ErrorMessage(Server.HtmlEncode(AppLogic.GetString("checkoutpayment.aspx.2", ThisCustomer.SkinID, ThisCustomer.LocaleSetting)));
                Response.Redirect("shoppingcart.aspx?resetlinkback=1&errormsg=" + err.MessageId);
            }

            SectionTitle = AppLogic.GetString("checkoutreview.aspx.1", SkinID, ThisCustomer.LocaleSetting);
            //cart = new ShoppingCart(SkinID, ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);


            // re-validate all shipping info, as ANYTHING could have changed since last page:
            if (!cart.ShippingIsAllValid())
            {
                err = new ErrorMessage(Server.HtmlEncode(AppLogic.GetString("shoppingcart.cs.95", ThisCustomer.SkinID, ThisCustomer.LocaleSetting)));
                HttpContext.Current.Response.Redirect("shoppingcart.aspx?resetlinkback=1&errormsg=" + err.MessageId);
            }

            CartTotal = cart.Total(true);
            NetTotal = CartTotal - CommonLogic.IIF(cart.Coupon.CouponType == CouponTypeEnum.GiftCard, CommonLogic.IIF(CartTotal < cart.Coupon.DiscountAmount, CartTotal, cart.Coupon.DiscountAmount), 0);
            if (NetTotal > System.Decimal.Zero || !AppLogic.AppConfigBool("SkipPaymentEntryOnZeroDollarCheckout"))
            {
                AppLogic.ValidatePM(PM); // this WILL throw a hard security exception on any problem!
            }

            String PayPalToken = ThisCustomer.ThisCustomerSession["PayPalExpressToken"];
            String PayerID = ThisCustomer.ThisCustomerSession["PayPalExpressPayerID"];

            if ((PM == AppLogic.ro_PMPayPalExpress || PM == AppLogic.ro_PMPayPalExpressMark)
                && (PayPalToken.Length == 0 || PayerID.Length == 0))
            {
                err = new ErrorMessage(Server.HtmlEncode(AppLogic.GetString("checkoutpayment.aspx.20", ThisCustomer.SkinID, ThisCustomer.LocaleSetting)));
                Response.Redirect("checkoutpayment.aspx?errormsg=" + err.MessageId);
            }

            if (!IsPostBack)
            {
                InitializePageContent();

                if ((PM == AppLogic.ro_PMPayPalExpress || PM == AppLogic.ro_PMPayPalExpressMark)
                    && CommonLogic.QueryStringCanBeDangerousContent("useraction").Equals("COMMIT", StringComparison.InvariantCultureIgnoreCase))
                {
                    ContinueCheckout();
                }
            }
            if (cart.CartItems.Count == 0)
            {
                Response.Redirect("shoppingcart.aspx");
            }

            AppLogic.eventHandler("CheckoutReview").CallEvent("&CheckoutReview=true");

            //add edit links here
            HyperLink1.NavigateUrl = String.Format("~/JWMyAddresses.aspx?Checkout=True&AddressType=1&returnURL=checkoutreview.aspx%3fpaymentmethod%3d{0}", Request.QueryString["paymentmethod"]);
            HyperLink3.NavigateUrl = String.Format("~/JWMyAddresses.aspx?Checkout=True&AddressType=2&returnURL=checkoutreview.aspx%3fpaymentmethod%3d{0}", Request.QueryString["paymentmethod"]);
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
                cart.SetCoupon(CouponCode, true);
                //cart = new ShoppingCart(SkinID, ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);
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
            //btnContinueCheckout1.Click += new EventHandler(btnContinueCheckout1_Click);
            btnContinueCheckout2.Click += new EventHandler(btnContinueCheckout2_Click);
        }

        #endregion

        protected void btnContinueCheckout1_Click(object sender, EventArgs e)
        {
            btnContinueCheckout2.Enabled = false;
            ContinueCheckout();
        }

        protected void btnContinueCheckout2_Click(object sender, EventArgs e)
        {
           // btnContinueCheckout1.Enabled = false;
            ContinueCheckout();
        }

        private bool validateuserfunds()
        {
            if (AuthenticationSSO.IsDealerUser(ThisCustomer.CustomerLevelID) || AuthenticationSSO.IsInternalUser(ThisCustomer.CustomerLevelID))
            {
                return AuthenticationSSO.ValidateCustomerFund(ThisCustomer.CustomerID, true);
            }
            else
                return true;
        }
        private void ContinueCheckout()
        {
            string errormessage = AppLogic.GetString("FundLimitExceeds", SkinID, ThisCustomer.LocaleSetting.ToString());
            string script = "alert('" + errormessage + "')";
            if (validateuserfunds())
            {
                String PayPalToken = CommonLogic.QueryStringCanBeDangerousContent("token").Trim();
                String PayerID = CommonLogic.QueryStringCanBeDangerousContent("payerid").Trim();
                ProcessCheckout();                
            }

            else
            {
                System.Web.UI.ClientScriptManager cs = this.ClientScript;
                cs.RegisterClientScriptBlock(this.GetType(), "alertMessage", script, true);              
               
            }
        }

        private void InitializePageContent()
        {
            JSPopupRoutines.Text = AppLogic.GetJSPopupRoutines();
            
            String XmlPackageName = AppLogic.AppConfig("XmlPackage.CheckoutReviewPageHeader");
            if (XmlPackageName.Length != 0)
            {
                XmlPackage_CheckoutReviewPageHeader.Text = "" + AppLogic.RunXmlPackage(XmlPackageName, base.GetParser, ThisCustomer, SkinID, String.Empty, String.Empty, true, true);
            }

            Address BillingAddress = new Address();
            BillingAddress.LoadFromDB(ThisCustomer.PrimaryBillingAddressID);
            Address ShippingAddress = new Address();
            ShippingAddress.LoadFromDB(ThisCustomer.PrimaryShippingAddressID);

            litBillingAddress.Text = BillingAddress.DisplayHTML(true);

            litPaymentMethod.Text = GetPaymentMethod(BillingAddress);


            if (cart.HasMultipleShippingAddresses())
            {
                litShippingAddress.Text = "" + AppLogic.GetString("checkoutreview.aspx.25", SkinID, ThisCustomer.LocaleSetting);
            }
            else if (cart.HasGiftRegistryComponents() && cart.HasGiftRegistryAddresses())
            {
                litShippingAddress.Text = "" + AppLogic.GetString("checkoutreview.aspx.26", SkinID, ThisCustomer.LocaleSetting);
            }
            else if (AppLogic.AppConfigBool("SkipShippingOnCheckout") || cart.IsAllDownloadComponents() || cart.IsAllSystemComponents())
            {
                ordercs57.Visible = false;
                spn3.Visible = false;
            }
            else
            {
                litShippingAddress.Text = ShippingAddress.DisplayHTML(true);
            }

            CartSummary.Text = cart.DisplaySummary(true, true, true, true, false);

            String XmlPackageName2 = AppLogic.AppConfig("XmlPackage.CheckoutReviewPageFooter");
            if (XmlPackageName2.Length != 0)
            {
                XmlPackage_CheckoutReviewPageFooter.Text = "" + AppLogic.RunXmlPackage(XmlPackageName2, base.GetParser, ThisCustomer, SkinID, String.Empty, String.Empty, true, true);
            }

          //  AppLogic.GetButtonDisable(btnContinueCheckout1);
            AppLogic.GetButtonDisable(btnContinueCheckout2);
          //  btnContinueCheckout1.Attributes["onclick"] = string.Format("{0}{1}", btnContinueCheckout1.Attributes["onclick"], "document.getElementById(\"" + btnContinueCheckout2.ClientID + "\").disabled = true;");
           // btnContinueCheckout2.Attributes["onclick"] = string.Format("{0}{1}", btnContinueCheckout2.Attributes["onclick"], "document.getElementById(\"" + btnContinueCheckout1.ClientID + "\").disabled = true;");

            GatewayCheckoutByAmazon.CheckoutByAmazon checkoutByAmazon = new GatewayCheckoutByAmazon.CheckoutByAmazon();
            if (checkoutByAmazon.IsEnabled && checkoutByAmazon.IsCheckingOut)
            {
                pnlAmazonAddressWidget.Visible =
                    pnlAmazonWalletWidget.Visible = true;

                litBillingAddress.Visible =
                    litShippingAddress.Visible =
                    litPaymentMethod.Visible =
                    checkoutreviewaspx8.Visible =
                    checkoutreviewaspx9.Visible =
                    ordercs57.Visible = false;
            }

			if (AppLogic.AppConfigBool("PayPal.Express.UseIntegratedCheckout"))
				ltPayPalIntegratedCheckout.Text = AspDotNetStorefrontGateways.Processors.PayPalController.GetExpressCheckoutIntegratedScript(false);

        }

        private string GetPaymentMethod(Address BillingAddress)
        {
            if (cart.CartItems.Count == 0)
            {
                Response.Redirect("shoppingcart.aspx");
            }
            StringBuilder sPmtMethod = new StringBuilder(1024);
            if (PM == AppLogic.ro_PMCreditCard)
                //if (cart.Total(true) == System.Decimal.Zero && AppLogic.AppConfigBool("SkipPaymentEntryOnZeroDollarCheckout"))
                if (NetTotal == System.Decimal.Zero && AppLogic.AppConfigBool("SkipPaymentEntryOnZeroDollarCheckout"))
                {
                    sPmtMethod.Append("<br/>");
                    sPmtMethod.Append(AppLogic.GetString("checkoutpayment.aspx.28", SkinID, ThisCustomer.LocaleSetting));
                }
                else
                {
                    // This code may be needed for next phase so i am not removing it.

                    //sPmtMethod.Append(AppLogic.GetString("account.aspx.45", SkinID, ThisCustomer.LocaleSetting));
                    //sPmtMethod.Append("");

                    if (AppLogic.ActivePaymentGatewayCleaned() != Gateway.ro_GWNETAXEPT)
                    {
                        if (BillingAddress.PaymentMethodLastUsed.Equals(AppLogic.ro_PMCreditCard))
                        {
                            sPmtMethod.Append("<p>");
                            sPmtMethod.Append("<span class='block-text'>");
                            sPmtMethod.Append(BillingAddress.CardType);
                            sPmtMethod.Append(AppLogic.SafeDisplayCardNumber(BillingAddress.CardNumber, "Address", BillingAddress.AddressID));
                            sPmtMethod.Append("<span>");
                            sPmtMethod.Append("<span class='block-text'> Expires: ");
                            sPmtMethod.Append(BillingAddress.CardExpirationMonth.PadLeft(2, '0') + "/" + BillingAddress.CardExpirationYear);
                            sPmtMethod.Append("<span>");
                            sPmtMethod.Append("<span class='block-text'>");
                            sPmtMethod.Append(BillingAddress.Country);
                            sPmtMethod.Append("<span>");
                            sPmtMethod.Append("</p>");
                        }
                        else
                        {
                            sPmtMethod.Append("<p>");
                            sPmtMethod.Append("<span class='block-text'> Purchase order#: ");
                            sPmtMethod.Append(BillingAddress.PONumber);
                            sPmtMethod.Append("</p>");
                        }
                        

                        // This code may be needed for next phase so i am not removing it.

                        //sPmtMethod.Append("<div class='page-row'>");
                        //sPmtMethod.Append("<div class='one-half'>");
                        //sPmtMethod.Append(AppLogic.GetString("checkoutreview.aspx.10", SkinID, ThisCustomer.LocaleSetting));
                        //sPmtMethod.Append("</div>");
                        //sPmtMethod.Append("<div class='one-half'>");
                        //sPmtMethod.Append(BillingAddress.CardName);
                        //sPmtMethod.Append("</div>");
                        //sPmtMethod.Append("</div>");

                        //sPmtMethod.Append("<div class='page-row'>");
                        //sPmtMethod.Append("<div class='one-half'>");
                        //sPmtMethod.Append(AppLogic.GetString("checkoutreview.aspx.11", SkinID, ThisCustomer.LocaleSetting));
                        //sPmtMethod.Append("</div>");
                        //sPmtMethod.Append("<div class='one-half'>");
                        //sPmtMethod.Append(BillingAddress.CardType);
                        //sPmtMethod.Append("</div>");
                        //sPmtMethod.Append("</div>");

                        //sPmtMethod.Append("<div class='page-row'>");
                        //sPmtMethod.Append("<div class='one-half'>");
                        //sPmtMethod.Append(AppLogic.GetString("checkoutreview.aspx.12", SkinID, ThisCustomer.LocaleSetting));
                        //sPmtMethod.Append("</div>");
                        //sPmtMethod.Append("<div class='one-half'>");
                        //sPmtMethod.Append(AppLogic.SafeDisplayCardNumber(BillingAddress.CardNumber, "Address", BillingAddress.AddressID));
                        //sPmtMethod.Append("</div>");
                        //sPmtMethod.Append("</div>");

                        //sPmtMethod.Append("<div class='page-row'>");
                        //sPmtMethod.Append("<div class='one-half'>");
                        //sPmtMethod.Append(AppLogic.GetString("checkoutreview.aspx.13", SkinID, ThisCustomer.LocaleSetting));
                        //sPmtMethod.Append("</div>");
                        //sPmtMethod.Append("<div class='one-half'>");
                        //sPmtMethod.Append(BillingAddress.CardExpirationMonth.PadLeft(2, '0') + "/" + BillingAddress.CardExpirationYear);
                        //sPmtMethod.Append("</div>");
                        //sPmtMethod.Append("</div>");
                    }
                }
            else if (PM == AppLogic.ro_PMPurchaseOrder)
            {
                sPmtMethod.Append("<p>");
                sPmtMethod.Append("<span class='block-text'> Purchase Order#: ");
                sPmtMethod.Append(BillingAddress.PONumber);
                sPmtMethod.Append("</p>");
            }
            else if (PM == AppLogic.ro_PMCODMoneyOrder)
            {
                sPmtMethod.Append(AppLogic.GetString("checkoutreview.aspx.14", SkinID, ThisCustomer.LocaleSetting) + BillingAddress.PONumber);
            }
            else if (PM == AppLogic.ro_PMCODCompanyCheck)
            {
                sPmtMethod.Append(AppLogic.GetString("checkoutreview.aspx.14", SkinID, ThisCustomer.LocaleSetting) + BillingAddress.PONumber);
            }
            else if (PM == AppLogic.ro_PMCODNet30)
            {
                sPmtMethod.Append(AppLogic.GetString("checkoutreview.aspx.14", SkinID, ThisCustomer.LocaleSetting) + BillingAddress.PONumber);
            }
            else if (PM == AppLogic.ro_PMPayPal)
            {
            }
            else if (PM == AppLogic.ro_PMPayPalExpress || PM == AppLogic.ro_PMPayPalExpressMark)
            {
                sPmtMethod.Append("<font color=\"red\">PayPal</font>");
            }
            else if (PM == AppLogic.ro_PMRequestQuote)
            {
                sPmtMethod.Append(AppLogic.GetString("checkoutreview.aspx.20", SkinID, ThisCustomer.LocaleSetting));
            }
            else if (PM == AppLogic.ro_PMCheckByMail)
            {
                sPmtMethod.Append(AppLogic.GetString("checkoutreview.aspx.21", SkinID, ThisCustomer.LocaleSetting));
            }
            else if (PM == AppLogic.ro_PMCOD)
            {
                sPmtMethod.Append(AppLogic.GetString("checkoutreview.aspx.22", SkinID, ThisCustomer.LocaleSetting));
            }
            else if (PM == AppLogic.ro_PMECheck)
            {
                int BANMasklen = CommonLogic.IIF(BillingAddress.ECheckBankAccountNumber.Length - 4 < 0, Convert.ToInt32(decimal.Ceiling(BillingAddress.ECheckBankAccountNumber.Length / 2)), BillingAddress.ECheckBankAccountNumber.Length - 4);
                int ABAMasklen = CommonLogic.IIF(BillingAddress.ECheckBankABACode.Length - 4 < 0, Convert.ToInt32(decimal.Ceiling(BillingAddress.ECheckBankABACode.Length / 2)), BillingAddress.ECheckBankABACode.Length - 4);


                sPmtMethod.Append("<div class='page-row'>");
                sPmtMethod.Append("<div class='one-half'>");
                sPmtMethod.Append(AppLogic.GetString("checkoutreview.aspx.15", SkinID, ThisCustomer.LocaleSetting));
                sPmtMethod.Append("</div>");
                sPmtMethod.Append("<div class='one-half'>");
                sPmtMethod.Append(BillingAddress.ECheckBankAccountName);
                sPmtMethod.Append("</div>");
                sPmtMethod.Append("</div>");

                sPmtMethod.Append("<div class='page-row'>");
                sPmtMethod.Append("<div class='one-half'>");
                sPmtMethod.Append(AppLogic.GetString("checkoutreview.aspx.16", SkinID, ThisCustomer.LocaleSetting));
                sPmtMethod.Append("</div>");
                sPmtMethod.Append("<div class='one-half'>");
                sPmtMethod.Append(BillingAddress.ECheckBankAccountType);
                sPmtMethod.Append("</div>");
                sPmtMethod.Append("</div>");

                sPmtMethod.Append("<div class='page-row'>");
                sPmtMethod.Append("<div class='one-half'>");
                sPmtMethod.Append(AppLogic.GetString("checkoutreview.aspx.17", SkinID, ThisCustomer.LocaleSetting));
                sPmtMethod.Append("</div>");
                sPmtMethod.Append("<div class='one-half'>");
                sPmtMethod.Append(BillingAddress.ECheckBankName);
                sPmtMethod.Append("</div>");
                sPmtMethod.Append("</div>");

                sPmtMethod.Append("<div class='page-row'>");
                sPmtMethod.Append("<div class='one-half'>");
                sPmtMethod.Append(AppLogic.GetString("checkoutreview.aspx.18", SkinID, ThisCustomer.LocaleSetting));
                sPmtMethod.Append("</div>");
                sPmtMethod.Append("<div class='one-half'>");
                sPmtMethod.Append("********************".Substring(1, BANMasklen) + BillingAddress.ECheckBankAccountNumber.Substring(BANMasklen, BillingAddress.ECheckBankAccountNumber.Length - BANMasklen));
                sPmtMethod.Append("</div>");
                sPmtMethod.Append("</div>");


                sPmtMethod.Append("<div class='page-row'>");
                sPmtMethod.Append("<div class='one-half'>");
                sPmtMethod.Append(AppLogic.GetString("checkoutreview.aspx.19", SkinID, ThisCustomer.LocaleSetting));
                sPmtMethod.Append("</div>");
                sPmtMethod.Append("<div class='one-half'>");
                sPmtMethod.Append("*********".Substring(1, ABAMasklen) + BillingAddress.ECheckBankABACode.Substring(ABAMasklen, BillingAddress.ECheckBankABACode.Length - ABAMasklen));
                sPmtMethod.Append("</div>");
                sPmtMethod.Append("</div>");
            }
            else if (PM == AppLogic.ro_PMMicropay)
            {
                sPmtMethod.Append(AppLogic.GetString("checkoutreview.aspx.23", SkinID, ThisCustomer.LocaleSetting));

            }
            else if (PM == AppLogic.ro_PMSecureNetVault)
            {
                sPmtMethod.Append("securenet.selectedpaymentmethod".StringResource());
            }
            return sPmtMethod.ToString();
        }

        private void ProcessCheckout()
        {
            //Set GLcodes of each item to ordernotes field of cart
            foreach(CartItem citem in cart.CartItems)
            {
                cart.OrderNotes +="Product ID: " + citem.ProductID + ", GL: " + citem.GLcode + ",";
            }
            cart.OrderNotes = cart.OrderNotes.Remove(cart.OrderNotes.LastIndexOf(","), 1);
            //end set gl code
            Address BillingAddress = new Address();
            BillingAddress.LoadByCustomer(ThisCustomer.CustomerID, ThisCustomer.PrimaryBillingAddressID, AddressTypes.Billing);

            int OrderNumber = 0;
            // ----------------------------------------------------------------
            // Process The Order:
            // ----------------------------------------------------------------
            ErrorMessage err;
            if (PaymentMethod.Length == 0 || PM != AppLogic.CleanPaymentMethod(BillingAddress.PaymentMethodLastUsed))
            {
                err = new ErrorMessage(Server.HtmlEncode(AppLogic.GetString("checkoutpayment.aspx.20", ThisCustomer.SkinID, ThisCustomer.LocaleSetting)));
                Response.Redirect("checkoutpayment.aspx?errormsg=" + err.MessageId);
            }
            if (PM == AppLogic.ro_PMCreditCard)
            {
                if (Cardinal.EnabledForCheckout(cart.Total(true), BillingAddress.CardType))
                {
					OrderNumber = AppLogic.GetNextOrderNumber();
					
					if (Cardinal.PreChargeLookupAndStoreSession(ThisCustomer, OrderNumber, cart.Total(true), 
						BillingAddress.CardNumber, BillingAddress.CardExpirationMonth, BillingAddress.CardExpirationYear))
					{
						Response.Redirect("cardinalform.aspx");// this will eventually come "back" to us in cardinal_process.aspx after going through banking system pages
					}
					else
					{
						// user not enrolled or cardinal gateway returned error, so process card normally, using already created order #:

						string ECIFlag = Cardinal.GetECIFlag(BillingAddress.CardType);						

						String status = Gateway.MakeOrder(String.Empty, AppLogic.TransactionMode(), cart, OrderNumber, String.Empty, ECIFlag, String.Empty, String.Empty);
						if (status != AppLogic.ro_OK)
						{
							err = new ErrorMessage(Server.HtmlEncode(status));
							Response.Redirect("checkoutpayment.aspx?TryToShowPM=" + PM + "&errormsg=" + err.MessageId);
						}
						DB.ExecuteSQL("update orders set CardinalLookupResult=" + DB.SQuote(ThisCustomer.ThisCustomerSession["Cardinal.LookupResult"]) + " where OrderNumber=" + OrderNumber.ToString());
					}
                }
                else
                {
                    decimal CartTotal = cart.Total(true);
                    decimal NetTotal = CartTotal - CommonLogic.IIF(cart.Coupon.CouponType == CouponTypeEnum.GiftCard, CommonLogic.IIF(CartTotal < cart.Coupon.DiscountAmount, CartTotal, cart.Coupon.DiscountAmount), 0);
                    // this is  specific for nexaxept gateway
                    if (AppLogic.ActivePaymentGatewayCleaned() == Gateway.ro_GWNETAXEPT &&
                        NetTotal > System.Decimal.Zero)
                    {
                        int ordnum;
                        bool result = int.TryParse(ThisCustomer.ThisCustomerSession["Nextaxept_OrderNumber"], out ordnum);

                        if (result)
                        {
                            OrderNumber = ordnum;
                        }
                    }
                    else
                    {
                        // try create the order record, check for status of TX though:
                        OrderNumber = AppLogic.GetNextOrderNumber();
                    }
                    String status = Gateway.MakeOrder(String.Empty, AppLogic.TransactionMode(), cart, OrderNumber, String.Empty, String.Empty, String.Empty, String.Empty);
                    if (status == AppLogic.ro_3DSecure)
                    { // If credit card is enrolled in a 3D Secure service (Verified by Visa, etc.)
                        Response.Redirect("secureform.aspx");
                    }
                    if (status != AppLogic.ro_OK)
                    {
                        err = new ErrorMessage(Server.HtmlEncode(status));
                        Response.Redirect("checkoutpayment.aspx?TryToShowPM=" + PM + "&errormsg=" + err.MessageId);
                    }
                }
            }
			else if (PM.ToLower() == GatewayCheckoutByAmazon.CheckoutByAmazon.CBA_Gateway_Identifier.ToLower())
			{
				OrderNumber = AppLogic.GetNextOrderNumber();
				String status = Gateway.MakeOrder(String.Empty, AppLogic.TransactionMode(), cart, OrderNumber, String.Empty, String.Empty, String.Empty, String.Empty);
				if (status != AppLogic.ro_OK)
				{
					err = new ErrorMessage(Server.HtmlEncode(status));
					Response.Redirect("shoppingcart.aspx?TryToShowPM=" + PM + "&errormsg=" + err.MessageId);
				}
			}
            else if (PM == AppLogic.ro_PMPurchaseOrder)
            {
                // try create the order record, check for status of TX though:
                OrderNumber = AppLogic.GetNextOrderNumber();
                String status = Gateway.MakeOrder(String.Empty, AppLogic.TransactionMode(), cart, OrderNumber, String.Empty, String.Empty, String.Empty, String.Empty);
                if (status != AppLogic.ro_OK)
                {
                    err = new ErrorMessage(Server.HtmlEncode(status));
                    Response.Redirect("checkoutpayment.aspx?TryToShowPM=" + PM + "&errormsg=" + err.MessageId);
                }
            }
            else if (PM == AppLogic.ro_PMCODMoneyOrder)
            {
                // try create the order record, check for status of TX though:
                OrderNumber = AppLogic.GetNextOrderNumber();
                String status = Gateway.MakeOrder(String.Empty, AppLogic.TransactionMode(), cart, OrderNumber, String.Empty, String.Empty, String.Empty, String.Empty);
                if (status != AppLogic.ro_OK)
                {
                    err = new ErrorMessage(Server.HtmlEncode(status));
                    Response.Redirect("checkoutpayment.aspx?TryToShowPM=" + PM + "&errormsg=" + err.MessageId);
                }
            }
            else if (PM == AppLogic.ro_PMCODCompanyCheck)
            {
                // try create the order record, check for status of TX though:
                OrderNumber = AppLogic.GetNextOrderNumber();
                String status = Gateway.MakeOrder(String.Empty, AppLogic.TransactionMode(), cart, OrderNumber, String.Empty, String.Empty, String.Empty, String.Empty);
                if (status != AppLogic.ro_OK)
                {
                    err = new ErrorMessage(Server.HtmlEncode(status));
                    Response.Redirect("checkoutpayment.aspx?TryToShowPM=" + PM + "&errormsg=" + err.MessageId);
                }
            }
            else if (PM == AppLogic.ro_PMCODNet30)
            {
                // try create the order record, check for status of TX though:
                OrderNumber = AppLogic.GetNextOrderNumber();
                String status = Gateway.MakeOrder(String.Empty, AppLogic.TransactionMode(), cart, OrderNumber, String.Empty, String.Empty, String.Empty, String.Empty);
                if (status != AppLogic.ro_OK)
                {
                    err = new ErrorMessage(Server.HtmlEncode(status));
                    Response.Redirect("checkoutpayment.aspx?TryToShowPM=" + PM + "&errormsg=" + err.MessageId);
                }
            }
            else if (PM == AppLogic.ro_PMPayPal)
            {
            }
            else if (PM == AppLogic.ro_PMPayPalExpress || PM == AppLogic.ro_PMPayPalExpressMark)
            {
                String PayPalToken = Security.UnmungeString(ThisCustomer.ThisCustomerSession["PayPalExpressToken"]);
                String PayerID = Security.UnmungeString(ThisCustomer.ThisCustomerSession["PayPalExpressPayerID"]);
                if (PayPalToken.Length > 0)
                {
                    OrderNumber = AppLogic.GetNextOrderNumber();

                    Address UseBillingAddress = new Address();
                    UseBillingAddress.LoadByCustomer(ThisCustomer.CustomerID, ThisCustomer.PrimaryBillingAddressID, AddressTypes.Billing);
                    UseBillingAddress.PaymentMethodLastUsed = PM;
                    UseBillingAddress.CardNumber = String.Empty;
                    UseBillingAddress.CardType = String.Empty;
                    UseBillingAddress.CardExpirationMonth = String.Empty;
                    UseBillingAddress.CardExpirationYear = String.Empty;
                    UseBillingAddress.CardName = String.Empty;
                    UseBillingAddress.CardStartDate = String.Empty;
                    UseBillingAddress.CardIssueNumber = String.Empty;
                    UseBillingAddress.UpdateDB();

                    IDictionary<string, string> transactionContext = new Dictionary<string, string>();
                    transactionContext.Add("TENDER", "P");

                    string gatewayToUse = string.Empty;

                    if (AspDotNetStorefrontGateways.Processors.PayPalController.GetAppropriateExpressType() == AspDotNetStorefrontGateways.Processors.ExpressAPIType.PayFlowPro)
                        gatewayToUse = Gateway.ro_GWPAYFLOWPRO;

                    String status = Gateway.MakeOrder(gatewayToUse, AppLogic.TransactionMode(), cart, OrderNumber, PayPalToken, PayerID, PayPalToken, String.Empty, transactionContext);
                    if (status != AppLogic.ro_OK)
                    {
                        err = new ErrorMessage(Server.HtmlEncode(status));
                        Response.Redirect("checkoutpayment.aspx?errormsg=" + err.MessageId);
                    }
                    else
                    {
                        ThisCustomer.ThisCustomerSession["PayPalExpressToken"] = "";
                        ThisCustomer.ThisCustomerSession["PayPalExpressPayerID"] = "";
                    }
                }
                else
                {
                    err = new ErrorMessage("The PaypalExpress checkout token has expired, please re-login to your PayPal account or checkout using a different method of payment.");
                    Response.Redirect("shoppingcart.aspx?errormsg=" + err.MessageId);
                }
            }
            else if (PM == AppLogic.ro_PMRequestQuote)
            {
                // try create the order record, check for status of TX though:
                OrderNumber = AppLogic.GetNextOrderNumber();
                String status = Gateway.MakeOrder(String.Empty, AppLogic.TransactionMode(), cart, OrderNumber, String.Empty, String.Empty, String.Empty, String.Empty);
                if (status != AppLogic.ro_OK)
                {
                    err = new ErrorMessage(Server.HtmlEncode(status));
                    Response.Redirect("checkoutpayment.aspx?TryToShowPM=" + PM + "&errormsg=" + err.MessageId);
                }
            }
            else if (PM == AppLogic.ro_PMCheckByMail)
            {
                // try create the order record, check for status of TX though:
                OrderNumber = AppLogic.GetNextOrderNumber();
                String status = Gateway.MakeOrder(String.Empty, AppLogic.TransactionMode(), cart, OrderNumber, String.Empty, String.Empty, String.Empty, String.Empty);
                if (status != AppLogic.ro_OK)
                {
                    err = new ErrorMessage(Server.HtmlEncode(status));
                    Response.Redirect("checkoutpayment.aspx?TryToShowPM=" + PM + "&errormsg=" + err.MessageId);
                }
            }
            else if (PM == AppLogic.ro_PMCOD)
            {
                // try create the order record, check for status of TX though:
                OrderNumber = AppLogic.GetNextOrderNumber();
                String status = Gateway.MakeOrder(String.Empty, AppLogic.TransactionMode(), cart, OrderNumber, String.Empty, String.Empty, String.Empty, String.Empty);
                if (status != AppLogic.ro_OK)
                {
                    err = new ErrorMessage(Server.HtmlEncode(status));
                    Response.Redirect("checkoutpayment.aspx?TryToShowPM=" + PM + "&errormsg=" + err.MessageId);
                }
            }
            else if (PM == AppLogic.ro_PMECheck)
            {
                // try create the order record, check for status of TX though:
                OrderNumber = AppLogic.GetNextOrderNumber();
                String status = Gateway.MakeOrder(String.Empty, AppLogic.TransactionMode(), cart, OrderNumber, String.Empty, String.Empty, String.Empty, String.Empty);
                if (status != AppLogic.ro_OK)
                {
                    err = new ErrorMessage(Server.HtmlEncode(status));
                    Response.Redirect("checkoutpayment.aspx?TryToShowPM=" + PM + "&errormsg=" + err.MessageId);
                }
            }
            else if (PM == AppLogic.ro_PMMicropay)
            {
                // try create the order record, check for status of TX though:
                OrderNumber = AppLogic.GetNextOrderNumber();
                String status = Gateway.MakeOrder(String.Empty, AppLogic.TransactionMode(), cart, OrderNumber, String.Empty, String.Empty, String.Empty, String.Empty);
                if (status != AppLogic.ro_OK)
                {
                    err = new ErrorMessage(Server.HtmlEncode(status));
                    Response.Redirect("checkoutpayment.aspx?TryToShowPM=" + PM + "&errormsg=" + err.MessageId);
                }
            }
            else if (PM == AppLogic.ro_PMSecureNetVault)
            {
                OrderNumber = AppLogic.GetNextOrderNumber();
                SecureNetVault vault = new SecureNetVault(ThisCustomer);
                String status = Gateway.MakeOrder(String.Empty, AppLogic.TransactionMode(), cart, OrderNumber, String.Empty, String.Empty, String.Empty, String.Empty);
                //String status = vault.ProcessVaultTransaction();
                if (status != AppLogic.ro_OK)
                {
                    err = new ErrorMessage(Server.HtmlEncode(status));
                    Response.Redirect("checkoutpayment.aspx?TryToShowPM=" + PM + "&errormsg=" + err.MessageId);
                }
            }
            AuthenticationSSO.CommitCustomerFund(ThisCustomer.CustomerID);
            //Once order is processed then change payment method to credit card to make it default selected for new order
            Address BillingAddressTemp = new Address();
            BillingAddressTemp.LoadByCustomer(ThisCustomer.CustomerID, ThisCustomer.PrimaryBillingAddressID, AddressTypes.Billing);
            BillingAddressTemp.PaymentMethodLastUsed = AppLogic.ro_PMCreditCard;
            BillingAddressTemp.ClearCCInfo();
            BillingAddressTemp.UpdateDB();
            //End
            EmailOrderReceipt(OrderNumber);
            Response.Redirect("orderconfirmation.aspx?ordernumber=" + OrderNumber.ToString() + "&paymentmethod=" + Server.UrlEncode(PaymentMethod));
        }
        protected void EmailOrderReceipt(int OrderNumber)
        {
            // email Order Receipt once order is done.
            try
            {
                String lblSOFFundsTotal = String.Empty;
                String lblDirectMailFundsTotal = String.Empty;
                String lblDisplayFundsTotal = String.Empty;
                String lblLiteratureFundsTotal = String.Empty;
                String lblPOPFundsTotal = String.Empty;
                String lblBluBucks = String.Empty;
                decimal bluBucksUsed = 0;

                List<decimal> lstFund = Enumerable.Repeat(0m, 7).ToList();
                using (var conn = DB.dbConn())
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("aspdnsf_GetOrderDetail", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ORDERNUMBER", OrderNumber);

                        IDataReader reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            for (var i = 2; i < 7; i++)
                            {
                                if (Convert.ToDecimal(reader[i.ToString()].ToString()) != 0)
                                {
                                    lstFund[i] =
                                        lstFund[i] + Convert.ToDecimal(reader[i.ToString()].ToString());

                                    if (lstFund[i] != 0 && i == (int)FundType.SOFFunds)
                                    {
                                        lblSOFFundsTotal = "SOF Fund: ";
                                        lblSOFFundsTotal = string.Format(CultureInfo.GetCultureInfo(ThisCustomer.LocaleSetting), AppLogic.AppConfig("CurrencyFormat"), lstFund[i]);
                                    }
                                    else if (lstFund[i] != 0 && i == (int)FundType.DirectMailFunds)
                                    {
                                        lblDirectMailFundsTotal = "Direct Mail Funds: ";
                                        lblDirectMailFundsTotal = string.Format(CultureInfo.GetCultureInfo(ThisCustomer.LocaleSetting), AppLogic.AppConfig("CurrencyFormat"), lstFund[i]);
                                    }
                                    else if (lstFund[i] != 0 && i == (int)FundType.DisplayFunds)
                                    {
                                        lblDisplayFundsTotal = "Display Funds: ";
                                        lblDisplayFundsTotal = string.Format(CultureInfo.GetCultureInfo(ThisCustomer.LocaleSetting), AppLogic.AppConfig("CurrencyFormat"), lstFund[i]);
                                    }
                                    else if (lstFund[i] != 0 && i == (int)FundType.LiteratureFunds)
                                    {
                                        lblLiteratureFundsTotal = "Literature Funds: ";
                                        lblLiteratureFundsTotal = string.Format(CultureInfo.GetCultureInfo(ThisCustomer.LocaleSetting), AppLogic.AppConfig("CurrencyFormat"), lstFund[i]);
                                    }
                                    else if (lstFund[i] != 0 && i == (int)FundType.POPFunds)
                                    {
                                        lblPOPFundsTotal = "POP Funds: ";
                                        lblPOPFundsTotal = string.Format(CultureInfo.GetCultureInfo(ThisCustomer.LocaleSetting), AppLogic.AppConfig("CurrencyFormat"), lstFund[i]);
                                    }
                                }
                            }
                        }
                    }
                }
                //get the Blu Bucks used for a perticular order number
                using (var conn = DB.dbConn())
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("select SUM(BluBucksUsed) from Orders_ShoppingCart where OrderNumber=" + OrderNumber, conn))
                    {
                        bluBucksUsed = Convert.ToDecimal(cmd.ExecuteScalar());
                    }
                }
                lblBluBucks = Localization.CurrencyStringForDisplayWithExchangeRate(bluBucksUsed, ThisCustomer.CurrencySetting);
                string invoiceFee = Localization.CurrencyStringForDisplayWithExchangeRate(Convert.ToDecimal(AppLogic.AppConfig("Invoice.fee")), ThisCustomer.CurrencySetting);
                String SubjectReceipt = String.Format(AppLogic.GetString("common.cs.2", SkinID, ThisCustomer.LocaleSetting), AppLogic.AppConfig("StoreName", ThisCustomer.StoreID, true));
                String PackageName = AppLogic.AppConfig("XmlPackage.OrderReceipt");
                XmlPackage2 p = new XmlPackage2(PackageName, null, SkinID, String.Empty, "ordernumber=" + OrderNumber + "&BBCredit=" + lblBluBucks + "&SOFunds=" + lblSOFFundsTotal + "&DirectMailFunds=" + lblDirectMailFundsTotal + "&DisplayFunds=" + lblDisplayFundsTotal + "&LiteratureFunds=" + lblLiteratureFundsTotal + "&POPFunds=" + lblPOPFundsTotal + "&Invoicefee=" + invoiceFee);
                String receiptBody = p.TransformString();
                AppLogic.SendMail(SubjectReceipt, receiptBody + AppLogic.AppConfig("MailFooter"), true, AppLogic.AppConfig("ReceiptEMailFrom", ThisCustomer.StoreID, true), AppLogic.AppConfig("ReceiptEMailFromName", ThisCustomer.StoreID, true), ThisCustomer.EMail, String.Empty, String.Empty, AppLogic.MailServer());
            }
            catch (Exception ex)
            {
                SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                ex.Message + ((ex.InnerException != null && string.IsNullOrEmpty(ex.InnerException.Message)) ? " :: " + ex.InnerException.Message : ""),
                MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
            }        
        
        }
        protected override string OverrideTemplate()
        {
            var masterHome = AppLogic.HomeTemplate();
            if (masterHome.Trim().Length == 0)
            {
                masterHome = "JeldWenTemplate";
            }
            if (masterHome.EndsWith(".ascx"))
            {
                masterHome = masterHome.Replace(".ascx", ".master");
            }
            if (!masterHome.EndsWith(".master", StringComparison.OrdinalIgnoreCase))
            {
                masterHome = masterHome + ".master";
            }
            if (!CommonLogic.FileExists(CommonLogic.SafeMapPath("~/App_Templates/Skin_" + SkinID + "/" + masterHome)))
            {
                masterHome = "JeldWenTemplate";
            }
            return masterHome;
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            if (Session["hdnreferalurl"] != "")
                Response.Redirect(Session["hdnreferalurl"].ToString());
        }

    }
}
