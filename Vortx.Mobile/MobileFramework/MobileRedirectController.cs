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
using Vortx.Data.Config;

namespace Vortx.MobileFramework
{
    public class MobileRedirectController
    {
        #region Private Static Variables
        private static String[] DriverPages = { "default.aspx", "engine.aspx", "driver.aspx", "driver2.aspx", "driver3.aspx", "showcategory.aspx", "showdistributor.aspx", "showgenre.aspx", "showvector.aspx", "showsection.aspx", "showmanufacturer.aspx", "showproduct.aspx", "showlibrary.aspx", "execpackage.aspx" };
        private static String[] DriverPageTypes = { "ASP.default_aspx", "ASP.engine_aspx", "ASP.driver_aspx", "ASP.showcategory_aspx", "ASP.showdistributor_aspx", "ASP.showgenre_aspx", "ASP.showvector_aspx", "ASP.showsection_aspx", "ASP.showmanufacturer_aspx", "ASP.showproduct_aspx", "ASP.showlibrary_aspx" };
        private static String[] MobilePageExceptions = { "reorder.aspx", "disclaimer.aspx", "addtocart.aspx", "sendform.aspx", "signout.aspx", "secureprocess.aspx" };
        #endregion
        #region Mobile Redirect, Skin, Locale Driver
        public static Customer SkinBaseHook(int SkinID, Customer ThisCustomer)
        {
            if (!MobileHelper.isMobile())
            {
                MobileHelper.SetDesktopSkinLocale(ThisCustomer, SkinID);
            }
            else
            {
                MobileHelper.SetCustomerToMobileSkinId(ThisCustomer);

                string pageName = CommonLogic.IIF(
                                HttpContext.Current.Items["RequestedPage"] != null,
                                HttpContext.Current.Items["RequestedPage"].ToString(),
                                CommonLogic.GetThisPageName(false));

                PageType pt = GetPageType(pageName, CommonLogic.GetThisPageName(false), HttpContext.Current.Handler);


                switch (pt)
                {
                    case PageType.Driver:
                        //continue to page - place exceptions here.
                        break;
                    case PageType.PageWithRedirect:
                        String redirect = GetRedirect(pageName, CommonLogic.GetThisPageName(false));
                        if (CommonLogic.ServerVariables("QUERY_STRING").Length > 0)
                        {
                            redirect += "?" + CommonLogic.ServerVariables("QUERY_STRING");
                        }
                        HttpContext.Current.Response.Redirect(redirect);
                        break;
                    case PageType.DesktopPageWithoutRedirect:
                        String middleManRedirect = pageName;
                        String referer = "js";
                        if (HttpContext.Current.Request.UrlReferrer != null)
                        {
                            string tmp = HttpContext.Current.Request.UrlReferrer.ToString();
                            int lastindex = tmp.LastIndexOf('/') + 1;
                            if (lastindex > -1)
                            {
                                tmp = tmp.Substring(lastindex, tmp.Length - lastindex);
                            }
                            referer = tmp == "" ? "js" : tmp.ToString();
                        }
                        HttpContext.Current.Response.Redirect(AppLogic.GetStoreHTTPLocation(true) +  "mobiledesktopwarning.aspx?currentpage="+Security.UrlEncode(referer)+"&targetpage=" + Security.UrlEncode(pageName));
                        break;
                    case PageType.Mobile:
                        //continue to page
                        break;
                    default:
                        break;
                }
            }

            return ThisCustomer;
        }
        #endregion
        #region Private Helper Methods
        
        private static String[] GetUserPageExceptions()
        {
            String[] retarray = { };
            String ConfigName = "Mobile.PageExceptions";
            if (AppConfigManager.AppConfigExists(ConfigName))
            {
                retarray = AppLogic.AppConfig(ConfigName).Replace(" ", "").Split(',');
            }
            return retarray;
        }

        private static Boolean StringArrayContains(String[] a, String st, StringComparison comparisionType)
        {
            foreach (string s in a)
	        {
                if (s.Equals(st, comparisionType))
                {
                    return true;
                }
	        }
            return false;
        }

        private static PageType GetPageType(string RequestedPage, string ServedPage, IHttpHandler Handler)
        {
            ServedPage = ServedPage.ToLowerInvariant();

            if(StringArrayContains(GetUserPageExceptions(), ServedPage, StringComparison.InvariantCultureIgnoreCase))
            {
                return PageType.Mobile;
            }
            if (GetRedirect(RequestedPage, ServedPage).Length > 0)
            {
                if (GetRedirect(RequestedPage, ServedPage) == ServedPage)
                {
                    return PageType.Mobile;
                }
                return PageType.PageWithRedirect;
            }
            if(StringArrayContains(DriverPages, ServedPage, StringComparison.InvariantCultureIgnoreCase))
            {
                return PageType.Driver;
            }
            if (StringArrayContains(DriverPageTypes, Handler.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                return PageType.Driver;
            }
            if (StringArrayContains(MobilePageExceptions, ServedPage, StringComparison.InvariantCultureIgnoreCase))
            {
                return PageType.Mobile;
            }
            
            if (ServedPage.ToLowerInvariant().StartsWith("mobile"))
            {
                return PageType.Mobile;
            }
            if (ConfigCommon.AppConfigValueExists(ServedPage, true))
            {
                return PageType.Mobile;
            }

            return PageType.DesktopPageWithoutRedirect;
        }

        private static string GetRedirect(string RequestedPage, string ServedPage)
        {
            String AppConfigPrefix = "Mobile.Redirect.";
            String RequestedPageConfigName = AppConfigPrefix + RequestedPage;
            if (AppLogic.AppConfigExists(RequestedPageConfigName))
            {
                return AppLogic.AppConfig(RequestedPageConfigName);
            }
            String ServedPageConfigName = AppConfigPrefix + ServedPage;
            if (AppLogic.AppConfigExists(ServedPageConfigName))
            {
                return AppLogic.AppConfig(ServedPageConfigName);
            }

            if (CommonLogic.FileExists("mobile" + RequestedPage))
            {
                return "mobile" + RequestedPage;
            }
            return "";
        }

        enum PageType
        {
            Driver,
            PageWithRedirect,
            DesktopPageWithoutRedirect,
            Mobile
        }
        #endregion
    }
}
