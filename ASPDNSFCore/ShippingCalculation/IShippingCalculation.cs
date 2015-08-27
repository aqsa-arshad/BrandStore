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
    /// Base Interface for Shipping Calculations
    /// </summary>
    public interface IShippingCalculation
    {
        Customer ThisCustomer { get;set;}
        Address ShippingAddress { get;set;}
        ShoppingCart Cart { get;set;}
        decimal TaxRate { get;set;}
        bool ShippingIsFreeIfIncludedInFreeList { get;set;}
        decimal HandlingExtraFee { get;set;}
        bool ExcludeZeroFreightCosts { get;set;}
        bool RequirePostalCode { get; }        
        ShippingMethodCollection GetShippingMethods(int storeId);
    }
}
