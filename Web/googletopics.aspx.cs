// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Web;
using System.Web.SessionState;
using System.IO;
using System.Globalization;
using AspDotNetStorefrontCore;
using System.Data.SqlClient;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for googletopics.
    /// </summary>
    public partial class googletopics : System.Web.UI.Page
    {

        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.ContentType = "text/xml";
            Response.ContentEncoding = new System.Text.UTF8Encoding();
            Response.Write("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");

            int SkinID = 1; // not sure what to do about this...google can't invoke different skins easily
            String StoreLoc = AppLogic.GetStoreHTTPLocation(false, false);
            bool filter = AppLogic.GlobalConfigBool("AllowTopicFiltering");
            List<String> dupes = new List<string>();


            Response.Write("<urlset xmlns=\"" + AppLogic.AppConfig("GoogleSiteMap.Xmlns") + "\">");

            if (AppLogic.AppConfigBool("SiteMap.ShowTopics"))
            {
                // DB Topics:
                using (SqlConnection conn = DB.dbConn())
                {
                    conn.Open();
                    if (filter)
                    {
                        using (IDataReader rs = DB.GetRS(string.Format("select Name from Topic with (NOLOCK) where {0} Deleted=0 and (SkinID IS NULL or SkinID=0 or SkinID={1}) and (StoreID=0 OR StoreID=" + AppLogic.StoreID() + ") Order By StoreID DESC, DisplayOrder, Name ASC", CommonLogic.IIF(AppLogic.IsAdminSite, "", "ShowInSiteMap=1 and "), SkinID.ToString()), conn))
                        {
                            while (rs.Read())
                            {
                                //Only display the first instance of the topic name, store-specific version first
                                if (!dupes.Contains(DB.RSFieldByLocale(rs, "Name", Localization.GetDefaultLocale())))
                                {
                                    Response.Write("<url>");
                                    Response.Write("<loc>" + XmlCommon.XmlEncode(StoreLoc + SE.MakeDriverLink(DB.RSFieldByLocale(rs, "Name", Localization.GetDefaultLocale()))) + "</loc> ");
                                    Response.Write("<changefreq>" + AppLogic.AppConfig("GoogleSiteMap.TopicChangeFreq") + "</changefreq> ");
                                    Response.Write("<priority>" + AppLogic.AppConfig("GoogleSiteMap.TopicPriority") + "</priority> ");
                                    Response.Write("</url>");
                                    dupes.Add(DB.RSFieldByLocale(rs, "Name", Localization.GetDefaultLocale()));
                                }
                            }
                        }
                    }
                    else
                    {
                        using (IDataReader rs = DB.GetRS(string.Format("select Name from Topic with (NOLOCK) where {0} Deleted=0 and (SkinID IS NULL or SkinID=0 or SkinID={1}) and (StoreID=0) Order By DisplayOrder, Name ASC", CommonLogic.IIF(AppLogic.IsAdminSite, "", "ShowInSiteMap=1 and "), SkinID.ToString()), conn))
                        {
                            while (rs.Read())
                            {
                                Response.Write("<url>");
                                Response.Write("<loc>" + XmlCommon.XmlEncode(StoreLoc + SE.MakeDriverLink(DB.RSFieldByLocale(rs, "Name", Localization.GetDefaultLocale()))) + "</loc> ");
                                Response.Write("<changefreq>" + AppLogic.AppConfig("GoogleSiteMap.TopicChangeFreq") + "</changefreq> ");
                                Response.Write("<priority>" + AppLogic.AppConfig("GoogleSiteMap.TopicPriority") + "</priority> ");
                                Response.Write("</url>");
                                dupes.Add(DB.RSFieldByLocale(rs, "Name", Localization.GetDefaultLocale()));
                            }
                        }
                    }
                }                

                // File Topics:
                // create an array to hold the list of files
                ArrayList fArray = new ArrayList();

                // get information about our initial directory
                String SFP = CommonLogic.SafeMapPath(CommonLogic.IIF(AppLogic.IsAdminSite, "../", "") + "~/App_Templates/Skin_" + SkinID.ToString() + "/template.htm").Replace("template.htm", "");

                DirectoryInfo dirInfo = new DirectoryInfo(SFP);

                // retrieve array of files & subdirectories
                FileSystemInfo[] myDir = dirInfo.GetFileSystemInfos();

                for (int i = 0; i < myDir.Length; i++)
                {
                    // check the file attributes

                    // if a subdirectory, add it to the sArray    
                    // otherwise, add it to the fArray
                    if (((Convert.ToUInt32(myDir[i].Attributes) & Convert.ToUInt32(FileAttributes.Directory)) > 0))
                    {
                    }
                    else
                    {
                        bool skipit = false;
                        if (!myDir[i].FullName.EndsWith("htm", StringComparison.InvariantCultureIgnoreCase) || (myDir[i].FullName.IndexOf("TEMPLATE", StringComparison.InvariantCultureIgnoreCase) != -1) || (myDir[i].FullName.IndexOf("AFFILIATE_", StringComparison.InvariantCultureIgnoreCase) != -1) || (myDir[i].FullName.IndexOf(AppLogic.ro_PMMicropay,  StringComparison.InvariantCultureIgnoreCase) != -1))
                        {
                            skipit = true;
                        }
                        if (!skipit)
                        {
                            fArray.Add(Path.GetFileName(myDir[i].FullName));
                        }
                    }
                }

                if (fArray.Count != 0)
                {
                    // sort the files alphabetically
                    fArray.Sort(0, fArray.Count, null);
                    for (int i = 0; i < fArray.Count; i++)
                    {
                        Response.Write("<url>");
                        Response.Write("<loc>" + StoreLoc + SE.MakeDriverLink(fArray[i].ToString().Replace(".htm", "")) + "</loc> ");
                        Response.Write("<changefreq>" + AppLogic.AppConfig("GoogleSiteMap.TopicChangeFreq") + "</changefreq> ");
                        Response.Write("<priority>" + AppLogic.AppConfig("GoogleSiteMap.TopicPriority") + "</priority> ");
                        Response.Write("</url>");
                    }
                }
            }

            Response.Write("</urlset>");
        }

    }
}
