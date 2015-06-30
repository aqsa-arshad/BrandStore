// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Avalara.AvaTax.Adapter;
using Avalara.AvaTax.Adapter.TaxService;
using Avalara.AvaTax.Adapter.AddressService;
using System.Text;

namespace AspDotNetStorefrontCore
{
    public class Line : Avalara.AvaTax.Adapter.TaxService.Line
    {
        private string description;
        public new string Description
        {
            get
            {
                return description.Length > 255 ? description.Substring(0, 255) : description;
            }
            set
            {
                description = value;
            }
        }
    }

	public class AvaTax
	{
		#region AppConfig Keys

		const string EnabledAppConfigKey = "AvalaraTax.Enabled";
		const string AccountAppConfigKey = "AvalaraTax.Account";
		const string LicenseAppConfigKey = "AvalaraTax.License";
		const string ServiceUrlAppConfigKey = "AvalaraTax.ServiceUrl";
		const string CompanyCodeAppConfigKey = "AvalaraTax.CompanyCode";
		const string TaxRefundsAppConfigKey = "AvalaraTax.TaxRefunds";
		const string CommitTaxesConfigKey = "AvalaraTax.CommitTaxes";
		const string CommitRefundsConfigKey = "AvalaraTax.CommitRefunds";

		#endregion

		#region Predefined SKU's
		
		const string ShippingItemSku = "Avalara Tax Shipping Item";
		const string DiscountItemSku = "Avalara Tax Discount Item";
		const string OrderOptionItemSku = "Avalara Tax Order Option ";

		#endregion

		public bool Enabled { get; protected set; }
		public string Account { get; protected set; }
		public string License { get; protected set; }
		public string ServiceURL { get; protected set; }
		public string CompanyCode { get; protected set; }
		public bool TaxRefunds { get; protected set; }
		public bool CommitTaxes { get; protected set; }
		public bool CommitRefunds { get; protected set; }

		public AvaTax()
		{
			ValidateAppConfigs();
			LoadConfiguration();
		}

		#region Configuration

		protected void ValidateAppConfigs()
		{
			if(String.IsNullOrEmpty(AppLogic.AppConfig(EnabledAppConfigKey)))
				throw new Exception(String.Format("Please ensure that you have configured a value for the \"{0}\" AppConfig.", EnabledAppConfigKey));

			if(String.IsNullOrEmpty(AppLogic.AppConfig(AccountAppConfigKey)))
				throw new Exception(String.Format("Please ensure that you have configured a value for the \"{0}\" AppConfig.", AccountAppConfigKey));

			if(String.IsNullOrEmpty(AppLogic.AppConfig(LicenseAppConfigKey)))
				throw new Exception(String.Format("Please ensure that you have configured a value for the \"{0}\" AppConfig.", LicenseAppConfigKey));

			if(String.IsNullOrEmpty(AppLogic.AppConfig(ServiceUrlAppConfigKey)))
				throw new Exception(String.Format("Please ensure that you have configured a value for the \"{0}\" AppConfig.", ServiceUrlAppConfigKey));

			if(String.IsNullOrEmpty(AppLogic.AppConfig(CompanyCodeAppConfigKey)))
				throw new Exception(String.Format("Please ensure that you have configured a value for the \"{0}\" AppConfig.", CompanyCodeAppConfigKey));

			if(String.IsNullOrEmpty(AppLogic.AppConfig(TaxRefundsAppConfigKey)))
				throw new Exception(String.Format("Please ensure that you have configured a value for the \"{0}\" AppConfig.", TaxRefundsAppConfigKey));

			if(String.IsNullOrEmpty(AppLogic.AppConfig(CommitTaxesConfigKey)))
				throw new Exception(String.Format("Please ensure that you have configured a value for the \"{0}\" AppConfig.", CommitTaxesConfigKey));

			if(String.IsNullOrEmpty(AppLogic.AppConfig(CommitRefundsConfigKey)))
				throw new Exception(String.Format("Please ensure that you have configured a value for the \"{0}\" AppConfig.", CommitRefundsConfigKey));
		}

