// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Compilation;
using System.Web.UI;
using System.Web.UI.Design;

namespace AspDotNetStorefrontCore
{
    [ExpressionPrefix("Tokens"), ExpressionEditor(typeof(TokenEditor))]
    public class Tokens : ExpressionBuilder
    {
        /// <summary>
        /// Gets the eval data.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="target">The target.</param>
        /// <param name="entry">The entry.</param>
        /// <returns></returns>
        public static object GetEvalData(string expression, Type target, string entry)
        {
            Customer ThisCustomer = AppLogic.GetCurrentCustomer();
            bool IsRegistered = CommonLogic.IIF(ThisCustomer != null, ThisCustomer.IsRegistered, false);

            string[] values = expression.Split(',');
            string command = values[0];
            switch (command.ToLowerInvariant())
            {
             
                case "cartprompt":
                    return AppLogic.GetString("AppConfig.CartPrompt", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
               
                case "currency_locale_robots_tag":
                    return CurrencyLocaleRobotsTag(ThisCustomer);
                    
                case "num_cart_items":
                    return NumCartItemsParser(ThisCustomer);
                    
                case "pageinfo":
                    return PageInfoParser(ThisCustomer);

                case "buysafeseal":
                    return BuySafeSealParser(ThisCustomer);

                case "bongoextend":
                    return BongoExtendParser(ThisCustomer);
                    
                case "username":
                    return UserNameParser(ThisCustomer);                    
                
                case "signinout_link":
                    return SignInOutLinkParser(ThisCustomer);
                    
                case "signinout_text":
                    return SignInOutTextParser(ThisCustomer);
                    
                case "skinid":
                    return ThisCustomer.SkinID.ToString();

                case "stringresource":
                    return StringResourceParser(ThisCustomer, values);
                    
                case "stringresourceformat":
                    return StringResourceFormatParser(ThisCustomer, values);                    

                case "appconfig":
                    return AppConfigParser(values);
                    
                case "appconfigbool":
                    return AppConfigBoolParser(values);
                    
                case "appconfigusint":
                    return AppConfigUSIntParser(values);
                    
                case "topic":
                    return TopicParser(ThisCustomer, values);

                case "topictitle":
                    return Topic.GetTitle(values[1].Trim(), ThisCustomer.LocaleSetting, AppLogic.StoreID());

                case "topiclink":
                    return SE.MakeDriverLink(values[1].Trim());
                    
                case "xmlpackage":
                    if (values.Length >= 2)
                    {
                        string xmlPackageName = values[1];
                        string runtimeParams = values.Length >= 3 ? values[2] : string.Empty;

                        return RunXmlPackage(ThisCustomer, xmlPackageName.Trim(), runtimeParams.Trim());
                    }
                    else
                    {
                        return "Invalid number of parameters";
                    }


                case "stringformat":
                    return StringFormat(expression);                
                    
                case "user_menu_name":                    
                    return CommonLogic.IIF(!IsRegistered, "my account", ThisCustomer.FullName());

                case "customerid":
                    return ThisCustomer.CustomerID.ToString();
                    
                case "skinimagedir":
                    return AppLogic.SkinImageDir();
                    
                case "skinimage":
                    if (values.Length < 2)
                    {
                        return "{Image File name not specified}";
                    }
                    string imgFile = values[1];
                    return AppLogic.SkinImage(imgFile.Trim());

                case "adminlink":
                    return AppLogic.AdminLinkUrl(values[1].Trim());

                case "google_ecom_tracking_v2":
					if (AppLogic.AppConfigBool("Google.DeprecatedEcomTokens.Enabled"))
					{
						if (CommonLogic.GetThisPageName(false).ToLowerInvariant().StartsWith("orderconfirmation.aspx"))
						{
							return String.Empty;
						}
						else
						{
							return AppLogic.GetGoogleEComTrackingV2(ThisCustomer, false);
						}
					}
					else
					{
						return String.Empty;
					}
                case "google_ecom_tracking_asynch":
					if (AppLogic.AppConfigBool("Google.DeprecatedEcomTokens.Enabled"))
					{
						if (CommonLogic.GetThisPageName(false).ToLowerInvariant().StartsWith("orderconfirmation.aspx") ||
										CommonLogic.GetThisPageName(false).ToLowerInvariant().StartsWith("mobileorderconfirmation.aspx"))
						{
							return AppLogic.GetGoogleEComTrackingAsynch(ThisCustomer, true);
						}
						else
						{
							return AppLogic.GetGoogleEComTrackingAsynch(ThisCustomer, false);
						}
					}
					else
					{
						return String.Empty;
					}
                case "vbv":
                    return VBVParser(ThisCustomer);
            
                default:
                    return string.Empty;
            }
        }

