// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;

namespace GatewayMoneybookers
{
	class PaymentRequest<TPaymentMethod>
		where TPaymentMethod : PaymentMethod
	{
		public UniqueIdentifier InstanceId { get; protected set; }
		public TransactionMode TransactionMode { get; protected set; }
		public ResponseMode ResponseMode { get; protected set; }
		public UniqueIdentifier ChannelId { get; protected set; }
		public UniqueIdentifier UserLogin { get; protected set; }
		public UserPassword UserPassword { get; protected set; }
		public LimitedString TransactionId { get; protected set; }
		public LimitedString InvoiceId { get; protected set; }
		public LimitedString ShopperId { get; protected set; }
		public UniqueIdentifier ReferenceId { get; protected set; }
		public PaymentCode PaymentCode { get; protected set; }
		public Amount PaymentAmount { get; protected set; }
		public Currency PaymentCurrency { get; protected set; }
		public LimitedString PaymentUsage { get; protected set; }
		public Prefix CustomerSalutation { get; protected set; }
		public Prefix CustomerTitle { get; protected set; }
		public Name CustomerGivenName { get; protected set; }
		public Name CustomerFamilyName { get; protected set; }
		public Sex CustomerSex { get; protected set; }
		public Date CustomerBirthdate { get; protected set; }
		public Name CustomerCompany { get; protected set; }
		public Street CustomerStreet { get; protected set; }
		public Zip CustomerZip { get; protected set; }
		public City CustomerCity { get; protected set; }
		public State CustomerState { get; protected set; }
		public CountryCode CustomerCountry { get; protected set; }
		public PhoneNumber CustomerPhone { get; protected set; }
		public MobilePhoneNumber CustomerMobilePhone { get; protected set; }
		public EmailAddress CustomerEmail { get; protected set; }
		public IpAddress CustomerIp { get; protected set; }
		public LimitedString ResponseUrl { get; protected set; }
		public LimitedString SessionId { get; protected set; }

		/// <param name="instanceId">Must not be null</param>
		/// <param name="transactionMode">Must not be null</param>
		/// <param name="responseMode">Must not be null</param>
		/// <param name="channelId">Must not be null</param>
		/// <param name="userLogin">Must not be null</param>
		/// <param name="userPassword">Must not be null</param>
		/// <param name="transactionId"></param>
		/// <param name="invoiceId"></param>
		/// <param name="shopperId"></param>
		/// <param name="referenceId">Must not be null for refernce transactions</param>
		/// <param name="paymentCode">Must not be null</param>
		/// <param name="paymentAmount">Must not be null</param>
		/// <param name="paymentCurrency">Must not be null</param>
		/// <param name="paymentUsage">Must not be null</param>
		/// <param name="customerSalutation"></param>
		/// <param name="customerTitle"></param>
		/// <param name="customerGivenName">Must not be null for originating transactions</param>
		/// <param name="customerFamilyName">Must not be null for originating transactions</param>
		/// <param name="customerSex"></param>
		/// <param name="customerBirthdate"></param>
		/// <param name="customerCompany"></param>
		/// <param name="customerStreet">Must not be null for originating transactions</param>
		/// <param name="customerZip">Must not be null for originating transactions</param>
		/// <param name="customerCity">Must not be null for originating transactions</param>
		/// <param name="customerState"></param>
		/// <param name="customerCountry">Must not be null for originating transactions</param>
		/// <param name="customerPhone"></param>
		/// <param name="customerMobilePhone"></param>
		/// <param name="customerEmail">Must not be null for originating transactions</param>
		/// <param name="customerIp">Must not be null for originating transactions</param>
		/// <param name="responseUrl">Must not be null for Virtual Account transactions</param>
		public PaymentRequest(
			UniqueIdentifier instanceId,
			TransactionMode transactionMode,
			ResponseMode responseMode,
			UniqueIdentifier channelId,
			UniqueIdentifier userLogin,
			UserPassword userPassword,
			LimitedString transactionId,
			LimitedString invoiceId,
			LimitedString shopperId,
			UniqueIdentifier referenceId,
			PaymentCode paymentCode,
			Amount paymentAmount,
			Currency paymentCurrency,
			LimitedString paymentUsage,
			Prefix customerSalutation,
			Prefix customerTitle,
			Name customerGivenName,
			Name customerFamilyName,
			Sex customerSex,
			Date customerBirthdate,
			Name customerCompany,
			Street customerStreet,
			Zip customerZip,
			City customerCity,
			State customerState,
			CountryCode customerCountry,
			PhoneNumber customerPhone,
			MobilePhoneNumber customerMobilePhone,
			EmailAddress customerEmail,
			IpAddress customerIp,
			LimitedString responseUrl,
			LimitedString sessionId)
		{
			if(instanceId == null)
				throw new ArgumentNullException("instanceId");

			if(instanceId == null)
				throw new ArgumentNullException("instanceId");

			if(responseMode == null)
				throw new ArgumentNullException("responseMode");

			if(channelId == null)
				throw new ArgumentNullException("channelId");

			if(userLogin == null)
				throw new ArgumentNullException("userLogin");

			if(userPassword == null)
				throw new ArgumentNullException("userPassword");

			if(referenceId == null && (	paymentCode.PaymentType == PaymentType.Capture || 
										paymentCode.PaymentType == PaymentType.Reversal || 
										paymentCode.PaymentType == PaymentType.Refund || 
										paymentCode.PaymentType == PaymentType.Registration || 
										paymentCode.PaymentType == PaymentType.Deregistration || 
										paymentCode.PaymentType == PaymentType.Reschedule))
				throw new ArgumentNullException("referenceId");

			if(paymentCode == null)
				throw new ArgumentNullException("paymentCode");

			if(paymentAmount == null)
				throw new ArgumentNullException("paymentAmount");

			if(paymentCurrency == null)
				throw new ArgumentNullException("paymentCurrency");

			if(paymentUsage == null)
				throw new ArgumentNullException("paymentUsage");

			if(paymentCode.PaymentType == PaymentType.Preauthorisation ||
				paymentCode.PaymentType == PaymentType.Debit)
			{

				if(customerGivenName == null)
					throw new ArgumentNullException("customerGivenName");

				if(customerFamilyName == null)
					throw new ArgumentNullException("customerFamilyName");

				if(customerStreet == null)
					throw new ArgumentNullException("customerStreet");

				if(customerZip == null)
					throw new ArgumentNullException("customerZip");

				if(customerCity == null)
					throw new ArgumentNullException("customerCity");

				if(customerCountry == null)
					throw new ArgumentNullException("customerCountry");

				if(customerEmail == null)
					throw new ArgumentNullException("customerEmail");

				if(customerIp == null)
					throw new ArgumentNullException("customerIp");
			}

			if(paymentCode.PaymentMethod is VirtualAccountPaymentMethod)
			{
				if(responseUrl == null)
					throw new ArgumentNullException("responseUrl");
			}

			InstanceId = instanceId;
			TransactionMode = transactionMode;
			ResponseMode = responseMode;
			ChannelId = channelId;
			UserLogin = userLogin;
			UserPassword = userPassword;
			TransactionId = transactionId;
			InvoiceId = invoiceId;
			ShopperId = shopperId;
			ReferenceId = referenceId;
			PaymentCode = paymentCode;
			PaymentAmount = paymentAmount;
			PaymentCurrency = paymentCurrency;
			PaymentUsage = paymentUsage;
			CustomerSalutation = customerSalutation;
			CustomerTitle = customerTitle;
			CustomerGivenName = customerGivenName;
			CustomerFamilyName = customerFamilyName;
			CustomerSex = customerSex;
			CustomerBirthdate = customerBirthdate;
			CustomerCompany = customerCompany;
			CustomerStreet = customerStreet;
			CustomerZip = customerZip;
			CustomerCity = customerCity;
			CustomerState = customerState;
			CustomerCountry = customerCountry;
			CustomerPhone = customerPhone;
			CustomerMobilePhone = customerMobilePhone;
			CustomerEmail = customerEmail;
			CustomerIp = customerIp;
			ResponseUrl = responseUrl;
			SessionId = sessionId;
		}
	}
}
