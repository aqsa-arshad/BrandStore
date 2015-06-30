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

namespace GatewayMoneybookers
{
	class PaymentResponse
	{
		public ResponseMode ResponseMode { get; protected set; }
		public string TransactionId { get; protected set; }
		public string TransactionUniqueId { get; protected set; }
		public string TransactionShortId { get; protected set; }
		public string ProcessingCode { get; protected set; }
		public DateTime Timestamp { get; protected set; }
		public string Result { get; protected set; }
		public string StatusCode { get; protected set; }
		public string Status { get; protected set; }
		public string ReasonCode { get; protected set; }
		public string Reason { get; protected set; }
		public string ReturnCode { get; protected set; }
		public string Return { get; protected set; }
		public string RedirectUrl { get; protected set; }
		public Dictionary<string, string> RedirectParameters { get; protected set; }
		public string SessionId { get; protected set; }

		public PaymentResponse(
			ResponseMode responseMode,
			string transactionId,
			string transactionUniqueId,
			string transactionShortId,
			string processingCode,
			DateTime timestamp,
			string result,
			string statusCode,
			string status,
			string reasonCode,
			string reason,
			string returnCode,
			string returnValue,
			string redirectUrl,
			Dictionary<string, string> redirectParameters,
			string sessionId)
		{
			ResponseMode = responseMode;
			TransactionId = transactionId;
			TransactionUniqueId = transactionUniqueId;
			TransactionShortId = transactionShortId;
			ProcessingCode = processingCode;
			Timestamp = timestamp;
			Result = result;
			StatusCode = statusCode;
			Status = status;
			ReasonCode = reasonCode;
			Reason = reason;
			ReturnCode = returnCode;
			Return = returnValue;
			RedirectUrl = redirectUrl;
			RedirectParameters = redirectParameters;
			SessionId = sessionId;
		}
	}
}
