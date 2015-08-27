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
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontControls.Validators
{
    /// <summary>
    /// Validates the Zip code
    /// </summary>
    public class ZipCodeValidator : BaseValidator
    {
        private int m_countryid;

        public int CountryID
        {
            get { return m_countryid; }
            set { m_countryid = value; }
        }
        
        /// <summary>
        /// When overridden in a derived class, this method contains the code to determine whether the value in the input control is valid.
        /// </summary>
        /// <returns>
        /// true if the value in the input control is valid; otherwise, false.
        /// </returns>
        protected override bool EvaluateIsValid()
        {
            Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;
            string zipCode = base.GetControlValidationValue(this.ControlToValidate);
            bool Success = false;
            if (zipCode.Length == 0)
            {
                ErrorMessage = AppLogic.GetString("address.cs.18", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
            }
            else
            {

                Success = AppLogic.ValidatePostalCode(zipCode, CountryID);

                if (!Success)
                {
                    ErrorMessage = AppLogic.GetCountryPostalErrorMessage(CountryID, ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                }
            }
            
            return Success;
        }
    }
}
