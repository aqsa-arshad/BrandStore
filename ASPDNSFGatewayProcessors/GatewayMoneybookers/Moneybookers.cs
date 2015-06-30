// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using AspDotNetStorefrontCore;
using System.Collections.Generic;

namespace AspDotNetStorefrontGateways.Processors
{
	public class Moneybookers : GatewayProcessor, AspDotNetStorefrontGateways.IExternalPaymentMethodProvider
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
        public override string AdministratorSetupPrompt
        {
            get
            {
                return "<a href='http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=skrill&type=learnmore' target='_blank'>Learn More...</a>";
            }
        }


		public string ThreeDSecureRequestUri
		{
			get
			{
				if(AppLogic.AppConfigBool("UseLiveTransactions"))
					return AppLogic.AppConfig("Moneybookers.LiveServer.3DSecure");
				else
					return AppLogic.AppConfig("Moneybookers.TestServer.3DSecure");
			}
		}

		public override String CaptureOrder(Order order)
		{
			decimal amount = order.Total(true); ;
			GatewayMoneybookers.PaymentType paymentType = GatewayMoneybookers.PaymentType.Capture;

			// Generate payment request
			GatewayMoneybookers.PaymentRequestBuilder requestBuilder = new GatewayMoneybookers.PaymentRequestBuilder();
			var paymentRequest = requestBuilder.BuildReferencedPaymentRequest<GatewayMoneybookers.CreditCardPaymentMethod>(
				paymentType,
				order.OrderNumber,
				order.AuthorizationPNREF.Substring(5),	// Trim off "AUTH="
				amount);

			GatewayMoneybookers.PaymentXmlTransformer<GatewayMoneybookers.CreditCardPaymentMethod> paymentTransformer = new GatewayMoneybookers.PaymentXmlTransformer<GatewayMoneybookers.CreditCardPaymentMethod>();
			var paymentRequestData = paymentTransformer.TransformRequest(paymentRequest, null);

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
				order.CaptureTXCommand = paymentRequestData;
				order.CaptureTXResult = paymentResponseData;

				if(paymentResponse.Result.ToUpperInvariant() == "ACK")
				{
					order.AuthorizationPNREF = order.AuthorizationPNREF + "|CAPTURE=" + paymentResponse.TransactionUniqueId;
					result = AppLogic.ro_OK;
				}
				else
				{
					result = String.Format("{0} - {1} - {2} - {3}", paymentResponse.Result, paymentResponse.Status, paymentResponse.Reason, paymentResponse.Return);
					if(result.Length == 0)
					{
						result = "Unspecified Error";
					}
				}
			}

