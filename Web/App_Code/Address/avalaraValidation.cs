// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using AspDotNetStorefrontCore;
using System;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for AddressValidation
    /// </summary>
	public partial class AddressValidation
    {
        /// <summary>
        /// Validate Address using Avalara Validation.
        /// </summary>
        /// <param name="EnteredAddress">The address as entered by a customer</param>
        /// <param name="ResultAddress">The resulting validated address</param>
        /// <returns>String,
        /// ro_OK => ResultAddress = EnteredAddress proceed with no further user review,
        /// 'some message' => address requires edit or verification by customer
        /// </returns>
        public String avalaraValidate(Address EnteredAddress, out Address ResultAddress)
        {
            string result = AppLogic.ro_OK;
            ResultAddress = new Address();
            ResultAddress.LoadFromDB(EnteredAddress.AddressID);

            if (EnteredAddress.Country != "United States")
            { // Avalara doesn't validate other countries
                return AppLogic.ro_OK;
            }
            
            Customer thisCustomer = AppLogic.GetCurrentCustomer();
            var AvalaraValidate = new AvaTax();
            result = AvalaraValidate.ValidateAddress(thisCustomer, EnteredAddress, out ResultAddress);
            return result == String.Empty ? AppLogic.ro_OK : result;
        }

	}
}
