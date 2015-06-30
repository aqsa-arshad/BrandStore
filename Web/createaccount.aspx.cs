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
using AspDotNetStorefrontCore.Validation;

namespace AspDotNetStorefront
{
    public partial class createaccount : SkinBase
    {
        bool Checkout = false;
        bool SkipRegistration = false;
        bool RequireSecurityCode = false;
        bool AllowShipToDifferentThanBillTo = false;
        bool VerifyAddressPrompt = false;
        String VerifyAddressesProvider = AppLogic.AppConfig("VerifyAddressesProvider");
        String VerifyResult = String.Empty;
        String ReturnURL = String.Empty;
        String checkouttype = "";
        Address BillingAddress = new AspDotNetStorefrontCore.Address(); // qualification needed for vb.net (not sure why)
        Address ShippingAddress = new AspDotNetStorefrontCore.Address(); // qualification needed for vb.net (not sure why)
        Address StandardizedAddress = new AspDotNetStorefrontCore.Address();

        protected void Page_Load(object sender, System.EventArgs e)
        {
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
                CheckoutSteps.Visible = false;
            }
            else
            {
                CheckoutSteps.Visible = true;
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
            lblErrorMessage.Text = "";

            if (!IsPostBack)
            {
                BillingAddress.LoadByCustomer(ThisCustomer.CustomerID, ThisCustomer.PrimaryBillingAddressID, AddressTypes.Billing);
                ShippingAddress.LoadByCustomer(ThisCustomer.CustomerID, ThisCustomer.PrimaryShippingAddressID, AddressTypes.Shipping);
                InitializeValidationErrorMessages();
                InitializePageContent();
            }
            else
            {
                ctrlAccount.txtPassword.Attributes.Add("value", ctrlAccount.txtPassword.Text);
                ctrlAccount.txtPasswordConfirm.Attributes.Add("value", ctrlAccount.txtPasswordConfirm.Text);
                GetJavaScriptFunctions();
            }
        }


        #region EventHandlers

        public void btnContinueCheckout_Click(object sender, EventArgs e)
        {
            CreateAccount();
        }

        public void btnShppingEqBilling_Click(object sender, EventArgs e)
        {
            SetPasswordFields();
            StateDropdownData(ctrlBillingAddress, ctrlShippingAddress);

            ctrlShippingAddress.NickName = ctrlBillingAddress.NickName;
            ctrlShippingAddress.FirstName = ctrlBillingAddress.FirstName;
            ctrlShippingAddress.LastName = ctrlBillingAddress.LastName;
            ctrlShippingAddress.PhoneNumber = ctrlBillingAddress.PhoneNumber;
            ctrlShippingAddress.Company = ctrlBillingAddress.Company;
            ctrlShippingAddress.ResidenceType = ctrlBillingAddress.ResidenceType;
            ctrlShippingAddress.Address1 = ctrlBillingAddress.Address1;
            ctrlShippingAddress.Address2 = ctrlBillingAddress.Address2;
            ctrlShippingAddress.Suite = ctrlBillingAddress.Suite;
            ctrlShippingAddress.City = ctrlBillingAddress.City;
            ctrlShippingAddress.Country = ctrlBillingAddress.Country;
            ctrlShippingAddress.State = ctrlBillingAddress.State;
            ctrlShippingAddress.ZipCode = ctrlBillingAddress.ZipCode;
            ctrlShippingAddress.ShowZip = ctrlBillingAddress.ShowZip;
        }

        #endregion

        public void ValidateAddress(object source, ServerValidateEventArgs args)
        {
            String Adr1 = CommonLogic.IIF(AllowShipToDifferentThanBillTo, ctrlShippingAddress.Address1, ctrlBillingAddress.Address1);
            Adr1 = Adr1.Replace(" ", "").Trim().Replace(".", "");
            bool IsPOBoxAddress = !(new POBoxAddressValidator()).IsValid(Adr1);
            bool RejectDueToPOBoxAddress = (IsPOBoxAddress && AppLogic.AppConfigBool("DisallowShippingToPOBoxes")); // undocumented feature
            args.IsValid = !RejectDueToPOBoxAddress;
        }

