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
	public class CIMResponse
	{
		public long PaymentProfileId { get; set; }
		public bool Success { get; set; }
		public string AuthCode { get; set; }
		public string AuthMessage { get; set; }
		public string TransactionId { get; set; }
		public string ErrorMessage { get; set; }
		public string ErrorCode { get; set; }
		public string AvsCode { get; set; }
	}
}
