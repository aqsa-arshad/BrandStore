// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Xml.Serialization;
using System.IO;
using System.Text;
using System.Collections.Generic;
using AspDotNetStorefrontCommon;
using System.Text.RegularExpressions;
using System.Web.Caching;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for SiteMapProviderFactory
    /// </summary>
    public static class SiteMapProviderFactory
    {
        /// <summary>
        /// Gets or loads the sitemap provider, locale aware
        /// </summary>
        /// <param name="thisCustomer"></param>
        /// <returns></returns>
        public static ASPDNSFSiteMapProvider GetSiteMap(Customer thisCustomer)
        {
            return GetSiteMap(thisCustomer, AppLogic.AppConfigUSInt("MaxMenuLevel"));
        }

        /// <summary>
        /// Gets or loads the sitemap provider, locale aware
        /// </summary>
        /// <param name="thisCustomer"></param>
        /// <param name="maxDynamicDisplayLvl"></param>
        /// <returns></returns>
        public static ASPDNSFSiteMapProvider GetSiteMap(Customer thisCustomer, int maxDynamicDisplayLvl)
        {
            bool cachingOn = AppLogic.AppConfigBool("CacheMenus");

            ASPDNSFSiteMapProvider prov = null;

            // caching is dependent on the locale-setting and storeid
            string cacheKey = "AspNetMenu_{0}_{1}".FormatWith(thisCustomer.LocaleSetting.ToLowerInvariant(), AppLogic.StoreID());

            if (cachingOn)
            {
                // check if already chached
                prov = HttpRuntime.Cache[cacheKey] as ASPDNSFSiteMapProvider;
            }

            if (prov == null)
            {
                // We'll pass the customer object but the only relevant information is the locale
                // the customer object is just used during the XmlPackage render call
                prov = new ASPDNSFSiteMapProvider(thisCustomer);
                prov.XmlPackage = "page.menu.xml.config";
                prov.MaximumDynamicDisplayLevels = maxDynamicDisplayLvl;
                prov.BuildSiteMap();

                // cache if caching is turned on
                if (cachingOn)
                {
                    // cache duration is controlled by the appconfig CacheDurationMinutes
                    // which will expire the cached object N-minutes from now
                    int cacheDurationInMinutes = AppLogic.AppConfigUSInt("CacheDurationMinutes");
                    if (cacheDurationInMinutes <= 0)
                    {
                        // reset to default
                        cacheDurationInMinutes = 60;
                    }

                    DateTime expireOn = DateTime.UtcNow.AddMinutes(cacheDurationInMinutes);
                    HttpRuntime.Cache.Insert(cacheKey, prov, null, expireOn, Cache.NoSlidingExpiration);
                }
            }

            return prov;
        }
    }
}

