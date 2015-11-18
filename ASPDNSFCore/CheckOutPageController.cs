// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;

namespace AspDotNetStorefrontCore
{
	public enum CheckOutType
	{
		Other,
		Standard,
		SmartOPC
	}

	public interface ICheckOutPageController
	{
		CheckOutType GetCheckoutType();
		string GetContinueCheckoutPage ();
		string GetStandardCheckoutPage ();
		string GetOnePageCheckoutPage();	
		string GetSmartOnePageCheckoutPage();		
		string GetOtherCheckoutPage();
        string GetCheckoutPaymentPage();
		bool CanUseOnePageCheckout();		
		bool CanUseInternationalCheckout();		
	}

	public class CheckOutPageController : ICheckOutPageController
	{
		public Customer Customer { get; set; }
		public ShoppingCart ShoppingCart { get; set; }

		public CheckOutPageController(Customer customer, ShoppingCart shoppingCart)
		{
			this.Customer = customer;
			this.ShoppingCart = shoppingCart;
		}

		/// <summary>
		/// Determine the checkout type
		/// </summary>
		/// <returns>The current CheckoutType</returns>
		public virtual CheckOutType GetCheckoutType()
		{
			CheckOutType checkoutType = CheckOutType.Standard;
			try
			{
				checkoutType = (CheckOutType)Enum.Parse(typeof(CheckOutType), AppLogic.AppConfig("Checkout.Type", AppLogic.StoreID(), true));
			}
			catch (Exception)
			{
				checkoutType = CheckOutType.Standard;
			}
			return checkoutType;
		}

		/// <summary>
		/// Determines the page to proceed to checkout from after the shopping cart page.
		/// </summary>
		/// <returns>
		/// If CheckoutType is other, returns Checkout.Page
		/// If CheckoutType is SmartOPC, and CanUseOPC, returns smartcheckout.aspx
		/// otherwise, returns standard checkout page based on checkout rules (multi-ship, anon, gift-card, etc)
		public virtual string GetContinueCheckoutPage ()
		{
			switch (GetCheckoutType())
			{
				case CheckOutType.Other:
					return GetOtherCheckoutPage();
				case CheckOutType.SmartOPC:
					return GetSmartOnePageCheckoutPage();
				default:
					return GetStandardCheckoutPage();
			}		
		}

		/// <summary>
		/// Determine which standard checkout page to use
		/// </summary>
		/// <returns>The checkout page to send the customer to based on checkout rules.</returns>
		public virtual string GetStandardCheckoutPage()
		{
            //if (!this.Customer.IsRegistered || this.Customer.EMail.Length == 0)
            //{
            //    return "createaccount.aspx?checkout=true"; 
            //}

			if ((this.Customer.IsRegistered || this.Customer.EMail.Length != 0) &&
				(this.Customer.Password.Length == 0 || this.Customer.PrimaryBillingAddressID == 0 ||
				this.Customer.PrimaryShippingAddressID == 0 || !this.Customer.HasAtLeastOneAddress()))
			{
                return "jwmyaddresses.aspx?checkout=true&addresstype=2&returnurl=checkoutshipping.aspx";
			}

			if (!this.Customer.IsRegistered || this.Customer.PrimaryBillingAddressID == 0 ||
				this.Customer.PrimaryShippingAddressID == 0 || !this.Customer.HasAtLeastOneAddress())
			{
				//return "checkoutanon.aspx?checkout=true";
                return "signin.aspx?checkout=true";
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
		
		/// <summary>
		/// Determine which one page checkout to use.
		/// </summary>
		/// <returns>If CanUseOnePageCheckout, and checkout type is Smart, return Smart one page checkout page.
		/// If CanUseOnePageCheckout, and checkout type is anything, returns Basic one page checkout.
		/// If one page checkout is not available, returns standard checkout.
		/// </returns>
		public virtual string GetOnePageCheckoutPage()
		{
			switch (GetCheckoutType())
			{
				default:
					return GetSmartOnePageCheckoutPage();
			}
		}

		/// <summary>
		/// Attempts to retrieve the SmartOPC page
		/// </summary>
		/// <returns>smartcheckout.aspx if CanUseOnePageCheckout, otherwise standard checkout</returns>
		public virtual string GetSmartOnePageCheckoutPage()
		{
			if (CanUseOnePageCheckout())
				return "smartcheckout.aspx";
			else
				return GetStandardCheckoutPage();
		}

		/// <summary>
		/// Determine the Other checkout page
		/// </summary>
		/// <returns>returns Checkout.Page</returns>
		public virtual string GetOtherCheckoutPage()
		{
			return AppLogic.AppConfig("Checkout.Page");
		}

		/// <summary>
		/// Determine if we can use OPC
		/// </summary>
		/// <returns>true if cart qualifies for OPC</returns>
		public virtual bool CanUseOnePageCheckout()
		{
			return !this.ShoppingCart.HasGiftRegistryComponents() &&
				!this.ShoppingCart.HasMultipleShippingAddresses() &&
				!this.ShoppingCart.ContainsGiftCard();
		}
		
		/// <summary>
		/// Determine if we can use international checkout
		/// </summary>
		/// <returns>true if cart qualifies for IC</returns>
		public virtual bool CanUseInternationalCheckout()
		{
			// handle international checkout buttons now (see internationalcheckout.com).
			if (AppLogic.AppConfigBool("InternationalCheckout.Enabled"))
			{
				// check to see if cart contains all known, and US addresses...if so, internationalcheckout should not be visible
				bool gAllUSAddresses = true;
				foreach (CartItem c in this.ShoppingCart.CartItems)
				{
					if (!c.IsDownload && !c.IsSystem && c.ShippingAddressID != 0)
					{
						Address sa = new Address();
						sa.LoadFromDB(c.ShippingAddressID);
						if (sa.Country.Trim().Equals("UNITED STATES", StringComparison.InvariantCultureIgnoreCase) == false)
						{
							gAllUSAddresses = false;
							break;
						}
					}
					else
					{
						gAllUSAddresses = false; // unknown address, or download or system product, etc, so it could be going anywhere
						break;
					}
				}

				if (!gAllUSAddresses && !this.ShoppingCart.HasDownloadComponents() && !this.ShoppingCart.HasGiftRegistryComponents() && !this.ShoppingCart.HasCoupon()
					&& !this.ShoppingCart.HasMicropayProduct() && !this.ShoppingCart.HasRecurringComponents() && !this.ShoppingCart.HasMultipleShippingAddresses()
					&& !this.ShoppingCart.HasSystemComponents() && !this.ShoppingCart.IsEmpty() && !this.ShoppingCart.ContainsGiftCard() && !this.ShoppingCart.HasKitComponents())
				{
					return true;
				}
			}

			return false;
		}

        //returns the checkout page url
        public virtual string GetCheckoutPaymentPage()
        {
            switch (GetCheckoutType())
            {
                case CheckOutType.SmartOPC:
                    return GetSmartOnePageCheckoutPage();
                default:
                    return "checkoutpayment.aspx";
            }
        }
	}

	public class CheckOutPageControllerFactory
	{
		public static ICheckOutPageController CreateCheckOutPageController(Customer customer, ShoppingCart cart)
		{
			return new CheckOutPageController(customer, cart);
		}

		public static ICheckOutPageController CreateCheckOutPageController()
		{
			Customer customer = Customer.Current;
			return new CheckOutPageController(customer , new ShoppingCart(customer.SkinID, customer, CartTypeEnum.ShoppingCart, 0, false));
		}
	}
}
