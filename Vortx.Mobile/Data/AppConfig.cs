// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Data;
using System.Configuration;
using System;
using System.Data.SqlClient;
using AspDotNetStorefrontCore;
using System.Text;

/// <summary>
/// Utility class for wrapping appconfigs
/// </summary>
/// 
namespace Vortx.Data
{
    namespace Config
    {
        public class ConfigCommon
        {
            /// 
            private static string GetAppConfig(string sAppConfigName)
            {                
                return AppLogic.AppConfig(sAppConfigName);
            }

            public static T GetConfig<T>(string fieldName)
            {
                string appConfig = GetAppConfig(fieldName);

                if (string.IsNullOrEmpty(appConfig))
                    return default(T);

                return (T)Convert.ChangeType(appConfig, typeof(T));
            }

            public static void SetConfig(string name, object value)
            {
                AppLogic.SetAppConfig(name, value.ToString());
            }

            public static bool AppConfigValueExists(string Value, bool ignoreCase)
            {
                string DBVal = "";
                bool hitMatch = false;

                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS("select configvalue from appconfig where configvalue = " + DB.SQuote(Value), con))
                    {
                        while (rs.Read())
                        {
                            DBVal = DB.RSField(rs, "configvalue");
                            if (Value == DBVal || ignoreCase && Value.ToLowerInvariant() == DBVal.ToLowerInvariant())
                            {
                                hitMatch = true;
                            }
                        }
                    }
                }
                return hitMatch;
            }
        }

        public class MobilePlatform
        {
            public static int SkinId
            {
                get { return ConfigCommon.GetConfig<int>("Mobile.SkinId"); }
                set { ConfigCommon.SetConfig("Mobile.SkinId", value); }
            }
            public static string MobileLocaleDefault
            {
                get { return ConfigCommon.GetConfig<string>("Mobile.DefaultLocaleSetting"); }
                set { ConfigCommon.SetConfig("Mobile.DefaultLocaleSetting", value); }
            }
            public static bool IncludeEmailLinks
            {
                get { return ConfigCommon.GetConfig<bool>("Mobile.IncludeEmailLinks"); }
                set { ConfigCommon.SetConfig("Mobile.IncludeEmailLinks", value); }
            }
            public static bool IsEnabled
            {
                get { return ConfigCommon.GetConfig<bool>("Mobile.IsEnabled"); }
                set { ConfigCommon.SetConfig("Mobile.IsEnabled", value); }
            }
            public static string PageExceptions
            {
                get { return ConfigCommon.GetConfig<string>("Mobile.PageExceptions"); }
                set { ConfigCommon.SetConfig("Mobile.PageExceptions", value); }
            }
            public static bool IncludePhoneLinks
            {
                get { return ConfigCommon.GetConfig<bool>("Mobile.IncludePhoneLinks"); }
                set { ConfigCommon.SetConfig("Mobile.IncludePhoneLinks", value); }
            }
            public static bool TopicsShowImages
            {
                get { return ConfigCommon.GetConfig<bool>("Mobile.TopicsShowImages"); }
                set { ConfigCommon.SetConfig("Mobile.TopicsShowImages", value); }
            }
            public static string ContactPhoneNumber
            {
                get { return ConfigCommon.GetConfig<string>("Mobile.ContactPhoneNumber"); }
                set { ConfigCommon.SetConfig("Mobile.ContactPhoneNumber", value); }
            }
            public static string DefaultXmlPackageProduct
            {
                get { return ConfigCommon.GetConfig<string>("Mobile.DefaultXmlPackageProduct"); }
                set { ConfigCommon.SetConfig("Mobile.DefaultXmlPackageProduct", value); }
            }
            public static string DefaultXmlPackageEntity
            {
                get { return ConfigCommon.GetConfig<string>("Mobile.DefaultXmlPackageEntity"); }
                set { ConfigCommon.SetConfig("Mobile.DefaultXmlPackageEntity", value); }
            }
            //useragent lists
            public static string UserAgentList
            {
                get { return ConfigCommon.GetConfig<string>("Mobile.UserAgentList"); }
                set { ConfigCommon.SetConfig("Mobile.UserAgentList", value); }
            }
            public static string ShortUserAgentList
            {
                get { return ConfigCommon.GetConfig<string>("Mobile.ShortUserAgentList"); }
                set { ConfigCommon.SetConfig("Mobile.ShortUserAgentList", value); }
            }

            //Entity Config
            public static int EntityImageWidth
            {
                get { return ConfigCommon.GetConfig<int>("Mobile.Entity.ImageWidth"); }
                set { ConfigCommon.SetConfig("Mobile.Entity.ImageWidth", value); }
            }
            public static int EntityPageSize
            {
                get { return ConfigCommon.GetConfig<int>("Mobile.Entity.PageSize"); }
                set { ConfigCommon.SetConfig("Mobile.Entity.PageSize", value); }
            }

            //slider configs
            public static int ProductSliderMaxProducts
            {
                get { return ConfigCommon.GetConfig<int>("Mobile.ProductSlider.MaxProducts"); }
                set { ConfigCommon.SetConfig("Mobile.ProductSlider.MaxProducts", value); }
            }
            public static int ProductSliderWidth
            {
                get { return ConfigCommon.GetConfig<int>("Mobile.ProductSlider.Width"); }
                set { ConfigCommon.SetConfig("Mobile.ProductSlider.Width", value); }
            }
            public static int ProductSliderImageWidth
            {
                get { return ConfigCommon.GetConfig<int>("Mobile.ProductSlider.ImageWidth"); }
                set { ConfigCommon.SetConfig("Mobile.ProductSlider.ImageWidth", value); }
            }
            //checkout
            public static bool ShowAlternateCheckouts
            {
                get { return ConfigCommon.GetConfig<bool>("Mobile.ShowAlternateCheckouts"); }
                set { ConfigCommon.SetConfig("Mobile.ShowAlternateCheckouts", value); }
            }

            public static bool AllowAddressChangeOnCheckoutShipping
            {
                get { return ConfigCommon.GetConfig<bool>("Mobile.AllowAddressChangeOnCheckoutShipping"); }
                set { ConfigCommon.SetConfig("Mobile.AllowAddressChangeOnCheckoutShipping", value); }
            }
            public static bool AllowMultiShipOnCheckout
            {
                get { return ConfigCommon.GetConfig<bool>("Mobile.AllowMultiShipOnCheckout"); }
                set { ConfigCommon.SetConfig("Mobile.AllowMultiShipOnCheckout", value); }
            }

            public static bool ShowMobileOniPad
            {
                get { return ConfigCommon.GetConfig<bool>("Mobile.ShowMobileOniPad"); }
                set { ConfigCommon.SetConfig("Mobile.ShowMobileOniPad", value); }
            }

            public static void CreateConfig(String name, String description, String value, String group)
            {
                CreateConfig(name, description, value, group, "string", null);
            }

            public static void CreateConfig(String name, String description, String value, String group, String type, String[] AllowedValues)
            {
                if (!AppLogic.AppConfigExists(name))
                    AppConfigManager.CreateDBAndCacheAppConfig(name, description, value, type, JoinStringArray(AllowedValues, ","), group, false, 0);
            }

            private static String JoinStringArray(String[] sa, String joinDelimiter)
            {
                if (sa == null || sa.Length == 0)
                    return String.Empty;

                StringBuilder av = new StringBuilder();
                for (int i = 0; i < sa.Length; i++)
                {
                    if (i != 0)
                        av.Append(joinDelimiter);
                    av.Append(sa[i]);
                }
                return av.ToString();
            }
        }
    }
}
