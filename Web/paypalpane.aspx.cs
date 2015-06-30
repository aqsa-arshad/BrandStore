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
    public partial class paypalpane : System.Web.UI.Page
    {
        ShoppingCart cart = null;
        Address BillingAddress = new Address();

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
                NetTotal = cart.Total(true);

                InitializePageContent();
            }
        }

        private void InitializePageContent()
        {
            Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;
            Address BillingAddress = new Address();



            BillingAddress.LoadByCustomer(ThisCustomer.CustomerID, ThisCustomer.PrimaryBillingAddressID, AddressTypes.Billing);

            if (cart.HasRecurringComponents())
                PayPalForm.Text = WriteSubscriptionPane(BillingAddress);
            else
                PayPalForm.Text = WritePayPalPane(BillingAddress);
        }

        private string WritePayPalPane(Address BillingAddress)
        {
            StringBuilder s = new StringBuilder("");

            Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;

            s.Append("<body onload='javascript:document.forms.PayPalForm.submit();' >");
            if (AppLogic.AppConfigBool("UseLiveTransactions"))
            {
                s.Append("<form target='_top' action='" + AppLogic.AppConfig("PayPal.LiveServer") + "' method='post' name='PayPalForm' id='PayPalForm' onsubmit='return (validateForm(this) && PayPalForm_Validator(this))'>\n");
            }
            else
            {
                s.Append("<form target='_top' action='" + AppLogic.AppConfig("PayPal.TestServer") + "' method='post' name='PayPalForm' id='PayPalForm' onsubmit='return (validateForm(this) && PayPalForm_Validator(this))'>\n");
            }
            s.Append("<input type='hidden' name='return' value='" + AppLogic.GetStoreHTTPLocation(true) + AppLogic.AppConfig("PayPal.ReturnOKURL") + "'>\n");
            s.Append("<input type='hidden' name='cancel_return' value='" + AppLogic.GetStoreHTTPLocation(true) + AppLogic.AppConfig("PayPal.ReturnCancelURL") + "'>\n");

            if (AppLogic.AppConfigBool("PayPal.UseInstantNotification"))
            {
                s.Append("<input type='hidden' name='notify_url' value='" + AppLogic.GetStoreHTTPLocation(true) + AppLogic.AppConfig("PayPal.NotificationURL") + "?CID=" + ThisCustomer.CustomerID.ToString() + "'>\n");
            }

            s.Append("<input type='hidden' name='cmd' value='_cart'>\n");
            s.Append("<input type='hidden' name='upload' value='1'>\n");
            s.Append("<input type='hidden' name='bn' value='" + AspDotNetStorefrontGateways.Processors.PayPal.BN + "'>\n");

            bool bEBay = AspDotNetStorefrontGateways.Processors.PayPalController.GetFirstAuctionSite(cart.CartItems).Equals("EBAY", StringComparison.InvariantCultureIgnoreCase);
            bool bAuthOnly = true;

            if (bEBay || AppLogic.TransactionModeIsAuthCapture() || AppLogic.AppConfigBool("PayPal.ForceCapture"))
            {
                bAuthOnly = false;
            }

            if (bAuthOnly)
            {
                s.Append("<input type='hidden' name='paymentaction' value='authorization'>\n");
            }

            s.Append("<input type='hidden' name='redirect_cmd' value='_xclick'>\n");
            s.Append("<input type='hidden' name='business' value='" + AppLogic.AppConfig("PayPal.BusinessID") + "'>\n");

            StringBuilder cartItemList = new StringBuilder("");

            try
            {
                StringBuilder innerBuilder = new StringBuilder();
                decimal SubTotalWODiscount = cart.SubTotal(false, false, true, true);
                decimal SubTotalWDiscount = cart.SubTotal(true, false, true, true);
                decimal dSavings = SubTotalWODiscount - SubTotalWDiscount;
                bool hasDiscountAmount = (NetTotal < cart.Total(false) || dSavings > 0.0M);

                PayPalItemList ppCart = new PayPalItemList(cart, false);

                for (int i = 0; i < ppCart.Count; i++)
                {
                    PayPalItem ppItem = ppCart.Item(i);

                    if (!String.IsNullOrEmpty(ppItem.Site))
                    {
                        innerBuilder.Append("<input type='hidden' name='site_" + (i + 1).ToString() + "' value='" + ppItem.Site + "'>\n");
                    }
                    if (!String.IsNullOrEmpty(ppItem.ItemNumber))
                    {
                        innerBuilder.Append("<input type='hidden' name='ai_" + (i + 1).ToString() + "' value='" + ppItem.ItemNumber + "'>\n");
                    }
                    if (!String.IsNullOrEmpty(ppItem.TransactionID))
                    {
                        innerBuilder.Append("<input type='hidden' name='at_" + (i + 1).ToString() + "' value='" + ppItem.TransactionID + "'>\n");
                    }
                    if (!String.IsNullOrEmpty(ppItem.BuyerID))
                    {
                        innerBuilder.Append("<input type='hidden' name='ab_" + (i + 1).ToString() + "' value='" + ppItem.BuyerID + "'>\n");
                    }

                    innerBuilder.Append("<input type='hidden' name='item_name_" + (i + 1).ToString() + "' value='" + ppItem.Name + "'>\n");

                    innerBuilder.Append("<input type='hidden' name='amount_" + (i + 1).ToString() + "' value='"
                        + Localization.CurrencyStringForGatewayWithoutExchangeRate(CommonLogic.IIF(hasDiscountAmount, 0.0M, ppItem.Amount)) + "'>\n");
                    innerBuilder.Append("<input type='hidden' name='quantity_" + (i + 1).ToString() + "' value='" + ppItem.Quantity.ToString() + "'>\n");
                }

                if (hasDiscountAmount)
                {
                    innerBuilder.Append("<input type='hidden' name='item_name_" + (ppCart.Count + 1).ToString() + "' value='" + AppLogic.AppConfig("StoreName") + " Purchase'>\n");
                    innerBuilder.Append("<input type='hidden' name='amount_" + (ppCart.Count + 1).ToString() + "' value='" + Localization.CurrencyStringForGatewayWithoutExchangeRate(NetTotal) + "'>\n");
                    innerBuilder.Append("<input type='hidden' name='quantity_" + (ppCart.Count + 1).ToString() + "' value='1'>\n");
                }
                else
                {
                    innerBuilder.Append("<input type='hidden' name='shipping_" + (ppCart.Count).ToString() + "' value='" + Localization.CurrencyStringForGatewayWithoutExchangeRate(ppCart.ShippingAmount) + "'>\n");

                    innerBuilder.Append("<input type='hidden' name='tax_cart' value='" + Localization.CurrencyStringForGatewayWithoutExchangeRate(ppCart.TaxAmount) + "'>\n");
                }
                cartItemList.Append(innerBuilder.ToString());
                innerBuilder = null;
            }
            catch
            {
                cartItemList.Append("<input type='hidden' name='item_name_1' value='" + AppLogic.AppConfig("StoreName") + " Purchase'>\n");
                cartItemList.Append("<input type='hidden' name='amount_1' value='" + Localization.CurrencyStringForGatewayWithoutExchangeRate(NetTotal) + "'>\n");
                cartItemList.Append("<input type='hidden' name='quantity_1' value='1'>\n");
            }

            s.Append(cartItemList.ToString());

            Address shipto = new Address();
            if (ThisCustomer.PrimaryShippingAddressID > 0)
            {
                shipto.LoadByCustomer(ThisCustomer.CustomerID, ThisCustomer.PrimaryShippingAddressID, AddressTypes.Shipping);
            }
            else
            {
                shipto = BillingAddress;
            }

            s.Append("<input type='hidden' name='rm' value='2'>\n");
            s.Append("<input type='hidden' name='no_note' value='1'>\n");
            s.Append("<input type='hidden' name='cs' value='1'>\n");
            s.Append("<input type='hidden' name='custom' value='" + ThisCustomer.CustomerID.ToString() + "'>\n");
            s.Append("<input type='hidden' name='currency_code' value='" + Localization.StoreCurrency() + "'>\n");
            s.Append("<input type='hidden' name='lc' value='" + AppLogic.AppConfig("PayPal.DefaultLocaleCode") + "'>\n");
            s.Append("<input type='hidden' name='country' value='" + AppLogic.GetCountryTwoLetterISOCode(shipto.Country) + "'>\n");

            if (!AppLogic.AppConfigBool("PayPal.RequireConfirmedAddress"))
            {
                s.Append("<input type='hidden' name='address_override' value='1'>\n");
                s.Append("<input type='hidden' name='first_name' value='" + shipto.FirstName + "'>\n");
                s.Append("<input type='hidden' name='last_name' value='" + shipto.LastName + "'>\n");
                s.Append("<input type='hidden' name='address1' value='" + shipto.Address1 + "'>\n");
                s.Append("<input type='hidden' name='address2' value='" + shipto.Address2 + "'>\n");
                s.Append("<input type='hidden' name='city' value='" + shipto.City + "'>\n");
                s.Append("<input type='hidden' name='state' value='" + shipto.State + "'>\n");
                s.Append("<input type='hidden' name='zip' value='" + shipto.Zip + "'>\n");
            }

            try
            {
                String ph = AppLogic.MakeProperPhoneFormat(shipto.Phone);
                Match m = Regex.Match(ph, @"^(?:(?:[\+]?(?<CountryCode>[\d]{1,3})(?:[ ]+|[\-.]))?[(]?(?<AreaCode>[\d]{3})[\-/)]?(?:[ ]+)?)?(?<Exchange>[a-zA-Z2-9]{3,})[ ]*[ \-.](?<Number>[a-zA-Z0-9]{4,})(?:(?:[ ]+|[xX]|(i:ext[\.]?)){1,2}(?<Ext>[\d]{1,5}))?$", RegexOptions.Compiled);
                string sCountry = m.Groups["ContryCode"].Value.Trim();
                string sArea = m.Groups["AreaCode"].Value.Trim();
                string sExchange = m.Groups["Exchange"].Value.Trim();
                string sNumber = m.Groups["Number"].Value.Trim();
                int cc = 0;
                if (sArea.Length > 0 && sExchange.Length > 0 && sNumber.Length > 0)
                {
                    if (sCountry.Length > 0)
                    {
                        cc = int.Parse(sCountry);
                    }
                    if (cc != 0)
                    {
                        s.Append("<input type='hidden' name='night_phone_a' value='" + sCountry + "'>\n");
                        s.Append("<input type='hidden' name='night_phone_b' value='" + sArea + sExchange + sNumber + "'>\n");
                        s.Append("<input type='hidden' name='day_phone_a' value='" + sCountry + "'>\n");
                        s.Append("<input type='hidden' name='day_phone_b' value='" + sArea + sExchange + sNumber + "'>\n");
                    }
                    else
                    {
                        s.Append("<input type='hidden' name='night_phone_a' value='" + sArea + "'>\n");
                        s.Append("<input type='hidden' name='night_phone_b' value='" + sExchange + "'>\n");
                        s.Append("<input type='hidden' name='night_phone_c' value='" + sNumber + "'>\n");
                        s.Append("<input type='hidden' name='day_phone_a' value='" + sArea + "'>\n");
                        s.Append("<input type='hidden' name='day_phone_b' value='" + sExchange + "'>\n");
                        s.Append("<input type='hidden' name='day_phone_c' value='" + sNumber + "'>\n");
                    }
                }
            }
            catch { }

            s.Append("</form>");
            s.Append("</body>");

            return s.ToString();
        }

        private string WriteSubscriptionPane(Address BillingAddress)
        {
            ErrorMessage err = new ErrorMessage(Server.HtmlEncode(AppLogic.GetString("shoppingcart.aspx.19", cart.ThisCustomer.SkinID, cart.ThisCustomer.LocaleSetting)));
            
            foreach (CartItem c in cart.CartItems)
            {
                if(!c.IsRecurring)
                    Response.Redirect("shoppingcart.aspx?errormsg=" + err.MessageId);
            }

            Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;  
            StringBuilder s = new StringBuilder();

            s.Append("<body onload='javascript:document.forms.PayPalForm.submit();' >");
            if (AppLogic.AppConfigBool("UseLiveTransactions"))
                s.Append("<form target='_top' action='" + AppLogic.AppConfig("PayPal.LiveServer") + "' method='post' name='PayPalForm' id='PayPalForm' onsubmit='return (validateForm(this) && PayPalForm_Validator(this))'>\n");
            else
                s.Append("<form target='_top' action='" + AppLogic.AppConfig("PayPal.TestServer") + "' method='post' name='PayPalForm' id='PayPalForm' onsubmit='return (validateForm(this) && PayPalForm_Validator(this))'>\n");

            s.Append("<input type='hidden' name='return' value='" + AppLogic.GetStoreHTTPLocation(true) + AppLogic.AppConfig("PayPal.ReturnOKURL") + "'>\n");
            s.Append("<input type='hidden' name='cancel_return' value='" + AppLogic.GetStoreHTTPLocation(true) + AppLogic.AppConfig("PayPal.ReturnCancelURL") + "'>\n");

            if (AppLogic.AppConfigBool("PayPal.UseInstantNotification"))
                s.Append("<input type='hidden' name='notify_url' value='" + AppLogic.GetStoreHTTPLocation(true) + AppLogic.AppConfig("PayPal.NotificationURL") + "?CID=" + ThisCustomer.CustomerID.ToString() + "'>\n");

            s.Append("<input type='hidden' name='cmd' value='_xclick-subscriptions'>\n");

            bool bEBay = AspDotNetStorefrontGateways.Processors.PayPalController.GetFirstAuctionSite(cart.CartItems).Equals("EBAY", StringComparison.InvariantCultureIgnoreCase);
            bool bAuthOnly = true;

            if (bEBay || AppLogic.TransactionModeIsAuthCapture() || AppLogic.AppConfigBool("PayPal.ForceCapture"))
                bAuthOnly = false;

            if (bAuthOnly)
                s.Append("<input type='hidden' name='paymentaction' value='authorization'>\n");

            s.Append("<input type='hidden' name='business' value='" + AppLogic.AppConfig("PayPal.BusinessID") + "'>\n");

            string description = string.Empty;
            int count = 0;
            foreach (CartItem c in cart.CartItems)
            {
                description += c.ProperProductName;
                count++;
                if(count != cart.CartItems.Count)
                    description += ", ";
            }

            s.Append("<input type='hidden' name='item_name' value='" + description + "'>\n");

            decimal recurringTotal = cart.Total(true);

            s.Append("<input type='hidden' name='a3' value='" + recurringTotal.ToString("C") + "'>\n");

            string recurringInterval = string.Empty;
            switch (cart.FirstCartItem.RecurringIntervalType)
            {
                case DateIntervalTypeEnum.Day:
                    recurringInterval = "D";
                    break;
                case DateIntervalTypeEnum.Weekly:
                case DateIntervalTypeEnum.Week:
                    recurringInterval = "W";
                    break;
                case DateIntervalTypeEnum.Month:
                case DateIntervalTypeEnum.Monthly:
                    recurringInterval = "M";
                    break;
                case DateIntervalTypeEnum.Year:
                case DateIntervalTypeEnum.Yearly:
                    recurringInterval = "Y";
                    break;
            }

            s.Append("<input type='hidden' name='p3' value='" + cart.FirstCartItem.RecurringInterval  + "'>\n");

            s.Append("<input type='hidden' name='t3' value='" + recurringInterval + "'>\n");

            s.Append("<input type='hidden' name='src' value='1'>\n");
            s.Append("<input type='hidden' name='srt' value='52'>\n");
            s.Append("<input type='hidden' name='sra' value='1'>\n");


            Address shipto = new Address();
            if (ThisCustomer.PrimaryShippingAddressID > 0)
                shipto.LoadByCustomer(ThisCustomer.CustomerID, ThisCustomer.PrimaryShippingAddressID, AddressTypes.Shipping);
            else
                shipto = BillingAddress;

            s.Append("<input type='hidden' name='rm' value='2'>\n");
            s.Append("<input type='hidden' name='no_note' value='1'>\n");
            s.Append("<input type='hidden' name='cs' value='1'>\n");
            s.Append("<input type='hidden' name='custom' value='" + ThisCustomer.CustomerID.ToString() + "'>\n");
            s.Append("<input type='hidden' name='currency_code' value='" + Localization.StoreCurrency() + "'>\n");
            s.Append("<input type='hidden' name='lc' value='" + AppLogic.AppConfig("PayPal.DefaultLocaleCode") + "'>\n");
            s.Append("<input type='hidden' name='country' value='" + AppLogic.GetCountryTwoLetterISOCode(shipto.Country) + "'>\n");

            if (!AppLogic.AppConfigBool("PayPal.RequireConfirmedAddress"))
            {
                s.Append("<input type='hidden' name='address_override' value='1'>\n");
                s.Append("<input type='hidden' name='first_name' value='" + shipto.FirstName + "'>\n");
                s.Append("<input type='hidden' name='last_name' value='" + shipto.LastName + "'>\n");
                s.Append("<input type='hidden' name='address1' value='" + shipto.Address1 + "'>\n");
                s.Append("<input type='hidden' name='address2' value='" + shipto.Address2 + "'>\n");
                s.Append("<input type='hidden' name='city' value='" + shipto.City + "'>\n");
                s.Append("<input type='hidden' name='state' value='" + shipto.State + "'>\n");
                s.Append("<input type='hidden' name='zip' value='" + shipto.Zip + "'>\n");
            }

            try
            {
                String ph = AppLogic.MakeProperPhoneFormat(shipto.Phone);
                Match m = Regex.Match(ph, @"^(?:(?:[\+]?(?<CountryCode>[\d]{1,3})(?:[ ]+|[\-.]))?[(]?(?<AreaCode>[\d]{3})[\-/)]?(?:[ ]+)?)?(?<Exchange>[a-zA-Z2-9]{3,})[ ]*[ \-.](?<Number>[a-zA-Z0-9]{4,})(?:(?:[ ]+|[xX]|(i:ext[\.]?)){1,2}(?<Ext>[\d]{1,5}))?$", RegexOptions.Compiled);
                string sCountry = m.Groups["ContryCode"].Value.Trim();
                string sArea = m.Groups["AreaCode"].Value.Trim();
                string sExchange = m.Groups["Exchange"].Value.Trim();
                string sNumber = m.Groups["Number"].Value.Trim();
                int cc = 0;
                if (sArea.Length > 0 && sExchange.Length > 0 && sNumber.Length > 0)
                {
                    if (sCountry.Length > 0)
                    {
                        cc = int.Parse(sCountry);
                    }
                    if (cc != 0)
                    {
                        s.Append("<input type='hidden' name='night_phone_a' value='" + sCountry + "'>\n");
                        s.Append("<input type='hidden' name='night_phone_b' value='" + sArea + sExchange + sNumber + "'>\n");
                        s.Append("<input type='hidden' name='day_phone_a' value='" + sCountry + "'>\n");
                        s.Append("<input type='hidden' name='day_phone_b' value='" + sArea + sExchange + sNumber + "'>\n");
                    }
                    else
                    {
                        s.Append("<input type='hidden' name='night_phone_a' value='" + sArea + "'>\n");
                        s.Append("<input type='hidden' name='night_phone_b' value='" + sExchange + "'>\n");
                        s.Append("<input type='hidden' name='night_phone_c' value='" + sNumber + "'>\n");
                        s.Append("<input type='hidden' name='day_phone_a' value='" + sArea + "'>\n");
                        s.Append("<input type='hidden' name='day_phone_b' value='" + sExchange + "'>\n");
                        s.Append("<input type='hidden' name='day_phone_c' value='" + sNumber + "'>\n");
                    }
                }
            }
            catch { }

            s.Append("</form>");
            s.Append("</body>");

            return s.ToString();
        }
    }
}
