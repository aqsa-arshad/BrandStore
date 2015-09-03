// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;

namespace AspDotNetStorefrontCore
{
    /// <summary>
    /// Class responsible for all pricing calculations.  All new routines
    /// and modifications to existing routines related to pricing should
    /// be made in this class to maintain rounding consistency.
    /// </summary>
    public partial class Prices
    {
        #region Vat

        /// <summary>
        /// Calculates display price for a given item considering sale price, extended
        /// price, customer level, attributes with price modifiers, and VAT for
        /// product and entity pages.
        /// </summary>
        /// <param name="ThisCustomer">Customer object for customer level and discount data retrieval</param>
        /// <param name="variantID">int value indicating the variant id of the item being priced</param>
        /// <param name="regularPrice">Decimal value representing the regular price</param>
        /// <param name="salePrice">Decimal value representing the sale price</param>
        /// <param name="extPrice">Decimal value representing the extended price</param>
        /// <param name="attributesPriceDelta">Decimal value representing any additional price for attributes</param>
        /// <param name="returnDiscountedPrice">Boolean value indicating whether to calculate the regular price or the discounted (sale, extended, customer level) price</param>
        /// <param name="taxClassID">int value represending the tax class id of the current item</param>
        /// <returns>Decimal price, rounded to 2 decimal places</returns>
        public static Decimal VariantPrice(Customer ThisCustomer, int variantID, decimal regularPrice, decimal salePrice, decimal extPrice, decimal attributesPriceDelta, Boolean returnDiscountedPrice, int taxClassID)
        {
            // is VAT enabled and supported
            Boolean VATEnabled = AppLogic.AppConfigBool("VAT.Enabled");

            // is the item taxable
            Boolean taxable = IsTaxable(variantID);

            // set the regular price
            Decimal price = regularPrice + attributesPriceDelta;

            // if trying to get the discounted (sale, customer level discounted, or extended) price
            if (returnDiscountedPrice)
            {
                // is sale price defined and does customer not belong to a customer level
                if (salePrice > System.Decimal.Zero && ThisCustomer.CustomerLevelID == 0)
                {
                    price = salePrice + attributesPriceDelta;
                }

                // if this customer is a member of a customer level, determine customer level prices
                if (ThisCustomer.CustomerLevelID > 0)
                {
                    // determine multiplier for customer level discount
                    decimal customerLevelMultiplier = 1 - (ThisCustomer.LevelDiscountPct / 100);

                    // defaults to regular price with applied discount
                    decimal customerLevelPrice = (regularPrice + attributesPriceDelta) * customerLevelMultiplier;

                    // if extended pricing is defined (non-zero), use it instead
                    if (extPrice > System.Decimal.Zero)
                    {
                        // do customerLevel discounts apply to extended pricing?
                        if (ThisCustomer.DiscountExtendedPrices)
                        {
                            // yes, use extended pricing and apply discount
                            customerLevelPrice = (extPrice + attributesPriceDelta) * customerLevelMultiplier;
                        }
                        else
                        {
                            // no, just use extended price with no additional discount
                            customerLevelPrice = extPrice + attributesPriceDelta;
                        }
                    }

                    // set the return price to the customer level discounted price
                    price = customerLevelPrice;
                }
            }

            // if the item is taxable, VAT is enabled, and viewing prices inclusive of VAT
            if (taxable && VATEnabled && ThisCustomer.VATSettingReconciled == VATSettingEnum.ShowPricesInclusiveOfVAT)
            {
                // add the VAT rate to the price
                price = price + (price * (TaxRate(ThisCustomer, taxClassID) / 100));
            }

            // return the price, rounded to 2 decimal places
            return price;
        }

        /// <summary>
        /// Determines the VAT amount for a line item in the shopping cart
        /// </summary>
        /// <param name="ci">CartItem to retrieve VAT for</param>
        /// <param name="ThisCustomer">Customer object</param>
        /// <returns>The VAT amount for a line item for display in the shopping cart</returns>
        public static Decimal LineItemVAT(CartItem ci, List<CouponObject> cList, List<QDObject> qList, Customer ThisCustomer)
        {
            Decimal VAT = 0.0M;
            Decimal price = ci.Price;

            // if there is a quantity discount
            if (ci.QuantityDiscountID > 0)
            {
                // get product level quantity discount price
                price = GetQuantityDiscount(ci, ThisCustomer);
            }

            if (ci.ThisShoppingCart.DiscountResults != null)
            {
                foreach (var discountResult in ci.ThisShoppingCart.DiscountResults)
                {
                    foreach (var discountedItem in discountResult.DiscountedItems.Where(di => di.ShoppingCartRecordId == ci.ShoppingCartRecordID))
                    {
                        price += ((discountedItem.Quantity != 0) ? discountedItem.DiscountAmount / discountedItem.Quantity : 0);
                        if (price < 0)
                            price = 0;
                    }
                }
            }

            VAT = GetVATPrice(price, ci.Quantity, ThisCustomer, ci.TaxClassID);

            return VAT;
        }

        public static Decimal GetVATPrice(Decimal price, Customer customer, Int32 taxClassId)
        {
            return GetVATPrice(price, 1, customer, taxClassId);
        }

        public static Decimal GetVATPrice(Decimal price, Int32 quantity, Customer customer, Int32 taxClassId)
        {
            if (AppLogic.AppConfigBool("VAT.RoundPerItem"))
                // Multiply the price by the tax, then round to 2 decimals, then multiply by the quantity to get the VAT amount
                return Math.Round(price * (TaxRate(customer, taxClassId) / 100), 2, MidpointRounding.AwayFromZero) * quantity;
            else
                // Multiply the price by the tax, then multiply by the quantity, then round to 2 decimals to get the VAT amount
                return price * (TaxRate(customer, taxClassId) / 100) * quantity;
        }

        #endregion

        #region Tax

        /// <summary>
        /// Determines if a Variant has been set as taxable
        /// </summary>
        /// <param name="VariantID">The VariantID of the item to check</param>
        /// <returns>True if the item is taxable, else false</returns>
        public static Boolean IsTaxable(int VariantID)
        {
            Boolean taxable = false;

            // establish connection to the database
            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                // open the connection
                conn.Open();

                // create the data reader to retireve the information from the ProductVariant table
                using (IDataReader reader = DB.GetRS("SELECT IsTaxable FROM dbo.ProductVariant with(NOLOCK) WHERE VariantID=" + VariantID.ToString(), conn))
                {
                    // determine if the item is taxable by checking the IsTaxable field
                    taxable = reader.Read() && DB.RSFieldTinyInt(reader, "IsTaxable") == 1;
                }
            }

            // return true for taxable, false for non-taxable
            return taxable;
        }

        /// <summary>
        /// Returns the tax rate for the specified tax class for the Customer's curent shipping address, if the TaxCalcMode AppConfig is set to "billing" the rate is for the billing address
        /// </summary>
        /// <param name="TaxClassID">The item tax class ID</param>
        /// <returns>The tax rate for the item</returns>
        public static Decimal TaxRate(Customer ThisCustomer, int TaxClassID)
        {
            // if the TaxCalcMode AppConfig parameter is set to billing
            if ("billing".Equals(AppLogic.AppConfig("TaxCalcMode"), StringComparison.InvariantCultureIgnoreCase))
            {
                // use the billing address to get the tax rate
				return TaxRate(ThisCustomer.PrimaryBillingAddress, TaxClassID, ThisCustomer);
            }
            else // TaxCalcMode is shipping
            {
                // use the shipping address to get the tax rate
				return TaxRate(ThisCustomer.PrimaryShippingAddress, TaxClassID, ThisCustomer);
            }
        }
		
        /// <summary>
        /// Determines the appropriate tax rate to be charged.  If a tax AddIn is present/installed, it will be
        /// used to calculate the tax rate rather than the in-store logic.
        /// </summary>
        /// <param name="useAddress">The customer address to calculate taxes for</param>
		/// <param name="TaxClassID">The item tax class id</param>
		/// <param name="ThisCustomer">The customer being taxed</param>
        /// <returns></returns>
        public static Decimal TaxRate(Address useAddress, int taxClassID, Customer thisCustomer)
        {
            Decimal rate = System.Decimal.Zero;

            //sets default vat country id
            int countryID = AppLogic.AppConfigBool("VAT.Enabled") ? AppLogic.AppConfigUSInt("VAT.CountryID") : 0;
            int stateID = 0;
            string zipCode = String.Empty;

            if (useAddress.CountryID > 0)
            {
                countryID = useAddress.CountryID;
            }
            if (useAddress.StateID > 0)
            {
                stateID = useAddress.StateID;
            }
            if (useAddress.Zip.Trim().Length != 0)
            {
                zipCode = useAddress.Zip.Trim();
            }

			rate = TaxRate(countryID, stateID, zipCode, taxClassID, thisCustomer);

            return rate;
        }