        #region Private Functions

        private void InitializePageContent()
        {
            if (CommonLogic.QueryStringNativeInt("errormsg") > 0)
            {
                ErrorMessage e = new ErrorMessage(CommonLogic.QueryStringNativeInt("errormsg"));
				lblErrorMessage.Text = string.Format("<div class='error-large'>{0}</div>", e.Message);
                pnlErrorMsg.Visible = true;
            }

            if (Checkout && !ThisCustomer.IsRegistered)
            {
                ltSignin.Text = "<div class='page-row signin-row'>" + AppLogic.GetString("createaccount.aspx.3", SkinID, ThisCustomer.LocaleSetting) + " <a href=\"signin.aspx?checkout=" + CommonLogic.QueryStringBool("checkout").ToString().ToLowerInvariant() + "&returnURL=" + Server.UrlEncode(CommonLogic.IIF(Checkout, "shoppingcart.aspx?checkout=true", "account.aspx")) + "\">" + AppLogic.GetString("createaccount.aspx.4", SkinID, ThisCustomer.LocaleSetting) + "</a>.</div>";
            }

            //if the customer already has entered a password don't ask them for another one
            Password p = new Password("", ThisCustomer.SaltKey);

            ctrlAccount.ShowPassword = (ThisCustomer.Password == "" || ThisCustomer.Password == p.SaltedPassword);

            ctrlAccount.Over13 = ThisCustomer.IsOver13;
            ctrlAccount.VATRegistrationID = ThisCustomer.VATRegistrationID;

            //Account Info
            if (!SkipRegistration)
            {
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
            }
            else
            {
                valReqSkipRegEmail.Enabled = AppLogic.AppConfigBool("AnonCheckoutReqEmail");
                String emailx = ThisCustomer.EMail;
                txtSkipRegEmail.Text = Server.HtmlEncode(emailx).ToLowerInvariant().Trim();

                Literal2.Visible = AppLogic.AppConfigBool("RequireOver13Checked");
                SkipRegOver13.Visible = AppLogic.AppConfigBool("RequireOver13Checked");

                SkipRegOver13.Checked = ThisCustomer.IsOver13;
                pnlSkipReg.Visible = true;
                createaccountaspx30.Visible = false;
                BillingEqualsAccount.Visible = false;
                createaccountaspx31.Visible = false;
            }

            if (!IsPostBack)
            {
                createaccountaspx31.Text = AppLogic.GetString("createaccount.aspx.31", SkinID, ThisCustomer.LocaleSetting);
                if (AllowShipToDifferentThanBillTo)
                {
                    createaccountaspx30.Text = AppLogic.GetString("createaccount.aspx.30", SkinID, ThisCustomer.LocaleSetting);
                }
                else
                {
                    createaccountaspx30.Text = AppLogic.GetString("createaccount.aspx.32", SkinID, ThisCustomer.LocaleSetting);
                }

                ctrlBillingAddress.NickName = Server.HtmlEncode(ctrlBillingAddress.NickName);
                ctrlBillingAddress.FirstName = Server.HtmlEncode(CommonLogic.IIF(ThisCustomer.FirstName.Length != 0, ThisCustomer.FirstName, BillingAddress.FirstName));
                ctrlBillingAddress.LastName = Server.HtmlEncode(CommonLogic.IIF(ThisCustomer.LastName.Length != 0, ThisCustomer.LastName, BillingAddress.LastName));
                ctrlBillingAddress.PhoneNumber = Server.HtmlEncode(BillingAddress.Phone);
                ctrlBillingAddress.Company = Server.HtmlEncode(BillingAddress.Company);
                ctrlBillingAddress.ResidenceType = BillingAddress.ResidenceType.ToString();
                ctrlBillingAddress.Address1 = Server.HtmlEncode(BillingAddress.Address1);
                ctrlBillingAddress.Address2 = Server.HtmlEncode(BillingAddress.Address2);
                ctrlBillingAddress.Suite = Server.HtmlEncode(BillingAddress.Suite);
                ctrlBillingAddress.State = Server.HtmlEncode(BillingAddress.State);
                ctrlBillingAddress.City = Server.HtmlEncode(BillingAddress.City);
                ctrlBillingAddress.ZipCode = BillingAddress.Zip;
                ctrlBillingAddress.ShowZip = AppLogic.GetCountryPostalCodeRequired(AppLogic.GetCountryID(ctrlBillingAddress.Country));

                //Shipping Info
                if (AllowShipToDifferentThanBillTo)
                {
                    pnlShippingInfo.Visible = true;
                    ctrlShippingAddress.NickName = Server.HtmlEncode(ShippingAddress.NickName);
                    ctrlShippingAddress.FirstName = Server.HtmlEncode(CommonLogic.IIF(ThisCustomer.FirstName.Length != 0, ThisCustomer.FirstName, BillingAddress.FirstName));
                    ctrlShippingAddress.LastName = Server.HtmlEncode(CommonLogic.IIF(ThisCustomer.LastName.Length != 0, ThisCustomer.LastName, BillingAddress.LastName));
                    ctrlShippingAddress.PhoneNumber = Server.HtmlEncode(ShippingAddress.Phone);
                    ctrlShippingAddress.Company = Server.HtmlEncode(ShippingAddress.Company);
                    ctrlShippingAddress.ResidenceType = ShippingAddress.ResidenceType.ToString();
                    ctrlShippingAddress.Address1 = Server.HtmlEncode(ShippingAddress.Address1);
                    ctrlShippingAddress.Address2 = Server.HtmlEncode(ShippingAddress.Address2);
                    ctrlShippingAddress.Suite = Server.HtmlEncode(ShippingAddress.Suite);
                    ctrlShippingAddress.City = Server.HtmlEncode(ShippingAddress.City);
                    ctrlShippingAddress.State = Server.HtmlEncode(ShippingAddress.State);
                    ctrlShippingAddress.ZipCode = ShippingAddress.Zip;
                    ctrlShippingAddress.ShowZip = AppLogic.GetCountryPostalCodeRequired(AppLogic.GetCountryID(ctrlShippingAddress.Country));
                }

                if (!Checkout)
                {
                    //hide billing and shipping inputs
                    pnlBillingInfo.Visible = false;
                    pnlShippingInfo.Visible = false;
                }
            }

            GatewayCheckoutByAmazon.CheckoutByAmazon checkoutByAmazon = new GatewayCheckoutByAmazon.CheckoutByAmazon();

            if (checkoutByAmazon.IsCheckingOut)
            {
                pnlAccountInfo.Visible = !ThisCustomer.IsRegistered && !SkipRegistration;

                pnlBillingInfo.Visible =
                    pnlShippingInfo.Visible = false;

                pnlCBAAddressWidget.Visible = true;

                litCBAAddressWidget.Text = checkoutByAmazon.RenderAddressWidget("CBAAddressWidgetContainer", false, String.Empty, new Guid(ThisCustomer.CustomerGUID), 300, 200);
                litCBAAddressWidgetInstruction.Text = "gw.checkoutbyamazon.display.4".StringResource();
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

            GetJavaScriptFunctions();
            AppLogic.GetButtonDisable(this.btnContinueCheckout, this.btnContinueCheckout.ValidationGroup);

        }

        private void InitializeValidationErrorMessages()
        {
            valRegExSkipRegEmail.ErrorMessage = AppLogic.GetString("createaccount.aspx.17", SkinID, ThisCustomer.LocaleSetting);
        }

        private void GetJavaScriptFunctions()
        {
            BillingEqualsAccount.Attributes.Add("onclick", "copyaccount(this.form);");
            string strScript = "<script type=\"text/javascript\">";
            strScript += "function copyaccount(theForm){ ";
            strScript += "if (theForm." + BillingEqualsAccount.ClientID + ".checked){";
            strScript += "theForm." + ctrlBillingAddress.FindControl("FirstName").ClientID + ".value = theForm." + ctrlAccount.FindControl("txtFirstName").ClientID + ".value;";
            strScript += "theForm." + ctrlBillingAddress.FindControl("LastName").ClientID + ".value = theForm." + ctrlAccount.FindControl("txtLastName").ClientID + ".value;";
            strScript += "theForm." + ctrlBillingAddress.FindControl("Phone").ClientID + ".value = theForm." + ctrlAccount.FindControl("txtPhone").ClientID + ".value;";
            strScript += "} ";
            strScript += "return true; }  ";
            strScript += "</script> ";

            ClientScript.RegisterClientScriptBlock(this.GetType(), "CopyAccount", strScript);

        }

        private void AddToNewsletterList(string firstName, string lastName, string emailAddress)
        {
            new NewsletterControlService().Subscribe(emailAddress, firstName, lastName);
        }
        private void CreateAccount()
        {
            GatewayCheckoutByAmazon.CheckoutByAmazon checkoutByAmazon = new GatewayCheckoutByAmazon.CheckoutByAmazon();

            if (checkoutByAmazon.IsEnabled && checkoutByAmazon.IsCheckingOut && checkoutByAmazon.GetDefaultShippingAddress() == null)
            {
                lblErrorMessage.Text = "gw.checkoutbyamazon.display.3".StringResource();
                pnlErrorMsg.Visible = true;
                return;
            }

            if (checkoutByAmazon.IsEnabled && checkoutByAmazon.IsCheckingOut && ThisCustomer.IsRegistered)
            {
                checkoutByAmazon.BeginCheckout(new Guid(ThisCustomer.CustomerGUID), false, false);
                Response.Redirect("checkoutshipping.aspx");
            }
            else if (checkoutByAmazon.IsEnabled && checkoutByAmazon.IsCheckingOut)
            {
                checkoutByAmazon.BeginCheckout(new Guid(ThisCustomer.CustomerGUID), false, false);
            }

            SetPasswordFields();

            string AccountName = (ctrlAccount.FirstName.Trim() + " " + ctrlAccount.LastName.Trim()).Trim();
            if (SkipRegistration)
            {
                AccountName = String.Format("{0} {1}", ctrlBillingAddress.FirstName.Trim(), ctrlBillingAddress.LastName.Trim()).Trim();

                if (checkoutByAmazon.IsEnabled && checkoutByAmazon.IsCheckingOut)
                    AccountName = "Anonymous Amazon Customer";
            }

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
                if ((!ThisCustomer.IsRegistered) && !checkoutByAmazon.IsCheckingOut && (ctrlAccount.Password.Length == 0 || ctrlAccount.PasswordConfirm.Length == 0))
                {
                    lblErrorMessage.Text = "createaccount.aspx.6".StringResource();
                    ResetScrollPosition();
                    pnlErrorMsg.Visible = true;
                    return;
                }

                ctrlBillingAddress.CountryIDToValidateZipCode = AppLogic.GetCountryID(ctrlBillingAddress.Country);
                ctrlShippingAddress.CountryIDToValidateZipCode = AppLogic.GetCountryID(ctrlShippingAddress.Country);

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
                            lblErrorMessage.Text = string.Format(AppLogic.GetString("lat_signin_process.aspx.5", SkinID, ThisCustomer.LocaleSetting), sCode, fCode);
                            ctrlAccount.txtSecurityCode.Text = String.Empty;
                            ctrlAccount.imgAccountSecurityImage.ImageUrl = "~/Captcha.ashx?id=1";
                            ResetScrollPosition();
                            pnlErrorMsg.Visible = true;
                            return;
                        }
                    }
                    else
                    {
                        lblErrorMessage.Text = string.Format(AppLogic.GetString("lat_signin_process.aspx.5", SkinID, ThisCustomer.LocaleSetting), "", ctrlAccount.txtSecurityCode.Text);
                        ctrlAccount.txtSecurityCode.Text = String.Empty;
                        ctrlAccount.imgAccountSecurityImage.ImageUrl = "~/Captcha.ashx?id=1";
                        ResetScrollPosition();
                        pnlErrorMsg.Visible = true;
                        return;
                    }
                }

