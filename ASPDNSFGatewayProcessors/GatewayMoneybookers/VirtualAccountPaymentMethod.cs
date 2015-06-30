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
	class VirtualAccountPaymentMethod : PaymentMethod
	{
		public override string Name { get { return "Virtual Account"; } }
		public override string Code { get { return "VA"; } }

		/// <param name="holder">Must not be null</param>
		public VirtualAccountPaymentMethod(
			AccountHolder holder,
			AccountId id)
		{
			Holder = holder;
			Id = id;
			Brand = new AccountBrand("MONEYBOOKERS");
		}

		// Provided for reference payment methods
		protected VirtualAccountPaymentMethod()
		{ }
	}
}
