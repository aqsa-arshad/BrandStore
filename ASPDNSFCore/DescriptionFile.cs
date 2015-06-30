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
using System.Collections;
using System.IO;
using System.Net;
using System.Xml;
using System.Drawing;
using System.Xml.Serialization;
using System.Globalization;

namespace AspDotNetStorefrontCore
{
	/// <summary>
    /// Summary description for DescriptionFile: class finder for section/category description files (i.e. /descriptions/{descriptiontype}/...*.htm) with locale support
	/// </summary>
	public class DescriptionFile
	{

		private int m_ID = 0;
		private int m_SkinID = 1;
		private String m_Name = String.Empty;
		private String m_DescriptionType = String.Empty; // should be a valid entity name
		private String m_LocaleSetting = String.Empty;
		private String m_Root  = String.Empty;
		private String m_Contents = String.Empty;
		private String m_ContentsRAW = String.Empty;
		private String m_FN = String.Empty;
		private String m_URL = String.Empty;

		public DescriptionFile(String DescriptionType, int ID)
			: this(DescriptionType,ID,Localization.GetDefaultLocale(),1)
		{}

		public DescriptionFile(String DescriptionType, int ID, String LocaleSetting)
			: this(DescriptionType,ID,LocaleSetting,1)
		{}

		public DescriptionFile(String DescriptionType, int ID, int SkinID)
			: this(DescriptionType,ID,Localization.GetDefaultLocale(),SkinID)
		{}

		public DescriptionFile(String DescriptionType, int ID, String LocaleSetting, int SkinID)
		{
			m_DescriptionType = DescriptionType.Trim().ToLowerInvariant();
			m_ID = ID;
			m_LocaleSetting = LocaleSetting;
			m_SkinID = SkinID;
			// Find the description file content. will be in /{descriptiontype}descriptions or some locale subdir. find by ID
            m_Root = CommonLogic.IIF(AppLogic.IsAdminSite, "../", "") + "descriptions/" + m_DescriptionType.ToLowerInvariant() + "/";

			m_Contents = String.Empty;
			m_ContentsRAW = String.Empty;
			m_FN = String.Empty;

			if(AppLogic.AppConfigBool("UseNameForSectionDescriptionName"))
			{
				//Try SKU version first
				if(!FindNameFile())
				{
					FindIDFile();
				}
			}
			else
			{
				FindIDFile();
			}

			if(m_FN.Length != 0 && CommonLogic.FileExists(m_FN))
			{
				m_URL = Path.Combine(AppLogic.GetStoreHTTPLocation(false),m_URL);
				m_Contents = CommonLogic.ReadFile(m_FN,true);
				m_ContentsRAW = m_Contents;
				m_Contents = CommonLogic.ExtractBody(m_Contents);
				m_Contents = m_Contents.Replace("(!SKINID!)",SkinID.ToString());
			}
			else
			{
				m_URL = String.Empty;
				m_FN = String.Empty;
			}
		}

		public String FN
		{
			get 
			{
				return m_FN;
			}
		}

		public String URL
		{
			get 
			{
				return m_URL;
			}
		}

		public String Contents
		{
			get 
			{
				return m_Contents;
			}
		}

		public String ContentsRAW
		{
			get 
			{
				return m_ContentsRAW;
			}
		}

		public String LocaleSetting
		{
			get 
			{
				return m_LocaleSetting;
			}
		}

		public int SkinID
		{
			get 
			{
				return m_SkinID;
			}
		}

		private bool FindNameFile()
		{
			try
			{
				m_Name = AppLogic.GetEntityName(m_DescriptionType,m_ID,m_LocaleSetting);

				if (m_Name.Length !=0)
				{
					// try specified locale
					m_URL = Path.Combine(m_Root,m_Name + "." + m_LocaleSetting + ".htm");
					m_FN = CommonLogic.SafeMapPath(m_URL);
					if(CommonLogic.FileExists(m_FN))
					{
						return true;
					}

					// try default store locale path:
					m_URL = Path.Combine(m_Root,m_Name + "." + Localization.GetDefaultLocale() + ".htm");
					m_FN = CommonLogic.SafeMapPath(m_URL);
					if(CommonLogic.FileExists(m_FN))
					{
						return true;
					}

					// try base (NULL) path:
					m_URL = Path.Combine(m_Root,m_Name + ".htm");
					m_FN = CommonLogic.SafeMapPath(m_URL);
					if(CommonLogic.FileExists(m_FN))
					{
						return true;
					}
				}
				m_URL = String.Empty;
				m_FN = String.Empty;
				return false;
			}
			catch
			{
				return false;
			}
		}

		private bool FindIDFile()
		{
			if (m_ID != 0)
			{
				// try to locate by id.htm
				m_URL = Path.Combine(m_Root,m_ID.ToString() + "." + m_LocaleSetting + ".htm");
				m_FN = CommonLogic.SafeMapPath(m_URL);
				if(CommonLogic.FileExists(m_FN))
				{
					return true;
				}

				// try default store locale path:
				m_URL = Path.Combine(m_Root,m_ID.ToString() + "." + Localization.GetDefaultLocale() + ".htm");
				m_FN = CommonLogic.SafeMapPath(m_URL);
				if(CommonLogic.FileExists(m_FN))
				{
					return true;
				}
      
				// try skin (NULL) path:
				m_URL = Path.Combine(m_Root,m_ID.ToString() + ".htm");
				m_FN = CommonLogic.SafeMapPath(m_URL);
				if(CommonLogic.FileExists(m_FN))
				{
					return true;
				}
			}
			m_URL = String.Empty;
			m_FN = String.Empty;
			return false;
		}

	}
}
