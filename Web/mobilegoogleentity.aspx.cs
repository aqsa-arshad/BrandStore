// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Web;
using System.Web.SessionState;
using AspDotNetStorefrontCore;
using System.Text;
using System.Data.SqlClient;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for googleentity.
    /// </summary>
    public partial class mobilegoogleentity : System.Web.UI.Page
    {

        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.ContentType = "text/xml";
            Response.ContentEncoding = new System.Text.UTF8Encoding();
            Response.Write("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");

            String EntityName = CommonLogic.QueryStringCanBeDangerousContent("EntityName");
            AppLogic.CheckForScriptTag(EntityName);
            int EntityID = CommonLogic.QueryStringUSInt("EntityID");

            EntityHelper eHlp = AppLogic.LookupHelper(EntityName, AppLogic.StoreID());

            Response.Write("<urlset xmlns='http://www.sitemaps.org/schemas/sitemap/0.9' xmlns:mobile='http://www.google.com/schemas/sitemap-mobile/1.0'>\n");

            Response.Write("<url>");
            Response.Write("<loc>" + XmlCommon.XmlEncode(AppLogic.GetStoreHTTPLocation(false, false) + SE.MakeEntityLink(EntityName, EntityID, String.Empty)) + "</loc> ");
            Response.Write("<changefreq>" + AppLogic.AppConfig("GoogleSiteMap.EntityChangeFreq") + "</changefreq> ");
            Response.Write("<priority>" + AppLogic.AppConfig("GoogleSiteMap.EntityPriority") + "</priority> ");
            Response.Write("<mobile:mobile/></url>\n");

            Response.Write(GetMobileEntityGoogleObjectList(EntityDefinitions.LookupSpecs(EntityName), EntityID, Localization.GetDefaultLocale(), 0, 0));

            Response.Write("</urlset>");

        }

        public String GetMobileEntityGoogleObjectList(EntitySpecs eSpecs, int EntityID, String LocaleSetting, int AffiliateID, int CustomerLevelID)
        {
            String CacheName = String.Format("GetMobileEntityGoogleObjectList_{0}_{1}_{2}_{3}_{4}", eSpecs.m_EntityName, EntityID.ToString(), LocaleSetting, AffiliateID.ToString(), CustomerLevelID.ToString());
            if (AppLogic.CachingOn)
            {
                String s = (String)HttpContext.Current.Cache.Get(CacheName);
                if (s != null)
                {
                    return s;
                }
            }

            String StoreLoc = AppLogic.GetStoreHTTPLocation(false);
            StringBuilder tmpS = new StringBuilder(1000);
            String sql = GetMobileListProductsSQL(eSpecs, EntityID, AffiliateID, CustomerLevelID, true, true, false);

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS(sql, dbconn))
                {
                    while (rs.Read())
                    {
                        tmpS.Append("<url>");
                        tmpS.Append("<loc>" + StoreLoc + SE.MakeObjectAndEntityLink(eSpecs.m_ObjectName, eSpecs.m_EntityName, DB.RSFieldInt(rs, "ObjectID"), EntityID, DB.RSField(rs, "SEName")) + "</loc> ");
                        tmpS.Append("<changefreq>" + AppLogic.AppConfig("GoogleSiteMap.ObjectChangeFreq") + "</changefreq> ");
                        tmpS.Append("<priority>" + AppLogic.AppConfig("GoogleSiteMap.ObjectPriority") + "</priority> ");
                        tmpS.Append("<mobile:mobile/></url>\n");
                    }
                }
            }

            if (AppLogic.CachingOn)
            {
                HttpContext.Current.Cache.Insert(CacheName, tmpS.ToString(), null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }

            return tmpS.ToString();
        }

        public String GetMobileListProductsSQL(EntitySpecs eSpecs, int EntityInstanceID, int AffiliateID, int CustomerLevelID, bool AllowKits, bool AllowPacks, bool OrderByLooks)
        {
            String OneVariantSQL = String.Empty;
            String FilterSQL = String.Empty;
            String OrderBySQL = String.Empty;
            String FinalSQL = String.Empty;

            FinalSQL = "exec aspdnsf_GetSimpleObjectEntityList @entityname=" + DB.SQuote(eSpecs.m_EntityName);

            if (EntityInstanceID != 0)
            {
                FinalSQL += ", @entityid=" + EntityInstanceID.ToString();
            }

            if (AppLogic.AppConfigBool("Filter" + eSpecs.m_ObjectName + "sByAffiliate"))
            {
                FinalSQL += ", @affiliateid=" + AffiliateID.ToString();
            }
            if (AppLogic.AppConfigBool("Filter" + eSpecs.m_ObjectName + "sByCustomerLevel"))
            {
                FinalSQL += ", @customerlevelid=" + CustomerLevelID.ToString();

            }

            if (!AllowKits)
            {
                FinalSQL += ", @AllowKits=0";
            }

            if (!AllowPacks)
            {
                FinalSQL += ", @AllowPacks=0";
            }
            if (!AppLogic.IsAdminSite)
            {
                FinalSQL += ", @PublishedOnly=1";
            }

            if (OrderByLooks)
            {
                FinalSQL += ", @OrderByLooks=1";
            }

            return FinalSQL;
        }
    }
}
