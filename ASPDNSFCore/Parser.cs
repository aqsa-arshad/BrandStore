// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.VisualBasic;

namespace AspDotNetStorefrontCore
{
    /// <summary>
    /// Summary description for Parser.
    /// </summary>
    public class Parser
    {
        // the ASPDNSF string parser, for skin files and XmlPackages
        // replaces found tokenm_S.

        private Customer m_ThisCustomer = null;
        private int m_SkinID = 1;
        private System.Collections.Generic.Dictionary<string, EntityHelper> m_EntityHelpers; // dictionary of EntityHelpers, key = EntityName
        private string RegExString = "\\(\\!(\\w+)(?:\\s(?:(\\w*)=(?:'|\")(.*?)(?:\"|'))?)*\\!\\)";
        private MatchEvaluator m_CmdMatchEval;
        private int m_CacheMinutes = 0;

        // static tokes are the same from one page request to the next, regardless of who's logged in, viewing the page, 
        // and what the active locale is, etc
        // these are put in application cache, if caching is enabled
        private Hashtable m_StaticTokens = null;

        // dynamic tokens vary on EACH page request, due to live customer state, etc.
        // these are not cached, but built up the first time a parser object is created on the page request.
        private Hashtable m_DynamicTokens = null;

        public Parser(int SkinID, Customer cust)
            : this(null, SkinID, cust)
        { }

        public Parser(System.Collections.Generic.Dictionary<string, EntityHelper> EntityHelpers, int SkinID, Customer cust)
        {
            m_ThisCustomer = cust;
            if (m_ThisCustomer == null)
            {
                try
                {
                    m_ThisCustomer = ((AspDotNetStorefrontPrincipal)HttpContext.Current.User).ThisCustomer;
                }
                catch { }
                if (m_ThisCustomer == null)
                {
                    m_ThisCustomer = new Customer(true);
                }
            }
            m_SkinID = SkinID;
            m_EntityHelpers = EntityHelpers;
            m_CmdMatchEval = new MatchEvaluator(CommandMatchEvaluator);
            m_CacheMinutes = AppLogic.CacheDurationMinutes();
        }

        public String ReplacePageStaticTokens(String s)
        {
            if (m_StaticTokens == null)
            {
                BuildPageStaticTokens();
            }
            IDictionaryEnumerator en = m_StaticTokens.GetEnumerator();
            String result = s;
            while (en.MoveNext())
            {
                result = Strings.Replace(result, en.Key.ToString(), en.Value.ToString(), 1, -1, CompareMethod.Text);    
            }
            // PROCESS REGEX TYPE TOKENS HERE, e.g. AppConfig, Param, Topic, XmlPackage, Localize, etc...
            return Regex.Replace(result.ToString(), RegExString, m_CmdMatchEval, RegexOptions.Compiled);
        }

        public String ReplacePageDynamicTokens(String s)
        {
            if (m_DynamicTokens == null)
            {
                BuildPageDynamicTokens();
            }
            StringBuilder result = new StringBuilder(s, s.Length * 4);
            IDictionaryEnumerator en = m_DynamicTokens.GetEnumerator();
            while (en.MoveNext())
            {
                result.Replace(en.Key.ToString(), en.Value.ToString());
            }
            // PROCESS REGEX TYPE TOKENS HERE, e.g. AppConfig, Param, Topic, XmlPackage, Localize, etc...
            return Regex.Replace(result.ToString(), RegExString, m_CmdMatchEval, RegexOptions.Compiled);
        }

