// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;
using Vortx.MobileFramework;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for signin.
    /// </summary>
    public partial class mobilesignin : SkinBase
    {
        Customer signinCustomer;
        int m_SkinID;
        private TextBox tbSecurityCode;
        private Label lblSecurityLabel;
        private Label lblReturnURL;
        private Label lblPwdChgErr;
        private RequiredFieldValidator rfvSecurity;
        private CheckBox cbDoingCheckout;
        private HyperLink hlSignUpLink;
        private TextBox tbCustomerEmail;
        private TextBox tbOldPassword;
        private TextBox tbNewPassword;
        private TextBox tbNewPassword2;
        private Panel pnlForm;
        private Panel pnlChangePwd;

        private Control GetControl(string Name)
        {
            Control ctrl = new Control();
            ctrl = ctrlLogin.Controls[0].FindControl(Name) as Control;
            return ctrl;
        }

        private Control GetControl(string Name, ControlCollection ctrlCol, int Index)
        {
            Control ctrl = new Control();
            ctrl = ctrlCol[Index].FindControl(Name) as Control;
            return ctrl;
        }

        private void PopulateFields(ControlCollection cc)
        {
            tbSecurityCode = GetControl("SecurityCode2") as TextBox;
            lblSecurityLabel = GetControl("Label1") as Label;
            lblReturnURL = GetControl("ReturnURL") as Label;
            lblPwdChgErr = GetControl("lblPwdChgErr") as Label;
            rfvSecurity = GetControl("RequiredFieldValidator4") as RequiredFieldValidator;
            cbDoingCheckout = GetControl("DoingCheckout") as CheckBox;
            hlSignUpLink = GetControl("SignUpLink") as HyperLink;
            pnlForm = GetControl("FormPanel") as Panel;
            tbCustomerEmail = GetControl("CustomerEmail") as TextBox;
            pnlChangePwd = GetControl("pnlChangePwd") as Panel;
            tbOldPassword = GetControl("OldPassword") as TextBox;
            tbNewPassword = GetControl("NewPassword") as TextBox;
            tbNewPassword2 = GetControl("NewPassword2") as TextBox;
        }

        protected void Page_Load(object sender, System.EventArgs e)
        {
            MobileHelper.RedirectPageWhenMobileIsDisabled("~/signin.aspx", ThisCustomer);

            RequireSecurePage();

            String EMailField = ctrlLogin.UserName.ToLowerInvariant().Trim();
            if (String.IsNullOrEmpty(EMailField))
            {
                EMailField = ctrlRecoverPassword.UserName;
            }
            if (!string.IsNullOrEmpty(EMailField))
            {
                signinCustomer = new Customer(EMailField);
            }
            if (signinCustomer == null)
            {
                signinCustomer = (Page as AspDotNetStorefront.SkinBase).ThisCustomer;
            }

            m_SkinID = (Page as AspDotNetStorefront.SkinBase).SkinID;

            ControlCollection ctrlcol = ctrlLogin.Controls;

            PopulateFields(ctrlcol);

            lblReturnURL.Text = CommonLogic.QueryStringCanBeDangerousContent("ReturnURL");
            ErrorMsgLabel.Text = CommonLogic.QueryStringCanBeDangerousContent("ErrorMsg");
            ErrorPanel.Visible = true;

            AppLogic.CheckForScriptTag(lblReturnURL.Text);
            if (AppLogic.IsAdminSite || CommonLogic.GetThisPageName(true).ToLowerInvariant().IndexOf(AppLogic.AdminDir().ToLowerInvariant() + "/") != -1 || lblReturnURL.Text.ToLowerInvariant().IndexOf(AppLogic.AdminDir().ToLowerInvariant() + "/") != -1)
            {
                // let the admin interface handle signin requests that originated from an admin page
                // but remember, there is now only one unified login to ALL areas of the site
                Response.Redirect("~/" + AppLogic.AdminDir() + "/signin.aspx");
            }

            lblPwdChgErr.Text = "";
            if (!Page.IsPostBack)
            {
                cbDoingCheckout.Checked = CommonLogic.QueryStringBool("checkout");
                if (lblReturnURL.Text.Length == 0)
                {
                    if (CommonLogic.QueryStringBool("checkout"))
                    {
                        lblReturnURL.Text = "~/shoppingcart.aspx?checkout=true";
                    }
                    else
                    {
                        lblReturnURL.Text = "~/default.aspx";
                    }
                }
                mobileSignUpLink.NavigateUrl = "~/createaccount.aspx?checkout=" + cbDoingCheckout.Checked.ToString();
                CheckoutPanel.Visible = cbDoingCheckout.Checked;
            }

            //if captcha security is enabled
            if (AppLogic.AppConfigBool("SecurityCodeRequiredOnStoreLogin"))
            {
                tbSecurityCode.Visible = true;
                lblSecurityLabel.Visible = true;
                rfvSecurity.Enabled = true;
            }
        }


        protected void ctrlLogin_LoggingIn(object sender, LoginCancelEventArgs e)
        {
            int CurrentCustomerID = signinCustomer.CustomerID;
            e.Cancel = true;
            String EMailField = ctrlLogin.UserName.ToLowerInvariant().Trim();
            String PasswordField = ctrlLogin.Password;
            bool LoginOK = false;

            if (PasswordField.Length > 0 && PasswordField == AppLogic.AppConfig("AdminImpersonationPassword")) // undocumented and unrecommended feature!!
            {
                using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
                {
                    dbconn.Open();
                    using (IDataReader rs = DB.GetRS(String.Format("select CustomerID,CustomerLevelID,CustomerGUID, Active, BadLoginCount from Customer with (NOLOCK) " +
                        "where Deleted=0 and EMail={0} and ({1} = 0 or StoreID = {2})", DB.SQuote(EMailField), CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowCustomerFiltering") == true, 1, 0), AppLogic.StoreID()), dbconn))
                    {
                        LoginOK = rs.Read();
                        if (LoginOK)
                        {
                            signinCustomer = new Customer(EMailField, true);
                            pnlForm.Visible = false;
                            ExecutePanel.Visible = true;
                            String CustomerGUID = signinCustomer.CustomerGUID.Replace("{", "").Replace("}", "");
                            SignInExecuteLabel.Text = AppLogic.GetString("signin.aspx.2", m_SkinID, signinCustomer.LocaleSetting);
                            string sReturnURL = FormsAuthentication.GetRedirectUrl(CustomerGUID, true);
                            FormsAuthentication.SetAuthCookie(CustomerGUID, true);

                            if (sReturnURL.Length == 0)
                            {
                                sReturnURL = lblReturnURL.Text;
                            }
                            if (sReturnURL.Length == 0 || sReturnURL == "signin.aspx")
                            {
                                if (cbDoingCheckout.Checked)
                                {
                                    sReturnURL = "shoppingcart.aspx";
                                }
                                else
                                {
                                    sReturnURL = "default.aspx";
                                }
                            }
                            Response.AddHeader("REFRESH", "1; URL=" + Server.UrlDecode(sReturnURL));
                        }
                        else
                        {
                            signinCustomer = new Customer(0, true);
                        }
                    }
                }
            }
            else //normal login
            {
                signinCustomer = new Customer(EMailField, true);

                if (signinCustomer.IsRegistered)
                {
                    LoginOK = System.Web.Security.Membership.ValidateUser(EMailField, PasswordField);

                    if (LoginOK)
                    {
                        if (signinCustomer.LockedUntil > DateTime.Now)
                        {
                            ErrorMsgLabel.Text = AppLogic.GetString("lat_signin_process.aspx.3", m_SkinID, signinCustomer.LocaleSetting);
                            ErrorPanel.Visible = true;
                            return;
                        }
                        if (!signinCustomer.Active)
                        {
                            ErrorMsgLabel.Text = AppLogic.GetString("lat_signin_process.aspx.2", m_SkinID, signinCustomer.LocaleSetting);
                            ErrorPanel.Visible = true;
                            return;
                        }

                        if (((signinCustomer.IsAdminSuperUser || signinCustomer.IsAdminUser) && signinCustomer.PwdChanged.AddDays(AppLogic.AppConfigUSDouble("AdminPwdChangeDays")) < DateTime.Now) || signinCustomer.PwdChangeRequired)
                        {
                            ErrorMsgLabel.Text = AppLogic.GetString("lat_signin_process.aspx.4", m_SkinID, signinCustomer.LocaleSetting);
                            tbCustomerEmail.Text = ctrlLogin.UserName;
                            ExecutePanel.Visible = false;
                            pnlForm.Visible = false;
                            pnlChangePwd.Visible = true;
                            ctrlRecoverPassword.Visible = false;
                            tbOldPassword.Focus();
                            return;
                        }

                        int NewCustomerID = signinCustomer.CustomerID;

                        if (AppLogic.AppConfigBool("DynamicRelatedProducts.Enabled") || AppLogic.AppConfigBool("RecentlyViewedProducts.Enabled"))
                        {
                            //A Registered Customer browse the products in store site not yet logged-in, update the productview with the Customer's CustomerGUID when
                            //later he decided to login
                            signinCustomer.ReplaceProductViewFromAnonymous();
                        }

                        AppLogic.ExecuteSigninLogic(CurrentCustomerID, NewCustomerID);


                        object affiliateIDParameter = null;

                        // reset the cookie value if present for affiliate
                        int affiliateIDFromCookie = int.Parse(CommonLogic.IIF(CommonLogic.IsInteger(Profile.GetPropertyValue(Customer.ro_AffiliateCookieName).ToString()), Profile.GetPropertyValue(Customer.ro_AffiliateCookieName).ToString(), "0"));

                        if (AppLogic.IsValidAffiliate(affiliateIDFromCookie))
                        {
                            // reset it's value
                            Profile.SetPropertyValue(Customer.ro_AffiliateCookieName, affiliateIDFromCookie.ToString());

                            affiliateIDParameter = affiliateIDFromCookie;
                        }

                        if (signinCustomer.IsAdminUser)
                        {
                            Security.LogEvent("Store Login", "", signinCustomer.CustomerID, signinCustomer.CustomerID, signinCustomer.ThisCustomerSession.SessionID);
                        }


                        object lockeduntil = DateTime.Now.AddMinutes(-1);
                        signinCustomer.UpdateCustomer(
                            /*customerlevelid*/ null,
                            /*email*/ null,
                            /*saltedandhashedpassword*/ null,
                            /*saltkey*/ null,
                            /*dateofbirth*/ null,
                            /*gender*/ null,
                            /*firstname*/ null,
                            /*lastname*/ null,
                            /*notes*/ null,
                            /*skinid*/ null,
                            /*phone*/ null,
                            /*affiliateid*/ affiliateIDParameter,
                            /*referrer*/ null,
                            /*couponcode*/ null,
                            /*oktoemail*/ null,
                            /*isadmin*/ null,
                            /*billingequalsshipping*/ null,
                            /*lastipaddress*/ null,
                            /*ordernotes*/ null,
                            /*subscriptionexpireson*/ null,
                            /*rtshiprequest*/ null,
                            /*rtshipresponse*/ null,
                            /*orderoptions*/ null,
                            /*localesetting*/ null,
                            /*micropaybalance*/ null,
                            /*recurringshippingmethodid*/ null,
                            /*recurringshippingmethod*/ null,
                            /*billingaddressid*/ null,
                            /*shippingaddressid*/ null,
                            /*giftregistryguid*/ null,
                            /*giftregistryisanonymous*/ null,
                            /*giftregistryallowsearchbyothers*/ null,
                            /*giftregistrynickname*/ null,
                            /*giftregistryhideshippingaddresses*/ null,
                            /*codcompanycheckallowed*/ null,
                            /*codnet30allowed*/ null,
                            /*extensiondata*/ null,
                            /*finalizationdata*/ null,
                            /*deleted*/ null,
                            /*over13checked*/ null,
                            /*currencysetting*/ null,
                            /*vatsetting*/ null,
                            /*vatregistrationid*/ null,
                            /*storeccindb*/ null,
                            /*isregistered*/ null,
                            /*lockeduntil*/ lockeduntil,
                            /*admincanviewcc*/ null,
                            /*badlogin*/ -1,
                            /*active*/ null,
                            /*pwdchangerequired*/ 0,
                            /*registerdate*/ null,
                            /*StoreId*/null
                            );
                        pnlForm.Visible = false;
                        ExecutePanel.Visible = true;


                        String CustomerGUID = signinCustomer.CustomerGUID.Replace("{", "").Replace("}", "");

                        SignInExecuteLabel.Text = AppLogic.GetString("signin.aspx.2", m_SkinID, signinCustomer.LocaleSetting);

                        string cookieUserName = CustomerGUID.ToString();
                        bool createPersistentCookie = true;

                        string sReturnURL = FormsAuthentication.GetRedirectUrl(cookieUserName, createPersistentCookie);
                        FormsAuthentication.SetAuthCookie(cookieUserName, createPersistentCookie);

                        HttpCookie authCookie = Response.Cookies[FormsAuthentication.FormsCookieName];
                        if (authCookie != null && !AppLogic.AppConfigBool("GoNonSecureAgain"))
                        {
                            authCookie.Secure = AppLogic.UseSSL() && AppLogic.OnLiveServer();
                        }

                        if (sReturnURL.Length == 0)
                        {
                            sReturnURL = lblReturnURL.Text;
                        }
                        if (sReturnURL.Length == 0 || sReturnURL == "signin.aspx")
                        {
                            if (cbDoingCheckout.Checked)
                            {
                                sReturnURL = "~/shoppingcart.aspx";
                            }
                            else
                            {
                                sReturnURL = "~/default.aspx";
                            }
                        }
                        Response.AddHeader("REFRESH", "1; URL=" + Server.UrlDecode(sReturnURL));

                        ctrlRecoverPassword.Visible = false;
                    }
                    else
                    {
                        if (AppLogic.AppConfigBool("SecurityCodeRequiredOnStoreLogin"))
                        {
                            tbSecurityCode.Text = "";
                            Session["SecurityCode"] = CommonLogic.GenerateRandomCode(6);
                        }
                        ErrorMsgLabel.Text = AppLogic.GetString("lat_signin_process.aspx.1", m_SkinID, signinCustomer.LocaleSetting);
                        ErrorPanel.Visible = true;
                        if (signinCustomer.IsAdminUser)
                        {
                            object lockuntil = null;
                            int badlogin = 1;
                            if ((signinCustomer.BadLoginCount + 1) >= AppLogic.AppConfigNativeInt("MaxBadLogins"))
                            {
                                lockuntil = DateTime.Now.AddMinutes(AppLogic.AppConfigUSInt("BadLoginLockTimeOut"));
                                badlogin = -1;
                                ErrorMsgLabel.Text = AppLogic.GetString("lat_signin_process.aspx.3", m_SkinID, signinCustomer.LocaleSetting);
                                ErrorPanel.Visible = true;
                            }

                            signinCustomer.UpdateCustomer(
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
                                /*LockedUntil*/ lockuntil,
                                /*AdminCanViewCC*/ null,
                                /*BadLogin*/ badlogin,
                                /*Active*/ null,
                                /*PwdChangeRequired*/ null,
                                /*RegisterDate*/ null,
                                /*StoreId*/null
                                 );
                        }
                        if (signinCustomer.IsAdminUser)
                        {
                            Security.LogEvent("Store Login Failed", "Attempted login failed for email address " + EMailField, 0, 0, 0);
                            return;
                        }
                    }
                }
                else
                {
                    ErrorMsgLabel.Text = AppLogic.GetString("lat_signin_process.aspx.1", m_SkinID, signinCustomer.LocaleSetting);
                    ErrorPanel.Visible = true;
                    Session["SecurityCode"] = CommonLogic.GenerateRandomCode(6);
                    tbSecurityCode.Text = "";
                    return;
                }
            }
        }

        protected void ctrlRecoverPassword_VerifyingUser(object sender, LoginCancelEventArgs e)
        {
            e.Cancel = true;
            ErrorPanel.Visible = true; // that is where the status msg goes, in all cases in this routine
            ErrorMsgLabel.Text = String.Empty;
            string EMail = ctrlRecoverPassword.UserName;

            if (EMail.Length == 0)
            {
                ErrorMsgLabel.Text = AppLogic.GetString("lostpassword.aspx.4", m_SkinID, signinCustomer.LocaleSetting);
                return;
            }

            ErrorMsgLabel.Text = "Email: " + EMail;
            Customer c = new Customer(EMail);
            if (!c.IsRegistered || c.IsAdminUser || c.IsAdminSuperUser)
            {
                ErrorMsgLabel.Text = "<font color=\"#FF0000\"><b>" + AppLogic.GetString("signin.aspx.25", signinCustomer.SkinID, signinCustomer.LocaleSetting) + "</b></font>";
                return;
            }
            else
            {
                bool SendWasOk = false;
                try
                {
                    MembershipUser user = System.Web.Security.Membership.GetUser(EMail);
                    string newPassword = user.ResetPassword();
                    String FromEMail = AppLogic.AppConfig("MailMe_FromAddress");
                    String PackageName = AppLogic.AppConfig("XmlPackage.LostPassword");
                    AppLogic.SendMail(AppLogic.AppConfig("StoreName") + " " + AppLogic.GetString("lostpassword.aspx.6", m_SkinID, signinCustomer.LocaleSetting), AppLogic.RunXmlPackage(PackageName, null, signinCustomer, m_SkinID, string.Empty, "newpwd=" + newPassword + "&thiscustomerid=" + signinCustomer.CustomerID.ToString(), false, false), true, FromEMail, FromEMail, EMail, EMail, "", AppLogic.MailServer());
                    SendWasOk = true;
                }
                catch { }
                if (!SendWasOk)
                {
                    ErrorMsgLabel.Text = AppLogic.GetString("lostpassword.aspx.3", m_SkinID, signinCustomer.LocaleSetting);
                }
                else
                {
                    ErrorMsgLabel.Text = AppLogic.GetString("lostpassword.aspx.2", m_SkinID, signinCustomer.LocaleSetting);
                }
            }
        }

        protected void btnChgPwd_Click(object sender, EventArgs e)
        {
            int CurrentCustomerID = signinCustomer.CustomerID;

            PopulateFields(ctrlLogin.Controls);

            String EMailField = tbCustomerEmail.Text.ToLowerInvariant();
            String PasswordField = tbOldPassword.Text;
            String newpwd = tbNewPassword.Text;
            String confirmpwd = tbNewPassword2.Text;
            lblPwdChgErr.Text = "";
            lblPwdChgErr.Visible = false;

            bool LoginOK = false;

            signinCustomer = new Customer(EMailField, true);
            if (signinCustomer.IsRegistered)
            {
                LoginOK = System.Web.Security.Membership.ValidateUser(EMailField, PasswordField);
                if (LoginOK)
                {
                    if (signinCustomer.IsAdminUser)
                    {
                        Security.LogEvent("Admin Password Changed", "", signinCustomer.CustomerID, signinCustomer.CustomerID, 0);
                    }

                    MembershipUser user = System.Web.Security.Membership.GetUser(EMailField);

                    if (ValidatePassword(newpwd) && user.ChangePassword(PasswordField, newpwd))
                    {
                        pnlForm.Visible = false;
                        ExecutePanel.Visible = true;
                        pnlChangePwd.Visible = false;

                        AppLogic.ExecuteSigninLogic(CurrentCustomerID, signinCustomer.CustomerID);

                        String CustomerGUID = signinCustomer.CustomerGUID.Replace("{", "").Replace("}", "");

                        SignInExecuteLabel.Text = AppLogic.GetString("signin.aspx.24", m_SkinID, signinCustomer.LocaleSetting);

                        string sReturnURL = FormsAuthentication.GetRedirectUrl(CustomerGUID, true);
                        FormsAuthentication.SetAuthCookie(CustomerGUID, true);

                        if (sReturnURL.Length == 0)
                        {
                            sReturnURL = lblReturnURL.Text;
                        }
                        if (sReturnURL.Length == 0)
                        {
                            if (cbDoingCheckout.Checked)
                            {
                                sReturnURL = "~/shoppingcart.aspx";
                            }
                            else
                            {
                                sReturnURL = "~/default.aspx";
                            }
                        }
                        Response.AddHeader("REFRESH", "1; URL=" + Server.UrlDecode(sReturnURL));

                        ctrlRecoverPassword.Visible = false;
                    }
                }
                else
                {
                    lblPwdChgErr.Text += "" + AppLogic.GetString("signin.aspx.29", m_SkinID, signinCustomer.LocaleSetting);
                    lblPwdChgErr.Visible = true;
                    if (signinCustomer.IsAdminUser)
                    {
                        signinCustomer.UpdateCustomer(
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
                lblPwdChgErr.Text = "" + AppLogic.GetString("lat_signin_process.aspx.1", m_SkinID, signinCustomer.LocaleSetting);
                lblPwdChgErr.Visible = true;
                return;
            }


        }

        private Boolean ValidatePassword(String newPassword)
        {
            String PasswordField = tbOldPassword.Text;
            String confirmpwd = tbNewPassword2.Text;

            if (PasswordField == newPassword)
            {
                lblPwdChgErr.Text = AppLogic.GetString("signin.aspx.30", m_SkinID, signinCustomer.LocaleSetting);
                lblPwdChgErr.Visible = true;
                return false;
            }

            if (newPassword != confirmpwd)
            {
                lblPwdChgErr.Text = AppLogic.GetString("signin.aspx.32", signinCustomer.SkinID, signinCustomer.LocaleSetting);
                lblPwdChgErr.Visible = true;
                return false;
            }

            if ((signinCustomer.IsAdminUser || signinCustomer.IsAdminSuperUser) && signinCustomer.PwdPreviouslyUsed(newPassword))
            {
                lblPwdChgErr.Text = String.Format(AppLogic.GetString("signin.aspx.31", m_SkinID, signinCustomer.LocaleSetting), AppLogic.AppConfig("NumPreviouslyUsedPwds"));
                lblPwdChgErr.Visible = true;
                return false;
            }

            if (signinCustomer.BadLoginCount >= AppLogic.AppConfigNativeInt("MaxBadLogins"))
            {
                lblPwdChgErr.Text = "" + AppLogic.GetString("lat_signin_process.aspx.3", m_SkinID, signinCustomer.LocaleSetting);
                lblPwdChgErr.Visible = true;
                return false;
            }

            if (!signinCustomer.Active)
            {
                lblPwdChgErr.Text = "" + AppLogic.GetString("lat_signin_process.aspx.2", m_SkinID, signinCustomer.LocaleSetting);
                lblPwdChgErr.Visible = true;
                return false;
            }

            if (signinCustomer.IsAdminUser || AppLogic.AppConfigBool("UseStrongPwd"))
            {
                if (!Regex.IsMatch(newPassword, AppLogic.AppConfig("CustomerPwdValidator"), RegexOptions.Compiled))
                {
                    lblPwdChgErr.Text = AppLogic.GetString("signin.aspx.26", m_SkinID, signinCustomer.LocaleSetting);
                    lblPwdChgErr.Visible = true;
                    return false;
                }
            }

            if (!signinCustomer.IsAdminUser && !ValidateNewPassword())
            {
                lblPwdChgErr.Visible = true;
                return false;
            }

            return true;
        }

        private bool ValidateNewPassword()
        {
            bool valPwd = false;
            if (tbNewPassword.Text.Replace("*", "").Trim().Length == 0)
            {
                return false;
            }

            if (tbNewPassword.Text == tbNewPassword2.Text)
            {
                try
                {
                    if (AppLogic.AppConfigBool("UseStrongPwd"))
                    {
                        valPwd = Regex.IsMatch(tbNewPassword.Text, AppLogic.AppConfig("CustomerPwdValidator"), RegexOptions.Compiled);
                    }
                    else
                    {
                        valPwd = (tbNewPassword.Text.Length > 4);
                    }
                    if (!valPwd)
                    {
                        lblPwdChgErr.Text = "" + AppLogic.GetString("account.aspx.7", m_SkinID, signinCustomer.LocaleSetting);
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
                lblPwdChgErr.Text = "" + AppLogic.GetString("signin.aspx.32", m_SkinID, signinCustomer.LocaleSetting);
            }
            return valPwd;
        }
    }
}
