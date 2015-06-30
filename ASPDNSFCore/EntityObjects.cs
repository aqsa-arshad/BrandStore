// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Drawing;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace AspDotNetStorefrontCore
{
    /// <summary>
    /// Generic database object class
    /// </summary>
    [Serializable]
    public class DatabaseObject
    {
        public DatabaseObject()
        {
        }

        private int m_id;

        /// <summary>
        /// Gets or sets the ID
        /// </summary>
        public int ID
        {
            get { return m_id; }
            set { m_id = value; }
        }

        private string m_name;
        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        private string m_description;

        /// <summary>
        /// Gets or sets the Description
        /// </summary>
        public string Description
        {
            get { return m_description; }
            set { m_description = value; }
        }

        private Guid m_guid;

        /// <summary>
        /// Gets or sets the Guid
        /// </summary>
        public Guid GUID
        {
            get { return m_guid; }
            set { m_guid = value; }
        }

        private string m_type;

        /// <summary>
        /// Gets or sets the Type
        /// </summary>
        public string Type
        {
            get { return m_type; }
            set { m_type = value; }
        }
    }

    /// <summary>
    /// Generic collection that provides paged information
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PagedEntityObjectCollection<T> : List<T> where T : class
    {
        private string m_entitytype;
        private PagingInfo m_pageinfo;

        public string EntityType
        {
            get { return m_entitytype; }
            set { m_entitytype = value; }
        }

        public PagingInfo PageInfo
        {
            get { return m_pageinfo; }
            set { m_pageinfo = value; }
        }
    }

    /// <summary>
    /// DatabaseObject collection class
    /// </summary>
    public class DatabaseObjectCollection : PagedEntityObjectCollection<DatabaseObject>
    {
        /// <summary>
        /// Gets a collection of databaseobjects
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="alphaFilter"></param>
        /// <param name="searchFilter"></param>
        /// <param name="pageSize"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public static DatabaseObjectCollection GetObjects(string entityType,
            string alphaFilter,
            string searchFilter,
            int pageSize,
            int page)
        {
            DatabaseObjectCollection objs = new DatabaseObjectCollection();
            objs.EntityType = entityType;

            SqlCommand cmd = new SqlCommand("aspdnsf_GetObjects");
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@EntityType", SqlDbType.NVarChar, 30) { Value = entityType });

            // nullable parameters
            SqlParameter pAlphaFilter = new SqlParameter("@AlphaFilter", SqlDbType.NVarChar, 30) { Value = DBNull.Value };
            SqlParameter pSearchFilter = new SqlParameter("@SearchFilter", SqlDbType.NVarChar, 30) { Value = DBNull.Value };
            SqlParameter pPageSize = new SqlParameter("@PageSize", SqlDbType.Int) { Value = DBNull.Value };
            SqlParameter pPage = new SqlParameter("@Page", SqlDbType.Int) { Value = DBNull.Value };

            if (!string.IsNullOrEmpty(alphaFilter))
            {
                pAlphaFilter.Value = alphaFilter;
            }

            if (!string.IsNullOrEmpty(searchFilter))
            {
                pSearchFilter.Value = searchFilter;
            }

            if (pageSize > 0 && page > 0)
            {
                pPageSize.Value = pageSize;
                pPage.Value = page;
            }

            cmd.Parameters.Add(pAlphaFilter);
            cmd.Parameters.Add(pSearchFilter);
            cmd.Parameters.Add(pPageSize);
            cmd.Parameters.Add(pPage);

            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                using (IDataReader rs = cmd.ExecuteReader())
                {
                    // first result set is the paging data
                    if (rs.Read())
                    {
                        PagingInfo nfo = new PagingInfo();
                        nfo.TotalCount = rs.FieldInt("TotalCount");
                        nfo.PageSize = rs.FieldInt("PageSize");
                        nfo.CurrentPage = rs.FieldInt("CurrentPage");
                        nfo.TotalPages = rs.FieldInt("TotalPages");
                        nfo.StartIndex = rs.FieldInt("StartIndex");
                        nfo.EndIndex = rs.FieldInt("EndIndex");

                        objs.PageInfo = nfo;
                    }

                    rs.NextResult();

                    // next is the actual result set
                    while (rs.Read())
                    {
                        DatabaseObject obj = new DatabaseObject();
                        obj.ID = rs.FieldInt("ID");
                        obj.Type = rs.Field("EntityType");
                        obj.Name = rs.Field("Name");
                        objs.Add(obj);
                    }
                }
            }

            return objs;
        }
    }


    #region "SEO"
    public interface iSearchable
    {
        SEOTags SEO();

    }

    /// <summary>
    /// Tags for SEO Optimization
    /// </summary>
    [Serializable]
    public class SEOTags
    {
        private static string retrieveSQL
        {
            get
            {
                return @"
    SELECT SEKeywords, SEDescription, SETitle, SEAltText, SEName
    FROM EntityMaster WITH (NOLOCK) WHERE EntityID = @EntityID AND EntityType = @EntityType
";
            }
        }
        public SEOTags(string EntityType, int objectID)
        {
            using (SqlConnection xCon = new SqlConnection(DB.GetDBConn()))
            {
                xCon.Open();
                SqlCommand xCmd = new SqlCommand(retrieveSQL, xCon);
                xCmd.Parameters.Add(new SqlParameter("@EntityID", objectID));
                xCmd.Parameters.Add(new SqlParameter("@EntityType", EntityType));
                SqlDataReader xRdr = xCmd.ExecuteReader();
                xRdr.Read();
                new SEOTags(xRdr);
            }
        }
        /// <param name="reader">DataReader with a row read containing SEO fields</param>
        public SEOTags(IDataReader reader)
        {
            Keywords = reader["SEKeywords"].ToString();
            Description = reader["SEDescription"].ToString();
            Title = reader["SETitle"].ToString();
            AltText = reader["SEAltText"].ToString();
            Name = reader["SEName"].ToString();
        }

        /// <param name="row">DataRow containing SEO Fields</param>
        public SEOTags(DataRow row)
        {
            Keywords = row["SEKeywords"].ToString();
            Description = row["SEDescription"].ToString();
            Title = row["SETitle"].ToString();
            AltText = row["SEAltText"].ToString();
            Name = row["SEName"].ToString();
        }

        protected SEOTags()
        {

        }

        private string m_keywords;
        /// <summary>
        /// SEO keywords
        /// </summary>
        public string Keywords
        {
            get { return m_keywords; }
            set { m_keywords = value; }
        }

        private string m_description;
        
        /// <summary>
        /// SEO description
        /// </summary>
        public string Description
        {
            get { return m_description; }
            set { m_description = value; }
        }

        private string m_title;

        /// <summary>
        /// SEO Title
        /// </summary>
        public string Title
        {
            get { return m_title; }
            set { m_title = value; }
        }

        private string m_alttext;
        
        /// <summary>
        /// Alt Text for the image (SEO indexed)
        /// </summary>
        public string AltText
        {
            get { return m_alttext; }
            set { m_alttext = value; }
        }

        private string m_name;
        
        /// <summary>
        /// SEO Name
        /// </summary>
        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }


    }
    #endregion

    #region "Rendered Content"


    public interface iDisplay
    {
        DisplayTags Display();
    }
    
    [Serializable]
    public class DisplayTags
    {
        protected DisplayTags()
        {
            
        }
        private static string retrieveSQL
        {
            get
            {
                return @"
    SELECT ContentsBGColor,PageBGColor,GraphicsColor,ImageFilenameOverride,SkinID,XMLPackage,
    Summary,Description,TemplateName,ColWidth,SortByLooks,DisplayOrder,Published,ShowInProductBrowser,
    PageSize
    FROM EntityMaster WITH (NOLOCK) WHERE EntityID = @EntityID AND EntityType = @EntityType
";
            }
        }
        public DisplayTags(string EntityType, int objectID)
        {
            using (SqlConnection xCon = new SqlConnection(DB.GetDBConn()))
            {
                xCon.Open();
                SqlCommand xCmd = new SqlCommand(retrieveSQL, xCon);
                xCmd.Parameters.Add(new SqlParameter("@EntityID", objectID));
                xCmd.Parameters.Add(new SqlParameter("@EntityType", EntityType));
                SqlDataReader xRdr = xCmd.ExecuteReader();
                xRdr.Read();
                new DisplayTags(xRdr);
            }
        }

        public DisplayTags(IDataReader reader)
        {
            ContentsBGColor = reader["ContentsBGColor"].ToString();
            PageBGColor = reader["PageBGColor"].ToString();
            GraphicsColor = reader["GraphicsColor"].ToString();
            ImageFileNameOverride = reader["ImageFilenameOverride"].ToString();
            SkinID = (int)reader["SkinID"];
            XMLPackage = reader["XMLPackage"].ToString();
            Summary = reader["Summary"].ToString();
            Description = reader["Description"].ToString();
            TemplateName = reader["TemplateName"].ToString();
            ColWidth = (int)reader["ColWidth"];
            SortByLooks = (byte)reader["SortByLooks"] == 1 ;
            DisplayOrder = (int)reader["DisplayOrder"];
            Published = (byte)reader["Published"] == 1;
            ShowInProductBrowser = (int)reader["ShowInProductBrowser"] == 1;
            PageSize  = (int)reader["PageSize"];

        }
        public DisplayTags(DataRow row)
        {
            ContentsBGColor = row["ContentsBGColor"].ToString();
            PageBGColor = row["PageBGColor"].ToString();
            GraphicsColor = row["GraphicsColor"].ToString();
            ImageFileNameOverride = row["ImageFilenameOverride"].ToString();
            SkinID = (int)row["SkinID"];
            XMLPackage = row["XMLPackage"].ToString();
            Summary = row["Summary"].ToString();
            Description = row["Description"].ToString();
            TemplateName = row["TemplateName"].ToString();
            ColWidth = (int)row["ColWidth"];
            SortByLooks = (byte)row["SortByLooks"] == 1;
            DisplayOrder = (int)row["DisplayOrder"];
            Published = (byte)row["Published"] == 1;
            ShowInProductBrowser = (int)row["ShowInProductBrowser"] == 1;
            PageSize = (int)row["PageSize"];


        }
#region "Properties"
        private string m_xmlpackage;
        public string XMLPackage
        {
            get { return m_xmlpackage; }
            set { m_xmlpackage = value; }
        }
        private string m_contentsbgcolor;
        public string ContentsBGColor
        {
            get { return m_contentsbgcolor; }
            set { m_contentsbgcolor = value; }
        }
        private string m_pagebgcolor;
        public string PageBGColor
        {
            get { return m_pagebgcolor; }
            set { m_pagebgcolor = value; }
        }
        private string m_graphicscolor;
        public string GraphicsColor
        {
            get { return m_graphicscolor; }
            set { m_graphicscolor = value; }
        }
        private string m_imagefilenameoverride;
        public string ImageFileNameOverride
        {
            get { return m_imagefilenameoverride; }
            set { m_imagefilenameoverride = value; }
        }
        private int m_skinid;
        public int SkinID
        {
            get { return m_skinid; }
            set { m_skinid = value; }
        }
        private string m_summary;
        public string Summary
        {
            get { return m_summary; }
            set { m_summary = value; }
        }
        private string m_description;
        public string Description
        {
            get { return m_description; }
            set { m_description = value; }
        }
        private string m_templatename;
        public string TemplateName
        {
            get { return m_templatename; }
            set { m_templatename = value; }
        }
        private int m_colwidth;
        public int ColWidth
        {
            get { return m_colwidth; }
            set { m_colwidth = value; }
        }
        private bool m_sortbylooks;
        public bool SortByLooks
        {
            get { return m_sortbylooks; }
            set { m_sortbylooks = value; }
        }
        private int m_displayorder;
        public int DisplayOrder
        {
            get { return m_displayorder; }
            set { m_displayorder = value; }
        }
        private bool m_published;
        public bool Published
        {
            get { return m_published; }
            set { m_published = value; }
        }
        private bool m_showinproductbrowser;
        public bool ShowInProductBrowser
        {
            get { return m_showinproductbrowser; }
            set { m_showinproductbrowser = value; }
        }
        private int m_pagesize;
        public int PageSize
        {
            get { return m_pagesize; }
            set { m_pagesize = value; }
        }

#endregion
    }

    #endregion

}
