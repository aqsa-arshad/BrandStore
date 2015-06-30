// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for secureauth.
    /// 3-D Secure processing.
    /// </summary>
    public partial class secureauthHSBC : System.Web.UI.Page
    {
        private void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;
            ThisCustomer.RequireCustomerRecord();

            Response.Cache.SetAllowResponseInBrowserHistory(false);
            int CustomerID = ThisCustomer.CustomerID;
            Address UseBillingAddress = new Address();
            UseBillingAddress.LoadFromDB(ThisCustomer.PrimaryBillingAddressID);

            Response.Write("<HTML>\n");
            Response.Write("<BODY onLoad=\"document.frmLaunchACS.submit();\">\n");
            Response.Write("<center>\n");
            Response.Write("<FORM name=\"frmLaunchACS\" method=\"Post\" action=\"" + AppLogic.AppConfig("HSBC.CcpaURL") + "\">\n");
            Response.Write("<noscript>\n");
            Response.Write("	<br><br>\n");
            Response.Write("	<center>\n");
            Response.Write("	<font color=\"red\">\n");
            Response.Write("	<h1>" + AppLogic.GetString("secureauth.aspx.2", 1, Localization.GetDefaultLocale()) + "</h1>\n");
            Response.Write("	<h2>" + AppLogic.GetString("secureauth.aspx.3", 1, Localization.GetDefaultLocale()) + "<br></h2>\n");
            Response.Write("	<h3>" + AppLogic.GetString("secureauth.aspx.4", 1, Localization.GetDefaultLocale()) + "</h3>\n");
            Response.Write("	</font>\n");
            Response.Write("	<input type=\"submit\" value=\"" + AppLogic.GetString("secureauth.aspx.5", 1, Localization.GetDefaultLocale()) + "\">\n");
            Response.Write("	</center>\n");
            Response.Write("</noscript>\n");
            if (UseBillingAddress.CardExpirationYear.ToString().Length < 2)
                return;
            Response.Write("<input type=hidden name=\"CardExpiration\" value=\"" + UseBillingAddress.CardExpirationYear.ToString().Substring(2, 2) + UseBillingAddress.CardExpirationMonth.PadLeft(2, '0') + "\">\n");
            Response.Write("<input type=hidden name=\"CardholderPan\" value=\"" + UseBillingAddress.CardNumber + "\">\n");
            Response.Write("<input type=hidden name=\"CcpaClientId\" value=\"" + AppLogic.AppConfig("HSBC.CcpaClientID") + "\">\n");
            Response.Write("<input type=hidden name=\"CurrencyExponent\" value=\"2\">\n");
            Response.Write("<input type=hidden name=\"PurchaseCurrency\" value=\"" + Localization.StoreCurrencyNumericCode() + "\">\n");
            Response.Write("<input type=hidden name=\"PurchaseAmount\" value=\"" + ThisCustomer.ThisCustomerSession["3DSecure.HSBCAmount"] + "\">\n");
            Response.Write("<input type=hidden name=\"PurchaseAmountRaw\" value=\"" + ThisCustomer.ThisCustomerSession["3DSecure.HSBCAmountRaw"] + "\">\n");
            Response.Write("<input type=hidden name=\"ResultUrl\" value=\"" + AppLogic.GetStoreHTTPLocation(true) + "secureprocessHSBC.aspx\">\n");
            Response.Write("<input type=hidden name=\"MD\" value=\"" + ThisCustomer.ThisCustomerSession["3DSecure.MD"] + "\">\n");
            Response.Write("</FORM>\n");
            Response.Write("</center>\n");
            Response.Write("</BODY>\n");
            Response.Write("</HTML>\n");
        }
    }
}
