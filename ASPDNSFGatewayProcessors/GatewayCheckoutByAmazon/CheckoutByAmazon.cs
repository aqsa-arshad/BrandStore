// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
#region Using Directives

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Xml.Linq;
using Amazon.CheckoutByAmazonService;
using AspDotNetStorefrontCore;
using MarketplaceWebService.Model;
#endregion

namespace GatewayCheckoutByAmazon
{
	/// <summary>
	///  The CheckoutByAmazon class is a manager class for the CBA service.
	/// </summary>
	/// <remarks>
	///  Exceptions will be recorded to the System Log in Admin.
	/// </remarks>
	/// <see cref="https://payments.amazon.com/sdui/sdui/business/cba"/>
	public class CheckoutByAmazon
	{
		/// <summary>
		///  Creates a new instance of the CheckoutByAmazon class.
		/// </summary>
		/// <remarks>Initialization will occur during instantiation including default configuration.</remarks>
		public CheckoutByAmazon ()
		{
			Initialize();
		}

		#region Fields

		/// <summary>
		///  CBA uses a purchase contract to identify customers and their current shopping. The purchase contract is held in a session
		///   and this os the key for it.
		/// </summary>
		private const String CBA_Session_Key = "CBAPCID";

		/// <summary>
		///  This is the constant String identifier for the CBA payment method.
		/// </summary>
		public const String CBA_Gateway_Identifier = "CheckoutByAmazon";

		#endregion

		#region Properties

		/// <summary>
		///  Gets or sets the CBA Access Key.
		/// </summary>
		/// <remarks>
		///  This is set in your Seller Central account.
		/// </remarks>
		private String CbaAccessKeyId { get; set; }

		/// <summary>
		///  Gets or sets the CBA Secret Key.
		/// </summary>
		/// <remarks>
		///  This is set in your Seller Central account.
		/// </remarks>
		private String CbaSecretKey { get; set; }

		/// <summary>
		///  Gets or sets the Marketplace Web Service Access Key.
		/// </summary>
		/// <remarks>
		///  This is set in your Seller Central account.
		/// </remarks>
		private String MwsAccessKeyId { get; set; }

		/// <summary>
		///  Gets or sets the Marketplace Web Service Secret Key.
		/// </summary>
		/// <remarks>
		///  This is set in your Seller Central account.
		/// </remarks>
		private String MwsSecretKey { get; set; }

		/// <summary>
		///  Gets or sets the CBA Merchant identifier.
		/// </summary>
		/// <remarks>
		///  This is set in your Seller Central account.
		/// </remarks>
		private String MerchantId { get; set; }

		/// <summary>
		///  Gets or sets the Marketplace Web Service Marketplace identifier.
		/// </summary>
		/// <remarks>
		///  This is set in your Seller Central account.
		/// </remarks>
		private String Marketplace { get; set; }

		/// <summary>
		///  Gets or sets the url used to render the widget scripts.
		/// </summary>
		private String WidgetUrl { get; set; }

		/// <summary>
		///  Gets or sets the sandbox url used to render the widget scripts.
		/// </summary>
		private String WidgetSandboxUrl {get;set;}

		/// <summary>
		///  Gets or sets the url used to call the cba service.
		/// </summary>
		private String CBAServiceUrl { get; set; }

		/// <summary>
		///  Gets or sets the sandbox url used to call the cba service.
		/// </summary>
		private String CBAServiceSandboxUrl { get; set; }

		/// <summary>
		///  Gets or sets the url used to call the merchant service.
		/// </summary>
		private String MerchantServiceUrl { get; set; }

		/// <summary>
		///  Gets or sets the sandbox url used to call the merchant scripts.
		/// </summary>
		private String MerchantServiceSandboxUrl { get; set; }

		/// <summary>
		///  Gets whether or not CBA has been enabled. This logic is determined during initialization.
		/// </summary>
		public Boolean IsEnabled { get; private set; }

		/// <summary>
		///  Gets whether or not CBA is in test mode.
		/// </summary>
		public Boolean UseSandbox { get; private set; }

		/// <summary>
		///  Gets the Fullfillment Type CBA should use when receiving Ready To Ship notifications from Amazon.
		/// </summary>
		public OrderFulfillmentType OrderFulfillmentType { get; private set; }

		/// <summary>
		///  Gets whether or not the current customer is checking out with CBA.
		/// </summary>
		public Boolean IsCheckingOut
		{
			get { return HttpContext.Current.Session[CBA_Session_Key] != null; }
		}

		/// <summary>
		///  Gets the current purchase contract id.
		/// </summary>
		/// <returns></returns>
		public String CurrentPurchaseContractId
		{
			get
			{
				if (HttpContext.Current.Session[CBA_Session_Key] == null)
					return String.Empty;

				return HttpContext.Current.Session[CBA_Session_Key].ToString();
			}
		}

		#endregion

		#region Private Methods

		/// <summary>
		///  Reads the store appconfigs for CBA, creates and defaults them if necessary, and initializes the class to use.
		/// </summary>
		private void Initialize ()
		{
			try
			{
				CbaAccessKeyId = AppLogic.AppConfig("CheckoutByAmazon.CbaAccessKey");
                CbaSecretKey = AppLogic.AppConfig("CheckoutByAmazon.CbaSecretKey");
                MwsAccessKeyId = AppLogic.AppConfig("CheckoutByAmazon.MwsAccessKey");
                MwsSecretKey = AppLogic.AppConfig("CheckoutByAmazon.MwsSecretKey");
                MerchantId = AppLogic.AppConfig("CheckoutByAmazon.MerchantId");
                Marketplace = AppLogic.AppConfig("CheckoutByAmazon.Marketplace");

                WidgetUrl = AppLogic.AppConfig("CheckoutByAmazon.WidgetUrl");
                WidgetSandboxUrl = AppLogic.AppConfig("CheckoutByAmazon.WidgetSandboxUrl");
                CBAServiceUrl = AppLogic.AppConfig("CheckoutByAmazon.CBAServiceUrl");
                CBAServiceSandboxUrl = AppLogic.AppConfig("CheckoutByAmazon.CBAServiceSandboxUrl");
                MerchantServiceUrl = AppLogic.AppConfig("CheckoutByAmazon.MerchantServiceUrl");
                MerchantServiceSandboxUrl = AppLogic.AppConfig("CheckoutByAmazon.MerchantServiceSandboxUrl");

                UseSandbox = AppLogic.AppConfigBool("CheckoutByAmazon.UseSandbox");
                OrderFulfillmentType = (OrderFulfillmentType)Enum.Parse(typeof(OrderFulfillmentType), AppLogic.AppConfig("CheckoutByAmazon.OrderFulfillmentType"));

                IsEnabled = !String.IsNullOrEmpty(CbaAccessKeyId) && !String.IsNullOrEmpty(CbaSecretKey) && !String.IsNullOrEmpty(MerchantId) && AppLogic.AppConfig("PaymentMethods").Contains("CheckoutByAmazon");
            }
			catch (Exception exception)
			{
				SysLog.LogMessage("Checkout By Amazon: Exception on initialize.", exception.Message, MessageTypeEnum.Unknown, MessageSeverityEnum.Error);
			}
		}

