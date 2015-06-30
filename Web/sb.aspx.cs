// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------

//===========================================================================
// This file was modified as part of an ASP.NET 2.0 Web project conversion.
// The class name was changed and the class modified to inherit from the abstract base class 
// in file 'App_Code\Migrated\Stub_sb_aspx_cs.cs'.
// During runtime, this allows other classes in your web application to bind and access 
// the code-behind page using the abstract base class.
// The associated content page 'sb.aspx' was also modified to refer to the new class name.
// For more information on this code pattern, please refer to http://go.microsoft.com/fwlink/?LinkId=46995 
//===========================================================================
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
using System.Globalization;
using AspDotNetStorefrontCore;


namespace AspDotNetStorefront
{
	/// <summary>
	/// Summary description for sb.
	/// </summary>
	public partial class sb : System.Web.UI.Page
	{
		protected void Page_Load(object sender, System.EventArgs e)
		{
			Response.CacheControl="private";
			Response.Expires=0;
			Response.AddHeader("pragma", "no-cache");

            Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;
			EntityHelper CategoryHelper = AppLogic.CategoryStoreEntityHelper[0];

            int skinID = int.Parse(CommonLogic.IIF(CommonLogic.IsInteger(Profile.SkinID.ToString()), Profile.SkinID.ToString(), "0")); 
            if (skinID <= 0)
            {
                skinID = ThisCustomer.SkinID;
            }

			Response.Write("<html>\n");
			Response.Write("<head>\n");
			Response.Write("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">\n");
			Response.Write("<title>Product Browser</title>\n");

            Response.Write("<link rel=\"stylesheet\" href=\"App_Themes/Skin_" + skinID.ToString() + "/style.css\" type=\"text/css\">\n");
			Response.Write("<script type=\"text/javascript\" src=\"jscripts/formValidate.js\"></script>\n");
			Response.Write("</head>\n");
            Response.Write("<body class=\"ProductBrowserBody\" bottommargin=\"0\" leftmargin=\"0\" marginheight=\"0\" marginwidth=\"0\" rightmargin=\"0\" topmargin=\"0\" bgcolor=\"#FFFFFF\">\n");
			Response.Write("<!-- PAGE INVOCATION: '%INVOCATION%' -->\n");

            Response.Write(AppLogic.RunXmlPackage("stylebrowser.xml.config", null, ThisCustomer, 1, String.Empty, String.Empty, false, false));

			Response.Write("</body>\n");
			Response.Write("</html>\n");
		}


	}
}
