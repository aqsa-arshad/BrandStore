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
    public delegate int AddToCartDelegate(int productId, int variantId, int quantity);

	public interface IRuleContext
	{
        String Code { get; set; }
        Int32 PromotionId { get; set; }
        Boolean FilterByStore { get; set; }
        Int32 StoreId { get; set; }
        Int32 CustomerId { get; set; }
        Boolean IsRegistered { get; set; }
        Int32 ShippingMethodId { get; set; }
        Int32 ShippingAddressId { get; set; }
        Int32 BillingAddressId { get; set; }
        Decimal ShippingTotal { get; set; }
        Int32 CustomerLevel { get; set; }
        String EmailAddress { get; set; }
        String State { get; set; }
        String[] ExcludeStates { get; set; }
        String ZipCode { get; set; }
        String CountryCode { get; set; }
		IEnumerable<ShoppingCartItem> ShoppingCartItems { get; }
        Decimal SubTotal { get; set; }
        Boolean MultiShipEnabled { get; set; }
        Int32? CartType { get; set; }

        AddToCartDelegate AddItemToCart { get; set; }
	}

	[Serializable]
	public class SimpleRuleContext : IRuleContext
	{
        public String Code { get; set; }
		public Int32 PromotionId { get; set; }
        public Boolean FilterByStore { get; set; }
        public Int32 StoreId { get; set; }
		public Int32 CustomerId { get; set; }
        public Boolean IsRegistered { get; set; }
        public Int32 ShippingMethodId { get; set; }
        public Int32 ShippingAddressId { get; set; }
        public Int32 BillingAddressId { get; set; }
        public Decimal ShippingTotal { get; set; }
        public Int32 CustomerLevel { get; set; }		
		public String EmailAddress { get; set; }  
    	public String State { get; set; }
        public String[] ExcludeStates { get; set; }	
		public String ZipCode { get; set; }		
		public String CountryCode { get; set; }
        public IEnumerable<ShoppingCartItem> ShoppingCartItems { get; set; }
        public Decimal SubTotal { get; set; }
        public Boolean MultiShipEnabled { get; set; }
        public Int32? CartType { get; set; }

        public AddToCartDelegate AddItemToCart { get; set; }

		public SimpleRuleContext()
		{ }

		public SimpleRuleContext(IRuleContext ruleContext)
		{
			Code = ruleContext.Code;
			PromotionId = ruleContext.PromotionId;
			FilterByStore = ruleContext.FilterByStore;
			StoreId = ruleContext.StoreId;
			CustomerId = ruleContext.CustomerId;
            IsRegistered = ruleContext.IsRegistered;
			ShippingMethodId = ruleContext.ShippingMethodId;
			ShippingAddressId = ruleContext.ShippingAddressId;
			ShippingTotal = ruleContext.ShippingTotal;
			CustomerLevel = ruleContext.CustomerLevel;
			EmailAddress = ruleContext.EmailAddress;
			State = ruleContext.State;
			ExcludeStates = ruleContext.ExcludeStates;
			ZipCode = ruleContext.ZipCode;
			CountryCode = ruleContext.CountryCode;
			ShoppingCartItems = ruleContext.ShoppingCartItems;
			SubTotal = ruleContext.SubTotal;
            MultiShipEnabled = ruleContext.MultiShipEnabled;
            CartType = ruleContext.CartType;
		}
	}

    public class ShoppingCartItem
    {
        public Int32 ShoppingCartRecordId { get; set; }
        public Int32[] CategoryIds { get; set; }
        public Int32[] SectionIds { get; set; }
        public Int32[] ManufacturerIds { get; set; }
        public Int32 ProductId { get; set; }
        public Int32 VariantId { get; set; }
        public String Name { get; set; }
        public String Sku { get; set; }
        public Int32 Quantity { get; set; }
        public Decimal CartPrice { get; set; }
        public Decimal Subtotal { get; set; }
        public Subscription Subscription { get; set; }
        public bool IsGift { get; set; }
    }

    public class Subscription
    {
        public Int32 Interval { get; set; }
        public Int32 IntervalType { get; set; }
    }
}
