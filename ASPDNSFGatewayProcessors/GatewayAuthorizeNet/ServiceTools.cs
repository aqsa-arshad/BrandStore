// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using System.Diagnostics;
using GatewayAuthorizeNet.AuthorizeNetApi;

namespace GatewayAuthorizeNet
{
    /// <summary>
    /// Summary description for AuthorizeNet.
    /// </summary>
    public class ServiceTools        
    {
		public static MerchantAuthenticationType CreateMerchantAuthenticationType(string merchantName, string merchantTransactionKey)
        {
			MerchantAuthenticationType mat = new MerchantAuthenticationType()
			{
				name = merchantName,
				transactionKey = merchantTransactionKey,
			};
			return mat;
        }

		#region Transaction Methods

		public static ProfileTransactionType CreateProfileTransactionType(ProfileTransOrderType transType)
		{
			return new ProfileTransactionType()
			{
				Item = transType
			};
		}

		public static ProfileTransactionType CreateProfileTransactionType(ProfileTransRefundType transType)
		{
			return new ProfileTransactionType()
			{
				Item = transType
			};
		}

		public static ProfileTransactionType CreateProfileTransactionType(ProfileTransVoidType transType)
		{
			return new ProfileTransactionType()
			{
				Item = transType
			};
		}

		public static ProfileTransAuthOnlyType CreateProfileTransAuthOnlyType(Int64 profileId, Int64 paymentProfileId, OrderExType order, decimal amount)
		{			
			return new ProfileTransAuthOnlyType()
			{				
				customerProfileId = profileId,
				customerPaymentProfileId = paymentProfileId,
				amount = amount,
				order = order,
			};
		}

		public static ProfileTransAuthCaptureType CreateProfileTransAuthCaptureType(Int64 profileId, Int64 paymentProfileId, OrderExType order, decimal amount)
		{
			return new ProfileTransAuthCaptureType()
			{
				customerProfileId = profileId,
				customerPaymentProfileId = paymentProfileId,
				amount = amount,
				order = order,
			};
		}

		public static ProfileTransCaptureOnlyType CreateProfileTransCaptureOnlyType(Int64 profileId, Int64 paymentProfileId, string authCode, decimal amount)
		{
			return new ProfileTransCaptureOnlyType()
			{
				customerProfileId = profileId,
				customerPaymentProfileId = paymentProfileId,
				amount = amount,
				approvalCode = authCode
			};
		}

		public static ProfileTransRefundType CreateProfileTransRefundType(Int64 profileId, Int64 paymentProfileId, string transId, decimal amount)
		{
			return new ProfileTransRefundType()
			{
				customerProfileId = profileId,
				customerPaymentProfileId = paymentProfileId,
				amount = amount,
				transId = transId
			};
		}

		public static ProfileTransVoidType CreateProfileTransVoidType(Int64 profileId, Int64 paymentProfileId, string transactionId)
		{
			return new ProfileTransVoidType()
			{
				customerProfileId = profileId,
				customerPaymentProfileId = paymentProfileId,
				transId = transactionId
			};
		}

		public static OrderExType CreateOrderExType(string orderNumber, string description, string purchseOrderNumber)
		{
			return new OrderExType()
			{
				invoiceNumber = orderNumber,
				description = description,
				purchaseOrderNumber = purchseOrderNumber
			};
		}

		public static CIMResponse ParseDirectResponse(string responseText)
		{
			if (string.IsNullOrEmpty(responseText))
			{
				return new CIMResponse()
				{
					Success = false
				};
			}

			var responseSplit = responseText.Split(',').Select(s => s.Trim()).ToArray();
			if (responseSplit.Length <= 1)
			{
				responseSplit = responseText.Split('|').Select(s => s.Trim()).ToArray();
			}
			if (responseSplit.Length <= 1)
			{
				return new CIMResponse()
				{
					Success = false
				};
			}

			String responseCode = responseSplit[2];
			String authResponseText = responseSplit[3];
			String approvalCode = responseSplit[4];
			String avsResult = responseSplit[5];
			String transID = responseSplit[6];

			return new CIMResponse()
			{
				AuthCode = approvalCode,
				AvsCode = avsResult,
				AuthMessage = authResponseText,
				Success = responseCode == "1",
				TransactionId = transID
			};
		}

		#endregion

		#region Profile Methods

		public static CustomerProfileType CreateCustomerProfileType(int customerId, string email, string description)
		{
			return new CustomerProfileType()
			{
				email = email,
				merchantCustomerId = customerId.ToString(),
				description = description
			};
		}

		public static CustomerProfileExType CreateCustomerProfileExType(Int64 profileId, int customerId, string email, string description)
		{
			return new CustomerProfileExType()
			{
				email = email,
				merchantCustomerId = customerId.ToString(),
				description = description,
				customerProfileId = profileId
			};
		}

		public static CustomerPaymentProfileType CreatePaymentProfileType(CustomerAddressType addressType, PaymentType paymentType)
		{
			return new CustomerPaymentProfileType()
			{
				 billTo = addressType,
				 payment = paymentType
			};
		}

		public static CustomerPaymentProfileExType CreatePaymentProfileExType(Int64 paymentProfileId, CustomerAddressType addressType, PaymentType paymentType)
		{
			return new CustomerPaymentProfileExType()
			{
				billTo = addressType,
				payment = paymentType,
				customerPaymentProfileId = paymentProfileId
			};
		}

		public static PaymentType CreatePaymentType(string creditCardNumber, string cardCode, string expDate)
		{
			CreditCardType cardType = new CreditCardType()
			{
				cardNumber = creditCardNumber,
				expirationDate = expDate,
				cardCode = cardCode
			};

			return new PaymentType()
			{
				 Item = cardType
			};
		}

		#endregion

		#region Helper Methods

		public static XmlDocument SerializeToXml<T>(T objToSerialize)
		{
			MemoryStream memoryStream = new MemoryStream();
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
			XmlWriter writer = new XmlTextWriter(memoryStream, Encoding.UTF8);
			xmlSerializer.Serialize(writer, objToSerialize);

			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(memoryStream);
			return xmlDoc;
		}

		public static string GetAuthNetExpirationDate(int month, int year)
		{
			return String.Format("{1}-{0:00}", month, year);
		}

		public static void GetExpirationDateFromAuthNetString(string authNetExpDate, out int month, out int year)
		{
			month = 0;
			year = 0;

			var authNetSplit = authNetExpDate.Split('-');
			month = int.Parse(authNetSplit[0]);
			year = int.Parse(authNetSplit[1]);
		}

		#endregion

	}
}
