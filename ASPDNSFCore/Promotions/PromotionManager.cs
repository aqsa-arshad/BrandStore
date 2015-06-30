// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using AspDotNetStorefront.Promotions;
using PromotionsData = AspDotNetStorefront.Promotions.Data;
using System.Web;
using System.Xml.Serialization;
using System.IO;
using System.Text;

namespace AspDotNetStorefrontCore
{
    public static class PromotionManager
    {

        private static PromotionsData.Promotion GetPromotionById(int promotionId)
        {
            return PromotionsData.DataContextProvider.Current.Promotions.FirstOrDefault(p => p.Id == promotionId);
        }

        private static PromotionsData.Promotion GetPromotionByCode(string promotionCode, Boolean activeOnly)
        {
            return PromotionsData.DataContextProvider.Current.Promotions.FirstOrDefault(p => (!activeOnly || (activeOnly && p.Active)) && p.Code.ToUpper() == promotionCode.ToUpper());
        }

        public static PromotionController CreatePromotionController()
        {
            PromotionController promotionController = new PromotionController();
            promotionController.OnLookupData += new PromotionController.LookupDataDelegate(PromotionController_OnLookupData);
            return promotionController;
        }

        private static IDataLookupResult PromotionController_OnLookupData(IDataLookupContext dataLookupContext)
        {
            IDataLookupResult lookupResult = new SimpleDataLookupResult();

            switch (dataLookupContext.LookupType)
            {
                case LookupType.TotalPromotionUses:
                    IQueryable<PromotionsData.PromotionUsage> promotionUsages = PromotionsData.DataContextProvider.Current.PromotionUsages.Where(pu => pu.Complete);

                    if (dataLookupContext.CustomerId > 0)
                        promotionUsages = promotionUsages.Where(pu => pu.CustomerId == dataLookupContext.CustomerId);

                    if (dataLookupContext.PromotionId > 0)
                        promotionUsages = promotionUsages.Where(pu => pu.PromotionId == dataLookupContext.PromotionId);

                    promotionUsages = DateFilterPromotionUsage(dataLookupContext, promotionUsages);
                    lookupResult.Int32Result = promotionUsages.Count();
                    break;

                case LookupType.TotalPromotionDiscounts:
                    IQueryable<PromotionsData.PromotionUsage> promotionUsageDiscount = PromotionsData.DataContextProvider.Current.PromotionUsages.Where(pu => pu.Complete);

                    if (dataLookupContext.CustomerId > 0)
                        promotionUsageDiscount = promotionUsageDiscount.Where(pu => pu.CustomerId == dataLookupContext.CustomerId);

                    if (dataLookupContext.PromotionId > 0)
                        promotionUsageDiscount = promotionUsageDiscount.Where(pu => pu.PromotionId == dataLookupContext.PromotionId);

                    promotionUsageDiscount = DateFilterPromotionUsage(dataLookupContext, promotionUsageDiscount).Where(pud => pud.DiscountAmount != null);                    
                    lookupResult.DecimalResult = promotionUsageDiscount.Any() ? (decimal)promotionUsageDiscount.Sum(pu => pu.DiscountAmount) : 0.00M;
                    break;

                case LookupType.TotalOrders:
                    IQueryable<PromotionsData.Order> totalOrders = PromotionsData.DataContextProvider.Current.Orders;
                    if (dataLookupContext.CustomerId > 0)
                        totalOrders = totalOrders.Where(o => o.CustomerID == dataLookupContext.CustomerId);

                    totalOrders = DateFilterOrders(dataLookupContext, totalOrders);
                    lookupResult.Int32Result = totalOrders.Count();
                    break;

                case LookupType.TotalOrderAmount:
                    IQueryable<PromotionsData.Order> totalOrderAmount = PromotionsData.DataContextProvider.Current.Orders;
                    if (dataLookupContext.CustomerId > 0)
                        totalOrderAmount = totalOrderAmount.Where(o => o.CustomerID == dataLookupContext.CustomerId);

                    totalOrderAmount = DateFilterOrders(dataLookupContext, totalOrderAmount);
                    lookupResult.DecimalResult = totalOrderAmount.Any() ? totalOrderAmount.Sum(o => o.OrderTotal) : 0.00M;
                    break;

                case LookupType.TotalProductOrdered:
                    IQueryable<PromotionsData.Orders_ShoppingCart> totalProducts = PromotionsData.DataContextProvider.Current.Orders_ShoppingCarts
						.Where(w => !dataLookupContext.ProductIds.Any() || dataLookupContext.ProductIds.Contains(w.ProductID));
                    
					if (dataLookupContext.CustomerId > 0)
                        totalProducts = totalProducts.Where(os => os.CustomerID == dataLookupContext.CustomerId);

                    totalProducts = DateFilterOrders_ShoppingCart(dataLookupContext, totalProducts);
                    lookupResult.Int32Result = totalProducts.Any() ? totalProducts.Sum(s => s.Quantity) : 0;
                    lookupResult.StringResult = String.Join(", ", PromotionsData.DataContextProvider.Current.Products.Where(p => dataLookupContext.ProductIds.Contains(p.ProductID)).Select(p => p.Name).ToArray());
                    break;

                case LookupType.TotalProductOrderedAmount:
                    IQueryable<PromotionsData.Orders_ShoppingCart> totalProductAmount = PromotionsData.DataContextProvider.Current.Orders_ShoppingCarts
						.Where(w => !dataLookupContext.ProductIds.Any() || dataLookupContext.ProductIds.Contains(w.ProductID));

                    if (dataLookupContext.CustomerId > 0)
                        totalProductAmount = totalProductAmount.Where(os => os.CustomerID == dataLookupContext.CustomerId);

                    totalProductAmount = DateFilterOrders_ShoppingCart(dataLookupContext, totalProductAmount).Where(tpa => tpa.OrderedProductPrice != null);
                    lookupResult.DecimalResult = totalProductAmount.Any() ? (decimal)totalProductAmount.Sum(os => os.OrderedProductPrice) : 0.00M;
                    lookupResult.StringResult = String.Join(", ", PromotionsData.DataContextProvider.Current.Products.Where(p => dataLookupContext.ProductIds.Contains(p.ProductID)).Select(p => p.Name).ToArray());

                    break;

                case LookupType.TotalCostForSkus:
                    IQueryable<PromotionsData.ShoppingCart> totalCostForSkus = PromotionsData.DataContextProvider.Current.ShoppingCarts.Where(s => s.CartType == (int)CartTypeEnum.ShoppingCart);
                    if (dataLookupContext.CustomerId > 0)
                        totalCostForSkus = totalCostForSkus.Where(os => os.CustomerID == dataLookupContext.CustomerId);

                    totalCostForSkus = SkuFilterShoppingCart(dataLookupContext, totalCostForSkus).Where(sfsc => sfsc.ProductPrice != null);
                    lookupResult.DecimalResult = totalCostForSkus.Any() ? (decimal)totalCostForSkus.Sum(s => s.ProductPrice) : 0.00M;
                    break;

                case LookupType.TotalPromotions:
                    IQueryable<PromotionsData.PromotionUsage> totalPromotionUsages = PromotionsData.DataContextProvider.Current.PromotionUsages.Where(pu => pu.Complete);

                    if (dataLookupContext.CustomerId > 0)
                        totalPromotionUsages = totalPromotionUsages.Where(pu => pu.CustomerId == dataLookupContext.CustomerId);

                    if (dataLookupContext.PromotionId > 0)
                        totalPromotionUsages = totalPromotionUsages.Where(pu => pu.PromotionId == dataLookupContext.PromotionId);

                    totalPromotionUsages = DateFilterPromotionUsage(dataLookupContext, totalPromotionUsages);
                    lookupResult.Int32Result = totalPromotionUsages.Count();
                    break;

                case LookupType.LastPromotionUsage:
                    IQueryable<PromotionsData.PromotionUsage> lastPromotionUsages = PromotionsData.DataContextProvider.Current.PromotionUsages;

                    if (dataLookupContext.CustomerId > 0)
                        lastPromotionUsages = lastPromotionUsages.Where(pu => pu.CustomerId == dataLookupContext.CustomerId);

                    if (dataLookupContext.PromotionId > 0)
                        lastPromotionUsages = lastPromotionUsages.Where(pu => pu.PromotionId == dataLookupContext.PromotionId);

                    lastPromotionUsages = DateFilterPromotionUsage(dataLookupContext, lastPromotionUsages);
                    PromotionsData.PromotionUsage lastPromotionUsage = lastPromotionUsages.OrderByDescending(pu => pu.DateApplied).FirstOrDefault();
                    if (lastPromotionUsage != null)
                        lookupResult.DateTimeResult = lastPromotionUsage.DateApplied.Value;
                    else
                        lookupResult.DateTimeResult = DateTime.MinValue;
                    break;
            }

            return lookupResult;
        }

