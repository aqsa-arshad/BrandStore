// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Configuration;
using System.Web.SessionState;
using System.Web.Caching;
using System.Web.Util;
using System.Data;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;
using System.Globalization;

namespace AspDotNetStorefrontCore
{
    /// <summary>
    /// Summary description for SiteMap1PhoneOrder.
    /// </summary>
    public class SiteMap1PhoneOrder
    {
        private String m_Contents;

        public String Contents
        {
            get
            {
                return m_Contents;
            }
        }

        public SiteMap1PhoneOrder(System.Collections.Generic.Dictionary<string, EntityHelper> EntityHelpers, int SkinID, Customer ThisCustomer, String IGD)
        {
            bool FromCache = false;
            String CacheName = String.Format("SiteMap1PhoneOrder_{0}_{1}_{2}", SkinID.ToString(), ThisCustomer.LocaleSetting, IGD);
            if (AppLogic.CachingOn)
            {
                m_Contents = (String)HttpContext.Current.Cache.Get(CacheName);
                if (m_Contents != null)
                {
                    FromCache = true;
                }
            }

            if (!FromCache)
            {
                StringBuilder tmpS = new StringBuilder(50000);

                // Categories:
                String s = AppLogic.LookupHelper("Category", 0).GetEntityULListPhone(0, ThisCustomer.LocaleSetting, ThisCustomer.AffiliateID, ThisCustomer.CustomerLevelID, true, AppLogic.AppConfigBool("SiteMap.ShowProducts") && AppLogic.NumProductsInDB < 250, true, "sitemapul", true, 0, String.Empty, IGD);
                if (s.Length != 0)
                {
                    tmpS.Append("<b>");
                    tmpS.Append(AppLogic.GetString("AppConfig.CategoryPromptPlural", SkinID, ThisCustomer.LocaleSetting).ToUpperInvariant());
                    tmpS.Append("</b>");
                    tmpS.Append(s);
                }
                // Sections:
                s = AppLogic.LookupHelper("Section", 0).GetEntityULListPhone(0, ThisCustomer.LocaleSetting, ThisCustomer.AffiliateID, ThisCustomer.CustomerLevelID, true, AppLogic.AppConfigBool("SiteMap.ShowProducts") && AppLogic.NumProductsInDB < 250, true, "sitemapul", true, 0, String.Empty, IGD);
                if (s.Length != 0)
                {
                    tmpS.Append("<b>");
                    tmpS.Append(AppLogic.GetString("AppConfig.SectionPromptPlural", SkinID, ThisCustomer.LocaleSetting).ToUpperInvariant());
                    tmpS.Append("</b>");
                    tmpS.Append(s);
                }

                // Manufacturers:
                s = AppLogic.LookupHelper("Manufacturer", 0).GetEntityULListPhone(0, ThisCustomer.LocaleSetting, ThisCustomer.AffiliateID, ThisCustomer.CustomerLevelID, true, AppLogic.AppConfigBool("SiteMap.ShowProducts") && AppLogic.NumProductsInDB < 250, true, "sitemapul", true, 0, String.Empty, IGD);
                if (s.Length != 0)
                {
                    tmpS.Append("<b>");
                    tmpS.Append(AppLogic.GetString("AppConfig.ManufacturerPromptPlural", SkinID, ThisCustomer.LocaleSetting).ToUpperInvariant());
                    tmpS.Append("</b>");
                    tmpS.Append(s);
                }
                m_Contents = tmpS.ToString();
            }

            if (AppLogic.CachingOn)
            {
                HttpContext.Current.Cache.Insert(CacheName, m_Contents, null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }
        }

    }
}
