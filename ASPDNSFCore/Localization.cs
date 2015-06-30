// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Configuration;
using System.Xml;
using System.Web;
using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Data;
using System.Data.SqlClient;

namespace AspDotNetStorefrontCore
{
    /// <summary>
    /// Summary description for Localization.
    /// </summary>
    public class Localization
    {
        static private CultureInfo USCulture = new CultureInfo("en-US");

        static private String WebConfigLocale = String.Empty;
        static private String SqlServerLocale = String.Empty;
        static private CultureInfo SqlServerCulture;
        public static string ALL_LOCALES = "AllLocales";

        public Localization() { }

        static public XmlDocument LocalesDoc
        {
            get
            {
                XmlDocument d = (XmlDocument)HttpContext.Current.Cache.Get("LocalesDoc");
                if (d == null)
                {
                    d = DB.GetSqlXmlDoc("select LocaleSettingID, Name, Description, DisplayOrder, DefaultCurrencyID from dbo.LocaleSetting Locales with (NOLOCK)  order by DisplayOrder,Name for xml auto", null);
                    HttpContext.Current.Cache.Insert("LocalesDoc", d, null, System.DateTime.Now.AddMinutes(AppLogic.AppConfigUSInt("Localization.CurrencyCacheMinutes")), TimeSpan.Zero);
                }
                return d;
            }
        }

        static public void FlushCache()
        {
            try
            {
                HttpContext.Current.Cache.Remove("LocalesDoc");
            }
            catch { }
        }



        static public bool isValidLocale(String Locale)
        {
            String tmpS = String.Empty;
            XmlNode n = LocalesDoc.SelectSingleNode("//Locales[@Name='" + Locale + "']");
            return (n != null);

        }

        static public String GetDefaultLocale() { return GetDefaultLocale(AppLogic.StoreID()); }
        static public String GetDefaultLocale(Int32 StoreId)
        {
            string config = AppLogic.AppConfig("DefaultLocale", StoreId, true);
            if (!String.IsNullOrEmpty(config) && Localization.isValidLocale(config))
            {
                return config;
            }
            return GetWebConfigDefaultLocale();
        }

        static private String GetWebConfigDefaultLocale()
        {
            if (WebConfigLocale.Length == 0)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(CommonLogic.SafeMapPath("~/web.config")); // Always the top App web.config
                XmlNode node = doc.DocumentElement.SelectSingleNode("/descendant::*[local-name()='globalization']");
                WebConfigLocale = CheckLocaleSettingForProperCase(node.Attributes["culture"].InnerText);
            }
            return WebConfigLocale;
        }

        static public string GetCustomerLocaleSettingID(int ID)
 	 	{
 	 	    string LocaleSetting = String.Empty;
 	 	 	using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
 	 	 	{
 	 	 	    con.Open();
 	 	 	    using (IDataReader rs = DB.GetRS("SELECT LocaleSetting FROM Customer with (NOLOCK) WHERE CustomerID=" + DB.SQuote(ID.ToString()), con))
 	 	 	    {
 	 	 	        if (rs.Read())
 	 	 	        {
 	 	 	            LocaleSetting = DB.RSField(rs, "LocaleSetting");
 	 	 	        }
 	 	 	    }
 	 	 	}
 	 	 	return LocaleSetting;
 	 	}