			return result;
		}

		public override String VoidOrder(int orderNumber)
		{
			Order order = new Order(orderNumber);
			decimal amount = order.Total(true); ;
			GatewayMoneybookers.PaymentType paymentType = GatewayMoneybookers.PaymentType.Reversal;

			// Generate payment request
			GatewayMoneybookers.PaymentRequestBuilder requestBuilder = new GatewayMoneybookers.PaymentRequestBuilder();
			var paymentRequest = requestBuilder.BuildReferencedPaymentRequest<GatewayMoneybookers.CreditCardPaymentMethod>(
				paymentType,
				order.OrderNumber,
				order.AuthorizationPNREF.Substring(5),
				amount);

			GatewayMoneybookers.PaymentXmlTransformer<GatewayMoneybookers.CreditCardPaymentMethod> paymentTransformer = new GatewayMoneybookers.PaymentXmlTransformer<GatewayMoneybookers.CreditCardPaymentMethod>();
			var paymentRequestData = paymentTransformer.TransformRequest(paymentRequest, null);

			// Submit request and get response
			string paymentResponseData = null;
			string result = String.Empty;
			int maxAttempts = AppLogic.AppConfigUSInt("GatewayRetries") + 1;

			DB.ExecuteSQL("update orders set VoidTXCommand=" + DB.SQuote(paymentRequestData) + " where OrderNumber=" + orderNumber.ToString());

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
				DB.ExecuteSQL("update orders set VoidTXResult=" + DB.SQuote(paymentResponseData) + " where OrderNumber=" + orderNumber.ToString());

				if(paymentResponse.Result.ToUpperInvariant() == "ACK")
				{
					result = AppLogic.ro_OK;
				}
				else
				{
					result = String.Format("{0} - {1} - {2} - {3}", paymentResponse.Result, paymentResponse.Status, paymentResponse.Reason, paymentResponse.Return);
					if(result.Length == 0)
					{
						result = "Unspecified Error";
					}
				}
			}

			return result;
		}

		public override String RefundOrder(int originalOrderNumber, int newOrderNumber, decimal refundAmount, String refundReason, Address useBillingAddress)
		{
			Order order = new Order(originalOrderNumber);

			if(order.PaymentMethod.ToUpper() == AppLogic.ro_PMMoneybookersQuickCheckout)
				return RefundOrder<GatewayMoneybookers.VirtualAccountPaymentMethod>(order, originalOrderNumber, newOrderNumber, refundAmount, refundReason, useBillingAddress, new GatewayMoneybookers.VirtualAccountPaymentMethodXmlTransformer());
			else
				return RefundOrder<GatewayMoneybookers.CreditCardPaymentMethod>(order, originalOrderNumber, newOrderNumber, refundAmount, refundReason, useBillingAddress, new GatewayMoneybookers.CreditCardPaymentMethodXmlTransformer());
		}

		private String RefundOrder<TPaymentMethod>(Order order, int originalOrderNumber, int newOrderNumber, decimal refundAmount, String refundReason, Address useBillingAddress, GatewayMoneybookers.IPaymentMethodXmlTransformer<TPaymentMethod> paymentMethodXmlTransformer)
			where TPaymentMethod : GatewayMoneybookers.PaymentMethod
		{
			decimal amount = refundAmount;
			GatewayMoneybookers.PaymentType paymentType = GatewayMoneybookers.PaymentType.Refund;

			DB.ExecuteSQL("update orders set RefundTXCommand=NULL, RefundTXResult=NULL where OrderNumber=" + originalOrderNumber.ToString());

			// Generate payment request
			GatewayMoneybookers.PaymentRequestBuilder requestBuilder = new GatewayMoneybookers.PaymentRequestBuilder();
			var paymentRequest = requestBuilder.BuildReferencedPaymentRequest<TPaymentMethod>(
				paymentType,
				order.OrderNumber,
				order.AuthorizationPNREF.Substring(order.AuthorizationPNREF.IndexOf("|CAPTURE=") + 9),
				amount);

			GatewayMoneybookers.PaymentXmlTransformer<TPaymentMethod> paymentTransformer = new GatewayMoneybookers.PaymentXmlTransformer<TPaymentMethod>();
			var paymentRequestData = paymentTransformer.TransformRequest(paymentRequest, paymentMethodXmlTransformer);

			// Submit request and get response
			string paymentResponseData = null;
			string result = String.Empty;
			int maxAttempts = AppLogic.AppConfigUSInt("GatewayRetries") + 1;

	        DB.ExecuteSQL("update orders set RefundTXCommand=" + DB.SQuote(paymentRequestData) + " where OrderNumber=" + originalOrderNumber.ToString());

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
				DB.ExecuteSQL("update orders set RefundTXResult=" + DB.SQuote(paymentResponseData)  + " where OrderNumber=" + originalOrderNumber.ToString());

				if(paymentResponse.Result.ToUpperInvariant() == "ACK")
				{
					result = AppLogic.ro_OK;
					
					// We can't record the transaction ID because the field is not long enough
					//DB.ExecuteSQL("update orders set AuthorizationPNREF=AuthorizationPNREF+" + DB.SQuote("|REFUND=" + paymentResponse.TransactionUniqueId) + " where OrderNumber=" + originalOrderNumber.ToString());
				}
				else
				{
					result = String.Format("{0} - {1} - {2} - {3}", paymentResponse.Result, paymentResponse.Status, paymentResponse.Reason, paymentResponse.Return);
					if(result.Length == 0)
					{
						result = "Unspecified Error";
					}
				}
			}

			return result;
		}

		public override String ProcessCard(int OrderNumber, int CustomerID, Decimal OrderTotal, bool useLiveTransactions, TransactionModeEnum TransactionMode, Address UseBillingAddress, String CardExtraCode, Address UseShippingAddress, String CAVV, String ECI, String XID, out String AVSResult, out String AuthorizationResult, out String AuthorizationCode, out String AuthorizationTransID, out String TransactionCommandOut, out String TransactionResponse)
		{
			String AVSAddr = String.Empty;
			String AVSZip = String.Empty;
			AuthorizationCode = String.Empty;
			AuthorizationResult = String.Empty;
			AuthorizationTransID = String.Empty;
			AVSResult = String.Empty;
			TransactionCommandOut = String.Empty;
			TransactionResponse = String.Empty;

			// Generate payment request
			Customer customer = new Customer(CustomerID, true);

			GatewayMoneybookers.IPaymentType paymentType;
			if(TransactionMode == TransactionModeEnum.auth)
				paymentType = GatewayMoneybookers.PaymentType.Preauthorisation;
			else
				paymentType = GatewayMoneybookers.PaymentType.Debit;

			string customerIpAddress = CommonLogic.CustomerIpAddress();

			if(customerIpAddress == "::1")
				customerIpAddress = "127.0.0.1";
			else if(customerIpAddress.Contains(":"))
                throw new Exception("The Skrill (Moneybookers) payment gateway does not support IPv6.");

			string cardType = UseBillingAddress.CardType;
			if(cardType.ToUpper() == "MASTERCARD")
				cardType = "MASTER";

			string result;
			CustomerSession customerSession = new CustomerSession(CustomerID);
			
			if(customerSession.SessionUSInt("Moneybookers_3DSecure_OrderNumber") == OrderNumber)
				result = ProcessOrderThrough3DSecure(OrderNumber, CustomerID, OrderTotal, TransactionMode, UseBillingAddress, CardExtraCode, ref AuthorizationResult, ref AuthorizationCode, ref AuthorizationTransID, ref TransactionCommandOut, ref TransactionResponse, customer, paymentType, customerIpAddress, cardType, customerSession);
			else
				result = ProcessOrderThroughGateway(OrderNumber, CustomerID, OrderTotal, TransactionMode, UseBillingAddress, CardExtraCode, ref AuthorizationResult, ref AuthorizationCode, ref AuthorizationTransID, ref TransactionCommandOut, ref TransactionResponse, customer, paymentType, customerIpAddress, cardType);

            return result;
		}

		private string ProcessOrderThrough3DSecure(int OrderNumber, int CustomerID, Decimal OrderTotal, TransactionModeEnum TransactionMode, Address UseBillingAddress, String CardExtraCode, ref String AuthorizationResult, ref String AuthorizationCode, ref String AuthorizationTransID, ref String TransactionCommandOut, ref String TransactionResponse, Customer customer, GatewayMoneybookers.IPaymentType paymentType, string customerIpAddress, string cardType, CustomerSession customerSession)
		{
			string originalResponseData = customerSession["Moneybookers_3DSecure_Response"];
			var paymentXmlTransformer = new GatewayMoneybookers.PaymentXmlTransformer<GatewayMoneybookers.CreditCardPaymentMethod>();
            var paymentResponse = paymentXmlTransformer.TransformResponse(originalResponseData);

			AuthorizationCode = paymentResponse.ProcessingCode;
			AuthorizationResult = paymentResponse.Return;
			AuthorizationTransID = CommonLogic.IIF(TransactionMode == TransactionModeEnum.auth, "AUTH=", "CAPTURE=") + paymentResponse.TransactionUniqueId;

			TransactionResponse = originalResponseData;

			// Check for timeout
			if(paymentResponse.ReturnCode == "100.380.501")
				return "3D Secure payment verification timed out.";

			// Verify 3D secure response
			string verificationRequestUri = ThreeDSecureRequestUri + customerSession["Moneybookers_3DSecure_VerificationPath"];
			
			// Submit request and get response
			string responseData = null;
			string result = String.Empty;
			int maxAttempts = AppLogic.AppConfigUSInt("GatewayRetries") + 1;

			for(int attemptCount = 0; attemptCount < maxAttempts; attemptCount++)
			{
				HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(verificationRequestUri);
				request.Method = "POST";
				request.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";

				try
				{
					using(var requestStream = request.GetRequestStream())
					{
						using(StreamWriter requestWriter = new StreamWriter(requestStream))
						{
							requestWriter.Write("response={0}", Uri.EscapeDataString(originalResponseData));
							requestWriter.Close();
						}

						requestStream.Close();
					}

					HttpWebResponse response = (HttpWebResponse)request.GetResponse();
					using(var responseStream = response.GetResponseStream())
					{
						using(StreamReader responseReader = new StreamReader(responseStream))
						{
							responseData = responseReader.ReadToEnd();
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
			if(String.IsNullOrEmpty(responseData))
				result = "Empty response from 3D Secure validation call.";
			else if(responseData == "VERIFIED")
				result = AppLogic.ro_OK;
			else
				result = "Payment method was not validated. Please try submitting your payment again";

			return result;
		}

		private string ProcessOrderThroughGateway(int OrderNumber, int CustomerID, Decimal OrderTotal, TransactionModeEnum TransactionMode, Address UseBillingAddress, String CardExtraCode, ref String AuthorizationResult, ref String AuthorizationCode, ref String AuthorizationTransID, ref String TransactionCommandOut, ref String TransactionResponse, Customer customer, GatewayMoneybookers.IPaymentType paymentType, string customerIpAddress, string cardType)
		{
			GatewayMoneybookers.PaymentRequestBuilder requestBuilder = new GatewayMoneybookers.PaymentRequestBuilder();
			var paymentRequest = requestBuilder.BuildCreditCardPaymentRequest(
				paymentType,
				OrderNumber,
				null,
				CustomerID,
				customer.EMail,
				UseBillingAddress.FirstName,
				UseBillingAddress.LastName,
				UseBillingAddress.Address1,
				UseBillingAddress.City,
				CommonLogic.IIF(String.IsNullOrEmpty(UseBillingAddress.State), null, UseBillingAddress.State),
				UseBillingAddress.Zip,
				UseBillingAddress.Country,
				customerIpAddress,
				OrderTotal,
				cardType,
				UseBillingAddress.CardNumber,
				UseBillingAddress.CardExpirationMonth.PadLeft(2, '0'),
				UseBillingAddress.CardExpirationYear,
				CommonLogic.IIF(String.IsNullOrEmpty(UseBillingAddress.CardIssueNumber), null, UseBillingAddress.CardIssueNumber),
				CardExtraCode);

			GatewayMoneybookers.PaymentXmlTransformer<GatewayMoneybookers.CreditCardPaymentMethod> paymentTransformer = new GatewayMoneybookers.PaymentXmlTransformer<GatewayMoneybookers.CreditCardPaymentMethod>();
			GatewayMoneybookers.CreditCardPaymentMethodXmlTransformer paymentMethodTransformer = new GatewayMoneybookers.CreditCardPaymentMethodXmlTransformer();
			var paymentRequestData = paymentTransformer.TransformRequest(paymentRequest, paymentMethodTransformer);

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
							result = String.Format("Error calling Moneybookers payment gateway. {0}{1}", exception.Message, responseReader.ReadToEnd());
							responseReader.Close();
						}
						responseStream.Close();
					}
				}
#endif
				catch(Exception exception)
				{
					result = String.Format("Error calling Moneybookers payment gateway. {0}", exception.Message);
				}
			}

			// Process response
			if(paymentResponseData != null)
			{
				GatewayMoneybookers.PaymentResponse paymentResponse = paymentTransformer.TransformResponse(paymentResponseData);
				AuthorizationCode = paymentResponse.ProcessingCode;
				AuthorizationResult = paymentResponse.Return;
				AuthorizationTransID = CommonLogic.IIF(TransactionMode == TransactionModeEnum.auth, "AUTH=", "CAPTURE=") + paymentResponse.TransactionUniqueId;

				TransactionCommandOut = paymentRequestData;
				TransactionResponse = paymentResponseData;

				if(paymentResponse.Result.ToUpperInvariant() == "ACK")
				{
					if(paymentResponse.ResponseMode == GatewayMoneybookers.ResponseMode.Async)
					{
						string redirectUrl = paymentResponse.RedirectUrl;

						string parameterKeys = String.Join(";", paymentResponse.RedirectParameters.Keys.Select(s => "Moneybookers_3DSecure_Parameter_" + s).ToArray());

						CustomerSession customerSession = new CustomerSession(CustomerID);
						customerSession["Moneybookers_3DSecure_OrderNumber"] = OrderNumber.ToString();
						customerSession["Moneybookers_3DSecure_RedirectUrl"] = paymentResponse.RedirectUrl;
						customerSession["Moneybookers_3DSecure_ParameterKeys"] = parameterKeys;

						foreach(var kvp in paymentResponse.RedirectParameters)
							customerSession["Moneybookers_3DSecure_Parameter_" + kvp.Key] = kvp.Value;

						result = AppLogic.ro_3DSecure;
					}
					else
						result = AppLogic.ro_OK;
				}
				else
				{
					result = String.Format("{0} - {1} - {2} - {3}", paymentResponse.Result, paymentResponse.Status, paymentResponse.Reason, paymentResponse.Return);
					if(result.Length == 0)
					{
						result = "Unspecified Error";
					}
				}
			}

			return result;
		}

		public ExternalPaymentMethodContext Process3DSecureResponse(Dictionary<string, string> responseData)
		{
			try
			{
				if(responseData == null || responseData["response"] == null)
				{
					var errorMessage = new ErrorMessage(System.Web.HttpUtility.HtmlEncode(AppLogic.GetString("secureprocess.aspx.1", Customer.Current.SkinID, Customer.Current.LocaleSetting)));
					string redirectUrl = "checkoutpayment.aspx?error=1&errormsg=" + errorMessage.MessageId;
					return new ExternalPaymentMethodContext("No response data", redirectUrl, new Dictionary<string, string>());
				}

				GatewayMoneybookers.PaymentXmlTransformer<GatewayMoneybookers.CreditCardPaymentMethod> paymentTransformer = new GatewayMoneybookers.PaymentXmlTransformer<GatewayMoneybookers.CreditCardPaymentMethod>();
				GatewayMoneybookers.PaymentResponse paymentResponse = paymentTransformer.TransformResponse(responseData["response"]);
				string result = String.Format("{0} - {1} - {2} - {3}", paymentResponse.Result, paymentResponse.Status, paymentResponse.Reason, paymentResponse.Return);

				int customerId = Int32.Parse(paymentResponse.SessionId);
				Customer customer = new Customer(customerId, true);
				customer.RequireCustomerRecord();

				ShoppingCart cart = new ShoppingCart(customer.SkinID, customer, CartTypeEnum.ShoppingCart, 0, false);
				if(cart.IsEmpty())
				{
					string redirectUrl = "shoppingcart.aspx";
					return new ExternalPaymentMethodContext("Shopping cart empty", redirectUrl, new Dictionary<string, string>());
				}

				int orderNumber = customer.ThisCustomerSession.SessionUSInt("Moneybookers_3DSecure_OrderNumber");
				if(orderNumber == 0)
				{
					var errorMessage = new ErrorMessage(System.Web.HttpUtility.HtmlEncode(AppLogic.GetString("secureprocess.aspx.1", Customer.Current.SkinID, Customer.Current.LocaleSetting)));
					string redirectUrl = "checkoutpayment.aspx?error=1&errormsg=" + errorMessage.MessageId;
					return new ExternalPaymentMethodContext(result, redirectUrl, new Dictionary<string, string>());
				}

				if(paymentResponse.Result.ToUpperInvariant() != "ACK")
				{
					var errorMessage = new ErrorMessage(System.Web.HttpUtility.HtmlEncode(AppLogic.GetString("secureprocess.aspx.3", Customer.Current.SkinID, Customer.Current.LocaleSetting)));
					string redirectUrl = "checkoutpayment.aspx?error=1&errormsg=" + errorMessage.MessageId;
					return new ExternalPaymentMethodContext(result, redirectUrl, new Dictionary<string, string>());
				}

				CustomerSession customerSession = new CustomerSession(customerId);
				customerSession["Moneybookers_3DSecure_Approved"] = Boolean.TrueString;
				customerSession["Moneybookers_3DSecure_Response"] = responseData["response"];
				customerSession["Moneybookers_3DSecure_VerificationPath"] = responseData["threedsecure_verificationpath"];

				string status = Gateway.MakeOrder(String.Empty, AppLogic.TransactionMode(), cart, orderNumber, String.Empty, String.Empty, String.Empty, String.Empty);

				if (status != AppLogic.ro_OK)
                {
					var errorMessage = new ErrorMessage(System.Web.HttpUtility.HtmlEncode(status));
					string redirectUrl = "checkoutpayment.aspx?error=1&errormsg=" + errorMessage.MessageId;
					return new ExternalPaymentMethodContext(result, redirectUrl, new Dictionary<string, string>());
				}

				CustomerSession.StaticClear(customer.CustomerID);

				return new ExternalPaymentMethodContext(result, AppLogic.GetStoreHTTPLocation(false) + "orderconfirmation.aspx?paymentmethod=Credit+Card&ordernumber=" + orderNumber, new Dictionary<string, string>());
			}
			catch(Exception exception)
			{
				string result = "Error processing order: " + exception.ToString();
				ErrorMessage errorMessage = new ErrorMessage(AspDotNetStorefrontCore.AppLogic.GetString("checkoutpayment.aspx.35", Customer.Current.SkinID, Customer.Current.LocaleSetting));
				string redirectUrl = String.Format("{0}shoppingcart.aspx?error=1&ErrorMsg={1}", AppLogic.GetStoreHTTPLocation(false), errorMessage.MessageId);

				return new ExternalPaymentMethodContext(result, redirectUrl, new Dictionary<string, string>());
			}
		}

		public IExternalPaymentMethod GetExternalPaymentMethod(string name)
		{
			if(name == "Quick Checkout")
				return new GatewayMoneybookers.QuickCheckout();

			return null;
		}
	}
}
