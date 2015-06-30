// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for checkoutanon.
    /// </summary>
    public partial class checkoutanon : SkinBase
    {

        String PaymentMethod = String.Empty;
        String checkouttype = "";
        ShoppingCart cart;
        Customer c;
        private Boolean DisablePasswordAutocomplete
        {
            get { return AppLogic.AppConfigBool("DisablePasswordAutocomplete"); }
        }

        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = -1;
            Response.AddHeader("pragma", "no-cache");

            RequireSecurePage();

            SectionTitle = AppLogic.GetString("checkoutanon.aspx.1", SkinID, ThisCustomer.LocaleSetting);
            checkouttype = CommonLogic.QueryStringCanBeDangerousContent("checkouttype");

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

            if (DisablePasswordAutocomplete)
            {
                AppLogic.DisableAutocomplete(Password);
                AppLogic.DisableAutocomplete(OldPassword);
                AppLogic.DisableAutocomplete(NewPassword);
                AppLogic.DisableAutocomplete(NewPassword2);
            }

            cart = new ShoppingCart(SkinID, ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);

            cart.ValidProceedCheckout(); // will not come back from this if any issue. they are sent back to the cart page!

            if (AppLogic.AppConfigBool("PasswordIsOptionalDuringCheckout") && !cart.HasRecurringComponents())
            {
                PasswordOptionalPanel.Visible = true;
            }

            ErrorMsgLabel.Text = "";
            if (!IsPostBack)
            {
                InitializePageContent();
                
            }
            if (AppLogic.AppConfigBool("SecurityCodeRequiredOnStoreLogin"))
            {
                // Create a random code and store it in the Session object.
                pnlSecurity.Visible = true;
                SecurityImage.Visible = true;
                SecurityCode2.Visible = true;
                RequiredFieldValidator9.Enabled = true;
                ltSecurity.Visible = true;
                if (!IsPostBack)
                    SecurityImage.ImageUrl = "Captcha.ashx?id=1";
                else
                    SecurityImage.ImageUrl = "Captcha.ashx?id=2";
                
            }
        }

        private void InitializePageContent()
        {
            JSPopupRoutines.Text = AppLogic.GetJSPopupRoutines();
            if (AppLogic.AppConfigBool("SecurityCodeRequiredOnStoreLogin"))
            {
                // Create a random code and store it in the Session object.
                Session["SecurityCode"] = CommonLogic.GenerateRandomCode(6);
            }
        }        

        protected void btnSignInAndCheckout_Click(object sender, EventArgs e)
        {
            Page.Validate();
            if (Page.IsValid)
            {

                String EMailField = EMail.Text.ToLowerInvariant().Trim();
                String PasswordField = Password.Text;

                bool LoginOK = false;

                if (AppLogic.AppConfigBool("SecurityCodeRequiredOnStoreLogin"))
                {
                    String sCode = Session["SecurityCode"].ToString();
                    String fCode = SecurityCode2.Text;
                    if (fCode != sCode)
                    {
                        ErrorMsgLabel.Text = string.Format(AppLogic.GetString("lat_signin_process.aspx.5", SkinID, ThisCustomer.LocaleSetting), sCode, fCode);
                        ErrorPanel.Visible = true;
                        SecurityCode2.Text = String.Empty;
                        Session["SecurityCode"] = CommonLogic.GenerateRandomCode(6);
                        return;
                    }
                }               

                Customer c = new Customer(EMailField, true);
                LoginOK = c.CheckLogin(PasswordField);


                if (!c.IsRegistered)
                {
                    ErrorMsgLabel.Text = AppLogic.GetString("lat_signin_process.aspx.1", SkinID, ThisCustomer.LocaleSetting);
                    ErrorPanel.Visible = true;
                    return;
                }
                else
                {
                    if (LoginOK)
                    {
                        if (c.BadLoginCount >= AppLogic.AppConfigNativeInt("MaxBadLogins"))
                        {
                            ErrorMsgLabel.Text = AppLogic.GetString("lat_signin_process.aspx.3", SkinID, ThisCustomer.LocaleSetting);
                            ErrorPanel.Visible = true;
                            return;
                        }
                        else if (!c.Active)
                        {
                            ErrorMsgLabel.Text = AppLogic.GetString("lat_signin_process.aspx.2", SkinID, ThisCustomer.LocaleSetting);
                            ErrorPanel.Visible = true;
                            return;
                        }
                        else if (c.PwdChangeRequired)
                        {
                            ExecutePanel.Visible = false;
                            FormPanel.Visible = false;
                            pnlChangePwd.Visible = true;
                            CustomerEmail.Text = EMailField;
                            OldPassword.Focus();
                            return;
                        }
                        int CurrentCustomerID = ThisCustomer.CustomerID;
                        int NewCustomerID = c.CustomerID;

                        if (AppLogic.AppConfigBool("DynamicRelatedProducts.Enabled") || AppLogic.AppConfigBool("RecentlyViewedProducts.Enabled"))
                        {
                            c.ReplaceProductViewFromAnonymous();
                        }

                        AppLogic.ExecuteSigninLogic(CurrentCustomerID, NewCustomerID);

                        // update the cookie value if present for affiliate
                        int affiliateIDFromCookie = int.Parse(CommonLogic.IIF(CommonLogic.IsInteger(HttpContext.Current.Profile.GetPropertyValue(Customer.ro_AffiliateCookieName).ToString()), HttpContext.Current.Profile.GetPropertyValue(Customer.ro_AffiliateCookieName).ToString(), "0"));

                        if (AppLogic.IsValidAffiliate(affiliateIDFromCookie))
                        {
                            c.AffiliateID = affiliateIDFromCookie;
                        }

                        if (c.IsAdminUser)
                        {
                            Security.LogEvent("Store Login", "", c.CustomerID, c.CustomerID, c.ThisCustomerSession.SessionID);
                        }

                        // we've got a good login:
                        FormPanel.Visible = false;
                        ExecutePanel.Visible = true;

                        String CustomerGUID = c.CustomerGUID.Replace("{", "").Replace("}", "");

                        SignInExecuteLabel.Text = AppLogic.GetString("signin.aspx.2", SkinID, ThisCustomer.LocaleSetting);

                        // set the Secure property if the site has SSL enabled and is on live
                        FormsAuthentication.SetAuthCookie(CustomerGUID, true);

                        HttpCookie authCookie = Response.Cookies[FormsAuthentication.FormsCookieName];
                        if (authCookie != null && !AppLogic.AppConfigBool("GoNonSecureAgain"))
                        {
                            authCookie.Secure = AppLogic.UseSSL() && AppLogic.OnLiveServer();
                        }

                        if (!AppLogic.AppConfigBool("Checkout.RedirectToCartOnSignin"))
                        {
                            ShoppingCart newCart = new ShoppingCart(SkinID, c, CartTypeEnum.ShoppingCart, 0, false);
                            string returnURL = newCart.PageToBeginCheckout(false, false);
                            if (newCart.Total(true) != cart.Total(true))
                            {
								ErrorMessage em = new ErrorMessage("checkoutshipping.aspx.25".StringResource());
								returnURL = returnURL.AppendQueryString("errormsg=" + em.MessageId);
                            }
                            Response.AddHeader("REFRESH", "1; URL=" + Server.UrlDecode(returnURL));
                        }
                        else
                            Response.AddHeader("REFRESH", "1; URL=" + Server.UrlDecode("shoppingcart.aspx"));
                    }
                    else
                    {
                        ErrorMsgLabel.Text = AppLogic.GetString("lat_signin_process.aspx.1", SkinID, ThisCustomer.LocaleSetting);
                        ErrorPanel.Visible = true;
                        if (c.IsAdminUser)
                        {
                            c.UpdateCustomer(
                                /*CustomerLevelID*/ null,
                                /*EMail*/ null,
                                /*SaltedAndHashedPassword*/ null,
                                /*SaltKey*/ null,
                                /*DateOfBirth*/ null,
                                /*Gender*/ null,
                                /*FirstName*/ null,
                                /*LastName*/ null,
                                /*Notes*/ null,
                                /*SkinID*/ null,
                                /*Phone*/ null,
                                /*AffiliateID*/ null,
                                /*Referrer*/ null,
                                /*CouponCode*/ null,
                                /*OkToEmail*/ null,
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
                                /*Over13Checked*/ null,
                                /*CurrencySetting*/ null,
                                /*VATSetting*/ null,
                                /*VATRegistrationID*/ null,
                                /*StoreCCInDB*/ null,
                                /*IsRegistered*/ null,
                                /*LockedUntil*/ null,
                                /*AdminCanViewCC*/ null,
                                /*BadLogin*/ 1,
                                /*Active*/ null,
                                /*PwdChangeRequired*/ null,
                                /*RegisterDate*/ null,
                                /*StoreId*/null
                                 );

                        }
                        Security.LogEvent("Store Login Failed", "Attempted login failed for email address " + EMailField, 0, 0, 0);
                        return;
                    }
                }
            }
        }
        
        protected void RegisterAndCheckoutButton_Click(object sender, EventArgs e)
        {
            Response.Redirect("createaccount.aspx?checkout=true&checkouttype=" + checkouttype);
        }

        protected void Skipregistration_Click(object sender, EventArgs e)
        {
            Response.Redirect("createaccount.aspx?checkout=true&skipreg=true");
        }

        protected void btnChgPwd_Click(object sender, EventArgs e)
        {
            String EMailField = CustomerEmail.Text.ToLowerInvariant();
            String PasswordField = OldPassword.Text;
            String newpwd = NewPassword.Text;
            String confirmpwd = NewPassword2.Text;
            String newencryptedpwd = "";
            lblPwdChgErr.Text = "";
            lblPwdChgErr.Visible = false;

            bool LoginOK = false;

            c = new Customer(EMailField, true);
            Password pwdold = new Password(PasswordField, c.SaltKey);
            Password pwdnew = new Password(newpwd, c.SaltKey);
            if (c.IsRegistered)
            {
                newencryptedpwd = pwdnew.SaltedPassword;
                LoginOK = (c.Password == pwdold.SaltedPassword);
                if (LoginOK)
                {
                    if (PasswordField == newpwd)
                    {
                        lblPwdChgErr.Text = AppLogic.GetString("signin.aspx.30", SkinID, ThisCustomer.LocaleSetting);
                        return;
                    }

                    if (newpwd != confirmpwd)
                    {
                        lblPwdChgErr.Text = AppLogic.GetString("signin.aspx.32", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                        return;
                    }

                    if ((c.IsAdminUser || c.IsAdminSuperUser) && c.PwdPreviouslyUsed(newpwd))
                    {
                        lblPwdChgErr.Text = String.Format(AppLogic.GetString("signin.aspx.31", SkinID, ThisCustomer.LocaleSetting), AppLogic.AppConfig("NumPreviouslyUsedPwds"));
                        lblPwdChgErr.Visible = true;
                        return;
                    }

                    if (c.BadLoginCount >= AppLogic.AppConfigNativeInt("MaxBadLogins"))
                    {
                        lblPwdChgErr.Text = "<br/><br/>" + AppLogic.GetString("lat_signin_process.aspx.3", SkinID, ThisCustomer.LocaleSetting);
                        lblPwdChgErr.Visible = true;
                        return;
                    }

                    if (!c.Active)
                    {
                        lblPwdChgErr.Text = "<br/><br/>" + AppLogic.GetString("lat_signin_process.aspx.2", SkinID, ThisCustomer.LocaleSetting);
                        lblPwdChgErr.Visible = true;
                        return;
                    }
                    if (c.IsAdminUser || AppLogic.AppConfigBool("UseStrongPwd"))
                    {
                        if (!Regex.IsMatch(newpwd, AppLogic.AppConfig("CustomerPwdValidator"), RegexOptions.Compiled))
                        {
                            lblPwdChgErr.Text = AppLogic.GetString("signin.aspx.26", SkinID, ThisCustomer.LocaleSetting);
                            lblPwdChgErr.Visible = true;
                            return;
                        }
                    }

                    if (!c.IsAdminUser && !ValidateNewPassword())
                    {
                        lblPwdChgErr.Visible = true;
                        return;
                    }


                    if (c.IsAdminUser)
                    {
                        Security.LogEvent("Admin Password Changed", "", c.CustomerID, c.CustomerID, 0);
                    }

                    c.UpdateCustomer(
                        /*CustomerLevelID*/ null,
                        /*EMail*/ null,
                        /*SaltedAndHashedPassword*/ pwdnew.SaltedPassword,
                        /*SaltKey*/ pwdnew.Salt,
                        /*DateOfBirth*/ null,
                        /*Gender*/ null,
                        /*FirstName*/ null,
                        /*LastName*/ null,
                        /*Notes*/ null,
                        /*SkinID*/ null,
                        /*Phone*/ null,
                        /*AffiliateID*/ null,
                        /*Referrer*/ null,
                        /*CouponCode*/ null,
                        /*OkToEmail*/ null,
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
                        /*Over13Checked*/ null,
                        /*CurrencySetting*/ null,
                        /*VATSetting*/ null,
                        /*VATRegistrationID*/ null,
                        /*StoreCCInDB*/ null,
                        /*IsRegistered*/ null,
                        /*LockedUntil*/ null,
                        /*AdminCanViewCC*/ null,
                        /*BadLogin*/ -1,
                        /*Active*/ null,
                        /*PwdChangeRequired*/ 0,
                        /*RegisterDate*/ null,
                        /*StoreId*/null
                         );
                    FormPanel.Visible = false;
                    ExecutePanel.Visible = true;
                    pnlChangePwd.Visible = false;

                    AppLogic.ExecuteSigninLogic(ThisCustomer.CustomerID, c.CustomerID);

                    String CustomerGUID = c.CustomerGUID.Replace("{", "").Replace("}", "");

                    SignInExecuteLabel.Text = AppLogic.GetString("signin.aspx.24", SkinID, ThisCustomer.LocaleSetting);

                    string sReturnURL = string.Empty;

                    if (CheckingOut)
                    {
                        sReturnURL = "shoppingcart.aspx";
                    }
                    else
                    {
                        sReturnURL = FormsAuthentication.GetRedirectUrl(CustomerGUID, true);
                    }
                    FormsAuthentication.SetAuthCookie(CustomerGUID, true);

                    if (sReturnURL.Length == 0)
                    {
                        sReturnURL = FormsAuthentication.GetRedirectUrl(CustomerGUID, false);
                    }
                    Response.AddHeader("REFRESH", "1; URL=" + Server.UrlDecode(sReturnURL));
                }
                else
                {
                    lblPwdChgErr.Text += "<br/>" + AppLogic.GetString("lat_signin_process.aspx.1", SkinID, ThisCustomer.LocaleSetting);
                    lblPwdChgErr.Visible = true;
                    if (c.IsAdminUser)
                    {
                        c.UpdateCustomer(
                            /*CustomerLevelID*/ null,
                            /*EMail*/ null,
                            /*SaltedAndHashedPassword*/ null,
                            /*SaltKey*/ null,
                            /*DateOfBirth*/ null,
                            /*Gender*/ null,
                            /*FirstName*/ null,
                            /*LastName*/ null,
                            /*Notes*/ null,
                            /*SkinID*/ null,
                            /*Phone*/ null,
                            /*AffiliateID*/ null,
                            /*Referrer*/ null,
                            /*CouponCode*/ null,
                            /*OkToEmail*/ null,
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
                            /*Over13Checked*/ null,
                            /*CurrencySetting*/ null,
                            /*VATSetting*/ null,
                            /*VATRegistrationID*/ null,
                            /*StoreCCInDB*/ null,
                            /*IsRegistered*/ null,
                            /*LockedUntil*/ null,
                            /*AdminCanViewCC*/ null,
                            /*BadLogin*/ 1,
                            /*Active*/ null,
                            /*PwdChangeRequired*/ null,
                            /*RegisterDate*/ null,
                            /*StoreId*/null
                            );
                    }
                    return;
                }

            }
            else
            {
                lblPwdChgErr.Text = "<br/><br/>" + AppLogic.GetString("lat_signin_process.aspx.1", SkinID, ThisCustomer.LocaleSetting);
                lblPwdChgErr.Visible = true;
                return;
            }
        }

        private bool ValidateNewPassword()
        {
            bool IsValid = false;
            string err = string.Empty;
            if (NewPassword.Text.Replace("*", "").Trim().Length == 0)
            {
                err = "<br/><br/>" + AppLogic.GetString("account.aspx.7", SkinID, ThisCustomer.LocaleSetting);
                IsValid = false;
            }
            else if (NewPassword.Text == NewPassword2.Text)
            {
                try
                {
                    if (AppLogic.AppConfigBool("UseStrongPwd") || ThisCustomer.IsAdminUser)
                    {
                        IsValid = Regex.IsMatch(NewPassword.Text, AppLogic.AppConfig("CustomerPwdValidator"), RegexOptions.Compiled);
                    }
                    else
                    {
                        IsValid = (NewPassword.Text.Length > 4);
                    }
                    if (!IsValid)
                    {
                        err = "<br/><br/>" + AppLogic.GetString("account.aspx.7", SkinID, ThisCustomer.LocaleSetting);
                    }
                }
                catch
                {
                    AppLogic.SendMail("Invalid Password Validation Pattern", "", false, AppLogic.AppConfig("MailMe_ToAddress"), AppLogic.AppConfig("MailMe_ToAddress"), AppLogic.AppConfig("MailMe_ToAddress"), AppLogic.AppConfig("MailMe_ToAddress"), "", "", AppLogic.MailServer());
                    throw new Exception("Password validation expression is invalid, please notify site administrator");
                }
            }
            else
            {
                err = "<br/><br/>" + AppLogic.GetString("signin.aspx.32", SkinID, ThisCustomer.LocaleSetting);
            }
            lblPwdChgErr.Text = err;
            return IsValid;
        }


        /// <summary>
        /// Wrapper for querystring["checkout"]
        /// </summary>
        private bool CheckingOut
        {
            get
            {
                try
                {
                    return CommonLogic.QueryStringCanBeDangerousContent("checkout") == "true";
                }
                catch { }
                return false;
            }
        }
    }
}
