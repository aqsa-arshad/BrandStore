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
using AspDotNetStorefrontGateways;
using AspDotNetStorefrontCore;
using System.Net;
using System.IO;

namespace GatewayMoneybookers
{
	class QuickCheckout : IExternalPaymentMethod
	{
		public string RequestUri
		{
			get
			{
				if(AppLogic.AppConfigBool("UseLiveTransactions"))
					return AppLogic.AppConfig("Moneybookers.LiveServer");
				else
					return AppLogic.AppConfig("Moneybookers.TestServer");
			}
		}

		private string GenerateTransactionId(AspDotNetStorefrontCore.ShoppingCart cart, decimal cartTotal)
		{
			return String.Format("{0}{1}{2}{3}", cart.ShipCalcID, cart.ThisCustomer.CustomerGUID, cartTotal, cart.FirstCartItem.ShoppingCartRecordID);
		}

		private string GetEncryptedTransactionId(AspDotNetStorefrontCore.ShoppingCart cart, decimal cartTotal)
		{
			return Security.MungeString(GenerateTransactionId(cart, cartTotal));
		}

		private string DecryptTransactionId(string encryptedTransactionId)
		{
			return Security.UnmungeString(encryptedTransactionId);
		}

		public ExternalPaymentMethodContext BeginCheckout(AspDotNetStorefrontCore.ShoppingCart cart)
		{
			decimal amount = cart.Total(true);
			GatewayMoneybookers.PaymentType paymentType = GatewayMoneybookers.PaymentType.Capture;

			string customerIpAddress = CommonLogic.CustomerIpAddress();
			if(customerIpAddress == "::1")
				customerIpAddress = "127.0.0.1";
			else if(customerIpAddress.Contains(":"))
				throw new Exception("The Moneybookers payment gateway does not support IPv6.");

			// Generate payment request
			GatewayMoneybookers.PaymentRequestBuilder requestBuilder = new GatewayMoneybookers.PaymentRequestBuilder();
			var paymentRequest = requestBuilder.BuildQuickCheckoutRequest(
				GetEncryptedTransactionId(cart, amount),
				cart.ThisCustomer.CustomerID,
				cart.ThisCustomer.EMail,
				cart.ThisCustomer.PrimaryBillingAddress.FirstName,
				cart.ThisCustomer.PrimaryBillingAddress.LastName,
				cart.ThisCustomer.PrimaryBillingAddress.Address1,
				cart.ThisCustomer.PrimaryBillingAddress.City,
				CommonLogic.IIF(String.IsNullOrEmpty(cart.ThisCustomer.PrimaryBillingAddress.State), null, cart.ThisCustomer.PrimaryBillingAddress.State),
				cart.ThisCustomer.PrimaryBillingAddress.Zip,
				cart.ThisCustomer.PrimaryBillingAddress.Country,
				customerIpAddress,
				amount);

			GatewayMoneybookers.PaymentXmlTransformer<GatewayMoneybookers.VirtualAccountPaymentMethod> paymentTransformer = new GatewayMoneybookers.PaymentXmlTransformer<GatewayMoneybookers.VirtualAccountPaymentMethod>();
			var paymentRequestData = paymentTransformer.TransformRequest(paymentRequest, new VirtualAccountPaymentMethodXmlTransformer());

			// Submit request and get response
			string paymentResponseData = null;
			string result = String.Empty;
			int maxAttempts = AppLogic.AppConfigUSInt("GatewayRetries") + 1;

			for(int attemptCount = 0; attemptCount < maxAttempts; attemptCount++)
			{
				HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(RequestUri);
				request.Method = "POST";
				request.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";

				try
				{
					using(var requestStream = request.GetRequestStream())
					{
						using(StreamWriter requestWriter = new StreamWriter(requestStream))
						{
							requestWriter.Write("load={0}", Uri.EscapeDataString(paymentRequestData));
							requestWriter.Close();
						}

						requestStream.Close();
					}

					HttpWebResponse response = (HttpWebResponse)request.GetResponse();
					using(var responseStream = response.GetResponseStream())
					{
						using(StreamReader responseReader = new StreamReader(responseStream))
						{
							paymentResponseData = responseReader.ReadToEnd();
							responseReader.Close();
						}

						responseStream.Close();
					}

					break;
				}
#if DEBUG
				catch(WebException exception)
				{
					using(var responseStream = exception.Response.GetResponseStream())
					{
						using(StreamReader responseReader = new StreamReader(responseStream))
						{
                            result = String.Format("Error calling Skrill (Moneybookers) payment gateway. {0}{1}", exception.Message, responseReader.ReadToEnd());
							responseReader.Close();
						}
						responseStream.Close();
					}
				}
#endif
				catch(Exception exception)
				{
                    result = String.Format("Error calling Skrill (Moneybookers) payment gateway. {0}", exception.Message);
				}
			}

			// Process response
			if(paymentResponseData != null)
			{
				GatewayMoneybookers.PaymentResponse paymentResponse = paymentTransformer.TransformResponse(paymentResponseData);
				result = String.Format("{0} - {1} - {2} - {3}", paymentResponse.Result, paymentResponse.Status, paymentResponse.Reason, paymentResponse.Return);

				if(paymentResponse.Result.ToUpperInvariant() == "ACK")
				{
					return new ExternalPaymentMethodContext(result, paymentResponse.RedirectUrl, paymentResponse.RedirectParameters);
				}
				else
				{
					if(result.Length == 0)
						result = "Unspecified Error";

					return new ExternalPaymentMethodContext(result);
				}
			}
			else
				return new ExternalPaymentMethodContext(result);
		}

