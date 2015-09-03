// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data.Linq;

namespace AspDotNetStorefrontCore
{
	public class OrderShipmentCollection : IEnumerable<OrderShipment>
	{
		protected List<OrderShipment> Shipments { get; set; }

		public OrderShipmentCollection(int orderNumber)
		{
			Shipments = new List<OrderShipment>();

            using(var connection = new SqlConnection(DB.GetDBConn()))
            {
				// Note that the order of the returned columns is important.
				var command = new SqlCommand("select OrderShipmentID, OrderNumber, AddressID, ShippingTotal from OrderShipment where OrderNumber = @orderNumber", connection);
                command.Parameters.Add(new SqlParameter("@orderNumber", orderNumber));
                connection.Open();

				using(var reader = command.ExecuteReader())
				{
					while(reader.Read())
					{
						Shipments.Add(new OrderShipment(
							reader.GetInt32(0),
							reader.GetInt32(1),
							reader.GetInt32(2),
							reader.GetDecimal(3)
						));
					}
				}
			}
		}

		public IEnumerator<OrderShipment> GetEnumerator()
		{
			return Shipments.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return Shipments.GetEnumerator();
		}
	}
}
