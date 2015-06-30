// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.Security;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for orderconfirmation.
    /// </summary>
	[PageType("orderconfirmation")]
    public partial class orderconfirmation : SkinBase
    {
        protected void Page_Load(object sender, System.EventArgs e)
        {

            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            RequireSecurePage();

            Address BillingAddress = new Address();

            if (!ThisCustomer.IsRegistered)
            {
                bool boolAllowAnon = AppLogic.AppConfigBool("PasswordIsOptionalDuringCheckout");
                if (!boolAllowAnon && ThisCustomer.PrimaryBillingAddressID > 0)
                {
                    BillingAddress.LoadByCustomer(ThisCustomer.CustomerID, ThisCustomer.PrimaryBillingAddressID, AddressTypes.Billing);
                    if (BillingAddress.PaymentMethodLastUsed == AppLogic.ro_PMPayPalExpress || BillingAddress.PaymentMethodLastUsed == AppLogic.ro_PMPayPalExpressMark)
                    {
                        boolAllowAnon = AppLogic.AppConfigBool("PayPal.Express.AllowAnonCheckout");
                    }
                }

                if (!boolAllowAnon)
                {
                    RequiresLogin(CommonLogic.GetThisPageName(false) + "?" + CommonLogic.ServerVariables("QUERY_STRING"));
                }
            }

            // this may be overwridden by the XmlPackage below!
            SectionTitle = AppLogic.GetString("orderconfirmation.aspx.1", SkinID, ThisCustomer.LocaleSetting);

            // clear anything that should not be stored except for immediate usage:
            BillingAddress.LoadByCustomer(ThisCustomer.CustomerID, ThisCustomer.PrimaryBillingAddressID, AddressTypes.Billing);
            BillingAddress.PONumber = String.Empty;
            if (!ThisCustomer.MasterShouldWeStoreCreditCardInfo)
            {
                BillingAddress.ClearCCInfo();
            }
            BillingAddress.UpdateDB();
            AppLogic.ClearCardExtraCodeInSession(ThisCustomer);
        }

        protected override void OnInit(EventArgs e)
        {
            int CustomerID = ThisCustomer.CustomerID;
            int OrderNumber = CommonLogic.QueryStringUSInt("OrderNumber");

            StringBuilder output = new StringBuilder();

            if (CustomerID != 0 && OrderNumber != 0)
            {
                Order ord = new Order(OrderNumber, ThisCustomer.LocaleSetting);

                if (ThisCustomer.CustomerID != ord.CustomerID)
                {
                    Response.Redirect(SE.MakeDriverLink("ordernotfound"));
                }

                if (ThisCustomer.ThisCustomerSession["3DSecure.LookupResult"].Length > 0)
                {
                    DB.ExecuteSQL("update orders set CardinalLookupResult=" + DB.SQuote(ThisCustomer.ThisCustomerSession["3DSecure.LookupResult"]) + " where OrderNumber=" + OrderNumber.ToString());
                }
                ThisCustomer.ThisCustomerSession.Clear();

                String ReceiptURL = "receipt.aspx?ordernumber=" + OrderNumber.ToString() + "&customerid=" + CustomerID.ToString();

                bool orderexists;
                using (SqlConnection conn = DB.dbConn())
                {
                    conn.Open();
                    using (IDataReader rs = DB.GetRS("select * from dbo.orders where customerid=" + CustomerID.ToString() + " and ordernumber=" + OrderNumber.ToString(), conn))
                    {
                        orderexists = rs.Read();
                    }
                }

                if (orderexists)
                {

                    String PM = AppLogic.CleanPaymentMethod(ord.PaymentMethod);
                    String StoreName = AppLogic.AppConfig("StoreName");
                    bool UseLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");

                    if (!ord.AlreadyConfirmed)
                    {
                        // check to see if this was an "admin edit order" and if so, cleanup the old order, as it was being replaced by this new order:
                        int EditingOrderNumber = base.EditingOrderImpersonation;
                        if (base.IsInImpersonation && EditingOrderNumber != 0)
                        {
                            Order editedOrder = new Order(EditingOrderNumber, Localization.GetDefaultLocale());
                            if (!editedOrder.HasBeenEdited && editedOrder.TransactionState == AppLogic.ro_TXStateAuthorized || editedOrder.TransactionState == AppLogic.ro_TXStateCaptured)
                            {
                                editedOrder.EditedOn = System.DateTime.Now;
                                editedOrder.RelatedOrderNumber = OrderNumber;
                                // try void first, or refund if that doesn't work
                                if (Gateway.OrderManagement_DoVoid(editedOrder, Localization.GetDefaultLocale()) != AppLogic.ro_OK)
                                {
                                    Gateway.OrderManagement_DoFullRefund(editedOrder, Localization.GetDefaultLocale(), "Order Was Edited, New Order #: " + OrderNumber.ToString());
                                }
                            }
                            base.AdminImpersonatingCustomer.ThisCustomerSession.ClearVal("IGD_EDITINGORDER");
                        }

                        DB.ExecuteSQL("update Customer set OrderOptions=NULL, OrderNotes=NULL, FinalizationData=NULL where CustomerID=" + CustomerID.ToString());


                        if (PM.ToUpper() != "CHECKOUTBYAMAZON")
                            AppLogic.SendOrderEMail(ThisCustomer, OrderNumber, false, PM, true, base.EntityHelpers, base.GetParser);
                    }

                    String XmlPackageName = AppLogic.AppConfig("XmlPackage.OrderConfirmationPage");
                    if (XmlPackageName.Length == 0)
                    {
                        XmlPackageName = "page.orderconfirmation.xml.config";
                    }

                    if (XmlPackageName.Length != 0)
                    {
                        output.Append(AppLogic.RunXmlPackage(XmlPackageName, base.GetParser, ThisCustomer, SkinID, String.Empty, "OrderNumber=" + OrderNumber.ToString(), true, true));
                    }

                    Order order = new Order(OrderNumber);

                    if (order.PaymentMethod.ToLower() == GatewayCheckoutByAmazon.CheckoutByAmazon.CBA_Gateway_Identifier.ToLower())
                    {
                        GatewayCheckoutByAmazon.CheckoutByAmazon checkoutByAmazon = new GatewayCheckoutByAmazon.CheckoutByAmazon();
                        output.Append("");
                        output.Append(checkoutByAmazon.RenderOrderDetailWidget(OrderNumber));
                    }

                    if (!ord.AlreadyConfirmed)
                    {
                        if (AppLogic.AppConfigBool("IncludeGoogleTrackingCode"))
                        {
                            Topic GoogleTrackingCode = new Topic("GoogleTrackingCode");
                            if (GoogleTrackingCode.Contents.Length != 0)
                            {
                                output.Append(GoogleTrackingCode.Contents.Replace("(!ORDERTOTAL!)", Localization.CurrencyStringForGatewayWithoutExchangeRate(ord.Total(true))).Replace("(!ORDERNUMBER!)", OrderNumber.ToString()).Replace("(!CUSTOMERID!)", ThisCustomer.CustomerID.ToString()));
                            }
                        }

                        Topic GeneralTrackingCode = new Topic("ConfirmationTracking");
                        if (GeneralTrackingCode.Contents.Length != 0)
                        {
                            output.Append(GeneralTrackingCode.Contents.Replace("(!ORDERTOTAL!)", Localization.CurrencyStringForGatewayWithoutExchangeRate(ord.Total(true))).Replace("(!ORDERNUMBER!)", OrderNumber.ToString()).Replace("(!CUSTOMERID!)", ThisCustomer.CustomerID.ToString()));
                        }
                        if (AppLogic.AppConfigBool("Google.EcomOrderTrackingEnabled") && AppLogic.AppConfigBool("Google.DeprecatedEcomTokens.Enabled"))
                        {
                            output.Append(AppLogic.GetGoogleEComTrackingV2(ThisCustomer, true));
                        }

                        //Google Trusted Stores Order Confiramation module code
                        if (AppLogic.AppConfigBool("GoogleTrustedStoreEnabled") && AppLogic.AppConfig("GoogleTrustedStoreID").Length != 0)
                        {
                            output.AppendLine("");
                            output.AppendLine("<!-- START Trusted Stores Order --> ");
                            output.AppendLine("<div id=\"gts-order\" style=\"display:none;\">");
                            output.AppendLine("<!-- start order and merchant information -->");
                            output.AppendLine("<span id=\"gts-o-id\">" + OrderNumber.ToString() + "</span>");
                            output.AppendLine("<span id=\"gts-o-domain\">" + AppLogic.AppConfig("LiveServer") + "</span>");
                            output.AppendLine("<span id=\"gts-o-email\">" + CommonLogic.IIF(ThisCustomer.EMail.Length > 0, ThisCustomer.EMail, "anonymous@anonymous.com") + "</span>");
                            output.AppendLine("<span id=\"gts-o-country\">" + "US" + "</span>"); //Hard-coded, Google Trusted Stores is for US only currently.
                            output.AppendLine("<span id=\"gts-o-currency\">" + "USD" + "</span>"); //Hard-coded, Google Trusted Stores is for USD only currently.
                            output.AppendLine("<span id=\"gts-o-total\">" + Math.Round(ord.Total(true), 2).ToString() + "</span>");
                            output.AppendLine("<span id=\"gts-o-discounts\">" + Math.Round((ord.SubTotal(false) - ord.SubTotal(true)), 2).ToString() + "</span>");
                            output.AppendLine("<span id=\"gts-o-shipping-total\">" + Math.Round(ord.ShippingTotal(true), 2).ToString() + "</span>");
                            output.AppendLine("<span id=\"gts-o-tax-total\">" + Math.Round(ord.TaxTotal(true), 2).ToString() + "</span>");
                            output.AppendLine("<span id=\"gts-o-est-ship-date\">" + (System.DateTime.Now.AddDays(AppLogic.AppConfigUSInt("GoogleTrustedStoreShippingLeadTime"))).ToString("yyyy-MM-dd") + "</span>");
                            output.AppendLine("<span id=\"gts-o-has-preorder\">" + "N" + "</span>"); //Hard-coded for now, backorders/pre-orders not currently supported
                            output.AppendLine("<span id=\"gts-o-has-digital\">" + CommonLogic.IIF(ord.HasDownloadComponents(false), "Y", "N") + "</span>");
                            output.AppendLine("<!-- end order and merchant information -->");
                            output.AppendLine("<!-- start repeated item specific information -->");

                            foreach (CartItem ci in ord.CartItems)
                            {
                                output.AppendLine("<span class=\"gts-item\">");
                                output.AppendLine("<span class=\"gts-i-name\">" + ci.ProductName + "</span>");
                                output.AppendLine("<span class=\"gts-i-price\">" + Math.Round(ci.Price, 2).ToString() + "</span>");
                                output.AppendLine("<span class=\"gts-i-quantity\">" + ci.Quantity + "</span>");
                                output.AppendLine("<span class=\"gts-i-prodsearch-id\">" + ci.ProductID.ToString() + "-" + ci.VariantID.ToString() + "-" + AppLogic.CleanSizeColorOption(ci.ChosenSize) + "-" + AppLogic.CleanSizeColorOption(ci.ChosenColor) + "</span>");
                                output.AppendLine("<span class=\"gts-i-prodsearch-store-id\">" + AppLogic.AppConfig("GoogleTrustedStoreProductSearchID") + "</span>");
                                output.AppendLine("<span class=\"gts-i-prodsearch-country\">" + AppLogic.AppConfig("GoogleTrustedStoreCountry") + "</span>");
                                output.AppendLine("<span class=\"gts-i-prodsearch-language\">" + AppLogic.AppConfig("GoogleTrustedStoreLanguage") + "</span>");
                                output.AppendLine("</span>");
                            }

                            output.AppendLine("<!-- end repeated item specific information -->");
                            output.AppendLine("</div>");
                            output.AppendLine("<!-- END Trusted Stores -->");
                        }

                        if (AppLogic.GlobalConfigBool("BuySafe.Enabled") && AppLogic.GlobalConfig("BuySafe.Hash").Length != 0)
                        {
                            output.AppendLine("");
                            output.AppendLine("<!-- BEGIN: buySAFE Guarantee--> ");
                            output.AppendLine("<script src=\"" + AppLogic.GlobalConfig("BuySafe.RollOverJSLocation") + "\"></script>");
                            output.AppendLine("<span id=\"BuySafeGuaranteeSpan\"></span>");
                            output.AppendLine("<script type=\"text/javascript\"> ");
                            output.AppendLine("    buySAFE.Hash = '" + AppLogic.GlobalConfig("BuySafe.Hash") + "';");
                            output.AppendLine("    buySAFE.Guarantee.order = \"" + OrderNumber.ToString() + "\"; ");
                            output.AppendLine("    buySAFE.Guarantee.total = \"" + Localization.CurrencyStringForGatewayWithoutExchangeRate(ord.Total(true)) + "\"; ");
                            output.AppendLine("    buySAFE.Guarantee.email = \"" + ThisCustomer.EMail + "\"; ");
                            output.AppendLine("    WriteBuySafeGuarantee(\"JavaScript\"); ");
                            output.AppendLine("</script> ");
                            output.AppendLine("<!-- END: buySAFE Guarantee-->");
                        }

						if (AppLogic.AppConfigBool("PayPal.Express.UseIntegratedCheckout"))
							output.AppendLine(AspDotNetStorefrontGateways.Processors.PayPalController.GetExpressCheckoutIntegratedScript(false));

                    }
                    DB.ExecuteSQL("Update Orders set AlreadyConfirmed=1 where OrderNumber=" + OrderNumber.ToString());
                }
                else
                {
                    output.Append("<div align=\"center\">");
                    output.Append("");
                    output.Append(AppLogic.GetString("orderconfirmation.aspx.19", SkinID, ThisCustomer.LocaleSetting));
                    output.Append("");
                    output.Append("</div>");
                }

                if (!ord.AlreadyConfirmed) //only do this once
                {
                    //Low inventory notification
                    if (AppLogic.AppConfigBool("SendLowStockWarnings") && ord.TransactionIsCaptured()) //If delayed capture, we'll check this when the order is captured
                    {
                        List<int> purchasedVariants = new List<int>();
                        foreach (CartItem ci in ord.CartItems)
                        {
                            purchasedVariants.Add(ci.VariantID);
                        }

                        AppLogic.LowInventoryWarning(purchasedVariants);
                    }
                }

            }
            else
            {
                output.Append("<p><b>Error: Invalid Customer ID or Invalid Order Number</b></p>");
            }

            if (!ThisCustomer.IsRegistered || AppLogic.AppConfigBool("ForceSignoutOnOrderCompletion"))
            {
                if (AppLogic.AppConfigBool("SiteDisclaimerRequired"))
                {
                    Profile.SiteDisclaimerAccepted = string.Empty;
                }

                //V3_9 Kill the Authentication ticket.
                Session.Clear();
                Session.Abandon();
                FormsAuthentication.SignOut();
                ThisCustomer.Logout();
            }

            litOutput.Text = output.ToString();

            base.OnInit(e);
        }

    }
}
