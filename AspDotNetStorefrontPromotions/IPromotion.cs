// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using AspDotNetStorefront.Promotions.Data;
using System.Data.Linq;

namespace AspDotNetStorefront.Promotions
{
	/// <summary>
	///  Contract defining the necessary members of a promotion.
	/// </summary>
	public interface IPromotion
	{
		/// <summary>
		///  The unique identifier of the promotion.
		/// </summary>
		Int32 Id { get; }

        /// <summary>
        ///  The long unique identifier of the promotion.
        /// </summary>
        Guid PromotionGuid { get; }

		/// <summary>
		///  The unique name of the promotion.
		/// </summary>
		String Name { get; }

		/// <summary>
		///  Customer friendly description of the promotion.
		/// </summary>
		String Description { get; }

        /// <summary>
        ///  Customer friendly usage text for the cart summary.
        /// </summary>
        String UsageText { get; }

		/// <summary>
		///  The unique code for this promotion.
		/// </summary>
		String Code { get; }

		/// <summary>
		///  The priority of a promotion determines what conflicting discounts get applied.
		/// </summary>
		Decimal Priority { get; }

		/// <summary>
		///  Flag to determine if the promotion can be applied or not.
		/// </summary>
		Boolean Active { get; }

		/// <summary>
		///  Flag to determine if the promotion system should automatically apply to valid customers.
		/// </summary>
		Boolean AutoAssigned { get; }
		
		/// <summary>
		///  A collection of rules to use when validating this promotion.
		/// </summary>
		List<PromotionRuleBase> PromotionRules { get; }

		/// <summary>
		///  A collection o discounts to apply when using this promotion.
		/// </summary>
		List<PromotionDiscountBase> PromotionDiscounts { get; }

        IEnumerable<PromotionStore> MappedStores { get; }

	}
}
