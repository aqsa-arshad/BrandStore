// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Serialization;

namespace AspDotNetStorefrontCore
{
    public static partial class Shipwire
    {
        public static void SubmitOrder(Order ord, int DistributorID)
        {
            // create a list of order Items to send to Shipwire
            List<sw.Item> items = new List<sw.Item>();

            int itemNum = 0;
            foreach (CartItem crtItem in ord.CartItems)
            {
                // skip cart items that don't belong to this distributor
                if (crtItem.DistributorID != DistributorID)
                    continue;

                sw.Item item = new sw.Item();
                item.num = itemNum;
                item.Code = crtItem.SKU;
                item.Quantity = crtItem.Quantity;

                items.Add(item);
                itemNum++;
            }

            // create an AddressInfo for the recipient address
            sw.AddressInfo addressInfo = new sw.AddressInfo();
            addressInfo.Name.Full = string.Copy(ord.ShippingAddress.m_FirstName + " " +
                ord.ShippingAddress.m_LastName).PadRight(25).Substring(0, 25).Trim();
            addressInfo.Address1 = ord.ShippingAddress.m_Address1.PadRight(25).Substring(0, 25).Trim();
            addressInfo.Address2 = string.Copy(ord.ShippingAddress.m_Address2 + " " +
                ord.ShippingAddress.m_Suite).PadRight(25).Substring(0, 25).Trim();
            addressInfo.City = ord.ShippingAddress.m_City.PadRight(25).Substring(0, 25).Trim();
            addressInfo.State = ord.ShippingAddress.m_State;
            addressInfo.Country = ord.ShippingAddress.m_Country;
            addressInfo.Zip = ord.ShippingAddress.m_Zip;
            addressInfo.Phone = ord.ShippingAddress.m_Phone;
            addressInfo.Email = ord.ShippingAddress.m_EMail.PadRight(50).Substring(0, 50).Trim();

            // create a Shipwire Order containing the Items and AddressInfo
            sw.Order order = new sw.Order();
            order.id = ord.OrderNumber.ToString();
            order.AddressInfo[0] = addressInfo;
            order.Shipping = "1D"; //ord.ShippingMethod;
            order.Items = items;

            // wrap Order in an OrderList XML document
            sw.OrderList orderList = new sw.OrderList();
            orderList.EmailAddress = AppLogic.AppConfig("Shipwire.Username");
            orderList.Password = AppLogic.AppConfig("Shipwire.Password");
            orderList.Server = "Production";
            orderList.Order[0] = order;

            // serialize OrderList into an xml string
            XmlWriterSettings xwSettings = new XmlWriterSettings();
            xwSettings.OmitXmlDeclaration = true;

            XmlSerializer serRequest = new XmlSerializer(orderList.GetType());
            StringWriter swRequest = new StringWriter();
            XmlWriter xwRequest = XmlWriter.Create(swRequest, xwSettings);
            serRequest.Serialize(xwRequest, orderList);

            string req = swRequest.ToString();

            // sent to Shipwire
            string resp = SendRequest("FulfillmentServices.php", "OrderListXML=", req);

            // deserialize the xml response into a SubmitOrderResponse object
            sw.SubmitOrderResponse response = new sw.SubmitOrderResponse();
            XmlSerializer serResponse = new XmlSerializer(typeof(sw.SubmitOrderResponse));
            StringReader srResponse = new StringReader(resp);

            try
            {
                response = (sw.SubmitOrderResponse)serResponse.Deserialize(srResponse);
            }
            catch (InvalidOperationException e)  // invalid xml, or no reply received from Shipwire
            {
                throw new Exception("Could not parse response from Shipwire server: " + e.Message
                    + " Response received: " + resp);
            }

            srResponse.Close();

            // check SubmitOrderResponse
            if (response.Status == "Error")
                throw new Exception("Error in ShipwireSubmitOrder(): " + response.ErrorMessage);
        }

