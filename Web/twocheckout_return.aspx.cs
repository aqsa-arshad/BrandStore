// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways;

namespace AspDotNetStorefront
{
	/// <summary>
	/// Summary description for twocheckout_return.
	/// </summary>
	public partial class twocheckout_return : SkinBase
	{
		protected void Page_Load(object sender, System.EventArgs e)
		{
			if(CommonLogic.FormCanBeDangerousContent("x_2checked") == "Y" || CommonLogic.FormCanBeDangerousContent("x_2checked") == "K")
			{
				ShoppingCart cart = new ShoppingCart(1,ThisCustomer,CartTypeEnum.ShoppingCart,0,false);
				int OrderNumber = AppLogic.GetNextOrderNumber();

				Address UseBillingAddress = new Address();
				UseBillingAddress.LoadByCustomer(ThisCustomer.CustomerID,ThisCustomer.PrimaryBillingAddressID,AddressTypes.Billing);
                String status = Gateway.MakeOrder(String.Empty, AppLogic.TransactionMode(), cart, OrderNumber, String.Empty, String.Empty, String.Empty, String.Empty);

				Response.Redirect("orderconfirmation.aspx?ordernumber=" + OrderNumber.ToString() + "&paymentmethod=Credit+Card");
			}
			
			// error or not approved:
            ErrorMessage err = new ErrorMessage(Server.HtmlEncode(AppLogic.GetString("twocheckout_return.aspx.1", SkinID, ThisCustomer.LocaleSetting)));
			Response.Redirect("shoppingcart.aspx?errormsg=" + err.MessageId);
		}
	}
}
