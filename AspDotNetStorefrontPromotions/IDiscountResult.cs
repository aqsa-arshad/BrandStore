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
	public interface IDiscountResult
	{
		#region Properties

		IPromotion Promotion { get; set; }

		DiscountType DiscountType { get; set; }
		
		Decimal LineItemTotal { get; set; }
		
		Decimal ShippingTotal { get; set; }
		
		Decimal OrderTotal { get; set; }

		Decimal TotalDiscount { get; }

        Decimal GiftProductTotal { get; set; }

        PromotionDiscountBase.PromotionSequence SequenceType { get; set; }

        List<DiscountedItem> DiscountedItems { get; set; }

		#endregion
	}

	#region SimpleRuleContext

	[Serializable]
	public class SimpleDiscountResult : IDiscountResult
	{
		#region Properties

		public IPromotion Promotion { get; set; }

		public DiscountType DiscountType { get; set; }

		public Decimal LineItemTotal { get; set; }

		public Decimal ShippingTotal { get; set; }
		
		public Decimal OrderTotal { get; set; }

        public Decimal GiftProductTotal { get; set; }

        public PromotionDiscountBase.PromotionSequence SequenceType { get; set; }

        public List<DiscountedItem> DiscountedItems { get; set; }

		public Decimal TotalDiscount
		{
            get { return this.LineItemTotal + this.ShippingTotal + this.OrderTotal + this.GiftProductTotal; }
		}


		#endregion
	}

    public class DiscountedItem
    {
        public Int32 ShoppingCartRecordId { get; set; }
        public Int32 ProductId { get; set; }
        public Int32 VariantId { get; set; }
        public String Sku { get; set; }
        public Int32 Quantity { get; set; }
        public Decimal CartPrice { get; set; }
        public Decimal Subtotal { get; set; }
        public DiscountType DiscountType { get; set; }
        public Decimal GiftAmount { get; set; }
        public Decimal DiscountAmount { get; set; }
        public Boolean IsAGift { get; set; }
        public Decimal DiscountPercentage { get; set; }
        public IPromotionUsage PromotionUsage { get; set; }
    }
	#endregion
}