        private static IQueryable<PromotionsData.PromotionUsage> DateFilterPromotionUsage(IDataLookupContext dataLookupContext, IQueryable<PromotionsData.PromotionUsage> promotionUsages)
        {
            IQueryable<PromotionsData.PromotionUsage> retVal = promotionUsages.Where(pu => pu.DateApplied.HasValue);

            DateTime startDate = CalculateQueryDate(dataLookupContext.StartDateType, dataLookupContext.CustomStartDate, dataLookupContext.PromotionId, dataLookupContext.CustomerId);
            DateTime endDate = CalculateQueryDate(dataLookupContext.EndDateType, dataLookupContext.CustomEndDate, dataLookupContext.PromotionId, dataLookupContext.CustomerId);

            if (startDate != DateTime.MinValue && endDate != DateTime.MinValue)
                retVal = retVal.Where(pu => startDate <= pu.DateApplied.Value && pu.DateApplied.Value <= endDate);

            return retVal;
        }

        private static IQueryable<PromotionsData.Order> DateFilterOrders(IDataLookupContext dataLookupContext, IQueryable<PromotionsData.Order> orders)
        {
            IQueryable<PromotionsData.Order> retVal = orders;

            DateTime startDate = CalculateQueryDate(dataLookupContext.StartDateType, dataLookupContext.CustomStartDate, dataLookupContext.PromotionId, dataLookupContext.CustomerId);
            DateTime endDate = CalculateQueryDate(dataLookupContext.EndDateType, dataLookupContext.CustomEndDate, dataLookupContext.PromotionId, dataLookupContext.CustomerId);

            if (startDate != DateTime.MinValue && endDate != DateTime.MinValue)
                retVal = retVal.Where(o => startDate <= o.OrderDate && o.OrderDate <= endDate);

            return retVal;
        }

        private static IQueryable<PromotionsData.Orders_ShoppingCart> DateFilterOrders_ShoppingCart(IDataLookupContext dataLookupContext, IQueryable<PromotionsData.Orders_ShoppingCart> orders_ShoppingCart)
        {
            IQueryable<PromotionsData.Orders_ShoppingCart> retVal = orders_ShoppingCart;

            DateTime startDate = CalculateQueryDate(dataLookupContext.StartDateType, dataLookupContext.CustomStartDate, dataLookupContext.PromotionId, dataLookupContext.CustomerId);
            DateTime endDate = CalculateQueryDate(dataLookupContext.EndDateType, dataLookupContext.CustomEndDate, dataLookupContext.PromotionId, dataLookupContext.CustomerId);

            if (startDate != DateTime.MinValue && endDate != DateTime.MinValue)
                retVal = retVal.Where(o => startDate <= o.CreatedOn && o.CreatedOn <= endDate);

            return retVal;
        }

