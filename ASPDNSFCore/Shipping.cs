// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using AspDotNetStorefrontCore.ShippingCalculation;

namespace AspDotNetStorefrontCore
{
    /// <summary>
    /// Summary description for Shipping.
    /// </summary>
    public class Shipping
    {

        // this MUST match the table defs in ShippingCalculation table
        public enum ShippingCalculationEnum
        {
            Unknown = 0,
            CalculateShippingByWeight = 1,
            CalculateShippingByTotal = 2,
            UseFixedPrice = 3,
            AllOrdersHaveFreeShipping = 4,
            UseFixedPercentageOfTotal = 5,
            UseIndividualItemShippingCosts = 6,
            UseRealTimeRates = 7,
            CalculateShippingByWeightAndZone = 8,
            CalculateShippingByTotalAndZone = 9,
            CalculateShippingByTotalByPercent = 10,
        };

        public enum FreeShippingReasonEnum
        {
            DoesNotQualify = 0,
            AllOrdersHaveFreeShipping = 1,
            AllDownloadItems = 2,
            ExceedsFreeShippingThreshold = 3,
            CustomerLevelHasFreeShipping = 4,
            CouponHasFreeShipping = 5,
            AllFreeShippingItems = 6
        };

        /// <summary>
        /// Flag used to disable shipping filtering per store
        /// </summary>
        public const int DONT_FILTER_PER_STORE = -1;

        public Shipping() { }

