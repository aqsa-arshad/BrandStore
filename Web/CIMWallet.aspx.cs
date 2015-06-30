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
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore.ShippingCalculation;

namespace AspDotNetStorefront
{
	public partial class CIMWallet : SkinBase
	{
		/// <summary>
		/// Sets that this page requires a scriptmanager
		/// </summary>
		public override bool RequireScriptManager
		{
			get
			{
				return true;
			}
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			SectionTitle = "Wallet";

			RequireSecurePage();
			ThisCustomer.RequireCustomerRecord();

		}
	}
}
