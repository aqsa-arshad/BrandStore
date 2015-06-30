// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using AspDotNetStorefrontCore;
using System.Data;
using Vortx.Data.Config;
using System.Xml;
using System.Globalization;

namespace Vortx.MobileFramework
{
    public class LocaleManagement
    {
        public static Boolean LocaleExists(string locale)
        {
            MobileCulture mc = new MobileCulture(locale);
            return mc.ExistsInDB();
        }

        internal static Boolean LocaleIsMobile(string locale)
        {
            string sql = "select mobilelocale from mobilelocalemapping where mobilelocale = " + DB.SQuote(locale);
            using (SqlConnection dbConn = new SqlConnection(DB.GetDBConn()))
            {
                dbConn.Open();
                using (IDataReader idr = DB.GetRS(sql, dbConn))
                {
                    if (idr.Read())
                        return true;
                }
                DeleteInvalidLocaleMappings();
                sql = "select mobilelocale from mobilelocalemapping";
                using (IDataReader idr = DB.GetRS(sql, dbConn))
                {
                    if (!idr.Read())
                        return true;
                }
            }
            return false;
        }

        public static void DeleteInvalidLocaleMappings()
        {
            DB.ExecuteSQL("delete from mobilelocalemapping where desktoplocale not in (select name from localesetting) or mobilelocale not in (select name from localesetting)");
        }

