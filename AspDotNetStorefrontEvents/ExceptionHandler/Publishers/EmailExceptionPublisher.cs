// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;

using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontEventHandlers.ExceptionHandlers.Publishers
{
    internal class EmailExceptionPublisher : ExceptionPublisher
    {
        public override void Publish(string errorCode, string error)
        {
            string sendToThisEmailAddress = AppLogic.AppConfig("MailMe_ErrorToAddress");
            string sendToThisEmailName = AppLogic.AppConfig("MailMe_ErrorToName");
            string sendFromThisEmail = AppLogic.AppConfig("MailMe_ErrorFromAddress");
            string sendFromThisEmailName = AppLogic.AppConfig("MailMe_ErrorFromName");

            if (CommonLogic.IsStringNullOrEmpty(sendToThisEmailAddress))
            {
                sendToThisEmailAddress = AppLogic.AppConfig("MailMe_ToAddress");
            }
            if (CommonLogic.IsStringNullOrEmpty(sendToThisEmailName))
            {
                sendToThisEmailName = AppLogic.AppConfig("MailMe_ToName");
            }
            if (CommonLogic.IsStringNullOrEmpty(sendFromThisEmail))
            {
                sendFromThisEmail = AppLogic.AppConfig("MailMe_FromAddress");
            }
            if (CommonLogic.IsStringNullOrEmpty(sendFromThisEmail))
            {
                sendFromThisEmail = AppLogic.AppConfig("MailMe_FromName");
            }

            AppLogic.SendMail(string.Format("Error on Site {0} (Error Code:{1})", AppLogic.AppConfig("StoreName"), errorCode),
                error,
                false,
                sendFromThisEmail,
                sendToThisEmailName,
                sendToThisEmailAddress,
                sendToThisEmailName,
                String.Empty,
                AppLogic.AppConfig("MailMe_Server"));
        }
    }
}
