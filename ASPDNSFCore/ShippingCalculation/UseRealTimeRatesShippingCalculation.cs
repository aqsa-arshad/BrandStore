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
    /// Summary for UseRealTimeRates
    /// </summary>
    public class UseRealTimeRatesShippingCalculation : ShippingCalculation
    {
        /// <summary>
        /// Gets whether this shipping calculation requires postal code
        /// </summary>
        public override bool RequirePostalCode
        {
            get
            {
                return true;
            }
        }
        /// <summary>
        /// Set the info regarding Use Real time rates in a collection
        /// </summary>
        /// <returns>Return a collection of shipping method for  UseRealtimerates based in the computation of Use Real time rates</returns>
        public override ShippingMethodCollection GetShippingMethods(int storeId)
        {
            ShippingMethodCollection availableShippingMethods = new ShippingMethodCollection();
            ShippingMethods s_methods = this.Cart.GetRates(this.ShippingAddress, AppLogic.AppConfigUSDecimal("ShippingHandlingExtraFee"));            
           
            int Count = 0;
            if (s_methods.Count > 0)
            {
                foreach (ShipMethod s_method in s_methods)
                {
                    Count++;
                    ShippingMethod realTimeMethod = (ShippingMethod)s_method;
                    realTimeMethod.IsFree &= this.ShippingIsFreeIfIncludedInFreeList;
                    realTimeMethod.IsRealTime = true;

                    if (Cart.ShippingIsFree)
                    {
                        string FreeShippingMethodIDs = Shipping.GetFreeShippingMethodIDs();

                        if (CommonLogic.IntegerIsInIntegerList(realTimeMethod.Id, FreeShippingMethodIDs))
                        {
                            realTimeMethod.Freight = 0.0M;
                            realTimeMethod.ShippingIsFree = true;
                        }
                    }
                   
                    bool include = !(this.ExcludeZeroFreightCosts == true && realTimeMethod.Freight == decimal.Zero);

                    if (include)
                    {
                        availableShippingMethods.Add(realTimeMethod);
                        // add it to the db
                        if (DB.GetSqlN("select count(*) as N from ShippingMethod  with (NOLOCK)  where IsRTShipping=1 and convert(nvarchar(4000),Name)=" + DB.SQuote(realTimeMethod.Name)) == 0)
                        {
                            DB.ExecuteSQL(String.Format("insert ShippingMethod(Name,IsRTShipping) values({0},1)", DB.SQuote(realTimeMethod.Name)));
                        }
                    }   
                }
            }
            else if (s_methods.ErrorMsg.IndexOf(AppLogic.AppConfig("RTShipping.CallForShippingPrompt")) != -1)
            {
                ShippingMethod onlyCallForPromptShippingMethod = new ShippingMethod();
                onlyCallForPromptShippingMethod.Name = AppLogic.AppConfig("RTShipping.CallForShippingPrompt");
                onlyCallForPromptShippingMethod.IsRealTime = true;
                onlyCallForPromptShippingMethod.Id = -1;
                availableShippingMethods.Add(onlyCallForPromptShippingMethod);
            }
            else
            {
                ShippingMethod noShippingMethodFound = new ShippingMethod();
                noShippingMethodFound.Name = AppLogic.GetString("checkoutshipping.estimator.control.InvalidAddress", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                noShippingMethodFound.IsRealTime = true;
                availableShippingMethods.Add(noShippingMethodFound);
            }

            return availableShippingMethods;
        }

    }
}
