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
		private const string ValidationRegularExpression = @"^[a-zA-Z0-9][-\w\.\+]*@([a-zA-Z0-9][\w\-]*\.)+[a-zA-Z]{2,4}$";

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
        private const string ValidationRegularExpression = @"^\s*(?:\+?(\d{1,3}))?[-. (]*(\d{3})[-. )]*(\d{3})[-. ]*(\d{4})(?: *x(\d+))?\s*$";

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
