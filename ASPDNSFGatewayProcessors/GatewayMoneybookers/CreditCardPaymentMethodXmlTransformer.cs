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
using System.Xml.Linq;

namespace GatewayMoneybookers
{
	class CreditCardPaymentMethodXmlTransformer : IPaymentMethodXmlTransformer<CreditCardPaymentMethod>
	{
		public XElement GenerateAccountElement(PaymentMethod method)
		{
			return new XElement("Account",
				method.Holder.ToXElement("Holder"),
				method.Number.ToXElement("Number"),
				method.Brand.ToXElement("Brand"),
				new XElement("Expiry",
					method.ExpiryMonth.ToXAttribute("month"),
					method.ExpiryYear.ToXAttribute("year")),
				method.CardIssueNumber.ToXElement("CardIssueNumber"),
				method.Verification.ToXElement("Verification"));
		}
	}
}
