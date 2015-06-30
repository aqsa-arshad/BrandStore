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
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for rssfeed.
    /// </summary>
    public partial class rssfeed : System.Web.UI.Page
    {
        private void Page_Load(object sender, System.EventArgs e)
        {
            String Channel = CommonLogic.QueryStringCanBeDangerousContent("Channel").ToLowerInvariant();

            if (Channel.Length == 0 || !CommonLogic.FileExists("XmlPackages/rss." + Channel + ".xml.config"))
            {
                Channel = "unknown";
            }

            Response.ContentEncoding = System.Text.Encoding.UTF8;
            Response.ContentType = "text/xml";
            Response.Write(AppLogic.RunXmlPackage("rss." + Channel + ".xml.config", null, null, 1, string.Empty, string.Empty, false, false));
        }
    }
}
