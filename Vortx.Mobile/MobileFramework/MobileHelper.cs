// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using AspDotNetStorefrontCore;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Vortx.Data.Config;



namespace Vortx.MobileFramework
{
    public class MobileHelper
    {
        private static readonly string ForceMobileCookie = "ASPDNSF.IsMobile";
		private MobileHelper() { } //CA1053: Static holder types should not have constructors

        public static bool isMobile()
        {
            return isMobile(true);
        }

        public static bool isMobile(bool AllowCookieOverride)
        {
			if (AppLogic.IsAdminSite == true || !MobilePlatform.IsEnabled)
            {
                return false;
            }

            if (AllowCookieOverride && CommonLogic.CookieCanBeDangerousContent(ForceMobileCookie, false).Length > 0)
            {
                return CommonLogic.CookieBool(ForceMobileCookie);
            }

            //example userAgentString
            //Mozilla/5.0 (iPhone; U; CPU iPhone OS 3_0 like Mac OS X; en-us) AppleWebKit/528.18 (KHTML, like Gecko) Version/4.0 Mobile/7A341 Safari/528.16
            string userAgentString = CommonLogic.ServerVariables("HTTP_USER_AGENT").ToUpperInvariant();
            string httpAccept = CommonLogic.ServerVariables("HTTP_ACCEPT");
            string xProfile = CommonLogic.ServerVariables("HTTP_X_PROFILE");
            string httpProfile = CommonLogic.ServerVariables("HTTP_PROFILE");
            string userAgentList = MobilePlatform.UserAgentList ?? String.Empty; //android, palm, motorola, etc
            string shortUserAgentList = MobilePlatform.ShortUserAgentList ?? String.Empty; //moto, noki, sany, etc

            if (!MobilePlatform.ShowMobileOniPad && userAgentString.Contains("IPAD;"))
            {
                return false;
            }

            if (httpAccept.Contains("application/vnd.wap.xhtml+xml") || xProfile.Length > 0 || httpProfile.Length > 0)
            {
                SetMobileContextItem(true);
                return true;
            }

			//check for most common mobile
            string[] agentList = userAgentList.ToUpperInvariant().Split(',');
            //check if userAgentString contains any of our agents in the agentList
            if (agentList.Any(userAgentString.Contains))
			{
				SetMobileContextItem(true);
				return true;
			}

			//check for mobile that slipped through with longer list of substrings
            string[] shortAgentList = shortUserAgentList.ToUpperInvariant().Split(',');
            //check if userAgentString contains any of our agents in the shortAgentList
            if (shortAgentList.Any(userAgentString.Contains))
			{
				SetMobileContextItem(true);
				return true;
			}

			return false;
		}

        public static void SetMobileContextItem(bool value)
        {
            HttpContext.Current.Items["IsMobile"] = value;
        }

        public static void SetCustomerToMobileSkinId(Customer c)
        {
            c.SkinID = MobilePlatform.SkinId;
        }

        public static void SetMobileSkinLocale(Customer c)
        {
            if (!LocaleManagement.LocaleIsMobile(c.LocaleSetting))
            {
                c.LocaleSetting = LocaleManagement.GetMobileLocaleMapping(c.LocaleSetting);
            }
        }

        public static void SetDesktopSkinLocale(Customer c, int SkinID)
        {
            if (c.SkinID == MobilePlatform.SkinId || SkinID == MobilePlatform.SkinId)
            {
                c.SkinID = AppLogic.GetStoreSkinID(AppLogic.StoreID());
            }
            else
            {
                c.SkinID = SkinID;
            }
            if (!LocaleManagement.LocaleIsDesktop(c.LocaleSetting))
            {
                c.LocaleSetting = LocaleManagement.GetDesktopLocaleMapping(c.LocaleSetting);
            }
        }

        public static void setForceMobileCookie(Customer ThisCustomer)
        {
            AppLogic.SetCookie(ForceMobileCookie, "True", new TimeSpan(240, 0, 0));
            SetCustomerToMobileSkinId(ThisCustomer);
            SetMobileSkinLocale(ThisCustomer);
        }

        public static void setForceDesktopCookie(Customer ThisCustomer)
        {
            AppLogic.SetCookie(ForceMobileCookie, "False", new TimeSpan(240, 0, 0));
            SetDesktopSkinLocale(ThisCustomer, AppLogic.GetStoreSkinID(AppLogic.StoreID()));
        }

        public static void removeForceCookie(Customer ThisCustomer)
        {
            if (HttpContext.Current.Response.Cookies[ForceMobileCookie] != null)
            {
                HttpContext.Current.Response.Cookies[ForceMobileCookie].Expires = System.DateTime.Now.AddDays(-1);
            }
            if (isMobile(false))
            {
                SetCustomerToMobileSkinId(ThisCustomer);
                SetMobileSkinLocale(ThisCustomer);
            }
            else
            {
                SetDesktopSkinLocale(ThisCustomer, AppLogic.GetStoreSkinID(AppLogic.StoreID()));
            }
        }

        public static int GetMLTopicID(string topicName)
        {
            int tmp = 0;
            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS(string.Format("select TopicID from Topic with (NOLOCK) where Deleted=0 and Published=1 and lower(Name)={0}", DB.SQuote(topicName.ToLowerInvariant())), con))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSFieldInt(rs, "TopicID");
                    }
                }
            }

            if (tmp == 0)
            {
                int topicID = 0;
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    string dbTopicName = topicName.Replace("-", " ");

                    con.Open();
                    using (IDataReader rs = DB.GetRS(string.Format("select TopicID from Topic with (NOLOCK) where Deleted=0 and Published=1 and Name like {0} Order By DisplayOrder, Name ASC", DB.SQuote("%>" + dbTopicName + "</locale>%")), con))
                    {
                        if (rs.Read())
                        {
                            topicID = DB.RSFieldInt(rs, "TopicID");
                            if (topicID > 0)
                            {
                                return topicID;
                            }
                        }
                    }
                }
            }
            return 0;
        }

        public static void RedirectPageWhenMobileIsDisabled(string redirectTo, Customer thisCustomer)
        {
            try
            {
                if (!isMobile())
                {
                    removeForceCookie(thisCustomer);
                    System.Web.UI.Page page = HttpContext.Current.Handler as System.Web.UI.Page;
                    page.Response.Redirect(redirectTo);
                }
            }
            catch { }
        }
    }
}
