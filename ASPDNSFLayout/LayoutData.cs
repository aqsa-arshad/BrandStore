// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Data;
using System.Web.UI.WebControls;
using System;
using System.Web.UI;
using AspDotNetStorefrontCore;
using System.Data.SqlClient;
using System.IO;
using System.Xml;
using System.Text;
using System.Xml.Linq;

namespace AspDotNetStorefrontLayout
{
    public enum LayoutFieldEnum
    {
        Unknown = -1,
        ASPDNSFImageField = 0,
        ASPDNSFTextField = 1
    }

    /// <summary>
    /// Represents an AspDotNetStorefront Layout
    /// </summary>
    public class LayoutData
    {
        #region Private Variables

        private int m_layoutid;
        private Guid m_layoutguid;
        private string m_name;
        private string m_description;
        private string m_html;
        private string m_micro;
        private string m_icon;
        private string m_medium;
        private string m_large;
        //private string m_controlmarkup;
        private int m_version;
        private DateTime m_createdon;
        private DateTime m_updatedon;
        private List<LayoutField> m_layoutfields;

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiates a new, blank LayoutData
        /// </summary>
        public LayoutData() 
        {
            m_layoutid = 0;
            m_layoutguid = new Guid();
            m_name = String.Empty;
            m_description = String.Empty;
            m_html = String.Empty;
            m_micro = String.Empty;
            m_icon = String.Empty;
            m_medium = String.Empty;
            m_large = String.Empty;
            //m_controlmarkup = String.Empty;
            m_version = 1;
            m_createdon = DateTime.Now;
            m_updatedon = DateTime.Now;
            m_layoutfields = new List<LayoutField>();
        }

        /// <summary>
        /// Instantiates a new LayoutData, retrieving data from the database using the LayoutID
        /// </summary>
        /// <param name="lID">The LayoutID of the Layout to retrieve</param>
        public LayoutData(int lID)
        {
            if (lID > 0)
            {
                using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
                {
                    conn.Open();

                    using (IDataReader rs = DB.GetRS("select * from dbo.Layout with(NOLOCK) where LayoutID=" + lID.ToString(), conn))
                    {
                        if (rs.Read())
                        {
                            m_layoutid = DB.RSFieldInt(rs, "LayoutID");
                            m_layoutguid = DB.RSFieldGUID2(rs, "LayoutGUID");
                            m_name = DB.RSField(rs, "Name");
                            m_description = DB.RSField(rs, "Description");
                            m_html = DB.RSField(rs, "HTML");
                            m_micro = DB.RSField(rs, "Micro");
                            m_icon = DB.RSField(rs, "Icon");
                            m_medium = DB.RSField(rs, "Medium");
                            m_large = DB.RSField(rs, "Large");
                            //m_controlmarkup = DB.RSField(rs, "ControlMarkup");
                            m_version = DB.RSFieldInt(rs, "Version");
                            m_createdon = DB.RSFieldDateTime(rs, "CreatedOn");
                            m_updatedon = DB.RSFieldDateTime(rs, "UpdatedOn");

                        }

                        rs.Close();
                        rs.Dispose();
                    }
                    conn.Close();
                }

                m_layoutfields = LayoutField.GetFields(lID);
            }
        }

        /// <summary>
        /// Instantiates a new LayoutData, retrieving data from the database using the LayoutGUID
        /// </summary>
        /// <param name="lGUID">The LayoutGUID of the Layout to retrieve</param>
        public LayoutData(Guid lGUID)
        {
            if (!lGUID.Equals(new Guid()))
            {
                using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
                {
                    conn.Open();

                    using (IDataReader rs = DB.GetRS("select * from dbo.Layout with(NOLOCK) where LayoutGUID=" +DB.SQuote(lGUID.ToString()), conn))
                    {
                        if (rs.Read())
                        {
                            m_layoutid = DB.RSFieldInt(rs, "LayoutID");
                            m_layoutguid = DB.RSFieldGUID2(rs, "LayoutGUID");
                            m_name = DB.RSField(rs, "Name");
                            m_description = DB.RSField(rs, "Description");
                            m_html = DB.RSField(rs, "HTML");
                            m_micro = DB.RSField(rs, "Micro");
                            m_icon = DB.RSField(rs, "Icon");
                            m_medium = DB.RSField(rs, "Medium");
                            m_large = DB.RSField(rs, "Large");
                            //m_controlmarkup = DB.RSField(rs, "ControlMarkup");
                            m_version = DB.RSFieldInt(rs, "Version");
                            m_createdon = DB.RSFieldDateTime(rs, "CreatedOn");
                            m_updatedon = DB.RSFieldDateTime(rs, "UpdatedOn");
                        }

                        rs.Close();
                        rs.Dispose();
                    }
                    conn.Close();
                }

                m_layoutfields = LayoutField.GetFields(m_layoutid);
            }
        }

        /// <summary>
        /// Instantiates a new LayoutData, retrieving data from the database using the Name
        /// </summary>
        /// <param name="lName">The Name of the Layout to retrieve</param>
        public LayoutData(String lName)
        {
            if (lName.Length > 0)
            {
                using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
                {
                    conn.Open();

                    using (IDataReader rs = DB.GetRS("select * from dbo.Layout with(NOLOCK) where Name=" + DB.SQuote(lName), conn))
                    {
                        if (rs.Read())
                        {
                            m_layoutid = DB.RSFieldInt(rs, "LayoutID");
                            m_layoutguid = DB.RSFieldGUID2(rs, "LayoutGUID");
                            m_name = DB.RSField(rs, "Name");
                            m_description = DB.RSField(rs, "Description");
                            m_html = DB.RSField(rs, "HTML");
                            m_micro = DB.RSField(rs, "Micro");
                            m_icon = DB.RSField(rs, "Icon");
                            m_medium = DB.RSField(rs, "Medium");
                            m_large = DB.RSField(rs, "Large");
                            //m_controlmarkup = DB.RSField(rs, "ControlMarkup");
                            m_version = DB.RSFieldInt(rs, "Version");
                            m_createdon = DB.RSFieldDateTime(rs, "CreatedOn");
                            m_updatedon = DB.RSFieldDateTime(rs, "UpdatedOn");
                        }

                        rs.Close();
                        rs.Dispose();
                    }
                    conn.Close();
                }

                m_layoutfields = LayoutField.GetFields(m_layoutid);
            }
        }

