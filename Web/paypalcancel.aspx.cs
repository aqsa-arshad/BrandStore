// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront
{
	/// <summary>
	/// Summary description for paypalcancel.
	/// </summary>
	public partial class paypalcancel : SkinBase
	{
		protected void Page_Load(object sender, System.EventArgs e)
		{
			Response.CacheControl="private";
			Response.Expires=0;
			Response.AddHeader("pragma", "no-cache");
			
            String PM = AppLogic.CleanPaymentMethod(AppLogic.ro_PMPayPal);
            AppLogic.ValidatePM(PM); // this WILL throw a hard security exception on any problem!

            ThisCustomer.RequireCustomerRecord();
            if (!ThisCustomer.IsRegistered && !AppLogic.AppConfigBool("PasswordIsOptionalDuringCheckout"))
            {
                Response.Redirect("createaccount.aspx?checkout=true");
            }
			
			SectionTitle =  AppLogic.GetString("paypalcancel.aspx.1",SkinID,ThisCustomer.LocaleSetting);
			int OrderNumber = CommonLogic.QueryStringUSInt("OrderNumber");
			if (OrderNumber != 0 && AppLogic.AppConfigBool("PayPal.UseInstantNotification"))
			{
                if (!Order.OrderHasCleared(OrderNumber))
                {
                }
			}
            Label1.Text = AppLogic.GetString("paypalcancel.aspx.2", SkinID, ThisCustomer.LocaleSetting);
		}

	}
}
