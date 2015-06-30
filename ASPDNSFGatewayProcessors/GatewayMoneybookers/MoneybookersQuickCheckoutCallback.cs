// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AspDotNetStorefrontGateways;

namespace GatewayMoneybookers
{
	public class MoneybookersQuickCheckoutCallback : IHttpHandler
	{
		public bool IsReusable
		{
			get { return true; }
		}

		public void ProcessRequest(HttpContext context)
		{
			try
			{
				var gateway = GatewayLoader.GetProcessor("Moneybookers");

				if(gateway == null)
                    throw new Exception("Skrill (Moneybookers) Quick Checkout requires the Moneybookers gateway.");

				var quickCheckout = ((IExternalPaymentMethodProvider)gateway).GetExternalPaymentMethod("Quick Checkout");

				if(quickCheckout == null)
                    throw new Exception("The installed version of the Skrill (Moneybookers) gateway does not support Quick Checkout.");

				string responseData = context.Request.Form["response"];

				if(responseData == null)
				{
					context.Response.Write("No response parameter present. Aborting.");
					return;
				}

				responseData = HttpUtility.UrlDecode(responseData);

				// If payment method is VA, then
				var result = quickCheckout.ProcessCallback(new Dictionary<string, string>
				{
					{ "response", responseData }
				});
				context.Response.Write(result.RedirectUrl);

				// otherwise
			}
			catch
			{
				string errorMessage = AspDotNetStorefrontCore.AppLogic.GetString("checkoutpayment.aspx.34", AspDotNetStorefrontCore.Customer.Current.SkinID, AspDotNetStorefrontCore.Customer.Current.LocaleSetting);
				if(errorMessage == String.Empty)
					errorMessage = "There was an error processing your payment. Please try again.";

				context.Response.Write(AspDotNetStorefrontCore.AppLogic.GetStoreHTTPLocation(false) + "shoppingcart.aspx?ErrorMsg=" + HttpUtility.UrlEncode(errorMessage));
			}
		}
	}
}