        public static string UpdateTracking()
        {
            // create a Shipwire TrackingUpdate request
            sw.TrackingUpdate trackingUpdate = new sw.TrackingUpdate();
            trackingUpdate.EmailAddress = AppLogic.AppConfig("Shipwire.Username");
            trackingUpdate.Password = AppLogic.AppConfig("Shipwire.Password");
            trackingUpdate.Server = "Production";
            trackingUpdate.Bookmark = (int) sw.BookmarkType.sinceLastBookmarkAndMoveBookmark;

            // serialize TrackingUpdate into an xml string
            XmlWriterSettings xwSettings = new XmlWriterSettings();
            xwSettings.OmitXmlDeclaration = true;

            XmlSerializer serRequest = new XmlSerializer(trackingUpdate.GetType());
            StringWriter swRequest = new StringWriter();
            XmlWriter xwRequest = XmlWriter.Create(swRequest, xwSettings);
            serRequest.Serialize(xwRequest, trackingUpdate);

            string req = swRequest.ToString();

            // sent to Shipwire
            string resp = SendRequest("TrackingServices.php", "TrackingUpdateXML=", req);

            // deserialize the xml response into a TrackingUpdateResponse object
            sw.TrackingUpdateResponse response = new sw.TrackingUpdateResponse();
            XmlSerializer serResponse = new XmlSerializer(typeof(sw.TrackingUpdateResponse));
            StringReader srResponse = new StringReader(resp);

            try
            {
                response = (sw.TrackingUpdateResponse)serResponse.Deserialize(srResponse);
            }
            catch (InvalidOperationException e)  // invalid xml, or no reply received from Shipwire
            {
                throw new Exception("Could not parse response from Shipwire server: " + e.Message + " Response received: " + resp);
            }

            srResponse.Close();

            // check TrackingUpdateResponse
            if (response.Status == "Error")
                throw new Exception("Shipwire returned an error. Please ensure that your Username and Password are correct: " +
                    response.ErrorMessage);

            // check for results
            if (response.OrderStatus == null)
                return "No orders found.";

            // mark shipped orders as shipped
            foreach (sw.OrderStatus ordStatus in response.OrderStatus)
                if (ordStatus.shipped.Equals("yes", StringComparison.InvariantCultureIgnoreCase))
                    Order.MarkOrderAsShipped(Convert.ToInt32(ordStatus.id), ordStatus.shipper, ordStatus.trackingNumber,
                        System.DateTime.Now, false, null, null, true);

            return string.Format("Marked {0} orders as Shipped, out of a total of {1} orders.",
                response.TotalShippedOrders, response.TotalOrders);
        }

        public static string UpdateInventory()
        {
            // create a Shipwire InventoryUpdate request
            sw.InventoryUpdate inventoryUpdate = new sw.InventoryUpdate();
            inventoryUpdate.EmailAddress = AppLogic.AppConfig("Shipwire.Username");
            inventoryUpdate.Password = AppLogic.AppConfig("Shipwire.Password");
            inventoryUpdate.Server = "Production";

            // serialize InventoryUpdate into an xml string
            XmlWriterSettings xwSettings = new XmlWriterSettings();
            xwSettings.OmitXmlDeclaration = true;

            XmlSerializer serRequest = new XmlSerializer(inventoryUpdate.GetType());
            StringWriter swRequest = new StringWriter();
            XmlWriter xwRequest = XmlWriter.Create(swRequest, xwSettings);
            serRequest.Serialize(xwRequest, inventoryUpdate);

            string req = swRequest.ToString();

            // sent to Shipwire
            string resp = SendRequest("InventoryServices.php", "InventoryUpdateXML=", req);

            // deserialize the xml response into a InventoryUpdateResponse object
            sw.InventoryUpdateResponse response = new sw.InventoryUpdateResponse();
            XmlSerializer serResponse = new XmlSerializer(typeof(sw.InventoryUpdateResponse));
            StringReader srResponse = new StringReader(resp);

            try
            {
                response = (sw.InventoryUpdateResponse)serResponse.Deserialize(srResponse);
            }
            catch (InvalidOperationException e)  // invalid xml, or no reply received from Shipwire
            {
                throw new Exception("Could not parse response from Shipwire server: " + e.Message
                    + " Response received: " + resp);
            }

            srResponse.Close();

            // check InventoryUpdateResponse
            if (response.Status == "Error")
                throw new Exception("Shipwire returned an error. Please ensure that your Username and Password are correct: " +
                    response.ErrorMessage);

            // check for results
            if (response.Product == null)
                return "No products found.";

            // update Inventory column of ProductVariant table
            foreach (sw.Product invProduct in response.Product)
                DB.ExecuteSQL("update ProductVariant set Inventory = " + invProduct.quantity.ToString() + " " +
                    "from ProductVariant pv inner join Product p on p.ProductID = pv.ProductID " +
                    "where p.SKU + pv.SKUSuffix = '" + invProduct.code + "'");

            return string.Format("Updated inventory of {0} products.", response.TotalProducts.ToString());
        }

