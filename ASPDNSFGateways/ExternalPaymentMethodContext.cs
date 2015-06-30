// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AspDotNetStorefrontGateways
{
	public class ExternalPaymentMethodContext
	{
		public string Result { get; protected set; }
		public string RedirectUrl { get; protected set; }
		public Dictionary<string, string> RedirectParameters { get; protected set; }

		public ExternalPaymentMethodContext(string result)
		{
			Result = result;
		}

		public ExternalPaymentMethodContext(string result, string redirectUrl, Dictionary<string, string> redirectParameters)
		{
			Result = result;
			RedirectUrl = redirectUrl;
			RedirectParameters = redirectParameters;
		}
	}
}
