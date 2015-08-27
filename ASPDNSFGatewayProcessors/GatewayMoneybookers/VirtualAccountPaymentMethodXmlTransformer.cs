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
	class VirtualAccountPaymentMethodXmlTransformer : IPaymentMethodXmlTransformer<VirtualAccountPaymentMethod>
	{
		public XElement GenerateAccountElement(PaymentMethod method)
		{
			return new XElement("Account",
				method.Holder.ToXElement("Holder"),
				method.Brand.ToXElement("Brand"),
				method.Id.ToXElement("Id"));
		}
	}
}
