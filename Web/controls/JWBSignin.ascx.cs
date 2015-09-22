using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

public partial class controls_JWBSignin : System.Web.UI.UserControl
{
    Customer ThisCustomer;
    int m_SkinID;
    private TextBox tbSecurityCode;
    private Image imgSecurityImage;
    private Label lblSecurityLabel;
    private Label lblReturnURL;
    private Panel pnlPasswordChangeError;
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
    private Boolean DisablePasswordAutocomplete
    {
        get { return AppLogic.AppConfigBool("DisablePasswordAutocomplete"); }
    }

    private Control GetControl(string Name)
    {
        Control ctrl = new Control();
        //ctrl = ctrlLogin.Controls[0].FindControl(Name) as Control;
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
        tbSecurityCode = GetControl("SecurityCode") as TextBox;
        imgSecurityImage = GetControl("SecurityImage") as Image;
        lblSecurityLabel = GetControl("Label1") as Label;
        lblReturnURL = GetControl("ReturnURL") as Label;
        lblPwdChgErr = GetControl("lblPwdChgErr") as Label;
        pnlPasswordChangeError = GetControl("pnlPasswordChangeError") as Panel;
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
    protected void forgotpasswordButton_Click(object sender, EventArgs e)
    {
        HiddenLabel.Text = "true";
        ForgotPasswordErrorPanel.Visible = true; // that is where the status msg goes, in all cases in this routine
        ForgotPasswordErrorMsgLabel.Text = String.Empty;
        string EMail = ForgotPasswordEmailTextField.Text.ToString();

        if (EMail.Length == 0)
        {
            ForgotPasswordErrorMsgLabel.Text = AppLogic.GetString("lostpassword.aspx.4", m_SkinID, ThisCustomer.LocaleSetting);
            return;
        }

        ForgotPasswordErrorMsgLabel.Text = "Email: " + EMail;
        Customer c = new Customer(EMail);
        if (!c.IsRegistered || c.IsAdminUser || c.IsAdminSuperUser)
        {
            ForgotPasswordErrorMsgLabel.Text = AppLogic.GetString("signin.aspx.25", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
            return;
        }
        else
        {
            bool SendWasOk = false;
            try
            {
                MembershipUser user = System.Web.Security.Membership.GetUser(EMail);
                string newPassword = user.ResetPassword();
                while (newPassword.Contains('*')) // *'s in passwords fail because of replacement - keep generating new passwords until no *'s
                {
                    newPassword = user.ResetPassword();
                }
                String FromEMail = AppLogic.AppConfig("MailMe_FromAddress");
                String PackageName = AppLogic.AppConfig("XmlPackage.LostPassword");
                AppLogic.SendMail(AppLogic.AppConfig("StoreName") + " " + AppLogic.GetString("lostpassword.aspx.6", m_SkinID, ThisCustomer.LocaleSetting), AppLogic.RunXmlPackage(PackageName, null, ThisCustomer, m_SkinID, string.Empty, "newpwd=" + newPassword + "&thiscustomerid=" + ThisCustomer.CustomerID.ToString(), false, false), true, FromEMail, FromEMail, EMail, EMail, "", AppLogic.MailServer());
                SendWasOk = true;
            }
            catch { }
            if (!SendWasOk)
            {
                ForgotPasswordErrorMsgLabel.Text = AppLogic.GetString("lostpassword.aspx.3", m_SkinID, ThisCustomer.LocaleSetting);
            }
            else
            {
                ForgotPasswordErrorMsgLabel.Text = AppLogic.GetString("lostpassword.aspx.2", m_SkinID, ThisCustomer.LocaleSetting);
            }
        }

    }
    protected void forgotpasswordLink_Click(object sender, EventArgs e)
    {
        HiddenLabel.Text = "true";
        ForgotPasswordPanel.Visible = true;
        LoginPanel.Visible = false;

    }
    protected void Page_Load(object sender, EventArgs e)
    {
        string HiddenFieldText = HiddenLabel.Text;
        if (HiddenFieldText.Equals("true"))
        {
            ForgotPasswordPanel.Visible = true;
            LoginPanel.Visible = false;
        }
        else
        {
            ForgotPasswordPanel.Visible = false;
            LoginPanel.Visible = true;
        }
        HiddenLabel.Text = "false";

        // aqsa arshad 19/09/2015
        // Below code is realted to renewing password when expires. as right now no need of it so i commented this code

        //if (DisablePasswordAutocomplete)
        //{
        //    TextBox tbPassword = ctrlLogin.FindControl("Password") as TextBox;
        //    AppLogic.DisableAutocomplete(tbPassword);
        //    AppLogic.DisableAutocomplete(tbOldPassword);
        //    AppLogic.DisableAutocomplete(tbNewPassword);
        //    AppLogic.DisableAutocomplete(tbNewPassword2);
        //}

        String EMailField = EmailTextField.Text.ToLowerInvariant().Trim();

        // aqsa arshad 19/09/2015
        // Below code is realted to renewing password when expires.as right now no need of it so i commented this code

        //if (String.IsNullOrEmpty(EMailField))
        //{
        //    EMailField = ctrlRecoverPassword.UserName;

        //}



        if (!string.IsNullOrEmpty(EMailField))
        {
            ThisCustomer = new Customer(EMailField);
        }
        if (ThisCustomer == null)
        {
            ThisCustomer = (Page as AspDotNetStorefront.SkinBase).ThisCustomer;
        }

        m_SkinID = (Page as AspDotNetStorefront.SkinBase).SkinID;

        EmailTextField.Text = ThisCustomer.EMail;
        PasswordTextField.Text = ThisCustomer.Password;

        // aqsa arshad 19/09/2015
        // Below code is realted to doing login before checkout, as this functionality is not included in this sprit so i commented this code.


        //ControlCollection ctrlcol = ctrlLogin.Controls;

        //PopulateFields(ctrlcol);

        //lblReturnURL.Text = CommonLogic.QueryStringCanBeDangerousContent("ReturnURL");
        //if (CommonLogic.QueryStringNativeInt("ErrorMsg") > 0)
        //{
        //    ErrorMessage err = new ErrorMessage(CommonLogic.QueryStringNativeInt("ErrorMsg"));
        //    ErrorMsgLabel.Text = err.Message;
        //    ErrorPanel.Visible = true;
        //}
        //else
        //{
        //    ErrorMsgLabel.Text = string.Empty;
        //    ErrorPanel.Visible = false;
        //}


        //AppLogic.CheckForScriptTag(lblReturnURL.Text);
        //if (AppLogic.IsAdminSite || CommonLogic.GetThisPageName(true).ToLowerInvariant().IndexOf(AppLogic.AdminDir().ToLowerInvariant() + "/") != -1 || lblReturnURL.Text.ToLowerInvariant().IndexOf(AppLogic.AdminDir().ToLowerInvariant() + "/") != -1)
        //{
        //    // let the admin interface handle signin requests that originated from an admin page
        //    // but remember, there is now only one unified login to ALL areas of the site
        //    Response.Redirect("~/" + AppLogic.AdminDir() + "/signin.aspx");
        //}

        //lblPwdChgErr.Text = "";
        //lblPwdChgErr.Visible = false;
        //if (!Page.IsPostBack)
        //{
        //    cbDoingCheckout.Checked = CommonLogic.QueryStringBool("checkout");
        //    if (lblReturnURL.Text.Length == 0)
        //    {
        //        if (CommonLogic.QueryStringBool("checkout"))
        //        {
        //            lblReturnURL.Text = "~/shoppingcart.aspx?checkout=true";
        //        }
        //        else
        //        {
        //            lblReturnURL.Text = "~/default.aspx";
        //        }
        //    }
        //    hlSignUpLink.NavigateUrl = "~/createaccount.aspx?checkout=" + cbDoingCheckout.Checked.ToString();
        //}

        // aqsa arshad 19/09/2015 
        //if captcha security is enabled 

        if (AppLogic.AppConfigBool("SecurityCodeRequiredOnStoreLogin"))
        {
            tbSecurityCode.Visible = true;
            imgSecurityImage.Visible = true;
            imgSecurityImage.ImageUrl = "~/Captcha.ashx?id=1";
            lblSecurityLabel.Visible = true;
            rfvSecurity.Enabled = true;
        }
    }
    protected void submitButton_Click(object sender, EventArgs e)
    {
        int CurrentCustomerID = ThisCustomer.CustomerID;

        bool RememberMeCheckBox = RememberMe.Checked;
        String EMailField = EmailTextField.Text.ToString();
        String PasswordField = PasswordTextField.Text.ToString();
        bool LoginOK = false;
        // aqsa arshad 19/09/2015 
        // For keeping its value true we have to modify UI of our Login Control , as its value is geeting from Db so no need to comment code for capcha security
        if (AppLogic.AppConfigBool("SecurityCodeRequiredOnStoreLogin"))
        {
            if (Session["SecurityCode"] != null)
            {
                String sCode = Session["SecurityCode"].ToString();
                String fCode = tbSecurityCode.Text;
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
                    ErrorMsgLabel.Text = string.Format(AppLogic.GetString("lat_signin_process.aspx.5", m_SkinID, ThisCustomer.LocaleSetting), sCode, fCode);
                    ErrorPanel.Visible = true;
                    //tbSecurityCode.Text = String.Empty;
                    //imgSecurityImage.ImageUrl = "~/Captcha.ashx?id=1";
                    return;
                }
            }
            else
            {
                ErrorMsgLabel.Text = string.Format(AppLogic.GetString("lat_signin_process.aspx.5", m_SkinID, ThisCustomer.LocaleSetting), "", tbSecurityCode.Text);
                ErrorPanel.Visible = true;
                //tbSecurityCode.Text = String.Empty;
                //imgSecurityImage.ImageUrl = "~/Captcha.ashx?id=1";
                return;
            }
        }

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
                        ThisCustomer = new Customer(EMailField, true);
                        pnlForm.Visible = false;
                        ExecutePanel.Visible = true;
                        String CustomerGUID = ThisCustomer.CustomerGUID.Replace("{", "").Replace("}", "");
                        ExecutePanel.Visible = true;
                        SignInExecuteLabel.Text = AppLogic.GetString("signin.aspx.2", m_SkinID, ThisCustomer.LocaleSetting);
                        string sReturnURL = FormsAuthentication.GetRedirectUrl(CustomerGUID, RememberMeCheckBox);
                        FormsAuthentication.SetAuthCookie(CustomerGUID, RememberMeCheckBox);

                        // aqsa arshad 19/09/2015 
                        // Below code work when user want to login just before checkout , as  right now there is no need of it so i comment it.

                        //if (sReturnURL.Length == 0)
                        //{
                        //    sReturnURL = lblReturnURL.Text;
                        //}
                        //if (sReturnURL.Length == 0 || sReturnURL == "signin.aspx")
                        //{
                        //    if (cbDoingCheckout.Checked)
                        //    {
                        //        sReturnURL = "shoppingcart.aspx";
                        //    }
                        //    else
                        //    {
                        //        sReturnURL = "default.aspx";
                        //    }
                        //}
                        Response.AddHeader("REFRESH", "1; URL=" + Server.UrlDecode(sReturnURL));
                    }
                    else
                    {
                        ThisCustomer = new Customer(0, true);
                    }
                }
            }
        }
        else //normal login
        {
            ThisCustomer = new Customer(EMailField, true);

            if (ThisCustomer.IsRegistered)
            {
                LoginOK = System.Web.Security.Membership.ValidateUser(EMailField, PasswordField);

                if (LoginOK)
                {
                    if (ThisCustomer.LockedUntil > DateTime.Now)
                    {
                        ErrorMsgLabel.Text = AppLogic.GetString("lat_signin_process.aspx.3", m_SkinID, ThisCustomer.LocaleSetting);
                        ErrorPanel.Visible = true;
                        return;
                    }
                    if (!ThisCustomer.Active)
                    {
                        ErrorMsgLabel.Text = AppLogic.GetString("lat_signin_process.aspx.2", m_SkinID, ThisCustomer.LocaleSetting);
                        ErrorPanel.Visible = true;
                        return;
                    }

                    // aqsa arshad 19/09/2015 
                    // Below code work when admin password expires, if i uncomment this then i have to Update UI of login control, as right now there is no need of it so i comment it.

                    //if (((ThisCustomer.IsAdminSuperUser || ThisCustomer.IsAdminUser) && ThisCustomer.PwdChanged.AddDays(AppLogic.AppConfigUSDouble("AdminPwdChangeDays")) < DateTime.Now) || ThisCustomer.PwdChangeRequired)
                    //{
                    //    ErrorMsgLabel.Text = AppLogic.GetString("lat_signin_process.aspx.4", m_SkinID, ThisCustomer.LocaleSetting);
                    //    tbCustomerEmail.Text = EmailTextField.Text.ToString(); //ctrlLogin.UserName;
                    //    ExecutePanel.Visible = false;
                    //    pnlForm.Visible = false;
                    //    pnlChangePwd.Visible = true;
                    //    pnlPasswordChangeError.Visible = false;
                    //   // ctrlRecoverPassword.Visible = false;
                    //    tbOldPassword.Focus();
                    //    return;
                    //}

                    int NewCustomerID = ThisCustomer.CustomerID;

                    if (AppLogic.AppConfigBool("DynamicRelatedProducts.Enabled") || AppLogic.AppConfigBool("RecentlyViewedProducts.Enabled"))
                    {
                        //A Registered Customer browse the products in store site not yet logged-in, update the productview with the Customer's CustomerGUID when
                        //later he decided to login
                        ThisCustomer.ReplaceProductViewFromAnonymous();
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

                    if (ThisCustomer.IsAdminUser)
                    {
                        Security.LogEvent("Store Login", "", ThisCustomer.CustomerID, ThisCustomer.CustomerID, ThisCustomer.ThisCustomerSession.SessionID);
                    }


                    object lockeduntil = DateTime.Now.AddMinutes(-1);
                    ThisCustomer.UpdateCustomer(
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
                    // pnlForm.Visible = false;
                    ExecutePanel.Visible = true;


                    String CustomerGUID = ThisCustomer.CustomerGUID.Replace("{", "").Replace("}", "");

                    ExecutePanel.Visible = true;
                    SignInExecuteLabel.Text = AppLogic.GetString("signin.aspx.2", m_SkinID, ThisCustomer.LocaleSetting);

                    string cookieUserName = CustomerGUID.ToString();

                    bool createPersistentCookie = RememberMeCheckBox;


                    string sReturnURL = FormsAuthentication.GetRedirectUrl(cookieUserName, createPersistentCookie);
                    FormsAuthentication.SetAuthCookie(cookieUserName, createPersistentCookie);

                    HttpCookie authCookie = Response.Cookies[FormsAuthentication.FormsCookieName];
                    if (authCookie != null && !AppLogic.AppConfigBool("GoNonSecureAgain"))
                    {
                        authCookie.Secure = AppLogic.UseSSL() && AppLogic.OnLiveServer();
                    }

                    // aqsa arshad 19/09/2015 
                    // Below code work when user want to login just before checkout , as  right now there is no need of it so i comment it.

                    //if (sReturnURL.Length == 0)
                    //{
                    //    sReturnURL = lblReturnURL.Text;
                    //}
                    //if (sReturnURL.Length == 0 || sReturnURL == "signin.aspx")
                    //{
                    //    if (cbDoingCheckout.Checked)
                    //    {
                    //        sReturnURL = "~/shoppingcart.aspx";
                    //    }
                    //    else
                    //    {
                    //        sReturnURL = "~/default.aspx";
                    //    }
                    //}

                    // Response line after login 

                    //Response.AddHeader("REFRESH", "1; URL=" + Server.UrlDecode(sReturnURL));

                    Response.Redirect("/account.aspx");

                    // aqsa arshad 19/09/2015 
                    // Below code work when admin password expires , it will show UI for changing its passowrd. as there is no need of it so i comment code below

                    //  ctrlRecoverPassword.Visible = false;
                }
                else
                {
                    if (AppLogic.AppConfigBool("SecurityCodeRequiredOnStoreLogin"))
                    {
                        tbSecurityCode.Text = "";
                        Session["SecurityCode"] = CommonLogic.GenerateRandomCode(6);
                    }
                    ErrorMsgLabel.Text = AppLogic.GetString("lat_signin_process.aspx.1", m_SkinID, ThisCustomer.LocaleSetting);
                    ErrorPanel.Visible = true;
                    if (ThisCustomer.IsAdminUser)
                    {
                        object lockuntil = null;
                        int badlogin = 1;
                        if ((ThisCustomer.BadLoginCount + 1) >= AppLogic.AppConfigNativeInt("MaxBadLogins"))
                        {
                            lockuntil = DateTime.Now.AddMinutes(AppLogic.AppConfigUSInt("BadLoginLockTimeOut"));
                            badlogin = -1;
                            ErrorMsgLabel.Text = AppLogic.GetString("lat_signin_process.aspx.3", m_SkinID, ThisCustomer.LocaleSetting);
                            ErrorPanel.Visible = true;
                        }

                        ThisCustomer.UpdateCustomer(
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
                    if (ThisCustomer.IsAdminUser)
                    {
                        Security.LogEvent("Store Login Failed", "Attempted login failed for email address " + EMailField, 0, 0, 0);
                        return;
                    }
                }
            }
            else
            {
                ErrorMsgLabel.Text = AppLogic.GetString("lat_signin_process.aspx.1", m_SkinID, ThisCustomer.LocaleSetting);
                ErrorPanel.Visible = true;
                // Session["SecurityCode"] = CommonLogic.GenerateRandomCode(6);
                //tbSecurityCode.Text = "";
                return;
            }
        }
    }
}