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
	class PaymentType : IPaymentType
	{
		public static PaymentType Preauthorisation = new PaymentType("Preauthorisation", "PA");
		public static PaymentType Debit = new PaymentType("Debit", "DB");
		public static PaymentType Capture = new PaymentType("Capture", "CP");
		public static PaymentType Credit = new PaymentType("Credit", "CD");
		public static PaymentType Reversal = new PaymentType("Reversal", "RV");
		public static PaymentType Refund = new PaymentType("Refund", "RF");
		public static PaymentType Rebill = new PaymentType("Rebill", "RB");
		public static PaymentType Chargeback = new PaymentType("Chargeback", "CB");
		public static PaymentType Receipt = new PaymentType("Receipt", "RC");
		public static PaymentType Registration = new PaymentType("Registration", "RG");
		public static PaymentType Reregistration = new PaymentType("Reregistration", "RR");
		public static PaymentType Deregistration = new PaymentType("Deregistration", "DR");
		public static PaymentType Confirmation = new PaymentType("Confirmation", "CR");
		public static PaymentType Schedule = new PaymentType("Schedule", "SD");
		public static PaymentType Reschedule = new PaymentType("Reschedule", "RS");
		public static PaymentType Deschedule = new PaymentType("Deschedule", "DS");

		public string Name { get; protected set; }
		public string Code { get; protected set; }

		public PaymentType(string name, string code)
		{
			if(name == null)
				throw new ArgumentNullException("name");

			if(code == null)
				throw new ArgumentNullException("code");

			Name = name;
			Code = code;
		}

		public override bool Equals(object obj)
		{
			if(!(obj is PaymentType) || obj == null)
				return false;

			return Code == ((PaymentType)obj).Code;
		}

		public override int GetHashCode()
		{
			return Code.GetHashCode();
		}

		public static bool operator ==(PaymentType a, IPaymentType b)
		{
			if(Object.ReferenceEquals(a, b))
				return true;

			if(((object)a == null) || ((object)b == null))
				return false;

			return a.Code == b.Code;
		}

		public static bool operator !=(PaymentType a, IPaymentType b)
		{
			return !(a == b);
		}
	}
}
