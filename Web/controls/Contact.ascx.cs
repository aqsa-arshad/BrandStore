// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Text.RegularExpressions;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontCore.Validation;

namespace AspDotNetStorefront
{
    public partial class Contact : System.Web.UI.UserControl
    {
        Customer ThisCustomer;

        private Boolean UseCaptcha
        {
            get
            {
                return AppLogic.AppConfigBool("ContactUs.UseCaptcha");
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            ThisCustomer = (Page as SkinBase).ThisCustomer;
            phCaptchaPane.Visible = UseCaptcha;
        }

        private string GetString(string key)
        {
            return AppLogic.GetString(key, ThisCustomer.LocaleSetting);
        }

        private bool Validate()
        {
            lblCaptchaError.Text = string.Empty;
            lblNameError.Text = string.Empty;
            lblPhoneError.Text = string.Empty;
            lblEmailError.Text = string.Empty;
            lblSubjectError.Text = string.Empty;
            lblMessageError.Text = string.Empty;
            txtCaptchaText.Style.Remove("border");

            bool isValid = true;
            bool captchaPassed = true;

            if (UseCaptcha)
            {
                String userCode = txtCaptchaText.Text;
                String securityCode = (string)Context.Session["SecurityCode"];

                if (AppLogic.AppConfigBool("Captcha.CaseSensitive"))
                    captchaPassed = userCode.Equals(securityCode);
                else
                    captchaPassed = userCode.Equals(securityCode, StringComparison.InvariantCultureIgnoreCase);

                if (!captchaPassed)
                {
                    lblCaptchaError.Text = GetString("Global.CaptchaFailed");
                    isValid = false;
                }
            }
            
            if (FromName == string.Empty)
            {
                lblNameError.Text = string.Format(GetString("Error.FieldRequired"), GetString("ContactUs.Name"));
                isValid = false;
            }
            if (PhoneNumber == string.Empty)
            {
                lblPhoneError.Text = string.Format(GetString("Error.FieldRequired"), GetString("ContactUs.PhoneNumber"));
                isValid = false;
            }
            else
            { 
                //NOTE: AspDotNetStoreFront form does not use ASP.net validators! WTF?
                Regex regex = new Regex(@"^\s*(?:\+?(\d{1,3}))?[-. (]*(\d{3})[-. )]*(\d{3})[-. ]*(\d{4})(?: *x(\d+))?\s*$");
                if (!regex.IsMatch(PhoneNumber))
                {
                    lblPhoneError.Text = "Phone number is not valid";
                    isValid = false;
                }
            }
            if (Message == string.Empty)
            {
                lblMessageError.Text = string.Format(GetString("Error.FieldRequired"),GetString("ContactUs.Message"));
                isValid = false;
            }
            if (EmailAddress == string.Empty)
            {
                lblEmailError.Text = string.Format(GetString("Error.FieldRequired"),GetString("ContactUs.EmailAddress"));
                isValid = false;
            }
            else if (!(new EmailAddressValidator()).IsValidEmailAddress(EmailAddress))
            {
                lblEmailError.Text = GetString("Error.InvalidEmailAddressFormat");
                isValid = false;
            }
            if (Subject == string.Empty)
            {
                lblSubjectError.Text = string.Format(GetString("Error.FieldRequired"), "Subject");
                isValid = false;
            }

            if (Message.Length > 2000)
            {
                lblMessageError.Text = "Message is too long";
                isValid = false;
            }

            if (isValid)
            {
                lblEmailError.Text = String.Empty;
                lblCaptchaError.Text = String.Empty;
            }

            if (!isValid && captchaPassed)
            {
                txtCaptchaText.Text = "";
                txtCaptchaText.Style.Add("border", "solid 1px red;");

            }

            return isValid;
        }

        #region "Properties"
        private string FromName
        {
            get { return txtName.Text; }
        }
        private string PhoneNumber
        {
            get { return txtPhone.Text; }
            set { txtPhone.Text = value; }
        }
        private string EmailAddress
        {
            get { return txtEmailAddress.Text; }
        }
        private string Subject
        {
            get { return txtSubject.Text; }
        }
        private string Message
        {
            get { return txtMessage.Text; }
        }

        #endregion

        protected void cmdSubmit_Click(object sender, EventArgs e)
        {
            if (Validate())
            {
                try
                {
                    EMail em = new EMail();
                    em.EmailAddress = AppLogic.AppConfig("GotOrderEMailTo");
                    em.FromAddress = AppLogic.AppConfig("GotOrderEMailFrom");
                    em.FromName = AppLogic.AppConfig("GotOrderEMailFromName");
                    em.MailSubject = Subject;
                    em.MailContents = GetContactTopic();
                    em.MailServer = AppLogic.MailServer();
                    em.MailType = BulkMailTypeEnum.None;
                    em.UseHTML = true;
                    em.IncludeFooter = false;

                    em.Send();

                    resetError(GetString("ContactUs.Successful"), false);
                    resetFields();
                    pnlContactForm.Visible = false;
                }
                catch (Exception ex)
                {
                    resetError(ex.Message, true);
                }
            }
        }

        private String GetContactTopic()
        {
            String emailTopic = new Topic("ContactEmail").ContentsRAW;

            emailTopic = emailTopic
                .Replace("%NAME%", FromName)
                .Replace("%EMAIL%", EmailAddress)
                .Replace("%PHONE%", PhoneNumber)
                .Replace("%SUBJECT%", Subject)
                .Replace("%MESSAGE%", Message);

            return emailTopic;
        }

        private void resetError(String str, bool IsError)
        {
            if (IsError)
            {
                ltError.Text = "<div style=\"color:Red;\">{0}</div>".FormatWith(str);
                txtCaptchaText.Text = String.Empty;
            }
            else
            {
                ltError.Text = "<b>{0}</b>".FormatWith(str);
            }
        }

        private void resetFields()
        {
            txtCaptchaText.Text = String.Empty;
            txtEmailAddress.Text = String.Empty;
            txtMessage.Text = String.Empty;
            txtName.Text = String.Empty;
            txtPhone.Text = String.Empty;
            txtSubject.Text = String.Empty;
        }
    }
}
