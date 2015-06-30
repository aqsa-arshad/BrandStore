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

#endregion

namespace AspDotNetStorefront.Promotions
{
    public interface IPromotionRule
    {
        IPromotionValidationResult Validate(IPromotionController promotionController, IRuleContext ruleContext);
    }

    #region PromotionValidationResult

    public interface IPromotionValidationResult
    {
        Boolean IsValid { get; }
        IEnumerable<IPromotionValidationResultReason> Reasons { get; }
    }

    public class SimplePromotionValidationResult : IPromotionValidationResult
    {
        public SimplePromotionValidationResult()
        {
            Reasons = Enumerable.Empty<IPromotionValidationResultReason>();
        }

        public SimplePromotionValidationResult(Boolean isValid)
        {
            IsValid = isValid;
            Reasons = Enumerable.Empty<IPromotionValidationResultReason>();
        }

        public SimplePromotionValidationResult(Boolean isValid, String messageKey)
        {
            IsValid = isValid;
            Reasons = new List<IPromotionValidationResultReason> { new PromotionValidationResultReason(messageKey) };
        }


        public SimplePromotionValidationResult(Boolean isValid, params IPromotionValidationResultReason[] reasons)
        {
            IsValid = isValid;
            Reasons = reasons.ToList();
        }

        public Boolean IsValid { get; protected set; }

        public IEnumerable<IPromotionValidationResultReason> Reasons { get; protected set; }
    }

    #endregion

    #region PromotionValidationResultReason

    public interface IPromotionValidationResultReason
    {
        String MessageKey { get; }
        IDictionary<String, Object> ContextItems { get; }
    }

    public class PromotionValidationResultReason : IPromotionValidationResultReason
    {
        public PromotionValidationResultReason()
        {
            this.MessageKey = String.Empty;
            ContextItems = new Dictionary<String, Object>();
        }

        public PromotionValidationResultReason(String messageKey)
        {
            this.MessageKey = messageKey;
            ContextItems = new Dictionary<String, Object>();
        }

        public String MessageKey { get; protected set; }

        public IDictionary<String, Object> ContextItems { get; protected set; }
    }

    #endregion

    [Serializable]
    [XmlInclude(typeof(StartDatePromotionRule))]
    [XmlInclude(typeof(ExpirationDatePromotionRule))]
    [XmlInclude(typeof(ExpirationNumberOfUsesPromotionRule))]
    [XmlInclude(typeof(ExpirationNumberOfUsesPerCustomerPromotionRule))]
    [XmlInclude(typeof(CustomerLevelPromotionRule))]
    [XmlInclude(typeof(CategoryPromotionRule))]
    [XmlInclude(typeof(SectionPromotionRule))]
    [XmlInclude(typeof(ManufacturerPromotionRule))]
    [XmlInclude(typeof(CartTotalPromotionRule))]
    [XmlInclude(typeof(EmailAddressPromotionRule))]
    [XmlInclude(typeof(ProductIdPromotionRule))]
    [XmlInclude(typeof(StatePromotionRule))]
    [XmlInclude(typeof(ZipCodePromotionRule))]
    [XmlInclude(typeof(CountryPromotionRule))]
    [XmlInclude(typeof(MinimumOrdersPromotionRule))]
    [XmlInclude(typeof(MinimumOrderAmountPromotionRule))]
    [XmlInclude(typeof(MinimumCartAmountPromotionRule))]
    [XmlInclude(typeof(MinimumProductsOrderedPromotionRule))]
    [XmlInclude(typeof(MinimumProductAmountOrderedPromotionRule))]
    [XmlInclude(typeof(ShippingMethodIdPromotionRule))]
    [XmlInclude(typeof(ShippingPromotionRule))]
    public abstract class PromotionRuleBase : IPromotionRule
    {
        public abstract IPromotionValidationResult Validate(IPromotionController promotionController, IRuleContext ruleContext);
    }


    #region StartDatePromotionRule

    [Serializable]
    public class StartDatePromotionRule : PromotionRuleBase
    {
        #region Properties

        public DateTime StartDate { get; set; }

        #endregion

        #region Public Methods

