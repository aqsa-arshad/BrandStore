using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using AspDotNetStorefrontCore;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Collections;
using System.Xml;



namespace Vortx.VortxFramework
{
    public class ProductImageCollection
    {
        private int _ProductID;
        private int _SkinID;
        private string _Locale;
        private string _ImageFileNameOverride;
        private String _SKU;
        private Dictionary<ImageSize, ArrayList> _ImageCollection;
        private Dictionary<string, int> _ColorIndex;

        public Dictionary<ImageSize, ArrayList> ImageCollection
        {
            get { return _ImageCollection; }
        }

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
        }
        private string GetImage(ImageSize size, string color, int view)
        {
            string tmp = "";
            try
            {
               
                bool isicon = (size.ToString().ToLower() == "icon");
                if (color == "" && view == 0)
                {
                    tmp = AppLogic.LookupImage("product", _ProductID, size.ToString().ToLower(), _SkinID, _Locale);
                }
                else
                {
                    tmp = AppLogic.LookupProductImageByNumberAndColor(_ProductID, _SkinID, _ImageFileNameOverride, _SKU, _Locale, view, color, size.ToString().ToLower());
                }
            }
            catch (Exception ex)
            { 
            
            }
            return tmp;
        }

        public XmlDocument GetXMLBySize()
        {
            try
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

                            string extension = pic.Substring(pic.Length - 4, 4);
                            sb.Append("<View value=\"" + i.ToString() + "\" width=\"" + width + "\" height=\"" + height + "\">");
                            sb.Append(pic);
                            sb.Append("</View>\n");
                            //find the highest view
                            if (pic.Contains("nopicture") && !hasAddedEndView)
                            {
                                endView.Add(i - 1);
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
            catch (Exception ex)
            {

            }
            XmlDocument dd = new XmlDocument();          
            return dd;

        }

    }

    public class VariantImageCollection
    {
        private int _productId;
        private int _skinId;
        private string _locale;

        public VariantImageCollection(int productId, int skinId, string locale)
        {
            _productId = productId;
            _skinId = skinId;
            _locale = locale;

        }

        public XmlDocument GetXMLBySize()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<VariantImages>\n");
            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS(String.Format("select VariantID, Name, ImageFileNameOverride, IsDefault from ProductVariant with (NOLOCK) where ProductID = {0} order by DisplayOrder, VariantID", _productId), dbconn))
                {
                    while (rs.Read())
                    {
                        var name = DB.RSFieldByLocale(rs, "Name", _locale);
                        var variantId = DB.RSFieldInt(rs, "VariantID");
                        var isDefault = DB.RSFieldInt(rs, "IsDefault");
                        sb.Append(String.Format("<Variant name=\"{0}\" variantid=\"{1}\" isdefault=\"{2}\">\n", XmlConvert.EncodeName(name), variantId, isDefault));
                        foreach (ImageSize sz in Enum.GetValues(typeof(ImageSize)))
                        {
                            string variantImage = AppLogic.LookupImage("variant", variantId, sz.ToString().ToLower(), _skinId, _locale);
                            System.Drawing.Size defaultImageDimensions = CommonLogic.GetImagePixelSize(variantImage);
                            string defaultImageWidth = defaultImageDimensions.Width.ToString();
                            string defaultImageHeight = defaultImageDimensions.Height.ToString();

                            sb.Append(String.Format("<Image size=\"{0}\" width=\"{1}\" height=\"{2}\">", sz.ToString().ToLower(), defaultImageWidth, defaultImageHeight));
                            sb.Append(variantImage);
                            sb.Append("</Image>\n");
                        }
                        sb.Append("</Variant>\n");
                    }
                }
            }



            sb.Append("</VariantImages>\n");

            string ret = sb.ToString();
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
