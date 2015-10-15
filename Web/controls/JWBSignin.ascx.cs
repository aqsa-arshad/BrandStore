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
using AspDotNetStorefront;

public partial class controls_JWBSignin : System.Web.UI.UserControl
{
    Customer ThisCustomer;
    int m_SkinID;
    protected void errorMessageNotification()
    {
        ForgotPaswwordSuccessMessage.Text = String.Empty;
        ForgotPasswordExecutepanel.Visible = false;
        ForgotPasswordErrorPanel.Visible = true;
        ForgotPasswordErrorMsgLabel.Text = String.Empty;
    }
    protected void successMessageNotification()
    {
        ForgotPasswordErrorPanel.Visible = false;
        ForgotPasswordErrorMsgLabel.Text = String.Empty;
        ForgotPasswordExecutepanel.Visible = true;
        ForgotPaswwordSuccessMessage.Text = String.Empty;
    }
    protected void forgotpasswordButton_Click(object sender, EventArgs e)
    {
        HiddenLabel.Text = "true";
        string EMail = ForgotPasswordEmailTextField.Text.ToString();
        if (EMail.Length == 0)
        {
            errorMessageNotification();
            ForgotPasswordErrorMsgLabel.Text = AppLogic.GetString("lostpassword.aspx.4", m_SkinID, ThisCustomer.LocaleSetting);
            return;
        }

        ForgotPasswordErrorMsgLabel.Text = "Email: " + EMail;
        bool SendWasOk = false;
        UserModel userModel = AuthenticationSSO.GetUserModel(EMail);

        if (userModel != null) // If Okta User
        {
            SendWasOk = AuthenticationSSO.ForgotPasswordRequest(userModel.id);
        }
        else
        {
            Customer c = new Customer(EMail);
            if (!c.IsRegistered || c.IsAdminUser || c.IsAdminSuperUser)
            {
                errorMessageNotification();
                ForgotPasswordErrorMsgLabel.Text = AppLogic.GetString("signin.aspx.25", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                return;
            }
            else
            {
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
            }
        }

        if (!SendWasOk)
        {
            errorMessageNotification();
            ForgotPasswordErrorMsgLabel.Text = AppLogic.GetString("lostpassword.aspx.3", m_SkinID, ThisCustomer.LocaleSetting);
        }
        else
        {
            successMessageNotification();
            ForgotPaswwordSuccessMessage.Text = AppLogic.GetString("lostpassword.aspx.2", m_SkinID, ThisCustomer.LocaleSetting);
        }
    }

    protected void forgotpasswordLink_Click(object sender, EventArgs e)
    {
        HiddenLabel.Text = "true";
        ForgotPasswordEmailTextField.Focus();
        ForgotPasswordPanel.Visible = true;
        LoginPanel.Visible = false;

    }

    protected void GoBackToLoginLink_Click(object sender, EventArgs e)
    {
        HiddenLabel.Text = "false";
        ForgotPasswordPanel.Visible = false;
        LoginPanel.Visible = true;
    }

    protected void Page_Load(object sender, EventArgs e)
    {

        string HiddenFieldText = HiddenLabel.Text;
        if (HiddenFieldText.Equals("true"))
        {
            ForgotPasswordEmailTextField.Focus();
            ForgotPasswordPanel.Visible = true;
            ForgotPasswordExecutepanel.Visible = false;
            LoginPanel.Visible = false;
            ForgotPasswordErrorPanel.Visible = false; // that is where the status msg goes, in all cases in this routine
            ForgotPasswordErrorMsgLabel.Text = String.Empty;
        }
        else
        {
            EmailTextField.Focus();
            ForgotPasswordPanel.Visible = false;
            LoginPanel.Visible = true;
        }
        HiddenLabel.Text = "false";

        String EMailField = EmailTextField.Text.ToLowerInvariant().Trim();

        if (!string.IsNullOrEmpty(EMailField))
        {
            ThisCustomer = new Customer(EMailField);
        }
        if (ThisCustomer == null)
        {
            ThisCustomer = (Page as AspDotNetStorefront.SkinBase).ThisCustomer;
        }

        m_SkinID = (Page as AspDotNetStorefront.SkinBase).SkinID;
    }

    protected void submitButton_Click(object sender, EventArgs e)
    {
        int CurrentCustomerID = ThisCustomer.CustomerID;
        bool RememberMeCheckBox = RememberMe.Checked;
        String EMailField = EmailTextField.Text.ToString();
        String PasswordField = PasswordTextField.Text.ToString();
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
                        ThisCustomer = new Customer(EMailField, true);
                        ExecutePanel.Visible = true;
                        String CustomerGUID = ThisCustomer.CustomerGUID.Replace("{", "").Replace("}", "");
                        ExecutePanel.Visible = true;
                        SignInExecuteLabel.Text = AppLogic.GetString("signin.aspx.2", m_SkinID, ThisCustomer.LocaleSetting);
                        string sReturnURL = FormsAuthentication.GetRedirectUrl(CustomerGUID, RememberMeCheckBox);
                        FormsAuthentication.SetAuthCookie(CustomerGUID, RememberMeCheckBox);
                        Response.Redirect("home.aspx");
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
            /*            
             * Initialize Customer Object after OKTA Authentication
             */
            ThisCustomer = AuthenticationSSO.InitializeCustomerObject(EMailField, PasswordField);
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
                    Response.Redirect("home.aspx");
                }
                else
                {
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
                return;
            }
        }
    }

}