        public override IPromotionValidationResult Validate(IPromotionController promotionController, IRuleContext ruleContext)
        {
            return new SimplePromotionValidationResult(DateTime.Now >= this.StartDate, "Promotion.Reason.StartDate");
        }

        #endregion
    }

    #endregion

    #region ExpirationDatePromotionRule

    [Serializable]
    public class ExpirationDatePromotionRule : PromotionRuleBase
    {
        #region Properties

        public DateTime ExpirationDate { get; set; }

        #endregion

        #region Public Methods

        public override IPromotionValidationResult Validate(IPromotionController promotionController, IRuleContext ruleContext)
        {
            return new SimplePromotionValidationResult(DateTime.Now < ExpirationDate, "Promotion.Reason.ExpirationDate");
        }

        #endregion
    }

    #endregion

    #region ExpirationNumberOfUsesPromotionRule

    [Serializable]
    public class ExpirationNumberOfUsesPromotionRule : PromotionRuleBase
    {
        #region Properties

        public Int32 NumberOfUsesAllowed { get; set; }

        #endregion

        #region Public Methods

        public override IPromotionValidationResult Validate(IPromotionController promotionController, IRuleContext ruleContext)
        {
            IDataLookupResult dataLookupResult = promotionController.LookupData(new SimpleDataLookupContext()
            {
                PromotionId = ruleContext.PromotionId,
                LookupType = LookupType.TotalPromotionUses,
            });

            return new SimplePromotionValidationResult(dataLookupResult.Int32Result < NumberOfUsesAllowed, "Promotion.Reason.ExpirationNumberOfUses");
        }

        #endregion
    }

    #endregion

    #region ExpirationNumberOfUsesPerCustomerPromotionRule

    [Serializable]
    public class ExpirationNumberOfUsesPerCustomerPromotionRule : PromotionRuleBase
    {
        #region Properties

        public Int32 NumberOfUsesAllowed { get; set; }

        #endregion

        #region Public Methods

        public override IPromotionValidationResult Validate(IPromotionController promotionController, IRuleContext ruleContext)
        {
            IDataLookupResult dataLookupResult = promotionController.LookupData(new SimpleDataLookupContext()
            {
                PromotionId = ruleContext.PromotionId,
                CustomerId = ruleContext.CustomerId,
                LookupType = LookupType.TotalPromotionUses,
            });

            return new SimplePromotionValidationResult(dataLookupResult.Int32Result < NumberOfUsesAllowed, "Promotion.Reason.ExpirationNumberOfUsesPerCustomer");
        }

        #endregion
    }

    #endregion
    
    #region CustomerLevelPromotionRule

    [Serializable]
    public class CustomerLevelPromotionRule : PromotionRuleBase
    {
        #region Properties

        public Int32[] CustomerLevels { get; set; }

        #endregion

        #region Public Methods

        public override IPromotionValidationResult Validate(IPromotionController promotionController, IRuleContext ruleContext)
        {
            if (!ruleContext.IsRegistered)
                return new SimplePromotionValidationResult(false, "Promotion.Reason.CustomerLevel.NotLoggedIn");
            
            return new SimplePromotionValidationResult(this.CustomerLevels.Any(cl => cl == ruleContext.CustomerLevel), "Promotion.Reason.CustomerLevel.NoMatch");
        }

        #endregion
    }

    #endregion

    #region CategoryPromotionRule

    [Serializable]
    public class CategoryPromotionRule : PromotionRuleBase
    {
        #region Properties

        public Int32[] CategoryIds { get; set; }

        #endregion

        #region Public Methods

        public override IPromotionValidationResult Validate(IPromotionController promotionController, IRuleContext ruleContext)
        {
            if (this.CategoryIds == null)
                return new SimplePromotionValidationResult(false, "Promotion.Reason.Category.InvalidRuleDefined");
            
            foreach (ShoppingCartItem item in ruleContext.ShoppingCartItems)
            {
                if (this.CategoryIds.Intersect(item.CategoryIds).Any())
                    return new SimplePromotionValidationResult(true);
            }

            return new SimplePromotionValidationResult(false, "Promotion.Reason.Category.NoMatch");
        }

        #endregion
    }

    #endregion

