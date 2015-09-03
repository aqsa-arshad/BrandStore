// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;

namespace AspDotNetStorefrontEventHandlers.ExceptionHandlers.Formatters
{
    public interface IErrorFormatter
    {
        void Prepare(string errorCode, Exception source);
        string Error { get; }
    }
}
