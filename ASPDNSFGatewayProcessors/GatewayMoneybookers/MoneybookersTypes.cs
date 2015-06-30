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
using AspDotNetStorefrontCore;

namespace GatewayMoneybookers
{
	class AccountHolder : RestrictedLengthString
	{
		public const int MinLength = 4;
		public const int MaxLength = 128;

		public AccountHolder(string value)
			: base(MinLength, MaxLength, value)
		{ }
	}

	class AccountNumber : RestrictedLengthString
	{
		public const int MinLength = 3;
		public const int MaxLength = 64;

		public AccountNumber(string value)
			: base(MinLength, MaxLength, value)
		{ }
	}

	class AccountBrand : RestrictedLengthString
	{
		public const int MinLength = 3;
		public const int MaxLength = 12;		// Docs say max length is 10, but Quick Checkout docs say to use a 12-character longs string

		public AccountBrand(string value)
			: base(MinLength, MaxLength, value)
		{ }
	}

	class AccountId : RestrictedLengthString
	{
		public const int MinLength = 3;
		public const int MaxLength = 128;

		public AccountId(string value)
			: base(MinLength, MaxLength, value)
		{ }
	}

	class AccountExpiryMonth : RestrictedLengthString
	{
		public const int MinLength = 2;
		public const int MaxLength = 2;

		public AccountExpiryMonth(string value)
			: base(MinLength, MaxLength, value)
		{ }
	}

	class AccountExpiryYear : RestrictedLengthString
	{
		public const int MinLength = 4;
		public const int MaxLength = 4;

		public AccountExpiryYear(string value)
			: base(MinLength, MaxLength, value)
		{ }
	}

	class AccountCardIssueNumber : RestrictedLengthString
	{
		public const int MinLength = 1;
		public const int MaxLength = 2;

		public AccountCardIssueNumber(string value)
			: base(MinLength, MaxLength, value)
		{ }
	}

	class AccountVerification : RestrictedLengthString
	{
		public const int MinLength = 3;
		public const int MaxLength = 4;

		public AccountVerification(string value)
			: base(MinLength, MaxLength, value)
		{ }
	}

	class TransactionMode
	{
		public static TransactionMode IntegratorTest = new TransactionMode("INTEGRATOR_TEST");
		public static TransactionMode ConnectorTest = new TransactionMode("CONNECTOR_TEST");
		public static TransactionMode Live = new TransactionMode("LIVE");

		public string Mode { get; protected set; }

		private TransactionMode(string mode)
		{
			if(String.IsNullOrEmpty(mode))
				throw new ArgumentNullException("moneybookers.exception.transactionmode");

			Mode = mode;
		}

		public override string ToString()
		{
			return Mode;
		}
	}

	class ResponseMode
	{
		public static ResponseMode Sync = new ResponseMode("SYNC");
		public static ResponseMode Async = new ResponseMode("ASYNC");

		public string Mode { get; protected set; }

		public ResponseMode(string mode)
		{
			if(String.IsNullOrEmpty(mode))
                throw new ArgumentNullException("moneybookers.exception.responsemode");

			Mode = mode;
		}

		public override string ToString()
		{
			return Mode;
		}

		public override bool Equals(object obj)
		{
			if(!(obj is ResponseMode) || obj == null)
				return false;

			return Mode == ((ResponseMode)obj).Mode;
		}

		public override int GetHashCode()
		{
			return Mode.GetHashCode();
		}

		public static bool operator ==(ResponseMode a, ResponseMode b)
		{
			if(Object.ReferenceEquals(a, b))
				return true;

			if(((object)a == null) || ((object)b == null))
				return false;

			return a.Mode.Equals(b.Mode, StringComparison.InvariantCultureIgnoreCase);
		}

		public static bool operator !=(ResponseMode a, ResponseMode b)
		{
			return !(a == b);
		}
	}

	class PaymentCode
	{
		public PaymentMethod PaymentMethod { get; protected set; }
		public IPaymentType PaymentType { get; protected set; }

		public PaymentCode(PaymentMethod paymentMethod, IPaymentType paymentType)
		{
			if(paymentMethod == null)
                throw new ArgumentNullException("paymentMethod", "moneybookers.exception.nullmethod");

			if(paymentType == null)
                throw new ArgumentNullException("paymentType", "moneybookers.exception.nulltype");

			PaymentMethod = paymentMethod;
			PaymentType = paymentType;
		}

		public override string ToString()
		{
			return String.Format("{0}.{1}", PaymentMethod.Code, PaymentType.Code);
		}
	}

	class Amount
	{
		public decimal Value { get; protected set; }

		public Amount(decimal value)
		{
			if(value >= 10000000000 || value <= -100000000)
                throw new FormatException("moneybookers.exception.invalidamount");

			Value = value;
		}

