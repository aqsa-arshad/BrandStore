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
using System.Xml.Serialization;
using AspDotNetStorefront.Promotions.Data;

#endregion

namespace AspDotNetStorefront.Promotions
{
    public interface IPromotionDiscount
    {
        int SequenceNumber { get; }
		IDiscountResult ApplyPromotion(IPromotionUsage promotionUsage, IRuleContext ruleContext, IDiscountContext discountContext, Func<IDiscountResult> resultFactory);
    }

    [Serializable]
    [XmlInclude(typeof(OrderPromotionDiscount))]
    [XmlInclude(typeof(OrderItemPromotionDiscount))]
    [XmlInclude(typeof(ShippingPromotionDiscount))]
    [XmlInclude(typeof(GiftProductPromotionDiscount))]
    public abstract class PromotionDiscountBase : IPromotionDiscount
    {        
        public enum PromotionSequence
        {
            Gift = 0
            , OrderItem
            , Order
            , Shipping
        }
        public abstract int SequenceNumber { get; }
		public abstract IDiscountResult ApplyPromotion(IPromotionUsage promotionUsage, IRuleContext ruleContext, IDiscountContext discountContext, Func<IDiscountResult> resultFactory);
    }

    #region DiscountType

    /// <summary>
    ///  The type of calculation to use when applying a promotion discount.
    /// </summary>
    public enum DiscountType
    {
        /// <summary>
        ///  Discounts the original amount with a flat dollar amount.
        /// </summary>
        Fixed,

        /// <summary>
        ///  Discounts the original amount with a percentage off.
        /// </summary>
        Percentage,
    }

    #endregion

    #region OrderPromotionDiscount

    [Serializable]
    public class OrderPromotionDiscount : PromotionDiscountBase
    {
        #region Properties

        public DiscountType DiscountType { get; set; }
        public Decimal DiscountAmount { get; set; }
        public override int SequenceNumber
        {
            get { return (int)PromotionSequence.Order; }
        }

        #endregion

        #region Public Methods

		public override IDiscountResult ApplyPromotion(IPromotionUsage promotionUsage, IRuleContext ruleContext, IDiscountContext discountContext, Func<IDiscountResult> resultFactory)
        {
            IDiscountResult retVal = resultFactory();
            retVal.DiscountType = this.DiscountType;
            retVal.SequenceType = (PromotionSequence)SequenceNumber;

            switch (DiscountType)
            {
                case DiscountType.Fixed:
                    retVal.OrderTotal = -this.DiscountAmount;
                    break;
                case DiscountType.Percentage:
                    retVal.OrderTotal = -discountContext.OrderTotal * this.DiscountAmount;
                    break;
            }

            if (discountContext.OrderTotal + retVal.OrderTotal < 0)
                retVal.OrderTotal = discountContext.OrderTotal * -1;

            return retVal;
        }

        #endregion
    }

    #endregion

    #region OrderItemPromotionDiscount

    [Serializable]
    public class OrderItemPromotionDiscount : PromotionDiscountBase
    {
        #region Properties

        public DiscountType DiscountType { get; set; }
        public Decimal DiscountAmount { get; set; }
        public override int SequenceNumber
        {
            get { return (int)PromotionSequence.OrderItem; }
        }
        #endregion

        #region Public Methods

