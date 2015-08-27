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
	public interface IExternalPaymentMethod
	{
		ExternalPaymentMethodContext BeginCheckout(AspDotNetStorefrontCore.ShoppingCart cart);
		ExternalPaymentMethodContext ProcessCallback(Dictionary<string, string> responseData);
	}
}
