// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Security;
using System.Web;
using System.Web.Configuration;
using System.Web.Util;
using System.Configuration;
using System.Web.SessionState;
using System.Web.Caching;
using System.Data;
using System.Text;
using System.Collections;
using System.IO;
using System.Net;
using System.Xml;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Resources;
using System.Reflection;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

namespace AspDotNetStorefrontCore
{
    /// <summary>
    /// Summary description for CommonLogic.
    /// </summary>
    public class CommonLogic
    {

        // this class now contains general support routines, but no "store" specific logic.
        // Store specific logic has been moved to the new AppLogic class

        static private Random RandomGenerator = new Random(System.DateTime.Now.Millisecond);

        public CommonLogic() { }

        static public String[] SupportedImageTypes = { ".jpg", ".gif", ".png" };

        static public string GenerateRandomCode(int NumDigits)
        {
            String s = "";
            for (int i = 1; i <= NumDigits; i++)
            {
                s += RandomGenerator.Next(10).ToString();
            }
            return s;
        }

        /// <summary>
        /// Gets the image extension for a given mime type
        /// </summary>
        /// <param name="mimeType"></param>
        /// <returns></returns>
        public static string ResolveExtensionFromMimeType(string mimeType)
        {
            string extension = string.Empty;

            switch (mimeType)
            {
                case "image/gif":
                    extension = ".gif";
                    break;
                case "image/png":
                case "image/x-png":
                    extension = ".png";
                    break;
                case "image/jpg":
                case "image/jpeg":
                case "image/pjpeg":
                    extension = ".jpg";
                    break;
            }

            return extension;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool IsStringNullOrEmpty(string value)
        {
            if (null == value) return true;

            return value.Length == 0 || value.Trim().Length == 0;
        }

        // this is good enough for what we need to use it for. it is not scientifically 100% valid for all cases!!
        static public bool StringIsAlreadyHTMLEncoded(String s)
        {
            return s.Contains("&nbsp;") || s.Contains("&quot;") || s.Contains("&amp;") || s.Contains("&lt;") || s.Contains("&gt;") || Regex.IsMatch(s, "&#[^;]+;", RegexOptions.Compiled);
        }

        static public String CleanLevelOne(String s)
        {
            // specify ALLOWED chars here, anything else is removed due to ^ (not) operator:
            return Regex.Replace(s, @"[^\w\s\.\-!@#\$%\^&\*\(\)\+=\?\/\{\}\[\]\\\|~`';:<>,_""]", "", RegexOptions.Compiled);
        }

        // allows only space chars
        static public String CleanLevelTwo(String s)
        {
            // specify ALLOWED chars here, anything else is removed due to ^ (not) operator:
            return Regex.Replace(s, @"[^\w \.\-!@#\$%\^&\*\(\)\+=\?\/\{\}\[\]\\\|~`';:<>,_""]", "", RegexOptions.Compiled);
        }

        // allows a-z, A-Z, 0-9 and space char, period, $ sign, % sign, and comma
        static public String CleanLevelThree(String s)
        {
            // specify ALLOWED chars here, anything else is removed due to ^ (not) operator:
            return Regex.Replace(s, @"[^\w \.\$%,]", "", RegexOptions.Compiled);
        }

        // allows a-z, A-Z, 0-9 and space char
        static public String CleanLevelFour(String s)
        {
            // specify ALLOWED chars here, anything else is removed due to ^ (not) operator:
            return Regex.Replace(s, @"[^\w ]", "", RegexOptions.Compiled);
        }

        // allows a-z, A-Z, 0-9
        static public String CleanLevelFive(String s)
        {
            // specify ALLOWED chars here, anything else is removed due to ^ (not) operator:
            return Regex.Replace(s, @"[^\w]", "", RegexOptions.Compiled);
        }

        /// <summary>
        /// Check String for invalid character
        /// </summary>
        /// <param name="s">The string to verify</param>
        /// <returns>Returns True if contains Invalid Character </returns>
        static public Boolean HasInvalidChar(String s)
        {
                              
            return !Regex.IsMatch(s,@"^[^<>`~!/@\#}$%:;)(_^{&*=|'+]+$");
                     
         }        

        /// <summary>
        /// Escapes input string from illegal javascript characters
        /// </summary>
        /// <param name="s">The input string</param>
        /// <returns>The javascript illegal chars escaped string</returns>
        public static string JavascriptEscape(string s)
        {
            StringBuilder sb = new StringBuilder(s);
            sb.Replace("\b", string.Empty);
            sb.Replace("\f", string.Empty);
            sb.Replace("\n", string.Empty);
            sb.Replace("\r", string.Empty);
            sb.Replace("\t", string.Empty);
            sb.Replace("'", "\\'");
            sb.Replace("\"", "\\\"");

            return sb.ToString();
        }

        static public System.Drawing.Image LoadImage(String url)
        {
            string imgName = SafeMapPath(url);
            Bitmap bmp = new Bitmap(imgName);
            return bmp;
        }

        // can use either text copyright, or image copyright, or both:
        // imgPhoto is image (memory) on which to add copyright text/mark
        static public System.Drawing.Image AddWatermark(System.Drawing.Image imgPhoto, String CopyrightText, String CopyrightImageUrl)
        {
            int phWidth = imgPhoto.Width;
            int phHeight = imgPhoto.Height;
            float WatermarkOpacity = AppLogic.AppConfigUSSingle("Watermark.Opacity");

            //create a Bitmap the Size of the original photograph
            Bitmap bmPhoto = new Bitmap(phWidth, phHeight, PixelFormat.Format24bppRgb);

            bmPhoto.SetResolution(imgPhoto.HorizontalResolution, imgPhoto.VerticalResolution);

            //load the Bitmap into a Graphics object 
            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;
            grPhoto.SmoothingMode = SmoothingMode.HighQuality;
            grPhoto.PixelOffsetMode = PixelOffsetMode.HighQuality;
            grPhoto.CompositingQuality = CompositingQuality.HighQuality;
            //create a image object containing the watermark
            Image imgWatermark = null;
            int wmWidth = 0;
            int wmHeight = 0;		
            string imageUrl = SafeMapPath(CopyrightImageUrl).ToLower();
                
			if(FileExists(imageUrl))
			{
				if(imageUrl.IndexOf("controls") != -1)
				{
					imageUrl = imageUrl.Replace("\\controls", "");
				}
				imgWatermark = new Bitmap(imageUrl);
				wmWidth = imgWatermark.Width;
				wmHeight = imgWatermark.Height;             
			}

            //------------------------------------------------------------
            //Step #1 - Insert Copyright message
            //------------------------------------------------------------

            //Set the rendering quality for this Graphics object
            grPhoto.SmoothingMode = SmoothingMode.AntiAlias;

            //Draws the photo Image object at original size to the graphics object.
            grPhoto.DrawImage(
                imgPhoto,                               // Photo Image object
                new Rectangle(0, 0, phWidth, phHeight), // Rectangle structure
                0,                                      // x-coordinate of the portion of the source image to draw. 
                0,                                      // y-coordinate of the portion of the source image to draw. 
                phWidth,                                // Width of the portion of the source image to draw. 
                phHeight,                               // Height of the portion of the source image to draw. 
                GraphicsUnit.Pixel);                    // Units of measure 

            //-------------------------------------------------------
            //to maximize the size of the Copyright message we will 
            //test multiple Font sizes to determine the largest posible 
            //font we can use for the width of the Photograph
            //define an array of point sizes you would like to consider as possiblities
            //-------------------------------------------------------
            int[] sizes = new int[] { 16, 14, 12, 10, 8, 6, 4 };

            Font crFont = null;
            SizeF crSize = new SizeF();

            //Loop through the defined sizes checking the length of the Copyright string
            //If its length in pixles is less then the image width choose this Font size.
            for (int i = 0; i < 7; i++)
            {
                //set a Font object to Arial (i)pt, Bold
                crFont = new Font("arial", sizes[i], FontStyle.Bold);
                //Measure the Copyright string in this Font
                crSize = grPhoto.MeasureString(CopyrightText, crFont);

                if ((ushort)crSize.Width < (ushort)phWidth)
                    break;
            }

            //Since all photographs will have varying heights, determine a 
            //position 5% from the bottom of the image
            int OffsetPercentage = AppLogic.AppConfigUSInt("Watermark.OffsetFromBottomPercentage");
            if (OffsetPercentage == 0)
            {
                OffsetPercentage = 5;
            }
            int yPixlesFromBottom = (int)(phHeight * (OffsetPercentage / 100.0));

            //Now that we have a point size use the Copyrights string height 
            //to determine a y-coordinate to draw the string of the photograph
            float yPosFromBottom = ((phHeight - yPixlesFromBottom) - (crSize.Height / 2.0F));

            //Determine its x-coordinate by calculating the center of the width of the image
            float xCenterOfImg = (phWidth / 2);

            //Define the text layout by setting the text alignment to centered
            StringFormat StrFormat = new StringFormat();
            StrFormat.Alignment = StringAlignment.Center;

            int textOpacity = (int)(153 * WatermarkOpacity);
            //define a Brush which is semi trasparent black (Alpha set to 153)
            SolidBrush semiTransBrush2 = new SolidBrush(Color.FromArgb(textOpacity, 0, 0, 0));

            //Draw the Copyright string
            grPhoto.DrawString(CopyrightText,                 //string of text
                crFont,                                   //font
                semiTransBrush2,                           //Brush
                new PointF(xCenterOfImg + 1, yPosFromBottom + 1),  //Position
                StrFormat);

            //define a Brush which is semi trasparent white (Alpha set to 153)
            SolidBrush semiTransBrush = new SolidBrush(Color.FromArgb(textOpacity, 255, 255, 255));

            //Draw the Copyright string a second time to create a shadow effect
            //Make sure to move this text 1 pixel to the right and down 1 pixel
            grPhoto.DrawString(CopyrightText,                 //string of text
                crFont,                                   //font
                semiTransBrush,                           //Brush
                new PointF(xCenterOfImg, yPosFromBottom),  //Position
                StrFormat);                               //Text alignment

            //------------------------------------------------------------
            //Step #2 - Insert Watermark image
            //------------------------------------------------------------
            if (imgWatermark != null)
            {
                //Create a Bitmap based on the previously modified photograph Bitmap
                Bitmap bmWatermark = new Bitmap(bmPhoto);
                bmWatermark.SetResolution(imgPhoto.HorizontalResolution, imgPhoto.VerticalResolution);
                //Load this Bitmap into a new Graphic Object
                Graphics grWatermark = Graphics.FromImage(bmWatermark);

                //To achieve a transulcent watermark we will apply (2) color 
                //manipulations by defineing a ImageAttributes object and 
                //seting (2) of its properties.
                ImageAttributes imageAttributes = new ImageAttributes();

                //The first step in manipulating the watermark image is to replace 
                //the background color with one that is trasparent (Alpha=0, R=0, G=0, B=0)
                //to do this we will use a Colormap and use this to define a RemapTable
                ColorMap colorMap = new ColorMap();

                //Watermark image should be defined with a background of 100% Green this will
                //be the color we search for and replace with transparency
                colorMap.OldColor = Color.FromArgb(255, 0, 255, 0);
                colorMap.NewColor = Color.FromArgb(0, 0, 0, 0);

                ColorMap[] remapTable = { colorMap };

                imageAttributes.SetRemapTable(remapTable, ColorAdjustType.Bitmap);

                //The second color manipulation is used to change the opacity of the 
                //watermark.  This is done by applying a 5x5 matrix that contains the 
                //coordinates for the RGBA space.  By setting the 3rd row and 3rd column 
                //to 0.1f we achive a level of opacity
                float[][] colorMatrixElements = {   new float[] {1.0f,  0.0f,  0.0f,  0.0f, 0.0f},       
													new float[] {0.0f,  1.0f,  0.0f,  0.0f, 0.0f},        
													new float[] {0.0f,  0.0f,  1.0f,  0.0f, 0.0f},        
													new float[] {0.0f,  0.0f,  0.0f,  0.1f, 0.0f},        
													new float[] {0.0f,  0.0f,  0.0f,  0.0f, 1.0f}};
                
                if (WatermarkOpacity != 0.0F)
                {
                    colorMatrixElements[3][3] = WatermarkOpacity;
                }
                ColorMatrix wmColorMatrix = new ColorMatrix(colorMatrixElements);

                imageAttributes.SetColorMatrix(wmColorMatrix, ColorMatrixFlag.Default,
                    ColorAdjustType.Bitmap);

                //For this example we will place the watermark in center of the photograph.

                int xPosOfWm = ((phWidth - wmWidth) / 2);
                int yPosOfWm = ((phHeight - wmHeight) / 2);

                grWatermark.DrawImage(imgWatermark,
                    new Rectangle(xPosOfWm, yPosOfWm, wmWidth, wmHeight),  //Set the detination Position
                    0,                  // x-coordinate of the portion of the source image to draw. 
                    0,                  // y-coordinate of the portion of the source image to draw. 
                    wmWidth,            // Watermark Width
                    wmHeight,		    // Watermark Height
                    GraphicsUnit.Pixel, // Unit of measurment
                    imageAttributes);   //ImageAttributes Object
                bmPhoto = bmWatermark;
                grWatermark.Dispose();
            }
            grPhoto.Dispose();
            if (imgWatermark != null)
            {
                imgWatermark.Dispose();
            }
            return bmPhoto;
        }

        static public String PageParamsAsXml(bool IncludeRootNode, String RootNodeName, bool ExcludeVldtFields)
        {
            StringBuilder tmpS = new StringBuilder(4096);
            if (IncludeRootNode)
            {
                if (RootNodeName.Length == 0)
                {
                    tmpS.Append("<root>");
                }
                else
                {
                    tmpS.Append("<" + RootNodeName + ">");
                }
            }
            tmpS.Append("<QueryString>");
            for (int i = 0; i <= HttpContext.Current.Request.QueryString.Count - 1; i++)
            {
                String FieldName = HttpContext.Current.Request.QueryString.Keys[i].ToString();
                String FieldValue = HttpContext.Current.Request.QueryString[HttpContext.Current.Request.QueryString.Keys[i]].ToString();
                if (!ExcludeVldtFields || !FieldName.EndsWith("_vldt", StringComparison.InvariantCultureIgnoreCase))
                {
                    tmpS.Append(String.Format("<{0}>{1}</{2}>", FieldName, XmlCommon.XmlEncode(FieldValue), FieldName));
                }
            }
            tmpS.Append("</QueryString>");

            tmpS.Append("<Form>");
            for (int i = 0; i <= HttpContext.Current.Request.Form.Count - 1; i++)
            {
                String FieldName = HttpContext.Current.Request.Form.Keys[i].ToString();
                String FieldValue = HttpContext.Current.Request.Form[HttpContext.Current.Request.Form.Keys[i]].ToString();
                if (!ExcludeVldtFields || !FieldName.EndsWith("_vldt", StringComparison.InvariantCultureIgnoreCase))
                {
                    tmpS.Append(String.Format("<{0}>{1}</{2}>", FieldName, XmlCommon.XmlEncode(FieldValue), FieldName));
                }
            }
            tmpS.Append("</Form>");

            if (IncludeRootNode)
            {
                if (RootNodeName.Length == 0)
                {
                    tmpS.Append("</root>");
                }
                else
                {
                    tmpS.Append("</" + RootNodeName + ">");
                }
            }
            return tmpS.ToString();
        }

        static public String UTF8ByteArrayToString(Byte[] characters)
        {

            UTF8Encoding encoding = new UTF8Encoding();
            String constructedString = encoding.GetString(characters);
            return constructedString;
        }

        static public Byte[] StringToUTF8ByteArray(String pXmlString)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            Byte[] byteArray = encoding.GetBytes(pXmlString);
            return byteArray;
        }

