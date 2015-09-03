// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;

namespace AspDotNetStorefrontCore
{
	public class OrderShipment
	{
		public int OrderShipmentID { get; protected set; }
		public int OrderNumber { get; protected set; }
		public int AddressID { get; protected set; }
		public decimal ShippingTotal { get; protected set; }

		public OrderShipment(int orderShipmentID, int orderNumber, int addressID, decimal shippingTotal)
		{
			OrderShipmentID = orderShipmentID;
			OrderNumber = orderNumber;
			AddressID = addressID;
			ShippingTotal = shippingTotal;
		}
	}
}