        /// <summary>
        /// Instantiates a new LayoutData, using an existing data reader
        /// </summary>
        /// <param name="rs">The IDataReader containing LayoutData data</param>
        //public LayoutData(IDataReader rs)
        //{
        //    m_layoutid = DB.RSFieldInt(rs, "LayoutID");
        //    m_layoutguid = DB.RSFieldGUID2(rs, "LayoutGUID");
        //    m_name = DB.RSField(rs, "Name");
        //    m_description = DB.RSField(rs, "Description");
        //    m_html = DB.RSField(rs, "HTML");
        //    m_version = DB.RSFieldInt(rs, "Version");
        //    m_createdon = DB.RSFieldDateTime(rs, "CreatedOn");
        //    m_updatedon = DB.RSFieldDateTime(rs, "UpdatedOn");
        //}

        /// <summary>
        /// Instantiates a new LayoutData with user-defined data
        /// </summary>
        /// <param name="lID">The ID of the layout</param>
        /// <param name="lGUID">The GUID of the layout</param>
        /// <param name="lName">The Name of the layout</param>
        /// <param name="lDescription">The Description of the layout</param>
        /// <param name="lHTML">The HTML of the layout</param>
        /// <param name="lVersion">The Version of the layout</param>
        /// <param name="lCreatedOn">The CreatedOn date of the layout</param>
        /// <param name="lUpdatedOn">The UpdatedOn date of the layout</param>
        public LayoutData(int lID, Guid lGUID, string lName, string lDescription, string lHTML, int lVersion, DateTime lCreatedOn, DateTime lUpdatedOn)
        {
            m_layoutid = lID;
            m_layoutguid = lGUID;
            m_name = lName;
            m_description = lDescription;
            m_html = lHTML;
            m_micro = String.Empty;
            m_icon = String.Empty;
            m_medium = String.Empty;
            m_large = String.Empty;
            //m_controlmarkup = lControlMarkup;
            m_version = lVersion;
            m_createdon = lCreatedOn;
            m_updatedon = lUpdatedOn;
            m_layoutfields = new List<LayoutField>();
        }

        #endregion

        #region Public Properties

        public int LayoutID
        {
            get { return m_layoutid; }
            set { m_layoutid = value; }
        }

        public Guid LayoutGUID
        {
            get { return m_layoutguid; }
            set { m_layoutguid = value; }
        }

        public String Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        public String Description
        {
            get { return m_description; }
            set { m_description = value; }
        }

        public String HTML
        {
            get { return m_html; }
            set { m_html = value; }
        }

        public String Micro
        {
            get { return m_micro; }
            set { m_micro = value; }
        }

        public String Icon
        {
            get { return m_icon; }
            set { m_icon = value; }
        }

        public String Medium
        {
            get { return m_medium; }
            set { m_medium = value; }
        }

        public String Large
        {
            get { return m_large; }
            set { m_large = value; }
        }

        //public String ControlMarkup
        //{
        //    get { return m_controlmarkup; }
        //    set { m_controlmarkup = value; }
        //}

        public int Version
        {
            get { return m_version; }
            set { m_version = value; }
        }

        public DateTime CreatedOn
        {
            get { return m_createdon; }
            set { m_createdon = value; }
        }

        public DateTime UpdatedOn
        {
            get { return m_updatedon; }
            set { m_updatedon = value; }
        }

        public Boolean IsMapped
        {
            get { return DB.GetSqlN("select count(*) as N from dbo.LayoutMap with(NOLOCK) where LayoutID=" + LayoutID.ToString()) > 0; }
        }

        public String LayoutFile
        {
            get { return CommonLogic.SafeMapPath("~/layouts/" + LayoutID.ToString() + "/" + Name + "_" + Version.ToString() + ".ascx"); }
        }