		protected void LoadConfiguration()
		{
			Enabled = AppLogic.AppConfigBool(EnabledAppConfigKey);
			Account = AppLogic.AppConfig(AccountAppConfigKey);
			License = AppLogic.AppConfig(LicenseAppConfigKey);
			ServiceURL = AppLogic.AppConfig(ServiceUrlAppConfigKey);
			CompanyCode = AppLogic.AppConfig(CompanyCodeAppConfigKey);
			TaxRefunds = AppLogic.AppConfigBool(TaxRefundsAppConfigKey);
			CommitTaxes = AppLogic.AppConfigBool(CommitTaxesConfigKey);
			CommitRefunds = AppLogic.AppConfigBool(CommitRefundsConfigKey);
		}

		#endregion

		protected TaxSvc CreateTaxService()
		{
			var assemblyVersion = System.Reflection.Assembly.GetAssembly(this.GetType()).GetName().Version;

			var taxService = new TaxSvc();
			taxService.Configuration.Security.Account = Account;
			taxService.Configuration.Security.License = License;
			taxService.Configuration.Url = ServiceURL;
			taxService.Profile.Client = String.Format("AspDotNetStorefront,{0}.{1},{2}.{3},AspDotNetStorefront Avalara Tax Addin,{0}.{1},{2}.{3}", assemblyVersion.Major, assemblyVersion.MajorRevision, assemblyVersion.Minor, assemblyVersion.MinorRevision);
			return taxService;
		}

		public bool TestAddin(out string reason)
		{
			try
			{
				if(!Enabled)
					throw new InvalidOperationException("AvalaraInactiveException");

				string expected = "Testing";

				TaxSvc taxService = CreateTaxService();
				var result = taxService.Ping(expected);

				if(result.ResultCode == SeverityLevel.Success)
				{
					reason = null;
					return true;
				}

				reason = "<ul>";

				foreach(Message message in result.Messages)
					reason = String.Format("{0}<li>{1}: {2}</li>", reason, message.Name, message.Details);

				reason += "</ul>";

				return false;
			}
			catch(Exception exception)
			{
				reason = exception.Message;
				return false;
			}
		}
        public string ValidateAddress(Customer customer)
        {
            var fakeAddress = new Address();
            return ValidateAddress(customer, null, out fakeAddress);
        }
		public string ValidateAddress(Customer customer, Address inputAddress, out Address ResultAddress)
		{
            //If coming from address.aspx do not include Avalara-specific string resources
            bool stringResourceInResult = inputAddress == null ? true : false;
            ResultAddress = new Address();
            if (!Enabled)
				throw new InvalidOperationException("AvalaraInactiveException");

			if (!customer.HasAtLeastOneAddress())
				return AppLogic.GetString("Avalara.AddressValidate.AtLeastOneAddress", customer.LocaleSetting);

			var addrSvc =  new AddressSvc();
			addrSvc.Configuration.Url = ServiceURL;
			addrSvc.Configuration.Security.Account = Account;
			addrSvc.Configuration.Security.License = License;

            //set address to validate and ensure ResultAddress returns non-blank address
			var addrReq = new ValidateRequest();
            if (inputAddress == null)
            {
                addrReq.Address = LoadAvalaraAddress(customer.PrimaryShippingAddressID);
                ResultAddress.LoadFromDB(customer.PrimaryShippingAddressID);
            }
            else
            {
                addrReq.Address = LoadAvalaraAddress(inputAddress.AddressID);
                ResultAddress = inputAddress;
            }

			try
			{
                //http://developer.avalara.com/api-docs/api-reference/address-validation
                ValidateResult addressResult = addrSvc.Validate(addrReq);
				if (addressResult.Messages.Count > 0)
				{
					var errorMessages = new StringBuilder();
					foreach (Message message in addressResult.Messages)
					{
                        if (message.Severity == SeverityLevel.Error)
                        {
                            if (stringResourceInResult)
                                errorMessages.Append(String.Format(AppLogic.GetString("Avalara.AddressValidate.ValidationFailed", customer.LocaleSetting), message.Details));
                            errorMessages.Append(message.Details);
                        }
					}
					return errorMessages.ToString();
				}
				else
				{
                    if (addressResult.ResultCode == SeverityLevel.Success && addressResult.Addresses.Count == 1)
                    {
                        ValidAddress normalizedAddress = addressResult.Addresses[0];
                        ResultAddress.Address1 = normalizedAddress.Line1;
                        ResultAddress.Address2 = normalizedAddress.Line2;
                        ResultAddress.City = normalizedAddress.City;
                        ResultAddress.State = normalizedAddress.Region;
                        ResultAddress.Zip = normalizedAddress.PostalCode;
                        ResultAddress.Country = AppLogic.GetCountryNameFromTwoLetterISOCode(normalizedAddress.Country);
                    }
					return String.Empty;
				}
			}
			catch (Exception ex)
			{
				return String.Format(AppLogic.GetString("Avalara.AddressValidate.Exception", customer.LocaleSetting), ex.Message);
			}
		}