        // these are the same for ALL page requests since app start!
        public void BuildPageStaticTokens()
        {
            String m_CacheName = String.Empty;
            if (AppLogic.CachingOn)
            {
                m_CacheName = String.Format("StaticTokens_{0}_{1}_{2}_{3}_{4}_{5}", SkinID.ToString(), ThisCustomer.LocaleSetting, ThisCustomer.CurrencySetting, ThisCustomer.CustomerLevelID, ThisCustomer.AffiliateID, ThisCustomer.VATSettingReconciled);
                m_StaticTokens = (Hashtable)HttpContext.Current.Cache.Get(m_CacheName);
            }
            if (m_StaticTokens == null)
            {
                m_StaticTokens = new Hashtable();
                m_StaticTokens.Add("(!STORE_VERSION!)", String.Empty);
                m_StaticTokens.Add("(!COPYRIGHTYEARS!)", AppLogic.AppConfig("StartingCopyrightYear") + "-" + DateTime.Now.Year.ToString());
                if (AppLogic.AppConfigBool("CardinalCommerce.Centinel.Enabled"))
                {
                    m_StaticTokens.Add("(!VBV!)", "<img src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/vbv.jpg") + "\" border=\"0\" alt=\"Store protected with Verified By Visa/MasterCard Secure Initiatives\">");
                }
                else
                {
                    m_StaticTokens.Add("(!VBV!)", String.Empty);
                }
                m_StaticTokens.Add("(!SKINID!)", SkinID.ToString());
                m_StaticTokens.Add("(!RIGHTCOL!)", "The RIGHTCOL token is no longer supported. You should put the right column you want directly into your skin templtae.ascx design where you want it");
                m_StaticTokens.Add("(!SITENAME!)", AppLogic.AppConfig("StoreName"));
                m_StaticTokens.Add("(!SITE_NAME!)", AppLogic.AppConfig("StoreName"));
                m_StaticTokens.Add("(!STORELOCALE!)", Localization.GetDefaultLocale());
                m_StaticTokens.Add("(!LOCALESETTING!)", ThisCustomer.LocaleSetting);
                m_StaticTokens.Add("(!CUSTOMERLOCALE!)", ThisCustomer.LocaleSetting);
                m_StaticTokens.Add("(!CURRENCY_LOCALE_ROBOTS_TAG!)", String.Empty); //CommonLogic.IIF(ThisCustomer.CurrencySetting == Localization.GetPrimaryCurrency() && ThisCustomer.LocaleSetting == Localization.GetWebConfigLocale(), String.Empty, "<meta name=\"robots\" content=\"noindex,nofollow,noarchive\">")); // to prevent indexing of store pages in "foreign" currencies"
                m_StaticTokens.Add("(!SEARCH_BOX!)", AppLogic.GetSearchBox(SkinID, ThisCustomer.LocaleSetting));
                m_StaticTokens.Add("(!COUNTRYBAR!)", AppLogic.GetCountryBar(ThisCustomer.LocaleSetting));
                m_StaticTokens.Add("(!HELPBOX!)", AppLogic.GetHelpBox(SkinID, true, ThisCustomer.LocaleSetting, null));
                m_StaticTokens.Add("(!HELPBOX_CONTENTS!)", AppLogic.GetHelpBox(SkinID, false, ThisCustomer.LocaleSetting, null));
                m_StaticTokens.Add("(!NEWS_SUMMARY!)", AppLogic.GetNewsSummary(3));
                m_StaticTokens.Add("(!CATEGORY_PROMPT!)", CommonLogic.IIF(AppLogic.GetString("AppConfig.CategoryPromptPlural", SkinID, ThisCustomer.LocaleSetting).Length != 0, AppLogic.GetString("AppConfig.CategoryPromptPlural", SkinID, ThisCustomer.LocaleSetting), AppLogic.GetString("skinbase.cs.3", SkinID, ThisCustomer.LocaleSetting)));
                m_StaticTokens.Add("(!CATEGORY_PROMPT_SINGULAR!)", CommonLogic.IIF(AppLogic.GetString("AppConfig.CategoryPromptSingular", SkinID, ThisCustomer.LocaleSetting).Length != 0, AppLogic.GetString("AppConfig.CategoryPromptSingular", SkinID, ThisCustomer.LocaleSetting), AppLogic.GetString("skinbase.cs.3", SkinID, ThisCustomer.LocaleSetting)));
                m_StaticTokens.Add("(!CATEGORY_PROMPT_PLURAL!)", CommonLogic.IIF(AppLogic.GetString("AppConfig.CategoryPromptPlural", SkinID, ThisCustomer.LocaleSetting).Length != 0, AppLogic.GetString("AppConfig.CategoryPromptPlural", SkinID, ThisCustomer.LocaleSetting), AppLogic.GetString("skinbase.cs.2", SkinID, ThisCustomer.LocaleSetting)).ToUpperInvariant());
                m_StaticTokens.Add("(!SECTION_PROMPT!)", CommonLogic.IIF(AppLogic.GetString("AppConfig.SectionPromptPlural", SkinID, ThisCustomer.LocaleSetting).Length != 0, AppLogic.GetString("AppConfig.SectionPromptPlural", SkinID, ThisCustomer.LocaleSetting), AppLogic.GetString("skinbase.cs.2", SkinID, ThisCustomer.LocaleSetting)));
                m_StaticTokens.Add("(!SECTION_PROMPT_SINGULAR!)", CommonLogic.IIF(AppLogic.GetString("AppConfig.SectionPromptSingular", SkinID, ThisCustomer.LocaleSetting).Length != 0, AppLogic.GetString("AppConfig.SectionPromptSingular", SkinID, ThisCustomer.LocaleSetting), AppLogic.GetString("skinbase.cs.2", SkinID, ThisCustomer.LocaleSetting)));
                m_StaticTokens.Add("(!SECTION_PROMPT_PLURAL!)", CommonLogic.IIF(AppLogic.GetString("AppConfig.SectionPromptPlural", SkinID, ThisCustomer.LocaleSetting).Length != 0, AppLogic.GetString("AppConfig.SectionPromptPlural", SkinID, ThisCustomer.LocaleSetting), AppLogic.GetString("skinbase.cs.1", SkinID, ThisCustomer.LocaleSetting)).ToUpperInvariant());
                m_StaticTokens.Add("(!MANUFACTURER_PROMPT!)", CommonLogic.IIF(AppLogic.GetString("AppConfig.ManufacturerPromptPlural", SkinID, ThisCustomer.LocaleSetting).Length != 0, AppLogic.GetString("AppConfig.ManufacturerPromptPlural", SkinID, ThisCustomer.LocaleSetting), AppLogic.GetString("skinbase.cs.3", SkinID, ThisCustomer.LocaleSetting)));
                m_StaticTokens.Add("(!MANUFACTURER_PROMPT_SINGULAR!)", CommonLogic.IIF(AppLogic.GetString("AppConfig.ManufacturerPromptSingular", SkinID, ThisCustomer.LocaleSetting).Length != 0, AppLogic.GetString("AppConfig.ManufacturerPromptSingular", SkinID, ThisCustomer.LocaleSetting), AppLogic.GetString("skinbase.cs.3", SkinID, ThisCustomer.LocaleSetting)));
                m_StaticTokens.Add("(!MANUFACTURER_PROMPT_PLURAL!)", CommonLogic.IIF(AppLogic.GetString("AppConfig.ManufacturerPromptPlural", SkinID, ThisCustomer.LocaleSetting).Length != 0, AppLogic.GetString("AppConfig.ManufacturerPromptPlural", SkinID, ThisCustomer.LocaleSetting), AppLogic.GetString("skinbase.cs.2", SkinID, ThisCustomer.LocaleSetting)).ToUpperInvariant());
                m_StaticTokens.Add("(!UNSUP_4!)", AppLogic.GetCategoryBox(AppLogic.AppConfigUSInt("KitCategoryID"), true, 5, false, "Our custom tailored kits provide everything you need in one package!", SkinID, ThisCustomer.LocaleSetting));
                m_StaticTokens.Add("(!ADMIN_FOR!)", AppLogic.GetString("admin.main.ascx.AdminFor", 1, Localization.GetDefaultLocale()));
                
                foreach (String EntityName in AppLogic.ro_SupportedEntities)
                {
                    String ENU = EntityName.ToUpperInvariant();
                    StringBuilder tmpSx = new StringBuilder(4096);
                    EntityHelper Helper = AppLogic.LookupHelper(EntityName, 0);

                    m_StaticTokens.Add("(!" + ENU + "_BROWSE_BOX!)", Helper.GetEntityBrowseBox(SkinID, ThisCustomer.LocaleSetting));
                }
                m_StaticTokens = AspDotNetStorefront.Global.CompleteParser(m_StaticTokens);
            }
            if (AppLogic.CachingOn)
            {
                HttpContext.Current.Cache.Insert(m_CacheName, m_StaticTokens, null, System.DateTime.Now.AddMinutes(m_CacheMinutes), TimeSpan.Zero);
            }
        }

