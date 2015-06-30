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
	abstract class PaymentMethod
	{
		/// <summary>
		/// Some payment types (e.g. Capture) don't need account details but do need to know the payment method. Use this instance in those cases.
		/// </summary>
		public static TPaymentMethod GetReferencedPaymentMethod<TPaymentMethod>()
			where TPaymentMethod : PaymentMethod
		{
			var type = typeof(TPaymentMethod);
			var constructor = type.GetConstructors(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

			if(constructor == null)
				throw new InvalidOperationException(String.Format("PaymentType \"{0}\" does not support reference transactions.", type));

			var instance = constructor[0].Invoke(null);

			return (TPaymentMethod)instance;
		}

		public abstract string Name { get; }
		public abstract string Code { get; }

		public AccountHolder Holder { get; protected set; }
		public AccountNumber Number { get; protected set; }
		public AccountBrand Brand { get; protected set; }
		public AccountId Id { get; protected set; }
		public AccountExpiryMonth ExpiryMonth { get; protected set; }
		public AccountExpiryYear ExpiryYear { get; protected set; }
		public AccountCardIssueNumber CardIssueNumber { get; protected set; }
		public AccountVerification Verification { get; protected set; }

		protected PaymentMethod()
		{ }
	}
}
