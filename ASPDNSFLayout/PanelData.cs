// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using AspDotNetStorefrontControls;

namespace AspDotNetStorefrontLayout
{
    
    public class ImagePanelData
    {
        public string ImageSource
        { get; set; }
    }

    public class TextPanelData
    {
        private TextPanelData(string inVal)
        {
            value = inVal;
        }
        private string value
        { get; set; }
        public static implicit operator TextPanelData(string arg)
        {
            return new TextPanelData(arg);
        }
        public static implicit operator string(TextPanelData arg)
        {
            return arg.ToString();
        }
        public override string ToString()
        {
            return value;
        }
    }

    internal enum DataType
    {
        ImagePanelData,
        TextPanelData
    }

    public class LayoutDataSource
    {
        public LayoutDataSource()
        {
            _ImageData = new Dictionary<string, ImagePanelData>();
            _TextData = new Dictionary<string, TextPanelData>();
            ImagePanelData ipd1 = new ImagePanelData();
            ipd1.ImageSource = "http://imgs.xkcd.com/comics/bored_with_the_internet.jpg";
            _ImageData.Add("ImageData1", ipd1);
            _TextData.Add("textData1", "I'm teaching every 8-year-old relative to say this, and every 14-year-old to do the same thing with Toy Story.  Also, Pokemon hit the US over a decade ago and kids born after Aladdin came out will turn 18 next year.");
        }
        #region Data Accessors
        internal ImagePanelData ImageData(int Index)
        {
            return _ImageData[_ImageData.Keys.ToArray<string>()[Index]];
        }
        internal ImagePanelData ImageData(string key)
        {
            return _ImageData[key];
        }
        internal TextPanelData TextData(int Index)
        {
            return _TextData[_TextData.Keys.ToArray<string>()[Index]];
        }
        internal TextPanelData TextData(string key)
        {
            return _TextData[key];
        }

        internal bool HasData(DataType data, string key)
        {
            switch (data)
            {
                case DataType.ImagePanelData:
                    return _ImageData.ContainsKey(key);
                case DataType.TextPanelData:
                    return _TextData.ContainsKey(key);
                default:
                    return false;
            }
        }
        internal bool HasData(DataType data, int index)
        {
            switch (data)
            {
                case DataType.ImagePanelData:
                    return _ImageData.ContainsKey(_ImageData.Keys.ToArray<string>()[index]);
                case DataType.TextPanelData:
                    return _TextData.ContainsKey((_TextData.Keys.ToArray<string>()[index]));
                default:
                    return false;
            }
        }

        #endregion

        private Dictionary<string, ImagePanelData> _ImageData
        { get; set; }
        private Dictionary<string, TextPanelData> _TextData
        { get; set; }
    }


}