        static public bool IntegerIsInIntegerList(int SearchInt, String ListOfInts)
        {
            String MasterList = ListOfInts.Replace(" ", "").Trim();
            if (MasterList.Length == 0)
            {
                return false;
            }
            String target = SearchInt.ToString();
            foreach (string spat in MasterList.Split(','))
            {
                if (target == spat)
                {
                    return true;
                }
            }
            return false;
        }

        static public String GenerateHtmlEditor(String FieldID)
        {
            StringBuilder tmpS = new StringBuilder(4096);
            tmpS.Append("\n<script type=\"text/javascript\">\n<!--\n");
            tmpS.Append("editor_generate('" + FieldID + "');\n\n");
            tmpS.Append("//-->\n</script>\n");
            return tmpS.ToString();
        }

        static public long GetImageSize(String imgname)
        {
            String imgfullpath = SafeMapPath(imgname);
            try
            {
                FileInfo fi = new FileInfo(imgfullpath);
                long l = fi.Length;
                fi = null;
                return l;
            }
            catch
            {
                return 0;
            }
        }

        static public String GetFormInput(bool ExcludeVldtFields, String separator)
        {
            StringBuilder tmpS = new StringBuilder(10000);
            bool first = true;
            for (int i = 0; i < HttpContext.Current.Request.Form.Count; i++)
            {
                bool okField = true;
                if (ExcludeVldtFields)
                {
                    if (HttpContext.Current.Request.Form.Keys[i].ToUpperInvariant().IndexOf("_VLDT") != -1)
                    {
                        okField = false;
                    }
                }
                if (HttpContext.Current.Request.Form.Keys[i].ToUpperInvariant() == "__EVENTTARGET" || HttpContext.Current.Request.Form.Keys[i].ToUpperInvariant() == "__EVENTARGUMENT" || HttpContext.Current.Request.Form.Keys[i].ToUpperInvariant() == "__VIEWSTATE")
                {
                    okField = false;
                }
                if (okField)
                {
                    if (!first)
                    {
                        tmpS.Append(separator);
                    }
                    tmpS.Append("<b>" + HttpContext.Current.Request.Form.Keys[i] + "</b>=" + HttpContext.Current.Request.Form[HttpContext.Current.Request.Form.Keys[i]]);
                    first = false;
                }
            }
            return tmpS.ToString();
        }