		public decimal GetTaxRate(Customer customer, CartItemCollection cartItems, IEnumerable<OrderOption> orderOptions)
		{
			if(!Enabled)
				throw new InvalidOperationException("AvalaraInactiveException");

			if(!customer.HasAtLeastOneAddress() || !cartItems.Any())
				return 0m;

			// Return the cached value if it's present and still valid
			string lastCartHash = HttpContext.Current.Items["Avalara.CartHash"] as string;
			decimal? lastTaxAmount = HttpContext.Current.Items["Avalara.TaxAmount"] as decimal?;
			string currentCartHash = GetCartHash(cartItems, customer, orderOptions);

			if(lastTaxAmount != null && currentCartHash == lastCartHash)
				return lastTaxAmount.Value;

			// Create line items for all cart items and shipping selections
			var cartItemAddressGroups = GroupCartItemsByShippingAddress(cartItems, customer.PrimaryShippingAddressID);
			var lineItems = CreateItemAndShippingLineItems(cartItemAddressGroups, (shipmentAddressId, shipmentAddress, shipmentCartItems) => CreateCartShippingLineItem(shipmentAddress, customer, shipmentCartItems, orderOptions));

			// Create line items for order options using the first shipping address as the destination
			var firstShippingAddress = LoadAvalaraAddress(cartItemAddressGroups.First().Key);
			lineItems = lineItems.Concat(CreateOrderOptionLineItems(orderOptions, firstShippingAddress));

			decimal discountAmount = -cartItems.DiscountResults.Sum(dr => dr.OrderTotal);	// This value is returned as negative, but Avalara expectes a positive

			// Build and submit the tax request
			GetTaxRequest taxRequest = BuildTaxRequest(customer, GetOriginAddress(), DocumentType.SalesOrder);
			taxRequest.Discount = discountAmount;

			// Add each line to the request, setting the line number sequentially
			int lineItemIndex = 1;
			foreach(var line in lineItems)
			{
				line.No = (lineItemIndex++).ToString();
				taxRequest.Lines.Add(line);
			}

			TaxSvc taxService = CreateTaxService();
			GetTaxResult taxResult = taxService.GetTax(taxRequest);
			foreach(Message message in taxResult.Messages)
				LogErrorMessage(message);

			decimal taxAmount = taxResult.TotalTax;

			// Cache the tax amount
			HttpContext.Current.Items["Avalara.CartHash"] = currentCartHash;
			HttpContext.Current.Items["Avalara.TaxAmount"] = taxAmount;

			return taxAmount;
		}