        /// <summary>
        /// Determines the appropriate tax rate based on the Country, State, and ZipCode.  Should never be called directly from outside
        /// the Prices class unless the intention is to bypass any Tax Add-Ins.
        /// </summary>
        /// <param name="CountryID">CountryID of the country where the tax is calulated, set to -1 for no country tax</param>
        /// <param name="StateID">StateID of the state where the tax is calulated, set to -1 for no country tax</param>
        /// <param name="ZipCode">Postal Code of the region where the tax is calulated, set to empty string for no zip code tax</param>
        /// <param name="TaxClassID">The product tax class</param>
        /// <returns>Decimal rate</returns>
        private static Decimal TaxRate(int CountryID, int StateID, string ZipCode, int TaxClassID, Customer ThisCustomer)
        {
            if (ThisCustomer.LevelHasNoTax || ThisCustomer.IsVatExempt())
            {
                return 0;
            }

            Decimal rate = System.Decimal.Zero;

            rate += AppLogic.CountryTaxRatesTable.GetTaxRate(CountryID, TaxClassID);
            rate += AppLogic.StateTaxRatesTable.GetTaxRate(StateID, TaxClassID);
            rate += AppLogic.ZipTaxRatesTable.GetTaxRate(ZipCode, TaxClassID, CountryID);

            return rate;
        }

