// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Routing;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

namespace AspDotNetStorefrontCore
{
    /// <summary>
    /// Summary description for AppLogic.
    /// </summary>
    /// 
    public delegate void ApplicationStartRoutine();

	public partial class AppLogic
    {

        public static List<Inventory> LstInventory = new List<Inventory>();

        static public int NumProductsInDB = 0; // set to # of products in the db on Application_Start. Not updated thereafter
        static public bool CachingOn = false;  // set to true in Application_Start if AppConfig:CacheMenus=true

        public static GlobalConfigCollection GlobalConfigTable;
        static public AspdnsfEventHandlers EventHandlerTable;//LOADED in application_start of the respective web project
        static public BadWords BadWordTable;
        //static public StringResources StringResourceTable; // LOADED in application_start of the respective web project
        static public CountryTaxRates CountryTaxRatesTable; // LOADED in application_start of the respective web project
        static public StateTaxRates StateTaxRatesTable; // LOADED in application_start of the respective web project
        static public ZipTaxRates ZipTaxRatesTable; // LOADED in application_start of the respective web project

        static public Hashtable ImageFilenameCache = new Hashtable(); // Caching is ALWAYS on for images, cache of category/section/product/etc image filenames from LookupImage. Added on first need
        
        static public Dictionary<int, EntityHelper> CategoryStoreEntityHelper;
        static public Dictionary<int, EntityHelper> SectionStoreEntityHelper;
        static public Dictionary<int, EntityHelper> ManufacturerStoreEntityHelper;
        static public Dictionary<int, EntityHelper> DistributorStoreEntityHelper;
        static public Dictionary<int, EntityHelper> GenreStoreEntityHelper;
        static public Dictionary<int, EntityHelper> VectorStoreEntityHelper;
        static public Dictionary<int, EntityHelper> LibraryStoreEntityHelper;

        static public ApplicationStartRoutine m_RestartApp;

        static public readonly String[] ro_SupportedEntities = { "Category", "Section", "Manufacturer", "Library", "Distributor", "Genre", "Vector" };

        // the innova admin HTML editor adds images so they can be viewed in the admin site, so set this to true (via AppConfig) 
        // if you want ../images to be replaced with images so images resolve on the store side, applies to any HTML field in the db.
        static public bool ReplaceImageURLFromAssetMgr = false;

        static public bool AppIsStarted = false;
        static public AspNetHostingPermissionLevel TrustLevel;

        static public XmlDocument Customerlevels;

        static public int MicropayProductID = 0;
        static public int MicropayVariantID = 0;
        static public int AdHocProductID = 0;
        static public int AdHocVariantID = 0;

        static public readonly String ro_DefaultProductXmlPackage = "product.variantsinrightbar.xml.config";
        static public readonly String ro_DefaultProductKitXmlPackage = "product.kitproduct.xml.config";
        static public readonly String ro_DefaultEntityXmlPackage = "entity.grid.xml.config";
        static public readonly String ro_DefaultCelebrityXmlPackage = "entity.grid.xml.config";

        static public readonly String ro_NotApplicable = "N/A";

        public enum TransactionTypeEnum
        {
            UNKNOWN = 0,
            CHARGE = 1,
            CREDIT = 2,
            RECURRING_AUTO = 3
        }


        public enum EntityType
        {
            Unknown = 0,
            Category = 1,
            Section = 2,
            Manufacturer = 3,
            Distributor = 4,
            Library = 5,
            Genre = 6,
            Vector = 7,
            Affiliate = 8,
            CustomerLevel = 9,
            Product = 10
        }

        static public readonly String ro_CCNotStoredString = "Not Stored";
        static public readonly String ro_TXModeAuthCapture = "AUTH CAPTURE";
        static public readonly String ro_TXModeAuthOnly = "AUTH";
        static public readonly String ro_TXStateAuthorized = "AUTHORIZED";
        static public readonly String ro_TXStateCaptured = "CAPTURED";
        static public readonly String ro_TXStateVoided = "VOIDED";
        static public readonly String ro_TXStateForceVoided = "FORCE VOIDED";
        static public readonly String ro_TXStateRefunded = "REFUNDED";
        static public readonly String ro_TXStateFraud = "FRAUD";
        static public readonly String ro_TXStateUnknown = "UNKNOWN"; // possible, but not used
        static public readonly String ro_TXStatePending = "PENDING"; // possible, but not used
        public static readonly String ro_TXStateRefused = "REFUSED"; // possible, but not used
        public static readonly String ro_TXStateError = "ERROR"; // possible, but not used

        static public readonly int ro_GenericTaxClassID = 1;

        static public readonly String ro_OK = "OK";
        static public readonly String ro_TBD = "TBD";
        static public readonly String ro_3DSecure = "3Denrollee";
		static public readonly String ro_CardinalCommerce = "CardinalCommerce";

        static public readonly String ro_SKUMicropay = "MICROPAY";

        static public readonly String ro_PMMicropay = "MICROPAY";
        static public readonly String ro_PMCreditCard = "CREDITCARD";
        static public readonly String ro_PMECheck = "ECHECK";
        static public readonly String ro_PMRequestQuote = "REQUESTQUOTE";
        static public readonly String ro_PMCOD = "COD";
        static public readonly String ro_PMCODMoneyOrder = "CODMONEYORDER";
        static public readonly String ro_PMCODCompanyCheck = "CODCOMPANYCHECK";
        static public readonly String ro_PMCODNet30 = "CODNET30";
        static public readonly String ro_PMPurchaseOrder = "PURCHASEORDER";
        static public readonly String ro_PMPayPal = "PAYPAL";
        static public readonly String ro_PMPayPalExpress = "PAYPALEXPRESS";
        static public readonly String ro_PMPayPalExpressMark = "PAYPALEXPRESSMARK"; // used for checkout flow only, order is stored as ro_PMPayPalExpress
        static public readonly String ro_PMCheckByMail = "CHECKBYMAIL";
        static public readonly String ro_PMCardinalMyECheck = "CARDINALMYECHECK";
        static public readonly String ro_PMAmazonSimplePay = "AMAZONSIMPLEPAY";
		static public readonly String ro_PMMoneybookersQuickCheckout = "MONEYBOOKERSQUICKCHECKOUT";
        static public readonly String ro_PMSecureNetVault = "SECURENETVAULT";
        static public readonly String ro_PMPayPalEmbeddedCheckout = "PAYPALPAYMENTSADVANCED";
		static public readonly String ro_PMBypassGateway = "BYPASSGATEWAY";

        public static readonly String[] ro_PMsWhichAreSetToPendingForInitialTXState = {
                                                                                               ro_PMCheckByMail, ro_PMCOD,
                                                                                               ro_PMPurchaseOrder,
                                                                                               ro_PMCODNet30,
                                                                                               ro_PMCODMoneyOrder,
                                                                                               ro_PMCODCompanyCheck,
                                                                                               ro_PMRequestQuote,
                                                                                               ro_PMCardinalMyECheck
                                                                                       };

        public static bool isPendingPM(string PM)
        {
            bool setAsPending = false;
            foreach (String s in ro_PMsWhichAreSetToPendingForInitialTXState)
            {
                if (PM == s)
                {
                    setAsPending = true;
                    break;
                }
            }
            return setAsPending;
        }

        // these are pulled from string resources:
        static public readonly String ro_PMMicropayForDisplay = "(!pm.micropay.display!)";
        static public readonly String ro_PMCreditCardForDisplay = "(!pm.creditcard.display!)";
        static public readonly String ro_PMECheckForDisplay = "(!pm.echeck.display!)";
        static public readonly String ro_PMRequestQuoteForDisplay = "(!pm.requestquote.display!)";
        static public readonly String ro_PMCODForDisplay = "(!pm.cod.display!)";
        static public readonly String ro_PMCODMoneyOrderForDisplay = "(!pm.codmoneyorder.display!)";
        static public readonly String ro_PMCODCompanyCheckForDisplay = "(!pm.codcompanycheck.display!)";
        static public readonly String ro_PMCODNet30ForDisplay = "(!pm.codnet30.display!)";
        static public readonly String ro_PMPurchaseOrderForDisplay = "(!pm.purchaseorder.display!)";
        static public readonly String ro_PMPayPalForDisplay = "(!pm.paypal.display!)";
        static public readonly String ro_PMPayPalExpressForDisplay = "(!pm.paypalexpress.display!)";
        static public readonly String ro_PMCheckByMailForDisplay = "(!pm.checkbymail.display!)";
        static public readonly String ro_PMBypassGatewayForDisplay = "(!pm.bypassgateway.display!)";

        private const int NONE_FOUND = 0;

        public const int OUT_OF_STOCK_ALL_VARIANTS = -1;
        public const int OUT_OF_STOCK_DEFAULT_VARIANT = 0;

        public AppLogic() { }

        public static bool HideForWholesaleSite(int CustomerLevelId)
        {
            bool isSomeSortOfDefault = CustomerLevelId == 0 || CustomerLevelId == AppLogic.AppConfigUSInt("DefaultCustomerLevelID");
            return AppLogic.AppConfigBool("WholesaleOnlySite") && isSomeSortOfDefault;
        }

        public static bool ThereAreRecurringOrdersThatNeedCCStorage()
        {
            return !AppLogic.AppConfigBool("Recurring.UseGatewayInternalBilling") && (DB.GetSqlN("select count(*) as N from ShoppingCart  with (NOLOCK)  where RecurringSubscriptionID='' and CartType=" + ((int)CartTypeEnum.RecurringCart).ToString()) > 0);
        }

        public static bool ThereAreRecurringGatewayAutoBillOrders()
        {
            return AppLogic.AppConfigBool("Recurring.UseGatewayInternalBilling") && (DB.GetSqlN("select count(*) as N from ShoppingCart  with (NOLOCK)  where RecurringSubscriptionID<>'' and CartType=" + ((int)CartTypeEnum.RecurringCart).ToString()) > 0);
        }

        public static String GetGoogleEComTracking(Customer ThisCustomer)
        {
            if (!AppLogic.AppConfigBool("UseLiveTransactions") ||
                ThisCustomer == null ||
                false == CommonLogic.GetThisPageName(false).StartsWith("orderconfirmation.aspx", StringComparison.InvariantCultureIgnoreCase) ||
                CommonLogic.QueryStringUSInt("OrderNumber") == 0)
            {
                return String.Empty;
            }

            try
            {
                int OrderNumber = CommonLogic.QueryStringUSInt("OrderNumber");
                Order ord = new Order(OrderNumber, Localization.GetDefaultLocale());

                if (ThisCustomer.CustomerID != ord.CustomerID)
                {
                    return String.Empty;
                }

                StringBuilder tmpS = new StringBuilder(1024);
                tmpS.Append("<form style=\"display:none;\" name=\"utmform\">\n");
                tmpS.Append("<textarea id=\"utmtrans\">\n");
                tmpS.Append(String.Format(" UTM:T|{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7} \n",
                    ord.OrderNumber.ToString(),
                    ord.AffiliateName,
                    Localization.CurrencyStringForGatewayWithoutExchangeRate(ord.Total(true)),
                    Localization.CurrencyStringForGatewayWithoutExchangeRate(ord.TaxTotal(true)),
                    Localization.CurrencyStringForGatewayWithoutExchangeRate(ord.ShippingTotal(true)),
                    ord.BillingAddress.m_City,
                    ord.BillingAddress.m_State,
                    ord.BillingAddress.m_Country
                    ));

                foreach (CartItem c in ord.CartItems)
                {
                    tmpS.Append(String.Format(" UTM:I|{0}|{1}|{2}|{3}|{4}|{5} \n",
                        ord.OrderNumber.ToString(),
                        c.SKU,
                        c.ProductName,
                        AppLogic.GetFirstProductEntity(AppLogic.LookupHelper(EntityDefinitions.readonly_CategoryEntitySpecs.m_EntityName, 0), c.ProductID, false, Localization.GetDefaultLocale()),
                        Localization.CurrencyStringForGatewayWithoutExchangeRate(c.Price / c.Quantity),
                        c.Quantity.ToString()
                        ));
                }

                tmpS.Append("</textarea>");
                tmpS.Append("</form>\n");
                tmpS.Append("<script type=\"text/javascript\">__utmSetTrans();</script>");
                return tmpS.ToString();
            }
            catch
            {
                return String.Empty;
            }
        }

        public static string SkipJackEmail(int CustomerID)
        {
            string Email = "";
            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("SELECT Email FROM CUSTOMER WHERE CustomerID='" + CustomerID.ToString() + "';", dbconn))
                {
                    if (rs.Read())
                    {
                        Email = DB.RSField(rs, "Email");
                    }
                }
            }
            return Email;
        }

		/// <summary>
		/// This method has been deprecated
		/// </summary>
        public static String GetGoogleEComTrackingV2(Customer ThisCustomer, Boolean isOrderConfirmationAspx)
        {
            if (!AppLogic.AppConfigBool("UseLiveTransactions"))
            {
                return String.Empty;
            }

            StringBuilder tmpS = new StringBuilder(1024);

            try
            {
                if (isOrderConfirmationAspx)
                {
                    if (ThisCustomer == null ||
                        false == CommonLogic.GetThisPageName(false).StartsWith("orderconfirmation.aspx", StringComparison.InvariantCultureIgnoreCase) ||
                        CommonLogic.QueryStringUSInt("OrderNumber") == 0)
                    {
                        return String.Empty;
                    }

                    int OrderNumber = CommonLogic.QueryStringUSInt("OrderNumber");
                    Order ord = new Order(OrderNumber, Localization.GetDefaultLocale());

                    if (ord.AlreadyConfirmed) //Order confirmation page has already been viewed, so return nothing
                    {
                        return String.Empty;
                    }

                    if (ThisCustomer.CustomerID != ord.CustomerID)
                    {
                        return String.Empty;
                    }

                    tmpS.Append("<script type=\"text/javascript\">\n");
                    tmpS.Append("var pageTracker = _gat._getTracker(\"" + AppConfig("Google.AnalyticsAccount") + "\");\n");
                    tmpS.Append("pageTracker._initData();\n");
                    tmpS.Append("pageTracker._trackPageview();\n");
                    tmpS.Append("pageTracker._addTrans(");
                    tmpS.Append(String.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\"",
                        ord.OrderNumber.ToString(), // order ID - required
                        ord.AffiliateName, // affiliation or store name
                        Localization.CurrencyStringForGatewayWithoutExchangeRate(ord.Total(true)), // total - required
                        Localization.CurrencyStringForGatewayWithoutExchangeRate(ord.TaxTotal(true)), // tax
                        Localization.CurrencyStringForGatewayWithoutExchangeRate(ord.ShippingTotal(true)), // shipping
                        ord.BillingAddress.m_City, // city
                        ord.BillingAddress.m_State, // state or province
                        ord.BillingAddress.m_Country // country
                        ));
                    tmpS.Append(");\n");


                    foreach (CartItem c in ord.CartItems)
                    {
                        tmpS.Append("pageTracker._addItem(");
                        tmpS.Append(String.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\"",
                            ord.OrderNumber.ToString(), // order ID - required
                            c.SKU, // SKU/code
                            c.ProductName, // product name
                            AppLogic.GetFirstProductEntity(AppLogic.LookupHelper(EntityDefinitions.readonly_CategoryEntitySpecs.m_EntityName, 0), c.ProductID, false, Localization.GetDefaultLocale()), // category or variation
                            Localization.CurrencyStringForGatewayWithoutExchangeRate(c.Price / c.Quantity), // unit price - required
                            c.Quantity.ToString() // quantity - required
                        ));
                        tmpS.Append(");\n");
                    }

                    tmpS.Append("pageTracker._trackTrans();\n");
                    tmpS.Append("</script>\n");
                }
                else
                {
                    tmpS.Append("<script type=\"text/javascript\">\n");
                    tmpS.Append("var pageTracker = _gat._getTracker(\"" + AppConfig("Google.AnalyticsAccount") + "\");\n");
                    tmpS.Append("pageTracker._initData();\n");
                    tmpS.Append("pageTracker._trackPageview();\n");
                    tmpS.Append("</script>\n");
                }
            }
            catch
            {
                return String.Empty;
            }

            return tmpS.ToString();
        }

		/// <summary>
		/// This method has been deprecated
		/// </summary>
        public static String GetGoogleEComTrackingAsynch(Customer ThisCustomer, Boolean isOrderConfirmationAspx)
        {
            if (!AppLogic.AppConfigBool("UseLiveTransactions"))
            {
                return String.Empty;
            }

            StringBuilder tmpS = new StringBuilder(1024);

            tmpS.Append("<script type=\"text/javascript\">\n");
            tmpS.Append("var _gaq = _gaq || [];\n");
            tmpS.Append("_gaq.push(['_setAccount', '" + AppConfig("Google.AnalyticsAccount") + "']);\n");
            tmpS.Append("_gaq.push(['_trackPageview']);\n");
            
            try
            {
                if (isOrderConfirmationAspx && AppConfigBool("Google.EcomOrderTrackingEnabled"))
                {
                    if (ThisCustomer == null || CommonLogic.QueryStringUSInt("OrderNumber") == 0)
                    {
                        return String.Empty;
                    }

                    int OrderNumber = CommonLogic.QueryStringUSInt("OrderNumber");
                    Order ord = new Order(OrderNumber, Localization.GetDefaultLocale());

                    if (ord.AlreadyConfirmed) //Order confirmation page has already been viewed, so return nothing
                    {
                        return String.Empty;
                    }

                    if (ThisCustomer.CustomerID != ord.CustomerID)
                    {
                        return String.Empty;
                    }

                    tmpS.Append("_gaq.push(['_addTrans',");
                    tmpS.Append("'" + ord.OrderNumber.ToString() + "',");
                    tmpS.Append("'" + AppConfig("StoreName") + "',");
                    tmpS.Append("'" + Localization.CurrencyStringForGatewayWithoutExchangeRate(ord.Total(true)) + "',");
                    tmpS.Append("'" + Localization.CurrencyStringForGatewayWithoutExchangeRate(ord.TaxTotal(true)) + "',");
                    tmpS.Append("'" + Localization.CurrencyStringForGatewayWithoutExchangeRate(ord.ShippingTotal(true)) + "',");
                    tmpS.Append("'" + ord.ShippingAddress.m_City + "',");
                    tmpS.Append("'" + ord.ShippingAddress.m_State + "',");
                    tmpS.Append("'" + ord.ShippingAddress.m_Country + "'");
                    tmpS.Append("]);\n");

                    foreach (CartItem c in ord.CartItems)
                    {
                        tmpS.Append("_gaq.push(['_addItem',");
                        tmpS.Append("'" + ord.OrderNumber.ToString() + "',");
                        tmpS.Append("'" + c.SKU + "',");
                        tmpS.Append("'" + HttpContext.Current.Server.HtmlEncode(c.ProductName) + "',");
                        tmpS.Append("'" + c.ChosenColor + CommonLogic.IIF(c.ChosenColor.Length > 0, " ", "") + c.ChosenSize + "',");
                        tmpS.Append("'" + Localization.CurrencyStringForGatewayWithoutExchangeRate(c.Price / c.Quantity) + "',");
                        tmpS.Append("'" + c.Quantity + "'");
                        tmpS.Append("]);\n");
                    }

                    tmpS.Append("_gaq.push(['_trackTrans']);\n");

                }

                tmpS.Append("(function() {\n");
                tmpS.Append("var ga = document.createElement('script'); ga.type = 'text/javascript'; ga.async = true;\n");
                tmpS.Append("ga.src = ('https:' == document.location.protocol ? 'https://ssl' : 'http://www') + '.google-analytics.com/ga.js';\n");
                tmpS.Append("var s = document.getElementsByTagName('script')[0]; s.parentNode.insertBefore(ga, s);\n");
                tmpS.Append("})();\n");
                tmpS.Append("</script>\n");
            }
            catch(Exception ex)
            {
                SysLog.LogException(ex, MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
                return String.Empty;
            }

            return tmpS.ToString();
        }

        static public String GetCurrentEntityTemplateName(String EntityName)
        {
            return GetCurrentEntityTemplateName(EntityName, 0);
        }

        static public String GetCurrentEntityTemplateName(String EntityName, int UseThisEntityID)
        {
            EntityHelper helper = AppLogic.LookupHelper(EntityName, 0);
            int eID = CommonLogic.QueryStringUSInt(helper.GetEntitySpecs.m_EntityName + "ID");
            if (UseThisEntityID != 0)
            {
                eID = UseThisEntityID;
            }

            XmlNode n = helper.m_TblMgr.SetContext(eID);
            String HT = XmlCommon.XmlField(n, "TemplateName");

            while (HT.Length == 0 && (n = helper.m_TblMgr.MoveParent(n)) != null)
            {
                HT = XmlCommon.XmlField(n, "TemplateName");
            }

            if (HT.Length != 0)
            {
                if (!HT.EndsWith(".master", StringComparison.InvariantCultureIgnoreCase))
                {
                    HT = HT + ".master";
                }
            }
            return HT;
        }

        static public void LoadEntityHelpers()
        {
            CategoryStoreEntityHelper = new Dictionary<int, EntityHelper>();
            SectionStoreEntityHelper = new Dictionary<int, EntityHelper>();
            ManufacturerStoreEntityHelper = new Dictionary<int, EntityHelper>();
            DistributorStoreEntityHelper = new Dictionary<int, EntityHelper>();
            GenreStoreEntityHelper = new Dictionary<int, EntityHelper>();
            VectorStoreEntityHelper = new Dictionary<int, EntityHelper>();
            LibraryStoreEntityHelper = new Dictionary<int, EntityHelper>();

            CategoryStoreEntityHelper[0] = new EntityHelper(0, EntityDefinitions.LookupSpecs("Category"), true, 0);
            SectionStoreEntityHelper[0] = new EntityHelper(0, EntityDefinitions.LookupSpecs("Section"), true, 0);
            ManufacturerStoreEntityHelper[0] = new EntityHelper(0, EntityDefinitions.LookupSpecs("Manufacturer"), true, 0);
            DistributorStoreEntityHelper[0] = new EntityHelper(0, EntityDefinitions.LookupSpecs("Distributor"), true, 0);
            GenreStoreEntityHelper[0] = new EntityHelper(0, EntityDefinitions.LookupSpecs("Genre"), true, 0);
            VectorStoreEntityHelper[0] = new EntityHelper(0, EntityDefinitions.LookupSpecs("Vector"), true, 0);
            LibraryStoreEntityHelper[0] = new EntityHelper(0, EntityDefinitions.LookupSpecs("Library"), true, 0);

            if (AppLogic.GlobalConfigBool("AllowEntityFiltering"))
            {
                foreach (Store s in Store.GetStoreList())
                {
                    CategoryStoreEntityHelper[s.StoreID] = new EntityHelper(0, EntityDefinitions.LookupSpecs("Category"), true, s.StoreID);
                    SectionStoreEntityHelper[s.StoreID] = new EntityHelper(0, EntityDefinitions.LookupSpecs("Section"), true, s.StoreID);
                    ManufacturerStoreEntityHelper[s.StoreID] = new EntityHelper(0, EntityDefinitions.LookupSpecs("Manufacturer"), true, s.StoreID);
                    DistributorStoreEntityHelper[s.StoreID] = new EntityHelper(0, EntityDefinitions.LookupSpecs("Distributor"), true, s.StoreID);
                    GenreStoreEntityHelper[s.StoreID] = new EntityHelper(0, EntityDefinitions.LookupSpecs("Genre"), true, s.StoreID);
                    VectorStoreEntityHelper[s.StoreID] = new EntityHelper(0, EntityDefinitions.LookupSpecs("Vector"), true, s.StoreID);
                    LibraryStoreEntityHelper[s.StoreID] = new EntityHelper(0, EntityDefinitions.LookupSpecs("Library"), true, s.StoreID);
                }
            }
        }

        static public void RequireSecurePage()
        {
            if (AppLogic.UseSSL())
            {
                if (AppLogic.OnLiveServer() && !CommonLogic.IsSecureConnection())
                {
                    HttpContext.Current.Response.Redirect(AppLogic.GetStoreHTTPLocation(true) + CommonLogic.GetThisPageName(false) + "?" + CommonLogic.ServerVariables("QUERY_STRING"));
                }
            }
        }

		static public string GetCurrentPageID()
		{
			var routeData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(HttpContext.Current));
			if (routeData == null)
				return "";

			string entityType = GetCurrentPageType();
			return (routeData.Values[entityType + "ID"] as string) ?? "";
		}

		static public string GetCurrentPageType()
		{
			var handler = HttpContext.Current.CurrentHandler;
			if (handler == null)
				return "";
			
			var handlerType = handler.GetType();
			var pageType = handlerType
				.GetCustomAttributes(typeof(PageTypeAttribute), true)
				.Cast<PageTypeAttribute>()
				.Select(pt => pt.PageType)
				.DefaultIfEmpty(String.Empty)
				.FirstOrDefault();
			return pageType;
		}

        public static String ActivePaymentGatewayRAW()
        {
            return AppLogic.AppConfig("PaymentGateway");
        }

        public static String ActivePaymentGatewayCleaned()
        {
            return AppLogic.CleanPaymentGateway(AppLogic.ActivePaymentGatewayRAW());
        }

        public static bool UseSpecialRecurringIntervals()
        {
            bool UseSpecial = AppLogic.AppConfigBool("Recurring.UseGatewayInternalBilling");
            if (UseSpecial)
            {
                String GWCleaned = AppLogic.ActivePaymentGatewayCleaned();
                if (!(GWCleaned == "VERISIGN" || GWCleaned == "PAYFLOWPRO"))
                {
                    UseSpecial = false;
                }
            }
            return UseSpecial;
        }

        public static bool StoreCCInDB()
        {
            return AppLogic.AppConfigBool("StoreCCInDB");
        }

        public static int DefaultSkinID()
        {
            return AppLogic.GetStoreSkinID(AppLogic.StoreID());
        }

        public static String LiveServer()
        {
            return AppLogic.AppConfig("LiveServer");
        }

        public static String MailServer()
        {
            return AppLogic.AppConfig("MailMe_Server");
        }

        public static String AdminDir()
        {
            return AppLogic.AppConfig("AdminDir");
        }

        public static String HomeTemplate()
        {
            return AppLogic.AppConfig("HomeTemplate");
        }

        public static bool RedirectLiveToWWW()
        {
            return AppLogic.AppConfigBool("RedirectLiveToWWW");
        }

        public static bool EventLoggingEnabled()
        {
            return AppLogic.AppConfigBool("EventLoggingEnabled");
        }

        public static bool HomeTemplateAsIs()
        {
            return AppLogic.AppConfigBool("HomeTemplateAsIs");
        }

        public static bool UseSSL()
        {
            return AppLogic.AppConfigBool("UseSSL");
        }

        public static void NukeCustomer(int CustomerID, bool BanTheirIPAddress)
        {
            AppLogic.eventHandler("NukeCustomer").CallEvent("&NukeCustomer=true");
            if (BanTheirIPAddress)
            {
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();

                    IDataReader rs = DB.GetRS("Select LastIPAddress from Customer where CustomerID=" + CustomerID.ToString(), con);

                    try
                    {
                        if (rs.Read())
                        {
                            // ignore duplicates:
                            if (DB.RSField(rs, "LastIPAddress").Length != 0)
                            {
                                DB.ExecuteSQL("insert RestrictedIP(IPAddress) values(" + DB.SQuote(DB.RSField(rs, "LastIPAddress")) + ")");
                            }
                        }
                    }
                    catch { }
                    finally
                    {
                        rs.Dispose();
                    }
                }
            }

            // remove any download folders also:
            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rso = DB.GetRS("Select ordernumber from orders   with (NOLOCK)  where CustomerID=" + CustomerID.ToString(), con))
                {
                    while (rso.Read())
                    {
                        if (!DB.RSFieldDateTime(rso, "DownloadEMailSentOn").Equals(System.DateTime.MinValue))
                        {
                            String dirname = DB.RSFieldInt(rso, "OrderNumber").ToString() + "_" + CustomerID.ToString().ToString();
                            try
                            {
                                System.IO.Directory.Delete(CommonLogic.SafeMapPath("../orderdownloads/" + dirname), true);
                            }
                            catch { }
                        }
                    }
                }
            }

            DB.ExecuteSQL("delete from promotionusage where CustomerID=" + CustomerID.ToString());
            DB.ExecuteSQL("delete from couponusage where CustomerID=" + CustomerID.ToString());
            DB.ExecuteSQL("delete from orders_kitcart where CustomerID=" + CustomerID.ToString());
            DB.ExecuteSQL("delete from orders_ShoppingCart where CustomerID=" + CustomerID.ToString());
            DB.ExecuteSQL("delete from orders where CustomerID=" + CustomerID.ToString());
            DB.ExecuteSQL("delete from ShoppingCart where CustomerID=" + CustomerID.ToString());
            DB.ExecuteSQL("delete from failedtransaction where CustomerID=" + CustomerID.ToString());
            DB.ExecuteSQL("delete from kitcart where CustomerID=" + CustomerID.ToString());
            DB.ExecuteSQL("delete from pollvotingrecord where CustomerID=" + CustomerID.ToString());
            DB.ExecuteSQL("delete from ratingcommenthelpfulness where RatingCustomerID=" + CustomerID.ToString() + " or VotingCustomerID=" + CustomerID.ToString());
            DB.ExecuteSQL("delete from rating where CustomerID=" + CustomerID.ToString());
			DB.ExecuteSQL("delete from CIM_AddressPaymentProfileMap where CustomerID=" + CustomerID.ToString());
			DB.ExecuteSQL("delete from Address where CustomerID=" + CustomerID.ToString());
            DB.ExecuteSQL("delete from customer where CustomerID=" + CustomerID.ToString());
        }

        static public bool VATRegistrationIDIsValid(Customer ThisCustomer, String RegID, out Exception exception)
        {
            String Ctry = ThisCustomer.PrimaryBillingAddress.Country;
            Ctry = DB.GetSqlS("select TwoLetterISOCode S FROM DBO.COUNTRY WHERE Name = " + DB.SQuote(Ctry));
            return VATRegistrationIDIsValid(Ctry, RegID, out exception);
        }

        /// <summary>
        /// Checks the VAT registration ID
        /// </summary>
        /// <param name="Country">The full country name as specified in the Country table</param>
        /// <param name="RegID">Customer VAT registration ID</param>
        /// <returns></returns>
		static public bool VATRegistrationIDIsValid (string Country, String RegID, out Exception exception)
        {
            VATCheck.checkVatService vt = new VATCheck.checkVatService();
            if (!String.IsNullOrEmpty(AppLogic.AppConfig("VAT.VATCheckServiceURL")))
                vt.Url = AppLogic.AppConfig("VAT.VATCheckServiceURL");

			exception = null;

            bool IsValid = false;
            String sName = String.Empty;
            String sAddress = String.Empty;
            RegID = RegID.Replace(" ", "").Trim();
            if (Country.Length != 2)
            {
                Country = DB.GetSqlS("select TwoLetterISOCode S FROM DBO.COUNTRY WHERE Name = " + DB.SQuote(Country));
            }
            try
            {
                vt.checkVat(ref Country, ref RegID, out IsValid, out sName, out sAddress);
            }
            catch (Exception e)
			{
				exception = e;
			}

            return IsValid;
        }

        static public void CheckForScriptTag(String s)
        {
            if (s.Replace(" ", "").IndexOf("<script", StringComparison.InvariantCultureIgnoreCase) != -1)
            {
                throw new ArgumentException("SECURITY EXCEPTION");
            }
        }

        static public bool IsAdminSite
        {
            get
            {
                bool ia = (String)HttpContext.Current.Items["IsAdminSite"] == "true";
                return ia;
            }
        }

        // This is expensive to call, do not call it unless you absolutely have to:
        static public AspNetHostingPermissionLevel DetermineTrustLevel()
        {
            foreach (AspNetHostingPermissionLevel trustLevel in
                    new AspNetHostingPermissionLevel[] {
                AspNetHostingPermissionLevel.Unrestricted,
                AspNetHostingPermissionLevel.High,
                AspNetHostingPermissionLevel.Medium,
                AspNetHostingPermissionLevel.Low,
                AspNetHostingPermissionLevel.Minimal 
            })
            {
                try
                {
                    new AspNetHostingPermission(trustLevel).Demand();
                }
                catch (System.Security.SecurityException)
                {
                    continue;
                }

                return trustLevel;
            }

            return AspNetHostingPermissionLevel.None;
        }

        static public bool IPIsRestricted(String IPAddress)
        {
            if (IPAddress.Length == 0)
            {
                return false;
            }
            return (DB.GetSqlN("select count(*) as N from RestrictedIP where IPAddress=" + DB.SQuote(IPAddress)) > 0);
        }

        static public bool ExceedsFailedTransactionsThreshold(Customer ThisCustomer)
        {
            int MaxAllowed = AppLogic.AppConfigUSInt("IPAddress.MaxFailedTransactions");
            if (MaxAllowed == 0)
            {
                MaxAllowed = 5;
            }
            int NumFailedTransactions = DB.GetSqlN("select count(*) as N from FailedTransaction where orderdate>dateadd(hh,-1,getdate()) and IPAddress=" + DB.SQuote(ThisCustomer.LastIPAddress));
            return NumFailedTransactions > MaxAllowed;
        }

        // crash the page if not!!
        // prevent hacking of the querystring by hacks trying to force their own payment method in here (e.g. PayPal)
        public static void ValidatePM(String PM)
        {
            PM = AppLogic.CleanPaymentMethod(PM);

            if (PM == AppLogic.ro_PMPayPalExpressMark)
            {
                PM = AppLogic.ro_PMPayPalExpress;
            }

            if (PM.Length != 0)
            {
                Boolean OkToProceed = false;
                foreach (String s in AppLogic.AppConfig("PaymentMethods").Split(','))
                {
                    String s2 = AppLogic.CleanPaymentMethod(s);
                    if (s2 == PM)
                    {
                        OkToProceed = true;
                    }
                }
                if (AppLogic.MicropayIsEnabled() && PM == AppLogic.ro_PMMicropay)
                {
                    OkToProceed = true;
                }
                if (PM.IndexOf("GOOGLE") != -1 || PM.IndexOf("BYPASS") != -1)
                {
                    OkToProceed = true;
                }
                if (PM == AppLogic.ro_PMPayPalExpress && AppLogic.ActivePaymentGatewayCleaned() == "PAYPALPRO")
                {
                    OkToProceed = true;
                }
                if (AppLogic.SecureNetVaultIsEnabled() && PM == AppLogic.ro_PMSecureNetVault)
                {
                    OkToProceed = true;
                }
                if (!OkToProceed)
                {
                    throw new ArgumentException("SECURITY EXCEPTION");
                }
            }
        }

        public static Boolean SecureNetVaultIsEnabled()
        {
            return AppLogic.AppConfigBool("SecureNetV4.VaultEnabled")
                && AppLogic.AppConfig("PaymentGateway").EqualsIgnoreCase("SECURENETV4");
        }

        public static bool VATIsEnabled()
        {
                return AppLogic.AppConfigBool("VAT.Enabled");
            }

        public static System.Collections.Generic.Dictionary<string, EntityHelper> MakeEntityHelpers()
        {
            System.Collections.Generic.Dictionary<string, EntityHelper> m_EntityHelpers = new System.Collections.Generic.Dictionary<string, EntityHelper>();
            m_EntityHelpers.Add("Category", LookupHelper("Category", 0));
            m_EntityHelpers.Add("Section", LookupHelper("Section", 0));
            m_EntityHelpers.Add("Manufacturer", LookupHelper("Manufacturer", 0));
                m_EntityHelpers.Add("Distributor", LookupHelper("Distributor", 0));
                m_EntityHelpers.Add("Genre", LookupHelper("Genre", 0));
                m_EntityHelpers.Add("Vector", LookupHelper("Vector", 0));
                m_EntityHelpers.Add("Library", LookupHelper("Library", 0));
            
            return m_EntityHelpers;
        }


        public static String GetLocaleDefaultCurrency(String LocaleSetting)
        {
            int tmp = 0;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select DefaultCurrencyID from LocaleSetting  with (NOLOCK)  where Name=" + DB.SQuote(LocaleSetting), con))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSFieldInt(rs, "DefaultCurrencyID");
                    }
                }
            }

            if (tmp == 0)
            {
                return Localization.GetPrimaryCurrency();
            }
            else
            {
                return Currency.GetCurrencyCode(tmp);
            }
        }

        // just removes all <....> markeup from the text string. this is brute force, and may or may not give
        // the right aesthetic result to the text. it just brute force removes the markeup tags
        public static string StripHtml(String s)
        {
            return Regex.Replace(s, @"<(.|\n)*?>", string.Empty, RegexOptions.Compiled);
        }

        // input CardNumber can be in plain text or encrypted, doesn't matter:
        public static String SafeDisplayCardNumber(String CardNumber, String Table, int TableID)
        {
            if (CardNumber == null || CardNumber.Length == 0)
            {
                return String.Empty;
            }
            String SaltKey = String.Empty;
            if (Table == "Address")
            {
                SaltKey = Address.StaticGetSaltKey(TableID);
            }
            else
            {
                SaltKey = Order.StaticGetSaltKey(TableID);
            }
            String CardNumberDecrypt = Security.UnmungeString(CardNumber, SaltKey);
            if (CardNumberDecrypt.StartsWith(Security.ro_DecryptFailedPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                CardNumberDecrypt = CardNumber;
            }
            if (CardNumberDecrypt == AppLogic.ro_CCNotStoredString)
            {
                return String.Empty;
            }
            if (CardNumberDecrypt.Length > 4)
            {
                return "****" + CardNumberDecrypt.Substring(CardNumberDecrypt.Length - 4, 4);
            }
            else
            {
                return String.Empty;
            }
        }

        // input CardExtraCode can be in plain text or encrypted, doesn't matter:
        public static String SafeDisplayCardExtraCode(String CardExtraCode)
        {
            if (CardExtraCode == null || CardExtraCode.Length == 0)
            {
                return String.Empty;
            }
            String CardExtraCodeDecrypt = Security.UnmungeString(CardExtraCode);
            if (CardExtraCodeDecrypt.StartsWith(Security.ro_DecryptFailedPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                CardExtraCodeDecrypt = CardExtraCode;
            }
            return "*".PadLeft(CardExtraCodeDecrypt.Length, '*');
        }

        // returns empty string, or decrypted card extra code from appropriate session state location
        public static String GetCardExtraCodeFromSession(Customer ThisCustomer)
        {
            String CardExtraCode = Security.UnmungeString(ThisCustomer.ThisCustomerSession["CardExtraCode"]);
            return CardExtraCode;
        }

        // stores cardextracode in appropriate session, encrypted
        public static void StoreCardExtraCodeInSession(Customer ThisCustomer, String CardExtraCode)
        {
            ThisCustomer.ThisCustomerSession["CardExtraCode"] = Security.MungeString(CardExtraCode);
        }

        public static void ClearCardExtraCodeInSession(Customer ThisCustomer)
        {
            // wipe from both sessions, it was only in one of them anyway, but wipe both for safety:
            ThisCustomer.ThisCustomerSession["CardExtraCode"] = "111111111";
            ThisCustomer.ThisCustomerSession["CardExtraCode"] = String.Empty;
        }


        // returns empty string, or decrypted card extra code from appropriate session state location
        public static String GetSelectedSecureNetVault(Customer ThisCustomer)
        {
            return Security.UnmungeString(ThisCustomer.ThisCustomerSession["SelectedSecureNetVault"]);
        }

        // stores cardextracode in appropriate session, encrypted
        public static void StoreSelectedSecureNetVault(Customer ThisCustomer, String SelectedSecureNetVault)
        {
            ThisCustomer.ThisCustomerSession["SelectedSecureNetVault"] = Security.MungeString(SelectedSecureNetVault);
        }

        public static void ClearSelectedSecureNetVaultInSession(Customer ThisCustomer)
        {
            // wipe from both sessions, it was only in one of them anyway, but wipe both for safety:
            ThisCustomer.ThisCustomerSession["SelectedSecureNetVault"] = "111111111";
            ThisCustomer.ThisCustomerSession["SelectedSecureNetVault"] = String.Empty;
        }

        // input CardNumber can be in plain text or encrypted, doesn't matter:
        public static String AdminViewCardNumber(String CardNumber, String Table, int TableID)
        {
            if (CardNumber.Length == 0 || CardNumber == AppLogic.ro_CCNotStoredString)
            {
                return CardNumber;
            }
            String SaltKey = String.Empty;
            if (Table == "Address")
            {
                SaltKey = Address.StaticGetSaltKey(TableID);
            }
            else
            {
                SaltKey = Order.StaticGetSaltKey(TableID);
            }

            String CardNumberDecrypt = Security.UnmungeString(CardNumber, SaltKey);
            if (CardNumberDecrypt.StartsWith(Security.ro_DecryptFailedPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                CardNumberDecrypt = CardNumber;
            }
            if (AppLogic.IsAdminSite)
            {
                return CardNumberDecrypt;
            }
            else
            {
                return SafeDisplayCardNumber(CardNumber, Table, TableID);
            }
        }

        // input CardNumber can be in plain text or encrypted, doesn't matter:
        public static String SafeDisplayCardNumberLast4(String CardNumber, String Table, int TableID)
        {
            String SaltKey = String.Empty;
            if (Table == "Address")
            {
                SaltKey = Address.StaticGetSaltKey(TableID);
            }
            else
            {
                SaltKey = Order.StaticGetSaltKey(TableID);
            }
            String CardNumberDecrypt = Security.UnmungeString(CardNumber, SaltKey);
            if (CardNumberDecrypt.StartsWith(Security.ro_DecryptFailedPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                CardNumberDecrypt = CardNumber;
            }
            if (CardNumberDecrypt == AppLogic.ro_CCNotStoredString)
            {
                return String.Empty;
            }
            if (CardNumberDecrypt.Length >= 4)
            {
                return CardNumberDecrypt.Substring(CardNumberDecrypt.Length - 4, 4);
            }
            else
            {
                return String.Empty;
            }
        }


        public static bool ProductHasVisibleBuyButton(int ProductID)
        {
            bool tmp = true;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select * from Product  with (NOLOCK)  where ProductID=" + ProductID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSFieldBool(rs, "ShowBuyButton") && !DB.RSFieldBool(rs, "IsCallToOrder");
                    }
                }
            }

            return tmp;
        }

        public static bool VariantAllowsCustomerPricing(int VariantID)
        {
            bool tmp = false;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select CustomerEntersPrice from ProductVariant  with (NOLOCK)  where VariantID=" + VariantID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSFieldBool(rs, "CustomerEntersPrice");
                    }
                }
            }

            return tmp;
        }


        public static bool ObjectIsMapToStore(string type, int id, int storeid)
        {
            List<Store> stores = Store.GetStoreList();
            Store thisStore = stores.Find(s => s.StoreID == storeid);
            return thisStore.IsMapped(type, id);
        }

        public static string ExportProductList(int categoryid, int sectionid, int manufacturerid, int distributorid, int genreid, int vectorid)
        {
            SqlConnection dbconn = new SqlConnection();
            dbconn.ConnectionString = DB.GetDBConn();
            dbconn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = dbconn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "aspdnsf_ExportProductList";

            cmd.Parameters.Add(new SqlParameter("@categoryID", SqlDbType.Int));
            cmd.Parameters.Add(new SqlParameter("@sectionID", SqlDbType.Int));
            cmd.Parameters.Add(new SqlParameter("@manufacturerID", SqlDbType.Int));
            cmd.Parameters.Add(new SqlParameter("@distributorID", SqlDbType.Int));
            cmd.Parameters.Add(new SqlParameter("@genreID", SqlDbType.Int));
            cmd.Parameters.Add(new SqlParameter("@vectorID", SqlDbType.Int));

            cmd.Parameters["@categoryID"].Value = categoryid;
            cmd.Parameters["@sectionID"].Value = sectionid;
            cmd.Parameters["@manufacturerID"].Value = manufacturerid;
            cmd.Parameters["@distributorID"].Value = distributorid;
            cmd.Parameters["@genreID"].Value = genreid;
            cmd.Parameters["@vectorID"].Value = vectorid;

            SqlDataReader dr = cmd.ExecuteReader();
            StringBuilder tmpS = new StringBuilder(1024);
            int n = DB.GetENLocaleXml(dr, "root", "product", ref tmpS);
            dr.Close();

            return tmpS.ToString();
        }


        public static string ImportProductList(string importtext)
        {
            if (importtext.Trim() == String.Empty)
            {
                return "No data to import";
            }

            SqlConnection dbconn = new SqlConnection();
            dbconn.ConnectionString = DB.GetDBConn();
            dbconn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = dbconn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "aspdnsf_ImportProductPricing_XML";

            cmd.Parameters.Add(new SqlParameter("@pricing", SqlDbType.NText));

            cmd.Parameters["@pricing"].Value = importtext;

            try
            {
                cmd.ExecuteNonQuery();
                dbconn.Close();
                return String.Empty;
            }
            catch (Exception ex)
            {
                dbconn.Close();
                return ex.Message;
            }

        }
        static public int MaxMenuSize()
        {
            int tmp = AppLogic.AppConfigUSInt("MaxMenuSize");
            if (tmp == 0)
            {
                tmp = 25;
            }
            return tmp;
        }

        // returns PM in all uppercase with only primary chars included
        static public String CleanPaymentMethod(String PM)
        {
            return PM.Replace(" ", String.Empty).Replace(".", String.Empty).Replace("(", String.Empty).Replace(")", String.Empty).Trim().ToUpperInvariant();
        }

        // returns PM in all uppercase with only primary chars included
        static public String CleanPaymentGateway(String GW)
        {
            return GW.Replace(" ", String.Empty).Replace(".", String.Empty).Replace("(", String.Empty).Replace(")", String.Empty).Trim().ToUpperInvariant();
        }

        static public bool ProductCanBeAddedToGiftRegistry(int VariantID)
        {
            bool tmp = false;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select IsDownload,IsRecurring from ProductVariant  with (NOLOCK)  where VariantID=" + VariantID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmp = !DB.RSFieldBool(rs, "IsDownload") && !DB.RSFieldBool(rs, "IsRecurring");
                    }
                }
            }

            return tmp;
        }

        static public String GiftRegistryDisplayName(int GiftRegistryCustomerID, bool IncludeAddressIfAllowed, int SkinID, String LocaleSetting)
        {
            String DisplayName = String.Empty;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select FirstName,LastName,GiftRegistryIsAnonymous,GiftRegistryNickName,GiftRegistryHideShippingAddresses,ShippingAddressID from Customer where CustomerID=" + GiftRegistryCustomerID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        if (IncludeAddressIfAllowed && !DB.RSFieldBool(rs, "GiftRegistryIsAnonymous") && !DB.RSFieldBool(rs, "GiftRegistryHideShippingAddresses"))
                        {
                            Address adr = new Address();
                            DisplayName = adr.DisplayHTML(true);
                        }
                        else
                        {
                            DisplayName = (DB.RSField(rs, "FirstName") + " " + DB.RSField(rs, "LastName")).Trim();
                            if (DB.RSFieldBool(rs, "GiftRegistryIsAnonymous"))
                            {
                                DisplayName = DB.RSField(rs, "GiftRegistryNickName");
                            }
                        }
                    }
                    if (DisplayName.Length == 0)
                    {
                        DisplayName = AppLogic.GetString("giftregistry.aspx.15", SkinID, LocaleSetting);
                    }
                }
            }

            return DisplayName;
        }

        static public int GiftRegistryOwnerID(String GiftRegistryGUID)
        {
            int tmp = 0;

            if (GiftRegistryGUID.Length != 0)
            {
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS("select CustomerID from Customer where GiftRegistryGUID=" + DB.SQuote(GiftRegistryGUID) + " and Deleted=0", con))
                    {
                        if (rs.Read())
                        {
                            tmp = DB.RSFieldInt(rs, "CustomerID");
                        }
                    }
                }
            }

            return tmp;
        }

        static public int GiftRegistryShippingAddressID(int GiftRegistryCustomerID)
        {
            int tmp = 0;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select ShippingAddressID from Customer where CustomerID=" + GiftRegistryCustomerID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSFieldInt(rs, "ShippingAddressID");
                    }
                }
            }

            return tmp;
        }

        // returns comma separate list of skin id's found on the web site, e.g. 1,2,3,4
        static public String FindAllSkins()
        {
            String CacheName = "FindAllSkins";
            if (AppLogic.CachingOn)
            {
                String s = (String)HttpContext.Current.Cache.Get(CacheName);
                if (s != null && s.Trim().Length > 0)
                {
                    return s;
                }
            }
            StringBuilder tmpS = new StringBuilder(1024);
            int MaxNumberSkins = AppLogic.AppConfigUSInt("MaxNumberSkins");
            if (MaxNumberSkins == 0)
            {
                MaxNumberSkins = 10;
            }
            for (int i = 0; i <= 100; i++)
            {
                String FN = CommonLogic.SafeMapPath(CommonLogic.IIF(AppLogic.IsAdminSite, "../", String.Empty) + "App_Templates/skin_" + i.ToString() + "/template.master");
                if (CommonLogic.FileExists(FN))
                {
                    if (tmpS.Length != 0)
                    {
                        tmpS.Append(",");
                    }
                    tmpS.Append(i.ToString());
                }
            }

            if (CachingOn)
            {
                HttpContext.Current.Cache.Insert(CacheName, tmpS.ToString(), null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }
            return tmpS.ToString();
        }

        static public EntityHelper LookupHelper(System.Collections.Generic.Dictionary<string, EntityHelper> EntityHelpers, String EntityName)
        {
            String en = EntityName.Substring(0, 1).ToUpperInvariant() + EntityName.Substring(1, EntityName.Length - 1).ToLowerInvariant();
            EntityHelper h = null;
            if (EntityHelpers == null)
            {
                h = new EntityHelper(EntityDefinitions.LookupSpecs(en), 0);
            }
            else
            {
                if (!EntityHelpers.ContainsKey(en))
                {
                    h = new EntityHelper(EntityDefinitions.LookupSpecs(en), 0);
                }
                else
                {
                    h = (EntityHelper)EntityHelpers[en];
                }
            }
            return h;
        }

        static public EntityHelper LookupHelper(String EntityName, int StoreID)
        {
            EntityHelper h = null;
            Dictionary<int, EntityHelper> genericEntityDictionary;

            switch (EntityName.ToUpperInvariant())
            {
                case "CATEGORY":
                    genericEntityDictionary = CategoryStoreEntityHelper;
                    break;
                case "SECTION":
                    genericEntityDictionary = SectionStoreEntityHelper;
                    break;
                case "MANUFACTURER":
                    genericEntityDictionary = ManufacturerStoreEntityHelper;
                    break;
                case "DISTRIBUTOR":
                    genericEntityDictionary = DistributorStoreEntityHelper;
                    break;
                case "GENRE":
                    genericEntityDictionary = GenreStoreEntityHelper;
                    break;
                case "VECTOR":
                    genericEntityDictionary = VectorStoreEntityHelper;
                    break;
                case "LIBRARY":
                    genericEntityDictionary = LibraryStoreEntityHelper;
                    break;
                default:
                    h = new EntityHelper(EntityDefinitions.LookupSpecs(EntityName), StoreID);
                    return h;
            }
            if (genericEntityDictionary.ContainsKey(StoreID))
            {
                h = genericEntityDictionary[StoreID];
            }
            else
            {
                h = genericEntityDictionary[0];
            }
            return h;

        }

        static public String GetCheckoutTermsAndConditions(int SkinID, String LocaleSetting, Parser UseParser, bool InitialStateIsChecked)
        {
            String CacheName = String.Format("GetCheckoutTermsAndConditions_{0}_{1}", SkinID.ToString(), LocaleSetting);
            if (AppLogic.CachingOn)
            {
                String s = (String)HttpContext.Current.Cache.Get(CacheName);
                if (s != null)
                {
                    return s;
                }
            }

            StringBuilder tmpS = new StringBuilder(4096);
            tmpS.Append("<div class=\"form terms-form\">");
            tmpS.Append("	<div class=\"form-group\">");
            tmpS.Append("		<input type=\"checkbox\" value=\"true\" id=\"TermsAndConditionsRead\" name=\"TermsAndConditionsRead\" " + CommonLogic.IIF(InitialStateIsChecked, " checked ", String.Empty) + ">&nbsp;");
            tmpS.Append(AppLogic.GetString("checkoutcard.aspx.14", SkinID, LocaleSetting));
            tmpS.Append("		<div class=\"form-text\">");
            Topic t = new Topic("checkouttermsandconditions", LocaleSetting, SkinID, UseParser);
            tmpS.Append(t.Contents);
            tmpS.Append("		</div>");
            tmpS.Append("	</div>");
            tmpS.Append("</div>\n");

            if (CachingOn)
            {
                HttpContext.Current.Cache.Insert(CacheName, tmpS.ToString(), null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }
            return tmpS.ToString();
        }

        static public String RunXmlPackage(String XmlPackageName, Parser UseParser, Customer ThisCustomer, int SkinID, String RunTimeQuery, String RunTimeParms, bool ReplaceTokens, bool WriteExceptionMessage)
        {
            try
            {
                String pn = XmlPackageName;
                if (!pn.EndsWith(".xml.config", StringComparison.InvariantCultureIgnoreCase))
                {
                    pn += ".xml.config";
                }
                using (XmlPackage2 p = new XmlPackage2(XmlPackageName, ThisCustomer, SkinID, RunTimeQuery, RunTimeParms, null))
                {
                    return RunXmlPackage(p, UseParser, ThisCustomer, SkinID, ReplaceTokens, WriteExceptionMessage);
                }
            }
            catch (Exception ex)
            {
                return CommonLogic.GetExceptionDetail(ex, "");
            }
        }

        static public String RunXmlPackage(XmlPackage2 p, Parser UseParser, Customer ThisCustomer, int SkinID, bool ReplaceTokens, bool WriteExceptionMessage)
        {
            StringBuilder tmpS = new StringBuilder(10000);
            try
            {
                if (p != null)
                {
                    String XmlPackageName = p.Name;
                    if (CommonLogic.ApplicationBool("DumpSQL") && !p.Name.Equals("page.menu.xml.config"))
                    {
                        tmpS.Append("<p><b>XmlPackage: " + XmlPackageName + "</b></p>");
                    }
                    try
                    {
                        String s = p.TransformString();
                        if (ReplaceTokens && p.RequiresParser)
                        {
                            if (UseParser == null)
                            {
                                UseParser = new Parser(SkinID, ThisCustomer);
                            }
                            tmpS.Append(UseParser.ReplaceTokens(s));
                        }
                        else
                        {
                            tmpS.Append(s);
                        }
                    }
                    catch (Exception ex)
                    {
                        tmpS.Append("XmlPackage Exception: " + CommonLogic.GetExceptionDetail(ex, "") + ex.ToString() + "");
                    }
                    if ((AppLogic.AppConfigBool("XmlPackage.DumpTransform") || p.IsDebug) && !p.Name.EqualsIgnoreCase("page.menu.xml.config"))
                    {
                        tmpS.Append("<div>");
                        tmpS.Append("<b>" + p.URL + "");
                        tmpS.Append("<textarea READONLY cols=\"80\" rows=\"50\">" + XmlCommon.XmlEncode(XmlCommon.PrettyPrintXml(p.PackageDocument.InnerXml)) + "</textarea>");
                        tmpS.Append("</div>");

                        tmpS.Append("<div>");
                        tmpS.Append("<b>" + XmlPackageName + "_store.runtime.xml");
                        //tmpS.Append("<textarea READONLY cols=\"80\" rows=\"50\">" + XmlCommon.XmlEncode(CommonLogic.ReadFile(CommonLogic.IIF(AppLogic.IsAdminSite, "../", String.Empty) + "images/" + XmlPackageName + "_" + CommonLogic.IIF(AppLogic.IsAdminSite, "admin", "store") + ".runtime.xml", true)) + "</textarea>");
                        tmpS.Append("<textarea READONLY cols=\"80\" rows=\"50\">" + XmlCommon.XmlEncode(CommonLogic.ReadFile(CommonLogic.IIF(AppLogic.IsAdminSite, "~/{0}".FormatWith(AppLogic.AdminDir()), "~/") + "images/" + XmlPackageName + "_" + CommonLogic.IIF(AppLogic.IsAdminSite, "admin", "store") + ".runtime.xml", true)) + "</textarea>");
                        tmpS.Append("</div>");

                        tmpS.Append("<div>");
                        tmpS.Append("<b>" + XmlPackageName + "_store.runtime.sql");
                        //tmpS.Append("<textarea READONLY cols=\"80\" rows=\"50\">" + XmlCommon.XmlEncode(CommonLogic.ReadFile(CommonLogic.IIF(AppLogic.IsAdminSite, "../", String.Empty) + "images/" + XmlPackageName + "_" + CommonLogic.IIF(AppLogic.IsAdminSite, "admin", "store") + ".runtime.sql", true)) + "</textarea>");
                        tmpS.Append("<textarea READONLY cols=\"80\" rows=\"50\">" + XmlCommon.XmlEncode(CommonLogic.ReadFile(CommonLogic.IIF(AppLogic.IsAdminSite, "~/{0}".FormatWith(AppLogic.AdminDir()), "~/") + "images/" + XmlPackageName + "_" + CommonLogic.IIF(AppLogic.IsAdminSite, "admin", "store") + ".runtime.sql", true)) + "</textarea>");
                        tmpS.Append("</div>");

                        tmpS.Append("<div>");
                        tmpS.Append("<b>" + XmlPackageName + "_store.xfrm.xml");
                        //tmpS.Append("<textarea READONLY cols=\"80\" rows=\"50\">" + XmlCommon.XmlEncode(CommonLogic.ReadFile(CommonLogic.IIF(AppLogic.IsAdminSite, "../", String.Empty) + "images/" + XmlPackageName + "_" + CommonLogic.IIF(AppLogic.IsAdminSite, "admin", "store") + ".xfrm.xml", true)) + "</textarea>");
                        tmpS.Append("<textarea READONLY cols=\"80\" rows=\"50\">" + XmlCommon.XmlEncode(CommonLogic.ReadFile(CommonLogic.IIF(AppLogic.IsAdminSite, "~/{0}".FormatWith(AppLogic.AdminDir()), "~/") + "images/" + XmlPackageName + "_" + CommonLogic.IIF(AppLogic.IsAdminSite, "admin", "store") + ".xfrm.xml", true)) + "</textarea>");
                        tmpS.Append("</div>");
                    }
                }
                else
                {
                    tmpS.Append("XmlPackage2 parameter is null");
                }
            }
            catch (Exception ex)
            {
                tmpS.Append(CommonLogic.GetExceptionDetail(ex, ""));
            }
            return tmpS.ToString();
        }

        static public String RunXmlPackage(String XmlPackageName, Parser UseParser, Customer ThisCustomer, int SkinID, String RunTimeQuery, String RunTimeParms, bool ReplaceTokens, bool WriteExceptionMessage, System.Collections.Generic.Dictionary<string, EntityHelper> EntityHelpers)
        {
            try
            {
                String pn = XmlPackageName;
                if (!pn.EndsWith(".xml.config", StringComparison.InvariantCultureIgnoreCase))
                {
                    pn += ".xml.config";
                }
                using (XmlPackage2 p = new XmlPackage2(XmlPackageName, ThisCustomer, SkinID, RunTimeQuery, RunTimeParms, String.Empty, true))
                {
                    return RunXmlPackage(p, UseParser, ThisCustomer, SkinID, ReplaceTokens, WriteExceptionMessage);
                }
            }
            catch (Exception ex)
            {
                return CommonLogic.GetExceptionDetail(ex, "");
            }

        }

        static public ArrayList ReadXmlPackages(String PackageFilePrefix, int SkinID)
        {
            // create an array to hold the list of files
            ArrayList fArray = new ArrayList();

            // check skin area:
            String SFP = CommonLogic.SafeMapPath(CommonLogic.IIF(AppLogic.IsAdminSite, "../", String.Empty) + "App_Templates/skin_" + SkinID.ToString() + "/XmlPackages/bogus.htm").Replace("bogus.htm", String.Empty);
            DirectoryInfo dirInfo = new DirectoryInfo(SFP);
            FileSystemInfo[] myDir = dirInfo.GetFiles(CommonLogic.IIF(PackageFilePrefix.Length != 0, PackageFilePrefix + ".*.xml.config", "*.xml.config"));
            for (int i = 0; i < myDir.Length; i++)
            {
                fArray.Add(myDir[i].ToString().ToLowerInvariant());
            }


            // now check common area:
            SFP = CommonLogic.SafeMapPath(CommonLogic.IIF(AppLogic.IsAdminSite, "../", String.Empty) + "XmlPackages/bogus.htm").Replace("bogus.htm", String.Empty);
            dirInfo = new DirectoryInfo(SFP);
            myDir = dirInfo.GetFiles(CommonLogic.IIF(PackageFilePrefix.Length != 0, PackageFilePrefix + ".*.xml.config", "*.xml.config"));
            for (int i = 0; i < myDir.Length; i++)
            {
                fArray.Add(myDir[i].ToString().ToLowerInvariant());
            }

            if (fArray.Count != 0)
            {
                // sort the files alphabetically
                fArray.Sort(0, fArray.Count, null);
            }
            return fArray;
        }

        static public ArrayList ReadAdminXmlPackages(String PackageFilePrefix)
        {
            // create an array to hold the list of files
            ArrayList fArray = new ArrayList();

            // now check common area:
            String SFP = CommonLogic.SafeMapPath(CommonLogic.IIF(AppLogic.IsAdminSite, String.Empty, "admin/") + "XmlPackages/bogus.htm").Replace("bogus.htm", String.Empty);
            DirectoryInfo dirInfo = new DirectoryInfo(SFP);
            FileSystemInfo[]  myDir = dirInfo.GetFiles(CommonLogic.IIF(PackageFilePrefix.Length != 0, PackageFilePrefix + ".*.xml.config", "*.xml.config"));
            for (int i = 0; i < myDir.Length; i++)
            {
                fArray.Add(myDir[i].ToString().ToLowerInvariant());
            }

            if (fArray.Count != 0)
            {
                // sort the files alphabetically
                fArray.Sort(0, fArray.Count, null);
            }
            return fArray;
        }

        static public void EnsureProductHasADefaultVariantSet(int ProductID)
        {
            if (DB.GetSqlN("select count(VariantID) as N from ProductVariant where Deleted=0 and ProductID=" + ProductID.ToString() + " and IsDefault=1") == 0)
            {
                // force a default variant, none was specified!
                DB.ExecuteSQL("update ProductVariant set IsDefault=1 where Deleted=0 and ProductID=" + ProductID.ToString() + " and VariantID in (SELECT top 1 VariantID from ProductVariant where Deleted=0 and Published=1 and ProductID=" + ProductID.ToString() + " order by DisplayOrder,Name)");
            }
            DB.ExecuteSQL("update ProductVariant set IsDefault=0 where Deleted=1 and ProductID=" + ProductID.ToString());
        }

        /// <summary>
        /// Ensures that the product has at least one variant and if not adds one
        /// </summary>
        /// <param name="ProductID">The product ID of the product being evaluated.</param>
        static public void MakeSureProductHasAtLeastOneVariant(int ProductID)
        {
            if (DB.GetSqlN("select count(VariantID) as N from ProductVariant where Deleted=0 and ProductID=" + ProductID.ToString()) == 0)
            {
                // CREATE AN EMPTY VARIANT
                String NewGUID = DB.GetNewGUID();
                StringBuilder sql = new StringBuilder(1024);
                sql.Append("insert into productvariant(VariantGUID,Name,IsDefault,ProductID,Price,Inventory,Published) values(");
                sql.Append(DB.SQuote(NewGUID) + ",");
                sql.Append(DB.SQuote(String.Empty) + ","); // add empty variant name
                sql.Append("1,"); // IsDefault=1
                sql.Append(ProductID.ToString() + ",");
                sql.Append(Localization.CurrencyStringForDBWithoutExchangeRate(System.Decimal.Zero) + ",");
                sql.Append(CommonLogic.IIF(CommonLogic.IsInteger(AppLogic.AppConfig("Admin_DefaultInventory")), AppLogic.AppConfig("Admin_DefaultInventory"), "100000") + ",");
                sql.Append("1");
                sql.Append(")");
                DB.ExecuteSQL(sql.ToString());
            }
        }

        static public String GetCountrySelectList(String currentLocaleSetting)
        {
            String CacheName = "GetCountrySelectList" + currentLocaleSetting + "_" + CommonLogic.GetThisPageName(true);
            StringBuilder optionlist = new StringBuilder();
            StringBuilder tmpS = new StringBuilder(4096);
            tmpS.Append("<!-- COUNTRY SELECT LIST -->\n");

            tmpS.Append("<select size=\"1\" onChange=\"self.location='setlocale.aspx?returnURL=" + ReturnURLEncode(GetThisPageUrlWithQueryString()) + "&localesetting=' + document.getElementById('CountrySelectList').value\" id=\"CountrySelectList\" name=\"CountrySelectList\" class=\"CountrySelectList\">");

            if (AppLogic.CachingOn)
            {
                String Menu = (String)HttpContext.Current.Cache.Get(CacheName);
                if (Menu != null)
                {
                    tmpS.Append(Menu);
                    tmpS.Append("</select>");

                    tmpS.Append("<!-- END COUNTRY SELECT LIST -->\n");
                    if (CommonLogic.ApplicationBool("DumpSQL"))
                    {
                        HttpContext.Current.Response.Write("Cache Hit Found!\n");
                    }
                    return tmpS.ToString();
                }
            }

            bool hasLocales = false;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select * from LocaleSetting   with (NOLOCK)  order by displayorder,description", con))
                {
                    while (rs.Read())
                    {
                        hasLocales = true;
                        optionlist.Append("<option value=\"" + DB.RSField(rs, "Name") + "\" " + CommonLogic.IIF(currentLocaleSetting == DB.RSField(rs, "Name"), " selected ", String.Empty) + ">" + DB.RSField(rs, "Description") + "</option>");
                    }
                }
            }

            if (!hasLocales)
            {
                return string.Empty;
            }

            tmpS.Append(optionlist.ToString());
            tmpS.Append("</select>");

            tmpS.Append("<!-- END COUNTRY SELECT LIST -->\n");



            if (CachingOn)
            {
                HttpContext.Current.Cache.Insert(CacheName, optionlist.ToString(), null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }
            return tmpS.ToString();
        }

        static public String GetCurrencySelectList(Customer ThisCustomer)
        {
            if (Currency.NumPublishedCurrencies() == 0)
            {
                return String.Empty;
            }

            String tmpS = Currency.GetSelectList("CurrencySelectList", "self.location='setcurrency.aspx?returnURL=" + ReturnURLEncode(GetThisPageUrlWithQueryString()) + "&currencysetting=' + document.getElementById('CurrencySelectList').value", String.Empty, ThisCustomer.CurrencySetting);
            return tmpS.ToString();
        }

        static public String GetVATSelectList(Customer ThisCustomer)
        {
            if (!VATIsEnabled())
            {
                return String.Empty;
            }
            String SelectName = "VATSelectList";

            String OnChangeHandler = "self.location='setvatsetting.aspx?returnURL=" + ReturnURLEncode(GetThisPageUrlWithQueryString()) + "&vatsetting=' + document.getElementById('VATSelectList').value";
            String CssClass = String.Empty;
            StringBuilder tmpS = new StringBuilder(4096);
            tmpS.Append("<select size=\"1\" id=\"" + SelectName + "\" name=\"" + SelectName + "\"");
            if (OnChangeHandler.Length != 0)
            {
                tmpS.Append(" onChange=\"" + OnChangeHandler + "\"");
            }
            if (CssClass.Length != 0)
            {
                tmpS.Append(" class=\"" + CssClass + "\"");
            }
            tmpS.Append(">");

            String msg1 = AppLogic.GetString("setvatsetting.aspx.5", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
            String msg2 = AppLogic.GetString("setvatsetting.aspx.3", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
            String msg3 = AppLogic.GetString("setvatsetting.aspx.4", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);

            tmpS.Append("<option value=\"" + ((int)VATSettingEnum.ShowPricesInclusiveOfVAT).ToString() + "\" " + CommonLogic.IIF(ThisCustomer.VATSettingRAW == VATSettingEnum.ShowPricesInclusiveOfVAT, " selected ", "") + ">" + msg2 + "</option>");
            tmpS.Append("<option value=\"" + ((int)VATSettingEnum.ShowPricesExclusiveOfVAT).ToString() + "\" " + CommonLogic.IIF(ThisCustomer.VATSettingRAW == VATSettingEnum.ShowPricesExclusiveOfVAT, " selected ", "") + ">" + msg3 + "</option>");

            tmpS.Append("</select>");
            return tmpS.ToString();
        }

        public static int SessionTimeout()
        {
            int ST = AppLogic.AppConfigUSInt("SessionTimeoutInMinutes");
            if (ST == 0)
            {
                ST = 20;
            }
            return ST;
        }

        public static int CacheDurationMinutes()
        {
            int ST = AppLogic.AppConfigUSInt("CacheDurationMinutes");
            if (ST == 0)
            {
                ST = 20;
            }
            return ST;
        }

        static public String GetUserMenu(bool IsRegistered, int SkinID, String m_LocaleSetting)
        {
            StringBuilder tmpS = new StringBuilder(1000);
            tmpS.Append("<div id=\"userMenu\" class=\"menu\">\n");
            if (!IsRegistered)
            {
                tmpS.Append("<a class=\"menu-item\" href=\"signin.aspx\">" + GetString("skinbase.cs.4", SkinID, m_LocaleSetting) + "</a>\n");
                tmpS.Append("<a class=\"menu-item\" href=\"account.aspx\">" + GetString("skinbase.cs.6", SkinID, m_LocaleSetting) + "</a>\n");
            }
            else
            {
                tmpS.Append("<a class=\"menu-item\" href=\"account.aspx\">" + GetString("skinbase.cs.7", SkinID, m_LocaleSetting) + "</a>\n");
                tmpS.Append("<a class=\"menu-item\" href=\"signout.aspx\">" + GetString("skinbase.cs.5", SkinID, m_LocaleSetting) + "</a>\n");
            }
            tmpS.Append("</div>\n");
            return tmpS.ToString();
        }

        static public String NoPictureImageURL(bool icon, int SkinID, String LocaleSetting)
        {
            return AppLogic.LocateImageURL("~/App_Themes/skin_" + SkinID.ToString() + "/images/nopicture" + CommonLogic.IIF(icon, "icon", String.Empty) + ".gif", LocaleSetting);
        }

        // given an input image string like /skins/skin_1/images/shoppingcart.gif
        // tries to resolve it to the proper locale by:
        // /skins/skin_1/images/shoppingcart.LocaleSetting.gif first
        // /skins/skin_1/images/shoppingcart.WebConfigLocale.gif second
        // /skins/skin_1/images/shoppingcart.gif last
        static public String LocateImageURL(String ImageName, String LocaleSetting)
        {
            String CacheName = "LocateImageURL_" + ImageName + "_" + LocaleSetting;
            if (AppLogic.CachingOn)
            {
                String s = (String)HttpContext.Current.Cache.Get(CacheName);
                if (s != null)
                {
                    if (CommonLogic.ApplicationBool("DumpSQL"))
                    {
                        HttpContext.Current.Response.Write("Cache Hit Found!\n");
                    }
                    if (!CommonLogic.FileExists(ResolveUrl(s)) && !s.Contains("~"))
                        s = "~/" + s;
                    return ResolveUrl(s);
                }
            }
            int i = ImageName.LastIndexOf(".");
            String url = String.Empty;
            if (i == -1)
            {
                url = ImageName; // no extension??
            }
            else
            {
                String Extension = ImageName.Substring(i);
                url = ImageName.Substring(0, i) + "." + LocaleSetting + Extension;
                if (!CommonLogic.FileExists(url))
                {
                    url = ImageName.Substring(0, i) + "." + Localization.GetDefaultLocale() + Extension;
                    url = ResolveUrl(url);
                }
                if (!CommonLogic.FileExists(url))
                {
                    url = ImageName.Substring(0, i) + Extension;
                    url = ResolveUrl(url);
                }
            }
            if (CachingOn)
            {
                HttpContext.Current.Cache.Insert(CacheName, url, null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }
            if (!CommonLogic.FileExists(url) && !url.Contains("~"))
                url = "~/" + url;
            return ResolveUrl(url);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ImageName">Image filename with or without the extension</param>
        /// <param name="ImageType">e.g. Category, Section, Product</param>
        /// <param name="LocaleSetting">Viewing Locale</param>
        /// <returns>full path to the image</returns>
        static public String LocateImageURL(String ImageName, String ImageType, String ImgSize, String LocaleSetting)
        {
            try
            {
                ImageName = ImageName.Trim();
                string WebConfigLocale = "." + Localization.GetDefaultLocale();
                string IPath = GetImagePath(ImageType, ImgSize, true);
                if (LocaleSetting.Trim() != String.Empty)
                {
                    LocaleSetting = "." + LocaleSetting;
                }
                bool UseCache = !IsAdminSite;

                //Used for ImageFilenameOverride
                if (ImageName.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase) || ImageName.EndsWith(".gif", StringComparison.InvariantCultureIgnoreCase) || ImageName.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase))
                {
                    String[] imagepaths = { IPath + ImageName.Replace(".", LocaleSetting + "."), IPath + ImageName.Replace(".", WebConfigLocale + "."), IPath + ImageName };
                    foreach (string ImagePath in imagepaths)
                    {
                        if (UseCache && ImageFilenameCache.ContainsKey(ImagePath) && ((String)ImageFilenameCache[ImagePath]).Length > 1)
                        {
                            return (String)ImageFilenameCache[ImagePath];
                        }
                        else if (File.Exists(ImagePath))
                        {
                            if (UseCache)
                            {
                                ImageFilenameCache[ImagePath] = GetImagePath(ImageType, ImgSize, false) + ImageName;
                                return (String)ImageFilenameCache[ImagePath];
                            }
                            else
                            {
                                return GetImagePath(ImageType, ImgSize, false) + ImageName;
                            }
                        }
                        if (UseCache && (ImageFilenameCache[ImagePath] == null || (String)ImageFilenameCache[ImagePath] == String.Empty)) ImageFilenameCache[ImagePath] = "0";
                    }
                    return String.Empty;
                }
                else //all other image name formats (i.e. productid, sku)
                {
                    String[] imageext = { ".jpg", ".gif", ".png" };
                    foreach (string ext in imageext)
                    {
                        String[] locales = { LocaleSetting, WebConfigLocale, String.Empty };
                        foreach (string locale in locales)
                        {
                            string ImagePath = IPath + ImageName + locale + ext;
                            if (UseCache && ImageFilenameCache.ContainsKey(ImagePath) && ((String)ImageFilenameCache[ImagePath]).Length > 1)
                            {
                                return (String)ImageFilenameCache[ImagePath];
                            }
                            else if (File.Exists(ImagePath))
                            {
                                if (UseCache)
                                {
                                    ImageFilenameCache[ImagePath] = GetImagePath(ImageType, ImgSize, false) + ImageName + locale + ext;
                                    return (String)ImageFilenameCache[ImagePath];
                                }
                                else
                                {
                                    return GetImagePath(ImageType, ImgSize, false) + ImageName + locale + ext;
                                }
                            }
                            if (UseCache && (ImageFilenameCache[ImagePath] == null || (String)ImageFilenameCache[ImagePath] == String.Empty)) ImageFilenameCache[ImagePath] = "0";
                        }
                    }
                    return String.Empty;
                }
            }
            catch
            {
                return String.Empty;
            }

        }

        static public String LocateImageURL(String ImageName)
        {
            return AppLogic.LocateImageURL(ImageName, Thread.CurrentThread.CurrentUICulture.Name);
        }

        static public String WriteTabbedContents(String tabDivName, int selectedTabIdx, bool includeTabJSDriverFile, String[] names, String[] values)
        {
            StringBuilder tmpS = new StringBuilder(10000);
            if (includeTabJSDriverFile)
            {
                tmpS.Append("<script type='text/javascript' src='Scripts/tabs.js'></script>");
            }
            tmpS.Append("<div class=\"tab-container\" id=\"" + tabDivName + "\" width=\"100%\">\n");
            tmpS.Append("<ul class=\"tabs\">\n");
            bool addName = true;
            for (int i = names.GetLowerBound(0); i <= names.GetUpperBound(0); i++)
            {
                if (addName)
                {
                    tmpS.Append("<li><a href=\"#\" onClick=\"return showPane('" + tabDivName + "_pane" + (i + 1).ToString() + "', this)\" id=\"" + tabDivName + "_tab" + (i + 1).ToString() + "\">" + names[i] + "</a></li>\n");
                }
            }
            tmpS.Append("</ul>\n");
            tmpS.Append("<div class=\"tab-panes\" width=\"100%\">\n");

            for (int i = names.GetLowerBound(0); i <= names.GetUpperBound(0); i++)
            {
                if (addName)
                {
                    tmpS.Append("<div id=\"" + tabDivName + "_pane" + (i + 1).ToString() + "\" width=\"100%\" style=\"overflow:auto;height:200px;\">\n");
                    tmpS.Append(values[i]);
                    tmpS.Append("</div>\n");
                }
            }
            tmpS.Append("</div>\n");
            tmpS.Append("</div>\n");
            tmpS.Append("<script language=\"JavaScript1.3\">\nsetupPanes('" + tabDivName + "', '" + tabDivName + "_tab" + CommonLogic.IIF(selectedTabIdx == 0, "1", selectedTabIdx.ToString()) + "');\n</script>\n");
            return tmpS.ToString();
        }

        static public String GetLocaleEntryFields(String fieldVal, String baseFormFieldName, bool useTextArea, bool htmlEncodeIt, bool isRequired, String requiredFieldMissingPrompt, int maxLength, int displaySize, int displayRows, int displayCols, bool HTMLOk)
        {
            String MasterLocale = Localization.GetDefaultLocale();
            StringBuilder tmpS = new StringBuilder(4096);
            String ThisLocale = String.Empty;

            if (displayRows == 0)
            {
                displayRows = 5;
            }
            if (displayCols == 0)
            {
                displayCols = 80;
            }

            if (AppLogic.NumLocaleSettingsInstalled() < 2)
            {
                // for only 1 locale, just store things directly for speed:
                ThisLocale = MasterLocale;
                String FormFieldName = baseFormFieldName;
                String ThisLocaleValue = fieldVal;
                if (fieldVal.StartsWith("<ml>", StringComparison.InvariantCultureIgnoreCase) || fieldVal.StartsWith("&lt;ml&gt;", StringComparison.InvariantCultureIgnoreCase))
                {
                    ThisLocaleValue = XmlCommon.GetLocaleEntry(fieldVal, ThisLocale, false);
                }
                if (htmlEncodeIt)
                {
                    ThisLocaleValue = HttpContext.Current.Server.HtmlEncode(ThisLocaleValue);
                }
                if (useTextArea)
                {
                    tmpS.Append("<div id=\"id" + FormFieldName + "\" style=\"height: 1%;\">");
                    tmpS.Append("<textarea cols=\"" + displayCols + "\" rows=\"" + displayRows.ToString() + "\" id=\"" + FormFieldName + "\" name=\"" + FormFieldName + "\">" + ThisLocaleValue + "</textarea>\n");
                    tmpS.Append("</div>");
                }
                else
                {
                    tmpS.Append("<input maxLength=\"" + maxLength + "\" size=\"" + displaySize + "\" id=\"" + FormFieldName + "\" name=\"" + FormFieldName + "\" value=\"" + ThisLocaleValue + "\">");
                }
                if (isRequired)
                {
                    tmpS.Append("<input type=\"hidden\" name=\"" + FormFieldName + "_vldt\" value=\"[req][blankalert=" + requiredFieldMissingPrompt + " (" + ThisLocale + ")]\">\n");
                }
            }
            else
            {
                List<string> locales = new List<string>();
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS("select Name from LocaleSetting   with (NOLOCK)  order by displayorder,description", con))
                    {
                        while (rs.Read())
                        {
                            locales.Add(DB.RSField(rs, "Name"));
                        }
                    }
                }

                if (useTextArea)
                {
                    tmpS.Append("<div class=\"tab-container2\" id=\"" + baseFormFieldName + "Div\">\n");
                }
                else
                {
                    tmpS.Append("<div class=\"tab-container\" id=\"" + baseFormFieldName + "Div\">\n");
                }

                int i = 1;
                tmpS.Append("<div class=\"tab-panes\">\n");
                tmpS.Append("<table>\n");
                tmpS.Append("<tbody>\n");
                foreach (string locale in locales)
                {
                    tmpS.Append("<tr>\n");
                    ThisLocale = locale;
                    String FormFieldName = baseFormFieldName + "_" + ThisLocale.Replace("-", "_");
                    String ThisLocaleValue = XmlCommon.GetLocaleEntry(fieldVal, ThisLocale, false);
                    if (htmlEncodeIt)
                    {
                        ThisLocaleValue = HttpContext.Current.Server.HtmlEncode(ThisLocaleValue);
                    }
                    if (useTextArea)
                    {
                        tmpS.Append("<td>" + locale + "</td><td>");
                        tmpS.Append("<div id=\"id" + FormFieldName + "\" style=\"height: 1%;\">");
                        tmpS.Append(locale + ": <textarea rows=\"" + displayRows.ToString() + "\" cols=\"" + displayCols.ToString() + "\" id=\"" + FormFieldName + "\" name=\"" + FormFieldName + "\">" + ThisLocaleValue + "</textarea>\n");
                        tmpS.Append("</div>");
                        tmpS.Append("</td>");
                    }
                    else
                    {
                        tmpS.Append("<td>" + locale + "</td><td> <input maxLength=\"" + maxLength + "\" size=\"" + displaySize + "\" id=\"" + FormFieldName + "\" name=\"" + FormFieldName + "\" value=\"" + ThisLocaleValue + "\">");
                    }
                    if (isRequired && ThisLocale == MasterLocale)
                    {
                        tmpS.Append("<b>*</b><input type=\"hidden\" name=\"" + FormFieldName + "_vldt\" value=\"[req][blankalert=" + requiredFieldMissingPrompt + " (" + ThisLocale + ")]\">\n");
                    }

                    tmpS.Append("\n");
                    tmpS.Append("</tr>\n");
                    i++;
                }
                tmpS.Append("</tbody>\n");
                tmpS.Append("</table>\n");
                tmpS.Append("</div>\n");
                tmpS.Append("</div>\n");
            }

            return tmpS.ToString();
        }

        static public void UpdateNumLocaleSettingsInstalled()
        {
                String CacheName = "NumLocaleSettingsInstalled";
                int N = DB.GetSqlN("select count(*) as N from LocaleSetting with (NOLOCK)");
                HttpContext.Current.Cache.Insert(CacheName, N.ToString(), null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }

        static public int NumLocaleSettingsInstalled()
        {
            int N = 0; // can't ever be 0 really ;)
                String CacheName = "NumLocaleSettingsInstalled";
                if (AppLogic.CachingOn)
                {
                    String s = (String)HttpContext.Current.Cache.Get(CacheName);
                    if (s != null)
                    {
                        if (CommonLogic.ApplicationBool("DumpSQL"))
                        {
                            HttpContext.Current.Response.Write("Cache Hit Found!\n");
                        }
                        N = Localization.ParseUSInt(s);
                    }
                }
                if (N == 0)
                {
                    N = DB.GetSqlN("select count(*) as N from LocaleSetting with (NOLOCK)");
                }
                if (N == 0)
                {
                    N = 1;
                }
                if (CachingOn)
                {
                    HttpContext.Current.Cache.Insert(CacheName, N.ToString(), null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
                }
            
            return N;
        }

        static public String FormLocaleXml(String baseFormFieldName)
        {
            if (AppLogic.NumLocaleSettingsInstalled() > 1)
            {
                    StringBuilder tmpS = new StringBuilder(4096);
                    tmpS.Append("<ml>");

                    using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                    {
                        con.Open();
                        using (IDataReader rs = DB.GetRS("select * from LocaleSetting   with (NOLOCK)  order by displayorder,description", con))
                        {
                            while (rs.Read())
                            {
                                String ThisLocale = DB.RSField(rs, "Name");
                                String FormFieldName = baseFormFieldName + "_" + ThisLocale.Replace("-", "_");
                                String FormFieldVal = CommonLogic.FormCanBeDangerousContent(FormFieldName);
                                if (FormFieldVal.Length != 0)
                                {
                                    tmpS.Append("<locale name=\"" + ThisLocale + "\">");
                                    tmpS.Append(XmlCommon.XmlEncode(FormFieldVal));
                                    tmpS.Append("</locale>");
                                }
                            }
                        }
                    }

                    tmpS.Append("</ml>");
                    return tmpS.ToString();
                }
            else
            {
                return CommonLogic.FormCanBeDangerousContent(baseFormFieldName);
            }
        }

        static public String FormLocaleXml(String FieldLocaleVal, string locale)
        {
            if (AppLogic.NumLocaleSettingsInstalled() > 1)
            {
                StringBuilder tmpS = new StringBuilder(4096);
                tmpS.Append("<ml>");
                XmlNodeList nl = Localization.LocalesDoc.SelectNodes("//Locales");
                foreach (XmlNode xn in nl)
                {
                    String thisLocale = xn.Attributes["Name"].InnerText;
                    tmpS.Append("<locale name=\"" + thisLocale + "\">");
                    if (thisLocale == locale)
                    {
                        tmpS.Append(XmlCommon.XmlEncode(FieldLocaleVal));
                    }
                    tmpS.Append("</locale>");
                }
                tmpS.Append("</ml>");
                return tmpS.ToString();
            }
            else
            {
                return FieldLocaleVal;
            }
        }

        static public String FormLocaleXml(string sqlName, string formValue, string locale, EntitySpecs eSpecs, int eID)
        {
            if (AppLogic.NumLocaleSettingsInstalled() > 1)
            {
                //gets the current DB value
                string sqlNameValue = DB.GetSqlSAllLocales("SELECT " + sqlName + " AS S FROM " + eSpecs.m_EntityName + " WHERE " + eSpecs.m_EntityName + "ID=" + eID.ToString());

                StringBuilder tmpS = new StringBuilder(4096);
                tmpS.Append("<ml>");
                XmlNodeList nl = Localization.LocalesDoc.SelectNodes("//Locales");
                foreach (XmlNode xn in nl)
                {
                    String thisLocale = xn.Attributes["Name"].InnerText;
                    string localeEntry = XmlCommon.GetLocaleEntry(sqlNameValue, thisLocale, false);
                    if (thisLocale.Equals(locale, StringComparison.InvariantCultureIgnoreCase))
                    {
                        tmpS.Append("<locale name=\"" + thisLocale + "\">");
                        tmpS.Append(XmlCommon.XmlEncode(formValue));
                        tmpS.Append("</locale>");
                    }
                    else
                    {
                        tmpS.Append("<locale name=\"" + thisLocale + "\">");
                        tmpS.Append(XmlCommon.XmlEncode(localeEntry));
                        tmpS.Append("</locale>");
                    }
                }
                tmpS.Append("</ml>");
                return tmpS.ToString();
            }
            else
            {
                return formValue;
            }
        }

        static public String FormLocaleXml(string sqlName, string formValue, string locale, string table, int eID)
        {
            if (AppLogic.NumLocaleSettingsInstalled() > 1)
            {
                //gets the current DB value
                string sqlNameValue = DB.GetSqlSAllLocales("SELECT " + sqlName + " AS S FROM " + table + " WHERE " + table + "ID=" + eID.ToString());

                StringBuilder tmpS = new StringBuilder(4096);
                tmpS.Append("<ml>");
                XmlNodeList nl = Localization.LocalesDoc.SelectNodes("//Locales");
                foreach (XmlNode xn in nl)
                {
                    String thisLocale = xn.Attributes["Name"].InnerText;
                    string localeEntry = XmlCommon.GetLocaleEntry(sqlNameValue, thisLocale, false);
                    if (thisLocale.Equals(locale, StringComparison.InvariantCultureIgnoreCase))
                    {
                        tmpS.Append("<locale name=\"" + thisLocale + "\">");
                        tmpS.Append(XmlCommon.XmlEncode(formValue));
                        tmpS.Append("</locale>");
                    }
                    else
                    {
                        tmpS.Append("<locale name=\"" + thisLocale + "\">");
                        if (localeEntry.Length == 0)
                        {
                            XmlCommon.XmlEncode(formValue);
                        }
                        else
                        {
                            tmpS.Append(XmlCommon.XmlEncode(localeEntry));
                        }
                        tmpS.Append("</locale>");
                    }
                }
                tmpS.Append("</ml>");
                return tmpS.ToString();
            }
            else
            {
                return formValue;
            }
        }

        static public String FormLocaleXmlVariant(string sqlName, string formValue, string locale, int eID)
        {
            if (AppLogic.NumLocaleSettingsInstalled() > 1)
            {
                //gets the current DB value
                string sqlNameValue = DB.GetSqlSAllLocales("SELECT " + sqlName + " AS S FROM ProductVariant WHERE VariantID=" + eID.ToString());

                StringBuilder tmpS = new StringBuilder(4096);
                tmpS.Append("<ml>");
                XmlNodeList nl = Localization.LocalesDoc.SelectNodes("//Locales");
                foreach (XmlNode xn in nl)
                {
                    String thisLocale = xn.Attributes["Name"].InnerText;
                    string localeEntry = XmlCommon.GetLocaleEntry(sqlNameValue, thisLocale, false);
                    if (thisLocale.Equals(locale, StringComparison.InvariantCultureIgnoreCase))
                    {
                        tmpS.Append("<locale name=\"" + thisLocale + "\">");
                        tmpS.Append(XmlCommon.XmlEncode(formValue));
                        tmpS.Append("</locale>");
                    }
                    else
                    {
                        tmpS.Append("<locale name=\"" + thisLocale + "\">");
                        if (localeEntry.Length == 0)
                        {
                            XmlCommon.XmlEncode(formValue);
                        }
                        else
                        {
                            tmpS.Append(XmlCommon.XmlEncode(localeEntry));
                        }
                        tmpS.Append("</locale>");
                    }
                }
                tmpS.Append("</ml>");
                return tmpS.ToString();
            }
            else
            {
                return formValue;
            }
        }

        static public String FormLocaleXmlEditor(string sqlName, string formName, string locale, EntitySpecs eSpecs, int eID)
        {
            if (AppLogic.NumLocaleSettingsInstalled() > 1)
            {
                //gets the current DB value
                string sqlNameValue = DB.GetSqlSAllLocales("SELECT " + sqlName + " AS S FROM " + eSpecs.m_EntityName + " WHERE " + eSpecs.m_EntityName + "ID=" + eID.ToString());

                StringBuilder tmpS = new StringBuilder(4096);
                tmpS.Append("<ml>");
                XmlNodeList nl = Localization.LocalesDoc.SelectNodes("//Locales");
                foreach (XmlNode xn in nl)
                {
                    String thisLocale = xn.Attributes["Name"].InnerText;
                    string localeEntry = XmlCommon.GetLocaleEntry(sqlNameValue, thisLocale, false);

                    if (thisLocale.Equals(locale, StringComparison.InvariantCultureIgnoreCase))
                    {
                        tmpS.Append("<locale name=\"" + thisLocale + "\">");
                        tmpS.Append(XmlCommon.XmlEncode(CommonLogic.FormCanBeDangerousContent(formName)));
                        tmpS.Append("</locale>");
                    }
                    else
                    {
                        tmpS.Append("<locale name=\"" + thisLocale + "\">");
                        tmpS.Append(XmlCommon.XmlEncode(localeEntry));
                        tmpS.Append("</locale>");
                    }
                }
                tmpS.Append("</ml>");
                return tmpS.ToString();
            }
            else
            {
                return CommonLogic.FormCanBeDangerousContent(formName);
            }
        }

        static public String FormLocaleXmlEditor(string sqlName, string formName, string locale, string table, int eID)
        {
            if (AppLogic.NumLocaleSettingsInstalled() > 1)
            {
                //gets the current DB value
                string sqlNameValue = DB.GetSqlSAllLocales("SELECT " + sqlName + " AS S FROM " + table + " WHERE " + table + "ID=" + eID.ToString());

                StringBuilder tmpS = new StringBuilder(4096);
                tmpS.Append("<ml>");
                XmlNodeList nl = Localization.LocalesDoc.SelectNodes("//Locales");
                foreach (XmlNode xn in nl)
                {
                    String thisLocale = xn.Attributes["Name"].InnerText;
                    string localeEntry = XmlCommon.GetLocaleEntry(sqlNameValue, thisLocale, false);

                    if (thisLocale.Equals(locale, StringComparison.InvariantCultureIgnoreCase))
                    {
                        tmpS.Append("<locale name=\"" + thisLocale + "\">");
                        tmpS.Append(XmlCommon.XmlEncode(CommonLogic.FormCanBeDangerousContent(formName)));
                        tmpS.Append("</locale>");
                    }
                    else
                    {
                        tmpS.Append("<locale name=\"" + thisLocale + "\">");
                        tmpS.Append(XmlCommon.XmlEncode(localeEntry));
                        tmpS.Append("</locale>");
                    }
                }
                tmpS.Append("</ml>");
                return tmpS.ToString();
            }
            else
            {
                return CommonLogic.FormCanBeDangerousContent(formName);
            }
        }

        static public String FormLocaleXmlEditorVariant(string sqlName, string formName, string locale, int eID)
        {
            if (AppLogic.NumLocaleSettingsInstalled() > 1)
            {
                //gets the current DB value
                string sqlNameValue = DB.GetSqlSAllLocales("SELECT " + sqlName + " AS S FROM ProductVariant WHERE VariantID=" + eID.ToString());

                StringBuilder tmpS = new StringBuilder(4096);
                tmpS.Append("<ml>");
                XmlNodeList nl = Localization.LocalesDoc.SelectNodes("//Locales");
                foreach (XmlNode xn in nl)
                {
                    String thisLocale = xn.Attributes["Name"].InnerText;
                    string localeEntry = XmlCommon.GetLocaleEntry(sqlNameValue, thisLocale, false);

                    if (thisLocale.Equals(locale, StringComparison.InvariantCultureIgnoreCase))
                    {
                        tmpS.Append("<locale name=\"" + thisLocale + "\">");
                        tmpS.Append(XmlCommon.XmlEncode(CommonLogic.FormCanBeDangerousContent(formName)));
                        tmpS.Append("</locale>");
                    }
                    else
                    {
                        tmpS.Append("<locale name=\"" + thisLocale + "\">");
                        tmpS.Append(XmlCommon.XmlEncode(localeEntry));
                        tmpS.Append("</locale>");
                    }
                }
                tmpS.Append("</ml>");
                return tmpS.ToString();
            }
            else
            {
                return CommonLogic.FormCanBeDangerousContent(formName);
            }
        }

        static public String GetFormsDefaultLocale(string sqlName, string formValue, string locale, EntitySpecs eSpecs, int eID)
        {
            if (AppLogic.NumLocaleSettingsInstalled() > 1)
            {
                //gets the current DB value
                string sqlNameValue = DB.GetSqlSAllLocales("SELECT " + sqlName + " AS S FROM " + eSpecs.m_EntityName + " WHERE " + eSpecs.m_EntityName + "ID=" + eID.ToString());

                XmlNodeList nl = Localization.LocalesDoc.SelectNodes("//Locales");
                foreach (XmlNode xn in nl)
                {
                    String thisLocale = xn.Attributes["Name"].InnerText;
                    string localeEntry = XmlCommon.GetLocaleEntry(sqlNameValue, thisLocale, false);

                    if (thisLocale.Equals(Localization.GetDefaultLocale(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (thisLocale.Equals(locale, StringComparison.InvariantCultureIgnoreCase))
                        {
                            return formValue;
                        }
                        else
                        {
                            return localeEntry;
                        }
                    }
                }
                return formValue;
            }
            else
            {
                return formValue;
            }
        }

        static public String GetFormsDefaultLocale(string sqlName, string formValue, string locale, string table, int eID)
        {
            if (AppLogic.NumLocaleSettingsInstalled() > 1)
            {
                //gets the current DB value
                string sqlNameValue = DB.GetSqlSAllLocales("SELECT " + sqlName + " AS S FROM " + table + " WHERE " + table + "ID=" + eID.ToString());

                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS("select * from LocaleSetting   with (NOLOCK)  order by displayorder,description", con))
                    {
                        while (rs.Read())
                        {
                            String thisLocale = DB.RSField(rs, "Name");
                            string localeEntry = XmlCommon.GetLocaleEntry(sqlNameValue, thisLocale, false);

                            if (thisLocale.Equals(Localization.GetDefaultLocale(), StringComparison.InvariantCultureIgnoreCase))
                            {
                                if (thisLocale.Equals(locale, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    return formValue;
                                }
                                else
                                {
                                    return localeEntry;
                                }
                            }
                        }
                    }
                }

                return formValue;
            }
            else
            {
                return formValue;
            }
        }

        static public String GetCartContinueShoppingURL(int SkinID, String LocaleSetting)
        {
            String tmpS = AppLogic.AppConfig("ContinueShoppingURL"); // this overrides all!
            if (tmpS.Length == 0)
            {
                // no appconfig set, so try to do something reasonable:
                if (CommonLogic.QueryStringUSInt("ResetLinkback") == 1)
                {
                    tmpS = "default.aspx";
                }
            }
            return tmpS;
        }

        // if an attribute (size/color) has a price modifier, this removes that and just returns the
        // base color/size portion
        static public String RemoveAttributePriceModifier(String s)
        {
            char[] splitchars = { '[', ']' };
            String[] x = s.Split(splitchars);
            String tmp = x[0];
            return tmp.Trim();
        }

        public static void GetSubscriptionInterval(int VariantID, out int i, out DateIntervalTypeEnum iType)
        {
            i = 0;
            iType = DateIntervalTypeEnum.Monthly; // for backwards compatibility

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("Select SubscriptionInterval,SubscriptionIntervalType from productvariant where variantid=" + VariantID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        i = DB.RSFieldInt(rs, "SubscriptionInterval");
                        iType = (DateIntervalTypeEnum)DB.RSFieldInt(rs, "SubscriptionIntervalType");
                    }
                }
            }
        }

        public static void CustomerSubscriptionUpdate(int OrderNumber)
        {
            CustomerSubscriptionUpdate(OrderNumber, AppLogic.AppConfigBool("SubscriptionExtensionOccursFromOrderDate"));
        }

        /// <summary>
        /// WARNING! This will override the appconfig setting for SubscriptionExtensionOccursFromOrderDate.
        /// If you don't want to do that, use the overload that only specifies an OrderNumber.
        /// </summary>
        /// <param name="OrderNumber"></param>
        /// <param name="SubscriptionExtensionOccursFromOrderDate"></param>
        public static void CustomerSubscriptionUpdate(int OrderNumber, bool SubscriptionExtensionOccursFromOrderDate)
        {
                Order order = new Order(OrderNumber, Localization.GetDefaultLocale());
                if (order.TransactionIsCaptured())
                {
                    foreach (CartItem c in order.CartItems)
                    {
                        int SubscriptionInterval = c.SubscriptionInterval;
                        DateIntervalTypeEnum SubscriptionIntervalType = c.SubscriptionIntervalType;

                        if (SubscriptionInterval != 0)
                        {
                            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                            {
                                con.Open();
                                using (IDataReader rs = DB.GetRS("Select SubscriptionExpiresOn from customer  with (NOLOCK)  where CustomerID=" + order.CustomerID.ToString(), con))
                                {
                                    if (rs.Read())
                                    {
                                        DateTime NewExpireDate = System.DateTime.Now;
                                        if (!SubscriptionExtensionOccursFromOrderDate && !DB.RSFieldDateTime(rs, "SubscriptionExpiresOn").Equals(System.DateTime.MinValue))
                                        {
                                            NewExpireDate = DB.RSFieldDateTime(rs, "SubscriptionExpiresOn");
                                        }
                                        switch (SubscriptionIntervalType)
                                        {
                                            case DateIntervalTypeEnum.Day:
                                                NewExpireDate = NewExpireDate.AddDays(SubscriptionInterval * c.Quantity);
                                                break;
                                            case DateIntervalTypeEnum.Week:
                                                NewExpireDate = NewExpireDate.AddDays(7 * SubscriptionInterval * c.Quantity);
                                                break;
                                            case DateIntervalTypeEnum.Month:
                                                NewExpireDate = NewExpireDate.AddMonths(SubscriptionInterval * c.Quantity);
                                                break;
                                            case DateIntervalTypeEnum.Year:
                                                NewExpireDate = NewExpireDate.AddYears(SubscriptionInterval * c.Quantity);
                                                break;
                                            case DateIntervalTypeEnum.Weekly:
                                                NewExpireDate = NewExpireDate.AddDays(7 * c.Quantity);
                                                break;
                                            case DateIntervalTypeEnum.BiWeekly:
                                                NewExpireDate = NewExpireDate.AddDays(14 * c.Quantity);
                                                break;
                                            case DateIntervalTypeEnum.EveryFourWeeks:
                                                NewExpireDate = NewExpireDate.AddDays(28 * c.Quantity);
                                                break;
                                            case DateIntervalTypeEnum.Monthly:
                                                NewExpireDate = NewExpireDate.AddMonths(1 * c.Quantity);
                                                break;
                                            case DateIntervalTypeEnum.Quarterly:
                                                NewExpireDate = NewExpireDate.AddMonths(3 * c.Quantity);
                                                break;
                                            case DateIntervalTypeEnum.SemiYearly:
                                                NewExpireDate = NewExpireDate.AddMonths(6 * c.Quantity);
                                                break;
                                            case DateIntervalTypeEnum.Yearly:
                                                NewExpireDate = NewExpireDate.AddYears(1 * c.Quantity);
                                                break;
                                            default:
                                                NewExpireDate = NewExpireDate.AddMonths(SubscriptionInterval * c.Quantity);
                                                break;
                                        }
                                        DB.ExecuteSQL(String.Format("update Customer set SubscriptionExpiresOn={0} where CustomerID={1}", DB.DateQuote(Localization.ToDBShortDateString(NewExpireDate)), order.CustomerID));
                                    }
                                }
                            }
                        }
                    }
                }
            }

        public static void CustomerSubscriptionUndo(int OrderNumber)
        {
            Order order = new Order(OrderNumber, Localization.GetDefaultLocale());
            if (order.TransactionIsCaptured())
            {
                foreach (CartItem c in order.CartItems)
                {
                    int SubscriptionInterval = c.SubscriptionInterval;
                    DateIntervalTypeEnum SubscriptionIntervalType = c.SubscriptionIntervalType;

                    if (SubscriptionInterval != 0)
                    {
                        // get customer's current subscription expiration:
                        using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                        {
                            con.Open();
                            using (IDataReader rs = DB.GetRS("Select SubscriptionExpiresOn from customer  with (NOLOCK)  where CustomerID=" + order.CustomerID.ToString(), con))
                            {
                                if (rs.Read())
                                {
                                    DateTime NewExpireDate = System.DateTime.Now;
                                    if (!AppLogic.AppConfigBool("SubscriptionExtensionOccursFromOrderDate") && !DB.RSFieldDateTime(rs, "SubscriptionExpiresOn").Equals(System.DateTime.MinValue))
                                    {
                                        NewExpireDate = DB.RSFieldDateTime(rs, "SubscriptionExpiresOn");
                                    }
                                    switch (SubscriptionIntervalType)
                                    {
                                        case DateIntervalTypeEnum.Day:
                                            NewExpireDate = NewExpireDate.AddDays(-SubscriptionInterval * c.Quantity);
                                            break;
                                        case DateIntervalTypeEnum.Week:
                                            NewExpireDate = NewExpireDate.AddDays(-7 * SubscriptionInterval * c.Quantity);
                                            break;
                                        case DateIntervalTypeEnum.Month:
                                            NewExpireDate = NewExpireDate.AddMonths(-SubscriptionInterval * c.Quantity);
                                            break;
                                        case DateIntervalTypeEnum.Year:
                                            NewExpireDate = NewExpireDate.AddYears(-SubscriptionInterval * c.Quantity);
                                            break;
                                        case DateIntervalTypeEnum.Weekly:
                                            NewExpireDate = NewExpireDate.AddDays(-7 * c.Quantity);
                                            break;
                                        case DateIntervalTypeEnum.BiWeekly:
                                            NewExpireDate = NewExpireDate.AddDays(-14 * c.Quantity);
                                            break;
                                        case DateIntervalTypeEnum.EveryFourWeeks:
                                            NewExpireDate = NewExpireDate.AddDays(-28 * c.Quantity);
                                            break;
                                        case DateIntervalTypeEnum.Monthly:
                                            NewExpireDate = NewExpireDate.AddMonths(-1 * c.Quantity);
                                            break;
                                        case DateIntervalTypeEnum.Quarterly:
                                            NewExpireDate = NewExpireDate.AddMonths(-3 * c.Quantity);
                                            break;
                                        case DateIntervalTypeEnum.SemiYearly:
                                            NewExpireDate = NewExpireDate.AddMonths(-6 * c.Quantity);
                                            break;
                                        case DateIntervalTypeEnum.Yearly:
                                            NewExpireDate = NewExpireDate.AddYears(-1 * c.Quantity);
                                            break;
                                        default:
                                            NewExpireDate = NewExpireDate.AddMonths(-SubscriptionInterval * c.Quantity);
                                            break;
                                    }
                                    DB.ExecuteSQL(String.Format("update Customer set SubscriptionExpiresOn={0} where CustomerID={1}", DB.DateQuote(Localization.ToDBShortDateString(NewExpireDate)), order.CustomerID));
                                }
                            }
                        }
                    }
                }
            }
        }

        // handles login "transformation" from one customer on the site to another, moving
        // cart items, etc...as required. This is a fairly complicated routine to get right ;)
        // this does NOT alter any session/cookie data...you should do that before/after this call
        // don't migrate their shipping info...their "old address book should take priority"
        static public void ExecuteSigninLogic(int CurrentCustomerID, int NewCustomerID)
        {
            String CurrentCustomerOrderNotes = String.Empty;
            String CurrentOrderOptions = String.Empty;
            String Coupon = String.Empty;
            String CustReferrer = HttpContext.Current.Profile.GetPropertyValue("Referrer").ToString();
            String CustSkinID = HttpContext.Current.Profile.GetPropertyValue("SkinID").ToString();


            if (CurrentCustomerID != 0)
            {
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rsx = DB.GetRS("Select * from customer   with (NOLOCK)  where customerid=" + CurrentCustomerID.ToString(), con))
                    {
                        if (rsx.Read())
                        {
                            CurrentCustomerOrderNotes = DB.RSField(rsx, "OrderNotes");
                            CurrentOrderOptions = DB.RSField(rsx, "OrderOptions");
                            Coupon = DB.RSField(rsx, "CouponCode");
                        }
                    }
                }
            }


            if (Coupon.Length != 0)
            {
                DB.ExecuteSQL("update Customer set CouponCode=" + DB.SQuote(Coupon) + " where CustomerID=" + NewCustomerID.ToString());
            }

            if (CustReferrer.Trim().Length > 0)
            {
                DB.ExecuteSQL("update customer set Referrer=" + DB.SQuote(CustReferrer) + " where customerid=" + NewCustomerID.ToString());
            }

            int ItemsInCartNow = ShoppingCart.NumItems(CurrentCustomerID, CartTypeEnum.ShoppingCart);

            if (AppLogic.AppConfigBool("ClearOldCartOnSignin")) // but not wish list, gift registry or recurring items!
            {
                // if preserve is on, force delete of old cart even if not set at appconfig level, since we are replacing it with the active cart:
                DB.ExecuteSQL("delete from ShoppingCart where CartType=" + ((int)CartTypeEnum.ShoppingCart).ToString() + " and customerid=" + NewCustomerID.ToString());
                DB.ExecuteSQL("delete from kitcart where CartType=" + ((int)CartTypeEnum.ShoppingCart).ToString() + " and customerid=" + NewCustomerID.ToString());
                PromotionManager.ClearAllPromotionUsages(NewCustomerID);
            }

            if (ItemsInCartNow > 0 && AppLogic.AppConfigBool("PreserveActiveCartOnSignin"))
            {
                if (CurrentCustomerID != 0)
				{
					Customer c = new Customer(NewCustomerID);
					DB.ExecuteSQL("update ShoppingCart set CustomerID=" + NewCustomerID.ToString() + " where CartType in (" + ((int) CartTypeEnum.ShoppingCart).ToString() + ") and customerid=" + CurrentCustomerID.ToString());
					DB.ExecuteSQL("update ShoppingCart set billingaddressid=" + c.PrimaryBillingAddressID.ToString() + " , ShippingAddressID=" + c.PrimaryShippingAddressID.ToString() + " where CartType in (" + ((int) CartTypeEnum.ShoppingCart).ToString() + ") and customerid=" + NewCustomerID.ToString());
					DB.ExecuteSQL("update kitcart set CustomerID=" + NewCustomerID.ToString() + " where CartType in (" + ((int) CartTypeEnum.ShoppingCart).ToString() + ") and customerid=" + CurrentCustomerID.ToString());
					DB.ExecuteSQL("update customer set Referrer=" + DB.SQuote(CustReferrer) + ", OrderNotes=" + DB.SQuote(CurrentCustomerOrderNotes) + ", FinalizationData=NULL, OrderOptions=" + DB.SQuote(CurrentOrderOptions) + " where customerid=" + NewCustomerID.ToString());
                    PromotionManager.TransferPromotionsOnUserLogin(CurrentCustomerID, NewCustomerID);
				}
            }

            int ItemsInWishListNow = ShoppingCart.NumItems(CurrentCustomerID, CartTypeEnum.WishCart);

            if (AppLogic.AppConfigBool("ClearOldCartOnSignin"))
            {
                // if preserve is on, force delete of old cart even if not set at appconfig level, since we are replacing it with the active cart:
                DB.ExecuteSQL("delete from ShoppingCart where CartType=" + ((int)CartTypeEnum.WishCart).ToString() + " and customerid=" + NewCustomerID.ToString());
                DB.ExecuteSQL("delete from kitcart where CartType=" + ((int)CartTypeEnum.WishCart).ToString() + " and customerid=" + NewCustomerID.ToString());
            }

            if (ItemsInWishListNow > 0 && AppLogic.AppConfigBool("PreserveActiveCartOnSignin"))
            {
                if (CurrentCustomerID != 0)
                {
                    DB.ExecuteSQL("update ShoppingCart set CustomerID=" + NewCustomerID.ToString() + " where CartType in (" + ((int)CartTypeEnum.WishCart).ToString() + ") and customerid=" + CurrentCustomerID.ToString());
                    DB.ExecuteSQL("update kitcart set CustomerID=" + NewCustomerID.ToString() + " where CartType in (" + ((int)CartTypeEnum.WishCart).ToString() + ") and customerid=" + CurrentCustomerID.ToString());
                }
            }

            //JH 10.20.2010 also move gift registry according to cart moving rules, but don't ever delete gift registry items
            int ItemsInGiftRegistryNow = ShoppingCart.NumItems(CurrentCustomerID, CartTypeEnum.GiftRegistryCart);

            if (ItemsInGiftRegistryNow > 0 && AppLogic.AppConfigBool("PreserveActiveCartOnSignin"))
            {
                if (CurrentCustomerID != 0)
                {
                    DB.ExecuteSQL("update ShoppingCart set CustomerID=" + NewCustomerID.ToString() + " where CartType in (" + ((int)CartTypeEnum.GiftRegistryCart).ToString() + ") and customerid=" + CurrentCustomerID.ToString());
                    DB.ExecuteSQL("update kitcart set CustomerID=" + NewCustomerID.ToString() + " where CartType in (" + ((int)CartTypeEnum.GiftRegistryCart).ToString() + ") and customerid=" + CurrentCustomerID.ToString());
                }
            } 
            //end JH


            Customer cust = new Customer(NewCustomerID);
            ShoppingCart sc = new ShoppingCart(cust.SkinID, cust, CartTypeEnum.ShoppingCart, 0, false);
            sc.ApplyShippingRules();
            sc.ConsolidateCartItems();


            Customer.ClearAllCustomerProfile();

            Customer newCustomer = new Customer(NewCustomerID);
            switch (AppLogic.AppConfig("Signin.SkinMaster").ToLower())
            {
                case "session":
                    HttpContext.Current.Profile.SetPropertyValue("SkinID", CustSkinID.ToString());
                    int sessionSkinID;
                    if (int.TryParse(CustSkinID, out sessionSkinID) && newCustomer.SkinID != sessionSkinID)
                    {
                        newCustomer.SkinID = sessionSkinID;
                        newCustomer.UpdateCustomer(new SqlParameter[] { new SqlParameter("SkinID", sessionSkinID) });
                    }
                    break;
                case "default":
                    HttpContext.Current.Profile.SetPropertyValue("SkinID", AppLogic.DefaultSkinID().ToString());
                    if (newCustomer.SkinID != AppLogic.DefaultSkinID())
                    {
                        newCustomer.SkinID = AppLogic.DefaultSkinID();
                        newCustomer.UpdateCustomer(new SqlParameter[] { new SqlParameter("SkinID", AppLogic.DefaultSkinID()) });
                    }
                    break;
            }
        }

		

        // examines the specified option string, which should correspond to a size or color option in the product variant,
        // and returns JUST the main option text, removing any cost delta specifiers
        static public String CleanSizeColorOption(String s)
        {
            String tmp = s;
            int i = s.IndexOf("[");
            if (i > 0)
            {
                tmp = s.Substring(0, i).Trim();
            }
            return tmp.Trim();
        }

        static public void SendDistributorNotifications(Order ord)
        {
                String MailServer = AppLogic.MailServer();

                if (MailServer.Length != 0 &&
                    false == MailServer.Equals(AppLogic.ro_TBD, StringComparison.InvariantCultureIgnoreCase))
                {
                    String DistCC = AppLogic.AppConfig("DistributorEMailCC").Trim();
                    String SubjectReceipt = AppLogic.AppConfig("StoreName") + " - Distributor Notification, Order #" + ord.OrderNumber.ToString();
                    String sql = "select distinct DistributorID,EMail from Distributor  with (NOLOCK)  where DistributorID in (select distinct DistributorID from Orders_ShoppingCart where OrderNumber=" + ord.OrderNumber.ToString() + " and (DistributorID IS NOT NULL and DistributorID <> 0))";

                    using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                    {
                        con.Open();
                        using (IDataReader rs = DB.GetRS(sql, con))
                        {
                            while (rs.Read())
                            {
                                if (DB.RSFieldInt(rs, "DistributorID") != 0)
                                {
                                    String EM = DB.RSField(rs, "EMail").Trim();
                                    EM = EM.Replace(";", "|").Replace(",", "|").Replace(" ", "|");
                                    bool first = true;
                                    foreach (String emx in EM.Split('|'))
                                    {
                                        if (emx.IndexOf("shipwire", StringComparison.InvariantCultureIgnoreCase) != -1)
                                            Shipwire.SubmitOrder(ord, DB.RSFieldInt(rs, "DistributorID"));
                                        else if (emx.Length != 0)
                                        {
                                            try
                                            {
                                                AppLogic.SendMail(SubjectReceipt, ord.DistributorNotification(DB.RSFieldInt(rs, "DistributorID")) + AppLogic.AppConfig("MailFooter"), true, AppLogic.AppConfig("ReceiptEMailFrom"), AppLogic.AppConfig("ReceiptEMailFromName"), emx, emx, CommonLogic.IIF(DistCC.Length != 0, DistCC, String.Empty), MailServer);
                                                if (first)
                                                {
                                                    DB.ExecuteSQL("update Orders set DistributorEMailSentOn=getdate() where OrderNumber=" + ord.OrderNumber.ToString());
                                                }
                                                first = false;
                                            }
                                            catch { }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

        static public String GetAllDistributorNotifications(Order ord)
        {
                StringBuilder tmpS = new StringBuilder(10000);
                String SubjectReceipt = AppLogic.AppConfig("StoreName") + " - Distributor Notification, Order #" + ord.OrderNumber.ToString();
                String sql = "select distinct Name, DistributorID,EMail from Distributor  with (NOLOCK)  where DistributorID in (select distinct DistributorID from Orders_ShoppingCart where OrderNumber=" + ord.OrderNumber.ToString() + " and (DistributorID IS NOT NULL and DistributorID <> 0))";

                bool first = true;

                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS(sql, con))
                    {
                        while (rs.Read())
                        {
                            if (DB.RSFieldInt(rs, "DistributorID") != 0)
                            {
                                if (!first)
                                {
                                    tmpS.Append("<hr/>");
                                }
                                String EM = DB.RSField(rs, "EMail").Trim();
                                tmpS.Append("<p><b>Distributor Name: " + DB.RSField(rs, "Name") + "</b></p>");
                                tmpS.Append("<p><b>Distributor ID: " + DB.RSFieldInt(rs, "DistributorID").ToString() + "</b></p>");
                                tmpS.Append("<p><b>Distributor E-Mail: " + EM + "</b></p>");
                                tmpS.Append("<p><b>Distributor XmlPackage: " + ord.GetDistributorNotificationPackageToUse(DB.RSFieldInt(rs, "DistributorID")) + "</b></p>");
                                tmpS.Append("<p><b>Notification E-Mail Subject: " + SubjectReceipt + "</b></p>");
                                tmpS.Append("<p><b>Notification E-Mail Body:</b></p>");
                                tmpS.Append("<div>");
                                tmpS.Append(ord.DistributorNotification(DB.RSFieldInt(rs, "DistributorID")) + AppLogic.AppConfig("MailFooter"));
                                tmpS.Append("</div>");
                                first = false;
                            }
                        }
                    }
                }

                return tmpS.ToString();
            }

        static public void SendOrderEMail(Customer ActiveCustomer, int OrderNumber, bool IsRecurring, String PaymentMethod, bool NotifyStoreAdmin, System.Collections.Generic.Dictionary<string, EntityHelper> EntityHelpers, Parser UseParser)
        {
            Order ord = new Order(OrderNumber, ActiveCustomer.LocaleSetting);
            bool UseLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");
            String StoreName = AppLogic.AppConfig("StoreName");
            String MailServer = AppLogic.MailServer();

            String PM = AppLogic.CleanPaymentMethod(PaymentMethod);
            String OrdPM = AppLogic.CleanPaymentMethod(ord.PaymentMethod);
            String SubjectReceipt = String.Empty;
            if (UseLiveTransactions)
            {
                SubjectReceipt = String.Format(GetString("common.cs.1", ord.SkinID, ord.LocaleSetting), StoreName);
            }
            else
            {
                SubjectReceipt = String.Format(GetString("common.cs.2", ord.SkinID, ord.LocaleSetting), StoreName);
            }
            if (OrdPM == AppLogic.ro_PMRequestQuote)
            {
                SubjectReceipt += GetString("common.cs.3", ord.SkinID, ord.LocaleSetting);
            }
            String SubjectNotification = String.Empty;
            if (UseLiveTransactions)
            {
                SubjectNotification = String.Format(GetString("common.cs.4", ord.SkinID, ord.LocaleSetting), StoreName);
            }
            else
            {
                SubjectNotification = String.Format(GetString("common.cs.5", ord.SkinID, ord.LocaleSetting), StoreName);
            }
            if (OrdPM == AppLogic.ro_PMRequestQuote)
            {
                SubjectNotification += GetString("common.cs.3", ord.SkinID, ord.LocaleSetting);
            }

            if (IsRecurring)
            {
                SubjectReceipt += GetString("common.cs.6", ord.SkinID, ord.LocaleSetting);
            }

            if (NotifyStoreAdmin)
            {
                // send E-Mail notice to store admin:
                if (ord.ReceiptEMailSentOn.Equals(System.DateTime.MinValue))
                {
                    try
                    {
                        if (AppLogic.AppConfig("GotOrderEMailTo").Length != 0 && !AppLogic.AppConfigBool("TurnOffStoreAdminEMailNotifications"))
                        {
                            String SendToList = AppLogic.AppConfig("GotOrderEMailTo").Replace(",", ";");
                            if (SendToList.IndexOf(';') != -1)
                            {
                                foreach (String s in SendToList.Split(';'))
                                {
                                    AppLogic.SendMail(SubjectNotification, ord.AdminNotification() + AppLogic.AppConfig("MailFooter"), true, AppLogic.AppConfig("GotOrderEMailFrom"), AppLogic.AppConfig("GotOrderEMailFromName"), s.Trim(), s.Trim(), String.Empty, AppLogic.MailServer());
                                }
                            }
                            else
                            {
                                AppLogic.SendMail(SubjectNotification, ord.AdminNotification() + AppLogic.AppConfig("MailFooter"), true, AppLogic.AppConfig("GotOrderEMailFrom"), AppLogic.AppConfig("GotOrderEMailFromName"), SendToList, SendToList, String.Empty, AppLogic.MailServer());
                            }
                        }
                    }
                    catch { }
                }

                // send SMS notice to store admin:
                if (ord.ReceiptEMailSentOn.Equals(System.DateTime.MinValue))
                {
                    // SEND CELL MESSAGE NOTIFICATION:
                    try
                    {
                        SMS.Send(ord, AppLogic.AppConfig("ReceiptEMailFrom"), MailServer, ActiveCustomer);
                    }
                    catch { }
                }
            }

			//  now send customer e-mails:
            bool OKToSend = false;
            if (ord.BillingAddress.m_EMail != "")
            {
                if (IsRecurring)
                {
                    if (AppLogic.AppConfigBool("Recurring.SendOrderEMailToCustomer") && MailServer.Length != 0 && MailServer != AppLogic.ro_TBD)
                    {
                        OKToSend = true;
                    }
                }
                else
                {
                    if (AppLogic.AppConfigBool("SendOrderEMailToCustomer") && MailServer.Length != 0 && MailServer != AppLogic.ro_TBD)
                    {
                        OKToSend = true;
                    }
                }
            }
            if (OKToSend)
            {
                try
                {
                    // NOTE: we changed this to ALWAYS send the receipt:
                    if (ord.ReceiptEMailSentOn.Equals(System.DateTime.MinValue))
                    {
                        try
                        {
                            AppLogic.SendMail(SubjectReceipt, ord.Receipt(ActiveCustomer, true) + AppLogic.AppConfig("MailFooter"), true, AppLogic.AppConfig("ReceiptEMailFrom"), AppLogic.AppConfig("ReceiptEMailFromName"), ord.BillingAddress.m_EMail, ord.BillingAddress.m_EMail, String.Empty, MailServer);
                            DB.ExecuteSQL("update Orders set ReceiptEMailSentOn=getdate() where OrderNumber=" + ord.OrderNumber.ToString());
                        }
                        catch { }
                    }
                    String PMCleaned = AppLogic.CleanPaymentMethod(PaymentMethod);
                }
                catch { }
            }
                bool DelayTheDropShipNotification = AppLogic.AppConfigBool("DelayedDropShipNotifications");
                if (!DelayTheDropShipNotification && (AppLogic.AppConfigBool("MaxMind.Enabled") && ord.MaxMindFraudScore >= AppLogic.AppConfigNativeDecimal("MaxMind.DelayDropShipThreshold")))
                {
                    DelayTheDropShipNotification = true; // delay it anyway if maxmind fraud score is too high!
                }
                if (!DelayTheDropShipNotification && ord.TransactionIsCaptured() && ord.DistributorEMailSentOn.Equals(System.DateTime.MinValue) && ord.HasDistributorComponents())
                {
                    AppLogic.SendDistributorNotifications(new Order(ord.OrderNumber, ord.ViewInLocaleSetting)); // must reload order object to make this call work!
                }
            }

        static public String ReplaceTokens(String S, int SkinID, Customer ThisCustomer, System.Collections.Generic.Dictionary<string, EntityHelper> EntityHelpers, Parser useParser)
        {
            if (S.IndexOf("(!") == -1)
            {
                return S; // no tokens!
            }
            if (useParser == null)
            {
                useParser = new Parser(EntityHelpers, SkinID, ThisCustomer);
            }
            return useParser.ReplaceTokens(S);
        }

        static public String GetRecurringCart(System.Collections.Generic.Dictionary<string, EntityHelper> EntityHelpers, Parser UseParser, Customer ThisCustomer, int OriginalRecurringOrderNumber, int SkinID, bool OnlyLoadRecurringItemsThatAreDue)
        {
            return GetRecurringCart(EntityHelpers, UseParser, ThisCustomer, OriginalRecurringOrderNumber, SkinID, OnlyLoadRecurringItemsThatAreDue, true, false, false, String.Empty);
        }

        static public String GetRecurringCart(System.Collections.Generic.Dictionary<string, EntityHelper> EntityHelpers, Parser UseParser, Customer ThisCustomer, int OriginalRecurringOrderNumber, int SkinID, bool OnlyLoadRecurringItemsThatAreDue, bool ShowCancelButton, bool ShowRetryButton, bool ShowRestartButton, String GatewayStatus)
        {
            ShoppingCart cart = new ShoppingCart(SkinID, ThisCustomer, CartTypeEnum.RecurringCart, OriginalRecurringOrderNumber, OnlyLoadRecurringItemsThatAreDue);

            //Admins cannot manually process PayPal Express Checkout recurring orders
            Boolean isPPECOrder = false;
            Order recurringOrder = new Order(OriginalRecurringOrderNumber);
            if (recurringOrder.PaymentMethod == AppLogic.ro_PMPayPalExpress && AppLogic.IsAdminSite)
                isPPECOrder = true;

            // Need to find one of the CartItems that match the OriginalRecurringOrderNumber
            CartItem co = new CartItem();
            foreach (CartItem c in cart.CartItems)
            {
                if (c.OriginalRecurringOrderNumber == OriginalRecurringOrderNumber)
                {
                    co = c;
                    break;
                }
            }

            StringBuilder tmpS = new StringBuilder(10000);
          
            if (co.RecurringSubscriptionID != null && AppLogic.IsAdminSite && (co.NextRecurringShipDate <= System.DateTime.Now) && co.RecurringSubscriptionID.Length == 0 && !isPPECOrder)
            {
                tmpS.Append(String.Format("<div><a href=\"recurring.aspx?processCustomerID={0}&OriginalRecurringOrderNumber={1}\"><img src=\"" + AppLogic.LocateImageURL("App_Themes/skin_{2}/images/processrecurring.gif") + "\" border=\"0\" /></a></div>", ThisCustomer.CustomerID, OriginalRecurringOrderNumber, SkinID));
            }

            tmpS.Append(cart.DisplayRecurring(OriginalRecurringOrderNumber, SkinID, ShowCancelButton, ShowRetryButton, ShowRestartButton, GatewayStatus, ThisCustomer.LocaleSetting, UseParser));


            return tmpS.ToString();
        }

        public static String GetRecurringSubscriptionIDFromShoppingCart(int ShoppingCartRecID)
        {
            String RecurringSubscriptionID = String.Empty;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("Select RecurringSubscriptionID from ShoppingCart  with (NOLOCK)  where ShoppingCartRecID=" + ShoppingCartRecID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        RecurringSubscriptionID = DB.RSField(rs, "RecurringSubscriptionID");
                    }
                }
            }

            return RecurringSubscriptionID;
        }

        public static String GetRecurringSubscriptionIDFromOrder(int OrderNumber)
        {
            String RecurringSubscriptionID = String.Empty;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("Select RecurringSubscriptionID from Orders  with (NOLOCK)  where OrderNumber=" + OrderNumber.ToString(), con))
                {
                    if (rs.Read())
                    {
                        RecurringSubscriptionID = DB.RSField(rs, "RecurringSubscriptionID");
                    }
                }
            }

            return RecurringSubscriptionID;
        }

        public static int GetOriginalRecurringOrderNumberFromSubscriptionID(String RecurringSubscriptionID)
        {
            int OrderNumber = 0;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("Select top 1 OriginalRecurringOrderNumber from ShoppingCart  with (NOLOCK)  where RecurringSubscriptionID=" + DB.SQuote(RecurringSubscriptionID), con))
                {
                    if (rs.Read())
                    {
                        OrderNumber = DB.RSFieldInt(rs, "OriginalRecurringOrderNumber");
                    }
                }
            }

            return OrderNumber;
        }

        static public String GetCountryBar(String currentLocaleSetting)
        {
            return String.Empty; // this token was discountinued due to too many cross browser issues. use (!COUNTRYSELECTLIST!) instead.
        }

        static public String GetLocaleSelectList(String currentLocaleSetting)
        {
            if (AppLogic.NumLocaleSettingsInstalled() < 2)
            {
                return String.Empty;
            }
            String CacheName = "GetLocaleSelectList_" + currentLocaleSetting;
            if (AppLogic.CachingOn)
            {
                String Menu = (String)HttpContext.Current.Cache.Get(CacheName);
                if (Menu != null)
                {
                    if (CommonLogic.ApplicationBool("DumpSQL"))
                    {
                        HttpContext.Current.Response.Write("Cache Hit Found!\n");
                    }
                    return Menu;
                }
            }

            StringBuilder tmpS = new StringBuilder(4096);

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                bool hasLocales = false;
                StringBuilder options = new StringBuilder();

                con.Open();
                using (IDataReader rs = DB.GetRS("select * from LocaleSetting   with (NOLOCK)  order by displayorder,description", con))
                {
                    while (rs.Read())
                    {
                        hasLocales = true;
                        options.Append("<option value=\"" + DB.RSField(rs, "Name") + "\" " + CommonLogic.IIF(currentLocaleSetting == DB.RSField(rs, "Name"), " selected ", "") + ">" + DB.RSField(rs, "Description") + "</option>");
                    }
                }

                if (hasLocales)
                {
                    tmpS.Append("<!-- COUNTRY SELECT LIST -->\n");
                    tmpS.Append("<select size=\"1\" onChange=\"self.location='setlocale.aspx?LocaleSetting=' + document.getElementById('CountrieselectList').value\" id=\"CountrieselectList\" name=\"CountrieselectList\" class=\"CountrieselectList\">");

                    tmpS.Append(options.ToString());

                    tmpS.Append("</select>");
                    tmpS.Append("<!-- END COUNTRY SELECT LIST -->\n");
                }
            }

            if (CachingOn)
            {
                HttpContext.Current.Cache.Insert(CacheName, tmpS.ToString(), null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }

            return tmpS.ToString();
        }

        static public String GetRecurringVariantsList()
        {
            String CacheName = "GetRecurringVariantsList";
            if (AppLogic.CachingOn)
            {
                String Menu = (String)HttpContext.Current.Cache.Get(CacheName);
                if (Menu != null)
                {
                    if (CommonLogic.ApplicationBool("DumpSQL"))
                    {
                        HttpContext.Current.Response.Write("Cache Hit Found!\n");
                    }
                    return Menu;
                }
            }

            StringBuilder tmpS = new StringBuilder(4096);

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("Select variantid from productvariant  with (NOLOCK)  where IsRecurring=1 and deleted=0", con))
                {
                    while (rs.Read())
                    {
                        if (tmpS.Length != 0)
                        {
                            tmpS.Append(",");
                        }
                        tmpS.Append(DB.RSFieldInt(rs, "VariantID").ToString());
                    }
                }
            }

            if (CachingOn)
            {
                HttpContext.Current.Cache.Insert(CacheName, tmpS.ToString(), null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }
            return tmpS.ToString();
        }



        static public String AffiliateEMail(int AffiliateID)
        {
            String tmpS = String.Empty;

            if (AffiliateID != 0)
            {
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS("Select EMail from Affiliate  with (NOLOCK)  where AffiliateID=" + AffiliateID.ToString(), con))
                    {
                        if (rs.Read())
                        {
                            tmpS = DB.RSField(rs, "EMail");
                        }
                    }
                }
            }

            return tmpS;
        }

        static public String AffiliateMailingAddress(int AffiliateID, String separator)
        {
            StringBuilder tmpS = new StringBuilder(1024);

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select * from Affiliate  with (NOLOCK)  where AffiliateID=" + AffiliateID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmpS.Append((DB.RSField(rs, "FirstName") + " " + DB.RSField(rs, "LastName")).Trim() + separator);
                        if (DB.RSField(rs, "Company").Length != 0)
                        {
                            tmpS.Append(DB.RSField(rs, "Company") + separator);
                        }
                        tmpS.Append(DB.RSField(rs, "Address1") + separator);
                        if (DB.RSField(rs, "Address2").Length != 0)
                        {
                            tmpS.Append(DB.RSField(rs, "Address2") + separator);
                        }
                        if (DB.RSField(rs, "Suite").Length != 0)
                        {
                            tmpS.Append("Suite: " + DB.RSField(rs, "Suite") + separator);
                        }
                        tmpS.Append(DB.RSField(rs, "City") + ", " + DB.RSField(rs, "State") + " " + DB.RSField(rs, "Zip") + separator);
                    }
                }
            }

            return tmpS.ToString();
        }


        static public bool IsValidAffiliate(int AffiliateID)
        {
            string query = string.Format("Select count(a.AffiliateID) as N from Affiliate a with (NOLOCK) inner join(select a.AffiliateID from Affiliate a with (nolock) left join AffiliateStore b with (nolock) on a.AffiliateID = b.AffiliateID " +
                            "where ({0}= 0 or StoreID = {1})) b on a.AffiliateID = b.AffiliateID where Deleted=0 and a.AffiliateID = {2}", CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowAffiliateFiltering") == true, 1, 0), AppLogic.StoreID(), AffiliateID);
            return (DB.GetSqlN(query) != 0);
        }


        static public bool IsOnlineAffiliate(int AffiliateID)
        {
            bool tmp = false;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select IsOnline from Affiliate  with (NOLOCK)  where AffiliateID=" + AffiliateID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSFieldBool(rs, "IsOnline");
                    }
                }
            }

            return tmp;
        }

        public static String GetProductDescription(int ProductID, String LocaleSetting)
        {
            String tmpS = String.Empty;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select Description from Product  with (NOLOCK)  where ProductID=" + ProductID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmpS = DB.RSFieldByLocale(rs, "Description", LocaleSetting);
                    }
                }
            }

            if (AppLogic.ReplaceImageURLFromAssetMgr)
            {
                tmpS = tmpS.Replace("../images", "images");
            }
            return tmpS;
        }

        public static String GetProductSummary(int productID, String localeSetting)
        {
            String tmpS = String.Empty;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select Summary from Product  with (NOLOCK)  where ProductID=" + productID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmpS = DB.RSFieldByLocale(rs, "Summary", localeSetting);
                    }
                }
            }

            return tmpS;
        }

        public static String GetProductSEName(int productID, String localeSetting)
        {
            String tmpS = String.Empty;
            SqlParameter[] spa = { DB.CreateSQLParameter("@ProductID", SqlDbType.Int, 4, productID, ParameterDirection.Input) };

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select SEName from Product  with (NOLOCK)  where ProductID = @ProductID", spa, con))
                {
                    if (rs.Read())
                    {
                        tmpS = DB.RSFieldByLocale(rs, "SEName", localeSetting);
                    }
                }
            }

            return tmpS;
        }

        public static String GetVariantSEName(int variantID, String localeSetting)
        {
            String tmpS = String.Empty;
            SqlParameter[] spa = { DB.CreateSQLParameter("@VariantID", SqlDbType.Int, 4, variantID, ParameterDirection.Input) };

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select SEName from ProductVariant  with (NOLOCK)  where VariantID = @VariantID", spa, con))
                {
                    if (rs.Read())
                    {
                        tmpS = DB.RSFieldByLocale(rs, "SEName", localeSetting);
                    }
                }
            }

            return tmpS;
        }

        public static String GetEntitySEName(int EntityID, String entityType, string localeSetting)
        {
            return GetEntitySEName(EntityID, (AppLogic.EntityType)Enum.Parse(typeof(AppLogic.EntityType), entityType, true), localeSetting);
        }

        public static String GetEntitySEName(int EntityID, EntityType entityType, String localeSetting)
        {
            String tmpS = String.Empty;
            SqlParameter[] spa = 
            { 
                new SqlParameter("@EntityID", EntityID)
            };

            if (entityType == EntityType.Affiliate || entityType == EntityType.CustomerLevel || entityType == EntityType.Unknown)
            {
                throw new ArgumentException("Unsupported EntityType", "entityType");
            }
            
            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select SEName from " + entityType.ToString() + " with (NOLOCK) where " + entityType.ToString() + "ID = @EntityID", spa, con))
                {
                    if (rs.Read())
                    {
                        tmpS = DB.RSFieldByLocale(rs, "SEName", localeSetting);
                    }
                }
            }

            return tmpS;
        }

        static public String GetCAVV(int OrderNumber)
        {
            String tmpS = String.Empty;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("Select CardinalAuthenticateResult from Orders  with (NOLOCK)  where OrderNumber=" + OrderNumber.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmpS = CommonLogic.ExtractToken(DB.RSField(rs, "CardinalAuthenticateResult"), "<Cavv>", "</Cavv>");
                    }
                }
            }

            return tmpS;
        }

        static public int GetNextOrderNumber()
        {
            String NewGUID = CommonLogic.GetNewGUID();
            DB.ExecuteSQL("insert into OrderNumbers(OrderNumberGUID) values(" + DB.SQuote(NewGUID) + ")");
            int tmp = 0;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("Select OrderNumber from OrderNumbers  with (NOLOCK)  where OrderNumberGUID=" + DB.SQuote(NewGUID), con))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSFieldInt(rs, "ordernumber");
                    }
                }
            }

            return tmp;
        }

        static public String TransactionMode()
        {
            String tmpS = AppLogic.AppConfig("TransactionMode").Trim().ToUpperInvariant();
            if (tmpS.Length == 0)
            {
                tmpS = AppLogic.ro_TXModeAuthOnly; // forcefully set SOME default!
            }
            return tmpS;
        }

        static public bool TransactionModeIsAuthCapture()
        {
            return TransactionMode() != AppLogic.ro_TXModeAuthOnly;
        }

        static public bool TransactionModeIsAuthOnly()
        {
            return !TransactionModeIsAuthCapture();
        }

        static public Hashtable LoadAppConfigsFromDB()
        {
            Hashtable ht = new Hashtable();

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("Select * from AppConfig with (NOLOCK)", con))
                {
                    while (rs.Read())
                    {
                        String key = DB.RSField(rs, "Name");
                        // ignore dups, first one in wins:
                        if (!ht.Contains(key.ToLowerInvariant()))
                        {
                            // undocumented feature: allow web.config to override appconfig parm:
                            String theVal = CommonLogic.Application(key);
                            if (theVal.Length == 0)
                            {
                                theVal = DB.RSField(rs, "ConfigValue");
                            }
                            ht.Add(key.ToLowerInvariant(), theVal);
                        }
                    }
                }
            }

            return ht;
        }

        /// <summary>
        /// Gets the default store
        /// </summary>
        /// <returns></returns>
        public static Store GetDefaultStore()
        {
            Store defStore = null;

            try
            {
                var stores = Store.GetStoreList();
                defStore = stores.FirstOrDefault(store => store.IsDefault);
            }
            catch (Exception ex)
            {
                var logEx = new Exception("Default store not found", ex);
                SysLog.LogException(logEx, MessageTypeEnum.GeneralException, MessageSeverityEnum.Message);
            }

            return defStore;
        }

        static public bool AnyCustomerHasUsedCoupon(String CouponCode)
        {
            return (DB.GetSqlN(string.Format("select count(ordernumber) as N from orders  with (NOLOCK)  where CouponCode= {0}", DB.SQuote(CouponCode))) != 0);
        }

        static public int GetNumberOfCouponUses(String CouponCode)
        {
            int tmp = 0;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("Select NumUses from coupon with (NOLOCK) where CouponCode=" + DB.SQuote(CouponCode), con))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSFieldInt(rs, "NumUses");
                    }
                }
            }

            return tmp;
        }

        static public bool MicropayIsEnabled()
        {
            return AppLogic.AppConfig("PaymentMethods").IndexOf(AppLogic.ro_PMMicropay, StringComparison.InvariantCultureIgnoreCase) != -1;
        }

        static public void RecordCouponUsage(int CustomerID, String CouponCode)
        {
            if (CouponCode.Length != 0)
            {
                try
                {
                    DB.ExecuteSQL("update coupon set NumUses=NumUses+1 where lower(CouponCode)=" + DB.SQuote(CouponCode.ToLowerInvariant()));
                    DB.ExecuteSQL("insert into CouponUsage(CustomerID,CouponCode) values(" + CustomerID.ToString() + "," + DB.SQuote(CouponCode) + ")");
                }
                catch { }
            }
        }


        /// <summary>
        /// Mask the specifc value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Mask(string value)
        {
            return Mask(value, 4);
        }


        /// <summary>
        ///  Mask the specifc value
        /// </summary>
        /// <param name="value"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string Mask(string value, int length)
        {
            string masked = "****";
            if (!CommonLogic.IsStringNullOrEmpty(value) &&
                value.Length > length)
            {
                masked += value.Substring(value.Length - length, length);
            }

            return masked;
        }

        /// <summary>
        /// Gets the Theme aware skin image directory
        /// </summary>
        /// <returns></returns>
        public static string SkinImageDir()
        {
            var handler = HttpContext.Current.Handler;
            if (handler != null && handler is Page)
            {
                return "App_Themes/{0}/images".FormatWith((handler as Page).Theme);
            }
            else
            {
                Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)HttpContext.Current.User).ThisCustomer;
                return "App_Themes/Skin_{0}/images".FormatWith(ThisCustomer.SkinID.ToString());
            }
        }

        /// <summary>
        /// Gets the relative image path from the App_Themes/Skin_{current} directory for the specified image
        /// </summary>
        /// <param name="fileName">The image file name</param>
        /// <returns>The relative path from the themes/image directory</returns>
        public static string SkinImage(string fileName)
        {
            return AppLogic.LocateImageURL(ResolveUrl("~/{0}/{1}".FormatWith(AppLogic.SkinImageDir(), fileName)));
        }

        /// <summary>
        /// Determines if  product is out of stock base on the inventory and OutOfStockThreshold appconfig
        /// </summary>
        /// <param name="productId">The product id</param>
        /// <param name="variantId">The variant id</param>
        /// <param name="TrackInventoryBySizeAndColor"></param>
        /// <returns>True or False</returns>
        public static bool ProbablyOutOfStock(int productId, int variantId, bool trackInventoryBySizeAndColor)
        {
            return ProbablyOutOfStock(productId, variantId, trackInventoryBySizeAndColor, "Product");
        }


        /// <summary>
        /// Determines if  product is out of stock base on the inventory and OutOfStockThreshold appconfig
        /// </summary>
        /// <param name="productId">The product id</param>
        /// <param name="variantId">The variant id</param>
        /// <param name="trackInventoryBySizeAndColor">Determine if inventroy is track by sizes and color</param>
        /// <param name="page"></param>
        /// <returns>True or False</returns>
		public static bool ProbablyOutOfStock(int productId, int variantId, bool trackInventoryBySizeAndColor, string page)
		{
			if (AppLogic.IsAKit(productId))
			{
				KitProductData kit = KitProductData.Find(productId, Customer.Current);
				return !kit.HasStock;
			}
			else
			{
				return DetermineOutOfStock(productId, variantId, trackInventoryBySizeAndColor, page);
			}
		}
		
        public static bool DetermineOutOfStock(int productId, int variantId, bool trackInventoryBySizeAndColor)
        {
            return DetermineOutOfStock(productId, variantId, trackInventoryBySizeAndColor, "Product");
        }

        public static bool DetermineOutOfStock(int productId, int variantId, bool trackInventoryBySizeAndColor, string page)
        {
            int inventoryLevel = 0;

            if (trackInventoryBySizeAndColor)//this will query the inventory base on total quantity of the attribute for variant
            {
                if (page.Equals("Entity", StringComparison.InvariantCultureIgnoreCase) || variantId == OUT_OF_STOCK_ALL_VARIANTS)
                {
                    using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
                    {
                        StringBuilder st = new StringBuilder();
                        st.Append("SELECT SUM(Inventory.Quan) AS TotalQuantity ");
                        st.Append("FROM Inventory with (NOLOCK) INNER JOIN ProductVariant ON Inventory.VariantID = ProductVariant.VariantID INNER JOIN ");
                        st.Append("Product ON ProductVariant.ProductID = Product.ProductID WHERE Product.ProductID =" + productId.ToString());
                        conn.Open();
                        using (IDataReader bySizesAndColorReader = DB.GetRS(st.ToString(), conn))
                        {
                            if (bySizesAndColorReader.Read())
                            {
                                inventoryLevel = DB.RSFieldInt(bySizesAndColorReader, "TotalQuantity");
                            }
                        }
                    }
                }
                else
                {
                    using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
                    {
                        conn.Open();
                        using (IDataReader bySizesAndColorReader = DB.GetRS("SELECT SUM(Quan) as TotalQuantity from Inventory  WITH (NOLOCK)  WHERE VariantID=" + variantId.ToString(), conn))
                        {
                            if (bySizesAndColorReader.Read())
                            {
                                inventoryLevel = DB.RSFieldInt(bySizesAndColorReader, "TotalQuantity");
                            }
                        }
                    }
                }

            }
            else if (page.Equals("Entity", StringComparison.InvariantCultureIgnoreCase) || variantId == OUT_OF_STOCK_ALL_VARIANTS)//this will query the total inventory of variants base on product id
            {
                using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
                {
                    conn.Open();
                    using (IDataReader allVariantOfProductReader = DB.GetRS("SELECT SUM(Inventory) as Total from ProductVariant  WITH (NOLOCK)  WHERE ProductId =" + productId.ToString(), conn))
                    {
                        if (allVariantOfProductReader.Read())
                        {
                            inventoryLevel = DB.RSFieldInt(allVariantOfProductReader, "Total");
                        }
                    }
                }

            }
            else //this will query the inventory per variant
            {
                using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
                {
                    conn.Open();
                    using (IDataReader inventoryReader = DB.GetRS("SELECT Inventory FROM ProductVariant  WITH (NOLOCK)  WHERE VariantID=" + variantId.ToString(), conn))
                    {
                        if (inventoryReader.Read())
                        {
                            inventoryLevel = DB.RSFieldInt(inventoryReader, "Inventory");
                        }
                    }
                }
            }

            //If OutOfStockThreshold is greater than inventory it will be consider out of stock
            if (AppLogic.AppConfigNativeInt("OutOfStockThreshold") > inventoryLevel)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the inventory info.
        /// </summary>
        /// <param name="ProductID">The Product id</param>
        /// <param name="VariantID">The variant id</param>
        /// <param name="ShowActualValues">If true will show the actual value eg. 100 , if false 'yes' or 'no' depending on the stringresource</param>
        /// <param name="SkinID">The skin id</param>
        /// <param name="IncludeFrame">If include border on inventory table</param>
        /// <param name="ForEdit">This is use by admin page to edit the inventory</param>
        /// <returns>HTML of inventory info</returns>
        static public String GetInventoryTable(int ProductID, int VariantID, bool ShowActualValues, int SkinID, bool IncludeFrame, bool ForEdit)
        {
            if (ForEdit)
            {
                ShowActualValues = true;
            }

            StringBuilder tmpS = new StringBuilder(10000);
            tmpS.Append("<div class=\"inventory-table-wrap\">");
            bool ProductTracksInventoryBySizeAndColor = AppLogic.ProductTracksInventoryBySizeAndColor(ProductID);
            if (ProductTracksInventoryBySizeAndColor)
            {
                bool ProductTracksInventoryBySize = AppLogic.ProductTracksInventoryBySize(ProductID);
                bool ProductTracksInventoryByColor = AppLogic.ProductTracksInventoryByColor(ProductID);

                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS("select * from productvariant   with (NOLOCK)  where VariantID=" + VariantID.ToString(), con))
                    {
                        rs.Read();
                        String ProductSKU = AppLogic.GetProductSKU(ProductID);
                        String VariantSKU = AppLogic.GetVariantSKUSuffix(VariantID);

                        Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)HttpContext.Current.User).ThisCustomer;
                        String SizesDisplay = DB.RSFieldByLocale(rs, "Sizes", ThisCustomer.LocaleSetting).Trim();
                        String ColorsDisplay = DB.RSFieldByLocale(rs, "Colors", ThisCustomer.LocaleSetting).Trim();

                        String Sizes = DB.RSFieldByLocale(rs, "Sizes", Localization.GetDefaultLocale());
                        String Colors = DB.RSFieldByLocale(rs, "Colors", Localization.GetDefaultLocale());

                        if (SizesDisplay.Length == 0)
                        {
                            SizesDisplay = Sizes;
                        }

                        if (ColorsDisplay.Length == 0)
                        {
                            ColorsDisplay = Colors;
                        }

                        if (!ProductTracksInventoryBySize)
                        {
                            Sizes = String.Empty;
                        }
                        if (!ProductTracksInventoryByColor)
                        {
                            Colors = String.Empty;
                        }

                        String[] ColorsSplit = Colors.Split(',');
                        String[] SizesSplit = Sizes.Split(',');

                        String[] ColorsDisplaySplit = ColorsDisplay.Split(',');
                        String[] SizesDisplaySplit = SizesDisplay.Split(',');

                        tmpS.Append("<table class=\"table table-striped inventory-table\">\n");
                        tmpS.Append("<tr class=\"table-header\">\n");
                        tmpS.Append("<th>");
                        if (!ForEdit)
                        {
                            tmpS.Append(AppLogic.GetString("common.cs.83", SkinID, Thread.CurrentThread.CurrentUICulture.Name));
                        }
                        tmpS.Append("</th>\n");
                        for (int i = SizesSplit.GetLowerBound(0); i <= SizesSplit.GetUpperBound(0); i++)
                        {
                            tmpS.Append("<th>" + AppLogic.CleanSizeColorOption(SizesDisplaySplit[i]) + "</th>\n");
                        }
                        tmpS.Append("</tr>\n");
                        int FormFieldID = 1000; // arbitrary number
                        for (int i = ColorsSplit.GetLowerBound(0); i <= ColorsSplit.GetUpperBound(0); i++)
                        {
                            tmpS.Append("<tr class=\"table-row\">\n");
                            tmpS.Append("<td>" + AppLogic.CleanSizeColorOption(ColorsDisplaySplit[i]) + "</td>\n");
                            for (int j = SizesSplit.GetLowerBound(0); j <= SizesSplit.GetUpperBound(0); j++)
                            {
                                tmpS.Append("<td>");
                                int iVal = AppLogic.GetInventory(ProductID, VariantID, CleanSizeColorOption(SizesSplit[j]), CleanSizeColorOption(ColorsSplit[i]), ProductTracksInventoryBySizeAndColor, ProductTracksInventoryByColor, ProductTracksInventoryBySize);
                                if (ForEdit)
                                {
                                    String fldName = "sizecolor|" + ProductID.ToString() + "|" + VariantID.ToString() + "|" + CleanSizeColorOption(SizesSplit[j]) + "|" + CleanSizeColorOption(ColorsSplit[i]);
                                    if (AppLogic.IsAdminSite)
                                    {
                                        tmpS.Append("<input type=\"text\" class=\"form-control\" id=\"" + fldName + "\" name=\"" + fldName + "\" value=\"" + iVal.ToString() + "\">");
                                    }
                                    else
                                    {
                                        tmpS.Append("<input type=\"text\" id=\"" + fldName + "\" name=\"" + fldName + "\" class=\"form-control\" value=\"" + iVal.ToString() + "\">");
                                    }
                                }
                                else
                                {
                                    if (ShowActualValues)
                                    {
                                        tmpS.Append(iVal);
                                    }
                                    else
                                    {
                                        tmpS.Append(CommonLogic.IIF(iVal > 0, GetString("common.cs.28", SkinID, Thread.CurrentThread.CurrentUICulture.Name), GetString("common.cs.29", SkinID, Thread.CurrentThread.CurrentUICulture.Name)));
                                    }
                                }
                                FormFieldID++;
                                tmpS.Append("</td>\n");
                            }
                            tmpS.Append("</tr>\n");
                        }

                        tmpS.Append("</table>\n");
                    }
                }
            }
            else
            {
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS("Select Inventory from ProductVariant  with (NOLOCK)  where VariantID=" + VariantID.ToString(), con))
                    {
                        if (rs.Read())
                        {
                            int iVal = DB.RSFieldInt(rs, "Inventory");
                            if (ForEdit)
                            {
                                String fldName = "simple|" + ProductID.ToString() + "|" + VariantID.ToString() + "||"; // size and color are blank here to make all fields have 5 parts
                                if (AppLogic.IsAdminSite)
                                {
                                    tmpS.Append("<input type=\"text\" class=\"form-control\" id=\"" + fldName + "\" name=\"" + fldName + "\" value=\"" + iVal.ToString() + "\">");
                                }
                                else
                                {
                                    tmpS.Append("<input type=\"text\" id=\"" + fldName + "\" name=\"" + fldName + "\" class=\"form-control\" value=\"" + iVal.ToString() + "\">");
                                }
                            }
                            else
                            {
                                bool displayOutOfStockProductOnProductPage = AppLogic.AppConfigBool("DisplayOutOfStockProducts") && AppLogic.AppConfigBool("DisplayOutOfStockOnProductPages");

                                //This will use the stringresource for out stock and instock that is set in inventory.aspx
                                if (displayOutOfStockProductOnProductPage)
                                {
                                    //define the stock message and css that will be use
                                    if (ProbablyOutOfStock(ProductID, VariantID, false))
                                    {
										tmpS.Append("<span class='stock-hint out-stock-hint'>\n");
                                        tmpS.Append(AppLogic.GetString("OutofStock.DisplayOutOfStockOnProductPage", SkinID, Thread.CurrentThread.CurrentUICulture.Name));
                                    }
                                    else
                                    {
                                        string messageInStockOnProductPage = AppLogic.GetString("OutofStock.DisplayInStockOnProductPage", SkinID, Thread.CurrentThread.CurrentUICulture.Name);

                                        //We will use span to set the css
										tmpS.Append("<span class='stock-hint in-stock-hint'>\n");

                                        if (messageInStockOnProductPage != string.Empty)
                                        {
                                            if (ShowActualValues)
                                            {
                                                tmpS.Append(messageInStockOnProductPage);
                                                tmpS.Append(":" + iVal.ToString());
                                            }
                                            else
                                            {
                                                tmpS.Append(messageInStockOnProductPage);
                                            }
                                        }
                                    }

                                    tmpS.Append("</span>");
                                }
                                else
                                {
									bool inStock = iVal > 0;
									string stockClass = inStock ? "in-stock-hint" : "out-stock-hint";
									tmpS.Append(String.Format("<span class='stock-hint {0}'>{1}", stockClass, AppLogic.GetString("showproduct.aspx.25", SkinID, Thread.CurrentThread.CurrentUICulture.Name)));
									tmpS.Append("<span class='stock-hint-value'>");
                                    if (ShowActualValues)// Actual value eg. 100
                                        tmpS.Append(iVal.ToString());
                                    else // value eg. Yes or No 
                                        tmpS.Append(CommonLogic.IIF(iVal > 0, GetString("common.cs.28", SkinID, Thread.CurrentThread.CurrentUICulture.Name), GetString("common.cs.29", SkinID, Thread.CurrentThread.CurrentUICulture.Name)));
									tmpS.Append("</span></span>");
                                }
                            }
                        }
                    }
                }
            }
            tmpS.Append("</div>");
            return tmpS.ToString();
        }

        static public String GetInventoryList(int ProductID, int VariantID)
        {
            String CacheName = "GetInventoryList_" + ProductID.ToString() + "_" + VariantID.ToString();
            if (AppLogic.CachingOn && !AppLogic.IsAdminSite)
            {
                String Menu = (String)HttpContext.Current.Cache.Get(CacheName);
                if (Menu != null)
                {
                    if (CommonLogic.ApplicationBool("DumpSQL"))
                    {
                        HttpContext.Current.Response.Write("Cache Hit Found!\n");
                    }
                    return Menu;
                }
            }
            bool ProductTracksInventoryBySize = AppLogic.ProductTracksInventoryBySize(ProductID);
            bool ProductTracksInventoryByColor = AppLogic.ProductTracksInventoryByColor(ProductID);

            StringBuilder tmpS = new StringBuilder(10000);

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select * from productvariant   with (NOLOCK)  where VariantID=" + VariantID.ToString(), con))
                {
                    rs.Read();

                    String ProductSKU = AppLogic.GetProductSKU(ProductID);
                    String VariantSKU = AppLogic.GetVariantSKUSuffix(VariantID);

                    String Sizes = DB.RSFieldByLocale(rs, "Sizes", Localization.GetDefaultLocale());
                    String Colors = DB.RSFieldByLocale(rs, "Colors", Localization.GetDefaultLocale());

                    if (!ProductTracksInventoryBySize)
                    {
                        Sizes = String.Empty;
                    }
                    if (!ProductTracksInventoryByColor)
                    {
                        Colors = String.Empty;
                    }

                    String[] ColorsSplit = Colors.Split(',');
                    String[] SizesSplit = Sizes.Split(',');

                    bool first = true;
                    for (int i = ColorsSplit.GetLowerBound(0); i <= ColorsSplit.GetUpperBound(0); i++)
                    {
                        for (int j = SizesSplit.GetLowerBound(0); j <= SizesSplit.GetUpperBound(0); j++)
                        {
                            int qty = AppLogic.GetInventory(ProductID, VariantID, CleanSizeColorOption(SizesSplit[j]), CleanSizeColorOption(ColorsSplit[i]));
                            if (!first)
                            {
                                tmpS.Append("|");
                            }
                            tmpS.Append(CleanSizeColorOption(ColorsSplit[i]) + "," + CleanSizeColorOption(SizesSplit[j]) + "," + qty.ToString());
                            first = false;
                        }
                        first = false;
                    }
                }
            }

            if (AppLogic.CachingOn && !AppLogic.IsAdminSite)
            {
                HttpContext.Current.Cache.Insert(CacheName, tmpS.ToString(), null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }
            return tmpS.ToString();
        }

        static public String GetInventoryList(int ProductID, int VariantID, bool ProductTracksInventoryByColor, bool ProductTracksInventoryBySize, String ProductSKU, String VariantSKU, String Sizes, String Colors)
        {
            String CacheName = "GetInventoryList_" + ProductID.ToString() + "_" + VariantID.ToString();
            if (AppLogic.CachingOn && !AppLogic.IsAdminSite)
            {
                String Menu = (String)HttpContext.Current.Cache.Get(CacheName);
                if (Menu != null)
                {
                    if (CommonLogic.ApplicationBool("DumpSQL"))
                    {
                        HttpContext.Current.Response.Write("Cache Hit Found!\n");
                    }
                    return Menu;
                }
            }
            StringBuilder tmpS = new StringBuilder(10000);

            if (!ProductTracksInventoryBySize)
            {
                Sizes = String.Empty;
            }
            if (!ProductTracksInventoryByColor)
            {
                Colors = String.Empty;
            }

            String[] ColorsSplit = Colors.Split(',');
            String[] SizesSplit = Sizes.Split(',');

            bool first = true;
            for (int i = ColorsSplit.GetLowerBound(0); i <= ColorsSplit.GetUpperBound(0); i++)
            {
                for (int j = SizesSplit.GetLowerBound(0); j <= SizesSplit.GetUpperBound(0); j++)
                {
                    int qty = AppLogic.GetInventory(ProductID, VariantID, CleanSizeColorOption(SizesSplit[j]), CleanSizeColorOption(ColorsSplit[i]), ProductTracksInventoryByColor, ProductTracksInventoryByColor, ProductTracksInventoryBySize);
                    if (!first)
                    {
                        tmpS.Append("|");
                    }
                    tmpS.Append(CleanSizeColorOption(ColorsSplit[i]) + "," + CleanSizeColorOption(SizesSplit[j]) + "," + qty.ToString());
                    first = false;
                }
                first = false;
            }

            if (AppLogic.CachingOn && !AppLogic.IsAdminSite)
            {
                HttpContext.Current.Cache.Insert(CacheName, tmpS.ToString(), null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }
            return tmpS.ToString();
        }

        static public int GetProductManufacturerID(int ProductID)
        {
            int tmp = 0;
            if (ProductID != 0)
            {
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS("select ManufacturerID from ProductManufacturer  with (NOLOCK)  where ProductID=" + ProductID.ToString(), con))
                    {
                        if (rs.Read())
                        {
                            tmp = DB.RSFieldInt(rs, "ManufacturerID");
                        }
                    }
                }
            }
            return tmp;
        }


        static public int GetProductDistributorID(int ProductID)
        {
            int tmp = 0;
            if (ProductID != 0)
            {
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS("select DistributorID from ProductDistributor  with (NOLOCK)  where ProductID=" + ProductID.ToString(), con))
                    {
                        if (rs.Read())
                        {
                            tmp = DB.RSFieldInt(rs, "DistributorID");
                        }
                    }
                }
            }
            return tmp;
        }


		[Obsolete("deprecated (9.4.0.0) in favor of the GetProductsDefaultVariantID method")]
        static public int GetProductsFirstVariantID(int ProductID)
        {
            int tmp = 0;
            if (ProductID != 0)
            {
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS("select min(VariantID) as VID from ProductVariant  with (NOLOCK)  where Deleted=0 and Published=1 and ProductID=" + ProductID.ToString(), con))
                    {
                        if (rs.Read())
                        {
                            tmp = DB.RSFieldInt(rs, "VID");
                        }
                    }
                }
            }
            return tmp;
        }

        static public int GetProductsDefaultVariantID(int ProductID)
        {
            int tmp = 0;
            if (ProductID != 0)
            {
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS("select VariantID from ProductVariant  with (NOLOCK)  where Deleted=0 and Published=1 and IsDefault=1 and ProductID=" + ProductID.ToString(), con))
                    {
                        if (rs.Read())
                        {
                            tmp = DB.RSFieldInt(rs, "VariantID");
                        }
                    }
                }
            }
            return tmp;
        }

        static public void ProcessKitForm(Customer ThisCustomer, int ProductID)
        {
            string sql = string.Empty;
            ThisCustomer.RequireCustomerRecord();
            int VariantID = AppLogic.GetDefaultProductVariant(ProductID);
            bool FromCart = (CommonLogic.FormUSInt("CartRecID") > 0);
            string CartRecID = CommonLogic.FormCanBeDangerousContent("CartRecID");
            if (FromCart)
            {
                DB.ExecuteSQL("update KitCart set ShoppingCartRecID=-ShoppingCartRecID where ShoppingCartRecID=" + CartRecID);
            }
            else
            {
                DB.ExecuteSQL("delete from KitCart where CustomerID=" + ThisCustomer.CustomerID.ToString() + " and ProductID=" + ProductID.ToString() + " and ShoppingCartRecID=0");
            }

            for (int i = 0; i <= HttpContext.Current.Request.Form.Count - 1; i++)
            {
                if (HttpContext.Current.Request.Form.Keys[i].StartsWith("kitgroupid", StringComparison.InvariantCultureIgnoreCase))
                {
                    int thisID = Localization.ParseUSInt(HttpContext.Current.Request.Form.Keys[i].Split('_')[1]);
                    if (HttpContext.Current.Request.Form.Keys[i].IndexOf("TextOption_") != -1)
                    {
                        String thisVal = Localization.ParseUSInt(HttpContext.Current.Request.Form.Keys[i].Split('_')[3]).ToString();
                        string t = CommonLogic.FormCanBeDangerousContent(HttpContext.Current.Request.Form.Keys[i]);
                        //MOD by Mike 
                        //2006-11-15 19:48 GMT+1
                        //ISSUE:Kit always sum up text option price (if price delta) even if its empty
                        //SOLUTION: Check if text field is empty
                        if (t.Trim().Length > 0)
                        {
                            if (FromCart)
                            {
                                sql = "update kitcart set ShoppingCartRecID=" + CartRecID + ", TextOption = " + DB.SQuote(t) + " where ShoppingCartRecID=-" + CartRecID + " and kitItemID=" + thisVal;
                            }
                            else
                            {
                                sql = "insert into kitcart(customerID,ProductID,VariantID,KitGroupID,KitGroupTypeID,KitItemID,CartType,TextOption, InventoryVariantID, InventoryVariantColor, InventoryVariantSize) ";
                                sql += "select " + ThisCustomer.CustomerID.ToString() + "," + ProductID.ToString() + "," + VariantID.ToString() + "," + thisID.ToString() + ",kg.KitGroupTypeID," + thisVal + "," + ((int)CartTypeEnum.ShoppingCart).ToString() + "," + DB.SQuote(t) + ", ki.InventoryVariantID, ki.InventoryVariantColor, ki.InventoryVariantSize from dbo.kitgroup kg join dbo.KitItem ki on  kg.KitGroupID = ki.KitGroupID  where kg.KitGroupID = " + thisID.ToString() + " and ki.KitItemID = " + thisVal;
                            }
                            DB.ExecuteSQL(sql);
                        }
                    }
                    else if (HttpContext.Current.Request.Form.Keys[i].IndexOf("FileOption_") != -1)
                    {

                        String thisVal = Localization.ParseUSInt(HttpContext.Current.Request.Form.Keys[i].Split('_')[3]).ToString();

                        String useID = thisVal;
                        HttpPostedFile ImageFile = HttpContext.Current.Request.Files["FileName_" + useID];
                        if (ImageFile != null && ImageFile.ContentLength != 0)
                        {
                            String FN = ThisCustomer.CustomerID.ToString() + "_" + ProductID.ToString() + "_" + useID + "_" + DateTime.Now.ToString("yyyyMMddHHmmss");
                            String SaveName = AppLogic.GetImagePath("Orders", String.Empty, true) + FN;
                            String ImageURL = AppLogic.GetImagePath("Orders", String.Empty, false) + FN;
                            // delete any current image file first
                            try
                            {
                                if (FN.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase) || FN.EndsWith(".gif", StringComparison.InvariantCultureIgnoreCase) || FN.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    System.IO.File.Delete(AppLogic.GetImagePath("Orders", String.Empty, true) + FN);
                                }
                                else
                                {
                                    System.IO.File.Delete(AppLogic.GetImagePath("Orders", String.Empty, true) + FN + ".jpg");
                                    System.IO.File.Delete(AppLogic.GetImagePath("Orders", String.Empty, true) + FN + ".gif");
                                    System.IO.File.Delete(AppLogic.GetImagePath("Orders", String.Empty, true) + FN + ".png");
                                }
                            }
                            catch
                            { }
                            String s = ImageFile.ContentType;
                            String ImageExt = String.Empty;
                            switch (ImageFile.ContentType)
                            {
                                case "image/gif":
                                    ImageExt = ".gif";
                                    break;
                                case "image/x-png":
                                    ImageExt = ".png";
                                    break;
                                case "image/jpg":
                                case "image/jpeg":
                                case "image/pjpeg":
                                    ImageExt = ".jpg";
                                    break;
                            }
                            SaveName += ImageExt;
                            ImageURL += ImageExt;
                            if (ImageExt.Length > 0)
                            {
                                ImageFile.SaveAs(SaveName);
                                if (FromCart)
                                {
                                    sql = "update kitcart set ShoppingCartRecID=" + CartRecID + ", TextOption = " + DB.SQuote(ImageURL) + " where ShoppingCartRecID=-" + CartRecID + " and kitItemID=" + thisVal;
                                }
                                else
                                {
                                    /*
                                    sql = "insert into kitcart(customerID,ProductID,VariantID,KitGroupID,KitGroupTypeID,KitItemID,CartType,TextOption) ";
                                    sql += "select " + ThisCustomer.CustomerID.ToString() + "," + ProductID.ToString() + "," + VariantID.ToString() + "," + thisID.ToString() + ",KitGroupTypeID," + thisVal + "," + ((int)CartTypeEnum.ShoppingCart).ToString() + "," + DB.SQuote(ImageURL) + " from dbo.kitgroup where KitGroupID = " + thisID.ToString();
                                    */
                                    sql = "insert into kitcart(customerID,ProductID,VariantID,KitGroupID,KitGroupTypeID,KitItemID,CartType,TextOption, InventoryVariantID, InventoryVariantColor, InventoryVariantSize) ";
                                    sql += "select " + ThisCustomer.CustomerID.ToString() + "," + ProductID.ToString() + "," + VariantID.ToString() + "," + thisID.ToString() + ",kg.KitGroupTypeID," + thisVal + "," + ((int)CartTypeEnum.ShoppingCart).ToString() + "," + DB.SQuote(ImageURL) + ", ki.InventoryVariantID, ki.InventoryVariantColor, ki.InventoryVariantSize from dbo.kitgroup kg join dbo.KitItem ki on  kg.KitGroupID = ki.KitGroupID  where kg.KitGroupID = " + thisID.ToString() + " and ki.KitItemID = " + thisVal;
                                }
                                DB.ExecuteSQL(sql);
                            }
                        }
                        else if (HttpContext.Current.Request.Form["FileName_" + useID] != null)
                        {
                            if (FromCart)
                            {
                                sql = "update kitcart set ShoppingCartRecID=" + CartRecID + ", TextOption = " + DB.SQuote(HttpContext.Current.Request.Form["Filename_" + useID]) + " where ShoppingCartRecID=-" + CartRecID + " and kitItemID=" + thisVal;
                            }
                            else
                            {
                                /*
                                sql = "insert into kitcart(customerID,ProductID,VariantID,KitGroupID,KitGroupTypeID,KitItemID,CartType,TextOption) ";
                                sql += "select " + ThisCustomer.CustomerID.ToString() + "," + ProductID.ToString() + "," + VariantID.ToString() + "," + thisID.ToString() + ",KitGroupTypeID," + thisVal + "," + ((int)CartTypeEnum.ShoppingCart).ToString() + "," + DB.SQuote(HttpContext.Current.Request.Form["Filename_" + useID]) + " from dbo.kitgroup where KitGroupID = " + thisID.ToString();
                                */
                                sql = "insert into kitcart(customerID,ProductID,VariantID,KitGroupID,KitGroupTypeID,KitItemID,CartType,TextOption, InventoryVariantID, InventoryVariantColor, InventoryVariantSize) ";
                                sql += "select " + ThisCustomer.CustomerID.ToString() + "," + ProductID.ToString() + "," + VariantID.ToString() + "," + thisID.ToString() + ",kg.KitGroupTypeID," + thisVal + "," + ((int)CartTypeEnum.ShoppingCart).ToString() + "," + DB.SQuote(HttpContext.Current.Request.Form["Filename_" + useID]) + ", ki.InventoryVariantID, ki.InventoryVariantColor, ki.InventoryVariantSize from dbo.kitgroup kg join dbo.KitItem ki on  kg.KitGroupID = ki.KitGroupID  where kg.KitGroupID = " + thisID.ToString() + " and ki.KitItemID = " + thisVal;
                            }
                            DB.ExecuteSQL(sql);
                        }


                    }
                    else
                    {
                        String thisVal = Localization.ParseUSInt(CommonLogic.FormCanBeDangerousContent(HttpContext.Current.Request.Form.Keys[i])).ToString();

                        if (FromCart)
                        {
                            sql = "update kitcart set ShoppingCartRecID=" + CartRecID + " where ShoppingCartRecID=-" + CartRecID + " and kitItemID=" + thisVal + "\n";
                            sql += "if @@rowcount=0" + "\n";
                            sql += "insert into kitcart(ShoppingCartRecID, customerID,ProductID,VariantID,KitGroupID,KitGroupTypeID, KitItemID,CartType) ";
                            sql += "select " + CartRecID + "," + ThisCustomer.CustomerID.ToString() + "," + ProductID.ToString() + "," + VariantID.ToString() + "," + thisID.ToString() + ",KitGroupTypeID, " + thisVal + "," + ((int)CartTypeEnum.ShoppingCart).ToString() + " from dbo.kitgroup where KitGroupID = " + thisID.ToString();
                        }
                        else
                        {
                            /*
                            sql = "insert into kitcart(customerID,ProductID,VariantID,KitGroupID,KitGroupTypeID,KitItemID,CartType) ";
                            sql += "select " + ThisCustomer.CustomerID.ToString() + "," + ProductID.ToString() + "," + VariantID.ToString() + "," + thisID.ToString() + ",KitGroupTypeID," + thisVal + "," + ((int)CartTypeEnum.ShoppingCart).ToString() + " from dbo.kitgroup where KitGroupID = " + thisID.ToString();
                            */
                            sql = "insert into kitcart(customerID,ProductID,VariantID,KitGroupID,KitGroupTypeID,KitItemID,CartType,InventoryVariantID, InventoryVariantColor, InventoryVariantSize) ";
                            sql += "select " + ThisCustomer.CustomerID.ToString() + "," + ProductID.ToString() + "," + VariantID.ToString() + "," + thisID.ToString() + ",kg.KitGroupTypeID," + thisVal + "," + ((int)CartTypeEnum.ShoppingCart).ToString() + ", ki.InventoryVariantID, ki.InventoryVariantColor, ki.InventoryVariantSize from dbo.kitgroup kg join dbo.KitItem ki on  kg.KitGroupID = ki.KitGroupID  where kg.KitGroupID = " + thisID.ToString() + " and ki.KitItemID = " + thisVal;
                        }
                        DB.ExecuteSQL(sql);
                    }
                }
            }
            if (FromCart)
            {
                DB.ExecuteSQL("delete from KitCart where ShoppingCartRecID=-" + CartRecID);
                DB.ExecuteSQL("exec dbo.aspdnsf_UpdateCartKitPrice " + CartRecID + ", " + ThisCustomer.CustomerLevelID);
            }
        }

        // mod start
        // -- new kit format -- //
        public static void ProcessKitComposition(Customer ThisCustomer, KitComposition composition)
        {
            foreach (KitCartItem item in composition.Compositions)
            {
                string textOptionOrImagePath = item.TextOption;
                if (item.ImageFile != null)
                {
                    string extension = string.Empty;
                    switch (item.ImageFile.ContentType)
                    {
                        case "image/gif":
                            extension = "gif";
                            break;
                        case "image/png":
                        case "image/x-png":
                            extension = "png";
                            break;
                        case "image/jpg":
                        case "image/jpeg":
                        case "image/pjpeg":
                            extension = "jpg";
                            break;
                    }

                    if (extension != string.Empty)
                    {
                        string saveFileName = string.Format("{0}_{1}_{2}_{3}.{4}", ThisCustomer.CustomerID, item.ProductID, item.KitItemID, DateTime.Now.ToString("yyyyMMddHHmmss"), extension);
                        string saveImagePath = AppLogic.GetImagePath("Orders", string.Empty, false) + saveFileName;
                        string saveImageFullPath = AppLogic.GetImagePath("Orders", string.Empty, true) + saveFileName;

                        textOptionOrImagePath = saveImagePath;

                        try
                        {
                            item.ImageFile.SaveAs(saveImageFullPath);
                        }
                        catch { }
                    }
                    else
                    {
                        textOptionOrImagePath = string.Empty;
                    }
                }

                CreateKitItem(ThisCustomer, composition.CartID, item.ProductID, item.VariantID, item.KitGroupID, item.KitItemID, textOptionOrImagePath, item.Quantity);
            }
        }

        public static void ClearKitItems(Customer ThisCustomer, int ProductID, int VariantID, int CartRecID)
        {
            string clearKitItemsCommand =
                string.Format("DELETE FROM KitCart WHERE CustomerID = {0} AND ProductID = {1} AND VariantID = {2} AND ShoppingCartRecID = {3}", ThisCustomer.CustomerID, ProductID, VariantID, CartRecID);

            DB.ExecuteSQL(clearKitItemsCommand);
        }

        public static void CreateKitItem(Customer ThisCustomer,
            int CartRecID,
            int ProductID,
            int VariantID,
            int KitGroupID,
            int KitItemID,
            string textOption,
            int quantity)
        {
            int KitGroupTypeID = 0;
            int InventoryVariantID = 0;
            String InventoryVariantColor = String.Empty;
            String InventoryVariantSize = String.Empty;

            string sql = string.Format("select kg.KitGroupTypeID, ki.InventoryVariantID, isNull(ki.InventoryVariantColor,'') InventoryVariantColor, isNull(ki.InventoryVariantSize,'') InventoryVariantSize from KitItem ki with (NOLOCK) inner join KitGroup kg with (NOLOCK) on kg.KitGroupID=ki.KitGroupID where ki.KitItemID={0}", KitItemID);

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader reader = DB.GetRS(sql, con))
                {
                    while (reader.Read())
                    {
                        KitGroupTypeID = DB.RSFieldInt(reader, "KitGroupTypeID");
                        InventoryVariantID = DB.RSFieldInt(reader, "InventoryVariantID");
                        InventoryVariantColor = DB.RSField(reader, "InventoryVariantColor");
                        InventoryVariantSize = DB.RSField(reader, "InventoryVariantSize");
                    }
                }
            }

            string createKitItemCommand =
            string.Format(
                "INSERT INTO KitCart(CustomerID,ProductID,VariantID,ShoppingCartRecID,KitGroupID,KitGroupTypeID,KitItemID,CartType,InventoryVariantID, InventoryVariantColor, InventoryVariantSize, TextOption, Quantity) VALUES({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12})",
                ThisCustomer.CustomerID,
                ProductID,
                VariantID,
                CartRecID,
                KitGroupID,
                KitGroupTypeID,
                KitItemID,
                ((int)CartTypeEnum.ShoppingCart),
                InventoryVariantID,
                DB.SQuote(InventoryVariantColor),
                DB.SQuote(InventoryVariantSize),
                DB.SQuote(textOption),
                quantity);

            DB.ExecuteSQL(createKitItemCommand);
        }
        // -- new kit format -- //
        // mod end

        static public bool KitContainsItem(int CustomerID, int ProductID, int ShoppingCartRecID, int KitItemID)
        {
            return DB.GetSqlN("select count(*) as N from kitcart   with (NOLOCK)  where customerid=" + CustomerID.ToString() + " and productid=" + ProductID.ToString() + " and ShoppingCartrecid=" + ShoppingCartRecID.ToString() + " and kititemid=" + KitItemID.ToString()) > 0;
        }

        static public String KitContainsText(int CustomerID, int ProductID, int ShoppingCartRecID, int KitItemID)
        {
            return DB.GetSqlS("select TextOption as S from kitcart  with (NOLOCK)  where customerid=" + CustomerID.ToString() + " and productid=" + ProductID.ToString() + " and ShoppingCartrecid=" + ShoppingCartRecID.ToString() + " and kititemid=" + KitItemID.ToString());
        }


        static public bool KitContainsAnyGroupItems(int CustomerID, int ProductID, int ShoppingCartRecID, int KitGroupID)
        {
            return DB.GetSqlN("select count(*) as N from kitcart   with (NOLOCK)  where customerid=" + CustomerID.ToString() + " and productid=" + ProductID.ToString() + " and ShoppingCartrecid=" + ShoppingCartRecID.ToString() + " and kititemid in (select kititemid from kititem   with (NOLOCK)  where kitgroupid=" + KitGroupID.ToString() + ")") > 0;
        }

        static public decimal KitPriceDelta(int CustomerID, int ProductID, int ShoppingCartRecID)
        {
            return KitPriceDelta(CustomerID, ProductID, ShoppingCartRecID, Localization.StoreCurrency());
        }

        static public decimal KitPriceDelta(int CustomerID, int ProductID, int ShoppingCartRecID, string ForCurrency)
        {
            return Prices.KitPriceDelta(CustomerID, ProductID, ShoppingCartRecID, ForCurrency);
        }

        static public decimal KitWeightDelta(int CustomerID, int ProductID, int ShoppingCartRecID)
        {
            decimal tmp = System.Decimal.Zero;
            if (CustomerID != 0)
            {
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS("select sum(quantity*weightdelta) as WT from kitcart   with (NOLOCK)  inner join kititem   with (NOLOCK)  on kitcart.kititemid=kitItem.kititemid where customerid=" + CustomerID.ToString() + " and productid=" + ProductID.ToString() + " and ShoppingCartrecid=" + ShoppingCartRecID.ToString(), con))
                    {
                        if (rs.Read())
                        {
                            tmp = DB.RSFieldDecimal(rs, "WT");
                        }
                    }
                }
            }
            return tmp;
        }

        static public decimal GetColorAndSizePriceDelta(String ChosenColor, String ChosenSize)
        {
            return Prices.GetColorAndSizePriceDelta(ChosenColor, ChosenSize);
        }

        static public decimal GetColorAndSizePriceDelta(String ChosenColor, String ChosenSize, int TaxClassID, Customer ThisCustomer, bool WithDiscount, bool WithVAT)
        {
            return Prices.GetColorAndSizePriceDelta(ChosenColor, ChosenSize, TaxClassID, ThisCustomer, WithDiscount, WithVAT);
        }

        static public decimal GetKitTotalPrice(int CustomerID, int CustomerLevelID, int ProductID, int VariantID, int ShoppingCartRecID)
        {
            return Prices.GetKitTotalPrice(CustomerID, CustomerLevelID, ProductID, VariantID, ShoppingCartRecID);
        }

        static public decimal GetKitTotalWeight(int CustomerID, int CustomerLevelID, int ProductID, int VariantID, int ShoppingCartRecID)
        {
            Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)HttpContext.Current.User).ThisCustomer;
            decimal tmp = System.Decimal.Zero;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("SELECT Product.*, ProductVariant.Price, ProductVariant.SalePrice, ProductVariant.Weight FROM Product   with (NOLOCK)  inner join productvariant   with (NOLOCK)  on product.productid=productvariant.productid where ProductVariant.VariantID=" + VariantID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        decimal KitWeight = DB.RSFieldDecimal(rs, "Weight");
                        decimal KitWeightDelta = AppLogic.KitWeightDelta(CustomerID, ProductID, ShoppingCartRecID);
                        tmp = KitWeight + KitWeightDelta;
                    }
                }
            }

            return tmp;
        }


        static public bool KitContainsAllRequiredItems(int CustomerID, int ProductID, int ShoppingCartRecID)
        {
            bool AllRequiredFound = true;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select KitGroupID from KitGroup  with (NOLOCK)  where IsRequired=1 and KitGroup.ProductID=" + ProductID.ToString(), con))
                {
                    while (rs.Read())
                    {
                        if (!AppLogic.KitContainsAnyGroupItems(CustomerID, ProductID, ShoppingCartRecID, DB.RSFieldInt(rs, "KitGroupID")))
                        {
                            AllRequiredFound = false;
                        }
                    }
                }
            }

            return AllRequiredFound;
        }

        static public String GetJSPopupRoutines()
        {
            StringBuilder tmpS = new StringBuilder(2500);
            tmpS.Append("<script type=\"text/javascript\">\n");
            tmpS.Append("function popupwh(title,url,w,h)\n");
            tmpS.Append("	{\n");
            tmpS.Append("	window.open('popup.aspx?title=' + title + '&src=' + url,'Popup" + CommonLogic.GetRandomNumber(1, 100000).ToString() + "','toolbar=no,location=no,directories=no,status=no,menubar=no,scrollbars=no,resizable=no,copyhistory=no,width=' + w + ',height=' + h + ',left=0,top=0');\n");
            tmpS.Append("	return (true);\n");
            tmpS.Append("	}\n");
            tmpS.Append("function popuptopicwh(title,topic,w,h,scrollbars)\n");
            tmpS.Append("	{\n");
            tmpS.Append("	window.open('popup.aspx?title=' + title + '&topic=' + topic,'Popup" + CommonLogic.GetRandomNumber(1, 100000).ToString() + "','toolbar=no,location=no,directories=no,status=no,menubar=no,scrollbars=' + scrollbars + ',resizable=no,copyhistory=no,width=' + w + ',height=' + h + ',left=0,top=0');\n");
            tmpS.Append("	return (true);\n");
            tmpS.Append("	}\n");
            tmpS.Append("function popuporderoptionwh(title,id,w,h,scrollbars)\n");
            tmpS.Append("	{\n");
            tmpS.Append("	window.open('popup.aspx?title=' + title + '&orderoptionid=' + id,'Popup" + CommonLogic.GetRandomNumber(1, 100000).ToString() + "','toolbar=no,location=no,directories=no,status=no,menubar=no,scrollbars=' + scrollbars + ',resizable=no,copyhistory=no,width=' + w + ',height=' + h + ',left=0,top=0');\n");
            tmpS.Append("	return (true);\n");
            tmpS.Append("	}\n");
            tmpS.Append("function popupkitgroupwh(title,kitgroupid,w,h,scrollbars)\n");
            tmpS.Append("	{\n");
            tmpS.Append("	window.open('popup.aspx?title=' + title + '&kitgroupid=' + kitgroupid,'Popup" + CommonLogic.GetRandomNumber(1, 100000).ToString() + "','toolbar=no,location=no,directories=no,status=no,menubar=no,scrollbars=' + scrollbars + ',resizable=no,copyhistory=no,width=' + w + ',height=' + h + ',left=0,top=0');\n");
            tmpS.Append("	return (true);\n");
            tmpS.Append("	}\n");
            tmpS.Append("function popupkititemwh(title,kititemid,w,h,scrollbars)\n");
            tmpS.Append("	{\n");
            tmpS.Append("	window.open('popup.aspx?title=' + title + '&kititemid=' + kititemid,'Popup" + CommonLogic.GetRandomNumber(1, 100000).ToString() + "','toolbar=no,location=no,directories=no,status=no,menubar=no,scrollbars=' + scrollbars + ',resizable=no,copyhistory=no,width=' + w + ',height=' + h + ',left=0,top=0');\n");
            tmpS.Append("	return (true);\n");
            tmpS.Append("	}\n");
            tmpS.Append("function popup(title,url)\n");
            tmpS.Append("	{\n");
            tmpS.Append("	popupwh(title,url,600,375);\n");
            tmpS.Append("	return (true);\n");
            tmpS.Append("	}\n");
            tmpS.Append("function popuptopic(title,topic,scrollbars)\n");
            tmpS.Append("	{\n");
            tmpS.Append("	popuptopicwh(title,topic,600,375,scrollbars);\n");
            tmpS.Append("	return (true);\n");
            tmpS.Append("	}\n");
            tmpS.Append("</script>\n");
            return tmpS.ToString();
        }

        public static bool IsAKit(int ProductID)
        {
            bool tmpS = false;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select IsAKit from Product   with (NOLOCK)  where ProductID=" + ProductID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmpS = DB.RSFieldBool(rs, "IsAKit");
                    }
                }
            }

            return tmpS;
        }

        public static int GetStoreSkinID(int StoreID)
        {
            int SkinID = 1;
            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("SELECT SkinID FROM Store with (NOLOCK) WHERE StoreID='" + StoreID.ToString() + "';", con))
                {
                    if (rs.Read())
                    {
                        SkinID = DB.RSFieldInt(rs, "SkinID");
                    }
                }
            }
            return SkinID;
        }

        public static String GetStoreDomain()
        {
            String ThisDomain = AppLogic.GetStoreHTTPLocation(false).Replace("http://", "");
            int idx = ThisDomain.IndexOf("/");
            if (idx != -1)
            {
                ThisDomain = ThisDomain.Substring(0, idx);
            }
            return ThisDomain;
        }

        public static String GetStoreHTTPLocation(bool TryToUseSSL)
        {
            return GetStoreHTTPLocation(TryToUseSSL, true);
        }

        public static String GetStoreHTTPLocation(bool TryToUseSSL, bool includeScriptLocation)
        {
            String ScriptLocation = String.Empty;

            if (includeScriptLocation)
            {
                String[] ScriptPathItems = CommonLogic.ServerVariables("SCRIPT_NAME").Split('/');
                for (int i = 0; i < ScriptPathItems.GetUpperBound(0); i++)
                {
                    if (false == ScriptPathItems[i].Equals(AppLogic.GetAdminDir(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        ScriptLocation += ScriptPathItems[i] + "/";
                    }
                }
                if (ScriptLocation.Length == 0)
                {
                    ScriptLocation = "/";
                }
                if (!ScriptLocation.EndsWith("/"))
                {
                    ScriptLocation = "/";
                }
            }

            // ScriptLocation should now be everything after server name, including trailing "/", e.g. "/netstore/" or "/"
            String s = "http://" + CommonLogic.ServerVariables("HTTP_HOST") + ScriptLocation;
            if (TryToUseSSL && AppLogic.UseSSL() && AppLogic.OnLiveServer())
            {
                if (AppLogic.AppConfig("SharedSSLLocation").Length == 0)
                {
                    s = s.Replace("http:/", "https:/");
                    if (AppLogic.RedirectLiveToWWW())
                    {
                        if (s.IndexOf("https://www") == -1)
                        {
                            s = s.Replace("https://", "https://www.");
                        }
                        s = s.Replace("www.www", "www"); // safety check
                    }
                }
                else
                {
                    s = AppLogic.AppConfig("SharedSSLLocation") + ScriptLocation;
                }
            }
            return s;
        }

        static public bool ProductTracksInventoryBySizeAndColor(int ProductID)
        {
            bool tmp = false;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select TrackInventoryBySizeAndColor from Product   with (NOLOCK)  where ProductID=" + ProductID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSFieldBool(rs, "TrackInventoryBySizeAndColor");
                    }
                }
            }

            return tmp;
        }

        static public bool ProductTracksInventoryBySize(int ProductID)
        {
            bool tmp = false;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select TrackInventoryBySize from Product   with (NOLOCK)  where ProductID=" + ProductID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSFieldBool(rs, "TrackInventoryBySize");
                    }
                }
            }

            return tmp;
        }

        static public bool ProductTracksInventoryByColor(int ProductID)
        {
            bool tmp = false;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select TrackInventoryByColor from Product   with (NOLOCK)  where ProductID=" + ProductID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSFieldBool(rs, "TrackInventoryByColor");
                    }
                }
            }

            return tmp;
        }

		static public int GetInventory(int productId, int variantId, string chosenSize, string chosenColor)
        {
            bool trackInventoryBySizeAndColor = false;
            bool trackInventoryBySize = false;
            bool trackInventoryByColor = false;
            SqlParameter[] spa = { DB.CreateSQLParameter("@ProductID", SqlDbType.Int, 4, productId, ParameterDirection.Input) };

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader dr = DB.GetRS("select TrackInventoryBySizeAndColor, TrackInventoryBySize, TrackInventoryByColor from dbo.Product where ProductID = @ProductID", spa, con))
                {
                    bool b = dr.Read();
                    if (b)
                    {
                        trackInventoryBySizeAndColor = DB.RSFieldBool(dr, "TrackInventoryBySizeAndColor");
                        trackInventoryBySize = DB.RSFieldBool(dr, "TrackInventoryBySize");
                        trackInventoryByColor = DB.RSFieldBool(dr, "TrackInventoryByColor");
                    }
                }
            }

			return GetInventory(productId, variantId, chosenSize, chosenColor, trackInventoryBySizeAndColor, trackInventoryByColor, trackInventoryBySize);
        }

		static public int GetInventory(int productID, int variantID, string chosenSize, string chosenColor, bool trackInventoryBySizeAndColor, bool trackInventoryByColor, bool tracksInventoryBySize)
        {
			string warehouseLocation = string.Empty;
			string fullSku = string.Empty;

			return GetInventory(productID, variantID, chosenSize, chosenColor, trackInventoryBySizeAndColor, trackInventoryByColor, tracksInventoryBySize, out warehouseLocation, out fullSku);
        }

		static public int GetInventory(int productID, int variantID, string chosenSize, string chosenColor, bool trackInventoryBySizeAndColor, bool trackInventoryByColor, bool tracksInventoryBySize, out string warehouseLocation, out string fullSku)
        {
			string vendorId = string.Empty;
			decimal weightDelta = decimal.Zero;
			string gtin = string.Empty;

			return GetInventory(productID, variantID, chosenSize, chosenColor, trackInventoryBySizeAndColor, trackInventoryByColor, tracksInventoryBySize, out warehouseLocation, out fullSku, out vendorId, out weightDelta, out gtin);
        }

		static public int GetInventory(int productID, int variantID, string chosenSize, string chosenColor, bool trackInventoryBySizeAndColor, bool trackInventoryByColor, bool tracksInventoryBySize, out string warehouseLocation, out string fullSku, out string vendorId, out decimal weightDelta, out string gtin)
        {
            int inventory = 0;
			warehouseLocation = string.Empty;
			fullSku = string.Empty;
			vendorId = string.Empty;
            weightDelta = decimal.Zero;
			gtin = string.Empty;

            if (!trackInventoryBySizeAndColor)
            {
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
					using (IDataReader rs = DB.GetRS("Select Inventory from ProductVariant with (NOLOCK)  where VariantId = @VariantId", new SqlParameter[] { new SqlParameter("@VariantId", variantID) }, con))
                    {
                        if (rs.Read())
                        {
                            inventory = DB.RSFieldInt(rs, "Inventory");
                        }
                    }
                }
            }
            else
            {
				string size = tracksInventoryBySize ? CleanSizeColorOption(chosenSize).ToLower() : string.Empty;
				string color = trackInventoryByColor ? CleanSizeColorOption(chosenColor).ToLower() : string.Empty;

                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS(@"select Quan, WarehouseLocation, VendorFullSKU, VendorID, WeightDelta, GTIN 
														from Inventory with (NOLOCK) 
														where VariantID = @VariantId 
														and lower([size]) = @Size 
														and lower(color) = @Color", new SqlParameter[] { new SqlParameter("@VariantId", variantID), new SqlParameter("@Size", size), new SqlParameter("@Color", color) }, con))
                    {
                        if (rs.Read())
                        {
							inventory = DB.RSFieldInt(rs, "Quan");
                            warehouseLocation = DB.RSField(rs, "WarehouseLocation");
                            fullSku = DB.RSField(rs, "VendorFullSKU");
                            vendorId = DB.RSField(rs, "VendorID");
                            weightDelta = DB.RSFieldDecimal(rs, "WeightDelta");
							gtin = DB.RSField(rs, "GTIN");
                        }
                    }
                }
            }
            return (inventory < 0) ? 0 : inventory;
        }

        static public bool CustomerLevelAllowsPO(int CustomerLevelID)
        {
            if (CustomerLevelID == 0)
            {
                // consumers cannot use PO's, unless overridden by other parameters:
                return AppLogic.AppConfigBool("CustomerLevel0AllowsPOs");
            }
            bool tmpS = false;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("Select LevelAllowsPO from CustomerLevel   with (NOLOCK)  where CustomerLevelID=" + CustomerLevelID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmpS = DB.RSFieldBool(rs, "LevelAllowsPO");
                    }
                }
            }

            return tmpS;
        }

        static public bool OrderHasDownloadComponents(int OrderNumber, bool RequireDownloadLocationAlso)
        {
            return DB.GetSqlN("select count(*) as N from orders_ShoppingCart where IsDownload=1 " + CommonLogic.IIF(RequireDownloadLocationAlso, " and DownloadLocation is not null and datalength(DownloadLocation) > 0", "") + " and OrderNumber=" + OrderNumber.ToString()) != 0;
        }

        static public bool OrderHasShippableComponents(int OrderNumber)
        {
            return DB.GetSqlN("select count(*) as N from orders_ShoppingCart   with (NOLOCK)  where IsDownload=0 and FreeShipping!=2 and OrderNumber=" + OrderNumber.ToString()) != 0;
        }

        static public String GetRelatedProductsBoxExpanded(int forProductID, int showNum, bool showPics, String teaser, bool gridFormat, Customer ThisCustomer, int SkinID, String LocaleSetting)
        {
            return AppLogic.RunXmlPackage("relatedproducts.xml.config", null, ThisCustomer, SkinID, "", "ProductID=" + forProductID.ToString(), false, false);
        }

        static public Decimal GetUpsellProductPrice(int SourceProductID, int UpsellProductID, int CustomerLevelID)
        {
            return Prices.GetUpsellProductPrice(SourceProductID, UpsellProductID, CustomerLevelID);
        }

        static public String GetUpsellProductsBoxExpanded(int forProductID, int showNum, bool showPics, String teaser, bool gridFormat, int SkinID, Customer ThisCustomer)
        {
            String s = AppLogic.RunXmlPackage("upsellproducts.xml.config", new Parser(SkinID, ThisCustomer), ThisCustomer, SkinID, "", "Productid=" + forProductID.ToString(), false, false);
            return s;
        }

        static public String GetUpsellProductsBoxExpandedForCart(String UpsellProductList, int showNum, bool showPics, String teaser, bool gridFormat, int SkinID, Customer ThisCustomer)
        {
            String s = AppLogic.RunXmlPackage("upsellproducts.xml.config", new Parser(SkinID, ThisCustomer), ThisCustomer, SkinID, "", "cart=1", false, false);
            return s;
        }

        public static bool ReferrerOKForSubmit()
        {
            return true; // routine is obscolete
        }

        static public String GetNewsBoxExpanded(bool LinkHeadline, bool ShowCopy, int showNum, bool IncludeFrame, bool useCache, String teaser, int SkinID, String LocaleSetting)
        {
            String CacheName = "GetNewsBoxExpanded_" + showNum.ToString() + "_" + teaser + "_" + SkinID.ToString() + "_" + LocaleSetting;
            if (AppLogic.CachingOn && useCache)
            {
                String cachedData = (String)HttpContext.Current.Cache.Get(CacheName);
                if (cachedData != null)
                {
                    if (CommonLogic.ApplicationBool("DumpSQL"))
                    {
                        HttpContext.Current.Response.Write("Cache Hit Found!\n");
                    }
                    return cachedData;
                }
            }

            StringBuilder tmpS = new StringBuilder(10000);

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                string query = string.Format("select COUNT(a.*) AS N from News a with (NOLOCK) inner join (Select distinct a.NewsID FROM News a with (nolock) left join NewsStore b with (NOLOCK) on a.NewsID = b.NewsID WHERE ({0} = 0 or StoreID = {1})) b on a.NewsID = b.NewsID where ExpiresOn>getdate() and Deleted=0 and Published=1 ; " +
                                             "select a.* from News a with (NOLOCK) inner join (SELECT a.NewsID FROM News a with (nolock) left join NewsStore b with (NOLOCK) on a.NewsID = b.NewsID WHERE ({0} = 0 or StoreID = {1})) b on a.NewsID = b.NewsID where ExpiresOn>getdate() and Deleted=0 and Published=1 order by a.CreatedON desc", CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowNewsFiltering") == true, 1, 0), AppLogic.StoreID());
                using (IDataReader rs = DB.GetRS(query, con))
                {
                    if (rs.Read() && DB.RSFieldInt(rs, "N") > 0)
                    {
                        if (rs.NextResult())
                        {
                            if (IncludeFrame)
                            {
                                tmpS.Append("<table width=\"100%\" cellpadding=\"2\" cellspacing=\"0\" border=\"0\" style=\"border-style: solid; border-width: 0px; border-color: #" + AppLogic.AppConfig("HeaderBGColor") + "\">\n");
                                tmpS.Append("<tr><td align=\"left\" valign=\"top\">\n");
                                tmpS.Append("<a href=\"news.aspx\"><img src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/newsexpanded.gif") + "\" border=\"0\" /></a>");
                                tmpS.Append("<table width=\"100%\" cellpadding=\"4\" cellspacing=\"0\" border=\"0\" style=\"" + AppLogic.AppConfig("BoxFrameStyle") + "\">\n");
                                tmpS.Append("<tr><td align=\"left\" valign=\"top\">\n");
                            }

                            tmpS.Append("<p><b>" + teaser + "</b></p>\n");


                            tmpS.Append("<table width=\"100%\" cellpadding=\"2\" cellspacing=\"0\" border=\"0\">\n");
                            int i = 1;

                            while (rs.Read())
                            {
                                if (i > showNum)
                                {
                                    tmpS.Append("<tr><td colspan=\"2\"><hr size=\"1\" color=\"#" + AppLogic.AppConfig("MediumCellColor") + "\"/><a href=\"news.aspx\">more...</a></td></tr>");
                                    break;
                                }
                                if (i > 1)
                                {
                                    tmpS.Append("<tr><td colspan=\"2\"><hr size=\"1\" color=\"#" + AppLogic.AppConfig("MediumCellColor") + "\"/></td></tr>");
                                }
                                tmpS.Append("<tr>");
                                tmpS.Append("<td width=\"15%\" align=\"left\" valign=\"top\">\n");
                                tmpS.Append("<b>" + Localization.ToThreadCultureShortDateString(DB.RSFieldDateTime(rs, "CreatedOn")) + "</b>");
                                tmpS.Append("</td>");
                                tmpS.Append("<td align=\"left\" valign=\"top\">\n");
                                String Hdl = DB.RSFieldByLocale(rs, "Headline", LocaleSetting);
                                if (Hdl.Length == 0)
                                {
                                    Hdl = CommonLogic.Ellipses(DB.RSFieldByLocale(rs, "NewsCopy", LocaleSetting), 50, true);
                                }
                                tmpS.Append("<div align=\"left\">");
                                if (LinkHeadline)
                                {
                                    tmpS.Append("<a href=\"news.aspx?showarticle=" + DB.RSFieldInt(rs, "NewsID").ToString() + "\">");
                                }
                                tmpS.Append("<b>");
                                tmpS.Append(Hdl);
                                tmpS.Append("</b>");
                                if (LinkHeadline)
                                {
                                    tmpS.Append("</a>");
                                }
                                tmpS.Append("</div>");
                                if (ShowCopy)
                                {
                                    tmpS.Append("<div align=\"left\">" + HttpContext.Current.Server.HtmlDecode(DB.RSFieldByLocale(rs, "NewsCopy", LocaleSetting)) + "</div>");
                                }
                                tmpS.Append("</td>");
                                tmpS.Append("</tr>");
                                i++;
                            }

                            tmpS.Append("</table>\n");

                            if (IncludeFrame)
                            {
                                tmpS.Append("</td></tr>\n");
                                tmpS.Append("</table>\n");
                                tmpS.Append("</td></tr>\n");
                                tmpS.Append("</table>\n");
                            }
                        }
                    }
                }
            }

            if (CachingOn && useCache)
            {
                HttpContext.Current.Cache.Insert(CacheName, tmpS.ToString(), null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }
            return tmpS.ToString();
        }


        static public String GetNewsSummary(int ShowNum)
        {
            String CacheName = "GetNewsSummary_" + ShowNum.ToString();
            if (AppLogic.CachingOn)
            {
                String Menu = (String)HttpContext.Current.Cache.Get(CacheName);
                if (Menu != null)
                {
                    if (CommonLogic.ApplicationBool("DumpSQL"))
                    {
                        HttpContext.Current.Response.Write("Cache Hit Found!\n");
                    }
                    return Menu;
                }
            }

            StringBuilder tmpS = new StringBuilder(4096);
            String sql = string.Format("select * from News a with (NOLOCK) inner join (SELECT distinct a.NewsID FROM News a with (nolock) left join NewsStore b with (nolock) on a.NewsID = b.NewsID WHERE ({0} = 0 or StoreID = {1})) b on a.NewsID = b.NewsID where ExpiresOn>=getdate() and Deleted=0 and Published=1 order by CreatedON desc", CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowNewsFiltering") == true, 1, 0), AppLogic.StoreID());

            tmpS.Append("			<table cellpadding=\"0\" cellspacing=\"0\" border=\"0\">\n");
            bool anyFound = false;
            int i = 1;
            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS(sql, con))
                {
                    while (rs.Read() && i <= ShowNum)
                    {
                        String Hdl = DB.RSFieldByLocale(rs, "Headline", Thread.CurrentThread.CurrentUICulture.Name);
                        if (Hdl.Length == 0)
                        {
                            Hdl = CommonLogic.Ellipses(DB.RSFieldByLocale(rs, "NewsCopy", Thread.CurrentThread.CurrentUICulture.Name), 50, true);
                        }
                        tmpS.Append("				<tr><td valign=\"top\"><img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_(!SKINID!)/images/y.gif") + "\" hspace=\"10\" /></td><td><a class=\"NewsItem\" href=\"news.aspx?showarticle=" + DB.RSFieldInt(rs, "NewsID").ToString() + "\">" + Hdl + "</a></td></tr>\n");
                        tmpS.Append("				<tr><td height=\"5\"></td></tr>\n");
                        anyFound = true;
                        i++;
                        if (i > ShowNum)
                        {
                            tmpS.Append("<tr><td colspan=\"2\"><a href=\"news.aspx\">" + GetString("news.MoreNews.1", Customer.Current.SkinID, Customer.Current.LocaleSetting) + "</a></td></tr>");
                            break;
                        }
                    }
                }
            }



            if (!anyFound)
            {
                tmpS.AppendFormat("				<tr><td valign=\"top\"><img align=\"absmiddle\" src=\"{0}\" hspace=\"10\" /></td><td><font class=\"NewsItem\">{1}</font></td></tr>\n", AppLogic.LocateImageURL("App_Themes/skin_(!SKINID!)/images/y.gif"), GetString("news.NoNews.1", Customer.Current.SkinID, Customer.Current.LocaleSetting));
                tmpS.Append("				<tr><td height=\"5\"></td></tr>\n");
            }
            tmpS.Append("			</table>\n");

            if (CachingOn)
            {
                HttpContext.Current.Cache.Insert(CacheName, tmpS.ToString(), null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }
            return tmpS.ToString();
        }


        static public decimal GetCustomerLevelDiscountAmount(int CustomerLevelID)
        {
            if (CustomerLevelID == 0)
            {
                // consumers always have tax:
                return System.Decimal.Zero;
            }
            decimal tmpS = System.Decimal.Zero;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("Select  LevelDiscountAmount from CustomerLevel   with (NOLOCK)  where CustomerLevelID=" + CustomerLevelID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmpS = DB.RSFieldDecimal(rs, "LevelDiscountAmount");
                    }
                }
            }

            return tmpS;

        }

        static public Decimal GetCustomerLevelDiscountPercent(int CustomerLevelID)
        {
            if (CustomerLevelID == 0)
            {
                // consumers always have tax:
                return 0.0M;
            }
            Decimal tmpS = 0.0M;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("Select LevelDiscountPercent from CustomerLevel   with (NOLOCK)  where CustomerLevelID=" + CustomerLevelID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmpS = DB.RSFieldDecimal(rs, "LevelDiscountPercent");
                    }
                }
            }

            return tmpS;

        }

        static public bool CustomerLevelHasNoTax(int CustomerLevelID)
        {
            if (CustomerLevelID == 0)
            {
                // consumers always have tax:
                return false;
            }
            bool tmpS = false;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("Select LevelHasNoTax from CustomerLevel   with (NOLOCK)  where CustomerLevelID=" + CustomerLevelID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmpS = DB.RSFieldBool(rs, "LevelHasNoTax");
                    }
                }
            }

            return tmpS;

        }

        static public bool CustomerLevelHasFreeShipping(int CustomerLevelID)
        {
            if (CustomerLevelID == 0)
            {
                // consumers always have shipping, unless overridden by other parameters:
                return false;
            }
            bool tmpS = false;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("Select LevelHasFreeShipping from CustomerLevel   with (NOLOCK)  where CustomerLevelID=" + CustomerLevelID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmpS = DB.RSFieldBool(rs, "LevelHasFreeShipping");
                    }
                }
            }

            return tmpS;

        }

        static public bool CustomerLevelAllowsCoupons(int CustomerLevelID)
        {
            return CustomerLevelAllowsCoupons(null, CustomerLevelID);
        }

        static public bool CustomerLevelAllowsCoupons(SqlTransaction m_DBTrans, int CustomerLevelID)
        {
            if (CustomerLevelID == 0)
            {
                // consumers always have this option by default, it can be overridden by product/variant settings however:
                return true;
            }
            bool tmpS = false;

            SqlConnection con = null;
            IDataReader rs = null;
            try
            {
                string query = "Select LevelAllowsCoupons from CustomerLevel   with (NOLOCK)  where CustomerLevelID=" + CustomerLevelID.ToString();
                if (m_DBTrans != null)
                {
                    // if a transaction was passed, we should use the transaction objects connection
                    rs = DB.GetRS(query, m_DBTrans);
                }
                else
                {
                    // otherwise create it
                    con = new SqlConnection(DB.GetDBConn());
                    con.Open();
                    rs = DB.GetRS(query, con);
                }

                using (rs)
                {
                    if (rs.Read())
                    {
                        tmpS = DB.RSFieldBool(rs, "LevelAllowsCoupons");
                    }
                }
            }
            catch { throw; }
            finally
            {
                // we can't dispose of the connection if it's part of a transaction
                if (con != null && m_DBTrans == null)
                {
                    // here it's safe to dispose since we created the connection ourself
                    con.Dispose();
                }

                // make sure we won't reference this again in code
                rs = null;
                con = null;
            }

            return tmpS;
        }

        static public int GetVariantID(string GUID)
        {
            int VariantID = 0;
            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select VariantID from ProductVariant with (NOLOCK) where VariantGUID=" + DB.SQuote(GUID), con))
                {
                    if (rs.Read())
                    {
                        VariantID = DB.RSFieldInt(rs, "VariantID");
                    }
                }
            }
            return VariantID;
        }

        static public decimal GetVariantExtendedPrice(int VariantID, int CustomerLevelID)
        {
            decimal pr = System.Decimal.Zero;
            if (CustomerLevelID != 0)
            {
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS("select Price from ExtendedPrice  with (NOLOCK)  where VariantID=" + VariantID.ToString() + " and CustomerLevelID=" + CustomerLevelID.ToString() + " and VariantID in (select VariantID from ProductVariant where ProductID in (select ProductID from ProductCustomerLevel where CustomerLevelID=" + CustomerLevelID.ToString() + "))", con))
                    {
                        if (rs.Read())
                        {
                            pr = DB.RSFieldDecimal(rs, "Price");
                        }
                    }
                }
            }
            return pr;
        }

        // does all 3 pricing lookups at one time (regular price, sale price, extended price)
        public static decimal VariantPriceLookup(Customer ThisCustomer, int VariantID)
        {
            return Prices.VariantPriceLookup(ThisCustomer, VariantID);

            //decimal tmp = System.Decimal.Zero;
            //int CL = 0;
            //if (ThisCustomer != null)
            //{
            //    CL = ThisCustomer.CustomerLevelID;
            //}

            //using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            //{
            //    con.Open();
            //    using (IDataReader rs = DB.GetRS("SELECT pv.VariantID, pv.Price, isnull(pv.SalePrice, 0) SalePrice, isnull(e.Price, 0) ExtendedPrice FROM ProductVariant pv with (NOLOCK) left join ExtendedPrice e with (NOLOCK) on pv.VariantID=e.VariantID and e.CustomerLevelID=" + CL.ToString() + " WHERE pv.VariantID=" + VariantID.ToString(), con))
            //    {
            //        if (rs.Read())
            //        {
            //            if (DB.RSFieldDecimal(rs, "ExtendedPrice") != System.Decimal.Zero)
            //            {
            //                tmp = DB.RSFieldDecimal(rs, "ExtendedPrice");
            //            }
            //            else if (DB.RSFieldDecimal(rs, "SalePrice") != System.Decimal.Zero)
            //            {
            //                tmp = DB.RSFieldDecimal(rs, "SalePrice");
            //            }
            //            else
            //            {
            //                tmp = DB.RSFieldDecimal(rs, "Price");
            //            }
            //        }
            //    }
            //}

            //return tmp;
        }

        static public decimal GetVariantPrice(int VariantID)
        {
            return Prices.GetVariantPrice(VariantID);
        }

        static public decimal GetVariantWeight(int VariantID)
        {
            decimal pr = System.Decimal.Zero;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select Weight from productvariant   with (NOLOCK)  where VariantID=" + VariantID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        pr = DB.RSFieldDecimal(rs, "Weight");
                    }
                }
            }

            return pr;
        }

        static public int GetVariantPoints(int VariantID)
        {
            int pr = 0;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select Points from productvariant   with (NOLOCK)  where VariantID=" + VariantID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        pr = DB.RSFieldInt(rs, "Points");
                    }
                }
            }

            return pr;
        }

        static public decimal GetVariantSalePrice(int VariantID)
        {
            return Prices.GetVariantSalePrice(VariantID);
            //decimal pr = System.Decimal.Zero;

            //using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            //{
            //    con.Open();
            //    using (IDataReader rs = DB.GetRS("select SalePrice from productvariant   with (NOLOCK)  where VariantID=" + VariantID.ToString(), con))
            //    {
            //        if (rs.Read())
            //        {
            //            pr = DB.RSFieldDecimal(rs, "SalePrice");
            //        }
            //    }
            //}

            //return pr;
        }

        static public decimal DetermineLevelPrice(int VariantID, int CustomerLevelID, out bool IsOnSale)
        {
            return Prices.DetermineLevelPrice(VariantID, CustomerLevelID, out IsOnSale);

            //// the way the site is written, this should NOT be called with CustomerLevelID=0 but, you never know
            //// if that's the case, return the sale price if any, and if not, the regular price instead:
            //decimal pr = System.Decimal.Zero;
            //IsOnSale = false;
            //if (CustomerLevelID == 0)
            //{
            //    using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            //    {
            //        con.Open();
            //        using (IDataReader rs = DB.GetRS("select * from productvariant   with (NOLOCK)  where VariantID=" + VariantID.ToString(), con))
            //        {
            //            if (rs.Read())
            //            {
            //                if (DB.RSFieldDecimal(rs, "SalePrice") != System.Decimal.Zero)
            //                {
            //                    pr = DB.RSFieldDecimal(rs, "SalePrice");
            //                    IsOnSale = true;
            //                }
            //                else
            //                {
            //                    pr = DB.RSFieldDecimal(rs, "Price");
            //                }
            //            }
            //            else
            //            {
            //                // well, this is bad, we can't return 0, and we don't have ANY valid price to return...stop the web page!
            //                throw (new ApplicationException("Invalid Variant Price Structure, VariantID=" + VariantID.ToString()));
            //            }
            //        }
            //    }
            //}
            //else
            //{
            //    // ok, now for the hard part (e.g. the fun)
            //    // determine the actual price for this thing, considering everything involved!
            //    // If we have an extended price, get that first!
            //    bool ExtendedPriceFound = false;

            //    using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            //    {
            //        con.Open();
            //        using (IDataReader rs = DB.GetRS("select Price from ExtendedPrice  with (NOLOCK)  where VariantID=" + VariantID.ToString() + " and CustomerLevelID=" + CustomerLevelID.ToString() + " and VariantID in (select VariantID from ProductVariant where ProductID in (select ProductID from ProductCustomerLevel where CustomerLevelID=" + CustomerLevelID.ToString() + "))", con))
            //        {
            //            if (rs.Read())
            //            {
            //                pr = DB.RSFieldDecimal(rs, "Price");
            //                ExtendedPriceFound = true;
            //            }
            //        }
            //    }

            //    if (!ExtendedPriceFound)
            //    {
            //        pr = AppLogic.GetVariantPrice(VariantID);
            //    }

            //    // now get the "level" info:
            //    using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            //    {
            //        con.Open();
            //        using (IDataReader rs = DB.GetRS("select * from CustomerLevel   with (NOLOCK)  where CustomerLevelID=" + CustomerLevelID.ToString(), con))
            //        {
            //            if (rs.Read())
            //            {
            //                Decimal DiscountPercent = DB.RSFieldDecimal(rs, "LevelDiscountPercent");
            //                bool LevelDiscountsApplyToExtendedPrices = DB.RSFieldBool(rs, "LevelDiscountsApplyToExtendedPrices");
            //                rs.Close();
            //                if (DiscountPercent != 0.0M)
            //                {
            //                    if (!ExtendedPriceFound || (ExtendedPriceFound && LevelDiscountsApplyToExtendedPrices))
            //                    {
            //                        pr = pr * (decimal)(1.00M - (DiscountPercent / 100.0M));
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}
            //return Decimal.Round(pr, 2, MidpointRounding.AwayFromZero);
        }

        static public int GetTaxClassID(String Name)
        {
            int tmp = 0;
            if (Name.Length != 0)
            {
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS("select TaxClassID from  TaxClass  with (NOLOCK)  where name like " + DB.SQuote("%" + Name + "%"), con))
                    {
                        if (rs.Read())
                        {
                            tmp = DB.RSFieldInt(rs, "TaxClassID");
                        }
                    }
                }
            }
            return tmp;
        }


        static public int GetNumSlides(String inDir)
        {
            String tPath = inDir;
            if (inDir.IndexOf(":") == -1 && inDir.IndexOf("\\\\") == -1)
            {
                tPath = CommonLogic.SafeMapPath(inDir);
            }
            bool anyFound = false;
            for (int i = 1; i <= AppLogic.AppConfigUSInt("MaxSlides"); i++)
            {
                if (!CommonLogic.FileExists(tPath + "slide" + i.ToString().PadLeft(2, '0') + ".jpg"))
                {
                    return i - 1;
                }
                else
                {
                    anyFound = true;
                }
            }
            return CommonLogic.IIF(anyFound, AppLogic.AppConfigUSInt("MaxSlides"), 0);
        }

        public static String MakeProperPhoneFormat(String PhoneNumber)
        {
            return PhoneNumber;
        }

        static public bool ProductHasBeenDeleted(int ProductID)
        {
            return (DB.GetSqlN("Select count(ProductID) as N from Product   with (NOLOCK)  where Deleted=0 and Published=1 and ProductID=" + ProductID.ToString()) == 0);
        }

        static public bool VariantHasBeenDeleted(int VariantID)
        {
            return (DB.GetSqlN("Select count(VariantID) as N from ProductVariant   with (NOLOCK)  where Deleted=0 and Published=1 and VariantID=" + VariantID.ToString()) == 0);
        }

        static public bool AddressHasBeenDeleted(int AddressID)
        {
            return (DB.GetSqlN("Select count(AddressID) as N from Address   with (NOLOCK)  where Deleted=0 and AddressID=" + AddressID.ToString()) == 0);
        }

        public static String GetAdminDir()
        {
            String AdminDir = AppLogic.AdminDir();
            if (AdminDir.Length == 0)
            {
                AdminDir = "admin";
            }
            if (AdminDir.EndsWith("/"))
            {
                AdminDir = AdminDir.Substring(0, AdminDir.Length - 1);
            }
            return AdminDir;
        }

        public static String GetAdminHTTPLocation(bool TryToUseSSL)
        {
            return GetStoreHTTPLocation(TryToUseSSL);
        }

        public static bool OnLiveServer()
        {
            return (CommonLogic.ServerVariables("HTTP_HOST").IndexOf(AppLogic.LiveServer(), StringComparison.InvariantCultureIgnoreCase) != -1);
        }

        public static String GetProductEntityMappings(int ProductID, String EntityName)
        {
            StringBuilder tmpS = new StringBuilder(512);
            String separator = String.Empty;
            String sql = String.Format("select * from product{0} with (NOLOCK) where ProductID={1} order by displayorder", EntityName, ProductID.ToString());
            String idxfield = EntityName + "ID";

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS(sql, con))
                {
                    while (rs.Read())
                    {
                        tmpS.Append(separator);
                        tmpS.Append(DB.RSFieldInt(rs, idxfield).ToString());
                        separator = ",";
                    }
                }
            }

            return tmpS.ToString();
        }

        public static int GetProductDisplayOrder(int ProductID, int CategoryID)
        {
            int tmp = 0;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select DisplayOrder from ProductCategory  with (NOLOCK)  where ProductID=" + ProductID.ToString() + " and categoryid=" + CategoryID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSFieldInt(rs, "DisplayOrder");
                    }
                }
            }

            return tmp;
        }

        // returns the "next" product in this category, after the specified product
        // "next" is defined as either the product that is next higher display order, or same display order and next highest alphabetical order
        // is circular also (i.e. if last, return first)
        public static int GetNextProduct(int ProductID, int CategoryID, int SectionID, int ManufacturerID, int ProductTypeID, bool SortByLooks, bool includeKits, bool includePacks)
        {
            String sql = String.Empty;

            if (CategoryID == 0 && SectionID == 0 && ManufacturerID == 0)
            {
                CategoryID = AppLogic.GetFirstProductEntityID(AppLogic.LookupHelper("Category", 0), ProductID, false);
            }
            String tbl = String.Empty;
            if (CategoryID != 0)
            {
                tbl = "PC";
                sql = "SELECT P.ProductID FROM ((Category C  with (NOLOCK)  INNER JOIN ProductCategory PC   with (NOLOCK)  ON c.CategoryID = Pc.CategoryID) INNER JOIN Product P   with (NOLOCK)  ON Pc.ProductID = P.ProductID) WHERE " + tbl + ".categoryid=" + CategoryID.ToString() + "  and (P.Published = 1) AND (P.Deleted = 0) " + CommonLogic.IIF(includeKits, String.Empty, " and P.IsAKit=0") + CommonLogic.IIF(includePacks, String.Empty, " and P.IsAPack=0") + " and Pc.Categoryid=" + CategoryID.ToString() + " AND P.ProductID IN (SELECT ProductID FROM ProductStore WHERE StoreID = " + AppLogic.StoreID() + ")";
            }
            else if (SectionID != 0)
            {
                tbl = "PS";
                sql = "SELECT P.ProductID from (([Section] S  with (NOLOCK)  INNER JOIN ProductSection PS   with (NOLOCK)  ON S.SectionID = PS.SectionID) INNER JOIN Product P   with (NOLOCK)  ON PS.ProductID = P.ProductID) WHERE " + tbl + ".Sectionid=" + SectionID.ToString() + " and (P.Published = 1) AND (P.Deleted = 0) " + CommonLogic.IIF(includeKits, String.Empty, " and P.IsAKit=0") + CommonLogic.IIF(includePacks, String.Empty, " and P.IsAPack=0") + " and PS.Sectionid=" + SectionID.ToString() + " AND P.ProductID IN (SELECT ProductID FROM ProductStore WHERE StoreID = " + AppLogic.StoreID() + ")";
            }
            else if (ManufacturerID != 0)
            {
                tbl = "PM";
                sql = "SELECT P.ProductID from (([Manufacturer] M  with (NOLOCK)  INNER JOIN ProductManufacturer PM   with (NOLOCK)  ON M.ManufacturerID = PM.ManufacturerID) INNER JOIN Product P   with (NOLOCK)  ON PM.ProductID = P.ProductID) WHERE " + tbl + ".Manufacturerid=" + ManufacturerID.ToString() + " and (P.Published = 1) AND (P.Deleted = 0) " + CommonLogic.IIF(includeKits, String.Empty, " and P.IsAKit=0") + CommonLogic.IIF(includePacks, String.Empty, " and P.IsAPack=0") + " and PM.Manufacturerid=" + ManufacturerID.ToString() + " AND P.ProductID IN (SELECT ProductID FROM ProductStore WHERE StoreID = " + AppLogic.StoreID() + ")";
            }
            if (ProductTypeID != 0)
            {
                sql += " and p.ProductTypeID=" + ProductTypeID.ToString();
            }

            if (SortByLooks)
            {
                sql += ""; //no longer sort by looks
            }
            else
            {
                sql += " order by " + tbl + ".displayorder";
            }

            int id = NONE_FOUND;

            // add found produtids into a temp collection
            LinkedList<int> productIds = new LinkedList<int>();

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS(sql, con))
                {
                    while (rs.Read())
                    {
                        productIds.AddLast(DB.RSFieldInt(rs, "ProductID"));
                    }
                }
            }

            LinkedListNode<int> foundNode = productIds.Find(ProductID);
            if (null != foundNode)
            {
                // if last return first
                if (foundNode.Next == null)
                {
                    id = productIds.First.Value;
                }
                else
                {
                    id = foundNode.Next.Value;
                }
            }

            return id;
        }

        // returns the "next" variant in this product, after the specified variant
        // "next" is defined as either the product that is next higher display order, or same display order and next highest alphabetical order
        // is circular also (i.e. if last, return first)
        public static int GetNextVariant(int ProductID, int VariantID)
        {
            String sql = "SELECT VariantID from ProductVariant where ProductID=" + ProductID.ToString() + " and Deleted=0 order by DisplayOrder,Name";

            int id = NONE_FOUND;

            LinkedList<int> variantIds = new LinkedList<int>();
            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS(sql, con))
                {
                    while (rs.Read())
                    {
                        variantIds.AddLast(DB.RSFieldInt(rs, "VariantID"));
                    }
                }
            }

            LinkedListNode<int> foundNode = variantIds.Find(VariantID);
            if (null != foundNode)
            {
                // if last return first
                if (foundNode.Next == null)
                {
                    id = variantIds.First.Value;
                }
                else
                {
                    id = foundNode.Next.Value;
                }
            }

            return id;
        }

        // returns the "previous" product in this category, after the specified product
        // "previous" is defined as either the product that is next lower display order, or same display order and next lowest alphabetical order
        // is circular also (i.e. if first, return last)        
        public static int GetPreviousProduct(int ProductID, int CategoryID, int SectionID, int ManufacturerID, int ProductTypeID, bool SortByLooks, bool includeKits, bool includePacks)
        {
            String sql = String.Empty;

            if (CategoryID == 0 && SectionID == 0 && ManufacturerID == 0)
            {
                CategoryID = AppLogic.GetFirstProductEntityID(AppLogic.LookupHelper("Category", 0), ProductID, false);
            }
            String tbl = String.Empty;
            if (CategoryID != 0)
            {
                tbl = "PC";
                sql = "SELECT P.ProductID FROM ((Category C  with (NOLOCK)  INNER JOIN ProductCategory PC   with (NOLOCK)  ON c.CategoryID = Pc.CategoryID) INNER JOIN Product P   with (NOLOCK)  ON Pc.ProductID = P.ProductID) WHERE " + tbl + ".categoryid=" + CategoryID.ToString() + "  and (P.Published = 1) AND (P.Deleted = 0) " + CommonLogic.IIF(includeKits, String.Empty, " and P.IsAKit=0") + CommonLogic.IIF(includePacks, String.Empty, " and P.IsAPack=0") + " and Pc.Categoryid=" + CategoryID.ToString() + " AND P.ProductID IN (SELECT ProductID FROM ProductStore WHERE StoreID = " + AppLogic.StoreID() + ")";
            }
            else if (SectionID != 0)
            {
                tbl = "PS";
                sql = "SELECT P.ProductID from (([Section] S  with (NOLOCK)  INNER JOIN ProductSection PS   with (NOLOCK)  ON S.SectionID = PS.SectionID) INNER JOIN Product P   with (NOLOCK)  ON PS.ProductID = P.ProductID) WHERE " + tbl + ".Sectionid=" + SectionID.ToString() + " and (P.Published = 1) AND (P.Deleted = 0) " + CommonLogic.IIF(includeKits, String.Empty, " and P.IsAKit=0") + CommonLogic.IIF(includePacks, String.Empty, " and P.IsAPack=0") + " and PS.Sectionid=" + SectionID.ToString() + " AND P.ProductID IN (SELECT ProductID FROM ProductStore WHERE StoreID = " + AppLogic.StoreID() + ")";
            }
            else if (ManufacturerID != 0)
            {
                tbl = "PM";
                sql = "SELECT P.ProductID from (([Manufacturer] M  with (NOLOCK)  INNER JOIN ProductManufacturer PM   with (NOLOCK)  ON M.ManufacturerID = PM.ManufacturerID) INNER JOIN Product P   with (NOLOCK)  ON PM.ProductID = P.ProductID) WHERE " + tbl + ".Manufacturerid=" + ManufacturerID.ToString() + " and (P.Published = 1) AND (P.Deleted = 0) " + CommonLogic.IIF(includeKits, String.Empty, " and P.IsAKit=0") + CommonLogic.IIF(includePacks, String.Empty, " and P.IsAPack=0") + " and PM.Manufacturerid=" + ManufacturerID.ToString() + " AND P.ProductID IN (SELECT ProductID FROM ProductStore WHERE StoreID = " + AppLogic.StoreID() + ")";
            }
            if (ProductTypeID != 0)
            {
                sql += " and p.ProductTypeID=" + ProductTypeID.ToString();
            }
            if (SortByLooks)
            {
                sql += " order by " + tbl + ".displayorder"; //no longer sorts by looks
            }

            int id = NONE_FOUND;

            LinkedList<int> productIds = new LinkedList<int>();
            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS(sql, con))
                {
                    while (rs.Read())
                    {
                        productIds.AddLast(DB.RSFieldInt(rs, "ProductID"));
                    }
                }
            }

            LinkedListNode<int> foundNode = productIds.Find(ProductID);
            if (null != foundNode)
            {
                // if first, return last
                if (productIds.First == foundNode)
                {
                    id = productIds.Last.Value;
                }
                else
                {
                    id = foundNode.Previous.Value;
                }
            }

            return id;
        }

        public static int GetProductSequence(string direction, int ProductID, int EntityID, string EntityName, int ProductTypeID, bool SortByLooks, bool includeKits, bool includePacks, Customer cust, out string SEName)
        {
            if (false == direction.Equals("first", StringComparison.InvariantCultureIgnoreCase) &&
                false == direction.Equals("last", StringComparison.InvariantCultureIgnoreCase) &&
                false == direction.Equals("next", StringComparison.InvariantCultureIgnoreCase) &&
                false == direction.Equals("previous", StringComparison.InvariantCultureIgnoreCase))
            {
                direction = "first";
            }
            if (EntityName.Trim() == String.Empty)
            {
                EntityID = AppLogic.GetFirstProductEntityID(AppLogic.LookupHelper("Category", 0), ProductID, false);
                EntityName = "CATEGORY";
            }

            String sql = "exec aspdnsf_ProductSequence @positioning, @ProductID, @EntityName, @EntityID, @ProductTypeID, @IncludeKits, @IncludePacks, @SortByLooks, @CustomerLevelID, @affiliateID, @StoreID, @FilterProductsByStore, @FilterOutOfStockProducts";
            SqlParameter[] spa = {DB.CreateSQLParameter("@positioning", SqlDbType.VarChar, 10, direction, ParameterDirection.Input), 
                                  DB.CreateSQLParameter("@ProductID", SqlDbType.Int, 4, ProductID, ParameterDirection.Input), 
                                  DB.CreateSQLParameter("@EntityName", SqlDbType.VarChar, 20, EntityName, ParameterDirection.Input), 
                                  DB.CreateSQLParameter("@EntityID", SqlDbType.Int, 4, EntityID, ParameterDirection.Input), 
                                  DB.CreateSQLParameter("@ProductTypeID", SqlDbType.Int, 4, ProductTypeID, ParameterDirection.Input), 
                                  DB.CreateSQLParameter("@IncludeKits", SqlDbType.Bit, 1, includeKits, ParameterDirection.Input), 
                                  DB.CreateSQLParameter("@IncludePacks", SqlDbType.Bit, 1, includePacks, ParameterDirection.Input),
                                  DB.CreateSQLParameter("@SortByLooks", SqlDbType.Bit, 1, SortByLooks, ParameterDirection.Input), 
                                  DB.CreateSQLParameter("@CustomerLevelID", SqlDbType.Int, 4, cust.CustomerLevelID, ParameterDirection.Input),
                                  DB.CreateSQLParameter("@affiliateID", SqlDbType.Int, 4, cust.AffiliateID, ParameterDirection.Input),
                                  DB.CreateSQLParameter("@StoreID", SqlDbType.Int, 4, AppLogic.StoreID(), ParameterDirection.Input),
                                  DB.CreateSQLParameter("@FilterProductsByStore", SqlDbType.Bit, 1, CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowProductFiltering"), true, false), ParameterDirection.Input),
                                  DB.CreateSQLParameter("@FilterOutOfStockProducts", SqlDbType.Bit, 1, AppLogic.AppConfig("HideProductsWithLessThanThisInventoryLevel") != "-1", ParameterDirection.Input)
                                 };

            int id = 0;
            SEName = string.Empty;
            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader dr = DB.GetRS(sql, spa, con))
                {
                    while (dr.Read())
                    {
                        id = Convert.ToInt32(dr["ProductID"]);
                        SEName = dr["SEName"].ToString();
                    }
                }
            }

            return id;
        }

        public static int GetPreviousVariant(int ProductID, int VariantID)
        {
            String sql = "SELECT VariantID from ProductVariant where ProductID=" + ProductID.ToString() + " and deleted=0 order by DisplayOrder,Name";

            int id = NONE_FOUND;

            LinkedList<int> variantIds = new LinkedList<int>();
            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS(sql, con))
                {
                    while (rs.Read())
                    {
                        variantIds.AddLast(DB.RSFieldInt(rs, "VariantID"));
                    }
                }
            }

            LinkedListNode<int> foundNode = variantIds.Find(VariantID);
            if (null != foundNode)
            {
                // if first, return last
                if (variantIds.First == foundNode)
                {
                    id = variantIds.Last.Value;
                }
                else
                {
                    id = foundNode.Previous.Value;
                }
            }

            return id;
        }

        public static bool ManufacturerHasVisibleProducts(int ManufacturerID)
        {
            bool tmp = false;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select count(*) as N from product  with (NOLOCK)  where ManufacturerID=" + ManufacturerID.ToString() + " and deleted=0 and published=1", con))
                {
                    rs.Read();
                    tmp = (DB.RSFieldInt(rs, "N") != 0);
                }
            }

            return tmp;
        }

        public static bool ProductTypeHasVisibleProducts(int ProductTypeID)
        {
            return DB.GetSqlN("select count(*) as N from Product  with (NOLOCK)  where ProductTypeID=" + ProductTypeID.ToString() + " and Deleted=0 and Published=1") > 0;
        }

        public static String GetProductSalePrice(int ProductID, Customer ViewingCustomer)
        {
            return Prices.GetProductSalePrice(ProductID, ViewingCustomer);
            //// NOTE: IGNORE ANY EXTENDED PRICING HERE, THIS ALWAYS RETURNS NORMAL CUSTOMER PRICE AND SALE PRICE
            //// YOU COULD ALTER THAT, BUT IT'S PROBABLY NOT NECESSARY, SPECIALS ARE TYPICALLY ONLY FOR "CONSUMERS"
            //// return string in format: $regularprice,$saleprice (note that $saleprice could be empty), and
            //// note that this proc returns the FIRST sales price of any variant found, if there are multiple sales prices
            //// then you have to write a different proc if you want them returned.
            //String tmpS = String.Empty;

            //using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            //{
            //    con.Open();
            //    using (IDataReader rs = DB.GetRS("select * from product   with (NOLOCK)  left outer join productvariant   with (NOLOCK)  on product.productid=productvariant.productid where saleprice IS NOT NULL and saleprice<>price and product.productid=" + ProductID.ToString(), con))
            //    {
            //        if (rs.Read())
            //        {
            //            tmpS = ViewingCustomer.CurrencyString(DB.RSFieldDecimal(rs, "Price")) + "|" + ViewingCustomer.CurrencyString(DB.RSFieldDecimal(rs, "SalePrice"));
            //        }
            //    }
            //}

            //return tmpS;
        }

        static public String GetUserBox(Customer ThisCustomer, int SkinID)
        {
            StringBuilder tmpS = new StringBuilder(4096);
            tmpS.Append("<table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" >\n");
            tmpS.Append("<tr><td colspan=\"3\" height=20></td></tr>\n");
            tmpS.Append("<tr>\n");
            tmpS.Append("	<td colspan=\"2\" align=\"right\"><span style=\"color: white; font-size: 11px; font-weight: bold;\">" + GetString("common.cs.36", SkinID, ThisCustomer.LocaleSetting) + " <a class=\"DarkCellText\" href=\"account.aspx\"><b>" + ThisCustomer.FullName() + "</b></a></span></td>\n");
            tmpS.Append("	<td width=\"25\"><img src=\"images/spacer.gif\" width=25\" height=\"1\" /></td>\n");
            tmpS.Append("</tr>\n");
            tmpS.Append("<tr><td colspan=\"3\" height=4></td></tr>\n");
            tmpS.Append("<tr>\n");
            tmpS.Append("	<td colspan=\"2\" align=\"right\"><span style=\"color: white; font-size: 11px; font-weight: bold;\"><a class=\"DarkCellText\" href=\"signout.aspx\"><b>" + GetString("common.cs.37", SkinID, ThisCustomer.LocaleSetting) + "</b></a></span></td>\n");
            tmpS.Append("	<td width=\"25\"><img src=\"images/spacer.gif\" width=25\" height=\"1\" /></td>\n");
            tmpS.Append("</tr>\n");
            tmpS.Append("<tr>\n");
            tmpS.Append("	<td colspan=\"3\"><img src=\"~/App_Themes/Skin_(!SKINID!)/images/spacer.gif\" height=\"2\" width=\"1\" /></td></tr>\n");
            tmpS.Append("<tr>\n");
            tmpS.Append("	<td colspan=\"2\" align=\"right\"><span style=\"color: white; font-size: 11px; font-weight: bold;\"><a class=\"DarkCellText\" href=\"signin.aspx\"><b>" + GetString("common.cs.38", SkinID, ThisCustomer.LocaleSetting) + "</b></a></span></td>\n");
            tmpS.Append("	<td width=\"25\"><img src=\"images/spacer.gif\" width=25\" height=\"1\" /></td>\n");
            tmpS.Append("</tr>\n");
            tmpS.Append("</table>\n");
            return tmpS.ToString();
        }

        static public String GetLoginBox(int SkinID)
        {
            StringBuilder tmpS = new StringBuilder(4096);
            tmpS.Append("<form name=\"LoginForm\" method=\"POST\" action=\"signin.aspx\" onsubmit=\"return (validateForm(this) && LoginForm_Validator(this))\">\n");
            tmpS.Append("           <input type=\"hidden\" name=\"IsSubmit\" value=\"true\">\n");
            tmpS.Append("<table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\">\n");
            tmpS.Append("<tr>\n");
            tmpS.Append("	<td colspan=\"2\" align=\"right\"><span style=\"color: white; font-size: 11px; font-weight: bold;\">" + GetString("common.cs.39", SkinID, Thread.CurrentThread.CurrentUICulture.Name) + "</span></td>\n");
            tmpS.Append("	<td width=\"15\"><img src=\"images/spacer.gif\" width=15\" height=\"1\" /></td>\n");
            tmpS.Append("</tr>\n");
            tmpS.Append("<tr>\n");
            tmpS.Append("	<td width=\"100%\" align=\"right\">\n");
            tmpS.Append("		<span style=\"color: white; font-size: 10px; font-weight: bold;\"><nobr>" + GetString("common.cs.40", SkinID, Thread.CurrentThread.CurrentUICulture.Name) + "&nbsp;</nobr></span>\n");
            tmpS.Append("	</td>\n");
            tmpS.Append("	<td align=\"left\">\n");
            tmpS.Append("		<input name=\"EMail\" type=\"text\" size=\"25\" maxlength=\"100\"><input name=\"EMail_vldt\" type=\"hidden\" value=\"[req][blankalert=" + GetString("common.cs.41", SkinID, Thread.CurrentThread.CurrentUICulture.Name) + "]\">\n");
            tmpS.Append("	</td>\n");
            tmpS.Append("	<td width=\"15\"><img src=\"images/spacer.gif\" width=15\" height=\"1\" /></td>\n");
            tmpS.Append("</tr>\n");
            tmpS.Append("<tr>\n");
            tmpS.Append("	<td colspan=\"3\"><img src=\"~/App_Themes/Skin_(!SKINID!)/images/spacer.gif\" height=\"2\" width=\"1\" /></td></tr>\n");
            tmpS.Append("<tr>\n");
            tmpS.Append("	<td width=\"100%\" align=\"right\">\n");
            tmpS.Append("		<span style=\"color: white; font-size: 10px; font-weight: bold;\"><nobr>" + GetString("common.cs.42", SkinID, Thread.CurrentThread.CurrentUICulture.Name) + "&nbsp;</nobr></span>\n");
            tmpS.Append("	</td>\n");
            tmpS.Append("	<td align=\"left\">\n");
            tmpS.Append("		<input name=\"Password\" type=\"password\" size=\"25\" maxlength=\"100\"><input name=\"Password_vldt\" type=\"hidden\" value=\"[req][blankalert=" + GetString("common.cs.43", SkinID, Thread.CurrentThread.CurrentUICulture.Name) + "]\">\n");
            tmpS.Append("	</td>\n");
            tmpS.Append("	<td width=\"15\"><img src=\"images/spacer.gif\" width=15\" height=\"1\" /></td>\n");
            tmpS.Append("</tr>\n");

            tmpS.Append("<tr>\n");
            tmpS.Append("	<td width=\"100%\" align=\"right\" colspan=\"2\">\n");
            tmpS.Append("		<span style=\"color: white; font-size: 10px; font-weight: bold;\"><nobr>" + GetString("common.cs.44", SkinID, Thread.CurrentThread.CurrentUICulture.Name) + "&nbsp;</nobr></span><input type=\"checkbox\" name=\"PersistLogin\" checked><input type=\"submit\" name=\"submit\" value=\"" + GetString("common.cs.45", SkinID, Thread.CurrentThread.CurrentUICulture.Name) + "\">\n");
            tmpS.Append("	</td>\n");
            tmpS.Append("	<td width=\"15\"><img src=\"images/spacer.gif\" width=15\" height=\"1\" /></td>\n");
            tmpS.Append("</tr>\n");
            tmpS.Append("<tr>\n");
            tmpS.Append("	<td colspan=\"2\" align=\"right\"><small><a class=\"DarkCellText\" href=\"signin.aspx\">" + GetString("common.cs.46", SkinID, Thread.CurrentThread.CurrentUICulture.Name) + "</a>&nbsp;&nbsp;&nbsp;&nbsp;<a class=\"DarkCellText\" href=\"createaccount.aspx\">" + GetString("common.cs.47", SkinID, Thread.CurrentThread.CurrentUICulture.Name) + "</a></small></td>\n");
            tmpS.Append("	<td width=\"15\"><img src=\"images/spacer.gif\" width=15\" height=\"1\" /></td>\n");
            tmpS.Append("</tr>\n");
            tmpS.Append("</table>\n");
            tmpS.Append("</form>\n");
            return tmpS.ToString();
        }

        static public String GetSpecialsBox(int categoryID, int showNum, bool showPics, String teaser, int SkinID, String LocaleSetting)
        {
            if (categoryID == 0)
            {
                categoryID = AppLogic.AppConfigUSInt("IsFeaturedCategoryID");
            }
            String CacheName = "SpecialsBox_" + categoryID.ToString() + "_" + showNum.ToString() + "_" + showPics.ToString() + "_" + SkinID.ToString() + "_" + LocaleSetting;
            if (AppLogic.CachingOn)
            {
                String ProductsMenu = (String)HttpContext.Current.Cache.Get(CacheName);
                if (ProductsMenu != null)
                {
                    if (CommonLogic.ApplicationBool("DumpSQL"))
                    {
                        HttpContext.Current.Response.Write("Cache Hit Found!\n");
                    }
                    return ProductsMenu;
                }
            }

            StringBuilder tmpS = new StringBuilder(10000);
            tmpS.Append("<table cellpadding=\"2\" cellspacing=\"0\" border=\"0\" style=\"border-style: solid; border-width: 0px; border-color: #" + AppLogic.AppConfig("HeaderBGColor") + "\">\n");
            tmpS.Append("<tr><td align=\"left\" valign=\"top\">\n");
            tmpS.Append("<img src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/Specials.gif") + "\" border=\"0\" />");
            tmpS.Append("<table width=\"100%\" bgcolor=\"#FFFFFF\" cellpadding=\"4\" cellspacing=\"0\" border=\"0\" style=\"" + AppLogic.AppConfig("BoxFrameStyle") + "\">\n");
            tmpS.Append("<tr><td align=\"" + CommonLogic.IIF(AppLogic.AppConfigBool("ShowSpecialsPics"), "center", "left") + "\" valign=\"top\">\n");

            tmpS.Append("<p align=\"" + CommonLogic.IIF(AppLogic.AppConfigBool("ShowSpecialsPics"), "center", "left") + "\"><b>" + teaser + "</b></p>\n");


            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select COUNT(*) AS N from product   with (NOLOCK)  where deleted=0 and published=1 and productid in (select distinct productid from productcategory   with (NOLOCK)  where categoryid=" + categoryID.ToString() + ")" + " ; " + "select * from product   with (NOLOCK)  where deleted=0 and published=1 and productid in (select distinct productid from productcategory   with (NOLOCK)  where categoryid=" + categoryID.ToString() + ")", con))
                {
                    if (rs.Read())
                    {
                        int N = DB.RSFieldInt(rs, "N");
                        if (N > 0 && rs.NextResult())
                        {
                            int i = 1;
                            while (rs.Read())
                            {
                                if (i > showNum)
                                {
                                    tmpS.Append("<tr><td " + CommonLogic.IIF(showPics, "colspan=\"2\"", String.Empty) + "><hr size=\"1\" class=\"LightCellText\"/><a href=\"showcategory.aspx?categoryid=" + categoryID.ToString() + "&resetfilter=true\">" + GetString("common.cs.25", SkinID, Thread.CurrentThread.CurrentUICulture.Name) + "</a></td></tr>");
                                    break;
                                }
                                tmpS.Append("<a href=\"" + SE.MakeProductLink(DB.RSFieldInt(rs, "ProductID"), DB.RSField(rs, "SEName")) + "\">");
                                if (showPics)
                                {
                                    String ImgUrl = String.Empty;
									ImgUrl = AppLogic.LookupImage("Product", DB.RSFieldInt(rs, "ProductID"), "icon", SkinID, LocaleSetting);

                                    if (ImgUrl.Length != 0)
                                    {
                                        if (AppLogic.AppConfigBool("ShowSpecialsPics"))
                                        {
                                            tmpS.Append("<img src=\"" + ImgUrl + "\" border=\"0\" />");
                                        }
                                    }
                                }
                                tmpS.Append("<b>" + DB.RSFieldByLocale(rs, "Name", LocaleSetting) + "</b></a>");
                                tmpS.Append("");
                                i++;
                            }
                        }
                    }
                }
            }

            tmpS.Append("</td></tr>\n");
            tmpS.Append("</table>\n");
            tmpS.Append("</td></tr>\n");
            tmpS.Append("</table>\n");
            if (CachingOn)
            {
                HttpContext.Current.Cache.Insert(CacheName, tmpS.ToString(), null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }
            return tmpS.ToString();
        }

        static public String GetHelpBox(int SkinID, bool includeFrame, String LocaleSetting, Parser UseParser)
        {
            String CacheName = String.Format("GetHelpBox_{0}_{1}_{2}", SkinID.ToString(), includeFrame.ToString(), LocaleSetting);
            if (AppLogic.CachingOn)
            {
                String Menu = (String)HttpContext.Current.Cache.Get(CacheName);
                if (Menu != null)
                {
                    if (CommonLogic.ApplicationBool("DumpSQL"))
                    {
                        HttpContext.Current.Response.Write("Cache Hit Found!\n");
                    }
                    return Menu;
                }
            }

            StringBuilder tmpS = new StringBuilder(10000);

            if (includeFrame)
            {
                tmpS.Append("<table cellpadding=\"2\" cellspacing=\"0\" border=\"0\" style=\"border-style: solid; border-width: 0px; border-color: #" + AppLogic.AppConfig("HeaderBGColor") + "\">\n");
                tmpS.Append("<tr><td align=\"left\" valign=\"top\">\n");
                tmpS.Append("<img src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/help.gif") + "\" border=\"0\" />");
                tmpS.Append("<table width=\"100%\" cellpadding=\"4\" cellspacing=\"0\" border=\"0\" style=\"" + AppLogic.AppConfig("BoxFrameStyle") + "\">\n");
                tmpS.Append("<tr><td align=\"left\" valign=\"top\">\n");
            }

            Topic t1 = new Topic("helpbox", LocaleSetting, SkinID, UseParser);
            tmpS.Append(t1.Contents);

            if (includeFrame)
            {
                tmpS.Append("</td></tr>\n");
                tmpS.Append("</table>\n");
                tmpS.Append("</td></tr>\n");
                tmpS.Append("</table>\n");
            }

            if (CachingOn)
            {
                HttpContext.Current.Cache.Insert(CacheName, tmpS.ToString(), null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }
            return tmpS.ToString();
        }


        static public String GetTechTalkBox(int SkinID)
        {
            String CacheName = "GetTechTalkBox_" + SkinID.ToString();
            if (AppLogic.CachingOn)
            {
                String Menu = (String)HttpContext.Current.Cache.Get(CacheName);
                if (Menu != null)
                {
                    if (CommonLogic.ApplicationBool("DumpSQL"))
                    {
                        HttpContext.Current.Response.Write("Cache Hit Found!\n");
                    }
                    return Menu;
                }
            }

            StringBuilder tmpS = new StringBuilder(10000);
            tmpS.Append("<table cellpadding=\"2\" cellspacing=\"0\" border=\"0\" style=\"border-style: solid; border-width: 0px; border-color: #" + AppLogic.AppConfig("HeaderBGColor") + "\">\n");
            tmpS.Append("<tr><td align=\"left\" valign=\"top\">\n");
            tmpS.Append("<a href=\"techtalk.aspx\"><img src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/learn.gif") + "\" border=\"0\" /></a>");
            tmpS.Append("<table width=\"100%\" cellpadding=\"4\" cellspacing=\"0\" border=\"0\" style=\"" + AppLogic.AppConfig("BoxFrameStyle") + "\">\n");
            tmpS.Append("<tr><td align=\"left\" valign=\"top\">\n");

            tmpS.Append("NOT IMPLEMENTED YET");

            tmpS.Append("</td></tr>\n");
            tmpS.Append("</table>\n");
            tmpS.Append("</td></tr>\n");
            tmpS.Append("</table>\n");
            if (CachingOn)
            {
                HttpContext.Current.Cache.Insert(CacheName, tmpS.ToString(), null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }
            return tmpS.ToString();
        }

        static public String GetTechTalkBoxExpanded(int SkinID)
        {
            String CacheName = "GetTechTalkBoxExpanded_" + SkinID.ToString();
            if (AppLogic.CachingOn)
            {
                String Menu = (String)HttpContext.Current.Cache.Get(CacheName);
                if (Menu != null)
                {
                    if (CommonLogic.ApplicationBool("DumpSQL"))
                    {
                        HttpContext.Current.Response.Write("Cache Hit Found!\n");
                    }
                    return Menu;
                }
            }


            StringBuilder tmpS = new StringBuilder(10000);
            tmpS.Append("<table width=\"100%\" cellpadding=\"2\" cellspacing=\"0\" border=\"0\" style=\"border-style: solid; border-width: 0px; border-color: #" + AppLogic.AppConfig("HeaderBGColor") + "\">\n");
            tmpS.Append("<tr><td align=\"left\" valign=\"top\">\n");
            tmpS.Append("<a href=\"techtalk.aspx\"><img src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/learnexpanded.gif") + "\" border=\"0\" /></a>");
            tmpS.Append("<table width=\"100%\" cellpadding=\"4\" cellspacing=\"0\" border=\"0\" style=\"" + AppLogic.AppConfig("BoxFrameStyle") + "\">\n");
            tmpS.Append("<tr><td width=\"100%\" align=\"left\" valign=\"top\">\n");

            tmpS.Append("NOT IMPLEMENTED YET");

            tmpS.Append("</td></tr>\n");
            tmpS.Append("</table>\n");
            tmpS.Append("</td></tr>\n");
            tmpS.Append("</table>\n");
            if (CachingOn)
            {
                HttpContext.Current.Cache.Insert(CacheName, tmpS.ToString(), null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }
            return tmpS.ToString();
        }

        static public String GetManufacturersBoxExpanded(int SkinID, String LocaleSetting)
        {
            String CacheName = "GetManufacturersBoxExpanded_" + SkinID.ToString() + "_" + LocaleSetting;
            if (AppLogic.CachingOn)
            {
                String Menu = (String)HttpContext.Current.Cache.Get(CacheName);
                if (Menu != null)
                {
                    if (CommonLogic.ApplicationBool("DumpSQL"))
                    {
                        HttpContext.Current.Response.Write("Cache Hit Found!\n");
                    }
                    return Menu;
                }
            }

            StringBuilder tmpS = new StringBuilder(10000);
            tmpS.Append("<table width=\"100%\" cellpadding=\"2\" cellspacing=\"0\" border=\"0\" style=\"border-style: solid; border-width: 0px; border-color: #" + AppLogic.AppConfig("HeaderBGColor") + "\">\n");
            tmpS.Append("<tr><td align=\"left\" valign=\"top\">\n");
            tmpS.Append("<a href=\"manufacturers.aspx\"><img src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/manufacturersbox.gif") + "\" border=\"0\" /></a>");
            tmpS.Append("<table width=\"100%\" cellpadding=\"4\" cellspacing=\"0\" border=\"0\" style=\"" + AppLogic.AppConfig("BoxFrameStyle") + "\">\n");
            tmpS.Append("<tr><td align=\"left\" valign=\"top\">\n");

            tmpS.Append("<p><b>" + GetString("common.cs.48", SkinID, LocaleSetting) + "</b></p>\n");

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select * from manufacturer   with (NOLOCK)  where deleted=0 order by DisplayOrder,Name", con))
                {
                    while (rs.Read())
                    {
                        tmpS.Append("<b class=\"a4\"><a href=\"" + SE.MakeManufacturerLink(DB.RSFieldInt(rs, "ManufacturerID"), DB.RSField(rs, "SEName")) + "\"><font style=\"font-size: 14px;\">" + DB.RSFieldByLocale(rs, "Name", LocaleSetting));
                        if (DB.RSFieldByLocale(rs, "Summary", LocaleSetting).Length != 0)
                        {
                            tmpS.Append(": " + DB.RSFieldByLocale(rs, "Summary", LocaleSetting));
                        }
                        tmpS.Append("</font></a></b>\n");
                        if (DB.RSFieldByLocale(rs, "Description", LocaleSetting).Length != 0)
                        {
                            String tmpD = DB.RSFieldByLocale(rs, "Description", LocaleSetting);
                            if (AppLogic.ReplaceImageURLFromAssetMgr)
                            {
                                tmpD = tmpD.Replace("../images", "images");
                            }
                            tmpS.Append("<span class=\"a2\">" + tmpD + "</span>\n");
                        }
                        tmpS.Append("<div class=\"a1\" style=\"PADDING-BOTTOM: 10px\">\n");
                        if (DB.RSField(rs, "URL").Length != 0)
                        {
                            tmpS.Append("<a href=\"" + DB.RSField(rs, "URL") + "\" target=\"_blank\">" + DB.RSField(rs, "URL") + "</a>");
                        }
                        tmpS.Append("</div>\n");
                    }
                }
            }



            tmpS.Append("</td></tr>\n");
            tmpS.Append("</table>\n");
            tmpS.Append("</td></tr>\n");
            tmpS.Append("</table>\n");
            if (CachingOn)
            {
                HttpContext.Current.Cache.Insert(CacheName, tmpS.ToString(), null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }
            return tmpS.ToString();
        }


        static public String GetRequestQuoteBoxExpanded(bool useCache, int SkinID, String LocaleSetting)
        {
            String CacheName = "GetRequestQuoteBoxExpanded_" + SkinID.ToString();
            if (AppLogic.CachingOn && useCache)
            {
                String Menu = (String)HttpContext.Current.Cache.Get(CacheName);
                if (Menu != null)
                {
                    if (CommonLogic.ApplicationBool("DumpSQL"))
                    {
                        HttpContext.Current.Response.Write("Cache Hit Found!\n");
                    }
                    return Menu;
                }
            }

            StringBuilder tmpS = new StringBuilder(10000);
            tmpS.Append("<table width=\"100%\" cellpadding=\"2\" cellspacing=\"0\" border=\"0\" style=\"border-style: solid; border-width: 0px; border-color: #" + AppLogic.AppConfig("HeaderBGColor") + "\">\n");
            tmpS.Append("<tr><td align=\"left\" valign=\"top\">\n");
            tmpS.Append("<img src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/requestquoteexpanded.gif") + "\" border=\"0\" />");
            tmpS.Append("<table width=\"100%\" cellpadding=\"4\" cellspacing=\"0\" border=\"0\" style=\"" + AppLogic.AppConfig("BoxFrameStyle") + "\">\n");
            tmpS.Append("<tr><td align=\"left\" valign=\"top\">\n");

            tmpS.Append("<script type=\"text/javascript\">\n<!==\n");
            tmpS.Append("function CustomBidForm_Validator(theForm)\n");
            tmpS.Append("{\n");
            tmpS.Append("	submitonce(theForm);\n");
            tmpS.Append("	if (theForm.Summary.value.length == 0)\n");
            tmpS.Append("	{\n");
            tmpS.Append("		alert(\"" + GetString("common.cs.50", SkinID, LocaleSetting) + "\");\n");
            tmpS.Append("		theForm.Summary.focus();\n");
            tmpS.Append("		submitenabled(theForm);\n");
            tmpS.Append("		return (false);\n");
            tmpS.Append("    }\n");
            tmpS.Append("  return (true);\n");
            tmpS.Append("}\n//-->\n");
            tmpS.Append("</script>\n");

            // CUSTOM QUOTE FORM:
            tmpS.Append("<div align=\"left\">\n");
            tmpS.Append("<form method=\"POST\" action=\"custombid.aspx\" onsubmit=\"return (validateForm(this) && CustomBidForm_Validator(this))\" id=\"CustomBidForm\" name=\"CustomBidForm\">\n");
            tmpS.Append("<b>" + String.Format(GetString("common.cs.51", SkinID, LocaleSetting), SE.MakeDriverLink("contact")) + "\n");
            tmpS.Append("<table cellpadding=\"0\" cellspacing=\"0\" border=\"0\">\n");
            tmpS.Append("<tr>\n");
            tmpS.Append("<td align=\"left\" valign=\"top\">\n");
            tmpS.Append(GetString("common.cs.52", SkinID, LocaleSetting) + "<textarea name=\"Summary\" style=\"width: 100%; height: 250px;\" ></textarea>\n");
            tmpS.Append("</td>\n");
            tmpS.Append("<td width=\"20\"></td>\n");
            tmpS.Append("<td align=\"left\" valign=\"top\">\n");
            tmpS.Append("" + GetString("common.cs.53", SkinID, LocaleSetting) + "\n");
            tmpS.Append("<input type=\"text\" name=\"Name\" size=\"35\">\n");
            tmpS.Append("<input type=\"hidden\" name=\"Name_vldt\" value=\"[req][blankalert=" + GetString("common.cs.54", SkinID, LocaleSetting) + "]\">\n");
            tmpS.Append("" + GetString("common.cs.55", SkinID, LocaleSetting) + "\n");
            tmpS.Append("<input type=\"text\" name=\"Organization\" size=\"35\">\n");
            tmpS.Append("<input type=\"hidden\" name=\"Organization_vldt\" value=\"[req][blankalert=" + GetString("common.cs.56", SkinID, LocaleSetting) + "]\">\n");
            tmpS.Append("" + GetString("common.cs.57", SkinID, LocaleSetting) + "\n");
            tmpS.Append("<input type=\"text\" name=\"EMail\" size=\"35\">\n");
            tmpS.Append("<input type=\"hidden\" name=\"EMail_vldt\" value=\"[req][blankalert=" + GetString("common.cs.58", SkinID, LocaleSetting) + "]\">\n");
            tmpS.Append("" + GetString("common.cs.59", SkinID, LocaleSetting) + "\n");
            tmpS.Append("<input type=\"text\" name=\"Phone\" size=\"35\">\n");
            tmpS.Append("<input type=\"hidden\" name=\"Phone_vldt\" value=\"[req][blankalert=" + GetString("common.cs.60", SkinID, LocaleSetting) + "]\">\n");
            tmpS.Append("<input type=\"submit\" value=\"" + GetString("common.cs.61", SkinID, LocaleSetting) + "\" name=\"B1\">\n");
            tmpS.Append("</td>\n");
            tmpS.Append("</tr>\n");
            tmpS.Append("</table>\n");
            tmpS.Append("</form>\n");
            tmpS.Append("</div>\n");


            tmpS.Append("</td></tr>\n");
            tmpS.Append("</table>\n");
            tmpS.Append("</td></tr>\n");
            tmpS.Append("</table>\n");
            if (CachingOn && useCache)
            {
                HttpContext.Current.Cache.Insert(CacheName, tmpS.ToString(), null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }
            return tmpS.ToString();
        }

        static public String GetSpecialsBoxExpanded(int categoryID, int showNum, bool useCache, bool showPics, bool IncludeFrame, String teaser, int SkinID, String LocaleSetting, Customer ViewingCustomer)
        {
            if (categoryID == 0)
            {
                categoryID = AppLogic.AppConfigUSInt("IsFeaturedCategoryID");
            }

            StringBuilder tmpS = new StringBuilder(10000);

            if (IncludeFrame)
            {
                tmpS.Append("<table width=\"100%\" cellpadding=\"2\" cellspacing=\"0\" border=\"0\" style=\"border-style: solid; border-width: 0px; border-color: #" + AppLogic.AppConfig("HeaderBGColor") + "\">\n");
                tmpS.Append("<tr><td align=\"left\" valign=\"top\">\n");
                tmpS.Append("<a href=\"showcategory.aspx?categoryid=" + categoryID.ToString() + "&resetfilter=true\"><img src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/Specialsexpanded.gif") + "\" border=\"0\" /></a>");
                tmpS.Append("<table width=\"100%\" cellpadding=\"4\" cellspacing=\"0\" border=\"0\" style=\"" + AppLogic.AppConfig("BoxFrameStyle") + "\">\n");
                tmpS.Append("<tr><td align=\"left\" valign=\"top\">\n");
            }

            tmpS.Append("<p><b>" + teaser + "</b></p>\n");


            string sql = "select p.*, pv.price, isnull(pv.saleprice, 0) saleprice, isnull(ep.price, 0) extendedprice ";
            sql += "from product p ";
            sql += "    join productcategory pc on p.productid = pc.productid ";
            sql += "    join productvariant pv on p.productid = pv.productid and pv.IsDefault = 1";
            sql += "    left join extendedprice ep on pv.variantid = ep.variantid and ep.CustomerLevelID = " + ViewingCustomer.CustomerLevelID.ToString() + "  ";
            sql += "    left join (select VariantID, sum(quan) quan from Inventory with (nolock) group by VariantID) i on i.VariantID = pv.VariantID ";
            sql += "where p.deleted=0  ";
            sql += "    and p.published=1 ";
            sql += "    and pc.categoryid = " + categoryID.ToString();
            sql += "    and case p.TrackInventoryBySizeAndColor when 1 then isnull(i.quan, 0) else pv.inventory end >= " + AppLogic.AppConfigNativeInt("HideProductsWithLessThanThisInventoryLevel").ToString();

            tmpS.Append("<table width=\"100%\" cellpadding=\"2\" cellspacing=\"0\" border=\"0\">\n");
            int i = 1;
            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS(sql, con))
                {
                    while (rs.Read())
                    {
                        if (i > showNum)
                        {
                            tmpS.Append("<tr><td " + CommonLogic.IIF(showPics, "colspan=\"2\"", String.Empty) + "><hr size=\"1\" class=\"LightCellText\"/><a href=\"showcategory.aspx?categoryid=" + categoryID.ToString() + "&resetfilter=true\">" + GetString("common.cs.62", SkinID, LocaleSetting) + "</a></td></tr>");
                            break;
                        }
                        if (i > 1)
                        {
                            tmpS.Append("<tr><td " + CommonLogic.IIF(showPics, "colspan=\"2\"", String.Empty) + "><hr size=\"1\" class=\"LightCellText\"/></td></tr>");
                        }
                        tmpS.Append("<tr>");
                        String ImgUrl = String.Empty;

						ImgUrl = AppLogic.LookupImage("Product", DB.RSFieldInt(rs, "ProductID"), "icon", SkinID, LocaleSetting);

                        if (showPics)
                        {
                            tmpS.Append("<td align=\"left\" valign=\"top\">\n");
                            tmpS.Append("<a href=\"" + SE.MakeProductLink(DB.RSFieldInt(rs, "ProductID"), DB.RSField(rs, "SEName")) + "\">");
                            tmpS.Append("<img align=\"left\" src=\"" + ImgUrl + "\" border=\"0\" />");
                            tmpS.Append("</a>");
                            tmpS.Append("</td>");
                        }

                        tmpS.Append("<td align=\"left\" valign=\"top\">\n");
                        tmpS.Append("<b class=\"a4\">");
                        tmpS.Append("<a href=\"" + SE.MakeProductLink(DB.RSFieldInt(rs, "ProductID"), DB.RSField(rs, "SEName")) + "\">" + DB.RSFieldByLocale(rs, "Name", LocaleSetting));
                        tmpS.Append("</a>");

                        decimal RegPrice = DB.RSFieldDecimal(rs, "price");
                        decimal SalePrice = DB.RSFieldDecimal(rs, "saleprice");
                        decimal ExtPrice = DB.RSFieldDecimal(rs, "extendedprice");

                        if (SalePrice > 0 && ViewingCustomer.CustomerLevelID == 0)
                        {
                            tmpS.Append("<strike>" + GetString("common.cs.63", SkinID, LocaleSetting) + " " + ViewingCustomer.CurrencyString(RegPrice) + "</strike>" + GetString("common.cs.64", SkinID, LocaleSetting) + " " + ViewingCustomer.CurrencyString(SalePrice));
                        }
                        else if (ViewingCustomer.LevelDiscountPct > 0.0M || ExtPrice > 0.0M)
                        {
                            decimal CustLvlPrice = CommonLogic.IIF(ExtPrice == 0.0M, RegPrice * (1.00M - (ViewingCustomer.LevelDiscountPct / 100.0M)), CommonLogic.IIF(ViewingCustomer.DiscountExtendedPrices, ExtPrice * (1.00M - (ViewingCustomer.LevelDiscountPct / 100.0M)), ExtPrice));
                            tmpS.Append("<strike>" + GetString("common.cs.63", SkinID, LocaleSetting) + " " + ViewingCustomer.CurrencyString(RegPrice) + "</strike><span class=\"LevelPrice\" style=\"color:" + AppLogic.AppConfig("OnSaleForTextColor") + "\">" + ViewingCustomer.CustomerLevelName + " Price: " + ViewingCustomer.CurrencyString(CustLvlPrice) + "</span>");
                        }
						
                        if (DB.RSFieldByLocale(rs, "Summary", LocaleSetting).Length != 0)
                        {
                            tmpS.Append("" + DB.RSFieldByLocale(rs, "Summary", LocaleSetting));
                        }
                        tmpS.Append("</b>\n");

                        if (DB.RSFieldByLocale(rs, "Description", LocaleSetting).Length != 0)
                        {
                            String tmpD = DB.RSFieldByLocale(rs, "Description", LocaleSetting);
                            if (AppLogic.ReplaceImageURLFromAssetMgr)
                            {
                                tmpD = tmpD.Replace("../images", "images");
                            }
                            tmpS.Append("<span class=\"a2\">" + tmpD + "</span>\n");
                        }
                        tmpS.Append("<div class=\"a1\" style=\"PADDING-BOTTOM: 10px\">\n");
                        tmpS.Append("<a href=\"" + SE.MakeProductLink(DB.RSFieldInt(rs, "ProductID"), DB.RSField(rs, "SEName")) + "\">");
                        tmpS.Append(GetString("common.cs.49", SkinID, LocaleSetting));
                        tmpS.Append("</a>");
                        tmpS.Append("</div>\n");
                        tmpS.Append("</td>");
                        tmpS.Append("</tr>");
                        i++;
                    }
                }
            }
            tmpS.Append("</table>\n");
			
            if (IncludeFrame)
            {
                tmpS.Append("</td></tr>\n");
                tmpS.Append("</table>\n");
                tmpS.Append("</td></tr>\n");
                tmpS.Append("</table>\n");
            }

            return tmpS.ToString();
        }

        static public String GetSpecialsBoxExpandedRandom(int categoryID, bool showPics, bool IncludeFrame, String teaser, int SkinID, String LocaleSetting, Customer ViewingCustomer)
        {
            if (categoryID == 0)
            {
                categoryID = AppLogic.AppConfigUSInt("IsFeaturedCategoryID");
            }

            StringBuilder tmpS = new StringBuilder(4096);

            if (IncludeFrame)
            {
                tmpS.Append("<table width=\"100%\" cellpadding=\"2\" cellspacing=\"0\" border=\"0\" style=\"border-style: solid; border-width: 0px; border-color: #" + AppLogic.AppConfig("HeaderBGColor") + "\">\n");
                tmpS.Append("<tr><td align=\"left\" valign=\"top\">\n");
                tmpS.Append("<a href=\"showcategory.aspx?categoryid=" + categoryID.ToString() + "&resetfilter=true\"><img src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/Specialsexpanded.gif") + "\" border=\"0\" /></a>");
                tmpS.Append("<table width=\"100%\" cellpadding=\"4\" cellspacing=\"0\" border=\"0\" style=\"" + AppLogic.AppConfig("BoxFrameStyle") + "\">\n");
                tmpS.Append("<tr><td align=\"left\" valign=\"top\">\n");
            }

            tmpS.Append("<p><b>" + teaser + "</b></p>\n");

            string sql = "from product p ";
            sql += "    join productcategory pc on p.productid = pc.productid ";
            sql += "    join productvariant pv on p.productid = pv.productid and pv.IsDefault = 1";
            sql += "    left join extendedprice ep on pv.variantid = ep.variantid and ep.CustomerLevelID = " + ViewingCustomer.CustomerLevelID.ToString() + "  ";
            sql += "    left join (select VariantID, sum(quan) quan from Inventory with (nolock) group by VariantID) i on i.VariantID = pv.VariantID ";
            sql += "where p.deleted=0  ";
            sql += "    and p.published=1 ";
            sql += "    and pc.categoryid = " + categoryID.ToString();
            sql += "    and case p.TrackInventoryBySizeAndColor when 1 then isnull(i.quan, 0) else pv.inventory end >= " + AppLogic.AppConfigNativeInt("HideProductsWithLessThanThisInventoryLevel").ToString();

            string querySql = "select p.*, pv.price, isnull(pv.saleprice, 0) saleprice, isnull(ep.price, 0) extendedprice " + sql;
            string countSql = "select COUNT(*) AS N " + sql;

            tmpS.Append("<table width=\"100%\" cellpadding=\"2\" cellspacing=\"0\" border=\"0\">\n");


            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS(countSql + " ; " + querySql, con))
                {
                    if (rs.Read())
                    {
                        int NumRecs = DB.RSFieldInt(rs, "N");
                        int ShowRecNum = CommonLogic.GetRandomNumber(1, NumRecs);
                        int i = 1;

                        if (rs.NextResult())
                        {
                            while (rs.Read())
                            {
                                if (i == ShowRecNum)
                                {
                                    tmpS.Append("<tr>");
                                    String ImgUrl = String.Empty;

									ImgUrl = AppLogic.LookupImage("Product", DB.RSFieldInt(rs, "ProductID"), "icon", SkinID, LocaleSetting);

                                    if (showPics)
                                    {
                                        tmpS.Append("<td align=\"left\" valign=\"top\">\n");
                                        tmpS.Append("<a href=\"" + SE.MakeProductLink(DB.RSFieldInt(rs, "ProductID"), DB.RSField(rs, "SEName")) + "\">");
                                        tmpS.Append("<img align=\"left\" src=\"" + ImgUrl + "\" border=\"0\" />");
                                        tmpS.Append("</a>");
                                        tmpS.Append("</td>");
                                    }

                                    tmpS.Append("<td align=\"left\" valign=\"top\">\n");
                                    tmpS.Append("<b class=\"a4\">");
                                    tmpS.Append("<a href=\"" + SE.MakeProductLink(DB.RSFieldInt(rs, "ProductID"), DB.RSField(rs, "SEName")) + "\">" + DB.RSFieldByLocale(rs, "Name", LocaleSetting));
                                    tmpS.Append("</a>");

                                    decimal RegPrice = DB.RSFieldDecimal(rs, "price");
                                    decimal SalePrice = DB.RSFieldDecimal(rs, "saleprice");
                                    decimal ExtPrice = DB.RSFieldDecimal(rs, "extendedprice");
                                    if (SalePrice > 0 && ViewingCustomer.CustomerLevelID == 0)
                                    {
                                        tmpS.Append("<strike>" + GetString("common.cs.63", SkinID, LocaleSetting) + " " + ViewingCustomer.CurrencyString(RegPrice) + "</strike>" + GetString("common.cs.64", SkinID, LocaleSetting) + " " + ViewingCustomer.CurrencyString(SalePrice));
                                    }
                                    else if (ViewingCustomer.LevelDiscountPct > 0.0M || ExtPrice > 0.0M)
                                    {
                                        decimal CustLvlPrice = CommonLogic.IIF(ExtPrice == 0.0M, RegPrice * (1.00M - (ViewingCustomer.LevelDiscountPct / 100.0M)), CommonLogic.IIF(ViewingCustomer.DiscountExtendedPrices, ExtPrice * (1.00M - (ViewingCustomer.LevelDiscountPct / 100.0M)), ExtPrice));
                                        tmpS.Append("<strike>" + GetString("common.cs.63", SkinID, LocaleSetting) + " " + ViewingCustomer.CurrencyString(RegPrice) + "</strike><span class=\"LevelPrice\" style=\"color:" + AppLogic.AppConfig("OnSaleForTextColor") + "\">" + ViewingCustomer.CustomerLevelName + " Price: " + ViewingCustomer.CurrencyString(CustLvlPrice) + "</span>");
                                    }

                                    if (DB.RSFieldByLocale(rs, "Summary", LocaleSetting).Length != 0)
                                    {
                                        tmpS.Append("" + DB.RSFieldByLocale(rs, "Summary", LocaleSetting));
                                    }
                                    tmpS.Append("</b>\n");
                                    if (DB.RSFieldByLocale(rs, "Description", LocaleSetting).Length != 0)
                                    {
                                        String tmpD = DB.RSFieldByLocale(rs, "Description", LocaleSetting);
                                        if (AppLogic.ReplaceImageURLFromAssetMgr)
                                        {
                                            tmpD = tmpD.Replace("../images", "images");
                                        }
                                        tmpS.Append("<span class=\"a2\">" + tmpD + "</span>\n");
                                    }
                                    tmpS.Append("<div class=\"a1\" style=\"PADDING-BOTTOM: 10px\">\n");
                                    tmpS.Append("<a href=\"" + SE.MakeProductLink(DB.RSFieldInt(rs, "ProductID"), DB.RSField(rs, "SEName")) + "\">");
                                    tmpS.Append(GetString("common.cs.49", SkinID, LocaleSetting));
                                    tmpS.Append("</a>");
                                    tmpS.Append("</div>\n");
                                    tmpS.Append("</td>");
                                    tmpS.Append("</tr>");
                                }
                                i++;
                            }
                        }
                    }
                }
            }

            tmpS.Append("<tr><td " + CommonLogic.IIF(showPics, "colspan=\"2\"", String.Empty) + "><hr size=\"1\" class=\"LightCellText\"/><a href=\"showcategory.aspx?categoryid=" + categoryID.ToString() + "&resetfilter=true\">Show me more specials...</a></td></tr>");
            tmpS.Append("</table>\n");
						
            if (IncludeFrame)
            {
                tmpS.Append("</td></tr>\n");
                tmpS.Append("</table>\n");
                tmpS.Append("</td></tr>\n");
                tmpS.Append("</table>\n");
            }

            return tmpS.ToString();
        }

        static public String GetSearchBox(int SkinID, String LocaleSetting)
        {
            String CacheName = "GetSearchBox_" + SkinID.ToString() + "_" + LocaleSetting;
            if (AppLogic.CachingOn)
            {
                String Menu = (String)HttpContext.Current.Cache.Get(CacheName);
                if (Menu != null)
                {
                    if (CommonLogic.ApplicationBool("DumpSQL"))
                    {
                        HttpContext.Current.Response.Write("Cache Hit Found!\n");
                    }
                    return Menu;
                }
            }

            StringBuilder tmpS = new StringBuilder(10000);
            tmpS.Append("<table cellpadding=\"2\" cellspacing=\"0\" border=\"0\" style=\"border-style: solid; border-width: 0px; border-color: #" + AppLogic.AppConfig("HeaderBGColor") + "\">\n");
            tmpS.Append("<tr><td align=\"left\" valign=\"top\">\n");
            tmpS.Append("<img src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/search.gif", LocaleSetting) + "\" border=\"0\" />");
            tmpS.Append("<table width=\"100%\" cellpadding=\"4\" cellspacing=\"0\" border=\"0\" style=\"" + AppLogic.AppConfig("BoxFrameStyle") + "\">\n");
            tmpS.Append("<tr><td align=\"left\" valign=\"top\">\n");

            tmpS.Append("<script type=\"text/javascript\">\n");
            tmpS.Append("function SearchBoxForm_Validator(theForm)\n");
            tmpS.Append("{\n");
            tmpS.Append("  submitonce(theForm);\n");
            tmpS.Append("  if (theForm.SearchTerm.value.length < " + AppLogic.AppConfig("MinSearchStringLength") + ")\n");
            tmpS.Append("  {\n");
            tmpS.Append("    alert('" + String.Format(GetString("common.cs.66", SkinID, LocaleSetting), AppLogic.AppConfig("MinSearchStringLength")) + "');\n");
            tmpS.Append("    theForm.SearchTerm.focus();\n");
            tmpS.Append("    submitenabled(theForm);\n");
            tmpS.Append("    return (false);\n");
            tmpS.Append("  }\n");
            tmpS.Append("  return (true);\n");
            tmpS.Append("}\n");
            tmpS.Append("</script>\n");

            tmpS.Append("<form style=\"margin-top: 0px; margin-bottom: 0px;\" name=\"SearchBoxForm\" action=\"searchadv.aspx\" method=\"GET\" onsubmit=\"return SearchBoxForm_Validator(this)\">\n");
            tmpS.Append("<input type=\"hidden\" name=\"IsSubmit\" value=\"true\" />" + GetString("common.cs.82", SkinID, LocaleSetting) + " <input name=\"SearchTerm\" size=\"10\" /><img src=\"images/spacer.gif\" width=\"4\" height=\"4\" /><INPUT NAME=\"submit\" TYPE=\"Image\" ALIGN=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/go.gif") + "\" border=\"0\" />\n");
            tmpS.Append("</form>");
            tmpS.Append("</td></tr>\n");
            tmpS.Append("</table>\n");
            tmpS.Append("</td></tr>\n");
            tmpS.Append("</table>\n");
            if (CachingOn)
            {
                HttpContext.Current.Cache.Insert(CacheName, tmpS.ToString(), null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }
            return tmpS.ToString();
        }

        static public int GetProductTypeID(int ProductID)
        {
            int tmp = 0;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select producttypeid from product   with (NOLOCK)  where ProductID=" + ProductID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSFieldInt(rs, "ProductTypeID");
                    }
                }
            }

            return tmp;
        }

        public static String GetAppConfigName(int AppConfigID)
        {
            String tmp = String.Empty;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("Select name Name AppConfig  with (NOLOCK)  where AppConfigID=" + AppConfigID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSField(rs, "Name");
                    }
                }
            }

            return tmp;
        }

        static public String GetVariantSKUSuffix(int VariantID)
        {
            String tmpS = String.Empty;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("Select * from ProductVariant   with (NOLOCK)  where VariantID=" + VariantID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmpS = DB.RSField(rs, "SKUSuffix");
                    }
                }
            }

            return tmpS;
        }

        // ONLY a helper function to GetDefaultProductVariant now!
        static private int x_GetFirstProductVariant(int ProductID)
        {
            int tmp = 0;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select top 1 VariantID from ProductVariant   with (NOLOCK)  where deleted=0 and published=1 and productid=" + ProductID.ToString() + " order by DisplayOrder,Name", con))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSFieldInt(rs, "VariantID");
                    }
                }
            }

            return tmp;
        }

        static public int GetDefaultProductVariant(int ProductID)
        {
            return GetDefaultProductVariant(ProductID, true);
        }

        static public int GetDefaultProductVariant(int ProductID, bool PublishedOnly)
        {
            int tmp = 0;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select top 1 VariantID from ProductVariant   with (NOLOCK)  where deleted=0 " + CommonLogic.IIF(PublishedOnly, " and published=1", String.Empty) + " and IsDefault=1 and ProductID=" + ProductID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSFieldInt(rs, "VariantID");
                    }
                }
            }

            if (tmp == 0)
            {
                tmp = x_GetFirstProductVariant(ProductID);
            }

            return tmp;
        }

        public static String GetPollCategories(int PollID)
        {
            StringBuilder tmpS = new StringBuilder(1000);

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select * from Pollcategory   with (NOLOCK)  where Pollid=" + PollID.ToString(), con))
                {
                    while (rs.Read())
                    {
                        if (tmpS.Length != 0)
                        {
                            tmpS.Append(",");
                        }
                        tmpS.Append(DB.RSFieldInt(rs, "CategoryID").ToString());
                    }
                }
            }

            return tmpS.ToString();
        }

        public static String GetPollSections(int PollID)
        {
            StringBuilder tmpS = new StringBuilder(1000);

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select * from Pollsection   with (NOLOCK)  where Pollid=" + PollID.ToString(), con))
                {
                    while (rs.Read())
                    {
                        if (tmpS.Length != 0)
                        {
                            tmpS.Append(",");
                        }
                        tmpS.Append(DB.RSFieldInt(rs, "SectionID").ToString());
                    }
                }
            }

            return tmpS.ToString();
        }

        public static String GetPollName(int PollID, String LocaleSetting)
        {
            String tmpS = String.Empty;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select * from Poll   with (NOLOCK)  where Pollid=" + PollID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmpS = DB.RSFieldByLocale(rs, "Name", LocaleSetting);
                    }
                }
            }

            return tmpS;
        }

        static public bool ShowProductBuyButton(int ProductID)
        {
            bool tmp = true;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select showbuybutton from product   with (NOLOCK)  where ProductID=" + ProductID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSFieldBool(rs, "showbuybutton");
                    }
                }
            }

            return tmp;
        }

        static public bool ProductIsCallToOrder(int ProductID)
        {
            bool tmp = true;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select IsCallToOrder from product   with (NOLOCK)  where ProductID=" + ProductID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSFieldBool(rs, "IsCallToOrder");
                    }
                }
            }

            return tmp;
        }

        static public int MultiShipMaxNumItemsAllowed()
        {
            int tmp = AppLogic.AppConfigUSInt("MultiShipMaxItemsAllowed");

            if (tmp == 0)
            {
                tmp = 25; // force a default
            }
            return tmp;
        }

        static public int GetVariantProductID(int VariantID)
        {
            int tmp = 0;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select productid from productvariant   with (NOLOCK)  where variantid=" + VariantID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSFieldInt(rs, "ProductID");
                    }
                }
            }

            return tmp;
        }

        /// <summary>
        /// Converts a State/Province/County Name to its Abbreviation. If no match is found in the given country, the input is returned.
        /// </summary>
        static public string GetStateAbbreviation(string stateName, string countryName)
        {
            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS(
                        "select Abbreviation from dbo.State s with (NOLOCK) " +
                        "inner join dbo.Country c on c.CountryID = s.CountryID " +
                        "where s.Name = " + DB.SQuote(stateName) + " and c.Name = " + DB.SQuote(countryName), con))
                {
                    while (rs.Read())
                    {
                        return DB.RSField(rs, "Abbreviation");
                    }
                }
            }

            return stateName;
        }

		static public int GetStateID (String StateAbbreviation)
		{
			int tmp = 0;

			using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using (IDataReader rs = DB.GetRS("Select * from state with (NOLOCK)  order by DisplayOrder,Name", con))
				{
					while (rs.Read())
					{
						if (StateAbbreviation.Equals(DB.RSField(rs, "Abbreviation"), StringComparison.InvariantCultureIgnoreCase))
						{
							tmp = DB.RSFieldInt(rs, "StateID");
							break;
						}
					}
				}
			}

			return tmp;
		}

		static public int GetStateID (String StateAbbreviation, int CountryID)
		{
			int tmp = 0;
			string nonUS = "--";

			if (StateAbbreviation.Equals(nonUS))
			{
				tmp = GetStateID(StateAbbreviation);
			}
			else
			{
				string sSql = String.Format("Select * from state with (NOLOCK) where Abbreviation={0} and CountryID={1} order by DisplayOrder,Name", DB.SQuote(StateAbbreviation), CountryID);

				using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
				{
					con.Open();
					using (IDataReader rs = DB.GetRS(sSql, con))
					{
						while (rs.Read())
						{
							if (StateAbbreviation.Equals(DB.RSField(rs, "Abbreviation"), StringComparison.InvariantCultureIgnoreCase))
							{
								tmp = DB.RSFieldInt(rs, "StateID");
								break;
							}
						}
					}
				}
			}

			return tmp;
		}

		static public int GetStateIDByName (String StateName)
		{
			int tmp = 0;

			using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using (IDataReader rs = DB.GetRS("Select * from state with (NOLOCK)  order by DisplayOrder,Name", con))
				{
					while (rs.Read())
					{
						if (StateName.Equals(DB.RSField(rs, "Name"), StringComparison.InvariantCultureIgnoreCase))
						{
							tmp = DB.RSFieldInt(rs, "StateID");
							break;
						}
					}
				}
			}

			return tmp;
		}

        /// <summary>
        /// Used for Shipping and Tax Estimate control
        /// </summary>
        /// <param name="StateID"></param>
        /// <param name="TaxClassID"></param>
        /// <returns></returns>
        static public Decimal GetStateTaxRate(int StateID, int TaxClassID)
        {
            Decimal tmp = 0.0M;
            tmp = StateTaxRatesTable.GetTaxRate(StateID, TaxClassID);
            return tmp;
        }

        /// <summary>
        /// Used for Shipping and Tax Estimate control
        /// </summary>
        /// <param name="TaxClassID"></param>
        /// <param name="StateName"></param>
        /// <returns></returns>
        static public Decimal GetStateTaxRate(int TaxClassID, string StateName)
        {
            return GetStateTaxRate(GetStateIDByName(StateName), TaxClassID);
        }

        /// <summary>
        /// Used for Shipping and Tax Estimate control
        /// </summary>
        /// <param name="StateAbbrev"></param>
        /// <param name="TaxClassID"></param>
        /// <returns></returns>
        static public Decimal GetStateTaxRate(String StateAbbrev, int TaxClassID)
        {
            return GetStateTaxRate(GetStateID(StateAbbrev), TaxClassID);
        }

        /// <summary>
        /// Used for Shipping and Tax Estimate control
        /// </summary>
        /// <param name="CountryID"></param>
        /// <param name="TaxClassID"></param>
        /// <returns></returns>
        static public Decimal GetCountryTaxRate(int CountryID, int TaxClassID)
        {
            Decimal tmp = 0.0M;
            tmp = CountryTaxRatesTable.GetTaxRate(CountryID, TaxClassID);
            return tmp;
        }

        /// <summary>
        /// Used for Shipping and Tax Estimate control
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="TaxClassID"></param>
        /// <returns></returns>
        static public Decimal GetCountryTaxRate(String Name, int TaxClassID)
        {
            return GetCountryTaxRate(GetCountryID(Name), TaxClassID);
        }

        static public String GetCountryNameFromTwoLetterISOCode(string CountryTwoLetterISOCode)
        {
            String tmp = String.Empty;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("Select * from Country with (NOLOCK)  order by DisplayOrder,Name", con))
                {
                    while (rs.Read())
                    {
                        if (CountryTwoLetterISOCode.Equals(DB.RSField(rs, "TwoLetterISOCode"), StringComparison.InvariantCultureIgnoreCase))
                        {
                            tmp = DB.RSField(rs, "Name");
                            break;
                        }
                    }
                }
            }

            return tmp;
        }

        static public String GetCountryNameFromThreeLetterISOCode(string CountryThreeLetterISOCode)
        {
            String tmp = String.Empty;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("Select * from Country with (NOLOCK)  order by DisplayOrder,Name", con))
                {
                    while (rs.Read())
                    {
                        if (CountryThreeLetterISOCode.Equals(DB.RSField(rs, "ThreeLetterISOCode"), StringComparison.InvariantCultureIgnoreCase))
                        {
                            tmp = DB.RSField(rs, "Name");
                            break;
                        }
                    }
                }
            }

            return tmp;
        }

        static public Int32 GetCountryIDFromTwoLetterISOCode(string CountryTwoLetterISOCode)
        {
            if (CountryTwoLetterISOCode.Length != 2)
            {
                return 0;
            }

            Int32 tmp = 0;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("Select * from Country with (NOLOCK)  order by DisplayOrder,Name", con))
                {
                    while (rs.Read())
                    {
                        if (CountryTwoLetterISOCode.Equals(DB.RSField(rs, "TwoLetterISOCode"), StringComparison.InvariantCultureIgnoreCase))
                        {
                            tmp = DB.RSFieldInt(rs, "CountryID");
                            break;
                        }
                    }
                }
            }

            return tmp;
        }

        static public Int32 GetCountryIDFromThreeLetterISOCode(string CountryThreeLetterISOCode)
        {
            if (CountryThreeLetterISOCode.Length != 3)
            {
                return 0;
            }

            Int32 tmp = 0;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("Select * from Country with (NOLOCK)  order by DisplayOrder,Name", con))
                {
                    while (rs.Read())
                    {
                        if (CountryThreeLetterISOCode.Equals(DB.RSField(rs, "ThreeLetterISOCode"), StringComparison.InvariantCultureIgnoreCase))
                        {
                            tmp = DB.RSFieldInt(rs, "CountryID");
                            break;
                        }
                    }
                }
            }

            return tmp;
        }

        static public String GetCountryTwoLetterISOCode(String CountryName)
        {
            String tmp = "US";

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("Select * from Country with (NOLOCK)  order by DisplayOrder,Name", con))
                {
                    while (rs.Read())
                    {
                        if (CountryName.Equals(DB.RSField(rs, "Name"), StringComparison.InvariantCultureIgnoreCase))
                        {
                            tmp = DB.RSField(rs, "TwoLetterISOCode");
                            break;
                        }
                    }
                }
            }

            return tmp;
        }

        static public String GetCountryThreeLetterISOCode(String CountryName)
        {
            String tmp = "US";

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("Select * from Country with (NOLOCK)  order by DisplayOrder,Name", con))
                {
                    while (rs.Read())
                    {
                        if (CountryName.Equals(DB.RSField(rs, "Name"), StringComparison.InvariantCultureIgnoreCase))
                        {
                            tmp = DB.RSField(rs, "ThreeLetterISOCode");
                            break;
                        }
                    }
                }
            }

            return tmp;
        }

        static public int GetCountryID(String Name)
        {
            int tmp = 0;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("Select * from Country with (NOLOCK)  order by DisplayOrder,Name", con))
                {
                    while (rs.Read())
                    {
                        if (Name.Equals(DB.RSField(rs, "Name"), StringComparison.InvariantCultureIgnoreCase))
                        {
                            tmp = DB.RSFieldInt(rs, "CountryID");
                            break;
                        }
                    }
                }
            }

            return tmp;
        }
		
        /// <summary>
        /// Check the inputed Zip Code and match the value from Country PostalCodeRegEx Regular Expression
        /// </summary>
        /// <param name="ZipCode">ZipCode entered by user</param>
        /// <param name="CountryID">The country chosen</param>
        /// <returns>returns the validated postal code for each specific country</returns>
        public static Boolean ValidatePostalCode(String ZipCode, int CountryID)
        {
            if (AppLogic.GetCountryPostalCodeRequired(CountryID))
            {
                string ZipRegEx = AppLogic.GetCountryPostalCodeRegEx(CountryID);
                if (!CommonLogic.IsStringNullOrEmpty(ZipRegEx))
                {
                    Regex regExpValue = new Regex(ZipRegEx);
                    Match regExpMatch = regExpValue.Match(ZipCode);

                    return regExpMatch.Success;
                }
            }
            return true;
        }

        static public String GetCountryPostalCodeRegEx(int CountryID)
        {
            String tmp = String.Empty;

            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS(String.Format("Select * from Country with (NOLOCK) where CountryID={0}", CountryID), conn))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSField(rs, "PostalCodeRegex");
                    }
                }
            }

            return tmp;
        }

        static public Boolean GetCountryPostalCodeRequired(int CountryID)
        {
            bool tmp = false;

            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS(String.Format("Select * from Country with (NOLOCK) where CountryID={0}", CountryID), conn))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSFieldBool(rs, "PostalCodeRequired");
                    }
                }
            }

            return tmp;
        }
		
        static public String GetCountryPostalExample(int CountryID)
        {
            String tmp = String.Empty;
            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS(String.Format("Select * from Country with (NOLOCK) where CountryID={0}", CountryID), conn))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSField(rs, "PostalCodeExample");
                    }
                }
            }

            return tmp;
        }

        /// <summary>
        /// Gets the error message whenever country zip does not match the specified PostalCodeRegex
        /// </summary>
        /// <param name="CountryID">The countryid of whom to get the PostalCodeRegex</param>
        /// <param name="skinID">The currently used skinID</param>
        /// <param name="LocaleSetting">The currently used Locale Setting</param>
        /// <returns>returns string</returns>
        public static String GetCountryPostalErrorMessage(int CountryID, int skinID, String LocaleSetting)
        {
            String tmp = "";

            tmp = AppLogic.GetCountryPostalExample(CountryID);

            return String.Format("{0} {1}", AppLogic.GetString("admin.common.postalcodeerrormessage", skinID, LocaleSetting), tmp);
        }

		static public Decimal GetZipTaxRate(String Zip)
        {
            Decimal taxrate = 0.0M;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS(String.Format("Select TaxRate from ZipTaxRate   with (NOLOCK)  where ZipCode = {0}", DB.SQuote(Zip)), con))
                {
                    if (rs.Read())
                    {
                        taxrate = DB.RSFieldDecimal(rs, "TaxRate");
                    }
                }
            }

            return taxrate;
        }

        public static bool ProductIsInCategory(int ProductID, int CategoryID)
        {
            bool IsInCat = false;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select count(*) as N from productcategory   with (NOLOCK)  where productid=" + ProductID.ToString() + " and categoryid=" + CategoryID.ToString(), con))
                {
                    rs.Read();
                    IsInCat = (DB.RSFieldInt(rs, "N") != 0);
                }
            }

            return IsInCat;
        }

        public static String GetProductCustomerLevels(int ProductID)
        {
            StringBuilder tmpS = new StringBuilder(1000);
            if (ProductID != 0)
            {
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS("select * from productcustomerlevel   with (NOLOCK)  where productid=" + ProductID.ToString(), con))
                    {
                        while (rs.Read())
                        {
                            if (tmpS.Length != 0)
                            {
                                tmpS.Append(",");
                            }
                            tmpS.Append(DB.RSFieldInt(rs, "CustomerLevelID").ToString());
                        }
                    }
                }
            }
            return tmpS.ToString();
        }

        public static String GetProductGenres(int ProductID)
        {
            StringBuilder tmpS = new StringBuilder(1000);
            if (ProductID != 0)
            {
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS("select * from ProductGenre   with (NOLOCK)  where productid=" + ProductID.ToString(), con))
                    {
                        while (rs.Read())
                        {
                            if (tmpS.Length != 0)
                            {
                                tmpS.Append(",");
                            }
                            tmpS.Append(DB.RSFieldInt(rs, "GenreID").ToString());
                        }
                    }
                }
            }
            return tmpS.ToString();
        }

        public static String GetProductVectors(int ProductID)
        {
            StringBuilder tmpS = new StringBuilder(1000);
            if (ProductID != 0)
            {
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS("select * from ProductVector   with (NOLOCK)  where productid=" + ProductID.ToString(), con))
                    {
                        while (rs.Read())
                        {
                            if (tmpS.Length != 0)
                            {
                                tmpS.Append(",");
                            }
                            tmpS.Append(DB.RSFieldInt(rs, "VectorID").ToString());
                        }
                    }
                }
            }
            return tmpS.ToString();
        }

        public static String GetProductDistributors(int ProductID)
        {
                StringBuilder tmpS = new StringBuilder(1000);
                if (ProductID != 0)
                {
                    using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                    {
                        con.Open();
                        using (IDataReader rs = DB.GetRS("select * from productdistributor   with (NOLOCK)  where productid=" + ProductID.ToString(), con))
                        {
                            while (rs.Read())
                            {
                                if (tmpS.Length != 0)
                                {
                                    tmpS.Append(",");
                                }
                                tmpS.Append(DB.RSFieldInt(rs, "DistributorID").ToString());
                            }
                        }
                    }
                }
                return tmpS.ToString();
            }

        public static int GetFirstProduct(int CategoryID, bool AllowKits, bool AllowPacks)
        {
            int id = 0;
            if (CategoryID != 0)
            {
                String sql = "exec aspdnsf_GetProducts @sortEntityName = 'category'";

                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS(sql, con))
                    {
                        if (rs.Read())
                        {
                            id = DB.RSFieldInt(rs, "ProductID");
                        }
                    }
                }
            }

            return id;
        }

        static public String GetEntityName(String EntityName, int EntityID, String LocaleSetting)
        {
            String tmpS = String.Empty;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS(String.Format("select Name from {0} with (NOLOCK) where {1}ID={2}", EntityName, EntityName, EntityID.ToString()), con))
                {
                    if (rs.Read())
                    {
                        tmpS = DB.RSFieldByLocale(rs, "Name", LocaleSetting);
                    }
                }
            }

            return tmpS;
        }

        public static String GetFirstProductEntity(EntityHelper EntityHelper, int ProductID, bool ForProductBrowser, String LocaleSetting)
        {
            String tmpS = EntityHelper.GetObjectEntities(ProductID, ForProductBrowser);
            if (tmpS.Length == 0)
            {
                return String.Empty;
            }
            String[] ss = tmpS.Split(',');
            String result = String.Empty;
            try
            {
                result = EntityHelper.GetEntityName(Localization.ParseUSInt(ss[0]), LocaleSetting);
            }
            catch { }
            return result;
        }

        public static int GetFirstProductEntityID(EntityHelper EntityHelper, int ProductID, bool ForProductBrowser)
        {
            String tmpS = EntityHelper.GetObjectEntities(ProductID, ForProductBrowser);
            if (tmpS.Length == 0)
            {
                return 0;
            }
            String[] ss = tmpS.Split(',');
            int result = 0;
            try
            {
                result = Localization.ParseUSInt(ss[0]);
            }
            catch { }
            return result;
        }

        public static String GetProductName(int ProductID, String LocaleSetting)
        {
            String tmpS = String.Empty;
            if (ProductID != 0)
            {
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS("select Name from product   with (NOLOCK)  where productid=" + ProductID.ToString(), con))
                    {
                        if (rs.Read())
                        {
                            tmpS = DB.RSFieldByLocale(rs, "Name", LocaleSetting);
                        }
                    }
                }
            }
            return tmpS;
        }

        public static bool GetProductIsShipSeparately(int ProductID)
        {
            bool tmpS = false;
            if (ProductID != 0)
            {
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS("select IsShipSeparately from productvariant   with (NOLOCK)  where productid=" + ProductID.ToString(), con))
                    {
                        if (rs.Read())
                        {
                            tmpS = DB.RSFieldBool(rs, "IsShipSeparately");
                        }
                    }
                }
            }
            return tmpS;
        }

        public static String GetProductXmlPackage(int ProductID)
        {
            String tmpS = String.Empty;
            if (ProductID != 0)
            {
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS("select XmlPackage from product   with (NOLOCK)  where productid=" + ProductID.ToString(), con))
                    {
                        if (rs.Read())
                        {
                            tmpS = DB.RSField(rs, "XmlPackage").ToLowerInvariant();
                        }
                    }
                }
            }
            return tmpS;
        }

        public static String GetLocaleSettingDescription(String LocaleSetting)
        {
            String tmp = String.Empty;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select * from LocaleSetting   with (NOLOCK)  order by displayorder,description", con))
                {
                    while (rs.Read())
                    {
                        if (LocaleSetting == DB.RSField(rs, "Name"))
                        {
                            tmp = DB.RSField(rs, "Description");
                        }
                    }
                }
            }

            return tmp;
        }

        public static int GetLocaleSettingID(String LocaleSetting)
        {
            int tmp = 0;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select * from LocaleSetting   with (NOLOCK)  order by displayorder,description", con))
                {
                    while (rs.Read())
                    {
                        if (LocaleSetting == DB.RSField(rs, "Name"))
                        {
                            tmp = DB.RSFieldInt(rs, "LocaleSettingID");
                        }
                    }
                }
            }

            return tmp;
        }

        public static String GetRequiresProducts(int ProductID)
        {
            String tmpS = String.Empty;
            if (ProductID != 0)
            {
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS("select RequiresProducts from product   with (NOLOCK)  where productid=" + ProductID.ToString(), con))
                    {
                        if (rs.Read())
                        {
                            tmpS = DB.RSField(rs, "RequiresProducts");
                        }
                    }
                }
            }
            return tmpS;
        }

        public static String GetVariantName(int VariantID, String LocaleSetting)
        {
            String tmpS = String.Empty;
            if (VariantID != 0)
            {
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS("select Name from productvariant   with (NOLOCK)  where VariantID=" + VariantID.ToString(), con))
                    {
                        if (rs.Read())
                        {
                            tmpS = DB.RSFieldByLocale(rs, "Name", LocaleSetting);
                        }
                    }
                }
            }
            return tmpS;
        }

        public static String GetRestrictedQuantities(int VariantID, out int MinimumQuantity)
        {
            MinimumQuantity = 0;
            String tmpS = String.Empty;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select RestrictedQuantities,MinimumQuantity from productvariant  with (NOLOCK)  where VariantID=" + VariantID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmpS = DB.RSField(rs, "RestrictedQuantities");
                        MinimumQuantity = DB.RSFieldInt(rs, "MinimumQuantity");
                    }
                }
            }

            return tmpS;
        }

        public static String GetProductSKU(int ProductID)
        {
            String tmpS = String.Empty;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select SKU from product   with (NOLOCK)  where productid=" + ProductID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmpS = DB.RSField(rs, "SKU");
                    }
                }
            }

            return tmpS;
        }

        //--------------------------------------------------------------------
        //photo resize methods
        //--------------------------------------------------------------------

        public static Hashtable SplitConfig(String configValue)
        {
            char[] trimChars = { ' ', ';' };

            String imgAppConfigParam = AppConfig(configValue).TrimEnd(trimChars);

            Hashtable m_mapEntries = new Hashtable();

            // Split the input string
            string[] arEntries = imgAppConfigParam.Split(';');
            for (int i = 0; i < arEntries.Length; ++i)
            {
                // Split this entry in to key and value
                string[] arPieces = arEntries[i].Split(':');

                try
                {
                    if (arPieces.Length > 1)
                    {
                        // Add this key/value pair (trimmed and lowercase) to our map
                        m_mapEntries[arPieces[0].Trim().ToLower()] = arPieces[1].Trim().ToLower();
                    }
                }
                catch { }

            }

            return m_mapEntries;
        }

        public static Hashtable SplitConfig(String EntityOrObjectName, String img_SizeType)
        {
            char[] trimChars = { ' ', ';' };

            String imgAppConfigParam = AppConfig(EntityOrObjectName + "Img_" + img_SizeType).TrimEnd(trimChars);

            Hashtable m_mapEntries = new Hashtable();

            // Split the input string
            string[] arEntries = imgAppConfigParam.Split(';');
            for (int i = 0; i < arEntries.Length; ++i)
            {
                // Split this entry in to key and value
                string[] arPieces = arEntries[i].Split(':');

                try
                {
                    if (arPieces.Length > 1)
                    {
                        // Add this key/value pair (trimmed and lowercase) to our map
                        m_mapEntries[arPieces[0].Trim().ToLower()] = arPieces[1].Trim().ToLower();
                    }
                }
                catch { }

            }

            return m_mapEntries;
        }

        public static Hashtable SplitConfig(String EntityOrObjectName, String img_SizeType, String img_Properties)
        {
            char[] trimChars = { ' ', ';', ',' };

            String imgParams = img_Properties.TrimEnd(trimChars);

            Hashtable m_mapEntries = new Hashtable();

            // Split the input string
            string[] arEntries = imgParams.Split(';');
            for (int i = 0; i < arEntries.Length; ++i)
            {
                // Split this entry in to key and value
                string[] arPieces = arEntries[i].Split(':');

                try
                {
                    if (arPieces.Length > 1)
                    {
                        // Add this key/value pair (trimmed and lowercase) to our map
                        m_mapEntries[arPieces[0].Trim().ToLower()] = arPieces[1].Trim().ToLower();
                    }
                }
                catch { }

            }

            return m_mapEntries;
        }

        /// <summary>
        /// Begin resizing images imported via admin
        /// </summary>
        /// <param name="EntityOrObjectName">Entity type (Category, Manufacturer, etc) or "Product" or "Variant"</param>
        /// <param name="tmpImg_fName">Full path to temp image to resize from</param>
        /// <param name="img_fName">Full path of image to be saved</param>
        /// <param name="img_SizeType">Icon, Medium, Large</param>
        /// <param name="img_ContentType">jpeg, gif, or .png</param>

        public static void ResizeEntityOrObject(String EntityOrObjectName, String tmpImg_fName, String img_fName, String img_SizeType, String img_ContentType)
        {
            Hashtable spltConfig = new Hashtable();

            // TODO: Image must be fully qualified (System.Drawing.Image) to support VB conversion 
            // During conversion System.Drawing will be lost...must be re-added
            System.Drawing.Image ImageToResize = System.Drawing.Image.FromFile(tmpImg_fName, true);

            switch (EntityOrObjectName.ToUpperInvariant())
            {
                case "CATEGORY":
                    spltConfig = SplitConfig("Category", img_SizeType);
                    break;
                case "MANUFACTURER":
                    spltConfig = SplitConfig("Manufacturer", img_SizeType);
                    break;
                case "SECTION":
                    spltConfig = SplitConfig("Section", img_SizeType);
                    break;
                case "DISTRIBUTOR":
                    spltConfig = SplitConfig("Distributor", img_SizeType);
                    break;
                //unsupported
                /*
                case "GENRE":
                    splitConfig = SplitResizeConfig("Genre", img_SizeType);
                    break;
                case "VECTOR":
                    splitConfig = SplitResizeConfig("Vector", img_SizeType);
                    break;
                case "LIBRARY":
                    splitConfig = SplitResizeConfig("Library", img_SizeType);
                    break;
                */
                case "PRODUCT":
                    spltConfig = SplitConfig("Product", img_SizeType);
                    break;
                case "VARIANT":
                    spltConfig = SplitConfig("Variant", img_SizeType);
                    break;
                default:
                    return;
            }

            ResizePhoto(spltConfig, ImageToResize, img_fName, tmpImg_fName, img_SizeType, img_ContentType);
        }

        /// <summary>
        /// Begin image resizing for WSI or any other method where the image params have already been declared
        /// </summary>
        /// <param name="EntityOrObjectName">Entity type (Category, Manufacturer, etc) or "Product" or "Variant"</param>
        /// <param name="tmpImg_fName">Full path to temp image to resize from</param>
        /// <param name="img_fName">Full path of image to be saved</param>
        /// <param name="eoID">Original name of image (ID, SKU, or IFNO)</param>
        /// <param name="img_SizeType">Icon, Medium, Large</param>
        /// <param name="img_ContentType">jpeg, gif, or .png</param>
        /// <param name="img_Params">Width, Height, Quality, etc... for images</param>
        /// <param name="img_UseAppConfigs">Boolean value that determines whether appconfig parameters are used or not</param>

        public static void ResizeEntityOrObject(String EntityOrObjectName, String tmpImg_fName, String img_fName, String eoID, String img_SizeType, String img_ContentType, String img_Params, Boolean img_UseAppConfigs)
        {
            Hashtable splitConfigParams = new Hashtable();
            Hashtable splitConfigAppConfigs = new Hashtable();

            // TODO: Image must be fully qualified (System.Drawing.Image) to support VB conversion 
            // During conversion System.Drawing will be lost...must be re-added
            System.Drawing.Image ImageToResize = System.Drawing.Image.FromFile(tmpImg_fName, true);

            splitConfigParams = SplitConfig(EntityOrObjectName.ToUpperInvariant(), img_SizeType, img_Params);

            if (img_UseAppConfigs)
            {
                splitConfigAppConfigs = SplitConfig(EntityOrObjectName, img_SizeType);
            }

            ResizePhoto(splitConfigParams, splitConfigAppConfigs, ImageToResize, img_fName, tmpImg_fName, img_SizeType, img_ContentType);

            eoID = eoID.Replace(".", "").Replace("jpg", "").Replace("jpeg", "").Replace("gif", "").Replace("png", "");

            if (img_SizeType.Equals("large", StringComparison.InvariantCultureIgnoreCase))
            {
                CreateOthersFromLarge(EntityOrObjectName, tmpImg_fName, eoID, img_ContentType);
            }
            if (img_SizeType.Equals("medium", StringComparison.InvariantCultureIgnoreCase))
            {
                AppLogic.MakeMicroPic(eoID, tmpImg_fName, img_Params, img_SizeType);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="splitConfigParams"></param>
        /// <param name="splitConfigAppConfigs"></param>
        /// <param name="origPhoto">System.Drawing.Image representing the original image to be resized.</param>
        /// <param name="img_fName"></param>
        /// <param name="tmpImg_fName"></param>
        /// <param name="img_SizeType"></param>
        /// <param name="img_ContentType"></param>
        /// TODO: Image must be fully qualified (System.Drawing.Image) to support VB conversion.
        /// During conversion System.Drawing will be lost...must be re-added.
        public static void ResizePhoto(Hashtable splitConfigParams, Hashtable splitConfigAppConfigs, System.Drawing.Image origPhoto, String img_fName, String tmpImg_fName, String img_SizeType, String img_ContentType)
        {
            Boolean resizeMe = true;

            if (splitConfigAppConfigs.Count > 0)
            {
                if (splitConfigAppConfigs.ContainsKey("resize") && splitConfigAppConfigs["resize"].ToString() == "false")
                {
                    resizeMe = false;
                }
            }

            if (resizeMe)
            {
                int resizedWidth = AppConfigNativeInt("DefaultWidth_" + img_SizeType);
                int resizedHeight = AppConfigNativeInt("DefaultHeight_" + img_SizeType);

                int resizedQuality = AppConfigNativeInt("DefaultQuality");

                String stretchMe = AppConfig("DefaultStretch");
                String cropMe = AppConfig("DefaultCrop");
                String cropV = AppConfig("DefaultCropVertical");
                String cropH = AppConfig("DefaultCropHorizontal");
                String fillColor = AppConfig("DefaultFillColor");

                int sourceWidth = origPhoto.Width;
                int sourceHeight = origPhoto.Height;
                int sourceX = 0;
                int sourceY = 0;
                // we will extend 2 pixels on all sides to avoid the border bug
                // we could use InterpolationMode.NearestNeighbor instead of
                // InterpolationMode.HighQualityBicubic but not without sacrificing quality
                int destX = -2;
                int destY = -2;

                float nPercent = 0;
                float nPercentW = 0;
                float nPercentH = 0;

                int destWidth = 0;
                int destHeight = 0;

                if (splitConfigAppConfigs.Count > 0)
                {
                    if (splitConfigAppConfigs.ContainsKey("width"))
                    {
                        if (CommonLogic.IsInteger(splitConfigAppConfigs["width"].ToString()))
                        {
                            resizedWidth = Int32.Parse(splitConfigAppConfigs["width"].ToString());
                        }
                    }
                    if (splitConfigAppConfigs.ContainsKey("height"))
                    {
                        if (CommonLogic.IsInteger(splitConfigAppConfigs["height"].ToString()))
                        {
                            resizedHeight = Int32.Parse(splitConfigAppConfigs["height"].ToString());
                        }
                    }
                    if (splitConfigAppConfigs.ContainsKey("quality"))
                    {
                        if (CommonLogic.IsInteger(splitConfigAppConfigs["quality"].ToString()))
                        {
                            resizedQuality = Int32.Parse(splitConfigAppConfigs["quality"].ToString());
                        }
                    }
                    if (splitConfigAppConfigs.ContainsKey("stretch"))
                    {
                        stretchMe = splitConfigAppConfigs["stretch"].ToString();
                    }
                    if (splitConfigAppConfigs.ContainsKey("fill"))
                    {
                        fillColor = splitConfigAppConfigs["fill"].ToString();
                    }
                    if (splitConfigAppConfigs.ContainsKey("crop"))
                    {
                        cropMe = splitConfigAppConfigs["crop"].ToString();
                        if (cropMe == "true")
                        {
                            if (splitConfigAppConfigs.ContainsKey("cropv"))
                            {
                                cropV = splitConfigAppConfigs["cropv"].ToString();
                            }
                            if (splitConfigAppConfigs.ContainsKey("croph"))
                            {
                                cropH = splitConfigAppConfigs["croph"].ToString();
                            }
                        }
                    }
                }
                //check for params passed in through WSI
                if (splitConfigParams.ContainsKey("width"))
                {
                    if (CommonLogic.IsInteger(splitConfigParams["width"].ToString()))
                    {
                        resizedWidth = Int32.Parse(splitConfigParams["width"].ToString());
                    }
                }
                if (splitConfigParams.ContainsKey("height"))
                {
                    if (CommonLogic.IsInteger(splitConfigParams["height"].ToString()))
                    {
                        resizedHeight = Int32.Parse(splitConfigParams["height"].ToString());
                    }
                }
                if (splitConfigParams.ContainsKey("quality"))
                {
                    if (CommonLogic.IsInteger(splitConfigParams["quality"].ToString()))
                    {
                        resizedQuality = Int32.Parse(splitConfigParams["quality"].ToString());
                    }
                }
                if (splitConfigParams.ContainsKey("stretch"))
                {
                    stretchMe = splitConfigParams["stretch"].ToString();
                }
                if (splitConfigParams.ContainsKey("fill"))
                {
                    fillColor = splitConfigParams["fill"].ToString();
                }
                if (splitConfigParams.ContainsKey("crop"))
                {
                    cropMe = splitConfigParams["crop"].ToString();
                    if (cropMe == "true")
                    {
                        if (splitConfigParams.ContainsKey("cropv"))
                        {
                            cropV = splitConfigParams["cropv"].ToString();
                        }
                        if (splitConfigParams.ContainsKey("croph"))
                        {
                            cropH = splitConfigParams["croph"].ToString();
                        }
                    }
                }
                //
                if (resizedWidth < 1 || resizedHeight < 1)
                {
                    resizedWidth = origPhoto.Width;
                    resizedHeight = origPhoto.Height;
                }


                if (cropMe == "true")
                {
                    String AnchorUpDown = cropV;
                    String AnchorLeftRight = cropH;
                    nPercentW = ((float)resizedWidth / (float)sourceWidth);
                    nPercentH = ((float)resizedHeight / (float)sourceHeight);

                    if (nPercentH < nPercentW)
                    {
                        nPercent = nPercentW;
                        switch (AnchorUpDown)
                        {
                            case "top":
                                destY = -2;
                                break;
                            case "bottom":
                                destY = (int)(resizedHeight - (sourceHeight * nPercent));
                                break;
                            case "center":
                            default:
                                destY = (int)((resizedHeight - (sourceHeight * nPercent)) / 2) - 2;
                                break;
                        }
                    }
                    else
                    {
                        nPercent = nPercentH;
                        switch (AnchorLeftRight.ToUpper())
                        {
                            case "left":
                                destX = 0;
                                break;
                            case "right":
                                destX = (int)(resizedWidth - (sourceWidth * nPercent));
                                break;
                            case "middle":
                            default:
                                destX = (int)((resizedWidth - (sourceWidth * nPercent)) / 2) - 2;
                                break;
                        }
                    }
                }
                else
                {
                    nPercentW = ((float)resizedWidth / (float)sourceWidth);
                    nPercentH = ((float)resizedHeight / (float)sourceHeight);

                    if (nPercentH < nPercentW)
                    {
                        nPercent = nPercentH;
                        destX = (int)((resizedWidth - (sourceWidth * nPercent)) / 2);
                    }
                    else
                    {
                        nPercent = nPercentW;
                        destY = (int)((resizedHeight - (sourceHeight * nPercent)) / 2) - 2;
                    }
                }
                // let's account for the extra pixels we left to avoid the borderbug here
                // some distortion will occur...but it should be unnoticeable
                if (stretchMe == "false" && (origPhoto.Width < resizedWidth && origPhoto.Height < resizedHeight))
                {
                    destWidth = origPhoto.Width;
                    destHeight = origPhoto.Height;
                    destX = (int)((resizedWidth / 2) - (origPhoto.Width / 2));
                    destY = (int)((resizedHeight / 2) - (origPhoto.Height / 2));
                }
                else
                {
                    destWidth = (int)Math.Ceiling(sourceWidth * nPercent) + 4;
                    destHeight = (int)Math.Ceiling(sourceHeight * nPercent) + 4;
                }

                SavePhoto(resizedWidth, resizedHeight, destHeight, destWidth, destX, destY, sourceHeight, sourceWidth, sourceX, sourceY, origPhoto, img_fName, fillColor, resizedQuality, img_ContentType);
            }
            else
            {
                origPhoto.Save(img_fName);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configValues"></param>
        /// <param name="origPhoto">System.Drawing.Image representing the original image to be resized.</param>
        /// <param name="img_fName"></param>
        /// <param name="tmpImg_fName"></param>
        /// <param name="img_SizeType"></param>
        /// <param name="img_ContentType"></param>
        /// TODO: Image must be fully qualified (System.Drawing.Image) to support VB conversion
        /// During conversion System.Drawing will be lost...must be re-added
        public static void ResizePhoto(Hashtable configValues, System.Drawing.Image origPhoto, String img_fName, String tmpImg_fName, String img_SizeType, String img_ContentType)
        {
            bool resizeMe = AppConfigBool("UseImageResize");

            if (configValues.ContainsKey("resize") && configValues["resize"].ToString() == "false")
            {
                resizeMe = false;
            }
            else if (configValues.ContainsKey("resize") && configValues["resize"].ToString() == "true")
            {
                resizeMe = true;
            }

            if (resizeMe)
            {
                int resizedWidth = AppConfigNativeInt("DefaultWidth_" + img_SizeType);
                int resizedHeight = AppConfigNativeInt("DefaultHeight_" + img_SizeType);

                int resizedQuality = AppConfigNativeInt("DefaultQuality");

                String stretchMe = AppConfig("DefaultStretch");
                String cropMe = AppConfig("DefaultCrop");
                String cropV = AppConfig("DefaultCropVertical");
                String cropH = AppConfig("DefaultCropHorizontal");
                String fillColor = AppConfig("DefaultFillColor");

                int sourceWidth = origPhoto.Width;
                int sourceHeight = origPhoto.Height;
                int sourceX = 0;
                int sourceY = 0;
                // we will extend 2 pixels on all sides to avoid the border bug
                // we could use InterpolationMode.NearestNeighbor instead of
                // InterpolationMode.HighQualityBicubic but not without sacrificing quality
                int destX = -2;
                int destY = -2;

                float nPercent = 0;
                float nPercentW = 0;
                float nPercentH = 0;

                int destWidth = 0;
                int destHeight = 0;

                if (configValues.ContainsKey("width"))
                {
                    if (CommonLogic.IsInteger(configValues["width"].ToString()))
                    {
                        resizedWidth = Int32.Parse(configValues["width"].ToString());
                    }
                }
                if (configValues.ContainsKey("height"))
                {
                    if (CommonLogic.IsInteger(configValues["height"].ToString()))
                    {
                        resizedHeight = Int32.Parse(configValues["height"].ToString());
                    }
                }
                if (configValues.ContainsKey("quality"))
                {
                    if (CommonLogic.IsInteger(configValues["quality"].ToString()))
                    {
                        resizedQuality = Int32.Parse(configValues["quality"].ToString());
                    }
                }
                if (configValues.ContainsKey("stretch"))
                {
                    stretchMe = configValues["stretch"].ToString();
                }
                if (configValues.ContainsKey("fill"))
                {
                    fillColor = configValues["fill"].ToString();
                }
                if (configValues.ContainsKey("crop"))
                {
                    cropMe = configValues["crop"].ToString();
                    if (cropMe == "true")
                    {
                        if (configValues.ContainsKey("cropv"))
                        {
                            cropV = configValues["cropv"].ToString();
                        }
                        if (configValues.ContainsKey("croph"))
                        {
                            cropH = configValues["croph"].ToString();
                        }
                    }
                }

                if (resizedWidth < 1 || resizedHeight < 1)
                {
                    resizedWidth = origPhoto.Width;
                    resizedHeight = origPhoto.Height;
                }


                if (cropMe == "true")
                {
                    String AnchorUpDown = cropV;
                    String AnchorLeftRight = cropH;
                    nPercentW = ((float)resizedWidth / (float)sourceWidth);
                    nPercentH = ((float)resizedHeight / (float)sourceHeight);

                    if (nPercentH < nPercentW)
                    {
                        nPercent = nPercentW;
                        switch (AnchorUpDown)
                        {
                            case "top":
                                destY = -2;
                                break;
                            case "bottom":
                                destY = (int)(resizedHeight - (sourceHeight * nPercent));
                                break;
                            case "center":
                            default:
                                destY = (int)((resizedHeight - (sourceHeight * nPercent)) / 2) - 2;
                                break;
                        }
                    }
                    else
                    {
                        nPercent = nPercentH;
                        switch (AnchorLeftRight.ToUpper())
                        {
                            case "left":
                                destX = 0;
                                break;
                            case "right":
                                destX = (int)(resizedWidth - (sourceWidth * nPercent));
                                break;
                            case "middle":
                            default:
                                destX = (int)((resizedWidth - (sourceWidth * nPercent)) / 2) - 2;
                                break;
                        }
                    }
                }
                else
                {
                    nPercentW = ((float)resizedWidth / (float)sourceWidth);
                    nPercentH = ((float)resizedHeight / (float)sourceHeight);

                    if (nPercentH < nPercentW)
                    {
                        nPercent = nPercentH;
                        destX = (int)((resizedWidth - (sourceWidth * nPercent)) / 2);
                    }
                    else
                    {
                        nPercent = nPercentW;
                        destY = (int)((resizedHeight - (sourceHeight * nPercent)) / 2) - 2;
                    }
                }
                // let's account for the extra pixels we left to avoid the borderbug here
                // some distortion will occur...but it should be unnoticeable
                if (stretchMe == "false" && (origPhoto.Width < resizedWidth && origPhoto.Height < resizedHeight))
                {
                    destWidth = origPhoto.Width;
                    destHeight = origPhoto.Height;
                    destX = (int)((resizedWidth / 2) - (origPhoto.Width / 2));
                    destY = (int)((resizedHeight / 2) - (origPhoto.Height / 2));
                }
                else
                {
                    destWidth = (int)Math.Ceiling(sourceWidth * nPercent) + 4;
                    destHeight = (int)Math.Ceiling(sourceHeight * nPercent) + 4;
                }

                SavePhoto(resizedWidth, resizedHeight, destHeight, destWidth, destX, destY, sourceHeight, sourceWidth, sourceX, sourceY, origPhoto, img_fName, fillColor, resizedQuality, img_ContentType);
            }
            else
            {
                origPhoto.Save(img_fName);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resizedWidth"></param>
        /// <param name="resizedHeight"></param>
        /// <param name="destHeight"></param>
        /// <param name="destWidth"></param>
        /// <param name="destX"></param>
        /// <param name="destY"></param>
        /// <param name="sourceHeight"></param>
        /// <param name="sourceWidth"></param>
        /// <param name="sourceX"></param>
        /// <param name="sourceY"></param>
        /// <param name="origPhoto">System.Drawing.Image representing the original photo to be resized</param>
        /// <param name="img_fName"></param>
        /// <param name="fillColor"></param>
        /// <param name="resizedQuality"></param>
        /// <param name="img_ContentType"></param>
        /// TODO: Image must be fully qualified (System.Drawing.Image) to support VB conversion
        /// During conversion System.Drawing will be lost...must be re-added
        private static void SavePhoto(int resizedWidth, int resizedHeight, int destHeight, int destWidth, int destX, int destY, int sourceHeight, int sourceWidth, int sourceX, int sourceY, System.Drawing.Image origPhoto, String img_fName, String fillColor, int resizedQuality, String img_ContentType)
        {
            Bitmap resizedPhoto = new Bitmap(resizedWidth, resizedHeight, PixelFormat.Format24bppRgb);
            resizedPhoto.SetResolution(origPhoto.HorizontalResolution, origPhoto.VerticalResolution);

            Graphics grPhoto = Graphics.FromImage(resizedPhoto);

            Color clearColor = new Color();
            try
            {
                clearColor = System.Drawing.ColorTranslator.FromHtml(fillColor);
            }
            catch
            {
                clearColor = Color.White;
            }
            grPhoto.Clear(clearColor);

            if (resizedQuality > 100 || resizedQuality < 1)
            {
                resizedQuality = 100;
            }

            // Encoder parameter for image quality 
            EncoderParameter qualityParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, resizedQuality);

            if (img_ContentType == "image/gif")
                img_ContentType = "image/jpeg";
            // Image codec 
            ImageCodecInfo imgCodec = GetEncoderInfo(img_ContentType);

            EncoderParameters encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = qualityParam;

            grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;

            grPhoto.DrawImage(origPhoto,
            new Rectangle(destX, destY, destWidth, destHeight),
            new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
            GraphicsUnit.Pixel);
            try
            {
                resizedPhoto.Save(img_fName, imgCodec, encoderParams);
            }
            catch
            {

                throw new Exception("You do not have proper permissions set.  You must ensure that the ASPNET and NETWORK SERVICE users have write/modify permissions on the images directory and it's sub-directories.");

            }
            finally
            {
                grPhoto.Dispose();
                origPhoto.Dispose();
            }

        }

        public static void DisposeOfTempImage(String tmpImg_fName)
        {
            try
            {
                System.IO.File.Delete(tmpImg_fName);
            }
            catch { }
        }

        public static void CreateOthersFromLarge(String EntityOrObjectName, String TempImage3, String FN, String ContentType)
        {
            String imgExt = ".jpg";

            bool largeCreates = false;
            bool largeOverwrites = false;

            bool globalCreate = (AppConfigBool("LargeCreatesOthers"));
            bool globalOverwrite = (AppConfigBool("LargeOverwritesOthers"));

            String localCreate = String.Empty;
            String localOverwrite = String.Empty;

            Hashtable configValues = SplitConfig(EntityOrObjectName, "large");

            if (configValues.ContainsKey("largecreates"))
            {
                localCreate = configValues["largecreates"].ToString();
            }
            if (configValues.ContainsKey("largeoverwrites"))
            {
                localOverwrite = configValues["largeoverwrites"].ToString();
            }

            if (localCreate == "false")
                largeCreates = false;
            else if (localCreate == "true")
                largeCreates = true;
            else
                largeCreates = globalCreate;

            if (localOverwrite == "false")
                largeOverwrites = false;
            else if (localOverwrite == "true")
                largeOverwrites = true;
            else
                largeOverwrites = globalOverwrite;

            switch (ContentType)
            {
                case "images/png":
                    imgExt = ".png";
                    break;
                case "images/jpeg":
                default:
                    imgExt = ".jpg";
                    break;
            }

            FN = FN.Replace(".", "");
            if (largeCreates)
            {
                String Image3Micro = GetImagePath(EntityOrObjectName, "micro", true) + FN + imgExt;
                String Image3Icon = GetImagePath(EntityOrObjectName, "icon", true) + FN + imgExt;
                String Image3Medium = GetImagePath(EntityOrObjectName, "medium", true) + FN + imgExt;
                if (largeOverwrites)
                {
                    // delete any smaller image files first
                    try
                    {
                        foreach (String ss in CommonLogic.SupportedImageTypes)
                        {
                            System.IO.File.Delete(GetImagePath(EntityOrObjectName, "icon", true) + FN + ss);
                            System.IO.File.Delete(GetImagePath(EntityOrObjectName, "medium", true) + FN + ss);
                        }
                    }
                    catch
                    { }
                    ResizeEntityOrObject(EntityOrObjectName, TempImage3, Image3Icon, "icon", ContentType);
                    ResizeEntityOrObject(EntityOrObjectName, TempImage3, Image3Medium, "medium", ContentType);

                    if (EntityOrObjectName.Equals("PRODUCT", StringComparison.InvariantCultureIgnoreCase))
                    {
                        foreach (String ss in CommonLogic.SupportedImageTypes)
                        {
                            System.IO.File.Delete(GetImagePath(EntityOrObjectName, "micro", true) + FN + ss);
                        }
                        ResizeEntityOrObject(EntityOrObjectName, TempImage3, Image3Micro.Replace("_.", "."), "micro", ContentType);
                    }
                }
                else
                {
                    bool iconExists = false;
                    bool mediumExists = false;
                    foreach (String ss in CommonLogic.SupportedImageTypes)
                    {
                        if (CommonLogic.FileExists(GetImagePath(EntityOrObjectName, "icon", true) + FN + ss))
                        {
                            iconExists = true;
                        }
                        if (CommonLogic.FileExists(GetImagePath(EntityOrObjectName, "medium", true) + FN + ss))
                        {
                            mediumExists = true;
                        }
                    }
                    if (!iconExists)
                    {
                        ResizeEntityOrObject(EntityOrObjectName, TempImage3, Image3Icon, "icon", ContentType);
                    }
                    if (!mediumExists)
                    {
                        ResizeEntityOrObject(EntityOrObjectName, TempImage3, Image3Medium, "medium", ContentType);
                    }
                    if (EntityOrObjectName.Equals("PRODUCT", StringComparison.InvariantCultureIgnoreCase))
                    {
                        bool microExists = false;
                        foreach (String ss in CommonLogic.SupportedImageTypes)
                        {
                            if (CommonLogic.FileExists(GetImagePath(EntityOrObjectName, "micro", true) + FN + ss))
                            {
                                microExists = true;
                            }
                        }
                        if (!microExists)
                        {
                            ResizeEntityOrObject(EntityOrObjectName, TempImage3, Image3Micro.Replace("_.", "."), "micro", ContentType);
                        }
                    }
                }
            }
        }

        public static void CreateOthersFromLarge(String EntityOrObjectName, String TempImage3, String FN, String ContentType, String img_params, Boolean UseAppConfigs, Boolean CreateOthers, Boolean OverwriteOthers)
        {
            String imgExt = ".jpg";

            bool largeCreates = false;
            bool largeOverwrites = false;

            bool globalCreate = (AppConfigBool("LargeCreatesOthers"));
            bool globalOverwrite = (AppConfigBool("LargeOverwritesOthers"));

            String localCreate = String.Empty;
            String localOverwrite = String.Empty;

            Hashtable configParams = new Hashtable();
            Hashtable configAppConfigs = new Hashtable();

            if (UseAppConfigs)
            {
                configAppConfigs = SplitConfig(EntityOrObjectName, "large");

                if (configAppConfigs.ContainsKey("largecreates"))
                {
                    localCreate = configAppConfigs["largecreates"].ToString();
                }
                if (configAppConfigs.ContainsKey("largeoverwrites"))
                {
                    localOverwrite = configAppConfigs["largeoverwrites"].ToString();
                }

                if (localCreate == "false")
                    largeCreates = false;
                else if (localCreate == "true")
                    largeCreates = true;
                else
                    largeCreates = globalCreate;

                if (localOverwrite == "false")
                    largeOverwrites = false;
                else if (localOverwrite == "true")
                    largeOverwrites = true;
                else
                    largeOverwrites = globalOverwrite;
            }

            configParams = SplitConfig(img_params);
            if (configParams.ContainsKey("largecreates"))
            {
                localCreate = configParams["largecreates"].ToString();
            }
            if (configParams.ContainsKey("largeoverwrites"))
            {
                localOverwrite = configParams["largeoverwrites"].ToString();
            }

            if (localCreate == "false")
                largeCreates = false;
            else if (localCreate == "true")
                largeCreates = true;
            else
                largeCreates = globalCreate;

            if (localOverwrite == "false")
                largeOverwrites = false;
            else if (localOverwrite == "true")
                largeOverwrites = true;
            else
                largeOverwrites = globalOverwrite;

            switch (ContentType)
            {
                case "images/png":
                    imgExt = ".png";
                    break;
                case "images/jpeg":
                default:
                    imgExt = ".jpg";
                    break;
            }

            if (largeCreates)
            {
                String Image3Icon = GetImagePath(EntityOrObjectName, "icon", true) + FN + imgExt;
                String Image3Medium = GetImagePath(EntityOrObjectName, "medium", true) + FN + imgExt;
                if (largeOverwrites)
                {
                    // delete any smaller image files first
                    try
                    {
                        foreach (String ss in CommonLogic.SupportedImageTypes)
                        {
                            System.IO.File.Delete(GetImagePath(EntityOrObjectName, "icon", true) + FN + ss);
                            System.IO.File.Delete(GetImagePath(EntityOrObjectName, "medium", true) + FN + ss);
                        }
                    }
                    catch
                    { }
                    ResizeEntityOrObject(EntityOrObjectName, TempImage3, Image3Icon, "icon", ContentType);
                    ResizeEntityOrObject(EntityOrObjectName, TempImage3, Image3Medium, "medium", ContentType);
                }
                else
                {
                    bool iconExists = false;
                    bool mediumExists = false;
                    foreach (String ss in CommonLogic.SupportedImageTypes)
                    {
                        if (CommonLogic.FileExists(GetImagePath(EntityOrObjectName, "icon", true) + FN + ss))
                        {
                            iconExists = true;
                        }
                        if (CommonLogic.FileExists(GetImagePath(EntityOrObjectName, "medium", true) + FN + ss))
                        {
                            mediumExists = true;
                        }
                    }
                    if (!iconExists)
                    {
                        ResizeEntityOrObject(EntityOrObjectName, TempImage3, Image3Icon, "icon", ContentType);
                    }
                    if (!mediumExists)
                    {
                        ResizeEntityOrObject(EntityOrObjectName, TempImage3, Image3Medium, "medium", ContentType);
                    }
                }
            }
        }

        private static ImageCodecInfo GetEncoderInfo(string resizeMimeType)
        {
            // Get image codecs for all image formats 
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

            // Find the correct image codec 
            for (int i = 0; i < codecs.Length; i++)
                if (codecs[i].MimeType == resizeMimeType)
                    return codecs[i];
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="img_fName"></param>
        /// <param name="swatchWidth"></param>
        /// <param name="swatchHeight"></param>
        /// <returns>A resized System.Drawing.Image to be used for a swatch</returns>
        /// TODO: Image must be fully qualified (System.Drawing.Image) to support VB conversion
        /// During conversion System.Drawing will be lost...must be re-added
        public static System.Drawing.Image ResizeForSwatch(String img_fName, int swatchWidth, int swatchHeight)
        {
            try
            {
                int resizedWidth = swatchWidth;
                int resizedHeight = swatchHeight;

                // TODO: Image must be fully qualified (System.Drawing.Image) to support VB conversion
                // During conversion System.Drawing will be lost...must be re-added
                System.Drawing.Image origPhoto = System.Drawing.Image.FromFile(GetImagePath("product", "medium", true) + img_fName.Substring(img_fName.LastIndexOf('/') + 1));

                int sourceWidth = origPhoto.Width;
                int sourceHeight = origPhoto.Height;
                int sourceX = 0;
                int sourceY = 0;

                int destX = 0;
                int destY = 0;

                float nPercent = 0;
                float nPercentW = ((float)resizedWidth / (float)sourceWidth);
                float nPercentH = ((float)resizedHeight / (float)sourceHeight);

                int destWidth = 0;
                int destHeight = 0;

                if (nPercentH < nPercentW)
                {
                    nPercent = nPercentW;
                    destY = (int)((resizedHeight - (sourceHeight * nPercent)) / 2);
                }
                else
                {
                    nPercent = nPercentH;
                    destX = (int)((resizedWidth - (sourceWidth * nPercent)) / 2);
                }

                destWidth = (int)Math.Ceiling(sourceWidth * nPercent);
                destHeight = (int)Math.Ceiling(sourceHeight * nPercent);

                Bitmap resizedPhoto = new Bitmap(resizedWidth, resizedHeight, PixelFormat.Format24bppRgb);

                Graphics grPhoto = Graphics.FromImage(resizedPhoto);
                grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;
                grPhoto.DrawImage(origPhoto,
                    new Rectangle(destX, destY, destWidth, destHeight),
                    new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                    GraphicsUnit.Pixel);

                grPhoto.Dispose();
                origPhoto.Dispose();
                return resizedPhoto;
            }
            catch (Exception)
            {
                return new Bitmap(1, swatchHeight);
            }
        }

        public static void MakeColorSwatch(int ProductID, int SkinID, String CustomerLocale, String variantColors, String FN)
        {
            Hashtable configValues = SplitConfig("SwatchStyleAuto");

            int indSwatchWidth = 30;
            int indSwatchHeight = 30;

            if (configValues.ContainsKey("width"))
            {
                if (CommonLogic.IsInteger(configValues["width"].ToString()))
                {
                    indSwatchWidth = Int32.Parse(configValues["width"].ToString());
                }
            }
            if (configValues.ContainsKey("height"))
            {
                if (CommonLogic.IsInteger(configValues["height"].ToString()))
                {
                    indSwatchHeight = Int32.Parse(configValues["height"].ToString());
                }
            }

            int destX = -1;
            int destY = -1;

            String Colors = "," + variantColors;
            String[] ColorsSplit = Colors.Split(',');

            StringBuilder sql = new StringBuilder(2500);
            sql.Append("update product set ");

            // TODO: Image must be fully qualified (System.Drawing.Image) to support VB conversion
            // During conversion System.Drawing will be lost...must be re-added
            System.Drawing.Image swatchAddition = null;

            Bitmap newSwatch = new Bitmap((ColorsSplit.Length * indSwatchWidth), indSwatchHeight, PixelFormat.Format24bppRgb);

            Graphics grPhoto = Graphics.FromImage(newSwatch);

            bool first = true;

            String newMap = "<map Name=\"SwatchMap\">";
            for (int i = ColorsSplit.GetLowerBound(0); i <= ColorsSplit.GetUpperBound(0); i++)
            {
                if (ColorsSplit[i].Length == 0 && !first)
                {
                    continue;
                }

                swatchAddition = ResizeForSwatch(AppLogic.LookupProductImageByNumberAndColor(ProductID, SkinID, CustomerLocale, 1, ColorsSplit[i], "medium"), indSwatchWidth, indSwatchHeight);
                grPhoto.DrawImage(swatchAddition,
            new Rectangle(destX, destY, swatchAddition.Width + 10, swatchAddition.Height + 10),
            new Rectangle(0, 0, swatchAddition.Width, swatchAddition.Height),
            GraphicsUnit.Pixel);

                newMap += "<area href=\"javascript:void(0);\" onClick=\"setcolorpic_" + ProductID.ToString() + "('" + HttpContext.Current.Server.HtmlEncode(ColorsSplit[i]) + "')\" shape=\"rect\" coords=\"" + destX + ", " + destY + ", " + (indSwatchWidth * (i + 1)) + ", " + indSwatchHeight + "\">";
                destX += swatchAddition.Width;
                swatchAddition.Dispose();
            }

            newMap += "</map>";


            newSwatch.Save(GetImagePath("Product", "swatch", true) + FN + ".jpg");


            grPhoto.Dispose();
            newSwatch.Dispose();

            sql.Append("SwatchImageMap=" + DB.SQuote(newMap) + " where productid=" + ProductID);
            DB.ExecuteSQL(sql.ToString());

        }

        public static void ResizeForMicro(String tmpImg_fName, String newImg_fName, int microWidth, int microHeight)
        {
            int resizedWidth = microWidth;
            int resizedHeight = microHeight;

            // TODO: Image must be fully qualified (System.Drawing.Image) to support VB conversion
            // During conversion System.Drawing will be lost...must be re-added
            System.Drawing.Image origPhoto = System.Drawing.Image.FromFile(CommonLogic.SafeMapPath(tmpImg_fName));

            int sourceWidth = origPhoto.Width;
            int sourceHeight = origPhoto.Height;
            int sourceX = 0;
            int sourceY = 0;
            int destX = -2;
            int destY = -2;

            float nPercent = 0;
            float nPercentW = ((float)resizedWidth / (float)sourceWidth);
            float nPercentH = ((float)resizedHeight / (float)sourceHeight);

            int destWidth = 0;
            int destHeight = 0;

            if (nPercentH < nPercentW)
            {
                nPercent = nPercentW;
                destY = (int)((resizedHeight - (sourceHeight * nPercent)) / 2) - 2;
            }
            else
            {
                nPercent = nPercentH;
                destX = (int)((resizedWidth - (sourceWidth * nPercent)) / 2) - 2;
            }

            destWidth = (int)Math.Ceiling(sourceWidth * nPercent) + 4;
            destHeight = (int)Math.Ceiling(sourceHeight * nPercent) + 4;

            Bitmap resizedPhoto = new Bitmap(resizedWidth, resizedHeight, PixelFormat.Format24bppRgb);

            Graphics grPhoto = Graphics.FromImage(resizedPhoto);
            grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;
            grPhoto.DrawImage(origPhoto,
                new Rectangle(destX, destY, destWidth, destHeight),
                new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                GraphicsUnit.Pixel);

            grPhoto.Dispose();
            origPhoto.Dispose();
            resizedPhoto.Save(newImg_fName);
            resizedPhoto.Dispose();

        }

		public static void MakeMicroPic(String fileName, String tempImage, String imageNumber)
        {
			int microWidth  = AppConfigNativeInt("DefaultWidth_micro");
			int microHeight = AppConfigNativeInt("DefaultHeight_micro");

			String microMime = ".jpg";

			String newImageName = AppLogic.GetImagePath("Product", "micro", true) + fileName + "_" + imageNumber.ToLowerInvariant() + microMime;

			ResizeForMicro(tempImage, newImageName, microWidth, microHeight);
        }

        public static void MakeMicroPic(String fileName, String tempImage, String microParams, String imageSize)
        {
            fileName = fileName.Replace("_.", ".");
            Hashtable spltConfig = new Hashtable();

			int microWidth = AppConfigNativeInt("DefaultWidth_micro");
			int microHeight = AppConfigNativeInt("DefaultHeight_micro");

			String microMime = ".jpg";
            
            spltConfig = SplitConfig(microParams);
            if (spltConfig.ContainsKey("width"))
                microWidth = Int32.Parse(spltConfig["width"].ToString());
            if (spltConfig.ContainsKey("height"))
                microHeight = Int32.Parse(spltConfig["height"].ToString());
            if (spltConfig.ContainsKey("mime"))
				microMime = spltConfig["mime"].ToString();

            if (microHeight < 1)
                microHeight = 40;
            if (microWidth < 1)
                microWidth = 40;

			switch(microMime)
            {
                case "png":
					microMime = ".png";
                    break;
                case "jpg":
                default:
					microMime = ".jpg";
                    break;
            }

            if (imageSize == "large")
            {
                //check for large creates others and large overwrites others to create the micro image if desired
                bool largeCreates = false;
                bool largeOverwrites = false;

                bool globalCreate = (AppConfigBool("LargeCreatesOthers"));
                bool globalOverwrite = (AppConfigBool("LargeOverwritesOthers"));

                String localCreate = String.Empty;
                String localOverwrite = String.Empty;

                Hashtable configValues = AppLogic.SplitConfig("Micro", "large", microParams);

                if (configValues.ContainsKey("largecreates"))
                {
                    localCreate = configValues["largecreates"].ToString();
                }
                if (configValues.ContainsKey("largeoverwrites"))
                {
                    localOverwrite = configValues["largeoverwrites"].ToString();
                }

                if (localCreate == "false")
                    largeCreates = false;
                else if (localCreate == "true")
                    largeCreates = true;
                else
                    largeCreates = globalCreate;

                if (localOverwrite == "false")
                    largeOverwrites = false;
                else if (localOverwrite == "true")
                    largeOverwrites = true;
                else
                    largeOverwrites = globalOverwrite;
				
                if (largeCreates)
                {
                    if (largeOverwrites)
                    {
                        try
                        {
                            foreach (String ss in CommonLogic.SupportedImageTypes)
                            {
                                System.IO.File.Delete(AppLogic.GetImagePath("Product", "micro", true) + fileName + ss);
                            }
                        }
                        catch { }

						String newImageName = AppLogic.GetImagePath("Product", "micro", true) + fileName + microMime;

                        ResizeForMicro(tempImage, newImageName, microWidth, microHeight);

                    }
                    else
                    {

                        bool microExists = false;
                        foreach (String ss in CommonLogic.SupportedImageTypes)
                        {
                            if (CommonLogic.FileExists(AppLogic.GetImagePath("Product", "micro", true) + fileName + ss))
                                microExists = true;
                        }

                        if (!microExists)
                        {
							String newImageName = AppLogic.GetImagePath("Product", "micro", true) + fileName + microMime;

                            ResizeForMicro(tempImage, newImageName, microWidth, microHeight);

                        }
                    }
                }
            }
            else if (AppLogic.AppConfigBool("MultiMakesMicros") && imageSize == "medium")
            {
                // lets create micro images if using the medium multi image manager
                // since the medium icons are what show on the product pages
				String newImageName = AppLogic.GetImagePath("Product", "micro", true) + fileName + microMime;

                ResizeForMicro(tempImage, newImageName, microWidth, microHeight);

            }
        }

        public static void MakeOtherMultis(String FN, String ImageNumber, String SafeColor, String TempImage2, String ContentType)
        {
            String imgExt = ".jpg";

            bool largeCreates = false;
            bool largeOverwrites = false;

            bool globalCreate = (AppConfigBool("LargeCreatesOthers"));
            bool globalOverwrite = (AppConfigBool("LargeOverwritesOthers"));

            String localCreate = String.Empty;
            String localOverwrite = String.Empty;

            Hashtable configValues = SplitConfig("Product", "large");

            if (configValues.ContainsKey("largecreates"))
            {
                localCreate = configValues["largecreates"].ToString();
            }
            if (configValues.ContainsKey("largeoverwrites"))
            {
                localOverwrite = configValues["largeoverwrites"].ToString();
            }

            if (localCreate == "false")
                largeCreates = false;
            else if (localCreate == "true")
                largeCreates = true;
            else
                largeCreates = globalCreate;

            if (localOverwrite == "false")
                largeOverwrites = false;
            else if (localOverwrite == "true")
                largeOverwrites = true;
            else
                largeOverwrites = globalOverwrite;

            switch (ContentType)
            {
                case "images/png":
                    imgExt = ".png";
                    break;
                case "images/jpeg":
                default:
                    imgExt = ".jpg";
                    break;
            }

            if (largeCreates)
            {
                Boolean makeMicros = AppLogic.AppConfigBool("MultiMakesMicros");
                List<ProductImageSize> SizesToMake = new List<ProductImageSize>();
                SizesToMake.Add(ProductImageSize.icon);
                SizesToMake.Add(ProductImageSize.medium);
                if(makeMicros)
                    SizesToMake.Add(ProductImageSize.micro);

                //String Image3Icon = ;
                //String Image3Medium = GetImagePath("Product", "medium", true) + FN + "_" + ImageNumber.ToLowerInvariant() + "_" + SafeColor + imgExt;
                foreach (ProductImageSize size in SizesToMake)
                {
                    string imagepath = GetImagePath("Product", size.ToString(), true) + FN + "_" + ImageNumber.ToLowerInvariant() + "_" + SafeColor + imgExt;
                    if (largeOverwrites)
                    {
                        // delete any smaller image files first
                        foreach (String ss in CommonLogic.SupportedImageTypes)
                        {
                            try
                            {
                                System.IO.File.Delete(GetImagePath("Product", size.ToString(), true) + FN + "_" + ImageNumber.ToLowerInvariant() + "_" + SafeColor + ss);
                            }
                            catch { }
                        }
                        ResizeEntityOrObject("Product", TempImage2, imagepath, size.ToString(), ContentType);
                    }
                    else
                    {
                        bool imageExists = false;
                        foreach (String ss in CommonLogic.SupportedImageTypes)
                        {
                            if (CommonLogic.FileExists(GetImagePath("Product", size.ToString(), true) + FN + ss))
                                imageExists = true;
                        }
                        if (!imageExists)
                            ResizeEntityOrObject("Product", TempImage2, imagepath, size.ToString(), ContentType);
                    }
                }
            }
        }

        //--------------------------------------------------------------------
        //end photo resize methods
        //--------------------------------------------------------------------

        public static String GetImagePath(String EntityOrObjectName, String Size, bool fullPath)
        {
            string pth = string.Empty;
            string pthPrefix = string.Empty;

            pth = pthPrefix + "~/images/" + EntityOrObjectName;

            if (Size.Length != 0)
            {
                pth += "/" + Size.ToLowerInvariant();
            }
            pth += "/";
            //Now have a _full_ url pth which will take into account any virtual directory mappings
            if (fullPath)
            {
                pth = CommonLogic.SafeMapPath(pth); //AppLogic.AppConfig("StoreFilesPath");
            }
            else
            {
                pth = ResolveUrl(pth);  // resolve tilde to application root
            }
            return pth;
        }

		public static string LookupImage(string EntityOrObjectName, int ID, string ImgSize, int SkinID, string LocaleSetting)
        {
			bool useWatermarks = (AppLogic.AppConfigBool("Watermark.Enabled") && !AppLogic.IsAdminSite) && (EntityOrObjectName.Equals("VARIANT", StringComparison.InvariantCultureIgnoreCase) || EntityOrObjectName.Equals("PRODUCT", StringComparison.InvariantCultureIgnoreCase));
			string fileName = string.Empty;
			
            try
            {
                // using exception block because not all "entities or objects" support this feature, 
                // and this is easiest way to code it so it works for all of them:

				String TableName = EntityOrObjectName.Replace("]", "");
                if (TableName.Equals("VARIANT", StringComparison.InvariantCultureIgnoreCase))
                {
                    TableName = "PRODUCTVARIANT";
                }

                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS(String.Format("select ImageFilenameOverride from {0} with (NOLOCK) where {1}ID={2}", "[" + TableName + "]", EntityOrObjectName, ID.ToString()), con))
                    {
                        if (rs.Read())
                        {
							fileName = DB.RSField(rs, "ImageFilenameOverride");
                        }
                    }
                }
            }
            catch { }
			if(fileName.Length == 0 && EntityOrObjectName.Equals("Product", StringComparison.InvariantCultureIgnoreCase) && AppLogic.AppConfigBool("UseSKUForProductImageName"))
            {
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS("select SKU from product   with (NOLOCK)  where productid=" + ID.ToString(), con))
                    {
                        if (rs.Read())
                        {
							string SKU = DB.RSField(rs, "SKU").Trim();
                            if (SKU.Length != 0)
                            {
								fileName = SKU;
                            }
                        }
                    }
                }
            }
			if(fileName.Length == 0)
            {
				fileName = ID.ToString();
            }

            string Image1URL = string.Empty;
			Image1URL = LocateImageURL(fileName, EntityOrObjectName, ImgSize, LocaleSetting);

            if (Image1URL.Length == 0)
            {
				Image1URL = AppLogic.NoPictureImageURL(ImgSize.Equals("icon", StringComparison.InvariantCultureIgnoreCase), SkinID, LocaleSetting);
            }
			else if(useWatermarks)
			{
				Image1URL = GetWatermarkUrl(ImgSize, Image1URL);
			}
			
            return Image1URL;
        }

		public static string LookupImage(string EntityOrObjectName, int ID, string ImageFileNameOverride, string SKU, string ImgSize, int SkinID, string LocaleSetting)
        {
			bool useWatermarks = (AppLogic.AppConfigBool("Watermark.Enabled") && !AppLogic.IsAdminSite) && (EntityOrObjectName.Equals("VARIANT", StringComparison.InvariantCultureIgnoreCase) || EntityOrObjectName.Equals("PRODUCT", StringComparison.InvariantCultureIgnoreCase));
			string fileName = ImageFileNameOverride;

			if(fileName.Length == 0 && EntityOrObjectName.Equals("PRODUCT", StringComparison.InvariantCultureIgnoreCase) && AppLogic.AppConfigBool("UseSKUForProductImageName") && SKU.Trim().Length > 0)
            {
				fileName = SKU;
            }
			if(fileName.Length == 0)
            {
				fileName = ID.ToString();
            }

			string Image1URL = string.Empty;
			Image1URL = LocateImageURL(fileName, EntityOrObjectName, ImgSize, LocaleSetting).Replace("//", "/");

			if(Image1URL.Length == 0)
			{
				if(ImgSize.Equals("icon", StringComparison.InvariantCultureIgnoreCase) || ImgSize.Equals("medium", StringComparison.InvariantCultureIgnoreCase) || ImgSize.Equals("large", StringComparison.InvariantCultureIgnoreCase))
				{
					Image1URL = AppLogic.NoPictureImageURL(ImgSize.Equals("icon", StringComparison.InvariantCultureIgnoreCase), SkinID, LocaleSetting);
				}				
			}
			else if(useWatermarks)
			{
				Image1URL = GetWatermarkUrl(ImgSize, Image1URL);
			}

            return Image1URL;
        }        

        // Color string MUST be in master store LocaleSetting!
		public static string LookupProductImageByNumberAndColor(int ProductID, int SkinID, string LocaleSetting, int ImageNumber, string Color, string ImgSize)
        {
			bool useWatermarks = AppLogic.AppConfigBool("Watermark.Enabled") && !AppLogic.IsAdminSite;
			string fileName = ProductID.ToString();
			string EntityOrObjectName = "Product";
			string SafeColor = string.Empty;

            int idx = Color.IndexOf("[");
            if (idx != -1)
            {
                SafeColor = CommonLogic.MakeSafeFilesystemName(Color.Substring(0, idx));
            }
            else
            {
                SafeColor = CommonLogic.MakeSafeFilesystemName(Color);
            }

            if (EntityOrObjectName.Equals("PRODUCT", StringComparison.InvariantCultureIgnoreCase) &&
                AppLogic.AppConfigBool("UseSKUForProductImageName"))
            {
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS("select SKU from product   with (NOLOCK)  where productid=" + ProductID.ToString(), con))
                    {
                        if (rs.Read())
                        {
							string SKU = DB.RSField(rs, "SKU").Trim();
                            if (SKU.Length != 0)
                            {
								fileName = SKU;
                            }
                        }
                    }
                }
            }
			string Image1 = AppLogic.GetImagePath(EntityOrObjectName, ImgSize, true) + fileName + "_" + ImageNumber.ToString() + "_" + SafeColor + ".jpg";
			string Image1URL = AppLogic.GetImagePath(EntityOrObjectName, ImgSize, false) + fileName + "_" + ImageNumber.ToString() + "_" + SafeColor + ".jpg";
            if (!CommonLogic.FileExists(Image1))
            {
				Image1 = AppLogic.GetImagePath(EntityOrObjectName, ImgSize, true) + fileName + "_" + ImageNumber.ToString() + "_" + SafeColor + ".gif";
				Image1URL = AppLogic.GetImagePath(EntityOrObjectName, ImgSize, false) + fileName + "_" + ImageNumber.ToString() + "_" + SafeColor + ".gif";
            }
            if (!CommonLogic.FileExists(Image1))
            {
				Image1 = AppLogic.GetImagePath(EntityOrObjectName, ImgSize, true) + fileName + "_" + ImageNumber.ToString() + "_" + SafeColor + ".png";
				Image1URL = AppLogic.GetImagePath(EntityOrObjectName, ImgSize, false) + fileName + "_" + ImageNumber.ToString() + "_" + SafeColor + ".png";
            }
            if (!CommonLogic.FileExists(Image1))
            {
				Image1URL = string.Empty;
            }

            if (Image1URL.Length == 0)
            {
				Image1URL = AppLogic.NoPictureImageURL(ImgSize.Equals("icon", StringComparison.InvariantCultureIgnoreCase), SkinID, LocaleSetting);                
            }
			else if(useWatermarks)
			{
				Image1URL = GetWatermarkUrl(ImgSize, Image1URL);
			}

            return Image1URL;
        }

		public static string LookupProductImageByNumberAndColor(int ProductID, int SkinID, string SKU, string LocaleSetting, int ImageNumber, string Color, string ImgSize)
        {
			bool useWatermarks = AppLogic.AppConfigBool("Watermark.Enabled") && !AppLogic.IsAdminSite;
			string fileName = ProductID.ToString();
			string EntityOrObjectName = "Product";
			string SafeColor = CommonLogic.MakeSafeFilesystemName(Color);

            if (EntityOrObjectName.Equals("PRODUCT", StringComparison.InvariantCultureIgnoreCase) &&
                AppLogic.AppConfigBool("UseSKUForProductImageName") &&
                SKU.Trim().Length > 0)
            {
				fileName = SKU.Trim();
            }

			fileName = fileName + "_" + ImageNumber.ToString() + "_" + SafeColor;

			string Image1URL = LocateImageURL(fileName, "PRODUCT", ImgSize, LocaleSetting);

            if (Image1URL.Length == 0)
            {
				Image1URL = AppLogic.NoPictureImageURL(ImgSize.Equals("icon", StringComparison.InvariantCultureIgnoreCase), SkinID, LocaleSetting);
            }
			else if(useWatermarks)
			{
				Image1URL = GetWatermarkUrl(ImgSize, Image1URL);
			}

            return Image1URL;
        }

		public static string LookupProductImageByNumberAndColor(int ProductID, int SkinID, string ImageFileNameOverride, string SKU, string LocaleSetting, int ImageNumber, string Color, string ImgSize)
        {
			bool useWatermarks = AppLogic.AppConfigBool("Watermark.Enabled") && !AppLogic.IsAdminSite;
			string fileName = string.Empty;

            if (ImageFileNameOverride.Trim().Length > 0 && ImageFileNameOverride.Contains("."))
            {
                fileName = ImageFileNameOverride.Substring(0, ImageFileNameOverride.IndexOf("."));
            }
            else
            {
                fileName = ImageFileNameOverride.Substring(0, (ImageFileNameOverride.Length));
            }
			string EntityOrObjectName = "Product";
			string SafeColor = CommonLogic.MakeSafeFilesystemName(Color);

            if (EntityOrObjectName.Equals("PRODUCT", StringComparison.InvariantCultureIgnoreCase) &&
                AppLogic.AppConfigBool("UseSKUForProductImageName") &&
                SKU.Trim().Length > 0)
            {
				fileName = SKU.Trim();
            }
			if(fileName.Length == 0)
            {
				fileName = ProductID.ToString();
            }

			fileName = fileName + "_" + ImageNumber.ToString() + "_" + SafeColor;
			string Image1URL = LocateImageURL(fileName, "PRODUCT", ImgSize, LocaleSetting);
            if (Image1URL.Length == 0)
            {
				Image1URL = AppLogic.NoPictureImageURL(ImgSize.Equals("icon", StringComparison.InvariantCultureIgnoreCase), SkinID, LocaleSetting);
            }
			else if(useWatermarks)
			{
				Image1URL = GetWatermarkUrl(ImgSize, Image1URL);
			}

            return Image1URL;
        }

		/// <summary>
		/// Gets the watermarked image
		/// </summary>
		/// <param name="imgSize"></param>
		/// <param name="imageURL"></param>
		/// <returns></returns>
		private static string GetWatermarkUrl(string imgSize, string imageURL)
		{
			if (imgSize.Length > 0 && imageURL.Length > 0)
			{
				if(imgSize.Equals("icon", StringComparison.InvariantCultureIgnoreCase) && !AppLogic.AppConfigBool("Watermark.Icons.Enabled"))
				{
					return imageURL;
				}
				string imagePathWithOutApplicationPath = string.Format("images/{0}", Regex.Split(imageURL, "/images/")[1]);
				imageURL = ResolveUrl(string.Format("~/watermark.axd?size={0}&imgurl={1}", imgSize, imagePathWithOutApplicationPath));
			}
			
			return imageURL;
		}

        public static void SetCookie(String cookieName, String cookieVal, TimeSpan ts)
        {
            try
            {
                HttpCookie cookie = new HttpCookie(cookieName);
                cookie.Value = HttpContext.Current.Server.UrlEncode(cookieVal);
                DateTime dt = DateTime.Now;
                cookie.Expires = dt.Add(ts);
                if (AppLogic.OnLiveServer())
                {
                    cookie.Domain = AppLogic.LiveServer();
                }
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
            catch
            { }
        }

        public static void SetSessionCookie(String cookieName, String cookieVal)
        {
            try
            {
                HttpCookie cookie = new HttpCookie(cookieName);
                cookie.Value = HttpContext.Current.Server.UrlEncode(cookieVal);
                if (AppLogic.OnLiveServer())
                {
                    cookie.Domain = AppLogic.LiveServer();
                }
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
            catch
            { }
        }

        static public String GetManufacturersBox(int SkinID, String LocaleSetting)
        {
            String CacheName = "GetManufacturersBox_" + SkinID.ToString() + "_" + LocaleSetting;
            if (AppLogic.CachingOn)
            {
                String Menu = (String)HttpContext.Current.Cache.Get(CacheName);
                if (Menu != null)
                {
                    if (CommonLogic.ApplicationBool("DumpSQL"))
                    {
                        HttpContext.Current.Response.Write("Cache Hit Found!\n");
                    }
                    return Menu;
                }
            }

            StringBuilder tmpS = new StringBuilder(10000);
            tmpS.Append("<table cellpadding=\"2\" cellspacing=\"0\" border=\"0\" style=\"border-style: solid; border-width: 0px; border-color: #" + AppLogic.AppConfig("HeaderBGColor") + "\">\n");
            tmpS.Append("<tr><td align=\"left\" valign=\"top\">\n");
            tmpS.Append("<a href=\"manufacturers.aspx\"><img src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/manufacturers.gif") + "\" border=\"0\" /></a>");
            tmpS.Append("<table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"" + AppLogic.AppConfig("BoxFrameStyle") + "\">\n");
            tmpS.Append("<tr><td align=\"left\" valign=\"top\">\n");

            tmpS.Append("<script language=\"JavaScript1.1\">\n\n");
            tmpS.Append("\n");
            tmpS.Append("//specify interval between slide (in mili seconds)\n");
            tmpS.Append("var slidespeed=3000\n");
            tmpS.Append("\n");
            tmpS.Append("//specify images\n");
            String SlideImages = String.Empty;
            String SlideLinks = String.Empty;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("Select * from manufacturer   with (NOLOCK)  where deleted=0 order by DisplayOrder,Name", con))
                {
                    while (rs.Read())
                    {
                        String MfgPic = AppLogic.LookupImage("Manufacturer", DB.RSFieldInt(rs, "ManufacturerID"), "", SkinID, LocaleSetting);

                        if (MfgPic.Length != 0)
                        {
                            if (SlideImages.Length != 0)
                            {
                                SlideImages += ",";
                            }
                            if (SlideLinks.Length != 0)
                            {
                                SlideLinks += ",";
                            }
                            SlideImages += "'" + MfgPic + "'";
                            SlideLinks += CommonLogic.SQuote(DB.RSField(rs, "Url"));
                        }
                    }
                }
            }

            tmpS.Append("var slideimages=new Array(" + SlideImages + ");");
            tmpS.Append("var slidelinks=new Array(" + SlideLinks + ");");

            tmpS.Append("\n");
            tmpS.Append("var newwindow=1 //open links in new window? 1=yes, 0=no\n");
            tmpS.Append("\n");
            tmpS.Append("var imageholder=new Array()\n");
            tmpS.Append("var ie=document.all\n");
            tmpS.Append("for (i=0;i<slideimages.length;i++)\n");
            tmpS.Append("{\n");
            tmpS.Append("imageholder[i]=new Image()\n");
            tmpS.Append("imageholder[i].src=slideimages[i]\n");
            tmpS.Append("}\n");
            tmpS.Append("\n");
            tmpS.Append("function gotoshow()\n");
            tmpS.Append("{\n");
            if (AppLogic.AppConfigBool("ManufacturersLinkToOurPage"))
            {
                tmpS.Append("window.location='manufacturers.aspx'\n");
            }
            else
            {
                tmpS.Append("if (newwindow)\n");
                tmpS.Append("window.open(slidelinks[whichlink])\n");
                tmpS.Append("else\n");
                tmpS.Append("window.location=slidelinks[whichlink]\n");
            }
            tmpS.Append("}\n");
            tmpS.Append("</script>\n");

            tmpS.Append("<center>\n");
            tmpS.Append("<a href=\"javascript:gotoshow()\"><img src=\"image1.gif\" name=\"slide\" border=\"0\" style=\"filter:blendTrans(duration=3)\" width=\"165\" height=\"100\" /></a>\n");
            tmpS.Append("</center>\n");
            tmpS.Append("\n");
            tmpS.Append("<script language=\"JavaScript1.1\">\n");
            tmpS.Append("\n");
            tmpS.Append("var whichlink=0\n");
            tmpS.Append("var whichimage=0\n");
            tmpS.Append("var blenddelay=(ie)? document.images.slide.filters[0].duration*1000 : 0\n");
            tmpS.Append("function slideit()\n");
            tmpS.Append("{\n");
            tmpS.Append("if (!document.images) return\n");
            tmpS.Append("if (ie) document.images.slide.filters[0].apply()\n");
            tmpS.Append("document.images.slide.src=imageholder[whichimage].src\n");
            tmpS.Append("if (ie) document.images.slide.filters[0].play()\n");
            tmpS.Append("whichlink=whichimage\n");
            tmpS.Append("whichimage=(whichimage<slideimages.length-1)? whichimage+1 : 0\n");
            tmpS.Append("setTimeout(\"slideit()\",slidespeed+blenddelay)\n");
            tmpS.Append("}\n");
            tmpS.Append("slideit()\n");
            tmpS.Append("\n");
            tmpS.Append("</script>\n");

            tmpS.Append("</td></tr>\n");
            tmpS.Append("</table>\n");
            tmpS.Append("</td></tr>\n");
            tmpS.Append("</table>\n");
            if (CachingOn)
            {
                HttpContext.Current.Cache.Insert(CacheName, tmpS.ToString(), null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }

            return tmpS.ToString();
        }

        public static String MakeProperObjectName(String pname, String vname, String LocaleSetting)
        {
            vname = XmlCommon.GetLocaleEntry(vname, LocaleSetting, true);
            pname = XmlCommon.GetLocaleEntry(pname, LocaleSetting, true);
            if (vname.Trim().Length == 0 || pname == vname)
            {
                return pname.Trim();
            }
            else
            {
                return String.Format("{0}-{1}", pname.Trim(), vname.Trim());
            }
        }

        public static String MakeProperProductName(int ProductID, int VariantID, String LocaleSetting)
        {
            return MakeProperObjectName(AppLogic.GetProductName(ProductID, LocaleSetting), AppLogic.GetVariantName(VariantID, LocaleSetting), LocaleSetting);
        }

        public static String MakeProperProductSKU(String pSKU, String vSKU, String colorMod, String sizeMod)
        {
            return pSKU + vSKU + colorMod + sizeMod;
        }

        static public void SendMail(String subject, String body, bool useHTML)
        {
            SendMail(subject, body, useHTML, AppLogic.AppConfig("MailMe_FromAddress"), AppLogic.AppConfig("MailMe_FromName"), AppLogic.AppConfig("MailMe_ToAddress"), AppLogic.AppConfig("MailMe_ToName"), String.Empty, AppLogic.MailServer());
        }

        static public void SendMail(String subject, String body, bool useHTML, String fromaddress, String fromname, String toaddress, String toname, String bccaddresses, String server)
        {
            SendMail(subject, body, useHTML, fromaddress, fromname, toaddress, toname, bccaddresses, String.Empty, server);
        }
        static public void SendOutOfStockMail(String subject, String body, bool useHTML, String fromaddress, String fromname, String toaddress, String toname, String bccaddresses, String server)
        {
            SendOutOfStockMail(subject, body, useHTML, fromaddress, fromname, toaddress, toname, bccaddresses, String.Empty, server);
        }
        static public void SendOutOfStockMail(String subject, String body, bool useHTML, String fromaddress, String fromname, String toaddress, String toname, String bccaddresses, String ReplyTo, String server)
        {
            if (false == server.Equals(AppLogic.ro_TBD, StringComparison.InvariantCultureIgnoreCase) &&
                false == server.Equals("MAIL.YOURDOMAIN.COM", StringComparison.InvariantCultureIgnoreCase) &&
                server.Length != 0)
            {
                System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage(new MailAddress(fromaddress, fromname), new MailAddress(toaddress, toname));
                if (ReplyTo.Length != 0)
                {
                    msg.ReplyTo = new MailAddress(ReplyTo);
                }
                msg.Subject = subject;
                msg.Body = body;
                msg.IsBodyHtml = useHTML;
                if (bccaddresses.Length != 0)
                {
                    MailAddressCollection mc = new MailAddressCollection();
                    foreach (String s in bccaddresses.Split(new char[] { ',', ';' }))
                    {
                        if (s.Trim().Length > 0)
                        {
                            msg.Bcc.Add(new MailAddress(s.Trim()));
                        }
                    }
                }
                SmtpClient client = new SmtpClient(server);
                if (AppLogic.AppConfig("MailMe_User").Length != 0)
                {
                    System.Net.NetworkCredential SMTPUserInfo = new System.Net.NetworkCredential(AppLogic.AppConfig("MailMe_User"), AppLogic.AppConfig("MailMe_Pwd"));
                    client.UseDefaultCredentials = false;
                    client.Credentials = SMTPUserInfo;
                }
                else
                {
                    client.Credentials = CredentialCache.DefaultNetworkCredentials;
                }

                //Added to support SSL and non-standard port configurations
                client.EnableSsl = AppLogic.AppConfigBool("MailMe_UseSSL");
                client.Port = AppLogic.AppConfigNativeInt("MailMe_Port");
                //End SSL Support

                try
                {
                    client.Send(msg);
                }
                catch (Exception ex)
                {

                    SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                    ex.Message + ((ex.InnerException != null && string.IsNullOrEmpty(ex.InnerException.Message)) ? " :: " + ex.InnerException.Message : ""),
                    MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
                    if (!AppLogic.IsAdminSite)
                    {
                        throw new ArgumentException("Mail Error occurred - " + CommonLogic.GetExceptionDetail(ex, ""));
                    }
                }
                msg.Dispose();
            }
            else
            {
                if (!AppLogic.IsAdminSite)
                {
                    throw new ArgumentException("Invalid Mail Server: " + server + "" + AppLogic.GetString("admin.splash.aspx.security.MailServer", Customer.Current.SkinID, Customer.Current.LocaleSetting));
                }
            }
        }

        // mask errors on store site, better to have a lost receipt than crash the site
        // on admin site, throw exceptions
        static public void SendMail(String subject, String body, bool useHTML, String fromaddress, String fromname, String toaddress, String toname, String bccaddresses, String ReplyTo, String server)
        {
            if (false == server.Equals(AppLogic.ro_TBD, StringComparison.InvariantCultureIgnoreCase) &&
                false == server.Equals("MAIL.YOURDOMAIN.COM", StringComparison.InvariantCultureIgnoreCase) &&
                server.Length != 0)
            {
                System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage(new MailAddress(fromaddress, fromname), new MailAddress(toaddress, toname));
                if (ReplyTo.Length != 0)
                {
                    msg.ReplyTo = new MailAddress(ReplyTo);
                }
                msg.Subject = subject;
                msg.Body = body;
                msg.IsBodyHtml = useHTML;
                if (bccaddresses.Length != 0)
                {
                    MailAddressCollection mc = new MailAddressCollection();
                    foreach (String s in bccaddresses.Split(new char[] { ',', ';' }))
                    {
                        if (s.Trim().Length > 0)
                        {
                            msg.Bcc.Add(new MailAddress(s.Trim()));
                        }
                    }
                }
                SmtpClient client = new SmtpClient(server);
                if (AppLogic.AppConfig("MailMe_User").Length != 0)
                {
                    System.Net.NetworkCredential SMTPUserInfo = new System.Net.NetworkCredential(AppLogic.AppConfig("MailMe_User"), AppLogic.AppConfig("MailMe_Pwd"));
                    client.UseDefaultCredentials = false;
                    client.Credentials = SMTPUserInfo;
                }
                else
                {
                    client.Credentials = CredentialCache.DefaultNetworkCredentials;
                }

                //Added to support SSL and non-standard port configurations
                client.EnableSsl = AppLogic.AppConfigBool("MailMe_UseSSL");
                client.Port = AppLogic.AppConfigNativeInt("MailMe_Port");
                //End SSL Support

                try
                {
                    client.Send(msg);
                }
                catch (Exception ex)
                {
                    if (AppLogic.IsAdminSite)
                    {
                        throw new ArgumentException("Mail Error occurred - " + CommonLogic.GetExceptionDetail(ex, ""));
                    }
                }
                msg.Dispose();
            }
            else
            {
                if (AppLogic.IsAdminSite)
                {
                    throw new ArgumentException("Invalid Mail Server: " + server + "" + AppLogic.GetString("admin.splash.aspx.security.MailServer", Customer.Current.SkinID, Customer.Current.LocaleSetting));
                }
            }
        }

        static public bool HasBadWords(String s)
        {
            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select upper(Word) as BadWord from BadWord  with (NOLOCK)  where LocaleSetting=" + DB.SQuote(Thread.CurrentThread.CurrentUICulture.Name), con))
                {
                    while (rs.Read())
                    {
                        if (s.IndexOf(DB.RSField(rs, "BadWord"), StringComparison.InvariantCultureIgnoreCase) != -1)
                        {
                            rs.Close();
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        static public String GetCountryName(String CountryTwoLetterISOCode)
        {
            String tmp = "United Countries"; // default to US just in case

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select * from country   with (NOLOCK)  where upper(TwoLetterISOCode)=" + DB.SQuote(CountryTwoLetterISOCode.ToUpperInvariant()), con))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSField(rs, "name");
                    }
                }
            }

            return tmp;
        }

        /// <summary>
        /// //Returns true if the Customer has previously purchased this product.
        /// </summary>
        public static bool Owns(int ProductID, int CustomerID)
        {
            int nCount = 0;

            //VIP users have total access
            if (HttpContext.Current.User.IsInRole("VIP"))
            {
                return true;
            }

            if (ProductID != 0)
            {
                nCount = DB.GetSqlN(String.Format("select top 1 os.productid as N from dbo.orders_ShoppingCart os  with (NOLOCK)  join dbo.orders o  with (NOLOCK)  on o.ordernumber = os.ordernumber  where o.CustomerID={0} and os.ProductID={1} and o.TransactionState={2}", CustomerID, ProductID, DB.SQuote(AppLogic.ro_TXStateCaptured)));
                if (nCount != 0)
                {
                    return true;
                }
            }
            return false;
        }

        static public String GetCategoryBox(int categoryID, bool subCatsOnly, int showNum, bool showPics, String teaser, int _SkinID, String LocaleSetting)
        {
            String CacheName = "CategoryBox" + categoryID.ToString();
            bool CachingOn = AppLogic.AppConfigBool("CacheMenus");
            if (CachingOn)
            {
                String ProductsMenu = (String)HttpContext.Current.Cache.Get(CacheName);
                if (ProductsMenu != null)
                {
                    if (CommonLogic.ApplicationBool("DumpSQL"))
                    {
                        HttpContext.Current.Response.Write("Cache Hit Found!\n");
                    }
                    return ProductsMenu;
                }
            }

            StringBuilder tmpS = new StringBuilder(10000);
            tmpS.Append("<table cellpadding=\"2\" cellspacing=\"0\" border=\"0\" style=\"border-style: solid; border-width: 0px; border-color: #" + AppLogic.AppConfig("HeaderBGColor") + "\">\n");
            tmpS.Append("<tr><td align=\"left\" valign=\"top\">\n");
            tmpS.Append("<a href=\"" + SE.MakeCategoryLink(categoryID, String.Empty) + "\"><img src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + _SkinID.ToString() + "/images/kits.gif") + "\" border=\"0\"></a>");
            tmpS.Append("<table width=\"100%\" cellpadding=\"4\" cellspacing=\"0\" border=\"0\" style=\"" + AppLogic.AppConfig("BoxFrameStyle") + "\">\n");
            tmpS.Append("<tr><td align=\"" + CommonLogic.IIF(showPics, "center", "left") + "\" valign=\"top\">\n");

            tmpS.Append("<p align=\"" + CommonLogic.IIF(showPics, "center", "left") + "\"><b>" + teaser + "</b></p>\n");

            if (subCatsOnly)
            {
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS("select * from category   with (NOLOCK)  where deleted=0 and published<>0 and parentcategoryid=" + categoryID.ToString() + " order by displayorder,name", con))
                    {
                        int i = 1;
                        while (rs.Read())
                        {
                            if (i > showNum)
                            {
                                tmpS.Append("<img height=\"8\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + _SkinID.ToString() + "/images/redarrow.gif") + "\">&nbsp;<a href=\"" + SE.MakeCategoryLink(categoryID, DB.RSField(rs, "SEName")) + "\">" + GetString("news.MoreNews.1", Customer.Current.SkinID, Customer.Current.LocaleSetting) + "</a>");
                                break;
                            }
                            tmpS.Append("<img height=\"8\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + _SkinID.ToString() + "/images/redarrow.gif") + "\">&nbsp;<a href=\"" + SE.MakeCategoryLink(DB.RSFieldInt(rs, "CategoryID"), DB.RSField(rs, "SEName")) + "\">");
                            String ImgUrl = AppLogic.LookupImage("Category", DB.RSFieldInt(rs, "CategoryID"), "icon", _SkinID, LocaleSetting);
                            if (ImgUrl.Length != 0)
                            {
                                System.Drawing.Size size = CommonLogic.GetImagePixelSize(ImgUrl);
                                if (showPics)
                                {
                                    tmpS.Append("<img src=\"" + ImgUrl + "\" width=\"" + CommonLogic.IIF(size.Width >= 155, 155, size.Width).ToString() + "\" border=\"0\">");
                                }
                            }
                            tmpS.Append(DB.RSField(rs, "Name") + "</a>");
                            tmpS.Append("");
                            if (showPics)
                            {
                                tmpS.Append("");
                            }
                            i++;
                        }
                    }
                }
            }
            else
            {

                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS("select p.*,DO.displayorder from product P   with (NOLOCK)  left outer join ProductCategory DO   with (NOLOCK)  on p.productid=do.productid and do.categoryid=" + categoryID.ToString() + " where p.deleted=0 and p.published=1 and p.productid in (select distinct productid from productcategory   with (NOLOCK)  where categoryid=" + categoryID.ToString() + ") order by do.displayorder", con))
                    {
                        int i = 1;
                        while (rs.Read())
                        {
                            if (i > showNum)
                            {
                                tmpS.Append("<img height=\"8\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + _SkinID.ToString() + "/images/redarrow.gif") + "\">&nbsp;<a href=\"" + SE.MakeCategoryLink(categoryID, "") + "\">more...</a>");
                                break;
                            }
                            tmpS.Append("<img height=\"8\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + _SkinID.ToString() + "/images/redarrow.gif") + "\">&nbsp;<a href=\"" + SE.MakeProductLink(DB.RSFieldInt(rs, "ProductID"), DB.RSField(rs, "SEName")) + "\">");
                            String ImgUrl = String.Empty;
							ImgUrl = AppLogic.LookupImage("Product", DB.RSFieldInt(rs, "ProductID"), "icon", _SkinID, LocaleSetting);

                            if (ImgUrl.Length != 0)
                            {
                                System.Drawing.Size size = CommonLogic.GetImagePixelSize(ImgUrl);
                                if (showPics)
                                {
                                    tmpS.Append("<img src=\"" + ImgUrl + "\" width=\"" + CommonLogic.IIF(size.Width >= 155, 155, size.Width).ToString() + "\" border=\"0\">");
                                }
                            }
                            tmpS.Append(DB.RSField(rs, "Name") + "</a>");
                            tmpS.Append("");
                            if (showPics)
                            {
                                tmpS.Append("");
                            }
                            i++;
                        }
                    }
                }
            }

            tmpS.Append("</td></tr>\n");
            tmpS.Append("</table>\n");
            tmpS.Append("</td></tr>\n");
            tmpS.Append("</table>\n");
            if (CachingOn)
            {
                HttpContext.Current.Cache.Insert(CacheName, tmpS.ToString(), null, System.DateTime.Now.AddHours(1), TimeSpan.Zero);
            }
            return tmpS.ToString();
        }

        // gets roles of current httpcontext user, prior set by SetRoles
        public static String GetRoles()
        {
            String tmpS = String.Empty;
            try
            {
                AspDotNetStorefrontPrincipal p = (AspDotNetStorefrontPrincipal)HttpContext.Current.User;
                if (null != p)
                {
                    tmpS = p.Roles;
                }
            }
            catch { }
            return tmpS;
        }


        public static int GetMicroPayProductID()
        {
            int result = 0;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select ProductID from Product   with (NOLOCK)  where deleted=0 and SKU='MICROPAY'", con))
                {
                    if (rs.Read())
                    {
                        result = DB.RSFieldInt(rs, "ProductID");
                    }
                }
            }

            return result;
        }

        public static int GetAdHocProductID()
        {
            int result = 0;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select ProductID from Product   with (NOLOCK)  where deleted=0 and SKU='ADHOCCHARGE'", con))
                {
                    if (rs.Read())
                    {
                        result = DB.RSFieldInt(rs, "ProductID");
                    }
                }
            }

            return result;
        }


        public static decimal GetMicroPayBalance(int CustomerID)
        {
            decimal result = System.Decimal.Zero;

            if (CustomerID != 0)
            {
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS(String.Format("select MicroPayBalance from Customer   with (NOLOCK)  where CustomerID={0}", CustomerID), con))
                    {
                        if (rs.Read())
                        {
                            result = DB.RSFieldDecimal(rs, "MicroPayBalance");
                        }
                    }
                }
            }
            return result;
        }

        static String readonly_WCLocale = String.Empty; // for speed

        static public string GetString_OLD(string key, int SkinID, String LocaleSetting)
        {
            return "";
        }

        /// <summary>
        /// Returns the string resource for the specified key using the default web.config locale and default store skin ID
        /// This should ONLY be used in instances where multi-lingual support is NEVER needed
        /// </summary>
        /// <param name="key">The name (key) of the string to return</param>
        /// <returns></returns>
        static public string GetStringForDefaultLocale(string key)
        {
            return GetString(key, DefaultSkinID(), readonly_WCLocale);
        }

        /// <summary>
        /// Determines the runtime StringResource for 'key'
        /// </summary>
        /// <param name="key">Key of the string resource</param>
        /// <param name="LocaleSetting">The language setting from which do derive the string</param>
        /// <returns>The StringResource from the appropriate language for the key specified</returns>
        static public string GetString(string key, string LocaleSetting)
        {
            // undocumented diagnostic mode:
            if (AppLogic.AppConfigBool("ShowStringResourceKeys"))
            {
                return key;
            }
            if (readonly_WCLocale.Length == 0)
            {
                readonly_WCLocale = Localization.GetDefaultLocale();
            }

            var stringsLoaded = StringResourceManager.HasAnyStrings();
            if (!stringsLoaded)
            {
                // try to reaload
                StringResourceManager.LoadAllStrings(true);
                //LoadStringResourcesFromDB(true);
            }

            if (!stringsLoaded)
            {
                if (AppLogic.MailServer().Length != 0 &&
                    false == AppLogic.MailServer().Equals(AppLogic.ro_TBD, StringComparison.InvariantCultureIgnoreCase))
                {
                    try
                    {
                        String Explanation = "This message means that your site is flushing application memory for some (unknown) reason. If you get this e-mail more then very rarely, it could be a big performance impact, and you should check with your hosting provider about this issue. Their server may be running low of RAM or something is causing their application asp.net memory caches to get flushed. This could cause your site to send up to 1000 database queries to the store on every single page load. If our store sends you an e-mail (which is VERY rare), it's not for a minor issue. First, please check with your hosting provider.";
                        AppLogic.SendMail(AppLogic.AppConfig("StoreName") + " String Table Empty Incident", "String Tables Empty, Reloaded at " + Localization.ToNativeDateTimeString(System.DateTime.Now) + "." + Explanation, false, AppLogic.AppConfig("MailMe_FromAddress"), AppLogic.AppConfig("MailMe_FromName"), AppLogic.AppConfig("MailMe_ToAddress"), AppLogic.AppConfig("MailMe_ToName"), String.Empty, AppLogic.MailServer());
                    }
                    catch { }
                }
                return key; // they don't even have this locale, return the key as a placeholder
            }

            var currentStoreId = AppLogic.StoreID();

            // first try to look at the specified locale
            StringResource str = StringResourceManager.GetStringResource(currentStoreId, LocaleSetting, key);
            if (str == null)
            {
                // if not found, try to look using the Web.config locale
                str = StringResourceManager.GetStringResource(currentStoreId, readonly_WCLocale, key);
            }

            // if still not found, let's fall back to the default store
            // if we're not on the default
            if (str == null)
            {
                var defaultStoreId = AppLogic.DefaultStoreID();
                str = StringResourceManager.GetStringResource(defaultStoreId, LocaleSetting, key);

                // already using the default store but string still not found
                // let's fall back again this time to the web.config locale
                if (str != null)
                {
                    str = StringResourceManager.GetStringResource(defaultStoreId, readonly_WCLocale, key);
                }
            }

            // not found
            if (str == null)
            {
                return key;
            }
            else
            {
                return str.ConfigValue;
            }
        }

        /// <summary>
        /// Returns the runtime StringResource for 'key'
        /// </summary>
        /// <param name="key">Key of the string resource</param>
        /// <param name="SkinID">ID of the skin for which to return</param>
        /// <param name="LocaleSetting">The language setting from which do derive the string</param>
        /// <returns></returns>
        static public string GetString(string key, int SkinID, String LocaleSetting)
        {
            // undocumented diagnostic mode:
            if (AppLogic.AppConfigBool("ShowStringResourceKeys"))
            {
                return key;
            }
            if (readonly_WCLocale.Length == 0)
            {
                readonly_WCLocale = Localization.GetDefaultLocale();
            }

            var stringsLoaded = StringResourceManager.HasAnyStrings();
            if (!stringsLoaded)
            {
                // try to reaload
                StringResourceManager.LoadAllStrings(true);
                //LoadStringResourcesFromDB(true);
            }

            if (!stringsLoaded)
            {
                if (AppLogic.MailServer().Length != 0 &&
                    false == AppLogic.MailServer().Equals(AppLogic.ro_TBD, StringComparison.InvariantCultureIgnoreCase))
                {
                    try
                    {
                        String Explanation = "This message means that your site is flushing application memory for some (unknown) reason. If you get this e-mail more then very rarely, it could be a big performance impact, and you should check with your hosting provider about this issue. Their server may be running low of RAM or something is causing their application asp.net memory caches to get flushed. This could cause your site to send up to 1000 database queries to the store on every single page load. If our store sends you an e-mail (which is VERY rare), it's not for a minor issue. First, please check with your hosting provider.";
                        AppLogic.SendMail(AppLogic.AppConfig("StoreName") + " String Table Empty Incident", "String Tables Empty, Reloaded at " + Localization.ToNativeDateTimeString(System.DateTime.Now) + "." + Explanation, false, AppLogic.AppConfig("MailMe_FromAddress"), AppLogic.AppConfig("MailMe_FromName"), AppLogic.AppConfig("MailMe_ToAddress"), AppLogic.AppConfig("MailMe_ToName"), String.Empty, AppLogic.MailServer());
                    }
                    catch { }
                }
                return key; // they don't even have this locale, return the key as a placeholder
            }

            var currentStoreId = AppLogic.StoreID();

            // first try to look at the specified locale
            StringResource str = StringResourceManager.GetStringResource(currentStoreId, LocaleSetting, key);
            if (str == null)
            {
                // if not found, try to look using the Web.config locale
                str = StringResourceManager.GetStringResource(currentStoreId, readonly_WCLocale, key);
            }

            // if still not found, let's fall back to the default store
            // if we're not on the default
            if (str == null)
            {
                var defaultStoreId = AppLogic.DefaultStoreID();
                str = StringResourceManager.GetStringResource(defaultStoreId, LocaleSetting, key);

                // already using the default store but string still not found
                // let's fall back again this time to the web.config locale
                if (str != null)
                {
                    str = StringResourceManager.GetStringResource(defaultStoreId, readonly_WCLocale, key);
                }
            }

            // not found
            if (str == null)
            {
                return key;
            }
            else
            {
                if (!AppLogic.IsAdminSite && AppLogic.AppConfigBool("StringResources.ReturnUnderscores"))
                {
                    return "".PadRight(str.ConfigValue.Length, '_');
                }
                return str.ConfigValue;
            }
        }


        // ----------------------------------------------------------------
        //
        // APPCONFIG SUPPORT ROUTINES
        //
        // ----------------------------------------------------------------
        public static void SetGlobalConfig(String Name, String ConfigValue)
        {
            AspDotNetStorefrontCore.GlobalConfig c = AspDotNetStorefrontCore.GlobalConfig.getGlobalConfig(Name);
            if (c == null)
                return;
            c.ConfigValue = ConfigValue;
            c.Save();
        }

        public static void SetAppConfig(String Name, String ConfigValue)
        {
            SetAppConfig(Name, ConfigValue, AppLogic.StoreID());
        }

        public static void SetAppConfig(String Name, String ConfigValue, int StoreID)
        {
            Name = Name.Trim();
            var config = GetAppConfig(0, Name);
            if (config != null)
            {
                SqlParameter sp1 = new SqlParameter("@ConfigValue", SqlDbType.NVarChar);
                sp1.Value = ConfigValue;
                SqlParameter[] spa = { sp1 };
                config.Update(spa);
                config.ConfigValue = ConfigValue;
            }
            else
            {
                AddAppConfig(Name, "", ConfigValue, "Custom", false, StoreID);
            }
        }

        public static AppConfig AddAppConfig(string Name, string Description, string ConfigValue, string GroupName, bool SuperOnly)
        {
            return AddAppConfig(Name, Description, ConfigValue, null, null, GroupName, SuperOnly, AppLogic.StoreID());
        }

        public static AppConfig AddAppConfig(string Name, string Description, string ConfigValue, string GroupName, bool SuperOnly, int storeId)
        {
			return AddAppConfig(Name, Description, ConfigValue, null, null, GroupName, SuperOnly, storeId);
        }

		public static AppConfig AddAppConfig(string name, string description, string configValue, string valueType, string allowableValues, string groupName, bool superOnly)
		{
			return AddAppConfig(name, description, configValue, valueType, allowableValues, groupName, superOnly, AppLogic.StoreID());
		}

		public static AppConfig AddAppConfig(string name, string description, string configValue, string valueType, string allowableValues, string groupName, bool superOnly, int storeId)
		{
			var configs = AppConfigManager.GetAppConfigCollection(storeId, true);
			return configs.Add(name, description, configValue, valueType, allowableValues, groupName, superOnly, storeId);
		}

        public static bool HasAppConfigsLoaded()
        {
            // check if the appconfigs has already been loaded for this site
            // regardless of the current store request
            return AppConfigManager.HasConfigsLoaded();
        }

        public static AppConfig GetAppConfig(int id)
        {
            return GetAppConfig(AppLogic.StoreID(), id);
        }

        public static AppConfig GetAppConfig(Int32 storeId, int id)
        {
            return AppConfigManager.GetAppConfig(storeId, id);
        }

        public static AppConfig GetAppConfig(String paramName)
        {
            return AppConfigManager.GetAppConfig(AppLogic.StoreID(), paramName);
        }

        public static AppConfig GetAppConfig(Int32 storeId, String paramName)
        {
            return AppConfigManager.GetAppConfig(storeId, paramName);
        }

        public static bool AppConfigExists(String paramName)
        {
            return AppConfigManager.AppConfigExists(AppLogic.StoreID(), paramName);
        }

		public static bool AppConfigExists(String paramName, int storeId)
		{
			return AppConfigManager.AppConfigExists(storeId, paramName);
		}
		
		public static string GlobalConfig(string paramName)
        {
            var cfg = GlobalConfigTable[paramName];
            if (cfg != null)
            {
                return cfg.ConfigValue;
            }

            // not found, default
            return string.Empty;
        }

        public static bool GlobalConfigBool(string paramName)
        {
            var cfg = GlobalConfigTable[paramName];
            if (cfg != null)
            {
                return Localization.ParseBoolean(cfg.ConfigValue);
            }

            // not found, default to false
            return false;
        }

        /// <summary>
        /// Gets the AppConfig value
        /// </summary>
        /// <param name="paramName"></param>
        /// <returns></returns>
        public static String AppConfig(String paramName)
        {
            AppConfig ac = GetAppConfigRouted(paramName);
            if (ac != null)
            {
                return ac.ConfigValue;
            }
            return "";
        }

        public static AppConfig GetAppConfigRouted(String paramName)
        {
            return GetAppConfigRouted(paramName, AppLogic.StoreID());
        }

        public static AppConfig GetAppConfigRouted(String paramName, int storeId)
        {
            // try the storeid for the current request
            var config = AppConfigManager.GetAppConfig(storeId, paramName);
            if (config != null)
                return config;

            //fallback on default app config specified by storeid 0
            config = AppConfigManager.GetAppConfig(0, paramName);
            if (config != null)
                return config;

            // fallback to the default store
            storeId = AppLogic.DefaultStoreID();
            config = AppConfigManager.GetAppConfig(storeId, paramName);
            if (config != null)
                return config;

            return null;
        }


        ///// <summary>
        ///// Used with StoreID parameter. Load appconfig depending on StoreID parameter.
        ///// </summary>
        ///// <param name="storeID">Store ID to load an appconfig value</param>
        ///// <param name="paramName">Appconfig name</param>
        ///// <param name="cascadeToDefault">If the app config for this store is not found the default will be returned.</param>
        ///// <returns>Appconfig value</returns>
        public static String AppConfig(String paramName, int storeID, bool cascadeToDefault)
        {
            return AppConfigManager.AppConfig(storeID, paramName, cascadeToDefault);
        }

        /// <summary>
        /// Used with StoreID parameter. Load appconfig depending on StoreID parameter.
        /// </summary>
        /// <param name="storeID">Store ID to load an appconfig value</param>
        /// <param name="paramName">Appconfig name</param>
        /// <returns>Appconfig value</returns>
        //public static String GetAppConfig(int storeID, String paramName)
        //{
        //    return AppConfigManager.AppConfig(storeID, paramName);
        //}

        //public static bool AppConfigBool(int StoreId, String paramName)
        //{ return AppConfigBool(StoreId, paramName, false); }
        //public static bool AppConfigBool(int StoreId, String paramName, bool cascadeToDefault)
        public static bool AppConfigBool(String paramName, int StoreId, bool CascadeToDefault)
        {
            String tmp = AppConfig(paramName, StoreId, CascadeToDefault);
            if (tmp.Equals("TRUE", StringComparison.InvariantCultureIgnoreCase) ||
                tmp.Equals("YES", StringComparison.InvariantCultureIgnoreCase) ||
                tmp.Equals("1", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool AppConfigBool(String paramName)
        {
            String tmp = AppConfig(paramName);
            if (tmp.Equals("TRUE", StringComparison.InvariantCultureIgnoreCase) ||
                tmp.Equals("YES", StringComparison.InvariantCultureIgnoreCase) ||
                tmp.Equals("1", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static int AppConfigUSInt(String paramName)
        {
            String tmpS = AppConfig(paramName);
            return Localization.ParseUSInt(tmpS);
        }

        public static long AppConfigUSLong(String paramName)
        {
            String tmpS = AppConfig(paramName);
            return Localization.ParseUSLong(tmpS);
        }

        public static Single AppConfigUSSingle(String paramName)
        {
            String tmpS = AppConfig(paramName);
            return Localization.ParseUSSingle(tmpS);
        }

        public static Double AppConfigUSDouble(String paramName)
        {
            String tmpS = AppConfig(paramName);
            return Localization.ParseUSDouble(tmpS);
        }

        public static Decimal AppConfigUSDecimal(String paramName)
        {
            String tmpS = AppConfig(paramName);
            return Localization.ParseUSDecimal(tmpS);
        }

        public static DateTime AppConfigUSDateTime(String paramName)
        {
            return Localization.ParseUSDateTime(AppConfig(paramName));
        }

        public static int AppConfigNativeInt(String paramName)
        {
            String tmpS = AppConfig(paramName);
            return Localization.ParseNativeInt(tmpS);
        }

        public static long AppConfigNativeLong(String paramName)
        {
            String tmpS = AppConfig(paramName);
            return Localization.ParseNativeLong(tmpS);
        }

        public static Single AppConfigNativeSingle(String paramName)
        {
            String tmpS = AppConfig(paramName);
            return Localization.ParseNativeSingle(tmpS);
        }

        public static Double AppConfigNativeDouble(String paramName)
        {
            String tmpS = AppConfig(paramName);
            return Localization.ParseNativeDouble(tmpS);
        }

        public static Decimal AppConfigNativeDecimal(String paramName)
        {
            String tmpS = AppConfig(paramName);
            return Localization.ParseNativeDecimal(tmpS);
        }

        public static DateTime AppConfigNativeDateTime(String paramName)
        {
            return Localization.ParseNativeDateTime(AppConfig(paramName));
        }

        //-----------------------------------------
        //EventHandler Support Routine
        //
        //------------------------------------------
        public static AspdnsfEventHandler eventHandler(String eventName)
        {
            if (EventHandlerTable[eventName] != null)
            {
                return EventHandlerTable[eventName];
            }
            else
            {   //Returning Dummy object with Active as False to avoid null returns
                AspdnsfEventHandler dummyHandler = new AspdnsfEventHandler(0, "", "", "", false, false);
                return dummyHandler;
            }
        }

        public static BadWord badWord(String word)
        {
            if (BadWordTable[word] != null)
            {
                return BadWordTable[word];
            }
            else
            {   //Returning Dummy object with Active as False to avoid null returns
                BadWord dummy = new BadWord(0, "", "", DateTime.Now);
                return dummy;
            }
        }

		public static void GetButtonDisable(Button btn, string validationGroup)
		{
			StringBuilder sbValid = new StringBuilder(1024);
			sbValid.Append("if (typeof(Page_ClientValidate) == 'function') { ");
			sbValid.Append("if (Page_ClientValidate(");
			if(validationGroup != null)
				sbValid.Append("'" + validationGroup + "'");
			sbValid.Append(") == false) { return false; }} ");
			sbValid.Append("this.disabled = true;");
			sbValid.Append("document.getElementById(\"" + btn.ClientID + "\").disabled = true;");
			//GetPostBackEventReference obtains a reference to a client-side script function that causes the server to post back to the page.

			sbValid.Append(btn.Page.ClientScript.GetPostBackEventReference(btn, String.Empty));
			sbValid.Append(";");
			btn.Attributes.Add("onclick", sbValid.ToString());
		}
		
		public static void GetButtonDisable(Button btn)
        {
			GetButtonDisable(btn, null);
        }

        public static string RequestInputStreamToString()
        {
            StringBuilder sb = new StringBuilder();
            int streamLength;
            int streamRead;
            Stream s = HttpContext.Current.Request.InputStream;
            streamLength = Convert.ToInt32(s.Length);

            Byte[] streamArray = new Byte[streamLength];

            streamRead = s.Read(streamArray, 0, streamLength);

            for (int i = 0; i < streamLength; i++)
            {
                sb.Append(Convert.ToChar(streamArray[i]));
            }

            return sb.ToString();
        }


        /// <summary>
        /// </summary>
        /// <param name="EntityOrObjectName">Product or Variant</param>
        /// <param name="ID"></param>
        /// <param name="ImgSize">medium or large</param>
        /// <param name="SkinID"></param>
        /// <param name="LocaleSetting"></param>
        /// <returns></returns>
        public static String ZoomifyMarkup(String sDataPath, String ImgSize, String AlternateImgSrc, Customer ThisCustomer, int SkinID)
        {
            return AppLogic.RunXmlPackage("Zoomify." + ImgSize, null, ThisCustomer, SkinID, "", "ImagePath=" + sDataPath + "&AltSrc=" + AlternateImgSrc, false, false);
        }

        /// <summary>
        /// </summary>
        /// <param name="EntityOrObjectName">Product or Variant</param>
        /// <param name="ID"></param>
        /// <param name="ImgSize">medium or large</param>
        /// <param name="SkinID"></param>
        /// <param name="LocaleSetting"></param>
        /// <returns></returns>
        public static String ZoomifyMarkup(String EntityOrObjectName, int ID, String ImgSize, Customer ThisCustomer, int SkinID)
        {
            String AlternateImgSrc = LookupImage(EntityOrObjectName, ID, ImgSize, SkinID, ThisCustomer.LocaleSetting);
            String dataPath = ZoomifyDirectory(EntityOrObjectName, ID);

            return ZoomifyMarkup(dataPath, ImgSize, AlternateImgSrc, ThisCustomer, SkinID);
        }


        /// <summary>
        /// </summary>
        /// <param name="EntityOrObjectName">Product or Variant</param>
        /// <param name="ID"></param>
        /// <param name="ImgSize">medium or large</param>
        /// <param name="SkinID"></param>
        /// <param name="LocaleSetting"></param>
        /// <returns></returns>
        public static bool ZoomifyExists(String EntityOrObjectName, int ID)
        {
            if (!AppLogic.AppConfigBool("Zoomify.Active"))
            {
                return false;
            }
            return (ZoomifyDirectory(EntityOrObjectName, ID).Length != 0);
        }

        public static String ZoomifyDirectory(String EntityOrObjectName, int ID)
        {
            if (!AppLogic.AppConfigBool("Zoomify.Active"))
            {
                return String.Empty;
            }

            String EONU = EntityOrObjectName;
            String FN = String.Empty;
            try
            {
                // using exception block because not all "entities or objects" support this feature, 
                // and this is easiest way to code it so it works for all of them:
                String TableName = EntityOrObjectName.Replace("]", "");

                if (TableName.Equals("VARIANT", StringComparison.InvariantCultureIgnoreCase))
                {
                    TableName = "PRODUCTVARIANT";
                }

                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS(String.Format("select ImageFilenameOverride from {0} with (NOLOCK) where {1}ID={2}", "[" + TableName + "]", EntityOrObjectName, ID.ToString()), con))
                    {
                        if (rs.Read())
                        {
                            FN = DB.RSField(rs, "ImageFilenameOverride");
                            if (FN.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase) || FN.EndsWith(".gif", StringComparison.InvariantCultureIgnoreCase) || FN.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase))
                            {
                                FN = FN.Substring(0, FN.Length - 4); // strip off the extension
                            }
                        }
                    }
                }
            }
            catch { }
            if (FN.Length == 0 && EONU == "PRODUCT" && AppLogic.AppConfigBool("UseSKUForProductImageName"))
            {
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS("select SKU from product   with (NOLOCK)  where productid=" + ID.ToString(), con))
                    {
                        if (rs.Read())
                        {
                            String SKU = DB.RSField(rs, "SKU").Trim();
                            if (SKU.Length != 0)
                            {
                                FN = SKU;
                            }
                        }
                    }
                }
            }
            if (FN.Length == 0)
            {
                FN = ID.ToString();
            }

            String Image1URL = String.Empty;

            Image1URL = ZoomifyLocateURL(FN, EONU);
            return Image1URL;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ImageName">Image filename with or without the extension</param>
        /// <param name="ImageType">e.g. Category, Section, Product</param>
        /// <param name="LocaleSetting">Viewing Locale</param>
        /// <returns>full path to the zoomify image directory</returns>
        static private String ZoomifyLocateURL(String ImageName, String ImageType)
        {
            String ImgSize = "large";
            try
            {
                ImageName = ImageName.Trim();
                string WebConfigLocale = "." + Localization.GetDefaultLocale();
                string IPath = AppLogic.GetImagePath(ImageType, ImgSize, true);

                bool UseCache = !AppLogic.IsAdminSite;

                string ImagePath = IPath + ImageName;
                string cacheKey = ImagePath + ".zoomify";
                if (UseCache && AppLogic.ImageFilenameCache.ContainsKey(cacheKey) && ((String)AppLogic.ImageFilenameCache[cacheKey]).Length > 1)
                {
                    return (String)AppLogic.ImageFilenameCache[cacheKey];
                }
                else if (Directory.Exists(ImagePath))
                {
                    if (UseCache)
                    {
                        AppLogic.ImageFilenameCache[cacheKey] = AppLogic.GetImagePath(ImageType, ImgSize, false) + ImageName;
                        return (String)AppLogic.ImageFilenameCache[cacheKey];
                    }
                    else
                    {
                        return AppLogic.GetImagePath(ImageType, ImgSize, false) + ImageName;
                    }
                }
                if (UseCache && (AppLogic.ImageFilenameCache[cacheKey] == null || (String)AppLogic.ImageFilenameCache[cacheKey] == String.Empty)) ImageFilenameCache[cacheKey] = "0";

                return String.Empty;
            }
            catch
            {
                return String.Empty;
            }
        }

        public static void AuditLogInsert(int CustomerID, int UpdatedCustomerID, int OrderNumber, string Description, string Details, string PagePath, string AuditGroup)
        {
            if (UpdatedCustomerID == 0 && OrderNumber != 0)
            {
                UpdatedCustomerID = Order.GetOrderCustomerID(OrderNumber);
            }

            if (CustomerID == 0)
            {
                try
                {
                    if (HttpContext.Current.User != null)
                    {
                        Customer AuditCustomer = ((AspDotNetStorefrontPrincipal)HttpContext.Current.User).ThisCustomer;
                        if (AuditCustomer != null)
                        {
                            CustomerID = AuditCustomer.CustomerID;
                        }
                    }
                }
                catch { }
            }

            if (Description.Length > 100) Description = CommonLogic.Ellipses(Description, 100, false);
            if (Details.Length > 1000) Details = CommonLogic.Ellipses(Details, 1000, true);
            if (PagePath.Length > 200) PagePath = PagePath.Substring(0, 200);
            if (AuditGroup.Length > 30) AuditGroup = AuditGroup.Substring(0, 30);
            DB.ExecuteSQL("insert into AuditLog(CustomerID,UpdatedCustomerID,OrderNumber,"
                + "Description,Details,PagePath,AuditGroup)"
                + " values(" + CustomerID.ToString() + "," + UpdatedCustomerID.ToString() + "," + OrderNumber.ToString()
                + "," + DB.SQuote(Description) + "," + DB.SQuote(Details) + "," + DB.SQuote(PagePath) + "," + DB.SQuote(AuditGroup) + ")");
        }

        /// <summary>
        /// Determine the current website id.
        /// </summary>
        /// <returns>Store id.</returns>
        public static int StoreID()
        {
            // force default
            int sID = 1;
            if (HttpContext.Current != null &&
                HttpContext.Current.Items["StoreId"] != null)
            {
                return Convert.ToInt32(HttpContext.Current.Items["StoreId"]);
            }
            return sID;
        }

        /// <summary>
        /// Gets the default store id
        /// </summary>
        /// <returns>Store id.</returns>
        public static int DefaultStoreID()
        {
            // force default
            int defStoreId = 1;
            if (HttpContext.Current != null &&
                HttpContext.Current.Items["DefaultStoreId"] != null)
            {
                return Convert.ToInt32(HttpContext.Current.Items["DefaultStoreId"]);
            }
            else
            {
                var defStore = AppLogic.GetDefaultStore();
                if (defStore != null)
                {
                    defStoreId = defStore.StoreID;
                }
            }

            return defStoreId;
        }

        /// <summary>
        /// Determine the store id where the order is created.
        /// </summary>
        /// <param name="orderNumber">Order number to be queried.</param>
        /// <returns>Store ID of the order.</returns>
        public static int GetOrdersStoreID(int orderNumber)
        {
            int id = 1;
            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();
                id = DB.GetSqlN(string.Format("Select StoreID as N from Orders with (NOLOCK) where OrderNumber={0}", orderNumber), conn);
            }

            return id;
        }

        /// <summary>
        /// Get the total number of products in the product table
        /// </summary>
        /// <returns>returns the total number of published and non deleted products from the product table</returns>
        public static int GetProductCount()
        {
            int productCount = 0;
            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();
                productCount = DB.GetSqlN("Select Count(*) as N from Product with (NOLOCK) where Published = 1 and Deleted = 0", conn);
            }

            return productCount;
        }

        /// <summary>
        /// ML/Express feature that limits the number of products
        /// </summary>
        /// <returns>returns true if total count of published and non deleted products exceeded the specified limit</returns>
        public static bool MaxProductsExceeded()
        {
            return false;
        }

        public static int GetEntitiesCount()
        {
            int entityCategoryCount = 0;
            int entitySectionCount = 0;
            int manufacturerEntity = 0;

            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();
                entityCategoryCount = DB.GetSqlN("Select Count(*) as N from Category with (NOLOCK) where Published = 1 and Deleted = 0", conn);
                entitySectionCount = DB.GetSqlN("Select Count(*) as N from Section with (NOLOCK) where Published = 1 and Deleted = 0", conn);
                manufacturerEntity = DB.GetSqlN("Select Count(*) as N from Manufacturer with (NOLOCK) where Published = 1 and Deleted = 0", conn);
            }

            return entityCategoryCount + entitySectionCount + manufacturerEntity;
        }

        /// <summary>
        /// ML/Express feature that limits the number of entities
        /// </summary>
        /// <returns>returns true if total count of published and non deleted entities exceeded the specified limit</returns>
        public static bool MaxEntitiesExceeded()
        {
            return false;
        }
        /// <summary>
        /// Composes the returnUrl query string into 1 chunk of string. 
        /// Escapes the & and = characters on the string to preserve the original url if it has additonal query strings
        /// so that it will not get truncated on the next page that will use the returnUrl value.
        /// The code that will use this value should use ReturnUrlDecode to properly interpret the original url passed
        /// Sample:
        /// http://localhost/AspDotNetStoreFront70/p-1-simple-product-1.aspx?hello=world
        /// Becomes:
        /// http://localhost/AspDotNetStoreFront70/p-1-simple-product-1.aspx?hello$world
        /// </summary>
        /// <param name="returnUrl">The return url string to be used on the query string</param>
        /// <returns></returns>
        public static string ReturnURLEncode(string returnUrl)
        {
            return returnUrl.Replace('&', '|').Replace('=', '$');
        }

        /// <summary>
        /// Decodes the returnUrl string that was originally encoded by calling  ReturnUrlEncode
        /// </summary>
        /// <param name="returnUrl">The returnUrl querystring that was originally ReturnUrlEncoded</param>
        /// <returns></returns>
        public static string ReturnURLDecode(string returnUrl)
        {
            return returnUrl.Replace('|', '&').Replace('$', '=');
        }

        /// <summary>
        /// Gets the current page name together with additional query strings if any
        /// This method honors the current page if it's url-rewrited + any other query strings
        /// i.e. http://localhost/AspDotNetStoreFront70/p-1-simple-product-1.aspx?hello=world
        /// The example holds 3 query strings even though it's url rewrited : ProductID, SEName and Hello
        /// It will return the url in the original format plus append any additional query strings
        /// </summary>
        /// <returns></returns>
        public static string GetThisPageUrlWithQueryString()
        {
            return GetThisPageUrlWithQueryString(string.Empty);
        }

        /// <summary>
        /// Gets the current page name together with additional query strings if any
        /// This method honors the current page if it's url-rewrited + any other query strings
        /// i.e. http://localhost/AspDotNetStoreFront70/p-1-simple-product-1.aspx?hello=world
        /// The example holds 3 query strings even though it's url rewrited : ProductID, SEName and Hello
        /// It will return the url in the original format plus append any additional query strings
        /// </summary>
        /// <param name="additionalQueryStringsInNameValuePair">The additional query strings in name=value&name=value format</param>
        /// <returns></returns>
        public static string GetThisPageUrlWithQueryString(string additionalQueryStringsInNameValuePair)
        {
            return GetThisPageUrlWithQueryString(additionalQueryStringsInNameValuePair, true);
        }

        /// <summary>
        /// Gets the current page name together with additional query strings if any
        /// This method honors the current page if it's url-rewrited + any other query strings
        /// i.e. http://localhost/AspDotNetStoreFront70/p-1-simple-product-1.aspx?hello=world
        /// The example holds 3 query strings even though it's url rewrited : ProductID, SEName and Hello
        /// It will return the url in the original format plus append any additional query strings
        /// </summary>
        /// <param name="additionalQueryStringsInNameValuePair">The additional query strings in name=value&name=value format</param>
        /// <param name="encodeValues">The flag whether to url encode the values, should always default this to true</param>
        /// <returns></returns>
        public static string GetThisPageUrlWithQueryString(string additionalQueryStringsInNameValuePair, bool encodeValues)
        {
            // NOTE:
            //  If the current url is url-rewrited, we must honor the current format
            StringBuilder url = new StringBuilder();

            string pageName = CommonLogic.IIF(
                                HttpContext.Current.Items["RequestedPage"] != null,
                                HttpContext.Current.Items["RequestedPage"].ToString(),
                                CommonLogic.GetThisPageName(false));
            url.Append(pageName);

            // our data structure to hold temporarily the query name value pairs
            Dictionary<string, string> allQueryStrings = new Dictionary<string, string>();

            string originalQueryStringsInNameValuePair = CommonLogic.IIF(
                                            HttpContext.Current.Items["RequestedQuerystring"] != null,
                                            HttpContext.Current.Items["RequestedQuerystring"].ToString(),
                                            HttpContext.Current.Request.Url.Query);

            if (originalQueryStringsInNameValuePair.StartsWith("?"))
            {
                originalQueryStringsInNameValuePair = originalQueryStringsInNameValuePair.Remove(0, 1);
            }

            string[] originalQueryStrings = originalQueryStringsInNameValuePair.Split('&');

            // first add the original query strings if any            
            if (originalQueryStrings.Length > 0)
            {
                foreach (string queryStringNameValuePair in originalQueryStrings)
                {
                    string[] queryStringValues = queryStringNameValuePair.Split('=');
                    if (queryStringValues.Length == 2)
                    {
                        string queryStringName = queryStringValues[0];
                        string queryStringValue = queryStringValues[1];

                        // let's make sure we have no duplicates in the query string
                        // if we have any, we'll use the first one
                        if (!allQueryStrings.ContainsKey(queryStringName))
                        {
                            allQueryStrings.Add(queryStringName, queryStringValue);
                        }
                    }
                }
            }

            // now let's add the additional query strings if we have any
            string[] additionalQueryStrings = additionalQueryStringsInNameValuePair.Split('&');
            if (additionalQueryStrings.Length > 0)
            {
                foreach (string queryStringNameValuePair in additionalQueryStrings)
                {
                    string[] queryStringValues = queryStringNameValuePair.Split('=');
                    if (queryStringValues.Length == 2)
                    {
                        string queryStringName = queryStringValues[0];
                        string queryStringValue = queryStringValues[1];

                        // let's make sure we have no duplicates in the query string
                        // if we have any, we'll use the first one
                        if (!allQueryStrings.ContainsKey(queryStringName))
                        {
                            allQueryStrings.Add(queryStringName, queryStringValue);
                        }
                    }
                }
            }

            // check if we have query strings
            if (allQueryStrings.Count > 0)
            {
                url.Append("?");

                int ctr = 0;
                foreach (KeyValuePair<string, string> queryString in allQueryStrings)
                {
                    if (ctr != 0)
                    {
                        url.Append("&");
                    }

                    url.AppendFormat(
                        "{0}={1}",
                        HttpUtility.UrlEncode(queryString.Key),
                        CommonLogic.IIF(encodeValues, HttpUtility.UrlEncode(queryString.Value), queryString.Value));

                    ctr++;
                }
            }

            return url.ToString();
        }

        /// <summary>
        /// Returns a site relative HTTP path from a partial path starting out with a ~.
        /// Same syntax that ASP.Net internally supports but this method can be used
        /// outside of the Page framework.
        ///
        /// Works like Control.ResolveUrl including support for ~ syntax
        /// but returns an absolute URL.
        /// </summary>
        /// <param name="originalUrl">Any Url including those starting with ~</param>
        /// <returns>relative url</returns>
        public static string ResolveUrl(string originalUrl)
        {
            if (originalUrl == null) return null;

            // *** Absolute path - just return
            if (originalUrl.IndexOf("://") != -1) return originalUrl;

            // *** Fix up image path for ~ root app dir directory
            if (originalUrl.StartsWith("~"))
            {
                string newUrl = "";
                if (HttpContext.Current != null)
                {
                    newUrl = HttpContext.Current.Request.ApplicationPath + originalUrl.Substring(1).Replace("//", "/");
                }
                else
                {
                    // *** Not context: assume current directory is the base directory
                    throw new ArgumentException("Invalid URL: Relative URL not allowed.");
                }

                // *** Just to be sure fix up any double slashes
                return newUrl.Replace("//", "/");
            }

            return originalUrl;
        }

        /// <summary>
        /// Formats fully qualified page URLs for use on the admin site since masterpages can be upredictable with relative urls
        /// </summary>
        /// <param name="pageName">page name to generate the link to</param>
        /// <returns>fully qualified URL to the supplied page name</returns>
        static public String AdminLinkUrl(String pageName)
        {
            return AdminLinkUrl(pageName, false);
        }

        static public String AdminLinkUrl(String pageName, bool AllowSub)
        {
            if (pageName.ToLowerInvariant().Contains("http"))
            {
                //fully qualified URL.  Nothing to do.
                return pageName;
            }
            else
            {
                String caller = HttpContext.Current.Request.FilePath;
                String url = caller.Remove(caller.LastIndexOf("/"));

                // don't replace "/" because we're digging into a subdirectory
                if (AllowSub)
                {
                    return url + "/" + pageName;
                }
                return url + "/" + pageName.Replace("/", "");
            }
        }

        #region CUSTOMGLOBALLOGIC
        public static void Custom_ApplicationStart_Logic(Object sender, EventArgs e)
        {
            // put any custom application start logic you need here...
            // do not change this routine unless you know exactly what you are doing
        }
        public static void Custom_ApplicationEnd_Logic(Object sender, EventArgs e)
        {
            // put any custom application end logic you need here...
            // do not change this routine unless you know exactly what you are doing
        }
        public static void Custom_SessionStart_Logic(Object sender, EventArgs e)
        {
            // put any custom session start logic you need here...
            // do not change this routine unless you know exactly what you are doing
        }
        public static void Custom_SessionEnd_Logic(Object sender, EventArgs e)
        {
            // put any custom session end logic you need here...
            // do not change this routine unless you know exactly what you are doing
        }
        public static void Custom_Application_Error(Object sender, EventArgs e)
        {
            // put any custom application error logic you need here...
            // do not change this routine unless you know exactly what you are doing
        }
        public static void Custom_Application_EndRequest_Logic(Object sender, EventArgs e)
        {
            // put any custom application end request logic you need here...
            // do not change this routine unless you know exactly what you are doing
        }
        public static bool Custom_Application_BeginRequest_Logic(Object sender, EventArgs e)
        {
            // put any custom application begin request logic you need here...
            // return TRUE if you do NOT want our UrlRewriter to fire
            // return FALSE if you do want our UrlRewriter to fire and handle this event
            // do not change this routine unless you know exactly what you are doing
            return true;
        }
        #endregion


        #region GetSuperAdminCustomerIDs
        /// <summary>
        /// Gets the CustomerIDs of Super Admin Users
        /// </summary>
        /// <returns></returns>
        public static IntegerCollection GetSuperAdminCustomerIDs()
        {
            IntegerCollection ids = new IntegerCollection();

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("SELECT CustomerID FROM Customer WHERE IsAdmin = 3", con))
                {
                    while (rs.Read())
                    {
                        ids.Add(DB.RSFieldInt(rs, "CustomerID"));
                    }
                }
            }

            return ids;
        }
        #endregion

        /// <summary>
        /// Gets the primary billing address ID.
        /// </summary>
        /// <param name="CustomerID">The customer ID.</param>
        /// <returns></returns>
        public static int GetPrimaryBillingAddressID(int CustomerID)
        {
            int tmp = 0;

            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                string sSql = String.Format("SELECT BillingAddressID FROM Customer WHERE CustomerID={0}", CustomerID);
                using (IDataReader rs = DB.GetRS(sSql, conn))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSFieldInt(rs, "BillingAddressID");
                    }
                }
            }

            return tmp;
        }

        /// <summary>
        /// Gets the primary shipping address ID.
        /// </summary>
        /// <param name="CustomerID">The customer ID.</param>
        /// <returns></returns>
        public static int GetPrimaryShippingAddressID(int CustomerID)
        {
            int tmp = 0;

            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                string sSql = String.Format("SELECT ShippingAddressID FROM Customer WHERE CustomerID={0}", CustomerID);
                using (IDataReader rs = DB.GetRS(sSql, conn))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSFieldInt(rs, "ShippingAddressID");
                    }
                }
            }

            return tmp;
        }

        /// <summary>
        /// Checks if the current request is a callback
        /// </summary>
        /// <returns></returns>
        public static bool CurrentRequestIsCallBack()
        {
            HttpContext ctx = HttpContext.Current;
            if (ctx != null)
            {
                return ctx.CurrentHandler is Page &&
                        (ctx.CurrentHandler as Page).IsCallback;
            }

            return false;
        }

        /// <summary>
        /// Checks the collection for the name value pair
        /// </summary>
        /// <param name="values"></param>
        /// <param name="name"></param>
        /// <param name="compareWith"></param>
        /// <returns></returns>
        public static bool ContainsValue(NameValueCollection values, string name, string compareWith)
        {
            foreach (string key in values.AllKeys)
            {
                if (key.EqualsIgnoreCase(name))
                {
                    string value = values[key];
                    return !string.IsNullOrEmpty(value) && value.EqualsIgnoreCase(compareWith);
                }
            }

            return false;
        }
        
        /// <summary>
        /// Checks the current request if it's an async postback for asp.net ajax Update Panel
        /// </summary>
        /// <returns></returns>
        public static bool CurrentRequestIsAsyncPostBack()
        {
            HttpContext ctx = HttpContext.Current;
            if (ctx != null)
            {
                return ContainsValue(ctx.Request.Headers, "X-MicrosoftAjax", "Delta=true") &&
                        ContainsValue(ctx.Request.Form, "__ASYNCPOST", "true");
            }

            return false;
        }

        /// <summary>
        /// Used for aspx page that do not inherit SkinBase.
        /// Example are those gateways that requires external redirection.
        /// </summary>
        /// <returns>Current customer making the order.</returns>
        public static Customer GetCurrentCustomer()
        {
            Customer m_ThisCustomer = Customer.Current;
            Customer PhoneCustomer = null;
            bool IGDQueryClear = false;
            string m_IGD = CommonLogic.QueryStringCanBeDangerousContent("IGD").Trim();
            if (m_IGD.Length == 0 && CommonLogic.ServerVariables("QUERY_STRING").IndexOf("IGD=") != -1)
            {
                m_IGD = String.Empty; // there was IGD={blank} in the query string, so forcefully clear IGD!
                IGDQueryClear = true;
            }
            bool IsStartOfImpersonation = m_IGD.Length != 0; // the url invocation starts the impersonation only!

            if (!IGDQueryClear && m_IGD.Length == 0)
            {
                if (m_ThisCustomer.IsAdminUser)
                {
                    // pull out the impersonation IGD from the customer session, if any
                    m_IGD = m_ThisCustomer.ThisCustomerSession["IGD"];
                }
            }

            if (IGDQueryClear)
            {
                // forcefully clear any IGD for this customer, just to be safe!
                m_ThisCustomer.ThisCustomerSession["IGD"] = "";
                m_ThisCustomer.ThisCustomerSession["IGD_EDITINGORDER"] = "";
            }

            if (m_IGD.Length != 0)
            {
                if (m_ThisCustomer.IsAdminUser)
                {
                    try
                    {
                        Guid IGD = new Guid(m_IGD);
                        PhoneCustomer = new Customer(IGD);
                    }
                    catch
                    {
                        m_ThisCustomer.ThisCustomerSession["IGD"] = "";
                        m_ThisCustomer.ThisCustomerSession["IGD_EDITINGORDER"] = "";
                        m_IGD = string.Empty;
                    }
                }
                if (PhoneCustomer != null && PhoneCustomer.HasCustomerRecord)
                {
                    int ImpersonationTimeoutInMinutes = AppLogic.AppConfigUSInt("ImpersonationTimeoutInMinutes");
                    if (ImpersonationTimeoutInMinutes == 0)
                    {
                        ImpersonationTimeoutInMinutes = 20;
                    }
                    if (PhoneCustomer.ThisCustomerSession.LastActivity >= DateTime.Now.AddMinutes(-ImpersonationTimeoutInMinutes))
                    {
                        m_ThisCustomer.ThisCustomerSession["IGD"] = m_IGD;
                        m_ThisCustomer = PhoneCustomer; // build the impersonation customer the phone order customer
                        bool IsAdmin = CommonLogic.ApplicationBool("IsAdminSite");
                        if (!HttpContext.Current.Items.Contains("IsBeingImpersonated"))
                        {
                            HttpContext.Current.Items.Add("IsBeingImpersonated", "true");
                        }
                    }
                    else
                    {
                        m_ThisCustomer.ThisCustomerSession["IGD"] = "";
                        m_ThisCustomer.ThisCustomerSession["IGD_EDITINGORDER"] = "";
                        m_ThisCustomer = null;
                    }
                }
            }
            return m_ThisCustomer;
        }

        public static void DisableAutocomplete(TextBox tb)
        {
            if (tb == null)
                return;

            tb.Attributes.Add("autocomplete", "off");
            tb.AutoCompleteType = AutoCompleteType.Disabled;
        }

        public static void LowInventoryWarning(List<int> variantIds)
        {
            bool sendWarning = false;
            int warningThreshold = AppLogic.AppConfigNativeInt("SendLowStockWarningsThreshold");
            string lowProducts = string.Empty;

            foreach (int vId in variantIds)
            {
                ProductVariant varToCheck = new ProductVariant(vId);
                if (varToCheck.Inventory < warningThreshold)
                {
                    Product tempProduct = new Product(varToCheck.ProductID);
                    lowProducts += Environment.NewLine + tempProduct.LocaleName + " (VariantID: " + varToCheck.VariantID.ToString() + ")";
                    sendWarning = true;
                }
            }

            if (sendWarning)
            {
                try //Don't not display the orderconfirmation page just because email sending failed
                {
                    AppLogic.SendMail(AppLogic.AppConfig("StoreName") + " " + AppLogic.GetStringForDefaultLocale("admin.LowStockWarningTitle"), 
                        AppLogic.GetStringForDefaultLocale("admin.LowStockWarningBody") + lowProducts, 
                        false, 
                        AppLogic.AppConfig("GotOrderEmailFrom"),
                        AppLogic.AppConfig("GotOrderEmailFromName"),
                        AppLogic.AppConfig("GotOrderEmailTo"), 
                        AppLogic.AppConfig("MailMe_ToName"), 
                        string.Empty, 
                        AppLogic.AppConfig("MailMe_Server"));
                }
                catch (Exception ex)
                {
                    var logEx = new Exception("Low stock warning couldn't be sent", ex);
                    SysLog.LogException(logEx, MessageTypeEnum.GeneralException, MessageSeverityEnum.Message);
                }
            }
        }

        public static Boolean AllowRegularCheckout(ShoppingCart cart)
        {
            String allowedPaymentMethods = AppLogic.AppConfig("PaymentMethods").ToUpperInvariant();
            if (AppLogic.MicropayIsEnabled() && !cart.HasSystemComponents())
            {
                if (allowedPaymentMethods.Length != 0)
                {
                    allowedPaymentMethods += ",";
                }
                allowedPaymentMethods += AppLogic.ro_PMMicropay.ToUpper();
            }

            List<string> listOfAllowedMethods = allowedPaymentMethods.Replace(" ", "").Split(',').ToList();

            return listOfAllowedMethods.Contains(AppLogic.ro_PMCreditCard)
                || listOfAllowedMethods.Contains(AppLogic.ro_PMPurchaseOrder)
                || listOfAllowedMethods.Contains(AppLogic.ro_PMCODMoneyOrder)
                || listOfAllowedMethods.Contains(AppLogic.ro_PMCODCompanyCheck)
                || listOfAllowedMethods.Contains(AppLogic.ro_PMCODNet30)
                || listOfAllowedMethods.Contains(AppLogic.ro_PMPayPal)
                || listOfAllowedMethods.Contains(AppLogic.ro_PMRequestQuote)
                || listOfAllowedMethods.Contains(AppLogic.ro_PMCheckByMail)
                || listOfAllowedMethods.Contains(AppLogic.ro_PMCOD)
                || listOfAllowedMethods.Contains(AppLogic.ro_PMECheck)
                || listOfAllowedMethods.Contains(AppLogic.ro_PMCardinalMyECheck)
                || listOfAllowedMethods.Contains(AppLogic.ro_PMMoneybookersQuickCheckout)
                || listOfAllowedMethods.Contains(AppLogic.ro_PMMicropay);
        }

        public static Boolean CheckForMobileRequest()
        {
			string mobileCookieName = "ASPDNSF.IsMobile";
			bool cookieIsMobile = CommonLogic.CookieBool(mobileCookieName);
			bool contextIsMobile = (bool?)HttpContext.Current.Items["IsMobile"] ?? false;
			
			return (cookieIsMobile || contextIsMobile);
        }
    }



    /// <summary>
    /// Represents Collection of Int32s
    /// </summary>
    public class IntegerCollection : List<int>
    {
        /// <summary>
        /// Gets the comma separated values of this collection
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            String output = string.Empty;

            for (int ctr = 0; ctr < this.Count; ctr++)
            {
                output += this[ctr].ToString();

                if (ctr + 1 != this.Count)
                {
                    output += ", ";
                }
            }

            return output;
        }

        public static IntegerCollection Parse(string s)
        {
            IntegerCollection ints = new IntegerCollection();

            string[] intStrings = s.Split(',');
            foreach (string intString in intStrings)
            {
                int id = 0;
                if (int.TryParse(intString, out id))
                {
                    ints.Add(id);
                }
            }

            return ints;
        }
    }



    public static class AppStartLogger
    {
        public static void WriteLine(String Message)
        {
            // don't let logging crash anything itself!
            try
            {
                FileStream fs = new FileStream(CommonLogic.SafeMapPath("images/appstart.log"), FileMode.Append, FileAccess.Write, FileShare.Read | FileShare.Delete);
                StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
                sw.AutoFlush = true;
                sw.WriteLine("{0:G}: {1}\r\n", DateTime.Now, Message);
                sw.Close();
                fs.Close();
            }
            catch { }
        }

        public static void ResetLog()
        {
            // don't let logging crash anything itself!
            try
            {
                File.Delete(CommonLogic.SafeMapPath("images/appstart.log"));
            }
            catch { }
        }
    }

    public class AspDotNetStorefrontPrincipal : IPrincipal
    {
        private Customer m_customer;

        public AspDotNetStorefrontPrincipal(Customer customer)
        {
            m_customer = customer;
        }


        #region IPrincipal Members implementation
        public IIdentity Identity
        {
            get
            {
                return m_customer;
            }
        }


        public bool IsInRole(string role)
        {
            foreach (string r in m_customer.Roles.Split(','))
            {
                if (r.Equals(role, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region Public Properties
        public string Roles
        {
            get { return this.m_customer.Roles.ToString(); }
        }
        public Customer ThisCustomer
        {
            get { return m_customer; }
        }
        #endregion
    }    

}
