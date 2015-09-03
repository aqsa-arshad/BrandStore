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

namespace GatewayMoneybookers
{
	class CreditCardPaymentMethod : PaymentMethod
	{
		public override string Name { get { return "Credit Card"; } }
		public override string Code { get { return "CC"; } }

		/// <param name="holder">Must not be null</param>
		/// <param name="number">Must not be null</param>
		/// <param name="brand">Must not be null</param>
		/// <param name="expiryMonth">Must not be null</param>
		/// <param name="expiryYear">Must not be null</param>
		/// <param name="cardIssueNumber">Can be null</param>
		/// <param name="verification">Must not be null</param>
		public CreditCardPaymentMethod(
			AccountHolder holder,
			AccountNumber number,
			AccountBrand brand,
			AccountExpiryMonth expiryMonth,
			AccountExpiryYear expiryYear,
			AccountCardIssueNumber cardIssueNumber,
			AccountVerification verification)
		{
			Holder = holder;
			Number = number;
			Brand = brand;
			ExpiryMonth = expiryMonth;
			ExpiryYear = expiryYear;
			CardIssueNumber = cardIssueNumber;
			Verification = verification;
		}

		// Provided for reference payment methods
		protected CreditCardPaymentMethod()
		{ }
	}
}
