// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Web.Caching;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.IO;
using System.Collections;
using System.Security;
using System.Security.Policy;
using System.Globalization;
using System.Threading;

namespace AspDotNetStorefrontCore
{
    /// <summary>
    /// Summary description for XmlPackage2.
    /// </summary>
    public class XmlPackage2: IDisposable
    {
        #region Private Variables
        private static readonly String readonly_DefaultUserQueryName = "UserQuery";
        private String m_PackageName = String.Empty;
        private String m_PackageUrl = String.Empty;
        private Customer m_ThisCustomer = null;
        private int m_SkinID = 1;
        private XmlDocument m_PackageDocument = null;
        private XmlDocument m_DataDocument = new XmlDocument();
        private String m_DocumentSource = String.Empty;
        private String m_SQLData = String.Empty;
        private String m_WebData = String.Empty;
        private String m_EntityHelpersXml = String.Empty;
        private String m_AdditionalRuntimeParms = String.Empty;
        private String m_UserSpecifiedQuery = String.Empty;
        private XmlDocument m_TransformDocument = new XmlDocument();
        private XslCompiledTransform m_Transform;
        private XsltArgumentList m_TransformArgumentList = new XsltArgumentList();
        private string m_TransformSource = String.Empty; //full local path to the transform document
        private DataSet m_Data = null;
        private string m_FinalResult = String.Empty;
        private string m_SectionTitle = String.Empty;
        private int m_NumRows = 0; // this is set AFTER transformString is called, and ONLY set for the first query in the package!
        private readonly String[] ro_ServerVariablesList = { "HTTP_HOST", "HTTP_USER_AGENT", "AUTH_TYPE", "AUTH_USER", "AUTH_PASSWORD", "HTTPS", "LOCAL_ADDR", "PATH_INFO", "PATH_TRANSLATED", "SCRIPT_NAME", "SERVER_NAME", "SERVER_PORT_SECURE", "HTTP_CLUSTER_HTTPS" };
        private Hashtable m_RuntimeQueries = new Hashtable();
        private Hashtable m_RuntimeParameters = new Hashtable();
        private bool m_RequiresParser = false;
        private string m_LocaleSetting = String.Empty;
        private string m_CurrencySetting = String.Empty;

        private decimal m_Version = 0;
        private bool m_IsDebug = false;
        private string m_displayname = String.Empty;
        private bool m_IncludesEntityHelper = false;
        private bool HasASPDNSFNameSpace = false;
        private bool m_AllowEngine = false;

        private string m_ContentType = "text/html";
        private string m_SETitle = string.Empty;
        private string m_SEKeywords = string.Empty;
        private string m_SEDescription = string.Empty;
        private string m_SENoScript = string.Empty;
        private string m_SqlDebug = string.Empty;

        private bool m_PackageFound = false;
        private DataTable m_SystemT = new DataTable("System");
        private XmlDocument m_SystemData = new XmlDocument();
        private bool disposed = false;

        private const string NULL_STRING = "null";
        #endregion

        #region Constructors
        public XmlPackage2(string PackageName, Customer cust)
            : this(PackageName, cust, cust.SkinID, String.Empty, String.Empty) { }

        public XmlPackage2(string PackageName)
            : this(PackageName, null, 1, String.Empty, String.Empty)
        { }
        
        public XmlPackage2(string PackageName, string AdditionalRuntimeParms)
            : this(PackageName, null, 1, String.Empty, AdditionalRuntimeParms)
        { }
        
        public XmlPackage2(string PackageName, int SkinID)
            : this(PackageName, null, SkinID, String.Empty, String.Empty) { }

        public XmlPackage2(string PackageName, Customer cust, int SkinID)
            : this(PackageName, cust, SkinID, String.Empty, String.Empty) { }

        public XmlPackage2(string PackageName, Customer cust, int SkinID, String UserQuery)
            : this(PackageName, cust, SkinID, UserQuery, String.Empty)
        { }

        public XmlPackage2(string PackageName, Customer cust, String UserQuery)
            : this(PackageName, cust, cust.SkinID, UserQuery, String.Empty)
        { }

        // remember, AdditionalQueryStringParms values must be properly urlencoded
        public XmlPackage2(string PackageName, Customer cust, String UserQuery, String AdditionalQueryStringParms)
            : this(PackageName, cust, cust.SkinID, UserQuery, AdditionalQueryStringParms)
        { }

        public XmlPackage2(string PackageName, Customer cust, int SkinID, String UserQuery, String AdditionalRuntimeParms)
            : this(PackageName, cust, SkinID, UserQuery, AdditionalRuntimeParms, String.Empty)
        { }

        public XmlPackage2(string PackageName, Customer cust, int SkinID, String UserQuery, String AdditionalRuntimeParms, String OnlyRunNamedQuery)
            : this(PackageName, cust, SkinID, UserQuery, AdditionalRuntimeParms, OnlyRunNamedQuery, true) { }


        // remember, AdditionalQueryStringParms values must be properly urlencoded, they are added as runtime parms to the Xsl doc
        public XmlPackage2(string PackageName, Customer cust, int SkinID, String UserQuery, String AdditionalRuntimeParms, String OnlyRunNamedQuery, bool UseExtensions)
        {
            m_PackageName = PackageName;
            m_ThisCustomer = cust;
            if (m_ThisCustomer == null)
            {
                try
                {
                    if (((AspDotNetStorefrontPrincipal)HttpContext.Current.User) != null)
                    {
                        m_ThisCustomer = ((AspDotNetStorefrontPrincipal)HttpContext.Current.User).ThisCustomer;
                    }
                }
                catch { }
            }
            if (m_ThisCustomer == null)
            {
                m_ThisCustomer = new Customer(true);
                m_LocaleSetting = Localization.GetDefaultLocale();
                m_CurrencySetting = Localization.GetPrimaryCurrency();
            }
            else
            {
                m_LocaleSetting = m_ThisCustomer.LocaleSetting;
                m_CurrencySetting = m_ThisCustomer.CurrencySetting;
            }

            m_SkinID = SkinID;
            m_AdditionalRuntimeParms = AdditionalRuntimeParms.Trim();
            m_UserSpecifiedQuery = UserQuery.Trim();
            
            //Add user ExtensionObjects
            if (UseExtensions)
            {
                XSLTExtensions ExtObj = new XSLTExtensions(m_ThisCustomer, m_SkinID);
                m_TransformArgumentList.AddExtensionObject("urn:aspdnsf", ExtObj);

                XsltObjects.ExtensionConfiguration objExtConfig = XsltObjects.ExtensionConfiguration.GetExtensionConfiguration("xsltobjects");
                if (objExtConfig != null)
                {
                    Object userExtObj;
                    foreach (XsltObjects.Extension ext in objExtConfig.Extensions.Values)
                    {
                        userExtObj = ExtensionObjects.CreateExtension(ext.Type, m_ThisCustomer, m_SkinID, null);
                        m_TransformArgumentList.AddExtensionObject(ext.Attributes["namespace"], userExtObj);
                    }
                }
            }

            if (m_UserSpecifiedQuery.Length != 0)
            {
                AddRunTimeQuery(readonly_DefaultUserQueryName, m_UserSpecifiedQuery);
            }
            if (m_AdditionalRuntimeParms.Length != 0)
            {
                foreach (String s in m_AdditionalRuntimeParms.Split('&'))
                {
                    String[] s2 = s.Split(new Char[] { '=' }, 2);
                    String ParamName = String.Empty;
                    try
                    {
                        ParamName = s2[0];
                    }
                    catch { }
                    String ParamValue = String.Empty;
                    try
                    {
                        ParamValue = HttpContext.Current.Server.UrlDecode(s2[1]);
                    }
                    catch { }
                    if (ParamName.Length != 0)
                    {
                        AddRunTimeParam(ParamName, ParamValue);
                    }
                }
            }

            //Load the package (from cache first) and create a Transform using the packageTransform
            //then get the data based on the queries in the package and any user queries
            string CacheName = PackageName + "_" + SkinID.ToString() + "_" + m_LocaleSetting;
            m_PackageDocument = (XmlDocument)HttpContext.Current.Cache.Get(CacheName);


            if (m_PackageDocument == null)
            {
                m_PackageUrl = FullPackageUrl(PackageName, m_SkinID);
                m_TransformSource = CommonLogic.SafeMapPath(m_PackageUrl);
                if (!CommonLogic.FileExists(m_TransformSource))
                {
                    m_PackageFound = false;
                }

                m_PackageDocument = new XmlDocument();

                try
                {
                    m_PackageDocument.Load(m_TransformSource);
                    if (File.Exists(m_TransformSource)) HttpContext.Current.Cache.Insert(CacheName, m_PackageDocument, new CacheDependency(m_TransformSource));
                }
                catch (Exception ex)
                {
                    String XMsg = CommonLogic.GetExceptionDetail(ex, "");
                    if (XMsg.IndexOf("could not find file", StringComparison.InvariantCultureIgnoreCase) != -1)
                    {
                        // clean up message to make it more "helpful"
                        XMsg = AppLogic.GetString("XmlPackage.LoadError", ThisCustomer.LocaleSetting);
                    }
                    throw new ArgumentException("Error in XmlPackage(.Load), Package=[" + PackageName + "], Msg=[" + XMsg + "]");
                }
            }

            XmlNode packageNode = m_PackageDocument.SelectSingleNode("/package");


            //Assign package property vairables
            XmlNode reqparser = packageNode.Attributes["RequiresParser"];
            if (reqparser != null && reqparser.InnerText == "true")
            {
                m_RequiresParser = true;
            }


            if (packageNode.Attributes["version"] != null)
            {
                try
                {
                    m_Version = Localization.ParseUSDecimal(packageNode.Attributes["version"].InnerText);
                }
                catch
                {
                    throw new Exception("Invalid package version specified");
                }
            }
            else
            {
                m_Version = 2.0M;
            }

            XmlNode debug = packageNode.Attributes["debug"];
            if (((debug != null && debug.InnerText == "true") || AppLogic.AppConfigBool("XmlPackage.DumpTransform")) && !PackageName.EqualsIgnoreCase("page.menu.xml.config"))
            {
                m_Transform = new XslCompiledTransform(true);
                m_IsDebug = true;
            }

            XmlNode includesentityhelper = packageNode.Attributes["includeentityhelper"];
            if (includesentityhelper != null && includesentityhelper.InnerText == "true")
            {
                m_IncludesEntityHelper = true;
            }

            XmlNode allowengine = packageNode.Attributes["allowengine"];
            if (allowengine != null && allowengine.InnerText == "true")
            {
                m_AllowEngine = true;
            }

            XmlNode displayname = packageNode.Attributes["displayname"];
            if (displayname != null)
            {
                m_displayname = displayname.InnerText;
            }

            XmlNode contenttype = packageNode.Attributes["contenttype"];
            if (contenttype != null)
            {
                m_ContentType = contenttype.InnerText;
            }

            XmlNodeList RuntimeNodes = m_PackageDocument.SelectNodes("/package/runtime");
            foreach (XmlNode n in RuntimeNodes)
            {
                switch (n.Attributes["paramtype"].InnerText)
                {
                    case "appconfig":
                        AddRunTimeParam(n.Attributes["paramname"].InnerText, AppLogic.AppConfig(n.Attributes["requestparamname"].InnerText));
                        break;
                    case "request":
                        AddRunTimeParam(n.Attributes["paramname"].InnerText, CommonLogic.ParamsCanBeDangerousContent(n.Attributes["requestparamname"].InnerText));
                        break;
                }
            }

            HasASPDNSFNameSpace = (m_PackageDocument.SelectSingleNode("/package/PackageTransform").FirstChild.Attributes["xmlns:aspdnsf"] != null);




            //Load transform
            if (m_PackageDocument.SelectSingleNode("/package/PackageTransform").FirstChild == null)
            {
                throw new Exception("The PackageTransform element must contain an xsl:stylesheet node");
            }

            XmlUrlResolver resolver = new XmlUrlResolver();
            resolver.Credentials = System.Net.CredentialCache.DefaultCredentials;
            if ((this.IsDebug || AppLogic.AppConfigBool("XmlPackage.DumpTransform")) && !this.Name.EqualsIgnoreCase("page.menu.xml.config"))
            {
                String xFormFile = String.Empty;
                //xFormFile = CommonLogic.SafeMapPath(String.Format("{0}images/{1}_{2}.runtime.xsl", CommonLogic.IIF(AppLogic.IsAdminSite, "../", ""), m_PackageName, CommonLogic.IIF(AppLogic.IsAdminSite, "admin", "store")));
                xFormFile = CommonLogic.SafeMapPath(String.Format("~/images/{0}_{1}.runtime.xsl", m_PackageName, CommonLogic.IIF(AppLogic.IsAdminSite, "admin", "store")));

                StreamWriter sw = File.CreateText(xFormFile);
                sw.WriteLine(XmlCommon.PrettyPrintXml(m_PackageDocument.SelectSingleNode("/package/PackageTransform").InnerXml));
                sw.Close();

                try
                {
                    m_Transform.Load(xFormFile, XsltSettings.TrustedXslt, resolver);
                }
                catch (SecurityException)
                {
                    m_Transform = new XslCompiledTransform(false); //if it failed it must be in Medium trust so turn off debugging in the tranform itself.
                    m_Transform.Load(xFormFile, XsltSettings.TrustedXslt, resolver);
                }
            }
            else
            {
                m_Transform = (XslCompiledTransform)HttpContext.Current.Cache.Get(m_PackageDocument.BaseURI);
                if (m_Transform == null)
                {
                    m_Transform = new XslCompiledTransform(false);
                    m_Transform.Load(m_PackageDocument.SelectSingleNode("/package/PackageTransform").FirstChild, XsltSettings.TrustedXslt, resolver);
                    if (File.Exists(m_PackageDocument.BaseURI.Replace("file:///", "")))
                    {
                        HttpContext.Current.Cache.Insert(m_PackageDocument.BaseURI, m_Transform, new CacheDependency(m_PackageDocument.BaseURI.Replace("file:///", "")));
                    }
                }

            }

            //Create Xml DataDocument
            CultureInfo tmpCurrentCulture = Thread.CurrentThread.CurrentCulture;
            CultureInfo tmpCurrentUICulture = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(Localization.GetSqlServerLocale());
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Localization.GetSqlServerLocale());
            using (StringReader sr = new StringReader("<root/>"))
            {
                using (XmlReader xr = XmlReader.Create(sr))
                {
                    m_DataDocument.Load(xr);
                }
            }
            InitializeSystemData();
            GetSqlData(OnlyRunNamedQuery);
            Thread.CurrentThread.CurrentCulture = tmpCurrentCulture;
            Thread.CurrentThread.CurrentUICulture = tmpCurrentUICulture;
            GetWebData();