        private static string SendRequest(string ShipwireEndpoint, string prefix, string req)
        {
            // Send xml request to Shipwire server
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create("https://www.shipwire.com/exec/" + ShipwireEndpoint);
            webRequest.Method = "POST";
            webRequest.ContentType = "application/x-www-form-urlencoded";

            // Transmit the request to Shipwire
            string encodedReq = System.Web.HttpUtility.UrlEncode(req);
            byte[] data = System.Text.Encoding.ASCII.GetBytes(prefix + encodedReq);
            webRequest.ContentLength = data.Length;
            Stream requestStream;

            try
            {
                requestStream = webRequest.GetRequestStream();
            }
            catch (WebException e)  // could not connect to Shipwire endpoint
            {
                throw new Exception("Tried to reach Shipwire Server: " + e.Message);
            }

            requestStream.Write(data, 0, data.Length);
            requestStream.Close();

            // get the response from Shipwire
            WebResponse webResponse = null;
            string resp;
            try
            {
                webResponse = webRequest.GetResponse();
            }
            catch (WebException e)  // could not receive a response from Shipwire endpoint
            {
                throw new Exception("No response from Shipwire Server: " + e.Message);
            }

            using (StreamReader sr = new StreamReader(webResponse.GetResponseStream()))
            {
                resp = sr.ReadToEnd();
                sr.Close();
            }
            webResponse.Close();
            return resp;
        }
    }

    namespace sw
    {
        [XmlType(AnonymousType = true)]
        [XmlRoot(IsNullable = false)]
        public partial class OrderList
        {
            [XmlAttribute()]
            public string StoreAccountName;

            public string EmailAddress;
            public string Password;
            public string Server;
            public string Referer = "1886";

            [XmlElement("Order")]
            public Order[] Order = new Order[1];
        }

        [XmlType(AnonymousType = true)]
        [XmlRoot(IsNullable = false)]
        public partial class Order
        {
            [XmlAttribute()]
            public string id;

            public string Warehouse;

            [XmlElement("AddressInfo")]
            public AddressInfo[] AddressInfo = new AddressInfo[1];

            public string Shipping;

            [XmlElement("Item")]
            public List<Item> Items;
        }

        [XmlType(AnonymousType = true)]
        [XmlRoot(IsNullable = false)]
        public partial class AddressInfo
        {
            [XmlAttribute()]
            public AddressInfoType type = AddressInfoType.ship;

            public Name Name = new Name();
            public string Address1;
            public string Address2;
            public string City;
            public string State;
            public string Country;
            public string Zip;
            public string Phone;
            public string Email;
        }

        [XmlType(AnonymousType = true)]
        [XmlRoot(IsNullable = false)]
        public partial class Name
        {
            public string Full;
        }

        [XmlType(AnonymousType = true)]
        public enum AddressInfoType
        {
            ship,
        }

        [XmlType(AnonymousType = true)]
        [XmlRoot(IsNullable = false)]
        public partial class Item
        {
            [XmlAttribute()]
            public int num;

            public string Code;
            public int Quantity;
            public string Description;
            public decimal Length;
            public decimal Width;
            public decimal Height;
            public decimal Weight;
            public decimal DeclaredValue;
        }

        [XmlType(AnonymousType = true)]
        [XmlRoot(IsNullable = false)]
        public partial class SubmitOrderResponse
        {
            public string Status;
            public int TotalOrders;
            public int TotalItems;
            public string TransactionId;
            public string ErrorMessage;
        }

        [XmlType(AnonymousType = true)]
        [XmlRoot(IsNullable = false)]
        public partial class TrackingUpdate
        {
            public string EmailAddress;
            public string Password;
            public string Server;
            public int Bookmark;
        }

        [XmlType(AnonymousType = true)]
        [XmlRoot(IsNullable = false)]
        public partial class TrackingUpdateResponse
        {
            public string Status;

            [XmlElement("Order")]
            public OrderStatus[] OrderStatus;

            public string TotalOrders;
            public string TotalShippedOrders;
            public string Bookmark;
            public string ErrorMessage;
        }

        [XmlType(AnonymousType = true)]
        public enum BookmarkType
        {
            everything = 1, sinceLastBookmark = 2, sinceLastBookmarkAndMoveBookmark = 3
        }

        [XmlType(AnonymousType = true)]
        public partial class OrderStatus
        {
            [XmlAttribute()]
            public string id;

            [XmlAttribute()]
            public string shipped;

            [XmlAttribute()]
            public string trackingNumber;

            [XmlAttribute()]
            public string shipper;

            [XmlAttribute()]
            public string handling;

            [XmlAttribute()]
            public string shipping;

            [XmlAttribute()]
            public string total;
        }

        [XmlType(AnonymousType = true)]
        [XmlRoot(IsNullable = false)]
        public partial class InventoryUpdate
        {
            public string EmailAddress;
            public string Password;
            public string Server;
            public string Warehouse;
            public string ProductCode;
        }

        [XmlType(AnonymousType = true)]
        [XmlRoot(IsNullable = false)]
        public partial class InventoryUpdateResponse
        {
            public string Status;

            [XmlElement("Product")]
            public Product[] Product;

            public int TotalProducts;
            public string ProductCode;
            public string ErrorMessage;
        }

        [XmlType(AnonymousType = true)]
        public partial class Product
        {
            [XmlAttribute()]
            public string code;

            [XmlAttribute()]
            public int quantity;
        }
    }
}