		public override string ToString()
		{
			return Value.ToString("0.00");
		}
	}

	class Date
	{
		public DateTime Value { get; protected set; }

		public Date(DateTime value)
		{
			Value = value;
		}

		public override string ToString()
		{
			return String.Format("yyyy-MM-dd", Value);
		}
	}

	class UniqueIdentifier : RestrictedLengthString
	{
		public const int MinLength = 32;
		public const int MaxLength = 32;

		public UniqueIdentifier(string value)
			: base(MinLength, MaxLength, value)
		{ }
	}

	class LimitedString : RestrictedLengthString
	{
		public const int MinLength = 0;
		public const int MaxLength = 256;

		public LimitedString(string value)
			: base(MinLength, MaxLength, value)
		{ }
	}

	class UserPassword : RestrictedLengthString
	{
		public const int MinLength = 4;		// The documentation says the min length is 5, but the test account password is 4
		public const int MaxLength = 32;

		public UserPassword(string value)
			: base(MinLength, MaxLength, value)
		{ }
	}

	class Currency : RestrictedLengthString
	{
		public const int MinLength = 3;
		public const int MaxLength = 3;

		public Currency(string value)
			: base(MinLength, MaxLength, value)
		{ }
	}

	class Name : RestrictedLengthString
	{
		public const int MinLength = 2;
		public const int MaxLength = 40;

		public Name(string value)
			: base(MinLength, MaxLength, value)
		{ }
	}

	class Prefix : RestrictedLengthString
	{
		public const int MinLength = 1;
		public const int MaxLength = 20;

		public Prefix(string value)
			: base(MinLength, MaxLength, value)
		{ }
	}

	class Sex : RestrictedLengthString
	{
		public const int MinLength = 1;
		public const int MaxLength = 1;

		public Sex(string value)
			: base(MinLength, MaxLength, value)
		{ }
	}

	class Street : RestrictedLengthString
	{
		public const int MinLength = 5;
		public const int MaxLength = 50;

		public Street(string value)
			: base(MinLength, MaxLength, value)
		{ }
	}

	class Zip : RestrictedLengthString
	{
		public const int MinLength = 1;
		public const int MaxLength = 10;

		public Zip(string value)
			: base(MinLength, MaxLength, value)
		{ }
	}

	class City : RestrictedLengthString
	{
		public const int MinLength = 2;
		public const int MaxLength = 30;

		public City(string value)
			: base(MinLength, MaxLength, value)
		{ }
	}

	class State : RestrictedLengthString
	{
		public const int MinLength = 2;
		public const int MaxLength = 8;

		public State(string value)
			: base(MinLength, MaxLength, value)
		{ }
	}

	class CountryCode : RestrictedLengthString
	{
		public const int MinLength = 2;
		public const int MaxLength = 2;

		public CountryCode(string value)
			: base(MinLength, MaxLength, value)
		{ }
	}

	class PhoneNumber : RestrictedLengthString
	{
		public const int MinLength = 8;
		public const int MaxLength = 64;

		public PhoneNumber(string value)
			: base(MinLength, MaxLength, value)
		{ }
	}

	class MobilePhoneNumber : RestrictedLengthString
	{
		public const int MinLength = 10;
		public const int MaxLength = 64;

		public MobilePhoneNumber(string value)
			: base(MinLength, MaxLength, value)
		{ }
	}

	class EmailAddress : RestrictedLengthString
	{
		public const int MinLength = 6;
		public const int MaxLength = 128;

		public EmailAddress(string value)
			: base(MinLength, MaxLength, value)
		{ }
	}

	class IpAddress : RestrictedLengthString
	{
		public const int MinLength = 15;
		public const int MaxLength = 15;

		public IpAddress(string value)
			: base(MinLength, MaxLength, value)
		{ }
	}

	class RestrictedLengthString
	{
		public string Value { get; protected set; }

		public RestrictedLengthString(int minLength, int maxLength, string value)
		{
			if(minLength < 0)
                throw new ArgumentOutOfRangeException("moneybookers.exception.minlength");

			if(minLength > maxLength)
                throw new ArgumentOutOfRangeException("moneybookers.exception.maxlength");

			if(value == null)
                throw new ArgumentNullException("value", "moneybookers.exception.nullvalue");

			if(value.Length < minLength || value.Length > maxLength)
				if(minLength == maxLength)
                    throw new FormatException("moneybookers.exception.exact" + GetType().Name.ToLower());
				else
                    throw new FormatException("moneybookers.exception." + GetType().Name.ToLower());

			Value = value;
		}

		public override string ToString()
		{
			return Value;
		}
	}
}
