// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;

namespace GatewayAuthorizeNet
{
	public class AddressPaymentProfileMap
	{
		public long AuthorizeNetProfileId { get; protected set; }
		public string CardType { get; protected set; }
		public string ExpirationMonth { get; protected set; }
		public string ExpirationYear { get; protected set; }
		public int AddressId { get; protected set; }

		public AddressPaymentProfileMap(long authorizeNetProfileId, string cardType, string expirationMonth, string expirationYear, int addressId)
		{
			AuthorizeNetProfileId = authorizeNetProfileId;
			CardType = cardType;
			ExpirationMonth = expirationMonth;
			ExpirationYear = expirationYear;
			AddressId = addressId;
		}
	}
}