		/// <summary>
		///  Creates a new InstantOrderNotification class based off of values in the current request.
		/// </summary>
		/// <returns>Returns a new instance of the InstantOrderNotification class with all fields populated with values from the current request.</returns>
		private InstantOrderNotification BuildNotification ()
		{
			InstantOrderNotification notification = new InstantOrderNotification();

			notification.UUID = HttpContext.Current.Request.Params["UUID"];
			notification.Timestamp = HttpContext.Current.Request.Params["Timestamp"];
			notification.Signature = HttpContext.Current.Request.Params["Signature"];
			notification.NotificationType = HttpContext.Current.Request.Params["NotificationType"];
			notification.Xml = XElement.Parse(HttpContext.Current.Request.Params["NotificationData"]);
			notification.Xns = notification.Xml.Name.Namespace.NamespaceName;

			return notification;
		}

		/// <summary>
		///  Validates the notification sent from CBA.
		/// </summary>
		/// <param name="secretKey">The secret key on the current CBA seller account.</param>
		/// <param name="notification">The notification that needs to be validated.</param>
		/// <returns>True if the notification is from Amazon and untampered.</returns>
		private Boolean ValidateNotification (String secretKey, InstantOrderNotification notification)
		{
			DateTime timeStamp = DateTime.Now.AddMinutes(60);
			if (!DateTime.TryParse(notification.Timestamp, out timeStamp))
				return false;

			if (timeStamp.AddMinutes(15) < DateTime.Now)
				return false;

			return notification.Signature == GetHash(secretKey, notification.UUID + notification.Timestamp);
		}

		/// <summary>
		///  Creates a hash of the specified value to be used for validation.
		/// </summary>
		/// <param name="cbaSecretKey">The secret key for the current CBA seller account.</param>
		/// <param name="value">The value to be hashed.</param>
		/// <returns>A base 64 encoded has of the specified value.</returns>
		/// <remarks>Using HMACSHA1 and UTF8.</remarks>
		private String GetHash (String cbaSecretKey, String value)
		{
			HMACSHA1 hmacsha1 = new HMACSHA1(ASCIIEncoding.UTF8.GetBytes(cbaSecretKey));
			Byte[] hashBytes = hmacsha1.ComputeHash(ASCIIEncoding.UTF8.GetBytes(value));
			String retVal = System.Convert.ToBase64String(hashBytes);

			return retVal;
		}

		/// <summary>
		///  Processes the specified notification as a new order.
		/// </summary>
		/// <param name="notification">The notification to be processed.</param>
		private void ProcessNewOrderNotification (InstantOrderNotification notification)
		{
			String amazonOrderId = GetXmlValue(notification.Xml.Element(notification.Xns + "ProcessedOrder").Element(notification.Xns + "AmazonOrderID"));
            String name = GetXmlValue(notification.Xml.Element(notification.Xns + "ProcessedOrder").Element(notification.Xns + "ShippingAddress").Element(notification.Xns + "Name"));
			
            String firstName = (name.Length > 0 && name.Split(' ')[0] != null) ? name.Split(' ')[0] : string.Empty;
            String lastName = (name.Length > 0 && name.Split(' ')[1] != null) ? name.Split(' ')[1] : string.Empty;

            String address1 = GetXmlValue(notification.Xml.Element(notification.Xns + "ProcessedOrder").Element(notification.Xns + "ShippingAddress").Element(notification.Xns + "AddressFieldOne"));
			String address2 = GetXmlValue(notification.Xml.Element(notification.Xns + "ProcessedOrder").Element(notification.Xns + "ShippingAddress").Element(notification.Xns + "AddressFieldTwo"));
			String city = GetXmlValue(notification.Xml.Element(notification.Xns + "ProcessedOrder").Element(notification.Xns + "ShippingAddress").Element(notification.Xns + "City"));        
            String state = GetXmlValue(notification.Xml.Element(notification.Xns + "ProcessedOrder").Element(notification.Xns + "ShippingAddress").Element(notification.Xns + "State"));                                 
            String postalCode = GetXmlValue(notification.Xml.Element(notification.Xns + "ProcessedOrder").Element(notification.Xns + "ShippingAddress").Element(notification.Xns + "PostalCode"));
			String countryCode = GetXmlValue(notification.Xml.Element(notification.Xns + "ProcessedOrder").Element(notification.Xns + "ShippingAddress").Element(notification.Xns + "CountryCode"));

            // Convert CBA country to full country name
            String fullCountryName = AppLogic.GetCountryNameFromTwoLetterISOCode(countryCode);

            if (String.IsNullOrEmpty(fullCountryName))
                fullCountryName = AppLogic.GetCountryNameFromThreeLetterISOCode(countryCode);

            if (String.IsNullOrEmpty(fullCountryName))
                fullCountryName = countryCode;

            // CBA can pass down full state names (e.g. Washington instead of WA). Ensure we have an abbreviation.
            string stateAbbreviation = string.Empty;

            if (state != null)
                stateAbbreviation = (state.Length > 2) ? AppLogic.GetStateAbbreviation(state, fullCountryName) : state;

            if (string.IsNullOrEmpty(state) || (state.Length > 2 && fullCountryName.ToLower() != "united states"))
                stateAbbreviation = "--";

			Int32 orderNumber = DB.GetSqlN(String.Format("select cast(OrderNumber as int) as N from Orders where AuthorizationPNREF like '%{0}%'", amazonOrderId));
            Order order = new Order(orderNumber);
            Customer customer = new Customer(order.CustomerID);
            if (orderNumber > 0)
			{

				if (!order.HasMultipleShippingAddresses())
				{
                    String updateOrderSql = String.Format("update Orders set ShippingFirstName = '{7}', ShippingLastName = '{8}', ShippingAddress1 = '{1}', ShippingAddress2='{2}', ShippingCity='{3}', ShippingState='{4}', ShippingZip='{5}', ShippingCountry='{6}' where OrderNumber = {0}", orderNumber, address1, address2, city, stateAbbreviation, postalCode, fullCountryName, firstName, lastName);
                    DB.ExecuteSQL(updateOrderSql);

                    if (String.IsNullOrEmpty(customer.FirstName))
                    {
                        List<SqlParameter> sqlParams = new List<SqlParameter>();
                        sqlParams.Add(DB.CreateSQLParameter("@FirstName", SqlDbType.VarChar, 100, firstName, ParameterDirection.Input));
                        sqlParams.Add(DB.CreateSQLParameter("@LastName", SqlDbType.VarChar, 100, lastName, ParameterDirection.Input));
                        sqlParams.Add(DB.CreateSQLParameter("@CustomerID", SqlDbType.Int, 4, customer.CustomerID, ParameterDirection.Input));

                        using (SqlConnection dbconn = DB.dbConn())
                        {
                            dbconn.Open();
                            using (SqlCommand cmd = new SqlCommand("update Customer set FirstName = @FirstName, LastName = @LastName where CustomerID = @CustomerId;", dbconn))
                            {
                                cmd.Parameters.AddRange(sqlParams.ToArray());
                                cmd.ExecuteNonQuery();
                                cmd.Parameters.Clear();
                            }
                        }
                    }

                    Address shippingAddress = new Address();

                    if (customer.PrimaryShippingAddressID != 0)
                    {
                        shippingAddress.LoadFromDB(customer.PrimaryShippingAddressID);
                        shippingAddress.FirstName = firstName;
                        shippingAddress.LastName = lastName;
                        shippingAddress.Address1 = address1;
                        shippingAddress.Address2 = address2;
                        shippingAddress.City = city;
                        shippingAddress.State = stateAbbreviation;
                        shippingAddress.Country = fullCountryName;
                        shippingAddress.PaymentMethodLastUsed = CBA_Gateway_Identifier;
                        shippingAddress.UpdateDB();
                    }
                    else
                    {
                        shippingAddress.FirstName = firstName;
                        shippingAddress.LastName = lastName;
                        shippingAddress.Address1 = address1;
                        shippingAddress.Address2 = address2;
                        shippingAddress.City = city;
                        shippingAddress.State = stateAbbreviation;
                        shippingAddress.Country = fullCountryName;
                        shippingAddress.PaymentMethodLastUsed = CBA_Gateway_Identifier;
                        shippingAddress.InsertDB(order.CustomerID);
                        
                    }
                    shippingAddress.MakeCustomersPrimaryAddress(AddressTypes.Shipping);

                    if (customer.PrimaryBillingAddressID == 0)
                    {
                        shippingAddress.MakeCustomersPrimaryAddress(AddressTypes.Billing);

                        List<SqlParameter> sqlParams = new List<SqlParameter>();
                        sqlParams.Add(DB.CreateSQLParameter("@CustomerID", SqlDbType.Int, 4, customer.CustomerID, ParameterDirection.Input));

                        using (SqlConnection dbconn = DB.dbConn())
                        {
                            dbconn.Open();
                            using (SqlCommand cmd = new SqlCommand("update Customer set BillingEqualsShipping = 1 where CustomerID = @CustomerId;", dbconn))
                            {
                                cmd.Parameters.AddRange(sqlParams.ToArray());
                                cmd.ExecuteNonQuery();
                                cmd.Parameters.Clear();
                            }
                        }		
                    }
				}

                AppLogic.SendOrderEMail(customer, orderNumber, false, "CheckoutByAmazon", true, AppLogic.MakeEntityHelpers(), null);
			}
		}

