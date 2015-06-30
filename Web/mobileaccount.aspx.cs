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
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontCore.Validation;
using AspDotNetStorefrontGateways;
using AspDotNetStorefrontGateways.Processors;
using Vortx.MobileFramework;
using Vortx.MobileFramework.Utils;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for account.
    /// </summary>
    public partial class mobileaccount : SkinBase
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
            MobileHelper.RedirectPageWhenMobileIsDisabled("~/account.aspx", ThisCustomer);

            if (ThisCustomer.IsAdminUser || AppLogic.AppConfigBool("UseStrongPwd"))
                ctrlAccount.PasswordNote = AppLogic.GetString("account.strongPassword", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
            RequireSecurePage();
            RequiresLogin(CommonLogic.GetThisPageName(false) + "?" + CommonLogic.ServerVariables("QUERY_STRING"));
            SectionTitle = AppLogic.GetString("account.aspx.56", SkinID, ThisCustomer.LocaleSetting);
            Checkout = CommonLogic.QueryStringBool("checkout");
            if (Checkout)
            {
                ThisCustomer.RequireCustomerRecord();
                pnlAccountInfoMP.Visible = false;
                pnlOrderHistoryMP.Visible = false;
            }

            ErrorMsgLabel.Text = "";
            lblAcctUpdateMsg.Text = "";

            bool newAccount = CommonLogic.QueryStringBool("newaccount");
            if (newAccount)
            {
                ErrorMsgLabel.Text = "<b><center>" + AppLogic.GetString("createaccount.aspx.86", SkinID, ThisCustomer.LocaleSetting) + "</center></b>";
            }

            ThisCustomer.ValidatePrimaryAddresses();
			
            //If there is a DeleteID remove it from the cart
            int DeleteID = CommonLogic.QueryStringUSInt("DeleteID");
            if (DeleteID != 0 && Customer.OwnsThisOrder(ThisCustomer.CustomerID, DeleteID))
            {
                RecurringOrderMgr rmgr = new RecurringOrderMgr(base.EntityHelpers, base.GetParser);
                rmgr.CancelRecurringOrder(DeleteID);
            }

            //If there is a FullRefundID refund it
            int FullRefundID = CommonLogic.QueryStringUSInt("FullRefundID");
            if (FullRefundID != 0 && Customer.OwnsThisOrder(ThisCustomer.CustomerID, FullRefundID))
            {
                RecurringOrderMgr rmgr = new RecurringOrderMgr(base.EntityHelpers, base.GetParser);
                rmgr.ProcessAutoBillFullRefund(FullRefundID);
            }

            //If there is a PartialRefundID refund it
            int PartialRefundID = CommonLogic.QueryStringUSInt("PartialRefundID");
            if (PartialRefundID != 0 && Customer.OwnsThisOrder(ThisCustomer.CustomerID, PartialRefundID))
            {
                RecurringOrderMgr rmgr = new RecurringOrderMgr(base.EntityHelpers, base.GetParser);
                rmgr.ProcessAutoBillPartialRefund(PartialRefundID);
            }
			
            if (!this.IsPostBack)
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
                if (ThisCustomer.OKToEMail)
                {
                    ctrlAccount.OKToEmailYes = true;
                }
                else
                {
                    ctrlAccount.OKToEmailNo = true;
                }

                RefreshPage();
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

		private void InitPaymentMethods ()
        {
			if(AppLogic.ActivePaymentGatewayCleaned() == Gateway.ro_GWSECURENETVAULTV4 && AppLogic.SecureNetVaultIsEnabled())
            {
                ctrlAccount.ShowSaveCC = true;
                ctrlAccount.SaveCC = true;
            }
        }

        public void btnContinueToCheckOut_Click(object sender, EventArgs e)
        {
            Response.Redirect("checkoutshipping.aspx");
        }

        public void btnUpdateAccount_Click(object sender, EventArgs e)
        {
            ctrlAccount.PasswordValidate = ctrlAccount.Password;
            ctrlAccount.PasswordConfirmValidate = ctrlAccount.PasswordConfirm;
            ctrlAccount.Over13 = ctrlAccount.Over13;
            ErrorMsgLabel.Text = "";
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
								ErrorMsgLabel.Text = Server.HtmlEncode(vatServiceException.Message.Substring(0, 255));
							else
								ErrorMsgLabel.Text = Server.HtmlEncode(vatServiceException.Message);
						else
							ErrorMsgLabel.Text = "account.aspx.91".StringResource();
						
						vtr = null;
						ctrlAccount.ShowVATRegistrationIDInvalid = true;
						ctrlAccount.VATRegistrationID = String.Empty;
					}
                }
                ThisCustomer.UpdateCustomer(
                    /*CustomerLevelID*/ null,
                    /*EMail*/ EMailField,
                    /*SaltedAndHashedPassword*/ pwd,
                    /*SaltKey*/ saltkey,
                    /*DateOfBirth*/ null,
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
                if (ShippingStatus.Length != 0)
                {
                    ShippingStatus += "";
                }
                DateTime dwm = Localization.ParseDBDateTime(DownloadEMailSentOn);
                if (dwm != System.DateTime.MinValue)
                {
                    ShippingStatus += String.Format(AppLogic.GetString("account.aspx.53a", SkinID, ThisCustomer.LocaleSetting), Localization.ToThreadCultureShortDateString(dwm));
                }
                else
                {
                    ShippingStatus += AppLogic.GetString("account.aspx.53", SkinID, ThisCustomer.LocaleSetting);
                }
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
                return "";
            }

        }

        public string GetReorder(string OrderNumber, string RecurringSubscriptionID)
        {
            if (RecurringSubscriptionID.Length == 0)
            {
                return "<a href=\"javascript:ReOrder(" + OrderNumber + ");\">" + AppLogic.GetString("account.aspx.57", SkinID, ThisCustomer.LocaleSetting).Replace("<br />", "") + "</a>";
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
                pnlCheckoutImage.Visible = true;
                pnlAccountInfoMP.Visible = false;
                pnlOrderHistoryMP.Visible = false;
                if (ThisCustomer.PrimaryBillingAddressID == 0 || ThisCustomer.PrimaryShippingAddressID == 0 || !ThisCustomer.HasAtLeastOneAddress())
                {
                    ErrorMsgLabel.Text = AppLogic.GetString("account.aspx.73", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                }

            }

            pnlCheckoutImage.Visible = Checkout;
            ErrorMessage e = new ErrorMessage(CommonLogic.QueryStringNativeInt("errormsg"));
            ErrorMsgLabel.Text += Server.HtmlEncode(e.Message);
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

				//If email address confirmation is on, prefill the box so they don't have to populate it to change other things
				TextBox txtReEnterEmail = (TextBox)ctrlAccount.FindControl("txtReEnterEmail");
				if (txtReEnterEmail != null)
					txtReEnterEmail.Text = ThisCustomer.EMail.ToLowerInvariant().Trim();
            }

            OriginalEMail.Text = ThisCustomer.EMail;
            btnContinueToCheckOut.Visible = phContinueCheckout.Visible = Checkout;

            if (ThisCustomer.PrimaryBillingAddressID == 0)
            {
                pnlBilling.Visible = false;
            }
            if (ThisCustomer.PrimaryShippingAddressID == 0)
            {
                pnlShipping.Visible = false;
            }

            lnkAddBillingAddress.NavigateUrl = "address.aspx?add=true&addressType=Billing&Checkout=" + Checkout.ToString() + "&returnURL=" + Server.UrlEncode("account.aspx?checkout=" + Checkout.ToString());
            lnkAddBillingAddress.Text = AppLogic.GetString("account.aspx.63", SkinID, ThisCustomer.LocaleSetting);
            lnkAddShippingAddress.NavigateUrl = "address.aspx?add=true&addressType=Shipping&Checkout=" + Checkout.ToString() + "&returnURL=" + Server.UrlEncode("account.aspx?checkout=" + Checkout.ToString());
            lnkAddShippingAddress.Text = AppLogic.GetString("account.aspx.62", SkinID, ThisCustomer.LocaleSetting);

            litBillingAddress.Text = BillingAddress.DisplayHTML(true);
            if (BillingAddress.PaymentMethodLastUsed.Length != 0)
            {
                litBillingAddress.Text += "<b>" + AppLogic.GetString("account.aspx.31", SkinID, ThisCustomer.LocaleSetting) + "</b>";
                litBillingAddress.Text += BillingAddress.DisplayPaymentMethodInfo(ThisCustomer, BillingAddress.PaymentMethodLastUsed);
            }

            litShippingAddress.Text = ShippingAddress.DisplayHTML(true);
            pnlOrderHistory.Visible = !Checkout;

			if(!String.IsNullOrEmpty(ShippingAddress.Address1))
				litShipTeaser.Text = "(" + TextUtil.ClipText(ShippingAddress.Address1, 15) + ")";

			if(!String.IsNullOrEmpty(BillingAddress.Address1))
				litBillTeaser.Text = "(" + TextUtil.ClipText(BillingAddress.Address1, 15) + ")";
            
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

            #region Vortx Mobile Modification
            pnlOrderHistory.Visible = !accountaspx55.Visible;
            #endregion

            ctrlAccount.Password = String.Empty;
            ctrlAccount.PasswordConfirm = String.Empty;

            ClientScriptManager cs = Page.ClientScript;
            cs.RegisterClientScriptBlock(this.GetType(), Guid.NewGuid().ToString(), "function ReOrder(OrderNumber) {if(confirm('" + AppLogic.GetString("account.aspx.64", SkinID, ThisCustomer.LocaleSetting) + "')) {top.location.href='reorder.aspx?ordernumber='+OrderNumber;} }", true);
        }


    }

}
