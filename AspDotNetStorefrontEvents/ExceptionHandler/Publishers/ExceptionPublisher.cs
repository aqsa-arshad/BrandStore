// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using AspDotNetStorefrontCommon;

namespace AspDotNetStorefrontEventHandlers.ExceptionHandlers.Publishers
{
    public abstract class ExceptionPublisher : IExceptionPublisher
    {
        public abstract void Publish(string errorCode, string source);
    }
}
