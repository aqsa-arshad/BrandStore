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
    /// Summary for CalculateShippingByTotalAndZone
    /// </summary>
    public class CalculateShippingByTotalAndZoneShippingCalculation : ShippingCalculation
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
       
        public override ShippingMethodCollection GetShippingMethods(int storeId)
        {
            ShippingMethodCollection availableShippingMethods = new ShippingMethodCollection();
            
            decimal extraFee = AppLogic.AppConfigUSDecimal("ShippingHandlingExtraFee");

            string shipsql = GenerateShippingMethodsQuery(storeId, true);

            ShoppingCart weight = new ShoppingCart(1, ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);
            decimal SubTotalWithoutDownload = this.Cart.SubTotal(true, false, false, true, true, false, 0, true); 

            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
                using (IDataReader reader = DB.GetRS(shipsql.ToString(),dbconn))
                {
                    while (reader.Read())
                    {
                        ShippingMethod thisMethod = new ShippingMethod();
                        thisMethod.Id = DB.RSFieldInt(reader, "ShippingMethodID");
                        thisMethod.Name = DB.RSFieldByLocale(reader, "Name", ThisCustomer.LocaleSetting);
                        thisMethod.IsFree = this.ShippingIsFreeIfIncludedInFreeList && Shipping.ShippingMethodIsInFreeList(thisMethod.Id);

                        if (thisMethod.IsFree)
                        {
                            thisMethod.ShippingIsFree = true;
                            thisMethod.Freight = decimal.Zero;
                        }
                        else
                        {

                            int ZoneID = Shipping.ZoneLookup(this.ShippingAddress.Zip);
                            decimal freight = Shipping.GetShipByTotalAndZoneCharge(thisMethod.Id, SubTotalWithoutDownload, ZoneID); // exclude download items!

                            if (extraFee > System.Decimal.Zero)
                            {
                                freight += extraFee;
                            }
                            else if (freight > System.Decimal.Zero && extraFee > System.Decimal.Zero)
                            {
                                freight += extraFee;

                            }
                            if (freight < 0)
                            {
                                freight = 0;
                            }
                            thisMethod.Freight = freight;
                        }

                        bool include = !(this.ExcludeZeroFreightCosts == true && (thisMethod.Freight == decimal.Zero && !thisMethod.IsFree));

                        if (include)
                        {
                            availableShippingMethods.Add(thisMethod);
                        }
                    }
                }
            
            }
           
            return availableShippingMethods;
        }

    }
}