        private static string RunXmlPackage(Customer ThisCustomer, string xmlPackageName, string additionalRuntimeParams)
        {
            Parser p = new Parser(ThisCustomer.SkinID, ThisCustomer);
            
            string output = string.Empty;
            using (XmlPackage2 xp = new XmlPackage2(xmlPackageName, ThisCustomer, ThisCustomer.SkinID, "", additionalRuntimeParams, String.Empty, true))
            {
                output = AppLogic.RunXmlPackage(xp, p, ThisCustomer, ThisCustomer.SkinID, true, true);
            }

            return output;
        }

        private static string StringFormat(string expression)
        {
            string[] entries = expression.Split(',');

            // the 1st entry is the token command "StringFormat"
            // the 2nd entry is the format string
            string formatString = entries[1];

            // individual contents could be anything
            List<string> args = entries.Skip(2).ToList();

            return string.Format(formatString, args.ToArray());
        }
      

        private static string VatSelectListParser(Customer ThisCustomer)
        {
            string tmp = "";
            if (AppLogic.VATIsEnabled() && AppLogic.AppConfigBool("VAT.AllowCustomerToChooseSetting"))
            {
                tmp = AppLogic.GetVATSelectList(ThisCustomer);
            }
            return tmp;
        }

        private static string CurrencySelectListParser(Customer ThisCustomer)
        {
            string tmp = "";
            if (AppLogic.NumLocaleSettingsInstalled() > 1)
            {
                tmp = AppLogic.GetCurrencySelectList(ThisCustomer);
            }
            return tmp;
        }

        private static string CountrySelectListParser(Customer ThisCustomer)
        {
            string tmp = "";
            if (Currency.NumPublishedCurrencies() > 1)
            {
                tmp = AppLogic.GetCountrySelectList(ThisCustomer.LocaleSetting);
            }
            return tmp;
        }

        private static string SignInOutLinkParser(Customer ThisCustomer)
        {
            return AppLogic.ResolveUrl(CommonLogic.IIF(!ThisCustomer.IsRegistered, "~/signin.aspx", "~/signout.aspx"));
        }

        private static string SignInOutTextParser(Customer ThisCustomer)
        {
            return CommonLogic.IIF(!ThisCustomer.IsRegistered, AppLogic.GetString("skinbase.cs.4", ThisCustomer.SkinID, ThisCustomer.LocaleSetting), AppLogic.GetString("skinbase.cs.5", ThisCustomer.SkinID, ThisCustomer.LocaleSetting));
        }


        private static int AppConfigUSIntParser(string[] values)
        {
            if (values.Length >= 2)
            {
                string appConfigName = values[1].Trim();
                return AppLogic.AppConfigUSInt(appConfigName);
            }
            else
            {
                throw new InvalidOperationException("AppConfig Name not specified!!!");
            }
        }

        private static string SectionTitleParser()
        {
            return "";
        }

        private static string UserNameParser(Customer ThisCustomer)
        {

            if (ThisCustomer.IsRegistered && !AppLogic.IsAdminSite)
            {
                return string.Format(
                    "{0}<a class='username' href='{1}'>{2}</a>{3}",
                    new object[]{
                        "skinbase.cs.1".StringResource(),
                        AppLogic.ResolveUrl("~/account.aspx"),
                        ThisCustomer.FullName(),
                        ThisCustomer.CustomerLevelID != 0 ?
                            "&nbsp;(" + ThisCustomer.CustomerLevelName + ")":
                            ""
                    });
            }
            else
            {
                return string.Empty;
            }
        }

        private static string TopicParser(Customer ThisCustomer, string[] values)
        {
            Parser p = null;

            if (values.Length > 2 && values[2].EqualsIgnoreCase("true"))
            {
                p = new Parser(ThisCustomer.SkinID, ThisCustomer); 
            }

            String LS = Localization.GetDefaultLocale(), tmp = "";

            if (ThisCustomer != null)
            {
                LS = ThisCustomer.LocaleSetting;
            }

            if (values[1].Length != 0)
            {
                Topic t = new Topic(values[1], LS, ThisCustomer.SkinID, p);
                tmp = t.Contents;
            }

            return tmp;
        }

        private static string PageInfoParser(Customer ThisCustomer)
        {
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

            return tmp.ToString();
        }

