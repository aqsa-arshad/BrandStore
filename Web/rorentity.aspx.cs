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
    /// Summary description for rorentity.
    /// </summary>
    public partial class rorentity : System.Web.UI.Page
    {

        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.ContentType = "text/xml";
            Response.ContentEncoding = new System.Text.UTF8Encoding();
            Response.Write("<?xml version='1.0' encoding='utf-8'?>");
            Response.Write("<rss version='2.0' xmlns:ror='http://rorweb.com/0.1/'>");
            Response.Write("<channel>");
            Response.Write("<title>Products</title>");

            String EntityName = CommonLogic.QueryStringCanBeDangerousContent("EntityName");
            AppLogic.CheckForScriptTag(EntityName);
            int EntityID = CommonLogic.QueryStringUSInt("EntityID");

            EntityHelper eHlp = AppLogic.LookupHelper(EntityName, 0);

            Response.Write("<item>");
            Response.Write("<ror:type>Products</ror:type>");
            Response.Write("<link>" + XmlCommon.XmlEncode(AppLogic.GetStoreHTTPLocation(false) + SE.MakeEntityLink(EntityName, EntityID, String.Empty)) + "</link> ");
            Response.Write("</item>");

            Response.Write(eHlp.GetEntityRorObjectList(EntityID, Localization.GetDefaultLocale(), 0, 0));

            Response.Write("</channel>");
            Response.Write("</rss>");

        }

    }
}
