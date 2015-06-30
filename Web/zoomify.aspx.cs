// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Data;
using System.Globalization;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for popup.
    /// </summary>
    public partial class zoomify : System.Web.UI.Page
    {
        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            String PageTitle = CommonLogic.QueryStringCanBeDangerousContent("Title");
            AppLogic.CheckForScriptTag(PageTitle);
            if (PageTitle.Length == 0)
            {
                PageTitle = "Popup Window " + CommonLogic.GetRandomNumber(1, 1000000).ToString();
            }
            Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;

            Response.Write("<html>\n");
            Response.Write("<head>\n");
            Response.Write("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">\n");
            Response.Write("<title>" + PageTitle + "</title>\n");
            Response.Write("<link rel=\"stylesheet\" href=\"~/App_Themes/Skin_" + ThisCustomer.SkinID.ToString() + "/style.css\" type=\"text/css\">\n");
            Response.Write("<script type=\"text/javascript\" src=\"jscripts/formValidate.js\"></script>\n");
            Response.Write("</head>\n");


            if (CommonLogic.QueryStringCanBeDangerousContent("src").Length != 0)
            {
                string AlternateImgSrc = string.Empty;
                if (CommonLogic.QueryStringCanBeDangerousContent("altsrc").Length != 0)
                {
                    AlternateImgSrc = "&AltSrc=" + CommonLogic.QueryStringCanBeDangerousContent("altsrc");
                }
                Response.Write(AppLogic.RunXmlPackage("Zoomify.Large", null, ThisCustomer, ThisCustomer.SkinID, "", "ImagePath=" + CommonLogic.QueryStringCanBeDangerousContent("src") + AlternateImgSrc, false, false));
                Response.Write("\n");
                Response.Write("<a href=\"javascript:self.close();\">" + AppLogic.GetString("popup.aspx.1", 1, Localization.GetDefaultLocale()) + "</a>\n");
            }
            else
            {
                // empty page message
                Response.Write("<img src=\"images/spacer.gif\" border=\"0\" height=\"100\" width=\"1\">\n");
                Response.Write("<p align=\"center\"><font class=\"big\"><b>" + AppLogic.GetString("popup.aspx.5", 1, Localization.GetDefaultLocale()) + "</b></font></p>");
            }
            Response.Write("</body>\n");
            Response.Write("</html>\n");

        }

    }
}
