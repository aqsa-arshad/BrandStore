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
using System.Text;

#endregion

namespace AspDotNetStorefront.Promotions
{
	public interface IDataLookupContext
	{
		#region Properties

		LookupType LookupType { get; set; }

		Int32 PromotionId { get; set; }

		Int32 CustomerId { get; set; }

        Int32 ShippingMethodId { get; set; }

		DateType StartDateType { get; set; }

		DateType EndDateType { get; set; }

		DateTime CustomStartDate { get; set; }

		DateTime CustomEndDate { get; set; }

		String[] Skus { get; set; }

        Int32[] ProductIds { get; set; }

        String[] GiftSkus { get; set; }

        Int32[] GiftProductIds { get; set; }
		#endregion
	}

	public class SimpleDataLookupContext : IDataLookupContext
	{
		#region Properties

		public LookupType LookupType { get; set; }

		public Int32 PromotionId { get; set; }

		public Int32 CustomerId { get; set; }

        public Int32 ShippingMethodId { get; set; }

		public DateType StartDateType { get; set; }

		public DateType EndDateType { get; set; }

		public DateTime CustomStartDate { get; set; }

		public DateTime CustomEndDate { get; set; }

		public String[] Skus { get; set; }

        public Int32[] ProductIds { get; set; }

        public String[] GiftSkus { get; set; }

        public Int32[] GiftProductIds { get; set; }
		#endregion
	}

	public enum DateType
	{
		Unspecified,
		CurrentDate,
		CustomDate,
		LastPromotionUsage,
	}
	
	public enum LookupType
	{
		TotalPromotionUses,
		TotalPromotionDiscounts,
		TotalOrders,
		TotalOrderAmount,
		TotalProductOrdered,
		TotalProductOrderedAmount,
		TotalPromotions,
		LastPromotionUsage,
        TotalCostForSkus,
	}
}
