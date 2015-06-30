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

namespace AspDotNetStorefront
{
	/// <summary>
    /// Summary description for rorindex.
	/// </summary>
    public partial class rorindex : System.Web.UI.Page
	{

		protected void Page_Load(object sender, System.EventArgs e)
		{
			Response.ContentType = "text/xml";
			Response.ContentEncoding = new System.Text.UTF8Encoding();
			Response.Write("<?xml version='1.0' encoding='utf-8'?>");
            Response.Write("<rss version='2.0' xmlns:ror='http://rorweb.com/0.1/'>");
            Response.Write("<channel>");

            String StoreLoc = AppLogic.GetStoreHTTPLocation(false);


            Response.Write("<title>" + XmlCommon.XmlEncode(AppLogic.AppConfig("SE_MetaTitle")) + "</title>");
            Response.Write("<link>" + StoreLoc + "</link>");

            Response.Write("<item>");
            Response.Write("    <title>" + XmlCommon.XmlEncode(AppLogic.AppConfig("SE_MetaTitle")) + "</title>");
            Response.Write("    <link>" + StoreLoc + "</link>");
            Response.Write("    <description>" + XmlCommon.XmlEncode(AppLogic.AppConfig("SE_MetaDescription")) + "</description>");
            Response.Write("    <ror:type>Main</ror:type>");
            Response.Write("    <ror:keywords>" + XmlCommon.XmlEncode(AppLogic.AppConfig("SE_MetaKeywords")) + "</ror:keywords>");
            Response.Write("    <ror:image></ror:image>"); // not supported
            Response.Write("    <ror:updated>" + System.DateTime.Now.Year.ToString() + "-" + System.DateTime.Now.Month.ToString() + "-" + System.DateTime.Now.Day.ToString() + "</ror:updated>");
            Response.Write("    <ror:updatePeriod>day</ror:updatePeriod>");
            Response.Write("</item>");

            Response.Write("<item>");
            Response.Write("    <title>Articles</title> ");
            Response.Write("    <ror:type>Articles</ror:type>");
            Response.Write("    <ror:seeAlso>" + StoreLoc + "rortopics.aspx</ror:seeAlso>");
            Response.Write("</item>");

            Response.Write(AppLogic.CategoryStoreEntityHelper[0].GetEntityRorSiteMap(0, Localization.GetDefaultLocale(), true, true));
            Response.Write(AppLogic.SectionStoreEntityHelper[0].GetEntityRorSiteMap(0, Localization.GetDefaultLocale(), true, true));
            Response.Write(AppLogic.ManufacturerStoreEntityHelper[0].GetEntityRorSiteMap(0, Localization.GetDefaultLocale(), true, true));
            Response.Write(AppLogic.DistributorStoreEntityHelper[0].GetEntityRorSiteMap(0, Localization.GetDefaultLocale(), true, true));
            Response.Write(AppLogic.LibraryStoreEntityHelper[0].GetEntityRorSiteMap(0, Localization.GetDefaultLocale(), true, true));
            Response.Write(AppLogic.GenreStoreEntityHelper[0].GetEntityRorSiteMap(0, Localization.GetDefaultLocale(), true, true));
            Response.Write(AppLogic.VectorStoreEntityHelper[0].GetEntityRorSiteMap(0, Localization.GetDefaultLocale(), true, true));
			Response.Write("</channel>");
			Response.Write("</rss>");

		}

	}
}
