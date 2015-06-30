// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml;
using System.IO;
using System.Xml.XPath;
using System.Text;
using AspDotNetStorefrontCore;
using Vortx.Data.Config;
using Vortx.MobileFramework;

namespace Vortx.MobileFramework
{
    public class MobileXSLTExtensionBase
    {
        /// <summary>
        /// Clips text to given length, stopping at nearest space character.
        /// </summary>
        /// <returns>Clipped text</returns>
        public static string TextClip(string text, int length)
        {
            return Vortx.MobileFramework.Utils.TextUtil.ClipText(text, length);
        }

        /// <summary>
        /// Clips text to given length, stopping at nearest given character.
        /// </summary>
        /// <returns>Clipped text</returns>
        public static string TextClip(string text, int length, string nearestChar)
        {
            if (nearestChar.Length == 0)
                return text;

            return Vortx.MobileFramework.Utils.TextUtil.ClipText(text, length, nearestChar[0]);
        }


        #region ProductImageCollection Methods

        public static XPathNodeIterator ProductImageCollectionXML(int ProductID, string ImageFileNameOverride, string SKU, string colors)
        {
            ProductImageCollection pic = new ProductImageCollection(ProductID, ImageFileNameOverride, SKU, 1, "en-US", colors);
            XmlDocument doc = pic.GetXMLBySize();
            XPathNavigator nav = doc.CreateNavigator();
            XPathNodeIterator ret = nav.Select(".");

            return ret;
        }
        public static String ProductImageCollectionString(int ProductID, string ImageFileNameOverride, string SKU, string colors)
        {
            ProductImageCollection pic = new ProductImageCollection(ProductID, ImageFileNameOverride, SKU, 1, "en-US", colors);
            XmlDocument doc = pic.GetXMLBySize();
            return doc.InnerXml;
        }

        public static XPathNodeIterator StringToNode(string s)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(s);
            XPathNavigator nav = doc.CreateNavigator();
            XPathNodeIterator ret = nav.Select(".");
            return ret;
        }

        #endregion

        public static XPathNodeIterator GetCheckoutHeaderLinks()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<CheckoutSteps>");

            AddCheckoutStep(sb, "Shopping&lt;br/&gt;Cart", "shoppingcart.aspx", "shopping");
            AddCheckoutStep(sb, "Address&lt;br/&gt;Book", "account.aspx?checkout=true", "address");
            AddCheckoutStep(sb, "Shipping&lt;br/&gt;Options", "checkoutshipping.aspx?dontupdateid=true", "shipping");
            AddCheckoutStep(sb, "Payment&lt;br/&gt;Info", "mobilecheckoutpayment.aspx", "payment");
            AddCheckoutStep(sb, "Order&lt;br/&gt;Review", "checkoutreview.aspx?paymentmethod=CREDIT CARD", "review");

            sb.Append("</CheckoutSteps>");

