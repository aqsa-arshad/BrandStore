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
    /// Summary for ShippingByWeightAndZone
    /// </summary>
    public class CalculateShippingByWeightAndZoneShippingCalculation : ShippingCalculation
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
        /// Represent the calculation ShippingByWeightAndZone
        /// </summary>
        /// <returns>return collection of shipping method based on the computation of ShippingByWeightAndZone </returns>
        public override ShippingMethodCollection GetShippingMethods(int storeId)
        {
            ShippingMethodCollection availableShippingMethods = new ShippingMethodCollection();

            bool shippingMethodToStateMapIsEmpty = Shipping.ShippingMethodToStateMapIsEmpty();
            bool shippingMethodToCountryMapIsEmpty = Shipping.ShippingMethodToCountryMapIsEmpty();
            bool shippingMethodToZoneMapIsEmpty = Shipping.ShippingMethodToZoneMapIsEmpty();
            
            decimal extraFee = AppLogic.AppConfigUSDecimal("ShippingHandlingExtraFee");

            string shipsql = GenerateShippingMethodsQuery(storeId, true);

            ShoppingCart weight = new ShoppingCart(1, ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);

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
                            decimal freight = Shipping.GetShipByWeightAndZoneCharge(thisMethod.Id, weight.WeightTotal(), ZoneID); // exclude download items!

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
