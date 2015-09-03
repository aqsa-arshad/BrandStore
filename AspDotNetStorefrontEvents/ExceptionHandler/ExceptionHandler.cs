// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using AspDotNetStorefrontEventHandlers.ExceptionHandlers.Formatters;
using AspDotNetStorefrontEventHandlers.ExceptionHandlers.Publishers;
using System.Web;

using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontEventHandlers.ExceptionHandlers
{
    
    public static class ExceptionHandler
    {

        public static void Handle(Exception ex)
        {
            bool errorLoggingEnabled = AppLogic.AppConfigBool("System.LoggingEnabled");
            bool friendlyErrors = AppLogic.AppConfigBool("System.ShowFriendlyErrors");

            // check if we're just checking a 404 error message
            // if such, we just redirect the user to our custom topic page
            if (ex is HttpException)
            {
                HttpException pageNotFoundException = ex as HttpException;
                if (pageNotFoundException.GetHttpCode() == 404)
                {
                    // just return here instead of redirecting to PageNotFound.aspx
                    // need to let IIS handle the 404 redirect configured in the web.config
                    // so we don't lose the aspxerrorpath querystring parameter or we can't
                    // make 404 suggestions
                    return;
                }
            }

            // Make sure the exception is not being thrown on InvalidRequest.aspx to prevent loops
            try
            {
                if (CommonLogic.GetThisPageName(false).IndexOf("invalidrequest.aspx", StringComparison.InvariantCultureIgnoreCase) != -1)
                {
                    return;
                }
            }
            catch
            {
                return;
            }

            string errorCode = Guid.NewGuid().ToString("N").Substring(0, 7).ToUpper();

            if (errorLoggingEnabled)
            {
                string configuredPublishers = AppLogic.AppConfig("System.LoggingLocation");

                if (CommonLogic.IsStringNullOrEmpty(configuredPublishers)) return;

                bool publishInFile = CommonLogic.StringInCommaDelimitedStringList("file", configuredPublishers);
                bool publishInEmail = CommonLogic.StringInCommaDelimitedStringList("email", configuredPublishers);
                bool publishInEventLog = CommonLogic.StringInCommaDelimitedStringList("eventLog", configuredPublishers);
                bool publishInDatabase = CommonLogic.StringInCommaDelimitedStringList("database", configuredPublishers);

                if ((publishInFile || publishInEmail || publishInEventLog || publishInDatabase) == false) return;

                // generate a new error code for reference

                IErrorFormatter formatter = new TextErrorFormatter();
                formatter.Prepare(errorCode, ex);

                List<IExceptionPublisher> publishers = new List<IExceptionPublisher>();

                if (publishInFile)
                {
                    publishers.Add(new FileBasedExceptionPublisher());
                }
                if (publishInEmail)
                {
                    publishers.Add(new EmailExceptionPublisher());
                }
                if (publishInEventLog)
                {
                    publishers.Add(new EventLogExceptionPublisher());
                }

                // if an error occurs be silent about it
                // since these are the ones supposed to send the notification!
                foreach (IExceptionPublisher publisher in publishers)
                {
                    try
                    {
                        publisher.Publish(errorCode, formatter.Error);
                    }
                    catch { }
                }

                if (publishInDatabase)
                {
                    SysLog.LogException(ex, MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
                }
            }

            Customer thisCustomer = ((AspDotNetStorefrontPrincipal)HttpContext.Current.User).ThisCustomer;

            // Now, notify the user on the webpage
            HttpContext ctx = HttpContext.Current;
            
            // Rewrites the error message using a friendly XML package
            if (friendlyErrors)
            {
                ctx.Response.Clear();
                ctx.Server.ClearError();

                HttpContext.Current.Response.Redirect("InvalidRequest.aspx");                
            }

            //ctx.Response.End();
            ctx.Server.ClearError();
        }
    }
}