        private static string BongoExtendParser(Customer ThisCustomer)
        {
            if (AppLogic.AppConfigBool("Bongo.Extend.Enabled") && AppLogic.AppConfig("Bongo.Extend.Script").Length > 0)
                return "<script type=\"text/javascript\" src=\"{0}\"></script>".FormatWith(AppLogic.AppConfig("Bongo.Extend.Script"));
            return String.Empty;
        }

        private static string BuySafeSealParser(Customer ThisCustomer)
        {
            StringBuilder tmp = new StringBuilder(4096);
            if (AppLogic.GlobalConfigBool("BuySafe.Enabled") && AppLogic.GlobalConfig("BuySafe.Hash").Length != 0)
            {
                tmp.AppendLine("<!-- BEGIN: buySAFE Guarantee Seal -->");
                tmp.AppendLine("<script src=\"" + AppLogic.GlobalConfig("BuySafe.RollOverJSLocation") + "\"></script>");
                tmp.AppendLine("<span id=\"BuySafeSealSpan\"></span>");
                tmp.AppendLine("<script type=\"text/javascript\">");
                tmp.AppendLine("    buySAFE.Hash = '" + AppLogic.GlobalConfig("BuySafe.Hash") + "';");
                tmp.AppendLine("    WriteBuySafeSeal(\"BuySafeSealSpan\", \"GuaranteedSeal\");");
                tmp.AppendLine("</script>");
                tmp.AppendLine("<!-- END: buySAFE Guarantee Seal -->");
            }
            return tmp.ToString();
        }


        private static object VatDivVisibility()
        {
            return AppLogic.VATIsEnabled() && AppLogic.AppConfigBool("VAT.AllowCustomerToChooseSetting");
        }

        private static bool CurrencyDivVisibility()
        {
            return Currency.NumPublishedCurrencies() > 1;
        }

        private static bool CountryDivVisibilityParser()
        {
            return AppLogic.NumLocaleSettingsInstalled() > 1;
        }

        private static string CurrencyLocaleRobotsTag(Customer ThisCustomer)
        {
            string tmp = string.Empty;

            if (ThisCustomer.CurrencySetting != Localization.GetPrimaryCurrency()
                && ThisCustomer.LocaleSetting != Localization.GetDefaultLocale())
            {
                tmp = "<meta name=\"robots\" content=\"noindex,nofollow,noarchive\">";
            }

            return tmp;
        }

        private static string NumCartItemsParser(Customer ThisCustomer)
        {
            return ShoppingCart.NumItems(ThisCustomer.CustomerID, CartTypeEnum.ShoppingCart).ToString();
        }

        private static object AppConfigBoolParser(string[] values)
        {
            if (values.Length >= 2)
            {
                string appConfigName = values[1].Trim();
                return AppLogic.AppConfigBool(appConfigName);
            }
            else
            {
                throw new InvalidOperationException("AppConfig Name not specified!!!");
            }
        }

        private static object AppConfigParser(string[] values)
        {
            if (values.Length >= 2)
            {
                string appConfigName = values[1].Trim();
                return AppLogic.AppConfig(appConfigName);
            }
            else
            {
                throw new InvalidOperationException("AppConfig Name not specified!!!");
            }
        }

