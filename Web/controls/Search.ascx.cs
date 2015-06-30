// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

public partial class controls_Search : System.Web.UI.UserControl
{
	protected void Page_Load(object sender, EventArgs e)
	{
		SearchBox.Attributes.Add("onclick", "this.value = ''");

		string queryStringSearchTerm = CommonLogic.QueryStringCanBeDangerousContent("searchterm");
		if (!IsPostBack)
		{
			if (queryStringSearchTerm.Length > 0)
				SearchBox.Text = queryStringSearchTerm;
		}
		
	}

	protected void SearchButton_Click(object sender, EventArgs e)
	{
		Response.Redirect(String.Format("~/search.aspx?searchterm={0}", Server.UrlEncode(SearchBox.Text)));
	}

}