        internal static bool LocaleIsDesktop(string locale)
        {
            string sql = @"
                select ls.name as localename, ls.[Description] as localedescription, mobilelocale.name as mobilelocalename, mobilelocale.[description] as mobilelocaledescription
                from localesetting ls
                    left join MobileLocaleMapping mlpd on ls.name = mlpd.desktoplocale
                    left join MobileLocaleMapping mobileexclusion on ls.name = mobileexclusion.mobilelocale
                    left join localesetting mobilelocale on mlpd.mobilelocale = mobilelocale.name
                where mobileexclusion.mobilelocale is null and ls.name = " + DB.SQuote(locale);
            using (SqlConnection dbConn = new SqlConnection(DB.GetDBConn()))
            {
                dbConn.Open();
                using (IDataReader idr = DB.GetRS(sql, dbConn))
                {
                    if (idr.Read())
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal static string GetMobileLocaleMapping(string desktopLocale)
        {
            string sql = @"
                select mobilelocale 
                from mobilelocalemapping mlm
                    join localesetting ls on ls.name = mlm.mobilelocale
                where DesktopLocale =  " + DB.SQuote(desktopLocale);
            using (SqlConnection dbConn = new SqlConnection(DB.GetDBConn()))
            {
                dbConn.Open();
                using (IDataReader idr = DB.GetRS(sql, dbConn))
                {
                    if (idr.Read())
                    {
                        return DB.RSField(idr, "mobilelocale");
                    }
                }
            }

            if (LocaleIsMobile(desktopLocale))
                return desktopLocale;

            return Vortx.Data.Config.MobilePlatform.MobileLocaleDefault;

        }

        internal static string GetDesktopLocaleMapping(string mobileLocale)
        {
            string sql = @"
                select desktoplocale 
                from mobilelocalemapping mlm
                    join localesetting ls on ls.name = mlm.desktoplocale
                where mobilelocale =  " + DB.SQuote(mobileLocale);
            using (SqlConnection dbConn = new SqlConnection(DB.GetDBConn()))
            {
                dbConn.Open();
                using (IDataReader idr = DB.GetRS(sql, dbConn))
                {
                    if (idr.Read())
                    {
                        return DB.RSField(idr, "desktoplocale");
                    }
                }
            }

            if (LocaleIsDesktop(mobileLocale))
                return mobileLocale;

            return Localization.GetDefaultLocale();
        }

        public static void CreateLocale(string locale, string description)
        {
            MobileCulture mc = new MobileCulture(locale);
            mc.CreateForDB(description);
        }

        public static string OverWriteLocaleData(string FromLocale, string ToLocale, bool StripHTML, List<LocaleTableUpdateRequest> TablesToUpdate)
        {
            AppLogic.m_RestartApp(); //require app restart to make sure locales are not cached

            StringBuilder errors = new StringBuilder();
            foreach (LocaleTableUpdateRequest tableUpdateRequeset in TablesToUpdate)
            {
                errors.Append(ProcessTable( tableUpdateRequeset.TableName, 
                                            FromLocale, 
                                            ToLocale, 
                                            StripHTML, 
                                            tableUpdateRequeset.FieldsToUpdate, 
                                            tableUpdateRequeset.UniqueIntIDFieldName));
            }

            return errors.ToString();
        }

        #region Generic Locale Management
        private static string ProcessTable(string entityName, string sourceLocale, string targetLocale, bool StripHTML, string[] fields, string UniqueIntIDFieldName)
        {
            string idName = UniqueIntIDFieldName;
            StringBuilder errors = new StringBuilder();

            string sSql = string.Format("SELECT * FROM {0} WHERE deleted = 0", entityName);
            using (SqlConnection dbConn = new SqlConnection(DB.GetDBConn()))
            {
                dbConn.Open();
                using (IDataReader idr = DB.GetRS(sSql, dbConn))
                {
                    while (idr.Read())
                    {
                        int entityId = DB.RSFieldInt(idr, idName);
                        if (entityId < 1)
                            return null;

                        string[] currentData = new string[fields.Length];
                        string[] updatedData = new string[fields.Length];
                        for (int i = 0; i < fields.Length; i++)
                        {
                            string CurrentDBValue = DB.RSField(idr, fields[i]);
                            string rowName = "";
                            if (entityName.ToLower() == "topic")
                            {
                                try
                                {
                                    {
                                        rowName = DB.RSFieldByLocale(idr, "name", sourceLocale);
                                    }
                                }
                                catch (Exception) { }
                            }
                            bool isHTMLStripExcludedTopic = rowName == "contact" || rowName.StartsWith("Mobile", StringComparison.InvariantCultureIgnoreCase);
                            
                            if (StripHTML && !isHTMLStripExcludedTopic)
                                currentData[i] = Utils.TextUtil.StripScriptAndHTML(DB.RSFieldByLocale(idr, fields[i], sourceLocale));
                            else
                                currentData[i] = DB.RSFieldByLocale(idr, fields[i], sourceLocale);

                            updatedData[i] = MobileFormLocaleXml(fields[i], currentData[i], targetLocale, CurrentDBValue);
                        }

                        // ok to update:
                        String update = String.Format("UPDATE {0} ", entityName);
                        String where = String.Format(" where {0}={1}", idName, entityId);

                        for (int i = 0; i < currentData.Length; i++)
                        {
                            string set = "set " + fields[i] + " = " + (String.IsNullOrEmpty(updatedData[i]) ? "NULL" : DB.SQuote(updatedData[i]));
                            try
                            {
                                DB.ExecuteSQL(update + set + where);
                            }
                            catch (Exception)
                            {
                                errors.Append("<!-- " + update + set + where + " -->");
                                errors.Append("Please Check the " + fields[i] + " for " + entityName + " " + entityId + ".\n");
                            }
                        }
                    }
                }
            }
            return errors.ToString();
        }

        static public String MobileFormLocaleXml(string sqlName, string formValue, string locale, string currentDBValue)
        {
            if (AppLogic.NumLocaleSettingsInstalled() < 2)
                return formValue;

            StringBuilder tmpS = new StringBuilder(4096);
            tmpS.Append("<ml>");
            XmlNodeList nl = Localization.LocalesDoc.SelectNodes("//Locales");
            foreach (XmlNode xn in nl)
            {
                String thisLocale = xn.Attributes["Name"].InnerText;
                string localeEntry = XmlCommon.GetLocaleEntry(currentDBValue, thisLocale, false);
                if (HasLocaleEntry(currentDBValue, thisLocale) || thisLocale.Equals(locale, StringComparison.InvariantCultureIgnoreCase)) // only include already existing locales and the locale currently being set
                {
                    if (thisLocale.Equals(locale, StringComparison.InvariantCultureIgnoreCase))
                    {
                        tmpS.Append("<locale name=\"" + thisLocale + "\">");
                        tmpS.Append(XmlCommon.XmlEncode(formValue));
                        tmpS.Append("</locale>");
                    }
                    else
                    {
                        tmpS.Append("<locale name=\"" + thisLocale + "\">");
                        tmpS.Append(XmlCommon.XmlEncode(localeEntry));
                        tmpS.Append("</locale>");
                    }
                }
            }
            tmpS.Append("</ml>");
            return tmpS.ToString();
        }

        public static Boolean HasLocaleEntry(String S, String LocaleSetting)
        {
            String WebConfigLocale = Localization.GetDefaultLocale();
            if (LocaleSetting.Equals(Localization.GetDefaultLocale(), StringComparison.InvariantCultureIgnoreCase))
                return true;
            if (S.Length == 0)
                return false;
            if (S.StartsWith("&lt;ml&gt;", StringComparison.InvariantCultureIgnoreCase))
                S = XmlCommon.XmlDecode(S);
            if (!S.StartsWith("<ml>", StringComparison.InvariantCultureIgnoreCase))
                return false;
                
            if (S.IndexOf("<locale name=\"" + LocaleSetting + "\">") != -1)
                return true;
            else
                return false;
        }
        #endregion

        public static void UpdateLongBuiltInStringResources(string TargetLocale)
        {
            Dictionary<String, String> UpdatedStrings = new Dictionary<string, string>();
            UpdatedStrings.Add("account.aspx.13", "*First Name:");
            UpdatedStrings.Add("account.aspx.14", "*Last Name:");
            UpdatedStrings.Add("account.aspx.15", "*E-Mail:");
            UpdatedStrings.Add("account.aspx.66", "Change Password:");
            UpdatedStrings.Add("account.aspx.67", "Confirm Password:");
            UpdatedStrings.Add("account.aspx.78", "13 years or older:");
            UpdatedStrings.Add("account.aspx.63", "Add/Edit Billing Addresses");
            UpdatedStrings.Add("account.aspx.62", "Add/Edit Shipping Addresses");
            UpdatedStrings.Add("shoppingcart.cs.27", "Option");

            // Checkout Shipping Strings
            UpdatedStrings.Add("checkoutshipping.aspx.15", "");
            UpdatedStrings.Add("checkoutshipping.aspx.13", "Continue Checkout");
            UpdatedStrings.Add("checkoutshipping.aspx.11", "Please select a shipping method:");

            // Checkout Payment Strings
            UpdatedStrings.Add("address.cs.23", "*Full Name:");
            UpdatedStrings.Add("checkoutcard.aspx.6", "Please enter your payment details:");
            UpdatedStrings.Add("address.cs.28", "*Verification Code:");

            // Checkout Review
            UpdatedStrings.Add("checkoutreview.aspx.6", "");
            UpdatedStrings.Add("checkoutreview.aspx.7", "Complete Order");

            String sql = @"
                IF (exists(select ConfigValue from StringResource where name = {0} and localesetting = {1}))
                Begin
                update stringresource set ConfigValue = {2} where name = {0} and localesetting = {1}
                End
                Else
                Begin
                INSERT INTO [dbo].[StringResource]
                           ([Name]
                           ,[LocaleSetting]
                           ,[ConfigValue]
                           )
                     VALUES
                           ({0}
                           ,{1}
                           ,{2})
                END
                ";
            foreach (String key in UpdatedStrings.Keys)
            {
                String Value = UpdatedStrings[key];
                DB.ExecuteSQL(String.Format(sql, DB.SQuote(key), DB.SQuote(TargetLocale), DB.SQuote(Value)));
            }
        }

        public static String CopyMobileLocaleForSystem(string fromLocale)
        {
            String MobilePostfix = "-MOBI";
            if (fromLocale.EndsWith(MobilePostfix))
                return fromLocale;

            string toLocale = fromLocale + MobilePostfix;
            //CultureAndRegionInfoBuilder cib = new CultureAndRegionInfoBuilder(this.MobileCultureName, CultureAndRegionModifiers.None);
            CultureAndRegionInfoBuilder cib = new CultureAndRegionInfoBuilder(toLocale, CultureAndRegionModifiers.None);

            // Populate the new CultureAndRegionInfoBuilder object with culture information.
            CultureInfo ci = new CultureInfo(fromLocale);
            cib.LoadDataFromCultureInfo(ci);

            // Populate the new CultureAndRegionInfoBuilder object with region information.
            String BaseRegion = fromLocale.Split('-')[1];
            RegionInfo ri = new RegionInfo(BaseRegion);
            cib.LoadDataFromRegionInfo(ri);

            cib.CultureEnglishName = "Mobile " + cib.CultureEnglishName;
            cib.CultureNativeName = "Mobile " + cib.CultureNativeName;

            try
            {
                cib.Register();
            }
            catch (UnauthorizedAccessException uaex)
            {
                throw uaex;
            }
            catch (Exception)
            {}
            return fromLocale + MobilePostfix;
        }

        public static void DeleteMobileSystemLocales()
        {
            CultureInfo[] systemcultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);

            foreach (CultureInfo ci in systemcultures)
            {
                if (ci.IetfLanguageTag.EndsWith("-Mobile") || ci.IetfLanguageTag.EndsWith("-MOBI"))
                {
                    CultureAndRegionInfoBuilder.Unregister(ci.Name);
                }
            }
        }

        public static string RemoveTableNameIndex(string tablename, string indexname)
        {
            try
            {
                DB.ExecuteSQL("drop Index "+indexname+" on " + tablename);
                return "Index " + indexname + " on " + tablename + " removed sucessfuly.";
            }
            catch (Exception)
            {
                return "Error removing the index " + indexname + " from " + tablename + ". It may have already been removed.\n";
            }
        }

        public static string LenthenNameField(string tablename, string namefield)
        {
            try
            {
                DB.ExecuteSQL("alter table " + tablename + " alter column " + namefield + " nvarchar(MAX)");
                return "Name field on " + tablename + " lengthened sucessfuly.";
            }
            catch (Exception)
            {
                return "Error lengthening the name field on " + tablename + ".\n";
            }
        }
    }

    public class LocaleTableUpdateRequest
    {
        public String TableName { get; set; }
        public String[] FieldsToUpdate { get; set; }
        public String UniqueIntIDFieldName { get; set; }

        public LocaleTableUpdateRequest(String tableName, String[] fieldsToUpdate, String uniqueIntIDFieldName)
        {
            TableName = tableName;
            FieldsToUpdate = fieldsToUpdate;
            UniqueIntIDFieldName = uniqueIntIDFieldName;
        }
    }
}
