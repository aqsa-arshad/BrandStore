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
using AspDotNetStorefrontCommon;

namespace AspDotNetStorefront
{
    public partial class worldpaypane : System.Web.UI.Page
    {
        ShoppingCart cart = null;
        Address BillingAddress = new Address();

        decimal CartTotal = Decimal.Zero;
        decimal NetTotal = Decimal.Zero;

        bool useLiveTransactions = false;

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

            useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");

            BillingAddress.LoadByCustomer(ThisCustomer.CustomerID, ThisCustomer.PrimaryBillingAddressID, AddressTypes.Billing);
            WorldPayForm.Text = WriteWorldPayPane(BillingAddress);
        }

        private string WriteWorldPayPane(Address BillingAddress)
        {
            StringBuilder s = new StringBuilder("");

            Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;

            s.Append("<script type=\"text/javascript\">\n");
            s.Append("function WorldPayForm_Validator(theForm)\n");
            s.Append("	{\n");
            s.Append("	submitenabled(theForm);\n");

            s.Append("	return (true);\n");
            s.Append("	}\n");
            s.Append("</script>\n");

            s.Append("<body onload=\"javascript:document.forms.WorldPayForm.submit();\" >");
            s.Append("<form action=\"" + AppLogic.AppConfig("WorldPay_Live_Server") + "\" method=\"post\" name=\"WorldPayForm\" id=\"WorldPayForm\" onsubmit=\"return (validateForm(this) && WorldPayForm_Validator(this))\">\n");

            s.Append("<input type=\"hidden\" name=\"instId\" value=\"" + AppLogic.AppConfig("WorldPay_InstallationID") + "\">\n");
            s.Append("<input type=\"hidden\" name=\"cartId\" value=\"" + cart.ThisCustomer.CustomerID.ToString() + "\">\n");
            s.Append("<input type=\"hidden\" name=\"amount\" value=\"" + Localization.CurrencyStringForGatewayWithoutExchangeRate((NetTotal)) + "\">\n");
            s.Append("<input type=\"hidden\" name=\"currency\" value=\"" + Localization.StoreCurrency() + "\">\n");
            s.Append("<input type=\"hidden\" name=\"des\" value=\"" + AppLogic.AppConfig("StoreName") + " Purchase\">\n");
            s.Append("<input type=\"hidden\" name=\"description\" value=\"" + AppLogic.AppConfig("StoreName") + " Purchase\">\n");
            s.Append("<input type=\"hidden\" name=\"MC_callback\" value=\"" + AppLogic.GetStoreHTTPLocation(true) + AppLogic.AppConfig("WorldPay_ReturnURL") + "\">\n");
            s.Append("<input type=\"hidden\" name=\"authMode\" value=\"" + CommonLogic.IIF(AppLogic.TransactionModeIsAuthOnly(), "E", "A") + "\">\n");

            s.Append("<input type=\"hidden\" name=\"name\" value=\"" + (BillingAddress.FirstName + " " + BillingAddress.LastName) + "\">\n");
            s.Append("<input type=\"hidden\" name=\"address\" value=\"" + BillingAddress.Address1 + "\">\n");
            s.Append("<input type=\"hidden\" name=\"postcode\" value=\"" + BillingAddress.Zip + "\">\n");
            s.Append("<input type=\"hidden\" name=\"country\" value=\"" + AppLogic.GetCountryTwoLetterISOCode(BillingAddress.Country) + "\">\n");
            s.Append("<input type=\"hidden\" name=\"tel\" value=\"" + BillingAddress.Phone + "\">\n");
            s.Append("<input type=\"hidden\" name=\"email\" value=\"" + CommonLogic.IIF(string.IsNullOrEmpty(ThisCustomer.EMail), BillingAddress.EMail, ThisCustomer.EMail) + "\">\n");
            s.Append("<input type=\"hidden\" name=\"lang\" value=\"" + AppLogic.AppConfig("WorldPay_LanguageLocale") + "\">\n");

            if (AppLogic.AppConfigBool("WorldPay_FixContact"))
            {
                s.Append("<input type=\"hidden\" name=\"fixContact\" value=\"true\">\n");
            }

            if (AppLogic.AppConfigBool("WorldPay_HideContact"))
            {
                s.Append("<input type=\"hidden\" name=\"hideContact\" value=\"true\">\n");
            }

            if (AppLogic.AppConfigBool("WorldPay_TestMode"))
            {
                s.Append("<input type=\"hidden\" name=\"testMode\" value=\"" + AppLogic.AppConfig("WorldPay_TestModeCode") + "\">\n");
            }

            s.Append("</form>\n");
            s.Append("</body>");

            return s.ToString();
        }
    }
}
