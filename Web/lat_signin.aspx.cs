// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontCore.Validation;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for lat_signin.
    /// </summary>
    public partial class lat_signin : SkinBase
    {
        private Boolean DisablePasswordAutocomplete
        {
            get { return AppLogic.AppConfigBool("DisablePasswordAutocomplete"); }
        }

        protected void Page_Load(object sender, System.EventArgs e)
        {
            RequireSecurePage();

            if (DisablePasswordAutocomplete)
            {
                AppLogic.DisableAutocomplete(Password);
                AppLogic.DisableAutocomplete(Password);
            }

            int AffiliateID = int.Parse(CommonLogic.IIF(CommonLogic.IsInteger(Profile.LATAffiliateID), Profile.LATAffiliateID, "0"));           


            // this may be overwridden by the XmlPackage below!
            SectionTitle = "<a href=\"lat_account.aspx\">" + AppLogic.GetString("AppConfig.AffiliateProgramName", SkinID, ThisCustomer.LocaleSetting) + "</a> - Sign In";

            String ReturnURL = CommonLogic.QueryStringCanBeDangerousContent("returnurl");
            AppLogic.CheckForScriptTag(ReturnURL);

            if (!IsPostBack)
            {
                UpdatePageContent();
            }

        }


        private void UpdatePageContent()
        {
            AppConfig_AffiliateProgramName2.Text = AppLogic.GetString("AppConfig.AffiliateProgramName", SkinID, ThisCustomer.LocaleSetting);
            AppConfig_AffiliateProgramName3.Text = AppLogic.GetString("AppConfig.AffiliateProgramName", SkinID, ThisCustomer.LocaleSetting);
       
            if (CommonLogic.QueryStringNativeInt("errormsg") > 0)
            {
                ErrorMessage e = new ErrorMessage(CommonLogic.QueryStringNativeInt("errormsg"));
                lblErrMsg.Text = Server.HtmlEncode(e.Message) + " Please Try Again:";
            }

            lblReqPwdErr.Text = "";

        }

        private void ProcessSignIn()
        {
            String EMailField = EMail.Text.ToLowerInvariant().Trim();
            String PasswordField = Password.Text;

            if (EMailField.Length == 0 || PasswordField.Length == 0)
            {
                lblErrMsg.Text = "Please enter both your e-mail and password";
                return;
            }

            Affiliate a = new Affiliate(EMailField);
            Password p = new Password(PasswordField, a.SaltKey);

            if (a == null || a.Password != p.SaltedPassword)
            {
                ErrorMessage err = new ErrorMessage(AppLogic.GetString("lat_signin_process.aspx.1", SkinID, ThisCustomer.LocaleSetting));
                Response.Redirect("lat_signin.aspx?errormsg=" + err.MessageId);
            }

            int AffiliateID = a.AffiliateID;

            // we've got a good login:
            Profile.LATAffiliateID = AffiliateID.ToString();

            String AffiliateGUID = a.AffiliateGUID.ToString();

            lblSigninSuccess.Text = AppLogic.GetString("AppConfig.AffiliateProgramName", SkinID, ThisCustomer.LocaleSetting) + " sign-in complete, please wait...";
            pnlSigninSuccess.Visible = true;
            pnlContent.Visible = false;

            String ReturnURLx = Server.HtmlDecode(ReturnURL.Text);
            if (ReturnURLx.Length == 0)
            {
                ReturnURLx = "lat_account.aspx";
            }
            Response.AddHeader("REFRESH", "1; URL=" + Server.UrlDecode(ReturnURLx));
        }

        public void btnSignIn_Click(object sender, EventArgs e)
        {
            ProcessSignIn();
        }

        public void btnLostPassword_Click(object sender, EventArgs e)
        {
            if (ResetPwdEMail.Text.Trim().Length == 0)
            {
                lblReqPwdErr.Text = "Please enter a valid email address";
                return;
            }

            String AffName = AppLogic.GetString("AppConfig.AffiliateProgramName", 1, Localization.GetDefaultLocale());
            Affiliate a = new Affiliate(ResetPwdEMail.Text);
            if (a.AffiliateID != -1)
            {
                Password p = new Password();
                a.Update(null, p.SaltedPassword, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, p.Salt);
                try
                {
                    AppLogic.SendMail(AffName + " Password", AppLogic.RunXmlPackage("notification.lostpassword.xml.config", null, ThisCustomer, ThisCustomer.SkinID, "", "thisaffiliateid=" + a.AffiliateID.ToString() + "&newpwd=" + p.ClearPassword + "&AffName=" + AffName, false, false), true, AppLogic.AppConfig("AffiliateEMailAddress"), AppLogic.AppConfig("AffiliateEMailAddress"), a.EMail, a.EMail, "", AppLogic.MailServer());
                    lblReqPwdErr.Text = "Your new password has been sent.";
                }
                catch
                {
                    lblReqPwdErr.Text = "There were problems emailing your password please try again later.";
                }
            }
            else
            {
                lblReqPwdErr.Text = "There is no registered " + AffName + " member with that e-mail address!";
            }
        }
    }
}
