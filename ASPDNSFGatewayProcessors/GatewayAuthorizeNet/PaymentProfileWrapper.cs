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

namespace GatewayAuthorizeNet
{
	public class PaymentProfileWrapper
	{
		public long CustomerId { get; set; }
		public long ProfileId { get; set; }
		public string CreditCardNumberMasked { get; set; }
		public string CardType { get; set; }
		public string ExpirationMonth { get; set; }
		public string ExpirationYear { get; set; }
	}
}
