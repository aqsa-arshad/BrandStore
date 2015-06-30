// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for sendform.
    /// </summary>
    public partial class sendform : SkinBase
    {
        protected void Page_Load(object sender, System.EventArgs e)
        {
            SectionTitle = AppLogic.GetString("sendform.aspx.1", SkinID, ThisCustomer.LocaleSetting);
            // DOS attack prevention:
            if (AppLogic.OnLiveServer() && (Request.UrlReferrer == null || Request.UrlReferrer.Authority != Request.Url.Authority))
            {
                Response.Redirect("default.aspx", true);
                return;
            }
            // send form to store administrator:
            String FormContents = String.Empty;

            // Undocumented Feature: use XmlPackage if specified by AppConfig or in form post, to create the actual Email Contents (the XmlPackage has full access to all form post data).
            // you can force an XmlPackage for each form submitted here by including a hidden form field with the name "UseXmlPackage" and the value set to the name of the XmlPackage you want to use to handle that particular form

            String UseXmlPackage = CommonLogic.FormCanBeDangerousContent("UseXmlPackage").Trim();
            if (UseXmlPackage.Length == 0)
            {
                UseXmlPackage = AppLogic.AppConfig("SendForm.XmlPackage").Trim();
            }
            if (UseXmlPackage.Length != 0)
            {
                // use xmlpackage specified
                FormContents = AppLogic.RunXmlPackage(UseXmlPackage, base.GetParser, ThisCustomer, ThisCustomer.SkinID, String.Empty, String.Empty, true, false);
            }
            else
            {
                // just build up form inputs, and send them
                if (CommonLogic.FormCanBeDangerousContent("AsXml").Length == 0)
                {
                    FormContents = CommonLogic.GetFormInput(true, "");
                    FormContents = FormContents + AppLogic.AppConfig("MailFooter");
                }
                else
                {
                    FormContents = CommonLogic.GetFormInputAsXml(true, "root");
                }
            }
            String Subject = CommonLogic.FormCanBeDangerousContent("Subject");
            if (Subject.Length == 0)
            {
                Subject = AppLogic.GetString("sendform.aspx.2", SkinID, ThisCustomer.LocaleSetting);
            }
            String SendTo = CommonLogic.FormCanBeDangerousContent("SendTo");
            if (SendTo.Length == 0)
            {
                SendTo = AppLogic.AppConfig("GotOrderEMailTo");
            }
            else
            {
                SendTo += "," + AppLogic.AppConfig("GotOrderEMailTo");
            }

            foreach (String s in SendTo.Replace(",", ";").Split(';'))
            {
                String s2 = s.Trim();
                if (AppLogic.AppConfig("GotOrderEMailFrom").Trim().Length == 0 || s2.Length == 0)
                {
                    throw new ArgumentException("Please run your store Configuration Wizard in your admin site, to properly setup all your store e-mail address AppConfig values!");
                }
                AppLogic.SendMail(Subject, FormContents, true, AppLogic.AppConfig("GotOrderEMailFrom"), AppLogic.AppConfig("GotOrderEMailFromName"), s2, s2, "", AppLogic.MailServer());
            }
            Label1.Text = AppLogic.GetString("sendform.aspx.3", SkinID, ThisCustomer.LocaleSetting);
        }


    }
}
