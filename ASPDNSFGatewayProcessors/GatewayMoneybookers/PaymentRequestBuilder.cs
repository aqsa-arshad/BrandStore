// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AspDotNetStorefrontCore;

namespace GatewayMoneybookers
{
	class PaymentRequestBuilder
	{
		public TransactionMode TransactionMode
		{
			get
			{
				if(AppLogic.AppConfigBool("UseLiveTransactions"))
					return TransactionMode.Live;
				else
					return TransactionMode.ConnectorTest;
			}
		}

		public PaymentRequest<CreditCardPaymentMethod> BuildCreditCardPaymentRequest(
			IPaymentType paymentType,
			int orderNumber,
			string orderReferenceId,
			int orderCustomerId,
			string orderCustomerEmail, 
			string orderCustomerFirstName, 
			string orderCustomerLastName, 
			string orderCustomerAddress, 
			string orderCustomerCity, 
			string orderCustomerState, 
			string orderCustomerZip,
			string orderCustomerCountry, 
			string orderCustomerIp,
			decimal orderTotal, 
			string orderCreditCardType, 
			string orderCreditCardNumber, 
			string orderCreditCardExpirationMonth, 
			string orderCreditCardExpirationYear,
			string orderCreditCardIssueNumber,
			string orderCreditCardCvv)
		{
			UniqueIdentifier instanceId = LoadAppConfigString("Moneybookers.SenderID", s => new UniqueIdentifier(s));
			TransactionMode transactionMode = TransactionMode;
			ResponseMode responseMode = ResponseMode.Sync;
			UniqueIdentifier channelId = LoadAppConfigString("Moneybookers.ChannelID", s => new UniqueIdentifier(s));
			UniqueIdentifier userLogin = LoadAppConfigString("Moneybookers.UserLogin", s => new UniqueIdentifier(s));
			UserPassword userPassword = LoadAppConfigString("Moneybookers.UserPassword", s => new UserPassword(s));
			LimitedString transactionId = null;
			LimitedString invoiceId = new LimitedString(orderNumber.ToString());
			LimitedString shopperId = new LimitedString(orderCustomerEmail.TrimToMax(LimitedString.MaxLength));
			
			UniqueIdentifier referenceId = null;
			if(orderReferenceId != null)
				referenceId = new UniqueIdentifier(orderReferenceId);
			
			Amount paymentAmount = new Amount(orderTotal);
			Currency paymentCurrency = LoadAppConfigString("Localization.StoreCurrency", s => new Currency(s));
			LimitedString paymentUsage = new LimitedString(String.Format("Order number {0}", orderNumber));
			Prefix customerSalutation = null;
			Prefix customerTitle = null;
			Name customerGivenName = new Name(orderCustomerFirstName.TrimToMax(Name.MaxLength));
			Name customerFamilyName = new Name(orderCustomerLastName.TrimToMax(Name.MaxLength));
			Sex customerSex = null;
			Date customerBirthdate = null;
			Name customerCompany = null;
			Street customerStreet = new Street(orderCustomerAddress.TrimToMax(Street.MaxLength));
			Zip customerZip = new Zip(orderCustomerZip.TrimToMax(Zip.MaxLength));
			City customerCity = new City(orderCustomerCity.TrimToMax(City.MaxLength));
			
			State customerState = null;
			if(orderCustomerState != null)
				customerState = new State(orderCustomerState.TrimToMax(State.MaxLength));

			string orderCustomerCountryCode = AppLogic.GetCountryTwoLetterISOCode(orderCustomerCountry);
			CountryCode customerCountry = new CountryCode(orderCustomerCountryCode.TrimToMax(CountryCode.MaxLength));

			PhoneNumber customerPhone = null;
			MobilePhoneNumber customerMobilePhone = null;

			EmailAddress customerEmail ;
			if(String.IsNullOrEmpty(orderCustomerEmail))
				customerEmail = new EmailAddress("no_email_provided@aspdotnetstorefront.com".TrimToMax(EmailAddress.MaxLength));
			else
				customerEmail = new EmailAddress(orderCustomerEmail.TrimToMax(EmailAddress.MaxLength));

			var formattedOrderCustomerIp = String.Join(".", orderCustomerIp.Split('.').Select(s => s.PadLeft(3, '0')).ToArray());
			IpAddress customerIp = new IpAddress(formattedOrderCustomerIp);

			AccountHolder accountHolder = new AccountHolder(String.Format("{0} {1}", orderCustomerFirstName, orderCustomerLastName).TrimToMax(AccountHolder.MaxLength));
			AccountNumber accountNumber = new AccountNumber(orderCreditCardNumber);
			AccountBrand accountBrand = new AccountBrand(orderCreditCardType);
			AccountExpiryMonth accountExpiryMonth = new AccountExpiryMonth(orderCreditCardExpirationMonth);
			AccountExpiryYear accountExpiryYear = new AccountExpiryYear(orderCreditCardExpirationYear);
			
			AccountCardIssueNumber accountCardIssueNumber = null;
			if(orderCreditCardIssueNumber != null)
				accountCardIssueNumber = new AccountCardIssueNumber(orderCreditCardIssueNumber);
			
			AccountVerification accountVerification = new AccountVerification(orderCreditCardCvv);

			LimitedString sessionId = new LimitedString(orderCustomerId.ToString());

			CreditCardPaymentMethod paymentMethod = new CreditCardPaymentMethod(
				accountHolder,
				accountNumber,
				accountBrand,
				accountExpiryMonth,
				accountExpiryYear,
				accountCardIssueNumber,
				accountVerification);

			PaymentCode paymentCode = new PaymentCode(paymentMethod, paymentType);

			PaymentRequest<CreditCardPaymentMethod> paymentRequest = new PaymentRequest<CreditCardPaymentMethod>(
				instanceId,
				transactionMode,
				responseMode,
				channelId,
				userLogin,
				userPassword,
				transactionId,
				invoiceId,
				shopperId,
				referenceId,
				paymentCode,
				paymentAmount,
				paymentCurrency,
				paymentUsage,
				customerSalutation,
				customerTitle,
				customerGivenName,
				customerFamilyName,
				customerSex,
				customerBirthdate,
				customerCompany,
				customerStreet,
				customerZip,
				customerCity,
				customerState,
				customerCountry,
				customerPhone,
				customerMobilePhone,
				customerEmail,
				customerIp,
				new LimitedString(AppLogic.GetStoreHTTPLocation(true) + "moneybookers3DSecureCallback.aspx"),
				sessionId);

			return paymentRequest;
		}