        private static string StringResourceParser(Customer ThisCustomer, string[] values)
        {
            if (values.Length >= 2 && ThisCustomer != null)
            {
                string key = values[1].Trim();
                return AppLogic.GetString(key, ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
            }
            else
            {
                throw new InvalidOperationException("String Resource Key not specified!!!");
            }
        }

        private static string StringResourceFormatParser(Customer ThisCustomer, string[] values)
        {
            if (values.Length >= 2 && ThisCustomer != null)
            {
                string primaryKey = values[1].Trim();
                int idx = 2; // start indext 
                List<string> otherKeys = new List<string>();
                for (; idx < values.Length; idx++)
                {
                    string otherKey = values[idx].Trim();
                    string keyStringValue = AppLogic.GetString(otherKey, ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                    otherKeys.Add(keyStringValue);
                }

                string primaryKeyValue = AppLogic.GetString(primaryKey, ThisCustomer.SkinID, ThisCustomer.LocaleSetting);

                return string.Format(primaryKeyValue, otherKeys.ToArray());
            }
            else
            {
                throw new InvalidOperationException("String Resource Key not specified!!!");
            }
        }

        private static string MetaDescriptionParser()
        {
            return AppLogic.AppConfig("SE_MetaDescription");
        }

        private static string MetaKeywordsParser()
        {
            return AppLogic.AppConfig("SE_MetaKeywords");
        }

        private static string MetaTitleParser()
        {
            return AppLogic.AppConfig("SE_MetaTitle");
        }

        private static string VBVParser(Customer ThisCustomer)
        {
            string tmp = string.Empty;

            if (AppLogic.AppConfigBool("CardinalCommerce.Centinel.Enabled"))
            {
                tmp = "<img src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + ThisCustomer.SkinID.ToString() + "/images/vbv.jpg") + "\" border=\"0\" alt=\"" + AppLogic.GetString("skintoken.vbv", ThisCustomer.LocaleSetting) + "\">";
            }

            return tmp;
        }


        public override object EvaluateExpression(object target, BoundPropertyEntry entry, object parsedData, ExpressionBuilderContext context)
        {
            return GetEvalData(entry.Expression, target.GetType(), entry.Name);
        }

        public override CodeExpression GetCodeExpression(BoundPropertyEntry entry, object parsedData, ExpressionBuilderContext context)
        {
            string[] args = parsedData.ToString().Split(',');
            if (args[0].StartsWith("invoke", StringComparison.OrdinalIgnoreCase))
            {
                string snippet = string.Empty;
                for (int ctr = 1; ctr < args.Length; ctr++)
                {
                    if (ctr > 1)
                    {
                        snippet += ",";
                    }
                    snippet += args[ctr];
                }
                return new CodeSnippetExpression(snippet);
            }
            else
            {
                Type declaringClassType = entry.DeclaringType;
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(declaringClassType)[entry.PropertyInfo.Name];
                CodeExpression[] expressionArray = new CodeExpression[] {
                new CodePrimitiveExpression(entry.Expression.Trim()),
                new CodeTypeOfExpression(declaringClassType),
                new CodePrimitiveExpression(entry.Name) };

                return new CodeCastExpression(descriptor.PropertyType, new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(base.GetType()), "GetEvalData", expressionArray));
            }
        }

        public override object ParseExpression(string expression, Type propertyType, ExpressionBuilderContext context)
        {
            return expression;
        }

        public override bool SupportsEvaluate
        {
            get { return true; }
        }
    }

    public sealed class TokenEditor : ExpressionEditor
    {
        public override object EvaluateExpression(string expression, object parseTimeData, Type propertyType, IServiceProvider serviceProvider)
        {
            string[] parts = expression.Split(',');

            switch (parts[0].Trim())
            {                
                default:
                    return "[" + parts[0].Trim() + "]";
            }
        }

        public override ExpressionEditorSheet GetExpressionEditorSheet(string expression, IServiceProvider serviceProvider)
        {
            return new TokenEditorSheet(expression, serviceProvider);
        }
    }


    public sealed class TokenEditorSheet : ExpressionEditorSheet
    {
        public TokenEditorSheet(string expression, IServiceProvider provider)
            : base(provider)
        {
            string[] parts = expression.Split(',');
            Token = parts[0].Trim();
            if (parts.GetUpperBound(0) > 0) Value = parts[1].Trim();
        }

        private string m_token;
        [Description("The name of a Parser Token"), TypeConverter(typeof(TokenConverter))]
        public string Token 
        {
            get { return m_token; }
            set { m_token = value; }
        }

        private string m_value;
        [Description("An optional value for the Parser Token")]
        public string Value
        {
            get { return m_value; }
            set { m_value = value; }
        }

        public override string GetExpression()
        {
            if (Value == null || Value.Length == 0) return Token;

            return Token + ", " + Value;
        }

        public class TokenConverter : StringConverter
        {
            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
            {
                return true;
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }

            public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                List<string> tokenList = new List<string>();

                tokenList.Add("Currency_Locale_Robots_Tag");
                tokenList.Add("MetaTitle");
                tokenList.Add("MetaDescription");
                tokenList.Add("MetaKeywords");
                tokenList.Add("PageInfo");
                tokenList.Add("BuySafeSeal");
                tokenList.Add("XmlPackage");
                tokenList.Add("StringResource");
                tokenList.Add("AppConfig");
                tokenList.Add("AppConfigBool");
                tokenList.Add("AppConfigUSInt");
                tokenList.Add("AppConfigNativeInt");
                tokenList.Add("AppConfigNativeDecimal");
                tokenList.Add("SignInOut_Link");
                tokenList.Add("SignInOut_Text");
                tokenList.Add("CountrySelectList");
                tokenList.Add("CountryDivVisibility");
                tokenList.Add("CurrencySelectList");
                tokenList.Add("CurrencyDivVisibility");
                tokenList.Add("VatSelectList");
                tokenList.Add("VatDivVisibility");
                tokenList.Add("UserName");
                tokenList.Add("Num_Cart_Items");
                tokenList.Add("VBV");

                return new StandardValuesCollection(tokenList.ToArray());
            }
        }
    }
}
