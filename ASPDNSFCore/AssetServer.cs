// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using AspDotNetStorefrontCore;

namespace AssetServer
{
    public class AssetServerAsset
    {

        private static String AssetServerURL //hardcoded for testing. we will need to pull the license guid dynamically.
        {
            get
            {
                String url = "http://www.aspdotnetstorefront.com/License/Feed.aspx"; //default can be overridden with an app config
                if (!String.IsNullOrEmpty(AppLogic.AppConfig("AssetServerURL")))
                    url = AppLogic.AppConfig("AssetServerURL");
                return url + "?LicenseGuid=" + AspDotNetStorefront.Global.LicenseInfo("id") + "&version=" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public String Title { get; private set; }
        public String Description { get; private set; }
        public String Link { get; private set; }
        public int VersionId { get; private set; }
        public Guid ResourceGuid { get; private set; }
        public DateTime PublishDate { get; private set; }
        public String Version { get; private set; }
        public Boolean IsVB { get; private set; }
        public Boolean IsSLA { get; private set; }
        public Boolean IsPatch { get; private set; }
        public String DownloadInstructions { get; private set; }

        public AssetServerAsset(XmlDocument xDoc)
        {
            Title = xDoc.SelectSingleNode("//title").InnerText;
            Description = xDoc.SelectSingleNode("//description").InnerText;
            Link = xDoc.SelectSingleNode("//link").InnerText;
            try { VersionId = int.Parse(xDoc.SelectSingleNode("//versionid").InnerText); }
            catch (Exception) { }
            ResourceGuid = Guid.Empty;
            try { ResourceGuid = new Guid(xDoc.SelectSingleNode("//resourceguid").InnerText); }
            catch (Exception) { }

            DateTime parseDate;
            if (!DateTime.TryParse(xDoc.SelectSingleNode("//pubDate").InnerText, out parseDate))
                PublishDate = DateTime.MinValue;
            else
                PublishDate = parseDate;

            Version = xDoc.SelectSingleNode("//version").InnerText;
            IsVB = xDoc.SelectSingleNode("//isvb").InnerText.ToBool();
            IsSLA = xDoc.SelectSingleNode("//issla").InnerText.ToBool();
            IsPatch = xDoc.SelectSingleNode("//ispatch").InnerText.ToBool();

            XmlNode xnDownloadInstructions = xDoc.SelectSingleNode("//downloadInstructions"); 
            if (xnDownloadInstructions != null && !String.IsNullOrEmpty(xnDownloadInstructions.InnerText))
            {
                DownloadInstructions = xnDownloadInstructions.InnerText;
            }
        }

        public static Dictionary<String, List<AssetServerAsset>> GetAssetServerAssets()
        {
            try
            {
                Dictionary<String, List<AssetServerAsset>> ret = new Dictionary<String, List<AssetServerAsset>>();
                String rssXML = CommonLogic.AspHTTP(AssetServerURL, 5);
                if (rssXML.Contains("<html"))
                    throw new InvalidOperationException("The license server feed was invalid.");
                XmlDocument rssDoc = new XmlDocument();
                rssDoc.LoadXml(rssXML);
                XmlNodeList items = rssDoc.SelectNodes("//item");
                XmlDocument item = new XmlDocument();
                for (int i = 0; i < items.Count; i++)
                {
                    item.LoadXml(items[i].OuterXml);
                    AssetServerAsset newItem = new AssetServerAsset(item);
                    if (!ret.ContainsKey(newItem.Description.Replace(" ", "")))
                        ret.Add(newItem.Description.Replace(" ", ""), new List<AssetServerAsset>());
                    ret[newItem.Description.Replace(" ", "")].Add(newItem);
                }
                return ret;
            }
            catch
            {
                return new Dictionary<String, List<AssetServerAsset>>();
            }
        }
    }

    public class AssetVersion : IComparable
    {
        public int[] VersionArray { get; private set; }
        public string VersionString
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < VersionArray.Length; i++)
                {
                    if (i != 0)
                        sb.Append(".");
                    sb.Append(VersionArray[i].ToString());
                }
                return sb.ToString();
            }
        }

        public AssetVersion(string sVersion)
        {
            String[] versions = sVersion.Trim().Split('.');
            VersionArray = new int[versions.Length];
            for (int i = 0; i < versions.Length; i++)
                VersionArray[i] = versions[i].ToNativeInt();
        }

        #region IComparable Members

        public int CompareTo(object obj)
        {
            int[] compared = ((AssetVersion)obj).VersionArray;
            int[] thisv = this.VersionArray;

            int length = thisv.Length;
            if (compared.Length > thisv.Length)
                length = compared.Length;

            compared = ZeroFillIntArray(compared, length);
            thisv = ZeroFillIntArray(thisv, length);

            for (int i = 0; i < length; i++)
            {
                if (thisv[i] > compared[i])
                    return 1;
                else if (compared[i] > thisv[i])
                    return -1;
            }

            return 0;
        }

        private int[] ZeroFillIntArray(int[] A, int Length)
        {
            if (A.Length == Length)
                return A;

            int[] ret = new int[Length];
            for (int i = 0; i < A.Length; i++)
                ret[i] = A[i];
            return ret;
        }

        #endregion
    }
}

