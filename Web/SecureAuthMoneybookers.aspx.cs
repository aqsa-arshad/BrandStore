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
    /// 3-D Secure processing.
    /// </summary>
	public partial class SecureAuthMoneybookers : System.Web.UI.Page
    {
		protected string FormPostUrl { get; set; }

        private void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");
			Response.Cache.SetAllowResponseInBrowserHistory(false);

            Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;
            ThisCustomer.RequireCustomerRecord();
            int CustomerID = ThisCustomer.CustomerID;

			Address UseBillingAddress = new Address();
            UseBillingAddress.LoadFromDB(ThisCustomer.PrimaryBillingAddressID);

			CustomerSession customerSession = new CustomerSession(CustomerID);
			FormPostUrl = customerSession["Moneybookers_3DSecure_RedirectUrl"];
			System.Text.StringBuilder formFields = new System.Text.StringBuilder();

			foreach(var key in customerSession["Moneybookers_3DSecure_ParameterKeys"].Split(';'))
				formFields.AppendFormat("<input type='hidden' name='{0}' value='{1}' />\r\n", key.Substring("Moneybookers_3DSecure_Parameter_".Length), customerSession[key]);

			litFormFields.Text = formFields.ToString();
		}
    }
}