        private static IQueryable<PromotionsData.ShoppingCart> SkuFilterShoppingCart(IDataLookupContext dataLookupContext, IQueryable<PromotionsData.ShoppingCart> ShoppingCart)
        {
            IQueryable<PromotionsData.ShoppingCart> retVal = ShoppingCart.Where(s => s.CartType == (int)CartTypeEnum.ShoppingCart); ;

            retVal = retVal.Where(s => dataLookupContext.GiftSkus.Contains(s.ProductSKU) || dataLookupContext.GiftProductIds.Contains(s.ProductID));

            return retVal;
        }

        private static DateTime CalculateQueryDate(DateType dateType, DateTime customDate, int promotionId, int customerId)
        {
            switch (dateType)
            {
                case DateType.CustomDate:
                    return customDate;

                case DateType.LastPromotionUsage:
                    return PromotionController_OnLookupData(new SimpleDataLookupContext
                    {
                        CustomerId = customerId,
                        PromotionId = promotionId,
                        LookupType = LookupType.LastPromotionUsage,
                    }).DateTimeResult;

                case DateType.CurrentDate:
                    return DateTime.Now;

                case DateType.Unspecified:
                default:
                    return DateTime.MinValue;
            }
        }

		public static IDiscountContext CreateDiscountContext(IDiscountContext discountContext, IEnumerable<DiscountableItem> discountableItems)
		{
			return new SimpleDiscountContext(discountContext)
			{
				DiscountableItems = discountableItems,
			};
		}

		public static IDiscountContext CreateDiscountContext(IRuleContext ruleContext)
		{
			return new SimpleDiscountContext
			{
				LineItemTotal = ruleContext.SubTotal,
				ShippingTotal = ruleContext.ShippingTotal,
				ShippingTaxTotal = 0,
				OrderTaxTotal = 0,
				OrderTotal = ruleContext.SubTotal,
				DiscountableItems = null,
				BillingAddressId = ruleContext.BillingAddressId,
				CustomerId = ruleContext.CustomerId,
				CustomerLevelId = ruleContext.CustomerLevel,
				ShippingAddressId = ruleContext.ShippingAddressId,
				StoreId = ruleContext.StoreId
			};
		}

        public static IEnumerable<IPromotionValidationResult> ValidatePromotion(string promotionCode, IRuleContext ruleContext)
        {
            PromotionsData.Promotion promotion = GetPromotionByCode(promotionCode, false);
            if (promotion == null)
                return new[] { new SimplePromotionValidationResult(false, "Promotion.Reason.DoesNotExist") };
            else if (!promotion.Active)
                return new[] { new SimplePromotionValidationResult(false, "Promotion.Reason.InactivePromotion") };

            ruleContext.PromotionId = promotion.Id;

            return CreatePromotionController().ValidatePromotion(promotion, ruleContext, AppLogic.CustomerLevelAllowsCoupons(ruleContext.CustomerLevel));
        }

        public static void AssignPromotion(int customerId, string promotionCode)
        {
            PromotionsData.Promotion promotion = GetPromotionByCode(promotionCode, true);
            if (promotion == null)
                return;

            AssignPromotion(customerId, promotion.Id);
        }

        public static void AssignPromotion(int customerId, int promotionId)
        {
            PromotionsData.PromotionUsage promotionUsage = GetPromotionUsagesByCustomer(customerId).FirstOrDefault(pu => pu.PromotionId == promotionId);
            if (promotionUsage == null)
            {
                promotionUsage = new PromotionsData.PromotionUsage
                {
                    CustomerId = customerId,
                };
                PromotionsData.DataContextProvider.Current.PromotionUsages.InsertOnSubmit(promotionUsage);
            }
            promotionUsage.PromotionId = promotionId;
            PromotionsData.DataContextProvider.Current.SubmitChanges();
        }

        public static bool HasAssignedPromotions(int customerId)
        {
            return GetPromotionUsagesByCustomer(customerId).Count() > 0;
        }

        public static IQueryable<PromotionsData.PromotionUsage> GetPromotionUsagesByCustomer(int customerId)
        {
            return PromotionsData.DataContextProvider.Current.PromotionUsages.Where(pu => pu.CustomerId == customerId && pu.Complete == false);
        }

        public static IQueryable<PromotionsData.Promotion> GetAssignedPromotions(int customerId)
        {
            IQueryable<PromotionsData.PromotionUsage> promotionUsages = GetPromotionUsagesByCustomer(customerId).Where(p => p.Complete == false);
            foreach (PromotionsData.PromotionUsage p in promotionUsages)
            {
                bool pcomplete = p.Complete;
            }
            return promotionUsages.Select(pu => GetPromotionById(pu.PromotionId));
        }

        public static IDiscountResult GetPromotionDiscount(IRuleContext ruleContext)
        {
            IList<IDiscountResult> discountResults;
            return GetPromotionDiscount(ruleContext, out discountResults);
        }

