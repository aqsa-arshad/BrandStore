// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
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
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	/// <summary>
	/// Summary description for cst_selectaddress_process.
	/// </summary>
	public partial class cst_selectaddress_process : System.Web.UI.Page
	{
		protected void Page_Load(object sender, System.EventArgs e)
		{
			Response.CacheControl="private";
			Response.Expires=0;
			Response.AddHeader("pragma", "no-cache");

            Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;

			int AddressID = CommonLogic.QueryStringUSInt("AddressID");
			int CustomerID = CommonLogic.QueryStringUSInt("CustomerID");
			int OriginalRecurringOrderNumber = CommonLogic.QueryStringUSInt("OriginalRecurringOrderNumber");
			string ReturnUrl = CommonLogic.QueryStringCanBeDangerousContent("ReturnUrl");
            // clean..
            ReturnUrl = AppLogic.ReturnURLDecode(ReturnUrl);

			String AddressTypeString = CommonLogic.QueryStringCanBeDangerousContent("AddressType");
      		AddressTypes AddressType = (AddressTypes)Enum.Parse(typeof(AddressTypes),AddressTypeString,true);

            bool AllowShipToDifferentThanBillTo = AppLogic.AppConfigBool("AllowShipToDifferentThanBillTo") && !AppLogic.AppConfigBool("SkipShippingOnCheckout");
            if (!AllowShipToDifferentThanBillTo) 
			{
				//Shipping and Billing address nust be the same so save both
				AddressType = AddressTypes.Billing | AddressTypes.Shipping;
			}

			Address thisAddress = new Address();

			if(AddressID != 0) //Users Selected an ID from the Address Grid
			{
				if (OriginalRecurringOrderNumber == 0)
				{
					thisAddress.LoadFromDB(AddressID);
                    thisAddress.MakeCustomersPrimaryAddress(AddressType);
				}
			}
			else  //Entered a new address to add
			{
				thisAddress.CustomerID = CustomerID;
                thisAddress.NickName = CommonLogic.FormCanBeDangerousContent("AddressNickName");
				thisAddress.FirstName = CommonLogic.FormCanBeDangerousContent("AddressFirstName");
				thisAddress.LastName = CommonLogic.FormCanBeDangerousContent("AddressLastName");
				thisAddress.Company = CommonLogic.FormCanBeDangerousContent("AddressCompany");
				thisAddress.Address1 = CommonLogic.FormCanBeDangerousContent("AddressAddress1");
				thisAddress.Address2 = CommonLogic.FormCanBeDangerousContent("AddressAddress2");
				thisAddress.Suite = CommonLogic.FormCanBeDangerousContent("AddressSuite");
				thisAddress.City = CommonLogic.FormCanBeDangerousContent("AddressCity");
				thisAddress.State = CommonLogic.FormCanBeDangerousContent("AddressState");
				thisAddress.Zip = CommonLogic.FormCanBeDangerousContent("AddressZip");
				thisAddress.Country = CommonLogic.FormCanBeDangerousContent("AddressCountry");
				thisAddress.Phone = CommonLogic.FormCanBeDangerousContent("AddressPhone");
       
				thisAddress.InsertDB();
				AddressID = thisAddress.AddressID;

				if (OriginalRecurringOrderNumber ==0)
				{
                    thisAddress.MakeCustomersPrimaryAddress(AddressType);
				}
			}
			if (OriginalRecurringOrderNumber != 0)
			{
				//put it in the ShoppingCart record
				string sql = String.Empty;
				if ((AddressType & AddressTypes.Billing) != 0)
				{
					sql = String.Format("BillingAddressID={0}",AddressID);
				}
				if ((AddressType & AddressTypes.Shipping) != 0)
				{
					if (sql.Length != 0)
					{
					sql += ",";
					}
					sql += String.Format("ShippingAddressID={0}",AddressID);
				}
				sql = String.Format("update ShoppingCart set " + sql + " where OriginalRecurringOrderNumber={0}",OriginalRecurringOrderNumber);
				DB.ExecuteSQL(sql);
			}

			Response.Redirect(ReturnUrl);
		}

	}
}