        static public int GetLocaleSettingID(string Name)
        {
            int LocaleID = 0;
            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("SELECT LocaleSettingID FROM LocaleSetting with (NOLOCK) WHERE Name=" + DB.SQuote(Name), con))
                {
                    if (rs.Read())
                    {
                        LocaleID = DB.RSFieldInt(rs, "LocaleSettingID");
                    }
                }
            }
            return LocaleID;
        }

        static public String GetPrimaryCurrency()
        {
            return GetPrimaryCurrency(true);
        }
        static public String GetPrimaryCurrency(Boolean ForceDefault)
        {
            return StoreCurrency(ForceDefault);
        }

        static public String GetUSLocale()
        {
            return "en-US";
        }
       
        public static DataTable GetLocales()
        {
            using (DataTable dtLocales = new DataTable())
            {
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS("select * from LocaleSetting  with (NOLOCK)  order by DisplayOrder,Name", con))
                    {
                        dtLocales.Load(rs);
                    }
                }
                return dtLocales;
            }

            
        }

        static public String ValidateLocaleSetting(String Locale)
        {
            String tmp = Localization.WebConfigLocale;
            if(Localization.isValidLocale(Locale))
            {
                tmp = Locale;
            }
            return CheckLocaleSettingForProperCase(tmp);
        }

        static public String CheckLocaleSettingForProperCase(String LocaleSetting)
        {
            // make sure locale is xx-XX:
            int i = LocaleSetting.IndexOf("-");
            if (i != -1)
            {
                LocaleSetting = LocaleSetting.Substring(0, i) + "-" + LocaleSetting.Substring(i + 1, LocaleSetting.Length - (i + 1)).ToUpperInvariant();
            }
            return LocaleSetting;
        }

        static public String CheckCurrencySettingForProperCase(String CurrencySetting)
        {
            return CurrencySetting.ToUpperInvariant();
        }

        static public String GetSqlServerLocale()
        {
            if (SqlServerLocale.Length == 0)
            {
                SqlServerLocale = CommonLogic.Application("DBSQLServerLocaleSetting");
                SqlServerCulture = new CultureInfo(SqlServerLocale);
            }
            return SqlServerLocale;
        }

        static public String WeightUnits()
        {
            return AppLogic.AppConfig("Localization.WeightUnits");
        }

        static public String ShortDateFormat()
        {
            String tmp = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.ToUpperInvariant();
            return tmp;
        }

        static public string JSCalendarDateFormatSpec()
        {
            // see jscalendar/calendar-setup.js for more info. Typical format would be: " + Localization.JSCalendarDateFormatSpec() + "
            String tmp = ShortDateFormat();
            tmp = tmp.Replace("MM", "%m").Replace("M", "%m").Replace("DD", "%d").Replace("D", "%d").Replace("YYYY", "%Y").Replace("YY", "%Y");
            return tmp;
        }

        static public String JSCalendarLanguageFile()
        {
            return "calendar-" + GetDefaultLocale().Substring(0, 2) + ".js";
        }

        static public String StoreCurrency()
        {
            return StoreCurrency(true);
        }

        static public String StoreCurrency(Boolean ForceDefault)
        {
            String tmpS;
            if (!ForceDefault)
            {
                tmpS = AppLogic.AppConfig("Localization.StoreCurrency").ToUpperInvariant();
            }
            else
            {
                tmpS = AppLogic.AppConfig("Localization.StoreCurrency", 0, true).ToUpperInvariant();
            }
            if (tmpS.Length == 0)
            {
                tmpS = "USD"; // set some default
            }
            return tmpS;
        }

        static public String StoreCurrencyNumericCode()
        {
            String tmpS = AppLogic.AppConfig("Localization.StoreCurrencyNumericCode");
            if (tmpS.Length == 0)
            {
                tmpS = "840"; // set some default
            }
            return tmpS;
        }

        static public bool ParseBoolean(String s)
        {
            try
            {
                return System.Boolean.Parse(s);
            }
            catch
            {
                return false;
            }
        }

        static public int ParseUSInt(String s)
        {
            int usi;
            System.Int32.TryParse(s, NumberStyles.Integer, USCulture, out usi); // use default locale setting
            return usi;
        }

        static public int ParseNativeInt(String s)
        {
            int ni;
            System.Int32.TryParse(s, NumberStyles.Integer, Thread.CurrentThread.CurrentUICulture, out ni); // use default locale setting
            return ni;
        }

        static public long ParseUSLong(String s)
        {
            long usl;
            System.Int64.TryParse(s, NumberStyles.Integer, USCulture, out usl); // use default locale setting
            return usl;
        }

        static public long ParseNativeLong(String s)
        {
            long nl;
            System.Int64.TryParse(s, NumberStyles.Integer, Thread.CurrentThread.CurrentUICulture, out nl); // use default locale setting
            return nl;
        }

        static public Single ParseUSSingle(String s)
        {
            Single uss;
            System.Single.TryParse(s, NumberStyles.Number, USCulture, out uss);
            return uss;
        }

        static public Single ParseNativeSingle(String s)
        {
            Single ns;
            System.Single.TryParse(s, NumberStyles.Number, Thread.CurrentThread.CurrentUICulture, out ns);
            return ns;
        }

        static public Double ParseUSDouble(String s)
        {
            Double usd;
            System.Double.TryParse(s, NumberStyles.Number, USCulture, out usd);
            return usd;
        }

        static public Double ParseNativeDouble(String s)
        {
            Double nd;
            System.Double.TryParse(s, NumberStyles.Number, Thread.CurrentThread.CurrentUICulture, out nd);
            return nd;
        }

        static public decimal ParseUSCurrency(String s)
        {
            Decimal usc;
            System.Decimal.TryParse(s, NumberStyles.Currency, USCulture, out usc);
            return usc;
        }

        static public decimal ParseNativeCurrency(String s)
        {
            Decimal nc;
            System.Decimal.TryParse(s, NumberStyles.Currency, Thread.CurrentThread.CurrentUICulture, out nc);
            return nc;
        }

        static public decimal ParseUSDecimal(String s)
        {
            Decimal usd;
            System.Decimal.TryParse(s, NumberStyles.Number, USCulture, out usd);
            return usd;
        }

        static public decimal ParseNativeDecimal(String s)
        {
            Decimal nd;
            System.Decimal.TryParse(s, NumberStyles.Number, Thread.CurrentThread.CurrentUICulture, out nd);
            return nd;
        }

        static public DateTime ParseUSDateTime(String s)
        {
            try
            {
                return System.DateTime.Parse(s, USCulture);
            }
            catch
            {
                return System.DateTime.MinValue;
            }
        }

        static public DateTime ParseNativeDateTime(String s)
        {
            try
            {
                return System.DateTime.Parse(s); // use default locale setting
            }
            catch
            {
                return System.DateTime.MinValue;
            }
        }


        static public String ToUSShortDateString(DateTime dt)
        {
            if (dt.Equals(System.DateTime.MinValue))
            {
                return String.Empty;
            }
            return dt.ToString("MM/dd/yy"); //dt.Month.ToString().PadLeft(2,'0') + "/" + dt.Day.ToString().PadLeft(2,'0') + "/" + dt.Year.ToString().Substring(2,2);
        }

        static public String ToThreadCultureShortDateString(DateTime dt)
        {
            if (dt.Equals(System.DateTime.MinValue))
            {
                return String.Empty;
            }
            return dt.ToShortDateString();
        }

        static public String ToNativeShortDateString(DateTime dt)
        {
            if (dt.Equals(System.DateTime.MinValue))
            {
                return String.Empty;
            }
            return dt.ToString(new CultureInfo(Localization.GetDefaultLocale())).Split(' ')[0];
        }

        static public String ToUSDateTimeString(DateTime dt)
        {
            if (dt.Equals(System.DateTime.MinValue))
            {
                return String.Empty;
            }
            return dt.ToString("MM/dd/yyyy HH:mm:ss"); //dt.Month.ToString().PadLeft(2,'0') + "/" + dt.Day.ToString().PadLeft(2,'0') + "/" + dt.Year.ToString().Substring(2,2) + " " + dt.Hour.ToString().PadLeft(2,'0') + ":" + dt.Minute.ToString().PadLeft(2,'0') + ":" + dt.Second.ToString().PadLeft(2,'0') + "." + dt.Millisecond.ToString() + " " + dt.TimeOfDay;
        }

        static public String ToNativeDateTimeString(DateTime dt)
        {
            if (dt.Equals(System.DateTime.MinValue))
            {
                return String.Empty;
            }
            return dt.ToString(new CultureInfo(Localization.GetDefaultLocale()));
        }

        static public String ToDBDateTimeString(DateTime dt)
        {
            return DateTimeStringForDB(dt);
        }

        static public String ToDBShortDateString(DateTime dt)
        {
            return DateStringForDB(dt);
        }

        // no exchange rate is ever applied!
        // input amt is assumed to be in the store's PRIMARY CURRENCY!
        // converted to web.config locale format
        static public String CurrencyStringForDisplayWithoutExchangeRate(decimal amt)
        {
            return CurrencyStringForDisplayWithoutExchangeRate(amt, true);
        }

        // no exchange rate is ever applied!
        // input amt is assumed to be in the store's PRIMARY CURRENCY!
        // converted to web.config locale format
        static public String CurrencyStringForDisplayWithoutExchangeRate(decimal amt, bool ShowCurrency)
        {
            String tmpS = amt.ToString("C", new CultureInfo(Localization.GetDefaultLocale()));
            if (tmpS.StartsWith("("))
            {
                tmpS = "-" + tmpS.Replace("(", "").Replace(")", "");
            }
            if (ShowCurrency && Currency.NumPublishedCurrencies() > 1)
            {
                tmpS = String.Format("{0} ({1})", tmpS, Localization.GetPrimaryCurrency());
            }
            return tmpS;
        }

        // no exchange rate is ever applied!
        // input amt is assumed to be in the store's PRIMARY CURRENCY!
        // converted to format defined by LocaleSetting
        static public String CurrencyStringForDisplayWithoutExchangeRate(decimal amt, String TargetCurrencyCode)
        {
            String tmpS = String.Empty;

            // get currency specs for display control:
            String DisplaySpec = Currency.GetDisplaySpec(TargetCurrencyCode);
            String DisplayLocaleFormat = Currency.GetDisplayLocaleFormat(TargetCurrencyCode);

            if (DisplaySpec.Length != 0)
            {
                CultureInfo ci = new CultureInfo(DisplayLocaleFormat);

                if (DisplaySpec.LastIndexOf(',') > DisplaySpec.LastIndexOf("."))
                {
                    ci.NumberFormat.CurrencyDecimalSeparator = ",";

                    if (DisplaySpec.LastIndexOf(".") > -1)
                    {
                        ci.NumberFormat.CurrencyGroupSeparator = ".";
                    }
                }

                if (!DisplaySpec.StartsWith("#"))
                {
                    int indexOf = DisplaySpec.IndexOf('#');

                    if (indexOf > -1)
                    {
                        ci.NumberFormat.CurrencySymbol = DisplaySpec.Substring(0, indexOf);
                    }
                }

                String fmtCur = amt.ToString("C", ci.NumberFormat);
                tmpS = String.Format("{0} ({1})",fmtCur,TargetCurrencyCode);
            }
            else if (DisplayLocaleFormat.Length != 0)
            {
                tmpS = amt.ToString("C", new CultureInfo(DisplayLocaleFormat));
                if (tmpS.StartsWith("("))
                {
                    tmpS = "-" + tmpS.Replace("(", "").Replace(")", "");
                }
                if (Currency.NumPublishedCurrencies() > 1)
                {
                    tmpS = String.Format("{0} ({1})", tmpS, TargetCurrencyCode);
                }
            }
            else
            {
                tmpS = CurrencyStringForDisplayWithoutExchangeRate(amt); // use some generic default!
            }
            return tmpS;
        }

        // uses DisplaySpec if provided, else uses LocaleSetting
        // applies exchange rate!!
        // input amt is assumed to be in the store's PRIMARY CURRENCY!
        static public String CurrencyStringForDisplayWithExchangeRate(decimal amt, String TargetCurrencyCode)
        {
            String tmpS = String.Empty;
            // apply exchange rate if not outputting in primary store currency:
            amt = Currency.Convert(amt, Localization.GetPrimaryCurrency(), TargetCurrencyCode);
            return CurrencyStringForDisplayWithoutExchangeRate(amt, TargetCurrencyCode);
        }

        // input amt is assumed to be in the store's PRIMARY CURRENCY!
        static private String CurrencyStringForDisplayUSWithoutExchangeRate(decimal amt)
        {
            String tmpS = amt.ToString("C", USCulture);
            if (tmpS.StartsWith("("))
            {
                tmpS = "-" + tmpS.Replace("(", "").Replace(")", "");
            }
            if (Currency.NumPublishedCurrencies() > 1)
            {
                tmpS = String.Format("{0} ({1})", tmpS, "USD");
            }
            return tmpS;
        }

        // input amt is assumed to be in the store's PRIMARY CURRENCY!
        // NO formatting is applied. No exchange rates are applied
        // just returns xxxx.xx foramt
        static public String CurrencyStringForGatewayWithoutExchangeRate(decimal amt)
        {
            String s = amt.ToString("#.00", USCulture);
            if(s == ".00")
            {
                s = "0.00";
            }
            return s;
        }

        static public DateTime ParseDBDateTime(String s)
        {
            try
            {
                return System.DateTime.Parse(s, SqlServerCulture);
            }
            catch
            {
                return System.DateTime.MinValue;
            }
        }
        
        static public Double ParseDBDouble(String theval)
        {
            try
            {
                return System.Double.Parse(theval, SqlServerCulture);
            }
            catch
            {
                return 0.0D;
            }
        }

        static public Single ParseDBSingle(String theval)
        {
            try
            {
                return System.Single.Parse(theval, SqlServerCulture);
            }
            catch
            {
                return 0.0F;
            }
        }

        static public Decimal ParseDBDecimal(String theval)
        {
            try
            {
                return System.Decimal.Parse(theval, SqlServerCulture);
            }
            catch
            {
                return 0.0M;
            }
        }


        static public DateTime ParseLocaleDateTime(String theval, String LocaleSetting)
        {
            try
            {
                return System.DateTime.Parse(theval, new CultureInfo(LocaleSetting));
            }
            catch
            {
                return System.DateTime.MinValue;
            }
        }
        
        static public Double ParseLocaleDouble(String theval, String LocaleSetting)
        {
            try
            {
                return System.Double.Parse(theval, new CultureInfo(LocaleSetting));
            }
            catch
            {
                return 0.0D;
            }
        }

        static public Single ParseLocaleSingle(String theval, String LocaleSetting)
        {
            try
            {
                return System.Single.Parse(theval, new CultureInfo(LocaleSetting));
            }
            catch
            {
                return 0.0F;
            }
        }

        static public Decimal ParseLocaleDecimal(String theval, String LocaleSetting)
        {
            try
            {
                return System.Decimal.Parse(theval, new CultureInfo(LocaleSetting));
            }
            catch
            {
                return 0.0M;
            }
        }


        /// <summary>
        /// Converts a string in the SourceLocaleSetting format to a date string in the DestLocaleSetting format
        /// </summary>
        /// <param name="theval"></param>
        /// <param name="SourceLocaleSetting"></param>
        /// <param name="DestLocaleSetting"></param>
        /// <returns></returns>
        static public string ConvertLocaleDateTime(String theval, String SourceLocaleSetting, String DestLocaleSetting)
        {
            try
            {
                return System.DateTime.Parse(theval, new CultureInfo(SourceLocaleSetting)).ToString(new CultureInfo(DestLocaleSetting));
            }
            catch
            {
                return System.DateTime.MinValue.ToString(new CultureInfo(DestLocaleSetting));
            }
        }
        


        // ----------------------------------------------------------------------------------------------
        // the following routines must (should) work in ALL locales, no matter what SQL Server setting is
        // ----------------------------------------------------------------------------------------------
        static public String DateStringForDB(DateTime dt)
        {
            return dt.ToString("yyyyMMdd");
        }

        static public String DateTimeStringForDB(DateTime dt)
        {
            return dt.ToString("s");
        }

        static public String CurrencyStringForDBWithoutExchangeRate(decimal amt)
        {
            String tmpS = amt.ToString("C4", USCulture);
            if (tmpS.StartsWith("("))
            {
                tmpS = "-" + tmpS.Replace("(", "").Replace(")", "");
            }
            return tmpS.Replace("$", "").Replace(",", "");
        }

        static public String IntStringForDB(int amt)
        {
            return amt.ToString("G", USCulture).Replace(",", "");
        }

        static public String SingleStringForDB(Single amt)
        {
            return amt.ToString("G", USCulture).Replace(",", "");
        }

        static public String DoubleStringForDB(double amt)
        {
            return amt.ToString("G", USCulture).Replace(",", "");
        }

        static public String DecimalStringForDB(decimal amt)
        {
            return amt.ToString("G", USCulture).Replace(",", "");
        }

        // ------------------------------------------------------------------------------
        // Type Formatting
        // ------------------------------------------------------------------------------
        public static string FormatDecimal2Places(decimal temp)
        {
            return temp.ToString("N2");
        }

        public static string FormatDecimal2Places(string temp)
        {
            decimal dec = Localization.ParseDBDecimal(temp);
            return dec.ToString("N2");
        }
    }
}