        public static IDiscountResult GetPromotionDiscount(IRuleContext ruleContext, out IList<IDiscountResult> discountResults)
        {
            PromotionController promotionController = CreatePromotionController();

            discountResults = new List<IDiscountResult>();

            IQueryable<PromotionsData.PromotionUsage> promotionUsages = GetPromotionUsagesByCustomer(ruleContext.CustomerId).Where(p => p.Complete == false);
            Dictionary<IPromotionUsage, IPromotionDiscount> AllPromotionDiscounts = new Dictionary<IPromotionUsage, IPromotionDiscount>();

            //Need to loop all promos and all discount types in those promos so we can build up a list of all the discounts on this order
            foreach (PromotionsData.PromotionUsage promotionUsage in promotionUsages)
            {
                foreach (IPromotionDiscount promoDiscount in promotionUsage.Promotion.PromotionDiscounts)
                {
                    //We need to add only one item per promo usage but we want to order by the non-shipping option when it has shipping plus another discount type on one promo
                    //this only works becuase we restrict promos to shipping plus one other type of discount.
                    if(promotionUsage.Promotion.PromotionDiscounts.Count == 1 || promoDiscount.SequenceNumber != (int)PromotionDiscountBase.PromotionSequence.Shipping)
                        AllPromotionDiscounts.Add(promotionUsage, promoDiscount);
                }
            }

            //Sort the discounts, this is incase we need to deal with line item -vs- order level coupon priority
            var sortedPromotionDiscounts = AllPromotionDiscounts.ToArray().OrderBy(apd => apd.Key.Id).OrderBy(apd => apd.Value.SequenceNumber);
            var discountContext = CreateDiscountContext(ruleContext);

            foreach (KeyValuePair<IPromotionUsage, IPromotionDiscount> discountPair in sortedPromotionDiscounts)
            {
                var promotionRuleContext = CreateRuleContext(ruleContext, discountPair.Key.PromotionId);

                List<DiscountableItem> discountableItems = GetDiscountableItems(promotionRuleContext, discountPair.Key.PromotionId);

                var promotionDiscountContext = CreateDiscountContext(discountContext, discountableItems);

                IDiscountResult discountResult = promotionController.ApplyPromotion(discountPair.Key, promotionRuleContext, promotionDiscountContext, () => new SimpleDiscountResult(), CreatePromotionController(), AppLogic.CustomerLevelAllowsCoupons(ruleContext.CustomerLevel));

                if (discountResult != null)
                    discountResults.Add(discountResult);
            }

            return promotionController.CombineDiscounts(discountResults, delegate() { return new SimpleDiscountResult(); });
        }


        public static void PrioritizePromotions(IRuleContext ruleContext)
        {
            if (IsValidCartType(ruleContext))
                return;

            RemoveDuplicatePromotionUsages(ruleContext.CustomerId);
            List<PromotionsData.Promotion> assignedPromotions = GetAssignedPromotions(ruleContext.CustomerId).ToList();  // get all assigned promotions

            if (assignedPromotions.Any())
            {
                decimal priority = 0;

                if (assignedPromotions.Where(w => w.Priority != 0).Any())
                    priority = assignedPromotions.Where(w => w.Priority != 0).Min(m => m.Priority); // get lowest priority

                foreach (PromotionsData.Promotion p in assignedPromotions.Where(w => w.Priority == 0 || w.Priority == priority))
                {
                    if (PromotionManager.ValidatePromotion(p.Code, ruleContext).All(vr => vr.IsValid))
                        PromotionManager.AssignPromotion(ruleContext.CustomerId, p.Code);
                    else
                        PromotionManager.ClearPromotionUsages(ruleContext.CustomerId, p.Code, false);
                }
                foreach (PromotionsData.Promotion p in assignedPromotions.Where(w => w.Priority > priority))
                {
                    PromotionManager.ClearPromotionUsages(ruleContext.CustomerId, p.Code, false);
                }
            }
        }

        public static void ClearPromotionUsages(int customerId, string promotionCode, bool removeAutoAssigned)
        {
            PromotionsData.EntityContextDataContext db = PromotionsData.DataContextProvider.Current;

            var promotionUsages = from pu in db.PromotionUsages
                                  join p in db.Promotions on pu.PromotionId equals p.Id
                                  join pli in db.PromotionLineItems on pu.Id equals pli.PromotionUsageId into pui
                                  from pli in pui.DefaultIfEmpty()
                                  join sc in db.ShoppingCarts on pli.shoppingCartRecordId equals sc.ShoppingCartRecID into sci
                                  from sc in sci.DefaultIfEmpty()
                                  where pu.CustomerId == customerId && !pu.Complete
                                  
                                  select new
                                  {
                                      promoUsage = pu
                                      , promo = p
                                      , promoLineItem = pli
                                      , scItem = sc
                                  };

            if(!string.IsNullOrEmpty(promotionCode))
                promotionUsages = promotionUsages.Where(puli => puli.promo.Code == promotionCode);

            var giftCarts = promotionUsages.Where(puli => puli.promoLineItem.isAGift).Select(puli => puli.scItem).Where(sc => sc != null);

            db.ShoppingCarts.DeleteAllOnSubmit(giftCarts);
            db.PromotionUsages.DeleteAllOnSubmit(promotionUsages.Select(puli => puli.promoUsage));

            //Only add auto assigned promos to the removed list if they are intentionally removed by user
            if (removeAutoAssigned)
            {
                foreach (var promoUsage in promotionUsages.Where(puli => puli.promo.AutoAssigned))
                {
                    PromotionManager.AddAutoAssignedPromotionToRemovedList(promoUsage.promoUsage.CustomerId, promoUsage.promo.Id);
                }
            }
            db.SubmitChanges();
        }

        public static void ClearAllPromotionUsages(int customerId)
        {
            ClearPromotionUsages(customerId, null, false);
        }

