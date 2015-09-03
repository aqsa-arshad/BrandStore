// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Web;
using System.Text.RegularExpressions;
using AspDotNetStorefrontCore;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Collections;
using System.Xml;



namespace Vortx.MobileFramework
{
    public class ProductImageCollection
    {
        private int _ProductID;
        private int _SkinID;
        private string _Locale;
        private string _ImageFileNameOverride;
        private String _SKU;
        private String[,,] _ImageArray;
        private Dictionary<string, int> _ColorIndex;


        public ProductImageCollection(int ProductID, string ImageFileNameOverride, string SKU, int SkinID, string Locale)
            : this(ProductID, ImageFileNameOverride, SKU, SkinID, Locale, string.Empty) { }
        public ProductImageCollection(int ProductID, string ImageFileNameOverride, string SKU, int SkinID, string Locale, string Colors)
        {
            _ProductID = ProductID;
            _SkinID = SkinID;
            _Locale = Locale;
            _ImageFileNameOverride = ImageFileNameOverride;
            _SKU = SKU;


            _ColorIndex = new Dictionary<string, int>();
            String[] colorArray;
            _ColorIndex.Add(String.Empty, 0);

            if (Colors != "")
            {
                colorArray = Colors.Split(',');
                for (int i = 0; i < colorArray.Length; i++)
                {
                    _ColorIndex.Add(AppLogic.RemoveAttributePriceModifier(colorArray[i]), i + 1);
                }
            }
            int colorInt;
            _ImageArray = new String[3, _ColorIndex.Count, 11];
            foreach (ImageSize sz in Enum.GetValues(typeof(ImageSize)))
            {
                foreach (String key in _ColorIndex.Keys)  
                {
                    colorInt = _ColorIndex[key];
                    for (int i = 0; i < 11; i++)
                    {
                        _ImageArray[(int)sz, colorInt, i] = GetImage(sz, key, i);
                    }
                }
            }
        }
        private string GetImage(ImageSize size, string color, int view)
        {
            string tmp = "";
            bool isicon = (size.ToString().ToLower() == "icon");
            if (color == "" && view == 0)
            {
                tmp = AppLogic.LookupImage("product", _ProductID, size.ToString().ToLower(), _SkinID, _Locale);
            }
            else
            {
                tmp = AppLogic.LookupProductImageByNumberAndColor(_ProductID, _SkinID, _ImageFileNameOverride, _SKU, _Locale, view, color, size.ToString().ToLower());
            }
            return tmp;
        }

        public XmlDocument GetXMLBySize()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<ProductImages maximageindex=\"{0}\" minimageindex=\"{1}\">\n");

            ArrayList endView = new ArrayList();
            bool hasAddedEndView;
            foreach (ImageSize sz in Enum.GetValues(typeof(ImageSize)))
            {
                sb.Append("<Size value=\"" + sz.ToString() + "\" >\n");

                string defaultImage = GetImage(sz, "", 0);

                System.Drawing.Size defaultImageDimensions = CommonLogic.GetImagePixelSize(defaultImage);
                string defaultImageWidth = defaultImageDimensions.Width.ToString();
                string defaultImageHeight = defaultImageDimensions.Height.ToString();

                sb.Append("<DefaultImage width=\"" + defaultImageWidth + "\" height=\"" + defaultImageHeight + "\">");
                sb.Append(GetImage(sz, "", 0));
                sb.Append("</DefaultImage>");
                foreach (String key in _ColorIndex.Keys)
                {
                    sb.Append("<Color value=\"" + key + "\">\n");
                    hasAddedEndView = false;
                    for (int i = 1; i < 11; i++)
                    {
                        string pic = GetImage(sz, key, i);
                        System.Drawing.Size imageDimensions = CommonLogic.GetImagePixelSize(pic);
                        string width = imageDimensions.Width.ToString();
                        string height = imageDimensions.Height.ToString();
                        
                        string extension = pic.Substring(pic.Length - 4,4);
                        sb.Append("<View value=\"" + i.ToString() + "\" width=\"" + width + "\" height=\"" + height + "\">");
                        sb.Append(pic);
                        sb.Append("</View>\n");
                        //find the highest view
                        if (pic.Contains("nopicture") && !hasAddedEndView)
	                    {
                    		endView.Add(i-1);
                            hasAddedEndView = true;
	                    }
                        else if (i == 10 && !hasAddedEndView)
                        {
                            endView.Add(i);
                            hasAddedEndView = true;
                        }
                    }
                    sb.Append("</Color>\n");
                }
                sb.Append("</Size>\n");
                endView.Sort();
                
            }

            sb.Append("</ProductImages>\n");

            int min = 0;
            int max = 0;
            if (endView.Count > 0)
            {
                min = (int)endView[0];
                max = (int)endView[endView.Count - 1];
            }

            string ret = string.Format(sb.ToString(), max.ToString(), min.ToString());
            XmlDocument d = new XmlDocument();
            d.LoadXml(ret);
            return d;
        }
    }


    public enum ImageSize
    {
        Icon,
        Medium,
        Large
    }
}