            //Add EntityHelper data to the Xml DataDocument
            if (this.IncludesEntityHelper)
            {
                m_DataDocument.DocumentElement.AppendChild(m_DataDocument.CreateNode(XmlNodeType.Element, "EntityHelpers", ""));
                foreach (string s in AppLogic.ro_SupportedEntities)
                {
                    int storeID = CommonLogic.IIF(AppLogic.IsAdminSite, 0, AppLogic.StoreID());
                    m_DataDocument.SelectSingleNode("/root/EntityHelpers").AppendChild(m_DataDocument.CreateNode(XmlNodeType.Element, s, ""));
                    foreach (XmlNode x in AppLogic.LookupHelper(s, storeID).m_TblMgr.XmlDoc.SelectNodes("/root/Entity"))
                    {
                        m_DataDocument.SelectSingleNode("/root/EntityHelpers/" + s).AppendChild(m_DataDocument.ImportNode(x, true));
                    }
                }
            }
           
            //Add Search Engine Settings
            ProcessSESettings();
            ProcessAfterActions();
        }

        #endregion

        #region Public Properties
        public DataSet Data
        {
            get
            {
                if (m_Data == null)
                {
                    m_Data = new DataSet("root");
                }
                return m_Data;
            }
        }

        public Customer ThisCustomer
        {
            get
            {
                return m_ThisCustomer;
            }
        }

        public int SkinID
        {
            get
            {
                return m_SkinID;
            }
        }
        public bool PackageFound
        {
            get
            {
                return m_PackageFound;
            }
        }
        public string ContentType
        {
            get
            {
                return m_ContentType;
            }
        }
        public string SETitle
        {
            get
            {
                return m_SETitle;
            }
        }
        public string FinalResult
        {
            get
            {
                return m_FinalResult;
            }
        }
        public String SectionTitle
        {
            get
            {
                return m_SectionTitle;
            }
        }
        public string SEKeywords
        {
            get
            {
                return m_SEKeywords;
            }
        }
        public string SEDescription
        {
            get
            {
                return m_SEDescription;
            }
        }
        public string SENoScript
        {
            get
            {
                return m_SENoScript;
            }
        }
        public string Name
        {
            get
            {
                return m_PackageName;
            }
        }
        public bool RequiresParser
        {
            get
            {
                return m_RequiresParser;
            }
        }
        public String URL
        {
            get
            {
                return m_PackageUrl;
            }
        }
        public XsltArgumentList TransformArgumentList
        {
            get
            {
                if (m_TransformArgumentList == null)
                {
                    m_TransformArgumentList = new XsltArgumentList();
                }
                return m_TransformArgumentList;
            }
            set
            {
                m_TransformArgumentList = value;
            }
        }
        public string XmlSystemData
        {
            get
            {
                return m_SystemData.OuterXml;
            }
        }
        public string XmlSqlData
        {
            get
            {
                return m_SQLData;
            }
        }
        public XmlDocument XmlDataDocument
        {
            get
            {
                return m_DataDocument;
            }
        }
        public XmlDocument TransformDocument
        {
            get
            {
                return m_TransformDocument;
            }
        }
        public XmlDocument PackageDocument
        {
            get
            {
                return m_PackageDocument;
            }
        }
        public XslCompiledTransform Transform
        {
            get
            {
                return m_Transform;
            }
        }
        public decimal Version
        {
            get { return m_Version; }
        }
        public bool IsDebug
        {
            get { return m_IsDebug; }
        }
        public string DisplayName
        {
            get { return m_displayname; }
            set { m_displayname = value; }
        }
        public string SqlDebug
        {
            get { return m_SqlDebug; }
        }
        public bool IncludesEntityHelper
        {
            get { return m_IncludesEntityHelper; }
        }
        public bool AllowEngine
        {
            get { return m_AllowEngine; }
        }
        public XmlNodeList HttpHeaders
        {
            get { return m_PackageDocument.SelectNodes("/package/HTTPHeaders/HTTPHeader"); }
        }
        #endregion
        
        #region Public Methods
        public void Dispose()
        {
            if (!disposed)
            {
                m_SystemT.Dispose();
                disposed = true;
            }
        }