        public static IList<PromotionsData.Promotion> AutoAssignPromotions(int customerId, IRuleContext ruleContext)
        {
            IList<PromotionsData.Promotion> autoAssignPromotions = new List<PromotionsData.Promotion>();

            if (ruleContext.CartType != null && ruleContext.CartType != (Int32)CartTypeEnum.ShoppingCart)
                return autoAssignPromotions;

            PromotionController promotionController = CreatePromotionController();

            if (!AppLogic.AppConfigExists("AspDotNetStorefront.Promotions.excludestates"))
            {
                AppLogic.AddAppConfig("AspDotNetStorefront.Promotions.excludestates", "states to be excluded from shipping promotions", String.Empty, "", true);
            }
            string excludeConfig = AppLogic.AppConfig("AspDotNetStorefront.Promotions.excludestates");
            if (excludeConfig.Length > 0)
            {
                ruleContext.ExcludeStates = excludeConfig.Split(',');
            }

            var promosToAssign = PromotionsData.DataContextProvider.Current.Promotions.Where(p => p.Active && p.AutoAssigned && !PromotionManager.GetRemovedAutoAssignPromotionIdList(customerId).Contains(p.Id));
                
            foreach (PromotionsData.Promotion promotion in promosToAssign)
            {
                ruleContext.PromotionId = promotion.Id;
				if (promotionController.ValidatePromotion(promotion, ruleContext, AppLogic.CustomerLevelAllowsCoupons(ruleContext.CustomerLevel)).All(vr => vr.IsValid))
                {
                    AssignPromotion(customerId, promotion.Id);
                    autoAssignPromotions.Add(promotion);
                }
            }

            return autoAssignPromotions;
        }

        public static IList<PromotionsData.Promotion> GetLoyaltyPromotions(int customerId, IRuleContext ruleContext)
        {
            IList<PromotionsData.Promotion> autoAssignPromotions = new List<PromotionsData.Promotion>();

            PromotionController promotionController = CreatePromotionController();

            if (!AppLogic.AppConfigExists("AspDotNetStorefront.Promotions.excludestates"))
            {
                AppLogic.AddAppConfig("AspDotNetStorefront.Promotions.excludestates", "states to be excluded from shipping promotions", String.Empty, "", true);
            }
            string excludeConfig = AppLogic.AppConfig("AspDotNetStorefront.Promotions.excludestates");
            if (excludeConfig.Length > 0)
            {
                ruleContext.ExcludeStates = excludeConfig.Split(',');
            }
            IList<PromotionsData.Promotion> loyaltyPromotions = new List<PromotionsData.Promotion>();

            foreach (PromotionsData.Promotion promotion in PromotionsData.DataContextProvider.Current.Promotions.Where(p => p.Active))
            {
                if (promotion.PromotionRules.Where(p => p.GetType() == typeof(MinimumOrderAmountPromotionRule) || p.GetType() == typeof(MinimumOrdersPromotionRule) || p.GetType() == typeof(MinimumProductAmountOrderedPromotionRule) || p.GetType() == typeof(MinimumProductsOrderedPromotionRule)).Any())
                {
                    ruleContext.PromotionId = promotion.Id;
                    if (promotionController.ValidateLoyaltyPromotion(promotion, ruleContext))
                    {
                        loyaltyPromotions.Add(promotion);
                    }

                }
            }

            return loyaltyPromotions;
        }

        public static Decimal LookupPriceBySku(string sku)
        {
            Decimal price = decimal.Zero;

            foreach (PromotionsData.Product product in PromotionsData.DataContextProvider.Current.Products.Where(p => p.SKU.Equals(sku)))
            {
                Decimal productId = product.ProductID;

                foreach (PromotionsData.ProductVariant productVariant in PromotionsData.DataContextProvider.Current.ProductVariants.Where(pv => pv.ProductID == productId && pv.IsDefault == 1))
                {
                    if (productVariant.SalePrice > 0)
                    {
                        price = (decimal)productVariant.SalePrice;
                    }
                    else
                    {
                        price = productVariant.Price;
                    }
                }
            }
            return price;
        }

        public static int LookupProductIdBySku(string sku)
        {
            int productId = 0;

            foreach (PromotionsData.Product product in PromotionsData.DataContextProvider.Current.Products.Where(p => p.SKU.Equals(sku)))
            {
                productId = product.ProductID;
            }
            return productId;
        }

        public static bool HasQualifyingItemGiftProductPromotion(IRuleContext ruleContext)
        {
            bool isQualifyingItemGiftProductPromotion = false;
            PromotionController promotionController = CreatePromotionController();

            foreach (PromotionsData.PromotionUsage promotionUsage in PromotionsData.DataContextProvider.Current.PromotionUsages.Where(pu => pu.CustomerId == ruleContext.CustomerId && pu.Complete == false))
            {

                PromotionsData.Promotion promotion = PromotionsData.DataContextProvider.Current.Promotions.FirstOrDefault(p => p.Id == promotionUsage.PromotionId);
                ruleContext.PromotionId = promotion.Id;

                if (promotionController.ValidatePromotion(promotion, ruleContext, AppLogic.CustomerLevelAllowsCoupons(ruleContext.CustomerLevel)).All(vr => vr.IsValid))
                {
                    foreach (PromotionDiscountBase pd in promotion.PromotionDiscounts.Where(p => p.GetType() == typeof(GiftProductPromotionDiscount)))
                    {
                        CategoryPromotionRule categories = new CategoryPromotionRule();
                        SectionPromotionRule sections = new SectionPromotionRule();
                        ManufacturerPromotionRule manufacturers = new ManufacturerPromotionRule();
                        ProductIdPromotionRule productids = new ProductIdPromotionRule();

                        if (promotion.PromotionRules.Where(pr => (pr.GetType() == typeof(CategoryPromotionRule)) || (pr.GetType() == typeof(SectionPromotionRule)) || (pr.GetType() == typeof(ManufacturerPromotionRule)) || (pr.GetType() == typeof(ProductIdPromotionRule))).Count() > 0)
                        {
                            isQualifyingItemGiftProductPromotion = true;
                        }
                    }
                }
            }

            return isQualifyingItemGiftProductPromotion;
        }