        static public decimal GetVariantShippingCost(int VariantID, int ShippingMethodID)
        {
            decimal tmp = System.Decimal.Zero;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select ShippingCost from ShippingByProduct  with (NOLOCK)  where VariantID=" + VariantID.ToString() + " and ShippingMethodID=" + ShippingMethodID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSFieldDecimal(rs, "ShippingCost");
                    }
                }
            }
            return tmp;
        }

        static public int GetFirstFreeShippingMethodID()
        {
            foreach (String s in AppLogic.AppConfig("ShippingMethodIDIfFreeShippingIsOn").Trim().Split(','))
            {
                String s2 = s.Trim();
                if (s2.Length != 0)
                {
                    if (CommonLogic.IsInteger(s2))
                    {
                        return System.Int32.Parse(s2);
                    }
                }
            }
            return 0;
        }

        static public bool ShippingMethodIsInFreeList(int ShippingMethodID)
        {
            return CommonLogic.IntegerIsInIntegerList(ShippingMethodID, AppLogic.AppConfig("ShippingMethodIDIfFreeShippingIsOn"));
        }

        static public String GetFreeShippingMethodIDs()
        {
            return AppLogic.AppConfig("ShippingMethodIDIfFreeShippingIsOn").Trim();
        }

        static public bool ShippingMethodIsValid(int ShippingMethodID, String StateAbbrev, String CountryName)
        {
            // helper for shoppingcart class for efficiency:
            return ShippingMethodIsAllowedForState(ShippingMethodID, AppLogic.GetStateID(StateAbbrev)) && ShippingMethodIsAllowedForCountry(ShippingMethodID, AppLogic.GetCountryID(CountryName));
        }

        static public bool ShippingMethodToStateMapIsEmpty()
        {
            return (DB.GetSqlN("select count(*) as N from ShippingMethodToStateMap  with (NOLOCK)") == 0);
        }

        static public bool ShippingMethodToStateMapIsEmpty(int countryID)
        {
            return (DB.GetSqlN("select count(*) as N from dbo.ShippingMethodToStateMap sm with (NOLOCK) inner join dbo.State s with (NOLOCK) on s.StateID = sm.StateID where s.CountryID = " + countryID.ToString()) == 0);
        }

        static public bool ShippingMethodToCountryMapIsEmpty()
        {
            return (DB.GetSqlN("select count(*) as N from ShippingMethodToCountryMap  with (NOLOCK)") == 0);
        }

        static public bool ShippingMethodToZoneMapIsEmpty()
        {
            return (DB.GetSqlN("select count(*) as N from ShippingMethodToZoneMap  with (NOLOCK)") == 0);
        }

        static public bool ShippingMethodIsAllowedForState(int ShippingMethodID, int StateID)
        {
            if (ShippingMethodToStateMapIsEmpty() || GetActiveShippingCalculationID() == ShippingCalculationEnum.UseRealTimeRates)
            {
                return true;
            }
            return (DB.GetSqlN("select count(*) as N from ShippingMethodToStateMap  with (NOLOCK)  where ShippingMethodID=" + ShippingMethodID.ToString() + " and StateID=" + StateID.ToString()) != 0);
        }

        static public bool ShippingMethodIsAllowedForCountry(int ShippingMethodID, int CountryID)
        {
            if (ShippingMethodToCountryMapIsEmpty() || GetActiveShippingCalculationID() == ShippingCalculationEnum.UseRealTimeRates)
            {
                return true;
            }
            return (DB.GetSqlN("select count(*) as N from ShippingMethodToCountryMap  with (NOLOCK)  where ShippingMethodID=" + ShippingMethodID.ToString() + " and CountryID=" + CountryID.ToString()) != 0);
        }

        static public bool ShippingMethodIsAllowedForZone(int ShippingMethodID, int ShippingZoneID)
        {
            if (ShippingMethodToZoneMapIsEmpty() || GetActiveShippingCalculationID() == ShippingCalculationEnum.UseRealTimeRates)
            {
                return true;
            }
            return (DB.GetSqlN("select count(*) as N from ShippingMethodToZoneMap  with (NOLOCK)  where ShippingMethodID=" + ShippingMethodID.ToString() + " and ShippingZoneID=" + ShippingZoneID.ToString()) != 0);
        }

        static public bool MultiShipEnabled()
        {
            return AppLogic.AppConfigBool("AllowMultipleShippingAddressPerOrder") || AppLogic.AppConfigBool("ShowGiftRegistryButtons");
        }

        public static int ZoneLookup(String zip)
        {
            int ZipCodePrefixLength = AppLogic.AppConfigNativeInt("ZipCodePrefixLength");

            if (ZipCodePrefixLength < 0)
                ZipCodePrefixLength = 3;

            if (ZipCodePrefixLength > 5)
                ZipCodePrefixLength = 5;

            zip = zip.Trim().PadRight(5, '0');
            String ZipSubStr = zip.Substring(0, ZipCodePrefixLength);
            int ZipSubStrInt = 0;
            try
            {
                ZipSubStrInt = Localization.ParseUSInt(ZipSubStr);
            }
            catch
            {
                return AppLogic.AppConfigUSInt("ZoneIDForNoMatch"); // something bad as input zip
            }
            int ZoneID = 0;
            
            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("select * from ShippingZone with (NOLOCK)", dbconn))
                {
                    while (rs.Read())
                    {
                        String[] thisZipList = Regex.Replace(DB.RSField(rs, "ZipCodes"), "\\s+", "", RegexOptions.Compiled).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (String s in thisZipList)
                        {
                            // is it a single 3 digit prefix, or a range:
                            if (s.IndexOf("-") ==
                                -1)
                            {
                                String s2 = s.Substring(0, ZipCodePrefixLength);
                                // single item:
                                int LowPrefix = 0;
                                try
                                {
                                    if (CommonLogic.IsInteger(s2))
                                    {
                                        LowPrefix = Localization.ParseUSInt(s2);
                                    }
                                }
                                catch
                                {
                                }
                                if (LowPrefix == ZipSubStrInt)
                                {
                                    ZoneID = DB.RSFieldInt(rs, "ShippingZoneID");
                                    break;
                                }
                            }
                            else
                            {
                                // range:
                                String[] s2 = s.Split('-');
                                int LowPrefix = 0;
                                int HighPrefix = 0;
                                try
                                {
                                    String s2a;
                                    s2a = s2[0].Substring(0, ZipCodePrefixLength);
                                    String s2b;
                                    s2b = s2[1].Substring(0, ZipCodePrefixLength);
                                    if (CommonLogic.IsInteger(s2a))
                                    {
                                        LowPrefix = Localization.ParseUSInt(s2a);
                                    }
                                    if (CommonLogic.IsInteger(s2b))
                                    {
                                        HighPrefix = Localization.ParseUSInt(s2b);
                                    }
                                }
                                catch
                                {
                                }
                                if (LowPrefix <= ZipSubStrInt &&
                                    ZipSubStrInt <= HighPrefix)
                                {
                                    ZoneID = DB.RSFieldInt(rs, "ShippingZoneID");
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            if (ZoneID == 0)
            {
                ZoneID = AppLogic.AppConfigUSInt("ZoneIDForNoMatch");
            }
            return ZoneID;
        }

        /// <summary>
        /// Gets the active shiping calculation, multi-store aware
        /// </summary>
        /// <returns></returns>
        static public Shipping.ShippingCalculationEnum GetActiveShippingCalculationID()
        {
            // default from the appconfig
            ShippingCalculationEnum calcMode = (ShippingCalculationEnum)AppLogic.AppConfigUSInt("DefaultShippingCalculationID");

            // let's try for a store specific shipping calculation
            int storeId = AppLogic.StoreID();
            ShippingCalculationEnum storeCalcMode = GetActiveShippingCalculationID(storeId);

            // unknown means we didn't find any for that store
            if (storeCalcMode == ShippingCalculationEnum.Unknown)
            {
                // let's fallback to the default store
                storeId = AppLogic.DefaultStoreID();
                storeCalcMode = GetActiveShippingCalculationID(storeId);
            }

            // if we did find a store specific calc mode use that, which will be almost always unless not configured
            if (storeCalcMode != ShippingCalculationEnum.Unknown)
            {
                calcMode = storeCalcMode;
            }

            return calcMode;
        }
        
        /// <summary>
        /// Gets the active shipping calculation per store
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        private static ShippingCalculationEnum GetActiveShippingCalculationID(int storeId)
        {
            ShippingCalculationEnum calcId = ShippingCalculationEnum.Unknown;
            string query = "SELECT ShippingCalculationID FROM ShippingCalculationStore WITH (NOLOCK) WHERE StoreId = {0}".FormatWith(storeId);
            
            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                using (IDataReader rs = DB.GetRS(query, conn))
                {
                    if (rs.Read())
                    {
                        calcId = (ShippingCalculationEnum)DB.RSFieldInt(rs, "ShippingCalculationID");
                    }

                    rs.Close();
                    rs.Dispose();
                }

                conn.Close();
                conn.Dispose();
            }
            
            return calcId;
        }

        static public String GetShippingMethodName(int ShippingMethodID, String LocaleSetting)
        {
            String tmp = string.Empty;

            if (ShippingMethodID > 0)
            {
                using (SqlConnection dbconn = DB.dbConn())
                {
                    dbconn.Open();

                    using (IDataReader rs = DB.GetRS("Select * from ShippingMethod with (NOLOCK) WHERE ShippingMethodID=" + ShippingMethodID.ToString(), dbconn))
                    {
                        if (rs.Read())
                        {
                            try
                            {
                                if (LocaleSetting == null)
                                {
                                    tmp = DB.RSField(rs, "Name");
                                }
                                else
                                {
                                    tmp = DB.RSFieldByLocale(rs, "Name", LocaleSetting);
                                }
                            }
                            catch { }
                        }
                    }
                }
            }
            
            return tmp;
        }

		static public String GetShippingMethodDisplayName(int ShippingMethodID, String LocaleSetting)
		{
			var shippingMethodDisplayname = string.Empty;
			if(ShippingMethodID > 0)
			{
				using(SqlConnection dbconn = DB.dbConn())
				{
					var sqlParameters = new SqlParameter[] { 
						new SqlParameter("ShippingMethodId", ShippingMethodID)
					};
					dbconn.Open();
					using(IDataReader rs = DB.GetRS("Select Name, DisplayName from ShippingMethod with (NOLOCK) WHERE ShippingMethodID = @ShippingMethodId", sqlParameters, dbconn))
					{
						if(rs.Read())
						{
							var displayName = DB.RSFieldByLocale(rs, "DisplayName", LocaleSetting);
							shippingMethodDisplayname = !String.IsNullOrEmpty(displayName) ? displayName : DB.RSFieldByLocale(rs, "Name", LocaleSetting);
						}
					}
				}
			}

			return shippingMethodDisplayname;
		}

        static public int GetShippingMethodID(String Name)
        {
            int tmp = 0;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader dr = DB.GetRS("Select * from ShippingMethod with (NOLOCK) where name = " + DB.SQuote(Name), con))
                {
                    if (dr.Read())
                    {
                        tmp = DB.RSFieldInt(dr, "ShippingMethodID");
                    }
                }
            }

            return tmp;
        }

        public static String GetZoneName(int ShippingZoneID, String LocaleSetting)
        {
            String tmpS = String.Empty;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select Name from ShippingZone  with (NOLOCK)  where ShippingZoneID=" + ShippingZoneID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmpS = DB.RSFieldByLocale(rs, "Name", LocaleSetting);
                    }
                }
            }

            return tmpS;
        }

        static public decimal GetShipByTotalCharge(int ShippingMethodID, String RowGUID)
        {
            decimal tmp = System.Decimal.Zero;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("Select ShippingCharge from ShippingByTotal  with (NOLOCK)  where RowGUID=" + DB.SQuote(RowGUID) + " and ShippingMethodID=" + ShippingMethodID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSFieldDecimal(rs, "ShippingCharge");
                    }
                }
            }

            return tmp;
        }

        static public decimal GetShipByTotalByPercentCharge(int ShippingMethodID, String RowGUID, out Decimal MinimumCharge, out Decimal SurCharge)
        {
            decimal tmp = System.Decimal.Zero;
            MinimumCharge = System.Decimal.Zero;
            SurCharge = System.Decimal.Zero;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("Select PercentOfTotal,MinimumCharge,SurCharge from ShippingByTotalByPercent  with (NOLOCK)  where RowGUID=" + DB.SQuote(RowGUID) + " and ShippingMethodID=" + ShippingMethodID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSFieldDecimal(rs, "PercentOfTotal");
                        MinimumCharge = DB.RSFieldDecimal(rs, "MinimumCharge");
                        SurCharge = DB.RSFieldDecimal(rs, "SurCharge");
                    }
                }
            }

            return tmp;
        }

        static public decimal GetShipByFixedPercentageCharge(int ShippingMethodID, decimal SubTotal)
        {
            decimal tmp = System.Decimal.Zero;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("Select * from ShippingMethod   with (NOLOCK)  where ShippingMethodID=" + ShippingMethodID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmp = (decimal)DB.RSFieldDecimal(rs, "FixedPercentOfTotal");
                    }
                }
            }

            return (tmp / 100.0M) * SubTotal;
        }

        static public decimal GetShipByItemCharge(int ShippingMethodID, CartItemCollection cartItems)
        {
            decimal tmp = System.Decimal.Zero;
            foreach (CartItem c in cartItems)
            {
                if (!c.IsDownload)
                {
                    int Q = c.Quantity;
                    decimal PR = Shipping.GetVariantShippingCost(c.VariantID, ShippingMethodID) * Q;
                    tmp += PR;
                }
            }
            return tmp;
        }

        static public decimal GetShipByTotalCharge(int ShippingMethodID, decimal SubTotal)
        {
            decimal tmp = 0.0M;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select * from ShippingByTotal  with (NOLOCK)  where LowValue<=" + Localization.CurrencyStringForDBWithoutExchangeRate(SubTotal) + " and HighValue>=" + Localization.CurrencyStringForDBWithoutExchangeRate(SubTotal) + " and ShippingMethodID=" + ShippingMethodID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSFieldDecimal(rs, "ShippingCharge");
                    }
                }
            }

            return tmp;
        }

        static public decimal GetShipByTotalByPercentCharge(int ShippingMethodID, decimal SubTotal)
        {
            decimal tmp = System.Decimal.Zero;
            decimal MinimumCharge = System.Decimal.Zero;
            decimal SurCharge = System.Decimal.Zero;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select * from ShippingByTotalByPercent  with (NOLOCK)  where LowValue<=" + Localization.CurrencyStringForDBWithoutExchangeRate(SubTotal) + " and HighValue>=" + Localization.CurrencyStringForDBWithoutExchangeRate(SubTotal) + " and ShippingMethodID=" + ShippingMethodID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSFieldDecimal(rs, "PercentOfTotal");
                        MinimumCharge = DB.RSFieldDecimal(rs, "MinimumCharge");
                        SurCharge = DB.RSFieldDecimal(rs, "SurCharge");
                    }
                }
            }

            tmp = (SubTotal * (tmp / 100.0M)) + SurCharge;
            if (tmp < MinimumCharge)
            {
                tmp = MinimumCharge;
            }
            return tmp;
        }

        static public decimal GetShipByFixedPrice(int ShippingMethodID, decimal SubTotal)
        {
            decimal tmp = System.Decimal.Zero;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("Select * from ShippingMethod  with (NOLOCK)  where ShippingMethodID=" + ShippingMethodID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmp = (decimal)DB.RSFieldDecimal(rs, "FixedRate");
                    }
                }
            }

            return tmp;
        }

        static public decimal GetShipByWeightCharge(int ShippingMethodID, String RowGUID)
        {
            decimal tmp = System.Decimal.Zero;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("Select ShippingCharge from ShippingByWeight  with (NOLOCK)  where RowGUID=" + DB.SQuote(RowGUID) + " and ShippingMethodID=" + ShippingMethodID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSFieldDecimal(rs, "ShippingCharge");
                    }
                }
            }

            return tmp;
        }

        static public decimal GetShipByWeightCharge(int ShippingMethodID, Decimal WeightTotal)
        {
            decimal tmp = 0.0M;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select * from ShippingByWeight  with (NOLOCK)  where LowValue<=" + Localization.DecimalStringForDB(WeightTotal) + " and HighValue>=" + Localization.DecimalStringForDB(WeightTotal) + " and ShippingMethodID=" + ShippingMethodID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSFieldDecimal(rs, "ShippingCharge");
                    }
                }
            }

            return tmp;
        }

        static public decimal GetShipByWeightAndZoneCharge(int ShippingZoneID, int ShippingMethodID, String RowGUID)
        {
            decimal tmp = System.Decimal.Zero;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("Select ShippingCharge from ShippingWeightByZone  with (NOLOCK)  where ShippingMethodID=" + ShippingMethodID.ToString() + " and RowGUID=" + DB.SQuote(RowGUID) + " and ShippingZoneID=" + ShippingZoneID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSFieldDecimal(rs, "ShippingCharge");
                    }
                }
            }

            return tmp;
        }

        static public decimal GetShipByTotalAndZoneCharge(int ShippingZoneID, int ShippingMethodID, String RowGUID)
        {
            decimal tmp = System.Decimal.Zero;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("Select ShippingCharge from ShippingTotalByZone  with (NOLOCK)  where ShippingMethodID=" + ShippingMethodID.ToString() + " and RowGUID=" + DB.SQuote(RowGUID) + " and ShippingZoneID=" + ShippingZoneID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSFieldDecimal(rs, "ShippingCharge");
                    }
                }
            }

            return tmp;
        }

        static public decimal GetShipByWeightAndZoneCharge(int ShippingMethodID, Decimal WeightTotal, int ShippingZoneID)
        {
            decimal tmp = -1;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("Select ShippingCharge from ShippingWeightByZone  with (NOLOCK)  where LowValue<=" + Localization.DecimalStringForDB(WeightTotal) + " and HighValue>=" + Localization.DecimalStringForDB(WeightTotal) + " and ShippingMethodID=" + ShippingMethodID.ToString() + " and ShippingZoneID=" + ShippingZoneID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSFieldDecimal(rs, "ShippingCharge");
                    }
                }
            }

            return tmp;
        }

        static public decimal GetShipByTotalAndZoneCharge(int ShippingMethodID, decimal OrderTotal, int ShippingZoneID)
        {
            decimal tmp = -1;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("Select ShippingCharge from ShippingTotalByZone  with (NOLOCK)  where LowValue<=" + Localization.CurrencyStringForDBWithoutExchangeRate(OrderTotal) + " and HighValue>=" + Localization.CurrencyStringForDBWithoutExchangeRate(OrderTotal) + " and ShippingMethodID=" + ShippingMethodID.ToString() + " and ShippingZoneID=" + ShippingZoneID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSFieldDecimal(rs, "ShippingCharge");
                    }
                }
            }

            return tmp;
        }

        static public String GetTrackingURL(String ShippingTrackingNumber)
        {
            // Trim tracking number, get rid of spaces and hyphens.
            ShippingTrackingNumber = ShippingTrackingNumber.Replace(" ", "").Replace("-", "").Trim();

            if (ShippingTrackingNumber.Length == 0)
            {
                return "";
            }

            // Check for a match on the ShippingTrackingNumber

            String[] CarrierList = AppLogic.AppConfig("ShippingTrackingCarriers").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            String match = String.Empty;

            foreach (String Carrier in CarrierList)
            {
                match = Regex.Match(ShippingTrackingNumber, AppLogic.AppConfig("ShippingTrackingRegex." + Carrier.Trim()), RegexOptions.Compiled).Value;

                if (match.Length != 0)
                {
                    return String.Format(AppLogic.AppConfig("ShippingTrackingURL." + Carrier.Trim()), ShippingTrackingNumber);
                }
            }

            return "";
        }

        static public List<int> GetDistinctShippingAddressIDs(CartItemCollection cic)
        {
            List<int> addressIDs = new List<int>();
            //String tmpS = ",";
            foreach (CartItem c in cic)
            {
                if (!addressIDs.Contains(c.ShippingAddressID))
                {
                    addressIDs.Add(c.ShippingAddressID);
                }
            }
            return addressIDs;
        }

        static public bool ShippingIsAllValid(CartItemCollection CartItems)
        {
            bool isRealTime = Shipping.GetActiveShippingCalculationID().Equals(ShippingCalculationEnum.UseRealTimeRates);
            // EVERY non-download item must have a shipping address and shipping method:
            foreach (CartItem c in CartItems)
            {
                if (isRealTime)
                {
                    String selMethod = c.ShippingMethod.Split('|')[0];

                    if (selMethod == AppLogic.AppConfig("RTShipping.CallForShippingPrompt") || selMethod == AppLogic.GetString("RTShipping.LocalPickupMethodName", Customer.Current.LocaleSetting))
                    {
                        return true;
                    }
                }

                if (!c.IsDownload && !c.FreeShipping && !GiftCard.s_IsEmailGiftCard(c.ProductID) && c.Shippable)
                {         
                    // at the time this routine is called, ALL of these must be filled in!
                    if (c.ShippingAddressID == 0 || c.ShippingMethodID == 0 || c.ShippingMethod.Length == 0)
                    {
                        return false;
                    }
                }
            }
           
            // if any invalid state/country/shipping method combinations found, then shipping is not valid:
            List<int> AlreadyCheckedAddressIDs = new List<int>();
            
            foreach (CartItem c in CartItems)
            {
                if(!AlreadyCheckedAddressIDs.Contains(c.ShippingAddressID))
                {
                    if (!c.IsDownload && !c.FreeShipping && !c.IsSystem && !GiftCard.s_IsEmailGiftCard(c.ProductID) && !c.Shippable)
                    {
                        Address sa = new Address();
                        sa.LoadFromDB(c.ShippingAddressID);

                        if (sa.State.Trim().Length == 0)
                        {
                            throw new ArgumentException("State field in address record is BLANK. That is not allowed!");
                        }
                        if (sa.Country.Trim().Length == 0)
                        {
                            throw new ArgumentException("Country field in address record is BLANK. That is not allowed!");
                        }
                        if (!Shipping.ShippingMethodIsValid(c.ShippingMethodID, sa.State, sa.Country))
                        {
                            return false;
                        }
                    }
                }
                AlreadyCheckedAddressIDs.Add(c.ShippingAddressID);
            }
            // all seems ok
            return true;
        }

        static public bool NoShippingRequiredComponents(CartItemCollection CartItems)
        {
            // CartItemCollection inherits from List, so .Count in C# doesn't directly
            // translate to LINQ in VB. Use .Where(Func predicate).Count() instead...
            return CartItems.Where(ci => ci.Shippable).Count() == 0;
        }

        static public bool IsAllFreeShippingComponents(CartItemCollection CartItems)
        {
            // CartItemCollection inherits from List, so .Count in C# doesn't directly
            // translate to LINQ in VB. Use .Where(Func predicate).Count() instead...
            //JH 10.20.2010  - free shipping componentes include freeshipping, not shipable, not email gift card, and download products (as it was in 8012)
            return CartItems.All(ci => ci.FreeShipping || !ci.Shippable || ci.IsDownload || GiftCard.s_IsEmailGiftCard(ci.ProductID));
        }

        static public bool HasFreeShippingComponents(CartItemCollection CartItems)
        {
            // CartItemCollection inherits from List, so .Count in C# doesn't directly
            // translate to LINQ in VB. Use .Where(Func predicate).Count() instead...
            return CartItems.Where(ci => ci.FreeShipping).Count() > 0;
        }

        static public string GetFormattedRealTimeShippingMethodForSelectList(ShippingMethod shippingMethod)
        {
            return GetFormattedRealTimeShippingMethodForSelectList(shippingMethod.Id, shippingMethod.GetNameForDisplay(), shippingMethod.Freight, shippingMethod.VatRate);                    
        }

        static public string GetFormattedRealTimeShippingMethodForSelectList(int id, string name, decimal freight, decimal vat)
        {
            var usNumberFormat = new System.Globalization.CultureInfo("en-US").NumberFormat;
            return string.Format("{0}|{1}|{2}|{3}", id, name, freight.ToString("0.00", usNumberFormat), vat.ToString("0.00", usNumberFormat));
        }

        static public string GetFormattedRealTimeShippingMethodForDatabase(string name, decimal freight, decimal vat)
        {
            var usNumberFormat = new System.Globalization.CultureInfo("en-US").NumberFormat;
            return GetFormattedRealTimeShippingMethodForDatabase(name, freight.ToString("0.00", usNumberFormat), vat.ToString("0.00", usNumberFormat));
        }

        static public string GetFormattedRealTimeShippingMethodForDatabase(string name, string freight, string vat)
        {      
            return string.Format("{0}|{1}|{2}", name, freight, vat);
        }
    }

    // Shipping collections for shipping methods
    // Let's get rid of the pipe delimited string of values and structure the data in an object
    // Data class which holds a collection of shipping methods
    public class ShippingMethods : CollectionBase
    {
        public static explicit operator ShippingMethodCollection(ShippingMethods methods)
        {
            ShippingMethodCollection col = new ShippingMethodCollection();
            foreach(ShippingMethod method in methods)
            {
                col.Add(method);
            }
            return col;
        }

        private String error_msg;

        public ShippingMethods()
        {
            error_msg = String.Empty;
        }

        public String ErrorMsg
        {
            get { return this.error_msg; }
            set { this.error_msg = value; }
        }

        public Boolean MethodExists(String method)
        {
            foreach (ShipMethod s in this)
            {
                if (s.ServiceName.ToUpperInvariant() == method.ToUpperInvariant())
                {
                    return true;
                }
            }
            return false;
        }

        public int GetIndex(String method)
        {
            foreach (ShipMethod s in this)
            {
                if (s.ServiceName.ToUpperInvariant() == method.ToUpperInvariant())
                {
                    return this.List.IndexOf(s);
                }
            }
            return -1;
        }

        public void AddMethod(ShipMethod s_method)
        {
            this.List.Add(s_method);
        }

        public ShipMethod this[int index]
        {
            set
            {
                this.List[index] = value;
            }
            get
            {
                return (ShipMethod)this.List[index];
            }
        }

        public void Sort(IComparer Comparer)
        {
            ArrayList.Adapter(this.List).Sort(Comparer);
        }

        public IEnumerable PerCarrier(string carrier)
        {
            List<ShipMethod> shippingMethodsPerCarrier = new List<ShipMethod>();

            foreach (ShipMethod shipMethod in this)
            {
                if (shipMethod.Carrier.Equals(carrier, StringComparison.InvariantCultureIgnoreCase))
                {
                    shippingMethodsPerCarrier.Add(shipMethod);
                }
            }

            return shippingMethodsPerCarrier;
        }
    }

    public partial class ShipMethod : CollectionBase // Data class to hold information about a specific shipping method
    {
        #region CastOperators
        public static explicit operator ShippingMethod(ShipMethod meth)
        {
            ShippingMethod SM = new ShippingMethod();

			SM.Name = meth.ServiceName;
			SM.DisplayName = meth.DisplayName;
            SM.Id = Shipping.GetShippingMethodID(meth.ServiceName);
            SM.IsFree = Shipping.ShippingMethodIsInFreeList(SM.Id);
            SM.Freight = meth.ServiceRate;
            SM.VatRate = meth.VatRate;
            SM.IsRealTime = true;
            return SM;
        }

        #endregion

        public ShipMethod()
        {
			ServiceName = String.Empty;
			DisplayName = String.Empty;
            Carrier = String.Empty;
        }
		public int ShippingMethodID { get; set; }
        public String ServiceName { get; set; }
		public String DisplayName { get; set; }
        public Decimal ServiceRate { get; set; }
        public Decimal VatRate { get; set; }
		public String Carrier { get; set; }
		public Decimal FreeItemsRate { get; set; }
		public string GetNameForDisplay()
		{
			return !String.IsNullOrEmpty(this.DisplayName) ? this.DisplayName : this.ServiceName;
		}
    }


    //Collections for holding information about cart items and addresses which we can then use for real time carriers
    public class Shipments : CollectionBase // Data class which holds information about different Packages (a group of items shipping from the same origin)
    {
        private bool s_HasDistributorItems;
        private bool s_HasFreeItems;
        public bool IsInternational
        {
            get { return List.OfType<Packages>().Any(p => p.DestinationCountry != "US" || p.DestinationCountry != "United States"); }
        }

        public Shipments()
        {
            s_HasDistributorItems = false;
            s_HasFreeItems = false;
        }

        public bool HasDistributorItems
        {
            get { return this.s_HasDistributorItems; }
            set { this.s_HasDistributorItems = value; }
        }

        public bool HasFreeItems
        {
            get { return this.s_HasFreeItems; }
            set { this.s_HasFreeItems = value; }
        }

        public void AddPackages(Packages packages)
        {
            this.List.Add(packages);
        }

        public Packages this[int index]
        {
            get
            {
                return (Packages)this.List[index];
            }
        }
    }

    public class Packages : CollectionBase	// Data class which holds the multiples packages information
    {

        private string m_DestinationAddress1;
        private string m_DestinationAddress2;
        private string m_DestinationCity;
        private string m_DestinationStateProvince;
        private string m_DestinationZipPostalCode;
        private string m_DestinationCountry;
        private string m_DestinationCountryCode;
        private ResidenceTypes m_DestinationResidenceType;
        private Decimal m_Weight;

        private string m_Pickuptype;
        private string m_OriginAddress1;
        private string m_OriginAddress2;
        private string m_OriginCity;
        private string m_OriginStateProvince;
        private string m_OriginZipPostalCode;
        private string m_OriginCountryCode;
        private bool m_HasDistributorItems;

        public Packages()
        {
            m_DestinationAddress1 = string.Empty;
            m_DestinationAddress2 = string.Empty;
            m_DestinationCity = string.Empty;
            m_DestinationStateProvince = string.Empty;
            m_DestinationZipPostalCode = string.Empty;
            m_DestinationCountry = string.Empty;
            m_DestinationCountryCode = string.Empty;

            m_Pickuptype = string.Empty;
            m_OriginAddress1 = string.Empty;
            m_OriginAddress2 = string.Empty;
            m_OriginCity = string.Empty;
            m_OriginStateProvince = string.Empty;
            m_OriginZipPostalCode = string.Empty;
            m_OriginCountryCode = string.Empty;
            m_HasDistributorItems = false;

            m_Weight = 0.0M;
        }

        public string PickupType	// Shipment pickup type
        {
            get { return this.m_Pickuptype; }
            set { this.m_Pickuptype = value.Trim(); }
        }

        public string OriginCity
        {
            get { return this.m_OriginCity; }
            set { this.m_OriginCity = value; }
        }

        public string OriginAddress1
        {
            get { return this.m_OriginAddress1; }
            set { this.m_OriginAddress1 = value; }
        }

        public string OriginZipPostalCode
        {
            get { return this.m_OriginZipPostalCode; }
            set { this.m_OriginZipPostalCode = value; }
        }

        public string OriginAddress2
        {
            get { return this.m_OriginAddress2; }
            set { this.m_OriginAddress2 = value; }
        }

        public string OriginStateProvince
        {
            get
            {
                if (m_OriginStateProvince == "-" || m_OriginStateProvince == "--" || m_OriginStateProvince == "ZZ")
                {
                    return String.Empty;
                }
                else
                {
                    return m_OriginStateProvince;
                }
            }
            set { m_OriginStateProvince = value; }
        }

        public string OriginCountryCode
        {
            get { return this.m_OriginCountryCode; }
            set { this.m_OriginCountryCode = value; }
        }

        public bool HasDistributorItems
        {
            get { return this.m_HasDistributorItems; }
            set { this.m_HasDistributorItems = value; }
        }

        public Decimal Weight
        {
            get
            {
                this.m_Weight = 0;
                for (int i = 0; i < this.List.Count; i++)
                {
                    Package p = (Package)this.List[i];
                    this.m_Weight += p.Weight;
                    p = null;
                }

                return this.m_Weight;
            }
        }

        public string DestinationCity
        {
            get { return this.m_DestinationCity; }
            set { this.m_DestinationCity = value; }
        }

        public string DestinationAddress1
        {
            get { return this.m_DestinationAddress1; }
            set { this.m_DestinationAddress1 = value; }
        }

        public string DestinationZipPostalCode
        {
            get { return this.m_DestinationZipPostalCode; }
            set { this.m_DestinationZipPostalCode = value; }
        }

        public string DestinationAddress2
        {
            get { return this.m_DestinationAddress2; }
            set { this.m_DestinationAddress2 = value; }
        }

        public string DestinationStateProvince	// Shipment destination State or Province
        {
            get
            {
                if (m_DestinationStateProvince == "-" || m_DestinationStateProvince == "--" || m_DestinationStateProvince == "ZZ")
                {
                    return String.Empty;
                }
                else
                {
                    return m_DestinationStateProvince;
                }
            }
            set { m_DestinationStateProvince = value; }
        }

        public string DestinationCountry
        {
            get { return this.m_DestinationCountry; }
            set { this.m_DestinationCountry = value; }
        }

        public string DestinationCountryCode
        {
            get { return this.m_DestinationCountryCode; }
            set { this.m_DestinationCountryCode = value; }
        }

        public ResidenceTypes DestinationResidenceType
        {
            get { return this.m_DestinationResidenceType; }
            set { this.m_DestinationResidenceType = value; }
        }

        public void AddPackage(Package package)
        {
            this.List.Add(package);
        }


        public Package this[int index]
        {
            get
            {
                return (Package)this.List[index];
            }
        }


        public int PackageCount
        {
            get
            {
                int count = 0;
                bool hasShipSeparatelyItems = false;
                foreach (Package p in this.List)
                {
                    if (p.IsShipSeparately)
                    {
                        count += p.Quantity;
                        hasShipSeparatelyItems = true;
                    }
                }

                if (hasShipSeparatelyItems)
                    return count;
                else
                    return 1;
            }
        }
    }

    public class Package	// Data class which holds information about a single package
    {
        public Package()
        {
        }
        public Package(CartItem fromCartItem)
        {
            //JH fix from 5004 notes
            Quantity = fromCartItem.Quantity;
            IsShipSeparately = fromCartItem.IsShipSeparately;
            //end JH
            DimensionsWHD = fromCartItem.Dimensions.ToLowerInvariant();
            Weight = fromCartItem.Weight != 0.0M ?
                fromCartItem.Weight:
                AppLogic.AppConfigUSDecimal("RTShipping.DefaultItemWeight")
                ;
            if (Weight == 0.0M)
            {
                Weight = 0.5M;
            }
            Weight += AppLogic.AppConfigUSDecimal("RTShipping.PackageExtraWeight");
            Insured = AppLogic.AppConfigBool("RTShipping.Insured");
            InsuredValue = fromCartItem.Price;
            IsFreeShipping = fromCartItem.FreeShipping;
        }


        #region Properties

        private int m_quantity;
        public int Quantity
        {
            get { return m_quantity; }
            set { m_quantity = value; }
        }

        private Boolean m_isshipseparately;
        public Boolean IsShipSeparately
        {
            get { return m_isshipseparately; }
            set { m_isshipseparately = value; }
        }

        private Boolean m_isfreeshipping;
        public Boolean IsFreeShipping
        {
            get { return m_isfreeshipping; }
            set { m_isfreeshipping = value; }
        }

        private Decimal m_insuredvalue;
        public Decimal InsuredValue
        {
            get { return m_insuredvalue; }
            set { m_insuredvalue = value; }
        }

        private int m_packageid;
        public int PackageId
        {
            get { return m_packageid; }
            set { m_packageid = value; }
        }

        private bool m_insured;
        public bool Insured
        {
            get { return m_insured; }
            set { m_insured = value; }
        }

        private Decimal m_width;
        public Decimal Width
        {
            get { return m_width; }
            set { m_width = value; }
        }

        private decimal m_weight;
        public Decimal Weight
        {
            get { return m_weight; }
            set { m_weight = value; }
        }

        private Decimal m_height;
        public Decimal Height
        {
            get { return m_height; }
            set { m_height = value; }
        }

        private Decimal m_length;
        public Decimal Length
        {
            get { return m_length; }
            set { m_length = value; }
        }


        /// <summary>
        /// Gets or sets package dimensions in format of N.NN x N.NN x N.NN.
        /// This is Height x Length x Width
        /// </summary>
        public string DimensionsWHD
        {
            get
            {
                return string.Format("{0}x{1}x{2}",
                    Width,
                    Height,
                    Length);
            }
            set
            {
                string[] dd = value.Split('x');
                try
                {
                    Width = Localization.ParseUSDecimal(dd[0].Trim());
                    Height = Localization.ParseUSDecimal(dd[1].Trim());
                    Length = Localization.ParseUSDecimal(dd[2].Trim());
                }
                catch
                {
					Width = 0;
					Height = 0;
					Length = 0;
                }
            }
        }

        #endregion


    }
}
