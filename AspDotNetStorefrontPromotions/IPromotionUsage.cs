// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
#region Using Directives

using System;
using System.Collections.Generic;
using System.Linq;
using AspDotNetStorefront.Promotions.Data;

#endregion

namespace AspDotNetStorefront.Promotions
{
	public interface IPromotionUsage 
	{
		Int32 Id { get; }
		Int32 PromotionId { get; }
        Promotion Promotion { get; } 
		Int32 CustomerId { get; }
		Int32? OrderId { get; }
		DateTime? DateApplied { get; }
		Decimal? DiscountAmount { get; }
		Boolean Complete { get; }
	}
}
