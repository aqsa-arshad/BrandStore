// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using AspDotNetStorefrontCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace AspDotNetStorefrontGateways.Processors
{
	public class USAePay : GatewayProcessor
	{
        //Populate default values from AppConfig Parameters
        private static readonly bool UseLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");
		private static readonly bool UseSandBox = AppLogic.AppConfigBool("USAePay.UseSandBox");

        //Set the credentials based on whether or not live transactions are being used
		private static readonly string SourceKey = CommonLogic.IIF(UseLiveTransactions, AppLogic.AppConfig("USAePay.Live.SourceKey"), AppLogic.AppConfig("USAePay.Test.SourceKey"));
		private static readonly string Pin = CommonLogic.IIF(UseLiveTransactions, AppLogic.AppConfig("USAePay.Live.Pin"), AppLogic.AppConfig("USAePay.Test.Pin"));

        /// <summary>
        /// Initializes a new instance of the <see cref="USAePay"/> class.
        /// </summary>
        public USAePay() { }

		#region Recurring
		public override Boolean RecurringIntervalSupportsMultiplier
		{
			get { return false; }
		}

		public override RecurringSupportType RecurringSupportType()
		{
			return Processors.RecurringSupportType.Normal;
		}

		public override List<DateIntervalTypeEnum> GetAllowedRecurringIntervals()
		{
			//http://wiki.usaepay.com/developer/recurringbilling
			//Possible values are: daily, weekly, biweekly, monthly, bimonthly, quarterly, biannually, annually.
			//Default is monthly. If you do not want to enable recurring billing for this customer, set schedule=“disabled”
			List<DateIntervalTypeEnum> includedTypes = new List<DateIntervalTypeEnum>();
			//includedTypes.Add(DateIntervalTypeEnum.Day); //Daily with recurring interval = 1
			includedTypes.Add(DateIntervalTypeEnum.Weekly);
			includedTypes.Add(DateIntervalTypeEnum.BiWeekly);
			includedTypes.Add(DateIntervalTypeEnum.Monthly);
			includedTypes.Add(DateIntervalTypeEnum.Quarterly);
			includedTypes.Add(DateIntervalTypeEnum.SemiYearly); //biannually
			includedTypes.Add(DateIntervalTypeEnum.Yearly); //annually
			return includedTypes;
		}

        public override string RecurringBillingCreateSubscription(
			string SubscriptionDescription, 
			Customer ThisCustomer, 
			Address UseBillingAddress, 
			Address UseShippingAddress, 
			decimal RecurringAmount, 
			DateTime StartDate, 
			int RecurringInterval, 
			DateIntervalTypeEnum RecurringIntervalType, 
			int OriginalRecurringOrderNumber, 
			string XID, 
			IDictionary<string,string> TransactionContext, 
			out string RecurringSubscriptionID, out string RecurringSubscriptionCommand, out string RecurringSubscriptionResult)
        {
			String result = String.Empty; //explicitly set this to AppLogic.ro_OK when things are ok
			RecurringSubscriptionID = String.Empty;
			RecurringSubscriptionCommand = String.Empty;
			RecurringSubscriptionResult = String.Empty;

            if (String.IsNullOrEmpty(GetUSAePaySchedule(RecurringIntervalType)))
            {
                //order has product with invalid recurring interval - bail
				return result = string.Format(CultureInfo.CurrentCulture, "USAePay - Not supported: Product set to invalid Recurring interval type: {0} or Recurring interval: {1}", RecurringIntervalType, RecurringInterval.ToString(CultureInfo.InvariantCulture));
            }

			//set service configuration
			GatewayUSAePay.USAePaySOAP.ueSoapServerPortType client = GetServiceClient(UseSandBox);			
			var token = GetSecurityToken();
            var customer = MakeRecurringUSAePayCustomer(UseBillingAddress, ThisCustomer, RecurringIntervalType, StartDate, RecurringAmount, OriginalRecurringOrderNumber);
            String response = String.Empty;

			try
			{
				response = client.addCustomer(token, customer);
                RecurringSubscriptionID = response;
                RecurringSubscriptionResult = "USAePay Customer Subscription added. USAePay CustNum = " + response;
				result = AppLogic.ro_OK;
                return result;
			}

			catch (Exception err)
			{
                //TODO Card can process ok but if usaepay doesn't like the customer info, 
				//order still goes through but does not create subscription.
				//This is a known issue with recurring - (Gateway.cs::1722 ::1760 and ::1793.)
				result = err.Message;
                RecurringSubscriptionResult = "USAePay Subscription FAILED - " + err.Message;
                return result;
			}
        }
  
		private GatewayUSAePay.USAePaySOAP.CustomerObject MakeRecurringUSAePayCustomer(Address UseBillingAddress, Customer ThisCustomer, DateIntervalTypeEnum RecurringIntervalType, DateTime StartDate, decimal RecurringAmount, int OriginalRecurringOrderNumber)
		{
            var addressBilling = GetUSAePayBillingAddress(UseBillingAddress);
            var customer = new GatewayUSAePay.USAePaySOAP.CustomerObject();
            customer.BillingAddress = addressBilling;
            customer.CustomerID = ThisCustomer.CustomerID.ToString(CultureInfo.InvariantCulture);//ADNSF CustomerId, not USAePay CustNum (SubscriptionId)
            //customer recurring setup http://wiki.usaepay.com/developer/soap-1.6/objects/customerobject
            customer.Schedule = GetUSAePaySchedule(RecurringIntervalType);

            customer.Enabled = true; //enable recurring
            customer.NumLeft = "-1"; //unlimited transactions
            customer.Next = StartDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? String.Empty;
            customer.Amount = Convert.ToDouble(RecurringAmount);
			customer.OrderID = OriginalRecurringOrderNumber.ToString(CultureInfo.InvariantCulture) ?? String.Empty;
            customer.SendReceipt = false; //USAePay gateway sending email receipts

			var payMethod = GetUSAePayCCPaymentMethod(UseBillingAddress);
			customer.PaymentMethods = payMethod;
			return customer;
		}
  
		private GatewayUSAePay.USAePaySOAP.PaymentMethod[] GetUSAePayCCPaymentMethod(Address UseBillingAddress)
		{
			var payMethod = new GatewayUSAePay.USAePaySOAP.PaymentMethod[1];
			payMethod[0] = new GatewayUSAePay.USAePaySOAP.PaymentMethod();
			payMethod[0].AccountType = "CreditCard"; //('CreditCard', 'ACH', 'StoredValue' or 'Invoice')
			payMethod[0].CardType = UseBillingAddress.CardType ?? String.Empty;
			payMethod[0].CardExpiration = string.Format(CultureInfo.InvariantCulture,"{0}-{1}", UseBillingAddress.CardExpirationYear, UseBillingAddress.CardExpirationMonth); //YYYY-MM or MMYY
			payMethod[0].CardNumber = UseBillingAddress.CardNumber ?? String.Empty;
			payMethod[0].AvsStreet = UseBillingAddress.Address1 ?? String.Empty;
			payMethod[0].AvsZip = UseBillingAddress.Zip ?? String.Empty;
			payMethod[0].MethodName = UseBillingAddress.CardName ?? String.Empty;
			return payMethod;
		}

		public override string RecurringBillingCancelSubscription(string RecurringSubscriptionID, int OriginalRecurringOrderNumber, IDictionary<string, string> TransactionContext)
		{
			String result = String.Empty;
			//set service configuration
			GatewayUSAePay.USAePaySOAP.ueSoapServerPortType client = GetServiceClient(UseSandBox);
			
			var token = GetSecurityToken();
			
			try
			{
				if (client.disableCustomer(token, RecurringSubscriptionID))
				{
					return AppLogic.ro_OK;
				}
			}
			catch (Exception e)
			{
				SysLog.LogMessage(String.Format(CultureInfo.CurrentCulture, "USAePay Request Failed - Cancel Subscription for order {0}, USAePay Customer Number {1}", OriginalRecurringOrderNumber.ToString(CultureInfo.InvariantCulture), RecurringSubscriptionID),
					e.Message,
					MessageTypeEnum.GeneralException,
					MessageSeverityEnum.Error);
			}
			result = "USAePay Cancel Subscription Failed";
			return result;
		}

		public override string RecurringBillingAddressUpdate(string RecurringSubscriptionID, int OriginalRecurringOrderNumber, Address UseBillingAddress)
		{
			String result = String.Empty;
			String custNum = RecurringSubscriptionID ?? String.Empty;//clarifying that SubcriptionID is USAePay Customer Number
			if (UseBillingAddress == null)
			{
				return "USAePay Billing Information Update Failed - Address is empty.";
			}
				bool updatePaymentMethod = !String.IsNullOrEmpty(UseBillingAddress.CardNumber)
									&& !String.IsNullOrEmpty(UseBillingAddress.CardExpirationMonth)
									&& !String.IsNullOrEmpty(UseBillingAddress.CardExpirationYear)
									&& UseBillingAddress.PaymentMethodLastUsed == AppLogic.ro_PMCreditCard;

			//set service configuration
			GatewayUSAePay.USAePaySOAP.ueSoapServerPortType client = GetServiceClient(UseSandBox);
			var token = GetSecurityToken();

			var customer = new GatewayUSAePay.USAePaySOAP.CustomerObject();
			var addressBilling = GetUSAePayBillingAddress(UseBillingAddress);

			try
			{
				//get current customer info from USAePay, including existing PaymentMethod
				customer = client.getCustomer(token, custNum);

				//USAePay supports multiple payment methods but we should only have one.
				if (customer.PaymentMethods.Length > 1)
				{
					return "USAePay Payment Method Update Error - Please contact customer support to update subscription";
				}

				if (updatePaymentMethod)
				{
					customer.PaymentMethods = GetUSAePayCCPaymentMethod(UseBillingAddress);
				}

				customer.BillingAddress = addressBilling; //assign billing address
				if (client.updateCustomer(token, custNum, customer))
				{
					result = AppLogic.ro_OK;
				}
				else
				{
					result = "USAePay Billing Information Update Failed";
				}

				return result;
			}
			catch (Exception e)
			{
				SysLog.LogMessage(String.Format(CultureInfo.CurrentCulture, "USAePay Request Failed - Update Billing for order {0}, USAePay custNum {1}", OriginalRecurringOrderNumber.ToString(CultureInfo.InvariantCulture), RecurringSubscriptionID),
					e.Message,
					MessageTypeEnum.GeneralException,
					MessageSeverityEnum.Error);
				return "USAePay Billing Information Update Failed";
			}
		}

		public override string RecurringBillingGetStatusFile()
		{
			return "USAePay DOES NOT CURRENTLY SUPPORT GET FILE.";//USAePay API doesn't seem to support this.
		}

		#endregion
		/// <summary>
        /// Captures the order 
        /// </summary>
        /// <param name="o">The sales order</param>
        /// <returns>Status</returns>
        public override String CaptureOrder(Order o)
        {
            String result = String.Empty;

			//set service configuration
			GatewayUSAePay.USAePaySOAP.ueSoapServerPortType client = GetServiceClient(UseSandBox);

			if (o == null)
				return "Empty Order";

			var token = GetSecurityToken();
			var response = new GatewayUSAePay.USAePaySOAP.TransactionResponse();

            try
            {
				response = client.captureTransaction(token, o.AuthorizationPNREF, Convert.ToDouble(o.Total(true)));
				if (response.ResultCode == "A")
                {
                    result = AppLogic.ro_OK;
                }
                else
				{
					result = string.Format(CultureInfo.CurrentCulture, "Capture Not Processed: {0}", response.Error);
				}
            }
            catch (Exception err)
            {
                result = string.Format("Capture Not Processed - {0}", err.Message);
                //nothing is returned from this method so exceptions are not logged
				SysLog.LogMessage(String.Format(CultureInfo.CurrentCulture, "USAePay Capture Transaction Exception - {0}", o.OrderNumber.ToString(CultureInfo.InvariantCulture)),
					err.Message,
					MessageTypeEnum.GeneralException,
					MessageSeverityEnum.Message);
			}

            return result;
        }

        /// <summary>
        /// Voids the order.
        /// </summary>
        /// <param name="OrderNumber">The order number.</param>
        /// <returns>Status</returns>
        public override String VoidOrder(int OrderNumber)
        {
			String result = String.Empty;
			//set service configuration
			GatewayUSAePay.USAePaySOAP.ueSoapServerPortType client = GetServiceClient(UseSandBox);

			
			var token = GetSecurityToken();

			Order o = new Order(OrderNumber);

			Boolean response;
			try
			{
				response = client.voidTransaction(token, o.AuthorizationPNREF);

				if (response == true)
				{
					result = AppLogic.ro_OK;
				}
				else
				{
					result = "Void not processed";
				}
			}
			catch (Exception err)
			{
                result = string.Format("Void Not Processed - {0}", err.Message);
				//nothing is returned from this method so exceptions are not logged
				SysLog.LogMessage(String.Format(CultureInfo.CurrentCulture, "USAePay Void Transaction Exception - {0}", OrderNumber.ToString(CultureInfo.InvariantCulture)),
					err.Message,
					MessageTypeEnum.GeneralException,
					MessageSeverityEnum.Message);
			}

			return result;

        }

        /// <summary>
        /// Refunds the order.
        /// </summary>
        /// <param name="OriginalOrderNumber">The original order number.</param>
        /// <param name="NewOrderNumber">The new order number.</param>
        /// <param name="RefundAmount">The refund amount. If 0.0M, refund ordertotal</param>
        /// <param name="RefundReason">The refund reason.</param>
        /// <param name="UseBillingAddress">The use billing address.</param>
        /// <returns>Status</returns>
        public override String RefundOrder(int OriginalOrderNumber, int NewOrderNumber, decimal RefundAmount, String RefundReason, Address UseBillingAddress)
        {
            String result = String.Empty;
			double amount;
			//set service configuration
			GatewayUSAePay.USAePaySOAP.ueSoapServerPortType client = GetServiceClient(UseSandBox);

			
			var token = GetSecurityToken();

			Order o = new Order(OriginalOrderNumber);

			// if RefundAmount == 0.0M, then then ENTIRE order amount will be refunded!
			amount = (RefundAmount == System.Decimal.Zero) ? Convert.ToDouble(o.Total(true)) : Convert.ToDouble(RefundAmount);
			var response = new GatewayUSAePay.USAePaySOAP.TransactionResponse();

			try
			{
				response = client.refundTransaction(token, o.AuthorizationPNREF, Convert.ToDouble(amount));

				if (response.ResultCode == "A")
				{
					result = AppLogic.ro_OK;
					o.RefundTXCommand = response.RefNum;
					o.RefundTXResult = string.Format(CultureInfo.CurrentCulture, "{0} - Amount: {1}", response.Result, amount);
				}
				else
				{
					result = string.Format(CultureInfo.CurrentCulture, "Refund not processed: {0}", response.Error);
					o.RefundTXResult = string.Format(CultureInfo.CurrentCulture, "{0} - {1}", response.Result, response.Error);
				}
			}
			catch (Exception err)
			{
                result = string.Format("Refund Not Processed - {0}", err.Message);
                //nothing is returned from this method so exceptions are not logged
				SysLog.LogMessage(String.Format(CultureInfo.CurrentCulture, "USAePay Refund Transaction Exception - {0}", OriginalOrderNumber.ToString(CultureInfo.InvariantCulture)),
					err.Message,
					MessageTypeEnum.GeneralException,
					MessageSeverityEnum.Message);
			}

			return result;
        }

        /// <summary>
        /// Processes the card.
        /// </summary>
        /// <param name="OrderNumber">The order number.</param>
        /// <param name="CustomerID">The customer ID.</param>
        /// <param name="OrderTotal">The order total.</param>
        /// <param name="useLiveTransactions">if set to <c>true</c> [use live transactions].</param>
        /// <param name="TransactionMode">The transaction mode.</param>
        /// <param name="UseBillingAddress">The use billing address.</param>
        /// <param name="CardExtraCode">The card extra code.</param>
        /// <param name="UseShippingAddress">The use shipping address.</param>
        /// <param name="CAVV">The CAVV.</param>
        /// <param name="ECI">The ECI.</param>
        /// <param name="XID">The XID.</param>
        /// <param name="AVSResult">The AVS result.</param>
        /// <param name="AuthorizationResult">The authorization result.</param>
        /// <param name="AuthorizationCode">The authorization code.</param>
        /// <param name="AuthorizationTransID">The authorization trans ID.</param>
        /// <param name="TransactionCommandOut">The transaction command sent to the gateway</param>
        /// <param name="TransactionResponse">The transaction response.</param>
        /// <returns>Status</returns>
		public override String ProcessCard(int OrderNumber, int CustomerID, Decimal OrderTotal, bool useLiveTransactions, TransactionModeEnum TransactionMode, Address UseBillingAddress, String CardExtraCode, Address UseShippingAddress, String CAVV, String ECI, String XID, out String AVSResult, out String AuthorizationResult, out String AuthorizationCode, out String AuthorizationTransID, out String TransactionCommandOut, out String TransactionResponse)
		{
            String result = AppLogic.ro_OK;
            AuthorizationCode = AuthorizationResult = AuthorizationTransID = AVSResult = TransactionCommandOut = TransactionResponse = String.Empty;

			//set service configuration
			GatewayUSAePay.USAePaySOAP.ueSoapServerPortType client = GetServiceClient(UseSandBox);

			
			var token = GetSecurityToken();

			var tran = GetProcessCardTransactionRequestObject(OrderTotal, OrderNumber, UseBillingAddress, CardExtraCode, CAVV, ECI, XID, UseShippingAddress);

			var response = new GatewayUSAePay.USAePaySOAP.TransactionResponse();

            if (TransactionMode == TransactionModeEnum.auth)
            {
				try
                {
					response = client.runAuthOnly(token, tran);
					if (response.ResultCode == "A")
                    {
                        result = AppLogic.ro_OK;
                    }
					else if (response.ResultCode == "D")
					{
						result = string.Format("{0}: {1}", AppLogic.AppConfig("USAePay.Declined"), response.Error);
					}
                    else
					{
						result = string.Format("{0}: {1}", AppLogic.AppConfig("USAePay.TransactionError"), response.Error);
					}
                }
				catch (Exception ex)
				{
					result = string.Format("{0}: {1}", AppLogic.AppConfig("USAePay.ConnectionError"), ex.Message);
				}
            }
            else if (TransactionMode == TransactionModeEnum.authcapture)
            {
				try
                {
                    response = client.runSale(token, tran);
					if (response.ResultCode == "A")
                    {
                        result = AppLogic.ro_OK;
                    }
					else if (response.ResultCode == "D")
					{
						result = string.Format("{0}: {1}", AppLogic.AppConfig("USAePay.Declined"), response.Error);
					}
                    else
					{
						result = string.Format("{0}: {1}", AppLogic.AppConfig("USAePay.TransactionError"), response.Error);
					}
                }
                catch (Exception ex)
				{
                    result = string.Format("{0}: {1}", AppLogic.AppConfig("USAePay.ConnectionError"), ex.Message); 
				}
            }

			AVSResult = response.AvsResult ?? String.Empty;
			AuthorizationResult = response.CardCodeResult ?? String.Empty;
			AuthorizationCode = response.AuthCode ?? String.Empty;
			AuthorizationTransID = response.RefNum ?? String.Empty;
			TransactionCommandOut = response.Error ?? String.Empty;//Approved, etc
			TransactionResponse = response.Result ?? String.Empty;

            return result;
		}

		private string GetUSAePaySchedule(DateIntervalTypeEnum RecurringIntervalType)
		{
			String schedule = String.Empty;
			switch (RecurringIntervalType)
			{
				case DateIntervalTypeEnum.Weekly:
					schedule = "weekly";
					break;
				case DateIntervalTypeEnum.BiWeekly:
					schedule = "biweekly";
					break;
				case DateIntervalTypeEnum.Monthly:
					schedule = "monthly";
					break;
				case DateIntervalTypeEnum.Quarterly:
					schedule = "quarterly";
					break;
				case DateIntervalTypeEnum.SemiYearly:
					schedule = "biannually";
					break;
				case DateIntervalTypeEnum.Yearly:
					schedule = "annually";
					break;
				default:
					//something is wrong, if intervaltype is incorrect it shouldn't run through recurring charge
					schedule = String.Empty;
					break;
			}
			return schedule;
		}

		private GatewayUSAePay.USAePaySOAP.TransactionRequestObject GetProcessCardTransactionRequestObject(decimal OrderTotal, int OrderNumber, Address UseBillingAddress, string CardExtraCode, string CAVV, string ECI, string XID, Address UseShippingAddress)
		{
			var tranDetail = new GatewayUSAePay.USAePaySOAP.TransactionDetail();
			tranDetail.Amount = Convert.ToDouble(OrderTotal);
			tranDetail.AmountSpecified = tranDetail.Amount > 0 ? true : false;
			tranDetail.Description = AppLogic.AppConfig("USAePay.Description");
			tranDetail.Invoice = OrderNumber.ToString(CultureInfo.InvariantCulture); //truncated to 10 chars
			tranDetail.OrderID = OrderNumber.ToString(CultureInfo.InvariantCulture); //64 chars

			var tranCardData = GetUSAePayCreditCardData(UseBillingAddress, CardExtraCode, CAVV, ECI, XID);

			var addressBilling = GetUSAePayBillingAddress(UseBillingAddress);

			var addressShipping = GetUSAePayShippingAddress(UseShippingAddress);
  			
			//This object contains the data needed to run a new transaction, including sale, credit, void and authonly
			var tran = new GatewayUSAePay.USAePaySOAP.TransactionRequestObject();
			tran.AccountHolder = UseBillingAddress.CardName;
			tran.Details = tranDetail;
			tran.BillingAddress = addressBilling;
			tran.ShippingAddress = addressShipping;
			tran.CreditCardData = tranCardData;
			tran.ClientIP = CommonLogic.CustomerIpAddress();
			tran.CustomerID = UseBillingAddress.CustomerID.ToString(CultureInfo.InvariantCulture);
			return tran;
		}
  
		private GatewayUSAePay.USAePaySOAP.CreditCardData GetUSAePayCreditCardData(Address UseBillingAddress, string CardExtraCode, string CAVV, string ECI, string XID)
		{
			var tranCardData = new GatewayUSAePay.USAePaySOAP.CreditCardData();
			tranCardData.CardNumber = UseBillingAddress.CardNumber;
			tranCardData.CardExpiration = UseBillingAddress.CardExpirationMonth.PadLeft(2, '0') + "/" + UseBillingAddress.CardExpirationYear;
			tranCardData.CardCode = CardExtraCode.Length != 0 ? CardExtraCode.Trim() : "-9";//set to -2 if not legible, -9 if not on the card
			tranCardData.AvsStreet = UseBillingAddress.Address1;
			tranCardData.AvsZip = UseBillingAddress.Zip;
			tranCardData.CAVV = CAVV ?? String.Empty;
			tranCardData.ECI = ECI ?? String.Empty;
			tranCardData.XID = XID ?? String.Empty;
			return tranCardData;
		}
  
		private GatewayUSAePay.USAePaySOAP.Address GetUSAePayShippingAddress(Address UseShippingAddress)
		{
			var addressShipping = new GatewayUSAePay.USAePaySOAP.Address();
			addressShipping.FirstName = UseShippingAddress.FirstName;
			addressShipping.LastName = UseShippingAddress.LastName;
			addressShipping.Company = UseShippingAddress.Company;
			addressShipping.Street = UseShippingAddress.Address1;
			addressShipping.Street2 = UseShippingAddress.Address2;
			addressShipping.City = UseShippingAddress.City;
			addressShipping.State = UseShippingAddress.State;
			addressShipping.Zip = UseShippingAddress.Zip;
			addressShipping.Country = UseShippingAddress.Country;
			addressShipping.Phone = UseShippingAddress.Phone;
			addressShipping.Email = UseShippingAddress.EMail;
			return addressShipping;
		}
  
		private GatewayUSAePay.USAePaySOAP.Address GetUSAePayBillingAddress(Address UseBillingAddress)
		{
			var addressBilling = new GatewayUSAePay.USAePaySOAP.Address();
			addressBilling.FirstName = UseBillingAddress.FirstName;
			addressBilling.LastName = UseBillingAddress.LastName;
			addressBilling.Company = UseBillingAddress.Company;
			addressBilling.Street = UseBillingAddress.Address1;
			addressBilling.Street2 = UseBillingAddress.Address2;
			addressBilling.City = UseBillingAddress.City;
			addressBilling.State = UseBillingAddress.State;
			addressBilling.Zip = UseBillingAddress.Zip;
			addressBilling.Country = UseBillingAddress.Country;
			addressBilling.Phone = UseBillingAddress.Phone;
			addressBilling.Email = UseBillingAddress.EMail;
			return addressBilling;
		}
  
		private GatewayUSAePay.USAePaySOAP.ueSoapServerPortType GetServiceClient(bool useTheSandBox)
		{
			//set service configuration - useTheSandBox passed in to make it clear it's being used.  I know it's a parameter ;)
			//sandbox url requires SourceKey/Pin from valid USAePay "Test Account". Set new sourcekey at https://sandbox.usaepay.com/login

			var wsdl = useTheSandBox ? AppLogic.AppConfig("USAePay.EndpointSandbox") : AppLogic.AppConfig("USAePay.EndpointLive");
			var endpointAddress = new System.ServiceModel.EndpointAddress(new Uri(wsdl));
			var binding = new System.ServiceModel.BasicHttpBinding();
			binding.Name = "ueSoapServerBinding";
			binding.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.Transport;
  			
			// wiki.usaepay.com/developer/soap-1.6/howto/vb2010
			GatewayUSAePay.USAePaySOAP.ueSoapServerPortType client = new GatewayUSAePay.USAePaySOAP.ueSoapServerPortTypeClient(binding, endpointAddress);
			return client;
		}
  
		private GatewayUSAePay.USAePaySOAP.ueSecurityToken GetSecurityToken()
		{
			//make ueSecurityToken - wiki.usaepay.com/developer/soap-1.6/howto/csharp
			var token = new GatewayUSAePay.USAePaySOAP.ueSecurityToken();
			token.SourceKey = SourceKey;
			token.ClientIP = CommonLogic.CustomerIpAddress();
			token.PinHash = new GatewayUSAePay.USAePaySOAP.ueHash();
			token.PinHash.Seed = Guid.NewGuid().ToString(); //can hardcode a number like 1234 for debug
			token.PinHash.Type = "md5";

			String preHashValue = String.Empty;
			preHashValue = token.SourceKey + token.PinHash.Seed + Pin;
			
			#region CalculateMD5
			
			// step 1, calculate MD5 hash from input
			// Create a new instance of the MD5CryptoServiceProvider object.
			MD5 md5Hasher = MD5.Create();
			
			// Convert the input string to a byte array and compute the hash.
			byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(preHashValue));
			
			// Create a new Stringbuilder to collect the bytes
			// and create a string.
			StringBuilder sBuilder = new StringBuilder();
			
			// Loop through each byte of the hashed data 
			// and format each one as a hexadecimal string.
			for (int i = 0; i < data.Length; i++)
			{
				sBuilder.Append(data[i].ToString("x2", CultureInfo.InvariantCulture));
			}
			
			#endregion
			
			token.PinHash.HashValue = sBuilder.ToString();
			return token;
		}
	}
}
