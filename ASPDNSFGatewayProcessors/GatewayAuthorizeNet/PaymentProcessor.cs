// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using System.Reflection;
using GatewayAuthorizeNet.AuthorizeNetApi;

namespace GatewayAuthorizeNet
{	
    class PaymentProcessor
    {
		public CIMResponse Authorize(Int64 profileId, Int64 paymentProfileId, int orderNumber, decimal amount)
		{
			ServiceProcessContext serviceCtx = new ServiceProcessContext();

			var orderType = ServiceTools.CreateOrderExType(orderNumber.ToString(), string.Empty, string.Empty);
			var authTransactionType = ServiceTools.CreateProfileTransAuthOnlyType(profileId, paymentProfileId, orderType, amount);
			var transactionType = ServiceTools.CreateProfileTransactionType(authTransactionType);

			var transactionResponse = serviceCtx.Service.CreateCustomerProfileTransaction(serviceCtx.MerchantAuthenticationType, transactionType, string.Empty);

			Trace.WriteLine(transactionResponse.directResponse);

			foreach (var message in transactionResponse.messages)
				Trace.WriteLine(string.Format("{0}: {1}", message.code, message.text));

            if (transactionResponse.directResponse == null)
            {
                //if directResponse is empty, the first message will say why
                return new CIMResponse()
                {
                    AuthMessage = transactionResponse.messages[0].text,
                    Success = false
                };
            }
            else
            {
                return ServiceTools.ParseDirectResponse(transactionResponse.directResponse);
            }
		}

		public CIMResponse AuthCapture(Int64 profileId, Int64 paymentProfileId, int orderNumber, decimal amount)
		{
			ServiceProcessContext serviceCtx = new ServiceProcessContext();

			var orderType = ServiceTools.CreateOrderExType(orderNumber.ToString(), string.Empty, string.Empty);
			var authTransactionType = ServiceTools.CreateProfileTransAuthCaptureType(profileId, paymentProfileId, orderType, amount);
			var transactionType = ServiceTools.CreateProfileTransactionType(authTransactionType);

			var transactionResponse = serviceCtx.Service.CreateCustomerProfileTransaction(serviceCtx.MerchantAuthenticationType, transactionType, string.Empty);

			Trace.WriteLine(transactionResponse.directResponse);
			
			foreach (var message in transactionResponse.messages)
				Trace.WriteLine(string.Format("{0}: {1}", message.code, message.text));

            if (transactionResponse.directResponse == null)
            {
                //if directResponse is empty, the first message will say why
                return new CIMResponse()
                {
                    AuthMessage = transactionResponse.messages[0].text,
                    Success = false
                };
            }
            else
            {
                return ServiceTools.ParseDirectResponse(transactionResponse.directResponse);
            }
		}

		public CIMResponse Capture(Int64 profileId, Int64 paymentProfileId, string authCode, decimal amount)
		{
			ServiceProcessContext serviceCtx = new ServiceProcessContext();

			var profileTransactionType = ServiceTools.CreateProfileTransCaptureOnlyType(profileId, paymentProfileId, authCode, amount);
			var transactionType = ServiceTools.CreateProfileTransactionType(profileTransactionType);

			var transactionResponse = serviceCtx.Service.CreateCustomerProfileTransaction(serviceCtx.MerchantAuthenticationType, transactionType, string.Empty);

			Trace.WriteLine(transactionResponse.directResponse);

			foreach (var message in transactionResponse.messages)
				Trace.WriteLine(string.Format("{0}: {1}", message.code, message.text));

			return ServiceTools.ParseDirectResponse(transactionResponse.directResponse);
		}

		public CIMResponse Refund(Int64 profileId, Int64 paymentProfileId, string transId, decimal amount)
		{
			ServiceProcessContext serviceCtx = new ServiceProcessContext();

			var profileTransactionType = ServiceTools.CreateProfileTransRefundType(profileId, paymentProfileId, transId, amount);
			var transactionType = ServiceTools.CreateProfileTransactionType(profileTransactionType);

			var transactionResponse = serviceCtx.Service.CreateCustomerProfileTransaction(serviceCtx.MerchantAuthenticationType, transactionType, string.Empty);

			Trace.WriteLine(transactionResponse.directResponse);

			foreach (var message in transactionResponse.messages)
				Trace.WriteLine(string.Format("{0}: {1}", message.code, message.text));

			return ServiceTools.ParseDirectResponse(transactionResponse.directResponse);
		}

		public CIMResponse Void(Int64 profileId, Int64 paymentProfileId, string transactionId)
		{
			ServiceProcessContext serviceCtx = new ServiceProcessContext();

			var profileTransactionType = ServiceTools.CreateProfileTransVoidType(profileId, paymentProfileId, transactionId);
			var transactionType = ServiceTools.CreateProfileTransactionType(profileTransactionType);

			var transactionResponse = serviceCtx.Service
				.CreateCustomerProfileTransaction(serviceCtx.MerchantAuthenticationType, transactionType, string.Empty);

			Trace.WriteLine(transactionResponse.directResponse);

			foreach (var message in transactionResponse.messages)
				Trace.WriteLine(string.Format("{0}: {1}", message.code, message.text));

			return ServiceTools.ParseDirectResponse(transactionResponse.directResponse);
		}		
    }
}
