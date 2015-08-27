// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Web;

namespace AspDotNetStorefrontCore
{
    /// <summary>
    /// Summary description for SiteMapComponentArt.
    /// </summary>
    public class SiteMapComponentArt
    {
        private String m_Contents;

        public String Contents
        {
            get
            {
                return m_Contents;
            }
        }
        public SiteMapComponentArt(Dictionary<string, EntityHelper> EntityHelpers, int SkinID, Customer ThisCustomer)
            :this(EntityHelpers, SkinID, ThisCustomer, true)
        {

        }
        public SiteMapComponentArt(Dictionary<string, EntityHelper> EntityHelpers, int SkinID, Customer ThisCustomer, bool showCustomerService)
        {
            bool FromCache = false;
            String CacheName = String.Format("SiteMapComponentArt_{0}_{1}", SkinID.ToString(), ThisCustomer.LocaleSetting);
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
                tmpS.Append("<SiteMap>\n");

                if (AppLogic.IsAdminSite || AppLogic.AppConfigBool("SiteMap.ShowCategories"))
                {
                    // Categories:
                    String s = AppLogic.LookupHelper("Category", 0).GetEntityComponentArtNode(0, ThisCustomer.LocaleSetting, ThisCustomer.AffiliateID, ThisCustomer.CustomerLevelID, true, AppLogic.IsAdminSite || AppLogic.AppConfigBool("SiteMap.ShowProducts") && AppLogic.NumProductsInDB < 250);
                    if (s.Length != 0)
                    {
                        if (AppLogic.IsAdminSite)
                        {
                            tmpS.Append("<node Text=\"" + XmlCommon.XmlEncodeAttribute(AppLogic.GetString("AppConfig.CategoryPromptPlural", SkinID, ThisCustomer.LocaleSetting).ToUpperInvariant()) + "\" NavigateUrl=\"newentities.aspx?entityname=category\">\n");
                        }
                        else
                        {
                            tmpS.Append("<node Text=\"" + XmlCommon.XmlEncodeAttribute(AppLogic.GetString("AppConfig.CategoryPromptPlural", SkinID, ThisCustomer.LocaleSetting).ToUpperInvariant()) + "\">\n");
                        }
                        tmpS.Append(s);
                        tmpS.Append("</node>");
                    }
                }

                if (AppLogic.IsAdminSite || AppLogic.AppConfigBool("SiteMap.ShowSections"))
                {
                    // Sections:
                    String s = AppLogic.LookupHelper("Section", 0).GetEntityComponentArtNode(0, ThisCustomer.LocaleSetting, ThisCustomer.AffiliateID, ThisCustomer.CustomerLevelID, true, AppLogic.IsAdminSite || AppLogic.AppConfigBool("SiteMap.ShowProducts") && AppLogic.NumProductsInDB < 250);
                    if (s.Length != 0)
                    {
                        if (AppLogic.IsAdminSite)
                        {
                            tmpS.Append("<node Text=\"" + XmlCommon.XmlEncodeAttribute(AppLogic.GetString("AppConfig.SectionPromptPlural", SkinID, ThisCustomer.LocaleSetting).ToUpperInvariant()) + "\" NavigateUrl=\"newentities.aspx?entityname=section\">\n");
                        }
                        else
                        {
                            tmpS.Append("<node Text=\"" + XmlCommon.XmlEncodeAttribute(AppLogic.GetString("AppConfig.SectionPromptPlural", SkinID, ThisCustomer.LocaleSetting).ToUpperInvariant()) + "\">\n");
                        }
                        tmpS.Append(s);
                        tmpS.Append("</node>");
                    }
                }

                if (AppLogic.IsAdminSite || AppLogic.AppConfigBool("SiteMap.ShowLibraries"))
                {
                    // Libraries:
                    String s = AppLogic.LookupHelper("Library", 0).GetEntityComponentArtNode(0, ThisCustomer.LocaleSetting, ThisCustomer.AffiliateID, ThisCustomer.CustomerLevelID, true, AppLogic.IsAdminSite || AppLogic.AppConfigBool("SiteMap.ShowDocuments") && AppLogic.NumProductsInDB < 250);
                    if (s.Length != 0)
                    {
                        if (AppLogic.IsAdminSite)
                        {
                            tmpS.Append("<node Text=\"" + XmlCommon.XmlEncodeAttribute(AppLogic.GetString("AppConfig.LibraryPromptPlural", SkinID, ThisCustomer.LocaleSetting).ToUpperInvariant()) + "\" NavigateUrl=\"newentities.aspx?entityname=library\">\n");
                        }
                        else
                        {
                            tmpS.Append("<node Text=\"" + XmlCommon.XmlEncodeAttribute(AppLogic.GetString("AppConfig.LibraryPromptPlural", SkinID, ThisCustomer.LocaleSetting).ToUpperInvariant()) + "\">\n");
                        }
                        tmpS.Append(s);
                        tmpS.Append("</node>");
                    }
                }

                if (AppLogic.IsAdminSite || AppLogic.AppConfigBool("SiteMap.ShowManufacturers"))
                {
                    // Manufacturers:
                    String s = AppLogic.LookupHelper("Manufacturer", 0).GetEntityComponentArtNode(0, ThisCustomer.LocaleSetting, ThisCustomer.AffiliateID, ThisCustomer.CustomerLevelID, true, AppLogic.IsAdminSite || AppLogic.AppConfigBool("SiteMap.ShowProducts") && AppLogic.NumProductsInDB < 250);
                    if (s.Length != 0)
                    {
                        if (AppLogic.IsAdminSite)
                        {
                            tmpS.Append("<node Text=\"" + XmlCommon.XmlEncodeAttribute(AppLogic.GetString("AppConfig.ManufacturerPromptPlural", SkinID, ThisCustomer.LocaleSetting).ToUpperInvariant()) + "\" NavigateUrl=\"newentities.aspx?entityname=manufacturer\">\n");
                        }
                        else
                        {
                            tmpS.Append("<node Text=\"" + XmlCommon.XmlEncodeAttribute(AppLogic.GetString("AppConfig.ManufacturerPromptPlural", SkinID, ThisCustomer.LocaleSetting).ToUpperInvariant()) + "\">\n");
                        }
                        tmpS.Append(s);
                        tmpS.Append("</node>");
                    }
                }

                if (!AppLogic.IsAdminSite && AppLogic.AppConfigBool("SiteMap.ShowCustomerService") && showCustomerService)
                {
                    tmpS.Append("<node Text=\"" + XmlCommon.XmlEncodeAttribute(AppLogic.GetString("menu.CustomerService", ThisCustomer.SkinID, ThisCustomer.LocaleSetting)) + "\">\n");
                    tmpS.Append("	<node Text=\"" + XmlCommon.XmlEncodeAttribute(AppLogic.GetString("menu.YourAccount", SkinID, ThisCustomer.LocaleSetting)) + "\" NavigateUrl=\"account.aspx\" />\n");
                    tmpS.Append("	<node Text=\"" + XmlCommon.XmlEncodeAttribute(AppLogic.GetString("menu.OrderHistory", SkinID, ThisCustomer.LocaleSetting)) + "\" NavigateUrl=\"account.aspx\" />\n");
                    
                    tmpS.Append("	<node Text=\"" + XmlCommon.XmlEncodeAttribute(AppLogic.GetString("menu.PolicyReturns", SkinID, ThisCustomer.LocaleSetting)) + "\" NavigateUrl=\"" + SE.MakeDriverLink("returns") + "\" />\n");
                    tmpS.Append("	<node Text=\"" + XmlCommon.XmlEncodeAttribute(AppLogic.GetString("menu.Shipping", SkinID, ThisCustomer.LocaleSetting)) + "\" NavigateUrl=\"" + SE.MakeDriverLink("shipping") + "\" />\n");
                    tmpS.Append("	<node Text=\"" + XmlCommon.XmlEncodeAttribute(AppLogic.GetString("menu.Contact", SkinID, ThisCustomer.LocaleSetting)) + "\" NavigateUrl=\"contactus.aspx\" />\n");
                    tmpS.Append("	<node Text=\"" + XmlCommon.XmlEncodeAttribute(AppLogic.GetString("menu.PolicyPrivacy", SkinID, ThisCustomer.LocaleSetting)) + "\" NavigateUrl=\"" + SE.MakeDriverLink("privacy") + "\" />\n");
                    tmpS.Append("	<node Text=\"" + XmlCommon.XmlEncodeAttribute(AppLogic.GetString("menu.PolicySecurity", SkinID, ThisCustomer.LocaleSetting)) + "\" NavigateUrl=\"" + SE.MakeDriverLink("security") + "\" />\n");
                    tmpS.Append("</node>\n");
                }

                if (AppLogic.IsAdminSite || AppLogic.AppConfigBool("SiteMap.ShowTopics"))
                {
                    // Topics:
                    if (AppLogic.IsAdminSite)
                    {
                        tmpS.Append("<node Text=\"" + XmlCommon.XmlEncodeAttribute(AppLogic.GetString("sitemap.aspx.2", SkinID, ThisCustomer.LocaleSetting).ToUpperInvariant()) + "\" NavigateUrl=\"topics.aspx\">\n");
                    }
                    else
                    {
                        tmpS.Append("<node Text=\"" + XmlCommon.XmlEncodeAttribute(AppLogic.GetString("sitemap.aspx.2", SkinID, ThisCustomer.LocaleSetting).ToUpperInvariant()) + "\" NavigateUrl=\"\">\n");
                    }

                    using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                    {
                        con.Open();
                        using (IDataReader rs = DB.GetRS(string.Format("select Name,Title,TopicID,ShowInSiteMap from Topic with (NOLOCK) where {0} Deleted=0 and Published=1 and (SkinID IS NULL or SkinID=0 or SkinID={1}) Order By DisplayOrder, Name ASC", CommonLogic.IIF(AppLogic.IsAdminSite, "", "ShowInSiteMap=1 and "), SkinID.ToString()), con))
                        {
                            string displayname = string.Empty;
                            string name = string.Empty;
                            while(rs.Read())
                            {
                                String URL = String.Empty;
                                name = DB.RSFieldByLocale(rs, "Name", ThisCustomer.LocaleSetting);
                                if (AppLogic.IsAdminSite)
                                {
                                    URL = String.Format("topics.aspx?EditTopicId={0}", DB.RSFieldInt(rs, "TopicID").ToString());
                                }
                                else
                                {
                                    URL = SE.MakeDriverLink(name);
                                }
                                displayname = XmlCommon.XmlEncodeAttribute(DB.RSFieldByLocale(rs, "Title", ThisCustomer.LocaleSetting));
                                if (displayname != string.Empty)
                                {
                                    if (name.IndexOf("GOOGLE", StringComparison.InvariantCultureIgnoreCase) == -1 && name.IndexOf("PHONE", StringComparison.InvariantCultureIgnoreCase) == -1
                                        && name.IndexOf("AFFILIATE", StringComparison.InvariantCultureIgnoreCase) == -1 && name.IndexOf("GIFTREGISTRY", StringComparison.InvariantCultureIgnoreCase) == -1
                                        && name.IndexOf("WISHLIST", StringComparison.InvariantCultureIgnoreCase) == -1 && name.IndexOf("CHECKOUTANON", StringComparison.InvariantCultureIgnoreCase) == -1
                                        && name.IndexOf("DOWNLOAD", StringComparison.InvariantCultureIgnoreCase) == -1 && name.IndexOf("GENRE", StringComparison.InvariantCultureIgnoreCase) == -1
                                        && name.IndexOf("DISTRIBUTOR", StringComparison.InvariantCultureIgnoreCase) == -1 && name.IndexOf("VECTOR", StringComparison.InvariantCultureIgnoreCase) == -1
                                        && name.IndexOf("CARTPAGEFOOTER", StringComparison.InvariantCultureIgnoreCase) == -1 && name.IndexOf("CODINSTRUCTIONS", StringComparison.InvariantCultureIgnoreCase) == -1)
                                    {
                                        tmpS.Append("<node Text=\"" + displayname + "\" NavigateUrl=\"" + XmlCommon.XmlEncodeAttribute(URL) + "\" />\n");
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
                            String URL = String.Empty;
                            if (!AppLogic.IsAdminSite) // admin site can't link to these kinds of topics
                            {
                                URL = SE.MakeDriverLink(fArray[i].ToString().Replace(".htm", ""));
                            }
                            tmpS.Append("<node Text=\"" + XmlCommon.XmlEncodeAttribute(CommonLogic.Capitalize(fArray[i].ToString().Replace(".htm", ""))) + "\" " + CommonLogic.IIF(URL.Length != 0, "NavigateUrl=\"" + XmlCommon.XmlEncodeAttribute(URL) + "\"", "") + "/>\n");
                        }
                    }
                    tmpS.Append("</node>");
                }

                tmpS.Append("</SiteMap>\n");
                m_Contents = tmpS.ToString();
                if (AppLogic.CachingOn)
                {
                    HttpContext.Current.Cache.Insert(CacheName, m_Contents, null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
                }
            }

        }
    }
}
