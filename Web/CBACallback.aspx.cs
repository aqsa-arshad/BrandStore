// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
#region Using Directives

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Xml.Linq;
using AspDotNetStorefrontCore;
using System.Text;
using System.Security.Cryptography;
using System.Net;
using System.IO;

#endregion

namespace AspDotNetStorefront
{
	public partial class CBACallback : Page
	{
		#region Event Handlers

		protected void Page_Load(object sender, System.EventArgs e)
		{
			Response.Clear();
			Response.BufferOutput = true;

			GatewayCheckoutByAmazon.CheckoutByAmazon checkoutByAmazon = new GatewayCheckoutByAmazon.CheckoutByAmazon();
			checkoutByAmazon.HandleCallback();

			Response.StatusCode = 200;
			Response.Flush();
		}

		#endregion
	}
}