    #region SectionPromotionRule

    [Serializable]
    public class SectionPromotionRule : PromotionRuleBase
    {
        #region Properties

        public Int32[] SectionIds { get; set; }

        #endregion

        #region Public Methods

        public override IPromotionValidationResult Validate(IPromotionController promotionController, IRuleContext ruleContext)
        {
            if (this.SectionIds == null)
                return new SimplePromotionValidationResult(false, "Promotion.Reason.Section.InvalidRuleDefined");

            foreach (ShoppingCartItem item in ruleContext.ShoppingCartItems)
            {
                if (this.SectionIds.Intersect(item.SectionIds).Any())
                    return new SimplePromotionValidationResult(true);
            }

            return new SimplePromotionValidationResult(false, "Promotion.Reason.Section.NoMatch");
        }

        #endregion
    }

    #endregion

    #region ManufacturerPromotionRule

    [Serializable]
    public class ManufacturerPromotionRule : PromotionRuleBase
    {
        #region Properties

        public Int32[] ManufacturerIds { get; set; }

        #endregion

        #region Public Methods

        public override IPromotionValidationResult Validate(IPromotionController promotionController, IRuleContext ruleContext)
        {
            if (this.ManufacturerIds == null)
                return new SimplePromotionValidationResult(false, "Promotion.Reason.Manufacturer.InvalidRuleDefined");

            foreach (ShoppingCartItem item in ruleContext.ShoppingCartItems)
            {
                if (this.ManufacturerIds.Intersect(item.ManufacturerIds).Any())
                    return new SimplePromotionValidationResult(true);
            }

            return new SimplePromotionValidationResult(false, "Promotion.Reason.Manufacturer.NoMatch");
        }

        #endregion
    }

    #endregion

    #region EmailAddressPromotionRule

    [Serializable]
    public class EmailAddressPromotionRule : PromotionRuleBase
    {
        #region Properties

        public String[] EmailAddresses { get; set; }

        #endregion

        #region Public Methods

        public override IPromotionValidationResult Validate(IPromotionController promotionController, IRuleContext ruleContext)
        {
            return new SimplePromotionValidationResult(EmailAddresses.Any(ea => ea.ToUpperInvariant() == ruleContext.EmailAddress.ToUpperInvariant()), "Promotion.Reason.EmailAddress.NoMatch");
        }

        #endregion
    }

    #endregion

    #region CartTotalPromotionRule

    [Serializable]
    public class CartTotalPromotionRule : PromotionRuleBase
    {
        #region Properties

        public Decimal MinimumCartTotalAllowed { get; set; }

        #endregion

        #region Public Methods

        public override IPromotionValidationResult Validate(IPromotionController promotionController, IRuleContext ruleContext)
        {
            //If this gets implemented we will have to figure out how to filter out gift items from the total
            //return ruleContext.CartTotal >= MinimumCartTotalAllowed;
            throw new NotImplementedException();
        }

        #endregion
    }

    #endregion
    
    #region ProductIdPromotionRule

    [Serializable]
    public class ProductIdPromotionRule : PromotionRuleBase
    {
        #region Properties

        public Int32[] ProductIds { get; set; }
        public Boolean RequireQuantity { get; set; }
        public Int32 Quantity { get; set; }
        public Boolean AndTogether { get; set; }

        #endregion

        #region Public Methods

