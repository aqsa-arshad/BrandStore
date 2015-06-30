// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Web.UI.WebControls;
using System.Data;
using System.Text;
using System.Globalization;
using AspDotNetStorefrontCore;
using System.Collections.Generic;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for checkoutshippingmult2.
    /// </summary>
    public partial class checkoutshippingmult2 : SkinBase
    {

        ShoppingCart cart = null;

        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = -1;
            Response.AddHeader("pragma", "no-cache");
            ErrorMessage err;
            if (AppLogic.AppConfigBool("RequireOver13Checked") && !ThisCustomer.IsOver13)
            {
                err = new ErrorMessage(Server.HtmlEncode(AppLogic.GetString("checkout.over13required", ThisCustomer.SkinID, ThisCustomer.LocaleSetting)));
                Response.Redirect("shoppingcart.aspx?errormsg=" + err.MessageId);
            }

            RequireSecurePage();

            // -----------------------------------------------------------------------------------------------
            // NOTE ON PAGE LOAD LOGIC:
            // We are checking here for required elements to allowing the customer to stay on this page.
            // Many of these checks may be redundant, and they DO add a bit of overhead in terms of db calls, but ANYTHING really
            // could have changed since the customer was on the last page. Remember, the web is completely stateless. Assume this
            // page was executed by ANYONE at ANYTIME (even someone trying to break the cart). 
            // It could have been yesterday, or 1 second ago, and other customers could have purchased limitied inventory products, 
            // coupons may no longer be valid, etc, etc, etc...
            // -----------------------------------------------------------------------------------------------
            ThisCustomer.RequireCustomerRecord();

            if (!ThisCustomer.IsRegistered)
            {
                bool boolAllowAnon = (AppLogic.AppConfigBool("PasswordIsOptionalDuringCheckout") && !cart.HasRecurringComponents());
                if (!boolAllowAnon && ThisCustomer.PrimaryBillingAddressID > 0)
                {
                    Address BillingAddress = new Address();
                    BillingAddress.LoadByCustomer(ThisCustomer.CustomerID, ThisCustomer.PrimaryBillingAddressID, AddressTypes.Billing);
                    if (BillingAddress.PaymentMethodLastUsed == AppLogic.ro_PMPayPalExpress || BillingAddress.PaymentMethodLastUsed == AppLogic.ro_PMPayPalExpressMark)
                    {
                        boolAllowAnon = AppLogic.AppConfigBool("PayPal.Express.AllowAnonCheckout");
                    }
                }

                if (!boolAllowAnon)
                {
                    Response.Redirect("createaccount.aspx?checkout=true");
                }
            }
            if (ThisCustomer.PrimaryBillingAddressID == 0 || ThisCustomer.PrimaryShippingAddressID == 0)
            {
                err = new ErrorMessage(Server.HtmlEncode(AppLogic.GetString("checkoutpayment.aspx.2", ThisCustomer.SkinID, ThisCustomer.LocaleSetting)));
                Response.Redirect("shoppingcart.aspx?resetlinkback=1&errormsg=" + err.MessageId);
            }

            SectionTitle = AppLogic.GetString("checkoutshippingmult2.aspx.1", SkinID, ThisCustomer.LocaleSetting);
            cart = new ShoppingCart(SkinID, ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);

            cart.ValidProceedCheckout(); // will not come back from this if any issue. they are sent back to the cart page!

            if (cart.IsAllDownloadComponents() || !Shipping.MultiShipEnabled() || cart.TotalQuantity() > AppLogic.MultiShipMaxNumItemsAllowed() || !cart.CartAllowsShippingMethodSelection)
            {
                // not allowed here:
                err = new ErrorMessage(Server.HtmlEncode(AppLogic.GetString("checkoutshippingmult2.aspx.12", ThisCustomer.SkinID, ThisCustomer.LocaleSetting)));
                Response.Redirect("shoppingcart.aspx?resetlinkback=1&errormsg=" + err.MessageId);
            }

            CartItem FirstCartItem = (CartItem)cart.CartItems[0];
            Address FirstItemShippingAddress = new Address();
            FirstItemShippingAddress.LoadByCustomer(ThisCustomer.CustomerID, FirstCartItem.ShippingAddressID, AddressTypes.Shipping);
            if (FirstItemShippingAddress.AddressID == 0)
            {
                // not allowed here anymore!
                err = new ErrorMessage(Server.HtmlEncode(AppLogic.GetString("checkoutshippingmult2.aspx.10", ThisCustomer.SkinID, ThisCustomer.LocaleSetting)));
                Response.Redirect("shoppingcart.aspx?errormsg=" + err.MessageId);
            }


            if (!IsPostBack && CommonLogic.FormCanBeDangerousContent("update") == "" && CommonLogic.FormCanBeDangerousContent("continue") == "")
            {
                UpdatepageContent();
            }

            if (CommonLogic.FormCanBeDangerousContent("update") != "" || CommonLogic.FormCanBeDangerousContent("continue") != "")
            {
                ProcessCart();
            }
            JSPopupRoutines.Text = AppLogic.GetJSPopupRoutines();
        }

        private void UpdatepageContent()
        {
            if (CommonLogic.QueryStringNativeInt("ErrorMsg") > 0)
            {
                ErrorMessage e = new ErrorMessage(CommonLogic.QueryStringNativeInt("ErrorMsg"));
                ErrorMsgLabel.Text = "<div class='error-wrap'><span class=\"error-large\">" + Server.HtmlEncode(e.Message) + "</span></div>";
                pnlErrorMsg.Visible = true;
            }
            else
            {
                pnlErrorMsg.Visible = false;
            }

            //write out header package is it exists
            String XmlPackageName = AppLogic.AppConfig("XmlPackage.CheckoutShippingMult2PageHeader");
            if (XmlPackageName.Length != 0)
            {
                XmlPackage_CheckoutShippingMult2PageHeader.Text = AppLogic.RunXmlPackage(XmlPackageName, base.GetParser, ThisCustomer, SkinID, String.Empty, String.Empty, true, true);
            }


            if (!cart.ShippingIsFree && cart.MoreNeededToReachFreeShipping != 0.0M)
            {
                GetFreeShipping.Text = "<div class=\"page-row free-shipping-prompt\">";
                GetFreeShipping.Text += String.Format(AppLogic.GetString("checkoutshippingmult.aspx.2", SkinID, ThisCustomer.LocaleSetting), ThisCustomer.CurrencyString(cart.FreeShippingThreshold), CommonLogic.Capitalize(cart.FreeShippingMethod));
                GetFreeShipping.Text += "</div>";
                pnlGetFreeShipping.Visible = true;
            }
            else
            {
                pnlGetFreeShipping.Visible = false;
            }

            CartItems.Text = WriteItemAddresses();

            String XmlPackageName2 = AppLogic.AppConfig("XmlPackage.CheckoutShippingMult2PageFooter");
            if (XmlPackageName2.Length != 0)
            {
                XmlPackage_CheckoutShippingMult2PageFooter.Text = AppLogic.RunXmlPackage(XmlPackageName2, base.GetParser, ThisCustomer, SkinID, String.Empty, String.Empty, true, true);
            }

        }
        private string WriteItemAddresses()
        {
            StringBuilder s = new StringBuilder("");

            s.Append("<div class='page-row address-row'>");
            s.Append(AppLogic.GetString("checkoutshippingmult2.aspx.16", SkinID, ThisCustomer.LocaleSetting));
            s.Append("</div>");
			s.Append("<div class='form shipping-method-form'>");
            s.Append(cart.DisplayMultiShipMethodSelector(false, ThisCustomer));
			s.Append("</div>");
            return s.ToString();

        }
        private void ProcessCart()
        {
            if (cart.IsEmpty())
            {
                Response.Redirect("shoppingcart.aspx");
            }

            bool ContinueCheckout = (CommonLogic.FormCanBeDangerousContent("continue") != "");

            List<int> DistinctAddrIds = Shipping.GetDistinctShippingAddressIDs(cart.CartItems);
            
            if (DistinctAddrIds.Count != 0)
            {
                foreach (int AddressID in DistinctAddrIds)
                {
                    String ShippingMethodIDFormField = CommonLogic.FormCanBeDangerousContent("ShippingMethodID_" + AddressID).Replace(",", ""); // remember to remove the hidden field which adds a comma to the form post (javascript again)
                    int ShippingMethodID = 0;
                    String ShippingMethod = String.Empty;
                    if (cart.ShipCalcID != Shipping.ShippingCalculationEnum.UseRealTimeRates)
                    {
                        ShippingMethodID = Localization.ParseUSInt(ShippingMethodIDFormField);
						ShippingMethod = Shipping.GetShippingMethodDisplayName(ShippingMethodID, null);
                    }
                    else
                    {
						if (!String.IsNullOrEmpty(ShippingMethodIDFormField))
						{
							String[] frmsplit = ShippingMethodIDFormField.Split('|');
							ShippingMethodID = Localization.ParseUSInt(frmsplit[0]);
							ShippingMethod = String.Format("{0}|{1}", frmsplit[1], frmsplit[2]);
						}
						else
							return;
                    }

                    String sql = String.Format("update ShoppingCart set ShippingMethodID={0}, ShippingMethod={1} where CustomerID={2} and CartType={3} and ShippingAddressID={4}", ShippingMethodID.ToString(), DB.SQuote(ShippingMethod), ThisCustomer.CustomerID.ToString(), ((int)CartTypeEnum.ShoppingCart).ToString(), AddressID);
                    DB.ExecuteSQL(sql);

                    //find and update the existing cart item so it's available later when displaying the page.
                    for (int i = 0; i < cart.CartItems.Count; i++)
                    {
                        if (((CartItem)cart.CartItems[i]).ShippingAddressID == AddressID)
                        {
                            CartItem ci = (CartItem)cart.CartItems[i];
                            ci.ShippingMethod = ShippingMethod;
                            ci.ShippingMethodID = ShippingMethodID;
                            string itemnotes = Request.Form["notes_" + ci.ShoppingCartRecordID.ToString()];
                            if (itemnotes != null)
                            {
                                cart.SetItemNotes(ci.ShoppingCartRecordID, itemnotes);
                                ci.Notes = itemnotes;
                            }
                            cart.CartItems[i] = ci;
                        }
                    }
                }
                if (!cart.HasMultipleShippingAddresses())
                {
                    // They only selected a single destination addresss, make it primary.
                    int newID = DistinctAddrIds[0];
                    if (newID > 0)
                    {
                        DB.ExecuteSQL(String.Format("update dbo.Customer set ShippingAddressID = {0} where CustomerID={1} ", newID.ToString(), ThisCustomer.CustomerID.ToString()));
                    }
                }
            }

            if (!ContinueCheckout)
            {
                UpdatepageContent();
            }
            else
            {
                if (cart.ContainsGiftCard())
                {
                    Response.Redirect("checkoutgiftcard.aspx");
                }
                else if (ThisCustomer.ThisCustomerSession["PayPalExpressToken"] == "")
                {
                    Response.Redirect("checkoutpayment.aspx");
                }
                else
                {
                    Response.Redirect("checkoutreview.aspx?PaymentMethod=PAYPALEXPRESS");
                }
            }
        }
    }
}