		public void OrderPlaced(Order order)
		{
			if(!Enabled)
				throw new InvalidOperationException("AvalaraInactiveException");

			var customer = new Customer(order.CustomerID);
			var cartItems = order.CartItems;
			var orderOptions = GetOrderOptions(order);

			// Create line items for all cart items and shipping selections
			var cartItemAddressGroups = GroupCartItemsByShippingAddress(cartItems, customer.PrimaryShippingAddressID);
			var lineItems = CreateItemAndShippingLineItems(cartItemAddressGroups, (shipmentAddressId, shipmentAddress, shipmentCartItems) => CreateOrderShippingLineItem(shipmentAddress, shipmentCartItems, order, shipmentAddressId));

			// Create line items for order options using the first shipping address as the destination
			var firstShippingAddress = LoadAvalaraAddress(cartItemAddressGroups.First().Key);
			lineItems = lineItems.Concat(CreateOrderOptionLineItems(orderOptions, firstShippingAddress));

			// Calculate the discount from the promotion usages for this order
			decimal discountAmount;
			using(var promotionsDataContext = new AspDotNetStorefront.Promotions.Data.EntityContextDataContext())
			{
				discountAmount = promotionsDataContext.PromotionUsages
					.Where(pu => pu.OrderId == order.OrderNumber)
					.Where(pu => pu.Complete)
					.Sum(pu => -pu.OrderDiscountAmount)	// Avalara expects a positive number
					.GetValueOrDefault(0);
			}

			// Build and submit the tax request
			GetTaxRequest taxRequest = BuildTaxRequest(customer, GetOriginAddress(), DocumentType.SalesInvoice);
			taxRequest.Discount = discountAmount;
			taxRequest.DocCode = order.OrderNumber.ToString();
			taxRequest.TaxOverride.TaxDate = order.OrderDate;

			// Add each line to the request, setting the line number sequentially
			int lineItemIndex = 0;
			foreach(var line in lineItems)
			{
				line.No = (++lineItemIndex).ToString();
				taxRequest.Lines.Add(line);
			}

			TaxSvc taxService = CreateTaxService();
			GetTaxResult taxResult = taxService.GetTax(taxRequest);
			foreach(Message message in taxResult.Messages)
				LogErrorMessage(message); //this throws an exception

			//not used currently
			//decimal taxAmount = taxResult.TotalTax;
		}

		public void CommitTax(Order order)
		{
			if(!Enabled)
				throw new InvalidOperationException("AvalaraInactiveException");

			// Avalara rounds each line item to two decimal places before calculating 
			// the order total in GetTax(). The order total derived here may end up
			// with a different value, as we will only round the total. If the value
			// is different, it will generate a warning below, which we will ignore.
			decimal totalAmount = Decimal.Round(order.SubTotal(true) + order.ShippingTotal(true), 2, MidpointRounding.AwayFromZero);

			PostTaxRequest postTaxRequest = new PostTaxRequest
			{
				CompanyCode = CompanyCode,
				DocType = DocumentType.SalesInvoice,
				DocCode = order.OrderNumber.ToString(),
				DocDate = DateTime.Now,
				TotalAmount = totalAmount,
				TotalTax = order.TaxTotal(true),
				Commit = CommitTaxes,
			};

			TaxSvc taxService = CreateTaxService();
			PostTaxResult postTaxResult = taxService.PostTax(postTaxRequest);
			foreach(Message message in postTaxResult.Messages)
			{
				// Ignore warnings at this stage only
				if(message.Severity == SeverityLevel.Warning)
					continue;

				LogErrorMessage(message);
			}
		}

		public void VoidTax(Order order)
		{
			if(!Enabled)
				throw new InvalidOperationException("AvalaraInactiveException");

			CancelTaxRequest cancelTaxRequest = new CancelTaxRequest
			{
				CancelCode = CancelCode.DocVoided,
				CompanyCode = CompanyCode,
				DocType = DocumentType.SalesInvoice,
				DocCode = order.OrderNumber.ToString(),
			};

			TaxSvc taxService = CreateTaxService();
			CancelTaxResult cancelTaxResult = taxService.CancelTax(cancelTaxRequest);
			foreach(Message message in cancelTaxResult.Messages)
				LogErrorMessage(message);
		}

