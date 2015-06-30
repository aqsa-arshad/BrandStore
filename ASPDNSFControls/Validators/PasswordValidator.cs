// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using AspDotNetStorefrontCore;
using System;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;

namespace AspDotNetStorefrontControls.Validators
{
    /// <summary>
    /// Checks if the new password is strong or is equal to the confirm password field
    /// </summary>
    public class PasswordValidator : BaseValidator
    {
        private readonly TextBox _Password = new TextBox();
        private readonly TextBox _PasswordConfirm = new TextBox();

        public string Password
        {
            get { return _Password.Text; }
            set { _Password.Text = value; }
        }
        public string PasswordConfirm
        {
            get { return _PasswordConfirm.Text; }
            set { _PasswordConfirm.Text = value; }
        }

        protected override bool EvaluateIsValid()
        {
            Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;

            if (_Password.Text.Trim().Length == 0)
            {
                return true;
            }
            if (_Password.Text == _PasswordConfirm.Text)
            {
                try
                {
                    if (AppLogic.AppConfigBool("UseStrongPwd") || ThisCustomer.IsAdminUser)
                    {

                        if (Regex.IsMatch(_Password.Text, AppLogic.AppConfig("CustomerPwdValidator"), RegexOptions.Compiled))
                        {
                            return true;
                        }
                        else
                        {
                            ErrorMessage = AppLogic.GetString("account.aspx.69", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                            return false;
                        }
                    }
                    else
                    {
                        ErrorMessage = AppLogic.GetString("account.aspx.7", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                        return (_Password.Text.Length > 4);
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
                ErrorMessage = AppLogic.GetString("account.aspx.68", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                return false;
            }
        }
    }
}