        public static List<Int32> GetNonDiscountableShoppingCartIds(IRuleContext ruleContext)
        {
            List<Int32> nonDiscountableIds = new List<int>();
            IQueryable<PromotionsData.PromotionLineItem> promotionLineItems = PromotionsData.DataContextProvider.Current.PromotionLineItems
                                                                                .Where(pli => ruleContext.ShoppingCartItems.Select(sci => sci.ShoppingCartRecordId).Contains(pli.shoppingCartRecordId) && pli.isAGift);
            if (promotionLineItems.Any())
                nonDiscountableIds.AddRange(promotionLineItems.Select(pli => pli.shoppingCartRecordId).ToList());
            
            return nonDiscountableIds;
        }

        public static List<DiscountableItem> GetDiscountableItems(IRuleContext ruleContext, int promoId)
        {
            List<DiscountableItem> discountableItems = new List<DiscountableItem>();
            List<Int32> nonDiscountableIds = GetNonDiscountableShoppingCartIds(ruleContext);
            PromotionController promotionController = CreatePromotionController();

            PromotionsData.Promotion promotion = PromotionsData.DataContextProvider.Current.Promotions.FirstOrDefault(p => p.Active && p.Id == promoId);
            ruleContext.PromotionId = promotion.Id;

			if (promotionController.ValidatePromotion(promotion, ruleContext, AppLogic.CustomerLevelAllowsCoupons(ruleContext.CustomerLevel)).All(vr => vr.IsValid))
            {
                foreach (PromotionDiscountBase pd in promotion.PromotionDiscounts.Where(p => p.GetType() == typeof(OrderItemPromotionDiscount) || p.GetType() == typeof(GiftProductPromotionDiscount) || p.GetType() == typeof(OrderPromotionDiscount)))
                {
                    CategoryPromotionRule categoryIdRule = new CategoryPromotionRule();
                    SectionPromotionRule sectionIdRule = new SectionPromotionRule();
                    ManufacturerPromotionRule manufacturerIdRule = new ManufacturerPromotionRule();
                    ProductIdPromotionRule productIdRule = new ProductIdPromotionRule();

                    if (promotion.PromotionRules.Where(pr => pr.GetType() == typeof(CategoryPromotionRule)).Count() > 0)
                        categoryIdRule = promotion.PromotionRules.Where(pr => pr.GetType() == typeof(CategoryPromotionRule)).First() as CategoryPromotionRule;

                    if (promotion.PromotionRules.Where(pr => pr.GetType() == typeof(SectionPromotionRule)).Count() > 0)
                        sectionIdRule = promotion.PromotionRules.Where(pr => pr.GetType() == typeof(SectionPromotionRule)).First() as SectionPromotionRule;

                    if (promotion.PromotionRules.Where(pr => pr.GetType() == typeof(ManufacturerPromotionRule)).Count() > 0)
                        manufacturerIdRule = promotion.PromotionRules.Where(pr => pr.GetType() == typeof(ManufacturerPromotionRule)).First() as ManufacturerPromotionRule;

                    if (promotion.PromotionRules.Where(pr => pr.GetType() == typeof(ProductIdPromotionRule)).Count() > 0)
                        productIdRule = promotion.PromotionRules.Where(pr => pr.GetType() == typeof(ProductIdPromotionRule)).First() as ProductIdPromotionRule;

                    foreach (ShoppingCartItem cartItem in ruleContext.ShoppingCartItems)
                    {
                        bool qualifies = true;
                        if (categoryIdRule.CategoryIds != null && categoryIdRule.CategoryIds.Count() > 0 && cartItem.CategoryIds != null && cartItem.CategoryIds.Intersect(categoryIdRule.CategoryIds).Count() == 0)
                            qualifies = false;

                        if (sectionIdRule.SectionIds != null && sectionIdRule.SectionIds.Count() > 0 && cartItem.SectionIds != null && cartItem.SectionIds.Intersect(sectionIdRule.SectionIds).Count() == 0)
                            qualifies = false;

                        if (manufacturerIdRule.ManufacturerIds != null && manufacturerIdRule.ManufacturerIds.Count() > 0 && cartItem.ManufacturerIds != null && cartItem.ManufacturerIds.Intersect(manufacturerIdRule.ManufacturerIds).Count() == 0)
                            qualifies = false;

                        if (productIdRule.ProductIds != null && !productIdRule.ProductIds.Contains(cartItem.ProductId))
                            qualifies = false;

                        if(nonDiscountableIds.Contains(cartItem.ShoppingCartRecordId))
                            qualifies = false;

                        if (qualifies)
                        {
                            if (pd.GetType() == typeof(OrderItemPromotionDiscount) || pd.GetType() == typeof(GiftProductPromotionDiscount))
                            {
                                DiscountableItem discountableItem = new DiscountableItem();
                                discountableItem.CartPrice = cartItem.CartPrice;
                                discountableItem.ProductId = cartItem.ProductId;
                                discountableItem.Quantity = cartItem.Quantity;
                                discountableItem.ShoppingCartRecordId = cartItem.ShoppingCartRecordId;
                                discountableItem.Sku = cartItem.Sku;
                                discountableItem.Subtotal = cartItem.Subtotal;
                                discountableItem.VariantId = cartItem.VariantId;
                                discountableItems.Add(discountableItem);
                            }
                        }
                    }
                }

            }
            return discountableItems;
        }

