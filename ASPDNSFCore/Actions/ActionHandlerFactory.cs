// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;

namespace AspDotNetStorefrontCore.Actions
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ActionHandlerFactory
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IActionHandler GetHandler(string name)
        {
            ActionHandlerConfiguration config = ActionConfigurationSection.Current.Handlers[name];
            if (null != config)
            {
                string typeName = config.Type;
                try
                {
                    Type handlerType = Type.GetType(typeName);
                    IActionHandler handler = Activator.CreateInstance(handlerType) as IActionHandler;
                    if (null != handler)
                    {
                        return handler;
                    }
                }
                catch {}
            }

            return null;
        }
    }
}
