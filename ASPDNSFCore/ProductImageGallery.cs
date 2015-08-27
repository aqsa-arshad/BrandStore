// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;
using System.Linq;

namespace AspDotNetStorefrontCore
{
	/// <summary>
	/// Summary description for ProductImageGallery.
	/// </summary>
	public class ProductImageGallery
	{

		private int m_ProductID;
		private int m_VariantID;
		private int m_SkinID;
		private String m_LocaleSetting;
		private String m_ProductSKU;
		private int m_MaxImageIndex; // will be 0 if empty
		private String m_Colors;
		private String[] m_ColorsSplit;
		private String m_ImageNumbers = "1,2,3,4,5,6,7,8,9,10";
		private String[] m_ImageNumbersSplit;
		private String m_ImgGalIcons;
		private String m_ImgDHTML;
		private String[,] m_ImageUrlsicon;
		private String[,] m_ImageUrlsmedium;
		private String[,] m_ImageUrlslarge;
		private bool m_HasSomeLarge;

		public ProductImageGallery(int ProductID, int SkinID, String LocaleSetting)
		{
			m_ProductID = ProductID;
			m_VariantID = AppLogic.GetDefaultProductVariant(m_ProductID);
			m_SkinID = SkinID;
			m_LocaleSetting = LocaleSetting;
			m_MaxImageIndex = 0;
			m_Colors = String.Empty;
			m_ImgGalIcons = String.Empty;
			m_ImgDHTML = String.Empty;
			m_HasSomeLarge = false;
			m_ProductSKU = String.Empty;
			LoadFromDB();
		}
		public ProductImageGallery(int ProductID, int SkinID, String LocaleSetting, string SKU)
		{
			m_ProductID = ProductID;
			m_VariantID = AppLogic.GetDefaultProductVariant(m_ProductID);
			m_SkinID = SkinID;
			m_LocaleSetting = LocaleSetting;
			m_MaxImageIndex = 0;
			m_Colors = String.Empty;
			m_ImgGalIcons = String.Empty;
			m_ImgDHTML = String.Empty;
			m_HasSomeLarge = false;
			m_ProductSKU = SKU;
			LoadFromDB();
		}
		public ProductImageGallery(int ProductID, string Colors, int SkinID, String LocaleSetting)
		{
			m_ProductID = ProductID;
			m_VariantID = AppLogic.GetDefaultProductVariant(m_ProductID);
			m_SkinID = SkinID;
			m_LocaleSetting = LocaleSetting;
			m_MaxImageIndex = 0;
			m_Colors = Colors;
			m_ImgGalIcons = String.Empty;
			m_ImgDHTML = String.Empty;
			m_HasSomeLarge = false;
			m_ProductSKU = String.Empty;
			LoadFromDB();
		}

		public ProductImageGallery(int ProductID, int VariantID, int SkinID, String LocaleSetting)
		{
			m_ProductID = ProductID;
			m_VariantID = VariantID;
			m_SkinID = SkinID;
			m_LocaleSetting = LocaleSetting;
			m_MaxImageIndex = 0;
			m_Colors = String.Empty;
			m_ImgGalIcons = String.Empty;
			m_ImgDHTML = String.Empty;
			m_HasSomeLarge = false;
			m_ProductSKU = String.Empty;
			LoadFromDB();
		}

		public ProductImageGallery(int ProductID, int VariantID, int SkinID, String LocaleSetting, string Colors)
		{
			m_ProductID = ProductID;
			m_VariantID = VariantID;
			m_SkinID = SkinID;
			m_LocaleSetting = LocaleSetting;
			m_MaxImageIndex = 0;
			m_Colors = Colors;
			m_ImgGalIcons = String.Empty;
			m_ImgDHTML = String.Empty;
			m_HasSomeLarge = false;
			m_ProductSKU = String.Empty;
			LoadFromDB();
		}

		public String ImgDHTML
		{
			get
			{
				return m_ImgDHTML;
			}
			set
			{
				m_ImgDHTML = value;
			}
		}

		public String ImgGalIcons
		{
			get
			{
				return m_ImgGalIcons;
			}
			set
			{
				m_ImgGalIcons = value;
			}
		}