        public override IPromotionValidationResult Validate(IPromotionController promotionController, IRuleContext ruleContext)
        {
            if (this.AndTogether)
            {
                // validate all in list are in cart
                foreach (Int32 productId in ProductIds)
                {
                    if (!ruleContext.ShoppingCartItems.Select(s => s.ProductId).Contains(productId))
                        return new SimplePromotionValidationResult(false, "Promotion.Reason.ProductId.NoMatch");
                }
            }
            else
            {
                if (!ruleContext.ShoppingCartItems.Where(w => ProductIds.Contains(w.ProductId)).Any())
                    return new SimplePromotionValidationResult(false, "Promotion.Reason.ProductId.NoMatch");
            }

            if (RequireQuantity)
            {
				var requiredProductsInCart = ruleContext.ShoppingCartItems
					.Where(w => ProductIds.Contains(w.ProductId))
					.Select(item => new
					{
						CartItem = item,
						SufficientQuantity = item.Quantity >= Quantity,
					});

				var sufficientQuantityItems = requiredProductsInCart
					.Where(o => o.SufficientQuantity)
					.Select(o => o.CartItem);

				var insufficientQuantityItems = requiredProductsInCart
					.Where(o => !o.SufficientQuantity)
					.Select(o => o.CartItem);

				if(!sufficientQuantityItems.Any() || (this.AndTogether && insufficientQuantityItems.Any()))
				{
                    var reason = new PromotionValidationResultReason("Promotion.Reason.ProductId.InsufficientQuantity");
                    reason.ContextItems.Add("ProductName", insufficientQuantityItems.First().Name);
                    reason.ContextItems.Add("QuantityDelta", Quantity - insufficientQuantityItems.First().Quantity);
                             
                    return new SimplePromotionValidationResult(false, reason);
				}
            }
            
            return new SimplePromotionValidationResult(true);
        }

        #endregion
    }

    #endregion

    #region StatePromotionRule

    [Serializable]
    public class StatePromotionRule : PromotionRuleBase
    {
        #region Properties

        public String[] States { get; set; }

        #endregion

        #region Public Methods

        public override IPromotionValidationResult Validate(IPromotionController promotionController, IRuleContext ruleContext)
        {
            if (!ruleContext.IsRegistered)
                return new SimplePromotionValidationResult(false, "Promotion.Reason.State.NotLoggedIn");
            
            return new SimplePromotionValidationResult(this.States.Any(s => s.ToUpperInvariant() == ruleContext.State.ToUpperInvariant()), "Promotion.Reason.State.NoMatch");
        }

        #endregion
    }

    #endregion

    #region ZipCodePromotionRule

    [Serializable]
    public class ZipCodePromotionRule : PromotionRuleBase
    {
        #region Properties

        public String[] ZipCodes { get; set; }

        #endregion

        #region Public Methods

        public override IPromotionValidationResult Validate(IPromotionController promotionController, IRuleContext ruleContext)
        {
            if (!ruleContext.IsRegistered)
                return new SimplePromotionValidationResult(false, "Promotion.Reason.ZipCode.NotLoggedIn");
            
            return new SimplePromotionValidationResult(this.ZipCodes.Any(z => ruleContext.ZipCode.StartsWith(z)), "Promotion.Reason.ZipCode.NoMatch");
        }

        #endregion
    }

    #endregion

    #region CountryPromotionRule

    [Serializable]
    public class CountryPromotionRule : PromotionRuleBase
    {
        #region Properties

        public String[] CountryCodes { get; set; }

        #endregion

        #region Public Methods

        public override IPromotionValidationResult Validate(IPromotionController promotionController, IRuleContext ruleContext)
        {
            if (!ruleContext.IsRegistered)
                return new SimplePromotionValidationResult(false, "Promotion.Reason.Country.NotLoggedIn");
            
            return new SimplePromotionValidationResult(this.CountryCodes.Any(c => c.ToUpperInvariant() == ruleContext.CountryCode.ToUpperInvariant()), "Promotion.Reason.Country.NoMatch");
        }

        #endregion
    }

    #endregion

    #region MinimumOrdersPromotionRule

    [Serializable]
    public class MinimumOrdersPromotionRule : PromotionRuleBase
    {
        #region Properties

        public Int32 MinimumOrdersAllowed { get; set; }

        public DateType StartDateType { get; set; }

        public DateType EndDateType { get; set; }

        public DateTime CustomStartDate { get; set; }

        public DateTime CustomEndDate { get; set; }

        #endregion

        #region Public Methods

