// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontCore.Validation;
using AspDotNetStorefrontGateways;
using AspDotNetStorefrontGateways.Processors;

namespace AspDotNetStorefront
{
	public partial class account : SkinBase
	{
		bool AccountUpdated = false;
		bool NewEmailAddressAllowed = false;
		bool Checkout = false;
		public CultureInfo SqlServerCulture = new System.Globalization.CultureInfo(CommonLogic.Application("DBSQLServerLocaleSetting")); // qualification needed for vb.net (not sure why)
		public string m_StoreLoc = AppLogic.GetStoreHTTPLocation(true);

		public override bool RequireScriptManager
		{
			get
			{
				return true;
			}
		}

		protected void Page_Load(object sender, System.EventArgs e)
		{
            
            if (ThisCustomer.IsAdminUser || AppLogic.AppConfigBool("UseStrongPwd"))
				ctrlAccount.PasswordNote = AppLogic.GetString("account.strongPassword", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
			RequireSecurePage();
            Checkout = CommonLogic.QueryStringBool("checkout");
			RequiresLogin(CommonLogic.GetThisPageName(false) + "?" + CommonLogic.ServerVariables("QUERY_STRING"));
			SectionTitle = AppLogic.GetString("account.aspx.56", SkinID, ThisCustomer.LocaleSetting);
            ctrlAccount.Attributes.Add("Disabled", "");
			if (Checkout)
			{
				GatewayCheckoutByAmazon.CheckoutByAmazon checkoutByAmazon = new GatewayCheckoutByAmazon.CheckoutByAmazon();
				if (checkoutByAmazon.IsCheckingOut)
				{
					pnlCBAAddressWidget.Visible = true;
					litCBAAddressWidget.Text = checkoutByAmazon.RenderAddressWidgetWithRedirect("CBAAddressWidgetContainer", Server.UrlEncode("account.aspx?checkout=true"), new Guid(ThisCustomer.CustomerGUID), 300, 200);
					litCBAAddressWidgetInstruction.Text = "gw.checkoutbyamazon.display.4".StringResource();

					pnlAddress.Visible = false;
				}

				ThisCustomer.RequireCustomerRecord();
                CheckoutSteps.Visible = true;
			}
            else
            {
                CheckoutSteps.Visible = false;
            }

			lblErrorMessage.Text = String.Empty;
            pnlErrorMsg.Visible = false;
			lblAcctUpdateMsg.Text = String.Empty;

			var status = new StringBuilder();

			bool newAccount = CommonLogic.QueryStringBool("newaccount");
			if (newAccount)
			{
				lblErrorMessage.Text = AppLogic.GetString("createaccount.aspx.86", SkinID, ThisCustomer.LocaleSetting);
                pnlErrorMsg.Visible = true;
			}

			ThisCustomer.ValidatePrimaryAddresses();

			bool AllowShipToDifferentThanBillTo = AppLogic.AppConfigBool("AllowShipToDifferentThanBillTo") && !AppLogic.AppConfigBool("SkipShippingOnCheckout");
			if (!AllowShipToDifferentThanBillTo)
			{
				pnlShipping.Visible = false;
			}

			//If there is a DeleteID remove it from the cart
			int DeleteID = CommonLogic.QueryStringUSInt("DeleteID");
			if (DeleteID != 0 && Customer.OwnsThisOrder(ThisCustomer.CustomerID, DeleteID))
			{
				Order originalOrder = new Order(DeleteID);
				RecurringOrderMgr rmgr = new RecurringOrderMgr(base.EntityHelpers, base.GetParser);

				ExpressAPIType expressApiType = PayPalController.GetAppropriateExpressType();

				if (originalOrder.PaymentMethod == AppLogic.ro_PMPayPalExpress && expressApiType == ExpressAPIType.PayPalExpress)
					status.Append(rmgr.CancelPPECRecurringOrder(DeleteID, false));
				else
					status.Append(rmgr.CancelRecurringOrder(DeleteID));
			}

			//If there is a FullRefundID refund it
			int FullRefundID = CommonLogic.QueryStringUSInt("FullRefundID");
			if (FullRefundID != 0 && Customer.OwnsThisOrder(ThisCustomer.CustomerID, FullRefundID))
			{
				RecurringOrderMgr rmgr = new RecurringOrderMgr(base.EntityHelpers, base.GetParser);
				status.Append(rmgr.ProcessAutoBillFullRefund(FullRefundID));
			}

			//If there is a PartialRefundID refund it
			int PartialRefundID = CommonLogic.QueryStringUSInt("PartialRefundID");
			if (PartialRefundID != 0 && Customer.OwnsThisOrder(ThisCustomer.CustomerID, PartialRefundID))
			{
				RecurringOrderMgr rmgr = new RecurringOrderMgr(base.EntityHelpers, base.GetParser);
				status.Append(rmgr.ProcessAutoBillPartialRefund(PartialRefundID));
			}

			//do not display an error message saying OK
			lblErrorMessage.Text = status.ToString() != AppLogic.ro_OK ? status.ToString() : String.Empty;
            pnlErrorMsg.Visible = lblErrorMessage.Text.Length > 0;

			if (!this.IsPostBack)
			{
				RefreshPage();
                SetAccountFields();
                if (ThisCustomer.IsRegistered)
                {
                
                    this.hdnCustomerLevel1.Text = ThisCustomer.CustomerLevelID.ToString();
                }
                else
                {
          
                    hdnCustomerLevel1.Text = "-1";
                }
			}
           
		}

		private void SetAccountFields()
		{
			ctrlAccount.FirstName = ThisCustomer.FirstName;
			ctrlAccount.LastName = ThisCustomer.LastName;

			ctrlAccount.Email = ThisCustomer.EMail.ToLowerInvariant().Trim();
			ctrlAccount.Password = String.Empty;
			ctrlAccount.PasswordConfirm = String.Empty;
			ctrlAccount.Phone = ThisCustomer.Phone;
			ctrlAccount.SaveCC = ThisCustomer.MasterShouldWeStoreCreditCardInfo || ThisCustomer.SecureNetVaultMasterShouldWeStoreCreditCardInfo;
			ctrlAccount.Over13 = ThisCustomer.IsOver13;
			ctrlAccount.VATRegistrationID = ThisCustomer.VATRegistrationID;

			string[] dateOfBirth = new string[2];
			dateOfBirth = ThisCustomer.DateOfBirth.ToShortDateString().Split('/');
			ctrlAccount.DOBMonth = dateOfBirth[0].ToString();
			ctrlAccount.DOBDay = dateOfBirth[1].ToString();
			ctrlAccount.DOBYear = dateOfBirth[2].ToString();

			if (ThisCustomer.OKToEMail)
			{
				ctrlAccount.OKToEmailYes = true;
			}
			else
			{
				ctrlAccount.OKToEmailNo = true;
			}

			//If email address confirmation is on, prefill the box so they don't have to populate it to change other things
			TextBox txtReEnterEmail = (TextBox)ctrlAccount.FindControl("txtReEnterEmail");
			if (txtReEnterEmail != null)
				txtReEnterEmail.Text = ThisCustomer.EMail.ToLowerInvariant().Trim();
		}

		protected override void OnInit(EventArgs e)
		{
			InitPaymentMethods();
			base.OnInit(e);
		}

		private void InitPaymentMethods()
		{
			AuthorizeNet authorizeNet = new AuthorizeNet();

			if ((AppLogic.ActivePaymentGatewayCleaned() == Gateway.ro_GWSECURENETVAULTV4 && AppLogic.SecureNetVaultIsEnabled()) || authorizeNet.IsCimEnabled)
			{
				pnlPaymentMethods.Visible = true;
			}
			else
				return;

			pnlSecureNetPaymentMethods.Visible = AppLogic.ActivePaymentGatewayCleaned() == Gateway.ro_GWSECURENETVAULTV4 && AppLogic.SecureNetVaultIsEnabled();

			PanelWallet.Visible = authorizeNet.IsCimEnabled && ThisCustomer.PrimaryBillingAddressID != 0;
			if (ThisCustomer.PrimaryBillingAddressID == 0)
			{
				LabelPanelMessage.Visible = true;
				LabelPanelMessage.Text = AppLogic.GetString("account.aspx.92", SkinID, ThisCustomer.LocaleSetting);
			}
			else
				LabelPanelMessage.Visible = false;

			if (AppLogic.ActivePaymentGatewayCleaned() == Gateway.ro_GWSECURENETVAULTV4 && AppLogic.SecureNetVaultIsEnabled())
			{
				ctrlAccount.ShowSaveCC = true;
				ctrlAccount.SaveCC = true;
			}
		}

		public void btnContinueToCheckOut_Click(object sender, EventArgs e)
		{
            btnUpdateAccount_Click(null,null);
			Response.Redirect(CheckOutPageControllerFactory.CreateCheckOutPageController().GetContinueCheckoutPage());
		}

		public void btnUpdateAccount_Click(object sender, EventArgs e)
		{
			ctrlAccount.PasswordValidate = ctrlAccount.Password;
			ctrlAccount.PasswordConfirmValidate = ctrlAccount.PasswordConfirm;
			ctrlAccount.Over13 = ctrlAccount.Over13;
			lblErrorMessage.Text = String.Empty;
            pnlErrorMsg.Visible = false;
			Page.Validate("account");
			if (Page.IsValid)
			{
				String EMailField = ctrlAccount.Email.ToLowerInvariant().Trim();
				NewEmailAddressAllowed = Customer.NewEmailPassesDuplicationRules(EMailField, ThisCustomer.CustomerID, false);

				bool emailisvalid = new EmailAddressValidator().IsValidEmailAddress(EMailField);
				if (!emailisvalid)
				{
					lblAcctUpdateMsg.Text = AppLogic.GetString("createaccount.aspx.17", SkinID, ThisCustomer.LocaleSetting);
				}

				if (!NewEmailAddressAllowed || !emailisvalid)
				{
					EMailField = ThisCustomer.EMail; // preserve the old email but go ahead and update their account with other changes below
				}


				string pwd = null;
				object saltkey = null;
				if (ctrlAccount.Password.Trim().Length > 0)
				{
					Password p = new Password(ctrlAccount.Password);
					pwd = p.SaltedPassword;
					saltkey = p.Salt;
				}
				bool HasActiveRecurring = ThisCustomer.HasActiveRecurringOrders(true);

				ctrlAccount.ShowSaveCCNote = false;


				if (!ctrlAccount.SaveCC && (HasActiveRecurring && !AppLogic.AppConfigBool("Recurring.UseGatewayInternalBilling")))
				{
					ctrlAccount.SaveCC = true;
					ctrlAccount.ShowSaveCCNote = true;
				}

				String vtr = ctrlAccount.VATRegistrationID;
				if (!AppLogic.AppConfigBool("VAT.Enabled"))
				{
					vtr = null;
					ctrlAccount.ShowVATRegistrationIDInvalid = false;
					ctrlAccount.VATRegistrationID = String.Empty;
				}
				else
				{
					Exception vatServiceException;
					Boolean vatIsValid = AppLogic.VATRegistrationIDIsValid(ThisCustomer, vtr, out vatServiceException);
					if (ctrlAccount.VATRegistrationID.Length == 0 || vatIsValid)
					{
						ctrlAccount.ShowVATRegistrationIDInvalid = false;
					}
					else
					{
						if (vatServiceException != null && vatServiceException.Message.Length > 0)
							if (vatServiceException.Message.Length > 255)
								lblErrorMessage.Text = Server.HtmlEncode(vatServiceException.Message.Substring(0, 255));
							else
								lblErrorMessage.Text = Server.HtmlEncode(vatServiceException.Message);
						else
							lblErrorMessage.Text = "account.aspx.91".StringResource();

                        pnlErrorMsg.Visible = lblErrorMessage.Text.Length > 0;

						vtr = null;
						ctrlAccount.ShowVATRegistrationIDInvalid = true;
						ctrlAccount.VATRegistrationID = String.Empty;
					}
				}

				string strDOB = null;
				if (AppLogic.AppConfigBool("Account.ShowBirthDateField"))
				{
					strDOB = ctrlAccount.DOBMonth + "/" + ctrlAccount.DOBDay + "/" + ctrlAccount.DOBYear;
				}
				ThisCustomer.UpdateCustomer(
					/*CustomerLevelID*/ null,
					/*EMail*/ EMailField,
					/*SaltedAndHashedPassword*/ pwd,
					/*SaltKey*/ saltkey,
					/*DateOfBirth*/ strDOB,
					/*Gender*/ null,
					/*FirstName*/ ctrlAccount.FirstName,
					/*LastName*/ ctrlAccount.LastName,
					/*Notes*/ null,
					/*SkinID*/ null,
					/*Phone*/ ctrlAccount.Phone,
					/*AffiliateID*/ null,
					/*Referrer*/ null,
					/*CouponCode*/ null,
					/*OkToEmail*/ CommonLogic.IIF(ctrlAccount.OKToEmailYes, 1, 0),
					/*IsAdmin*/ null,
					/*BillingEqualsShipping*/ null,
					/*LastIPAddress*/ null,
					/*OrderNotes*/ null,
					/*SubscriptionExpiresOn*/ null,
					/*RTShipRequest*/ null,
					/*RTShipResponse*/ null,
					/*OrderOptions*/ null,
					/*LocaleSetting*/ null,
					/*MicroPayBalance*/ null,
					/*RecurringShippingMethodID*/ null,
					/*RecurringShippingMethod*/ null,
					/*BillingAddressID*/ null,
					/*ShippingAddressID*/ null,
					/*GiftRegistryGUID*/ null,
					/*GiftRegistryIsAnonymous*/ null,
					/*GiftRegistryAllowSearchByOthers*/ null,
					/*GiftRegistryNickName*/ null,
					/*GiftRegistryHideShippingAddresses*/ null,
					/*CODCompanyCheckAllowed*/ null,
					/*CODNet30Allowed*/ null,
					/*ExtensionData*/ null,
					/*FinalizationData*/ null,
					/*Deleted*/ null,
					/*Over13Checked*/ CommonLogic.IIF(ctrlAccount.Over13, 1, 0),
					/*CurrencySetting*/ null,
					/*VATSetting*/ null,
					/*VATRegistrationID*/ vtr,
					/*StoreCCInDB*/ CommonLogic.IIF(ctrlAccount.SaveCC, 1, 0),
					/*IsRegistered*/ null,
					/*LockedUntil*/ null,
					/*AdminCanViewCC*/ null,
					/*BadLogin*/ null,
					/*Active*/ null,
					/*PwdChangeRequired*/ null,
					/*RegisterDate*/ null,
					/*StoreId*/null
					);

				AccountUpdated = true;
			}
			RefreshPage();
		}

		public string GetPaymentStatus(string PaymentMethod, string CardNumber, string TransactionState, object decOrderTotal)
		{
			decimal OrderTotal = Convert.ToDecimal(decOrderTotal);
			if (OrderTotal == Decimal.Zero)
			{
				return AppLogic.GetString("order.cs.16", SkinID, ThisCustomer.LocaleSetting);
			}
			String PaymentStatus = String.Empty;
			if (PaymentMethod.Length != 0)
			{
				PaymentStatus = AppLogic.GetString("account.aspx.43", SkinID, ThisCustomer.LocaleSetting) + " " + PaymentMethod.Replace(AppLogic.ro_PMMicropay, AppLogic.GetString("account.aspx.11", SkinID, ThisCustomer.LocaleSetting)) + "";
			}
			else
			{
				PaymentStatus = AppLogic.GetString("account.aspx.43", SkinID, ThisCustomer.LocaleSetting) + " " + CommonLogic.IIF(CardNumber.StartsWith(AppLogic.ro_PMPayPal, StringComparison.InvariantCultureIgnoreCase), AppLogic.GetString("account.aspx.44", SkinID, ThisCustomer.LocaleSetting), AppLogic.GetString("account.aspx.45", SkinID, ThisCustomer.LocaleSetting)) + "";
			}
			PaymentStatus += CommonLogic.IIF(TransactionState == AppLogic.ro_TXStatePending, "<span style=\"color:red;\">" + TransactionState + "</span>", TransactionState);
			return PaymentStatus;

		}

		public string GetShippingStatus(int OrderNumber, string ShippedOn, string ShippedVIA, string ShippingTrackingNumber, string TransactionState, string DownloadEMailSentOn)
		{
			String ShippingStatus = String.Empty;
			if (AppLogic.OrderHasShippableComponents(OrderNumber))
			{
				if (ShippedOn != "")
				{
					ShippingStatus = AppLogic.GetString("account.aspx.48", SkinID, ThisCustomer.LocaleSetting);
					if (ShippedVIA.Length != 0)
					{
						ShippingStatus += " " + AppLogic.GetString("account.aspx.49", SkinID, ThisCustomer.LocaleSetting) + " " + ShippedVIA;
					}

					ShippingStatus += " " + AppLogic.GetString("account.aspx.50", SkinID, ThisCustomer.LocaleSetting) + " " + Localization.ParseNativeDateTime(ShippedOn).ToString(new CultureInfo(ThisCustomer.LocaleSetting));
					if (ShippingTrackingNumber.Length != 0)
					{
						ShippingStatus += " " + AppLogic.GetString("account.aspx.51", SkinID, ThisCustomer.LocaleSetting) + " ";

						String TrackURL = Shipping.GetTrackingURL(ShippingTrackingNumber);
						if (TrackURL.Length != 0)
						{
							ShippingStatus += "<a href=\"" + TrackURL + "\" target=\"_blank\">" + ShippingTrackingNumber + "</a>";
						}
						else
						{
							ShippingStatus += ShippingTrackingNumber;
						}
					}
				}
				else
				{
					ShippingStatus = AppLogic.GetString("account.aspx.52", SkinID, ThisCustomer.LocaleSetting);
				}
			}
			if (AppLogic.OrderHasDownloadComponents(OrderNumber, true))
			{
				ShippingStatus += string.Format("<div><a href=\"downloads.aspx\">{0}</a></div>", AppLogic.GetString("download.aspx.1", SkinID, ThisCustomer.LocaleSetting));
			}

			return ShippingStatus;
		}

		public string GetOrderTotal(int QuoteCheckout, string PaymentMethod, object decOrderTotal, int CouponType, object decCouponAmount)
		{
			decimal OrderTotal = Convert.ToDecimal(decOrderTotal);
			decimal CouponAmount = Convert.ToDecimal(decCouponAmount);
			if (CouponType == 2)
				OrderTotal = CommonLogic.IIF(OrderTotal < CouponAmount, 0, OrderTotal - CouponAmount);

			return CommonLogic.IIF(QuoteCheckout == 1 || AppLogic.CleanPaymentMethod(PaymentMethod) == AppLogic.ro_PMRequestQuote, AppLogic.GetString("account.aspx.54", SkinID, ThisCustomer.LocaleSetting), ThisCustomer.CurrencyString(OrderTotal));
		}

		public string GetCustSvcNotes(string CustomerServiceNotes)
		{
			if (AppLogic.AppConfigBool("ShowCustomerServiceNotesInReceipts"))
			{
				return CommonLogic.IIF(CustomerServiceNotes.Length == 0, AppLogic.GetString("order.cs.29", SkinID, ThisCustomer.LocaleSetting), CustomerServiceNotes);
			}
			else
			{
				return String.Empty;
			}

		}

		public string GetReorder(string OrderNumber, string RecurringSubscriptionID)
		{
            if (RecurringSubscriptionID.Length == 0 && AppLogic.AppConfigBool("Reorder.Enabled"))
            {
                return "<a href=\"javascript:ReOrder(" + OrderNumber + ");\">" + AppLogic.GetString("account.aspx.57", SkinID, ThisCustomer.LocaleSetting) + "</a>";
            }
            else
            {
                return String.Empty;
            }	
		}

		public void RefreshPage()
        {
            Address BillingAddress = new Address();
            Address ShippingAddress = new Address();

            BillingAddress.LoadByCustomer(ThisCustomer.CustomerID, ThisCustomer.PrimaryBillingAddressID, AddressTypes.Billing);
            ShippingAddress.LoadByCustomer(ThisCustomer.CustomerID, ThisCustomer.PrimaryShippingAddressID, AddressTypes.Shipping);

            if (Checkout)
            {
                if (ThisCustomer.PrimaryBillingAddressID == 0 || ThisCustomer.PrimaryShippingAddressID == 0 ||
                    !ThisCustomer.HasAtLeastOneAddress() || (AppLogic.AppConfigBool("DisallowShippingToPOBoxes") && (!(new POBoxAddressValidator()).IsValid(ShippingAddress))))
                {
                    lblErrorMessage.Text = AppLogic.GetString("account.aspx.73", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                    pnlErrorMsg.Visible = true;
                }

            }

            ErrorMessage e = new ErrorMessage(CommonLogic.QueryStringNativeInt("errormsg"));
            lblErrorMessage.Text += Server.HtmlEncode(e.Message);
            pnlAccountUpdated.Visible = AccountUpdated;
            if (AccountUpdated)
            {
                if (!NewEmailAddressAllowed)
                {
                    lblAcctUpdateMsg.Text += CommonLogic.IIF(lblAcctUpdateMsg.Text.Trim() == "", "", "") + AppLogic.GetString("account.aspx.3", SkinID, ThisCustomer.LocaleSetting);
                    ctrlAccount.Email = ThisCustomer.EMail;
                }
                else
                {
                    lblAcctUpdateMsg.Text = CommonLogic.IIF(lblAcctUpdateMsg.Text.Trim() == "", "", "") + AppLogic.GetString("account.aspx.2", SkinID, ThisCustomer.LocaleSetting);
                }

                //In case email address confirmation is on.
                TextBox txtReEnterEmail = (TextBox)ctrlAccount.FindControl("txtReEnterEmail");
                if (txtReEnterEmail != null)
                    txtReEnterEmail.Text = String.Empty;
            }

            pnlNotCheckOutButtons.Visible = !Checkout;
            pnlShowWishButton.Visible = AppLogic.AppConfigBool("ShowWishButtons");
            pnlShowGiftRegistryButtons.Visible = AppLogic.AppConfigBool("ShowGiftRegistryButtons");
            pnlSubscriptionExpiresOn.Visible = (ThisCustomer.SubscriptionExpiresOn > System.DateTime.Now);
            lblSubscriptionExpiresOn.Text = String.Format(AppLogic.GetString("account.aspx.5", ThisCustomer.SkinID, ThisCustomer.LocaleSetting), Localization.ToThreadCultureShortDateString(ThisCustomer.SubscriptionExpiresOn));
            OriginalEMail.Text = ThisCustomer.EMail;
            //lblCustomerLevel.Visible = pnlCustomerLevel.Visible = (ThisCustomer.CustomerLevelID != 0);
            lblCustomerLevel.Visible = pnlCustomerLevel.Visible = false;
            lblCustomerLevel.Text = String.Format(AppLogic.GetString("account.aspx.9", ThisCustomer.SkinID, ThisCustomer.LocaleSetting), ThisCustomer.CustomerLevelName);
            lblMicroPayEnabled.Visible = pnlMicroPayEnabled.Visible = (AppLogic.MicropayIsEnabled() && ThisCustomer.IsRegistered && AppLogic.GetMicroPayProductID() != 0);
            lblMicroPayEnabled.Text = String.Format(AppLogic.GetString("account.aspx.10", ThisCustomer.SkinID, ThisCustomer.LocaleSetting), AppLogic.GetString("account.aspx.11", ThisCustomer.SkinID, ThisCustomer.LocaleSetting), ThisCustomer.CurrencyString(ThisCustomer.MicroPayBalance));
            btnContinueToCheckOut.Visible = Checkout;

            GatewayCheckoutByAmazon.CheckoutByAmazon checkoutByAmazon = new GatewayCheckoutByAmazon.CheckoutByAmazon();

            if (ThisCustomer.PrimaryBillingAddressID == 0 || checkoutByAmazon.IsAmazonAddress(ThisCustomer.PrimaryBillingAddress))
            {
              //  pnlBilling.Visible = false;
            }
            if (ThisCustomer.PrimaryShippingAddressID == 0 || checkoutByAmazon.IsAmazonAddress(ThisCustomer.PrimaryShippingAddress))
            {
                //pnlShipping.Visible = false;
            }
            lnkChangeBilling.NavigateUrl = "javascript:self.location='JWMyAddresses.aspx?Checkout=" + Checkout.ToString() + "&AddressType=1&returnURL=" + Server.UrlEncode("account.aspx?checkout=" + Checkout.ToString()) + "'";
            lnkChangeShipping.NavigateUrl = "javascript:self.location='JWMyAddresses.aspx?Checkout=" + Checkout.ToString() + "&AddressType=2&returnURL=" + Server.UrlEncode("account.aspx?checkout=" + Checkout.ToString()) + "'";

            //lnkAddBillingAddress.NavigateUrl = "JWMyAddresses.aspx?add=true&addressType=1&Checkout=" + Checkout.ToString() + "&returnURL=" + Server.UrlEncode("account.aspx?checkout=" + Checkout.ToString());
            //lnkAddBillingAddress.Text = "<div>" + AppLogic.GetString("account.aspx.63", SkinID, ThisCustomer.LocaleSetting) + "</div>";
            //lnkAddShippingAddress.NavigateUrl = "JWMyAddresses.aspx?add=true&addressType=2&Checkout=" + Checkout.ToString() + "&returnURL=" + Server.UrlEncode("account.aspx?checkout=" + Checkout.ToString());
            //lnkAddShippingAddress.Text = "<div>" + AppLogic.GetString("account.aspx.62", SkinID, ThisCustomer.LocaleSetting) + "</div>";

            if (BillingAddress.AddressID != 0)
            {
                litBillingAddress.Text = BillingAddress.DisplayHTML(true);
            }
            if (BillingAddress.PaymentMethodLastUsed.Length != 0)
            {
                //need for future use
                //litBillingAddress.Text += "<div>" + AppLogic.GetString("account.aspx.31", SkinID, ThisCustomer.LocaleSetting);
              //  litBillingAddress.Text += "<div>" + BillingAddress.DisplayPaymentMethodInfo(ThisCustomer, BillingAddress.PaymentMethodLastUsed) + "</div>";
            }

            if (!(new POBoxAddressValidator()).IsValid(ShippingAddress))
            {
                litShippingAddress.Text = "<div class='error-wrap'>" + "createaccount_process.aspx.3".StringResource() + "</div>"; //PO box not allowed
            }
            else
            {
                if (ShippingAddress.AddressID != 0)
                {
                    litShippingAddress.Text = "Cannot ship to P.O boxes"+ ShippingAddress.DisplayHTML(true);
                }
            }
               

            pnlOrderHistory.Visible = !Checkout;

            GiftCards gc = new GiftCards(ThisCustomer.CustomerID, GiftCardCollectionFilterType.UsingCustomerID);
            if (gc.Count > 0)
            {
                rptrGiftCards.DataSource = gc;
                rptrGiftCards.DataBind();
                pnlGiftCards.Visible = true;
            }

            if (ShoppingCart.NumItems(ThisCustomer.CustomerID, CartTypeEnum.RecurringCart) != 0)
            {
                ltRecurringOrders.Text = "<div class=\"group-header account-header recurring-header\">" + AppLogic.GetString("account.aspx.35", SkinID, ThisCustomer.LocaleSetting) + "</div>";

                // build JS code to show/hide address update block:
                StringBuilder tmpS = new StringBuilder(4096);
                tmpS.Append("<script type=\"text/javascript\">\n");
                tmpS.Append("function toggleLayer(DivID)\n");
                tmpS.Append("{\n");
                tmpS.Append("	var elem;\n");
                tmpS.Append("	var vis;\n");
                tmpS.Append("	if(document.getElementById)\n");
                tmpS.Append("	{\n");
                tmpS.Append("		// standards\n");
                tmpS.Append("		elem = document.getElementById(DivID);\n");
                tmpS.Append("	}\n");
                tmpS.Append("	else if(document.all)\n");
                tmpS.Append("	{\n");
                tmpS.Append("		// old msie versions\n");
                tmpS.Append("		elem = document.all[DivID];\n");
                tmpS.Append("	}\n");
                tmpS.Append("	else if(document.layers)\n");
                tmpS.Append("	{\n");
                tmpS.Append("		// nn4\n");
                tmpS.Append("		elem = document.layers[DivID];\n");
                tmpS.Append("	}\n");
                tmpS.Append("	vis = elem.style;\n");
                tmpS.Append("	if(vis.display == '' && elem.offsetWidth != undefined && elem.offsetHeight != undefined)\n");
                tmpS.Append("	{\n");
                tmpS.Append("		vis.display = (elem.offsetWidth != 0 && elem.offsetHeight != 0) ? 'block' : 'none';\n");
                tmpS.Append("	}\n");
                tmpS.Append("	vis.display = (vis.display == '' || vis.display == 'block') ? 'none' : 'block' ;\n");
                tmpS.Append("}\n");
                tmpS.Append("</script>\n");
                tmpS.Append("\n");
                tmpS.Append("<style type=\"text/css\">\n");
                tmpS.Append("	.addressBlockDiv { margin: 0px 20px 0px 20px;  display: none;}\n");
                tmpS.Append("</style>\n");
                ltRecurringOrders.Text += tmpS.ToString();

                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rsr = DB.GetRS("Select distinct OriginalRecurringOrderNumber from ShoppingCart   with (NOLOCK)  where CartType=" + ((int)CartTypeEnum.RecurringCart).ToString() + " and CustomerID=" + ThisCustomer.CustomerID.ToString() + " order by OriginalRecurringOrderNumber desc", con))
                    {
                        while (rsr.Read())
                        {
                            ltRecurringOrders.Text += AppLogic.GetRecurringCart(base.EntityHelpers, base.GetParser, ThisCustomer, DB.RSFieldInt(rsr, "OriginalRecurringOrderNumber"), SkinID, false);
                        }
                    }
                }
            }

            string[] TrxStates = { DB.SQuote(AppLogic.ro_TXStateAuthorized), DB.SQuote(AppLogic.ro_TXStateCaptured), DB.SQuote(AppLogic.ro_TXStatePending) };

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS(string.Format("Select OrderNumber, OrderDate, RecurringSubscriptionID, PaymentMethod, CardNumber, TransactionState, QuoteCheckout, ShippedOn, ShippedVIA, ShippingTrackingNumber, DownloadEMailSentOn, QuoteCheckout, PaymentMethod, " +
                    "OrderTotal, CouponType, isnull(CouponDiscountAmount, 0) CouponDiscountAmount, CustomerServiceNotes  from dbo.orders   with (NOLOCK)  where TransactionState in ({0}) and CustomerID={1} and ({2} = 0 or StoreID = {3}) order by OrderDate desc", String.Join(",", TrxStates),
                    ThisCustomer.CustomerID, CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowCustomerFiltering") == true, 1, 0), AppLogic.StoreID()), con))
                {
                    orderhistorylist.DataSource = rs;
                    orderhistorylist.DataBind();
                }
            }

            accountaspx55.Visible = (orderhistorylist.Items.Count == 0);

            ClientScriptManager cs = Page.ClientScript;
            cs.RegisterClientScriptBlock(this.GetType(), Guid.NewGuid().ToString(), "function ReOrder(OrderNumber) {if(confirm('" + AppLogic.GetString("account.aspx.64", SkinID, ThisCustomer.LocaleSetting) + "')) {top.location.href='reorder.aspx?ordernumber='+OrderNumber;} }", true);
           
            ctrlAccount.Password = String.Empty;
            ctrlAccount.PasswordConfirm = String.Empty;
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
	}

}
