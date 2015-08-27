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
    /// Summary for ShippingByTotalByPercent Shipping method
    /// </summary>
    public class CalculateShippingByTotalByPercentShippingCalculation : ShippingCalculation
    {
        /// <summary>
        /// Summary for calculation ShippingByTotalByPercent
        /// </summary>
        /// <returns>Return a collection of ShippingMethod based on the computation of ShippingByTotalByPercent</returns>
        public override ShippingMethodCollection GetShippingMethods(int storeId)
        {
            ShippingMethodCollection availableShippingMethods = new ShippingMethodCollection();

            bool shippingMethodToStateMapIsEmpty = Shipping.ShippingMethodToStateMapIsEmpty();
            bool shippingMethodToCountryMapIsEmpty = Shipping.ShippingMethodToCountryMapIsEmpty();
         
            decimal extraFee = AppLogic.AppConfigUSDecimal("ShippingHandlingExtraFee");

            string shipsql = GenerateShippingMethodsQuery(storeId, false);
            //shipsql.Append("select * from ShippingMethod  with (NOLOCK)  where IsRTShipping=0 ");
            
            //if (!shippingMethodToStateMapIsEmpty && !ThisCustomer.IsRegistered)
            //{
            //    shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToStateMap  with (NOLOCK))");
            //}

            //if (!shippingMethodToStateMapIsEmpty && ThisCustomer.IsRegistered)
            //{
            //    shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToStateMap  with (NOLOCK)  where StateID=" + AppLogic.GetStateID(this.ShippingAddress.State).ToString() + ")");
            //}
            
            //if (!shippingMethodToCountryMapIsEmpty)
            //{
            //    shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToCountryMap  with (NOLOCK)  where CountryID=" + AppLogic.GetCountryID(this.ShippingAddress.Country).ToString() + ")");
            //}
            //shipsql.Append(" order by Displayorder");

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
                            thisMethod.Freight = decimal.Zero;
                            thisMethod.ShippingIsFree = true;
                        }
                        else
                        {
                            decimal freight = Shipping.GetShipByTotalByPercentCharge(thisMethod.Id, SubTotalWithoutDownload); // exclude download items!

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
