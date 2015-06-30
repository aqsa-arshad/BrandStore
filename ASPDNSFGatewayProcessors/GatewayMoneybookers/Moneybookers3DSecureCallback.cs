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
	public class Moneybookers3DSecureCallback : IHttpHandler
	{
		public bool IsReusable
		{
			get { return true; }
		}

		private string GetRedirectPage(string redirectUrl)
		{
			return String.Format(
				"<html><head><title>Please wait while your browser is redirected</title></head><body><script type='text/javascript'>top.location = '{0}';</script></body></html>",
				redirectUrl);
		}

		public void ProcessRequest(HttpContext context)
		{
			try
			{
				context.Response.CacheControl = "private";
				context.Response.Expires = 0;
				context.Response.AddHeader("pragma", "no-cache");
				context.Response.Cache.SetAllowResponseInBrowserHistory(false);

				string responseData = context.Request.Form["response"];

				if(responseData == null)
					throw new InvalidCastException("No response parameter present.");

				responseData = HttpUtility.UrlDecode(responseData);

				AspDotNetStorefrontGateways.Processors.Moneybookers moneybookers = new AspDotNetStorefrontGateways.Processors.Moneybookers();
				var result = moneybookers.Process3DSecureResponse(new Dictionary<string, string>
				{
					{ "response", responseData },
					{ "threedsecure_verificationpath", context.Request.Form["threedsecure_verificationpath"] },
				});

				context.Response.Write(GetRedirectPage(result.RedirectUrl));
			}
			catch
			{
				string errorMessage = AspDotNetStorefrontCore.AppLogic.GetString("checkoutpayment.aspx.34", AspDotNetStorefrontCore.Customer.Current.SkinID, AspDotNetStorefrontCore.Customer.Current.LocaleSetting);
				if(errorMessage == String.Empty)
					errorMessage = "There was an error processing your payment. Please try again.";

				string redirectUrl = AspDotNetStorefrontCore.AppLogic.GetStoreHTTPLocation(false) + "shoppingcart.aspx?ErrorMsg=" + HttpUtility.UrlEncode(errorMessage);
				context.Response.Write(GetRedirectPage(redirectUrl));
			}
		}
	}
}