		public override IDiscountResult ApplyPromotion(IPromotionUsage promotionUsage, IRuleContext ruleContext, IDiscountContext discountContext, Func<IDiscountResult> resultFactory)
        {
            IDiscountResult retVal = resultFactory();
            retVal.DiscountType = this.DiscountType;
            retVal.SequenceType = (PromotionSequence)SequenceNumber;

            List<DiscountedItem> discountedItems = new List<DiscountedItem>();
            Decimal lineItemTotal = decimal.Zero;

            foreach (DiscountableItem item in discountContext.DiscountableItems)
            {
                DiscountedItem discountedItem = new DiscountedItem();
                discountedItem.CartPrice = item.CartPrice;
                discountedItem.IsAGift = false;
                discountedItem.ProductId = item.ProductId;
                discountedItem.Quantity = item.Quantity;
                discountedItem.ShoppingCartRecordId = item.ShoppingCartRecordId;
                discountedItem.Sku = item.Sku;
                discountedItem.Subtotal = item.Subtotal;
                discountedItem.VariantId = item.VariantId;
                discountedItem.DiscountPercentage = decimal.Zero;
                discountedItem.DiscountType = this.DiscountType;
                discountedItem.PromotionUsage = promotionUsage;
                
                switch (DiscountType)
                {
                    case DiscountType.Fixed:
                        Decimal maximumDiscount = this.DiscountAmount;
                        if (this.DiscountAmount > discountedItem.CartPrice)
                            maximumDiscount = discountedItem.CartPrice;

                        Decimal fixedDiscount = -(maximumDiscount) * item.Quantity;
                        lineItemTotal += fixedDiscount;
                        discountedItem.DiscountAmount = fixedDiscount;
                        break;
                    case DiscountType.Percentage:
                        Decimal percentDiscount = -item.Subtotal * this.DiscountAmount;
                        lineItemTotal += percentDiscount;
                        discountedItem.DiscountAmount = percentDiscount;
                        discountedItem.DiscountPercentage = this.DiscountAmount;
                        break;
                }
                
                Data.ContextController.TrackLineItemDiscount(discountedItem);
                discountedItems.Add(discountedItem);
            }
            retVal.LineItemTotal = lineItemTotal;
            retVal.DiscountedItems = discountedItems;
            return retVal;
        }

        #endregion
    }

    #endregion

    #region ShippingPromotionDiscount

    [Serializable]
    public class ShippingPromotionDiscount : PromotionDiscountBase
    {
		public int[] ShippingMethodIds { get; set; }
		public DiscountType DiscountType { get; set; }
        public Decimal DiscountAmount { get; set; }
        public override int SequenceNumber
        {
            get { return (int)PromotionSequence.Shipping; }
        }

		private ShippingPromotionDiscount()
		{
			// Parameterless constructor for serialization support only
			ShippingMethodIds = new int[0];
		}

		public ShippingPromotionDiscount(DiscountType discountType, decimal discountAmount, IEnumerable<int> shippingMethodIds)
		{
			DiscountType = discountType;
			DiscountAmount = discountAmount;
			ShippingMethodIds = (shippingMethodIds ?? Enumerable.Empty<int>()).ToArray();
		}

		public override IDiscountResult ApplyPromotion(IPromotionUsage promotionUsage, IRuleContext ruleContext, IDiscountContext discountContext, Func<IDiscountResult> resultFactory)
        {
            IDiscountResult retVal = resultFactory();
            retVal.DiscountType = this.DiscountType;
            retVal.SequenceType = (PromotionSequence)SequenceNumber;

			if(ShippingMethodIds.Any() && !ShippingMethodIds.Contains(ruleContext.ShippingMethodId))
			    return retVal;

            switch (DiscountType)
            {
                case DiscountType.Fixed:
                    retVal.ShippingTotal = -this.DiscountAmount;
                    break;
                case DiscountType.Percentage:
                    retVal.ShippingTotal = -discountContext.ShippingTotal * this.DiscountAmount;
                    break;
            }

            if (discountContext.ShippingTotal + retVal.ShippingTotal < 0)
                retVal.ShippingTotal = discountContext.ShippingTotal * -1;

            return retVal;
        }
    }

    #endregion

    #region FreeProductPromotionDiscount

    [Serializable]
    public class GiftProductPromotionDiscount : PromotionDiscountBase
    {
        #region Properties
        public Int32 PromotionId { get; set; }
        public String[] GiftSkus { get; set; }
        public Int32[] GiftProductIds { get; set; }
        public Boolean MatchQuantities { get; set; }
        public Decimal GiftDiscountPercentage { get; set; }
        public Boolean ShowInProductTab { get; set; }
        public override int SequenceNumber
        {
            get { return (int)PromotionSequence.Gift; }
        }
        #endregion

        #region Public Methods

