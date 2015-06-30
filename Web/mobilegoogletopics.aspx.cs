// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Web;
using System.Web.SessionState;
using System.IO;
using System.Globalization;
using AspDotNetStorefrontCore;
using System.Data.SqlClient;
using Vortx.MobileFramework;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for googletopics.
    /// </summary>
    public partial class mobilegoogletopics : System.Web.UI.Page
    {

        protected void Page_Load(object sender, System.EventArgs e)
        {
            Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;
            MobileHelper.RedirectPageWhenMobileIsDisabled("~/googletopics.aspx", ThisCustomer);

            Response.ContentType = "text/xml";
            Response.ContentEncoding = new System.Text.UTF8Encoding();
            Response.Write("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");

            int SkinID = 1; // not sure what to do about this...google can't invoke different skins easily
            String StoreLoc = AppLogic.GetStoreHTTPLocation(false);

            Response.Write("<urlset xmlns='http://www.sitemaps.org/schemas/sitemap/0.9' xmlns:mobile='http://www.google.com/schemas/sitemap-mobile/1.0'>\n");

            if (AppLogic.AppConfigBool("SiteMap.ShowTopics"))
            {
                // DB Topics:
                using (SqlConnection conn = DB.dbConn())
                {
                    conn.Open();
                    using(IDataReader rs = DB.GetRS(string.Format("select Name from Topic with (NOLOCK) where {0} Deleted=0 and (SkinID IS NULL or SkinID=0 or SkinID={1}) Order By DisplayOrder, Name ASC", CommonLogic.IIF(AppLogic.IsAdminSite, "", "ShowInSiteMap=1 and "), SkinID.ToString()), conn))
                    {
                        while(rs.Read())
                        {
                            Response.Write("<url>");
                            Response.Write("<loc>" + XmlCommon.XmlEncode(StoreLoc + SE.MakeDriverLink(DB.RSFieldByLocale(rs, "Name", Localization.GetDefaultLocale()))) + "</loc> ");
                            Response.Write("<changefreq>" + AppLogic.AppConfig("GoogleSiteMap.TopicChangeFreq") + "</changefreq> ");
                            Response.Write("<priority>" + AppLogic.AppConfig("GoogleSiteMap.TopicPriority") + "</priority> ");
                            Response.Write("<mobile:mobile/></url>\n");
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