                if (!Page.IsValid && RequireSecurityCode)
                {
                    Session["SecurityCode"] = CommonLogic.GenerateRandomCode(6);
                }

            }



            Page.Validate("createacccount");


            if (Page.IsValid && AccountName.Length > 0)
            {
                String EMailField = CommonLogic.IIF(SkipRegistration, txtSkipRegEmail.Text.ToLowerInvariant().Trim(), ctrlAccount.Email.ToLowerInvariant().Trim());

                bool NewEmailAllowed = Customer.NewEmailPassesDuplicationRules(EMailField, ThisCustomer.CustomerID, SkipRegistration);

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

                    string strDOB = null;
                    if (AppLogic.AppConfigBool("Account.ShowBirthDateField"))
                    {
                        strDOB = ctrlAccount.DOBMonth + "/" + ctrlAccount.DOBDay + "/" + ctrlAccount.DOBYear;
                        //DOB defaults to 0/0/0 when doing anonymous checkout and blows up dbo.aspdnsf_updCustomer, preventing checkout
                        strDOB = (strDOB.Equals("0/0/0", StringComparison.Ordinal)) ? null : strDOB;
                    }

                    var defaultCustomerLevel_Public = 4;

                    ThisCustomer.UpdateCustomer(
                        /*CustomerLevelID*/ defaultCustomerLevel_Public,
                        /*EMail*/ EMailField,
                        /*SaltedAndHashedPassword*/ newpwd,
                        /*SaltKey*/ newsaltkey,
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
                        /*Over13Checked*/ CommonLogic.IIF(ctrlAccount.Over13 || SkipRegOver13.Checked, 1, 0),
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
                    BillingAddress = ThisCustomer.PrimaryBillingAddress;
                    if (BillingAddress.AddressID == 0 && !checkoutByAmazon.IsCheckingOut)
                    {
                        if (pnlBillingInfo.Visible)
                        {
                            BillingAddress.NickName = ctrlBillingAddress.NickName;
                            BillingAddress.LastName = ctrlBillingAddress.LastName;
                            BillingAddress.FirstName = ctrlBillingAddress.FirstName;
                            BillingAddress.Phone = ctrlBillingAddress.PhoneNumber;
                            BillingAddress.Company = ctrlBillingAddress.Company;
                            BillingAddress.ResidenceType = (ResidenceTypes)Enum.Parse(typeof(ResidenceTypes), ctrlBillingAddress.ResidenceType);
                            BillingAddress.Address1 = ctrlBillingAddress.Address1;
                            BillingAddress.Address2 = ctrlBillingAddress.Address2;
                            BillingAddress.Suite = ctrlBillingAddress.Suite;
                            BillingAddress.City = ctrlBillingAddress.City;
                            BillingAddress.State = ctrlBillingAddress.State;
                            BillingAddress.Zip = ctrlBillingAddress.ZipCode;
                            BillingAddress.Country = ctrlBillingAddress.Country;

                            BillingAddress.InsertDB(ThisCustomer.CustomerID);
                            BillingAddress.MakeCustomersPrimaryAddress(AddressTypes.Billing);
                        }
                    }
                    else
                    {
                        BillingAddress.NickName = String.Format("{0} {1}", ctrlAccount.FirstName, ctrlAccount.LastName);
                        BillingAddress.LastName = ctrlAccount.FirstName;
                        BillingAddress.FirstName = ctrlAccount.LastName;
                        BillingAddress.Phone = ctrlAccount.Phone;
                    }

                    ShippingAddress = ThisCustomer.PrimaryShippingAddress;
                    if (ShippingAddress.AddressID == 0 && !checkoutByAmazon.IsCheckingOut)
                    {
                        if (AllowShipToDifferentThanBillTo)
                        {
                            if (ctrlShippingAddress.Visible)
                            {
                                ShippingAddress.NickName = ctrlBillingAddress.NickName;
                                ShippingAddress.LastName = ctrlShippingAddress.LastName;
                                ShippingAddress.FirstName = ctrlShippingAddress.FirstName;
                                ShippingAddress.Phone = ctrlShippingAddress.PhoneNumber;
                                ShippingAddress.Company = ctrlShippingAddress.Company;
                                ShippingAddress.ResidenceType = (ResidenceTypes)Enum.Parse(typeof(ResidenceTypes), ctrlShippingAddress.ResidenceType);
                                ShippingAddress.Address1 = ctrlShippingAddress.Address1;
                                ShippingAddress.Address2 = ctrlShippingAddress.Address2;
                                ShippingAddress.Suite = ctrlShippingAddress.Suite;
                                ShippingAddress.City = ctrlShippingAddress.City;
                                ShippingAddress.State = ctrlShippingAddress.State;
                                ShippingAddress.Zip = ctrlShippingAddress.ZipCode;
                                ShippingAddress.Country = ctrlShippingAddress.Country;

                                ShippingAddress.InsertDB(ThisCustomer.CustomerID);
                                if (!String.IsNullOrEmpty(VerifyAddressesProvider))
                                {

                                    VerifyResult = AddressValidation.RunValidate(ShippingAddress, out StandardizedAddress);
                                    VerifyAddressPrompt = (VerifyResult != AppLogic.ro_OK);
                                    if (VerifyAddressPrompt)
                                    {
                                        ShippingAddress = StandardizedAddress;
                                        ShippingAddress.UpdateDB();
                                    }
                                }
                                ShippingAddress.MakeCustomersPrimaryAddress(AddressTypes.Shipping);
                            }
                        }
                        else
                        {
                            if (!String.IsNullOrEmpty(VerifyAddressesProvider))
                            {

                                VerifyResult = AddressValidation.RunValidate(BillingAddress, out StandardizedAddress);
                                VerifyAddressPrompt = (VerifyResult != AppLogic.ro_OK);
                                if (VerifyAddressPrompt)
                                {
                                    BillingAddress = StandardizedAddress;
                                    BillingAddress.UpdateDB();
                                }
                            }
                            BillingAddress.MakeCustomersPrimaryAddress(AddressTypes.Shipping);
                        }
                    }
                    else
                    {
                        ShippingAddress.NickName = String.Format("{0} {1}", ctrlAccount.FirstName, ctrlAccount.LastName);
                        ShippingAddress.LastName = ctrlAccount.FirstName;
                        ShippingAddress.FirstName = ctrlAccount.LastName;
                        ShippingAddress.Phone = ctrlAccount.Phone;
                    }

                    if (AppLogic.AppConfigBool("Vat.Enabled") && ctrlAccount.VATRegistrationID.Length > 0)
                    {
                        String vtr = ctrlAccount.VATRegistrationID.Trim();

                        Exception vatServiceException = null;
                        if (AppLogic.VATRegistrationIDIsValid(ctrlBillingAddress.Country, vtr, out vatServiceException))
                        {
                            ThisCustomer.SetVATRegistrationID(vtr);
                        }
                        else
                        {
                            vtr = String.Empty;

                            if (vatServiceException != null && !String.IsNullOrEmpty(vatServiceException.Message))
                            {
                                if (vatServiceException.Message.Length > 255)
                                    lblErrorMessage.Text = Server.HtmlEncode(vatServiceException.Message.Substring(0, 255));
                                else
                                    lblErrorMessage.Text = Server.HtmlEncode(vatServiceException.Message);
                            }
                            else
                            {
                                lblErrorMessage.Text = "account.aspx.91".StringResource();
                            }
                            pnlErrorMsg.Visible = true;
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
                        lblErrorMessage.Text = AppLogic.GetString("createaccount_process.aspx.1", 1, Localization.GetDefaultLocale());
                        InitializePageContent();
                    }
                    else
                    {
                        if (AppLogic.AppConfigBool("SendWelcomeEmail") && EMailField.IndexOf("@") != -1 && ThisCustomer.IsRegistered == true)
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
                        if (VerifyAddressPrompt)
                        {
                            if (AllowShipToDifferentThanBillTo)
                            {
                                Response.Redirect("address.aspx?Checkout=True&AddressType=Shipping&AddressID=" + Customer.GetCustomerPrimaryShippingAddressID(ThisCustomer.CustomerID).ToString() + "&NewAccount=true&prompt=" + VerifyResult + "&skipreg=" + SkipRegistration + "&returnURL=checkoutshipping.aspx?checkout=true");
                            }
                            else
                            {
                                Response.Redirect("address.aspx?Checkout=True&AddressType=Billing&AddressID=" + Customer.GetCustomerPrimaryShippingAddressID(ThisCustomer.CustomerID).ToString() + "&NewAccount=true&prompt=" + VerifyResult + "&skipreg=" + SkipRegistration + "&returnURL=checkoutshipping.aspx?checkout=true");
                            }
                        }
                        else
                        {
                            if (checkouttype == "ppec" || checkouttype == "ppbml" || checkouttype == "gc")
                            {
                                Response.Redirect("shoppingcart.aspx");
                            }
                            else
                            {
                                Response.Redirect("checkoutshipping.aspx");
                            }
                        }
                    }
                }
                else
                {
                    if (!NewEmailAllowed)
                    {
                        DB.ExecuteSQL("update customer set EMail='', IsRegistered = 0 where CustomerID=" + ThisCustomer.CustomerID);
                        lblErrorMessage.Text = AppLogic.GetString("createaccount_process.aspx.1", 1, Localization.GetDefaultLocale());
                        InitializePageContent();
                    }
                    else
                    {
                        if (AppLogic.AppConfigBool("SendWelcomeEmail") && EMailField.IndexOf("@") != -1 && ThisCustomer.IsRegistered == true)
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
                                Response.Redirect("address.aspx?Checkout=False&AddressType=Shipping&AddressID=" + Customer.GetCustomerPrimaryShippingAddressID(ThisCustomer.CustomerID).ToString() + "&NewAccount=true&prompt=" + VerifyResult + "&skipreg=" + SkipRegistration);
                            }
                            else
                            {
                                Response.Redirect("address.aspx?Checkout=False&AddressType=Billing&AddressID=" + Customer.GetCustomerPrimaryShippingAddressID(ThisCustomer.CustomerID).ToString() + "&NewAccount=true&prompt=" + VerifyResult + "&skipreg=" + SkipRegistration);
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
				lblErrorMessage.Text += String.Format("<div class='error-header'>{0}</div>", AppLogic.GetString("createaccount.aspx.84", 1, Localization.GetDefaultLocale()));
				lblErrorMessage.Text += "<ul class='error-list'>";
                if (AccountName.Length == 0)
                {
                    lblErrorMessage.Text += String.Format("<li>{0}</li>", AppLogic.GetString("createaccount.aspx.5", 1, Localization.GetDefaultLocale()));
                }
                foreach (IValidator aValidator in this.Validators)
                {
                    if (!aValidator.IsValid)
                    {
                        lblErrorMessage.Text += String.Format("<li>{0}</li>", aValidator.ErrorMessage);
                    }
                }
				lblErrorMessage.Text += "</ul>";
                lblErrorMessage.Text += "";
                ResetScrollPosition();
            }

            pnlErrorMsg.Visible = lblErrorMessage.Text.Length > 0;

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
            InitializePageContent2();
            if (AppLogic.SecureNetVaultIsEnabled())
            {
                ctrlAccount.ShowSaveCC = true;
                ctrlAccount.SaveCC = true;
            }
            base.OnInit(e);
        }

        private void InitializePageContent2()
        {
            if (ctrlBillingAddress != null)
            {
                CountryDropdownData(ctrlBillingAddress);
                StateDropdownData(ctrlBillingAddress);
            }

            if (ctrlShippingAddress != null)
            {
                CountryDropdownData(ctrlShippingAddress);
                StateDropdownData(ctrlShippingAddress);
            }
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
    }

}