        static public String GetHTTPRequestAsXml()
        {
            StringBuilder tmpS = new StringBuilder(1024);
            tmpS.Append("<request>");
            int loop1 = 0;
            int loop2 = 0;
            NameValueCollection coll;

            // Load Header collection into NameValueCollection object. 
            coll = HttpContext.Current.Request.Headers;

            // Put the names of all keys into a string array. 
            String[] arr1 = coll.AllKeys;
            for (loop1 = 0; loop1 < arr1.Length; loop1++)
            {
                tmpS.Append("<key id=\"" + XmlCommon.XmlEncodeAttribute(arr1[loop1]) + "\">");
                // Get all values under this key. 
                String[] arr2 = coll.GetValues(arr1[loop1]);
                for (loop2 = 0; loop2 < arr2.Length; loop2++)
                {
                    tmpS.Append("<value>");
                    tmpS.Append(XmlCommon.XmlEncode(arr2[loop2]));
                    tmpS.Append("</value>");
                }
                tmpS.Append("</key>");
            }
            tmpS.Append("</request>");
            return tmpS.ToString();
        }
        
        static public String GetQueryStringInput(bool ExcludeVldtFields, String separator)
        {
            return GetQueryStringInput(ExcludeVldtFields, separator, true);
        }

        static public String GetQueryStringInput(bool ExcludeVldtFields, String separator, bool includeTags)
        {
            return GetQueryStringInput(ExcludeVldtFields, separator, true, null);
        }

        static public String GetQueryStringInput(bool ExcludeVldtFields, String separator, bool includeTags, List<String> ExcludeParams)
        {
            StringBuilder tmpS = new StringBuilder(10000);
            bool first = true;
            bool skipParams = ExcludeParams != null && ExcludeParams.Count > 0;

            for (int i = 0; i < HttpContext.Current.Request.QueryString.Count; i++)
            {
                bool okField = true;
                if (ExcludeVldtFields)
                {
                    if (HttpContext.Current.Request.QueryString.Keys[i].ToUpperInvariant().IndexOf("_VLDT") != -1)
                    {
                        okField = false;
                    }
                }
                if (skipParams)
                {
                    if (ExcludeParams.Contains(HttpContext.Current.Request.QueryString.Keys[i].ToLowerInvariant()))
                    {
                        okField = false;
                    }
                }
                if (okField)
                {
                    if (!first)
                    {
                        tmpS.Append(separator);
                    }

                    if (includeTags)
                    {
                        tmpS.Append("<b>" + HttpContext.Current.Request.QueryString.Keys[i] + "</b>=" + HttpContext.Current.Request.QueryString[HttpContext.Current.Request.QueryString.Keys[i]]);
                    }
                    else
                    {
                        tmpS.Append(HttpContext.Current.Request.QueryString.Keys[i] + "=" + HttpContext.Current.Request.QueryString[HttpContext.Current.Request.QueryString.Keys[i]]);
                    }
                    first = false;
                }
            }
            return tmpS.ToString();
        }

        static public String GetFormInputAsXml(bool ExcludeVldtFields, String RootNode)
        {
            StringBuilder tmpS = new StringBuilder(10000);
            tmpS.Append("<" + RootNode + ">");
            for (int i = 0; i < HttpContext.Current.Request.Form.Count; i++)
            {
                bool okField = true;
                if (ExcludeVldtFields)
                {
                    if (HttpContext.Current.Request.Form.Keys[i].ToUpperInvariant().IndexOf("_VLDT") != -1)
                    {
                        okField = false;
                    }
                }
                if (okField)
                {
                    String nodename = XmlCommon.XmlEncode(HttpContext.Current.Request.Form.Keys[i]);
                    String nodeval = XmlCommon.XmlEncode(HttpContext.Current.Request.Form[HttpContext.Current.Request.Form.Keys[i]]);
                    tmpS.Append("<" + nodename + ">");
                    tmpS.Append(nodeval);
                    tmpS.Append("</" + nodename + ">");
                }
            }
            tmpS.Append("</" + RootNode + ">");
            return tmpS.ToString();
        }

        static public String GetQueryStringInputAsXml(bool ExcludeVldtFields, String RootNode)
        {
            StringBuilder tmpS = new StringBuilder(10000);
            tmpS.Append("<" + RootNode + ">");
            for (int i = 0; i < HttpContext.Current.Request.QueryString.Count; i++)
            {
                bool okField = true;
                if (ExcludeVldtFields)
                {
                    if (HttpContext.Current.Request.QueryString.Keys[i].ToUpperInvariant().IndexOf("_VLDT") != -1)
                    {
                        okField = false;
                    }
                }
                if (okField)
                {
                    String nodename = XmlCommon.XmlEncode(HttpContext.Current.Request.QueryString.Keys[i]);
                    String nodeval = XmlCommon.XmlEncode(HttpContext.Current.Request.QueryString[HttpContext.Current.Request.QueryString.Keys[i]]);
                    tmpS.Append("<" + nodename + ">");
                    tmpS.Append(nodeval);
                    tmpS.Append("</" + nodename + ">");
                }
            }
            tmpS.Append("</" + RootNode + ">");
            return tmpS.ToString();
        }

        static public int IIF(bool condition, int a, int b)
        {
            int x = 0;
            if (condition)
            {
                x = a;
            }
            else
            {
                x = b;
            }
            return x;
        }

        static public bool IIF(bool condition, bool a, bool b)
        {
            bool x = false;
            if (condition)
            {
                x = a;
            }
            else
            {
                x = b;
            }
            return x;
        }

        static public Single IIF(bool condition, Single a, Single b)
        {
            float x = 0;
            if (condition)
            {
                x = a;
            }
            else
            {
                x = b;
            }
            return x;
        }

        static public Double IIF(bool condition, double a, double b)
        {
            double x = 0;
            if (condition)
            {
                x = a;
            }
            else
            {
                x = b;
            }
            return x;
        }

        static public decimal IIF(bool condition, decimal a, decimal b)
        {
            decimal x = 0;
            if (condition)
            {
                x = a;
            }
            else
            {
                x = b;
            }
            return x;
        }

        static public String IIF(bool condition, String a, String b)
        {
            String x = String.Empty;
            if (condition)
            {
                x = a;
            }
            else
            {
                x = b;
            }
            return x;
        }

		/// <summary>
		/// Accepts an object and if that object is null then the value passed to replacementValue will be returned.  If not null then the object value is returned.
		/// </summary>
		/// <param name="testValue">Value to test for null</param>
		/// <param name="replacementValue">Value returned if testValue is null</param>
		/// <returns></returns>
		static public T IsNull<T>(T testValue, T replacementValue)
		{
			if(testValue == null)
				return replacementValue;
			else
				return testValue;
		}

        public static int Min(int a, int b)
        {
            if (a < b)
            {
                return a;
            }
            return b;
        }

        public static int Max(int a, int b)
        {
            if (a > b)
            {
                return a;
            }
            return b;
        }

        public static decimal Min(decimal a, decimal b)
        {
            if (a < b)
            {
                return a;
            }
            return b;
        }

        public static decimal Max(decimal a, decimal b)
        {
            if (a > b)
            {
                return a;
            }
            return b;
        }

        public static Single Min(Single a, Single b)
        {
            if (a < b)
            {
                return a;
            }
            return b;
        }

        public static Single Max(Single a, Single b)
        {
            if (a > b)
            {
                return a;
            }
            return b;
        }

        public static DateTime Min(DateTime a, DateTime b)
        {
            if (a < b)
            {
                return a;
            }
            return b;
        }

