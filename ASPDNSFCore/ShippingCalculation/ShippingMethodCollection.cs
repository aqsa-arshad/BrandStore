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
    /// Summary for shiping method collection
    /// </summary>
    public class ShippingMethodCollection : List<ShippingMethod>
    {
        public static explicit operator ShippingMethods(ShippingMethodCollection col)
        {
            ShippingMethods SM = new ShippingMethods();
            foreach(ShipMethod meth in col)
            {
                SM.AddMethod(meth);
            }
            return SM; 
        }
       /// <summary>
       /// Get the lowest freight in a collection
       /// </summary>
        public ShippingMethod LowestFreight
        {
            get
            {
                if (this.Count > 0)
                {
                    ShippingMethod lowestFreightMethod = this[0];
                    foreach (ShippingMethod thisMethod in this)
                    {
                        
                        if (thisMethod.Freight < lowestFreightMethod.Freight && thisMethod.Freight >= 0)
                        {
                            lowestFreightMethod = thisMethod;
                        }
                    }

                    return lowestFreightMethod;
                }

                return null;
            }
        }
    }   
}