		/// <summary>
		///  Processes the specified notification as an order ready to be shipped.
		/// </summary>
		/// <param name="notification">The notification to be processed.</param>
		private void ProcessOrderReadyToShipNotification (InstantOrderNotification notification)
		{
			String amazonOrderId = GetXmlValue(notification.Xml.Element(notification.Xns + "ProcessedOrder").Element(notification.Xns + "AmazonOrderID"));
			Int32 orderNumber = DB.GetSqlN(String.Format("select cast(OrderNumber as int) as N from Orders where AuthorizationPNREF like '%{0}%'", amazonOrderId));
			if (orderNumber > 0)
			{
				String updateOrderSql = String.Format("update Orders set TransactionState='{0}', CapturedOn=getdate(), ReadyToShip=1, CaptureTxResult='{1}' where OrderNumber={2}", AppLogic.ro_TXStateCaptured, notification.Xml, orderNumber);
				DB.ExecuteSQL(updateOrderSql);

				if (this.OrderFulfillmentType == GatewayCheckoutByAmazon.OrderFulfillmentType.Instant)
				{
					String errorMessage;
					MarkOrderShipped(orderNumber, out errorMessage);
				}
			};
		}

		/// <summary>
		///  Processes the specified notification as a cancelled order.
		/// </summary>
		/// <param name="notification">The notification to be processed.</param>
		private void ProcessOrderCancelledNotification (InstantOrderNotification notification)
		{
			String amazonOrderId = GetXmlValue(notification.Xml.Element(notification.Xns + "ProcessedOrder").Element(notification.Xns + "AmazonOrderID"));
			Int32 orderNumber = DB.GetSqlN(String.Format("select cast(OrderNumber as int) as N from Orders where AuthorizationPNREF like '%{0}%'", amazonOrderId));

			String updateOrderSql = String.Format("update Orders set TransactionState = '{0}' where OrderNumber = {1}", AppLogic.ro_TXStateRefused, orderNumber);
			DB.ExecuteSQL(updateOrderSql);
		}

		/// <summary>
		///  This sets all of the line items in the cart on the current purchase contract.
		/// </summary>
		/// <param name="service">The CBA web service proxy.</param>
		/// <param name="merchantId"></param>
		/// <param name="purchaseContractId"></param>
		/// <param name="cart"></param>
		/// <param name="cartItems"></param>
		/// <param name="customerLevelAllowsQuantityDiscount"></param>
		/// <param name="calculatedSubTotal"></param>
		/// <param name="response"></param>
		/// <param name="exception"></param>
		/// <returns></returns>
		private Boolean SetPurchaseItems (CheckoutByAmazonService service, String merchantId, String purchaseContractId, ShoppingCart cart, CartItem[] cartItems, Boolean customerLevelAllowsQuantityDiscount, out Decimal calculatedSubTotal, out  MessageSetPurchaseItemsResponse response, out Exception exception)
		{
			response = new MessageSetPurchaseItemsResponse();

			exception = null;
			calculatedSubTotal = 0;

			try
			{
				IList<PurchaseItem> purchaseItems = new List<PurchaseItem>();

				foreach (CartItem cartItem in cartItems)
				{
					Decimal productPrice = CalculateOrderedProductPrice(cart, cartItem, customerLevelAllowsQuantityDiscount);
					if (cart.VatEnabled)
					{
						if (cart.VatIsInclusive)
							productPrice = Math.Round(cartItem.ExtPrice / cartItem.Quantity, 2, MidpointRounding.AwayFromZero);
						else
							productPrice = Math.Round((cartItem.RegularPrice + cartItem.VatRate) / 2, 2, MidpointRounding.AwayFromZero);
					}

                    calculatedSubTotal += Math.Round(productPrice, 2) * cartItem.Quantity;

					var purchaseItem = new PurchaseItem
					{
						Category = String.Empty,
						Description = cartItem.ProductName,
						ItemCustomData = String.Empty,
						MerchantId = merchantId,
						MerchantItemId = cartItem.ShoppingCartRecordID.ToString(),
						PhysicalProductAttributes = new PhysicalProductAttributes
						{
							DeliveryMethod = new DeliveryMethod
							{
								DisplayableShippingLabel = cartItem.ShippingMethod,
							},

							ItemCharges = new Charges
							{
								GiftWrap = new Price
								{
									CurrencyCode = Localization.StoreCurrency(false),
									Amount = 0.00
								},
								Shipping = new Price
								{
									CurrencyCode = Localization.StoreCurrency(false),
									Amount = 0.00
								},
								Tax = new Price
								{
									CurrencyCode = Localization.StoreCurrency(false),
									Amount = 0.00
								},
							},
						},
						ProductType = ProductType.PHYSICAL,
						ProductTypeSpecified = true,
						Quantity = cartItem.Quantity.ToString(),
						SKU = cartItem.SKU,
						Title = cartItem.ProductName,
						UnitPrice = new Price
						{
							CurrencyCode = Localization.StoreCurrency(false),
							Amount = (Double) Math.Round(productPrice, 2),
						},
						URL = String.Empty
					};

					purchaseItems.Add(purchaseItem);
				}

				foreach (OrderOption orderOption in cart.OrderOptions.OrderBy(oo => oo.DisplayOrder))
				{
					var purchaseItem = new PurchaseItem
					{
						Category = String.Empty,
						Description = orderOption.Description,
						ItemCustomData = String.Empty,
						MerchantId = merchantId,
						MerchantItemId = orderOption.ID.ToString(),
						PhysicalProductAttributes = new PhysicalProductAttributes
						{
							DeliveryMethod = new DeliveryMethod
							{
								DisplayableShippingLabel = cartItems.First().ShippingMethod,
							},
							ItemCharges = new Charges
							{
								Shipping = new Price
								{
									CurrencyCode = Localization.StoreCurrency(false),
									Amount = 0.00
								},
								Tax = new Price
								{
									CurrencyCode = Localization.StoreCurrency(false),
									Amount = 0.00
								},
							},
						},
						ProductType = ProductType.PHYSICAL,
						ProductTypeSpecified = true,
						Quantity = 1.ToString(),
						SKU = orderOption.UniqueID.ToString(),
						Title = orderOption.Name,
						UnitPrice = new Price
						{
							CurrencyCode = Localization.StoreCurrency(false),
							Amount = (Double) Math.Round(orderOption.Cost, 2),
						},
						URL = String.Empty
					};

					purchaseItems.Add(purchaseItem);
				}

				MessageSetPurchaseItems messageSetPurchaseItems = new MessageSetPurchaseItems
				{
					PurchaseContractId = purchaseContractId,
					PurchaseItems = purchaseItems.ToArray(),
				};

				response = service.SetPurchaseItems(messageSetPurchaseItems);

				return true;
			}
			catch (Exception e)
			{
				exception = e;
				return false;
			}
		}

