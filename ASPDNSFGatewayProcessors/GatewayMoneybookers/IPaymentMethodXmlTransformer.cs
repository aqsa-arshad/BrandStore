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
	interface IPaymentMethodXmlTransformer<TPaymentMethod>
		where TPaymentMethod : PaymentMethod
	{
		XElement GenerateAccountElement(PaymentMethod method);
	}
}
