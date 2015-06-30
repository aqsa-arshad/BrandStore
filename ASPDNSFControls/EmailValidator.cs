// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore.Validation;

namespace AspDotNetStorefrontControls
{
	public class EmailValidator : RegularExpressionValidator
	{
		public EmailValidator()
		{
			var emailAddressValidator = new EmailAddressValidator();
			this.ValidationExpression = emailAddressValidator.GetValidationRegExString();
		}
	}
}