        // these can change on EVERY page request!!
        public void BuildPageDynamicTokens()
        {
            if (m_DynamicTokens == null)
            {
                // page/customer specific items (that may change every page):
                m_DynamicTokens = new Hashtable();

                if (CommonLogic.GetThisPageName(false).ToLowerInvariant().StartsWith("orderconfirmation.aspx"))
                {
                    m_DynamicTokens.Add("(!GOOGLE_ECOM_TRACKING!)", AppLogic.GetGoogleEComTracking(ThisCustomer));
                }
                else
                {
                    m_DynamicTokens.Add("(!GOOGLE_ECOM_TRACKING!)", String.Empty);
                }

                if (CommonLogic.GetThisPageName(false).ToLowerInvariant().StartsWith("orderconfirmation.aspx"))
                {
                    m_DynamicTokens.Add("(!GOOGLE_ECOM_TRACKING_V2!)", String.Empty);
                }
                else
                {
                    m_DynamicTokens.Add("(!GOOGLE_ECOM_TRACKING_V2!)", AppLogic.GetGoogleEComTrackingV2(ThisCustomer, false));
                }

                if (!AppLogic.VATIsEnabled())
                {
                    m_DynamicTokens.Add("(!VATREGISTRATIONID!)", String.Empty);
                }
                else
                {
                    StringBuilder tmpS2 = new StringBuilder(1024);
                    if (ThisCustomer.HasCustomerRecord)
                    {
                        tmpS2.Append("<span class=\"VATRegistrationIDPrompt\">" + AppLogic.GetString("setvatsetting.aspx.8", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "</span><input type=\"text\" style=\"VATRegistrationID\" id=\"VATRegistrationID\" value=\"" + ThisCustomer.VATRegistrationID + "\">");
                    }
                    m_DynamicTokens.Add("(!VATREGISTRATIONID!)", tmpS2.ToString());
                }

                if (AppLogic.NumLocaleSettingsInstalled() < 2)
                {
                    m_DynamicTokens.Add("(!COUNTRYDIVVISIBILITY!)", "hidden");
                    m_DynamicTokens.Add("(!COUNTRYDIVDISPLAY!)", "none");
                    m_DynamicTokens.Add("(!COUNTRYSELECTLIST!)", String.Empty);
                }
                else
                {
                    m_DynamicTokens.Add("(!COUNTRYDIVVISIBILITY!)", "visible");
                    m_DynamicTokens.Add("(!COUNTRYDIVDISPLAY!)", "inline");
                    m_DynamicTokens.Add("(!COUNTRYSELECTLIST!)", AppLogic.GetCountrySelectList(ThisCustomer.LocaleSetting));
                }

                if (Currency.NumPublishedCurrencies() < 2)
                {
                    m_DynamicTokens.Add("(!CURRENCYDIVVISIBILITY!)", "hidden");
                    m_DynamicTokens.Add("(!CURRENCYDIVDISPLAY!)", "none");
                    m_DynamicTokens.Add("(!CURRENCYSELECTLIST!)", String.Empty);
                }
                else
                {
                    m_DynamicTokens.Add("(!CURRENCYDIVVISIBILITY!)", "visible");
                    m_DynamicTokens.Add("(!CURRENCYDIVDISPLAY!)", "inline");
                    m_DynamicTokens.Add("(!CURRENCYSELECTLIST!)", AppLogic.GetCurrencySelectList(ThisCustomer));
                }

                if (AppLogic.VATIsEnabled() && AppLogic.AppConfigBool("VAT.AllowCustomerToChooseSetting"))
                {
                    m_DynamicTokens.Add("(!VATDIVVISIBILITY!)", "visible");
                    m_DynamicTokens.Add("(!VATDIVDISPLAY!)", "inline");
                    m_DynamicTokens.Add("(!VATSELECTLIST!)", AppLogic.GetVATSelectList(ThisCustomer));
                }
                else
                {
                    m_DynamicTokens.Add("(!VATDIVVISIBILITY!)", "hidden");
                    m_DynamicTokens.Add("(!VATDIVDISPLAY!)", "none");
                    m_DynamicTokens.Add("(!VATSELECTLIST!)", String.Empty);
                }

                if (!ThisCustomer.IsRegistered)
                {
                    m_DynamicTokens.Add("(!SUBSCRIPTION_EXPIRATION!)", AppLogic.ro_NotApplicable);
                }
                else
                {
                    if (ThisCustomer.SubscriptionExpiresOn.Equals(System.DateTime.MinValue))
                    {
                        m_DynamicTokens.Add("(!SUBSCRIPTION_EXPIRATION!)", "Expired");
                    }
                    else
                    {
                        m_DynamicTokens.Add("(!SUBSCRIPTION_EXPIRATION!)", Localization.ToThreadCultureShortDateString(ThisCustomer.SubscriptionExpiresOn));
                    }
                }

                m_DynamicTokens.Add("(!PAGEURL!)", HttpContext.Current.Server.UrlEncode(CommonLogic.GetThisPageName(false) + "?" + CommonLogic.ServerVariables("QUERY_STRING")));
                m_DynamicTokens.Add("(!RANDOM!)", CommonLogic.GetRandomNumber(1, 7).ToString());
                m_DynamicTokens.Add("(!HDRID!)", CommonLogic.GetRandomNumber(1, 7).ToString());
                m_DynamicTokens.Add("(!INVOCATION!)", HttpContext.Current.Server.HtmlEncode(CommonLogic.PageInvocation()));
                m_DynamicTokens.Add("(!REFERRER!)", HttpContext.Current.Server.HtmlEncode(CommonLogic.PageReferrer()));

                StringBuilder tmp = new StringBuilder(4096);
                tmp.Append("<!--\n");
                tmp.Append("PAGE INVOCATION: " + HttpContext.Current.Server.HtmlEncode(CommonLogic.PageInvocation()) + "\n");
                tmp.Append("PAGE REFERRER: " + HttpContext.Current.Server.HtmlEncode(CommonLogic.PageReferrer()) + "\n");
                tmp.Append("STORE LOCALE: " + Localization.GetDefaultLocale() + "\n");
                tmp.Append("STORE CURRENCY: " + Localization.GetPrimaryCurrency() + "\n");
                tmp.Append("CUSTOMER ID: " + ThisCustomer.CustomerID.ToString() + "\n");
                tmp.Append("AFFILIATE ID: " + ThisCustomer.AffiliateID.ToString() + "\n");
                tmp.Append("CUSTOMER LOCALE: " + ThisCustomer.LocaleSetting + "\n");
                tmp.Append("CURRENCY SETTING: " + ThisCustomer.CurrencySetting + "\n");
                tmp.Append("CACHE MENUS: " + AppLogic.AppConfigBool("CacheMenus").ToString() + "\n");
                tmp.Append("-->\n");
                m_DynamicTokens.Add("(!PAGEINFO!)", tmp.ToString());

                bool IsRegistered = CommonLogic.IIF(ThisCustomer != null, ThisCustomer.IsRegistered, false);

                String tmpS = String.Empty;

                if (IsRegistered)
                {
                    if (!AppLogic.IsAdminSite)
                    {
                        tmpS = AppLogic.GetString("skinbase.cs.1", SkinID, ThisCustomer.LocaleSetting) + " <a class=\"username\" href=\"account.aspx\">" + ThisCustomer.FullName() + "</a>" + CommonLogic.IIF(ThisCustomer.CustomerLevelID != 0, "&nbsp;(" + ThisCustomer.CustomerLevelName + ")", "");
                    }
                    m_DynamicTokens.Add("(!USER_NAME!)", tmpS);
                    m_DynamicTokens.Add("(!USERNAME!)", tmpS);
                }
                else
                {
                    m_DynamicTokens.Add("(!USER_NAME!)", String.Empty);
                    m_DynamicTokens.Add("(!USERNAME!)", String.Empty);
                }

                m_DynamicTokens.Add("(!USER_MENU_NAME!)", CommonLogic.IIF(!IsRegistered, "my account", ThisCustomer.FullName()));
                m_DynamicTokens.Add("(!USER_MENU!)", AppLogic.GetUserMenu(ThisCustomer.IsRegistered, SkinID, ThisCustomer.LocaleSetting));
                if (AppLogic.MicropayIsEnabled())
                {
                    tmpS = "Your " + AppLogic.GetString("account.aspx.11", SkinID, ThisCustomer.LocaleSetting) + " balance is: " + Localization.DecimalStringForDB(ThisCustomer.MicroPayBalance);
                    m_DynamicTokens.Add("(!MICROPAY_BALANCE!)", tmpS);
                    m_DynamicTokens.Add("(!MICROPAY_BALANCE_RAW!)", Localization.DecimalStringForDB(ThisCustomer.MicroPayBalance));
                    m_DynamicTokens.Add("(!MICROPAY_BALANCE_CURRENCY!)", ThisCustomer.CurrencyString(ThisCustomer.MicroPayBalance));
                }
                tmpS = ShoppingCart.NumItems(ThisCustomer.CustomerID, CartTypeEnum.ShoppingCart).ToString();
                m_DynamicTokens.Add("(!NUM_CART_ITEMS!)", tmpS);
                tmpS = AppLogic.GetString("AppConfig.CartPrompt", SkinID, ThisCustomer.LocaleSetting);
                m_DynamicTokens.Add("(!CARTPROMPT!)", tmpS);
                tmpS = ShoppingCart.NumItems(ThisCustomer.CustomerID, CartTypeEnum.WishCart).ToString();
                m_DynamicTokens.Add("(!NUM_WISH_ITEMS!)", tmpS);
                tmpS = ShoppingCart.NumItems(ThisCustomer.CustomerID, CartTypeEnum.GiftRegistryCart).ToString();
                m_DynamicTokens.Add("(!NUM_GIFT_ITEMS!)", tmpS);
                tmpS = CommonLogic.IIF(!IsRegistered, AppLogic.GetString("skinbase.cs.4", SkinID, ThisCustomer.LocaleSetting), AppLogic.GetString("skinbase.cs.5", SkinID, ThisCustomer.LocaleSetting));
                m_DynamicTokens.Add("(!SIGNINOUT_TEXT!)", tmpS);
                m_DynamicTokens.Add("(!SIGNINOUT_LINK!)", CommonLogic.IIF(!IsRegistered, "signin.aspx", "signout.aspx"));
                String PN = CommonLogic.GetThisPageName(false);
                if (AppLogic.AppConfigBool("ShowMiniCart"))
                {
                    if (PN.StartsWith("shoppingcart", StringComparison.InvariantCultureIgnoreCase) || PN.StartsWith("checkout", StringComparison.InvariantCultureIgnoreCase) || PN.StartsWith("cardinal", StringComparison.InvariantCultureIgnoreCase) || PN.StartsWith("addtocart") || PN.IndexOf("_process", StringComparison.InvariantCultureIgnoreCase) != -1 || PN.StartsWith("lat_", StringComparison.InvariantCultureIgnoreCase))
                    {
                        m_DynamicTokens.Add("(!MINICART!)", String.Empty); // don't show on these pages
                    }
                    else
                    {
                        m_DynamicTokens.Add("(!MINICART!)", ShoppingCart.DisplayMiniCart(ThisCustomer, SkinID, true));
                    }
                    if (PN.StartsWith("shoppingcart", StringComparison.InvariantCultureIgnoreCase) || PN.StartsWith("checkout", StringComparison.InvariantCultureIgnoreCase) || PN.StartsWith("cardinal", StringComparison.InvariantCultureIgnoreCase) || PN.StartsWith("addtocart", StringComparison.InvariantCultureIgnoreCase) || PN.IndexOf("_process", StringComparison.InvariantCultureIgnoreCase) != -1 || PN.StartsWith("lat_", StringComparison.InvariantCultureIgnoreCase))
                    {
                        m_DynamicTokens.Add("(!MINICART_PLAIN!)", String.Empty); // don't show on these pages
                    }
                    else
                    {
                        m_DynamicTokens.Add("(!MINICART_PLAIN!)", ShoppingCart.DisplayMiniCart(ThisCustomer, SkinID, false));
                    }
                }
                m_DynamicTokens.Add("(!CUSTOMERID!)", ThisCustomer.CustomerID.ToString());
            }
        }

        public String ReplaceRegExTokens(String s)
        {
            // PROCESS REGEX TYPE TOKENS HERE:
            // e.g. AppConfig, Param, Topic, XmlPackage, Localize, etc...
            return Regex.Replace(s, RegExString, m_CmdMatchEval, RegexOptions.Compiled);
        }

        public String ReplaceTokens(String s)
        {
            if (s.IndexOf("(!") == -1)
            {
                return s; // no tokens!
            }

            s = ReplacePageStaticTokens(s);
            s = ReplacePageDynamicTokens(s);
            s = ReplaceRegExTokens(s);

            return s;
        }


        /// <summary>
        /// Evaluates (!!) tokens and replaces them with correct command output
        /// </summary>
        protected String CommandMatchEvaluator(Match match)
        {
            string cmd = match.Groups[1].Value; // The command string

            Hashtable parameters = new Hashtable(); //CaseInsensitiveHashCodeProvider();

            for (int i = 0; i < match.Groups[2].Captures.Count; i++)
            {
                string attr = match.Groups[2].Captures[i].Value;
                if (attr == null)
                {
                    attr = String.Empty;
                }

                string val = match.Groups[3].Captures[i].Value;
                if (val == null)
                {
                    val = String.Empty;
                }

                parameters.Add(attr.ToLowerInvariant(), val);
            }
            return DispatchCommand(cmd, parameters);
        }

        /// <summary>
        /// Takes command string and parameters and returns the result string of the command.
        /// </summary>
        protected string DispatchCommand(string command, Hashtable parameters)
        {
            string result = "(!" + command + "!)";
            command = command.ToLowerInvariant().Replace("username", "user_name");
            XSLTExtensions ExtObj = new XSLTExtensions(m_ThisCustomer, m_SkinID);

            switch (command)
            {
                case "obfuscatedemail":
                    {
                        String EMail = CommonLogic.HashtableParam(parameters, "email");
                        //No longer supported.  Just return the email address.
                        result = EMail;
                        break;
                    }
                case "remoteurl": // (!RemoteUrl URL=""!)
                    {
                        String URL = CommonLogic.HashtableParam(parameters, "url");
                        if (URL.Length != 0)
                        {
                            result = ExtObj.RemoteUrl(URL);
                        }
                        break;
                    }
                case "pagingcontrol":
                    {
                        // (!PagingControl BaseURL="" PageNum="N" NumPages="M"!)
                        String BaseURL = CommonLogic.HashtableParam(parameters, "baseurl"); // optional, will use existing QUERY_STRING if not provided
                        int PageNum = CommonLogic.HashtableParamUSInt(parameters, "pagenum"); // optional, can get from QUERY_STRING if not provided
                        int NumPages = CommonLogic.HashtableParamUSInt(parameters, "numpages"); // required
                        result = ExtObj.PagingControl(BaseURL, PageNum.ToString(), NumPages.ToString());
                        break;
                    }
                case "skinid":
                    {
                        // (!SKINID!)
                        result = SkinID.ToString();
                        break;
                    }
                case "customerid":
                    {
                        // (!CUSTOMERID!)
                        if (ThisCustomer != null)
                        {
                            result = ThisCustomer.CustomerID.ToString();
                        }
                        else
                        {
                            result = String.Empty;
                        }
                        break;
                    }
                case "user_name":
                    {
                        result = ExtObj.User_Name();
                        break;
                    }
                case "user_menu_name":
                    {
                        result = ExtObj.User_Menu_Name();
                        break;
                    }
                case "store_version":
                    {
                        // (!STORE_VERSION!)
                        result = String.Empty;
                        break;
                    }
                case "manufacturerlink":
                    {
                        // (!ManufacturerLink ManufacturerID="N" SEName="xxx" IncludeATag="true/false" InnerText="Some Text"!)
                        int ManufacturerID = CommonLogic.HashtableParamUSInt(parameters, "manufacturerid");
                        String SEName = CommonLogic.HashtableParam(parameters, "sename");
                        bool IncludeATag = CommonLogic.HashtableParamBool(parameters, "includeatag");
                        result = ExtObj.ManufacturerLink(ManufacturerID.ToString(), SEName, IncludeATag.ToString());
                        break;
                    }
                case "categorylink":
                    {
                        // (!CategoryLink CategoryID="N" SEName="xxx" IncludeATag="true/false"!)
                        int CategoryID = CommonLogic.HashtableParamUSInt(parameters, "categoryid");
                        String SEName = CommonLogic.HashtableParam(parameters, "sename");
                        bool IncludeATag = CommonLogic.HashtableParamBool(parameters, "includeatag");
                        result = ExtObj.CategoryLink(CategoryID.ToString(), SEName, IncludeATag.ToString());
                        break;
                    }
                case "sectionlink":
                    {
                        // (!SectionLink SectionID="N" SEName="xxx" IncludeATag="true/false"!)
                        int SectionID = CommonLogic.HashtableParamUSInt(parameters, "sectionid");
                        String SEName = CommonLogic.HashtableParam(parameters, "sename");
                        bool IncludeATag = CommonLogic.HashtableParamBool(parameters, "includeatag");
                        result = ExtObj.SectionLink(SectionID.ToString(), SEName, IncludeATag.ToString());
                        break;
                    }
                case "librarylink":
                    {
                        // (!LibraryLink LibraryID="N" SEName="xxx" IncludeATag="true/false"!)
                        int LibraryID = CommonLogic.HashtableParamUSInt(parameters, "libraryid");
                        String SEName = CommonLogic.HashtableParam(parameters, "sename");
                        bool IncludeATag = CommonLogic.HashtableParamBool(parameters, "includeatag");
                        result = ExtObj.LibraryLink(LibraryID.ToString(), SEName, IncludeATag.ToString());
                        break;
                    }
                case "productlink":
                    {
                        // (!ProductLink ProductID="N" SEName="xxx" IncludeATag="true/false"!)
                        int ProductID = CommonLogic.HashtableParamUSInt(parameters, "productid");
                        String SEName = CommonLogic.HashtableParam(parameters, "sename");
                        bool IncludeATag = CommonLogic.HashtableParamBool(parameters, "includeatag");
                        result = ExtObj.ProductLink(ProductID.ToString(), SEName, IncludeATag.ToString());
                        break;
                    }
                case "upsellproducts":
                    {
                        // (!UpsellProducts ProductID="N"!)
                        int ProductID = CommonLogic.HashtableParamUSInt(parameters, "productid");
                        result = ExtObj.ShowUpsellProducts(ProductID.ToString());
                        break;
                    }
                case "relatedproducts":
                    {
                        // (!RelatedProducts ProductID="N"!)
                        int ProductID = CommonLogic.HashtableParamUSInt(parameters, "productid");
                        result = ExtObj.RelatedProducts(ProductID.ToString());
                        break;
                    }
                case "documentlink":
                    {
                        // (!DocumentLink DocumentID="N" SEName="xxx" IncludeATag="true/false"!)
                        int DocumentID = CommonLogic.HashtableParamUSInt(parameters, "documentid");
                        String SEName = CommonLogic.HashtableParam(parameters, "sename");
                        bool IncludeATag = CommonLogic.HashtableParamBool(parameters, "includeatag");
                        result = ExtObj.DocumentLink(DocumentID.ToString(), SEName, IncludeATag.ToString());
                        break;
                    }
                case "productandcategorylink":
                    {
                        // (!ProductAndCategoryLink ProductID="N" CategoryID="M" SEName="xxx" IncludeATag="true/false"!)
                        int ProductID = CommonLogic.HashtableParamUSInt(parameters, "productid");
                        String SEName = CommonLogic.HashtableParam(parameters, "sename");
                        int CategoryID = CommonLogic.HashtableParamUSInt(parameters, "categoryid");
                        bool IncludeATag = CommonLogic.HashtableParamBool(parameters, "includeatag");
                        result = ExtObj.ProductandCategoryLink(ProductID.ToString(), SEName, CategoryID.ToString(), IncludeATag.ToString());
                        break;
                    }
                case "productandsectionlink":
                    {
                        // (!ProductAndSectionLink ProductID="N" SectionID="M" SEName="xxx" IncludeATag="true/false"!)
                        int ProductID = CommonLogic.HashtableParamUSInt(parameters, "productid");
                        String SEName = CommonLogic.HashtableParam(parameters, "sename");
                        int SectionID = CommonLogic.HashtableParamUSInt(parameters, "sectionid");
                        bool IncludeATag = CommonLogic.HashtableParamBool(parameters, "includeatag");
                        result = ExtObj.ProductandSectionLink(ProductID.ToString(), SEName, SectionID.ToString(), IncludeATag.ToString());
                        break;
                    }
                case "productandmanufacturerlink":
                    {
                        // (!ProductAndManufacturerLink ProductID="N" ManufacturerID="M" SEName="xxx" IncludeATag="true/false"!)
                        int ProductID = CommonLogic.HashtableParamUSInt(parameters, "productid");
                        String SEName = CommonLogic.HashtableParam(parameters, "sename");
                        int ManufacturerID = CommonLogic.HashtableParamUSInt(parameters, "manufacturerid");
                        bool IncludeATag = CommonLogic.HashtableParamBool(parameters, "includeatag");
                        result = ExtObj.ProductandManufacturerLink(ProductID.ToString(), SEName, ManufacturerID.ToString(), IncludeATag.ToString());
                        break;
                    }
                case "productpropername":
                    {
                        // (!ProductProperName ProductID="N" VariantID="M"!)
                        int ProductID = CommonLogic.HashtableParamUSInt(parameters, "productid");
                        int VariantID = CommonLogic.HashtableParamUSInt(parameters, "variantid");
                        result = ExtObj.ProductProperName(ProductID.ToString(), VariantID.ToString());
                        break;
                    }
                case "documentandlibrarylink":
                    {
                        // (!DocumentAndLibraryLink DocumentID="N" LibraryID="M" SEName="xxx" IncludeATag="true/false"!)
                        int DocumentID = CommonLogic.HashtableParamUSInt(parameters, "documentid");
                        String SEName = CommonLogic.HashtableParam(parameters, "sename");
                        int LibraryID = CommonLogic.HashtableParamUSInt(parameters, "libraryid");
                        bool IncludeATag = CommonLogic.HashtableParamBool(parameters, "includeatag");
                        result = ExtObj.DocumentandLibraryLink(DocumentID.ToString(), SEName, LibraryID.ToString(), IncludeATag.ToString());
                        break;
                    }
                case "entitylink":
                    {
                        // (!EntityLink EntityID="N" EntityName="xxx" SEName="xxx" IncludeATag="true/false"!)
                        int EntityID = CommonLogic.HashtableParamUSInt(parameters, "entityid");
                        String SEName = CommonLogic.HashtableParam(parameters, "sename");
                        String EntityName = CommonLogic.HashtableParam(parameters, "entityname");
                        bool IncludeATag = CommonLogic.HashtableParamBool(parameters, "includeatag");
                        result = ExtObj.EntityLink(EntityID.ToString(), SEName, EntityName, IncludeATag.ToString());
                        break;
                    }
                case "objectlink":
                    {
                        // (!ObjectLink ObjectID="N" ObjectName="xxx" SEName="xxx" IncludeATag="true/false"!)
                        int ObjectID = CommonLogic.HashtableParamUSInt(parameters, "objectid");
                        String SEName = CommonLogic.HashtableParam(parameters, "sename");
                        String ObjectName = CommonLogic.HashtableParam(parameters, "objectname");
                        bool IncludeATag = CommonLogic.HashtableParamBool(parameters, "includeatag");
                        result = ExtObj.ObjectLink(ObjectID.ToString(), SEName, ObjectName, IncludeATag.ToString());
                        break;
                    }
                case "productandentitylink":
                    {
                        // (!ProductAndEntityLink ProductID="N" EntityID="M" EntityName="xxx" SEName="xxx" IncludeATag="true/false"!)
                        int ProductID = CommonLogic.HashtableParamUSInt(parameters, "productid");
                        String SEName = CommonLogic.HashtableParam(parameters, "sename");
                        int EntityID = CommonLogic.HashtableParamUSInt(parameters, "entityid");
                        String EntityName = CommonLogic.HashtableParam(parameters, "entityname");
                        bool IncludeATag = CommonLogic.HashtableParamBool(parameters, "includeatag");
                        String InnerText = CommonLogic.HashtableParam(parameters, "innertext");
                        result = ExtObj.ProductandEntityLink(ProductID.ToString(), SEName, EntityID.ToString(), EntityName, IncludeATag.ToString());
                        break;
                    }
                case "topic":
                    {
                        // (!Topic TopicID="M"!) or (!Topic ID="M"!) or (!Topic Name="xxx"!)
                        int TopicID = CommonLogic.HashtableParamUSInt(parameters, "id");
                        if (TopicID == 0)
                        {
                            TopicID = CommonLogic.HashtableParamUSInt(parameters, "topicid");
                        }
                        String LS = Localization.GetDefaultLocale();
                        if (ThisCustomer != null)
                        {
                            LS = ThisCustomer.LocaleSetting;
                        }
                        if (TopicID != 0)
                        {
                          
                            Topic t = new Topic(TopicID, LS, SkinID, null);
                            result = t.Contents;
                        }
                        else
                        {
                            String TopicName = CommonLogic.HashtableParam(parameters, "name");
                            if (TopicName.Length != 0)
                            {
                                
                                Topic t = new Topic(TopicName, LS, SkinID, null);
                                result = t.Contents;
                            }
                        }
                        break;
                    }
                case "appconfig":
                    {
                        // (!AppConfig Name="xxx"!)
                        String AppConfigName = CommonLogic.HashtableParam(parameters, "name");
                        result = ExtObj.AppConfig(AppConfigName);
                        break;
                    }
                case "stringresource":
                    {
                        // (!StringResource Name="xxx"!)
                        String StringResourceName = CommonLogic.HashtableParam(parameters, "name");
                        result = ExtObj.StringResource(StringResourceName);
                        break;
                    }
                case "getstring":
                    {
                        // (!GetString Name="xxx"!)
                        String StringResourceName = CommonLogic.HashtableParam(parameters, "name");
                        result = ExtObj.StringResource(StringResourceName);
                        break;
                    }
                case "loginoutprompt":
                    {
                        // (!LoginOutPrompt!)
                        result = AppLogic.GetLoginBox(SkinID);
                        break;
                    }

                case "searchbox":
                    {
                        // (!SearchBox!)
                        result = ExtObj.SearchBox();
                        break;
                    }

                case "helpbox":
                    {
                        // (!HelpBox!)
                        result = ExtObj.HelpBox();
                        break;
                    }
                case "addtocartform":
                    {
                     
                        int ProductID = CommonLogic.HashtableParamUSInt(parameters, "productid");
                        int VariantID = CommonLogic.HashtableParamUSInt(parameters, "variantid");
                        bool ColorChangeProductImage = CommonLogic.HashtableParamBool(parameters, "colorchangeproductimage");
                        result = ExtObj.AddtoCartForm(ProductID.ToString(), VariantID.ToString(), ColorChangeProductImage.ToString());
                        break;
                    }
                case "lookupimage":
                    {
                       
                        int ID = CommonLogic.HashtableParamUSInt(parameters, "id");
                        String EntityOrObjectName = CommonLogic.HashtableParam(parameters, "type");
                        String DesiredSize = CommonLogic.HashtableParam(parameters, "size");
                        bool IncludeATag = CommonLogic.HashtableParamBool(parameters, "includeatag");
                        result = ExtObj.LookupImage(ID.ToString(), EntityOrObjectName, DesiredSize, IncludeATag.ToString());
                        break;
                    }
                case "productnavlinks":
                    {
                        
                        int ProductID = CommonLogic.HashtableParamUSInt(parameters, "productid");
                        int CategoryID = CommonLogic.QueryStringUSInt("CategoryID"); // should really get them from parameters, NOT from the querystring, but whatever...
                        int SectionID = CommonLogic.QueryStringUSInt("SectionID"); // should really get them from parameters, NOT from the querystring, but whatever...
                        bool UseGraphics = CommonLogic.HashtableParamBool(parameters, "usegraphics");
                        result = ExtObj.ProductNavLinks(ProductID.ToString(), CategoryID.ToString(), SectionID.ToString(), UseGraphics.ToString());
                        break;
                    }
                case "emailproducttofriend":
                    {
                      
                        int ProductID = CommonLogic.HashtableParamUSInt(parameters, "productid");
                        int CategoryID = CommonLogic.HashtableParamUSInt(parameters, "categoryid");
                        result = ExtObj.EmailProductToFriend(ProductID.ToString(), CategoryID.ToString());
                        break;
                    }
                case "productdescriptionfile":
                    {
                       
                        int ProductID = CommonLogic.HashtableParamUSInt(parameters, "productid");
                        bool IncludeBRBefore = CommonLogic.HashtableParamBool(parameters, "includebrbefore");
                        result = ExtObj.ProductDescriptionFile(ProductID.ToString(), IncludeBRBefore.ToString());
                        break;
                    }
                case "productspecs":
                    {
                       
                        int ProductID = CommonLogic.HashtableParamUSInt(parameters, "productid");
                        bool IncludeBRBefore = CommonLogic.HashtableParamBool(parameters, "includebrbefore");
                        result = ExtObj.ProductSpecs(ProductID.ToString(), IncludeBRBefore.ToString());
                        break;
                    }
                case "productratings":
                    {
                       
                        int ProductID = CommonLogic.HashtableParamUSInt(parameters, "productid");
                        int CategoryID = CommonLogic.QueryStringUSInt("CategoryID"); // should really get them from parameters, NOT from the querystring, but whatever...
                        int SectionID = CommonLogic.QueryStringUSInt("SectionID"); // should really get them from parameters, NOT from the querystring, but whatever...
                        int ManufacturerID = CommonLogic.QueryStringUSInt("ManufacturerID"); // should really get them from parameters, NOT from the querystring, but whatever...
                        bool IncludeBRBefore = CommonLogic.HashtableParamBool(parameters, "includebrbefore");
                        result = ExtObj.ProductRatings(ProductID.ToString(), CategoryID.ToString(), SectionID.ToString(), ManufacturerID.ToString(), IncludeBRBefore.ToString());
                        break;
                    }
                case "formatcurrency":
                    {
                      
                        decimal CurrencyValue = CommonLogic.HashtableParamNativeDecimal(parameters, "value");
                        String LocaleSetting = CommonLogic.HashtableParam(parameters, "localesetting");
                        result = ExtObj.FormatCurrency(CurrencyValue.ToString());
                        break;
                    }
                case "getspecialsboxexpandedrandom":
                    {
                      
                        int CategoryID = CommonLogic.HashtableParamUSInt(parameters, "categoryid");
                        bool ShowPics = CommonLogic.HashtableParamBool(parameters, "showpics");
                        bool IncludeFrame = CommonLogic.HashtableParamBool(parameters, "includeframe");
                        String Teaser = CommonLogic.HashtableParam(parameters, "teaser");
                        result = ExtObj.GetSpecialsBoxExpandedRandom(CategoryID.ToString(), ShowPics.ToString(), IncludeFrame.ToString(), Teaser);
                        break;
                    }
                case "getspecialsboxexpanded":
                    {
                       
                        int CategoryID = CommonLogic.HashtableParamUSInt(parameters, "categoryid");
                        int ShowNum = CommonLogic.HashtableParamUSInt(parameters, "shownum");
                        bool ShowPics = CommonLogic.HashtableParamBool(parameters, "showpics");
                        bool IncludeFrame = CommonLogic.HashtableParamBool(parameters, "includeframe");
                        String Teaser = CommonLogic.HashtableParam(parameters, "teaser");
                        result = ExtObj.GetSpecialsBoxExpanded(CategoryID.ToString(), ShowNum.ToString(), ShowPics.ToString(), IncludeFrame.ToString(), Teaser);
                        break;
                    }
                case "getnewsboxexpanded":
                    {
                       
                        bool ShowCopy = CommonLogic.HashtableParamBool(parameters, "showcopy");
                        int ShowNum = CommonLogic.HashtableParamUSInt(parameters, "shownum");
                        bool IncludeFrame = CommonLogic.HashtableParamBool(parameters, "includeframe");
                        String Teaser = CommonLogic.HashtableParam(parameters, "teaser");
                        result = ExtObj.GetNewsBoxExpanded(ShowCopy.ToString(), ShowNum.ToString(), IncludeFrame.ToString(), Teaser);
                        break;
                    }
                case "xmlpackage":
                    {
                        // (!XmlPackage Name="xxx" version="N"!)
                        // version can only be 2 at this time, or blank
                        String PackageName = CommonLogic.HashtableParam(parameters, "name");
                        String VersionID = CommonLogic.HashtableParam(parameters, "version"); // optional
                        Hashtable userruntimeparams = parameters;
                        userruntimeparams.Remove("name");
                        userruntimeparams.Remove("version");
                        string runtimeparams = String.Empty;
                        foreach (DictionaryEntry de in userruntimeparams)
                        {
                            runtimeparams += de.Key.ToString() + "=" + de.Value.ToString() + "&";
                        }
                        if (runtimeparams.Length > 0)
                        {
                            runtimeparams = runtimeparams.Substring(0, runtimeparams.Length - 1);
                        }
                      
                        if (PackageName.Length != 0)
                        {
                            if (PackageName.EndsWith(".xslt", StringComparison.InvariantCultureIgnoreCase) && VersionID != "2")
                            {
                                throw new ArgumentException("Version 1 XmlPackages are no longer supported!");
                            }
                            else
                            {
                                // WARNING YOU COULD CAUSE ENDLESS RECURSION HERE! if your XmlPackage refers to itself in some direct, or INDIRECT! way!!
                                result = AppLogic.RunXmlPackage(PackageName, this, ThisCustomer, SkinID, String.Empty, runtimeparams, true, true);
                            }
                        }
                        break;
                    }
            }
            return result;
        }

        public int SkinID
        {
            get
            {
                return m_SkinID;
            }
            set
            {
                m_SkinID = value;
            }
        }

        public Customer ThisCustomer
        {
            get
            {
                return m_ThisCustomer;
            }
            set
            {
                m_ThisCustomer = value;
            }
        }

    }
}
