// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;

namespace AspDotNetStorefrontCore.ShippingCalculation
{    
    /// <summary>
    /// Summary for AllOrdersHaveFreeShipping
    /// </summary>
    public class AllOrdersHaveFreeShippingShippingCalculation : ShippingCalculation
    {
        
        /// <summary>
        /// Summary for AllOrdersHaveFreeShipping
        /// </summary>
        /// <returns>Return FREE SHIPPING  as freight name in a collection</returns>
        public override ShippingMethodCollection GetShippingMethods(int storeId)
        {
            ShippingMethodCollection availableShippingMethods = new ShippingMethodCollection();
            ShippingMethod thisMethod = new ShippingMethod();

            thisMethod.Name = "FREE SHIPPING (All Orders Have Free Shipping)";
            availableShippingMethods.Add(thisMethod);
            return availableShippingMethods;
        }
    }
}