        /// <summary>
        /// Calculates the total amount of tax to be charged
        /// </summary>
        /// <param name="ThisCustomer">Customer object </param>
        /// <param name="cartItems">A CartItemCollection collection of items in the shopping cart</param>
        /// <param name="shipCost">The total cost of shipping</param>
        /// <param name="orderOptions">A collection of OrderOption</param>
        /// <returns></returns>
		public static Decimal TaxTotal(Customer thisCustomer, CartItemCollection cartItems, Decimal shipCost, IEnumerable<OrderOption> orderOptions)
		{
            if (AppLogic.AppConfigBool("VAT.Enabled") && thisCustomer.VATSettingReconciled == VATSettingEnum.ShowPricesInclusiveOfVAT)
                return 0.00M;

			try
			{
				// Apply Avalara if enabled
				if(AppLogic.AppConfigBool("AvalaraTax.Enabled"))
				{
					AvaTax avaTax = new AvaTax();
					return avaTax.GetTaxRate(thisCustomer, cartItems, orderOptions);
				}
			}
			catch(Exception Ex)
			{
				SysLog.LogException(Ex, MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
			}

			/**********************************************************************/
			//Determine internal tax rates if there are no AddIn interfaces and Avalara is disabled or there were exceptions using either.
			decimal taxAmount = Decimal.Zero;

			// Promotions: Look for promotions in the shopping cart and find any order level discounts
			//  and save them for discounting line items before calculating taxes.
			Decimal totalDiscount = 0.00M;
			if(cartItems.HasDiscountResults)
				totalDiscount = -cartItems.DiscountResults.Sum(dr => dr.OrderTotal);

            IList<Int32> taxClassIds = new List<Int32>();
            using(SqlConnection conn = new SqlConnection(DB.GetDBConn()))
			{
				conn.Open();
				using(IDataReader rs = DB.GetRS("select TaxClassID from TaxClass Order By DisplayOrder", conn))
				{
					while(rs.Read())
					{
                        taxClassIds.Add(DB.RSFieldInt(rs, "TaxClassID"));
                    }
                }
            }
            
			// item tax
            foreach (var taxClassId in taxClassIds)
            {
                Decimal lineItemTotal = System.Decimal.Zero;

                foreach (CartItem ci in cartItems.Where(c => c.IsTaxable && c.TaxClassID == taxClassId))
                {
                    lineItemTotal += LineItemPrice(ci, cartItems.CouponList, cartItems.QuantityDiscountList, thisCustomer);

                    if (totalDiscount > 0)
                    {
                        if (totalDiscount > lineItemTotal)
                        {
                            totalDiscount -= lineItemTotal;
                            lineItemTotal = 0;
                        }
                        else
                        {
                            lineItemTotal -= totalDiscount;
                            totalDiscount = 0;
                        }
                    }
                }

                if (AppLogic.AppConfigBool("VAT.Enabled") && thisCustomer.VATSettingReconciled != VATSettingEnum.ShowPricesInclusiveOfVAT)
                    taxAmount += GetVATPrice(lineItemTotal, thisCustomer, taxClassId);
                else if (!AppLogic.AppConfigBool("VAT.Enabled"))
                    taxAmount += lineItemTotal * (TaxRate(thisCustomer, taxClassId) / 100);
            }

			//shipping tax
			List<int> shipAddresses = Shipping.GetDistinctShippingAddressIDs(cartItems);
			if(shipAddresses.Count() == 1)
			{
				taxAmount += ShippingTax(shipCost, shipAddresses.First(), thisCustomer);
			}
			else
			{
				foreach(int addr in shipAddresses)
				{
					IEnumerable<CartItem> addrCart = cartItems.Where(c => c.ShippingAddressID == addr);
					CartItemCollection tmpcic = new CartItemCollection(addrCart);
					shipCost = ShippingTotal(true, false, tmpcic, thisCustomer, orderOptions);
					taxAmount += ShippingTax(shipCost, addr, thisCustomer);
				}
			}

			// order option tax
			taxAmount += orderOptions.Sum(oo => oo.TaxRate);

			return taxAmount;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shipCost"></param>
        /// <param name="addressID"></param>
        /// <returns></returns>
        public static decimal ShippingTax(Decimal shipCost, int addressID, Customer thisCustomer)
        {
            Address addr = new Address();
            addr.LoadFromDB(addressID);

			return ShippingTax(shipCost, addr, thisCustomer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static decimal ShippingTax(Decimal shipCost, Address shipAddr, Customer thisCustomer)
        {
            Decimal tax = System.Decimal.Zero;

			tax = shipCost * (TaxRate(shipAddr, AppLogic.AppConfigUSInt("ShippingTaxClassID"), thisCustomer) / 100.0M);

            return tax;
        }

        public static CartItemCollection RemoveTaxRelatedCartItems(CartItemCollection Collection)
        {
            if (AppLogic.AppConfigBool("AvalaraTax.Enabled"))
                Collection.RemoveAll(c => c.SKU.ToLower() == "shipping" || c.ProductID == 0);

            return Collection;
        }

        #endregion

        #region Order Option

        /// <summary>
        /// Returns the VAT amount for an order option
        /// </summary>
        /// <param name="ci">CartItem to retrieve VAT for</param>
        /// <param name="ThisCustomer">Customer object</param>
        /// <returns></returns>
        public static Decimal OrderOptionVAT(Customer ThisCustomer, Decimal optionCost, int TaxClassID)
        {
            Decimal VAT = System.Decimal.Zero;

            VAT = (TaxRate(ThisCustomer, TaxClassID) * optionCost) / 100;

            return VAT;
        }

        /// <summary>
        /// Calculates the subtotal of all order options selected
        /// </summary>
        /// <returns>Decimal optiontotal, rounded to 2 decimal places</returns>
        public static Decimal OrderOptionTotal(Customer ThisCustomer, IEnumerable<OrderOption> OrderOptions)
        {
            Decimal optiontotal = System.Decimal.Zero;

            bool VATEnabled = AppLogic.AppConfigBool("VAT.Enabled");
            bool VATOn = (VATEnabled && ThisCustomer.VATSettingReconciled == VATSettingEnum.ShowPricesInclusiveOfVAT);

            if (OrderOptions.Count() > 0)
            {
                // cost and tax was already computed upon cart.LoadOrderOptions
                if (VATOn)
                {
                    optiontotal = OrderOptions.Sum(opt => opt.Cost + opt.TaxRate);
                }
                else
                {
                    optiontotal = OrderOptions.Sum(opt => opt.Cost);
                }
            }

            return optiontotal;
        }

        #endregion

        #region Line Item Price

        /// <summary>
        /// Calculates the final line item price for display in the shopping cart,
        /// considering quantity discounts, customer level percent discounts, product
        /// level coupon discounts, sale prices, extended prices, and VAT.
        /// </summary>
        /// <returns>Decimal price, rounded to 2 decimal places</returns>
        public static Decimal LineItemPrice(CartItem ci, List<CouponObject> cList, List<QDObject> qList, Customer ThisCustomer)
        {
            return LineItemPrice(ci, cList, qList, ThisCustomer, true, false);
        }

        /// <summary>
        /// Calculates the final line item price for display in the shopping cart,
        /// considering quantity discounts, customer level percent discounts, product
        /// level coupon discounts, sale prices, extended prices, and VAT.
        /// </summary>
        /// <returns>Decimal price, rounded to 2 decimal places</returns>
        public static Decimal LineItemPrice(CartItem ci, List<CouponObject> cList, List<QDObject> qList, Customer ThisCustomer, Boolean includeDiscount, Boolean excludeTax)
        {
            // is VAT enabled and supported
            Boolean VATEnabled = AppLogic.AppConfigBool("VAT.Enabled");

            // return the price
            return LineItemPrice(ci, cList, qList, ThisCustomer, includeDiscount, excludeTax, VATEnabled);
        }

        /// <summary>
        /// Calculates the final line item price for display in the shopping cart,
        /// considering quantity discounts, customer level percent discounts, product
        /// level coupon discounts, sale prices, extended prices, and VAT.
        /// </summary>
        /// <returns>Decimal price, rounded to 2 decimal places</returns>
        public static Decimal LineItemPrice(CartItem ci, List<CouponObject> cList, List<QDObject> qList, Customer ThisCustomer, Boolean includeDiscount, Boolean excludeTax, Boolean VATEnabled)
        {
            Decimal price = ci.Price;

            // if retrieving the discounted price apply quantity discounts and coupons
            if (includeDiscount)
            {
                if (ci.QuantityDiscountID > 0)
                    price = GetQuantityDiscount(ci, ThisCustomer);

                // Promotions: If there are any promotions assigned to the cart, we need to look at each discount and see if there are any line item level
                //  discounts and decrement the price by the amount.
                if (ci.ThisShoppingCart.DiscountResults != null)
                {
                    foreach (var discountResult in ci.ThisShoppingCart.DiscountResults)
                    {
                        foreach (var discountedItem in discountResult.DiscountedItems.Where(di => di.ShoppingCartRecordId == ci.ShoppingCartRecordID))
                        {
                            price += ((discountedItem.Quantity != 0) ? discountedItem.DiscountAmount / discountedItem.Quantity : 0);
                            if (price < 0)
                                price = 0;
                        }
                    }
                }
            }

            // If the item is taxable and we're either not excluding the tax, or vat is enabled and the tax is inclusive.
            if (ci.IsTaxable && (!excludeTax || VATEnabled && ThisCustomer.VATSettingReconciled == VATSettingEnum.ShowPricesInclusiveOfVAT))
            {
                if (VATEnabled && ThisCustomer.VATSettingReconciled == VATSettingEnum.ShowPricesInclusiveOfVAT)
                {
                    if (AppLogic.AppConfigBool("VAT.RoundPerItem"))
                        price = Math.Round(price + (price * (TaxRate(ThisCustomer, ci.TaxClassID) / 100)), 4, MidpointRounding.AwayFromZero) * ci.Quantity;
                    else
                        price = (price + (price * (TaxRate(ThisCustomer, ci.TaxClassID) / 100))) * ci.Quantity;
                }
                else
                {
                    // taxes aren't shown per line item, just multiply the price by the quantity
                    price = price * ci.Quantity;
                }
            }
            else
                price = price * ci.Quantity;

            return price;
        }
        
        #endregion

        #region SubTotal

        public static Decimal SubTotal(bool includeDiscount, bool onlyIncludeTaxableItems, bool includeDownloadItems, bool includeFreeShippingItems, bool includeSystemItems, bool UseCustomerCurrencySetting, CartItemCollection cic, Customer ThisCustomer, IEnumerable<OrderOption> OrderOptions)
        {
            return SubTotal(includeDiscount, onlyIncludeTaxableItems, includeDownloadItems, includeFreeShippingItems, includeSystemItems, UseCustomerCurrencySetting, 0, cic, ThisCustomer, OrderOptions);
        }

        public static Decimal SubTotal(bool includeDiscount, bool onlyIncludeTaxableItems, bool includeDownloadItems, bool includeFreeShippingItems, bool includeSystemItems, bool UseCustomerCurrencySetting, int ForShippingAddressID, CartItemCollection cic, Customer ThisCustomer, IEnumerable<OrderOption> OrderOptions)
        {
            return SubTotal(includeDiscount, onlyIncludeTaxableItems, includeDownloadItems, includeFreeShippingItems, includeSystemItems, UseCustomerCurrencySetting, 0, false, cic, ThisCustomer, OrderOptions);
        }

        public static Decimal SubTotal(bool includeDiscount, bool onlyIncludeTaxableItems, bool includeDownloadItems, bool includeFreeShippingItems, bool includeSystemItems, bool UseCustomerCurrencySetting, int ForShippingAddressID, bool ExcludeTax, CartItemCollection cic, Customer ThisCustomer, IEnumerable<OrderOption> OrderOptions)
        {
            return SubTotal(includeDiscount, onlyIncludeTaxableItems, includeDownloadItems, includeFreeShippingItems, includeSystemItems, UseCustomerCurrencySetting, ForShippingAddressID, ExcludeTax, cic, ThisCustomer, OrderOptions, 0, false);
        }

        public static Decimal SubTotal(bool includeDiscount, bool onlyIncludeTaxableItems, bool includeDownloadItems, bool includeFreeShippingItems, bool includeSystemItems, bool UseCustomerCurrencySetting, int ForShippingAddressID, bool excludeTax, CartItemCollection cic, Customer ThisCustomer, IEnumerable<OrderOption> OrderOptions, int OriginalRecurringOrderNumber, bool OnlyLoadRecurringItemsThatAreDue)
        {
            Decimal subTotal = System.Decimal.Zero;
            IEnumerable<CartItem> tmpList = cic;

            if (onlyIncludeTaxableItems)
                tmpList = tmpList.Where(ci => ci.IsTaxable);

            if (!includeDownloadItems)
                tmpList = tmpList.Where(ci => !ci.IsDownload);

            if (!includeFreeShippingItems)
                tmpList = tmpList.Where(ci => !ci.FreeShipping);

            foreach (CartItem ci in tmpList)
                subTotal += LineItemPrice(ci, cic.CouponList, cic.QuantityDiscountList, ThisCustomer, includeDiscount, excludeTax);

            if (includeDiscount)
                subTotal = GetOrderCustomerLevelDiscount(subTotal, ThisCustomer);

            subTotal += OrderOptionTotal(ThisCustomer, OrderOptions);

            return subTotal;
        }

        #endregion

        #region Shipping

        /// <summary>
        /// Calculates the total to be charged for shipping
        /// </summary>
        /// <returns>Decimal shipping</returns>
        public static Decimal ShippingTotal(bool includeDiscount, bool includeTax, CartItemCollection cartItems, Customer customer, IEnumerable<OrderOption> orderOptions)
        {
            if (cartItems == null || !cartItems.Any())
                return System.Decimal.Zero;

            decimal CODFee = System.Decimal.Zero;
			if(AppLogic.CleanPaymentMethod(customer.PrimaryBillingAddress.PaymentMethodLastUsed) == AppLogic.ro_PMCOD)
				CODFee = AppLogic.AppConfigUSDecimal("CODHandlingExtraFee");

			bool allShippingMethodsAreInFreeList = cartItems
				.Select(ci => ci.ShippingMethodID)
				.All(i => Shipping.ShippingMethodIsInFreeList(i));

            try
            {
                if (cartItems.IsAllFreeShippingComponents && allShippingMethodsAreInFreeList && !cartItems.IsAllDownloadComponents && !Shipping.NoShippingRequiredComponents(cartItems) && !cartItems.IsAllEmailGiftCards)
                    return CODFee;
            }
            catch { }

            decimal shippingCost = System.Decimal.Zero;
            List<int> cartAddressIds = Shipping.GetDistinctShippingAddressIDs(cartItems);
			foreach(int addressId in cartAddressIds)
				shippingCost += ShippingTotalForAddress(includeDiscount, includeTax, cartItems, customer, orderOptions, addressId, cartAddressIds.Count);

            // Ensure that it is never a negative amount.
			if(shippingCost < 0)
				shippingCost = 0;

			return shippingCost;
        }

		public static decimal ShippingTotalForAddress(bool includeDiscount, bool includeTax, CartItemCollection cartItems, Customer customer, IEnumerable<OrderOption> orderOptions, int destinationAddressId, int numberOfShipmentsInOrder)
		{
			IEnumerable<CartItem> addrCart = cartItems.Where(ci => ci.ShippingAddressID == destinationAddressId);
			CartItemCollection cartItemsForAddress = new CartItemCollection(addrCart);

			bool taxCalcModeBilling = AppLogic.AppConfig("taxcalcmode").Equals("Billing", StringComparison.InvariantCultureIgnoreCase);
			decimal ExtraFee = AppLogic.AppConfigUSDecimal("ShippingHandlingExtraFee");

			//Check every single cart item so that if a single item has a shipping it will return the
			//right shipping method id
			bool doesCartItemShippingExist = false;
			int CartItemPosition = 0, ShippingMethodID = 0;
			int i = -1;
			if(cartItems.ContainsRecurring)
			{
				foreach(CartItem ci in cartItems)
				{
					i++;
					if(ci.ShippingMethodID != 0)
					{
						doesCartItemShippingExist = true;
						CartItemPosition = i;
					}
				}
			}

			if(doesCartItemShippingExist && CartItemPosition < cartItemsForAddress.Count)
			{
				ShippingMethodID = (cartItemsForAddress[CartItemPosition]).ShippingMethodID;
			}
			else
			{
				ShippingMethodID = (cartItemsForAddress.FirstOrDefault()).ShippingMethodID;
			}

			Decimal TotalWeight = cartItemsForAddress.WeightTotal(addrCart);
			bool IsAllFreeShipping = cartItemsForAddress.IsAllFreeShippingComponents;
			bool vatOn = AppLogic.AppConfigBool("VAT.Enabled") && customer.VATSettingReconciled == VATSettingEnum.ShowPricesInclusiveOfVAT;
			cartItemsForAddress.CouponList = cartItems.CouponList;
			Decimal sum = Prices.SubTotal(includeDiscount, false, false, IsAllFreeShipping, true, false, cartItemsForAddress.FirstOrDefault().BillingAddressID, vatOn, cartItemsForAddress, customer, orderOptions);
			Decimal FreeShippingThreshold = AppLogic.AppConfigUSDecimal("FreeShippingThreshold");

			// first case, free shipping threshhold is on
			bool freeShippingByThreshold = (FreeShippingThreshold != System.Decimal.Zero &&
											sum >= FreeShippingThreshold &&
											Shipping.ShippingMethodIsInFreeList(ShippingMethodID));

			bool freeShippingByCustomerLevel = customer.LevelHasFreeShipping() && Shipping.ShippingMethodIsInFreeList(ShippingMethodID);
			
			decimal shippingCost = Decimal.Zero;

			if (freeShippingByThreshold || freeShippingByCustomerLevel)
			{
				shippingCost += AppLogic.AppConfigUSDecimal("ShippingHandlingExtraFee");
			}
			else if(!Shipping.NoShippingRequiredComponents(cartItemsForAddress))
			{
				decimal ThisAddressShipCost = decimal.Zero;
				switch(Shipping.GetActiveShippingCalculationID())
				{
					case Shipping.ShippingCalculationEnum.CalculateShippingByWeight:
						{
							ThisAddressShipCost = Shipping.GetShipByWeightCharge(ShippingMethodID, TotalWeight);
							break;
						}
					case Shipping.ShippingCalculationEnum.CalculateShippingByTotal:
						{
							ThisAddressShipCost = Shipping.GetShipByTotalCharge(ShippingMethodID, sum);
							break;
						}
					case Shipping.ShippingCalculationEnum.CalculateShippingByTotalByPercent:
						{
							ThisAddressShipCost = Shipping.GetShipByTotalByPercentCharge(ShippingMethodID, sum);
							break;
						}
					case Shipping.ShippingCalculationEnum.UseFixedPrice:
						{
							ThisAddressShipCost = Shipping.GetShipByFixedPrice(ShippingMethodID, sum);
							break;
						}
					case Shipping.ShippingCalculationEnum.AllOrdersHaveFreeShipping:
						{
							ThisAddressShipCost = System.Decimal.Zero;
							break;
						}
					case Shipping.ShippingCalculationEnum.UseFixedPercentageOfTotal:
						{
							decimal shipPercent = System.Decimal.Zero;

							try
							{
								string query = "Select * from ShippingMethod   with (NOLOCK)  where ShippingMethodID=" + ShippingMethodID.ToString();

								using(SqlConnection conn = new SqlConnection(DB.GetDBConn()))
								{
									conn.Open();
									using(IDataReader rs = DB.GetRS(query, conn))
									{
										if(rs.Read())
										{
											shipPercent = DB.RSFieldDecimal(rs, "FixedPercentOfTotal");
										}
									}
								}
							}
							catch { throw; }

							ThisAddressShipCost = sum * (shipPercent / 100.0M);
							break;
						}
					case Shipping.ShippingCalculationEnum.UseIndividualItemShippingCosts:
						ThisAddressShipCost = Shipping.GetShipByItemCharge(ShippingMethodID, cartItemsForAddress);
						break;
					case Shipping.ShippingCalculationEnum.CalculateShippingByWeightAndZone:
						{
							Address shippingAddress = new Address();
							shippingAddress.LoadByCustomer(customer.CustomerID, CommonLogic.IIF(taxCalcModeBilling, cartItemsForAddress.FirstOrDefault().BillingAddressID, cartItemsForAddress.FirstOrDefault().ShippingAddressID), AddressTypes.Shipping);
							int ZoneID = Shipping.ZoneLookup(shippingAddress.Zip);
							decimal shippingcost = Shipping.GetShipByWeightAndZoneCharge(ShippingMethodID, TotalWeight, ZoneID);
							ThisAddressShipCost = CommonLogic.IIF(shippingcost == -1, 0, shippingcost);
							break;
						}
					case Shipping.ShippingCalculationEnum.CalculateShippingByTotalAndZone:
						{
							Address shippingAddress = new Address();
							shippingAddress.LoadByCustomer(customer.CustomerID, CommonLogic.IIF(taxCalcModeBilling, cartItemsForAddress.FirstOrDefault().BillingAddressID, cartItemsForAddress.FirstOrDefault().ShippingAddressID), AddressTypes.Shipping);
							int ZoneID = Shipping.ZoneLookup(shippingAddress.Zip);
							decimal shippingcost = Shipping.GetShipByTotalAndZoneCharge(ShippingMethodID, Prices.SubTotal(true, false, false, false, true, false, CommonLogic.IIF(taxCalcModeBilling, cartItemsForAddress.FirstOrDefault().BillingAddressID, cartItemsForAddress.FirstOrDefault().ShippingAddressID), true, cartItemsForAddress, customer, orderOptions), ZoneID);
							ThisAddressShipCost = CommonLogic.IIF(shippingcost == -1, 0, shippingcost);
							break;
						}
					case Shipping.ShippingCalculationEnum.UseRealTimeRates:
						// get the rate from the firstitem shippingmethod field
						String rt = cartItemsForAddress.FirstOrDefault().ShippingMethod;
						if(rt.IndexOf('|') != -1)
						{
							ThisAddressShipCost = Localization.ParseUSDecimal(rt.Split('|')[1]);
						}
						break;
				}
                
				// now add fixed "handling" fee for all non-zero shipping totals
				// Note: RTShipping already has the extra fee factored in the select lists!
				// note: rtrates ALREADY have the extra fee in them!

				bool isAllEmailGiftCard = cartItemsForAddress.IsAllEmailGiftCards;
				bool isRealTime = Shipping.GetActiveShippingCalculationID() == Shipping.ShippingCalculationEnum.UseRealTimeRates;

                if (ExtraFee != System.Decimal.Zero && !isRealTime && ThisAddressShipCost != System.Decimal.Zero
                        && !isAllEmailGiftCard && !cartItemsForAddress.IsAllDownloadComponents)
                {
                    ThisAddressShipCost += ExtraFee;
                }

				shippingCost += ThisAddressShipCost;

				decimal VAT = 0.0M;
				if(vatOn && includeTax)
				{
					Address shipAdr = new Address();
					shipAdr.LoadByCustomer(customer.CustomerID, CommonLogic.IIF(taxCalcModeBilling, cartItemsForAddress.FirstOrDefault().BillingAddressID, cartItemsForAddress.FirstOrDefault().ShippingAddressID), AddressTypes.Shipping);
					VAT = ShippingTax(ThisAddressShipCost, shipAdr, customer);
				}
				shippingCost += VAT;
			}

            if (cartItems.HasDiscountResults)
                shippingCost += cartItems.DiscountResults.Sum(dr => dr.ShippingTotal);

			// Add COD fee for non-zero shipping cost COD orders
			if(shippingCost > 0 && AppLogic.CleanPaymentMethod(customer.PrimaryBillingAddress.PaymentMethodLastUsed) == AppLogic.ro_PMCOD)
			{
				// Spread the COD fee across all shipments
				decimal codFeeForThisAddress = AppLogic.AppConfigUSDecimal("CODHandlingExtraFee") / numberOfShipmentsInOrder;
				shippingCost += codFeeForThisAddress;
			}
            
			return shippingCost;
		}

        #endregion

        #region Total

        public static Decimal Total(bool includeDiscount, CartItemCollection cic, Customer ThisCustomer, IEnumerable<OrderOption> OrderOptions, Boolean IncludeTax)
        {
            Decimal orderTotal = System.Decimal.Zero;

            bool VATEnabled = AppLogic.AppConfigBool("VAT.Enabled");
            bool VATOn = (VATEnabled && ThisCustomer.VATSettingReconciled == VATSettingEnum.ShowPricesInclusiveOfVAT);
            Decimal sTotal = SubTotal(includeDiscount, false, true, true, true, false, 0, true, cic, ThisCustomer, OrderOptions);
            Decimal shTotal = ShippingTotal(includeDiscount, VATOn, cic, ThisCustomer, OrderOptions);
            Decimal tTotal = 0;
            
            if (IncludeTax)
                tTotal = Prices.TaxTotal(ThisCustomer, cic, shTotal, OrderOptions);

            RemoveTaxRelatedCartItems(cic);

            // Promotions: Line Item and Shipping discounts happen in the individual calculations so all that's left is to apply order level discounts.
            var orderDiscount = 0.00M;
            if (includeDiscount && cic.HasDiscountResults)
            {
                orderDiscount = cic.DiscountResults.Sum(dr => dr.OrderTotal);
                orderDiscount = Math.Round(orderDiscount, 2, MidpointRounding.AwayFromZero);
            }

            orderTotal = sTotal + orderDiscount;
            
            // Promotions: Because multiple promotions can be applied, it's possible to get a negative value, which should be caught and zeroed out.
            if (orderTotal < 0)
                orderTotal = 0;

            // Shipping and Tax can never be discounted so it's added after discounts.
            orderTotal += Math.Round(shTotal, 2, MidpointRounding.AwayFromZero);
            orderTotal += Math.Round(tTotal, 2, MidpointRounding.AwayFromZero);

            return orderTotal;
        }
        
        #endregion

        #region Customer Level Discount

        /// <summary>
        /// Calculates a customer level discount for an order
        /// </summary>
        /// <returns>Decimal total after discount</returns>
        public static Decimal GetOrderCustomerLevelDiscount(Decimal st, Customer ThisCustomer)
        {
            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                using (IDataReader rs = DB.GetRS("select LevelDiscountAmount from dbo.CustomerLevel with(NOLOCK) where CustomerLevelID = @CustomerLevelId", new SqlParameter[] { new SqlParameter("CustomerLevelId", ThisCustomer.CustomerLevelID) }, conn))
                {
                    if (rs.Read())
                    {
                        st = st - DB.RSFieldDecimal(rs, "LevelDiscountAmount");
                    }
                }
            }

            return st;
        }

        #endregion

        #region GetQuantityDiscount

        /// <summary>
        /// 
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="quantity"></param>
        /// <param name="priceRate"></param>
        /// <param name="fixedPriceDID"></param>
        /// <param name="discountPercent"></param>
        /// <param name="ThisCustomer"></param>
        /// <returns></returns>
        public static decimal GetQuantityDiscount(int productId, int quantity, Decimal priceRate, QuantityDiscount.QuantityDiscountType fixedPriceDID, Decimal discountPercent, Customer ThisCustomer)
        {
            Decimal DIDPercent = 0.0M;
            Decimal DiscountedItemPrice = priceRate;


            if (fixedPriceDID != QuantityDiscount.QuantityDiscountType.None)
            {
                // Make sure customer isn't in a customer level that disallows quantity discounts
                if (QuantityDiscount.CustomerLevelAllowsQuantityDiscounts(ThisCustomer.CustomerLevelID))
                {
                    if (DIDPercent != 0.0M)
                    {
                        if (fixedPriceDID == QuantityDiscount.QuantityDiscountType.FixedAmount)
                        {
                            if (Currency.GetDefaultCurrency() == ThisCustomer.CurrencySetting)
                            {
                                DiscountedItemPrice = (priceRate - DIDPercent);
                            }
                        }
                        else
                        {
                            DiscountedItemPrice = priceRate * ((100.0M - DIDPercent) / 100.0M);
                        }
                    }
                }
            }

            return DiscountedItemPrice;
            //if (fixedPriceDID != QuantityDiscount.QuantityDiscountType.None)
            //{
            //    if (fixedPriceDID == QuantityDiscount.QuantityDiscountType.FixedAmount)
            //    {
            //        string storeCurrency = Localization.StoreCurrency();
            //        bool customerCurrencyIsDefault = storeCurrency == ThisCustomer.CurrencySetting;
            //        if (customerCurrencyIsDefault)
            //        {
            //            priceRate = priceRate - discountPercent;
            //        }
            //        else
            //        {
            //            discountPercent = Currency.Convert(discountPercent, storeCurrency, ThisCustomer.CurrencySetting);
            //            // round
            //            discountPercent = Decimal.Round(discountPercent, 2, MidpointRounding.AwayFromZero);

            //            priceRate = ((priceRate - discountPercent) * (decimal)quantity);
            //        }
            //    }
            //    else
            //    {
            //        priceRate = ((100.0M - discountPercent) / 100.0M) * priceRate;
            //    }
            //}

            //return priceRate;
        }

        /// <summary>
        /// Applies a quantity discount to a line item total
        /// </summary>
        /// <param name="ci"></param>
        /// <param name="ThisCustomer"></param>
        /// <returns>Decimal total after discount</returns>
        public static Decimal GetQuantityDiscount(CartItem ci, Customer ThisCustomer)
        {
            Decimal DIDPercent = 0.0M;
            Decimal DiscountedItemPrice = ci.Price;
            QuantityDiscount.QuantityDiscountType fixedPriceDID = QuantityDiscount.QuantityDiscountType.Percentage;

            // Make sure customer isn't in a customer level that disallows quantity discounts
            if (QuantityDiscount.CustomerLevelAllowsQuantityDiscounts(ThisCustomer.CustomerLevelID))
            {
                DIDPercent = QuantityDiscount.GetQuantityDiscountTablePercentageForLineItem(ci, out fixedPriceDID);
                if (DIDPercent != 0.0M)
                {
                    if (fixedPriceDID == QuantityDiscount.QuantityDiscountType.FixedAmount)
                    {
                        if (Currency.GetDefaultCurrency() == ThisCustomer.CurrencySetting)
                        {
                            DiscountedItemPrice = (ci.Price - DIDPercent);
                        }
                    }
                    else
                    {
                        DiscountedItemPrice = ci.Price * ((100.0M - DIDPercent) / 100.0M);
                    }
                }
            }

            // A failsafe check to insure that a flat rate discount never discounts an item into the negative.
            if (DiscountedItemPrice < 0)
                DiscountedItemPrice = 0;

            return DiscountedItemPrice;
        }

        #endregion
                
        #region Product Pricing

        /// <summary>
        /// Get defined ProductVariant for the Shipping Item used by Avalara
        /// </summary>
        /// <returns>ProductVariant object.</returns>
        public static int GetShippingProductVariant()
        {
            int SPV = 0;
            bool Exists = false;
            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                using (IDataReader rs = DB.GetRS("SELECT * from dbo.ProductVariant with(NOLOCK) where Name='ShippingProductVariant' AND SKUSuffix='Shipping'", conn))
                {
                    if (rs.Read())
                    {
                        int VariantID = DB.RSFieldInt(rs, "VariantID");
                        Exists = true;
                        SPV = VariantID;
                    }
                }
            }

            if (!Exists)
            {
                ProductVariant NewSPV = new ProductVariant();
                NewSPV.VariantGUID = Guid.NewGuid().ToString("D");
                NewSPV.IsDefault = true;
                NewSPV.Name = "ShippingProductVariant";
                NewSPV.ProductID = 0;
                NewSPV.SKUSuffix = "Shipping";
                NewSPV.Price = Decimal.Zero;
                NewSPV.Create();
                SPV = AppLogic.GetVariantID(NewSPV.VariantGUID);
            }

            return SPV;

        }

        /// <summary>
        /// Calculates the price, with discounts if they exist, for upsell items
        /// </summary>
        /// <param name="SourceProductID">The ProductID of the product that has the upsell product</param>
        /// <param name="UpsellProductID">The ProductID of the upsell product</param>
        /// <param name="CustomerLevelID">The CustomerLevelID of the current customer</param>
        /// <returns>Decimal price, rounded to 2 decimal places</returns>
        static public Decimal GetUpsellProductPrice(int SourceProductID, int UpsellProductID, int CustomerLevelID)
        {
            Decimal UpsellProductDiscountPercentage = 0.0M;

            if (SourceProductID == 0)
            {
                string sql = "select top 1 UpsellProductDiscountPercentage N From dbo.product with(NOLOCK) where deleted = 0 and published = 1 and charindex(','+@UpsellProductID+',', ','+convert(nvarchar(4000), upsellproducts)+',') > 0 order by productid";
                SqlParameter[] spa = { DB.CreateSQLParameter("@UpsellProductID", SqlDbType.VarChar, 10, UpsellProductID.ToString(), ParameterDirection.Input) };
                UpsellProductDiscountPercentage = DB.GetSqlNDecimal(sql, spa);
            }
            else
            {
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS("Select UpsellProducts,UpsellProductDiscountPercentage from dbo.Product with (NOLOCK) where ProductID=" + SourceProductID.ToString(), con))
                    {
                        if (rs.Read())
                        {
                            UpsellProductDiscountPercentage = DB.RSFieldDecimal(rs, "UpsellProductDiscountPercentage");
                        }
                    }
                }
            }
            bool IsOnSale = true; // don't care really
            Decimal PR = AppLogic.DetermineLevelPrice(AppLogic.GetProductsDefaultVariantID(UpsellProductID), CustomerLevelID, out IsOnSale);
            if (UpsellProductDiscountPercentage != 0.0M)
            {
                PR = PR * (Decimal)(1 - (UpsellProductDiscountPercentage / 100.0M));
            }

            // return the price, rounded to 2 decimal places
            return PR;
            //return PR;
        }