        public override IPromotionValidationResult Validate(IPromotionController promotionController, IRuleContext ruleContext)
        {
            if (!ruleContext.IsRegistered)
                return new SimplePromotionValidationResult(false, "Promotion.Reason.MinimumOrders.NotLoggedIn");

            IDataLookupResult dataLookupResult = promotionController.LookupData(new SimpleDataLookupContext()
            {
                CustomerId = ruleContext.CustomerId,
                LookupType = LookupType.TotalOrders,
                StartDateType = this.StartDateType,
                EndDateType = this.EndDateType,
                CustomStartDate = this.CustomStartDate,
                CustomEndDate = this.CustomEndDate,
            });

            var reason = new PromotionValidationResultReason("Promotion.Reason.MinimumOrders.InsufficientQuantity");
            reason.ContextItems.Add("OrderQuantityDelta", MinimumOrdersAllowed - dataLookupResult.Int32Result);
            reason.ContextItems.Add("PromotionEndDate", this.CustomEndDate.ToShortDateString());

            return new SimplePromotionValidationResult(dataLookupResult.Int32Result >= MinimumOrdersAllowed, reason);
        }

        #endregion
    }

    #endregion

    #region MinimumOrderAmountPromotionRule

    [Serializable]
    public class MinimumOrderAmountPromotionRule : PromotionRuleBase
    {
        #region Properties

        public Decimal MinimumOrderAmountAllowed { get; set; }

        public DateType StartDateType { get; set; }

        public DateType EndDateType { get; set; }

        public DateTime CustomStartDate { get; set; }

        public DateTime CustomEndDate { get; set; }

        #endregion

        #region Public Methods

        public override IPromotionValidationResult Validate(IPromotionController promotionController, IRuleContext ruleContext)
        {
            if (!ruleContext.IsRegistered)
                return new SimplePromotionValidationResult(false, "Promotion.Reason.MinimumOrderAmount.NotLoggedIn");

            IDataLookupResult dataLookupResult = promotionController.LookupData(new SimpleDataLookupContext()
            {
                CustomerId = ruleContext.CustomerId,
                LookupType = LookupType.TotalOrderAmount,
                StartDateType = this.StartDateType,
                EndDateType = this.EndDateType,
                CustomStartDate = this.CustomStartDate,
                CustomEndDate = this.CustomEndDate,
            });

            var reason = new PromotionValidationResultReason("Promotion.Reason.MinimumOrderAmount.InsufficientAmount");
            reason.ContextItems.Add("OrderAmountDelta", String.Format("{0:C2}", MinimumOrderAmountAllowed - dataLookupResult.DecimalResult));
            reason.ContextItems.Add("PromotionEndDate", this.CustomEndDate.ToShortDateString());

            return new SimplePromotionValidationResult(dataLookupResult.DecimalResult >= MinimumOrderAmountAllowed, reason);
        }

        #endregion
    }

    #endregion

    #region MinimumCartAmountPromotionRule

    [Serializable]
    public class MinimumCartAmountPromotionRule : PromotionRuleBase
    {
        #region Properties

        public Decimal CartAmount { get; set; }

        #endregion

        #region Public Methods

        public override IPromotionValidationResult Validate(IPromotionController promotionController, IRuleContext ruleContext)
        {
            
            decimal cartSubTotal = ruleContext.ShoppingCartItems.Any() ? ruleContext.ShoppingCartItems.Sum(sci => sci.Subtotal) : 0.0M;

            var reason = new PromotionValidationResultReason("Promotion.Reason.MinimumCartAmount.InsufficientAmount");
            reason.ContextItems.Add("CartAmountDelta", String.Format("{0:C2}", CartAmount - cartSubTotal));

            return new SimplePromotionValidationResult(cartSubTotal >= CartAmount, reason);
        }

        #endregion
    }

    #endregion

    #region MinimumProductsOrderedPromotionRule

    [Serializable]
    public class MinimumProductsOrderedPromotionRule : PromotionRuleBase
    {
        #region Properties

        public Int32[] ProductIds { get; set; }

        public Int32 MinimumProductsOrderedAllowed { get; set; }

        public DateType StartDateType { get; set; }

        public DateType EndDateType { get; set; }

        public DateTime CustomStartDate { get; set; }

        public DateTime CustomEndDate { get; set; }

        #endregion

        #region Public Methods

