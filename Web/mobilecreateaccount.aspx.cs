// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;
using Vortx.MobileFramework;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for createaccount.c
    /// </summary>
    public partial class mobilecreateaccount : SkinBase
    {

        bool Checkout = false;
        bool SkipRegistration = false;
        bool RequireSecurityCode = false;
        bool AllowShipToDifferentThanBillTo = false;
        bool VerifyAddressPrompt = false;
        String VerifyResult = String.Empty;
        String ReturnURL = String.Empty;
        String checkouttype = "";
        Address BillingAddress = new AspDotNetStorefrontCore.Address(); // qualification needed for vb.net (not sure why)
        Address ShippingAddress = new AspDotNetStorefrontCore.Address(); // qualification needed for vb.net (not sure why)
        Address StandardizedAddress = new AspDotNetStorefrontCore.Address();

        protected void Page_Load(object sender, System.EventArgs e)
        {
            MobileHelper.RedirectPageWhenMobileIsDisabled("~/createaccount.aspx", ThisCustomer);

            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            RequireSecurePage();
            ThisCustomer.RequireCustomerRecord();

            SectionTitle = AppLogic.GetString("createaccount.aspx.1", SkinID, ThisCustomer.LocaleSetting);
            Checkout = CommonLogic.QueryStringBool("checkout");
            SkipRegistration = CommonLogic.QueryStringBool("skipreg");
            checkouttype = CommonLogic.QueryStringCanBeDangerousContent("checkouttype");			

            if (!Checkout)
            {
                RequireSecurityCode = AppLogic.AppConfigBool("SecurityCodeRequiredOnCreateAccount");
            }
            else
            {
                RequireSecurityCode = AppLogic.AppConfigBool("SecurityCodeRequiredOnCreateAccountDuringCheckout");
            }

            if (RequireSecurityCode)
            {
                if (!IsPostBack)
                {
                    // Create a random code and store it in the Session object.
                    ctrlAccount.SecurityImage = "~/Captcha.ashx?id=1";
                }
                else
                {
                    String code = Session["SecurityCode"].ToString();

                    ctrlAccount.SecurityImage = "~/Captcha.ashx?id=2";
                }
            }
            else
            {
                ctrlAccount.ShowSecurityCode = false;
                ctrlAccount.txtSecurityCode.Visible = false;
                ctrlAccount.imgAccountSecurityImage.Visible = false;
                ctrlAccount.lblSecurityCode.Visible = false;
            }

            if (Checkout)
            {
                SectionTitle = AppLogic.GetString("createaccount.aspx.2", SkinID, ThisCustomer.LocaleSetting) + SectionTitle;

                // -----------------------------------------------------------------------------------------------
                // NOTE ON PAGE LOAD LOGIC:
                // We are checking here for required elements to allowing the customer to stay on this page.
                // Many of these checks may be redundant, and they DO add a bit of overhead in terms of db calls, but ANYTHING really
                // could have changed since the customer was on the last page. Remember, the web is completely stateless. Assume this
                // page was executed by ANYONE at ANYTIME (even someone trying to break the cart). 
                // It could have been yesterday, or 1 second ago, and other customers could have purchased limitied inventory products, 
                // coupons may no longer be valid, etc, etc, etc...
                // -----------------------------------------------------------------------------------------------
                ShoppingCart cart = new ShoppingCart(SkinID, ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);
                cart.ValidProceedCheckout(); // will not come back from this if any issue. they are sent back to the cart page!

            }

            AllowShipToDifferentThanBillTo = AppLogic.AppConfigBool("AllowShipToDifferentThanBillTo") &&
                                            !AppLogic.AppConfigBool("SkipShippingOnCheckout");
            ReturnURL = CommonLogic.QueryStringCanBeDangerousContent("ReturnURL");
            AppLogic.CheckForScriptTag(ReturnURL);
            ErrorMsgLabel.Text = "";

            if (!IsPostBack)
            {
                BillingAddress.LoadByCustomer(ThisCustomer.CustomerID, ThisCustomer.PrimaryBillingAddressID, AddressTypes.Billing);
                ShippingAddress.LoadByCustomer(ThisCustomer.CustomerID, ThisCustomer.PrimaryShippingAddressID, AddressTypes.Shipping);
                InitializeValidationErrorMessages();
                InitializePageContent();
            }

            #region vortx init xmlpackage
            //CheckoutHeader.SetContext = this;
            CheckoutHeader.Text = Vortx.MobileFramework.MobileXSLTExtensionBase.GetCheckoutHeader("address");
            #endregion
        }


        #region EventHandlers

        public void btnContinueCheckout_Click(object sender, EventArgs e)
        {
            CreateAccount();
        }

        //public void btnShppingEqBilling_Click(object sender, EventArgs e)
        //{
        //    SetPasswordFields();
        //    StateDropdownData(ctrlBillingAddress, ctrlShippingAddress);

        //    ctrlShippingAddress.NickName = ctrlBillingAddress.NickName;
        //    ctrlShippingAddress.FirstName = ctrlBillingAddress.FirstName;
        //    ctrlShippingAddress.LastName = ctrlBillingAddress.LastName;
        //    ctrlShippingAddress.PhoneNumber = ctrlBillingAddress.PhoneNumber;
        //    ctrlShippingAddress.Company = ctrlBillingAddress.Company;
        //    ctrlShippingAddress.ResidenceType = ctrlBillingAddress.ResidenceType;
        //    ctrlShippingAddress.Address1 = ctrlBillingAddress.Address1;
        //    ctrlShippingAddress.Address2 = ctrlBillingAddress.Address2;
        //    ctrlShippingAddress.Suite = ctrlBillingAddress.Suite;
        //    ctrlShippingAddress.City = ctrlBillingAddress.City;
        //    ctrlShippingAddress.Country = ctrlBillingAddress.Country;
        //    ctrlShippingAddress.State = ctrlBillingAddress.State;
        //    ctrlShippingAddress.ZipCode = ctrlBillingAddress.ZipCode;
        //    ctrlShippingAddress.ShowZip = ctrlBillingAddress.ShowZip;
        //}

        #endregion

        #region Private Functions

        private void InitializePageContent()
        {

            if (Checkout)
            {
                pnlCheckoutImage.Visible = true;
            }

            if (CommonLogic.QueryStringNativeInt("errormsg") > 0)
            {
                ErrorMessage e = new ErrorMessage(CommonLogic.QueryStringNativeInt("errormsg"));
				pnlErrorMsg.Controls.Add(new LiteralControl(String.Format("<b style='color:red;'>{0}</b>", e.Message)));
            }

            if (!ThisCustomer.IsRegistered)
            {
                Signin.Text = "<ul data-role=\"listview\" class=\"action\"><li><a id=\"MPSignInLink\" class=\"fullwidth\" href=\"signin.aspx?checkout=" + CommonLogic.QueryStringBool("checkout").ToString().ToLowerInvariant() + "&returnURL=" + Server.UrlEncode(CommonLogic.IIF(Checkout, "shoppingcart.aspx?checkout=true", "account.aspx")) + "\"><b>" + AppLogic.GetString("createaccount.aspx.4", SkinID, ThisCustomer.LocaleSetting) + "</b></a></li></ul>";
            }

            //if the customer already has entered a password don't ask them for another one
            Password p = new Password("", ThisCustomer.SaltKey);

            ctrlAccount.ShowPassword = (ThisCustomer.Password == "" || ThisCustomer.Password == p.SaltedPassword);

            ctrlAccount.Over13 = ThisCustomer.IsOver13;
            ctrlAccount.VATRegistrationID = ThisCustomer.VATRegistrationID;

         
            pnlAccountInfo.Visible = true;
            if (ViewState["custpwd"] == null)
            {
                ctrlAccount.txtPassword.TextMode = TextBoxMode.Password;
                ctrlAccount.txtPasswordConfirm.TextMode = TextBoxMode.Password;
            }

            ShoppingCart cart = new ShoppingCart(ThisCustomer.SkinID, ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);

            if (Checkout && !cart.HasRecurringComponents() && (AppLogic.AppConfigBool("PasswordIsOptionalDuringCheckout") || AppLogic.AppConfigBool("HidePasswordFieldDuringCheckout")))
            {
                ctrlAccount.PasswordValidator.Visible = false;
                ctrlAccount.PasswordValidator.Enabled = false;
            }
            ctrlAccount.ShowOver13 = AppLogic.AppConfigBool("RequireOver13Checked");
            if (!AppLogic.AppConfigBool("Vat.Enabled"))
            {
                ctrlAccount.ShowVATRegistrationID = false;
            }

            if (!IsPostBack)
            {
                ctrlAccount.FirstName = Server.HtmlEncode(CommonLogic.IIF(ThisCustomer.FirstName.Length != 0, ThisCustomer.FirstName, BillingAddress.FirstName));
                ctrlAccount.LastName = Server.HtmlEncode(CommonLogic.IIF(ThisCustomer.LastName.Length != 0, ThisCustomer.LastName, BillingAddress.LastName));

                String emailx = ThisCustomer.EMail;

                ctrlAccount.Email = Server.HtmlEncode(emailx).ToLowerInvariant().Trim();

                ctrlAccount.Phone = Server.HtmlEncode(CommonLogic.IIF(ThisCustomer.Phone.Length != 0, ThisCustomer.Phone, BillingAddress.Phone));
                // Create a phone validation error message

                ctrlAccount.OKToEmailYes = (ThisCustomer.EMail.Length != 0);
                ctrlAccount.OKToEmailNo = !ctrlAccount.OKToEmailYes;
            }

            
            if (!ThisCustomer.IsRegistered)
            {
                if (SkipRegistration)
                {
                    btnContinueCheckout.Text = CommonLogic.IIF(Checkout, AppLogic.GetString("createaccount.aspx.76", ThisCustomer.SkinID, ThisCustomer.LocaleSetting), AppLogic.GetString("createaccount.aspx.75", SkinID, ThisCustomer.LocaleSetting));
                }
                else
                {
                    btnContinueCheckout.Text = CommonLogic.IIF(Checkout, AppLogic.GetString("createaccount.aspx.74", ThisCustomer.SkinID, ThisCustomer.LocaleSetting), AppLogic.GetString("createaccount.aspx.75", SkinID, ThisCustomer.LocaleSetting));
                }
            }
            else
            {
                btnContinueCheckout.Text = AppLogic.GetString("account.aspx.60", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
            }

            AppLogic.GetButtonDisable(this.btnContinueCheckout, this.btnContinueCheckout.ValidationGroup);
        }

        private void InitializeValidationErrorMessages()
        {
            //valRegExSkipRegEmail.ErrorMessage = AppLogic.GetString("createaccount.aspx.17", SkinID, ThisCustomer.LocaleSetting);
        }

        private void AddToNewsletterList(string firstName, string lastName, string emailAddress)
        {
            new NewsletterControlService().Subscribe(emailAddress, firstName, lastName);
        }

        private void CreateAccount()
        {
            SetPasswordFields();

            string AccountName = (ctrlAccount.FirstName.Trim() + " " + ctrlAccount.LastName.Trim()).Trim();

            if (SkipRegistration)
            {
                Page.Validate("skipreg");
            }
            else
            {
                if (ctrlAccount.Password.Contains('\xFF') || ctrlAccount.Password.Length == 0)
                    ctrlAccount.PasswordValidate = ViewState["custpwd"].ToString();
                else
                    ctrlAccount.PasswordValidate = ctrlAccount.Password;

                if (ctrlAccount.PasswordConfirm.Contains('\xFF') || ctrlAccount.PasswordConfirm.Length == 0)
                    ctrlAccount.PasswordConfirmValidate = ViewState["custpwd2"].ToString();
                else
                    ctrlAccount.PasswordConfirmValidate = ctrlAccount.PasswordConfirm;

                ctrlAccount.Over13 = ctrlAccount.Over13;
                if ((!ThisCustomer.IsRegistered) && (ctrlAccount.Password.Length == 0 || ctrlAccount.PasswordConfirm.Length == 0))
                {
                    ErrorMsgLabel.Text = "createaccount.aspx.6".StringResource();
                    ResetScrollPosition();
                    return;
                }

                Page.Validate("registration");

                if (RequireSecurityCode)
                {
                    if (Session["SecurityCode"] != null)
                    {
                        String sCode = Session["SecurityCode"].ToString();
                        String fCode = ctrlAccount.txtSecurityCode.Text;
                        Boolean codeMatch = false;

                        if (AppLogic.AppConfigBool("Captcha.CaseSensitive"))
                        {
                            if (fCode.Equals(sCode))
                            {
                                codeMatch = true;
                            }
                        }
                        else
                        {
                            if (fCode.Equals(sCode, StringComparison.InvariantCultureIgnoreCase))
                            {
                                codeMatch = true;
                            }
                        }

                        if (!codeMatch)
                        {
                            ErrorMsgLabel.Text = string.Format(AppLogic.GetString("lat_signin_process.aspx.5", SkinID, ThisCustomer.LocaleSetting), sCode, fCode);
                            ctrlAccount.txtSecurityCode.Text = String.Empty;
                            ctrlAccount.imgAccountSecurityImage.ImageUrl = "~/Captcha.ashx?id=1";
                            ResetScrollPosition();
                            return;
                        }
                    }
                    else
                    {
                        ErrorMsgLabel.Text = string.Format(AppLogic.GetString("lat_signin_process.aspx.5", SkinID, ThisCustomer.LocaleSetting), "", ctrlAccount.txtSecurityCode.Text);
                        ctrlAccount.txtSecurityCode.Text = String.Empty;
                        ctrlAccount.imgAccountSecurityImage.ImageUrl = "~/Captcha.ashx?id=1";
                        ResetScrollPosition();
                        return;
                    }
                }

                if (!Page.IsValid && RequireSecurityCode)
                {
                    Session["SecurityCode"] = CommonLogic.GenerateRandomCode(6);
                }

            }

            //ctrlBillingAddress.CountryIDToValidateZipCode = AppLogic.GetCountryID(ctrlBillingAddress.Country);
            //ctrlShippingAddress.CountryIDToValidateZipCode = AppLogic.GetCountryID(ctrlBillingAddress.Country);

            Page.Validate("createacccount");


            if (Page.IsValid && AccountName.Length > 0)
            {
                //String EMailField = CommonLogic.IIF(SkipRegistration, txtSkipRegEmail.Text.ToLowerInvariant().Trim(), ctrlAccount.Email.ToLowerInvariant().Trim());
                String EMailField = ctrlAccount.Email.ToLowerInvariant().Trim();
                bool NewEmailAllowed = Customer.NewEmailPassesDuplicationRules(EMailField, ThisCustomer.CustomerID, false);

                String PWD = ViewState["custpwd"].ToString();
                Password p = new Password(PWD);
                String newpwd = p.SaltedPassword;
                System.Nullable<int> newsaltkey = p.Salt;

                Password blankpwd = new Password("", ThisCustomer.SaltKey);
                if (!(ThisCustomer.Password == "" || ThisCustomer.Password == blankpwd.SaltedPassword))
                {
                    // do NOT allow passwords to be changed on this page. this is only for creating an account.
                    // if they want to change their password, they must use their account page
                    newpwd = null;
                    newsaltkey = null;
                }
                if (NewEmailAllowed)
                {
                    AppLogic.eventHandler("CreateAccount").CallEvent("&CreateAccount=true");

                    ThisCustomer.UpdateCustomer(
                        /*CustomerLevelID*/ null,
                        /*EMail*/ EMailField,
                        /*SaltedAndHashedPassword*/ newpwd,
                        /*SaltKey*/ newsaltkey,
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
                        /*BillingEqualsShipping*/ CommonLogic.IIF(AppLogic.AppConfigBool("AllowShipToDifferentThanBillTo"), 0, 1),
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
                        /*VATRegistrationID*/ null,
                        /*StoreCCInDB*/ CommonLogic.IIF(ctrlAccount.ShowSaveCC, ctrlAccount.SaveCC, true),
                        /*IsRegistered*/ CommonLogic.IIF(SkipRegistration, 0, 1),
                        /*LockedUntil*/ null,
                        /*AdminCanViewCC*/ null,
                        /*BadLogin*/ null,
                        /*Active*/ null,
                        /*PwdChangeRequired*/ null,
                        /*RegisterDate*/ null,
                        /*StoreId*/AppLogic.StoreID()
                     );
                    if (ctrlAccount.OKToEmailYes)
                    {
                        AddToNewsletterList(ctrlAccount.FirstName, ctrlAccount.LastName, EMailField);
                    }

					if (AppLogic.AppConfigBool("Vat.Enabled") && ctrlAccount.VATRegistrationID.Length > 0)
					{
						String vtr = ctrlAccount.VATRegistrationID.Trim();

						Exception vatServiceException = null;
						if (AppLogic.VATRegistrationIDIsValid("UK", vtr, out vatServiceException))
						{
							ThisCustomer.SetVATRegistrationID(vtr);
						}
						else
						{
							vtr = String.Empty;

							if (vatServiceException != null && !String.IsNullOrEmpty(vatServiceException.Message))
							{
								if (vatServiceException.Message.Length > 255)
									ErrorMsgLabel.Text = Server.HtmlEncode(vatServiceException.Message.Substring(0, 255));
								else
									ErrorMsgLabel.Text = Server.HtmlEncode(vatServiceException.Message);
							}
							else
							{
								ErrorMsgLabel.Text = "account.aspx.91".StringResource();
							}
							return;
						}

						
					}
                    if (AppLogic.AppConfigBool("DynamicRelatedProducts.Enabled") ||
                        AppLogic.AppConfigBool("RecentlyViewedProducts.Enabled"))
                    {
                        ThisCustomer.ReplaceProductViewFromAnonymous();
                    }
                }
                if (Checkout)
                {
                    if (!NewEmailAllowed)
                    {
                        ErrorMsgLabel.Text = AppLogic.GetString("createaccount_process.aspx.1", 1, Localization.GetDefaultLocale());
                        InitializePageContent();
                    }
                    else
                    {
                        if (AppLogic.AppConfigBool("SendWelcomeEmail") && EMailField.IndexOf("@") != -1)
                        {
                            // don't let a simple welcome stop checkout!
                            try
                            {
                                string body = AppLogic.RunXmlPackage(AppLogic.AppConfig("XmlPackage.WelcomeEmail"),
                                                null,
                                                ThisCustomer,
                                                this.SkinID,
                                                "",
                                                "fullname=" + ctrlAccount.FirstName.Trim() + " " + ctrlAccount.LastName.Trim(),
                                                false,
                                                false,
                                                this.EntityHelpers);

                                AppLogic.SendMail(AppLogic.GetString("createaccount.aspx.79", ThisCustomer.SkinID, ThisCustomer.LocaleSetting),
                                    body,
                                    true,
                                    AppLogic.AppConfig("MailMe_FromAddress"),
                                    AppLogic.AppConfig("MailMe_FromName"),
                                    EMailField,
                                    ctrlAccount.FirstName.Trim() + " " + ctrlAccount.LastName.Trim(),
                                    "",
                                    AppLogic.MailServer());
                            }
                            catch { }
                        } 
                        Response.Redirect("mobilegetaddress.aspx?addresstype=billing&" + BuildRedirectQuerystring());
                    }
                }
                else
                {
                    if (!NewEmailAllowed)
                    {
                        DB.ExecuteSQL("update customer set EMail='', IsRegistered = 0 where CustomerID=" + ThisCustomer.CustomerID);
                        ErrorMsgLabel.Text = AppLogic.GetString("createaccount_process.aspx.1", 1, Localization.GetDefaultLocale());
                        InitializePageContent();
                    }
                    else
                    {
                        if (AppLogic.AppConfigBool("SendWelcomeEmail") && EMailField.IndexOf("@") != -1)
                        {
                            // don't let a simple welcome stop checkout!
                            try
                            {
                                string body = AppLogic.RunXmlPackage(AppLogic.AppConfig("XmlPackage.WelcomeEmail"),
                                                null,
                                                ThisCustomer,
                                                this.SkinID,
                                                "",
                                                "",
                                                false,
                                                false,
                                                this.EntityHelpers);

                                AppLogic.SendMail(AppLogic.GetString("createaccount.aspx.79", ThisCustomer.SkinID, ThisCustomer.LocaleSetting),
                                    body,
                                    true,
                                    AppLogic.AppConfig("MailMe_FromAddress"),
                                    AppLogic.AppConfig("MailMe_FromName"),
                                    EMailField,
                                    ctrlAccount.FirstName.Trim() + " " + ctrlAccount.LastName.Trim(), "",
                                    AppLogic.MailServer());
                            }
                            catch { }
                        }
                        if (VerifyAddressPrompt)
                        {
                            if (AllowShipToDifferentThanBillTo)
                            {
                                Response.Redirect("address.aspx?Checkout=False&AddressType=Shipping&AddressID=" + Customer.GetCustomerPrimaryShippingAddressID(ThisCustomer.CustomerID).ToString() + "&NewAccount=true&prompt=" + VerifyResult);
                            }
                            else
                            {
                                Response.Redirect("address.aspx?Checkout=False&AddressType=Billing&AddressID=" + Customer.GetCustomerPrimaryShippingAddressID(ThisCustomer.CustomerID).ToString() + "&NewAccount=true&prompt=" + VerifyResult);
                            }
                        }
                        else
                        {
                            Response.Redirect("account.aspx?newaccount=true");
                        }
                    }
                }
            }
            else
            {
                ErrorMsgLabel.Text += "" + AppLogic.GetString("createaccount.aspx.84", 1, Localization.GetDefaultLocale()) + "";
                if (AccountName.Length == 0)
                {
                    ErrorMsgLabel.Text += "&bull; " + AppLogic.GetString("createaccount.aspx.5", 1, Localization.GetDefaultLocale()) + "";
                }
                foreach (IValidator aValidator in this.Validators)
                {
                    if (!aValidator.IsValid)
                    {
                        ErrorMsgLabel.Text += "&bull; " + aValidator.ErrorMessage + "";
                    }
                }
                ErrorMsgLabel.Text += "";
                ResetScrollPosition();
            }
                
            pnlErrorMsg.Visible = (ErrorMsgLabel.Text.Length > 5);
        }

        private void SetPasswordFields()
        {
            if (ViewState["custpwd"] == null)
            {
                ViewState["custpwd"] = "";
            }

            if (ctrlAccount.Password.Trim() != "" && Regex.IsMatch(ctrlAccount.Password.Trim(), "[^\xFF]", RegexOptions.Compiled))
            {
                ViewState["custpwd"] = ctrlAccount.Password;

                ctrlAccount.PasswordReqFieldValidator.Enabled = false;

                string fillpwd = new string('\xFF', ctrlAccount.Password.Length);

                ctrlAccount.txtPassword.Attributes.Add("value", fillpwd);
            }

            if (ViewState["custpwd2"] == null)
            {
                ViewState["custpwd2"] = "";
            }
            if (ctrlAccount.PasswordConfirm != "" && Regex.IsMatch(ctrlAccount.PasswordConfirm.Trim(), "[^\xFF]", RegexOptions.Compiled))
            {
                ViewState["custpwd2"] = ctrlAccount.PasswordConfirm;

                string fillpwd2 = new string('\xFF', ctrlAccount.PasswordConfirm.Length);

                ctrlAccount.txtPasswordConfirm.Attributes.Add("value", fillpwd2);
            }
        }

        private void ResetScrollPosition()
        {
            if (!ClientScript.IsClientScriptBlockRegistered(this.GetType(), "CreateResetScrollPosition"))
            {
                StringBuilder script = new StringBuilder();
                script.AppendLine();
                script.Append("<script type=\"text/javascript\">\n");
                script.Append("function ResetScrollPosition() {\n");
                script.Append("  var scrollX = document.getElementById('__SCROLLPOSITIONX');\n");
                script.Append("  var scrollY = document.getElementById('__SCROLLPOSITIONY');\n");
                script.Append("  if (scrollX != null && scrollY != null)\n");
                script.Append("     {\n");
                script.Append("         scrollX.value = 0;\n");
                script.Append("         scrollY.value = 0;\n");
                script.Append("     }\n");
                script.Append("}\n");
                script.Append("</script>\n");

                ClientScript.RegisterClientScriptBlock(this.GetType(), "CreateResetScrollPosition", script.ToString());
                ClientScript.RegisterStartupScript(this.GetType(), "CallResetScrollPosition", "ResetScrollPosition();", true);
            }

        }
        #endregion

        protected override void OnInit(EventArgs e)
        {
            //InitializePageContent2();
            if (AppLogic.SecureNetVaultIsEnabled())
            {
                ctrlAccount.ShowSaveCC = true;
                ctrlAccount.SaveCC = true;
            }
            base.OnInit(e);
        }

        protected void ctrlAddress_SelectedCountryChanged(object sender, EventArgs e)
        {
            AddressControl ctrlAddress = sender as AddressControl;

            if (ctrlAddress != null)
            {
                StateDropdownData(ctrlAddress);
                ctrlAddress.ShowZip = AppLogic.GetCountryPostalCodeRequired(AppLogic.GetCountryID(ctrlAddress.Country));
            }
        }

        private void CountryDropdownData(AddressControl ctrlAddress)
        {
            if (ctrlAddress != null)
            {
                //Billing Country DropDown
                ctrlAddress.CountryDataSource = Country.GetAll();
                ctrlAddress.CountryDataTextField = "Name";
                ctrlAddress.CountryDataValueField = "Name";

            }
        }

        private void StateDropdownData(AddressControl ctrlAddress)
        {
            ctrlAddress.StateDataSource = State.GetAllStateForCountry(AppLogic.GetCountryID(ctrlAddress.Country), ThisCustomer.LocaleSetting);
        }

        private void StateDropdownData(AddressControl ctrlBilling, AddressControl ctrlShipping)
        {
            ctrlShipping.StateDataSource = State.GetAllStateForCountry(AppLogic.GetCountryID(ctrlBilling.Country), ThisCustomer.LocaleSetting);
        }

        private string BuildRedirectQuerystring()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("checkout=" + Checkout.ToString() + "&");
            if (checkouttype != "")
            {
                sb.Append("checkouttype=" + checkouttype + "&");
            }
            if (ReturnURL != "")
            {
                sb.Append("returnurl=" + ReturnURL + "&");
            }
            return sb.ToString();
        }
    }

}
