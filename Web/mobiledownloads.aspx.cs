// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Data;
using System.Globalization;
using System.Web.Security;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways;
using System.Data.SqlClient;
using System.Text;
using Vortx.MobileFramework;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for orderconfirmation.
    /// </summary>
	public partial class mobiledownloads : SkinBase
    {
        protected void Page_Load(object sender, System.EventArgs e)
        {

            MobileHelper.RedirectPageWhenMobileIsDisabled("~/downloads.aspx", ThisCustomer);

            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            RequireSecurePage();

            Address BillingAddress = new Address();

            // this may be overwridden by the XmlPackage below!
            SectionTitle = AppLogic.GetString("downloads.aspx.1", SkinID, ThisCustomer.LocaleSetting);
			Topic mobileDownloadTopic = new Topic("Download.MobilePageContent");
			litOutput.Text = mobileDownloadTopic.Contents;
          
        }
    }
}
