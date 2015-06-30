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
using AspDotNetStorefrontGateways.Processors;

namespace AspDotNetStorefront
{
	/// <summary>
    /// Summary description for ogone_return.
	/// </summary>
    public partial class ogone_postsale : System.Web.UI.Page
	{
		protected void Page_Load(object sender, System.EventArgs e)
		{
            // This page gets requested by the Ogone payment server, not the client's browser

            if (AppLogic.ActivePaymentGatewayCleaned().ToLower() == "ogone")
            {
                // Check SHASIGN before proceeding
                String OgoneSignatureSeed = CommonLogic.FormCanBeDangerousContent("orderID") + CommonLogic.FormCanBeDangerousContent("currency");
                OgoneSignatureSeed += CommonLogic.FormCanBeDangerousContent("amount") + CommonLogic.FormCanBeDangerousContent("PM");
                OgoneSignatureSeed += CommonLogic.FormCanBeDangerousContent("ACCEPTANCE") + CommonLogic.FormCanBeDangerousContent("STATUS");
                OgoneSignatureSeed += CommonLogic.FormCanBeDangerousContent("CARDNO") + CommonLogic.FormCanBeDangerousContent("PAYID");
                OgoneSignatureSeed += CommonLogic.FormCanBeDangerousContent("NCERROR") + CommonLogic.FormCanBeDangerousContent("BRAND");
                if (CommonLogic.FormCanBeDangerousContent("SHASIGN") == Ogone.Signature(OgoneSignatureSeed))
                {
                    // Valid Ogone request
                    if (CommonLogic.FormCanBeDangerousContent("STATUS").Substring(0, 1) == "5" || CommonLogic.FormCanBeDangerousContent("STATUS").Substring(0, 1) == "9")
                    {
                        String sCustomer = CommonLogic.FormCanBeDangerousContent("orderID").Split(new char[] { '-' }, 2, StringSplitOptions.None).GetValue(0).ToString();
                        int OgoneCustomerID = Localization.ParseNativeInt(sCustomer);
                        Customer OgoneCustomer = new Customer(OgoneCustomerID);
                        ShoppingCart cart = new ShoppingCart(1, OgoneCustomer, CartTypeEnum.ShoppingCart, 0, false);
                        int OrderNumber = AppLogic.GetNextOrderNumber();
                        String TransactionID = CommonLogic.FormCanBeDangerousContent("PAYID");
                        Address UseBillingAddress = new Address();
                        UseBillingAddress.LoadByCustomer(OgoneCustomer.CustomerID, OgoneCustomer.PrimaryBillingAddressID, AddressTypes.Billing);
                        String status = Gateway.MakeOrder(String.Empty, AppLogic.TransactionMode(), cart, OrderNumber, String.Empty, String.Empty, TransactionID, String.Empty);

                        String AVSResult = CommonLogic.FormCanBeDangerousContent("AAVCheck");
                        String CVCResult = CommonLogic.FormCanBeDangerousContent("CVCCheck");
                        if (CVCResult.Length > 0)
                        {
                            if (AVSResult.Length != 0)
                            {
                                AVSResult += ", ";
                            }
                            AVSResult += "CV Result: " + CVCResult;
                        }
                        String CardNo = CommonLogic.FormCanBeDangerousContent("CARDNO");
                        String Last4 = CardNo.Substring(CardNo.Length - 4, 4);
                        String sql = String.Format("update Orders set AVSResult={0}, AuthorizationCode={1}, Last4={2} where OrderNumber={3}",
                            DB.SQuote(AVSResult), DB.SQuote(CommonLogic.FormCanBeDangerousContent("ACCEPTANCE")), DB.SQuote(Last4), OrderNumber.ToString());
                        DB.ExecuteSQL(sql);
                        Response.Redirect(AppLogic.GetStoreHTTPLocation(true) + "orderconfirmation.aspx?ordernumber=" + OrderNumber.ToString() + "&paymentmethod=Credit+Card");
                    }
                }
            }
            // if it was not a successful order then we will display a message to the customer
            Response.Redirect(AppLogic.GetStoreHTTPLocation(true) + "ogone_return.aspx");
		}
	}
}
