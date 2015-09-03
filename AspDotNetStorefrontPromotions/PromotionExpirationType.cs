// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;

namespace AspDotNetStorefront.Promotions
{
	/// <summary>
	///  The type of calculation to use when determining if the promotion has expired.
	/// </summary>
	public enum PromotionExpirationType : byte
	{
		/// <summary>
		///  The promotion has no expiration date.
		/// </summary>
		NoExpiration,

		/// <summary>
		///  Expires after a set date.
		/// </summary>
		ExpirationDate,

		/// <summary>
		///  Expires after a set number of uses.
		/// </summary>
		NumberOfUses,

		/// <summary>
		///  Expires after a set number of uses by specific customers.
		/// </summary>
		NumberOfUsesPerCustomer,

		/// <summary>
		///  Expires after the total discounts incurred have been reached.
		/// </summary>
		TotalDiscount,

		/// <summary>
		///  Expires after the total discounts incurred for this customer have been reached.
		/// </summary>
		TotalDiscountPerCustomer,
	}
}