		public void IssueRefund(Order order, Address originAddress, decimal refundTotal)
		{
			if(!Enabled)
				throw new InvalidOperationException("AvalaraInactiveException");

			var customer = new Customer(order.CustomerID);
			var cartItems = order.CartItems;
			var orderOptions = GetOrderOptions(order);
			int numberOfRefundTransactions = (order.ChildOrderNumbers ?? String.Empty).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Length;

			decimal discountAmount = -cartItems.DiscountResults.Sum(dr => dr.OrderTotal);	// This value is returned as negative, but Avalara expectes a positive

			// Build and submit the tax request
			GetTaxRequest taxRequest = BuildTaxRequest(customer, GetOriginAddress(), DocumentType.SalesInvoice);
			taxRequest.Discount = discountAmount;
			taxRequest.DocCode = order.OrderNumber.ToString() + "." + (numberOfRefundTransactions + 1).ToString();
			taxRequest.TaxOverride.TaxOverrideType = TaxOverrideType.TaxDate;
			taxRequest.TaxOverride.TaxDate = order.OrderDate;
			taxRequest.TaxOverride.Reason = "Refund";
			taxRequest.DocDate = DateTime.Now;
			taxRequest.Commit = CommitRefunds;

			if(refundTotal > 0)
			{
				// Partial refund
				// Add in the refund line object
				Line refundLineItem = new Line
				{
					Amount = -refundTotal,
					Qty = 1,
					No = "1",
					TaxCode = TaxRefunds ? null : "NT" // Make this refund non-taxable as recommended by Avalara
				};

				taxRequest.Lines.Add(refundLineItem);
			}
			else
			{
				// Refund the full order
				// Void any partial refunds
				VoidRefunds(order);

				// Add order items to the refund transaction
				// Create line items for all cart items and shipping selections
				var cartItemAddressGroups = GroupCartItemsByShippingAddress(cartItems, customer.PrimaryShippingAddressID);
				var lineItems = CreateItemAndShippingLineItems(cartItemAddressGroups, (shipmentAddressId, shipmentAddress, shipmentCartItems) => CreateOrderShippingLineItem(shipmentAddress, shipmentCartItems, order, shipmentAddressId));

				// Create line items for order options using the first shipping address as the destination
				var firstShippingAddress = LoadAvalaraAddress(cartItemAddressGroups.First().Key);
				lineItems = lineItems.Concat(CreateOrderOptionLineItems(orderOptions, firstShippingAddress));

				// Add each line to the request, setting the line number sequentially
				int lineItemIndex = 0;
				foreach(var line in lineItems)
				{
					line.Amount = -line.Amount;
					line.No = (++lineItemIndex).ToString();
					taxRequest.Lines.Add(line);
				}
			}

			TaxSvc taxService = CreateTaxService();
			GetTaxResult taxResult = taxService.GetTax(taxRequest);
			foreach(Message message in taxResult.Messages)
				LogErrorMessage(message);
		}

		private void VoidRefunds(Order order)
		{
			TaxSvc taxService = CreateTaxService();
			int numberOfRefundTransactions = (order.ChildOrderNumbers ?? String.Empty).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Length;

			for(int i = 1; i <= numberOfRefundTransactions; i++)
			{
				CancelTaxRequest cancelTaxRequest = new CancelTaxRequest
				{
					CancelCode = CancelCode.DocVoided,
					CompanyCode = CompanyCode,
					DocCode = order.OrderNumber.ToString() + "." + i.ToString(),
					DocType = DocumentType.ReturnInvoice,
				};

				var cancelTaxResult = taxService.CancelTax(cancelTaxRequest);
				foreach(Message message in cancelTaxResult.Messages)
					LogErrorMessage(message);
			}
		}

		#region Line Item Creation
		
