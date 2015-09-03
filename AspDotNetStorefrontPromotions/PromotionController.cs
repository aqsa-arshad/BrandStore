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
	public interface IPromotionController
	{
		#region Methods

		IEnumerable<IPromotion> ValidatePromotions(IEnumerable<IPromotion> promotions, IRuleContext ruleContext, bool allowsPromotions);

		IEnumerable<IPromotionValidationResult> ValidatePromotion(IPromotion promotion, IRuleContext ruleContext, bool allowsPromotions);

		IDiscountResult ApplyPromotion(IPromotionUsage promotionUsage, IRuleContext ruleContext, IDiscountContext discountContext, Func<IDiscountResult> resultFactory, IPromotionController promotionController, bool allowsPromotions);

		IDiscountResult CombineDiscounts (IEnumerable<IDiscountResult> discounts, Func<IDiscountResult> resultFactory);

		IDataLookupResult LookupData (IDataLookupContext dataLookupContext);

		#endregion
	}

	/// <summary>
	///  The PromotionController is the primary means of validating and applying a promotion. 
	/// </summary>
	public class PromotionController : IPromotionController
	{
		#region Fields

		public delegate IDataLookupResult LookupDataDelegate (IDataLookupContext dataLookupContext);

		#endregion

		#region Properties

		public event LookupDataDelegate OnLookupData;

		#endregion

		#region Public Methods

		public IEnumerable<IPromotion> ValidatePromotions (IEnumerable<IPromotion> promotions, IRuleContext ruleContext, bool allowsPromotions)
		{
            return promotions.Where(p => ValidatePromotion(p, ruleContext, allowsPromotions).Any(vr => !vr.IsValid));
		}

        public IEnumerable<IPromotionValidationResult> ValidatePromotion(IPromotion promotion, IRuleContext ruleContext, bool allowsPromotions)// i need to return a validation message
		{
			if (promotion == null)
				throw new ArgumentNullException("promotion", "Promotions cannot be null when validating.");

			if (ruleContext == null)
				throw new ArgumentNullException("ruleContext", "A rules context must be specified when validating promotions.");

			if (!promotion.Active)
                return new[] { new SimplePromotionValidationResult(false, "Promotion.Reason.InactivePromotion") };

			if (!allowsPromotions)
				return new[] { new SimplePromotionValidationResult(false, "Promotion.Reason.CustomerLevelDoesNotAllow") };

            if (ruleContext.FilterByStore)
            {
                if (!promotion.MappedStores.Any(ps => ps.StoreID == ruleContext.StoreId))
                    return new[] { new SimplePromotionValidationResult(false, "Promotion.Reason.MultiStore.NoMatch") };
            }

            if (promotion.PromotionRules == null || promotion.PromotionDiscounts.Count() == 0)
            {
                return new[] { new SimplePromotionValidationResult(false, "Promotion.Reason.DoesNotExist") };
            }
            else
            {
                return promotion.PromotionRules.Select(r => r.Validate(this, ruleContext));
            }

		}

        public Boolean ValidateLoyaltyPromotion(IPromotion promotion, IRuleContext ruleContext)// i need to return a validation message
        {
            if (promotion == null)
                throw new ArgumentNullException("promotion", "Promotions cannot be null when validating.");

            if (ruleContext == null)
                throw new ArgumentNullException("ruleContext", "A rules context must be specified when validating promotions.");

            if (!promotion.Active)
                return false;

            if (promotion.PromotionRules == null || !promotion.PromotionRules.Any())
                return true;

            return promotion.PromotionRules.Where(p => 
                p.GetType() == typeof(CustomerLevelPromotionRule)
                || p.GetType() == typeof(EmailAddressPromotionRule)
                || p.GetType() == typeof(ZipCodePromotionRule)
                || p.GetType() == typeof(StatePromotionRule)
                || p.GetType() == typeof(CountryPromotionRule)
                || p.GetType() == typeof(MinimumOrderAmountPromotionRule) 
                || p.GetType() == typeof(MinimumOrdersPromotionRule) 
                || p.GetType() == typeof(MinimumProductAmountOrderedPromotionRule) 
                || p.GetType() == typeof(MinimumProductsOrderedPromotionRule)
                ).All(r => r.Validate(this, ruleContext).IsValid);
        }

		public IDiscountResult ApplyPromotion (IPromotionUsage promotionUsage, IRuleContext ruleContext, IDiscountContext discountContext, Func<IDiscountResult> resultFactory, IPromotionController promotionController, bool allowsPromotions)
		{
            if (!ValidatePromotion(promotionUsage.Promotion, ruleContext, allowsPromotions).All(vr => vr.IsValid))
				return null;

			IDiscountResult retVal = resultFactory();
            retVal.Promotion = promotionUsage.Promotion;
            List<DiscountedItem> discountedItems = new List<DiscountedItem>();
			foreach (IPromotionDiscount promotionDiscount in promotionUsage.Promotion.PromotionDiscounts)
			{
                IDiscountResult result = promotionDiscount.ApplyPromotion(promotionUsage, ruleContext, discountContext, resultFactory);
                if(result.DiscountedItems != null)
                    discountedItems.AddRange(result.DiscountedItems);

				retVal.LineItemTotal += result.LineItemTotal;
				retVal.ShippingTotal += result.ShippingTotal;
				retVal.OrderTotal += result.OrderTotal;
                retVal.GiftProductTotal += result.GiftProductTotal;
			}
            retVal.DiscountedItems = discountedItems;

			return retVal;
		}

		public IDiscountResult CombineDiscounts (IEnumerable<IDiscountResult> discounts, Func<IDiscountResult> resultFactory)
		{
			IDiscountResult result = resultFactory();

			if (discounts.Where(d => d.LineItemTotal < 0).Count() > 0)
			{
                decimal minLineItemTotalPriority = 0;
                if(discounts.Where(d => d.LineItemTotal < 0 && d.Promotion.Priority != 0).Any())
                    minLineItemTotalPriority = discounts.Where(d => d.LineItemTotal < 0 && d.Promotion.Priority != 0).Min(d => d.Promotion.Priority);
				result.LineItemTotal = discounts.Where(d => d.Promotion.Priority == minLineItemTotalPriority || d.Promotion.Priority == 0).Sum(d => d.LineItemTotal);
			}

			if (discounts.Where(d => d.ShippingTotal < 0).Count() > 0)
			{
                decimal minShippingTotalPriority = 0;
                if(discounts.Where(d => d.ShippingTotal < 0 && d.Promotion.Priority != 0).Any())
                    minShippingTotalPriority = discounts.Where(d => d.ShippingTotal < 0 && d.Promotion.Priority != 0).Min(d => d.Promotion.Priority);
                result.ShippingTotal = discounts.Where(d => d.Promotion.Priority == minShippingTotalPriority || d.Promotion.Priority == 0).Sum(d => d.ShippingTotal);
			}

			if (discounts.Where(d => d.OrderTotal < 0).Count() > 0)
			{
                decimal minOrderTotalPriority = 0;
                if(discounts.Where(d => d.OrderTotal < 0 && d.Promotion.Priority != 0).Any())
                    minOrderTotalPriority = discounts.Where(d => d.OrderTotal < 0 && d.Promotion.Priority != 0).Min(d => d.Promotion.Priority);
                result.OrderTotal = discounts.Where(d => d.Promotion.Priority == minOrderTotalPriority || d.Promotion.Priority == 0).Sum(d => d.OrderTotal);
			}

            if (discounts.Where(d => d.GiftProductTotal < 0).Count() > 0)
            {
                decimal minFreeProductPriority = 0;
                if(discounts.Where(d => d.GiftProductTotal < 0 && d.Promotion.Priority != 0).Any())
                    minFreeProductPriority = discounts.Where(d => d.GiftProductTotal < 0 && d.Promotion.Priority != 0).Min(d => d.Promotion.Priority);
                result.GiftProductTotal = discounts.Where(d => d.Promotion.Priority == minFreeProductPriority || d.Promotion.Priority == 0).Sum(d => d.GiftProductTotal);
            }

			return result;
		}

        public IDiscountResult CombineShippingDiscounts(IEnumerable<IDiscountResult> discounts, Func<IDiscountResult> resultFactory)
        {
            IDiscountResult result = resultFactory();

            if (discounts.Where(d => d.ShippingTotal < 0).Count() > 0)
            {
                decimal minShippingTotalPriority = 0;
                if (discounts.Where(d => d.ShippingTotal < 0 && d.Promotion.Priority != 0).Any())
                    minShippingTotalPriority = discounts.Where(d => d.ShippingTotal < 0 && d.Promotion.Priority != 0).Min(d => d.Promotion.Priority);
                result.ShippingTotal = discounts.Where(d => d.Promotion.Priority == minShippingTotalPriority || d.Promotion.Priority == 0).Sum(d => d.ShippingTotal);
            }

            return result;
        }

		public IDataLookupResult LookupData (IDataLookupContext dataLookupContext)
		{
			if (OnLookupData != null)
				return OnLookupData(dataLookupContext);

			return null;
		}

		#endregion
	}
}