		public override IDiscountResult ApplyPromotion(IPromotionUsage promotionUsage, IRuleContext ruleContext, IDiscountContext discountContext, Func<IDiscountResult> resultFactory)
        {
            Data.EntityContextDataContext context = new Data.EntityContextDataContext();
            IDiscountResult retVal = resultFactory();
            Decimal totalDiscount = Decimal.Zero;
            List<DiscountedItem> discountedItems = new List<DiscountedItem>();
            Int32 quantity = 1;
            Int32 contextQuantity = discountContext.DiscountableItems.Sum(s => s.Quantity);
            Int32 shoppingCartRecordId = 0;
            if (MatchQuantities && contextQuantity > 0)
                quantity = discountContext.DiscountableItems.Sum(s => s.Quantity);

            foreach (Int32 productId in GiftProductIds)
            {
                Data.ShoppingCart cart = null;

                //Try to find an existing free gift in the promotionlineitem table for this promousage
                PromotionLineItem giftItem = context.PromotionLineItems.FirstOrDefault(pli => pli.productId == productId && pli.isAGift && pli.PromotionUsageId == promotionUsage.Id);

                //Try to grab the shopping cart item for the promolineitem
                if (giftItem != null)
                    cart = context.ShoppingCarts.FirstOrDefault(sc => sc.ShoppingCartRecID == giftItem.shoppingCartRecordId);

                //Add the free item to the shoppingcart if it doesn't already exist
                if (cart == null)
                {
                    int variantId = context.ProductVariants.FirstOrDefault(pv => pv.ProductID == productId && pv.IsDefault == 1).VariantID;
                    if (ruleContext.AddItemToCart != null)
                        shoppingCartRecordId = ruleContext.AddItemToCart(productId, variantId, quantity);
                    cart = context.ShoppingCarts.FirstOrDefault(sc => sc.ShoppingCartRecID == shoppingCartRecordId);
                }
                else
                {
                    //Make sure our quantities match up.
                    cart.Quantity = quantity;
                    context.SubmitChanges();
                }

                if (cart != null)
                {
                    DiscountedItem discountedItem = new DiscountedItem();

                    //We store the original price of the item in the promotionlineitem table, we want to use that original price if we already have a promo line item                    
                    discountedItem.CartPrice = giftItem == null ? (decimal)cart.ProductPrice : giftItem.cartPrice;
                    discountedItem.IsAGift = true;
                    discountedItem.ProductId = productId;
                    discountedItem.Quantity = cart.Quantity;
                    discountedItem.ShoppingCartRecordId = cart.ShoppingCartRecID;
                    discountedItem.Sku = cart.ProductSKU;
                    discountedItem.Subtotal = (decimal)cart.ProductPrice * cart.Quantity;
                    discountedItem.VariantId = cart.VariantID;
                    discountedItem.PromotionUsage = promotionUsage;

                    decimal discount = -(discountedItem.CartPrice) * (GiftDiscountPercentage * .01m);
                    //Make sure our price won't go negative
                    discount = Math.Abs(discount) > discountedItem.CartPrice ? -(discountedItem.CartPrice) : discount;
                    
                    //The discount is already baked into the cart price we will record the discount in gift amount instead
                    discountedItem.DiscountAmount = 0.0M;
                    discountedItem.GiftAmount = discount * cart.Quantity;
                    totalDiscount += discountedItem.GiftAmount;
                    discountedItem.DiscountPercentage = GiftDiscountPercentage;

                    Data.ContextController.TrackLineItemDiscount(discountedItem); 

                    discountedItems.Add(discountedItem);                    
                    cart.ProductPrice = discountedItem.CartPrice + discount;
                    cart.IsGift = true;
                    cart.CustomerEntersPrice = 1;
                    context.SubmitChanges();
                }

            }
            retVal.DiscountedItems = discountedItems;
            retVal.DiscountType = DiscountType.Fixed;
            retVal.GiftProductTotal = totalDiscount;
            retVal.SequenceType = (PromotionSequence)SequenceNumber;

            return retVal;
        }

        #endregion

    }


    #endregion
}