		private Line CreateLineItem(CartItem cartItem, Avalara.AvaTax.Adapter.AddressService.Address destinationAddress)
		{
			decimal extendedPrice;

			if(cartItem.ThisShoppingCart == null)
			{
				// Order line items
				using(var promotionsDataContext = new AspDotNetStorefront.Promotions.Data.EntityContextDataContext())
				{
					// Sum the discount for every PromotionLineItem that applies to the current cart item.
					// A gift product's line item price is already discounted, so don't include the discount when IsAGift is true.
					var lineItemDiscountAmount = promotionsDataContext.PromotionLineItems
						.Where(pli => !pli.isAGift)
						.Where(pli => pli.shoppingCartRecordId == cartItem.ShoppingCartRecordID)
						.Sum(pli => (decimal?)pli.discountAmount);

					extendedPrice = cartItem.Price + (lineItemDiscountAmount ?? 0);
				}
			}
			else
			{
				// Shopping cart items
				CartItemCollection cartItems = cartItem.ThisShoppingCart.CartItems;
				extendedPrice = Prices.LineItemPrice(cartItem, cartItems.CouponList, cartItems.QuantityDiscountList, cartItem.ThisCustomer);
			}

			Line lineItem = new Line
			{
				ItemCode = cartItem.SKU,
				Description = cartItem.ProductName,
				Amount = extendedPrice,
				Qty = (double)cartItem.Quantity,
				Discounted = true,
				DestinationAddress = destinationAddress,
			};

			if(cartItem.IsTaxable)
			{
				var lineItemTaxClass = new TaxClass(cartItem.TaxClassID);
				lineItem.TaxCode = lineItemTaxClass.TaxCode;
			}
			else
				lineItem.TaxCode = "NT";

			lineItem.TaxOverride.TaxDate = System.DateTime.Today;

			return lineItem;
		}

		private IEnumerable<Line> CreateCartShippingLineItem(Avalara.AvaTax.Adapter.AddressService.Address destinationAddress, Customer customer, IEnumerable<CartItem> cartItems, IEnumerable<OrderOption> orderOptions)
		{
			var lineItemTaxClass = new TaxClass(AppLogic.AppConfigUSInt("ShippingTaxClassID"));

			Line lineItem = new Line
			{
				ItemCode = ShippingItemSku,
				Description = String.Empty,
				Amount = Prices.ShippingTotal(true, true, new CartItemCollection(cartItems), customer, orderOptions),
				Qty = 1,
				Discounted = false,
				DestinationAddress = destinationAddress,
				TaxCode = lineItemTaxClass.TaxCode,
			};

			lineItem.TaxOverride.TaxDate = System.DateTime.Today;

			yield return lineItem;
		}

		private IEnumerable<Line> CreateOrderShippingLineItem(Avalara.AvaTax.Adapter.AddressService.Address destinationAddress, IEnumerable<CartItem> cartItems, Order order, int adnsfShippingAddressId)
		{
			var lineItemTaxClass = new TaxClass(AppLogic.AppConfigUSInt("ShippingTaxClassID"));

			var shippingAmount = new OrderShipmentCollection(order.OrderNumber)
				.Where(os => os.AddressID == adnsfShippingAddressId)
				.Select(os => os.ShippingTotal)
				.FirstOrDefault();

			Line lineItem = new Line
			{
				ItemCode = ShippingItemSku,
				Description = String.Empty,
				Amount = shippingAmount,
				Qty = 1,
				Discounted = false,
				DestinationAddress = destinationAddress,
				TaxCode = lineItemTaxClass.TaxCode,
			};

			lineItem.TaxOverride.TaxDate = System.DateTime.Today;

			yield return lineItem;
		}

		private IEnumerable<Line> CreateOrderOptionLineItems(IEnumerable<OrderOption> orderOptions, Avalara.AvaTax.Adapter.AddressService.Address destinationAddress)
		{
			return orderOptions.Select(oo => new
				{
					OrderOption = oo,
					TaxClass = new TaxClass(oo.TaxClassID),
				})
				.Select(o => new Line
				{
					ItemCode = OrderOptionItemSku + o.OrderOption.Name.Replace(" ", "_"),
					Description = o.OrderOption.Name,
					Amount = o.OrderOption.Cost,
					Qty = 1,
					Discounted = true,
					DestinationAddress = destinationAddress,
				}
			);
		}

