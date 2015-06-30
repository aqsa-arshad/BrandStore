// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Web;
using System.Text.RegularExpressions;
using AspDotNetStorefrontCore;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Globalization;


namespace Vortx.MobileFramework
{
    public class MobileCulture
    {
        public string BaseCulture { get; set; }
        //public string BaseRegion { get; private set; }

        public string MobileCultureName
        {
            get { return Data.Config.MobilePlatform.MobileLocaleDefault; }
        }

        public MobileCulture(string baseCulture)
        {
            BaseCulture = Localization.CheckLocaleSettingForProperCase(baseCulture);
            //BaseRegion = BaseCulture.Split('-')[1];


            //MobileCultureName = Vortx.Data.Config.MobilePlatform.MobileLocaleDefault; // BaseCulture;

            //if (IsMobileCulture(BaseCulture))
            //{
            //    MobileCultureName = Vortx.Data.Config.MobilePlatform.MobileLocale; // BaseCulture;
            //}
            //else
            //{
            //    MobileCultureName = Vortx.Data.Config.MobilePlatform.MobileLocale; //Localization.CheckLocaleSettingForProperCase(BaseCulture + "-" + MobileCultureExtension);
            //}
        }


        //public bool HasMobileCulture()
        //{
        //    string sSql = string.Format("SELECT Name AS S From LocaleSetting WHERE Name={0}", DB.SQuote(this.MobileCultureName));
        //    string name = DB.GetSqlS(sSql);
        //    return name == this.MobileCultureName;
        //}

        public void CreateForSystem()
        {
            throw new NotImplementedException();

            //CultureAndRegionInfoBuilder cib = new CultureAndRegionInfoBuilder(this.MobileCultureName, CultureAndRegionModifiers.None);

            //// Populate the new CultureAndRegionInfoBuilder object with culture information.
            //CultureInfo ci = new CultureInfo(this.BaseCulture);
            //cib.LoadDataFromCultureInfo(ci);

            //// Populate the new CultureAndRegionInfoBuilder object with region information.
            //RegionInfo ri = new RegionInfo(this.BaseRegion);
            //cib.LoadDataFromRegionInfo(ri);

            //try
            //{
            //    cib.Register();
            //}
            //catch (UnauthorizedAccessException uaex)
            //{

            //}
            //catch (InvalidOperationException ex)
            //{
            //    // ignore the error if the culture already exists
            //    if (ex.Message.IndexOf("already exists.") < 0)
            //        throw ex;
            //}
        }

        public void DeleteForSystem()
        {
            throw new NotImplementedException();

//            CultureAndRegionInfoBuilder.Unregister(this.MobileCultureName);
        }

        public Boolean ExistsInDB()
        {
            string sSql = string.Format("SELECT Name AS S From LocaleSetting WHERE Name={0}", DB.SQuote(this.BaseCulture));
            using (SqlConnection dbConn = new SqlConnection(DB.GetDBConn()))
            {
                dbConn.Open();
                using (IDataReader idr = DB.GetRS(sSql, dbConn))
                {
                    if (idr.Read())
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void CreateForDB()
        {
            CreateForDB("Mobile Language");
        }

        public void CreateForDB(string description)
        {
            if (ExistsInDB())
            {
                throw new ArgumentException("Cannot create locale. It already exists.");
            }

            //if (HasMobileCulture())
            //    return;

            // Setup Mobile Locale
            StringBuilder sql = new StringBuilder();
            String NewGUID = DB.GetNewGUID();
            int storecurrency = 1;
            int testcurrency = Currency.GetCurrencyID(AppLogic.AppConfig("Localization.StoreCurrency"));
            if (testcurrency > 0)
            {
                storecurrency = testcurrency;
            }
            sql.Append("insert into LocaleSetting(LocaleSettingGUID,Name,Description,DefaultCurrencyID,DisplayOrder) values(");
            sql.Append(DB.SQuote(NewGUID) + ",");
            sql.Append(DB.SQuote(this.BaseCulture) + ",");
            sql.Append(DB.SQuote(description) + ",");
            sql.Append(DB.SQuote(storecurrency.ToString()) + ",");
            sql.Append(DB.SQuote("7"));
            sql.Append(")");

            try
            {
                DB.ExecuteSQL(sql.ToString());
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
