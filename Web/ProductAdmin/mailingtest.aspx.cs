// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
    public partial class mailingTest : AdminPageBase
    {
        #region Event Handlers

        protected void Page_Load(Object sender, EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            InitializeEditor();
        }

        protected void ssOne_SelectedIndexChanged(Object sender, EventArgs e)
        {
            LoadContent(ssOne.SelectedStoreID);
        }

        protected void btnUpdateAppConfigs_Click(Object sender, EventArgs e)
        {
            AppConfigManager.LoadAllConfigs();
            UpdateAppConfigs(ssOne.SelectedStoreID);
        }

        protected void btnSendTestReceipt_Click(Object sender, EventArgs e)
        {
            AppConfigManager.LoadAllConfigs();
            UpdateAppConfigs(ssOne.SelectedStoreID);

            AppConfigManager.LoadAllConfigs();
            ltError.Text = SendTestReceiptEmail(ssOne.SelectedStoreID);
        }

        protected void btnSendNewOrderNotification_Click(Object sender, EventArgs e)
        {
            AppConfigManager.LoadAllConfigs();
            UpdateAppConfigs(ssOne.SelectedStoreID);

            AppConfigManager.LoadAllConfigs();
            ltError.Text = SendTestNewOrderNotification(ssOne.SelectedStoreID);
        }

        protected void btnSendTestShipped_Click(Object sender, EventArgs e)
        {
            AppConfigManager.LoadAllConfigs();
            UpdateAppConfigs(ssOne.SelectedStoreID);

            AppConfigManager.LoadAllConfigs();
            ltError.Text = SendTestShippedEmail(ssOne.SelectedStoreID);
        }

        protected void btnSendAll_Click(Object sender, EventArgs e)
        {
            AppConfigManager.LoadAllConfigs();
            UpdateAppConfigs(ssOne.SelectedStoreID);

            AppConfigManager.LoadAllConfigs();
            ltError.Text = SendAllTests(ssOne.SelectedStoreID);
        }

        #endregion

        #region Private Methods

        private void InitializeEditor()
        {
            ltError.Text = String.Empty;
            SectionTitle = AppLogic.GetString("admin.common.EMailTest", 1, LocaleSetting);
            ltPreEntity.Text = AppLogic.GetString("admin.common.EMailTest", 1, LocaleSetting);

            ssOne.Visible = ssOne.StoreCount > 1;

            if (!ThisCustomer.IsAdminSuperUser)
            {
                ltError.Text = AppLogic.GetString("admin.common.InsufficientPermissions", SkinID, LocaleSetting);
                btnSendAll.Enabled = false;
                btnSendNewOrderNotification.Enabled = false;
                btnSendTestReceipt.Enabled = false;
                btnSendTestShipped.Enabled = false;
            }

            if (IsPostBack)
                return;

            if (ThisCustomer.IsAdminSuperUser)
            {
                LoadContent(AppLogic.StoreID());
            }
        }

        private void LoadContent(Int32 storeId)
        {
            ddXmlPackageReceipt.Items.Clear();
            ddXmlPackageOrderNotifications.Items.Clear();
            ddXmlPackageShipped.Items.Clear();

            txtMailMe_Server.Text = AppLogic.GetAppConfigRouted("MailMe_Server", storeId).ConfigValue;
            txtMailServerPort.Text = AppLogic.GetAppConfigRouted("MailMe_Port", storeId).ConfigValue;
            txtMailServerUser.Text = AppLogic.GetAppConfigRouted("MailMe_User", storeId).ConfigValue;
            txtMailServerPwd.Text = AppLogic.GetAppConfigRouted("MailMe_Pwd", storeId).ConfigValue;
            rblMailServerSSL.SelectedValue = AppLogic.GetAppConfigRouted("MailMe_UseSSL", storeId).ConfigValue;

            txtReceiptFrom.Text = AppLogic.GetAppConfigRouted("ReceiptEMailFrom", storeId).ConfigValue;
            txtReceiptFromName.Text = AppLogic.GetAppConfigRouted("ReceiptEMailFromName", storeId).ConfigValue;
            txtOrderNotificationFrom.Text = AppLogic.GetAppConfigRouted("GotOrderEMailFrom", storeId).ConfigValue;
            txtOrderNotificationFromName.Text = AppLogic.GetAppConfigRouted("GotOrderEMailFromName", storeId).ConfigValue;

            txtOrderNotificationTo.Text = AppLogic.GetAppConfigRouted("GotOrderEMailTo", storeId).ConfigValue;
            txtOrderNotificationToName.Text = AppLogic.GetAppConfigRouted("MailMe_ToName", storeId).ConfigValue;

            rblSendReceipts.SelectedValue = AppLogic.GetAppConfigRouted("SendOrderEMailToCustomer", storeId).ConfigValue;
            rblSendShippedNotifications.SelectedValue = AppLogic.GetAppConfigRouted("SendShippedEMailToCustomer", storeId).ConfigValue;
            rblSendOrderNotifications.SelectedValue = AppLogic.GetAppConfigRouted("TurnOffStoreAdminEMailNotifications", storeId).ConfigValue;

            ArrayList xmlPackages = AppLogic.ReadXmlPackages("notification", SkinID);
            foreach (String xmlPackage in xmlPackages)
            {
                ddXmlPackageReceipt.Items.Add(new ListItem(xmlPackage, xmlPackage));
                ddXmlPackageOrderNotifications.Items.Add(new ListItem(xmlPackage, xmlPackage));
                ddXmlPackageShipped.Items.Add(new ListItem(xmlPackage, xmlPackage));
            }

            foreach (ListItem xmlPackageItem in ddXmlPackageReceipt.Items)
            {
                if (xmlPackageItem.Value.Equals(AppLogic.GetAppConfigRouted("XmlPackage.OrderReceipt", storeId).ConfigValue.ToLowerInvariant()))
                    ddXmlPackageReceipt.SelectedValue = AppLogic.GetAppConfigRouted("XmlPackage.OrderReceipt", storeId).ConfigValue.ToLowerInvariant();

                if (xmlPackageItem.Value.Equals(AppLogic.GetAppConfigRouted("XmlPackage.NewOrderAdminNotification", storeId).ConfigValue.ToLowerInvariant()))
                    ddXmlPackageOrderNotifications.SelectedValue = AppLogic.GetAppConfigRouted("XmlPackage.NewOrderAdminNotification", storeId).ConfigValue.ToLowerInvariant();

                if (xmlPackageItem.Value.Equals(AppLogic.GetAppConfigRouted("XmlPackage.OrderShipped", storeId).ConfigValue.ToLowerInvariant()))
                    ddXmlPackageShipped.SelectedValue = AppLogic.GetAppConfigRouted("XmlPackage.OrderShipped", storeId).ConfigValue.ToLowerInvariant();
            }
        }

        private void UpdateAppConfigs(Int32 storeId)
        {
            //
            UpdateAppConfig(storeId, "MailMe_Server", txtMailMe_Server.Text);
            UpdateAppConfig(storeId, "MailMe_Port", txtMailServerPort.Text);
            UpdateAppConfig(storeId, "MailMe_User", txtMailServerUser.Text);
            UpdateAppConfig(storeId, "MailMe_Pwd", txtMailServerPwd.Text);
            UpdateAppConfig(storeId, "MailMe_UseSSL", rblMailServerSSL.SelectedValue);

            UpdateAppConfig(storeId, "MailMe_FromAddress", txtReceiptFrom.Text);
            UpdateAppConfig(storeId, "MailMe_FromName", txtReceiptFromName.Text);
            UpdateAppConfig(storeId, "MailMe_ToAddress", txtOrderNotificationTo.Text);
            UpdateAppConfig(storeId, "MailMe_ToName", txtOrderNotificationToName.Text);

            UpdateAppConfig(storeId, "GotOrderEMailTo", txtOrderNotificationTo.Text);
            UpdateAppConfig(storeId, "GotOrderEMailFrom", txtOrderNotificationFrom.Text);
            UpdateAppConfig(storeId, "GotOrderEMailFromName", txtOrderNotificationFromName.Text);

            UpdateAppConfig(storeId, "ReceiptEMailFrom", txtReceiptFrom.Text);
            UpdateAppConfig(storeId, "ReceiptEMailFromName", txtReceiptFromName.Text);

            UpdateAppConfig(storeId, "SendOrderEMailToCustomer", rblSendReceipts.SelectedValue);
            UpdateAppConfig(storeId, "SendShippedEMailToCustomer", rblSendShippedNotifications.SelectedValue);
            UpdateAppConfig(storeId, "TurnOffStoreAdminEMailNotifications", rblSendOrderNotifications.SelectedValue);

            UpdateAppConfig(storeId, "XmlPackage.NewOrderAdminNotification", (ddXmlPackageOrderNotifications.SelectedValue != "0") ? ddXmlPackageOrderNotifications.SelectedValue.ToLowerInvariant() : "notification.adminneworder.xml.config");
            UpdateAppConfig(storeId, "XmlPackage.OrderReceipt", (ddXmlPackageReceipt.SelectedValue != "0") ? ddXmlPackageReceipt.SelectedValue.ToLowerInvariant() : "notification.receipt.xml.config");
            UpdateAppConfig(storeId, "XmlPackage.OrderShipped", (ddXmlPackageShipped.SelectedValue != "0") ? ddXmlPackageShipped.SelectedValue.ToLowerInvariant() : "notification.shipped.xml.config");
        }

        private void UpdateAppConfig(Int32 storeId, String name, String value)
        {
            AppConfig emailAppConfig = AppLogic.GetAppConfig(storeId, name);
            if (emailAppConfig == null)
                emailAppConfig = AppConfig.Create(name, String.Empty, value, "EMAIL", true, storeId);
            else
                AppConfig.Update(emailAppConfig.AppConfigID, emailAppConfig.Description, value, emailAppConfig.GroupName, emailAppConfig.SuperOnly, storeId);
        }

        private String SendAllTests(Int32 storeId)
        {
            String errorMessage = SendTestReceiptEmail(storeId);
            errorMessage = String.Format(AppLogic.GetString("admin.mailingtest.ErrorText", SkinID, LocaleSetting), errorMessage, SendTestNewOrderNotification(storeId));
            errorMessage = String.Format(AppLogic.GetString("admin.mailingtest.ErrorText", SkinID, LocaleSetting), errorMessage, SendTestShippedEmail(storeId));

            return errorMessage;
        }

        private String SendTestReceiptEmail(Int32 storeId)
        {
            if (!AppLogic.AppConfigBool("SendOrderEMailToCustomer"))
                return AppLogic.GetString("mailingtest.aspx.8", SkinID, LocaleSetting);

            try
            {
                String SubjectReceipt = String.Format(AppLogic.GetString("common.cs.2", SkinID, LocaleSetting), AppLogic.AppConfig("StoreName",ssOne.SelectedStoreID, true));
                String PackageName = AppLogic.AppConfig("XmlPackage.OrderReceipt");
                XmlPackage2 p = new XmlPackage2(PackageName, null, SkinID, String.Empty, "ordernumber=999999");
                String receiptBody = p.TransformString();
                AppLogic.SendMail(SubjectReceipt, receiptBody + AppLogic.AppConfig("MailFooter"), true, AppLogic.AppConfig("ReceiptEMailFrom",storeId, true), AppLogic.AppConfig("ReceiptEMailFromName", storeId, true), ThisCustomer.EMail, String.Empty, String.Empty, AppLogic.MailServer());
            }
            catch (Exception exception)
            {
                return GenerateExceptionMessage(exception, storeId);
            }

            return AppLogic.GetString("mailingtest.aspx.1", SkinID, LocaleSetting);
        }

        private String SendTestShippedEmail(Int32 storeId)
        {
            if (!AppLogic.AppConfigBool("SendShippedEMailToCustomer"))
            {
                return AppLogic.GetString("mailingtest.aspx.11", SkinID, LocaleSetting);
            }
            try
            {
                String SubjectReceipt = String.Format(AppLogic.GetString("common.cs.2", SkinID, LocaleSetting), AppLogic.AppConfig("StoreName", storeId, true));
                String PackageName = AppLogic.AppConfig("XmlPackage.OrderShipped");
                XmlPackage2 p = new XmlPackage2(PackageName, null, SkinID, String.Empty, "ordernumber=999999");
                String receiptBody = p.TransformString();
                AppLogic.SendMail(SubjectReceipt, receiptBody + AppLogic.AppConfig("MailFooter"), true, AppLogic.AppConfig("ReceiptEMailFrom", storeId, true), AppLogic.AppConfig("ReceiptEMailFromName", storeId, true), ThisCustomer.EMail, String.Empty, String.Empty, AppLogic.MailServer());
            }
            catch (Exception exception)
            {
                return GenerateExceptionMessage(exception, storeId);
            }
            return AppLogic.GetString("mailingtest.aspx.10", SkinID, LocaleSetting);
        }

        private String SendTestNewOrderNotification(Int32 storeId)
        {
            if (AppLogic.AppConfigBool("TurnOffStoreAdminEMailNotifications"))
            {
                return AppLogic.GetString("mailingtest.aspx.5", SkinID, LocaleSetting);
            }
            try
            {
                String newOrderSubject = String.Format(AppLogic.GetString("common.cs.5", SkinID, LocaleSetting), AppLogic.AppConfig("StoreName", storeId, true));

                String PackageName = AppLogic.AppConfig("XmlPackage.NewOrderAdminNotification");

                XmlPackage2 p = new XmlPackage2(PackageName, null, SkinID, String.Empty, "ordernumber=999999");
                String newOrderNotification = p.TransformString();

                String SendToList = AppLogic.AppConfig("GotOrderEMailTo").ToString().Replace(",", ";");
                if (SendToList.IndexOf(';') != -1)
                {
                    foreach (String s in SendToList.Split(';'))
                    {
                        AppLogic.SendMail(newOrderSubject, newOrderNotification + AppLogic.AppConfig("MailFooter", storeId, true), true, AppLogic.AppConfig("GotOrderEMailFrom", storeId, true), AppLogic.AppConfig("GotOrderEMailFromName", storeId, true), s.Trim(), s.Trim(), String.Empty, AppLogic.MailServer());
                    }
                }
                else
                {
                    AppLogic.SendMail(newOrderSubject, newOrderNotification + AppLogic.AppConfig("MailFooter", storeId, true), true, AppLogic.AppConfig("GotOrderEMailFrom", storeId, true), AppLogic.AppConfig("GotOrderEMailFromName", storeId, true), SendToList, SendToList, String.Empty, AppLogic.MailServer());
                }
            }
            catch (Exception exception)
            {
                return GenerateExceptionMessage(exception, storeId);
            }
            return AppLogic.GetString("mailingtest.aspx.2", SkinID, LocaleSetting);
        }

        private String GenerateExceptionMessage(Exception exception, Int32 storeId)
        {
            Int32 MailMe_PwdLen = AppLogic.AppConfig("MailMe_Pwd", storeId, true).ToString().Length;
            Int32 MailMe_UserLen = AppLogic.AppConfig("MailMe_User", storeId, true).ToString().Length;

            String retVal = String.Empty;

            if (exception.Message.ToString().IndexOf("AUTHENTICATION", StringComparison.InvariantCultureIgnoreCase) != -1 || exception.Message.ToString().IndexOf("OBJECT REFERENCE", StringComparison.InvariantCultureIgnoreCase) != -1 || exception.Message.ToString().IndexOf("NO SUCH USER HERE", StringComparison.InvariantCultureIgnoreCase) != -1)
            {
                if (MailMe_UserLen == 0 && MailMe_PwdLen == 0)
                    retVal = AppLogic.GetString("mailingtest.aspx.3", SkinID, LocaleSetting) + "<br/>&nbsp;·" + AppLogic.GetString("mailingtest.aspx.7", SkinID, LocaleSetting) + "<br/>&nbsp;·" + AppLogic.GetString("mailingtest.aspx.6", SkinID, LocaleSetting);
                else if (MailMe_UserLen == 0)
                    retVal = AppLogic.GetString("mailingtest.aspx.3", SkinID, LocaleSetting) + "<br/>&nbsp;·" + AppLogic.GetString("mailingtest.aspx.7", SkinID, LocaleSetting);
                else if (MailMe_PwdLen == 0)
                    retVal = AppLogic.GetString("mailingtest.aspx.3", SkinID, LocaleSetting) + "<br/>&nbsp;·" + AppLogic.GetString("mailingtest.aspx.6", SkinID, LocaleSetting);
                else
                    retVal = AppLogic.GetString("mailingtest.aspx.3", SkinID, LocaleSetting) + "<br/>&nbsp;·" + AppLogic.GetString("mailingtest.aspx.9", SkinID, LocaleSetting);

                if (retVal.Length != 0)
                    return retVal;
            }

            return AppLogic.GetString("mailingtest.aspx.3", SkinID, LocaleSetting) + "<br/>&nbsp;·" + exception.Message.ToString();
        }

        #endregion
    }
}
