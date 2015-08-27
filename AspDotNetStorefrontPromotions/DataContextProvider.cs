// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
#region Using Directives

using System;
using System.Configuration;
using System.Collections.Generic;
using System.Web;
using System.Collections;

#endregion

namespace AspDotNetStorefront.Promotions.Data
{
	public static class DataContextProvider
	{
		#region Properties

        public static EntityContextDataContext Current
		{
			get
			{
				if (ContextStorageController.Current == null)
					ContextStorageController.Current = new HttpContextStorage();

				if (ContextStorageController.Current["PromotionsDataContext"] == null)
                    ContextStorageController.Current["PromotionsDataContext"] = new EntityContextDataContext(ConfigurationManager.AppSettings["DBConn"]);

                return (EntityContextDataContext)ContextStorageController.Current["PromotionsDataContext"];
			}
		}

        public static EntityContextDataContext New()
		{
            return new EntityContextDataContext(ConfigurationManager.AppSettings["DBConn"]);
		}

		#endregion
	}
}