        public string TransformString()
        {

            if (AppLogic.AppConfigBool("XmlPackage.DumpTransform") || this.IsDebug)
            {
                try // don't let logging crash the site
                {
                    //String fn = CommonLogic.SafeMapPath(String.Format("{0}images/{1}_{2}.runtime.xml", CommonLogic.IIF(AppLogic.IsAdminSite, "../", ""), m_PackageName, CommonLogic.IIF(AppLogic.IsAdminSite, "admin", "store")));
                    String fn = CommonLogic.SafeMapPath(String.Format("{0}images/{1}_{2}.runtime.xml", CommonLogic.IIF(AppLogic.IsAdminSite, "~/{0}".FormatWith(AppLogic.AdminDir()), "~/"), m_PackageName, CommonLogic.IIF(AppLogic.IsAdminSite, "admin", "store")));

                    //xFormFile = CommonLogic.SafeMapPath(String.Format("{0}images/{1}_{2}.runtime.xml", CommonLogic.IIF(AppLogic.IsAdminSite, "../", ""), m_PackageName, CommonLogic.IIF(AppLogic.IsAdminSite, "admin", "store")));

                    using (StreamWriter sw = File.CreateText(fn))
                    {
                        sw.WriteLine(XmlCommon.PrettyPrintXml(m_DataDocument.InnerXml));
                        sw.Close();
                    }

                    //fn = CommonLogic.SafeMapPath(String.Format("{0}images/{1}_{2}.runtime.sql", CommonLogic.IIF(AppLogic.IsAdminSite, "../", ""), m_PackageName, CommonLogic.IIF(AppLogic.IsAdminSite, "admin", "store")));
                    fn = CommonLogic.SafeMapPath(String.Format("{0}images/{1}_{2}.runtime.sql", CommonLogic.IIF(AppLogic.IsAdminSite, "~/{0}".FormatWith(AppLogic.AdminDir()), "~/"), m_PackageName, CommonLogic.IIF(AppLogic.IsAdminSite, "admin", "store")));

                    using (StreamWriter sw = File.CreateText(fn))
                    {
                        sw.WriteLine(this.SqlDebug);
                        sw.Close();
                    }
                }
                catch
                { }
            }

            bool errorHasOccurred = false;

            using (MemoryStream ms = new MemoryStream())
            {
                try
                {
                    if (HasASPDNSFNameSpace)
                    {
                        m_Transform.Transform(m_DataDocument, m_TransformArgumentList, ms);
                        m_RequiresParser = true;
                    }
                    else
                    {
                        m_Transform.Transform(m_DataDocument, null, ms);
                    }
                }
                catch (Exception ex)
                {
                    Exception logEx = ex;
                    if (ex.InnerException != null)
                    {
                        logEx = ex.InnerException;
                    }

                    errorHasOccurred = true;
                    // wrap it around another exception so that we can specify the xml package name
                    logEx = new Exception("Error has occurred on xml package: {0}".FormatWith(m_PackageName), logEx);

                    if (AppLogic.AppConfigBool("System.LoggingEnabled"))
                    {
                        SysLog.LogException(logEx, MessageTypeEnum.XmlPackageException, MessageSeverityEnum.Error);
                        Topic t = new Topic("InvalidRequest");
                        m_FinalResult = t.Contents;
                        if (ThisCustomer.IsAdminUser)
                        {
                            m_FinalResult += "" + AppLogic.GetString("invalidrequest.aspx.2", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                        }
                    }
                    else
                    {
                        // output it directly
                        m_FinalResult = "<pre>" + HttpUtility.HtmlEncode(logEx.ToString()) + "</pre>";
                    }
                    
                }

                ms.Position = 0;
                if (!errorHasOccurred)
                {
                    using (StreamReader sr = new StreamReader(ms, m_Transform.OutputSettings.Encoding))
                    {
                        m_FinalResult = sr.ReadToEnd();
                    }
                }
                ms.Close();
            }

            if (!errorHasOccurred)
            {
                if (AppLogic.AppConfigBool("XmlPackage.DumpTransform") || this.IsDebug)
                {
                    // don't let logging crash the site!
                    try
                    {
                        //String fn = CommonLogic.SafeMapPath(String.Format("{0}images/{1}_{2}.xfrm.xml", CommonLogic.IIF(AppLogic.IsAdminSite, "../", ""), m_PackageName, CommonLogic.IIF(AppLogic.IsAdminSite, "admin", "store")));
                        String fn = CommonLogic.SafeMapPath(String.Format("{0}images/{1}_{2}.xfrm.xml", CommonLogic.IIF(AppLogic.IsAdminSite, "~/{0}".FormatWith(AppLogic.AdminDir()), "~/"), m_PackageName, CommonLogic.IIF(AppLogic.IsAdminSite, "admin", "store")));
                        using (StreamWriter sw = File.CreateText(fn))
                        {
                            sw.WriteLine(XmlCommon.PrettyPrintXml(m_FinalResult));
                            sw.Close();
                        }
                    }
                    catch { }
                }
            }

            return m_FinalResult;
        }

        #endregion 

        #region Private Methods
        private string FullPackageUrl(string PackageName, int SkinID)
        {
            if (!PackageName.EndsWith(".xml.config", StringComparison.InvariantCultureIgnoreCase))
            {
                PackageName += ".xml.config";
            }

            string url = String.Empty;

            // if we have a fully specified path on input, just use it after trying to find locale specific versions:
            if (PackageName.StartsWith("//") || PackageName.IndexOf(":") != -1)
            {
                if (m_ThisCustomer != null)
                {
                    url = PackageName.Replace("xml.config", m_ThisCustomer.LocaleSetting + ".xml.config");
                }
                if (url.Length == 0 || !CommonLogic.FileExists(url))
                {
                    url = PackageName.Replace("xml.config", Localization.GetDefaultLocale() + ".xml.config");
                }
                if (url.Length == 0 || !CommonLogic.FileExists(url))
                {
                    url = PackageName;
                }
                return url;
            }

            int subdirs = CommonLogic.IIF(HttpContext.Current.Request.ApplicationPath.Length == 1, 1, HttpContext.Current.Request.ApplicationPath.Split('/').Length);
            string appdir = HttpContext.Current.Request.PhysicalApplicationPath;
            string admindir = string.Empty;
            if (AppLogic.IsAdminSite)
            {
                admindir = Path.Combine(appdir, HttpContext.Current.Request.Path.Split('/')[subdirs]);
            }
            else
            {
                admindir = Path.Combine(appdir, AppLogic.AppConfig("AdminDir"));
            }
            string rootUrl = Path.Combine(appdir, String.Format("App_Templates\\Skin_{0}\\XmlPackages", SkinID.ToString()));
            string XmlPackageDir = Path.Combine(appdir, "XmlPackages");
            string EntityMgrDir = Path.Combine(appdir, "EntityHelper");
            string AdminXmlPackageDir = Path.Combine(admindir, "XmlPackages");
            string AdminEntityMgrDir = Path.Combine(admindir, "EntityHelper");


            if (m_ThisCustomer != null)
            {
                url = Path.Combine(rootUrl, PackageName.Replace("xml.config", m_ThisCustomer.LocaleSetting + ".xml.config"));
            }
            if (url.Length == 0 || !CommonLogic.FileExists(url))
            {
                url = Path.Combine(rootUrl, PackageName.Replace("xml.config", Localization.GetDefaultLocale() + ".xml.config"));
            }
            if (url.Length == 0 || !CommonLogic.FileExists(url))
            {
                url = Path.Combine(rootUrl, PackageName);
            }
            if ((url.Length == 0 || !CommonLogic.FileExists(url)) && m_ThisCustomer != null)
            {
                url = Path.Combine(XmlPackageDir, PackageName.Replace("xml.config", m_ThisCustomer.LocaleSetting + ".xml.config"));
            }
            if (url.Length == 0 || !CommonLogic.FileExists(url))
            {
                url = Path.Combine(XmlPackageDir, PackageName.Replace("xml.config", Localization.GetDefaultLocale() + ".xml.config"));
            }
            if (url.Length == 0 || !CommonLogic.FileExists(url))
            {
                url = Path.Combine(XmlPackageDir, PackageName);
            }
            if ((url.Length == 0 || !CommonLogic.FileExists(url)) && m_ThisCustomer != null)
            {
                url = Path.Combine(EntityMgrDir, PackageName.Replace("xml.config", m_ThisCustomer.LocaleSetting + ".xml.config"));
            }
            if (url.Length == 0 || !CommonLogic.FileExists(url))
            {
                url = Path.Combine(EntityMgrDir, PackageName.Replace("xml.config", Localization.GetDefaultLocale() + ".xml.config"));
            }
            if (url.Length == 0 || !CommonLogic.FileExists(url))
            {
                url = Path.Combine(EntityMgrDir, PackageName);
            }
            // try one level up. in case we are in a subdir (e.g. on admin site, or in another directory that has code in it)
            if (AppLogic.IsAdminSite)
            {
                if ((url.Length == 0 || !CommonLogic.FileExists(url)) && m_ThisCustomer != null)
                {
                    url = Path.Combine(AdminXmlPackageDir, PackageName.Replace("xml.config", m_ThisCustomer.LocaleSetting + ".xml.config"));
                }
                if ((url.Length == 0 || !CommonLogic.FileExists(url)))
                {
                    url = Path.Combine(AdminXmlPackageDir, PackageName.Replace("xml.config", Localization.GetDefaultLocale() + ".xml.config"));
                }
                if ((url.Length == 0 || !CommonLogic.FileExists(url)))
                {
                    url = Path.Combine(AdminXmlPackageDir, PackageName);
                }
                if ((url.Length == 0 || !CommonLogic.FileExists(url)) && m_ThisCustomer != null)
                {
                    url = Path.Combine(AdminEntityMgrDir, PackageName.Replace("xml.config", m_ThisCustomer.LocaleSetting + ".xml.config"));
                }
                if ((url.Length == 0 || !CommonLogic.FileExists(url)))
                {
                    url = Path.Combine(AdminEntityMgrDir, PackageName.Replace("xml.config", Localization.GetDefaultLocale() + ".xml.config"));
                }
                if ((url.Length == 0 || !CommonLogic.FileExists(url)))
                {
                    url = Path.Combine(AdminEntityMgrDir, PackageName);
                }
            }

            return url;
        }


        private void InitializeSystemData()
        {
            String PrimaryCurrency = Localization.GetPrimaryCurrency();
            String PrimaryCurrencyDisplayLocaleFormat = Currency.GetDisplayLocaleFormat(PrimaryCurrency);
            String WebConfigLocale = Localization.GetDefaultLocale();

            using (StringReader sr = new StringReader("<System><IsAdminSite /><IsAdminSiteInt /><CustomerID /><DefaultVATSetting /><CustomerVATSetting /><UseVATSetting /><CustomerLevelID /><CustomerLevelName /><CustomerFirstName /><CustomerLastName /><CustomerFullName />" +
                "<SubscriptionExpiresOn /><CustomerRoles /><IsAdminUser /><IsSuperUser /><VAT.Enabled /><VAT.AllowCustomerToChooseSetting /><LocaleSetting /><CurrencySetting /><CurrencyDisplayLocaleFormat /><WebConfigLocaleSetting /><SqlServerLocaleSetting /><PrimaryCurrency />" +
                "<PrimaryCurrencyDisplayLocaleFormat /><Date /><Time /><SkinID /><AffiliateID /><IPAddress /><QueryStringRAW /><UseStaticLinks /><PageName /><FullPageName /><XmlPackageName /><StoreUrl /><CurrentDateTime /><CustomerIsRegistered /><StoreID /><FilterProduct />" +
				"<FilterEntity /><FilterTopic /><FilterNews /><RequestedPage /><RequestedQuerystring /><PageType /><PageID /></System>"))
            {
                using (XmlReader xr = XmlReader.Create(sr))
                {
                    m_SystemData.Load(xr);
                }
            }
            
            if (m_ThisCustomer != null)
            {
                m_SystemData.SelectSingleNode("//CustomerID").InnerText = ThisCustomer.CustomerID.ToString();

                m_SystemData.SelectSingleNode("//DefaultVATSetting").InnerText = AppLogic.AppConfig("VAT.DefaultSetting");
                AddRunTimeParam("DefaultVATSetting", AppLogic.AppConfigUSInt("VAT.DefaultSetting").ToString());

                m_SystemData.SelectSingleNode("//CustomerVATSetting").InnerText = ((int)ThisCustomer.VATSettingRAW).ToString();
                AddRunTimeParam("CustomerVATSetting", ((int)ThisCustomer.VATSettingRAW).ToString());

                m_SystemData.SelectSingleNode("//UseVATSetting").InnerText = ((int)ThisCustomer.VATSettingReconciled).ToString();
                AddRunTimeParam("UseVATSetting", ((int)ThisCustomer.VATSettingReconciled).ToString());

                m_SystemData.SelectSingleNode("//CustomerLevelID").InnerText = m_ThisCustomer.CustomerLevelID.ToString();
                AddRunTimeParam("CustomerLevelID", m_ThisCustomer.CustomerLevelID.ToString());

                m_SystemData.SelectSingleNode("//CustomerLevelName").InnerText = XmlCommon.XmlEncode(m_ThisCustomer.CustomerLevelName);
                AddRunTimeParam("CustomerLevelName", m_ThisCustomer.CustomerLevelName);

                m_SystemData.SelectSingleNode("//CustomerFirstName").InnerText = XmlCommon.XmlEncode(m_ThisCustomer.FirstName);
                AddRunTimeParam("CustomerFirstName", m_ThisCustomer.FirstName);

                m_SystemData.SelectSingleNode("//CustomerLastName").InnerText = XmlCommon.XmlEncode(m_ThisCustomer.LastName);
                AddRunTimeParam("CustomerLastName", m_ThisCustomer.LastName);

                m_SystemData.SelectSingleNode("//CustomerFullName").InnerText = XmlCommon.XmlEncode((m_ThisCustomer.FirstName + " " + m_ThisCustomer.LastName).Trim());
                AddRunTimeParam("CustomerFullName", (m_ThisCustomer.FirstName + " " + m_ThisCustomer.LastName).Trim());

                String SubExp = String.Empty;
                if (!m_ThisCustomer.SubscriptionExpiresOn.Equals(System.DateTime.MinValue))
                {
                    SubExp = Localization.ToThreadCultureShortDateString(m_ThisCustomer.SubscriptionExpiresOn);
                }
                m_SystemData.SelectSingleNode("//SubscriptionExpiresOn").InnerText = XmlCommon.XmlEncode(SubExp);
                AddRunTimeParam("SubscriptionExpiresOn", SubExp);

                String CustRoles = AppLogic.GetRoles();
                m_SystemData.SelectSingleNode("//CustomerRoles").InnerText = XmlCommon.XmlEncode(CustRoles);
                AddRunTimeParam("CustomerRoles", CustRoles);

                m_SystemData.SelectSingleNode("//IsAdminUser").InnerText = m_ThisCustomer.IsAdminUser.ToString();
                AddRunTimeParam("IsAdminUser", m_ThisCustomer.IsAdminUser.ToString());

                m_SystemData.SelectSingleNode("//IsSuperUser").InnerText = m_ThisCustomer.IsAdminSuperUser.ToString();
                AddRunTimeParam("IsSuperUser", m_ThisCustomer.IsAdminSuperUser.ToString());

                m_SystemData.SelectSingleNode("//VAT.Enabled").InnerText = AppLogic.AppConfig("VAT.Enabled");
                AddRunTimeParam("VAT.Enabled", AppLogic.AppConfigBool("VAT.Enabled").ToString());

                m_SystemData.SelectSingleNode("//VAT.AllowCustomerToChooseSetting").InnerText = AppLogic.AppConfig("VAT.AllowCustomerToChooseSetting");
                AddRunTimeParam("VAT.AllowCustomerToChooseSetting", AppLogic.AppConfigBool("VAT.AllowCustomerToChooseSetting").ToString());

                m_SystemData.SelectSingleNode("//LocaleSetting").InnerText = m_ThisCustomer.LocaleSetting;
                AddRunTimeParam("LocaleSetting", m_ThisCustomer.LocaleSetting);

                m_SystemData.SelectSingleNode("//CurrencySetting").InnerText = m_ThisCustomer.CurrencySetting;
                AddRunTimeParam("CurrencySetting", m_ThisCustomer.CurrencySetting);

                String MM = Currency.GetDisplayLocaleFormat(m_ThisCustomer.CurrencySetting);
                m_SystemData.SelectSingleNode("//CurrencyDisplayLocaleFormat").InnerText = MM;
                AddRunTimeParam("CurrencyDisplayLocaleFormat", MM);

                m_SystemData.SelectSingleNode("//IPAddress").InnerText = m_ThisCustomer.LastIPAddress;
                AddRunTimeParam("IPAddress", m_ThisCustomer.LastIPAddress);

                m_SystemData.SelectSingleNode("//AffiliateID").InnerText = m_ThisCustomer.AffiliateID.ToString();
                AddRunTimeParam("AffiliateID", m_ThisCustomer.AffiliateID.ToString());
            }
            else
            {
                int xvat = AppLogic.AppConfigUSInt("VAT.DefaultSetting");

                m_SystemData.SelectSingleNode("//DefaultVATSetting").InnerText = xvat.ToString();
                AddRunTimeParam("DefaultVATSetting", xvat.ToString());

                m_SystemData.SelectSingleNode("//CustomerVATSetting").InnerText = xvat.ToString();
                AddRunTimeParam("CustomerVATSetting", xvat.ToString());

                m_SystemData.SelectSingleNode("//UseVATSetting").InnerText = xvat.ToString();
                AddRunTimeParam("UseVATSetting", xvat.ToString());

                m_SystemData.SelectSingleNode("//CustomerLevelID").InnerText = "0";
                AddRunTimeParam("CustomerLevelID", "0");

                AddRunTimeParam("CustomerLevelName", String.Empty);

                AddRunTimeParam("CustomerFirstName", String.Empty);

                AddRunTimeParam("CustomerLastName", String.Empty);

                AddRunTimeParam("CustomerFullName", String.Empty);

                m_SystemData.SelectSingleNode("//IsAdminUser").InnerText = "False";
                AddRunTimeParam("IsAdminUser", "False");

                m_SystemData.SelectSingleNode("//IsSuperUser").InnerText = "False";
                AddRunTimeParam("IsSuperUser", "False");

                m_SystemData.SelectSingleNode("//VAT.Enabled").InnerText = AppLogic.AppConfig("VAT.Enabled");
                AddRunTimeParam("VAT.Enabled", AppLogic.AppConfigBool("VAT.Enabled").ToString());

                m_SystemData.SelectSingleNode("//VAT.AllowCustomerToChooseSetting").InnerText =AppLogic.AppConfig("VAT.AllowCustomerToChooseSetting");
                AddRunTimeParam("VAT.AllowCustomerToChooseSetting", AppLogic.AppConfigBool("VAT.AllowCustomerToChooseSetting").ToString());

                m_SystemData.SelectSingleNode("//LocaleSetting").InnerText = WebConfigLocale;
                AddRunTimeParam("LocaleSetting", WebConfigLocale);

                m_SystemData.SelectSingleNode("//CurrencySetting").InnerText = PrimaryCurrency;
                AddRunTimeParam("CurrencySetting", PrimaryCurrency);

                AddRunTimeParam("IPAddress", String.Empty);

                m_SystemData.SelectSingleNode("//AffiliateID").InnerText = "0";
                AddRunTimeParam("AffiliateID", "0");
            }

            m_SystemData.SelectSingleNode("//IsAdminSite").InnerText = AppLogic.IsAdminSite.ToString();
            AddRunTimeParam("IsAdminSite", AppLogic.IsAdminSite.ToString());

            m_SystemData.SelectSingleNode("//IsAdminSiteInt").InnerText = CommonLogic.IIF(AppLogic.IsAdminSite, "1", "0");
            AddRunTimeParam("IsAdminSiteInt", CommonLogic.IIF(AppLogic.IsAdminSite, "1", "0"));

            m_SystemData.SelectSingleNode("//WebConfigLocaleSetting").InnerText = WebConfigLocale;
            AddRunTimeParam("WebConfigLocaleSetting", WebConfigLocale);

            m_SystemData.SelectSingleNode("//SqlServerLocaleSetting").InnerText = Localization.GetSqlServerLocale();
            AddRunTimeParam("SqlServerLocaleSetting", Localization.GetSqlServerLocale());

            m_SystemData.SelectSingleNode("//PrimaryCurrency").InnerText = PrimaryCurrency;
            AddRunTimeParam("PrimaryCurrency", PrimaryCurrency);

            m_SystemData.SelectSingleNode("//PrimaryCurrencyDisplayLocaleFormat").InnerText = PrimaryCurrencyDisplayLocaleFormat;
            AddRunTimeParam("PrimaryCurrencyDisplayLocaleFormat", PrimaryCurrencyDisplayLocaleFormat);

            m_SystemData.SelectSingleNode("//Date").InnerText = Localization.ToThreadCultureShortDateString(DateTime.Now);
            AddRunTimeParam("Date", Localization.ToThreadCultureShortDateString(DateTime.Now));

            m_SystemData.SelectSingleNode("//Time").InnerText = DateTime.Now.ToShortTimeString();
            AddRunTimeParam("Time", DateTime.Now.ToShortTimeString());

            m_SystemData.SelectSingleNode("//SkinID").InnerText = SkinID.ToString();
            AddRunTimeParam("SkinID", SkinID.ToString());

            String qstr = XmlCommon.XmlEncode(HttpContext.Current.Request.QueryString.ToString());
            m_SystemData.SelectSingleNode("//QueryStringRAW").InnerText = qstr;
            AddRunTimeParam("QueryStringRAW", qstr);

            m_SystemData.SelectSingleNode("//UseStaticLinks").InnerText = AppLogic.AppConfig("UseStaticLinks");
            AddRunTimeParam("UseStaticLinks", AppLogic.AppConfigBool("UseStaticLinks").ToString());

            m_SystemData.SelectSingleNode("//XmlPackageName").InnerText = m_PackageName;
            AddRunTimeParam("XmlPackageName", m_PackageName);

            String PN = CommonLogic.GetThisPageName(false);
            m_SystemData.SelectSingleNode("//PageName").InnerText = PN;
            AddRunTimeParam("PageName", PN);

            PN = CommonLogic.GetThisPageName(true);
            m_SystemData.SelectSingleNode("//FullPageName").InnerText = PN;
            AddRunTimeParam("FullPageName", PN);

            String SURL = AppLogic.GetStoreHTTPLocation(true).ToLowerInvariant();
            SURL = SURL.Replace(AppLogic.AdminDir().ToLowerInvariant() + "/", "");
            m_SystemData.SelectSingleNode("//StoreUrl").InnerText = SURL;
            AddRunTimeParam("StoreUrl", SURL);

            m_SystemData.SelectSingleNode("//CurrentDateTime").InnerText = DateTime.Now.ToUniversalTime().ToString("ddd, dd MMM yyyy HH:mm:ss") + " GMT";

            m_SystemData.SelectSingleNode("//CustomerIsRegistered").InnerText = CommonLogic.IIF(m_ThisCustomer == null, "False", m_ThisCustomer.IsRegistered.ToString().ToLowerInvariant());
            AddRunTimeParam("CustomerIsRegistered", CommonLogic.IIF(m_ThisCustomer == null, "False", m_ThisCustomer.IsRegistered.ToString().ToLowerInvariant()));

            m_SystemData.SelectSingleNode("//StoreID").InnerText = AppLogic.StoreID().ToString();
            AddRunTimeParam("StoreID", AppLogic.StoreID().ToString());

            m_SystemData.SelectSingleNode("//FilterProduct").InnerText = AppLogic.GlobalConfigBool("AllowProductFiltering").ToString();
            AddRunTimeParam("FilterProduct", AppLogic.GlobalConfigBool("AllowProductFiltering").ToString());

            m_SystemData.SelectSingleNode("//FilterEntity").InnerText = AppLogic.GlobalConfigBool("AllowEntityFiltering").ToString();
            AddRunTimeParam("FilterEntity", AppLogic.GlobalConfigBool("AllowEntityFiltering").ToString());

            m_SystemData.SelectSingleNode("//FilterTopic").InnerText = AppLogic.GlobalConfigBool("AllowTopicFiltering").ToString();
            AddRunTimeParam("FilterTopic", AppLogic.GlobalConfigBool("AllowTopicFiltering").ToString());

            m_SystemData.SelectSingleNode("//FilterNews").InnerText = AppLogic.GlobalConfigBool("AllowNewsFiltering").ToString();
            AddRunTimeParam("FilterNews", AppLogic.GlobalConfigBool("AllowNewsFiltering").ToString());

			m_SystemData.SelectSingleNode("//PageType").InnerText = AppLogic.GetCurrentPageType();
			AddRunTimeParam("PageType", AppLogic.GetCurrentPageType());

			m_SystemData.SelectSingleNode("//PageID").InnerText = AppLogic.GetCurrentPageID();
			AddRunTimeParam("PageID", AppLogic.GetCurrentPageID());
            
            if (HttpContext.Current.Items.Contains("RequestedPage"))
            {
                m_SystemData.SelectSingleNode("//RequestedPage").InnerText = HttpContext.Current.Items["RequestedPage"].ToString();
            }

            if (HttpContext.Current.Items.Contains("RequestedQuerystring"))
            {
                m_SystemData.SelectSingleNode("//RequestedQuerystring").InnerText = HttpContext.Current.Items["RequestedQuerystring"].ToString();
            }

            m_DataDocument.DocumentElement.AppendChild(m_DataDocument.ImportNode(m_SystemData.DocumentElement, true));

            //Querystring Params
            m_DataDocument.DocumentElement.AppendChild(m_DataDocument.CreateNode(XmlNodeType.Element, "QueryString", ""));
            for (int i = 0; i <= HttpContext.Current.Request.QueryString.Count - 1; i++)
            {
                try
                {
                    String ParamName = HttpContext.Current.Request.QueryString.Keys[i].ToLowerInvariant();
                    String ParamValue = HttpContext.Current.Request.QueryString[HttpContext.Current.Request.QueryString.Keys[i]].ToString();
                    if (ParamName.Length != 0)
                    {
                        m_DataDocument.SelectSingleNode("//QueryString").AppendChild(m_DataDocument.CreateNode(XmlNodeType.Element, ParamName, ""));
                        m_DataDocument.SelectSingleNode("/root/QueryString/" + ParamName).InnerText = CommonLogic.IIF(ParamValue.Trim().Equals(string.Empty), "", ParamValue);
                    }
                }
                catch (Exception ex)
                {
                    string err = ex.Message;
                }
            }

            //Form Params
            m_DataDocument.DocumentElement.AppendChild(m_DataDocument.CreateNode(XmlNodeType.Element, "Form", ""));
            for (int i = 0; i <= HttpContext.Current.Request.Form.Count - 1; i++)
            {
                try
                {
                    String ParamName = HttpContext.Current.Request.Form.Keys[i].ToLowerInvariant();
                    String ParamValue = HttpContext.Current.Request.Form[HttpContext.Current.Request.Form.Keys[i]].ToString();
                    if (ParamName.Length != 0)
                    {
                        m_DataDocument.SelectSingleNode("//Form").AppendChild(m_DataDocument.CreateNode(XmlNodeType.Element, ParamName, ""));
                        m_DataDocument.SelectSingleNode("/root/Form/" + ParamName).InnerText = ParamValue;
                    }
                }
                catch { }
            }

            
            //Session Params (there should be none unless some custom work has been done)
            m_DataDocument.DocumentElement.AppendChild(m_DataDocument.CreateNode(XmlNodeType.Element, "Session", ""));
            if (HttpContext.Current.Session != null)
            {
                for (int i = 0; i <= HttpContext.Current.Session.Count - 1; i++)
                {
                    try
                    {
                        String ParamName = HttpContext.Current.Session.Keys[i].ToLowerInvariant();
                        String ParamValue = CommonLogic.SessionNotServerFarmSafe(ParamName);
                        if (ParamName.Length != 0)
                        {
                            m_DataDocument.SelectSingleNode("//Session").AppendChild(m_DataDocument.CreateNode(XmlNodeType.Element, ParamName, ""));
                            m_DataDocument.SelectSingleNode("/root/Session/" + ParamName).InnerText = ParamValue;
                        }
                    }
                    catch { }

                }
            }

            // Cookie params:
            m_DataDocument.DocumentElement.AppendChild(m_DataDocument.CreateNode(XmlNodeType.Element, "Cookies", ""));
            if (HttpContext.Current.Request.Cookies != null)
            {
                for (int i = 0; i <= HttpContext.Current.Request.Cookies.Count - 1; i++)
                {
                    String ParamName = HttpContext.Current.Request.Cookies.Keys[i];
                    String ParamValue = CommonLogic.CookieCanBeDangerousContent(ParamName, true);

                    if (ParamName.Length != 0 && ParamName.Equals("asp.net_sessionid", StringComparison.InvariantCultureIgnoreCase) == false && ParamName.IndexOf("aspxauth", StringComparison.InvariantCultureIgnoreCase) == -1)
                    {
                        try
                        {
                            if (m_DataDocument.SelectSingleNode("/root/Cookies/" + ParamName) == null)
                            {
                                m_DataDocument.SelectSingleNode("//Cookies").AppendChild(m_DataDocument.CreateNode(XmlNodeType.Element, ParamName, ""));
                                m_DataDocument.SelectSingleNode("/root/Cookies/" + ParamName).InnerText = ParamValue;
                            }
                        }
                        catch { }
                    }
                }
            }
            


            // ServerVariables params (just the useful ones):
            m_DataDocument.DocumentElement.AppendChild(m_DataDocument.CreateNode(XmlNodeType.Element, "ServerVariables", ""));
            foreach (String s in ro_ServerVariablesList)
            {
                m_DataDocument.SelectSingleNode("//ServerVariables").AppendChild(m_DataDocument.CreateNode(XmlNodeType.Element, s, ""));
            }
            foreach (String s in ro_ServerVariablesList)
            {
                m_DataDocument.SelectSingleNode("/root/ServerVariables/" + s).InnerText = CommonLogic.ServerVariables(s);
            }


            //Runtime params:
            m_DataDocument.DocumentElement.AppendChild(m_DataDocument.CreateNode(XmlNodeType.Element, "Runtime", ""));
            IDictionaryEnumerator en = m_RuntimeParameters.GetEnumerator();
            while (en.MoveNext())
            {
                String ParamName = en.Key.ToString();
                m_DataDocument.SelectSingleNode("//Runtime").AppendChild(m_DataDocument.CreateNode(XmlNodeType.Element, ParamName, ""));
            }
            en = m_RuntimeParameters.GetEnumerator();
            while (en.MoveNext())
            {
                String ParamValue = en.Value.ToString();
                String ParamName = en.Key.ToString();
                m_DataDocument.SelectSingleNode("/root/Runtime/" + ParamName).InnerText = ParamValue;
            }
        }

        private void GetSqlData(String OnlyRunNamedQuery)
        {
            SqlConnection cn = new SqlConnection(DB.GetDBConn());
            cn.Open();

            String sql = String.Empty;
            StringBuilder xml = new StringBuilder("");
            StringBuilder tmpXml = new StringBuilder("");
            int numrows = 0;
            bool firstquery = true;
            string queryname = String.Empty;
            string replaceTag = String.Empty;

            string replaceType = String.Empty;
            string replaceParamName = String.Empty;
            string defaultValue = String.Empty;
            string validationpattern = String.Empty;
            string replaceValue = String.Empty;
            string RowElementName = String.Empty;
            XmlNodeList qryNodes;
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = cn;
            cmd.CommandType = CommandType.Text;
            SqlDataReader dr;

            if (m_PackageDocument != null)
            {
                if (OnlyRunNamedQuery != null && OnlyRunNamedQuery.Length > 0)
                {
                    qryNodes = m_PackageDocument.SelectNodes("/package/query[@name='" + OnlyRunNamedQuery + "']");
                }
                else
                {
                    qryNodes = m_PackageDocument.SelectNodes("/package/query");
                }

                foreach (XmlNode n in qryNodes)
                {
                    //determine if the query is going to be run
                    XmlNode runif = n.Attributes["runif"];
                    if (runif != null && runif.InnerText.Trim() != "")
                    {
                        string runifparam = runif.InnerText;
                        if (CommonLogic.IsStringNullOrEmpty(CommonLogic.ParamsCanBeDangerousContent(runifparam)) && 
                            CommonLogic.IsStringNullOrEmpty(AppLogic.AppConfig(runifparam)) && 
                            !m_RuntimeParameters.Contains(runifparam))
                        {
                            continue;
                        }
                    }

                    queryname = n.Attributes["name"].InnerText;
                    if (n.Attributes["rowElementName"] == null || n.Attributes["rowElementName"].InnerText.Trim() == "")
                    {
                        RowElementName = queryname + "row";
                    }
                    else
                    {
                        RowElementName = n.Attributes["rowElementName"].InnerText;
                    }


                    cmd.Parameters.Clear();
                    cmd.CommandText = "";

                    sql = n.FirstChild.InnerText.ToString();
                    XmlNodeList replacenodes = n.SelectNodes("querystringreplace");
                    foreach (XmlNode rn in replacenodes)
                    {
                        replaceTag = rn.Attributes["replaceTag"].InnerText;
                        replaceType = rn.Attributes["replacetype"].InnerText.ToLowerInvariant();
                        replaceParamName = rn.Attributes["replaceparamname"].InnerText;
                        defaultValue = rn.Attributes["defvalue"].InnerText;
                        validationpattern = rn.Attributes["validationpattern"].InnerText.Trim();

                        switch (replaceType)
                        {
                            case "request":
                                {
                                    if (CommonLogic.ParamsCanBeDangerousContent(replaceParamName) != "")
                                    {

                                        replaceValue = CommonLogic.ParamsCanBeDangerousContent(replaceParamName);
                                    }
                                    else
                                    {
                                        replaceValue = defaultValue;
                                    }

                                    break;
                                }

                            case "appconfig":
                                {
                                    replaceValue = AppLogic.AppConfig(replaceParamName);
                                    break;
                                }
                            case "webconfig":
                                {
                                    replaceValue = CommonLogic.Application(replaceParamName);
                                    break;
                                }
                            case "runtime":
                                {
                                    replaceValue = GetRuntimeParamValue(replaceParamName);
                                    break;
                                }
                            case "system":
                                {
                                    replaceValue = m_SystemData.SelectSingleNode("/System/"+ replaceParamName).InnerText;
                                    break;
                                }
                        }
                        if (validationpattern.Length > 0)
                        {
                            if (Regex.IsMatch(replaceValue, validationpattern, RegexOptions.Compiled | RegexOptions.IgnoreCase))
                            {
                                sql = sql.Replace(replaceTag, replaceValue);
                            }
                            else
                            {
                                throw new Exception("String Replace parameter " + replaceTag + " failed validation");
                            }
                        }
                        else
                        {
                            sql = sql.Replace(replaceTag, replaceValue);
                        }
                    }

                    cmd.CommandText = sql;

                    XmlNodeList qp = n.SelectNodes("queryparam");
                    foreach (XmlNode pn in qp)
                    {
                        //create a parameter of the appropriate type
                        cmd.Parameters.Add(CreateParameter(pn));
                    }

                    string m_DebugParamsDeclare = string.Empty;
                    string m_DebugParamsValues = string.Empty;
                    if (m_IsDebug)
                    {
                        m_SqlDebug += "************************************  SQL Statement and parameters for query " + n.Attributes["name"].InnerText + "  ************************************" + Environment.NewLine + Environment.NewLine;
                        foreach (SqlParameter sp in cmd.Parameters)
                        {
                            m_DebugParamsDeclare += "declare " + sp.ParameterName + " " + sp.SqlDbType.ToString() + Environment.NewLine;
                            string paramdatatype = sp.SqlDbType.ToString().ToLowerInvariant();
                            if (paramdatatype == "varchar" || paramdatatype == "char" || paramdatatype == "datetime" || paramdatatype == "smalldatetime" || paramdatatype == "nchar" || paramdatatype == "nvarchar" || paramdatatype == "text" || paramdatatype == "ntext" || paramdatatype == "uniqueidentifier")
                                m_DebugParamsValues += "set " + sp.ParameterName + " = '" + sp.Value.ToString() + "'" + Environment.NewLine;
                            else
                                m_DebugParamsValues += "set " + sp.ParameterName + " = " + sp.Value.ToString() + Environment.NewLine;
                        }
                        m_SqlDebug += m_DebugParamsDeclare + Environment.NewLine + m_DebugParamsValues + Environment.NewLine + Environment.NewLine;
                        m_SqlDebug += cmd.CommandText.Trim() + Environment.NewLine + Environment.NewLine;
                    }

                    dr = cmd.ExecuteReader();

                    if (n.Attributes["retType"] != null && n.Attributes["retType"].InnerText.Equals("xml", StringComparison.InvariantCultureIgnoreCase))
                    {
                        tmpXml.Append("<");
                        tmpXml.Append(queryname);
                        tmpXml.Append(">");
                        while (dr.Read())
                        {
                            tmpXml.Append(dr.GetString(0));
                        }
                        tmpXml.Append("</");
                        tmpXml.Append(queryname);
                        tmpXml.Append(">");

                        int resultset = 1;
                        while (dr.NextResult())
                        {
                            resultset++;
                            tmpXml.Append("<");
                            tmpXml.Append(queryname);
                            tmpXml.Append(resultset.ToString());
                            tmpXml.Append(">");
                            while (dr.Read())
                            {
                                tmpXml.Append(dr.GetString(0));
                            }
                            tmpXml.Append("</");
                            tmpXml.Append(queryname);
                            tmpXml.Append(resultset.ToString());
                            tmpXml.Append(">");
                        }
                    }
                    else
                    {
                        numrows = DB.GetXml(dr, queryname, RowElementName, (sql.IndexOf("aspdnsf_PageQuery") > 0), ref tmpXml);
                    }
                    if (!dr.IsClosed)
                    {
                        dr.Close();
                    }
                    dr.Dispose();

                    if (firstquery)
                    {
                        m_NumRows = numrows;
                        firstquery = false;
                    }

                    //transform the query using the specified querytransform
                    XmlNode qt = n.SelectSingleNode("querytransform");
                    if (qt != null)
                    {
                        XslCompiledTransform xsl = new XslCompiledTransform();
                        StringWriter sw = new StringWriter();
                        XmlDocument x = new XmlDocument();
                        x.LoadXml(qt.InnerXml);
                        xsl.Load(x, null, null);
                        XmlDocument xd = new XmlDocument();
                        xd.LoadXml("<root>" + m_SystemData.OuterXml + tmpXml.ToString() + "</root>");
                        xsl.Transform(xd, null, sw);
                        xml.Append(sw.ToString());
                    }
                    else
                    {
                        xml.Append(tmpXml.ToString());
                    }
                    tmpXml.Length = 0;
                }

                // execute runtime query
                IDictionaryEnumerator en = m_RuntimeQueries.GetEnumerator();
                while (en.MoveNext())
                {
                    cmd.Parameters.Clear();
                    String sqlQuery = en.Value.ToString();
                    cmd.CommandText = sqlQuery;
                    dr = cmd.ExecuteReader();
                    numrows = DB.GetXml(dr, readonly_DefaultUserQueryName, readonly_DefaultUserQueryName + "row", (sqlQuery.IndexOf("aspdnsf_PageQuery") > 0), ref xml);
                }

                cn.Close();
                cmd.Dispose();
                cn.Dispose();

                if (xml.Length > 0)
                {
                    StringBuilder sb = new StringBuilder("<root>" + xml.ToString() + "</root>");
                    XmlDocument xdoc = new XmlDocument();
                    xdoc.LoadXml(sb.ToString());

                    foreach (XmlNode x in xdoc.DocumentElement.ChildNodes)
                    {
                        m_DataDocument.DocumentElement.AppendChild(m_DataDocument.ImportNode(x, true));
                    }
             
                }
            }
            else
            {
                cn.Close();
                cmd.Dispose();
                cn.Dispose();
            }
        }

        private SqlParameter CreateParameter(XmlNode pn)
        {
            string paramVal = String.Empty;
            SqlParameter p = new SqlParameter();
            p.ParameterName = pn.Attributes["paramname"].InnerText;

            string requestparamname = pn.Attributes["requestparamname"].InnerText;
            string sqlDataTypeName = pn.Attributes["sqlDataType"].InnerText.ToLowerInvariant();
            string validationpattern = pn.Attributes["validationpattern"].InnerText.Trim();
            string defvalue = pn.Attributes["defvalue"].InnerText;

            //get the parameter value
            switch (pn.Attributes["paramtype"].InnerText.ToLowerInvariant())
            {
                case "request":
                    paramVal = HttpContext.Current.GetRequestData().StringValue(requestparamname);
                    
                    if (paramVal == "")
                    {
                        paramVal = CommonLogic.FormCanBeDangerousContent(requestparamname);
                    }
                    else
                    {
                        break;
                    }

                    if (paramVal == "")
                    {
                        paramVal = CommonLogic.CookieCanBeDangerousContent(requestparamname, true);
                    }
                    else
                    {
                        break;
                    }

                    if (paramVal == "")
                    {
                        paramVal = CommonLogic.ServerVariables(requestparamname);
                    }
                    else
                    {
                        break;
                    }
                    break;
                case "webconfig":
                    paramVal = CommonLogic.Application(requestparamname);
                    break;
                case "appconfig":
                    paramVal = AppLogic.AppConfig(requestparamname);
                    break;
                case "runtime":
                    paramVal = GetRuntimeParamValue(requestparamname);
                    break;
                case "system":
                    paramVal = m_SystemData.SelectSingleNode("/System/" + requestparamname).InnerText;
                    break;
                case "form":
                    try
                    {
                        paramVal = m_DataDocument.SelectSingleNode("/root/Form/" + requestparamname).InnerText;
                    }
                    catch (Exception ex)
                    {
                        SysLog.LogMessage(String.Format("Error finding form key {0} for XmlPackage {1}.", requestparamname, this.m_PackageName), 
                                            ex.Message.ToString(), 
                                            MessageTypeEnum.XmlPackageException, 
                                            MessageSeverityEnum.Message);
                    }
                    break;
                case "xpath":
                    paramVal = "";
                    XmlNode x = m_DataDocument.SelectSingleNode(requestparamname);
                    if (paramVal != null)
                    {
                        paramVal = x.InnerText;
                    }
                    else
                    {
                        throw new Exception("Error executing xpath statement (" + requestparamname + ") in query param");
                    }
                    break;
            }
            if (paramVal.Trim().Length == 0)
            {
                paramVal = defvalue;
            }

            try
            {
                if (validationpattern.Length > 0)
                {
                    if (!Regex.IsMatch(paramVal, validationpattern, RegexOptions.Compiled | RegexOptions.IgnoreCase))
                    {
                        throw new Exception("Query parameter failed validation: paramvalue=" + paramVal + "; validationpattern=" + validationpattern);
                    }
                }

                switch (sqlDataTypeName)
                {
                    case "bigint":
                        p.SqlDbType = SqlDbType.BigInt;
                        p.Value = Convert.ToInt64(paramVal);
                        break;

                    case "bit":
                        p.SqlDbType = SqlDbType.Bit;
                        if (paramVal.Equals("null", StringComparison.InvariantCultureIgnoreCase)) p.Value = DBNull.Value;
                        else p.Value = Convert.ToBoolean(paramVal);
                        break;

                    case "char":
                        p.SqlDbType = SqlDbType.Char;
                        if (paramVal.Equals("null", StringComparison.InvariantCultureIgnoreCase)) p.Value = DBNull.Value;
                        else p.Value = paramVal;
                        break;

                    case "datetime":
                        p.SqlDbType = SqlDbType.DateTime;
                        if (paramVal.Equals("null", StringComparison.InvariantCultureIgnoreCase)) p.Value = DBNull.Value;
                        else p.Value = Convert.ToDateTime(paramVal);
                        break;

                    case "decimal":
                        p.SqlDbType = SqlDbType.Decimal;
                        if (paramVal.Equals("null", StringComparison.InvariantCultureIgnoreCase)) p.Value = DBNull.Value;
                        else p.Value = Convert.ToDecimal(paramVal);
                        break;

                    case "float":
                        p.SqlDbType = SqlDbType.Float;
                        if (paramVal.Equals("null", StringComparison.InvariantCultureIgnoreCase)) p.Value = DBNull.Value;
                        else p.Value = Convert.ToDouble(paramVal);
                        break;

                    case "int":
                        p.SqlDbType = SqlDbType.Int;
                        if (paramVal.Equals("null", StringComparison.InvariantCultureIgnoreCase)) p.Value = DBNull.Value;
                        else p.Value = Convert.ToInt32(paramVal);
                        break;

                    case "money":
                        p.SqlDbType = SqlDbType.Money;
                        if (paramVal.Equals("null", StringComparison.InvariantCultureIgnoreCase)) p.Value = DBNull.Value;
                        else p.Value = Convert.ToDecimal(paramVal);
                        break;

                    case "nchar":
                        p.SqlDbType = SqlDbType.NChar;
                        if (paramVal.Equals("null", StringComparison.InvariantCultureIgnoreCase)) p.Value = DBNull.Value;
                        else p.Value = paramVal;
                        break;

                    case "ntext":
                        p.SqlDbType = SqlDbType.NText;
                        if (paramVal.Equals("null", StringComparison.InvariantCultureIgnoreCase)) p.Value = DBNull.Value;
                        else p.Value = paramVal;
                        break;

                    case "nvarchar":
                        p.SqlDbType = SqlDbType.NVarChar;
                        if (paramVal.Equals("null", StringComparison.InvariantCultureIgnoreCase)) p.Value = DBNull.Value;
                        else p.Value = paramVal;
                        break;

                    case "real":
                        p.SqlDbType = SqlDbType.Real;
                        if (paramVal.Equals("null", StringComparison.InvariantCultureIgnoreCase)) p.Value = DBNull.Value;
                        else p.Value = Convert.ToSingle(paramVal);
                        break;

                    case "smalldatetime":
                        p.SqlDbType = SqlDbType.SmallDateTime;
                        if (paramVal.Equals("null", StringComparison.InvariantCultureIgnoreCase)) p.Value = DBNull.Value;
                        else p.Value = Convert.ToDateTime(paramVal);
                        break;

                    case "smallint":
                        p.SqlDbType = SqlDbType.SmallInt;
                        if (paramVal.Equals("null", StringComparison.InvariantCultureIgnoreCase)) p.Value = DBNull.Value;
                        else p.Value = Convert.ToInt16(paramVal);
                        break;

                    case "smallmoney":
                        p.SqlDbType = SqlDbType.SmallMoney;
                        if (paramVal.Equals("null", StringComparison.InvariantCultureIgnoreCase)) p.Value = DBNull.Value;
                        else p.Value = Convert.ToDecimal(paramVal);
                        break;

                    case "text":
                        p.SqlDbType = SqlDbType.Text;
                        if (paramVal.Equals("null", StringComparison.InvariantCultureIgnoreCase)) p.Value = DBNull.Value;
                        else p.Value = paramVal;
                        break;

                    case "tinyint":
                        p.SqlDbType = SqlDbType.TinyInt;
                        if (paramVal.Equals("null", StringComparison.InvariantCultureIgnoreCase)) p.Value = DBNull.Value;
                        else p.Value = Convert.ToByte(paramVal);
                        break;

                    case "uniqueidentifier":
                        p.SqlDbType = SqlDbType.UniqueIdentifier;
                        if (paramVal.Equals("null", StringComparison.InvariantCultureIgnoreCase)) p.Value = DBNull.Value;
                        else p.Value = new Guid(paramVal);
                        break;

                    case "varchar":
                        p.SqlDbType = SqlDbType.VarChar;
                        if (paramVal.Equals("null", StringComparison.InvariantCultureIgnoreCase)) p.Value = DBNull.Value;
                        else p.Value = paramVal;
                        break;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Invalid parameter specification (" + ex.Message + ")");
            }
            return p;
        }             

        private void GetWebData()
        {
            StringBuilder xml = new StringBuilder("");
            StringBuilder tmpXml = new StringBuilder("");
            string url = String.Empty;
            string queryname = String.Empty;
            string rettype = String.Empty;
            string replaceTag = String.Empty;
            string replaceType = String.Empty;
            string replaceParamName = String.Empty;
            string defaultValue = String.Empty;
            string validationpattern = String.Empty;
            string replaceValue = String.Empty;
            XmlNodeList qryNodes;


            if (m_PackageDocument != null)
            {
                qryNodes = m_PackageDocument.SelectNodes("//webquery");

                foreach (XmlNode n in qryNodes)
                {
                    XmlNode runif = n.Attributes["runif"];
                    if (runif != null && runif.InnerText.Trim() != "")
                    {
                        string runifparam = runif.InnerText;
                        if (CommonLogic.IsStringNullOrEmpty(CommonLogic.ParamsCanBeDangerousContent(runifparam)) && 
                            CommonLogic.IsStringNullOrEmpty(AppLogic.AppConfig(runifparam)))
                        {
                            continue;
                        }
                    }

                    XmlNode method = n.Attributes["method"];
                    string sendmethod = "get";
                    if (method != null && method.InnerText.Trim() != "")
                    {
                        sendmethod = method.InnerText;
                    }

                    queryname = n.Attributes["name"].InnerText;
                    rettype = n.Attributes["RetType"].InnerText;
                    url = n.SelectSingleNode("url").InnerText;
                    
                    int timeout = 30;
                    if (n.Attributes["timeout"] != null && CommonLogic.IsInteger(n.Attributes["timeout"].InnerText))
                    {
                        timeout = Convert.ToInt32(n.Attributes["timeout"].InnerText);
                    }
                    
                    if (url.Length == 0)
                    {
                        continue;
                    }

                    XmlNodeList replacenodes = n.SelectNodes("querystringreplace");
                    foreach (XmlNode rn in replacenodes)
                    {
                        replaceTag = rn.Attributes["replaceTag"].InnerText;
                        replaceType = rn.Attributes["replacetype"].InnerText.ToLowerInvariant();
                        replaceParamName = rn.Attributes["replaceparamname"].InnerText;
                        defaultValue = rn.Attributes["defvalue"].InnerText;
                        validationpattern = rn.Attributes["validationpattern"].InnerText.Trim();

                        switch (replaceType)
                        {
                            case "request":
                                if (CommonLogic.ParamsCanBeDangerousContent(replaceParamName) != "")
                                {
                                    replaceValue = CommonLogic.ParamsCanBeDangerousContent(replaceParamName);
                                }
                                else
                                {
                                    replaceValue = defaultValue;
                                }
                                break;
                            case "appconfig":
                                replaceValue = AppLogic.AppConfig(replaceParamName);
                                break;
                            case "webconfig":
                                replaceValue = CommonLogic.Application(replaceParamName);
                                break;
                            case "runtime":
                                replaceValue = GetRuntimeParamValue(replaceParamName);
                                break;
                            case "system":
                                replaceValue = m_SystemT.Rows[0][replaceParamName].ToString();
                                break;
                        }
                        if (validationpattern.Length > 0)
                        {
                            if (Regex.IsMatch(replaceValue, validationpattern, RegexOptions.Compiled | RegexOptions.IgnoreCase))
                            {
                                url = url.Replace(replaceTag, replaceValue);
                            }
                            else
                            {
                                throw new Exception("String Replace parameter " + replaceTag + " failed validation");
                            }
                        }
                        else
                        {
                            url = url.Replace(replaceTag, replaceValue);
                        }
                    }

                    if (sendmethod == "post")
                    {
                        XmlNode postdataNode = n.SelectSingleNode("postdata");
                        string postdata = "";
                        if (postdataNode != null)
                        {
                            string paramtype = postdataNode.Attributes["paramtype"].InnerText;
                            string paramname = postdataNode.Attributes["paramname"].InnerText.ToLowerInvariant();
                            switch (paramtype)
                            {
                                case "request":
                                    postdata = CommonLogic.ParamsCanBeDangerousContent(paramname);
                                    break;
                                case "appconfig":
                                    postdata = AppLogic.AppConfig(paramname);
                                    break;
                                case "webconfig":
                                    postdata = CommonLogic.Application(paramname);
                                    break;
                                case "runtime":
                                    postdata = GetRuntimeParamValue(paramname);
                                    break;
                                case "system":
                                    postdata = m_SystemT.Rows[0][paramname].ToString();
                                    break;
                            }
                        }
                        tmpXml.Append(CommonLogic.AspHTTP(url, timeout, postdata));
                    }
                    else
                    {
                        tmpXml.Append(CommonLogic.AspHTTP(url, timeout));
                    }

                    //transform the results using the specified querytransform
                    if (rettype == "xml")
                    {
                        XmlDocument xdoc = new XmlDocument();
                        try
                        {
                            xdoc.LoadXml(tmpXml.ToString());
                            XmlNode qt = n.SelectSingleNode("querytransform");
                            if (qt != null)
                            {
                                XslCompiledTransform xsl = new XslCompiledTransform();
                                StringWriter sw = new StringWriter();
                                XmlDocument x = new XmlDocument();
                                x.LoadXml(qt.InnerXml);
                                xsl.Load(x, null, null);
                                XmlDocument xd = new XmlDocument();
                                xd.LoadXml("<root>" + m_SystemData.OuterXml + xdoc.DocumentElement.OuterXml + "</root>");
                                xsl.Transform(xd, null, sw);
                                xml.Append(sw.ToString());
                            }
                            else
                            {
                                xml.Append("<" + queryname + ">" + xdoc.DocumentElement.OuterXml + "</" + queryname + ">");
                            }
                        }
                        catch
                        {
                            xml.Append("<" + queryname + ">" + "<![CDATA[" + tmpXml.ToString() + "]]>" + "</" + queryname + ">");
                        }
                    }
                    else
                    {
                        xml.Append("<" + queryname + ">" + "<![CDATA[" + tmpXml.ToString() + "]]>" + "</" + queryname + ">");
                    }
                    tmpXml.Length = 0;
                }

                if (xml.Length > 0)
                {
                    using (StringReader sr = new StringReader(xml.ToString()))
                    {
                        using (XmlReader xr = XmlReader.Create(sr))
                        {
                            XmlDocument xdoc = new XmlDocument();
                            xdoc.Load(xr);
                            m_DataDocument.DocumentElement.AppendChild(m_DataDocument.ImportNode(xdoc.DocumentElement, true));
                        }
                    }
                }

            }
        }

        private void AddRunTimeQuery(String QueryName, String UserSpecifiedQuery)
        {
            if (m_RuntimeQueries.Contains(QueryName))
            {
                m_RuntimeQueries.Remove(QueryName);
            }
            m_RuntimeQueries.Add(QueryName, UserSpecifiedQuery);
        }

        private void AddRunTimeParam(String ParameterName, String ParameterValue)
        {
            if (m_RuntimeParameters.Contains(ParameterName))
            {
                m_RuntimeParameters.Remove(ParameterName);
            }
            m_RuntimeParameters.Add(ParameterName, ParameterValue);
        }

        private void AddPackageNode(XmlNode xNode)
        {
            string packageName = xNode.Attributes["name"].InnerText;
            using (StringReader sr = new StringReader(AppLogic.RunXmlPackage(packageName, null, ThisCustomer, SkinID, String.Empty, String.Empty, false, false)))
            {
                using (XmlReader xr = XmlReader.Create(sr))
                {
                    XmlDocument xNew = new XmlDocument();
                    xNew.Load(xr);
                    XmlNode xNewChild = xNode.OwnerDocument.ImportNode(xNew.DocumentElement, true);
                    xNode.ParentNode.ReplaceChild(xNewChild, xNode);
                }
            }
        }

        private string GetRuntimeParamValue(String FindParamName)
        {
            IDictionaryEnumerator en = m_RuntimeParameters.GetEnumerator();
            while (en.MoveNext())
            {
                String ParamName = en.Key.ToString();
                String ParamValue = en.Value.ToString();

                if (ParamName.Equals(FindParamName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return ParamValue;
                }
            }
            return String.Empty;
        }

        private string AddParameterNode(XmlNode pNode, string sqlQuery)
        {
            if (pNode.Attributes["name"] != null)
            {
                string paramName = pNode.Attributes["name"].InnerText;
                string paramValue = String.Empty;
                if (pNode.Attributes["aspdnsf:appconfig"] != null)
                {
                    paramValue = AppLogic.AppConfig(pNode.Attributes["aspdnsf:appconfig"].InnerText);
                }
                if (pNode.Attributes["aspdnsf:params"] != null)
                {
                    paramValue = CommonLogic.ParamsCanBeDangerousContent(pNode.Attributes["aspdnsf:params"].InnerText);
                }
                if (pNode.Attributes["aspdnsf:runtime"] != null)
                {
                    paramValue = GetRuntimeParamValue(paramName); // pNode.Attributes["aspdnsf:runtime"].InnerText;
                }
                // try getting a default from the Xsl File (e.g. <xsl:param ...>defaultvalue</xsl:param>
                if (paramValue.Length == 0)
                {
                    paramValue = pNode.InnerText;
                }
                pNode.InnerText = paramValue;
                if (paramName.Length != 0)
                {
                    sqlQuery = sqlQuery.Replace("{" + paramName + "}", paramValue); //Get the value from Params
                }
            }
            return sqlQuery;
        }
        private void ProcessSESettings()
        {
            XmlNode SESettings = m_PackageDocument.SelectSingleNode("/package/SearchEngineSettings");
            if (SESettings != null)
            {
                XslCompiledTransform t;
                XmlDocument x = new XmlDocument();
                StringWriter xsw;
                XmlNode XPathResult;

                XmlNode SectionTitle = SESettings.SelectSingleNode("SectionTitle");
                XmlNode SETitle = SESettings.SelectSingleNode("SETitle");
                XmlNode SEKeywords = SESettings.SelectSingleNode("SEKeywords");
                XmlNode SEDescription = SESettings.SelectSingleNode("SEDescription");
                XmlNode SENoScript = SESettings.SelectSingleNode("SENoScript");

                if (SectionTitle != null)
                {
                    if (SectionTitle.Attributes["actionType"] == null)
                    {
                        throw new Exception("actionType attribute not specified for SectionTitle element");
                    }
                    switch (SectionTitle.Attributes["actionType"].InnerText)
                    {
                        case "xpath":
                            string xpath = SectionTitle.InnerText;
                            XPathResult = m_DataDocument.SelectSingleNode(xpath);
                            if (XPathResult != null)
                            {
                                m_SectionTitle = XPathResult.InnerText;
                            }
                            break;

                        case "transform":
                            xsw = new StringWriter();
                            t = new XslCompiledTransform();
                            x.LoadXml(SectionTitle.InnerXml);
                            t.Load(x);
                            t.Transform(m_DataDocument, m_TransformArgumentList, xsw);
                            m_SectionTitle = xsw.ToString();
                            break;

                        case "text":
                            m_SectionTitle = SectionTitle.InnerText.Replace("\r\n", "").Trim();
                            break;
                    }


                }

                if (SETitle != null)
                {
                    if (SETitle.Attributes["actionType"] == null)
                    {
                        throw new Exception("actionType attribute not specified for SETitle element");
                    }
                    switch (SETitle.Attributes["actionType"].InnerText)
                    {
                        case "xpath":
                            string xpath = SETitle.InnerText;
                            XPathResult = m_DataDocument.SelectSingleNode(xpath);
                            if (XPathResult != null)
                            {
                                m_SETitle = XPathResult.InnerText;
                            }
                            break;

                        case "transform":
                            xsw = new StringWriter();
                            t = new XslCompiledTransform();
                            x.LoadXml(SETitle.InnerXml);
                            t.Load(x);
                            t.Transform(m_DataDocument, m_TransformArgumentList, xsw);
                            m_SETitle = xsw.ToString();
                            break;

                        case "text":
                            m_SETitle = SETitle.InnerText.Replace("\r\n", "").Trim();
                            break;
                    }
                }

                if (SEKeywords != null)
                {
                    if (SEKeywords.Attributes["actionType"] == null)
                    {
                        throw new Exception("actionType attribute not specified for SEKeywords element");
                    }
                    switch (SEKeywords.Attributes["actionType"].InnerText)
                    {
                        case "xpath":
                            string xpath = SEKeywords.InnerText;
                            XPathResult = m_DataDocument.SelectSingleNode(xpath);
                            if (XPathResult != null)
                            {
                                m_SEKeywords = XPathResult.InnerText;
                            }
                            break;

                        case "transform":
                            xsw = new StringWriter();
                            t = new XslCompiledTransform();
                            x.LoadXml(SEKeywords.InnerXml);
                            t.Load(x);
                            t.Transform(m_DataDocument, m_TransformArgumentList, xsw);
                            m_SEKeywords = xsw.ToString();
                            break;

                        case "text":
                            m_SEKeywords = SEKeywords.InnerText.Replace("\r\n", "").Trim();
                            break;
                    }

                }

                if (SEDescription != null)
                {
                    if (SEDescription.Attributes["actionType"] == null)
                    {
                        throw new Exception("actionType attribute not specified for SEDescription element");
                    }
                    switch (SEDescription.Attributes["actionType"].InnerText)
                    {
                        case "xpath":
                            string xpath = SEDescription.InnerText;
                            XPathResult = m_DataDocument.SelectSingleNode(xpath);
                            if (XPathResult != null)
                            {
                                m_SEDescription = XPathResult.InnerText;
                            }
                            break;

                        case "transform":
                            xsw = new StringWriter();
                            t = new XslCompiledTransform();
                            x.LoadXml(SEDescription.InnerXml);
                            t.Load(x);
                            t.Transform(m_DataDocument, m_TransformArgumentList, xsw);
                            m_SEDescription = xsw.ToString();
                            break;
                        case "text":
                            m_SEDescription = SEDescription.InnerText.Replace("\r\n", "").Trim();
                            break;
                    }
                }

                if (SENoScript != null)
                {
                    if (SENoScript.Attributes["actionType"] == null)
                    {
                        throw new Exception("actionType attribute not specified for SENoScript element");
                    }
                    switch (SENoScript.Attributes["actionType"].InnerText)
                    {
                        case "xpath":
                            string xpath = SENoScript.InnerText;
                            XPathResult = m_DataDocument.SelectSingleNode(xpath);
                            if (XPathResult != null)
                            {
                                m_SENoScript = XPathResult.InnerText;
                            }
                            break;

                        case "transform":
                            xsw = new StringWriter();
                            t = new XslCompiledTransform();
                            x.LoadXml(SENoScript.InnerXml);
                            t.Load(x);
                            t.Transform(m_DataDocument, m_TransformArgumentList, xsw);
                            m_SENoScript = xsw.ToString();
                            break;

                        case "text":
                            m_SENoScript = SENoScript.InnerText.Replace("\r\n", "").Trim();
                            break;
                    }

                }

            }
        }
        private void ProcessAfterActions()
        {
            XmlNodeList q;
            XmlNode PostProcessing = m_PackageDocument.SelectSingleNode("/package/PostProcessing");
            if (PostProcessing != null)
            {
                q = PostProcessing.SelectNodes("/package/PostProcessing/queryafter");
                foreach (XmlNode n in q)
                {
                    ProcessSQLAfterActions(n);
                }

                q = PostProcessing.SelectNodes("/package/PostProcessing/webqueryafter");
                foreach (XmlNode n in q)
                {
                    ProcessWebQueryAfterActions(n);
                }

                q = PostProcessing.SelectNodes("/package/PostProcessing/setcookie");
                foreach (XmlNode n in q)
                {
                    ProcessCookieAfterActions(n);
                }
            }
        }
        private void ProcessSQLAfterActions(XmlNode n)
        {
            SqlConnection cn = new SqlConnection(DB.GetDBConn());
            cn.Open();

            String sql = String.Empty;
            StringBuilder xml = new StringBuilder("");
            StringBuilder tmpXml = new StringBuilder("");
            string queryname = String.Empty;
            string replaceTag = String.Empty;

            string replaceType = String.Empty;
            string replaceParamName = String.Empty;
            string defaultValue = String.Empty;
            string validationpattern = String.Empty;
            string replaceValue = String.Empty;
            string RowElementName = String.Empty;
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = cn;
            cmd.CommandType = CommandType.Text;


            //determine if the query is going to be run
            XmlNode runif = n.SelectSingleNode("runif");
            if (runif != null)
            {
                string runifparam = runif.Attributes["paramsource"].InnerText;
                switch (runif.Attributes["paramtype"].InnerText)
                {
                    case "request":
                    case "appconfig":
                        if (CommonLogic.IsStringNullOrEmpty(CommonLogic.ParamsCanBeDangerousContent(runifparam)) && 
                            CommonLogic.IsStringNullOrEmpty(AppLogic.AppConfig(runifparam)))
                        {
                            return;
                        }
                        break;
                    case "xpath":
                        if (m_DataDocument.SelectSingleNode(runifparam) == null)
                        {
                            return;
                        }
                        break;
                    default:
                        return;
                }
            }

            cmd.Parameters.Clear();
            cmd.CommandText = "";

            sql = n.FirstChild.InnerText.ToString();
            XmlNodeList replacenodes = n.SelectNodes("querystringreplace");
            foreach (XmlNode rn in replacenodes)
            {
                replaceTag = rn.Attributes["replaceTag"].InnerText;
                replaceType = rn.Attributes["replacetype"].InnerText.ToLowerInvariant();
                replaceParamName = rn.Attributes["replaceparamname"].InnerText;
                defaultValue = rn.Attributes["defvalue"].InnerText;
                validationpattern = rn.Attributes["validationpattern"].InnerText.Trim();

                replaceValue = "";
                switch (replaceType)
                {
                    case "request":
                        replaceValue = CommonLogic.ParamsCanBeDangerousContent(replaceParamName);
                        break;
                    case "appconfig":
                        replaceValue = AppLogic.AppConfig(replaceParamName);
                        break;
                    case "webconfig":
                        replaceValue = CommonLogic.Application(replaceParamName);
                        break;
                    case "runtime":
                        replaceValue = GetRuntimeParamValue(replaceParamName);
                        break;
                    case "xpath":
                        XmlNode x = m_DataDocument.SelectSingleNode(replaceParamName);
                        if (x != null)
                        {
                            replaceValue = x.InnerText;
                        }
                        break;
                }
                if (replaceValue.Trim().Length == 0)
                {
                    replaceValue = defaultValue;
                }
                if (validationpattern.Length > 0)
                {
                    if (Regex.IsMatch(replaceValue, validationpattern, RegexOptions.IgnoreCase | RegexOptions.Compiled))
                    {
                        sql = sql.Replace(replaceTag, replaceValue);
                    }
                    else
                    {
                        throw new Exception("String Replace parameter " + replaceTag + " failed validation");
                    }
                }
                else
                {
                    sql = sql.Replace(replaceTag, replaceValue);
                }
            }

            cmd.CommandText = sql;

            XmlNodeList qp = n.SelectNodes("queryparam");
            foreach (XmlNode pn in qp)
            {
                //create a parameter of the appropriate type
                cmd.Parameters.Add(CreateParameter(pn));
            }
            cmd.ExecuteNonQuery();

            cn.Close();
            cmd.Dispose();
            cn.Dispose();

        }
        private void ProcessWebQueryAfterActions(XmlNode n)
        {
            StringBuilder xml = new StringBuilder("");
            StringBuilder tmpXml = new StringBuilder("");
            string url = String.Empty;
            string queryname = String.Empty;
            string rettype = String.Empty;
            string replaceTag = String.Empty;
            string replaceType = String.Empty;
            string replaceParamName = String.Empty;
            string defaultValue = String.Empty;
            string validationpattern = String.Empty;
            string replaceValue = String.Empty;

            //determine if the query is going to be run
            XmlNode runif = n.SelectSingleNode("runif");
            if (runif != null)
            {
                string runifparam = runif.Attributes["paramsource"].InnerText;
                switch (runif.Attributes["paramtype"].InnerText)
                {
                    case "request":
                    case "appconfig":
                        if (CommonLogic.IsStringNullOrEmpty(CommonLogic.ParamsCanBeDangerousContent(runifparam)) && 
                            CommonLogic.IsStringNullOrEmpty(AppLogic.AppConfig(runifparam)))
                        {
                            return;
                        }
                        break;
                    case "xpath":
                        if (m_DataDocument.SelectSingleNode(runifparam) == null)
                        {
                            return;
                        }
                        break;
                    default:
                        return;
                }
            }
            queryname = n.Attributes["name"].InnerText;
            rettype = n.Attributes["RetType"].InnerText;
            url = n.SelectSingleNode("url").InnerText;
            if (url.Length == 0)
            {
                return;
            }

            XmlNodeList replacenodes = n.SelectNodes("querystringreplace");
            foreach (XmlNode rn in replacenodes)
            {
                replaceTag = rn.Attributes["replaceTag"].InnerText;
                replaceType = rn.Attributes["replacetype"].InnerText.ToLowerInvariant();
                replaceParamName = rn.Attributes["replaceparamname"].InnerText;
                defaultValue = rn.Attributes["defvalue"].InnerText;
                validationpattern = rn.Attributes["validationpattern"].InnerText.Trim();

                switch (replaceType)
                {
                    case "request":
                        replaceValue = CommonLogic.ParamsCanBeDangerousContent(replaceParamName);
                        break;

                    case "appconfig":
                        replaceValue = AppLogic.AppConfig(replaceParamName);
                        break;
                    case "webconfig":
                        replaceValue = CommonLogic.Application(replaceParamName);
                        break;
                    case "runtime":
                        replaceValue = GetRuntimeParamValue(replaceParamName);
                        break;
                    case "xpath":
                        XmlNode x = m_DataDocument.SelectSingleNode(replaceParamName);
                        if (x != null)
                        {
                            replaceValue = x.InnerText;
                        }
                        break;
                }
                if (replaceValue.Length == 0)
                {
                    replaceValue = defaultValue;
                }

                if (validationpattern.Length > 0)
                {
                    if (Regex.IsMatch(replaceValue, validationpattern, RegexOptions.IgnoreCase | RegexOptions.Compiled))
                    {
                        url = url.Replace(replaceTag, replaceValue);
                    }
                    else
                    {
                        throw new Exception("String Replace parameter " + replaceTag + " failed validation");
                    }
                }
                else
                {
                    url = url.Replace(replaceTag, replaceValue);
                }
            }
            tmpXml.Append(CommonLogic.AspHTTP(url,30));

        }
        private void ProcessCookieAfterActions(XmlNode n)
        {
            string valuetype = n.Attributes["valuetype"].InnerText.ToLowerInvariant();
            string cookiesource = n.Attributes["cookiesource"].InnerText;
            string cookiename = n.Attributes["cookiename"].InnerText;
            string cookievalue = String.Empty;
            double cookieexpires = 0;
            if (n.Attributes["expires"] != null)
            {
                cookieexpires = Convert.ToDouble(n.Attributes["expires"].InnerText.Trim());
            }

            switch (valuetype)
            {
                case "request":
                    cookievalue = CommonLogic.ParamsCanBeDangerousContent(cookiesource);
                    break;
                case "appconfig":
                    cookievalue = AppLogic.AppConfig(cookiesource);
                    break;
                case "webconfig":
                    cookievalue = CommonLogic.Application(cookiesource);
                    break;
                case "xpath":
                    XmlNode x = m_DataDocument.SelectSingleNode(cookiesource);
                    if (x != null)
                    {
                        cookievalue = x.InnerText;
                    }
                    break;
            }
            HttpContext.Current.Response.Cookies[cookiename].Value = cookievalue;
            if (cookieexpires > 0)
            {
                HttpContext.Current.Response.Cookies[cookiename].Expires = DateTime.Now.AddDays(cookieexpires);
            }
        }

        #endregion
    }
}
