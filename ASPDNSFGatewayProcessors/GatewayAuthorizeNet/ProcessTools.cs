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

namespace GatewayAuthorizeNet
{
	public class ProcessTools
	{
		private const String OK = "OK";
		private const String ERROR = "ERROR";

		public static String ProcessCard(Int32 orderNumber, Int32 customerId, Decimal orderTotal, Int64 paymentProfileId, String transactionMode,
			Boolean useLiveTransactions, out String AVSResult, out String AuthorizationResult,
			out String AuthorizationCode, out String AuthorizationTransID, out String TransactionCommandOut, out String TransactionResponse)
		{
			AVSResult = String.Empty;
			AuthorizationResult = String.Empty;
			AuthorizationCode = String.Empty;
			AuthorizationTransID = String.Empty;
			TransactionCommandOut = String.Empty;
			TransactionResponse = String.Empty;

			String Status = ERROR;

			PaymentProcessor paymentProcessor = new PaymentProcessor();
			CIMResponse processStatus = null;
			Int64 profileId = DataUtility.GetProfileId(customerId);
			if(profileId > 0)
			{
				if(paymentProfileId > 0)
				{
					if(transactionMode.ToUpperInvariant() == "AUTH")
					{
						processStatus = paymentProcessor.Authorize(profileId, paymentProfileId, orderNumber, orderTotal);
					}
					else if(transactionMode.ToUpperInvariant() == "AUTHCAPTURE")
					{
						processStatus = paymentProcessor.AuthCapture(profileId, paymentProfileId, orderNumber, orderTotal);
					}
				}
			}
			if (processStatus != null)
			{
				if (processStatus.Success)
				{
					Status = OK;
					AVSResult = processStatus.AvsCode;
					AuthorizationResult = processStatus.AuthMessage;
					AuthorizationCode = processStatus.AuthCode;
					AuthorizationTransID = processStatus.TransactionId;
				}
				else
				{
					Status = processStatus.AuthMessage;

					if(String.IsNullOrEmpty(Status))
						Status = "There was an error processing your payment. Please try again, choose a different payment method, or contact customer support.";
				}
			}
			return Status;
		}


		public static Int64 SaveProfileAndPaymentProfile(Int32 customerId, String email, String storeName, Int64 paymentProfileId, Int32 addressId, String cardNumber, String cardCode, String expMonth, String expYear, out String errorMessage, out string errorCode)
		{
			errorMessage = String.Empty;
			errorCode = String.Empty;
			Int64 profileId = DataUtility.GetProfileId(customerId);
            ProfileManager profileMgr = profileId > 0 ? new ProfileManager(customerId, email, profileId) : null;

            // Create profile if needed
            if(profileMgr == null || !profileMgr.UpdateProfile(email, storeName + " CIM Profile"))
			{
                //Clear out the profile id incase this is a case where the auth.net account has changed and we need to create
                //a new profile for a customer that already has a profile id
                paymentProfileId = 0;
				profileMgr = ProfileManager.CreateProfile(customerId, email, storeName + " CIM Profile", out errorMessage);

				if(profileMgr == null)
					return 0;

				profileId = profileMgr.ProfileId;

				if (profileId <= 0)
					return 0;

				DataUtility.SaveProfileId(customerId, profileId);
			}

			GatewayAuthorizeNet.AuthorizeNetApi.CustomerAddressType cimAddress = DataUtility.GetCustomerAddressFromAddress(addressId);

			if (paymentProfileId <= 0)
			{
				// create new payment profile
				var paymentProfileResponse = profileMgr.CreatePaymentProfile(cimAddress, cardNumber.Trim(), cardCode.Trim(),
					Int32.Parse(expMonth), Int32.Parse(expYear));
				if (paymentProfileResponse == null)
				{
					errorMessage = "Null payment profile response.";
					return 0;
				}
				if (!paymentProfileResponse.Success)
				{
					errorMessage = paymentProfileResponse.ErrorMessage;
					errorCode = paymentProfileResponse.ErrorCode;
					return 0;
				}
				paymentProfileId = paymentProfileResponse.PaymentProfileId;
			}
			else
			{
				// update profile
				profileMgr.UpdatePaymentProfile(paymentProfileId, cimAddress, cardNumber.Trim(), cardCode.Trim(),
					Int32.Parse(expMonth), Int32.Parse(expYear));
			}

			if (paymentProfileId > 0)
			{
				String cardType = DetectCreditCardType(cardNumber.Trim());
				DataUtility.SavePaymentProfile(customerId,
					addressId, paymentProfileId,
					expMonth, expYear,
					cardType != null ? cardType : String.Empty);
			}

			return paymentProfileId;
		}

		public static String DetectCreditCardType(String cardNo)
		{
			cardNo = cardNo.Replace(" ", "").Replace("-", "");
			if (cardNo.Length < 6)
			{
				return null;
			}

			Int32 cardNum = 0;
			Int32.TryParse(cardNo.Substring(0, 2), out cardNum);
			switch (cardNum)
			{
				case 34:
				case 37:
					return "AMEX";
				case 36:
					return "Diners Club";
				case 38:
					return "Carte Blanche";
			};

			if (cardNum >= 51 && cardNum <= 55)
			{
				return "MasterCard";
			}

			cardNum = 0;
			Int32.TryParse(cardNo.Substring(0, 4), out cardNum);
			switch (cardNum)
			{
				case 2014:
				case 2149:
					return "enRoute";
				case 2131:
				case 1800:
					return "JCB";
				case 6011:
					return "Discover";
			};

			cardNum = 0;
			Int32.TryParse(cardNo.Substring(0, 3), out cardNum);
			if (cardNum >= 300 && cardNum <= 305)
			{
				return "Diners Club";
			};

			cardNum = 0;
			Int32.TryParse(cardNo.Substring(0, 1), out cardNum);
			switch (cardNum)
			{
				case 3:
					return "JCB";
				case 4:
					return "Visa";
			}

			return String.Empty;
		}
	}
}