		private Boolean SetContractCharges (CheckoutByAmazonService service, String purchaseContractId, Customer customer, Decimal orderSubTotal, Decimal shippingTotal, Boolean vatEnabled, Boolean vatIsExclusive, Decimal taxTotal, Decimal orderTotal, out  MessageSetContractChargesResponse response, out Exception exception)
		{
			response = new MessageSetContractChargesResponse();
			exception = null;

			try
			{
				
				MessageSetContractCharges messageSetContractCharges = new MessageSetContractCharges
				{
					PurchaseContractId = purchaseContractId,
					Charges = new Charges
					{
						GiftWrap = new Price
						{
							CurrencyCode = Localization.StoreCurrency(false),
							Amount = 0
						},
						Shipping = new Price
						{
							CurrencyCode = Localization.StoreCurrency(false),
							Amount = (Double) Math.Round(shippingTotal, 2, MidpointRounding.AwayFromZero),
						},
						Tax = new Price
						{
							CurrencyCode = Localization.StoreCurrency(false),
                            Amount = (Double)Math.Round(taxTotal, 2, MidpointRounding.AwayFromZero),
						},
					},
				};

				if (vatEnabled)
				{
					if (vatIsExclusive)
						messageSetContractCharges.Charges.Shipping.Amount += Math.Round((Double)Prices.GetVATPrice((Decimal)messageSetContractCharges.Charges.Shipping.Amount, 1, customer, AppLogic.AppConfigNativeInt("Admin_DefaultTaxClassID")), 2, MidpointRounding.AwayFromZero);

					messageSetContractCharges.Charges.Tax = new Price
					{
						CurrencyCode = Localization.StoreCurrency(false),
						Amount = 0,
					};
				}

                Decimal discountTotal = orderSubTotal + shippingTotal + taxTotal - orderTotal;

				if (discountTotal > 0)
				{
					messageSetContractCharges.Charges.Promotions = new Promotion[]
					{
						new Promotion
						{
							Description = String.Empty,
							Discount = new Price
							{
								CurrencyCode = Localization.StoreCurrency(false),
								Amount = (Double)Math.Round(discountTotal, 2, MidpointRounding.AwayFromZero),
							},
							PromotionId = "Discount",
						}
					};
				}

				response = service.SetContractCharges(messageSetContractCharges);

				return true;
			}
			catch (Exception e)
			{
				exception = e;
				return false;
			}
		}

		private Boolean CompletePurchaseContract (CheckoutByAmazonService service, String purchaseContractId, out  MessageCompletePurchaseContractResponse response, out Exception exception)
		{
			response = new MessageCompletePurchaseContractResponse();
			exception = null;

			try
			{
				response = service.CompletePurchaseContract(new MessageCompletePurchaseContract
				{
					PurchaseContractId = purchaseContractId,
					IntegratorId = String.Empty,
					IntegratorName = String.Empty,
					InstantOrderProcessingNotificationURLs = new InstantOrderProcessingNotificationURLs
					{
						IntegratorURL = String.Empty,
						MerchantURL = String.Empty,
					},
				});

				return true;
			}
			catch (Exception e)
			{
				exception = e;
				return false;
			}
		}

		private Decimal CalculateOrderedProductPrice (ShoppingCart cart, CartItem cartItem, Boolean customerLevelAllowsQuantityDiscount)
		{
			if (cartItem.CustomerEntersPrice || AppLogic.IsAKit(cartItem.ProductID) || cartItem.IsUpsell)
				return cartItem.Price;

			Decimal retVal = 0.0M;
			Boolean isOnSale = false;

			retVal = AppLogic.DetermineLevelPrice(cartItem.VariantID, cart.ThisCustomer.CustomerLevelID, out isOnSale);

			Decimal DIDPercent = 0.0M;
			QuantityDiscount.QuantityDiscountType fixedPriceDID = QuantityDiscount.QuantityDiscountType.Percentage;
			if (customerLevelAllowsQuantityDiscount)
			{
				DIDPercent = QuantityDiscount.GetQuantityDiscountTablePercentageWithoutCartAwareness(cartItem.ProductID, cartItem.Quantity, out fixedPriceDID);
				if (DIDPercent != 0.0M)
				{
					if (fixedPriceDID.Equals(QuantityDiscount.QuantityDiscountType.FixedAmount))
						retVal = (cartItem.Price - DIDPercent);
					else
						retVal = (1.0M - (DIDPercent / 100.0M)) * retVal;
				}
			}

			decimal regular_pr = System.Decimal.Zero;
			decimal sale_pr = System.Decimal.Zero;
			decimal extended_pr = System.Decimal.Zero;
			if (cart.CartType != CartTypeEnum.RecurringCart)
			{
				regular_pr = AppLogic.GetVariantPrice(cartItem.VariantID);
				sale_pr = AppLogic.GetVariantSalePrice(cartItem.VariantID);
				extended_pr = AppLogic.GetVariantExtendedPrice(cartItem.VariantID, cart.ThisCustomer.CustomerLevelID);

				Decimal PrMod = AppLogic.GetColorAndSizePriceDelta(cartItem.ChosenColor, cartItem.ChosenSize, cartItem.TaxClassID, cart.ThisCustomer, true, true);
				if (PrMod != System.Decimal.Zero)
					retVal += Decimal.Round(PrMod * (1.0M - (DIDPercent / 100.0M)), 2, MidpointRounding.AwayFromZero);

				if (retVal < System.Decimal.Zero)
					retVal = System.Decimal.Zero;
			}
			else
			{
				regular_pr = cartItem.Price;
				sale_pr = System.Decimal.Zero;
				extended_pr = System.Decimal.Zero;
			}

			return retVal;
		}

		/// <summary>
		///  Helper method to retrieve values from an xml element that may be null.
		/// </summary>
		/// <param name="xml">XElement whose value needs to be retrieved.</param>
		/// <returns>The value of the element unless it is null, then String.Empty.</returns>
		private String GetXmlValue (XElement xml)
		{
			return xml != null ? xml.Value.Trim() : String.Empty;
		}

