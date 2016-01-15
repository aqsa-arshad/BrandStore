// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using AspDotNetStorefrontCore;
using System;
using System.Web.UI.WebControls;

namespace AspDotNetStorefrontAdmin.Controls
{
    public partial class AdminMenu : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Build the horizontal menu
                BuildMenu();
            }
        }

        /// <summary>
        /// Populates the horizontal navigation menu control
        /// </summary>
        private void BuildMenu()
        {
            //TODO:  Since this fires on every page, add caching logic
            mnuHorizontalNav.Orientation = Orientation.Horizontal;

            MenuItem[] items = {
                                GetMenuItem("admin.menu.Home", "home", "default.aspx"),
                                GetMenuItem("admin.menu.Orders", "orders", "orders.aspx"),
                                GetMenuItem("admin.menu.Products", "products", "newproducts.aspx"),
                                GetMenuItem("admin.menu.Organization", "organization", "newentities.aspx?entityname=category"),
                                GetMenuItem("admin.menu.Content", "content", "topics.aspx"),
                                GetMenuItem("admin.menu.Customers", "customers", "customers.aspx"),
                                GetMenuItem("admin.menu.Configuration", "configuration", "appconfig.aspx"),
                                GetMenuItem("admin.menu.Help", "help", "about.aspx")
                               };

            foreach (MenuItem m in items)
            {
                mnuHorizontalNav.Items.Add(m);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private MenuItem GetMenuItem(String key, String value)
        {
            return GetMenuItem(key, value, String.Empty, String.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="Url"></param>
        /// <returns></returns>
        private MenuItem GetMenuItem(String key, String value, String Url)
        {
            return GetMenuItem(key, value, Url, String.Empty);
        }

        /// <summary>
        /// Builds the root level menu item nodes
        /// </summary>
        /// <param name="node">String Resource Key of the node you wish to create a menu item for (eg. Orders)</param>
        /// <param name="Url">Url to navigate to if the menu item is clicked</param>
        /// <returns>MenuItem object populated with all values and child items</returns>
        private MenuItem GetMenuItem(String key, String value, String Url, String target)
        {
            String name = AppLogic.GetStringForDefaultLocale(key);
             MenuItem item = new MenuItem();
            //disable Manage Customer level links
            if(name.Equals("Manage Customer Levels"))
            {
                item.Enabled = false;
            }
            item.Text = AppLogic.GetStringForDefaultLocale(key);
            item.Value = CommonLogic.IIF(String.IsNullOrEmpty(value), name, value);
            if (!String.IsNullOrEmpty(Url))
            {
                if (Url.StartsWithIgnoreCase("http"))
                {
                    // Fully qualified URL.  Don't change it
                    item.NavigateUrl = Url;
                }
                else
                {
                    // Relative URL.  Format it as a fully qualified URL
                    item.NavigateUrl = AppLogic.AdminLinkUrl(Url);
                }
            }
            if (!String.IsNullOrEmpty(target))
            {
                item.Target = target;
            }
            // Append Child Nodes
            item = GetChildMenuItems(item);
            return item;
        }

        /// <summary>
        /// Builds the child menu items for a supplied child node
        /// </summary>
        /// <param name="parent">Parent menu item to append child items to</param>
        /// <returns>Parent item with all child items appended</returns>
        private MenuItem GetChildMenuItems(MenuItem parent)
        {
            switch (parent.Value.ToLowerInvariant())
            {
                case "home":
                    break;
                //ORDER MENU NODES
                #region orderMenu
                case "orders":
                    parent.ChildItems.Add(GetMenuItem("admin.menu.OrderManage", "manageOrders", "orders.aspx"));
                    parent.ChildItems.Add(GetMenuItem("admin.menu.PhoneOrder", "phoneOrders", "phoneorder.aspx"));

                    //ORDERS --> RECURRING ORDERS SUBMENU
                    MenuItem childRecurring = GetMenuItem("admin.menu.OrderRecurring", "recurringOrders");
                    childRecurring.ChildItems.Add(GetMenuItem("admin.menu.OrderRecurringToday", "recurringOrdersToday", "recurring.aspx"));
                    childRecurring.ChildItems.Add(GetMenuItem("admin.menu.OrderRecurringPending", "recurringOrdersPending", "recurring.aspx?show=all"));
                    childRecurring.ChildItems.Add(GetMenuItem("admin.menu.OrderRecurringImport", "recurringOrdersImport", "recurringimport.aspx"));
                    parent.ChildItems.Add(childRecurring);

                    parent.ChildItems.Add(GetMenuItem("admin.menu.FailedTransactions", "failedTransactions", "failedtransactions.aspx"));
                    parent.ChildItems.Add(GetMenuItem("admin.menu.FraudOrders", "fraudOrders", "fraudorders.aspx"));

                    //ORDERS --> ORDER REPORTS SUBMENU
                    parent.ChildItems.Add(GetMenuItem("admin.menu.Reports", "reports", "reports.aspx"));

                    break;
                #endregion
                //PRODUCT MENU NODES
                #region productMenu
                case "products":
                    parent.ChildItems.Add(GetMenuItem("admin.menu.ProductMgr", "productManager", "newproducts.aspx"));
                    parent.ChildItems.Add(GetMenuItem("admin.menu.ProductTypes", "productTypes", "producttypes.aspx"));
                    parent.ChildItems.Add(GetMenuItem("admin.menu.Manufacturers", "productManufacturers", "newentities.aspx?entityname=manufacturer"));
                    parent.ChildItems.Add(GetMenuItem("admin.menu.Distributors", "productDistributors", "newentities.aspx?entityname=distributor"));

                    //PRODUCT --> AFFILIATES SUBMENU
                    MenuItem childAffiliates = GetMenuItem("admin.menu.Affiliates", "productAffiliates");
                    childAffiliates.ChildItems.Add(GetMenuItem("admin.menu.AffiliateSearch", "affiliatesSearch", "affiliates.aspx"));
                    childAffiliates.ChildItems.Add(GetMenuItem("admin.menu.AffiliateNew", "affiliatesNew", "editaffiliates.aspx"));
                    parent.ChildItems.Add(childAffiliates);

                    parent.ChildItems.Add(GetMenuItem("admin.menu.ProductPrices", "productPrices", "prices.aspx"));
                    parent.ChildItems.Add(GetMenuItem("admin.menu.ProductSalePrices", "productSalePrices", "saleprices.aspx"));

                    //PRODUCT --> IMPORT/EXPORT SUBMENU
                    MenuItem childImport = GetMenuItem("admin.menu.ProductImportExport", "productImportExport");
                    childImport.ChildItems.Add(GetMenuItem("admin.menu.ProductLoadFromXml", "importFromXml", "importproductsfromxml.aspx"));
                    childImport.ChildItems.Add(GetMenuItem("admin.menu.ProductLoadFromExcel", "importFromExcel", "importproductsfromexcel.aspx"));
                    childImport.ChildItems.Add(GetMenuItem("admin.menu.PriceExport", "importPriceExport", "exportproductpricing.aspx"));
                    childImport.ChildItems.Add(GetMenuItem("admin.menu.PriceImport", "importPriceImport", "importproductpricing.aspx"));
                    parent.ChildItems.Add(childImport);

                    parent.ChildItems.Add(GetMenuItem("admin.menu.Feeds", "productFeeds", "feeds.aspx"));
                    break;
                #endregion
                //ORGANIZATION MENU NODES
                #region organizationMenu
                case "organization":
                    //TODO - Add dynamic entity nodes
                    parent.ChildItems.Add(GetMenuItem("admin.menu.Categories", "organizationCategories", "newentities.aspx?entityname=category"));
                    parent.ChildItems.Add(GetMenuItem("admin.menu.Sections", "organizationSections", "newentities.aspx?entityname=section"));
                    /* UNSUPPORTED ENTITES - GENRES AND VECTORS - UNCOMMENT TO ADD TO MENU
                    parent.ChildItems.Add(GetMenuItem("admin.menu.Genres", "organizationGenres", "newentities.aspx?entityname=genre"));
                    parent.ChildItems.Add(GetMenuItem("admin.menu.Vectors", "organizationVectors", "newentities.aspx?entityname=vector"));
                    */
                    parent.ChildItems.Add(GetMenuItem("admin.menu.Manufacturers", "organizationManufacturers", "newentities.aspx?entityname=manufacturer"));
                    parent.ChildItems.Add(GetMenuItem("admin.menu.Distributors", "organizationdistributor", "newentities.aspx?entityname=distributor"));
                    break;
                #endregion
                //CONTENT MENU NODES
                #region contentMenu
                case "content":
                    parent.ChildItems.Add(GetMenuItem("admin.menu.Topics", "contentTopics", "topics.aspx"));
                    parent.ChildItems.Add(GetMenuItem("admin.menu.News", "contentNews", "news.aspx"));
                    parent.ChildItems.Add(GetMenuItem("admin.menu.StringResources", "contentStringResources", "stringresource.aspx"));
                    break;
                #endregion
                //CUSTOMER MENU NODES
                #region customerMenu
                case "customers":
                    parent.ChildItems.Add(GetMenuItem("admin.menu.CustomerAlerts", "customerAlerts", "customeralerts.aspx"));
                    parent.ChildItems.Add(GetMenuItem("admin.menu.CustomerLevels", "customersLevels", "customerlevels.aspx"));
                    parent.ChildItems.Add(GetMenuItem("admin.menu.CustomerEdit", "customersEdit", "customers.aspx"));
                    parent.ChildItems.Add(GetMenuItem("admin.menu.CustomerAdd", "customersAdd", "CustomerDetail.aspx"));
                    break;
                #endregion
                //CONFIGURATION MENU NODES
                #region configurationMenu
                case "configuration":
          
                    parent.ChildItems.Add(GetMenuItem("admin.menu.StoreMaintenance", "configurationStoreMaintenance", "storemaintenance.aspx"));                  
                    //CONFIGURATION --> ADVANCED CONFIGURATION SUBMENU
                    MenuItem childAdvanced = GetMenuItem("admin.menu.AdvancedConfiguration", "configurationAdvanced");
                    childAdvanced.ChildItems.Add(GetMenuItem("admin.menu.AppConfigParameters", "advancedAppConfigs", "appconfig.aspx"));
                    childAdvanced.ChildItems.Add(GetMenuItem("admin.menu.GlobalConfigParameters", "advancedGlobalConfigs", "storemaintenance.aspx"));
                    childAdvanced.ChildItems.Add(GetMenuItem("admin.menu.EventHandlerParameters", "advancedEventHandlers", "eventhandler.aspx"));
                    childAdvanced.ChildItems.Add(GetMenuItem("admin.menu.RunSQL", "advancedRunSql", "runsql.aspx"));
                    childAdvanced.ChildItems.Add(GetMenuItem("admin.menu.XMLPackageValidator", "advancedXmlPackageValidator", "packagevalidation.aspx"));
                    childAdvanced.ChildItems.Add(GetMenuItem("admin.menu.FullTextSearch", "advancedFullTextSearch", "setupfts.aspx"));
                    parent.ChildItems.Add(childAdvanced);
                    parent.ChildItems.Add(GetMenuItem("admin.menu.Wizard", "configurationWizard", "wizard.aspx"));
                    parent.ChildItems.Add(GetMenuItem("admin.menu.SkinSelector", "skinselector", "skinselector.aspx"));

					//CONFIGURATION --> PAYMENT SUBMENU
                    MenuItem childPayment = GetMenuItem("admin.menu.Payment", "configurationPayment");
                    childPayment.ChildItems.Add(GetMenuItem("admin.menu.GiftCards", "paymentGiftCards", "giftcards.aspx"));
                    childPayment.ChildItems.Add(GetMenuItem("admin.menu.CreditCards", "paymentCreditCards", "creditcards.aspx"));
                    childPayment.ChildItems.Add(GetMenuItem("admin.menu.PaymentMethods", "paymentMethods", "wizard.aspx#payments"));
                    parent.ChildItems.Add(childPayment);

                    //CONFIGURATION --> PRICING AND PROMOTIONS SUBMENU
                    MenuItem childPricingPromo = GetMenuItem("admin.menu.PricingAndPromotions", "configurationPromo");
					childPricingPromo.ChildItems.Add(GetMenuItem("admin.menu.PayPalBillMeLater", "billmelaterSetup", "bmlsettings.aspx"));
                    childPricingPromo.ChildItems.Add(GetMenuItem("admin.menu.BuySafe", "buysafeSetup", "buysafeSetup.aspx"));
                    childPricingPromo.ChildItems.Add(GetMenuItem("admin.menu.Coupons", "promoCoupons", "PromotionEditor.aspx"));
                    childPricingPromo.ChildItems.Add(GetMenuItem("admin.menu.QuantityDiscounts", "promoQuantityDiscounts", "quantitydiscounts.aspx"));
                    childPricingPromo.ChildItems.Add(GetMenuItem("admin.menu.OrderOptions", "promoOrderOptions", "orderoption.aspx"));
                    childPricingPromo.ChildItems.Add(GetMenuItem("admin.menu.SalesPrompts", "promoSalesPrompts", "salesprompts.aspx"));
                    childPricingPromo.ChildItems.Add(GetMenuItem("admin.menu.Mailing", "promoMailingManager", "mailingmgr.aspx"));
                    childPricingPromo.ChildItems.Add(GetMenuItem("admin.menu.NewsletterManager", "promoNewsletterManager", "newslettermailinglistmanager.aspx"));

                    parent.ChildItems.Add(childPricingPromo);

                    //CONFIGURATION --> SHIPPING SUBMENU
                    MenuItem childShipping = GetMenuItem("admin.menu.Shipping", "configurationShipping");
                    childShipping.ChildItems.Add(GetMenuItem("admin.menu.ShippingMethods", "shippingMethods", "shippingmethods.aspx"));
                    childShipping.ChildItems.Add(GetMenuItem("admin.menu.ShippingZones", "shippingZones", "shippingzones.aspx"));
                    childShipping.ChildItems.Add(GetMenuItem("admin.menu.ShippingRates", "shippingRates", "shipping.aspx"));
                    childShipping.ChildItems.Add(GetMenuItem("admin.menu.RTShipping.LocalPickup", "shippingLocalPickup", "rtshippinglocalpickup.aspx"));
                    childShipping.ChildItems.Add(GetMenuItem("admin.menu.ShippingFedExMeter", "shippingFedExMeter", "fedexmeterrequest.aspx"));
                    childShipping.ChildItems.Add(GetMenuItem("admin.menu.Shipwire", "shippingShipwire", "/shipwire.aspx"));
                    parent.ChildItems.Add(childShipping);

                    //CONFIGURATION --> TAXES SUBMENU
                    MenuItem childTaxes = GetMenuItem("admin.menu.Taxes", "configurationTaxes");
                    childTaxes.ChildItems.Add(GetMenuItem("admin.menu.TaxClass", "taxesClass", "taxclass.aspx"));
                    childTaxes.ChildItems.Add(GetMenuItem("admin.menu.States", "taxesStates", "states.aspx"));
                    childTaxes.ChildItems.Add(GetMenuItem("admin.menu.Countries", "taxesCountries", "countries.aspx"));
                    childTaxes.ChildItems.Add(GetMenuItem("admin.menu.TaxZip", "taxesZips", "taxzips.aspx"));
                    parent.ChildItems.Add(childTaxes);

                    parent.ChildItems.Add(GetMenuItem("admin.menu.MailingTest", "configurationMailingTest", "mailingtest.aspx"));

                    //CONFIGURATION --> LOCALIZATION SUBMENU
                    MenuItem childLocalization = GetMenuItem("admin.menu.Localization", "configurationLocalization");
                    childLocalization.ChildItems.Add(GetMenuItem("admin.menu.LocaleSettings", "localizationLocaleSettings", "localesettings.aspx"));
                    childLocalization.ChildItems.Add(GetMenuItem("admin.menu.Currencies", "localizationCurrencies", "currencies.aspx"));
                    childLocalization.ChildItems.Add(GetMenuItem("admin.menu.StringResources", "localizationStringResources", "stringresource.aspx"));
                    parent.ChildItems.Add(childLocalization);

                    //CONFIGURATION --> MAINTENANCE SUBMENU
                    MenuItem childMaintenance = GetMenuItem("admin.menu.Maintenance", "configurationMaintenance");
                    childMaintenance.ChildItems.Add(GetMenuItem("admin.menu.MonthlyMaintenance", "maintenanceMonthly", "monthlymaintenance.aspx"));
                    childMaintenance.ChildItems.Add(GetMenuItem("admin.menu.StoreWide", "maintenanceStoreWide", "storewide.aspx"));
                    childMaintenance.ChildItems.Add(GetMenuItem("admin.menu.ChangeEncryptKey", "maintenanceChangeEncryptKey", "/changeencryptkey.aspx"));
                    childMaintenance.ChildItems.Add(GetMenuItem("admin.menu.ViewSystemLog", "maintenanceViewSystemLog", "systemlog.aspx"));
                    childMaintenance.ChildItems.Add(GetMenuItem("admin.menu.ViewSecurityLog", "maintenanceViewSecurityLog", "securitylog.aspx"));
                    parent.ChildItems.Add(childMaintenance);

                    //CONFIGURATION --> POLLS AND REVIEWS
                    MenuItem childPolls = GetMenuItem("admin.menu.PollsAndReviews", "configurationPolls");
                    childPolls.ChildItems.Add(GetMenuItem("admin.menu.Polls", "pollsPolls", "polls.aspx"));
                    childPolls.ChildItems.Add(GetMenuItem("admin.menu.Ratings", "pollsRatings", "manageratings.aspx"));
                    childPolls.ChildItems.Add(GetMenuItem("admin.menu.BadWord", "pollsBadWords", "badword.aspx"));
                    parent.ChildItems.Add(childPolls);

                    parent.ChildItems.Add(GetMenuItem("admin.menu.InventoryControl", "configurationInventoryControl", "inventorycontrol.aspx"));
                    break;
                #endregion
                //HELP MENU NODES
                #region helpMenu
                case "help":
                    parent.ChildItems.Add(GetMenuItem("admin.menu.OnlineManual", "helpOnlineManual", "http://manual.aspdotnetstorefront.com", "_blank"));
                    parent.ChildItems.Add(GetMenuItem("admin.menu.About", "helpAbout", "about.aspx?debuglicense=true"));
                    break;
                #endregion
                default:
                    break;
            }
            return parent;
        }
    }
}
