// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;

namespace GatewayMoneybookers
{
	class PaymentXmlTransformer<TPaymentMethod>
		where TPaymentMethod : PaymentMethod
	{
		public string TransformRequest(PaymentRequest<TPaymentMethod> paymentRequest, IPaymentMethodXmlTransformer<TPaymentMethod> paymentMethodXmlTransformer)
		{
			XDocument result = new XDocument(
				new XDeclaration("1.0", Encoding.UTF8.WebName, "yes"),
				new XElement("Request",
					new XAttribute("version", "1.0"),
					new XElement("Header",
						new XElement("Security",
							paymentRequest.InstanceId.ToXAttribute("sender"))),
					new XElement("Transaction",
						paymentRequest.TransactionMode.ToXAttribute("mode"),
						paymentRequest.ResponseMode.ToXAttribute("response"),
						paymentRequest.ChannelId.ToXAttribute("channel"),
						new XElement("User",
							paymentRequest.UserLogin.ToXAttribute("login"),
							paymentRequest.UserPassword.ToXAttribute("pwd")),
						new XElement("Identification",
							paymentRequest.TransactionId.ToXElement("TransactionID"),
							paymentRequest.InvoiceId.ToXElement("InvoiceID"),
							paymentRequest.ShopperId.ToXElement("ShopperID"),
							paymentRequest.ReferenceId.ToXElement("ReferenceID")
						),
						GeneratePaymentElement(paymentRequest),
						// Recurrence
						// Job
						GenerateAccountElement(paymentRequest, paymentMethodXmlTransformer),
						GenerateCustomerElement(paymentRequest),
						// Details
						GenerateFrontendElement(paymentRequest),
						GenerateAnalysisElement(paymentRequest)
			)));

			return result.ToString();
		}

		private XElement GeneratePaymentElement(PaymentRequest<TPaymentMethod> paymentRequest)
		{
			return new XElement("Payment",
				paymentRequest.PaymentCode.ToXAttribute("code"),
				new XElement("Presentation",
					paymentRequest.PaymentAmount.ToXElement("Amount"),
					paymentRequest.PaymentCurrency.ToXElement("Currency"),
					paymentRequest.PaymentUsage.ToXElement("Usage")));
		}

		private XElement GenerateAccountElement(PaymentRequest<TPaymentMethod> paymentRequest, IPaymentMethodXmlTransformer<TPaymentMethod> paymentMethodXmlTransformer)
		{
			if(paymentRequest.PaymentCode.PaymentMethod is CreditCardPaymentMethod && (
				paymentRequest.PaymentCode.PaymentType == PaymentType.Capture ||
				paymentRequest.PaymentCode.PaymentType == PaymentType.Reversal ||
				paymentRequest.PaymentCode.PaymentType == PaymentType.Refund))
				return null;

			return paymentMethodXmlTransformer.GenerateAccountElement(paymentRequest.PaymentCode.PaymentMethod);
		}
		
		private XElement GenerateCustomerElement(PaymentRequest<TPaymentMethod> paymentRequest)
		{
			if(paymentRequest.PaymentCode.PaymentType == PaymentType.Capture ||
				paymentRequest.PaymentCode.PaymentType == PaymentType.Reversal ||
				paymentRequest.PaymentCode.PaymentType == PaymentType.Refund)
				return null;

			return new XElement("Customer",
				new XElement("Name",
					paymentRequest.CustomerSalutation.ToXElement("Salutation"),
					paymentRequest.CustomerTitle.ToXElement("Title"),
					paymentRequest.CustomerGivenName.ToXElement("Given"),
					paymentRequest.CustomerFamilyName.ToXElement("Family")),
					paymentRequest.CustomerSex.ToXElement("Sex"),
					paymentRequest.CustomerBirthdate.ToXElement("Birthdate"),
					paymentRequest.CustomerCompany.ToXElement("Company"),
				new XElement("Address",
					paymentRequest.CustomerStreet.ToXElement("Street"),
					paymentRequest.CustomerZip.ToXElement("Zip"),
					paymentRequest.CustomerCity.ToXElement("City"),
					paymentRequest.CustomerState.ToXElement("State"),
					paymentRequest.CustomerCountry.ToXElement("Country")),
				new XElement("Contact",
					paymentRequest.CustomerPhone.ToXElement("Phone"),
					paymentRequest.CustomerMobilePhone.ToXElement("Mobile"),
					paymentRequest.CustomerEmail.ToXElement("Email"),
					paymentRequest.CustomerIp.ToXElement("Ip")));
		}

		private XElement GenerateFrontendElement(PaymentRequest<TPaymentMethod> paymentRequest)
		{
			return new XElement("Frontend",
				paymentRequest.ResponseUrl.ToXElement("ResponseUrl"),
				paymentRequest.SessionId.ToXElement("SessionID"));
		}

		private XElement GenerateAnalysisElement(PaymentRequest<TPaymentMethod> paymentRequest)
		{
			XElement analysisElement = new XElement("Analysis",
				new XElement("Criterion",
					new XAttribute("name", "MONEYBOOKERS_merchant_fields"),
					"platform"),
				new XElement("Criterion",
					new XAttribute("name", "MONEYBOOKERS_platform"),
					"23106369"));

			if(paymentRequest.PaymentCode.PaymentMethod is VirtualAccountPaymentMethod)
			{
				string hideLogin;
				if(AspDotNetStorefrontCore.AppLogic.AppConfigBool("Moneybookers.QuickCheckout.HideLogin"))
					hideLogin = "1";
				else
					hideLogin = "0";

				string storeName = AspDotNetStorefrontCore.AppLogic.AppConfig("StoreName");

				analysisElement.Add(
					new XElement("Criterion",
						new XAttribute("name", "MONEYBOOKERS_payment_methods"),
						"ACC"),	// All card types
					new XElement("Criterion",
						new XAttribute("name", "MONEYBOOKERS_hide_login"),
						hideLogin),
					new XElement("Criterion",
						new XAttribute("name", "MONEYBOOKERS_recipient_description"),
						storeName));
			}

			return analysisElement;
		}

		public PaymentResponse TransformResponse(string paymentResponseData)
		{
			XDocument responseDoc = XDocument.Parse(paymentResponseData);
			ResponseMode responseMode = new ResponseMode(responseDoc.XPathSelectElements("/Response/Transaction").Select(e => e.Attribute("response")).Select(a => a.Value).FirstOrDefault());
			string transactionId = responseDoc.XPathSelectElements("/Response/Transaction/Identification/TransactionID").Select(e => e.Value).FirstOrDefault();
			string transactionUniqueId = responseDoc.XPathSelectElements("/Response/Transaction/Identification/UniqueID").Select(e => e.Value).FirstOrDefault();
			string transactionShortId = responseDoc.XPathSelectElements("/Response/Transaction/Identification/ShortID").Select(e => e.Value).FirstOrDefault();
			string processingCode = responseDoc.XPathSelectElements("/Response/Transaction/Processing").Select(e => e.Attributes("code").FirstOrDefault()).Select(a => a.Value).FirstOrDefault();
			string timestamp = responseDoc.XPathSelectElements("/Response/Transaction/Processing/Timestamp").Select(e => e.Value).FirstOrDefault();
			string result = responseDoc.XPathSelectElements("/Response/Transaction/Processing/Result").Select(e => e.Value).FirstOrDefault();
			string statusCode = responseDoc.XPathSelectElements("/Response/Transaction/Processing/Status").Select(e => e.Attributes("code").FirstOrDefault()).Select(a => a.Value).FirstOrDefault();
			string status = responseDoc.XPathSelectElements("/Response/Transaction/Processing/Status").Select(e => e.Value).FirstOrDefault();
			string reasonCode = responseDoc.XPathSelectElements("/Response/Transaction/Processing/Reason").Select(e => e.Attributes("code").FirstOrDefault()).Select(a => a.Value).FirstOrDefault();
			string reason = responseDoc.XPathSelectElements("/Response/Transaction/Processing/Reason").Select(e => e.Value).FirstOrDefault();
			string returnCode = responseDoc.XPathSelectElements("/Response/Transaction/Processing/Return").Select(e => e.Attributes("code").FirstOrDefault()).Select(a => a.Value).FirstOrDefault();
			string returnValue = responseDoc.XPathSelectElements("/Response/Transaction/Processing/Return").Select(e => e.Value).FirstOrDefault();
			string redirectUrl = responseDoc.XPathSelectElements("/Response/Transaction/Processing/Redirect").Select(e => e.Attributes("url").FirstOrDefault()).Select(a => a.Value).FirstOrDefault();
			var redirectParameters = responseDoc.XPathSelectElements("/Response/Transaction/Processing/Redirect/Parameter").ToDictionary(e => e.Attributes("name").FirstOrDefault().Value, e => e.Value);
			string sessionId = responseDoc.XPathSelectElements("/Response/Transaction/Frontend/SessionID").Select(e => e.Value).FirstOrDefault();

			PaymentResponse response = new PaymentResponse(
				responseMode,
				transactionId,
				transactionUniqueId,
				transactionShortId,
				processingCode,
				DateTime.Parse(timestamp),
				result,
				statusCode,
				status,
				reasonCode,
				reason,
				returnCode,
				returnValue,
				redirectUrl,
				redirectParameters,
				sessionId);

			return response;
		}
	}
}
