// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;
using System.Linq.Expressions;
using System.Xml;
using System.Data;
using System.Data.SqlClient;

/// <summary>
/// Summary description for MultiShipOrder_Shipment
/// </summary>
namespace AspDotNetStorefrontCore
{
    public class MultiShipOrder_ShipmentCollection
    {
        public static List<MultiShipOrder_Shipment> GetMultiShipOrder_ShipmentCollection(Order order)
	    {
            List<MultiShipOrder_Shipment> Shipments = new List<MultiShipOrder_Shipment>();

            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("Select MultiShipOrder_ShipmentId from MultiShipOrder_Shipment with (NOLOCK) where OrderNumber=" + order.OrderNumber.ToString(), dbconn))
                {
                    while (rs.Read())
                    {
                        Shipments.Add(new MultiShipOrder_Shipment(DB.RSFieldInt(rs, "MultiShipOrder_ShipmentId")));
                    }
                }
            }
            return Shipments;
	    }
        public static CartItemCollection GetMultiShipOrder_ShipmentCollection_AsCartItemCollection(Order order)
        {
            CartItemCollection cartItems = new CartItemCollection();

            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("Select MultiShipOrder_ShipmentId from MultiShipOrder_Shipment with (NOLOCK) where OrderNumber=" + order.OrderNumber.ToString(), dbconn))
                {
                    while (rs.Read())
                    {
                        MultiShipOrder_Shipment shipment = new MultiShipOrder_Shipment(DB.RSFieldInt(rs, "MultiShipOrder_ShipmentId"));

                        CartItem ciShipping = new CartItem();
                        ciShipping.VariantID = Prices.GetShippingProductVariant();

                        CartItemCollection ciSingleShippingCollection = new CartItemCollection();
                        ciShipping.ShippingAddressID = shipment.ShippingAddressId;
                        ciShipping.BillingAddressID = shipment.BillingAddressId;

                        //If we do not otherwise apply a shipping method here, our 
                        //item will become invalid during the ShippingIsAllValid routine.
                        ciShipping.ShippingMethodID = shipment.ShippingMethodId;

                        //Take note that we're referencing the current items product ID
                        //for later use when combining the collections.
                        ciShipping.ProductID = 1;
                        ciShipping.ThisShoppingCart = order.CartItems[0].ThisShoppingCart;
                        ciShipping.CartType = CartTypeEnum.ShoppingCart;
                        ciShipping.ThisCustomer = order.CartItems[0].ThisCustomer;
                        if (order.CartItems.IsAllFreeShippingComponents) { ciShipping.IsTaxable = false; }
                        ciShipping.Quantity = 1;
                        ciShipping.Shippable = false;
                        ciShipping.SKU = "SHIPPING";
                        ciShipping.TaxClassID = AppLogic.AppConfigUSInt("ShippingTaxClassID");

                        //We can use the ShippingTotal method, but we should send the item itself
                        //over for proper shipping cost evaluation.
                        ciShipping.Price = shipment.ShippingAmount;
                        ciShipping.IsTaxable = ciShipping.Price > System.Decimal.Zero ? true : false;
                        cartItems.Add(ciShipping);
                    }
                }
            }
            return cartItems;
        }
    }
    public class MultiShipOrder_Shipment
    {
        #region fields and properties
        private int _MultiShipOrder_ShipmentId = 0;
        private string _MultiShipOrder_ShipmentGUID = string.Empty;
        private int _OrderNumber = 0;
        private string _DestinationAddress = string.Empty;
        private decimal _ShippingAmount = 0M;
        private int _ShippingMethodId = 0;
        private int _ShippingAddressId = 0;
        private int _BillingAddressId = 0;

        public int ShippingAddressId
        {
            get { return _ShippingAddressId; }
            set { _ShippingAddressId = value; }
        }
        public int BillingAddressId
        {
            get { return _BillingAddressId; }
            set { _BillingAddressId = value; }
        }
        public int ShippingMethodId
        {
            get { return _ShippingMethodId; }
            set { _ShippingMethodId = value; }
        }
        public int MultiShipOrder_ShipmentId
        {
            get { return _MultiShipOrder_ShipmentId; }
        }
        public int OrderNumber
        {
            get { return _OrderNumber; }
            set { _OrderNumber = value; }
        }
        public string DestinationAddress
        {
            get { return _DestinationAddress; }
            set { _DestinationAddress = value; }
        }
        public decimal ShippingAmount
        {
            get { return _ShippingAmount; }
            set { _ShippingAmount = value; }
        }
        #endregion

        public MultiShipOrder_Shipment(){}

        public MultiShipOrder_Shipment(int Id)
        {
            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("Select OrderNumber, DestinationAddress, ShippingAmount, ShippingMethodId, MultiShipOrder_ShipmentGUID, ShippingAddressId, BillingAddressId from MultiShipOrder_Shipment with (NOLOCK) where MultiShipOrder_ShipmentId=" + Id.ToString(), dbconn))
                {
                    if (rs.Read())
                    {
                        _MultiShipOrder_ShipmentId = Id;
                        _OrderNumber = DB.RSFieldInt(rs, "OrderNumber");
                        _DestinationAddress = DB.RSField(rs, "DestinationAddress");
                        _ShippingAmount = DB.RSFieldDecimal(rs, "ShippingAmount");
                        _MultiShipOrder_ShipmentGUID = DB.RSFieldGUID(rs, "MultiShipOrder_ShipmentGUID");
                        _ShippingMethodId = DB.RSFieldInt(rs, "ShippingMethodId");
                        _ShippingAddressId = DB.RSFieldInt(rs, "ShippingAddressId");
                        _BillingAddressId = DB.RSFieldInt(rs, "BillingAddressId");
                    }
                }
            }
        }

        public void Save()
        {
            if (_MultiShipOrder_ShipmentId > 0)
                UpdateDB();
            else
                InsertDB();
        }

        /// <summary>
        /// Adds Shipment details from a Multiple Shipment Order 
        /// </summary>
        public void InsertDB()
        {
            string MultiShipOrder_ShipmentGUID = CommonLogic.GetNewGUID();
            string sql = String.Format("insert into MultiShipOrder_Shipment(MultiShipOrder_ShipmentGUID,OrderNumber,DestinationAddress,ShippingAmount,ShippingMethodId,ShippingAddressId,BillingAddressId) values({0},{1},{2},{3},{4},{5},{6})", DB.SQuote(MultiShipOrder_ShipmentGUID), _OrderNumber, DB.SQuote(_DestinationAddress), _ShippingAmount, _ShippingMethodId,_ShippingAddressId,_BillingAddressId);
            DB.ExecuteSQL(sql);

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS(String.Format("select MultiShipOrder_ShipmentId from MultiShipOrder_Shipment with (NOLOCK) where MultiShipOrder_ShipmentGUID={0}", DB.SQuote(MultiShipOrder_ShipmentGUID)), dbconn))
                {
                    if (rs.Read())
                    {
                        _MultiShipOrder_ShipmentId = DB.RSFieldInt(rs, "MultiShipOrder_ShipmentId");
                    }
                }
            }
        }

        /// <summary>
        /// Updates Shipment details from a Multiple Shipment Order 
        /// </summary>
        public void UpdateDB()
        {
            string sql = String.Format("update Address set OrderNumber={1},DestinationAddress={2},ShippingAmount={3},ShippingMethodId={4},ShippingAddressId={5},BillingAddressId={6} where MultiShipOrder_Shipmentid={0}", _MultiShipOrder_ShipmentId, _OrderNumber, DB.SQuote(_DestinationAddress), _ShippingAmount, _ShippingMethodId,_ShippingAddressId,_BillingAddressId);

            DB.ExecuteSQL(sql);
        }
    }
}
