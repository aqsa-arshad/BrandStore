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
    /// Base clas for ShippingCalulations
    /// </summary>
    public abstract partial class ShippingCalculation : IShippingCalculation
    {
        #region Variable Declaration
        
        private Customer _thisCustomer = null;
        private Address _shippingAddress = null;
        private ShoppingCart _cart = null;
        private decimal _taxRate = decimal.Zero;
        private bool _shippingIsFreeIfIncludedInFreeList = false;
        private bool _excludeZeroFreightCosts = false;
        private decimal _handlingExtraFee = decimal.Zero;

        #endregion

        #region Properties

        public Customer ThisCustomer
        {
            get { return _thisCustomer; }
            set { _thisCustomer = value; }
        }

        public Address ShippingAddress
        {
            get { return _shippingAddress; }
            set { _shippingAddress = value; }
        }

        public ShoppingCart Cart
        {
            get { return _cart; }
            set { _cart = value; }
        }

        public decimal TaxRate
        {
            get { return _taxRate; }
            set { _taxRate = value; }
        }

        public bool ShippingIsFreeIfIncludedInFreeList
        {
            get { return _shippingIsFreeIfIncludedInFreeList; }
            set { _shippingIsFreeIfIncludedInFreeList = value; }
        }

        public decimal HandlingExtraFee
        {
            get { return _handlingExtraFee; }
            set { _handlingExtraFee = value; }
        }

        public bool ExcludeZeroFreightCosts
        {
            get { return _excludeZeroFreightCosts; }
            set { _excludeZeroFreightCosts = value; }
        }

        /// <summary>
        /// Gets whether this shipping calculation requires postal code
        /// </summary>
        public virtual bool RequirePostalCode
        {
            get { return false; }
        }

        #endregion

        #region Methods

        ///// <summary>
        ///// Generates the shipping method query multi-store aware
        ///// </summary>
        ///// <returns></returns>
        //protected string GetShippingMethodsQuery()
        //{
        //    return GetShippingMethodsQuery(false);
        //}

        ///// <summary>
        ///// Generates the shipping method query multi-store aware
        ///// </summary>
        ///// <param name="includeZone">Whether to include store mapping</param>
        ///// <returns></returns>
        //protected string GenerateShippingMethodsQuery(bool includeZone)
        //{
        //    // first let's try the current requested store's shipping methods
        //    var storeId = AppLogic.StoreID();

        //    var query = GetShippingMethodsQuery(storeId, includeZone);
        //    if (!HasShippingRecordsFound(query)) // if we found no shipping method for it
        //    {
        //        // fallback and try the default store if we're not on the default already
        //        var defStoreId = AppLogic.StoreID();
        //        if (storeId != defStoreId)
        //        {
        //            query = GetShippingMethodsQuery(defStoreId, includeZone);
        //        }
        //    }

        //    return query;
        //}

        ///// <summary>
        ///// Gets whether we are able to get any shipping methods for a particular query
        ///// </summary>
        ///// <param name="query"></param>
        ///// <returns></returns>
        //protected bool HasShippingRecordsFound(string query)
        //{
        //    bool foundAny = false;

        //    Action<IDataReader> readAction = (rs) =>
        //    {
        //        foundAny = rs.Read();
        //    };
        //    DB.UseDataReader(query, readAction);

        //    return foundAny;
        //}

        /// <summary>
        /// Generates the shipping method query per store
        /// </summary>
        /// <param name="storeId">The store id</param>
        /// <param name="includeZone">Whether to include zone mapping</param>
        /// <returns></returns>
        protected string GenerateShippingMethodsQuery(int storeId, bool includeZone)
        {
            bool shippingMethodToStateMapIsEmpty = Shipping.ShippingMethodToStateMapIsEmpty();
            bool shippingMethodToCountryMapIsEmpty = Shipping.ShippingMethodToCountryMapIsEmpty();
            //JH shipping estimator fix - check stateid rather than is registered
            int customerStateID = AppLogic.GetStateID(this.ShippingAddress.State);
            int customerCountryID = AppLogic.GetCountryID(this.ShippingAddress.Country);
            //end JH
            if (customerCountryID < 1 && this.ShippingAddress.Country.Length == 2)
            {
                customerCountryID = AppLogic.GetCountryIDFromTwoLetterISOCode(this.ShippingAddress.Country);
            }
            if (customerCountryID < 1 && this.ShippingAddress.Country.Length == 3)
            {
                customerCountryID = AppLogic.GetCountryIDFromThreeLetterISOCode(this.ShippingAddress.Country);
            }

            StringBuilder shipsql = new StringBuilder(1024);

            if (storeId == Shipping.DONT_FILTER_PER_STORE)
            {
                shipsql.AppendFormat("select sm.* from ShippingMethod sm with (NOLOCK)  WHERE sm.IsRTShipping=0");
            }
            else
            {
                shipsql.AppendFormat(@"SELECT sm.* FROM ShippingMethod sm 
                                       INNER JOIN ShippingMethodStore sms ON sms.ShippingMethodId = sm.ShippingMethodId 
                                       WHERE sm.IsRTShipping=0 AND sms.StoreId = {0}", storeId);
                //shipsql.AppendFormat("select * from StoreShippingMethodMappingView  with (NOLOCK)  where IsRTShipping=0 AND StoreId = {0}", storeId);
            }


            //JH shipping estimator fix - check stateid rather than is registered
            if (!shippingMethodToStateMapIsEmpty && customerStateID <= 0)
            {
                shipsql.Append(" and sm.ShippingMethodID in (select ShippingMethodID from ShippingMethodToStateMap  with (NOLOCK))");
            }

            if (!shippingMethodToStateMapIsEmpty && customerStateID > 0)
            {
                shipsql.Append(" and sm.ShippingMethodID in (select ShippingMethodID from ShippingMethodToStateMap  with (NOLOCK)  where StateID=" + customerStateID.ToString() + ")");
            }
            //end JH

            if (!shippingMethodToCountryMapIsEmpty)
            {
                shipsql.Append(" and sm.ShippingMethodID in (select ShippingMethodID from ShippingMethodToCountryMap  with (NOLOCK)  where CountryID=" + customerCountryID.ToString() + ")");
            }

            // most of the shipping methods honor the state and country mapping
            // except for the zip zone mappings which only a handful or them use
            if (includeZone)
            {
                bool shippingMethodToZoneMapIsEmpty = Shipping.ShippingMethodToZoneMapIsEmpty();
                if (!shippingMethodToZoneMapIsEmpty)
                {
                    shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToZoneMap  with (NOLOCK)  where ShippingZoneID=" + Shipping.ZoneLookup(this.ShippingAddress.Zip).ToString() + ")");
                }
            }
            
            shipsql.Append(" order by Displayorder");

            return shipsql.ToString();
        }

        public abstract ShippingMethodCollection GetShippingMethods(int storeId);

        #endregion
        
    }
}
