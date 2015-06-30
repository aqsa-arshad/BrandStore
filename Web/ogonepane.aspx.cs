// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways;
using AspDotNetStorefrontGateways.Processors;
using AspDotNetStorefrontCommon;

namespace AspDotNetStorefront
{
    public partial class ogonepane : System.Web.UI.Page
    {
        ShoppingCart cart = null;
        Address BillingAddress = new Address();

        decimal CartTotal = Decimal.Zero;
        decimal NetTotal = Decimal.Zero;


        protected void Page_Load(object sender, EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;

            cart = new ShoppingCart(1, ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);

            if (cart.CartItems.Count == 0)
            {
                Response.Redirect("shoppingcart.aspx");
            }

            if (!cart.ShippingIsAllValid())
            {
                ErrorMessage err = new ErrorMessage(Server.HtmlEncode(AppLogic.GetString("shoppingcart.cs.95", ThisCustomer.SkinID, ThisCustomer.LocaleSetting)));
                HttpContext.Current.Response.Redirect("shoppingcart.aspx?resetlinkback=1&errormsg=" + err.MessageId);
            }

            if (!this.IsPostBack)
            {
                CartTotal = cart.Total(true);
                NetTotal = CartTotal - CommonLogic.IIF(cart.Coupon.CouponType == CouponTypeEnum.GiftCard, CommonLogic.IIF(CartTotal < cart.Coupon.DiscountAmount, CartTotal, cart.Coupon.DiscountAmount), 0);

                InitializePageContent();
            }
        }

        private void InitializePageContent()
        {
            Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;
            Address BillingAddress = new Address();

            BillingAddress.LoadByCustomer(ThisCustomer.CustomerID, ThisCustomer.PrimaryBillingAddressID, AddressTypes.Billing);
            OgoneForm.Text = WriteOgonePane(BillingAddress);
        }

        private string WriteOgonePane(Address BillingAddress)
        {
            StringBuilder s = new StringBuilder("");

            Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;

            String OgoneOrderID = ThisCustomer.CustomerID + "-" + Localization.ToDBDateTimeString(DateTime.Now); // Max length 30 chars, we don't know what the order number will be...
            String OgoneAmount = Localization.CurrencyStringForGatewayWithoutExchangeRate((NetTotal)).Replace(".", "");
            String OgoneSignatureSeed = OgoneOrderID + OgoneAmount + Localization.StoreCurrency() + AppLogic.AppConfig("Ogone.PSPID");

            s.Append("<script type=\"text/javascript\">\n");
            s.Append("function OgoneForm_Validator(theForm)\n");
            s.Append("	{\n");
            s.Append("	submitenabled(theForm);\n");
            s.Append("	return (true);\n");
            s.Append("	}\n");
            s.Append("</script>\n");
            s.Append("<body onload=\"javascript:document.forms.OgoneForm.submit();\" >");
            s.Append("<form id=\"OgoneForm\" name=\"OgoneForm\" target=\"_top\" action=\"" + CommonLogic.IIF(AppLogic.AppConfigBool("UseLiveTransactions"), AppLogic.AppConfig("Ogone.LivePostURL"), AppLogic.AppConfig("Ogone.TestPostURL")) + "\" method=\"post\" onsubmit=\"return (validateForm(this) && OgoneForm_Validator(this))\">\n");
            s.Append("<input type=\"hidden\" name=\"PSPID\" value=\"" + AppLogic.AppConfig("Ogone.PSPID") + "\">\n");
            s.Append("<input type=\"hidden\" name=\"amount\" value=\"" + OgoneAmount + "\">\n");
            s.Append("<input type=\"hidden\" name=\"orderID\" value=\"" + OgoneOrderID + "\">\n");
            s.Append("<input type=\"hidden\" name=\"CN\" value=\"" + BillingAddress.FirstName + " " + BillingAddress.LastName + "\">\n");
            s.Append("<input type=\"hidden\" name=\"owneraddress\" value=\"" + BillingAddress.Address1 + "\">\n");
            s.Append("<input type=\"hidden\" name=\"ownertown\" value=\"" + BillingAddress.City + "\">\n");
            s.Append("<input type=\"hidden\" name=\"ownerZIP\" value=\"" + BillingAddress.Zip + "\">\n");
            s.Append("<input type=\"hidden\" name=\"ownercty\" value=\"" + AppLogic.GetCountryTwoLetterISOCode(BillingAddress.Country) + "\">\n");
            s.Append("<input type=\"hidden\" name=\"EMAIL\" value=\"" + BillingAddress.EMail + "\">\n");
            s.Append("<input type=\"hidden\" name=\"ownertelno\" value=\"" + BillingAddress.Phone + "\">\n");
            s.Append("<input type=\"hidden\" name=\"currency\" value=\"" + Localization.StoreCurrency() + "\">\n");
            s.Append("<input type=\"hidden\" name=\"language\" value=\"" + ThisCustomer.LocaleSetting.Replace("-", "_") + "\">\n");
            s.Append("<input type=\"hidden\" name=\"SHASign\" value=\"" + Ogone.Signature(OgoneSignatureSeed) + "\">\n");
            s.Append("<input type=\"hidden\" name=\"accepturl\" value=\"" + AppLogic.GetStoreHTTPLocation(true) + "ogone_postsale.aspx\">\n");
            s.Append("<input type=\"hidden\" name=\"declineurl\" value=\"" + AppLogic.GetStoreHTTPLocation(true) + "ogone_postsale.aspx\">\n");
            s.Append("<input type=\"hidden\" name=\"exceptionurl\" value=\"" + AppLogic.GetStoreHTTPLocation(true) + "ogone_postsale.aspx\">\n");
            s.Append("<input type=\"hidden\" name=\"cancelurl\" value=\"" + AppLogic.GetStoreHTTPLocation(true) + "ogone_postsale.aspx\">\n");
            s.Append("</form>\n");
            s.Append("</body>");

            return s.ToString();
        }
    }
}
