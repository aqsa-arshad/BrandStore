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
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Threading;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontControls
{
    
    public class BaseControl : System.Web.UI.UserControl
    {
        
        public Customer ThisCustomer
        {
            get { return Customer.Current; }
        }

        public virtual void ShowError(string errorMessage, bool isError)
        {
            if (this.Page is ISplashHost)
            {
                (this.Page as ISplashHost).ShowError(errorMessage, isError);
            }
        }
    }
}
