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
using AspDotNetStorefrontCore;
using AspDotNetStorefrontCommon;

namespace AspDotNetStorefrontEventHandlers
{
    /// <summary>
    /// Implements methods to intercept application errors and log them appropriately
    /// </summary>
    class OnErrorHandler : IHttpModule
    {
        /// <summary>
        /// Required method responsible for loading the module
        /// </summary>
        /// <param name="app"></param>
        public void Init(HttpApplication app)
        {
            app.Error += new EventHandler(app_Error);
        }

        public void Dispose() { }

        /// <summary>
        /// Fires whenever an unhandled exception in the application occurs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void app_Error(Object sender, EventArgs e)
        {
            //Do not let a database error or something of that nature throw the site into an endless exception loop
            try
            {
                if (AppLogic.AppIsStarted && AppLogic.AppConfigBool("System.ErrorHandlingEnabled"))
                {
                    HttpContext context = HttpContext.Current;

                    AppLogic.Custom_Application_Error(sender, e);

                    if (context.Server.GetLastError() != null)
                    {
                        ExceptionHandlers.ExceptionHandler.Handle(context.Server.GetLastError());
                    }
                }
            }
            catch { }
        }

    }
}
