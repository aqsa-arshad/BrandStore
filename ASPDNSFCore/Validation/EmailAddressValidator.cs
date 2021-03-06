// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Text.RegularExpressions;

namespace AspDotNetStorefrontCore.Validation
{
	public class EmailAddressValidator
	{
        private const string ValidationRegularExpression = @"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";

		public string GetValidationRegExString()
		{
			return ValidationRegularExpression;
		}

		public bool IsValidEmailAddress(string email)
		{
			return string.IsNullOrEmpty(email) ? false : Regex.IsMatch(email, ValidationRegularExpression, RegexOptions.IgnoreCase);
		}
	}

    public class PhoneValidator
    {
        private const string ValidationRegularExpression = @"\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})";

        public string GetValidationRegExString()
        {
            return ValidationRegularExpression;
        }

        public bool IsValidPhone(string phone)
        {
            return string.IsNullOrEmpty(phone) ? false : Regex.IsMatch(phone, ValidationRegularExpression, RegexOptions.IgnoreCase);
        }
    }
}