        public List<LayoutField> LayoutFields
        {
            get { return m_layoutfields; }
            set { m_layoutfields = value; }
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Determines if a Layout by the same name already exists
        /// </summary>
        /// <param name="Name">The name of the layout being used</param>
        /// <returns>True if the layout already exists, else false</returns>
        public static bool LayoutExists(String Name)
        {
            bool exists = false;

            if (Name.Length > 0)
            {
                using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
                {
                    conn.Open();

                    int count = DB.GetSqlN("select count(*) as N from dbo.Layout with(NOLOCK) where Name=" + DB.SQuote(Name), conn);

                    if (count > 0)
                    {
                        exists = true;
                    }

                    conn.Close();
                    conn.Dispose();
                }
            }

            return exists;
        }

        /// <summary>
        /// Creates a new Layout
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Description"></param>
        /// <param name="HTML"></param>
        /// <param name="ControlMarkup"></param>
        /// <returns>Fully populated LayoutData with newly added layout data</returns>
        public static LayoutData CreateLayout(String Name, String Description, String HTML, String ControlMarkup)
        {
            if (Name.Trim().Length == 0)
            {
                return null;
            }

            LayoutData ld = new LayoutData();

            ld.LayoutID = 0;
            ld.LayoutGUID = new Guid();
            ld.Name = Name;
            ld.Description = Description;
            ld.HTML = HTML;
            ld.Micro = String.Empty;
            ld.Icon = String.Empty;
            ld.Medium = String.Empty;
            ld.Large = String.Empty;
            //ld.ControlMarkup = ControlMarkup;
            ld.Version = 1;
            ld.CreatedOn = DateTime.Now;
            ld.UpdatedOn = DateTime.Now;

            return CreateLayout(ld);

        }

        /// <summary>
        /// Commits a new layout to the database. Will normally only be called from WSI.
        /// <Layout Action="Add">
        ///   <Name>Layout Name</Name>
        ///   <Description>Layout Description</Description>
        ///   <HTML><![CDATA[Layout HTML markup]]></HTML>
        ///   <Micro>Layout Micro Image Name</Micro>
        ///   <Icon>Layout Icon Image Name</Icon>
        ///   <Medium>Layout Medium Image Name</Medium>
        ///   <Large>Layout Large Image Name</Large>
        ///   <Version>Layout Version Number</Version>
        ///   <CreatedOn>Layout Created On Date</CreatedOn>
        ///   <UpdatedOn>Layout Updated On Date</UpdatedOn>
        /// </Layout>
        /// </summary>
        /// <param name="node">The XmlNode containing the Layout xml</param>
        /// <returns></returns>
        public static LayoutData CreateLayout(XmlNode node)
        {
            LayoutData ld = new LayoutData();

            ld.Name = XmlCommon.XmlField(node, "Name");
            ld.Description = XmlCommon.XmlField(node, "Description");
            ld.HTML = XmlCommon.XmlField(node, "HTML");
            ld.Micro = XmlCommon.XmlField(node, "Micro");
            ld.Icon = XmlCommon.XmlField(node, "Icon");
            ld.Medium = XmlCommon.XmlField(node, "Medium");
            ld.Large = XmlCommon.XmlField(node, "Large");
            ld.Version = XmlCommon.XmlFieldNativeInt(node, "Version");
            ld.CreatedOn = XmlCommon.XmlFieldNativeDateTime(node, "CreatedOn");
            ld.UpdatedOn = XmlCommon.XmlFieldNativeDateTime(node, "UpdatedOn");

            ld = CreateLayout(ld);

            ld.CreateLayoutControl();

            return ld;
        }

        /// <summary>
        /// Creates a new Layout
        /// </summary>
        /// <param name="ld"></param>
        /// <returns>Fully populated LayoutData with newly added layout data</returns>
        public static LayoutData CreateLayout(LayoutData ld)
        {
            int LayoutID = 0;

            if (ld == null)
            {
                return null;
            }

            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                using (SqlCommand scom = new SqlCommand())
                {
                    scom.Connection = conn;
                    scom.CommandType = CommandType.StoredProcedure;
                    scom.CommandText = "dbo.aspdnsf_insLayout";

                    scom.Parameters.Add(new SqlParameter("@LayoutGUID", SqlDbType.UniqueIdentifier));
                    scom.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar));
                    scom.Parameters.Add(new SqlParameter("@Description", SqlDbType.NVarChar));
                    scom.Parameters.Add(new SqlParameter("@HTML", SqlDbType.NVarChar));
                    scom.Parameters.Add(new SqlParameter("@Version", SqlDbType.Int, 4));
                    scom.Parameters.Add(new SqlParameter("@CreatedOn", SqlDbType.DateTime));
                    scom.Parameters.Add(new SqlParameter("@UpdatedOn", SqlDbType.DateTime));
                    scom.Parameters.Add(new SqlParameter("@LayoutID", SqlDbType.Int, 4)).Direction = ParameterDirection.Output;

                    if (ld.LayoutGUID.Equals(new Guid()))
                    {
                        ld.LayoutGUID = new Guid(DB.GetNewGUID());
                    }

                    scom.Parameters["@LayoutGUID"].Value = ld.LayoutGUID;
                    scom.Parameters["@Name"].Value = ld.Name;
                    scom.Parameters["@Description"].Value = ld.Description;
                    scom.Parameters["@HTML"].Value = ld.HTML;
                    scom.Parameters["@Version"].Value = ld.Version;
                    scom.Parameters["@CreatedOn"].Value = ld.CreatedOn;
                    scom.Parameters["@UpdatedOn"].Value = ld.UpdatedOn;


                    try
                    {
                        scom.ExecuteNonQuery();
                        LayoutID = Int32.Parse(scom.Parameters["@LayoutID"].Value.ToString());
                    }
                    catch { }

                    scom.Dispose();
                }
                conn.Close();
                conn.Dispose();
            }

            if (LayoutID > 0)
            {
                LayoutData ldata = new LayoutData(LayoutID);
                return ldata;
            }