        /// <summary>
        /// Used for backward compatibility with AppLogic.DetermineLevelPrice
        /// </summary>
        /// <param name="VariantID"></param>
        /// <param name="CustomerLevelID"></param>
        /// <param name="IsOnSale"></param>
        /// <returns></returns>
        static public decimal DetermineLevelPrice(int VariantID, int CustomerLevelID, out bool IsOnSale)
        {
            // the way the site is written, this should NOT be called with CustomerLevelID=0 but, you never know
            // if that's the case, return the sale price if any, and if not, the regular price instead:
            decimal pr = System.Decimal.Zero;
            IsOnSale = false;
            if (CustomerLevelID == 0)
            {
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS("select * from productvariant   with (NOLOCK)  where VariantID=" + VariantID.ToString(), con))
                    {
                        if (rs.Read())
                        {
                            if (DB.RSFieldDecimal(rs, "SalePrice") != System.Decimal.Zero)
                            {
                                pr = DB.RSFieldDecimal(rs, "SalePrice");
                                IsOnSale = true;
                            }
                            else
                            {
                                pr = DB.RSFieldDecimal(rs, "Price");
                            }
                        }
                        else
                        {
                            // well, this is bad, we can't return 0, and we don't have ANY valid price to return...stop the web page!
                            throw (new ApplicationException("Invalid Variant Price Structure, VariantID=" + VariantID.ToString()));
                        }
                    }
                }
            }
            else
            {
                // ok, now for the hard part (e.g. the fun)
                // determine the actual price for this thing, considering everything involved!
                // If we have an extended price, get that first!
                bool ExtendedPriceFound = false;

                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS("select Price from ExtendedPrice  with (NOLOCK)  where VariantID=" + VariantID.ToString() + " and CustomerLevelID=" + CustomerLevelID.ToString() + " and VariantID in (select VariantID from ProductVariant where ProductID in (select ProductID from ProductCustomerLevel where CustomerLevelID=" + CustomerLevelID.ToString() + "))", con))
                    {
                        if (rs.Read())
                        {
                            pr = DB.RSFieldDecimal(rs, "Price");
                            ExtendedPriceFound = true;
                        }
                    }
                }

                if (!ExtendedPriceFound)
                {
                    pr = GetVariantPrice(VariantID);
                }

                // now get the "level" info:
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS("select * from CustomerLevel   with (NOLOCK)  where CustomerLevelID=" + CustomerLevelID.ToString(), con))
                    {
                        if (rs.Read())
                        {
                            Decimal DiscountPercent = DB.RSFieldDecimal(rs, "LevelDiscountPercent");
                            bool LevelDiscountsApplyToExtendedPrices = DB.RSFieldBool(rs, "LevelDiscountsApplyToExtendedPrices");
                            rs.Close();
                            if (DiscountPercent != 0.0M)
                            {
                                if (!ExtendedPriceFound || (ExtendedPriceFound && LevelDiscountsApplyToExtendedPrices))
                                {
                                    pr = pr * (decimal)(1.00M - (DiscountPercent / 100.0M));
                                }
                            }
                        }
                    }
                }
            }

            // WEBOPIUS, was 2
            // return the price, rounded to 4 decimal places
            return Decimal.Round(pr, 4, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Used for backward compatibility with AppLogic.GetVariantPrice
        /// </summary>
        /// <param name="VariantID"></param>
        /// <returns></returns>
        static public decimal GetVariantPrice(int VariantID)
        {
            decimal pr = System.Decimal.Zero;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select Price from productvariant   with (NOLOCK)  where VariantID=" + VariantID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        pr = DB.RSFieldDecimal(rs, "Price");
                    }
                }
            }

            return pr;
        }

        /// <summary>
        /// Used for backward compatibility with AppLogic.VariantPriceLookup
        /// </summary>
        /// <param name="ThisCustomer"></param>
        /// <param name="VariantID"></param>
        /// <returns></returns>
        public static decimal VariantPriceLookup(Customer ThisCustomer, int VariantID)
        {
            decimal tmp = System.Decimal.Zero;
            int CL = 0;
            if (ThisCustomer != null)
            {
                CL = ThisCustomer.CustomerLevelID;
            }

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("SELECT pv.VariantID, pv.Price, isnull(pv.SalePrice, 0) SalePrice, isnull(e.Price, 0) ExtendedPrice FROM ProductVariant pv with (NOLOCK) left join ExtendedPrice e with (NOLOCK) on pv.VariantID=e.VariantID and e.CustomerLevelID=" + CL.ToString() + " WHERE pv.VariantID=" + VariantID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        if (DB.RSFieldDecimal(rs, "ExtendedPrice") != System.Decimal.Zero)
                        {
                            tmp = DB.RSFieldDecimal(rs, "ExtendedPrice");
                        }
                        else if (DB.RSFieldDecimal(rs, "SalePrice") != System.Decimal.Zero)
                        {
                            tmp = DB.RSFieldDecimal(rs, "SalePrice");
                        }
                        else
                        {
                            tmp = DB.RSFieldDecimal(rs, "Price");
                        }
                    }
                }
            }

            return tmp;
        }

        /// <summary>
        /// Used for backward compatibility with AppLogic.GetKitTotalPrice
        /// </summary>
        /// <param name="CustomerID"></param>
        /// <param name="CustomerLevelID"></param>
        /// <param name="ProductID"></param>
        /// <param name="VariantID"></param>
        /// <param name="ShoppingCartRecID"></param>
        /// <returns></returns>
        static public decimal GetKitTotalPrice(int CustomerID, int CustomerLevelID, int ProductID, int VariantID, int ShoppingCartRecID)
        {
            Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)HttpContext.Current.User).ThisCustomer;
            decimal tmp = System.Decimal.Zero;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("SELECT Product.*, ProductVariant.Price, ProductVariant.SalePrice FROM Product   with (NOLOCK)  inner join productvariant   with (NOLOCK)  on product.productid=productvariant.productid where ProductVariant.VariantID=" + VariantID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        decimal BasePrice = System.Decimal.Zero;
                        if (DB.RSFieldDecimal(rs, "SalePrice") != System.Decimal.Zero && CustomerLevelID == 0)
                        {
                            BasePrice = DB.RSFieldDecimal(rs, "SalePrice");
                        }
                        else
                        {
                            bool isonsale = false; // not used
                            BasePrice = AppLogic.DetermineLevelPrice(VariantID, CustomerLevelID, out isonsale);
                        }
                        decimal KitPriceDelta = AppLogic.KitPriceDelta(CustomerID, ProductID, ShoppingCartRecID);
                        tmp = BasePrice + (KitPriceDelta * (1 - (ThisCustomer.LevelDiscountPct / 100)));
                    }
                }
            }

            return tmp;
        }

        /// <summary>
        /// Used for backward compatibility with AppLogic.GetProductSalePrice
        /// </summary>
        /// <param name="ProductID"></param>
        /// <param name="ViewingCustomer"></param>
        /// <returns></returns>
        public static String GetProductSalePrice(int ProductID, Customer ViewingCustomer)
        {
            // NOTE: IGNORE ANY EXTENDED PRICING HERE, THIS ALWAYS RETURNS NORMAL CUSTOMER PRICE AND SALE PRICE
            // YOU COULD ALTER THAT, BUT IT'S PROBABLY NOT NECESSARY, SPECIALS ARE TYPICALLY ONLY FOR "CONSUMERS"
            // return string in format: $regularprice,$saleprice (note that $saleprice could be empty), and
            // note that this proc returns the FIRST sales price of any variant found, if there are multiple sales prices
            // then you have to write a different proc if you want them returned.
            String tmpS = String.Empty;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select * from product   with (NOLOCK)  left outer join productvariant   with (NOLOCK)  on product.productid=productvariant.productid where saleprice IS NOT NULL and saleprice<>price and product.productid=" + ProductID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmpS = ViewingCustomer.CurrencyString(DB.RSFieldDecimal(rs, "Price")) + "|" + ViewingCustomer.CurrencyString(DB.RSFieldDecimal(rs, "SalePrice"));
                    }
                }
            }

            return tmpS;
        }

        /// <summary>
        /// Used for backward compatibility with AppLogic.KitPriceDelta
        /// </summary>
        /// <param name="CustomerID"></param>
        /// <param name="ProductID"></param>
        /// <param name="ShoppingCartRecID"></param>
        /// <param name="ForCurrency"></param>
        /// <returns></returns>
        static public decimal KitPriceDelta(int CustomerID, int ProductID, int ShoppingCartRecID, string ForCurrency)
        {
            decimal tmp = System.Decimal.Zero;
            if (CustomerID != 0)
            {
                string sql = String.Empty;
                if (ForCurrency == Localization.StoreCurrency())
                {
                    sql = "select sum(pricedelta) as PR from kitcart   with (NOLOCK)  inner join kititem   with (NOLOCK)  on kitcart.kititemid=kitItem.kititemid where customerid=" + CustomerID.ToString() + " and productid=" + ProductID.ToString() + " and ShoppingCartrecid=" + ShoppingCartRecID.ToString();
                }
                else
                {
                    sql = "select sum(round(pricedelta*dbo.ExchangeRate(" + DB.SQuote(ForCurrency) + "), 2)) as PR from kitcart   with (NOLOCK)  inner join kititem   with (NOLOCK)  on kitcart.kititemid=kitItem.kititemid where customerid=" + CustomerID.ToString() + " and productid=" + ProductID.ToString() + " and ShoppingCartrecid=" + ShoppingCartRecID.ToString();
                }

                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS(sql, con))
                    {
                        if (rs.Read())
                        {
                            tmp = DB.RSFieldDecimal(rs, "PR");
                        }
                    }
                }
            }
            return tmp;
        }

        /// <summary>
        /// Used for backward compatibility with AppLogic.GetVariantSalePrice
        /// </summary>
        /// <param name="VariantID"></param>
        /// <returns></returns>
        static public decimal GetVariantSalePrice(int VariantID)
        {
            decimal pr = System.Decimal.Zero;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select SalePrice from productvariant   with (NOLOCK)  where VariantID=" + VariantID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        pr = DB.RSFieldDecimal(rs, "SalePrice");
                    }
                }
            }

            return pr;
        }

        /// <summary>
        /// Used for backwards compatibility with XSLTExtensionBase.GetUpsellVariantPrice
        /// </summary>
        /// <param name="ThisCustomer"></param>
        /// <param name="sVariantID"></param>
        /// <param name="sHidePriceUntilCart"></param>
        /// <param name="sPrice"></param>
        /// <param name="sSalePrice"></param>
        /// <param name="sExtPrice"></param>
        /// <param name="sPoints"></param>
        /// <param name="sSalesPromptName"></param>
        /// <param name="sShowpricelabel"></param>
        /// <param name="sTaxClassID"></param>
        /// <param name="decUpSelldiscountPct"></param>
        /// <returns></returns>
        public static string GetUpsellVariantPrice(Customer ThisCustomer, String sVariantID, String sHidePriceUntilCart, string sPrice, string sSalePrice, string sExtPrice, String sPoints, string sSalesPromptName, String sShowpricelabel, string sTaxClassID, string decUpSelldiscountPct) 
        {
            return GetUpsellVariantPrice(ThisCustomer, sVariantID, sHidePriceUntilCart, sPrice, sSalePrice, sExtPrice, sPoints, sSalesPromptName, sShowpricelabel, sTaxClassID, decUpSelldiscountPct, "0", string.Empty, string.Empty);
        }
        public static string GetUpsellVariantPrice(Customer ThisCustomer, String sVariantID, String sHidePriceUntilCart, string sPrice, string sSalePrice, string sExtPrice, String sPoints, string sSalesPromptName, String sShowpricelabel, string sTaxClassID, string decUpSelldiscountPct, string sProductId, string sStartDate, string sEndDate)
        {
            bool m_VATEnabled = AppLogic.AppConfigBool("VAT.Enabled");
            bool m_VATOn = (m_VATEnabled && ThisCustomer.VATSettingReconciled == VATSettingEnum.ShowPricesInclusiveOfVAT);

            InputValidator IV = new InputValidator("GetVariantPrice");
            bool HidePriceUntilCart = IV.ValidateBool("HidePriceUntilCart", sHidePriceUntilCart);
            Decimal Price = IV.ValidateDecimal("Price", sPrice);
            Decimal SalePrice = IV.ValidateDecimal("SalePrice", sSalePrice);
            Decimal ExtPrice = IV.ValidateDecimal("ExtPrice", sExtPrice);
            int TaxClassID = IV.ValidateInt("TaxClassID", sTaxClassID);
            Decimal UpSelldiscountPct = IV.ValidateDecimal("UpSelldiscountPct", decUpSelldiscountPct);

            StringBuilder results = new StringBuilder(1024);

            Decimal vatPrice = Price;
            Decimal vatSalePrice = SalePrice;
            Decimal vatExtPrice = ExtPrice;

            decimal TaxRate = Prices.TaxRate(ThisCustomer, TaxClassID);
            Decimal TaxMultiplier = (1.0M + (TaxRate / 100.00M));
            if (m_VATOn)
            {
                vatPrice = TaxMultiplier * Price;
                vatSalePrice = TaxMultiplier * SalePrice;
                vatExtPrice = TaxMultiplier * ExtPrice;
            }

            Decimal VariantPrice;
            if (HidePriceUntilCart
                || AppLogic.HideForWholesaleSite(ThisCustomer.CustomerLevelID))
            {
                // hide price
                VariantPrice = Decimal.MinusOne;
            }
            else
            {
                if (ThisCustomer.CustomerLevelID == 0 || (ThisCustomer.LevelDiscountPct == 0.0M && ExtPrice == 0.0M))
                {
                    // show consumer pricing (e.g. level 0):
                    VariantPrice = vatPrice;
                    if (SalePrice != Decimal.Zero)
                    {
                        VariantPrice = vatSalePrice;
                    }
                }
                else
                {
                    VariantPrice = CommonLogic.IIF(ExtPrice == 0.0M, Price, ExtPrice);
                    if (ThisCustomer.LevelDiscountPct != 0.0M && (ExtPrice == 0.0M || (ExtPrice > 0.0M && ThisCustomer.DiscountExtendedPrices)))
                    {
                        VariantPrice = VariantPrice * (decimal)(1.00M - (ThisCustomer.LevelDiscountPct / 100.0M)) * CommonLogic.IIF(m_VATOn, TaxMultiplier, 1.0M);
                    }
                }
            }

            if (VariantPrice == Decimal.MinusOne)
            {
                results.Append("&nbsp;");
            }
            else
            {
				if ((UpSelldiscountPct * VariantPrice) > 0)
				{
					var storeDefaultCultureInfo = CultureInfo.GetCultureInfo(Localization.GetDefaultLocale());
					var formattedSchemaPrice = String.Format(storeDefaultCultureInfo, "{0:C}", UpSelldiscountPct * VariantPrice);
					var schemaRegionInfo = new RegionInfo(storeDefaultCultureInfo.Name);
                    XSLTExtensionBase XSLTExtensionBaseVariable = new XSLTExtensionBase(ThisCustomer, AppLogic.GetStoreSkinID(AppLogic.StoreID()));

                    results.AppendFormat("<span itemprop=\"offers\" itemscope itemtype=\"{0}://schema.org/Offer\">{1}", HttpContext.Current.Request.Url.Scheme, Environment.NewLine);

                    int numValue = 0;
                    int numValue2 = 0;
                    bool productIdIsNumber = int.TryParse(sProductId, out numValue);
                    bool variantIdIsNumber = int.TryParse(sVariantID, out numValue2);
                    if (productIdIsNumber && variantIdIsNumber && Convert.ToInt32(sProductId) > 0 && Convert.ToInt32(sVariantID) > 0) 
                    {                        
                        string stockStatusText = XSLTExtensionBaseVariable.GetStockStatusText(sProductId, sVariantID, "Product");
                        if (stockStatusText.Length > 0)
                        {
                            results.AppendFormat("<link itemprop=\"availability\" href=\"{0}://schema.org/{1}\" content=\"{2}\"/>{3}", HttpContext.Current.Request.Url.Scheme, stockStatusText.Split('|')[0], stockStatusText.Split('|')[1], Environment.NewLine);
                        }
                    }
                    if (sStartDate.Length > 0)
                    {
                        string sStartDateFormatted = XSLTExtensionBaseVariable.GetISODateTime(sStartDate);
                        if (sStartDateFormatted.Length > 0)
                        {
                            results.AppendFormat("<meta itemprop=\"availabilityStarts\" content=\"{0}\"/>{1}", sStartDateFormatted, Environment.NewLine);
                        }
                    }
                    if (sEndDate.Length > 0)
                    {
                        string sEndDateFormatted = XSLTExtensionBaseVariable.GetISODateTime(sEndDate);
                        if (sEndDateFormatted.Length > 0)
                        {
                            results.AppendFormat("<meta itemprop=\"availabilityEnds\" content=\"{0}\"/>{1}", sEndDateFormatted, Environment.NewLine);
                        }
                    }

                    results.AppendFormat("<meta itemprop=\"price\" content=\"{0}\"/>{1}", formattedSchemaPrice, Environment.NewLine);
                    results.AppendFormat("<meta itemprop=\"priceCurrency\" content=\"{0}\"/>{1}", schemaRegionInfo.ISOCurrencySymbol, Environment.NewLine);
					results.Append("</span>");
				}
				
                results.Append(Localization.CurrencyStringForDisplayWithExchangeRate(UpSelldiscountPct * VariantPrice, ThisCustomer.CurrencySetting));
            }

            return results.ToString();
        }

        /// <summary>
        /// Used for backwards compatibility with XSLTExtensionBase.GetCartPrice
        /// </summary>
        /// <param name="ThisCustomer"></param>
        /// <param name="intProductID"></param>
        /// <param name="intQuantity"></param>
        /// <param name="decProductPrice"></param>
        /// <param name="intTaxClassID"></param>
        /// <returns></returns>
        public static string GetCartPrice(Customer ThisCustomer, string intProductID, string intQuantity, string decProductPrice, string intTaxClassID)
        {
            InputValidator IV = new InputValidator("GetCartPrice");
            int ProductID = IV.ValidateInt("ProductID", intProductID);
            int TaxClassID = IV.ValidateInt("TaxClassID", intTaxClassID);
            int Quantity = IV.ValidateInt("Quantity", intQuantity);
            Decimal Price = IV.ValidateDecimal("Price", decProductPrice);

            if (Currency.GetDefaultCurrency() != ThisCustomer.CurrencySetting)
            {
                Price = Decimal.Round(Currency.Convert(Price, Localization.StoreCurrency(), ThisCustomer.CurrencySetting), 2, MidpointRounding.AwayFromZero);
            }

            decimal TaxRate = 0.0M;
            bool VATEnabled = AppLogic.AppConfigBool("VAT.Enabled");
            bool VATOn = (VATEnabled && ThisCustomer.VATSettingReconciled == VATSettingEnum.ShowPricesInclusiveOfVAT);
            if (VATEnabled)
            {
                TaxRate = Prices.TaxRate(ThisCustomer, TaxClassID);
            }

            int ProductQty = DB.GetSqlN("select sum(quantity) N from shoppingcart where carttype = 0 and customerid = " + ThisCustomer.CustomerID.ToString() + " and productid = " + intProductID);

            int Q = Quantity;
            decimal PR = Price * (decimal)Q;
            Decimal DIDPercent = 0.0M;
            QuantityDiscount.QuantityDiscountType fixedPriceDID = QuantityDiscount.QuantityDiscountType.Percentage;
            if (QuantityDiscount.CustomerLevelAllowsQuantityDiscounts(ThisCustomer.CustomerLevelID))
            {
                DIDPercent = QuantityDiscount.GetQuantityDiscountTablePercentageWithoutCartAwareness(ProductID, ProductQty, out fixedPriceDID);
                if (DIDPercent != 0.0M)
                {
                    if (fixedPriceDID == QuantityDiscount.QuantityDiscountType.FixedAmount)
                    {
                        if (Currency.GetDefaultCurrency() == ThisCustomer.CurrencySetting)
                        {
                            PR = (Price - DIDPercent) * (Decimal)Q;
                        }
                        else
                        {
                            DIDPercent = Decimal.Round(Currency.Convert(DIDPercent, Localization.StoreCurrency(), ThisCustomer.CurrencySetting), 2, MidpointRounding.AwayFromZero);
                            PR = (Price - DIDPercent) * (Decimal)Q;
                        }
                    }
                    else
                    {
                        PR = PR * ((100.0M - DIDPercent) / 100.0M);
                    }
                }
            }
            decimal VAT = PR * (TaxRate / 100.0M);
            if (VATOn)
            {
                PR += VAT;
            }
            StringBuilder results = new StringBuilder();
            results.Append(Localization.CurrencyStringForDisplayWithoutExchangeRate(PR, ThisCustomer.CurrencySetting));
            if (VATEnabled)
            {
                results.Append("&nbsp;");
                if (VATOn)
                {
                    results.Append(AppLogic.GetString("setvatsetting.aspx.6", ThisCustomer.SkinID, ThisCustomer.LocaleSetting));
                }
                else
                {
                    results.Append(AppLogic.GetString("setvatsetting.aspx.7", ThisCustomer.SkinID, ThisCustomer.LocaleSetting));
                }
            }

            return results.ToString();
        }

        /// <summary>
        /// Used for backwards compatibility with AppLogic.GetColorAndSizePriceDelta
        /// </summary>
        /// <param name="ChosenColor"></param>
        /// <param name="ChosenSize"></param>
        /// <returns></returns>
        static public decimal GetColorAndSizePriceDelta(String ChosenColor, String ChosenSize)
        {
            decimal price = System.Decimal.Zero;
            String ColorPriceModifier = String.Empty;
            String SizePriceModifier = String.Empty;
            if (ChosenColor.IndexOf("[") != -1)
            {
                int i1 = ChosenColor.IndexOf("[");
                int i2 = ChosenColor.IndexOf("]");
                if (i1 != -1 && i2 != -1)
                {
                    ColorPriceModifier = ChosenColor.Substring(i1 + 1, i2 - i1 - 1);
                }
            }
            if (ChosenSize.IndexOf("[") != -1)
            {
                int i1 = ChosenSize.IndexOf("[");
                int i2 = ChosenSize.IndexOf("]");
                if (i1 != -1 && i2 != -1)
                {
                    SizePriceModifier = ChosenSize.Substring(i1 + 1, i2 - i1 - 1);
                }
            }

            if (ColorPriceModifier.Length != 0)
            {
                price += Localization.ParseUSDecimal(ColorPriceModifier);
            }
            if (SizePriceModifier.Length != 0)
            {
                price += Localization.ParseUSDecimal(SizePriceModifier);
            }
            return price;
        }

        /// <summary>
        /// Used for backwards compatibility with AppLogic.GetColorAndSizePriceDelta
        /// </summary>
        /// <param name="ChosenColor"></param>
        /// <param name="ChosenSize"></param>
        /// <param name="TaxClassID"></param>
        /// <param name="ThisCustomer"></param>
        /// <param name="WithDiscount"></param>
        /// <param name="WithVAT"></param>
        /// <returns></returns>
        static public decimal GetColorAndSizePriceDelta(String ChosenColor, String ChosenSize, int TaxClassID, Customer ThisCustomer, bool WithDiscount, bool WithVAT)
        {
            bool VATEnabled = AppLogic.AppConfigBool("VAT.Enabled");
            bool VATOn = (VATEnabled && ThisCustomer.VATSettingReconciled == VATSettingEnum.ShowPricesInclusiveOfVAT);

            decimal CustLevelDiscountPct = 1.0M;
            decimal price = System.Decimal.Zero;
            String ColorPriceModifier = String.Empty;
            String SizePriceModifier = String.Empty;
            if (ThisCustomer.CustomerLevelID > 0 && WithDiscount)
            {
                decimal LevelDiscountPercent = System.Decimal.Zero;
                using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
                {
                    dbconn.Open();
                    string sSql = string.Format("select LevelDiscountPercent from CustomerLevel with (NOLOCK) where CustomerLevelID={0}", ThisCustomer.CustomerLevelID);
                    using (IDataReader rs = DB.GetRS(sSql, dbconn))
                    {
                        if (rs.Read())
                        {
                            LevelDiscountPercent = DB.RSFieldDecimal(rs, "LevelDiscountPercent");
                        }
                    }
                }

                if (LevelDiscountPercent != System.Decimal.Zero)
                {
                    CustLevelDiscountPct -= LevelDiscountPercent / 100.0M;
                }
            }
            if (TaxClassID != AppLogic.AppConfigNativeInt("ShippingTaxClassId") && ChosenColor != null && ChosenSize != null)
            {
                if (ChosenColor.IndexOf("[") != -1)
                {
                    int i1 = ChosenColor.IndexOf("[");
                    int i2 = ChosenColor.IndexOf("]");
                    if (i1 != -1 && i2 != -1)
                    {
                        ColorPriceModifier = ChosenColor.Substring(i1 + 1, i2 - i1 - 1);
                    }
                }
                if (ChosenSize.IndexOf("[") != -1)
                {
                    int i1 = ChosenSize.IndexOf("[");
                    int i2 = ChosenSize.IndexOf("]");
                    if (i1 != -1 && i2 != -1)
                    {
                        SizePriceModifier = ChosenSize.Substring(i1 + 1, i2 - i1 - 1);
                    }
                }

                if (ColorPriceModifier.Length != 0)
                {
                    //Modifier is 1.23 -- never 1,23 -- force en-US. Comma format handled elsewhere
                    price += Localization.ParseLocaleDecimal(ColorPriceModifier, "en-US");
                }
                if (SizePriceModifier.Length != 0)
                {
                    //Modifier is 1.23 -- never 1,23 -- force en-US. Comma format handled elsewhere
                    price += Localization.ParseLocaleDecimal(SizePriceModifier, "en-US");
                }
            }
            if (VATOn && WithVAT)
            {
                decimal TaxRate = 0.0M;

                TaxRate = Prices.TaxRate(ThisCustomer, TaxClassID);

                Decimal TaxMultiplier = (1.0M + (TaxRate / 100.00M));
                price = TaxMultiplier * price;
            }
            return price * CustLevelDiscountPct;
        }

        #endregion
    }
}
