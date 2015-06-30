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
    /// Summary description for googleentity.
    /// </summary>
    public partial class googleentity : System.Web.UI.Page
    {

        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.ContentType = "text/xml";
            Response.ContentEncoding = new System.Text.UTF8Encoding();
            Response.Write("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");

            String EntityName = CommonLogic.QueryStringCanBeDangerousContent("EntityName");
            AppLogic.CheckForScriptTag(EntityName);
            int EntityID = CommonLogic.QueryStringUSInt("EntityID");

            EntityHelper eHlp = AppLogic.LookupHelper(EntityName, 0);

            Response.Write("<urlset xmlns=\"" + AppLogic.AppConfig("GoogleSiteMap.Xmlns") + "\">\n");

            Response.Write("<url>");
            Response.Write("<loc>" + XmlCommon.XmlEncode(AppLogic.GetStoreHTTPLocation(false, false) + SE.MakeEntityLink(EntityName, EntityID, String.Empty)) + "</loc> ");
            Response.Write("<changefreq>" + AppLogic.AppConfig("GoogleSiteMap.EntityChangeFreq") + "</changefreq> ");
            Response.Write("<priority>" + AppLogic.AppConfig("GoogleSiteMap.EntityPriority") + "</priority> ");
            Response.Write("</url>\n");

            Response.Write(GoogleSiteMap.GetGoogleEntityProductURLNodes(EntityName, EntityID));

            Response.Write("</urlset>");

        }

    }
}