            return null;
        }

        /// <summary>
        /// Updates an existing layout
        /// </summary>
        /// <param name="LayoutID">The LayoutID of the layout to update</param>
        /// <param name="Description">The updated layout description</param>
        /// <param name="HTML">The updated layout HTML</param>
        /// <param name="ControlMarkup">The updated layout ControlMarkup</param>
        /// <returns>True if the update was successful, else false</returns>
        public static bool Update(int LayoutID, String Description, String HTML, String ControlMarkup)
        {
            bool success = true;

            LayoutData ld = new LayoutData(LayoutID);

            ld.Description = Description;
            ld.HTML = HTML;
            ld.UpdatedOn = DateTime.Now;

            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                using (SqlCommand scom = new SqlCommand())
                {
                    scom.CommandType = CommandType.StoredProcedure;
                    scom.CommandText = "dbo.aspdnsf_updLayout";

                    scom.Parameters.Add(new SqlParameter("@LayoutID", SqlDbType.Int, 4));
                    scom.Parameters.Add(new SqlParameter("@Description", SqlDbType.NVarChar));
                    scom.Parameters.Add(new SqlParameter("@HTML", SqlDbType.NVarChar));
                    scom.Parameters.Add(new SqlParameter("@Micro", SqlDbType.NVarChar));
                    scom.Parameters.Add(new SqlParameter("@Icon", SqlDbType.NVarChar));
                    scom.Parameters.Add(new SqlParameter("@Medium", SqlDbType.NVarChar));
                    scom.Parameters.Add(new SqlParameter("@Large", SqlDbType.NVarChar));
                    scom.Parameters.Add(new SqlParameter("@Version", SqlDbType.Int, 4));
                    scom.Parameters.Add(new SqlParameter("@UpdatedOn", SqlDbType.DateTime));

                    scom.Parameters["@LayoutID"].Value = ld.LayoutID;
                    scom.Parameters["@Description"].Value = CommonLogic.IsNull<object>(ld.Description, DBNull.Value);
					scom.Parameters["@HTML"].Value = CommonLogic.IsNull<object>(ld.HTML, DBNull.Value);
					scom.Parameters["@Micro"].Value = CommonLogic.IsNull<object>(ld.Micro, DBNull.Value);
					scom.Parameters["@Icon"].Value = CommonLogic.IsNull<object>(ld.Micro, DBNull.Value);
					scom.Parameters["@Medium"].Value = CommonLogic.IsNull<object>(ld.Micro, DBNull.Value);
					scom.Parameters["@Large"].Value = CommonLogic.IsNull<object>(ld.Large, DBNull.Value);
					scom.Parameters["@Version"].Value = CommonLogic.IsNull<object>(ld.Version, DBNull.Value);
					scom.Parameters["@UpdatedOn"].Value = CommonLogic.IsNull<object>(ld.UpdatedOn, DBNull.Value);

                    try
                    {
                        scom.ExecuteNonQuery();
                    }
                    catch
                    {
                        success = false;
                    }

                    scom.Dispose();
                }

                conn.Close();
                conn.Dispose();
            }

            return success;
        }

        /// <summary>
        /// Retrieves all layouts from the database
        /// </summary>
        /// <returns>A List of LayoutData representing the existing layouts</returns>
        public static List<LayoutData> GetLayouts()
        {
            List<LayoutData> layouts = new List<LayoutData>();
            
            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                using (IDataReader rs = DB.GetRS("SELECT * FROM dbo.Layout WITH (NOLOCK)", conn))
                {
                    while (rs.Read())
                    {
                        LayoutData ld = new LayoutData();

                        ld.LayoutID = DB.RSFieldInt(rs, "LayoutID");
                        ld.LayoutGUID = DB.RSFieldGUID2(rs, "LayoutGUID");
                        ld.Name = DB.RSField(rs, "Name");
                        ld.Description = DB.RSField(rs, "Description");
                        ld.HTML = DB.RSField(rs, "HTML");
                        ld.Micro = DB.RSField(rs, "Micro");
                        ld.Icon = DB.RSField(rs, "Icon");
                        ld.Medium = DB.RSField(rs, "Medium");
                        ld.Large = DB.RSField(rs, "Large");
                        //ld.ControlMarkup = DB.RSField(rs, "ControlMarkup");
                        ld.Version = DB.RSFieldInt(rs, "Version");
                        ld.CreatedOn = DB.RSFieldDateTime(rs, "CreatedOn");
                        ld.UpdatedOn = DB.RSFieldDateTime(rs, "UpdatedOn");

                        ld.LayoutFields = LayoutField.GetFields(ld.LayoutID);

                        layouts.Add(ld);
                    }

                    rs.Close();
                    rs.Dispose();
                }
                conn.Close();
                conn.Dispose();
            }

            return layouts;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates or updates a layout control from the HTML of the layout
        /// </summary>
        /// <returns></returns>
        public bool CreateLayoutControl()
        {
            bool created = false;

            String layoutDir = CommonLogic.IIF(AppLogic.IsAdminSite, "../", String.Empty) + "layouts/" + this.LayoutID.ToString();
            String fName = this.Name + "_" + this.Version.ToString() + ".ascx";

            String layoutf = CommonLogic.SafeMapPath(Path.Combine(layoutDir, fName));

            if (!Directory.Exists(CommonLogic.SafeMapPath(layoutDir)))
            {
                Directory.CreateDirectory(CommonLogic.SafeMapPath(layoutDir));
            }

            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml(this.HTML);

            LayoutParser lp = new LayoutParser(xdoc);

            FileInfo fi = new FileInfo(layoutf);

            try
            {
                using (FileStream fs = fi.Create())
                {
                    byte[] fileContents = UTF8Encoding.UTF8.GetBytes(lp.Parse(this.LayoutID));
                    fs.Write(fileContents, 0, fileContents.Length);

                    fs.Close();
                    fs.Dispose();

                    created = true;
                }
            }
            catch
            {
                created = false;
            }

            return created;
        }

        /// <summary>
        /// Commits the Layout to the database as a new Layout
        /// </summary>
        public void Commit()
        {
            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                using (SqlCommand scom = new SqlCommand())
                {
                    scom.Connection = conn;
                    scom.CommandType = CommandType.StoredProcedure;
                    scom.CommandText = "dbo.aspdnsf_insLayout";

                    scom.Parameters.Add(new SqlParameter("@LayoutGUID", SqlDbType.UniqueIdentifier));
                    scom.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar));
                    scom.Parameters.Add(new SqlParameter("@Description", SqlDbType.NVarChar));
                    scom.Parameters.Add(new SqlParameter("@HTML", SqlDbType.NVarChar));
                    scom.Parameters.Add(new SqlParameter("@Micro", SqlDbType.NVarChar));
                    scom.Parameters.Add(new SqlParameter("@Icon", SqlDbType.NVarChar));
                    scom.Parameters.Add(new SqlParameter("@Medium", SqlDbType.NVarChar));
                    scom.Parameters.Add(new SqlParameter("@Large", SqlDbType.NVarChar));
                    scom.Parameters.Add(new SqlParameter("@Version", SqlDbType.Int, 4));
                    scom.Parameters.Add(new SqlParameter("@CreatedOn", SqlDbType.DateTime));
                    scom.Parameters.Add(new SqlParameter("@UpdatedOn", SqlDbType.DateTime));
                    scom.Parameters.Add(new SqlParameter("@LayoutID", SqlDbType.Int, 4)).Direction = ParameterDirection.Output;

                    if (this.LayoutGUID.Equals(new Guid()))
                    {
                        this.LayoutGUID = new Guid(DB.GetNewGUID());
                    }

                    scom.Parameters["@LayoutGUID"].Value = this.LayoutGUID;
                    scom.Parameters["@Name"].Value = this.Name;
                    scom.Parameters["@Description"].Value = this.Description;
                    scom.Parameters["@HTML"].Value = this.HTML;
                    scom.Parameters["@Micro"].Value = this.Micro;
                    scom.Parameters["@Icon"].Value = this.Icon;
                    scom.Parameters["@Medium"].Value = this.Medium;
                    scom.Parameters["@Large"].Value = this.Large;
                    scom.Parameters["@Version"].Value = this.Version;
                    scom.Parameters["@CreatedOn"].Value = this.CreatedOn;
                    scom.Parameters["@UpdatedOn"].Value = this.UpdatedOn;


                    try
                    {
                        scom.ExecuteNonQuery();
                        this.LayoutID = Int32.Parse(scom.Parameters["@LayoutID"].Value.ToString());

                        this.CreateLayoutControl();
                    }
                    catch { }

                    scom.Dispose();
                }
                conn.Close();
                conn.Dispose();
            }
        }

        /// <summary>
        /// Updates the Layout
        /// </summary>
        public void Update()
        {
            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                using (SqlCommand scom = new SqlCommand())
                {
                    scom.CommandType = CommandType.StoredProcedure;
                    scom.Connection = conn;
                    scom.CommandText = "dbo.aspdnsf_updLayout";

                    scom.Parameters.Add(new SqlParameter("@LayoutID", SqlDbType.Int, 4));
                    scom.Parameters.Add(new SqlParameter("@Description", SqlDbType.NVarChar));
                    scom.Parameters.Add(new SqlParameter("@HTML", SqlDbType.NVarChar));
                    scom.Parameters.Add(new SqlParameter("@Micro", SqlDbType.NVarChar));
                    scom.Parameters.Add(new SqlParameter("@Icon", SqlDbType.NVarChar));
                    scom.Parameters.Add(new SqlParameter("@Medium", SqlDbType.NVarChar));
                    scom.Parameters.Add(new SqlParameter("@Large", SqlDbType.NVarChar));
                    scom.Parameters.Add(new SqlParameter("@Version", SqlDbType.Int, 4));
                    scom.Parameters.Add(new SqlParameter("@UpdatedOn", SqlDbType.DateTime));

                    scom.Parameters["@LayoutID"].Value = this.LayoutID;
					scom.Parameters["@Description"].Value = CommonLogic.IsNull<object>(this.Description, DBNull.Value);
					scom.Parameters["@HTML"].Value = CommonLogic.IsNull<object>(this.HTML, DBNull.Value);
					scom.Parameters["@Micro"].Value = CommonLogic.IsNull<object>(this.Micro, DBNull.Value);
					scom.Parameters["@Icon"].Value = CommonLogic.IsNull<object>(this.Icon, DBNull.Value);
					scom.Parameters["@Medium"].Value = CommonLogic.IsNull<object>(this.Medium, DBNull.Value);
					scom.Parameters["@Large"].Value = CommonLogic.IsNull<object>(this.Large, DBNull.Value);
					scom.Parameters["@Version"].Value = CommonLogic.IsNull<object>(this.Version, DBNull.Value);
					scom.Parameters["@UpdatedOn"].Value = CommonLogic.IsNull<object>(this.UpdatedOn, DateTime.Now);

                    try
                    {
                        scom.ExecuteNonQuery();

                        this.CreateLayoutControl();
                    }
                    catch { }

                    scom.Dispose();
                }

                conn.Close();
                conn.Dispose();
            }
        }

        public void Reload()
        {
            if (m_layoutid > 0)
            {
                using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
                {
                    conn.Open();

                    using (IDataReader rs = DB.GetRS("select * from dbo.Layout with(NOLOCK) where LayoutID=" + m_layoutid.ToString(), conn))
                    {
                        if (rs.Read())
                        {
                            m_layoutid = DB.RSFieldInt(rs, "LayoutID");
                            m_layoutguid = DB.RSFieldGUID2(rs, "LayoutGUID");
                            m_name = DB.RSField(rs, "Name");
                            m_description = DB.RSField(rs, "Description");
                            m_html = DB.RSField(rs, "HTML");
                            m_micro = DB.RSField(rs, "Micro");
                            m_icon = DB.RSField(rs, "Icon");
                            m_medium = DB.RSField(rs, "Medium");
                            m_large = DB.RSField(rs, "Large");
                            m_version = DB.RSFieldInt(rs, "Version");
                            m_createdon = DB.RSFieldDateTime(rs, "CreatedOn");
                            m_updatedon = DB.RSFieldDateTime(rs, "UpdatedOn");
                        }

                        rs.Close();
                        rs.Dispose();
                    }
                    conn.Close();
                }

                m_layoutfields = LayoutField.GetFields(m_layoutid);
            }
        }

        /// <summary>
        /// Removes the Layout
        /// </summary>
        public void Remove()
        {
            try
            {
                DB.ExecuteSQL("delete dbo.layout where layoutid = " + LayoutID.ToString());
                DB.ExecuteSQL("delete dbo.layoutfield where layoutid = " + LayoutID.ToString());
                DB.ExecuteSQL("delete dbo.layoutfieldattribute where layoutid=" + LayoutID.ToString());
                DB.ExecuteSQL("delete dbo.layoutmap where layoutid=" + LayoutID.ToString());
            }
            catch { }

            //Directory.Delete(CommonLogic.SafeMapPath("~/layouts/" + LayoutID.ToString()), true);
            //Directory.Delete(CommonLogic.SafeMapPath("~/images
        }

        /// <summary>
        /// Clones the Layout
        /// </summary>
        public void Clone()
        {
            String newLayoutGUID = DB.GetNewGUID();

            //DB.ExecuteSQL(String.Format("insert dbo.Layout (LayoutGUID,Name,Description,Thumb,Large,HTML) select {0},{1},{2},{3},{4},{5} {6}",
            //    DB.SQuote(newLayoutGUID),
            //    DB.SQuote(Name + "(CLONED)"),
            //    DB.SQuote(Description),
            //    DB.SQuote(Thumb),
            //    DB.SQuote(Large),
            //    DB.SQuote(HTML),
            //    " from dbo.Layout where LayoutID=" + LayoutID.ToString()));

            //int ClonedLayoutID = DB.GetSqlN("select LayoutID as N from dbo.Layout with(NOLOCK) where LayoutGUID=" + DB.SQuote(newLayoutGUID));

            //DB.ExecuteSQL(String.Format("insert dbo.LayoutField (LayoutID, FieldType, FieldID) select {0},{1},{2} {3}",
            //    ClonedLayoutID.ToString(),
            //    "FieldType",
            //    "FieldID",
            //    " from dbo.LayoutField where LayoutID=" + LayoutID.ToString()));

        }

        #endregion
    }

    public class LayoutField
    {
        #region Private Variables

        private int m_layoutfieldid;
        private Guid m_layoutfieldguid;
        private int m_layoutid;
        private LayoutFieldEnum m_fieldtype;
        private string m_fieldid;
        private List<LayoutFieldAttribute> m_layoutfieldattributes;

        #endregion

        #region Constructors

        public LayoutField() 
        {
            m_layoutfieldid = 0;
            m_layoutfieldguid = new Guid();
            m_layoutid = 0;
            m_fieldtype = LayoutFieldEnum.Unknown;
            m_fieldid = String.Empty;
            m_layoutfieldattributes = new List<LayoutFieldAttribute>();
        }

        public LayoutField(int lfid)
        {
            if (lfid > 0)
            {
                using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
                {
                    conn.Open();

                    using (IDataReader rs = DB.GetRS("select * from dbo.LayoutField with(NOLOCK) where LayoutFieldID=" + lfid.ToString(), conn))
                    {
                        if (rs.Read())
                        {
                            m_layoutfieldid = DB.RSFieldInt(rs, "LayoutFieldID");
                            m_layoutfieldguid = DB.RSFieldGUID2(rs, "LayoutFieldGUID");
                            m_layoutid = DB.RSFieldInt(rs, "LayoutID");
                            m_fieldtype = (LayoutFieldEnum)DB.RSFieldInt(rs, "FieldType");
                            m_fieldid = DB.RSField(rs, "FieldID");
                        }

                        rs.Close();
                        rs.Dispose();

                    }

                    conn.Close();
                    conn.Dispose();
                }

                m_layoutfieldattributes = LayoutFieldAttribute.GetAttributes(lfid);
            }
        }

        #endregion

        #region Static Methods

        public static List<LayoutField> GetFields(int lID)
        {
            List<LayoutField> llf = new List<LayoutField>();

            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                using (IDataReader rs = DB.GetRS("select * from dbo.LayoutField with(NOLOCK) where LayoutID=" + lID.ToString(), conn))
                {
                    while (rs.Read())
                    {
                        LayoutField lf = new LayoutField();

                        lf.LayoutFieldID = DB.RSFieldInt(rs, "LayoutFieldID");
                        lf.LayoutFieldGUID = DB.RSFieldGUID2(rs, "LayoutFieldGUID");
                        lf.LayoutID = lID;
                        lf.FieldType = (LayoutFieldEnum)DB.RSFieldInt(rs, "FieldType");
                        lf.FieldID = DB.RSField(rs, "FieldID");
                        lf.LayoutFieldAttributes = LayoutFieldAttribute.GetAttributes(lf.LayoutFieldID);

                        llf.Add(lf);
                    }

                    rs.Close();
                    rs.Dispose();
                }

                conn.Close();
                conn.Dispose();
            }

            return llf;
        }

        #endregion

        #region Public Properties

        public int LayoutFieldID
        {
            get { return m_layoutfieldid; }
            set { m_layoutfieldid = value; }
        }

        public Guid LayoutFieldGUID
        {
            get { return m_layoutfieldguid; }
            set { m_layoutfieldguid = value; }
        }

        public int LayoutID
        {
            get { return m_layoutid; }
            set { m_layoutid = value; }
        }

        public LayoutFieldEnum FieldType
        {
            get { return m_fieldtype; }
            set { m_fieldtype = value; }
        }

        public String FieldID
        {
            get { return m_fieldid; }
            set { m_fieldid = value; }
        }

        public List<LayoutFieldAttribute> LayoutFieldAttributes
        {
            get { return m_layoutfieldattributes; }
            set { m_layoutfieldattributes = value; }
        }

        #endregion

        #region Public Methods

        public void Reload()
        {
            if (m_layoutfieldid > 0)
            {
                using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
                {
                    conn.Open();

                    using (IDataReader rs = DB.GetRS("select * from dbo.LayoutField with(NOLOCK) where LayoutFieldID=" + m_layoutfieldid.ToString(), conn))
                    {
                        if (rs.Read())
                        {
                            m_layoutfieldid = DB.RSFieldInt(rs, "LayoutFieldID");
                            m_layoutfieldguid = DB.RSFieldGUID2(rs, "LayoutFieldGUID");
                            m_layoutid = DB.RSFieldInt(rs, "LayoutID");
                            m_fieldtype = (LayoutFieldEnum)DB.RSFieldInt(rs, "FieldType");
                            m_fieldid = DB.RSField(rs, "FieldID");
                        }
                        rs.Close();
                        rs.Dispose();
                    }

                    conn.Close();
                    conn.Dispose();
                }

                m_layoutfieldattributes = LayoutFieldAttribute.GetAttributes(m_layoutfieldid);
            }
        }

        #endregion
    }

    public class LayoutFieldAttribute
    {
        #region Private Variables

        private int m_layoutfieldattributeid;
        private Guid m_layoutfieldattributeguid;
        private int m_layoutid;
        private int m_layoutfieldid;
        private String m_name;
        private String m_value;

        #endregion

        #region Public Properties

        public int LayoutFieldAttributeID
        {
            get { return m_layoutfieldattributeid; }
            set { m_layoutfieldattributeid = value; }
        }

        public Guid LayoutFieldAttributeGUID
        {
            get { return m_layoutfieldattributeguid; }
            set { m_layoutfieldattributeguid = value; }
        }

        public int LayoutID
        {
            get { return m_layoutid; }
            set { m_layoutid = value; }
        }

        public int LayoutFieldID
        {
            get { return m_layoutfieldid; }
            set { m_layoutfieldid = value; }
        }

        public String Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        public String Value
        {
            get { return m_value; }
            set { m_value = value; }
        }

        #endregion

        #region Constructors

        public LayoutFieldAttribute()
        {
            m_layoutfieldattributeid = 0;
            m_layoutfieldattributeguid = new Guid(DB.GetNewGUID());
            m_layoutfieldid = 0;
            m_name = String.Empty;
            m_value = String.Empty;
        }

        public LayoutFieldAttribute(int lfaID)
        {
            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                using (IDataReader rs = DB.GetRS("select * from dbo.LayoutFieldAttribute with(NOLOCK) where LayoutFieldAttributeID=" + lfaID.ToString(), conn))
                {
                    if (rs.Read())
                    {
                        m_layoutfieldattributeid = DB.RSFieldInt(rs, "LayoutFieldAttributeID");
                        m_layoutfieldattributeguid = DB.RSFieldGUID2(rs, "LayoutFieldAttributeGUID");
                        m_layoutfieldid = DB.RSFieldInt(rs, "LayoutFieldID");
                        m_name = DB.RSField(rs, "Name");
                        m_value = DB.RSField(rs, "Value");
                    }

                    rs.Close();
                    rs.Dispose();
                }

                conn.Close();
                conn.Dispose();
            }
        }

        public LayoutFieldAttribute(int lfID, String attr)
        {
            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                using (IDataReader rs = DB.GetRS("select * from dbo.LayoutFieldAttribute with(NOLOCK) where LayoutFieldID=" + lfID.ToString() + " and Name=" + DB.SQuote(attr), conn))
                {

                    if (rs.Read())
                    {
                        m_layoutfieldattributeid = DB.RSFieldInt(rs, "LayoutFieldAttributeID");
                        m_layoutfieldattributeguid = DB.RSFieldGUID2(rs, "LayoutFieldAttributeGUID");
                        m_layoutfieldid = DB.RSFieldInt(rs, "LayoutFieldID");
                        m_name = DB.RSField(rs, "Name");
                        m_value = DB.RSField(rs, "Value");
                    }

                    rs.Close();
                    rs.Dispose();
                }

                conn.Close();
                conn.Dispose();
            }
        }

        #endregion

        #region Static Methods

        public static List<LayoutFieldAttribute> GetAttributes(int LayoutFieldID)
        {
            List<LayoutFieldAttribute> llfa = new List<LayoutFieldAttribute>();

            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                using (IDataReader rs = DB.GetRS("select * from dbo.LayoutFieldAttribute with(NOLOCK) where LayoutFieldID=" + LayoutFieldID.ToString(), conn))
                {
                    while (rs.Read())
                    {
                        LayoutFieldAttribute lfa = new LayoutFieldAttribute();

                        lfa.LayoutFieldAttributeID = DB.RSFieldInt(rs, "LayoutFieldAttributeID");
                        lfa.LayoutFieldAttributeGUID = DB.RSFieldGUID2(rs, "LayoutFieldAttributeGUID");
                        lfa.LayoutID = DB.RSFieldInt(rs, "LayoutID");
                        lfa.LayoutFieldID = DB.RSFieldInt(rs, "LayoutFieldID");
                        lfa.Name = DB.RSField(rs, "Name");
                        lfa.Value = DB.RSField(rs, "Value");

                        llfa.Add(lfa);
                    }

                    rs.Close();
                    rs.Dispose();
                }

                conn.Close();
                conn.Dispose();
            }

            return llfa;
        }

        #endregion

        #region Public Methods

        public void Update()
        {
            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                using (SqlCommand scom = new SqlCommand())
                {
                    scom.CommandType = CommandType.StoredProcedure;
                    scom.Connection = conn;
                    scom.CommandText = "dbo.aspdnsf_updLayoutFieldAttribute";

                    scom.Parameters.Add(new SqlParameter("@LayoutID", SqlDbType.Int,4));
                    scom.Parameters.Add(new SqlParameter("@LayoutFieldID", SqlDbType.Int,4));
                    //scom.Parameters.Add(new SqlParameter("@ControlMarkup", SqlDbType.NVarChar));
                    scom.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar));
                    scom.Parameters.Add(new SqlParameter("@Value", SqlDbType.NVarChar));

					scom.Parameters["@LayoutID"].Value = CommonLogic.IsNull<object>(this.LayoutID, DBNull.Value);
					scom.Parameters["@LayoutFieldID"].Value = CommonLogic.IsNull<object>(this.LayoutFieldID, DBNull.Value);
					scom.Parameters["@Name"].Value = CommonLogic.IsNull<object>(this.Name, DBNull.Value);
					scom.Parameters["@Value"].Value = CommonLogic.IsNull<object>(this.Value, DateTime.Now);

                    try
                    {
                        scom.ExecuteNonQuery();
                    }
                    catch { }

                    scom.Dispose();
                }

                conn.Close();
                conn.Dispose();
            }
        }
        #endregion
    }

    public class LayoutMap
    {
        #region Private Variables

        private int m_layoutmapid;
        private Guid m_layoutmapguid;
        private int m_layoutid;
        private int m_pagetypeid;
        private int m_pageid;
        private String m_pagetypename;
        private DateTime m_createdon;

        #endregion

        #region Constructors

        public LayoutMap() 
        { 
            m_layoutmapid = 0;
            m_layoutmapguid = new Guid();
            m_layoutid = 0;
            m_pagetypeid = 0;
            m_pageid = 0;
            m_pagetypename = String.Empty;
            m_createdon = DateTime.MinValue;
        }

        public LayoutMap(int LayoutMapID)
        {
            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                using (IDataReader rs = DB.GetRS("select lm.*, pt.PageTypeName from dbo.LayoutMap lm with(NOLOCK) left join dbo.PageType pt with(NOLOCK) on lm.PageTypeID = pt.PageTypeID where lm.LayoutMapID=" + LayoutMapID.ToString(), conn))
                {
                    if (rs.Read())
                    {
                        m_layoutmapid = DB.RSFieldInt(rs, "LayoutMapID");
                        m_layoutmapguid = DB.RSFieldGUID2(rs, "LayoutMapGUID");
                        m_layoutid = DB.RSFieldInt(rs, "LayoutID");
                        m_pagetypeid = DB.RSFieldInt(rs, "PageTypeID");
                        m_pageid = DB.RSFieldInt(rs, "PageID");
                        m_pagetypename = DB.RSField(rs, "PageTypeName");
                        m_createdon = DB.RSFieldDateTime(rs, "CreatedOn");
                    }

                    rs.Close();
                    rs.Dispose();
                }

                conn.Close();
                conn.Dispose();
            }
        }

        public LayoutMap(String pName, int eoID)
        {
            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                using (IDataReader rs = DB.GetRS("select lm.*, pt.PageTypeName from dbo.LayoutMap lm with(NOLOCK) left join dbo.PageType pt with(NOLOCK) on lm.PageTypeID = pt.PageTypeID where pt.PageTypeName=" + DB.SQuote(pName) + " and lm.PageID=" + eoID.ToString(), conn))
                {
                    if (rs.Read())
                    {
                        m_layoutmapid = DB.RSFieldInt(rs, "LayoutMapID");
                        m_layoutmapguid = DB.RSFieldGUID2(rs, "LayoutMapGUID");
                        m_layoutid = DB.RSFieldInt(rs, "LayoutID");
                        m_pagetypeid = DB.RSFieldInt(rs, "PageTypeID");
                        m_pageid = DB.RSFieldInt(rs, "PageID");
                        m_pagetypename = DB.RSField(rs, "PageTypeName");
                        m_createdon = DB.RSFieldDateTime(rs, "CreatedOn");
                    }

                    rs.Close();
                    rs.Dispose();
                }

                conn.Close();
                conn.Dispose();
            }
        }

        #endregion

        #region Public Properties

        public int LayoutMapID
        {
            get { return m_layoutmapid; }
            set { m_layoutmapid = value; }
        }

        public Guid LayoutMapGUID
        {
            get { return m_layoutmapguid; }
            set { m_layoutmapguid = value; }
        }

        public int LayoutID
        {
            get { return m_layoutid;  }
            set { m_layoutid = value; }
        }

        public int PageTypeID
        {
            get { return m_pagetypeid; }
            set { m_pagetypeid = value; }
        }

        public int PageID
        {
            get { return m_pageid; }
            set { m_pageid = value; }
        }

        public DateTime CreatedOn
        {
            get { return m_createdon; }
            set { m_createdon = value; }
        }

        public String PageTypeName
        {
            get { return m_pagetypename; }
            set { m_pagetypename = value; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Commits the LayoutMap to the database
        /// </summary>
        public void Commit()
        {
            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                using (SqlCommand scom = new SqlCommand())
                {
                    scom.Connection = conn;
                    scom.CommandType = CommandType.StoredProcedure;
                    scom.CommandText = "dbo.aspdnsf_insLayoutMap";

                    scom.Parameters.Add(new SqlParameter("@layoutid", SqlDbType.Int, 4));
                    scom.Parameters.Add(new SqlParameter("@pageid", SqlDbType.Int, 4));
                    scom.Parameters.Add(new SqlParameter("@pagetypename", SqlDbType.NVarChar, 100));

                    scom.Parameters["@layoutid"].Value = this.LayoutID;
                    scom.Parameters["@PageID"].Value = this.PageID;
                    scom.Parameters["@pagetypename"].Value = this.PageTypeName;


                    try
                    {
                        scom.ExecuteNonQuery();
                    }
                    catch { }

                    scom.Dispose();
                }
                conn.Close();
                conn.Dispose();
            }
        }

        public void Delete()
        {
            if (m_layoutmapid > 0)
            {
                DB.ExecuteSQL("delete dbo.LayoutMap where LayoutMapID=" + m_layoutmapid.ToString());
            }
        }

        #endregion

        #region Static Methods

        public static List<LayoutMap> GetMappings(int LayoutID)
        {
            List<LayoutMap> llm = new List<LayoutMap>();

            // TBD

            return llm;
        }
        #endregion
    }

    public class LayoutFieldInstance
    {

    }
}
