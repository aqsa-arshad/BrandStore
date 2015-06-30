// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Text;
using AspDotNetStorefrontCore;
using AspDotNetStorefront;
using AspDotNetStorefront.License.Contract;
using AspDotNetStorefront.License.Concrete;
using System.Linq;
using System.Collections.Generic;

namespace AspDotNetStorefrontAdmin
{
    public partial class about : AdminPageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            ltDateTime.Text = Localization.ToNativeDateTimeString(System.DateTime.Now);
            ltExecutionMode.Text = CommonLogic.IIF(IntPtr.Size == 8, "64 Bit", "32 Bit");

            SectionTitle = AppLogic.GetString("admin.sectiontitle.about", SkinID, LocaleSetting);

            if (!IsPostBack)
            {
                loadSystemInformation();
                loadLicensing();
                loadLocalizationInformation();
                loadGatewayInformation();
                loadShippingInformation();
            }
        }

        protected void loadSystemInformation()
        {
            ltStoreVersion.Text = CommonLogic.GetVersion();
            ltOnLiveServer.Text = AppLogic.OnLiveServer().ToString();
            ltUseSSL.Text = AppLogic.UseSSL().ToString();
            ltServerPortSecure.Text = CommonLogic.IsSecureConnection().ToString();
            ltAdminDirChanged.Text = CommonLogic.IIF(AppLogic.AppConfig("AdminDir").ToLowerInvariant() == "admin", AppLogic.GetString("admin.common.No", SkinID, LocaleSetting), AppLogic.GetString("admin.common.Yes", SkinID, LocaleSetting));
            ltCaching.Text = (AppLogic.AppConfigBool("CacheMenus") ? AppLogic.GetString("admin.common.OnUC", SkinID, LocaleSetting) : AppLogic.GetString("admin.common.OffUC", SkinID, LocaleSetting));
        }

        protected void loadLicensing()
        {
            ILicense[] licenses = new ILicense[0];
            IPermissionSet permissionSet = new PermissionSet();
            LicenseException licenseException = new LicenseException();
            String currentResourceGuid = AspDotNetStorefront.Global.LicenseInfo("id");

			permissionSet = Global.GetLicenseInfo(out licenses, out licenseException);

            StringBuilder retVal = new StringBuilder();

            retVal.AppendLine("<br /><br />");

            if (licenses.Count() == 0)
            {
                pnlLicenseInformation.Text = retVal.ToString();
                return;
            }

            retVal.Append("The following licenses have been found in your images directory:<br /><br />");
            foreach (ILicense license in licenses)
                retVal.Append(GetLicenseSummary(license));

            if (permissionSet == null)
            {
                pnlLicenseInformation.Text = retVal.ToString();
                return;
            }

            DomainPermission[] domainPermissions = permissionSet.PermissionsOfType<DomainPermission>();
            if (domainPermissions.Count() > 0)
            {
                retVal.Append("<b>The following domains are authorized for this resource</b>: ");
                foreach (DomainPermission domainPermission in domainPermissions)
                    foreach (String domain in domainPermission.Domains)
                        retVal.AppendFormat("{0}<br />", domain);
            }

            retVal.AppendLine("<br /><br />");

            TokenPermission[] tokenPermissions = permissionSet.PermissionsOfType<TokenPermission>();
            if (tokenPermissions.Count() > 0)
            {
                retVal.Append("The following tokens are authorized:<br />");
                foreach (TokenPermission tokenPermission in tokenPermissions)
                    foreach (String token in tokenPermission.Tokens)
                        retVal.AppendFormat("{0}<br />", token);
            }

            retVal.AppendLine("<br /><br />");

            IdentifierPermission[] identifierPermissions = permissionSet.PermissionsOfType<IdentifierPermission>();
            if (identifierPermissions.Count() > 0)
            {
                retVal.Append("The following identifiers are authorized:<br />");
                foreach (IdentifierPermission identifierPermission in identifierPermissions)
                    foreach (Guid identifier in identifierPermission.Identifiers)
                        retVal.AppendFormat("{0}<br />", identifier);
            }

            retVal.AppendLine("<br /><br />");

            IEnumerable<String> notifications = licenses.SelectMany(l => l.Policies.Select(p => p.NotificationSet).SelectMany(ns => ns.Notifications))
                            .Where(n =>
                                            n.GetType() == typeof(ADNSFWebsiteNotification)

                                            &&
                                            (n.NotificationAreaTarget == NotificationAreaTarget.Private ||
                                            n.NotificationAreaTarget == NotificationAreaTarget.Both)

                                            && (n.NotificationRenderType == NotificationRenderType.RenderAlways ||
                                            n.NotificationRenderType == NotificationRenderType.RenderIfValid)

                                            && (!n.ActiveBeginDate.HasValue || DateTime.Now >= n.ActiveBeginDate)
                                            && (!n.ActiveEndDate.HasValue || DateTime.Now < n.ActiveEndDate)
                                            ).Select(n => n.Message);

            foreach (String notification in notifications)
                retVal.AppendFormat("{0}<br />", notification);

            pnlLicenseInformation.Text = retVal.ToString();
        }

        private string GetLicenseSummary(ILicense license)
        {
            StringBuilder retVal = new StringBuilder();
            retVal.AppendFormat("<b>Id: {0}</b><br />", license.Identifier);
            retVal.AppendFormat("<b>Created: {0}</b><br />", license.CreatedDate);
            retVal.AppendFormat("<b>Filename: {0}</b><br /><br />", license.Filename);
            int policynumber = 1;
            retVal.Append("<table>");
            foreach (IPolicy ip in license.Policies.Where(p => p.GetType() == typeof(ADNSFPolicy)))
            {
                ADNSFPolicy aip = ip as ADNSFPolicy;
                String token = string.Empty;
                TokenPermission tp = aip.PermissionSet.Permissions.FirstOrDefault(p => p.GetType() == typeof(TokenPermission)) as TokenPermission;
                if (tp != null)
                    token = string.Join(" ", tp.Tokens);

                retVal.Append("<tr><td><b>Policy " + policynumber++ + "</b></td><td><b>"+token+"</b></td></tr>");
                retVal.AppendFormat("<tr><td align=\"right\">{0}</td><td>{1}</td></tr>", "Resource:", aip.ResourceIdentifier );

                foreach (IRule ir in ip.RuleSet.Rules.Where(r => r.GetType() == typeof(HostRule)))
                {
                    HostRule hr = ir as HostRule;
                    retVal.AppendFormat("<tr><td align=\"right\">{0}</td><td>{1}</td></tr>", CommonLogic.IIF(hr.Domains.Count() > 1,  "Valid Hosts:", "Valid Host:"), String.Join("; ", hr.Domains) + ";");
                }

                foreach (IRule ir in ip.RuleSet.Rules.Where(r => r.GetType() == typeof(LicenseRule)))
                {
                    LicenseRule lr = ir as LicenseRule;
                    retVal.AppendFormat("<tr><td align=\"right\">{0}</td><td>{1}</td></tr>", "Required License Filename:", lr.RequiredFileName);
                }

                foreach (IRule ir in ip.RuleSet.Rules.Where(r => r.GetType() == typeof(TimePeriodRule)))
                {
                    TimePeriodRule tpr = ir as TimePeriodRule;
                    retVal.AppendFormat("<tr><td align=\"right\">{0}</td><td>{1}</td></tr>", "Start Date:", tpr.BeginDate.ToShortDateString());
                    retVal.AppendFormat("<tr><td align=\"right\">{0}</td><td>{1}</td></tr>", "End Date:", tpr.EndDate.ToShortDateString());
                }

                foreach (IRule ir in ip.RuleSet.Rules.Where(r => r.GetType() == typeof(EnvironmentRule)))
                {
                    EnvironmentRule er = ir as EnvironmentRule;
                    if (!String.IsNullOrEmpty(er.MachineName))
                        retVal.AppendFormat("<tr><td align=\"right\">{0}</td><td>{1}</td></tr>", "Required Machine Name:", er.MachineName);
                    if (!String.IsNullOrEmpty(er.OSVersion))
                        retVal.AppendFormat("<tr><td align=\"right\">{0}</td><td>{1}</td></tr>", "Required OS Version:", er.OSVersion);
                    if (!String.IsNullOrEmpty(er.PlatformVersion))
                        retVal.AppendFormat("<tr><td align=\"right\">{0}</td><td>{1}</td></tr>", "Required Platform Version:", er.PlatformVersion);
                    if (er.ProcessorCount > 0)
                        retVal.AppendFormat("<tr><td align=\"right\">{0}</td><td>{1}</td></tr>", "Required Processor Count:", er.ProcessorCount);
                }

                foreach (IRule ir in ip.RuleSet.Rules.Where(r => r.GetType() == typeof(ADNSFLimitsRule)))
                {
                    ADNSFLimitsRule lr = ir as ADNSFLimitsRule;
                    if (lr.CategoryLimit > 0)
                        retVal.AppendFormat("<tr><td align=\"right\">{0}</td><td>{1}</td></tr>", "Category Limit:", lr.CategoryLimit);
                    if (lr.CustomerLimit > 0)
                        retVal.AppendFormat("<tr><td align=\"right\">{0}</td><td>{1}</td></tr>", "Customer Limit:", lr.CustomerLimit);
                    if (lr.OrderLimit > 0)
                        retVal.AppendFormat("<tr><td align=\"right\">{0}</td><td>{1}</td></tr>", "Order Limit:", lr.OrderLimit);
                    if (lr.ProductLimit > 0)
                        retVal.AppendFormat("<tr><td align=\"right\">{0}</td><td>{1}</td></tr>", "Product Limit:", lr.ProductLimit);
                    if (lr.SectionLimit > 0)
                        retVal.AppendFormat("<tr><td align=\"right\">{0}</td><td>{1}</td></tr>", "Section Limit:", lr.SectionLimit);
                }


            }
            retVal.Append("</table>");
            retVal.AppendLine("<br /><br />");
            return retVal.ToString();
        }

        protected void loadLocalizationInformation()
        {
            ltWebConfigLocaleSetting.Text = Localization.GetDefaultLocale();
            ltSQLLocaleSetting.Text = Localization.GetSqlServerLocale();
            ltCustomerLocaleSetting.Text = LocaleSetting;
            ltLocalizationCurrencyCode.Text = AppLogic.AppConfig("Localization.StoreCurrency");
            ltLocalizationCurrencyNumericCode.Text = AppLogic.AppConfig("Localization.StoreCurrencyNumericCode");
        }

        protected void loadGatewayInformation()
        {
            ltPaymentGateway.Text = AppLogic.ActivePaymentGatewayRAW();
            ltUseLiveTransactions.Text = (AppLogic.AppConfigBool("UseLiveTransactions") ? AppLogic.GetString("admin.splash.aspx.20", SkinID, LocaleSetting) : AppLogic.GetString("admin.splash.aspx.21", SkinID, LocaleSetting));
            ltTransactionMode.Text = AppLogic.AppConfig("TransactionMode").ToString();
            ltPaymentMethods.Text = AppLogic.AppConfig("PaymentMethods").ToString();
            ltPrimaryCurrency.Text = Localization.GetPrimaryCurrency();
        }

        protected void loadShippingInformation()
        {
            ltShippingCalculation.Text = DB.GetSqlS("select Name as S from dbo.ShippingCalculation where Selected=1");
            ltOriginState.Text = AppLogic.AppConfig("RTShipping.OriginState");
            ltOriginZip.Text = AppLogic.AppConfig("RTShipping.OriginZip");
            ltOriginCountry.Text = AppLogic.AppConfig("RTShipping.OriginCountry");
            ltFreeShippingThreshold.Text = AppLogic.AppConfigNativeDecimal("FreeShippingThreshold").ToString();
            ltFreeShippingMethods.Text = AppLogic.AppConfig("ShippingMethodIDIfFreeShippingIsOn");
            ltFreeShippingRateSelection.Text = CommonLogic.IIF(AppLogic.AppConfigBool("FreeShippingAllowsRateSelection"),"On","Off");
        }

    }
}