		#endregion

		#region Public Methods

		public String RenderJSLibrary ()
		{
			String url = this.UseSandbox ? this.WidgetSandboxUrl : this.WidgetUrl;
			return @"<script language=""javascript"" src=""" + url + @"""></script>";
		}

		public String RenderCheckoutButton (String containerId, Guid customerGuid, Boolean express)
		{
			if (!IsEnabled)
				return String.Empty;

			String url = RenderJSLibrary();

			return @"<div id=""" + containerId + @"""></div>" + RenderJSLibrary() + @"
						<script type=""text/javascript"">
							new CBA.Widgets." + (express ? "Express" : "Inline") + @"CheckoutWidget(
							{
								merchantId: """ + this.MerchantId + @""",
								onAuthorize: function (widget) {
									window.location = 'CBACallback.aspx?Action=checkout&CGUID=" + customerGuid.ToString() + @"&" + CBA_Session_Key + @"=' + widget.getPurchaseContractId();
								}
							}).render(""" + containerId + @""");
						</script>";
		}

		public String RenderAddressWidget (String containerId, Boolean readOnly, String postBackScript, Guid customerGuid)
		{
			return RenderAddressWidget(containerId, readOnly, postBackScript, customerGuid, 300, 200);
		}

		public String RenderAddressWidget (String containerId, Boolean readOnly, String postBackScript, Guid customerGuid, Int32 width, Int32 height)
		{
			if (!IsEnabled || !IsCheckingOut)
				return String.Empty;

			Boolean hasAddressSelected = GetDefaultShippingAddress() != null;

			String url = this.UseSandbox ? this.WidgetSandboxUrl : this.WidgetUrl;
			
			return @"<div id=""" + containerId + @"""></div>
						<script language=""javascript"" src=""" + url + @"""></script>
						<script type=""text/javascript"">
							cbaAddressWidget = new CBA.Widgets.AddressWidget(
							{
								merchantId: """ + this.MerchantId + @""",
								" + (readOnly ? "displayMode:'Read'," : "") + @"
								design: { size: { width: '" + width.ToString() + @"', height: '" + height.ToString() + @"' } },
								" + (readOnly ? String.Empty : @"onAddressSelect: function (widget){ 
									widget.adnsf_Ticks++;
								if (widget.adnsf_Ticks > " + (hasAddressSelected ? "1" : "0") + @")
								{
									widget.adnsf_Ticks = 0;
									jQuery.ajax({url: 'CBACallback.aspx?Action=checkout&CGUID=" + customerGuid.ToString() + @"&" + CBA_Session_Key + @"=' + widget.getPurchaseContractId()});			
									" + (!String.IsNullOrEmpty(postBackScript) ? postBackScript : String.Empty) + @"
								}
								}") + @"
							});
							cbaAddressWidget.adnsf_Ticks = 0;
							cbaAddressWidget.render(""" + containerId + @""");
						</script>";
		}

		public String RenderAddressWidgetWithRedirect (String containerId, String redirectUrl, Guid customerGuid, Int32 width, Int32 height)
		{
			if (!IsEnabled || !IsCheckingOut)
				return String.Empty;

			String url = this.UseSandbox ? this.WidgetSandboxUrl : this.WidgetUrl;
			
			Boolean hasAddressSelected = GetDefaultShippingAddress() != null;

			return @"<div id=""" + containerId + @"""></div>
						<script language=""javascript"" src=""" + url + @"""></script>
						<script type=""text/javascript"">
							cbaAddressWidget = new CBA.Widgets.AddressWidget(
							{
								merchantId: """ + this.MerchantId + @""",
								design: { size: { width: '" + width.ToString() + @"', height: '" + height.ToString() + @"' } },
								onAddressSelect: function (widget){ 
									widget.adnsf_Ticks++;
									if (widget.adnsf_Ticks > " + (hasAddressSelected ? "1" : "0") + @")
									{
                                        var btnContinueCheckout = document.getElementById('ctl00_PageContent_btnContinueCheckout');
                                        if (btnContinueCheckout != null) { btnContinueCheckout.disabled = true; }
										widget.adnsf_Ticks = 0;
										window.location = 'CBACallback.aspx?Action=checkout&rurl=" + redirectUrl + @"&CGUID=" + customerGuid.ToString() + @"&" + CBA_Session_Key + @"=' + widget.getPurchaseContractId();
									}
								}
							});
							cbaAddressWidget.adnsf_Ticks = 0;
							cbaAddressWidget.render(""" + containerId + @""");
						</script>";
		}

		public String RenderWalletWidget (String containerId, Boolean readOnly)
		{
			return RenderWalletWidget(containerId, readOnly, 300, 200);
		}

		public String RenderWalletWidget (String containerId, Boolean readOnly, Int32 width, Int32 height)
		{
			if (!IsEnabled || !IsCheckingOut)
				return String.Empty;

			String url = this.UseSandbox ? this.WidgetSandboxUrl : this.WidgetUrl;

			return @"<div id=""" + containerId + @"""></div>
						<script language=""javascript"" src=""" + url + @"""></script>
						<script type=""text/javascript"">
							cbaWalletWidget = new CBA.Widgets.WalletWidget(
							{
								merchantId: """ + this.MerchantId + @""",
								" + (readOnly ? "displayMode:'Read'," : "") + @"
								design: { size: { width: '" + width.ToString() + @"', height: '" + height.ToString() + @"' } }
							});
							cbaWalletWidget.render(""" + containerId + @""");
						</script>";
		}

		public String RenderOrderDetailWidget (Int32 orderNumber)
		{
			if (!IsEnabled || !IsCheckingOut)
				return String.Empty;

			StringBuilder retVal = new StringBuilder();

			String url = this.UseSandbox ? this.WidgetSandboxUrl : this.WidgetUrl;
			retVal.Append(@"<script language=""javascript"" src=""" + url + @"""></script>");

			Order order = new Order(orderNumber);
			String[] amazonOrderIds = order.AuthorizationPNREF.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			Int32 orderIdx = 0;
			
			foreach (String amazonOrderId in amazonOrderIds)
			{
				retVal.Append(@"<div class=""checkoutbyamazon-widget-wrap""" + "gw.checkoutbyamazon.display.1".StringResource() + amazonOrderId + @"<div id=""AmazonExpressOrderDetailsWidget" + orderIdx.ToString() + @""" class=""checkoutbyamazon-widget""></div></div>
						<script type=""text/javascript"">
							new CBA.Widgets.OrderDetailsWidget(
							{
								merchantId: """ + this.MerchantId + @""",
								orderID: '" + amazonOrderId + @"'
							}).render(""AmazonExpressOrderDetailsWidget" + orderIdx.ToString() + @""");
						</script>");
				orderIdx++;
			}

