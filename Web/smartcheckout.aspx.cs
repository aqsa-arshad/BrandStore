// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways;
using System.Data.SqlClient;
using System.Collections.Generic;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore.ShippingCalculation;


namespace AspDotNetStorefront
{
	public partial class smartcheckout : SkinBase
	{

		/// <summary>
		/// Sets that this page requires a scriptmanager
		/// </summary>
		public override bool RequireScriptManager
		{
			get
			{
				return true;
			}
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			bool phoneCustomer = ((HttpContext.Current.Items["IsBeingImpersonated"] != null) && ((string)HttpContext.Current.Items["IsBeingImpersonated"] == "true"));
			if (phoneCustomer)
			{
				Response.Redirect("checkoutshipping.aspx");
			}

            SectionTitle = AppLogic.GetString("smartcheckout.aspx.164", SkinID, Customer.Current.LocaleSetting);

			Response.CacheControl = "private";
			Response.Expires = -1;
			Response.AddHeader("pragma", "no-cache");

			RequireSecurePage();
			ThisCustomer.RequireCustomerRecord();

			if (!Page.IsPostBack)
            {
                ((SkinBase)Page).OPCShoppingCart.ValidProceedCheckout(); // will not come back from this if any issue. they are sent back to the cart page!
			}

			if (AppLogic.AppConfigBool("PayPal.Express.UseIntegratedCheckout"))
				ltPayPalIntegratedCheckout.Text = AspDotNetStorefrontGateways.Processors.PayPalController.GetExpressCheckoutIntegratedScript(false);
		}
	}
}