        public static DateTime Max(DateTime a, DateTime b)
        {
            if (a > b)
            {
                return a;
            }
            return b;
        }

        public static String PageInvocation()
        {
            return HttpContext.Current.Request.RawUrl;
        }

        public static String PageReferrer()
        {
            try
            {
                if (HttpContext.Current.Request.UrlReferrer == null)
                {
                    return String.Empty;
                }
                else
                {
                    return HttpContext.Current.Request.UrlReferrer.ToString();
                }
            }
            catch
            { }
            return String.Empty;
        }

        static public String GetThisPageName(bool includePath)
        {
            String s = CommonLogic.ServerVariables("SCRIPT_NAME");
            if (!includePath)
            {
                int ix = s.LastIndexOf("/");
                if (ix != -1)
                {
                    s = s.Substring(ix + 1);
                }
            }
            return s;
        }

        static public String GetVersion()
        {
            Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            object[] attributes = a.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
            string strAssemblyDescription = ((AssemblyProductAttribute)attributes[0]).Product;
            return strAssemblyDescription + " " + a.GetName().Version.ToString() + "/" + AppLogic.AppConfig("StoreVersion");
        }

        public static String GetPhoneDisplayFormat(String PhoneNumber)
        {
            if (PhoneNumber.Length == 0)
            {
                return String.Empty;
            }
            if (PhoneNumber.Length != 11)
            {
                return PhoneNumber;
            }
            return "(" + PhoneNumber.Substring(1, 3) + ") " + PhoneNumber.Substring(4, 3) + "-" + PhoneNumber.Substring(7, 4);
        }

