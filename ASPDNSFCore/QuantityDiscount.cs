// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using System.Web;

namespace AspDotNetStorefrontCore
{
	public class QuantityDiscount
	{
		public enum QuantityDiscountType
		{
			None = -1,
			Percentage = 0,
			FixedAmount = 1
		}

		static public String GetQuantityDiscountName(int QuantityDiscountID, String LocaleSetting)
		{
			String tmpS = String.Empty;

			using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using (IDataReader rs = DB.GetRS("Select Name from QuantityDiscount   with (NOLOCK)  where QuantityDiscountID=" + QuantityDiscountID.ToString(), con))
				{
					if (rs.Read())
					{
						tmpS = DB.RSFieldByLocale(rs, "Name", LocaleSetting);
					}
				}
			}

			return tmpS;
		}

		// don't return any quotes, single quotes, or carraige returns in this string!
		static public String GetQuantityDiscountDisplayTable(int DID, int SkinID)
		{
			String CacheName = "GetQuantityDiscountDisplayTable_" + DID.ToString() + "_" + SkinID.ToString();
			if (AppLogic.CachingOn)
			{
				String CacheData = (String)HttpContext.Current.Cache.Get(CacheName);
				if (CacheData != null)
				{
					if (CommonLogic.ApplicationBool("DumpSQL"))
					{
						HttpContext.Current.Response.Write("Cache Hit Found!");
					}
					return CacheData;
				}
			}
			Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)HttpContext.Current.User).ThisCustomer;
			bool fixedDiscount = isFixedQuantityDiscount(DID);
			StringBuilder tmpS = new StringBuilder(10000);
			String sql = "select * from dbo.QuantityDiscountTable  with (NOLOCK)  where QuantityDiscountID=" + DID.ToString() + " order by LowQuantity";

            tmpS.Append("<table class=\"table table-striped quantity-discount-table\">");
            tmpS.Append("<tr class=\"table-header\"><th>" + AppLogic.GetString("common.cs.34", SkinID, Thread.CurrentThread.CurrentUICulture.Name) + "</th><th>" + CommonLogic.IIF(fixedDiscount, AppLogic.GetString("shoppingcart.cs.116", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + " ", "") + AppLogic.GetString("common.cs.35", SkinID, Thread.CurrentThread.CurrentUICulture.Name) + "</th></tr>");
			using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using (IDataReader rs = DB.GetRS(sql, con))
				{
					while (rs.Read())
					{
                        tmpS.Append("<tr class=\"table-row\">");
						tmpS.Append("<td class=\"quantity-cell\">");
						tmpS.Append(DB.RSFieldInt(rs, "LowQuantity").ToString() + CommonLogic.IIF(DB.RSFieldInt(rs, "HighQuantity") > 9999, "+", "-" + DB.RSFieldInt(rs, "HighQuantity").ToString()));
						tmpS.Append("</td>");
						tmpS.Append("<td class=\"discount-cell\">");
						if (fixedDiscount)
						{
							tmpS.Append(Localization.CurrencyStringForDisplayWithExchangeRate(DB.RSFieldDecimal(rs, "DiscountPercent"), ThisCustomer.CurrencySetting));
						}
						else
						{
							tmpS.Append(DB.RSFieldDecimal(rs, "DiscountPercent").ToString("N" + AppLogic.AppConfigNativeInt("QuantityDiscount.PercentDecimalPlaces")) + "%");
						}
						tmpS.Append("</td>");
						tmpS.Append("</tr>");
					}
				}
			}

			tmpS.Append("</table>");

			if (AppLogic.CachingOn)
			{
				HttpContext.Current.Cache.Insert(CacheName, tmpS.ToString(), null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
			}
			return tmpS.ToString();
		}

		/// <summary>
		/// Returns the Quantity Discount Percent for the specified product and quantity
		/// </summary>
		/// <param name="ProductID">The product ID to evaluate</param>
		/// <param name="Quantity">The quantity value to evaluate</param>
		/// <returns></returns>
		static private Decimal GetQuantityDiscountTablePercentage(int ProductID, int Quantity, out QuantityDiscountType DiscountType)
		{

			Decimal tmp = 0.0M;
			DiscountType = QuantityDiscountType.Percentage;
			if (ProductID != 0)
			{
				SqlParameter[] spa = { DB.CreateSQLParameter("@productid", SqlDbType.Int, 4, ProductID, ParameterDirection.Input), DB.CreateSQLParameter("@qty", SqlDbType.Int, 4, Quantity, ParameterDirection.Input) };

				using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
				{
					con.Open();
					using (IDataReader rs = DB.GetRS("select dbo.GetQtyDiscount(@productid, @qty, 0) Pct, dbo.GetQtyDiscount(@productid, @qty, 1) Amt", spa, con))
					{
						if (rs.Read())
						{
							Decimal pct = DB.RSFieldDecimal(rs, "Pct");
							Decimal amt = DB.RSFieldDecimal(rs, "Amt");
							if (amt > decimal.Zero)
							{
								DiscountType = QuantityDiscountType.FixedAmount;
								tmp = amt;
							}
							else
							{
								DiscountType = QuantityDiscountType.Percentage;
								tmp = pct;
							}
						}
					}
				}
			}
			return tmp;
		}

		static public Decimal GetQuantityDiscountTablePercentageWithoutCartAwareness(int ProductID, int Quantity, out QuantityDiscountType DiscountType)
		{
			return GetQuantityDiscountTablePercentage(ProductID, Quantity, out DiscountType);
		}

		static public Decimal GetQuantityDiscountTablePercentageForLineItem(CartItem item, out QuantityDiscountType DiscountType)
		{
			if (!AppLogic.AppConfigBool("QuantityDiscount.CombineQuantityByProduct"))
				return GetQuantityDiscountTablePercentage(item.ProductID, item.Quantity, out DiscountType);

			if (item.ThisShoppingCart.CartItems.Count < 1)
				throw new ArgumentException("cart items must be greater than 0.");

			int quan = 0;
			foreach (CartItem c in item.ThisShoppingCart.CartItems)
			{
				if (c.ProductID == item.ProductID)
				{
					quan += c.Quantity;
				}
			}

			return GetQuantityDiscountTablePercentage(item.ProductID, quan, out DiscountType);
		}

		/// <summary>
		/// Returns the Quantity Discount ID for the specified Product
		/// </summary>
		/// <param name="ProductID">The productID to find the QuantityDiscountID for</param>
		/// <returns>Quantity Discount ID</returns>
		static public int LookupProductQuantityDiscountID(int ProductID)
		{

			int DID = 0;
			SqlParameter[] spa = { DB.CreateSQLParameter("@productid", SqlDbType.Int, 4, ProductID, ParameterDirection.Input) };

			using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using (IDataReader rs = DB.GetRS("select dbo.GetQtyDiscountID(@productid) DID", spa, con))
				{
					if (rs.Read())
					{
						DID = DB.RSFieldInt(rs, "DID");
					}
				}
			}

			return DID;
		}

		static public int GetQuantityDiscountTableID(String Name)
		{
			int tmp = 0;
			if (Name.Length != 0)
			{
				using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
				{
					con.Open();
					using (IDataReader rs = DB.GetRS("select QuantityDiscountID from  QuantityDiscount  with (NOLOCK)  where name like " + DB.SQuote("%" + Name + "%"), con))
					{
						if (rs.Read())
						{
							tmp = DB.RSFieldInt(rs, "QuantityDiscountID");
						}
					}
				}
			}
			return tmp;
		}

		static public bool isFixedQuantityDiscount(int quantityDiscountID)
		{
			bool tmp = false;
			if (quantityDiscountID != 0)
			{
				using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
				{
					con.Open();
					using (IDataReader rs = DB.GetRS("select DiscountType from  QuantityDiscount  with (NOLOCK)  where QuantityDiscountID=" + quantityDiscountID, con))
					{
						if (rs.Read())
						{
							tmp = DB.RSFieldBool(rs, "DiscountType");
						}
					}
				}
			}
			return tmp;
		}

		static public bool CustomerLevelAllowsQuantityDiscounts(int CustomerLevelID)
		{
			if (CustomerLevelID == 0)
			{
				// consumers always have this option by default, it can be overridden by product/variant settings however:
				return true;
			}
			bool tmpS = false;

			using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using (IDataReader rs = DB.GetRS("Select LevelAllowsQuantityDiscounts from CustomerLevel   with (NOLOCK)  where CustomerLevelID=" + CustomerLevelID.ToString(), con))
				{
					if (rs.Read())
					{
						tmpS = DB.RSFieldBool(rs, "LevelAllowsQuantityDiscounts");
					}
				}
			}

			return tmpS;

		}
	}

	public class QDObject
	{
		internal int m_quantitydiscountid { get; set; }
		internal string m_quantitydiscountguid { get; set; }
		internal string m_name { get; set; }
		internal int m_displayorder { get; set; }
		internal string m_extensiondata { get; set; }
		internal int m_discounttype { get; set; }
		internal DateTime m_createdon { get; set; }
		internal IList<QDTObject> m_quantitydiscounttables { get; set; }

		public QDObject()
		{
			m_quantitydiscounttables = new List<QDTObject>();
		}

		public int QuantityDiscountID()
		{
			return m_quantitydiscountid;
		}

		public string QuantityDiscountGUID()
		{
			return m_quantitydiscountguid;
		}

		public string Name()
		{
			return m_name;
		}

		public int DisplayOrder()
		{
			return m_displayorder;
		}

		public string ExtensionData()
		{
			return m_extensiondata;
		}

		public int DiscountType()
		{
			return m_discounttype;
		}

		public DateTime CreatedOn()
		{
			return m_createdon;
		}

		public IList<QDTObject> QuantityDiscountTables()
		{
			return m_quantitydiscounttables;
		}
	}

	public class QDTObject
	{
		internal int m_quantitydiscounttableid { get; set; }
		internal string m_quantitydiscounttableguid { get; set; }
		internal int m_quantitydiscountid { get; set; }
		internal int m_lowquantity { get; set; }
		internal int m_highquantity { get; set; }
		internal decimal m_discountpercent { get; set; }
		internal decimal m_discountamount { get; set; }
		internal DateTime m_createdon { get; set; }

		public QDTObject() { }

		public int QuantityDiscountTableID()
		{
			return m_quantitydiscounttableid;
		}

		public string QuantityDiscountTableGUID()
		{
			return m_quantitydiscounttableguid;
		}

		public int QuantityDiscountID()
		{
			return m_quantitydiscountid;
		}

		public int LowQuantity()
		{
			return m_lowquantity;
		}

		public int HighQuantity()
		{
			return m_highquantity;
		}

		public decimal DiscountPercent()
		{
			return m_discountpercent;
		}

		public DateTime CreatedOn()
		{
			return m_createdon;
		}

	}
}
