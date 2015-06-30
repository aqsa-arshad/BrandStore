// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontCore.Validation;

namespace AspDotNetStorefrontControls.Validators
{
    /// <summary>
    /// Validates the address
    /// </summary>
    public class AddressValidator : BaseValidator
    {
        public AddressTypes AddressType
        {
            get;
            set;
        }
       
        /// <summary>
        /// When overridden in a derived class, this method contains the code to determine whether the value in the input control is valid.
        /// </summary>
        /// <returns>
        /// true if the value in the input control is valid; otherwise, false.
        /// </returns>
        protected override bool EvaluateIsValid()
        {
            if (!AppLogic.AppConfigBool("DisallowShippingToPOBoxes"))
            {
                return true;
            }
            // default to shipping address
            AddressTypes addressType = AddressTypes.Shipping;

            string addressTypeQueryString = CommonLogic.QueryStringCanBeDangerousContent("AddressType");
            if (!CommonLogic.IsStringNullOrEmpty(addressTypeQueryString))
            {
                if (addressTypeQueryString.Trim().ToUpper() == "BILLING" ||
                    addressTypeQueryString.Trim().ToUpper() == "SHIPPING")
                {
                    addressType = (AddressTypes)Enum.Parse(typeof(AddressTypes), addressTypeQueryString, true);
                }
            }

            if (this.AddressType == AddressTypes.Billing || addressType == AddressTypes.Billing)
            {
                return true;
            }

            string address1 = base.GetControlValidationValue(this.ControlToValidate);
            return new POBoxAddressValidator().IsValid(address1);
        }
    }

}
