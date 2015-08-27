// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Configuration;
using System.Web.SessionState;
using System.Web.Caching;
using System.Web.Util;
using System.Data;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Data.SqlClient;

namespace AspDotNetStorefrontCore
{
    /// <summary>
    /// Summary description for SiteMap1.
    /// </summary>
    public class SiteMap1
    {
        private String m_Contents;

        public String Contents
        {
            get
            {
                return m_Contents;
            }
        }

        public SiteMap1(System.Collections.Generic.Dictionary<string, EntityHelper> EntityHelpers, int SkinID, Customer ThisCustomer)
        {
            bool FromCache = false;
            String CacheName = String.Format("SiteMap1_{0}_{1}", SkinID.ToString(), ThisCustomer.LocaleSetting);
            if (AppLogic.CachingOn)
            {
                m_Contents = (String)HttpContext.Current.Cache.Get(CacheName);
                if (m_Contents != null)
                {
                    FromCache = true;
                }
            }

            if (!FromCache)
            {                
                StringBuilder tmpS = new StringBuilder(50000);

                if (AppLogic.IsAdminSite || AppLogic.AppConfigBool("SiteMap.ShowCategories"))
                {
                    // Categories:
                    String s = AppLogic.LookupHelper("Category", 0).GetEntityULList(0, ThisCustomer.LocaleSetting, ThisCustomer.AffiliateID, ThisCustomer.CustomerLevelID, true, AppLogic.AppConfigBool("SiteMap.ShowProducts") && AppLogic.NumProductsInDB < 250, true, "sitemapul", true, 0, String.Empty);
                    if (s.Length != 0)
                    {
                        tmpS.Append("<b>");
                        if (AppLogic.IsAdminSite)
                        {
                            tmpS.Append("<a href=\"newentities.aspx?entityname=category\">");
                        }
                        tmpS.Append(AppLogic.GetString("AppConfig.CategoryPromptPlural", SkinID, ThisCustomer.LocaleSetting).ToUpperInvariant());
                        if (AppLogic.IsAdminSite)
                        {
                            tmpS.Append("</a>");
                        }
                        tmpS.Append("</b>");
                        tmpS.Append(s);
                    }
                }

                if (AppLogic.IsAdminSite || AppLogic.AppConfigBool("SiteMap.ShowSections"))
                {
                    // Sections:
                    String s = AppLogic.LookupHelper("Section", 0).GetEntityULList(0, ThisCustomer.LocaleSetting, ThisCustomer.AffiliateID, ThisCustomer.CustomerLevelID, true, AppLogic.AppConfigBool("SiteMap.ShowProducts") && AppLogic.NumProductsInDB < 250, true, "sitemapul", true, 0, String.Empty);
                    if (s.Length != 0)
                    {
                        tmpS.Append("<b>");
                        if (AppLogic.IsAdminSite)
                        {
                            tmpS.Append("<a href=\"newentities.aspx?entityname=section\">");
                        }
                        tmpS.Append(AppLogic.GetString("AppConfig.SectionPromptPlural", SkinID, ThisCustomer.LocaleSetting).ToUpperInvariant());
                        if (AppLogic.IsAdminSite)
                        {
                            tmpS.Append("</a>");
                        }
                        tmpS.Append("</b>");
                        tmpS.Append(s);
                    }
                }

                if (AppLogic.IsAdminSite || AppLogic.AppConfigBool("SiteMap.ShowLibraries"))
                {
                    // Libraries:
                    String s = AppLogic.LookupHelper("Library", 0).GetEntityULList(0, ThisCustomer.LocaleSetting, ThisCustomer.AffiliateID, ThisCustomer.CustomerLevelID, AppLogic.AppConfigBool("SiteMap.ShowDocuments"), true, true, "sitemapul", true, 0, String.Empty);
                    if (s.Length != 0)
                    {
                        tmpS.Append("<b>");
                        if (AppLogic.IsAdminSite)
                        {
                            tmpS.Append("<a href=\"newentities.aspx?entityname=library\">");
                        }
                        tmpS.Append(AppLogic.GetString("AppConfig.LibraryPromptPlural", SkinID, ThisCustomer.LocaleSetting).ToUpperInvariant());
                        if (AppLogic.IsAdminSite)
                        {
                            tmpS.Append("</a>");
                        }
                        tmpS.Append("</b>");
                        tmpS.Append(s);
                    }
                }

                if (AppLogic.IsAdminSite || AppLogic.AppConfigBool("SiteMap.ShowManufacturers"))
                {
                    // Manufacturers:
                    String s = AppLogic.LookupHelper("Manufacturer", 0).GetEntityULList(0, ThisCustomer.LocaleSetting, ThisCustomer.AffiliateID, ThisCustomer.CustomerLevelID, false, AppLogic.AppConfigBool("SiteMap.ShowProducts") && AppLogic.NumProductsInDB < 250, true, "sitemapul", true, 0, String.Empty);
                    if (s.Length != 0)
                    {
                        tmpS.Append("<b>");
                        if (AppLogic.IsAdminSite)
                        {
                            tmpS.Append("<a href=\"newentities.aspx?entityname=manufacturer\">");
                        }
                        tmpS.Append(AppLogic.GetString("AppConfig.ManufacturerPromptPlural", SkinID, ThisCustomer.LocaleSetting).ToUpperInvariant());
                        if (AppLogic.IsAdminSite)
                        {
                            tmpS.Append("</a>");
                        }
                        tmpS.Append("</b>");
                        tmpS.Append(s);
                    }
                }

                if (AppLogic.IsAdminSite || AppLogic.AppConfigBool("SiteMap.ShowTopics"))
                {
                    // Topics:
                    tmpS.Append("<b>");
                    if (AppLogic.IsAdminSite)
                    {
                        tmpS.Append("<a href=\"topics.aspx\">");
                    }
                    tmpS.Append(AppLogic.GetString("sitemap.aspx.2", SkinID, ThisCustomer.LocaleSetting).ToUpperInvariant());
                    if (AppLogic.IsAdminSite)
                    {
                        tmpS.Append("</a>");
                    }
                    tmpS.Append("</b>");
                    tmpS.Append("<ul class=\"sitemapul\">\n");
                    
                    using (SqlConnection dbconn = DB.dbConn())
                    {
                        dbconn.Open();
                        using (IDataReader rs = DB.GetRS(string.Format("select count(*) as N from Topic with (NOLOCK) where {0} Deleted=0 AND Published=1 and (SkinID IS NULL or SkinID=0 or SkinID={1}) ", CommonLogic.IIF(AppLogic.IsAdminSite, "", "ShowInSiteMap=1 and "), SkinID.ToString()) + "; " + string.Format("select Name,Title,TopicID from Topic with (NOLOCK) where {0} Deleted=0 and Published=1 and (SkinID IS NULL or SkinID=0 or SkinID={1}) Order By DisplayOrder, Name ASC", CommonLogic.IIF(AppLogic.IsAdminSite, "", "ShowInSiteMap=1 and "), SkinID.ToString()), dbconn))
                        {
                            if (rs.Read() && DB.RSFieldInt(rs, "N") > 0)
                            {
                                if (rs.NextResult())
                                {
                                    while (rs.Read())
                                    {
                                        tmpS.Append("<li>");
                                        if (AppLogic.IsAdminSite)
                                        {
                                            tmpS.Append(String.Format("<a href=\"edittopic.aspx?topicid={0}\">", DB.RSFieldInt(rs, "TopicID").ToString()));
                                        }
                                        else
                                        {
                                            tmpS.Append("<a href=\"" + SE.MakeDriverLink(DB.RSFieldByLocale(rs, "Name", Localization.GetDefaultLocale())) + "\">");
                                        }
                                        tmpS.Append(Security.HtmlEncode(DB.RSFieldByLocale(rs, "Title", Localization.GetDefaultLocale())));
                                        tmpS.Append("</a>");
                                        tmpS.Append("</li>\n");
                                    }
                                }
                            }
                        }
                    }

                    // File Topics:
                    // create an array to hold the list of files
                    ArrayList fArray = new ArrayList();

                    // get information about our initial directory
                    String SFP = CommonLogic.SafeMapPath(CommonLogic.IIF(AppLogic.IsAdminSite, "../", "") + "App_Templates/Skin_" + SkinID.ToString() + "/template.htm").Replace("template.htm", "");

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
                            if (!myDir[i].FullName.EndsWith("htm", StringComparison.InvariantCultureIgnoreCase) || 
                                (myDir[i].FullName.IndexOf("TEMPLATE", StringComparison.InvariantCultureIgnoreCase) != -1) || 
                                (myDir[i].FullName.IndexOf("AFFILIATE_", StringComparison.InvariantCultureIgnoreCase) != -1) || 
                                (myDir[i].FullName.IndexOf(AppLogic.ro_PMMicropay, StringComparison.InvariantCultureIgnoreCase) != -1))
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
                            tmpS.Append("<li>");
                            if (!AppLogic.IsAdminSite) // admin site can't link to these kinds of topics
                            {
                                tmpS.Append("<a href=\"" + SE.MakeDriverLink(fArray[i].ToString().Replace(".htm", "")) + "\">");
                            }
                            else
                            {
                                tmpS.Append("(file based topic) ");
                            }
                            tmpS.Append(Security.HtmlEncode(CommonLogic.Capitalize(fArray[i].ToString().Replace(".htm", ""))));
                            if (!AppLogic.IsAdminSite)
                            {
                                tmpS.Append("</a>");
                            }
                            tmpS.Append("</li>\n");
                        }
                    }
                    tmpS.Append("</ul>\n");
                }
                m_Contents = tmpS.ToString();
                if (AppLogic.CachingOn)
                {
                    HttpContext.Current.Cache.Insert(CacheName, m_Contents, null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
                }
            }

        }
    }
}
