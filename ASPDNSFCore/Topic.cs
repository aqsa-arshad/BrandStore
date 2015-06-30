// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Web.Security;
using System.Configuration;
using System.Web.SessionState;
using System.Web.Caching;
using System.Net.Mail;
using System.Web.Util;
using System.Data;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.IO;
using System.Net;
using System.Xml;
using System.Drawing;
using System.Xml.Serialization;
using System.Globalization;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace AspDotNetStorefrontCore
{
	/// <summary>
	/// Summary description for Topic.
	/// this class is now changed in ML v5.7+ to NO LONGER do any automatic token replacement in the Topic object. The
	/// caller is now responsible for doing that after the topic object has been created, if they need to.
	/// </summary>
	public class Topic
	{

		private String m_TopicName = String.Empty;
		private int m_TopicID = 0;
		private String m_LocaleSetting = String.Empty;
		private int m_SkinID = 1;
        private int m_StoreID = 0;
        private int m_DisplayOrder = 1;
		private String m_Contents = String.Empty;
		private String m_ContentsRAW = String.Empty;
		private String m_SectionTitle = String.Empty;
		private String m_SETitle = String.Empty;
		private String m_SEKeywords = String.Empty;
		private String m_SEDescription = String.Empty;
        private string m_SENoScript = string.Empty;
        private String m_Password = String.Empty;
		private bool m_FromDB = false;
		private bool m_RequiresSubscription = false;
		private bool m_RequiresDisclaimer = false;
        private bool m_HtmlOk = true;
        private bool m_ShowInSiteMap = true;
		private String m_FN = String.Empty;
		private String m_MasterLocale = String.Empty;
		//private Regex m_CmdMatch = new Regex("\\(\\!(\\w+)(?:\\s(?:(\\w*)=(?:'|\")(.*?)(?:\"|'))?)*\\!\\)"); // Parses tokens of the form "(!command attribute="value" attribute2='value' ..!)"
		private MatchEvaluator m_CmdMatchEval;
		private Hashtable m_CommandHashtable;
		private String m_GraphicsColor = String.Empty;
		private String m_ContentsBGColor = String.Empty;
		private String m_PageBGColor = String.Empty;
		private Parser m_UseParser = null;
        private bool m_HasChildren = false;
        private bool m_ChildMappedToSelf = false;
        private bool m_IsMapped = false;
        private List<int> m_Children = new List<int>();

        public Topic() {}

		public Topic(int TopicID)
			: this(TopicID,Localization.GetDefaultLocale(),1,null)
		{}

		public Topic(int TopicID, int SkinID)
			: this(TopicID,Localization.GetDefaultLocale(),SkinID,null)
		{}
		
		public Topic(int TopicID, String LocaleSetting)
			: this(TopicID,LocaleSetting,1,null)
		{}

		public Topic(int TopicID, String LocaleSetting, int SkinID)
			: this(TopicID,LocaleSetting,SkinID,null)
		{}

        public Topic(int TopicID, String LocaleSetting, int SkinID, Parser UseParser)
            : this(TopicID, LocaleSetting, SkinID, UseParser, AppLogic.StoreID())
        { }

		public Topic(int TopicID, String LocaleSetting, int SkinID, Parser UseParser, int StoreID)
		{
			m_LocaleSetting = LocaleSetting;
			m_TopicID = TopicID;
			m_SkinID = SkinID;
			m_TopicName = String.Empty; //GetName(TopicID,Localization.GetWebConfigLocale());
            m_CommandHashtable = new Hashtable();
            m_CmdMatchEval = new MatchEvaluator(CommandMatchEvaluator);
			m_UseParser = UseParser;
            m_StoreID = StoreID;
			LoadFromDB(StoreID);
		}

		public Topic(String TopicName)
			: this(TopicName,Localization.GetDefaultLocale(),1,null, AppLogic.StoreID())
		{}
		
		public Topic(String TopicName, String LocaleSetting)
            : this(TopicName, LocaleSetting, 1, null, AppLogic.StoreID())
		{}

		public Topic(String TopicName, int SkinID)
            : this(TopicName, Localization.GetDefaultLocale(), SkinID, null, AppLogic.StoreID())
		{}
		
		public Topic(String TopicName, String LocaleSetting, int SkinID)
            : this(TopicName, LocaleSetting, SkinID, null)
		{}

        public Topic(String TopicName, String LocaleSetting, int SkinID, Parser UseParser)
            : this(TopicName, LocaleSetting, SkinID, UseParser, AppLogic.StoreID())
        {}

		public Topic(String TopicName, String LocaleSetting, int SkinID, Parser UseParser, int StoreID)
		{
			m_TopicName = TopicName.Trim();
			m_LocaleSetting = LocaleSetting;
			m_SkinID = SkinID;
			m_TopicID = Topic.GetTopicID(TopicName,Localization.GetDefaultLocale(), StoreID); // always find topics by MASTER locale name!
			m_CommandHashtable = new Hashtable();
			m_CmdMatchEval = new MatchEvaluator(CommandMatchEvaluator);
			m_UseParser = UseParser;
            m_StoreID = StoreID;
			LoadFromDB(StoreID);
		}

        public Topic(Guid TopicGuid)
        {
            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS("select topicid from topic where topicguid = " + DB.SQuote(TopicGuid.ToString()), conn))
                {
                    if (rs.Read())
                    {
                        m_TopicID = DB.RSFieldInt(rs, "topicid");
                        m_CommandHashtable = new Hashtable();
                        m_CmdMatchEval = new MatchEvaluator(CommandMatchEvaluator);
                        LoadFromDB(AppLogic.StoreID());
                    }
                }
            }
        }

		/// <summary>
		/// Evaluates (!command value="xxx"!) tokens
		/// </summary>
		protected String CommandMatchEvaluator(Match match)
		{
			string cmd = match.Groups[1].Value.ToLowerInvariant(); // The command string

            Hashtable parameters = new Hashtable(); //new CaseInsensitiveHashCodeProvider(),new CaseInsensitiveComparer());

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
				try
				{
					m_CommandHashtable.Add(cmd.ToLowerInvariant(),val);
				}
				catch {}
			}
			return String.Empty;
		}

        private Customer thisCustomer;
        public Customer ThisCustomer
        {
            get
            {
                if (thisCustomer == null)
                    thisCustomer = AppLogic.GetCurrentCustomer();

                return thisCustomer;
            }
        }
		
		public String Contents
		{
			get 
			{
				return m_Contents;
			}
            set { m_Contents = value; }
		}

        public String ContentsRAW
        {
            get
            {
                return m_ContentsRAW;
            }
            set { m_ContentsRAW = value; }
        }

		public String GraphicsColor
		{
			get 
			{
				return m_GraphicsColor;
			}
            set { m_GraphicsColor = value; }
		}

		public String ContentsBGColor
		{
			get 
			{
				return m_ContentsBGColor;
			}
		}

		public String PageBGColor
		{
			get 
			{
				return m_PageBGColor;
			}
		}

		public bool FromDB
		{
			get 
			{
				return m_FromDB;
			}
		}

        public Hashtable CommandHashtable
        {
            get
            {
                return m_CommandHashtable;
            }
        }


		public bool RequiresSubscription
		{
			get 
			{
				return m_RequiresSubscription;
			}
            set { m_RequiresSubscription = value; }
		}

		public bool RequiresDisclaimer
		{
			get 
			{
				return m_RequiresDisclaimer;
			}
            set { m_RequiresDisclaimer = value; }
		}

		public String TopicName
		{
			get 
			{
				return m_TopicName;
			}
            set { m_TopicName = value; }
		}

		public String SectionTitle
		{
			get 
			{
				return m_SectionTitle;
			}
            set { m_SectionTitle = value; }
		}

		public String Password
		{
			get 
			{
				return m_Password;
			}
            set { m_Password = value; }
		}

		public String FN
		{
			get 
			{
				return m_FN;
			}
            set { m_FN = value; }
		}

		public int TopicID
		{
			get 
			{
				return m_TopicID;
			}
		}

        public int DisplayOrder
        {
            get { return m_DisplayOrder; }
            set { m_DisplayOrder = value; }
        }

        public int StoreID
        {
            get { return m_StoreID; }
            set { m_StoreID = value; }
        }

		public int SkinID
		{
			get 
			{
				return m_SkinID;
			}
            set { m_SkinID = value; }
		}

        public String LocaleSetting
        {
            get { return m_LocaleSetting; }
            set { m_LocaleSetting = value; }
        }

		public String SETitle
		{
			get 
			{
				return m_SETitle;
			}
            set { m_SETitle = value; }
		}

		public String SEKeywords
		{
			get 
			{
				return m_SEKeywords;
			}
            set { m_SEKeywords = value; }
		}

		public String SEDescription
		{
			get 
			{
				return m_SEDescription;
			}
            set { m_SEDescription = value; }
		}

		public string SENoScript
        {
            get
            {
                return m_SENoScript;
            }
            set { m_SENoScript = value; }
        }

        public bool HTMLOk
        {
            get { return m_HtmlOk; }
            set { m_HtmlOk = value; }
        }

        public bool ShowInSiteMap
        {
            get { return m_ShowInSiteMap; }
            set { m_ShowInSiteMap = value; }
        }

        public bool HasChildren
        {
            get { return m_HasChildren; }
        }

        public bool ChildMappedToSelf
        {
            get { return m_ChildMappedToSelf; }
        }

        public List<int> Children
        {
            get { return m_Children; }
            set { m_Children = value; }
        }

        public bool IsMapped
        {
            get { return m_IsMapped; }
            set { m_IsMapped = value; }
        }
        
        // Find the specified topic content. note, we try to find content even if it doesn't exactly match the input specs, by doing an ordered lookup in various areas
		// we want to show SOME topic content if it is at all possible, even if the language is not right, etc...
		// Note: site id only used for file based topic _contents
		// Search Order is (yes, other orderings are possible, but this is the one we chose, where ANY db topic match overrides file content):
		// the other option would be to match on locales in the order of DB/File (Customer Locale), DB/File (Store Locale), DB/File (Null locale)
		// DB (customer locale)
		// DB (store locale)
		// DB (null locale)
		// File (customer locale)
		// File (store locale)
		// File (null locale)
		void LoadFromDB(int StoreID)
		{
			m_FromDB = false;
            m_DisplayOrder = 1;
            m_SkinID = ThisCustomer.SkinID;
            m_StoreID = StoreID;
            m_LocaleSetting = CommonLogic.IIF(m_LocaleSetting.Length > 0, m_LocaleSetting, Localization.GetDefaultLocale());
			m_Contents = String.Empty;
			m_ContentsRAW = String.Empty;
			m_SectionTitle = String.Empty;
			m_RequiresSubscription = false;
			m_RequiresDisclaimer = false;
            m_ShowInSiteMap = true;
			m_Password = String.Empty;
			m_SETitle = m_TopicName;
			m_SEKeywords = String.Empty;
            m_SEDescription = String.Empty;
            m_SENoScript = String.Empty;
            m_FN = String.Empty;
			m_MasterLocale = m_LocaleSetting;
            m_Children = new List<int>();
            m_HasChildren = false;

			if(m_TopicID == 0)
			{
				m_TopicID = Topic.GetTopicID(m_TopicName,CommonLogic.IIF(AppLogic.IsAdminSite, m_MasterLocale, m_LocaleSetting), AppLogic.StoreID());
			}

			if(m_TopicID != 0)
			{
                String sql = String.Format("SELECT * from Topic with (NOLOCK) where Deleted=0 and Published=1 and TopicID={1} and (SkinID IS NULL or SkinID=0 or SkinID={2}) order by DisplayOrder, Name ASC", CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowTopicFiltering") == true, 1, 0), m_TopicID.ToString(), m_SkinID.ToString());

                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS(sql, con))
                    {
                        if (rs.Read())
                        {
                            m_FromDB = true;
                            m_TopicID = DB.RSFieldInt(rs, "TopicID");
                            m_TopicName = DB.RSField(rs, "Name");
                            m_Contents = DB.RSFieldByLocale(rs, "Description", m_LocaleSetting);
                            m_Password = DB.RSField(rs, "Password");
                            m_RequiresSubscription = DB.RSFieldBool(rs, "RequiresSubscription");
                            m_RequiresDisclaimer = DB.RSFieldBool(rs, "RequiresDisclaimer");
                            m_GraphicsColor = DB.RSField(rs, "GraphicsColor");
                            m_ContentsBGColor = DB.RSField(rs, "ContentsBGColor");
                            m_PageBGColor = DB.RSField(rs, "PageBGColor");
                            m_DisplayOrder = DB.RSFieldInt(rs, "DisplayOrder");
                            m_ShowInSiteMap = DB.RSFieldBool(rs, "ShowInSiteMap");
                            m_SkinID = DB.RSFieldInt(rs, "SkinID");
                            if (m_Contents.Length != 0)
                            {
                                m_ContentsRAW = m_Contents;
                                m_SectionTitle = DB.RSFieldByLocale(rs, "Title", m_LocaleSetting);
                                m_SETitle = DB.RSFieldByLocale(rs, "SETitle", m_LocaleSetting);
                                m_SEKeywords = DB.RSFieldByLocale(rs, "SEKeywords", m_LocaleSetting);
                                m_SEDescription = DB.RSFieldByLocale(rs, "SEDescription", m_LocaleSetting);
                                m_SENoScript = DB.RSFieldByLocale(rs, "SENoScript", m_LocaleSetting);
                            }
                            else // nothing found, try master locale:
                            {
                                m_Contents = DB.RSFieldByLocale(rs, "Description", m_MasterLocale);
                                m_ContentsRAW = m_Contents;
                                m_SectionTitle = DB.RSFieldByLocale(rs, "Title", m_MasterLocale);
                                m_SETitle = DB.RSFieldByLocale(rs, "SETitle", m_MasterLocale);
                                m_SEKeywords = DB.RSFieldByLocale(rs, "SEKeywords", m_MasterLocale);
                                m_SEDescription = DB.RSFieldByLocale(rs, "SEDescription", m_MasterLocale);
                                m_SENoScript = DB.RSFieldByLocale(rs, "SENoScript", m_MasterLocale);
                            }

                            // if an html tag is present, extract just the body of the content
                            if (m_Contents.IndexOf("<html", StringComparison.InvariantCultureIgnoreCase) != -1)
                            {
                                m_Contents = CommonLogic.ExtractBody(m_ContentsRAW);
                            }
                        }
                    }
                }

                // loads the child topics
                m_HasChildren = DB.GetSqlN("select count(tm.TopicID) as N from dbo.TopicMapping tm with(NOLOCK) left join dbo.Topic t with(NOLOCK) on t.TopicID = tm.ParentTopicID where t.TopicID =" + m_TopicID.ToString() + " and t.Deleted=0") > 0;
                
                if (m_HasChildren)
                {
                    LoadChildren();
                }
			}

			if(!m_FromDB) // did not find anything in db, try file based topic content (in skins folder as topicname.htm)
			{
                string appdir = HttpContext.Current.Request.PhysicalApplicationPath;

                List<string> possibleFileNames = new List<string> {
                    Path.Combine(appdir, string.Format("App_Templates\\Skin_{0}\\Topics\\{1}.{2}.htm", SkinID.ToString(), m_TopicName, m_LocaleSetting)),   //Skin specific, localized
                    Path.Combine(appdir, String.Format("App_Templates\\Skin_{0}\\Topics\\{1}.htm", SkinID.ToString(), m_TopicName)),                        //Skin specific, unlocalized
                    Path.Combine(appdir, string.Format("Topics\\{0}.{1}.htm", m_TopicName, m_LocaleSetting)),                                               //Root folder, localized
                    Path.Combine(appdir, string.Format("Topics\\{0}.htm", m_TopicName)),                                                                    //Root folder, unlocalized
                    Path.Combine(appdir, string.Format("App_Templates\\Skin_{0}\\Topics\\{1}.{2}.html", SkinID.ToString(), m_TopicName, m_LocaleSetting)),  //Skin specific, localized HTML
                    Path.Combine(appdir, String.Format("App_Templates\\Skin_{0}\\Topics\\{1}.html", SkinID.ToString(), m_TopicName)),                       //Skin specific, unlocalized HTML
                    Path.Combine(appdir, string.Format("Topics\\{0}.{1}.html", m_TopicName, m_LocaleSetting)),                                              //Root folder, localized HTML
                    Path.Combine(appdir, string.Format("Topics\\{0}.html", m_TopicName))                                                                    //Root folder, unlocalized HTML
                };

                foreach (string fileNametoCheck in possibleFileNames)
                {
                    m_FN = CommonLogic.SafeMapPath(fileNametoCheck);

                    if (CommonLogic.FileExists(m_FN))
                        break;
                }

                if (m_FN.Length != 0 && CommonLogic.FileExists(m_FN))
                {
                    m_Contents = CommonLogic.ReadFile(m_FN, true);
                    m_ContentsRAW = m_Contents;
                    m_SectionTitle = CommonLogic.ExtractToken(m_ContentsRAW, "<title>", "</title>");
                    m_Contents = CommonLogic.ExtractBody(m_Contents);

                    // try old token formats first, for backwards compatibility:
                    m_SETitle = CommonLogic.ExtractToken(m_ContentsRAW, "<PAGETITLE>", "</PAGETITLE>");
                    m_SEKeywords = CommonLogic.ExtractToken(m_ContentsRAW, "<PAGEKEYWORDS>", "</PAGEKEYWORDS>");
                    m_SEDescription = CommonLogic.ExtractToken(m_ContentsRAW, "<PAGEDESCRIPTION>", "</PAGEDESCRIPTION>");
                    m_SENoScript = CommonLogic.ExtractToken(m_ContentsRAW, "<NOSCRIPT>", "</NOSCRIPT>");

                    // if regular HTML tokens found, try to parse it out in regular HTML syntax meta tag format and they take precedence over the old tokens (above):
                    String t = Regex.Match(m_ContentsRAW, @"(?<=<title[^\>]*>).*?(?=</title>)", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture).Value;
                    if (t.Length != 0)
                    {
                        m_SETitle = t;
                    }

                    String MK = String.Empty;
                    String MV = String.Empty;
                    foreach (Match metamatch in Regex.Matches(m_ContentsRAW, @"<meta\s*(?:(?:\b(\w|-)+\b\s*(?:=\s*(?:""[^""]*""|'[^']*'|[^""'<> ]+)\s*)?)*)/?\s*>", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture))
                    {
                        MK = String.Empty;
                        MV = String.Empty;
                        // Loop through the attribute/value pairs inside the tag
                        foreach (Match submatch in Regex.Matches(metamatch.Value.ToString(), @"(?<name>\b(\w|-)+\b)\s*=\s*(""(?<value>[^""]*)""|'(?<value>[^']*)'|(?<value>[^""'<> ]+)\s*)+", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture))
                        {
                            if ("http-equiv".Equals(submatch.Groups[1].ToString(), StringComparison.InvariantCultureIgnoreCase))
                            {
                                MV = submatch.Groups[2].ToString();
                            }
                            if (("name".Equals(submatch.Groups[1].ToString(), StringComparison.InvariantCultureIgnoreCase)) && 
                                MK == String.Empty) // if it's already set, HTTP-EQUIV takes precedence
                            {
                                MV = submatch.Groups[2].ToString();
                            }
                            if ("content".Equals(submatch.Groups[1].ToString(), StringComparison.InvariantCultureIgnoreCase))
                            {
                                MV = submatch.Groups[2].ToString();
                            }
                        }
                        switch (MK.ToLowerInvariant())
                        {
                            case "description":
                                m_SEDescription = MV;
                                break;
                            case "keywords":
                            case "keyword":
                                m_SEKeywords = MV;
                                break;
                        }
                    }
                }

				if(m_CommandHashtable.Contains("contentsbgcolor"))
				{
					m_ContentsBGColor = m_CommandHashtable["contentsbgcolor"].ToString();
				}
				if(m_CommandHashtable.Contains("pagebgcolor"))
				{
					m_PageBGColor = m_CommandHashtable["pagebgcolor"].ToString();
				}
				if(m_CommandHashtable.Contains("graphicscolor"))
				{
					m_GraphicsColor = m_CommandHashtable["graphicscolor"].ToString();
				}
			}
			
			if(m_SETitle.Length == 0)
			{
				m_SETitle = m_SectionTitle;
			}
	
			if(AppLogic.ReplaceImageURLFromAssetMgr)
			{
				while(m_Contents.IndexOf("../images") != -1)
				{
					m_Contents = m_Contents.Replace("../images","images");
				}
			}
			if(m_UseParser != null)
			{
				m_Contents = m_UseParser.ReplaceTokens(m_Contents);
			}
			else
			{
                if (SkinID > 0)
                {
                    m_Contents = m_Contents.Replace("(!SKINID!)", SkinID.ToString());
                }
			}
        }

        public bool Commit()
        {
            bool commited = false;

            Customer ThisCustomer = AppLogic.GetCurrentCustomer();

            if (ThisCustomer.IsAdminUser)
            {
                bool topicexists = GetTopicID(TopicName, AppLogic.StoreID()) > 0;

                // ok to add them:
                if (!topicexists)
                {
                    StringBuilder sql = new StringBuilder(2500);

                    String NewGUID = DB.GetNewGUID();
                    sql.Append("insert into Topic(TopicGUID,Name,SkinID,DisplayOrder,ContentsBGColor,PageBGColor,GraphicsColor,Title,Description,Password,RequiresSubscription,HTMLOk,RequiresDisclaimer,ShowInSiteMap,SEKeywords,SEDescription,SETitle) values(");
                    sql.Append(DB.SQuote(NewGUID) + ",");
                    sql.Append(DB.SQuote(AppLogic.FormLocaleXml(TopicName, LocaleSetting)) + ",");
                    sql.Append(Localization.ParseUSInt(SkinID.ToString()) + ",");
                    sql.Append(Localization.ParseUSInt(DisplayOrder.ToString()) + ",");
                    sql.Append(DB.SQuote(ContentsBGColor) + ",");
                    sql.Append(DB.SQuote(PageBGColor) + ",");
                    sql.Append(DB.SQuote(GraphicsColor) + ",");
                    sql.Append(DB.SQuote(AppLogic.FormLocaleXml(SectionTitle, LocaleSetting)) + ",");
                    
                    if (Contents.Length != 0)
                    {
                        sql.Append(DB.SQuote(Contents) + ",");
                    }
                    else
                    {
                        sql.Append("NULL,");
                    }
                    if (Password.Trim().Length != 0)
                    {
                        sql.Append(DB.SQuote(Password) + ",");
                    }
                    else
                    {
                        sql.Append("NULL,");
                    }
                    sql.Append(CommonLogic.IIF(RequiresSubscription,"1","0") + ",");
                    sql.Append(CommonLogic.IIF(HTMLOk, "1", "0") + ",");
                    sql.Append(CommonLogic.IIF(RequiresDisclaimer, "1", "0") + ",");
                    sql.Append(CommonLogic.IIF(ShowInSiteMap, "1", "0") + ",");
                    if (AppLogic.FormLocaleXml("ltSEKeywords").Length != 0)
                    {
                        sql.Append(DB.SQuote(AppLogic.FormLocaleXml(SEKeywords, LocaleSetting)) + ",");
                    }
                    else
                    {
                        sql.Append("NULL,");
                    }
                    if (AppLogic.FormLocaleXml("ltSEDescription").Length != 0)
                    {
                        sql.Append(DB.SQuote(AppLogic.FormLocaleXml(SEDescription, LocaleSetting)) + ",");
                    }
                    else
                    {
                        sql.Append("NULL,");
                    }
                    if (AppLogic.FormLocaleXml("ltSETitle").Length != 0)
                    {
                        sql.Append(DB.SQuote(AppLogic.FormLocaleXml(SETitle, LocaleSetting)));
                    }
                    else
                    {
                        sql.Append("NULL");
                    }
                    sql.Append(")");
                    try
                    {
                        DB.ExecuteSQL(sql.ToString());
                        commited = true;
                        LoadFromDB(AppLogic.StoreID());
                    }
                    catch 
                    {
                        commited = false;
                    }
                }
            }

            return commited;
        }

        public bool HasBeenMapped(int TopicToCheck)
        {
            int i = DB.GetSqlN("select count(*) as N from dbo.TopicMapping where TopicID=" + TopicToCheck.ToString() + " and ParentTopicID=" + TopicID.ToString());
            
            return i > 0;
        }

        public bool Update()
        {
            bool updated = false;

            Customer ThisCustomer = Customer.Current;

            if (ThisCustomer.IsAdminUser)
            {
                int StoreID = AppLogic.StoreID();
                int thisTopicID = GetTopicID(TopicName, StoreID);
                bool topicexists = thisTopicID > 0;

                // ok to update:
                if (topicexists)
                {
                    StringBuilder sql = new StringBuilder(2500);

                    sql.Append("update Topic set ");
                    sql.Append("Name=" + DB.SQuote(AppLogic.FormLocaleXml("Name", TopicName, LocaleSetting, "topic", Convert.ToInt32(thisTopicID))) + ",");
                    sql.Append("SkinID=" + Localization.ParseUSInt(SkinID.ToString()) + ",");
                    sql.Append("DisplayOrder=" + Localization.ParseUSInt(DisplayOrder.ToString()) + ",");
                    sql.Append("ContentsBGColor=" + DB.SQuote(ContentsBGColor) + ",");
                    sql.Append("PageBGColor=" + DB.SQuote(PageBGColor) + ",");
                    sql.Append("GraphicsColor=" + DB.SQuote(GraphicsColor) + ",");
                    sql.Append("Title=" + DB.SQuote(AppLogic.FormLocaleXml("Title", SectionTitle, LocaleSetting, "topic", Convert.ToInt32(thisTopicID))) + ",");
                    
                    if (Contents.Length != 0)
                    {
                        sql.Append("Description=" + DB.SQuote(Contents) + ",");
                    }
                    else
                    {
                        sql.Append("Description=NULL,");
                    }
                    if (Password.Trim().Length != 0)
                    {
                        sql.Append("Password=" + DB.SQuote(Password) + ",");
                    }
                    else
                    {
                        sql.Append("Password=NULL,");
                    }
                    sql.Append("RequiresSubscription=" + CommonLogic.IIF(RequiresSubscription, "1", "0") + ",");
                    sql.Append("HTMLOk=" + CommonLogic.IIF(HTMLOk,"1","0") + ",");
                    sql.Append("RequiresDisclaimer=" + CommonLogic.IIF(RequiresDisclaimer,"1","0") + ",");
                    sql.Append("ShowInSiteMap=" + CommonLogic.IIF(ShowInSiteMap,"1","0") + ",");
                    if (AppLogic.FormLocaleXml("ltSEKeywords").Length != 0)
                    {
                        sql.Append("SEKeywords=" + DB.SQuote(AppLogic.FormLocaleXml("SEKeywords", SEKeywords, LocaleSetting, "topic", Convert.ToInt32(thisTopicID))) + ",");
                    }
                    else
                    {
                        sql.Append("SEKeywords=NULL,");
                    }
                    if (AppLogic.FormLocaleXml("ltSEDescription").Length != 0)
                    {
                        sql.Append("SEDescription=" + DB.SQuote(AppLogic.FormLocaleXml("SEDescription", SEDescription, LocaleSetting, "topic", Convert.ToInt32(thisTopicID))) + ",");
                    }
                    else
                    {
                        sql.Append("SEDescription=NULL,");
                    }
                    if (AppLogic.FormLocaleXml("ltSETitle").Length != 0)
                    {
                        sql.Append("SETitle=" + DB.SQuote(AppLogic.FormLocaleXml("SETitle", SETitle, LocaleSetting, "topic", Convert.ToInt32(thisTopicID))));
                    }
                    else
                    {
                        sql.Append("SETitle=NULL");
                    }
                    sql.Append(" where TopicID=" + thisTopicID.ToString());

                    try
                    {
                        DB.ExecuteSQL(sql.ToString());
                        updated = true;
                        LoadFromDB(AppLogic.StoreID());
                    }
                    catch
                    {
                        updated = false;
                    }
                }
            }

            return updated;
        }

        private void LoadChildren()
        {
            m_Children = new List<int>();

            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                using (IDataReader rs = DB.GetRS("select TopicID from dbo.TopicMapping with(NOLOCK) where ParentTopicID=" + m_TopicID.ToString(), conn))
                {
                    while (rs.Read())
                    {
                        int tID = DB.RSFieldInt(rs, "TopicID");
                        if (tID != m_TopicID)
                        {
                            m_Children.Add(tID);
                        }
                    }

                    rs.Close();
                    rs.Dispose();
                }

                conn.Close();
                conn.Dispose();
            }
        }

        #region Static Methods

        public static List<Topic> GetTopics()
        {
            List<Topic> lt = new List<Topic>();

            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                using (IDataReader rs = DB.GetRS("select TopicID from Topic with(NOLOCK) where Deleted=0", conn))
                {
                    while (rs.Read())
                    {
                        Topic t = new Topic(DB.RSFieldInt(rs, "TopicID"));

                        lt.Add(t);
                    }

                    rs.Close();
                }

                conn.Close();
            }

            return lt;
        }

        public static String GetTitle(String TopicName, String LocaleSetting, int StoreID)
        {
            int tID = GetTopicID(TopicName, LocaleSetting, StoreID);

            return GetTitle(tID, LocaleSetting);
        }

        public static String GetTitle(int TopicID, String LocaleSetting)
        {
            String tmpS = String.Empty;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS(string.Format("select Title from Topic with (NOLOCK) where Deleted=0 and TopicID={2}", CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowTopicFiltering") == true, 1, 0), AppLogic.StoreID(), TopicID.ToString()), con))
 	 	        {
                    if (rs.Read())
                    {
                        tmpS = DB.RSFieldByLocale(rs, "Title", LocaleSetting);
                    }
                }
            }

            return tmpS;
        }

        public static String GetName(int TopicID, String LocaleSetting, int StoreID)
		{
			String tmpS = String.Empty;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS(string.Format("SELECT Name from Topic WITH (NOLOCK) WHERE Deleted=0 AND StoreID='" + AppLogic.StoreID() + "' AND TopicID='" + TopicID.ToString()) + "';", con))
                {
                    if (rs.Read())
                    {
                        tmpS = DB.RSFieldByLocale(rs, "Name", LocaleSetting);
                    }
                }
            }

			return tmpS;
		}

        public static int GetTopicID(string topicName)
        {
            return GetTopicID(topicName, AppLogic.StoreID());
        }

        public static int GetTopicID(string topicName, int StoreID)
        {
            return GetTopicID(topicName, Localization.GetDefaultLocale(), StoreID);
        }

        public static int GetTopicID(String TopicName, String LocaleSetting, int StoreID)
        {
            if (String.IsNullOrEmpty(LocaleSetting))
            {
                LocaleSetting = Localization.GetDefaultLocale();
            }
            string localeMatch = BuildLocaleSearchString(LocaleSetting, TopicName);
            int tmp = 0;
            String topicSQL = "select TopicID from Topic where (StoreID={1} or 0={0}) AND Deleted=0 AND Published=1 and (lower(Name)={2} OR Name like '%' + {3} + '%') order by storeid";
            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS(string.Format(topicSQL
                                                                    , CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowTopicFiltering") == true, 1, 0)
                                                                    , StoreID
                                                                    , DB.SQuote(TopicName.ToLowerInvariant())
                                                                    , DB.SQuote(localeMatch))
                                                , con))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSFieldInt(rs, "TopicID");
                    }
                }
            }

            if (tmp == 0)
            {
                StoreID = 0;
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS(string.Format(topicSQL
                                                                    , CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowTopicFiltering") == true, 1, 0)
                                                                    , StoreID
                                                                    , DB.SQuote(TopicName.ToLowerInvariant())
                                                                    , DB.SQuote(localeMatch))
                                                    , con))
                    {
                        if (rs.Read())
                        {
                            tmp = DB.RSFieldInt(rs, "TopicID");
                        }
                    }
                }
            }
            return tmp;
        }

        private static string BuildLocaleSearchString(string LocaleSetting, string TopicName)
        {
            return "<locale name=\"" + LocaleSetting + "\">" + TopicName + "</locale>";
        }

        #endregion


        public Topic CopyForStore(int sId)
        {
            string tsql = "select Name,SkinID,DisplayOrder,ContentsBGColor,PageBGColor,GraphicsColor,Title,Description,Password,RequiresSubscription,HTMLOk,RequiresDisclaimer,ShowInSiteMap,SEKeywords,SEDescription,SETitle,StoreID from Topic with (NOLOCK) where deleted=0 and topicid = "+this.TopicID;

            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS(tsql, conn))
                {
                    if (rs.Read())
                    {
                        StringBuilder sql = new StringBuilder(2500);
                        Guid NewGUID = System.Guid.NewGuid();
                        sql.Append("insert into Topic(TopicGUID,Name,SkinID,DisplayOrder,ContentsBGColor,PageBGColor,GraphicsColor,Title,Description,Password,RequiresSubscription,HTMLOk,RequiresDisclaimer,ShowInSiteMap,SEKeywords,SEDescription,SETitle,StoreID) values(");
                        sql.Append(DB.SQuote(NewGUID.ToString()) + ",");
                        sql.Append(DB.SQuote(DB.RSField(rs, "Name")) + ",");
                        sql.Append(DB.RSFieldInt(rs, "SkinID") + ",");
                        sql.Append(DB.RSFieldInt(rs, "DisplayOrder") + ",");
                        sql.Append(DB.SQuote(DB.RSField(rs, "ContentsBGColor")) + ",");
                        sql.Append(DB.SQuote(DB.RSField(rs, "PageBGColor")) + ",");
                        sql.Append(DB.SQuote(DB.RSField(rs, "GraphicsColor")) + ",");
                        sql.Append(DB.SQuote(DB.RSField(rs, "Title")) + ",");
                        sql.Append(DB.SQuote(DB.RSField(rs, "Description")) + ",");
                        sql.Append(DB.SQuote(DB.RSField(rs, "Password")) + ",");
                        sql.Append(CommonLogic.IIF(DB.RSFieldBool(rs, "RequiresSubscription"), 1, 0).ToString() + ",");
                        sql.Append(CommonLogic.IIF(DB.RSFieldBool(rs, "HTMLOk"), 1, 0).ToString() + ",");
                        sql.Append(CommonLogic.IIF(DB.RSFieldBool(rs, "RequiresDisclaimer"), 1, 0).ToString() + ",");
                        sql.Append(CommonLogic.IIF(DB.RSFieldBool(rs, "ShowInSiteMap"), 1, 0).ToString() + ",");
                        sql.Append(DB.SQuote(DB.RSField(rs, "SEKeywords")) + ",");
                        sql.Append(DB.SQuote(DB.RSField(rs, "SEDescription")) + ",");
                        sql.Append(DB.SQuote(DB.RSField(rs, "SETitle")) + ",");
                        sql.Append(sId.ToString());
                        sql.Append(")");
                        try
                        {
                            DB.ExecuteSQL(sql.ToString());
                        }
                        catch (Exception)
                        {
                            return null;
                        }
                        Topic newTopic = new Topic(NewGUID);
                        if (!string.IsNullOrEmpty(newTopic.TopicName))
                        {
                            return newTopic;
                        }
                    }
                }
            }


            return null;
        }
    }

    public class TopicEditEventArgs : EventArgs
    {
        public int TopicId;
        public Boolean NameChanged;
        public TopicEditEventArgs(int topicId, Boolean nameChanged)
        {
            this.TopicId = topicId;
            this.NameChanged = nameChanged;
        }
    }
}
