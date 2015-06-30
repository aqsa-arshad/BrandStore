// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------


public partial class Moneybookers_postsale : System.Web.UI.Page
{
	protected void Page_Load(object sender, System.EventArgs e)
	{
		Response.CacheControl = "private";
		Response.Expires = 0;
		Response.AddHeader("pragma", "no-cache");

		ErrorMsg.Text = "No longer supported.";
	}
}
