// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Web.Security;
using System.Data;
using System.Text;
using System.Xml;
using System.Collections;
using System.Globalization;

namespace AspDotNetStorefrontCore
{
    /// <summary>
    /// </summary>
    public static class Currency
    {

        static public String m_LastRatesResponseXml = String.Empty;
        static public String m_LastRatesTransformedXml = String.Empty;

        static private XmlDocument RatesDoc
        {
            get
            {
                XmlDocument d = (XmlDocument)HttpContext.Current.Cache.Get("CurrencyDoc");
                if (d == null)
                {
                    d = DB.GetSqlXmlDoc("select * from Currency  with (NOLOCK)  order by Published desc, DisplayOrder,Name for xml auto", null);
                    HttpContext.Current.Cache.Insert("CurrencyDoc", d, null, System.DateTime.Now.AddMinutes(AppLogic.AppConfigUSInt("Localization.CurrencyCacheMinutes")), TimeSpan.Zero);
                }
                return d;
            }
        }

        static public void GetLiveRates()
        {
            String PN = AppLogic.AppConfig("Localization.CurrencyFeedXmlPackage");
            if (PN.Length != 0)
            {
                try
                {
                    using (XmlPackage2 p = new XmlPackage2(PN))
                    {

                        m_LastRatesResponseXml = p.XmlDataDocument.InnerXml;
                        m_LastRatesTransformedXml = p.TransformString();
                        if (m_LastRatesTransformedXml.Length != 0)
                        {
                            // update master db table:
                            XmlDocument d = new XmlDocument();
                            d.LoadXml(m_LastRatesTransformedXml);
                            foreach (XmlNode n in d.SelectNodes("//currency"))
                            {
                                String CurrencyCode = XmlCommon.XmlAttribute(n, "code");
                                String rate = XmlCommon.XmlAttribute(n, "rate");
                                DB.ExecuteSQL("update Currency set ExchangeRate=" + rate + ", WasLiveRate=1, LastUpdated=getdate() where CurrencyCode=" + DB.SQuote(CurrencyCode));
                            }
                        }
                    }
                    FlushCache(); // flush anyway for safety
                }
                catch (Exception ex)
                {
                    try
                    {
                        AppLogic.SendMail(AppLogic.AppConfig("StoreName") + " Currency.GetLiveRates Failure", "Occurred at: " + Localization.ToNativeDateTimeString(System.DateTime.Now) + CommonLogic.GetExceptionDetail(ex, ""), false, AppLogic.AppConfig("MailMe_FromAddress"), AppLogic.AppConfig("MailMe_FromName"), AppLogic.AppConfig("MailMe_ToAddress"), AppLogic.AppConfig("MailMe_ToName"), String.Empty, AppLogic.MailServer());
                    }
                    catch { }
                }
            }
        }

        static public void FlushCache()
        {
            try
            {
                HttpContext.Current.Cache.Remove("CurrencyDoc");
            }
            catch { }
        }


        static public bool isValidCurrencyCode(String CurrencyCode)
        {
            String tmpS = String.Empty;
            XmlNode n = RatesDoc.SelectSingleNode("//Currency[@CurrencyCode='" + CurrencyCode + "']");
            return (n != null);

        }

        static public String GetCurrencyCode(String Name)
        {
            if (Name.Length == 0)
            {
                throw new ArgumentException("Invalid Currency Name (empty string)");
            }
            String tmpS = String.Empty;
            XmlNode n = RatesDoc.SelectSingleNode("//Currency[@Name=" + Name + "]");
            if (n != null)
            {
                tmpS = XmlCommon.XmlAttribute(n, "CurrencyCode");
            }
            return tmpS;
        }

        static public int GetCurrencyID(String CurrencyCode)
        {
            if (CurrencyCode.Length == 0)
            {
                return 0;
            }
            int tmpS = 0;
            XmlNode n = RatesDoc.SelectSingleNode("//Currency[@CurrencyCode=" + CommonLogic.SQuote(CurrencyCode) + "]");
            if (n != null)
            {
                tmpS = XmlCommon.XmlAttributeNativeInt(n, "CurrencyID");
            }
            return tmpS;
        }