        public static IRuleContext CreateRuleContext(ShoppingCart cart)
        {
			if(!AppLogic.AppConfigExists("AspDotNetStorefront.Promotions.excludestates"))
				AppLogic.AddAppConfig("AspDotNetStorefront.Promotions.excludestates", "states to be excluded from shipping promotions", String.Empty, "CUSTOM", false);

			string excludeConfig = AppLogic.AppConfig("AspDotNetStorefront.Promotions.excludestates");

			bool filterByStore = false;
			var allowCouponFiltering = GlobalConfig.getGlobalConfig("AllowCouponFiltering");
			if(allowCouponFiltering != null)
				Boolean.TryParse(allowCouponFiltering.ConfigValue, out filterByStore);

			var cartItems = cart.CartItems
                .Where(ci => !ci.IsGift)
				.Select(ci =>
					new ShoppingCartItem
					{
						CartPrice = ci.Price,
						CategoryIds = Array.ConvertAll<string, int>(AppLogic.GetProductEntityMappings(ci.ProductID, "CATEGORY").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries), i => Convert.ToInt32(i.ToString())),
						ManufacturerIds = Array.ConvertAll<string, int>(AppLogic.GetProductEntityMappings(ci.ProductID, "MANUFACTURER").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries), i => Convert.ToInt32(i.ToString())),
						ProductId = ci.ProductID,
						Quantity = ci.Quantity,
						SectionIds = Array.ConvertAll<string, int>(AppLogic.GetProductEntityMappings(ci.ProductID, "SECTION").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries), i => Convert.ToInt32(i.ToString())),
						ShoppingCartRecordId = ci.ShoppingCartRecordID,
						Sku = ci.SKU,
                        Name = ci.ProductName,
						Subscription = new Subscription() { Interval = ci.SubscriptionInterval, IntervalType = (int)ci.SubscriptionIntervalType },
						Subtotal = ci.Price * ci.Quantity,
						VariantId = ci.VariantID,
                        IsGift = ci.IsGift
					}
				);

            //To check is Promotion applied Before Quantity Discount.
            bool includeDiscounts = AppLogic.AppConfigBool("Promotions.ApplyDiscountsBeforePromoApplied");

			var ruleContext = new SimpleRuleContext
			{
				ShoppingCartItems = cartItems,
				StoreId = AppLogic.StoreID(),
				CustomerId = cart.ThisCustomer.CustomerID,
                IsRegistered = cart.ThisCustomer.IsRegistered,
				BillingAddressId = cart.ThisCustomer.PrimaryBillingAddressID,
				ShippingAddressId = cart.ThisCustomer.PrimaryShippingAddressID,
				EmailAddress = cart.ThisCustomer.EMail,
				CustomerLevel = cart.ThisCustomer.CustomerLevelID,
				ShippingMethodId = cart.CartItems.Count > 0 ? cart.FirstItem().ShippingMethodID : 0,
                State = cart.ThisCustomer.PrimaryShippingAddress.State,
				ZipCode = cart.ThisCustomer.PrimaryShippingAddress.Zip,
                CountryCode = cart.ThisCustomer.PrimaryShippingAddress.Country,
				ShippingTotal = cart.ShippingTotal(true, true),
                SubTotal = cart.SubTotal(includeDiscounts, false, true, true),
				ExcludeStates = excludeConfig.Length > 0 ? excludeConfig.Split(',') : null,
				FilterByStore = filterByStore,
                AddItemToCart = cart.AddItemToCart,
                MultiShipEnabled = AppLogic.AppConfigBool("AllowMultipleShippingAddressPerOrder") || AppLogic.AppConfigBool("ShowGiftRegistryButtons"),
                CartType = (Int32?)cart.CartType
			};

            return ruleContext;
        }

		public static IRuleContext CreateRuleContext(IRuleContext ruleContext, int promotionId)
		{
			return new SimpleRuleContext(ruleContext)
			{
				PromotionId = promotionId,
                MultiShipEnabled = AppLogic.AppConfigBool("AllowMultipleShippingAddressPerOrder") || AppLogic.AppConfigBool("ShowGiftRegistryButtons"),
                AddItemToCart = ruleContext.AddItemToCart
			};
		}

        public static IList<IDiscountResult> GetDiscountResultList(IRuleContext ruleContext)
        {
            IList<IDiscountResult> discountResults = new List<IDiscountResult>();

            if (IsValidCartType(ruleContext))
                return discountResults;

            IDiscountResult discountResult = GetPromotionDiscount(ruleContext, out discountResults);

            return discountResults;
        }

        public static decimal GetFinalDiscount(IDiscountResult discountResult, decimal cartTotal)
        {
            decimal finalDiscount = decimal.Zero;
            decimal currentDiscount = discountResult.TotalDiscount * -1;
            if (currentDiscount > cartTotal)
                finalDiscount = cartTotal;
            else
                finalDiscount = currentDiscount;

            return finalDiscount * -1;
        }

        public static void FinalizePromotionsOnOrderComplete(ShoppingCart cart, int orderNumber)
        {
            foreach (IDiscountResult result in cart.DiscountResults)
            {
                PromotionsData.PromotionUsage promotionUsage = PromotionsData.DataContextProvider.Current.PromotionUsages
                                                                .FirstOrDefault(pu => pu.PromotionId == result.Promotion.Id && pu.Complete == false && pu.CustomerId == cart.ThisCustomer.CustomerID);
                if (promotionUsage == null)
                    continue;

                promotionUsage.OrderId = orderNumber;
                promotionUsage.DateApplied = DateTime.Now;
                promotionUsage.LineItemDiscountAmount = result.LineItemTotal;
                promotionUsage.ShippingDiscountAmount = result.ShippingTotal;
                promotionUsage.OrderDiscountAmount = result.OrderTotal;
                promotionUsage.DiscountAmount = result.TotalDiscount;
                promotionUsage.Complete = true;

                PromotionsData.DataContextProvider.Current.SubmitChanges();
            }

            //Make sure we clear out the auto assigned list.
            PromotionManager.ResetAutoAssignedPromotionList(cart.ThisCustomer.CustomerID);
        }

        public static void TransferPromotionsOnUserLogin(int currentCustomerId, int newCustomerId)
        {
            PromotionsData.EntityContextDataContext db = PromotionsData.DataContextProvider.Current;
            var promotionUsages = db.PromotionUsages.Where(pu => pu.CustomerId == currentCustomerId);
            foreach (PromotionsData.PromotionUsage promoUsage in promotionUsages)
            {
                promoUsage.CustomerId = newCustomerId;
            }

            db.SubmitChanges();
        }

        public static bool IsValidCartType(IRuleContext ruleContext)
        {
            return (ruleContext.CartType == null || ruleContext.CartType != (Int32)CartTypeEnum.ShoppingCart);
        }

        public static void ResetAutoAssignedPromotionList(int customerId)
        {
            Customer cust = new Customer(customerId, true);
            cust.ThisCustomerSession["RemovedAutoAssignPromotionIdList"] = "";
        }

        public static void RemoveDuplicatePromotionUsages(int CustomerId)
        {
            PromotionsData.EntityContextDataContext db = PromotionsData.DataContextProvider.Current;

            //Grab a list of all promos for this customer that are applied to the current cart (IE not complete)
            var promoUsages = from pu in db.PromotionUsages
                                      join p in db.Promotions on pu.PromotionId equals p.Id
                                      join pli in db.PromotionLineItems on pu.Id equals pli.PromotionUsageId into pui
                                      from pli in pui.DefaultIfEmpty()
                                      join sc in db.ShoppingCarts on pli.shoppingCartRecordId equals sc.ShoppingCartRecID into sci
                                      from sc in sci.DefaultIfEmpty()
                                      where pu.CustomerId == CustomerId && !pu.Complete
                                  
                                      select new
                                      {
                                          promoUsage = pu
                                          , promo = p
                                          , promoLineItem = pli
                                          , scItem = sc
                                      };                           

            //Get all the promousages that are duplicates (should only have on promousage per promo on a customers cart, can have multiple complete but should only have one un-complete per promo)                      
            var groupedPromoUsages = from pu in promoUsages
                                     group pu by new {pu.promoUsage.Complete, pu.promoUsage.CustomerId, pu.promoUsage.PromotionId} into pug
                                     where pug.Count() > 1

                                     select new
                                     {
                                         pug.Key.CustomerId
                                         , pug.Key.PromotionId
                                         , pug.Key.Complete
                                     };

            if (!groupedPromoUsages.Any())
                return;

            //reduce our list of promo usages down to just those that have duplicates
            promoUsages = from pu in promoUsages
                            join pug in groupedPromoUsages on new { pu.promoUsage.CustomerId, pu.promoUsage.PromotionId, pu.promoUsage.Complete } equals new { pug.CustomerId, pug.PromotionId, pug.Complete }
                            select pu;

            foreach (var promoGroup in groupedPromoUsages)
            {
                //we will keep the first dupe we find, doesn't matter which
                var promoToSave = promoUsages.FirstOrDefault(pu => pu.promoUsage.PromotionId == promoGroup.PromotionId);

                //select all the duplicate records
                var promoUsageToDelete = promoUsages.Where(pu => pu.promoUsage.PromotionId == promoGroup.PromotionId && pu.promoUsage.Id != promoToSave.promoUsage.Id);
                //delete any duplicate free gift items
                var giftCarts = promoUsageToDelete.Where(pu => pu.promoLineItem.isAGift).Select(pu => pu.scItem).Where(sc => sc != null);
                db.ShoppingCarts.DeleteAllOnSubmit(giftCarts);
                //delete the duplicate promo usages
                db.PromotionUsages.DeleteAllOnSubmit(promoUsageToDelete.Select(pu => pu.promoUsage));

            }
            db.SubmitChanges();
        }

		/// <summary>
		/// This method clears out any unused promotions for an order.  Unused promotions are an artifact of promotions that include
		/// shipping discounts.  They stay applied in order to support switching of shipping methods without the promotion falling 
		/// off in the checkout process.
		/// </summary>
		/// <param name="orderNumber"></param>
		public static void RemoveUnusedPromotionsForOrder(int orderNumber)
		{
			PromotionsData.EntityContextDataContext db = PromotionsData.DataContextProvider.Current;

			var promoUsages = from pu in db.PromotionUsages
							  where pu.OrderId == orderNumber && pu.Complete && pu.DiscountAmount == decimal.Zero
							  select pu;

			db.PromotionUsages.DeleteAllOnSubmit(promoUsages);
			db.SubmitChanges();
		}

        public static List<int> GetRemovedAutoAssignPromotionIdList(int customerId)
        {
            Customer cust = new Customer(customerId, true);
            string strPromoList = cust.ThisCustomerSession["RemovedAutoAssignPromotionIdList"];

            XmlSerializer serializer = new XmlSerializer(typeof(List<int>));
        
            
            if(!string.IsNullOrEmpty(strPromoList))
            {
                using (TextReader reader = new StringReader(strPromoList))
                {
                    return (serializer.Deserialize(reader) as List<int>) ?? new List<int>();
                }
            }
            else
            {
                return new List<int>();
            }
        }

        public static void AddAutoAssignedPromotionToRemovedList(int customerId, int promotionId)
        {
            Customer cust = new Customer(customerId, true);
            XmlSerializer serializer = new XmlSerializer(typeof(List<int>));
            StringBuilder sb = new StringBuilder();
            List<int> promoList = GetRemovedAutoAssignPromotionIdList(customerId);
            promoList.Add(promotionId);

            using (TextWriter writer = new StringWriter(sb))
            {
                serializer.Serialize(writer, promoList);
            }

            cust.ThisCustomerSession["RemovedAutoAssignPromotionIdList"] = sb.ToString();
        }
    }
}