		public ExternalPaymentMethodContext ProcessCallback(Dictionary<string, string> responseData)
		{
			try
			{
				GatewayMoneybookers.PaymentXmlTransformer<GatewayMoneybookers.VirtualAccountPaymentMethod> paymentTransformer = new GatewayMoneybookers.PaymentXmlTransformer<GatewayMoneybookers.VirtualAccountPaymentMethod>();
				GatewayMoneybookers.PaymentResponse paymentResponse = paymentTransformer.TransformResponse(responseData["response"]);
				string result = String.Format("{0} - {1} - {2} - {3}", paymentResponse.Result, paymentResponse.Status, paymentResponse.Reason, paymentResponse.Return);

				if(paymentResponse.Result.ToUpperInvariant() != "ACK")
				{
					ErrorMessage errorMessage = new ErrorMessage(AspDotNetStorefrontCore.AppLogic.GetString("checkoutpayment.aspx.35", Customer.Current.SkinID, Customer.Current.LocaleSetting));
					string redirectUrl = String.Format("{0}shoppingcart.aspx?ErrorMsg={1}", AppLogic.GetStoreHTTPLocation(false), errorMessage.MessageId);
					return new ExternalPaymentMethodContext(result, redirectUrl, new Dictionary<string, string>());
				}

				int customerId = Int32.Parse(paymentResponse.SessionId);
				Customer customer = new Customer(customerId);
	
				ShoppingCart cart = new ShoppingCart(customer.SkinID, customer, CartTypeEnum.ShoppingCart, 0, false);

				Address billingAddress = new Address();
				billingAddress.LoadByCustomer(customer.CustomerID, customer.PrimaryBillingAddressID, AddressTypes.Billing);
				billingAddress.ClearCCInfo();
				billingAddress.UpdateDB();

				var expectedTransactionId = GenerateTransactionId(cart, cart.Total(true));
				var receivedTransactionId = DecryptTransactionId(paymentResponse.TransactionId);

				if(receivedTransactionId != expectedTransactionId)
				{
					ErrorMessage errorMessage = new ErrorMessage(AspDotNetStorefrontCore.AppLogic.GetString("checkoutpayment.aspx.38", Customer.Current.SkinID, Customer.Current.LocaleSetting));
					string redirectUrl = String.Format("{0}shoppingcart.aspx?ErrorMsg={1}", AppLogic.GetStoreHTTPLocation(false), errorMessage.MessageId);
					return new ExternalPaymentMethodContext(result, redirectUrl, new Dictionary<string, string>());
				}

				int orderNumber = AppLogic.GetNextOrderNumber();
				String status = Gateway.MakeOrder(Gateway.ro_GWMONEYBOOKERS, AppLogic.TransactionMode(), cart, orderNumber, String.Empty, String.Empty, String.Empty, String.Empty);

				if(status != AppLogic.ro_OK)
					throw new Exception("Gateway processing error: " + result);

				string sql = String.Format("update Orders set AuthorizationResult={0}, PaymentGateway='MoneybookersQuickCheckout', AuthorizationPNREF={1}, TransactionCommand=null, AuthorizedOn=getDate() where OrderNumber={2}",
					DB.SQuote(paymentResponse.Return),
					DB.SQuote(String.Format("AUTH={0}|CAPTURE={0}", paymentResponse.TransactionUniqueId)),
					orderNumber);
				DB.ExecuteSQL(sql);

				Gateway.ProcessOrderAsCaptured(orderNumber);

				return new ExternalPaymentMethodContext(result, AppLogic.GetStoreHTTPLocation(false) + "orderconfirmation.aspx?ordernumber=" + orderNumber, new Dictionary<string, string>());
			}
			catch(Exception exception)
			{
				string result = "Error processing order: " + exception.ToString();
				ErrorMessage errorMessage = new ErrorMessage(AspDotNetStorefrontCore.AppLogic.GetString("checkoutpayment.aspx.35", Customer.Current.SkinID, Customer.Current.LocaleSetting));
				string redirectUrl = String.Format("{0}shoppingcart.aspx?ErrorMsg={1}", AppLogic.GetStoreHTTPLocation(false), errorMessage.MessageId);

				return new ExternalPaymentMethodContext(result, redirectUrl, new Dictionary<string, string>());
			}
		}
	}
}
