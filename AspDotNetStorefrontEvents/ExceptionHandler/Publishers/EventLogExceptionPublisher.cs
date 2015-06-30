// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace AspDotNetStorefrontEventHandlers.ExceptionHandlers.Publishers
{
    public class EventLogExceptionPublisher : ExceptionPublisher
    {
        public override void Publish(string errorCode, string error)
        {
            EventLog eventLog = new EventLog();
            eventLog.Source = "Application";
            eventLog.WriteEntry(error, EventLogEntryType.Error);
        }
    }
}