        public override IPromotionValidationResult Validate(IPromotionController promotionController, IRuleContext ruleContext)
        {
             if (!ruleContext.IsRegistered)
                 return new SimplePromotionValidationResult(false, "Promotion.Reason.MinimumProductsOrdered.NotLoggedIn");

            IDataLookupResult dataLookupResult = promotionController.LookupData(new SimpleDataLookupContext()
            {
                LookupType = LookupType.TotalProductOrdered,
                CustomerId = ruleContext.CustomerId,
                ProductIds = this.ProductIds,
                StartDateType = this.StartDateType,
                EndDateType = this.EndDateType,
                CustomStartDate = this.CustomStartDate,
                CustomEndDate = this.CustomEndDate,
            });

            var reason = new PromotionValidationResultReason("Promotion.Reason.MinimumProductsOrdered.InsufficientQuantity");
            reason.ContextItems.Add("OrderQuantityDelta", MinimumProductsOrderedAllowed - dataLookupResult.Int32Result);
            reason.ContextItems.Add("PromotionEndDate", this.CustomEndDate.ToShortDateString());
            reason.ContextItems.Add("ProductNames", dataLookupResult.StringResult);

            return new SimplePromotionValidationResult(dataLookupResult.Int32Result >= MinimumProductsOrderedAllowed, reason);
        }

        #endregion
    }

    #endregion

    #region MinimumProductAmountOrderedPromotionRule

    [Serializable]
    public class MinimumProductAmountOrderedPromotionRule : PromotionRuleBase
    {
        #region Properties

        public String[] Skus { get; set; }

        public Int32[] ProductIds { get; set; }

        public Decimal MinimumProductAmountOrderedAllowed { get; set; }

        public DateType StartDateType { get; set; }

        public DateType EndDateType { get; set; }

        public DateTime CustomStartDate { get; set; }

        public DateTime CustomEndDate { get; set; }

        #endregion

        #region Public Methods

        public override IPromotionValidationResult Validate(IPromotionController promotionController, IRuleContext ruleContext)
        {
            if (!ruleContext.IsRegistered)
                return new SimplePromotionValidationResult(false, "Promotion.Reason.MinimumProductAmountOrdered.NotLoggedIn");

            IDataLookupResult dataLookupResult = promotionController.LookupData(new SimpleDataLookupContext()
            {
                LookupType = LookupType.TotalProductOrderedAmount,
                CustomerId = ruleContext.CustomerId,
                Skus = this.Skus,
                ProductIds = this.ProductIds,
                StartDateType = this.StartDateType,
                EndDateType = this.EndDateType,
                CustomStartDate = this.CustomStartDate,
                CustomEndDate = this.CustomEndDate,
            });
            
            var reason = new PromotionValidationResultReason("Promotion.Reason.MinimumProductAmountOrdered.InsufficientAmount");
            reason.ContextItems.Add("OrderAmountDelta", String.Format("{0:C2}", MinimumProductAmountOrderedAllowed - dataLookupResult.DecimalResult));
            reason.ContextItems.Add("PromotionEndDate", this.CustomEndDate.ToShortDateString());
            reason.ContextItems.Add("ProductNames", dataLookupResult.StringResult);

            return new SimplePromotionValidationResult(dataLookupResult.DecimalResult >= MinimumProductAmountOrderedAllowed, reason);
        }

        #endregion
    }

    #endregion

    #region ShippingMethodIdPromotionRule

	[Serializable]
	public class ShippingMethodIdPromotionRule : PromotionRuleBase
	{
        public override IPromotionValidationResult Validate(IPromotionController promotionController, IRuleContext ruleContext)
        {
			if(!String.IsNullOrEmpty(ruleContext.State) && ruleContext.ExcludeStates != null && ruleContext.ExcludeStates.Contains(ruleContext.State))
                return new SimplePromotionValidationResult(false, "Promotion.Reason.State.NoMatch");

            return new SimplePromotionValidationResult(true);
		}
	}

    #endregion

    #region ShippingRule

    [Serializable]
    public class ShippingPromotionRule : PromotionRuleBase
    {
        public override IPromotionValidationResult Validate(IPromotionController promotionController, IRuleContext ruleContext)
        {
            if (ruleContext.MultiShipEnabled)
                return new SimplePromotionValidationResult(false, "Promotion.Reason.Shipping.MultishipEnabled");

            return new SimplePromotionValidationResult(true);
        }
    }

    #endregion
}