        public static bool IsNumber(string expression)
        {
            expression = expression.Trim();
            if (expression.Length == 0)
            {
                return false;
            }
            expression = expression.Replace(",", "").Replace(".", "").Replace("-", "");
            for (int i = 0; i < expression.Length; i++)
            {
                // only allow numeric digits
                if (!char.IsNumber(expression[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsInteger(string expression)
        {
            if (expression.Trim().Length == 0)
            {
                return false;
            }
            // leading - is ok
            expression = expression.Trim();
            int startIdx = 0;
            if (expression.StartsWith("-"))
            {
                startIdx = 1;
            }
            for (int i = startIdx; i < expression.Length; i++)
            {
                if (!char.IsNumber(expression[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsDate(string expression)
        {
            if (expression.Trim().Length == 0)
            {
                return false;
            }
            try
            {
                DateTime.Parse(expression);
                return true;
            }
            catch
            {
                return false;
            }
        }

        static public int GetRandomNumber(int lowerBound, int upperBound)
        {
            return new System.Random().Next(lowerBound, upperBound + 1);
        }

        static public String GetExceptionDetail(Exception ex, String LineSeparator)
        {
            String ExDetail = "Exception=" + ex.Message + LineSeparator;
            while (ex.InnerException != null)
            {
                ExDetail += ex.InnerException.Message + LineSeparator;
                ex = ex.InnerException;
            }
            return ExDetail;
        }

        static public String HighlightTerm(String InString, String Term)
        {
            int i = InString.ToUpperInvariant().IndexOf(Term.ToUpperInvariant());
            if (i != -1)
            {
                InString = InString.Substring(0, i) + "<b>" + InString.Substring(i, Term.Length) + "</b>" + InString.Substring(i + Term.Length, InString.Length - Term.Length - i);
            }
            return InString;
        }

        static public String BuildStarsImage(Decimal d, int SkinID)
        {
            String s = String.Empty;
            if (d < 0.25M)
            {
                s = "<img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/stare.gif") + "\"><img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/stare.gif") + "\"><img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/stare.gif") + "\"><img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/stare.gif") + "\"><img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/stare.gif") + "\" />";
            }
            else if (d >= 0.25M && d < 0.75M)
            {
                s = "<img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/starh.gif") + "\"><img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/stare.gif") + "\"><img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/stare.gif") + "\"><img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/stare.gif") + "\"><img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/stare.gif") + "\" />";
            }
            else if (d >= 0.75M && d < 1.25M)
            {
                s = "<img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/starf.gif") + "\"><img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/stare.gif") + "\"><img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/stare.gif") + "\"><img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/stare.gif") + "\"><img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/stare.gif") + "\" />";
            }
            else if (d >= 1.25M && d < 1.75M)
            {
                s = "<img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/starf.gif") + "\"><img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/starh.gif") + "\"><img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/stare.gif") + "\"><img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/stare.gif") + "\"><img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/stare.gif") + "\" />";
            }
            else if (d >= 1.75M && d < 2.25M)
            {
                s = "<img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/starf.gif") + "\"><img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/starf.gif") + "\"><img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/stare.gif") + "\"><img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/stare.gif") + "\"><img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/stare.gif") + "\" />";
            }
            else if (d >= 2.25M && d < 2.75M)
            {
                s = "<img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/starf.gif") + "\"><img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/starf.gif") + "\"><img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/starh.gif") + "\"><img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/stare.gif") + "\"><img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/stare.gif") + "\" />";
            }
            else if (d >= 2.75M && d < 3.25M)
            {
                s = "<img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/starf.gif") + "\"><img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/starf.gif") + "\"><img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/starf.gif") + "\"><img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/stare.gif") + "\"><img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/stare.gif") + "\" />";
            }
            else if (d >= 3.25M && d < 3.75M)
            {
                s = "<img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/starf.gif") + "\"><img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/starf.gif") + "\"><img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/starf.gif") + "\"><img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/starh.gif") + "\"><img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/stare.gif") + "\" />";
            }
            else if (d >= 3.75M && d < 4.25M)
            {
                s = "<img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/starf.gif") + "\"><img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/starf.gif") + "\"><img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/starf.gif") + "\"><img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/starf.gif") + "\"><img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/stare.gif") + "\" />";
            }
            else if (d >= 4.25M && d < 4.75M)
            {
                s = "<img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/starf.gif") + "\"><img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/starf.gif") + "\"><img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/starf.gif") + "\"><img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/starf.gif") + "\"><img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/starh.gif") + "\" />";
            }
            else if (d >= 4.75M)
            {
                s = "<img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/starf.gif") + "\"><img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/starf.gif") + "\"><img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/starf.gif") + "\"><img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/starf.gif") + "\"><img align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/starf.gif") + "\" />";
            }
            return s;
        }

        static public String Left(String s, int l)
        {
            if (s.Length <= l)
            {
                return s;
            }
            return s.Substring(0, l - 1);
        }

        // this really is never meant to be called with ridiculously  small l values (e.g. l < 10'ish)
        static public String Ellipses(String s, int l, bool BreakBetweenWords)
        {
            if (l < 1)
            {
                return String.Empty;
            }
            if (l >= s.Length)
            {
                return s;
            }
            String tmpS = Left(s, l - 2);
            if (BreakBetweenWords)
            {
                try
                {
                    tmpS = tmpS.Substring(0, tmpS.LastIndexOf(" "));
                }
                catch { }
            }
            tmpS += "...";
            return tmpS;
        }

        public static String AspHTTP(String url, int TimeoutSecs)
        {
            String result;
            try
            {
                WebResponse objResponse;
                WebRequest objRequest = System.Net.HttpWebRequest.Create(url);
                if (TimeoutSecs > 0)
                {
                    objRequest.Timeout = TimeoutSecs * 1000; // ms
                }
                else
                {
                    objRequest.Timeout = System.Threading.Timeout.Infinite;
                }
                objResponse = objRequest.GetResponse();
                using (StreamReader sr = new StreamReader(objResponse.GetResponseStream()))
                {
                    result = sr.ReadToEnd();
                    // Close and clean up the StreamReader
                    sr.Close();
                }
                objResponse.Close();
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }

        public static String AspHTTP(String url, int TimeoutSecs, string postData)
        {
            String result;
            try
            {
                WebResponse objResponse;
                WebRequest objRequest = System.Net.HttpWebRequest.Create(url);
                objRequest.ContentType = "application/x-www-form-urlencoded";
                objRequest.Method = "POST";
                UTF8Encoding encoder = new UTF8Encoding();
                byte[] bytearray = encoder.GetBytes(postData);
                objRequest.ContentLength = bytearray.Length;
                Stream reqStream = objRequest.GetRequestStream();
                reqStream.Write(bytearray, 0, bytearray.Length);
                reqStream.Close();

                if (TimeoutSecs > 0)
                {
                    objRequest.Timeout = TimeoutSecs * 1000; // ms
                }
                else
                {
                    objRequest.Timeout = System.Threading.Timeout.Infinite;
                }
                objResponse = objRequest.GetResponse();
                using (StreamReader sr = new StreamReader(objResponse.GetResponseStream()))
                {
                    result = sr.ReadToEnd();
                    // Close and clean up the StreamReader
                    sr.Close();
                }
                objResponse.Close();
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }

        public static String SelectOption(String activeValue, String oname, String fieldname)
        {
            if (activeValue == oname)
            {
                return " selected";
            }
            else
            {
                return String.Empty;
            }
        }

        public static String SelectOption(IDataReader rs, String oname, String fieldname)
        {
            return SelectOption(DB.RSField(rs, fieldname), oname, fieldname);
        }

        public static String MakeFullName(String fn, String ln)
        {
            String tmp = fn + " " + ln;
            return tmp.Trim();
        }

        public static String ExtractBody(String ss)
        {
            try
            {
                int startAt;
                int stopAt;
                startAt = ss.IndexOf("<body");
                if (startAt == -1)
                {
                    startAt = ss.IndexOf("<BODY");
                }
                if (startAt == -1)
                {
                    startAt = ss.IndexOf("<Body");
                }
                startAt = ss.IndexOf(">", startAt);
                stopAt = ss.IndexOf("</body>");
                if (stopAt == -1)
                {
                    stopAt = ss.IndexOf("</BODY>");
                }
                if (stopAt == -1)
                {
                    stopAt = ss.IndexOf("</Body>");
                }
                if (startAt == -1)
                {
                    startAt = 1;
                }
                else
                {
                    startAt = startAt + 1;
                }
                if (stopAt == -1)
                {
                    stopAt = ss.Length;
                }
                return ss.Substring(startAt, stopAt - startAt);
            }
            catch
            {
                return String.Empty;
            }
        }

        public static void WriteFile(String fname, String contents, bool WriteFileInUTF8)
        {
            fname = SafeMapPath(fname);
            StreamWriter wr;
            if (WriteFileInUTF8)
            {
                wr = new StreamWriter(fname, false, System.Text.Encoding.UTF8, 4096);
            }
            else
            {
                wr = new StreamWriter(fname, false, System.Text.Encoding.ASCII, 4096);
            }
            wr.Write(contents);
            wr.Flush();
            wr.Close();
        }

        public static String ReadFile(String fname, bool ignoreErrors)
        {
            String contents;
            try
            {
                fname = SafeMapPath(fname);
                using (StreamReader rd = new StreamReader(fname))
                {
                    contents = rd.ReadToEnd();
                    rd.Close();
                    return contents;
                }
            }
            catch (Exception e)
            {
                if (ignoreErrors)
                    return String.Empty;
                else
                    throw e;
            }
        }

        public static String Capitalize(String s)
        {
            if (s.Length == 0)
            {
                return String.Empty;
            }
            else if (s.Length == 1)
            {
                return s.ToUpper(CultureInfo.InvariantCulture);
            }
            else
            {
                return s.Substring(0, 1).ToUpper(CultureInfo.InvariantCulture) + s.Substring(1, s.Length - 1).ToLowerInvariant();
            }
        }

        public static String ServerVariables(String paramName)
        {
            String tmpS = String.Empty;
            if (HttpContext.Current.Request.ServerVariables[paramName] != null)
            {
                try
                {
                    tmpS = HttpContext.Current.Request.ServerVariables[paramName].ToString();
                }
                catch
                {
                    tmpS = String.Empty;
                }
            }
            return tmpS;
        }

        /// <summary>
        /// Returns the customers' IPv4 or IPv6 Address. Works for proxies and clusters.
        /// </summary>
        public static string CustomerIpAddress()
        {
            // check request headers for client IP Address
            string ipAddr = ServerVariables("HTTP_X_FORWARDED_FOR").Trim(); // ssl cluster
            if (string.IsNullOrEmpty(ipAddr) || ipAddr.Equals("unknown", StringComparison.OrdinalIgnoreCase)) ipAddr = ServerVariables("HTTP_X_CLUSTER_CLIENT_IP").Trim(); // non-ssl cluster
            if (string.IsNullOrEmpty(ipAddr) || ipAddr.Equals("unknown", StringComparison.OrdinalIgnoreCase)) ipAddr = ServerVariables("HTTP_CLIENT_IP").Trim(); // proxy
            if (string.IsNullOrEmpty(ipAddr) || ipAddr.Equals("unknown", StringComparison.OrdinalIgnoreCase)) ipAddr = ServerVariables("REMOTE_ADDR").Trim(); // non-cluster

            // proxies can return a comma-separated list, so use the left-most (furthest downstream) one
            return ipAddr.Split(',')[0].Trim();
        }

        /// <summary>
        /// Determines whether request came from a secure (https) connection.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if request came from a secure (https) connection; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsSecureConnection()
        {
            // the HTTP_CLUSTER_HTTPS server variable is added by Mosso by their SSL Accelerator. A Mosso site sees all requests on port 80.
            return HttpContext.Current.Request.IsSecureConnection || ServerVariables("HTTP_CLUSTER_HTTPS").Trim().Equals("on", StringComparison.OrdinalIgnoreCase);
        }

        
        
        // can take virtual fname, or fully qualified path fname
        public static bool FileExists(String fname)
        {
            bool retVal = File.Exists(SafeMapPath(fname));
            if (!retVal && !fname.Contains(":") )
            {
                retVal = File.Exists(HttpContext.Current.Request.MapPath(fname));
            }
            return retVal;
        }

        // this is probably the implementation that Microsoft SHOULD have done!
        // use this helper function for ALL MapPath calls in the entire storefront for safety!
        public static String SafeMapPath(String fname)
        {
            if (string.IsNullOrEmpty(fname) || Path.IsPathRooted(fname))
            {
                return fname;
            }

            string result = fname;
         
            //Try it as a virtual path. Try to map it based on the Request.MapPath to handle Medium trust level and "~/" paths automatically 
            try
            {
                result = HttpContext.Current.Request.MapPath(fname);
            }
            catch
            {
                //Didn't like something about the virtual path.
                //May be a drive path. See if it will expand to a valid path
                try
                {
                    //Try a GetFullPath. If the path is not virtual or has other malformed problems
                    //Return it as is
                    result = Path.GetFullPath(fname);
                }
                catch (NotSupportedException) // Contains a colon, probably already a full path.
                {
                    return fname;
                }
                catch (SecurityException exc)//Path is somewhere you're not allowed to access or is otherwise damaged
                {
                    throw new SecurityException("If you are running in Medium Trust you may have virtual directories defined that are not accessible at this trust level,\n " + exc.Message);
                }
            }
            return result;
        }


        public static String ExtractToken(String ss, String t1, String t2)
        {
            if (ss.Length == 0)
            {
                return String.Empty;
            }
            int i1 = ss.IndexOf(t1);
            int i2 = ss.IndexOf(t2, CommonLogic.IIF(i1 == -1, 0, i1));
            if (i1 == -1 || i2 == -1 || i1 >= i2 || (i2 - i1) <= 0)
            {
                return String.Empty;
            }
            return ss.Substring(i1, i2 - i1).Replace(t1, "");
        }


        static public void SetField(DataSet ds, String fieldname)
        {
            ds.Tables["Customers"].Rows[0][fieldname] = CommonLogic.FormCanBeDangerousContent(fieldname);
        }

        static public String MakeSafeJavascriptName(String s)
        {
            String OKChars = "abcdefghijklmnopqrstuvwxyz1234567890_";
            s = s.ToLowerInvariant();
            StringBuilder tmpS = new StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                String tok = s.Substring(i, 1);
                if (OKChars.IndexOf(tok) != -1)
                {
                    tmpS.Append(tok);
                }
            }
            return tmpS.ToString();
        }

        static public String MakeSafeFilesystemName(String s)
        {
            String OKChars = "abcdefghijklmnopqrstuvwxyz1234567890_";
            s = s.ToLowerInvariant();
            StringBuilder tmpS = new StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                String tok = s.Substring(i, 1);
                if (OKChars.IndexOf(tok) != -1)
                {
                    tmpS.Append(tok);
                }
            }
            return tmpS.ToString();
        }

        static public String MakeSafeJavascriptString(String s)
        {
            return s.Replace("'", "\\'").Replace("\"", "\\\"");
        }

        public static void ReadWholeArray(Stream stream, byte[] data)
        {
            /// <summary>
            /// Reads data into a complete array, throwing an EndOfStreamException
            /// if the stream runs out of data first, or if an IOException
            /// naturally occurs.
            /// </summary>
            /// <param name="stream">The stream to read data from</param>
            /// <param name="data">The array to read bytes into. The array
            /// will be completely filled from the stream, so an appropriate
            /// size must be given.</param>
            int offset = 0;
            int remaining = data.Length;
            while (remaining > 0)
            {
                int read = stream.Read(data, offset, remaining);
                if (read <= 0)
                {
                    return;
                }
                remaining -= read;
            }
        }

        public static byte[] ReadFully(Stream stream)
        {
            /// <summary>
            /// Reads data from a stream until the end is reached. The
            /// data is returned as a byte array. An IOException is
            /// thrown if any of the underlying IO calls fail.
            /// </summary>
            byte[] buffer = new byte[32768];
            using (MemoryStream ms = new MemoryStream())
            {
                while (true)
                {
                    int read = stream.Read(buffer, 0, buffer.Length);
                    if (read <= 0)
                    {
                        return ms.ToArray();
                    }
                    ms.Write(buffer, 0, read);
                }
            }
        }

        static public Size GetImagePixelSize(String imgname)
        {
            try
            {
                //create instance of Bitmap class around specified image file
                // must use try/catch in case the image file is bogus
                using (Bitmap img = new Bitmap(SafeMapPath(imgname), false))
                {
                    return new Size(img.Width, img.Height);
                }
            }
            catch
            {
                try
                {
                    using (Bitmap img = new Bitmap(HttpContext.Current.Request.MapPath(imgname), false))
                    {
                        return new Size(img.Width, img.Height);
                    }
                }
                catch
                {
                    return new Size(0, 0);
                }
            }
        }

        static public String WrapString(String s, int ColWidth, String Separator)
        {
            StringBuilder tmpS = new StringBuilder(s.Length + 100);
            if (s.Length <= ColWidth || ColWidth == 0)
            {
                return s;
            }
            int start = 0;
            int length = Min(ColWidth, s.Length);
            while (start < s.Length)
            {
                if (tmpS.Length != 0)
                {
                    tmpS.Append(Separator);
                }
                tmpS.Append(s.Substring(start, length));
                start += ColWidth;
                length = Min(ColWidth, s.Length - start);
            }
            return tmpS.ToString();
        }

        public static String GetNewGUID()
        {
            return System.Guid.NewGuid().ToString().ToUpperInvariant();
        }

        static public String HtmlEncode(String S)
        {
            String result = String.Empty;
            for (int i = 0; i < S.Length; i++)
            {
                String c = S.Substring(i, 1);
                int acode = (int)c[0];
                if (acode < 32 || acode > 127)
                {
                    result += "&#" + acode.ToString() + ";";
                }
                else
                {
                    switch (acode)
                    {
                        case 32:
                            result += "&nbsp;";
                            break;
                        case 34:
                            result += "&quot;";
                            break;
                        case 38:
                            result += "&amp;";
                            break;
                        case 60:
                            result += "&lt;";
                            break;
                        case 62:
                            result += "&gt;";
                            break;
                        default:
                            result += c;
                            break;
                    }
                }
            }
            return result;
        }

        // this version is NOT to be used to squote db sql stuff!
        public static String SQuote(String s)
        {
            return "'" + s.Replace("'", "''") + "'";
        }


        // this version is NOT to be used to squote db sql stuff!
        public static String DQuote(String s)
        {
            return "\"" + s.Replace("\"", "\"\"") + "\"";
        }

        // ----------------------------------------------------------------
        //
        // PARAMS SUPPORT ROUTINES Uses Request.Params[]
        //
        // ----------------------------------------------------------------

        [Obsolete(" METHOD IS OBSOLETE, Use ParamsCanBeDangerousContent instead ")]
        public static String Params(String paramName)
        {
            return ParamsCanBeDangerousContent(paramName);
        }

        public static String ParamsCanBeDangerousContent(String paramName)
        {
            String tmpS = String.Empty;
            if (HttpContext.Current.Request.Params[paramName] != null)
            {
                try
                {
                    tmpS = HttpContext.Current.Request.Params[paramName];
                }
                catch
                {
                    tmpS = String.Empty;
                }
            }
            return tmpS;
        }

        public static bool ParamsBool(String paramName)
        {
            String tmpS = ParamsCanBeDangerousContent(paramName).ToUpperInvariant();
            return (tmpS == "TRUE" || tmpS == "YES" || tmpS == "1");
        }

        public static int ParamsUSInt(String paramName)
        {
            String tmpS = ParamsCanBeDangerousContent(paramName);
            return Localization.ParseUSInt(tmpS);
        }

        public static long ParamsUSLong(String paramName)
        {
            String tmpS = ParamsCanBeDangerousContent(paramName);
            return Localization.ParseUSLong(tmpS);
        }

        public static Single ParamsUSSingle(String paramName)
        {
            String tmpS = ParamsCanBeDangerousContent(paramName);
            return Localization.ParseUSSingle(tmpS);
        }

        public static Double ParamsUSDouble(String paramName)
        {
            String tmpS = ParamsCanBeDangerousContent(paramName);
            return Localization.ParseUSDouble(tmpS);
        }

        public static decimal ParamsUSDecimal(String paramName)
        {
            String tmpS = ParamsCanBeDangerousContent(paramName);
            return Localization.ParseUSDecimal(tmpS);
        }

        public static DateTime ParamsUSDateTime(String paramName)
        {
            String tmpS = ParamsCanBeDangerousContent(paramName);
            return Localization.ParseUSDateTime(tmpS);
        }

        public static int ParamsNativeInt(String paramName)
        {
            String tmpS = ParamsCanBeDangerousContent(paramName);
            return Localization.ParseNativeInt(tmpS);
        }

        public static long ParamsNativeLong(String paramName)
        {
            String tmpS = ParamsCanBeDangerousContent(paramName);
            return Localization.ParseNativeLong(tmpS);
        }

        public static Single ParamsNativeSingle(String paramName)
        {
            String tmpS = ParamsCanBeDangerousContent(paramName);
            return Localization.ParseNativeSingle(tmpS);
        }

        public static Double ParamsNativeDouble(String paramName)
        {
            String tmpS = ParamsCanBeDangerousContent(paramName);
            return Localization.ParseNativeDouble(tmpS);
        }

        public static decimal ParamsNativeDecimal(String paramName)
        {
            String tmpS = ParamsCanBeDangerousContent(paramName);
            return Localization.ParseNativeDecimal(tmpS);
        }

        public static DateTime ParamsNativeDateTime(String paramName)
        {
            String tmpS = ParamsCanBeDangerousContent(paramName);
            return Localization.ParseNativeDateTime(tmpS);
        }

        // ----------------------------------------------------------------
        //
        // FORM SUPPORT ROUTINES
        //
        // ----------------------------------------------------------------

        [Obsolete(" METHOD IS OBSOLETE, Use FormCanBeDangerousContent Instead ")]
        public static String Form(String paramName)
        {
            return FormCanBeDangerousContent(paramName);
        }

        public static String FormCanBeDangerousContent(String paramName)
        {
            String tmpS = String.Empty;
            if (HttpContext.Current.Request.Form[paramName] != null)
            {
                try
                {
                    tmpS = HttpContext.Current.Request.Form[paramName].ToString();
                }
                catch
                {
                    tmpS = String.Empty;
                }
            }
            return tmpS;
        }
        /// <summary>
        /// Sring to date validation
        /// </summary>
        /// <param name="sdate"></param>
        /// <returns>boolean</returns>
        public static bool IsValidDate(string sdate)
        {
            DateTime dt;
                 if (DateTime.TryParse(sdate,out dt))
                {
                    return true;
                }
                else
                {
                    return  false;
                }
        }

        public static bool FormBool(String paramName)
        {
            String tmpS = FormCanBeDangerousContent(paramName).ToUpperInvariant();
            return (tmpS == "TRUE" || tmpS == "YES" || tmpS == "1");
        }

        public static int FormUSInt(String paramName)
        {
            String tmpS = FormCanBeDangerousContent(paramName);
            return Localization.ParseUSInt(tmpS);
        }

        public static long FormUSLong(String paramName)
        {
            String tmpS = FormCanBeDangerousContent(paramName);
            return Localization.ParseUSLong(tmpS);
        }

        public static Single FormUSSingle(String paramName)
        {
            String tmpS = FormCanBeDangerousContent(paramName);
            return Localization.ParseUSSingle(tmpS);
        }

        public static Double FormUSDouble(String paramName)
        {
            String tmpS = FormCanBeDangerousContent(paramName);
            return Localization.ParseUSDouble(tmpS);
        }

        public static decimal FormUSDecimal(String paramName)
        {
            String tmpS = FormCanBeDangerousContent(paramName);
            return Localization.ParseUSDecimal(tmpS);
        }

        public static DateTime FormUSDateTime(String paramName)
        {
            String tmpS = FormCanBeDangerousContent(paramName);
            return Localization.ParseUSDateTime(tmpS);
        }

        public static int FormNativeInt(String paramName)
        {
            String tmpS = FormCanBeDangerousContent(paramName);
            return Localization.ParseNativeInt(tmpS);
        }

        public static long FormNativeLong(String paramName)
        {
            String tmpS = FormCanBeDangerousContent(paramName);
            return Localization.ParseNativeLong(tmpS);
        }

        public static Single FormNativeSingle(String paramName)
        {
            String tmpS = FormCanBeDangerousContent(paramName);
            return Localization.ParseNativeSingle(tmpS);
        }

        public static Double FormNativeDouble(String paramName)
        {
            String tmpS = FormCanBeDangerousContent(paramName);
            return Localization.ParseNativeDouble(tmpS);
        }

        public static decimal FormNativeDecimal(String paramName)
        {
            String tmpS = FormCanBeDangerousContent(paramName);
            return Localization.ParseNativeDecimal(tmpS);
        }

        public static DateTime FormNativeDateTime(String paramName)
        {
            String tmpS = FormCanBeDangerousContent(paramName);
            return Localization.ParseNativeDateTime(tmpS);
        }

        // ----------------------------------------------------------------
        //
        // QUERYSTRING SUPPORT ROUTINES
        //
        // ----------------------------------------------------------------

        [Obsolete(" METHOD IS OBSOLETE, Use QueryStringCanBeDangerousContent Instead ")]
        public static String QueryString(String paramName)
        {
            return QueryStringCanBeDangerousContent(paramName);
        }

        public static String QueryStringCanBeDangerousContent(String paramName)
        {
            String tmpS = String.Empty;
            
            RequestData reqData = HttpContext.Current.GetRequestData();
            if (reqData != null && 
                reqData.StringValue(paramName) != string.Empty)
            {
                try
                {
                    tmpS = reqData.StringValue(paramName);
                }
                catch
                {
                    tmpS = String.Empty;
                }
            }
            return tmpS;
        }

        public static bool QueryStringBool(String paramName)
        {
            String tmpS = QueryStringCanBeDangerousContent(paramName).ToUpperInvariant();
            return (tmpS == "TRUE" || tmpS == "YES" || tmpS == "1");
        }

        public static int QueryStringUSInt(String paramName)
        {
            String tmpS = QueryStringCanBeDangerousContent(paramName);
            return Localization.ParseUSInt(tmpS);
        }

        public static long QueryStringUSLong(String paramName)
        {
            String tmpS = QueryStringCanBeDangerousContent(paramName);
            return Localization.ParseUSLong(tmpS);
        }

        public static Single QueryStringUSSingle(String paramName)
        {
            String tmpS = QueryStringCanBeDangerousContent(paramName);
            return Localization.ParseUSSingle(tmpS);
        }

        public static Double QueryStringUSDouble(String paramName)
        {
            String tmpS = QueryStringCanBeDangerousContent(paramName);
            return Localization.ParseUSDouble(tmpS);
        }

        public static decimal QueryStringUSDecimal(String paramName)
        {
            String tmpS = QueryStringCanBeDangerousContent(paramName);
            return Localization.ParseUSDecimal(tmpS);
        }

        public static DateTime QueryStringUSDateTime(String paramName)
        {
            String tmpS = QueryStringCanBeDangerousContent(paramName);
            return Localization.ParseUSDateTime(tmpS);
        }

        public static int QueryStringNativeInt(String paramName)
        {
            String tmpS = QueryStringCanBeDangerousContent(paramName);
            return Localization.ParseNativeInt(tmpS);
        }

        public static long QueryStringNativeLong(String paramName)
        {
            String tmpS = QueryStringCanBeDangerousContent(paramName);
            return Localization.ParseNativeLong(tmpS);
        }

        public static Single QueryStringNativeSingle(String paramName)
        {
            String tmpS = QueryStringCanBeDangerousContent(paramName);
            return Localization.ParseNativeSingle(tmpS);
        }

        public static Double QueryStringNativeDouble(String paramName)
        {
            String tmpS = QueryStringCanBeDangerousContent(paramName);
            return Localization.ParseNativeDouble(tmpS);
        }

        public static decimal QueryStringNativeDecimal(String paramName)
        {
            String tmpS = QueryStringCanBeDangerousContent(paramName);
            return Localization.ParseNativeDecimal(tmpS);
        }

        public static DateTime QueryStringNativeDateTime(String paramName)
        {
            String tmpS = QueryStringCanBeDangerousContent(paramName);
            return Localization.ParseNativeDateTime(tmpS);
        }

        // ----------------------------------------------------------------
        //
        // SESSION SUPPORT ROUTINES --  These routines are all depricated in favor of using the CustomerSession object in the Customer object
        //
        // ----------------------------------------------------------------
       
        [Obsolete(" METHOD IS OBSOLETE, Use SessionNotServerFarmSafe Instead ")]
        public static String Session(String paramName)
        {
            return SessionNotServerFarmSafe(paramName);
        }

        public static String SessionNotServerFarmSafe(String paramName)
        {
            String tmpS = String.Empty;
            try
            {
                tmpS = HttpContext.Current.Session[paramName].ToString();
            }
            catch
            {
                tmpS = String.Empty;
            }
            return tmpS;
        }

        public static String Application(String paramName)
        {
            String tmpS = String.Empty;
            if (System.Web.Configuration.WebConfigurationManager.AppSettings[paramName] != null)
            {
                try
                {
                    tmpS = System.Web.Configuration.WebConfigurationManager.AppSettings[paramName];
                }
                catch
                {
                    tmpS = String.Empty;
                }
            }
            return tmpS;
        }

        public static bool ApplicationBool(String paramName)
        {
            String tmpS = Application(paramName).ToUpperInvariant();
            return (tmpS == "TRUE" || tmpS == "YES" || tmpS == "1");
        }

        public static int ApplicationUSInt(String paramName)
        {
            String tmpS = Application(paramName);
            return Localization.ParseUSInt(tmpS);
        }

        public static long ApplicationUSLong(String paramName)
        {
            String tmpS = Application(paramName);
            return Localization.ParseUSLong(tmpS);
        }

        public static Single ApplicationUSSingle(String paramName)
        {
            String tmpS = Application(paramName);
            return Localization.ParseUSSingle(tmpS);
        }

        public static Double ApplicationUSDouble(String paramName)
        {
            String tmpS = Application(paramName);
            return Localization.ParseUSDouble(tmpS);
        }

        public static Decimal ApplicationUSDecimal(String paramName)
        {
            String tmpS = Application(paramName);
            return Localization.ParseUSDecimal(tmpS);
        }

        public static DateTime ApplicationUSDateTime(String paramName)
        {
            String tmpS = Application(paramName);
            return Localization.ParseUSDateTime(tmpS);
        }

        public static int ApplicationNativeInt(String paramName)
        {
            String tmpS = Application(paramName);
            return Localization.ParseNativeInt(tmpS);
        }

        public static long ApplicationNativeLong(String paramName)
        {
            String tmpS = Application(paramName);
            return Localization.ParseNativeLong(tmpS);
        }

        public static Single ApplicationNativeSingle(String paramName)
        {
            String tmpS = Application(paramName);
            return Localization.ParseNativeSingle(tmpS);
        }

        public static Double ApplicationNativeDouble(String paramName)
        {
            String tmpS = Application(paramName);
            return Localization.ParseNativeDouble(tmpS);
        }

        public static Decimal ApplicationNativeDecimal(String paramName)
        {
            String tmpS = Application(paramName);
            return Localization.ParseNativeDecimal(tmpS);
        }

        public static DateTime ApplicationNativeDateTime(String paramName)
        {
            String tmpS = Application(paramName);
            return Localization.ParseNativeDateTime(tmpS);
        }

        // ----------------------------------------------------------------
        //
        // COOKIE SUPPORT ROUTINES
        //
        // ----------------------------------------------------------------

        [Obsolete(" METHOD IS OBSOLETE, Use CookieCanBeDangerousContent Instead ")]
        public static String Cookie(String paramName, bool decode)
        {
            return CookieCanBeDangerousContent(paramName, decode);
        }

        public static String CookieCanBeDangerousContent(String paramName, bool decode)
        {
            if (HttpContext.Current.Request.Cookies[paramName] == null)
            {
                return String.Empty;
            }
            try
            {
                String tmp = HttpContext.Current.Request.Cookies[paramName].Value.ToString();
                if (decode)
                {
                    tmp = HttpContext.Current.Server.UrlDecode(tmp);
                }
                return tmp;
            }
            catch
            {
                return String.Empty;
            }
        }

        public static bool CookieBool(String paramName)
        {
            String tmpS = CookieCanBeDangerousContent(paramName, true).ToUpperInvariant();
            return (tmpS == "TRUE" || tmpS == "YES" || tmpS == "1");
        }

        public static int CookieUSInt(String paramName)
        {
            String tmpS = CookieCanBeDangerousContent(paramName, true);
            return Localization.ParseUSInt(tmpS);
        }

        public static long CookieUSLong(String paramName)
        {
            String tmpS = CookieCanBeDangerousContent(paramName, true);
            return Localization.ParseUSLong(tmpS);
        }

        public static Single CookieUSSingle(String paramName)
        {
            String tmpS = CookieCanBeDangerousContent(paramName, true);
            return Localization.ParseUSSingle(tmpS);
        }

        public static Double CookieUSDouble(String paramName)
        {
            String tmpS = CookieCanBeDangerousContent(paramName, true);
            return Localization.ParseUSDouble(tmpS);
        }

        public static Decimal CookieUSDecimal(String paramName)
        {
            String tmpS = CookieCanBeDangerousContent(paramName, true);
            return Localization.ParseUSDecimal(tmpS);
        }

        public static DateTime CookieUSDateTime(String paramName)
        {
            String tmpS = CookieCanBeDangerousContent(paramName, true);
            return Localization.ParseUSDateTime(tmpS);
        }

        public static int CookieNativeInt(String paramName)
        {
            String tmpS = CookieCanBeDangerousContent(paramName, true);
            return Localization.ParseNativeInt(tmpS);
        }

        public static long CookieNativeLong(String paramName)
        {
            String tmpS = CookieCanBeDangerousContent(paramName, true);
            return Localization.ParseNativeLong(tmpS);
        }

        public static Single CookieNativeSingle(String paramName)
        {
            String tmpS = CookieCanBeDangerousContent(paramName, true);
            return Localization.ParseNativeSingle(tmpS);
        }

        public static Double CookieNativeDouble(String paramName)
        {
            String tmpS = CookieCanBeDangerousContent(paramName, true);
            return Localization.ParseNativeDouble(tmpS);
        }

        public static Decimal CookieNativeDecimal(String paramName)
        {
            String tmpS = CookieCanBeDangerousContent(paramName, true);
            return Localization.ParseNativeDecimal(tmpS);
        }

        public static DateTime CookieNativeDateTime(String paramName)
        {
            String tmpS = CookieCanBeDangerousContent(paramName, true);
            return Localization.ParseNativeDateTime(tmpS);
        }


        // ----------------------------------------------------------------
        //
        // Hashtable PARAM SUPPORT ROUTINES
        // assumes has table has string keys, and string values.
        //
        // ----------------------------------------------------------------
        public static String HashtableParam(Hashtable t, String paramName)
        {
            String tmpS = String.Empty;
            if (t[paramName] != null)
            {
                try
                {
                    tmpS = t[paramName].ToString();
                }
                catch
                {
                    tmpS = String.Empty;
                }
            }
            return tmpS;
        }

        public static bool HashtableParamBool(Hashtable t, String paramName)
        {
            String tmpS = CommonLogic.HashtableParam(t, paramName).ToUpperInvariant();
            return (tmpS == "TRUE" || tmpS == "YES" || tmpS == "1");
        }

        public static int HashtableParamUSInt(Hashtable t, String paramName)
        {
            String tmpS = HashtableParam(t, paramName);
            return Localization.ParseUSInt(tmpS);
        }

        public static long HashtableParamUSLong(Hashtable t, String paramName)
        {
            String tmpS = HashtableParam(t, paramName);
            return Localization.ParseUSLong(tmpS);
        }

        public static Single HashtableParamUSSingle(Hashtable t, String paramName)
        {
            String tmpS = HashtableParam(t, paramName);
            return Localization.ParseUSSingle(tmpS);
        }

        public static Double HashtableParamUSDouble(Hashtable t, String paramName)
        {
            String tmpS = HashtableParam(t, paramName);
            return Localization.ParseUSDouble(tmpS);
        }

        public static decimal HashtableParamUSDecimal(Hashtable t, String paramName)
        {
            String tmpS = HashtableParam(t, paramName);
            return Localization.ParseUSDecimal(tmpS);
        }

        public static DateTime HashtableParamUSDateTime(Hashtable t, String paramName)
        {
            String tmpS = HashtableParam(t, paramName);
            return Localization.ParseUSDateTime(tmpS);
        }

        public static int HashtableParamNativeInt(Hashtable t, String paramName)
        {
            String tmpS = HashtableParam(t, paramName);
            return Localization.ParseNativeInt(tmpS);
        }

        public static long HashtableParamNativeLong(Hashtable t, String paramName)
        {
            String tmpS = HashtableParam(t, paramName);
            return Localization.ParseNativeLong(tmpS);
        }

        public static Single HashtableParamNativeSingle(Hashtable t, String paramName)
        {
            String tmpS = HashtableParam(t, paramName);
            return Localization.ParseNativeSingle(tmpS);
        }

        public static Double HashtableParamNativeDouble(Hashtable t, String paramName)
        {
            String tmpS = HashtableParam(t, paramName);
            return Localization.ParseNativeDouble(tmpS);
        }

        public static decimal HashtableParamNativeDecimal(Hashtable t, String paramName)
        {
            String tmpS = HashtableParam(t, paramName);
            return Localization.ParseNativeDecimal(tmpS);
        }

        public static DateTime HashtableParamNativeDateTime(Hashtable t, String paramName)
        {
            String tmpS = HashtableParam(t, paramName);
            return Localization.ParseNativeDateTime(tmpS);
        }

        public static string UnzipBase64DataToString(String s)
        {
            string result = string.Empty;

            // So far this is only used for the Cybersource gateway
            // Cybersource requires unzipping base 64 encoded data to save as transaction proof for Streamline 3D Secure transactions.

            try
            {
                byte[] rawBytes = Convert.FromBase64String(s);

                // ZLIB format, first 2 bytes is header, last 4 bytes is checksum, throw those out
                byte[] decodedBytes = new byte[(rawBytes.Length - 6)];
                System.Buffer.BlockCopy(rawBytes, 2, decodedBytes, 0, decodedBytes.Length);

                MemoryStream ms = new MemoryStream(decodedBytes, false);
                ms.Position = 0;
                Stream zipStream = new DeflateStream(ms, CompressionMode.Decompress, false);
                StreamReader SR = new StreamReader(zipStream);
                result = SR.ReadToEnd();
            }
            catch { }

            return result;
        }


        /// <summary>
        /// Utility function to check if a string value is included within the commadelimited string to match with
        /// </summary>
        /// <param name="stringValue">the string to look for</param>
        /// <param name="commaDelimitedList">the comma delimited string to search at</param>
        /// <returns></returns>
        public static bool StringInCommaDelimitedStringList(string stringValue, string commaDelimitedList)
        {
            try
            {
                if (CommonLogic.IsStringNullOrEmpty(stringValue) ||
                    CommonLogic.IsStringNullOrEmpty(commaDelimitedList))
                {
                    return false;
                }

                string[] individualValues = commaDelimitedList.Split(',');

                foreach (string valueToMatch in individualValues)
                {
                    if (valueToMatch.ToUpperInvariant().Trim().Equals(stringValue.ToUpperInvariant().Trim()))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        /// <summary>
        /// Builds a List of ints from a comma separated string value
        /// </summary>
        /// <param name="s">The comma separated string to be converted</param>
        /// <returns>List of int values</returns>
        public static List<int> BuildListFromCommaString(String s)
        {
            List<int> newList = new List<int>();

            if (!String.IsNullOrEmpty(s))
            {
                foreach (String listItem in s.Split(','))
                {
                    try
                    {
                        newList.Add(int.Parse(listItem.Trim()));
                    }
                    catch { }
                }
            }

            return newList;
        }

        /// <summary>
        /// Builds a comma separated string from a list of ints
        /// </summary>
        /// <param name="s">The int List to be converted to a comma separated string</param>
        /// <returns>Comma separated string of int values</returns>
        public static string BuildCommaStringFromList(List<int> s)
        {
            StringBuilder tmpS = new StringBuilder();

            if (s.Count > 0)
            {
                foreach (int i in s)
                {
                    tmpS.Append(i.ToString() + ",");
                }
            }

            return tmpS.ToString().TrimEnd(',');
        }
    }

}