		public PaymentRequest<TPaymentMethod> BuildReferencedPaymentRequest<TPaymentMethod>(
				IPaymentType paymentType,
				int orderNumber,
				string orderReferenceId,
				decimal orderTotal)
			where TPaymentMethod : PaymentMethod
		{
			UniqueIdentifier instanceId = LoadAppConfigString("Moneybookers.SenderID", s => new UniqueIdentifier(s));
			TransactionMode transactionMode = TransactionMode;
			ResponseMode responseMode = ResponseMode.Sync;
			UniqueIdentifier channelId = LoadAppConfigString("Moneybookers.ChannelID", s => new UniqueIdentifier(s));
			UniqueIdentifier userLogin = LoadAppConfigString("Moneybookers.UserLogin", s => new UniqueIdentifier(s));
			UserPassword userPassword = LoadAppConfigString("Moneybookers.UserPassword", s => new UserPassword(s));
			LimitedString invoiceId = new LimitedString(orderNumber.ToString());

			UniqueIdentifier referenceId = null;
			if(orderReferenceId != null)
				referenceId = new UniqueIdentifier(orderReferenceId);

			Amount paymentAmount = new Amount(orderTotal);
			Currency paymentCurrency = LoadAppConfigString("Localization.StoreCurrency", s => new Currency(s));
			LimitedString paymentUsage = new LimitedString(String.Format("Order number {0}", orderNumber));

			PaymentMethod referencedPaymentMethod = PaymentMethod.GetReferencedPaymentMethod<TPaymentMethod>();
			PaymentCode paymentCode = new PaymentCode(referencedPaymentMethod, paymentType);

			PaymentRequest<TPaymentMethod> paymentRequest = new PaymentRequest<TPaymentMethod>(
				instanceId,
				transactionMode,
				responseMode,
				channelId,
				userLogin,
				userPassword,
				null,
				invoiceId,
				null,
				referenceId,
				paymentCode,
				paymentAmount,
				paymentCurrency,
				paymentUsage,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				new LimitedString(AppLogic.GetStoreHTTPLocation(true) + "moneybookers3DSecureCallback.aspx"),
				null);

			return paymentRequest;
		}