		private IEnumerable<Line> CreateItemAndShippingLineItems(IEnumerable<IGrouping<int, CartItem>> cartItemAddressGroups, Func<int, Avalara.AvaTax.Adapter.AddressService.Address, IGrouping<int, CartItem>, IEnumerable<Line>> createShippingLines)
		{
			return cartItemAddressGroups
				.Select(cig => new
				{
					DestinationAddress = LoadAvalaraAddress(cig.Key),
					CartItemGroup = cig,
				})
				.SelectMany(o => o.CartItemGroup
					.Select(ci => CreateLineItem(ci, o.DestinationAddress))
					.Concat(createShippingLines(o.CartItemGroup.Key, o.DestinationAddress, o.CartItemGroup))
				);
		}

		#endregion

		#region Addresses

		private Avalara.AvaTax.Adapter.AddressService.Address LoadAvalaraAddress(int adnsfAddressId)
		{
			var adnsfAddress = new Address();
			adnsfAddress.LoadFromDB(adnsfAddressId);

			return BuildAvalaraAddress(adnsfAddress);
		}
	
		private Avalara.AvaTax.Adapter.AddressService.Address BuildAvalaraAddress(Address sourceAddress)
		{
			return new Avalara.AvaTax.Adapter.AddressService.Address
			{
				Line1 = sourceAddress.Address1,
				Line2 = sourceAddress.Address2,
				Line3 = sourceAddress.Suite,
				City = sourceAddress.City,
				Region = sourceAddress.State,
				PostalCode = sourceAddress.Zip,
				Country = sourceAddress.Country,
			};
		}

		private Avalara.AvaTax.Adapter.AddressService.Address BuildAvalaraAddress(AddressInfo sourceAddress)
		{
			return new Avalara.AvaTax.Adapter.AddressService.Address
			{
				Line1 = sourceAddress.m_Address1,
				Line2 = sourceAddress.m_Address2,
				Line3 = sourceAddress.m_Suite,
				City = sourceAddress.m_City,
				Region = sourceAddress.m_State,
				PostalCode = sourceAddress.m_Zip,
				Country = sourceAddress.m_Country,
			};
		}

		private Avalara.AvaTax.Adapter.AddressService.Address GetOriginAddress()
		{
			return new Avalara.AvaTax.Adapter.AddressService.Address
			{
				Line1 = AppLogic.AppConfig("RTShipping.OriginAddress"),
				Line2 = AppLogic.AppConfig("RTShipping.OriginAddress2"),
				City = AppLogic.AppConfig("RTShipping.OriginCity"),
				Region = AppLogic.AppConfig("RTShipping.OriginState"),
				PostalCode = AppLogic.AppConfig("RTShipping.OriginZip"),
				Country = AppLogic.AppConfig("RTShipping.OriginCountry"),
			};
		}

		#endregion

		#region Error Logging

		private void LogErrorMessage(Message m)
		{
			string errorMessage = "Severity:" + m.Severity + "";
			errorMessage += "Name:" + m.Name + "";
			errorMessage += "Details:" + m.Details + "";
			errorMessage += "Summary:" + m.Summary + "";

			LogErrorMessage(errorMessage);
		}

		private void LogErrorMessage(string errorMessage)
		{
			throw new Exception(errorMessage);
		}

		#endregion

		private GetTaxRequest BuildTaxRequest(Customer cust, Avalara.AvaTax.Adapter.AddressService.Address originAddress, DocumentType docType)
		{
			// Instantiate a new GetTaxRequest 
			GetTaxRequest avalaraTaxRequest = new GetTaxRequest
			{
				OriginAddress = originAddress,
				CompanyCode = CompanyCode,
				CustomerCode = cust.CustomerID.ToString(),
				DetailLevel = DetailLevel.Tax,
				CurrencyCode = Localization.StoreCurrency(),
				DocCode = ("DocDate" + System.DateTime.Now.ToString()),
				DocDate = System.DateTime.Today,
				DocType = docType,
				Discount = System.Decimal.Zero,
			};

			avalaraTaxRequest.TaxOverride.TaxDate = System.DateTime.Today;

			if(cust.LevelHasNoTax)
				avalaraTaxRequest.ExemptionNo = (cust.CustomerLevelName.Length > 25 ? cust.CustomerLevelName.Substring(0, 25) : cust.CustomerLevelName)
                    ?? "Customer Level Tax Exempt"; //Avalara can't handle more than 25 chars here

			return avalaraTaxRequest;
		}

