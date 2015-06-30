// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Globalization;
using System.Text;
using System.Web;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
    /// <summary>
    /// Summary description for importfinalize
    /// </summary>
    public partial class importfinalize : AdminPageBase
    {

        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");
            Server.ScriptTimeout = 1000000;
            SectionTitle = AppLogic.GetString("admin.sectiontitle.importfinalize", SkinID, LocaleSetting);
            RenderHtml();
        }

        private void RenderHtml()
        {
            StringBuilder writer = new StringBuilder();
            String action = CommonLogic.QueryStringCanBeDangerousContent("action").ToUpperInvariant();
            if (action == "ACCEPT")
            {
                DB.ExecuteLongTimeSQL("aspdnsf_ClearAllImportFlags", 1000);
                writer.Append("<p><b>" + AppLogic.GetString("admin.common.ImportAcceptedUC", SkinID, LocaleSetting) + "</b></p>");
            }
            if (action == "UNDO")
            {
                DB.ExecuteLongTimeSQL("aspdnsf_UndoImport", 1000);
                writer.Append("<p><b><font color=red>" + AppLogic.GetString("admin.common.ImportHasBeenUndone", SkinID, LocaleSetting) + "</font></b></p>");
            }
            ltContent.Text = writer.ToString();
        }
    }
}
