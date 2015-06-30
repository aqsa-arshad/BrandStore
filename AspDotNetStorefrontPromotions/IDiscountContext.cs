// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace AspDotNetStorefront.Promotions
{
	public interface IDiscountContext
	{
		Decimal LineItemTotal { get; }
		Decimal ShippingTotal { get; }		
		Decimal ShippingTaxTotal { get; }		
		Decimal OrderTaxTotal { get; }		
		Decimal OrderTotal { get; }
		IEnumerable<DiscountableItem> DiscountableItems { get; }
        Int32 CustomerId { get; }
        Int32 ShippingAddressId { get; }
        Int32 BillingAddressId { get; }
        Int32 CustomerLevelId { get; }
        Int32 StoreId { get; }
	}

    [Serializable]
	public class SimpleDiscountContext : IDiscountContext
	{
		public Decimal LineItemTotal { get; set; }
		public Decimal ShippingTotal { get; set; }		
		public Decimal ShippingTaxTotal { get; set; }			
		public Decimal OrderTaxTotal { get; set; }		
		public Decimal OrderTotal { get; set; }
        public IEnumerable<DiscountableItem> DiscountableItems { get; set; }
        public Int32 CustomerId { get; set; }
        public Int32 ShippingAddressId { get; set; }
        public Int32 BillingAddressId { get; set; }
        public Int32 CustomerLevelId { get; set; }
        public Int32 StoreId { get; set; }

		public SimpleDiscountContext()
		{ }

		public SimpleDiscountContext(IDiscountContext discountContext)
		{
			LineItemTotal = discountContext.LineItemTotal;
			ShippingTotal = discountContext.ShippingTotal;
			ShippingTaxTotal = discountContext.ShippingTaxTotal;
			OrderTaxTotal = discountContext.OrderTaxTotal;
			OrderTotal = discountContext.OrderTotal;
			DiscountableItems = discountContext.DiscountableItems;
			CustomerId = discountContext.CustomerId;
			ShippingAddressId = discountContext.ShippingAddressId;
			BillingAddressId = discountContext.BillingAddressId;
			CustomerLevelId = discountContext.CustomerLevelId;
			StoreId = discountContext.StoreId;
		}
	}

    public class DiscountableItem
    {
        public Int32 ShoppingCartRecordId { get; set; }
        public Int32 ProductId { get; set; }
        public Int32 VariantId { get; set; }
        public String Sku { get; set; }
        public Int32 Quantity { get; set; }
        public Decimal CartPrice { get; set; }
        public Decimal Subtotal { get; set; }
    }
}
