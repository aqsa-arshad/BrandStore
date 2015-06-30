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
using System.Linq;
using System.Text;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class shippingfrm : AdminPageBase
	{
		/// <summary>
		/// Gets or sets the available stores
		/// </summary>
		public IEnumerable<Store> Stores
		{ get; set; }

		/// <summary>
		/// Gets or sets whether multi-store filtering is enabled
		/// </summary>
		public bool MultiStoreFilteringEnabled
		{ get { return AppLogic.GlobalConfigBool("AllowShippingFiltering"); } }

		/// <summary>
		/// Gets whether the postback was caused by the stores dropdown
		/// </summary>
		/// <returns></returns>
		private bool IsStoreFilterChangePostBack
		{ get { return IsPostBack && Request["__EVENTTARGET"].EqualsIgnoreCase("cboStores"); } }

		/// <summary>
		/// Gets or sets the store id for filtering
		/// </summary>
		public int StoreFilter
		{ get; set; }

		protected void Page_Load(object sender, System.EventArgs e)
		{
			InitializeStores();

			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");

			SectionTitle = "Shipping Tables";

			RenderHtml();
		}

		/// <summary>
		/// Initializes the stores collection and determines the default store filter
		/// </summary>
		private void InitializeStores()
		{
			Stores = Store.GetStoreList();

			if(Request.QueryStringNativeInt("StoreId") > 0)
				StoreFilter = Request.QueryStringNativeInt("StoreId");

			if(CommonLogic.FormNativeInt("StoreFilter") > 0)
				StoreFilter = CommonLogic.FormNativeInt("StoreFilter");

			if(IsStoreFilterChangePostBack && CommonLogic.FormNativeInt("cboStores") > 0)
				StoreFilter = CommonLogic.FormNativeInt("cboStores");

			if(!MultiStoreFilteringEnabled || StoreFilter == 0)
			{
				StoreFilter = Stores
					.Where(store => store.IsDefault)
					.Select(store => store.StoreID)
					.FirstOrDefault();
			}
		}

		/// <summary>
		/// Ensures that current storeid has a corresponding calculation id on the ShippingCalculationStore table
		/// </summary>
		/// <returns></returns>
		private int EnsureStoreShippingCalculation()
		{
			int calcId;

			// first check if we have an entry in the mapping table for this store calculation
			if(!HasShippingCalculationMap(StoreFilter))
			{
				calcId = AppLogic.AppConfigUSInt("DefaultShippingCalculationID");
				string insertDefaultSql = string.Format("INSERT INTO ShippingCalculationStore(StoreId, ShippingCalculationId) Values({0}, {1})", StoreFilter, calcId);

				DB.ExecuteSQL(insertDefaultSql);
			}

			string query = string.Format("SELECT ShippingCalculationId AS N FROM ShippingCalculationStore WHERE StoreID = {0}", StoreFilter);
			calcId = DB.GetSqlN(query);

			return calcId;
		}

		/// <summary>
		/// Determines if the storeid has a calculation configured
		/// </summary>
		/// <param name="storeId"></param>
		/// <returns></returns>
		private bool HasShippingCalculationMap(int storeId)
		{
			string query = string.Format("SELECT COUNT(*) AS N FROM ShippingCalculationStore WHERE StoreID = {0}", storeId);
			return DB.GetSqlN(query) > 0;
		}

		private void RenderHtml()
		{
			StringBuilder html = new StringBuilder();

			String EditGUID = CommonLogic.FormCanBeDangerousContent("EditGUID");
			String UpdateGUID = CommonLogic.FormCanBeDangerousContent("UpdateGUID");
			String DeleteGUID = CommonLogic.FormCanBeDangerousContent("DeleteGUID");
			if(EditGUID.Length == 0)
			{
				EditGUID = CommonLogic.QueryStringCanBeDangerousContent("EditGUID");
			}
			if(UpdateGUID.Length == 0)
			{
				UpdateGUID = CommonLogic.QueryStringCanBeDangerousContent("UpdateGUID");
			}
			if(DeleteGUID.Length == 0)
			{
				DeleteGUID = CommonLogic.QueryStringCanBeDangerousContent("DeleteGUID");
			}

			if(CommonLogic.FormBool("IsSubmitCalculationID") && !IsStoreFilterChangePostBack)
			{
				EnsureStoreShippingCalculation();

				int preferredCalculationId = CommonLogic.FormUSInt("ShippingCalculationID");
				DB.ExecuteSQL("Update ShippingCalculation set Selected=0");

				string updateStoreCalculation = string.Format("Update ShippingCalculationStore set ShippingCalculationID={0} where StoreId={1}", preferredCalculationId, StoreFilter);
				DB.ExecuteSQL(updateStoreCalculation);
			}

			if(CommonLogic.FormBool("IsSubmitFixedRate"))
			{
				using(SqlConnection sqlConnection = new SqlConnection(DB.GetDBConn()))
				{
					sqlConnection.Open();

					string getShippingMethodMapping = "exec aspdnsf_GetStoreShippingMethodMapping @StoreID = @storeId, @IsRTShipping = 0, @OnlyMapped = @filterByStore, @ExcludeNameLike = 'RealTime'";
					var getShippingMethodMappingParams = new[]
					{
						new SqlParameter("@storeId", StoreFilter),
						new SqlParameter("@filterByStore", MultiStoreFilteringEnabled),
					};

					using(IDataReader rs = DB.GetRS(getShippingMethodMapping, getShippingMethodMappingParams, sqlConnection))
					{
						while(rs.Read())
						{
							String FieldName = "FixedRate_" + DB.RSFieldInt(rs, "ShippingMethodID").ToString();
							if(CommonLogic.FormCanBeDangerousContent(FieldName).Length != 0)
							{
								DB.ExecuteSQL("Update ShippingMethod set FixedRate=" + Localization.CurrencyStringForDBWithoutExchangeRate(CommonLogic.FormUSDecimal(FieldName)) + " where ShippingMethodID=" + DB.RSFieldInt(rs, "ShippingMethodID").ToString());
							}
							else
							{
								DB.ExecuteSQL("Update ShippingMethod set FixedRate=NULL where ShippingMethodID=" + DB.RSFieldInt(rs, "ShippingMethodID").ToString());
							}
						}
					}
				}
                Response.Redirect("shipping.aspx");
			}

			if(CommonLogic.FormBool("IsSubmitFixedPercentOfTotal"))
			{
				using(SqlConnection sqlConnection = new SqlConnection(DB.GetDBConn()))
				{
					sqlConnection.Open();

					string getShippingMethodMapping = "exec aspdnsf_GetStoreShippingMethodMapping @StoreID = @storeId, @IsRTShipping = 0, @OnlyMapped = @filterByStore, @ExcludeNameLike = 'RealTime'";
					var getShippingMethodMappingParams = new[]
					{
						new SqlParameter("@storeId", StoreFilter),
						new SqlParameter("@filterByStore", MultiStoreFilteringEnabled),
					};

					using(IDataReader rs = DB.GetRS(getShippingMethodMapping, getShippingMethodMappingParams, sqlConnection))
					{
						while(rs.Read())
						{
							String FieldName = "FixedPercentOfTotal_" + DB.RSFieldInt(rs, "ShippingMethodID").ToString();
							if(CommonLogic.FormCanBeDangerousContent(FieldName).Length != 0)
							{
								DB.ExecuteSQL("Update ShippingMethod set FixedPercentOfTotal=" + Localization.DecimalStringForDB(CommonLogic.FormUSDecimal(FieldName)) + " where ShippingMethodID=" + DB.RSFieldInt(rs, "ShippingMethodID").ToString());
							}
							else
							{
								DB.ExecuteSQL("Update ShippingMethod set FixedPercentOfTotal=NULL where ShippingMethodID=" + DB.RSFieldInt(rs, "ShippingMethodID").ToString());
							}
						}
					}
				}
                Response.Redirect("shipping.aspx");
			}

			if(CommonLogic.FormBool("IsSubmitByTotal"))
			{
				if(EditGUID.Length != 0)
				{
					DB.ExecuteSQL("delete from ShippingByTotal where RowGUID=" + DB.SQuote(EditGUID));
				}

				// check for new row addition:
				Decimal Low0 = CommonLogic.FormUSDecimal("Low_0");
				Decimal High0 = CommonLogic.FormUSDecimal("High_0");
				String NewRowGUID = DB.GetNewGUID();

				if(Low0 != System.Decimal.Zero || High0 != System.Decimal.Zero)
				{
					using(SqlConnection sqlConnection = new SqlConnection(DB.GetDBConn()))
					{
						sqlConnection.Open();

						string getShippingMethodMapping = "exec aspdnsf_GetStoreShippingMethodMapping @StoreID = @storeId, @IsRTShipping = 0, @OnlyMapped = @filterByStore, @ExcludeNameLike = 'RealTime'";
						var getShippingMethodMappingParams = new[]
						{
							new SqlParameter("@storeId", StoreFilter),
							new SqlParameter("@filterByStore", MultiStoreFilteringEnabled),
						};

						using(IDataReader rs = DB.GetRS(getShippingMethodMapping, getShippingMethodMappingParams, sqlConnection))
						{
							while(rs.Read())
							{
								decimal Charge = CommonLogic.FormUSDecimal("Rate_0_" + DB.RSFieldInt(rs, "ShippingMethodID").ToString());
								DB.ExecuteSQL("insert into ShippingByTotal(RowGUID,LowValue,HighValue,ShippingMethodID,ShippingCharge) values(" + DB.SQuote(NewRowGUID) + "," + Localization.CurrencyStringForDBWithoutExchangeRate(Low0) + "," + Localization.CurrencyStringForDBWithoutExchangeRate(High0) + "," + DB.RSFieldInt(rs, "ShippingMethodID").ToString() + "," + Localization.CurrencyStringForDBWithoutExchangeRate(Charge) + ")");
							}
						}
					}
				}

				// update existing rows:
				for(int i = 0; i <= Request.Form.Count - 1; i++)
				{
					String FieldName = Request.Form.Keys[i];
					if(FieldName.IndexOf("_0_") == -1 && FieldName != "Low_0" && FieldName != "High_0" && FieldName.IndexOf("_vldt") == -1 && (FieldName.IndexOf("Rate_") != -1 || FieldName.IndexOf("Low_") != -1 || FieldName.IndexOf("High_") != -1))
					{
						decimal FieldVal = CommonLogic.FormUSDecimal(FieldName);
						// this field should be processed
						String[] Parsed = FieldName.Split('_');
						if(FieldName.IndexOf("Rate_") != -1)
						{
							// update shipping costs:
							DB.ExecuteSQL("insert into ShippingByTotal(RowGUID,LowValue,HighValue,ShippingMethodID,ShippingCharge) values(" + DB.SQuote(Parsed[1]) + "," + Localization.CurrencyStringForDBWithoutExchangeRate(CommonLogic.FormUSDecimal("Low_" + Parsed[1])) + "," + Localization.CurrencyStringForDBWithoutExchangeRate(CommonLogic.FormUSDecimal("High_" + Parsed[1])) + "," + Parsed[2] + "," + Localization.CurrencyStringForDBWithoutExchangeRate(FieldVal) + ")");
						}
					}
				}
				DB.ExecuteSQL("Update ShippingByTotal set HighValue=99999.99 where HighValue=0.0 and LowValue<>0.0");

                Response.Redirect("shipping.aspx");
			}

			if(CommonLogic.FormBool("IsSubmitByTotalByPercent"))
			{
				if(EditGUID.Length != 0)
				{
					DB.ExecuteSQL("delete from ShippingByTotalByPercent where RowGUID=" + DB.SQuote(EditGUID));
				}

				// check for new row addition:
				Decimal Low0 = CommonLogic.FormUSDecimal("Low_0");
				Decimal High0 = CommonLogic.FormUSDecimal("High_0");
				Decimal Minimum0 = CommonLogic.FormUSDecimal("Minimum_0");
				Decimal Base0 = CommonLogic.FormUSDecimal("Base_0");
				String NewRowGUID = DB.GetNewGUID();

				if(Low0 != System.Decimal.Zero || High0 != System.Decimal.Zero)
				{
					// add the new row if necessary:
					using(SqlConnection sqlConnection = new SqlConnection(DB.GetDBConn()))
					{
						sqlConnection.Open();

						string getShippingMethodMapping = "exec aspdnsf_GetStoreShippingMethodMapping @StoreID = @storeId, @IsRTShipping = 0, @OnlyMapped = @filterByStore, @ExcludeNameLike = 'RealTime'";
						var getShippingMethodMappingParams = new[]
						{
							new SqlParameter("@storeId", StoreFilter),
							new SqlParameter("@filterByStore", MultiStoreFilteringEnabled),
						};

						using(IDataReader rs = DB.GetRS(getShippingMethodMapping, getShippingMethodMappingParams, sqlConnection))
						{
							while(rs.Read())
							{
								decimal PercentOfTotal = CommonLogic.FormUSDecimal("Rate_0_" + DB.RSFieldInt(rs, "ShippingMethodID").ToString());
								String sql = "insert into ShippingByTotalByPercent(RowGUID,LowValue,HighValue,ShippingMethodID,MinimumCharge,SurCharge,PercentOfTotal) values(" + DB.SQuote(NewRowGUID) + "," + Localization.CurrencyStringForDBWithoutExchangeRate(Low0) + "," + Localization.CurrencyStringForDBWithoutExchangeRate(High0) + "," + DB.RSFieldInt(rs, "ShippingMethodID").ToString() + "," + Localization.CurrencyStringForDBWithoutExchangeRate(Minimum0) + "," + Localization.CurrencyStringForDBWithoutExchangeRate(Base0) + "," + Localization.CurrencyStringForDBWithoutExchangeRate(PercentOfTotal) + ")";
								DB.ExecuteSQL(sql);
							}
						}
					}
				}

				// update existing rows:
				for(int i = 0; i <= Request.Form.Count - 1; i++)
				{
					String FieldName = Request.Form.Keys[i];
					if(FieldName.IndexOf("_0_") == -1 && FieldName != "Low_0" && FieldName != "High_0" && FieldName.IndexOf("_vldt") == -1 && (FieldName.IndexOf("Rate_") != -1 || FieldName.IndexOf("Low_") != -1 || FieldName.IndexOf("High_") != -1))
					{
						decimal FieldVal = CommonLogic.FormUSDecimal(FieldName);
						// this field should be processed
						String[] Parsed = FieldName.Split('_');
						if(FieldName.IndexOf("Rate_") != -1)
						{
							// update shipping costs:
							String sql = "insert into ShippingByTotalByPercent(RowGUID,LowValue,HighValue,ShippingMethodID,MinimumCharge,SurCharge,PercentOfTotal) values(" + DB.SQuote(Parsed[1]) + "," + Localization.CurrencyStringForDBWithoutExchangeRate(CommonLogic.FormUSDecimal("Low_" + Parsed[1])) + "," + Localization.CurrencyStringForDBWithoutExchangeRate(CommonLogic.FormUSDecimal("High_" + Parsed[1])) + "," + Parsed[2] + "," + Localization.CurrencyStringForDBWithoutExchangeRate(CommonLogic.FormUSDecimal("Minimum_" + Parsed[1])) + "," + Localization.CurrencyStringForDBWithoutExchangeRate(CommonLogic.FormUSDecimal("Base_" + Parsed[1])) + "," + Localization.CurrencyStringForDBWithoutExchangeRate(FieldVal) + ")";
							DB.ExecuteSQL(sql);
						}
					}
				}
				DB.ExecuteSQL("Update ShippingByTotalByPercent set HighValue=99999.99 where HighValue=0.0 and LowValue<>0.0");
			}

			if(CommonLogic.FormBool("IsSubmitByWeight"))
			{
				if(EditGUID.Length != 0)
				{
					DB.ExecuteSQL("delete from ShippingByWeight where RowGUID=" + DB.SQuote(EditGUID));
				}

				// check for new row addition:
				Decimal Low0 = CommonLogic.FormUSDecimal("Low_0");
				Decimal High0 = CommonLogic.FormUSDecimal("High_0");
				String NewRowGUID = DB.GetNewGUID();

				if(Low0 != 0.0M || High0 != 0.0M)
				{
					// add the new row if necessary:
					using(SqlConnection sqlConnection = new SqlConnection(DB.GetDBConn()))
					{
						sqlConnection.Open();

						string getShippingMethodMapping = "exec aspdnsf_GetStoreShippingMethodMapping @StoreID = @storeId, @IsRTShipping = 0, @OnlyMapped = @filterByStore, @ExcludeNameLike = 'RealTime'";
						var getShippingMethodMappingParams = new[]
						{
							new SqlParameter("@storeId", StoreFilter),
							new SqlParameter("@filterByStore", MultiStoreFilteringEnabled),
						};

						using(IDataReader rs = DB.GetRS(getShippingMethodMapping, getShippingMethodMappingParams, sqlConnection))
						{
							while(rs.Read())
							{
								string insertShippingByWeightRow = "insert into ShippingByWeight(RowGUID, LowValue, HighValue, ShippingMethodID, ShippingCharge, StoreID) values(@rowGuid, @lowAmount, @highAmount, @shippingMethodId, @shippingCharge, @storeId)";
								var insertShippingByWeightRowParams = new[]
								{
									new SqlParameter("@rowGuid", NewRowGUID),
									new SqlParameter("@lowAmount", Low0),
									new SqlParameter("@highAmount", High0),
									new SqlParameter("@shippingMethodId", DB.RSFieldInt(rs, "ShippingMethodID")),
									new SqlParameter("@shippingCharge", CommonLogic.FormUSDecimal("Rate_0_" + DB.RSFieldInt(rs, "ShippingMethodID").ToString())),
									new SqlParameter("@storeId", StoreFilter),
								};

								// Can't reuse the existing connection because the DataReader is still open
								DB.ExecuteSQL(insertShippingByWeightRow, insertShippingByWeightRowParams);
							}
						}
					}
				}

				// update existing rows:
				for(int i = 0; i <= Request.Form.Count - 1; i++)
				{
					String FieldName = Request.Form.Keys[i];
					if(FieldName.IndexOf("_0_") == -1 && FieldName != "Low_0" && FieldName != "High_0" && FieldName.IndexOf("_vldt") == -1 && (FieldName.IndexOf("Rate_") != -1 || FieldName.IndexOf("Low_") != -1 || FieldName.IndexOf("High_") != -1))
					{
						decimal FieldVal = CommonLogic.FormUSDecimal(FieldName);
						// this field should be processed
						String[] Parsed = FieldName.Split('_');
						if(FieldName.IndexOf("Rate_") != -1)
						{
							// update shipping costs:
							DB.ExecuteSQL("insert into ShippingByWeight(RowGUID,LowValue,HighValue,ShippingMethodID,ShippingCharge) values(" + DB.SQuote(Parsed[1]) + "," + Localization.DecimalStringForDB(CommonLogic.FormUSDecimal("Low_" + Parsed[1])) + "," + Localization.DecimalStringForDB(CommonLogic.FormUSDecimal("High_" + Parsed[1])) + "," + Parsed[2] + "," + Localization.CurrencyStringForDBWithoutExchangeRate(FieldVal) + ")");
						}
					}
				}
				DB.ExecuteSQL("Update ShippingByWeight set HighValue=99999.99 where HighValue=0.0 and LowValue<>0.0");
                Response.Redirect("shipping.aspx");
			}

			if(CommonLogic.FormBool("IsSubmitWeightByZone"))
			{
				int ShippingMethodID = CommonLogic.FormUSInt("ShippingMethodID");
				if(ShippingMethodID == 0)
				{
					ShippingMethodID = CommonLogic.QueryStringUSInt("ShippingMethodID");
				}
				if(UpdateGUID.Length != 0)
				{
					DB.ExecuteSQL("delete from ShippingWeightByZone where ShippingMethodID=" + ShippingMethodID.ToString() + " and RowGUID=" + DB.SQuote(UpdateGUID));
				}
				if(DeleteGUID.Length != 0)
				{
					DB.ExecuteSQL("delete from ShippingWeightByZone where ShippingMethodID=" + ShippingMethodID.ToString() + " and RowGUID=" + DB.SQuote(DeleteGUID));
				}

				// check for new row addition:
				Decimal Low0 = CommonLogic.FormUSDecimal("Low_0");
				Decimal High0 = CommonLogic.FormUSDecimal("High_0");
				String NewRowGUID = DB.GetNewGUID();

				if(Low0 != 0.0M || High0 != 0.0M)
				{
					// add the new row if necessary:
					using(SqlConnection con = new SqlConnection(DB.GetDBConn()))
					{
						con.Open();
						using(IDataReader rs = DB.GetRS("select * from ShippingZone  with (NOLOCK)  where Deleted=0 order by DisplayOrder,Name", con))
						{
							while(rs.Read())
							{
								Decimal Charge = CommonLogic.FormUSDecimal("Rate_0_" + DB.RSFieldInt(rs, "ShippingZoneID").ToString());
								DB.ExecuteSQL("insert into ShippingWeightByZone(RowGUID,ShippingMethodID,LowValue,HighValue,ShippingZoneID,ShippingCharge) values(" + DB.SQuote(NewRowGUID) + "," + ShippingMethodID.ToString() + "," + Localization.DecimalStringForDB(Low0) + "," + Localization.DecimalStringForDB(High0) + "," + DB.RSFieldInt(rs, "ShippingZoneID").ToString() + "," + Localization.CurrencyStringForDBWithoutExchangeRate(Charge) + ")");
							}
						}
					}
				}

				// update existing rows:
				for(int i = 0; i <= Request.Form.Count - 1; i++)
				{
					String FieldName = Request.Form.Keys[i];
					if(FieldName.IndexOf("_0_") == -1 && FieldName != "Low_0" && FieldName != "High_0" && FieldName.IndexOf("_vldt") == -1 && (FieldName.IndexOf("Rate_") != -1 || FieldName.IndexOf("Low_") != -1 || FieldName.IndexOf("High_") != -1))
					{
						Decimal FieldVal = CommonLogic.FormUSDecimal(FieldName);
						// this field should be processed
						String[] Parsed = FieldName.Split('_');
						if(FieldName.IndexOf("Rate_") != -1)
						{
							// update shipping costs:
							DB.ExecuteSQL("insert into ShippingWeightByZone(RowGUID,ShippingMethodID,LowValue,HighValue,ShippingZoneID,ShippingCharge) values(" + DB.SQuote(Parsed[1]) + "," + ShippingMethodID.ToString() + "," + Localization.DecimalStringForDB(CommonLogic.FormUSDecimal("Low_" + Parsed[1])) + "," + Localization.DecimalStringForDB(CommonLogic.FormUSDecimal("High_" + Parsed[1])) + "," + Parsed[2] + "," + Localization.CurrencyStringForDBWithoutExchangeRate(FieldVal) + ")");
						}
					}
				}
				DB.ExecuteSQL("Update ShippingWeightByZone set HighValue=99999.99 where HighValue=0.0 and LowValue<>0.0");
                Response.Redirect("shipping.aspx");
			}

			if(CommonLogic.FormBool("IsSubmitTotalByZone"))
			{
				int ShippingMethodID = CommonLogic.FormUSInt("ShippingMethodID");
				if(ShippingMethodID == 0)
				{
					ShippingMethodID = CommonLogic.QueryStringUSInt("ShippingMethodID");
				}
				if(UpdateGUID.Length != 0)
				{
					DB.ExecuteSQL("delete from ShippingTotalByZone where ShippingMethodID=" + ShippingMethodID.ToString() + " and RowGUID=" + DB.SQuote(UpdateGUID));
				}
				if(DeleteGUID.Length != 0)
				{
					DB.ExecuteSQL("delete from ShippingTotalByZone where ShippingMethodID=" + ShippingMethodID.ToString() + " and RowGUID=" + DB.SQuote(DeleteGUID));
				}

				// check for new row addition:
				Decimal Low0 = CommonLogic.FormUSDecimal("Low_0");
				Decimal High0 = CommonLogic.FormUSDecimal("High_0");
				String NewRowGUID = DB.GetNewGUID();

				if(Low0 != 0.0M || High0 != 0.0M)
				{
					// add the new row if necessary:
					using(SqlConnection con = new SqlConnection(DB.GetDBConn()))
					{
						con.Open();
						using(IDataReader rs = DB.GetRS("select * from ShippingZone  with (NOLOCK)  where Deleted=0 order by DisplayOrder,Name", con))
						{
							while(rs.Read())
							{
								Decimal Charge = CommonLogic.FormUSDecimal("Rate_0_" + DB.RSFieldInt(rs, "ShippingZoneID").ToString());
								DB.ExecuteSQL("insert into ShippingTotalByZone(RowGUID,ShippingMethodID,LowValue,HighValue,ShippingZoneID,ShippingCharge) values(" + DB.SQuote(NewRowGUID) + "," + ShippingMethodID.ToString() + "," + Localization.CurrencyStringForDBWithoutExchangeRate(Low0) + "," + Localization.CurrencyStringForDBWithoutExchangeRate(High0) + "," + DB.RSFieldInt(rs, "ShippingZoneID").ToString() + "," + Localization.CurrencyStringForDBWithoutExchangeRate(Charge) + ")");
							}
						}
					}
				}

				// update existing rows:
				for(int i = 0; i <= Request.Form.Count - 1; i++)
				{
					String FieldName = Request.Form.Keys[i];
					if(FieldName.IndexOf("_0_") == -1 && FieldName != "Low_0" && FieldName != "High_0" && FieldName.IndexOf("_vldt") == -1 && (FieldName.IndexOf("Rate_") != -1 || FieldName.IndexOf("Low_") != -1 || FieldName.IndexOf("High_") != -1))
					{
						Decimal FieldVal = CommonLogic.FormUSDecimal(FieldName);
						// this field should be processed
						String[] Parsed = FieldName.Split('_');
						if(FieldName.IndexOf("Rate_") != -1)
						{
							// update shipping costs:
							DB.ExecuteSQL("insert into ShippingTotalByZone(RowGUID,ShippingMethodID,LowValue,HighValue,ShippingZoneID,ShippingCharge) values(" + DB.SQuote(Parsed[1]) + "," + ShippingMethodID.ToString() + "," + Localization.CurrencyStringForDBWithoutExchangeRate(CommonLogic.FormUSDecimal("Low_" + Parsed[1])) + "," + Localization.CurrencyStringForDBWithoutExchangeRate(CommonLogic.FormUSDecimal("High_" + Parsed[1])) + "," + Parsed[2] + "," + Localization.CurrencyStringForDBWithoutExchangeRate(FieldVal) + ")");
						}
					}
				}
				DB.ExecuteSQL("Update ShippingTotalByZone set HighValue=99999.99 where HighValue=0.0 and LowValue<>0.0");
                Response.Redirect("shipping.aspx");
			}

			if(CommonLogic.QueryStringCanBeDangerousContent("deletebytotalid").Length != 0)
			{
				DB.ExecuteSQL("delete from ShippingByTotal where RowGUID=" + DB.SQuote(CommonLogic.QueryStringCanBeDangerousContent("DeleteByTotalID")));
			}

			if(CommonLogic.QueryStringCanBeDangerousContent("deletebytotalbypercentid").Length != 0)
			{
				DB.ExecuteSQL("delete from ShippingByTotalByPercent where RowGUID=" + DB.SQuote(CommonLogic.QueryStringCanBeDangerousContent("DeleteByTotalByPercentID")));
			}

			if(CommonLogic.QueryStringCanBeDangerousContent("deletebyWeightid").Length != 0)
			{
				DB.ExecuteSQL("delete from ShippingByWeight where RowGUID=" + DB.SQuote(CommonLogic.QueryStringCanBeDangerousContent("DeleteByWeightID")));
			}
			if(CommonLogic.QueryStringCanBeDangerousContent("deleteWeightByZoneid").Length != 0)
			{
				DB.ExecuteSQL("delete from ShippingWeightByZone where ShippingMethodID=" + CommonLogic.QueryStringUSInt("ShippingMethodID").ToString() + " and RowGUID=" + DB.SQuote(CommonLogic.QueryStringCanBeDangerousContent("DeleteWeightByZoneID")));
			}

			if(CommonLogic.QueryStringCanBeDangerousContent("deleteTotalByZoneid").Length != 0)
			{
				DB.ExecuteSQL("delete from ShippingTotalByZone where ShippingMethodID=" + CommonLogic.QueryStringUSInt("ShippingMethodID").ToString() + " and RowGUID=" + DB.SQuote(CommonLogic.QueryStringCanBeDangerousContent("DeleteTotalByZoneID")));
			}

			html.Append("<script type=\"text/javascript\">\n");
			html.Append("function ShippingForm_Validator(theForm)\n");
			html.Append("{\n");
			html.Append("submitonce(theForm);\n");
			html.Append("return (true);\n");
			html.Append("}\n");
			html.Append("function FixedRateForm_Validator(theForm)\n");
			html.Append("{\n");
			html.Append("submitonce(theForm);\n");
			html.Append("return (true);\n");
			html.Append("}\n");
			html.Append("function FixedPercentOfTotalForm_Validator(theForm)\n");
			html.Append("{\n");
			html.Append("submitonce(theForm);\n");
			html.Append("return (true);\n");
			html.Append("}\n");
			html.Append("function ByTotalForm_Validator(theForm)\n");
			html.Append("{\n");
			html.Append("submitonce(theForm);\n");
			html.Append("return (true);\n");
			html.Append("}\n");
			html.Append("function ByTotalByPercentForm_Validator(theForm)\n");
			html.Append("{\n");
			html.Append("submitonce(theForm);\n");
			html.Append("return (true);\n");
			html.Append("}\n");
			html.Append("function ByWeightForm_Validator(theForm)\n");
			html.Append("{\n");
			html.Append("submitonce(theForm);\n");
			html.Append("return (true);\n");
			html.Append("}\n");
			html.Append("function WeightByZoneForm_Validator(theForm)\n");
			html.Append("{\n");
			html.Append("submitonce(theForm);\n");
			html.Append("return (true);\n");
			html.Append("}\n");
			html.Append("function TotalByZoneForm_Validator(theForm)\n");
			html.Append("{\n");
			html.Append("submitonce(theForm);\n");
			html.Append("return (true);\n");
			html.Append("}\n");
			html.Append("</script>\n");

			Shipping.ShippingCalculationEnum ShipCalcID = 0;

            html.Append("<script type=\"text/javascript\" src=\"./scripts/formValidate.js\"></script>\n");
			html.Append("<form action=\"" + AppLogic.AdminLinkUrl("shipping.aspx") + "?StoreId=" + StoreFilter + "\" method=\"post\" id=\"ShippingForm\" name=\"aspnetForm\" onsubmit=\"return (validateForm(this) && ShippingForm_Validator(this))\" onReset=\"return confirm('Do you want to reset all fields to their starting values?');\">\n");

			html.AppendFormat("<input type=\"hidden\" name=\"StoreFilter\" id=\"StoreFilter\" value=\"{0}\" />\n", StoreFilter);

			if(MultiStoreFilteringEnabled)
			{
				html.AppendLine();

                var aspnetPostbackScript = @"<script type=""text/javascript"">
											//<![CDATA[
											function postback() {
                                                var e = document.getElementById('cboStores');
                                                window.location.href = 'shipping.aspx?StoreId=' + e.options[e.selectedIndex].value;
											}
											//]]>
											</script>";

				html.Append(aspnetPostbackScript);

				html.AppendLine();
                html.AppendFormat("Store: <select id=\"cboStores\" name=\"cboStores\" onchange=\"javascript:postback()\" >\n");

				foreach(var store in Stores)
				{
					bool shouldBeSelected = store.StoreID == StoreFilter;
					html.AppendFormat("    <option value=\"{0}\" {2}>{1}</option>\n", store.StoreID, store.Name, shouldBeSelected ? "selected=\"selected\"" : string.Empty);
				}

				html.AppendFormat("</select>\n");

				html.AppendFormat("&nbsp;<span>Multi-Store Filtering: {0}", MultiStoreFilteringEnabled ? "On" : "Off");

				html.AppendLine();
			}

			html.Append("<input type=\"hidden\" name=\"IsSubmitCalculationID\" value=\"true\">\n");

			string query = string.Empty;
			query = "select * from ShippingCalculation   with (NOLOCK)  order by shippingcalculationid";

			ShipCalcID = (Shipping.ShippingCalculationEnum)EnsureStoreShippingCalculation();

			using(SqlConnection con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(IDataReader rs = DB.GetRS(query, con))
				{
					while(rs.Read())
					{
						int calcId = rs.FieldInt("ShippingCalculationID");
						bool defaultSelected = (calcId == (int)ShipCalcID);

						html.Append("<p><input type=\"radio\" name=\"ShippingCalculationID\" value=\"" + calcId.ToString() + "\" " + CommonLogic.IIF(defaultSelected, " checked ", "") + "><b>" + DB.RSFieldByLocale(rs, "Name", LocaleSetting) + "</b></p>\n");
						if(DB.RSFieldBool(rs, "Selected"))
						{
							ShipCalcID = (Shipping.ShippingCalculationEnum)DB.RSFieldInt(rs, "ShippingCalculationID");
						}
					}
				}
			}

			html.Append("<input class=\"normalButtons\" type=\"submit\" value=\"Set As Active Shipping Calculation Method\" name=\"btnSubmit\">\n");
			html.Append("</form>\n");

			switch(ShipCalcID)
			{
				case Shipping.ShippingCalculationEnum.CalculateShippingByWeight:
					RenderCalculateShippingByWeightForm(html, EditGUID);
					break;

				case Shipping.ShippingCalculationEnum.CalculateShippingByTotal:
					RenderCalculateShippingByTotalForm(html, EditGUID);
					break;

				case Shipping.ShippingCalculationEnum.CalculateShippingByTotalByPercent:
					RenderCalculateShippingByTotalByPercentForm(html, EditGUID);
					break;

				case Shipping.ShippingCalculationEnum.UseFixedPrice:
					RenderUseFixedPriceForm(html, EditGUID);
					break;

				case Shipping.ShippingCalculationEnum.AllOrdersHaveFreeShipping:
					break;
				
				case Shipping.ShippingCalculationEnum.UseFixedPercentageOfTotal:
					RenderUseFixedPercentageOfTotalForm(html, EditGUID);
					break;

				case Shipping.ShippingCalculationEnum.UseIndividualItemShippingCosts:
					html.Append("<p>Set Your shipping costs in each product variant.</p>");
					break;

				case Shipping.ShippingCalculationEnum.UseRealTimeRates:
					{
						html.Append("<p align=\"left\">Real Time I/F will be used for rates, based on order weights. Remember to set your AppConfig:RTShipping parameters accordingly! Current settings are shown below.<br/><br/>");
						html.Append("<a href=\"" + AppLogic.AdminLinkUrl("appconfig.aspx") + "?searchfor=RTShipping\"><b>Click here</b></a> to edit these settings.<br/><br/>");

						using(SqlConnection con = new SqlConnection(DB.GetDBConn()))
						{
							con.Open();
							using(IDataReader rss = DB.GetRS("Select distinct Name from appconfig with (NOLOCK)  where name like " + DB.SQuote("RTShipping%") + " order by name", con))
							{
								while(rss.Read())
								{
									html.Append(DB.RSField(rss, "Name") + "=" + AppLogic.GetAppConfigRouted(DB.RSField(rss, "Name"), StoreFilter).ConfigValue + "<br/>");
								}
							}
						}
						html.Append("</p>");
					}
					break;

				case Shipping.ShippingCalculationEnum.CalculateShippingByWeightAndZone:
					RenderCalculateShippingByWeightAndZoneForm(html, EditGUID);
					break;

				case Shipping.ShippingCalculationEnum.CalculateShippingByTotalAndZone:
					RenderCalculateShippingByTotalAndZoneForm(html, EditGUID);
					break;
			}

			ltContent.Text = html.ToString();
		}

		private void RenderCalculateShippingByWeightForm(StringBuilder html, String EditGUID)
		{
			html.Append("<hr size=1>");

			html.Append("<p><b>ACTIVE RATE TABLE FOR: CALCULATE SHIPPING BY ORDER WEIGHT:</b></p>\n");
            html.Append("<script type=\"text/javascript\" src=\"./scripts/formValidate.js\"></script>\n");
            html.Append("<form action=\"" + AppLogic.AdminLinkUrl("shipping.aspx") + "?StoreId=" + StoreFilter + "&EditGUID=" + EditGUID + "\" method=\"post\" id=\"ByWeightForm\" name=\"ByWeightForm\" onsubmit=\"return (validateForm(this) && ByWeightForm_Validator(this))\" onReset=\"return confirm('Do you want to reset all fields to their starting values?');\">\n");
			html.Append("<input type=\"hidden\" name=\"IsSubmitByWeight\" value=\"true\">\n");

			using(DataTable dtWeight = new DataTable())
			{
				using(SqlConnection sqlConnection = new SqlConnection(DB.GetDBConn()))
				{
					sqlConnection.Open();

					string getShippingMethodMapping = "exec aspdnsf_GetStoreShippingMethodMapping @StoreID = @storeId, @IsRTShipping = 0, @OnlyMapped = @filterByStore";
					var getShippingMethodMappingParams = new[]
					{
						new SqlParameter("@storeId", StoreFilter),
						new SqlParameter("@filterByStore", MultiStoreFilteringEnabled),
					};

					using(IDataReader rs = DB.GetRS(getShippingMethodMapping, getShippingMethodMappingParams, sqlConnection))
						dtWeight.Load(rs);
				}

				if(dtWeight.Rows.Count > 0)
				{
					html.Append("<table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\">\n");
					html.Append("<tr class=\"tablenormal\"><td colspan=2 align=\"left\" valign=\"middle\">Order Weight (in " + Localization.WeightUnits() + ")</td><td align=\"left\" valign=\"middle\" colspan=" + dtWeight.Rows.Count.ToString() + ">Shipping Charge By Weight</td><td align=\"left\" valign=\"middle\">&nbsp;</td><td>&nbsp;</td></tr>\n");
					html.Append("<tr>\n");
					html.Append("<td align=\"left\" valign=\"middle\">Low</td>\n");
					html.Append("<td align=\"left\" valign=\"middle\">High</td>\n");
					foreach(DataRow row in dtWeight.Rows)
					{
						html.Append("<td align=\"left\">" + DB.RowFieldByLocale(row, "Name", LocaleSetting) + "</td>\n");
					}
					html.Append("<td align=\"left\" valign=\"middle\">Edit</td>\n");
					html.Append("<td align=\"left\" valign=\"middle\">Delete</td>\n");
					html.Append("</tr>\n");

					var sqlShipping = string.Empty;
					sqlShipping =	@"SELECT DISTINCT sw.RowGUID, sw.LowValue, sw.HighValue
									FROM ShippingByWeight sw WITH (NOLOCK)
									INNER JOIN ShippingMethod sm WITH (NOLOCK) ON sm.ShippingMethodid = sw.ShippingMethodId
									ORDER BY LowValue";

					using(SqlConnection con = new SqlConnection(DB.GetDBConn()))
					{
						con.Open();
						using(IDataReader rs = DB.GetRS(sqlShipping, con))
						{
							while(rs.Read())
							{
								bool EditRow = (EditGUID == DB.RSFieldGUID(rs, "RowGUID"));
								html.Append("<tr>\n");
								html.Append("<td align=\"left\" valign=\"middle\">\n");
								if(EditRow)
								{
									html.Append("<input maxLength=\"10\" size=\"10\" name=\"Low_" + DB.RSFieldGUID(rs, "RowGUID") + "\" value=\"" + Localization.DecimalStringForDB(DB.RSFieldDecimal(rs, "LowValue")) + "\">\n");
									html.Append("<input type=\"hidden\" name=\"Low_" + DB.RSFieldGUID(rs, "RowGUID") + "_vldt\" value=\"[number][blankalert=Please enter starting order amount][invalidalert=Please enter a weight value]\">\n");
								}
								else
								{
									html.Append(Localization.DecimalStringForDB(DB.RSFieldDecimal(rs, "LowValue")));
								}
								html.Append("</td>\n");
								html.Append("<td align=\"left\" valign=\"middle\">\n");
								if(EditRow)
								{
									html.Append("<input maxLength=\"10\" size=\"10\" name=\"High_" + DB.RSFieldGUID(rs, "RowGUID") + "\" value=\"" + Localization.DecimalStringForDB(DB.RSFieldDecimal(rs, "HighValue")) + "\">\n");
									html.Append("<input type=\"hidden\" name=\"High_" + DB.RSFieldGUID(rs, "RowGUID") + "_vldt\" value=\"[number][blankalert=Please enter ending order amount][invalidalert=Please enter a weight value]\">\n");
								}
								else
								{
									html.Append(Localization.DecimalStringForDB(DB.RSFieldDecimal(rs, "HighValue")));
								}
								html.Append("</td>\n");

								foreach(DataRow row in dtWeight.Rows)
								{
									html.Append("<td align=\"left\" valign=\"middle\">\n");
									if(EditRow)
									{
										html.Append("<input maxLength=\"10\" size=\"10\" name=\"Rate_" + DB.RSFieldGUID(rs, "RowGUID") + "_" + DB.RowFieldInt(row, "ShippingMethodID").ToString() + "\" value=\"" + Localization.CurrencyStringForDBWithoutExchangeRate(Shipping.GetShipByWeightCharge(DB.RowFieldInt(row, "ShippingMethodID"), DB.RSFieldGUID(rs, "RowGUID"))) + "\">\n");
										html.Append("<input type=\"hidden\" name=\"Rate_" + DB.RSFieldGUID(rs, "RowGUID") + "_" + DB.RowFieldInt(row, "ShippingMethodID").ToString() + "_vldt\" value=\"[number][blankalert=Please enter the shipping cost][invalidalert=Please enter a money value, WITHOUT the dollar sign]\">\n");
									}
									else
									{
										html.Append(Localization.CurrencyStringForDBWithoutExchangeRate(Shipping.GetShipByWeightCharge(DB.RowFieldInt(row, "ShippingMethodID"), DB.RSFieldGUID(rs, "RowGUID"))));
									}
									html.Append("</td>\n");
								}
								if(EditRow)
								{
									html.Append("<td align=\"left\" valign=\"middle\">");
									html.Append("<input class=\"normalButtons\" type=\"submit\" value=\"Update\" name=\"btnSubmit\">\n");
									html.Append("</td>");
								}
								else
								{
                                    html.Append("<td align=\"left\" valign=\"middle\"><input class=\"normalButtons\" type=\"Button\" name=\"Edit\" value=\"Edit\" onClick=\"self.location='" + AppLogic.AdminLinkUrl("shipping.aspx") + "?StoreId=" + StoreFilter + "&EditGUID=" + DB.RSFieldGUID(rs, "RowGUID") + "'\"></td>\n");
								}
                                html.Append("<td align=\"left\" valign=\"middle\"><input class=\"normalButtons\" type=\"Button\" name=\"Delete\" value=\"Delete\" onClick=\"self.location='" + AppLogic.AdminLinkUrl("shipping.aspx") + "?StoreId=" + StoreFilter + "&DeleteByWeightID=" + DB.RSFieldGUID(rs, "RowGUID") + "'\"></td>\n");
								html.Append("</tr>\n");
							}
						}
					}

					// add new row:
					html.Append("<tr>\n");
					html.Append("<td align=\"left\" valign=\"middle\">\n");
					html.Append("<input maxLength=\"10\" size=\"10\" name=\"Low_0\" \">\n");
					html.Append("<input type=\"hidden\" name=\"Low_0_vldt\" value=\"[number][blankalert=Please enter starting order amount][invalidalert=Please enter a money value, WITHOUT the dollar sign]\">\n");
					html.Append("</td>\n");
					html.Append("<td align=\"left\" valign=\"middle\">\n");
					html.Append("<input maxLength=\"10\" size=\"10\" name=\"High_0\" >\n");
					html.Append("<input type=\"hidden\" name=\"High_0_vldt\" value=\"[number][blankalert=Please enter ending order amount][invalidalert=Please enter a money value, WITHOUT the dollar sign]\">\n");
					html.Append("</td>\n");

					foreach(DataRow row in dtWeight.Rows)
					{
						html.Append("<td align=\"left\" valign=\"middle\">\n");
						html.Append("<input maxLength=\"10\" size=\"10\" name=\"Rate_0_" + DB.RowFieldInt(row, "ShippingMethodID").ToString() + "\">\n");
						html.Append("<input type=\"hidden\" name=\"Rate_0_" + DB.RowFieldInt(row, "ShippingMethodID").ToString() + "_vldt\" value=\"[number][blankalert=Please enter the shipping cost][invalidalert=Please enter a money value, WITHOUT the dollar sign]\">\n");
						html.Append("</td>\n");
					}
					html.Append("<td align=\"left\" valign=\"middle\">");
					html.Append("<input class=\"normalButtons\" type=\"submit\" value=\"Add New Row\" name=\"btnSubmit\">\n");
					html.Append("</td>\n");
					html.Append("<td align=\"left\" valign=\"middle\">&nbsp;</td>");
					html.Append("</tr>\n");
					html.Append("</table>\n");
				}
			}

			html.Append("</form>\n");
		}

		private void RenderCalculateShippingByTotalForm(StringBuilder html, String EditGUID)
		{
			html.Append("<hr size=1>");

			html.Append("<p><b>ACTIVE RATE TABLE FOR: CALCULATE SHIPPING BY ORDER TOTAL:</b></p>\n");
            html.Append("<script type=\"text/javascript\" src=\"./scripts/formValidate.js\"></script>\n");
            html.Append("<form action=\"" + AppLogic.AdminLinkUrl("shipping.aspx") + "?StoreId=" + StoreFilter + "&EditGUID=" + EditGUID + "\" method=\"post\" id=\"ByTotalForm\" name=\"ByTotalForm\" onsubmit=\"return (validateForm(this) && ByTotalForm_Validator(this))\" onReset=\"return confirm('Do you want to reset all fields to their starting values?');\">\n");
			html.Append("<input type=\"hidden\" name=\"IsSubmitByTotal\" value=\"true\">\n");

			using(DataTable dtTotal = new DataTable())
			{
				using(SqlConnection sqlConnection = new SqlConnection(DB.GetDBConn()))
				{
					sqlConnection.Open();

					string getShippingMethodMapping = "exec aspdnsf_GetStoreShippingMethodMapping @StoreID = @storeId, @IsRTShipping = 0, @OnlyMapped = @filterByStore";
					var getShippingMethodMappingParams = new[]
					{
						new SqlParameter("@storeId", StoreFilter),
						new SqlParameter("@filterByStore", MultiStoreFilteringEnabled),
					};

					using(IDataReader rs = DB.GetRS(getShippingMethodMapping, getShippingMethodMappingParams, sqlConnection))
						dtTotal.Load(rs);
				}

				if(dtTotal.Rows.Count == 0)
				{
					html.Append("No shipping methods are defined. <a href=\"" + AppLogic.AdminLinkUrl("shippingmethods.aspx") + "\"><b>Click here</b></a> to define your shipping methods");
				}
				else
				{
					html.Append("<table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\">\n");
					html.Append("<tr class=\"tablenormal\"><td colspan=2 align=\"left\" valign=\"middle\">Order Total (in your currency)</td><td align=\"left\" valign=\"middle\" colspan=" + dtTotal.Rows.Count.ToString() + ">Shipping Charge By Total</td><td>&nbsp;</td><td>&nbsp;</td></tr>\n");
					html.Append("<tr>\n");
					html.Append("<td align=\"left\" valign=\"middle\">Low</td>\n");
					html.Append("<td align=\"left\" valign=\"middle\">High</td>\n");
					foreach(DataRow row in dtTotal.Rows)
					{
						html.Append("<td align=\"left\" valign=\"middle\">" + DB.RowFieldByLocale(row, "Name", LocaleSetting) + "</td>\n");
					}
					html.Append("<td align=\"left\" valign=\"middle\">Edit</td>\n");
					html.Append("<td align=\"left\" valign=\"middle\">Delete</td>\n");
					html.Append("</tr>\n");

					var sqlShipping = string.Empty;
					if(!MultiStoreFilteringEnabled)
					{
						sqlShipping =	@"SELECT DISTINCT st.rowguid,st.lowvalue,st.highvalue 
										FROM ShippingByTotal st WITH (NOLOCK) 
										INNER JOIN ShippingMethod sm WITH (NOLOCK) ON sm.ShippingMethodid = st.ShippingMethodId                                         
										order by LowValue";
					}
					else
					{
						sqlShipping =	@"SELECT DISTINCT st.rowguid,st.lowvalue,st.highvalue 
										FROM ShippingByTotal st WITH (NOLOCK) 
										INNER JOIN ShippingMethod sm WITH (NOLOCK) ON sm.ShippingMethodid = st.ShippingMethodId 
										INNER JOIN ShippingMethodStore ssm WITH (NOLOCK) ON ssm.ShippingMethodid = sm.ShippingMethodId
										WHERE ssm.StoreId = " + StoreFilter.ToString() +
										@" order by LowValue";
					}

					using(SqlConnection con = new SqlConnection(DB.GetDBConn()))
					{
						con.Open();
						using(IDataReader rs = DB.GetRS(sqlShipping, con))
						{
							while(rs.Read())
							{
								bool EditRow = (EditGUID == DB.RSFieldGUID(rs, "RowGUID"));
								html.Append("<tr>\n");
								html.Append("<td align=\"center\">\n");
								if(EditRow)
								{
									html.Append("<input maxLength=\"10\" size=\"10\" name=\"Low_" + DB.RSFieldGUID(rs, "RowGUID") + "\" value=\"" + Localization.CurrencyStringForDBWithoutExchangeRate(DB.RSFieldDecimal(rs, "LowValue")) + "\">\n");
									html.Append("<input type=\"hidden\" name=\"Low_" + DB.RSFieldGUID(rs, "RowGUID") + "_vldt\" value=\"[number][blankalert=Please enter starting order amount][invalidalert=Please enter a money value, WITHOUT the dollar sign]\">\n");
								}
								else
								{
									html.Append(Localization.CurrencyStringForDBWithoutExchangeRate(DB.RSFieldDecimal(rs, "LowValue")));
								}
								html.Append("</td>\n");
								html.Append("<td align=\"left\" valign=\"middle\">\n");
								if(EditRow)
								{
									html.Append("<input maxLength=\"10\" size=\"10\" name=\"High_" + DB.RSFieldGUID(rs, "RowGUID") + "\" value=\"" + Localization.CurrencyStringForDBWithoutExchangeRate(DB.RSFieldDecimal(rs, "HighValue")) + "\">\n");
									html.Append("<input type=\"hidden\" name=\"High_" + DB.RSFieldGUID(rs, "RowGUID") + "_vldt\" value=\"[number][blankalert=Please enter ending order amount][invalidalert=Please enter a money value, WITHOUT the dollar sign]\">\n");
								}
								else
								{
									html.Append(Localization.CurrencyStringForDBWithoutExchangeRate(DB.RSFieldDecimal(rs, "HighValue")));
								}
								html.Append("</td>\n");
								foreach(DataRow row in dtTotal.Rows)
								{
									html.Append("<td align=\"center\">\n");
									if(EditRow)
									{
										html.Append("<input maxLength=\"10\" size=\"10\" name=\"Rate_" + DB.RSFieldGUID(rs, "RowGUID") + "_" + DB.RowFieldInt(row, "ShippingMethodID").ToString() + "\" value=\"" + Localization.CurrencyStringForDBWithoutExchangeRate(Shipping.GetShipByTotalCharge(DB.RowFieldInt(row, "ShippingMethodID"), DB.RSFieldGUID(rs, "RowGUID"))) + "\">\n");
										html.Append("<input type=\"hidden\" name=\"Rate_" + DB.RSFieldGUID(rs, "RowGUID") + "_" + DB.RowFieldInt(row, "ShippingMethodID").ToString() + "_vldt\" value=\"[number][blankalert=Please enter the shipping cost][invalidalert=Please enter a money value, WITHOUT the dollar sign]\">\n");
									}
									else
									{
										html.Append(Localization.CurrencyStringForDBWithoutExchangeRate(Shipping.GetShipByTotalCharge(DB.RowFieldInt(row, "ShippingMethodID"), DB.RSFieldGUID(rs, "RowGUID"))));
									}
									html.Append("</td>\n");
								}
								if(EditRow)
								{
									html.Append("<td align=\"left\" valign=\"middle\">");
                                    html.Append("<input class=\"normalButtons\" type=\"submit\" value=\"Update\" name=\"btnSubmit\">\n");
									html.Append("</td>");
								}
								else
								{
                                    html.Append("<td align=\"left\" valign=\"middle\"><input class=\"normalButtons\" type=\"Button\" name=\"Edit\" value=\"Edit\" onClick=\"self.location='" + AppLogic.AdminLinkUrl("shipping.aspx") + "?StoreId=" + StoreFilter + "&EditGUID=" + DB.RSFieldGUID(rs, "RowGUID") + "'\"></td>\n");
								}
                                html.Append("<td align=\"left\" valign=\"middle\"><input class=\"normalButtons\" type=\"Button\" name=\"Delete\" value=\"Delete\" onClick=\"self.location='" + AppLogic.AdminLinkUrl("shipping.aspx") + "?StoreId=" + StoreFilter + "&deleteByTotalid=" + DB.RSFieldGUID(rs, "RowGUID") + "'\"></td>\n");
								html.Append("</tr>\n");

							}
						}
					}
					// add new row:
					html.Append("<tr>\n");
					html.Append("<td align=\"left\" valign=\"middle\">\n");
					html.Append("<input maxLength=\"10\" size=\"10\" name=\"Low_0\" \">\n");
					html.Append("<input type=\"hidden\" name=\"Low_0_vldt\" value=\"[number][blankalert=Please enter starting order amount][invalidalert=Please enter a money value, WITHOUT the dollar sign]\">\n");
					html.Append("</td>\n");
					html.Append("<td align=\"left\" valign=\"middle\">\n");
					html.Append("<input maxLength=\"10\" size=\"10\" name=\"High_0\" >\n");
					html.Append("<input type=\"hidden\" name=\"High_0_vldt\" value=\"[number][blankalert=Please enter ending order amount][invalidalert=Please enter a money value, WITHOUT the dollar sign]\">\n");
					html.Append("</td>\n");

					foreach(DataRow row in dtTotal.Rows)
					{
						html.Append("<td align=\"left\" valign=\"middle\">\n");
						html.Append("<input maxLength=\"10\" size=\"10\" name=\"Rate_0_" + DB.RowFieldInt(row, "ShippingMethodID").ToString() + "\">\n");
						html.Append("<input type=\"hidden\" name=\"Rate_0_" + DB.RowFieldInt(row, "ShippingMethodID").ToString() + "_vldt\" value=\"[number][blankalert=Please enter the shipping cost][invalidalert=Please enter a money value, WITHOUT the dollar sign]\">\n");
						html.Append("</td>\n");
					}
					html.Append("<td align=\"left\" valign=\"middle\">");
					html.Append("<input class=\"normalButtons\" type=\"submit\" value=\"Add New Row\" name=\"btnSubmit\">\n");
					html.Append("</td>\n");
					html.Append("<td>&nbsp;</td>");
					html.Append("</tr>\n");
					html.Append("</table>\n");
				}
			}

			html.Append("</form>\n");
		}

		private void RenderCalculateShippingByTotalByPercentForm(StringBuilder html, String EditGUID)
		{
			html.Append("<hr size=1>");

			html.Append("<p><b>ACTIVE RATE TABLE FOR: CALCULATE SHIPPING BY ORDER TOTAL BY PERCENT:</b></p>\n");
            html.Append("<script type=\"text/javascript\" src=\"./scripts/formValidate.js\"></script>\n");
            html.Append("<form action=\"" + AppLogic.AdminLinkUrl("shipping.aspx") + "?StoreId=" + StoreFilter + "&EditGUID=" + "\" method=\"post\" id=\"ByTotalByPercentForm\" name=\"ByTotalByPercentForm\" onsubmit=\"return (validateForm(this) && ByTotalByPercentForm_Validator(this))\" onReset=\"return confirm('Do you want to reset all fields to their starting values?');\">\n");
			html.Append("<input type=\"hidden\" name=\"IsSubmitByTotalByPercent\" value=\"true\">\n");

			using(DataTable dtTotalByPercent = new DataTable())
			{
				using(SqlConnection sqlConnection = new SqlConnection(DB.GetDBConn()))
				{
					sqlConnection.Open();

					string getShippingMethodMapping = "exec aspdnsf_GetStoreShippingMethodMapping @StoreID = @storeId, @IsRTShipping = 0, @OnlyMapped = @filterByStore";
					var getShippingMethodMappingParams = new[]
					{
						new SqlParameter("@storeId", StoreFilter),
						new SqlParameter("@filterByStore", MultiStoreFilteringEnabled),
					};

					using(IDataReader rs = DB.GetRS(getShippingMethodMapping, getShippingMethodMappingParams, sqlConnection))
						dtTotalByPercent.Load(rs);
				}

				if(dtTotalByPercent.Rows.Count == 0)
				{
					html.Append("No shipping methods are defined. <a href=\"" + AppLogic.AdminLinkUrl("shippingmethods.aspx") + "\"><b>Click here</b></a> to define your shipping methods");
				}
				else
				{
					html.Append("<table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"1\">\n");
					html.Append("<tr class=\"tablenormal\"><td colspan=2 align=\"center\" valign=\"middle\"><b>Order Total (xx.xx)</b></td><td align=\"center\" valign=\"middle\"><b>Minimum Charge (xx.xx)</b></td></td><td align=\"center\" valign=\"middle\"><b>Base Charge (xx.xx)</b></td><td align=\"center\" valign=\"middle\" colspan=" + dtTotalByPercent.Rows.Count.ToString() + "><b>Shipping Charge As % Of Total</b></td><td>&nbsp;</td><td>&nbsp;</td></tr>\n");
					html.Append("<tr>\n");
					html.Append("<td align=\"left\" valign=\"middle\">Low</td>\n");
					html.Append("<td align=\"left\" valign=\"middle\">High</td>\n");
					html.Append("<td align=\"left\" valign=\"middle\">Minimum</td>\n");
					html.Append("<td align=\"left\" valign=\"middle\">Base</td>\n");
					foreach(DataRow row in dtTotalByPercent.Rows)
					{
						html.Append("<td align=\"left\" valign=\"middle\">" + DB.RowFieldByLocale(row, "Name", LocaleSetting) + "</td>\n");
					}
					html.Append("<td align=\"left\" valign=\"middle\">Edit</td>\n");
					html.Append("<td align=\"left\" valign=\"middle\">Delete</td>\n");
					html.Append("</tr>\n");

					var sqlShipping = string.Empty;
					if(!MultiStoreFilteringEnabled)
					{
						sqlShipping =	@"SELECT DISTINCT stp.rowguid,stp.lowvalue,stp.highvalue,stp.minimumcharge,stp.SurCharge 
										FROM ShippingByTotalByPercent stp with (NOLOCK) 
										INNER JOIN ShippingMethod sm WITH (NOLOCK) ON sm.ShippingMethodid = stp.ShippingMethodId
										order by LowValue";
					}
					else
					{
						sqlShipping = @"SELECT DISTINCT stp.rowguid,stp.lowvalue,stp.highvalue,stp.minimumcharge,stp.SurCharge 
										FROM ShippingByTotalByPercent stp with (NOLOCK) 
										INNER JOIN ShippingMethod sm WITH (NOLOCK) ON sm.ShippingMethodid = stp.ShippingMethodId 
										INNER JOIN ShippingMethodStore ssm WITH (NOLOCK) ON ssm.ShippingMethodid = sm.ShippingMethodId
										WHERE ssm.StoreId = " + StoreFilter.ToString() +
										@" order by LowValue";
					}

					using(SqlConnection con = new SqlConnection(DB.GetDBConn()))
					{
						con.Open();
						using(IDataReader rs = DB.GetRS(sqlShipping, con))
						{
							while(rs.Read())
							{
								bool EditRow = (EditGUID == DB.RSFieldGUID(rs, "RowGUID"));
								html.Append("<tr>\n");
								html.Append("<td align=\"left\" valign=\"middle\">\n");
								if(EditRow)
								{
									html.Append("<input maxLength=\"10\" size=\"10\" name=\"Low_" + DB.RSFieldGUID(rs, "RowGUID") + "\" value=\"" + Localization.CurrencyStringForDBWithoutExchangeRate(DB.RSFieldDecimal(rs, "LowValue")) + "\">\n");
									html.Append("<input type=\"hidden\" name=\"Low_" + DB.RSFieldGUID(rs, "RowGUID") + "_vldt\" value=\"[number][blankalert=Please enter starting order amount][invalidalert=Please enter a money value, WITHOUT the dollar sign]\">\n");
								}
								else
								{
									html.Append(Localization.CurrencyStringForDBWithoutExchangeRate(DB.RSFieldDecimal(rs, "LowValue")));
								}
								html.Append("</td>\n");
								html.Append("<td align=\"left\" valign=\"middle\">\n");
								if(EditRow)
								{
									html.Append("<input maxLength=\"10\" size=\"10\" name=\"High_" + DB.RSFieldGUID(rs, "RowGUID") + "\" value=\"" + Localization.CurrencyStringForDBWithoutExchangeRate(DB.RSFieldDecimal(rs, "HighValue")) + "\">\n");
									html.Append("<input type=\"hidden\" name=\"High_" + DB.RSFieldGUID(rs, "RowGUID") + "_vldt\" value=\"[number][blankalert=Please enter ending order amount][invalidalert=Please enter a money value, WITHOUT the dollar sign]\">\n");
								}
								else
								{
									html.Append(Localization.CurrencyStringForDBWithoutExchangeRate(DB.RSFieldDecimal(rs, "HighValue")));
								}
								html.Append("</td>\n");

								html.Append("<td align=\"left\" valign=\"middle\">\n");
								if(EditRow)
								{
									html.Append("<input maxLength=\"10\" size=\"10\" name=\"Minimum_" + DB.RSFieldGUID(rs, "RowGUID") + "\" value=\"" + Localization.CurrencyStringForDBWithoutExchangeRate(DB.RSFieldDecimal(rs, "MinimumCharge")) + "\">\n");
									html.Append("<input type=\"hidden\" name=\"Minimum_" + DB.RSFieldGUID(rs, "RowGUID") + "_vldt\" value=\"[number][blankalert=Please enter Minimum shipping cost for this order range, in xx.xx format][invalidalert=Please enter a money value, WITHOUT the dollar sign]\">\n");
								}
								else
								{
									html.Append(Localization.CurrencyStringForDBWithoutExchangeRate(DB.RSFieldDecimal(rs, "MinimumCharge")));
								}
								html.Append("</td>\n");

								html.Append("<td align=\"left\" valign=\"middle\">\n");
								if(EditRow)
								{
									html.Append("<input maxLength=\"10\" size=\"10\" name=\"Base_" + DB.RSFieldGUID(rs, "RowGUID") + "\" value=\"" + Localization.CurrencyStringForDBWithoutExchangeRate(DB.RSFieldDecimal(rs, "SurCharge")) + "\">\n");
									html.Append("<input type=\"hidden\" name=\"Base_" + DB.RSFieldGUID(rs, "RowGUID") + "_vldt\" value=\"[number][blankalert=Please enter base shipping cost for this order range, in xx.xx format][invalidalert=Please enter a money value, WITHOUT the dollar sign]\">\n");
								}
								else
								{
									html.Append(Localization.CurrencyStringForDBWithoutExchangeRate(DB.RSFieldDecimal(rs, "SurCharge")));
								}
								html.Append("</td>\n");

								foreach(DataRow row in dtTotalByPercent.Rows)
								{
									Decimal SurCharge = System.Decimal.Zero; // not used here
									Decimal MinimumCharge = System.Decimal.Zero; // not used here
									html.Append("<td align=\"left\" valign=\"middle\">\n");
									if(EditRow)
									{
										html.Append("<input maxLength=\"10\" size=\"10\" name=\"Rate_" + DB.RSFieldGUID(rs, "RowGUID") + "_" + DB.RowFieldInt(row, "ShippingMethodID").ToString() + "\" value=\"" + Localization.CurrencyStringForDBWithoutExchangeRate(Shipping.GetShipByTotalByPercentCharge(DB.RowFieldInt(row, "ShippingMethodID"), DB.RSFieldGUID(rs, "RowGUID"), out MinimumCharge, out SurCharge)) + "\">\n");
										html.Append("<input type=\"hidden\" name=\"Rate_" + DB.RSFieldGUID(rs, "RowGUID") + "_" + DB.RowFieldInt(row, "ShippingMethodID").ToString() + "_vldt\" value=\"[number][blankalert=Please enter the shipping cost][invalidalert=Please enter a money value, WITHOUT the dollar sign]\">\n");
									}
									else
									{
										html.Append(Localization.CurrencyStringForDBWithoutExchangeRate(Shipping.GetShipByTotalByPercentCharge(DB.RowFieldInt(row, "ShippingMethodID"), DB.RSFieldGUID(rs, "RowGUID"), out MinimumCharge, out SurCharge)));
									}
									html.Append("</td>\n");
								}
								if(EditRow)
								{
									html.Append("<td align=\"left\" valign=\"middle\">");
									html.Append("<input class=\"normalButtons\" type=\"submit\" value=\"Update\" name=\"btnSubmit\">\n");
									html.Append("</td>");
								}
								else
								{
                                    html.Append("<td align=\"left\" valign=\"middle\"><input class=\"normalButtons\" type=\"Button\" name=\"Edit\" value=\"Edit\" onClick=\"self.location='" + AppLogic.AdminLinkUrl("shipping.aspx") + "?StoreId=" + StoreFilter + "&EditGUID=" + DB.RSFieldGUID(rs, "RowGUID") + "'\"></td>\n");
								}
                                html.Append("<td align=\"left\"><input class=\"normalButtons\" type=\"Button\" name=\"Delete\" value=\"Delete\" onClick=\"self.location='" + AppLogic.AdminLinkUrl("shipping.aspx") + "?StoreId=" + StoreFilter + "&deleteByTotalByPercentid=" + DB.RSFieldGUID(rs, "RowGUID") + "'\"></td>\n");
								html.Append("</tr>\n");

							}
						}
					}
					// add new row:
					html.Append("<tr>\n");
					html.Append("<td align=\"left\" valign=\"middle\">\n");
					html.Append("<input maxLength=\"10\" size=\"10\" name=\"Low_0\" \">\n");
					html.Append("<input type=\"hidden\" name=\"Low_0_vldt\" value=\"[number][blankalert=Please enter starting order amount][invalidalert=Please enter a money value, WITHOUT the dollar sign]\">\n");
					html.Append("</td>\n");
					html.Append("<td align=\"left\" valign=\"middle\">\n");
					html.Append("<input maxLength=\"10\" size=\"10\" name=\"High_0\" >\n");
					html.Append("<input type=\"hidden\" name=\"High_0_vldt\" value=\"[number][blankalert=Please enter ending order amount][invalidalert=Please enter a money value, WITHOUT the dollar sign]\">\n");
					html.Append("</td>\n");
					html.Append("<td align=\"left\" valign=\"middle\">\n");
					html.Append("<input maxLength=\"10\" size=\"10\" name=\"Minimum_0\" >\n");
					html.Append("<input type=\"hidden\" name=\"Minimum_0_vldt\" value=\"[number][blankalert=Please enter minimum shipping charge for this range, in xx.xx format][invalidalert=Please enter a money value, WITHOUT the dollar sign]\">\n");
					html.Append("</td>\n");
					html.Append("<td align=\"left\" valign=\"middle\">\n");
					html.Append("<input maxLength=\"10\" size=\"10\" name=\"Base_0\" >\n");
					html.Append("<input type=\"hidden\" name=\"Base_0_vldt\" value=\"[number][blankalert=Please enter base shipping charge for this range, in xx.xx format][invalidalert=Please enter a money value, WITHOUT the dollar sign]\">\n");
					html.Append("</td>\n");

					foreach(DataRow row in dtTotalByPercent.Rows)
					{
						html.Append("<td align=\"left\" valign=\"middle\">\n");
						html.Append("<input maxLength=\"10\" size=\"10\" name=\"Rate_0_" + DB.RowFieldInt(row, "ShippingMethodID").ToString() + "\">\n");
						html.Append("<input type=\"hidden\" name=\"Rate_0_" + DB.RowFieldInt(row, "ShippingMethodID").ToString() + "_vldt\" value=\"[number][blankalert=Please enter the shipping cost][invalidalert=Please enter a money value, WITHOUT the dollar sign]\">\n");
						html.Append("</td>\n");
					}

					html.Append("<td align=\"left\" valign=\"middle\">");
					html.Append("<input class=\"normalButtons\" type=\"submit\" value=\"Add New Row\" name=\"btnSubmit\">\n");
					html.Append("</td>\n");
					html.Append("<td>&nbsp;</td>");
					html.Append("</tr>\n");
					html.Append("</table>\n");

				}
			}

			html.Append("</form>\n");
		}

		private void RenderUseFixedPriceForm(StringBuilder html, String EditGUID)
		{
			html.Append("<hr size=1>");
			html.Append("<p><b>FIXED RATE SHIPPING TABLE:</b></p>\n");
            html.Append("<script type=\"text/javascript\" src=\"./scripts/formValidate.js\"></script>\n");
            html.Append("<form action=\"" + AppLogic.AdminLinkUrl("shipping.aspx") + "?StoreId=" + StoreFilter + "&EditGUID=" + EditGUID + "\" method=\"post\" id=\"FixedRateForm\" name=\"FixedRateForm\" onsubmit=\"return (validateForm(this) && FixedRateForm_Validator(this))\" onReset=\"return confirm('Do you want to reset all fields to their starting values?');\">\n");
			html.Append("<input type=\"hidden\" name=\"IsSubmitFixedRate\" value=\"true\">\n");

			using(SqlConnection sqlConnection = new SqlConnection(DB.GetDBConn()))
			{
				sqlConnection.Open();

				string getShippingMethodMapping = "exec aspdnsf_GetStoreShippingMethodMapping @StoreID = @storeId, @IsRTShipping = 0, @OnlyMapped = @filterByStore";
				var getShippingMethodMappingParams = new[]
				{
					new SqlParameter("@storeId", StoreFilter),
					new SqlParameter("@filterByStore", MultiStoreFilteringEnabled),
				};

				using(IDataReader rs = DB.GetRS(getShippingMethodMapping, getShippingMethodMappingParams, sqlConnection))
				{
					if(rs.Read())
					{
						html.Append("<table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\">\n");
						html.Append("<tr><td align=\"left\" valign=\"middle\">Shipping Method</td><td align=\"left\" valign=\"middle\">Flat Rate</td></tr>\n");
						do
						{
							html.Append("<tr>\n");
							html.Append("<td align=\"left\" valign=\"middle\">" + DB.RSFieldByLocale(rs, "Name", LocaleSetting) + "</td>\n");
							html.Append("<td>\n");
							html.Append("<input maxLength=\"10\" size=\"10\" name=\"FixedRate_" + DB.RSFieldInt(rs, "ShippingMethodID").ToString() + "\" value=\"" + Localization.CurrencyStringForDBWithoutExchangeRate(DB.RSFieldDecimal(rs, "FixedRate")) + "\"> (in x.xx format)\n");
							html.Append("<input type=\"hidden\" name=\"FixedRate_" + DB.RSFieldInt(rs, "ShippingMethodID").ToString() + "_vldt\" value=\"[number][req][blankalert=Please enter the shipping cost][invalidalert=Please enter a money value, WITHOUT the dollar sign]\">\n");
							html.Append("</td>\n");
							html.Append("</tr>\n");
						} while(rs.Read());
						html.Append("<tr><td align=\"left\" valign=\"middle\"></td><td align=\"left\" valign=\"middle\"><input class=\"normalButtons\" type=\"submit\" value=\"Update\" name=\"btnSubmit\"></td></tr>\n");
						html.Append("</table>\n");
					}
					else
					{
						html.Append("No shipping methods are defined. <a href=\"" + AppLogic.AdminLinkUrl("shippingmethods.aspx") + "\"><b>Click here</b></a> to define your shipping methods first");
					}
				}
			}

			html.Append("</form>\n");
		}

		private void RenderUseFixedPercentageOfTotalForm(StringBuilder html, String EditGUID)
		{
			html.Append("<hr size=1>");
			html.Append("<p><b>FIXED PERCENT OF TOTAL ORDER SHIPPING TABLE:</b></p>\n");
            html.Append("<script type=\"text/javascript\" src=\"./scripts/formValidate.js\"></script>\n");
            html.Append("<form action=\"" + AppLogic.AdminLinkUrl("shipping.aspx") + "?StoreId=" + StoreFilter + "&EditGUID=" + EditGUID + "\" method=\"post\" id=\"FixedPercentOfTotalForm\" name=\"FixedPercentOfTotalForm\" onsubmit=\"return (validateForm(this) && FixedPercentOfTotalForm_Validator(this))\" onReset=\"return confirm('Do you want to reset all fields to their starting values?');\">\n");
			html.Append("<input type=\"hidden\" name=\"IsSubmitFixedPercentOfTotal\" value=\"true\">\n");

			using(SqlConnection sqlConnection = new SqlConnection(DB.GetDBConn()))
			{
				sqlConnection.Open();

				string getShippingMethodMapping = "exec aspdnsf_GetStoreShippingMethodMapping @StoreID = @storeId, @IsRTShipping = 0, @OnlyMapped = @filterByStore";
				var getShippingMethodMappingParams = new[]
				{
					new SqlParameter("@storeId", StoreFilter),
					new SqlParameter("@filterByStore", MultiStoreFilteringEnabled),
				};

				using(IDataReader rs = DB.GetRS(getShippingMethodMapping, getShippingMethodMappingParams, sqlConnection))
				{
					if(rs.Read())
					{
						html.Append("<table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\">\n");
						html.Append("<tr class=\"tablenormal\"><td align=\"left\" valign=\"middle\">Shipping Method</td><td align=\"left\" valign=\"middle\">Flat Percent Of Total Order Cost</td></tr>\n");
						do
						{
							html.Append("<tr>\n");
							html.Append("<td align=\"left\" valign=\"middle\">" + DB.RSFieldByLocale(rs, "Name", LocaleSetting) + "</td>\n");
							html.Append("<td>\n");
							html.Append("<input maxLength=\"10\" size=\"10\" name=\"FixedPercentOfTotal_" + DB.RSFieldInt(rs, "ShippingMethodID").ToString() + "\" value=\"" + Localization.DecimalStringForDB(DB.RSFieldDecimal(rs, "FixedPercentOfTotal")) + "\"> (in x.xx format)\n");
							html.Append("<input type=\"hidden\" name=\"FixedPercentOfTotal_" + DB.RSFieldInt(rs, "ShippingMethodID").ToString() + "_vldt\" value=\"[number][req][blankalert=Please enter the shipping percentage][invalidalert=Please enter a shipping percentage (percent of total order) without the leading % sign]\">\n");
							html.Append("</td>\n");
							html.Append("</tr>\n");
						} while(rs.Read());
						html.Append("<tr><td></td><td align=\"left\" valign=\"middle\"><input class=\"normalButtons\" type=\"submit\" value=\"Update\" name=\"btnSubmit\"></td></tr>\n");
						html.Append("</table>\n");
					}
					else
					{
						html.Append("No shipping methods are defined. <a href=\"" + AppLogic.AdminLinkUrl("shippingmethods.aspx") + "\"><b>Click here</b></a> to define your shipping methods first");
					}
				}
			}

			html.Append("</form>\n");
		}

		private void RenderCalculateShippingByWeightAndZoneForm(StringBuilder html, String EditGUID)
		{
			html.Append("<hr size=1>");

			html.Append("<p><b>ACTIVE RATE TABLE FOR: CALCULATE SHIPPING BY ORDER WEIGHT BY ZONE:</b></p>\n");

			int ShippingMethodID = CommonLogic.FormUSInt("ShippingMethodID");
			if(ShippingMethodID == 0)
			{
				ShippingMethodID = CommonLogic.QueryStringUSInt("ShippingMethodID");
			}
            html.Append("<script type=\"text/javascript\" src=\"./scripts/formValidate.js\"></script>\n");
            html.Append("<form action=\"" + AppLogic.AdminLinkUrl("shipping.aspx") + "?StoreId=" + StoreFilter + "\" method=\"post\" id=\"WeightByZoneSelectForm\" name=\"WeightByZoneSelectForm\" onSubmit=\"return confirm('If you have unsaved changes in your rate table below, click CANCEL and save them first!')\">\n");
			html.Append("<input type=\"hidden\" name=\"IsSubmitWeightByZone\" value=\"true\">\n");
			html.Append("Edit weight/zone table for shipping method: ");
			html.Append("<select name=\"ShippingMethodID\" id=\"ShippingMethodID\" size=\"1\">");
			html.Append("<option value=\"0\" " + CommonLogic.IIF(ShippingMethodID == 0, " selected ", "") + ">Select Shipping Method To Edit</option>");

			using(SqlConnection sqlConnection = new SqlConnection(DB.GetDBConn()))
			{
				sqlConnection.Open();

				string getShippingMethodMapping = "exec aspdnsf_GetStoreShippingMethodMapping @StoreID = @storeId, @IsRTShipping = 0, @OnlyMapped = @filterByStore";
				var getShippingMethodMappingParams = new[]
				{
					new SqlParameter("@storeId", StoreFilter),
					new SqlParameter("@filterByStore", MultiStoreFilteringEnabled),
				};

				using(IDataReader rs = DB.GetRS(getShippingMethodMapping, getShippingMethodMappingParams, sqlConnection))
					while(rs.Read())
						html.Append("<option value=\"" + DB.RSFieldInt(rs, "ShippingMethodID").ToString() + "\" " + CommonLogic.IIF(ShippingMethodID == DB.RSFieldInt(rs, "ShippingMethodID"), " selected ", "") + ">" + DB.RSFieldByLocale(rs, "Name", LocaleSetting) + "</option>");
			}

			html.Append("</select>");
			html.Append("&nbsp;");
			html.Append("<input class=\"normalButtons\" type=\"submit\" value=\"Submit\" name=\"btnSubmit\">");
			html.Append("</form>");
			if(ShippingMethodID != 0)
			{
				using(DataTable dtWeightAndZone = new DataTable())
				{
					using(SqlConnection con = new SqlConnection(DB.GetDBConn()))
					{
						con.Open();
						using(IDataReader rs = DB.GetRS("select * from ShippingZone  with (NOLOCK)  where Deleted=0 order by DisplayOrder,Name", con))
						{
							dtWeightAndZone.Load(rs);
						}
					}

					if(dtWeightAndZone.Rows.Count == 0)
					{
						html.Append("No shipping zones are defined. <a href=\"" + AppLogic.AdminLinkUrl("shippingzones.aspx") + "\"><b>Click here</b></a> to define your zones");
					}
					else
					{
                        html.Append("<script type=\"text/javascript\" src=\"./scripts/formValidate.js\"></script>\n");
                        html.Append("<form action=\"" + AppLogic.AdminLinkUrl("shipping.aspx") + "?StoreId=" + StoreFilter + "\" method=\"post\" id=\"WeightByZoneForm\" name=\"WeightByZoneForm\" onsubmit=\"return (validateForm(this) && WeightByZoneForm_Validator(this))\" onReset=\"return confirm('Do you want to reset all fields to their starting values?');\">\n");
						html.Append("<input type=\"hidden\" name=\"EditGUID\" value=\"" + EditGUID + "\">");
						html.Append("<input type=\"hidden\" name=\"UpdateGUID\" value=\"\">");
						html.Append("<input type=\"hidden\" name=\"DeleteGUID\" value=\"\">");
						html.Append("<input type=\"hidden\" name=\"IsSubmitWeightByZone\" value=\"true\">\n");
						html.Append("<input type=\"hidden\" name=\"ShippingMethodID\" value=\"" + ShippingMethodID.ToString() + "\">\n");
						html.Append("<table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"1\">\n");
						html.Append("<tr class=\"tablenormal\"><td colspan=2 align=\"center\"><b>Order Weight (in " + Localization.WeightUnits() + ")</b></td><td colspan=" + dtWeightAndZone.Rows.Count.ToString() + " align=\"center\"><b>Shipping Charge By Zone</b></td><td>&nbsp;</td><td>&nbsp;</td></tr>\n");
						html.Append("<tr>\n");
						html.Append("<td align=\"left\" valign=\"middle\">Low</td>\n");
						html.Append("<td align=\"left\" valign=\"middle\">High</td>\n");

						foreach(DataRow row in dtWeightAndZone.Rows)
						{
							html.Append("<td align=\"center\">" + DB.RowFieldByLocale(row, "Name", LocaleSetting) + "</td>\n");
						}

						html.Append("<td align=\"left\" valign=\"middle\">Edit</td>\n");
						html.Append("<td align=\"left\" valign=\"middle\">Delete</td>\n");
						html.Append("</tr>\n");

						var sqlShipping = string.Empty;
						sqlShipping =	@"SELECT DISTINCT swz.RowGUID,swz.LowValue,swz.HighValue 
										FROM ShippingWeightByZone swz with (NOLOCK) 
										INNER JOIN ShippingMethod sm WITH (NOLOCK) ON sm.ShippingMethodid = swz.ShippingMethodId 
										order by LowValue";

						using(SqlConnection con = new SqlConnection(DB.GetDBConn()))
						{
							con.Open();
							using(IDataReader rs = DB.GetRS(sqlShipping, con))
							{
								while(rs.Read())
								{

									bool EditRow = (EditGUID == DB.RSFieldGUID(rs, "RowGUID"));
									html.Append("<tr>\n");
									html.Append("<td align=\"left\" valign=\"middle\">\n");
									if(EditRow)
									{
										html.Append("<input maxLength=\"10\" size=\"10\" name=\"Low_" + DB.RSFieldGUID(rs, "RowGUID") + "\" value=\"" + Localization.CurrencyStringForDBWithoutExchangeRate(DB.RSFieldDecimal(rs, "LowValue")) + "\">\n");
										html.Append("<input type=\"hidden\" name=\"Low_" + DB.RSFieldGUID(rs, "RowGUID") + "_vldt\" value=\"[number][blankalert=Please enter starting order amount][invalidalert=Please enter a money value, WITHOUT the dollar sign]\">\n");
									}
									else
									{
										html.Append(Localization.CurrencyStringForDBWithoutExchangeRate(DB.RSFieldDecimal(rs, "LowValue")));
									}
									html.Append("</td>\n");
									html.Append("<td align=\"left\" valign=\"middle\">\n");
									if(EditRow)
									{
										html.Append("<input maxLength=\"10\" size=\"10\" name=\"High_" + DB.RSFieldGUID(rs, "RowGUID") + "\" value=\"" + Localization.CurrencyStringForDBWithoutExchangeRate(DB.RSFieldDecimal(rs, "HighValue")) + "\">\n");
										html.Append("<input type=\"hidden\" name=\"High_" + DB.RSFieldGUID(rs, "RowGUID") + "_vldt\" value=\"[number][blankalert=Please enter ending order amount][invalidalert=Please enter a money value, WITHOUT the dollar sign]\">\n");
									}
									else
									{
										html.Append(Localization.CurrencyStringForDBWithoutExchangeRate(DB.RSFieldDecimal(rs, "HighValue")));
									}
									html.Append("</td>\n");
									foreach(DataRow row in dtWeightAndZone.Rows)
									{
										html.Append("<td align=\"left\" valign=\"middle\">\n");
										if(EditRow)
										{
											html.Append("<input maxLength=\"10\" size=\"10\" name=\"Rate_" + DB.RSFieldGUID(rs, "RowGUID") + "_" + DB.RowFieldInt(row, "ShippingZoneID").ToString() + "\" value=\"" + Localization.CurrencyStringForDBWithoutExchangeRate(Shipping.GetShipByWeightAndZoneCharge(DB.RowFieldInt(row, "ShippingZoneID"), ShippingMethodID, DB.RSFieldGUID(rs, "RowGUID"))) + "\">\n");
											html.Append("<input type=\"hidden\" name=\"Rate_" + DB.RSFieldGUID(rs, "RowGUID") + "_" + DB.RowFieldInt(row, "ShippingZoneID").ToString() + "_vldt\" value=\"[number][blankalert=Please enter the shipping cost][invalidalert=Please enter a money value, WITHOUT the dollar sign]\">\n");
										}
										else
										{
											html.Append(Localization.CurrencyStringForDBWithoutExchangeRate(Shipping.GetShipByWeightAndZoneCharge(DB.RowFieldInt(row, "ShippingZoneID"), ShippingMethodID, DB.RSFieldGUID(rs, "RowGUID"))));
										}
										html.Append("</td>\n");
									}
									if(EditRow)
									{
										html.Append("<td align=\"left\" valign=\"middle\">");
										html.Append("<input class=\"normalButtons\" type=\"Button\" onClick=\"document.getElementById('WeightByZoneForm').elements['UpdateGUID'].value='" + DB.RSFieldGUID(rs, "RowGUID") + "';document.getElementById('WeightByZoneForm').elements['EditGUID'].value='';document.getElementById('WeightByZoneForm').submit();\" value=\"Update\" name=\"btnSubmit\">\n");
										html.Append("</td>");
									}
									else
									{
										html.Append("<td align=\"left\" valign=\"middle\"><input class=\"normalButtons\" type=\"Button\" name=\"Edit\" value=\"Edit\" onClick=\"document.getElementById('WeightByZoneForm').elements['EditGUID'].value='" + DB.RSFieldGUID(rs, "RowGUID") + "';document.getElementById('WeightByZoneForm').submit();\"></td>\n");
									}
									html.Append("<td align=\"left\" valign=\"middle\"><input class=\"normalButtons\" type=\"Button\" name=\"Delete\" value=\"Delete\" onClick=\"document.getElementById('WeightByZoneForm').elements['DeleteGUID'].value='" + DB.RSFieldGUID(rs, "RowGUID") + "';document.getElementById('WeightByZoneForm').elements['EditGUID'].value='';document.getElementById('WeightByZoneForm').submit();\"></td>\n");
									html.Append("</tr>\n");

								}
							}
						}

						// add new row:
						html.Append("<tr>\n");
						html.Append("<td align=\"left\" valign=\"middle\">\n");
						html.Append("<input maxLength=\"10\" size=\"10\" name=\"Low_0\" \">\n");
						html.Append("<input type=\"hidden\" name=\"Low_0_vldt\" value=\"[number][blankalert=Please enter starting order amount][invalidalert=Please enter a money value, WITHOUT the dollar sign]\">\n");
						html.Append("</td>\n");
						html.Append("<td align=\"left\" valign=\"middle\">\n");
						html.Append("<input maxLength=\"10\" size=\"10\" name=\"High_0\" >\n");
						html.Append("<input type=\"hidden\" name=\"High_0_vldt\" value=\"[number][blankalert=Please enter ending order amount][invalidalert=Please enter a money value, WITHOUT the dollar sign]\">\n");
						html.Append("</td>\n");

						foreach(DataRow row in dtWeightAndZone.Rows)
						{
							html.Append("<td align=\"left\" valign=\"middle\">\n");
							html.Append("<input maxLength=\"10\" size=\"10\" name=\"Rate_0_" + DB.RowFieldInt(row, "ShippingZoneID").ToString() + "\">\n");
							html.Append("<input type=\"hidden\" name=\"Rate_0_" + DB.RowFieldInt(row, "ShippingZoneID").ToString() + "_vldt\" value=\"[number][blankalert=Please enter the shipping cost][invalidalert=Please enter a money value, WITHOUT the dollar sign]\">\n");
							html.Append("</td>\n");
						}

						html.Append("<td align=\"left\" valign=\"middle\">");
						html.Append("<input class=\"normalButtons\" type=\"submit\" value=\"Add New Row\" name=\"btnSubmit\">\n");
						html.Append("</td>\n");
						html.Append("<td>&nbsp;</td>");
						html.Append("</tr>\n");
						html.Append("</table>\n");
					}
				}
			}

			html.Append("</form>\n");
		}

		private void RenderCalculateShippingByTotalAndZoneForm(StringBuilder html, String EditGUID)
		{
			html.Append("<hr size=1>");

			html.Append("<p><b>ACTIVE RATE TABLE FOR: CALCULATE SHIPPING BY ORDER TOTAL BY ZONE:</b></p>\n");

			int ShippingMethodID = CommonLogic.FormUSInt("ShippingMethodID");
			if(ShippingMethodID == 0)
			{
				ShippingMethodID = CommonLogic.QueryStringUSInt("ShippingMethodID");
			}

            html.Append("<script type=\"text/javascript\" src=\"./scripts/formValidate.js\"></script>\n");
			html.Append("<form action=\"" + AppLogic.AdminLinkUrl("shipping.aspx") + "?StoreId=" + StoreFilter + "\" method=\"post\" id=\"TotalByZoneSelectForm\" name=\"TotalByZoneSelectForm\" onSubmit=\"return confirm('If you have unsaved changes in your rate table below, click CANCEL and save them first!')\">\n");
			html.Append("<input type=\"hidden\" name=\"IsSubmitTotalByZone\" value=\"true\">\n");
			html.Append("Edit total/zone table for shipping method: ");
			html.Append("<select name=\"ShippingMethodID\" id=\"ShippingMethodID\" size=\"1\">");
			html.Append("<option value=\"0\" " + CommonLogic.IIF(ShippingMethodID == 0, " selected ", "") + ">Select Shipping Method To Edit</option>");

			using(SqlConnection sqlConnection = new SqlConnection(DB.GetDBConn()))
			{
				sqlConnection.Open();

				string getShippingMethodMapping = "exec aspdnsf_GetStoreShippingMethodMapping @StoreID = @storeId, @IsRTShipping = 0, @OnlyMapped = @filterByStore";
				var getShippingMethodMappingParams = new[]
				{
					new SqlParameter("@storeId", StoreFilter),
					new SqlParameter("@filterByStore", MultiStoreFilteringEnabled),
				};

				using(IDataReader rs = DB.GetRS(getShippingMethodMapping, getShippingMethodMappingParams, sqlConnection))
					while(rs.Read())
						html.Append("<option value=\"" + DB.RSFieldInt(rs, "ShippingMethodID").ToString() + "\" " + CommonLogic.IIF(ShippingMethodID == DB.RSFieldInt(rs, "ShippingMethodID"), " selected ", "") + ">" + DB.RSFieldByLocale(rs, "Name", LocaleSetting) + "</option>");
			}

			html.Append("</select>");
			html.Append("&nbsp;");
			html.Append("<input class=\"normalButtons\" type=\"submit\" value=\"Submit\" name=\"btnSubmit\">");
			html.Append("</form>");
			if(ShippingMethodID != 0)
			{
				using(DataTable dtTotalAndZone = new DataTable())
				{
					using(SqlConnection con = new SqlConnection(DB.GetDBConn()))
					{
						con.Open();
						using(IDataReader rs = DB.GetRS("select * from ShippingZone  with (NOLOCK)  where deleted=0 order by DisplayOrder,Name", con))
						{
							dtTotalAndZone.Load(rs);
						}
					}

					if(dtTotalAndZone.Rows.Count == 0)
					{
						html.Append("No shipping zones are defined. <a href=\"" + AppLogic.AdminLinkUrl("shippingzones.aspx") + "\"><b>Click here</b></a> to define your zones");
					}
					else
					{
                        html.Append("<script type=\"text/javascript\" src=\"./scripts/formValidate.js\"></script>\n");
						html.Append("<form action=\"" + AppLogic.AdminLinkUrl("shipping.aspx") + "?StoreId=" + StoreFilter + "\" method=\"post\" id=\"TotalByZoneForm\" name=\"TotalByZoneForm\" onsubmit=\"return (validateForm(this) && TotalByZoneForm_Validator(this))\" onReset=\"return confirm('Do you want to reset all fields to their starting values?');\">\n");
						html.Append("<input type=\"hidden\" name=\"EditGUID\" value=\"" + EditGUID + "\">");
						html.Append("<input type=\"hidden\" name=\"UpdateGUID\" value=\"\">");
						html.Append("<input type=\"hidden\" name=\"DeleteGUID\" value=\"\">");
						html.Append("<input type=\"hidden\" name=\"IsSubmitTotalByZone\" value=\"true\">\n");
						html.Append("<input type=\"hidden\" name=\"ShippingMethodID\" value=\"" + ShippingMethodID.ToString() + "\">\n");
						html.Append("<table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"1\">\n");
						html.Append("<tr class=\"tablenormal\"><td colspan=2 align=\"center\"><b>Order Total</b></td><td colspan=" + dtTotalAndZone.Rows.Count.ToString() + " align=\"center\"><b>Shipping Charge By Zone</b></td><td>&nbsp;</td><td>&nbsp;</td></tr>\n");
						html.Append("<tr>\n");
						html.Append("<td align=\"left\" valign=\"middle\">Low</td>\n");
						html.Append("<td align=\"left\" valign=\"middle\">High</td>\n");
						foreach(DataRow row in dtTotalAndZone.Rows)
						{
							html.Append("<td align=\"center\">" + DB.RowFieldByLocale(row, "Name", LocaleSetting) + "</td>\n");
						}
						html.Append("<td align=\"left\" valign=\"middle\">Edit</td>\n");
						html.Append("<td align=\"left\" valign=\"middle\">Delete</td>\n");
						html.Append("</tr>\n");

						var sqlShipping = string.Empty;
						sqlShipping =	@"SELECT DISTINCT stz.RowGUID,stz.LowValue,stz.HighValue 
										FROM ShippingTotalByZone stz with (NOLOCK) 
										INNER JOIN ShippingMethod sm WITH (NOLOCK) ON sm.ShippingMethodid = stz.ShippingMethodId 
										ORDER BY LowValue";

						using(SqlConnection con = new SqlConnection(DB.GetDBConn()))
						{
							con.Open();
							using(IDataReader rs = DB.GetRS(sqlShipping, con))
							{
								while(rs.Read())
								{
									bool EditRow = (EditGUID == DB.RSFieldGUID(rs, "RowGUID"));
									html.Append("<tr>\n");
									html.Append("<td align=\"left\" valign=\"middle\">\n");
									if(EditRow)
									{
										html.Append("<input maxLength=\"10\" size=\"10\" name=\"Low_" + DB.RSFieldGUID(rs, "RowGUID") + "\" value=\"" + Localization.CurrencyStringForDBWithoutExchangeRate(DB.RSFieldDecimal(rs, "LowValue")) + "\">\n");
										html.Append("<input type=\"hidden\" name=\"Low_" + DB.RSFieldGUID(rs, "RowGUID") + "_vldt\" value=\"[number][blankalert=Please enter starting order amount][invalidalert=Please enter a money value, WITHOUT the dollar sign]\">\n");
									}
									else
									{
										html.Append(Localization.CurrencyStringForDBWithoutExchangeRate(DB.RSFieldDecimal(rs, "LowValue")));
									}
									html.Append("</td>\n");
									html.Append("<td align=\"left\" valign=\"middle\">\n");
									if(EditRow)
									{
										html.Append("<input maxLength=\"10\" size=\"10\" name=\"High_" + DB.RSFieldGUID(rs, "RowGUID") + "\" value=\"" + Localization.CurrencyStringForDBWithoutExchangeRate(DB.RSFieldDecimal(rs, "HighValue")) + "\">\n");
										html.Append("<input type=\"hidden\" name=\"High_" + DB.RSFieldGUID(rs, "RowGUID") + "_vldt\" value=\"[number][blankalert=Please enter ending order amount][invalidalert=Please enter a money value, WITHOUT the dollar sign]\">\n");
									}
									else
									{
										html.Append(Localization.CurrencyStringForDBWithoutExchangeRate(DB.RSFieldDecimal(rs, "HighValue")));
									}
									html.Append("</td>\n");
									foreach(DataRow row in dtTotalAndZone.Rows)
									{
										html.Append("<td align=\"left\" valign=\"middle\">\n");
										if(EditRow)
										{
											html.Append("<input maxLength=\"10\" size=\"10\" name=\"Rate_" + DB.RSFieldGUID(rs, "RowGUID") + "_" + DB.RowFieldInt(row, "ShippingZoneID").ToString() + "\" value=\"" + Localization.CurrencyStringForDBWithoutExchangeRate(Shipping.GetShipByTotalAndZoneCharge(DB.RowFieldInt(row, "ShippingZoneID"), ShippingMethodID, DB.RSFieldGUID(rs, "RowGUID"))) + "\">\n");
											html.Append("<input type=\"hidden\" name=\"Rate_" + DB.RSFieldGUID(rs, "RowGUID") + "_" + DB.RowFieldInt(row, "ShippingZoneID").ToString() + "_vldt\" value=\"[number][blankalert=Please enter the shipping cost][invalidalert=Please enter a money value, WITHOUT the dollar sign]\">\n");
										}
										else
										{
											html.Append(Localization.CurrencyStringForDBWithoutExchangeRate(Shipping.GetShipByTotalAndZoneCharge(DB.RowFieldInt(row, "ShippingZoneID"), ShippingMethodID, DB.RSFieldGUID(rs, "RowGUID"))));
										}
										html.Append("</td>\n");
									}
									if(EditRow)
									{
										html.Append("<td align=\"left\" valign=\"middle\">");
										html.Append("<input class=\"normalButtons\" type=\"Button\" onClick=\"document.getElementById('TotalByZoneForm').elements['UpdateGUID'].value='" + DB.RSFieldGUID(rs, "RowGUID") + "';document.getElementById('TotalByZoneForm').elements['EditGUID'].value='';document.getElementById('TotalByZoneForm').submit();\"  value=\"Update\" name=\"btnSubmit\">\n");
										html.Append("</td>");
									}
									else
									{
										html.Append("<td align=\"left\" valign=\"middle\"><input class=\"normalButtons\" type=\"Button\" name=\"Edit\" value=\"Edit\" onClick=\"document.getElementById('TotalByZoneForm').elements['EditGUID'].value='" + DB.RSFieldGUID(rs, "RowGUID") + "';document.getElementById('TotalByZoneForm').submit();\"></td>\n");
									}
									html.Append("<td align=\"left\" valign=\"middle\"><input class=\"normalButtons\" type=\"Button\" name=\"Delete\" value=\"Delete\" onClick=\"document.getElementById('TotalByZoneForm').elements['DeleteGUID'].value='" + DB.RSFieldGUID(rs, "RowGUID") + "';document.getElementById('TotalByZoneForm').elements['EditGUID'].value='';document.getElementById('TotalByZoneForm').submit();\"></td>\n");
									html.Append("</tr>\n");
								}
							}
						}
						// add new row:
						html.Append("<tr>\n");
						html.Append("<td align=\"left\" valign=\"middle\">\n");
						html.Append("<input maxLength=\"10\" size=\"10\" name=\"Low_0\" \">\n");
						html.Append("<input type=\"hidden\" name=\"Low_0_vldt\" value=\"[number][blankalert=Please enter starting order amount][invalidalert=Please enter a money value, WITHOUT the dollar sign]\">\n");
						html.Append("</td>\n");
						html.Append("<td align=\"left\" valign=\"middle\">\n");
						html.Append("<input maxLength=\"10\" size=\"10\" name=\"High_0\" >\n");
						html.Append("<input type=\"hidden\" name=\"High_0_vldt\" value=\"[number][blankalert=Please enter ending order amount][invalidalert=Please enter a money value, WITHOUT the dollar sign]\">\n");
						html.Append("</td>\n");
						foreach(DataRow row in dtTotalAndZone.Rows)
						{
							html.Append("<td align=\"left\" valign=\"middle\">\n");
							html.Append("<input maxLength=\"10\" size=\"10\" name=\"Rate_0_" + DB.RowFieldInt(row, "ShippingZoneID").ToString() + "\">\n");
							html.Append("<input type=\"hidden\" name=\"Rate_0_" + DB.RowFieldInt(row, "ShippingZoneID").ToString() + "_vldt\" value=\"[number][blankalert=Please enter the shipping cost][invalidalert=Please enter a money value, WITHOUT the dollar sign]\">\n");
							html.Append("</td>\n");
						}
						html.Append("<td align=\"left\" valign=\"middle\">");
						html.Append("<input class=\"normalButtons\" type=\"submit\" value=\"Add New Row\" name=\"btnSubmit\">\n");
						html.Append("</td>\n");
						html.Append("<td>&nbsp;</td>");
						html.Append("</tr>\n");
						html.Append("</table>\n");
					}
				}
			}

			html.Append("</form>\n");
		}
	}
}