            return StringToNode(sb.ToString());
        }

        private static void AddCheckoutStep(StringBuilder sb, String name, string link, string key)
        {
            sb.Append("<CheckoutStep>");
            sb.Append("<Name>");
            sb.Append(name);
            sb.Append("</Name>");
            sb.Append("<Link>");
            sb.Append(link);
            sb.Append("</Link>");
            sb.Append("<Key>");
            sb.Append(key);
            sb.Append("</Key>");
            sb.Append("</CheckoutStep>");
        }

        public string ShowQuantityDiscountTableForAccordion(String sProductID, string HeaderText, string ID) //our version to allow display inline without changing app config "ShowQuantityDiscountTablesInline"
        {
            Customer ThisCustomer = null;

            try
            {
                ThisCustomer = ((AspDotNetStorefrontPrincipal)HttpContext.Current.User).ThisCustomer;
            }
            catch
            {
            }
            if (ThisCustomer == null)
            {
                ThisCustomer = new Customer(true);
            }

            InputValidator IV = new InputValidator("ShowQuantityDiscountTable");
            int ProductID = IV.ValidateInt("ProductID", sProductID);
            StringBuilder results = new StringBuilder("");
            bool CustomerLevelAllowsQuantityDiscounts = QuantityDiscount.CustomerLevelAllowsQuantityDiscounts(ThisCustomer.CustomerLevelID);

            String MainProductSKU = String.Empty;
            int ActiveDIDID = QuantityDiscount.LookupProductQuantityDiscountID(ProductID);
            bool ActiveDID = (ActiveDIDID != 0);
            if (!CustomerLevelAllowsQuantityDiscounts)
            {
                ActiveDID = false;
            }
            if (ActiveDID)
            {
                results.Append("<div data-role=\"collapsible\">");
                results.Append("<h3>" + HeaderText + "</h3>");
                results.Append("<div>" + QuantityDiscount.GetQuantityDiscountDisplayTable(ActiveDIDID, ThisCustomer.SkinID) + "</div>");
                results.Append("</div>");
            }
            return results.ToString();
        }

        public static string GetCheckoutHeader(string currentstep)
        { 
            string ret = "";

            Customer ThisCustomer = null;

            try
            {
                ThisCustomer = ((AspDotNetStorefrontPrincipal)HttpContext.Current.User).ThisCustomer;
            }
            catch
            {
            }
            if (ThisCustomer == null)
            {
                ThisCustomer = new Customer(true);
            }

            if (ThisCustomer != null)
            {
                using (XmlPackage2 m_P = new XmlPackage2("mobile.checkoutheader.xml.config", ThisCustomer, Data.Config.MobilePlatform.SkinId, String.Empty, "active=" + currentstep, String.Empty, true))
                {
                    try
                    {
                        ret = AppLogic.RunXmlPackage(m_P, null, ThisCustomer, Data.Config.MobilePlatform.SkinId, true, false);
                    }
                    catch (Exception)
                    {               
                    }
                }
            }
            return ret;
        }
        public static string GetContactLinks()
        {
            string ret = "";

            if (MobilePlatform.IncludeEmailLinks)
            {
                ret += "<li><a id=\"MPEmailLink\" href=\"mailto:" +
                    AppLogic.AppConfig("MailMe_ToAddress").Trim() + "\">" + AppLogic.GetString("Mobile.Global.EmailLink", MobilePlatform.SkinId, MobilePlatform.MobileLocaleDefault) +"</a></li>";
            }
            if (MobilePlatform.IncludePhoneLinks)
            {
                ret += "<li><a id=\"MPCallLink\" href=\"tel:" +
                    MobilePlatform.ContactPhoneNumber + "\">" + AppLogic.GetString("Mobile.Global.PhoneLink", MobilePlatform.SkinId, MobilePlatform.MobileLocaleDefault) + "</a></li>";
            }

            if (MobilePlatform.IncludeEmailLinks || MobilePlatform.IncludePhoneLinks)
            {
                return "<ul>" + ret + "</ul>";
            }

            return ""; 
        }

        public static string GlobalConfig(String sGlobalConfigName)
        {
            InputValidator IV = new InputValidator("AppConfig");
            String GlobalConfigName = IV.ValidateString("AppConfigName", sGlobalConfigName);
            string result = String.Empty;
            if (GlobalConfigName.Length != 0)
            {
                result = AppLogic.GlobalConfig(GlobalConfigName);
            }
            return result;
        }

        public virtual string GlobalConfigBool(String sGlobalConfigName)
        {
            InputValidator IV = new InputValidator("AppConfigBool");
            String GlobalConfigName = IV.ValidateString("AppConfigName", sGlobalConfigName);
            return AppLogic.GlobalConfigBool(GlobalConfigName).ToString().ToLowerInvariant();
        }

		public bool IsMobileUserAgent()
		{
			return MobileHelper.isMobile(false);
		}

		public bool IsMobileEnabled()
		{
			return MobilePlatform.IsEnabled;
		}

    }
}