		public bool HasSomeLarge
		{
			get
			{
				return m_HasSomeLarge;
			}
		}

		public bool IsEmpty()
		{
			return m_MaxImageIndex == 0;
		}

		public void LoadFromDB()
		{
			string suffix = "_" + m_ProductID.ToString();
			string pvsuffix = "_" + m_ProductID.ToString() + "_" + m_VariantID.ToString();
			m_ImageNumbersSplit = m_ImageNumbers.Split(',');
			bool m_WatermarksEnabled = AppLogic.AppConfigBool("Watermark.Enabled");


			m_ColorsSplit = new String[1] { "" };
			if(m_Colors == String.Empty)
			{
				using(SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
				{
					dbconn.Open();
					using(IDataReader rs = DB.GetRS("select Colors from productvariant   with (NOLOCK)  where VariantID=" + m_VariantID.ToString(), dbconn))
					{
						if(rs.Read())
						{
							m_Colors = DB.RSFieldByLocale(rs, "Colors", Localization.GetDefaultLocale()); // remember to add "empty" color to front, for no color selected
							if(m_Colors.Length != 0)
							{
								m_ColorsSplit = ("," + m_Colors).Split(',');
							}
						}
					}

				}

			}
			else
			{
				m_ColorsSplit = ("," + m_Colors).Split(',');
			}
			if(m_Colors.Length != 0)
			{
				for(int i = m_ColorsSplit.GetLowerBound(0); i <= m_ColorsSplit.GetUpperBound(0); i++)
				{
					String s2 = AppLogic.RemoveAttributePriceModifier(m_ColorsSplit[i]);
					m_ColorsSplit[i] = CommonLogic.MakeSafeFilesystemName(s2);
				}
			}

			if(AppLogic.AppConfigBool("MultiImage.UseProductIconPics"))
			{
				m_ImageUrlsicon = new String[m_ImageNumbersSplit.Length, m_ColorsSplit.Length];
				for(int x = m_ImageNumbersSplit.GetLowerBound(0); x <= m_ImageNumbersSplit.GetUpperBound(0); x++)
				{
					int ImgIdx = Localization.ParseUSInt(m_ImageNumbersSplit[x]);
					for(int i = m_ColorsSplit.GetLowerBound(0); i <= m_ColorsSplit.GetUpperBound(0); i++)
					{
						String Url = string.Empty;
						if(m_ProductSKU == string.Empty)
						{
							Url = AppLogic.LookupProductImageByNumberAndColor(m_ProductID, m_SkinID, m_LocaleSetting, ImgIdx, AppLogic.RemoveAttributePriceModifier(m_ColorsSplit[i]), "icon");
						}
						else
						{
							Url = AppLogic.LookupProductImageByNumberAndColor(m_ProductID, m_SkinID, m_ProductSKU, m_LocaleSetting, ImgIdx, AppLogic.RemoveAttributePriceModifier(m_ColorsSplit[i]), "icon");
						}
						if(m_WatermarksEnabled && Url.Length != 0 && Url.IndexOf("nopicture") == -1)
						{
							if (Url.StartsWith("/"))
							{
								m_ImageUrlsicon[x, i] = Url.Substring(HttpContext.Current.Request.ApplicationPath.Length);
							}
							else
							{
								m_ImageUrlsicon[x, i] = Url.Substring(HttpContext.Current.Request.ApplicationPath.Length - 1);
							}
							
							if(m_ImageUrlsicon[x, i].StartsWith("/"))
							{
								m_ImageUrlsicon[x, i] = m_ImageUrlsicon[x, i].TrimStart('/');
							}
						}
						else
						{
							m_ImageUrlsicon[x, i] = Url;
						}
					}
				}
				for(int x = m_ImageNumbersSplit.GetLowerBound(0); x <= m_ImageNumbersSplit.GetUpperBound(0); x++)
				{
					int ImgIdx = Localization.ParseUSInt(m_ImageNumbersSplit[x]);
					if(m_ImageUrlsicon[x, 0].IndexOf("nopicture") == -1)
					{
						m_MaxImageIndex = ImgIdx;
					}
				}
			}

			m_ImageUrlsmedium = new String[m_ImageNumbersSplit.Length, m_ColorsSplit.Length];
			for(int j = m_ImageNumbersSplit.GetLowerBound(0); j <= m_ImageNumbersSplit.GetUpperBound(0); j++)
			{
				int ImgIdx = Localization.ParseUSInt(m_ImageNumbersSplit[j]);
				for(int i = m_ColorsSplit.GetLowerBound(0); i <= m_ColorsSplit.GetUpperBound(0); i++)
				{
					String Url = string.Empty;
					if(m_ProductSKU == string.Empty)
					{
						Url = AppLogic.LookupProductImageByNumberAndColor(m_ProductID, m_SkinID, m_LocaleSetting, ImgIdx, AppLogic.RemoveAttributePriceModifier(m_ColorsSplit[i]), "medium");
					}
					else
					{
						Url = AppLogic.LookupProductImageByNumberAndColor(m_ProductID, m_SkinID, m_ProductSKU, m_LocaleSetting, ImgIdx, AppLogic.RemoveAttributePriceModifier(m_ColorsSplit[i]), "medium");
					}
					if(m_WatermarksEnabled && Url.Length != 0 && Url.IndexOf("nopicture") == -1)
					{
						if(Url.StartsWith("/"))
						{
							m_ImageUrlsmedium[j, i] = Url.Substring(HttpContext.Current.Request.ApplicationPath.Length);
						}
						else
						{
							m_ImageUrlsmedium[j, i] = Url.Substring(HttpContext.Current.Request.ApplicationPath.Length - 1);
						}
						
						if(m_ImageUrlsmedium[j, i].StartsWith("/"))
						{
							m_ImageUrlsmedium[j, i] = m_ImageUrlsmedium[j, i].TrimStart('/');
						}
					}
					else
					{
						m_ImageUrlsmedium[j, i] = Url;
					}
				}
			}
			for(int j = m_ImageNumbersSplit.GetLowerBound(0); j <= m_ImageNumbersSplit.GetUpperBound(0); j++)
			{
				int ImgIdx = Localization.ParseUSInt(m_ImageNumbersSplit[j]);
				if(m_ImageUrlsmedium[j, 0].IndexOf("nopicture") == -1)
				{
					m_MaxImageIndex = ImgIdx;
				}
			}

			m_ImageUrlslarge = new String[m_ImageNumbersSplit.Length, m_ColorsSplit.Length];
			for(int j = m_ImageNumbersSplit.GetLowerBound(0); j <= m_ImageNumbersSplit.GetUpperBound(0); j++)
			{
				int ImgIdx = Localization.ParseUSInt(m_ImageNumbersSplit[j]);
				for(int i = m_ColorsSplit.GetLowerBound(0); i <= m_ColorsSplit.GetUpperBound(0); i++)
				{
					String Url = string.Empty;
					if(m_ProductSKU == string.Empty)
					{
						Url = AppLogic.LookupProductImageByNumberAndColor(m_ProductID, m_SkinID, m_LocaleSetting, ImgIdx, AppLogic.RemoveAttributePriceModifier(m_ColorsSplit[i]), "large");
					}
					else
					{
						Url = AppLogic.LookupProductImageByNumberAndColor(m_ProductID, m_SkinID, m_ProductSKU, m_LocaleSetting, ImgIdx, AppLogic.RemoveAttributePriceModifier(m_ColorsSplit[i]), "large");
					}					

					if(m_WatermarksEnabled && Url.Length != 0 && Url.IndexOf("nopicture") == -1)
					{
						if(Url.StartsWith("/"))
						{
							m_ImageUrlslarge[j, i] = Url.Substring(HttpContext.Current.Request.ApplicationPath.Length);
						}
						else
						{
							m_ImageUrlslarge[j, i] = Url.Substring(HttpContext.Current.Request.ApplicationPath.Length - 1);
						}

						if(m_ImageUrlslarge[j, i].StartsWith("/"))
						{
							m_ImageUrlslarge[j, i] = m_ImageUrlslarge[j, i].TrimStart('/');
						}

						m_HasSomeLarge = true;
					}
					else if(Url.Length == 0 || Url.IndexOf("nopicture") != -1)
					{
						m_ImageUrlslarge[j, i] = String.Empty;
					}
					else
					{
						m_HasSomeLarge = true;
						m_ImageUrlslarge[j, i] = Url;
					}					
				}
			}

			if(!IsEmpty())
			{
				bool AttemptZoomify = AppLogic.AppConfigBool("Zoomify.Active") && (AppLogic.AppConfigBool("Zoomify.GalleryMedium") || AppLogic.AppConfigBool("Zoomify.ProductMedium"));
				bool GalleryZoomify = AttemptZoomify && AppLogic.AppConfigBool("Zoomify.GalleryMedium");

				StringBuilder tmpS = new StringBuilder(4096);
				tmpS.Append("<script type=\"text/javascript\">\n");
				tmpS.Append("var ProductPicIndex" + suffix + " = 1;\n");
				tmpS.Append("var ProductColor" + suffix + " = '';\n");
				tmpS.Append("var boardpics" + suffix + " = new Array();\n");
				tmpS.Append("var boardpicslg" + suffix + " = new Array();\n");
				tmpS.Append("var boardpicslgwidth" + suffix + " = new Array();\n");
				tmpS.Append("var boardpicslgheight" + suffix + " = new Array();\n");
				if(AttemptZoomify)
				{
					tmpS.Append("var boardpicsZ" + suffix + " = new Array();\n");
				}
				for(int i = 1; i <= m_MaxImageIndex; i++)
				{
					foreach(String c in m_ColorsSplit)
					{
						String MdUrl = ImageUrl(i, c, "medium").ToLowerInvariant();
						String MdWatermarkedUrl = MdUrl;

						if(m_WatermarksEnabled)
						{
							if(MdUrl.Length > 0)
							{
								string[] split = MdUrl.Split('/');
								string lastPart = split.Last();
								MdUrl = AppLogic.LocateImageURL(lastPart, "PRODUCT", "medium", "");
							} 
						}

						tmpS.Append("boardpics" + suffix + "['" + i.ToString() + "," + c + "'] = '" + MdWatermarkedUrl + "';\n");

						String LgUrl = ImageUrl(i, c, "large").ToLowerInvariant();
						String LgWatermarkedUrl = LgUrl;

						if(m_WatermarksEnabled)
						{
							if(LgUrl.Length > 0)
							{
								string[] split = LgUrl.Split('/');
								string lastPart = split.Last();
								LgUrl = AppLogic.LocateImageURL(lastPart, "PRODUCT", "large", "");
							}
						}

						tmpS.Append("boardpicslg" + suffix + "['" + i.ToString() + "," + c + "'] = '" + LgWatermarkedUrl + "';\n");
						
						if(LgUrl.Length > 0)
						{
							System.Drawing.Size lgsz = CommonLogic.GetImagePixelSize(LgUrl);
							tmpS.Append("boardpicslgwidth" + suffix + "['" + i.ToString() + "," + c + "'] = '" + lgsz.Width.ToString() + "';\n");
							tmpS.Append("boardpicslgheight" + suffix + "['" + i.ToString() + "," + c + "'] = '" + lgsz.Height.ToString() + "';\n");
						}

						if(AttemptZoomify)
						{							
							String ZMdUrl = string.Empty;

							// Yes we use the large url here, because the Zoomify data is always in Large
							if(LgUrl.Length > 0)
							{
								ZMdUrl = LgUrl.Remove(LgUrl.Length - 4); // remove extension
							}

							if(GalleryZoomify && CommonLogic.FileExists(CommonLogic.SafeMapPath(LgUrl)))
							{
								tmpS.Append("boardpicsZ" + suffix + "['" + i.ToString() + "," + c + "'] = '" + AppLogic.RunXmlPackage("Zoomify.Medium", null, null, m_SkinID, "", "ImagePath=" + ZMdUrl + "&AltSrc=" + LgUrl, false, false).Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ").Replace("'", "\\'") + "';\n"); // the Replace's are to make the xmlpackage output consumable by javascript
							}
							else
							{
								tmpS.Append("boardpicsZ" + suffix + "['" + i.ToString() + "," + c + "'] = '';\n");
							}
						}						
					}
				}

				if(AttemptZoomify)
				{
					tmpS.Append("function changeContent(markup)\n");
					tmpS.Append("{\n");
					tmpS.Append("	id='divProductPicZ" + m_ProductID.ToString() + "';\n");
					tmpS.Append("	if (document.getElementById || document.all)\n");
					tmpS.Append("	{\n");
					tmpS.Append("		var el = document.getElementById? document.getElementById(id): document.all[id];\n");
					tmpS.Append("		if (el && typeof el.innerHTML != \"undefined\") el.innerHTML = markup;\n");
					tmpS.Append("	}\n");
					tmpS.Append("}\n");
				}

				tmpS.Append("function changecolorimg" + suffix + "()\n");
				tmpS.Append("{\n");
				tmpS.Append("	var scidx = ProductPicIndex" + suffix + " + ',' + ProductColor" + suffix + ".toLowerCase();\n");
				
				if(AttemptZoomify)
				{
					tmpS.Append("if (boardpicsZ" + suffix + "[scidx]!='') {\n");
					tmpS.Append("  divProductPicZ" + m_ProductID.ToString() + ".style.display='inline';\n");
					tmpS.Append("  divProductPic" + m_ProductID.ToString() + ".style.display='none';\n");
					tmpS.Append("  changeContent(boardpicsZ" + suffix + "[scidx]); }\n");
					tmpS.Append("else {\n");
					tmpS.Append("  divProductPicZ" + m_ProductID.ToString() + ".style.display='none';\n");
					tmpS.Append("  divProductPic" + m_ProductID.ToString() + ".style.display='inline';\n");
					tmpS.Append("  document.ProductPic" + m_ProductID.ToString() + ".src=boardpics" + suffix + "[scidx]; }\n");
				}
				else
				{
					tmpS.Append("	document.ProductPic" + m_ProductID.ToString() + ".src=boardpics" + suffix + "[scidx];\n");
				}

				tmpS.Append("}\n");

				tmpS.Append("function popuplarge" + suffix + "()\n");
				tmpS.Append("{\n");
				tmpS.Append("	var scidx = ProductPicIndex" + suffix + " + ',' + ProductColor" + suffix + ".toLowerCase();\n");
				tmpS.Append("	var LargeSrc = boardpicslg" + suffix + "[scidx];\n");
				
				if (m_WatermarksEnabled) 
				{
					tmpS.AppendFormat("	var imageName = LargeSrc.split(\"/\").pop(-1);{0}", Environment.NewLine);
					tmpS.AppendFormat("	LargeSrc = 'watermark.axd?size=large&imgurl=images/product/large/' + imageName;{0}", Environment.NewLine);					
				}				
				tmpS.Append("if(boardpicslg" + suffix + "[scidx] != '')\n");
				tmpS.Append("{\n");
				tmpS.Append("	window.open('popup.aspx?src=' + LargeSrc,'LargerImage" + CommonLogic.GetRandomNumber(1, 100000) + "','toolbar=no,location=no,directories=no,status=no,menubar=no,scrollbars=" + CommonLogic.IIF(AppLogic.AppConfigBool("ResizableLargeImagePopup"), "yes", "no") + ",resizable=" + CommonLogic.IIF(AppLogic.AppConfigBool("ResizableLargeImagePopup"), "yes", "no") + ",copyhistory=no,width=' + boardpicslgwidth" + suffix + "[scidx] + ',height=' + boardpicslgheight" + suffix + "[scidx] + ',left=0,top=0');\n");
				tmpS.Append("}\n");
				tmpS.Append("else\n");
				tmpS.Append("{\n");
				tmpS.Append("	alert('There is no large image available for this picture');\n");
				tmpS.Append("}\n");
				tmpS.Append("}\n");

				tmpS.Append("function setcolorpicidx" + suffix + "(idx)\n");
				tmpS.Append("{\n");
				tmpS.Append("	ProductPicIndex" + suffix + " = idx;\n");
				tmpS.Append("	changecolorimg" + suffix + "();\n");
				tmpS.Append("}\n");
				
				tmpS.Append("function setActive(element)\n");
				tmpS.Append("{\n");
				tmpS.Append("	adnsf$('li.page-link').removeClass('active');\n");
				tmpS.Append("	adnsf$(element).parent().addClass('active');\n");
				tmpS.Append("}\n");

				tmpS.Append("function cleansizecoloroption" + suffix + "(theVal)\n");
				tmpS.Append("{\n");
				tmpS.Append("   if(theVal.indexOf('[') != -1){theVal = theVal.substring(0, theVal.indexOf('['))}");
				tmpS.Append("	theVal = theVal.replace(/[\\W]/g,\"\");\n");
				tmpS.Append("	theVal = theVal.toLowerCase();\n");
				tmpS.Append("	return theVal;\n");
				tmpS.Append("}\n");

				tmpS.Append("function setcolorpic" + suffix + "(color)\n");
				tmpS.Append("{\n");

				tmpS.Append("	while(color != unescape(color))\n");
				tmpS.Append("	{\n");
				tmpS.Append("		color = unescape(color);\n");
				tmpS.Append("	}\n");

				tmpS.Append("	if(color == '-,-' || color == '-')\n");
				tmpS.Append("	{\n");
				tmpS.Append("		color = '';\n");
				tmpS.Append("	}\n");

				tmpS.Append("	if(color != '' && color.indexOf(',') != -1)\n");
				tmpS.Append("	{\n");

				tmpS.Append("		color = color.substring(0,color.indexOf(',')).replace(new RegExp(\"'\", 'gi'), '');\n"); // remove sku from color select value

				tmpS.Append("	}\n");
				tmpS.Append("	if(color != '' && color.indexOf('[') != -1)\n");
				tmpS.Append("	{\n");

				tmpS.Append("	    color = color.substring(0,color.indexOf('[')).replace(new RegExp(\"'\", 'gi'), '');\n");
				tmpS.Append("		color = color.replace(/[\\s]+$/g,\"\");\n");

				tmpS.Append("	}\n");
				tmpS.Append("	ProductColor" + suffix + " = cleansizecoloroption" + suffix + "(color);\n");

				tmpS.Append("	changecolorimg" + suffix + "();\n");
				tmpS.Append("	setcolorlisttoactiveitem" + suffix + "(color);\n");
				tmpS.Append("	return (true);\n");
				tmpS.Append("}\n");

				// this one (without suffix) added back for backwards compatibility with older existing product data, where
				// the swatch map called to this js routine directly
				tmpS.Append("function setcolorpic(color)\n");
				tmpS.Append("{\n");

				tmpS.Append("	if(color == '-,-' || color == '-')\n");
				tmpS.Append("	{\n");
				tmpS.Append("		color = '';\n");
				tmpS.Append("	}\n");

				tmpS.Append("	if(color != '' && color.indexOf(',') != -1)\n");
				tmpS.Append("	{\n");

				tmpS.Append("		color = color.substring(0,color.indexOf(',')).replace(new RegExp(\"'\", 'gi'), '');\n"); // remove sku from color select value

				tmpS.Append("	}\n");
				tmpS.Append("	if(color != '' && color.indexOf('[') != -1)\n");
				tmpS.Append("	{\n");

				tmpS.Append("	    color = color.substring(0,color.indexOf('[')).replace(new RegExp(\"'\", 'gi'), '');\n");
				tmpS.Append("		color = color.replace(/[\\s]+$/g,\"\");\n");

				tmpS.Append("	}\n");
				tmpS.Append("	ProductColor" + suffix + " = cleansizecoloroption" + suffix + "(color);\n");

				tmpS.Append("	changecolorimg" + suffix + "();\n");
				tmpS.Append("	setcolorlisttoactiveitem" + suffix + "(color.toLowerCase());\n");
				tmpS.Append("	return (true);\n");
				tmpS.Append("}\n");

				tmpS.Append("function setcolorlisttoactiveitem" + suffix + "(color)\n");
				tmpS.Append("{\n");

				tmpS.Append("var lst = document.getElementById('Color" + pvsuffix + "');\n");

				tmpS.Append("var matchColor = cleansizecoloroption" + suffix + "(color);\n");

				tmpS.Append("for (var i=0; i < lst.length; i++)\n");
				tmpS.Append("   {\n");

				tmpS.Append("var value = lst[i].value;\n");
				tmpS.Append("var arrayValue = value.split(',');\n");
				tmpS.Append("var lstColor = cleansizecoloroption" + suffix + "(arrayValue[0]);\n");

				//tmpS.Append("	var lstColor = cleansizecoloroption" + suffix + "(lst[i].value);\n");

				tmpS.Append("   if (lstColor == matchColor)\n");
				tmpS.Append("      {\n");
				tmpS.Append("		lst.selectedIndex = i;\n");
				tmpS.Append("		return (true);\n");
				tmpS.Append("      }\n");
				tmpS.Append("   }\n");

				tmpS.Append("return (true);\n");
				tmpS.Append("}\n");

				tmpS.Append("</script>\n");
				m_ImgDHTML = tmpS.ToString();

				bool useMicros = AppLogic.AppConfigBool("UseImagesForMultiNav");

				bool microAction = CommonLogic.IIF(AppLogic.AppConfigBool("UseRolloverForMultiNav"), true, false);

				if(m_MaxImageIndex > 1)
				{
					tmpS.Remove(0, tmpS.Length);

					if(!AppLogic.AppConfigBool("MultiImage.UseProductIconPics") && !useMicros)
					{
						tmpS.Append("<ul class=\"pagination image-paging\">");
						for(int i = 1; i <= m_MaxImageIndex; i++)
						{
							if(i == 1)
								tmpS.Append("<li class=\"page-link active\">");
							else
								tmpS.Append("<li class=\"page-link\">");

							tmpS.Append(string.Format("<a href=\"javascript:void(0);\" onclick='setcolorpicidx{0}({1});setActive(this);' class=\"page-number\">{1}</a>", suffix, i));
							tmpS.Append("</li>");
						}
						tmpS.Append("</ul>");
					}
					else
					{
						tmpS.Append("<div class=\"product-gallery-items\">");
						for(int i = 1; i <= m_MaxImageIndex; i++)
						{
							tmpS.Append("<div class=\"product-gallery-item\">");
							tmpS.Append("	<div class=\"gallery-item-inner\">");
							if(AppLogic.AppConfigBool("MultiImage.UseProductIconPics"))
							{
								string strImageTag = "<img class='product-gallery-image' onclick='setcolorpicidx{0}({1});setImageURL(\"{2}\")' alt='Show Picture {1}' src='{2}' border='0' />";
								tmpS.AppendFormat(strImageTag, new object[]{
                                suffix,
                                i,
                                m_ImageUrlsicon[i - 1, 0].ToString()
                            });
							}
							else
							{
								// check for different extensions but don't let the non existance leave a gap
								// or crash because it can't find an image
								String ImageLoc = String.Empty;
								if(AppLogic.AppConfigBool("UseSKUForProductImageName"))
								{
									using(SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
									{
										dbconn.Open();
										using(IDataReader skus = DB.GetRS("SELECT p.SKU FROM Product p  with (NOLOCK)  WHERE p.ProductID=" + m_ProductID.ToString(), dbconn))
										{
											try
											{
												String microSKU = String.Empty;
												if(skus.Read())
												{
													microSKU = DB.RSField(skus, "SKU");
												}
												ImageLoc = AppLogic.LocateImageURL("images/product/micro/" + microSKU.ToString() + "_" + i.ToString() + ".gif");
												if(!CommonLogic.FileExists(ImageLoc))
													ImageLoc = AppLogic.LocateImageURL("images/product/micro/" + microSKU.ToString() + "_" + i.ToString() + "_" + ".gif");
												if(!CommonLogic.FileExists(ImageLoc))
													ImageLoc = AppLogic.LocateImageURL("images/product/micro/" + microSKU.ToString() + "_" + i.ToString() + ".jpg");
												if(!CommonLogic.FileExists(ImageLoc))
													ImageLoc = AppLogic.LocateImageURL("images/product/micro/" + microSKU.ToString() + "_" + i.ToString() + "_" + ".jpg");
												if(!CommonLogic.FileExists(ImageLoc))
													ImageLoc = AppLogic.LocateImageURL("images/product/micro/" + microSKU.ToString() + "_" + i.ToString() + ".png");
												if(!CommonLogic.FileExists(ImageLoc))
													ImageLoc = AppLogic.LocateImageURL("images/product/micro/" + microSKU.ToString() + "_" + i.ToString() + "_" + ".png");
												if(!CommonLogic.FileExists(ImageLoc))
													ImageLoc = AppLogic.LocateImageURL("App_Themes/skin_" + m_SkinID + "/images/nopicturemicro.gif");
											}
											catch { }
										}

									}

								}
								else
								{
									ImageLoc = AppLogic.LocateImageURL("images/product/micro/" + m_ProductID.ToString() + "_" + i.ToString() + ".gif");
									if(!CommonLogic.FileExists(ImageLoc))
										ImageLoc = AppLogic.LocateImageURL("images/product/micro/" + m_ProductID.ToString() + "_" + i.ToString() + "_" + ".gif");
									if(!CommonLogic.FileExists(ImageLoc))
										ImageLoc = AppLogic.LocateImageURL("images/product/micro/" + m_ProductID.ToString() + "_" + i.ToString() + ".jpg");
									if(!CommonLogic.FileExists(ImageLoc))
										ImageLoc = AppLogic.LocateImageURL("images/product/micro/" + m_ProductID.ToString() + "_" + i.ToString() + "_" + ".jpg");
									if(!CommonLogic.FileExists(ImageLoc))
										ImageLoc = AppLogic.LocateImageURL("images/product/micro/" + m_ProductID.ToString() + "_" + i.ToString() + ".png");
									if(!CommonLogic.FileExists(ImageLoc))
										ImageLoc = AppLogic.LocateImageURL("images/product/micro/" + m_ProductID.ToString() + "_" + i.ToString() + "_" + ".png");
									if(!CommonLogic.FileExists(ImageLoc))
										ImageLoc = AppLogic.LocateImageURL("App_Themes/skin_" + m_SkinID + "/images/nopicturemicro.gif");
								}

								// if not using rollover to change the images
								if(!microAction && ImageLoc.Length > 0)
								{
									string strImageTag = string.Format("<img class='product-gallery-image' onclick='setcolorpicidx{0}({1});setImageURL(\"{2}\")' alt='Show Picture {1}' src='{2}' border='0' />",
										new object[]
                                    {
                                        suffix, i,ImageLoc
                                    });
									tmpS.Append(strImageTag);
								}
								else if(ImageLoc.Length > 0)
								{
									string strImageTag = string.Format("<img class='product-gallery-image' onMouseOver='setcolorpicidx{0}({1});setImageURL(\"{2}\")' alt='Show Picture {1}' src='{2}' border='0' />",
										new object[]
                                    {
                                        suffix, i,ImageLoc
                                    });
									tmpS.Append(strImageTag);
								}
							}
							tmpS.Append("	</div>");
							tmpS.Append("</div>");
						}
						tmpS.Append("</div>");
					}

					m_ImgGalIcons = tmpS.ToString();
				}
			}
		}

		public int MaxImageIndex
		{
			get
			{
				return m_MaxImageIndex;
			}
		}

		private int GetColorIndex(String Color)
		{
			int i = 0;
			foreach(String s in m_ColorsSplit)
			{
				if(s == Color)
				{
					return i;
				}
				i++;
			}
			return 0;
		}

		public String ImageUrl(int Index, String Color, String ImgSize)
		{
			String s = ImgSize.ToLower(CultureInfo.InstalledUICulture);
			try
			{
				if(s == "icon")
				{
					return String.Empty;
				}
				else if(s == "medium")
				{
					return m_ImageUrlsmedium[Index - 1, GetColorIndex(Color)].Replace("//", "/");
				}
				else if(s == "large")
				{
					return m_ImageUrlslarge[Index - 1, GetColorIndex(Color)].Replace("//", "/"); ;
				}
				return String.Empty;
			}
			catch
			{
				return String.Empty;
			}
		}

	}

	public enum ProductImageSize
	{
		micro,
		icon,
		medium,
		large
	}
}
