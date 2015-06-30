// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace AspDotNetStorefrontCore
{
	/// <summary>
	/// Summary description for XmlCommon.
	/// </summary>
	public class XmlCommon
	{

		public XmlCommon() 
		{
		}

        static public String SerializeObject(Object pObject, System.Type objectType) 
		{
			try 
			{
				String XmlizedString = null;
				MemoryStream memoryStream = new MemoryStream();
				XmlSerializer xs = new XmlSerializer(objectType);
				XmlTextWriter XmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
				xs.Serialize(XmlTextWriter, pObject);
				memoryStream = (MemoryStream)XmlTextWriter.BaseStream;
				XmlizedString = CommonLogic.UTF8ByteArrayToString(memoryStream.ToArray());
				return XmlizedString;
			}
			catch (Exception ex)
			{
				return CommonLogic.GetExceptionDetail(ex,"\n");
			}
		} 

		static public String FormatXml(XmlDocument inputXml)
		{
			StringWriter writer = new StringWriter();
			XmlTextWriter XmlWriter = new XmlTextWriter(writer);
			XmlWriter.Formatting = Formatting.Indented;
			XmlWriter.Indentation = 2;
			inputXml.WriteTo(XmlWriter);
			return writer.ToString();
		}

		public static String PrettyPrintXml(String Xml)
		{
			String Result = Xml;
            if (Xml.Length != 0)
            {
                Xml = Xml.Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>", "");
                try
                {
                    // Load the XmlDocument with the Xml.
                    XmlDocument D = new XmlDocument();
                    D.LoadXml(Xml);
                    MemoryStream MS = new MemoryStream();
                    XmlTextWriter W = new XmlTextWriter(MS, Encoding.Unicode);

                    W.Formatting = Formatting.Indented;

                    // Write the Xml into a formatting XmlTextWriter
                    D.WriteContentTo(W);
                    W.Flush();
                    MS.Flush();

                    // Have to rewind the MemoryStream in order to read
                    // its contents.
                    MS.Position = 0;

                    // Read MemoryStream contents into a StreamReader.
                    StreamReader SR = new StreamReader(MS);

                    // Extract the text from the StreamReader.
                    String FormattedXml = SR.ReadToEnd();

                    Result = FormattedXml;

                    try
                    {
                        MS.Close();
                        MS = null;
                        W.Close();
                        W = null;
                    }
                    catch { }
                }
                catch { }
            }
			return Result;
		}

		// strips illegal Xml characters:
		static public String XmlEncode(String S)
		{
			if (S == null) 
			{
				return null; 
			}
            S = Regex.Replace(S, @"[^\u0009\u000A\u000D\u0020-\uD7FF\uE000-\uFFFD]", "", RegexOptions.Compiled);
			return XmlEncodeAsIs(S);
		}

		// leaves whatever data is there, and just XmlEncodes it:
		static public String XmlEncodeAsIs(String S)
		{
			if (S == null) 
			{
				return null; 
			}
            StringBuilder sTmp = new StringBuilder();
            using (StringWriter sw = new StringWriter())
            {
                using (XmlTextWriter xwr = new XmlTextWriter(sw))
                {
                    xwr.WriteString(S);
                    sTmp.Append(sw.ToString());
                    xwr.Flush();
                    xwr.Close();
                }
                sw.Close();
            }
            return sTmp.ToString();
        }

        // for paymentech gateway, which is kind of silly:
        static public String XmlEncodeMaxLength(String s, int MaxChars)
        {
            String result = String.Empty;
            foreach (char c in s)
            {
                String sx = new String(c, 1);
                sx = XmlCommon.XmlEncode(sx);
                if (result.Length + sx.Length < MaxChars)
                {
                    result += sx;
                }
            }
            return result;
        }

		// strips illegal Xml characters:
		static public String XmlEncodeAttribute(String S)
		{
			if (S == null) 
			{
				return null; 
			}
            S = Regex.Replace(S, @"[^\u0009\u000A\u000D\u0020-\uD7FF\uE000-\uFFFD]", "", RegexOptions.Compiled);
			return XmlEncodeAttributeAsIs(S);
		}

		// leaves whatever data is there, and just XmlEncodes it:
		static public String XmlEncodeAttributeAsIs(String S)
		{
            return XmlEncodeAsIs(S).Replace("\"","&quot;");
		}
		
		static public String XmlEncodeComment(String S)
		{
			if (S == null) 
			{
				return null; 
			}
			return S.Replace("--","- -"); // -- combination is not allowed, everything else is valid
		}
		
		static public String XmlDecode(String S)
		{
			StringBuilder tmpS = new StringBuilder(S);
			String sTmp = tmpS.Replace("&quot;","\"").Replace("&apos;","'").Replace("&lt;","<").Replace("&gt;",">").Replace("&amp;","&").ToString();
			return sTmp;
		}

		// ----------------------------------------------------------------
		//
		// SIMPLE Xml FIELD ROUTINES
		//
		// ----------------------------------------------------------------

		public static String GetLocaleEntry(String S, String LocaleSetting, bool fallBack)
		{
            String tmpS = String.Empty;
            if (S.Length == 0)
            {
                return tmpS;
            }
            if (S.StartsWith("&lt;ml&gt;", StringComparison.InvariantCultureIgnoreCase))
            {
                S = XmlDecode(S);
            }
            if (S.StartsWith("<ml>", StringComparison.InvariantCultureIgnoreCase))
            {
                String WebConfigLocale = Localization.GetDefaultLocale();
                if (AppLogic.AppConfigBool("UseXmlDOMForLocaleExtraction"))
                {
                    try
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(S);
                        XmlNode node = doc.DocumentElement.SelectSingleNode("//locale[@name=\"" + LocaleSetting + "\"]");
                        if (fallBack && (node == null))
                        {
                            node = doc.DocumentElement.SelectSingleNode("//locale[@name=\"" + WebConfigLocale + "\"]");
                        }
                        if (node != null)
                        {
                            tmpS = node.InnerText.Trim();
                        }
                        if (tmpS.Length != 0)
                        {
                            tmpS = XmlCommon.XmlDecode(tmpS);
                        }
                    }
                    catch { }
                }
                else
                {
                    // for speed, we are using lightweight simple string token extraction here, not full Xml DOM for speed
                    // return what is between <locale name=\"en-US\">...</locale>, Xml Decoded properly.
                    // we have a good locale field formatted field, so try to get desired locale:
                    if (S.IndexOf("<locale name=\"" + LocaleSetting + "\">") != -1)
                    {
                        tmpS = CommonLogic.ExtractToken(S, "<locale name=\"" + LocaleSetting + "\">", "</locale>");
                    }
                    else if (fallBack && (S.IndexOf("<locale name=\"" + WebConfigLocale + "\">") != -1))
                    {
                        tmpS = CommonLogic.ExtractToken(S, "<locale name=\"" + WebConfigLocale + "\">", "</locale>");
                    }
                    else
                    {
                        tmpS = String.Empty;
                    }
                    if (tmpS.Length != 0)
                    {
                        tmpS = XmlCommon.XmlDecode(tmpS);
                    }
                }
            }
            else
            {
                tmpS = S; // for backwards compatibility...they have no locale info, so just return the field.
            }
            return tmpS;
		}

        public static bool NodeContainsAttribute(XmlNode n, String AttributeName)
        {
            return (n.Attributes[AttributeName] != null);
        }

		// assumes this "xmlnode" n has <ml>...</ml> markup on it!
		public static String GetLocaleEntry(XmlNode n, String LocaleSetting, bool fallBack)
		{
            String tmpS = String.Empty;
            if (n != null)
            {
                if (n.InnerText.StartsWith("&lt;ml&gt;", StringComparison.InvariantCultureIgnoreCase))
                {
                    return GetLocaleEntry(XmlDecode(n.InnerText), LocaleSetting, fallBack);
                }
                if (n.HasChildNodes && n.FirstChild.LocalName.Equals("ml", StringComparison.InvariantCultureIgnoreCase))
                {
                    String WebConfigLocale = Localization.GetDefaultLocale();
                    try
                    {
                        XmlNode node = n.SelectSingleNode("ml/locale[@name=\"" + LocaleSetting + "\"]");
                        if (fallBack && (node == null))
                        {
                            node = n.SelectSingleNode("ml/locale[@name=\"" + WebConfigLocale + "\"]");
                        }
                        if (node != null)
                        {
                            tmpS = node.InnerText.Trim();
                        }
                        if (tmpS.Length != 0)
                        {
                            tmpS = XmlCommon.XmlDecode(tmpS);
                        }
                    }
                    catch { }
                }
                else
                {
                    tmpS = n.InnerText.Trim(); // for backwards compatibility...they have no locale info, so just return the field.
                }
            }

            return tmpS;
        }

        public static String XmlFieldExtended(XmlNode node, String fieldName)
        {

            XmlNode n = node.SelectSingleNode(fieldName);
            if (n == null)
            {
                return String.Empty;
            }
            String fldVal = String.Empty;
            if (n.InnerXml.Length != 0 && !n.InnerXml.StartsWith("<![CDATA["))
            {
                fldVal = n.InnerXml;
            }
            else
            {
                fldVal = XmlCommon.XmlField(node, fieldName);
            }
            if (fldVal.Length == 0)
            {
                if (n.NodeType == XmlNodeType.CDATA)
                {
                    fldVal = n.Value;
                }
            }
            return fldVal;
        }

		
		public static String XmlField(XmlNode node, String fieldName)
		{
			String fieldVal = String.Empty;
			try
			{
				fieldVal = node.SelectSingleNode(@fieldName).InnerText.Trim();
			}
			catch {} // node might not be there
			return fieldVal;
		}

		public static String XmlFieldByLocale(XmlNode node, String fieldName, String LocaleSetting)
		{
			String fieldVal = String.Empty;
            XmlNode n = node.SelectSingleNode(@fieldName);
            
            if (n != null)
            {
                if (n.InnerText.StartsWith("&lt;ml&gt;", StringComparison.InvariantCultureIgnoreCase))
                {
                    fieldVal = GetLocaleEntry(XmlCommon.XmlDecode(n.InnerText.Trim()), LocaleSetting, true);
                }
                if (n.HasChildNodes && n.FirstChild.LocalName.Equals("ml", StringComparison.InvariantCultureIgnoreCase))
                {
                    fieldVal = GetLocaleEntry(n, LocaleSetting, true);
                }
                else
                {
                    fieldVal = n.InnerText.Trim();
                }
            }
            if (fieldVal.StartsWith("<ml>", StringComparison.InvariantCultureIgnoreCase))
            {
                fieldVal = GetLocaleEntry(fieldVal, LocaleSetting, true);
            }

			return fieldVal;
		}

		public static bool XmlFieldBool(XmlNode node, String fieldName)
		{
            String tmp = XmlField(node, fieldName);
			if( tmp.Equals("TRUE", StringComparison.InvariantCultureIgnoreCase) || 
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

		public static int XmlFieldUSInt(XmlNode node, String fieldName)
		{
			String tmpS = XmlField(node,fieldName);
			return Localization.ParseUSInt(tmpS);
		}

		public static long XmlFieldUSLong(XmlNode node, String fieldName)
		{
			String tmpS = XmlField(node,fieldName);
			return Localization.ParseUSLong(tmpS);
		}

		public static Single XmlFieldUSSingle(XmlNode node, String fieldName)
		{
			String tmpS = XmlField(node,fieldName);
			return Localization.ParseUSSingle(tmpS);
		}

		public static Double XmlFieldUSDouble(XmlNode node, String fieldName)
		{
			String tmpS = XmlField(node,fieldName);
			return Localization.ParseUSDouble(tmpS);
		}

		public static decimal XmlFieldUSDecimal(XmlNode node, String fieldName)
		{
			String tmpS = XmlField(node,fieldName);
			return Localization.ParseUSCurrency(tmpS);
		}

		public static DateTime XmlFieldUSDateTime(XmlNode node, String fieldName)
		{
			String tmpS = XmlField(node,fieldName);
			return Localization.ParseUSDateTime(tmpS);
		}

		public static int XmlFieldNativeInt(XmlNode node, String fieldName)
		{
			String tmpS = XmlField(node,fieldName);
			return Localization.ParseNativeInt(tmpS);
		}

		public static long XmlFieldNativeLong(XmlNode node, String fieldName)
		{
			String tmpS = XmlField(node,fieldName);
			return Localization.ParseNativeLong(tmpS);
		}

		public static Single XmlFieldNativeSingle(XmlNode node, String fieldName)
		{
			String tmpS = XmlField(node,fieldName);
			return Localization.ParseNativeSingle(tmpS);
		}

		public static Double XmlFieldNativeDouble(XmlNode node, String fieldName)
		{
			String tmpS = XmlField(node,fieldName);
			return Localization.ParseNativeDouble(tmpS);
		}

		public static decimal XmlFieldNativeDecimal(XmlNode node, String fieldName)
		{
			String tmpS = XmlField(node,fieldName);
			return Localization.ParseNativeDecimal(tmpS);
		}

		public static DateTime XmlFieldNativeDateTime(XmlNode node, String fieldName)
		{
			String tmpS = XmlField(node,fieldName);
			return Localization.ParseNativeDateTime(tmpS);
		}

		// ----------------------------------------------------------------
		//
		// SIMPLE Xml ATTRIBUTE ROUTINES
		//
		// ----------------------------------------------------------------


        public static String XmlAttribute(XmlNode node, String AttributeName)
        {
            if (node == null)
            {
                return String.Empty;
            }

            String AttributeVal = String.Empty;
            try
            {
                if (node.Attributes != null && node.Attributes[AttributeName] != null)
                {
                    AttributeVal = node.Attributes[AttributeName].InnerText.Trim();
                }
            }
            catch { } // node might not be there
            return AttributeVal;
        }

		public static bool XmlAttributeBool(XmlNode node, String AttributeName)
		{
            String tmp = XmlAttribute(node, AttributeName);

			if( tmp.Equals("TRUE", StringComparison.InvariantCultureIgnoreCase) || 
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

		public static int XmlAttributeUSInt(XmlNode node, String AttributeName)
		{
			String tmpS = XmlAttribute(node,AttributeName);
			return Localization.ParseUSInt(tmpS);
		}

		public static long XmlAttributeUSLong(XmlNode node, String AttributeName)
		{
			String tmpS = XmlAttribute(node,AttributeName);
			return Localization.ParseUSLong(tmpS);
		}

		public static Single XmlAttributeUSSingle(XmlNode node, String AttributeName)
		{
			String tmpS = XmlAttribute(node,AttributeName);
			return Localization.ParseUSSingle(tmpS);
		}

		public static Double XmlAttributeUSDouble(XmlNode node, String AttributeName)
		{
			String tmpS = XmlAttribute(node,AttributeName);
			return Localization.ParseUSDouble(tmpS);
		}

		public static decimal XmlAttributeUSDecimal(XmlNode node, String AttributeName)
		{
			String tmpS = XmlAttribute(node,AttributeName);
			return Localization.ParseUSDecimal(tmpS);
		}

		public static DateTime XmlAttributeUSDateTime(XmlNode node, String AttributeName)
		{
			String tmpS = XmlAttribute(node,AttributeName);
			return Localization.ParseUSDateTime(tmpS);
		}

		public static int XmlAttributeNativeInt(XmlNode node, String AttributeName)
		{
			String tmpS = XmlAttribute(node,AttributeName);
			return Localization.ParseNativeInt(tmpS);
		}

		public static long XmlAttributeNativeLong(XmlNode node, String AttributeName)
		{
			String tmpS = XmlAttribute(node,AttributeName);
			return Localization.ParseNativeLong(tmpS);
		}

		public static Single XmlAttributeNativeSingle(XmlNode node, String AttributeName)
		{
			String tmpS = XmlAttribute(node,AttributeName);
			return Localization.ParseNativeSingle(tmpS);
		}

		public static Double XmlAttributeNativeDouble(XmlNode node, String AttributeName)
		{
			String tmpS = XmlAttribute(node,AttributeName);
			return Localization.ParseNativeDouble(tmpS);
		}

		public static decimal XmlAttributeNativeDecimal(XmlNode node, String AttributeName)
		{
			String tmpS = XmlAttribute(node,AttributeName);
			return Localization.ParseNativeDecimal(tmpS);
		}

		public static DateTime XmlAttributeNativeDateTime(XmlNode node, String AttributeName)
		{
			String tmpS = XmlAttribute(node,AttributeName);
			return Localization.ParseNativeDateTime(tmpS);
		}

		public static String GetXPathEntry(String S, String XPath)
		{
			String tmpS = String.Empty;
			if(S.Length == 0)
			{
				return tmpS;
			}
			try
			{
				XmlDocument doc = new XmlDocument();
				doc.LoadXml(S);
				XmlNode node = doc.DocumentElement.SelectSingleNode(XPath);
				if(node != null)
				{
					tmpS = node.InnerText;
				}
				if(tmpS.Length != 0)
				{
					tmpS = XmlCommon.XmlDecode(tmpS);
				}
			}
			catch {}
			return tmpS;
		}

        /// <summary>
        /// Send and capture data using GET
        /// </summary>
        /// <param name="Request">The Xml Request to be sent</param>
        /// <param name="Server">The server the request should be sent to</param>
        /// <returns>String</returns>
        public static String GETandReceiveData(String Request, String Server)
        {
            // check for cache hit:
            String CacheName = Server + Request;
            String s = (String)HttpContext.Current.Cache.Get(CacheName);
            if (s != null)
            {
                return s;
            }
            HttpWebRequest requestX = (HttpWebRequest)WebRequest.Create(Server + "?" + Request);
            HttpWebResponse response = (HttpWebResponse)requestX.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream());
            String result = sr.ReadToEnd();
            response.Close();
            sr.Close();
            sr.Dispose();

            // cache result. if there was no error in it!
            if (result.IndexOf("error:", StringComparison.InvariantCultureIgnoreCase) != -1)
            {
                try
                {
                    HttpContext.Current.Cache.Remove(CacheName);
                }
                catch { }
            }
            else
            {
                HttpContext.Current.Cache.Insert(CacheName, result, null, System.DateTime.Now.AddMinutes(15), TimeSpan.Zero);
            }

            return result;
        }
	}

    internal class StringWriterWithEncoding : StringWriter
    {
        private Encoding m_Encoding;

        public StringWriterWithEncoding(StringBuilder sb, Encoding encoding)
            : base()
        {
            m_Encoding = encoding;
        }

        public override Encoding Encoding { get { return m_Encoding; } }
    }

}
