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
    /// Checks whether the I am over 13 years old field is checked
    /// </summary>
    public class Over13Validator : BaseValidator
    {

        public Over13Validator()
        {
        }

        protected override bool ControlPropertiesValid()
        {
            return true;
        }

        private Control m_controlinstancetovalidate;

        public Control ControlInstanceToValidate
        {
            get { return m_controlinstancetovalidate; }
            set { m_controlinstancetovalidate = value; }
        }

        /// <summary>
        /// When overridden in a derived class, this method contains the code to determine whether the value in the input control is valid.
        /// </summary>
        /// <returns>
        /// true if the value in the input control is valid; otherwise, false.
        /// </returns>
        protected override bool EvaluateIsValid()
        {
            CheckBox chk = this.ControlInstanceToValidate as CheckBox;
            Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;

            if (AppLogic.AppConfigBool("RequireOver13Checked"))
            {
                if (chk.Checked)
                {
                    return true;
                }
                else
                {
                    ErrorMessage = AppLogic.GetString("Over13OnCreateAccount", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                    return false;
                }
            }
            else
            {
                return true;
            }
        }
    }
}
