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
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	/// <summary>
	/// Summary description for resetsqllog.
	/// </summary>
    public partial class resetsqllog : AdminPageBase
	{
		protected void Page_Load(object sender, System.EventArgs e)
		{
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");
            RenderHtml();
		}

		private void RenderHtml()
		{
            StringBuilder writer = new StringBuilder();
            writer.Append("<img src=\"" + AppLogic.LocateImageURL("~/App_Themes/Admin_Default/images/spacer.gif") + "\" border=\"0\" height=\"100\" width=\"1\"><br/>\n");
			if(ThisCustomer.IsAdminSuperUser)
			{
				DB.ExecuteSQL("truncate table SQLLog");
				writer.Append("<p align=\"center\"><font class=\"big\"><b>Done.</b></font></p>");
			}
			else
			{
				writer.Append("<p align=\"center\"><font class=\"big\" color=red><b>INSUFFICIENT PRIVILEGE</b></font></p>");
			}
            ltContent.Text = writer.ToString();
		}

	}
}