        static public Decimal GetExchangeRate(String CurrencyCode)
        {
            if (CurrencyCode.Length == 0)
            {
                throw new ArgumentException("Invalid CurrencyCode (empty string)");
            }
            Decimal tmpS = System.Decimal.Zero;
            XmlNode n = RatesDoc.SelectSingleNode("//Currency[@CurrencyCode=" + CommonLogic.SQuote(CurrencyCode) + "]");
            if (n != null)
            {
                tmpS = XmlCommon.XmlAttributeUSDecimal(n, "ExchangeRate");
            }
            return tmpS;
        }

        static public String GetFeedReferenceCurrencyCode()
        {
            return AppLogic.AppConfig("Localization.CurrencyFeedBaseRateCurrencyCode");
        }

        static public void SetReferenceCurrencyCode(String CurrencyCode, bool UpdateRates)
        {
            if (CurrencyCode.Length == 0)
            {
                throw new ArgumentException("Invalid CurrencyCode (empty string)");
            }
            AppLogic.SetAppConfig("Localization.CurrencyFeedBaseRateCurrencyCode", CurrencyCode);
            if (UpdateRates)
            {
                GetLiveRates();
            }
        }

        static public String GetFeedUrl()
        {
            return AppLogic.AppConfig("Localization.CurrencyFeedUrl");
        }

        static public void SetFeedUrl(String NewUrl, bool UpdateRates)
        {
            AppLogic.SetAppConfig("Localization.CurrencyFeedUrl", NewUrl);
            if (NewUrl.Length != 0 && UpdateRates)
            {
                GetLiveRates();
            }
        }

        static public String GetCurrencyCode(int CurrencyID)
        {
            String tmpS = String.Empty;
            XmlNode n = RatesDoc.SelectSingleNode("//Currency[@CurrencyID=" + CurrencyID.ToString() + "]");
            if (n != null)
            {
                tmpS = XmlCommon.XmlAttribute(n, "CurrencyCode");
            }
            return tmpS;
        }

        static public String GetName(int CurrencyID)
        {
            String tmpS = String.Empty;
            XmlNode n = RatesDoc.SelectSingleNode("//Currency[@CurrencyID=" + CurrencyID.ToString() + "]");
            if (n != null)
            {
                tmpS = XmlCommon.XmlAttribute(n, "Name");
            }
            return tmpS;
        }

        static public String GetName(String CurrencyCode)
        {
            if (CurrencyCode.Length == 0)
            {
                throw new ArgumentException("Invalid CurrencyCode (empty string)");
            }
            String tmpS = String.Empty;
            XmlNode n = RatesDoc.SelectSingleNode("//Currency[@CurrencyCode=" + CommonLogic.SQuote(CurrencyCode) + "]");
            if (n != null)
            {
                tmpS = XmlCommon.XmlAttribute(n, "Name");
            }
            return tmpS;
        }

        static public String GetDisplayLocaleFormat(String CurrencyCode)
        {
            if (CurrencyCode.Length == 0)
            {
                throw new ArgumentException("Invalid CurrencyCode (empty string)");
            }
            String tmpS = String.Empty;
            XmlNode n = RatesDoc.SelectSingleNode("//Currency[@CurrencyCode=" + CommonLogic.SQuote(CurrencyCode) + "]");
            if (n != null)
            {
                tmpS = XmlCommon.XmlAttribute(n, "DisplayLocaleFormat");
            }
            return tmpS;
        }

        static public String GetDisplaySpec(String CurrencyCode)
        {
            if (CurrencyCode.Length == 0)
            {
                throw new ArgumentException("Invalid CurrencyCode (empty string)");
            }
            String tmpS = String.Empty;
            XmlNode n = RatesDoc.SelectSingleNode("//Currency[@CurrencyCode=" + CommonLogic.SQuote(CurrencyCode) + "]");
            if (n != null)
            {
                tmpS = XmlCommon.XmlAttribute(n, "DisplaySpec");
            }
            return tmpS;
        }

