// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.IO;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	/// <summary>
	/// Summary description for deleteimage.
	/// </summary>
	public partial class deleteimage : System.Web.UI.Page
	{

		protected void Page_Load(object sender, System.EventArgs e)
		{
			Response.CacheControl="private";
			Response.Expires=0;
			Response.AddHeader("pragma", "no-cache");

            Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;
			String FormImageName = CommonLogic.QueryStringCanBeDangerousContent("FormImageName");
			String ImgUrl = CommonLogic.QueryStringCanBeDangerousContent("ImgUrl");

			if(ThisCustomer.IsAdminUser)
			{
				Response.Write("<html><head><title>Delete Image</title></head><body>\n");
				System.IO.File.Delete(CommonLogic.SafeMapPath("~/" + ImgUrl.Substring(ImgUrl.IndexOf("images"))));
				Response.Write("<script type=\"text/javascript\">\n");
				Response.Write("opener.document.getElementById('" + FormImageName + "').src = '../images/spacer.gif';\n");
				Response.Write("self.close();\n");
				Response.Write("</script>\n");
			}
			else
			{
				Response.Write("<script type=\"text/javascript\">\n");
				Response.Write("self.close();\n");
				Response.Write("</script>\n");
			}

			Response.Write("</body></html>\n");
		}

	}
}
