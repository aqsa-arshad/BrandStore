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
using Vortx.MobileFramework;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for secureauth.
    /// 3-D Secure processing.
    /// </summary>
    public partial class mobilesecureauth : System.Web.UI.Page
    {
        private void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;

            MobileHelper.RedirectPageWhenMobileIsDisabled("~/PageNotFound.aspx", ThisCustomer);

            ThisCustomer.RequireCustomerRecord();

            //=====================================================================================\n");
            //= Easy Connect - secure Commerce (http://www.securecommerce.com)\n");
            //= ecauth.aspx\n");
            //=\n");
            //= Usage\n");
            //=		Form used to POST the payer authentication request to the Card Issuer Servers.\n");
            //=		The Card Issuer Servers will in turn display the payer authentication window\n");
            //=		to the consumer within this location.\n");
            //=\n");
            //=		Note that the form field names below are case sensitive. For additional information\n");
            //=		please see the integration documentation.\n");
            //=\n");
            //=====================================================================================\n");
            Response.Cache.SetAllowResponseInBrowserHistory(false);
            int CustomerID = ThisCustomer.CustomerID;
            if (ThisCustomer.ThisCustomerSession["3Dsecure.ACSURL"].Length == 0)
            {
                Response.Write("<HTML>\n");
                Response.Write("<BODY>\n");
                Response.Write("<center>" + AppLogic.GetString("secureauth.aspx.1", 1, Localization.GetDefaultLocale()) + "</center>\n");
                Response.Write("</BODY>\n");
                Response.Write("</HTML>\n");
            }
            else
            {
                Response.Write("<HTML>\n");
                Response.Write("<BODY onLoad=\"document.frmLaunchACS.submit();\">\n");
                Response.Write("<center>\n");
                //=====================================================================================\n");
                // The Inline Authentication window must be a minimum of 410 pixel width by\n");
                // 400 pixel height.\n");
                //=====================================================================================\n");
                Response.Write("<FORM name=\"frmLaunchACS\" method=\"Post\" action=\"" + ThisCustomer.ThisCustomerSession["3DSecure.ACSUrl"] + "\">\n");
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
                Response.Write("<input type=hidden name=\"PaReq\" value=\"" + ThisCustomer.ThisCustomerSession["3Dsecure.paReq"] + "\">\n");
                Response.Write("<input type=hidden name=\"TermUrl\" value=\"" + AppLogic.GetStoreHTTPLocation(true) + "secureprocess.aspx\">\n");
                Response.Write("<input type=hidden name=\"MD\" value=\"" + ThisCustomer.ThisCustomerSession["3DSecure.MD"] + "\">\n");
                Response.Write("</FORM>\n");
                Response.Write("</center>\n");
                Response.Write("</BODY>\n");
                Response.Write("</HTML>\n");
            }
        }
    }
}
