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
	/// Summary description for ProductDescriptionFile: class finder for product Description description files (i.e. /descriptions/product/...*.htm) with locale support
	/// </summary>
	public class ProductDescriptionFile
	{
		private int m_SkinID = 1;
		private int m_ProductID = 0;
		private String m_Root = String.Empty;
		private String m_ProductSKU = String.Empty;
		private String m_LocaleSetting = String.Empty;

		private String m_Contents = String.Empty;
		private String m_ContentsRAW = String.Empty;
		private String m_FN = String.Empty;
		private String m_URL = String.Empty;

		public ProductDescriptionFile(int ProductID)
			: this(ProductID,Localization.GetDefaultLocale(),1)
		{}

		public ProductDescriptionFile(int ProductID, String LocaleSetting)
			: this(ProductID,LocaleSetting,1)
		{}

		public ProductDescriptionFile(int ProductID, int SkinID)
			: this(ProductID,Localization.GetDefaultLocale(),SkinID)
		{}

		public ProductDescriptionFile(int ProductID, String LocaleSetting, int SkinID)
		{
			m_ProductID = ProductID;
			m_LocaleSetting = LocaleSetting;
			m_SkinID = SkinID;
			m_ProductSKU = String.Empty;
			m_Contents = String.Empty;
			m_ContentsRAW = String.Empty;
			m_FN = String.Empty;

			// Find the Descriptionified ProductDescription content. will be in /descriptions/product or some locale subdir. find by productid or SKU, whichever is provided.
            m_Root = CommonLogic.IIF(AppLogic.IsAdminSite, "../", "") + "descriptions/product/";

			if (AppLogic.AppConfigBool("UseSKUForProductDescriptionName"))
			{
				//Try SKU version first
				m_ProductSKU = AppLogic.GetProductSKU(ProductID);
				if (!FindSKUFile())
				{
					FindIDFile();
				}
			}
			else
			{
				FindIDFile();
			}

			if(FN.Length != 0 && CommonLogic.FileExists(FN))
			{
				m_URL = Path.Combine(AppLogic.GetStoreHTTPLocation(false),m_URL);
				m_Contents = CommonLogic.ReadFile(FN,true);
				m_ContentsRAW = m_Contents;
				m_Contents = CommonLogic.ExtractBody(m_Contents);
				m_Contents = m_Contents.Replace("(!SKINID!)",SkinID.ToString());
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

		private bool FindSKUFile()
		{
			try
			{
				if (m_ProductSKU.Length !=0)
				{
					// try specified locale first
					m_URL = Path.Combine(m_Root,m_ProductSKU + "." + m_LocaleSetting + ".htm");
					m_FN = CommonLogic.SafeMapPath(m_URL);
					if(CommonLogic.FileExists(m_FN))
					{
						return true;
					}

					// try default store locale path:
					m_URL = Path.Combine(m_Root,m_ProductSKU + "." + Localization.GetDefaultLocale() + ".htm");
					m_FN = CommonLogic.SafeMapPath(m_URL);
					if(CommonLogic.FileExists(m_FN))
					{
						return true;
					}

					// try base (NULL) path:
					m_URL = Path.Combine(m_Root,m_ProductSKU + ".htm");
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
			if (m_ProductID != 0)
			{
				// try specified locale
				m_URL = Path.Combine(m_Root,m_ProductID.ToString() + "." + m_LocaleSetting + ".htm");
				m_FN = CommonLogic.SafeMapPath(m_URL);
				if(CommonLogic.FileExists(m_FN))
				{
					return true;
				}

				// try default store locale path:
				m_URL = Path.Combine(m_Root,m_ProductID.ToString() + "." + Localization.GetDefaultLocale() + ".htm");
				m_FN = CommonLogic.SafeMapPath(m_URL);
				if(CommonLogic.FileExists(m_FN))
				{
					return true;
				}
      
				// try skin (NULL) path:
				m_URL = Path.Combine(m_Root,m_ProductID.ToString() + ".htm");
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
