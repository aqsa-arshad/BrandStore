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
    public partial class twocheckoutpane : System.Web.UI.Page
    {
        ShoppingCart cart = null;
        Address BillingAddress = new Address();

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
                NetTotal = cart.Total(true);

                InitializePageContent();
            }
        }

        private void InitializePageContent()
        {
            Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;
            Address BillingAddress = new Address();

            useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");

            BillingAddress.LoadByCustomer(ThisCustomer.CustomerID, ThisCustomer.PrimaryBillingAddressID, AddressTypes.Billing);
            TwoCheckoutForm.Text = WriteTwoCheckoutPane(BillingAddress);
        }

        private string WriteTwoCheckoutPane(Address BillingAddress)
        {
            StringBuilder s = new StringBuilder("");

            Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;

            //2checkout requires a leading 0 on totals < $1.00 otherwise it processes them as $0 orders.    
            System.Globalization.CultureInfo USCulture = new System.Globalization.CultureInfo("en-US");
            string formatedTotal = NetTotal.ToString("0.00", USCulture);           

            s.Append("<script type=\"text/javascript\">\n");
            s.Append("function TwoCheckoutForm_Validator(theForm)\n");
            s.Append("	{\n");
            s.Append("	submitenabled(theForm);\n");
            s.Append("	return (true);\n");
            s.Append("	}\n");
            s.Append("</script>\n");

            s.Append("<body onload=\"javascript:document.forms.TwoCheckoutForm.submit();\" >");
            s.Append("<form id=\"TwoCheckoutForm\" name=\"TwoCheckoutForm\" target=\"_top\" action=\"" + AppLogic.AppConfig("2CHECKOUT_Live_Server") + "\" method=\"post\" onsubmit=\"return (validateForm(this) && TwoCheckoutForm_Validator(this))\">\n");

            s.Append("<input type=\"hidden\" name=\"x_login\" value=\"" + AppLogic.AppConfig("2CHECKOUT_VendorID") + "\">\n");
            
            s.Append("<input type=\"hidden\" name=\"x_amount\" value=\"" + formatedTotal + "\">\n");
            s.Append("<input type=\"hidden\" name=\"x_invoice_num\" value=\"" + CommonLogic.GetNewGUID() + "\">\n");
            s.Append("<input type=\"hidden\" name=\"x_receipt_link_url\" value=\"" + AppLogic.GetStoreHTTPLocation(true) + "twocheckout_return.aspx\">\n");
            s.Append("<input type=\"hidden\" name=\"x_return_url\" value=\"" + AppLogic.GetStoreHTTPLocation(true) + "twocheckout_return.aspx\">\n");
            s.Append("<input type=\"hidden\" name=\"x_return\" value=\"" + AppLogic.GetStoreHTTPLocation(true) + "twocheckout_return.aspx\">\n");
            if (!useLiveTransactions)
            {
                s.Append("<input type=\"hidden\" name=\"demo\" value=\"Y\">\n");
            }
            s.Append("<input type=\"hidden\" name=\"x_First_Name\" value=\"" + BillingAddress.FirstName + "\">\n");
            s.Append("<input type=\"hidden\" name=\"x_Last_Name\" value=\"" + BillingAddress.LastName + "\">\n");
            s.Append("<input type=\"hidden\" name=\"x_Address\" value=\"" + BillingAddress.Address1 + "\">\n");
            s.Append("<input type=\"hidden\" name=\"x_City\" value=\"" + BillingAddress.City + "\">\n");
            s.Append("<input type=\"hidden\" name=\"x_State\" value=\"" + BillingAddress.State + "\">\n");
            s.Append("<input type=\"hidden\" name=\"x_Zip\" value=\"" + BillingAddress.Zip + "\">\n");
            s.Append("<input type=\"hidden\" name=\"x_Country\" value=\"" + BillingAddress.Country + "\">\n");
            s.Append("<input type=\"hidden\" name=\"x_EMail\" value=\"" + BillingAddress.EMail + "\">\n");
            s.Append("<input type=\"hidden\" name=\"x_Phone\" value=\"" + BillingAddress.Phone + "\">\n");
            s.Append("<input type=\"hidden\" name=\"city\" value=\"" + BillingAddress.City + "\">\n");
            s.Append("</form>\n");
            s.Append("</body>");

            return s.ToString();
        }
    }
}