		public PaymentRequest<VirtualAccountPaymentMethod> BuildQuickCheckoutRequest(
			string orderTransactionId,
			int orderCustomerId,
			string orderCustomerEmail,
			string orderCustomerFirstName,
			string orderCustomerLastName,
			string orderCustomerAddress,
			string orderCustomerCity,
			string orderCustomerState,
			string orderCustomerZip,
			string orderCustomerCountry,
			string orderCustomerIp,
			decimal orderTotal)
		{
			UniqueIdentifier instanceId = LoadAppConfigString("Moneybookers.SenderID", s => new UniqueIdentifier(s));
			TransactionMode transactionMode = TransactionMode;
			ResponseMode responseMode = ResponseMode.Async;
			UniqueIdentifier channelId = LoadAppConfigString("Moneybookers.ChannelID", s => new UniqueIdentifier(s));
			UniqueIdentifier userLogin = LoadAppConfigString("Moneybookers.UserLogin", s => new UniqueIdentifier(s));
			UserPassword userPassword = LoadAppConfigString("Moneybookers.UserPassword", s => new UserPassword(s));
			LimitedString transactionId = new LimitedString(orderTransactionId.TrimToMax(LimitedString.MaxLength));
			LimitedString invoiceId = null;
			LimitedString shopperId = new LimitedString(orderCustomerEmail.TrimToMax(LimitedString.MaxLength));
			UniqueIdentifier referenceId = null;
			Amount paymentAmount = new Amount(orderTotal);
			Currency paymentCurrency = LoadAppConfigString("Localization.StoreCurrency", s => new Currency(s));
			LimitedString paymentUsage = new LimitedString("Online order");
			Prefix customerSalutation = null;
			Prefix customerTitle = null;
			Name customerGivenName = new Name(orderCustomerFirstName.TrimToMax(Name.MaxLength));
			Name customerFamilyName = new Name(orderCustomerLastName.TrimToMax(Name.MaxLength));
			Sex customerSex = null;
			Date customerBirthdate = null;
			Name customerCompany = null;
			Street customerStreet = new Street(orderCustomerAddress.TrimToMax(Street.MaxLength));
			Zip customerZip = new Zip(orderCustomerZip.TrimToMax(Zip.MaxLength));
			City customerCity = new City(orderCustomerCity.TrimToMax(City.MaxLength));

			State customerState = null;
			if(orderCustomerState != null)
				customerState = new State(orderCustomerState.TrimToMax(State.MaxLength));

			string orderCustomerCountryCode = AppLogic.GetCountryTwoLetterISOCode(orderCustomerCountry);
			CountryCode customerCountry = new CountryCode(orderCustomerCountryCode.TrimToMax(CountryCode.MaxLength));

			PhoneNumber customerPhone = null;
			MobilePhoneNumber customerMobilePhone = null;
			EmailAddress customerEmail = new EmailAddress(orderCustomerEmail.TrimToMax(EmailAddress.MaxLength));

			var formattedOrderCustomerIp = String.Join(".", orderCustomerIp.Split('.').Select(s => s.PadLeft(3, '0')).ToArray());
			IpAddress customerIp = new IpAddress(formattedOrderCustomerIp);

			AccountHolder accountHolder = new AccountHolder(String.Format("{0} {1}", orderCustomerFirstName, orderCustomerLastName).TrimToMax(AccountHolder.MaxLength));
			AccountId accountId = new AccountId(orderCustomerEmail.TrimToMax(AccountId.MaxLength));

			LimitedString sessionId = new LimitedString(orderCustomerId.ToString());

			VirtualAccountPaymentMethod paymentMethod = new VirtualAccountPaymentMethod(accountHolder, accountId);

			PaymentCode paymentCode = new PaymentCode(paymentMethod, PaymentType.Debit);

			PaymentRequest<VirtualAccountPaymentMethod> paymentRequest = new PaymentRequest<VirtualAccountPaymentMethod>(
				instanceId,
				transactionMode,
				responseMode,
				channelId,
				userLogin,
				userPassword,
				transactionId,
				invoiceId,
				shopperId,
				referenceId,
				paymentCode,
				paymentAmount,
				paymentCurrency,
				paymentUsage,
				customerSalutation,
				customerTitle,
				customerGivenName,
				customerFamilyName,
				customerSex,
				customerBirthdate,
				customerCompany,
				customerStreet,
				customerZip,
				customerCity,
				customerState,
				customerCountry,
				customerPhone,
				customerMobilePhone,
				customerEmail,
				customerIp,
				new LimitedString(AppLogic.GetStoreHTTPLocation(true) + "moneybookersQuickCheckoutCallback.aspx"), 
				sessionId);

			return paymentRequest;
		}

		private TResult LoadAppConfigString<TResult>(string key, Func<string, TResult> resultLoader)
		{
			string value;
			TResult result;

			try
			{
				value = AppLogic.AppConfig(key);
				result = resultLoader(value);
				return result;
			}
			catch(Exception exception)
			{
				throw new Exception("The \"" + key + "\" AppConfig is not configured correctly: " + exception.Message, exception);
			}
		}
	}
}
