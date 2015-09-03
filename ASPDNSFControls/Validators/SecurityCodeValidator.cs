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
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontControls.Validators
{
    /// <summary>
    /// Checks if the security code entered is correct
    /// </summary>
    public class SecurityCodeValidator : BaseValidator
    {
        private TextBox _SecurityCode = new TextBox();

        /// <summary>
        /// Gets or sets the security code.
        /// </summary>
        /// <value>The security code.</value>
        public string SecurityCode
        {
            get { return _SecurityCode.Text; }
            set { _SecurityCode.Text = value; }
        }

        /// <summary>
        /// When overridden in a derived class, this method contains the code to determine whether the value in the input control is valid.
        /// </summary>
        /// <returns>
        /// true if the value in the input control is valid; otherwise, false.
        /// </returns>
        protected override bool EvaluateIsValid()
        {
            ErrorMessage =  AppLogic.GetString("createaccount_process.aspx.2", 1, Localization.GetDefaultLocale());
            return (_SecurityCode.Text.Trim() == CommonLogic.SessionNotServerFarmSafe("SecurityCode"));
        }
    }
}
