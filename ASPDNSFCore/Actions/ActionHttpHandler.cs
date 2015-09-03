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

namespace AspDotNetStorefrontCore.Actions
{
    public class ActionHttpHandler : IHttpHandler
    {
        #region IHttpHandler Members

        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            string action = context.Request.QueryString["action"];
            if (!CommonLogic.IsStringNullOrEmpty(action))
            {
                IActionHandler handler = ActionHandlerFactory.GetHandler(action);
                if (null != handler)
                {
                    handler.Handle(context);
                }
            }
        }

        #endregion
    }
}
