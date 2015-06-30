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
using System.Web;
using System.Diagnostics;
using System.Reflection;
using GatewayAuthorizeNet.AuthorizeNetApi;

namespace GatewayAuthorizeNet
{
	public class ProfileManager
	{
		public long ProfileId { get; protected set; }
		public int CustomerId { get; protected set; }
		public string Email { get; protected set; }

		public ValidationModeEnum ValidationMode
		{ get { return AspDotNetStorefrontCore.AppLogic.AppConfigBool("AUTHORIZENET_CIM_UseSandbox") ? ValidationModeEnum.testMode : ValidationModeEnum.liveMode; } }

		public ProfileManager(int customerId, string email, long profileId)
		{
			if(profileId <= 0)
				throw new ArgumentException("Profile ID is empty");

			this.Email = email;
			this.CustomerId = customerId;
			this.ProfileId = profileId;
		}

		public static ProfileManager CreateProfile(int customerId, string email, string profileDescription, out string errorMessage)
		{
			errorMessage = string.Empty;

			ServiceProcessContext serviceCtx = new ServiceProcessContext();
			var customerProfile = ServiceTools.CreateCustomerProfileType(customerId, email, profileDescription);
			var response = serviceCtx.Service.CreateCustomerProfile(serviceCtx.MerchantAuthenticationType, customerProfile, ValidationModeEnum.none);

			ProfileManager profile = null;

            //If we have a duplicate customer record try to delete it and re-create
            //This should solve issues caused by auth.net accounts being switched for the site
            if (response.resultCode == MessageTypeEnum.Error && response.messages[0].code.ToUpper() == "E00039")
            {
                //if we have a duplicate account try to get the account profile from the error message
                string[] strAry = response.messages[0].text.Split(' ');
                foreach (string str in strAry)
                {
                    long custProfId;
                    if(long.TryParse(str, out custProfId))
                    {
                        //if we have an existing profile then delete it and start over
                        profile = new ProfileManager(customerId, email, custProfId);
                        if (profile.DeleteProfile())
                            response = serviceCtx.Service.CreateCustomerProfile(serviceCtx.MerchantAuthenticationType, customerProfile, ValidationModeEnum.none);
                        
                        //reset our profile for normal processing below
                        profile = null;
                    }
                }
            }

			if(response.resultCode == MessageTypeEnum.Ok)
				profile = new ProfileManager(customerId, email, response.customerProfileId);
			else if(response.resultCode == MessageTypeEnum.Error)
				errorMessage = response.messages[0].text;

			return profile;
		}

		public CustomerProfileMaskedType GetProfile()
		{
			ServiceProcessContext serviceCtx = new ServiceProcessContext();

			var response = serviceCtx.Service.GetCustomerProfile(serviceCtx.MerchantAuthenticationType, this.ProfileId);
			if (response.resultCode == MessageTypeEnum.Error)
				return null;

			return response.profile;
		}
		
		public bool UpdateProfile(string newEmail, string profileDescription)
		{
			ServiceProcessContext serviceCtx = new ServiceProcessContext();

			var customerProfile = ServiceTools.CreateCustomerProfileExType(this.ProfileId, this.CustomerId, newEmail, profileDescription);
			var response = serviceCtx.Service.UpdateCustomerProfile(serviceCtx.MerchantAuthenticationType, customerProfile);
			
			return response.resultCode == MessageTypeEnum.Ok;
		}

		public bool DeleteProfile()
		{
			ServiceProcessContext serviceCtx = new ServiceProcessContext();
			var response = serviceCtx.Service.DeleteCustomerProfile(serviceCtx.MerchantAuthenticationType, this.ProfileId);

			return response.resultCode == MessageTypeEnum.Ok;
		}

		public CIMResponse CreatePaymentProfile(CustomerAddressType address, string creditCardNumber, string cardCode, Int32 expMonth, Int32 expYear)
		{
			ServiceProcessContext serviceCtx = new ServiceProcessContext();

			string expDate = ServiceTools.GetAuthNetExpirationDate(expMonth, expYear);
			var paymentType = ServiceTools.CreatePaymentType(creditCardNumber, cardCode, expDate);
			var paymentProfileType = ServiceTools.CreatePaymentProfileType(address, paymentType);

			var response = serviceCtx.Service.CreateCustomerPaymentProfile(serviceCtx.MerchantAuthenticationType, this.ProfileId, paymentProfileType, ValidationMode);

			var retResponse = ServiceTools.ParseDirectResponse(response.validationDirectResponse);
			if (response.resultCode == MessageTypeEnum.Error)
			{
				retResponse.ErrorMessage = response.messages[0].text;
				retResponse.ErrorCode = response.messages[0].code;
				retResponse.Success = false;
			}
			retResponse.PaymentProfileId = response.customerPaymentProfileId;
			return retResponse;
		}

		public CustomerPaymentProfileMaskedType GetPaymentProfile(long paymentProfileId)
		{
			ServiceProcessContext serviceCtx = new ServiceProcessContext();

			var response = serviceCtx.Service.GetCustomerPaymentProfile(serviceCtx.MerchantAuthenticationType, this.ProfileId, paymentProfileId);
			if (response.resultCode == MessageTypeEnum.Error)
				return null;

			return response.paymentProfile;
		}

		public CIMResponse UpdatePaymentProfile(long paymentProfileId, CustomerAddressType address, string creditCardNumber, string cardCode, Int32 expMonth, Int32 expYear)
		{
			ServiceProcessContext serviceCtx = new ServiceProcessContext();

			string expDate = ServiceTools.GetAuthNetExpirationDate(expMonth, expYear);
			var paymentType = ServiceTools.CreatePaymentType(creditCardNumber, cardCode, expDate);
			var paymentProfileType = ServiceTools.CreatePaymentProfileExType(paymentProfileId, address, paymentType);

			var response = serviceCtx.Service.UpdateCustomerPaymentProfile(serviceCtx.MerchantAuthenticationType, this.ProfileId, paymentProfileType, ValidationMode);

			return ServiceTools.ParseDirectResponse(response.validationDirectResponse);
		}

		public bool DeletePaymentProfile(long paymentProfileId)
		{
			ServiceProcessContext serviceCtx = new ServiceProcessContext();
			var response = serviceCtx.Service.DeleteCustomerPaymentProfile(serviceCtx.MerchantAuthenticationType,
				this.ProfileId,
				paymentProfileId);

			return response.resultCode == MessageTypeEnum.Ok;
		}
	}
}
