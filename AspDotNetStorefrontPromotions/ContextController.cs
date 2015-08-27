// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
#region Using Directives

using System;
using System.Data.Linq;
using System.Linq;
#endregion

namespace AspDotNetStorefront.Promotions.Data
{
	/// <summary>
	///  The ContextController class is responsible for creating a data context to the database of a specific version of adnsf.
	/// </summary>
	public static class ContextController
	{
		#region Public Methods

		/// <summary>
		///  Creates a new data context to the database of the specified version of adnsf.
		/// </summary>
		/// <param name="connectionString">Connection string to use when creating the context.</param>
		/// <returns>A new data context pointing to the specified adnsf database.</returns>
		/// <exception cref="ArgumentNullException" />
		/// /// <exception cref="InvalidOperationException" />
		public static DataContext CreateDataContext (String connectionString)
		{
			if (String.IsNullOrEmpty(connectionString))
				throw new ArgumentNullException("connectionString", "Connection string cannot be null or empty.");

			DataContext dataContext = null;

			try
			{
                dataContext = new EntityContextDataContext(connectionString);
			}
			catch (Exception exception)
			{
				throw new InvalidOperationException("An error occurred trying to create a data context for an ADNSF8.0.1.2 database.", exception);
			}

			return dataContext;
		}

        public static void TrackLineItemDiscount(DiscountedItem discountedItem)
        {
            Data.EntityContextDataContext context = new Data.EntityContextDataContext();

            //Need to check for line items where the shoppingcartrecid doesn't exist anymore.  PromoUsage should ensure we are grabbing the right line item when the card record dosen't match.
            Data.PromotionLineItem lineItem = context.PromotionLineItems.FirstOrDefault(f => f.shoppingCartRecordId == discountedItem.ShoppingCartRecordId 
                                                        || (f.PromotionUsageId == discountedItem.PromotionUsage.Id && f.productId == discountedItem.ProductId && f.variantId == discountedItem.VariantId));
            if (lineItem != null)
            {
                lineItem.quantity = discountedItem.Quantity;
                lineItem.subTotal = discountedItem.Subtotal;
                lineItem.discountAmount = discountedItem.IsAGift ? discountedItem.GiftAmount : discountedItem.DiscountAmount;
                lineItem.cartPrice = discountedItem.CartPrice;
                lineItem.shoppingCartRecordId = discountedItem.ShoppingCartRecordId; //need to update this if we've found a line item that doesn't have a matching shoppingcartrecid
                context.SubmitChanges();
            }
            else
            {
                Data.PromotionLineItem newLineItem = new Data.PromotionLineItem();
                newLineItem.cartPrice = discountedItem.CartPrice;
                newLineItem.discountAmount = discountedItem.IsAGift ? discountedItem.GiftAmount : discountedItem.DiscountAmount;
                newLineItem.isAGift = discountedItem.IsAGift;
                newLineItem.productId = discountedItem.ProductId;
                newLineItem.quantity = discountedItem.Quantity;
                newLineItem.shoppingCartRecordId = discountedItem.ShoppingCartRecordId;
                newLineItem.sku = discountedItem.Sku;
                newLineItem.subTotal = discountedItem.Subtotal;
                newLineItem.variantId = discountedItem.VariantId;
                newLineItem.PromotionUsageId = discountedItem.PromotionUsage.Id;
                context.PromotionLineItems.InsertOnSubmit(newLineItem);
                context.SubmitChanges();
            }
        }

		#endregion
	}
}
