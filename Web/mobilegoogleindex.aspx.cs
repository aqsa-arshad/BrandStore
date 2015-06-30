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
using System.IO;
using System.Xml.Xsl;
using System.Web.Caching;
using Vortx.Data.Config;
using Vortx.MobileFramework;

namespace AspDotNetStorefront
{
	/// <summary>
	/// Summary description for googleindex.
	/// </summary>
    public partial class mobilegoogleindex : System.Web.UI.Page
	{

		protected void Page_Load(object sender, System.EventArgs e)
		{
            Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;

            MobileHelper.RedirectPageWhenMobileIsDisabled("~/googleindex.aspx", ThisCustomer);

			Response.ContentType = "text/xml";
			Response.ContentEncoding = new System.Text.UTF8Encoding();
            Response.Write("<?xml version='1.0' encoding='UTF-8'?>\n");

            Response.Write("<sitemapindex xmlns='http://www.sitemaps.org/schemas/sitemap/0.9'>\n");
            Response.Write("<sitemap>");
            Response.Write("<loc>" + AppLogic.GetStoreHTTPLocation(false) + "mobilegoogletopics.aspx</loc>");
            Response.Write("</sitemap>\n");

            Response.Write(GetMobileEntityGoogleSiteMap(AppLogic.CategoryStoreEntityHelper[0], EntityDefinitions.LookupSpecs("Category"), 0, MobilePlatform.MobileLocaleDefault, true, true));
            Response.Write(GetMobileEntityGoogleSiteMap(AppLogic.SectionStoreEntityHelper[0], EntityDefinitions.LookupSpecs("Section"), 0, MobilePlatform.MobileLocaleDefault, true, true));
            Response.Write(GetMobileEntityGoogleSiteMap(AppLogic.ManufacturerStoreEntityHelper[0], EntityDefinitions.LookupSpecs("Manufacturer"), 0, MobilePlatform.MobileLocaleDefault, true, true));
            Response.Write(GetMobileEntityGoogleSiteMap(AppLogic.DistributorStoreEntityHelper[0], EntityDefinitions.LookupSpecs("Distributor"), 0, MobilePlatform.MobileLocaleDefault, true, true));
            Response.Write(GetMobileEntityGoogleSiteMap(AppLogic.GenreStoreEntityHelper[0], EntityDefinitions.LookupSpecs("Genre"), 0, MobilePlatform.MobileLocaleDefault, true, true));
            Response.Write(GetMobileEntityGoogleSiteMap(AppLogic.VectorStoreEntityHelper[0], EntityDefinitions.LookupSpecs("Vector"), 0, MobilePlatform.MobileLocaleDefault, true, true));
            Response.Write(GetMobileEntityGoogleSiteMap(AppLogic.LibraryStoreEntityHelper[0], EntityDefinitions.LookupSpecs("Library"), 0, MobilePlatform.MobileLocaleDefault, true, true));
            
            Response.Write("</sitemapindex>");
		}

        public String GetMobileEntityGoogleSiteMap(EntityHelper passedEntityHelper, EntitySpecs eSpecs, int ForParentEntityID, String LocaleSetting, bool AllowCaching, bool RecurseChildren)
        {
            String CacheName = String.Format("GetMobileEntityGoogleSiteMap_{0}_{1}_{2}_{3}", eSpecs.m_EntityName, ForParentEntityID.ToString(), LocaleSetting, RecurseChildren.ToString());
            if (AppLogic.CachingOn)
            {
                String Menu = (String)HttpContext.Current.Cache.Get(CacheName);
                if (Menu != null)
                {
                    return Menu;
                }
            }
            String StoreLoc = AppLogic.GetStoreHTTPLocation(false);

            StringWriter tmpS = new StringWriter();
            String XslFile = "MobileEntityGoogleSiteMap";
            XslCompiledTransform xForm;
            string XslFilePath = CommonLogic.SafeMapPath(XslFile + ".xslt");
            xForm = (XslCompiledTransform)HttpContext.Current.Cache.Get(XslFilePath);
            if (xForm == null)
            {
                xForm = new XslCompiledTransform(false);
                xForm.Load(XslFilePath);
                HttpContext.Current.Cache.Insert(XslFilePath, xForm, new CacheDependency(XslFilePath));
            }
            XsltArgumentList xslArgs = new XsltArgumentList();
            xslArgs.AddParam("entity", "", eSpecs.m_EntityName);
            xslArgs.AddParam("ForParentEntityID", "", ForParentEntityID);
            xslArgs.AddParam("StoreLoc", "", StoreLoc);
            xForm.Transform(passedEntityHelper.m_TblMgr.XmlDoc, xslArgs, tmpS);

            if (AppLogic.CachingOn)
            {
                HttpContext.Current.Cache.Insert(CacheName, tmpS.ToString(), null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }
            return tmpS.ToString();
        }

	}
}
