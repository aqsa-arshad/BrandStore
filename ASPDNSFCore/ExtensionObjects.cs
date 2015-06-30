// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Web;

namespace AspDotNetStorefrontCore
{
    public class ExtensionObjects
    {
        public ExtensionObjects()
        {
            
        }

        public static object CreateExtension(string TypeName, Customer cust, int SkinID, System.Collections.Generic.Dictionary<string, EntityHelper> EntityHelpers)
        {
            Type objType = null;
            objType = ((Type)(HttpRuntime.Cache.Get(TypeName)));

            if (objType == null)
            {
               
                    objType = System.Web.Compilation.BuildManager.GetType(TypeName, true);
                    HttpRuntime.Cache.Add(TypeName, objType, null, DateTime.MaxValue, new TimeSpan(0,5,0) , System.Web.Caching.CacheItemPriority.Default, null);
              
            }
            Object[] args = { cust, SkinID, EntityHelpers };

            Object obj;
            if (objType.IsSubclassOf(typeof(XSLTExtensionBase)))
            {
                obj = Activator.CreateInstance(objType, args);
            }
            else
            {
                obj = Activator.CreateInstance(objType);
            }

            return obj;
        }
    }
}