			return retVal.ToString();
		}

		/// <summary>
		///  Handles dispatching calls on any CBA callbacks.
		/// </summary>
		/// <remarks>
		///  This method is usually invoked from the CBACallback.aspx page. Action is a defined command by the checkout widget on the shopping cart page. 
		///   The NotificationData param is specified by CBA.
		/// </remarks>
		public void HandleCallback ()
		{
			try
			{
				if (HttpContext.Current.Request.Params["Action"] != null)
				{
					Guid customerGuid = Guid.Empty;

					switch (HttpContext.Current.Request.Params["Action"].Trim().ToLower())
					{
						case "checkout":
							customerGuid = Guid.NewGuid();
							try
							{
								customerGuid = new Guid(HttpContext.Current.Request.Params["CGUID"]);
								BeginCheckout(customerGuid, true, true);
								return;
							}
							catch
							{
								return;
							}
					}
				}

				if (HttpContext.Current.Request.Params["NotificationData"] != null)
				{
					InstantOrderNotification notification = BuildNotification();
					if (!ValidateNotification(this.CbaSecretKey, notification))
						return;

					switch (notification.NotificationType)
					{
						case "NewOrderNotification":
							ProcessNewOrderNotification(notification);
							break;
						case "OrderReadyToShipNotification":
							ProcessOrderReadyToShipNotification(notification);
							break;
						case "OrderCancelledNotification":
							ProcessOrderCancelledNotification(notification);
							break;
					}
				}
			}
			catch (Exception exception)
			{
				if (exception.Message == "Thread was being aborted.") // Patch for terminated requests throwing exceptions.
					return;

				SysLog.LogMessage("Checkout By Amazon: Exception on callback.", exception.Message, MessageTypeEnum.Unknown, MessageSeverityEnum.Error);
			}
		}

		/// <summary>
		///  Initializes the checkout for the current customer.
		/// </summary>
		/// <param name="customerId">Id of the current customer to begin checking out as CBA.</param>
		public void BeginCheckout (Guid customerGuid, Boolean initSession, Boolean redirect)
		{
			if (initSession)
			{
				// If there is no purchase contract in the request, don't checkout. An error has occurred.
				if (HttpContext.Current.Request.Params[CBA_Session_Key] == null)
					return;

				// Save the purchase contract identifier in the session.
				HttpContext.Current.Session[CBA_Session_Key] = HttpContext.Current.Request.Params[CBA_Session_Key];
			}

			Customer customer = new Customer(customerGuid);
				
			Int32 defaultAddressId = 0;

			ShippingAddress amazonShippingAddress = GetDefaultShippingAddress();
			if (amazonShippingAddress != null)
			{
				// Handle defaulting addresses if we're checking out anonymously.
				defaultAddressId = SyncCBAAddress(customer.CustomerID, amazonShippingAddress);
			}
				
			// If the customer doesn't have a billing address, go ahead and create one using the CBA shipping address so at least
			//  we have something.
			if (customer.PrimaryBillingAddress.AddressID == 0)
			{
				customer.PrimaryBillingAddressID = defaultAddressId;
				DB.ExecuteSQL(String.Format("update Customer set BillingAddressID = {0} where CustomerID = {1}", defaultAddressId, customer.CustomerID));
			}

			// Always assign the selected CBA shipping address as the primary shipping address.
			customer.PrimaryShippingAddressID = defaultAddressId;
			DB.ExecuteSQL(String.Format("update Customer set ShippingAddressID = {0}, BillingEqualsShipping = 0 where CustomerID = {1}", defaultAddressId, customer.CustomerID));
			DB.ExecuteSQL(String.Format("update ShoppingCart set ShippingAddressID = {0}, ShippingMethodId = 0, ShippingMethod = '' where CustomerID = {1}", defaultAddressId, customer.CustomerID));

			// Update primary billing address PaymentMethodLastUsed to CBA
			DB.ExecuteSQL(String.Format("update Address set PaymentMethodLastUsed={0} where CustomerID={1} AND AddressID={2}",
				DB.SQuote(CBA_Gateway_Identifier), customer.CustomerID, defaultAddressId));

			if (!redirect)
				return;

			// Validate the shopping cart before moving forward in the process.
			ShoppingCart shoppingCart = new ShoppingCart(customer.SkinID, customer, CartTypeEnum.ShoppingCart, 0, false, false);

			Boolean validated = true;
			if (!shoppingCart.MeetsMinimumOrderAmount(AppLogic.AppConfigUSDecimal("CartMinOrderAmount")))
				validated = false;

			if (!shoppingCart.MeetsMinimumOrderQuantity(AppLogic.AppConfigUSInt("MinCartItemsBeforeCheckout")))
				validated = false;

            if (shoppingCart.ExceedsMaximumOrderQuantity(AppLogic.AppConfigUSInt("MaxCartItemsBeforeCheckout")))
                validated = false;
			
			if (validated)
			{
				AppLogic.eventHandler("BeginCheckout").CallEvent("&BeginCheckout=true");

				String redirectUrl = HttpContext.Current.Request.Params["rurl"];
				if (!String.IsNullOrEmpty(redirectUrl))
					HttpContext.Current.Response.Redirect(redirectUrl);
				else
				{
					var checkoutController = new CBACheckoutPageController(customer, shoppingCart);
					HttpContext.Current.Response.Redirect(checkoutController.GetContinueCheckoutPage());
				}
			}
			else
				HttpContext.Current.Response.Redirect("shoppingcart.aspx");
		}

		/// <summary>
		///  Terminates the current CBA checkout and cleans up any leftover cba data, so the user can checkout normally.
		/// </summary>
		/// <param name="customerId">Id of the customer to reset the checkout for.</param>
		public void ResetCheckout (Int32 customerId)
		{
			if (!IsCheckingOut)
				return;

			HttpContext.Current.Session[CBA_Session_Key] = null;
			DB.ExecuteSQL(String.Format("delete from Address where CustomerId = {0} and Address1 = 'Hidden By Amazon'", customerId));
			DB.ExecuteSQL(String.Format("update Customer set BillingAddressId = 0 where CustomerId = {0} and BillingAddressId not in (select AddressId from Address where CustomerId = {0})", customerId));
			DB.ExecuteSQL(String.Format("update Customer set ShippingAddressId = 0 where CustomerId = {0} and ShippingAddressId not in (select AddressId from Address where CustomerId = {0})", customerId));
            DB.ExecuteSQL(String.Format("update Address set PaymentMethodLastUsed = '' where CustomerId = {0} and PaymentMethodLastUsed = 'CheckoutByAmazon'", customerId));

			DB.ExecuteSQL(String.Format("update ShoppingCart set ShippingAddressId = 0, ShippingMethodId = 0, ShippingMethod = '' where CustomerId = {0}", customerId));
			
			DB.ExecuteSQL(String.Format("update Customer set Email = '' where CustomerId = {0} and IsRegistered = 0", customerId));
		}

		/// <summary>
		///  Terminates the current CBA checkout and cleans up any leftover cba data, so the user can checkout normally.
		/// </summary>
		/// <param name="customerId">Id of the customer to reset the checkout for.</param>
		public void ResetShippingAddress (Int32 customerId)
		{
			DB.ExecuteSQL(String.Format("delete from Address where CustomerId = {0} and Address1 = 'Hidden By Amazon'", customerId));
			DB.ExecuteSQL(String.Format("update Customer set ShippingAddressId = 0 where CustomerId = {0}", customerId));
		}

		/// <summary>
		///  Gets the default shipping address on the current purchase contract.
		/// </summary>
		/// <returns>The Default CBA shipping address. Null if it doesn't exist yet.</returns>
		public ShippingAddress GetDefaultShippingAddress ()
		{
			try
			{
				CheckoutByAmazonService service = new CheckoutByAmazonServiceClient(CbaAccessKeyId, CbaSecretKey, new CheckoutByAmazonServiceConfig
				{
					SignatureVersion = "2",
					ServiceURL = this.UseSandbox ? this.CBAServiceSandboxUrl : this.CBAServiceUrl,
				});

				var purchaseContractResponse = service.GetPurchaseContract(new MessageGetPurchaseContract
				{
					PurchaseContractId = CurrentPurchaseContractId,
				});

                if (purchaseContractResponse == null || purchaseContractResponse.GetPurchaseContractResult.PurchaseContract == null || purchaseContractResponse.GetPurchaseContractResult.PurchaseContract.Destinations == null)
                    return null;

				var destination = purchaseContractResponse.GetPurchaseContractResult.PurchaseContract.Destinations.FirstOrDefault(d => d.DestinationName == "#default");
				if (destination == null)
					return null;

				if (destination.PhysicalDestinationAttributes == null)
					return null;

				return destination.PhysicalDestinationAttributes.ShippingAddress;
			}
			catch (Exception exception)
			{
                SysLog.LogMessage("Checkout By Amazon: Exception on checkout  (GetDefaultShippingAddress).", exception.Message, MessageTypeEnum.Unknown, MessageSeverityEnum.Error);
				return null;
			}
		}

		/// <summary>
		///  Retrieves the current purchase contract with CBA, reads all destination shipping addresses, syncronizes them with the current
		///   users account and updates the shopping cart records with the addresses.
		/// </summary>
		/// <param name="customerId">Id of the customer whose shopping cart needs to be updated.</param>
		/// <remarks>CBA destinations will be named after the shopping cart record id when using multi-ship.</remarks>
		public void UpdateShoppingCartAddresses (Int32 customerId)
		{
			try
			{
				CheckoutByAmazonService service = new CheckoutByAmazonServiceClient(CbaAccessKeyId, CbaSecretKey, new CheckoutByAmazonServiceConfig
				{
					SignatureVersion = "2",
					ServiceURL = this.UseSandbox ? this.CBAServiceSandboxUrl : this.CBAServiceUrl,
				});

				var purchaseContractResponse = service.GetPurchaseContract(new MessageGetPurchaseContract
				{
					PurchaseContractId = CurrentPurchaseContractId,
				});

				var destinations = purchaseContractResponse.GetPurchaseContractResult.PurchaseContract.Destinations;
				foreach (var destination in destinations)
				{
					if (destination == null)
						continue;

					if (destination.PhysicalDestinationAttributes == null)
						continue;

					Int32 shoppingCartItemId;
					if (!Int32.TryParse(destination.DestinationName, out shoppingCartItemId))
						continue;

					Int32 existingAddressId = SyncCBAAddress(customerId, destination.PhysicalDestinationAttributes.ShippingAddress);
					DB.ExecuteSQL(String.Format("update ShoppingCart set ShippingAddressId = {0} where ShoppingCartRecId = {1}", existingAddressId, shoppingCartItemId));
				}
			}
			catch (Exception exception)
			{
                SysLog.LogMessage("Checkout By Amazon: Exception on checkout (UpdateShoppingCartAddresses).", exception.Message, MessageTypeEnum.Unknown, MessageSeverityEnum.Error);
			}
		}

		/// <summary>
		///  Gets the matching store address for the specified cba address. If the store address does not exist, it will be created.
		/// </summary>
		/// <param name="customerId">Id of the customer the address is for.</param>
		/// <param name="cbaAddress">The cba address to find or create the related store address with.</param>
		/// <remarks>
		///		Because CBA will not return street information on an address until the order is placed, line one will read "Hidden By Amazon".
		/// </remarks>
		/// <returns></returns>
		public Int32 SyncCBAAddress (Int32 customerId, ShippingAddress cbaAddress)
		{
			// Amazon supports full zip codes but ADNSF doesn't, always only take the first 5 characters.
			String postalCode = cbaAddress.PostalCode;
			if (postalCode == null)
				postalCode = String.Empty;

			if (postalCode.Length > 5)
				postalCode = postalCode.Substring(0, 5);

			try
			{
                // Convert CBA country to full country name
                string fullCountryName = AppLogic.GetCountryNameFromTwoLetterISOCode(cbaAddress.CountryCode);

                if (String.IsNullOrEmpty(fullCountryName))
                    fullCountryName = AppLogic.GetCountryNameFromThreeLetterISOCode(cbaAddress.CountryCode);

                if (String.IsNullOrEmpty(fullCountryName))
                    fullCountryName = cbaAddress.CountryCode;

                // CBA can pass down full state names (e.g. Washington instead of WA). Ensure we have an abbreviation.
                string stateAbbreviation = string.Empty;

                if (cbaAddress.StateOrProvinceCode != null)
                    stateAbbreviation = (cbaAddress.StateOrProvinceCode.Length > 2) ? AppLogic.GetStateAbbreviation(cbaAddress.StateOrProvinceCode, fullCountryName) : cbaAddress.StateOrProvinceCode;

                if (string.IsNullOrEmpty(cbaAddress.StateOrProvinceCode) || (cbaAddress.StateOrProvinceCode.Length > 2 && fullCountryName.ToLower() != "united states"))
                    stateAbbreviation = "--";

				Int32 existingAddressId = DB.GetSqlN(String.Format("select AddressID as N from Address where CustomerId = {0} and (Address1 = 'Hidden By Amazon' or Address1 = '{1}') and City = '{3}' and State = '{4}' and Zip = '{5}' and Country = '{6}' and Deleted = 0",
					customerId,
					cbaAddress.AddressLineOne ?? String.Empty,
					cbaAddress.AddressLineTwo ?? String.Empty,
					cbaAddress.City ?? String.Empty,
					stateAbbreviation,
					postalCode,
                    fullCountryName));

				if (existingAddressId == 0)
				{
					Address shippingAddress = new Address(customerId);
					shippingAddress.InsertDB();

					// Populate the address object
					shippingAddress.Address1 = (cbaAddress.AddressLineOne ?? String.Empty) != String.Empty ? cbaAddress.AddressLineOne : "Hidden By Amazon";
					shippingAddress.Address2 = cbaAddress.AddressLineTwo ?? String.Empty;
					shippingAddress.City = cbaAddress.City ?? String.Empty;
					shippingAddress.State = stateAbbreviation ?? String.Empty;
					shippingAddress.Zip = postalCode ?? String.Empty;
					shippingAddress.Country = fullCountryName ?? String.Empty;

					shippingAddress.PaymentMethodLastUsed = CBA_Gateway_Identifier;
					shippingAddress.UpdateDB();
                    shippingAddress.MakeCustomersPrimaryAddress(AddressTypes.Shipping);
                    
                    existingAddressId = shippingAddress.AddressID;
				}
				else
					DB.ExecuteSQL(String.Format("update Address set PaymentMethodLastUsed = '{0}' where AddressId = {1}", CBA_Gateway_Identifier, existingAddressId));

				return existingAddressId;
			}
			catch (Exception exception)
			{
				SysLog.LogMessage("Checkout By Amazon: Exception on checkout (SyncCBAAddress).", exception.Message, MessageTypeEnum.Unknown, MessageSeverityEnum.Error);
				return 0;
			}
		}

		public String CaptureOrder (String purchaseContractId, ShoppingCart cart, CartItem[] cartItems, Boolean customerLevelAllowsQuantityDiscount, Decimal shippingTotal, Decimal taxTotal, Decimal orderTotal, out String[] amazonOrderIds, out Exception exception)
		{
			exception = null;
			amazonOrderIds = new String[0];

			try
			{
				CheckoutByAmazonService service = new CheckoutByAmazonServiceClient(CbaAccessKeyId, CbaSecretKey, new CheckoutByAmazonServiceConfig
				{
					SignatureVersion = "2",
					ServiceURL = this.UseSandbox ? this.CBAServiceSandboxUrl : this.CBAServiceUrl,
				});

				MessageSetPurchaseItemsResponse messageSetPurchaseItemsResponse;
				MessageSetContractChargesResponse messageSetContractChargesResponse;
				MessageCompletePurchaseContractResponse messageCompletePurchaseContractResponse;

				Decimal calculatedSubTotal;

				if (!SetPurchaseItems(service, MerchantId, purchaseContractId, cart, cartItems, customerLevelAllowsQuantityDiscount, out calculatedSubTotal, out messageSetPurchaseItemsResponse, out exception))
					return String.Format("CheckoutByAmazon: {0}", exception.Message);

                if (!SetContractCharges(service, purchaseContractId, cart.ThisCustomer, calculatedSubTotal, shippingTotal, cart.VatEnabled, !cart.VatIsInclusive, taxTotal, orderTotal, out messageSetContractChargesResponse, out exception))
					return String.Format("CheckoutByAmazon: {0}", exception.Message);

				if (!CompletePurchaseContract(service, purchaseContractId, out messageCompletePurchaseContractResponse, out exception))
					return String.Format("CheckoutByAmazon: {0}", exception.Message);

				amazonOrderIds = messageCompletePurchaseContractResponse.CompletePurchaseContractResult.OrderIds;

				return AppLogic.ro_OK;
			}
			catch (Exception generalException)
			{
				SysLog.LogMessage("Checkout By Amazon: Exception on capture order.", generalException.Message, MessageTypeEnum.Unknown, MessageSeverityEnum.Error);
				return "Checkout By Amazon could not complete the transaction.";
			}
		}

		public Boolean MarkOrderShipped (Int32 orderId, out String errorMessage)
		{
			errorMessage = String.Empty;

			try
			{
				String authorizationPNREF = DB.GetSqlS(String.Format("select AuthorizationPNREF as S from Orders where OrderNumber = {0}", orderId));
				if (String.IsNullOrEmpty(authorizationPNREF))
				{
					errorMessage = "Amazon order ids could not be found on the order.";
					return false;
				}

				IList<String> feedSubmissionIds = new List<String>();

				// Although we currently don't support multi-ship, we still handle a comma delimited list of amazon order ids.
				String[] amazonOrderIds = authorizationPNREF.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				Int32 messageIdx = 1;
				foreach (String amazonOrderId in amazonOrderIds)
				{
					MarketplaceWebService.MarketplaceWebServiceClient client = new MarketplaceWebService.MarketplaceWebServiceClient(this.MwsAccessKeyId, this.MwsSecretKey, "AspDotNetStorefront", "9.x", new MarketplaceWebService.MarketplaceWebServiceConfig
					{
						SignatureVersion = "2",
						ServiceURL = this.UseSandbox ? this.MerchantServiceSandboxUrl : this.MerchantServiceUrl,
					});

					String feedContent = XDocument.Parse(String.Format(@"<?xml version=""1.0"" encoding=""UTF-8""?><AmazonEnvelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:noNamespaceSchemaLocation=""amzn-envelope.xsd""><Header><DocumentVersion>1.01</DocumentVersion><MerchantIdentifier>AspDotNetStorefront</MerchantIdentifier></Header><MessageType>OrderFulfillment</MessageType><Message><MessageID>{2}</MessageID><OrderFulfillment><AmazonOrderID>{0}</AmazonOrderID><FulfillmentDate>{1}</FulfillmentDate></OrderFulfillment></Message></AmazonEnvelope>", amazonOrderId, DateTime.Now.ToString("o"), messageIdx)).ToString();

					MD5 md5 = MD5.Create();
					byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(feedContent));

					using (Stream feedStream = new MemoryStream())
					{
						using (StreamWriter streamWriter = new StreamWriter(feedStream))
						{
							streamWriter.Write(feedContent);
							streamWriter.Flush();

							feedStream.Position = 0;

							SubmitFeedResponse response = client.SubmitFeed(new SubmitFeedRequest
							{
								Marketplace = this.Marketplace,
								Merchant = this.MerchantId,
								ContentMD5 = Convert.ToBase64String(hash),
								FeedContent = feedStream,
								FeedType = "_POST_ORDER_FULFILLMENT_DATA_",
							});

							feedSubmissionIds.Add(response.SubmitFeedResult.FeedSubmissionInfo.FeedSubmissionId);
						}
					}

					messageIdx++;
				}

				DB.ExecuteSQL(String.Format("update Orders set ShippedOn=getdate(), FinalizationData='{1}'  where OrderNumber = {0}", orderId, String.Join(",", feedSubmissionIds.ToArray())));
				return true;
			}
			catch (Exception exception)
			{
				SysLog.LogMessage("Checkout By Amazon: Exception on Mark Order Shipped.", exception.Message, MessageTypeEnum.Unknown, MessageSeverityEnum.Error);
				errorMessage = exception.Message;

				if (exception.InnerException != null)
					errorMessage += exception.InnerException.Message;

				return false;
			}
		}

		public Boolean IsAmazonAddress (Address address)
		{
			return address.Address1 == "Hidden By Amazon";
		}

		#endregion
	}

	#region OrderFulfillmentType

	/// <summary>
	///  The OrderFulfillmentType enumeration defines when the cart will send its order shipped notification to CBA.
	/// </summary>
	public enum OrderFulfillmentType
	{
		/// <summary>
		///  A call to the MWS service will be made immediately after receiving a Ready to Ship notification.
		/// </summary>
		Instant,

		/// <summary>
		///  A call to the MWS service will be made when an admin marks an order shipped in the admin.
		/// </summary>
		MarkedAsShipped,

		/// <summary>
		///  No calls will be made to the MWS service and admins will have to manage fulfillment at Seller Central.
		/// </summary>
		Never,
	}

	#endregion

	#region Instant Order Notification

	/// <summary>
	///  The InstantOrderNotification class is a simple type used to represent a CBA request.
	/// </summary>
	public class InstantOrderNotification
	{
		#region Properties

		/// <summary>
		///  Gets or sets the xml payload sent by CBA.
		/// </summary>
		public XElement Xml { get; set; }

		/// <summary>
		///  Gets or sets the namespace defined in the xml payload.
		/// </summary>
		public XNamespace Xns { get; set; }

		/// <summary>
		///  Gets or sets the uuid sent in the payload to be used for validation.
		/// </summary>
		public String UUID { get; set; }

		/// <summary>
		///  Gets or sets the timestamp sent in the payload to be used for validation.
		/// </summary>
		public String Timestamp { get; set; }

		/// <summary>
		///  Gets or sets the signature sent in the payload to be used for validation.
		/// </summary>
		public String Signature { get; set; }

		/// <summary>
		///  Gets or sets the type of notification this payload is for.
		/// </summary>
		public String NotificationType { get; set; }

		#endregion
	}

	#endregion
}