		private IEnumerable<IGrouping<int, CartItem>> GroupCartItemsByShippingAddress(CartItemCollection cartItems, int defaultShippingAddressId)
		{
			return cartItems
				.Select(ci => new
				{
					ShippingAddressId = ci.ShippingAddressID > 0 ? ci.ShippingAddressID : defaultShippingAddressId,
					CartItems = ci,
				})
				.GroupBy(o => o.ShippingAddressId, o => o.CartItems);
		}

		private IEnumerable<OrderOption> GetOrderOptions(Order order)
		{
			var orderOptions = (order.OrderOptions ?? String.Empty)
				.Split('^')
				.Where(s => !String.IsNullOrEmpty(s))
				.Select(s => s.Split('|'))
				.Select(sa => new OrderOption
				{
					ID = Convert.ToInt32(sa[0]),
					Name = sa[2],
					UniqueID = new Guid(sa[1]),
					TaxRate = Convert.ToDecimal(CommonLogic.IIF(sa[4].IndexOf("(") == -1, sa[4], sa[4].Substring(0, sa[4].IndexOf("("))).Replace("$", "")),
					Cost = Convert.ToDecimal(CommonLogic.IIF(sa[3].IndexOf("(") == -1, sa[3], sa[3].Substring(0, sa[3].IndexOf("("))).Replace("$", "")),
					ImageUrl = sa[5],
					TaxClassID = sa.Length > 6 ? Convert.ToInt32(sa[6]) : 0,
				});

			return orderOptions;
		}

		private string GetCartHash(CartItemCollection cartItems, Customer customer, IEnumerable<OrderOption> orderOptions)
		{
			var originAddress = GetOriginAddress();

			string cartComposite = cartItems
				.Select(c => new
				{
					CartItem = c,
					Address = new Address(c.ShippingAddressID)
				})
				.Aggregate(
					String.Format("{0}_{1}_{2}_{3}_{4}_{5}_{6}_{7}_{8}_{9}_{10}",
						originAddress.City,
						originAddress.Region,
						originAddress.PostalCode,
						customer.PrimaryShippingAddress.City,
						customer.PrimaryShippingAddress.State,
						customer.PrimaryShippingAddress.Zip, 
						customer.LevelHasNoTax,
						cartItems.DiscountResults.Sum(dr => dr.OrderTotal), 
						cartItems.DiscountResults.Sum(dr => dr.ShippingTotal), 
						cartItems.DiscountResults.Sum(dr => dr.LineItemTotal),
						orderOptions.Aggregate("_", (s, oo) => String.Format("{0}_{1}_{2}_{3}", s, oo.ID, oo.TaxClassID, oo.Cost))),
					(s, o) => String.Format("{0}_{1}_{2}_{3}_{4}_{5}_{6}_{7}_{8}",
						s,
						o.CartItem.ShoppingCartRecordID,
						o.CartItem.VariantID,
						o.CartItem.Quantity,
						o.CartItem.Price,
						o.CartItem.IsTaxable,
						o.CartItem.TaxClassID,
						o.CartItem.ShippingAddressID,
						o.CartItem.ShippingMethodID)
				);

			using(var md5 = System.Security.Cryptography.MD5CryptoServiceProvider.Create())
			{
				var hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(cartComposite));
				return hash.Aggregate(String.Empty, (s, b) => String.Format("{0}{1:x2}", s, b));
			}
		}
	}
}
