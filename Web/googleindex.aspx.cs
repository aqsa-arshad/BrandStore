// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront
{
	/// <summary>
	/// Summary description for googleindex.
	/// </summary>
	public partial class googleindex : System.Web.UI.Page
	{

		protected void Page_Load(object sender, System.EventArgs e)
		{
			Response.ContentType = "text/xml";
			Response.ContentEncoding = new System.Text.UTF8Encoding();
            Response.Write("<?xml version='1.0' encoding='UTF-8'?>\n");

            Response.Write("<sitemapindex xmlns='http://www.sitemaps.org/schemas/sitemap/0.9'>\n");
            Response.Write("<sitemap>");
            Response.Write("<loc>" + AppLogic.GetStoreHTTPLocation(false) + "googletopics.aspx</loc>");
            Response.Write("</sitemap>\n");

            Response.Write(GoogleSiteMap.GetGoogleEntitySiteMap("category"));
            Response.Write(GoogleSiteMap.GetGoogleEntitySiteMap("section"));
            Response.Write(GoogleSiteMap.GetGoogleEntitySiteMap("manufacturer"));
            Response.Write(GoogleSiteMap.GetGoogleEntitySiteMap("section"));
            Response.Write(GoogleSiteMap.GetGoogleEntitySiteMap("library"));
            Response.Write(GoogleSiteMap.GetGoogleEntitySiteMap("genre"));
            Response.Write(GoogleSiteMap.GetGoogleEntitySiteMap("vector"));

            Response.Write("</sitemapindex>");
		}

	}
}