        static public Decimal Convert(Decimal SourceValue, String SourceCurrencyCode, String TargetCurrencyCode)
        {
            Decimal result = SourceValue;
            if (SourceCurrencyCode == TargetCurrencyCode)
            {
                return SourceValue;
            }
            if (result != System.Decimal.Zero && SourceCurrencyCode.ToLower() != TargetCurrencyCode.ToLower())
            {
                result = ConvertToBaseCurrency(result, SourceCurrencyCode);
                result = ConvertFromBaseCurrency(result, TargetCurrencyCode);
            }
            return result;
        }

        static public Decimal ConvertToBaseCurrency(Decimal SourceValue, String SourceCurrencyCode)
        {
            Decimal result = SourceValue;
            if (result != System.Decimal.Zero && SourceCurrencyCode != GetFeedReferenceCurrencyCode())
            {
                Decimal ExchangeRate = GetExchangeRate(SourceCurrencyCode);
                if (ExchangeRate == System.Decimal.Zero)
                {
                    throw new ArgumentException("Exchange Rate Not Found for Currency=" + SourceCurrencyCode);
                }
                result = result / ExchangeRate;
            }
            return result;
        }

        static public Decimal ConvertFromBaseCurrency(Decimal SourceValue, String TargetCurrencyCode)
        {
            Decimal result = SourceValue;
            if (result != System.Decimal.Zero && TargetCurrencyCode != GetFeedReferenceCurrencyCode())
            {
                Decimal ExchangeRate = GetExchangeRate(TargetCurrencyCode);
                if (ExchangeRate == System.Decimal.Zero)
                {
                    throw new ArgumentException("Exchange Rate Not Found for Currency=" + TargetCurrencyCode);
                }
                result = result * ExchangeRate;
            }
            return result;
        }

        static public String GetDefaultCurrency()
        {
            return Localization.GetPrimaryCurrency();
        }

        static public String ValidateCurrencySetting(String Currency)
        {
            String tmp = Localization.GetPrimaryCurrency();
            if (isValidCurrencyCode(Currency))
            {
                tmp = Currency;
            }
            return Localization.CheckCurrencySettingForProperCase(tmp);
        }

        // this DOES NOT apply any exchange rates!!
        // it is FORMATTING ONLY!
        static public String ToString(decimal amt, String TargetCurrencyCode)
        {
            return Localization.CurrencyStringForDisplayWithoutExchangeRate(amt, TargetCurrencyCode);
        }


        static public int NumPublishedCurrencies()
        {
            return RatesDoc.SelectNodes("//Currency[@Published = 1]").Count;
        }

        static public String GetSelectList(String SelectName, String OnChangeHandler, String CssClass, String SelectedCurrencyCode)
        {
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
            foreach (XmlNode n in RatesDoc.SelectNodes("//Currency[@Published = 1]"))
            {
                string cc = XmlCommon.XmlAttribute(n, "CurrencyCode");
                tmpS.Append("<option value=\"" + cc + "\" " + CommonLogic.IIF(SelectedCurrencyCode == cc, " selected ", "") + ">" + cc + " (" + XmlCommon.XmlAttribute(n, "Name") + ")</option>");
            }
            tmpS.Append("</select>");
            return tmpS.ToString();
        }

        static public ArrayList getCurrencyList()
        {
            ArrayList list = new ArrayList();

            foreach (XmlNode n in RatesDoc.SelectNodes("//Currency[@Published = 1]"))
            {
                int cID = XmlCommon.XmlAttributeNativeInt(n, "CurrencyID");
                string cc = XmlCommon.XmlAttribute(n, "CurrencyCode");
                string cn = XmlCommon.XmlAttribute(n, "Name");
                ListItemClass item = new ListItemClass();
                item.Item = cc + " (" + cn + ")";
                item.Value = cID;
                list.Add(item);
            }

            return list;
        }
    }
}
