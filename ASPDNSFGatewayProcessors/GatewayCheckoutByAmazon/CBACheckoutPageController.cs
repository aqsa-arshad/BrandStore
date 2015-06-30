// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
#region Using Directives

using System;
using AspDotNetStorefrontCore;

#endregion

namespace GatewayCheckoutByAmazon
{
	public class CBACheckoutPageController : CheckOutPageController
	{
		public CBACheckoutPageController (Customer customer, ShoppingCart shoppingcart)
			: base(customer, shoppingcart)
		{
		}

		#region Overrides

		public override string GetStandardCheckoutPage ()
		{
			if (this.Customer.IsRegistered  && (this.Customer.PrimaryBillingAddressID == 0 || this.Customer.PrimaryShippingAddressID == 0 || !this.Customer.HasAtLeastOneAddress() || this.Customer.Password.Length == 0))
			{
				return "createaccount.aspx?checkout=true";
			}

			if (!this.Customer.IsRegistered && (this.Customer.PrimaryBillingAddressID == 0 || this.Customer.PrimaryShippingAddressID == 0 || !this.Customer.HasAtLeastOneAddress() || this.Customer.Password.Length == 0))
			{
				return "checkoutanon.aspx?checkout=true";
			}
			else
			{
				if (AppLogic.AppConfigBool("SkipShippingOnCheckout") ||
					this.ShoppingCart.IsAllSystemComponents() || this.ShoppingCart.IsAllDownloadComponents())
				{
					if (this.ShoppingCart.ContainsGiftCard())
					{
						return "checkoutgiftcard.aspx";
					}
					else
					{
						return "checkoutpayment.aspx";
					}
				}

				if ((this.ShoppingCart.HasMultipleShippingAddresses() || this.ShoppingCart.HasGiftRegistryComponents()) &&
					this.ShoppingCart.TotalQuantity() <= AppLogic.MultiShipMaxNumItemsAllowed() && this.ShoppingCart.CartAllowsShippingMethodSelection)
				{
					return "checkoutshippingmult.aspx";
				}
				else
				{
					return "checkoutshipping.aspx";
				}
			}
		}

		#endregion
	}
}
